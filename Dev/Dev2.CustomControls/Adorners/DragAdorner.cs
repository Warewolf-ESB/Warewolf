/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - January 2007

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF.JoshSmith.Adorners
{
    /// <summary>
    ///     A lightweight adorner which renders a visual that can follow the mouse cursor,
    ///     such as during a drag-and-drop operation.
    /// </summary>
    /// <remarks>
    ///     Used In: http://www.codeproject.com/KB/WPF/ListViewDragDropManager.aspx
    /// </remarks>
    public class DragAdorner : SingleChildAdornerBase
    {
        #region Constructor

        /// <summary>
        ///     Initializes a new instance of DragAdorner.
        /// </summary>
        /// <param name="adornedElement">The element being adorned.</param>
        /// <param name="size">The size of the adorner.</param>
        /// <param name="brush">A brush with which to paint the adorner.</param>
        public DragAdorner(UIElement adornedElement, Size size, Brush brush)
            : base(adornedElement)
        {
            var rect = new Rectangle {Fill = brush, Width = size.Width, Height = size.Height, IsHitTestVisible = false};
            child = rect;
        }

        #endregion // Constructor
    }
}