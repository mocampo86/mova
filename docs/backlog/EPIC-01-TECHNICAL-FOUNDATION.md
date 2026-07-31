# EPIC-01 — Technical Foundation

## Objective

Set up the backend, frontend, database, and CI/CD foundations so the team can build features on a stable, observable, and testable base.

## Scope

- Backend solution and project structure.
- Frontend application scaffold.
- PostgreSQL local setup with Docker Compose.
- Environment configuration and secrets handling.
- Global error handling, structured logging, and health checks.
- CI pipeline for build and test.

## User stories

| ID | Story |
|----|-------|
| US-001 | As a developer, I want a runnable .NET solution so I can implement API features. |
| US-002 | As a developer, I want a React application scaffold so I can implement UI features. |
| US-003 | As a developer, I want PostgreSQL running locally via Docker Compose so I can develop and test persistently. |
| US-004 | As a developer, I want environment-based configuration and secret management so production credentials are never committed. |
| US-005 | As an operator, I want structured logging and global exception handling so I can diagnose issues. |
| US-006 | As an operator, I want health checks exposed so orchestrators can monitor the API. |
| US-007 | As a team, I want a CI pipeline that builds and tests the solution on every PR. |

## Acceptance criteria

- [ ] `dotnet build` succeeds for all backend projects.
- [ ] `npm run build` succeeds for the frontend.
- [ ] `docker compose up` starts PostgreSQL.
- [ ] API health endpoints return healthy status.
- [ ] CI runs build and tests on pull requests.
- [ ] No secrets are stored in source control.

## Dependencies

None. This is the first epic.

## Technical notes

- Backend projects: `Api`, `Application`, `Domain`, `Infrastructure`, `Contracts`.
- Tests projects: `UnitTests`, `IntegrationTests`, `ArchitectureTests`.
- Frontend: Vite + React + TypeScript + MUI + TanStack Query + React Router + React Hook Form + Zod.
- Logging: Serilog for backend; structured console logs for frontend.
- CI: GitHub Actions or Azure DevOps.
