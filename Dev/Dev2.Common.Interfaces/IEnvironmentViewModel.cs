/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Common.Interfaces.Explorer;

namespace Dev2.Common.Interfaces
{
    public interface IEnvironmentViewModel : IExplorerTreeItem
    {
        string DisplayName { get; set; }
        bool IsConnected { get; }
        bool IsLoaded { get; }

        bool Connect();

        bool IsConnecting { get; set; }
        bool IsServerIconVisible { get; set; }
        bool IsServerUnavailableIconVisible { get; set; }
        bool IsVisible { get; set; }

        ICommand RefreshCommand { get; set; }
        ICommand ShowServerVersionCommand { get; set; }

        ICollection<IExplorerItemViewModel> AsList();

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void SetItemCheckedState(Guid id, bool state);

        void SelectItem(string selectedPath, Action<IExplorerItemViewModel> foundAction);

        void RemoveItem(IExplorerItemViewModel vm);

        void Filter(string filter);

        void Filter(Func<IExplorerItemViewModel, bool> filter);

        void SetPropertiesForDialog();

        void SetPropertiesForDialogFromPermissions(IWindowsGroupPermission permissions);

        Action SelectAll { get; set; }
        ObservableCollection<IExplorerItemViewModel> UnfilteredChildren { get; set; }

        Task<bool> Load(bool isDeploy = false,bool reloadCatalogue = false);

        Task<bool> LoadDialog(string selectedId, bool b = false, bool reloadCatalogue = false);

        Task<bool> LoadDialog(Guid selectedPath);
        IExplorerTreeItem FindByPath(string path);

        ObservableCollection<IExplorerItemViewModel> CreateExplorerItemModels(IEnumerable<IExplorerItem> explorerItems, IServer server, IExplorerTreeItem parent, bool isDialog, bool isDeploy);
    }
}