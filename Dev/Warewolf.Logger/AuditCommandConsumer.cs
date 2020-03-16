/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
    public interface IAuditCommandConsumerFactory
    {
        IAuditCommandConsumer New(SeriLogConsumer innerConsumer, IWebSocketConnection socket, IWriter writer);
    }

    public class AuditCommandConsumerFactory : IAuditCommandConsumerFactory
    {
        public IAuditCommandConsumer New(SeriLogConsumer innerConsumer, IWebSocketConnection socket, IWriter writer)
        {
            return new AuditCommandConsumer(innerConsumer, socket, writer);
        }
    }

    public interface IAuditCommandConsumer : IConsumer<AuditCommand>
    {
        new Task<ConsumerResult> Consume(AuditCommand item);
    }

    public class AuditCommandConsumer : IAuditCommandConsumer, IConsumer<AuditCommand>
    {
        private readonly IWebSocketConnection _socket;
        private readonly IWriter _writer;
        private readonly ILoggerConsumer<IAuditEntry> _logger;

        public AuditCommandConsumer(ILoggerConsumer<IAuditEntry> loggerConsumer, IWebSocketConnection socket, IWriter writer)
        {
            _socket = socket;
            _writer = writer;
            _logger = loggerConsumer;
        }

        public Task<ConsumerResult> Consume(AuditCommand item, object parameters)
        {

            _writer.WriteLine("Logging Server OnMessage: Type:" + item.Type);

            var msg = item;
            switch (item.Type)
            {
                case "LogEntry":
                    _logger.Consume(msg.Audit, parameters);
                    break;
                case "LogQuery":
                    _writer.WriteLine("Executing query: " + msg.Query);
                    ExecuteLogQuery(msg.Query, _socket, _writer);
                    break;
                case "TriggerQuery":
                    _writer.WriteLine("Executing TriggerQuery: " + msg.Query);
                    QueryTriggerLog(msg.Query, _socket, _writer);
                    break;
                case "LogEntryCommand":
                    _writer.WriteLine(msg.LogEntry.OutputTemplate);
                    _logger.Consume(msg.LogEntry, parameters);
                    break;
                case "ExecutionAuditCommand":
                    _writer.WriteLine(msg.ExecutionHistory.ResourceId.ToString());
                    _logger.Consume(msg.ExecutionHistory, parameters);
                    break;
                default:
                    _writer.WriteLine("Logging Server Invalid Message Type");
                    Dev2Logger.Info("** Logging Serve Invalid Message Type **", GlobalConstants.WarewolfInfo);
                    break;
            }

            return Task.FromResult(ConsumerResult.Success);
        }
        private static void QueryTriggerLog(Dictionary<string, StringBuilder> query, IWebSocketConnection socket, IWriter writer)
        {
            var serializer = new Dev2JsonSerializer();
            var seriLoggerSource = new SeriLoggerSource();
            var auditQueryable = new AuditQueryableSqlite(seriLoggerSource.ConnectionString, seriLoggerSource.TableName);
            var results = auditQueryable.QueryTriggerData(query);

            writer.WriteLine("sending QueryTriggerLog to server: " + results + "...");
            socket.Send(serializer.Serialize(results));
        }
        private static void ExecuteLogQuery(Dictionary<string, StringBuilder> query, IWebSocketConnection socket, IWriter writer)
        {
            var serializer = new Dev2JsonSerializer();
            var seriLoggerSource = new SeriLoggerSource();
            var auditQueryable = new AuditQueryableSqlite(seriLoggerSource.ConnectionString, seriLoggerSource.TableName);
            var results = auditQueryable.QueryLogData(query);

            writer.WriteLine("sending QueryLog to server: " + results + "...");
            socket.Send(serializer.Serialize(results));
        }
    }
}