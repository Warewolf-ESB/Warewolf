using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Services;
using Microsoft.AspNet.SignalR.Client;

namespace Dev2.Network
{
    public class ResourceServiceProxy:IResourceService
    {
        public IHubProxy ResourceHubProxy { get; private set; }

        #region Implementation of IResourceService

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ResourceServiceProxy(IHubProxy resourceHubProxy)
        {
            ResourceHubProxy = resourceHubProxy;
        }

        public Task<ExecuteMessage> DeleteResource(Guid workspaceID, string resourceName, string type)
        {
            return ResourceHubProxy.Invoke<ExecuteMessage>("DeleteResource", workspaceID, resourceName, type);
        }

        public Task<ExecuteMessage> FindDependencies(Guid workspaceID, string resourceName, bool dependsOnMe)
        {
            throw new NotImplementedException();
        }

        public Task<ExecuteMessage> FetchResourceDefinition(Guid workspaceID, Guid resourceID)
        {
            throw new NotImplementedException();
        }

        public Task<IList<SerializableResource>> FindResource(Guid workspaceID, string resourceName, string resourceType)
        {
            throw new NotImplementedException();
        }

        public Task<IList<SerializableResource>> FindResourcesByID(Guid workspaceID, string guidCsv, string resourceType)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable> FindSourcesByType(Guid workspaceID, string typeOf)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetDependanciesOnList(Guid workspaceID, List<string> resourceNames, bool dependsOnMe)
        {
            throw new NotImplementedException();
        }

        public Task<ExecuteMessage> ReloadResource(Guid workspaceID, string resourceID, string resourceType)
        {
            throw new NotImplementedException();
        }

        public Task<ExecuteMessage> RenameResourceCategory(Guid workspaceID, string oldCategory, string newCategory, string resourceType)
        {
            throw new NotImplementedException();
        }

        public Task<ExecuteMessage> RenameResource(Guid workspaceID, Guid resourceID, string newName)
        {
            throw new NotImplementedException();
        }

        public Task<ExecuteMessage> SaveResource(Guid workspaceID, string resourceXml)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
