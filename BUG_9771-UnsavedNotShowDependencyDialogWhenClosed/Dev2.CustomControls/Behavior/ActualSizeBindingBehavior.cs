using System.Windows;
using System.Windows.Interactivity;

namespace Dev2.CustomControls.Behavior
{
    public class ActualSizeBindingBehavior : Behavior<FrameworkElement>
    {
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register("HorizontalOffset", typeof(double), 
            typeof(ActualSizeBindingBehavior), new PropertyMetadata(0D));

        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register("VerticalOffset", typeof(double), 
            typeof(ActualSizeBindingBehavior), new PropertyMetadata(0D));       

        public double ActualHeight
        {
            get { return (double)GetValue(ActualHeightProperty); }
            set { SetValue(ActualHeightProperty, value); }
        }

        public static readonly DependencyProperty ActualHeightProperty =
            DependencyProperty.Register("ActualHeight", typeof (double),
                                        typeof (ActualSizeBindingBehavior),
                                        new PropertyMetadata(0D));

        public double ActualWidth
        {
            get { return (double)GetValue(ActualWidthProperty); }
            set { SetValue(ActualWidthProperty, value); }
        }

        public static readonly DependencyProperty ActualWidthProperty =
            DependencyProperty.Register("ActualWidth", typeof(double), 
            typeof(ActualSizeBindingBehavior), new PropertyMetadata(0D));


        protected override void OnAttached()
        {
            base.OnAttached();
            AttachEvents();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            DetachEvents();
        }

        private void DetachEvents()
        {
            AssociatedObject.SizeChanged -= SizeChanged;
        }

        private void AttachEvents()
        {
            AssociatedObject.SizeChanged += SizeChanged;
        }

        private void SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ActualHeight = AssociatedObject.ActualHeight - VerticalOffset;
            ActualWidth = AssociatedObject.ActualWidth - HorizontalOffset;
        }
    }
}
