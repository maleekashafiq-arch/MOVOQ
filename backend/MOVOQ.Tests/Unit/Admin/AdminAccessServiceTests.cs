using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Features.Admin;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;
using MOVOQ.Tests.Unit.Admin.Fakes;

namespace MOVOQ.Tests.Unit.Admin;

public sealed class AdminAccessServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task A_signed_in_user_who_is_not_an_operator_is_refused()
    {
        var harness = new Harness();

        var error = await Assert.ThrowsAsync<AdminException>(() =>
            harness.Sut.RequireAsync(Identity("walker@movoq.test"), AdminRole.ReadOnly));

        Assert.Equal(ErrorCodes.AdminAccessDenied, error.Code);
        Assert.Equal(403, error.StatusCode);
    }

    [Fact]
    public async Task A_suspended_operator_is_refused()
    {
        var harness = new Harness();
        harness.AddOperator("ops@movoq.test", AdminRole.Admin, AdminStatus.Suspended);

        var error = await Assert.ThrowsAsync<AdminException>(() =>
            harness.Sut.RequireAsync(Identity("ops@movoq.test"), AdminRole.ReadOnly));

        Assert.Equal(ErrorCodes.AdminAccessDenied, error.Code);
    }

    [Fact]
    public async Task An_operator_below_the_required_role_is_refused()
    {
        var harness = new Harness();
        harness.AddOperator("viewer@movoq.test", AdminRole.ReadOnly);

        var error = await Assert.ThrowsAsync<AdminException>(() =>
            harness.Sut.RequireAsync(Identity("viewer@movoq.test"), AdminRole.Admin));

        Assert.Equal(ErrorCodes.AdminRoleRequired, error.Code);
        Assert.Equal(403, error.StatusCode);
    }

    [Theory]
    [InlineData(AdminRole.SuperAdmin, AdminRole.Admin)]
    [InlineData(AdminRole.Admin, AdminRole.Admin)]
    [InlineData(AdminRole.Admin, AdminRole.ReadOnly)]
    [InlineData(AdminRole.Support, AdminRole.ReadOnly)]
    public async Task A_role_satisfies_everything_at_or_below_it(AdminRole held, AdminRole required)
    {
        var harness = new Harness();
        harness.AddOperator("ops@movoq.test", held);

        var context = await harness.Sut.RequireAsync(Identity("ops@movoq.test"), required);

        Assert.Equal(held, context.Role);
        Assert.Equal("ops@movoq.test", context.Email);
    }

    [Fact]
    public async Task Email_matching_ignores_case_and_surrounding_space()
    {
        var harness = new Harness();
        harness.AddOperator("ops@movoq.test", AdminRole.Admin);

        var context = await harness.Sut.RequireAsync(Identity("  OPS@Movoq.Test "), AdminRole.Admin);

        Assert.Equal("ops@movoq.test", context.Email);
    }

    [Fact]
    public async Task Signing_in_stamps_the_login_and_audits_it()
    {
        var harness = new Harness();
        harness.AddOperator("ops@movoq.test", AdminRole.SuperAdmin);

        var session = await harness.Sut.SignInAsync(Identity("ops@movoq.test"));

        Assert.Equal(AdminRole.SuperAdmin, session.Role);
        Assert.Null(session.LastLoginAt);
        Assert.Equal(Now, harness.Admins.Items.Single().LastLoginAt);

        var entry = Assert.Single(harness.Audit.Items);
        Assert.Equal(AuditActionType.AdminLogin, entry.ActionType);
        Assert.Equal("203.0.113.7", entry.IpAddress);
    }

    [Fact]
    public async Task An_identity_without_an_email_cannot_be_matched_to_an_operator()
    {
        var harness = new Harness();

        var error = await Assert.ThrowsAsync<AdminException>(() =>
            harness.Sut.RequireAsync(new ExternalIdentity("uid", string.Empty, true, null), AdminRole.ReadOnly));

        Assert.Equal(ErrorCodes.EmailRequired, error.Code);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    public async Task An_unverified_Firebase_email_cannot_open_the_back_office()
    {
        var harness = new Harness();
        harness.AddOperator("ops@movoq.test", AdminRole.Admin);

        var error = await Assert.ThrowsAsync<AdminException>(() =>
            harness.Sut.RequireAsync(new ExternalIdentity("uid-1", "ops@movoq.test", false, "Operator"), AdminRole.ReadOnly));

        Assert.Equal(ErrorCodes.EmailNotVerified, error.Code);
        Assert.Equal(403, error.StatusCode);
    }

    [Fact]
    public async Task The_local_test_identity_does_not_need_a_verified_inbox()
    {
        var harness = new Harness();
        harness.AddOperator("dev@localhost", AdminRole.SuperAdmin);

        var context = await harness.Sut.RequireAsync(
            new ExternalIdentity(AdminAccessService.LocalTestUserId, "dev@localhost", false, "Local Walker"),
            AdminRole.SuperAdmin);

        Assert.Equal("dev@localhost", context.Email);
    }

    private static ExternalIdentity Identity(string email) => new("uid-1", email, true, "Operator");

    private sealed class Harness
    {
        public Harness()
        {
            var time = new FixedTimeProvider(Now);
            Sut = new AdminAccessService(
                Admins,
                new AuditTrail(Audit, new FakeCallerContext("203.0.113.7"), time),
                new FakeUnitOfWork(),
                time);
        }

        public FakeAdminUserRepository Admins { get; } = new();

        public FakeAuditLogRepository Audit { get; } = new();

        public AdminAccessService Sut { get; }

        public void AddOperator(string email, AdminRole role, AdminStatus status = AdminStatus.Active) =>
            Admins.Add(new AdminUser
            {
                Email = email,
                DisplayName = email,
                Role = role,
                Status = status,
            });
    }
}
