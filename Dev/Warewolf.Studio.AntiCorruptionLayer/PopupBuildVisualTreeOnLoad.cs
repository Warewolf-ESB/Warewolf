using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class PopupBuildVisualTreeOnLoad : Behavior<Popup>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            AssociatedObject.Visibility = System.Windows.Visibility.Hidden;
            AssociatedObject.IsOpen = true;
            AssociatedObject.IsOpen = false;
            AssociatedObject.Visibility = System.Windows.Visibility.Visible;
        }
    }
}