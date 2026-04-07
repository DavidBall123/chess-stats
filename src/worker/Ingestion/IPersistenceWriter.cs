using ChessMonitor.Shared.Contracts;

namespace ChessMonitor.Worker.Ingestion;

public interface IPersistenceWriter
{
    Task<PersistenceWriteResult> WriteGamesAsync(
        IReadOnlyCollection<GameUpsertRequest> games,
        CancellationToken cancellationToken);
}
