
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - August 2006 

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Dev2.CustomControls.Panels
{
    /// <summary>
    /// A Canvas which manages dragging of the UIElements it contains.  
    /// </summary>
    /// <remarks>
    /// Documentation: http://www.codeproject.com/KB/WPF/DraggingElementsInCanvas.aspx
    /// </remarks>
    public class DragCanvas : Canvas
    {
        #region Data

        // Stores a reference to the UIElement currently being dragged by the user.
        private UIElement _elementBeingDragged;

        // Keeps track of where the mouse cursor was when a drag operation began.		
        private Point _origCursorLocation;

        // The offsets from the DragCanvas' edges when the drag operation began.
        private double _origHorizOffset, _origVertOffset;

        // Keeps track of which horizontal and vertical offset should be modified for the drag element.
        private bool _modifyLeftOffset, _modifyTopOffset;

        // True if a drag operation is underway, else false.
        private bool _isDragInProgress;

        #endregion // Data

        #region Static Constructor

        static DragCanvas()
        {
            AllowDraggingProperty = DependencyProperty.Register(
                "AllowDragging",
                typeof(bool),
                typeof(DragCanvas),
                new PropertyMetadata(true));

            AllowDragOutOfViewProperty = DependencyProperty.Register(
                "AllowDragOutOfView",
                typeof(bool),
                typeof(DragCanvas),
                new UIPropertyMetadata(false));

            CanBeDraggedProperty = DependencyProperty.RegisterAttached(
                "CanBeDragged",
                typeof(bool),
                typeof(DragCanvas),
                new UIPropertyMetadata(true));
        }

        #endregion // Static Constructor

        #region Constructor

        #endregion // Constructor

        #region Attached Properties

        #region CanBeDragged

        /// <summary>
        /// Identifies the RegexValidator's ErrorMessage attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty CanBeDraggedProperty;

        /// <summary>
        /// Returns true if the specified UIElement is allowed to be dragged, else false.
        /// </summary>
        /// <param name="uiElement">The UIElement to check for dragging permission.</param>
        public static bool GetCanBeDragged(UIElement uiElement)
        {
            if (uiElement == null)
                return false;

            return (bool)uiElement.GetValue(CanBeDraggedProperty);
        }

        /// <summary>
        /// Sets the CanBeDragged attached property for the specified UIElement.
        /// </summary>
        /// <param name="uiElement">The UIElement to set the property for.</param>
        /// <param name="value">Pass true if the element can be dragged, else false.</param>
        public static void SetCanBeDragged(UIElement uiElement, bool value)
        {
            if (uiElement != null)
                uiElement.SetValue(CanBeDraggedProperty, value);
        }

        #endregion // CanBeDragged

        #endregion // Attached Properties

        #region Interface

        #region AllowDragging

        /// <summary>
        /// Identifies the DragCanvas's AllowDragging dependency property.
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty AllowDraggingProperty;

        /// <summary>
        /// Gets/sets whether elements in the DragCanvas should be draggable by the user.
        /// The default value is true.  This is a dependency property.
        /// </summary>
        public bool AllowDragging
        {
            get { return (bool)GetValue(AllowDraggingProperty); }
            set { SetValue(AllowDraggingProperty, value); }
        }

        #endregion // AllowDragging

        #region AllowDragOutOfView

        /// <summary>
        /// Identifies the DragCanvas's AllowDragOutOfView dependency property.
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty AllowDragOutOfViewProperty;

        /// <summary>
        /// Gets/sets whether the user should be able to drag elements in the DragCanvas out of
        /// the viewable area.  The default value is false.  This is a dependency property.
        /// </summary>
        public bool AllowDragOutOfView
        {
            get { return (bool)GetValue(AllowDragOutOfViewProperty); }
            set { SetValue(AllowDragOutOfViewProperty, value); }
        }

        #endregion // AllowDragOutOfView

        #region BringToFront / SendToBack

        /// <summary>
        /// Assigns the element a z-index which will ensure that 
        /// it is in front of every other element in the Canvas.
        /// The z-index of every element whose z-index is between 
        /// the element's old and new z-index will have its z-index 
        /// decremented by one.
        /// </summary>
        /// <param name="element">
        /// The element to be sent to the front of the z-order.
        /// </param>
        public void BringToFront(UIElement element)
        {
            UpdateZOrder(element, true);
        }

        /// <summary>
        /// Assigns the element a z-index which will ensure that 
        /// it is behind every other element in the Canvas.
        /// The z-index of every element whose z-index is between 
        /// the element's old and new z-index will have its z-index 
        /// incremented by one.
        /// </summary>
        /// <param name="element">
        /// The element to be sent to the back of the z-order.
        /// </param>
        public void SendToBack(UIElement element)
        {
            UpdateZOrder(element, false);
        }

        #endregion // BringToFront / SendToBack

        #region ElementBeingDragged

        /// <summary>
        /// Returns the UIElement currently being dragged, or null.
        /// </summary>
        /// <remarks>
        /// Note to inheritors: This property exposes a protected 
        /// setter which should be used to modify the drag element.
        /// </remarks>
        public UIElement ElementBeingDragged
        {
            get
            {
                if (!AllowDragging)
                    return null;

                return _elementBeingDragged;
            }
            protected set
            {
                if (_elementBeingDragged != null)
                    _elementBeingDragged.ReleaseMouseCapture();

                if (!AllowDragging)
                    _elementBeingDragged = null;
                else
                {
                    if (GetCanBeDragged(value))
                    {
                        _elementBeingDragged = value;
                        _elementBeingDragged.CaptureMouse();
                    }
                    else
                        _elementBeingDragged = null;
                }
            }
        }

        #endregion // ElementBeingDragged

        #region FindCanvasChild

        /// <summary>
        /// Walks up the visual tree starting with the specified DependencyObject, 
        /// looking for a UIElement which is a child of the Canvas.  If a suitable 
        /// element is not found, null is returned.  If the 'depObj' object is a 
        /// UIElement in the Canvas's Children collection, it will be returned.
        /// </summary>
        /// <param name="depObj">
        /// A DependencyObject from which the search begins.
        /// </param>
        public UIElement FindCanvasChild(DependencyObject depObj)
        {
            while (depObj != null)
            {
                // If the current object is a UIElement which is a child of the
                // Canvas, exit the loop and return it.
                UIElement elem = depObj as UIElement;
                if (elem != null && Children.Contains(elem))
                    break;

                // VisualTreeHelper works with objects of type Visual or Visual3D.
                // If the current object is not derived from Visual or Visual3D,
                // then use the LogicalTreeHelper to find the parent element.
                if (depObj is Visual || depObj is Visual3D)
                    depObj = VisualTreeHelper.GetParent(depObj);
                else
                    depObj = LogicalTreeHelper.GetParent(depObj);
            }
            return depObj as UIElement;
        }

        #endregion // FindCanvasChild

        #endregion // Interface

        #region Overrides

        #region OnPreviewMouseLeftButtonDown

        /// <summary>
        /// Overrides base implementation to support element dragging.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            _isDragInProgress = false;

            // Don't continue if there was a double or tripple click
            if(e.ClickCount > 1)
            {
                return;
            }

            // Cache the mouse cursor location.
            _origCursorLocation = e.GetPosition(this);

            // Walk up the visual tree from the element that was clicked, 
            // looking for an element that is a direct child of the Canvas.
            ElementBeingDragged = FindCanvasChild(e.Source as DependencyObject);
            if (ElementBeingDragged == null)
                return;

            // Get the element's offsets from the four sides of the Canvas.
            double left = GetLeft(ElementBeingDragged);
            double right = GetRight(ElementBeingDragged);
            double top = GetTop(ElementBeingDragged);
            double bottom = GetBottom(ElementBeingDragged);

            // Calculate the offset deltas and determine for which sides
            // of the Canvas to adjust the offsets.
            _origHorizOffset = ResolveOffset(left, right, out _modifyLeftOffset);
            _origVertOffset = ResolveOffset(top, bottom, out _modifyTopOffset);

            // Set the Handled flag so that a control being dragged 
            // does not react to the mouse input.
            e.Handled = true;

            _isDragInProgress = true;
        }

        #endregion // OnPreviewMouseLeftButtonDown

        #region OnPreviewMouseLeftButtonUp
        /// <summary>
        ///  Release item being dragged once the mouse is up
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            ElementBeingDragged = null;
        }
        #endregion


        #region OnPreviewMouseMove

        /// <summary>
        /// Overrides base implementation to support element dragging.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            
            // If no element is being dragged, there is nothing to do.
            if (ElementBeingDragged == null || !_isDragInProgress)
                return;

            // Get the position of the mouse cursor, relative to the DragCanvas.
            Point cursorLocation = e.GetPosition(this);

            // These values will store the new offsets of the drag element.
            double newHorizontalOffset, newVerticalOffset;

            #region Calculate Offsets

            // Determine the horizontal offset.
            if (_modifyLeftOffset)
                newHorizontalOffset = _origHorizOffset + (cursorLocation.X - _origCursorLocation.X);
            else
                newHorizontalOffset = _origHorizOffset - (cursorLocation.X - _origCursorLocation.X);

            // Determine the vertical offset.
            if (_modifyTopOffset)
                newVerticalOffset = _origVertOffset + (cursorLocation.Y - _origCursorLocation.Y);
            else
                newVerticalOffset = _origVertOffset - (cursorLocation.Y - _origCursorLocation.Y);

            #endregion // Calculate Offsets

            if (!AllowDragOutOfView)
            {
                #region Verify Drag Element Location

                // Get the bounding rect of the drag element.
                Rect elemRect = CalculateDragElementRect(newHorizontalOffset, newVerticalOffset);

                //
                // If the element is being dragged out of the viewable area, 
                // determine the ideal rect location, so that the element is 
                // within the edge(s) of the canvas.
                //
                bool leftAlign = elemRect.Left < 0;
                bool rightAlign = elemRect.Right > ActualWidth;

                if (leftAlign)
                    newHorizontalOffset = _modifyLeftOffset ? 0 : ActualWidth - elemRect.Width;
                else if (rightAlign)
                    newHorizontalOffset = _modifyLeftOffset ? ActualWidth - elemRect.Width : 0;

                bool topAlign = elemRect.Top < 0;
                bool bottomAlign = elemRect.Bottom > ActualHeight;

                if (topAlign)
                    newVerticalOffset = _modifyTopOffset ? 0 : ActualHeight - elemRect.Height;
                else if (bottomAlign)
                    newVerticalOffset = _modifyTopOffset ? ActualHeight - elemRect.Height : 0;

                #endregion // Verify Drag Element Location
            }

            #region Move Drag Element

            if (_modifyLeftOffset)
                SetLeft(ElementBeingDragged, newHorizontalOffset);
            else
                SetRight(ElementBeingDragged, newHorizontalOffset);

            if (_modifyTopOffset)
                SetTop(ElementBeingDragged, newVerticalOffset);
            else
                SetBottom(ElementBeingDragged, newVerticalOffset);

            #endregion // Move Drag Element
        }

        #endregion // OnPreviewMouseMove

        #region OnHostPreviewMouseUp

        /// <summary>
        /// Overrides base implementation to support element dragging.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            // Reset the field whether the left or right mouse button was 
            // released, in case a context menu was opened on the drag element.
            ElementBeingDragged = null;
        }
        #endregion // OnHostPreviewMouseUp

        #endregion // Host Event Handlers

        #region Private Helpers

        #region CalculateDragElementRect

        /// <summary>
        /// Returns a Rect which describes the bounds of the element being dragged.
        /// </summary>
        private Rect CalculateDragElementRect(double newHorizOffset, double newVertOffset)
        {
            if (ElementBeingDragged == null)
                throw new InvalidOperationException("ElementBeingDragged is null.");

            Size elemSize = ElementBeingDragged.RenderSize;

            double x, y;

            if (_modifyLeftOffset)
                x = newHorizOffset;
            else
                x = ActualWidth - newHorizOffset - elemSize.Width;

            if (_modifyTopOffset)
                y = newVertOffset;
            else
                y = ActualHeight - newVertOffset - elemSize.Height;

            Point elemLoc = new Point(x, y);

            return new Rect(elemLoc, elemSize);
        }

        #endregion // CalculateDragElementRect

        #region ResolveOffset

        /// <summary>
        /// Determines one component of a UIElement's location 
        /// within a Canvas (either the horizontal or vertical offset).
        /// </summary>
        /// <param name="side1">
        /// The value of an offset relative to a default side of the 
        /// Canvas (i.e. top or left).
        /// </param>
        /// <param name="side2">
        /// The value of the offset relative to the other side of the 
        /// Canvas (i.e. bottom or right).
        /// </param>
        /// <param name="useSide1">
        /// Will be set to true if the returned value should be used 
        /// for the offset from the side represented by the 'side1' 
        /// parameter.  Otherwise, it will be set to false.
        /// </param>
        private static double ResolveOffset(double side1, double side2, out bool useSide1)
        {
            // If the Canvas.Left and Canvas.Right attached properties 
            // are specified for an element, the 'Left' value is honored.
            // The 'Top' value is honored if both Canvas.Top and 
            // Canvas.Bottom are set on the same element.  If one 
            // of those attached properties is not set on an element, 
            // the default value is Double.NaN.
            useSide1 = true;
            double result;
            if (Double.IsNaN(side1))
            {
                if (Double.IsNaN(side2))
                {
                    // Both sides have no value, so set the
                    // first side to a value of zero.
                    result = 0;
                }
                else
                {
                    result = side2;
                    useSide1 = false;
                }
            }
            else
            {
                result = side1;
            }
            return result;
        }

        #endregion // ResolveOffset

        #region UpdateZOrder

        /// <summary>
        /// Helper method used by the BringToFront and SendToBack methods.
        /// </summary>
        /// <param name="element">
        /// The element to bring to the front or send to the back.
        /// </param>
        /// <param name="bringToFront">
        /// Pass true if calling from BringToFront, else false.
        /// </param>
        private void UpdateZOrder(UIElement element, bool bringToFront)
        {
            #region Safety Check

            if (element == null)
                throw new ArgumentNullException("element");

            if (!Children.Contains(element))
                throw new ArgumentException(@"Must be a child element of the Canvas.", "element");

            #endregion // Safety Check

            #region Calculate Z-Indici And Offset

            // Determine the Z-Index for the target UIElement.
            int elementNewZIndex = -1;
            if (bringToFront)
            {
                elementNewZIndex += Children.Cast<UIElement>().Count(elem => elem.Visibility != Visibility.Collapsed);
            }
            else
            {
                elementNewZIndex = 0;
            }

            // Determine if the other UIElements' Z-Index 
            // should be raised or lowered by one. 
            int offset = (elementNewZIndex == 0) ? +1 : -1;

            int elementCurrentZIndex = GetZIndex(element);

            #endregion // Calculate Z-Indici And Offset

            #region Update Z-Indici

            // Update the Z-Index of every UIElement in the Canvas.
            foreach (UIElement childElement in Children)
            {
// ReSharper disable PossibleUnintendedReferenceComparison
                if (childElement == element)
// ReSharper restore PossibleUnintendedReferenceComparison
                    SetZIndex(element, elementNewZIndex);
                else
                {
                    int zIndex = GetZIndex(childElement);

                    // Only modify the z-index of an element if it is  
                    // in between the target element's old and new z-index.
                    if (bringToFront && elementCurrentZIndex < zIndex ||
                        !bringToFront && zIndex < elementCurrentZIndex)
                    {
                        SetZIndex(childElement, zIndex + offset);
                    }
                }
            }

            #endregion // Update Z-Indici
        }

        #endregion // UpdateZOrder

        #endregion // Private Helpers
    }
}
