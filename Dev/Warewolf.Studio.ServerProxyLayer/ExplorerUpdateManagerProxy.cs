using System;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Explorer;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class ExplorerUpdateManagerProxy:ProxyBase,IExplorerUpdateManager
    {
        #region Implementation of IExplorerUpdateManager

        public ExplorerUpdateManagerProxy(ICommunicationControllerFactory communicationControllerFactory, Dev2.Studio.Core.Interfaces.IEnvironmentConnection connection)
            :base(communicationControllerFactory,connection)
        {

        }

        /// <summary>
        /// Add a folder to a warewolf server
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        public void AddFolder(string path, string name, Guid id)
        {
            var controller = CommunicationControllerFactory.CreateController("AddFolderService");
            string resourcePath = String.IsNullOrEmpty(path) ? name : $"{path}\\{name}";
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            ServerExplorerItem explorerItemModel = new ServerExplorerItem
            {
                DisplayName = name,
                ResourceType = "Folder",
                ResourcePath = resourcePath,
                ResourceId = id
            };
            controller.AddPayloadArgument("itemToAdd", serialiser.SerializeToBuilder(explorerItemModel));
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID);
            if(result.Status != ExecStatus.Success)
            {
                throw new WarewolfSaveException(result.Message,null);
            }            
        }

        /// <summary>
        /// delete a folder from a warewolf server
        /// </summary>
        /// <param name="path">the folder path</param>
        public void DeleteFolder(string path)
        {
            var controller = CommunicationControllerFactory.CreateController("DeleteItemService");
            controller.AddPayloadArgument("folderToDelete", path);
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID);
            if (result.Status != ExecStatus.Success)
            {
                throw new WarewolfSaveException(result.Message, null);
            }
        }

        /// <summary>
        /// delete a resource from a warewolf server
        /// </summary>
        /// <param name="id">resource id</param>
        public void DeleteResource(Guid id)
        {
            var controller = CommunicationControllerFactory.CreateController("DeleteItemService");
            controller.AddPayloadArgument("itemToDelete", id.ToString());
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID);
            if (result?.Status != ExecStatus.Success)
            {
                if(result != null)
                {
                    throw new WarewolfSaveException(result.Message, null);
                }
            }
        }

        /// <summary>
        /// Rename a resource
        /// </summary>
        /// <param name="id">the resource id</param>
        /// <param name="newName">the new name</param>
        public void Rename(Guid id, string newName)
        {
            var controller = CommunicationControllerFactory.CreateController("RenameItemService");
            controller.AddPayloadArgument("itemToRename", id.ToString());
            controller.AddPayloadArgument("newName", newName);
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID);
            if (result.Status != ExecStatus.Success)
            {
                throw new WarewolfSaveException(result.Message, null);
            }
        }

        /// <summary>
        /// Rename a folder
        /// </summary>
        /// <param name="path">the folder path</param>
        /// <param name="newName">the new name</param>
        public void RenameFolder(string path, string newName)
        {
            var controller = CommunicationControllerFactory.CreateController("RenameItemService");
            controller.AddPayloadArgument("folderToRename", path);
            controller.AddPayloadArgument("newName", newName);
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID);
            if (result.Status != ExecStatus.Success)
            {
                throw new WarewolfSaveException(result.Message, null);
            }
        }

        /// <summary>
        /// Move a resource to another folder
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="destinationPath"></param>
        /// <param name="resourcePath"></param>
        public async Task<IExplorerRepositoryResult> MoveItem(Guid sourceId, string destinationPath, string resourcePath)
        {
            var controller = CommunicationControllerFactory.CreateController("MoveItemService");
            controller.AddPayloadArgument("itemToMove", sourceId.ToString());
            controller.AddPayloadArgument("newPath", destinationPath);
            controller.AddPayloadArgument("itemToBeRenamedPath", resourcePath);
            return await controller.ExecuteCommandAsync<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID);
        }

        #endregion
    }
}
