using System;
using System.Globalization;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class StringToAutomationIdConverter : IValueConverter
    {
        private readonly string _autoIdPrefix = "UI_";
        private readonly string _autoIdSufix = "_AutoID";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;

            if(!string.IsNullOrEmpty(s))
            {
                return string.Concat(_autoIdPrefix, s, _autoIdSufix);
            }
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
