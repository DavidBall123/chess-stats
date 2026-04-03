using ChessMonitor.Shared.Contracts;

namespace ChessMonitor.Worker.Ingestion;

public interface IArchiveFetcher
{
    Task<IReadOnlyCollection<FetchedArchive>> FetchArchivesAsync(CancellationToken cancellationToken);
}
