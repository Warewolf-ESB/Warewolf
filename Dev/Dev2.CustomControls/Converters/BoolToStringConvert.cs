using System;
using System.Globalization;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
{
    public class BoolToStringConvert : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isSuccessful = value as bool?;
            if (isSuccessful.HasValue)
            {
                return isSuccessful.Value ? "Success" : "Failure";
            }
            return "Failure";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
