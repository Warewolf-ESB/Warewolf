
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Common.Interfaces.Data;
using Dev2.Models;
using Dev2.Services.Security;
using Dev2.Studio.Core;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.AppResources.Behaviors
{
    public class NavigationItemViewModelDragDropBehavior : Behavior<FrameworkElement>
    {


        #region Dependency Properties

        #region DontAllowDraging

        public bool DontAllowDraging
        {
            get { return (bool)GetValue(DontAllowDragingProperty); }
            set { SetValue(DontAllowDragingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SetActiveEnvironment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DontAllowDragingProperty =
            DependencyProperty.Register("DontAllowDraging", typeof(bool), typeof(NavigationItemViewModelDragDropBehavior), new PropertyMetadata(true));

        #endregion DontAllowDraging

        #endregion

        #region Class Members

        private FrameworkElement _dragSource;
        private Point _lastMouseDown;

        #endregion Class Members

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
        /// Subscribes to the associated objects events for this behavior
        /// </summary>
        private void SubscribeToEvents()
        {
            if(AssociatedObject == null)
            {
                return;
            }

            var navigationItemViewModel = AssociatedObject.DataContext as ExplorerItemModel;

            if(navigationItemViewModel == null)
            {
                return;
            }

            //var resource = navigationItemViewModel.DataContext;

            //if(resource == null)
            //{
            //    return;
            //}

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
        /// Unsubscribes from the associated objects events for this behavior
        /// </summary>
        private void UnsubscribeToEvents()
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

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            //UnsubscribeToEvents();
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if(!DontAllowDraging)
            {
                var inputElement = sender as IInputElement;
                var dependencyObject = sender as DependencyObject;

                if(e.LeftButton == MouseButtonState.Pressed && inputElement != null && dependencyObject != null && AssociatedObject != null)
                {
                    Point currentPosition = e.GetPosition(inputElement);

                    if((Math.Abs(currentPosition.X - _lastMouseDown.X) > 2) || (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 2))
                    {
                        var dragData = new DataObject();
                        var dragSourceDataContext = AssociatedObject.DataContext as ExplorerItemModel;
                        if(dragSourceDataContext != null)
                        {
                            if(dragSourceDataContext.IsRenaming)
                            {
                                return;
                            }

                            //dragSourceDataContext.IsNew = true;
                            var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == dragSourceDataContext.EnvironmentId);
                            var hasPermissionToDrag = true;
                            if(environmentModel != null && environmentModel.AuthorizationService != null)
                            {
                                var canExecute = environmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.Execute, dragSourceDataContext.ResourceId.ToString());
                                var canView = environmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.View, dragSourceDataContext.ResourceId.ToString());
                                hasPermissionToDrag = canExecute && canView;
                            }
                            if(hasPermissionToDrag)
                            {
                                if(dragSourceDataContext.ResourceType <= ResourceType.WebService)
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

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var inputElement = sender as IInputElement;

            if(e.ChangedButton == MouseButton.Left && inputElement != null)
            {
                _lastMouseDown = e.GetPosition(inputElement);
                _dragSource = sender as FrameworkElement;
            }
        }

        #endregion Event Handler Methods
    }
}
