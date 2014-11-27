
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
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Dev2.Studio.Core.AppResources.Converters
{
    public class AllBoolToVisibilityValueConverter : IMultiValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public AllBoolToVisibilityValueConverter()
        {
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values.All(v => !(v is bool)))
            {
                return TrueValue;
            }

            return values.Cast<bool>().All(b => b) ? TrueValue : FalseValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { };
        }
    }
}
