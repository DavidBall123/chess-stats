# Contributing

## Branching
- Do not commit directly to `main`.
- Create a short-lived feature branch from `main`.
- Branch naming examples:
  - `feat/<topic>`
  - `fix/<topic>`
  - `chore/<topic>`

## Pull Requests
- Keep PRs scoped to one change theme.
- Include summary, test notes, and any environment/config impact.
- Prefer small, reviewable commits.

## Commit Messages
Use imperative tense and concise subjects.

Examples:
- `Add worker ingestion scaffold`
- `Fix API opening stats query`
- `Document Docker Desktop setup for WSL`

## Local Validation
Before opening a PR:
1. Ensure `docker compose -f infra/docker-compose.yml up --build` works.
2. Confirm no secrets are committed (`.env` must stay untracked).
3. Update docs if behavior or setup changed.
