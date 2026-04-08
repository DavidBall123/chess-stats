using ChessMonitor.Shared.Contracts;

namespace ChessMonitor.Worker.Ingestion;

public sealed class MockPersistenceWriter : IPersistenceWriter
{
    public Task<PersistenceWriteResult> WriteGamesAsync(
        IReadOnlyCollection<GameUpsertRequest> games,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new PersistenceWriteResult(
            Attempted: games.Count,
            Persisted: games.Count));
    }
}
