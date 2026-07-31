# Development Guidelines

## General principles

- Write clean, maintainable code aligned with Clean Architecture.
- Prefer explicit and readable code over clever shortcuts.
- Keep changes small and focused; one branch per user story or bug.
- Never commit secrets, credentials, or production connection strings.
- Update tests and documentation together with code changes.

## Backend conventions

### Project structure

```text
src/
├── ReservaCanchas.Api
├── ReservaCanchas.Application
├── ReservaCanchas.Domain
├── ReservaCanchas.Infrastructure
└── ReservaCanchas.Contracts
```

### Layer rules

- **Domain** has no dependencies on other layers.
- **Application** depends only on Domain and Contracts.
- **Infrastructure** depends on Application and Domain.
- **Api** depends on Application and Infrastructure and Contracts.
- **Contracts** has no project references except base libraries.

### Coding standards

- Use `async`/`await` for I/O-bound operations.
- Prefer immutable domain objects where practical.
- Use `CancellationToken` in all async public APIs.
- Validate input with FluentValidation and return problem details.
- Return `IActionResult` or `Results<T>` from endpoints, not raw entities.
- Use `IHostedService` or minimal-background jobs only when necessary.

### Naming

- Projects: `PascalCase`, e.g. `ReservaCanchas.Application`.
- Folders: `PascalCase` for namespaces; lowercase for non-code assets.
- Classes: `PascalCase` nouns.
- Interfaces: `PascalCase` prefixed with `I`.
- Methods: `PascalCase` verbs or verb phrases.
- Private fields: `_camelCase`.
- Constants: `PascalCase`.

## Frontend conventions

### Project structure

```text
src/
├── app/
├── components/
├── features/
├── hooks/
├── layouts/
├── pages/
├── services/
└── shared/
```

### Coding standards

- Use TypeScript strict mode.
- Use functional components and React hooks.
- Keep components focused; extract reusable UI into `components/`.
- Colocate feature logic under `features/<name>/`.
- Use TanStack Query for server state.
- Use React Hook Form + Zod for forms.
- Use Material UI theming and responsive breakpoints.
- Write tests with Vitest and React Testing Library.

### Naming

- Components: `PascalCase`.
- Hooks: `camelCase` prefixed with `use`.
- Feature folders: `kebab-case`.
- API client functions: `camelCase` action + resource, e.g. `getReservations`, `createReservation`.

## Pull requests

- Keep PRs focused; split large changes into stacked PRs.
- Write a clear description with context and test commands.
- Link to the user story or issue.
- Resolve all review comments before merging.
- Squash and merge is the default strategy.

## Code review

Reviewers check:

- Architecture alignment.
- Test coverage for critical paths.
- Security (auth, authorization, input validation, secrets).
- Performance and database query efficiency.
- Accessibility and responsive behavior for frontend changes.
- Proper migration and rollback for schema changes.

## Documentation

- Update `.ai-kit/docs` or `docs/` when decisions change.
- Add XML comments or JSDoc for public APIs.
- Document non-obvious business logic and invariants.
