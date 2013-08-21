using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Dev2.Common;
using Dev2.Data.Enums;

namespace Dev2.Studio.Core.AppResources.Converters
{

    [ValueConversion(typeof(enForEachType), typeof(Visibility))]
    public class ForEachTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
            {
                return Visibility.Collapsed;
            }

            var enumValue = Dev2EnumConverter.GetEnumFromStringDiscription(value as string, typeof(enForEachType));

            enForEachType visibleEnumValue;
            Enum.TryParse((string)parameter, out visibleEnumValue);

            if (visibleEnumValue.Equals(enumValue))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
