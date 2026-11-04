using idunno.AtProto;
using idunno.Bluesky;
using idunno.Bluesky.Chat;
using idunno.Bluesky.Notifications;
using Microsoft.Extensions.Logging;

namespace MnkeyFog.BskyService;

/// <summary>
/// Abstract base service for a Bluesky bot: authenticates and polls the server
/// for @mentions (via <c>app.bsky.notification.listNotifications</c> filtered
/// client-side to reason=Mention — server-side returns only notifications for
/// this account) and DM conversation logs (via
/// <c>app.bsky.chat.messages.getConversationLog</c>), dispatching both to
/// virtual handlers.
/// </summary>
public abstract class BskyBotService : IAsyncDisposable, IPlayerNotifier {
    private const int NotificationPageLimit = 50;
    private static readonly Uri DefaultService = new("https://bsky.social");

    protected ILogger Logger { get; }
    protected BlueskyAgent Agent { get; private set; } = null!;
    protected GameStore GameStore { get; private set; } = new();

    private CancellationTokenSource? _pollingCancellationSource;
    private Task? _pollingTask;
    // Cursors are opaque pagination tokens returned by the API, not indices. A
    // null cursor means "start from the beginning", so passing one before the
    // first page is fetched is valid (not a use-before-population bug).
    private string? _directMessageCursor;
    private string? _mentionCursor;
    private Did _botDid = null!;
    private readonly Dictionary<string, string> _conversationCache = new(StringComparer.OrdinalIgnoreCase);

    protected BskyBotService(ILogger logger) {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
    }

    #region lifecycle
    /// <summary>
    /// Login and start polling for mentions (notification list, server-side
    /// filtered to this account) and DMs (conversation log).
    /// </summary>
    public virtual async Task StartAsync(BskyBotOptions options, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(options);

        if (options.GamePersistenceDirectory is not null) {
            GameStore = new GameStore(options.GamePersistenceDirectory);
        }

        if (options.Service is not null && options.Service != DefaultService) {
            throw new NotSupportedException("Custom PDS services are not supported by this bot yet.");
        }

        Agent = BlueskyAgentBuilder.Create()
            .Build();

        var login = await Agent.Login(options.Handle, options.AppPassword, cancellationToken: cancellationToken);
        if (!login.Succeeded) {
            throw new InvalidOperationException($"Bluesky login failed for {options.Handle}.");
        }

        _botDid = Agent.Did!;

        _pollingCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _pollingTask = Task.Run(() => RunPollersAsync(
            options.MentionPollInterval ?? options.DirectMessagePollInterval ?? TimeSpan.FromSeconds(10),
            options.DirectMessagePollInterval ?? TimeSpan.FromSeconds(10),
            _pollingCancellationSource.Token
        ));

        Logger.LogInformation("Bot {handle} started.", options.Handle);
    }

    /// <summary>Stop polling and disconnect.</summary>
    public virtual async Task StopAsync() {
        _pollingCancellationSource?.Cancel();
        if (_pollingTask is not null) {
            try {
                await _pollingTask;
            } catch (OperationCanceledException) {
                // expected on stop
            }
        }
    }

    public async ValueTask DisposeAsync() {
        await StopAsync();
        GC.SuppressFinalize(this);
    }
    #endregion

    #region event handlers for subclasses
    /// <summary>Called for every direct message sent to the bot.</summary>
    protected internal abstract Task OnDirectMessageAsync(string conversationId, string senderDid, string message, CancellationToken cancellationToken);

    /// <summary>Called for every public post that @mentions the bot.</summary>
    protected internal abstract Task OnMentionAsync(AtUri postUri, string authorDid, string message, CancellationToken cancellationToken);
    #endregion

    #region outbound helpers
    /// <summary>Send a DM in the given conversation.</summary>
    protected async Task SendDirectMessageAsync(string conversationId, string text, CancellationToken cancellationToken = default) {
        var result = await Agent.SendMessage(conversationId, text, extractFacets: false, cancellationToken: cancellationToken);
        if (!result.Succeeded) {
            Logger.LogWarning("Failed to send DM to conversation {convoId}: {error}", conversationId, result.StatusCode);
        }
    }

