# Epic Review & Audit Workflow

## Purpose

Use this workflow to perform a complete audit of an entire Epic after its implementation.

The objective is to determine whether the Epic is:
- Functionally complete
- Consistent with its requirements
- Secure
- Architecturally sound
- Correctly integrated across all stories
- Sufficiently tested
- Production-ready
- Properly documented
- Free of significant gaps, regressions, and cross-story inconsistencies

This is an Epic-level review, not a single PR or task review.

The reviewer must evaluate the Epic as a complete product capability and actively search for problems that may only become visible when multiple stories are considered together.

Do not assume that an Epic is correct because every individual task or PR passed review.

---

## 1. Review Inputs

Before starting the audit, identify and inspect:

1. Epic description
2. Epic objective
3. Business requirements
4. All stories/tasks belonging to the Epic
5. Acceptance criteria for every story
6. Business rules
7. Validations
8. Error cases
9. Security requirements
10. Technical requirements
11. Dependencies
12. Out-of-scope items
13. Definition of Done
14. Relevant `.ai-kit/docs`
15. Existing architecture
16. Existing coding conventions
17. Git history and relevant diffs
18. Backend implementation
19. Frontend implementation
20. API contracts
21. Database schema and migrations
22. Tests
23. CI/CD configuration
24. Documentation

Do not review only the most recent PR. Consider the complete Epic scope.

---

## 2. Epic Scope Reconstruction

Before evaluating the implementation, reconstruct the intended Epic.

Determine:
- What business problem does the Epic solve?
- What is the expected end-to-end user journey?
- What capabilities should exist when the Epic is complete?
- Which stories contribute to each capability?
- Which stories depend on other stories?
- Which business rules span multiple stories?
- Which components are shared?
- Which APIs, database entities, and frontend features are involved?

Create a conceptual dependency map:

Epic
|
+-- Story A
|   +-- Backend
|   +-- Frontend
|   +-- Database
|
+-- Story B
|   +-- Backend
|   +-- Frontend
|
+-- Story C
    +-- Integration
    +-- Tests

Identify missing or unclear relationships.

---

## 3. Requirements Coverage

Build a complete requirements matrix.

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| Requirement | Story ID | Complete/Partial/Missing | Yes/Partial/No | PASS/GAP/FAIL | Details |

For every requirement determine:
- Is it implemented?
- Is it fully implemented?
- Is it implemented consistently across the Epic?
- Is it tested?
- Is it integrated with dependent functionality?
- Are edge cases covered?
- Is behavior consistent with business rules?

Do not consider a requirement complete merely because a related task exists.

---

## 4. Cross-Story Gap Analysis

This is one of the most important parts of the review.

Analyze interactions between stories.

Look for:
- Story A creates data that Story B cannot consume correctly.
- Story B assumes behavior not guaranteed by Story A.
- Story C introduces a state that Story A/B do not handle.
- One story validates something differently from another.
- Different stories use inconsistent authorization rules.
- Different endpoints expose inconsistent behavior.
- One story changes a shared component and breaks another.
- State transitions are incomplete.
- Error handling differs between related operations.
- Create/update/delete flows are inconsistent.
- Business rules are enforced in one path but bypassed in another.
- Different stories use incompatible data models.
- Frontend behavior does not match backend behavior.

Ask:

> What happens when all stories are executed together in the real user flow?

---

## 5. End-to-End Business Flow

Trace the Epic from the user's perspective.

For each major flow:

Frontend
-> API
-> Application
-> Domain
-> Infrastructure
-> Database
-> Response
-> Frontend
-> Next user action

Verify:
- Correct state transitions
- Correct data propagation
- Correct permissions
- Correct validation
- Correct error handling
- Correct persistence
- Correct UI state
- Correct navigation
- Correct side effects

Identify broken or incomplete flows.

---

## 6. Functional Completeness

Review the Epic as a complete feature.

Check:
- Happy paths
- Alternative paths
- Failure paths
- Empty states
- Loading states
- Partial completion
- Retry scenarios
- Cancellation
- Repeated operations
- Duplicate operations
- Invalid state transitions
- Unauthorized access
- Resource ownership
- Deleted/inactive entities
- Existing data

Ask:

> If a real user starts this Epic and follows every reasonable path, where can the experience break?

---

## 7. Business Rule Consistency

Create a list of business rules used throughout the Epic.

For each rule verify:
- Where is it implemented?
- Is it implemented exactly once or duplicated?
- Is it enforced consistently?
- Can it be bypassed through another endpoint/path?
- Is it tested?
- Does frontend behavior match backend behavior?

Pay special attention to rules spanning multiple stories:
- Status transitions
- Availability
- Ownership
- Permissions
- Limits
- Dates
- Conflicts
- Uniqueness
- Cancellation
- Expiration
- State-dependent operations

