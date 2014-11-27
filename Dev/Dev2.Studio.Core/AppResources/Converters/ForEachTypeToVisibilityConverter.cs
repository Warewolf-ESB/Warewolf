
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
using System.Windows;
using System.Windows.Data;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Data.Enums;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{

    [ValueConversion(typeof(enForEachType), typeof(Visibility))]
    public class ForEachTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
            {
                return Visibility.Collapsed;
            }

            var enumValue = Dev2EnumConverter.GetEnumFromStringDiscription(value as string, typeof(enForEachType));

            enForEachType visibleEnumValue;
            Enum.TryParse((string)parameter, out visibleEnumValue);

            if(visibleEnumValue.Equals(enumValue))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
