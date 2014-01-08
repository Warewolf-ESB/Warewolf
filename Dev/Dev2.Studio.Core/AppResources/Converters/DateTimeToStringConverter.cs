using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class DateTimeToStringConverter : DependencyObject, IValueConverter
    {
        #region Properties

        public string Format { get; set; }

        #endregion Properties

        #region Override Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is DateTime))
            {
                return Binding.DoNothing;
            }

            DateTime dateTime = (DateTime)value;

            if(string.IsNullOrWhiteSpace(Format))
            {
                return dateTime.ToString(CultureInfo.InvariantCulture);
            }
            return dateTime.ToString(Format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion Override Mehods
    }
}
