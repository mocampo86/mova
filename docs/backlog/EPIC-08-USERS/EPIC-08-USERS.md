# EPIC-08 — User Management and Blocking

## Objective

Allow complex administrators to manage users of their complex, view reservation history, and block or unblock users with a recorded reason.

## Scope

- Search users of a complex.
- View user profile and reservation history.
- Block a user for a complex.
- Unblock a user.
- Optional expiration date for a block.
- Prevent blocked users from creating new reservations.
- Display block status to the affected user.

## User stories

| ID | Story |
|----|-------|
| [US-042](US-042.md) | As an administrator, I want to search users who have made reservations in my complex. |
| [US-043](US-043.md) | As an administrator, I want to view a user's reservation history in my complex. |
| [US-044](US-044.md) | As an administrator, I want to block a user so they cannot make new reservations. |
| [US-045](US-045.md) | As an administrator, I want to unblock a user if the situation is resolved. |
| [US-046](US-046.md) | As an administrator, I want to set an optional expiration date for a block. |
| [US-047](US-047.md) | As a user, I want to see if I am blocked in a complex. |

## Acceptance criteria

- [ ] Blocks are scoped to a single complex; a user blocked in complex A can still use complex B.
- [ ] A blocked user cannot create reservations in the complex where they are blocked.
- [ ] Existing reservations are not automatically cancelled by a block (may be added later).
- [ ] Block records include user, complex, reason, blocked-by, blocked-at, and optional expiration.
- [ ] Expired blocks are treated as lifted.

## Dependencies

- EPIC-02 — Identity and Access.
- EPIC-06 — Reservations.

## Technical notes

- `BlockedUser` entity: `Id`, `SportsComplexId`, `UserId`, `Reason`, `BlockedAt`, `BlockedUntil`, `BlockedByUserId`, `Status`.
- Validation must check active blocks before allowing a reservation to be created.
- A unique partial index or filter helps prevent duplicate active blocks per `(SportsComplexId, UserId)`.
