/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;
using Warewolf.Streams;

namespace QueueWorker
{
    internal class NetworkLogger : ILoggerPublisher
    {
        private IWebSocketWrapper _ws;
        private readonly string _endpoint;
        private readonly ISerializer _serializer;
        protected ISerializer Serializer { get => _serializer; }

        public NetworkLogger(string endpoint, ISerializer serializer)
        {
            _endpoint = endpoint;
            _serializer = serializer;
            _ws = new WebSocketFactory().New(); 
            _ws.Connect();
        }

        private void SendMessage(LogEntry logEntry)
        {
            if (!_ws.IsOpen())
            {
                _ws = new WebSocketFactory().New();
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

        public void Debug(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Debug, outputTemplate, args));
        public void Error(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Error, outputTemplate, args));
        public void Fatal(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Fatal, outputTemplate, args));
        public void Info(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Info, outputTemplate, args));
        public void Warn(string outputTemplate, params object[] args) => SendMessage(new LogEntry(LogLevel.Warn, outputTemplate, args));

        public void Publish(byte[] value) => _ws.SendMessage(value);
    }
}
