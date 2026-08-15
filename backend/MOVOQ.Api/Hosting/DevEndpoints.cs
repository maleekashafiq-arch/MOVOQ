using MOVOQ.Application.Features.Dev;
using MOVOQ.Application.Features.Identity;

namespace MOVOQ.Api.Hosting;

internal static class DevEndpoints
{
    internal static WebApplication MapDevDashboard(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapGet("/dev", ShowDashboard)
            .WithName("DevDashboard")
            .WithTags("dev")
            .AllowAnonymous();
        app.MapPost("/dev/users", CreateFromForm)
            .WithName("DevCreateUserForm")
            .WithTags("dev")
            .AllowAnonymous();
        app.MapPost("/dev/seed", SeedFromForm)
            .WithName("DevSeedUsersForm")
            .WithTags("dev")
            .AllowAnonymous();

        var api = app.MapGroup("/api/v1/dev").WithTags("dev").AllowAnonymous();
        api.MapGet("/users", ListUsers).WithName("DevListUsers");
        api.MapPost("/users", CreateUser).WithName("DevCreateUser");
        api.MapPost("/users/seed", SeedUsers).WithName("DevSeedUsers");
        return app;
    }

    internal static async Task SeedIfDevelopmentAsync(WebApplication app, CancellationToken cancellationToken = default)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dev = scope.ServiceProvider.GetRequiredService<IDevUserService>();
        try
        {
            await dev.EnsureSampleUsersAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Sample data is a convenience. Never let it stop the API from coming up.
            app.Services.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DevEndpoints))
                .LogWarning(ex, "Seeding sample dev users failed. Use POST /dev/seed to retry.");
        }
    }

    private static async Task<IResult> ShowDashboard(IDevUserService dev, CancellationToken cancellationToken)
    {
        var users = await dev.ListAsync(cancellationToken);
        return Results.Content(DevDashboardHtml.Render(users), "text/html; charset=utf-8");
    }

    private static async Task<IResult> CreateFromForm(HttpContext http, IDevUserService dev, CancellationToken cancellationToken)
    {
        var form = await http.Request.ReadFormAsync(cancellationToken);
        return await ExecuteAsync(
            async () =>
            {
                await dev.CreateAsync(ReadCommand(form), cancellationToken);
                return Results.Redirect("/dev");
            });
    }

    private static async Task<IResult> SeedFromForm(IDevUserService dev, CancellationToken cancellationToken)
    {
        await dev.EnsureSampleUsersAsync(cancellationToken);
        return Results.Redirect("/dev");
    }

    private static async Task<IResult> ListUsers(IDevUserService dev, CancellationToken cancellationToken)
    {
        return Results.Ok(await dev.ListAsync(cancellationToken));
    }

    private static Task<IResult> CreateUser(CreateDevUserRequest body, IDevUserService dev, CancellationToken cancellationToken)
    {
        return ExecuteAsync(async () =>
        {
            var created = await dev.CreateAsync(
                new CreateDevUserCommand(
                    body.FullName,
                    body.Email,
                    body.Country,
                    body.State,
                    body.City,
                    body.Gender,
                    body.DailyTargetSteps,
                    body.WalletBalance,
                    body.ProviderUserId),
                cancellationToken);
            return Results.Ok(created);
        });
    }

    private static async Task<IResult> SeedUsers(IDevUserService dev, CancellationToken cancellationToken)
    {
        var result = await dev.EnsureSampleUsersAsync(cancellationToken);
        return Results.Ok(new { result.UsersAdded, result.ActivitySeeded });
    }

    private static CreateDevUserCommand ReadCommand(IFormCollection form)
    {
        int? goal = int.TryParse(form["dailyTargetSteps"], out var parsedGoal) ? parsedGoal : null;
        long? points = long.TryParse(form["walletBalance"], out var parsedPoints) ? parsedPoints : null;
        return new CreateDevUserCommand(
            NullIfEmpty(form["fullName"]),
            NullIfEmpty(form["email"]),
            NullIfEmpty(form["country"]),
            NullIfEmpty(form["state"]),
            NullIfEmpty(form["city"]),
            NullIfEmpty(form["gender"]),
            goal,
            points,
            NullIfEmpty(form["providerUserId"]));
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static async Task<IResult> ExecuteAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (IdentityException ex)
        {
            return Results.Problem(
                title: ex.Message,
                statusCode: ex.StatusCode,
                extensions: new Dictionary<string, object?> { ["code"] = ex.Code });
        }
    }

    private sealed record CreateDevUserRequest(
        string? FullName,
        string? Email,
        string? Country,
        string? State,
        string? City,
        string? Gender,
        int? DailyTargetSteps,
        long? WalletBalance,
        string? ProviderUserId);
}
