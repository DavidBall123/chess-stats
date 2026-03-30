using ChessMonitor.Shared.Domain;

namespace ChessMonitor.Shared.Contracts;

public sealed record GameUpsertRequest(
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
    AnalysisResult? Analysis);
