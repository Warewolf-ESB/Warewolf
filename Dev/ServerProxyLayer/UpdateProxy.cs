using System;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class UpdateProxy:ProxyBase,IUpdateManager
    {
        #region Implementation of IUpdateManager


        public UpdateProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection):base(communicationControllerFactory,connection)
        {

        }

        /// <summary>
        /// Deploy a resource. order of execution is gauranteed
        /// </summary>
        /// <param name="resource"></param>
        public void DeployItem(IResource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            var comsController = CommunicationControllerFactory.CreateController("DeployResourceService");
           //todo: this is different and need to use new method
            // comsController.AddPayloadArgument("ResourceDefinition", resource.ToServiceDefinition());
            comsController.AddPayloadArgument("Roles", "*");

            var con = Connection;
            comsController.ExecuteCommand<IExecuteMessage>(con, GlobalConstants.ServerWorkspaceID);
        }

        /// <summary>
        /// Save a resource to the server
        /// </summary>
        /// <param name="resource">resource to save</param>
        /// <param name="workspaceId">workspace to save to </param>
        public void SaveResource(StringBuilder resource,Guid workspaceId)
        {
            var con = Connection;
            var comsController = CommunicationControllerFactory.CreateController("SaveResourceService");
            comsController.AddPayloadArgument("ResourceXml", resource);
            comsController.ExecuteCommand<IExecuteMessage>(con, workspaceId);
        }

        #endregion
    }
}
