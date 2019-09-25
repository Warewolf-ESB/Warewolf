/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Newtonsoft.Json;
using System;
using Warewolf.Triggers;

namespace Warewolf.Auditing
{
    public class ExecutionHistory : IExecutionHistory
    {
        public ExecutionHistory(Guid resourceId, string workflowOutput, IExecutionInfo executionInfo, string userName)
        {
            ResourceId = resourceId;
            ExecutionInfo = executionInfo;
            WorkflowOutput = workflowOutput;
            UserName = userName;
        }
        public Guid ResourceId { get; set; }
        public string WorkflowOutput { get; private set; }
        public IExecutionInfo ExecutionInfo { get; private set; }
        public string UserName { get; set; }
    }
    public class ExecutionInfo : IExecutionInfo
    {
        public ExecutionInfo(DateTime startDate, TimeSpan duration, DateTime endDate, QueueRunStatus success, string executionId, string failureReason)
        {

            ExecutionId = executionId;
            Success = success;
            EndDate = endDate;
            Duration = duration;
            StartDate = startDate;
            FailureReason = failureReason;
        }
        [JsonConstructor]
        public ExecutionInfo(DateTime startDate, TimeSpan duration, DateTime endDate, QueueRunStatus success, string executionId)
            : this(startDate, duration, endDate, success, executionId, "")
        {
            ExecutionId = executionId;
            Success = success;
            EndDate = endDate;
            Duration = duration;
            StartDate = startDate;
        }

        public DateTime StartDate { get; private set; }
        public TimeSpan Duration { get; private set; }
        public DateTime EndDate { get; private set; }
        public QueueRunStatus Success { get; private set; }
        public string ExecutionId { get; private set; }
        public string FailureReason { get; private set; }
    }
}
