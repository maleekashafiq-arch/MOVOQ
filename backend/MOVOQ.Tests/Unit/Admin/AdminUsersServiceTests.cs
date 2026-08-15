using MOVOQ.Application.Features.Admin;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;
using MOVOQ.Tests.Unit.Admin.Fakes;

namespace MOVOQ.Tests.Unit.Admin;

public sealed class AdminUsersServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);
    private static readonly AdminContext Operator = new(Guid.NewGuid(), "ops@movoq.test", "Ops", AdminRole.Admin);

    [Fact]
    public async Task Crediting_points_posts_a_ledger_entry_and_moves_the_wallet()
    {
        var harness = new Harness();
        var user = harness.AddUser("walker@movoq.test", balance: 200);

        var result = await harness.Sut.AdjustPointsAsync(
            Operator,
            user.Id,
            new AdjustPointsCommand(150, "Goodwill for a failed redemption"));

        Assert.Equal(350, result.Balance);
        Assert.Equal(150, result.Amount);
        Assert.Equal(350, user.Wallet!.Balance);
        Assert.Equal(350, user.Wallet.LifetimeEarnedPoints);

        var entry = Assert.Single(harness.Ledger.Items);
        Assert.Equal(PointsTransactionType.AdminAdjustment, entry.TransactionType);
        Assert.Equal(LedgerReferenceType.AdminAdjustment, entry.ReferenceType);
        Assert.Contains("ops@movoq.test", entry.Description);
    }

    [Fact]
    public async Task Deducting_points_records_the_spend_against_the_lifetime_total()
    {
        var harness = new Harness();
        var user = harness.AddUser("walker@movoq.test", balance: 500);

        var result = await harness.Sut.AdjustPointsAsync(
            Operator,
            user.Id,
            new AdjustPointsCommand(-120, "Reversing a duplicate credit"));

        Assert.Equal(380, result.Balance);
        Assert.Equal(120, user.Wallet!.LifetimeSpentPoints);
    }

    [Fact]
    public async Task A_deduction_cannot_take_a_balance_below_zero()
    {
        var harness = new Harness();
        var user = harness.AddUser("walker@movoq.test", balance: 50);

        var error = await Assert.ThrowsAsync<AdminException>(() => harness.Sut.AdjustPointsAsync(
            Operator,
            user.Id,
            new AdjustPointsCommand(-100, "Clawback")));

        Assert.Equal(ErrorCodes.ValidationFailed, error.Code);
        Assert.Equal(50, user.Wallet!.Balance);
        Assert.Empty(harness.Ledger.Items);
        Assert.Empty(harness.Audit.Items);
    }

    [Theory]
    [InlineData(0, "Reason")]
    [InlineData(100, "")]
    [InlineData(100, "   ")]
    public async Task An_adjustment_needs_a_non_zero_amount_and_a_reason(long amount, string reason)
    {
        var harness = new Harness();
        var user = harness.AddUser("walker@movoq.test", balance: 500);

        var error = await Assert.ThrowsAsync<AdminException>(() =>
            harness.Sut.AdjustPointsAsync(Operator, user.Id, new AdjustPointsCommand(amount, reason)));

        Assert.Equal(ErrorCodes.ValidationFailed, error.Code);
        Assert.Empty(harness.Ledger.Items);
    }

    [Fact]
    public async Task Every_adjustment_leaves_an_audit_entry_with_the_balance_either_side()
    {
        var harness = new Harness();
        var user = harness.AddUser("walker@movoq.test", balance: 200);

        await harness.Sut.AdjustPointsAsync(Operator, user.Id, new AdjustPointsCommand(150, "Goodwill"));

        var entry = Assert.Single(harness.Audit.Items);
        Assert.Equal(AuditActionType.PointsAdjusted, entry.ActionType);
        Assert.Equal(Operator.AdminUserId, entry.AdminUserId);
        Assert.Equal(user.Id, entry.EntityId);
        Assert.Equal("200", entry.OldValue);
        Assert.Contains("350", entry.NewValue);
        Assert.Contains("Goodwill", entry.NewValue);
    }

    [Fact]
    public async Task Adjusting_an_unknown_user_reports_not_found()
    {
        var harness = new Harness();

        var error = await Assert.ThrowsAsync<AdminException>(() =>
            harness.Sut.AdjustPointsAsync(Operator, Guid.NewGuid(), new AdjustPointsCommand(10, "Test")));

        Assert.Equal(ErrorCodes.NotFound, error.Code);
        Assert.Equal(404, error.StatusCode);
    }

    [Fact]
    public async Task Suspending_an_account_changes_its_status_and_is_audited()
    {
        var harness = new Harness();
        var user = harness.AddUser("walker@movoq.test", balance: 0);

        var result = await harness.Sut.ChangeStatusAsync(
            Operator,
            user.Id,
            new ChangeUserStatusCommand(UserStatus.Suspended, "Step spike on three devices"));

        Assert.Equal(UserStatus.Suspended, result.Status);
        Assert.Equal(UserStatus.Suspended, user.Status);

        var entry = Assert.Single(harness.Audit.Items);
        Assert.Equal(AuditActionType.UserStatusChanged, entry.ActionType);
        Assert.Equal("Active", entry.OldValue);
        Assert.Contains("Step spike", entry.NewValue);
    }

    [Fact]
    public async Task Setting_the_status_it_already_has_writes_nothing()
    {
        var harness = new Harness();
        var user = harness.AddUser("walker@movoq.test", balance: 0);

        await harness.Sut.ChangeStatusAsync(Operator, user.Id, new ChangeUserStatusCommand(UserStatus.Active, null));

        Assert.Empty(harness.Audit.Items);
        Assert.Equal(0, harness.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task The_directory_filters_by_status_and_reports_the_totals_beside_it()
    {
        var harness = new Harness();
        harness.AddUser("one@movoq.test", balance: 10);
        harness.AddUser("two@movoq.test", balance: 20, status: UserStatus.Suspended);
        harness.AddUser("three@movoq.test", balance: 30);

        var page = await harness.Sut.ListAsync(null, UserStatus.Active, 1, 25);

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(2, page.ActiveCount);
        Assert.Equal(1, page.SuspendedCount);
        Assert.All(page.Items, item => Assert.Equal(UserStatus.Active, item.Status));
    }

    [Fact]
    public async Task An_oversized_page_request_is_clamped()
    {
        var harness = new Harness();
        harness.AddUser("one@movoq.test", balance: 10);

        var page = await harness.Sut.ListAsync(null, null, 0, 5000);

        Assert.Equal(1, page.Page);
        Assert.Equal(100, page.PageSize);
    }

    [Fact]
    public async Task A_user_without_a_profile_still_gets_a_readable_name()
    {
        var harness = new Harness();
        harness.AddUser("no.profile@movoq.test", balance: 0, displayName: null);

        var page = await harness.Sut.ListAsync(null, null, 1, 25);

        Assert.Equal("no.profile", page.Items.Single().Name);
    }

    [Fact]
    public async Task User_detail_fills_missing_days_in_the_step_history()
    {
        var harness = new Harness();
        var user = harness.AddUser("walker@movoq.test", balance: 0);
        harness.AddStepDay(user, Today, steps: 8200, goal: 6000, completed: true, points: 50);

        var detail = await harness.Sut.GetAsync(user.Id, 3);

        Assert.Equal(3, detail.StepHistory.Count);
        Assert.Equal([0, 0, 8200], detail.StepHistory.Select(d => d.Steps).ToArray());
        Assert.True(detail.StepHistory[^1].GoalReached);
        Assert.Equal(50, detail.StepHistory[^1].PointsAwarded);
    }

    private sealed class Harness
    {
        private readonly List<User> _users = [];

        public Harness()
        {
            var time = new FixedTimeProvider(Now);
            Reporting = new FakeAdminReportingRepository(_users);
            Sut = new AdminUsersService(
                Reporting,
                Users,
                Wallets,
                Ledger,
                StepRecords,
                new AuditTrail(Audit, new FakeCallerContext("203.0.113.7"), time),
                UnitOfWork,
                time);
        }

        public FakeUserRepository Users { get; } = new();

        public FakeWalletRepository Wallets { get; } = new();

        public FakeLedgerRepository Ledger { get; } = new();

        public FakeStepRecordRepository StepRecords { get; } = new();

        public FakeAuditLogRepository Audit { get; } = new();

        public FakeUnitOfWork UnitOfWork { get; } = new();

        public FakeAdminReportingRepository Reporting { get; }

        public AdminUsersService Sut { get; }

        public User AddUser(
            string email,
            long balance,
            UserStatus status = UserStatus.Active,
            string? displayName = "Walker")
        {
            var user = new User { Email = email, Status = status, CreatedAt = Now };
            user.Wallet = new UserWallet
            {
                UserId = user.Id,
                User = user,
                Balance = balance,
                LifetimeEarnedPoints = balance,
            };

            if (displayName is not null)
            {
                user.Profile = new UserProfile { UserId = user.Id, User = user, DisplayName = displayName };
            }

            Users.Add(user);
            _users.Add(user);
            return user;
        }

        public void AddStepDay(User user, DateOnly date, int steps, int goal, bool completed, long points) =>
            StepRecords.Add(new DailyStepRecord
            {
                UserId = user.Id,
                User = user,
                StepDate = date,
                ClaimedStepCount = steps,
                ValidatedStepCount = steps,
                DailyTargetStepsSnapshot = goal,
                TargetCompleted = completed,
                PointsAwarded = points > 0,
                PointsAmount = points,
            });
    }
}
