using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.Auditing;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class LogDataServiceBase
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;
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
        public IEnumerable<dynamic> BuildTempObjects(Dictionary<string, StringBuilder> keyValue)
        {
            var startTime = DateTime.Parse(HttpUtility.UrlDecode(GetValue<string>("StartDateTime", keyValue)));
            var endTime = DateTime.Parse(HttpUtility.UrlDecode(GetValue<string>("CompletedDateTime", keyValue))); 
            var auditType = GetValue<string>("AuditType", keyValue);
            var executingUser = GetValue<string>("ExecutingUser", keyValue);
            var workflowID = GetValue<string>("WorkflowID", keyValue);
            var executionID = GetValue<string>("ExecutionID", keyValue);
            var isSubExecution = Convert.ToInt32(GetValue<bool>("IsSubExecution", keyValue));
            var isRemoteWorkflow = Convert.ToInt32(GetValue<bool>("IsRemoteWorkflow", keyValue));
            var workflowName = GetValue<string>("WorkflowName", keyValue);
            var serverID = GetValue<string>("ServerID", keyValue);
            var parentID = GetValue<string>("ParentID", keyValue);
            if (startTime == default(DateTime))
            {
                startTime = Convert.ToDateTime(DateTime.Now.AddHours(-3));
            }
            if (endTime == default(DateTime))
            {
                endTime = Convert.ToDateTime(DateTime.Now.AddDays(1));
            }
            var results = Dev2StateAuditLogger.Query(entry =>
                    (entry.AuditDate >= startTime) && (entry.AuditDate <= endTime)
                    && (string.IsNullOrEmpty(auditType) || entry.AuditType == auditType)
                    && (string.IsNullOrEmpty(workflowID) || entry.WorkflowID == workflowID)
                    && (string.IsNullOrEmpty(executionID) || entry.ExecutionID == executionID)
                    && (entry.IsSubExecution == isSubExecution)
                    && (entry.IsRemoteWorkflow == isRemoteWorkflow)
                    && (string.IsNullOrEmpty(workflowName) || entry.WorkflowName == workflowName)
                    && (string.IsNullOrEmpty(serverID) || entry.ServerID == serverID)
                    && (string.IsNullOrEmpty(parentID) || entry.ParentID == parentID)
                    && (string.IsNullOrEmpty(executingUser) || (entry.ExecutingUser == executingUser))
                    ).ToList();
            return results;
        }      
    }
}