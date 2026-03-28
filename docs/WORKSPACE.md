# Workspace Reference

This document describes the current .NET workspace shape in the repository.

## Current Projects
- `src/api/ChessMonitor.Api.csproj`
- `src/worker/ChessMonitor.Worker.csproj`

## SDK and Framework
- `global.json` pins the SDK to `.NET 10` (`10.0.104`).
- `ChessMonitor.Api` targets `net10.0`.
- `ChessMonitor.Worker` targets `net10.0`.

## Planned Solution File
- Root solution file: `ChessMonitor.sln`

## Planned Shared Project
- `src/shared/ChessMonitor.Shared.csproj`

## Dependency Direction
- `ChessMonitor.Api` -> references `ChessMonitor.Shared`
- `ChessMonitor.Worker` -> references `ChessMonitor.Shared`
- `ChessMonitor.Shared` -> references no service-specific projects

## Why This Structure
- Keeps shared contracts centralized and versioned.
- Preserves clean separation between request/response API and background processing.
- Aligns with current `infra/docker-compose.yml` service boundaries.
