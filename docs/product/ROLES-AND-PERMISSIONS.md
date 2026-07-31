# Roles and Permissions

## User roles

### 1. User (End User)

**Permissions**

- Register and log in with Google.
- Complete and edit profile.
- Search active complexes and courts.
- View availability.
- Create and cancel own reservations.
- View own upcoming reservations and history.
- View whether they are blocked in a specific complex.

**Restrictions**

- Cannot access admin data of other users or complexes.
- Cannot create reservations while blocked in a complex.
- Must complete phone number before booking.

### 2. Complex Administrator

**Permissions within their complex(es)**

- Create, edit, activate, and deactivate their complex.
- Create, edit, activate, and deactivate courts.
- Assign sports to courts.
- Configure business hours and availability rules.
- View all reservations of the complex.
- Create reservations manually.
- Cancel reservations of the complex.
- Block and unblock time slots on a court.
- Block and unblock users of the complex.
- View customer history for their complex.

**Restrictions**

- Cannot access complexes they do not administer unless explicitly granted.
- Cannot modify global catalog data (e.g. `Sport` list) if restricted by super admin.
- Cannot delete data permanently; deactivation is used for soft removal.

### 3. Super Administrator

**Permissions**

- List, activate, and deactivate any complex.
- Manage complex administrators and responsibilities.
- Resolve operational incidents.
- View global metrics.
- Access global audit log.
- Access platform-level configuration.

**Restrictions**

- Should not access individual user passwords because authentication is delegated to Google.
- Must follow audit and change management practices.

## Permission matrix

| Action | User | Complex Admin | Super Admin |
|--------|------|---------------|-------------|
| View public complexes/courts | Yes | Yes | Yes |
| Make a reservation | Yes | Yes | Yes |
| Cancel own reservation | Yes | Yes | Yes |
| Manage complex profile | No | Own complex | Any complex |
| Manage courts | No | Own complex | Any complex |
| Manage availability rules | No | Own complex | Any complex |
| View complex reservations | No | Own complex | Any complex |
| Cancel any reservation in complex | No | Own complex | Any complex |
| Block time slots | No | Own complex | Any complex |
| Block users | No | Own complex | Any complex |
| View global audit log | No | No | Yes |
| Activate/deactivate complex | No | No | Yes |

## Authorization implementation

- Authentication via Google OpenID Connect + JWT.
- Role claims in JWT (`User`, `ComplexAdmin`, `SuperAdmin`).
- Complex-scoped access resolved from `ComplexAdministrators` table.
- Resource-based authorization ensures `SportsComplexId` matches the user's allowed complexes.
