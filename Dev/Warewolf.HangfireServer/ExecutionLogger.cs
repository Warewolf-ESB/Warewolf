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
using Warewolf.Streams;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Warewolf.HangfireServer
{
    internal class ExecutionLogger : NetworkLogger, IExecutionLogPublisher
    {
        public interface IExecutionLoggerFactory
        {
            IExecutionLogPublisher New(ISerializer jsonSerializer, IWebSocketPool webSocketPool);
        }

        public class ExecutionLoggerFactory : IExecutionLoggerFactory
        {
            public IExecutionLogPublisher New(ISerializer serializer, IWebSocketPool webSocketPool)
            {
                return new ExecutionLogger(serializer, webSocketPool);
            }
        }

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

        public void LogResumedExecution(Audit values)
        {
            var command = new AuditCommand
            {
                Type = "ResumeExecution",
                Audit = values
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