using Dev2.Common;
using Dev2.Common.Wrappers;
using Dev2.Common.Wrappers.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dev2.Runtime.Hosting
{
    public class ServerExplorerRepository : IExplorerServerResourceRepository
    {
        IExplorerRepositorySync _sync;
        public static IExplorerServerResourceRepository Instance { get; private set; }

        static ServerExplorerRepository()
        {
            Instance = new ServerExplorerRepository
                {
                    ResourceCatalogue = ResourceCatalog.Instance,
                    ExplorerItemFactory = new ExplorerItemFactory(ResourceCatalog.Instance, new DirectoryWrapper(), ServerAuthorizationService.Instance),
                    Directory = new DirectoryWrapper()
                };
        }

        ServerExplorerRepository()
        {
        }

        internal ServerExplorerRepository(IResourceCatalog resourceCatalog, IExplorerItemFactory explorerItemFactory, IDirectory directory, IExplorerRepositorySync sync)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>
                {
                    { "resourceCatalog", resourceCatalog },
                    { "explorerItemFactory", explorerItemFactory },
                    { "directory", directory }
                });
            _sync = sync;
            ResourceCatalogue = resourceCatalog;
            ExplorerItemFactory = explorerItemFactory;
            Directory = directory;
        }

        protected IExplorerItemFactory ExplorerItemFactory { get; private set; }
        public IDirectory Directory { get; private set; }

        public IResourceCatalog ResourceCatalogue { get; private set; }

        public IExplorerItem Load(Guid workSpaceId)
        {
            return ExplorerItemFactory.CreateRootExplorerItem(EnvironmentVariables.GetWorkspacePath(workSpaceId), workSpaceId);
        }

        public IExplorerItem Load(ResourceType type, Guid workSpaceId)
        {
            return ExplorerItemFactory.CreateRootExplorerItem(type, EnvironmentVariables.GetWorkspacePath(workSpaceId), workSpaceId);
        }

        public IExplorerRepositoryResult RenameItem(IExplorerItem itemToRename, string newName, Guid workSpaceId)
        {
            if(itemToRename == null)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, "Item to rename was null");
            }
            switch(itemToRename.ResourceType)
            {
                case ResourceType.Folder:
                    return RenameFolder(itemToRename.ResourcePath, newName, workSpaceId);
                default:
                    {
                        itemToRename.DisplayName = newName;
                        return RenameExplorerItem(itemToRename, workSpaceId);
                    }
            }
        }
        
        IExplorerRepositoryResult RenameExplorerItem(IExplorerItem itemToRename, Guid workSpaceId)
        {

            IEnumerable<IResource> item =
                ResourceCatalogue.GetResourceList(workSpaceId)
                                 .Where(
                                     a =>
                                     (a.ResourceName == itemToRename.DisplayName.Trim()) &&
                                     (a.ResourcePath == itemToRename.ResourcePath.Trim()));
            if(item.Any())
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, "There is an item that exists with the same name and path");
            }
            ResourceCatalogResult result = ResourceCatalogue.RenameResource(workSpaceId, itemToRename.ResourceId, itemToRename.DisplayName);
            return new ExplorerRepositoryResult(result.Status, result.Message);
        }

        public IExplorerRepositoryResult RenameFolder(string path, string newPath, Guid workSpaceId)
        {
            try
            {
                if(!Directory.Exists(DirectoryStructureFromPath(path)))
                {
                    return new ExplorerRepositoryResult(ExecStatus.NoMatch, "Requested folder does not exist on server. Folder: " + path);
                }
                var resourceCatalogResult = ResourceCatalogue.RenameCategory(workSpaceId, path, newPath);
                if(resourceCatalogResult.Status == ExecStatus.Success)
                {
                    Directory.Delete(DirectoryStructureFromPath(path), true);
                    return new ExplorerRepositoryResult(ExecStatus.Success, "");
                }
                return new ExplorerRepositoryResult(ExecStatus.Fail, resourceCatalogResult.Message);
            }
            catch(Exception err)
            {
                return new ExplorerRepositoryResult(ExecStatus.AccessViolation, err.Message);
            }
        }

        public void MessageSubscription(IExplorerRepositorySync sync)
        {
            VerifyArgument.IsNotNull("sync", sync);
            _sync = sync;
        }

        public IExplorerRepositoryResult DeleteItem(IExplorerItem itemToDelete, Guid workSpaceId)
        {
            if(itemToDelete == null)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, "Item to delete was null");
            }
            switch(itemToDelete.ResourceType)
            {
                case ResourceType.Folder:
                    {
                        return DeleteFolder(itemToDelete.ResourcePath, true, workSpaceId);
                    }
                default:
                    ResourceCatalogResult result = ResourceCatalogue.DeleteResource(workSpaceId, itemToDelete.DisplayName, itemToDelete.ResourceType.ToString());
                    return new ExplorerRepositoryResult(result.Status, result.Message);
            }
        }

        public IExplorerRepositoryResult DeleteFolder(string path, bool deleteContents, Guid workSpaceId)
        {
            if(!Directory.Exists(DirectoryStructureFromPath(path)))
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, "Requested folder does not exist on server. Folder: " + path);
            }
            if(!deleteContents && ResourceCatalogue.GetResourceList(workSpaceId).Count(a => a.ResourcePath == path) > 0)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, "Requested folder contains existing valid resources " + path);
            }
            if(path.Trim() == "")
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, "You may not delete the root path");
            }
            try
            {
                List<ResourceCatalogResult> deletedResources = ResourceCatalogue.GetResourceList(workSpaceId)
                                                                                .Where(a => a.ResourcePath.StartsWith(path))
                                                                                .Select(a => ResourceCatalogue.DeleteResource(workSpaceId, a.ResourceName, a.ResourceType.ToString())).ToList();
                if(deletedResources.Any(a => a.Status != ExecStatus.Success))
                {
                    return new ExplorerRepositoryResult(ExecStatus.Fail, "Failed to delete child items");
                }

                Directory.Delete(DirectoryStructureFromPath(path), true);
                return new ExplorerRepositoryResult(ExecStatus.Success, "");
            }
            catch(Exception err)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, err.Message);
            }
        }

        public IExplorerRepositoryResult AddItem(IExplorerItem itemToAdd, Guid workSpaceId)
        {
            if(itemToAdd == null)
            {
                return new ExplorerRepositoryResult(ExecStatus.Fail, "Item to add was null");
            }
            switch(itemToAdd.ResourceType)
            {
                case ResourceType.Folder:
                    {
                        try
                        {
                            string dir = string.Format("{0}\\", DirectoryStructureFromPath(itemToAdd.ResourcePath));

                            if(Directory.Exists(dir))
                            {
                                return new ExplorerRepositoryResult(ExecStatus.Fail, "Requested folder already exists on server.");
                            }
                            Directory.CreateIfNotExists(dir);
                            _sync.AddItemMessage(itemToAdd);
                            return new ExplorerRepositoryResult(ExecStatus.Success, "");
                        }
                        catch(Exception err)
                        {
                            return new ExplorerRepositoryResult(ExecStatus.Fail, err.Message);
                        }
                    }
                case ResourceType.DbSource:
                case ResourceType.EmailSource:
                case ResourceType.WebSource:
                case ResourceType.ServerSource:
                case ResourceType.PluginService:
                case ResourceType.WebService:
                case ResourceType.DbService:
                case ResourceType.WorkflowService:
                    {
                        try
                        {
                            _sync.AddItemMessage(itemToAdd);
                            return new ExplorerRepositoryResult(ExecStatus.Success, "");
                        }
                        catch(Exception err)
                        {
                            return new ExplorerRepositoryResult(ExecStatus.Fail, err.Message);
                        }
                    }
                default:
                    return new ExplorerRepositoryResult(ExecStatus.Fail, "Only user resources can be added from this repository");
            }
        }

        static string DirectoryStructureFromPath(string path)
        {
            return Path.Combine(EnvironmentVariables.ResourcePath, path);
        }

        public IExplorerItem Load(ResourceType type, string filter)
        {
            return ExplorerItemFactory.CreateRootExplorerItem(type, Path.Combine(EnvironmentVariables.GetWorkspacePath(Guid.Empty), filter), Guid.Empty);
        }

        public IExplorerItem Load(string filter)
        {
            return ExplorerItemFactory.CreateRootExplorerItem(Path.Combine(EnvironmentVariables.GetWorkspacePath(Guid.Empty), filter), Guid.Empty);
        }
    }
}
