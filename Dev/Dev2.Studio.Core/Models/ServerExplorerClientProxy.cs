
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Models
{
    public class ServerExplorerClientProxy : IClientExplorerResourceRepository
    {
        public ICommunicationControllerFactory CommunicationControllerFactory { get; private set; }
        private readonly IEnvironmentConnection _connection;

        public ServerExplorerClientProxy(IEnvironmentConnection connection, ICommunicationControllerFactory communicationControllerFactory)
        {
            CommunicationControllerFactory = communicationControllerFactory;
            _connection = connection;
        }
        public ServerExplorerClientProxy(IEnvironmentConnection connection)
        {
            CommunicationControllerFactory = new CommunicationControllerFactory();
            _connection = connection;
        }
        public IEnvironmentConnection Connection
        {
            get { return _connection; }
        }

        public IExplorerItem Load(Guid workSpaceId)
        {
            var controller = CommunicationControllerFactory.CreateController("FetchExplorerItemsService");
            return (controller.ExecuteCommand<IExplorerItem>(Connection, workSpaceId));
        }

        public IExplorerItem Load(ResourceType type, Guid workSpaceId)
        {
            var controller =
                CommunicationControllerFactory.CreateController("Management Services\\FetchExplorerItemsServiceByType");
            return controller.ExecuteCommand<IExplorerItem>(Connection, workSpaceId);
        }

        public IExplorerRepositoryResult RenameItem(IExplorerItem itemToRename, string newName, Guid workSpaceId)
        {
            var controller = CommunicationControllerFactory.CreateController("RenameItemService");
            if(itemToRename.Children != null)
            {
                var any = itemToRename.Children.Where(a => a.ResourceType == ResourceType.Version);
                itemToRename.Children = itemToRename.Children.Except(any).ToList();
            }
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("itemToRename", serializer.SerializeToBuilder(itemToRename).ToString());
            controller.AddPayloadArgument("newName", newName);
            return controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, workSpaceId);
        }

        public IExplorerRepositoryResult RenameFolder(string path, string newName, Guid workSpaceId)
        {
            var controller = CommunicationControllerFactory.CreateController("RenameFolderService");
            controller.AddPayloadArgument("path", path);
            controller.AddPayloadArgument("newPath", newName);
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
            if (itemToMove.Children != null)
            {
                var any = itemToMove.Children.Where(a => a.ResourceType == ResourceType.Version);
                itemToMove.Children = itemToMove.Children.Except(any).ToList();
            }
            var serializer = new Dev2JsonSerializer();
            controller.AddPayloadArgument("itemToMove", serializer.SerializeToBuilder(itemToMove).ToString());
            controller.AddPayloadArgument("newPath", newPath);
            return controller.ExecuteCommand<IExplorerRepositoryResult>(Connection, workSpaceId);
        }

        public string GetServerVersion()
        {
            var controller = CommunicationControllerFactory.CreateController("GetServerVersion");
            var version =  controller.ExecuteCommand<string>(Connection, Guid.Empty);
            if(String.IsNullOrEmpty(version))
                return "less than 0.4.19.1";
            return version;
        }
    }
}
