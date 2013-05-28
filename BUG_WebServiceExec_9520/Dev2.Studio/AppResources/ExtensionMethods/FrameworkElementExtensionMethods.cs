using System.Windows;
using System.Windows.Media;

namespace Dev2.Studio.AppResources.ExtensionMethods
{
    public static class FrameworkElementExtensionMethods
    {
        /// <summary>
        /// Finds and element by name across namescopes.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="partialMatch">Indicates if a partial name match should be performed</param>
        public static FrameworkElement FindNameAcrossNamescopes(this FrameworkElement frameworkElement, string name, bool partialMatch = false)
        {
            return FindNameAcrossNamescopesImpl(frameworkElement, name, partialMatch);
        }

        private static FrameworkElement FindNameAcrossNamescopesImpl(DependencyObject dp, string name, bool partialMatch = false)
        {
            if (dp == null)
            {
                return null;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(dp);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dp, i);

                FrameworkElement feChild = child as FrameworkElement;
                if (feChild != null && (feChild.Name == name || (partialMatch && feChild.Name.Contains(name))))
                {
                    return feChild;
                }
            }

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dp, i);

                FrameworkElement recursiveResult = FindNameAcrossNamescopesImpl(child, name, partialMatch);
                if (recursiveResult != null)
                {
                    return recursiveResult;
                }
            }

            return null;
        }
    }
}
