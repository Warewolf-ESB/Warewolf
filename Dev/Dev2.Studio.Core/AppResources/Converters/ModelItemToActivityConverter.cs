/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Globalization;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class ModelItemToActivityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var modelItem = value as ModelItem;

            return modelItem;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ModelItem modelItem)
            {
                var currentValue = modelItem.GetCurrentValue();
                if (currentValue is DsfDatabaseActivity databaseActivity)
                {
                    return databaseActivity;
                }
                if (currentValue is DsfPluginActivity pluginActivity)
                {
                    return pluginActivity;
                }
                var act = currentValue as Activity;
                return act;
            }
            return null;
        }

        #endregion Implementation of IValueConverter
    }
}