#region

using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.ViewModels.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

#endregion

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TreeviewScrollToEndOnNewContent : Behavior<TreeView>
    {
        #region Class Members

        ObservableCollection<DebugTreeViewItemViewModel> _collection;
        ScrollViewer _treeviewScrollViewer;

        #endregion Class Members

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObjectLoaded;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_collection != null)
            {
                _collection.CollectionChanged -= CollectionCollectionChanged;
            }
        }

        void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObjectLoaded;

            _collection = AssociatedObject.ItemsSource as ObservableCollection<DebugTreeViewItemViewModel>;
            if(_collection != null)
            {
                _collection.CollectionChanged -= CollectionCollectionChanged;
                _collection.CollectionChanged += CollectionCollectionChanged;
            }

            _treeviewScrollViewer = DependencyObjectExtensions.GetChildByType(AssociatedObject, typeof(ScrollViewer)) as ScrollViewer;

            //Juries - Removed, instead implement a collection changed handler, to only scroll to end when new items are added.          
            //if (_treeviewScrollViewer != null)
            //{
            //    _treeviewScrollViewer.ScrollChanged += _treeviewScrollViewer_ScrollChanged;
            //}
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            //Juries - Removed, instead implement a collection changed handler, to only scroll to end when new items are added.
            //if (_treeviewScrollViewer != null)
            //{
            //    _treeviewScrollViewer.ScrollChanged -= _treeviewScrollViewer_ScrollChanged;
            //}

            if(_collection != null)
            {
                _collection.CollectionChanged -= CollectionCollectionChanged;
            }
        }

        #endregion Override Methods

        #region Event Handlers

        void CollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null && e.NewItems.Count > 0)
            {
                // Travis.Frisinger : Null exception was being thrown on trunk ;)
                if(_treeviewScrollViewer != null)
                {
                    _treeviewScrollViewer.ScrollToEnd();
                }
            }
        }

        //Juries - Removed, instead implement a collection changed handler, to only scroll to end when new items are added.
        //void _treeviewScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    if (AssociatedObject.SelectedItem == null && ((e.ExtentHeightChange > 0 && e.ViewportHeightChange == 0) || (e.ExtentWidthChange > 0 && e.ViewportWidthChange == 0)))
        //    {
        //        _treeviewScrollViewer.ScrollToEnd();
        //    }
        //}

        #endregion Event Handlers
    }
}