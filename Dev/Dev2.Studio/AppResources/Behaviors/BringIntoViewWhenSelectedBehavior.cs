
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Dev2.Models;

// ReSharper disable CheckNamespace
namespace Dev2.CustomControls.Behavior
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Exposes attached behaviors that can be
    /// applied to TreeViewItem objects.
    /// </summary>
    public class BringIntoViewWhenSelectedBehavior : Behavior<TreeViewItem>
    {
        private Storyboard _storyBoard;
        private DispatcherTimer _timer;
        private TreeViewItem _item;

        protected override void OnAttached()
        {
            base.OnAttached();
            _storyBoard = Application.Current.Resources["TreeViewFocusOnAddStoryBoard"] as Storyboard;
            AssociatedObject.Selected += OnTreeViewItemSelected;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Selected -= OnTreeViewItemSelected;
            base.OnDetaching();
        }

        #region IsBroughtIntoViewWhenSelected

        private void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            //TODO - yeah, _item not associatedObject.
            //THis has to do with the original source getting lost somewhere along the way.
            //Look into this.

            // Only react to the Selected event raised by the TreeViewItem
            // whose IsSelected property was modified. Ignore all ancestors
            // who are merely reporting that a descendant's Selected fired.
            _item = e.OriginalSource as TreeViewItem;
            if(_item == null)
            {
                return;
            }

            if(!ReferenceEquals(sender, e.OriginalSource))
            {
                if(!_item.IsLoaded)
                {
                    _item.Loaded -= ItemInitialized;
                    _item.Loaded += ItemInitialized;
                }
                return;
            }

            BringIntoView(_item);
        }

        private void ItemInitialized(object sender, EventArgs e)
        {
            //TODO - yeah, _item not associatedObject.
            //THis has to do with the original source getting lost somewhere along the way.
            //Look into this.
            _item = sender as TreeViewItem;
            if(_item == null)
            {
                return;
            }

            if(_item.IsSelected)
            {
                BringIntoView(_item);
            }
        }

        private void BringIntoView(TreeViewItem item)
        {
            var treeNode = _item.DataContext as ExplorerItemModel;
            if(treeNode == null)
            {
                return;
            }
            item.BringIntoView();

            if(!treeNode.IsNew)
            {
                return;
            }

            //TODO find a better way to do this (instead of waiting halve a second and using a dispatchertimer)
            //We need to know when the item as been scrolled into view, and then fire the animation
            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 3000)
            };
            _timer.Tick += (sender, e) =>
            {
                _item.BeginStoryboard(_storyBoard);
                _timer.Stop();
                treeNode.IsNew = false;
            };
            _timer.Start();
        }
        #endregion IsBroughtIntoViewWhenSelected
    }
}
