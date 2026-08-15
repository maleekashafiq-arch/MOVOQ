using MOVOQ.Application.Features.Admin;
using MOVOQ.Shared;

namespace MOVOQ.Api.Hosting;

/// <summary>
/// Creates the operator accounts listed under <c>Admin:BootstrapEmails</c>. The admin API only trusts
/// emails already in <c>admin_users</c>, so without this there would be no way to sign in the first time.
/// </summary>
internal static class AdminBootstrap
{
    private const string DevelopmentOperator = "dev@localhost";

    internal static async Task EnsureOperatorsAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        var configured = app.Configuration.GetSection("Admin:BootstrapEmails").Get<string[]>() ?? [];

        // Matches the identity the local test auth handler falls back to, so `Bearer test` reaches the back office.
        var emails = app.Environment.IsDevelopment()
            ? configured.Append(DevelopmentOperator).ToArray()
            : configured;

        if (emails.Length == 0)
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(AdminBootstrap));

        try
        {
            var added = await scope.ServiceProvider
                .GetRequiredService<IAdminBootstrapper>()
                .EnsureOperatorsAsync(emails, AdminRole.SuperAdmin, cancellationToken);

            if (added > 0)
            {
                logger.LogInformation("Created {Count} back-office operator account(s) from configuration.", added);
            }
        }
        catch (Exception ex)
        {
            // A missing operator account blocks the admin app, not the mobile API. Never fail startup for it.
            logger.LogWarning(ex, "Could not ensure back-office operator accounts.");
        }
    }
}
