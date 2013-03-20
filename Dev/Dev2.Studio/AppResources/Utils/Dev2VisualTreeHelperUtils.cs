using System.Windows;
using System.Windows.Media;

namespace Dev2.Studio.AppResources.Utils
{
    static class Dev2VisualTreeHelperUtils
    {
        public static UIElement FindVisualChild(UIElement parent, string criteria)
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i) as UIElement;
                    if (child != null && child.ToString().Contains(criteria))
                        return child;
                }
            }
            return null;
        }
    }
}
