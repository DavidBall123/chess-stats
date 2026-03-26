# ChessMonitor Project Template Plan

## Goal
Create a reusable project template from the current architecture (web/api/worker/db) while preserving local-first Docker Compose development and a clear path to later hosting.

## Current Architecture Review
- `Architecture.md` defines 4 services: `web` (Vite/React), `api` (.NET 8 Web API), `worker` (.NET worker), `db` (Postgres).
- `infra/docker-compose.yml` already wires these services correctly for local orchestration.
- `src/` folders already exist:
  - `src/web`
  - `src/api`
  - `src/worker`
  - `src/shared`

## Template Design Decisions
- Keep a monorepo template with one `docker-compose.yml` as the primary local entrypoint.
- Keep service boundaries explicit (`api` vs `worker`) to avoid coupling ingestion/analysis with request handling.
- Use `src/shared` for cross-service contracts/utilities to reduce duplication.
- Add configuration via `.env` and service-level appsettings/env overrides so users can bootstrap quickly.
- Default to development-safe settings, with clear placeholders for production migration.

## Implementation Plan

### Phase 1: Baseline Template Skeleton
1. Add solution-level metadata and onboarding files.
2. Add minimal runnable stubs in `web`, `api`, and `worker` (health endpoint, basic UI page, worker heartbeat log).
3. Add shared project/library in `src/shared` and reference it from `api` + `worker`.
4. Add Dockerfiles for each service (if missing) aligned with compose build contexts.

Deliverable:
- `docker compose up --build` starts all services and basic health checks pass.

### Phase 2: Configuration and Environment Model
1. Add `.env.example` at repo root with:
   - `CHESSCOM_USERNAME`
   - optional DB overrides
   - analysis defaults (stockfish depth/threads)
2. Update compose to consume `.env` values with explicit defaults where safe.
3. Add app configuration structure in `api` and `worker` (`ConnectionStrings`, `ChessCom`, `Stockfish`).
4. Document required vs optional variables.

Deliverable:
- New contributor can copy `.env.example` to `.env` and run locally without code edits.

### Phase 3: Data Layer and Contracts
1. Define domain entities in `shared` (Game, AnalysisResult, OpeningStat, TimeControlStat).
2. Set up DB schema/migrations in API or a dedicated data project.
3. Implement idempotent game upsert model required by architecture.
4. Define API response contracts for dashboard and filters.

Deliverable:
- DB can be initialized from migrations; API returns typed placeholder stats from DB.

### Phase 4: Worker Pipeline Template
1. Add ingestion pipeline interfaces:
   - archive fetcher
   - game parser
   - persistence writer
2. Add analysis pipeline interfaces:
   - unanalysed game selector
   - stockfish evaluator adapter
   - aggregate writer
3. Add retry/backoff and idempotency boundaries.
4. Add structured logging and correlation IDs for worker runs.

Deliverable:
- Worker loop runs with placeholder/mock adapters and persists traceable run state.

### Phase 5: API + Web Vertical Slice
1. Implement API endpoints for:
   - overview stats
   - opening win ratio
   - rapid vs blitz breakdown
2. Build web dashboard page consuming API via `VITE_API_BASE_URL`.
3. Add loading, empty-state, and API-error handling in web.

Deliverable:
- End-to-end slice visible in browser with live data from API.

### Phase 6: Quality Gates for Template Readiness
1. Add tests:
   - API unit/integration tests
   - worker pipeline unit tests
   - web component/API client tests
2. Add lint/format scripts and CI workflow.
3. Add compose healthchecks and startup ordering hardening.
4. Validate cold-start setup on clean machine instructions.

Deliverable:
- Template is reproducible, tested, and suitable as a starting point for new deployments.

## Suggested File/Folder Additions
- `/root/projects/chess-stats/.env.example`
- `/root/projects/chess-stats/docs/SETUP.md`
- `/root/projects/chess-stats/docs/DEPLOYMENT_NOTES.md`
- `/root/projects/chess-stats/src/api/` (project scaffold + Dockerfile + migrations)
- `/root/projects/chess-stats/src/worker/` (project scaffold + Dockerfile)
- `/root/projects/chess-stats/src/web/` (Vite app + Dockerfile)
- `/root/projects/chess-stats/src/shared/` (shared contracts/domain models)

## Risks and Mitigations
- Risk: Compose works locally but not in CI due to timing/order assumptions.
  - Mitigation: Add explicit healthchecks and dependency readiness checks.
- Risk: Worker analysis cost grows quickly with historical imports.
  - Mitigation: Batch processing limits, checkpointing, and resumable runs.
- Risk: Shared project becomes a dumping ground.
  - Mitigation: Restrict `shared` to stable contracts and domain primitives only.

## Definition of Done for Template
- A new developer can clone repo, set `.env`, run `docker compose up --build`, and access:
  - Web UI on `http://localhost:5173`
  - API on `http://localhost:8080`
  - Postgres on `localhost:5432`
- Core architecture paths (ingest -> persist -> analyse -> serve -> visualize) are scaffolded with clear extension points.
- Documentation covers local setup, environment variables, and future hosting handoff notes.
