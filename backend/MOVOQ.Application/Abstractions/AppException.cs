namespace MOVOQ.Application.Abstractions;

/// <summary>
/// An expected failure that maps straight to an HTTP problem response, carrying a stable code clients can branch on.
/// </summary>
public class AppException(string code, string message, int statusCode = 400) : Exception(message)
{
    public string Code { get; } = code;

    public int StatusCode { get; } = statusCode;
}
