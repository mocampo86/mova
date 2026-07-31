# Multi-Tenancy Strategy

## Strategy: shared database with logical isolation

For the MVP the platform uses a **single PostgreSQL database** and isolates tenants (sports complexes) at the row level through `SportsComplexId` foreign keys and query filters.

This decision keeps infrastructure simple, reduces operational cost, and simplifies backups and migrations. A database-per-tenant model is intentionally deferred until there is evidence of scale or regulatory need.

## Tenant data model

Every tenant-scoped entity carries a `SportsComplexId` column:

- `Courts`
- `CourtAvailabilityRules`
- `Reservations`
- `RecurringReservations`
- `CourtBlocks`
- `BlockedUsers`
- `BusinessHours`
- `ComplexAdministrators`
- `AuditLogs`

Entities shared by all tenants do **not** require `SportsComplexId`:

- `Users`
- `Sports`

## Query filtering

All administrative and operational queries must filter by `SportsComplexId`. This is enforced by:

1. **Application-layer validation** of the user's right to access the complex.
2. **Repository-level query filters** using EF Core `HasQueryFilter` where appropriate.
3. **Database indexes** on `SportsComplexId` for performance.

### EF Core global query filter example

```csharp
builder.Entity<Court>()
    .HasQueryFilter(c => c.Status != "Deleted");
```

Use per-request filters for `SportsComplexId` rather than static global filters to avoid accidentally hiding data in super-admin queries.

## Authorization enforcement

A custom `IAuthorizationRequirement` and handler verify that the current user belongs to the requested complex:

```csharp
public class ComplexAdminRequirement : IAuthorizationRequirement
{
    public Guid SportsComplexId { get; }
    // handler resolves user's associations and compares
}
```

## Preventing cross-tenant access

- Reject any request where the `SportsComplexId` in route/body does not match one of the user's allowed complexes, unless the user is `SuperAdmin`.
- Never trust the `SportsComplexId` sent by the frontend for privileged operations; derive it from the authenticated user's associations.
- For public endpoints, allow reading only active complexes and their public fields.

## Tenant lifecycle

### Creation

1. `SuperAdmin` or onboarding flow creates the `SportsComplex`.
2. The first `ComplexAdministrator` is linked to the complex.
3. Default `BusinessHours` may be seeded.

### Activation / deactivation

- Deactivating a complex sets `Status` to `Inactive`.
- The frontend stops listing inactive complexes.
- Existing reservations are preserved for history but new reservations are blocked.

### Deletion

Hard deletion of a complex is not supported in the MVP. Deactivation serves as soft deletion.

## Tenant-aware testing

Integration tests must verify that:

- An admin of complex A cannot read or modify complex B data.
- A `User` can only see their own reservations across complexes.
- A public visitor can only search active complexes and public fields.

## Future evolution

If the platform grows to hundreds of complexes or requires per-complex data sovereignty, evaluate:

- Schema-per-tenant.
- Database-per-tenant.
- A separate tenant metadata service.

These changes are significantly more complex and require a migration plan for existing data.
