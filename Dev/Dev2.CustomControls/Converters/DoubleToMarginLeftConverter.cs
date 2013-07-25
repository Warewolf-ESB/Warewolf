using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Dev2.CustomControls.Converters
{
    public class DoubleToMarginLeftConverter : DependencyObject, IValueConverter
    {
        public double Offset
        {
            get { return (double)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(double),
            typeof(DoubleToMarginLeftConverter), new PropertyMetadata(0D));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                var width = (double)value;
                if (Math.Abs(Offset - 0D) > 0D)
                { 
                    width -= Offset;
                }
                return new Thickness(width, 0, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
