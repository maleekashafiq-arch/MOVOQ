using MOVOQ.Application.Features.Identity;

namespace MOVOQ.Application.Features.Draws;

/// <summary>Draw browsing and ticket buying for the mobile app.</summary>
public interface ILuckyDrawService
{
    Task<LuckyDrawListDto> ListAsync(ExternalIdentity identity, CancellationToken cancellationToken = default);

    Task<BuyTicketResultDto> BuyTicketsAsync(
        ExternalIdentity identity,
        Guid drawId,
        BuyTicketCommand command,
        CancellationToken cancellationToken = default);
}
