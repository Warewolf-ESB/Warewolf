
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
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
            for(int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                if(depth > 0)
                {
                    foreach(var descendent in Descendents(child, --depth))
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
            while(current != null)
            {
                yield return current;
                current = VisualTreeHelper.GetParent(current);
            }
        }

        public static DependencyObject FindChildByToString(this DependencyObject parent, string criteria, bool partialMatch = false)
        {
            if(parent != null)
            {
                for(int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i) as UIElement;
                    if(child != null && (child.ToString() == criteria || (partialMatch && child.ToString().Contains(criteria))))
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
            if(visual == null)
            {
                throw new ArgumentNullException("visual");
            }

            var parent = VisualTreeHelper.GetParent(visual) as Visual;
            while(parent != null)
            {
                var adornerDecorator = parent as AdornerDecorator;
                if(adornerDecorator != null)
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
            if(element == null)
            {
                throw new ArgumentNullException("element");
            }

            var uiElement = element as UIElement;
            if(uiElement != null)
            {
                return uiElement;
            }

            var contentElement = element as ContentElement;
            if(contentElement != null)
            {
                var parent = ContentOperations.GetParent(contentElement)
                    ?? LogicalTreeHelper.GetParent(contentElement);
                if(parent != null)
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
            if(parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if(parent != null)
            {
                return parent;
            }

            // use recursion to proceed with next level
            return FindVisualParent<T>(parentObject);
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if(depObj != null)
            {
                for(int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if(child is T)
                    {
                        yield return (T)child;
                    }

                    foreach(T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static UIElement TryFindFirstTextBoxInActivity(this DependencyObject activity, bool lookinOutput = false)
        {
            var firstBorder = activity.FindChildByToString("Border", true);
            if(firstBorder != null)
            {
                var adornerDecorator = firstBorder.FindChildByToString("AdornerDecorator", true);
                if(adornerDecorator != null)
                {
                    var adornerLayer = adornerDecorator.FindChildByToString("AdornerLayer", true);
                    if(adornerLayer != null)
                    {
                        var topVisualsAdornerWrapper = adornerLayer.FindChildByToString("TopVisualsAdornerWrapper",
                            true);
                        if(topVisualsAdornerWrapper != null)
                        {
                            var firstGrid = topVisualsAdornerWrapper.FindChildByToString("Grid", true);
                            if(firstGrid != null)
                            {
                                var contentControl = firstGrid.FindChildByToString("ContentControl", true);
                                if(contentControl != null)
                                {
                                    var contentPresenter = contentControl.FindChildByToString("ContentPresenter",
                                        true);
                                    if(contentPresenter != null)
                                    {
                                        var secondBorder = contentPresenter.FindChildByToString("Border", true);
                                        if(secondBorder != null)
                                        {
                                            var secondGrid = secondBorder.FindChildByToString("Grid", true);
                                            if(secondGrid != null)
                                            {
                                                DependencyObject inputOrOutputGrid;
                                                if(!lookinOutput)
                                                {
                                                    inputOrOutputGrid =
                                                        secondGrid.FindChildByToString("Grid", true);
                                                }
                                                else
                                                {
                                                    inputOrOutputGrid = VisualTreeHelper.GetChild(secondGrid, 2) as UIElement;
                                                }
                                                if(inputOrOutputGrid != null)
                                                {
                                                    var dataGrid = inputOrOutputGrid.FindChildByToString("DataGrid", true);
                                                    if(dataGrid != null)
                                                    {
                                                        var thirdBorder = dataGrid.FindChildByToString("Border",
                                                            true);
                                                        if(thirdBorder != null)
                                                        {
                                                            var scrollViewer =
                                                                thirdBorder.FindChildByToString("ScrollViewer", true);
                                                            if(scrollViewer != null)
                                                            {
                                                                var fourthGrid =
                                                                    scrollViewer.FindChildByToString(
                                                                        "Grid", true);
                                                                if(fourthGrid != null)
                                                                {
                                                                    var scrollContentPresenter =
                                                                        fourthGrid.FindChildByToString(
                                                                            "ScrollContentPresenter", true);
                                                                    if(scrollContentPresenter != null)
                                                                    {
                                                                        var firstItemsPresenter =
                                                                            scrollContentPresenter
                                                                                .FindChildByToString(
                                                                                    "ItemsPresenter",
                                                                                    true);
                                                                        if(firstItemsPresenter != null)
                                                                        {
                                                                            var dataGridRowsPresenter =
                                                                                firstItemsPresenter
                                                                                    .FindChildByToString(
                                                                                        "DataGridRowsPresenter",
                                                                                        true);
                                                                            if(dataGridRowsPresenter != null)
                                                                            {
                                                                                var dataGridRow =
                                                                                    dataGridRowsPresenter
                                                                                        .FindChildByToString(
                                                                                            "DataGridRow", true);
                                                                                if(dataGridRow == null && !lookinOutput)
                                                                                {
                                                                                    return activity
                                                                                        .TryFindFirstTextBoxInActivity(
                                                                                            true);
                                                                                }
                                                                                if(dataGridRow != null)
                                                                                {
                                                                                    var fourthBorder =
                                                                                        dataGridRow
                                                                                            .FindChildByToString(
                                                                                                "Border",
                                                                                                true);
                                                                                    if(fourthBorder != null)
                                                                                    {
                                                                                        var selectiveScrollingGrid =
                                                                                            fourthBorder
                                                                                                .FindChildByToString
                                                                                                ("SelectiveScrollingGrid",
                                                                                                    true);
                                                                                        if(
                                                                                            selectiveScrollingGrid !=
                                                                                            null)
                                                                                        {
                                                                                            var
                                                                                                dataGridCellsPresenter
                                                                                                    = selectiveScrollingGrid
                                                                                                        .FindChildByToString
                                                                                                        ("DataGridCellsPresenter",
                                                                                                            true);
                                                                                            if(
                                                                                                dataGridCellsPresenter !=
                                                                                                null)
                                                                                            {
                                                                                                var
                                                                                                    secondItemsPresenter
                                                                                                        =
                                                                                                        dataGridCellsPresenter
                                                                                                            .FindChildByToString
                                                                                                            ("ItemsPresenter",
                                                                                                                true);
                                                                                                if(
                                                                                                    secondItemsPresenter !=
                                                                                                    null)
                                                                                                {
                                                                                                    var
                                                                                                        dataGridCellsPanel
                                                                                                            =
                                                                                                            secondItemsPresenter
                                                                                                                .FindChildByToString
                                                                                                                ("DataGridCellsPanel",
                                                                                                                    true);
                                                                                                    if(
                                                                                                        dataGridCellsPanel !=
                                                                                                        null)
                                                                                                    {
                                                                                                        DependencyObject
                                                                                                            dataGridCell;
                                                                                                        if(
                                                                                                            !lookinOutput)
                                                                                                        {
                                                                                                            dataGridCell
                                                                                                                =
                                                                                                                dataGridCellsPanel
                                                                                                                    .FindChildByToString
                                                                                                                    ("DataGridCell",
                                                                                                                        true);
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            dataGridCell
                                                                                                                =
                                                                                                                VisualTreeHelper
                                                                                                                    .GetChild
                                                                                                                    (dataGridCellsPanel,
                                                                                                                        1)
                                                                                                                    as
                                                                                                                    UIElement;
                                                                                                        }
                                                                                                        if(
                                                                                                            dataGridCell !=
                                                                                                            null)
                                                                                                        {
                                                                                                            var
                                                                                                                fifthBorder
                                                                                                                    =
                                                                                                                    dataGridCell
                                                                                                                        .FindChildByToString
                                                                                                                        ("Border",
                                                                                                                            true);
                                                                                                            if(
                                                                                                                fifthBorder !=
                                                                                                                null)
                                                                                                            {
                                                                                                                var
                                                                                                                    firstContentPresenter
                                                                                                                        =
                                                                                                                        fifthBorder
                                                                                                                            .FindChildByToString
                                                                                                                            ("ContentPresenter",
                                                                                                                                true);
                                                                                                                if(
                                                                                                                    firstContentPresenter !=
                                                                                                                    null)
                                                                                                                {
                                                                                                                    var
                                                                                                                        secondContentPresenter
                                                                                                                            = firstContentPresenter
                                                                                                                                .FindChildByToString
                                                                                                                                ("ContentPresenter",
                                                                                                                                    true);
                                                                                                                    if
                                                                                                                        (
                                                                                                                        secondContentPresenter !=
                                                                                                                        null)
                                                                                                                    {
                                                                                                                        return
                                                                                                                            (
                                                                                                                                UIElement
                                                                                                                                )
                                                                                                                                secondContentPresenter
                                                                                                                                    .FindChildByToString
                                                                                                                                    ("IntellisenseTextBox",
                                                                                                                                        true);
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                                                                        }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
