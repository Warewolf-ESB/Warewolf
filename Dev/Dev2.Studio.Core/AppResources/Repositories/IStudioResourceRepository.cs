using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Dev2.AppResources.Repositories
{
    public interface IStudioResourceRepository
    {
        ObservableCollection<IExplorerItemModel> ExplorerItemModels { get; }
        void Load(Guid environmentId, IAsyncWorker asyncWorker);
        void Load(Guid environmentId, IAsyncWorker asyncWorker, Action<Guid> onCompletion);
        void Disconnect(Guid environmentId);
        void Connect(Guid environmentId);
        void RemoveEnvironment(Guid environmentId);
        void DeleteItem(Guid environmentId, Guid resourceId);
        void DeleteItem(IExplorerItemModel explorerItem);
        void AddServerNode(IExplorerItemModel explorerItem);
        void RenameItem(IExplorerItemModel explorerItem, string newName);
        void RenameFolder(IExplorerItemModel explorerItem, string newName);
        void AddItem(IExplorerItemModel item);
        IExplorerItemModel FindItemById(Guid id);
        IExplorerItemModel FindItemByIdAndEnvironment(Guid id, Guid environmentId);
        ObservableCollection<IExplorerItemModel> Filter(Func<IExplorerItemModel, bool> filter);
        IExplorerItemModel FindItem(Func<IExplorerItemModel, bool> searchCriteria);
        void UpdateItem(Guid id, Action<IExplorerItemModel> action, Guid environmentId);
        void AddResouceItem(IContextualResourceModel resourceModel);
        void ItemAddedMessageHandler(IExplorerItem item);
        void DeleteFolder(IExplorerItemModel item);
        void UpdateRootAndFoldersPermissions(Permissions modifiedPermissions, Guid guid, bool updateRoot = true);
        void PerformUpdateOnDispatcher(Action action);
        void ShowVersionHistory(Guid environmentId, Guid resourceId);
        void HideVersionHistory(Guid environmentId, Guid resourceId);
        StringBuilder GetVersion(IVersionInfo versionInfo, Guid environmentId);
        void RollbackTo(IVersionInfo versionInfo, Guid environmentId);
        void DeleteVersion(IVersionInfo versionInfo, Guid environmentId);
        void RefreshVersionHistory(Guid environmentId, Guid resourceId);
    }
}