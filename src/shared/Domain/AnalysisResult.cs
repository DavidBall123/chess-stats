namespace ChessMonitor.Shared.Domain;

public sealed record AnalysisResult(
    Guid GameId,
    int Inaccuracies,
    int Mistakes,
    int Blunders,
    decimal? AverageCentipawnLoss,
    DateTimeOffset AnalyzedAtUtc);
