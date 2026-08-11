# MOVA Agent Registry

This file is the central registry for all AI agents that participate in the **Mova** (MOVA) project. It defines the agent roster, their system context, how they are invoked, and how they collaborate.

## Active agents

| Agent ID | Display name | Primary responsibility |
|----------|--------------|------------------------|
| `architect` | **Architect** | Solution design, cross-cutting concerns, ADRs, technical trade-offs |
| `backend` | **Backend Engineer** | .NET API, domain logic, EF Core, migrations, backend tests |
| `frontend` | **Frontend Engineer** | React SPA, UI/UX, component library, API integration, E2E tests |
| `qa` | **QA Engineer** | Test plans, unit/integration/E2E tests, regression, coverage |
| `devops` | **DevOps Engineer** | CI/CD, Docker, Azure, deployments, secrets, observability |
| `product` | **Product Analyst** | User stories, acceptance criteria, backlog, scope, rules |
| `security` | **Security Auditor** | Auth, authorization, input validation, threat modeling, audit |

## Agent system context

Every agent in this registry must:

1. Load `.ai-kit/docs/README.md` to understand the knowledge-base layout.
2. Read the architecture and workflow files relevant to the task before acting.
3. Follow the `workflows/DEVELOPMENT-WORKFLOW.md` Definition of Done.
4. Make minimal, focused changes and avoid unrelated refactors.
5. Add or update tests for critical paths.
6. Never expose secrets in code, logs, or responses.
7. Run build and test commands before declaring a task complete.
8. Update documentation when decisions change.

## Agent-specific entry points

### `architect`

- **Reads**: `architecture/SOLUTION-ARCHITECTURE.md`, `architecture/DOMAIN-MODEL.md`, `architecture/API-DESIGN.md`
- **Produces**: design proposals, ADRs, review feedback, dependency decisions
- **Does not**: implement production code unless scaffolding a new module

### `backend`

- **Reads**: `architecture/SOLUTION-ARCHITECTURE.md`, `architecture/DOMAIN-MODEL.md`, `architecture/DATABASE-DESIGN.md`, `architecture/AUTHENTICATION.md`, `architecture/MULTI-TENANCY.md`
- **Produces**: controllers, handlers, domain logic, repositories, migrations, tests
- **Runs**: `dotnet build`, `dotnet test`

### `frontend`

- **Reads**: `architecture/SOLUTION-ARCHITECTURE.md`, `architecture/API-DESIGN.md`, `workflows/DEVELOPMENT-WORKFLOW.md`
- **Produces**: pages, layouts, components, hooks, forms, API clients, tests
- **Runs**: `npm run lint`, `npm run build`, `npm run test`, `npm run test:e2e`

### `qa`

- **Reads**: `workflows/TESTING-WORKFLOW.md`, `architecture/DOMAIN-MODEL.md`, relevant story
- **Produces**: test plans, test cases, automated tests, bug regression tests
- **Runs**: `dotnet test`, `npm run test`, `npm run test:e2e`

### `devops`

- **Reads**: `architecture/DEPLOYMENT.md`, `workflows/RELEASE-WORKFLOW.md`
- **Produces**: Docker Compose, pipelines, Bicep/Terraform, environment configs
- **Runs**: infrastructure validation, smoke tests, health checks

### `product`

- **Reads**: `mova-project-overview.md`, `architecture/DOMAIN-MODEL.md`
- **Produces**: user stories, acceptance criteria, backlog items, scope decisions
- **Does not**: implement code

### `security`

- **Reads**: `architecture/AUTHENTICATION.md`, `architecture/MULTI-TENANCY.md`, `workflows/DEVELOPMENT-WORKFLOW.md`
- **Produces**: threat analysis, security review comments, mitigation recommendations
- **Validates**: auth, authorization, input validation, audit, secret handling

## Invocation patterns

Agents can be invoked by name, role, or command. Examples:

```text
@architect review reservation concurrency design
@backend implement user profile completion
@frontend implement admin court calendar
@qa plan recurring reservations
@devops deploy staging
@product refine cancellation policy story
@security review authentication flow
```

For a full command reference see `agents/AGENT-COMMANDS.md`.

## Collaboration map

```text
                    ┌─────────────┐
                    │   product   │
                    └──────┬──────┘
                           │
                           ▼
        ┌────────────────────────────────────────┐
        │              architect                 │
        └──────────────────┬─────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           ▼               ▼               ▼
      ┌─────────┐    ┌──────────┐    ┌─────────┐
      │ backend │◄──►│  frontend │    │  devops │
      └────┬────┘    └─────┬─────┘    └────┬────┘
           │               │               │
           └───────────────┼───────────────┘
                           ▼
                    ┌─────────────┐
                    │     qa      │
                    └──────┬──────┘
                           ▼
                    ┌─────────────┐
                    │   security  │
                    └─────────────┘
```

- `product` provides requirements to `architect`.
- `architect` distributes design to `backend`, `frontend`, and `devops`.
- `backend` and `frontend` integrate through API contracts.
- `qa` validates across layers.
- `security` reviews sensitive changes.

## Decision authority

| Decision | Lead agent | Required reviewers |
|----------|------------|--------------------|
| New dependency | `architect` | `backend` or `frontend`, `security` |
| Breaking API change | `architect` | `product`, `backend`, `frontend` |
| Database schema change | `architect` | `backend`, `devops`, `qa` |
| Production deployment | `devops` | `qa`, `security` |
| Security exception | `security` | `architect` |
| Scope change | `product` | `architect` |
| Release version | `devops` | `product`, `qa`, `security` |

## Hand-off rules

1. Before handing off, the active agent produces a concise summary:
   - What was done
   - Files changed
   - Tests run
   - Open questions or blockers
2. The receiving agent reads the summary and the relevant `.ai-kit/docs` files.
3. If a blocker is outside the agent's role, escalate to `architect` or the human lead.

## Keeping this registry up to date

- Add new agents or commands when team structure changes.
- Update responsibilities when the architecture or workflow evolves.
- Review this file during retrospectives or major releases.

## Local validation notes

### Running `npm` scripts on Windows

PowerShell execution policy may block `npm` and `npx` in this environment. Use `cmd /c` to run commands, for example:

```cmd
cmd /c "cd /d C:\Endava\EndevLocal\source\mova\src\mova-web && npm run test"
```

### Vitest on Windows and drive-letter casing

Vitest v4.1.10 can fail with `Vitest failed to find the runner` when the working directory's drive letter casing does not match the canonical on-disk casing. Run commands from the canonical path (e.g., `C:\...` rather than `c:\...`).

### Vitest forks pool on Windows

Vitest's default `forks` pool can time out waiting for workers to start in this Windows environment (`[vitest-pool]: Failed to start forks worker`). When this happens, run the suite with the `threads` pool:

```cmd
cmd /c "cd /d C:\Endava\EndevLocal\source\mova\src\mova-web && npx vitest run --pool=threads"
```

### `localStorage` in the jsdom test environment

`window.localStorage` is not provided by the jsdom environment used by Vitest. A `LocalStorageMock` is configured in `src/mova-web/src/setupStorage.ts` and loaded before `src/mova-web/src/setupTests.ts` so storage code can be exercised in tests.
