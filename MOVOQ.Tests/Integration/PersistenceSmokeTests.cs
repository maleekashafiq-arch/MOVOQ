using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Infrastructure.Persistence;
using MOVOQ.Shared;

namespace MOVOQ.Tests.Integration;

[CollectionDefinition("Persistence sequential", DisableParallelization = true)]
public sealed class PersistenceSequentialCollectionDefinition;

[Collection("Persistence sequential")]
public sealed class PersistenceSmokeTests : IClassFixture<PersistenceWebApplicationFactory>, IAsyncLifetime
{
    private readonly PersistenceWebApplicationFactory _factory;

    public PersistenceSmokeTests(PersistenceWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        _ = _factory.CreateClient();
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MovoqDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Points_ledger_net_balance_matches_sum_of_amounts()
    {
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var ledger = scope.ServiceProvider.GetRequiredService<IPointsLedgerRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = new User { Email = "ledger.user@movoq.test" };
        users.Add(user);
        await uow.SaveChangesAsync();

        ledger.Add(new PointsLedger
        {
            User = user,
            UserId = user.Id,
            Amount = 42,
            TransactionType = PointsTransactionType.StepsEarned,
            ReferenceType = LedgerReferenceType.DailyStepRecord,
            OccurredAt = DateTime.UtcNow,
        });

        await uow.SaveChangesAsync();

        var balance = await ledger.GetNetBalanceAsync(user.Id);
        Assert.Equal(42, balance);
    }

    [Fact]
    public async Task User_profile_one_to_one_persists()
    {
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var db = scope.ServiceProvider.GetRequiredService<MovoqDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = new User { Email = "profile.user@movoq.test" };
        users.Add(user);
        await uow.SaveChangesAsync();

        db.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            User = user,
            DisplayName = "Profile Display",
        });

        await uow.SaveChangesAsync();

        var loaded = await db.UserProfiles.AsNoTracking().SingleAsync(p => p.UserId == user.Id);
        Assert.Equal("Profile Display", loaded.DisplayName);
    }

    [Fact]
    public async Task ProcessedCommand_duplicate_CommandId_throws()
    {
        var commandId = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MovoqDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        db.ProcessedCommands.Add(new ProcessedCommand
        {
            CommandId = commandId,
            CommandType = "TestCommand",
            Status = "Applied",
            ProcessedAt = DateTime.UtcNow,
        });
        await uow.SaveChangesAsync();

        db.ProcessedCommands.Add(new ProcessedCommand
        {
            CommandId = commandId,
            CommandType = "TestCommand",
            Status = "Applied",
            ProcessedAt = DateTime.UtcNow,
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => uow.SaveChangesAsync());
    }

    [Fact]
    public async Task ProcessedCommand_exists_by_command_id_reflects_store()
    {
        var commandId = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var processed = scope.ServiceProvider.GetRequiredService<IProcessedCommandRepository>();
        var db = scope.ServiceProvider.GetRequiredService<MovoqDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Assert.False(await processed.ExistsByCommandIdAsync(commandId));

        db.ProcessedCommands.Add(new ProcessedCommand
        {
            CommandId = commandId,
            CommandType = "TestCommand",
            Status = "Applied",
            ProcessedAt = DateTime.UtcNow,
        });
        await uow.SaveChangesAsync();

        Assert.True(await processed.ExistsByCommandIdAsync(commandId));
    }

    [Fact]
    public async Task AdsWatched_non_rewarded_with_points_violates_check_constraint()
    {
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var db = scope.ServiceProvider.GetRequiredService<MovoqDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = new User { Email = "ads.check@movoq.test" };
        users.Add(user);
        await uow.SaveChangesAsync();

        db.AdsWatched.Add(new AdsWatched
        {
            UserId = user.Id,
            User = user,
            AdSessionId = Guid.NewGuid(),
            DeviceWatchedAt = DateTime.UtcNow,
            IsRewarded = false,
            PointsAmount = 10,
            PointsAwarded = false,
            CommandId = Guid.NewGuid(),
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => uow.SaveChangesAsync());
    }
}
