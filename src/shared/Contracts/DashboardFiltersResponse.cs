namespace ChessMonitor.Shared.Contracts;

public sealed record DashboardFiltersResponse(
    IReadOnlyList<OpeningFilterOption> Openings,
    IReadOnlyList<string> TimeControls);
