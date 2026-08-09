# Mova Web

React frontend for the Mova application.

## Environment variables

Create a `.env` file in this directory based on the following variables:

| Variable | Description | Default |
| --- | --- | --- |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth 2.0 client ID for Google Sign-In. | Required |
| `VITE_API_BASE_URL` | Base URL of the Mova API. | `http://localhost:5098` |

## Development

```bash
npm ci
npm run dev
npm run lint
npm run test
npm run build
```

## Authentication

The application uses Google Sign-In to obtain an ID token, which is exchanged with the Mova API for a JWT access token. The access token is parsed to extract roles and complex associations used by the protected route guards.

## Available routes

| Route | Description | Access |
| --- | --- | --- |
| `/` | Public landing page with Mova's value proposition and sign-in call to action. | Public |
| `/complexes` | Search and browse active sports complexes. | Public |
| `/complexes/:complexId` | View an active complex, filter its courts by sport, and check court availability for a selected date. | Public |
| `/login` | Sign in with Google. | Public |
| `/complete-profile` | Complete the user profile (phone number). | Authenticated users |
| `/user` | User portal home. | Authenticated users |
| `/admin/super` | Super admin dashboard. | `SuperAdmin` |
| `/admin/complex/:complexId` | Complex admin dashboard overview with courts, reservations, and blocked users summaries. | `ComplexAdmin` of the requested complex |
| `/admin/complex/:complexId/profile` | Edit the complex public profile. | `ComplexAdmin` of the requested complex |
| `/admin/complex/:complexId/courts` | List and manage the courts of the complex. | `ComplexAdmin` of the requested complex |
| `/admin/complex/:complexId/reservations` | Complex admin reservations. | `ComplexAdmin` of the requested complex |
| `/admin/complex/:complexId/users` | Complex admin user management. | `ComplexAdmin` of the requested complex |

There is currently no UI for creating a sports complex; creation is exposed through the backend API at `POST /api/v1/complexes`. Editing a complex profile is available in the admin panel at `/admin/complex/:complexId/profile` using `PUT /api/v1/complexes/{complexId}`.
