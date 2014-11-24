
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

        #region Implementation of ISpookyLoadable<string>

        public string HandlesType()
        {
            return "DeleteVersion";
        }

        #endregion

        #region Implementation of IEsbManagementEndpoint

        /// <summary>
        /// Executes the service
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="theWorkspace">The workspace.</param>
        /// <returns></returns>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var execMessage = new ExecuteMessage { HasError = false };
            if(!values.ContainsKey("resourceId"))
            {
                Dev2Logger.Log.Info("Delete Version. Invalid Resource Id");
                execMessage.HasError = true;
                execMessage.Message = new StringBuilder( "No resourceId sent to server");
            }
            else if(!values.ContainsKey("versionNumber") )
            {
                Dev2Logger.Log.Info("Delete Version. Invalid Version number");
                execMessage.HasError = true;
                execMessage.Message = new StringBuilder("No versionNumber sent to server");
            }
            else
            {
                try
                {
                    var guid = Guid.Parse(values["resourceId"].ToString());
                    var version = values["versionNumber"].ToString();
                    Dev2Logger.Log.Info(String.Format("Delete Version. ResourceId:{0} VersionNumber{1}",guid,version));
                    var res = ServerVersionRepo.DeleteVersion(guid,version);
                    execMessage.Message = serializer.SerializeToBuilder(res); 
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(String.Format("Delete Version Error."),e);
                    execMessage.HasError = true;
                    execMessage.Message = new StringBuilder( e.Message);
                }
            }
            return serializer.SerializeToBuilder(execMessage);
        }


        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);
            return newDs;
        }

        public IServerVersionRepository ServerVersionRepo
        {
            get { return _serverExplorerRepository ?? new ServerVersionRepository(new VersionStrategy(), ResourceCatalog.Instance, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper()); }
            set { _serverExplorerRepository = value; }
        }

        #endregion
    }
}
