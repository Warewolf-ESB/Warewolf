using System;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        #region Properties

        public string Format { get; set; }

        #endregion Properties

        #region Override Methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(!(value is TimeSpan))
            {
                return Binding.DoNothing;
            }

            TimeSpan timeSpan = (TimeSpan)value;

            if(string.IsNullOrWhiteSpace(Format))
            {
                return timeSpan.ToString();
            }
            return timeSpan.ToString(Format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion Override Mehods
    }
}
