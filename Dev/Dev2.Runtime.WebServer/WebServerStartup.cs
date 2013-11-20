using System;
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
            var hubConfiguration = new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJSONP = true
            };

            // Remove the default route URL which clients 
            // will use to connect to the Hub: "/signalr"
            //app.MapSignalR("/services", new HubConfiguration());
            app.MapSignalR(hubConfiguration);

            // Add web server routing...
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Services", "{website}/{controller}/{*path}", new { path = RouteParameter.Optional });
            config.Routes.MapHttpRoute("Views", "{website}/{controller}/{*path}", new { path = RouteParameter.Optional });

            app.UseWebApi(config);
        }
    }
}