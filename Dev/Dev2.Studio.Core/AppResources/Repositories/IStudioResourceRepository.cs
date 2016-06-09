
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Text;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.AppResources.Repositories
{
    public interface IStudioResourceRepository
    {
        ObservableCollection<IExplorerItemModel> ExplorerItemModels { get; }
        void Load(Guid environmentId, IAsyncWorker asyncWorker, Action<Guid> onCompletion);
        void Disconnect(Guid environmentId);
        void RemoveEnvironment(Guid environmentId);
        void RenameItem(IExplorerItemModel explorerItem, string newName);
        void RenameFolder(IExplorerItemModel explorerItem, string newName);
        void AddItem(IExplorerItemModel item);
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
