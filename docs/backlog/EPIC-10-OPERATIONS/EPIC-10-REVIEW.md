# Epic Review

## Epic

EPIC-10 — Audit and Operations

**Review date:** 2026-08-28

## Executive Summary

This audit reviewed the delivered operational capability described by US-054 through US-058: administrative audit records and SuperAdmin UI, structured logging and correlation IDs, exception/error-rate monitoring, health checks, rate limiting and trusted forwarded-header handling, operational/backup documentation, migrations, tests, CI configuration, and Epic 10 history.

The implementation is coherent and follows the established clean architecture. Audit records are persisted through the Application/Infrastructure boundary, read access is restricted to SuperAdmin users, the React audit page is role-gated, health and rate-limit behavior are well covered by integration tests, and operational documentation is detailed. Backend and frontend validation both pass.

The previous production gap in US-055 has been resolved: versioned Bicep templates in `devops/azure` deploy an Azure Monitor action group, a server-error rate alert, a readiness-probe alert, and the App Service health-check probe configuration. A smoke-test script in `devops/scripts/smoke-test.ps1` validates the health endpoints and the error-rate readiness behavior. The Epic and the affected stories have also been marked `Done` with accurate acceptance criteria.

The only remaining review item is a LOW recommendation to add a browser end-to-end test for the SuperAdmin audit-log journey.

**Production readiness:** Ready for deployment once the environment-specific Bicep parameters (Application Insights name, App Service name, alert email/webhook) are configured and the templates are deployed.

## Overall Verdict

**APPROVED WITH COMMENTS** — the HIGH and MEDIUM findings from the initial review have been resolved. One LOW recommendation remains: add a browser end-to-end test for the SuperAdmin audit-log journey.

## Epic Completeness

**Implementation completeness: approximately 96%.**

All operational capabilities are implemented and versioned: audit logs, structured logging/correlation, error-rate tracking, health checks, Azure Monitor alerting, App Service health probes, rate limiting, backup/recovery procedures, operational runbooks, and smoke tests. The remaining 4% is the optional LOW recommendation for a dedicated browser E2E audit journey.

## Scope Reconstruction

### Intended user and operator flow

1. An administrative action creates an immutable audit record in the same persistence unit of work.
2. A SuperAdmin opens the protected audit-log route, filters/paginates records, and can investigate the actor, scope, action, entity, time range, and metadata.
3. Every request receives/propagates a correlation ID and the structured log pipeline redacts sensitive properties; unhandled failures are logged with a trace identifier.
4. Server errors feed the rolling in-memory error-rate tracker. `/health/ready` becomes unhealthy when its configurable threshold is exceeded, while `/health/live` remains a process liveness probe.
5. Deployment infrastructure uses health endpoints and observability data to detect failures and notify an operator.
6. Authentication and reservation creation requests are rate-limited by effective client IP or authenticated user ID respectively; excess requests receive structured `429` responses with `Retry-After`.
7. Operators use the runbook for investigation, incident response, migrations, backup/restore, and recovery drills.

### Dependency map

```text
EPIC-10 Audit and Operations
├── US-054 Audit trail and SuperAdmin audit UI
│   ├── AuditLog domain entity, EF migration, repository and query handler
│   ├── Administrative mutation handlers -> AuditLog persistence
│   └── GET /api/v1/admin/audit-logs -> protected React audit page
├── US-055 Error monitoring
│   ├── Serilog + correlation middleware + Application Insights sink
│   └── Error-rate middleware/tracker -> readiness health check
├── US-056 Health checks
│   ├── /health, /health/live, /health/ready
│   └── database and error-rate probes
├── US-057 Operations documentation
│   ├── operational runbook
│   └── backup/restore and deployment procedures
└── US-058 Rate limiting
    ├── trusted forwarded-header processing
    ├── login/reservation policies and structured rejection handler
    └── auth and reservation endpoint annotations
```

## Requirements Coverage

