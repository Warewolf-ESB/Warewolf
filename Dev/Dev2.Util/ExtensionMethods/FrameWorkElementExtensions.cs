using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Util.ExtensionMethods
{
    public static class FrameWorkElementExtensions
    {
        public static void BringToFront(this FrameworkElement element)
        {
            if(element == null) return;

            var parent = element.Parent as Panel;
            if(parent == null) return;

            var maxZ = parent.Children.OfType<UIElement>()
              .Where(x => !Equals(x, element))
              .Select(Panel.GetZIndex)
              .ToList();

            if(!maxZ.Any())
            {
                return;
            }
            var max = maxZ.Max();
            Panel.SetZIndex(element, max + 1);
        }

        public static void BringToMaxFront(this FrameworkElement element)
        {
            if(element == null)
            {
                return;
            }

            Panel.SetZIndex(element, Int32.MaxValue);
        }

        public static void SendToBack(this FrameworkElement element)
        {
            if(element == null)
            {
                return;
            }

            Panel.SetZIndex(element, Int32.MinValue);
        }
    }
}
