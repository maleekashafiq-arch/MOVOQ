using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Features.Admin;
using MOVOQ.Shared;

namespace MOVOQ.Api.Hosting;

/// <summary>
/// Back-office API consumed by the React admin app. Authentication proves identity; the
/// <see cref="RequireAdmin"/> filter proves the caller is an operator with a high enough role.
/// </summary>
internal static class AdminEndpoints
{
    internal static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder v1)
    {
        var admin = v1.MapGroup("/admin").WithTags("admin").RequireAuthorization();

        // Sign-in resolves the operator itself, so it must not sit behind the operator filter.
        admin.MapPost("/session", SignIn)
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .WithName("AdminSignIn")
            .WithSummary("Exchange an authenticated identity for a back-office session and audit the login.");

        var read = admin.MapGroup(string.Empty).RequireAdmin(AdminRole.ReadOnly);

        read.MapGet("/dashboard", GetDashboard)
            .WithName("AdminDashboard")
            .WithSummary("Platform KPIs, growth trends, recent activity, and the daily leaderboard.");

        read.MapGet("/activity", GetActivity)
            .WithName("AdminActivity")
            .WithSummary("Step activity for the requested window plus today's sync distribution by hour.");

        read.MapGet("/points", GetPoints)
            .WithName("AdminPoints")
            .WithSummary("Points issued and spent over time, with the active reward rules.");

        read.MapGet("/fraud", GetFraud)
            .WithName("AdminFraudSignals")
            .WithSummary("Most recent server-detected fraud and risk signals.");

        read.MapGet("/audit", GetAudit)
            .WithName("AdminAuditTrail")
            .WithSummary("Most recent privileged back-office actions.");

        read.MapGet("/users", ListUsers)
            .WithName("AdminListUsers")
            .WithSummary("Paged user directory with search and status filtering.");

        read.MapGet("/users/{id:guid}", GetUser)
            .WithName("AdminGetUser")
            .WithSummary("One user with recent step history and ledger entries.");

        read.MapGet("/settings", GetSettings)
            .WithName("AdminGetSettings")
            .WithSummary("Operator-editable platform configuration.");

        var write = admin.MapGroup(string.Empty).RequireAdmin(AdminRole.Admin);

        write.MapPost("/users/{id:guid}/points", AdjustPoints)
            .WithName("AdminAdjustPoints")
            .WithSummary("Credit or deduct points, posting to the ledger and the audit trail.");

        write.MapPost("/users/{id:guid}/status", ChangeStatus)
            .WithName("AdminChangeUserStatus")
            .WithSummary("Change an account's standing, for example suspending a suspicious user.");

        write.MapPut("/settings", UpdateSettings)
            .WithName("AdminUpdateSettings")
            .WithSummary("Save platform configuration and audit the diff.");

        read.MapGet("/draws", ListDraws)
            .WithName("AdminListDraws")
            .WithSummary("Every lucky draw, with ticket counts and whether a winner has been picked.");

        read.MapGet("/draws/{id:guid}", GetDraw)
            .WithName("AdminGetDraw")
            .WithSummary("One draw with its entries and winner, if any.");

        read.MapGet("/tickets", GetTickets)
            .WithName("AdminTickets")
            .WithSummary("Ticket purchases across every draw.");

        read.MapGet("/prizes", GetPrizes)
            .WithName("AdminPrizes")
            .WithSummary("Winners and how far each prize has travelled toward delivery.");

        read.MapGet("/sponsors", GetSponsors)
            .WithName("AdminSponsors")
            .WithSummary("Brands placing offers in the marketplace.");

        read.MapGet("/rewards", GetRewards)
            .WithName("AdminRewards")
            .WithSummary("The reward catalogue users spend points on.");

        read.MapGet("/operators", GetOperators)
            .WithName("AdminOperators")
            .WithSummary("The allowlist of people who may open the back office.");

        write.MapPost("/draws", CreateDraw)
            .WithName("AdminCreateDraw")
            .WithSummary("Create a draft lucky draw.");

        write.MapPut("/draws/{id:guid}", UpdateDraw)
            .WithName("AdminUpdateDraw")
            .WithSummary("Edit a draw that has not yet been drawn or cancelled.");

        write.MapPost("/draws/{id:guid}/status", ChangeDrawStatus)
            .WithName("AdminChangeDrawStatus")
            .WithSummary("Activate, close, or cancel a draw.");

        write.MapPost("/draws/{id:guid}/winner", SelectWinner)
            .WithName("AdminSelectDrawWinner")
            .WithSummary("Pick one active ticket at random as the winner.");

        write.MapPost("/prizes/{id:guid}/fulfillment", UpdateFulfillment)
            .WithName("AdminUpdatePrizeFulfillment")
            .WithSummary("Move a prize from awaiting details through to delivered.");

        write.MapPost("/sponsors", CreateSponsor)
            .WithName("AdminCreateSponsor")
            .WithSummary("Add a brand to the marketplace.");

        write.MapPut("/sponsors/{id:guid}", UpdateSponsor)
            .WithName("AdminUpdateSponsor")
            .WithSummary("Edit a sponsor's details or pause them.");

        write.MapPost("/rewards", CreateReward)
            .WithName("AdminCreateReward")
            .WithSummary("Add a reward to the catalogue.");

        write.MapPut("/rewards/{id:guid}", UpdateReward)
            .WithName("AdminUpdateReward")
            .WithSummary("Edit a reward's cost, stock, or standing.");

        var super = admin.MapGroup(string.Empty).RequireAdmin(AdminRole.SuperAdmin);

        super.MapPost("/operators", CreateOperator)
            .WithName("AdminCreateOperator")
            .WithSummary("Allow a Firebase email to open the back office.");

        super.MapPut("/operators/{id:guid}", UpdateOperator)
            .WithName("AdminUpdateOperator")
            .WithSummary("Change an operator's role or standing.");

        return v1;
    }

    /// <summary>
    /// Resolves the caller against <c>admin_users</c> before the handler runs. Resolution is cached for the
    /// request, so handlers can ask for the same context again without a second query.
    /// </summary>
    private static RouteGroupBuilder RequireAdmin(this RouteGroupBuilder group, AdminRole minimumRole) =>
        group.AddEndpointFilter(async (context, next) =>
        {
            var http = context.HttpContext;
            var access = http.RequestServices.GetRequiredService<IAdminAccessService>();

            try
            {
                await access.RequireAsync(http.User.ReadExternalIdentity(), minimumRole, http.RequestAborted);
            }
            catch (AppException ex)
            {
                return ApiResults.Problem(ex);
            }

            return await next(context);
        });

    private static Task<IResult> SignIn(
        HttpContext http,
        IAdminAccessService access,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => access.SignInAsync(http.User.ReadExternalIdentity(), cancellationToken));

    private static Task<IResult> GetDashboard(IAdminInsightsService insights, CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => insights.GetDashboardAsync(cancellationToken));

    private static Task<IResult> GetActivity(
        IAdminInsightsService insights,
        CancellationToken cancellationToken,
        int days = 14) =>
        ApiResults.ExecuteAsync(() => insights.GetActivityAsync(days, cancellationToken));

    private static Task<IResult> GetPoints(
        IAdminInsightsService insights,
        CancellationToken cancellationToken,
        int months = 6) =>
        ApiResults.ExecuteAsync(() => insights.GetPointsAsync(months, cancellationToken));

    private static Task<IResult> GetFraud(
        IAdminInsightsService insights,
        CancellationToken cancellationToken,
        int take = 50) =>
        ApiResults.ExecuteAsync(() => insights.GetFraudSignalsAsync(take, cancellationToken));

    private static Task<IResult> GetAudit(
        IAdminInsightsService insights,
        CancellationToken cancellationToken,
        int take = 50) =>
        ApiResults.ExecuteAsync(() => insights.GetAuditTrailAsync(take, cancellationToken));

    private static Task<IResult> ListUsers(
        IAdminUsersService users,
        CancellationToken cancellationToken,
        string? query = null,
        UserStatus? status = null,
        int page = 1,
        int pageSize = 25) =>
        ApiResults.ExecuteAsync(() => users.ListAsync(query, status, page, pageSize, cancellationToken));

    private static Task<IResult> GetUser(
        Guid id,
        IAdminUsersService users,
        CancellationToken cancellationToken,
        int days = 7) =>
        ApiResults.ExecuteAsync(() => users.GetAsync(id, days, cancellationToken));

    private static Task<IResult> GetSettings(IPlatformSettingsService settings, CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => settings.GetAsync(cancellationToken));

    private static Task<IResult> AdjustPoints(
        HttpContext http,
        Guid id,
        AdjustPointsRequest body,
        IAdminAccessService access,
        IAdminUsersService users,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await users.AdjustPointsAsync(
                admin,
                id,
                new AdjustPointsCommand(body.Amount, body.Reason),
                cancellationToken);
        });

    private static Task<IResult> ChangeStatus(
        HttpContext http,
        Guid id,
        ChangeUserStatusRequest body,
        IAdminAccessService access,
        IAdminUsersService users,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await users.ChangeStatusAsync(
                admin,
                id,
                new ChangeUserStatusCommand(body.Status, body.Reason),
                cancellationToken);
        });

    private static Task<IResult> UpdateSettings(
        HttpContext http,
        PlatformSettingsDto body,
        IAdminAccessService access,
        IPlatformSettingsService settings,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await settings.UpdateAsync(admin, body, cancellationToken);
        });

    private static Task<IResult> ListDraws(IAdminDrawsService draws, CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => draws.ListAsync(cancellationToken));

    private static Task<IResult> GetDraw(Guid id, IAdminDrawsService draws, CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => draws.GetAsync(id, cancellationToken));

    private static Task<IResult> GetTickets(
        IAdminDrawsService draws,
        CancellationToken cancellationToken,
        int take = 50) =>
        ApiResults.ExecuteAsync(() => draws.GetTicketsAsync(take, cancellationToken));

    private static Task<IResult> GetPrizes(
        IAdminDrawsService draws,
        CancellationToken cancellationToken,
        int take = 50) =>
        ApiResults.ExecuteAsync(() => draws.GetPrizesAsync(take, cancellationToken));

    private static Task<IResult> GetSponsors(IAdminMarketplaceService marketplace, CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => marketplace.GetSponsorsAsync(cancellationToken));

    private static Task<IResult> GetRewards(IAdminMarketplaceService marketplace, CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => marketplace.GetRewardsAsync(cancellationToken));

    private static Task<IResult> GetOperators(IAdminOperatorsService operators, CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => operators.ListAsync(cancellationToken));

    private static Task<IResult> CreateDraw(
        HttpContext http,
        SaveDrawCommand body,
        IAdminAccessService access,
        IAdminDrawsService draws,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await draws.CreateAsync(admin, body, cancellationToken);
        });

    private static Task<IResult> UpdateDraw(
        HttpContext http,
        Guid id,
        SaveDrawCommand body,
        IAdminAccessService access,
        IAdminDrawsService draws,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await draws.UpdateAsync(admin, id, body, cancellationToken);
        });

    private static Task<IResult> ChangeDrawStatus(
        HttpContext http,
        Guid id,
        ChangeDrawStatusRequest body,
        IAdminAccessService access,
        IAdminDrawsService draws,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await draws.ChangeStatusAsync(admin, id, body.Status, cancellationToken);
        });

    private static Task<IResult> SelectWinner(
        HttpContext http,
        Guid id,
        IAdminAccessService access,
        IAdminDrawsService draws,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await draws.SelectWinnerAsync(admin, id, cancellationToken);
        });

    private static Task<IResult> UpdateFulfillment(
        HttpContext http,
        Guid id,
        UpdateFulfillmentRequest body,
        IAdminAccessService access,
        IAdminDrawsService draws,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await draws.UpdateFulfillmentAsync(admin, id, body.Status, body.Notes, cancellationToken);
        });

    private static Task<IResult> CreateSponsor(
        HttpContext http,
        SaveSponsorCommand body,
        IAdminAccessService access,
        IAdminMarketplaceService marketplace,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await marketplace.CreateSponsorAsync(admin, body, cancellationToken);
        });

    private static Task<IResult> UpdateSponsor(
        HttpContext http,
        Guid id,
        SaveSponsorCommand body,
        IAdminAccessService access,
        IAdminMarketplaceService marketplace,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await marketplace.UpdateSponsorAsync(admin, id, body, cancellationToken);
        });

    private static Task<IResult> CreateReward(
        HttpContext http,
        SaveRewardCommand body,
        IAdminAccessService access,
        IAdminMarketplaceService marketplace,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await marketplace.CreateRewardAsync(admin, body, cancellationToken);
        });

    private static Task<IResult> UpdateReward(
        HttpContext http,
        Guid id,
        SaveRewardCommand body,
        IAdminAccessService access,
        IAdminMarketplaceService marketplace,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.Admin, cancellationToken);
            return await marketplace.UpdateRewardAsync(admin, id, body, cancellationToken);
        });

    private static Task<IResult> CreateOperator(
        HttpContext http,
        SaveOperatorCommand body,
        IAdminAccessService access,
        IAdminOperatorsService operators,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.SuperAdmin, cancellationToken);
            return await operators.CreateAsync(admin, body, cancellationToken);
        });

    private static Task<IResult> UpdateOperator(
        HttpContext http,
        Guid id,
        SaveOperatorCommand body,
        IAdminAccessService access,
        IAdminOperatorsService operators,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(async () =>
        {
            var admin = await access.RequireAsync(http.User.ReadExternalIdentity(), AdminRole.SuperAdmin, cancellationToken);
            return await operators.UpdateAsync(admin, id, body, cancellationToken);
        });

    private sealed record AdjustPointsRequest(long Amount, string Reason);

    private sealed record ChangeUserStatusRequest(UserStatus Status, string? Reason);

    private sealed record ChangeDrawStatusRequest(LuckyDrawStatus Status);

    private sealed record UpdateFulfillmentRequest(PrizeFulfillmentStatus Status, string? Notes);
}
