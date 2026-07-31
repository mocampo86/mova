# EPIC-05 — Public Availability and Discovery

## Objective

Enable visitors and logged-in users to discover sports complexes, view courts, filter by sport, and consult available time slots for a specific date.

## Scope

- Public landing page.
- List and detail views for complexes.
- List courts of a complex.
- Filter courts by sport.
- Availability query by date and court.
- Display upcoming available slots.

## User stories

| ID | Story |
|----|-------|
| US-023 | As a visitor, I want to see a landing page with the platform value proposition. |
| US-024 | As a user, I want to search and list active sports complexes. |
| US-025 | As a user, I want to view the details of a complex. |
| US-026 | As a user, I want to list the courts of a complex. |
| US-027 | As a user, I want to filter courts by sport. |
| US-028 | As a user, I want to see available time slots for a court on a selected date. |

## Acceptance criteria

- [ ] Public endpoints do not expose internal/admin data.
- [ ] Only active complexes and courts are listed.
- [ ] Availability query considers `CourtAvailabilityRule`, `BusinessHours`, existing reservations, and `CourtBlock`.
- [ ] Slots are returned with local time conversion handled by the frontend.
- [ ] Response time for availability query is acceptable for mobile users.

## Dependencies

- EPIC-03 — Sports Complex Administration.
- EPIC-04 — Court Administration.

## Technical notes

- Availability algorithm:
  1. Take the court's active `CourtAvailabilityRule` for the requested day of week.
  2. Split the range into slots of `SlotDurationMinutes`.
  3. Remove slots that overlap with active reservations or `CourtBlock` records.
  4. Remove slots outside the complex `BusinessHours` for that day.
- API: `GET /api/v1/complexes/{complexId}/availability?courtId=&date=`.
- All times stored in UTC; frontend converts to local time.
