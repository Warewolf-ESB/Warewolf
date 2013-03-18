using System.Collections.Generic;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using enActionType = Dev2.DynamicServices.enActionType;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// List resource for binding method
    /// </summary>
    public class FindResourceForBinding : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string resourceName;
            string type;
            string roles;

            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("Type", out type);
            values.TryGetValue("Roles", out roles);


            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var result = ResourceCatalog.Instance.GetPayload(theWorkspace.ID, resourceName, type, roles);

            // now extract the attributed junk for the server to properly use
            var attributes = new[] { "Name" };
            var childTags = new[] { "Category" };

            var returnValue = DataListUtil.ExtractAttributeFromTagAndMakeRecordset(result, "Service", attributes, childTags);
            returnValue = returnValue.Replace("<Service>", "<Dev2Service>").Replace("</Service>", "</Dev2Service>");

            return returnValue;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findServicesBinder = new DynamicService();
            findServicesBinder.Name = HandlesType();
            findServicesBinder.DataListSpecification = "<root><Type/><Roles/><ResourceName/></root>";

            ServiceAction findServiceActionBinder = new ServiceAction();
            findServiceActionBinder.Name = HandlesType();
            findServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;
            findServiceActionBinder.SourceMethod = HandlesType();

            findServicesBinder.Actions.Add(findServiceActionBinder);

            return findServicesBinder;
        }

        public string HandlesType()
        {
            return "ListResourcesForBindingService";
        }
    }
}
