/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Warewolf.Resource.Errors;
using Warewolf.Triggers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ExecutionHistory : IExecutionHistory
    {
        public ExecutionHistory(string workflowOutput, IExecutionInfo executionInfo, string userName)
        {
            ExecutionInfo = executionInfo;
            WorkflowOutput = workflowOutput;
            UserName = userName;
        }
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

    public class GetExecutionHistory : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var serializer = new Dev2JsonSerializer();
                if (values == null)
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }
                string queueName = null;
                values.TryGetValue("resourceID", out StringBuilder tmp);
                if (tmp != null)
                {
                    queueName = tmp.ToString();
                    Dev2Logger.Info("Get Execution History. " + tmp, GlobalConstants.WarewolfInfo);
                    //TODO: Once the events are being logged this line will be replaced with a query of that log
                    var b = new ExecutionInfo(new DateTime(2001, 01, 01), new TimeSpan(1, 0, 0), new DateTime(2001, 01, 01), QueueRunStatus.Error, "sdf");
                    var history = new List<IExecutionHistory>();
                    history.Add(new ExecutionHistory("WorflowA", b, "bob"));
                    history.Add(new ExecutionHistory("WorflowB", b, "bob"));
                    history.Add(new ExecutionHistory("WorflowC", b, "bob"));
                    return serializer.SerializeToBuilder(history);
                }
                Dev2Logger.Debug("No QueueName Provided", GlobalConstants.WarewolfDebug);
                return serializer.SerializeToBuilder(new List<IExecutionHistory>());
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public new AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList></DataList>");

        public override string HandlesType() => "GetExecutionHistoryService";
    }
}
