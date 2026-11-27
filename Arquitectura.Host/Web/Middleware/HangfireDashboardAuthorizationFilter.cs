using Hangfire.Dashboard;
using Microsoft.Extensions.Hosting;

namespace Arquitectura.Host.Web.Middleware;

/// <summary>
/// Restricts the Hangfire dashboard to the Development environment and to authenticated local
/// requests. Production deployments must provide a real authorization filter.
/// </summary>
/// <param name="environment">The hosting environment.</param>
public sealed class HangfireDashboardAuthorizationFilter(IHostEnvironment environment) : IDashboardAuthorizationFilter
{
    /// <inheritdoc />
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var remoteIp = httpContext.Connection.RemoteIpAddress;
        var isLocal = remoteIp is not null && System.Net.IPAddress.IsLoopback(remoteIp);

        return environment.IsDevelopment()
               && (httpContext.User.Identity?.IsAuthenticated == true || isLocal);
    }
}
