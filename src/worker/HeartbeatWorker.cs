namespace ChessMonitor.Worker;

public sealed class HeartbeatWorker(ILogger<HeartbeatWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker heartbeat started at {UtcTime}", DateTimeOffset.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker heartbeat at {UtcTime}", DateTimeOffset.UtcNow);
            await Task.Delay(Interval, stoppingToken);
        }

        logger.LogInformation("Worker heartbeat stopping at {UtcTime}", DateTimeOffset.UtcNow);
    }
}
