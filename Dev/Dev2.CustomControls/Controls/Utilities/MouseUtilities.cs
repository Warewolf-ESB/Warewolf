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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace WPF.JoshSmith.Controls.Utilities
{
    /// <summary>
    ///     Provides access to the mouse location by calling unmanaged code.
    /// </summary>
    /// <remarks>
    ///     This class was written by Dan Crevier (Microsoft).
    ///     http://blogs.msdn.com/llobo/archive/2006/09/06/Scrolling-Scrollviewer-on-Mouse-Drag-at-the-boundaries.aspx
    /// </remarks>
    public class MouseUtilities
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        // ReSharper disable UnusedMember.Local
        private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

        // ReSharper restore UnusedMember.Local

        /// <summary>
        ///     Returns the mouse cursor location.  This method is necessary during
        ///     a drag-drop operation because the WPF mechanisms for retrieving the
        ///     cursor coordinates are unreliable.
        /// </summary>
        /// <param name="relativeTo">The Visual to which the mouse coordinates will be relative.</param>
        public static Point GetMousePosition(Visual relativeTo)
        {
            var mouse = new Win32Point();
            GetCursorPos(ref mouse);

            // BUG FIX: 2/25/2007
            // Using PointFromScreen instead of Dan Crevier's code (commented out below)
            // is a bug fix created by William J. Roberts.  Read his comments about the fix
            // here: http://www.codeproject.com/useritems/ListViewDragDropManager.asp?msg=1911611#xx1911611xx
            return relativeTo.PointFromScreen(new Point(mouse.X, mouse.Y));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public readonly Int32 X;
            public readonly Int32 Y;
        };
    }
}