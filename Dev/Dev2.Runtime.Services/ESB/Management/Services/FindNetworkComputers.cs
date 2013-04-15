using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find Computers service
    /// </summary>
    class FindNetworkComputers : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder();
            string json = "[";
            try
            {
                var root = new DirectoryEntry("WinNT:");
                foreach (DirectoryEntry dom in root.Children)
                {
                    foreach (DirectoryEntry entry in dom.Children)
                    {
                        if (entry.SchemaClassName == "Computer")
                        {
                            json += @"{""ComputerName"":""" + entry.Name + @"""},";
                        }
                    }
                }
                json += "]";
                json = json.Replace(",]", "]"); // remove last comma
                result.Append("<JSON>");
                result.Append(json);
                result.Append("</JSON>");
            }
            catch (Exception ex)
            {
                result.Append(ex.Message);
            }

            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findNetworkComputersService = new DynamicService();
            findNetworkComputersService.Name = HandlesType();
            findNetworkComputersService.DataListSpecification = "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

            ServiceAction findNetworkComputersAction = new ServiceAction();
            findNetworkComputersAction.Name = HandlesType();
            findNetworkComputersAction.ActionType = enActionType.InvokeManagementDynamicService;
            findNetworkComputersAction.SourceMethod = HandlesType();
            

            findNetworkComputersService.Actions.Add(findNetworkComputersAction);

            return findNetworkComputersService;
        }

        public string HandlesType()
        {
            return "FindNetworkComputersService";
        }
    }
}
