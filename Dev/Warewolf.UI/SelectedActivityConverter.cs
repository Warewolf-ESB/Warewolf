/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2;
using Dev2.Activities;
using Dev2.Studio.Interfaces;

namespace Warewolf.UI
{
    public class SelectedActivityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ModelItem modelItem)
            {
                var currentValue = modelItem.GetCurrentValue();
                var act = currentValue as Activity;
                var modelItem1 = Dev2.Studio.Core.Activities.Utils.ModelItemUtils.CreateModelItem(act);
                return modelItem1;
            }

            return Binding.DoNothing;
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
                var modelItem1 = Dev2.Studio.Core.Activities.Utils.ModelItemUtils.CreateModelItem(act);
                return modelItem1;
            }
            return null;
        }
    }
}
