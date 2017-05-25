using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetLogDataService : LogDataServiceBase, IEsbManagementEndpoint
    {
        public string HandlesType()
        {
            return "GetLogDataService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2Logger.Info("Get Log Data Service", "Warewolf Info");

            var serializer = new Dev2JsonSerializer();
            try
            {
                var tmpObjects = BuildTempObjects();

                var logEntries = new List<LogEntry>();
                var groupedEntries = tmpObjects.GroupBy(o => o.ExecutionId);
                foreach (var groupedEntry in groupedEntries)
                {
                    var logEntry = new LogEntry
                    {
                        ExecutionId = groupedEntry.Key,
                        Status = "Success"
                    };
                    foreach (var s in groupedEntry)
                    {
                        if (s.Message.StartsWith("Started Execution"))
                        {
                            logEntry.StartDateTime = ParseDate(s.DateTime);
                        }
                        if (s.LogType == "ERROR")
                        {
                            logEntry.Result = "ERROR";
                        }
                        if (s.Message.StartsWith("Completed Execution"))
                        {
                            logEntry.CompletedDateTime = ParseDate(s.DateTime);
                        }
                        if (s.Message.StartsWith("About to execute"))
                        {
                            logEntry.User = GetUser(s.Message)?.TrimStart().TrimEnd() ?? "";
                        }
                        if (!string.IsNullOrEmpty(s.Url))
                        {
                            logEntry.Url = s.Url;
                        }
                    }
                    logEntry.ExecutionTime =
                        (logEntry.CompletedDateTime - logEntry.StartDateTime).Milliseconds.ToString();
                    logEntries.Add(logEntry);
                }

                return FilterResults(values, logEntries, serializer);
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Get Log Data ServiceError", e, "Warewolf Info");
            }
            return serializer.SerializeToBuilder("");
        }

        private StringBuilder FilterResults(Dictionary<string, StringBuilder> values, IEnumerable<LogEntry> filteredEntries, Dev2JsonSerializer dev2JsonSerializer)
        {
            StringBuilder startTimeKey;
            var startTime = default(DateTime);
            if (values.TryGetValue("StartDateTime", out startTimeKey))
            {
                startTime = ParseDate(startTimeKey.ToString());
            }

            StringBuilder endTimeKey;
            var endTime = default(DateTime);
            if (values.TryGetValue("CompletedDateTime", out endTimeKey))
            {
                endTime = ParseDate(endTimeKey.ToString());
            }

            StringBuilder statusKey;
            var status = "";
            if (values.TryGetValue("Status", out statusKey))
            {
                status = statusKey.ToString();
            }
            StringBuilder userKey;
            var user = "";
            if (values.TryGetValue("User", out userKey))
            {
                user = userKey.ToString();
            }
            StringBuilder executionIdKey;
            var executionId = "";
            if (values.TryGetValue("ExecutionId", out executionIdKey))
            {
                executionId = executionIdKey.ToString();
            }
            StringBuilder executionTimeKey;
            var executionTime = "";
            if (values.TryGetValue("ExecutionTime", out executionTimeKey))
            {
                executionTime = executionTimeKey.ToString();
            }
            var entries = filteredEntries.Where(entry => entry.StartDateTime >= startTime)
                .Where(entry => endTime == default(DateTime) || entry.CompletedDateTime <= endTime)
                .Where(entry => string.IsNullOrEmpty(status) || entry.Status.Equals(status, StringComparison.CurrentCultureIgnoreCase))
                .Where(entry => string.IsNullOrEmpty(executionId) || entry.ExecutionId.Equals(executionId, StringComparison.CurrentCultureIgnoreCase))
                .Where(entry => string.IsNullOrEmpty(executionTime) || entry.ExecutionTime.Equals(executionTime, StringComparison.CurrentCultureIgnoreCase))
                .Where(entry => string.IsNullOrEmpty(user) || (entry.User?.Equals(user, StringComparison.CurrentCultureIgnoreCase) ?? false));

            return dev2JsonSerializer.SerializeToBuilder(entries);
        }


        private static DateTime ParseDate(string s)
        {
            return DateTime.ParseExact(s, GlobalConstants.LogFileDateFormat, System.Globalization.CultureInfo.InvariantCulture);
        }


        private string GetUser(string message)
        {
            var toReturn = message.Split('[')[2].Split(':')[0];
            return toReturn;
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
    }
}