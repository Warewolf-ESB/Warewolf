using Dev2.Common.ExtMethods;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Adds a resource
    /// </summary>
    public class AddResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string roles;
            string resourceDefinition;

            values.TryGetValue("Roles", out roles);
            values.TryGetValue("ResourceXml", out resourceDefinition);

            resourceDefinition = resourceDefinition.Unescape();

            if(string.IsNullOrEmpty(roles) || string.IsNullOrEmpty(resourceDefinition))
            {
                throw new InvalidDataContractException("Roles or ResourceXml is missing");
            }

            List<DynamicServiceObjectBase> compiledResources = DynamicObjectHelper.GenerateObjectGraphFromString(resourceDefinition);
            if(compiledResources.Count == 0)
            {
                return string.Format("<{0}>{1}</{0}>", "Result", Resources.CompilerMessage_BuildFailed);
            }

            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            StringBuilder result = new StringBuilder();
            foreach(var dynamicServiceObjectBase in compiledResources)
            {
                var saveResult = ResourceCatalog.Instance.SaveResource(theWorkspace.ID, dynamicServiceObjectBase.ResourceDefinition);
                result.Append(saveResult.Message);
            }
            return result.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService();
            newDs.Name = HandlesType();
            newDs.DataListSpecification = "<DataList><Roles/><ResourceXml/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";
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
