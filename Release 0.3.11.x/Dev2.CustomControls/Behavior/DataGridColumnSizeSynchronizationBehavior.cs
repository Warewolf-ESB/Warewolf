using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        void AssociatedObjectSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            foreach (var dataGridColumn in AssociatedObject.Columns)
            {
                var dataGridLength = dataGridColumn.Width;
                dataGridColumn.ClearValue(DataGridColumn.WidthProperty);
                dataGridColumn.Width = dataGridLength;
            }
        }
    }
}
