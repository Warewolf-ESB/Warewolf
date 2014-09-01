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
    public class ServerExplorerClientProxy : IExplorerResourceRepository
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
    }
}
