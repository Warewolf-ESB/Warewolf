using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find resources in the service catalog
    /// </summary>
    public class FindResource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {

   
            string resourceName = null;
            string type = null;

            StringBuilder tmp;
            values.TryGetValue("ResourceName", out tmp);
            if(tmp != null)
            {
                resourceName = tmp.ToString();
            }
            values.TryGetValue("ResourceType", out tmp);
            if(tmp != null)
            {
                type = tmp.ToString();
            }
            Dev2Logger.Log.Info("Find Resource. ResourceName:"+resourceName);
            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var resources = ResourceCatalog.Instance.GetResourceList(theWorkspace.ID, resourceName, type, string.Empty);

            IList<SerializableResource> resourceList = resources.Select(new FindResourceHelper().SerializeResourceForStudio).ToList();

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(resourceList);
            }
            catch (Exception err)
            {
                Dev2Logger.Log.Error(err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };

            var findServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(findServiceAction);

            return findServices;
        }

        public string HandlesType()
        {
            return "FindResourceService";
        }
    }
}
