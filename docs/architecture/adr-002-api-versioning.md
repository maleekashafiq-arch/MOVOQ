# ADR-002: API Versioning Strategy

## Status

Accepted (Stage 1 baseline)

## Context

MOVOQ will serve multiple clients (Flutter mobile and React admin web) that depend on stable API contracts. Unversioned APIs increase risk of breaking existing clients when backend evolves.

## Decision

All backend APIs will use URL versioning from day one.

### Versioning rules

- Initial route prefix: `/api/v1/`
- Avoid unversioned APIs.
- Future breaking changes may introduce `/api/v2/` and beyond.

## Rationale

- Mobile and admin clients need stable, predictable contract lifecycles.
- URL versioning is explicit, easy to debug, and straightforward for client teams and API gateways.
- Early versioning prevents risky migration from unversioned to versioned routes later.

## Consequences

- Positive:
  - Improves backward compatibility management.
  - Enables phased client migrations.
  - Reduces accidental breaking changes.
- Trade-offs:
  - Requires version management discipline in routing and documentation.
  - May temporarily increase maintenance when multiple versions run concurrently.

## Documentation Guidance

- Swagger/OpenAPI should document versioned APIs in later implementation stages.
