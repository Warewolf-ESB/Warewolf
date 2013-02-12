using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Dev2.Studio.Core.AppResources.ExtensionMethods
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<DependencyObject> GetDescendents(this DependencyObject dependencyObject)
        {
            List<DependencyObject> descendents = new List<DependencyObject>();

            if (dependencyObject == null)
            {
                return descendents;
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                descendents.Add(VisualTreeHelper.GetChild(dependencyObject, i));
            }

            foreach (DependencyObject descendent in descendents.ToList())
            {
                descendents.AddRange(GetDescendents(descendent));
            }

            return descendents;
        }
    }
}
