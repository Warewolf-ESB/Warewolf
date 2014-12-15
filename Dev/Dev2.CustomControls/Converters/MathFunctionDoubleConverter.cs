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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
{
    [ValueConversion(typeof (double[]), typeof (Thickness))]
    public class MathFunctionDoubleToThicknessConverter : DependencyObject, IMultiValueConverter
    {
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof (double),
                typeof (MathFunctionDoubleToThicknessConverter), new PropertyMetadata(0D));

        public static readonly DependencyProperty FunctionProperty =
            DependencyProperty.Register("Function", typeof (MathFunction),
                typeof (MathFunctionDoubleToThicknessConverter), new PropertyMetadata(MathFunction.Max));

        public double Offset
        {
            get { return (double) GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public MathFunction Function
        {
            get { return (MathFunction) GetValue(FunctionProperty); }
            set { SetValue(FunctionProperty, value); }
        }


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!values.Any())
            {
                return default(Thickness);
            }

            var doubles = new List<double>();
            foreach (object value in values)
            {
                try
                {
                    double d = System.Convert.ToDouble(value);
                    doubles.Add(d);
                }
                catch (Exception e)
                {
                    throw new Exception("Converter only accepts doubles", e);
                }
            }

            switch (Function)
            {
                case MathFunction.Max:
                    return new Thickness(doubles.Max() - Offset, 0, 0, 0);
                case MathFunction.Min:
                    return new Thickness(doubles.Min() - Offset, 0, 0, 0);
                default:
                    return default(Thickness);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}