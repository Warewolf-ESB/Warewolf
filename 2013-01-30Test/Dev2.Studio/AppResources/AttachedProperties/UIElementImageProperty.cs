using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dev2.Studio.AppResources.AttachedProperties
{
    public static class UIElementImageProperty
    {
        public static void SetImage(UIElement element, string value)
        {
            element.SetValue(imageProperty, value);
        }

        public static string GetImage(UIElement element)
        {
            return (string)element.GetValue(imageProperty);
        }

        public static readonly DependencyProperty imageProperty =
            DependencyProperty.RegisterAttached("Image", typeof(string), typeof(UIElementImageProperty), new PropertyMetadata(default(string)));
    }
}
