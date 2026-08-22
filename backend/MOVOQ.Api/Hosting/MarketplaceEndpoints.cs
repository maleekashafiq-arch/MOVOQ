using MOVOQ.Application.Features.Marketplace;

namespace MOVOQ.Api.Hosting;

internal static class MarketplaceEndpoints
{
    internal static IEndpointRouteBuilder MapMarketplaceEndpoints(this IEndpointRouteBuilder v1)
    {
        var rewards = v1.MapGroup("/rewards").WithTags("rewards").RequireAuthorization();

        rewards.MapGet(string.Empty, ListRewards)
            .WithName("ListRewards")
            .WithSummary("Active marketplace rewards plus the caller's redemptions and wallet.");

        rewards.MapPost("/{id:guid}/redeem", Redeem)
            .RequireRateLimiting(RateLimitPolicies.Spend)
            .WithName("RedeemReward")
            .WithSummary("Spend points on a reward. Idempotent on CommandId.");

        return v1;
    }

    private static Task<IResult> ListRewards(
        HttpContext http,
        IMarketplaceService marketplace,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => marketplace.ListAsync(http.User.ReadExternalIdentity(), cancellationToken));

    private static Task<IResult> Redeem(
        HttpContext http,
        Guid id,
        RedeemRequest body,
        IMarketplaceService marketplace,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => marketplace.RedeemAsync(
            http.User.ReadExternalIdentity(),
            id,
            new RedeemCommand(body.CommandId),
            cancellationToken));

    private sealed record RedeemRequest(Guid CommandId);
}
