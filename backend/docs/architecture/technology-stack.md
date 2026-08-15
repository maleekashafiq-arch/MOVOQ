# MOVOQ — official technology stack

This document records the **final** MOVOQ technology decisions for the product.

---

## Platform overview

| Area | Technology |
|------|------------|
| **Backend API** | ASP.NET Core Web API (.NET 10) |
| **Architecture** | Clean Architecture (Domain → Application → Infrastructure; API as composition root) |
| **ORM** | Entity Framework Core |
| **Official database** | **PostgreSQL** |
| **Cloud database (MVP)** | **Neon** (pooled connection string) |
| **API hosting** | **Railway** (API only — not the database) |
| **Auth** | **Firebase Auth** (email/password + Google; Apple Sign In on iOS) |
| **Admin web** | React |
| **Mobile** | Flutter (Android and iOS) |
| **Mobile local database** | Drift + SQLite (cache and offline command queue only) |

SQL Server and Azure SQL are **not** part of the official stack.

---

## Database strategy

1. **PostgreSQL is the official relational database** for local development, CI, staging, and production.
2. **Neon** is the MVP and scale-up cloud host. Use the **pooled** connection string with EF Core.
3. **Schema evolution** is through Entity Framework Core migrations checked into source control.
4. **SQLite** is for the Flutter device cache only — never the backend source of truth.
5. Railway may host the API. Do not use Railway Postgres for the points ledger.

---

## Testing strategy

- Persistence and identity integration tests run against **PostgreSQL**.
- Prefer `MOVOQ_TEST_DATABASE` (Neon branch or local Postgres). If unset, tests try Testcontainers (Docker).
- If neither is available, those tests skip. Unit tests always run.

---

## Client applications

| Surface | Stack |
|---------|--------|
| Admin / operations web | React |
| End-user mobile | Flutter (Android + iOS) |

Clients consume the .NET Web API. The API and PostgreSQL database remain the source of truth.

---

## Related documentation

- Backend layering: root `README.md`
- Architecture decisions: `docs/adr/`
