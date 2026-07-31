# Definition of Done

A user story, bug fix, or technical task is considered done when all the following criteria are met.

## Functional criteria

- [ ] All acceptance criteria are implemented and verifiable.
- [ ] Business rules are enforced in domain or application logic.
- [ ] Input validation is implemented and tested.
- [ ] Authorization and multi-tenancy checks are in place.
- [ ] Edge cases and error scenarios are handled.

## Quality criteria

- [ ] The code compiles without warnings (treat warnings as errors where configured).
- [ ] Unit tests pass for new or affected domain logic.
- [ ] Integration tests pass for affected endpoints and repositories.
- [ ] Architecture tests pass (layer and dependency rules).
- [ ] Frontend build and tests pass.
- [ ] E2E tests pass for critical affected flows.
- [ ] Code review is approved.

## Security criteria

- [ ] No secrets, tokens, or credentials are committed.
- [ ] No sensitive data is logged.
- [ ] Endpoints are properly authorized.
- [ ] Input is validated on both client and server.
- [ ] Audit logging is added for administrative actions.

## Database criteria

- [ ] Entity Framework Core mapping changes are correct.
- [ ] A migration is created and tested against PostgreSQL.
- [ ] Migration rollback is understood and documented if needed.
- [ ] Indexes are added for new query patterns.

## Frontend criteria (if applicable)

- [ ] UI is responsive and works on mobile and desktop.
- [ ] Forms are validated with Zod and React Hook Form.
- [ ] Loading, error, and empty states are handled.
- [ ] API integration uses TanStack Query consistently.
- [ ] Accessibility best practices are followed (keyboard navigation, ARIA labels where needed).

## Documentation criteria

- [ ] `.ai-kit/docs` or `docs/` is updated if architecture or process changes.
- [ ] Public API changes are reflected in OpenAPI/Swagger annotations.
- [ ] Complex logic or invariants are documented in code comments.
- [ ] README or local setup instructions are updated if needed.

## Manual validation

- [ ] Feature is manually validated locally or in staging.
- [ ] Happy path and at least one error path are exercised.
- [ ] Mobile layout is checked if the change affects UI.

## Deployment criteria

- [ ] CI pipeline is green.
- [ ] No breaking changes are introduced without a migration or feature flag plan.
- [ ] Deployment steps are understood and documented if needed.

## Sign-off

- [ ] Developer confirms the task is complete.
- [ ] Reviewer(s) approve the PR.
- [ ] QA confirms critical tests pass (when applicable).
