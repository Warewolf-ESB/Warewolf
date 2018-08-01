using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Dev2.Util.ExtensionMethods;
using Dev2.Runtime.Auditing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetLogDataService : LogDataServiceBase, IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2Logger.Info("Get Log Data Service", GlobalConstants.WarewolfInfo);
            var serializer = new Dev2JsonSerializer();
            try
            {
                var startTime = GetValue<string> ("StartDateTime", values);
                var endTime = GetValue<string>("CompletedDateTime", values);
                var auditType = GetValue<string>("AuditType", values);
                var executingUser = GetValue<string>("User", values);
                var workflowID = GetValue<string>("WorkflowID", values);
                var executionID = GetValue<string>("ExecutionID", values);
                var executionOrigin = GetValue<long>("ExecutionOrigin", values);
                var isSubExecution = GetValue<long>("IsSubExecution", values);
                var isRemoteWorkflow = GetValue<long>("IsRemoteWorkflow", values);
                var workflowName = GetValue<string>("WorkflowName", values);
                var serverID = GetValue<string>("ServerID", values);
                var parentID = GetValue<string>("ParentID", values);

                var results = Dev2StateAuditLogger.Query(entry =>
                    (string.IsNullOrEmpty(startTime) || entry.AuditDate == startTime)
                    && (string.IsNullOrEmpty(endTime) || entry.AuditDate == endTime)
                    && (string.IsNullOrEmpty(auditType) || entry.AuditType == auditType)
                    && (string.IsNullOrEmpty(workflowID) || entry.WorkflowID == workflowID)
                    && (string.IsNullOrEmpty(executionID) || entry.ExecutionID == executionID)
                    && (isSubExecution ==0 || entry.IsSubExecution == isSubExecution)
                    && (isRemoteWorkflow ==0 || entry.IsRemoteWorkflow == isRemoteWorkflow)
                    && (string.IsNullOrEmpty(workflowName) || entry.WorkflowName == workflowName)
                    && (string.IsNullOrEmpty(serverID) || entry.ServerID == serverID)
                    && (string.IsNullOrEmpty(parentID) || entry.ParentID == parentID)
                    && (string.IsNullOrEmpty(executingUser) || (entry.ExecutingUser == executingUser))
                    );
                var result = results.ToList();
                var logEntries = new List<LogEntry>();
                LogDataCache.CurrentResults = result;
                return serializer.SerializeToBuilder(result);
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Get Log Data ServiceError", e, GlobalConstants.WarewolfInfo);
            }
            return serializer.SerializeToBuilder("");
        }

        T GetValue<T>(string key, Dictionary<string, StringBuilder> values)
        {
            var toReturn = default(T);
            if (values.TryGetValue(key, out StringBuilder value))
            {
                var item = value.ToString();
                return (T)Convert.ChangeType(item, typeof(T));
            }
            return toReturn;
        }
              
        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "GetLogDataService";
    }

    public static class LogDataCache
    {
        public static IEnumerable<dynamic> CurrentResults { get; set; }
    }
}