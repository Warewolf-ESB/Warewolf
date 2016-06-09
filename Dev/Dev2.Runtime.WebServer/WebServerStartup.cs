
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;

namespace Dev2.Runtime.WebServer
{
    public class WebServerStartup
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
    }
}
