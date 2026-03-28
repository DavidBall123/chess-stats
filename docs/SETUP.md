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
3. Edit `.env` and set:
   - `CHESSCOM_USERNAME`
4. Start stack:
   ```bash
   docker compose -f infra/docker-compose.yml up --build
   ```

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
  docker compose -f infra/docker-compose.yml up -d --build
  ```
- Stop services:
  ```bash
  docker compose -f infra/docker-compose.yml down
  ```
- Stop and remove volume (fresh DB):
  ```bash
  docker compose -f infra/docker-compose.yml down -v
  ```

## Troubleshooting
- If `dotnet build` uses the wrong SDK, run `dotnet --info` and confirm the repo-local `global.json` resolves to .NET 10.
- If Docker commands fail in WSL, confirm Docker Desktop is running.
- If ports are already in use, stop conflicting local services.
- If worker exits early, confirm `.env` includes a valid `CHESSCOM_USERNAME`.
