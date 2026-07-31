# ADR-0002: Shared PostgreSQL with Logical Multi-Tenancy

## Status

Accepted

## Context

The platform must support multiple sports complexes (tenants) from the beginning. We needed to decide between a shared database with logical isolation, a schema-per-tenant, or a database-per-tenant model.

## Decision

For the MVP, we will use a **single PostgreSQL database** with row-level tenant isolation through a `SportsComplexId` column on tenant-scoped tables.

## Consequences

### Positive

- Simpler provisioning, backups, and monitoring.
- Lower infrastructure cost.
- Easier schema migrations because there is only one database.
- Faster queries without cross-database joins.

### Negative

- All tenants share the same database resources.
- Data isolation relies on application and query filters rather than physical separation.
- A heavy tenant can impact others until scaling or isolation changes are implemented.
- Future extraction to database-per-tenant will require a data migration project.

## Alternatives considered

- **Schema-per-tenant**: rejected because it complicates migrations and connection management for the MVP.
- **Database-per-tenant**: rejected because it adds provisioning, backup, and monitoring overhead that is unnecessary for the initial phase.

## Security considerations

All queries scoped to a complex must validate the user's authorized `SportsComplexId`. We will implement a custom authorization handler and add indexes on the `SportsComplexId` columns.

## Related decisions

- ADR-0001: Modular Monolith.
- ADR-0003: Google OpenID Connect and JWT.

## References

- `mova-project-overview.md` section 10.
- `.ai-kit/docs/architecture/MULTI-TENANCY.md`.
