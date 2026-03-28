using ChessMonitor.Shared;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

static ServiceStatusResponse CreateStatus(string serviceName, IHostEnvironment environment) =>
    new(serviceName, true, DateTimeOffset.UtcNow, environment.EnvironmentName);

app.MapGet("/", (IHostEnvironment environment) => Results.Ok(CreateStatus("api", environment)));

app.MapGet("/health", (IHostEnvironment environment) => Results.Ok(CreateStatus("api", environment)));

app.Run();
