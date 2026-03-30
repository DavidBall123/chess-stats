namespace ChessMonitor.Shared.Configuration;

public sealed class StockfishOptions
{
    public const string SectionName = "Stockfish";

    public int Threads { get; init; } = 8;

    public int Depth { get; init; } = 16;
}
