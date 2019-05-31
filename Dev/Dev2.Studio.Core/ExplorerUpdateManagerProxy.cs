#pragma warning disable
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
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Explorer;
using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core
{
    public class ExplorerUpdateManagerProxy : ProxyBase, IExplorerUpdateManager
    {
        #region Implementation of IExplorerUpdateManager

        public ExplorerUpdateManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection):base(communicationControllerFactory,connection)
        {

        }
        
        public void AddFolder(string path, string name, Guid id)
        {
            var controller = CommunicationControllerFactory.CreateController("AddFolderService");
            var resourcePath = String.IsNullOrEmpty(path) ? name : $"{path}\\{name}";
            var serialiser = new Dev2JsonSerializer();
            var explorerItemModel = new ServerExplorerItem
            {
                DisplayName = name,
                ResourceType = "Folder",
                ResourcePath = resourcePath,
                ResourceId = id,
                IsFolder = true
            };
            controller.AddPayloadArgument("itemToAdd", serialiser.SerializeToBuilder(explorerItemModel));
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID);
            if(result.Status != ExecStatus.Success)
            {
                throw new WarewolfSaveException(result.Message,null);
            }
        }
        
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
        
        public void DeleteResource(Guid id)
        {
            var controller = CommunicationControllerFactory.CreateController("DeleteItemService");
            controller.AddPayloadArgument("itemToDelete", id.ToString());
            var result = controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID);
            if (result?.Status != ExecStatus.Success && result != null)
            {
                throw new WarewolfSaveException(result.Message, null);
            }

        }
        
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
        
        public async Task<IExplorerRepositoryResult> MoveItem(Guid sourceId, string destinationPath, string itemPath)
        {
            var controller = CommunicationControllerFactory.CreateController("MoveItemService");
            controller.AddPayloadArgument("itemToMove", sourceId.ToString());
            controller.AddPayloadArgument("newPath", destinationPath);
            controller.AddPayloadArgument("itemToBeRenamedPath", itemPath);
            return await controller.ExecuteCommandAsync<IExplorerRepositoryResult>(Connection, GlobalConstants.ServerWorkspaceID).ConfigureAwait(true);
        }

        #endregion
    }
}
