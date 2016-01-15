using System;
using System.Globalization;
using System.Windows.Data;

namespace Warewolf.Studio.Core.Converters
{
    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value-12;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
