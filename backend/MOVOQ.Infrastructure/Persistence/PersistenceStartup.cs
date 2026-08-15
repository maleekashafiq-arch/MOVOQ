using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MOVOQ.Infrastructure.Persistence;

public static class PersistenceStartup
{
    public static async Task ApplyMigrationsIfEnabledAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue("Persistence:ApplyMigrationsAtStartup", false))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MovoqDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
