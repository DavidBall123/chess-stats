namespace ChessMonitor.Shared.Domain;

public sealed record OpeningStat(
    string OpeningCode,
    string OpeningName,
    int Games,
    int Wins,
    int Draws,
    int Losses,
    decimal WinRatio);
