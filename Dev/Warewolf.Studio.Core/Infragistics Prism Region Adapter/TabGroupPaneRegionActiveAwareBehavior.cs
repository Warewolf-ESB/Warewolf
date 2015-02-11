/* Written by Brian Lagunas
 * Blog: http://brianlagunas.com
 * Twitter: @brianlagunas
 * Email: blagunas@infragistics.com 
 */

using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.DockManager;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Regions.Behaviors;

namespace Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter
{
    public class TabGroupPaneRegionActiveAwareBehavior : RegionBehavior, IHostAwareRegionBehavior
    {
        public const string BehaviorKey = "TabGroupPaneRegionActiveAwareBehavior";

        XamDockManager _parentDockManager;

        TabGroupPane _hostControl;
        public DependencyObject HostControl
        {
            get { return _hostControl; }
            set { _hostControl = value as TabGroupPane; }
        }

        protected override void OnAttach()
        {
            _parentDockManager = XamDockManager.GetDockManager(_hostControl);
            if (_parentDockManager != null)
                _parentDockManager.ActivePaneChanged += DockManager_ActivePaneChanged;

            Region.ActiveViews.CollectionChanged += ActiveViews_CollectionChanged;
        }

        void DockManager_ActivePaneChanged(object sender, RoutedPropertyChangedEventArgs<ContentPane> e)
        {
            if (e.OldValue != null)
            {
                var item = e.OldValue;

                //are we dealing with a ContentPane directly
                if (Region.Views.Contains(item) && Region.ActiveViews.Contains(item))
                {
                    Region.Deactivate(item);
                }
                else
                {
                    //now check to see if we have any views that were injected
                    var contentControl = item as ContentControl;
                    if (contentControl != null)
                    {
                        var injectedView = contentControl.Content;
                        if (Region.Views.Contains(injectedView) && Region.ActiveViews.Contains(injectedView))
                            Region.Deactivate(injectedView);
                    }
                }
            }

            if (e.NewValue != null)
            {
                var item = e.NewValue;

                //are we dealing with a ContentPane directly
                if (Region.Views.Contains(item) && !Region.ActiveViews.Contains(item))
                {
                    Region.Activate(item);
                }
                else
                {
                    //now check to see if we have any views that were injected
                    var contentControl = item as ContentControl;
                    if (contentControl != null)
                    {
                        var injectedView = contentControl.Content;
                        if (Region.Views.Contains(injectedView) && !Region.ActiveViews.Contains(injectedView))
                            Region.Activate(injectedView);
                    }
                }
            }
        }

        void ActiveViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //are we dealing with a view
                FrameworkElement frameworkElement = e.NewItems[0] as FrameworkElement;
                if (frameworkElement != null)
                {
                    ContentPane contentPane = frameworkElement as ContentPane ?? frameworkElement.Parent as ContentPane;

                    if (contentPane != null && !contentPane.IsActivePane)
                        contentPane.Activate();
                }
                else
                {
                    //must be a viewmodel
                    object viewModel = e.NewItems[0];
                    var contentPane = GetContentPaneFromFromViewModel(viewModel);
                    if (contentPane != null)
                        contentPane.Activate();
                }
            }
        }

        ContentPane GetContentPaneFromFromViewModel(object viewModel)
        {
            var panes = XamDockManager.GetDockManager(_hostControl).GetPanes(PaneNavigationOrder.VisibleOrder);
            return panes.FirstOrDefault(contentPane => contentPane.DataContext == viewModel);
        }
    }
}
