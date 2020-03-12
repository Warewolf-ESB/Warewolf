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
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{

    public class SaveWebserviceSource : EsbManagementEndpointBase
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
        
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();
            try
            {
                Dev2Logger.Info("Save Webservice Source", GlobalConstants.WarewolfInfo);

                values.TryGetValue("WebserviceSource", out StringBuilder resourceDefinition);

                var src = serializer.Deserialize<WebServiceSourceDefinition>(resourceDefinition);
                if(src.Path.EndsWith("\\"))
                {
                    src.Path = src.Path.Substring(0, src.Path.LastIndexOf("\\", StringComparison.Ordinal));
                }

                var res = new WebSource
                {
                    AuthenticationType = src.AuthenticationType,
                    Address = src.HostName,
                    Password = src.Password,
                    UserName = src.UserName,
                    ResourceID = src.Id,
                    DefaultQuery = src.DefaultQuery,
                    ResourceName = src.Name
                };
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, res, src.Path);
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

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><WebserviceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "SaveWebserviceSource";
    }
}
