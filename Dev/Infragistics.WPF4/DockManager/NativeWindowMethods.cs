using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Runtime.CompilerServices;

namespace Infragistics.Windows.DockManager
{
	[System.Security.SuppressUnmanagedCodeSecurityAttribute()]
	[System.Runtime.InteropServices.ComVisible(false)]
	internal static class NativeWindowMethods
	{
		#region Constants

		#region HitTest Constants (HT_)

		internal const int HT_CAPTION = 2;

		#endregion //HitTest Constants (HT_)

        // AS 10/13/08 TFS6032
        #region ShowWindow Constants (SW_)

        internal const int SW_HIDE = 0;

        #endregion ShowWindow Constants (SW_)

		#region SetWindowPos Constants (SWP_)

		internal const int SWP_NOACTIVATE = 0x0010;
		internal const int SWP_SHOWWINDOW = 0x0040;
		internal const int SWP_NOSIZE = 0x0001;
		internal const int SWP_NOMOVE = 0x0002;
		internal const int SWP_NOSENDCHANGING = 0x0400;
		internal const int SWP_FRAMECHANGED = 0x20;
		internal const int SWP_NOOWNERZORDER = 0x200;
		internal const int SWP_NOZORDER = 0x4;
		internal const int SWP_NOREDRAW = 0x0008;
		internal const int SWP_HIDEWINDOW = 0x0080;
		internal const int SWP_NOCOPYBITS = 0x0100;
		internal const int SWP_DRAWFRAME = SWP_FRAMECHANGED;
		internal const int SWP_NOREPOSITION = SWP_NOOWNERZORDER;
		internal const int SWP_DEFERERASE = 0x2000;
		internal const int SWP_ASYNCWINDOWPOS = 0x4000;

		#endregion SetWindowPos Constants (SWP_)

		#region Window Messages (WM_)

		internal const int WM_NCHITTEST = 0x84;

		#endregion //Window Messages (WM_)

		#endregion // Constants

		#region Structs

		// AS 9/11/09 TFS21329
		#region POINT
		[StructLayout(LayoutKind.Sequential)]
		internal struct POINT : IEquatable<POINT>
		{
			internal int X;
			internal int Y;

			internal POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}

			internal Point Point { get { return new Point(this.X, this.Y); } }

			#region IEquatable<POINT> Members

			public bool Equals(POINT other)
			{
				return X == other.X && Y == other.Y;
			}

			#endregion
		}
		#endregion //POINT

		#endregion //Structs

		#region APIs

		// AS 9/11/09 TFS21329
		#region GetCursorPos

		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
		private static extern bool GetCursorPos([In, Out] POINT pt);

		#endregion //GetCursorPos 

		// AS 6/28/10 TFS32978
		#region IsWindowEnabled

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWindowEnabled(IntPtr hwnd); 

		#endregion //IsWindowEnabled

		#region SendMessage

		internal static IntPtr SendMessageApi(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			return SendMessage(hWnd, msg, wparam, lparam);
		}

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam);

		#endregion SendMessage

		#region SetWindowPos

		// MD 10/31/06 - 64-Bit Support
		//internal static bool SetWindowPosApi( int hWnd, int hWndInsertAfter,
		//    int x, int y, int cx, int cy, int flags)
		internal static bool SetWindowPosApi(IntPtr hWnd, IntPtr hWndInsertAfter,
			int x, int y, int cx, int cy, int flags)
		{
			return SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, flags);
		}

		// MD 10/31/06 - 64-Bit Support
		//[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		//private static extern bool SetWindowPos( int hWnd, int hWndInsertAfter,
		//    int x, int y, int cx, int cy, int flags);
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
			int x, int y, int cx, int cy, int flags);

		#endregion SetWindowPos

        // AS 10/13/08 TFS6032
        #region ShowWindow
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); 
        #endregion //ShowWindow

		#endregion // APIs

		#region Methods

		// AS 9/11/09 TFS21329
		#region GetCurrentMousePosition

		private static bool _apiGetCursorPosFailed;

		public static Point GetCurrentMousePosition()
		{
			POINT pt = new POINT();

			if (_apiGetCursorPosFailed)
			{
				try
				{
					GetCursorPos(pt);
				}
				catch
				{
					_apiGetCursorPosFailed = true;
					pt = GetWinFormCursorPos();
				}
			}
			else
				pt = GetWinFormCursorPos();

			return new Point(pt.X, pt.Y);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static POINT GetWinFormCursorPos()
		{
			System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
			return new POINT(pt.X, pt.Y);
		} 
		#endregion //GetCurrentMousePosition

		#region IntPtrToInt32
		internal static int IntPtrToInt32(IntPtr intPtr)
		{
			return unchecked((int)intPtr.ToInt64());
		}
		#endregion // IntPtrToInt32

		#region MoveWindow
		internal static void MoveWindow(Window window, Point location)
		{
			System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(window);

			if (helper.Handle == IntPtr.Zero)
			{
				window.Top = location.Y;
				window.Left = location.X;
			}
			else
			{
				System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
				Point pt = source.CompositionTarget.TransformToDevice.Transform(location);
				NativeWindowMethods.SetWindowPosApi(helper.Handle, IntPtr.Zero, (int)pt.X, (int)pt.Y, 0, 0,
					NativeWindowMethods.SWP_NOACTIVATE |
					NativeWindowMethods.SWP_NOOWNERZORDER |
					NativeWindowMethods.SWP_NOSIZE |
					NativeWindowMethods.SWP_NOZORDER);
			}
		} 
		#endregion //MoveWindow

		#endregion // Methods
	}
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved