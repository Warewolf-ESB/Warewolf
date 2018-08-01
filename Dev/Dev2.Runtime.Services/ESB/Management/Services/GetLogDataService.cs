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
                var startTime = GetDate("StartDateTime", values).ToString();
                var endTime = GetValue("CompletedDateTime", values).ToString();
                var auditType = GetValue("AuditType", values);
                var executingUser = GetValue("User", values);
                var workflowID = GetValue("WorkflowID", values);
                var executionID = GetValue("ExecutionID", values);
                var executionOrigin = GetValue("ExecutionOrigin", values);
                var isSubExecution = GetValue("IsSubExecution", values);
                var isRemoteWorkflow = GetValue("IsRemoteWorkflow", values);
                var workflowName = GetValue("WorkflowName", values);
                var serverID = GetValue("ServerID", values);
                var parentID = GetValue("ParentID", values);
                var executionToken = GetValue("ExecutionToken", values);
                var environment = GetValue("Environment", values);
                var previousActivity = GetValue("PreviousActivity", values);
                var nextActivity = GetValue("NextActivity", values);

                var results = Dev2StateAuditLogger.Query(item =>
               (workflowName == "" || item.WorkflowName.Equals(workflowName)) &&
               (auditType == "" || item.AuditType.Equals(auditType)) &&
               (previousActivity == "" || (item.PreviousActivity != null && item.PreviousActivity.Contains(previousActivity))));

                //var entries = Dev2StateAuditLogger.Query(entry =>
                //    (string.IsNullOrEmpty(startTime) || entry.AuditDate.Equals(startTime, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(endTime) || entry.AuditDate.Equals(endTime, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(auditType) || entry.AuditType.Equals(auditType, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(workflowID) || entry.WorkflowID.Equals(workflowID, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(executionID) || entry.ExecutionID.Equals(executionID, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(executionOrigin) || entry.ExecutionOrigin.Equals(int.Parse(executionOrigin)))
                //    || (string.IsNullOrEmpty(isSubExecution) || entry.IsSubExecution.Equals(int.Parse(isSubExecution)))
                //    || (string.IsNullOrEmpty(isRemoteWorkflow) || entry.IsRemoteWorkflow.Equals(int.Parse(isRemoteWorkflow)))
                //    || (string.IsNullOrEmpty(workflowName) || entry.WorkflowName.Equals(workflowName, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(serverID) || entry.ServerID.Equals(serverID, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(parentID) || entry.ParentID.Equals(parentID, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(executionToken) || entry.ExecutionToken.Equals(executionToken, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(environment) || entry.Environment.Equals(environment, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(previousActivity) || entry.PreviousActivity.Equals(previousActivity, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(nextActivity) || entry.NextActivity.Equals(nextActivity, StringComparison.CurrentCultureIgnoreCase))
                //    || (string.IsNullOrEmpty(executingUser) || (entry.ExecutingUser.Contains(executingUser, StringComparison.CurrentCultureIgnoreCase)))
                //    )
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

        string GetValue(string key, Dictionary<string, StringBuilder> values)
        {
            var toReturn = "";
            if (values.TryGetValue(key, out StringBuilder value))
            {
                toReturn = value.ToString();
            }
            return toReturn;
        }

        DateTime GetDate(string key, Dictionary<string, StringBuilder> values) => ParseDate(GetValue(key, values));
        static DateTime ParseDate(string s) => !string.IsNullOrEmpty(s) ?
              DateTime.ParseExact(s, GlobalConstants.LogFileDateFormat, System.Globalization.CultureInfo.InvariantCulture) :
              new DateTime();

    
        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "GetLogDataService";
    }

    public static class LogDataCache
    {
        public static IEnumerable<dynamic> CurrentResults { get; set; }
    }
}