# ChessMonitor (Local-First) – Architecture

## Purpose
Run locally on a single PC to ingest Chess.com games and compute:
- Average blunders/mistakes per game
- Win ratio by opening
- Split by Rapid vs Blitz

Designed to be hostable later with minimal changes.

## Local deployment model
Docker Compose runs 4 services:
- web: React + Tailwind UI (Vite)
- api: ASP.NET Core Web API (.NET 8)
- worker: .NET Worker Service (ingestion + analysis)
- db: Postgres (persistent volume)

## Data flow
1. worker fetches monthly game archives and games (JSON/PGN).
2. worker stores raw games in db (idempotent upsert).
3. worker analyses unanalysed games with Stockfish:
   - per-move centipawn loss and labels
   - per-game totals (blunders/mistakes/inaccuracies, avg CPL)
4. api serves stats from db to web.
5. web displays dashboards and filters.

## Key design choices
- Separate worker service to keep analysis off the API thread pool.
- Stockfish runs inside the worker container for consistency.
- Aggregated stats persisted in db for fast UI queries.

## Hosting later
- Replace db connection string with managed DB.
- Deploy api + worker as separate services (App Service/Container Apps/etc).
- web becomes a static build behind a CDN/reverse proxy.