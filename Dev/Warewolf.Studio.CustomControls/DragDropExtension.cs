#pragma warning disable
﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Warewolf.Studio.CustomControls
{
    public static class DragDropExtension
    {
        #region ScrollOnDragDropProperty

        static readonly DependencyProperty ScrollOnDragDropProperty =
            DependencyProperty.RegisterAttached("ScrollOnDragDrop",
                typeof(bool),
                typeof(DragDropExtension),
                new PropertyMetadata(false, HandleScrollOnDragDropChanged));

        public static bool GetScrollOnDragDrop(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (bool)element.GetValue(ScrollOnDragDropProperty);
        }

        public static void SetScrollOnDragDrop(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(ScrollOnDragDropProperty, value);
        }

        static void HandleScrollOnDragDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var container = d as FrameworkElement;

            if (d == null)
            {
                Debug.Fail("Invalid type!");
            }

            Unsubscribe(container);

            if (true.Equals(e.NewValue))
            {
                Subscribe(container);
            }
        }

        static void Subscribe(FrameworkElement container)
        {
            container.PreviewDragOver += OnContainerPreviewDragOver;
        }

        static void OnContainerPreviewDragOver(object sender, DragEventArgs e)
        {
            var container = sender as FrameworkElement;

            if (container == null)
            {
                return;
            }

            var scrollViewer = GetFirstVisualChild<ScrollViewer>(container);

            if (scrollViewer == null)
            {
                return;
            }

            double tolerance = 60;
            var verticalPos = e.GetPosition(container).Y;
            double offset = 20;

            if (verticalPos < tolerance) // Top of visible list? 
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); //Scroll up. 
            }
            else
            {
                if (verticalPos > container.ActualHeight - tolerance) //Bottom of visible list? 
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); //Scroll down.     
                }
            }
        }

        static void Unsubscribe(FrameworkElement container)
        {
            container.PreviewDragOver -= OnContainerPreviewDragOver;
        }

        static T GetFirstVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T visualChild)
                    {
                        return visualChild;
                    }

                    var childItem = GetFirstVisualChild<T>(child);
                    if (childItem != null)
                    {
                        return childItem;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
