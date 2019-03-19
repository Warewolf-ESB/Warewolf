#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
        
        List<TabGroupPane> GetAllTabGroupPanes()
        {
            _tabGroupPanes = new List<TabGroupPane>();
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
            get => (DocumentContentHost)GetValue(DocumentHostProperty);
            set => SetValue(DocumentHostProperty, value);
        }

        static void DocumentHostChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (!(dependencyObject is TabGroupPaneBindingBehavior itemsControlBindingBehavior))
            {
                return;
            }

            if (e.NewValue is DocumentContentHost newValue)
            {
                newValue.ActiveDocumentChanged -= itemsControlBindingBehavior.DocumentHostOnActiveDocumentChanged;
                newValue.ActiveDocumentChanged += itemsControlBindingBehavior.DocumentHostOnActiveDocumentChanged;
                newValue.PreviewMouseLeftButtonDown -= itemsControlBindingBehavior.NewValueOnPreviewMouseLeftButtonDown;
                newValue.PreviewMouseLeftButtonDown += itemsControlBindingBehavior.NewValueOnPreviewMouseLeftButtonDown;
            }
        }

        void NewValueOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var host = sender as DocumentContentHost;
            var workSurfaceContextViewModel = host?.ActiveDocument?.DataContext as WorkSurfaceContextViewModel;

            if (_shellViewModel != null && _shellViewModel.ActiveItem != workSurfaceContextViewModel)
            {
                _shellViewModel.ActiveItem = workSurfaceContextViewModel;
            }
        }

        #endregion DocumentHost

        static List<TabGroupPane> _tabGroupPanes;
        ShellViewModel _shellViewModel;

        void ActiveItemChanged(IWorkSurfaceContextViewModel workSurfaceContextViewModel)
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
            if (DocumentHost?.DataContext is ShellViewModel mainViewModel)
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

        static void SetActivePane(IWorkSurfaceContextViewModel newValue)
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
