/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;

// ReSharper disable InconsistentNaming

namespace Dev2.Interfaces
{
    public interface IMainViewModel
    {
        ICommand DeployCommand { get; }
        ICommand ExitCommand { get; }
        IEnvironmentModel ActiveEnvironment { get; set; }
        IContextualResourceModel DeployResource { get; set; }

        void AddWorkSurfaceContext(IContextualResourceModel resourceModel);

        bool MenuExpanded { get; set; }
        double MenuPanelWidth { get; set; }
        AuthorizeCommand SaveCommand { get; }
        AuthorizeCommand DebugCommand { get; }
        AuthorizeCommand SettingsCommand { get; }
        AuthorizeCommand SchedulerCommand { get; }
        IToolboxViewModel ToolboxViewModel { get; }
        IHelpWindowViewModel HelpViewModel { get; }
        ICommand ShowStartPageCommand { get; }
        AuthorizeCommand<string> NewServiceCommand { get; }
        AuthorizeCommand<string> NewPluginSourceCommand { get; }
        AuthorizeCommand<string> NewSqlServerSourceCommand { get; }
        AuthorizeCommand<string> NewMySqlSourceCommand { get; }
        AuthorizeCommand<string> NewPostgreSqlSourceCommand { get; }
        AuthorizeCommand<string> NewOracleSourceCommand { get; }
        AuthorizeCommand<string> NewOdbcSourceCommand { get; }
        AuthorizeCommand<string> NewWebSourceCommand { get; }
        AuthorizeCommand<string> NewServerSourceCommand { get; }
        AuthorizeCommand<string> NewEmailSourceCommand { get; }
        AuthorizeCommand<string> NewExchangeSourceCommand { get; }
        AuthorizeCommand<string> NewRabbitMQSourceCommand { get; }
        AuthorizeCommand<string> NewSharepointSourceCommand { get; }
        AuthorizeCommand<string> NewDropboxSourceCommand { get; }
        AuthorizeCommand<string> NewWcfSourceCommand { get; }
        IExplorerViewModel ExplorerViewModel { get; set; }

        void DisplayDialogForNewVersion();

        Task<bool> CheckForNewVersion();

        bool ShowDeleteDialogForFolder(string folderBeingDeleted);
        IWorkflowDesignerViewModel CreateNewDesigner(IContextualResourceModel resourceModel);
    }
}
