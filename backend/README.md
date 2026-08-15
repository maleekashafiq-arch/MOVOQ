# MOVOQ Backend

MOVOQ backend is a layered .NET solution structured to support maintainable, testable, and scalable server-side development using Clean Architecture principles.

## Official technology stack

| Concern | Choice |
|---------|--------|
| API | ASP.NET Core Web API (.NET) |
| Structure | Clean Architecture |
| Data access | Entity Framework Core |
| **Database (official)** | **PostgreSQL** (Neon in the cloud) |
| **API hosting** | Railway (API only) |
| **Auth** | Firebase Auth |
| Admin web | React |
| Mobile apps | Flutter (Android + iOS) |

**Database:** PostgreSQL is the official database for backend development, CI, and production. **Neon** is the cloud host. **SQLite** is only for the Flutter on-device cache. SQL Server is not used.

Full stack narrative, database strategy, and testing guidance: [`docs/architecture/technology-stack.md`](docs/architecture/technology-stack.md).

## Purpose

- Provide a clear architectural foundation before business features are implemented.
- Keep core domain and use-case logic independent from frameworks and infrastructure concerns.
- Enable safe growth through explicit project boundaries and dependency direction.

## Layer Responsibilities

- `MOVOQ.Domain`
  - Core business model building blocks.
  - Entities, value objects, domain events, enums, exceptions, and domain-level abstractions.
  - No dependency on other application layers.

- `MOVOQ.Application`
  - Use-case orchestration and application rules.
  - Contracts/abstractions for infrastructure dependencies.
  - DTOs, behaviors, validators, and feature workflows.
  - Depends on `MOVOQ.Domain`.

- `MOVOQ.Infrastructure`
  - Technical implementations for external concerns.
  - Persistence, repositories, service integrations, and framework adapters.
  - Implements abstractions defined by `MOVOQ.Application`.
  - Depends on `MOVOQ.Application` and `MOVOQ.Domain`.

- `MOVOQ.Api`
  - Composition root and delivery mechanism.
  - HTTP endpoints/controllers, middleware, request/response contracts, and startup wiring.
  - Depends on `MOVOQ.Application`, `MOVOQ.Infrastructure`, and `MOVOQ.Shared` (route prefixes and other HTTP-bound constants only).

- `MOVOQ.Tests`
  - Unit and integration test scaffolding.
  - Verifies behavior and boundary contracts across layers.
  - **Testing strategy:** persistence tests run against **PostgreSQL** (`MOVOQ_TEST_DATABASE` or Testcontainers). See `docs/architecture/technology-stack.md`.

## Dependency Rules

- Dependencies point inward toward business rules.
- `Domain` must not reference other project layers.
- `Application` may reference `Domain` only.
- `Infrastructure` may reference `Application` and `Domain`.
- `Api` may reference `Application`, `Infrastructure`, and `Shared` (constants only at the boundary).
- `Tests` may reference projects under test.

## Architecture Decisions - Stage 1

- `MOVOQ.Shared` is kept as a formal shared layer with strict scope:
  - allowed: shared enums, constants, error codes, primitive shared types
  - not allowed: DTOs, business logic, domain entities, database models
- DTOs remain in `MOVOQ.Application`.
- Domain entities and domain logic remain in `MOVOQ.Domain`.
- Stage 1 repository strategy keeps backend and future client apps in the same repository for simpler founder-level management.
- Offline model uses eventual consistency, while backend database remains source of truth.
- `MOVOQ.Tests` stays as one project with `Unit` and `Integration` folders for Stage 1.
- `MOVOQ.sln` is the **primary** solution for tools and CI; it includes all six projects (`MOVOQ.Shared` through `MOVOQ.Tests`). `MOVOQ.slnx` may remain in parallel.

## Feature 1 — Identity

Mobile signs in with Firebase (email/password or Google), then calls the API with the Firebase ID token.

| Method | Route | Purpose |
|--------|--------|---------|
| `POST` | `/api/v1/auth/session` | Create or resume the MOVOQ user |
| `GET` | `/api/v1/me` | Current user (provisions on first call) |
| `PUT` | `/api/v1/me/profile` | Mandatory profile: full name, country (ISO-2), state, city, daily goal |
| `POST` | `/api/v1/me/devices` | Register or refresh the device |

Set `Firebase:ProjectId` and a Neon **pooled** connection string in `appsettings.Development.json` (or environment variables) before running the API against real tokens.

In **Development**, open [http://localhost:5014/dev](http://localhost:5014/dev) for a local users dashboard. You can add fake users or seed the sample set. The same list is available at `GET /api/v1/dev/users`. These routes are not mapped outside Development.

Email verification is **not** required to use the app. The API returns `emailVerified` so later spend features can require it. Phone is optional until prize fulfillment.

## Feature 2 — Steps

The phone reports a step count per calendar day. The server decides what that day is worth; the client claim never awards anything on its own.

| Method | Route | Purpose |
|--------|--------|---------|
| `POST` | `/api/v1/steps/sync` | Submit one day's claim and get the server verdict |
| `GET` | `/api/v1/steps/today` | Today's validated steps, wallet balance, and streak |
| `GET` | `/api/v1/steps/history?days=7` | One entry per day, gaps filled with zeroes |

Server rules, all in `MOVOQ.Shared/StepRewardOptions.cs`:

- Reaching the daily goal credits **50 points**, at most once per day.
- The goal is snapshotted on the first sync of a day, so changing the target later cannot rewrite a day in progress.
- A day claiming more than **50,000** steps is marked `Rejected` and earns nothing.
- Within a day the server keeps the high-water mark, so a device reboot cannot walk the count backwards.
- Days older than **7** are refused with `sync_window_closed`; clients should stop retrying them.
- `commandId` is a client-generated UUID. Replaying one returns the stored day with `duplicate: true` and credits nothing.

Every points award writes a `points_ledgers` row and updates `user_wallets` in the same transaction. The ledger stays the source of truth; balance reads come from the wallet projection, and `IPointsLedgerRepository.SumLifetimeAmountsAsync` exists only for reconciliation.

## Current Setup Commands

Run from the `backend` directory:

```bash
dotnet new sln -n MOVOQ

dotnet new webapi -n MOVOQ.Api
dotnet new classlib -n MOVOQ.Application
dotnet new classlib -n MOVOQ.Domain
dotnet new classlib -n MOVOQ.Infrastructure
dotnet new xunit -n MOVOQ.Tests

dotnet sln add MOVOQ.Api/MOVOQ.Api.csproj
dotnet sln add MOVOQ.Application/MOVOQ.Application.csproj
dotnet sln add MOVOQ.Domain/MOVOQ.Domain.csproj
dotnet sln add MOVOQ.Infrastructure/MOVOQ.Infrastructure.csproj
dotnet sln add MOVOQ.Tests/MOVOQ.Tests.csproj

dotnet add MOVOQ.Application reference MOVOQ.Domain
dotnet add MOVOQ.Infrastructure reference MOVOQ.Application
dotnet add MOVOQ.Infrastructure reference MOVOQ.Domain
dotnet add MOVOQ.Api reference MOVOQ.Application
dotnet add MOVOQ.Api reference MOVOQ.Infrastructure
dotnet add MOVOQ.Api reference MOVOQ.Shared
dotnet add MOVOQ.Tests reference MOVOQ.Api
dotnet add MOVOQ.Tests reference MOVOQ.Application
dotnet add MOVOQ.Tests reference MOVOQ.Domain
dotnet add MOVOQ.Tests reference MOVOQ.Infrastructure

dotnet build
```
