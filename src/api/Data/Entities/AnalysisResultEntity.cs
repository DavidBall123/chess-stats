namespace ChessMonitor.Api.Data.Entities;

public sealed class AnalysisResultEntity
{
    public Guid GameId { get; set; }
    public int Inaccuracies { get; set; }
    public int Mistakes { get; set; }
    public int Blunders { get; set; }
    public decimal? AverageCentipawnLoss { get; set; }
    public DateTimeOffset AnalyzedAtUtc { get; set; }

    public ChessGameEntity Game { get; set; } = null!;
}
