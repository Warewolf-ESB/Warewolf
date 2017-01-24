/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Runtime;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using ServiceStack.Common.Extensions;
using Warewolf.Resource.Errors;
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local

// ReSharper disable ConvertToAutoProperty
// ReSharper disable MemberCanBePrivate.Global
namespace Dev2.Runtime.Hosting
{
    public class ServerExplorerRepository : IExplorerServerResourceRepository
    {
        IExplorerRepositorySync _sync;
        readonly IFile _file;
        public static IExplorerServerResourceRepository Instance { get; private set; }

        bool _isDirty;
        private IExplorerItem _root;

        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            private set
            {
                _isDirty = value;
            }
        }

        static ServerExplorerRepository()
        {
            Instance = new ServerExplorerRepository
            {
                ResourceCatalogue = ResourceCatalog.Instance,
                TestCatalog = Runtime.TestCatalog.Instance,
                ExplorerItemFactory = new ExplorerItemFactory(ResourceCatalog.Instance, new DirectoryWrapper(), ServerAuthorizationService.Instance),
                Directory = new DirectoryWrapper(),
                VersionRepository = new ServerVersionRepository(new VersionStrategy(), ResourceCatalog.Instance, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper())

            };

        }


        internal ServerExplorerRepository() { _file = new FileWrapper(); }

