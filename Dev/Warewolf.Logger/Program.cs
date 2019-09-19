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
using System.Linq;
using Warewolf.Auditing;
using Warewolf.Common;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public class Program
    {

        static void Main(string[] args)
        {           
            var config = new LoggerContext(args);
            if(config.Errors.Count() > 0)
            {
                Environment.Exit(1);
            };
            new Implementation(config, new WebSocketServerWrapper.WebSocketServerFactory()).Run();
        }
        private static void Pause() => Console.ReadLine();

        private class Implementation
        {
            private IWebSocketServerWrapper _server;
            private readonly WebSocketServerWrapper.IWebSocketServerFactory _webSocketServerFactory;
            private readonly ILoggerContext _config;

            public Implementation(ILoggerContext config, WebSocketServerWrapper.IWebSocketServerFactory webSocketServerFactory)
            {
                _config = config;
                _webSocketServerFactory = webSocketServerFactory;
            }

            public void Run()
            {
                _ = new ConsoleWindow();
                var loggerConfig = _config.LoggerConfig as ILoggerConfig;

                _server = _webSocketServerFactory.New(loggerConfig.ServerLoggingAddress); 

                var logger = _config.Source;
                var connection = logger.NewConnection(_config.LoggerConfig);
                var publisher = connection.NewPublisher();

                _server.Start(socket =>
                {
                    socket.OnMessage = message =>  publisher.Info(message);
                });
                Pause();
            }
        }
    }
}
