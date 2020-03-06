/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Dev2.Studio.Interfaces
{
    public interface IExplorerTooltips
    {
        string NewServiceTooltip { get; set; }
        string NewServerSourceTooltip { get; set; }
        string NewSqlServerSourceTooltip { get; set; }
        string NewMySqlSourceTooltip { get; set; }
        string NewPostgreSqlSourceTooltip { get; set; }
        string NewOracleSourceTooltip { get; set; }
        string NewOdbcSourceTooltip { get; set; }
        string NewWebSourceTooltip { get; set; }
        string NewRedisSourceTooltip { get; set; }
        string NewPluginSourceTooltip { get; set; }
        string NewComPluginSourceTooltip { get; set; }
        string NewWcfSourceTooltip { get; set; }
        string NewEmailSourceTooltip { get; set; }
        string NewExchangeSourceTooltip { get; set; }
        string NewRabbitMqSourceTooltip { get; set; }
        string NewDropboxSourceTooltip { get; set; }
        string NewSharepointSourceTooltip { get; set; }
        string NewElasticsearchSourceTooltip { get; set; }
        string DebugInputsTooltip { get; set; }
        string DebugStudioTooltip { get; set; }
        string DebugBrowserTooltip { get; set; }
        string ScheduleTooltip { get; set; }
        string QueueEventTooltip { get; set; }
        string NewFolderTooltip { get; set; }
        string RenameTooltip { get; set; }
        string DeleteTooltip { get; set; }
        string DuplicateTooltip { get; set; }
        string CreateTestTooltip { get; set; }
        string RunAllTestsTooltip { get; set; }
        string DeployTooltip { get; set; }
        string DependenciesTooltip { get; set; }
        string ViewSwaggerTooltip { get; set; }
        string ViewApisJsonTooltip { get; set; }
        string ShowHideVersionsTooltip { get; set; }
        string RollbackTooltip { get; set; }
        string OpenTooltip { get; set; }
        string ViewExecutionLoggingTooltip { get; set; }
        string ServerVersionTooltip { get; set; }
        string DeployResourceCheckboxTooltip { get; set; }
        string MergeTooltip { get; set; }

        void SetSourceTooltips(bool canCreateSource);
    }
}
