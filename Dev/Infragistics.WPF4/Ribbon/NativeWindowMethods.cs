using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

namespace Infragistics.Windows.Ribbon
{







	[ System.Security.SuppressUnmanagedCodeSecurityAttribute() ]
	[ System.Runtime.InteropServices.ComVisible(false) ]
	internal class NativeWindowMethods
	{
		#region Member Variables

		// AS 8/6/12 TFS118105
		// os theme support flag
		internal static readonly bool AreThemesSupported = false; 

		#endregion //Member Variables

		#region Constructor
		static NativeWindowMethods()
		{
			AreThemesSupported = CanOSSupportThemes(); // AS 8/6/12 TFS118105
		}

		// MD 10/31/06 - Added for FxCop warning
		private NativeWindowMethods() { } 
		#endregion //Constructor

		#region Static Methods
		
			#region CloseForm

		internal static void CloseForm( Form form )
		{
			if (form == null || !form.IsHandleCreated || form.Disposing || form.IsDisposed)
				return;

			SecurityPermission perm;

			try
			{
				perm = new SecurityPermission( SecurityPermissionFlag.UnmanagedCode );

				perm.Demand();

				// MD 12/19/06 - FxCop
				// Moved from outside the try block below
				// send a close message to the specified form as if someone had pressed alt-f4
				SendMessageApi( form.Handle, WM_SYSCOMMAND, new IntPtr( SC_CLOSE ), IntPtr.Zero );
			}
			catch 
			{
				form.Close();
				return;
			}

			// MD 12/19/06 - FxCop
			// Moved into the try block above
			//// send a close message to the specified form as if someone had pressed alt-f4
			//SendMessageApi(form.Handle, WM_SYSCOMMAND, new IntPtr(SC_CLOSE), IntPtr.Zero);
		}

			#endregion //CloseForm

			// JM 03-12-03 WTB736
			#region ResetTreeTooltips
		
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

			#endregion //ResetTreeTooltips

			// JM 03-12-03 WTB736
			#region ResetTreeToolTipsAPI
		
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

			#endregion //ResetTreeToolTipsAPI
		
		#endregion //Static Methods

		#region Constants

		// MD 10/31/06 - 64-Bit Support
		private const int PtrSizeOn32Bit = 4;
		private const int PtrSizeOn64Bit = 8;

		internal static readonly IntPtr TRUE			= new IntPtr(1);
		internal static readonly IntPtr FALSE			= IntPtr.Zero;

		internal const int WM_SETREDRAW		  = 0x000B;
		internal const int WM_SYSCOMMAND	  = 0x0112;

		internal const int WM_MOUSEMOVE		  = 0x0200;
		internal const int WM_LBUTTONDOWN	  = 0x0201;
		internal const int WM_LBUTTONUP		  = 0x0202;
		internal const int WM_LBUTTONDBLCLK	  = 0x0203;
		internal const int WM_RBUTTONDOWN	  = 0x0204;
		internal const int WM_RBUTTONUP		  = 0x0205;
		internal const int WM_RBUTTONDBLCLK	  = 0x0206;
		internal const int WM_MBUTTONDOWN	  = 0x0207;
		internal const int WM_MBUTTONUP		  = 0x0208;
		internal const int WM_MBUTTONDBLCLK	  = 0x0209;

		internal const int WM_NCMOUSEMOVE     = 0x00A0;
		internal const int WM_NCLBUTTONDOWN   = 0x00A1;
		internal const int WM_NCLBUTTONUP     = 0x00A2;
		internal const int WM_NCLBUTTONDBLCLK = 0x00A3;
		internal const int WM_NCRBUTTONDOWN   = 0x00A4;
		internal const int WM_NCRBUTTONUP     = 0x00A5;
		internal const int WM_NCRBUTTONDBLCLK = 0x00A6;
		internal const int WM_NCMBUTTONDOWN   = 0x00A7;
		internal const int WM_NCMBUTTONUP     = 0x00A8;
		internal const int WM_NCMBUTTONDBLCLK = 0x00A9;

        // AS 10/10/08 TFS6236
        internal const int WM_MOVE = 0x0003;

		// JJD 8/29/06 - added undocumented windows messages 
		internal const int WM_NCUAHDRAWCAPTION = 0x00AE;
		internal const int WM_NCUAHDRAWFRAME = 0x00AF;


		internal const int WM_CONTEXTMENU		= 0x007B;
		internal const int WM_STYLECHANGING		= 0x007C;
		internal const int WM_STYLECHANGED 		= 0x007D;
		
		internal const int WM_PAINT				= 0x000F;
		internal const int WM_NCCALCSIZE		= 0x0083;
		internal const int WM_NCPAINT			= 0x0085;
		internal const int WM_NCACTIVATE		= 0x0086;
		internal const int WM_ERASEBKGND		= 0x0014;

		internal const int WM_MDICREATE			= 0x0220;
		internal const int WM_MDIDESTROY		= 0x0221;
		internal const int WM_MDIACTIVATE		= 0x0222;
		internal const int WM_MDIRESTORE		= 0x0223;
		internal const int WM_MDINEXT			= 0x0224;
		internal const int WM_MDIMAXIMIZE		= 0x0225;
		internal const int WM_MDISETMENU		= 0x0230;
		internal const int WM_MDIREFRESHMENU	= 0x0234;

		internal const int WM_ACTIVATE			= 0x0006;
		internal const int WM_ACTIVATEAPP		= 0x001C;
        internal const int SW_SCROLLCHILDREN	= 0x0001;
        internal const int SW_INVALIDATE		= 0x0002;
        internal const int SW_ERASE				= 0x0004;
		internal const int SW_SMOOTHSCROLL		= 0x0010;
		
		internal const int NULLREGION			= 1;
		internal const int SIMPLEREGION			= 2;
		internal const int COMPLEXREGION		= 3;

		internal const int WM_NCHITTEST			= 0x84;
		internal const int HTTRANSPARENT		= (-1);
		// JJD 8/10/06 - NA 2006 vol 3 
		internal const int WM_CREATE			= 0x0001;
		internal const int WM_PRINT				= 0x0317;
		internal const int WM_MDICASCADE		= 0x227;
		internal const int WM_MDIICONARRANGE	= 0x228;
		internal const int WM_MDITILE			= 0x226;
		internal const int HTNOWHERE	= 0;
		internal const int HTCLIENT     = 1;
		internal const int HTCAPTION     = 2;
		internal const int HTSYSMENU     = 3;
		internal const int HTGROWBOX     = 4;
		internal const int HTLEFT        = 10;
		internal const int HTRIGHT       = 11;
		internal const int HTTOP         = 12;
		internal const int HTTOPLEFT     = 13;
		internal const int HTTOPRIGHT    = 14;
		internal const int HTBOTTOM      = 15;
		internal const int HTBOTTOMLEFT  = 16;
		internal const int HTBOTTOMRIGHT = 17;
		internal const int HTBORDER		= 18;

		// AS 8/14/06 Office 2007 Ribbon
		internal const int HTMAXBUTTON	= 9;
		internal const int HTMINBUTTON	= 8;
		internal const int HTCLOSE = 20;

		internal const int WM_KEYDOWN		=	0x0100;
		internal const int WM_KEYUP			=	0x0101;
		internal const int WM_SYSKEYDOWN	=	0x0104;
		internal const int WM_SYSKEYUP		=	0x0105;

		// AS 1/20/03 DNF33
		internal const int WM_GETMINMAXINFO	= 0x24;

		// JM 03-12-03 WTB736
		internal const int GWL_STYLE			= (-16);
		internal const int GWL_EX_STYLE			= (-20);
		private const int TVS_NOTOOLTIPS		= 0x80;

		// MD 5/11/07 - BR22786
		internal const int GWL_HWNDPARENT		= (-8);

        // MBS 4/11/06 BR10948
        internal const int SPI_GETFONTSMOOTHING     = 0x004A;
        internal const int SPI_GETFONTSMOOTHINGTYPE = 0x200A; // Only valid in XP/2003 and later

		// JJD 8/14/06 - NA 2006 vol 3
		// Added support for merging riboon with caption area
		internal const int WS_CLIPCHILDREN = 0x02000000;
		internal const int WS_CLIPSIBLINGS = 0x04000000;
		internal const int WS_CAPTION = 0x00C00000;
		internal const int WS_DISABLED = 0x08000000;
		internal const int WS_SYSMENU = 0x00080000;
		internal const int WS_BORDER = 0x00800000;
		internal const int WS_MINIMIZEBOX = 0x00020000;
		internal const int WS_MAXIMIZEBOX = 0x00010000;
		internal const int WS_THICKFRAME = 0x00040000;
		internal const int WS_VISIBLE = 0x10000000;
		internal const int WS_EX_LAYERED = 0x00080000;
		internal const int WS_EX_WINDOWEDGE = 0x00000100;

		// MD 1/18/07 - BR19298
		internal const int WS_EX_TOOLWINDOW = 0x00000080;

		// JJD 8/15/06 - NA 2006 vol 3
		// Added support for merging the ribbon into the caption area
		internal const int DCX_WINDOW = 0x1;
		internal const int DCX_VALIDATE = 0x200000;
		internal const int DCX_PARENTCLIP = 0x20;
		internal const int DCX_NORESETATTRS = 0x4;
		internal const int DCX_NORECOMPUTE = 0x100000;
		internal const int DCX_LOCKWINDOWUPDATE = 0x400;
		internal const int DCX_INTERSECTUPDATE = 0x200;
		internal const int DCX_INTERSECTRGN = 0x80;
		internal const int DCX_EXCLUDEUPDATE = 0x100;
		internal const int DCX_EXCLUDERGN = 0x40;
		internal const int DCX_CLIPSIBLINGS = 0x10;
		internal const int DCX_CLIPCHILDREN = 0x8;
		internal const int DCX_CACHE = 0x2;
		
