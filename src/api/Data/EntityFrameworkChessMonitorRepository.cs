using ChessMonitor.Api.Data.Entities;
using ChessMonitor.Shared.Contracts;
using ChessMonitor.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace ChessMonitor.Api.Data;

public sealed class EntityFrameworkChessMonitorRepository(ChessMonitorDbContext dbContext) : IChessMonitorRepository
{
    public Task<bool> HasGamesAsync(CancellationToken cancellationToken) =>
        dbContext.Games.AsNoTracking().AnyAsync(cancellationToken);

    public async Task<Guid> UpsertGameAsync(GameUpsertRequest request, CancellationToken cancellationToken)
    {
        var existingGame = await dbContext.Games
            .Include(x => x.AnalysisResult)
            .SingleOrDefaultAsync(
                x => x.SourceSystem == request.SourceSystem && x.ExternalGameId == request.ExternalGameId,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;

        if (existingGame is null)
        {
            existingGame = new ChessGameEntity
            {
                Id = Guid.NewGuid(),
                SourceSystem = request.SourceSystem,
                ExternalGameId = request.ExternalGameId,
                CreatedAtUtc = now
            };

            dbContext.Games.Add(existingGame);
        }

        existingGame.PlayerUsername = request.PlayerUsername;
        existingGame.WhitePlayer = request.WhitePlayer;
        existingGame.BlackPlayer = request.BlackPlayer;
        existingGame.PlayedAtUtc = request.PlayedAtUtc;
        existingGame.Result = request.Result;
        existingGame.TimeControl = request.TimeControl;
        existingGame.OpeningCode = request.OpeningCode;
        existingGame.OpeningName = request.OpeningName;
        existingGame.Pgn = request.Pgn;
        existingGame.UpdatedAtUtc = now;

        if (request.Analysis is not null)
        {
            var analysis = existingGame.AnalysisResult ?? new AnalysisResultEntity { GameId = existingGame.Id };
            analysis.Inaccuracies = request.Analysis.Inaccuracies;
            analysis.Mistakes = request.Analysis.Mistakes;
            analysis.Blunders = request.Analysis.Blunders;
            analysis.AverageCentipawnLoss = request.Analysis.AverageCentipawnLoss;
            analysis.AnalyzedAtUtc = request.Analysis.AnalyzedAtUtc;

            if (existingGame.AnalysisResult is null)
            {
                existingGame.AnalysisResult = analysis;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existingGame.Id;
    }

    public async Task<DashboardOverviewResponse> GetDashboardOverviewAsync(CancellationToken cancellationToken)
    {
        var totalGames = await dbContext.Games.AsNoTracking().CountAsync(cancellationToken);
        var analyzedGames = await dbContext.AnalysisResults.AsNoTracking().CountAsync(cancellationToken);

        var resultSummary = await dbContext.Games
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Wins = group.Count(x => x.Result == "win"),
                Draws = group.Count(x => x.Result == "draw"),
                Losses = group.Count(x => x.Result == "loss")
            })
            .SingleOrDefaultAsync(cancellationToken);

        var analysisSummary = await dbContext.AnalysisResults
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                AverageBlunders = group.Average(x => (decimal?)x.Blunders) ?? 0m,
                AverageMistakes = group.Average(x => (decimal?)x.Mistakes) ?? 0m,
                AverageInaccuracies = group.Average(x => (decimal?)x.Inaccuracies) ?? 0m
            })
            .SingleOrDefaultAsync(cancellationToken);

        var openingStats = await dbContext.Games
            .AsNoTracking()
            .GroupBy(x => new
            {
                OpeningCode = x.OpeningCode ?? "unknown",
                OpeningName = x.OpeningName ?? "Unknown opening"
            })
            .Select(group => new
            {
                group.Key.OpeningCode,
                group.Key.OpeningName,
                Games = group.Count(),
                Wins = group.Count(x => x.Result == "win"),
                Draws = group.Count(x => x.Result == "draw"),
                Losses = group.Count(x => x.Result == "loss"),
                WinRatio = group.Average(x => x.Result == "win" ? 1m : 0m)
            })
            .OrderByDescending(x => x.Games)
            .ThenBy(x => x.OpeningName)
            .Take(10)
            .ToListAsync(cancellationToken);

        var timeControlStats = await dbContext.Games
            .AsNoTracking()
            .GroupBy(x => x.TimeControl)
            .Select(group => new
            {
                TimeControl = group.Key,
                Games = group.Count(),
                Wins = group.Count(x => x.Result == "win"),
                Draws = group.Count(x => x.Result == "draw"),
                Losses = group.Count(x => x.Result == "loss"),
                WinRatio = group.Average(x => x.Result == "win" ? 1m : 0m),
                AverageBlunders = group.Where(x => x.AnalysisResult != null).Average(x => (decimal?)x.AnalysisResult!.Blunders) ?? 0m,
                AverageMistakes = group.Where(x => x.AnalysisResult != null).Average(x => (decimal?)x.AnalysisResult!.Mistakes) ?? 0m
            })
            .OrderByDescending(x => x.Games)
            .ThenBy(x => x.TimeControl)
            .ToListAsync(cancellationToken);

        return new DashboardOverviewResponse(
            totalGames,
            analyzedGames,
            resultSummary?.Wins ?? 0,
            resultSummary?.Draws ?? 0,
            resultSummary?.Losses ?? 0,
            analysisSummary?.AverageBlunders ?? 0m,
            analysisSummary?.AverageMistakes ?? 0m,
            analysisSummary?.AverageInaccuracies ?? 0m,
            openingStats.Select(x => new OpeningStat(
                x.OpeningCode,
                x.OpeningName,
                x.Games,
                x.Wins,
                x.Draws,
                x.Losses,
                x.WinRatio)).ToList(),
            timeControlStats.Select(x => new TimeControlStat(
                x.TimeControl,
                x.Games,
                x.Wins,
                x.Draws,
                x.Losses,
                x.WinRatio,
                x.AverageBlunders,
                x.AverageMistakes)).ToList());
    }

    public async Task<DashboardFiltersResponse> GetDashboardFiltersAsync(CancellationToken cancellationToken)
    {
        var openings = await dbContext.Games
            .AsNoTracking()
            .GroupBy(x => new
            {
                OpeningCode = x.OpeningCode ?? "unknown",
                OpeningName = x.OpeningName ?? "Unknown opening"
            })
            .Select(group => new
            {
                group.Key.OpeningCode,
                group.Key.OpeningName,
                Games = group.Count()
            })
            .OrderByDescending(x => x.Games)
            .ThenBy(x => x.OpeningName)
            .ToListAsync(cancellationToken);

        var timeControls = await dbContext.Games
            .AsNoTracking()
            .Select(x => x.TimeControl)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new DashboardFiltersResponse(
            openings.Select(x => new OpeningFilterOption(x.OpeningCode, x.OpeningName, x.Games)).ToList(),
            timeControls);
    }
}
