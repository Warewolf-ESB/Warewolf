/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;
// ReSharper disable UnusedMember.Global

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
                ExplorerItemFactory = new ExplorerItemFactory(ResourceCatalog.Instance, new DirectoryWrapper(), ServerAuthorizationService.Instance),
                Directory = new DirectoryWrapper(),
                VersionRepository = new ServerVersionRepository(new VersionStrategy(), ResourceCatalog.Instance, new DirectoryWrapper(), EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID), new FileWrapper())

            };

        }


        internal ServerExplorerRepository() { _file = new FileWrapper(); }

        public ServerExplorerRepository(IResourceCatalog resourceCatalog, IExplorerItemFactory explorerItemFactory, IDirectory directory, IExplorerRepositorySync sync, IServerVersionRepository versionRepository, IFile file)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>
                {
                    { "resourceCatalog", resourceCatalog },
                    { "explorerItemFactory", explorerItemFactory },
                    { "directory", directory }
                });
            _sync = sync;
            _file = file;
            VersionRepository = versionRepository;
            ResourceCatalogue = resourceCatalog;
            ExplorerItemFactory = explorerItemFactory;
            Directory = directory;

            IsDirty = false;
        }

        protected IExplorerItemFactory ExplorerItemFactory { get; private set; }
        public IDirectory Directory { get; private set; }

        public IResourceCatalog ResourceCatalogue { get; private set; }
        public IServerVersionRepository VersionRepository { get; set; }

        public IExplorerItem Load(Guid workSpaceId,bool reload=false)
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
                return RenameFolder(itemToRename.ResourcePath, newName, workSpaceId);
            }
            itemToRename.DisplayName = newName;
            return RenameExplorerItem(itemToRename, workSpaceId);

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
                    Load(workSpaceId, true);
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
            return null;
        }

        public IExplorerItem Find(Guid id)
        {
            var items = Load(Guid.Empty);
            return Find(items, id);
        }

        public IExplorerItem Find(Func<IExplorerItem, bool> predicate)
        {
            var items = Load(Guid.Empty);
            return Find(items, predicate);
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
                return deleteResult;
            }
            ResourceCatalogResult result = ResourceCatalogue.DeleteResource(workSpaceId, itemToDelete.ResourceId, itemToDelete.ResourceType);
            return new ExplorerRepositoryResult(result.Status, result.Message);
        }


        public IExplorerRepositoryResult DeleteFolder(string path, bool deleteContents, Guid workSpaceId)
        {
            if (!Directory.Exists(DirectoryStructureFromPath(path)))
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, string.Format(ErrorResource.RequestedFolderDoesNotExistOnServer, path));
            }
            if (!deleteContents && ResourceCatalogue.GetResourceList(workSpaceId).Count(a => a.GetResourcePath(workSpaceId) == path) > 0)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, string.Format(ErrorResource.RequestedFolderDoesNotExistOnServer, path));
            }
            if (path.Trim() == "")
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.CannotDeleteRootPath);
            }
            try
            {
                path = path + "\\";
                var resources = ResourceCatalogue.GetResourceList(workSpaceId).Where(a => a.GetResourcePath(workSpaceId).StartsWith(path)).ToList();
                List<ResourceCatalogResult> deletedResources = resources.Select(a => ResourceCatalogue.DeleteResource(workSpaceId, a.ResourceName, a.ResourceType.ToString())).ToList();
                if (deletedResources.Any(a => a.Status != ExecStatus.Success))
                {
                    return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.FailedToDeleteChildItems);
                }

                Directory.Delete(DirectoryStructureFromPath(path), true);

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
                    string dir = string.Format("{0}\\", DirectoryStructureFromPath(itemToAdd.ResourcePath));

                    if (Directory.Exists(dir))
                    {
                        return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.RequestedFolderAlreadyExists);
                    }
                    Directory.CreateIfNotExists(dir);

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
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    newPath = newPath + "\\" + itemToMove.DisplayName;
                }
                else
                {
                    newPath = itemToMove.DisplayName;
                }
                if (itemToMove.Children == null || itemToMove.Children.Count == 0)
                {
                    var oldPath = itemToMove.ResourcePath;
                    itemToMove.ResourcePath = newPath;
                    AddItem(itemToMove, workSpaceId);
                    DeleteFolder(oldPath, true, workSpaceId);
                }
                else
                {
                    foreach (var explorerItem in itemToMove.Children)
                    {
                        MoveItem(explorerItem, newPath, workSpaceId);
                    }
                }
                return new ExplorerRepositoryResult(ExecStatus.Success, "");
            }
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                IEnumerable<IResource> item = ResourceCatalogue.GetResourceList(workSpaceId).Where(a => a.GetResourcePath(workSpaceId) == newPath);
                if (item.Any())
                {
                    return new ExplorerRepositoryResult(ExecStatus.Fail, ErrorResource.ItemAlreadyExistInPath);
                }
                return MoveSingeItem(itemToMove, newPath, workSpaceId);
            }
        }

        IExplorerRepositoryResult MoveSingeItem(IExplorerItem itemToMove, string newPath, Guid workSpaceId)
        {
            MoveVersions(itemToMove, newPath);
            ResourceCatalogResult result = ResourceCatalogue.RenameCategory(workSpaceId, itemToMove.ResourcePath, newPath, new List<IResource> { ResourceCatalogue.GetResource(workSpaceId, itemToMove.ResourceId) });
            _file.Delete(string.Format("{0}.xml", DirectoryStructureFromPath(itemToMove.ResourcePath)));
            //Reload(workSpaceId);
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
