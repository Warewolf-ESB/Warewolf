using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Shapes;

using Infragistics.Windows.Selection;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Tiles.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Virtualization;
using System.Xml;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Windows.Resizing;
using Infragistics.Shared;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Internal;
using Infragistics.Collections;

namespace Infragistics.Windows.Tiles
{
	/// <summary>
	/// A <see cref="System.Windows.Controls.Panel"/> derived element that arranges and displays its child elements as tiles, with native support for scrolling and virtualizing those items.
	/// </summary>
	
    
	//[ToolboxItem(true)]
    //[System.Drawing.ToolboxBitmap(typeof(TilesPanel), AssemblyVersion.ToolBoxBitmapFolder + "TilesPanel.bmp")]
	//[Description("A Panel derived element that arranges and displays its child elements arranged as tiles, with native support for scrolling and virtualizing those items.")]
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class TilesPanel : TilesPanelBase
    {
        #region Private Members

        private ObservableCollectionExtended<object> _maximizedItems;
        private ReadOnlyObservableCollection<object> _maximizedItemsReadOnly;
        private WeakList<object>					 _maximizedItemsSnapshot; // JJD 4/19/11 - TFS58732 - added

        private UltraLicense            _license;
        private XamTilesControl         _tilesControl;
        private ItemsControl            _itemsControl;
        private NormalModeSettings      _normalModeSettings;
        private MaximizedModeSettings   _maximizedModeSettings;
        private Tile                    _currentMinimizedExpandedTile;
        private TileMgr                 _standAloneManager;

        private double _interTileAreaSpacingResolved;
        private double _interTileSpacingX = (double)InterTileSpacingXProperty.DefaultMetadata.DefaultValue;
        private double _interTileSpacingXMaximizedResolved;
        private double _interTileSpacingXMinimizedResolved;
        private double _interTileSpacingY = (double)InterTileSpacingYProperty.DefaultMetadata.DefaultValue;
        private double _interTileSpacingYMaximizedResolved;
        private double _interTileSpacingYMinimizedResolved;

        // JJD 1/5/10 - TFS25900
        // Maintain a flag so we know that we are in the process of setting states
        internal bool _isSettingTileState;
        
        private bool _isInitializingTilesControl;

		// JJD 10/8/10 - TFS37313 - added
		private bool? _wasLoaded;

		// JJD 07/12/12 - TFS112221
		// Maintain a flag that keep track of whether we are in th middle of a drag opewration
		internal bool _IsDragging;

		// JJD 11/1/11 - TFS88171
		// Added object to cache states during a swap operation
		private SwapInfo _swapinfo;

        [ThreadStatic()]
        static MaximizedModeSettings s_DefaultMaximizedModeSettings;

        [ThreadStatic()]
        static NormalModeSettings s_DefaultNormalModeSettings;

        #endregion //Private Members	
    
        #region Constructor

        static TilesPanel()
        {
        }

        /// <summary>
        /// Instantiates a new instance of a TilesPanel.
        /// </summary>
        public TilesPanel()
        {
            try
            {
                // We need to pass our type into the method since we do not want to pass in 
                // the derived type.
                this._license = LicenseManager.Validate(typeof(TilesPanel), this) as UltraLicense;
            }
            catch (System.IO.FileNotFoundException) { }

            this._maximizedItems = new ObservableCollectionExtended<object>();
            this._maximizedItemsReadOnly = new ReadOnlyObservableCollection<object>(this._maximizedItems);

        }

        #endregion //Constructor	
    
		#region Base Class Overrides

            #region ArrangeOverride

        /// <summary>
        /// Positions child elements and determines a size for this element.
        /// </summary>
        /// <param name="finalSize">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this._tilesControl != null &&
                 this.IsInMaximizedMode &&
                 this.MaximizedModeSettingsSafe.ShowTileAreaSplitter)
                this._tilesControl.InvalidateArrange();

