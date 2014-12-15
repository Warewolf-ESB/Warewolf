/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Dev2.CustomControls.Behavior
{
    public class DataGridColumnSizeSynchronizationBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SizeChanged += AssociatedObjectSizeChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SizeChanged -= AssociatedObjectSizeChanged;
        }

        private void AssociatedObjectSizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (DataGridColumn dataGridColumn in AssociatedObject.Columns)
            {
                DataGridLength dataGridLength = dataGridColumn.Width;
                dataGridColumn.ClearValue(DataGridColumn.WidthProperty);
                dataGridColumn.Width = dataGridLength;
            }
        }
    }
}