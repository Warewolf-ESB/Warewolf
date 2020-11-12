/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;
using Warewolf.Triggers;


namespace Warewolf.Execution
{
    public interface IExecutionLogPublisher : ILoggerPublisher
    {
        void ExecutionFailed(IExecutionHistory executionHistory);
        void ExecutionSucceeded(IExecutionHistory executionHistory);
        void StartExecution(string message, params object[] args);
        void LogResumedExecution(IAudit values);
    }
}