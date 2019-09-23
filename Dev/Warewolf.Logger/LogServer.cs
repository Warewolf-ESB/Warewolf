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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Auditing;
using Warewolf.Data;
using Warewolf.Driver.Serilog;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;
using Warewolf.Streams;

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
        void Start(IList<IWebSocketConnection> clients);
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

        public void Start(IList<IWebSocketConnection> clients)
        {
            var loggerConfig = _loggerContext.LoggerConfig as ILoggerConfig;

            _server = _webSocketServerFactory.New(loggerConfig.ServerLoggingAddress);
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

                var consumer = new SeriLogConsumer(_loggerContext);
                socket.StartConsuming<AuditCommand>(new MyLogConsumer(consumer, socket, _writer));
            });
        }

        class MyLogConsumer : IConsumer<AuditCommand>
        {
            private IWebSocketConnection _socket;
            private IWriter _writer;
            private ILoggerConsumer<IAudit>  _logger;

            public MyLogConsumer(ILoggerConsumer<IAudit> loggerConsumer, IWebSocketConnection socket, IWriter writer)
            {
                _socket = socket;
                _writer = writer;
                _logger = loggerConsumer;
            }

            public Task<ConsumerResult> Consume(AuditCommand msg)
            {
                
                _writer.Write("Logging Server OnMessage: Type:" + msg.Type);

                switch (msg.Type)
                {
                    case "LogEntry":
                        _logger.Consume(msg.Audit);
                        break;
                    case "LogQuery":
                        _writer.Write("Logging Server LogQuery " + msg.Query);
                        QueryLog(msg.Query, _socket, _writer);
                        break;
                    default:
                        _writer.Write("Logging Server Invalid Message Type");
                        Dev2Logger.Info("** Logging Serve Invalid Message Type **", GlobalConstants.WarewolfInfo);
                        break;
                }

                return Task.FromResult(ConsumerResult.Success);
            }
        }

        private static void QueryLog(Dictionary<string, StringBuilder> query, IWebSocketConnection socket, IWriter writer)
        {
            var serializer = new Dev2JsonSerializer();
            var seriLoggerSource = new SeriLoggerSource();
            var auditQueryable = new AuditQueryable(seriLoggerSource.ConnectionString, seriLoggerSource.TableName);
            var results = auditQueryable.QueryLogData(query);

            if (results.Count() > 0)
            {
                writer.Write("sending QueryLog to server: " + results + "...");
                socket.Send(serializer.Serialize(results));
            }
        }
    }
    public static class Ext
    {
        public static void StartConsuming<T>(this IWebSocketConnection socket, IConsumer<T> consumer)
        {
            socket.OnMessage = message =>
            {
                var serializer = new Dev2JsonSerializer();
                var msg = serializer.Deserialize<T>(message);
                consumer.Consume(msg);
            };
        }
    }
}