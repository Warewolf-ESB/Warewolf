using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Dev2.CustomControls.Behavior {
    public class ControlEnterToTabBehavior : Behavior<Control>, IDisposable 
    {
        public int NumberOfMoves
        {
            get { return (int)GetValue(NumberOfMovesProperty); }
            set { SetValue(NumberOfMovesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumberOfMoves.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfMovesProperty =
            DependencyProperty.Register("NumberOfMoves", typeof(int), typeof(ControlEnterToTabBehavior), new PropertyMetadata(1));

        
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
        }

        void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e) {
           
            if (e.Key == Key.Enter && (e.KeyboardDevice.Modifiers != ModifierKeys.Shift && e.KeyboardDevice.Modifiers != ModifierKeys.Control))
            {
                var count = 0;
                while (count < NumberOfMoves)
                {
                    var scope = FocusManager.GetFocusScope(AssociatedObject);
                    var focusedObject = FocusManager.GetFocusedElement(scope) as FrameworkElement;
                    if (focusedObject != null)
                    {
                        focusedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                    count++;
                }
                e.Handled = true;
            }
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            Dispose();
        }

        public void Dispose() {
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }
    }
}
