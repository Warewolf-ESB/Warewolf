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
using Warewolf.Interfaces.Auditing;
using Warewolf.Triggers;

namespace Warewolf.Auditing
{
    public class ExecutionHistory : IExecutionHistory, IAuditEntry
    {
        public ExecutionHistory()
        {
        }

        [JsonConstructor]
        public ExecutionHistory(Guid resourceId, string workflowOutput, ExecutionInfo executionInfo, string userName)
        {
            ResourceId = resourceId;
            ExecutionInfo = executionInfo;
            WorkflowOutput = workflowOutput;
            UserName = userName;
        }
        public Guid ResourceId { get; set; }
        public string WorkflowOutput { get; set; }
        public IExecutionInfo ExecutionInfo { get; set; }
        public string UserName { get; set; }
        public Exception Exception { get; set; }
        public string AuditType { get; set; } = "Information";
    }

    public class ExecutionInfo : IExecutionInfo
    {
        public ExecutionInfo()
        {
        }

        public ExecutionInfo(DateTime startDate, TimeSpan duration, DateTime endDate, QueueRunStatus success, Guid executionId, string failureReason, string customTransactionID)
        {
            ExecutionId = executionId;
            CustomTransactionID = customTransactionID;
            Success = success;
            EndDate = endDate;
            Duration = duration;
            StartDate = startDate;
            FailureReason = failureReason;
        }
        [JsonConstructor]
        public ExecutionInfo(DateTime startDate, TimeSpan duration, DateTime endDate, QueueRunStatus success, Guid executionId, string customTransactionID)
            : this(startDate, duration, endDate, success, executionId, "", customTransactionID)
        {
            ExecutionId = executionId;
            CustomTransactionID = customTransactionID;
            Success = success;
            EndDate = endDate;
            Duration = duration;
            StartDate = startDate;
        }
        public string CustomTransactionID { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndDate { get; set; }
        public QueueRunStatus Success { get; set; }
        public Guid ExecutionId { get; set; }
        public string FailureReason { get; set; }
    }
}