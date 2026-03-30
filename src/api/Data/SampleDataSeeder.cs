using ChessMonitor.Shared.Configuration;
using ChessMonitor.Shared.Contracts;
using ChessMonitor.Shared.Domain;
using Microsoft.Extensions.Options;

namespace ChessMonitor.Api.Data;

public sealed class SampleDataSeeder(
    ChessMonitorRepository repository,
    IOptions<ChessComOptions> chessComOptions)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await repository.HasGamesAsync(cancellationToken))
        {
            return;
        }

        var username = string.IsNullOrWhiteSpace(chessComOptions.Value.Username)
            ? "sample-player"
            : chessComOptions.Value.Username;

        var now = DateTimeOffset.UtcNow;
        var sampleGames = new[]
        {
            new GameUpsertRequest(
                "chess.com",
                "sample-rapid-001",
                username,
                username,
                "opponent-rapid",
                now.AddDays(-9),
                "win",
                "rapid",
                "C20",
                "King's Pawn Game",
                null,
                new AnalysisResult(Guid.Empty, 1, 0, 0, 22.4m, now.AddDays(-8))),
            new GameUpsertRequest(
                "chess.com",
                "sample-blitz-001",
                username,
                "opponent-blitz",
                username,
                now.AddDays(-6),
                "loss",
                "blitz",
                "B01",
                "Scandinavian Defense",
                null,
                new AnalysisResult(Guid.Empty, 2, 1, 1, 48.8m, now.AddDays(-5))),
            new GameUpsertRequest(
                "chess.com",
                "sample-rapid-002",
                username,
                username,
                "opponent-solid",
                now.AddDays(-3),
                "draw",
                "rapid",
                "D02",
                "Queen's Pawn Game",
                null,
                new AnalysisResult(Guid.Empty, 3, 1, 0, 31.1m, now.AddDays(-2))),
            new GameUpsertRequest(
                "chess.com",
                "sample-blitz-002",
                username,
                username,
                "opponent-tactic",
                now.AddDays(-1),
                "win",
                "blitz",
                "C50",
                "Italian Game",
                null,
                new AnalysisResult(Guid.Empty, 1, 1, 0, 18.7m, now)),
        };

        foreach (var game in sampleGames)
        {
            await repository.UpsertGameAsync(game, cancellationToken);
        }
    }
}
