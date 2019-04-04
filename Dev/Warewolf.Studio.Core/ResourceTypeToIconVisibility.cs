#pragma warning disable
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Warewolf.Studio.Core
{
    public class ResourceTypeToIconVisibility : IValueConverter
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
            if (value != null)
            {
                var resourceType = value.ToString();
                if (resourceType != "Folder")
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;

        #endregion
    }
}