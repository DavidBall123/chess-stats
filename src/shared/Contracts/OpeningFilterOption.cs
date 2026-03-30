namespace ChessMonitor.Shared.Contracts;

public sealed record OpeningFilterOption(
    string OpeningCode,
    string OpeningName,
    int Games);
