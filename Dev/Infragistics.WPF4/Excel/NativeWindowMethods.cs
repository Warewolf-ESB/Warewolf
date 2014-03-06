using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace Infragistics.Documents.Excel
{

	// MD 8/23/11
	// Found while fixing TFS84306
	[SecurityCritical]

	[SuppressUnmanagedCodeSecurity]
	// MD 8/27/08 - Code Analysis - Performance
	//internal class NativeWindowMethods
	internal static class NativeWindowMethods
	{
		#region Apis

		// MD 7/8/10 - TFS34814
		#region DeleteObject

		internal static bool DeleteObjectApi(IntPtr hObject)
		{
			return NativeWindowMethods.DeleteObject(hObject);
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("gdi32", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		internal static extern bool DeleteObject(IntPtr hObject); 

		#endregion // DeleteObject

		#region GetTextMetrics

		public static bool GetTextMetricsApi( IntPtr hdc, out TEXTMETRIC textmetric )
		{
			return NativeWindowMethods.GetTextMetrics( hdc, out textmetric );
		}

		[DllImport( "Gdi32", CharSet = CharSet.Auto )]
		[return: MarshalAs( UnmanagedType.Bool )]
		private static extern bool GetTextMetrics( IntPtr hdc, out TEXTMETRIC textmetric );

		#endregion GetTextMetrics

		#region SelectObject

		public static IntPtr SelectObjectApi( IntPtr hDC, IntPtr hObject )
		{
			return NativeWindowMethods.SelectObject( hDC, hObject );
		}

		[DllImport( "gdi32", ExactSpelling = true, CharSet = CharSet.Auto )]
		private static extern IntPtr SelectObject( IntPtr hDC, IntPtr hObject );

		#endregion SelectObject

		#endregion Apis

		#region Structs

		#region TEXTMETRIC

		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
		public struct TEXTMETRIC
		{
			public int tmHeight;
			public int tmAscent;
			public int tmDescent;
			public int tmInternalLeading;
			public int tmExternalLeading;
			public int tmAveCharWidth;
			public int tmMaxCharWidth;
			public int tmWeight;
			public int tmOverhang;
			public int tmDigitizedAspectX;
			public int tmDigitizedAspectY;

			public char tmFirstChar;
			public char tmLastChar;
			public char tmDefaultChar;
			public char tmBreakChar;

			public byte tmItalic;
			public byte tmUnderlined;
			public byte tmStruckOut;
			public byte tmPitchAndFamily;
			public byte tmCharSet;
		}

		#endregion TEXTMETRIC 

		#endregion Structs
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