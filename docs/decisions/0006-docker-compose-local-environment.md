# ADR-0006: Docker Compose for Local Development

## Status

Accepted

## Context

Developers need a reliable local environment that includes PostgreSQL and matches production as closely as possible. We needed a simple way to run dependencies locally.

## Decision

We will use **Docker Compose** to run PostgreSQL locally. The API and frontend will run directly on the developer machine via `dotnet run` and `npm run dev`.

## Consequences

### Positive

- PostgreSQL version matches the production target.
- Developers can reset state by recreating the container.
- No need to install PostgreSQL locally on each machine.
- Easy to add Redis or other dependencies later by extending the compose file.

### Negative

- Developers must have Docker installed.
- Persistent data requires a named volume; accidental volume removal can erase local data.
- The API and web processes are not containerized locally, so the local setup differs slightly from production containers.

## Implementation notes

- Provide a `docker-compose.yml` with a PostgreSQL service.
- Use `.env` or user secrets for connection strings and credentials.
- Document how to start, stop, and reset the local database in `README.md`.

## Alternatives considered

- **Local PostgreSQL installation**: rejected because it complicates onboarding and version alignment.
- **Full Docker Compose including API and web**: deferred to keep hot reload and debugger support simple for the MVP.

## Related decisions

- ADR-0002: Shared PostgreSQL with Logical Multi-Tenancy.

## References

- `mova-project-overview.md` section 8.4.
- `.ai-kit/docs/architecture/DEPLOYMENT.md`.
