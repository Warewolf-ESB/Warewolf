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
using ServiceStack.Common;

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
            _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, resource, fixedResource,"","",newPath);

        }
        private void SaveFolders(string sourceLocation, string destination, string newName, bool fixRefences)
        {
            var resourceList = _resourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID);
            var resourceToMove = resourceList.Where(resource => resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).ToUpper().StartsWith(sourceLocation.ToUpper()))
                                                .Where(resource => !(resource is ManagementServiceResource))
                                                .ToList();
            _resourcesToUpdate.AddRange(resourceToMove);
            foreach (var resource in resourceToMove)
            {
                try
                {
                    var result = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resource.ResourceID);
                    var xElement = result.ToXElement();
                    resource.IsUpgraded = true;
                    var newResourceId = Guid.NewGuid();
                    var oldResourceId = resource.ResourceID;
                    resource.ResourceID = newResourceId;
                    var fixedResource = xElement.ToStringBuilder();
                    var savePath = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).Replace(resource.ResourceName,"").TrimEnd('\\');
                    savePath = savePath.ReplaceFirst(sourceLocation, destination + "\\" + newName);
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, resource, fixedResource,"","", savePath);
                    _resourceUpdateMap.Add(oldResourceId, newResourceId);

                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, e);
                    _resourcesToUpdate.Remove(resource);
                }
            }
            if (fixRefences)
            {

                try
                {
                    using (var tx = new TransactionScope())
                    {
                        FixReferences();
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

        private readonly List<IResource> _resourcesToUpdate = new List<IResource>();
        private readonly Dictionary<Guid, Guid> _resourceUpdateMap = new Dictionary<Guid, Guid>();
        private void FixReferences()
        {
            foreach (var updatedResource in _resourcesToUpdate)
            {

                var contents = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, updatedResource.ResourceID);

                foreach (var oldToNewUpdate in _resourceUpdateMap)
                {
                    contents.Replace(oldToNewUpdate.Key.ToString(), oldToNewUpdate.Value.ToString());
                }
                _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, updatedResource, contents);
                updatedResource.LoadDependencies(contents.ToXElement());
            }
        }
    }
}
