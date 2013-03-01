using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class FilterStringToBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string != string.Empty);            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
