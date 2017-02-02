/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Warewolf.Studio.Core;

namespace Dev2.AppResources.Converters
{
    public class ExplorerItemModelToIconConverter : IMultiValueConverter
    {
        ResourceDictionary _dict;

        #region Implementation of IMultiValueConverter

        public ExplorerItemModelToIconConverter()
        {
            const string pathname = "/Warewolf.Studio.Themes.Luna;component/Images.xaml";
             _dict = Application.LoadComponent(new Uri(pathname, UriKind.Relative)) as ResourceDictionary;
        }

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method when it propagates the values from source bindings to the binding target.
        /// </summary>
        /// <returns>
        /// A converted value.If the method returns null, the valid null value is used.A return value of <see cref="T:System.Windows.DependencyProperty"/>.<see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the converter did not produce a value, and that the binding will use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> if it is available, or else will use the default value.A return value of <see cref="T:System.Windows.Data.Binding"/>.<see cref="F:System.Windows.Data.Binding.DoNothing"/> indicates that the binding does not transfer the value or use the <see cref="P:System.Windows.Data.BindingBase.FallbackValue"/> or the default value.
        /// </returns>
        /// <param name="values">The array of values that the source bindings in the <see cref="T:System.Windows.Data.MultiBinding"/> produces. The value <see cref="F:System.Windows.DependencyProperty.UnsetValue"/> indicates that the source binding has no value to provide for conversion.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            string resourceType = values[0].ToString();
            switch(resourceType)
            {
                case "WorkflowService":
                    return _dict[CustomMenuIcons.WorkflowService] as DrawingImage;
                case "DbService":
                    return _dict[CustomMenuIcons.DbService] as DrawingImage;
                case "PluginService":
                    return _dict[CustomMenuIcons.PluginService] as DrawingImage;
                case "WebService":
                    return _dict[CustomMenuIcons.WebService] as DrawingImage;
                case "SqlServerSource":
                    return _dict[CustomMenuIcons.SqlServerSource] as DrawingImage;
                case "MySqlSource":
                    return _dict[CustomMenuIcons.MySqlSource] as DrawingImage;
                case "PostgreSqlSource":
                    return _dict[CustomMenuIcons.PostgreSqlSource] as DrawingImage;
                case "OracleSource":
                    return _dict[CustomMenuIcons.OracleSource] as DrawingImage;
                case "OdbcSource":
                    return _dict[CustomMenuIcons.OdbcSource] as DrawingImage;
                case "PluginSource":
                    return _dict[CustomMenuIcons.PluginSource] as DrawingImage;
                case "WebSource":
                    return _dict[CustomMenuIcons.WebSource] as DrawingImage;
                case "EmailSource":
                    return _dict[CustomMenuIcons.EmailSource] as DrawingImage;
                case "ServerSource":
                    return _dict[CustomMenuIcons.ServerSource] as DrawingImage;
                case "Server":
                    return Application.Current.Resources["System-Logo"];
                case "Version":
                case "Message":
                    return null;
                case "Folder":
                    return _dict[CustomMenuIcons.Folder] as DrawingImage;
                case "OauthSource ":
                    return Application.Current.Resources["Dropbox"];
                case "SharepointServerSource":
                    return Application.Current.Resources["AddSharepointBlackLogo"];
                default:
                    return _dict[CustomMenuIcons.WorkflowService] as DrawingImage;
            }
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <returns>
        /// An array of values that have been converted from the target value back to the source values.
        /// </returns>
        /// <param name="value">The value that the binding target produces.</param><param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { };
        }

        #endregion
    }
}
