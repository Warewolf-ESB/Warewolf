#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Web.Http;
using Dev2.Common;
using Dev2.Runtime.WebServer.Hubs;
using Microsoft.AspNet.SignalR;
#if !NETFRAMEWORK
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
#endif
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;


namespace Dev2.Runtime.WebServer
{
    public interface IWebServerStartup
    {

    }
    public class WebServerStartup : IWebServerStartup
    {
        public const double SizeCapForDownload = 51200; // 50 KB size limit

#if NETFRAMEWORK
        public static IDisposable Start(Dev2Endpoint[] endpoints)
        {
            // Make long polling connections wait a maximum of 110 seconds for a
            // response. When that time expires, trigger a timeout command and
            // make the client reconnect.
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(180);

            // Wait a maximum of 30 seconds after a transport connection is lost
            // before raising the Disconnected event to terminate the SignalR connection.
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30);

            // For transports other than long polling, send a keepalive packet every
            // 10 seconds. 
            // This value must be no more than 1/3 of the DisconnectTimeout value.
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);

            GlobalHost.Configuration.DefaultMessageBufferSize = 1000;
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
            GlobalHost.Configuration.TransportConnectTimeout = TimeSpan.FromSeconds(10);

            var startOptions = new StartOptions();

            foreach (var endpoint in endpoints)
            {
                startOptions.Urls.Add(endpoint.Url);
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            return WebApp.Start<WebServerStartup>(startOptions);
        }


        public void Configuration(IAppBuilder app)
        {
            var listener = (HttpListener)app.Properties[typeof(HttpListener).FullName];
            listener.AuthenticationSchemeSelectorDelegate += AuthenticationSchemeSelectorDelegate;
            listener.IgnoreWriteExceptions = true;  // ignore errors written to disconnected clients.
            // Enable cross-domain calls
            app.UseCors(CorsOptions.AllowAll);

            //
            // Sequence is important!
            // ALWAYS MapSignalR first then UseWebApi
            //

            // Add SignalR routing...
            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = true, EnableJSONP = true };
            app.MapSignalR("/dsf", hubConfiguration);

            // Add web server routing...
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();
            app.UseWebApi(config);
        }

        AuthenticationSchemes AuthenticationSchemeSelectorDelegate(HttpListenerRequest httpRequest)
        {
            EnvironmentVariables.DnsName = httpRequest.Url.DnsSafeHost;
            EnvironmentVariables.Port = httpRequest.Url.Port;
            if (httpRequest.RawUrl.StartsWith("/public/", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticationSchemes.Anonymous;
            }
            if (httpRequest.RawUrl.StartsWith("/token/", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticationSchemes.Anonymous;
            }
            if (httpRequest.RawUrl.StartsWith("/login", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticationSchemes.Anonymous;
            }
            //DO NOT USE NEGOTIATE BREAKS SERVER to SERVER coms when using public authentication and hostname.
            return AuthenticationSchemes.Ntlm | AuthenticationSchemes.Basic;
        }
#else
        public static IWebHost Start(Dev2Endpoint[] endpoints)
        {
            var webHostBuilder = WebHost.CreateDefaultBuilder()
                            .UseStartup<WebServerStartup>();
            foreach (var endpoint in endpoints)
            {
                webHostBuilder.UseUrls(endpoint.Url);
            }
            var webHost = webHostBuilder.Build();
            return webHost;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Enable cross-domain calls
            app.UseCors((x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()));

            //
            // Sequence is important!
            // ALWAYS MapSignalR first then UseWebApi
            //

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Add SignalR routing...
                endpoints.MapHub<EsbHub>("/dsf");
                // Add web server routing...
                endpoints.MapControllers();
            });
        }
#endif
    }
}
