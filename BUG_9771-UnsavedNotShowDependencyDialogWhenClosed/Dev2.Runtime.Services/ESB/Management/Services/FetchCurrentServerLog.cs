using System.Collections.Generic;
using System.IO;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchCurrentServerLog : IEsbManagementEndpoint
    {
        readonly string _serverLogPath;

        public FetchCurrentServerLog()
            : this(Path.Combine(EnvironmentVariables.ApplicationPath, "ServerLog.txt"))
        {
        }

        public FetchCurrentServerLog(string serverLogPath)
        {
            _serverLogPath = serverLogPath;
        }

        public string ServerLogPath { get { return _serverLogPath; } }

        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            var logData = "";
            if(File.Exists(_serverLogPath))
            {
                logData = File.ReadAllText(_serverLogPath);
                File.Delete(_serverLogPath);
            }
            return logData;
        }

        public DynamicService CreateServiceEntry()
        {
            var findDirectoryService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            };

            var findDirectoryServiceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "FetchCurrentServerLogService";
        }
    }
}