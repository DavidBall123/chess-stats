using ChessMonitor.Shared.Contracts;
using ChessMonitor.Shared.Domain;
using Npgsql;

namespace ChessMonitor.Api.Data;

public sealed class ChessMonitorRepository(NpgsqlDataSource dataSource)
{
    public async Task<Guid> UpsertGameAsync(GameUpsertRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var gameId = await UpsertGameCoreAsync(connection, transaction, request, cancellationToken);

        if (request.Analysis is not null)
        {
            await UpsertAnalysisCoreAsync(connection, transaction, request.Analysis with { GameId = gameId }, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return gameId;
    }

    public async Task<bool> HasGamesAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT EXISTS (SELECT 1 FROM games);", connection);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    public async Task<DashboardOverviewResponse> GetDashboardOverviewAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string summarySql = """
            SELECT
                COUNT(*)::int AS total_games,
                COUNT(ar.game_id)::int AS analyzed_games,
                COALESCE(SUM(CASE WHEN g.result = 'win' THEN 1 ELSE 0 END), 0)::int AS wins,
                COALESCE(SUM(CASE WHEN g.result = 'draw' THEN 1 ELSE 0 END), 0)::int AS draws,
                COALESCE(SUM(CASE WHEN g.result = 'loss' THEN 1 ELSE 0 END), 0)::int AS losses,
                COALESCE(AVG(ar.blunders), 0)::numeric(10,2) AS avg_blunders,
                COALESCE(AVG(ar.mistakes), 0)::numeric(10,2) AS avg_mistakes,
                COALESCE(AVG(ar.inaccuracies), 0)::numeric(10,2) AS avg_inaccuracies
            FROM games g
            LEFT JOIN analysis_results ar ON ar.game_id = g.id;
            """;

        int totalGames;
        int analyzedGames;
        int wins;
        int draws;
        int losses;
        decimal averageBlundersPerGame;
        decimal averageMistakesPerGame;
        decimal averageInaccuraciesPerGame;

        await using (var command = new NpgsqlCommand(summarySql, connection))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            await reader.ReadAsync(cancellationToken);
            totalGames = reader.GetInt32(0);
            analyzedGames = reader.GetInt32(1);
            wins = reader.GetInt32(2);
            draws = reader.GetInt32(3);
            losses = reader.GetInt32(4);
            averageBlundersPerGame = reader.GetDecimal(5);
            averageMistakesPerGame = reader.GetDecimal(6);
            averageInaccuraciesPerGame = reader.GetDecimal(7);
        }

        var openingStats = await GetOpeningStatsAsync(connection, cancellationToken);
        var timeControlStats = await GetTimeControlStatsAsync(connection, cancellationToken);

        return new DashboardOverviewResponse(
            totalGames,
            analyzedGames,
            wins,
            draws,
            losses,
            averageBlundersPerGame,
            averageMistakesPerGame,
            averageInaccuraciesPerGame,
            openingStats,
            timeControlStats);
    }

    public async Task<DashboardFiltersResponse> GetDashboardFiltersAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string openingsSql = """
            SELECT
                COALESCE(opening_code, 'unknown') AS opening_code,
                COALESCE(opening_name, 'Unknown opening') AS opening_name,
                COUNT(*)::int AS games
            FROM games
            GROUP BY COALESCE(opening_code, 'unknown'), COALESCE(opening_name, 'Unknown opening')
            ORDER BY games DESC, opening_name ASC;
            """;

        var openings = new List<OpeningFilterOption>();
        await using (var command = new NpgsqlCommand(openingsSql, connection))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                openings.Add(new OpeningFilterOption(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetInt32(2)));
            }
        }

        const string timeControlsSql = """
            SELECT DISTINCT time_control
            FROM games
            ORDER BY time_control ASC;
            """;

        var timeControls = new List<string>();
        await using (var command = new NpgsqlCommand(timeControlsSql, connection))
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                timeControls.Add(reader.GetString(0));
            }
        }

        return new DashboardFiltersResponse(openings, timeControls);
    }

    private static async Task<Guid> UpsertGameCoreAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        GameUpsertRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO games (
                id,
                source_system,
                external_game_id,
                player_username,
                white_player,
                black_player,
                played_at_utc,
                result,
                time_control,
                opening_code,
                opening_name,
                pgn,
                created_at_utc,
                updated_at_utc
            )
            VALUES (
                @id,
                @sourceSystem,
                @externalGameId,
                @playerUsername,
                @whitePlayer,
                @blackPlayer,
                @playedAtUtc,
                @result,
                @timeControl,
                @openingCode,
                @openingName,
                @pgn,
                @createdAtUtc,
                @updatedAtUtc
            )
            ON CONFLICT (source_system, external_game_id)
            DO UPDATE SET
                player_username = EXCLUDED.player_username,
                white_player = EXCLUDED.white_player,
                black_player = EXCLUDED.black_player,
                played_at_utc = EXCLUDED.played_at_utc,
                result = EXCLUDED.result,
                time_control = EXCLUDED.time_control,
                opening_code = EXCLUDED.opening_code,
                opening_name = EXCLUDED.opening_name,
                pgn = EXCLUDED.pgn,
                updated_at_utc = EXCLUDED.updated_at_utc
            RETURNING id;
            """;

        await using var command = new NpgsqlCommand(sql, connection, transaction);

        var now = DateTimeOffset.UtcNow;
        command.Parameters.AddWithValue("id", Guid.NewGuid());
        command.Parameters.AddWithValue("sourceSystem", request.SourceSystem);
        command.Parameters.AddWithValue("externalGameId", request.ExternalGameId);
        command.Parameters.AddWithValue("playerUsername", request.PlayerUsername);
        command.Parameters.AddWithValue("whitePlayer", request.WhitePlayer);
        command.Parameters.AddWithValue("blackPlayer", request.BlackPlayer);
        command.Parameters.AddWithValue("playedAtUtc", request.PlayedAtUtc);
        command.Parameters.AddWithValue("result", request.Result);
        command.Parameters.AddWithValue("timeControl", request.TimeControl);
        command.Parameters.AddWithValue("openingCode", (object?)request.OpeningCode ?? DBNull.Value);
        command.Parameters.AddWithValue("openingName", (object?)request.OpeningName ?? DBNull.Value);
        command.Parameters.AddWithValue("pgn", (object?)request.Pgn ?? DBNull.Value);
        command.Parameters.AddWithValue("createdAtUtc", now);
        command.Parameters.AddWithValue("updatedAtUtc", now);

        return (Guid)(await command.ExecuteScalarAsync(cancellationToken)
            ?? throw new InvalidOperationException("Game upsert did not return an id."));
    }

    private static async Task UpsertAnalysisCoreAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        AnalysisResult result,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO analysis_results (
                game_id,
                inaccuracies,
                mistakes,
                blunders,
                average_centipawn_loss,
                analyzed_at_utc
            )
            VALUES (
                @gameId,
                @inaccuracies,
                @mistakes,
                @blunders,
                @averageCentipawnLoss,
                @analyzedAtUtc
            )
            ON CONFLICT (game_id)
            DO UPDATE SET
                inaccuracies = EXCLUDED.inaccuracies,
                mistakes = EXCLUDED.mistakes,
                blunders = EXCLUDED.blunders,
                average_centipawn_loss = EXCLUDED.average_centipawn_loss,
                analyzed_at_utc = EXCLUDED.analyzed_at_utc;
            """;

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("gameId", result.GameId);
        command.Parameters.AddWithValue("inaccuracies", result.Inaccuracies);
        command.Parameters.AddWithValue("mistakes", result.Mistakes);
        command.Parameters.AddWithValue("blunders", result.Blunders);
        command.Parameters.AddWithValue("averageCentipawnLoss", (object?)result.AverageCentipawnLoss ?? DBNull.Value);
        command.Parameters.AddWithValue("analyzedAtUtc", result.AnalyzedAtUtc);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<IReadOnlyList<OpeningStat>> GetOpeningStatsAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                COALESCE(g.opening_code, 'unknown') AS opening_code,
                COALESCE(g.opening_name, 'Unknown opening') AS opening_name,
                COUNT(*)::int AS games,
                COALESCE(SUM(CASE WHEN g.result = 'win' THEN 1 ELSE 0 END), 0)::int AS wins,
                COALESCE(SUM(CASE WHEN g.result = 'draw' THEN 1 ELSE 0 END), 0)::int AS draws,
                COALESCE(SUM(CASE WHEN g.result = 'loss' THEN 1 ELSE 0 END), 0)::int AS losses,
                COALESCE(AVG(CASE WHEN g.result = 'win' THEN 1.0 ELSE 0.0 END), 0)::numeric(10,4) AS win_ratio
            FROM games g
            GROUP BY COALESCE(g.opening_code, 'unknown'), COALESCE(g.opening_name, 'Unknown opening')
            ORDER BY games DESC, opening_name ASC
            LIMIT 10;
            """;

        var items = new List<OpeningStat>();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new OpeningStat(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetDecimal(6)));
        }

        return items;
    }

    private static async Task<IReadOnlyList<TimeControlStat>> GetTimeControlStatsAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                g.time_control,
                COUNT(*)::int AS games,
                COALESCE(SUM(CASE WHEN g.result = 'win' THEN 1 ELSE 0 END), 0)::int AS wins,
                COALESCE(SUM(CASE WHEN g.result = 'draw' THEN 1 ELSE 0 END), 0)::int AS draws,
                COALESCE(SUM(CASE WHEN g.result = 'loss' THEN 1 ELSE 0 END), 0)::int AS losses,
                COALESCE(AVG(CASE WHEN g.result = 'win' THEN 1.0 ELSE 0.0 END), 0)::numeric(10,4) AS win_ratio,
                COALESCE(AVG(ar.blunders), 0)::numeric(10,2) AS avg_blunders,
                COALESCE(AVG(ar.mistakes), 0)::numeric(10,2) AS avg_mistakes
            FROM games g
            LEFT JOIN analysis_results ar ON ar.game_id = g.id
            GROUP BY g.time_control
            ORDER BY games DESC, g.time_control ASC;
            """;

        var items = new List<TimeControlStat>();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new TimeControlStat(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetDecimal(5),
                reader.GetDecimal(6),
                reader.GetDecimal(7)));
        }

        return items;
    }
}
