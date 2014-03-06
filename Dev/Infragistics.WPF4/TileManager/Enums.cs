using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Controls.Layouts
{
	#region AllowTileDragging enum

	/// <summary>
	/// Determines whether tiles can be dragged from one location to another.
	/// </summary>
	public enum AllowTileDragging
	{
		/// <summary>
		/// Tiles can not be dragged from one location to another
		/// </summary>
		No = 0,

		/// <summary>
		/// A tile can be dragged to a new location. The Tile is repositioned when the drop occurs at which time its swaps its location with the Tile being dropped upon. 
		/// </summary>
		Swap = 1,

		/// <summary>
		/// A tile can be dragged to a new location. During the drag operation other tiles are dynamically animated over to make room for the Tile being dragged.
		/// </summary>
		Slide = 2,
	}

	#endregion //AllowTileDragging

    #region AllowTileSizing enum

    /// <summary>
    /// Determines whether tiles can be resized.
    /// </summary>
    /// <remarks>
    /// <para class="note"><b>Note:</b> the 'Synchronized' setting (the default) is ignored in maximized mode or if <see cref="TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'. 
    /// Also, if TileLayoutOrder is set to 'HorizontalVariable' only the synchronized height will be used. Likewise, if it is set to 'VerticalVaraible' only
    /// the synchronized width will be used.</para>
    /// </remarks>
    /// <seealso cref="NormalModeSettings.AllowTileSizing"/>
     public enum AllowTileSizing
    {
        /// <summary>
        /// When one tile is resized, all tiles are resized to that same size. This is the default setting.
        /// </summary>
        Synchronized = 0,

        /// <summary>
        /// Tiles can be resized independently of one another
        /// </summary>
        Individual = 1,

        /// <summary>
        /// Tiles can not be resized.
        /// </summary>
        No = 2,
    }

    #endregion //AllowTileSizing

	#region ExplicitLayoutTileSizeBehavior

	/// <summary>
	/// Determines whether tile heights are synchronized across columns and whether tile widths are synchronized across rows when <see cref="TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// If there are ColumnsSpan values specified > 1 on one or more tiles then all tiles in intersecting columns will behave as if this setting was 'SynchronizeTileWidthsAndHeights' with respect to all other tiles in those intersecting columns.</para>
	/// <para class="body">
	/// Likewise, if there are RowSpan values specified > 1 on one or more tiles then all tiles in intersecting rows will behave as if this setting was 'SynchronizeTileWidthsAndHeights' with respect to all other tiles in those intersecting rows.</para>
	/// <para class="note"><b>Note:</b> regardless of the value of this setting if the overall size is constrained (e.g. if the HorizontalTileAreaAlignment and/or VerticalTileAreaAlignment is set to 'Stretch') 
	/// then resizing a tile's width may indirectly affect the width of all tiles and resizing its height may indirectly affect the height of all tiles respectively.
	/// </para>
	/// </remarks>
	public enum ExplicitLayoutTileSizeBehavior
	{
		/// <summary>
		/// Tile heights and widths are synchronized across all row and columns in the layout. 
		/// For example, resizing a tile's height in one column will affect the height of tiles in all columns that intersect with the resized tile's row. 
		/// Likewise, resizing its width will affect the width of tiles in all rows that intersect with the resized tile's column.
		/// </summary>
		SynchronizeTileWidthsAndHeights = 0,

		/// <summary>
		/// Tile widths are synchronized across all rows in the layout. 
		/// For example, resizing a tile's width in one row will affect the width of tiles in all rows that intersect with the resized tile's column. 
		/// However, resizing its height will affect only that tile unless column spans > 1 are specified that intersect with the tile's column.
		/// </summary>
		SynchronizeTileWidthsAcrossRows = 1,

		/// <summary>
		/// Tile heights are synchronized across all columns in the layout. 
		/// For example, resizing a tile's height in one column will affect the height of tiles in all columns that intersect with the resized tile's row. 
		/// However, resizing its width will affect only that tile unless row spans > 1 are specified that intersect with the tile's row.
		/// </summary>
		SynchronizeTileHeightsAcrossColumns = 2,
	}

	#endregion //ExplicitLayoutTileSizeBehavior

	#region MaximizedTileLayoutOrder enum

	/// <summary>
	/// Determines how the maximized tiles will be arranged.
	/// </summary>
	public enum MaximizedTileLayoutOrder
	{
		/// <summary>
		/// Arrange maximized tiles one after another from top to bottom to fill all available space then move to the next column in a snaking fashion. If the resulting number of rows times columns doesn't exactly match the number of maximized tiles then the last tile will span any remaining rows in order to fill up all the avaialble space in the maximized area.
		/// </summary>
		VerticalWithLastTileFill = 0,

		/// <summary>
		/// Arrange maximized tiles one after another from top to bottom to fill all available space then move to the next column in a snaking fashion.
		/// </summary>
		Vertical = 1,

		/// <summary>
		/// Arrange maximized tiles one after another from left to right to fill all available columns then move to the next row below in a snaking fashion. If the resulting number of rows times columns doesn't exactly match the number of maximized tiles then the last tile will span any remaining columns in order to fill up all the avaialble space in the maximized area.
		/// </summary>
		HorizontalWithLastTileFill = 2,

		/// <summary>
		/// Arrange maximized tiles one after another from left to right to fill all available columns then move to the next row below in a snaking fashion.
		/// </summary>
		Horizontal = 3,

		/// <summary>
		/// Arrange maximized maxmized tiles vertically, one on top of the other.
		/// </summary>
		SingleColumn = 4,

		/// <summary>
		/// Arrange maximized maxmized tiles horizontally, left to right.
		/// </summary>
		SingleRow = 5,
	}

	#endregion //MaximizedTileLayoutOrder

	#region MaximizedTileLocation enum

	/// <summary>
	/// Determines where the maximized tile area will be relative to the minimized tile area
	/// </summary>
	public enum MaximizedTileLocation
	{
		/// <summary>
		/// Maximized tiles are arranged on the left therefore minimized tiles are on the right.
		/// </summary>
		Left = 1,

		/// <summary>
		/// Maximized tiles are arranged on the right therefore minimized tiles are on the left.
		/// </summary>
		Right = 2,

		/// <summary>
		/// Maximized tiles are arranged on the top therefore minimized tiles are on the bottom.
		/// </summary>
		Top = 3,

		/// <summary>
		/// Maximized tiles are arranged on the bottom therefore minimized tiles are on the top.
		/// </summary>
		Bottom = 4,







	}

	#endregion //MaximizedTileLocation

    #region MinimizedTileExpansionMode enum

    /// <summary>
    /// Determines how many tiles may be expanded in the <see cref="XamTileManager"/> when they are minimized
    /// </summary>
    /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
    public enum MinimizedTileExpansionMode
    {
        /// <summary>
        /// Only one minimized tile can be expanded at any time (i.e. expanding another tile will restore the existing one's state to 'Minimized').
        /// </summary>
        AllowOne = 0,

        /// <summary>
        /// Any number of minimized tiles can be expanded (i.e. in the 'MinimizedExpanded' state) and when a tile is initially shown in the minimized tile area its state will be 'Minimized'.
        /// </summary>
        AllowMultiple = 1,

        /// <summary>
        /// Any number of minimized tiles can be expanded (i.e. in the 'MinimizedExpanded' state) and when a tile is initially shown in the minimized tile area its state will be 'MinimizedExpanded'. This is the default setting.
        /// </summary>
        AllowMultipleExpandAllInitially = 2,
    }

    #endregion //MinimizedTileExpansionMode

	#region TileCommandType

	/// <summary>
	/// Identifies the commands exposed by <see cref="XamTile"/> 
	/// </summary>
	public enum TileCommandType : short
	{
		/// <summary>
		/// Command used to closes a <see cref="XamTile"/>. The source must be a XamTile or within it - or the CommandParameter must be the tile.
		/// </summary>
		Close,

		/// <summary>
		/// Command used to toggle the <see cref="XamTile.State"/> of a <see cref="XamTile"/> to and from 'Maximized'.
		/// </summary>
		ToggleMaximized,

		/// <summary>
		/// Command used to toggle the <see cref="XamTile.State"/> of a <see cref="XamTile"/> to and from 'MinimizedExpanded'.
		/// </summary>
		ToggleMinimizedExpansion,
	}

	#endregion //TileCommandType	

	#region TileLayoutOrder enum

	/// <summary>
	/// Determines how the tiles will be laid out in normal mode
	/// </summary>
	public enum TileLayoutOrder
	{
		/// <summary>
		/// Position tiles one after another from top to bottom to fill all available space vertically then move to the next column in a snaking fashion to form uniform rows and columns.
		/// </summary>
		Vertical = 0,

		/// <summary>
		/// Position tiles one after another from top to bottom to fill all available space vertically then move to the next column in a snaking fashion supporting variable height tiles so that each column can have a different number of tiles.
		/// </summary>
		VerticalVariable = 1,

		/// <summary>
		/// Position tiles one after another from left to right to fill all available columns then move to the next row in a snaking fashion to form uniform rows and columns.
		/// </summary>
		Horizontal = 2,

		/// <summary>
		/// Position tiles one after another from left to right to fill all available space horizontally then move to the next row in a snaking fashion supporting variable width tiles so that each row can have a different number of tiles.
		/// </summary>
		HorizontalVariable = 3,

		/// <summary>
		/// Position tiles based on their explicit Row, RowSpan, Column, and ColumnSpan settings.
		/// </summary>
		UseExplicitRowColumnOnTile = 4,
	}

	#endregion //TileLayoutOrder

	#region TileState enum

	/// <summary>
	/// Represents the state of an individual tile
	/// </summary>
	public enum TileState
	{
		/// <summary>
		/// All tiles will return 'Normal' as long as no tile is in a 'Maximized' state. Conversely if any Tile is 'Maximized' then no Tile with return 'Normal'.
		/// </summary>
		Normal = 0,

		/// <summary>
		/// If any tile is in a 'Maximized' state then all other tiles can only be 'Maximized', 'Minimized' or 'MinimizedExpanded'.
		/// </summary>
		Maximized = 1,

		/// <summary>
		/// 'Minimized' is only returned if there is at least one tile in a 'Maximized' state. A Tile in the 'Minimized' state will usually just display its Header not its Content.
		/// </summary>
		Minimized = 2,

		/// <summary>
		/// 'MinimizedExpanded' is only returned if there is at least one tile in a 'Maximized' state. A Tile in the 'MinimizedExpanded' state will usually display its Header and its Content but the Tile will be grouped with all other minimized Tiles, both collapsed and expanded.
		/// </summary>
		MinimizedExpanded = 3,
	}

	#endregion //TileState

    #region TileCloseAction enum

    /// <summary>
    /// Determines what happens when Tiles are closed.
    /// </summary>
    /// <seealso cref="XamTileManager.TileCloseAction"/>
    /// <seealso cref="XamTile.CloseAction"/>
    /// <seealso cref="ItemTileInfo"/>
    /// <seealso cref="XamTileManager.GetItemInfo(object)"/>
    /// <seealso cref="XamTile.CloseButtonVisibility"/>
    /// <seealso cref="XamTile.CloseButtonVisibilityResolved"/>
     public enum TileCloseAction
    {
        /// <summary>
        /// The default value which resolves to 'DoNothing'
        /// </summary>
        Default = 0,

        /// <summary>
        /// The <see cref="XamTile"/> can't be closed.
        /// </summary>
        DoNothing = 1,

        /// <summary>
        /// When the <see cref="XamTile"/> is closed its Visibility will be coerced to 'Collapsed' but the associated item will remain in the Items collection.
        /// </summary>
        CollapseTile = 2,

        /// <summary>
        /// When the <see cref="XamTile"/> is closed the associated item will be removed from the Items collection.
        /// </summary>
        RemoveItem = 3,
    }

    #endregion //TileCloseAction

// ------------------ internal enums -----------------------------
	 #region ScrollDirection enum

	 /// <summary>
	 /// Determines what direction to scroll
	 /// </summary>
	 internal enum ScrollDirection
	 {
		 /// <summary>
		 /// Increment the scroll value
		 /// </summary>
		 Increment = 0,
		 /// <summary>
		 /// Decrement the scroll value
		 /// </summary>
		 Decrement = 1,
	 }

	 #endregion //ScrollDirection enum	
        
	 #region ScrollSpeed enum

	 /// <summary>
	 /// Determines how fast a slection scroll should be
	 /// </summary>
	 internal enum ScrollSpeed
	 {
		 /// <summary>
		 /// Scroll the slowest
		 /// </summary>
		 Slowest = 0,
		 /// <summary>
		 /// Scroll slower
		 /// </summary>
		 Slower = 1,
		 /// <summary>
		 /// Scroll slowly
		 /// </summary>
		 Slow = 2,
		 /// <summary>
		 /// Scroll a moderate amount
		 /// </summary>
		 Medium = 3,
		 /// <summary>
		 /// Scroll fast
		 /// </summary>
		 Fast = 4,
		 /// <summary>
		 /// Scroll faster
		 /// </summary>
		 Faster = 5,
		 /// <summary>
		 /// Scroll fastest
		 /// </summary>
		 Fastest = 6
	 }

	 #endregion //ScrollSpeed enum

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