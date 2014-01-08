using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class IntInRangeToVisibilityConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values == null || values.Length < 3)
            {
                return Visibility.Visible;
            }

            var value = (int)values[0];
            var min = 0;
            if(values[1] is int)
            {
                min = (int)values[1];
            }
            var max = int.MaxValue;
            if(values[2] is int)
            {
                max = (int)values[2];
            }

            if(min == max)
            {
                return Visibility.Visible;
            }
            return value >= min && value <= max ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}