using Microsoft.Extensions.Options;
using MnkeyFog.Bsky.WorkerService;
using MnkeyFog.BskyService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<BskyBotOptions>(builder.Configuration.GetSection("Bsky"));
builder.Services.AddSingleton<BskyBotOptions>(
    sp => sp.GetRequiredService<IOptions<BskyBotOptions>>().Value);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
