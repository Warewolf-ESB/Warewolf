using System;
using System.Collections.Generic;
using Dev2.DynamicServices;
using Dev2.Reflection;
using System.Text;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find registred assemblies
    /// </summary>
    public class RegisteredAssembly : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder();
            IDynamicServicesHost theHost = theWorkspace.Host;

            try
            {
                IAssemblyName assemblyName;
                IAssemblyEnum assemblyEnum = GAC.CreateGACEnum();
                string json = "[";
                while (GAC.GetNextAssembly(assemblyEnum, out assemblyName) == 0)
                {
                    json += @"{""AssemblyName"":""" + GAC.GetName(assemblyName) + @"""}";
                    json += ",";
                }
                json += "]";
                json = json.Replace(",]", "]"); //remove the last comma in the string in order to have valid json
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
            DynamicService registeredAssemblyService = new DynamicService();
            registeredAssemblyService.Name = HandlesType();
            registeredAssemblyService.DataListSpecification = "<root></root>";

            ServiceAction registeredAssemblyAction = new ServiceAction();
            registeredAssemblyAction.Name = HandlesType();
            registeredAssemblyAction.SourceMethod = HandlesType();
            registeredAssemblyAction.ActionType = enActionType.InvokeManagementDynamicService;

            registeredAssemblyService.Actions.Add(registeredAssemblyAction);

            return registeredAssemblyService;
        }

        public string HandlesType()
        {
            return "RegisteredAssemblyService";
        }
    }
}