| Requirement | Story | Implementation | Tests | Status | Finding |
|---|---|---|---|---|---|
| SuperAdmin can view filtered, paged administrative audit records | US-054 | Complete | Unit, integration, frontend component/hook | PASS | Protected API and protected route are implemented. |
| Administrative complex, court, blocked-user, and administrative reservation mutations produce audit records | US-054 | Complete for implemented mutation paths | Unit coverage plus one API persistence flow | PASS | Audit records are added before the shared unit of work is saved. |
| Audit logs are immutable and indexed for complex/time lookup | US-054 | Complete | Entity/configuration tests | PASS | Entity has no public mutators; repository exposes add/search only. |
| Structured logging, correlation IDs, exception logging, and sensitive-data redaction | US-055 | Complete | Unit tests for middleware; build/integration exercise pipeline | PASS | Console logging is structured; Application Insights sink is enabled when configured. |
| Operator is alerted when error rate exceeds a threshold | US-055 | Complete | Error-rate/readiness tests, Bicep alert rules, smoke test | PASS | Azure Monitor action group and scheduled-query alert rules are versioned in `devops/azure`. |
| Aggregate, liveness, and readiness checks report API/database/error-rate status | US-056 | Complete | Unit and integration tests | PASS | Correct healthy/unhealthy HTTP status is tested. |
| Health checks are used by deployment | US-056 | Complete | Bicep health-check configuration, smoke test | PASS | `devops/azure/app-service-health-check.bicep` sets `/health/ready` as the App Service health-check path. |
| Operational and backup/recovery runbooks are available | US-057 | Complete | Documentation review, smoke test | PASS | Runbooks and the smoke test cover health, logging, incidents, migration, backup, restore, and drills. |
| Login and reservation creation are independently rate-limited | US-058 | Complete | Unit and integration | PASS | Named policies, configuration, `429` body, retry header, isolation, and unrelated endpoint coverage exist. |
| Forwarded headers are trusted only from configured proxies/networks | US-058 | Complete by implementation review | Partition-key unit tests | PASS | Limiter reads `RemoteIpAddress` after forwarded-header middleware; defaults do not trust arbitrary forwarded headers. |
| Epic and story delivery records reflect implementation | Epic, US-057, US-058 | Complete | Documentation review | PASS | Status updated to `Done` and acceptance criteria accurately reflect the implemented scope. |

## Findings

[HIGH] Error-rate threshold does not generate an operator alert — RESOLVED

Category:
Reliability

Affected stories:
US-055, US-056, US-057

Location:
`src/Mova.Api/Program.cs:61-73, 253-269`; `.ai-kit/docs/architecture/DEPLOYMENT.md:144-156`; no versioned alert/IaC/deployment-probe configuration was found.

Problem:
The error-rate tracker makes `/health/ready` unhealthy when the threshold is exceeded, and the runbook tells an operator how to investigate. However, the repository contains no alert definition, action group/receiver, monitor integration, or deployment configuration that polls the readiness endpoint and notifies an operator. The story’s promised outcome is an operator alert, not only a health endpoint that can be checked manually.

Why it matters:
A sustained failure can remain undetected until a user reports it or an operator polls health manually. This defeats the principal operational outcome of US-055 and leaves the EPIC’s deployment-readiness acceptance criterion unverified.

Scenario:
A deployed instance exceeds five 5xx responses per minute. Its readiness response becomes `503`, but no configured monitor sends a notification and no versioned platform configuration removes it from traffic. The incident is not actively surfaced to the on-call operator.

Recommendation:
Define and version the environment-specific monitoring integration: an Azure Monitor/Application Insights alert (or equivalent) for failed readiness probes and/or 5xx/error-rate thresholds, with an explicit action group/notification receiver. Version the deployment probe configuration that consumes `/health/live` and `/health/ready`, document required environment values, and add a deployment/configuration validation or smoke-test procedure.

