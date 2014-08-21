using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Dev2.ConnectionHelpers;
using Dev2.Threading;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Dev2.Models
{
    public interface IExplorerItemModel
    {
        bool CanCreateNewFolder { get; }
        bool IsNew { get; set; }
        bool CanSelectDependencies { get; }
        bool IsAuthorizedDeployFrom { get; }
        bool IsLocalHost { get; }
        int ChildrenCount { get; }
        string ActivityName { get; }
        Guid EnvironmentId { get; set; }
        IExplorerItemModel Parent { get; set; }
        string DisplayName { get; set; }
        Guid ResourceId { get; set; }
        ResourceType ResourceType { get; set; }
        ObservableCollection<IExplorerItemModel> Children { get; set; }
        Permissions Permissions { get; set; }
        bool IsExplorerExpanded { get; set; }
        bool IsResourcePickerExpanded { get; set; }
        bool IsDeploySourceExpanded { get; set; }
        bool IsDeployTargetExpanded { get; set; }
        bool IsExplorerSelected { get; set; }
        bool IsResourcePickerSelected { get; set; }
        bool IsDeploySourceSelected { get; set; }
        bool IsDeployTargetSelected { get; set; }
        bool IsConnected { get; set; }
        bool IsRenaming { get; set; }
        bool IsRefreshing { get; set; }
        IVersionInfo VersionInfo { get; set; }
        string DisplayNameValidationRegex { get; }
        bool CanAddResoure { get; }
        bool CanDebug { get; }
        bool CanRename { get; }
        bool CanRemove { get; }
        bool CanDelete { get; }
        bool CanEdit { get; }
        bool CanExecute { get; }
        bool CanConnect { get; }
        bool CanDeploy { get; }
        bool CanShowDependencies { get; }
        bool CanShowHistory { get; }
        string ToggleVersionHistoryHeader { get; set; }
        bool CanDisconnect { get; }
        string DeployTitle { get; }
        string ResourcePath { get; set; }
        bool IsAuthorized { get; set; }
        ICommand NewFolderCommand { get; set; }
        ICommand RollbackCommand { get;}
        ICommand RefreshCommand { get; set; }
        ICommand NewResourceCommand { get; }
        ICommand DeleteVersionCommand { get; }
        /// <summary>
        /// Gets the debug command.
        /// </summary>
        /// <value>
        /// The debug command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        ICommand DebugCommand { get; }
        /// <summary>
        /// Gets the show dependencies command.
        /// </summary>
        /// <value>
        /// The show dependencies command.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        ICommand ShowDependenciesCommand { get; }
        /// <summary>
        /// Gets the remove command.
        /// </summary>
        /// <value>
        /// The remove command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        ICommand RemoveCommand { get; }
        /// <summary>
        /// Gets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        ICommand DeleteCommand { get; }
        /// <summary>
        /// Gets the toggle version history command.
        /// </summary>
        /// <value>
        /// The the toggle version history command.
        /// </value>
        /// <author>Tshepo Ntlhokoa</author>
        ICommand ToggleVersionHistoryCommand { get; }
        /// <summary>
        /// Gets the rename command.
        /// </summary>
        /// <value> 
        /// The delete command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        ICommand RenameCommand { get; set; }
        /// <summary>
        /// Gets the deploy command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        ICommand DeployCommand { get; }
        /// <summary>
        /// Gets the edit command.
        /// </summary>
        /// <value>
        /// The edit command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        ICommand EditCommand { get; }
        /// <summary>
        /// Gets the connect command.
        /// </summary>
        /// <value>
        /// The connect command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        ICommand ConnectCommand { get; }
        /// <summary>
        /// Gets the disconnect command.
        /// </summary>
        /// <value>
        /// The disconnect command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        ICommand DisconnectCommand { get; }
        bool? IsChecked { get; set; }
        bool IsOverwrite { get; set; }
        bool IsAuthorizedDeployTo { get; }
        IAsyncWorker AsyncWorker { get; }

        void SetDisplay(string display);

        void OnChildrenChanged();

        ExplorerItemModel Clone();

        ExplorerItemModel Clone(IConnectControlSingleton connectControlSingleton, IStudioResourceRepository studioResourceRepository);

        void AddNewFolder();

        [ExcludeFromCodeCoverage]
        void CancelRename(KeyEventArgs eventArgs);

        void CancelRename();

        /// <summary>
        ///     Sets the IsChecked Property and updates children and parent
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="updateChildren">
        ///     if set to <c>true</c> [update children].
        /// </param>
        /// <param name="updateParent">
        ///     if set to <c>true</c> [update parent].
        /// </param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent);

        /// <summary>
        ///     Verifies the state of the IsChecked property by taking the childrens IsChecked State into account
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void VerifyCheckState();
        void RefreshName(string newName);
    }
}