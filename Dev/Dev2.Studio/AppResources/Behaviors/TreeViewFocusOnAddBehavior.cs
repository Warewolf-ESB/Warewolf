using Dev2.Studio.Core.ViewModels.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
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

            var nodes = collection as ObservableCollection<ITreeNode>;
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

            nodes = collectionView.SourceCollection as ObservableCollection<ITreeNode>;
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

            var treenodes = e.NewItems.OfType<ITreeNode>();
            treenodes.ToList().ForEach(n =>
                {
                    n.Children.CollectionChanged += ItemsSourceCollectionChanged;

                    if(!n.IsNew)
                    {
                        return;
                    }
                    ExpandToTop(n, new List<ITreeNode>());
                    n.IsSelected = true;
                });
        }

        private void ExpandToTop(ITreeNode treeNode, IList<ITreeNode> childrenToExpandTo)
        {
            if(treeNode == null || treeNode.IsExpanded)
            {
                if(childrenToExpandTo != null && childrenToExpandTo.Count >= 1)
                {
                    childrenToExpandTo.Reverse().ToList()
                        .ForEach(c => c.IsExpanded = true);
                }
            }
            else if(!treeNode.IsExpanded)
            {
                if(treeNode.TreeParent != null)
                {
                    childrenToExpandTo.Add(treeNode);
                    ExpandToTop(treeNode.TreeParent, childrenToExpandTo);
                }
                else if(childrenToExpandTo != null && childrenToExpandTo.Count >= 1)
                {
                    childrenToExpandTo.ToList().ForEach(c => c.IsExpanded = true);
                }
            }
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AttachSourceCollectionChangedHandler(AssociatedObject.Items);
            foreach(var node in AssociatedObject.Items.OfType<ITreeNode>())
            {
                AttachSourceCollectionChangedHandler(node.Children);
            }
        }

        #endregion event handlers
    }
}
