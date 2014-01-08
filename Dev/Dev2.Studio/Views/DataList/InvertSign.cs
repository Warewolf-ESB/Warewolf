using System;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.UI.Controls
{
    [ValueConversion(typeof(int), typeof(int))]
    public class InvertSignConverter : IValueConverter
    {
        public static InvertSignConverter Instance = new InvertSignConverter();

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;
            return (val * -1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;
            return (val * -1);
        }

        #endregion
    }
}
