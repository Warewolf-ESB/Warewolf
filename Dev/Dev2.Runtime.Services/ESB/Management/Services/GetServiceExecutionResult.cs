using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServiceExecutionResult : IEsbManagementEndpoint
    {
        private string _serverLogFilePath;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Administrator;
        }

        public string HandlesType()
        {
            return "GetServiceExecutionService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var executionId = values["ExecutionId"];
            var serializer = new Dev2JsonSerializer();
            var tmpObjects = new List<dynamic>();
            var buffor = new Queue<string>();
            Stream stream = File.Open(ServerLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var file = new StreamReader(stream);
            while (!file.EndOfStream)
            {
                var line = file.ReadLine();

                buffor.Enqueue(line);
            }
            var logData = buffor.AsQueryable();
            foreach (var singleEntry in logData)
            {
                var matches = Regex.Split(singleEntry,
                    @"(\d+[-.\/]\d+[-.\/]\d+ \d+[:]\d+[:]\d+,\d+)\s[[](\w+[-]\w+[-]\w+[-]\w+[-]\w+)[]]\s(\w+)\s+[-]\s+");
                if (matches.Length > 1)
                {
                    var match = matches;
                    var tmpObj = new
                    {
                        ExecutionId = match[2],
                        Message = match[4],
                    };
                    tmpObjects.Add(tmpObj);
                }
            }
            var single = tmpObjects.Single(o => o.Message.StartsWith("Execution Result") && o.ExecutionId?.Equals(executionId.ToString()) ?? false);
            var replace = single.Message.Replace("Execution Result [ ", "").Replace(" ]", "");
            var logEntry = new LogEntry {Result = replace};
            return serializer.SerializeToBuilder(logEntry);
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
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
    }
}