---

## 8. Security Audit

Perform an Epic-wide security review.

### Authentication
Verify that all relevant entry points require the correct authentication.

### Authorization
Check:
- Roles
- Scopes
- Permissions
- Resource ownership
- Object-level authorization
- Administrative access
- Cross-user access
- Cross-tenant access if applicable

### Data exposure
Check whether any endpoint exposes:
- Sensitive fields
- Internal identifiers
- Other users' data
- Administrative information
- Secrets
- Internal errors

### Input security
Check for:
- SQL injection
- XSS
- SSRF
- Path traversal
- Command injection
- Unsafe deserialization
- Mass assignment
- Unsafe file handling

A secure endpoint does not make the Epic secure if another related endpoint bypasses authorization.

---

## 9. Architecture Audit

Evaluate the entire Epic against the project's Clean Architecture:
- Domain
- Application
- Infrastructure
- API
- Contracts

Check:
- Dependency direction
- Layer boundaries
- Business logic placement
- Controller responsibilities
- Repository responsibilities
- Domain responsibilities
- Application handlers
- DTO/contracts
- Infrastructure isolation
- Shared abstractions

Look for:
- Duplicate business logic
- Multiple competing abstractions
- Inconsistent patterns between stories
- Cross-layer leakage
- Business logic in controllers
- Domain logic in repositories
- Infrastructure dependencies in Domain
- Shared components becoming overly coupled
- Unnecessary abstractions
- God services
- Circular dependencies

Determine whether the Epic leaves the codebase more coherent or less coherent.

---

## 10. API Consistency Audit

Review all APIs introduced or modified by the Epic.

Check consistency of:
- Routes
- HTTP methods
- Naming
- Request contracts
- Response contracts
- Status codes
- Validation
- Error responses
- Problem Details / `Result<T>`
- Pagination
- Filtering
- Sorting
- Idempotency
- Authorization

Look for APIs that represent the same business concept differently.

Identify breaking changes and compatibility risks.

---

## 11. Database & Data Model Audit

Review the complete data model affected by the Epic.

Inspect:
- Entities
- Relationships
- Foreign keys
- Indexes
- Unique constraints
- Nullable fields
- Defaults
- EF Core configurations
- Migrations
- Seed data
- Cascade behavior
- Transactions

Check:
- Data integrity
- Existing production data
- Migration safety
- Duplicate records
- Orphaned records
- Referential integrity
- Race conditions
- Concurrency
- N+1 queries
- Missing indexes
- Excessive DB calls

Ask:

> Can the complete Epic preserve data integrity under real production usage?

---

## 12. Frontend Audit

If the Epic includes frontend functionality, review the complete user experience.

Check:
- Routing
- Authorization
- Responsive/mobile-first behavior
- Feature-folder architecture
- TanStack Query
- Query invalidation
- Caching
- React Hook Form
- Zod validation
- Loading states
- Error states
- Empty states
- Success states
- Accessibility
- Keyboard navigation
- Reusable components
- State management
- Prop drilling

Verify that the frontend represents every meaningful backend state.

Look for states that exist in the backend but are impossible to represent correctly in the UI.

---

## 13. Frontend / Backend Contract Audit

Compare all frontend consumers against backend contracts.

Check:
- Property names
- Types
- Nullable fields
- Enums
- Status codes
- Error contracts
- Pagination
- Filtering
- Sorting
- Date/time formats
- Validation rules

Identify mismatches that may not be detected by unit tests.

---

## 14. Test Strategy Audit

Evaluate test coverage for the Epic as a whole.

Do not count tests. Evaluate whether tests prove important behavior.

### Unit Tests
Verify:
- Business rules
- Validation
- State transitions
- Edge cases
- Errors
- Authorization

### Integration Tests
Verify:
- API behavior
- Database behavior
- Persistence
- Transactions
- Cross-component integration

### E2E Tests
Verify critical user journeys across multiple stories.

Look specifically for missing tests that combine multiple stories.

Example:

Story A works.
Story B works.
Story C works.

But:

A -> B -> C

may still fail.

This is an Epic-level gap.

---

## 15. Regression Analysis

Identify existing functionality potentially affected by the Epic.

Inspect:
- Shared services
- Shared components
- Shared validators
- Shared queries
- Database entities
- API contracts
- Existing frontend flows
- Existing tests

Ask:

> What existing functionality could this Epic accidentally break?

Prioritize regressions in shared components and shared business rules.

---

## 16. Concurrency & Distributed Behavior

Where applicable, evaluate:
- Concurrent requests
- Duplicate submissions
- Race conditions
- Lost updates
- Optimistic concurrency
- Transactions
- Retry behavior
- Idempotency
- Background jobs
- Queue processing
- Eventual consistency
- Duplicate events
- Out-of-order events

