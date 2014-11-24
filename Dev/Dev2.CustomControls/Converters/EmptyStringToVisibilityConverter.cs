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
using System.Windows;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
{
    [ValueConversion(typeof (string), typeof (Visibility))]
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public EmptyStringToVisibilityConverter()
        {
            EmptyStringVisiblity = Visibility.Collapsed;
        }

        public Visibility EmptyStringVisiblity { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return EmptyStringVisiblity;
            }

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return EmptyStringVisiblity;
            }
            return EmptyStringVisiblity == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}