		internal const int PRF_NONCLIENT = 0x00000002;

		internal const int WVR_ALIGNTOP = 0x0010;
		internal const int WVR_ALIGNLEFT = 0x0020;
		internal const int WVR_ALIGNBOTTOM = 0x0040;
		internal const int WVR_ALIGNRIGHT = 0x0080;
		internal const int WVR_HREDRAW = 0x0100;
		internal const int WVR_VREDRAW = 0x0200;

		// AS 8/16/06 Office 2007 Ribbon
		internal const int WM_MOUSEWHEEL = 0x020A;

		internal const int WM_SETICON = 0x0080;

		// AS 9/12/06 BR15786
		internal const int WM_SETCURSOR = 0x0020;
		internal const int WM_INITMENUPOPUP = 0x117;
		internal const int WM_UNINITMENUPOPUP = 0x125;

		// AS 9/19/06
		internal const int WM_WINDOWPOSCHANGED = 0x47;

		// AS 11/8/06 BR17511
		internal const int WM_WINDOWPOSCHANGING = 0x46;

		// MD 9/26/06 - Office 2007 - Keyboard Access
		// currently defined blend function
		internal const int AC_SRC_OVER = 0x00;
		// alpha format flags
		internal const int AC_SRC_ALPHA = 0x01;

		// MD 10/12/06 - Keyboard Access
		internal const int MB_OK = 0x00000000;

        // MBS 10/30/06 - Glass/Vista
        internal const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

		// MD 1/12/07 - BR19029
		internal const int WM_ENTERSIZEMOVE = 0x0231;
		internal const int WM_EXITSIZEMOVE = 0x0232;

		// MD 1/16/07 - BR19179
		internal const int WM_DESTROY = 0x0002;
		internal const int WM_SHOWWINDOW = 0x0018;
		internal const int WM_QUERYUISTATE = 0x0129;

		// MD 3/8/07 - BR19764
		internal const int WM_SETTEXT = 0x000C;

		// MD 3/26/07 - BR21317
		internal const int WA_INACTIVE = 0;
		internal const int WA_ACTIVE = 1;
		internal const int WA_CLICKACTIVE = 2;

        internal const int WM_GETTITLEBARINFOEX = 0x033F;

		// AS 10/3/07
		internal const int WM_GETSYSMENU = 0x313;

		// AS 10/23/07 XamRibbonWindow IconResolved
		internal const int ICON_SMALL2 = 2;
		internal const int WM_GETICON = 0x007F;
		internal const int GCL_HICONSM = (-34);
		internal const int IDI_APPLICATION = 32512;
		internal const int IMAGE_ICON = 1;
		internal const uint LR_SHARED = 0x00008000;

			#region Hook Constants

		internal const int HC_ACTION			= 0;
		internal const int HC_NOREMOVE			= 3;

		internal const int WH_KEYBOARD			= 1;
		internal const int WH_GETMESSAGE		= 3;
		internal const int WH_CALLWNDPROC		= 4;
		internal const int WH_SYSMSGFILTER		= 6;
		internal const int WH_MOUSE				= 7;
		internal const int WH_FOREGROUNDIDLE	= 11;

			#endregion Hook Constants

			#region Menu (MF) constants

		internal const int MF_INSERT         			=	0x00000000;
		internal const int MF_CHANGE         			=	0x00000080;
		internal const int MF_APPEND         			=	0x00000100;
		internal const int MF_DELETE         			=	0x00000200;
		internal const int MF_REMOVE         			=	0x00001000;
		internal const int MF_BYCOMMAND      			=	0x00000000;
		internal const int MF_BYPOSITION     			=	0x00000400;
		internal const int MF_SEPARATOR      			=	0x00000800;
		internal const int MF_ENABLED        			=	0x00000000;
		internal const int MF_GRAYED         			=	0x00000001;
		internal const int MF_DISABLED       			=	0x00000002;
		internal const int MF_UNCHECKED      			=	0x00000000;
		internal const int MF_CHECKED        			=	0x00000008;
		internal const int MF_USECHECKBITMAPS			=	0x00000200;
		internal const int MF_STRING         			=	0x00000000;
		internal const int MF_BITMAP         			=	0x00000004;
		internal const int MF_OWNERDRAW      			=	0x00000100;
		internal const int MF_POPUP          			=	0x00000010;
		internal const int MF_MENUBARBREAK   			=	0x00000020;
		internal const int MF_MENUBREAK      			=	0x00000040;
		internal const int MF_UNHILITE       			=	0x00000000;
		internal const int MF_HILITE         			=	0x00000080;
		internal const int MF_DEFAULT        			=	0x00001000;
		internal const int MF_SYSMENU        			=	0x00002000;
		internal const int MF_HELP           			=	0x00004000;
		internal const int MF_RIGHTJUSTIFY   			=	0x00004000;
		internal const int MF_MOUSESELECT    			=	0x00008000;

			#endregion Menu (MF) constants

			#region Menu (MFS) constants

		internal const int MFS_GRAYED         			=	0x00000003;
		internal const int MFS_DISABLED        			=	0x00000003;

			#endregion Menu (MFS) constants

			#region MenuItemInfo (MIIM) constants

		internal const int MIIM_STATE     			=	0x00000001;
		internal const int MIIM_ID        			=	0x00000002;
		internal const int MIIM_SUBMENU   			=	0x00000004;
		internal const int MIIM_CHECKMARKS			=	0x00000008;
		internal const int MIIM_TYPE      			=	0x00000010;
		internal const int MIIM_DATA      			=	0x00000020;
		internal const int MIIM_STRING    			=	0x00000040;
		internal const int MIIM_BITMAP    			=	0x00000080;
		internal const int MIIM_FTYPE     			=	0x00000100;

			#endregion MenuItemInfo (MIIM) constants

			// AS 9/18/09 TFS19281
			#region MONITOR constants

		private const int MONITOR_DEFAULTTONULL = 0;
		private const int MONITOR_DEFAULTTOPRIMARY = 1;
		private const int MONITOR_DEFAULTTONEAREST = 2;

			#endregion //MONITOR constants

			#region Mouse Activate Constants

		internal const int WM_MOUSEACTIVATE		= 0x0021;
		internal const int MA_ACTIVATE			= 1;
		internal const int MA_ACTIVATEANDEAT	= 2;
		internal const int MA_NOACTIVATE		= 3;
		internal const int MA_NOACTIVATEANDEAT	= 4;

			#endregion Mouse Activate Constants

			#region RedrawWindow Constants

		internal const int RDW_INVALIDATE     	=	0x0001;
		internal const int RDW_INTERNALPAINT  	=	0x0002;
		internal const int RDW_ERASE          	=	0x0004;
		internal const int RDW_VALIDATE       	=	0x0008;
		internal const int RDW_NOINTERNALPAINT	=	0x0010;
		internal const int RDW_NOERASE        	=	0x0020;
		internal const int RDW_NOCHILDREN     	=	0x0040;
		internal const int RDW_ALLCHILDREN    	=	0x0080;
		internal const int RDW_UPDATENOW      	=	0x0100;
		internal const int RDW_ERASENOW       	=	0x0200;
		internal const int RDW_FRAME          	=	0x0400;
		internal const int RDW_NOFRAME        	=	0x0800;

			#endregion RedrawWindow Constants

			#region ShowWindow Constants

		// AS 11/7/06 BR17495
		internal const int SW_SHOWNORMAL	  = 1;
		internal const int SW_SHOWMINIMIZED	  = 2;
		internal const int SW_SHOWMAXIMIZED	  = 3;

		internal const int SW_SHOWNA		  = 0x0008;
		internal const int SW_SHOWNOACTIVATE  = 0x0004;
		internal const int SWP_NOACTIVATE     = 0x0010;
		internal const int SWP_SHOWWINDOW     = 0x0040;

		// MD 10/31/06 - 64-Bit Support
		//internal const int HWND_TOP			  = 0;
		//internal const int HWND_BOTTOM		  = 1;
		//internal const int HWND_TOPMOST		  = -1;
		//internal const int HWND_NOTOPMOST		  = -2;
		// MD 1/10/07 - Optimization
		// Not used
		//internal static readonly IntPtr HWND_TOP		= new IntPtr( 0 );
		//internal static readonly IntPtr HWND_BOTTOM		= new IntPtr( 1 );
		internal static readonly IntPtr HWND_TOPMOST	= new IntPtr( -1 );
		internal static readonly IntPtr HWND_NOTOPMOST	= new IntPtr( -2 );

		internal const int SWP_NOSIZE         = 0x0001;
		internal const int SWP_NOMOVE         = 0x0002;
		internal const int SWP_NOSENDCHANGING = 0x0400;

		// AS 9/11/06 BR15726
		internal const int SWP_FRAMECHANGED = 0x20;
		internal const int SWP_NOOWNERZORDER = 0x200;
		internal const int SWP_NOZORDER = 0x4;

