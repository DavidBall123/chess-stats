namespace ChessMonitor.Worker.Ingestion;

public sealed record RawGameRecord(
    string ExternalGameId,
    string WhitePlayer,
    string BlackPlayer,
    DateTimeOffset PlayedAtUtc,
    string Result,
    string TimeControl,
    string? OpeningCode,
    string? OpeningName,
    string? Pgn);