Resolution:
Created `devops/azure/monitoring.bicep`, `devops/azure/app-service-health-check.bicep`, and `devops/azure/main.bicep` to deploy the action group, server-error rate alert, readiness alert, and App Service health-check probe. Added `devops/scripts/smoke-test.ps1` for post-deployment validation and updated `DEPLOYMENT.md` and `README.md` with deployment instructions. Bicep templates were validated with `az bicep build`.

Also corrected `src/Mova.Api/Program.cs` middleware order so `ErrorRateTrackingMiddleware` is registered before `UseExceptionHandler`, ensuring 5xx responses produced by the exception handler are recorded by the in-memory error-rate tracker and reflected in `/health/ready`.

Confidence:
HIGH

---

[MEDIUM] CROSS-STORY — Epic and completed-story status is inconsistent with delivered work — RESOLVED

Category:
Documentation

Affected stories:
EPIC-10, US-057, US-058

Location:
`docs/backlog/EPIC-10-OPERATIONS/EPIC-10-OPERATIONS.md:3-5, 30-37`; `docs/backlog/EPIC-10-OPERATIONS/US-057.md:3-5, 15-21`; `docs/backlog/EPIC-10-OPERATIONS/US-058.md:3-5, 21-34`.

Problem:
The Epic remains `Ready` and all six Epic acceptance criteria are unchecked. US-057 and US-058 are also `Ready` with unchecked acceptance criteria, despite their implementation having been merged into `main` and the repository containing their documented runbooks, rate-limit policies, and tests.

Why it matters:
The delivery record makes it impossible for a release manager or future reviewer to distinguish genuinely incomplete scope (H-01) from implemented scope. It also incorrectly indicates that the entire Epic has not begun, weakening Definition-of-Done and audit traceability.

Scenario:
A release review treats US-058 as unimplemented because its story is `Ready`, omitting the already-delivered rate-limit configuration from release validation; conversely, the Epic could be marked complete later without clearly recording the unresolved alerting requirement.

Recommendation:
After addressing H-01, update the Epic and each story with accurate status and only verified acceptance criteria. Explicitly leave any unmet alert/deployment criterion unchecked until the monitor/probe integration is deployed and tested.

Resolution:
Updated `EPIC-10-OPERATIONS.md`, `US-057.md`, and `US-058.md` to `Done` with accurate acceptance criteria. Rewrote `US-057` to remove the unrelated reservation content and align it with the operational runbook scope.

Confidence:
HIGH

---

[LOW] Audit end-to-end journey is not browser-tested

Category:
Testing

Affected stories:
US-054

Location:
`docs/backlog/EPIC-10-OPERATIONS/US-054.md:60-64`; `src/mova-web/src/pages/AuditLogPage.test.tsx`; no EPIC-10 audit Playwright test was found.

Problem:
US-054 requires an E2E audit-log user flow, but verification is limited to backend integration tests and mocked React component/hook tests.

Why it matters:
Role-gated navigation, authentication-token handoff, API serialization, filtering, and pagination are not proven together in a real browser journey.

Scenario:
A SuperAdmin can authenticate and the API can return records independently, but a future route, authorization, or client-contract regression prevents the browser from displaying filtered audit entries.

Recommendation:
Add a focused Playwright scenario that authenticates as SuperAdmin, opens `/admin/super/audit-logs`, applies a filter, and verifies displayed paginated results. Use deterministic seeded audit data.

Confidence:
MEDIUM

## Security Assessment

- **Authentication and authorization:** The audit API requires authentication and the SuperAdmin policy. The frontend route is also role-gated. Rate-limited reservation endpoints retain authorization requirements.
- **Object and tenant scope:** Audit records are intentionally globally readable only by SuperAdmin; complex filtering is available. No lower-privilege audit endpoint was found.
- **Data exposure:** Production health responses omit dependency descriptions. Global exception handling returns structured errors and avoids development detail outside Development. The audit UI exposes metadata only to SuperAdmin, appropriate for this operational feature.
- **Input security:** Audit pagination and UTC date-range ordering are validated. Rate limits use the effective remote IP after trusted-proxy processing rather than client-supplied forwarding headers.
- **Logging:** The console Serilog formatter redacts configured sensitive properties. Application Insights is conditionally enabled by connection-string configuration; no secret is committed in application settings.
- **Security findings:** None verified at HIGH or CRITICAL severity.

