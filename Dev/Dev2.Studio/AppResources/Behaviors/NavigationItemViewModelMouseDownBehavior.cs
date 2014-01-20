using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
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
        readonly IEventAggregator _eventPublisher;

        public NavigationItemViewModelMouseDownBehavior()
            : this(EventPublishers.Aggregator)
        {
        }

        public NavigationItemViewModelMouseDownBehavior(IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
        }

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            SubscribeToEvents();
            _eventPublisher.Subscribe(this);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            UnsubscribeToEvents();
            _eventPublisher.Unsubscribe(this);
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

        #region DontAllowDoubleClick

        public bool DontAllowDoubleClick
        {
            get
            {
                return (bool)GetValue(DontAllowDoubleClickProperty);
            }
            set
            {
                SetValue(DontAllowDoubleClickProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for DontAllowDoubleClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DontAllowDoubleClickProperty =
            DependencyProperty.Register("DontAllowDoubleClick", typeof(bool), typeof(NavigationItemViewModelMouseDownBehavior), new PropertyMetadata(true));

        #endregion

        #region SelectOnRightClick

        public bool SelectOnRightClick
        {
            get { return (bool)GetValue(SelectOnRightClickProperty); }
            set { SetValue(SelectOnRightClickProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectOnRightClick.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectOnRightClickProperty =
            DependencyProperty.Register("SelectOnRightClick", typeof(bool), typeof(NavigationItemViewModelMouseDownBehavior), new PropertyMetadata(false));

        #endregion

        #endregion Dependency Properties

        #region Private Methods

        /// <summary>
        /// Subscribes to the associated objects events for this behavior
        /// </summary>
        private void SubscribeToEvents()
        {
            if(AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        /// <summary>
        /// Unsubscribes from the associated objects events for this behavior
        /// </summary>
        private void UnsubscribeToEvents()
        {
            if(AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        }

        #endregion Private Methods

        #region Event Handler Methods

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var treenode = AssociatedObject.DataContext as ITreeNode;

            if(treenode == null)
            {
                return;
            }

            //Select on rightclick
            if(SelectOnRightClick)
            {
                treenode.IsSelected = true;
            }

            //
            // Active environment logic
            //
            if(SetActiveEnvironmentOnClick && treenode.EnvironmentModel != null)
            {
                this.TraceInfo("Publish message of type - " + typeof(SetActiveEnvironmentMessage));
                _eventPublisher.Publish(new SetActiveEnvironmentMessage(treenode.EnvironmentModel));
            }

            var model = treenode as ResourceTreeViewModel;
            if(model != null)
            {
                var resourceTreeViewModel = model;
                if(resourceTreeViewModel.DataContext != null)
                {
                    //
                    // Double click logic
                    //
                    if(OpenOnDoubleClick && e.ClickCount == 2)
                    {
                        this.TraceInfo("Publish message of type - " + typeof(SetSelectedIContextualResourceModel));
                        _eventPublisher.Publish(new SetSelectedIContextualResourceModel(resourceTreeViewModel.DataContext, true));
                        if(!DontAllowDoubleClick)
                        {
                            if(resourceTreeViewModel.EditCommand.CanExecute(null))
                            {
                                resourceTreeViewModel.EditCommand.Execute(null);
                            }

                            //
                            // Event is set as handled to stop expansion of the treeview item if the behaviour is attached to an item in the treeview
                            //
                            e.Handled = true;
                        }
                    }
                    else if(OpenOnDoubleClick && e.ClickCount == 1)
                    {
                        this.TraceInfo("Publish message of type - " + typeof(SetSelectedIContextualResourceModel));
                        _eventPublisher.Publish(new SetSelectedIContextualResourceModel(resourceTreeViewModel.DataContext, false));
                    }

                }
            }
            else
            {
                this.TraceInfo("Publish message of type - " + typeof(SetSelectedIContextualResourceModel));
                _eventPublisher.Publish(new SetSelectedIContextualResourceModel(null, false));
            }
        }

        #endregion Event Handler Methods
    }

}
