using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>End-user account (mobile app). Server-controlled; not sync-enabled.</summary>
public sealed class User : BaseEntity
{
    public required string Email { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? PhoneVerifiedAt { get; set; }

    /// <summary>Opaque reference to stored credentials (never a plain password).</summary>
    public string? PasswordHashReference { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAt { get; set; }

    public UserProfile? Profile { get; set; }

    public UserWallet? Wallet { get; set; }

    public ICollection<UserAuthProvider> AuthProviders { get; } = new List<UserAuthProvider>();

    public ICollection<UserDevice> Devices { get; } = new List<UserDevice>();

    public ICollection<DailyStepRecord> DailyStepRecords { get; } = new List<DailyStepRecord>();

    public ICollection<StepGoal> StepGoals { get; } = new List<StepGoal>();

    public ICollection<PointsLedger> PointsLedgers { get; } = new List<PointsLedger>();

    public ICollection<AdsWatched> AdsWatched { get; } = new List<AdsWatched>();

    public ICollection<RewardRedemption> RewardRedemptions { get; } = new List<RewardRedemption>();

    public ICollection<LuckyDrawTicket> LuckyDrawTickets { get; } = new List<LuckyDrawTicket>();

    public ICollection<Notification> Notifications { get; } = new List<Notification>();

    public ICollection<FraudSignal> FraudSignals { get; } = new List<FraudSignal>();

    public ICollection<ProcessedCommand> ProcessedCommands { get; } = new List<ProcessedCommand>();
}
