# Setup Guide

## Prerequisites
- Windows 11 or Windows 10 with WSL2
- Ubuntu (WSL distribution)
- Docker Desktop with WSL Integration enabled for Ubuntu
- Git
- .NET 10 SDK

## WSL + Docker Desktop
1. Verify WSL status:
   ```bash
   wsl -l -v
   ```
2. Ensure your Ubuntu distro is version 2.
3. Open Docker Desktop and enable:
   - `Settings` -> `Resources` -> `WSL Integration`
   - Toggle integration for your Ubuntu distro.
4. Verify from Ubuntu shell:
   ```bash
   docker --version
   docker compose version
   docker run --rm hello-world
   ```

## Project Bootstrap
1. Clone and enter repository.
2. Create local env file:
   ```bash
   cp .env.example .env
   ```
3. Edit `.env`.
4. Required:
   - `CHESSCOM_USERNAME`
5. Optional overrides with defaults already supplied in `.env.example`:
   - `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`
   - `DB_PORT`, `API_PORT`, `WEB_PORT`
   - `STOCKFISH_THREADS`, `STOCKFISH_DEPTH`
   - `VITE_API_BASE_URL`
   - `ASPNETCORE_ENVIRONMENT`, `DOTNET_ENVIRONMENT`
6. Start stack:
   ```bash
   docker compose --env-file .env -f infra/docker-compose.yml up --build
   ```
7. Verify endpoints:
   - `http://localhost:8080/health`
   - `http://localhost:8080/api/dashboard/overview`
   - `http://localhost:8080/api/dashboard/filters`

## Service Endpoints
- Web UI: `http://localhost:5173`
- API: `http://localhost:8080`
- Postgres: `localhost:5432`

## Common Commands
- Build the .NET projects locally:
  ```bash
  dotnet build src/api/ChessMonitor.Api.csproj
  dotnet build src/worker/ChessMonitor.Worker.csproj
  ```
- Start detached:
  ```bash
  docker compose --env-file .env -f infra/docker-compose.yml up -d --build
  ```
- Stop services:
  ```bash
  docker compose --env-file .env -f infra/docker-compose.yml down
  ```
- Stop and remove volume (fresh DB):
  ```bash
  docker compose --env-file .env -f infra/docker-compose.yml down -v
  ```

## Troubleshooting
- If `dotnet build` uses the wrong SDK, run `dotnet --info` and confirm the repo-local `global.json` resolves to .NET 10.
- If Docker commands fail in WSL, confirm Docker Desktop is running.
- If ports are already in use, stop conflicting local services.
- If worker exits early, confirm `.env` includes a valid `CHESSCOM_USERNAME`.
- If the dashboard endpoints return no data, remove the DB volume and restart so the sample seed runs against a clean database.
- If repo-root `.env` values do not appear inside containers, make sure the command includes `--env-file .env`.
