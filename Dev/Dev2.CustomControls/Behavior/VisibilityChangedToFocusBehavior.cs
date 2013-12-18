using System.Windows;
using System.Windows.Interactivity;

namespace Dev2.CustomControls.Behavior
{
    public class VisibilityChangedToFocusBehavior : Behavior<UIElement>
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged += AssociatedObjectOnIsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged -= AssociatedObjectOnIsVisibleChanged;
        }


        void AssociatedObjectOnIsVisibleChanged(object o, DependencyPropertyChangedEventArgs args)
        {            
            if ((bool)args.NewValue && !(bool)args.OldValue)
            {
                AssociatedObject.Focus();
            }
        }
    }
}
