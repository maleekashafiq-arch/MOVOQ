# MOVOQ database schema export & review (read-only)

> **Official MOVOQ direction:** **Microsoft SQL Server** for all development and testing; **Azure SQL** as the cloud target. See [`docs/architecture/technology-stack.md`](../architecture/technology-stack.md). **SQLite is not** part of the official backend strategy. This document remains a **point-in-time export** of the EF model and migration artifacts **as they existed when the export was produced** (including references to SQLite where the migration was generated for that provider).

**Generated from:** `MovoqDbContext`, `MOVOQ.Infrastructure/Persistence/Configurations/*`, and migration `20260504082507_InitialCreate` (plus model snapshot).  
**Provider at export time (historical / non-official):** SQLite (`Microsoft.EntityFrameworkCore.Sqlite`).  
**No database was updated; no migrations or entities were changed for this document.**

---

## Executive summary (for founders)

MOVOQ already has a **single “InitialCreate” migration** that defines **20 tables** in the export snapshot (SQLite-flavored SQL at generation time). Almost everything on your “core V1” checklist exists as real tables—**except a dedicated “social login / external auth” table** (`UserAuthProviders`), which is **not** in the codebase yet. Money-related history lives mainly in **`points_ledgers`** (no soft-delete hiding rows), while **`user_wallets`** can be soft-deleted like other profile-style data—so leadership should agree what “deleting a user” means for **balance history vs current balance**. **`processed_commands`** stores one row per client **`CommandId`**, and that id is **globally unique in the database**, which matches “replay the same command once for the whole system.” **Product database work should align with SQL Server / Azure SQL** per the official stack document—not with SQLite.

---

## 1. `MovoqDbContext` inspection

**File:** `MOVOQ.Infrastructure/Persistence/MovoqDbContext.cs`

- **DbSets:** 20 entity sets (Users through ProcessedCommands) — see table list below.
- **`OnModelCreating`:** Only calls `base.OnModelCreating` and `modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovoqDbContext).Assembly)`.
- **Implication:** There are **no inline** fluent configurations, **no** value converters, **no** default values, and **no** global query filters defined **on the context itself**. All of that lives in the separate configuration classes (or EF conventions).

---

## 2. Global query filters (complete list)

These use the pattern `DeletedAt == null` (soft-delete: hide rows where `DeletedAt` is set).

| Entity            | Configuration file                 | Table (snake_case)   |
|-------------------|------------------------------------|----------------------|
| User              | `UserConfiguration.cs`             | `users`              |
| UserProfile       | `UserProfileConfiguration.cs`      | `user_profiles`      |
| UserWallet        | `UserWalletConfiguration.cs`       | `user_wallets`       |
| UserDevice        | `UserDeviceConfiguration.cs`       | `user_devices`       |
| DailyStepRecord   | `DailyStepRecordConfiguration.cs`  | `daily_step_records` |
| StepGoal          | `StepGoalConfiguration.cs`         | `step_goals`         |
| AdsWatched        | `AdsWatchedConfiguration.cs`       | `ads_watched`        |
| Sponsor           | `SponsorConfiguration.cs`          | `sponsors`           |
| Reward            | `RewardConfiguration.cs`           | `rewards`            |
| RewardRedemption  | `RewardRedemptionConfiguration.cs` | `reward_redemptions` |
| LuckyDraw         | `LuckyDrawConfiguration.cs`        | `lucky_draws`        |
| LuckyDrawTicket   | `LuckyDrawTicketConfiguration.cs`  | `lucky_draw_tickets` |
| Notification      | `NotificationConfiguration.cs`     | `notifications`      |
| AdminUser         | `AdminUserConfiguration.cs`        | `admin_users`        |
| AppSetting        | `AppSettingConfiguration.cs`      | `app_settings`       |

**Entities with no global query filter (by configuration):**

