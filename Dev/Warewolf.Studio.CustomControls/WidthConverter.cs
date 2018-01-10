using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.CustomControls
{
    /// <summary>
    /// Returns the exact length of the requred menu node based on its tree position
    /// </summary>
    public class WidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                if (values[0] is XamDataTreeNodeControl xamDataTreeNodeControl)
                {
                    return (double)values[1] - xamDataTreeNodeControl.Node.Manager.Level * 21 - 62;
                }
                return 22;
            }
            catch(Exception)
            {
                return 22;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MergeWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            try
            {
                var xamDataTreeNodeControl = values[0] as TreeViewItem;
                if (xamDataTreeNodeControl != null)
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
    public class MergeHeaderTemplateWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var expander = values[0] as Expander;
                if (expander != null)
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