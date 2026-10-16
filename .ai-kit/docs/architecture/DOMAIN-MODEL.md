# Domain Model

## Bounded contexts

The domain is organized around the following bounded contexts:

| Context | Responsibility |
|---------|---------------|
| Identity | User registration, profiles, authentication primitives, blocked-user status per complex |
| Complex Management | Sports complexes, administrators, business hours |
| Court Management | Courts, sports, availability rules |
| Reservations | Single and recurring reservations, availability, conflict detection |
| Scheduling | Blocked time slots, calendar aggregation |
| Audit | Domain-level audit trail for administrative actions |

## Core entities

### User

A person registered in the platform. Authentication is delegated to Google; the platform stores identity and profile data.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | Platform user identifier |
| GoogleSubjectId | string | Google sub claim |
| Email | string | Unique, from Google profile |
| FullName | string | Editable by user |
| PhoneNumber | string | Mandatory for registration completion |
| PhoneVerified | bool | Prepared for future SMS/WhatsApp verification |
| Status | enum | Active / Blocked globally |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime? | UTC |

### SportsComplex

A sports facility that owns courts and reservations.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| Name | string | Public name |
| Description | string? | |
| Address | string | |
| City | string | |
| Latitude | decimal? | |
| Longitude | decimal? | |
| PhoneNumber | string? | |
| Email | string? | |
| Status | enum | Active / Inactive |
| AllowUserRecurringReservations | bool | Default `true`; controls regular user recurring bookings |
| TimeZoneId | string? | IANA time zone identifier (e.g. `America/Montevideo`); `null` for unresolved legacy complexes |
| Currency | string | ISO 4217 currency code (e.g. `UYU`); applies to all court rates and reservations of the complex |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime? | UTC |

### ComplexAdministrator

Links a user to a complex with an administrative role.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| SportsComplexId | Guid | |
| UserId | Guid | |
| Role | enum | Admin, Manager, etc. |
| Status | enum | Active / Inactive |
| CreatedAt | DateTime | UTC |

### Sport

Catalog of sports supported by the platform.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| Name | string | Football, padel, tennis, etc. |
| Status | enum | Active / Inactive |

### Court

A physical or logical court inside a complex.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| SportsComplexId | Guid | Multi-tenancy key |
| Name | string | |
| Description | string? | |
| SurfaceType | string? | Grass, synthetic, concrete, etc. |
| Indoor | bool? | |
| Status | enum | Active / Inactive |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime? | UTC |

### CourtSport

Many-to-many relationship between `Court` and `Sport`.

| Field | Type |
|-------|------|
| CourtId | Guid |
| SportId | Guid |

### BusinessHours

Opening hours of a complex by day of week.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| SportsComplexId | Guid | |
| DayOfWeek | int | 0 = Sunday |
| OpeningTime | TimeSpan | |
| ClosingTime | TimeSpan | May be earlier than OpeningTime to represent overnight hours |
| IsClosed | bool | |

### CourtAvailabilityRule

Defines when a court is available and how long each slot lasts.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| CourtId | Guid | |
| DayOfWeek | int | |
| StartTime | TimeSpan | May be later than EndTime to represent overnight ranges |
| EndTime | TimeSpan | |
| SlotDurationMinutes | int | Default 60 |
| IsActive | bool | |

### CourtRate

A configurable hourly price for a court, valid on selected weekdays and a same-day time range. Times are interpreted in the complex's `TimeZoneId`; the currency is inherited from the complex.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| CourtId | Guid | |
| DaysOfWeek | int | Bitmask of applicable days (bit 0 = Sunday … bit 6 = Saturday) |
| StartTime | TimeSpan | Within the day (00:00–24:00) |
| EndTime | TimeSpan | Greater than StartTime, at most 24:00; overnight ranges are not allowed |
| AmountPerHour | decimal | Positive, at most two decimal places |
| IsActive | bool | Inactive rates do not apply to new reservations |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime? | UTC |

Active rates for the same court cannot overlap on a shared weekday.

### ReservationPriceLine

Immutable snapshot of one priced segment of a reservation.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| ReservationId | Guid | |
| CourtRateId | Guid? | Rate applied; kept as reference only (rate changes do not affect the snapshot) |
| StartAt | DateTime | UTC segment start |
| EndAt | DateTime | UTC segment end |
| HourlyRate | decimal | Rate applied per hour |
| Amount | decimal | Proportional segment amount, rounded to 2 decimals (`AwayFromZero`) |

### Reservation

