# Solution Architecture

## Overview

Mova is a web platform for sports complex owners to publish and manage courts, and for end users to check availability and make reservations.

The system is designed as a **modular monolith** with a clean vertical-slice organization. This keeps initial cost and operational complexity low while preserving the ability to extract services later if needed.

## High-level components

```text
┌─────────────────────────────────────────────────────────────────┐
│                         Client Browser                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │ Public Site  │  │ User Portal  │  │ Admin Panel (shared) │  │
│  └──────┬───────┘  └──────┬───────┘  └──────────┬───────────┘  │
└───────┬─────────────────┬─────────────────────┬──────────────────┘
        │                 │                     │
        └─────────────────┴─────────────────────┘
                          │
                  ┌───────▼────────┐
                  │  React SPA     │
                  │  Vite + TS     │
                  │  MUI / React   │
                  └───────┬────────┘
                          │ HTTPS / REST
                  ┌───────▼────────┐
                  │  ASP.NET Core  │
                  │  Web API       │
                  │  .NET 8/10     │
                  └───────┬────────┘
                          │
                  ┌───────▼────────┐
                  │  PostgreSQL    │
                  │  Single DB     │
                  └────────────────┘
```

## Backend structure

The backend follows a layered, modular monolith organization:

```text
src/
├── Mova.Api             # HTTP layer, middleware, auth, config
├── Mova.Application     # Use cases, commands, queries, validators
├── Mova.Domain          # Entities, value objects, business rules
├── Mova.Infrastructure    # EF Core, PostgreSQL, repositories, integrations
└── Mova.Contracts       # Public API contracts, DTOs

tests/
├── Mova.UnitTests
├── Mova.IntegrationTests
└── Mova.ArchitectureTests
```

### Layer responsibilities

- **Api**: controllers, endpoint mapping, OpenAPI, health checks, auth wiring, middlewares.
- **Application**: MediatR-style handlers (commands/queries), orchestration, validation interfaces, external service interfaces.
- **Domain**: pure business logic, entities, value objects, domain events when required, invariants.
- **Infrastructure**: persistence implementation, EF Core mappings, external integrations, authentication implementation, audit persistence.
- **Contracts**: request/response models, pagination, error contracts.

## Frontend structure

```text
src/
├── app/
│   └── routing, providers, global state
├── components/
│   └── shared, reusable UI components
├── layouts/
│   ├── PublicLayout
│   ├── UserLayout
│   └── AdminLayout
├── pages/
│   └── page-level components
├── features/
│   ├── auth
│   ├── users
│   ├── complexes
│   ├── courts
│   ├── availability
│   ├── reservations
│   ├── recurring-reservations
│   └── administration
├── services/
│   └── API clients
├── hooks/
│   └── custom React hooks
└── shared/
    └── utils, constants, types
```

## Technology stack

### Backend

- **Runtime**: .NET 8 LTS (preferred) or .NET 10
- **Web framework**: ASP.NET Core Web API
- **ORM**: Entity Framework Core
- **Database driver**: Npgsql
- **Validation**: FluentValidation
- **Logging**: Serilog
- **Authentication**: Google OpenID Connect + JWT Bearer
- **Testing**: xUnit, FluentAssertions, Testcontainers

### Frontend

- **Framework**: React 18+
- **Language**: TypeScript
- **Build tool**: Vite
- **Routing**: React Router
- **Server state**: TanStack Query
- **Forms**: React Hook Form + Zod
- **UI kit**: Material UI
- **Testing**: Vitest, React Testing Library, Playwright

### Data

- **Database**: PostgreSQL
- **Migrations**: Entity Framework Core
- **Local environment**: Docker Compose

### Infrastructure

- **Local**: Docker Compose (PostgreSQL container), API and web running locally
- **Cloud target**: Azure
  - API: Azure App Service or Container Apps
  - Frontend: Azure Static Web Apps
  - Database: Azure Database for PostgreSQL
  - Secrets: Azure Key Vault
  - Observability: Application Insights
  - CI/CD: GitHub Actions or Azure DevOps Pipelines

## Design principles

1. **Single shared database** for the MVP with logical isolation by `SportsComplexId`.
2. **Vertical slices** inside the application layer grouped by feature (complexes, courts, reservations, etc.).
3. **Domain-driven** entities with encapsulated business rules (no anemic models).
4. **API-first** with OpenAPI/Swagger generated from the controllers.
5. **Mobile-first** responsive UI.
6. **Security by default**: HTTPS, JWT, role-based authorization, input validation, audit logging.
7. **Test coverage for critical paths**: concurrency, authorization, and core business rules.

## C4 Level 1 - System Context

```text
[User] --(reserves courts, manages profile)--> [Mova Web App]
[Complex Admin] --(manages complex, courts, reservations, blocks)--> [Mova Web App]
[Super Admin] --(manages tenants, audits, metrics)--> [Mova Web App]

[Mova Web App] --(authenticates)--> [Google Identity]
[Mova Web App] --(reads/writes data)--> [PostgreSQL]
[Mova Web App] --(logs/metrics)--> [Application Insights]
```

## C4 Level 2 - Container Diagram

```text
[Browser]
   │
   ├─ React SPA (Vite, TypeScript, MUI)
   │
   └─ ASP.NET Core API (.NET)
        │
        ├─ Domain / Application / Infrastructure layers
        │
        └─ PostgreSQL
```

## Future evolution

- Extract `Billing` or `Notifications` to independent services if volume grows.
- Introduce read replicas or caching if availability queries become hot.
- Add native mobile apps only after web MVP validation.
