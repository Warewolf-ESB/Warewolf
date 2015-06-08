using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Editors;

namespace Warewolf.Studio.Views.Converters
{
    public class ComboBoxToMaxItemWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double maxWidth = 0;
            ComboBox cb = (ComboBox)value;
            foreach (var item in cb.Items)
            {
                ComboBoxItem cbItem = (ComboBoxItem)cb.ItemContainerGenerator.ContainerFromItem(item);
                if (cbItem.ActualWidth > maxWidth)
                    maxWidth = cbItem.ActualWidth;
            }
            return maxWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ComboEditorToMaxItemWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double maxWidth = 0;
            XamComboEditor cb = (XamComboEditor)value;
            foreach (var item in cb.Items)
            {
                ComboEditorItem first = null;
                foreach (var editorItem in cb.Items)
                {
                    first = editorItem;
                    break;
                }
                ComboEditorItem cbItem = (ComboEditorItem)first;
                if (cbItem != null)
                if (cbItem.ComboEditor.ActualWidth > maxWidth)
                    maxWidth = cbItem.ComboEditor.ActualWidth;
            }
            return maxWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
