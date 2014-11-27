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
    public class DoubleToMarginLeftConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof (double),
                typeof (DoubleToMarginLeftConverter), new PropertyMetadata(0D));

        public double Offset
        {
            get { return (double) GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var width = (double) value;
                if (Math.Abs(Offset - 0D) > 0D)
                {
                    width -= Offset;
                }
                return new Thickness(width, 0, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}