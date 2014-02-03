using System;
using System.Windows;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public Visibility EmptyStringVisiblity { get; set; }

        public EmptyStringToVisibilityConverter()
        {
            EmptyStringVisiblity = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
            {
                return EmptyStringVisiblity;
            }

            if(string.IsNullOrWhiteSpace(value.ToString()))
            {
                return EmptyStringVisiblity;
            }
            return EmptyStringVisiblity == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
