/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Windows.Data;


namespace Dev2.Studio.Core.AppResources.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        #region Properties

        public string Format { get; set; }

        #endregion Properties

        #region Override Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = value as TimeSpan? ?? throw new ArgumentException("value must be TimeSpan");
            if(string.IsNullOrWhiteSpace(Format))
            {
                return timeSpan.ToString();
            }
            return timeSpan.ToString(Format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        #endregion Override Mehods
    }
}
