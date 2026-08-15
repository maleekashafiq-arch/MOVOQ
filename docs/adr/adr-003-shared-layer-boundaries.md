# ADR 003 — Shared Layer Boundaries

Status: Accepted

Date: 2026-05-02

## Context

MOVOQ has multiple layers (`MOVOQ.Domain`, `MOVOQ.Application`, `MOVOQ.Infrastructure`, `MOVOQ.Api`) and client applications that must agree on a small set of stable primitives (enums, constants, error codes). Without explicit boundaries, `MOVOQ.Shared` can accidentally become a dumping ground for DTOs, entities, or framework code, which breaks Clean Architecture and creates circular dependency risk.

## Decision

`MOVOQ.Shared` is a **lightweight shared project** used only for **cross-layer primitive contracts**.

### Allowed in `MOVOQ.Shared`

- Enums
- Constants
- Primitive shared types (e.g. strongly-typed identifiers or small value types that are not domain entities)

### Not allowed in `MOVOQ.Shared`

- Domain entities
- DTOs (request/response models)
- Business logic
- Repository interfaces
- Service interfaces
- Infrastructure-specific code (EF, HTTP clients, cloud SDKs, etc.)

### Dependency rules

- `MOVOQ.Shared` **must not** reference `MOVOQ.Domain`, `MOVOQ.Application`, `MOVOQ.Infrastructure`, or `MOVOQ.Api`.
- `MOVOQ.Domain` may reference `MOVOQ.Shared` **only** for enums or other approved primitive shared concepts—not for application or infrastructure types.
- `MOVOQ.Shared` must remain **stable and minimal**; new types require the same discipline as public API changes.

## Consequences

- Clear separation between “vocabulary” (Shared) and “behavior + rules” (Domain/Application).
- Safer reuse across layers and future clients without pulling in heavy dependencies.
- Requires discipline in code review to reject inappropriate additions to Shared.

## Implementation Rules

- Before adding a type to `MOVOQ.Shared`, confirm it is an enum, constant, primitive shared type, or error code—not a model or service contract.
- Never add project references from `MOVOQ.Shared` to any other MOVOQ project.
- Prefer extending enums/constants in Shared over duplicating magic strings in clients or Domain.
- If a concept needs behavior or persistence, it belongs in `MOVOQ.Domain` or `MOVOQ.Application`, not `MOVOQ.Shared`.
