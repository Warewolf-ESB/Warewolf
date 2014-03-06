using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Tiles
{

    #region AllowTileDragging enum

    /// <summary>
    /// Determines whether tiles can be dragged from one location to another.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
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

	// JJD 5/9/11 - TFS74206 - added 
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
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
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
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
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

    #region TileLayoutOrder enum

    /// <summary>
    /// Determines how the tiles will be laid out in normal mode
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
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
    /// Represents the state of an indiviual tile
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
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