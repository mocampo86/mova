# AI Agents Overview

## Purpose

This project uses AI agents to accelerate development, review, testing, and operations. Each agent has a narrow, well-defined responsibility and consumes the `.ai-kit/docs` knowledge base before acting.

## Agent taxonomy

| Agent | Focus | When to invoke |
|-------|-------|----------------|
| **Architect** | High-level design, trade-offs, cross-cutting concerns | Before large changes or new modules |
| **Backend Engineer** | .NET, EF Core, API, domain logic | Backend features and fixes |
| **Frontend Engineer** | React, TypeScript, UI/UX | Frontend features and fixes |
| **QA Engineer** | Tests, edge cases, test plans | New critical paths or bug fixes |
| **DevOps Engineer** | CI/CD, infrastructure, Docker, Azure | Deployment, pipelines, env changes |
| **Product Analyst** | Requirements, acceptance criteria, backlog | Grooming, story refinement |
| **Security Auditor** | Auth, authorization, data protection | Security-sensitive changes |

## Agent behavior contract

Every agent in this project must:

1. Read the relevant `.ai-kit/docs` files before proposing changes.
2. Prefer minimal, focused edits over broad refactors.
3. Follow the `workflows/DEVELOPMENT-WORKFLOW.md`.
4. Add or update tests for critical paths.
5. Not install dependencies without justification.
6. Not expose secrets in code or logs.
7. Run build and tests before declaring a task complete.
8. Update documentation when a decision changes.

## Shared knowledge base

All agents should start with:

- `architecture/SOLUTION-ARCHITECTURE.md`
- `architecture/DOMAIN-MODEL.md`
- `architecture/API-DESIGN.md`
- `workflows/DEVELOPMENT-WORKFLOW.md`

Then read the domain-specific file that matches the task:

- Auth changes → `architecture/AUTHENTICATION.md`
- Database changes → `architecture/DATABASE-DESIGN.md`
- Deployment changes → `architecture/DEPLOYMENT.md`
- Multi-tenancy changes → `architecture/MULTI-TENANCY.md`

## Collaboration model

Agents may invoke each other through explicit hand-offs or workflow orchestration. For example:

1. `Product Analyst` drafts a user story.
2. `Architect` reviews design impact.
3. `Backend Engineer` implements the API and tests.
4. `Frontend Engineer` consumes the API in the UI.
5. `QA Engineer` adds integration tests and Playwright scenarios.
6. `Security Auditor` reviews authorization and input handling.

## Agent output format

Agents should produce output in concise, structured Markdown:

- Summary of changes
- Files modified
- Tests added or run
- Risks or follow-ups
- Commands to validate

## Naming and invocation

Agents can be invoked by role names, slash commands, or workflow triggers defined in `AGENT-COMMANDS.md` and `AGENT-WORKFLOWS.md`.
