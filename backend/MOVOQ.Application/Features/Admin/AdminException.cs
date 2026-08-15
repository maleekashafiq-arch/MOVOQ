using MOVOQ.Application.Abstractions;

namespace MOVOQ.Application.Features.Admin;

/// <summary>Back-office failure that maps to an HTTP problem, e.g. a signed-in user who is not an operator.</summary>
public sealed class AdminException(string code, string message, int statusCode = 400)
    : AppException(code, message, statusCode);
