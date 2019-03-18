#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Microsoft.AspNet.SignalR;
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
            
            foreach(var endpoint in endpoints)
            {
                startOptions.Urls.Add(endpoint.Url);
            }
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
            //DO NOT USE NEGOTIATE BREAKS SERVER to SERVER coms when using public authentication and hostname.
            return AuthenticationSchemes.Ntlm | AuthenticationSchemes.Basic;
        }
    }
}
