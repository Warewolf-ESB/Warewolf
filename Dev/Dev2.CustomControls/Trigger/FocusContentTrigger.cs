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

            if (treeviewItem.Items.Count > 0)
            {
                var firstItem = treeviewItem.Items.GetItemAt(0);
                var firstContainer = treeviewItem.ItemContainerGenerator.ContainerFromItem(firstItem) as UIElement;

                if (firstContainer != null)
                {
                    firstContainer.Focus();
                }
            }

        }
    }
}
