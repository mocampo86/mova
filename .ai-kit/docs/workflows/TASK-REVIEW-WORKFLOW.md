# Task Review & Gap Analysis Workflow

## Purpose

Use this workflow to perform a deep technical and functional review of a completed task, pull request, or feature implementation.

The objective is to identify:
- Functional gaps
- Missing acceptance criteria
- Incorrect business rules
- Bugs and edge cases
- Security and authorization issues
- Data integrity and concurrency problems
- Performance issues
- Architecture violations
- Code quality issues
- Missing or weak tests
- Documentation/configuration gaps
- API contract inconsistencies
- Regression risks
- CI/CD issues

The reviewer must be skeptical and actively try to find problems instead of assuming the implementation is correct.

---

## 1. Review Inputs

Before reviewing, inspect:

1. User story / issue / task description
2. Objective and business requirements
3. Functional description
4. Acceptance criteria
5. Business rules
6. Validations
7. Error cases
8. Security considerations
9. Technical notes
10. Required tests
11. Out-of-scope requirements
12. Dependencies
13. Definition of Done
14. Relevant `.ai-kit/docs`
15. Existing architecture and coding conventions
16. Complete git diff
17. Related tests
18. Related API contracts
19. Database migrations, if applicable
20. Frontend changes, if applicable

Do not review the implementation in isolation.

---

## 2. Scope Analysis

Determine:
- What problem is the task supposed to solve?
- What functionality was requested?
- What files/components/layers should be affected?
- What should not be affected?
- What dependencies exist?
- What assumptions does the implementation make?

Identify whether the implementation is complete, partial, over-engineered, under-engineered, or implemented in the wrong layer.

If requirements are ambiguous, explicitly identify the ambiguity instead of silently assuming behavior.

---

## 3. Requirement vs Implementation

For every acceptance criterion determine:
- Is it implemented?
- Is it fully implemented?
- Is it implemented only for the happy path?
- Are edge cases covered?
- Is it tested?
- Could it regress existing functionality?

Create a requirement coverage matrix:

| Requirement | Implementation | Tests | Status | Findings |
|---|---|---|---|---|
| Criterion | Yes/Partial/No | Yes/Partial/No | PASS/GAP/FAIL | Details |

Do not consider a criterion complete merely because related code exists.

---

## 4. Functional Review

Review from the user's and business perspective.

Check:
- Incorrect business logic
- Missing scenarios
- Incorrect state transitions
- Incorrect calculations
- Filtering/sorting/pagination
- Date/time and timezone handling
- Duplicate operations
- Idempotency
- Missing or incorrect validation
- Null/default handling
- Incorrect error behavior
- Unexpected side effects
- Permissions
- Partial updates
- Data loss
- Inconsistent create/update/delete behavior

Ask:
> How could a real user cause this implementation to behave incorrectly?

Provide concrete scenarios for meaningful issues.

---

## 5. Edge Cases

Actively consider:

### Input
- null
- empty/whitespace
- minimum/maximum values
- negative values
- extremely large values
- invalid formats
- duplicates
- unexpected characters
- malformed requests

### State
- entity does not exist
- entity already exists
- entity deleted/inactive
- already processed
- concurrent modification
- stale data
- partially completed operation

### Concurrency
Check for:
- Race conditions
- Duplicate processing
- Lost updates
- Double submission
- Concurrent inserts/updates
- Missing optimistic concurrency
- Missing database constraints
- Non-atomic operations

### External dependencies
Check behavior for:
- Unavailability
- Timeout
- 4xx
- 5xx
- Malformed/empty responses
- Slow dependencies
- Retries
- Duplicate side effects caused by retries

---

## 6. Backend Architecture

Verify compliance with Clean Architecture:
- Domain
- Application
- Infrastructure
- API
- Contracts

Check that:
- Controllers are thin.
- Business logic lives in Application handlers and/or Domain entities.
- Domain logic is not in controllers or repositories.
- Infrastructure concerns do not leak into Domain.
- DTOs/contracts are appropriate.
- Dependencies flow correctly.
- Application logic is testable.

Identify architecture violations, tight coupling, unnecessary/missing abstractions, duplication, God classes/methods, and hidden side effects.

---

## 7. API Review

For API changes verify:
- HTTP method
- Route
- Status codes
- Request/response contracts
- Validation
- Authorization/authentication
- Error responses
- Problem Details / `Result<T>` conventions
- Serialization
- Nullable vs non-nullable fields
- Backward compatibility
- Pagination/filtering/sorting
- Idempotency

Check for breaking changes and consistency with existing endpoints.

---

## 8. Validation Review

The project uses FluentValidation for backend input validation.