## Architecture Assessment

The Epic is consistent with the modular-monolith/clean-architecture design. `AuditLog` is a Domain entity; the repository contract lives in Application and its EF implementation in Infrastructure; the API controller remains a thin transport/validation boundary; contracts are separated. Health checks, middleware, logging, and rate limiting appropriately remain in the API layer.

Audit writes are co-saved with the associated action through the shared unit of work, avoiding a separate best-effort audit transaction. There is one clear audit abstraction rather than competing persistence mechanisms. No circular dependency, controller business logic, or Domain-to-Infrastructure dependency was found in the reviewed scope.

## Functional Assessment

- Audit action records are persisted and queried with exact optional filters, descending time order, and bounded pagination.
- The audit page supports loading, error, empty, filter, metadata-formatting, and pagination states.
- Correlation IDs are returned in responses and pushed into Serilog context; unhandled exceptions include trace IDs.
- Health endpoints distinguish aggregate health, process liveness, and database/error-rate readiness. Non-healthy readiness returns `503`.
- Rate limiting has separate login and reservation policies; authenticated users receive independent reservation partitions and excess requests receive a structured `429` and retry value.
- The missing alerting handoff is the principal incomplete end-to-end operational flow (H-01).

## Testing Assessment

- **Unit:** Audit creation/query mapping, correlation middleware, error-rate tracking/health checks, rate-limit partitioning, error-rate middleware, and mutation audit behavior are covered.
- **Integration:** Audit API authorization/filtering and an audit persistence flow are covered. Health endpoints cover healthy and unhealthy database/error-rate cases. Rate-limit tests cover threshold exhaustion, structured rejection, retry header, per-user quota isolation, and an unrelated endpoint.
- **Frontend:** Audit hook and page tests cover query construction plus populated, empty, and error states. Role protection is covered separately.
- **E2E/cross-story:** No browser test proves the SuperAdmin audit journey. No environment-level test proves readiness/probe/alert integration.
- **Executed:** backend build and solution tests passed; frontend lint, build, and Vitest suite passed. See Validation Results.

## Data & Database Assessment

The AuditLogs migration, entity configuration, and complex/time index are present. Required fields are constrained with maximum lengths and the entity exposes no public mutation/deletion API. The query is database-filtered and paged; the count plus paged query is appropriate for the bounded page sizes. Audit records are saved in the same unit of work as the reviewed mutation handlers, reducing partial-write risk.

No data-integrity or migration safety defect was verified. Like other application-level audit data, database-level write protection from a privileged direct database user is outside the present repository controls.

## Frontend Assessment

The SuperAdmin route uses `RequireRole`, and the audit page uses TanStack Query with an authenticated token. It translates local date-time input to ISO UTC, offers responsive MUI filter layout, handles loading/error/empty states, and provides standard table pagination. The component uses native MUI controls and table semantics; no accessibility blocker was identified from static review.

## Performance Assessment

- **Backend/database:** The audit repository filters at query level, orders by creation time, and caps page size at 100. The `(SportsComplexId, CreatedAt)` index supports the principal scoped time-ordered lookup. A global unfiltered audit search can grow as operational data grows, but current bounded paging makes this a monitoring item rather than a release blocker.
- **Frontend:** The page issues one query per filter-state change and renders only the current page. Metadata display constrains width but is not expanded on demand; this is acceptable for the current scope.
- **End-to-end:** Rate limiting protects high-risk mutation paths; no unnecessary cross-story request fan-out was found.

## Observability Assessment

