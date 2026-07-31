# EPIC-03 — Sports Complex Administration

## Objective

Allow administrators to register and manage sports complexes, their public information, location, and responsible users.

## Scope

- CRUD for `SportsComplex`.
- Complex activation/deactivation.
- Public information configuration.
- Address and optional coordinates.
- Linking the first administrator to a complex.
- Superadmin moderation (preparation).

## User stories

| ID | Story |
|----|-------|
| [US-013](US-013.md) | As an administrator, I want to create a complex with name, description, address, and contact data. |
| [US-014](US-014.md) | As an administrator, I want to edit my complex information so it is always up to date. |
| [US-015](US-015.md) | As an administrator, I want to activate or deactivate my complex so I can control public visibility. |
| [US-016](US-016.md) | As a super administrator, I want to see all complexes and activate/deactivate them if necessary. |
| [US-017](US-017.md) | As a system, I want to assign the first administrator to a complex automatically. |

## Acceptance criteria

- [ ] A logged-in user can create a complex and becomes its first administrator.
- [ ] Complex data can be edited by an administrator of that complex.
- [ ] Inactive complexes are not listed publicly.
- [ ] `CreatedAt` and `UpdatedAt` are tracked.
- [ ] Superadmins can list and manage all complexes.

## Dependencies

- EPIC-02 — Identity and Access.

## Technical notes

- `SportsComplex` entity: `Id`, `Name`, `Description`, `Address`, `City`, `Latitude`, `Longitude`, `PhoneNumber`, `Email`, `Status`, `CreatedAt`, `UpdatedAt`.
- `ComplexAdministrator` links user and complex with a role.
- Public listing returns only active complexes and public fields.
