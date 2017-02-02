/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Editors;

namespace Warewolf.Studio.Views.Converters
{
    public class ComboBoxToMaxItemWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ComboEditorToMaxItemWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
                ComboEditorItem cbItem = first;
                if (cbItem?.ComboEditor.ActualWidth > maxWidth)
                    maxWidth = cbItem.ComboEditor.ActualWidth;
            }
            return maxWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
