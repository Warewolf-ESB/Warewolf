using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class DeleteLog : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string result;

            string filePath;
            string directory;

            values.TryGetValue("FilePath", out filePath);
            values.TryGetValue("Directory", out directory);

            if(String.IsNullOrWhiteSpace(filePath))
            {
                result = FormatMessage("Can't delete a file if no filename is passed.", filePath, directory);
                ServerLogger.LogMessage(result);
            }
            else if(String.IsNullOrWhiteSpace(directory))
            {
                result = FormatMessage("Can't delete a file if no directory is passed.", filePath, directory);
                ServerLogger.LogMessage(result);
            }
            else if(!Directory.Exists(directory))
            {
                result = FormatMessage("No such directory exists on the server.", filePath, directory);
                ServerLogger.LogMessage(result);
            }
            else
            {
                var path = Path.Combine(directory, filePath);

                if(!File.Exists(path))
                {
                    result = FormatMessage("No such file exists on the server.", filePath, directory);
                    ServerLogger.LogMessage(result);
                }
                else
                {
                    try
                    {
                        File.Delete(path);
                        result = "Success";
                    }
                    catch(Exception ex)
                    {
                        result = FormatMessage(ex.Message, filePath, directory);
                        ServerLogger.LogMessage(result + "\n" + ex.StackTrace);
                    }
                }
            }
            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService();
            findDirectoryService.Name = HandlesType();
            findDirectoryService.DataListSpecification = "<DataList><Directory/><FilePath/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

            ServiceAction findDirectoryServiceAction = new ServiceAction();
            findDirectoryServiceAction.Name = HandlesType();
            findDirectoryServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDirectoryServiceAction.SourceMethod = HandlesType();

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "DeleteLogService";
        }

        static string FormatMessage(string message, string filePath, string directory)
        {
            return string.Format("DeleteLog: Error deleting '{0}' from '{1}'...{2}", filePath, directory, message);
        }
    }
}
