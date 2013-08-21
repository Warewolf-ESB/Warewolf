using System;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Infragistics.Windows.DockManager;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TabGroupPaneBindingBehavior : Behavior<TabGroupPane>
    {
        #region Private Methods
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