Check whether the Epic remains correct when operations happen simultaneously.

---

## 17. Error Handling & Resilience

Review error behavior consistently across the Epic.

Check:
- Validation errors
- Not found
- Unauthorized
- Forbidden
- Conflict
- Dependency failure
- Timeout
- Database failure
- Unexpected exceptions
- Retry behavior
- Partial failures

Verify that related operations use consistent error semantics.

---

## 18. Performance Audit

Evaluate the complete Epic for:
- N+1 queries
- Excessive database access
- Large payloads
- Missing pagination
- Inefficient queries
- Repeated API calls
- Excessive frontend rendering
- Blocking operations
- Memory-heavy processing
- Unnecessary network requests
- Missing caching where clearly appropriate

Consider cumulative performance.

A single endpoint may be acceptable while the complete user flow performs ten expensive requests.

---

## 19. Observability Audit

For important Epic operations check:
- Structured logging
- Correlation/request IDs
- Diagnostic information
- Metrics where appropriate
- Audit logging
- Security event logging

Determine whether production support teams could diagnose failures in the Epic.

---

## 20. Documentation & Operational Readiness

Check:
- README
- `.ai-kit/docs`
- ADRs
- API documentation
- Architecture documentation
- Environment variables
- Configuration
- Deployment requirements
- Migration instructions
- Operational/runbook information

Verify that the Epic can be understood and operated by someone who did not implement it.

---

## 21. CI/CD & Release Readiness

Verify:
- Backend build
- Backend tests
- Frontend build
- Frontend tests
- Lint
- Integration tests
- Database migrations
- Deployment configuration
- Environment configuration
- Feature flags where applicable
- CI pipeline

Use the project's existing validation commands where applicable:

```powershell
# Backend
cd src/Mova.Api
dotnet build
dotnet test

# Frontend
cd src/mova-web
npm ci
npm run lint
npm run build
npm run test
```

Never claim validation passed unless it was actually executed.

Report:
- Executed and passed
- Executed and failed
- Not executed
- Unable to execute

---

## 22. Production Readiness

Evaluate whether the Epic is safe to release.

Check:
- Functional completeness
- Security
- Data integrity
- Performance
- Error handling
- Observability
- Migration safety
- Rollback considerations
- Configuration
- Backward compatibility
- Operational support

Ask:

> What could go wrong immediately after deployment?

---

## 23. Finding Classification

Every finding must have:

### Severity
- **CRITICAL** — Severe security vulnerability, data loss, production outage, severe corruption, or major functional failure.
- **HIGH** — Significant functional bug, authorization issue, serious regression, major architectural issue, or important missing requirement.
- **MEDIUM** — Important edge case, missing validation/test, moderate architecture issue, maintainability problem, or meaningful production risk.
- **LOW** — Minor defect, code quality issue, documentation gap, or low-risk improvement.
- **INFO** — Observation or recommendation that is not a defect.

### Confidence
- **HIGH**
- **MEDIUM**
- **LOW**

Do not present speculation as fact.

---

## 24. Finding Format

Use:

```text
[SEVERITY] Short title

Category:
<Requirements | Functional | Security | Architecture | API | Database |
Frontend | Testing | Performance | Reliability | Observability |
Documentation | CI/CD | Regression>

Affected stories:
<Story IDs>

Location:
<file>:<line or method>

Problem:
<what is wrong>

Why it matters:
<impact>

Scenario:
<concrete scenario demonstrating the issue>

Recommendation:
<specific recommendation>

Confidence:
<HIGH | MEDIUM | LOW>
```

Findings must be specific and actionable.

---

## 25. Cross-Story Findings

Explicitly identify findings that cannot be detected by reviewing a single story.

Use the label:

`CROSS-STORY`

Examples:
- Inconsistent state transition
- Conflicting business rules
- Broken integration
- Missing handoff between stories
- Inconsistent authorization
- Incompatible API contracts
- Missing E2E coverage
- Data lifecycle inconsistency

These findings are especially important for Epic review.

---

## 26. False Positive Prevention

Before reporting a finding:

1. Verify the relevant implementation.
2. Search for safeguards elsewhere.
3. Inspect related stories.
4. Inspect related tests.
5. Inspect configuration.
6. Inspect documentation.
7. Determine whether behavior is intentional.
8. Check whether another component handles the scenario.

If uncertainty remains, mark confidence LOW.

Do not generate findings merely to increase the finding count.

---

## 27. Epic Quality Score

Calculate an approximate Epic score using these dimensions:

