# MOVOQ Architecture Review Report

## 1) Executive Summary

The current repository contains a cleanly separated .NET backend foundation in `backend/` and placeholders for `frontend/` and `docs/`.
The backend follows intended Clean Architecture dependency direction and compiles successfully, which is a strong base.

At the same time, this is still an early-stage skeleton:
- API is template-level (`/weatherforecast`)
- no concrete domain entities yet (only base abstractions/enums)
- no persistence implementation
- no auth/authorization implementation
- no documented sync contract, API versioning policy, or data classification policy

From an architect/CTO perspective: the foundation is correct, while governance and cross-app integration contracts are the largest current gap.

## 2) Current Repository Structure

Observed at repository root:

- `backend/`
  - `MOVOQ.Api`
  - `MOVOQ.Application`
  - `MOVOQ.Domain`
  - `MOVOQ.Infrastructure`
  - `MOVOQ.Tests`
  - `MOVOQ.Shared` (folder exists but currently empty and not part of solution)
  - `.gitignore`
  - `MOVOQ.slnx`
  - `README.md`
- `frontend/` (empty)
- `docs/` (empty before this report)

Notes:
- `MOVOQ.slnx` currently includes:
  - `MOVOQ.Api`
  - `MOVOQ.Application`
  - `MOVOQ.Domain`
  - `MOVOQ.Infrastructure`
  - `MOVOQ.Tests`
- `MOVOQ.Shared` exists as a reserved shared layer folder and is planned to be formalized with strict boundaries.

## 3) Technology Stack

Current, repository-confirmed stack:

- Backend runtime: .NET `net10.0`
- API framework: ASP.NET Core Web API (`Microsoft.NET.Sdk.Web`)
- OpenAPI package: `Microsoft.AspNetCore.OpenApi`
- Testing stack: xUnit + `Microsoft.NET.Test.Sdk` + `coverlet.collector`
- Solution format: `.slnx` exists; classic `.sln` is required as the primary compatibility target for tooling and CI/CD.
- Source control: Git initialized in `backend/`

Not yet present:
- database provider packages (EF Core provider, etc.)
- authentication/authorization implementation
- messaging/cache/integration infrastructure

## 4) Clean Architecture Overview

The backend is structured into canonical clean layers:

- `MOVOQ.Domain` (core model/contracts)
- `MOVOQ.Application` (use-case orchestration contracts)
- `MOVOQ.Infrastructure` (technical implementation layer)
- `MOVOQ.Api` (delivery/composition root)
- `MOVOQ.Tests` (verification)

This is suitable for a multi-client platform where mobile and admin web consume one backend.

## 5) Layer Responsibilities

Current intended responsibilities (consistent with `backend/README.md` and project layout):

- `MOVOQ.Domain`: core concepts, base entities, enums, abstractions, no outward dependency
- `MOVOQ.Application`: use-case orchestration and interfaces
- `MOVOQ.Infrastructure`: adapters (persistence/services) implementing application abstractions
- `MOVOQ.Api`: HTTP entrypoint, middleware, endpoint wiring, dependency composition
- `MOVOQ.Tests`: unit/integration scaffolding

## 6) Project Dependency Direction

Actual references in repository:

- `MOVOQ.Application` -> `MOVOQ.Domain`
- `MOVOQ.Infrastructure` -> `MOVOQ.Application`, `MOVOQ.Domain`
- `MOVOQ.Api` -> `MOVOQ.Application`, `MOVOQ.Infrastructure`
- `MOVOQ.Domain` -> (none)
- `MOVOQ.Tests` -> `MOVOQ.Api`, `MOVOQ.Application`, `MOVOQ.Domain`, `MOVOQ.Infrastructure`

Assessment:
- Core dependency direction is correct for Clean Architecture.
- Test project referencing all layers is acceptable for now; later, splitting unit/integration test projects can improve isolation.

## 7) Multi-App Strategy

### Flutter mobile app
- Should consume backend APIs only.
- Should treat local storage as cache/offline queue, not business source of truth.

### React admin web app
- Should consume backend APIs only.
- Must not access database directly.

### .NET backend API
- Should remain the single authority for business logic and permission checks.
- Should expose stable, versioned contracts for both clients.

