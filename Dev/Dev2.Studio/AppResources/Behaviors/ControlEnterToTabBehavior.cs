using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors {
    public class ControlEnterToTabBehavior : Behavior<Control>, IDisposable 
    {
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        void AssociatedObject_Unloaded(object sender, RoutedEventArgs e) {
            Dispose();
        }

        void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e) {
           
            if (e.Key == Key.Enter && (e.KeyboardDevice.Modifiers != ModifierKeys.Shift && e.KeyboardDevice.Modifiers != ModifierKeys.Control)) {
               AssociatedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
               AssociatedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        public void Dispose() {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }
    }
}
