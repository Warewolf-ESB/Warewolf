using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
            var startDateTime = GetValue<string>("StartDateTime", keyValue);
            var startTime = Convert.ToDateTime(DateTime.Now.AddHours(-3));
            if (!string.IsNullOrEmpty(startDateTime))
            {
                startTime = DateTime.Parse(HttpUtility.UrlDecode(startDateTime));
            }
            var completedDateTime = GetValue<string>("CompletedDateTime", keyValue);
            var endTime = Convert.ToDateTime(DateTime.Now.AddDays(1));
            if (!string.IsNullOrEmpty(startDateTime))
            {
                endTime = DateTime.Parse(HttpUtility.UrlDecode(completedDateTime));
            }
            var auditType = GetValue<string>("AuditType", keyValue);
            var executingUser = GetValue<string>("ExecutingUser", keyValue);
            var workflowID = GetValue<string>("WorkflowID", keyValue);
            var executionID = GetValue<string>("ExecutionID", keyValue);
            var isSubExecution = Convert.ToInt32(GetValue<bool>("IsSubExecution", keyValue));
            var isRemoteWorkflow = Convert.ToInt32(GetValue<bool>("IsRemoteWorkflow", keyValue));
            var workflowName = GetValue<string>("WorkflowName", keyValue);
            var serverID = GetValue<string>("ServerID", keyValue);
            var parentID = GetValue<string>("ParentID", keyValue);
            var previousActivityId = GetValue<string>("PreviousActivityId", keyValue);

            var predicate = PredicateBuilder.True<AuditLog>();
            predicate = predicate.And (p => (p.AuditDate >= startTime) && (p.AuditDate <= endTime));
            predicate = predicate.And (p => (string.IsNullOrEmpty(auditType) || p.AuditType == auditType));
            predicate = predicate.And(p => (string.IsNullOrEmpty(executingUser) || p.ExecutingUser == executingUser));
            predicate = predicate.And(p => (string.IsNullOrEmpty(workflowID) || p.WorkflowID == workflowID));
            predicate = predicate.And(p => (string.IsNullOrEmpty(executionID) || p.ExecutionID == executionID));
            predicate = predicate.And(p => (string.IsNullOrEmpty(workflowName) || p.WorkflowName == workflowName));
            predicate = predicate.And(p => (string.IsNullOrEmpty(serverID) || p.ServerID == serverID));
            predicate = predicate.And(p => (string.IsNullOrEmpty(parentID) || p.ParentID == parentID));
            predicate = predicate.And(p => (string.IsNullOrEmpty(previousActivityId) || p.PreviousActivityId == previousActivityId));
            predicate = predicate.And(p => (p.IsSubExecution == isSubExecution));
            predicate = predicate.And(p => (p.IsRemoteWorkflow == isRemoteWorkflow));

            var query = from p in Dev2StateAuditLogger.Query(predicate)
                  select p;

            return query.ToList();
        }
    }
}