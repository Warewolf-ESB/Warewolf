#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;

namespace Dev2
{
    public interface IWebServerConfiguration
    {
        Dev2Endpoint[] EndPoints { get; }
        bool IsWebServerEnabled { get; }
        bool IsWebServerSslEnabled { get; }
        void Execute();
    }

    public class WebServerConfiguration : IWebServerConfiguration
    {
        bool _isWebServerEnabled;
        bool _isWebServerSslEnabled;
        readonly IWriter _writer;
        readonly IFile _fileWrapper;
        public Dev2Endpoint[] EndPoints { get; private set; }

        public bool IsWebServerEnabled
        {
            get => _isWebServerEnabled;
            private set => _isWebServerEnabled = value;
        }

        public bool IsWebServerSslEnabled
        {
            get => _isWebServerSslEnabled;
            private set => _isWebServerSslEnabled = value;
        }

        public WebServerConfiguration(IWriter writer, IFile fileWrapper)
        {
            _writer = writer;
            _fileWrapper = fileWrapper;
        }
        public void Execute()
        {
            try
            {
                string webServerSslPort;
                string webServerPort;
                GlobalConstants.CollectUsageStats = ConfigurationManager.AppSettings["CollectUsageStats"];
                GlobalConstants.WebServerPort = webServerPort = ConfigurationManager.AppSettings["webServerPort"];
                GlobalConstants.WebServerSslPort = webServerSslPort = ConfigurationManager.AppSettings["webServerSslPort"];

                bool.TryParse(ConfigurationManager.AppSettings["webServerEnabled"], out _isWebServerEnabled);
                bool.TryParse(ConfigurationManager.AppSettings["webServerSslEnabled"], out _isWebServerSslEnabled);

                if (_isWebServerEnabled)
                {
                    if (string.IsNullOrEmpty(webServerPort) && _isWebServerEnabled)
                    {
                        throw new ArgumentException("Web server port not set but web server is enabled. Please set the webServerPort value in the configuration file.");
                    }


                    if (!int.TryParse(webServerPort, out int realPort))
                    {
                        throw new ArgumentException("Web server port is not valid. Please set the webServerPort value in the configuration file.");
                    }

                    var endpoints = new List<Dev2Endpoint>();

                    var httpEndpoint = new IPEndPoint(IPAddress.Any, realPort);
                    var httpUrl = $"http://*:{webServerPort}/";
                    endpoints.Add(new Dev2Endpoint(httpEndpoint, httpUrl));

                    EnvironmentVariables.WebServerUri = httpUrl.Replace("*", Environment.MachineName);
                    EnableSslForServer(webServerSslPort, endpoints);

                    EndPoints = endpoints.ToArray();
                }

            }
            catch (Exception ex)
            {
                _writer.Fail("Server initialization failed", ex);
            }
        }


        void EnableSslForServer(string webServerSslPort, List<Dev2Endpoint> endpoints)
        {
            if (!string.IsNullOrEmpty(webServerSslPort) && _isWebServerSslEnabled)
            {
                if (!int.TryParse(webServerSslPort, out int realWebServerSslPort))
                {
                    throw new ArgumentException("Web server ssl port is not valid. Please set the webServerSslPort value in the configuration file.");
                }

                var sslCertPath = ConfigurationManager.AppSettings["sslCertificateName"];

                if (!string.IsNullOrEmpty(sslCertPath))
                {
                    var httpsEndpoint = new IPEndPoint(IPAddress.Any, realWebServerSslPort);
                    var httpsUrl = $"https://*:{webServerSslPort}/";
                    var canEnableSsl = HostSecurityProvider.Instance.EnsureSsl(_fileWrapper, sslCertPath, httpsEndpoint);

                    if (canEnableSsl)
                    {
                        endpoints.Add(new Dev2Endpoint(httpsEndpoint, httpsUrl, sslCertPath));
                    }
                    else
                    {
                        _writer.WriteLine("Could not start webserver to listen for SSL traffic with cert [ " + sslCertPath + " ]");
                    }
                }
            }
        }
    }

}
