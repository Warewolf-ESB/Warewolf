using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dev2.Scheduler.Interfaces;

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
