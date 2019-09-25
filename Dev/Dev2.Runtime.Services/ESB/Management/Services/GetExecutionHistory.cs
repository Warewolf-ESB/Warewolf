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
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
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
        private readonly IWebSocketServerFactory _webSocketServerFactory;
        private readonly IWebSocketFactory _webSocketFactory;
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                var serializer = new Dev2JsonSerializer();
                var result = new List<IExecutionHistory>();
                if (values == null)
                {
                    throw new InvalidDataContractException(ErrorResource.NoParameter);
                }
                values.TryGetValue("ResourceId", out StringBuilder triggerID);
                if (triggerID != null)
                {
                    var _client = WebSocketWrapper.Create(Config.Auditing.Endpoint);
                    _client.Connect();

                    Dev2Logger.Info("Get Execution History Data from Logger Service. " + triggerID, GlobalConstants.WarewolfInfo);
                   
                    var response = "";
                    var message = new AuditCommand
                    {
                        Type = "TriggerQuery",
                        Query = values
                    };
                    try
                    {
                        var ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
                        _client.SendMessage(serializer.Serialize(message));
                        _client.OnMessage((msgResponse, socket) =>
                        {
                            ewh.Set();
                            response = msgResponse;
                            socket.Close();
                        });
                        ewh.WaitOne();
                        LogExecutionHistoryCache.CurrentResults = result;
                        //TODO: response is being returning as IAudit but needs to return as IExecutionHistory
                        return serializer.SerializeToBuilder(result);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Info("Get Execution History Data ServiceError", e, GlobalConstants.WarewolfInfo);
                    }
                    LogExecutionHistoryCache.CurrentResults = result;
                    return serializer.SerializeToBuilder(result);
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

    public static class LogExecutionHistoryCache
    {
        public static IEnumerable<dynamic> CurrentResults { get; set; }
    }
}
