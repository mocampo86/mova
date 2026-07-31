# Agent Workflows

## Workflow 1: New user story

```text
1. Product Analyst
   └─ draft user story with acceptance criteria, rules, tests, and scope
      in the backlog/ folder following the template in workflows/DEVELOPMENT-WORKFLOW.md

2. Architect
   └─ review against SOLUTION-ARCHITECTURE.md and DOMAIN-MODEL.md
      └─ flag cross-cutting concerns (auth, multi-tenancy, concurrency)

3. Backend Engineer
   └─ implement domain, application, API layers, and tests

4. Frontend Engineer
   └─ implement UI, hooks, and integration with the API

5. QA Engineer
   └─ add integration and E2E tests

6. Security Auditor
   └─ review auth, authorization, input validation, and audit logging

7. DevOps Engineer
   └─ update pipeline or deployment artifacts if needed

8. Merge
   └─ after CI passes and PR is approved
```

## Workflow 2: Bug fix

```text
1. QA Engineer or human reporter
   └─ describe reproduction steps, expected vs actual behavior

2. Architect (if scope is large) or Backend/Frontend Engineer
   └─ identify root cause and minimal fix

3. Implementer
   └─ write the fix and add a regression test

4. QA Engineer
   └─ verify the fix with the regression test

5. Security Auditor (if auth or input related)
   └─ confirm no new vulnerability is introduced

6. Merge
```

## Workflow 3: Database change

```text
1. Backend Engineer
   └─ update domain model and EF Core mapping

2. Architect
   └─ review impact on existing data and queries

3. Backend Engineer
   └─ scaffold migration with descriptive name

4. QA Engineer
   └─ run migration against Testcontainers PostgreSQL in integration tests

5. DevOps Engineer
   └─ plan production migration window and rollback script

6. Merge
```

## Workflow 4: Deployment

```text
1. DevOps Engineer
   └─ prepare environment configuration and secrets

2. Backend Engineer
   └─ build and tag Docker image

3. QA Engineer
   └─ run smoke tests against staging

4. DevOps Engineer
   └─ apply database migrations to target environment

5. DevOps Engineer
   └─ deploy API and frontend

6. QA Engineer
   └─ verify health checks and critical paths
```

## Workflow 5: Security review

```text
1. Security Auditor
   └─ identify assets, threats, and mitigations

2. Implementer
   └─ apply mitigations (validation, auth, audit, rate limiting)

3. QA Engineer
   └─ add tests for the security controls

4. Security Auditor
   └─ approve or request changes
```

## Workflow 6: Refactor

```text
1. Architect
   └─ define refactor scope and acceptance criteria

2. Implementer
   └─ apply the refactor incrementally

3. QA Engineer
   └─ run full test suite and add architecture tests if needed

4. Architect
   └─ approve design consistency
```

## Hand-off rules

- The active agent must produce a concise summary before handing off.
- The next agent must read the summary and relevant `.ai-kit/docs` files.
- If an agent detects a blocker outside its role, it should escalate to the Architect or human lead.

## Decision authority

| Decision | Authority |
|----------|-----------|
| New dependency | Architect |
| Breaking API change | Architect + Product |
| Database schema change | Architect + Backend + DevOps |
| Production deployment | DevOps + QA |
| Security exception | Security Auditor + Architect |
| Scope change | Product + Architect |
