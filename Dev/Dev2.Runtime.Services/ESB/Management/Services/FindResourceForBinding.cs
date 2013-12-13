using System.Collections.Generic;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
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
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            return new StringBuilder();
            //string resourceName = null;
            //string type = null;

            //StringBuilder tmp;
            //values.TryGetValue("ResourceName", out tmp);
            //if (tmp != null)
            //{
            //    resourceName = tmp.ToString();
            //}
            //values.TryGetValue("Type", out tmp);
            //if(tmp != null)
            //{
            //    type = tmp.ToString();
            //}

            //// BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            //var result = ResourceCatalog.Instance.GetPayload(theWorkspace.ID, resourceName, type);

            //// now extract the attributed junk for the server to properly use
            //var attributes = new[] { "Name" };
            //var childTags = new[] { "Category" };

            //var returnValue = DataListUtil.ExtractAttributeFromTagAndMakeRecordset(result, "Service", attributes, childTags);
            //returnValue = returnValue.Replace("<Service>", "<Dev2Service>").Replace("</Service>", "</Dev2Service>");

            //return returnValue;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findServicesBinder = new DynamicService();
            findServicesBinder.Name = HandlesType();
            findServicesBinder.DataListSpecification = "<DataList><Type/><Roles/><ResourceName/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>";

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
