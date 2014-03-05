using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev2.AppResources.Converters
{
    public class PaddingThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var theValue = value as string;

            if(string.IsNullOrEmpty(theValue))
            {
                return new Thickness(0, 0, 0, 0);
            }

            return new Thickness(2, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
