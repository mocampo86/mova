# Testing Workflow

## Testing pyramid

```text
      /
     / \     E2E (Playwright)
    /   \    ───────────────
   /     \   Integration (Testcontainers + API/DB)
  /       \  ───────────────
 /_________\ Unit (xUnit, Vitest, React Testing Library)
```

## Backend tests

### Unit tests

- **Scope**: domain entities, value objects, validators, pure logic.
- **Location**: `tests/Mova.UnitTests`.
- **Command**: `dotnet test --filter FullyQualifiedName~UnitTests`.

### Integration tests

- **Scope**: API endpoints, repositories, EF Core mappings, PostgreSQL persistence.
- **Location**: `tests/Mova.IntegrationTests`.
- **Database**: PostgreSQL via Testcontainers.
- **Command**: `dotnet test --filter FullyQualifiedName~IntegrationTests`.

### Architecture tests

- **Scope**: layer dependencies, naming conventions, forbidden dependencies.
- **Location**: `tests/Mova.ArchitectureTests`.
- **Command**: `dotnet test --filter FullyQualifiedName~ArchitectureTests`.

### Concurrency tests

- **Scope**: reservation conflict detection under simultaneous requests.
- **Approach**: parallel tasks with `HttpClient` and `await Task.WhenAll`.
- **Validation**: exactly one reservation succeeds when the same slot is requested twice.

### Authorization tests

- **Scope**: cross-complex access, role enforcement, missing JWT.
- **Approach**: generate tokens with different claims and assert 401/403 responses.

## Frontend tests

### Unit tests

- **Scope**: utilities, hooks, small components.
- **Tools**: Vitest, React Testing Library.
- **Command**: `npm run test`.

### Component tests

- **Scope**: forms, modals, tables, calendar widgets.
- **Approach**: render with mocked API and user events.

### End-to-end tests

- **Scope**: critical user journeys.
- **Tools**: Playwright.
- **Scenarios**:
  - Login with Google (mocked in CI).
  - Complete user profile.
  - Search and view a complex.
  - Create and cancel a reservation.
  - Create a court as an admin.
  - Block a user.
- **Command**: `npm run test:e2e`.

## Running tests locally

```powershell
# Backend full suite
dotnet test

# Frontend unit tests
npm run test

# E2E
npm run test:e2e
```

## CI test execution

The pipeline must run:

1. `dotnet restore && dotnet build`.
2. `dotnet test`.
3. `npm ci && npm run lint && npm run build`.
4. `npm run test`.
5. `npm run test:e2e` against the local or preview deployment.

## Coverage expectations

- **Critical paths**: > 90% (reservation creation, cancellation, conflict detection, auth).
- **Domain layer**: > 80%.
- **UI components**: > 60% for shared components, > 70% for critical flows.
- **Infrastructure wiring**: tested through integration tests.

## Test data management

- Use builders or factories for test entities.
- Integration tests seed the database per test class or use transactions to reset state.
- Never use production data in tests.

## Bug regression

When a bug is fixed, add a test that fails before the fix and passes after it.
