using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed record AdminSponsorDto(
    Guid Id,
    string Name,
    string? ContactPersonName,
    string? ContactEmail,
    string? ContactPhone,
    string? WebsiteUrl,
    bool IsActive,
    int Offers,
    int Redemptions);

public sealed record AdminSponsorsReportDto(
    int ActiveSponsors,
    int LiveOffers,
    int RedemptionsThisMonth,
    IReadOnlyList<AdminSponsorDto> Items);

public sealed record SaveSponsorCommand(
    string Name,
    string? ContactPersonName,
    string? ContactEmail,
    string? ContactPhone,
    string? WebsiteUrl,
    bool IsActive);

public sealed record AdminRewardDto(
    Guid Id,
    Guid SponsorId,
    string SponsorName,
    string Name,
    string? Description,
    long PointsCost,
    int? StockQuantity,
    RewardStatus Status,
    int Redeemed,
    DateTime? ValidFrom,
    DateTime? ValidUntil);

public sealed record AdminRewardsReportDto(
    int LiveRewards,
    int RedemptionsToday,
    long PointsRedeemedThisMonth,
    int OutOfStock,
    IReadOnlyList<AdminRewardDto> Items,
    IReadOnlyList<AdminSponsorOptionDto> Sponsors);

public sealed record AdminSponsorOptionDto(Guid Id, string Name, bool IsActive);

public sealed record SaveRewardCommand(
    Guid SponsorId,
    string Name,
    string? Description,
    long PointsCost,
    int? StockQuantity,
    RewardStatus Status,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? TermsAndConditions);
