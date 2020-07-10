/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Runtime.CompilerServices;
using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;
using Warewolf.Streams;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] 
namespace QueueWorker
{
    internal class NetworkLogger : ILoggerPublisher
    {
        private IWebSocketPool _webSocketPool;
        private readonly ISerializer _serializer;
        protected ISerializer Serializer { get => _serializer; }

        public NetworkLogger(ISerializer serializer, IWebSocketPool webSocketPool)
        {
            _webSocketPool = webSocketPool;
            _serializer = serializer;
        }

        private void SendMessage(LogEntry logEntry)
        {
            IWebSocketWrapper _ws = null;
            try
            {
                _ws = _webSocketPool.Acquire(Dev2.Common.Config.Auditing.Endpoint);
                if (!_ws.IsOpen())
                {
                    _ws.Connect();
                }

                var logCommand = new AuditCommand
                {
                    Type = "LogEntryCommand",
                    LogEntry = logEntry
                };
                var msg = _serializer.Serialize(logCommand);
                _ws.SendMessage(msg);
            }
            finally
            {
                _webSocketPool.Release(_ws);
            }
        }

        public void Debug(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Debug, outputTemplate, args));
        public void Error(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Error, outputTemplate, args));
        public void Fatal(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Fatal, outputTemplate, args));
        public void Info(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Info, outputTemplate, args));
        public void Warn(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Warn, outputTemplate, args));

        public void Publish(byte[] value)
        {
            IWebSocketWrapper _ws = null;
            try
            {
                _ws = _webSocketPool.Acquire(Dev2.Common.Config.Auditing.Endpoint);
                _ws.SendMessage(value);
            }
            finally
            {
                _webSocketPool.Release(_ws);
            }
        }
    }
}
