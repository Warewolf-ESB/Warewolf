/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetResourceByType : DefaultEsbManagementEndpoint
    {
        private readonly Lazy<IResourceCatalog> _resourceCatalog;

        public GetResourceByType()
            : this(new Lazy<IResourceCatalog>(() => ResourceCatalog.Instance))
        {
        }
        public GetResourceByType(Lazy<IResourceCatalog> resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                values.TryGetValue("ResourceId", out StringBuilder tmpResourceId);
                var resourceId = Guid.Empty;
                if (tmpResourceId != null)
                {
                    Guid.TryParse(tmpResourceId.ToString(), out resourceId);
                }

                if (resourceId == Guid.Empty)
                {
                    throw new ArgumentNullException("resourceid");
                }
                values.TryGetValue("WorkspaceId", out StringBuilder tmpWorkspaceId);
                var workspaceId = Guid.Empty;
                if (tmpWorkspaceId != null)
                {
                    Guid.TryParse(tmpWorkspaceId.ToString(), out workspaceId);
                }

                Dev2Logger.Info($"Get Resource By Id. workspaceId:{workspaceId} resourceId:{resourceId}", GlobalConstants.WarewolfInfo);

                var result = _resourceCatalog.Value.GetResource(workspaceId, resourceId);
                if (result != null)
                {
                    var serializer = new Dev2JsonSerializer();
                    return serializer.SerializeToBuilder(result);
                }

                return new StringBuilder();
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => nameof(GetResourceByType);
    }
}
