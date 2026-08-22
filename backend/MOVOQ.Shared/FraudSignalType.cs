namespace MOVOQ.Shared;

/// <summary>Reason a fraud signal was raised.</summary>
public enum FraudSignalType
{
    DuplicateCommand = 1,
    ExceededDailyLimit = 2,
    InvalidAdSession = 3,
    TamperedTimestamp = 4,
    SuspiciousDevicePattern = 5,
    MultipleAccountsSameDevice = 6,
    AbnormalStepCount = 7,
    ImpossibleStepRate = 8
}
