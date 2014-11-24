
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using Dev2.Models;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.AppResources.Behaviors
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/05</date>
    public class TreeViewFocusOnAddBehavior : Behavior<TreeView>
    {

        #region Dependency Properties

        public System.Windows.Media.Animation.Storyboard InsertAnimation
        {
            get { return (System.Windows.Media.Animation.Storyboard)GetValue(InsertAnimationProperty); }
            set { SetValue(InsertAnimationProperty, value); }
        }

        public static readonly DependencyProperty InsertAnimationProperty =
            DependencyProperty.Register("InsertAnimation",
            typeof(System.Windows.Media.Animation.Storyboard),
            typeof(TreeViewFocusOnAddBehavior), new PropertyMetadata(null));

        #endregion Dependency Properties

        #region overrides

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            DetachSourceCollectionChangedHandler(AssociatedObject.Items);
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            base.OnDetaching();
        }

        #endregion overrides

        #region Event Handlers

        private void AttachSourceCollectionChangedHandler(INotifyCollectionChanged collection)
        {
            if(collection == null)
            {
                return;
            }
            collection.CollectionChanged += ItemsSourceCollectionChanged;

            var nodes = collection as ObservableCollection<IExplorerItemModel>;
            if(nodes != null)
            {
                nodes.ToList().ForEach(c => AttachSourceCollectionChangedHandler(c.Children));
                return;
            }

            var collectionView = collection as CollectionView;
            if(collectionView == null)
            {
                return;
            }

            nodes = collectionView.SourceCollection as ObservableCollection<IExplorerItemModel>;
            if(nodes != null)
            {
                nodes.ToList().ForEach(c => AttachSourceCollectionChangedHandler(c.Children));
            }
        }

        private void DetachSourceCollectionChangedHandler(INotifyCollectionChanged collection)
        {
            if(collection == null)
            {
                return;
            }
            collection.CollectionChanged -= ItemsSourceCollectionChanged;
        }

        void ItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems == null)
            {
                return;
            }

            var treenodes = e.NewItems.OfType<ExplorerItemModel>();
            treenodes.ToList().ForEach(n =>
                {
                    n.Children.CollectionChanged += ItemsSourceCollectionChanged;

                    if(!n.IsNew)
                    {
                        return;
                    }
                    ExpandToTop(n, new List<IExplorerItemModel>());
                    n.IsExplorerSelected = true;
                });
        }

        private void ExpandToTop(IExplorerItemModel treeNode, IList<IExplorerItemModel> childrenToExpandTo)
        {
            if(treeNode == null || treeNode.IsExplorerExpanded)
            {
                if(childrenToExpandTo != null && childrenToExpandTo.Count >= 1)
                {
                    childrenToExpandTo.Reverse().ToList()
                        .ForEach(c => c.IsExplorerExpanded = true);
                }
            }
            else if(!treeNode.IsExplorerExpanded)
            {
                if(treeNode.Parent != null)
                {
                    childrenToExpandTo.Add(treeNode);
                    ExpandToTop(treeNode.Parent, childrenToExpandTo);
                }
                else if(childrenToExpandTo != null && childrenToExpandTo.Count >= 1)
                {
                    childrenToExpandTo.ToList().ForEach(c => c.IsExplorerExpanded = true);
                }
            }
        }

        // ReSharper disable InconsistentNaming
        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        // ReSharper restore InconsistentNaming
        {
            AttachSourceCollectionChangedHandler(AssociatedObject.Items);
            foreach(var node in AssociatedObject.Items.OfType<ExplorerItemModel>())
            {
                AttachSourceCollectionChangedHandler(node.Children);
            }
        }

        #endregion event handlers
    }
}
