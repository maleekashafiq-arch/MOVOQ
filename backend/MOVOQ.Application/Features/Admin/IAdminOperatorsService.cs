namespace MOVOQ.Application.Features.Admin;

/// <summary>
/// The operator allowlist. Firebase proves identity; this list is what proves someone may open the back office.
/// </summary>
public interface IAdminOperatorsService
{
    Task<AdminOperatorsReportDto> ListAsync(CancellationToken cancellationToken = default);

    Task<AdminOperatorDto> CreateAsync(
        AdminContext admin,
        SaveOperatorCommand command,
        CancellationToken cancellationToken = default);

    Task<AdminOperatorDto> UpdateAsync(
        AdminContext admin,
        Guid operatorId,
        SaveOperatorCommand command,
        CancellationToken cancellationToken = default);
}
