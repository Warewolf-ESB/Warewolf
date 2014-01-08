using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    // Simplified
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var theValue = value as string;
            bool invert;
            var invertStr = parameter as string;
            bool.TryParse(invertStr, out invert);

            var result = string.IsNullOrEmpty(theValue) ? Visibility.Collapsed : Visibility.Visible;
            if(invert)
            {
                return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
