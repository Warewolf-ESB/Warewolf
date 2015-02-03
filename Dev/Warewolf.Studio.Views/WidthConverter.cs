using System;
using System.Globalization;
using System.Windows.Data;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Returns the exact length of the requred menu node based on its tree position
    /// </summary>
    public class WidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)values[1] - ((values[0] as XamDataTreeNodeControl).Node.Manager.Level * 21) - 62;
            

        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}