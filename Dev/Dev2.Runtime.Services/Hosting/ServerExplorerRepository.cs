/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using ServiceStack.Common.Extensions;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.Hosting
{
    public class ServerExplorerRepository : IExplorerServerResourceRepository
    {
        public interface IConfig
        {
            IAuthorizationService AuthorizationService { get; }
            IResourceCatalog ResourceCatalog {get;}
            ITestCatalog TestCatalog {get;}
            IExplorerItemFactory ExplorerItemFactory { get; }
            IVersionStrategy VersionStrategy {get;}
            IDirectory DirectoryWrapper {get;}
            IFile FileWrapper {get;}
            IFilePath FilePathWrapper {get;}
            string WorkspacePath {get;}
        }
        public class Config : IConfig
        {
            public IAuthorizationService AuthorizationService { get => ServerAuthorizationService.Instance; }
            public IResourceCatalog ResourceCatalog { get => Hosting.ResourceCatalog.Instance; }
            public ITestCatalog TestCatalog { get => Runtime.TestCatalog.Instance; }

            IExplorerItemFactory _explorerItemFactory;
            public IExplorerItemFactory ExplorerItemFactory { get => _explorerItemFactory ?? (_explorerItemFactory = new ExplorerItemFactory(ResourceCatalog, DirectoryWrapper, AuthorizationService)); }

            IVersionStrategy _versionStrategy;
            public IVersionStrategy VersionStrategy { get => _versionStrategy ?? (_versionStrategy = new VersionStrategy()); }

            IDirectory _directoryWrapper;
            public IDirectory DirectoryWrapper { get => _directoryWrapper ?? (_directoryWrapper = new DirectoryWrapper()); }

            IFile _fileWrapper;
            public IFile FileWrapper { get => _fileWrapper ?? (_fileWrapper = new FileWrapper()); }

            IFilePath _filePathWrapper;
            public IFilePath FilePathWrapper { get => _filePathWrapper ?? (_filePathWrapper = new FilePathWrapper()); }
            public string WorkspacePath { get => EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID); }
        }

        IExplorerRepositorySync _sync;
        readonly IConfig _config;

        private static IExplorerServerResourceRepository _instance = new ServerExplorerRepository();
        public static IExplorerServerResourceRepository Instance { get => _instance; }
#pragma warning disable S3253 // Disable wrong warning for static constructor
        static ServerExplorerRepository()
        {
        }
#pragma warning restore S3253 // Constructor and destructor declarations should not be redundant

        private ServerExplorerRepository()
            :this(new Config())
        {
        }

        private ServerExplorerRepository(Config config)
            :this(config, null)
        {
        }

        internal ServerExplorerRepository(IConfig config, IExplorerRepositorySync sync)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>
                {
                    { nameof(config.ResourceCatalog), config.ResourceCatalog },
                    { nameof(config.ExplorerItemFactory), config.ExplorerItemFactory },
                    { nameof(config.DirectoryWrapper), config.DirectoryWrapper },
                    { nameof(config.TestCatalog) , config.TestCatalog }
                });
            _config = config;
            _sync = sync;
            IsDirty = false;
        }

        bool _isDirty;
        IExplorerItem _root;

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

        public IExplorerItem Load(Guid workSpaceId) => Load(workSpaceId, false);
        public IExplorerItem Load(Guid workSpaceId, bool reload)
        {
            if (_root == null || reload)
            {
                _root = _config.ExplorerItemFactory.CreateRootExplorerItem(EnvironmentVariables.GetWorkspacePath(workSpaceId), workSpaceId);
            }
            return _root;
        }

        public IExplorerItem Load(string type, Guid workSpaceId) => _config.ExplorerItemFactory.CreateRootExplorerItem(type, EnvironmentVariables.GetWorkspacePath(workSpaceId), workSpaceId);

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
                var item = Find(i => i.ResourcePath == itemToRename.ResourcePath);
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

        ResourceCatalogResult RenameChildrenPaths(string oldPath, string newName)
        {
            var resourcesToRename =
                _config.ResourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID)
                    .Where(a => a.GetResourcePath(GlobalConstants.ServerWorkspaceID).StartsWith(oldPath)).ToList();
            if (resourcesToRename.Count == 0)
            {
                var resourceCatalogResult = new ResourceCatalogResult { Status = ExecStatus.Success };
                return resourceCatalogResult;
            }
            var result = _config.ResourceCatalog.RenameCategory(GlobalConstants.ServerWorkspaceID, oldPath, newName, resourcesToRename);
            return result;
        }

        IExplorerRepositoryResult RenameExplorerItem(IExplorerItem itemToRename, Guid workSpaceId)
        {
            var item =
                _config.ResourceCatalog.GetResourceList(workSpaceId)
                                 .Where(
                                     a =>
                                     (a.ResourceName == itemToRename.DisplayName.Trim()) &&
                                     (a.ResourceID == itemToRename.ResourceId));
            if (item.Any())
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemAlreadyExistInPath);
            }
            var result = _config.ResourceCatalog.RenameResource(workSpaceId, itemToRename.ResourceId, itemToRename.DisplayName, itemToRename.ResourcePath);
            return new ExplorerRepositoryResult(result.Status, result.Message);
        }

        public IExplorerRepositoryResult RenameFolder(string path, string newPath, Guid workSpaceId)
        {
            try
            {
                if (!_config.DirectoryWrapper.Exists(DirectoryStructureFromPath(path)))
                {
                    return new ExplorerRepositoryResult(ExecStatus.NoMatch, string.Format(ErrorResource.RequestedFolderDoesNotExistOnServer, path));
                }
                if (!_config.DirectoryWrapper.Exists(newPath))
                {
                    _config.DirectoryWrapper.Move(DirectoryStructureFromPath(path), DirectoryStructureFromPath(newPath));
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
            if (_config.DirectoryWrapper.Exists(DirectoryStructureFromPath(path)) && _config.DirectoryWrapper.Exists(DirectoryStructureFromPath(newPath)))
            {
                var s = DirectoryStructureFromPath(path) + "\\" + GlobalConstants.VersionFolder;
                var t = DirectoryStructureFromPath(newPath) + "\\" + GlobalConstants.VersionFolder;
                if (_config.DirectoryWrapper.Exists(DirectoryStructureFromPath(path) + "\\" + GlobalConstants.VersionFolder))
                {
                    _config.DirectoryWrapper.Move(s, t);
                }
            }
        }

        public IExplorerItem UpdateItem(IResource resource)
        {
            var parentItem = Find(item => item.ResourcePath.ToLowerInvariant().TrimEnd('\\') == resource.GetSavePath().ToLowerInvariant().TrimEnd('\\'));
            if (parentItem != null)
            {
                var newExplorerItem = _config.ExplorerItemFactory.CreateResourceItem(resource, GlobalConstants.ServerWorkspaceID);
                if (parentItem.Children == null)
                {
                    parentItem.Children = new List<IExplorerItem>();
                }
                parentItem.Children.Add(newExplorerItem);
                newExplorerItem.Parent = parentItem;
                return newExplorerItem;
            }
            return null;
        }

        public IExplorerItem Find(Guid id)
        {
            if (_root == null)
            {
                Load(Guid.Empty, true);
            }
            return Find(_root, id);
        }

        public IExplorerItem Find(Func<IExplorerItem, bool> predicate)
        {
            if (_root == null)
            {
                Load(Guid.Empty, true);
            }
            return Find(_root, predicate);
        }

        public List<string> LoadDuplicate() => _config.ExplorerItemFactory.GetDuplicatedResourcesPaths();

        public IExplorerItem Find(IExplorerItem item, Guid itemToFind)
        {
            if (item.ResourceId == itemToFind)
            {
                return item;
            }

            if (item.Children == null || item.Children.Count == 0)
            {
                return null;
            }
            return item.Children.Select(child => Find(child, itemToFind)).FirstOrDefault(found => found != null);
        }

        public IExplorerItem Find(IExplorerItem item, Func<IExplorerItem, bool> predicate)
        {

            if (predicate?.Invoke(item) ?? default(bool))
            {
                return item;
            }

            if (item.Children == null || item.Children.Count == 0)
            {
                return null;
            }
            return item.Children.Select(child => Find(child, predicate)).FirstOrDefault(found => found != null);
        }


        public void MessageSubscription(IExplorerRepositorySync sync)
        {
            VerifyArgument.IsNotNull(nameof(sync), sync);
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
                return DeleteFolder(itemToDelete, workSpaceId);
            }
            var result = _config.ResourceCatalog.DeleteResource(workSpaceId, itemToDelete.ResourceId, itemToDelete.ResourceType);
            _config.TestCatalog.DeleteAllTests(itemToDelete.ResourceId);
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

        IExplorerRepositoryResult DeleteFolder(IExplorerItem itemToDelete, Guid workSpaceId)
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

        static string GetSavePath(IExplorerItem item)
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
            if (!_config.DirectoryWrapper.Exists(DirectoryStructureFromPath(path)))
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, string.Format(ErrorResource.RequestedFolderDoesNotExistOnServer, path));
            }
            var resourceList = _config.ResourceCatalog.GetResourceList(workSpaceId);
            if (!deleteContents && resourceList.Any(a => a.GetResourcePath(workSpaceId) == path))
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
                _config.DirectoryWrapper.Delete(DirectoryStructureFromPath(path), true);


                foreach (var guid in guids)
                {
                    _config.TestCatalog.DeleteAllTests(guid);
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
                Dev2Logger.Info("Invalid Item", GlobalConstants.WarewolfInfo);
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemToAddIsNull);
            }
            var resourceType = itemToAdd.ResourceType;
            if (resourceType == "Folder")
            {
                try
                {
                    var dir = $"{DirectoryStructureFromPath(itemToAdd.ResourcePath)}\\";

                    if (_config.DirectoryWrapper.Exists(dir))
                    {
                        return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.RequestedFolderAlreadyExists);
                    }
                    _config.DirectoryWrapper.CreateIfNotExists(dir);
                    if (itemToAdd.ResourcePath.Contains("\\"))
                    {
                        var idx = itemToAdd.ResourcePath.LastIndexOf("\\", StringComparison.InvariantCultureIgnoreCase);
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
                    Dev2Logger.Error("Add Folder Error", err, GlobalConstants.WarewolfError);
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
                    Dev2Logger.Error("Add Item Error", err, GlobalConstants.WarewolfError);
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
                _config.DirectoryWrapper.Move(DirectoryStructureFromPath(oldPath), DirectoryStructureFromPath(movePath));
                MoveChildren(itemToMove, newPath);
                return new ExplorerRepositoryResult(ExecStatus.Success, "");
            }
            var item = _config.ResourceCatalog.GetResourceList(workSpaceId).Where(a => a.GetResourcePath(workSpaceId) == newPath);
            if (item.Any())
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemAlreadyExistInPath);
            }
            return MoveSingeItem(itemToMove, newPath, workSpaceId);
        }

        void MoveChildren(IExplorerItem itemToMove, string newPath)
        {
            if (itemToMove == null)
            {
                return;
            }
            if (itemToMove.IsFolder)
            {
                itemToMove.ResourcePath = !string.IsNullOrWhiteSpace(itemToMove.ResourcePath) ? newPath + "\\" + itemToMove.DisplayName : itemToMove.ResourcePath.Replace(itemToMove.ResourcePath, newPath);
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

        void RenameChildren(IExplorerItem itemToRename, string oldPath, string newPath)
        {
            if (itemToRename == null)
            {
                return;
            }
            itemToRename.ResourcePath = !string.IsNullOrWhiteSpace(itemToRename.ResourcePath) ? itemToRename.ResourcePath.Replace(oldPath, newPath) : newPath;
            if ((itemToRename.IsFolder || itemToRename.ResourceType == "Folder") && (itemToRename.Children != null && itemToRename.Children.Count > 0))
            {
                itemToRename.Children.ForEach(item => RenameChildren(item, oldPath, newPath));
            }
        }

        IExplorerRepositoryResult MoveSingeItem(IExplorerItem itemToMove, string newPath, Guid workSpaceId)
        {
            var newResourcePath = newPath;
            if (!string.IsNullOrEmpty(itemToMove.ResourcePath))
            {
                newResourcePath = itemToMove.ResourcePath.Replace(itemToMove.ResourcePath, newPath);
            }
            var resource = _config.ResourceCatalog.GetResource(workSpaceId, itemToMove.ResourceId);
            var source = $"{DirectoryStructureFromPath(resource.GetResourcePath(GlobalConstants.ServerWorkspaceID))}.bite";
            var destination = $"{DirectoryStructureFromPath(newResourcePath)+"\\"+resource.ResourceName+".bite"}";
            if (_config.FileWrapper.Exists(source))
            {
                _config.FileWrapper.Move(source, destination);
            }
            var result = _config.ResourceCatalog.RenameCategory(workSpaceId, resource.GetSavePath(), newResourcePath, new List<IResource> { resource });
            itemToMove.ResourcePath = newResourcePath;
            return new ExplorerRepositoryResult(result.Status, result.Message);
        }

        public static string DirectoryStructureFromPath(string path) => Path.Combine(EnvironmentVariables.ResourcePath, path);

        public IExplorerItem Load(string type, string filter) => _config.ExplorerItemFactory.CreateRootExplorerItem(type, Path.Combine(EnvironmentVariables.GetWorkspacePath(Guid.Empty), filter), Guid.Empty);
    }
}
