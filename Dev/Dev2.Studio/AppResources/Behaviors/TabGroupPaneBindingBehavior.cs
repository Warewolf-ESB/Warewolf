/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.WorkSurface;
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
            _tabGroupPanes = new List<TabGroupPane>();

            if(DocumentHost == null)
            {
                if(AssociatedObject != null)
                {
                    _tabGroupPanes.Add(AssociatedObject);
                }

                return _tabGroupPanes;
            }
            _tabGroupPanes.AddRange(DocumentHost.GetDescendents().OfType<TabGroupPane>());
            return _tabGroupPanes;
        }

        #endregion Private Methods


        #region DocumentHost

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentHostProperty =
            DependencyProperty.Register("DocumentHost", typeof(DocumentContentHost), typeof(TabGroupPaneBindingBehavior), new PropertyMetadata(null, DocumentHostChangedCallback));

        public DocumentContentHost DocumentHost
        {
            get
            {
                return (DocumentContentHost) GetValue(DocumentHostProperty);
            }
            set
            {
                SetValue(DocumentHostProperty, value);
            }
        }        

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
                oldValue.PreviewMouseLeftButtonDown -= itemsControlBindingBehavior.NewValueOnPreviewMouseLeftButtonDown;
            }

            if (newValue != null)
            {
                newValue.ActiveDocumentChanged -= itemsControlBindingBehavior.DocumentHostOnActiveDocumentChanged;
                newValue.ActiveDocumentChanged += itemsControlBindingBehavior.DocumentHostOnActiveDocumentChanged;
                newValue.PreviewMouseLeftButtonDown -= itemsControlBindingBehavior.NewValueOnPreviewMouseLeftButtonDown;
                newValue.PreviewMouseLeftButtonDown += itemsControlBindingBehavior.NewValueOnPreviewMouseLeftButtonDown;
            }
        }

        private void NewValueOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var host = sender as DocumentContentHost;
            var workSurfaceContextViewModel = host?.ActiveDocument?.DataContext as WorkSurfaceContextViewModel;
            
            if (_shellViewModel == null)
            {
                var mainViewModel = DocumentHost?.DataContext as ShellViewModel;
                _shellViewModel = mainViewModel;                
            }
            if (_shellViewModel != null)
            {
                if (_shellViewModel.ActiveItem != workSurfaceContextViewModel)
                {
                    _shellViewModel.ActiveItem = workSurfaceContextViewModel;
                }
            }
        }

        #endregion DocumentHost

        private static List<TabGroupPane> _tabGroupPanes;
        private ShellViewModel _shellViewModel;

        private void ActiveItemChanged(WorkSurfaceContextViewModel workSurfaceContextViewModel)
        {
            if (_tabGroupPanes == null || _tabGroupPanes.Count <= 0)
            {
                _tabGroupPanes = GetAllTabGroupPanes();
            }

            SetActivePane(workSurfaceContextViewModel);
        }
        

        static void GotFocusHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            RefreshActiveEnvironment(sender);
            routedEventArgs.Handled = true;
        }

        static void RefreshActiveEnvironment(object sender)
        {
            var frameworkElement = sender as FrameworkElement;
            var vm = frameworkElement?.DataContext as ShellViewModel;
            vm?.RefreshActiveServer();
        }

        #region Event Handlers

        void DocumentHostOnActiveDocumentChanged(object sender, RoutedPropertyChangedEventArgs<ContentPane> routedPropertyChangedEventArgs)
        {

            var mainViewModel = DocumentHost?.DataContext as ShellViewModel;
            if (mainViewModel != null)
            {
                if (_shellViewModel == null)
                {
                    _shellViewModel = mainViewModel;
                    _shellViewModel.ActiveItemChanged = ActiveItemChanged;
                }

                var workSurfaceContextViewModel = routedPropertyChangedEventArgs.NewValue?.DataContext as WorkSurfaceContextViewModel;
                _shellViewModel.ActiveItemChanged = null;
                _shellViewModel.ActiveItem = workSurfaceContextViewModel;
                if (workSurfaceContextViewModel != null)
                {
                    _shellViewModel.PersistTabs();
                }
                _shellViewModel.ActiveItemChanged = ActiveItemChanged;
            }
        }

        private static void SetActivePane(WorkSurfaceContextViewModel newValue)
        {
            if (_tabGroupPanes != null && _tabGroupPanes.Count > 0)
            {
                var tabGroupPane = _tabGroupPanes[0];

                foreach (var item in from object item in tabGroupPane.Items
                    let frameworkElement = item as FrameworkElement
                    where frameworkElement != null && frameworkElement.DataContext == newValue
                    select item)
                {
                    if (tabGroupPane.SelectedItem != item)
                    {
                        tabGroupPane.SelectedItem = item;
                        break;
                    }
                }
                FocusManager.AddGotFocusHandler(tabGroupPane, GotFocusHandler);
            }
        }

        #endregion Event Handlers
    }
}
