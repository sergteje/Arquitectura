using Hangfire.Dashboard;

namespace Arquitectura.Infrastructure.Middleware;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow all in dev mode - in production add proper auth
        return true;
    }
}