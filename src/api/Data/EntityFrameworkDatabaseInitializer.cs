using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ChessMonitor.Api.Data;

public sealed class EntityFrameworkDatabaseInitializer(
    ChessMonitorDbContext dbContext,
    ILogger<EntityFrameworkDatabaseInitializer> logger) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await BaselineLegacySchemaAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Applied Entity Framework migrations.");
    }

    private async Task BaselineLegacySchemaAsync(CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            var gamesTableExists = await TableExistsAsync(connection, "games", cancellationToken);
            var analysisResultsTableExists = await TableExistsAsync(connection, "analysis_results", cancellationToken);
            var efHistoryTableExists = await TableExistsAsync(connection, "__EFMigrationsHistory", cancellationToken);

            if (!gamesTableExists || !analysisResultsTableExists || efHistoryTableExists)
            {
                return;
            }

            var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();
            var initialMigrationId = migrationsAssembly.Migrations.Keys.OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(initialMigrationId))
            {
                return;
            }

            var productVersion = typeof(DbContext).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                .Split('+', 2)[0]
                ?? "9.0.4";

            await using (var createHistoryCommand = connection.CreateCommand())
            {
                createHistoryCommand.CommandText = """
                    CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                        "MigrationId" character varying(150) NOT NULL,
                        "ProductVersion" character varying(32) NOT NULL,
                        CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
                    );
                    """;
                await createHistoryCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var insertHistoryCommand = connection.CreateCommand())
            {
                insertHistoryCommand.CommandText = """
                    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                    VALUES (@migrationId, @productVersion)
                    ON CONFLICT ("MigrationId") DO NOTHING;
                    """;

                var migrationParameter = insertHistoryCommand.CreateParameter();
                migrationParameter.ParameterName = "@migrationId";
                migrationParameter.Value = initialMigrationId;
                insertHistoryCommand.Parameters.Add(migrationParameter);

                var versionParameter = insertHistoryCommand.CreateParameter();
                versionParameter.ParameterName = "@productVersion";
                versionParameter.Value = productVersion;
                insertHistoryCommand.Parameters.Add(versionParameter);

                await insertHistoryCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            logger.LogInformation(
                "Baselined legacy schema into EF migrations history with migration {MigrationId}.",
                initialMigrationId);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private static async Task<bool> TableExistsAsync(
        System.Data.Common.DbConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = @tableName
            );
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }
}
