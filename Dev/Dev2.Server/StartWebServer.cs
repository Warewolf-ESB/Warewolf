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

using System;
using Dev2.Common;
using Dev2.Runtime.WebServer;
using Warewolf;

namespace Dev2
{

    public interface IStartWebServer : IDisposable
    {
        void Execute(IWebServerConfiguration webServerConfig, IPauseHelper pauseHelper);
    }

    public class StartWebServer : IStartWebServer
    {
        private readonly IWriter _writer;
        private readonly Func<Dev2Endpoint[], IDisposable> _startAction;

        IDisposable _owinServer;
        


        public StartWebServer(IWriter writer, Func<Dev2Endpoint[], IDisposable> startAction)
        {
            _writer = writer;
            _startAction = startAction;
        }

        public void Execute(IWebServerConfiguration webServerConfig, IPauseHelper pauseHelper)
        {
            if (webServerConfig.IsWebServerEnabled || webServerConfig.IsWebServerSslEnabled)
            {
                try
                {
                    DoStartWebServer(webServerConfig);
                }
                catch (Exception e)
                {

                    Dev2Logger.Error("Dev2.ServerLifecycleManager", e, GlobalConstants.WarewolfError);
                    EnvironmentVariables.IsServerOnline = false;
                    _writer.Fail("Webserver failed to start", e);
                    pauseHelper.Pause();
                }
            }
        }
        public void DoStartWebServer(IWebServerConfiguration webServerConfig)
        {
            var endPoints = webServerConfig.EndPoints;
            _owinServer = _startAction(endPoints); // WebServerStartup.Start(endPoints)
            EnvironmentVariables.IsServerOnline = true;
            _writer.WriteLine("\r\nWeb Server Started");
            foreach (var endpoint in endPoints)
            {
                _writer.WriteLine($"Web server listening at {endpoint.Url}");
            }
        }
        public void Dispose()
        {
            try
            {
                if (_owinServer != null)
                {
                    _owinServer.Dispose();
                    _owinServer = null;
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(nameof(StartWebServer), ex, GlobalConstants.WarewolfError);
            }
            finally
            {
                EnvironmentVariables.IsServerOnline = false;
            }
        }
    }
}
