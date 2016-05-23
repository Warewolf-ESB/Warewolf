using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Common.Interfaces;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Infragistics.Controls.Menus;
using Infragistics.DragDrop;

namespace Warewolf.Studio.CustomControls
{
    public class NavigationItemViewModelDragDropBehavior : Behavior<FrameworkElement>
    {
        #region Dependency Properties

        #region DontAllowDraging

        // Using a DependencyProperty as the backing store for SetActiveEnvironment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DontAllowDragingProperty =
            DependencyProperty.Register("DontAllowDraging", typeof(bool), typeof(NavigationItemViewModelDragDropBehavior), new PropertyMetadata(true));
        public bool DontAllowDraging
        {
            get
            {
                return (bool)GetValue(DontAllowDragingProperty);
            }
            set
            {
                SetValue(DontAllowDragingProperty, value);
            }
        }

        #endregion DontAllowDraging

        #endregion

        Point _lastMouseDown;

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            SubscribeToEvents();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            UnsubscribeToEvents();
        }

        #endregion Override Methods

        #region Private Methods

        /// <summary>
        ///     Subscribes to the associated objects events for this behavior
        /// </summary>
        void SubscribeToEvents()
        {
            if(AssociatedObject == null)
            {
                return;
            }

            var context = AssociatedObject.DataContext as XamDataTreeNodeDataContext;
            IExplorerItemViewModel navigationItemViewModel = null;
            if (context != null)
            {
                navigationItemViewModel = context.Data as IExplorerItemViewModel;
            }
            if(navigationItemViewModel == null)
            {
                return;
            }

            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.DragOver -= AssociatedObject_DragOver;
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;

            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        /// <summary>
        ///     Unsubscribes from the associated objects events for this behavior
        /// </summary>
        void UnsubscribeToEvents()
        {
            if(AssociatedObject != null)
            {
                AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
                AssociatedObject.DragOver -= AssociatedObject_DragOver;
                AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
                AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            }
        }

        #endregion Private Methods

        #region Event Handler Methods

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
        }

        void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
           
            var context = AssociatedObject.DataContext as XamDataTreeNodeDataContext;
            IExplorerItemViewModel dragSourceDataContext = null;
            if (context != null)
            {
                dragSourceDataContext = context.Data as IExplorerItemViewModel;
            }
            MouseMove(sender, e, dragSourceDataContext);
        }

        public void MouseMove(object sender, MouseEventArgs e, IExplorerItemViewModel dragSourceDataContext)
        {
            if(!DontAllowDraging)
            {
                var inputElement = sender as IInputElement;
                if (inputElement == null)
                {
                    var dragSource = sender as DragSource;
                    if (dragSource != null)
                    {
                        inputElement = dragSource.AssociatedObject;
                    }
                }
                var dependencyObject = sender as DependencyObject;

                if (dependencyObject != null && inputElement != null)
                {
                    Point currentPosition = e.GetPosition(inputElement);
                    var dragData = new DataObject();
                    if((Math.Abs(currentPosition.X - _lastMouseDown.X) > 2) || (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 2))
                    {
                        
                        if(dragSourceDataContext != null)
                        {
                            if(dragSourceDataContext.IsRenaming)
                            {
                                return;
                            }

                            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == dragSourceDataContext.Server.EnvironmentID);
                            bool hasPermissionToDrag = true;
                            if(environmentModel != null && environmentModel.AuthorizationService != null)
                            {
                                bool canExecute = environmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.Execute, dragSourceDataContext.ResourceId.ToString());
                                bool canView = environmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.View, dragSourceDataContext.ResourceId.ToString());
                                hasPermissionToDrag = canExecute && canView;
                            }
                            if(hasPermissionToDrag)
                            {
                                //if (dragSourceDataContext.ResourceType <= "WebService")
                                // FIX?
                                if (dragSourceDataContext.ResourceType.Contains("Service") && dragSourceDataContext.ResourceType != "ReservedService")
                                {
                                    dragData.SetData(DragDropHelper.WorkflowItemTypeNameFormat, dragSourceDataContext.ActivityName);
                                    dragData.SetData(dragSourceDataContext);
                                }
                                dragData.SetData(dragSourceDataContext);
                            }
                        }
                        DragDrop.DoDragDrop(dependencyObject, dragData, DragDropEffects.Link);
                    }
                }
            }
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
        }

        void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var inputElement = sender as IInputElement;

            if(e.ChangedButton == MouseButton.Left && inputElement != null)
            {
                _lastMouseDown = e.GetPosition(inputElement);
            }
        }

        #endregion Event Handler Methods
    }
}
