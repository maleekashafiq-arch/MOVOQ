namespace MOVOQ.Application.Features.Admin;

public interface IAdminMarketplaceService
{
    Task<AdminSponsorsReportDto> GetSponsorsAsync(CancellationToken cancellationToken = default);

    Task<AdminSponsorDto> CreateSponsorAsync(
        AdminContext admin,
        SaveSponsorCommand command,
        CancellationToken cancellationToken = default);

    Task<AdminSponsorDto> UpdateSponsorAsync(
        AdminContext admin,
        Guid sponsorId,
        SaveSponsorCommand command,
        CancellationToken cancellationToken = default);

    Task<AdminRewardsReportDto> GetRewardsAsync(CancellationToken cancellationToken = default);

    Task<AdminRewardDto> CreateRewardAsync(
        AdminContext admin,
        SaveRewardCommand command,
        CancellationToken cancellationToken = default);

    Task<AdminRewardDto> UpdateRewardAsync(
        AdminContext admin,
        Guid rewardId,
        SaveRewardCommand command,
        CancellationToken cancellationToken = default);
}