Repository strategy (Stage 1 decision):
- Keep backend, future Flutter app, and future React admin in one repository for founder-level management simplicity.
- `frontend/` is currently empty and will be populated as web/mobile clients are introduced.
- Future split into dedicated repositories remains a valid option.

## 8) Centralized vs Local Data Strategy

Required separation:

- Centralized backend DB (source of truth):
  - identity and account state
  - rewards, balances, redemptions, tickets, fraud outcomes
  - audit logs and admin actions
  - system/business configuration

- Mobile local storage:
  - cached read models
  - pending offline commands/events
  - temporary drafts/preferences
  - non-authoritative UI state

Current repository does not yet enforce this by implementation; it should be formalized before deeper feature work.

## 9) Mobile Sync Strategy

What exists now in `MOVOQ.Domain`:
- `BaseEntity`: `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, `Version`
- `ISyncableEntity`: `DeviceId`, `SyncedAt`, `Source`
- `DataSource` enum

Assessment:
- Good baseline primitives.
- Missing sync protocol details: conflict resolution, idempotency, retry/backoff, tombstone semantics, incremental sync boundaries.

## 10) Super Admin Access Strategy

Current state:
- no authN/authZ implementation yet
- role enum includes `SuperAdmin`

Recommended design:
- policy-based authorization at API boundary
- role + permission matrix with explicit scopes
- immutable audit for privileged operations
- least-privilege defaults
- emergency admin controls and review workflow

## 11) Security Considerations

Current security posture is pre-implementation.

Priority controls to add before high-risk features:
- authentication strategy and token lifecycle
- authorization policies and scope checks
- secret/config management
- input validation at boundary and use-case levels
- audit trail for admin/super-admin actions
- rate limiting and abuse protection
- secure client token storage guidance

## 12) Database Source-of-Truth Rules

Recommended non-negotiable rules:

- Database is authoritative for all business-critical states.
- Clients never write directly to database.
- Admin web never bypasses backend API.
- Offline writes represent intent/pending operations until server validation.
- Sync-enabled entities must use version/concurrency checks.

Current repository implies these rules but does not yet codify them in dedicated architecture docs/ADRs.

## 13) Current Completed Work

Implemented and present:
- clean architecture project split and references
- baseline folder scaffolding in each backend project
- backend `.gitignore` and `README.md`
- Domain Foundation V1 primitives:
  - `MOVOQ.Domain/Entities/BaseEntity.cs`
  - `MOVOQ.Domain/Abstractions/ISyncableEntity.cs`
  - domain enums for roles, statuses, source, fraud, notification, audit
- successful solution build (`dotnet build`)

## 14) Architecture Decisions (Stage 1 Baseline)

The following decisions are now explicit for Stage 1 implementation.

### 14.1 `MOVOQ.Shared` decision

Decision: keep `MOVOQ.Shared` as a formal shared project/layer.

Allowed scope:
- lightweight shared enums
- constants
- error codes
- shared primitive types

Restrictions:
- no DTOs
- no business logic
- no domain entities
- no database models

Boundary rule:
- DTOs stay in `MOVOQ.Application`
- domain entities and domain logic stay in `MOVOQ.Domain`

### 14.2 Repository strategy decision

Decision: Stage 1 stays in one repository containing backend and future client apps.

Planned Stage 1 shape:
- backend (`backend/`)
- future Flutter mobile app
- future React admin web app

Future option:
- split later into `movoq-backend`, `movoq-mobile-app`, and `movoq-admin-web` if team size/velocity requires it.
- API contracts can later be generated from OpenAPI/Swagger.

### 14.3 Offline consistency model decision

Decision: use eventual consistency for offline mobile sync.

Rules:
- backend database is source of truth
- mobile may cache data locally
- mobile may queue step/ad/device activities while offline
- backend validates synced activity before awarding points
- points, wallet balance, tickets, rewards, and winners are never finalized by mobile

### 14.4 Sync-enabled vs server-only entities decision

Sync-enabled/mobile-originated entities:
- `DailyStepRecord`
- `AdsWatched`
- `UserDevice`
- `FraudSignal`
- `Notification`

Server-only/source-of-truth entities:
- `User`
- `UserProfile`
- `UserWallet`
- `PointsLedger`
- `Reward`
- `RewardRedemption`
- `Sponsor`
- `LuckyDraw`
- `LuckyDrawTicket`
- `LuckyDrawWinner`
- `AdminUser`
- `AuditLog`
- `AppSetting`

### 14.5 Super-admin audit scope decision

Audit is mandatory for admin/super-admin actions affecting:
- users and user status changes
- wallet and points adjustments
- rewards and reward redemptions
- sponsors
- lucky draws, ticket operations, winner selection
- fraud signals
- app settings
- role/permission changes
- manual overrides

Mandatory audit log fields:
- admin user id
- action type
- entity name
- entity id
- old value (where practical)
- new value (where practical)
- timestamp
- IP/device metadata (where available)

### 14.6 Compliance posture decision (Stage 1)

Stage 1 baseline:
- basic PII protection
- protect email, phone number, device id, auth tokens
- no payment card data stored in MVP
- no user funds/cash wallet in MVP
- points are loyalty/reward points only
- privacy policy and terms required before launch
- account deletion process required before launch
- basic data retention rules required before launch
- user consent required for ads, notifications, and fitness/step data
- admin access must be role-based and audited

Potential later compliance areas:
- country-specific privacy/data protection rules
- lucky draw and legal promotion obligations
- sponsor terms/compliance
- payment/KYC only if real-money payout is introduced

### 14.7 Test strategy decision

Decision for Stage 1:
- keep one `MOVOQ.Tests` project with `Unit` and `Integration` folders.

Future option:
- split into `MOVOQ.UnitTests` and `MOVOQ.IntegrationTests` if scale requires.

### 14.8 Solution file compatibility decision

Decision:
- maintain classic `MOVOQ.sln` as primary for compatibility.
- `.slnx` may remain, but `.sln` should be treated as primary for Cursor, Visual Studio, CLI, and CI/CD.

## 15) Risks / Open Questions

Key risks:
- contract drift between backend and future clients
- audit implementation gaps despite defined scope
- data integrity issues when offline sync starts without conflict policy
- delay in formalizing `MOVOQ.Shared` boundaries as code-level guardrails
- limited observability/audit readiness for admin operations

## 16) Recommendations Before Domain Entities V1

Before large-scale entity implementation, complete:
- formalize `MOVOQ.Shared` as a constrained project (enums/constants/error codes/primitives only)
- entity conventions: timestamps, versioning, soft-delete semantics
- sync conflict/idempotency protocol on top of decided eventual consistency model
- source-of-truth and offline-data policy document
- role/permission matrix for `SuperAdmin`, `Admin`, `Coach`, `Influencer`, `User`
- API versioning and backward-compatibility policy

## 17) Recommended Next Development Steps

Suggested order:

1. Create ADRs for:
   - API-only client access
   - sync/conflict strategy
   - auth/authorization model
   - shared contract management strategy
   - Stage 1 compliance baseline
2. Build one small vertical slice end to end:
   - domain entity + application use case + infrastructure persistence + API endpoint + tests
3. Add policy authorization stubs for admin vs user routes.
4. Add API contract/versioning baseline.
5. Iterate with incremental slices, avoiding broad speculative modeling.

## 18) Questions for the Reviewing Architect

Questions now narrowed to implementation and governance details:

- Should `MOVOQ.Shared` be added to solution immediately or after first shared type set is defined?
- What is the preferred conflict resolution policy per sync-enabled entity (LWW vs merge-by-domain-rule)?
- Which super-admin operations require dual control/approval vs single-actor execution?
- What are target launch-country legal constraints for promotions/lucky draws?
- What is the minimum audit retention period for Stage 1?
- Which CI environments are mandatory for validating `MOVOQ.sln` compatibility?

## ADR References

This architecture baseline is further detailed in:

- `docs/architecture/adr-001-mobile-sync-protocol.md`
- `docs/architecture/adr-002-api-versioning.md`
- `docs/adr/adr-003-shared-layer-boundaries.md`
- `docs/adr/adr-004-auth-authorization-audit.md`
- `docs/adr/adr-005-offline-conflict-resolution.md`
