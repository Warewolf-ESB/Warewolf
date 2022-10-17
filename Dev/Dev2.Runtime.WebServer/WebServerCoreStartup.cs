
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Dev2.Runtime.WebServer
{
    public interface IWebServerStartup
    {

    }

    public class WebServerStartup : IWebServerStartup
    {
        public const double SizeCapForDownload = 51200; // 50 KB size limit

        public static IDisposable Start(Dev2Endpoint[] endpoints)
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new CustomActionFilter());
            });

            builder.Services.Configure<AuthorizationOptions>(options =>
            {
                options.AddPolicy("AuthorizeHub", policy => policy.Requirements.Add(new AuthorizeHubRequirement()));
            });

            #region Windows Authentication with UseWindowsAndAnonymousAuthenticationMiddleware
            // to use the UseWindowsAndAnonymousAuthenticationMiddleware uncomment below lines
            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
            #endregion

            builder.Services.AddSignalR(options =>
            {
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(180);
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.HandshakeTimeout = TimeSpan.FromSeconds(10);
                options.MaximumReceiveMessageSize = null;
                options.StreamBufferCapacity = 1000;
                options.EnableDetailedErrors = true;
            });


            var app = builder.Build();

            app.UseRouting();


            foreach (var endpoint in endpoints)
            {
                app.Urls.Add(endpoint.Url);
            }

            app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true) // allow any origin
               .AllowCredentials()); // allow credentials


            app.UseAuthentication();

            app.UseAuthorization();

            #region Windows Authentication with UseWindowsAndAnonymousAuthenticationMiddleware
            app.UseWindowsAndAnonymousAuthentication();
            #endregion

            app.MapControllers();
            app.MapHub<Hubs.EsbHub>("/dsf");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            
            app.StartAsync().Wait();

            return app;
        }
    }
}

