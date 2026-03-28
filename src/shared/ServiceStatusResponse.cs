namespace ChessMonitor.Shared;

public sealed record ServiceStatusResponse(
    string Service,
    bool Healthy,
    DateTimeOffset Utc,
    string Environment);
