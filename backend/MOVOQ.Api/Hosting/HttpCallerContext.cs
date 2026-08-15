using MOVOQ.Application.Abstractions;

namespace MOVOQ.Api.Hosting;

internal sealed class HttpCallerContext(IHttpContextAccessor accessor) : ICallerContext
{
    /// <summary>
    /// Behind a proxy the socket address is the proxy's, so prefer the forwarded client address when the
    /// deployment sets one. Only the first hop is trusted; the rest of the chain is caller-supplied.
    /// </summary>
    public string? IpAddress
    {
        get
        {
            var http = accessor.HttpContext;
            if (http is null)
            {
                return null;
            }

            var forwarded = http.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                return forwarded.Split(',')[0].Trim();
            }

            return http.Connection.RemoteIpAddress?.ToString();
        }
    }
}
