# MVP Scope

## In scope

The MVP must allow validation of the core value proposition: users and complexes can manage reservations without phone calls or manual agendas.

### Included features

- **Public site**
  - Landing page.
  - Responsive design for mobile and desktop.

- **Authentication**
  - Google login.
  - User registration and profile completion.
  - Mandatory phone number.
  - Role-based authorization.

- **User portal**
  - Profile management.
  - Search complexes and courts.
  - View availability by date.
  - Create a single reservation.
  - Create a weekly recurring reservation.
  - View upcoming reservations and history.
  - Cancel reservations according to policy.
  - See if blocked by a complex.

- **Complex administration**
  - Create and edit complex information.
  - Create, edit, activate, and deactivate courts.
  - Assign one or more sports per court.
  - Configure business hours.
  - Configure court availability rules and slot duration.
  - View and manage reservations.
  - Create manual reservations.
  - Cancel reservations.
  - Block time slots for maintenance, events, holidays, etc.
  - Block and unblock users.
  - View basic customer history.

- **Platform operations**
  - Structured logging.
  - Global error handling.
  - Health checks.
  - Audit logging for administrative actions.
  - OpenAPI/Swagger documentation.

### Out of scope

- Online payments or deposits.
- SMS or WhatsApp phone verification.
- Native mobile apps.
- Push notifications.
- Chat between users and administrators.
- Coupons, promotions, or dynamic pricing.
- Tournaments, leagues, or championships.
- Reputation, comments, or ratings.
- Accounting or electronic invoicing.
- Commission-based marketplace.
- Access control or smart locks.

## MVP success metrics

- At least one complex registers and configures courts.
- At least one user completes registration and books a court.
- No conflicting reservations accepted by the system.
- Reservation flow completed on mobile in under two minutes.
