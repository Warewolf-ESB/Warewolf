/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading.Tasks;
using Warewolf.Data;
using Warewolf.Logging;
using System;
using Dev2.Common;
using Serilog.Events;
using Warewolf.Interfaces.Auditing;

namespace Warewolf.Driver.Serilog
{
    public class SeriLogConsumer : ILoggerConsumer<IAuditEntry>
    {
        private readonly ILoggerContext _loggerContext;
        private readonly ILoggerPublisher _publisher;

        public SeriLogConsumer(ILoggerContext loggerContext)
        {
            _loggerContext = loggerContext;
            var loggerSource = _loggerContext.Source;
            var connection = loggerSource.NewConnection(_loggerContext.LoggerConfig);
            _publisher = connection.NewPublisher();
        }

        public Task<ConsumerResult> Consume(IAuditEntry item, object parameters)
        {
            try
            {
                LogMessage(item);

                return Task.FromResult(ConsumerResult.Success);
            }
            catch (Exception)
            {
                return Task.FromResult(ConsumerResult.Failed);
            }
        }

        private void LogMessage(IAuditEntry audit)
        {
            var logTemplate = GlobalConstants.WarewolfLogsTemplate;

            // TODO: do not use audit type to determine log level of audit entry
            switch (audit.AuditType)
            {
                case "Information":
                    _publisher.Info(logTemplate, audit);
                    break;
                case "Warning":
                    _publisher.Warn(logTemplate, audit);
                    break;
                case "Error":
                    _publisher.Error(logTemplate, audit.Exception);
                    break;
                case "Fatal":
                    _publisher.Fatal(logTemplate, audit.Exception);
                    break;
                default:
                    _publisher.Debug(logTemplate, audit);
                    break;
            }
        }
    }
}