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
            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = true, EnableJSONP = true };
            app.MapSignalR(hubConfiguration);

            // Add web server routing...
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);
        }
    }
}