        public ServerExplorerRepository(IResourceCatalog resourceCatalog, IExplorerItemFactory explorerItemFactory, IDirectory directory, IExplorerRepositorySync sync, IServerVersionRepository versionRepository, IFile file, ITestCatalog testCatalog)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>
                {
                    { "resourceCatalog", resourceCatalog },
                    { "explorerItemFactory", explorerItemFactory },
                    { "directory", directory },
                    { nameof(testCatalog) , testCatalog }
                });
            _sync = sync;
            _file = file;
            VersionRepository = versionRepository;
            ResourceCatalogue = resourceCatalog;
            ExplorerItemFactory = explorerItemFactory;
            Directory = directory;
            TestCatalog = testCatalog;
            IsDirty = false;
        }



        public IExplorerItemFactory ExplorerItemFactory { get; private set; }
        public IDirectory Directory { get; private set; }

        public IResourceCatalog ResourceCatalogue { get; private set; }
        public ITestCatalog TestCatalog { private get; set; }
        public IServerVersionRepository VersionRepository { get; set; }

        public IExplorerItem Load(Guid workSpaceId, bool reload = false)
        {
            if (_root == null || reload)
            {
                _root = ExplorerItemFactory.CreateRootExplorerItem(EnvironmentVariables.GetWorkspacePath(workSpaceId), workSpaceId);
            }
            return _root;
        }

        public IExplorerItem Load(string type, Guid workSpaceId)
        {
            return ExplorerItemFactory.CreateRootExplorerItem(type, EnvironmentVariables.GetWorkspacePath(workSpaceId), workSpaceId);
        }

        public IExplorerRepositoryResult RenameItem(IExplorerItem itemToRename, string newName, Guid workSpaceId)
        {
            if (itemToRename == null)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemToRenameIsNull);
            }
            if (itemToRename.ResourceType == "Folder")
            {
                var oldPath = itemToRename.ResourcePath;
                var moveResult = RenameFolder(itemToRename.ResourcePath, newName, workSpaceId);
                if (moveResult.Status != ExecStatus.Success)
                {
                    return new ExplorerRepositoryResult(moveResult.Status, moveResult.Message);
                }
                var item = Find(i=>i.ResourcePath== itemToRename.ResourcePath);
                RenameChildren(item, oldPath, newName);
                var resourcesRenameResult = RenameChildrenPaths(oldPath, newName);
                itemToRename.DisplayName = newName;
                if (resourcesRenameResult.Status != ExecStatus.Success)
                {
                    return new ExplorerRepositoryResult(resourcesRenameResult.Status, resourcesRenameResult.Message);
                }
                
                return moveResult;
            }
            itemToRename.DisplayName = newName;
            return RenameExplorerItem(itemToRename, workSpaceId);

        }

        private ResourceCatalogResult RenameChildrenPaths(string oldPath, string newName)
        {
            var resourcesToRename =
                ResourceCatalogue.GetResourceList(GlobalConstants.ServerWorkspaceID)
                    .Where(a => a.GetResourcePath(GlobalConstants.ServerWorkspaceID).StartsWith(oldPath)).ToList();
            if (resourcesToRename.Count == 0)
            {
                var resourceCatalogResult = new ResourceCatalogResult {Status = ExecStatus.Success};
                return resourceCatalogResult;
            }
            ResourceCatalogResult result = ResourceCatalogue.RenameCategory(GlobalConstants.ServerWorkspaceID, oldPath, newName, resourcesToRename);
            return result;
        }

        IExplorerRepositoryResult RenameExplorerItem(IExplorerItem itemToRename, Guid workSpaceId)
        {

            IEnumerable<IResource> item =
                ResourceCatalogue.GetResourceList(workSpaceId)
                                 .Where(
                                     a =>
                                     (a.ResourceName == itemToRename.DisplayName.Trim()) &&
                                     (a.ResourceID == itemToRename.ResourceId));
            if (item.Any())
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemAlreadyExistInPath);
            }
            ResourceCatalogResult result = ResourceCatalogue.RenameResource(workSpaceId, itemToRename.ResourceId, itemToRename.DisplayName, itemToRename.ResourcePath);
            return new ExplorerRepositoryResult(result.Status, result.Message);
        }

        public IExplorerRepositoryResult RenameFolder(string path, string newPath, Guid workSpaceId)
        {
            try
            {
                if (!Directory.Exists(DirectoryStructureFromPath(path)))
                {
                    return new ExplorerRepositoryResult(ExecStatus.NoMatch, string.Format(ErrorResource.RequestedFolderDoesNotExistOnServer, path));
                }
                if (!Directory.Exists(newPath))
                {
                    Directory.Move(DirectoryStructureFromPath(path), DirectoryStructureFromPath(newPath));
                    MoveVersionFolder(path, newPath);
                }
                else
                {
                    return new ExplorerRepositoryResult(ExecStatus.Fail, "Error Renaming");
                }
            }
            catch (Exception err)
            {
                return new ExplorerRepositoryResult(ExecStatus.AccessViolation, err.Message);
            }
            return new ExplorerRepositoryResult(ExecStatus.Success, "");
        }


        void MoveVersionFolder(string path, string newPath)
        {
            if (Directory.Exists(DirectoryStructureFromPath(path)) && Directory.Exists(DirectoryStructureFromPath(newPath)))
            {
                string s = DirectoryStructureFromPath(path) + "\\" + GlobalConstants.VersionFolder;
                string t = DirectoryStructureFromPath(newPath) + "\\" + GlobalConstants.VersionFolder;
                if (Directory.Exists(DirectoryStructureFromPath(path) + "\\" + GlobalConstants.VersionFolder))
                {
                    Directory.Move(s, t);
                }
            }
        }

        public IExplorerItem UpdateItem(IResource resource)
        {
            var parentItem = Find(item => item.ResourcePath.ToLowerInvariant().TrimEnd('\\') == resource.GetSavePath().ToLowerInvariant().TrimEnd('\\'));
            if (parentItem != null)
            {
                var newExplorerItem = ExplorerItemFactory.CreateResourceItem(resource, GlobalConstants.ServerWorkspaceID); 
                parentItem.Children.Add(newExplorerItem);
                newExplorerItem.Parent = parentItem;
                return newExplorerItem;
            }
            return null;
        }

        public IExplorerItem Find(Guid id)
        {
            //var items = Load(Guid.Empty);
            if (_root == null)
            {
                Load(Guid.Empty,true);
            }
            return Find(_root, id);
        }

        public IExplorerItem Find(Func<IExplorerItem, bool> predicate)
        {
            //var items = Load(Guid.Empty);
            if (_root == null)
            {
                Load(Guid.Empty, true);
            }
            return Find(_root, predicate);
        }

        public List<string> LoadDuplicate()
        {
            return ExplorerItemFactory.GetDuplicatedResourcesPaths();
        }

        public IExplorerItem Find(IExplorerItem item, Guid itemToFind)
        {
            if (item.ResourceId == itemToFind)
                return item;
            if (item.Children == null || item.Children.Count == 0)
            {
                return null;
            }
            return item.Children.Select(child => Find(child, itemToFind)).FirstOrDefault(found => found != null);
        }

        public IExplorerItem Find(IExplorerItem item, Func<IExplorerItem, bool> predicate)
        {

            if (predicate(item))
                return item;
            if (item.Children == null || item.Children.Count == 0)
            {
                return null;
            }
            return item.Children.Select(child => Find(child, predicate)).FirstOrDefault(found => found != null);
        }


        public void MessageSubscription(IExplorerRepositorySync sync)
        {
            VerifyArgument.IsNotNull("sync", sync);
            _sync = sync;
        }

        public IExplorerRepositoryResult DeleteItem(IExplorerItem itemToDelete, Guid workSpaceId)
        {
            if (itemToDelete == null)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemToDeleteWasNull);
            }
            if (itemToDelete.ResourceType == "Folder")
            {
                var deleteResult = DeleteFolder(itemToDelete.ResourcePath, true, workSpaceId);
                if (deleteResult.Status == ExecStatus.Success)
                {
                    var folderDeleted = Find(_root, item => item.ResourcePath == itemToDelete.ResourcePath);
                    if (folderDeleted != null)
                    {
                        var parent = Find(_root, item => item.ResourcePath == GetSavePath(folderDeleted));
                        if (parent != null)
                        {
                            parent.Children.Remove(folderDeleted);
                        }
                        else
                        {
                            _root.Children.Remove(folderDeleted);
                        }
                    }
                }
                return deleteResult;
            }
            ResourceCatalogResult result = ResourceCatalogue.DeleteResource(workSpaceId, itemToDelete.ResourceId, itemToDelete.ResourceType);
            TestCatalog.DeleteAllTests(itemToDelete.ResourceId);
            if (result.Status == ExecStatus.Success)
            {
                var itemDeleted = Find(_root, itemToDelete.ResourceId);
                if (itemDeleted != null)
                {
                    var parent = Find(_root, item => item.ResourcePath == GetSavePath(itemDeleted));
                    if (parent != null)
                    {
                        parent.Children.Remove(itemDeleted);
                    }
                    else
                    {
                        _root.Children.Remove(itemDeleted);
                    }
                }
            }
            return new ExplorerRepositoryResult(result.Status, result.Message);
        }

        private string GetSavePath(IExplorerItem item)
        {
            var resourcePath = item.ResourcePath;
            var savePath = item.ResourcePath;
            var resourceNameIndex = resourcePath.LastIndexOf(item.DisplayName, StringComparison.InvariantCultureIgnoreCase);
            if (resourceNameIndex >= 0)
            {
                savePath = resourcePath.Substring(0, resourceNameIndex);
            }
            return savePath.TrimEnd('\\');
        }

        public IExplorerRepositoryResult DeleteFolder(string path, bool deleteContents, Guid workSpaceId)
        {
            if (!Directory.Exists(DirectoryStructureFromPath(path)))
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, string.Format(ErrorResource.RequestedFolderDoesNotExistOnServer, path));
            }
            var resourceList = ResourceCatalogue.GetResourceList(workSpaceId);
            if (!deleteContents && resourceList.Count(a => a.GetResourcePath(workSpaceId) == path) > 0)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, string.Format(ErrorResource.RequestedFolderDoesNotExistOnServer, path));
            }
            if (path.Trim() == "")
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.CannotDeleteRootPath);
            }
            try
            {
                var testResourceIDsToDelete = resourceList.Where(resource => resource.GetResourcePath(workSpaceId).StartsWith(path));
                var guids = testResourceIDsToDelete.Select(resourceToDelete => resourceToDelete.ResourceID);
                Directory.Delete(DirectoryStructureFromPath(path), true);


                foreach (var guid in guids)
                {
                    TestCatalog.DeleteAllTests(guid);
                }


                return new ExplorerRepositoryResult(ExecStatus.Success, "");
            }
            catch (Exception err)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, err.Message);
            }
        }

        public IExplorerRepositoryResult AddItem(IExplorerItem itemToAdd, Guid workSpaceId)
        {
            if (itemToAdd == null)
            {
                Dev2Logger.Info("Invalid Item");
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemToAddIsNull);
            }
            var resourceType = itemToAdd.ResourceType;
            if (resourceType == "Folder")
            {
                try
                {
                    string dir = $"{DirectoryStructureFromPath(itemToAdd.ResourcePath)}\\";

                    if (Directory.Exists(dir))
                    {
                        return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.RequestedFolderAlreadyExists);
                    }
                    Directory.CreateIfNotExists(dir);
                    if (itemToAdd.ResourcePath.Contains("\\"))
                    {
                        var idx = itemToAdd.ResourcePath.LastIndexOf("\\",StringComparison.InvariantCultureIgnoreCase);
                        var pathToSearch = itemToAdd.ResourcePath.Substring(0, idx);
                        var parent = Find(item => item.ResourcePath.ToLowerInvariant().TrimEnd('\\') == pathToSearch.ToLowerInvariant().TrimEnd('\\'));
                        parent?.Children.Add(itemToAdd);
                    }
                    else
                    {
                        _root.Children.Add(itemToAdd);
                    }
                    _sync.AddItemMessage(itemToAdd);
                    return new ExplorerRepositoryResult(ExecStatus.Success, "");
                }
                catch (Exception err)
                {
                    Dev2Logger.Error("Add Folder Error", err);
                    return new ExplorerRepositoryResult(ExecStatus.Fail, err.Message);
                }
            }
            if (resourceType != null && resourceType != "Unknown" && resourceType != "ReservedService")
            {
                try
                {
                    _sync.AddItemMessage(itemToAdd);

                    return new ExplorerRepositoryResult(ExecStatus.Success, "");
                }
                catch (Exception err)
                {
                    Dev2Logger.Error("Add Item Error", err);
                    return new ExplorerRepositoryResult(ExecStatus.Fail, err.Message);
                }
            }
            return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.OnlyUserResourcesCanBeAdded);
        }

        public IExplorerRepositoryResult MoveItem(IExplorerItem itemToMove, string newPath, Guid workSpaceId)
        {
            if (itemToMove.ResourceType == "Folder")
            {
                var movePath = itemToMove.DisplayName;
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    movePath = newPath + "\\" + itemToMove.DisplayName;
                }
                var oldPath = itemToMove.ResourcePath;
                Directory.Move(DirectoryStructureFromPath(oldPath), DirectoryStructureFromPath(movePath));
                MoveChildren(itemToMove, newPath);
                return new ExplorerRepositoryResult(ExecStatus.Success, "");
            }
            IEnumerable<IResource> item = ResourceCatalogue.GetResourceList(workSpaceId).Where(a => a.GetResourcePath(workSpaceId) == newPath);
            if (item.Any())
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemAlreadyExistInPath);
            }
            return MoveSingeItem(itemToMove, newPath, workSpaceId);
        }

        private void MoveChildren(IExplorerItem itemToMove, string newPath)
        {
            if (itemToMove == null)
            {
                return;
            }
            if (itemToMove.IsFolder)
            {
                if (!string.IsNullOrWhiteSpace(itemToMove.ResourcePath))
                {
                    itemToMove.ResourcePath = newPath + "\\" + itemToMove.DisplayName;
                }
                else
                {
                    itemToMove.ResourcePath = itemToMove.ResourcePath.Replace(itemToMove.ResourcePath, newPath);
                }
                if (itemToMove.Children != null && itemToMove.Children.Count > 0)
                {
                    itemToMove.Children.ForEach(item => MoveChildren(item, itemToMove.ResourcePath));
                }
            }
            else
            {
                MoveSingeItem(itemToMove, newPath, GlobalConstants.ServerWorkspaceID);
            }
            
        }

        private void RenameChildren(IExplorerItem itemToRename, string oldPath, string newPath)
        {
            if (itemToRename == null)
            {
                return;
            }
            itemToRename.ResourcePath = !string.IsNullOrWhiteSpace(itemToRename.ResourcePath) ? itemToRename.ResourcePath.Replace(oldPath, newPath) : newPath;
            if (itemToRename.IsFolder || itemToRename.ResourceType == "Folder")
            {
                
                if (itemToRename.Children != null && itemToRename.Children.Count > 0)
                {
                    itemToRename.Children.ForEach(item => RenameChildren(item, oldPath, newPath));
                }
            }                        
        }

        IExplorerRepositoryResult MoveSingeItem(IExplorerItem itemToMove, string newPath, Guid workSpaceId)
        {
            var newResourcePath = newPath;
            if (!string.IsNullOrEmpty(itemToMove.ResourcePath))
            {
                newResourcePath = itemToMove.ResourcePath.Replace(itemToMove.ResourcePath, newPath);
            }
            var resource = ResourceCatalogue.GetResource(workSpaceId, itemToMove.ResourceId);
            var source = $"{DirectoryStructureFromPath(resource.GetResourcePath(GlobalConstants.ServerWorkspaceID))}.xml";
            var destination = $"{DirectoryStructureFromPath(newResourcePath)+"\\"+resource.ResourceName+".xml"}";
            if (_file.Exists(source))
            {
                _file.Move(source, destination);
            }
            ResourceCatalogResult result = ResourceCatalogue.RenameCategory(workSpaceId, resource.GetSavePath(), newResourcePath, new List<IResource> { resource });
            itemToMove.ResourcePath = newResourcePath;
            return new ExplorerRepositoryResult(result.Status, result.Message);
        }

        void MoveVersions(IExplorerItem itemToMove, string newPath)
        {
            VersionRepository.MoveVersions(itemToMove.ResourceId, newPath, itemToMove.ResourcePath);
        }

        public static string DirectoryStructureFromPath(string path)
        {
            return Path.Combine(EnvironmentVariables.ResourcePath, path);
        }

        public IExplorerItem Load(string type, string filter)
        {
            return ExplorerItemFactory.CreateRootExplorerItem(type, Path.Combine(EnvironmentVariables.GetWorkspacePath(Guid.Empty), filter), Guid.Empty);
        }

    }
}
