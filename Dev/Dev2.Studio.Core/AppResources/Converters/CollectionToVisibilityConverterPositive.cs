using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Dev2.AppResources.Converters
{
    public class CollectionToVisibilityConverter : IValueConverter
    {
        #region Properties

        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        #endregion Properties

        #region Implementation of IValueConverter

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? countValue = value as int?;

            if(countValue == null)
            {
                return Binding.DoNothing;
            }

            if(countValue.GetValueOrDefault() == 0)
            {
                return TrueValue;
            }

            return FalseValue;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}
