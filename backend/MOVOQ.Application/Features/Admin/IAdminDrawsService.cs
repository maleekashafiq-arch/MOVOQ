using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

/// <summary>Back-office management of lucky draws, their entries, and the winner.</summary>
public interface IAdminDrawsService
{
    Task<IReadOnlyList<AdminDrawDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<AdminDrawDetailDto> GetAsync(Guid drawId, CancellationToken cancellationToken = default);

    Task<AdminDrawDto> CreateAsync(
        AdminContext admin,
        SaveDrawCommand command,
        CancellationToken cancellationToken = default);

    Task<AdminDrawDto> UpdateAsync(
        AdminContext admin,
        Guid drawId,
        SaveDrawCommand command,
        CancellationToken cancellationToken = default);

    Task<AdminDrawDto> ChangeStatusAsync(
        AdminContext admin,
        Guid drawId,
        LuckyDrawStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>Picks one active ticket at random and records it as the winner. Runs once per draw.</summary>
    Task<AdminDrawDetailDto> SelectWinnerAsync(
        AdminContext admin,
        Guid drawId,
        CancellationToken cancellationToken = default);

    Task<AdminTicketsReportDto> GetTicketsAsync(int take, CancellationToken cancellationToken = default);

    Task<AdminPrizesReportDto> GetPrizesAsync(int take, CancellationToken cancellationToken = default);

    Task<AdminDrawWinnerDto> UpdateFulfillmentAsync(
        AdminContext admin,
        Guid winnerId,
        PrizeFulfillmentStatus status,
        string? notes,
        CancellationToken cancellationToken = default);
}
