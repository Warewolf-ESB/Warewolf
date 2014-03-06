using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.DockManager.Dragging
{
	#region AllowedDropLocations
	[Flags]
	internal enum AllowedDropLocations
	{
		Left = DockableAreas.Left,
		Top = DockableAreas.Top,
		Right = DockableAreas.Right,
		Bottom = DockableAreas.Bottom,
		Floating = DockableAreas.Floating,
		Document = 0x20,
		Docked = Left | Right | Top | Bottom,
		All = Left | Right | Top | Bottom | Floating | Document,
	} 
	#endregion //AllowedDropLocations

	#region DragState
	internal enum DragState
	{
		/// <summary>
		/// No drag is occurring
		/// </summary>
		None,

		/// <summary>
		/// A drag operation may begin
		/// </summary>
		Pending,

		/// <summary>
		/// A drag operation is occurring
		/// </summary>
		Dragging,

		// AS 12/9/09 TFS25268
		/// <summary>
		/// The drag is complete and the drop action is being performed.
		/// </summary>
		ProcessingDrop,
	}
	#endregion //DragState

	#region DropPreviewTabLocation
	/// <summary>
	/// Enumeration used within the <see cref="XamDockManager.DropPreviewStyleKey"/> to identify where the tab item should be displayed within the drop preview.
	/// </summary>
	public enum DropPreviewTabLocation
	{
		/// <summary>
		/// No tab item is used. The drop will result in a split
		/// </summary>
		None,

		/// <summary>
		/// The tab placement is such that the tab items are positioned on the left edge.
		/// </summary>
		Left,

		/// <summary>
		/// The tab placement is such that the tab items are positioned on the top edge.
		/// </summary>
		Top,

		/// <summary>
		/// The tab placement is such that the tab items are positioned on the right edge.
		/// </summary>
		Right,

		/// <summary>
		/// The tab placement is such that the tab items are positioned on the bottom edge.
		/// </summary>
		Bottom
	} 
	#endregion //DropPreviewTabLocation

	#region RootSplitPaneLocation
	/// <summary>
	/// Identifies the location of a new root split pane created during a drag operation.
	/// </summary>
	public enum RootSplitPaneLocation
	{
		/// <summary>
		/// A new split pane along the outer edge of the <see cref="XamDockManager"/> will be created
		/// </summary>
		OuterDockManagerEdge,

		/// <summary>
		/// A new split pane along the inner edge of the <see cref="XamDockManager"/> will be created
		/// </summary>
		InnerDockManagerEdge,

		/// <summary>
		/// A new split pane will be created to encompass the root split pane(s) within the <see cref="DocumentContentHost"/>
		/// </summary>
		DocumentContentHost
	} 
	#endregion //RootSplitPaneLocation
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