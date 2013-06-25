using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Framework;

namespace Dev2.Studio.Core.AppResources.Repositories
{
    public static class ResourceConverter
    {
        public static void FetchResources(ResourceType resourceType)
        {
            dynamic dataObj = new UnlimitedObject();
            dataObj.Service = "FindResourceService";
            dataObj.ResourceName = "*";
            dataObj.ResourceType = Enum.GetName(typeof(ResourceType), resourceType);
            dataObj.Role = "*";
            //dataObj.Roles = string.Join(",", _securityContext.Roles);

            //var resultObj = ResourceRepository.ExecuteCommand(_environmentModel, dataObj);

            //dynamic wfServices = (resourceType == ResourceType.Source) ? resultObj.Source : resultObj.Service;
            //if (wfServices is List<UnlimitedObject>)
            //{
            //    foreach (dynamic item in wfServices)
            //    {
            //        try
            //        {
            //            //IResourceModel resource = HydrateResourceModel(resourceType, item);
            //            //_resourceModels.Add(resource);
            //            //if (ItemAdded != null)
            //            //{
            //            //    ItemAdded(resource, null);
            //            //}
            //        }
            //        // ReSharper disable EmptyGeneralCatchClause
            //        catch
            //        // ReSharper restore EmptyGeneralCatchClause
            //        {
            //            // Ignore malformed resources
            //            // TODO Log this
            //        }
            //    }
            //}
        }
    }
}
