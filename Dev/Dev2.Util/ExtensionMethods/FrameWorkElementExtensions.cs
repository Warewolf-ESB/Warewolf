using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Util.ExtensionMethods
{
    public static class FrameWorkElementExtensions
    {
        public static void BringToFront(this FrameworkElement element)
        {
            if (element == null) return;

            var parent = element.Parent as Panel;
            if (parent == null) return;

            var maxZ = parent.Children.OfType<UIElement>()
              .Where(x => !Equals(x, element))
              .Select(Panel.GetZIndex)
              .ToList();

            if (!maxZ.Any())
            {
                return;
            }
            var max = maxZ.Max();
            Panel.SetZIndex(element, max + 1);
        }
    }
}
