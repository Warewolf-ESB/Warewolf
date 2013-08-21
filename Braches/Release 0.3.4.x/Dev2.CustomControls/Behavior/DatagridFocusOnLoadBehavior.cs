using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.CustomControls.Behavior
{
    public class DatagridFocusOnLoadBehavior : Behavior<DataGrid>
    {
        private int _count = 0;
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var txtBox = AssociatedObject.Columns[1].GetCellContent(AssociatedObject.Items[0]);
            var txt = txtBox.FindVisualChildren<TextBox>().FirstOrDefault();
            if(txt != null)
            {
                txt.Loaded += (s, e) =>
                    {
                        if(_count == 0)
                        {
                            txt.Focus();
                            _count++;
                        }
                    };
            }
        }
    }
}
