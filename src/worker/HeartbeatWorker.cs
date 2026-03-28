using ChessMonitor.Shared;

namespace ChessMonitor.Worker;

public sealed class HeartbeatWorker(ILogger<HeartbeatWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = CreateStatus();
        logger.LogInformation(
            "Worker heartbeat started for {Service} in {Environment} at {UtcTime}",
            started.Service,
            started.Environment,
            started.Utc);

        while (!stoppingToken.IsCancellationRequested)
        {
            var heartbeat = CreateStatus();
            logger.LogInformation(
                "Worker heartbeat for {Service} in {Environment} at {UtcTime}",
                heartbeat.Service,
                heartbeat.Environment,
                heartbeat.Utc);
            await Task.Delay(Interval, stoppingToken);
        }

        var stopped = CreateStatus();
        logger.LogInformation(
            "Worker heartbeat stopping for {Service} in {Environment} at {UtcTime}",
            stopped.Service,
            stopped.Environment,
            stopped.Utc);
    }

    private static ServiceStatusResponse CreateStatus() =>
        new("worker", true, DateTimeOffset.UtcNow, Environments.Production);
}
