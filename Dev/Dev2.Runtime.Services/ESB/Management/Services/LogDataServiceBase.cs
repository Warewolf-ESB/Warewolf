using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.Auditing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class LogDataServiceBase
    {
        string _serverLogFilePath;
        public string ServerLogFilePath
        {
            get
            {
                return _serverLogFilePath ?? EnvironmentVariables.ServerLogFile;
            }
            set
            {
                _serverLogFilePath = value;
            }
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;

        public IEnumerable<dynamic> BuildTempObjects(Dictionary<string, StringBuilder> values)
        {
            var startTime = GetValue<string>("StartDateTime", values);
            var endTime = GetValue<string>("CompletedDateTime", values);
            var auditType = GetValue<string>("AuditType", values);
            var executingUser = GetValue<string>("User", values);
            var workflowID = GetValue<string>("WorkflowID", values);
            var executionID = GetValue<string>("ExecutionID", values);
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
                    && (isSubExecution == 0 || entry.IsSubExecution == isSubExecution)
                    && (isRemoteWorkflow == 0 || entry.IsRemoteWorkflow == isRemoteWorkflow)
                    && (string.IsNullOrEmpty(workflowName) || entry.WorkflowName == workflowName)
                    && (string.IsNullOrEmpty(serverID) || entry.ServerID == serverID)
                    && (string.IsNullOrEmpty(parentID) || entry.ParentID == parentID)
                    && (string.IsNullOrEmpty(executingUser) || (entry.ExecutingUser == executingUser))
                    ).ToList();

            return results;
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
    }
}