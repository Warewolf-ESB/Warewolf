using System.Windows;
using System.Windows.Interactivity;

namespace Dev2.CustomControls.Behavior
{
    public class ActualSizeBindingBehavior : Behavior<FrameworkElement>
    {


        public double ActualHeight
        {
            get { return (double)GetValue(ActualHeightProperty); }
            set { SetValue(ActualHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualHeightProperty =
            DependencyProperty.Register("ActualHeight", typeof (double),
                                        typeof (ActualSizeBindingBehavior),
                                        new PropertyMetadata(0D));

        public double ActualWidth
        {
            get { return (double)GetValue(ActualWidthProperty); }
            set { SetValue(ActualWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualWidth.  This enables animation, styling, binding, etc...
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
            ActualHeight = AssociatedObject.ActualHeight;
            ActualWidth = AssociatedObject.ActualWidth;
        }
    }
}
