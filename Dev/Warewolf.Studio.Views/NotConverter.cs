using System;
using System.Globalization;
using System.Windows.Data;
using Infragistics.Controls.Primitives;

namespace Warewolf.Studio.Views
{
    public class NotConverter : IValueConverter
    {
        readonly BoolToVisibilityConverter _convertor;

        public NotConverter()
        {
            _convertor = new BoolToVisibilityConverter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _convertor.Convert( !(bool)value,targetType,parameter,culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _convertor.Convert(!(bool)value, targetType, parameter, culture);
        }
    }
}