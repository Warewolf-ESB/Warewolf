#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
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
using Warewolf.Resource.Errors;
using static Dev2.Common.Interfaces.WarewolfExecutionEnvironmentException;

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
            get
            {
                return _testCatalog ?? Runtime.TestCatalog.Instance;
            }
            set
            {
                _testCatalog = value;
            }
        }

        public ResourceCatalogDuplicateResult DuplicateFolder(string sourcePath, string destinationPath, string newName, bool fixRefences)
        {
            try
            {
                var items = DuplicateAndSaveFolders(sourcePath, destinationPath, newName, fixRefences);
                return new ResourceCatalogDuplicateResult
                {
                    Status = ExecStatus.Success,
                    Message = "Duplicated Successfully",
                    DuplicatedItems = items.ToList()
                };
            }
            catch(Exception x)
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
            catch(Exception x)
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
            if(xamlElement != null)
            {
                var xamlContent = xamlElement.Value;
                xamlElement.Value = xamlContent.Replace("x:Class=\"" + resource?.ResourceName + "\"", "x:Class=\"" + newResourceName + "\"")
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
            if(serviceTestModelTos != null && serviceTestModelTos.Count > 0)
            {
                foreach(var serviceTestModelTo in serviceTestModelTos)
                {
                    serviceTestModelTo.ResourceId = newResourceId;
                }

                TestCatalog.SaveTests(newResourceId, serviceTestModelTos);
            }
        }

        IEnumerable<IExplorerItem> DuplicateAndSaveFolders(string sourceLocation, string destination, string newName, bool fixRefences)
        {
            var resourcesToMove = GetResourcesToMoveFrom(sourceLocation);
            string destFolderPath = CalculateDestinationFolderPath(destination, newName, out List<IExplorerItem> items);

            try
            {
                var duplicatedResourcesMap = DuplicateAndSaveResources(sourceLocation, destFolderPath, resourcesToMove);

                DuplicateAndSaveTests(duplicatedResourcesMap);

                if(fixRefences)
                {
                    try
                    {
                        using(var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            FixReferences(duplicatedResourcesMap);
                            tx.Complete();
                        }
                    }
                    catch(TransactionAbortedException e)
                    {
                        Dev2Logger.Error("Failure Fixing references", e, GlobalConstants.WarewolfError);
                        throw new WarewolfThreadAbortException(string.Format(ErrorResource.ErrorDuringDuplicateFolderCallback, e.Message));
                    }
                }
            }
            catch(TransactionAbortedException ex)
            {
                Dev2Logger.Error(ErrorResource.ErrorDuringDuplicateFolderCallback, ex, GlobalConstants.WarewolfError);
                throw new WarewolfThreadAbortException(string.Format(ErrorResource.ErrorDuringDuplicateFolderCallback, ex.Message));
            }

            return items;
        }

        private List<DuplicateResourceTO> DuplicateAndSaveResources(string sourceLocation, string destFolderPath, List<IResource> resourcesToMove)
        {
            var resourceMaps = new List<DuplicateResourceTO>();

            foreach(var oldResource in resourcesToMove)
            {
                try
                {
                    string savePath = CalculateResourceSavePath(sourceLocation, destFolderPath, oldResource.ResourceName, oldResource);
                    var itemToAdd = GenerateItemToAdd(savePath);
                    ServerExplorerRepository.Instance.AddItem(itemToAdd, GlobalConstants.ServerWorkspaceID);

                    var result = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, oldResource.ResourceID);
                    var xElement = result.ToXElement();
                    var newResource = DuplicateResource(xElement);

                    resourceMaps.Add(
                    new DuplicateResourceTO
                    {
                        OldResourceID = oldResource.ResourceID,
                        NewResource = newResource,
                        DestinationPath = savePath,
                        ResourceContents = xElement.ToStringBuilder()
                    });
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                }
            }

            _resourceCatalog.SaveResources(GlobalConstants.ServerWorkspaceID, resourceMaps, overrideExisting: true);
            return resourceMaps;
        }

        private void DuplicateAndSaveTests(List<DuplicateResourceTO> resourcesMaps)
        {
            foreach(var mapper in resourcesMaps)
            {
                var oldResourceId = mapper.OldResourceID;
                var newResource = mapper.NewResource;
                var newResourceId = newResource.ResourceID;
                SaveTests(oldResourceId, newResourceId);
            }
        }

        private static ServerExplorerItem GenerateItemToAdd(string savePath)
        {
            var subActualPath = savePath.TrimStart('\\');
            var subTrimEnd = subActualPath.TrimStart('\\').TrimEnd('\\');
            var idx = subTrimEnd.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
            var name = subTrimEnd.Substring(idx + 1);
            var subItem = ServerExplorerRepository.Instance.Find(item => item.ResourcePath.ToLowerInvariant().TrimEnd('\\').Equals(subTrimEnd));
            return new ServerExplorerItem(name, Guid.NewGuid(), "Folder", new List<IExplorerItem>(), Permissions.Contribute, subTrimEnd)
            {
                IsFolder = true,
                Parent = subItem
            };
        }

        private Resource DuplicateResource(XElement xElement)
        {
            return new Resource(xElement)
            {
                IsUpgraded = true,
                ResourceID = Guid.NewGuid()
            };
        }

        private static string CalculateDestinationFolderPath(string destination, string newName, out List<IExplorerItem> items)
        {
            items = new List<IExplorerItem>();
            var destPath = destination + "\\" + newName;
            var actualPath = destPath.TrimStart('\\');
            var trimEnd = destination.TrimStart('\\').TrimEnd('\\');
            var parentItem = ServerExplorerRepository.Instance.Find(item => item.ResourcePath.ToLowerInvariant().TrimEnd('\\').Equals(trimEnd));
            if(parentItem == null)
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

            return destPath;
        }

        private List<IResource> GetResourcesToMoveFrom(string sourceLocation)
        {
            var resourceList = _resourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID);
            return resourceList.Where(
                resource =>
                {
                    var upper = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).ToUpper();
                    return upper.StartsWith(sourceLocation.ToUpper());
                }).Where(resource => !(resource is ManagementServiceResource)).ToList();
        }

        public static string CalculateResourceSavePath(string sourceLocation, string destPath, string resourceName, IResource resource)
        {
            var resourcePath = resource.GetResourcePath(GlobalConstants.ServerWorkspaceID);

            var savePath = resourcePath;
            var resourceNameIndex = resourcePath.LastIndexOf(resourceName, StringComparison.InvariantCultureIgnoreCase);
            if(resourceNameIndex >= 0)
            {
                savePath = resourcePath.Substring(0, resourceNameIndex);
            }

            savePath = savePath.ReplaceFirst(sourceLocation, destPath);
            return savePath;
        }

        void FixReferences(List<DuplicateResourceTO> resourceMaps)
        {
            foreach(var mapper in resourceMaps)
            {
                var newResource = mapper.NewResource;
                var oldResourceID = newResource.ResourceID.ToString();
                var newResourceID = mapper.OldResourceID.ToString();

                var contents = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, newResource.ResourceID);
                contents = contents.Replace(oldResourceID.ToLowerInvariant(), newResourceID.ToLowerInvariant());

                var resPath = newResource.GetResourcePath(GlobalConstants.ServerWorkspaceID);

                var savePath = resPath;
                var resourceNameIndex = resPath.LastIndexOf(newResource.ResourceName, StringComparison.InvariantCultureIgnoreCase);
                if(resourceNameIndex >= 0)
                {
                    savePath = resPath.Substring(0, resourceNameIndex);
                }

                newResource.LoadDependencies(contents.ToXElement());
            }

            _resourceCatalog.SaveResources(GlobalConstants.ServerWorkspaceID, resourceMaps, overrideExisting: true);
        }
    }
}