using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOVOQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordHashReference = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lucky_draws",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    PrizeDescription = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    TicketCostPoints = table.Column<long>(type: "INTEGER", nullable: false),
                    StartAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxTickets = table.Column<int>(type: "INTEGER", nullable: true),
                    IsWinnerSelected = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lucky_draws", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sponsors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ContactPersonName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sponsors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    PasswordHashReference = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ActionType = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OldValue = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_admin_users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "admin_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SponsorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    PointsCost = table.Column<long>(type: "INTEGER", nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TermsAndConditions = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rewards_sponsors_SponsorId",
                        column: x => x.SponsorId,
                        principalTable: "sponsors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fraud_signals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SignalType = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ReferenceType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fraud_signals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fraud_signals_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "points_ledgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<long>(type: "INTEGER", nullable: false),
                    TransactionType = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceType = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_points_ledgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_points_ledgers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "processed_commands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CommandId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CommandType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RelatedEntityName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_commands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_processed_commands_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "step_goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DailyTargetSteps = table.Column<int>(type: "INTEGER", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_step_goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_step_goals_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Platform = table.Column<int>(type: "INTEGER", nullable: false),
                    PushToken = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    DeviceModel = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    OsVersion = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CommandId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_devices_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    AvatarUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    PreferredLanguage = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Balance = table.Column<long>(type: "INTEGER", nullable: false),
                    LifetimeEarnedPoints = table.Column<long>(type: "INTEGER", nullable: false),
                    LifetimeSpentPoints = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_wallets_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lucky_draw_tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LuckyDrawId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TicketNumber = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsCostSnapshot = table.Column<long>(type: "INTEGER", nullable: false),
                    PointsLedgerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PurchasedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lucky_draw_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lucky_draw_tickets_lucky_draws_LuckyDrawId",
                        column: x => x.LuckyDrawId,
                        principalTable: "lucky_draws",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lucky_draw_tickets_points_ledgers_PointsLedgerId",
                        column: x => x.PointsLedgerId,
                        principalTable: "points_ledgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_lucky_draw_tickets_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reward_redemptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RewardId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsCostSnapshot = table.Column<long>(type: "INTEGER", nullable: false),
                    PointsLedgerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AssignedCode = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailureReason = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reward_redemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reward_redemptions_points_ledgers_PointsLedgerId",
                        column: x => x.PointsLedgerId,
                        principalTable: "points_ledgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reward_redemptions_rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reward_redemptions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ads_watched",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdType = table.Column<int>(type: "INTEGER", nullable: false),
                    AdProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DeviceWatchedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserDeviceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsRewarded = table.Column<bool>(type: "INTEGER", nullable: false),
                    RewardApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    PointsAwarded = table.Column<bool>(type: "INTEGER", nullable: false),
                    PointsAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    ValidatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValidationStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsLedgerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CommandId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ads_watched", x => x.Id);
                    table.CheckConstraint("CK_ads_watched_non_rewarded_no_points", "(IsRewarded = 1) OR (PointsAmount = 0 AND PointsAwarded = 0)");
                    table.ForeignKey(
                        name: "FK_ads_watched_points_ledgers_PointsLedgerId",
                        column: x => x.PointsLedgerId,
                        principalTable: "points_ledgers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ads_watched_user_devices_UserDeviceId",
                        column: x => x.UserDeviceId,
                        principalTable: "user_devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ads_watched_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "daily_step_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StepDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClaimedStepCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceRecordedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserDeviceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ValidatedStepCount = table.Column<int>(type: "INTEGER", nullable: true),
                    DailyTargetStepsSnapshot = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    PointsEligible = table.Column<bool>(type: "INTEGER", nullable: false),
                    PointsAwarded = table.Column<bool>(type: "INTEGER", nullable: false),
                    PointsAmount = table.Column<long>(type: "INTEGER", nullable: false),
                    ValidatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValidationStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsLedgerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CommandId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_step_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_daily_step_records_points_ledgers_PointsLedgerId",
                        column: x => x.PointsLedgerId,
                        principalTable: "points_ledgers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_daily_step_records_user_devices_UserDeviceId",
                        column: x => x.UserDeviceId,
                        principalTable: "user_devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_daily_step_records_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lucky_draw_winners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LuckyDrawId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LuckyDrawTicketId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SelectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SelectionMethod = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lucky_draw_winners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lucky_draw_winners_lucky_draw_tickets_LuckyDrawTicketId",
                        column: x => x.LuckyDrawTicketId,
                        principalTable: "lucky_draw_tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lucky_draw_winners_lucky_draws_LuckyDrawId",
                        column: x => x.LuckyDrawId,
                        principalTable: "lucky_draws",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lucky_draw_winners_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_users_Email",
                table: "admin_users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ads_watched_AdSessionId",
                table: "ads_watched",
                column: "AdSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ads_watched_CommandId",
                table: "ads_watched",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_ads_watched_PointsLedgerId",
                table: "ads_watched",
                column: "PointsLedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_ads_watched_UserDeviceId",
                table: "ads_watched",
                column: "UserDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_ads_watched_UserId",
                table: "ads_watched",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_Key",
                table: "app_settings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_AdminUserId",
                table: "audit_logs",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_daily_step_records_CommandId",
                table: "daily_step_records",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_daily_step_records_PointsLedgerId",
                table: "daily_step_records",
                column: "PointsLedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_daily_step_records_UserDeviceId",
                table: "daily_step_records",
                column: "UserDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_daily_step_records_UserId_StepDate",
                table: "daily_step_records",
                columns: new[] { "UserId", "StepDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fraud_signals_UserId_DetectedAt",
                table: "fraud_signals",
                columns: new[] { "UserId", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_lucky_draw_tickets_LuckyDrawId_TicketNumber",
                table: "lucky_draw_tickets",
                columns: new[] { "LuckyDrawId", "TicketNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lucky_draw_tickets_PointsLedgerId",
                table: "lucky_draw_tickets",
                column: "PointsLedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_lucky_draw_tickets_UserId",
                table: "lucky_draw_tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_lucky_draw_winners_LuckyDrawId",
                table: "lucky_draw_winners",
                column: "LuckyDrawId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lucky_draw_winners_LuckyDrawTicketId",
                table: "lucky_draw_winners",
                column: "LuckyDrawTicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lucky_draw_winners_UserId",
                table: "lucky_draw_winners",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId",
                table: "notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_points_ledgers_UserId_OccurredAt",
                table: "points_ledgers",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_processed_commands_CommandId",
                table: "processed_commands",
                column: "CommandId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_commands_ProcessedAt",
                table: "processed_commands",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_processed_commands_UserId",
                table: "processed_commands",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_reward_redemptions_PointsLedgerId",
                table: "reward_redemptions",
                column: "PointsLedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_reward_redemptions_RewardId",
                table: "reward_redemptions",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_reward_redemptions_UserId",
                table: "reward_redemptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_SponsorId",
                table: "rewards",
                column: "SponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_step_goals_UserId_IsActive",
                table: "step_goals",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_CommandId",
                table: "user_devices",
                column: "CommandId");

            migrationBuilder.CreateIndex(
                name: "IX_user_devices_UserId_DeviceIdentifier",
                table: "user_devices",
                columns: new[] { "UserId", "DeviceIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_UserId",
                table: "user_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_wallets_UserId",
                table: "user_wallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_PhoneNumber",
                table: "users",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ads_watched");

            migrationBuilder.DropTable(
                name: "app_settings");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "daily_step_records");

            migrationBuilder.DropTable(
                name: "fraud_signals");

            migrationBuilder.DropTable(
                name: "lucky_draw_winners");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "processed_commands");

            migrationBuilder.DropTable(
                name: "reward_redemptions");

            migrationBuilder.DropTable(
                name: "step_goals");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "user_wallets");

            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "user_devices");

            migrationBuilder.DropTable(
                name: "lucky_draw_tickets");

            migrationBuilder.DropTable(
                name: "rewards");

            migrationBuilder.DropTable(
                name: "lucky_draws");

            migrationBuilder.DropTable(
                name: "points_ledgers");

            migrationBuilder.DropTable(
                name: "sponsors");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
