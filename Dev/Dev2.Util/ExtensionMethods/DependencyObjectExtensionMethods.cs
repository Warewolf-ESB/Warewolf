using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Dev2.Studio.AppResources.ExtensionMethods
{
    public static class DependencyObjectExtensionMethods
    {
        public static IEnumerable<DependencyObject> Descendents(this DependencyObject root, int depth)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                if (depth > 0)
                {
                    foreach (var descendent in Descendents(child, --depth))
                        yield return descendent;
                }
            }
        }

        public static IEnumerable<DependencyObject> Descendents(this DependencyObject root)
        {
            return Descendents(root, Int32.MaxValue);
        }

        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject root)
        {
            DependencyObject current = VisualTreeHelper.GetParent(root);
            while (current != null)
            {
                yield return current;
                current = VisualTreeHelper.GetParent(current);
            }
        }

        public static DependencyObject FindChildByToString(this DependencyObject parent, string criteria, bool partialMatch = false)
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i) as UIElement;
                    if (child != null && (child.ToString() == criteria || (partialMatch && child.ToString().Contains(criteria))))
                        return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the first <see cref="AdornerLayer"/> in the ancestral chain including 
        /// the <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="visual">
        /// The <see cref="DependencyObject"/> to get the <see cref="AdornerLayer"/>.
        /// </param>
        /// <returns>
        /// The first <see cref="AdornerLayer"/> in the ancestral chain including the 
        /// <paramref name="visual"/>.
        /// </returns>
        public static AdornerLayer GetAdornerLayer(this DependencyObject visual)
        {
            if (visual == null)
            {
                throw new ArgumentNullException("visual");
            }

            var parent = VisualTreeHelper.GetParent(visual) as Visual;
            while (parent != null)
            {
                var adornerDecorator = parent as AdornerDecorator;
                if (adornerDecorator != null)
                {
                    return adornerDecorator.AdornerLayer;
                }

                parent = VisualTreeHelper.GetParent(parent) as Visual;
            }

            return null;
        }

        /// <summary>
        /// Returns the first <see cref="UIElement"/> in the ancestral chain including 
        /// the element itself.
        /// </summary>
        /// <param name="element">
        /// The <see cref="DependencyObject"/> to get the containing UI element for.
        /// </param>
        /// <returns>
        /// The first <see cref="UIElement"/> that contains the <paramref name="element"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="element"/> is null.
        /// </exception>
        public static UIElement GetContainingElement(this DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            var uiElement = element as UIElement;
            if (uiElement != null)
            {
                return uiElement;
            }

            var contentElement = element as ContentElement;
            if (contentElement != null)
            {
                var parent = ContentOperations.GetParent(contentElement)
                    ?? LogicalTreeHelper.GetParent(contentElement);
                if (parent != null)
                {
                    return GetContainingElement(parent);
                }
            }

            return null;
        } 
        
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        public static T FindVisualParent<T>(this DependencyObject child)
          where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }

    }
}
