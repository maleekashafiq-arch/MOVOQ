using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Infrastructure.Persistence;
using MOVOQ.Infrastructure.Persistence.Repositories;

namespace MOVOQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Data Source=movoq.db";
        }

        services.AddDbContext<MovoqDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPointsLedgerRepository, PointsLedgerRepository>();
        services.AddScoped<IProcessedCommandRepository, ProcessedCommandRepository>();

        return services;
    }
}
