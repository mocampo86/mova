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
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=mova;Username=postgres;Password=<your-password>;GSS Encryption Mode=Disable" --project src/Mova.Api
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

### Time zone data

The API resolves IANA time zone identifiers using .NET `TimeZoneInfo`, which depends on the host's IANA time zone database (`tzdata`). When running on Linux containers or App Service, ensure `tzdata` is installed and up to date. Windows 10/11 and recent Windows Server builds have native IANA support; older versions may require the ICU time zone data to be present. If `TimeZoneInfo.TryFindSystemTimeZoneById` cannot resolve a configured `TimeZoneId`, all time zone-dependent operations fail with `TIMEZONE_NOT_CONFIGURED`.

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

The API exposes the following health endpoints:

| Endpoint | Purpose | Checks |
|----------|---------|--------|
| `/health` | Aggregate health status. | All registered checks. |
| `/health/live` | Liveness probe. | Returns `Healthy` when the API process is running. |
| `/health/ready` | Readiness probe. | PostgreSQL connectivity and error-rate threshold. |

The `error-rate` check becomes `Unhealthy` when the rolling error rate (server-side errors per minute) in the last five minutes exceeds the configured `ErrorRateHealthCheck:MaxErrorRatePerMinute` threshold (default `5.0`). `ErrorRateTracker:EvaluationWindow` controls the window duration and `ErrorRateTracker:MaxQueueSize` caps the in-memory error queue. Load balancers and container orchestrators can use this to route traffic away from an instance that is experiencing elevated failures and to trigger operator alerts.

Load balancers and container orchestrators use these endpoints for routing and restart decisions.

## Monitoring and alerting

Versioned Bicep templates in `devops/azure` deploy the monitoring and alerting resources required for production operations:

- **Action group** (`<appServiceName>-mova-alerts`) with an email receiver and an optional webhook receiver.
- **Server-error rate alert** (`<appServiceName>-server-errors`) that fires when 5xx responses exceed the configured per-minute threshold.
- **Readiness probe alert** (`<appServiceName>-readiness-failures`) that fires when `/health/ready` returns a non-2xx response.
- **App Service health-check path** set to `/health/ready` so the platform can remove unhealthy instances from load balancing.

Default parameters match the API's `ErrorRateHealthCheck:MaxErrorRatePerMinute` (5.0) and `ErrorRateTracker:EvaluationWindow` (5 minutes) values in `src/Mova.Api/appsettings.json`.

To deploy the monitoring stack:

```powershell
cp devops/azure/main.bicepparam.example devops/azure/main.bicepparam
# Edit devops/azure/main.bicepparam with the environment-specific values

az deployment group create `
  --resource-group mova-prod-rg `
  --template-file devops/azure/main.bicep `
  --parameters devops/azure/main.bicepparam
```

For full parameter descriptions and smoke-test instructions, see `devops/azure/README.md`. For incident response procedures, see `operations/OPERATIONAL-RUNBOOK.md`.

## Rate limiting

The API uses ASP.NET Core rate limiting on sensitive endpoints. The `RateLimiting` configuration section controls each policy:

| Policy | Endpoints | Partition key | Default | Config keys |
|--------|-----------|---------------|---------|-------------|
| `search` | Complex user search | Authenticated user ID or client IP | 60 requests/minute | `RateLimiting:Search:PermitLimit`, `RateLimiting:Search:WindowSeconds`, `RateLimiting:Search:QueueLimit` |
| `login` | `POST /api/v1/auth/google`, `POST /api/v1/auth/complete-complex-admin` | Effective client IP | 20 requests/minute | `RateLimiting:Login:PermitLimit`, `RateLimiting:Login:WindowSeconds`, `RateLimiting:Login:QueueLimit` |
| `reservation` | Reservation and recurring-reservation creation | Authenticated user ID or client IP | 30 requests/minute | `RateLimiting:Reservation:PermitLimit`, `RateLimiting:Reservation:WindowSeconds`, `RateLimiting:Reservation:QueueLimit` |

Set `RateLimiting:Enabled` to `false` to disable rate-limiter middleware entirely (useful for integration tests).

When a limit is exceeded, the API returns `429 Too Many Requests` with the standard structured error response and a `Retry-After` header indicating when the client can retry.

## Forwarded headers

When the API runs behind a reverse proxy or load balancer (e.g., Azure Front Door, Azure Load Balancer), the `ForwardedHeaders` configuration section controls how the application derives the effective client IP used by the `login` rate-limiting policy. Only configure this for trusted proxies and networks.

| Setting | Purpose | Example |
|---------|---------|---------|
| `ForwardedHeaders:ForwardedHeaders` | Which forwarded headers to process. Defaults to `XForwardedFor` for client IP. | `XForwardedFor` |
| `ForwardedHeaders:ForwardLimit` | Maximum number of entries to read from `X-Forwarded-For`. | `1` |
| `ForwardedHeaders:KnownProxies` | Specific proxy IP addresses that are trusted. | `["10.0.0.10"]` |
| `ForwardedHeaders:KnownNetworks` | Trusted proxy networks in CIDR notation. | `["10.0.0.0/24"]` |

Do not set `KnownProxies` or `KnownNetworks` to broad ranges such as `0.0.0.0/0`, because that would allow clients to spoof their IP address. When `KnownProxies` and `KnownNetworks` are not configured, the middleware does not trust `X-Forwarded-For` and uses the direct TCP remote address.

## Backup and restore

See [operations/BACKUP-RESTORE.md](../operations/BACKUP-RESTORE.md) for detailed backup, restore, and drill procedures for local and Azure Database for PostgreSQL environments.

## Rollback plan

- Keep the previous Docker image tagged and ready for redeploy.
- Database rollbacks require a migration reversal script; forward-only migrations are preferred.
- Feature flags can be used to disable new functionality without redeploying.

## Cost optimization for MVP

- Use Azure App Service Free or Basic tier for low traffic.
- Use Azure Database for PostgreSQL Burstable tier.
- Use Azure Static Web Apps free tier.
- Monitor Application Insights sampling to control cost.
