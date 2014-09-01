using System;
using System.Windows.Data;
using Dev2.Common.Interfaces.Enums.Enums;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Dev2EnumConverter.ConvertEnumValueToString(value as Enum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Dev2EnumConverter.GetEnumFromStringDiscription(value.ToString(), targetType);
        }
    }
}
