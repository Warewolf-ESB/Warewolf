
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
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dev2.Common.Interfaces.Scheduler.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.AppResources.Converters
{
    public class SchedulerStatusToCheckedConverter : DependencyObject, IValueConverter
    {
        #region Override Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string isEnabledRadioButton = parameter as string;
            SchedulerStatus schedulerStatus = (SchedulerStatus)value;
            if(isEnabledRadioButton == "true")
            {
                if(schedulerStatus == SchedulerStatus.Enabled)
                {
                    return true;
                }
                return false;
            }
            if(schedulerStatus == SchedulerStatus.Disabled)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string isEnabledRadioButton = parameter as string;
            if((isEnabledRadioButton == "true" && (bool)value) || (isEnabledRadioButton == "false" && !(bool)value))
            {
                return SchedulerStatus.Enabled;
            }
            return SchedulerStatus.Disabled;
        }

        #endregion Override Mehods
    }
}
