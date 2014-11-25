
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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Dev2.ConnectionHelpers;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Threading;

namespace Dev2.Models
{
    /// <summary>
    /// tree datastructure. binds to treeviews in explorer
    /// </summary>
    public interface IExplorerItemModel
    {
        /// <summary>
        /// Can create child folder
        /// </summary>
        bool CanCreateNewFolder { get; }

        /// <summary>
        /// Has been created from explorer
        /// </summary>
        bool IsNew { get; set; }

        /// <summary>
        /// Can select dependencies. Deploy
        /// </summary>
        bool CanSelectDependencies { get; }

        /// <summary>
        /// Can deploy
        /// </summary>
        bool IsAuthorizedDeployFrom { get; }

        /// <summary>
        /// Is local server
        /// </summary>
        bool IsLocalHost { get; }
        /// <summary>
        /// Number of children
        /// </summary>
        int ChildrenCount { get; }


        string ActivityName { get; }
        
        /// <summary>
        /// Server 
        /// </summary>
        Guid EnvironmentId { get; set; }

        /// <summary>
        /// Parent
        /// </summary>
        IExplorerItemModel Parent { get; set; }

        /// <summary>
        /// Name in explorer
        /// </summary>

        string DisplayName { get; set; }

        /// <summary>
        /// Resource Identified
        /// </summary>
        Guid ResourceId { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        ResourceType ResourceType { get; set; }

        /// <summary>
        /// Children
        /// </summary>
        ObservableCollection<IExplorerItemModel> Children { get; set; }

        /// <summary>
        /// aggregated permissions
        /// </summary>
        Permissions Permissions { get; set; }

        /// <summary>
        /// Is expanded in explorer
        /// </summary>
        bool IsExplorerExpanded { get; set; }

        /// <summary>
        /// Expanded when this item is hosted in resource picker
        /// </summary>
        bool IsResourcePickerExpanded { get; set; }

        /// <summary>
        /// expanded in deploy
        /// </summary>
        bool IsDeploySourceExpanded { get; set; }
        /// <summary>
        /// expanded in target
        /// </summary>
        bool IsDeployTargetExpanded { get; set; }

        /// <summary>
        /// Is selected
        /// </summary>
        bool IsExplorerSelected { get; set; }

        /// <summary>
        /// Is selected in resource picker
        /// </summary>
        bool IsResourcePickerSelected { get; set; }

        /// <summary>
        /// Is selected in deploy as source
        /// </summary>
        bool IsDeploySourceSelected { get; set; }

        /// <summary>
        /// Is selected in deploy as target
        /// </summary>
        bool IsDeployTargetSelected { get; set; }

        /// <summary>
        /// Is connected. only if server
        /// </summary>
        bool IsConnected { get; set; }

        /// <summary>
        /// Is in a renaming state
        /// </summary>
        bool IsRenaming { get; set; }

        /// <summary>
        /// Is being updated or refreshed
        /// </summary>
        bool IsRefreshing { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        IVersionInfo VersionInfo { get; set; }

        /// <summary>
        /// Validation of display name
        /// </summary>
        string DisplayNameValidationRegex { get; }

        /// <summary>
        /// Can add children to this
        /// </summary>
        bool CanAddResoure { get; }

        /// <summary>
        /// Can debug from explorer
        /// </summary>
        bool CanDebug { get; }

        /// <summary>
        /// can rename from explorer
        /// </summary>
        bool CanRename { get; }
        /// <summary>
        /// can remove from explorer
        /// </summary>
        bool CanRemove { get; }
        /// <summary>
        /// can delete from explorer
        /// </summary>
        bool CanDelete { get; }

        /// <summary>
        /// can edite from explorer
        /// </summary>
        bool CanEdit { get; }

        /// <summary>
        /// can exectue from explorer
        /// </summary>
        bool CanExecute { get; }

        /// <summary>
        /// can connect. valid if server
        /// </summary>
        bool CanConnect { get; }

        /// <summary>
        /// can this be deployed
        /// </summary>
        bool CanDeploy { get; }

        /// <summary>
        /// Can sho dependencies. must be checked for this to be available
        /// </summary>
        bool CanShowDependencies { get; }

        /// <summary>
        /// can this have versions
        /// </summary>
        bool CanShowHistory { get; }

        /// <summary>
        /// Version string if no versions
        /// </summary>
        string ToggleVersionHistoryHeader { get; set; }

        /// <summary>
        /// can be disconnected
        /// </summary>
        bool CanDisconnect { get; }

        /// <summary>
        /// Title for deploy
        /// </summary>
        string DeployTitle { get; }

        /// <summary>
        /// relative path of server
        /// </summary>
        string ResourcePath { get; set; }
        /// <summary>
        /// Is authorised
        /// </summary>
        bool IsAuthorized { get; set; }

        /// <summary>
        /// Create new folder method binding
        /// </summary>
        ICommand NewFolderCommand { get; set; }
        /// <summary>
        /// Rollback binding
        /// </summary>
        ICommand RollbackCommand { get;}
        /// <summary>
        /// refresh binding
        /// </summary>
        RelayCommand RefreshCommand { get; set; }
        /// <summary>
        /// new binding
        /// </summary>
        ICommand NewResourceCommand { get; }
        /// <summary>
        /// delete binding
        /// </summary>
        ICommand DeleteVersionCommand { get; }
        /// <summary>
        /// Gets the debug command.
        /// </summary>


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
        RelayCommand ConnectCommand { get; }
        /// <summary>
        /// Gets the disconnect command.
        /// </summary>
        /// <value>
        /// The disconnect command.
        /// </value>
        /// <author>Massimo Guerrera</author>
        RelayCommand DisconnectCommand { get; }

        /// <summary>
        /// Is checked used for deploy
        /// </summary>
        bool? IsChecked { get; set; }
        /// <summary>
        /// Is overwrite. used for deploy
        /// </summary>
        bool IsOverwrite { get; set; }
        /// <summary>
        /// Has deploy permissions
        /// </summary>
        bool IsAuthorizedDeployTo { get; }

        /// <summary>
        /// Asyncworkr
        /// </summary>
        IAsyncWorker AsyncWorker { get; }
        /// <summary>
        /// Is this Item a version rather than a resource
        /// </summary>
        bool IsVersion { get; }

        /// <summary>
        /// Update display name
        /// </summary>
        /// <param name="display"></param>
        void SetDisplay(string display);

        /// <summary>
        /// Child changed
        /// </summary>
        void OnChildrenChanged();

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        ExplorerItemModel Clone();

        /// <summary>
        /// Clone with injection
        /// </summary>
        /// <param name="connectControlSingleton"></param>
        /// <param name="studioResourceRepository"></param>
        /// <returns></returns>
        ExplorerItemModel Clone(IConnectControlSingleton connectControlSingleton, IStudioResourceRepository studioResourceRepository);

        /// <summary>
        /// Creates a sub folder with default name. Folder goes into renaming
        /// </summary>
        void AddNewFolder();

        [ExcludeFromCodeCoverage]
        void CancelRename(KeyEventArgs eventArgs);

        /// <summary>
        /// cancel renaming
        /// </summary>
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
        /// <param name="calcStats"></param>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent,bool calcStats=true);

        /// <summary>
        ///     Verifies the state of the IsChecked property by taking the childrens IsChecked State into account
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/01/23</date>
        void VerifyCheckState();

        /// <summary>
        /// Refresh name 
        /// </summary>
        /// <param name="newName"></param>
        void RefreshName(string newName);

        /// <summary>
        /// remove selected child
        /// </summary>
        /// <param name="child"></param>
        void RemoveChild(IExplorerItemModel child);

        /// <summary>
        /// Udate item category with new category
        /// </summary>
        /// <param name="category"></param>
        void UpdateCategoryIfOpened(string category);
        /// <summary>
        /// Get the resource path without name
        /// </summary>
        string ResourcePathWithoutName { get; }
    }
}
