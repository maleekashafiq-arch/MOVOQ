using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed class AdminBootstrapper(
    IAdminUserRepository admins,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAdminBootstrapper
{
    public async Task<int> EnsureOperatorsAsync(
        IEnumerable<string> emails,
        AdminRole role,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var added = 0;

        foreach (var raw in emails)
        {
            var email = raw?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
            {
                continue;
            }

            if (await admins.GetByEmailAsync(email, cancellationToken) is not null)
            {
                continue;
            }

            var at = email.IndexOf('@');
            admins.Add(new AdminUser
            {
                Email = email,
                DisplayName = at > 0 ? email[..at] : email,
                Role = role,
                Status = AdminStatus.Active,
                CreatedAt = now,
            });
            added++;
        }

        if (added > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return added;
    }
}
