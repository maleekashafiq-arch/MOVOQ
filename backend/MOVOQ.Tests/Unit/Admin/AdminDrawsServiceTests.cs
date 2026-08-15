using MOVOQ.Application.Features.Admin;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;
using MOVOQ.Tests.Unit.Admin.Fakes;

namespace MOVOQ.Tests.Unit.Admin;

public sealed class AdminDrawsServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly AdminContext Operator = new(Guid.NewGuid(), "ops@movoq.test", "Ops", AdminRole.Admin);

    [Fact]
    public async Task A_new_draw_starts_as_a_draft()
    {
        var harness = new Harness();

        var created = await harness.Sut.CreateAsync(Operator, ValidCommand());

        Assert.Equal(LuckyDrawStatus.Draft, created.Status);
        Assert.False(created.IsWinnerSelected);
        Assert.Equal(0, created.Tickets);
        Assert.Single(harness.Draws.Items);
    }

    [Fact]
    public async Task Closing_an_active_draw_and_picking_a_winner_uses_one_of_the_active_tickets()
    {
        var harness = new Harness();
        var draw = await harness.CreateActiveDrawAsync();
        var walker = harness.AddWalker("walker@movoq.test");
        harness.AddTicket(draw, walker, "T-1");
        harness.AddTicket(draw, walker, "T-2");

        await harness.Sut.ChangeStatusAsync(Operator, draw.Id, LuckyDrawStatus.Closed);
        var detail = await harness.Sut.SelectWinnerAsync(Operator, draw.Id);

        Assert.Equal(LuckyDrawStatus.WinnerSelected, detail.Draw.Status);
        Assert.True(detail.Draw.IsWinnerSelected);
        Assert.NotNull(detail.Winner);
        Assert.Equal(walker.Id, detail.Winner!.UserId);
        Assert.Equal(PrizeFulfillmentStatus.AwaitingDetails, detail.Winner.FulfillmentStatus);
        Assert.Single(harness.Tickets.Items, t => t.Status == TicketStatus.Used);
        Assert.Single(harness.Audit.Items, e => e.ActionType == AuditActionType.DrawWinnerSelected);
    }

    [Fact]
    public async Task Picking_a_winner_twice_is_refused()
    {
        var harness = new Harness();
        var draw = await harness.CreateActiveDrawAsync();
        harness.AddTicket(draw, harness.AddWalker("walker@movoq.test"), "T-1");
        await harness.Sut.ChangeStatusAsync(Operator, draw.Id, LuckyDrawStatus.Closed);
        await harness.Sut.SelectWinnerAsync(Operator, draw.Id);

        var error = await Assert.ThrowsAsync<AdminException>(() => harness.Sut.SelectWinnerAsync(Operator, draw.Id));
        Assert.Equal(ErrorCodes.InvalidStateTransition, error.Code);
        Assert.Single(harness.Winners.Items);
    }

    [Fact]
    public async Task Cancelling_a_draw_voids_its_active_tickets()
    {
        var harness = new Harness();
        var draw = await harness.CreateActiveDrawAsync();
        harness.AddTicket(draw, harness.AddWalker("walker@movoq.test"), "T-1");

        var result = await harness.Sut.ChangeStatusAsync(Operator, draw.Id, LuckyDrawStatus.Cancelled);

        Assert.Equal(LuckyDrawStatus.Cancelled, result.Status);
        Assert.All(harness.Tickets.Items, t => Assert.Equal(TicketStatus.Cancelled, t.Status));
    }

    [Fact]
    public async Task Fulfillment_moves_a_prize_and_is_audited()
    {
        var harness = new Harness();
        var draw = await harness.CreateActiveDrawAsync();
        harness.AddTicket(draw, harness.AddWalker("walker@movoq.test"), "T-1");
        await harness.Sut.ChangeStatusAsync(Operator, draw.Id, LuckyDrawStatus.Closed);
        var detail = await harness.Sut.SelectWinnerAsync(Operator, draw.Id);

        var updated = await harness.Sut.UpdateFulfillmentAsync(
            Operator,
            detail.Winner!.Id,
            PrizeFulfillmentStatus.Shipped,
            "Courier booked");

        Assert.Equal(PrizeFulfillmentStatus.Shipped, updated.FulfillmentStatus);
        Assert.Equal("Courier booked", updated.Notes);
        Assert.Contains(harness.Audit.Items, e => e.ActionType == AuditActionType.PrizeFulfillmentUpdated);
    }

    private static SaveDrawCommand ValidCommand() => new(
        "August Grand Draw",
        "Monthly cash prize",
        "Rs 100,000 cash",
        "🎁",
        null,
        100,
        Now.AddDays(-1),
        Now.AddDays(10),
        10_000,
        true);

    private sealed class Harness
    {
        public Harness()
        {
            var time = new FixedTimeProvider(Now);
            Sut = new AdminDrawsService(
                Draws,
                Tickets,
                Winners,
                Users,
                new AuditTrail(Audit, new FakeCallerContext("203.0.113.7"), time),
                UnitOfWork,
                time);
        }

        public FakeLuckyDrawRepository Draws { get; } = new();
        public FakeLuckyDrawTicketRepository Tickets { get; } = new();
        public FakeLuckyDrawWinnerRepository Winners { get; } = new();
        public FakeUserRepository Users { get; } = new();
        public FakeAuditLogRepository Audit { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();
        public AdminDrawsService Sut { get; }

        public async Task<LuckyDraw> CreateActiveDrawAsync()
        {
            var created = await Sut.CreateAsync(Operator, ValidCommand());
            await Sut.ChangeStatusAsync(Operator, created.Id, LuckyDrawStatus.Active);
            return Draws.Items.Single(d => d.Id == created.Id);
        }

        public User AddWalker(string email)
        {
            var user = new User { Email = email, Profile = new UserProfile { DisplayName = "Walker" } };
            user.Profile.User = user;
            Users.Add(user);
            Tickets.UserNames[user.Id] = "Walker";
            Winners.UserNames[user.Id] = "Walker";
            return user;
        }

        public void AddTicket(LuckyDraw draw, User user, string number)
        {
            var ticket = new LuckyDrawTicket
            {
                UserId = user.Id,
                User = user,
                LuckyDrawId = draw.Id,
                LuckyDraw = draw,
                TicketNumber = number,
                PointsCostSnapshot = draw.TicketCostPoints,
                PurchasedAt = Now,
                Status = TicketStatus.Active,
            };
            Tickets.Add(ticket);
            Tickets.DrawNames[draw.Id] = draw.Name;
            draw.Tickets.Add(ticket);
        }
    }
}
