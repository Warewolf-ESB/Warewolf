#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveComPluginSource : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override  AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        IResourceCatalog _resourceCatalog;

        public SaveComPluginSource()
        {
            
        }

        public SaveComPluginSource(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public override  StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Com Plugin Source", GlobalConstants.WarewolfInfo);
                values.TryGetValue("ComPluginSource", out StringBuilder resourceDefinition);
                var src = serializer.Deserialize<ComPluginSourceDefinition>(resourceDefinition);
                if (src.ResourcePath == null)
                {
                    src.ResourcePath = string.Empty;
                }

                if (src.ResourcePath.EndsWith("\\"))
                {
                    src.ResourcePath = src.ResourcePath.Substring(0, src.ResourcePath.LastIndexOf("\\", StringComparison.Ordinal));
                }

                ComPluginSource res1;
                var existingSource = ResourceCat.GetResource(GlobalConstants.ServerWorkspaceID, src.Name);
                res1 = existingSource != null ? existingSource as ComPluginSource : new ComPluginSource
                {
                    ResourceID = src.Id,
                    ClsId = src.ClsId,
                    Is32Bit = src.Is32Bit,
                    ComName = src.SelectedDll.Name,
                    ResourceName = src.ResourceName
                };

                ResourceCat.SaveResource(GlobalConstants.ServerWorkspaceID, res1, src.ResourcePath);
                msg.HasError = false;
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public IResourceCatalog ResourceCat
        {
            get => _resourceCatalog ?? ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ComPluginSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "SaveComPluginSource";
    }
}