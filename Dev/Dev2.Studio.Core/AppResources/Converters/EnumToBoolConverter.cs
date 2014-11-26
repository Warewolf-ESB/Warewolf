
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
using Dev2.Common.Interfaces.Enums.Enums;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Converters
{
    public class EnumToBoolConverter : DependencyObject, IValueConverter
    {
        public EnumToBoolConverter()
        {
            TrueEnumValues = new EnumCollection();
        }

        #region TrueEnumValues

        public EnumCollection TrueEnumValues
        {
            get { return (EnumCollection)GetValue(TrueEnumValuesProperty); }
            set { SetValue(TrueEnumValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisibleEnumValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TrueEnumValuesProperty =
            DependencyProperty.Register("VisibleEnumValues", typeof(EnumCollection), typeof(EnumToBoolConverter), new PropertyMetadata(null));

        #endregion VisibleEnumValues

        #region Properties

        public bool NullValue { get; set; }

        #endregion Properties

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return NullValue;
            }

            if(value is string)
            {
                return TrueEnumValues.Any(e =>
                    {
                        object tempEnumValue = Dev2EnumConverter.GetEnumFromStringDiscription(value.ToString(), e.GetType());
                        return Equals(e, tempEnumValue);
                    });
            }

            if(!value.GetType().IsEnum)
            {
                return Binding.DoNothing;
            }

            return TrueEnumValues.Any(e => Equals(e, value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
