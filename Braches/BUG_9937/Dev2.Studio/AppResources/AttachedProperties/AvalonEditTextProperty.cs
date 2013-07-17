using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dev2.Studio.AttachedProperties {
    public static class AvalonEditTextProperty {
        public static readonly DependencyProperty propertyNameProperty =
            DependencyProperty.RegisterAttached("propertyName", typeof (string), typeof (AvalonEditTextProperty), new PropertyMetadata(default(string)));

        public static void SetpropertyName(UIElement element, string value) {
            element.SetValue(propertyNameProperty, value);
        }

        public static string GetpropertyName(UIElement element) {
            return (string) element.GetValue(propertyNameProperty);
        }
    }
}