            return base.ArrangeOverride(finalSize);
        }

            #endregion //ArrangeOverride	
 
            #region CanSwapContainers

        /// <summary>
        /// Returns true if the containers can be swapped
        /// </summary>
        /// <param name="dragItemContainer">The container of the item to be dragged.</param>
        /// <param name="dragItemInfo">The associated drag item's info</param>
        /// <param name="swapTargetContainer">The container that is the target of the swap.</param>
        /// <param name="swapTargetItemInfo">he associated swap target item's info</param>
        /// <returns>The default implementation returns true if both are maximized or both are not maximized.</returns>
        internal protected override bool CanSwapContainers(FrameworkElement dragItemContainer, ItemInfoBase dragItemInfo, FrameworkElement swapTargetContainer, ItemInfoBase swapTargetItemInfo)
        {

            Tile source = dragItemContainer as Tile;
            Tile target = swapTargetContainer as Tile;

            if (source == null ||
                 target == null)
                return base.CanSwapContainers(dragItemContainer, dragItemInfo, swapTargetContainer, swapTargetItemInfo);

            // JJD 3/5/10 - TFS29078
            // Check if the either the source or target are maximized (not both)
            // If so make sure the tile that will become maximized if the swap
            // were to happen allows it.
            bool isSourceMaximized = source.State == TileState.Maximized;
            bool isTargetMaximized = target.State == TileState.Maximized;

            if (isSourceMaximized != isTargetMaximized)
            {
                if (isSourceMaximized == false &&
                     source.AllowMaximizeResolved == false)
                    return false;

                if (isTargetMaximized == false &&
                     target.AllowMaximizeResolved == false)
                    return false;
			}

            TileSwappingEventArgs args = new TileSwappingEventArgs(source, dragItemInfo.Item, target, swapTargetItemInfo.Item);
			
			// JJD 11/1/11 - TFS88171 
			// Initialize the SwapIsExpandedWhenMinimized property on the TileSwappingEventArgs 
			// based on whether the source or target is maximized and the other tile is not
			args.SwapIsExpandedWhenMinimized = isSourceMaximized != isTargetMaximized;

            // raise the TileSwapping event
            this.RaiseTileSwapping(args);

            // if canceled return false
            if (args.Cancel)
                return false;

			// JJD 11/1/11 - TFS88171 
			// Cache the swap info for use after the swap
			_swapinfo = new SwapInfo();
			_swapinfo._swapIsExpandedWhenMinimized = args.SwapIsExpandedWhenMinimized;
			_swapinfo._sourceIsExpandedWhenMinimized = source.IsExpandedWhenMinimized;
			_swapinfo._targetIsExpandedWhenMinimized = target.IsExpandedWhenMinimized;

            return true;
        }

            #endregion //CanSwapContainers	
            
            #region GetAllowTileDragging

        /// <summary>
        /// Returns whether this item can be dragged
        /// </summary>
        /// <param name="container">The container of the item to be dragged.</param>
        /// <param name="itemInfo">The associated item's info</param>
        /// <returns>The default implementaion returns 'No'.</returns>
        internal protected override AllowTileDragging GetAllowTileDragging(FrameworkElement container, ItemInfoBase itemInfo)
        {
            
            if (this.IsInMaximizedMode)
                return this.MaximizedModeSettingsSafe.AllowTileDragging;
            else
                return this.NormalModeSettingsSafe.AllowTileDragging;
        }

            #endregion //GetAllowTileDragging	

            #region GetContainerConstraints

        /// <summary>
        /// Gets any explicit constraints for a container
        /// </summary>
        /// <param name="container">The container in question.</param>
        /// <param name="state">The current state of the container.</param>
        /// <returns>A <see cref="ITileConstraints"/> object or null.</returns>
        internal protected override ITileConstraints GetContainerConstraints(DependencyObject container, TileState state)
        {
            Tile tile = container as Tile;

            ITileConstraints constraints = null;
            
            switch (state)
            {
                case TileState.Normal:
                    if (tile != null)
                        constraints = tile.Constraints;
                    else
                        constraints = (ITileConstraints)container.GetValue(Tile.ConstraintsProperty);
                    break;

                case TileState.Maximized:
                    if (tile != null)
                        constraints = tile.ConstraintsMaximized;
                    else
                        constraints = (ITileConstraints)container.GetValue(Tile.ConstraintsMaximizedProperty);
                    break;

                case TileState.Minimized:
                    if (tile != null)
                        constraints = tile.ConstraintsMinimized;
                    else
                        constraints = (ITileConstraints)container.GetValue(Tile.ConstraintsMaximizedProperty);
                    break;

                case TileState.MinimizedExpanded:
                    if (tile != null)
                        constraints = tile.ConstraintsMinimizedExpanded;
                    else
                        constraints = (ITileConstraints)container.GetValue(Tile.ConstraintsMinimizedExpandedProperty);
                    break;
            }

            return constraints;
        }

        internal ITileConstraints GetContainerConstraintsInternal(DependencyObject container, TileState state)
        {
            return base.GetContainerConstraints(container, state);
        }

            #endregion //GetContainerConstraints	
 
            #region GetContainerState

        /// <summary>
        /// Gets the state of a container
        /// </summary>
        /// <param name="container">The container in question.</param>
        /// <returns>A <see cref="TileState"/> enumeration. The default is 'Normal'.</returns>
        internal protected override TileState GetContainerState(DependencyObject container)
        {
            Tile tile = container as Tile;

            if (tile != null)
                return tile.State;

            return (TileState)container.GetValue(Tile.StateProperty);
        }

            #endregion //GetContainerState	
 
            #region GetDefaultConstraints

        /// <summary>
        /// Gets the default constraints for a specific state
        /// </summary>
        /// <param name="state">The state in question.</param>
        /// <returns>A <see cref="ITileConstraints"/> object or null.</returns>
        internal protected override ITileConstraints GetDefaultConstraints(TileState state)
        {
            switch (state)
            {
                default:
                case TileState.Normal:
                    return this.NormalModeSettingsSafe.TileConstraints;

                case TileState.Maximized:
                    return this.MaximizedModeSettingsSafe.MaximizedTileConstraints;

                case TileState.Minimized:
                    return this.MaximizedModeSettingsSafe.MinimizedTileConstraints;

                case TileState.MinimizedExpanded:
                    return this.MaximizedModeSettingsSafe.MinimizedExpandedTileConstraints;
            }
        }

        internal ITileConstraints GetDefaultConstraintsInternal(TileState state)
        {
            return base.GetDefaultConstraints(state);
        }

            #endregion //GetDefaultConstraints	
    
			// JJD 5/9/11 - TFS74206 - added 
			#region GetExplicitLayoutTileSizeBehavior

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
		internal protected override ExplicitLayoutTileSizeBehavior GetExplicitLayoutTileSizeBehavior()
		{
			if (this.IsInMaximizedMode)
				return ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAndHeights;

			return this.NormalModeSettingsSafe.ExplicitLayoutTileSizeBehavior;
		}

			#endregion //GetExplicitLayoutTileSizeBehavior	

            #region GetExplicitMinimizedAreaExtent

        /// <summary>
        /// Gets an explicit extent for the minimized tile area
        /// </summary>
        /// <returns>An value that represents the width of the minimized area in maximized mode when MaximizedTileLocation is 'Left' or 'Right'. When MaximizedTileLocation is 'Top' or 'Bottom', it represents the height.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this method returns the default value of 0 then the indivdual tile preferred sizes will be used to determine the extent of this area.</para>
        /// </remarks>
        internal protected override double GetExplicitMinimizedAreaExtent()
        {
            return this.MinimizedAreaExplicitExtent;
        }

            #endregion //GetExplicitMinimizedAreaExtent	
    
            #region GetHorizontalTileAreaAlignment

        /// <summary>
        /// Determines the horizontal alignment of the complete block of visible tiles within the control.
        /// </summary>
        internal protected override HorizontalAlignment GetHorizontalTileAreaAlignment()
        {
            if ( this.IsInMaximizedMode )
                return this.MaximizedModeSettingsSafe.HorizontalTileAreaAlignment;
            else
                return this.NormalModeSettingsSafe.HorizontalTileAreaAlignment;
        }

            #endregion //GetHorizontalTileAreaAlignment	

            #region GetInterTileAreaSpacing

        /// <summary>
        /// Determines the amount of spacing between the maximized tile area and the miminized tile area when in maximized mode.
        /// </summary>
        internal protected override double GetInterTileAreaSpacing()
        {
            double spacing = this.InterTileAreaSpacingResolved;

            // if we are showing the splitter then make sure we allow room for it
            if (this._tilesControl != null && this.MaximizedModeSettingsSafe.ShowTileAreaSplitter)
                spacing = Math.Max(this._tilesControl.SplitterMinExtent, spacing);

            return spacing;
        }

            #endregion //GetInterTileAreaSpacing	

            #region GetInterTileSpacing

        /// <summary>
        /// Gets the amount of spacing between tiles in a specific state.
        /// </summary>
        /// <param name="vertical">True for vertical spacing, false for horzontal spacing.</param>
        /// <param name="state">The state of the tiles.</param>
        internal protected override double GetInterTileSpacing(bool vertical, TileState state)
        {
            switch (state)
            {
                case TileState.Normal:
                    if (vertical)
                        return this.InterTileSpacingY;
                    else
                        return this.InterTileSpacingX;

                case TileState.Maximized:
                    if (vertical)
                        return this.InterTileSpacingYMaximizedResolved;
                    else
                        return this.InterTileSpacingXMaximizedResolved;

                case TileState.Minimized:
                case TileState.MinimizedExpanded:
                    if (vertical)
                        return this.InterTileSpacingYMinimizedResolved;
                    else
                        return this.InterTileSpacingXMinimizedResolved;
            }

            return 0;
        }

            #endregion //GetInterTileSpacing	

            #region GetIsInMaximizedMode

        /// <summary>
        /// Returns true if there is at least one tile whose <see cref="TileState"/> is 'Maximized'.
        /// </summary>
        internal protected override bool GetIsInMaximizedMode()
        {
            return this.IsInMaximizedMode;
        }

            #endregion //GetIsInMaximizedMode	

            #region GetManager

        /// <summary>
        /// Gets the associated <see cref="TileManager"/>
        /// </summary>
        /// <returns>Must return a TileManager object</returns>
        internal protected override TileManager GetManager() { return this.Manager; }

            #endregion //GetManager	

            #region GetMaximizedItems

        /// <summary>
        /// Returns a read-only collection of the items that are maximized.
        /// </summary>
        internal protected override ObservableCollectionExtended<object> GetMaximizedItems()
        {
            return this._maximizedItems;
        }

            #endregion //GetMaximizedItems	

            #region GetMaximizedItems

        /// <summary>
        /// Returns the maximimum number of items allowed in the collection returned from <see cref="GetMaximizedItems()"/>
        /// </summary>
        internal protected override int GetMaximizedItemLimit()
        {
            return this.MaximizedTileLimit;
        }

            #endregion //GetMaximizedItems	

            #region GetMaximizedTileLocation

        /// <summary>
        /// Gets where the maximized tiles will be arranged relative to the minimized tiles.
        /// </summary>
        internal protected override MaximizedTileLocation GetMaximizedTileLocation()
        {
            return this.MaximizedModeSettingsSafe.MaximizedTileLocation;
        }

            #endregion //GetMaximizedTileLocation	

            #region GetMaximizedTileLayoutOrder

        /// <summary>
        /// Gets how multiple maximized tiles are laid out relative to one another
        /// </summary>
        internal protected override MaximizedTileLayoutOrder GetMaximizedTileLayoutOrder()
        {
            return this.MaximizedModeSettingsSafe.MaximizedTileLayoutOrder;
        }

            #endregion //GetMaximizedTileLayoutOrder	

            #region GetMin/Max/Columns/Rows

        /// <summary>
        /// Gets the maximum number of colums to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected override int GetMaxColumns()
        {
            return this.NormalModeSettingsSafe.MaxColumns;
        }

        /// <summary>
        /// Gets the maximum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected override int GetMaxRows()
        {
            return this.NormalModeSettingsSafe.MaxRows;
        }

        /// <summary>
        /// Gets the minimum number of colums to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected override int GetMinColumns()
        {
            return this.NormalModeSettingsSafe.MinColumns;
        }

        /// <summary>
        /// Gets the minimum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected override int GetMinRows()
        {
            return this.NormalModeSettingsSafe.MinRows;
        }

            #endregion //GetMin/Max/Columns/Rows	

            #region GetRepositionAnimation

        /// <summary>
        /// Determines how a tile> animates from one location to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="GetShouldAnimate()"/> returns 'False'.</para>
        /// </remarks>
        protected override DoubleAnimationBase GetRepositionAnimation()
        {
            if ( this.IsInMaximizedMode )
                return this.MaximizedModeSettingsSafe.RepositionAnimation;
            else
                return this.NormalModeSettingsSafe.RepositionAnimation;
        }

            #endregion //GetRepositionAnimation	

            #region GetResizeAnimation

        /// <summary>
        /// Determines how a tile> animates from one size to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="GetShouldAnimate()"/> returns 'False'.</para>
        /// </remarks>
        internal protected override DoubleAnimationBase GetResizeAnimation()
        {
            if ( this.IsInMaximizedMode )
                return this.MaximizedModeSettingsSafe.ResizeAnimation;
            else
                return this.NormalModeSettingsSafe.ResizeAnimation;
        }

            #endregion //GetResizeAnimation	

            #region GetShouldAnimate

        /// <summary>
        /// Gets/sets whether tiles will animate to their new position and size
        /// </summary>
        internal protected override bool GetShouldAnimate()
        {
            return this.ShouldAnimate;
        }

            #endregion //GetShouldAnimate	

			#region IsOkToCleanupUnusedGeneratedElements

		/// <summary>
		/// Called when elements are about to be cleaned up.  Return true to allow cleanup, false to prevent cleanup.
		/// </summary>
		protected override bool IsOkToCleanupUnusedGeneratedElements
		{
			get	{ return this.IsAnimationInProgress == false; }
		}

			#endregion IsOkToCleanupUnusedGeneratedElements
    
			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
    
            if ( this._itemsControl == null  &&
                this.IsItemsHost )
            {
                this._itemsControl = ItemsControl.GetItemsOwner(this);

                XamTilesControl tc = this._itemsControl as XamTilesControl;

                if ( tc != null )
                    tc.InitializeTilesPanel(this);
            }

            Size desiredSize = base.MeasureOverride(availableSize);

            if (this._tilesControl != null &&
                 this.IsInMaximizedMode &&
                 this.MaximizedModeSettingsSafe.ShowTileAreaSplitter)
                this._tilesControl.InvalidateArrange();

            return desiredSize;
        }

			#endregion //MeasureOverride	

            #region GetShowAllTiles

        /// <summary>
        /// Gets whether all tiles should be arranged in view
        /// </summary>
        internal protected override bool GetShowAllTiles()
        {
            return this.NormalModeSettingsSafe.ShowAllTiles;
        }

            #endregion //GetShowAllTiles

            #region GetShowAllMinimizedTiles

        /// <summary>
        /// Gets whether all minimized tiles should be arranged in view
        /// </summary>
        internal protected override bool GetShowAllMinimizedTiles()
        {
            return this.MaximizedModeSettingsSafe.ShowAllMinimizedTiles;
        }

            #endregion //GetShowAllMinimizedTiles

            #region GetSynchronizedSize

        /// <summary>
        /// Gets a size that will be used for all items
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this size is ignored in maximized mode or if <see cref="GetTileLayoutOrder()"/> returns 'UseExplicitRowColumnOnTile'. 
        /// Also, if TileLayoutOrder is 'HorizontalVariable' only the synchronized height will be used. Likewise, if it is 'VerticalVaraible' only
        /// the synchronized width will be used.</para>
        /// </remarks>
        /// <returns>The size to use for all items or null</returns>
        public override Size? GetSynchronizedSize()
        {
            return this.Manager.SynchronizedItemSize;
        }

            #endregion //GetSynchronizedSize	
    
            #region GetTileAreaPadding

        /// <summary>
        /// Get the amount of space between the panel and the area where the tiles are arranged.
        /// </summary>
        internal protected override Thickness GetTileAreaPadding()
        {
            return this.TileAreaPadding;
        }

            #endregion //GetTileAreaPadding

            #region GetTileLayoutOrder

        /// <summary>
        /// Determines how the panel will layout the tiles.
        /// </summary>
        internal protected override TileLayoutOrder GetTileLayoutOrder()
        {
            return this.NormalModeSettingsSafe.TileLayoutOrder;
        }

            #endregion //GetTileLayoutOrder

            #region GetVerticalTileAreaAlignment

        /// <summary>
        /// Determines the vertical alignment of the complete block of visible tiles within the control.
        /// </summary>
        internal protected override VerticalAlignment GetVerticalTileAreaAlignment()
        {
            if (this.IsInMaximizedMode)
                return this.MaximizedModeSettingsSafe.VerticalTileAreaAlignment;
            else
                return this.NormalModeSettingsSafe.VerticalTileAreaAlignment;
        }

            #endregion //GetVerticalTileAreaAlignment

            #region OnAnimationEnded

        /// <summary>
        /// Called when animations have completed on a specific container
        /// </summary>
        /// <param name="container"></param>
        protected override void OnAnimationEnded(DependencyObject container) 
        {
            Tile tile = container as Tile;

            if (tile != null)
                tile.OnAnimationEnded();
        }

            #endregion //OnAnimationEnded

            #region OnContainersSwapped

        /// <summary>
        /// Called when a container dropped on another to swap it.
        /// </summary>
        /// <param name="dragItemContainer">The container of the item that was dragged.</param>
        /// <param name="dragItemInfo">The associated drag item's info</param>
        /// <param name="swapTargetContainer">The container that is the target of the swap.</param>
        /// <param name="swapTargetItemInfo">The associated swap target item's info</param>
        internal protected override void OnContainersSwapped(FrameworkElement dragItemContainer, ItemInfoBase dragItemInfo, FrameworkElement swapTargetContainer, ItemInfoBase swapTargetItemInfo)
        {
            ItemInfo sourceInfo = dragItemInfo as ItemInfo;
            ItemInfo targetInfo = swapTargetItemInfo as ItemInfo;

            Debug.Assert(sourceInfo != null);
            Debug.Assert(targetInfo != null);

            if (sourceInfo == null ||
                 targetInfo == null)
                return;

            // if the IsMaximized states are different we need to do an atomic swap of 
            // the 
            if (sourceInfo.IsMaximized != targetInfo.IsMaximized)
            {
                Tile maximizedTile;
                Tile minimizedTile;
                ItemInfo maximizedInfo;
                ItemInfo minimzedInfo;

                if (sourceInfo.IsMaximized)
                {
                    maximizedTile = dragItemContainer as Tile;
                    minimizedTile = swapTargetContainer as Tile;
                    maximizedInfo = sourceInfo;
                    minimzedInfo = targetInfo;
                }
                else
                {
                    maximizedTile = swapTargetContainer as Tile;
                    minimizedTile = dragItemContainer as Tile;
                    maximizedInfo = targetInfo;
                    minimzedInfo = sourceInfo;
                }

                if (maximizedTile == null ||
                     minimizedTile == null)
                    return;

                int maximizedIndex = this._maximizedItems.IndexOf(maximizedInfo.Item);

                Debug.Assert(maximizedIndex >= 0, "Maximized item must be in the collection");

                if (maximizedIndex < 0)
                    return;

                this._isSettingTileState = true;

                try
                {
                    this._maximizedItems.BeginUpdate();
                    this._maximizedItems[maximizedIndex] = minimzedInfo.Item;

					// JJD 11/1/11 - TFS88171 
					// Either swap the isexpandedWhenMinimized state by using the minimized tile state directly
					// or set its state based on its IsExpandedWhenMinimizedResolved   
					if (_swapinfo._swapIsExpandedWhenMinimized)
					{
						bool? temp = maximizedInfo.IsExpandedWhenMinimized;
						maximizedInfo.IsExpandedWhenMinimized = minimzedInfo.IsExpandedWhenMinimized;
						minimzedInfo.IsExpandedWhenMinimized = temp;
						maximizedTile.State = minimizedTile.State;
					}
					else
						maximizedTile.State = maximizedInfo.IsExpandedWhenMinimizedResolved ? TileState.MinimizedExpanded : TileState.Minimized;

					minimizedTile.State = TileState.Maximized;

                    maximizedInfo.SyncFromTileState(maximizedTile);
                    minimzedInfo.SyncFromTileState(minimizedTile);
                }
                finally
                {
                    this._isSettingTileState = false;
                    this._maximizedItems.EndUpdate();
                }
            }
			else
			{
				// JJD 11/1/11 - TFS88171 
				// If _swapIsExpandedWhenMinimized was explicitly set in the event args then swap the isexpandedWhenMinimized settings
				if (_swapinfo._swapIsExpandedWhenMinimized)
				{
					sourceInfo.IsExpandedWhenMinimized = _swapinfo._targetIsExpandedWhenMinimized;
					targetInfo.IsExpandedWhenMinimized = _swapinfo._sourceIsExpandedWhenMinimized;
				}
			}
			
			// JJD 11/1/11 - TFS88171 
			// clear the cached swap info
			_swapinfo = null;

            TileSwappedEventArgs args = new TileSwappedEventArgs(dragItemContainer as Tile, dragItemInfo.Item, swapTargetContainer as Tile, swapTargetItemInfo.Item);

            // raise the TileSwapped event
            this.RaiseTileSwapped(args);
        }

            #endregion //OnContainersSwapped	

			#region OnItemsChanged
		/// <summary>
		/// Called when the contents of the associated list changes.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="args">An instance of ItemsChangedEventArgs that contains information about the items that were changed.</param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			base.OnItemsChanged(sender, args);

			// JJD 8/17/10 - TFS36079
			// On a Reset notification if we are bound (i.e. the XamTileControl's 
			// ItemSource property is set) we need to refreash the DataContext, Content and
			// ContentTemplateSelector properties on each container (tile)
			// The reason for this is that with certain DataSources, e.g. DataView and DataTable,
			// it is possible that the cell values have changed without individual row
			// notifications being sent out. In this case bindings would not update automatically
			// without the DataContext, Content and ContentTemplateSelectorproperties being refreshed.
			if (args.Action == NotifyCollectionChangedAction.Reset &&
				this._tilesControl != null &&
				this._tilesControl.ItemsSource != null)
			{
				IList children = this.ChildElements;

				for (int i = 0; i < children.Count; i++)
				{
					Tile tile = children[i] as Tile;

					if (tile != null)
					{
						object dc = tile.DataContext;
						DataTemplateSelector selector = tile.ContentTemplateSelector;
						object content = tile.Content;

						if (dc != null)
							tile.DataContext = null;

						if (content != null)
							tile.Content = null;

						if (selector != null)
							tile.ContentTemplateSelector = null;

						if (dc != null)
							tile.DataContext = dc;

						if (selector != null)
							tile.ContentTemplateSelector = selector;

						if (content != null)
							tile.Content = content;
					}
				}
			}
		}

			#endregion //OnItemsChanged	

			// JJD 07/12/12 - TFS112221 - added
			#region OnTileDragEnd

		/// <summary>
		/// Called when a tile drag operation ends.
		/// </summary>
		/// <param name="container"></param>
		protected override void OnTileDragEnd(FrameworkElement container)
		{
			// JJD 07/12/12 - TFS112221
			// Maintain a flag that keep track of whether we are in th middle of a drag opewration
			_IsDragging = false;

			base.OnTileDragEnd(container);
		}

			#endregion //OnTileDragEnd	
    
			// JJD 07/12/12 - TFS112221 - added
			#region OnTileDragStart

		/// <summary>
		/// Called when a tile drag operation starts
		/// </summary>
		/// <param name="container"></param>
		protected override void OnTileDragStart(FrameworkElement container)
		{
			// JJD 07/12/12 - TFS112221
			// Maintain a flag that keep track of whether we are in th middle of a drag opewration
			_IsDragging = true;

			base.OnTileDragStart(container);
		}

			#endregion //OnTileDragStart	
			
			// JJD 4/19/11 - TFS73129  added
			#region OnItemsInViewChanged

		/// <summary>
		/// Called when animations have completed after the items in view have changed
		/// </summary>
		protected override void OnItemsInViewChanged()
		{
			base.OnItemsInViewChanged();

			if (_tilesControl != null)
				_tilesControl.SyncIsAnimationInProgress();
		}

			#endregion //OnItemsInViewChanged	
    
            #region UpdateTransform

        /// <summary>
        /// Called during animations to reposition, resize elements.
        /// </summary>
        /// <remarks>
        /// <para clas="note"><b>Note:</b> derived classeds must override this method to update the RenderTransform for the container</para>
        /// </remarks>
        /// <param name="container">The element being moved.</param>
        /// <param name="originalRect">The original location of the element before the animation started.</param>
        /// <param name="currentRect">The current location of the element.</param>
        /// <param name="targetRect">The target location of the element once the animation has completed.</param>
        /// <param name="offset">Any addition offset to apply to the current rect.</param>
        /// <param name="resizeFactor">A number used during a resize animation where 0 repreents the starting size and 1 represents the ending size.</param>
        /// <param name="calledFromArrange">True is called during the initial arrange pass.</param>
        
        protected override void UpdateTransform(DependencyObject container, Rect originalRect, Rect currentRect, Rect targetRect, Vector offset, double resizeFactor, bool calledFromArrange)
        {
			// JJD 4/19/11 - TFS73129  added
			if (_tilesControl != null)
				_tilesControl.SyncIsAnimationInProgress();
			
			Tile tile = container as Tile;

            if (tile != null)
                tile.UpdateTransform(originalRect, currentRect, targetRect, offset, resizeFactor, calledFromArrange);
            else
                base.UpdateTransform(container, originalRect, currentRect, targetRect, offset, resizeFactor, calledFromArrange);
        }

            #endregion //UpdateTransform	

            #region VerifyMaximizedItems

        /// <summary>
        /// Called to make sure the list returned from <see cref="GetMaximizedItems()"/> is in synch with the items collection.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note: </b>Derived classes that support maximized tiles should override this method and ensure that the MaximizedItems collection is in synch.</para>
        /// </remarks>
        override internal protected void VerifyMaximizedItems()
        {
            int count = this._maximizedItems.Count;

            if (count < 0)
                return;

            bool wereItemsRemoved = false;

            try
            {
                TileManager manager = this.Manager;

                for (int i = count - 1; i >= 0; i--)
                {
                    object item = this._maximizedItems[i];

                    ItemInfo info = (ItemInfo)manager.GetItemInfo(item, false, -1);

                    Debug.Assert(info != null, "There should be an info object for all maximized items");

                    // if the index is no longer valid then remove the item
                    if (info == null ||
                        info.Index < 0)
                    {
                        if (wereItemsRemoved == false)
                        {
                            wereItemsRemoved = true;
                            this._maximizedItems.BeginUpdate();
                        }
                        this._maximizedItems.RemoveAt(i);
                    }
                }
            }
            finally
            {
                if (wereItemsRemoved)
                {
                    this._maximizedItems.EndUpdate();

                    bool isInMax = this.IsInMaximizedMode;

                    
                    
                    
                    
                    this.VerifyIsInMaximizedModeProperty();

                    
                    // if IsInMaximizedMode changed then bump the layou versions
                    if (isInMax != this.IsInMaximizedMode)
                        this.BumpLayoutVersion();
                }
            }

        }

           #endregion //VerifyMaximizedItems
    
		#endregion //Base Class Overrides

        #region Events

            #region TileClosed

        /// <summary>
        /// Event ID for the <see cref="TileClosed"/> routed event
        /// </summary>
        /// <seealso cref="TileClosed"/>
        /// <seealso cref="OnTileClosed"/>
        /// <seealso cref="TileClosedEventArgs"/>
        public static readonly RoutedEvent TileClosedEvent =
            EventManager.RegisterRoutedEvent("TileClosed", RoutingStrategy.Bubble, typeof(EventHandler<TileClosedEventArgs>), typeof(TilesPanel));

        /// <summary>
        /// Occurs after a <see cref="Tile"/> has been closed.
        /// </summary>
        /// <seealso cref="TileClosed"/>
        /// <seealso cref="TileClosedEvent"/>
        /// <seealso cref="TileClosedEventArgs"/>
        protected virtual void OnTileClosed(TileClosedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseTileClosed(TileClosedEventArgs args)
        {
            args.RoutedEvent = TilesPanel.TileClosedEvent;
            args.Source = this;
            this.OnTileClosed(args);
        }

        /// <summary>
        /// Occurs after a <see cref="Tile"/> has been closed.
        /// </summary>
        /// <seealso cref="OnTileClosed"/>
        /// <seealso cref="TileClosedEvent"/>
        /// <seealso cref="TileClosedEventArgs"/>
        //[Description("Occurs after the Tile has been closed.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileClosedEventArgs> TileClosed
        {
            add
            {
                base.AddHandler(TilesPanel.TileClosedEvent, value);
            }
            remove
            {
                base.RemoveHandler(TilesPanel.TileClosedEvent, value);
            }
        }

            #endregion //TileClosed

            #region TileClosing

        /// <summary>
        /// Event ID for the <see cref="TileClosing"/> routed event
        /// </summary>
        /// <seealso cref="TileClosing"/>
        /// <seealso cref="OnTileClosing"/>
        /// <seealso cref="TileClosingEventArgs"/>
        public static readonly RoutedEvent TileClosingEvent =
            EventManager.RegisterRoutedEvent("TileClosing", RoutingStrategy.Bubble, typeof(EventHandler<TileClosingEventArgs>), typeof(TilesPanel));

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is about to close.
        /// </summary>
        /// <seealso cref="TileClosing"/>
        /// <seealso cref="TileClosingEvent"/>
        /// <seealso cref="TileClosingEventArgs"/>
        protected virtual void OnTileClosing(TileClosingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseTileClosing(TileClosingEventArgs args)
        {
            args.RoutedEvent = TilesPanel.TileClosingEvent;
            args.Source = this;
            this.OnTileClosing(args);
        }

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is about to close.
        /// </summary>
        /// <seealso cref="OnTileClosing"/>
        /// <seealso cref="TileClosingEvent"/>
        /// <seealso cref="TileClosingEventArgs"/>
        //[Description("Occurs when a Tile is about to close.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileClosingEventArgs> TileClosing
        {
            add
            {
                base.AddHandler(TilesPanel.TileClosingEvent, value);
            }
            remove
            {
                base.RemoveHandler(TilesPanel.TileClosingEvent, value);
            }
        }

            #endregion //TileClosing

            #region TileDragging

        /// <summary>
        /// Event ID for the <see cref="TileDragging"/> routed event
        /// </summary>
        /// <seealso cref="TileDragging"/>
        /// <seealso cref="OnTileDragging"/>
        /// <seealso cref="TileDraggingEventArgs"/>
        public static readonly RoutedEvent TileDraggingEvent =
            EventManager.RegisterRoutedEvent("TileDragging", RoutingStrategy.Bubble, typeof(EventHandler<TileDraggingEventArgs>), typeof(TilesPanel));

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is about to be dragged.
        /// </summary>
        /// <seealso cref="TileDragging"/>
        /// <seealso cref="TileDraggingEvent"/>
        /// <seealso cref="TileDraggingEventArgs"/>
        protected virtual void OnTileDragging(TileDraggingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseTileDragging(TileDraggingEventArgs args)
        {
            args.RoutedEvent = TilesPanel.TileDraggingEvent;
            args.Source = this;
            this.OnTileDragging(args);
        }

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is about to be dragged
        /// </summary>
        /// <seealso cref="OnTileDragging"/>
        /// <seealso cref="TileDraggingEvent"/>
        /// <seealso cref="TileDraggingEventArgs"/>
        //[Description("Occurs when a Tile is about to be dragged.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileDraggingEventArgs> TileDragging
        {
            add
            {
                base.AddHandler(TilesPanel.TileDraggingEvent, value);
            }
            remove
            {
                base.RemoveHandler(TilesPanel.TileDraggingEvent, value);
            }
        }

            #endregion //TileDragging

            #region TileStateChanged

        /// <summary>
        /// Event ID for the <see cref="TileStateChanged"/> routed event
        /// </summary>
        /// <seealso cref="TileStateChanged"/>
        /// <seealso cref="OnTileStateChanged"/>
        /// <seealso cref="TileStateChangedEventArgs"/>
        public static readonly RoutedEvent TileStateChangedEvent =
            EventManager.RegisterRoutedEvent("TileStateChanged", RoutingStrategy.Bubble, typeof(EventHandler<TileStateChangedEventArgs>), typeof(TilesPanel));

        /// <summary>
        /// Occurs after the state of a <see cref="Tile"/> has changed.
        /// </summary>
        /// <seealso cref="TileStateChanged"/>
        /// <seealso cref="TileStateChangedEvent"/>
        /// <seealso cref="TileStateChangedEventArgs"/>
        protected virtual void OnTileStateChanged(TileStateChangedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseTileStateChanged(TileStateChangedEventArgs args)
        {
            args.RoutedEvent = TilesPanel.TileStateChangedEvent;
            args.Source = this;
            this.OnTileStateChanged(args);
        }

        /// <summary>
        /// Occurs after the state of a <see cref="Tile"/> has changed.
        /// </summary>
        /// <seealso cref="OnTileStateChanged"/>
        /// <seealso cref="TileStateChangedEvent"/>
        /// <seealso cref="TileStateChangedEventArgs"/>
        //[Description("Occurs after the state of a Tile has changed.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileStateChangedEventArgs> TileStateChanged
        {
            add
            {
                base.AddHandler(TilesPanel.TileStateChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(TilesPanel.TileStateChangedEvent, value);
            }
        }

            #endregion //TileStateChanged

            #region TileStateChanging

        /// <summary>
        /// Event ID for the <see cref="TileStateChanging"/> routed event
        /// </summary>
        /// <seealso cref="TileStateChanging"/>
        /// <seealso cref="OnTileStateChanging"/>
        /// <seealso cref="TileStateChangingEventArgs"/>
        public static readonly RoutedEvent TileStateChangingEvent =
            EventManager.RegisterRoutedEvent("TileStateChanging", RoutingStrategy.Bubble, typeof(EventHandler<TileStateChangingEventArgs>), typeof(TilesPanel));

        /// <summary>
        /// Occurs when the state of a <see cref="Tile"/> is about to change.
        /// </summary>
        /// <seealso cref="TileStateChanging"/>
        /// <seealso cref="TileStateChangingEvent"/>
        /// <seealso cref="TileStateChangingEventArgs"/>
        protected virtual void OnTileStateChanging(TileStateChangingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseTileStateChanging(TileStateChangingEventArgs args)
        {
            args.RoutedEvent = TilesPanel.TileStateChangingEvent;
            args.Source = this;
            this.OnTileStateChanging(args);
        }

        /// <summary>
        /// Occurs when the state of a <see cref="Tile"/> is about to change.
        /// </summary>
        /// <seealso cref="OnTileStateChanging"/>
        /// <seealso cref="TileStateChangingEvent"/>
        /// <seealso cref="TileStateChangingEventArgs"/>
        //[Description("Occurs when the state of a Tile is about to change.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileStateChangingEventArgs> TileStateChanging
        {
            add
            {
                base.AddHandler(TilesPanel.TileStateChangingEvent, value);
            }
            remove
            {
                base.RemoveHandler(TilesPanel.TileStateChangingEvent, value);
            }
        }

            #endregion //TileStateChanging

            #region TileSwapping

        /// <summary>
        /// Event ID for the <see cref="TileSwapping"/> routed event
        /// </summary>
        /// <seealso cref="TileSwapping"/>
        /// <seealso cref="OnTileSwapping"/>
        /// <seealso cref="TileSwappingEventArgs"/>
        public static readonly RoutedEvent TileSwappingEvent =
            EventManager.RegisterRoutedEvent("TileSwapping", RoutingStrategy.Bubble, typeof(EventHandler<TileSwappingEventArgs>), typeof(TilesPanel));

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is dragged over another tile that is a potential swap target.
        /// </summary>
        /// <seealso cref="TileSwapping"/>
        /// <seealso cref="TileSwappingEvent"/>
        /// <seealso cref="TileSwappingEventArgs"/>
        protected virtual void OnTileSwapping(TileSwappingEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseTileSwapping(TileSwappingEventArgs args)
        {
            args.RoutedEvent = TilesPanel.TileSwappingEvent;
            args.Source = this;
            this.OnTileSwapping(args);
        }

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is dragged over another tile that is a potential swap target
        /// </summary>
        /// <seealso cref="OnTileSwapping"/>
        /// <seealso cref="TileSwappingEvent"/>
        /// <seealso cref="TileSwappingEventArgs"/>
        //[Description("Occurs when a Tile is dragged over another tile that is a potential swap target.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileSwappingEventArgs> TileSwapping
        {
            add
            {
                base.AddHandler(TilesPanel.TileSwappingEvent, value);
            }
            remove
            {
                base.RemoveHandler(TilesPanel.TileSwappingEvent, value);
            }
        }

            #endregion //TileSwapping

            #region TileSwapped

        /// <summary>
        /// Event ID for the <see cref="TileSwapped"/> routed event
        /// </summary>
        /// <seealso cref="TileSwapped"/>
        /// <seealso cref="OnTileSwapped"/>
        /// <seealso cref="TileSwappedEventArgs"/>
        public static readonly RoutedEvent TileSwappedEvent =
            EventManager.RegisterRoutedEvent("TileSwapped", RoutingStrategy.Bubble, typeof(EventHandler<TileSwappedEventArgs>), typeof(TilesPanel));

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is dropped over another tile and swaps places with it.
        /// </summary>
        /// <seealso cref="TileSwapped"/>
        /// <seealso cref="TileSwappedEvent"/>
        /// <seealso cref="TileSwappedEventArgs"/>
        protected virtual void OnTileSwapped(TileSwappedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseTileSwapped(TileSwappedEventArgs args)
        {
            args.RoutedEvent = TilesPanel.TileSwappedEvent;
            args.Source = this;
            this.OnTileSwapped(args);
        }

        /// <summary>
        /// Occurs when a <see cref="Tile"/> is dropped over another tile and swaps places with it
        /// </summary>
        /// <seealso cref="OnTileSwapped"/>
        /// <seealso cref="TileSwappedEvent"/>
        /// <seealso cref="TileSwappedEventArgs"/>
        //[Description("Occurs when a Tile  is dropped over another tile and swaps places with it.")]
        //[Category("TilesControl Events")]  
        public event EventHandler<TileSwappedEventArgs> TileSwapped
        {
            add
            {
                base.AddHandler(TilesPanel.TileSwappedEvent, value);
            }
            remove
            {
                base.RemoveHandler(TilesPanel.TileSwappedEvent, value);
            }
        }

            #endregion //TileSwapped

        #endregion //Events	

        #region Properties
        
            #region Public Attached Properties

                #region Constraints

        /// <summary>
        /// Identifies the Constraints attached dependency property
        /// </summary>
        /// <seealso cref="GetConstraints"/>
        /// <seealso cref="SetConstraints"/>
        public static readonly DependencyProperty ConstraintsProperty = DependencyProperty.RegisterAttached("Constraints",
            typeof(ITileConstraints), typeof(TilesPanel), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'Constraints' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Normal'.
        /// </summary>
        /// <seealso cref="ConstraintsProperty"/>
        /// <seealso cref="SetConstraints"/>
        [AttachedPropertyBrowsableForChildren()]
        public static ITileConstraints GetConstraints(DependencyObject d)
        {
            return (ITileConstraints)d.GetValue(TilesPanel.ConstraintsProperty);
        }

        /// <summary>
        /// Sets the value of the 'Constraints' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Normal'.
        /// </summary>
        /// <seealso cref="ConstraintsProperty"/>
        /// <seealso cref="GetConstraints"/>
        public static void SetConstraints(DependencyObject d, ITileConstraints value)
        {
            d.SetValue(TilesPanel.ConstraintsProperty, value);
        }

                #endregion //Constraints

                #region ConstraintsMaximized

        /// <summary>
        /// Identifies the ConstraintsMaximized attached dependency property
        /// </summary>
        /// <seealso cref="GetConstraintsMaximized"/>
        /// <seealso cref="SetConstraintsMaximized"/>
        public static readonly DependencyProperty ConstraintsMaximizedProperty = DependencyProperty.RegisterAttached("ConstraintsMaximized",
            typeof(ITileConstraints), typeof(TilesPanel), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'ConstraintsMaximized' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Maximized'.
        /// </summary>
        /// <seealso cref="ConstraintsMaximizedProperty"/>
        /// <seealso cref="SetConstraintsMaximized"/>
        [AttachedPropertyBrowsableForChildren()]
        public static ITileConstraints GetConstraintsMaximized(DependencyObject d)
        {
            return (ITileConstraints)d.GetValue(TilesPanel.ConstraintsMaximizedProperty);
        }

        /// <summary>
        /// Sets the value of the 'ConstraintsMaximized' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Maximized'.
        /// </summary>
        /// <seealso cref="ConstraintsMaximizedProperty"/>
        /// <seealso cref="GetConstraintsMaximized"/>
        public static void SetConstraintsMaximized(DependencyObject d, ITileConstraints value)
        {
            d.SetValue(TilesPanel.ConstraintsMaximizedProperty, value);
        }

                #endregion //ConstraintsMaximized

                #region ConstraintsMinimized

        /// <summary>
        /// Identifies the ConstraintsMinimized attached dependency property
        /// </summary>
        /// <seealso cref="GetConstraintsMinimized"/>
        /// <seealso cref="SetConstraintsMinimized"/>
        public static readonly DependencyProperty ConstraintsMinimizedProperty = DependencyProperty.RegisterAttached("ConstraintsMinimized",
            typeof(ITileConstraints), typeof(TilesPanel), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'ConstraintsMinimized' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Minimized'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedProperty"/>
        /// <seealso cref="SetConstraintsMinimized"/>
        [AttachedPropertyBrowsableForChildren()]
        public static ITileConstraints GetConstraintsMinimized(DependencyObject d)
        {
            return (ITileConstraints)d.GetValue(TilesPanel.ConstraintsMinimizedProperty);
        }

        /// <summary>
        /// Sets the value of the 'ConstraintsMinimized' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'Minimized'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedProperty"/>
        /// <seealso cref="GetConstraintsMinimized"/>
        public static void SetConstraintsMinimized(DependencyObject d, ITileConstraints value)
        {
            d.SetValue(TilesPanel.ConstraintsMinimizedProperty, value);
        }

                #endregion //ConstraintsMinimized

                #region ConstraintsMinimizedExpanded

        /// <summary>
        /// Identifies the ConstraintsMinimizedExpanded attached dependency property
        /// </summary>
        /// <seealso cref="GetConstraintsMinimizedExpanded"/>
        /// <seealso cref="SetConstraintsMinimizedExpanded"/>
        public static readonly DependencyProperty ConstraintsMinimizedExpandedProperty = DependencyProperty.RegisterAttached("ConstraintsMinimizedExpanded",
            typeof(ITileConstraints), typeof(TilesPanel), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'ConstraintsMinimizedExpanded' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'MinimizedExpanded'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedExpandedProperty"/>
        /// <seealso cref="SetConstraintsMinimizedExpanded"/>
        [AttachedPropertyBrowsableForChildren()]
        public static ITileConstraints GetConstraintsMinimizedExpanded(DependencyObject d)
        {
            return (ITileConstraints)d.GetValue(TilesPanel.ConstraintsMinimizedExpandedProperty);
        }

        /// <summary>
        /// Sets the value of the 'ConstraintsMinimizedExpanded' attached property which contains size constraints for tiles when their <see cref="Tile.State"/> is 'MinimizedExpanded'.
        /// </summary>
        /// <seealso cref="ConstraintsMinimizedExpandedProperty"/>
        /// <seealso cref="GetConstraintsMinimizedExpanded"/>
        public static void SetConstraintsMinimizedExpanded(DependencyObject d, ITileConstraints value)
        {
            d.SetValue(TilesPanel.ConstraintsMinimizedExpandedProperty, value);
        }

                #endregion //ConstraintsMinimizedExpanded

                #region SerializationId

        /// <summary>
        /// Identifies the SerializationId attached dependency property
        /// </summary>
        /// <seealso cref="GetSerializationId"/>
        /// <seealso cref="SetSerializationId"/>
        public static readonly DependencyProperty SerializationIdProperty = DependencyProperty.RegisterAttached("SerializationId",
            typeof(string), typeof(TilesPanel), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the value of the 'SerializationId' attached property
        /// </summary>
        /// <seealso cref="SerializationIdProperty"/>
        /// <seealso cref="SetSerializationId"/>
        public static string GetSerializationId(DependencyObject d)
        {
            return (string)d.GetValue(TilesPanel.SerializationIdProperty);
        }

        /// <summary>
        /// Sets the value of the 'SerializationId' attached property
        /// </summary>
        /// <seealso cref="SerializationIdProperty"/>
        /// <seealso cref="GetSerializationId"/>
        public static void SetSerializationId(DependencyObject d, string value)
        {
            d.SetValue(TilesPanel.SerializationIdProperty, value);
        }

                #endregion //SerializationId

            #endregion //Public Attached Properties

            #region Public Properties

                #region InterTileAreaSpacingResolved

        internal static readonly DependencyPropertyKey InterTileAreaSpacingResolvedPropertyKey =
            DependencyProperty.RegisterReadOnly("InterTileAreaSpacingResolved",
            typeof(double), typeof(TilesPanel), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnInterTileAreaSpacingResolvedChanged)));

        /// <summary>
        /// Identifies the <see cref="InterTileAreaSpacingResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileAreaSpacingResolvedProperty =
            InterTileAreaSpacingResolvedPropertyKey.DependencyProperty;

        private static void OnInterTileAreaSpacingResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            panel._interTileAreaSpacingResolved = (double)e.NewValue;

            if ( panel.IsInMaximizedMode )
                panel.InvalidateMeasure();
        }

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the maximized tile area and the minimized tile area when is maximized mode.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileAreaSpacing"/> property is not set then the <see cref="InterTileSpacingXMaximizedResolved"/> or <see cref="InterTileSpacingYMaximizedResolved"/> setting will be used. If this also is not set then the <see cref="InterTileSpacingX"/> or <see cref="InterTileSpacingY"/> value will be used based on the <see cref="MaximizedTileLocation"/>.</para>
        /// </remarks>
        /// <seealso cref="MaximizedModeSettings"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileAreaSpacing"/>
        /// <seealso cref="TilesPanel.IsInMaximizedMode"/>
        /// <seealso cref="TilesPanel.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public double InterTileAreaSpacingResolved { get { return this._interTileAreaSpacingResolved; } }

                #endregion //InterTileAreaSpacingResolved	

                #region InterTileSpacingX

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingX"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingXProperty = DependencyProperty.Register("InterTileSpacingX",
            typeof(double), typeof(TilesPanel), new FrameworkPropertyMetadata(2.0d, new PropertyChangedCallback(OnInterTileSpacingXChanged)), new ValidateValueCallback(ValidateInterTileSpacing));
        
        private static bool ValidateInterTileSpacing(object objVal)
        {
            double val = (double)objVal;

            if (double.IsNaN(val) || double.IsInfinity(val))
                return false;

            return val >= 0d;
        }

        private static void OnInterTileSpacingXChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            panel._interTileSpacingX = (double)e.NewValue;
            panel.InvalidateMeasure();
            panel.CalculateResolvedSpacing();
        }

        /// <summary>
        /// Gets/sets the amount of spacing between tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> the default value for this property is 2.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingXProperty"/>
        //[Description("Gets/sets the amount of spacing between tiles horizontally.")]
        //[Category("TilesControl Properties")]
        public double InterTileSpacingX
        {
            get
            {
                return this._interTileSpacingX;
            }
            set
            {
                this.SetValue(TilesPanel.InterTileSpacingXProperty, value);
            }
        }

                #endregion //InterTileSpacingX

                #region InterTileSpacingY

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingY"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingYProperty = DependencyProperty.Register("InterTileSpacingY",
            typeof(double), typeof(TilesPanel), new FrameworkPropertyMetadata(2.0d, new PropertyChangedCallback(OnInterTileSpacingYChanged)), new ValidateValueCallback(ValidateInterTileSpacing));
        

        private static void OnInterTileSpacingYChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            panel._interTileSpacingY = (double)e.NewValue;
            panel.InvalidateMeasure();
            panel.CalculateResolvedSpacing();
        }

        /// <summary>
        /// Gets/sets the amount of spacing between tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> the default value for this property is 2.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingYProperty"/>
        //[Description("Gets/sets the amount of spacing between tiles horizontally.")]
        //[Category("TilesControl Properties")]
        public double InterTileSpacingY
        {
            get
            {
                return this._interTileSpacingY;
            }
            set
            {
                this.SetValue(TilesPanel.InterTileSpacingYProperty, value);
            }
        }

                #endregion //InterTileSpacingY

                #region InterTileSpacingXMaximizedResolved

        internal static readonly DependencyPropertyKey InterTileSpacingXMaximizedResolvedPropertyKey =
            DependencyProperty.RegisterReadOnly("InterTileSpacingXMaximizedResolved",
            typeof(double), typeof(TilesPanel), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnInterTileSpacingXMaximizedResolvedChanged)));

        private static void OnInterTileSpacingXMaximizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            panel._interTileSpacingXMaximizedResolved = (double)e.NewValue;

            if ( panel.IsInMaximizedMode )
                panel.InvalidateMeasure();
        }

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingXMaximizedResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingXMaximizedResolvedProperty =
            InterTileSpacingXMaximizedResolvedPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the maximized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingXMaximized"/> property is not set then the <see cref="InterTileSpacingX"/> value will be used.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingX"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingXMaximized"/>
        /// <seealso cref="TilesPanel.IsInMaximizedMode"/>
        /// <seealso cref="TilesPanel.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
         public double InterTileSpacingXMaximizedResolved { get { return this._interTileSpacingXMaximizedResolved; } }

                #endregion //InterTileSpacingXMaximizedResolved	

                #region InterTileSpacingXMinimizedResolved

        internal static readonly DependencyPropertyKey InterTileSpacingXMinimizedResolvedPropertyKey =
            DependencyProperty.RegisterReadOnly("InterTileSpacingXMinimizedResolved",
            typeof(double), typeof(TilesPanel), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnInterTileSpacingXMinimizedResolvedChanged)));

        private static void OnInterTileSpacingXMinimizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            panel._interTileSpacingXMinimizedResolved = (double)e.NewValue;

            if ( panel.IsInMaximizedMode )
                panel.InvalidateMeasure();
        }

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingXMinimizedResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingXMinimizedResolvedProperty =
            InterTileSpacingXMinimizedResolvedPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the minimized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingXMinimized"/> property is not set then the <see cref="InterTileSpacingX"/> value will be used.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingX"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingXMaximized"/>
        /// <seealso cref="TilesPanel.IsInMaximizedMode"/>
        /// <seealso cref="TilesPanel.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public double InterTileSpacingXMinimizedResolved { get { return this._interTileSpacingXMinimizedResolved; } }

                #endregion //InterTileSpacingXMinimizedResolved

                #region InterTileSpacingYMaximizedResolved

        internal static readonly DependencyPropertyKey InterTileSpacingYMaximizedResolvedPropertyKey =
            DependencyProperty.RegisterReadOnly("InterTileSpacingYMaximizedResolved",
            typeof(double), typeof(TilesPanel), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnInterTileSpacingYMaximizedResolvedChanged)));

        private static void OnInterTileSpacingYMaximizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            panel._interTileSpacingYMaximizedResolved = (double)e.NewValue;

            if ( panel.IsInMaximizedMode )
                panel.InvalidateMeasure();
        }

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingYMaximizedResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingYMaximizedResolvedProperty =
            InterTileSpacingYMaximizedResolvedPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the maximized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingYMaximized"/> property is not set then the <see cref="InterTileSpacingY"/> value will be used.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingY"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingYMaximized"/>
        /// <seealso cref="TilesPanel.IsInMaximizedMode"/>
        /// <seealso cref="TilesPanel.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
         public double InterTileSpacingYMaximizedResolved { get { return this._interTileSpacingYMaximizedResolved; } }

                #endregion //InterTileSpacingYMaximizedResolved	

                #region InterTileSpacingYMinimizedResolved

        internal static readonly DependencyPropertyKey InterTileSpacingYMinimizedResolvedPropertyKey =
            DependencyProperty.RegisterReadOnly("InterTileSpacingYMinimizedResolved",
            typeof(double), typeof(TilesPanel), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnInterTileSpacingYMinimizedResolvedChanged)));

        private static void OnInterTileSpacingYMinimizedResolvedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            panel._interTileSpacingYMinimizedResolved = (double)e.NewValue;

            if ( panel.IsInMaximizedMode )
                panel.InvalidateMeasure();
        }

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingYMinimizedResolved"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingYMinimizedResolvedProperty =
            InterTileSpacingYMinimizedResolvedPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the resolved value that will be used for spacing between the minimized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if the <see cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingYMinimized"/> property is not set then the <see cref="InterTileSpacingY"/> value will be used.</para>
        /// </remarks>
        /// <seealso cref="InterTileSpacingY"/>
        /// <seealso cref="Infragistics.Windows.Tiles.MaximizedModeSettings.InterTileSpacingYMaximized"/>
        /// <seealso cref="TilesPanel.IsInMaximizedMode"/>
        /// <seealso cref="TilesPanel.MaximizedItems"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public double InterTileSpacingYMinimizedResolved { get { return this._interTileSpacingYMinimizedResolved; } }

                #endregion //InterTileSpacingYMinimizedResolved

                #region IsInMaximizedMode

        internal static readonly DependencyPropertyKey IsInMaximizedModePropertyKey =
            DependencyProperty.RegisterReadOnly("IsInMaximizedMode",
            typeof(bool), typeof(TilesPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsInMaximizedMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsInMaximizedModeProperty =
            IsInMaximizedModePropertyKey.DependencyProperty;

        /// <summary>
        /// Returns true if there is at least one <see cref="Tile"/> whose <see cref="Tile.State"/> is 'Maximized'. (read-only)
        /// </summary>
        /// <seealso cref="IsInMaximizedModeProperty"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public bool IsInMaximizedMode
        {
            get
            {
                return (bool)this.GetValue(TilesPanel.IsInMaximizedModeProperty);
            }
        }

                #endregion //IsInMaximizedMode

                #region MaximizedModeSettings

        /// <summary>
        /// Identifies the <see cref="MaximizedModeSettings"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaximizedModeSettingsProperty = DependencyProperty.Register("MaximizedModeSettings",
            typeof(MaximizedModeSettings), typeof(TilesPanel), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMaximizedModeSettingsChanged)));

        private static void OnMaximizedModeSettingsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e )
        {
            TilesPanel panel = target as TilesPanel;
            MaximizedModeSettings oldMaximizedModeSettings = panel._maximizedModeSettings;
            MaximizedModeSettings newMaximizedModeSettings = e.NewValue as MaximizedModeSettings;

            if (oldMaximizedModeSettings != null)
            {
                // unwire the old settings property changed handler
                oldMaximizedModeSettings.PropertyChanged -= new PropertyChangedEventHandler(panel.OnMaximizedModeSettingsPropertyChanged);
            }

            panel._maximizedModeSettings = newMaximizedModeSettings;

            if (panel._maximizedModeSettings != null)
            {
                // wire the new settings property changed handler
                panel._maximizedModeSettings.PropertyChanged += new PropertyChangedEventHandler(panel.OnMaximizedModeSettingsPropertyChanged);
            }

            panel.BumpLayoutVersion();

            if (panel._tilesControl != null)
                panel._tilesControl.InvalidateArrange();

        }

        private void OnMaximizedModeSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.IsInMaximizedMode == false)
                return;

            bool affectsSplitterLocation = false;
            switch (e.PropertyName)
            {
                case "AllowTileSizing":
                case "AllowTileDragging":
                case "RepositionAnimation":
                case "ResizeAnimation":
                    break;

                case "InterTileSpacingXMaximized":
                case "InterTileSpacingXMinimized":
                case "InterTileSpacingYMaximized":
                case "InterTileSpacingYMinimized":
                    this.CalculateResolvedSpacing();
                    break;

                case "InterTileAreaSpacing":
                case "MaximizedTileLocation":
                case "ShowTileAreaSplitter":
                    this.CalculateResolvedSpacing();
                    this.InvalidateMeasure();
                    affectsSplitterLocation = true;
                    break;

                case "MinimizedTileExpandButtonVisibility":
                case "MinimizedTileExpansionMode":
                    this.BumpLayoutVersion();
                    this.InvalidateMeasure();
                    break;

                case "MaximizedTileLayoutOrder":
                case "ShowAllMinimizedTiles":
                default:
                    this.InvalidateMeasure();
                    break;
            }

            if (affectsSplitterLocation && this._tilesControl != null)
                this._tilesControl.InvalidateArrange();
        }

        /// <summary>
        /// Gets/sets the settings that are used to layout Tiles when in maximized mode
        /// </summary>
        /// <seealso cref="IsInMaximizedMode"/>
        /// <seealso cref="MaximizedModeSettingsProperty"/>
        //[Description("Gets/sets the settings that are used to layout Tiles when in maximized mode")]
        //[Category("TilesControl Properties")]
        public MaximizedModeSettings MaximizedModeSettings
        {
            get
            {
                return this._maximizedModeSettings;
            }
            set
            {
                this.SetValue(TilesPanel.MaximizedModeSettingsProperty, value);
            }
        }

        internal MaximizedModeSettings MaximizedModeSettingsSafe
        {
            get
            {
                if (this._maximizedModeSettings != null)
                    return this._maximizedModeSettings;

                if (s_DefaultMaximizedModeSettings == null)
                    s_DefaultMaximizedModeSettings = new MaximizedModeSettings();

                return s_DefaultMaximizedModeSettings;
            }
        }

                #endregion //MaximizedModeSettings

		        #region MaximizedItems
		/// <summary>
		/// Returns a read-only collection of the items that are maximized.
        /// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        [Bindable(true)]
        public ReadOnlyObservableCollection<object> MaximizedItems
		{
			get { return this._maximizedItemsReadOnly; }
		}
		        #endregion //MaximizedItems

                #region MaximizedTileLimit

        /// <summary>
        /// Identifies the <see cref="MaximizedTileLimit"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaximizedTileLimitProperty = DependencyProperty.Register("MaximizedTileLimit",
            typeof(int), typeof(TilesPanel), new FrameworkPropertyMetadata(1, new PropertyChangedCallback(OnMaximizedTileLimitChanged)), new ValidateValueCallback(ValidateMaximizedTileLimit));

        private static void OnMaximizedTileLimitChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            if (!panel._isInitializingTilesControl)
                panel.ProcessNewMaximizedTileLimit((int)e.NewValue);

        }

        private void ProcessNewMaximizedTileLimit(int limit)
        {
			// JJD 4/19/11 - TFS58732
			// Moved logic to VerifyMaximizedTileLimit
			this.VerifyMaximizedTileLimit(limit);

            
            
            
            this.VerifyIsInMaximizedModeProperty();
            this.BumpLayoutVersion();
        }

        /// <summary>
        /// Gets/sets the limit on the number of 'Maximized' tiles that will be allowed.
        /// </summary>
        /// <seealso cref="MaximizedTileLimitProperty"/>
        //[Description("Gets/sets the limit on the number of 'Maximized' tiles that will be allowed.")]
        //[Category("TilesControl Properties")]
        public int MaximizedTileLimit
        {
            get
            {
                return (int)this.GetValue(MaximizedTileLimitProperty);
            }
            set
            {
                this.SetValue(MaximizedTileLimitProperty, value);
            }
        }

                #endregion //MaximizedTileLimit

                #region NormalModeSettings

        /// <summary>
        /// Identifies the <see cref="NormalModeSettings"/> dependency property
        /// </summary>
        public static readonly DependencyProperty NormalModeSettingsProperty = DependencyProperty.Register("NormalModeSettings",
            typeof(NormalModeSettings), typeof(TilesPanel), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNormalModeSettingsChanged)));

        private static void OnNormalModeSettingsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e )
        {
            TilesPanel panel = target as TilesPanel;
            NormalModeSettings oldNormalModeSettings = panel._normalModeSettings;;
            NormalModeSettings newNormalModeSettings = e.NewValue as NormalModeSettings;

            if (oldNormalModeSettings != null)
            {
                // unwire the old settings property changed handler
                oldNormalModeSettings.PropertyChanged -= new PropertyChangedEventHandler(panel.OnNormalModeSettingsPropertyChanged);
            }

            panel._normalModeSettings = newNormalModeSettings;

            if (panel._normalModeSettings != null)
            {
                // wire the new settings property changed handler
                panel._normalModeSettings.PropertyChanged += new PropertyChangedEventHandler(panel.OnNormalModeSettingsPropertyChanged);
            }

        }

        private void OnNormalModeSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.IsInMaximizedMode == true)
                return;

            switch (e.PropertyName)
            {
                case "AllowTileSizing":
                case "AllowTileDragging":
                case "RepositionAnimation":
                case "ResizeAnimation":
                    break;

                case "EmptyTileAreaBackground":
                    this.InvalidateVisual();
                    break;

                case "HorizontalTileAreaAlignment":
                case "MaxColumns":
                case "MaxRows":
                case "MinColumns":
                case "MinRows":
                case "MinHeight":
                case "MaxHeight":
                case "TileLayoutOrder":
                case "ShowAllTiles":
                case "VerticalTileAreaAlignment":
                default:
                    this.InvalidateMeasure();
                    break;
            }
        }

        /// <summary>
        /// Gets/sets the settings that are used to layout Tiles when not in maximized mode
        /// </summary>
        /// <seealso cref="IsInMaximizedMode"/>
        /// <seealso cref="NormalModeSettingsProperty"/>
        //[Description("Gets/sets the settings that are used to layout Tiles when not in maximized mode")]
        //[Category("TilesControl Properties")]
        public NormalModeSettings NormalModeSettings
        {
            get
            {
                return this._normalModeSettings;
            }
            set
            {
                this.SetValue(TilesPanel.NormalModeSettingsProperty, value);
            }
        }

        internal NormalModeSettings NormalModeSettingsSafe
        {
            get
            {
                if (this._normalModeSettings != null)
                    return this._normalModeSettings;

                if (s_DefaultNormalModeSettings == null)
                    s_DefaultNormalModeSettings = new NormalModeSettings();

                return s_DefaultNormalModeSettings;
            }
        }

                #endregion //NormalModeSettings

                #region TileAreaPadding

        /// <summary>
        /// Identifies the <see cref="TileAreaPadding"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TileAreaPaddingProperty = DependencyProperty.Register("TileAreaPadding",
            typeof(Thickness), typeof(TilesPanel), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Get/sets that amount of space between the TilesPanel and the area where the tiles are arranged.
        /// </summary>
        /// <seealso cref="TileAreaPaddingProperty"/>
        //[Description("Get/sets that amount of space between the TilesPanel and the area where the tiles are arranged.")]
        //[Category("TilesControl Properties")]
        public Thickness TileAreaPadding
        {
            get
            {
                return (Thickness)this.GetValue(TilesPanel.TileAreaPaddingProperty);
            }
            set
            {
                this.SetValue(TilesPanel.TileAreaPaddingProperty, value);
            }
        }

                #endregion //TileAreaPadding

                #region TileCloseAction

        /// <summary>
        /// Identifies the <see cref="TileCloseAction"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TileCloseActionProperty = DependencyProperty.Register("TileCloseAction",
            typeof(TileCloseAction), typeof(TilesPanel), new FrameworkPropertyMetadata(TileCloseAction.Default, new PropertyChangedCallback(OnTileCloseActionChanged)), new ValidateValueCallback(ValidateTileCloseAction));

        private static bool ValidateTileCloseAction(object value)
        {
            return Enum.IsDefined(typeof(TileCloseAction), value);
        }

        private static void OnTileCloseActionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanel panel = target as TilesPanel;

            panel.BumpLayoutVersion();
        }

        /// <summary>
        /// Gets/sets what happens when Tiles are closed.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note</b>: if TileCloseAction is set to 'DoNothing' (its default value) then, by default, <see cref="Tile"/>s can't be closed. 
        /// However, this behavior can be overidden for individual tiles by setting their <see cref="Tile.CloseAction"/> property.</para>
        /// </remarks>
        /// <seealso cref="TileCloseActionProperty"/>
        /// <seealso cref="Tile.CloseAction"/>
        /// <seealso cref="Tile.CloseButtonVisibility"/>
        /// <seealso cref="Tile.CloseButtonVisibilityResolved"/>
        //[Description("Gets/sets what happens when Tiles are closed.")]
        //[Category("TilesControl Properties")]
        public TileCloseAction TileCloseAction
        {
            get
            {
                return (TileCloseAction)this.GetValue(TileCloseActionProperty);
            }
            set
            {
                this.SetValue(TileCloseActionProperty, value);
            }
        }

                #endregion //TileCloseAction

            #endregion //Public Properties

            #region Internal Properties

                #region CanDragTiles

        internal bool CanDragTiles
        {
            get
            {
                if (this.IsInMaximizedMode)
                    return this.MaximizedModeSettingsSafe.AllowTileDragging != AllowTileDragging.No;
                else
                    return this.NormalModeSettingsSafe.AllowTileDragging != AllowTileDragging.No;
            }
        }

                #endregion //CanDragTiles	
        
                #region CurrentMinimizedExpandedTile

        internal Tile CurrentMinimizedExpandedTile { get { return this._currentMinimizedExpandedTile; } }

                #endregion //CurrentMinimizedExpandedTile	
    
                #region IsExpandedWhenMinimizedDefault

        internal bool IsExpandedWhenMinimizedDefault
        {
            get
            {
                return this.MaximizedModeSettingsSafe.MinimizedTileExpansionMode == MinimizedTileExpansionMode.AllowMultipleExpandAllInitially;
            }
        }

                #endregion //IsExpandedWhenMinimizedDefault	
    
                #region Items

        internal IList Items
        {
            get
            {
                if (this._itemsControl != null)
                    return this._itemsControl.Items;

                return this.Children;
            }
        }

                #endregion //Items	
    
                #region LayoutVersion

        private static readonly DependencyPropertyKey LayoutVersionPropertyKey =
            DependencyProperty.RegisterReadOnly("LayoutVersion",
            typeof(int), typeof(TilesPanel), new FrameworkPropertyMetadata(0));

        internal static readonly DependencyProperty LayoutVersionProperty =
            LayoutVersionPropertyKey.DependencyProperty;

        internal void BumpLayoutVersion()
        {
            this.SetValue(LayoutVersionPropertyKey, this.LayoutVersion + 1);
        }

        internal int LayoutVersion
        {
            get
            {
                return (int)this.GetValue(TilesPanel.LayoutVersionProperty);
            }
        }

                #endregion //LayoutVersion

                #region Manager

        internal TileMgr Manager
        {
            get
            {
                if (this._tilesControl != null)
                    return this._tilesControl.Manager;

                if (this._standAloneManager == null)
                {
                    this._standAloneManager = new TileMgr();
                    this._standAloneManager.InitPanel(this);
                }

                return this._standAloneManager;
            }
        }

                #endregion //Manager	

                #region MaximizedItemsInternal

        internal ObservableCollectionExtended<object> MaximizedItemsInternal { get { return this._maximizedItems; } }

                #endregion //MaximizedItemsInternal	
    
                #region MinimizedAreaCurrentExtent

        internal double MinimizedAreaCurrentExtent
        {
            get
            {
                double extent = this.MinimizedAreaExplicitExtent;

                if (extent > 0)
                    return extent;

                return base.GetActualMinimizedAreaExtent();
            }
        }

                #endregion //MinimizedAreaCurrentExtent	

                #region MinimizedAreaExplicitExtent

        internal double MinimizedAreaExplicitExtent
        {
            get
            {
                if (this._tilesControl == null)
                    return 0;

                switch (this.MaximizedModeSettingsSafe.MaximizedTileLocation)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Right:
                        return this._tilesControl.MinimizedAreaExplicitExtentX;
                    default:
                        return this._tilesControl.MinimizedAreaExplicitExtentY;
                }
            }
            set
            {
                if (this._tilesControl != null)
                {
                    switch (this.MaximizedModeSettingsSafe.MaximizedTileLocation)
                    {
                        case MaximizedTileLocation.Left:
                        case MaximizedTileLocation.Right:
                            this._tilesControl.MinimizedAreaExplicitExtentX = value;
                            break;
                        default:
                            this._tilesControl.MinimizedAreaExplicitExtentY = value;
                            break;
                    }
                }
            }
        }

                #endregion //MinimizedAreaExplicitExtent	

                #region MinimizedAreaMinExtent

        internal double MinimizedAreaMinExtent
        {
            get
            {
                ITileConstraints constraints = this.GetMinimizedTileAreaConstraints();

                if (constraints == null)
                    return 0; 

                switch (this.MaximizedModeSettingsSafe.MaximizedTileLocation)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Right:
                        return constraints.MinWidth;
                    default:
                        return constraints.MinHeight;
                }
            }
        }

                #endregion //MinimizedAreaMinExtent	

                #region MinimizedAreaMaxExtent

        internal double MinimizedAreaMaxExtent
        {
            get
            {
                ITileConstraints constraints = this.GetMinimizedTileAreaConstraints();

                if (constraints == null)
                    return double.PositiveInfinity; 

                switch (this.MaximizedModeSettingsSafe.MaximizedTileLocation)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Right:
                        return constraints.MaxWidth;
                    default:
                        return constraints.MaxHeight;
                }
            }
        }

                #endregion //MinimizedAreaMaxExtent	

                #region ShouldAnimate

        internal bool ShouldAnimate
        {
            get
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                    return false;

                if (this.IsInMaximizedMode)
                    return this.MaximizedModeSettingsSafe.ShouldAnimate;
                else
                    return this.NormalModeSettingsSafe.ShouldAnimate;
            }
        }

                #endregion //ShouldAnimate	

                #region TileArea

        internal Rect TileArea
        {
            get { return base.GetTileArea(); }
        }

                #endregion //TileArea	
    
                #region TilesControl

        internal XamTilesControl TilesControl
        {
            get
            {
                return this._tilesControl;
            }
        }

                #endregion //TilesControl	

				// JJD 10/8/10 - TFS37313 - added
				#region WasLoaded

		internal bool WasLoaded
		{
			get
			{
				if (!_wasLoaded.HasValue)
				{
					_wasLoaded = this.IsLoaded;

					// JJD 10/8/10 - TFS37313 
					// since this is the first time this property's get was called
					// we want to wire the Loasded event if we aren't already loaded
					if (_wasLoaded == false)
						this.Loaded += new RoutedEventHandler(OnLoaded);
				}

				return _wasLoaded.Value;
			}
		}

				#endregion //WasLoaded	

            #endregion //Internal Properties	

            #region Private Properties

            #endregion //Private Properties	
            
            #endregion //Properties

        #region Methods

            #region Internal Methods

                #region ChangeTileState

		// JJD 1/13/11 - TFS62154 added overload with explicit old state
 		internal bool ChangeTileState(Tile tile, TileState newState)
		{
			return this.ChangeTileState(tile, newState, tile.State);
		}
        internal bool ChangeTileState(Tile tile, TileState newState, TileState oldState)
        {
            Debug.Assert(this._isSettingTileState == false);

            // JJD 1/5/10 - TFS25900
            // set a flag so we know that we are in the process of setting states
            this._isSettingTileState = true;

            //TileState oldState = tile.State;

            try
            {
                return this.ChangeTileStateHelper(tile, newState, oldState);
            }
            finally
            {
                // JJD 1/5/10 - TFS25900
                // clear the flag
                this._isSettingTileState = false;
            }
        }

        internal bool ChangeTileStateHelper(Tile tile, TileState newState, TileState oldState)
        {
			object item = this.ItemFromTile(tile);

            TileStateChangingEventArgs args = new TileStateChangingEventArgs(tile, item, newState);

            // raise the before event
            this.RaiseTileStateChanging(args);

            if (args.Cancel == true)
                return false;

            newState = args.NewState;

            if (newState == oldState)
                return false;

            bool bumpLayoutVersion = false;

            // JJD 3/25/10 - TFS29388
            // If we are loading a layout then bypass maintaining _maximizedItems
            if (this._tilesControl == null ||
                !this._tilesControl.IsLoadingLayout)
            {
                if (oldState == TileState.Maximized)
                {
                    int index = this.GetIndexInMaximizedCollection(tile);

                    
                    
                    
                    
                    

                    if (index >= 0)
                    {
                        this._maximizedItems.RemoveAt(index);
                        
                        
                        
                    }

                    
                    // Always bump the layout version even if we didn't remove it from the collection
                    bumpLayoutVersion = true;
                }
                else if (newState == TileState.Maximized)
                {
                    int index = this.GetIndexInMaximizedCollection(tile);

                    
                    
                    

                    if (index < 0)
                    {
                        
                        int limit = this.MaximizedTileLimit;

                        this._maximizedItems.BeginUpdate();

                        while (this._maximizedItems.Count > 0 &&
                                this._maximizedItems.Count > limit - 1)
                        {
							
							
							// Remove the last item until we get under the limit
							this.RemoveMaximizedItemAtIndex(this._maximizedItems.Count - 1);
						}

                        this._maximizedItems.Add(item);

                        this._maximizedItems.EndUpdate();

                        
                        
                        
                    }

                    
                    // Always bump the layout version even if we didn't add it to the collection
                    bumpLayoutVersion = true;
                }
            }

			// MD 5/14/10 - TFS32080
			// Moved below. See comments on moved line.
			//tile.State = newState;

            
            
            
            this.VerifyIsInMaximizedModeProperty();

			// MD 5/14/10 - TFS32080
			// We need to set the State after verifying the IsInMaximizedModeProperty. This is because the State value is coerced based on the 
			// IsInMaximizedModeProperty and so it needs to be updated correctly first.
			tile.State = newState;

            if (bumpLayoutVersion)
                this.BumpLayoutVersion();

            
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


            if (tile.State == TileState.MinimizedExpanded)
            {
                if (tile != this._currentMinimizedExpandedTile)
                {
                    // if we only alow one expanded guy then minimize the existing expanded tile
                    if (this.MaximizedModeSettingsSafe.MinimizedTileExpansionMode == MinimizedTileExpansionMode.AllowOne)
                    {
                        if (this._currentMinimizedExpandedTile != null)
                        {
                            if (this._currentMinimizedExpandedTile.State == TileState.MinimizedExpanded)
                                this._currentMinimizedExpandedTile.State = TileState.Minimized;
                            else
                            {
                                item = this.ItemFromTile(this._currentMinimizedExpandedTile);

                                ItemInfo info = this.Manager.GetItemInfo(item) as ItemInfo;
                                if (info != null)
                                    info.IsExpandedWhenMinimized = null;
                            }
                        }
                    }

                    this._currentMinimizedExpandedTile = tile;
                }
            }

            //Note: We dont' have to raise the TileStateChanged event here since that will get 
            //      raised inside the Tile.OnStateChanged method
            return true;
        }

                #endregion //ChangeTileState	
    
                #region CloseTile

        internal bool CloseTile(Tile tile)
        {
            TileCloseAction action = tile.CloseActionResolved;

            if (action == TileCloseAction.DoNothing)
                return false;

            object item = this.ItemFromTile(tile);

            TileClosingEventArgs args = new TileClosingEventArgs(tile, item);

            // raise the before event
            this.RaiseTileClosing(args);

            if (args.Cancel == true)
                return false;

            TileState oldState = tile.State;
            tile.IsClosed = true;

            if (tile.State == TileState.Maximized)
            {
                int index = this.GetIndexInMaximizedCollection(tile);

                //Debug.Assert(index >= 0, "Index not found in MaximizedItems collection");

                if (index >= 0)
                {
                    this._maximizedItems.RemoveAt(index);

                    
                    
                    this.VerifyIsInMaximizedModeProperty();

                    this.BumpLayoutVersion();
                }
            }

            // try to remove the item if TileCloseAction is set to RemoveItem
            if (action == TileCloseAction.RemoveItem )
            {
                if (item != null)
                {
                    ItemSourceWrapper wrapper = new ItemSourceWrapper(this.Items);

                    if (wrapper.CanRemove)
                        wrapper.Remove(item);
                }
            }

            // raise the after event
            this.RaiseTileClosed(new TileClosedEventArgs(tile, item));

            return true;
        }

                #endregion //CloseTile	
    
			    #region ExecuteCommandImpl

		internal bool ExecuteTileCommandImpl(Tile tile, RoutedCommand command, object parameter, object source, object originalSource)
		{
			// Make sure we have a command to execute.
			if (null == command)
				throw new ArgumentNullException("command");

            TileState oldState = tile.State;

			bool handled = false;
 
            if (command == Tile.CloseCommand)
            {
                this.CloseTile(tile);
                handled = true;
             }
            else
            if (command == Tile.ToggleMaximizedCommand)
            {
                TileState newState = TileState.Normal;
                if (oldState == TileState.Maximized)
                {
                    if (this._maximizedItems.Count == 1 &&
                        this._maximizedItems[0] == this.ItemFromTile(tile))
                        newState = TileState.Normal;
                    else if (this._maximizedItems.Count == 0)
                        newState = TileState.Normal;
                    else if (tile.IsExpandedWhenMinimizedResolved)
                        newState = TileState.MinimizedExpanded;
                    else
                        newState = TileState.Minimized;
                }
                else
                {
                    newState = TileState.Maximized;
                }

                this.ChangeTileState(tile, newState);

                handled = true;
            }
            else
            if (command == Tile.ToggleMinimizedExpansionCommand)
            {
                switch (oldState)
                {
                    case TileState.Minimized:
                        this.ChangeTileState(tile, TileState.MinimizedExpanded);
                        break;
                        
                    case TileState.MinimizedExpanded:
                        this.ChangeTileState(tile, TileState.Minimized);
                        break;

                    default:
                        if (tile.IsExpandedWhenMinimized.HasValue)
                            tile.IsExpandedWhenMinimized = null;
                        else
                            tile.IsExpandedWhenMinimized = !TilesPanel.GetIsExpandedWhenMinimizedHelper(this, tile, null, tile.IsExpandedWhenMinimized);
                        break;
                        
                }
                handled = true;
            }

			return handled;
		}

			    #endregion //ExecuteCommandImpl
   
				#region GetGenerator







		internal new IItemContainerGenerator GetGenerator()
		{
			if (this._tilesControl == null)
				throw new InvalidOperationException( XamTilesControl.GetString( "TilePanel's XamTilesControl was not found." ) );

			return this.ActiveItemContainerGenerator;
		}

				// JJD 5/22/07 - Optimization
				#region Obsolete code

				#endregion //Obsolete code	
    
				#endregion //GetGenerator	

                #region GetIsExpandedWhenMinimizedHelper

        internal static bool GetIsExpandedWhenMinimizedHelper(TilesPanel panel, Tile tile, object item, bool? isExplicitSetting)
        {
            bool defaultValue = false;

            if (panel != null)
            {
                defaultValue = panel.IsExpandedWhenMinimizedDefault;

                if (panel.MaximizedModeSettingsSafe.MinimizedTileExpansionMode == MinimizedTileExpansionMode.AllowOne)
                {
                    Tile currentMinimizedTile = panel.CurrentMinimizedExpandedTile;

                    if (tile == null &&
                        item != null)
                        tile = panel.TileFromItem(item);

                    if (currentMinimizedTile == null ||
                        currentMinimizedTile != tile)
                        return false;
                }
            }


            if (isExplicitSetting.HasValue)
                return isExplicitSetting.Value;

            return defaultValue;
        }

                #endregion //GetIsExpandedWhenMinimizedHelper	

                #region GetSplitterRect

        internal Rect GetSplitterRect()
        {
            if (!this.IsInMaximizedMode)
                return Rect.Empty;

            Rect area1 = this.GetMaximizedTileArea(false);
            Rect area2 = this.GetMinimizedTileArea(false);

            Debug.Assert(area1 != area2, "Max and min areas can be the same.");

            if (area1 == area2)
                return Rect.Empty;

            // Normalize the areas to make the logic blow simpler
            if (area1.Left >= area2.Right ||
                 area1.Top >= area2.Bottom)
            {
                Rect holdSwap = area1;
                area1 = area2;
                area2 = holdSwap;
            }

            Rect splitterRect = new Rect();
            if (area2.Left >= area1.Right)
            {
                splitterRect.X = area1.Right;
                splitterRect.Y = Math.Min(area1.Top, area2.Top);
                splitterRect.Width = Math.Max(area2.Left - area1.Right, 0);
                splitterRect.Height = Math.Max(area1.Bottom, area2.Bottom) - splitterRect.Y;
            }
            else
            {
                splitterRect.X = Math.Min(area1.Left, area2.Left);
                splitterRect.Y = area1.Bottom;
                splitterRect.Width = Math.Max(area1.Right, area2.Right) - splitterRect.X;
                splitterRect.Height = Math.Max(area2.Top - area1.Bottom, 0);
            }

            splitterRect.Intersect(this.TileArea);

            return splitterRect;
        }

                #endregion //GetSplitterRect	
    
                #region InitializeTilesControl

        internal void InitializeTilesControl(XamTilesControl tilesControl)
        {
            if (tilesControl == this._tilesControl)
                return;

            this._isInitializingTilesControl = true;

            try
            {
                if (this._tilesControl != null)
                {
                    BindingOperations.ClearBinding(this, TileCloseActionProperty);
                    BindingOperations.ClearBinding(this, InterTileSpacingXProperty);
                    BindingOperations.ClearBinding(this, InterTileSpacingYProperty);
                    BindingOperations.ClearBinding(this, MaximizedTileLimitProperty);
                    BindingOperations.ClearBinding(this, TileAreaPaddingProperty);

                    if (this._normalModeSettings == this._tilesControl.NormalModeSettings)
                        BindingOperations.ClearBinding(this, NormalModeSettingsProperty);

                    if (this._maximizedModeSettings == this._tilesControl.MaximizedModeSettings)
                        BindingOperations.ClearBinding(this, MaximizedModeSettingsProperty);

                    this._maximizedItems.Clear();

					// JJD 1/13/11 - TFS62154
					// unire the CollectionChanged event of the Items collection
					((INotifyCollectionChanged)(this._tilesControl.Items)).CollectionChanged -= new NotifyCollectionChangedEventHandler(OnItemsCollectionChanged);
				}

                this._tilesControl = tilesControl;
                this._itemsControl = tilesControl;

                if (this._tilesControl != null)
                {
                    if (this._standAloneManager != null)
                    {
                        this._standAloneManager.InitPanel(null);
                        this._standAloneManager = null;
                    }

                    this._tilesControl.Manager.InitPanel(this);

                    this.SetBinding(TileCloseActionProperty, Utilities.CreateBindingObject(XamTilesControl.TileCloseActionProperty, BindingMode.OneWay, this._tilesControl));
                    this.SetBinding(InterTileSpacingXProperty, Utilities.CreateBindingObject(XamTilesControl.InterTileSpacingXProperty, BindingMode.OneWay, this._tilesControl));
                    this.SetBinding(InterTileSpacingYProperty, Utilities.CreateBindingObject(XamTilesControl.InterTileSpacingYProperty, BindingMode.OneWay, this._tilesControl));
                    this.SetBinding(MaximizedTileLimitProperty, Utilities.CreateBindingObject(XamTilesControl.MaximizedTileLimitProperty, BindingMode.OneWay, this._tilesControl));
                    this.SetBinding(MaximizedModeSettingsProperty, Utilities.CreateBindingObject(XamTilesControl.MaximizedModeSettingsProperty, BindingMode.OneWay, this._tilesControl));
                    this.SetBinding(NormalModeSettingsProperty, Utilities.CreateBindingObject(XamTilesControl.NormalModeSettingsProperty, BindingMode.OneWay, this._tilesControl));
                    this.SetBinding(TileAreaPaddingProperty, Utilities.CreateBindingObject(XamTilesControl.TileAreaPaddingProperty, BindingMode.OneWay, this._tilesControl));

                    // sync up the maximized items from the control
                    if (this._maximizedItems.Count > 0 ||
                         this._tilesControl.MaximizedItems.Count > 0)
                    {
                        this._maximizedItems.BeginUpdate();
                        this._maximizedItems.Clear();
                        this._maximizedItems.AddRange(this._tilesControl.MaximizedItems);
                        this._maximizedItems.EndUpdate();
                    }

                    
                    
                    
                    this.VerifyIsInMaximizedModeProperty();

					// JJD 1/13/11 - TFS62154
					// Wire the CollectionChanged event of the Items collection
					((INotifyCollectionChanged)(this._tilesControl.Items)).CollectionChanged += new NotifyCollectionChangedEventHandler(OnItemsCollectionChanged);

                }

                this.CalculateResolvedSpacing();
            }
            finally
            {
                this._isInitializingTilesControl = false;
                this.ProcessNewMaximizedTileLimit(this.MaximizedTileLimit);
            }
        }

                #endregion //InitializeTilesControl
		
				// JJD 1/13/11 - TFS62154 - added
				#region ItemFromTile

		internal object ItemFromTile(Tile tile)
		{
			object item = this.ItemFromContainer(tile);

			// if ItemFromContainer returns UnsetValue check to see if the tile
			// is in the Items collection. If so return the tile as the item
			if (item == DependencyProperty.UnsetValue)
			{
				if (_tilesControl != null && _tilesControl.Items.Contains(tile))
					item = tile;
			}

			return item;
		}

				#endregion //ItemFromTile	
    
                
                #region OnItemIsClosedChanged

        
        
        
        internal void OnItemIsClosedChanged(object item, bool isClosed, bool isMaximized)
        {
            bool alreadyInList = this._maximizedItems.Contains(item);

            if (alreadyInList && isClosed)
            {
                // when the item is closed we need to remove it from the _maximizedItems collection
                this._maximizedItems.Remove(item);

                if (this._maximizedItems.Count == 0)
                {
                    this.VerifyIsInMaximizedModeProperty();
                    this.BumpLayoutVersion();
                }
            }
            else
            if (isMaximized     == true && 
                alreadyInList   == false && 
                isClosed        == false)
            {
                
                // When the item is un-closed and it is maximized it needs to
                // added back into the _maximizedItems collection
                this._maximizedItems.Add(item);

                if (this._maximizedItems.Count == 1)
                {
                    this.VerifyIsInMaximizedModeProperty();
                    this.BumpLayoutVersion();
                }
            }

			// JJD 4/22/11 - TFS58637
			// When an item is being unclosed and it isn'r maximized there is
			// no guarantee that it will be included in the next arrange pass
			// so we need to arrange it out of view
			if (isClosed == false && isMaximized == false)
			{
				Tile tile = this.TileFromItem(item);

				if (tile != null)
				{
					if (tile.Visibility != Visibility.Collapsed &&
						tile.DesiredSize.Width > 0 &&
						tile.DesiredSize.Height > 0)
					{
						tile.Arrange(new Rect(new Point(-100000, -100000), tile.DesiredSize));
					}
				}
			}
        }

                #endregion //OnItemIsClosedChanged	

				// JJD 4/19/11 - TFS58732
				#region OnTileIsClosedChanged

		internal void OnTileIsClosedChanged(Tile tile)
		{
			object item = ItemFromTileInternal(tile);
			if (item == null)
				return;

			if (tile.State == TileState.Maximized)
			{
				if (tile.IsClosed)
				{
					// when we collapse the tile we take a snapshot of the
					// current max items list using a WeakList. This is 
					// in an effort to maintain the items relative positioning
					// when it gets un-closed later
					if (tile.CloseActionResolved == TileCloseAction.CollapseTile)
					{
						if (this._maximizedItemsSnapshot == null)
						{
							this._maximizedItemsSnapshot = new WeakList<object>();
							this._maximizedItemsSnapshot.AddRange(_maximizedItems);
						}
						else
						{
							// if the item isn't already in the snapshot list then append it
							if (!this._maximizedItemsSnapshot.Contains(item))
								this._maximizedItemsSnapshot.Add(item);
						}
					}
				}
				else
				{
					if (!this._maximizedItems.Contains(item))
					{
						int insertAt = -1;

						// try to determine where we should re-sinsert this item using the 
						// snapshooted list
						if (this._maximizedItemsSnapshot != null)
						{
							this._maximizedItemsSnapshot.Compact();

							// get the index of the item in the snapshot we took when the item was collapsed
							int oldIndex = this._maximizedItemsSnapshot.IndexOf(item);

							if (oldIndex >= 0)
							{
								// walk over the snapshoted items backwards from the previous item
								// to see if any prior item is still in the current maximized list
								for (int i = oldIndex - 1; i >= 0; i--)
								{
									insertAt = _maximizedItems.IndexOf(_maximizedItemsSnapshot[i]);

									if (insertAt >= 0)
									{
										// since this previous item is still in the maximized list
										// we want to re-insert the item at the slot immediately after it
										insertAt++;
										break;
									}
								}

								if (insertAt < 0)
								{
									// walk over the snapshoted items forwards from the next item
									// to see if any trailing item is still in the current maximized list
									for (int i = oldIndex + 1; i < _maximizedItemsSnapshot.Count; i++)
									{
										insertAt = _maximizedItems.IndexOf(_maximizedItemsSnapshot[i]);

										if (insertAt >= 0)
											break;
									}
								}
							}
						}
						int limit = this.MaximizedTileLimit;

						// make sure we insert the item at an index that won't cause it to be beyond the
						// max tile limit and therefore be truncated  by the call to VerifyMaximizedTileLimit
						// below
						insertAt = Math.Min(insertAt, limit - 1);

						if (insertAt >= 0 && insertAt < _maximizedItems.Count)
							this._maximizedItems.Insert(insertAt, item);
						else
							this._maximizedItems.Add(item);

						// walk over the snapshotted list to make sure it is still needed
						if (this._maximizedItemsSnapshot != null)
						{
							bool allItemsFound = true;

							foreach (object obj in _maximizedItemsSnapshot)
							{
								if (!_maximizedItems.Contains(obj))
								{
									allItemsFound = false;
									break;
								}
							}

							// if all items were found in the current maximized items list
							// we can null out the snapshot
							if (allItemsFound)
								_maximizedItemsSnapshot = null;
						}

						this.VerifyMaximizedTileLimit(limit);
						this.VerifyIsInMaximizedModeProperty();
						this.BumpLayoutVersion();
					}
				}
			}

			
			// If the tile is being un-closed then invalidate the panel so the tile
			// can get measured and arranged
			if (tile.IsClosed == false)
				this.InvalidateMeasure();
		}

				#endregion //OnTileIsClosedChanged	
    
                #region MoveTileHelper

        internal bool MoveTileHelper(FrameworkElement container, object item, Point ptInScreenCoordinates)
        {
            TileDraggingEventArgs args = new TileDraggingEventArgs(container as Tile, item);

            // raise the TileDragging event
            this.RaiseTileDragging(args);

            // if canceled return false
            if (args.Cancel)
                return false;

            Point pt = this.PointFromScreen(ptInScreenCoordinates);

            return this.MoveTile(container, item, pt);
        }

                #endregion //MoveTileHelper	
    
                #region StartDrag

        internal bool StartDrag(Tile tile, MouseEventArgs e)
        {
            TileDraggingEventArgs args = new TileDraggingEventArgs(tile, this.ItemFromTile(tile));

            // raise the TileDragging event
            this.RaiseTileDragging(args);

            // if canceled return false
            if (args.Cancel)
                return false;

            return base.StartTileDrag(tile, this.ItemFromTile(tile), e);
        }

                #endregion //StartDrag	
            
                #region SyncItemInfo

		// JJD 8/17/10 - TFS35319 
		// added calledFromInitializePanel param
		internal void SyncItemInfo(Tile tile, bool calledFromInitializePanel)
        {
			// JJD 4/19/11 - TFS58732
			// Moved logic to helper method
			object item = ItemFromTileInternal(tile);
            if (item == null)
                return;

            ItemInfo info = (ItemInfo)this.Manager.GetItemInfo(item);

            Debug.Assert(info != null, "ItemInfo not found");

			if (info != null)
			{
				info.SyncFromTileState(tile);

				// JJD 8/17/10 - TFS35319
				// If the tile is being initialized with the panel and
				// it is maximized then make sure we update the maimized items
				// collection
				if (calledFromInitializePanel &&
					tile.State == TileState.Maximized &&
					this.MaximizedTileLimit > this._maximizedItems.Count &&
					!this._maximizedItems.Contains(item))
				{
					this._maximizedItems.Add(item);
					this.VerifyIsInMaximizedModeProperty();
					this.BumpLayoutVersion();
				}
			}

        }

                #endregion //SyncItemInfo	
    
                #region SyncTileFromItemInfo

        internal void SyncTileFromItemInfo(Tile tile)
        {
            object item;

            if (this.IsItemsHost)
                item = this.ItemFromTile(tile);
            else
                item = tile;

            Debug.Assert(item != null, "unknown tile");
            if (item == null)
                return;

            ItemInfo info = (ItemInfo)this.Manager.GetItemInfo(item);

            Debug.Assert(info != null, "ItemInfo not found");

            if (info != null)
                tile.SynchStateFromInfo(info);

        }

                #endregion //SyncTileFromItemInfo	

                #region TileFromItem

        internal Tile TileFromItem(object item)
        {
            Tile tile = item as Tile;

            if (tile != null)
                return tile;

            if (this.IsItemsHost)
                return this.ContainerFromItem(item) as Tile;

            return null;
        }

                #endregion //TileFromItem

                // JJD 2/12/10 - TFS27548 - added
                #region VerifyIsInMaximizedModeProperty

        internal void VerifyIsInMaximizedModeProperty()
        {
            if (this._maximizedItems.Count == 0)
                this.SetValue(IsInMaximizedModePropertyKey, KnownBoxes.FalseBox);
            else
                this.SetValue(IsInMaximizedModePropertyKey, KnownBoxes.TrueBox);
        }

                #endregion //VerifyIsInMaximizedModeProperty	
    
            #endregion //Internal Methods

            #region Private Methods

                #region CalculateResolvedSpacing

        private void CalculateResolvedSpacing()
        {
            MaximizedModeSettings maxModeSettings = this.MaximizedModeSettingsSafe;


            // set _interTileSpacingXMaximizedResolved (default to _interTileSpacingResolved;
            double value = maxModeSettings.InterTileSpacingXMaximized;
            if (double.IsNaN(value))
                value = this.InterTileSpacingX;

            this.SetValue(TilesPanel.InterTileSpacingXMaximizedResolvedPropertyKey, value);



            // set _interTileSpacingXMinimizedResolved (default to _interTileSpacingResolved;
            value = maxModeSettings.InterTileSpacingXMinimized;
            if (double.IsNaN(value))
                value = this.InterTileSpacingX;

            this.SetValue(TilesPanel.InterTileSpacingXMinimizedResolvedPropertyKey, value);


            // set _interTileSpacingYMaximizedResolved (default to _interTileSpacingResolved;
            value = maxModeSettings.InterTileSpacingYMaximized;
            if (double.IsNaN(value))
                value = this.InterTileSpacingY;

            this.SetValue(TilesPanel.InterTileSpacingYMaximizedResolvedPropertyKey, value);



            // set _interTileSpacingYMinimizedResolved (default to _interTileSpacingResolved;
            value = maxModeSettings.InterTileSpacingYMinimized;
            if (double.IsNaN(value))
                value = this.InterTileSpacingY;

            this.SetValue(TilesPanel.InterTileSpacingYMinimizedResolvedPropertyKey, value);


            // set _interTileAreaSpacingResolved (default to _interTileSpacingMaximizedResolved;
            value = maxModeSettings.InterTileAreaSpacing;
            if (double.IsNaN(value))
            {
                switch (maxModeSettings.MaximizedTileLocation)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Right:
                        value = this.InterTileSpacingXMaximizedResolved;
                        break;
                    default:
                        value = this.InterTileSpacingYMaximizedResolved;
                        break;
                }
            }

            this.SetValue(TilesPanel.InterTileAreaSpacingResolvedPropertyKey, value);


            if (this._tilesControl != null)
            {
                this._tilesControl.SetValue(TilesPanel.InterTileAreaSpacingResolvedPropertyKey, this.InterTileAreaSpacingResolved);
                this._tilesControl.SetValue(TilesPanel.InterTileSpacingXMaximizedResolvedPropertyKey, this.InterTileSpacingXMaximizedResolved);
                this._tilesControl.SetValue(TilesPanel.InterTileSpacingXMinimizedResolvedPropertyKey, this.InterTileSpacingXMinimizedResolved);
                this._tilesControl.SetValue(TilesPanel.InterTileSpacingYMaximizedResolvedPropertyKey, this.InterTileSpacingYMaximizedResolved);
                this._tilesControl.SetValue(TilesPanel.InterTileSpacingYMinimizedResolvedPropertyKey, this.InterTileSpacingYMinimizedResolved);
            }
        }

                #endregion //CalculateResolvedSpacing	

                #region GetIndexInMaximizedCollection

        private int GetIndexInMaximizedCollection(Tile tile)
        {
            if (this._maximizedItems.Count == 0)
                return -1;

            object item = this.ItemFromTile(tile);

            return this._maximizedItems.IndexOf(item);
        }

                #endregion //GetIndexInMaximizedCollection	

				// JJD 4/19/11 - TFS58732 - added
				#region ItemFromTileInternal

		private object ItemFromTileInternal(Tile tile)
		{
			object item;

			if (this.IsItemsHost)
			{
				item = this.ItemFromTile(tile);

				// JJD 3/1/10 - TFS27832
				// If the ItemFromContainer comes back with UnsetValue
				// then assume they added Tiles directly to the list so
				// use the tile as the item. If we don't find it below it
				// won't hurt anything
				if (item == DependencyProperty.UnsetValue)
					item = tile;
			}
			else
				item = tile;


			Debug.Assert(item != null, "unknown tile");
			return item;
		}

				#endregion //ItemFromTileInternal	
        
                #region OnConstraintPropertyChanged

        private static void OnConstraintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Visual v = d as Visual;
            TilesPanel panel = null != v ? VisualTreeHelper.GetParent(v) as TilesPanel : null;
            if (null != panel)
                panel.InvalidateMeasure();
        }

                #endregion // OnConstraintPropertyChanged

				// JJD 1/13/11 - TFS62154 - added
				#region OnItemsAdded

		private void OnItemsAdded(IList items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				Tile tile = items[i] as Tile;

				if (tile == null)
					continue;

				if (tile.Panel != this)
					tile.InitializePanel(this);

				if (tile.State != TileState.Maximized)
					continue;

				if (!this.MaximizedItemsInternal.Contains(tile))
					this.ChangeTileState(tile, tile.State, TileState.Normal);
			}
		}

				#endregion //OnItemsAdded	
		    
				// JJD 1/13/11 - TFS62154
				#region OnItemsCollectionChanged

		private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this._tilesControl != null)
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						this.OnItemsAdded(e.NewItems);
						break;
				}
			}
		}

				#endregion //OnItemsCollectionChanged	
        
				// JJD 10/8/10 - TFS37313 - added
				#region OnLoaded

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// since we are loaded now we should set the flag
			_wasLoaded = true;

			// unwire the event
			this.Loaded -= new RoutedEventHandler(OnLoaded);
		}

				#endregion //OnLoaded	
    
                #region RefreshTileContentVisibility

        private void RefreshTileContentVisibility()
        {
            foreach (Visual child in this.ChildElements)
            {
                Tile tile = child as Tile;

                if (tile != null)
                    tile.SetContentVisibility();
            }
        }

                #endregion //RefreshTileContentVisibility	

				#region RemoveMaximizedItemAtIndex

		
		
		private void RemoveMaximizedItemAtIndex(int index)
		{
			object item = this._maximizedItems[index];

			Tile tileToRemove = this.ContainerFromItem(item) as Tile;

			// JJD 10/8/10 - TFS37313
			// See if the item is a tile
			if (tileToRemove == null)
				tileToRemove = item as Tile;

			Debug.Assert(tileToRemove != null, "We should have a tile here");
			this._maximizedItems.RemoveAt(index);

			if (tileToRemove != null)
			{
				tileToRemove.State = tileToRemove.IsExpandedWhenMinimizedResolved
					? TileState.MinimizedExpanded
					: TileState.Minimized;
			}
			else
			{
				// JJD 10/8/10 - TFS37313
				// Since we don't have a tile try syncing up the IsMaximized state
				// of the ItemInfo object
				XamTilesControl tc = this.TilesControl;

				if (tc != null)
				{
					ItemInfo info = tc.GetItemInfo(item);

					if (info != null)
						info.IsMaximized = false;
				}
			}
		}

				#endregion //RemoveMaximizedItemAtIndex	
        
                #region ValidateColumnRow

        private static bool ValidateColumnRow(object objVal)
        {
            int val = (int)objVal;
            return val >= 0 || val == GridBagConstraintConstants.Relative;
        }

                #endregion // ValidateColumnRow

		        #region ValidateColumnRowSpan

		private static bool ValidateColumnRowSpan( object objVal )
		{
			int val = (int)objVal;
			return val >= 1 || val == GridBagConstraintConstants.Remainder;
		}

		        #endregion // ValidateColumnRowSpan

                #region ValidateColumnRowWeight

        private static bool ValidateColumnRowWeight(object objVal)
        {
            float val = (float)objVal;

            if ( float.IsNaN(val) || float.IsInfinity(val) )
                return false;

            return val >= 0f;
        }

                #endregion // ValidateColumnRowWeight

				// JJD 4/19/11 - TFS58732 - added
				#region VerifyMaximizedTileLimit

		private void VerifyMaximizedTileLimit(int limit)
		{
			if (limit < this.MaximizedItems.Count)
			{
				this._maximizedItems.BeginUpdate();

				while (this._maximizedItems.Count > 0 &&
						this._maximizedItems.Count > limit)
				{
					
					
					// Remove the first item until we get under the limit
					this.RemoveMaximizedItemAtIndex(0);
				}

				this._maximizedItems.EndUpdate();
			}
		}

				#endregion //VerifyMaximizedTileLimit	
    
            #endregion //Private Methods

        #endregion //Methods

        #region TileMgr nested class

        internal class TileMgr : TileManager
        {
            #region Private Members

            private Size? _synchronizedItemSize;

            #endregion //Private Members	
    
            #region Base class overrides

                #region CreateItemInfo

            /// <summary>
            /// Factory method to return a new instance of a ItemInfoBase derived class to represent an item.
            /// </summary>
            /// <param name="item">The item to represent</param>
            /// <param name="index">The index of the item in the items collection.</param>
            /// <returns></returns>
            protected override ItemInfoBase CreateItemInfo(object item, int index)
            {
                return new ItemInfo(this, item, index);
            }

                #endregion //CreateItemInfo

            #endregion //Base class overrides	

            #region Properties

            internal Size? SynchronizedItemSize
            {
                get
                {
                    TilesPanel panel = this.Panel as TilesPanel;

                    if (panel != null)
                    {
                        // since we don't support synchronized sizing in maximize mode return null
                        if (panel.IsInMaximizedMode)
                            return null;

                        // If we aren't doing synchronized sizing or if there is an
                        // explicit layout we should return null
                        NormalModeSettings settings = panel.NormalModeSettingsSafe;
                        if (settings.AllowTileSizing != AllowTileSizing.Synchronized ||
                            settings.TileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile)
                            return null;
                    }

                    return this._synchronizedItemSize;
                }
                set
                {
                    if (this._synchronizedItemSize != value)
                    {
                        this._synchronizedItemSize = value;

                        TilesPanel panel = this.Panel as TilesPanel;

                        if (panel != null)
                            panel.InvalidateMeasure();
                    }
                }
            }

            #endregion //Properties	
        
            #region Methods

            internal void InitPanel(TilesPanel panel)
            {
                base.InitializePanel(panel);
            }

            #endregion //Methods
        }

        #endregion //TileMgr nested class

		// JJD 11/1/11 - TFS88171
		// Added object to cache states during a swap operation
		#region SwapInfo nested private class

		private class SwapInfo
		{
			internal bool _swapIsExpandedWhenMinimized;
			internal bool? _sourceIsExpandedWhenMinimized;
			internal bool? _targetIsExpandedWhenMinimized;
		}

		#endregion //SwapInfo nested private class
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