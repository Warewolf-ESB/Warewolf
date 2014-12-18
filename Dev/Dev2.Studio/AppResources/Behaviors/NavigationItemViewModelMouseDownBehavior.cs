
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Caliburn.Micro;
using Dev2.AppResources.Enums;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Models;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.AppResources.Behaviors
// ReSharper restore CheckNamespace
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

        #region NavigationViewContextType

        public NavigationViewContextType NavigationViewContextType
        {
            get
            {
                return (NavigationViewContextType)GetValue(NavigationViewContextTypeProperty);
            }
            set
            {
                SetValue(NavigationViewContextTypeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for NavigationViewContextTypeProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NavigationViewContextTypeProperty =
            DependencyProperty.Register("NavigationViewContextTypeProperty", typeof(NavigationViewContextType), typeof(NavigationItemViewModelDragDropBehavior), new PropertyMetadata(NavigationViewContextType.Explorer));

        #endregion

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
            e.Handled = OnMouseDown(AssociatedObject.DataContext as ExplorerItemModel, e.ClickCount);
        }

        protected bool OnMouseDown(IExplorerItemModel treenode, int clickCount)
        {
            if(treenode == null)
            {
                return false;
            }

            //Select on rightclick
            if(SelectOnRightClick)
            {
                SetIsSelected(treenode, true);
            }

            //
            // Active environment logic
            //
            if(SetActiveEnvironmentOnClick)
            {
                IEnvironmentModel environment = EnvironmentRepository.Instance.FindSingle(environmentModel => environmentModel.ID == treenode.EnvironmentId);
                _eventPublisher.Publish(new SetActiveEnvironmentMessage(environment));
            }

            var model = treenode;
            if(model.Permissions < Permissions.View)
            {
                SetIsSelected(treenode, true);
                return true;
            }

            //
            // Double click logic
            //
            if(OpenOnDoubleClick && clickCount == 2)
            {
                if(model.ResourceType >= ResourceType.Folder)
                {
                    SetIsExpanded(model);
                }
                if(model.CanEdit)
                {
                    model.EditCommand.Execute(null);
                }
            }

            SetIsSelected(treenode, true);
            return true;
        }

        void SetIsExpanded(IExplorerItemModel treenode)
        {
            switch(NavigationViewContextType)
            {
                case NavigationViewContextType.Explorer:
                    treenode.IsExplorerExpanded = !treenode.IsExplorerExpanded;
                    break;
                case NavigationViewContextType.ResourcePicker:
                    treenode.IsResourcePickerExpanded = !treenode.IsResourcePickerExpanded;
                    break;
                case NavigationViewContextType.DeploySource:
                    treenode.IsDeploySourceExpanded = !treenode.IsDeploySourceExpanded;
                    break;
                case NavigationViewContextType.DeployTarget:
                    treenode.IsDeployTargetExpanded = !treenode.IsDeployTargetExpanded;
                    break;
            }
        }

        void SetIsSelected(IExplorerItemModel treenode, bool value)
        {
            switch(NavigationViewContextType)
            {
                case NavigationViewContextType.Explorer:
                    treenode.IsExplorerSelected = value;
                    break;
                case NavigationViewContextType.ResourcePicker:
                    treenode.IsResourcePickerSelected = value;
                    break;
                case NavigationViewContextType.DeploySource:
                    treenode.IsDeploySourceSelected = value;
                    break;
                case NavigationViewContextType.DeployTarget:
                    treenode.IsDeployTargetSelected = value;
                    break;
            }
        }

        #endregion Event Handler Methods
    }

}
