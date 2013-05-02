using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Dev2.Common;
using Unlimited.Framework;

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
            return Dev2EnumConverter.GetEnumFromStringValue(value.ToString(), targetType);
        }
    }
}
