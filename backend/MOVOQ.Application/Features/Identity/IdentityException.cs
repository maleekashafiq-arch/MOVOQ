using MOVOQ.Application.Abstractions;

namespace MOVOQ.Application.Features.Identity;

public sealed class IdentityException(string code, string message, int statusCode = 400)
    : AppException(code, message, statusCode);
