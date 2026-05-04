# MOVOQ — official technology stack

This document records the **final** MOVOQ technology decisions for the product. It supersedes earlier exploratory notes about other databases or local-only stores.

---

## Platform overview

| Area | Technology |
|------|------------|
| **Backend API** | ASP.NET Core Web API (.NET) |
| **Architecture** | Clean Architecture (Domain → Application → Infrastructure; API as composition root) |
| **ORM** | Entity Framework Core |
| **Official database (all environments)** | **Microsoft SQL Server** |
| **Local development** | SQL Server (Developer, Express, Docker, or LocalDB) — same engine family as production |
| **Automated testing** | **SQL Server** — integration and persistence tests run against SQL Server, not lightweight file databases |
| **Cloud database target** | **Azure SQL** (Azure SQL Database or Azure SQL Managed Instance, per deployment choice) |
| **Admin web application** | React |
| **Mobile applications** | Flutter |

PostgreSQL is **not** part of the official MOVOQ backend or data-store direction.

---

## Database strategy

1. **SQL Server is the official relational database** for MOVOQ backend development, CI, staging, and production paths aligned with this stack.
2. **Azure SQL** is the designated **future (and primary) cloud** relational target on Microsoft Azure, using the same logical schema and EF Core migrations where applicable.
3. **Schema evolution** continues through **Entity Framework Core migrations** checked into source control; environments apply migrations with an agreed, repeatable process (no ad-hoc drift).
4. **SQLite** is **not** part of the official backend database strategy. If it appears in older branches, docs, or one-off exports, treat it as **historical or temporary** only—not a decision equal to SQL Server.

---

## Testing strategy

- **Integration and persistence tests** should use **SQL Server** (dedicated test instance, container, or ephemeral database), so behavior matches production types, constraints, locking, and T-SQL semantics.
- Avoid relying on **SQLite** (or other non–SQL Server engines) for authoritative backend test suites once the solution is wired to SQL Server; parity gaps can hide real production issues.
- Unit tests that do not touch the database remain engine-agnostic.

---

## Client applications (reference)

| Surface | Stack |
|---------|--------|
| Admin / operations web | React |
| End-user and related mobile apps | Flutter |

These clients consume the .NET Web API; the API and database remain the **source of truth** for business rules and persisted state (see ADR-001 and related ADRs).

---

## Related documentation

- Backend layering and dependency rules: root `README.md`
- Database schema review (point-in-time export; may reference legacy provider in filenames only): `docs/database/movoq-schema-export-review.md`
- Architecture decisions: `docs/adr/`
