namespace MOVOQ.Application.Features.Marketplace;

public sealed record MarketplaceRewardDto(
    Guid Id,
    string Name,
    string? Description,
    string SponsorName,
    long PointsCost,
    int? StockRemaining,
    DateTime? ValidUntil,
    string? TermsAndConditions);

public sealed record MyRedemptionDto(
    Guid Id,
    string RewardName,
    string SponsorName,
    long PointsCost,
    string Status,
    string? AssignedCode,
    DateTime RequestedAt);

public sealed record MarketplaceListDto(
    IReadOnlyList<MarketplaceRewardDto> Rewards,
    IReadOnlyList<MyRedemptionDto> MyRedemptions,
    long WalletBalance);

public sealed record RedeemCommand(Guid CommandId);

public sealed record RedeemResultDto(
    Guid RedemptionId,
    string RewardName,
    string? AssignedCode,
    long PointsSpent,
    long WalletBalance,
    string Status,
    bool Duplicate);
