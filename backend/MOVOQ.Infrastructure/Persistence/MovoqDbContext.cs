using Microsoft.EntityFrameworkCore;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence;

public sealed class MovoqDbContext(DbContextOptions<MovoqDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<UserAuthProvider> UserAuthProviders => Set<UserAuthProvider>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<UserWallet> UserWallets => Set<UserWallet>();

    public DbSet<UserDevice> UserDevices => Set<UserDevice>();

    public DbSet<DailyStepRecord> DailyStepRecords => Set<DailyStepRecord>();

    public DbSet<StepGoal> StepGoals => Set<StepGoal>();

    public DbSet<PointsLedger> PointsLedgers => Set<PointsLedger>();

    public DbSet<AdsWatched> AdsWatched => Set<AdsWatched>();

    public DbSet<Sponsor> Sponsors => Set<Sponsor>();

    public DbSet<Reward> Rewards => Set<Reward>();

    public DbSet<RewardRedemption> RewardRedemptions => Set<RewardRedemption>();

    public DbSet<LuckyDraw> LuckyDraws => Set<LuckyDraw>();

    public DbSet<LuckyDrawTicket> LuckyDrawTickets => Set<LuckyDrawTicket>();

    public DbSet<LuckyDrawWinner> LuckyDrawWinners => Set<LuckyDrawWinner>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<FraudSignal> FraudSignals => Set<FraudSignal>();

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public DbSet<ProcessedCommand> ProcessedCommands => Set<ProcessedCommand>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampVersions()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version++;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovoqDbContext).Assembly);
    }
}
