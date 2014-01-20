using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Dev2.Providers.Logs;
using Dev2.Studio.ViewModels.Navigation;

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

            var navigationItemViewModel = AssociatedObject.DataContext as ResourceTreeViewModel;

            if(navigationItemViewModel == null)
            {
                return;
            }

            var resource = navigationItemViewModel.DataContext;

            if(resource == null)
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
                this.TraceInfo("Drag Is Allowed");
                var inputElement = sender as IInputElement;
                var dependencyObject = sender as DependencyObject;

                if(e.LeftButton == MouseButtonState.Pressed && inputElement != null && dependencyObject != null && _dragSource != null)
                {
                    this.TraceInfo("Starting Drag");
                    Point currentPosition = e.GetPosition(inputElement);

                    if((Math.Abs(currentPosition.X - _lastMouseDown.X) > 2) || (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 2))
                    {
                        var dragData = new DataObject();
                        var dragSourceDataContext = _dragSource.DataContext as ResourceTreeViewModel;
                        this.TraceInfo("Got DataContext");
                        if(dragSourceDataContext != null)
                        {
                            if(dragSourceDataContext.IsRenaming)
                            {
                                return;
                            }

                            dragSourceDataContext.IsNew = true;
                            this.TraceInfo("Set IsNew");
                            if(!string.IsNullOrEmpty(dragSourceDataContext.ActivityFullName))
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

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
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
