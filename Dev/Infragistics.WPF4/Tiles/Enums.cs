using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Tiles
{
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
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
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

    #region MinimizedTileExpansionMode enum

    /// <summary>
    /// Determines how many tiles may be expanded in the <see cref="XamTilesControl"/> when they are minimized
    /// </summary>
    /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
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

    #region TileCloseAction enum

    /// <summary>
    /// Determines what happens when Tiles are closed.
    /// </summary>
    /// <seealso cref="XamTilesControl.TileCloseAction"/>
    /// <seealso cref="Tile.CloseAction"/>
    /// <seealso cref="ItemInfo"/>
    /// <seealso cref="XamTilesControl.GetItemInfo(object)"/>
    /// <seealso cref="Tile.CloseButtonVisibility"/>
    /// <seealso cref="Tile.CloseButtonVisibilityResolved"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public enum TileCloseAction
    {
        /// <summary>
        /// The default value which resolves to 'DoNothing'
        /// </summary>
        Default = 0,

        /// <summary>
        /// The <see cref="Tile"/> can't be closed.
        /// </summary>
        DoNothing = 1,

        /// <summary>
        /// When the <see cref="Tile"/> is closed its Visibility will be coerced to 'Collapsed' but the associated item will remain in the Items collection.
        /// </summary>
        CollapseTile = 2,

        /// <summary>
        /// When the <see cref="Tile"/> is closed the associated item will be removed from the Items collection.
        /// </summary>
        RemoveItem = 3,
    }

    #endregion //TileCloseAction

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