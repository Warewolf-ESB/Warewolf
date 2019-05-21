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

using System.Collections.Generic;

namespace Warewolf.Studio.Core
{
    public static class CustomMenuIcons
    {
        enum MenuIcons
        {
            Unknown,
            WorkflowService,
            DbService,
            Version,
            PluginService,
            WebService,
            SqlServerSource,
            MySqlSource,
            PostgreSqlSource,
            OracleSource,
            OdbcSource,
            PluginSource,
            WebSource,
            EmailSource,
            OauthSource,
            ServerSource,
            Folder,
            Server,
            ReservedService,
            Message,
            Spacer,
            Execute,
            View,
            StartPage,
            RabbitMqSource,
            ExchangeSource,
            MergeConflicts
        }

        static readonly Dictionary<MenuIcons, string> MenuIconsDictionary = new Dictionary<MenuIcons, string>
        {
            {MenuIcons.WorkflowService, "Explorer-WorkflowService"},
            {MenuIcons.DbService, "Database"},
            {MenuIcons.PluginService, "DotNetDll"},
            {MenuIcons.WebService, "WebMethods"},
            {MenuIcons.Folder, "Explorer-Spacer"},
            {MenuIcons.Spacer, "Explorer-Spacer"},
            {MenuIcons.View, "Explorer-Permission-Disbled"},
            {MenuIcons.Execute, "Explorer-Run-Disabled"},
            {MenuIcons.WebSource, "WebMethodsSource"},
            {MenuIcons.PluginSource, "DotNetDllSource"},
            {MenuIcons.EmailSource, "EmailSourceImageLogo"},
            {MenuIcons.ExchangeSource, "ExchangeSource"},
            {MenuIcons.RabbitMqSource, "RabbitMqSource"},
            {MenuIcons.SqlServerSource, "MicrosoftSQLSource"},
            {MenuIcons.MySqlSource, "DatabaseSource"},
            {MenuIcons.PostgreSqlSource, "PostgreSource"},
            {MenuIcons.OracleSource, "DatabaseSource"},
            {MenuIcons.OdbcSource, "OdbcSource"},
            {MenuIcons.ServerSource, "System-Logo-Create"},
            {MenuIcons.Server, "System-Logo"},
            {MenuIcons.StartPage, "Fa-Home"},
            {MenuIcons.MergeConflicts, "Source-Merge-Logo"}
        };

        public static string ServerSource => MenuIconsDictionary[MenuIcons.ServerSource];

        public static string WorkflowService => MenuIconsDictionary[MenuIcons.WorkflowService];

        public static string Folder => MenuIconsDictionary[MenuIcons.Folder];

        public static string View => MenuIconsDictionary[MenuIcons.View];

        public static string SqlServerSource => MenuIconsDictionary[MenuIcons.SqlServerSource];

        public static string MySqlSource => MenuIconsDictionary[MenuIcons.MySqlSource];

        public static string OracleSource => MenuIconsDictionary[MenuIcons.OracleSource];

        public static string PluginSource => MenuIconsDictionary[MenuIcons.PluginSource];

        public static string EmailSource => MenuIconsDictionary[MenuIcons.EmailSource];

        public static string ExchangeSource => MenuIconsDictionary[MenuIcons.ExchangeSource];

        public static string RabbitMqSource => MenuIconsDictionary[MenuIcons.RabbitMqSource];

        public static string WebSource => MenuIconsDictionary[MenuIcons.WebSource];

        public static string MergeConflicts => MenuIconsDictionary[MenuIcons.MergeConflicts];
    }
}