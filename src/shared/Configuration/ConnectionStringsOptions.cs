using System.ComponentModel.DataAnnotations;
namespace ChessMonitor.Shared.Configuration;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required]
    public string Default { get; init; } = string.Empty;
}
