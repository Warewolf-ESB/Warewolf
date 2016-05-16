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
            DbSource,
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
            {MenuIcons.RabbitMqSource, "EmailSourceImageLogo"},
            {MenuIcons.DbSource, "DatabaseSource"},
            {MenuIcons.ServerSource, "System-Logo-Create"},
            {MenuIcons.Server, "System-Logo"},
            {MenuIcons.StartPage, "Fa-Home"}
        };

        public static string Server
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.Server];
            }
        }

        public static string ServerSource
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.ServerSource];
            }
        }

        public static string WorkflowService
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.WorkflowService];
            }
        }

        public static string DbService
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.DbService];
            }
        }

        public static string PluginService
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.PluginService];
            }
        }

        public static string WebService
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.WebService];
            }
        }

        public static string Folder
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.Folder];
            }
        }

        public static string Spacer
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.Spacer];
            }
        }

        public static string Execute
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.Execute];
            }
        }

        public static string View
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.View];
            }
        }

        public static string DbSource
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.DbSource];
            }
        }

        public static string PluginSource
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.PluginSource];
            }
        }

        public static string EmailSource
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.EmailSource];
            }
        }

        public static string ExchangeSource
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.ExchangeSource];
            }
        }

        public static string RabbitMqSource
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.RabbitMqSource];
            }
        }
        
        public static string WebSource
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.WebSource];
            }
        }

        public static string StartPage
        {
            get
            {
                return MenuIconsDictionary[MenuIcons.StartPage];
            }
        }

    }
}