

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Resources;
using Dev2.Controller;
using Dev2.Network;

namespace Warewolf.Common
{
    public interface IResourceCatalogProxy
    {
        T GetResourceById<T>(Guid workspaceId, Guid resourceId) where T : class;
    }
    public class ResourceCatalogProxy : IResourceCatalogProxy
    {
        ServerProxy _environmentConnection;
        public ResourceCatalogProxy(ServerProxy environmentConnection)
        {
            _environmentConnection = environmentConnection;
        }

        public T GetResourceById<T>(Guid workspaceId, Guid resourceId) where T: class
        {
            var communicationController = new CommunicationController
            {
                ServiceName = @"GetResourceById"
            };
            communicationController.AddPayloadArgument("WorkspaceId", workspaceId.ToString());
            communicationController.AddPayloadArgument("ResourceId", resourceId.ToString());
            var result = communicationController.ExecuteCommand<T>(_environmentConnection, workspaceId);

            return result;
        }

        //public IResource GetResourceById(Guid resourceId, Guid workspaceId)
        //{
        //    var communicationController = new CommunicationController
        //    {
        //        ServiceName = @"FetchResourceDefinitionService"
        //    };
        //    communicationController.AddPayloadArgument("ResourceID", resourceId.ToString());
        //    var result =  communicationController.ExecuteCommand<ExecuteMessage> (_environmentConnection, workspaceId);

        //    var xml = XElement.Parse(result.Message.ToString());
        //    var r = new Dev2.Runtime.ServiceModel.Data.Resource(xml);


        //    return null;
        //}
    }
}
