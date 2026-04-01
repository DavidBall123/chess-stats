using ChessMonitor.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChessMonitor.Api.Data;

public sealed class ChessMonitorDbContextFactory : IDesignTimeDbContextFactory<ChessMonitorDbContext>
{
    public ChessMonitorDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetSection(ConnectionStringsOptions.SectionName)[nameof(ConnectionStringsOptions.Default)]
            ?? "Host=localhost;Port=5432;Database=chessmonitor;Username=chess;Password=chess";

        var optionsBuilder = new DbContextOptionsBuilder<ChessMonitorDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new ChessMonitorDbContext(optionsBuilder.Options);
    }
}
