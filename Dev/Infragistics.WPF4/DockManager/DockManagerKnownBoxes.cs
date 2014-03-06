using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.DockManager.Dragging;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Class that maintains references to commonly boxed <see cref="XamDockManager"/> related values.
	/// </summary>
	internal static class DockManagerKnownBoxes
	{
		#region Constants

		#region PaneLocation

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'DockedLeft'.
		/// </summary>
		public static readonly object PaneLocationDockedLeftBox = PaneLocation.DockedLeft;

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'DockedTop'.
		/// </summary>
		public static readonly object PaneLocationDockedTopBox = PaneLocation.DockedTop;

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'DockedBottom'.
		/// </summary>
		public static readonly object PaneLocationDockedBottomBox = PaneLocation.DockedBottom;

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'DockedRight'.
		/// </summary>
		public static readonly object PaneLocationDockedRightBox = PaneLocation.DockedRight;

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'Floating'.
		/// </summary>
		public static readonly object PaneLocationFloatingBox = PaneLocation.Floating;

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'FloatingOnly'.
		/// </summary>
		public static readonly object PaneLocationFloatingOnlyBox = PaneLocation.FloatingOnly;

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'Unpinned'.
		/// </summary>
		public static readonly object PaneLocationUnpinnedBox = PaneLocation.Unpinned;

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'Unknown'.
		/// </summary>
		public static readonly object PaneLocationUnknownBox = PaneLocation.Unknown;

		/// <summary>
		/// Returns an object containing the <see cref="PaneLocation"/> 'Document'.
		/// </summary>
		public static readonly object PaneLocationDocumentBox = PaneLocation.Document;

		#endregion //PaneLocation

		#region DropPreviewTabLocation
		/// <summary>
		/// Returns an object containing the <see cref="DropPreviewTabLocation"/> 'None'.
		/// </summary>
		public static readonly object DropPreviewTabLocationNoneBox = DropPreviewTabLocation.None;


		/// <summary>
		/// Returns an object containing the <see cref="DropPreviewTabLocation"/> 'Top'.
		/// </summary>
		public static readonly object DropPreviewTabLocationTopBox = DropPreviewTabLocation.Top;


		/// <summary>
		/// Returns an object containing the <see cref="DropPreviewTabLocation"/> 'Left'.
		/// </summary>
		public static readonly object DropPreviewTabLocationLeftBox = DropPreviewTabLocation.Left;


		/// <summary>
		/// Returns an object containing the <see cref="DropPreviewTabLocation"/> 'Right'.
		/// </summary>
		public static readonly object DropPreviewTabLocationRightBox = DropPreviewTabLocation.Right;


		/// <summary>
		/// Returns an object containing the <see cref="DropPreviewTabLocation"/> 'Bottom'.
		/// </summary>
		public static readonly object DropPreviewTabLocationBottomBox = DropPreviewTabLocation.Bottom;

		#endregion //DropPreviewTabLocation

		#region InitialPaneLocation

		/// <summary>
		/// Returns an object containing the <see cref="InitialPaneLocation"/> 'DockedLeft'.
		/// </summary>
		public static readonly object InitialPaneLocationDockedLeftBox = InitialPaneLocation.DockedLeft;

		/// <summary>
		/// Returns an object containing the <see cref="InitialPaneLocation"/> 'DockedTop'.
		/// </summary>
		public static readonly object InitialPaneLocationDockedTopBox = InitialPaneLocation.DockedTop;

		/// <summary>
		/// Returns an object containing the <see cref="InitialPaneLocation"/> 'DockedBottom'.
		/// </summary>
		public static readonly object InitialPaneLocationDockedBottomBox = InitialPaneLocation.DockedBottom;

		/// <summary>
		/// Returns an object containing the <see cref="InitialPaneLocation"/> 'DockedRight'.
		/// </summary>
		public static readonly object InitialPaneLocationDockedRightBox = InitialPaneLocation.DockedRight;

		/// <summary>
		/// Returns an object containing the <see cref="InitialPaneLocation"/> 'DockableFloating'.
		/// </summary>
		public static readonly object InitialPaneLocationDockableFloatingBox = InitialPaneLocation.DockableFloating;

		/// <summary>
		/// Returns an object containing the <see cref="InitialPaneLocation"/> 'FloatingOnly'.
		/// </summary>
		public static readonly object InitialPaneLocationFloatingOnlyBox = InitialPaneLocation.FloatingOnly;

		#endregion //InitialPaneLocation

		#endregion //Constants

		#region FromValue
		/// <summary>
		/// Returns a boxed representation of the specified <see cref="PaneLocation"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="PaneLocation"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="PaneLocation"/> value</returns>
		public static object FromValue(PaneLocation value)
		{
			switch (value)
			{
				default:
				case PaneLocation.DockedBottom:
					return PaneLocationDockedBottomBox;
				case PaneLocation.DockedLeft:
					return PaneLocationDockedLeftBox;
				case PaneLocation.DockedTop:
					return PaneLocationDockedTopBox;
				case PaneLocation.DockedRight:
					return PaneLocationDockedRightBox;
				case PaneLocation.Unpinned:
					return PaneLocationUnpinnedBox;
				case PaneLocation.Floating:
					return PaneLocationFloatingBox;
				case PaneLocation.FloatingOnly:
					return PaneLocationFloatingOnlyBox;
				case PaneLocation.Document:
					return PaneLocationDocumentBox;
				case PaneLocation.Unknown:
					return PaneLocationUnknownBox;
			}
		}

		/// <summary>
		/// Returns a boxed representation of the specified <see cref="InitialPaneLocation"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="InitialPaneLocation"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="InitialPaneLocation"/> value</returns>
		public static object FromValue(InitialPaneLocation value)
		{
			switch (value)
			{
				case InitialPaneLocation.DockedBottom:
					return InitialPaneLocationDockedBottomBox;
				default:
				case InitialPaneLocation.DockedLeft:
					return InitialPaneLocationDockedLeftBox;
				case InitialPaneLocation.DockedTop:
					return InitialPaneLocationDockedTopBox;
				case InitialPaneLocation.DockedRight:
					return InitialPaneLocationDockedRightBox;
				case InitialPaneLocation.DockableFloating:
					return InitialPaneLocationDockableFloatingBox;
				case InitialPaneLocation.FloatingOnly:
					return InitialPaneLocationFloatingOnlyBox;
			}
		}

		/// <summary>
		/// Returns a boxed representation of the specified <see cref="DropPreviewTabLocation"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="DropPreviewTabLocation"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="DropPreviewTabLocation"/> value</returns>
		public static object FromValue(DropPreviewTabLocation value)
		{
			switch (value)
			{
				default:
				case DropPreviewTabLocation.None:
					return DropPreviewTabLocationNoneBox;
				case DropPreviewTabLocation.Left:
					return DropPreviewTabLocationLeftBox;
				case DropPreviewTabLocation.Right:
					return DropPreviewTabLocationRightBox;
				case DropPreviewTabLocation.Bottom:
					return DropPreviewTabLocationBottomBox;
				case DropPreviewTabLocation.Top:
					return DropPreviewTabLocationTopBox;
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