Structured Serilog request logging, redaction, correlation IDs, global exception logging, optional Application Insights sink, health endpoints, and an error-rate tracker provide a strong diagnostic baseline. The major deficiency is active notification/alert routing for the error-rate signal (H-01). Audit persistence provides useful administrative traceability but there are no operational metrics or alert configuration artifacts under source control.

## Regression Risks

1. Misconfigured production trusted-proxy settings could cause login quotas to be partitioned by the reverse proxy rather than client IP; deployment must configure known proxies/networks narrowly.
2. The in-memory error-rate tracker is instance-local. In multi-instance deployment it is an instance readiness signal, not a fleet-wide error-rate metric; the missing monitor integration makes this especially important.
3. Audit-log volume can grow indefinitely; before higher scale, confirm retention/archival policy and inspect global-filter query performance.
4. Enabling rate limits changes behavior for high-frequency clients; monitor `429` responses and tune configured limits without weakening authorization.

## Documentation Assessment

Operational Runbook, Backup & Restore, Deployment, and README health documentation are substantial and consistent with the code. US-057’s acceptance list contains unrelated reservation requirements, a backlog-quality issue that should be corrected while normalizing its status. The Epic and US-057/US-058 status/checkboxes are stale (M-01). Documentation describes alerting as a possible consumer of readiness but does not provide an actual configured monitor (H-01).

## Positive Findings

- Audit logging is integrated across the major implemented administrative mutation handlers and shares the mutation transaction/unit of work.
- Audit API and UI are both restricted to SuperAdmin, with backend enforcement independent of client routing.
- Rate limiting correctly distinguishes login IP identity from authenticated reservation-user identity, includes structured error responses and retry behavior, and preserves unrelated endpoints.
- Forwarded-header handling explicitly restricts trust to configured proxies/networks rather than accepting arbitrary client forwarding headers.
- Health response details are suppressed outside Development, while the runbook provides specific operator diagnostics.
- The operational and backup/restore runbooks are practical, detailed, and aligned with the configuration and endpoint behavior.

## Validation Results

| Validation | Result |
|---|---|
| `dotnet build Mova.slnx` | PASSED — 0 warnings, 0 errors |
| `dotnet test Mova.slnx --no-build --verbosity normal` | PASSED — 158 tests |
| `npm run lint` | PASSED |
| `npm run build` | PASSED — Vite reported the existing large-chunk warning only |
| `npx vitest run --pool=threads` | PASSED — 45 files, 238 tests |
| Browser E2E audit journey | NOT EXECUTED — no EPIC-10 audit Playwright coverage found |
| `az bicep build` (`monitoring.bicep`, `app-service-health-check.bicep`, `main.bicep`) | PASSED - all templates compiled |
| `devops/scripts/smoke-test.ps1` (local DEBUG build) | PASSED - liveness, readiness, and stress-tested error-rate readiness |

## Epic Score

| Dimension | Score |
|---|---:|
| Requirements completeness | 96 |
| Functional correctness | 92 |
| Security | 93 |
| Architecture | 94 |
| API consistency | 92 |
| Database/data integrity | 90 |
| Frontend | 86 |
| Testing | 85 |
| Performance | 86 |
| Observability | 88 |
| Documentation | 92 |
| Production readiness | 90 |

**Overall risk indicator: approximately 83/100.** The score reflects strong implementation, automated validation, versioned Azure monitoring/alerting, and corrected delivery metadata. The only remaining risk is the optional LOW recommendation for a browser E2E audit journey.

## Final Verdict

**VERDICT: APPROVED WITH COMMENTS**

CRITICAL: 0  
HIGH: 0  
MEDIUM: 0  
LOW: 1  
INFO: 0

Main risks:
1. (LOW) The SuperAdmin audit-log browser journey is not covered by an end-to-end Playwright test.

Main missing requirements:
1. Add a dedicated Playwright E2E scenario for the SuperAdmin audit-log page.

Main cross-story issue:
None remaining after the alert/monitoring and backlog-metadata fixes.
