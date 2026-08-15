namespace MOVOQ.Application.Features.Admin;

/// <summary>Read-only reporting behind the dashboard, activity, points, and fraud screens.</summary>
public interface IAdminInsightsService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<AdminActivityReportDto> GetActivityAsync(int days, CancellationToken cancellationToken = default);

    Task<AdminPointsReportDto> GetPointsAsync(int months, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminFraudSignalDto>> GetFraudSignalsAsync(int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminAuditEntryDto>> GetAuditTrailAsync(int take, CancellationToken cancellationToken = default);
}
