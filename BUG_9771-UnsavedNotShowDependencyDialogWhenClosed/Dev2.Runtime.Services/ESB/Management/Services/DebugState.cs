using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using System.Text;
using Dev2.Runtime.Configuration;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// The find directory service
    /// </summary>
    public class GetDebugState : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string filePath;
            string dir;

            values.TryGetValue("FilePath", out filePath);
            values.TryGetValue("DirectoryPath", out dir);

            var result = new StringBuilder();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                AppendError(result, "FilePath is required");
            }
            else if (string.IsNullOrWhiteSpace(dir))
            {
                AppendError(result, "DirectoryPath is required");
            }
            else
            {
                try
                {
                    Workflow workflow;

                    var fullPath = Path.Combine(dir, filePath);
                    using (var filestream = new FileStream(fullPath, FileMode.Open))
                    {
                        var serializer = new XmlSerializer(typeof (Workflow));
                        using (var reader = new StreamReader(filestream))
                        {
                            workflow = serializer.Deserialize(reader) as Workflow;
                        }
                    }

                    if (workflow == null)
                    {
                        AppendError(result, "This log is an empty file");
                    }
                    else
                    {
                        result.Append("<JSON>");
                        var json = JsonConvert.SerializeObject(workflow.DebugStates);
                        result.Append(json);
                        result.Append("</JSON>");
                    }
                }

                catch (Exception ex)
                {
                    AppendError(result, ex.Message);
                }
            }

            return result.ToString();
        }

        private static void AppendError(StringBuilder result, string msg)
        {
            result.AppendFormat("Error getting debugstates ...");
            result.AppendLine();
            result.Append(String.Format("Error: {0}", msg));
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findDirectoryService = new DynamicService();
            findDirectoryService.Name = HandlesType();
            findDirectoryService.DataListSpecification = "<DataList><DirectoryPath/><FilePath/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

            ServiceAction findDirectoryServiceAction = new ServiceAction();
            findDirectoryServiceAction.Name = HandlesType();
            findDirectoryServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            findDirectoryServiceAction.SourceMethod = HandlesType();

            findDirectoryService.Actions.Add(findDirectoryServiceAction);

            return findDirectoryService;
        }

        public string HandlesType()
        {
            return "DebugStateService";
        }
    }
}
