using System;
using System.Globalization;
using System.Windows.Data;
using Infragistics.Controls.Menus;

namespace Dev2.Activities.Designers2.Core
{
    public class DotNetActionsWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var xamDataTreeNodeControl = values[0] as XamDataTreeNodeControl;
                if (xamDataTreeNodeControl != null)
                {
                    return (double)values[1] - xamDataTreeNodeControl.Node.Manager.Level * 21 - 35;
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
}
