# MOVOQ Backend

MOVOQ backend is a layered .NET solution structured to support maintainable, testable, and scalable server-side development using Clean Architecture principles.

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
  - Depends on `MOVOQ.Application` and `MOVOQ.Infrastructure`.

- `MOVOQ.Tests`
  - Unit and integration test scaffolding.
  - Verifies behavior and boundary contracts across layers.

## Dependency Rules

- Dependencies point inward toward business rules.
- `Domain` must not reference other project layers.
- `Application` may reference `Domain` only.
- `Infrastructure` may reference `Application` and `Domain`.
- `Api` may reference `Application` and `Infrastructure`.
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
- `MOVOQ.sln` should be maintained as primary solution for tool and CI compatibility; `.slnx` may remain.

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
dotnet add MOVOQ.Tests reference MOVOQ.Api
dotnet add MOVOQ.Tests reference MOVOQ.Application
dotnet add MOVOQ.Tests reference MOVOQ.Domain
dotnet add MOVOQ.Tests reference MOVOQ.Infrastructure

dotnet build
```
