using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Studio.Core.AppResources.ExtensionMethods;

namespace Dev2.Studio.Core.AppResources.Behaviors
{
    public class PreventHorizontalScrollWhenFocusedTreeViewItemBehavior : Behavior<UIElement>
    {
        TreeViewItem _treeViewItem;
        protected override void OnAttached()
          {
            base.OnAttached();
            var item = AssociatedObject.GetParentByType(typeof(TreeViewItem));
            if(item is TreeViewItem)
            {
                _treeViewItem = (TreeViewItem)item;
                _treeViewItem.RequestBringIntoView += AssociatedObject_RequestBringIntoView;
            }
        }

        protected override void OnDetaching()
        {
            if (_treeViewItem != null)
                _treeViewItem.RequestBringIntoView -= AssociatedObject_RequestBringIntoView;
        }

        void AssociatedObject_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
