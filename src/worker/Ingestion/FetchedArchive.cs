namespace ChessMonitor.Worker.Ingestion;

public sealed record FetchedArchive(
    string ArchiveId,
    string SourceSystem,
    string PlayerUsername,
    IReadOnlyCollection<RawGameRecord> Games);
