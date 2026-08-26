# Changelog

All notable changes to Mova (MOVA) will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial `.ai-kit/docs` knowledge base with architecture, agents, and workflows.
- Initial project documentation (`mova-project-overview.md`) defining MVP vision, scope, and backlog.
- Recurring reservations: user and admin weekly series creation, occurrence generation, conflict-aware validation, single-occurrence and series cancellation, complex-scoped administration, and user self-service enable/disable setting.
- Idempotency support for recurring reservation mutations with durable key/response storage and replay.

### Notes

- MVP is in definition stage; no production code has been released yet.

## [0.1.0] - TBD

### Planned

- Foundation technical stack (backend, frontend, PostgreSQL, Docker Compose, CI).
- Identity and access (Google login, JWT, roles).
- Complex and court administration.
- Availability and reservation engine.
- Landing page and deployment.
