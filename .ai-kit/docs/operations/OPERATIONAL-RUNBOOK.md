# Operational Runbook

This document covers the common operational procedures for the Mova platform. It is intended for the DevOps engineer, backend engineer, and on-call operators.

## Table of contents

1. [Health checks](#health-checks)
2. [Logging and observability](#logging-and-observability)
3. [Error monitoring](#error-monitoring)
4. [Rate limiting](#rate-limiting)
5. [Audit log investigation](#audit-log-investigation)
6. [Incident response](#incident-response)
7. [Common procedures](#common-procedures)

---

## Health checks

The API exposes three health endpoints. All are unauthenticated to allow load balancers and orchestrators to probe them.

| Endpoint | Purpose | Checks included |
|----------|---------|-----------------|
| `GET /health` | Aggregate status | All registered checks |
| `GET /health/live` | Liveness probe | None — returns `Healthy` if the process is running |
| `GET /health/ready` | Readiness probe | `database`, `error-rate` |

### Response format

```json
{
  "status": "Healthy",
  "dependencies": [
    { "name": "database", "status": "Healthy" },
    { "name": "error-rate", "status": "Healthy" }
  ]
}
```

In Development, descriptions are included for each dependency. In Production, descriptions are omitted to avoid exposing internal details.

### Interpreting health check results

| Check | Healthy | Unhealthy | Action |
|-------|---------|-----------|--------|
| `database` | PostgreSQL connection succeeded | Cannot open a connection | Check PostgreSQL status, network, connection string, connection pool exhaustion |
| `error-rate` | Server errors per minute below threshold | Error rate exceeds `ErrorRateHealthCheck:MaxErrorRatePerMinute` (default `5.0`) | Investigate recent 5xx errors in logs, check for deployment issues or downstream failures |

### Load balancer configuration

- Use `/health/live` for **liveness** probes (restart the container if this fails).
- Use `/health/ready` for **readiness** probes (stop routing traffic if this fails).
- Recommended probe interval: 10 seconds, timeout: 5 seconds, failure threshold: 3.

---

## Logging and observability

### Serilog configuration

The API uses Serilog with structured JSON logging:

- **Console**: Compact JSON format with sensitive data redaction via `SensitiveDataRedactingFormatter`.
- **Application Insights**: Enabled when `ApplicationInsights:ConnectionString` or `APPLICATIONINSIGHTS_CONNECTION_STRING` is configured.
- **Minimum level**: `Debug` in Development, `Information` in Production.
- **Microsoft.AspNetCore** override: `Warning` (reduces noise from framework logs).

### Correlation IDs

Every request is tagged with a correlation ID, propagated through the `CorrelationIdMiddleware`:

1. The middleware checks for an `X-Correlation-Id` request header.
2. If absent, it uses the existing `HttpContext.TraceIdentifier` or generates a new GUID.
3. The correlation ID is:
   - Set as `HttpContext.TraceIdentifier`.
   - Added to the `X-Correlation-Id` response header.
   - Pushed into the Serilog `LogContext` as `CorrelationId` and `TraceId`.

**Usage**: When investigating an issue reported by a user, ask for the `X-Correlation-Id` from the response headers, then search logs by `CorrelationId`.

### Log queries

#### Application Insights (KQL)

Search for all log entries with a given correlation ID:

```kql
traces
| where customDimensions.CorrelationId == "your-correlation-id"
| order by timestamp asc
```

Search for recent server errors:

```kql
traces
| where severityLevel >= 3
| where timestamp > ago(1h)
| order by timestamp desc
| take 50
```

Search for errors on a specific endpoint:

```kql
requests
| where resultCode >= 500
| where timestamp > ago(1h)
| summarize count() by name, resultCode
| order by count_ desc
```

#### Console / local development

In local development, logs are written to stdout in compact JSON format. Use `jq` or similar tools to filter:

```powershell
# Search for a specific correlation ID
dotnet run 2>&1 | jq 'select(.CorrelationId == "your-id")'

# Search for errors
dotnet run 2>&1 | jq 'select(.["@l"] == "Error")'
```

### Sensitive data redaction

The `SensitiveDataRedactingFormatter` automatically redacts log properties that may contain sensitive data (passwords, tokens, secrets). Do not add PII or credentials to structured log properties.

---

## Error monitoring

### Global exception handler

Unhandled exceptions are caught by `GlobalExceptionHandler`:

- Logs the exception with the `TraceId`.
- Returns a structured error response.
- In Production, error details are **not** exposed to the client.
- In Development, additional debugging info is included in the response.

### Error rate tracking

The `ErrorRateTrackingMiddleware` records every response with status code >= 500 into an in-memory `ErrorRateTracker`:

- **Evaluation window**: configurable via `ErrorRateTracker:EvaluationWindow` (default: 5 minutes).
- **Max queue size**: configurable via `ErrorRateTracker:MaxQueueSize` (default: 1000).
- **Health check threshold**: configurable via `ErrorRateHealthCheck:MaxErrorRatePerMinute` (default: 5.0).

When the error rate exceeds the threshold, the `/health/ready` endpoint returns `Unhealthy`, allowing the load balancer to stop routing traffic to the instance.

### Responding to elevated error rates

1. Check the `/health/ready` endpoint — if `error-rate` is `Unhealthy`, proceed with investigation.
2. Query recent 500-level errors in Application Insights or console logs.
3. Look for common patterns: specific endpoints, users, or request payloads.
4. Check recent deployments — if a new version was deployed, consider rolling back.
5. Check downstream dependencies — PostgreSQL connectivity, external APIs.
6. If the issue is resolved, the error rate will naturally drop below the threshold as the evaluation window slides.

---

## Rate limiting

The API applies rate limiting on sensitive endpoints using ASP.NET Core `RateLimiting` middleware.

### Policies

| Policy | Applied to | Default limit | Config section |
|--------|-----------|---------------|----------------|
| `search` | Complex user search | 60 req/min | `RateLimiting:Search` |
| `login` | Authentication endpoints | 20 req/min | `RateLimiting:Login` |
| `reservation` | Reservation creation and mutation | 30 req/min | `RateLimiting:Reservation` |

### Configuration

Rate limiting is configured in `appsettings.json` under the `RateLimiting` section:

```json
{
  "RateLimiting": {
    "Enabled": true,
    "Search": {
      "PermitLimit": 60,
      "WindowSeconds": 60,
      "QueueLimit": 0
    },
    "Login": {
      "PermitLimit": 20,
      "WindowSeconds": 60,
      "QueueLimit": 0
    },
    "Reservation": {
      "PermitLimit": 30,
      "WindowSeconds": 60,
      "QueueLimit": 0
    }
  }
}
```

- Set `RateLimiting:Enabled` to `false` to disable rate limiting entirely (useful for load testing or integration tests).
- Rate limits are partitioned by authenticated user ID or remote IP address for anonymous requests.
- Responses exceeding the limit receive HTTP `429 Too Many Requests`.

### Adjusting rate limits

1. Monitor 429 response codes in Application Insights or logs.
2. If legitimate users are being throttled, increase the `PermitLimit` or `WindowSeconds`.
3. If abuse is detected, decrease the limits on the affected policy.
4. Changes to `appsettings.json` take effect on the next application restart (or through Azure App Configuration / environment variables for runtime changes).

---

## Audit log investigation

### Overview

All administrative actions are recorded in the `AuditLog` table with the following fields:

| Field | Description |
|-------|-------------|
| `Id` | Unique record identifier |
| `UserId` | Actor who performed the action (nullable for system actions) |
| `SportsComplexId` | Scope of the action (nullable for global actions) |
| `Action` | Action identifier (e.g., `Court.Create`, `Reservation.Cancel`) |
| `EntityType` | Type of the affected entity (e.g., `Court`, `Reservation`) |
| `EntityId` | Identifier of the affected entity |
| `CreatedAt` | UTC timestamp |
| `Metadata` | JSON payload with additional context |

### Querying audit logs via API

The `GET /api/v1/admin/audit-logs` endpoint (requires `SuperAdmin` role) supports the following query parameters:

| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (1-100, default: 20) |
| `sportsComplexId` | Guid? | Filter by complex |
| `userId` | Guid? | Filter by actor |
| `action` | string? | Filter by action type |
| `entityType` | string? | Filter by entity type |
| `entityId` | string? | Filter by entity ID |
| `from` | DateTime? | Start of date range (UTC) |
| `to` | DateTime? | End of date range (UTC) |

### Direct database queries

For bulk investigation, query the `audit_logs` table directly:

```sql
-- Recent admin actions for a specific complex
SELECT * FROM audit_logs
WHERE sports_complex_id = 'your-complex-id'
ORDER BY created_at DESC
LIMIT 50;

-- All actions by a specific user
SELECT * FROM audit_logs
WHERE user_id = 'your-user-id'
ORDER BY created_at DESC;

-- Count of actions by type in the last 24 hours
SELECT action, COUNT(*) as count
FROM audit_logs
WHERE created_at > NOW() - INTERVAL '24 hours'
GROUP BY action
ORDER BY count DESC;
```

### Audit log integrity

- Audit logs are **immutable** — they are never updated or deleted.
- If suspicious activity is detected, query by `UserId` and time range to trace the actor's actions.

---

## Incident response

### Severity levels

| Level | Description | Response time | Example |
|-------|-------------|---------------|---------|
| P1 — Critical | Service is down or data loss occurring | Immediate | Database unreachable, API not responding |
| P2 — High | Major feature broken, no workaround | 1 hour | Reservations failing, authentication broken |
| P3 — Medium | Feature degraded, workaround exists | 4 hours | Slow queries, intermittent 500 errors |
| P4 — Low | Minor issue, cosmetic | Next business day | UI alignment, non-critical log noise |

### Incident workflow

1. **Detect**: Alert from health check, Application Insights, or user report.
2. **Acknowledge**: Assign the incident and notify the team.
3. **Diagnose**:
   - Check `/health/ready` for service status.
   - Search logs by correlation ID or time range.
   - Check recent deployments and configuration changes.
   - Query the `audit_logs` table for recent administrative actions.
4. **Mitigate**:
   - Roll back the deployment if the issue was introduced by a recent change.
   - Restart the application if the issue is transient.
   - Disable rate limiting if legitimate traffic is being blocked.
   - Scale up resources if the issue is load-related.
5. **Resolve**: Confirm the fix, update health checks, close the incident.
6. **Post-mortem**: Document root cause, timeline, and preventive actions.

### Rollback procedure

1. Identify the last known good deployment (Docker image tag or commit SHA).
2. Redeploy the previous version:
   - **Azure App Service**: Swap deployment slots or redeploy the previous image.
   - **Container Apps**: Update the container image to the previous tag.
3. If a database migration was applied, evaluate whether a rollback migration is needed — forward-only migrations are preferred.
4. Verify the rollback by checking `/health/ready` and running smoke tests.

---

## Common procedures

### Restarting the API

#### Local

```powershell
# Stop the running process (Ctrl+C or kill)
# Restart
dotnet run --project src/Mova.Api
```

#### Azure App Service

```powershell
az webapp restart --resource-group <resource-group> --name <app-name>
```

### Applying database migrations

#### Local

```powershell
dotnet ef database update --project src/Mova.Infrastructure --startup-project src/Mova.Api
```

#### Production

1. **Back up the database** (see [BACKUP-RESTORE.md](BACKUP-RESTORE.md)).
2. Run the migration against the production connection string:
   ```powershell
   dotnet ef database update --project src/Mova.Infrastructure --startup-project src/Mova.Api --connection "your-production-connection-string"
   ```
3. Verify the migration by checking the `__EFMigrationsHistory` table.
4. Restart the API to pick up any schema changes.

### Checking connection pool status

If the database health check fails intermittently, connection pool exhaustion may be the cause:

```sql
-- Check active connections
SELECT count(*) FROM pg_stat_activity WHERE datname = 'mova';

-- Check connections by state
SELECT state, count(*) FROM pg_stat_activity WHERE datname = 'mova' GROUP BY state;
```

If the connection count is near the PostgreSQL `max_connections` limit, consider:
- Reducing `MaxPoolSize` in the connection string.
- Increasing `max_connections` on the server.
- Investigating long-running queries or leaked connections.

### Viewing application configuration

To verify the active configuration at runtime, check the structured logs at startup — Serilog logs the active configuration values. Sensitive values are redacted.

For Azure deployments, configuration values can be inspected via:

```powershell
az webapp config appsettings list --resource-group <resource-group> --name <app-name>
```

### Docker Compose operations (local development)

```powershell
# Start PostgreSQL
docker compose up -d

# Stop PostgreSQL
docker compose down

# View PostgreSQL logs
docker compose logs postgres

# Reset database (destructive)
docker compose down -v
docker compose up -d
```

---

## Related documentation

- [BACKUP-RESTORE.md](BACKUP-RESTORE.md) — Database backup and restore procedures.
- [DEPLOYMENT.md](../architecture/DEPLOYMENT.md) — Deployment architecture and CI/CD pipeline.
- [SOLUTION-ARCHITECTURE.md](../architecture/SOLUTION-ARCHITECTURE.md) — System architecture overview.
