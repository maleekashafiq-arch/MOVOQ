namespace MOVOQ.Application.Abstractions;

/// <summary>
/// Transport details the application layer needs but must not reach into ASP.NET for. Today this is just
/// the caller address, which the audit trail is required to record for privileged actions.
/// </summary>
public interface ICallerContext
{
    string? IpAddress { get; }
}
