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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveOAuthSource : EsbManagementEndpointBase
    {
        IExplorerServerResourceRepository _serverExplorerRepository;

        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        #region Implementation of IEsbManagementEndpoint

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save OAuth Source", GlobalConstants.WarewolfInfo);

                values.TryGetValue("OAuthSource", out StringBuilder resourceDefinition);

                values.TryGetValue("savePath", out StringBuilder savePath);

                var src = serializer.Deserialize<IOAuthSource>(resourceDefinition);

                IResource res = null;

                if (src.GetType().Name == "DropBoxSource")
                {
                    res = new DropBoxSource
                    {
                        ResourceID = src.ResourceID,
                        AppKey = src.AppKey,
                        AccessToken = src.AccessToken,
                        ResourceName = src.ResourceName
                    };
                }

                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res, savePath?.ToString());
                ServerExplorerRepo.UpdateItem(res);
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

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get => _serverExplorerRepository ?? ServerExplorerRepository.Instance;
            set => _serverExplorerRepository = value;
        }

        #endregion

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><PluginSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "SaveOAuthSource";
    }
}
