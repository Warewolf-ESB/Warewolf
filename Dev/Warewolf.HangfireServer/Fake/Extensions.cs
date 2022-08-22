using Owin;
using System;

namespace HangfireServer
{
    public static class Extensions
    {
        public static IAppBuilder UseHangfireDashboard(this IAppBuilder builder, string pathMatch, Hangfire.DashboardOptions options)
        {
            throw new NotImplementedException();
        }

        public static IAppBuilder UseHangfireServer(this IAppBuilder builder, Hangfire.BackgroundJobServerOptions options)
        {
            throw new NotImplementedException();

        }

    }
}
