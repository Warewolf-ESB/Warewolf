
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace Dev2.Runtime.WebServer
{
    public interface IWebServerStartup
    {

    }

    public class WebServerStartup : IWebServerStartup
    {
        public const double SizeCapForDownload = 51200; // 50 KB size limit

        public static IDisposable Start(Dev2Endpoint[] endpoints, WebApplicationBuilder builder)
        {
            builder.Services.AddControllers(options =>
            {
                options.OutputFormatters.Insert(0, new HttpResponseMessageOutputFormatter());
                //options.Filters.Add(new CustomActionFilter());
            }).AddApplicationPart(typeof(WebServerStartup).Assembly);



            #region Windows Authentication with UseWindowsAndAnonymousAuthenticationMiddleware
            // to use the UseWindowsAndAnonymousAuthenticationMiddleware uncomment below lines
            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
            #endregion

            builder.Services.AddSignalR(options =>
            {
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(180);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.MaximumReceiveMessageSize = null;
                options.StreamBufferCapacity = 1000;
                options.EnableDetailedErrors = true;
            }).AddHubOptions<Hubs.EsbHub>(huboptions =>
            {
                huboptions.AddFilter<CustomHubFilter>();
            });
            builder.Services.AddHttpContextAccessor();


            var app = builder.Build();

            //app.UseDeveloperExceptionPage();

            app.UseRouting();

            foreach (var endpoint in endpoints)
                app.Urls.Add(endpoint.Url);

            app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseWindowsAndAnonymousAuthentication();

            app.MapControllers();

            app.MapHub<Hubs.EsbHub>("/dsf");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            app.StartAsync().Wait();

            return app;
        }
    }
}

