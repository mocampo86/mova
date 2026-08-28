# MOVA .ai-kit Docs

This `.ai-kit/docs` package contains the technical knowledge base for the **Mova** project. It is designed to be consumed by AI agents and human developers during planning, implementation, review, and operations.

## What is .ai-kit?

`.ai-kit` is a project-local documentation and instruction set that accelerates AI-assisted development. It defines the architecture, domain, conventions, agent roles, and workflows so that every agent starts with the same context and produces consistent output.

## How to use this documentation

- **Before writing code**: read `architecture/SOLUTION-ARCHITECTURE.md` and `architecture/DOMAIN-MODEL.md`.
- **Before modifying the database**: read `architecture/DATABASE-DESIGN.md`.
- **Before creating a user story**: read `workflows/DEVELOPMENT-WORKFLOW.md`.
- **Before assigning work to an agent**: read `agents/AGENT-ROLES.md`.
- **Before performing operational tasks**: read `operations/OPERATIONAL-RUNBOOK.md`.

## Folder structure

```text
.ai-kit/docs/
├── README.md
├── architecture/
│   ├── SOLUTION-ARCHITECTURE.md
│   ├── DOMAIN-MODEL.md
│   ├── DATABASE-DESIGN.md
│   ├── API-DESIGN.md
│   ├── AUTHENTICATION.md
│   ├── MULTI-TENANCY.md
│   └── DEPLOYMENT.md
├── agents/
│   ├── AGENTS.md
│   ├── AGENT-ROLES.md
│   ├── AGENT-COMMANDS.md
│   └── AGENT-WORKFLOWS.md
├── operations/
│   ├── BACKUP-RESTORE.md
│   └── OPERATIONAL-RUNBOOK.md
└── workflows/
    ├── DEVELOPMENT-WORKFLOW.md
    ├── PR-WORKFLOW.md
    ├── TESTING-WORKFLOW.md
    └── RELEASE-WORKFLOW.md
```

## Status

This documentation is derived from `mova-project-overview.md` and reflects the MVP definition. It will be updated as decisions are made and code is implemented.

## Contributing

When a decision changes architecture, domain, deployment, or team process, update the corresponding `.ai-kit/docs` file in the same pull request as the code change.
