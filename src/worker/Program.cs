using ChessMonitor.Shared.Configuration;
using ChessMonitor.Worker;
using ChessMonitor.Worker.Ingestion;

var builder = Host.CreateApplicationBuilder(args);

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

builder.Services.AddSingleton<IArchiveFetcher, MockArchiveFetcher>();
builder.Services.AddSingleton<IGameParser, MockGameParser>();
builder.Services.AddSingleton<IPersistenceWriter, MockPersistenceWriter>();

builder.Services.AddHostedService<HeartbeatWorker>();

var app = builder.Build();
app.Run();
