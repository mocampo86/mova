# Pull Request Workflow

## Creating a PR

1. Push the branch to the remote.
2. Open a PR against `main`.
3. Fill in the PR template:
   - Summary of changes
   - Related story or issue
   - Acceptance criteria status
   - Tests run
   - Out of scope
   - Notes for reviewers

## Review checklist

### Author responsibilities

- [ ] PR is small and focused (preferably < 400 changed lines).
- [ ] Description explains why, not just what.
- [ ] CI passes before requesting review.
- [ ] Relevant tests added or updated.
- [ ] No commented-out code or console logs in production code.

### Reviewer responsibilities

- [ ] Code follows project conventions and Clean Architecture.
- [ ] Domain logic is correct and isolated.
- [ ] Authorization and multi-tenancy checks are present.
- [ ] Validation is appropriate and tested.
- [ ] EF Core mappings and migrations are coherent.
- [ ] Frontend components are accessible and responsive.
- [ ] No secrets or sensitive data introduced.

### Security reviewer

Required for changes touching:

- Authentication or authorization
- Input handling or file uploads
- Secrets or configuration
- Payment-related code (future)

## Merge policy

- Squash and merge is the default strategy.
- Merge only when:
  - At least one approving review.
  - CI is green.
  - Security review completed if required.
  - Branch is up to date with `main`.

## After merge

1. Monitor CI/CD pipeline.
2. Verify staging deployment and health checks.
3. Close the related story/issue.