- `PointsLedger` — append-only ledger (explicit comment in config).
- `AuditLog`, `FraudSignal`, `ProcessedCommand`, `LuckyDrawWinner` — no `DeletedAt` / no filter in configs.

---

## 3. Value converters & special column storage

| Location                         | What it does |
|----------------------------------|----------------|
| `DailyStepRecordConfiguration` | `StepDate` (`DateOnly` in C#) ↔ stored as `DateTime` at midnight (`TEXT` in the export’s SQLite-oriented migration). |

No other explicit `HasConversion` was found under `Persistence/Configurations/`.

**Note on types in this export:** Where the migration was SQLite-oriented, CLR `bool` mapped to `INTEGER`, and `Guid`/`DateTime`/enums appeared as `TEXT` or `INTEGER`. **SQL Server / Azure SQL** will use different physical types; treat this section as **conceptual** once the provider is SQL Server.

---

## 4. Connection string and file location (as observed at export time)

> **Official stack:** local and test workloads use **SQL Server**, not a file-based SQLite path. The table below describes **what was wired in configuration at the time of this export** and may be **historical** once the solution is switched fully to SQL Server.

| Source | What it said at export |
|--------|------------------------|
| `MOVOQ.Api/appsettings.json` | `ConnectionStrings:Database` = `Data Source=movoq.db` (relative path → file created next to the **process working directory**, often the API project folder when running from IDE). |
| `MOVOQ.Api/appsettings.Development.json` | Did **not** override `ConnectionStrings:Database`. |
| `DependencyInjection.cs` | Uses `configuration.GetConnectionString("Database")`; if missing, fell back to `Data Source=movoq.db`. |

**Environment variables (standard ASP.NET Core):**  
`ConnectionStrings__Database` can override the connection string without editing files (not referenced in code beyond normal configuration binding).

**Persistence:** `Persistence:ApplyMigrationsAtStartup` is `false` in base `appsettings.json` and `true` in `appsettings.Development.json` (behavior when running under Development only).

---

## 5. All tables (DbContext + migration)

Alphabetical by table name:

| # | Table name | Entity / DbSet |
|---|------------|----------------|
| 1 | `admin_users` | `AdminUser` |
| 2 | `ads_watched` | `AdsWatched` |
| 3 | `app_settings` | `AppSetting` |
| 4 | `audit_logs` | `AuditLog` |
| 5 | `daily_step_records` | `DailyStepRecord` |
| 6 | `fraud_signals` | `FraudSignal` |
| 7 | `lucky_draws` | `LuckyDraw` |
| 8 | `lucky_draw_tickets` | `LuckyDrawTicket` |
| 9 | `lucky_draw_winners` | `LuckyDrawWinner` |
| 10 | `notifications` | `Notification` |
| 11 | `points_ledgers` | `PointsLedger` |
| 12 | `processed_commands` | `ProcessedCommand` |
| 13 | `reward_redemptions` | `RewardRedemption` |
| 14 | `rewards` | `Reward` |
| 15 | `sponsors` | `Sponsor` |
| 16 | `step_goals` | `StepGoal` |
| 17 | `user_devices` | `UserDevice` |
| 18 | `user_profiles` | `UserProfile` |
| 19 | `user_wallets` | `UserWallet` |
| 20 | `users` | `User` |

---

## 6. Expected MOVOQ Core Entity V1 tables — coverage

| Expected (your list) | Present? | Actual table name |
|----------------------|------------|-------------------|
| Users | Yes | `users` |
| UserProfiles | Yes | `user_profiles` |
| **UserAuthProviders** | **No** | *Not in Domain / DbContext / migrations* |
| UserDevices | Yes | `user_devices` |
| DailyStepRecords | Yes | `daily_step_records` |
| StepGoals | Yes | `step_goals` |
| PointsLedger | Yes | `points_ledgers` |
| UserWallets | Yes | `user_wallets` |
| AdsWatched | Yes | `ads_watched` |
| Rewards | Yes | `rewards` |
| RewardRedemptions | Yes | `reward_redemptions` |
| Sponsors | Yes | `sponsors` |
| LuckyDraws | Yes | `lucky_draws` |
| LuckyDrawTickets | Yes | `lucky_draw_tickets` |
| LuckyDrawWinners | Yes | `lucky_draw_winners` |
| Notifications | Yes | `notifications` |
| FraudSignals | Yes | `fraud_signals` |
| AdminUsers | Yes | `admin_users` |
| AuditLogs | Yes | `audit_logs` |
| AppSettings | Yes | `app_settings` |
| ProcessedCommands | Yes | `processed_commands` |

---

## 7. `processed_commands` — `CommandId` uniqueness

- **Database:** Unique index `IX_processed_commands_CommandId` on column `CommandId` → **globally unique** across all rows (not combined with `UserId`, `DeviceId`, or `CommandType`).
- **Domain comment:** `ProcessedCommand` states one logical `CommandId` must not be applied twice across entity types.
- **`DeviceId` column:** Nullable `Guid`; **not** part of the unique index — used as metadata only unless you add a composite unique key later.

---

## 8. Per-table schema reference

Below, **PK** = primary key, **FK** = foreign key, **UQ** = unique constraint/index, **IX** = non-unique index.  
**Types** in the per-table sections follow the **export-time migration** (SQLite-oriented storage). **Enum** = stored as `INTEGER` in that snapshot. Under **SQL Server / Azure SQL**, physical types will differ.  
**Nullable** follows migration `nullable: true/false`.

### `admin_users` — `AdminUser`

| Column | Type | Nullable | Notes |
|--------|------|----------|--------|
| Id | TEXT (Guid) | no | PK |
| Email | TEXT(320) | no | |
| DisplayName | TEXT(256) | yes | |
| Role | INTEGER | no | enum |
| Status | INTEGER | no | enum |
| PasswordHashReference | TEXT(512) | yes | |
| LastLoginAt | TEXT | yes | DateTime |
| CreatedAt, UpdatedAt, DeletedAt | TEXT | mixed | soft-delete |
| Version | INTEGER | no | |

- **PK:** `Id`
- **FK:** none
- **Indexes:** `IX_admin_users_Email` **UQ**
- **Global filter:** `DeletedAt == null`

---

### `app_settings` — `AppSetting`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| Key | TEXT(256) | no |
| Value | TEXT | no |
| Description | TEXT(2000) | yes |
| LastModifiedBy | TEXT | yes |
| CreatedAt, UpdatedAt, DeletedAt | TEXT | mixed |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** none
- **Indexes:** `IX_app_settings_Key` **UQ**
- **Global filter:** yes

---

### `audit_logs` — `AuditLog`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| AdminUserId | TEXT | yes | FK → `admin_users` SET NULL |
| ActionType | INTEGER | no | enum |
| EntityName | TEXT(256) | no |
| EntityId | TEXT | yes |
| OldValue, NewValue | TEXT(8000) | yes |
| IpAddress | TEXT(64) | yes |
| CreatedAt | TEXT | no |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** `AdminUserId` → `admin_users`
- **Indexes:** `IX_audit_logs_AdminUserId`
- **Global filter:** none (append-only style; no `DeletedAt` column)

---

### `users` — `User`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| Email | TEXT(320) | yes |
| PhoneNumber | TEXT(32) | yes |
| PasswordHashReference | TEXT(512) | yes |
| Role | INTEGER | no | enum |
| Status | INTEGER | no | enum |
| LastLoginAt | TEXT | yes |
| CreatedAt, UpdatedAt, DeletedAt | TEXT | mixed |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** none (parent for many children)
- **Indexes:** `IX_users_Email` **UQ** with filter `[Email] IS NOT NULL`; `IX_users_PhoneNumber` **UQ** with filter `[PhoneNumber] IS NOT NULL`
- **Global filter:** yes

---

### `user_profiles` — `UserProfile`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| UserId | TEXT | no | FK → `users` CASCADE |
| DisplayName, AvatarUrl, Gender | TEXT | yes |
| DateOfBirth, Country, City, PreferredLanguage | TEXT | mixed |
| CreatedAt, UpdatedAt, DeletedAt | TEXT | mixed |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** `UserId` → `users`
- **Indexes:** `IX_user_profiles_UserId` **UQ**
- **Global filter:** yes

---

### `user_wallets` — `UserWallet`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| UserId | TEXT | no | FK → `users` CASCADE |
| Balance, LifetimeEarnedPoints, LifetimeSpentPoints | INTEGER (long) | no |
| CreatedAt, UpdatedAt, DeletedAt | TEXT | mixed |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** `UserId` → `users`
- **Indexes:** `IX_user_wallets_UserId` **UQ**
- **Global filter:** yes (financial **projection** row can disappear from default queries if soft-deleted)

---

### `user_devices` — `UserDevice`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| UserId | TEXT | no | FK → `users` RESTRICT |
| DeviceIdentifier | TEXT(256) | no |
| Platform | INTEGER | no | enum |
| PushToken | TEXT(512) | yes |
| DeviceModel | TEXT(128) | yes |
| OsVersion | TEXT(64) | yes |
| RegisteredAt | TEXT | no |
| LastSeenAt | TEXT | yes |
| IsActive | INTEGER (bool) | no |
| CommandId | TEXT | no |
| DeviceId | TEXT | yes |
| Source | INTEGER | no | enum |
| SyncedAt | TEXT | yes |
| CreatedAt, UpdatedAt, DeletedAt | TEXT | mixed |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** `UserId` → `users`
- **Indexes:** `IX_user_devices_UserId_DeviceIdentifier` **UQ**; `IX_user_devices_CommandId`
- **Global filter:** yes

---

### `step_goals` — `StepGoal`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| UserId | TEXT | no | FK → `users` |
| DailyTargetSteps | INTEGER | no |
| EffectiveFrom, EffectiveTo | TEXT | mixed |
| IsActive | INTEGER | no |
| CreatedAt, UpdatedAt, DeletedAt | TEXT | mixed |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** `UserId` → `users` RESTRICT
- **Indexes:** `IX_step_goals_UserId_IsActive`
- **Global filter:** yes

---

### `points_ledgers` — `PointsLedger`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| UserId | TEXT | no | FK → `users` RESTRICT |
| Amount | INTEGER (long) | no |
| TransactionType | INTEGER | no | enum |
| ReferenceType | INTEGER | no | enum |
| ReferenceId | TEXT | yes |
| Description | TEXT(1024) | yes |
| OccurredAt | TEXT | no |
| CreatedAt | TEXT | no |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** `UserId` → `users`
- **Indexes:** `IX_points_ledgers_UserId_OccurredAt`
- **Global filter:** none
- **Relationships:** referenced by `PointsLedgerId` on `ads_watched`, `daily_step_records`, `lucky_draw_tickets`, `reward_redemptions` (FKs in migration)

---

### `processed_commands` — `ProcessedCommand`

| Column | Type | Nullable |
|--------|------|----------|
| Id | TEXT | no | PK |
| CommandId | TEXT | no |
| UserId | TEXT | yes | FK → `users` SET NULL |
| DeviceId | TEXT | yes |
| CommandType | TEXT(128) | no |
| Status | TEXT(64) | no |
| ProcessedAt | TEXT | no |
| RelatedEntityName | TEXT(256) | yes |
| RelatedEntityId | TEXT | yes |
| CreatedAt | TEXT | no |
| Version | INTEGER | no |

- **PK:** `Id`
- **FK:** `UserId` → `users`
- **Indexes:** `IX_processed_commands_CommandId` **UQ**; `IX_processed_commands_ProcessedAt`; `IX_processed_commands_UserId`
- **Global filter:** none

---

### `sponsors` — `Sponsor`

(Contact fields, `IsActive`, audit columns — see migration lines 79–97.)

- **PK:** `Id`
- **FK:** none (parent of `rewards`)
- **Indexes:** (none beyond PK in migration)
- **Global filter:** yes

---

### `rewards` — `Reward`

- **PK:** `Id`
- **FK:** `SponsorId` → `sponsors` RESTRICT
- **Indexes:** `IX_rewards_SponsorId`
- **Global filter:** yes  
- **Enums:** `Status` → `RewardStatus`

---

### `reward_redemptions` — `RewardRedemption`

- **PK:** `Id`
- **FK:** `UserId` → `users`; `RewardId` → `rewards`; `PointsLedgerId` → `points_ledgers` SET NULL
- **Indexes:** on `UserId`, `RewardId`, `PointsLedgerId`
- **Global filter:** yes  
- **Enums:** `Status` → `RedemptionStatus`

---

### `lucky_draws` — `LuckyDraw`

- **PK:** `Id`
- **FK:** none
- **Global filter:** yes  
- **Enums:** `Status` → `LuckyDrawStatus`

---

### `lucky_draw_tickets` — `LuckyDrawTicket`

- **PK:** `Id`
- **FK:** `UserId` → `users`; `LuckyDrawId` → `lucky_draws`; `PointsLedgerId` → `points_ledgers` SET NULL
- **Indexes:** `IX_lucky_draw_tickets_LuckyDrawId_TicketNumber` **UQ**; others on `UserId`, `PointsLedgerId`
- **Global filter:** yes  
- **Enums:** `Status` → `TicketStatus`

---

### `lucky_draw_winners` — `LuckyDrawWinner`

- **PK:** `Id`
- **FK:** `LuckyDrawId` → `lucky_draws`; `LuckyDrawTicketId` → `lucky_draw_tickets`; `UserId` → `users` (all RESTRICT)
- **Indexes:** `IX_lucky_draw_winners_LuckyDrawId` **UQ**; `IX_lucky_draw_winners_LuckyDrawTicketId` **UQ**; `IX_lucky_draw_winners_UserId`
- **Global filter:** none

---

### `ads_watched` — `AdsWatched`

- **PK:** `Id`
- **FK:** `UserId` → `users`; `UserDeviceId` → `user_devices` SET NULL; `PointsLedgerId` → `points_ledgers` (no explicit ON DELETE in migration snippet)
- **Check constraint:** `CK_ads_watched_non_rewarded_no_points`: `(IsRewarded = 1) OR (PointsAmount = 0 AND PointsAwarded = 0)`
- **Indexes:** `IX_ads_watched_AdSessionId` **UQ**; `CommandId`, `PointsLedgerId`, `UserDeviceId`, `UserId`
- **Global filter:** yes  
- **Enums:** `AdType`, `ValidationStatus`, `Source` (`DataSource`)

---

### `daily_step_records` — `DailyStepRecord`

- **PK:** `Id`
- **FK:** `UserId` → `users`; `UserDeviceId` → `user_devices` SET NULL; `PointsLedgerId` → `points_ledgers`
- **Indexes:** `IX_daily_step_records_UserId_StepDate` **UQ**; `CommandId`, `PointsLedgerId`, `UserDeviceId`
- **Global filter:** yes  
- **Enums:** `ValidationStatus`, `Source`  
- **Note:** `StepDate` stored as `TEXT` (`DateTime` at date boundary) due to `DateOnly` conversion.

---

### `notifications` — `Notification`

- **PK:** `Id`
- **FK:** `UserId` → `users`
- **Indexes:** `IX_notifications_UserId`
- **Global filter:** yes  
- **Enums:** `Type` → `NotificationType`

---

### `fraud_signals` — `FraudSignal`

- **PK:** `Id`
- **FK:** `UserId` → `users` RESTRICT
- **Indexes:** `IX_fraud_signals_UserId_DetectedAt`
- **Global filter:** none  
- **Enums:** `SignalType`, `Severity`

---

## 9. Entity / configuration / migration consistency

- **DbContext, configurations, and `InitialCreate` align** on the same 20 tables and snake_case names.
- **EF model validation warnings** (observed when tooling builds the model): required relationships where the principal (`User`, `LuckyDraw`) has a **global query filter** but the dependent (`FraudSignal`, `LuckyDrawWinner`, `PointsLedger`) does **not** — can cause surprising query results if principals are soft-deleted. This is a **runtime modeling** concern, not a literal mismatch between files.

---

## 10. Risks: aligning the model with SQL Server and Azure SQL

**Official targets:** **SQL Server** (local + tests) and **Azure SQL** (cloud). PostgreSQL is **not** in scope.

| Area | Risk / note |
|------|-------------|
| **Types** | If any artifact was generated against SQLite, column types in SQL may differ from SQL Server/Azure SQL (`uniqueidentifier`, `datetime2`, `bigint`, `bit`, etc.). Regenerate or review migrations under the **SQL Server** provider before treating SQL as canonical. |
| **Filtered unique indexes** | `users` email/phone indexes used filtered unique syntax in the export — verify **T-SQL** filtered indexes on SQL Server / Azure SQL. |
| **CHECK constraints** | Re-verify `ads_watched` rules on SQL Server (`bit` / boolean semantics). |
| **Subqueries in CHECK** | A “winner UserId = ticket UserId” DB check was not used when SQLite forbade subqueries in CHECK; **Azure SQL / SQL Server** can support richer CHECK or use triggers—apply via an explicit, reviewed migration if still desired. |
| **Concurrency** | `Version` columns exist; validate optimistic concurrency behavior under **SQL Server** row/versioning, not file-based engines. |
| **Legacy SQLite** | Any remaining SQLite-only assumptions in docs or old migration files are **historical**; do not treat them as the architecture direction. |

---

## 11. Soft-delete filters vs financial / audit data

| Type | Tables | Risk |
|------|--------|------|
| **Append-only / compliance** | `points_ledgers`, `audit_logs`, `fraud_signals`, `processed_commands`, `lucky_draw_winners` | No soft-delete filter — good for **immutable history** and idempotency receipts. |
| **Wallet (aggregated)** | `user_wallets` | **Has** soft-delete filter — default EF queries **hide** soft-deleted wallets while **ledger rows remain visible** → risk of **conceptual inconsistency** unless product rules define “wallet row lifecycle” vs ledger. |
| **Operational data** | users, devices, steps, ads, rewards, etc. | Soft-delete is normal; ensure admin/reporting queries use `IgnoreQueryFilters()` where business must see deleted users’ ledger history. |

---

## 12. Recommendation (strategy only — no code change here)

| Option | Guidance |
|--------|----------|
| **Keep EF Core migrations** | **Recommended.** You already have a coherent model and one baseline migration; this is the standard path for .NET with **SQL Server** and **Azure SQL** when using the SQL Server provider and testing `dotnet ef database update` on a throwaway server. |
| **Modify DB visibility** | Use **separate connection strings** per environment and restricted DB users for least privilege; does not replace migrations. |
| **Change database approach** | Only consider if you abandon relational schema or EF (large cost); **not** suggested from this review alone. |

---

## 13. Migration artifacts on disk

| File | Role |
|------|------|
| `Persistence/Migrations/20260504082507_InitialCreate.cs` | Up/Down SQL as generated at export time (SQLite provider — **historical / non-official** per current stack) |
| `Persistence/Migrations/20260504082507_InitialCreate.Designer.cs` | Designer snapshot for that migration |
| `Persistence/Migrations/MovoqDbContextModelSnapshot.cs` | Current aggregate model |

---

*End of export / review document.*
