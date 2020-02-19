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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Studio.Interfaces
{
    public interface IExplorerItemViewModel : IExplorerTreeItem
    {
        bool Checked { get; set; }
        bool IsRenaming{ get; set; }
        bool IsVisible { get; set; }
        bool CanExecute { get; set; }
        bool CanEdit { get; set; }
        bool CanView { get; set; }
        bool CanShowDependencies { get; set; }
        bool IsVersion { get; set; }
        bool CanViewSwagger { get; set; }
        bool CanMerge { get; set; }
        bool CanDuplicate { get; set; }
        bool CanCreateTest { get; set; }

        string VersionNumber { get; set; }
        string VersionHeader { get; set; }
        string ExecuteToolTip { get; }
        string EditToolTip { get; }
        string ActivityName { get; }

        ICommand ViewSwaggerCommand { get; set; }
        ICommand MergeCommand { get; set; }
        ICommand OpenCommand { get; set; }
        ICommand DeleteVersionCommand { get; set; }
        ICommand ShowDependenciesCommand { get; set; }
        ICommand LostFocus { get; set; }
        bool CanMove { get; }
        ICommand DuplicateCommand { get; set; }
        ICommand CreateTestCommand { get; set; }
        bool CanDebugInputs { get; set; }
        bool CanDebugStudio { get; set; }
        bool CanDebugBrowser { get; set; }
        bool CanCreateSchedule { get; set; }
        bool CanCreateQueueEvent { get; set; }
        bool CanContribute { get; set; }
        IVersionInfo VersionInfo { get; set; }

        IEnumerable<IExplorerItemViewModel> AsList();

        Task<bool> MoveAsync(IExplorerTreeItem destination);
    
        void AddSibling(IExplorerItemViewModel sibling);
        void CreateNewFolder();
        void Apply(Action<IExplorerItemViewModel> action);
        void Filter(string filter);
        void Filter(Func<IExplorerItemViewModel, bool> filter);
        void ShowErrorMessage(string errorMessage, string header);

        void ShowDependencies();

        void SetPermissions(Permissions explorerItemPermissions);

        void SetPermissions(Permissions explorerItemPermissions, bool isDeploy);
        void SetIsResourceChecked(bool? isResource);
        void AfterResourceChecked();
    }
}