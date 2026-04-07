using ChessMonitor.Shared;
using ChessMonitor.Shared.Configuration;
using ChessMonitor.Worker.Ingestion;
using Microsoft.Extensions.Options;

namespace ChessMonitor.Worker;

public sealed class HeartbeatWorker(
    ILogger<HeartbeatWorker> logger,
    IHostEnvironment environment,
    IOptions<ChessComOptions> chessComOptions,
    IOptions<StockfishOptions> stockfishOptions,
    IArchiveFetcher archiveFetcher,
    IGameParser gameParser,
    IPersistenceWriter persistenceWriter) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = CreateStatus();
        logger.LogInformation(
            "Worker heartbeat started for {Service} in {Environment} at {UtcTime}. ChessComUsernameConfigured={ChessComUsernameConfigured}, StockfishThreads={StockfishThreads}, StockfishDepth={StockfishDepth}",
            started.Service,
            started.Environment,
            started.Utc,
            !string.IsNullOrWhiteSpace(chessComOptions.Value.Username),
            stockfishOptions.Value.Threads,
            stockfishOptions.Value.Depth);

        while (!stoppingToken.IsCancellationRequested)
        {
            var heartbeat = CreateStatus();
            var archives = await archiveFetcher.FetchArchivesAsync(stoppingToken);

            var parsedGames = new List<ChessMonitor.Shared.Contracts.GameUpsertRequest>();
            foreach (var archive in archives)
            {
                var games = await gameParser.ParseGamesAsync(archive, stoppingToken);
                parsedGames.AddRange(games);
            }

            var persistenceResult = await persistenceWriter.WriteGamesAsync(parsedGames, stoppingToken);
            logger.LogInformation(
                "Worker heartbeat for {Service} in {Environment} at {UtcTime}. IngestionArchives={IngestionArchives}, ParsedGames={ParsedGames}, PersistedGames={PersistedGames}",
                heartbeat.Service,
                heartbeat.Environment,
                heartbeat.Utc,
                archives.Count,
                parsedGames.Count,
                persistenceResult.Persisted);
            await Task.Delay(Interval, stoppingToken);
        }

        var stopped = CreateStatus();
        logger.LogInformation(
            "Worker heartbeat stopping for {Service} in {Environment} at {UtcTime}",
            stopped.Service,
            stopped.Environment,
            stopped.Utc);
    }

    private ServiceStatusResponse CreateStatus() =>
        new("worker", true, DateTimeOffset.UtcNow, environment.EnvironmentName);
}
