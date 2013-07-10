using System;
using System.Globalization;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
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
