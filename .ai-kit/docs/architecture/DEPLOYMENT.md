# Deployment & Infrastructure

## Local development environment

### Services

- **PostgreSQL** runs inside a Docker container.
- **API** runs locally via `dotnet run` or `dotnet watch`.
- **Web app** runs locally via `npm run dev`.
- **Migrations** are applied manually with `dotnet ef database update`.

### Docker Compose

The repository contains a `docker-compose.yml` file and an `.env.example` file.

1. Copy `.env.example` to `.env` and set a strong `POSTGRES_PASSWORD`.
2. Start PostgreSQL:

   ```powershell
   docker compose up -d
   ```

3. Stop PostgreSQL:

   ```powershell
   docker compose down
   ```

### Local secrets

Use .NET user secrets for the API connection string so the local password is not committed:

```powershell
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=reservacanchas;Username=postgres;Password=<your-password>" --project src/ReservaCanchas.Api
```

Do not commit `.env` or `secrets.json`.

## Environments

| Environment | Purpose |
|-------------|---------|
| Local | Developer machine |
| Dev | Integration branch deployment |
| QA | Manual and automated testing |
| Staging | Pre-production validation |
| Production | Live users |

## Production target (Azure)

### Compute

- **API**: Azure App Service or Azure Container Apps.
- **Frontend**: Azure Static Web Apps.

### Data

- **PostgreSQL**: Azure Database for PostgreSQL Flexible Server.

### Secrets and configuration

- **Azure Key Vault** for connection strings, Google client secrets, JWT signing keys.
- **Azure App Configuration** for non-secret feature flags and settings.

### Observability

- **Application Insights** for distributed tracing, request logging, and exceptions.
- **Log Analytics** for log queries and alerts.

### Networking

- HTTPS only.
- Custom domain and TLS certificate.
- CORS configured to allow only the frontend origin.

## CI/CD pipeline

### GitHub Actions example

```yaml
name: CI/CD
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore --configuration Release
      - run: dotnet test --no-build --verbosity normal

  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 20
      - run: npm ci
      - run: npm run lint
      - run: npm run build
      - run: npm run test

  deploy:
    needs: [backend, frontend]
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      # Build and push Docker image for API
      # Deploy Static Web App
      # Apply EF migrations to Azure PostgreSQL
```

## Database migrations in production

Migrations are applied in one of two ways:

1. **Startup migration** in a containerized environment with idempotent checks.
2. **CI/CD step** with `dotnet ef database update` against the production connection string before app deployment.

Always back up the database before applying migrations to production.

## Secrets management checklist

- [ ] Connection strings in Key Vault / user secrets.
- [ ] Google client credentials in Key Vault.
- [ ] JWT signing key in Key Vault (use RSA or symmetric key depending on strategy).
- [ ] No secrets in `appsettings.Production.json`.
- [ ] `.env` files ignored by Git.

## Health checks

The API exposes `/health` and `/health/ready` endpoints:

- `/health`: API is running.
- `/health/ready`: API can connect to PostgreSQL.

Load balancers and container orchestrators use these endpoints for routing and restart decisions.

## Rollback plan

- Keep the previous Docker image tagged and ready for redeploy.
- Database rollbacks require a migration reversal script; forward-only migrations are preferred.
- Feature flags can be used to disable new functionality without redeploying.

## Cost optimization for MVP

- Use Azure App Service Free or Basic tier for low traffic.
- Use Azure Database for PostgreSQL Burstable tier.
- Use Azure Static Web Apps free tier.
- Monitor Application Insights sampling to control cost.
