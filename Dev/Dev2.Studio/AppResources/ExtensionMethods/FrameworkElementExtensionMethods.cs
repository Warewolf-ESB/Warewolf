
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;
using System.Windows.Media;

// ReSharper disable once CheckNamespace
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
            if(dp == null)
            {
                return null;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(dp);
            for(int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dp, i);

                FrameworkElement feChild = child as FrameworkElement;
                if(feChild != null && (feChild.Name == name || (partialMatch && feChild.Name.Contains(name))))
                {
                    return feChild;
                }
            }

            for(int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dp, i);

                FrameworkElement recursiveResult = FindNameAcrossNamescopesImpl(child, name, partialMatch);
                if(recursiveResult != null)
                {
                    return recursiveResult;
                }
            }

            return null;
        }
    }
}
