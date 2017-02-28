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
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Toolbox;

// ReSharper disable InconsistentNaming

namespace Dev2.Studio.Interfaces
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
        IAuthorizeCommand SaveCommand { get; }
        IAuthorizeCommand DebugCommand { get; }
        IAuthorizeCommand SettingsCommand { get; }
        IAuthorizeCommand SchedulerCommand { get; }
        IToolboxViewModel ToolboxViewModel { get; }
        IHelpWindowViewModel HelpViewModel { get; }
        ICommand ShowStartPageCommand { get; }
        IAuthorizeCommand<string> NewServiceCommand { get; }
        IAuthorizeCommand<string> NewPluginSourceCommand { get; }
        IAuthorizeCommand<string> NewSqlServerSourceCommand { get; }
        IAuthorizeCommand<string> NewMySqlSourceCommand { get; }
        IAuthorizeCommand<string> NewPostgreSqlSourceCommand { get; }
        IAuthorizeCommand<string> NewOracleSourceCommand { get; }
        IAuthorizeCommand<string> NewOdbcSourceCommand { get; }
        IAuthorizeCommand<string> NewWebSourceCommand { get; }
        IAuthorizeCommand<string> NewServerSourceCommand { get; }
        IAuthorizeCommand<string> NewEmailSourceCommand { get; }
        IAuthorizeCommand<string> NewExchangeSourceCommand { get; }
        IAuthorizeCommand<string> NewRabbitMQSourceCommand { get; }
        IAuthorizeCommand<string> NewSharepointSourceCommand { get; }
        IAuthorizeCommand<string> NewDropboxSourceCommand { get; }
        IAuthorizeCommand<string> NewWcfSourceCommand { get; }
        IExplorerViewModel ExplorerViewModel { get; set; }

        void DisplayDialogForNewVersion();

        Task<bool> CheckForNewVersion();

        bool ShowDeleteDialogForFolder(string folderBeingDeleted);
        IWorkflowDesignerViewModel CreateNewDesigner(IContextualResourceModel resourceModel);
    }
}
