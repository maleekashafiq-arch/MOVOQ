using MOVOQ.Application.Abstractions;

namespace MOVOQ.Api.Hosting;

internal static class ApiResults
{
    /// <summary>Runs a use case and turns expected application failures into RFC 7807 problems with a stable code.</summary>
    internal static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return Results.Ok(await action());
        }
        catch (AppException ex)
        {
            return Problem(ex);
        }
    }

    internal static IResult Problem(AppException ex) => Results.Problem(
        title: ex.Message,
        statusCode: ex.StatusCode,
        extensions: new Dictionary<string, object?> { ["code"] = ex.Code });
}
