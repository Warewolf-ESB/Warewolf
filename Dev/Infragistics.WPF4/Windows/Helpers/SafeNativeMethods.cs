using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Markup;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics;
using System.Security;
using System.Windows.Forms;
using Infragistics.Windows.Controls;
using System.Windows.Interop;

namespace Infragistics.Windows.Helpers
{
	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	// Added lots of constants, structs, apis, methods, etc. to support the ToolWindow infrastructure.
	//





	[System.Security.SuppressUnmanagedCodeSecurityAttribute( )]
	[System.Runtime.InteropServices.ComVisible( false )]
	internal static class NativeWindowMethods
	{
		#region Member Variables

		private static bool _securityException;
		private static bool hasSecurityExceptionBeenThrown = false;

		#endregion //Member Variables

		#region Structs

		#region MINMAXINFO structure
		[StructLayout(LayoutKind.Sequential)]
		internal struct MINMAXINFO
		{
			public POINT ptReserved;
			public POINT ptMaxSize;
			public POINT ptMaxPosition;
			public POINT ptMinTrackSize;
			public POINT ptMaxTrackSize;
		}
		#endregion //MINMAXINFO structure

		#region MONITORINFO
		[StructLayout(LayoutKind.Sequential)]
		public struct MONITORINFO
		{
			public uint cbSize;
			public RECT rcMonitor;
			public RECT rcWork;
			public uint dwFlags;
		}
		#endregion //MONITORINFO

        // JJD 11/05/07 - BR28049 - Added
        #region MONITIORINFOEX

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        internal class MONITIORINFOEX
        {
            internal int _size;
            internal RECT _rcMonitor;
            internal RECT _rcWork;
            internal int _flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x20)]
            internal char[] _device;

