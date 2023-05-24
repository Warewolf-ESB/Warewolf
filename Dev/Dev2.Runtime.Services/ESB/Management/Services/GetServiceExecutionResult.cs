using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Dev2.Common;
using Warewolf.Auditing.Drivers;
using System;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServiceExecutionResult : LogDataServiceBase, IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var executionId = values["ExecutionID"];
            var trimExecutionId = executionId.ToString().Trim();
            var serializer = new Dev2JsonSerializer();
            var auditQueryable = new AuditQueryableSqlite();

			if (LogDataCache.CurrentResults == null || !LogDataCache.CurrentResults.Any())
            {
				LogDataCache.CurrentResults = auditQueryable.QueryLogData(values).ToList();
            }
            var tmpObjects = LogDataCache.CurrentResults.FirstOrDefault(r => r.ExecutionID == trimExecutionId && r.Environment != string.Empty);
            var message = string.Empty;
            var startDateTime = new DateTime();
            var url = string.Empty;
            var user = string.Empty;
            var completedDateTime = new DateTime();
            var executionTime = string.Empty;
            if (tmpObjects != null)
            {
                message = tmpObjects.Environment;
                startDateTime = tmpObjects.StartDateTime;
                url = tmpObjects.Url;
                user = tmpObjects.User;
                completedDateTime = tmpObjects.CompletedDateTime;
                executionTime = tmpObjects.ExecutionTime;
            }
            var logEntry = new LogEntry
            {
                Result = message,
                ExecutionId = trimExecutionId,
                StartDateTime = startDateTime,
                Url = url,
                User = user,
                CompletedDateTime = completedDateTime,
                ExecutionTime = executionTime
			};
            return serializer.SerializeToBuilder(logEntry);
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => nameof(GetServiceExecutionResult);
    }
}