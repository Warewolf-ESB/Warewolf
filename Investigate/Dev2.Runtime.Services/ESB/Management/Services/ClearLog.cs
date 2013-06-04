using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ClearLog : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder();

            string directory;

            values.TryGetValue("Directory", out directory);
            
            if (String.IsNullOrWhiteSpace(directory))
            {
                AppendError(result, directory,"Cant delete a file if no directory is passed.");
            }
            else if (!Directory.Exists(directory))
            {
                AppendError(result, directory, "No such directory exists on the server.");
            }
            else
            {
                try
                {
                    var files = Directory.GetFiles(directory);

                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    result.Append("Success");
                }
                catch (Exception ex)
                {
                    AppendError(result, directory, ex.Message);
                    ServerLogger.LogMessage(ex.StackTrace);
                }
            }

            return result.ToString();
        }

        private static void AppendError(StringBuilder result, string directory, string msg)
        {
            result.AppendFormat("Error clearing '{0}'...", directory);
            result.AppendLine();
            result.Append(String.Format("Error: {0}", msg));
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService();
            findDirectoryService.Name = HandlesType();
            findDirectoryService.DataListSpecification = "<DataList><Directory/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

            ServiceAction findDirectoryServiceAction = new ServiceAction();
            findDirectoryServiceAction.Name = HandlesType();
            findDirectoryServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDirectoryServiceAction.SourceMethod = HandlesType();

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "ClearLogService";
        }

    }
}
