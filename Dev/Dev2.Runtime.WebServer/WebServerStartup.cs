using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace Dev2.Runtime.WebServer
{
    public class WebServerStartup
    {
        public const double SizeCapForDownload = 51200; // 50 KB size limit

        public static IDisposable Start(Dev2Endpoint[] endpoints)
        {
            var startOptions = new StartOptions();
            foreach(var endpoint in endpoints)
            {
                startOptions.Urls.Add(endpoint.Url);
            }
            return WebApp.Start<WebServerStartup>(startOptions);
        }

        public static IDisposable Start(string url)
        {
            return WebApp.Start<WebServerStartup>(url);
        }

        public void Configuration(IAppBuilder app)
        {
            // Enable NTLM auth
            var listener = (HttpListener)app.Properties[typeof(HttpListener).FullName];
            listener.AuthenticationSchemes = AuthenticationSchemes.Ntlm;
           
            // Enable cross-domain calls
            app.UseCors(CorsOptions.AllowAll);

            //
            // Sequence is important!
            // ALWAYS MapSignalR first then UseWebApi
            //

            // Add SignalR routing...
            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = true, EnableJSONP = true };
            app.MapSignalR(hubConfiguration);
            GlobalHost.HubPipeline.RequireAuthentication();

            // Add web server routing...
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();
            app.UseWebApi(config);
        }

    }
}