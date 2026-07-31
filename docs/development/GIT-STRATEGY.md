# Git Strategy

## Branching model

Use a **GitHub Flow** / trunk-based model:

- `main` is the default and deployable branch.
- Feature branches are short-lived and branch from `main`.
- Releases are tagged on `main`.

## Branch naming

```text
feature/US-001-short-description
fix/US-042-resolve-conflict
chore/update-dotnet-version
hotfix/resolve-auth-bug
```

Use kebab-case and include the user story ID when applicable.

## Commit conventions

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```text
type(scope): subject

body (optional)

footer (optional)
```

Examples:

```text
feat(courts): add availability rule endpoints
fix(reservations): prevent overlapping bookings under concurrency
test(reservations): add concurrency tests
docs(api): update pagination contract
refactor(domain): extract TimeRange value object
chore(deps): update npgsql to 8.x
```

Types:

- `feat`: new feature
- `fix`: bug fix
- `docs`: documentation changes
- `style`: formatting only (no logic change)
- `refactor`: code change that neither fixes a bug nor adds a feature
- `test`: adding or updating tests
- `chore`: maintenance, dependency updates, build changes

## Pull request workflow

1. Create a feature branch from `main`.
2. Make focused commits.
3. Push the branch.
4. Open a PR to `main` with a clear description.
5. Ensure CI passes.
6. Request review from at least one peer.
7. Address review comments.
8. Squash and merge.
9. Delete the branch after merge.

## PR description template

```markdown
## Summary
Brief description of changes.

## Related story
Link to user story or issue.

## Changes
- Change 1
- Change 2

## Tests
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] E2E tests added/updated

## Checklist
- [ ] Build passes
- [ ] No secrets committed
- [ ] Documentation updated
```

## Merge policy

- Squash and merge is the default.
- A PR requires at least one approval and green CI before merging.
- Security-sensitive changes require an additional security review.

## Release tagging

Use Semantic Versioning:

```text
git tag -a v0.1.0 -m "Release v0.1.0 - MVP foundation"
git push origin v0.1.0
```

## Hotfixes

For production incidents:

1. Create `hotfix/...` from the latest release tag or `main`.
2. Apply the minimal fix.
3. Run tests.
4. Open a PR, fast-track review, and merge.
5. Tag a new patch release.
6. Deploy immediately.

## Protected branches

- `main` requires pull request reviews.
- `main` requires status checks to pass.
- `main` should reject force pushes.
