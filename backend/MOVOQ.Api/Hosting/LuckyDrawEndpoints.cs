using MOVOQ.Application.Features.Draws;

namespace MOVOQ.Api.Hosting;

internal static class LuckyDrawEndpoints
{
    internal static IEndpointRouteBuilder MapLuckyDrawEndpoints(this IEndpointRouteBuilder v1)
    {
        var draws = v1.MapGroup("/lucky-draws").WithTags("lucky-draws").RequireAuthorization();

        draws.MapGet(string.Empty, ListDraws)
            .WithName("ListLuckyDraws")
            .WithSummary("Open draws with entry counts, plus the caller's tickets and wallet balance.");

        draws.MapPost("/{id:guid}/tickets", BuyTickets)
            .WithName("BuyLuckyDrawTickets")
            .WithSummary("Spend points on one or more tickets for a draw. Idempotent on CommandId.");

        return v1;
    }

    private static Task<IResult> ListDraws(
        HttpContext http,
        ILuckyDrawService draws,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => draws.ListAsync(http.User.ReadExternalIdentity(), cancellationToken));

    private static Task<IResult> BuyTickets(
        HttpContext http,
        Guid id,
        BuyTicketRequest body,
        ILuckyDrawService draws,
        CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => draws.BuyTicketsAsync(
            http.User.ReadExternalIdentity(),
            id,
            new BuyTicketCommand(body.CommandId, body.Quantity ?? 1),
            cancellationToken));

    private sealed record BuyTicketRequest(Guid CommandId, int? Quantity);
}
