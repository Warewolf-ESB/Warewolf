using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common;
// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace Dev2.Runtime.ResourceCatalogImpl
{
    internal class ResourceDuplicateProvider : IResourceDuplicateProvider
    {
        private readonly IResourceCatalog _resourceCatalog;

        public ResourceDuplicateProvider(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public ResourceCatalogResult DuplicateFolder(string sourcePath, string destinationPath, string newName, bool fixRefences)
        {
            try
            {
               
                SaveFolders(sourcePath, destinationPath, newName, fixRefences);
                return ResourceCatalogResultBuilder.CreateSuccessResult("Duplicated Successfully");
            }
            catch (Exception x)
            {
                Dev2Logger.Error($"resource{sourcePath} ", x);
                return ResourceCatalogResultBuilder.CreateFailResult("Duplicated Unsuccessfully" + x.Message);
            }
        }

        public ResourceCatalogResult DuplicateResource(Guid resourceId, string newPath, string newName)
        {

            try
            {
                SaveResource(resourceId, newPath, newName);
                return ResourceCatalogResultBuilder.CreateSuccessResult("Duplicated Successfully");
            }
            catch (Exception x)
            {
                Dev2Logger.Error($"resource{resourceId} ", x);
                return ResourceCatalogResultBuilder.CreateFailResult("Duplicated Failure " + x.Message);
            }
        }
        private void SaveResource(Guid resourceId, string newPath, string newResourceName)
        {

            StringBuilder result = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
            var resource = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
            var xElement = result.ToXElement();
            resource.IsUpgraded = true;
            var resourceID = Guid.NewGuid();
            resource.ResourceName = newResourceName;
            resource.ResourceID = resourceID;
            xElement.SetElementValue("DisplayName", newResourceName);
            var fixedResource = xElement.ToStringBuilder();
            _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, resource, fixedResource,newPath);

        }
        private void SaveFolders(string sourceLocation, string destination, string newName, bool fixRefences)
        {
            var resourcesToUpdate = new List<IResource>();
            var resourceUpdateMap = new Dictionary<Guid, Guid>();
            var resourceList = _resourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID);
            var resourceToMove = resourceList.Where(resource =>
            {
                var upper = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).ToUpper();
                return upper.StartsWith(sourceLocation.ToUpper());
            }).Where(resource => !(resource is ManagementServiceResource)).ToList();
            
            foreach (var resource in resourceToMove)
            {
                try
                {
                    var result = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resource.ResourceID);
                    var xElement = result.ToXElement();
                    var newResource = new Resource(xElement) { IsUpgraded = true };
                    var newResourceId = Guid.NewGuid();
                    var oldResourceId = resource.ResourceID;
                    newResource.ResourceID = newResourceId;
                    var fixedResource = xElement.ToStringBuilder();
                    
                    var resourcePath = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID);

                    var savePath = resourcePath;
                    var resourceNameIndex = resourcePath.LastIndexOf(resource.ResourceName,StringComparison.InvariantCultureIgnoreCase);
                    if (resourceNameIndex >= 0)
                    {
                        savePath = resourcePath.Substring(0, resourceNameIndex);
                    }
                    savePath = savePath.ReplaceFirst(sourceLocation, destination + "\\" + newName);
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, newResource, fixedResource, savePath);
                    resourcesToUpdate.Add(newResource);
                    resourceUpdateMap.Add(oldResourceId, newResourceId);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, e);
                }
            }
            if (fixRefences)
            {

                try
                {
                    using (var tx = new TransactionScope())
                    {
                        FixReferences(resourcesToUpdate, resourceUpdateMap);
                        tx.Complete();
                    }

                }
                catch (Exception e)
                {
                    Transaction.Current.Rollback();
                    throw new Exception("Failure Fixing references", e);
                }
            }
        }

        private void FixReferences(List<IResource> resourcesToUpdate, Dictionary<Guid, Guid> oldToNewUpdates)
        {
            foreach (var updatedResource in resourcesToUpdate)
            {

                var contents = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, updatedResource.ResourceID);

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var oldToNewUpdate in oldToNewUpdates)
                {
                    contents = contents.Replace(oldToNewUpdate.Key.ToString().ToLowerInvariant(), oldToNewUpdate.Value.ToString().ToLowerInvariant());
                }
                _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, updatedResource, contents,updatedResource.GetResourcePath(GlobalConstants.ServerWorkspaceID));
                updatedResource.LoadDependencies(contents.ToXElement());
            }
        }
    }
}