| Dimension | Score |
|---|---:|
| Requirements completeness | 0-100 |
| Functional correctness | 0-100 |
| Security | 0-100 |
| Architecture | 0-100 |
| API consistency | 0-100 |
| Database/data integrity | 0-100 |
| Frontend | 0-100 |
| Testing | 0-100 |
| Performance | 0-100 |
| Observability | 0-100 |
| Documentation | 0-100 |
| Production readiness | 0-100 |

Calculate an overall score only when sufficient evidence exists.

Do not invent precision.

Use the score as a risk indicator, not as proof of correctness.

---

## 28. Final Epic Report

Produce the following structure:

# Epic Review

## Epic
<Epic ID and title>

## Executive Summary

Summarize:
- What was reviewed
- Overall implementation quality
- Major risks
- Major gaps
- Production readiness

## Overall Verdict

One of:
- **APPROVED**
- **APPROVED WITH COMMENTS**
- **CHANGES REQUESTED**
- **BLOCKED**

### Verdict criteria

**APPROVED**
- No meaningful issues found.

**APPROVED WITH COMMENTS**
- Only low-risk findings or recommendations.

**CHANGES REQUESTED**
- One or more MEDIUM/HIGH findings should be fixed.

**BLOCKED**
- CRITICAL issue, severe security issue, major data integrity issue, or major functional failure.

---

## Epic Completeness

Provide an approximate percentage only when evidence supports it.

Example:

Implementation completeness: 88%

Explain what is missing.

---

## Requirements Coverage

Provide the requirement matrix.

---

## Findings

Order by:
1. CRITICAL
2. HIGH
3. MEDIUM
4. LOW
5. INFO

Clearly mark CROSS-STORY findings.

---

## Security Assessment

Include:
- Authentication
- Authorization
- Data exposure
- Input security
- Security consistency
- Security findings

---

## Architecture Assessment

Explain:
- Clean Architecture compliance
- Layer boundaries
- Shared components
- Coupling
- Duplication
- Architectural risks

---

## Functional Assessment

Explain:
- Complete flows
- Missing flows
- Edge cases
- State transitions
- Business rule consistency

---

## Testing Assessment

Include:
- Unit tests
- Integration tests
- E2E tests
- Cross-story tests
- Missing coverage
- Tests executed and results

---

## Data & Database Assessment

Include:
- Schema
- Migrations
- Data integrity
- Concurrency
- Performance
- Production migration risk

---

## Frontend Assessment

Include:
- UX flows
- State handling
- API integration
- Accessibility
- Responsive behavior

---

## Performance Assessment

Include:
- Backend
- Database
- Frontend
- End-to-end flow

---

## Observability Assessment

Include:
- Logging
- Metrics
- Audit
- Diagnostics

---

## Regression Risks

List existing functionality that may be affected.

---

## Documentation Assessment

List missing or outdated documentation.

---

## Positive Findings

Explicitly mention what was implemented well.

Do not make the report exclusively negative.

---

## Validation Results

List every command or validation actually executed.

Example:

Backend build: PASSED
Backend tests: PASSED
Frontend build: PASSED
Frontend tests: NOT EXECUTED
Lint: PASSED

---

## Epic Score

Provide the score table when sufficient evidence exists.

---

## Final Verdict

Example:

VERDICT: CHANGES REQUESTED

CRITICAL: 0
HIGH: 2
MEDIUM: 4
LOW: 5
INFO: 3

Main risks:
1. <risk>
2. <risk>
3. <risk>

Main missing requirements:
1. <gap>
2. <gap>

Main cross-story issue:
<issue>

---

## 29. Reviewer Mindset

Act as a Senior Engineer / Tech Lead / Security Reviewer performing a production-readiness audit.

Do not assume:
- Every story was implemented correctly.
- Every story is compatible with the others.
- Passing tests prove Epic correctness.
- Existing code is correct.
- Happy paths are sufficient.
- Authorization is consistent.
- Database changes are safe.
- Frontend and backend contracts match.
- Error handling is complete.
- The Epic is complete because all tickets are marked Done.

Actively try to break the Epic.

Think in terms of:

Requirements -> Stories -> Components -> Integration -> User Journey -> Production

Ask:
- What requirement could have been missed?
- What happens when all stories interact?
- What happens when users perform operations in an unexpected order?
- What happens when two users act simultaneously?
- What happens when dependencies fail?
- What happens with existing production data?
- What happens when a user is unauthorized?
- What happens when an operation is repeated?
- What happens when one story changes behavior expected by another?

---

## 30. Important Rule

Do not modify the implementation during the review unless explicitly requested.

Default behavior:

Inspect -> Reconstruct Epic -> Analyze -> Test -> Find Issues -> Validate Findings -> Report

Do not silently fix findings.

The purpose of this workflow is to produce an objective Epic-level audit, not to implement fixes.