		// AS 6/22/11 TFS72532/TFS42892/TFS37239
		internal const int SWP_HIDEWINDOW = 0x80;
		internal const int SWP_DEFERERASE = 0x2000;
		internal const int SWP_NOCOPYBITS = 0x100;

			#endregion ShowWindow Constants

			#region Special Bitmap Constants

		internal const int HBMMENU_POPUP_CLOSE   		=	 8;
		internal const int HBMMENU_POPUP_RESTORE 		=	 9;
		internal const int HBMMENU_POPUP_MAXIMIZE		=	10;
		internal const int HBMMENU_POPUP_MINIMIZE		=	11;

			#endregion Special Bitmap Constants

			#region SysCommand Constants
	
		internal const int SC_MINIMIZE		=	0xF020;
		internal const int SC_MAXIMIZE		=	0xF030;
		internal const int SC_NEXTWINDOW	=	0xF040;
		internal const int SC_CLOSE   		=	0xF060;
		internal const int SC_RESTORE 		=	0xF120;

		// AS 8/31/06 Office 2007 Ribbon
		internal const int SC_KEYMENU		=	0xF100;
		internal const int SC_MOUSEMENU		=	0xF090;


			#endregion SysCommand Constants

			// AS 9/18/09 TFS19281
			#region SystemMetrics (SM) constants

		private const int SM_CMONITORS = 80;
		private const int SM_CXSCREEN = 0;
		private const int SM_CYSCREEN = 1; 

			#endregion //SystemMetrics (SM) constants

			// MD 9/26/06 - Office 2007 - Keyboard Access
			#region UpdateLayeredWindow Constants

		internal const int ULW_COLORKEY = 0x00000001;
		internal const int ULW_ALPHA = 0x00000002;
		internal const int ULW_OPAQUE = 0x00000004;

			#endregion UpdateLayeredWindow Constants

			#region Virtual Key Constants

		internal const int VK_TAB			=	0x09;
		internal const int VK_SHIFT			=	0x010;
		internal const int VK_CONTROL		=	0x011;
		internal const int VK_MENU			=	0x012;
		internal const int VK_ESCAPE		=	0x01B;
		internal const int VK_ENTER			=	0x0D;
		internal const int VK_LEFT  		=	0x025;
		internal const int VK_UP    		=	0x026;
		internal const int VK_RIGHT 		=	0x027;
		internal const int VK_DOWN  		=	0x028;

		// MD 8/2/06 - PopupGalleryTool
		// Added new key vallues for naviagtion
		internal const int VK_PRIOR			=   0x021;
		internal const int VK_NEXT			=   0x022;       
		internal const int VK_END           =   0x023;
		internal const int VK_HOME          =   0x024;

			#endregion Virtual Key Constants

			#region ChildWindowFromPointEx constants

		// AS 9/7/06
		// In VS2005, using RealChildWindowFromPoint is returning the overlay window instead of 
		// the form so we'll use ChildWindowFromPointEx instead.
		// 
		private const int CWP_ALL = 0x0000;
		private const int CWP_SKIPINVISIBLE = 0x0001;
		private const int CWP_SKIPDISABLED = 0x0002;
		private const int CWP_SKIPTRANSPARENT = 0x0004;

			#endregion //ChildWindowFromPointEx constants

            #region SystemParametersInfo constants

            // MRS 9/8/06
            internal const uint SPI_GETNONCLIENTMETRICS = 0x0029;

            #endregion //SystemParametersInfo constants

            #region GetSysColor Constants
            
            // MRS 9/8/06
            internal const int COLOR_GRADIENTACTIVECAPTION = 27;
            internal const int COLOR_GRADIENTINACTIVECAPTION = 28;

            #endregion GetSysColor Constants

        #endregion Constants

        #region Structures

			// MD 9/26/06 - Office 2007 - Keyboard Access
			#region BLENDFUNCTION
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		internal struct BLENDFUNCTION
		{
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;
		}
			#endregion //BLENDFUNCTION

			#region CWPSTRUCT structure

        [StructLayout(LayoutKind.Sequential)]
		internal struct CWPSTRUCT 
		{ 
			internal IntPtr lparam; 
			internal IntPtr wparam; 
			internal int    message; 
			internal IntPtr hwnd; 
		}

			#endregion CWPSTRUCT structure

			// JJD 8/14/06 - NA 2006 vol 3
			// Added support for merging the ribbon into the caption area
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

			#region MENUITEMINFO

		[StructLayout(LayoutKind.Sequential)]
			internal struct MENUITEMINFO 
		{ 
			internal uint		cbSize; 
			internal uint		fMask; 
			internal uint		fType; 
			internal uint		fState; 
			internal uint		wID; 
			internal IntPtr   hSubMenu; 
			internal IntPtr	hbmpChecked; 
			internal IntPtr	hbmpUnchecked; 
			internal IntPtr	dwItemData; 
			internal IntPtr	dwTypeData; 
			internal uint		cch; 
			internal IntPtr	hbmpItem;
		}

			#endregion MENUITEMINFO

			#region MOUSEHOOKSTRUCT structure

		[StructLayout(LayoutKind.Sequential)]
		internal struct MOUSEHOOKSTRUCT 
		{ 
			internal POINT  pt; 
			internal IntPtr hwnd; 
			internal int    hitTestCode; 
			internal IntPtr extraInfo; 
		}

			#endregion MOUSEHOOKSTRUCT structure

			#region MSG structure

		[StructLayout(LayoutKind.Sequential)]
		internal struct MSG 
		{ 
			internal IntPtr hwnd; 
			internal int    message; 
			internal IntPtr wparam; 
			internal IntPtr lparam; 
			internal int    time; 
			internal POINT  pt; 
		}

			#endregion MSG structure

			#region POINT structure

        [ StructLayout( LayoutKind.Sequential )]
        internal struct POINT 
		{
            internal int X;
            internal int Y;

            internal POINT(int x, int y) 
			{
                this.X = x;
                this.Y = y;
            }

			internal Point Point { get { return new Point( this.X, this.Y ); } }

            internal static POINT FromPoint( Point point ) 
			{
                return new POINT( point.X,
								  point.Y );
            }
        }

			#endregion POINT structure
			
			#region RECT structure

        [ StructLayout( LayoutKind.Sequential )]
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
			
			internal Rectangle Rect 
            {
				// AS 9/18/09 TFS19281
                //get { return new Rectangle( this.left, this.top, this.right - this.left, this.bottom - this.top ); } 
				get { return Rectangle.FromLTRB(this.left, this.top, this.right, this.bottom); } 
			}

			// AS 9/18/09 TFS19281
			internal static RECT Intersect(RECT rect1, RECT rect2)
			{
				Rectangle i = Rectangle.Intersect(rect1.Rect, rect2.Rect);
				return new RECT(i.X, i.Y, i.Right, i.Bottom);
			}

			// AS 9/18/09 TFS19281
			internal int Height
			{
				get { return this.bottom - this.top; }
			}

			// AS 9/18/09 TFS19281
			internal int Width
			{
				get { return this.right - this.left; }
			}

			// MD 1/10/07 - Optimization
			// Not used
			//internal static RECT FromXYWH(int x, int y, int width, int height) 
			//{
			//    return new RECT(x,
			//                    y,
			//                    x + width,
			//                    y + height);
			//}
			//
			//internal static RECT FromRectangle( Rectangle rect ) 
			//{
			//    return new RECT( rect.Left,
			//                     rect.Top,
			//                     rect.Right,
			//                     rect.Bottom );
			//}
        }

			#endregion RECT structure

			// MD 9/26/06 - Office 2007 - Keyboard Access
			#region SIZE
		[StructLayout( LayoutKind.Sequential )]
		internal struct SIZE
		{
			public int width;
			public int height;

			internal static SIZE FromSize( Size size )
			{
				SIZE s = new SIZE();
				s.width = size.Width;
				s.height = size.Height;
				return s;
			}
		}
			#endregion //SIZE

			// JJD 8/15/06 - NA 2006 vol 3
			// Added support for merging the ribbon into the caption area
			#region STYLESTRUCT structure

		[ StructLayout( LayoutKind.Sequential )]
        internal struct STYLESTRUCT 
		{
            internal int styleOld;
            internal int styleNew;
        }

			#endregion STYLESTRUCT structure

