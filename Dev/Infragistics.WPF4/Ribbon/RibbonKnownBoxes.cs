using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Class that maintains references to commonly boxed <see cref="XamRibbon"/> related values.
	/// </summary>
	internal static class RibbonKnownBoxes
	{
		#region Constants

		#region RibbonToolSizingMode
		/// <summary>
		/// Returns an object containing the <see cref="RibbonToolSizingMode"/> 'ImageAndTextLarge'.
		/// </summary>
		public static readonly object RibbonToolSizingModeImageAndTextLargeBox = RibbonToolSizingMode.ImageAndTextLarge;

		/// <summary>
		/// Returns an object containing the <see cref="RibbonToolSizingMode"/> 'ImageAndTextNormal'.
		/// </summary>
		public static readonly object RibbonToolSizingModeImageAndTextNormalBox = RibbonToolSizingMode.ImageAndTextNormal;

		/// <summary>
		/// Returns an object containing the <see cref="RibbonToolSizingMode"/> 'ImageOnly'.
		/// </summary>
		public static readonly object RibbonToolSizingModeImageOnlyBox = RibbonToolSizingMode.ImageOnly;

		#endregion //RibbonToolSizingMode

		#region ToolLocation
		/// <summary>
		/// Returns an object containing the <see cref="ToolLocation"/> 'ApplicationMenu'.
		/// </summary>
		public static readonly object ToolLocationApplicationMenuBox = ToolLocation.ApplicationMenu;

		/// <summary>
		/// Returns an object containing the <see cref="ToolLocation"/> 'ApplicationMenuFooterToolbar'.
		/// </summary>
		public static readonly object ToolLocationApplicationMenuFooterToolbarBox = ToolLocation.ApplicationMenuFooterToolbar;

		/// <summary>
		/// Returns an object containing the <see cref="ToolLocation"/> 'ApplicationMenuRecentItems'.
		/// </summary>
		public static readonly object ToolLocationApplicationMenuRecentItemsBox = ToolLocation.ApplicationMenuRecentItems;

		/// <summary>
		/// Returns an object containing the <see cref="ToolLocation"/> 'ApplicationMenuSubMenu'.
		/// </summary>
		public static readonly object ToolLocationApplicationMenuSubMenuBox = ToolLocation.ApplicationMenuSubMenu;

		/// <summary>
		/// Returns an object containing the <see cref="ToolLocation"/> 'Menu'.
		/// </summary>
		public static readonly object ToolLocationMenuBox = ToolLocation.Menu;

		/// <summary>
		/// Returns an object containing the <see cref="ToolLocation"/> 'QuickAccessToolbar'.
		/// </summary>
		public static readonly object ToolLocationQuickAccessToolbarBox = ToolLocation.QuickAccessToolbar;

		/// <summary>
		/// Returns an object containing the <see cref="ToolLocation"/> 'Ribbon'.
		/// </summary>
		public static readonly object ToolLocationRibbonBox = ToolLocation.Ribbon;

		/// <summary>
		/// Returns an object containing the <see cref="ToolLocation"/> 'Unknown'.
		/// </summary>
		public static readonly object ToolLocationUnknownBox = ToolLocation.Unknown;

		#endregion //ToolLocation

		// AS 10/17/07 - MenuItemDescriptionMinWidth
		internal static readonly object DoubleNanBox = double.NaN;
		internal static readonly object DoubleZeroBox = 0d;

		#endregion //Constants

		#region FromValue
		/// <summary>
		/// Returns a boxed representation of the specified <see cref="RibbonToolSizingMode"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="RibbonToolSizingMode"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="RibbonToolSizingMode"/> value</returns>
		public static object FromValue(RibbonToolSizingMode value)
		{
			switch (value)
			{
				default:
				case RibbonToolSizingMode.ImageAndTextLarge:
					return RibbonToolSizingModeImageAndTextLargeBox;
				case RibbonToolSizingMode.ImageAndTextNormal:
					return RibbonToolSizingModeImageAndTextNormalBox;
				case RibbonToolSizingMode.ImageOnly:
					return RibbonToolSizingModeImageOnlyBox;
			}
		}

		/// <summary>
		/// Returns a boxed representation of the specified <see cref="ToolLocation"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="ToolLocation"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="ToolLocation"/> value</returns>
		public static object FromValue(ToolLocation value)
		{
			switch (value)
			{
				default:
				case ToolLocation.ApplicationMenu:
					return ToolLocationApplicationMenuBox;
				case ToolLocation.ApplicationMenuFooterToolbar:
					return ToolLocationApplicationMenuFooterToolbarBox;
				case ToolLocation.ApplicationMenuRecentItems:
					return ToolLocationApplicationMenuRecentItemsBox;
				case ToolLocation.ApplicationMenuSubMenu:
					return ToolLocationApplicationMenuSubMenuBox;
				case ToolLocation.Menu:
					return ToolLocationMenuBox;
				case ToolLocation.QuickAccessToolbar:
					return ToolLocationQuickAccessToolbarBox;
				case ToolLocation.Ribbon:
					return ToolLocationRibbonBox;
				case ToolLocation.Unknown:
					return ToolLocationUnknownBox;
			}
		}
		#endregion //FromValue
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