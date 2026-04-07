namespace ChessMonitor.Worker.Ingestion;

public sealed record PersistenceWriteResult(
    int Attempted,
    int Persisted);
