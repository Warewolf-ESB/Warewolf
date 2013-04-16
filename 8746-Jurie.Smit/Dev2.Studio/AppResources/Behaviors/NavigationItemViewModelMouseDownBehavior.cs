using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class NavigationItemViewModelMouseDownBehavior : Behavior<FrameworkElement>
    {
        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            SubscribeToEvents();
            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
            EventAggregator.Subscribe(this);
        }

        protected IEventAggregator EventAggregator { get; set; }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            UnsubscribeToEvents();
        }

        #endregion Override Methods

        #region Dependency Properties

        #region SetActiveEnvironmentOnClick

        public bool SetActiveEnvironmentOnClick
        {
            get { return (bool)GetValue(SetActiveEnvironmentOnClickProperty); }
            set { SetValue(SetActiveEnvironmentOnClickProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SetActiveEnvironment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SetActiveEnvironmentOnClickProperty =
            DependencyProperty.Register("SetActiveEnvironmentOnClick", typeof(bool), typeof(NavigationItemViewModelMouseDownBehavior), new PropertyMetadata(false));

        #endregion SetActiveEnvironment

        #region OpenOnDoubleClick

        public bool OpenOnDoubleClick
        {
            get { return (bool)GetValue(OpenOnDoubleClickProperty); }
            set { SetValue(OpenOnDoubleClickProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenOnDoubleClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenOnDoubleClickProperty =
            DependencyProperty.Register("OpenOnDoubleClick", typeof(bool), typeof(NavigationItemViewModelMouseDownBehavior), new PropertyMetadata(false));

        #endregion OpenOnDoubleClick

        #region SelectOnClick

        public bool SelectOnClick
        {
            get { return (bool)GetValue(SelectOnClickProperty); }
            set { SetValue(SelectOnClickProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectOnClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectOnClickProperty =
            DependencyProperty.Register("SelectOnClick", typeof(bool), typeof(NavigationItemViewModelMouseDownBehavior), new PropertyMetadata(false));

        #endregion SelectOnClick

        #endregion Dependency Properties

        #region Private Methods

        /// <summary>
        /// Subscribes to the associated objects events for this behavior
        /// </summary>
        private void SubscribeToEvents()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
                AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;

                AssociatedObject.MouseDown += AssociatedObject_MouseDown;
                AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
            }
        }

        /// <summary>
        /// Unsubscribes from the associated objects events for this behavior
        /// </summary>
        private void UnsubscribeToEvents()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
                AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            }
        }

        #endregion Private Methods

        #region Event Handler Methods

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            //UnsubscribeToEvents();
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ITreeNode treenode = AssociatedObject.DataContext as ITreeNode;

            if (treenode != null)
            {
                //
                // Select logic
                //
                if (SelectOnClick)
                {
                    treenode.IsSelected = true;
                }

                //
                // Active environment logic
                //
                if (SetActiveEnvironmentOnClick && treenode.EnvironmentModel != null)
                {
                    EventAggregator.Publish(new SetActiveEnvironmentMessage(treenode.EnvironmentModel));
                }

                if (treenode is ResourceTreeViewModel)
                {
                    var resourceTreeViewModel = (ResourceTreeViewModel) treenode;
                    //
                    // Double click logic
                    //
                    if (OpenOnDoubleClick && e.ClickCount == 2 && resourceTreeViewModel.DataContext != null)
                    {
                        if (resourceTreeViewModel.EditCommand.CanExecute(null))
                        {
                            resourceTreeViewModel.EditCommand.Execute(null);
                        }

                        //
                        // Event is set as handled to stop expansion of the treeview item if the behaviour is attached to an item in the treeview
                        //
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion Event Handler Methods
    }

}
