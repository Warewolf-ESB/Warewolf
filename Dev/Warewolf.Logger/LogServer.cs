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
            var loggerConfig = _loggerContext.LoggerConfig;

            _server = _webSocketServerFactory.New(loggerConfig.Endpoint);
            if (Config.Server.ExecutionLogLevel.ConvertToLogLevelEnum() == Dev2.Data.Interfaces.Enums.LogLevel.INFO || Config.Server.ExecutionLogLevel.ConvertToLogLevelEnum() == Dev2.Data.Interfaces.Enums.LogLevel.TRACE)
            {
                FleckLog.Level = Fleck.LogLevel.Info;
            }
            else if (Config.Server.ExecutionLogLevel.ConvertToLogLevelEnum() == Dev2.Data.Interfaces.Enums.LogLevel.DEBUG)
            {
                FleckLog.Level = Fleck.LogLevel.Debug;
            }             
            else if (Config.Server.ExecutionLogLevel.ConvertToLogLevelEnum() == Dev2.Data.Interfaces.Enums.LogLevel.WARN)
            {
                FleckLog.Level = Fleck.LogLevel.Warn;
            }
            else if (Config.Server.ExecutionLogLevel.ConvertToLogLevelEnum() == Dev2.Data.Interfaces.Enums.LogLevel.ERROR || Config.Server.ExecutionLogLevel.ConvertToLogLevelEnum() == Dev2.Data.Interfaces.Enums.LogLevel.FATAL || Config.Server.ExecutionLogLevel.ConvertToLogLevelEnum() == Dev2.Data.Interfaces.Enums.LogLevel.OFF)
            {
                FleckLog.Level = Fleck.LogLevel.Error;
            }
            _server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    clients.Add(socket);
                };
                socket.OnClose = () =>
                {
                    clients.Remove(socket);
                };
                socket.OnError = exception => {
                    if (Config.Server.ExecutionLogLevel == "ERROR" || Config.Server.ExecutionLogLevel == "WARN" || Config.Server.ExecutionLogLevel == "INFO" || Config.Server.ExecutionLogLevel == "DEBUG" || Config.Server.ExecutionLogLevel == "TRACE")
                    {
                        _writer.WriteLine($"Logging Server OnError, Error details:{exception.Message}");
                    }
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
                consumer.Consume(msg, socket);
            };
        }
    }
}