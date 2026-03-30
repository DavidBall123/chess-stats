namespace ChessMonitor.Shared.Domain;

public sealed record TimeControlStat(
    string TimeControl,
    int Games,
    int Wins,
    int Draws,
    int Losses,
    decimal WinRatio,
    decimal AverageBlunders,
    decimal AverageMistakes);
