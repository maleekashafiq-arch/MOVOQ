using MOVOQ.Application.Features.Admin;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;
using MOVOQ.Tests.Unit.Admin.Fakes;

namespace MOVOQ.Tests.Unit.Admin;

public sealed class AdminOperatorsServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly AdminContext Super = new(Guid.NewGuid(), "owner@movoq.test", "Owner", AdminRole.SuperAdmin);

    [Fact]
    public async Task Adding_an_operator_puts_their_email_on_the_allowlist()
    {
        var harness = new Harness();
        harness.SeedSuper();

        var created = await harness.Sut.CreateAsync(
            Super,
            new SaveOperatorCommand("Ops@Movoq.Test", "Ops", AdminRole.Admin, AdminStatus.Active));

        Assert.Equal("ops@movoq.test", created.Email);
        Assert.Equal(AdminRole.Admin, created.Role);
        Assert.Equal(2, harness.Operators.Items.Count);
    }

    [Fact]
    public async Task The_last_active_superadmin_cannot_be_demoted()
    {
        var harness = new Harness();
        var owner = harness.SeedSuper();

        var error = await Assert.ThrowsAsync<AdminException>(() => harness.Sut.UpdateAsync(
            Super,
            owner.Id,
            new SaveOperatorCommand(owner.Email, owner.DisplayName, AdminRole.Admin, AdminStatus.Active)));

        Assert.Equal(ErrorCodes.InvalidStateTransition, error.Code);
        Assert.Equal(AdminRole.SuperAdmin, harness.Operators.Items.Single().Role);
    }

    [Fact]
    public async Task A_duplicate_email_is_refused()
    {
        var harness = new Harness();
        harness.SeedSuper();
        await harness.Sut.CreateAsync(
            Super,
            new SaveOperatorCommand("ops@movoq.test", null, AdminRole.Support, AdminStatus.Active));

        var error = await Assert.ThrowsAsync<AdminException>(() => harness.Sut.CreateAsync(
            Super,
            new SaveOperatorCommand("ops@movoq.test", null, AdminRole.Admin, AdminStatus.Active)));

        Assert.Equal(ErrorCodes.ValidationFailed, error.Code);
    }

    private sealed class Harness
    {
        public Harness()
        {
            var time = new FixedTimeProvider(Now);
            Sut = new AdminOperatorsService(
                Operators,
                new AuditTrail(Audit, new FakeCallerContext("203.0.113.7"), time),
                UnitOfWork,
                time);
        }

        public FakeAdminUserRepository Operators { get; } = new();
        public FakeAuditLogRepository Audit { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();
        public AdminOperatorsService Sut { get; }

        public AdminUser SeedSuper()
        {
            var owner = new AdminUser
            {
                Email = Super.Email,
                DisplayName = Super.DisplayName,
                Role = AdminRole.SuperAdmin,
                Status = AdminStatus.Active,
                CreatedAt = Now,
            };
            Operators.Add(owner);
            return owner;
        }
    }
}
