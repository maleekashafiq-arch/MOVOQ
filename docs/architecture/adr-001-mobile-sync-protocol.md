# ADR-001: Mobile Sync Protocol

## Status

Accepted (Stage 1 baseline)

## Context

MOVOQ supports mobile offline behavior for activity capture. Mobile clients can operate without network access and later synchronize queued actions with backend APIs. Without a strict sync protocol, duplicate submissions, inconsistent points awarding, and conflict ambiguity can occur.

## Decision

MOVOQ will use eventual consistency for mobile-originated offline data.

### Core rules

- Backend database remains the source of truth.
- Mobile may queue step/ad/device activities while offline.
- Each offline command must include a client-generated `CommandId` (GUID) for idempotency.
- Backend must reject duplicate `CommandId` values.
- Backend validates all synced activity before awarding points.
- `PointsLedger`, `UserWallet`, `RewardRedemption`, `LuckyDrawTicket`, and `LuckyDrawWinner` are server-controlled and authoritative.

### Conflict strategy

- Step/ad activity: backend validation wins.
- User profile edits: last-write-wins may be used initially.
- Points/tickets/rewards/winners: no mobile conflict resolution path; backend authority is final.

### Deletion strategy

- Use soft delete/tombstone semantics for records that require cross-device synchronization.

### Retry strategy

- Mobile retries failed pending commands using the same `CommandId`.
- Retries must remain idempotent through server-side duplicate detection.

## Consequences

- Positive:
  - Protects server-side financial/reward integrity.
  - Provides deterministic behavior for duplicate offline submissions.
  - Supports offline-first user experience for activity capture.
- Trade-offs:
  - Requires robust idempotency storage and duplicate-check logic in backend.
  - Requires clear client queue management and retry handling.

## Future Considerations

- A transactional outbox and background processor may be introduced later to improve reliability and scalability of sync/event processing.
