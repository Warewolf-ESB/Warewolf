using Dev2.Data.Enums;
using System;
using System.Windows;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    [ValueConversion(typeof(enForEachType), typeof(Visibility))]
    public class FindRecordsTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value != null && (string)value == "Not Contains" || (string)value == "Contains" || (string)value == "Equal" || (string)value == "Not Equal" || (string)value == "Ends With" || (string)value == "Starts With" || (string)value == "Regex" || (string)value == ">" || (string)value == "<" || (string)value == "<=" || (string)value == ">=")
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
