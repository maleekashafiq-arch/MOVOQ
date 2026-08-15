using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed record AdminOperatorDto(
    Guid Id,
    string Email,
    string? DisplayName,
    AdminRole Role,
    AdminStatus Status,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public sealed record AdminOperatorsReportDto(
    int OperatorCount,
    int SuperAdminCount,
    int SignedInToday,
    IReadOnlyList<AdminOperatorDto> Items);

public sealed record SaveOperatorCommand(
    string Email,
    string? DisplayName,
    AdminRole Role,
    AdminStatus Status);
