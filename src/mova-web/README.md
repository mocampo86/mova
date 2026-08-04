# Mova Web

React frontend for the Mova application.

## Environment variables

Create a `.env` file in this directory based on the following variables:

| Variable | Description | Default |
| --- | --- | --- |
| `VITE_GOOGLE_CLIENT_ID` | Google OAuth 2.0 client ID for Google Sign-In. | Required |
| `VITE_API_BASE_URL` | Base URL of the Mova API. | `http://localhost:5000` |

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
