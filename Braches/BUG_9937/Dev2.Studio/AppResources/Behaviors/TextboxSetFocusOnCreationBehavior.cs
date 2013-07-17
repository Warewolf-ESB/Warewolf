using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class TextboxSetFocusOnCreationBehavior : Behavior<TextBox>, IDisposable
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.LayoutUpdated += AssociatedObject_Loaded;           
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.LayoutUpdated -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }

        void AssociatedObject_Loaded(object sender, EventArgs e)
        {
            AssociatedObject.Focus();
        }       

        protected override void OnDetaching()
        {
            AssociatedObject.LayoutUpdated -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            base.OnDetaching();
        }

        public void Dispose()
        {
            AssociatedObject.LayoutUpdated -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }
    }
}
