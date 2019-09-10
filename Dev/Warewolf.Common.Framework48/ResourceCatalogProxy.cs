

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Resources;
using Dev2.Controller;
using Dev2.Network;
using Warewolf.Service;

namespace Warewolf.Common
{
    public interface IResourceCatalogProxy
    {
        T GetResourceById<T>(Guid workspaceId, Guid resourceId) where T : class;
    }
    public class ResourceCatalogProxy : IResourceCatalogProxy
    {
        readonly ServerProxy _environmentConnection;
        public ResourceCatalogProxy(ServerProxy environmentConnection)
        {
            _environmentConnection = environmentConnection;
        }

        public T GetResourceById<T>(Guid workspaceId, Guid resourceId) where T: class
        {
            var communicationController = new CommunicationController
            {
                ServiceName = nameof(Service.GetResourceById)
            };
            communicationController.AddPayloadArgument(Service.GetResourceById.WorkspaceId, workspaceId.ToString());
            communicationController.AddPayloadArgument(Service.GetResourceById.ResourceId, resourceId.ToString());
            var result = communicationController.ExecuteCommand<T>(_environmentConnection, workspaceId);

            return result;
        }
    }
}
