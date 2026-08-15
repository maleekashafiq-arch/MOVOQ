using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public interface IAdminUsersService
{
    Task<AdminUserListDto> ListAsync(
        string? query,
        UserStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminUserDetailDto> GetAsync(Guid userId, int historyDays, CancellationToken cancellationToken = default);

    Task<AdjustPointsResultDto> AdjustPointsAsync(
        AdminContext admin,
        Guid userId,
        AdjustPointsCommand command,
        CancellationToken cancellationToken = default);

    Task<AdminUserDto> ChangeStatusAsync(
        AdminContext admin,
        Guid userId,
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken = default);
}
