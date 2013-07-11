using Dev2.Studio.ActivityDesigners.Singeltons;
using Dev2.Studio.ViewModels.Navigation;
using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

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
            if (AssociatedObject == null)
            {
                return;
            }

            var navigationItemViewModel = AssociatedObject.DataContext as ResourceTreeViewModel;

            if (navigationItemViewModel == null)
            {
                return;
            }

            var resource = navigationItemViewModel.DataContext;

            if (resource == null || navigationItemViewModel.WizardEngine == null || navigationItemViewModel.WizardEngine.IsResourceWizard(resource))
            {
                return;
            }

            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.Drop -= AssociatedObject_Drop;
            AssociatedObject.DragOver -= AssociatedObject_DragOver;
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;

            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.Drop += AssociatedObject_Drop;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        /// <summary>
        /// Unsubscribes from the associated objects events for this behavior
        /// </summary>
        private void UnsubscribeToEvents()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
                AssociatedObject.Drop -= AssociatedObject_Drop;
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
            if (!DontAllowDraging)
            {
                IInputElement inputElement = sender as IInputElement;
                DependencyObject dependencyObject = sender as DependencyObject;

                if (e.LeftButton == MouseButtonState.Pressed && inputElement != null && dependencyObject != null && _dragSource != null)
                {
                    Point currentPosition = e.GetPosition(inputElement);

                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 2) ||
                        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 2))
                    {
                        DataObject dragData = new DataObject();
                        ResourceTreeViewModel dragSourceDataContext = _dragSource.DataContext as ResourceTreeViewModel;
                        if (dragSourceDataContext != null)
                        {
                            dragSourceDataContext.IsNew = true;
                            if (!string.IsNullOrEmpty(dragSourceDataContext.ActivityFullName))
                            {
                                dragData.SetData(DragDropHelper.WorkflowItemTypeNameFormat, dragSourceDataContext.ActivityFullName);
                                dragData.SetData(dragSourceDataContext);
                            }

                            dragData.SetData(dragSourceDataContext);
                        }
                        DragDrop.DoDragDrop(dependencyObject, dragData, DragDropEffects.Link);                      
                    }
                }
            }
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            // 2012-10-23 - Brendon.Page - This code was moved from the navigation view for PBI 5559, even though it was commented out,
            //                             incase the functionality is ot be reinstated.

            //var dragSource = e.Data.GetData(typeof(NavigationItemViewModel));

            //var dragNavigationItem = dragSource as NavigationItemViewModel;
            //if (dragNavigationItem != null) {

            //    var dropTargetNavigationItem = (e.Source as dynamic).DataContext as NavigationItemViewModel;

            //    if (dragNavigationItem == dropTargetNavigationItem) {
            //        return;
            //    }

            //    if (dragNavigationItem.IsServerLevel) {
            //        if ((dropTargetNavigationItem != null) && dropTargetNavigationItem.IsServerLevel) {
            //            var sourceItem = dragNavigationItem.DataContext as EnvironmentModel;
            //            var targetItem = dropTargetNavigationItem.DataContext as EnvironmentModel;

            //            if (sourceItem != null && targetItem != null) {
            //                var newItemList = sourceItem.Resources.All().ToList().Union(targetItem.Resources.All(), new ResourceModelEqualityComparer());


            //                newItemList.ToList().ForEach(c=> dropTargetNavigationItem.EnvironmentModel.Resources.Save(c));
            //                Mediator.SendMessage(MediatorMessages.ShowNavigation, dragNavigationItem);
            //            }

            //        }

            //    }

            //    if (dragNavigationItem.IsCategory) {
            //        //This is a category being dropped onto a resource
            //        //set all the items in the source category to the target category;
            //        if (dropTargetNavigationItem != null) {


            //            if (dropTargetNavigationItem.IsCategory) {
            //                var sourceItems = dragNavigationItem.DataContext as IEnumerable<ResourceModel>;
            //                if (sourceItems != null) {
            //                    var targetItems = dropTargetNavigationItem.DataContext as IEnumerable<ResourceModel>;
            //                    if (targetItems != null) {
            //                        var sourceItemsList = sourceItems.ToList();
            //                        sourceItemsList.ToList().ForEach(c => c.Category = dropTargetNavigationItem.Name);
            //                        Mediator.SendMessage(MediatorMessages.ShowNavigation, dragNavigationItem);
            //                    }
            //                }
            //            }


            //            if (!dropTargetNavigationItem.IsCategory && !dropTargetNavigationItem.IsServerLevel) {
            //                var sourceItems = dragNavigationItem.DataContext as IEnumerable<ResourceModel>;
            //                if (sourceItems != null) {
            //                    var targetItem = dropTargetNavigationItem.DataContext as ResourceModel;
            //                    if (targetItem != null) {
            //                        var sourceItemsList = sourceItems.ToList();
            //                        sourceItemsList.ToList().ForEach(c => c.Category = targetItem.Category);
            //                        Mediator.SendMessage(MediatorMessages.ShowNavigation, dragNavigationItem);
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    if (!dragNavigationItem.IsCategory && !dragNavigationItem.IsServerLevel) {
            //        if (dropTargetNavigationItem != null) {
            //            if (!dropTargetNavigationItem.IsCategory && !dropTargetNavigationItem.IsServerLevel) {
            //                var sourceItem = dragNavigationItem.DataContext as ResourceModel;
            //                if (sourceItem != null) {
            //                    var targetItem = dropTargetNavigationItem.DataContext as ResourceModel;
            //                    if (targetItem != null) {
            //                        sourceItem.Category = targetItem.Category;
            //                        Mediator.SendMessage(MediatorMessages.ShowNavigation, dragNavigationItem);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IInputElement inputElement = sender as IInputElement;

            if (e.ChangedButton == MouseButton.Left && inputElement != null)
            {
                _lastMouseDown = e.GetPosition(inputElement);
                _dragSource = sender as FrameworkElement;
            }
        }

        #endregion Event Handler Methods
    }
}
