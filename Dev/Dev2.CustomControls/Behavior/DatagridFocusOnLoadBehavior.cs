using System;
using System.Linq;
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
            AssociatedObject.LoadingRow += AssociatedObjectOnLoadingRow;

            //AssociatedObject.InitializingNewItem += AssociatedObjectOnInitializingNewItem;
        }

        void AssociatedObjectOnLoadingRow(object sender, DataGridRowEventArgs args)
        {
            
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
            AssociatedObject.LoadingRow -= AssociatedObjectOnLoadingRow;
            //AssociatedObject.InitializingNewItem -= AssociatedObjectOnInitializingNewItem;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var txtBox = AssociatedObject.Columns[1].GetCellContent(AssociatedObject.Items[0]);
            if(txtBox == null)
            {
                return;
            }
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

        void AssociatedObjectOnInitializingNewItem(object sender, InitializingNewItemEventArgs args)
        {
            var txtBox = AssociatedObject.Columns[0].GetCellContent(args.NewItem);
            var txt = txtBox.FindVisualChildren<TextBox>().FirstOrDefault();
            if(txt != null)
            {
                txt.Focus();
            }
        }
    }
}
