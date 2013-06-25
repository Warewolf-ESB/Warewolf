using Dev2.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

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

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return NullValue;
            }

            if (value is string)
            {
                return TrueEnumValues.Any(e => 
                    {
                        object tempEnumValue = Dev2EnumConverter.GetEnumFromStringDiscription(value.ToString(), e.GetType());
                        return Enum.Equals(e, tempEnumValue);
                    });
            }

            if (!value.GetType().IsEnum)
            {
                return Binding.DoNothing;
            }

            return TrueEnumValues.Any(e => Enum.Equals(e, value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
