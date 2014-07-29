using Dev2.Interfaces;
using Dev2.Models;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using System;
using System.Collections.ObjectModel;

namespace Dev2.AppResources.Repositories
{
    public interface IStudioResourceRepository
    {
        ObservableCollection<ExplorerItemModel> ExplorerItemModels { get; }
        void Load(Guid environmentId, IAsyncWorker asyncWorker);
        void Load(Guid environmentId, IAsyncWorker asyncWorker, Action<Guid> onCompletion);
        void Disconnect(Guid environmentId);
        void Connect(Guid environmentId);
        void RemoveEnvironment(Guid environmentId);
        void DeleteItem(Guid environmentId, Guid resourceId);
        void DeleteItem(ExplorerItemModel explorerItem);
        void AddServerNode(ExplorerItemModel explorerItem);
        void RenameItem(ExplorerItemModel explorerItem, string newName);
        void RenameFolder(ExplorerItemModel explorerItem, string newName);
        void AddItem(ExplorerItemModel item);
        ExplorerItemModel FindItemById(Guid id);
        ExplorerItemModel FindItemByIdAndEnvironment(Guid id, Guid environmentId);
        ObservableCollection<ExplorerItemModel> Filter(Func<ExplorerItemModel, bool> filter);
        ExplorerItemModel FindItem(Func<ExplorerItemModel, bool> searchCriteria);
        void UpdateItem(Guid id, Action<ExplorerItemModel> action, Guid environmentId);
        void AddResouceItem(IContextualResourceModel resourceModel);
        void ItemAddedMessageHandler(IExplorerItem item);
        void DeleteFolder(ExplorerItemModel item);
        void UpdateRootAndFoldersPermissions(Permissions modifiedPermissions, Guid guid, bool updateRoot = true);
        void PerformUpdateOnDispatcher(Action action);
    }
}