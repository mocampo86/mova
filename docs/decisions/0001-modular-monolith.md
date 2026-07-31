# ADR-0001: Modular Monolith

## Status

Accepted

## Context

The platform must support multiple sports complexes, user authentication, reservations, and administrative workflows while keeping initial cost and operational complexity low. We needed to decide between a modular monolith, microservices, or a serverless architecture.

## Decision

We will implement Reserva Canchas as a **modular monolith**.

## Consequences

### Positive

- Lower operational cost and simpler deployment in the early stages.
- Easier local development and testing.
- Clear separation of concerns through Clean Architecture layers.
- Modules can be extracted into independent services later if needed.
- Simpler transaction management for reservation conflict prevention.

### Negative

- All modules share the same deployable unit and runtime.
- A failure in one module can affect the whole application.
- Independent scaling of modules is not possible without extraction.

## Alternatives considered

- **Microservices**: rejected due to increased operational complexity, network latency, and distributed transaction challenges for the MVP.
- **Serverless functions**: rejected because long-term stateful operations (reservations, recurring rules) are easier to model in a traditional API with a relational database.

## Related decisions

- ADR-0002: Shared PostgreSQL with logical multi-tenancy.

## References

- `mova-project-overview.md` section 9.
- `.ai-kit/docs/architecture/SOLUTION-ARCHITECTURE.md`.
