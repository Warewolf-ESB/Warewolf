using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Dev2.Util.ExtensionMethods;

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

        StringBuilder FilterResults(Dictionary<string, StringBuilder> values, IEnumerable<LogEntry> filteredEntries, Dev2JsonSerializer dev2JsonSerializer)
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


        static DateTime ParseDate(string s) => !string.IsNullOrEmpty(s) ?
                DateTime.ParseExact(s, GlobalConstants.LogFileDateFormat, System.Globalization.CultureInfo.InvariantCulture) :
                new DateTime();

        string GetUser(string message)
        {
            var toReturn = message.Split('[')[2].Split(':')[0];
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