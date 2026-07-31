# Agent Roles

## Architect

**Responsibilities**

- Define and maintain the modular monolith structure.
- Review cross-cutting changes (auth, multi-tenancy, concurrency, deployment).
- Ensure domain model integrity.
- Resolve trade-offs and document decisions in `.ai-kit/docs` or ADRs.

**Constraints**

- Does not write production code directly unless the task is a structural scaffold.
- Must coordinate with Backend and Frontend Engineers before changing contracts.

**Commands**

- `architect review <scope>` — review a design proposal.
- `architect propose <topic>` — propose a design for a new module or refactor.

## Backend Engineer

**Responsibilities**

- Implement API controllers, application handlers, domain logic, repositories, and migrations.
- Ensure conflict detection, authorization, validation, and audit logging.
- Write unit, integration, and architecture tests.

**Focus areas**

- .NET, EF Core, PostgreSQL, xUnit, FluentValidation, MediatR-style handlers.
- Domain-driven design: entities, value objects, domain events.

**Commands**

- `backend implement <story>` — implement a backend user story.
- `backend test <area>` — add or run tests for a backend area.
- `backend migrate <name>` — create and validate an EF Core migration.

## Frontend Engineer

**Responsibilities**

- Build pages, layouts, components, forms, and API integration.
- Implement responsive, mobile-first UI.
- Write unit and end-to-end tests.

**Focus areas**

- React, TypeScript, Vite, React Router, TanStack Query, React Hook Form, Zod, MUI.

**Commands**

- `frontend implement <story>` — implement a frontend user story.
- `frontend test <area>` — add or run frontend tests.
- `frontend e2e <scenario>` — add or run Playwright scenario.

## QA Engineer

**Responsibilities**

- Define test plans for user stories.
- Add unit, integration, contract, and E2E tests.
- Verify concurrency, authorization, and edge cases.

**Focus areas**

- xUnit, FluentAssertions, Testcontainers, Playwright, React Testing Library, Vitest.

**Commands**

- `qa plan <story>` — generate a test plan.
- `qa run` — execute the relevant test suite.
- `qa coverage <area>` — review coverage for an area.

## DevOps Engineer

**Responsibilities**

- Maintain Docker Compose, CI/CD pipelines, Azure resources, and environments.
- Manage secrets, health checks, and observability.
- Automate deployment, migration, and rollback procedures.

**Commands**

- `devops deploy <env>` — prepare or execute deployment.
- `devops pipeline <name>` — create or update a CI/CD pipeline.
- `devops provision <env>` — provision Azure resources.

## Product Analyst

**Responsibilities**

- Draft and refine user stories, acceptance criteria, and backlog items.
- Map business rules to implementation hints.
- Clarify scope and out-of-scope boundaries.

**Commands**

- `product story <title>` — draft a user story.
- `product refine <story>` — refine acceptance criteria.
- `product backlog <epic>` — organize the backlog.

## Security Auditor

**Responsibilities**

- Review authentication, authorization, input validation, and secret handling.
- Identify OWASP Top 10 risks and propose mitigations.
- Verify audit logging and data protection.

**Commands**

- `security review <scope>` — security review of a change.
- `security threat <feature>` — identify threats for a feature.

## Super Admin (implicit)

A `SuperAdmin` agent may override normal tenant constraints for platform-level operations.
