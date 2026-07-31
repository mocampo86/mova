# EPIC-10 — Audit and Operations

## Objective

Provide operational visibility, audit trails, monitoring, and basic maintenance tooling so the platform can be run reliably and securely.

## Scope

- Audit log for administrative actions.
- Error monitoring and logging.
- Health checks and metrics.
- Basic operational documentation.
- Backup and recovery documentation.
- Rate limiting preparation on sensitive endpoints.

## User stories

| ID | Story |
|----|-------|
| US-054 | As a super administrator, I want to see an audit log of administrative actions. |
| US-055 | As an operator, I want to be alerted when error rates exceed a threshold. |
| US-056 | As an operator, I want health checks to report the status of the API and database. |
| US-057 | As a team, I want operational runbooks for common procedures. |
| US-058 | As a security owner, I want rate limiting on login and reservation endpoints. |

## Acceptance criteria

- [ ] Administrative mutations (create/update complex, court, block, cancellation) are recorded in `AuditLog`.
- [ ] Logs are structured and contain correlation IDs.
- [ ] `/health` and `/health/ready` endpoints exist and are used by the deployment.
- [ ] Application Insights or equivalent observability tool is configured.
- [ ] Sensitive endpoints (login, reservation creation) have rate limiting configured.
- [ ] Backup and restore procedures are documented.

## Dependencies

- EPIC-01 — Technical Foundation.
- EPIC-02 — Identity and Access.
- EPIC-06 — Reservations (for audit targets).

## Technical notes

- `AuditLog` entity: `Id`, `UserId`, `SportsComplexId`, `Action`, `EntityType`, `EntityId`, `CreatedAt`, `Metadata`.
- Use Serilog with correlation IDs propagated via `HttpContext`/`TraceIdentifier`.
- Application Insights for Azure; equivalent open-source alternatives for local/non-Azure environments.
- Rate limiting can be implemented with ASP.NET Core `RateLimiting` middleware.
- Keep audit logs immutable; do not update or delete audit records.
