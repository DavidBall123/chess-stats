namespace ChessMonitor.Shared.Configuration;

public sealed class ChessComOptions
{
    public const string SectionName = "ChessCom";

    public string Username { get; init; } = string.Empty;
}
