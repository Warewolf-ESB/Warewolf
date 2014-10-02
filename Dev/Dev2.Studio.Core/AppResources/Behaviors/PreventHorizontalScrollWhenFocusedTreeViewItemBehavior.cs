
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
using Dev2.Studio.Core.AppResources.ExtensionMethods;

// ReSharper disable once CheckNamespace
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
            if(_treeViewItem != null)
                _treeViewItem.RequestBringIntoView -= AssociatedObject_RequestBringIntoView;
        }

        void AssociatedObject_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
