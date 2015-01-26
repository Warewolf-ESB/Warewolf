
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
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Dev2.Common.Interfaces.Data;

namespace Warewolf.Studio.Views.Converters
{
    public static class CustomMenuIcons
    {
        private enum MenuIcons
        {
            WorkflowService = 1, //sux
            DbService = 2, //cool
            PluginService = 4, 
            WebService = 8, //cool
            Folder= 16 //cool
        }

        private static readonly Dictionary<MenuIcons, string> MenuIconsDictionary = new Dictionary<MenuIcons, string>
        {
            {MenuIcons.WorkflowService, "Explorer-WorkflowService"},
            {MenuIcons.DbService, "Explorer-DB"},
            {MenuIcons.PluginService, "Explorer-DLL"},
            {MenuIcons.WebService, "Explorer-WebService"},
            {MenuIcons.Folder, "Explorer-Spacer.xaml"},
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

            const string Pathname = "/Warewolf.Studio.Themes.Luna;component/Images.xaml";


            ResourceDictionary dict = Application.LoadComponent(new Uri(Pathname, System.UriKind.Relative)) as ResourceDictionary;

            ResourceType resourceType;
            if (Enum.TryParse(value.ToString(), out resourceType))
            {
                switch(resourceType)
                {
                    case ResourceType.WorkflowService:
                        return dict[CustomMenuIcons.WorkflowService] as DrawingImage;
                    case ResourceType.DbService:
                        return dict[CustomMenuIcons.DbService] as DrawingImage;
                    case ResourceType.PluginService:
                        return dict[CustomMenuIcons.PluginService] as DrawingImage;
                    case ResourceType.WebService:
                        return dict[CustomMenuIcons.WebService] as DrawingImage;
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
                        return dict[CustomMenuIcons.Folder] as DrawingImage;
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
