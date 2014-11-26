/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
{
    public class StringToTimespanConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>
        ///     Converts a TimeSpan to a string.
        /// </summary>
        /// <returns>
        ///     The string representation of the minutes in a TimeSpan.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            if (value is TimeSpan)
            {
                TimeSpan time;
                if (TimeSpan.TryParse(value.ToString(), out time))
                {
                    result = time.Minutes.ToString(CultureInfo.InvariantCulture);
                }
            }

            return result;
        }

        /// <summary>
        ///     Converts a string to a TimeSpan object as the minutes.
        /// </summary>
        /// <returns>
        ///     A TimeSpan object that has its minutes set to the string passed in.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = new TimeSpan();
            int inVal;

            if (int.TryParse(value.ToString(), out inVal))
            {
                result = new TimeSpan(0, inVal, 0);
            }

            return result;
        }

        #endregion
    }
}