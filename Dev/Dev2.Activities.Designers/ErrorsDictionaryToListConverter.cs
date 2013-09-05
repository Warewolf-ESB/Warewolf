using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Dev2.Providers.Errors;

namespace Dev2.Activities
{
    public class ErrorsDictionaryToListConverter : IValueConverter
    {
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
            var propertyName = parameter as string;
            if(String.IsNullOrEmpty(propertyName)) return new List<IErrorInfo>();
            var errorsDictionary = value as Dictionary<string, List<IActionableErrorInfo>>;
            if (errorsDictionary == null || errorsDictionary.Count == 0) return new List<IErrorInfo>();
            List<IActionableErrorInfo> errorsList;
            if(errorsDictionary.TryGetValue(propertyName, out errorsList))
            {
                return errorsList;
            }
            return new List<IErrorInfo>();
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
            return null;
        }

        #endregion
    }
}