
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
using Dev2.Data.Enums;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    [ValueConversion(typeof(enForEachType), typeof(Visibility))]
    public class FindRecordsTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value != null && (string)value == "Not Contains" || (string)value == "Contains" || (string)value == "Equal" || (string)value == "Not Equal" || (string)value == "Ends With" || (string)value == "Starts With" || (string)value == "Regex" || (string)value == ">" || (string)value == "<" || (string)value == "<=" || (string)value == ">=")
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
