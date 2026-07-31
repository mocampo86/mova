# Architecture Decision Records (ADRs)

This folder contains Architecture Decision Records (ADRs) for Reserva Canchas (MOVA).

## What is an ADR?

An ADR is a document that captures an important architectural decision made along with its context and consequences. It helps future team members understand why a particular approach was chosen.

## Naming convention

Use the following naming pattern:

```text
NNNN-title-with-dashes.md
```

Example:

```text
0001-use-modular-monolith.md
0002-shared-postgresql-multi-tenancy.md
0003-google-openid-connect.md
```

## How to create a new ADR

1. Copy `adr-template.md`.
2. Rename it with the next available number and a descriptive title.
3. Fill in the sections.
4. Open a pull request and request review from the Architect or tech lead.
5. Update the ADR status when a decision is accepted, deprecated, or superseded.

## Active ADRs

- [ADR-0001: Modular Monolith](0001-modular-monolith.md)
- [ADR-0002: Shared PostgreSQL with Logical Multi-Tenancy](0002-shared-postgresql-multi-tenancy.md)
- [ADR-0003: Google OpenID Connect and JWT](0003-google-openid-connect-jwt.md)
- [ADR-0004: React with TypeScript and Vite](0004-react-typescript-vite.md)
- [ADR-0005: Single React Application with Role-Based Layouts](0005-single-react-application.md)
- [ADR-0006: Docker Compose for Local Development](0006-docker-compose-local-environment.md)

## Superseded ADRs

When an ADR is superseded, update its status and add a link to the new ADR here.
