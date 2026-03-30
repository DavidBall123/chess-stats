namespace ChessMonitor.Shared.Domain;

public sealed record Game(
    Guid Id,
    string SourceSystem,
    string ExternalGameId,
    string PlayerUsername,
    string WhitePlayer,
    string BlackPlayer,
    DateTimeOffset PlayedAtUtc,
    string Result,
    string TimeControl,
    string? OpeningCode,
    string? OpeningName,
    string? Pgn,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
