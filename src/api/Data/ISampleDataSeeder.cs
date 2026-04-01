namespace ChessMonitor.Api.Data;

public interface ISampleDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}
