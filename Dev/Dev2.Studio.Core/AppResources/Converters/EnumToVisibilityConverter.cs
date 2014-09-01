using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

// ReSharper disable once CheckNamespace
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

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