			// JJD 8/14/06 - NA 2006 vol 3
			// Added support for merging the ribbon into the caption area
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
			internal uint flags;
		}
			#endregion //WINDOWPOS

			// AS 1/20/03 DNF33
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

			// MRS 9/8/06
			#region LOGFONT structure
			
			//	BF 4.9.02
			//	The Font object's GDICharSet property does not provide
			//	reliable information, so we need to use the ToLogFont method,
			//	which takes a LOGFONT
			//
			// JJD 8/30/06
			// Added CharSet=CharSet.Auto attribute
			//[ StructLayout( LayoutKind.Sequential )]
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			// Must be a class not a structure to get the facename to
			// marshal correctly.
			internal class LOGFONT
			{
			    public int lfHeight;
			    public int lfWidth;
			    public int lfEscapement;
			    public int lfOrientation;
			    public int lfWeight;
			    public byte lfItalic;
			    public byte lfUnderline;
			    public byte lfStrikeOut;
			    public byte lfCharSet;
			    public byte lfOutPrecision;
			    public byte lfClipPrecision;
			    public byte lfQuality;
			    public byte lfPitchAndFamily;
			
			    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			    public String lfFaceName;
			
			    public LOGFONT(bool dummy)
			    {
			        lfHeight = 0;
			        lfWidth = 0;
			        lfEscapement = 0;
			        lfOrientation = 0;
			        lfWeight = 0;
			        lfItalic = 0;
			        lfUnderline = 0;
			        lfStrikeOut = 0;
			        lfCharSet = 0;
			        lfOutPrecision = 0;
			        lfClipPrecision = 0;
			        lfQuality = 0;
			        lfPitchAndFamily = 0;
			        // JJD 8/30/06
			        // Changed initialization to an empty string
			        //lfFaceName = new String( ' ', 32 );
			        lfFaceName = string.Empty;
			    }
			
			}
			
			#endregion LOGFONT structure

			// MRS 9/8/06
			#region NONCLIENTMETRICS
			private struct NONCLIENTMETRICS
			{
			    public int cbSize;
			    public int iBorderWidth;
			    public int iScrollWidth;
			    public int iScrollHeight;
			    public int iCaptionWidth;
			    public int iCaptionHeight;
			    public LOGFONT lfCaptionFont;
			    public int iSMCaptionWidth;
			    public int iSMCaptionHeight;
			    public LOGFONT lfSMCaptionFont;
			    public int iMenuWidth;
			    public int iMenuHeight;
			    public LOGFONT lfMenuFont;
			    public LOGFONT lfStatusFont;
			    public LOGFONT lfMessageFont;
			} 
			#endregion //NONCLIENTMETRICS

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

            // MBS 10/27/06 - Glass/Vista
            #region MARGINS
            internal struct MARGINS
            {
                public int Left, Right, Top, Bottom;
                public MARGINS(int left, int right, int top, int bottom)
                {
                    this.Left = left;
                    this.Right = right;
                    this.Top = top;
                    this.Bottom = bottom;
                }
            }
            #endregion //MARGINS

			// AS 11/27/06
			#region PAINTSTRUCT
			[StructLayout(LayoutKind.Sequential)]
			internal struct PAINTSTRUCT
			{
				public IntPtr hdc;
				public bool fErase;
				public RECT rcPaint;
				public bool fRestore;
				public bool fIncUpdate;
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
				public byte[] rgbReserved;
			}
			#endregion //PAINTSTRUCT

            #region TITLEBARINFOEX
        [StructLayout(LayoutKind.Sequential)]
        internal struct TITLEBARINFOEX
        {
            public const int CCHILDREN_TITLEBAR = 5;
            public uint cbSize;
            public RECT rcTitleBar;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CCHILDREN_TITLEBAR + 1)]
            public uint[] rgstate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CCHILDREN_TITLEBAR + 1)]
            public RECT[] rgrect;
        }
        #endregion //TITLEBARINFOEX

			#region TPMPARAMS
		[StructLayout(LayoutKind.Sequential)]
		internal struct TPMPARAMS
		{
			public uint cbSize;     
			public RECT rcExclude;  
		} 
			#endregion //TPMPARAMS

            // AS 10/10/08 TFS6236
			#region WINDOWINFO
		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWINFO
		{
			public int	cbSize;
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

			// AS 9/18/09 TFS19281
			#region MONITIORINFOEX

			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
			internal class MONITIORINFOEX
			{
				internal int _size;
				internal RECT _rcMonitor;
				internal RECT _rcWork;
				internal int _flags;
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
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

        #endregion Structures

        #region Enums

        // MBS 10/26/06 - Glass/Vista
        #region DWMWINDOWATTRIBUTE
        internal enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,      // [get] Is non-client rendering enabled/disabled
            DWMWA_NCRENDERING_POLICY,           // [set] Non-client rendering policy
            DWMWA_TRANSITIONS_FORCEDISABLED,    // [set] Potentially enable/forcibly disable transitions
            DWMWA_ALLOW_NCPAINT,                // [set] Allow contents rendered in the non-client area to be visible on the DWM-drawn frame.
            DWMWA_CAPTION_BUTTON_BOUNDS,        // [get] Bounds of the caption button area in window-relative space.
            DWMWA_NONCLIENT_RTL_LAYOUT,         // [set] Is non-client content RTL mirrored
            DWMWA_FORCE_ICONIC_REPRESENTATION,  // [set] Force this window to display iconic thumbnails.
            DWMWA_FLIP3D_POLICY,                // [set] Designates how Flip3D will treat the window.
            DWMWA_EXTENDED_FRAME_BOUNDS,        // [get] Gets the extended frame bounds rectangle in screen space
            DWMWA_LAST
        };
        #endregion //DWMWINDOWATTRIBUTE

        #region RegionCombineMode

        internal enum RegionCombineMode
        {
            RGN_AND = 1,
            RGN_OR = 2,
            RGN_XOR = 3,
            RGN_DIFF = 4,
            RGN_COPY = 5,
            RGN_MIN = RGN_AND,
            RGN_MAX = RGN_COPY
        }
        #endregion //RegionCombineMode

        #region TPM_Flags
        internal enum TPM_Flags : uint
		{
			LEFTBUTTON = 0x0000,
			RIGHTBUTTON = 0x0002,
			LEFTALIGN = 0x0000,
			CENTERALIGN = 0x0004,
			RIGHTALIGN = 0x0008,
			//#if(WINVER >= 0x0400)
			TOPALIGN = 0x0000,
			VCENTERALIGN = 0x0010,
			BOTTOMALIGN = 0x0020,

			HORIZONTAL = 0x0000,     
			VERTICAL = 0x0040,     
			NONOTIFY = 0x0080,     
			RETURNCMD = 0x0100,
			//#if(WINVER >= 0x0500)
			RECURSE = 0x0001,
			HORPOSANIMATION = 0x0400,
			HORNEGANIMATION = 0x0800,
			VERPOSANIMATION = 0x1000,
			VERNEGANIMATION = 0x2000,
			//#if(_WIN32_WINNT >= 0x0500)
			NOANIMATION = 0x4000,
			//#if(_WIN32_WINNT >= 0x0501)
			LAYOUTRTL = 0x8000,
		} 
		#endregion //TPM_Flags

        #endregion // Enums

        #region Delegates

        #region HookProc

        // MD 10/31/06 - 64-Bit Support
		//internal delegate int HookProc(int ncode, IntPtr wparam, IntPtr lparam);
		internal delegate IntPtr HookProc( int ncode, IntPtr wparam, IntPtr lparam );

			#endregion HookProc

		#endregion Delegates

		// JJD 1/15/03
		// Added safe static methods for each api so that the calling methods
		// could be jitted without appropriate access rights

		#region Apis

			#region CallNextHookEx

		// MD 10/31/06 - 64-Bit Support
		//internal static int CallNextHookExApi(IntPtr hook, int nCode, IntPtr wparam,  IntPtr lparam)
		internal static IntPtr CallNextHookExApi( IntPtr hook, int nCode, IntPtr wparam, IntPtr lparam )
		{
			return CallNextHookEx(hook, nCode, wparam,  lparam);
		}
		
		// JJD 5/03/02 added hook apis
		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//private static extern int CallNextHookEx(IntPtr hook, int nCode, IntPtr wparam,  IntPtr lparam);
		private static extern IntPtr CallNextHookEx( IntPtr hook, int nCode, IntPtr wparam, IntPtr lparam );

			#endregion CallNextHookEx

			#region CombineRgn

        [DllImport("gdi32")]
        private static extern int CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, RegionCombineMode combineMode);

        internal static int CombineRgnApi(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, RegionCombineMode combineMode)
        {
            return CombineRgn(hrgnDest, hrgnSrc1, hrgnSrc2, combineMode);
        }
			#endregion //CombineRgn

			// MD 9/26/06 - Office 2007 - Keyboard Access
			#region CreateCompatibleDC

		[DllImport( "gdi32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto )]
		internal static extern IntPtr CreateCompatibleDC( IntPtr hDC );

			#endregion CreateCompatibleDC

			#region CreateRectRgn

        [DllImport("gdi32")]
        private static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        internal static IntPtr CreateRectRegionApi(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
        {
            return CreateRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect);
        }
			#endregion //CreateRectRgn

			#region CreateRoundRectRgn

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

        internal static IntPtr CreateRoundRectRgnApi(int x1, int y1, int x2, int y2, int cx, int cy)
        {
            return CreateRoundRectRgn(x1, y1, x2, y2, cx, cy);
        }
			#endregion //CreateRoundRectRgn

            // MD 9/26/06 - Office 2007 - Keyboard Access
			#region DeleteObject

		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport( "gdi32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto )]
		internal static extern bool DeleteObject( IntPtr hObject );

			#endregion DeleteObject

			#region DestroyMenu

		internal static bool DestroyMenuApi(IntPtr menu)
		{
			return DestroyMenu( menu );
		}

		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool DestroyMenu(IntPtr menu);

			#endregion DestroyMenu

			// MD 9/26/06 - Office 2007 - Keyboard Access
			#region GetDC
		[DllImport( "user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto )]
		internal static extern IntPtr GetDC( IntPtr hwnd );
			#endregion //GetDC

			#region Commented out
			
		// MD 1/10/07 - Optimization
		// Not used
		//    // JJD 8/15/06 - NA 2006 vol 3
		//    // Added support for merging the ribbon into the caption area
		//    #region GetDCEx
		//
		//internal static IntPtr GetDCExApi(IntPtr hwnd, IntPtr hrgnclip, uint fdwOptions)
		//{
		//    return GetDCEx(hwnd, hrgnclip, fdwOptions);
		//}
		//
		//[DllImport("user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		//private extern static IntPtr GetDCEx(IntPtr hwnd, IntPtr hrgnclip, uint fdwOptions);
		//
		//    #endregion //GetDCEx
		
			#endregion Commented out

			#region GetMenu

		internal static IntPtr GetMenuApi(IntPtr hwnd)
		{
			return GetMenu( hwnd );
		}

		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern IntPtr GetMenu(IntPtr hwnd);

			#endregion GetMenu

			#region GetMenuItemCount
 
		internal static int GetMenuItemCountApi(IntPtr hMenu)
		{
			return GetMenuItemCount( hMenu );
		}

		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern int GetMenuItemCount(IntPtr hMenu);

			#endregion GetMenuItemCount

			#region GetMenuItemInfo 

		internal static bool GetMenuItemInfoApi(IntPtr hMenu, uint uItem, bool byPosition, ref MENUITEMINFO mii)
		{
			return GetMenuItemInfo( hMenu, uItem, byPosition, ref mii);
		}

		// MD 10/31/06 - 64-Bit Support
		//[DllImport("user32")]
		//private static extern bool GetMenuItemInfo(IntPtr hMenu, uint uItem, bool byPosition, ref MENUITEMINFO mii);
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport( "user32" )]
		private static extern bool GetMenuItemInfo( IntPtr hMenu, uint uItem, [MarshalAs( UnmanagedType.Bool )] bool byPosition, ref MENUITEMINFO mii );

			#endregion GetMenuItemInfo

			#region GetMenuString

		internal static int GetMenuStringApi(IntPtr hMenu, uint uIDItem, IntPtr lpString, int nMaxCount, int uFlag)
		{
			return GetMenuString( hMenu, uIDItem, lpString, nMaxCount, uFlag);
		}

		[DllImport("user32")]
		private static extern int GetMenuString(IntPtr hMenu, uint uIDItem, IntPtr lpString, int nMaxCount, int uFlag);

			#endregion GetMenuString

			// AS 11/4/11 TFS91009
			#region GetMenuState

		[DllImport("user32.dll")]
		internal static extern int GetMenuState(IntPtr hMenu, int uID, int uFlags);

			#endregion //GetMenuState

			#region GetParent

		internal static IntPtr GetParentApi(IntPtr hwnd)
		{
			return GetParent( hwnd );
		}

		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern IntPtr GetParent(IntPtr hwnd);

			#endregion GetParent

			// JM 03-12-03 WTB736
			#region GetWindowLong

		// MD 10/31/06 - 64-Bit Support
		//internal static int GetWindowLongApi(IntPtr hwnd, int nIndex)
		//{
		//    return GetWindowLong(hwnd, nIndex);
		//}
		//
		//[DllImport("user32", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		//private static extern int GetWindowLong(IntPtr hwnd, int nIndex);
		[DllImport( "user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto )]
		public static extern IntPtr GetWindowLong32( IntPtr hWnd, int nIndex );

		[DllImport( "user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto )]
		public static extern IntPtr GetWindowLongPtr64( IntPtr hWnd, int nIndex );

		internal static IntPtr GetWindowLong( IntPtr hwnd, int nIndex )
		{
			if ( IntPtr.Size == PtrSizeOn32Bit )
				return GetWindowLong32( hwnd, nIndex );
			else
				return GetWindowLongPtr64( hwnd, nIndex );
		}

			#endregion GetWindowLong

			#region GetSystemMenu

		internal static IntPtr GetSystemMenuApi(IntPtr hWnd, bool revert)
		{
			return GetSystemMenu( hWnd, revert);
		}

		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool revert);
		private static extern IntPtr GetSystemMenu( IntPtr hWnd, [MarshalAs( UnmanagedType.Bool )]bool revert );

			#endregion GetSystemMenu

			#region Commented out

		// MD 1/10/07 - Optimization
		// Not used
		//    // JJD 1/30/04 - WTB1310
		//    #region MapVirtualKey
		//
		//internal static char MapVirtualKeyApi(char vkey)
		//{
		//    return (char)MapVirtualKey( vkey, 2 );
		//}
		//
		//[DllImport("user32", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		//private static extern System.UInt32 MapVirtualKey( System.UInt32 code, System.UInt32 mapType );
		//
		//    #endregion MapVirtualKey

			#endregion Commented out

			#region PostMessage

		// MD 10/31/06 - 64-Bit Support
		//internal static void PostMessageApi(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		internal static int PostMessageApi( HandleRef hWnd, int msg, IntPtr wparam, IntPtr lparam )
		{
			// MD 10/31/06 - 64-Bit Support
			//PostMessage( hWnd, msg, wparam, lparam); 
			return PostMessage( hWnd, msg, wparam, lparam ); 
		}

		[DllImport("user32", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//private static extern void PostMessage(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam);
		private static extern int PostMessage( HandleRef hWnd, int msg, IntPtr wparam, IntPtr lparam );

			#endregion PostMessage

			#region RedrawWindow

		internal static bool RedrawWindowApi(IntPtr hWnd, IntPtr rectUpdate, IntPtr hrgnUpdate, uint flags)
		{
			return RedrawWindow( hWnd, rectUpdate, hrgnUpdate, flags);
		}

		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool RedrawWindow(IntPtr hWnd, IntPtr rectUpdate, IntPtr hrgnUpdate, uint flags);

			#endregion RedrawWindow

			// JJD 8/15/06 - NA 2006 vol 3
			// Added support for merging the ribbon into the caption area
			#region ReleaseDC
		internal static int ReleaseDCApi(IntPtr hwnd, IntPtr hdc)
		{
			return ReleaseDC(hwnd, hdc);
		}

		[DllImport("user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

			#endregion //ReleaseDC

			// MD 9/26/06 - Office 2007 - Keyboard Access
			#region SelectObject

		[DllImport( "gdi32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto )]
		internal static extern IntPtr SelectObject( IntPtr hDC, IntPtr hObject );

			#endregion SelectObject

			#region SendMessage

		// MD 10/31/06 - 64-Bit Support
		//internal static int SendMessageApi(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		internal static IntPtr SendMessageApi( IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam )
		{
			return SendMessage( hWnd, msg, wparam, lparam);
		}

        internal static IntPtr SendMessageApi(IntPtr hWnd, int msg, IntPtr wparam, ref TITLEBARINFOEX lparam)
        {
            return SendMessage(hWnd, msg, wparam, ref lparam);
        }

		[DllImport("user32", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam);
		private static extern IntPtr SendMessage( IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam );

        [DllImport("user32", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wparam, ref TITLEBARINFOEX lparam);

			#endregion SendMessage

			// MD 12/18/06 - FxCop
			#region SetCapture

		internal static IntPtr SetCaptureApi( IntPtr hwnd )
		{
			return SetCapture( hwnd );
		}

		[DllImport( "user32" )]
		internal static extern IntPtr SetCapture( IntPtr hwnd );

			#endregion SetCapture

			// MD 12/18/06 - FxCop
			#region SetCursorPos

		internal static bool SetCursorPosApi( Point screenPosition )
		{
			return SetCursorPos( screenPosition.X, screenPosition.Y );
		}

		[DllImport( "user32" )]
		internal static extern bool SetCursorPos( int x, int y );

			#endregion SetCursorPos

			// MD 12/18/06 - FxCop
			#region SetFocus

		internal static IntPtr SetFocusApi( IntPtr hWnd )
		{
			return SetFocus( hWnd );
		}

		[DllImport( "user32" )]
		private static extern IntPtr SetFocus( IntPtr hWnd );

			#endregion SetFocus

			// MD 12/18/06 - FxCop
			#region SetForegroundWindow

		internal static bool SetForegroundWindowApi( IntPtr hWnd )
		{
			return SetForegroundWindow( hWnd );
		}

		[DllImport( "user32" )]
		internal static extern bool SetForegroundWindow( IntPtr hWnd );

			#endregion SetForegroundWindow

			#region SetMenu

		internal static bool SetMenuApi(IntPtr hwnd, IntPtr menu)
		{
			return SetMenu( hwnd, menu );
		}

		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool SetMenu(IntPtr hwnd, IntPtr menu);

			#endregion SetMenu

			#region SetWindowsHookEx

		// MD 10/31/06 - 64-Bit Support
		//internal static IntPtr SetWindowsHookExApi(int id, HookProc hookProc, int hmod, int dwThreadId)
		internal static IntPtr SetWindowsHookExApi( int id, HookProc hookProc, IntPtr hmod, int dwThreadId )
		{
			return SetWindowsHookEx( id, hookProc, hmod, dwThreadId);
		}

		// JJD 5/03/02 added hook apis
		[DllImport("user32", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//private static extern IntPtr SetWindowsHookEx(int id, HookProc hookProc, int hmod, int dwThreadId);
		private static extern IntPtr SetWindowsHookEx( int id, HookProc hookProc, IntPtr hmod, int dwThreadId );

			#endregion SetWindowsHookEx

			// JM 03-12-03 WTB736
			#region SetWindowLong

		// MD 10/31/06 - 64-Bit Support
		//internal static int SetWindowLongApi(IntPtr hwnd, int nIndex, int dwNewLong)
		//{
		//    return SetWindowLong(hwnd, nIndex, dwNewLong);
		//}
		//
		//[DllImport("user32", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		//private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);
		[DllImport( "user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto )]
		private static extern IntPtr SetWindowLong32( IntPtr hWnd, int nIndex, IntPtr dwNewLong );

		[DllImport( "user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto )]
		private static extern IntPtr SetWindowLongPtr64( IntPtr hWnd, int nIndex, IntPtr dwNewLong );


		internal static IntPtr SetWindowLong( IntPtr hwnd, int nIndex, IntPtr dwNewLong )
		{
			if ( IntPtr.Size == PtrSizeOn32Bit )
				return SetWindowLong32( hwnd, nIndex, dwNewLong );
			else
				return SetWindowLongPtr64( hwnd, nIndex, dwNewLong );
		}

			#endregion SetWindowLong

			#region SetWindowPos

		// MD 10/31/06 - 64-Bit Support
		//internal static bool SetWindowPosApi( int hWnd, int hWndInsertAfter,
		//    int x, int y, int cx, int cy, int flags)
		internal static bool SetWindowPosApi( IntPtr hWnd, IntPtr hWndInsertAfter,
			int x, int y, int cx, int cy, int flags )
		{
			return SetWindowPos( hWnd, hWndInsertAfter, x, y, cx, cy, flags);
		}

		// MD 10/31/06 - 64-Bit Support
		//[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		//private static extern bool SetWindowPos( int hWnd, int hWndInsertAfter,
		//    int x, int y, int cx, int cy, int flags);
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport( "user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto )]
		private static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter,
			int x, int y, int cx, int cy, int flags );

			#endregion SetWindowPos

			#region SetWindowRgn

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        internal static int SetWindowRgnApi(IntPtr hWnd, IntPtr hRgn, bool bRedraw)
        {
            return SetWindowRgn(hWnd, hRgn, bRedraw);
        }
			#endregion //SetWindowRgn

			#region ShowWindow

        // MD 10/31/06 - 64-Bit Support
		//internal static bool ShowWindowApi( int hWnd, int nCmdShow )
		internal static bool ShowWindowApi( IntPtr hWnd, int nCmdShow )
		{
			return ShowWindow( hWnd, nCmdShow );
		}

		// MD 10/31/06 - 64-Bit Support
		//[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		//private static extern bool ShowWindow( int hWnd, int nCmdShow );
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport( "user32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto )]
		private static extern bool ShowWindow( IntPtr hWnd, int nCmdShow );

			#endregion ShowWindow

			#region TrackPopupMenuEx

		[DllImport("user32.dll")]
		static extern int TrackPopupMenuEx(IntPtr hmenu, TPM_Flags fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

		[DllImport("user32.dll")]
		internal static extern int TrackPopupMenuEx(IntPtr hmenu, TPM_Flags fuFlags, int x, int y, IntPtr hwnd, ref TPMPARAMS lptpm);

			#endregion //TrackPopupMenuEx

			// MD 9/26/06 - Office 2007 - Keyboard Access
			#region UpdateLayeredWindow
		[DllImport( "User32", CharSet = CharSet.Auto )]
		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		internal static extern bool UpdateLayeredWindow( IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags );
			#endregion //UpdateLayeredWindow

			#region UnhookWindowsHookEx
		
		internal static bool UnhookWindowsHookExApi(IntPtr hook)
		{
			return UnhookWindowsHookEx(hook);
		}

		// JJD 5/03/02 added hook apis
		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern bool UnhookWindowsHookEx(IntPtr hook);

			#endregion UnhookWindowsHookEx

			#region WindowFromPoint

		internal static IntPtr WindowFromPointApi(Point point)
		{
			// AS 7/27/04
			// WindowFromPoint does not consider disabled windows but we actually need to
			// consider them - e.g. when the customize dialog is displayed modally.
			//
			//return WindowFromPoint(point);
			// MD 10/31/06 - 64-Bit Support
			//IntPtr hwnd = WindowFromPoint( point );
			IntPtr hwnd = WindowFromPoint( POINT.FromPoint( point ) );

			hwnd = GetRealWindowFromPoint(hwnd, point);


			return hwnd;
		}

		[DllImport("user32", ExactSpelling=true, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		// MD 10/31/06 - 64-Bit Support
		//private static extern IntPtr WindowFromPoint(Point point);
		private static extern IntPtr WindowFromPoint( POINT point );

			#endregion WindowFromPoint

			// AS 7/27/04
			// WindowFromPoint does not consider disabled windows but we actually need to
			// consider them when the customize dialog is displayed modally.
			//
			#region GetRealWindowFromPoint



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private static IntPtr GetRealWindowFromPoint(IntPtr hwndParent, Point screenPt)
		{
			// convert the coordinates to client coordinates
			Point clientPt = NativeWindowMethods.PointToClientHelper(hwndParent, screenPt);

			// use the api to determine the actual child at that point
			// AS 9/7/06
			// In VS2005, using RealChildWindowFromPoint is returning the overlay window instead of 
			// the form so we'll use ChildWindowFromPointEx instead.
			// 
			//IntPtr childHwnd = NativeWindowMethods.RealChildWindowFromPoint(hwndParent, clientPt.X, clientPt.Y);
			IntPtr childHwnd = NativeWindowMethods.ChildWindowFromPointEx(hwndParent, clientPt.X, clientPt.Y, CWP_SKIPINVISIBLE | CWP_SKIPTRANSPARENT);

			// if there is none, then return the parent
			if (childHwnd == IntPtr.Zero || childHwnd == hwndParent)
			    return hwndParent;

			// otherwise see if that window has any children at the specified screen location
			return GetRealWindowFromPoint(childHwnd, screenPt);
		}

		// MD 10/31/06
		// This didnt seem to be used anywhere
		//[DllImport("user32")]
		//private static extern IntPtr RealChildWindowFromPoint(IntPtr hwnd, int parentX, int parentY);

		// AS 9/7/06
		// In VS2005, using RealChildWindowFromPoint is returning the overlay window instead of 
		// the form so we'll use ChildWindowFromPointEx instead.
		// 
		[DllImport("user32")]
		private static extern IntPtr ChildWindowFromPointEx(IntPtr hwnd, int parentX, int parentY, int flags);

			#endregion //GetRealWindowFromPoint

			#region PointToClientHelper


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private static Point PointToClientHelper(IntPtr hwnd, Point pt)
		{
			POINT newPt = new POINT();
			newPt.X = pt.X;
			newPt.Y = pt.Y;
			MapWindowPoints( IntPtr.Zero, hwnd, ref newPt, 1 );

			return new Point(newPt.X, newPt.Y);
		}

		[DllImport("user32")]
		// MD 10/31/06 - 64-Bit Support
		//private static extern IntPtr MapWindowPoints(IntPtr hwndSrc, IntPtr hwndDest, [In,Out] ref POINT pt, int ptCount );
		private static extern int MapWindowPoints( IntPtr hwndSrc, IntPtr hwndDest, [In, Out] ref POINT pt, int ptCount );

			#endregion //PointToClientHelper

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
				// MD 12/15/06 - FxCop
				// This is not needed due to the SuppressUnmanagedCodeSecurityAttribute on NativeWindowMethods.
				//SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
				//
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

			// AS 9/8/05 BR04828
			// We needed to switch from using MapVirtualKey to using ToAscii
			// so we could take the shift key state into account when 
			// attempting to process a mnemonic based on a wm_keydown msg.
			//
			#region VKeyToAscii

			#region VKeyToAscii
		internal static char VKeyToAscii(IntPtr wParam)
		{
			byte[] keyState = new byte[256];
			byte scanCode = 0;
			System.Text.StringBuilder sb = new System.Text.StringBuilder(2);

			GetKeyboardState(keyState);

			int ok = ToAscii(wParam.ToInt32(), scanCode, keyState, sb, 0);

			if (ok > 0 && sb.Length > 0)
				return (char)sb[0];
			else
				return '\0';
		}
			#endregion //VKeyToAscii

			#region GetKeyboardState

		[DllImport("user32.dll")]
		private static extern int GetKeyboardState( 
			[MarshalAs(UnmanagedType.LPArray, SizeConst=256, ArraySubType=UnmanagedType.I1) ]
			[In,Out] byte[] lpKeyState );

			#endregion //GetKeyboardState

			#region ToAscii

		// MD 12/14/06 - FxCop



		[DllImport("user32.dll")]

		private static extern int ToAscii(int uVirtKey, int uScanCode, 
			byte [] lpKeyState, [Out] System.Text.StringBuilder lpChar, 
			uint uFlags);

			#endregion //ToAscii

			#endregion //VKeyToAscii

			// AS 12/20/05
			#region GetFocus

		// MD 6/11/07 - BR23708
		internal static IntPtr GetFocusApi()
		{
			return GetFocus();
		}

		[DllImport("user32")]
		internal static extern IntPtr GetFocus();
			#endregion //GetFocus

		    // MBS 4/11/06 CLR1 FontSmoothing Check
		    #region SystemParametersInfo
		internal static bool SystemParametersInfoApi(uint uiAction, uint uiParam, ref uint pvParam, uint fWinIni) 
		{
		    return SystemParametersInfo(uiAction, uiParam, ref pvParam, fWinIni);
		}
		
		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		  [DllImport("user32.dll")]
		  private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref uint pvParam, uint fWinIni);

		  // MRS 9/8/06
		private static bool SystemParametersInfoApi(uint uiAction, uint uiParam, ref NONCLIENTMETRICS nonClientMetrics, uint fWinIni)
		{
		    return SystemParametersInfo(uiAction, uiParam, ref nonClientMetrics, fWinIni);
		}
		
		  // MRS 9/8/06
		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		  [DllImport("user32.dll", SetLastError = true, CharSet=CharSet.Auto)]
		  private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref NONCLIENTMETRICS nonClientMetrics, uint fWinIni);

          #endregion // SystemParametersInfor

			// AS 8/14/06 Office 2007 Ribbon
		    #region GetMenuStringApi

		  internal static int GetMenuStringApi(IntPtr hMenu,
	  uint uIDItem,
	  ref System.Text.StringBuilder lpString,
	  int nMaxCount,
	  uint uFlag)
		  {
			  return GetMenuString(hMenu, uIDItem, lpString, nMaxCount, uFlag);
		  }

		  // MD 12/14/06 - FxCop



		  [DllImport("user32.dll")]

		  // MD 1/10/07 - Made private
		  //internal static extern int GetMenuString(IntPtr hMenu,
		  private static extern int GetMenuString( IntPtr hMenu,
			  uint uIDItem,
			  [Out, MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder lpString,
			  int nMaxCount,
			  uint uFlag);

		  #endregion //GetMenuStringApi    

			#region Commented out

			// MD 1/10/07 - Optimization
			// Not used
			//#region GetSystemMetrics
			//internal static int GetSystemMetricsApi(int smIndex)
			//{
			//    return GetSystemMetrics(smIndex);
			//}
			//
			//[DllImport("user32.dll")]
			//static extern int GetSystemMetrics(int smIndex);
			//#endregion GetSystemMetrics
		
			#endregion Commented out

			#region GetSysColor
			internal static int GetSysColorApi(int nIndex)
			{
			    return GetSysColor(nIndex);
			}
			
			[DllImport("user32.dll")]
			static extern int GetSysColor(int nIndex);
			#endregion GetSysColor

			// AS 11/7/06 BR17495
			#region GetWindowPlacement
			[return: MarshalAs(UnmanagedType.Bool)]
			[DllImport("user32.dll")]
			static extern internal bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl); 
			#endregion //GetWindowPlacement

            // MBS 10/26/06
            #region DwmIsCompositionEnabled
            [DllImport("dwmapi.dll")]
            private extern static int DwmIsCompositionEnabled(out bool enabled);
            #endregion //DwmIsCompositionEnabled

            // MBS 10/26/06
            #region DwmExtendFrameIntoClientArea
            [DllImport("dwmapi.dll")]
            private static extern int DwmExtendFrameIntoClientArea(System.IntPtr hWnd, ref MARGINS pMargins);
            #endregion //DwmExtendFrameIntoClientArea

            // MBS 10/26/06 - Glass/Vista
            #region DwmGetWindowAttribute
            [DllImport("dwmapi.dll")]
            private static extern int DwmGetWindowAttribute(
                IntPtr hwnd,
                uint dwAttribute,
                ref RECT pvAttribute,
                uint cbAttribute
            );
            #endregion //DwmGetWindowAttribute

            // MBS 10/26/06 - Glass/Vista
            #region DwmDefWindowProc
            [DllImport("dwmapi.dll")]
            internal static extern int DwmDefWindowProc(
                [In] IntPtr hWnd,
                uint msg,
                IntPtr wParam,
                IntPtr lParam,
                out int plResult
            );
            #endregion //DwmDefWindowProc

			// AS 11/27/06
			#region Begin/End Paint

		[DllImport("user32")]
		internal static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

		[DllImport("user32")]
		internal static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);
			#endregion //Begin/End Paint

			// MD 1/16/07 - BR19179
			#region AdjustWindowRectEx

		internal static bool AdjustWindowRectExApi( ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle )
		{
			return AdjustWindowRectEx( ref lpRect, dwStyle, bMenu, dwExStyle );
		}

		[DllImport( "user32.dll", CharSet = CharSet.Auto, ExactSpelling = true )]
		private static extern bool AdjustWindowRectEx( ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle );

			#endregion AdjustWindowRectEx

			#region GetCapture

		internal static IntPtr? GetCaptureApi()
		{
			try
			{
				return GetCapture();
			}
			catch
			{
				return null;
			}
		}

		[DllImport("user32")]
		private static extern IntPtr GetCapture();

			#endregion //GetCapture

			// AS 10/23/07 XamRibbonWindow IconResolved
			#region GetClassLongPtr
		internal static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size > 4)
				return GetClassLongPtr64(hWnd, nIndex);
			else
				return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
		}

		[DllImport("user32.dll", EntryPoint = "GetClassLong")]
		private static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
		private static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

			#endregion //GetClassLongPtr

			// AS 10/23/07 XamRibbonWindow IconResolved
			#region LoadImage
		internal static IntPtr LoadImageApi(IntPtr hinst, int lpszName, uint uType,
		   int cxDesired, int cyDesired, uint fuLoad)
		{
			return LoadImage(hinst, lpszName, uType, cxDesired, cyDesired, fuLoad);
		}

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern IntPtr LoadImage(IntPtr hinst, int lpszName, uint uType,
		   int cxDesired, int cyDesired, uint fuLoad);

			#endregion //LoadImage

            // AS 10/10/08 TFS6236
            #region GetClientRect

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetClientRect(IntPtr hWnd, ref RECT rect);

            #endregion //GetClientRect

            // AS 10/10/08 TFS6236
		    #region GetWindowInfo
		// MD 10/31/06 - 64-Bit Support
		[return: MarshalAs( UnmanagedType.Bool )]
		[DllImport("user32")]
		internal static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);
		    #endregion //GetWindowInfo

			// AS 9/18/09 TFS19281
			[DllImport("user32.dll", ExactSpelling = true)]
			private static extern IntPtr MonitorFromRect(ref RECT rect, int flags);

			// AS 9/18/09 TFS19281
			[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
			private static extern int GetSystemMetrics(int index);

			// AS 9/18/09 TFS19281
			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			private static extern bool GetMonitorInfo(HandleRef handle, [In, Out] MONITIORINFOEX monitorInfo);

			// AS 10/1/09 TFS22098
			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool IsWindowVisible(IntPtr hwnd);

			// AS 8/6/12 TFS118105
			[DllImport( "uxtheme.dll", SetLastError = true )]
			internal static extern int IsThemeActive();

			[DllImport( "uxtheme.dll", SetLastError = true )]
			internal static extern int IsAppThemed();

		#endregion Apis

        #region Helper Methods

		    // MRS 9/8/06
		    #region GetNonClientMetrics
		  private static NONCLIENTMETRICS GetNonClientMetrics()
		  {
		      NONCLIENTMETRICS metrics = new NONCLIENTMETRICS();
		      metrics.cbSize = Marshal.SizeOf(metrics);
		  
		      try
		      {
				  // MD 12/15/06 - FxCop
				  // This is not needed due to the SuppressUnmanagedCodeSecurityAttribute on NativeWindowMethods.
				  //
				  //// create a security permission for unmanaged code
				  //SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
		  
				  //// assert the permission
				  //sp.Assert();
		  
		          SystemParametersInfoApi(SPI_GETNONCLIENTMETRICS, Convert.ToUInt32(metrics.cbSize), ref metrics, 0);                  
		      }
		      catch { }
		  
		      return metrics;              
		  }
		  #endregion // GetNonClientMetrics

		    // MRS 9/8/06
		    #region GetCaptionFont
		  internal static Font GetCaptionFont()
		  {
		      Font font = null;
		  
		      NONCLIENTMETRICS metrics = GetNonClientMetrics();
		  
		      try
		      {
		          LOGFONT logFont = metrics.lfCaptionFont;
		          font = Font.FromLogFont(logFont);
		      }
		      catch { }
		  
		      return font;
		  }
		  #endregion // GetCaptionFont                   
			
			#region GetSystemColor
			internal static Color GetSystemColor(int nIndex)
			{
			    Color color = Color.Empty;
			
			    try
			    {
					// MD 12/15/06 - FxCop
					// This is not needed due to the SuppressUnmanagedCodeSecurityAttribute on NativeWindowMethods.
					//
					//// create a security permission for unmanaged code
					//SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
					//
					//// assert the permission
					//sp.Assert();
			
			        int colorInt = GetSysColorApi(nIndex);
			
			        color = ColorTranslator.FromWin32(colorInt);
			
			    }
			    catch { }                
			
			    return color;
			}
			#endregion GetSystemColor

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

            // MBS 11/21/06 - Glass Support
            #region DwmGetWindowAttributeApi

        internal static int DwmGetWindowAttributeApi(IntPtr hwnd, uint dwAttribute, ref RECT pvAttribute, uint cbAttribute)
        {
            if (System.Environment.OSVersion.Version.Major < 6)
                return -1;

			// MD 12/15/06 - FxCop
			// see comment on assert below
            //SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);

            try
            {
				// MD 12/15/06 - FxCop
				// This is not needed due to the SuppressUnmanagedCodeSecurityAttribute on NativeWindowMethods.
                //perm.Assert();
                return NativeWindowMethods.DwmGetWindowAttributeApiUnsafe(hwnd, dwAttribute, ref pvAttribute, cbAttribute);
            }
            catch
            {
                return -1;
            }
        }

        private static int DwmGetWindowAttributeApiUnsafe(IntPtr hwnd, uint dwAttribute, ref RECT pvAttribute, uint cbAttribute)
        {
            return NativeWindowMethods.DwmGetWindowAttribute(hwnd, dwAttribute, ref pvAttribute, cbAttribute);
        }

            #endregion //DwmGetWindowAttributeApi

			// MBS 11/21/06 - Glass Support
			#region DwmExtendFrameIntoClientAreaApi

        internal static int DwmExtendFrameIntoClientAreaApi(System.IntPtr hWnd, ref MARGINS pMargins)
        {
            if (System.Environment.OSVersion.Version.Major < 6)
                return -1;

			// MD 12/15/06 - FxCop
			// This is not needed due to the SuppressUnmanagedCodeSecurityAttribute on NativeWindowMethods.
			//
			//SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);

            try
            {
				//perm.Assert();

                return NativeWindowMethods.DwmExtendFrameIntoClientAreaApiUnsafe(hWnd, ref pMargins);
            }
            catch
            {
                return -1;
            }
        }

        private static int DwmExtendFrameIntoClientAreaApiUnsafe(System.IntPtr hWnd, ref MARGINS pMargins)
        {
            return NativeWindowMethods.DwmExtendFrameIntoClientArea(hWnd, ref pMargins);
        }

			#endregion //DwmExtendFrameIntoClientAreaApi

			// MD 10/31/06 - 64-Bit Support
			#region AddBits

        internal static IntPtr AddBits( IntPtr ptr, int bitsToAdd)
		{
			if ( IntPtr.Size == NativeWindowMethods.PtrSizeOn32Bit )
                return new IntPtr(ptr.ToInt32() | bitsToAdd);
			else
                return new IntPtr(ptr.ToInt64() | (uint)bitsToAdd);
		}

			#endregion AddBits

			#region AreBitsPresent

			internal static bool AreBitsPresent( IntPtr ptr, int bits )
			{
				if ( IntPtr.Size == NativeWindowMethods.PtrSizeOn32Bit )
					return ( ptr.ToInt32() & bits ) != 0;
				else
					return ( ptr.ToInt64() & bits ) != 0;
			}

			#endregion AreBitsPresent

			#region RemoveBits

			internal static IntPtr RemoveBits( IntPtr ptr, int bitsToRemove )
			{
				// AS 8/6/12 TFS118105
				//if ( IntPtr.Size == NativeWindowMethods.PtrSizeOn32Bit )
				//    return new IntPtr( ptr.ToInt32() & ~bitsToRemove );
				//else
				//    return new IntPtr( ptr.ToInt64() & ~bitsToRemove );
				int bitsRemoved;
				return RemoveBits( ptr, bitsToRemove, out bitsRemoved );
			}

			internal static IntPtr RemoveBits(IntPtr ptr, int bitsToRemove, out int bitsRemoved)
			{
				if ( IntPtr.Size == NativeWindowMethods.PtrSizeOn32Bit )
				{
					int intPtr = ptr.ToInt32();
					bitsRemoved = intPtr & bitsToRemove;
					return new IntPtr( intPtr & ~bitsToRemove );
				}
				else
				{
					long longPtr = ptr.ToInt64();
					bitsRemoved = (int)(longPtr & bitsToRemove);
					return new IntPtr( longPtr & ~bitsToRemove );
				}
			}

			#endregion RemoveBits

			// AS 11/7/06 BR17495
			#region GetWindowState
            
#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

			#endregion //GetWindowState

			#region GetXFromLParam
			internal static int GetXFromLParam(IntPtr lParam)
			{
				// explicitly casted to short to maintain sign
                // AS 11/7/08 TFS10274
                //return (short)(lParam.ToInt32() & 0xffff);
                return (short)(lParam.ToInt64() & 0xffff);
			} 
			#endregion //GetXFromLParam

			#region GetYFromLParam
			internal static int GetYFromLParam(IntPtr lParam)
			{
				// explicitly casted to short to maintain sign
                // AS 11/7/08 TFS10274
                //return (short)((lParam.ToInt32() >> 16) & 0xffff);
                return (short)((lParam.ToInt64() >> 16) & 0xffff);
			} 
			#endregion //GetYFromLParam

            // AS 10/10/08 TFS6236
            #region EncodeSize

            /// <summary>
            /// Encodes a size into and IntPtr to be used for certain window messages.
            /// </summary>
            internal static IntPtr MakeLParam(int width, int height)
            {
                // AS 3/4/09 TFS14926
                //return new IntPtr((height << 16) | width);
                return (IntPtr)(((short)height << 16) | (width & 0xffff));
            }

            #endregion EncodeSize

            #region GetWindowState
            internal static System.Windows.WindowState GetWindowState(System.Windows.Window window)
            {
                System.Windows.WindowState state = System.Windows.WindowState.Normal;

                if (window != null)
                {
                    System.Windows.Interop.WindowInteropHelper windowHelper = new System.Windows.Interop.WindowInteropHelper(window);
                    IntPtr hwnd = windowHelper.Handle;

                    if (hwnd != IntPtr.Zero)
                    {
                        NativeWindowMethods.WINDOWPLACEMENT placement = new NativeWindowMethods.WINDOWPLACEMENT();
                        placement.length = Marshal.SizeOf(placement);
                        bool retVal = NativeWindowMethods.GetWindowPlacement(hwnd, ref placement);
                        Debug.Assert(retVal, "GetWindowPlacement failed!");

                        if (placement.showCmd == SW_SHOWMINIMIZED)
                            state = System.Windows.WindowState.Minimized;
                        else if (placement.showCmd == SW_SHOWMAXIMIZED)
                            state = System.Windows.WindowState.Maximized;
                        else
                        {
                            Debug.Assert(placement.showCmd == SW_SHOWNORMAL, "Unexpected show command!");
                            state = System.Windows.WindowState.Normal;
                        }
                    }
                    else
                        state = window.WindowState;
                }

                return state;
            }
            #endregion //GetWindowState

			// AS 9/18/09 TFS19281
			#region AdjustMaximizedRect
			internal static void AdjustMaximizedRect(ref RECT rect)
			{
				RECT monitorRect = GetMonitorRect(rect);
				RECT intersection = RECT.Intersect(monitorRect, rect);

				// if the height is completely vertically obscured...
				if (intersection.Height == monitorRect.Height)
				{
					rect.bottom = monitorRect.bottom - 1;
				}
			}
			#endregion //AdjustMaximizedRect

			// AS 9/18/09 TFS19281
			#region GetMonitorRect
			private static RECT GetMonitorRect(RECT rect)
			{
				if (NativeWindowMethods.GetSystemMetrics(SM_CMONITORS) == 0)
				{
					return new RECT(0, 0, GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN));
				}

				IntPtr hMonitor = MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);

				MONITIORINFOEX mi = new MONITIORINFOEX();
				GetMonitorInfo(new HandleRef(null, hMonitor), mi);

				return mi._rcMonitor;
			}
			#endregion //GetMonitorRect

			// AS 11/4/11 TFS91009
			// If the MINIMIZEBOX or MAXIMIZEBOX style bit is removed then consider the associated command to be disabled.
			//
			#region IsWindowStyleRemoved
			internal static bool IsWindowStyleRemoved(System.Windows.Window window, int styleBit)
			{
				var helper = new System.Windows.Interop.WindowInteropHelper(window);

				if (helper.Handle != IntPtr.Zero)
				{
					var style = GetWindowLong(helper.Handle, GWL_STYLE);

					if (!AreBitsPresent(style, styleBit))
						return true;
				}

				return false;
			} 
			#endregion //IsWindowStyleRemoved

			// AS 11/4/11 TFS91009
			#region PreventClose
			internal static bool PreventClose(System.Windows.Window window)
			{
				var wih = new System.Windows.Interop.WindowInteropHelper(window);
				var hwnd = wih.Handle;

				if (hwnd != IntPtr.Zero)
				{
					IntPtr sysMenu = GetSystemMenu(hwnd, false);

					if (IntPtr.Zero != sysMenu)
					{
						int flags = GetMenuState(sysMenu, SC_CLOSE, MF_BYCOMMAND);

						if ((flags & (MF_GRAYED | MF_DISABLED)) != 0)
							return true;
					}

				}

				return false;
			} 
			#endregion //PreventClose

			// AS 8/6/12 TFS118105
			#region CanOSSupportThemes
			private static bool CanOSSupportThemes()
			{
				OperatingSystem os = Environment.OSVersion;

				if ( os.Platform == PlatformID.Win32NT )
					return os.Version.Major > 5 || (os.Version.Major == 5 && os.Version.Minor >= 1);

				return false;
			}
			#endregion //CanOSSupportThemes

			// AS 8/6/12 TFS118105
			#region SignedLoWord
			internal static int SignedLoWord(int value)
			{
				return (short)(value & 0xffff);
			}
			#endregion // SignedLoWord

			// AS 8/6/12 TFS118105
			#region SignedHiWord
			internal static int SignedHiWord(int value)
			{
				return (short)((value >> 16) & 0xffff);
			}
			#endregion // SignedHiWord

			// AS 8/6/12 TFS118105
			#region IsClassicTheme
			internal static bool IsClassicTheme
			{
				get
				{
					if ( !AreThemesSupported )
						return false;

					return !IsThemeActiveInternal();
				}
			}
			#endregion //IsClassicTheme

			// AS 8/6/12 TFS118105
			#region IsThemeActiveInternal
			[MethodImpl( MethodImplOptions.NoInlining )]
			private static bool IsThemeActiveInternal()
			{
				if ( IsThemeActive() == 0 )
					return false;

				if ( IsAppThemed() == 0 )
					return false;

				return true;
			}
			#endregion //IsThemeActiveInternal

		#endregion // Helper Methods
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