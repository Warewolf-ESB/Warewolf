#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Web;
using Dev2.Common.Interfaces.Enums;
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
        public IEnumerable<dynamic> BuildTempObjects(Dictionary<string, StringBuilder> values)
        {
            var startTime = Convert.ToDateTime(HttpUtility.UrlDecode(GetValue<string>("StartDateTime", values)));
            var endTime = Convert.ToDateTime(HttpUtility.UrlDecode(GetValue<string>("CompletedDateTime", values)));
            var auditType = GetValue<string>("AuditType", values);
            var executingUser = GetValue<string>("User", values);
            var workflowID = GetValue<string>("WorkflowID", values);
            var executionID = GetValue<string>("ExecutionID", values);
            var isSubExecution = GetValue<bool>("IsSubExecution", values);
            var isRemoteWorkflow = GetValue<bool>("IsRemoteWorkflow", values);
            var workflowName = GetValue<string>("WorkflowName", values);
            var serverID = GetValue<string>("ServerID", values);
            var parentID = GetValue<string>("ParentID", values);

            var results = Dev2StateAuditLogger.Query(entry =>
                    (entry.AuditDate >= startTime)
                    && (entry.AuditDate <= endTime)
                    && (string.IsNullOrEmpty(auditType) || entry.AuditType == auditType)
                    && (string.IsNullOrEmpty(workflowID) || entry.WorkflowID == workflowID)
                    && (string.IsNullOrEmpty(executionID) || entry.ExecutionID == executionID)
                    && (isSubExecution == false || entry.IsSubExecution == isSubExecution)
                    && (isRemoteWorkflow == false || entry.IsRemoteWorkflow == isRemoteWorkflow)
                    && (string.IsNullOrEmpty(workflowName) || entry.WorkflowName == workflowName)
                    && (string.IsNullOrEmpty(serverID) || entry.ServerID == serverID)
                    && (string.IsNullOrEmpty(parentID) || entry.ParentID == parentID)
                    && (string.IsNullOrEmpty(executingUser) || (entry.ExecutingUser == executingUser))
                    ).ToList();

            return results;
        }
    }
}