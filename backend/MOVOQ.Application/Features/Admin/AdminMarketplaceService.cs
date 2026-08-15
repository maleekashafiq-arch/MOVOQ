using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

/// <summary>Sponsors and the reward catalogue they place in the marketplace.</summary>
public sealed class AdminMarketplaceService(
    ISponsorRepository sponsors,
    IRewardRepository rewards,
    IRewardRedemptionRepository redemptions,
    IAuditTrail audit,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAdminMarketplaceService
{
    public async Task<AdminSponsorsReportDto> GetSponsorsAsync(CancellationToken cancellationToken = default)
    {
        var items = await sponsors.ListAsync(cancellationToken);
        var catalogue = await rewards.ListAsync(cancellationToken);
        var counts = (await redemptions.CountBySponsorAsync(items.Select(s => s.Id).ToList(), cancellationToken))
            .ToDictionary(x => x.Id, x => x.Count);

        var offersBySponsor = catalogue
            .GroupBy(r => r.SponsorId)
            .ToDictionary(g => g.Key, g => g.Count());

        var monthStart = new DateTime(
            timeProvider.GetUtcNow().UtcDateTime.Year,
            timeProvider.GetUtcNow().UtcDateTime.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        return new AdminSponsorsReportDto(
            items.Count(s => s.IsActive),
            catalogue.Count(r => r.Status == RewardStatus.Active),
            await redemptions.CountSinceAsync(monthStart, cancellationToken),
            items
                .Select(s => MapSponsor(
                    s,
                    offersBySponsor.GetValueOrDefault(s.Id),
                    counts.GetValueOrDefault(s.Id)))
                .ToList());
    }

    public async Task<AdminSponsorDto> CreateSponsorAsync(
        AdminContext admin,
        SaveSponsorCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateSponsor(command);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var sponsor = new Sponsor
        {
            Name = command.Name.Trim(),
            ContactPersonName = Clean(command.ContactPersonName),
            ContactEmail = Clean(command.ContactEmail),
            ContactPhone = Clean(command.ContactPhone),
            WebsiteUrl = Clean(command.WebsiteUrl),
            IsActive = command.IsActive,
            CreatedAt = now,
        };

        sponsors.Add(sponsor);
        audit.Record(admin, AuditActionType.SponsorModified, nameof(Sponsor), sponsor.Id, null, $"Created {sponsor.Name}");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapSponsor(sponsor, 0, 0);
    }

    public async Task<AdminSponsorDto> UpdateSponsorAsync(
        AdminContext admin,
        Guid sponsorId,
        SaveSponsorCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateSponsor(command);
        var sponsor = await sponsors.GetAsync(sponsorId, cancellationToken)
            ?? throw new AdminException(ErrorCodes.NotFound, $"No sponsor with id {sponsorId}.", 404);

        var before = $"{sponsor.Name}, {(sponsor.IsActive ? "active" : "paused")}";
        sponsor.Name = command.Name.Trim();
        sponsor.ContactPersonName = Clean(command.ContactPersonName);
        sponsor.ContactEmail = Clean(command.ContactEmail);
        sponsor.ContactPhone = Clean(command.ContactPhone);
        sponsor.WebsiteUrl = Clean(command.WebsiteUrl);
        sponsor.IsActive = command.IsActive;
        sponsor.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;

        audit.Record(
            admin,
            AuditActionType.SponsorModified,
            nameof(Sponsor),
            sponsor.Id,
            before,
            $"{sponsor.Name}, {(sponsor.IsActive ? "active" : "paused")}");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var catalogue = await rewards.ListAsync(cancellationToken);
        var offers = catalogue.Count(r => r.SponsorId == sponsor.Id);
        var counts = await redemptions.CountBySponsorAsync([sponsor.Id], cancellationToken);
        return MapSponsor(sponsor, offers, counts.FirstOrDefault()?.Count ?? 0);
    }

    public async Task<AdminRewardsReportDto> GetRewardsAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var items = await rewards.ListAsync(cancellationToken);
        var counts = (await redemptions.CountByRewardAsync(items.Select(r => r.Id).ToList(), cancellationToken))
            .ToDictionary(x => x.Id, x => x.Count);
        var sponsorOptions = (await sponsors.ListAsync(cancellationToken))
            .Select(s => new AdminSponsorOptionDto(s.Id, s.Name, s.IsActive))
            .ToList();

        return new AdminRewardsReportDto(
            items.Count(r => r.Status == RewardStatus.Active),
            await redemptions.CountSinceAsync(today, cancellationToken),
            await redemptions.PointsRedeemedSinceAsync(monthStart, cancellationToken),
            items.Count(r => r.Status == RewardStatus.OutOfStock || r.StockQuantity == 0),
            items.Select(r => MapReward(r, counts.GetValueOrDefault(r.Id))).ToList(),
            sponsorOptions);
    }

    public async Task<AdminRewardDto> CreateRewardAsync(
        AdminContext admin,
        SaveRewardCommand command,
        CancellationToken cancellationToken = default)
    {
        var sponsor = await RequireSponsorAsync(command, cancellationToken);
        var reward = BuildReward(command, sponsor);
        rewards.Add(reward);
        audit.Record(admin, AuditActionType.RewardModified, nameof(Reward), reward.Id, null, $"Created {reward.Name}");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapReward(reward, 0);
    }

    public async Task<AdminRewardDto> UpdateRewardAsync(
        AdminContext admin,
        Guid rewardId,
        SaveRewardCommand command,
        CancellationToken cancellationToken = default)
    {
        var reward = await rewards.GetAsync(rewardId, cancellationToken)
            ?? throw new AdminException(ErrorCodes.NotFound, $"No reward with id {rewardId}.", 404);
        var sponsor = await RequireSponsorAsync(command, cancellationToken);

        var before = $"{reward.Name}, {reward.PointsCost} pts, {reward.Status}";
        ApplyReward(reward, command, sponsor);
        reward.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;

        audit.Record(
            admin,
            AuditActionType.RewardModified,
            nameof(Reward),
            reward.Id,
            before,
            $"{reward.Name}, {reward.PointsCost} pts, {reward.Status}");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var counts = await redemptions.CountByRewardAsync([reward.Id], cancellationToken);
        return MapReward(reward, counts.FirstOrDefault()?.Count ?? 0);
    }

    private async Task<Sponsor> RequireSponsorAsync(SaveRewardCommand command, CancellationToken cancellationToken)
    {
        ValidateReward(command);
        return await sponsors.GetAsync(command.SponsorId, cancellationToken)
            ?? throw new AdminException(ErrorCodes.NotFound, $"No sponsor with id {command.SponsorId}.", 404);
    }

    private Reward BuildReward(SaveRewardCommand command, Sponsor sponsor)
    {
        var reward = new Reward
        {
            SponsorId = sponsor.Id,
            Sponsor = sponsor,
            Name = command.Name.Trim(),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
        };
        ApplyReward(reward, command, sponsor);
        return reward;
    }

    private static void ApplyReward(Reward reward, SaveRewardCommand command, Sponsor sponsor)
    {
        reward.SponsorId = sponsor.Id;
        reward.Sponsor = sponsor;
        reward.Name = command.Name.Trim();
        reward.Description = Clean(command.Description);
        reward.PointsCost = command.PointsCost;
        reward.StockQuantity = command.StockQuantity;
        reward.Status = command.StockQuantity == 0 ? RewardStatus.OutOfStock : command.Status;
        reward.ValidFrom = command.ValidFrom;
        reward.ValidUntil = command.ValidUntil;
        reward.TermsAndConditions = Clean(command.TermsAndConditions);
    }

    private static void ValidateSponsor(SaveSponsorCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "A sponsor needs a name.");
        }
    }

    private static void ValidateReward(SaveRewardCommand command)
    {
        if (command.SponsorId == Guid.Empty)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Pick the sponsor this reward belongs to.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "A reward needs a name.");
        }

        if (command.PointsCost < 1)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "A reward must cost at least one point.");
        }

        if (command.StockQuantity is int stock && stock < 0)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Stock cannot be negative.");
        }

        if (!Enum.IsDefined(command.Status))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Unknown reward status.");
        }
    }

    private static AdminSponsorDto MapSponsor(Sponsor sponsor, int offers, int redemptionCount) => new(
        sponsor.Id,
        sponsor.Name,
        sponsor.ContactPersonName,
        sponsor.ContactEmail,
        sponsor.ContactPhone,
        sponsor.WebsiteUrl,
        sponsor.IsActive,
        offers,
        redemptionCount);

    private static AdminRewardDto MapReward(Reward reward, int redeemed) => new(
        reward.Id,
        reward.SponsorId,
        reward.Sponsor.Name,
        reward.Name,
        reward.Description,
        reward.PointsCost,
        reward.StockQuantity,
        reward.Status,
        redeemed,
        reward.ValidFrom,
        reward.ValidUntil);

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
