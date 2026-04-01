using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessMonitor.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_system = table.Column<string>(type: "text", nullable: false),
                    external_game_id = table.Column<string>(type: "text", nullable: false),
                    player_username = table.Column<string>(type: "text", nullable: false),
                    white_player = table.Column<string>(type: "text", nullable: false),
                    black_player = table.Column<string>(type: "text", nullable: false),
                    played_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    result = table.Column<string>(type: "text", nullable: false),
                    time_control = table.Column<string>(type: "text", nullable: false),
                    opening_code = table.Column<string>(type: "text", nullable: true),
                    opening_name = table.Column<string>(type: "text", nullable: true),
                    pgn = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analysis_results",
                columns: table => new
                {
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inaccuracies = table.Column<int>(type: "integer", nullable: false),
                    mistakes = table.Column<int>(type: "integer", nullable: false),
                    blunders = table.Column<int>(type: "integer", nullable: false),
                    average_centipawn_loss = table.Column<decimal>(type: "numeric", nullable: true),
                    analyzed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_results", x => x.game_id);
                    table.ForeignKey(
                        name: "FK_analysis_results_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_games_source_system_external_game_id",
                table: "games",
                columns: new[] { "source_system", "external_game_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analysis_results");

            migrationBuilder.DropTable(
                name: "games");
        }
    }
}
