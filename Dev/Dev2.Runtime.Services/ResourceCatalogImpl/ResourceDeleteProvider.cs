#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
    class ResourceDeleteProvider : IResourceDeleteProvider
    {
        readonly FileWrapper _dev2FileWrapper = new FileWrapper();
        readonly IServerVersionRepository _serverVersionRepository;
        readonly IResourceCatalog _resourceCatalog;


        public ResourceDeleteProvider(IResourceCatalog resourceCatalog, IServerVersionRepository serverVersionRepository)
        {
            _resourceCatalog = resourceCatalog;
            _serverVersionRepository = serverVersionRepository;
        }

        #region Implementation of IResourceDeleteProvider

        public ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type) => DeleteResource(workspaceID, resourceName, type, true);

        public ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type, bool deleteVersions)
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
                var commands = new Dictionary<int, ResourceCatalogResult>()
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

        public ResourceCatalogResult DeleteResource(Guid workspaceID, Guid resourceID, string type) => DeleteResource(workspaceID, resourceID, type, true);

        public ResourceCatalogResult DeleteResource(Guid workspaceID, Guid resourceID, string type, bool deleteVersions)
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
                Dev2Logger.Error("Delete Error", err, GlobalConstants.WarewolfError);
                throw;
            }
        
        }

        ResourceCatalogResult DeleteFromWorkspace(Guid workspaceID, Guid resourceID, string type, bool deleteVersions)
        {
            var @lock = Common.GetWorkspaceLock(workspaceID);
            lock (@lock)
            {
                if (resourceID == Guid.Empty || string.IsNullOrEmpty(type))
                {
                    throw new InvalidDataContractException(ErrorResource.ResourceNameAndTypeMissing);
                }

                var workspaceResources = _resourceCatalog.GetResources(workspaceID);
                var resources = workspaceResources.FindAll(r => Equals(r.ResourceID, resourceID));

                var commands = GetDeleteCommands(workspaceID, resourceID, type, deleteVersions, resources, workspaceResources);
                if (commands.ContainsKey(resources.Count))
                {
                    var resourceCatalogResult = commands[resources.Count];
                    return resourceCatalogResult;
                }
                return ResourceCatalogResultBuilder.CreateDuplicateMatchResult($"<Result>Multiple matches found for {type} '{resourceID}'.</Result>");
            }
        }

        #endregion

        #region Private functions
        ResourceCatalogResult DeleteImpl(Guid workspaceID, IEnumerable<IResource> resources, List<IResource> workspaceResources, bool deleteVersions = true)
        {

            var resource = resources.FirstOrDefault();

            if (workspaceID == Guid.Empty && deleteVersions && resource != null)
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
            if (workspaceID == GlobalConstants.ServerWorkspaceID && resource != null)
            {
                ServiceActionRepo.Instance.RemoveFromCache(resource.ResourceID);
                ServerAuthorizationService.Instance.Remove(resource.ResourceID);
            }
            
            _resourceCatalog.RemoveFromResourceActivityCache(workspaceID, resource);
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


        Dictionary<int, ResourceCatalogResult> GetDeleteCommands(Guid workspaceID, Guid resourceID, string type, bool deleteVersions, IEnumerable<IResource> resources, List<IResource> workspaceResources)
        {
            var commands = new Dictionary<int, ResourceCatalogResult>()
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