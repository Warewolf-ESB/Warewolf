using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class StringToVisibilityConverterMulti : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = values.Cast<string>().Aggregate(false, (current, theValue) => !current && !string.IsNullOrEmpty(theValue));

            bool invert;
            var invertStr = parameter as string;
            bool.TryParse(invertStr, out invert);
            if(invert)
            {
                isVisible = !isVisible;
            }
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}
