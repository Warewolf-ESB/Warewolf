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
using System.Linq;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Interfaces;

namespace Dev2.Models
{
    public class ServerExplorerClientProxy : IExplorerResourceRepository
    {
        public ICommunicationControllerFactory CommunicationControllerFactory { get; private set; }

        public IEnvironmentConnection Connection => _connection;
        private readonly IEnvironmentConnection _connection;

        public ServerExplorerClientProxy(IEnvironmentConnection connection, ICommunicationControllerFactory communicationControllerFactory)
        {
            CommunicationControllerFactory = communicationControllerFactory;
            _connection = connection;
        }
        
        public IExplorerItem Load(Guid workSpaceId) => Load(workSpaceId, true);
        public IExplorerItem Load(Guid workSpaceId, bool reload)
        {
            var controller = CommunicationControllerFactory.CreateController("FetchExplorerItemsService");
            return controller.ExecuteCommand<IExplorerItem>(Connection, workSpaceId);
        }

        public IExplorerRepositoryResult RenameItem(IExplorerItem itemToRename, string newName, Guid workSpaceId)
        {
            var controller = CommunicationControllerFactory.CreateController("RenameItemService");
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("itemToRename", serializer.SerializeToBuilder(itemToRename).ToString());
            controller.AddPayloadArgument("newName", newName);
            return controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, workSpaceId);
        }

        public IExplorerRepositoryResult DeleteItem(IExplorerItem itemToRename, Guid workSpaceId)
        {
            var controller = CommunicationControllerFactory.CreateController("DeleteItemService");
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("itemToDelete", serializer.SerializeToBuilder(itemToRename).ToString());
            return controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, workSpaceId);
        }

        public IExplorerRepositoryResult AddItem(IExplorerItem itemToAdd, Guid workSpaceId)
        {
            var controller = CommunicationControllerFactory.CreateController("AddFolderService");
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("itemToAdd", serializer.SerializeToBuilder(itemToAdd).ToString());
            return controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, workSpaceId);
        }

        public IExplorerRepositoryResult MoveItem(IExplorerItem itemToMove, string newPath, Guid workSpaceId)
        {
            var controller = CommunicationControllerFactory.CreateController("MoveItemService");
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("itemToMove", serializer.SerializeToBuilder(itemToMove).ToString());
            controller.AddPayloadArgument("newPath", newPath);
            return controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, workSpaceId);
        }
    }
}
