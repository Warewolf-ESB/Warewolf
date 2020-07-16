/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Security;
using Dev2.Studio.Interfaces;

namespace Dev2.Studio.ViewModels
{
    public class WarewolfCommands
    {
        public WarewolfCommands(bool createDesigners, ShellViewModel shellViewModel)
        {
            ShellViewModel = shellViewModel;
            _worksurfaceContextManager = new WorksurfaceContextManager(createDesigners, shellViewModel);
        }

        private static ShellViewModel _shellViewModel;
        public static ShellViewModel ShellViewModel
        {
            get => _shellViewModel;
            set => _shellViewModel = value;
        }

        private static IWorksurfaceContextManager _worksurfaceContextManager;

        public IWorksurfaceContextManager WorksurfaceContextManager
        {
            get => _worksurfaceContextManager;
            set => _worksurfaceContextManager = value;
        }

        private static bool IsActiveServerConnected() => ShellViewModel.IsActiveServerConnected();

        private static AuthorizeCommand<string> _newServiceCommand;
        public static IAuthorizeCommand<string> NewServiceCommand => _newServiceCommand ?? (_newServiceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewService(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newPluginSourceCommand;
        public static IAuthorizeCommand<string> NewPluginSourceCommand => _newPluginSourceCommand ?? (_newPluginSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewPluginSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newSqlServerSourceCommand;
        public static IAuthorizeCommand<string> NewSqlServerSourceCommand => _newSqlServerSourceCommand ?? (_newSqlServerSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewSqlServerSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newMySqlSourceCommand;
        public static IAuthorizeCommand<string> NewMySqlSourceCommand => _newMySqlSourceCommand ?? (_newMySqlSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewMySqlSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newPostgreSqlSourceCommand;
        public static IAuthorizeCommand<string> NewPostgreSqlSourceCommand => _newPostgreSqlSourceCommand ?? (_newPostgreSqlSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewPostgreSqlSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newOracleSourceCommand;
        public static IAuthorizeCommand<string> NewOracleSourceCommand => _newOracleSourceCommand ?? (_newOracleSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewOracleSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newOdbcSourceCommand;
        public static IAuthorizeCommand<string> NewOdbcSourceCommand => _newOdbcSourceCommand ?? (_newOdbcSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewOdbcSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newWebSourceCommand;
        public static IAuthorizeCommand<string> NewWebSourceCommand => _newWebSourceCommand ?? (_newWebSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewWebSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newRedisSourceCommand;
        public static IAuthorizeCommand<string> NewRedisSourceCommand => _newRedisSourceCommand ?? (_newRedisSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewRedisSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newElasticsearchSourceCommand;
        public static IAuthorizeCommand<string> NewElasticsearchSourceCommand => _newElasticsearchSourceCommand ?? (_newElasticsearchSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewElasticsearchSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newServerSourceCommand;
        public static IAuthorizeCommand<string> NewServerSourceCommand => _newServerSourceCommand ?? (_newServerSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewServerSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newEmailSourceCommand;
        public static IAuthorizeCommand<string> NewEmailSourceCommand => _newEmailSourceCommand ?? (_newEmailSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewEmailSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newExchangeSourceCommand;
        public static IAuthorizeCommand<string> NewExchangeSourceCommand => _newExchangeSourceCommand ?? (_newExchangeSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewExchangeSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newRabbitMQSourceCommand;
        public static IAuthorizeCommand<string> NewRabbitMQSourceCommand => _newRabbitMQSourceCommand ?? (_newRabbitMQSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewRabbitMQSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newSharepointSourceCommand;
        public static IAuthorizeCommand<string> NewSharepointSourceCommand => _newSharepointSourceCommand ?? (_newSharepointSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewSharepointSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newDropboxSourceCommand;
        public static IAuthorizeCommand<string> NewDropboxSourceCommand => _newDropboxSourceCommand ?? (_newDropboxSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewDropboxSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand<string> _newWcfSourceCommand;
        public static IAuthorizeCommand<string> NewWcfSourceCommand => _newWcfSourceCommand ?? (_newWcfSourceCommand =
            new AuthorizeCommand<string>(AuthorizationContext.Contribute, param =>
                ShellViewModel.NewWcfSource(@""), param => IsActiveServerConnected()));

        private static AuthorizeCommand _schedulerCommand;
        public static IAuthorizeCommand SchedulerCommand => _schedulerCommand ?? (_schedulerCommand =
            new AuthorizeCommand(AuthorizationContext.Administrator, param =>
                _worksurfaceContextManager.AddSchedulerWorkSurface(), param => IsActiveServerConnected()));

        private static AuthorizeCommand _queueEventsCommand;
        public static IAuthorizeCommand QueueEventsCommand => _queueEventsCommand ?? (_queueEventsCommand =
            new AuthorizeCommand(AuthorizationContext.Administrator, param =>
                _worksurfaceContextManager.AddQueuesWorkSurface(), param => IsActiveServerConnected()));

        private static AuthorizeCommand _tasksCommand;
        public static IAuthorizeCommand TasksCommand => _tasksCommand ?? (_tasksCommand =
            new AuthorizeCommand(AuthorizationContext.Administrator, param =>
                _worksurfaceContextManager.AddTriggersWorkSurface(), param => IsActiveServerConnected()));

        private static AuthorizeCommand _settingsCommand;
        public static IAuthorizeCommand SettingsCommand => _settingsCommand ?? (_settingsCommand =
            new AuthorizeCommand(AuthorizationContext.Administrator, param =>
                _worksurfaceContextManager.AddSettingsWorkSurface(), param => IsActiveServerConnected()));

        private static ICommand _deployCommand;
        public static ICommand DeployCommand => _deployCommand ?? (_deployCommand =
            new RelayCommand(param => ShellViewModel.AddDeploySurface(new List<IExplorerTreeItem>())));

        private static ICommand _runCoverageCommand;
        public static ICommand RunCoverageCommand => _runCoverageCommand ?? (_runCoverageCommand = new DelegateCommand(ShellViewModel.RunCoverage));

        private static ICommand _runAllTestsCommand;
        public static ICommand RunAllTestsCommand => _runAllTestsCommand ?? (_runAllTestsCommand = new DelegateCommand(ShellViewModel.RunAllTests));

        private static ICommand _addWorkflowCommand;
        public static ICommand AddWorkflowCommand => _addWorkflowCommand ?? (_addWorkflowCommand = new DelegateCommand(ShellViewModel.OpenResourcePicker));

        private static ICommand _searchCommand;
        public static ICommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(ShellViewModel.ShowSearchWindow));

        private static ICommand _showCommunityPageCommand;
        public static ICommand ShowCommunityPageCommand => _showCommunityPageCommand ?? (_showCommunityPageCommand = new DelegateCommand(param => ShellViewModel.ShowCommunityPage()));

        private static ICommand _showStartPageCommand;
        public static ICommand ShowStartPageCommand => _showStartPageCommand ?? (_showStartPageCommand = new DelegateCommand(param => ShellViewModel.ShowStartPageAsync()));

        private static ICommand _exitCommand;
        public static ICommand ExitCommand => _exitCommand ?? (_exitCommand =
            new RelayCommand(param => Application.Current.Shutdown(), param => true));
    }
}