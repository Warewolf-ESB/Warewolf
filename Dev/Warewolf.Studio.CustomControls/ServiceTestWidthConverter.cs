using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Warewolf.Studio.CustomControls
{
    public class ServiceTestWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (values[0] is TreeViewItem xamDataTreeNodeControl)
                {
                    return xamDataTreeNodeControl.ActualWidth - 22;
                }
                return 22;
            }
            catch (Exception)
            {
                return 22;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ServiceTestHeaderTemplateWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] is Expander expander)
                {
                    return expander.ActualWidth - 80;
                }
                return double.NaN;
            }
            catch (Exception)
            {
                return double.NaN;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}