using System.Globalization;
using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed class PlatformSettingsService(
    IAppSettingRepository settings,
    IAuditTrail audit,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IPlatformSettingsService
{
    public static readonly PlatformSettingsDto Defaults = new(
        AppName: "MOVOQ",
        SupportEmail: "support@movoq.app",
        DailyGoalReward: StepRewardOptions.GoalCompletionPoints,
        AdReward: 10,
        StreakReward: 5,
        SpinMaxReward: 50,
        MaxPointsPerDay: 10_000,
        MaxTicketsPerDay: 10,
        MaxAdsPerDay: 20,
        TicketConversionRate: 100,
        TicketExpiryDays: 30,
        AdFrequency: 10,
        AdsOnHome: true,
        AdsOnMarketplace: true,
        CoachPrice: 299,
        CoachTrialDays: 7,
        FeatureNotifications: true,
        FeatureAds: true,
        FeatureLuckyDraws: true,
        FeatureTeams: true,
        FeatureAiCoach: true);

    private Dictionary<string, AppSetting>? _stored;

    public async Task<PlatformSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var stored = await LoadAsync(cancellationToken);
        return new PlatformSettingsDto(
            Text(stored, PlatformSettingKeys.AppName, Defaults.AppName),
            Text(stored, PlatformSettingKeys.SupportEmail, Defaults.SupportEmail),
            Number(stored, PlatformSettingKeys.DailyGoalReward, Defaults.DailyGoalReward),
            Number(stored, PlatformSettingKeys.AdReward, Defaults.AdReward),
            Number(stored, PlatformSettingKeys.StreakReward, Defaults.StreakReward),
            Number(stored, PlatformSettingKeys.SpinMaxReward, Defaults.SpinMaxReward),
            Number(stored, PlatformSettingKeys.MaxPointsPerDay, Defaults.MaxPointsPerDay),
            Number(stored, PlatformSettingKeys.MaxTicketsPerDay, Defaults.MaxTicketsPerDay),
            Number(stored, PlatformSettingKeys.MaxAdsPerDay, Defaults.MaxAdsPerDay),
            Number(stored, PlatformSettingKeys.TicketConversionRate, Defaults.TicketConversionRate),
            Number(stored, PlatformSettingKeys.TicketExpiryDays, Defaults.TicketExpiryDays),
            Number(stored, PlatformSettingKeys.AdFrequency, Defaults.AdFrequency),
            Flag(stored, PlatformSettingKeys.AdsOnHome, Defaults.AdsOnHome),
            Flag(stored, PlatformSettingKeys.AdsOnMarketplace, Defaults.AdsOnMarketplace),
            Number(stored, PlatformSettingKeys.CoachPrice, Defaults.CoachPrice),
            Number(stored, PlatformSettingKeys.CoachTrialDays, Defaults.CoachTrialDays),
            Flag(stored, PlatformSettingKeys.FeatureNotifications, Defaults.FeatureNotifications),
            Flag(stored, PlatformSettingKeys.FeatureAds, Defaults.FeatureAds),
            Flag(stored, PlatformSettingKeys.FeatureLuckyDraws, Defaults.FeatureLuckyDraws),
            Flag(stored, PlatformSettingKeys.FeatureTeams, Defaults.FeatureTeams),
            Flag(stored, PlatformSettingKeys.FeatureAiCoach, Defaults.FeatureAiCoach));
    }

    public async Task<int> GetIntAsync(string key, int fallback, CancellationToken cancellationToken = default)
    {
        var stored = await LoadAsync(cancellationToken);
        return Number(stored, key, fallback);
    }

    public async Task<PlatformSettingsDto> UpdateAsync(
        AdminContext admin,
        PlatformSettingsDto incoming,
        CancellationToken cancellationToken = default)
    {
        Validate(incoming);

        var before = await GetAsync(cancellationToken);
        var stored = await LoadAsync(cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var changed = new List<string>();

        // Compare against the effective values, not the stored rows: a key that has never been saved is
        // already serving its default, so re-saving that default would add a row and an audit entry for
        // a change nobody made.
        var effective = Flatten(before);
        foreach (var (key, value) in Flatten(incoming))
        {
            if (effective[key] == value)
            {
                continue;
            }

            Upsert(stored, key, value, admin.AdminUserId, now);
            changed.Add(key);
        }

        if (changed.Count == 0)
        {
            return before;
        }

        var after = await GetAsync(cancellationToken);
        audit.Record(
            admin,
            AuditActionType.SettingsUpdated,
            nameof(AppSetting),
            null,
            Describe(Flatten(before), changed),
            Describe(Flatten(after), changed));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return after;
    }

    private void Upsert(
        Dictionary<string, AppSetting> stored,
        string key,
        string value,
        Guid adminUserId,
        DateTime now)
    {
        if (stored.TryGetValue(key, out var existing))
        {
            existing.Value = value;
            existing.LastModifiedBy = adminUserId;
            existing.UpdatedAt = now;
            return;
        }

        var created = new AppSetting
        {
            Key = key,
            Value = value,
            LastModifiedBy = adminUserId,
            CreatedAt = now,
        };
        settings.Add(created);
        stored[key] = created;
    }

    private async Task<Dictionary<string, AppSetting>> LoadAsync(CancellationToken cancellationToken)
    {
        if (_stored is not null)
        {
            return _stored;
        }

        var rows = await settings.ListAsync(cancellationToken);
        return _stored = rows
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> Flatten(PlatformSettingsDto s) => new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [PlatformSettingKeys.AppName] = s.AppName,
        [PlatformSettingKeys.SupportEmail] = s.SupportEmail,
        [PlatformSettingKeys.DailyGoalReward] = Write(s.DailyGoalReward),
        [PlatformSettingKeys.AdReward] = Write(s.AdReward),
        [PlatformSettingKeys.StreakReward] = Write(s.StreakReward),
        [PlatformSettingKeys.SpinMaxReward] = Write(s.SpinMaxReward),
        [PlatformSettingKeys.MaxPointsPerDay] = Write(s.MaxPointsPerDay),
        [PlatformSettingKeys.MaxTicketsPerDay] = Write(s.MaxTicketsPerDay),
        [PlatformSettingKeys.MaxAdsPerDay] = Write(s.MaxAdsPerDay),
        [PlatformSettingKeys.TicketConversionRate] = Write(s.TicketConversionRate),
        [PlatformSettingKeys.TicketExpiryDays] = Write(s.TicketExpiryDays),
        [PlatformSettingKeys.AdFrequency] = Write(s.AdFrequency),
        [PlatformSettingKeys.AdsOnHome] = Write(s.AdsOnHome),
        [PlatformSettingKeys.AdsOnMarketplace] = Write(s.AdsOnMarketplace),
        [PlatformSettingKeys.CoachPrice] = Write(s.CoachPrice),
        [PlatformSettingKeys.CoachTrialDays] = Write(s.CoachTrialDays),
        [PlatformSettingKeys.FeatureNotifications] = Write(s.FeatureNotifications),
        [PlatformSettingKeys.FeatureAds] = Write(s.FeatureAds),
        [PlatformSettingKeys.FeatureLuckyDraws] = Write(s.FeatureLuckyDraws),
        [PlatformSettingKeys.FeatureTeams] = Write(s.FeatureTeams),
        [PlatformSettingKeys.FeatureAiCoach] = Write(s.FeatureAiCoach),
    };

    private static string Describe(IReadOnlyDictionary<string, string> values, IEnumerable<string> keys) =>
        string.Join("; ", keys.Order(StringComparer.Ordinal).Select(k => $"{k}={values[k]}"));

    private static void Validate(PlatformSettingsDto s)
    {
        if (string.IsNullOrWhiteSpace(s.AppName))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "App name is required.");
        }

        if (string.IsNullOrWhiteSpace(s.SupportEmail) || !s.SupportEmail.Contains('@'))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "A valid support email is required.");
        }

        RequireNonNegative(s.DailyGoalReward, "Daily goal reward");
        RequireNonNegative(s.AdReward, "Ad reward");
        RequireNonNegative(s.StreakReward, "Streak reward");
        RequireNonNegative(s.SpinMaxReward, "Spin maximum");
        RequireNonNegative(s.MaxPointsPerDay, "Max points per day");
        RequireNonNegative(s.MaxTicketsPerDay, "Max tickets per day");
        RequireNonNegative(s.MaxAdsPerDay, "Max ads per day");
        RequireNonNegative(s.AdFrequency, "Ad frequency");
        RequireNonNegative(s.CoachPrice, "AI Coach price");
        RequireNonNegative(s.CoachTrialDays, "AI Coach trial days");

        if (s.TicketConversionRate < 1)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Ticket conversion rate must be at least 1 point.");
        }

        if (s.TicketExpiryDays < 1)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Ticket expiry must be at least one day.");
        }
    }

    private static void RequireNonNegative(int value, string label)
    {
        if (value < 0)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, $"{label} cannot be negative.");
        }
    }

    private static string Text(IReadOnlyDictionary<string, AppSetting> stored, string key, string fallback) =>
        stored.TryGetValue(key, out var row) && !string.IsNullOrWhiteSpace(row.Value) ? row.Value : fallback;

    private static int Number(IReadOnlyDictionary<string, AppSetting> stored, string key, int fallback) =>
        stored.TryGetValue(key, out var row)
        && int.TryParse(row.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;

    private static bool Flag(IReadOnlyDictionary<string, AppSetting> stored, string key, bool fallback) =>
        stored.TryGetValue(key, out var row) && bool.TryParse(row.Value, out var parsed) ? parsed : fallback;

    private static string Write(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Write(bool value) => value ? "true" : "false";
}
