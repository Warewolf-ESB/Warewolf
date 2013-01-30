using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class BoolToValueConverter : IValueConverter
    {
        #region Properties

        public object TrueValue { get; set; }
        public object FalseValue { get; set; }

        #endregion Properties

        #region Methods

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? boolValue = value as bool?;

            if (boolValue == null)
            {
                return Binding.DoNothing;
            }

            if (boolValue.GetValueOrDefault())
            {
                return TrueValue;
            }

            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion Methods
    }
}
