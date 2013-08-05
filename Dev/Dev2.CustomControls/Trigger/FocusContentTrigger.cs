using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.CustomControls.Trigger
{
    public class FocusTreeViewItemContentTrigger : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            var treeviewItem = AssociatedObject.FindVisualParent<TreeViewItem>();
            var treeview = AssociatedObject.FindVisualParent<TreeView>();

            if (treeviewItem.Items.Count > 0)
            {
                var firstItem = treeviewItem.Items.GetItemAt(0);
                var firstContainer = treeviewItem.ItemContainerGenerator.ContainerFromItem(firstItem) as UIElement;
                var firstContainers = treeview.ItemContainerGenerator.ContainerFromItem(treeviewItem);
                var firstItemCOntainer = treeview.ItemContainerGenerator.ContainerFromItem(firstItem);
                if (firstContainer != null)
                {
                    firstContainer.Focus();
                }
            };

        }
    }
}
