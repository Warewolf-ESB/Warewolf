using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dev2.AppResources.Converters
{
    public class DataGridRowNumberConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //get the grid and the item
            var item = values[0];
            var grid = values[1] as DataGrid;
            if(grid == null)
            {
                return -1;
            }

            var index = grid.Items.IndexOf(item);
            return index;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
