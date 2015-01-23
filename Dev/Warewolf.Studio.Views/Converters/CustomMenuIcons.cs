
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Dev2.Common.Interfaces.Data;


namespace Warewolf.Studio.Views.Converters
{
    public static class CustomMenuIcons
    {
        private enum MenuIcons
        {
            WorkflowService = 1,
            DbService = 2,
            PluginService = 4,
            WebService = 8,
            Folder= 16
        }

        private static readonly Dictionary<MenuIcons, string> MenuIconsDictionary = new Dictionary<MenuIcons, string>
        {
            {MenuIcons.WorkflowService, "{StaticResource Resources-Service-TreeIcon}"},
            {MenuIcons.DbService, "{StaticResource System-DB-TreeIcon}"},
            {MenuIcons.PluginService, "{StaticResource System-DLL-TreeIcon}"},
            {MenuIcons.WebService, "{StaticResource System-WebService-TreeIcon}"},
            {MenuIcons.Folder, "{StaticResource System-WebService-TreeIcon}"},
        };

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
    }

    public class ResourceTypeToIconConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceType resourceType;
            if (Enum.TryParse(value.ToString(), out resourceType))
            {
                switch(resourceType)
                {
                    case ResourceType.WorkflowService:
                        return CustomMenuIcons.WorkflowService;
                    case ResourceType.DbService:
                        return CustomMenuIcons.DbService;
                    case ResourceType.PluginService:
                        return CustomMenuIcons.PluginService;
                    case ResourceType.WebService:
                        return CustomMenuIcons.WebService;
//                    case ResourceType.DbSource:
//                        break;
//                    case ResourceType.PluginSource:
//                        break;
//                    case ResourceType.WebSource:
//                        break;
//                    case ResourceType.EmailSource:
//                        break;
//                    case ResourceType.OauthSource:
//                        break;
//                    case ResourceType.ServerSource:
//                        break;
//                    case ResourceType.Server:
//                        break;
                    default:
                        return CustomMenuIcons.Folder;
                }
            }
            return null;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

}
