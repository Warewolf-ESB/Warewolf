/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Windows;
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
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                if (depth > 0)
                {
                    foreach (DependencyObject descendent in Descendents(child, --depth))
                        yield return descendent;
                }
            }
        }

        public static IEnumerable<DependencyObject> Descendents(this DependencyObject root)
        {
            return Descendents(root, Int32.MaxValue);
        }

        /// <summary>
        ///     Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>
        ///     The first parent item that matches the submitted type parameter.
        ///     If not matching item can be found, a null reference is being returned.
        /// </returns>
        public static T FindVisualParent<T>(this DependencyObject child)
            where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we've reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we're looking for
            var parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }

            // use recursion to proceed with next level
            return FindVisualParent<T>(parentObject);
        }
    }
}
