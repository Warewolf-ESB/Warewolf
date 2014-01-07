using System;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class NullableBoolToBoolConverter : IValueConverter
    {
        public bool NullValueReplacement { get; set; }

        public NullableBoolToBoolConverter()
        {
            NullValueReplacement = true;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var b = value as bool?;
            return b ?? NullValueReplacement;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
