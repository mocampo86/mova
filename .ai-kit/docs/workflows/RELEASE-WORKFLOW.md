# Release Workflow

## Versioning

Use **Semantic Versioning** for releases: `MAJOR.MINOR.PATCH`.

| Change | Version bump |
|--------|--------------|
| Breaking API or data contract | MAJOR |
| New feature, non-breaking | MINOR |
| Bug fix, security patch | PATCH |

Pre-release versions can use `-beta.N` or `-rc.N` suffixes.

## Release branches

For scheduled releases, create a release branch:

```text
release/v1.2.0
```

Cherry-pick or merge all completed features and fixes. Hot fixes can be merged directly to `main` and then back-ported to the active release branch.

## Release checklist

### Preparation

- [ ] All stories for the release are merged to `main` or release branch.
- [ ] `CHANGELOG.md` is updated with release notes.
- [ ] Version is bumped in relevant project files.
- [ ] Database migrations are consolidated and tested.
- [ ] Feature flags are configured.

### Validation

- [ ] Full test suite passes (`dotnet test`, `npm run test`, `npm run test:e2e`).
- [ ] Staging deployment is healthy.
- [ ] Smoke tests pass.
- [ ] Security review completed for the release.

### Deployment

1. Create a GitHub/GitLab release tag.
2. Run production CI/CD pipeline.
3. Apply database migrations.
4. Deploy API container or App Service.
5. Deploy frontend static assets.
6. Run production health checks.
7. Monitor Application Insights and error rates for a cooldown period (e.g. 30 minutes).

### Rollback

If a critical issue is detected:

1. Disable the feature flag if available.
2. Re-deploy the previous stable container/image.
3. If a database migration caused the issue, restore from backup and apply the reverse migration script if safe.
4. Communicate incident status.

## Environments

| Environment | Source | Deployment trigger |
|-------------|--------|-------------------|
| Dev | `main` latest | Every successful `main` build |
| QA | `main` or release branch | Manual or scheduled |
| Staging | Release tag | Before production |
| Production | Release tag | Manual approval |

## Changelog format

```markdown
## [1.1.0] - 2026-08-15

### Added
- Court availability rules.
- Recurring reservations.

### Fixed
- Race condition on concurrent reservation creation.

### Security
- Added rate limiting on authentication endpoints.
```

## Communication

After release:

- Notify the team in the project channel.
- Update the status dashboard.
- Schedule a post-release review if issues were found.
