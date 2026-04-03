using ChessMonitor.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace ChessMonitor.Worker.Ingestion;

public sealed class MockArchiveFetcher(
    IOptions<ChessComOptions> chessComOptions) : IArchiveFetcher
{
    public Task<IReadOnlyCollection<FetchedArchive>> FetchArchivesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var username = chessComOptions.Value.Username?.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult<IReadOnlyCollection<FetchedArchive>>(Array.Empty<FetchedArchive>());
        }

        var archive = new FetchedArchive(
            ArchiveId: $"{username}-demo-archive",
            SourceSystem: "chess.com",
            PlayerUsername: username,
            Games:
            [
                new RawGameRecord(
                    ExternalGameId: $"{username}-demo-game-1",
                    WhitePlayer: username,
                    BlackPlayer: "opponent",
                    PlayedAtUtc: DateTimeOffset.UtcNow.AddDays(-1),
                    Result: "win",
                    TimeControl: "rapid",
                    OpeningCode: "C20",
                    OpeningName: "King's Pawn Game",
                    Pgn: null)
            ]);

        return Task.FromResult<IReadOnlyCollection<FetchedArchive>>([archive]);
    }
}
