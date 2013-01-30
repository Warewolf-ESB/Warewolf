using System;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class BooleanInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is bool))
            {
                return Binding.DoNothing;
            }

            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is bool))
            {
                return Binding.DoNothing;
            }

            return !((bool)value);
        }
    }
}
