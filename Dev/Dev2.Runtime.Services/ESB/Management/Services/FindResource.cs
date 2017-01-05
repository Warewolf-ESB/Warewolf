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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find resources in the service catalog
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FindResource : IEsbManagementEndpoint
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
           return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                string resourceName = null;
                string type = null;
                string resourceId = null;
                StringBuilder tmp;
                values.TryGetValue("ResourceName", out tmp);
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
                if (resourceId == null || resourceId == "*")
                {
                    // ReSharper disable once RedundantAssignment
                    resources = ResourceCatalog.Instance.GetResourceList(theWorkspace.ID, new Dictionary<string, string> { { "resourceName", resourceName }, { "type", type } }).ToList();
                }
                else
                {
                    resources = ResourceCatalog.Instance.GetResourceList(theWorkspace.ID, new Dictionary<string, string> { { "guidCsv", resourceId }, { "type", type } }).ToList();
                }
                Dev2Logger.Info("Find Resource. ResourceName: " + resourceName);


                IList<SerializableResource> resourceList = resources.Select(r=>new FindResourceHelper().SerializeResourceForStudio(r,theWorkspace.ID)).ToList();

                var serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(resourceList);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><ResourceId ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

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
