using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Security;
using System.Runtime.CompilerServices;

namespace Infragistics.Windows.Helpers
{
	internal static class SafeSystemInformation
	{
		#region Private Members

		private static bool hasSecurityExceptionBeenThrown = false;

		#endregion //Private Members	
    
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