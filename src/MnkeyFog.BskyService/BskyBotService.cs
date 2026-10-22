using System.Text.Json;
using idunno.AtProto;
using idunno.AtProto.Jetstream;
using idunno.AtProto.Jetstream.Events;
using idunno.Bluesky;
using idunno.Bluesky.Chat;
using Microsoft.Extensions.Logging;

namespace MnkeyFog.BskyService;

/// <summary>
/// Abstract base service for a Bluesky bot: authenticates, connects a Jetstream
/// WebSocket for @mentions, polls DM conversation logs for direct messages, and
/// dispatches both to virtual handlers.
/// </summary>
public abstract class BskyBotService : IAsyncDisposable, IPlayerNotifier {
    private const string FeedPostCollection = "app.bsky.feed.post";
    private static readonly Uri DefaultService = new("https://bsky.social");
    private static readonly Uri DefaultJetstream = new("wss://jetstream1.us-west.bsky.network");

    protected ILogger Logger { get; }
    protected BlueskyAgent Agent { get; private set; } = null!;
    protected GameStore GameStore { get; private set; } = new();

    private AtProtoJetstream? _jetstream;
    private CancellationTokenSource? _pollingCts;
    private Task? _pollingTask;
    private string? _dmCursor;
    private Did _botDid = null!;
    private readonly Dictionary<string, string> _conversationCache = new(StringComparer.OrdinalIgnoreCase);

    protected BskyBotService(ILogger logger) {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
    }

    #region lifecycle
    /// <summary>
    /// Login and start listening for mentions (Jetstream) and DMs (conversation-log polling).
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

        var jetstreamUri = options.JetstreamUri ?? DefaultJetstream;
        _jetstream = AtProtoJetstreamBuilder.Create()
            .ConnectTo(jetstreamUri)
            .FilterTo([new Nsid(FeedPostCollection)])
            .FilterTo(new[] { _botDid })
            .Build();
        _jetstream.RecordReceived += OnJetstreamRecordReceived;
        _jetstream.FaultRaised += OnJetstreamFaultRaised;
        await _jetstream.ConnectAsync(cancellationToken);

        _pollingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _pollingTask = PollDirectMessagesAsync(
            options.DirectMessagePollInterval ?? TimeSpan.FromSeconds(10),
            _pollingCts.Token
        );

        Logger.LogInformation("Bot {handle} started.", options.Handle);
    }

    /// <summary>Stop listening and disconnect.</summary>
    public virtual async Task StopAsync() {
        _pollingCts?.Cancel();
        if (_pollingTask is not null) {
            try {
                await _pollingTask;
            } catch (OperationCanceledException) {
                // expected on stop
            }
        }

        if (_jetstream is not null) {
            if (_jetstream.IsConnected) {
                await _jetstream.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "bot shutting down", CancellationToken.None);
            }
            _jetstream.Dispose();
            _jetstream = null!;
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
    private void OnJetstreamRecordReceived(object? sender, RecordReceivedEventArgs e) {
        _ = Task.Run(async () => {
            try {
                if (e.ParsedEvent is not AtJetstreamCommitEvent commitEvent
                    || commitEvent.Commit.Collection != FeedPostCollection) {
                    return;
                }

                var post = commitEvent.Commit.Record?.RootElement;
                if (post is null) {
                    return;
                }

                // Only dispatch posts that mention the bot.
                if (!MentionsBot(post.Value, _botDid)) {
                    return;
                }

                if (post.Value.TryGetProperty("text", out var textElement)
                    && textElement.ValueKind == JsonValueKind.String) {
                    var postUri = new AtUri($"at://{commitEvent.Did}/{FeedPostCollection}/{commitEvent.Commit.RKey}");
                    await OnMentionAsync(
                        postUri,
                        commitEvent.Did,
                        textElement.GetString() ?? "",
                        CancellationToken.None
                    );
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error handling Jetstream record.");
            }
        });
    }

    private void OnJetstreamFaultRaised(object? sender, FaultRaisedEventArgs e) {
        Logger.LogWarning("Jetstream fault: {fault}", e.Fault);
    }

    private static bool MentionsBot(JsonElement post, Did botDid) {
        if (!post.TryGetProperty("facets", out var facets) || facets.ValueKind != JsonValueKind.Array) {
            return false;
        }

        foreach (var facet in facets.EnumerateArray()) {
            if (!facet.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array) {
                continue;
            }

            foreach (var feature in features.EnumerateArray()) {
                if (feature.ValueKind == JsonValueKind.Object
                    && feature.TryGetProperty("$type", out var type)
                    && type.GetString() == "app.bsky.richtext.facet#mention"
                    && feature.TryGetProperty("did", out var did)
                    && did.GetString() == (string)botDid) {
                    return true;
                }
            }
        }

        return false;
    }

    private async Task PollDirectMessagesAsync(TimeSpan interval, CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            try {
                var logs = await Agent.GetConversationLog(_dmCursor, cancellationToken);
                if (logs.Succeeded && logs.Result is not null) {
                    _dmCursor = logs.Result.Cursor;
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

            try {
                await Task.Delay(interval, cancellationToken);
            } catch (OperationCanceledException) {
                return;
            }
        }
    }
    #endregion
}
