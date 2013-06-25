using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public NullToVisibilityConverter()
        {
            NullVisibilityValue = Visibility.Collapsed;
            NotNullVisibilityValue = Visibility.Visible;
        }

        public Visibility NullVisibilityValue
        {
            get;
            set;
        }

        public Visibility NotNullVisibilityValue
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return NullVisibilityValue;
            }

            return NotNullVisibilityValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
