using System;
using System.Globalization;

using System.Windows.Data;

namespace Dev2.AppResources.Converters
{
    public class DateTimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is DateTime))
            {
                return Binding.DoNothing;
            }

            DateTime dateTime = (DateTime)value;

            return dateTime.ToString("yyyy/MM/dd hh:mm:ss tt");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
