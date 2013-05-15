using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Dev2.Studio.AppResources.AttachedProperties
{
    public static class UIElementTitleProperty
    {
        public static void SetTitle(UIElement element, string value)
        {
            element.SetValue(titleProperty, value);
        }

        public static string GetTitle(UIElement element)
        {
            return (string)element.GetValue(titleProperty);
        }

        public static readonly DependencyProperty titleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(UIElementTitleProperty), new PropertyMetadata(default(string)));
    }
}