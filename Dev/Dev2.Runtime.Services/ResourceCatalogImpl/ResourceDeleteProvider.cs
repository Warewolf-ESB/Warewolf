using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using ServiceStack.Common.Extensions;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ResourceCatalogImpl
{
    internal class ResourceDeleteProvider : IResourceDeleteProvider
    {
        private readonly FileWrapper _dev2FileWrapper = new FileWrapper();
        readonly IServerVersionRepository _serverVersionRepository;
        private readonly IResourceCatalog _resourceCatalog;
       

        public ResourceDeleteProvider(IResourceCatalog resourceCatalog, IServerVersionRepository serverVersionRepository)
        {
            _resourceCatalog = resourceCatalog;
            _serverVersionRepository = serverVersionRepository;
        }

        #region Implementation of IResourceDeleteProvider
        public ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type, bool deleteVersions = true)
        {
            var @lock = Common.GetWorkspaceLock(workspaceID);
            lock (@lock)
            {
                if (resourceName == "*")
                {
                    var noWildcardsAllowedhResult = ResourceCatalogResultBuilder.CreateNoWildcardsAllowedhResult("<Result>Delete resources does not accept wildcards.</Result>.");
                    return noWildcardsAllowedhResult;
                }

                if (string.IsNullOrEmpty(resourceName) || string.IsNullOrEmpty(type))
                {
                    throw new InvalidDataContractException(ErrorResource.ResourceNameAndTypeMissing);
                }

                var workspaceResources = _resourceCatalog.GetResources(workspaceID);
                var resources = _resourceCatalog.GetResourcesBasedOnType(type, workspaceResources, r => string.Equals(r.ResourceName, resourceName, StringComparison.InvariantCultureIgnoreCase));
                Dictionary<int, ResourceCatalogResult> commands = new Dictionary<int, ResourceCatalogResult>()
                {
                    {
                     0,ResourceCatalogResultBuilder.CreateNoMatchResult($"<Result>{type} '{resourceName}' was not found.</Result>")

                    },
                    {
                        1, DeleteImpl(workspaceID, resources, workspaceResources, deleteVersions)
                    },
                 };
                if (commands.ContainsKey(resources.Count))
                {
                    var resourceCatalogResult = commands[resources.Count];
                    return resourceCatalogResult;
                }

                return ResourceCatalogResultBuilder.CreateDuplicateMatchResult($"<Result>Multiple matches found for {type} '{resourceName}'.</Result>");
            }
        }

        public ResourceCatalogResult DeleteResource(Guid workspaceID, Guid resourceID, string type, bool deleteVersions = true)
        {
            try
            {
                if (workspaceID != GlobalConstants.ServerWorkspaceID)
                {
                    return DeleteFromWorkspace(workspaceID, resourceID, type, deleteVersions);
                }
                foreach(var wid in _resourceCatalog.WorkspaceResources.Keys)
                {
                    var result = DeleteFromWorkspace(wid, resourceID, type, deleteVersions);                    
                    if(wid==GlobalConstants.ServerWorkspaceID && result.Status != ExecStatus.Success)
                    {
                        return result;
                    }
                }
                return ResourceCatalogResultBuilder.CreateSuccessResult("Success");
            }
            catch (Exception err)
            {
                Dev2Logger.Error("Delete Error", err);
                throw;
            }
        
        }

        private ResourceCatalogResult DeleteFromWorkspace(Guid workspaceID, Guid resourceID, string type, bool deleteVersions)
        {
            var @lock = Common.GetWorkspaceLock(workspaceID);
            lock(@lock)
            {
                if(resourceID == Guid.Empty || string.IsNullOrEmpty(type))
                {
                    throw new InvalidDataContractException(ErrorResource.ResourceNameAndTypeMissing);
                }

                var workspaceResources = _resourceCatalog.GetResources(workspaceID);
                var resources = workspaceResources.FindAll(r => Equals(r.ResourceID, resourceID));

                var commands = GetDeleteCommands(workspaceID, resourceID, type, deleteVersions, resources, workspaceResources);
                if(commands.ContainsKey(resources.Count))
                {
                    var resourceCatalogResult = commands[resources.Count];
                    return resourceCatalogResult;
                }
                return ResourceCatalogResultBuilder.CreateDuplicateMatchResult($"<Result>Multiple matches found for {type} '{resourceID}'.</Result>");
            }
        }

        #endregion

        #region Private functions
        private ResourceCatalogResult DeleteImpl(Guid workspaceID, IEnumerable<IResource> resources, List<IResource> workspaceResources, bool deleteVersions = true)
        {

            IResource resource = resources.FirstOrDefault();

            if (workspaceID == Guid.Empty && deleteVersions)
                if (resource != null)
                {
                    var explorerItems = _serverVersionRepository.GetVersions(resource.ResourceID);
                    explorerItems?.ForEach(a => _serverVersionRepository.DeleteVersion(resource.ResourceID, a.VersionInfo.VersionNumber, resource.GetResourcePath(workspaceID)));
                }

            workspaceResources.Remove(resource);
            if (resource != null && _dev2FileWrapper.Exists(resource.FilePath))
            {
                _dev2FileWrapper.Delete(resource.FilePath);
            }
            if (resource != null)
            {
                var messages = new List<ICompileMessageTO>
                {
                    new CompileMessageTO
                    {
                        ErrorType = ErrorType.Critical,
                        MessageID = Guid.NewGuid(),
                        MessagePayload = "The resource has been deleted",
                        MessageType = CompileMessageType.ResourceDeleted,
                        ServiceID = resource.ResourceID
                    }
                };
                UpdateDependantResourceWithCompileMessages(workspaceID, resource, messages);
            }
            if (workspaceID == GlobalConstants.ServerWorkspaceID)
            {
                if (resource != null)
                {
                    ServiceActionRepo.Instance.RemoveFromCache(resource.ResourceID);
                    ServerAuthorizationService.Instance.Remove(resource.ResourceID);
                }
            }
            
            ((ResourceCatalog)_resourceCatalog).RemoveFromResourceActivityCache(workspaceID, resource);
            return ResourceCatalogResultBuilder.CreateSuccessResult("Success");
        }
        void UpdateDependantResourceWithCompileMessages(Guid workspaceID, IResource resource, IList<ICompileMessageTO> messages)
        {
            var resourceId = resource.ResourceID;
            var dependants = _resourceCatalog.GetDependentsAsResourceForTrees(workspaceID, resourceId);
            var dependsMessageList = new List<ICompileMessageTO>();
            foreach (var dependant in dependants)
            {
                var affectedResource = _resourceCatalog.GetResource(workspaceID, dependant.ResourceID);
                foreach (var compileMessageTO in messages)
                {
                    compileMessageTO.WorkspaceID = workspaceID;
                    compileMessageTO.UniqueID = dependant.UniqueID;
                    if (affectedResource != null)
                    {
                        compileMessageTO.ServiceName = affectedResource.ResourceName;
                        compileMessageTO.ServiceID = affectedResource.ResourceID;
                    }
                    dependsMessageList.Add(compileMessageTO.Clone());
                }
                if (affectedResource != null)
                {
                    Common.UpdateResourceXml(_resourceCatalog,workspaceID, affectedResource, messages);
                }
            }
            CompileMessageRepo.Instance.AddMessage(workspaceID, dependsMessageList);
        }


        private Dictionary<int, ResourceCatalogResult> GetDeleteCommands(Guid workspaceID, Guid resourceID, string type, bool deleteVersions, IEnumerable<IResource> resources, List<IResource> workspaceResources)
        {
            Dictionary<int, ResourceCatalogResult> commands = new Dictionary<int, ResourceCatalogResult>()
            {
                {
                    0,
                    ResourceCatalogResultBuilder.CreateNoMatchResult($"<Result>{type} '{resourceID}' was not found.</Result>")
                },
                { 1, DeleteImpl(workspaceID, resources, workspaceResources, deleteVersions) },
            };
            return commands;
        }

        #endregion
        
    }
}