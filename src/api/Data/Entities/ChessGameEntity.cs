namespace ChessMonitor.Api.Data.Entities;

public sealed class ChessGameEntity
{
    public Guid Id { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string ExternalGameId { get; set; } = string.Empty;
    public string PlayerUsername { get; set; } = string.Empty;
    public string WhitePlayer { get; set; } = string.Empty;
    public string BlackPlayer { get; set; } = string.Empty;
    public DateTimeOffset PlayedAtUtc { get; set; }
    public string Result { get; set; } = string.Empty;
    public string TimeControl { get; set; } = string.Empty;
    public string? OpeningCode { get; set; }
    public string? OpeningName { get; set; }
    public string? Pgn { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public AnalysisResultEntity? AnalysisResult { get; set; }
}
