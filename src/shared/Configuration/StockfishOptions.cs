using System.ComponentModel.DataAnnotations;

namespace ChessMonitor.Shared.Configuration;

public sealed class StockfishOptions
{
    public const string SectionName = "Stockfish";

    [Range(1, 128)]
    public int Threads { get; init; } = 8;

    [Range(1, 50)]
    public int Depth { get; init; } = 16;
}
