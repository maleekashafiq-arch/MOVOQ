using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Dev;

public interface IDevUserService
{
    Task<IReadOnlyList<DevUserDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<DevUserDto> CreateAsync(CreateDevUserCommand command, CancellationToken cancellationToken = default);

    Task<DevSeedResult> EnsureSampleUsersAsync(CancellationToken cancellationToken = default);
}

public sealed record DevSeedResult(int UsersAdded, int ActivitySeeded);

public sealed record DevUserDto(
    Guid Id,
    string Email,
    string? DisplayName,
    string? City,
    string? State,
    string? Country,
    string? Gender,
    long WalletBalance,
    int? DailyTargetSteps,
    bool ProfileComplete,
    string Status,
    DateTime? LastLoginAt,
    string ProviderUserId);

public sealed record CreateDevUserCommand(
    string? FullName,
    string? Email,
    string? Country,
    string? State,
    string? City,
    string? Gender,
    int? DailyTargetSteps,
    long? WalletBalance,
    string? ProviderUserId);

/// <summary>Development-only fake walkers for the admin dashboard and mobile home.</summary>
public sealed class DevUserService(
    IIdentityService identity,
    IUserRepository users,
    IDailyStepRecordRepository stepRecords,
    IPointsLedgerRepository ledger,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IDevUserService
{
    private const int TargetSampleCount = 50;
    private const int SimulationDays = 14;
    private const int SimulationMonths = 6;

    private static readonly (string Name, string Email, string City, string State, string Gender, int Goal, long Points, string Uid)[] Featured =
    [
        ("Ahmed Khan", "ahmed.khan@localhost", "Lahore", "Punjab", "Male", 6000, 1245, "fake-ahmed-khan"),
        ("Fatima Ali", "fatima.ali@localhost", "Karachi", "Sindh", "Female", 4000, 890, "fake-fatima-ali"),
        ("Usman Malik", "usman.malik@localhost", "Islamabad", "ICT", "Male", 10000, 2100, "fake-usman-malik"),
        ("Sara Raza", "sara.raza@localhost", "Lahore", "Punjab", "Female", 6000, 450, "fake-sara-raza"),
        ("Local Walker", "dev@localhost", "Lahore", "Punjab", "Other", 6000, 1245, "local-dev-user"),
    ];

    private static readonly string[] FirstNames =
    [
        "Hina", "Bilal", "Zara", "Omar", "Ayesha", "Hamza", "Nadia", "Ali", "Maryam", "Tariq",
        "Imran", "Sana", "Kamran", "Rabia", "Faisal", "Laiba", "Saad", "Mehwish", "Danish", "Anaya",
    ];

    private static readonly string[] LastNames =
    [
        "Mir", "Ahmed", "Khan", "Sheikh", "Noor", "Tariq", "Qureshi", "Raza", "Siddiqui", "Mehmood",
        "Hussain", "Malik", "Butt", "Iqbal", "Shah", "Akhtar", "Yousaf", "Hashmi", "Chaudhry", "Abbasi",
    ];

    private static readonly string[] Cities =
    [
        "Lahore", "Karachi", "Islamabad", "Rawalpindi", "Faisalabad", "Multan", "Peshawar", "Quetta",
    ];

    private static readonly string[] States = ["Punjab", "Sindh", "ICT", "KPK", "Balochistan"];

    private static readonly string[] Genders = ["Male", "Female", "Other"];

    private static readonly string[] RandomNames =
    [
        "Hina Mir", "Bilal Ahmed", "Zara Khan", "Omar Sheikh", "Ayesha Noor",
        "Hamza Tariq", "Nadia Qureshi", "Ali Raza", "Maryam Siddiqui", "Tariq Mehmood",
    ];

    public async Task<IReadOnlyList<DevUserDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await users.ListForDevAsync(cancellationToken);
        return rows.Select(Map).ToArray();
    }

    public async Task<DevUserDto> CreateAsync(CreateDevUserCommand command, CancellationToken cancellationToken = default)
    {
        var created = await CreateCoreAsync(Fill(command), cancellationToken);
        return created;
    }

    public async Task<DevSeedResult> EnsureSampleUsersAsync(CancellationToken cancellationToken = default)
    {
        var added = 0;
        foreach (var sample in BuildSampleSet())
        {
            if (await users.GetByEmailAsync(sample.Email, cancellationToken) is not null)
            {
                continue;
            }

            await CreateCoreAsync(
                new CreateDevUserCommand(
                    sample.Name,
                    sample.Email,
                    "PK",
                    sample.State,
                    sample.City,
                    sample.Gender,
                    sample.Goal,
                    sample.Points,
                    sample.Uid),
                cancellationToken);
            added++;
        }

        var simulated = await EnsureSimulationDataAsync(cancellationToken);
        return new DevSeedResult(added, simulated);
    }

    private static IEnumerable<(string Name, string Email, string City, string State, string Gender, int Goal, long Points, string Uid)> BuildSampleSet()
    {
        foreach (var sample in Featured)
        {
            yield return sample;
        }

        for (var index = 1; index <= TargetSampleCount - Featured.Length; index++)
        {
            var seed = StableHash($"walker-{index}");
            var name = $"{FirstNames[seed % FirstNames.Length]} {LastNames[(seed / 7) % LastNames.Length]}";
            var goal = DailyGoalOptions.Allowed[seed % DailyGoalOptions.Allowed.Length];
            var points = 180 + (seed % 8200);
            yield return (
                name,
                $"walker.{index:D3}@localhost",
                Cities[seed % Cities.Length],
                States[seed % States.Length],
                Genders[seed % Genders.Length],
                goal,
                points,
                $"fake-walker-{index:D3}");
        }
    }

    private async Task<int> EnsureSimulationDataAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);
        var from = today.AddDays(-(SimulationDays - 1));
        var seeded = 0;

        foreach (var user in await users.ListForDevAsync(cancellationToken))
        {
            if (!user.Email.EndsWith("@localhost", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var existing = await stepRecords.ListRangeAsync(user.Id, from, today, cancellationToken);
            if (existing.Count > 0)
            {
                continue;
            }

            await SeedUserSimulationAsync(user, now, today, cancellationToken);
            seeded++;
        }

        return seeded;
    }

    private async Task SeedUserSimulationAsync(
        User user,
        DateTime now,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var seed = StableHash(user.Email);
        var goal = user.StepGoals.FirstOrDefault(g => g.IsActive)?.DailyTargetSteps ?? DailyGoalOptions.Default;
        var balance = user.Wallet?.Balance ?? 180 + (seed % 8200);

        if (user.CreatedAt > now.AddDays(-170))
        {
            user.CreatedAt = now.AddDays(-(seed % 170 + 10));
        }

        user.LastLoginAt = now.AddDays(-(seed % 14));
        user.UpdatedAt = now;

        long lifetimeEarned = 0;
        long lifetimeSpent = 0;

        for (var offset = SimulationDays - 1; offset >= 0; offset--)
        {
            var date = today.AddDays(-offset);
            var steps = 1200 + (seed + offset * 7919) % 14_500;
            if (steps > StepRewardOptions.MaxPlausibleDailySteps)
            {
                steps = StepRewardOptions.MaxPlausibleDailySteps;
            }

            var completed = steps >= goal;
            var points = completed ? StepRewardOptions.GoalCompletionPoints : 0L;
            var syncHour = 6 + (seed + offset) % 16;
            var syncedAt = DateTime.SpecifyKind(
                date.ToDateTime(new TimeOnly(syncHour, (seed + offset * 3) % 60)),
                DateTimeKind.Utc);

            var record = new DailyStepRecord
            {
                UserId = user.Id,
                User = user,
                StepDate = date,
                ClaimedStepCount = steps,
                ValidatedStepCount = steps,
                DailyTargetStepsSnapshot = goal,
                TargetCompleted = completed,
                PointsEligible = completed,
                PointsAwarded = completed,
                PointsAmount = points,
                ValidatedAt = syncedAt,
                ValidationStatus = ValidationStatus.Validated,
                CommandId = Guid.NewGuid(),
                Source = DataSource.Mobile,
                SyncedAt = syncedAt,
                CreatedAt = syncedAt,
            };
            stepRecords.Add(record);

            if (points > 0 && offset <= 2)
            {
                ledger.Add(new PointsLedger
                {
                    UserId = user.Id,
                    User = user,
                    Amount = points,
                    TransactionType = PointsTransactionType.StepsEarned,
                    ReferenceType = LedgerReferenceType.DailyStepRecord,
                    Description = $"Daily goal on {date:yyyy-MM-dd}",
                    OccurredAt = syncedAt,
                    CreatedAt = syncedAt,
                });
            }
        }

        for (var monthOffset = SimulationMonths - 1; monthOffset >= 0; monthOffset--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 8, 0, 0, DateTimeKind.Utc).AddMonths(-monthOffset);
            var earned = 250 + (seed + monthOffset * 41) % 1400;
            var spent = monthOffset % 2 == 0 ? 80 + (seed + monthOffset) % 520 : 0;

            ledger.Add(new PointsLedger
            {
                UserId = user.Id,
                User = user,
                Amount = earned,
                TransactionType = PointsTransactionType.StepsEarned,
                ReferenceType = LedgerReferenceType.DailyStepRecord,
                Description = "Monthly step rewards",
                OccurredAt = monthStart.AddDays(4),
                CreatedAt = monthStart.AddDays(4),
            });
            lifetimeEarned += earned;

            if (spent <= 0)
            {
                continue;
            }

            ledger.Add(new PointsLedger
            {
                UserId = user.Id,
                User = user,
                Amount = -spent,
                TransactionType = monthOffset % 4 == 0
                    ? PointsTransactionType.RedemptionDeducted
                    : PointsTransactionType.TicketDeducted,
                ReferenceType = monthOffset % 4 == 0
                    ? LedgerReferenceType.RewardRedemption
                    : LedgerReferenceType.LuckyDrawTicket,
                Description = monthOffset % 4 == 0 ? "Reward redemption" : "Lucky draw ticket",
                OccurredAt = monthStart.AddDays(18),
                CreatedAt = monthStart.AddDays(18),
            });
            lifetimeSpent += spent;
        }

        if (user.Wallet is not null)
        {
            user.Wallet.Balance = balance;
            user.Wallet.LifetimeEarnedPoints = Math.Max(balance + lifetimeSpent, lifetimeEarned + balance / 2);
            user.Wallet.LifetimeSpentPoints = lifetimeSpent;
            user.Wallet.UpdatedAt = now;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<DevUserDto> CreateCoreAsync(CreateDevUserCommand command, CancellationToken cancellationToken)
    {
        var name = command.FullName!.Trim();
        var email = command.Email!.Trim().ToLowerInvariant();
        var uid = string.IsNullOrWhiteSpace(command.ProviderUserId)
            ? $"fake-{Guid.NewGuid():N}"
            : command.ProviderUserId.Trim();

        var external = new ExternalIdentity(uid, email, true, name);
        await identity.ExchangeSessionAsync(external, cancellationToken);
        await identity.CompleteProfileAsync(
            external,
            new CompleteProfileCommand(
                name,
                command.Country!,
                command.State!,
                command.City!,
                null,
                command.Gender,
                command.DailyTargetSteps ?? DailyGoalOptions.Default),
            cancellationToken);

        var user = await users.GetByEmailAsync(email, cancellationToken)
            ?? throw new InvalidOperationException("Fake user was not persisted.");

        if (user.Wallet is not null && command.WalletBalance is long points)
        {
            user.Wallet.Balance = points;
            user.Wallet.LifetimeEarnedPoints = points;
            user.Wallet.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Map(user) with { ProviderUserId = uid };
    }

    private static CreateDevUserCommand Fill(CreateDevUserCommand command)
    {
        var random = Random.Shared;
        var name = string.IsNullOrWhiteSpace(command.FullName)
            ? RandomNames[random.Next(RandomNames.Length)]
            : command.FullName.Trim();
        var email = string.IsNullOrWhiteSpace(command.Email)
            ? $"{Slug(name)}.{random.Next(100, 999)}@localhost"
            : command.Email.Trim();
        var city = string.IsNullOrWhiteSpace(command.City)
            ? Cities[random.Next(Cities.Length)]
            : command.City.Trim();
        var goal = command.DailyTargetSteps is int steps && DailyGoalOptions.IsAllowed(steps)
            ? steps
            : DailyGoalOptions.Allowed[random.Next(DailyGoalOptions.Allowed.Length)];
        var points = command.WalletBalance ?? random.Next(120, 2500);

        return command with
        {
            FullName = name,
            Email = email,
            Country = string.IsNullOrWhiteSpace(command.Country) ? "PK" : command.Country.Trim().ToUpperInvariant(),
            State = string.IsNullOrWhiteSpace(command.State) ? "Punjab" : command.State.Trim(),
            City = city,
            DailyTargetSteps = goal,
            WalletBalance = points,
        };
    }

    private static DevUserDto Map(User user)
    {
        var profile = user.Profile;
        var complete = profile is not null
            && !string.IsNullOrWhiteSpace(profile.DisplayName)
            && !string.IsNullOrWhiteSpace(profile.Country)
            && !string.IsNullOrWhiteSpace(profile.State)
            && !string.IsNullOrWhiteSpace(profile.City);
        var providerUserId = user.AuthProviders
            .OrderBy(x => x.LinkedAt)
            .Select(x => x.ProviderUserId)
            .FirstOrDefault() ?? user.Id.ToString();

        return new DevUserDto(
            user.Id,
            user.Email,
            profile?.DisplayName,
            profile?.City,
            profile?.State,
            profile?.Country,
            profile?.Gender,
            user.Wallet?.Balance ?? 0,
            user.StepGoals.SingleOrDefault(g => g.IsActive)?.DailyTargetSteps,
            complete,
            user.Status.ToString(),
            user.LastLoginAt,
            providerUserId);
    }

    private static int StableHash(string value) =>
        Math.Abs(value.GetHashCode(StringComparison.Ordinal));

    private static string Slug(string value)
    {
        var chars = value.ToLowerInvariant().Where(ch => char.IsLetterOrDigit(ch) || ch == ' ').ToArray();
        return new string(chars).Trim().Replace(' ', '.');
    }
}
