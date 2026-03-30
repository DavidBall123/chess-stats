CREATE TABLE IF NOT EXISTS schema_migrations (
    migration_id text PRIMARY KEY,
    applied_at_utc timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS games (
    id uuid PRIMARY KEY,
    source_system text NOT NULL,
    external_game_id text NOT NULL,
    player_username text NOT NULL,
    white_player text NOT NULL,
    black_player text NOT NULL,
    played_at_utc timestamptz NOT NULL,
    result text NOT NULL,
    time_control text NOT NULL,
    opening_code text NULL,
    opening_name text NULL,
    pgn text NULL,
    created_at_utc timestamptz NOT NULL,
    updated_at_utc timestamptz NOT NULL,
    CONSTRAINT uq_games_source_external UNIQUE (source_system, external_game_id)
);

CREATE TABLE IF NOT EXISTS analysis_results (
    game_id uuid PRIMARY KEY REFERENCES games(id) ON DELETE CASCADE,
    inaccuracies integer NOT NULL,
    mistakes integer NOT NULL,
    blunders integer NOT NULL,
    average_centipawn_loss numeric(10, 2) NULL,
    analyzed_at_utc timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_games_played_at_utc ON games (played_at_utc DESC);
CREATE INDEX IF NOT EXISTS ix_games_time_control ON games (time_control);
CREATE INDEX IF NOT EXISTS ix_games_opening_code ON games (opening_code);
