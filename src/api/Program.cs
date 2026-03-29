using ChessMonitor.Shared;
using ChessMonitor.Shared.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<ConnectionStringsOptions>()
    .BindConfiguration(ConnectionStringsOptions.SectionName)
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Default),
        $"{ConnectionStringsOptions.SectionName}:Default must be configured.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ChessComOptions>()
    .BindConfiguration(ChessComOptions.SectionName)
    .ValidateOnStart();

builder.Services
    .AddOptions<StockfishOptions>()
    .BindConfiguration(StockfishOptions.SectionName)
    .Validate(
        options => options.Threads is >= 1 and <= 128,
        $"{StockfishOptions.SectionName}:Threads must be between 1 and 128.")
    .Validate(
        options => options.Depth is >= 1 and <= 50,
        $"{StockfishOptions.SectionName}:Depth must be between 1 and 50.")
    .ValidateOnStart();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
var connectionStrings = app.Services.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
var chessCom = app.Services.GetRequiredService<IOptions<ChessComOptions>>().Value;
var stockfish = app.Services.GetRequiredService<IOptions<StockfishOptions>>().Value;

logger.LogInformation(
    "API configuration loaded. DatabaseConfigured={DatabaseConfigured}, ChessComUsernameConfigured={ChessComUsernameConfigured}, StockfishThreads={StockfishThreads}, StockfishDepth={StockfishDepth}",
    !string.IsNullOrWhiteSpace(connectionStrings.Default),
    !string.IsNullOrWhiteSpace(chessCom.Username),
    stockfish.Threads,
    stockfish.Depth);

static ServiceStatusResponse CreateStatus(string serviceName, IHostEnvironment environment) =>
    new(serviceName, true, DateTimeOffset.UtcNow, environment.EnvironmentName);

app.MapGet("/", (IHostEnvironment environment) => Results.Ok(CreateStatus("api", environment)));

app.MapGet("/health", (IHostEnvironment environment) => Results.Ok(CreateStatus("api", environment)));

app.Run();
