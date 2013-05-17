using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class ThumbResizeBehavior : Behavior<Thumb>
    {
        #region Class Members

        private readonly int _changeThreshold = 1;

        #endregion Class Members

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
            AssociatedObject.DragDelta += AssociatedObject_DragDelta;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
            AssociatedObject.DragDelta -= AssociatedObject_DragDelta;
        }

        #endregion Override Methods

        #region Attached Behaviours

        #region TargetElement

        public FrameworkElement TargetElement
        {
            get { return (FrameworkElement)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register("TargetElement", typeof(FrameworkElement), typeof(ThumbResizeBehavior), new PropertyMetadata(null));

        #endregion TargetElement

        #endregion Attached Behaviours

        #region Event Handlers

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.DragDelta -= AssociatedObject_DragDelta;
        }

        private void AssociatedObject_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (TargetElement == null)
            {
                return;
            }

            //
            // If the height or with is not a number get the actual value.
            //
            if (double.IsNaN(TargetElement.Height))
            {
                TargetElement.Height = TargetElement.ActualHeight;
            }

            if (double.IsNaN(TargetElement.Width))
            {
                TargetElement.Width = TargetElement.ActualWidth;
            }

            if (TargetElement.Height + e.VerticalChange > 0)
            {
                if (Math.Abs(e.VerticalChange) > _changeThreshold)
                {
                    TargetElement.Height += e.VerticalChange;
                }
            }
            else
            {
                TargetElement.Height = TargetElement.MinHeight;
            }

            if (TargetElement.Width + e.HorizontalChange > 0)
            {
                if (Math.Abs(e.HorizontalChange) > _changeThreshold)
                {
                    TargetElement.Width += e.HorizontalChange;
                }
            }
            else
            {
                TargetElement.Width = TargetElement.MinWidth;
            }
        }

        #endregion Event Handlers
    }
}
