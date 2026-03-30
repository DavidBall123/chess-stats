# ChessMonitor

Local-first chess analytics template using Docker Compose with separate `web`, `api`, `worker`, and `db` services.

## Stack
- Web: React + Vite + Tailwind (containerized)
- API: ASP.NET Core Web API (.NET 10)
- Worker: .NET Worker Service (ingestion + analysis)
- Database: PostgreSQL 16

## Repository Layout
- `infra/docker-compose.yml`: local orchestration entrypoint
- `src/web`: frontend app
- `src/api`: backend API
- `src/worker`: ingestion and analysis worker
- `src/shared`: shared contracts/domain models
- `docs/SETUP.md`: environment setup and first run
- `docs/WORKSPACE.md`: current .NET workspace/solution structure

## Quick Start
1. Copy `.env.example` to `.env` and set `CHESSCOM_USERNAME`.
2. Start Docker Desktop (WSL integration enabled if using WSL).
3. Run:
   ```bash
   docker compose --env-file .env -f infra/docker-compose.yml up --build
   ```
4. Access:
   - Web: `http://localhost:5173`
   - API: `http://localhost:8080`
   - Postgres: `localhost:5432`

Required variables:
- `CHESSCOM_USERNAME`

Optional variables with safe defaults:
- `POSTGRES_DB`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `DB_PORT`
- `API_PORT`
- `WEB_PORT`
- `STOCKFISH_THREADS`
- `STOCKFISH_DEPTH`
- `VITE_API_BASE_URL`
- `ASPNETCORE_ENVIRONMENT`
- `DOTNET_ENVIRONMENT`

Useful API endpoints:
- `GET /health`
- `GET /api/dashboard/overview`
- `GET /api/dashboard/filters`

Note:
When using `infra/docker-compose.yml`, pass `--env-file .env` so Docker Compose reads the repo-root environment file.

## Development Notes
- Local services are container-first; host-installed .NET 10 SDK/Node is optional.
- Keep `api` request handling independent from heavy analysis work in `worker`.
- Use `src/shared` only for stable cross-service contracts.

## Next Milestones
Refer to `PROJECT_TEMPLATE_PLAN.md` for phased implementation details.
