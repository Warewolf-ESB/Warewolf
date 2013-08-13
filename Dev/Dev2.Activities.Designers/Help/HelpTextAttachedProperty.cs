using System;
using System.Windows;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.Activities.Help
{
    public static class HelpText
    {
        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for HelptText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(HelpText), 
            new PropertyMetadata(string.Empty, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var uiElement = (UIElement)o;
            DependencyPropertyChangedEventHandler handler = delegate(object sender, DependencyPropertyChangedEventArgs args)
                {
                var helpText = (string)e.NewValue;
                    var isFocused = (bool)args.NewValue;
                    if (isFocused)
                    {
                        var overlay = uiElement.FindVisualParent<ActivityTemplate>();
                        var parentAsAdorner = overlay.Parent as AdornerPresenterBase;
                    if (parentAsAdorner != null)
                        {
                            var presenter = parentAsAdorner;
                            var activityDesigner = presenter.AssociatedActivityDesigner;
                            activityDesigner.HelpText = helpText;
                        }
                    }
                };
            uiElement.IsKeyboardFocusedChanged -= handler;
            uiElement.IsKeyboardFocusedChanged += handler;
        }
    }
}
