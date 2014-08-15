using System;
using System.Globalization;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class StringToAutomationIdConverter : IValueConverter
    {
        const string AutoIdPrefix = "UI_";
        const string AutoIdSufix = "_AutoID";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;


            if(!string.IsNullOrEmpty(s))
            {
                //Ashley: Remove server address part of localhost strings
                s = s.Contains(@"localhost (http://") ? "localhost" : s;
                return string.Concat(AutoIdPrefix, s, AutoIdSufix);
            }
            return s;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
