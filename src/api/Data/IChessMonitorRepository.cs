using ChessMonitor.Shared.Contracts;

namespace ChessMonitor.Api.Data;

public interface IChessMonitorRepository
{
    Task<Guid> UpsertGameAsync(GameUpsertRequest request, CancellationToken cancellationToken);
    Task<bool> HasGamesAsync(CancellationToken cancellationToken);
    Task<DashboardOverviewResponse> GetDashboardOverviewAsync(CancellationToken cancellationToken);
    Task<DashboardFiltersResponse> GetDashboardFiltersAsync(CancellationToken cancellationToken);
}
