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
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public interface ILogServerFactory
    {
        ILogServer New(IWebSocketServerFactory webSocketServerFactory, IWriter writer, ILoggerContext loggerContext);
    }

    public class LogServerFactory : ILogServerFactory
    {
        public LogServerFactory()
        {

        }

        public ILogServer New(IWebSocketServerFactory webSocketServerFactory, IWriter writer, ILoggerContext loggerContext)
        {
            return new LogServer(webSocketServerFactory, writer, loggerContext);
        }
    }

    public interface ILogServer
    {
        void Start();
    }

    public class LogServer : ILogServer
    {
        private readonly IWebSocketServerFactory _webSocketServerFactory;
        private readonly IWriter _writer;
        private IWebSocketServerWrapper _server;
        private readonly ILoggerContext _loggerContext;

        public LogServer(IWebSocketServerFactory webSocketServerFactory, IWriter writer, ILoggerContext loggerContext)
        {
            _webSocketServerFactory = webSocketServerFactory;
            _writer = writer;
            _loggerContext = loggerContext;
        }

        public void Start()
        {
            var clients = new List<IWebSocketConnection>();
            var loggerConfig = _loggerContext.LoggerConfig as ILoggerConfig;

            _server = _webSocketServerFactory.New(loggerConfig.ServerLoggingAddress);
            var logger = _loggerContext.Source;
            var connection = logger.NewConnection(_loggerContext.LoggerConfig);
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
    }

}