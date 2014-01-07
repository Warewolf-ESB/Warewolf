using System;
using System.Globalization;
using System.Windows.Data;

namespace Dev2.Studio.AppResources.Converters
{
    public class NullToDebugImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return StringResources.Pack_Uri_Debug_Image;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}