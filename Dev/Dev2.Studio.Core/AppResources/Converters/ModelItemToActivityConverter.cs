/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using System;
using System.Activities;
using System.Globalization;
#if WINDOWS || NETFRAMEWORK
using System.Activities.Presentation.Model;
using System.Windows.Data;
#endif

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class ModelItemToActivityConverter
#if WINDOWS || NETFRAMEWORK
        : IValueConverter
#endif
    {
        public
#if !(WINDOWS || NETFRAMEWORK)
            static
#endif
            object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var modelItem = value
#if WINDOWS || NETFRAMEWORK
                as ModelItem
#endif
                ;
            return modelItem;
        }

        public
#if !(WINDOWS || NETFRAMEWORK)
            static
#endif
            object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if WINDOWS || NETFRAMEWORK
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
#endif

            if (value is null && parameter?.ToString() == "Resume")
            {
                return new DsfSequenceActivity();
            }
            return null;
        }
    }
}