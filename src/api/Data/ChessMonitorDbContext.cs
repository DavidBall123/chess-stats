using ChessMonitor.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChessMonitor.Api.Data;

public sealed class ChessMonitorDbContext(DbContextOptions<ChessMonitorDbContext> options) : DbContext(options)
{
    public DbSet<ChessGameEntity> Games => Set<ChessGameEntity>();
    public DbSet<AnalysisResultEntity> AnalysisResults => Set<AnalysisResultEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var game = modelBuilder.Entity<ChessGameEntity>();
        game.ToTable("games");
        game.HasKey(x => x.Id);
        game.HasIndex(x => new { x.SourceSystem, x.ExternalGameId }).IsUnique();
        game.Property(x => x.Id).HasColumnName("id");
        game.Property(x => x.SourceSystem).HasColumnName("source_system");
        game.Property(x => x.ExternalGameId).HasColumnName("external_game_id");
        game.Property(x => x.PlayerUsername).HasColumnName("player_username");
        game.Property(x => x.WhitePlayer).HasColumnName("white_player");
        game.Property(x => x.BlackPlayer).HasColumnName("black_player");
        game.Property(x => x.PlayedAtUtc).HasColumnName("played_at_utc");
        game.Property(x => x.Result).HasColumnName("result");
        game.Property(x => x.TimeControl).HasColumnName("time_control");
        game.Property(x => x.OpeningCode).HasColumnName("opening_code");
        game.Property(x => x.OpeningName).HasColumnName("opening_name");
        game.Property(x => x.Pgn).HasColumnName("pgn");
        game.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        game.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        var analysis = modelBuilder.Entity<AnalysisResultEntity>();
        analysis.ToTable("analysis_results");
        analysis.HasKey(x => x.GameId);
        analysis.Property(x => x.GameId).HasColumnName("game_id");
        analysis.Property(x => x.Inaccuracies).HasColumnName("inaccuracies");
        analysis.Property(x => x.Mistakes).HasColumnName("mistakes");
        analysis.Property(x => x.Blunders).HasColumnName("blunders");
        analysis.Property(x => x.AverageCentipawnLoss).HasColumnName("average_centipawn_loss");
        analysis.Property(x => x.AnalyzedAtUtc).HasColumnName("analyzed_at_utc");

        game.HasOne(x => x.AnalysisResult)
            .WithOne(x => x.Game)
            .HasForeignKey<AnalysisResultEntity>(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
