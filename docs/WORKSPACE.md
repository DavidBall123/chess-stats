# Workspace Scaffold Reference

This document defines the expected workspace shape for the .NET solution once service projects are scaffolded.

## Planned Solution File
- Root solution file: `ChessMonitor.sln`

## Planned Projects
- `src/api/ChessMonitor.Api.csproj`
- `src/worker/ChessMonitor.Worker.csproj`
- `src/shared/ChessMonitor.Shared.csproj`

## Dependency Direction
- `ChessMonitor.Api` -> references `ChessMonitor.Shared`
- `ChessMonitor.Worker` -> references `ChessMonitor.Shared`
- `ChessMonitor.Shared` -> references no service-specific projects

## Why This Structure
- Keeps shared contracts centralized and versioned.
- Preserves clean separation between request/response API and background processing.
- Aligns with current `infra/docker-compose.yml` service boundaries.
