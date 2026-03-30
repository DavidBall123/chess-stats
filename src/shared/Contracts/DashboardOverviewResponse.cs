using ChessMonitor.Shared.Domain;

namespace ChessMonitor.Shared.Contracts;

public sealed record DashboardOverviewResponse(
    int TotalGames,
    int AnalyzedGames,
    int Wins,
    int Draws,
    int Losses,
    decimal AverageBlundersPerGame,
    decimal AverageMistakesPerGame,
    decimal AverageInaccuraciesPerGame,
    IReadOnlyList<OpeningStat> OpeningStats,
    IReadOnlyList<TimeControlStat> TimeControlStats);
