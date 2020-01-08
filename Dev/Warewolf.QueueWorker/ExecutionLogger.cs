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
using Warewolf.Streams;

namespace QueueWorker
{
    internal class ExecutionLogger : NetworkLogger, IExecutionLogPublisher
    {
        public ExecutionLogger(ISerializer serializer, IWebSocketPool webSocketPool)
            : base(serializer, webSocketPool)
        {
        }

        private void LogExecutionCompleted(ExecutionHistory executionHistory)
        {
            var command = new AuditCommand
            {
                Type = "ExecutionAuditCommand",
                ExecutionHistory = executionHistory
            };
            Publish(Serializer.Serialize(command));
        }

        public void ExecutionFailed(ExecutionHistory executionHistory)
        {
            LogExecutionCompleted(executionHistory);
        }

        public void ExecutionSucceeded(ExecutionHistory executionHistory)
        {
            LogExecutionCompleted(executionHistory);
        }

        public void StartExecution(string message, params object[] args) => Info(message, args);
    }
}