#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Dev2.Studio.Interfaces.Enums
{
    public enum WorkSurfaceContext
    {
        Unknown,
        Workflow,
        Service,
        SourceManager,

        //TODO: Remove once Triggers is the only entry point.
        [Description("Scheduler")]
        Scheduler,

        [Description("Triggers")]
        Triggers,

        [Description("Settings")]
        Settings,

        [Description("Dependency Visualiser")]
        DependencyVisualiser,

        [Description("Deploy")]
        DeployViewer,

        [Description("Start Page")]
        StartPage,

        Help,

        EmailSource,
        ServerSource,
        SqlServerSource,
		SqliteSource,
		MySqlSource,
        PostgreSqlSource,
        OracleSource,
        OdbcSource,
        OAuthSource,
        WebSource,
        RedisSource,
        ElasticsearchSource,
        PluginSource,
        ComPluginSource,
        SharepointServerSource,
        Exchange,
        RabbitMQSource,
        WcfSource,
        ServiceTestsViewer,
        MergeConflicts,
        SearchViewer
    }
}