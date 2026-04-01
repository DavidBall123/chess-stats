namespace ChessMonitor.Api.Data;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
