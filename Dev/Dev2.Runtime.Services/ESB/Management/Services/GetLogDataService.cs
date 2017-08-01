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
using Dev2.Util.ExtensionMethods;

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
                            logEntry.Status = "ERROR";
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
                    if (logEntry.StartDateTime != DateTime.MinValue)
                    {
                        logEntry.ExecutionTime = (logEntry.CompletedDateTime - logEntry.StartDateTime).Milliseconds.ToString();
                        logEntries.Add(logEntry);
                    }
                }
                LogDataCache.CurrentResults = tmpObjects;
                return FilterResults(values, logEntries, serializer);
            }
            catch (Exception e)
            {
                Dev2Logger.Info("Get Log Data ServiceError", e, "Warewolf Info");
            }
            return serializer.SerializeToBuilder("");
        }

        private string GetValue(string key, Dictionary<string, StringBuilder> values)
        {
            StringBuilder value;
            string toReturn = "";
            if (values.TryGetValue(key, out value))
            {
                toReturn = value.ToString();
            }
            return toReturn;
        }

        private DateTime GetDate(string key, Dictionary<string, StringBuilder> values)
        {
            return ParseDate(GetValue(key, values));
        }

        private StringBuilder FilterResults(Dictionary<string, StringBuilder> values, IEnumerable<LogEntry> filteredEntries, Dev2JsonSerializer dev2JsonSerializer)
        {
            var startTime = GetDate("StartDateTime", values);
            var endTime = GetDate("CompletedDateTime", values);
            var status = GetValue("Status", values);
            var user = GetValue("User", values);
            var executionId = GetValue("ExecutionId", values);
            var executionTime = GetValue("ExecutionTime", values);

            var entries = filteredEntries.Where(entry => entry.StartDateTime >= startTime)
                .Where(entry => endTime == default(DateTime) || entry.CompletedDateTime <= endTime)
                .Where(entry => string.IsNullOrEmpty(status) || entry.Status.Equals(status, StringComparison.CurrentCultureIgnoreCase))
                .Where(entry => string.IsNullOrEmpty(executionId) || entry.ExecutionId.Equals(executionId, StringComparison.CurrentCultureIgnoreCase))
                .Where(entry => string.IsNullOrEmpty(executionTime) || entry.ExecutionTime.Equals(executionTime, StringComparison.CurrentCultureIgnoreCase))
                .Where(entry => string.IsNullOrEmpty(user) || (entry.User?.Contains(user, StringComparison.CurrentCultureIgnoreCase) ?? false));

            return dev2JsonSerializer.SerializeToBuilder(entries);
        }


        private static DateTime ParseDate(string s)
        {
            return !string.IsNullOrEmpty(s) ?
                DateTime.ParseExact(s, GlobalConstants.LogFileDateFormat, System.Globalization.CultureInfo.InvariantCulture) :
                new DateTime();
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

    public static class LogDataCache
    {
        public static IEnumerable<dynamic> CurrentResults { get; set; }
    }
}