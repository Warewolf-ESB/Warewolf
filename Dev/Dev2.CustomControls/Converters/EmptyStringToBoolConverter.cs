using System;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class EmptyStringToBoolConverter : IValueConverter
    {
        public bool IsTrueWhenEmpty { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
            {
                return IsTrueWhenEmpty;
            }

            if(string.IsNullOrWhiteSpace(value.ToString()))
            {
                return IsTrueWhenEmpty;
            }
            return !IsTrueWhenEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
