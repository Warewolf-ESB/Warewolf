using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
{
    [ValueConversion(typeof(double[]), typeof(Thickness))]
    public class MathFunctionDoubleToThicknessConverter : DependencyObject, IMultiValueConverter
    {
        public double Offset
        {
            get { return (double)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(double),
            typeof(MathFunctionDoubleToThicknessConverter), new PropertyMetadata(0D));

        public MathFunction Function
        {
            get { return (MathFunction)GetValue(FunctionProperty); }
            set { SetValue(FunctionProperty, value); }
        }

        public static readonly DependencyProperty FunctionProperty =
            DependencyProperty.Register("Function", typeof(MathFunction), 
            typeof(MathFunctionDoubleToThicknessConverter), new PropertyMetadata(MathFunction.Max));

 
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!values.Any())
            {
                return default(Thickness);
            }

            var doubles = new List<double>();
            foreach (var value in values)
            {
                try
                {
                    var d = System.Convert.ToDouble(value);
                    doubles.Add(d);
                }
                catch(Exception e)
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
