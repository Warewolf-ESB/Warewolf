using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Infragistics.Windows.DockManager;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TabGroupPaneBindingBehavior : Behavior<TabGroupPane>
    {
        #region Override Methods

        protected override void OnDetaching()
        {
            base.OnDetaching();

            UnsubscribeFromItemsSource(ItemsSource as INotifyCollectionChanged);
        }

        #endregion Override Methods

        #region Private Methods

        /// <summary>
        /// Subscribe to ItemsSource events
        /// </summary>
        private void SubscribeToItemsSource(INotifyCollectionChanged source)
        {
            if (source == null) return;

            source.CollectionChanged -= SourceItemsOnCollectionChanged;
            source.CollectionChanged += SourceItemsOnCollectionChanged;
        }

        /// <summary>
        /// Unsubscribe from ItemsSource events
        /// </summary>
        private void UnsubscribeFromItemsSource(INotifyCollectionChanged source)
        {
            if (source == null) return;

            source.CollectionChanged -= SourceItemsOnCollectionChanged;
        }

        /// <summary>
        /// Updates the destination items form the items in the items source
        /// </summary>
        private void UpdateDestinationItems()
        {
            if (ItemsSource == null) return;

            List<TabGroupPane> tabGroupPanes = GetAllTabGroupPanes();
            List<object> destinationItems = tabGroupPanes.SelectMany(t => t.Items.OfType<FrameworkElement>().Select(f => f.DataContext)).ToList();
            List<object> itemsToRemove = destinationItems.Except(ItemsSource.OfType<object>()).ToList();
            List<object> itemsToAdd = ItemsSource.OfType<object>().Except(destinationItems).ToList();

            foreach (TabGroupPane tabGroupPane in tabGroupPanes)
            {
                foreach (FrameworkElement itemToRemove in tabGroupPane.Items.OfType<FrameworkElement>().Where(f => itemsToRemove.Contains(f.DataContext)).ToList())
                {
                    tabGroupPane.Items.Remove(itemToRemove);
                }
            }

            foreach (var itemToAdd in itemsToAdd)
            {
                FrameworkElement visualItem = ItemTemplate.LoadContent() as FrameworkElement;
                if (visualItem == null) continue;

                visualItem.DataContext = itemToAdd;
                AssociatedObject.Items.Add(visualItem);
            }
        }

        /// <summary>
        /// Gets all tab group panes which are descendents of the DocumentHost
        /// </summary>
        private List<TabGroupPane> GetAllTabGroupPanes()
        {
            List<TabGroupPane> tabGroupPanes = new List<TabGroupPane>();

            if (DocumentHost == null)
            {
                if (AssociatedObject != null)
                {
                    tabGroupPanes.Add(AssociatedObject);
                }

                return tabGroupPanes;
            }
            tabGroupPanes.AddRange(DocumentHost.GetDescendents().OfType<TabGroupPane>());
            return tabGroupPanes;
        }

        #endregion Private Methods

        #region Dependency Properties

        #region ItemsSource

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList), typeof(TabGroupPaneBindingBehavior), new PropertyMetadata(null, ItemsSourceChangedCallback));

        private static void ItemsSourceChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TabGroupPaneBindingBehavior itemsControlBindingBehavior = dependencyObject as TabGroupPaneBindingBehavior;
            if (itemsControlBindingBehavior == null) return;


            itemsControlBindingBehavior.UpdateDestinationItems();

            itemsControlBindingBehavior.UnsubscribeFromItemsSource(e.OldValue as INotifyCollectionChanged);
            itemsControlBindingBehavior.SubscribeToItemsSource(e.NewValue as INotifyCollectionChanged);
        }

        #endregion ItemsSource

        #region ItemTemplate

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TabGroupPaneBindingBehavior), new PropertyMetadata(null));

        #endregion ItemTemplate

        #region DocumentHost

        public DocumentContentHost DocumentHost
        {
            get { return (DocumentContentHost)GetValue(DocumentHostProperty); }
            set { SetValue(DocumentHostProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentHostProperty =
            DependencyProperty.Register("DocumentHost", typeof(DocumentContentHost), typeof(TabGroupPaneBindingBehavior), new PropertyMetadata(null, DocumentHostChangedCallback));

        private static void DocumentHostChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TabGroupPaneBindingBehavior itemsControlBindingBehavior = dependencyObject as TabGroupPaneBindingBehavior;
            if (itemsControlBindingBehavior == null) return;

            DocumentContentHost oldValue = e.OldValue as DocumentContentHost;
            DocumentContentHost newValue = e.NewValue as DocumentContentHost;

            if (oldValue != null)
            {
                oldValue.ActiveDocumentChanged -= itemsControlBindingBehavior.DocumentHostOnActiveDocumentChanged;
            }

            if (newValue != null)
            {
                newValue.ActiveDocumentChanged -= itemsControlBindingBehavior.DocumentHostOnActiveDocumentChanged;
                newValue.ActiveDocumentChanged += itemsControlBindingBehavior.DocumentHostOnActiveDocumentChanged;
            }
        }

        #endregion DocumentHost

        #region SelectedItem

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TabGroupPaneBindingBehavior), new UIPropertyMetadata(null, SelectedItemChangedCallback));

        private static void SelectedItemChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TabGroupPaneBindingBehavior itemsControlBindingBehavior = dependencyObject as TabGroupPaneBindingBehavior;
            if (itemsControlBindingBehavior == null) return;

            foreach (TabGroupPane tabGroupPane in itemsControlBindingBehavior.GetAllTabGroupPanes())
            {
                bool found = false;

                for (int i = 0; i < tabGroupPane.Items.Count; i++)
                {
                    FrameworkElement frameworkElement = tabGroupPane.Items[i] as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.DataContext == e.NewValue)
                    {
                        tabGroupPane.SelectedIndex = i;
                        found = true;
                        break;
                    }
                }

                if (found) break;
            }
        }

        #endregion SelectedItem

        #endregion Dependency Properties

        #region Event Handlers

        private void SourceItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            UpdateDestinationItems();
        }

        private void DocumentHostOnActiveDocumentChanged(object sender, RoutedPropertyChangedEventArgs<ContentPane> routedPropertyChangedEventArgs)
        {
            if (routedPropertyChangedEventArgs.NewValue == null)
            {
                SelectedItem = null;
            }
            else
            {
                SelectedItem = routedPropertyChangedEventArgs.NewValue.DataContext;
            }
        }

        #endregion Event Handlers
    }
}
