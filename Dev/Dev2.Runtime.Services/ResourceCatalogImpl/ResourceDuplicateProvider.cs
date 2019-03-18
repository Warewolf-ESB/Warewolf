#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Security;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common;




namespace Dev2.Runtime.ResourceCatalogImpl
{
    class ResourceDuplicateProvider : IResourceDuplicateProvider
    {
        readonly IResourceCatalog _resourceCatalog;
        ITestCatalog _testCatalog;

        public ResourceDuplicateProvider(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public ITestCatalog TestCatalog
        {
            get { return _testCatalog ?? Runtime.TestCatalog.Instance; }
            set { _testCatalog = value; }
        }

        public ResourceCatalogDuplicateResult DuplicateFolder(string sourcePath, string destinationPath, string newName, bool fixRefences)
        {
            try
            {

                var items = SaveFolders(sourcePath, destinationPath, newName, fixRefences);
                return new ResourceCatalogDuplicateResult
                {
                    Status = ExecStatus.Success,
                    Message = "Duplicated Successfully",
                    DuplicatedItems = items.ToList()
                };
            }
            catch (Exception x)
            {
                Dev2Logger.Error($"resource{sourcePath} ", x, GlobalConstants.WarewolfError);
                return new ResourceCatalogDuplicateResult
                {
                    Status = ExecStatus.Fail,
                    Message = "Duplicated Unsuccessfully" + x.Message
                };
            }
        }

        public ResourceCatalogDuplicateResult DuplicateResource(Guid resourceId, string sourcePath, string destinationPath)
        {

            try
            {
                var item = SaveResource(resourceId, sourcePath, destinationPath);
                return new ResourceCatalogDuplicateResult
                {
                    Status = ExecStatus.Success,
                    Message = "Duplicated Successfully",
                    DuplicatedItems = new List<IExplorerItem> { item }
                };
            }
            catch (Exception x)
            {
                Dev2Logger.Error($"resource{resourceId} ", x, GlobalConstants.WarewolfError);
                return null;
            }
        }
        IExplorerItem SaveResource(Guid resourceId, string newPath, string newResourceName)
        {
            var result = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
            var xElement = result.ToXElement();
            var newResource = new Resource(xElement)
            {
                IsUpgraded = true
            };
            var resource = _resourceCatalog.GetResources(GlobalConstants.ServerWorkspaceID)
                .FirstOrDefault(p => p.ResourceID == resourceId);
            var actionElement = xElement.Element("Action");
            var xamlElement = actionElement?.Element("XamlDefinition");
            xElement.SetAttributeValue("Name", newResourceName);
            var resourceID = Guid.NewGuid();
            newResource.ResourceName = newResourceName;
            newResource.ResourceID = resourceID;
            xElement.SetElementValue("DisplayName", newResourceName);
            if (xamlElement != null)
            {
                var xamlContent = xamlElement.Value;
                xamlElement.Value = xamlContent.
                        Replace("x:Class=\"" + resource?.ResourceName + "\"", "x:Class=\"" + newResourceName + "\"")
                        .Replace("Flowchart DisplayName=\"" + resource?.ResourceName + "\"", "Flowchart DisplayName=\"" + newResourceName + "\"");
            }
            var fixedResource = xElement.ToStringBuilder();
            _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, newResource, fixedResource, newPath);
            SaveTests(resourceId, resourceID);
            return ServerExplorerRepository.Instance.UpdateItem(newResource);
        }

        void SaveTests(Guid oldResourceId, Guid newResourceId)
        {
            var serviceTestModelTos = _testCatalog?.Fetch(oldResourceId);
            if (serviceTestModelTos != null && serviceTestModelTos.Count > 0)
            {
                foreach (var serviceTestModelTo in serviceTestModelTos)
                {
                    serviceTestModelTo.ResourceId = newResourceId;
                }
                TestCatalog.SaveTests(newResourceId, serviceTestModelTos);
            }
        }

