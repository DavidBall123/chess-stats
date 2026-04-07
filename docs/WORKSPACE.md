# Workspace Reference

This document describes the current workspace shape in the repository.

## Current Solution and Projects
- Root solution file: `chess-stats.sln`
- `src/api/ChessMonitor.Api.csproj`
- `src/worker/ChessMonitor.Worker.csproj`
- `src/shared/ChessMonitor.Shared.csproj`

## Service Layout
- `src/web`: Vite/React frontend and container assets
- `src/api`: ASP.NET Core API, EF Core data access, and migrations
- `src/worker`: .NET worker service and pipeline scaffolding
- `src/shared`: shared contracts, domain models, and configuration types
- `infra/docker-compose.yml`: local orchestration entrypoint

## SDK and Framework
- `global.json` pins the SDK to `.NET 10` (`10.0.104`).
- `ChessMonitor.Api` targets `net10.0`.
- `ChessMonitor.Worker` targets `net10.0`.
- `ChessMonitor.Shared` targets `net10.0`.

## Current Dependency Direction
- `ChessMonitor.Api` references `ChessMonitor.Shared`
- `ChessMonitor.Worker` references `ChessMonitor.Shared`
- `ChessMonitor.Shared` references no service-specific projects
- `src/web` is a separate Node/Vite app and does not participate in the .NET solution

## Current Backend Responsibilities
- API owns HTTP endpoints, database initialization, repository access, and EF Core migrations.
- Worker owns background execution and ingestion pipeline extension points such as archive fetching, game parsing, and persistence writing.
- Shared owns stable cross-service models including dashboard contracts, game upsert contracts, domain entities, and configuration option types.

## Why This Structure
- Keeps the API and worker separated so background ingestion and analysis do not affect request handling.
- Centralizes stable shared models without coupling services to each other.
- Preserves a local-first container workflow while keeping the codebase ready for later hosted deployment.
