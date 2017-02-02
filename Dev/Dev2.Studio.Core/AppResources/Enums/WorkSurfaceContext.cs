/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Enums
{
    public enum WorkSurfaceContext
    {
        Unknown,
        Workflow,
        Service,
        SourceManager,

        [Description("Scheduler")]
        Scheduler,

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
        MySqlSource,
        PostgreSqlSource,
        OracleSource,
        OdbcSource,
        OAuthSource,
        WebSource,
        PluginSource,
        ComPluginSource,
        SharepointServerSource,
        Exchange,
        // ReSharper disable once InconsistentNaming
        RabbitMQSource,
        WcfSource,
        ServiceTestsViewer
    }
}