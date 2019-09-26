/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading.Tasks;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Logging;
using Warewolf.Streams;

namespace QueueWorker
{
    internal class LoggingConsumerWrapper : IConsumer
    {
        private readonly ILoggerPublisher _logger;

        private readonly IConsumer _consumer;

        public LoggingConsumerWrapper(ILoggerPublisher logger, IConsumer consumer)
        {
            _logger = logger;
            _consumer = consumer;
        }

        public Task<ConsumerResult> Consume(byte[] body)
        {
            _logger.Info("processing {body}");

            return _consumer.Consume(body)
                .ContinueWith((requestForwarderResult) =>
                {
                    if (requestForwarderResult.Status == TaskStatus.RanToCompletion)
                    {
                        if (requestForwarderResult.Result == ConsumerResult.Success)
                        {
                            _logger.Info("success");
                        }
                        else
                        {
                            _logger.Warn("failure");
                        }
                    }
                    return requestForwarderResult.Result;
                });
        }
    }
}