    /// <summary>Post a public message.</summary>
    protected async Task PostAsync(string text, CancellationToken cancellationToken = default) {
        var result = await Agent.Post(text, cancellationToken: cancellationToken);
        if (!result.Succeeded) {
            Logger.LogWarning("Failed to post: {error}", result.StatusCode);
        }
    }

    /// <inheritdoc/>
    public virtual async Task SendDmToPlayerAsync(string playerDid, string text, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(playerDid);
        ArgumentNullException.ThrowIfNull(text);

        if (!_conversationCache.TryGetValue(playerDid, out var conversationId)) {
            var result = await Agent.GetConversationForMembers([playerDid], cancellationToken: cancellationToken);
            if (!result.Succeeded || result.Result is null) {
                Logger.LogWarning("No DM conversation exists for {playerDid}; dropping message.", playerDid);
                return;
            }

            conversationId = (string)result.Result.Id;
            _conversationCache[playerDid] = conversationId;
        }

        await SendDirectMessageAsync(conversationId, text, cancellationToken);
    }
    #endregion

    #region internals
    private async Task RunPollersAsync(
        TimeSpan mentionInterval,
        TimeSpan dmInterval,
        CancellationToken cancellationToken
    ) {
        // Run both pollers concurrently; the loop stays alive until cancellation.
        var mentionPoller = PollMentionsAsync(mentionInterval, cancellationToken);
        var dmPoller = PollDirectMessagesAsync(dmInterval, cancellationToken);
        await Task.WhenAll(mentionPoller, dmPoller);
    }

    private static async Task WaitForIntervalAsync(TimeSpan interval, CancellationToken cancellationToken) {
        try {
            await Task.Delay(interval, cancellationToken);
        } catch (OperationCanceledException) {
            // expected on stop
        }
    }

    private async Task PollMentionsAsync(TimeSpan interval, CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            try {
                var result = await Agent.ListNotifications(
                    limit: NotificationPageLimit,
                    cursor: _mentionCursor,
                    cancellationToken: cancellationToken
                );
                if (result.Succeeded && result.Result is not null) {
                    var notifications = result.Result;
                    for (var i = 0; i < notifications.Count; i++) {
                        var n = notifications[i];
                        if (n.Reason != NotificationReason.Mention) {
                            continue;
                        }

                        var authorDid = n.Author?.Did is null ? null : (string)n.Author.Did;
                        if (authorDid is null || authorDid == (string)_botDid) {
                            continue;
                        }

                        var postUri = n.ReasonSubject;
                        if (postUri is null) {
                            continue;
                        }
                        var text = n.Record?.ExtensionData is not null
                            && n.Record.ExtensionData.TryGetValue("text", out var textEl)
                            && textEl.ValueKind == System.Text.Json.JsonValueKind.String
                            ? textEl.GetString() ?? ""
                            : "";

                        await OnMentionAsync(postUri, authorDid, text, cancellationToken);
                    }

                    _mentionCursor = notifications.Cursor;
                }
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                Logger.LogWarning(ex, "Mention polling failed; will retry.");
            }

            await WaitForIntervalAsync(interval, cancellationToken);
        }
    }

    private async Task PollDirectMessagesAsync(TimeSpan interval, CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            try {
                var logs = await Agent.GetConversationLog(_directMessageCursor, cancellationToken);
                if (logs.Succeeded && logs.Result is not null) {
                    _directMessageCursor = logs.Result.Cursor;
                    for (var i = 0; i < logs.Result.Count; i++) {
                        var entry = logs.Result[i];
                        if (entry is MessageLogBase messageLog
                            && messageLog.Message is MessageView message
                            && (string)message.Sender.Did != (string)_botDid) {
                            await OnDirectMessageAsync(
                                messageLog.ConversationId,
                                (string)message.Sender.Did,
                                message.Text ?? "",
                                cancellationToken
                            );
                        }
                    }
                }
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                Logger.LogWarning(ex, "DM polling failed; will retry.");
            }

            await WaitForIntervalAsync(interval, cancellationToken);
        }
    }
    #endregion
}
