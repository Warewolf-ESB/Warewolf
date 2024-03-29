#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;


namespace Dev2.Studio.Core.AppResources.Converters
{
    public class EnumCollection : Collection<Enum>
    {

    }

    [ValueConversion(typeof(Enum), typeof(Visibility))]
    public class EnumToVisibilityConverter : DependencyObject, IValueConverter
    {
        public EnumToVisibilityConverter()
        {
            VisibleEnumValues = new EnumCollection();
        }

        #region VisibleEnumValues

        public EnumCollection VisibleEnumValues
        {
            get { return (EnumCollection)GetValue(VisibleEnumValuesProperty); }
            set { SetValue(VisibleEnumValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisibleEnumValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibleEnumValuesProperty =
            DependencyProperty.Register("VisibleEnumValues", typeof(EnumCollection), typeof(EnumToVisibilityConverter), new PropertyMetadata(null));

        #endregion VisibleEnumValues

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!value.GetType().IsEnum)
            {
                return Binding.DoNothing;
            }

            if(VisibleEnumValues.Any(e => Equals(e, value)))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
