using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MOVOQ.Shared;

namespace MOVOQ.Api.Hosting;

internal static class RateLimitPolicies
{
    public const string Auth = "auth";

    public const string StepsSync = "steps-sync";

    public const string Spend = "spend";
}

internal static class RateLimitingExtensions
{
    internal static IServiceCollection AddMoVoqRateLimiting(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var testing = environment.IsEnvironment("Testing");
        var development = environment.IsDevelopment();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Max(1, (int)retryAfter.TotalSeconds).ToString();
                }

                await Results.Problem(
                        title: "Too many requests. Slow down and try again.",
                        statusCode: StatusCodes.Status429TooManyRequests,
                        extensions: new Dictionary<string, object?> { ["code"] = ErrorCodes.RateLimited })
                    .ExecuteAsync(context.HttpContext);
            };

            options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
                Partition(httpContext, Permit(testing, development, 60, 20)));
            options.AddPolicy(RateLimitPolicies.StepsSync, httpContext =>
                Partition(httpContext, Permit(testing, development, 120, 30)));
            options.AddPolicy(RateLimitPolicies.Spend, httpContext =>
                Partition(httpContext, Permit(testing, development, 60, 15)));
        });

        return services;
    }

    internal static IApplicationBuilder UseMoVoqRateLimiting(this IApplicationBuilder app)
    {
        app.UseRateLimiter();
        return app;
    }

    private static int Permit(bool testing, bool development, int developmentLimit, int productionLimit)
    {
        if (testing)
        {
            return 10_000;
        }

        return development ? developmentLimit : productionLimit;
    }

    private static RateLimitPartition<string> Partition(HttpContext httpContext, int permitLimit) =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true,
            });

    private static string GetKey(HttpContext httpContext) =>
        httpContext.User.FindFirstValue("sub")
        ?? httpContext.User.FindFirstValue("user_id")
        ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? httpContext.Connection.RemoteIpAddress?.ToString()
        ?? "anonymous";
}
