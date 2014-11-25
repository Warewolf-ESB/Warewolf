
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Navigation
{
    /// <summary>
    ///     Implemented by treeviewitems with a context menu
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/01/23</date>
    public interface IContextCommands
    {
        bool HasExecutableCommands { get; }
        bool CanBuild { get; }
        bool CanDebug { get; }
        bool CanEdit { get; }
        bool CanManualEdit { get; }
        bool CanRun { get; }
        bool CanDelete { get; }
        bool CanHelp { get; }
        bool CanShowDependencies { get; }
        bool CanShowProperties { get; }
        bool CanCreateWizard { get; }
        bool CanEditWizard { get; }
        bool CanDeploy { get; }
        bool CanRemove { get; }
        bool CanDisconnect { get; }
        bool CanConnect { get; }

        ICommand BuildCommand { get; }
        ICommand DebugCommand { get; }
        ICommand EditCommand { get; }
        ICommand ManualEditCommand { get; }
        ICommand RunCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand HelpCommand { get; }
        ICommand ShowDependenciesCommand { get; }
        ICommand ShowPropertiesCommand { get; }
        ICommand CreateWizardCommand { get; }
        ICommand EditWizardCommand { get; }
        ICommand DeployCommand { get; }
        ICommand RemoveCommand { get; }
        ICommand DisconnectCommand { get; }
        ICommand ConnectCommand { get; }
        bool HasFileMenu { get; }
        ICommand NewResourceCommand { get; }
    }
}
