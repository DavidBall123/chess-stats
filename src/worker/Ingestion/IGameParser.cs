using ChessMonitor.Shared.Contracts;

namespace ChessMonitor.Worker.Ingestion;

public interface IGameParser
{
    Task<IReadOnlyCollection<GameUpsertRequest>> ParseGamesAsync(
        FetchedArchive archive,
        CancellationToken cancellationToken);
}
