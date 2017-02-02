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
using System.Activities;
using System.Activities.Presentation.Model;
using System.Globalization;
using System.Windows.Data;
using Dev2.Activities;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class ModelItemToActivityConverter : IValueConverter
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
            ModelItem modelItem = value as ModelItem;

            return modelItem;
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
            ModelItem modelItem = value as ModelItem;

            if(modelItem != null)
            {
                var currentValue = modelItem.GetCurrentValue();
                var databaseActivity = currentValue as DsfDatabaseActivity;
                if(databaseActivity != null)
                {
                    return databaseActivity;
                }
                var pluginActivity = currentValue as DsfPluginActivity;
                if(pluginActivity != null)
                {
                    return pluginActivity;
                }
                var webServiceActivity = currentValue as DsfWebserviceActivity;
                if(webServiceActivity != null)
                {
                    return webServiceActivity;
                }
                var act = currentValue as Activity;
                return act;
            }
            return null;
        }

        #endregion
    }
}
