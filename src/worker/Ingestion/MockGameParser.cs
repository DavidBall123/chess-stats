using ChessMonitor.Shared.Contracts;

namespace ChessMonitor.Worker.Ingestion;

public sealed class MockGameParser : IGameParser
{
    public Task<IReadOnlyCollection<GameUpsertRequest>> ParseGamesAsync(
        FetchedArchive archive,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var games = archive.Games
            .Select(game => new GameUpsertRequest(
                SourceSystem: archive.SourceSystem,
                ExternalGameId: game.ExternalGameId,
                PlayerUsername: archive.PlayerUsername,
                WhitePlayer: game.WhitePlayer,
                BlackPlayer: game.BlackPlayer,
                PlayedAtUtc: game.PlayedAtUtc,
                Result: game.Result,
                TimeControl: game.TimeControl,
                OpeningCode: game.OpeningCode,
                OpeningName: game.OpeningName,
                Pgn: game.Pgn,
                Analysis: null))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<GameUpsertRequest>>(games);
    }
}
