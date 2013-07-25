using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Activities.Designers;
using Dev2.Studio.AppResources.ExtensionMethods;

namespace Dev2.Activities.AttachedProperties
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
            var uiElement = (UIElement) o;
            var helpText = (string) e.NewValue;
            uiElement.IsKeyboardFocusedChanged += (sender, args) =>
                {
                    var isFocused = (bool)args.NewValue;
                    if (isFocused)
                    {
                        var activityDesigner = uiElement.FindVisualParent<ActivityDesignerBase>();
                        activityDesigner.HelpText = helpText;
                    }
                };
        }
    }
}
