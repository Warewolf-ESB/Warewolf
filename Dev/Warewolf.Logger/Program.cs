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
using Dev2.Common.Serializers;
using Fleck;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Warewolf.Auditing;
using Warewolf.Driver.Serilog;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public class Program
    {

        static void Main(string[] args)
        {
            var config = new LoggerContext(args);
            if (config.Errors.Count() > 0)
            {
                Environment.Exit(1);
            };
            var impl = new Implementation(config, new WebSocketServerWrapper.WebSocketServerFactory(), new ConsoleWindowFactory(), new Writer());
            impl.Run();
            impl.Pause();
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
              
                StartLogServer();
            }

            private void StartLogServer()
            {
                var clients = new List<IWebSocketConnection>();
                var loggerConfig = _config.LoggerConfig as ILoggerConfig;

                _server = _webSocketServerFactory.New(loggerConfig.ServerLoggingAddress);
                var logger = _config.Source;
                var connection = logger.NewConnection(_config.LoggerConfig);
                var publisher = connection.NewPublisher();

                _server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        _writer.Write("Logging Server OnOpen...");
                        clients.Add(socket);
                    };
                    socket.OnClose = () =>
                    {
                        _writer.Write("Logging Server OnClose...");
                        clients.Remove(socket);
                    };
                    socket.OnMessage = message =>
                    {
                        var serializer = new Dev2JsonSerializer();
                        var msg = serializer.Deserialize<AuditCommand>(message);
                        _writer.Write("Logging Server OnMessage: Type:" + msg.Type);

                        switch (msg.Type)
                        {
                            case "LogEntry":
                                _writer.Write("Logging Server LogMessage" + message);
                                LogMessage(publisher: publisher, audit: msg.Audit);
                                break;
                            case "LogQuery":
                                _writer.Write("Logging Server LogQuery" + message);
                                QueryLog(query: msg.Query, socket: socket);
                                break;
                            default:
                                _writer.Write("Logging Server Invalid Message Type");
                                Dev2Logger.Info("** Logging Serve Invalid Message Type **", GlobalConstants.WarewolfInfo);
                                break;
                        }
                    };

                });
            }

            private void QueryLog(Dictionary<string, StringBuilder> query, IWebSocketConnection socket)
            {
                var serializer = new Dev2JsonSerializer();
                var seriLoggerSource = new SeriLoggerSource();
                var auditQueryable = new AuditQueryable(seriLoggerSource.ConnectionString, seriLoggerSource.TableName);
                var results = auditQueryable.QueryLogData(query);

                if (results.Count() > 0)
                {
                    _writer.Write("sending QueryLog to server: " + results + "...");
                    socket.Send(serializer.Serialize(results));
                }
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
