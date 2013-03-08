using System.Collections.Generic;
using System.Runtime.Serialization;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Dev2.Common.ExtMethods;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    public class AddResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {

            IDynamicServicesHost theHost = theWorkspace.Host;

            string roles;
            string resourceDefinition;



            values.TryGetValue("Roles", out roles);
            values.TryGetValue("ResourceXml", out resourceDefinition);

            resourceDefinition = resourceDefinition.Unescape();

            if(string.IsNullOrEmpty(roles) || string.IsNullOrEmpty(resourceDefinition))
            {
                throw new InvalidDataContractException("Roles or ResourceXml is missing");
            }

            List<DynamicServiceObjectBase> compiledResources = theHost.GenerateObjectGraphFromString(resourceDefinition);
            if (compiledResources.Count == 0)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerMessage_BuildFailed);
            }

            return theHost.AddResources(compiledResources, roles);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<root><Roles/><ResourceXml/></root>";
            ServiceAction sa = new ServiceAction();
            sa.Name = HandlesType();
            sa.ActionType = enActionType.InvokeManagementDynamicService;
            sa.SourceMethod = HandlesType();
            newDs.Actions.Add(sa);

            return newDs;
        }

        public string HandlesType()
        {
            return "AddResourceService";
        }
    }
}
