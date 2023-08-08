using Dev2.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Dev2.Runtime.WebServer.Security
{
    public static class WindowsAndAnonymousAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseWindowsAndAnonymousAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WindowsAndAnonymousAuthenticationMiddleware>();
        }
    }

    public class WindowsAndAnonymousAuthenticationMiddleware
    {
        private readonly RequestDelegate next;

        public WindowsAndAnonymousAuthenticationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue && context.Request.Path.Value.EndsWith("/favicon.ico"))
            {
                context.Response.StatusCode = 200;
                return;
            }

            var isAuthenticated = context.User.Identity?.IsAuthenticated;
            
            if ((isAuthenticated ?? false) || context.User.IsAnonymous() || GetAuthenticationScheme(context) == AuthenticationSchemes.Anonymous)
            //if (GetAuthenticationScheme(context) == AuthenticationSchemes.Anonymous)
            {
                await next(context);
                return;
            }

            await context.ChallengeAsync(NegotiateDefaults.AuthenticationScheme);
        }

        public static AuthenticationSchemes GetAuthenticationScheme(HttpContext context)
        {
            var url = context.Request.Path.Value;

            var dnsName = context.Request.Host.Host;
            var port = context.Request.Host.Port;

            EnvironmentVariables.DnsName = dnsName;
            EnvironmentVariables.Port = port ?? 0;

            if (null == url) return AuthenticationSchemes.Ntlm | AuthenticationSchemes.Basic;

            if (url.StartsWith("/public/", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticationSchemes.Anonymous;
            }
            else if (url.StartsWith("/token/", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticationSchemes.Anonymous;
            }
            else if (url.StartsWith("/login/", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticationSchemes.Anonymous;
            }

            return AuthenticationSchemes.Ntlm | AuthenticationSchemes.Basic;
        }
    }
}
