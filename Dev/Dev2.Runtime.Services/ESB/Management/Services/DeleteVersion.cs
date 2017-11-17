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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class DeleteVersion : IEsbManagementEndpoint
    {
        IServerVersionRepository _serverExplorerRepository;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("resourceId", out StringBuilder tmp);
            if (tmp != null)
            {
                if (Guid.TryParse(tmp.ToString(), out Guid resourceId))
                {
                    return resourceId;
                }
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }

        #region Implementation of IEsbManagementEndpoint

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var execMessage = new ExecuteMessage { HasError = false };
            if(!values.ContainsKey("resourceId"))
            {
                Dev2Logger.Info("Delete Version. Invalid Resource Id", GlobalConstants.WarewolfInfo);
                execMessage.HasError = true;
                execMessage.Message = new StringBuilder( "No resourceId sent to server");
            }
            else if(!values.ContainsKey("versionNumber") )
            {
                Dev2Logger.Info("Delete Version. Invalid Version number", GlobalConstants.WarewolfInfo);
                execMessage.HasError = true;
                execMessage.Message = new StringBuilder("No versionNumber sent to server");
            }
            else
            {
                try
                {
                    var guid = Guid.Parse(values["resourceId"].ToString());
                    var version = values["versionNumber"].ToString();
                    Dev2Logger.Info($"Delete Version. ResourceId:{guid} VersionNumber{version}", GlobalConstants.WarewolfInfo);
                    string resourcePath = "";
                    values.TryGetValue("resourcePath", out StringBuilder tmp);
                    if (tmp != null)
                    {
                        resourcePath = tmp.ToString();
                    }
                    var res = ServerVersionRepo.DeleteVersion(guid,version, resourcePath);
                    execMessage.Message = serializer.SerializeToBuilder(res); 
                }
                catch (Exception e)
                {
                    Dev2Logger.Error("Delete Version Error.",e, GlobalConstants.WarewolfError);
                    execMessage.HasError = true;
                    execMessage.Message = new StringBuilder( e.Message);
                }
            }
            return serializer.SerializeToBuilder(execMessage);
        }

        public IServerVersionRepository ServerVersionRepo
        {
            get { return _serverExplorerRepository ?? new ServerVersionRepository(new VersionStrategy(), ResourceCatalog.Instance, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper()); }
            set { _serverExplorerRepository = value; }
        }

        #endregion

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "DeleteVersion";
    }
}
