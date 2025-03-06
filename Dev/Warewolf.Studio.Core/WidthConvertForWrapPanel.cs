#pragma warning disable
using System;
using System.Globalization;
#if WINDOWS
using System.Windows.Data;
#endif

namespace Warewolf.Studio.Core
{
    public class WidthConvertForWrapPanel
#if WINDOWS
        : IValueConverter
#endif
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (double)value - 10;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}