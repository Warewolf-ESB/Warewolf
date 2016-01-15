using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Warewolf.Studio.CustomControls
{
    public class DebugConverter : IValueConverter
    {
        public object Convert(object value, Type targeType, object parameter, CultureInfo culture)
        {
            Debugger.Break();
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debugger.Break();
            return value;
        }
    }
}
