using Microsoft.Extensions.Options;
using MnkeyFog.Bsky.WorkerService;
using MnkeyFog.Bsky;
using MnkeyFog.Text;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<BskyBotOptions>(builder.Configuration.GetSection("Bsky"));
builder.Services.AddSingleton<BskyBotOptions>(
    sp => sp.GetRequiredService<IOptions<BskyBotOptions>>().Value);
builder.Services.AddSingleton<ModelNamer>();
builder.Services.AddSingleton<ActionParser>();
builder.Services.AddSingleton<TextBoardRenderer>();
var persistDirectory = builder.Configuration["Bsky:GamePersistenceDirectory"];
builder.Services.AddSingleton<IGameSessionStore>(
    persistDirectory is null ? new NoOpGameSessionStore() : new FileSystemGameSessionStore(persistDirectory));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
