namespace MOVOQ.Application.Features.Identity;

public sealed record ExternalIdentity(
    string ProviderUserId,
    string Email,
    bool EmailVerified,
    string? DisplayName);
