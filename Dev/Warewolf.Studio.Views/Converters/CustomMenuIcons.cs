
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
using Infragistics.Controls.Menus;

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
            Folder = 16, //cool
            Spacer = 99,
            Execute = 98,
            View = 97
        }

        private static readonly Dictionary<MenuIcons, string> MenuIconsDictionary = new Dictionary<MenuIcons, string>
        {
            {MenuIcons.WorkflowService, "Explorer-WorkflowService"},
            {MenuIcons.DbService, "Explorer-DB"},
            {MenuIcons.PluginService, "Explorer-DLL"},
            {MenuIcons.WebService, "Explorer-WebService"},
            {MenuIcons.Folder, "Explorer-Spacer"},
            {MenuIcons.Spacer, "Explorer-Spacer"},
            {MenuIcons.View, "Explorer-Permission-Disbled"},
            {MenuIcons.Execute, "Explorer-Run-Disabled"},

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
                switch (resourceType)
                {
                    case ResourceType.WorkflowService:
                        return dict[CustomMenuIcons.WorkflowService] as DrawingImage;
                    case ResourceType.DbService:
                        return dict[CustomMenuIcons.DbService] as DrawingImage;
                    case ResourceType.PluginService:
                        return dict[CustomMenuIcons.PluginService] as DrawingImage;
                    case ResourceType.WebService:
                        return dict[CustomMenuIcons.WebService] as DrawingImage;
                    default:
                        return dict[CustomMenuIcons.Folder] as DrawingImage;
                }
            }
            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Returns the exact length of the requred menu node based on its tree position
    /// </summary>
    public class WidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            return (double)values[1] - (((XamDataTreeNodeControl)values[0]).Node.Manager.Level * 21) - 50;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ViewFolderIcon : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        /// Returns the width for the folder ICON. if it is a folder return 0;
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
                            if (resourceType == ResourceType.Folder)
                                return "Collapsed";
                        }

            return "Visible";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


}
