namespace ChessMonitor.Shared.Configuration;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    public string Default { get; init; } = string.Empty;
}
