/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
            var impl = new Implementation(config, new WebSocketServerWrapper.WebSocketServerFactory(), new ConsoleWindowFactory(), new Writer()).Run();
        }

        public class Implementation
        {
            private IWebSocketServerWrapper _server;
            private readonly IConsoleWindowFactory _consoleWindowFactory;
            private readonly WebSocketServerWrapper.IWebSocketServerFactory _webSocketServerFactory;
            private readonly ILoggerContext _config;
            private readonly IWriter _writer;

            public Implementation(ILoggerContext config, WebSocketServerWrapper.IWebSocketServerFactory webSocketServerFactory, IConsoleWindowFactory consoleWindowFactory, IWriter writer)
            {
                _config = config;
                _webSocketServerFactory = webSocketServerFactory;
                _consoleWindowFactory = consoleWindowFactory;
                _writer = writer;
            }

            public void Run()
            {
                _ = _consoleWindowFactory.New();
                var loggerConfig = _config.LoggerConfig as ILoggerConfig;

                _server = _webSocketServerFactory.New(loggerConfig.ServerLoggingAddress); 

                var logger = _config.Source;
                var connection = logger.NewConnection(_config.LoggerConfig);
                var publisher = connection.NewPublisher();

                _server.Start(socket =>
                {
                    //TODO: JsonConvert should be wrapped?
                    socket.OnMessage = message =>  LogMessage(publisher: publisher, audit: JsonConvert.DeserializeObject<Audit>(message));
                });
                Pause();
            }

            private void LogMessage(ILoggerPublisher publisher, Audit audit)
            {
                var logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

                switch (audit.AuditType)
                {
                    case "Information":
                        publisher.Info(logTemplate, DateTime.Now, LogEventLevel.Information, audit);
                        break;
                    case "Warning":
                        publisher.Warn(logTemplate, DateTime.Now, LogEventLevel.Warning, audit);
                        break;
                    case "Error":
                        publisher.Error(logTemplate, DateTime.Now, LogEventLevel.Error, audit, Environment.NewLine, audit.Exception);
                        break;
                    case "Fatal":
                        publisher.Fatal(logTemplate, DateTime.Now, LogEventLevel.Fatal, audit, Environment.NewLine, audit.Exception);
                        break;

                    default:
                        break;
                }
            }

            public void Pause()
            {
                _writer.ReadLine();
            }
        }
    }
}
