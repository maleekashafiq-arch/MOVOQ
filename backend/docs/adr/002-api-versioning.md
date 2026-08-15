# ADR 002 — API Versioning

Status: Accepted

Date: 2026-05-02

## Context

MOVOQ will support mobile apps, admin dashboards, and future integrations. API contracts must remain stable so client applications do not break when the backend changes.

## Decision

- All **business** APIs must use URL versioning with prefix:

  `/api/v1/`

- **No** unversioned business APIs allowed.
- System endpoints such as `/health` and `/ping` may remain unversioned.

### Breaking changes (require new version)

- Removing a field
- Renaming a field
- Changing data types
- Changing required fields
- Changing authentication or authorization behavior
- Changing response meaning

### Non-breaking changes

- Adding optional fields
- Adding new endpoints
- Enhancing validation without breaking existing clients

### Deprecation policy

- Previous API versions must be supported for **at least 6 months** after a new version release.
- Deprecated APIs must be documented.
- Clients must be notified before version removal.

### Breaking change governance

- Clients (mobile/admin) must be notified **at least 2 weeks** before a breaking version release.
- Backend team owns API contracts.
- All breaking changes must be documented before implementation.

### Scope clarification

- Third-party integrations (ads, sponsors, external APIs) are **out of scope** for this ADR.
- Those will be handled in separate ADRs.

## Consequences

- Backend must maintain multiple API versions simultaneously.
- Increased maintenance overhead.
- Safer client upgrades without breaking user experience.
- Slower removal of legacy endpoints.

## Implementation Rules

- All business endpoints must be under `/api/v1/`.
- Do not create unversioned business controllers.
- Keep system endpoints separate.
- Document all request/response contracts.
- Avoid breaking changes without a version increment.