            internal MONITIORINFOEX()
            {
                this._size = Marshal.SizeOf(typeof(MONITIORINFOEX));
                this._rcMonitor = new RECT();
                this._rcWork = new RECT();
                this._device = new char[0x20];
            }

        }

        #endregion //MONITIORINFOEX

		#region NCCALCSIZE_PARAMS
		[StructLayout(LayoutKind.Sequential)]
		internal struct NCCALCSIZE_PARAMS
		{
			internal RECT rectProposed;
			internal RECT rectBeforeMove;
			internal RECT rectClientBeforeMove;
			// JJD 8/14/06
			// This is a pointer to a WINDOWPOS structure which need to be marshalled separately
			// e.g.
			// NativeWindowMethods.NCCALCSIZE_PARAMS ncParams = (NativeWindowMethods.NCCALCSIZE_PARAMS)m.GetLParam(typeof(NativeWindowMethods.NCCALCSIZE_PARAMS));
			// NativeWindowMethods.WINDOWPOS wpos = (NativeWindowMethods.WINDOWPOS)Marshal.PtrToStructure(ncParams.ptrWindowPos, typeof(NativeWindowMethods.WINDOWPOS));
			internal IntPtr ptrWindowPos;
			//internal WINDOWPOS lpPos;
		}
		#endregion //NCCALCSIZE_PARAMS

        // AS 7/24/07 MousePanningDecorator
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

        // JJD 11/05/07 - BR28049 - Added
        #region RECT structure

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            internal int left;
            internal int top;
            internal int right;
            internal int bottom;


            internal RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


			public int Height
			{
				get { return this.bottom - this.top; }
			}

			public int Width
			{
				get { return this.right - this.left; }
			}

			public static RECT FromLTRB(POINT leftTop, POINT rightBottom)
			{
				return new RECT(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
			}

			// AS 8/4/11 TFS83465/TFS83469
			public Rect ToRect()
			{
				return new Rect(left, top, Math.Abs(right - left), Math.Abs(bottom - top));
			}
        }

        #endregion RECT structure

		#region SIZE
		[StructLayout(LayoutKind.Sequential)]
		internal struct SIZE
		{
			public int width;
			public int height;

			public SIZE(int width, int height)
			{
				this.width = width;
				this.height = height;
			}
		}
		#endregion //SIZE

		#region STYLESTRUCT
		[StructLayout(LayoutKind.Sequential)]
		internal struct STYLESTRUCT
		{
			public int styleOld;
			public int styleNew;
		}
		#endregion // STYLESTRUCT

		#region WINDOWINFO
		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWINFO
		{
			public int cbSize;
			public RECT rcWindow;
			public RECT rcClient;
			public uint dwStyle;
			public uint dwExStyle;
			public uint dwWindowStatus;
			public uint cxWindowBorders;
			public uint cyWindowBorders;
			public ushort atomWindowType;
			public ushort wCreatorVersion;
		}
		#endregion //WINDOWINFO

		// AS 11/7/06 BR17495
		#region WINDOWPLACEMENT structure

		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWPLACEMENT
		{
			public int length;
			public int flags;
			public int showCmd;
			public POINT ptMinPosition;
			public POINT ptMaxPosition;
			public RECT rcNormalPosition;
		}

		#endregion //WINDOWPLACEMENT structure

		#region WINDOWPOS
		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWPOS
		{
			internal IntPtr hwnd;
			internal IntPtr hWndInsertAfter;
			internal int x;
			internal int y;
			internal int cx;
			internal int cy;
			internal int flags;
		}
		#endregion //WINDOWPOS

		#endregion //Structs

        #region Constants

		#region CBT Hook Codes (HCBT_)

		internal const int HCBT_ACTIVATE = 5;

		#endregion //CBT Hook Codes (HCBT_)

		#region GetWindow flags (GW_)

		internal const uint GW_HWNDFIRST = 0;
		internal const uint GW_HWNDLAST = 1;
		internal const uint GW_HWNDNEXT = 2;
		internal const uint GW_HWNDPREV = 3;
		internal const uint GW_OWNER = 4;
		internal const uint GW_CHILD = 5;
		internal const uint GW_ENABLEDPOPUP = 6; 

		#endregion //GetWindow flags (GW_)

		#region GetWindowLong flags (GWL_)

        internal const int GWL_HWNDPARENT = (-8);
        internal const int GWL_STYLE = (-16);
		internal const int GWL_EXSTYLE = (-20);

		#endregion // GetWindowLong flags (GWL_)

		#region Menu Flags (MF_)

		internal const int MF_BYCOMMAND = 0x0;
		internal const int MF_BYPOSITION = 0x400;

		internal const int MF_ENABLED = 0x0;
		internal const int MF_GRAYED = 0x1;

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		internal const int MF_DEFAULT = 0x00001000;
		internal const int MF_SYSMENU = 0x00002000;

		#endregion //Menu Flags (MF_)

		#region MonitorFlags (MONITOR_)

		internal const int MONITOR_DEFAULTTONULL = 0;
		internal const int MONITOR_DEFAULTTOPRIMARY = 1;
		internal const int MONITOR_DEFAULTTONEAREST = 2;

		#endregion //MonitorFlags (MONITOR_)

		#region System Commands (SC_)

		internal const int SC_MINIMIZE = 0xF020;
		internal const int SC_MAXIMIZE = 0xF030;
		internal const int SC_NEXTWINDOW = 0xF040;
		internal const int SC_CLOSE = 0xF060;
		internal const int SC_RESTORE = 0xF120;
		internal const int SC_KEYMENU = 0xF100;
		internal const int SC_MOUSEMENU = 0xF090;
		internal const int SC_CONTEXTHELP = 0xF180;
		internal const int SC_MOVE = 0xF010;

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// This is an "undocumented" flag. Essentially it is SC_MOVE but
		// the os will not move the cursor over the title bar.
		//
		internal const int SC_DRAGMOVE = 0xF012;

		#endregion //System Commands (SC_)

		#region Size flags (SIZE_)

		internal const int SIZE_RESTORED = 0;
		internal const int SIZE_MINIMIZED = 1;
		internal const int SIZE_MAXIMIZED = 2;
		internal const int SIZE_MAXSHOW = 3;
		internal const int SIZE_MAXHIDE = 4;

		#endregion // Size flags (SIZE_)

		#region SystemMetrics flags (SM_)

		// JJD 11/05/07 - BR28049 - Added
		private const int SM_CXVIRTUALSCREEN = 78;
		private const int SM_CMONITORS = 80;
		// AS 3/19/08
		// Removed this one since I have all of them defined and grouped together.
		//
		//private const int MONITOR_DEFAULTTONEAREST = 2;

		#endregion //SystemMetrics flags (SM_)

		#region ShowWindow Constants (SW_)

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//internal const int SW_SHOWNORMAL = 1;
		//internal const int SW_SHOWMINIMIZED = 2;
		//internal const int SW_SHOWMAXIMIZED = 3;
		internal const int SW_FORCEMINIMIZE = 11; //Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread.
		internal const int SW_HIDE = 0; //Hides the window and activates another window.
		internal const int SW_MAXIMIZE = 3; // Maximizes the specified window.
		internal const int SW_MINIMIZE = 6; // Minimizes the specified window and activates the next top-level window in the Z order.
		internal const int SW_RESTORE = 9; // Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
		internal const int SW_SHOW = 5; // Activates the window and displays it in its current size and position. 
		internal const int SW_SHOWDEFAULT = 10; // Sets the show state based on the internal const int SW_ = value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application. 
		internal const int SW_SHOWMAXIMIZED = 3; // Activates the window and displays it as a maximized window.
		internal const int SW_SHOWMINIMIZED = 2; // Activates the window and displays it as a minimized window.
		internal const int SW_SHOWMINNOACTIVE = 7; // Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
		internal const int SW_SHOWNA = 8; // Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is not activated.
		internal const int SW_SHOWNOACTIVATE = 4; // Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated.
		internal const int SW_SHOWNORMAL = 1; // Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
 

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

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// undocumented flag indicating that the window state is changing
		internal const int SWP_STATECHANGED = 0x8000;

		#endregion SetWindowPos Constants (SWP_)

		#region Window Hook Constants (WH_)

		internal const int WH_CBT = 5;

		#endregion //Window Hook Constants (WH_)

		#region Window Messages (WM_)

		internal const int WM_NCHITTEST = 0x84;
		internal const int WM_NCCALCSIZE = 0x0083;
		internal const int WM_NCUAHDRAWCAPTION = 0x00AE;
		internal const int WM_NCUAHDRAWFRAME = 0x00AF;
		internal const int WM_NCPAINT = 0x0085;
		internal const int WM_NCACTIVATE = 0x0086;
		internal const int WM_STYLECHANGING = 0x007C;
		internal const int WM_GETMINMAXINFO = 0x24;
		internal const int WM_MOVING = 0x216;
		internal const int WM_SIZE = 0x5;

		internal const int WM_CONTEXTMENU = 0x007B;
		internal const int WM_SYSCOMMAND = 0x0112;
		internal const int WM_NCLBUTTONDBLCLK = 0x00A3;		

		// MBS 10/30/06 - Glass/Vista
		internal const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

		internal const int WM_WINDOWPOSCHANGING = 0x0046;
		internal const int WM_WINDOWPOSCHANGED = 0x0047;

		internal const int WM_NCLBUTTONDOWN = 0x00A1;

        // AS 10/13/08 TFS6107/BR34010
        internal const int WM_SHOWWINDOW = 0x0018;

        // AS 3/30/09 TFS16355 - WinForms Interop
        internal const int WM_MOUSEACTIVATE = 0x0021;

		// AS 8/4/11 TFS83465/TFS83469
		internal const int WM_DISPLAYCHANGE = 0x7E;

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		internal const int WM_LBUTTONUP = 0x0202;
		internal const int WM_ENTERSIZEMOVE = 0x0231;
		internal const int WM_EXITSIZEMOVE = 0x0232;

		// AS 3/12/12
		internal const int WM_TABLET_DEFBASE = 0x02C0;
		internal const int WM_TABLET_QUERYSYSTEMGESTURESTATUS = (WM_TABLET_DEFBASE + 12);

		#endregion //Window Messages (WM_)

		#region Window Styles (WS_)

		internal const int WS_MINIMIZEBOX = 0x00020000;
		internal const int WS_MAXIMIZEBOX = 0x00010000;
		internal const int WS_SYSMENU = 0x00080000;

		internal const int WS_OVERLAPPED       = 0x00000000;
		internal const int WS_POPUP            = int.MinValue; // 0x80000000;
		internal const int WS_CHILD            = 0x40000000;
		internal const int WS_MINIMIZE         = 0x20000000;
		internal const int WS_VISIBLE          = 0x10000000;
		internal const int WS_DISABLED         = 0x08000000;
		internal const int WS_CLIPSIBLINGS     = 0x04000000;
		internal const int WS_CLIPCHILDREN     = 0x02000000;
		internal const int WS_MAXIMIZE         = 0x01000000;
		internal const int WS_CAPTION          = 0x00C00000;     
		internal const int WS_BORDER           = 0x00800000;
		internal const int WS_DLGFRAME         = 0x00400000;
		internal const int WS_VSCROLL          = 0x00200000;
		internal const int WS_HSCROLL          = 0x00100000;
		internal const int WS_THICKFRAME       = 0x00040000;
		internal const int WS_GROUP            = 0x00020000;
		internal const int WS_TABSTOP          = 0x00010000;

		internal const int WS_TILED            = WS_OVERLAPPED;
		internal const int WS_ICONIC           = WS_MINIMIZE;
		internal const int WS_SIZEBOX          = WS_THICKFRAME;
		internal const int WS_TILEDWINDOW      = WS_OVERLAPPEDWINDOW;

		


		internal const int WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED     |
									 WS_CAPTION        |
									 WS_SYSMENU        |
									 WS_THICKFRAME     |
									 WS_MINIMIZEBOX    |
									 WS_MAXIMIZEBOX);

		internal const int WS_POPUPWINDOW = (WS_POPUP  |
									 WS_BORDER         |
									 WS_SYSMENU);

		internal const int WS_CHILDWINDOW = (WS_CHILD);

		#endregion // Window Styles (WS_)

		#region Extended Window Styles (WS_EX_)

		internal const int WS_EX_DLGMODALFRAME     = 0x00000001;
		internal const int WS_EX_NOPARENTNOTIFY    = 0x00000004;
		internal const int WS_EX_TOPMOST           = 0x00000008;
		internal const int WS_EX_ACCEPTFILES       = 0x00000010;
		internal const int WS_EX_TRANSPARENT       = 0x00000020;
		//#if(WINVER >= = 0x0400)
		internal const int WS_EX_MDICHILD          = 0x00000040;
		internal const int WS_EX_TOOLWINDOW        = 0x00000080;
		internal const int WS_EX_WINDOWEDGE        = 0x00000100;
		internal const int WS_EX_CLIENTEDGE        = 0x00000200;
		internal const int WS_EX_CONTEXTHELP       = 0x00000400;
		//#endif /* WINVER >= = 0x0400 */
		//#if(WINVER >= = 0x0400)

		internal const int WS_EX_RIGHT             = 0x00001000;
		internal const int WS_EX_LEFT              = 0x00000000;
		internal const int WS_EX_RTLREADING        = 0x00002000;
		internal const int WS_EX_LTRREADING        = 0x00000000;
		internal const int WS_EX_LEFTSCROLLBAR     = 0x00004000;
		internal const int WS_EX_RIGHTSCROLLBAR    = 0x00000000;

		internal const int WS_EX_CONTROLPARENT     = 0x00010000;
		internal const int WS_EX_STATICEDGE        = 0x00020000;
		internal const int WS_EX_APPWINDOW         = 0x00040000;


		internal const int WS_EX_OVERLAPPEDWINDOW  = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
		internal const int WS_EX_PALETTEWINDOW     = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);

		//#endif /* WINVER >= = 0x0400 */

		//#if(_WIN32_WINNT >= = 0x0500)
		internal const int WS_EX_LAYERED           = 0x00080000;
		//#endif /* _WIN32_WINNT >= = 0x0500 */

		//#if(WINVER >= = 0x0500)
		internal const int WS_EX_NOINHERITLAYOUT   = 0x00100000; // Disable inheritence of mirroring by children
		internal const int WS_EX_LAYOUTRTL         = 0x00400000 ;// Right to left mirroring
		//#endif /* WINVER >= = 0x0500 */

		//#if(_WIN32_WINNT >= = 0x0501)
		internal const int WS_EX_COMPOSITED        = 0x02000000;
		//#endif /* _WIN32_WINNT >= = 0x0501 */
		//#if(_WIN32_WINNT >= = 0x0500)
		internal const int WS_EX_NOACTIVATE        = 0x08000000;
		//#endif /* _WIN32_WINNT >= = 0x0500 */

		#endregion //Extended Window Styles (WS_EX_)

		#region Misc

		// MD 10/31/06 - 64-Bit Support
		private const int PtrSizeOn32Bit = 4;
		private const int PtrSizeOn64Bit = 8;

		#endregion // Misc

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region Hwnd constants (HWND_)

        internal static readonly IntPtr HWND_TOP = new IntPtr(0);
        internal static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        internal static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        #endregion //Hwnd constants (HWND_)

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region WindowPlacement Constants (WPF_)

		internal const int WPF_ASYNCWINDOWPLACEMENT = 0x0004; // If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
		internal const int WPF_RESTORETOMAXIMIZED = 0x0002; // The restored window will be maximized, regardless of whether it was maximized before it was minimized. This setting is only valid the next time the window is restored. It does not change the default restoration behavior. This flag is only valid when the SW_SHOWMINIMIZED value is specified for the showCmd member.
		internal const int WPF_SETMINPOSITION = 0x0001; //The coordinates of the minimized window may be specified.  

		#endregion //WindowPlacement Constants (WPF_)

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region WM_SHOWWINDOW status constants (SW_)

		internal const int SW_OTHERUNZOOM = 4; // The window is being uncovered because a maximize window was restored or minimized.
		internal const int SW_OTHERZOOM = 2; // The window is being covered by another window that has been maximized.
		internal const int SW_PARENTCLOSING = 1; // The window's owner window is being minimized.
		internal const int SW_PARENTOPENING = 3; // The window's owner window is being restored. 

		#endregion //WM_SHOWWINDOW status constants (SW_)

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region TrackPopupMenu Flags (TPM_)
		
		internal const uint TPM_CENTERALIGN = 0x0004;	// If this flag is set, the function centers the shortcut menu horizontally relative to the coordinate specified by the x parameter.
		internal const uint TPM_LEFTALIGN = 0x0000;		// If this flag is set, the function positions the shortcut menu so that its left side is aligned with the coordinate specified by the x parameter.
		internal const uint TPM_RIGHTALIGN = 0x0008;	// Positions the shortcut menu so that its right side is aligned with the coordinate specified by the x parameter.
		internal const uint TPM_BOTTOMALIGN = 0x0020;	// If this flag is set, the function positions the shortcut menu so that its bottom side is aligned with the coordinate specified by the y parameter.
		internal const uint TPM_TOPALIGN = 0x0000;		// If this flag is set, the function positions the shortcut menu so that its top side is aligned with the coordinate specified by the y parameter.
		internal const uint TPM_VCENTERALIGN = 0x0010;	// If this flag is set, the function centers the shortcut menu vertically relative to the coordinate specified by the y parameter.
		internal const uint TPM_NONOTIFY = 0x0080;		// If this flag is set, the function does not send notification messages when the user clicks on a menu item.
		internal const uint TPM_RETURNCMD = 0x0100;		// If this flag is set, the function returns the menu item identifier of the user's selection in the return value.

		internal const uint TPM_LEFTBUTTON = 0x0000;	// If this flag is set, the user can select menu items with only the left mouse button.
		internal const uint TPM_RIGHTBUTTON = 0x0002;	// If this flag is set, the user can select menu items with both the left and right mouse buttons.
 

		#endregion //TrackPopupMenu Flags (TPM_)

		// AS 2/22/12 TFS101038
		#region MOUSEEVENTF Flags

		internal const long MOUSEEVENTF_FROMTOUCH = 0xFF515700; 

		#endregion //MOUSEEVENTF Flags

		#endregion //Constants

		#region Enums

		#region HitTestResults
		internal enum HitTestResults
		{
			HTERROR = (-2),
			HTTRANSPARENT = (-1),
			HTNOWHERE = 0,
			HTCLIENT = 1,
			HTCAPTION = 2,
			HTSYSMENU = 3,
			HTGROWBOX = 4,
			HTSIZE = HTGROWBOX,
			HTMENU = 5,
			HTHSCROLL = 6,
			HTVSCROLL = 7,
			HTMINBUTTON = 8,
			HTMAXBUTTON = 9,
			HTLEFT = 10,
			HTRIGHT = 11,
			HTTOP = 12,
			HTTOPLEFT = 13,
			HTTOPRIGHT = 14,
			HTBOTTOM = 15,
			HTBOTTOMLEFT = 16,
			HTBOTTOMRIGHT = 17,
			HTBORDER = 18,
			HTREDUCE = HTMINBUTTON,
			HTZOOM = HTMAXBUTTON,
			HTSIZEFIRST = HTLEFT,
			HTSIZELAST = HTBOTTOMRIGHT,
			HTOBJECT = 19,
			HTCLOSE = 20,
			HTHELP = 21,
		}
		#endregion //HitTestResults

		#endregion //Enums

		#region APIs

		#region AdjustWindowRectEx

		internal static bool AdjustWindowRectExApi(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle)
		{
			return AdjustWindowRectEx(ref lpRect, dwStyle, bMenu, dwExStyle);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern bool AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

		#endregion AdjustWindowRectEx

		#region CallNextHookEx

		// JJD 5/03/02 added hook apis
		[DllImport("user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//public static extern int CallNextHookEx(IntPtr hook, int nCode, IntPtr wParam,  IntPtr lParam);
		public static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wParam, IntPtr lParam);

		#endregion CallNextHookEx

		#region DefWindowProc

		[DllImport("user32.dll")]
		internal static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

		#endregion //DefWindowProc

		// MBS 10/26/06
		#region DwmIsCompositionEnabled
		[DllImport("dwmapi.dll")]
		private extern static int DwmIsCompositionEnabled(out bool enabled);
		#endregion //DwmIsCompositionEnabled

		#region EnableMenuItem

		[DllImport("user32.dll")]
		internal static extern bool EnableMenuItem(IntPtr hMenu, int uIDEnableItem, int uEnable);

		#endregion //EnableMenuItem

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region EnumChildWindows

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll")]
		internal static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam); 
		
		#endregion //EnumChildWindows

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region EnumThreadWindows

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll")]
		internal static extern bool EnumThreadWindows(int dwThreadId, EnumWindowsProc lpfn, IntPtr lParam); 

		#endregion //EnumThreadWindows
 
        // AS 3/30/09 TFS16355 - WinForms Interop
        #region GetAncestor

        internal const int GA_PARENT = 1;
        internal const int GA_ROOT = 2;
        internal const int GA_ROOTOWNER = 3;

        internal static IntPtr GetAncestorApi(IntPtr hwnd, int gaFlags)
        {
            return GetAncestor(hwnd, gaFlags);
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetAncestor(IntPtr hwnd, int gaFlags);

        #endregion //GetAncestor

		#region GetCaretBlinkTime

		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		private static extern int GetCaretBlinkTime( );

		#endregion //GetCaretBlinkTime

		#region GetClipboardFormatName
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetClipboardFormatName(int format, StringBuilder lpString, int cchMax); 
		#endregion //GetClipboardFormatName

		// AS 10/26/04
		// In Whidbey/clr2, AppDomain.GetCurrentThreadId is obsolete in 
		// favor of Thread.CurrentThread.ManagedThreadId but that does not
		// return the same value and cannot be used with things like window
		// hooks, etc.
		//
		#region GetCurrentThreadIdApi
		internal static int GetCurrentThreadIdApi()
		{
			try
			{
				// MBS 1/10/07 - FxCop
				// The assert is no longer needed due to the SuppressUnmanagedCodeSecurityAttribute on the class
				//
				//SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);

				//perm.Assert();

				return GetCurrentThreadIdApiUnsafe();
			}
			catch (System.Security.SecurityException)
			{
			}

#pragma warning disable 0618
			// MRS 8/29/07 
			// Microsoft obsolete warning on this method is wrong. See 
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=93566
			return AppDomain.GetCurrentThreadId();
#pragma warning restore 0618
		}

		private static int GetCurrentThreadIdApiUnsafe()
		{
			return GetCurrentThreadId();
		}

		[DllImport("kernel32")]
		private static extern int GetCurrentThreadId();

		#endregion //GetCurrentThreadIdApi

		// AS 7/24/07 MousePanningDecorator
		#region GetCursorPos

		[DllImport("user32")]
		internal static extern int GetCursorPos(ref POINT lpPoint);

		#endregion //GetCursorPos

		#region GetDoubleClickTime

		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		private static extern int GetDoubleClickTime( );

		#endregion //GetDoubleClickTime

		// AS 1/3/12 TFS98257
		#region GetFocus
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetFocus(); 
		#endregion //GetFocus

		// AS 2/22/12 TFS101038
		#region GetMessageExtraInfo
		[DllImport("user32.dll")]
		internal static extern IntPtr GetMessageExtraInfo(); 
		#endregion //GetMessageExtraInfo

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region GetMessagePos

		[DllImport("user32")]
		internal static extern int GetMessagePos();

		#endregion //GetMessagePos

		#region GetMessageTime

		[DllImport("user32.dll")]
		internal static extern int GetMessageTime();

		#endregion //GetMessageTime

		// JJD 11/05/07 - BR28049 - Added
        #region GetMonitorInfo (MONITORINFOEX)

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
		private static extern bool GetMonitorInfo( HandleRef handle, [In, Out] MONITIORINFOEX monitorInfo);

        #endregion //GetMonitorInfo (MONITORINFOEX)

		#region GetMonitorInfo (MONITORINFO)

		[DllImport("user32.dll")]
		internal static extern bool GetMonitorInfo(IntPtr hMonitor, out MONITORINFO lpmi);

		#endregion //GetMonitorInfo (MONITORINFO)

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region GetParent

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetParent(IntPtr hwnd);

        #endregion //GetParent

		#region GetSystemMenu

		internal static IntPtr GetSystemMenuApi(IntPtr hWnd, bool revert)
		{
			return GetSystemMenu(hWnd, revert);
		}

		[DllImport("user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool revert);
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)]bool revert);

		#endregion GetSystemMenu

        #region GetSystemMetrics

        [DllImport( "user32.dll", CharSet = CharSet.Auto, ExactSpelling = true )]
		private static extern int GetSystemMetrics( int index);

		#endregion //GetSystemMetrics

		#region GetWindow

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		#endregion //GetWindow

		#region GetWindowInfo

		[DllImport("user32")]
		private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

		#endregion //GetWindowInfo

		#region GetWindowLong

		// MD 10/31/06 - 64-Bit Support
		//internal static int GetWindowLongApi(IntPtr hwnd, int nIndex)
		//{
		//    return GetWindowLong(hwnd, nIndex);
		//}
		//
		//[DllImport("user32", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		//private static extern int GetWindowLong(IntPtr hwnd, int nIndex);
		[DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

		internal static IntPtr GetWindowLong(IntPtr hwnd, int nIndex)
		{
			if (IntPtr.Size == PtrSizeOn32Bit)
				return GetWindowLong32(hwnd, nIndex);
			else
				return GetWindowLongPtr64(hwnd, nIndex);
		}

		#endregion GetWindowLong

		// AS 11/7/06 BR17495
		#region GetWindowPlacement
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll")]
		static extern internal bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
		#endregion //GetWindowPlacement

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region MapWindowPoints
        [DllImport("user32")]
        internal static extern int MapWindowPoints(IntPtr hwndSrc, IntPtr hwndDest, [In, Out] ref POINT pt, int ptCount);

        #endregion //MapWindowPoints

        // JJD 11/05/07 - BR28049 - Added
        #region MonitorFromPoint

        [DllImport( "user32.dll", ExactSpelling = true )]
		internal static extern IntPtr MonitorFromPoint( POINT pt, int flags);

        #endregion //MonitorFromPoint

		// AS 8/4/11 TFS83465/TFS83469
		#region MonitorFromRect

		[DllImport("user32.dll", ExactSpelling = true)]
		internal static extern IntPtr MonitorFromRect([In] ref RECT rect, int flags);

		#endregion //MonitorFromRect

		#region MonitorFromWindow

		[DllImport("user32.dll")]
		internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		#endregion //MonitorFromWindow

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region PostMessage

		[DllImport("user32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		internal static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam); 
		
		#endregion //PostMessage

		#region SendMessage

		// MD 10/31/06 - 64-Bit Support
		//internal static int SendMessageApi(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		internal static IntPtr SendMessageApi(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			return SendMessage(hWnd, msg, wparam, lparam);
		}

		[DllImport("user32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam);
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam);

		#endregion SendMessage

		#region SetWindowLong

		// MD 10/31/06 - 64-Bit Support
		//internal static int SetWindowLongApi(IntPtr hwnd, int nIndex, int dwNewLong)
		//{
		//    return SetWindowLong(hwnd, nIndex, dwNewLong);
		//}
		//
		//[DllImport("user32", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		//private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);
		[DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
		private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
		private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);


		internal static IntPtr SetWindowLong(IntPtr hwnd, int nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size == PtrSizeOn32Bit)
				return SetWindowLong32(hwnd, nIndex, dwNewLong);
			else
				return SetWindowLongPtr64(hwnd, nIndex, dwNewLong);
		}

		#endregion SetWindowLong

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region SetWindowPlacement
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll")]
		static extern internal bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
		#endregion //SetWindowPlacement

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

		#region SetWindowsHookEx

		// JJD 5/03/02 added hook apis
		[DllImport("user32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//public static extern IntPtr SetWindowsHookEx(int id, HookProc hookProc, int hmod,int dwThreadId);
		public static extern IntPtr SetWindowsHookEx(int id, HookProc hookProc, IntPtr hmod, int dwThreadId);

		#endregion SetWindowsHookEx

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region ShowWindow
		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll")]
		static extern internal bool ShowWindow(IntPtr hwnd, int nCmdShow); 
		#endregion //ShowWindow

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region TrackPopupMenuEx

		[DllImport("user32.dll")]
		internal static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

		#endregion //TrackPopupMenuEx

		#region UnhookWindowsHookEx

		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs(UnmanagedType.Bool)]
		// JJD 5/03/02 added hook apis
		[DllImport("user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		public static extern bool UnhookWindowsHookEx(IntPtr hook);

		#endregion UnhookWindowsHookEx

		#endregion //APIs

		#region Delegates

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region EnumWindowsProc
		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam); 
		#endregion //EnumWindowsProc

		#region HookProc

		// JJD 5/03/02 added hook apis
		// MD 10/31/06 - 64-Bit Support
		//public delegate int HookProc(int ncode, IntPtr wParam, IntPtr lParam);
		public delegate IntPtr HookProc(int ncode, IntPtr wParam, IntPtr lParam);

		#endregion HookProc

		#endregion Delegates

        #region Properties

        #region CaretBlinkTime

        private static bool _unsafeCaretBlinkTimeDoesntWork;
		private static bool _systemInformationCaretBlinkTimeDoesntWork;






		internal static int CaretBlinkTime
		{
			get
			{
				// MBS 1/10/07 - FxCop
				// The assert is no longer needed due to the SuppressUnmanagedCodeSecurityAttribute on the class
				//
				//SecurityPermission perm = new SecurityPermission( SecurityPermissionFlag.UnmanagedCode );

				if ( ! _unsafeCaretBlinkTimeDoesntWork )
				{
					try
					{
						// MBS 1/10/07
						//perm.Assert();

						return UnsafeCaretBlinkTime( );
					}
					catch
					{
						_unsafeCaretBlinkTimeDoesntWork = true;
					}
				}

				if ( !_systemInformationCaretBlinkTimeDoesntWork )
				{
					try
					{
						return GetSystemInformationCaretBlinkTime( );
					}
					catch
					{
						_systemInformationCaretBlinkTimeDoesntWork = true;
					}
				}

				return 500;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int GetSystemInformationCaretBlinkTime( )
		{
			return System.Windows.Forms.SystemInformation.CaretBlinkTime;
		}

		private static int UnsafeCaretBlinkTime( )
		{
			// AS 1/22/03 UWG1961
			// When you set the system blink rate to none,
			// this api returns -1 which is a not a valid interval
			//
			//return GetCaretBlinkTime();
			int blinkTime = GetCaretBlinkTime( );

			// SSP 1/24/03 UWG1961
			// As it truns out, setting the Interval to 0 also causes an exception so I'm
			// reverting back the change that return 0 instead of -1.
			//
			//if (blinkTime < 0)
			//	return 0;

			return blinkTime;
		}

		#endregion //CaretBlinkTime

		#region DoubleClickTime

		private static bool _unsafeDoubleClickTimeDoesntWork;
		private static bool _systemInformationDoubleClickTimeDoesntWork;






		internal static int DoubleClickTime
		{
			get
			{
				// MBS 1/10/07 - FxCop
				// The assert is no longer needed due to the SuppressUnmanagedCodeSecurityAttribute on the class
				//
				//SecurityPermission perm = new SecurityPermission( SecurityPermissionFlag.UnmanagedCode );

				if ( ! _unsafeDoubleClickTimeDoesntWork )
				{
					try
					{
						// MBS 1/10/07
						//perm.Assert();

						return UnsafeDoubleClickTime( );
					}
					catch
					{
						_unsafeDoubleClickTimeDoesntWork = true;
					}
				}

				if ( !_systemInformationDoubleClickTimeDoesntWork )
				{
					try
					{
						return GetSystemInformationDoubleClickTime( );
					}
					catch
					{
						_systemInformationDoubleClickTimeDoesntWork = true;
					}
				}

				return 500;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static int GetSystemInformationDoubleClickTime( )
		{
			return System.Windows.Forms.SystemInformation.DoubleClickTime;
		}

		private static int UnsafeDoubleClickTime( )
		{
			return GetDoubleClickTime( );
		}

		#endregion //DoubleClickTime

		#region DoubleClickSize

		private static bool _unsafeDoubleClickSizeDoesntWork;
		private static bool _systemInformationDoubleClickSizeDoesntWork;






		internal static Size DoubleClickSize
		{
			get
			{
				// MBS 1/10/07 - FxCop
				// The assert is no longer needed due to the SuppressUnmanagedCodeSecurityAttribute on the class
				//
				//SecurityPermission perm = new SecurityPermission( SecurityPermissionFlag.UnmanagedCode );

				if ( ! _unsafeDoubleClickSizeDoesntWork )
				{
					try
					{
						// MBS 1/10/07
						//perm.Assert();

						return UnsafeDoubleClickSize( );
					}
					catch
					{
						_unsafeDoubleClickSizeDoesntWork = true;
					}
				}

				if ( !_systemInformationDoubleClickSizeDoesntWork )
				{
					try
					{
						return GetSystemInformationDoubleClickSize( );
					}
					catch
					{
						_systemInformationDoubleClickSizeDoesntWork = true;
					}
				}

				return new Size(4, 4);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Size GetSystemInformationDoubleClickSize( )
		{
			System.Drawing.Size size = System.Windows.Forms.SystemInformation.DoubleClickSize;

			return new Size(ConvertToLogicalPixels(size.Width), ConvertToLogicalPixels(size.Height));
		}

		private static Size UnsafeDoubleClickSize( )
		{
			return new Size(GetSystemMetrics(0x24), GetSystemMetrics(0x25));
		}

		#endregion //DoubleClickSize

		#region DragSize

		private static bool _unsafeDragSizeDoesntWork;
		private static bool _systemInformationDragSizeDoesntWork;






		internal static Size DragSize
		{
			get
			{
				// MBS 1/10/07 - FxCop
				// The assert is no longer needed due to the SuppressUnmanagedCodeSecurityAttribute on the class
				//
				//SecurityPermission perm = new SecurityPermission( SecurityPermissionFlag.UnmanagedCode );

				if ( ! _unsafeDragSizeDoesntWork )
				{
					try
					{
						// MBS 1/10/07
						//perm.Assert();

						return UnsafeDragSize( );
					}
					catch
					{
						_unsafeDragSizeDoesntWork = true;
					}
				}

				if ( !_systemInformationDragSizeDoesntWork )
				{
					try
					{
						return GetSystemInformationDragSize( );
					}
					catch
					{
						_systemInformationDragSizeDoesntWork = true;
					}
				}

				return new Size(4, 4);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Size GetSystemInformationDragSize( )
		{
			System.Drawing.Size size = System.Windows.Forms.SystemInformation.DragSize;

			return new Size(ConvertToLogicalPixels(size.Width), ConvertToLogicalPixels(size.Height));
		}

		private static Size UnsafeDragSize( )
		{
			return new Size(GetSystemMetrics(0x44), GetSystemMetrics(0x45));
		}

		#endregion //DragSize

        // JJD 11/05/07 - BR28049 - Added
		#region ConvertToDevicePixels

		private static int ConvertToDevicePixels(double logicalValue)
		{
			// AS 3/24/10 TFS27164
			//int pixelScreenWidth = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
			//
			//double logicalPixelScreenWidth = SystemParameters.PrimaryScreenWidth;
			//
			//return (int)((logicalValue * (double)pixelScreenWidth) / logicalPixelScreenWidth);
			return Utilities.ConvertFromLogicalPixels(logicalValue);
		}

		#endregion //ConvertToDevicePixels

		#region ConvertToLogicalPixels

		private static double ConvertToLogicalPixels(int pixelValue)
		{
			// AS 3/24/10 TFS27164
			//int pixelScreenWidth = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
			//
			//double logicalPixelScreenWidth = SystemParameters.PrimaryScreenWidth;
			//
			//return ((double)pixelValue * logicalPixelScreenWidth) / (double)pixelScreenWidth;
			return Utilities.ConvertToLogicalPixels(pixelValue);
		}

		#endregion //ConvertToLogicalPixels

		// AS 3/24/10 TFS27164
		// In addition to the other changes I figured I'd try to avoid loading the windows forms assembly.
		//
		#region PrimaryMonitorSize
		internal static Size PrimaryMonitorSize
		{
			get
			{
				if (!NativeWindowMethods._securityException)
				{
					try
					{
						return PrimaryMonitorSizeUnsafe;
					}
					catch (SecurityException)
					{
						NativeWindowMethods._securityException = true;
					}
				}

				return PrimaryMonitorSizeFallback;
			}
		}
		private static Size PrimaryMonitorSizeUnsafe
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				return new Size(GetSystemMetrics(0), GetSystemMetrics(1));
			}
		}


		private static Size PrimaryMonitorSizeFallback
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get 
			{
				System.Drawing.Size size = SystemInformation.PrimaryMonitorSize;
				return new Size(size.Width, size.Height); 
			}
		}
		#endregion //PrimaryMonitorSize

        // JJD 11/02/07 - BR28049 - Added
		#region VirtualScreenArea

		internal static Rect VirtualScreenArea
		{
			get
			{
				if (hasSecurityExceptionBeenThrown)
					return VirtualScreenAreaFallback;
				try
				{
                    return new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop, SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
				}
				catch (SecurityException)
				{
					hasSecurityExceptionBeenThrown = true;
					return VirtualScreenAreaFallback;
				}
			}
		}

		private static Rect VirtualScreenAreaFallback
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
                System.Drawing.Rectangle pixelRect = System.Windows.Forms.SystemInformation.VirtualScreen;

				Rect rect = new Rect();

				rect.X = ConvertToLogicalPixels(pixelRect.X);
				rect.Y = ConvertToLogicalPixels(pixelRect.Y);
				rect.Width = ConvertToLogicalPixels(pixelRect.Width);
				rect.Height = ConvertToLogicalPixels(pixelRect.Height);

				return rect;
			}
		}

		#endregion //VirtualScreenArea

        // JJD 11/02/07 - BR28049 - Added
		#region VirtualScreenLeft

		internal static double VirtualScreenLeft
		{
			get
			{
				if (hasSecurityExceptionBeenThrown)
					return VirtualScreenLeftFallback;

				try
				{
					return SystemParameters.VirtualScreenLeft;
				}
				catch (SecurityException)
				{
					hasSecurityExceptionBeenThrown = true;
					return VirtualScreenLeftFallback;
				}
			}
		}

		private static double VirtualScreenLeftFallback
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				return ConvertToLogicalPixels(System.Windows.Forms.SystemInformation.VirtualScreen.Left);
			}
		}

		#endregion //VirtualScreenLeft

		#region VirtualScreenHeight

		internal static double VirtualScreenHeight
		{
			get
			{
				if (hasSecurityExceptionBeenThrown)
					return VirtualScreenHeightFallback;

				try
				{
					return SystemParameters.VirtualScreenHeight;
				}
				catch (SecurityException)
				{
					hasSecurityExceptionBeenThrown = true;
					return VirtualScreenHeightFallback;
				}
			}
		}

		private static double VirtualScreenHeightFallback
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				return ConvertToLogicalPixels(System.Windows.Forms.SystemInformation.VirtualScreen.Height);
			}
		}

		#endregion //VirtualScreenHeight

        // JJD 11/02/07 - BR28049 - Added
		#region VirtualScreenTop

		internal static double VirtualScreenTop
		{
			get
			{
				if (hasSecurityExceptionBeenThrown)
					return VirtualScreenTopFallback;

				try
				{
					return SystemParameters.VirtualScreenTop;
				}
				catch (SecurityException)
				{
					hasSecurityExceptionBeenThrown = true;
					return VirtualScreenTopFallback;
				}
			}
		}

		private static double VirtualScreenTopFallback
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				return ConvertToLogicalPixels(System.Windows.Forms.SystemInformation.VirtualScreen.Top);
			}
		}

		#endregion //VirtualScreenTop

		#region VirtualScreenWidth

		internal static double VirtualScreenWidth
		{
			get
			{
				if (hasSecurityExceptionBeenThrown)
					return VirtualScreenWidthFallback;

				try
				{
					return SystemParameters.VirtualScreenWidth;
				}
				catch (SecurityException)
				{
					hasSecurityExceptionBeenThrown = true;
					return VirtualScreenWidthFallback;
				}
			}
		}

		private static double VirtualScreenWidthFallback
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				return ConvertToLogicalPixels(System.Windows.Forms.SystemInformation.VirtualScreen.Width);
			}
		}

		#endregion //VirtualScreenWidth

		#region WorkArea

		internal static Rect WorkArea
		{
			get
			{
				if (hasSecurityExceptionBeenThrown)
					return WorkAreaFallback;
				try
				{
                    return SystemParameters.WorkArea;
                }
				catch (SecurityException)
				{
					hasSecurityExceptionBeenThrown = true;
					return WorkAreaFallback;
				}
			}
		}

		private static Rect WorkAreaFallback
		{
			[MethodImpl(MethodImplOptions.NoInlining)]
			get
			{
				System.Drawing.Rectangle pixelRect = System.Windows.Forms.SystemInformation.WorkingArea;

				Rect rect = new Rect();

				rect.X = ConvertToLogicalPixels(pixelRect.X);
				rect.Y = ConvertToLogicalPixels(pixelRect.Y);
				rect.Width = ConvertToLogicalPixels(pixelRect.Width);
				rect.Height = ConvertToLogicalPixels(pixelRect.Height);

				return rect;
			}
		}

		#endregion //WorkArea

		#endregion //Properties

		#region Methods

		// MD 10/31/06 - 64-Bit Support
		#region AddBits

		internal static IntPtr AddBits(IntPtr ptr, int bitsToAdd)
		{
			if (IntPtr.Size == NativeWindowMethods.PtrSizeOn32Bit)
				return new IntPtr(ptr.ToInt32() | bitsToAdd);
			else
				return new IntPtr(ptr.ToInt64() | (uint)bitsToAdd);
		}

		#endregion AddBits

		#region AreBitsPresent

		internal static bool AreBitsPresent(IntPtr ptr, int bits)
		{
			if (IntPtr.Size == NativeWindowMethods.PtrSizeOn32Bit)
				return (ptr.ToInt32() & bits) != 0;
			else
				return (ptr.ToInt64() & bits) != 0;
		}

		#endregion AreBitsPresent

		// MD 10/25/07 - BR27412
		#region EncodeSize

		/// <summary>
		/// Encodes a size into and IntPtr to be used for certain window messages.
		/// </summary>
		internal static IntPtr EncodeSize(int width, int height)
		{
			return new IntPtr((height << 16) | width);
		}

		#endregion EncodeSize

		// AS 2/22/12 TFS101038
		#region GetBits

		internal static long GetBits(IntPtr ptr)
		{
			if (IntPtr.Size == NativeWindowMethods.PtrSizeOn32Bit)
				return ptr.ToInt32();
			else
				return ptr.ToInt64();
		}

		#endregion GetBits

		// AS 7/24/07 MousePanningDecorator
		#region GetCursorPos
		internal static Point GetCursorPosApi()
		{
			if (false == _securityException)
			{
				try
				{
					return GetCursorPosUnsafe();
				}
				catch (SecurityException)
				{
					_securityException = true;
				}
			}

			return GetCursorPosSafe();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Point GetCursorPosSafe()
		{
			System.Drawing.Point screenPt = System.Windows.Forms.Cursor.Position;
			return new Point(screenPt.X, screenPt.Y);
		}

		private static Point GetCursorPosUnsafe()
		{
			POINT pt = new POINT();
			GetCursorPos(ref pt);
			return pt.Point;
		}
		#endregion //GetCursorPos

		#region GetHitTestResult

		internal static HitTestResults GetHitTestResult(ToolWindowPart part)
		{
			switch (part)
			{
				case ToolWindowPart.BorderBottom:
					return HitTestResults.HTBOTTOM;
				case ToolWindowPart.BorderBottomLeft:
					return HitTestResults.HTBOTTOMLEFT;
				case ToolWindowPart.BorderBottomRight:
					return HitTestResults.HTBOTTOMRIGHT;
				case ToolWindowPart.BorderLeft:
					return HitTestResults.HTLEFT;
				case ToolWindowPart.BorderRight:
					return HitTestResults.HTRIGHT;
				case ToolWindowPart.BorderTop:
					return HitTestResults.HTTOP;
				case ToolWindowPart.BorderTopLeft:
					return HitTestResults.HTTOPLEFT;
				case ToolWindowPart.BorderTopRight:
					return HitTestResults.HTTOPRIGHT;
				case ToolWindowPart.Caption:
				// we're going to treat the caption as part of the client area
				//return HitTestResults.HTCAPTION;
				case ToolWindowPart.Content:
					return HitTestResults.HTCLIENT;
				case ToolWindowPart.ResizeGrip:
					return HitTestResults.HTBOTTOMRIGHT;
				default:
					Debug.Fail("Unexpected part:" + part.ToString());
					return HitTestResults.HTCLIENT;

			}
		}

		#endregion //GetHitTestResult

		#region GetNonClientSize
		internal static SIZE GetNonClientSize(IntPtr hwnd)
		{
			int style = IntPtrToInt32(GetWindowLong(hwnd, GWL_STYLE));
			int styleEx = IntPtrToInt32(GetWindowLong(hwnd, GWL_EXSTYLE));
			RECT rect = new RECT();
			AdjustWindowRectExApi(ref rect, style, false, styleEx);
			return new SIZE(rect.right - rect.left, rect.bottom - rect.top);
		}
		#endregion // GetNonClientSize

		#region GetRegisteredMessageName
		internal static string GetRegisteredMessageName(int message)
		{
			if (message >= 0xC000 && message <= 0xFFFF)
			{
				StringBuilder sb = new StringBuilder(256);
				int len = NativeWindowMethods.GetClipboardFormatName(message, sb, sb.Capacity);

				if (len > 0)
				{
					return sb.ToString(0, len);
				}
			}

			return null;
		} 
		#endregion //GetRegisteredMessageName

		#region GetScreenHeight
		internal static int GetScreenHeight(IntPtr hwnd)
		{
			IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONULL);

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// This also happens when restoring a minimized window to maximized (at least 
			// while in the nccalcsize). The documentation indicates that it will consider 
			// the restore bounds when minimized but in ths case the window state is already 
			// maximized.
			//
			if (hMonitor == IntPtr.Zero)
			{
				hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
			}

			Debug.Assert(IntPtr.Zero != hMonitor, "The specified window is not on screen!");

			if (IntPtr.Zero != hMonitor)
			{
				MONITORINFO monitor = new MONITORINFO();
				monitor.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO));
				if (GetMonitorInfo(hMonitor, out monitor))
				{
					return monitor.rcMonitor.bottom - monitor.rcMonitor.top;
				}
			}

			Debug.Assert(null == HwndSource.FromHwnd(hwnd));

			return Utilities.ConvertFromLogicalPixels(SystemParameters.PrimaryScreenHeight);
		}
		#endregion //GetScreenHeight

		#region GetWindowInfo
		internal static WINDOWINFO GetWindowInfo(IntPtr hwnd)
		{
			NativeWindowMethods.WINDOWINFO info = new NativeWindowMethods.WINDOWINFO();
			info.cbSize = Marshal.SizeOf(info);
			NativeWindowMethods.GetWindowInfo(hwnd, ref info);
			return info;
		}
		#endregion //GetWindowInfo

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region GetWindowPlacement
		internal static NativeWindowMethods.WINDOWPLACEMENT GetWindowPlacement(IntPtr hwnd)
		{
			Debug.Assert(hwnd != IntPtr.Zero);

			NativeWindowMethods.WINDOWPLACEMENT placement = new NativeWindowMethods.WINDOWPLACEMENT();
			placement.length = Marshal.SizeOf(typeof(NativeWindowMethods.WINDOWPLACEMENT));

			if (hwnd != IntPtr.Zero)
			{
				bool retVal = NativeWindowMethods.GetWindowPlacement(hwnd, ref placement);
				Debug.Assert(retVal, "GetWindowPlacement failed!");
			}

			return placement;
		} 
		#endregion //GetWindowPlacement

		// AS 11/7/06 BR17495
		#region GetWindowState

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		internal static WindowState GetWindowState(Window window)
		{
			if (window == null)
				return WindowState.Normal;

			WindowInteropHelper wih = new WindowInteropHelper(window);

			if (wih.Handle == IntPtr.Zero)
				return window.WindowState;

			return GetWindowState(wih.Handle);
		}

		internal static WindowState GetWindowState(IntPtr hwnd)
		{
			Debug.Assert(IntPtr.Zero != hwnd);
			WindowState state = WindowState.Normal;

			if (IntPtr.Zero != hwnd)
			{
				NativeWindowMethods.WINDOWPLACEMENT placement = GetWindowPlacement(hwnd);

				if (placement.showCmd == SW_SHOWMINIMIZED)
					state = WindowState.Minimized;
				else if (placement.showCmd == SW_SHOWMAXIMIZED)
					state = WindowState.Maximized;
				else
				{
					Debug.Assert(placement.showCmd == SW_SHOWNORMAL, "Unexpected show command!");
					state = WindowState.Normal;
				}
			}

			return state;
		}
		#endregion //GetWindowState

		// JJD 11/05/07 - BR28049 - Added
		#region GetWorkArea

        internal static Rect GetWorkArea(Point point)
		{
            ScreenHelper sh = ScreenHelper.FromPoint(point);

			// AS 6/7/11 NA 11.2 Excel Style Filtering
			// In CLR4, the popup will not go over the taskbar. In discussing the issue with Joe we decided 
			// that we shouldn't be returning the full bounds of the screen from GetWorkArea but only the 
			// working area portion of the screen.
			//
			//return sh.Bounds;
			return sh.WorkingArea;
		}

		// AS 8/4/11 TFS83465/TFS83469
		internal static Rect GetWorkArea(Rect screenRect)
		{
			ScreenHelper sh = ScreenHelper.FromRect(screenRect);
			return sh.WorkingArea;
		}

		#endregion //GetWorkArea

		#region GetXFromLParam
		internal static int GetXFromLParam(IntPtr lParam)
		{
			// explicitly casted to short to maintain sign
			return SignedLoWord(IntPtrToInt32(lParam));
		}
		#endregion //GetXFromLParam

		#region GetYFromLParam
		internal static int GetYFromLParam(IntPtr lParam)
		{
			// explicitly casted to short to maintain sign
			return SignedHiWord(IntPtrToInt32(lParam));
		}
		#endregion //GetYFromLParam

		#region IntPtrToInt32
		internal static int IntPtrToInt32(IntPtr intPtr)
		{
			return unchecked((int)intPtr.ToInt64());
		}
		#endregion // IntPtrToInt32

		// MBS 10/26/06 - Glass Support
		#region IsDWMCompositionEnabled
		internal static bool IsDWMCompositionEnabled
		{
			get
			{
				// We can't have composition if we're not in Vista
				if (System.Environment.OSVersion.Version.Major < 6)
					return false;

				// MD 12/15/06 - FxCop
				// See comment on assert below
				//SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);

				try
				{
					// MD 12/15/06 - FxCop
					// This is not needed due to the SuppressUnmanagedCodeSecurityAttribute on NativeWindowMethods.
					//perm.Assert();
					return UnsafeIsDWMCompositionEnabled();
				}
				catch
				{
					return false;
				}
			}
		}

		private static bool UnsafeIsDWMCompositionEnabled()
		{
			bool enabled;
			int result = DwmIsCompositionEnabled(out enabled);

			return result == 0 && enabled;
		}
		#endregion //IsDWMCompositionEnabled

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region IsTopMostApi
        internal static bool IsTopMostApi(IntPtr hwnd)
        {
            bool isTopMost = false;

            if (hwnd != IntPtr.Zero)
            {
                IntPtr rootWindow = NativeWindowMethods.GetAncestorApi(hwnd, NativeWindowMethods.GA_ROOT);
                Debug.Assert(rootWindow != IntPtr.Zero, "The root window should not be 0.");

                if (rootWindow != IntPtr.Zero)
                {
                    IntPtr exStyle = NativeWindowMethods.GetWindowLong(rootWindow, NativeWindowMethods.GWL_EXSTYLE);

                    isTopMost = NativeWindowMethods.AreBitsPresent(exStyle, NativeWindowMethods.WS_EX_TOPMOST);
                }
            }

            return isTopMost;
        }
        #endregion //IsTopMostApi

		#region PointFromLParam

		internal static NativeWindowMethods.POINT PointFromLParam(IntPtr lParam)
		{
			int value = IntPtrToInt32(lParam);

			// AS 11/7/06 BR17498
			//return new Point(
			//	value & 0xffff,
			//	( value >> 16 ) & 0xffff );
			return new NativeWindowMethods.POINT(
				SignedLoWord(value),
				SignedHiWord(value));
		}

		#endregion PointFromLParam

		#region RemoveBits

		internal static IntPtr RemoveBits(IntPtr ptr, int bitsToRemove)
		{
			if (IntPtr.Size == NativeWindowMethods.PtrSizeOn32Bit)
				return new IntPtr(ptr.ToInt32() & ~bitsToRemove);
			else
				return new IntPtr(ptr.ToInt64() & ~bitsToRemove);
		}

		#endregion RemoveBits

		#region SignedLoWord
		internal static int SignedLoWord(int value)
		{
			return (short)(value & 0xffff);
		}
		#endregion // SignedLoWord

		#region SignedHiWord
		internal static int SignedHiWord(int value)
		{
			return (short)((value >> 16) & 0xffff);
		}
		#endregion // SignedHiWord

		#endregion //Methods

		#region ScreenHelper
		private class ScreenHelper
		{
			private object _screenObject;
			private Rect _bounds;

			// AS 6/7/11 NA 11.2 Excel Style Filtering
			// While testing the resizable menus using our PopupResizeDecorator I see that popups 
			// will not overlay the taskbar in CLR4. They are using the working area of the screen 
			// so I'm exposing that to allow the caller to decide what to use.
			//
			private Rect _workingArea;

			#region Constructors

			private ScreenHelper(object screenObject)
			{
				this._screenObject = screenObject;

				if (screenObject == null)
				{
					// AS 6/28/11 TFS78202
					// Just as PointToScreen does, this should return the bounds in device units.
					//
					//this._bounds = new Rect(SystemParameters.VirtualScreenLeft,
					//                        SystemParameters.VirtualScreenTop,
					//                        SystemParameters.VirtualScreenWidth,
					//                        SystemParameters.VirtualScreenHeight);
					this._bounds = new Rect(Utilities.ConvertFromLogicalPixels(SystemParameters.VirtualScreenLeft),
											Utilities.ConvertFromLogicalPixels(SystemParameters.VirtualScreenTop),
											Utilities.ConvertFromLogicalPixels(SystemParameters.VirtualScreenWidth),
											Utilities.ConvertFromLogicalPixels(SystemParameters.VirtualScreenHeight));

					// AS 6/7/11 NA 11.2 Excel Style Filtering
					_workingArea = _bounds;

					return;
				}

				if (!(screenObject is IntPtr))
				{
					// AS 6/7/11 NA 11.2 Excel Style Filtering
					//this._bounds = this.GetBoundsFallback();
					this._bounds = this.GetBoundsFallback(out _workingArea);
					return;
				}

				NativeWindowMethods.MONITIORINFOEX monitorInfo = new MONITIORINFOEX();

				GetMonitorInfo(new HandleRef(null, (IntPtr)screenObject), monitorInfo);

				double virtualWidthLogical = NativeWindowMethods.VirtualScreenWidth;
				int virtualWidthDevice = NativeWindowMethods.GetSystemMetrics(NativeWindowMethods.SM_CXVIRTUALSCREEN);

				// AS 6/28/11 TFS78202
				// Just as PointToScreen does, this should return the bounds in device units.
				//
				//double ratio = (double)virtualWidthLogical / virtualWidthDevice;
				double ratio = 1;

				RECT rc = monitorInfo._rcMonitor;

				this._bounds = new Rect(rc.left * ratio,
										rc.top * ratio,
										rc.Width * ratio,
										rc.Height * ratio);

				// AS 6/7/11 NA 11.2 Excel Style Filtering
				RECT rcWork = monitorInfo._rcWork;

				this._workingArea = new Rect(rcWork.left * ratio,
										rcWork.top * ratio,
										rcWork.Width * ratio,
										rcWork.Height * ratio);
			}

			#endregion //Constructors

			#region Properties

			#region Bounds

			internal Rect Bounds { get { return this._bounds; } }

			#endregion //Bounds

			// AS 6/7/11 NA 11.2 Excel Style Filtering
			#region WorkingArea

			internal Rect WorkingArea { get { return this._workingArea; } }

			#endregion //WorkingArea

			#endregion //Properties

			#region FromPoint static method

			internal static ScreenHelper FromPoint(Point point)
			{
				if (NativeWindowMethods.hasSecurityExceptionBeenThrown)
					return FromPointFallback(point);

				try
				{
					if (NativeWindowMethods.GetSystemMetrics(SM_CMONITORS) == 0)
						return new ScreenHelper(null);

					// AS 8/4/11 TFS83465/TFS83469
					// Since the point passed in is from PointToScreen and that is already device
					// units we don't need to convert from logical units.
					//
					//double virtualWidthLogical = NativeWindowMethods.VirtualScreenWidth;
					//int virtualWidthDevice = NativeWindowMethods.GetSystemMetrics(NativeWindowMethods.SM_CXVIRTUALSCREEN);
					//
					//double ratio = (double)virtualWidthDevice / virtualWidthLogical;
					//
					//POINT devicePoint = new POINT((int)(point.X * ratio),
					//                               (int)(point.Y * ratio));
					POINT devicePoint = new POINT((int)point.X, (int)point.Y);

					return new ScreenHelper(NativeWindowMethods.MonitorFromPoint(devicePoint, NativeWindowMethods.MONITOR_DEFAULTTONEAREST));
				}
				catch (SecurityException)
				{
					NativeWindowMethods.hasSecurityExceptionBeenThrown = true;
					return FromPointFallback(point);

				}
			}

			// AS 8/4/11 TFS83465/TFS83469
			internal static ScreenHelper FromRect(Rect rect)
			{
				// make sure we're dealing with a normalized rect
				rect = Utilities.RectFromPoints(rect.TopLeft, rect.BottomRight);

				if (!NativeWindowMethods.hasSecurityExceptionBeenThrown)
				{
					try
					{
						if (NativeWindowMethods.GetSystemMetrics(SM_CMONITORS) == 0)
							return new ScreenHelper(null);

						RECT deviceRect = new RECT((int)rect.Left, (int)rect.Top,(int)rect.Right, (int)rect.Bottom);

						return new ScreenHelper(NativeWindowMethods.MonitorFromRect(ref deviceRect, NativeWindowMethods.MONITOR_DEFAULTTONEAREST));
					}
					catch (SecurityException)
					{
						NativeWindowMethods.hasSecurityExceptionBeenThrown = true;
					}
				}

				return FromRectFallback(rect);
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static ScreenHelper FromPointFallback(Point point)
			{
				// AS 8/4/11 TFS83465/TFS83469
				// Since the point passed in is from PointToScreen and that is already device
				// units we don't need to convert from logical units.
				//
				//return new ScreenHelper(Screen.FromPoint(new System.Drawing.Point(NativeWindowMethods.ConvertToDevicePixels(point.X), NativeWindowMethods.ConvertToDevicePixels(point.Y))));
				return new ScreenHelper(Screen.FromPoint(new System.Drawing.Point((int)point.X, (int)point.Y)));
			}

			// AS 8/4/11 TFS83465/TFS83469
			[MethodImpl(MethodImplOptions.NoInlining)]
			private static ScreenHelper FromRectFallback(Rect rect)
			{
				var rectangle = System.Drawing.Rectangle.FromLTRB((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
				return new ScreenHelper(Screen.FromRectangle(rectangle));
			}

			#endregion //FromPoint static method

			#region GetBoundsFallback

			[MethodImpl(MethodImplOptions.NoInlining)]
			// AS 6/7/11 NA 11.2 Excel Style Filtering
			//private Rect GetBoundsFallback()
			private Rect GetBoundsFallback(out Rect workingRect)
			{
				Screen screen = this._screenObject as Screen;

				System.Drawing.Rectangle deviceBounds = screen.Bounds;

				// AS 6/7/11 NA 11.2 Excel Style Filtering
				System.Drawing.Rectangle workingBounds = screen.WorkingArea;
				workingRect = new Rect(workingBounds.Left, workingBounds.Top, workingBounds.Width, workingBounds.Height);

				// AS 6/28/11 TFS78202
				// Just as PointToScreen does, this should return the bounds in device units.
				//
				//return new Rect(NativeWindowMethods.ConvertToLogicalPixels(deviceBounds.Left),
				//                NativeWindowMethods.ConvertToLogicalPixels(deviceBounds.Top),
				//                NativeWindowMethods.ConvertToLogicalPixels(deviceBounds.Width),
				//                NativeWindowMethods.ConvertToLogicalPixels(deviceBounds.Height));
				return new Rect(deviceBounds.Left,
								deviceBounds.Top,
								deviceBounds.Width,
								deviceBounds.Height);
			}

			#endregion //GetBoundsFallback

		} 
		#endregion //ScreenHelper
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