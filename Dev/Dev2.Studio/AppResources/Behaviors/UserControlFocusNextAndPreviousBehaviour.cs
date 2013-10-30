using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class UserControlFocusNextAndPreviousBehaviour : Behavior<UserControl>, IDisposable
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            routedEventArgs.Handled = true;
        }

        void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;
            if (e.Key == Key.Up)
            {
                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                }
            }
            else if ((e.KeyboardDevice.IsKeyDown(Key.LeftShift)) && e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                    e.Handled = true;
                }
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            base.OnDetaching();
        }

        public void Dispose()
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }

    }
}
