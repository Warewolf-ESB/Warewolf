using System;
using System.Globalization;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class StringToIntInRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = value as int?;
            if (intValue != null && intValue == 0)
            {
                return string.Empty;
            }
            var parameterString = parameter as string;
            if (!string.IsNullOrWhiteSpace(parameterString))
            {
                var stringValue = parameterString.Split('-');
                var minValue = int.Parse(stringValue[0]);
                var maxValue = int.Parse(stringValue[1]);
                if (intValue >= minValue && intValue <= maxValue)
                {
                    return value;
                }
                if (intValue > minValue || intValue > maxValue)
                {
                    return value;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const int intValue = 0;
            var stringValue = value as string;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return intValue;
            }

            return value;
        }
    }
}
