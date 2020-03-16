/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerTooltips : BindableBase, IExplorerTooltips
    {
        string _newServiceTooltip;
        string _newServerSourceTooltip;
        string _newSqlServerSourceTooltip;
        string _newMySqlSourceTooltip;
        string _newPostgreSqlSourceTooltip;
        string _newOracleSourceTooltip;
        string _newOdbcSourceTooltip;
        string _newWebSourceTooltip;
        string _newRedisSourceTooltip;
        string _newPluginSourceTooltip;
        string _newComPluginSourceTooltip;
        string _newEmailSourceTooltip;
        string _newExchangeSourceTooltip;
        string _newRabbitMqSourceTooltip;
        string _newDropboxSourceTooltip;
        string _newSharepointSourceTooltip;
        string _newElasticsearchSourceTooltip;
        string _debugInputsTooltip;
        string _debugStudioTooltip;
        string _debugBrowserTooltip;
        string _scheduleTooltip;
        string _queueEventTooltip;
        string _newFolderTooltip;
        string _renameTooltip;
        string _deleteTooltip;
        string _duplicateTooltip;
        string _createTestTooltip;
        string _runAllTestsTooltip;
        string _deployTooltip;
        string _dependenciesTooltip;
        string _viewSwaggerTooltip;
        string _viewApisJsonTooltip;
        string _showHideVersionsTooltip;
        string _rollbackTooltip;
        string _openTooltip;
        string _newWcfSourceTooltip;
        string _viewExecutionLoggingTooltip;
        string _serverVersionTooltip;
        string _deployResourceCheckboxTooltip;
        private string _mergeTooltip;

        public void SetSourceTooltips(bool canCreateSource)
        {
            var noPermissionsToolTip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            NewServerSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewServerSourceTooltip : noPermissionsToolTip;
            NewWebSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewWebSourceTooltip : noPermissionsToolTip;
            NewRedisSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewRedisSourceTooltip : noPermissionsToolTip;
            NewRabbitMqSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewRabbitMqSourceTooltip : noPermissionsToolTip;
            NewDropboxSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewDropboxSourceTooltip : noPermissionsToolTip;
            NewSharepointSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewSharepointSourceTooltip : noPermissionsToolTip;
            NewWcfSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewWcfSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;

            SetEmailTooltips(canCreateSource, noPermissionsToolTip);
            SetDatabaseTooltips(canCreateSource, noPermissionsToolTip);
            SetDllTooltips(canCreateSource, noPermissionsToolTip);
        }

        void SetEmailTooltips(bool canCreateSource, string noPermissionsToolTip)
        {
            NewEmailSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewEmailSourceTooltip : noPermissionsToolTip;
            NewExchangeSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewExchangeSourceTooltip : noPermissionsToolTip;
        }

        void SetDllTooltips(bool canCreateSource, string noPermissionsToolTip)
        {
            NewPluginSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewPluginSourceTooltip : noPermissionsToolTip;
            NewComPluginSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewComPluginSourceTooltip : noPermissionsToolTip;
        }

        void SetDatabaseTooltips(bool canCreateSource, string noPermissionsToolTip)
        {
            NewSqlServerSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewSqlServerSourceTooltip : noPermissionsToolTip;
            NewMySqlSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewMySqlSourceTooltip : noPermissionsToolTip;
            NewPostgreSqlSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewPostgreSqlSourceTooltip : noPermissionsToolTip;
            NewOracleSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewOracleSourceTooltip : noPermissionsToolTip;
            NewOdbcSourceTooltip = canCreateSource ? Resources.Languages.Tooltips.NewOdbcSourceTooltip : noPermissionsToolTip;
        }

        public string NewServiceTooltip
        {
            get => _newServiceTooltip;
            set
            {
                _newServiceTooltip = value;
                OnPropertyChanged(() => NewServiceTooltip);
            }
        }
        public string NewServerSourceTooltip
        {
            get => _newServerSourceTooltip;
            set
            {
                _newServerSourceTooltip = value;
                OnPropertyChanged(() => NewServerSourceTooltip);
            }
        }
        public string NewSqlServerSourceTooltip
        {
            get => _newSqlServerSourceTooltip;
            set
            {
                _newSqlServerSourceTooltip = value;
                OnPropertyChanged(() => NewSqlServerSourceTooltip);
            }
        }
        public string NewMySqlSourceTooltip
        {
            get => _newMySqlSourceTooltip;
            set
            {
                _newMySqlSourceTooltip = value;
                OnPropertyChanged(() => NewMySqlSourceTooltip);
            }
        }
        public string NewPostgreSqlSourceTooltip
        {
            get => _newPostgreSqlSourceTooltip;
            set
            {
                _newPostgreSqlSourceTooltip = value;
                OnPropertyChanged(() => NewPostgreSqlSourceTooltip);
            }
        }
        public string NewOracleSourceTooltip
        {
            get => _newOracleSourceTooltip;
            set
            {
                _newOracleSourceTooltip = value;
                OnPropertyChanged(() => NewOracleSourceTooltip);
            }
        }
        public string NewOdbcSourceTooltip
        {
            get => _newOdbcSourceTooltip;
            set
            {
                _newOdbcSourceTooltip = value;
                OnPropertyChanged(() => NewOdbcSourceTooltip);
            }
        }
        public string NewWebSourceTooltip
        {
            get => _newWebSourceTooltip;
            set
            {
                _newWebSourceTooltip = value;
                OnPropertyChanged(() => NewWebSourceTooltip);
            }
        }
        public string NewRedisSourceTooltip
        {
            get => _newRedisSourceTooltip;
            set
            {
                _newRedisSourceTooltip = value;
                OnPropertyChanged(() => NewRedisSourceTooltip);
            }
        }
        public string NewPluginSourceTooltip
        {
            get => _newPluginSourceTooltip;
            set
            {
                _newPluginSourceTooltip = value;
                OnPropertyChanged(() => NewPluginSourceTooltip);
            }
        }
        public string NewComPluginSourceTooltip
        {
            get => _newComPluginSourceTooltip;
            set
            {
                _newComPluginSourceTooltip = value;
                OnPropertyChanged(() => NewComPluginSourceTooltip);
            }
        }
        public string NewWcfSourceTooltip
        {
            get => _newWcfSourceTooltip;
            set
            {
                _newWcfSourceTooltip = value;
                OnPropertyChanged(() => NewWcfSourceTooltip);
            }
        }
        public string NewEmailSourceTooltip
        {
            get => _newEmailSourceTooltip;
            set
            {
                _newEmailSourceTooltip = value;
                OnPropertyChanged(() => NewEmailSourceTooltip);
            }
        }
        public string NewExchangeSourceTooltip
        {
            get => _newExchangeSourceTooltip;
            set
            {
                _newExchangeSourceTooltip = value;
                OnPropertyChanged(() => NewExchangeSourceTooltip);
            }
        }
        public string NewRabbitMqSourceTooltip
        {
            get => _newRabbitMqSourceTooltip;
            set
            {
                _newRabbitMqSourceTooltip = value;
                OnPropertyChanged(() => NewRabbitMqSourceTooltip);
            }
        }
        public string NewDropboxSourceTooltip
        {
            get => _newDropboxSourceTooltip;
            set
            {
                _newDropboxSourceTooltip = value;
                OnPropertyChanged(() => NewDropboxSourceTooltip);
            }
        }
        public string NewSharepointSourceTooltip
        {
            get => _newSharepointSourceTooltip;
            set
            {
                _newSharepointSourceTooltip = value;
                OnPropertyChanged(() => NewSharepointSourceTooltip);
            }
        }

        public string NewElasticsearchSourceTooltip
        {
            get => _newElasticsearchSourceTooltip;
            set
            {
                _newElasticsearchSourceTooltip = value;
                OnPropertyChanged(() => NewElasticsearchSourceTooltip);
            }
        }

        public string DebugInputsTooltip
        {
            get => _debugInputsTooltip;
            set
            {
                _debugInputsTooltip = value;
                OnPropertyChanged(() => DebugInputsTooltip);
            }
        }
        public string DebugStudioTooltip
        {
            get => _debugStudioTooltip;
            set
            {
                _debugStudioTooltip = value;
                OnPropertyChanged(() => DebugStudioTooltip);
            }
        }
        public string DebugBrowserTooltip
        {
            get => _debugBrowserTooltip;
            set
            {
                _debugBrowserTooltip = value;
                OnPropertyChanged(() => DebugBrowserTooltip);
            }
        }
        public string ScheduleTooltip
        {
            get => _scheduleTooltip;
            set
            {
                _scheduleTooltip = value;
                OnPropertyChanged(() => ScheduleTooltip);
            }
        }
        public string QueueEventTooltip
        {
            get => _queueEventTooltip;
            set
            {
                _queueEventTooltip = value;
                OnPropertyChanged(() => QueueEventTooltip);
            }
        }
        public string NewFolderTooltip
        {
            get => _newFolderTooltip;
            set
            {
                _newFolderTooltip = value;
                OnPropertyChanged(() => NewFolderTooltip);
            }
        }
        public string RenameTooltip
        {
            get => _renameTooltip;
            set
            {
                _renameTooltip = value;
                OnPropertyChanged(() => RenameTooltip);
            }
        }
        public string DeleteTooltip
        {
            get => _deleteTooltip;
            set
            {
                _deleteTooltip = value;
                OnPropertyChanged(() => DeleteTooltip);
            }
        }
        public string DuplicateTooltip
        {
            get => _duplicateTooltip;
            set
            {
                _duplicateTooltip = value;
                OnPropertyChanged(() => DuplicateTooltip);
            }
        }
        public string CreateTestTooltip
        {
            get => _createTestTooltip;
            set
            {
                _createTestTooltip = value;
                OnPropertyChanged(() => CreateTestTooltip);
            }
        }
        public string RunAllTestsTooltip
        {
            get => _runAllTestsTooltip;
            set
            {
                _runAllTestsTooltip = value;
                OnPropertyChanged(() => RunAllTestsTooltip);
            }
        }
        public string DeployTooltip
        {
            get => _deployTooltip;
            set
            {
                _deployTooltip = value;
                OnPropertyChanged(() => DeployTooltip);
            }
        }
        public string DependenciesTooltip
        {
            get => _dependenciesTooltip;
            set
            {
                _dependenciesTooltip = value;
                OnPropertyChanged(() => DependenciesTooltip);
            }
        }
        public string ViewSwaggerTooltip
        {
            get => _viewSwaggerTooltip;
            set
            {
                _viewSwaggerTooltip = value;
                OnPropertyChanged(() => ViewSwaggerTooltip);
            }
        }
        public string ViewApisJsonTooltip
        {
            get => _viewApisJsonTooltip;
            set
            {
                _viewApisJsonTooltip = value;
                OnPropertyChanged(() => ViewApisJsonTooltip);
            }
        }
        public string ShowHideVersionsTooltip
        {
            get => _showHideVersionsTooltip;
            set
            {
                _showHideVersionsTooltip = value;
                OnPropertyChanged(() => ShowHideVersionsTooltip);
            }
        }
        public string RollbackTooltip
        {
            get => _rollbackTooltip;
            set
            {
                _rollbackTooltip = value;
                OnPropertyChanged(() => RollbackTooltip);
            }
        }
        public string OpenTooltip
        {
            get => _openTooltip;
            set
            {
                _openTooltip = value;
                OnPropertyChanged(() => OpenTooltip);
            }
        }
        public string ViewExecutionLoggingTooltip
        {
            get => _viewExecutionLoggingTooltip;
            set
            {
                _viewExecutionLoggingTooltip = value;
                OnPropertyChanged(() => ViewExecutionLoggingTooltip);
            }
        }
        public string ServerVersionTooltip
        {
            get => _serverVersionTooltip;
            set
            {
                _serverVersionTooltip = value;
                OnPropertyChanged(() => ServerVersionTooltip);
            }
        }
        public string DeployResourceCheckboxTooltip
        {
            get => _deployResourceCheckboxTooltip;
            set
            {
                _deployResourceCheckboxTooltip = value;
                OnPropertyChanged(() => DeployResourceCheckboxTooltip);
            }
        }
        public string MergeTooltip
        {
            get => _mergeTooltip;
            set
            {
                _mergeTooltip = value;
                OnPropertyChanged(() => MergeTooltip);
            }
        }
    }
}