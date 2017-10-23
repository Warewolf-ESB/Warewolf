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
using System.Windows;
using System.Windows.Data;

namespace Dev2.AppResources.Converters
{
    public class CollectionToVisibilityConverter : IValueConverter
    {
        #region Properties

        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        #endregion Properties

        #region Implementation of IValueConverter
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? countValue = value as int?;

            if(countValue == null)
            {
                return Binding.DoNothing;
            }

            if(countValue.GetValueOrDefault() == 0)
            {
                return TrueValue;
            }

            return FalseValue;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}