A concrete booking of a court for a time range.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| SportsComplexId | Guid | Multi-tenancy key |
| CourtId | Guid | |
| UserId | Guid | |
| StartAt | DateTime | UTC |
| EndAt | DateTime | UTC |
| Status | enum | Pending, Confirmed, CancelledByUser, CancelledByAdmin, Completed, NoShow |
| Source | enum | Web, Admin, Recurring |
| RecurringReservationId | Guid? | Nullable for series origin |
| Notes | string? | |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime? | UTC |
| CancelledAt | DateTime? | UTC |
| CancellationReason | string? | |
| CancelledByUserId | Guid? | Actor who performed the cancellation |
| TotalPrice | decimal? | Agreed total; `null` for reservations created before pricing existed |
| Currency | string? | ISO 4217 code snapshot from the complex; `null` for legacy reservations |
| PriceLines | collection | `ReservationPriceLine` snapshot of the applied segments |

### RecurringReservation

A rule that generates individual reservations on a weekly basis.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| SportsComplexId | Guid | |
| CourtId | Guid | |
| UserId | Guid | |
| DayOfWeek | int | |
| StartTime | TimeSpan | |
| DurationMinutes | int | |
| StartDate | DateOnly | First occurrence |
| EndDate | DateOnly | Last occurrence (must be defined) |
| Status | enum | Active / Cancelled |
| CreatedAt | DateTime | UTC |

### CourtBlock

A manual block that prevents reservations on a court for a period.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| SportsComplexId | Guid | |
| CourtId | Guid | |
| StartAt | DateTime | UTC |
| EndAt | DateTime | UTC |
| Reason | string? | Maintenance, event, internal, holiday, other |
| CreatedByUserId | Guid | |
| CreatedAt | DateTime | UTC |

### BlockedUser

A user blocked by a complex administrator.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| SportsComplexId | Guid | |
| UserId | Guid | |
| Reason | string? | |
| BlockedAt | DateTime | UTC |
| BlockedUntil | DateTime? | Optional expiration |
| BlockedByUserId | Guid | |
| Status | enum | Active / Lifted |

### AuditLog

Record of administrative actions.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| UserId | Guid? | Actor |
| SportsComplexId | Guid? | Scope |
| Action | string | e.g. `Court.Create` |
| EntityType | string | e.g. `Court` |
| EntityId | string | |
| CreatedAt | DateTime | UTC |
| Metadata | string? | JSON payload |

## Aggregate roots

| Aggregate | Root entity | Invariants |
|-----------|-------------|------------|
| User | User | Email uniqueness, phone validation, status |
| SportsComplex | SportsComplex | One or more administrators must be linkable; regular user recurring reservations can be toggled |
| Court | Court | Belongs to exactly one complex; has at least one sport when active |
| Reservation | Reservation | No overlapping active reservations on the same court |
| RecurringReservation | RecurringReservation | Generated occurrences cannot overlap existing active reservations |

## Value objects

- **TimeRange**: `Start` and `End` validated so that `Start < End`.
- **PhoneNumber**: normalized value object with country/region prefix awareness prepared for future validation.
- **Address**: street, city, optional coordinates.

## Domain events (when required)

- `ReservationCreated`
- `ReservationCancelled`
- `UserBlocked`
- `UserUnblocked`
- `RecurringReservationGenerated`

## Business rules

1. A court may belong to one or more sports.
2. A reservation cannot overlap with another active reservation on the same court.
3. A reservation cannot overlap with a `CourtBlock` in the same court.
4. A blocked user cannot create reservations in the blocking complex.
5. A recurring reservation must define an end date and a maximum number of weeks.
6. Cancellation must be allowed only before a configurable minimum notice period, and can be disabled entirely.
7. All date/time values are stored in UTC and converted to local time in the frontend using the sports complex's configured IANA time zone.
8. Every new reservation must be fully covered by active `CourtRate`s; an uncovered interval rejects the reservation (`PRICE_NOT_CONFIGURED`) — a missing rate is never free.
9. Reservation price = sum of proportional segments (`hourlyRate × minutes / 60`), each segment rounded to 2 decimals `AwayFromZero`; the sum of `PriceLines` always equals `TotalPrice`.
10. The client sends `expectedTotalPrice`/`expectedCurrency`; if the current calculation differs, confirmation is rejected (`PRICE_CHANGED`).
11. Recurring reservations price each occurrence independently; if any occurrence lacks coverage the whole series is rejected.

## Future domain extensions

- Payments and deposits.
- Notifications (email/SMS/WhatsApp).
- Reputation and comments.
