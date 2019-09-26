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

    public class AuditCommandConsumer : IConsumer<AuditCommand>
    {
        private readonly IWebSocketConnection _socket;
        private readonly IWriter _writer;
        private readonly ILoggerConsumer<IAudit> _logger;

        public AuditCommandConsumer(ILoggerConsumer<IAudit> loggerConsumer, IWebSocketConnection socket, IWriter writer)
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
                    ExecuteLogQuery(msg.Query, _socket, _writer);
                    break;
                case "TriggerQuery":
                    _writer.Write("Logging Server TriggerQuery " + msg.Query);
                    QueryTriggerLog(msg.Query, _socket, _writer);
                    break;
                default:
                    _writer.Write("Logging Server Invalid Message Type");
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

            writer.Write("sending QueryTriggerLog to server: " + results + "...");
            socket.Send(serializer.Serialize(results));
        }
        private static void ExecuteLogQuery(Dictionary<string, StringBuilder> query, IWebSocketConnection socket, IWriter writer)
        {
            var serializer = new Dev2JsonSerializer();
            var seriLoggerSource = new SeriLoggerSource();
            var auditQueryable = new AuditQueryableSqlite(seriLoggerSource.ConnectionString, seriLoggerSource.TableName);
            var results = auditQueryable.QueryLogData(query);

            writer.Write("sending QueryLog to server: " + results + "...");
            socket.Send(serializer.Serialize(results));
        }
    }
}