Verify:
- Required fields
- Length limits
- Numeric limits
- Enum validation
- Cross-field validation
- Business validation
- Duplicate validation
- Nested object validation
- Collection validation

Do not assume a validator is active merely because a validator class exists. Verify that it is actually executed.

---

## 9. Database Review

If persistence changes exist, inspect:
- Entities
- EF Core configuration
- Relationships
- Foreign keys
- Indexes
- Unique constraints
- Nullable columns
- Defaults
- Transactions
- Migrations
- Seed data
- Cascade behavior

Check for:
- Missing/incorrect migrations
- Data loss
- Missing indexes
- N+1 queries
- Excessive database calls
- Incorrect joins
- Tracking issues
- Missing transaction boundaries
- Duplicate records
- Referential integrity problems

Ask:
> What happens to existing production data when this change is deployed?

---

## 10. Security Review

Perform an explicit security review.

Check:
- Authentication
- Authorization
- Object-level authorization
- Resource ownership
- Injection risks
- Unsafe deserialization
- Path traversal
- SSRF
- XSS
- SQL injection
- Command injection
- Secrets/tokens/API keys
- Sensitive data in logs
- Excessive data exposure
- Mass assignment
- Insecure defaults
- Rate limiting where relevant

Treat real security vulnerabilities as high priority.

---

## 11. Frontend Review

If frontend changes exist, verify:
- Responsive/mobile-first behavior
- Feature-folder architecture
- TanStack Query usage
- Query invalidation
- Loading/error/empty states
- React Hook Form
- Zod validation
- Accessibility
- Keyboard navigation
- Reusable typed components
- State management
- Prop drilling

For new pages verify route, authorization, navigation, and `src/mova-web/README.md` updates.

---

## 12. Frontend/Backend Integration

Trace the complete flow:

Frontend -> HTTP -> API -> Application -> Domain -> Infrastructure -> Database -> Response -> UI

Check:
- Property naming
- Types
- Nullable fields
- Status codes
- Error handling
- Query invalidation
- Caching
- Loading states
- Race conditions
- Stale UI state

---

## 13. Test Review

Do not only verify that tests pass. Assess whether tests prove correctness.

### Unit tests
Check happy path, invalid input, boundaries, business rules, errors, nulls, and authorization.

### Integration tests
Check database, API, persistence, transactions, and relevant dependencies.

### E2E tests
Check critical user journeys.

Look for tests that:
- Assert too little
- Only verify status codes
- Mock the behavior under test
- Ignore important side effects
- Ignore failure paths

Identify missing tests for critical paths.

---

## 14. Regression Analysis

Determine what existing functionality could be affected.

Inspect:
- Existing tests
- Related services
- Shared components
- Validators
- Queries
- Database entities
- API contracts
- Frontend consumers

Ask:
> What existing behavior could this change accidentally break?

---

## 15. Performance Review

Look for:
- N+1 queries
- Excessive DB calls
- Large materializations
- Missing pagination
- Inefficient LINQ
- Repeated API calls
- Unnecessary rendering
- Large payloads
- Blocking operations
- Unsafe sequential operations
- Memory-heavy processing
- Missing caching where clearly appropriate

Do not recommend optimization without reasonable justification.

---

## 16. Error Handling & Resilience

Verify:
- Exceptions handled at the correct layer
- Expected errors represented consistently
- Unexpected exceptions not swallowed
- Useful diagnostic logs
- No sensitive data in logs
- External failures handled appropriately
- Retry policies used appropriately
- Retries do not duplicate side effects
- Appropriate timeouts

---

## 17. Observability

For important operations check:
- Structured logging
- Correlation/request IDs
- Useful diagnostics
- Metrics where appropriate
- Audit logging for security-sensitive actions

Avoid excessive/noisy logging.

---

## 18. Documentation

Check whether changes require updates to:
- README
- `.ai-kit/docs`
- ADRs
- API documentation
- Architecture documentation
- Configuration/environment variables
- Deployment documentation

Do not require documentation changes when genuinely unnecessary.

---

## 19. Git / PR Review

Inspect:
- Branch origin
- Changed files
- Unrelated modifications
- Accidental files
- Debug code
- Temporary code
- Commented-out code
- Generated files
- Secrets
- Commit quality
- PR description
- Story/issue reference

Flag changes unrelated to the task.

---

## 20. Automated Validation

Run applicable commands.

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

### PostgreSQL
```powershell
docker compose up
```

Never claim a command passed unless it was actually executed.

Report each validation as:
- Executed and passed
- Executed and failed
- Not executed
- Unable to execute

---

## 21. Finding Classification

Every finding must have:

### Severity
- **CRITICAL** — Security vulnerability, data loss, production outage, severe corruption, or major functional failure.
- **HIGH** — Significant functional bug, authorization issue, serious regression, or important missing requirement.
- **MEDIUM** — Incorrect behavior in specific conditions, important missing validation/test, architecture problem, or maintainability concern.
- **LOW** — Minor issue, code quality concern, documentation gap, or small improvement.
- **INFO** — Observation or recommendation that is not a defect.

### Confidence
- **HIGH**
- **MEDIUM**
- **LOW**

Do not report speculative issues as definite bugs.

---

## 22. Finding Format

Use:

```text
[SEVERITY] Short title

Location:
<file>:<line or method>

Problem:
<what is wrong>

Why it matters:
<impact>

Scenario:
<concrete example>

Recommendation:
<specific recommendation>

Confidence:
<HIGH | MEDIUM | LOW>
```

Findings must be specific and actionable.

Avoid vague comments such as "this could be improved."

---

## 23. False Positive Prevention

Before reporting a finding:

1. Verify the relevant code.
2. Search for safeguards elsewhere.
3. Check other layers.
4. Check existing tests.
5. Check configuration.
6. Check documentation.
7. Determine whether behavior is intentional.

If uncertain, mark confidence LOW and explain why.

---

## 24. Reviewer Mindset

Act as a senior engineer performing a production-readiness review.

Do not assume:
- The developer interpreted requirements correctly.
- Passing tests mean correctness.
- Existing code is correct.
- Happy-path tests are sufficient.
- Validators are actually executed.
- Authorization is correct.
- Database changes are safe.
- Frontend/backend contracts match.
- Error handling is complete.

Actively try to break the implementation.

Ask:
- What happens in production?
- What happens with unexpected input?
- What happens under concurrency?
- What happens when a dependency fails?
- What happens with existing data?
- What happens when the user is unauthorized?
- What happens when the operation is repeated?
- What requirement might have been missed?

---

## 25. Review Principle

The goal is not to maximize the number of findings.

The goal is to identify real, actionable issues affecting:
- Correctness
- Security
- Maintainability
- Reliability
- Performance
- Production readiness

A good review must clearly explain:
1. What was requested.
2. What was implemented.
3. What was verified.
4. What is correct.
5. What is missing.
6. What should be fixed before release.

---

## 26. Final Report

Produce the following structure:

# Review Summary

## Executive Summary
Brief assessment of completeness and quality.

## Overall Status
One of:
- APPROVED
- APPROVED WITH COMMENTS
- CHANGES REQUESTED
- BLOCKED

## Requirement Coverage
Requirement-by-requirement analysis.

## Findings
Ordered:
1. CRITICAL
2. HIGH
3. MEDIUM
4. LOW
5. INFO

## Security Findings
Explicit security assessment.

## Test Assessment
Include:
- Tests executed
- Tests passed
- Tests failed
- Missing tests
- Risk areas without coverage

## Architecture Assessment
Explain compliance with project architecture.

## Regression Risks
List potential affected areas.

## Documentation Assessment
Identify missing documentation.

## Positive Findings
Mention important things implemented correctly.

## Validation Results
Clearly distinguish executed/passed/failed/not executed.

## Final Verdict

Example:

VERDICT: CHANGES REQUESTED

CRITICAL: 0
HIGH: 1
MEDIUM: 2
LOW: 3
INFO: 1

Main blocking issue:
<short description>

---

## 27. Review Completion Checklist

- [ ] Requirements reviewed
- [ ] Acceptance criteria checked
- [ ] Business rules checked
- [ ] Validation reviewed
- [ ] Error cases reviewed
- [ ] Security reviewed
- [ ] Authorization reviewed
- [ ] Backend architecture reviewed
- [ ] Frontend architecture reviewed
- [ ] API contracts reviewed
- [ ] Database changes reviewed
- [ ] Migrations reviewed
- [ ] Edge cases reviewed
- [ ] Concurrency reviewed
- [ ] Performance reviewed
- [ ] Tests reviewed
- [ ] Missing tests identified
- [ ] Regression risks assessed
- [ ] Documentation reviewed
- [ ] Git diff reviewed
- [ ] Build executed where applicable
- [ ] Tests executed where applicable
- [ ] Lint executed where applicable
- [ ] Findings classified by severity
- [ ] Findings assigned confidence
- [ ] False positives checked
- [ ] Final verdict produced

---

## 28. Important Rule

Do not modify the implementation during the review unless explicitly requested.

Default behavior:

Inspect -> Analyze -> Test -> Find Issues -> Report

Do not silently fix findings.