        IEnumerable<IExplorerItem> SaveFolders(string sourceLocation, string destination, string newName, bool fixRefences)
        {
            var resourcesToUpdate = new List<IResource>();
            var resourceUpdateMap = new Dictionary<Guid, Guid>();
            var resourceList = _resourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID);
            var items = new List<IExplorerItem>();
            var resourceToMove = resourceList.Where(resource =>
            {
                var upper = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).ToUpper();
                return upper.StartsWith(sourceLocation.ToUpper());
            }).Where(resource => !(resource is ManagementServiceResource)).ToList();
            var destPath = destination + "\\" + newName;
            var actualPath = destPath.TrimStart('\\');
            var trimEnd = destination.TrimStart('\\').TrimEnd('\\');
            var parentItem = ServerExplorerRepository.Instance.Find(item => item.ResourcePath.ToLowerInvariant().TrimEnd('\\').Equals(trimEnd));
            if (parentItem == null)
            {
                var itemToAdd = new ServerExplorerItem(newName, Guid.NewGuid(), "Folder", new List<IExplorerItem>(), Permissions.Contribute, actualPath) { IsFolder = true };
                ServerExplorerRepository.Instance.AddItem(itemToAdd, GlobalConstants.ServerWorkspaceID);
                items.Add(itemToAdd);
            }
            else
            {
                var itemToAdd = new ServerExplorerItem(newName, Guid.NewGuid(), "Folder", new List<IExplorerItem>(), Permissions.Contribute, actualPath)
                {
                    IsFolder = true,
                    Parent = parentItem
                };
                parentItem.Children.Add(itemToAdd);
                items.Add(itemToAdd);
            }

            foreach (var resource in resourceToMove)
            {
                try
                {
                    var result = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resource.ResourceID);
                    var xElement = result.ToXElement();
                    var newResource = new Resource(xElement)
                    {
                        IsUpgraded = true
                    };
                    var newResourceId = Guid.NewGuid();
                    var oldResourceId = resource.ResourceID;
                    newResource.ResourceID = newResourceId;
                    var fixedResource = xElement.ToStringBuilder();

                    var resourcePath = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID);

                    var savePath = resourcePath;
                    var resourceNameIndex = resourcePath.LastIndexOf(resource.ResourceName, StringComparison.InvariantCultureIgnoreCase);
                    if (resourceNameIndex >= 0)
                    {
                        savePath = resourcePath.Substring(0, resourceNameIndex);
                    }
                    savePath = savePath.ReplaceFirst(sourceLocation, destPath);
                    var subActualPath = savePath.TrimStart('\\');
                    var subTrimEnd = subActualPath.TrimStart('\\').TrimEnd('\\');
                    var idx = subTrimEnd.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
                    var name = subTrimEnd.Substring(idx + 1);
                    var subItem = ServerExplorerRepository.Instance.Find(item => item.ResourcePath.ToLowerInvariant().TrimEnd('\\').Equals(subTrimEnd));
                    var itemToAdd = new ServerExplorerItem(name, Guid.NewGuid(), "Folder", new List<IExplorerItem>(), Permissions.Contribute, subTrimEnd)
                    {
                        IsFolder = true,
                        Parent = subItem
                    };
                    ServerExplorerRepository.Instance.AddItem(itemToAdd, GlobalConstants.ServerWorkspaceID);
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, newResource, fixedResource, savePath);
                    resourcesToUpdate.Add(newResource);
                    resourceUpdateMap.Add(oldResourceId, newResourceId);
                    SaveTests(oldResourceId, newResourceId);
                    ServerExplorerRepository.Instance.UpdateItem(newResource);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
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
            return items;
        }

        void FixReferences(List<IResource> resourcesToUpdate, Dictionary<Guid, Guid> oldToNewUpdates)
        {
            foreach (var updatedResource in resourcesToUpdate)
            {

                var contents = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, updatedResource.ResourceID);


                foreach (var oldToNewUpdate in oldToNewUpdates)
                {
                    contents = contents.Replace(oldToNewUpdate.Key.ToString().ToLowerInvariant(), oldToNewUpdate.Value.ToString().ToLowerInvariant());
                }
                var resPath = updatedResource.GetResourcePath(GlobalConstants.ServerWorkspaceID);

                var savePath = resPath;
                var resourceNameIndex = resPath.LastIndexOf(updatedResource.ResourceName, StringComparison.InvariantCultureIgnoreCase);
                if (resourceNameIndex >= 0)
                {
                    savePath = resPath.Substring(0, resourceNameIndex);
                }

                _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, updatedResource, contents, savePath);
                updatedResource.LoadDependencies(contents.ToXElement());
            }

        }
    }
}
