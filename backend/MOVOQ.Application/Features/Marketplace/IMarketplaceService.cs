using MOVOQ.Application.Features.Identity;

namespace MOVOQ.Application.Features.Marketplace;

public interface IMarketplaceService
{
    Task<MarketplaceListDto> ListAsync(ExternalIdentity identity, CancellationToken cancellationToken = default);

    Task<RedeemResultDto> RedeemAsync(
        ExternalIdentity identity,
        Guid rewardId,
        RedeemCommand command,
        CancellationToken cancellationToken = default);
}
