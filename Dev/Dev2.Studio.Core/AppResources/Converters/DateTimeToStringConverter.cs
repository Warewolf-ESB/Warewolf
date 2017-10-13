/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


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
                var customFormat = GlobalConstants.Dev2DotNetDefaultDateTimeFormat.Replace("ss", "ss.ffff");
                return dateTime.ToString(customFormat);
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
