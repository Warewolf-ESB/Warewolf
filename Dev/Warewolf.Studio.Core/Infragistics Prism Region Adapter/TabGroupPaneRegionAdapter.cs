/* Written by Brian Lagunas
 * Blog: http://brianlagunas.com
 * Twitter: @brianlagunas
 * Email: blagunas@infragistics.com 
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;
using Microsoft.Practices.Prism.Regions;

namespace Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter
{
    public class TabGroupPaneRegionAdapter : RegionAdapterBase<TabGroupPane>
    {
        /// <summary>
        /// Used to determine what views were injected and ContentPanes were generated for
        /// </summary>
        private static readonly DependencyProperty IsGeneratedProperty = DependencyProperty.RegisterAttached("IsGenerated", typeof(bool), typeof(TabGroupPaneRegionAdapter), null);

        /// <summary>
        /// Used to track the region that a ContentPane belongs to so that we can access the region from within the ContentPane.Closed event handler
        /// </summary>
        private static readonly DependencyProperty RegionProperty = DependencyProperty.RegisterAttached("Region", typeof(IRegion), typeof(TabGroupPaneRegionAdapter), null);

        public TabGroupPaneRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, TabGroupPane regionTarget)
        {
            if (regionTarget.ItemsSource != null)
                throw new InvalidOperationException("ItemsSource property is not empty. This control is being associated with a region, but the control is already bound to something else. If you did not explicitly set the control's ItemSource property, this exception may be caused by a change in the value of the inherited RegionManager attached property.");

            SynchronizeItems(region, regionTarget);

            region.Views.CollectionChanged += (Object sender, NotifyCollectionChangedEventArgs e) =>
            {
                OnViewsCollectionChanged(sender, e, region, regionTarget);
            };
        }

        protected override void AttachBehaviors(IRegion region, TabGroupPane regionTarget)
        {
            base.AttachBehaviors(region, regionTarget);

            if (!region.Behaviors.ContainsKey(TabGroupPaneRegionActiveAwareBehavior.BehaviorKey))
                region.Behaviors.Add(TabGroupPaneRegionActiveAwareBehavior.BehaviorKey, new TabGroupPaneRegionActiveAwareBehavior { HostControl = regionTarget });
        }

        protected override IRegion CreateRegion()
        {
            return new SingleActiveRegion();
        }

        private void OnViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, IRegion region, TabGroupPane regionTarget)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //we want to add them behind any previous views that may have been manually declare in XAML or injected
                int startIndex = e.NewStartingIndex;
                foreach (object newItem in e.NewItems)
                {
                    ContentPane contentPane = PrepareContainerForItem(newItem, region);

                    if (regionTarget.Items.Count != startIndex)
                        startIndex = 0;

                    //we must make sure we bring the TabGroupPane into view.  If we don't a System.StackOverflowException will occur in 
                    //UIAutomationProvider.dll if trying to add a ContentPane to a TabGroupPane that is not in view. 
                    //This is most common when using nested TabGroupPane regions. If you don't this, you can comment it out.
                    regionTarget.BringIntoView();

                    regionTarget.Items.Insert(startIndex, contentPane);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (regionTarget.Items.Count == 0)
                    return;

                IEnumerable<ContentPane> contentPanes = XamDockManager.GetDockManager(regionTarget).GetPanes(PaneNavigationOrder.VisibleOrder);
                foreach (ContentPane contentPane in contentPanes)
                {
                    if (e.OldItems.Contains(contentPane) || e.OldItems.Contains(contentPane.Content))
                        contentPane.ExecuteCommand(ContentPaneCommands.Close);
                }
            }
        }

        /// <summary>
        /// Takes all the views that were declared in XAML manually and merges them with the region.
        /// </summary>
        private void SynchronizeItems(IRegion region, TabGroupPane regionTarget)
        {
            if (regionTarget.Items.Count > 0)
            {
                foreach (object item in regionTarget.Items)
                {
                    PrepareContainerForItem(item, region);
                    region.Add(item);
                }
            }
        }

        /// <summary>
        /// Prepares a view being injected as a ContentPane
        /// </summary>
        /// <param name="item">the view</param>
        /// <returns>The injected view as a ContentPane</returns>
        protected ContentPane PrepareContainerForItem(object item, IRegion region)
        {
            ContentPane container = item as ContentPane;

            if (container == null)
            {
                container = new ContentPane();
                container.Content = item; //the content is the item being injected
                container.DataContext = ResolveDataContext(item); //make sure the dataContext is the same as the item. Most likely a ViewModel
                container.SetValue(IsGeneratedProperty, true); //we generated this one
                CreateDockAwareBindings(container);
            }

            container.SetValue(RegionProperty, region); //let's keep track of which region the container belongs to

            container.CloseAction = PaneCloseAction.RemovePane; //make it easy on ourselves and have the pane manage removing itself from the XamDockManager
            container.Closed += Container_Closed;

            return container;
        }

        void Container_Closed(object sender, PaneClosedEventArgs e)
        {
            ContentPane contentPane = sender as ContentPane;
            if (contentPane != null)
            {
                contentPane.Closed -= Container_Closed; //no memory leaks

                IRegion region = contentPane.GetValue(RegionProperty) as IRegion; //get the region associated with the ContentPane so that we can remove it.
                if (region != null)
                {
                    if (region.Views.Contains(contentPane)) //we are dealing with a ContentPane directly
                        region.Remove(contentPane);

                    var item = contentPane.Content; //this view was injected and set as the content of our ContentPane
                    if (item != null && region.Views.Contains(item))
                        region.Remove(item);
                }

                ClearContainerForItem(contentPane); //reduce memory leaks
            }
        }

        /// <summary>
        /// Checks to see if the item being injected implements the IDockAware interface and creates the necessary data bindings.
        /// </summary>
        /// <remarks>
        /// First we will check if the ContentPane.Content property implements the IDockAware interface. Depending on the type of item being injected,
        /// this may be a View or a ViewModel. If no IDockAware interface is found to create a binding, we next move to the ContentPane.DataContext property, which will 
        /// most likely be a ViewModel.
        /// </remarks>
        /// <param name="container"></param>
        void CreateDockAwareBindings(ContentPane contentPane)
        {
            Binding binding = new Binding("Header");

            //let's first check the item that was injected for IDockAware. This may be a View or ViewModel
            var dockAwareContent = contentPane.Content as IDockAware;
            if (dockAwareContent != null)
                binding.Source = dockAwareContent;

            //nothing was found on the item being injected, let's check the DataContext
            if (binding.Source == null)
            {
                //fall back to data context of the content pane, which is most likely a ViewModel
                var dockAwareDataContext = contentPane.DataContext as IDockAware;
                if (dockAwareDataContext != null)
                    binding.Source = dockAwareDataContext;
            }

            contentPane.SetBinding(HeaderedContentControl.HeaderProperty, binding);
        }

        /// <summary>
        /// Sets the Content property of a generated ContentPane to null.
        /// </summary>
        /// <param name="contentPane">The ContentPane</param>
        protected void ClearContainerForItem(ContentPane contentPane)
        {
            if ((bool)contentPane.GetValue(IsGeneratedProperty))
            {
                contentPane.ClearValue(HeaderedContentControl.HeaderProperty); //remove any bindings
                contentPane.Content = null;
            }
        }

        /// <summary>
        /// Finds the DataContext of an item.
        /// </summary>
        /// <remarks>
        /// If we are injecting a View, the result will be the Views DataContext. If we are injecting a ViewModel, the result will be the ViewModel.
        /// </remarks>
        /// <param name="item">The item</param>
        /// <returns>A Views DataContext or a ViewModel</returns>
        private object ResolveDataContext(object item)
        {
            FrameworkElement frameworkElement = item as FrameworkElement;
            return frameworkElement == null ? item : frameworkElement.DataContext;
        }
    }
}
