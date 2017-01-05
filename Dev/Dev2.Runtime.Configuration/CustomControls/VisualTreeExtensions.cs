/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using Warewolf.Resource.Errors;

namespace System.Windows.Controls
{
    /// <summary>
    /// A static class providing methods for working with the visual tree.  
    /// </summary>
    internal static class VisualTreeExtensions
    {
        /// <summary>
        /// Retrieves all the visual children of a framework element.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <returns>The visual children of the framework element.</returns>
        internal static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
        {
            Debug.Assert(parent != null, ErrorResource.ParentCannotBeNull);

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for(int counter = 0; counter < childCount; counter++)
            {
                yield return VisualTreeHelper.GetChild(parent, counter);
            }
        }

        /// <summary>
        /// Retrieves all the logical children of a framework element using a 
        /// breadth-first search.  A visual element is assumed to be a logical 
        /// child of another visual element if they are in the same namescope.
        /// For performance reasons this method manually manages the queue 
        /// instead of using recursion.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <returns>The logical children of the framework element.</returns>
        internal static IEnumerable<FrameworkElement> GetLogicalChildrenBreadthFirst(this FrameworkElement parent)
        {
            Debug.Assert(parent != null, ErrorResource.ParentCannotBeNull);

            Queue<FrameworkElement> queue =
                new Queue<FrameworkElement>(parent.GetVisualChildren().OfType<FrameworkElement>());

            while(queue.Count > 0)
            {
                FrameworkElement element = queue.Dequeue();
                yield return element;

                foreach(FrameworkElement visualChild in element.GetVisualChildren().OfType<FrameworkElement>())
                {
                    queue.Enqueue(visualChild);
                }
            }
        }
    }
}
