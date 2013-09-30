using System;
using System.Globalization;
using System.Windows.Data;

namespace Dev2.Activities.Designers2.Core.Converters
{
    public class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is bool))
            {
                return Binding.DoNothing;
            }

            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is bool))
            {
                return Binding.DoNothing;
            }
            return !((bool)value);
        }
    }
}
