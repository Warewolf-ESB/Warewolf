using System;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerDialogue;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Studio.ServerProxyLayer;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class StudioResourceUpdateManager : IStudioUpdateManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StudioResourceUpdateManager(ICommunicationControllerFactory controllerFactory, IEnvironmentConnection environmentConnection)
        {
            if (controllerFactory == null)
            {
                throw new ArgumentNullException("controllerFactory");
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException("environmentConnection");
            }

             UpdateManagerProxy = new UpdateProxy(controllerFactory, environmentConnection);

        }

        IUpdateManager UpdateManagerProxy { get; set; }

        public void Save(IServerSource serverSource)
        {
            Connection resource = new Connection
            {
                ResourceID = serverSource.ID,
                ResourceName = serverSource.Name,
                ResourceType = ResourceType.ServerSource,
                ResourcePath = serverSource.ResourcePath,
                Address = serverSource.Address,
                AuthenticationType = serverSource.AuthenticationType,
                UserName = serverSource.UserName,
                Password = serverSource.Password


            };
            UpdateManagerProxy.SaveResource(resource,GlobalConstants.ServerWorkspaceID);
        }
    }
}