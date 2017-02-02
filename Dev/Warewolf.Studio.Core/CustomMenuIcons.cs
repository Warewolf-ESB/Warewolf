using System.Collections.Generic;

namespace Warewolf.Studio.Core
{
    public static class CustomMenuIcons
    {
        private enum MenuIcons
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
            ExchangeSource
        }

        private static readonly Dictionary<MenuIcons, string> MenuIconsDictionary = new Dictionary<MenuIcons, string>
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
            {MenuIcons.StartPage, "Fa-Home"}
        };

        public static string Server => MenuIconsDictionary[MenuIcons.Server];

        public static string ServerSource => MenuIconsDictionary[MenuIcons.ServerSource];

        public static string WorkflowService => MenuIconsDictionary[MenuIcons.WorkflowService];

        public static string DbService => MenuIconsDictionary[MenuIcons.DbService];

        public static string PluginService => MenuIconsDictionary[MenuIcons.PluginService];

        public static string WebService => MenuIconsDictionary[MenuIcons.WebService];

        public static string Folder => MenuIconsDictionary[MenuIcons.Folder];

        public static string Spacer => MenuIconsDictionary[MenuIcons.Spacer];

        public static string Execute => MenuIconsDictionary[MenuIcons.Execute];

        public static string View => MenuIconsDictionary[MenuIcons.View];

        public static string SqlServerSource => MenuIconsDictionary[MenuIcons.SqlServerSource];

        public static string MySqlSource => MenuIconsDictionary[MenuIcons.MySqlSource];

        public static string PostgreSqlSource => MenuIconsDictionary[MenuIcons.PostgreSqlSource];

        public static string OracleSource => MenuIconsDictionary[MenuIcons.OracleSource];

        public static string OdbcSource => MenuIconsDictionary[MenuIcons.OdbcSource];

        public static string PluginSource => MenuIconsDictionary[MenuIcons.PluginSource];

        public static string EmailSource => MenuIconsDictionary[MenuIcons.EmailSource];

        public static string ExchangeSource => MenuIconsDictionary[MenuIcons.ExchangeSource];

        public static string RabbitMqSource => MenuIconsDictionary[MenuIcons.RabbitMqSource];

        public static string WebSource => MenuIconsDictionary[MenuIcons.WebSource];

        public static string StartPage => MenuIconsDictionary[MenuIcons.StartPage];
    }
}