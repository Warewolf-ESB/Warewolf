#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = false;
            if(value is int)
            {
                var count = (int)value;
                if(count > 0)
                {
                    isVisible = true;
                }
            }

            var negate = false;
            if(parameter is bool)
            {
                negate = (bool)parameter;
            }

            if(negate)
            {
                isVisible = !isVisible;
            }
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}