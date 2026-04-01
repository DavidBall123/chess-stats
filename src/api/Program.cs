using ChessMonitor.Api.Data;
using ChessMonitor.Shared;
using ChessMonitor.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddDbContext<ChessMonitorDbContext>((sp, options) =>
{
    var connectionString = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value.Default;
    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<IChessMonitorRepository, EntityFrameworkChessMonitorRepository>();
builder.Services.AddScoped<IDatabaseInitializer, EntityFrameworkDatabaseInitializer>();
builder.Services.AddScoped<ISampleDataSeeder, SampleDataSeeder>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    var seeder = scope.ServiceProvider.GetRequiredService<ISampleDataSeeder>();
    await databaseInitializer.InitializeAsync(app.Lifetime.ApplicationStopping);
    await seeder.SeedAsync(app.Lifetime.ApplicationStopping);
}
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

app.MapGet("/api/dashboard/overview", async (IChessMonitorRepository repository, CancellationToken cancellationToken) =>
    Results.Ok(await repository.GetDashboardOverviewAsync(cancellationToken)));

app.MapGet("/api/dashboard/filters", async (IChessMonitorRepository repository, CancellationToken cancellationToken) =>
    Results.Ok(await repository.GetDashboardFiltersAsync(cancellationToken)));

app.Run();
