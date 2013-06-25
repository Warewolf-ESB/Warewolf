using System;
using System.Windows;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class TypeToVisibilityConverter : IValueConverter
    {
        public Type Type { get; set; }

        public Visibility MatchingVisibilityValue { get; set; }
        public Visibility MismatchVisibilityValue { get; set; }

        public TypeToVisibilityConverter()
        {
            MatchingVisibilityValue = Visibility.Visible;
            MismatchVisibilityValue = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value.GetType() != Type)
            {
                return MismatchVisibilityValue;
            }

            return MatchingVisibilityValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
