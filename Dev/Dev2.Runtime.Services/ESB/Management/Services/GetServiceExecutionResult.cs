using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Dev2.Common;
using Warewolf.Auditing.Drivers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServiceExecutionResult : LogDataServiceBase, IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var executionId = values["ExecutionId"];
            var trimExecutionId = executionId.ToString().Trim();
            var serializer = new Dev2JsonSerializer();
            var auditQueryable = new AuditQueryableSqlite();

			if (LogDataCache.CurrentResults == null || !LogDataCache.CurrentResults.Any())
            {
				LogDataCache.CurrentResults = auditQueryable.QueryLogData(values).ToList();
            }
            var tmpObjects = LogDataCache.CurrentResults.FirstOrDefault(r => r.ExecutionId == trimExecutionId && r.Message.StartsWith("Execution Result"));
            var replace = "";
            if (tmpObjects != null)
            {
                string message = tmpObjects.Message;
                var startIdx = GlobalConstants.ExecutionLoggingResultStartTag.Length;
                var endIdx = GlobalConstants.ExecutionLoggingResultEndTag.Length;
                var lengthToSelect = message.Length - startIdx - endIdx;
                replace = message.Substring(startIdx, lengthToSelect);
            }
            var logEntry = new Common.LogEntry { Result = replace };
            return serializer.SerializeToBuilder(logEntry);
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => nameof(GetServiceExecutionResult);
    }
}