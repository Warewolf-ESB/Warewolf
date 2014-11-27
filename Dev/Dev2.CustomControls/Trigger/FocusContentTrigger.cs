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
                object firstItem = treeviewItem.Items.GetItemAt(0);
                var firstContainer = treeviewItem.ItemContainerGenerator.ContainerFromItem(firstItem) as UIElement;

                if (firstContainer != null)
                {
                    firstContainer.Focus();
                }
            }
        }
    }
}