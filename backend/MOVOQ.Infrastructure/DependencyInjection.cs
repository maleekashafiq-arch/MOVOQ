using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Admin;
using MOVOQ.Application.Features.Dev;
using MOVOQ.Application.Features.Draws;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Application.Features.Marketplace;
using MOVOQ.Application.Features.Steps;
using MOVOQ.Infrastructure.Persistence;
using MOVOQ.Infrastructure.Persistence.Repositories;
using Npgsql;

namespace MOVOQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = NormalizeNpgsqlConnectionString(configuration.GetConnectionString("Database"));
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Database' is required. Use a Neon (or other PostgreSQL) connection string.");
        }

        services.AddDbContext<MovoqDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAuthProviderRepository, UserAuthProviderRepository>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
        services.AddScoped<IUserWalletRepository, UserWalletRepository>();
        services.AddScoped<IStepGoalRepository, StepGoalRepository>();
        services.AddScoped<IDailyStepRecordRepository, DailyStepRecordRepository>();
        services.AddScoped<IPointsLedgerRepository, PointsLedgerRepository>();
        services.AddScoped<IProcessedCommandRepository, ProcessedCommandRepository>();
        services.AddScoped<IFraudSignalRepository, FraudSignalRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();
        services.AddScoped<IAdminReportingRepository, AdminReportingRepository>();
        services.AddScoped<ILuckyDrawRepository, LuckyDrawRepository>();
        services.AddScoped<ILuckyDrawTicketRepository, LuckyDrawTicketRepository>();
        services.AddScoped<ILuckyDrawWinnerRepository, LuckyDrawWinnerRepository>();
        services.AddScoped<ISponsorRepository, SponsorRepository>();
        services.AddScoped<IRewardRepository, RewardRepository>();
        services.AddScoped<IRewardRedemptionRepository, RewardRedemptionRepository>();

        // Identity is registered concretely so features that need the User entity can share the same scoped instance.
        services.AddScoped<IdentityService>();
        services.AddScoped<IIdentityService>(sp => sp.GetRequiredService<IdentityService>());
        services.AddScoped<IUserProvisioning>(sp => sp.GetRequiredService<IdentityService>());
        services.AddScoped<IStepsService, StepsService>();
        services.AddScoped<ILuckyDrawService, LuckyDrawService>();
        services.AddScoped<IMarketplaceService, MarketplaceService>();
        services.AddScoped<IDevUserService, DevUserService>();

        services.AddScoped<IAuditTrail, AuditTrail>();
        // Both admin services cache per request, so they are registered concretely and shared by interface.
        services.AddScoped<AdminAccessService>();
        services.AddScoped<IAdminAccessService>(sp => sp.GetRequiredService<AdminAccessService>());
        services.AddScoped<PlatformSettingsService>();
        services.AddScoped<IPlatformSettingsService>(sp => sp.GetRequiredService<PlatformSettingsService>());
        services.AddScoped<IAdminUsersService, AdminUsersService>();
        services.AddScoped<IAdminInsightsService, AdminInsightsService>();
        services.AddScoped<IAdminDrawsService, AdminDrawsService>();
        services.AddScoped<IAdminMarketplaceService, AdminMarketplaceService>();
        services.AddScoped<IAdminOperatorsService, AdminOperatorsService>();
        services.AddScoped<IAdminBootstrapper, AdminBootstrapper>();

        services.TryAddSingleton(TimeProvider.System);

        return services;
    }

    /// <summary>
    /// Neon copies a <c>postgresql://</c> URI. Npgsql's keyword parser treats <c>&amp;</c> as a separator,
    /// so convert URIs to <c>Host=...;SSL Mode=Require</c> form.
    /// </summary>
    internal static string? NormalizeNpgsqlConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            SslMode = SslMode.Require,
        };

        if (userInfo.Length > 1)
        {
            builder.Password = Uri.UnescapeDataString(userInfo[1]);
        }

        return builder.ConnectionString;
    }
}
