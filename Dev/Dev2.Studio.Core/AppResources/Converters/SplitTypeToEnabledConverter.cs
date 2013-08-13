using System;
using System.Windows;
using System.Windows.Data;
using Dev2.Common;
using Dev2.Data.Enums;

namespace Dev2.Studio.Core.AppResources.Converters
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class SplitTypeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;
            if (value != null)
            {
                string stringVal = value.ToString();
                if (stringVal == "Index" || stringVal == "Chars")
                {
                    result = true;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}