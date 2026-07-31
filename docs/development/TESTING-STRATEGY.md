# Testing Strategy

## Testing pyramid

```text
      /\
     /  \   E2E (Playwright)
    /    \  ───────────────
   /      \ Integration (Testcontainers + API/DB)
  /        \────────────────
 /__________\ Unit (xUnit / Vitest / React Testing Library)
```

## Unit tests

### Backend

- **Scope**: domain entities, value objects, validators, pure business logic, mappers.
- **Framework**: xUnit + FluentAssertions.
- **Command**: `dotnet test --filter FullyQualifiedName~UnitTests`.
- **Target coverage**: ≥ 80% for domain layer, ≥ 90% for critical rules.

### Frontend

- **Scope**: utilities, hooks, small components.
- **Framework**: Vitest + React Testing Library.
- **Command**: `npm run test`.
- **Target coverage**: ≥ 70% for shared components and hooks.

## Integration tests

### Backend

- **Scope**: controllers, repositories, EF Core mappings, database queries, auth wiring.
- **Framework**: xUnit + WebApplicationFactory + Testcontainers PostgreSQL.
- **Command**: `dotnet test --filter FullyQualifiedName~IntegrationTests`.
- **Critical scenarios**:
  - Reservation conflict detection.
  - Cross-complex authorization.
  - Recurring reservation generation and cancellation.
  - Migration validation.

### Frontend

- **Scope**: component integration with mocked API and state.
- **Framework**: Vitest + React Testing Library + MSW (Mock Service Worker).
- **Command**: `npm run test`.

## Architecture tests

- **Scope**: layer dependencies, naming conventions, forbidden references.
- **Framework**: NetArchTest or TngTech.ArchUnitNET.
- **Command**: `dotnet test --filter FullyQualifiedName~ArchitectureTests`.

## End-to-end tests

- **Tool**: Playwright.
- **Command**: `npm run test:e2e`.
- **Critical scenarios**:
  - Login with Google (mocked in CI).
  - Complete profile.
  - Search complex and view availability.
  - Create and cancel a reservation.
  - Admin creates a court and sets availability.
  - Admin blocks a user.

## Concurrency tests

- Use parallel `HttpClient` requests to simulate race conditions.
- Assert exactly one reservation wins for a single slot.
- Run against a real PostgreSQL container to test transaction isolation.

## Authorization tests

- Generate tokens with different claims.
- Assert 401 for missing tokens, 403 for forbidden resources, and 200/201 for allowed ones.
- Verify that complex A admin cannot access complex B data.

## Test data

- Use builders or factory methods for entities.
- Reset state per test class or use transactions.
- Do not use production data.

## CI requirements

The pipeline must run all test suites before allowing merge:

```bash
dotnet test
npm run test
npm run test:e2e
```

## Coverage reporting

- Generate coverage reports with `dotnet test --collect:"XPlat Code Coverage"`.
- Upload to CI artifacts or a coverage service.
- Fail the build if critical path coverage falls below the target.
