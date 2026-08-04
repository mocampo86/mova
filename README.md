# Mova

A web platform for sports complexes, clubs, and owners to publish and manage courts, while end users check availability and make reservations.

This repository contains the MVP for **Mova**, built as a modular monolith:

- **Backend**: ASP.NET Core Web API on .NET 10 with Clean Architecture layers.
- **Frontend**: React 18 + TypeScript + Vite SPA.
- **Database**: PostgreSQL 15+ (local development runs in Docker).
- **Auth**: Google OpenID Connect + JWT Bearer tokens.
- **Tests**: xUnit (backend), Vitest / React Testing Library (frontend).

For detailed architecture, domain, and workflow docs, see [.ai-kit/docs](.ai-kit/docs).

---

## Table of contents

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Project structure](#project-structure)
- [Technology stack](#technology-stack)
- [Configuration and secrets](#configuration-and-secrets)
- [Database and migrations](#database-and-migrations)
- [Running the application](#running-the-application)
- [Testing](#testing)
- [Health checks and API documentation](#health-checks-and-api-documentation)
- [CI/CD](#cicd)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet) (10.0.x)
- [Node.js](https://nodejs.org/) 20+ with npm
- [Docker](https://www.docker.com/) (Docker Engine or Docker Desktop) for PostgreSQL
- Optionally [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) CLI for migrations:

  ```powershell
  dotnet tool install --global dotnet-ef
  ```

---

## Quick start

1. Clone the repository and open it in your terminal:

   ```powershell
   cd C:\Endava\EndevLocal\source\mova
   ```

2. Create the local environment file for PostgreSQL:

   ```powershell
   cp .env.example .env
   ```

   Edit `.env` and set a strong `POSTGRES_PASSWORD`.

3. Start the local PostgreSQL container:

   ```powershell
   docker compose up -d
   ```

4. Configure the API connection string as a user secret (recommended) or environment variable:

   ```powershell
   dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=mova;Username=postgres;Password=<YOUR_PASSWORD>;GSS Encryption Mode=Disable" --project src/Mova.Api
   ```

   Replace `<YOUR_PASSWORD>` with the value from `.env`.

5. Apply database migrations once migrations are available:

   ```powershell
   dotnet ef database update --startup-project src/Mova.Api
   ```

6. Run the API and the web application (in separate terminals):

   ```powershell
   # API
   dotnet run --project src/Mova.Api

   # Web
   cd src/mova-web
   npm ci
   npm run dev
   ```

   The API runs on `http://localhost:5098` (and `https://localhost:7128` for the HTTPS profile).
   The web dev server runs on `http://localhost:5173` by default.

---

## Project structure

```text
.
├── .ai-kit/docs              # Architecture, domain, workflow, and agent docs
├── .github/workflows         # CI/CD pipelines
├── docker-compose.yml        # Local PostgreSQL container
├── .env.example              # Example environment variables for Docker
├── Mova.slnx       # .NET solution file
├── src/
│   ├── Mova.Api              # HTTP layer, middleware, health checks, config
│   ├── Mova.Application      # Use cases, commands, queries, validators
│   ├── Mova.Domain           # Entities, value objects, business rules
│   ├── Mova.Infrastructure   # EF Core, PostgreSQL, external integrations
│   ├── Mova.Contracts        # Public API DTOs and error contracts
│   └── mova-web              # React SPA (Vite + TypeScript + MUI)
└── tests/
    ├── Mova.UnitTests
    ├── Mova.IntegrationTests
    └── Mova.ArchitectureTests
```

---

## Technology stack

### Backend

- **Runtime**: .NET 10
- **Web framework**: ASP.NET Core Web API
- **ORM**: Entity Framework Core
- **Database driver**: Npgsql 10
- **Validation**: FluentValidation
- **Logging**: Serilog
- **Authentication**: Google OpenID Connect + JWT Bearer
- **Testing**: xUnit, `Microsoft.AspNetCore.Mvc.Testing`

### Frontend

- **Framework**: React 18
- **Language**: TypeScript
- **Build tool**: Vite
- **Routing**: React Router
- **Server state**: TanStack Query
- **Forms**: React Hook Form + Zod
- **UI kit**: Material UI
- **Testing**: Vitest, React Testing Library, Playwright

### Infrastructure

- **Database**: PostgreSQL 15+
- **Local containerization**: Docker Compose
- **CI/CD**: GitHub Actions
- **Cloud target**: Azure App Service / Container Apps, Azure Database for PostgreSQL, Azure Static Web Apps, Azure Key Vault

---

## Configuration and secrets

The API uses the standard ASP.NET Core configuration hierarchy:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User secrets (Development)
4. Environment variables

### Required settings

| Setting | Description | How to configure |
|---------|-------------|------------------|
| `Database:ConnectionString` | PostgreSQL connection string | User secrets or environment variable |
| Google OAuth credentials | `ClientId` / `ClientSecret` for Google sign-in | User secrets or Azure Key Vault |
| JWT signing key | Secret key or certificate for JWT tokens | User secrets or Azure Key Vault |

### Setting user secrets

```powershell
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=mova;Username=postgres;Password=<PASSWORD>;GSS Encryption Mode=Disable" --project src/Mova.Api
```

> Never commit secrets, `.env`, or user secrets files to source control. They are already ignored by `.gitignore`.

---

## Database and migrations

Migrations are managed with Entity Framework Core. The `Mova.Infrastructure` project is the intended home for the `DbContext` and migrations.

### Create a migration

```powershell
dotnet ef migrations add <MigrationName> --project src/Mova.Infrastructure --startup-project src/Mova.Api
```

### Apply migrations

```powershell
dotnet ef database update --startup-project src/Mova.Api
```

### Revert a migration

```powershell
dotnet ef database update <PreviousMigrationName> --startup-project src/Mova.Api
```

### Local PostgreSQL with Docker

```powershell
# Start
docker compose up -d

# View logs
docker compose logs -f postgres

# Stop
docker compose down

# Stop and remove data volume
docker compose down -v
```

---

## Running the application

### Backend only

```powershell
dotnet run --project src/Mova.Api --launch-profile https
```

### Frontend only

```powershell
cd src/mova-web
npm ci
npm run dev
```

### Full stack

1. Start PostgreSQL: `docker compose up -d`
2. Run migrations: `dotnet ef database update --startup-project src/Mova.Api`
3. Run the API: `dotnet run --project src/Mova.Api`
4. Run the web app: `cd src/mova-web && npm run dev`

---

## Testing

### Backend

```powershell
# Full solution test run (requires a running PostgreSQL for integration tests)
dotnet test

# Run only unit and architecture tests (no database required)
dotnet test --filter "FullyQualifiedName!~DatabaseConnectivityTests"

# Filter by test project
dotnet test --filter "FullyQualifiedName~UnitTests"
dotnet test --filter "FullyQualifiedName~IntegrationTests"
dotnet test --filter "FullyQualifiedName~ArchitectureTests"
```

### Frontend

```powershell
cd src/mova-web

# Install dependencies
npm ci

# Lint
npm run lint

# Build
npm run build

# Unit tests
npm run test

# End-to-end tests (Playwright)
npm run test:e2e
```

---

## Health checks and API documentation

When the API is running:

| Endpoint | Purpose |
|----------|---------|
| `GET /health/live` | Liveness probe (does not require the database) |
| `GET /health/ready` | Readiness probe (verifies database connectivity) |
| `GET /swagger/index.html` | OpenAPI/Swagger UI (Development environment) |
| `GET /swagger/v1/swagger.json` | OpenAPI JSON contract |

The API base path is `/api/v1`.

---

## CI/CD

The repository uses GitHub Actions. The workflow is defined in `.github/workflows/ci.yml`:

- Detects changes to backend or frontend paths.
- Backend job: restores, builds, and runs `dotnet test` against a PostgreSQL service container.
- Frontend job: installs dependencies, runs lint, build, and tests.

Pull requests targeting `main` or `develop` trigger the pipeline.

---

## Troubleshooting

### `Npgsql` connection failures on Linux or CI

Npgsql 10 defaults to `GSS Encryption Mode=Prefer`. On Linux runners without Kerberos this can cause hangs or connection errors. The default connection strings in `appsettings.json` and `appsettings.Development.json` include `GSS Encryption Mode=Disable`. If you override the connection string via user secrets or environment variables, keep that setting.

### PostgreSQL is not reachable from the API

- Verify Docker Compose is running: `docker compose ps`
- Check the port mapping in `.env` (`POSTGRES_PORT` defaults to `5432`)
- Confirm the connection string in user secrets matches the database, user, and password
- Check API logs for `NpgsqlException` or health check failures

### `dotnet ef` commands are not found

Install the EF Core CLI globally:

```powershell
dotnet tool install --global dotnet-ef
```

---

## More documentation

- [Architecture overview](.ai-kit/docs/architecture/SOLUTION-ARCHITECTURE.md)
- [Domain model](.ai-kit/docs/architecture/DOMAIN-MODEL.md)
- [Database design](.ai-kit/docs/architecture/DATABASE-DESIGN.md)
- [API design](.ai-kit/docs/architecture/API-DESIGN.md)
- [Authentication](.ai-kit/docs/architecture/AUTHENTICATION.md)
- [Deployment](.ai-kit/docs/architecture/DEPLOYMENT.md)
- [Development workflow](.ai-kit/docs/workflows/DEVELOPMENT-WORKFLOW.md)
- [Testing workflow](.ai-kit/docs/workflows/TESTING-WORKFLOW.md)
