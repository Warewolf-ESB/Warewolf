using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Navigation;

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
            if(AssociatedObject == null)
            {
                return;
            }
            SubscribeToEvents();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if(AssociatedObject == null)
            {
                return;
            }
            UnsubscribeFromEvents();
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
        protected virtual void SubscribeToEvents()
        {
            if(AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.MouseDown -= OnMouseDown;
            AssociatedObject.MouseDown += OnMouseDown;
            _eventPublisher.Subscribe(this);
        }

        /// <summary>
        /// Unsubscribes from the associated objects events for this behavior
        /// </summary>
        protected virtual void UnsubscribeFromEvents()
        {
            if(AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.MouseDown -= OnMouseDown;
            _eventPublisher.Unsubscribe(this);
        }

        #endregion Private Methods

        #region Event Handler Methods

        protected void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = OnMouseDown(AssociatedObject.DataContext as ITreeNode, e.ClickCount);
        }

        protected bool OnMouseDown(ITreeNode treenode, int clickCount)
        {
            if(treenode == null)
            {
                return false;
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
                    if(!resourceTreeViewModel.DataContext.IsAuthorized(AuthorizationContext.View))
                    {
                        return true;
                    }

                    //
                    // Double click logic
                    //
                    if(OpenOnDoubleClick && clickCount == 2)
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
                            return true;
                        }
                    }
                    else if(OpenOnDoubleClick && clickCount == 1)
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
            return false;
        }

        #endregion Event Handler Methods
    }

}
