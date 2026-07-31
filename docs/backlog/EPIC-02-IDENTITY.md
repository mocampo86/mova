# EPIC-02 — Identity and Access

## Objective

Allow users to register and log in with Google, complete their profile, and enforce role-based access for users, complex administrators, and super administrators.

## Scope

- Google OpenID Connect integration.
- JWT issuance and validation.
- User profile completion with mandatory phone number.
- Roles and authorization policies.
- Protected routes and endpoints.
- Preparation for future phone verification.

## User stories

| ID | Story |
|----|-------|
| US-008 | As a user, I want to log in with Google so I can access the platform securely. |
| US-009 | As a user, I want to complete my profile with my phone number so I can make reservations. |
| US-010 | As an administrator, I want the system to distinguish user, complex admin, and super admin roles so features are protected. |
| US-011 | As a developer, I want protected routes in the React app so unauthorized users cannot access admin features. |
| US-012 | As a developer, I want authorized API endpoints so only allowed actors can perform operations. |

## Acceptance criteria

- [ ] Users can log in via Google and receive a valid JWT.
- [ ] The JWT contains user id, email, name, and roles.
- [ ] Incomplete profiles are redirected to `/complete-profile`.
- [ ] Phone number is validated for format and stored.
- [ ] `User`, `ComplexAdmin`, and `SuperAdmin` roles are enforced in UI and API.
- [ ] Complex-scoped endpoints reject admins from other complexes.

## Dependencies

- EPIC-01 — Technical Foundation.

## Technical notes

- Use ASP.NET Core `AddJwtBearer` and Google authentication.
- Store `GoogleSubjectId`, `Email`, `FullName`, `PhoneNumber`, and `PhoneVerified`.
- Custom `IAuthorizationRequirement` for complex-scoped access.
- Frontend stores access token in memory and refresh token in `HttpOnly` cookie if implemented.
