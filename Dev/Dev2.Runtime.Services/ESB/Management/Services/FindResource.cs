/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindResource : DefaultEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
           return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                string resourceName = null;
                string type = null;
                string resourceId = null;
                values.TryGetValue("ResourceName", out StringBuilder tmp);
                if (tmp != null)
                {
                    resourceName = tmp.ToString();
                }
                values.TryGetValue("ResourceType", out tmp);
                if (tmp != null)
                {
                    type = tmp.ToString();
                }
                values.TryGetValue("ResourceId", out tmp);
                if (tmp != null)
                {
                    resourceId = tmp.ToString();
                }
           
                List<Resource> resources;
                resources = resourceId == null || resourceId == "*" ? ResourceCatalog.Instance.GetResourceList(theWorkspace.ID, new Dictionary<string, string> { { "resourceName", resourceName }, { "type", type } }).ToList() : ResourceCatalog.Instance.GetResourceList(theWorkspace.ID, new Dictionary<string, string> { { "guidCsv", resourceId }, { "type", type } }).ToList();
                Dev2Logger.Info("Find Resource. ResourceName: " + resourceName, GlobalConstants.WarewolfInfo);


                IList<SerializableResource> resourceList = resources.Select(r=>new FindResourceHelper().SerializeResourceForStudio(r,theWorkspace.ID)).ToList();

                var serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(resourceList);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><ResourceId ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FindResourceService";
    }
}
