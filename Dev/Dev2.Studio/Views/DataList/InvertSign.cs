
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
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.UI.Controls
{
    [ValueConversion(typeof(int), typeof(int))]
    public class InvertSignConverter : IValueConverter
    {
        public static InvertSignConverter Instance = new InvertSignConverter();

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;
            return (val * -1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;
            return (val * -1);
        }

        #endregion
    }
}
