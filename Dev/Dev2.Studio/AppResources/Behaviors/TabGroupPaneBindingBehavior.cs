using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.ViewModels;
using Infragistics.Windows.DockManager;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TabGroupPaneBindingBehavior : Behavior<TabGroupPane>
    {
        #region Private Methods

        /// <summary>
        ///     Gets all tab group panes which are descendents of the DocumentHost
        /// </summary>
        List<TabGroupPane> GetAllTabGroupPanes()
        {
            var tabGroupPanes = new List<TabGroupPane>();

            if(DocumentHost == null)
            {
                if(AssociatedObject != null)
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

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentHostProperty =
            DependencyProperty.Register("DocumentHost", typeof(DocumentContentHost), typeof(TabGroupPaneBindingBehavior), new PropertyMetadata(null, DocumentHostChangedCallback));
        public DocumentContentHost DocumentHost { get { return (DocumentContentHost)GetValue(DocumentHostProperty); } set { SetValue(DocumentHostProperty, value); } }

        static void DocumentHostChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var itemsControlBindingBehavior = dependencyObject as TabGroupPaneBindingBehavior;
            if (itemsControlBindingBehavior == null)
            {
                return;
            }

            var oldValue = e.OldValue as DocumentContentHost;
            var newValue = e.NewValue as DocumentContentHost;

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

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TabGroupPaneBindingBehavior), new UIPropertyMetadata(null, SelectedItemChangedCallback));
        public object SelectedItem { get { return GetValue(SelectedItemProperty); } set { SetValue(SelectedItemProperty, value); } }

        static void SelectedItemChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var itemsControlBindingBehavior = dependencyObject as TabGroupPaneBindingBehavior;
            if (itemsControlBindingBehavior == null)
            {
                return;
            }

            foreach (var tabGroupPane in itemsControlBindingBehavior.GetAllTabGroupPanes())
            {
                FocusManager.AddGotFocusHandler(tabGroupPane, GotFocusHandler);
                var found = false;

                for (var i = 0; i < tabGroupPane.Items.Count; i++)
                {
                    var frameworkElement = tabGroupPane.Items[i] as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.DataContext == e.NewValue)
                    {
                        tabGroupPane.SelectedIndex = i;
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }
        }

        static void GotFocusHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            RefreshActiveEnvironment(sender);
            routedEventArgs.Handled = true;
        }

        static void RefreshActiveEnvironment(object sender)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null && frameworkElement.DataContext != null)
            {
                var vm = frameworkElement.DataContext as MainViewModel;
                if (vm != null)
                {
                    vm.RefreshActiveEnvironment();
                }
            }
        }

        #endregion SelectedItem

        #endregion Dependency Properties

        #region Event Handlers

        void DocumentHostOnActiveDocumentChanged(object sender, RoutedPropertyChangedEventArgs<ContentPane> routedPropertyChangedEventArgs)
        {
            if (routedPropertyChangedEventArgs.NewValue != null)
            {
                SelectedItem = routedPropertyChangedEventArgs.NewValue.DataContext;
            }
        }

        #endregion Event Handlers
    }
}