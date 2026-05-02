# ADR 005 — Offline Conflict Resolution & Source of Truth

Status: Accepted

Date: 2026-05-02

## Context

Mobile clients may submit commands while offline or on poor networks. Without explicit conflict and authority rules, clients can diverge from server truth—especially for wallet balance, points, and rewards. This ADR complements ADR 001 by stating how conflicts are resolved and what clients must do after sync.

## Decision

The **backend database** is the **source of truth**. Offline client commands are **requests**, not final state.

### Idempotency and time

- **`CommandId`** is used for **idempotency** across retries.
- A **duplicate `CommandId`** must **not** apply business effects twice.
- **Server receipt time** is used for age validation and eligibility windows, **not** device clock alone.
- **Device timestamp** is **informational only** (metadata), never authoritative for financial outcomes.

### Wallet and ledger

- **`UserWallet` balance** is **never** taken as authoritative from the client.
- **Wallet balances must be derived from `PointsLedger`** (or equivalent ledger projection) on the server.
- **`PointsLedger`** conflicts are resolved by **server validation** and recorded outcomes; clients reconcile to server responses.

### Domain-specific authority

- **Rewards, redemptions, tickets, lucky draws, and winners** are controlled by **server state** only.
- **User profile** updates: for MVP, the server accepts the **latest valid server-accepted update** after validation (see ADR 001 for broader sync rules).

### Client behavior after sync

- The client **must refresh** server-confirmed state after each sync response (no stale “success” UI for rejected operations).
- **Rejected** offline commands must return **clear rejection reason codes** (stable codes suitable for client handling and support).

## Consequences

- Clients require explicit UX for pending, success, and rejected sync states.
- Server must implement idempotency storage, validation pipelines, and structured error/reason codes.
- Reduces client-side complexity at the cost of stricter API contracts and server-side bookkeeping.

## Implementation Rules

- Treat every offline command as “propose state”; only server confirmation updates authoritative UI for wallet, points, tickets, and rewards.
- Implement duplicate `CommandId` detection as a hard invariant before any points or inventory mutation.
- Always return machine-readable rejection codes for invalid or stale offline payloads.
- Document reason codes alongside API versioning (ADR 002) when endpoints are introduced.
