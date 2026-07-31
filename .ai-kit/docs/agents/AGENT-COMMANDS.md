# Agent Commands

## Conventions

- Commands are case-insensitive in this documentation; use lower-case in prompts.
- All commands assume the current working directory is the repository root unless stated otherwise.
- If a command requires a `<scope>` or `<story>` argument, the agent must locate the relevant `.ai-kit/docs` files and the active project context.

## Global commands

| Command | Description |
|---------|-------------|
| `ai context` | Load the full `.ai-kit/docs` knowledge base summary for the current task. |
| `ai status` | Show what files and decisions have changed since the last agent run. |
| `ai help` | List available commands and their descriptions. |

## Architect commands

| Command | Description |
|---------|-------------|
| `architect review <scope>` | Review a design or implementation for alignment with the architecture. |
| `architect propose <topic>` | Propose a design change or new module. |
| `architect decision <name>` | Create or update an Architecture Decision Record (ADR). |

## Backend commands

| Command | Description |
|---------|-------------|
| `backend implement <story>` | Implement a backend user story using Clean Architecture. |
| `backend test <area>` | Run or add backend tests for `<area>`. |
| `backend migrate <name>` | Scaffold and validate an EF Core migration. |
| `backend seed <entity>` | Add seed data for `<entity>`. |

## Frontend commands

| Command | Description |
|---------|-------------|
| `frontend implement <story>` | Implement a frontend user story. |
| `frontend test <area>` | Run or add unit/component tests. |
| `frontend e2e <scenario>` | Add or run a Playwright end-to-end scenario. |
| `frontend component <name>` | Scaffold a new shared component. |

## QA commands

| Command | Description |
|---------|-------------|
| `qa plan <story>` | Generate a test plan for a user story. |
| `qa run` | Execute the appropriate test suite for the current branch. |
| `qa coverage <area>` | Report test coverage for `<area>`. |
| `qa regression` | Run the full regression suite. |

## DevOps commands

| Command | Description |
|---------|-------------|
| `devops deploy <env>` | Prepare or execute a deployment to `<env>`. |
| `devops pipeline <name>` | Add or update a CI/CD pipeline definition. |
| `devops provision <env>` | Provision or update cloud resources for `<env>`. |
| `devops secrets` | Validate that no secrets are committed and rotate if needed. |

## Product commands

| Command | Description |
|---------|-------------|
| `product story <title>` | Draft a user story. |
| `product refine <story>` | Refine acceptance criteria and scope. |
| `product backlog <epic>` | List or organize the backlog for an epic. |

## Security commands

| Command | Description |
|---------|-------------|
| `security review <scope>` | Review a change for security issues. |
| `security threat <feature>` | Identify threats and mitigations for a feature. |
| `security audit` | Run a lightweight security audit of the codebase. |

## Command execution rules

1. Read the relevant `.ai-kit/docs` files before executing a command.
2. Validate the working tree before destructive operations (`devops deploy`, `backend migrate` on production, etc.).
3. Prefer `dotnet build` / `npm run build` and tests before declaring a command complete.
4. Update `.ai-kit/docs` only if the command changes an architecture or process decision.
5. Report failures with clear error messages and recommended next steps.
