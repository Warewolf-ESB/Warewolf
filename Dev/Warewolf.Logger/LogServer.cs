/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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

        public LogServer(IWebSocketServerFactory webSocketServerFactory, IWriter writer, ILoggerContext loggerContext)
        {
            _webSocketServerFactory = webSocketServerFactory ?? throw new ArgumentNullException(nameof(webSocketServerFactory));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _loggerContext = loggerContext ?? throw new ArgumentNullException(nameof(loggerContext));
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

                var consumer = new SeriLogConsumer(_loggerContext);
                socket.StartConsuming(new AuditCommandConsumer(consumer, socket, _writer));
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
    public static class Ext
    {
        public static void StartConsuming<T>(this IWebSocketConnection socket, IConsumer<T> consumer)
        {
            socket.OnMessage = message =>
            {
                var serializer = new Dev2JsonSerializer();
                var msg = serializer.Deserialize<T>(message);
                consumer.Consume(msg, null);
            };
        }
    }
}