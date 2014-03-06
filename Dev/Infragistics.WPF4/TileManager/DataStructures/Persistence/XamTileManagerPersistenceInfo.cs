using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Layouts;

namespace Infragistics.Controls.Layouts.Primitives
{
	/// <summary>
	/// An object used to serialize the state of a <see cref="XamTileManager"/>
	/// </summary>
	public class XamTileManagerPersistenceInfo
	{
		/// <summary>
		/// Serializes whether an explicit layout is being used
		/// </summary>
		public bool IsExplicitLayout { get; set; }
		
		/// <summary>
		/// Represents the collections of items in the <see cref="XamTileManager"/>
		/// </summary>
		public List<TileItemPersistenceInfo> Items { get; set; }

		/// <summary>
		/// Serializes the minimized area extent in the X dimension if it was resized explicitly by the user when the maximized area was on the left or right.
		/// </summary>
		public double MinimizedAreaExtentX { get; set; }

		/// <summary>
		/// Serializes the minimized area extent in the Y dimension if it was resized explicitly by the user when the maximized area was on the top or bottom.
		/// </summary>
		public double MinimizedAreaExtentY { get; set; }

		/// <summary>
		/// Serializes the synchronized height of all tiles after the user resized a tile and <see cref="NormalModeSettings.AllowTileSizing"/> was set to 'Synchronized'.
		/// </summary>
		public double SynchoronizedTileHeight { get; set; }

		/// <summary>
		/// Serializes the synchronized width of all tiles after the user resized a tile and <see cref="NormalModeSettings.AllowTileSizing"/> was set to 'Synchronized'.
		/// </summary>
		public double SynchoronizedTileWidth { get; set; }
	}

	/// <summary>
	/// An object used to serialize the state of an item in a <see cref="XamTileManager"/>
	/// </summary>
	public class TileItemPersistenceInfo
	{
		/// <summary>
		/// Serializes the item's associated <see cref="XamTile"/> column number if this is an explicit layout.
		/// </summary>
		public int Column { get; set; }

		/// <summary>
		/// Serializes the item's associated <see cref="XamTile"/> column span if this is an explicit layout.
		/// </summary>
		public int ColumnSpan { get; set; }

		/// <summary>
		/// Serializes the item's associated <see cref="XamTile"/> column weight if this is an explicit layout.
		/// </summary>
		public float ColumnWeight { get; set; }

		/// <summary>
		/// Serializes whether the item's associated <see cref="XamTile"/> is closed
		/// </summary>
		public bool IsClosed { get; set; }

		/// <summary>
		/// Serializes whether the item's associated <see cref="XamTile"/> is expanded when it is minimized.
		/// </summary>
		public bool? IsExpandedWhenMinimized { get; set; }

		/// <summary>
		/// Serializes whether the item's associated <see cref="XamTile"/> is maximized
		/// </summary>
		public bool IsMaximized { get; set; }

		/// <summary>
		/// Serializes the logical index of the item.
		/// </summary>
		/// <remarks><para class="note"><b>Note:</b> this can be different than the index in the <see cref="XamTileManager"/>'s <see cref="XamTileManager.Items"/> collection if the item was swapped with another item by a user drag operation.</para></remarks>
		public int LogicalIndex { get; set; }

		/// <summary>
		/// Serializes the index of the item in the <see cref="XamTileManager"/>'s <see cref="XamTileManager.MaximizedItems"/> collection.
		/// </summary>
		/// <value>The index of the item in the <see cref="XamTileManager.MaximizedItems"/> collection if the item was maximized. Otherwise -1.</value>
		public int MaximizedIndex { get; set; }

		/// <summary>
		/// Serializes the preferred height override of the item. This could have resulted from a user resize operation if the <see cref="NormalModeSettings.AllowTileSizing"/> was set to 'Individual'.
		/// </summary>
		public double PreferredHeightOverride { get; set; } 

		/// <summary>
		/// Serializes the preferred width override of the item. This could have resulted from a user resize operation if the <see cref="NormalModeSettings.AllowTileSizing"/> was set to 'Individual'.
		/// </summary>
		public double PreferredWidthOverride { get; set; } 

		/// <summary>
		/// Serializes the item's associated <see cref="XamTile"/> Row number if this is an explicit layout.
		/// </summary>
		public int Row { get; set; }

		/// <summary>
		/// Serializes the item's associated <see cref="XamTile"/> Row span if this is an explicit layout.
		/// </summary>
		public int RowSpan { get; set; }

		/// <summary>
		/// Serializes the item's associated <see cref="XamTile"/> Row weight if this is an explicit layout.
		/// </summary>
		public float RowWeight { get; set; }

		/// <summary>
		/// Serializes a string identifier that is used to match up the item during de-serialization.
		/// </summary>
		public string SerializationId { get; set; }
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