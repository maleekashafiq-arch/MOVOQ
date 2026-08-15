# ADR 004 — Authentication, Authorization & Audit

Status: Accepted

Date: 2026-05-02

## Context

MOVOQ serves end users, coaches/influencers, and privileged admin roles. Reward and account operations require strong access control and traceability. Without an agreed auth, authorization, and audit baseline, implementations risk inconsistent checks, missing audit trails, and insecure handling of secrets.

## Decision

MOVOQ will use **role-based access control (RBAC)** and **mandatory audit logging** for privileged operations.

### Identity and roles

- End users authenticate through supported login methods when implemented (e.g. email, phone, Google sign-in—exact providers are implementation details).
- Roles such as Admin, SuperAdmin, Coach, Influencer, and User are represented through `UserRole` (and related policies as implemented later).
- **Admin** and **SuperAdmin** access must be protected by authorization checks on every privileged endpoint or use case entry point.

### Audit

- **SuperAdmin** operations require **strict** audit logging.
- Privileged operations must create **AuditLog** (or equivalent) entries.
- Audit logs must include, at minimum:
  - **Actor** (who performed the action)
  - **Action type**
  - **Target entity** (type and identifier)
  - **Timestamp in UTC**
  - **Relevant metadata** (e.g. IP, user agent, correlation id when available)

### Secrets and implementation deferral

- **Secrets must not be hardcoded** in source; use configuration/secret stores appropriate to the environment.
- Token lifecycle, identity provider choice, and concrete middleware wiring are **implementation details** to be finalized **before** auth code ships, but this ADR locks the **governance** expectations above.

## Consequences

- Higher implementation cost up front for middleware, policies, and audit persistence.
- Stronger accountability and easier incident review for admin actions.
- Clear separation between “who you are” (auth) and “what you may do” (authorization + audit).

## Implementation Rules

- No privileged action without an authorization check aligned to `UserRole` (and future fine-grained permissions if added).
- Every SuperAdmin mutation path must emit an audit record before or within the same logical transaction as the change (exact ordering defined at implementation).
- Never log secrets, refresh tokens, or full payment data in audit entries.
- Document which actions are audited and the retention policy in a follow-on doc or ADR when retention is fixed.
