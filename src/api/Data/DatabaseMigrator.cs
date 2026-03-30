using Npgsql;

namespace ChessMonitor.Api.Data;

public sealed class DatabaseMigrator(ILogger<DatabaseMigrator> logger, NpgsqlDataSource dataSource)
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        var migrationsPath = Path.Combine(AppContext.BaseDirectory, "Data", "Migrations");
        if (!Directory.Exists(migrationsPath))
        {
            throw new DirectoryNotFoundException($"Migration directory not found: {migrationsPath}");
        }

        var migrationFiles = Directory.GetFiles(migrationsPath, "*.sql")
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string createMigrationsTableSql = """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                migration_id text PRIMARY KEY,
                applied_at_utc timestamptz NOT NULL
            );
            """;

        await using (var createTableCommand = new NpgsqlCommand(createMigrationsTableSql, connection))
        {
            await createTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var migrationFile in migrationFiles)
        {
            var migrationId = Path.GetFileNameWithoutExtension(migrationFile);

            await using var existsCommand = new NpgsqlCommand(
                "SELECT EXISTS (SELECT 1 FROM schema_migrations WHERE migration_id = @migrationId);",
                connection);
            existsCommand.Parameters.AddWithValue("migrationId", migrationId);

            var alreadyApplied = (bool)(await existsCommand.ExecuteScalarAsync(cancellationToken) ?? false);
            if (alreadyApplied)
            {
                continue;
            }

            var sql = await File.ReadAllTextAsync(migrationFile, cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                await using (var migrationCommand = new NpgsqlCommand(sql, connection, transaction))
                {
                    await migrationCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await using (var recordCommand = new NpgsqlCommand(
                    "INSERT INTO schema_migrations (migration_id, applied_at_utc) VALUES (@migrationId, @appliedAtUtc);",
                    connection,
                    transaction))
                {
                    recordCommand.Parameters.AddWithValue("migrationId", migrationId);
                    recordCommand.Parameters.AddWithValue("appliedAtUtc", DateTimeOffset.UtcNow);
                    await recordCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Applied database migration {MigrationId}", migrationId);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
