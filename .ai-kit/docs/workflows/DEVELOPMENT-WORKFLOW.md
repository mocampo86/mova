# Development Workflow

## Branching strategy

Use a **trunk-based** or **GitHub Flow** model:

- `main` is the source of truth and deployable branch.
- Short-lived feature branches from `main`: `feature/<id>-short-name`.
- Bug fix branches: `fix/<id>-short-name`.
- Hot fix branches: `hotfix/<id>-short-name`.

## Starting a task

1. Read the relevant `.ai-kit/docs` files.
2. Create a branch from `main`.
3. Ensure local environment is running (`docker compose up` for PostgreSQL).
4. Run existing tests to establish a green baseline.

## User story template

Every story should live in `docs/backlog/` or an issue tracker and contain:

```markdown
# <Story-ID> — Title

## Objective
What and why.

## Functional description
What the user/admin/system does.

## Acceptance criteria
- [ ] Criteria 1
- [ ] Criteria 2

## Business rules
Rules that must be enforced.

## Validations
Input validation, authorization, edge cases.

## Error cases
Expected error codes and messages.

## Security considerations
Roles, scopes, audit, secrets.

## Technical notes
Affected layers, endpoints, components.

## Tests required
- Unit tests
- Integration tests
- E2E tests

## Out of scope
Explicit exclusions.

## Dependencies
Other stories, APIs, or environments.

## Definition of Done
- Code compiles
- Tests pass
- Documentation updated
- PR reviewed and merged
```

## Coding standards

### Backend

- Use Clean Architecture layers: Domain, Application, Infrastructure, API, Contracts.
- Controllers are thin; business logic lives in Application handlers and Domain entities.
- Use FluentValidation for input validation.
- Use EF Core migrations for schema changes.
- Use `Result<T>` or problem-details for error responses.
- No domain logic in controllers or repositories.

### Frontend

- Mobile-first, responsive design.
- Use feature folders inside `src/features/`.
- API calls through TanStack Query with consistent hook patterns.
- Forms validated with Zod and React Hook Form.
- Components are typed and reusable.
- Avoid prop drilling; prefer context or state management only when needed.

## Definition of Done

A task is complete when:

- [ ] All acceptance criteria are met.
- [ ] Backend compiles and all tests pass.
- [ ] Frontend builds and all tests pass.
- [ ] New tests added for critical paths.
- [ ] No secrets or sensitive data exposed.
- [ ] Migrations created and validated if schema changed.
- [ ] Documentation updated (`README`, `.ai-kit/docs`, ADRs).
- [ ] Manual validation completed (local or staging).
- [ ] Code review approved.
- [ ] CI pipeline is green.

## Local validation commands

```powershell
# Backend
cd src/ReservaCanchas.Api
dotnet build
dotnet test

# Frontend
cd src/reservacanchas-web
npm ci
npm run lint
npm run build
npm run test
```

## Continuous Integration

CI runs on every pull request targeting `main` or `develop` via GitHub Actions (`.github/workflows/ci.yml`):

- A `detect-changes` job uses `dorny/paths-filter` to determine whether backend or frontend files changed.
- The `backend` job (if backend files changed) restores, builds, and runs `dotnet test` against the solution, with a PostgreSQL service container available for integration tests.
- The `frontend` job (if frontend files changed) runs `npm ci`, `npm run lint`, `npm run build`, and `npm run test` in `src/reservacanchas-web`.
- `backend` and `frontend` run in parallel; both must pass for the pipeline to succeed.
- Third-party actions are pinned to a specific commit SHA to reduce supply-chain risk.
- No pull request should be merged until the pipeline is green.

## Commit conventions

Use conventional commits:

```text
feat: add court availability rule
fix: prevent overlapping reservations
test: add concurrency tests for reservations
docs: update API design for pagination
refactor: extract reservation conflict validator
chore: update docker compose postgres version
```

## Pull request requirements

- Link to the story or issue.
- Include a summary of changes and any architectural decisions.
- Include test results or CI links.
- Request review from at least one peer.
- Resolve all comments before merging.
