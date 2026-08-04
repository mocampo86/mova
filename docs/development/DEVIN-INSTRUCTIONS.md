# Devin Instructions

This document explains how to write, submit, and validate tasks for Devin and other AI agents working on Mova (MOVA).

## Before assigning a task

Every task handed to an agent must include:

1. **Concrete objective**: what the agent must deliver.
2. **Functional context**: who uses the feature and why.
3. **Project and affected files**: where changes should happen.
4. **Acceptance criteria**: verifiable outcomes.
5. **Business rules**: constraints and invariants.
6. **Error cases**: expected behavior when things go wrong.
7. **Security considerations**: auth, authorization, data protection.
8. **Tests required**: unit, integration, E2E.
9. **Out of scope**: what should not be changed.
10. **Dependencies**: other stories, services, or environments.
11. **Validation commands**: how to build and test.
12. **Definition of Done**: when the task is complete.

## User story template

```markdown
# <Story-ID> — Title

## Objective

## Functional description

## Acceptance criteria
- [ ] Criteria 1
- [ ] Criteria 2

## Business rules

## Validations

## Error cases

## Security considerations

## Technical notes

## Tests required

## Out of scope

## Dependencies

## Definition of Done

## Project affected

## Suggested branch

## Instructions for Devin
```

## Agent context

Devin must read the `.ai-kit/docs` knowledge base before starting:

1. `.ai-kit/docs/README.md`
2. `.ai-kit/docs/architecture/SOLUTION-ARCHITECTURE.md`
3. `.ai-kit/docs/architecture/DOMAIN-MODEL.md`
4. `.ai-kit/docs/architecture/API-DESIGN.md`
5. `.ai-kit/docs/workflows/DEVELOPMENT-WORKFLOW.md`
6. Domain-specific files (`AUTHENTICATION.md`, `DATABASE-DESIGN.md`, etc.) if relevant.

## Implementation rules

- Do not modify functionality outside the task scope.
- Do not add dependencies without justifying the need.
- Maintain separation between `Domain`, `Application`, `Infrastructure`, and `Api`.
- Do not expose persistence entities from controllers.
- Do not store secrets in source control.
- Add EF Core migrations when the data model changes.
- Add or update tests for critical paths.
- Run `dotnet build` and `dotnet test` before finishing backend work.
- Run `npm run lint`, `npm run build`, and `npm run test` before finishing frontend work.
- Document assumptions and limitations in the PR description.

## Validation commands

### Backend

```powershell
cd src/Mova.Api
dotnet build
dotnet test
```

### Frontend

```powershell
cd src/mova-web
npm ci
npm run lint
npm run build
npm run test
```

### Local environment

```powershell
docker compose up -d
dotnet ef database update --project src/Mova.Infrastructure
```

## Task submission format

When a task is complete, the agent should provide:

- Summary of changes.
- Files modified.
- Tests added or run.
- Commands used for validation.
- Known assumptions or limitations.
- Open questions or follow-ups.

## Communication rules

- Escalate blockers to the `architect` agent or the human lead.
- Ask for clarification only when the requirement is genuinely ambiguous.
- Keep updates factual and concise.
