
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

namespace Microsoft.Win32
{
	internal static partial class NativeMethods
	{
		[DllImport("user32", ExactSpelling = true, SetLastError = true)]
		internal static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref NativeMethods.RECT rect, [MarshalAs(UnmanagedType.U4)] int cPoints);

		[DllImport("user32", ExactSpelling = true, SetLastError = true)]
		internal static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref System.Drawing.Point pt, [MarshalAs(UnmanagedType.U4)] int cPoints);

		public static System.Drawing.Point MapPointToClient(this System.Windows.Forms.IWin32Window ctrl, System.Drawing.Point pt)
		{
			return MapPoint(null, pt, ctrl);
		}

		public static System.Drawing.Point MapPoint(this System.Windows.Forms.IWin32Window ctrl, System.Drawing.Point pt, System.Windows.Forms.IWin32Window newWin = null)
		{
			MapWindowPoints(ctrl == null ? IntPtr.Zero : ctrl.Handle, newWin == null ? IntPtr.Zero : newWin.Handle, ref pt, 1);
			return pt;
		}

		public static System.Drawing.Rectangle MapRectangle(this System.Windows.Forms.IWin32Window ctrl, System.Drawing.Rectangle rectangle, System.Windows.Forms.IWin32Window newWin = null)
		{
			NativeMethods.RECT ir = rectangle;
			MapWindowPoints(ctrl == null ? IntPtr.Zero : ctrl.Handle, newWin == null ? IntPtr.Zero : newWin.Handle, ref ir, 2);
			return ir;
		}

		public static System.Drawing.Point[] MapPoints(this System.Windows.Forms.IWin32Window ctrl, System.Drawing.Point[] points, System.Windows.Forms.IWin32Window newWin = null)
		{
			System.Drawing.Point[] pts = new System.Drawing.Point[points.Length];
			points.CopyTo(pts, 0);
			for (int i = 0; i < pts.Length; i++)
				MapWindowPoints(ctrl == null ? IntPtr.Zero : ctrl.Handle, newWin == null ? IntPtr.Zero : newWin.Handle, ref pts[i], 1);
			return pts;
		}
	}
}
 
