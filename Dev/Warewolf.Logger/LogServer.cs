/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Serializers;
using Fleck;
using System;
using System.Collections.Generic;
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

    public partial class LogServer : ILogServer, IDisposable
    {
        private readonly IWebSocketServerFactory _webSocketServerFactory;
        private readonly IWriter _writer;
        private IWebSocketServerWrapper _server;
        private readonly ILoggerContext _loggerContext;
        private IAuditCommandConsumerFactory _auditCommandConsumerFactory;

        public LogServer(IWebSocketServerFactory webSocketServerFactory, IWriter writer, ILoggerContext loggerContext)
        {
            _webSocketServerFactory = webSocketServerFactory ?? throw new ArgumentNullException(nameof(webSocketServerFactory));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _loggerContext = loggerContext ?? throw new ArgumentNullException(nameof(loggerContext));
        }
        public LogServer(IWebSocketServerFactory webSocketServerFactory, IWriter writer, ILoggerContext loggerContext, IAuditCommandConsumerFactory auditCommandConsumerFactory)
            : this(webSocketServerFactory, writer, loggerContext)
        {
            _auditCommandConsumerFactory = auditCommandConsumerFactory;
        }


        public void Start(IList<IWebSocketConnection> clients)
        {
            var loggerConfig = _loggerContext.LoggerConfig as ILoggerConfig;

            _server = _webSocketServerFactory.New(loggerConfig.ServerLoggingAddress);
            _server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    _writer.WriteLine("Logging Server OnOpen...");
                    clients.Add(socket);
                };
                socket.OnClose = () =>
                {
                    _writer.WriteLine("Logging Server OnClose...");
                    clients.Remove(socket);
                };
                socket.OnError = exception =>
                {
                    _writer.WriteLine($"Logging Server OnError, Error details:{exception.Message}");
                };


                var innerConsumer = new SeriLogConsumer(_loggerContext);
                var defaultConsumer = _auditCommandConsumerFactory?.New(innerConsumer, socket, _writer) ?? new AuditCommandConsumerFactory().New(innerConsumer, socket, _writer);
                socket.StartConsuming(new ForwardingConsumer(defaultConsumer, _loggerContext));
            });
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                _server.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    internal class ForwardingConsumer : IConsumer<AuditCommand>
    {
        private readonly IAuditCommandConsumer _defaultConsumer;
        private static IConnection _leaderConnection;
        private readonly IPublisher _publisher;

        public ForwardingConsumer(IAuditCommandConsumer defaultConsumer, ILoggerContext loggerContext)
        {
            _defaultConsumer = defaultConsumer;
            _leaderConnection = loggerContext.LeaderSource?.NewConnection();
            _publisher = _leaderConnection?.NewPublisher(loggerContext.LeaderConfig);
        }

        public Task<ConsumerResult> Consume(AuditCommand item, object parameters)
        {
            try
            {
                var serialize = new JsonSerializer();
                var serilizedLog = serialize.Serialize<AuditCommand>(item);

                _publisher.Publish(serilizedLog);
                return Task.FromResult(ConsumerResult.Success);
            }
            catch
            {
                return _defaultConsumer.Consume(item, parameters);
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
                consumer.Consume(msg, socket);
            };
        }
    }
}