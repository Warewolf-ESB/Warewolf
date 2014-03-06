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

using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Collections;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Layouts
{
    internal class TileLayoutManager : ILayoutContainer
    {
        #region Private Members

        private TileAreaPanel                       _panel;
        private XamTileManager                      _owningManager;
        private GridBagLayoutManager                _gridBagLayoutNormal;
        private GridBagLayoutManager                _gridBagLayoutMaximized;
        private GridBagLayoutManager                _gridBagLayoutMinimized;
        private GridBagLayoutManager                _gridBagLayoutMaximizedModeOverall;
        private GridBagLayoutManager                _gridBagLayoutNormalModeOverall;

        private TileAreaLayoutItem                  _maximizedTileArea;
        private TileAreaLayoutItem                  _minimizedTileArea;

        private List<object>                        _itemsInOriginalOrder;

        private DependencyObject                    _lastTile;
        private ITileConstraints                     _lastElementConstraints;
        private ITileConstraints                     _lastElementDefaultConstraints;
        private TileState?                          _lastElementState;
        private MaximizedTileLocation               _maximizedTileLocation;
        private MaximizedTileLayoutOrder            _maximizedTileLayoutOrder;
        private TileLayoutOrder                     _tileLayoutOrder;
		private ExplicitLayoutTileSizeBehavior		_explicitTileSizeBehavior;	// JJD 5/9/11 - TFS74206 - added 

        private Size?								_synchronizedItemSize;
        private Size?                               _synchronizedSize;
        private HorizontalAlignment                 _lastHorizontalTileAreaAlignment;
        private VerticalAlignment                   _lastVerticalTileAreaAlignment;
        private double                              _explicitMinimizedAreaExtent;
        private bool                                _isInMaximizedMode;
        private bool                                _lastShowAllTiles;
        private bool                                _wasLastScrollableItemGened;
        private int                                 _scrollPosition;
        private int                                 _scrollPositionOfLastArrangedItem;
        private int                                 _scrollLogicalOffset;
        private int                                 _scrollLogicalOffsetOfLastArrangedItem;
        private int                                 _lastScrollPosition;
        private int                                 _lastScrollLogicalOffset;
                                                    
        private int                                 _lastNormalModeItemsPerRowColumn;
        private int                                 _normalModeItemsPerRowColumn;
        private int                                 _normalModeItemsPerRowColumnActual;
        private int                                 _normalModeRowColumnsInView;
        private int                                 _normalModeRowColumnsInViewActual;
        private int                                 _totalMaximizedColumns;
        private int                                 _totalMaximizedRows;
        private int                                 _variableTotalRowColmuns; // this is used for Horizontal/VerticalVariable layouts
        private double                              _variableAvgItemsPerRowColumn; // this is used for Horizontal/VerticalVariable layouts

        private int                                 _totalColumnsDisplayed;
        private int                                 _totalRowsDisplayed;
        private bool                                _scrollTilesVertically;
        private bool                                _scrollTilesHorizontally;
        private Vector                              _scrollSmallChange = new Vector();
        private Vector                              _scrollLargeChange = new Vector();
        private Vector                              _scrollMaxOffset = new Vector();
        private Vector                              _arrangeOffset = new Vector();
        private Vector                              _arrangeOffsetForMaxmizedTiles = new Vector();
        private double                              _minimizedAreaActualExtent = 0;
        private Size                                _defaultMinimunItemSize;
        private Size                                _lastMeasureDesiredSize;

        private bool								_isSynchLastScrollPosOperationPending;




        private WeakDictionary<XamTile, object> 
                                                    _containerToItemMap = new WeakDictionary<XamTile, object>(true, false);
        private WeakDictionary<object, LayoutItem>  
                                                    _layoutItemMap = new WeakDictionary<object, LayoutItem>(true, false);
        private WeakDictionary<object, ItemTileInfo> 
                                                    _itemInfoMap = new WeakDictionary<object, ItemTileInfo>(true, false);
        private HashSet <object>                    _itemsToBeArranged = new HashSet<object>();
        private List<ItemTileInfo>                  _itemsInView = new List<ItemTileInfo>();
        private List<ItemRowColumnSizeInfo>         _lastItemLocations = null;
        private ScrollManagerSparseArray            _sparseArray = new ScrollManagerSparseArray();

        private TileAreaPanel.GenerationCache      _genCache;

        private const int NON_TILE_SCROLL_SMALL_CHANGE = 10;

        #endregion //Private Members	
  
		#region Constructor

		internal TileLayoutManager(XamTileManager owningManager)
		{
			this._owningManager = owningManager;

			// wire the new CollectionChanged event
			((INotifyCollectionChanged)(this._owningManager.Items)).CollectionChanged += new NotifyCollectionChangedEventHandler(OnItemsCollectionChanged);
		}

		#endregion //Constructor	
    
        #region Properties

            #region Internal Properties
    
                #region ArrangeOffset

        internal Vector ArrangeOffset { get { return this._arrangeOffset; } }

                #endregion //ArrangeOffset
    
                #region ArrangeOffsetForMaxmizedTiles

        internal Vector ArrangeOffsetForMaxmizedTiles { get { return this._arrangeOffsetForMaxmizedTiles; } }

                #endregion //ArrangeOffsetForMaxmizedTiles

                #region CountOfItemsToBeArranged

        internal int CountOfItemsToBeArranged { get { return this._itemsToBeArranged.Count; } }

                #endregion //CountOfItemsToBeArranged	

                #region ScrollIndexOfFirstArrangedItem

        internal int ScrollIndexOfFirstArrangedItem { get { return this._scrollPosition; } }

                #endregion //ScrollIndexOfFirstArrangedItem	

                #region Items

        internal IList Items
        {
            get
            {
                return this._owningManager.Items;
            }
        }

                #endregion //Items	
        
                #region MinimizedAreaActualExtent

        internal double MinimizedAreaActualExtent
        {
            get { return this._minimizedAreaActualExtent; }

        }

                #endregion //MinimizedAreaPreferredExtent	
 
				#region OwningManager

		internal XamTileManager OwningManager { get { return _owningManager; } }

   				#endregion //OwningManager	
    
                #region Panel






        internal TileAreaPanel Panel { get { return this._panel; } }

                #endregion //Panel	
    
                #region ScrollIndexOfLastArrangedItem

        internal int ScrollIndexOfLastArrangedItem { get { return this._scrollPositionOfLastArrangedItem; } }

                #endregion //ScrollIndexOfLastArrangedItem	
    
                #region ScrollLargeChange

        internal Vector ScrollLargeChange { get { return this._scrollLargeChange; } }

                #endregion //ScrollLargeChange	
    
                #region ScrollMaxOffset

        internal Vector ScrollMaxOffset { get { return this._scrollMaxOffset; } }

                #endregion //ScrollMaxOffset

				#region ScrollPosition

		internal int ScrollPosition
		{
			get { return this._scrollPosition; }
			set
			{
				if (value != this._scrollPosition)
				{
					double offset = this.GetOffsetFromScrollPosition(value);

					TileAreaPanel.ScrollData scrollData = this.Panel.ScrollDataInfo;
					
					if (this._scrollTilesHorizontally)
						scrollData._offset = new Vector(offset, scrollData._offset.Y);
					else
						scrollData._offset = new Vector(scrollData._offset.X, offset);

					scrollData.VerifyScrollData(scrollData._viewport, scrollData._extent);

					this.Panel.InvalidateMeasure();
				}
			}
		}

				#endregion //ScrollPosition

				#region ScrollSmallChange

		internal Vector ScrollSmallChange { get { return this._scrollSmallChange; } }

                #endregion //ScrollSmallChange	

                #region ScrollTilesHorizontally

        internal bool ScrollTilesHorizontally { get { return this._scrollTilesHorizontally; } }

                #endregion //ScrollTilesHorizontally	
    
                #region ScrollTilesVertically

        internal bool ScrollTilesVertically { get { return this._scrollTilesVertically; } }

                #endregion //ScrollTilesVertically	
    
                #region SparseArray

        internal ScrollManagerSparseArray SparseArray { get { return this._sparseArray; } }

                #endregion //SparseArray	

				#region SynchronizedItemSize

		internal Size? SynchronizedItemSize
		{
			get
			{
				// since we don't support synchronized sizing in maximize mode return null
				if (_owningManager.IsInMaximizedMode)
					return null;

				if (_panel != null)
				{
					// If we aren't doing synchronized sizing or if there is an
					// explicit layout we should return null
					NormalModeSettings settings = _panel.NormalModeSettingsSafe;
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

					if (_panel != null)
						_panel.InvalidateMeasure();
				}
			}
		}

				#endregion //SynchronizedItemSize	
            
                #region TotalColumnsDisplayed

        internal int TotalColumnsDisplayed { get { return this._totalColumnsDisplayed; } }

                #endregion //TotalColumnsDisplayed	
        
                #region TotalRowsDisplayed

        internal int TotalRowsDisplayed { get { return this._totalRowsDisplayed; } }

                #endregion //TotalRowsDisplayed	
        
            #endregion //Internal Properties	

            #region Private Properties
    
                #region GridBagLayoutNormal

        private GridBagLayoutManager GridBagLayoutNormal
        {
            get
            {
                if (this._gridBagLayoutNormal == null)
                    this._gridBagLayoutNormal = new GridBagLayoutManager();

                return this._gridBagLayoutNormal;
            }
        }

                #endregion //GridBagLayoutNormal	
    
                #region GridBagLayoutNormalModeOverall

        private GridBagLayoutManager GridBagLayoutNormalModeOverall
        {
            get
            {
                if (this._gridBagLayoutNormalModeOverall == null)
                    this._gridBagLayoutNormalModeOverall = new GridBagLayoutManager();

                return this._gridBagLayoutNormalModeOverall;
            }
        }

                #endregion //GridBagLayoutNormalModeOverall	

                #region GridBagLayoutMaximized

        private GridBagLayoutManager GridBagLayoutMaximized
        {
            get
            {
                if (this._gridBagLayoutMaximized == null)
                {
                    this._gridBagLayoutMaximized = new GridBagLayoutManager();

                    this._gridBagLayoutMaximized.ExpandToFitHeight = true;
                    this._gridBagLayoutMaximized.ExpandToFitWidth = true;
                }

                return this._gridBagLayoutMaximized;
            }
        }

                #endregion //GridBagLayoutMaximized	

                #region GridBagLayoutMinimized

        private GridBagLayoutManager GridBagLayoutMinimized
        {
            get
            {
                if (this._gridBagLayoutMinimized == null)
                {
                    this._gridBagLayoutMinimized = new GridBagLayoutManager();
                }

                return this._gridBagLayoutMinimized;
            }
        }

                #endregion //GridBagLayoutMinimized	

                #region GridBagLayoutMaximizedModeOverall

        private GridBagLayoutManager GridBagLayoutMaximizedModeOverall
        {
            get
            {
                if (this._gridBagLayoutMaximizedModeOverall == null)
                {
                    this._gridBagLayoutMaximizedModeOverall = new GridBagLayoutManager();

                    this._gridBagLayoutMaximizedModeOverall.ExpandToFitHeight = true;
                    this._gridBagLayoutMaximizedModeOverall.ExpandToFitWidth = true;
                    
                    // create and add an area for maximized tiles
                    this._gridBagLayoutMaximizedModeOverall.LayoutItems.Add(this.MaximizedTileArea, this.MaximizedTileArea);
                    
                    // create and add an area for minimized tiles
                    this._gridBagLayoutMaximizedModeOverall.LayoutItems.Add(this.MinimizedTileArea, this.MinimizedTileArea);
                }

                Debug.Assert(_gridBagLayoutMaximizedModeOverall.LayoutItems.Count == 2);

                return this._gridBagLayoutMaximizedModeOverall;
            }
        }

                #endregion //GridBagLayoutMaximizedModeOverall	

                #region IsMaximizedAreaLeftOrRight

            
            
            internal bool IsMaximizedAreaLeftOrRight
            {
                get
                {
                    switch (this._maximizedTileLocation)
                    {
                        case MaximizedTileLocation.Left:
                        case MaximizedTileLocation.Right:
                            return true;
                    }
                    return false;
                }
            }

                #endregion //IsMaximizedAreaLeftOrRight	
    
                #region MaximizedTileArea

        private TileAreaLayoutItem MaximizedTileArea
        {
            get
            {
                if (this._maximizedTileArea == null)
                    this._maximizedTileArea = new TileAreaLayoutItem(this, TileArea.MaximizedTiles);

                return this._maximizedTileArea;
            }
        }

                #endregion //MaximizedTileArea	

                #region MinimizedTileArea

        private TileAreaLayoutItem MinimizedTileArea
        {
            get
            {
                if (this._minimizedTileArea == null)
                    this._minimizedTileArea = new TileAreaLayoutItem(this, TileArea.MinimizedTiles);

                return this._minimizedTileArea;
            }
        }

                #endregion //MinimizedTileArea	

                #region NormalModeItemsPerRowColumn

        private int NormalModeItemsPerRowColumn
        {
            get
            {
                return this._normalModeItemsPerRowColumn;
            }
        }

                #endregion //NormalModeItemsPerRowColumn	
            
                #region LastElementDefaultConstraints

        private ITileConstraints LastElementDefaultConstraints { get { return this._lastElementDefaultConstraints; } }

                #endregion //CurrentModeDefaultConstraints

            #endregion //Private Properties	
        
        #endregion //Properties	

        #region Methods

            #region Public Methods

                #region GetItemInfo

        /// <summary>
        /// Retuns the <see cref="ItemTileInfo"/> associated with a specific item
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>An <see cref="ItemTileInfo"/> object</returns>
        public ItemTileInfo GetItemInfo(object item)
        {
            return this.GetItemInfo(item, true, -1);
        }

        /// <summary>
        /// Retuns the <see cref="ItemTileInfo"/> associated with a specific item
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="createIfNotFound">True to create an ItemTileInfo if one doesn't exist.</param>
        /// <param name="originalItemIndex">The index of the item in the items collection or -1.</param>
        /// <returns>An <see cref="ItemTileInfo"/> object</returns>
        public ItemTileInfo GetItemInfo(object item, bool createIfNotFound, int originalItemIndex)
        {
            ItemTileInfo itemInfo;

            if (!this._itemInfoMap.TryGetValue(item, out itemInfo))
            {
                if (createIfNotFound == false )
                    return null;

                this.VerifyItemSlot(item, ref originalItemIndex);

                itemInfo = this.CreateItemInfo(item, originalItemIndex);

                this._itemInfoMap.Add(item, itemInfo);
            }

            return itemInfo;
        }

                #endregion //GetItemInfo

                #region SortItems

        /// <summary>
        /// Sorts the items (adjusts their logical index)
        /// </summary>
        /// <param name="comparer">The comparer to use</param>
        public void SortItems(IComparer<ItemTileInfo> comparer)
        {
            CoreUtilities.ValidateNotNull(comparer, "comparer");

            this.EnsureItemArrayIsAllocated();

            int count = this.Items.Count;

            if (count < 2)
                return;

            // loop aover all of the items making sure that all slots in the
            // sparse array have been filled
            for (int i = 0; i < count; i++)
            {
                object item = this.GetItem(i, false);
            }

            this.SparseArray.SortGeneric(comparer);

        }

                #endregion //SortItems	
    
            #endregion //Public Methods	
 
            #region Protected Methods

                #region CreateItemInfo

        /// <summary>
        /// Factory method to return a new instance of a ItemTileInfo derived class to represent an item.
        /// </summary>
        /// <param name="item">The item to represent</param>
        /// <param name="index">The index of the item in the items collection.</param>
        /// <returns></returns>
        protected virtual ItemTileInfo CreateItemInfo(object item, int index)
        {
            return new ItemTileInfo(this, item, index);
        }

                #endregion //CreateItemInfo

            #endregion //Protected Methods	
    
            #region Internal Methods

		        #region AreClose

		internal static bool AreClose(double value1, double value2)
		{
			if (value1 == value2)
				return true;

			return Math.Abs(value1 - value2) < .0000000001;
		}

		        #endregion //AreClose

                #region BeginGeneration

        internal ScrollDirection BeginGeneration(TileAreaPanel.GenerationCache genCache)
        {






            
            //Hold a ref to the gen cache for use during the generation
            this._genCache = genCache;

            this._itemsInView.Clear();

            this._wasLastScrollableItemGened        = false;

            // cache various settings from the panel so we don't need to get them multiple times
			this._defaultMinimunItemSize = this._panel.GetDefaultMinimumItemSize();

            bool wasInMaxmizedMode = this._isInMaximizedMode;

			this._isInMaximizedMode = this._owningManager.IsInMaximizedMode;

            // JJD 3/5/10 - TFS29058 
            // Hold the old offsets in stack variables
            Vector oldOffset = this._arrangeOffset;
            Vector oldOffsetForMaxmizedTiles  = this._arrangeOffsetForMaxmizedTiles;

            
            // If the mode changes then reset the arrange offsets used for pixel level scrolling
            if (this._isInMaximizedMode != wasInMaxmizedMode)
            {
                this._arrangeOffset = new Vector();
                this._arrangeOffsetForMaxmizedTiles = new Vector();
            }

            // JJD 3/5/10 - TFS29058
            #region Loop over the old items and verify that their maxmimzed state hasn't changed

            if (this._lastItemLocations != null)
            {
                int count = this._lastItemLocations.Count;

                for (int i = 0; i < count; i++)
                {
                    ItemRowColumnSizeInfo rci = this._lastItemLocations[i];

                    bool wasMaximized   = rci.WasMaximized;

                    // call VerifyMaximizeState which may return an adjustment
                    // if the maximized state has changed since the last arrangement
                    Vector adjustment = rci.VerifyMaximizeState(oldOffset, oldOffsetForMaxmizedTiles);

                    bool isMaximized   = rci.WasMaximized;

                    // if there is no adjustment and the maximized state 
                    // hasn't changed then continue
                    if (adjustment.X == 0 && adjustment.Y == 0 && wasMaximized == isMaximized)
                        continue;

                    // Get the asscoiated layout item and adjust its rects
                    LayoutItem li = this.GetLayoutItem(rci.Info.Item);

                    if (li == null)
                        continue;

                    li.OffsetAllRects(adjustment);

                    // JJD 3/5/10 - TFS29071
                    // If the item is going from maximized to non-maximized then
                    // we need to clear the ZIndex property since non-maximized tiles
                    // should be behind any maximized tiles
                    if (wasMaximized && !isMaximized)
                    {
                        if (li.Container != null)
                            li.Container.ClearValue(Canvas.ZIndexProperty);
                    }
                }
            }

            #endregion //Loop over the old items and verify their maximized state hasn't changed
    
            if (this._isInMaximizedMode)
            {
				MaximizedModeSettings maxModeSettings	= this._owningManager.MaximizedModeSettingsSafe;
				this._maximizedTileLocation				= maxModeSettings.MaximizedTileLocationResolved;
				this._maximizedTileLayoutOrder			= maxModeSettings.MaximizedTileLayoutOrder;
                this._explicitMinimizedAreaExtent		= this._panel.MinimizedAreaExplicitExtent;
            }
            else
            {
				NormalModeSettings normalSettings		= _owningManager.NormalModeSettingsSafe;
				this._tileLayoutOrder					= normalSettings.TileLayoutOrder;
				this._explicitTileSizeBehavior			= normalSettings.ExplicitLayoutTileSizeBehavior;

                this._synchronizedSize					= this._synchronizedItemSize;

                if (this._synchronizedSize != null)
                {
                    Size size = this._synchronizedSize.Value;

                    if (double.IsNaN(size.Width) ||
                        double.IsInfinity(size.Width))
                        size.Width = 0;

                    if (double.IsNaN(size.Height) ||
                        double.IsInfinity(size.Height))
                        size.Height = 0;

                    this._synchronizedSize = size;
                }
            }

            this.EnsureItemArrayIsAllocated();
            
            // JJD 3/18/10 - TFS28705 - Optimization
            // Create a list to help optimize recycling
            this.CreateExpectedItemsList(genCache);

            // copy the old items to be arranged so we know at the end
            // of GenerateElements which ones need to be animated out of view 
            
            
            
            
            genCache._previousItemsToBeArrangedTempCache = new Dictionary<DependencyObject, LayoutItem>();

            foreach (LayoutItem li in this._itemsToBeArranged)
            {
                DependencyObject container = li.Container;

                if (container != null)
                    genCache._previousItemsToBeArrangedTempCache.Add(container, li);
            }


            //create a hash to hold the new items that are being added 
            genCache._newItemsToBeArrangedTempCache = new HashSet<object>();

            if (this._gridBagLayoutNormal != null)
                this._gridBagLayoutNormal.LayoutItems.Clear();

            if (this._gridBagLayoutNormalModeOverall != null)
            {
                // cache the old _gridBagLayoutNormalModeOverall on the gen cache
                // so we can use the old items to help identify where we need to scroll to
                genCache._previousGridBagLayoutNormalModeOverall = this._gridBagLayoutNormalModeOverall;

                //clear the member 
                this._gridBagLayoutNormalModeOverall = null;
            }

            if (this._gridBagLayoutMaximized != null)
                this._gridBagLayoutMaximized.LayoutItems.Clear();

            if (this._gridBagLayoutMinimized != null)
                this._gridBagLayoutMinimized.LayoutItems.Clear();

            return this._scrollPosition >= this._lastScrollPosition
                        ? ScrollDirection.Increment
                        : ScrollDirection.Decrement;

        }

                #endregion //BeginGeneration	

                #region BringIndexIntoView

        
        internal void BringIndexIntoView(int index)
        {
            IList items = this.Items;
            if (index < 0 ||
                index >= items.Count)
                throw new ArgumentOutOfRangeException("index");

            object item = items[index];

            if (item == null)
                return;

            ItemTileInfo info = this.GetItemInfo(item, true, index);

            if (info == null)
                return;

            if (info.IsClosed)
                return;

            LayoutItem li = this.GetLayoutItem(item, null, false, false);

            if (li != null)
            {
                Rect panelRect = new Rect( new Point(), this._panel.TileAreaSize);

                if ( this._itemsInView.Contains(info) )
                {
                    FrameworkElement container = li.Container as FrameworkElement;

                    if ( container != null )
                    {
                        this.MakeVisible(container, Rect.Empty);
                        return;
                    }
                }
            }

            if (info.OccupiesScrollPosition &&
                (this._scrollTilesVertically || this._scrollTilesHorizontally))
            {
                
                double scrollOffset = this.GetOffsetFromScrollPosition(info.ScrollPosition);

                if (this._scrollTilesHorizontally)
                    ((IScrollInfo)this._panel).SetHorizontalOffset(scrollOffset);
                else if (this._scrollTilesVertically)
                    ((IScrollInfo)this._panel).SetVerticalOffset(scrollOffset);
                
                this._panel.Measure(this._panel.LastMeasureSize);
            }


        }

                #endregion //BringIndexIntoView	
    
                #region EndGeneration

        internal Size EndGeneration(TileAreaPanel.GenerationCache genCache)
        {






            // synchronize the _lastScrollPosition value asynchronously
            // since we can get called multiple times before the arrange logic
            // has started
			if (this._isSynchLastScrollPosOperationPending == false)
            {
                if (this._lastScrollPosition != this._scrollPosition ||
                    this._lastScrollLogicalOffset != this._scrollLogicalOffset)
                {
					_isSynchLastScrollPosOperationPending = true;

                    this._panel.Dispatcher.BeginInvoke(DispatcherPriority.Send, new TileUtilities.MethodDelegate(SynchLastScrollPosition));



                }
            }
        

            bool isScrollingVertically;

            GridBagLayoutManager gbl = GetOverallGridBagLayout(out isScrollingVertically);

            // clear the old _minimizedAreaActualExtent member.
            // If we are in maximized mode it will get reset when we call LayoutContainer below
            this._minimizedAreaActualExtent = 0;

            // layout all the items
            Rect layoutRect;
            
            gbl.LayoutContainer(this, gbl, out layoutRect);

            Size panelSize = this._panel.LastMeasureSize;

			// JJD 05/10/12 - TFS104615
			// if the panel measure size is infinite then use its desired size
			if (double.IsInfinity(panelSize.Height))
			{
				double height = _panel.DesiredSize.Height;

				if (height > 0)
					panelSize.Height = height;
			}

			// JJD 05/10/12 - TFS104615
			// if the panel measure size is infinite then use its desired size
			if (double.IsInfinity(panelSize.Width))
			{
				double width = _panel.DesiredSize.Width;

				if (width > 0)
					panelSize.Width = width;
			}

			Rect panelRect = new Rect(new Point(), panelSize);

            double? scrollingOffset = null;

            #region Calculate a pixel scrolling offset (if applicable)

            // see if the scroll position has changed since the last arrange
            if (this._lastScrollPosition != this._scrollPosition &&
                this._lastItemLocations != null )
            {
                bool oldAndNewItemFound = false;
                int lastItemArrangeCount = this._lastItemLocations.Count;

                #region Check the old in view items looking for an old item that is still in view

                // loop over the items that were in view on the last arrange
                // looking for tile that is scrollable that is still in view
                for (int i = 0; i < lastItemArrangeCount; i++)
                {
                    ItemRowColumnSizeInfo rci = this._lastItemLocations[i];

                    ItemTileInfo info = rci.Info;

                    // bypass maximized tiles since they don't scroll
                    if (info == null || info.IsMaximized)
                        continue;

                    LayoutItem li = this.GetLayoutItem(info.Item);

                    if (li == null)
                        continue;

                    // check the map first since this is most efficient
                    if (this._itemsToBeArranged.Contains(li))
                    {
                        // verify that the item is in the _itemsInView list
                        int index = this._itemsInView.IndexOf(info);

                        // Debug.Assert(index >= 0, "The item should be in the map and the itemsInView list at this point");

                        if (index >= 0)
                        {
                            if (oldAndNewItemFound == false)
                            {
                                Rect oldTargetRect = rci.TargetRect;
                                Rect newTargetRect = li.TargetRect;

                                // calculate the delta between the old and new target rects
                                if (isScrollingVertically)
                                    scrollingOffset = oldTargetRect.Top - newTargetRect.Top;
                                else
                                    scrollingOffset = oldTargetRect.Left - newTargetRect.Left;

                                if (CoreUtilities.AreClose(scrollingOffset.Value, 0))
                                    scrollingOffset = null;

                                oldAndNewItemFound = true;
                            }

                            
                            
                        }
                        else
                        {
                            
                            // Set the Target to what it was last time
                            li.SetTargetRect(rci.TargetRect, false);
                        }
                    }
                    else
                    {
                        
                        // Set the Target to what it was last time
                        li.SetTargetRect(rci.TargetRect, false);
                    }

                    
                    // Initialize the original rect
                    if (li.CurrentRectInternal != null)
                        li.SetOriginalRect(li.CurrentRectInternal.Value, false);
                    else
                        li.SetOriginalRect(rci.TargetRect, false);
                }

                #endregion //Check the old in view items looking for an old item that is still in view
                

                // if we didn't have any element that was in both the old and new 
                // layouts then set that scfrolling offset based on the size of the 
                // panel
                if (scrollingOffset == null && oldAndNewItemFound == false)
                {
					if (isScrollingVertically)
					{
						// JJD 05/10/12 - TFS104615
						// Only use the panel size if it isn't infinite
						if (!double.IsInfinity(panelSize.Height))
						{
							if (this._lastScrollPosition < this._scrollPosition)
								scrollingOffset = panelSize.Height;
							else
								scrollingOffset = -panelSize.Height;
						}
					}
					else
					{
						// JJD 05/10/12 - TFS104615
						// Only use the panel size if it isn't infinite
						if (!double.IsInfinity(panelSize.Width))
						{
							if (this._lastScrollPosition < this._scrollPosition)
								scrollingOffset = panelSize.Width;
							else
								scrollingOffset = -panelSize.Width;
						}
					}
                }
            }

            #endregion //Calculate a pixel scrolling offset (if applicable)

            // if we have a scrolling offset then initialize the current rect of any newly added
            // items
            if (scrollingOffset.HasValue)
            {
                foreach (LayoutItem li in genCache._newItemsToBeArrangedTempCache)
                {
                    Rect currentRect = li.TargetRect;

                    
                    // Make sure the target rect has a size 
                    // Otherwise set its target to a non-visible location
                    if (currentRect.Width == 0 &&
                        currentRect.Height == 0)
                    {
                        currentRect = new Rect(-10000, -10000, 100, 100);
                        li.SetTargetRect(currentRect, false);
                    }

                    if (isScrollingVertically)
                        currentRect.Y += scrollingOffset.Value;
                    else
                        currentRect.X += scrollingOffset.Value;

                    li.SetOriginalRect( currentRect, false );
                    
                    
                    
                }
            }

            //Debug.WriteLine(string.Format("---- Before unused element check, panel rect: {0}", panelRect));

            // loop over the items that need to be moved out of the way
            // based on whether they are before or after the current scroll position
            
            
            //foreach (LayoutItem li in genCache._previousItemsToBeArrangedTempCache)
            foreach (LayoutItem li in genCache._previousItemsToBeArrangedTempCache.Values)
            {
                ItemTileInfo info = this.GetItemInfo(li.Item);
                Rect targetRect = li.TargetRect;

                
                // If the target rect has no size at this point then remove it 
                // from the _itemsToBeArranged and _itemsInView caches
                if (targetRect.Width == 0)
                {
                    if (this._itemsToBeArranged.Contains(li))
                        this._itemsToBeArranged.Remove(li);

                    int index = this._itemsInView.IndexOf(info);

                    if ( index >= 0 )
                        this._itemsInView.RemoveAt(index);

					//li.RecyclingRelease();

					continue;
                }

                if (scrollingOffset.HasValue)
                {
                    
                    
                    
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


                    if (isScrollingVertically)
                        targetRect.Y -= scrollingOffset.Value;
                    else
                        targetRect.X -= scrollingOffset.Value;
//#if DEBUG
//                    ContentControl cc = li.Container as ContentControl;

//                    Debug.WriteLine(string.Format("SetTargetRect, rect: {0}, LI Hashcode: {1}, Tile content: {2}", targetRect, li.GetHashCode(), cc != null ? cc.Content : null));
//#endif

                    li.SetTargetRect(targetRect, true);

                    double adjustment = scrollingOffset.Value > 0 ? 1 : -1;

                    while (TileUtilities.IntersectsWith( targetRect, panelRect))
                    {
                        if (isScrollingVertically)
                            targetRect.Y -= adjustment;
                        else
                            targetRect.X -= adjustment;
                        
                        li.SetTargetRect(targetRect, true);
                    
                        
                    }
                }

                if (TileUtilities.IntersectsWith(targetRect, panelRect))
                {
                    if (info != null &&
                         info.ScrollPosition < this._scrollPosition)
                    {
						if (isScrollingVertically)
						{
							// JJD 05/10/12 - TFS104615
							// Only use the panel size if it isn't infinite
							if (!double.IsInfinity(panelSize.Height))
								targetRect.Y = (-Math.Min(Math.Abs(targetRect.Bottom), panelSize.Height)) - 1;
						}
						else
						{
							// JJD 05/10/12 - TFS104615
							// Only use the panel size if it isn't infinite
							if (!double.IsInfinity(panelSize.Width))
								targetRect.X = (-Math.Min(Math.Abs(targetRect.Right), panelSize.Width)) - 1;
						}
                    }
                    else
                    {
						if (isScrollingVertically)
						{
							// JJD 05/10/12 - TFS104615
							// Only use the panel size if it isn't infinite
							if (!double.IsInfinity(panelSize.Height))
								targetRect.Y = Math.Max(Math.Abs(targetRect.Y), 0) + panelSize.Height + 1;
						}
						else
						{
							// JJD 05/10/12 - TFS104615
							// Only use the panel size if it isn't infinite
							if (!double.IsInfinity(panelSize.Width))
								targetRect.X = Math.Max(Math.Abs(targetRect.X), 0) + panelSize.Width + 1;
						}
                    }

                    // JJD 2/25/10 - TFS27941
                    // Check if the item still interects with the panel. If so adjust it
                    // in the non-scrolling dimension so it is out of view
                    if (TileUtilities.IntersectsWith(targetRect, panelRect))
                    {
                        if (isScrollingVertically)
                        {
							// JJD 05/10/12 - TFS104615
							// Only use the panel size if it isn't infinite
							if (!double.IsInfinity(panelSize.Width))
							{
								if (targetRect.X > panelRect.Width / 2)
									targetRect.X = panelRect.Right + 1;
								else
									targetRect.X = -(targetRect.Width + 1);
							}
                        }
                        else
                        {
							// JJD 05/10/12 - TFS104615
							// Only use the panel size if it isn't infinite
							if (!double.IsInfinity(panelSize.Height))
							{
								if (targetRect.Y > panelRect.Height / 2)
									targetRect.Y = panelRect.Bottom + 1;
								else
									targetRect.Y = -(targetRect.Height + 1);
							}
                        }

                    }

                    //Debug.WriteLine(string.Format("Move unused element, rect: {0}, item: {1}", targetRect, li.Item));

                    li.SetTargetRect(targetRect, true);
                }
            }

            IList childElements = this._panel.Children;
            int count = childElements.Count;

            // make sure all child elements are accounted for. This can
            // happen because generation enumerators are usually called an extra time
            // to determine if the element will fit
            if (count > this._itemsToBeArranged.Count)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    XamTile child = childElements[i] as XamTile;

                    
                    // Bypass collapsed elements
                    if (child == null || child.Visibility == Visibility.Collapsed)
                    {
                        count--;
                        continue;
                    }

                    LayoutItem li = this.GetLayoutItem(null, child, false, false);

                    
                    // If the layout item is not in the _itemsToBeArranged cache
                    // then add it now
                    if (li != null && !this._itemsToBeArranged.Contains(li))
                    {
                        
                        
                        //this._itemsToBeArranged.Add(li);
                        AddItemToBeArranged(li);

                        if (count <= this._itemsToBeArranged.Count)
                            break;
                    }
                }
            }

            Size desiredSize = new Size( layoutRect.Width, layoutRect.Height );
            Size tileAreaSize = this._panel.TileAreaSize;


            // make sure that if we are expanding to fit we return the larger
            // of the desired size and the preferred size
            if (gbl.ExpandToFitHeight && !double.IsPositiveInfinity(tileAreaSize.Height))
                desiredSize.Height = Math.Max(desiredSize.Height, tileAreaSize.Height);

            if (gbl.ExpandToFitWidth && !double.IsPositiveInfinity(tileAreaSize.Width))
                desiredSize.Width = Math.Max(desiredSize.Width, tileAreaSize.Width);

            Size size = desiredSize;
            
            if ( !double.IsPositiveInfinity(tileAreaSize.Height))
                size.Height = Math.Max(size.Height, tileAreaSize.Height);

            if (!double.IsPositiveInfinity(tileAreaSize.Width))
                size.Width = Math.Max(size.Width, tileAreaSize.Width);

            this._lastMeasureDesiredSize = size;

            bool requiresAnotherPass = this.UpdateScrollingInfo(gbl, size, isScrollingVertically, false);

            if ( requiresAnotherPass )
                genCache._requiresAnotherLayoutPass = true;

            // clear out the temporary chaced values
            this._lastTile = null;
            this._lastElementConstraints = null;
            this._lastElementDefaultConstraints = null;
            this._lastElementState = null;
            
            this._genCache = null;

			// JJD 5/11/11 - TFS75082
			// In the situation where we are in maximized mode and we are showing all tiles
			// we need to constrain one dimension (based on the maximized tile location)
			// such that we don't return a desired size > than the measure size of the panel
			if (this._isInMaximizedMode && this._lastShowAllTiles)
			{
				switch (this._maximizedTileLocation)
				{
					case MaximizedTileLocation.Left:
					case MaximizedTileLocation.Right:
						desiredSize.Height = Math.Min(desiredSize.Height, _panel.LastMeasureSize.Height);
						break;
					case MaximizedTileLocation.Top:
					case MaximizedTileLocation.Bottom:
						desiredSize.Width = Math.Min(desiredSize.Width, _panel.LastMeasureSize.Width);
						break;
				}
			}
            return desiredSize;
        }

        private GridBagLayoutManager GetOverallGridBagLayout(out bool isScrollingVertically)
        {
            GridBagLayoutManager gbl;

            if (this._isInMaximizedMode)
            {
                gbl = this.GridBagLayoutMaximizedModeOverall;
                gbl.InvalidateLayout();

                
                
                
                
                isScrollingVertically = this.IsMaximizedAreaLeftOrRight;
            }
            else
            {
                switch (this._tileLayoutOrder)
                {
                    default:
                    case TileLayoutOrder.Vertical:
                    case TileLayoutOrder.Horizontal:
                        gbl = this.GridBagLayoutNormal;
                        break;
                    case TileLayoutOrder.UseExplicitRowColumnOnTile:
						// JJD 5/9/11 - TFS74206 - added ExplicitLayoutTileSizeBehavior
						if (this._explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAndHeights)
							gbl = this.GridBagLayoutNormal;
						else
							gbl = this.GridBagLayoutNormalModeOverall;
						break;
                    case TileLayoutOrder.VerticalVariable:
                    case TileLayoutOrder.HorizontalVariable:
                        gbl = this.GridBagLayoutNormalModeOverall;
                        break;
                }

                switch (this._tileLayoutOrder)
                {
                    case TileLayoutOrder.Horizontal:
                    case TileLayoutOrder.HorizontalVariable:
                        isScrollingVertically = true;
                        break;

                    default:
                        isScrollingVertically = false;
                        break;
                }
            }

            return gbl;
        }

                #endregion //EndGeneration	

                #region GenerateElements

        internal void GenerateElements(TileAreaPanel.GenerationCache genCache)
        {
			// JJD 05/08/12 - TFS104786
			// Clear the lastTile member.
			// Otherwise, in a situation where there is only one tile it is possible to pick
			// up stale constrains for that tile
			_lastTile = null;

            
            // When we are animating first do a pass over the previously arranged elements
            // and any that are still in view we want to gen the elements now so they
            // don't get reused for other items during this pass
            if (this._owningManager.ShouldAnimate)
            {
                Rect panelRect = new Rect(new Point(), this._panel.TileAreaSize);




                // loop over the items that were previously arranged in view but are
                // no longer (since they still need to be arranged out of view)
                // we need to make sure they are gened
				// Copy the items into a temp list to protect against any changes during the foreach loop below
				List<LayoutItem> tempSnapshot = new List<LayoutItem>(genCache._previousItemsToBeArrangedTempCache.Values);

				foreach (LayoutItem li in tempSnapshot)
                {
                    ItemTileInfo info = this.GetItemInfo(li.Item);

                    if (info == null)
                        continue;

                    UIElement container = li.Container as UIElement;

                    if (container == null || container.Visibility == Visibility.Collapsed)
                        continue;

                    

                    // call GenerateElement to make sure the associated tile has been gened during this cycle.
                    // Otherwise, the recycling generator will de-activate it.
                    if (info.Index >= 0)
                    {
                        Rect currentRect;

                        if (li.CurrentRectInternal != null)
                        {
                            currentRect = li.CurrentRectInternal.Value;

                            li.SetOriginalRect(currentRect, false);
                        }
                        else
                            currentRect = li.TargetRect;

                        if (TileUtilities.IntersectsWith(panelRect, currentRect) ||
                            TileUtilities.IntersectsWith(panelRect, li.TargetRect))
                        {
                            UIElement generatedElement = genCache.GenerateElement(info.Index);

                            Debug.Assert(li.Container == generatedElement, "Wrong tile");



                        }
                    }
                }
            }





            this._totalColumnsDisplayed = 0;
            this._totalRowsDisplayed = 0;

            if (this._isInMaximizedMode)
            {
                GridBagLayoutManager gbl = this.GridBagLayoutMaximizedModeOverall;

                switch (this._maximizedTileLocation)
                {
                    case MaximizedTileLocation.Top:
                    case MaximizedTileLocation.Bottom:
                        gbl.InterItemSpacingHorizontal  = 0;
                        gbl.InterItemSpacingVertical    = this._panel.GetInterTileAreaSpacing();
                        break;

                    default:
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Right:
                        gbl.InterItemSpacingHorizontal = this._panel.GetInterTileAreaSpacing();
                        gbl.InterItemSpacingVertical    = 0;
                        break;
                }

                this.GenerateMaximizedElements(genCache);

                this._wasLastScrollableItemGened = false;

                this.GenerateMinimizedElements(genCache);
            }
            else
            {
                this.GenerateNormalElements(genCache);
            }

            
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

        }

                #endregion //GenerateElements	

                #region GetCurrentColumn

        internal int GetCurrentColumn(ItemTileInfo info)
        {
            return this.GetCurrenRowColumnHelper(info, true);
        }

                #endregion //GetCurrentColumn

                #region GetCurrentRow

        internal int GetCurrentRow(ItemTileInfo info)
        {
            return this.GetCurrenRowColumnHelper(info, false);
        }

                #endregion //GetCurrentRow	
 
                // JJD 2/25/10 - TFS28159 added
                #region GetInViewRect

        internal Rect GetInViewRect(XamTile container)
        {

            if (this._panel == null)
                return Rect.Empty;

            object item;
            
            if (!this._containerToItemMap.TryGetValue(container, out item))
                return Rect.Empty;

            LayoutItem li;

            if (!this._layoutItemMap.TryGetValue(item, out li))
                return Rect.Empty;

            ItemTileInfo info = li.ItemInfo;

            if (info == null || false == this._itemsInView.Contains(info))
                return Rect.Empty;

            Rect targetRect = li.TargetRect;
            Rect panelRect = new Rect(new Point(), this._panel.TileAreaSize);

            Vector offset;

            // get any pixel offset value
            if (info.IsMaximized)
                offset = this.ArrangeOffsetForMaxmizedTiles;
            else
                offset = this.ArrangeOffset;

            // Offset the target rect by that amount
            targetRect.X += offset.X;
            targetRect.Y += offset.Y;

            Rect intersection = panelRect;
            intersection.Intersect(targetRect);

            if (intersection.IsEmpty)
                return intersection;

            // Offset the intersection so that it is in coordinates
            // relative to the tile, not the panel.
            intersection.X -= targetRect.Left;
			intersection.Y -= targetRect.Top;

            return intersection;
        }

                #endregion //GetInViewRect	
    
                #region GetItem

        internal object GetItem(int index, bool useRearrangedOrder)
        {
            if ( useRearrangedOrder && index < this._sparseArray.Count)
            {
                // if the slot is already filled in the _sparseArray
                // then return it. Otherwise, we need to fall thru the logic below
                // so both the original and sparse array slots get filled
                ItemTileInfo temp = (ItemTileInfo)this._sparseArray[index];

                if ( temp != null )
                    return temp.Item;
            }

            // get the item from the underlying list
            object item = this.Items[index];

            this.EnsureItemArrayIsAllocated();

            object itemInArray = this._itemsInOriginalOrder[index];

            // if the slot in the original array is not already filled then do that now
            if (itemInArray == null)
                this._itemsInOriginalOrder[index] = item;
            else
            {
                Debug.Assert(Object.Equals(item, itemInArray), "ItemsInOriginalOrder array out of sync");
            }

            ItemTileInfo sparseArrayItem = (ItemTileInfo)this._sparseArray[index];

            // if the corresponding slot in the rearranged array is not already filled then do that now
            if (sparseArrayItem == null)
            {
                Debug.Assert(itemInArray == null, "itemInRearrangedArray slot should only be empty if corresponding slot in original array is empty.");

                sparseArrayItem = this.GetItemInfo(item);

                this._sparseArray[index] = sparseArrayItem;
            }
            else
            {
                Debug.Assert(itemInArray != null, "itemInRearrangedArray slot should only be filled if corresponding slot in original array is filled.");
            }

            if (useRearrangedOrder && sparseArrayItem != null)
                return sparseArrayItem.Item;

            return item;
        }

                #endregion //GetItem	

                #region GetItemAtScrollIndex

        internal ItemTileInfo GetItemAtScrollIndex(int scrollindex)
        {
            if ( scrollindex < this._sparseArray.ScrollCount)
            {
                // if the slot is already filled in the _sparseArray
                // then return it. Otherwise, we need to fall thru the logic below
                // so both the original and sparseArray slots get filled
                ItemTileInfo temp = (ItemTileInfo)this._sparseArray.GetItemAtScrollIndex(scrollindex);

                if ( temp != null )
                    return temp;
            }

            int count = this.Items.Count;

            // since there can only be the same or more items in the 
            // underlying list than those that occupy scroll positions
            // the scrollindex can never be greater than the list index
            // for null slots. Therefore we can start at the scrollindex
            // and check each item's scrollindex until we find the
            // one that matches
            for (int i = scrollindex; i < count; i++)
            {
                object item = this.GetItem(i, true);

                if ( item != null )
                {
                    ItemTileInfo info = this.GetItemInfo(item);

                    if ( this._sparseArray.GetScrollIndexOfItem(info) == scrollindex )
                        return info;
                }
            }

            return null;
        }

                #endregion //GetItemAtScrollIndex	

				#region GetItemFromContainer

		internal object GetItemFromContainer(XamTile container)
		{
			object item;
			if (_containerToItemMap.TryGetValue(container, out item))
				return item;

			return null;
		}

				#endregion //GetItemFromContainer	
    
                #region GetItemsInView

        internal ItemTileInfo[] GetItemsInView()
        {
            return this._itemsInView.ToArray();
        }

                #endregion //GetItemsInView	
    
				// JJD 02/24/12 - TFS101202 - added
                #region GetItemsInViewCount

        internal int GetItemsInViewCount()
        {
            return this._itemsInView.Count;
        }

                #endregion //GetItemsInViewCount	

                #region GetItemsToBeArranged

        internal IEnumerator GetItemsToBeArranged()
        {
            return this._itemsToBeArranged.GetEnumerator();
        }

                #endregion //GetItemsToBeArranged	

                #region GetLayoutItem

        internal LayoutItem GetLayoutItem(object item)
        {
            return this.GetLayoutItem(item, null, false, false);
        }

        internal LayoutItem GetLayoutItem(object item, XamTile container, bool createIfNotFound, bool clearDimensions)
        {
            LayoutItem layoutItem;

            if (container == null)
            {
                container = this._owningManager.TileFromItem(item);

				//if (tile == null)
				//    return null;
            }

            // Maintain tile to item map
            if (container != null)
            {
                object itemInMap;
                if (this._containerToItemMap.TryGetValue(container, out itemInMap))
                {
					// MD 5/13/10 - TFS32040
					// If the items being stored are value types, they will be boxed and the boxes will be different objects,
					// so the equality check will always return False. To get around this, we need to call Object.Equals.
                    //if (item != itemInMap)
					if (Object.Equals(itemInMap, item) == false)
                    {
                        // if the item was passed in then update the map
                        // Otherwise set item to itemInMap so we can use it below
                        // to get the layout item
                        if (item != null)
                        {
							// MD 5/13/10 - TFS32040
							// We already got the value at the key of tile, so we don't need to get it again.
							// All references to oldItem below are changed to itemInMap.
                            //object oldItem = this._containerToItemMap[tile];

                            this._containerToItemMap[container] = item;

							
                            // If the item changes then remove the old entry from the cached lists
							// MD 5/13/10 - TFS32040
							// Since oldItem is the same as itemInMap, we already checked that these items are not equal above.
							// We don't need to check it again.
							//if (oldItem != item)
							//{
                                LayoutItem oldLi;
								// MD 5/13/10 - TFS32040
								// We want to get in here when we can get the value, not when we can't get the value. I have 
								// removed the '!' and changed the oldItem reference to use itemInMap instead.
                                //if (!this._layoutItemMap.TryGetValue(oldItem, out oldLi))
								if (this._layoutItemMap.TryGetValue(itemInMap, out oldLi))
                                {
                                    this._itemsToBeArranged.Remove(oldLi);

									oldLi.RecyclingRelease(false);

                                    if (this._genCache != null &&
                                         this._genCache._previousItemsToBeArrangedTempCache.ContainsKey(container))
                                        this._genCache._previousItemsToBeArrangedTempCache.Remove(container);
                                }
                            //}
                        }
                        else
                            item = itemInMap;
                    }
                }
                else
                {
                    // if the item was passed in the update the map
                    if ( item != null )
                        this._containerToItemMap.Add(container, item);
                }
            }
                    

            if (!this._layoutItemMap.TryGetValue(item, out layoutItem))
            {
                if (createIfNotFound == false)
                    return null;

                layoutItem = new LayoutItem(container, item, this);

                this._layoutItemMap.Add(item, layoutItem);

                
                // Make sure there is no duplicate entry
                this.RemoveDuplicateContainerRefs(layoutItem);
            }
            else
            {
                if (container != null)
                {
                    DependencyObject oldContainer = layoutItem.Container;

                    layoutItem.InitializeContainer(container, clearDimensions);

                    
                    // If the tile changes then remove any old entry
                    if (container != oldContainer)
                        this.RemoveDuplicateContainerRefs(layoutItem);
                }
            }

            return layoutItem;
        }

                #endregion //GetLayoutItem	

				#region GetLayoutItemFromMap

		internal LayoutItem GetLayoutItemFromMap(object item)
		{
			LayoutItem li;

			this._layoutItemMap.TryGetValue(item, out li);

			return li;
		}

				#endregion //GetLayoutItemFromMap	
    
                #region GetLayoutItemFromPoint

        internal LayoutItem GetLayoutItemFromPoint(Point ptRelativeToPanel, TileLayoutManager.LayoutItem bypassItem)
        {
            //Rect rectOverall = new Rect();

            Thickness padding = this._owningManager.TileAreaPadding;

            ptRelativeToPanel.X -= padding.Left;
			ptRelativeToPanel.Y -= padding.Top;

            foreach (LayoutItem li in this._itemsToBeArranged)
            {
                if (li == bypassItem)
                    continue;

                Rect rect = li.TargetRect;

                if (rect.Contains(ptRelativeToPanel))
                    return li;

                //rectOverall.X = Math.Min(rectOverall.X, rect.X);
                //rectOverall.Y = Math.Min(rectOverall.Y, rect.Y);
                //rectOverall.Height = Math.Max(rectOverall.Bottom, rect.Bottom) - rectOverall.Y;
                //rectOverall.Width = Math.Max(rectOverall.Right, rect.Right) - rectOverall.X);
            }

            return null;
        }

                #endregion //GetLayoutItemFromPoint	

                #region GetLayoutItemsToBeAnimated

        internal IEnumerator GetLayoutItemsToBeAnimated()
        {
            if (this._itemsToBeArranged == null ||
                 this._itemsToBeArranged.Count == 0)
                return null;

            return this._itemsToBeArranged.GetEnumerator();
        }

                #endregion //GetLayoutItemsToBeAnimated	

                #region GetLogicalIndexOfItem

        internal int GetLogicalIndexOfItem(object item, bool returnScrollPosition)
        {
            IList items = this.Items;

            ItemTileInfo info = this.GetItemInfo(item);

            Debug.Assert(info != null, "This should always return an item info");

            if (info == null)
                return -1;

            int index = this._sparseArray.IndexOf(info);

            if (index >= 0)
            {
                if ( returnScrollPosition )
                    return this._sparseArray.GetScrollIndexOfItem(info);
                else
                    return index;
            }

            this.EnsureItemArrayIsAllocated();

            int count = items.Count;

            // walk over each item until we reach the one we are looking for.
            // This will insure no empty slot islands are created
            for (int i = 0; i < count; i++)
            {
                if (Object.Equals(item, this.GetItem(i, true)))
                {
                    if (returnScrollPosition)
                        return this._sparseArray.GetScrollIndexOfItem(info);
                    else
                        return i;
                }
            }

            return -1;
        }

                #endregion //GetLogicalIndexOfItem	

                #region GetMaximizedModeTileArea

        internal Rect GetMaximizedModeTileArea(TileArea area, bool clipInsideTileArea)
        {
            if ( this._panel == null )
                return Rect.Empty;

            Rect tileAreaOverall = this._panel.GetTileArea();

            if (this._isInMaximizedMode == true)
            {
                TileAreaLayoutItem liArea = GetTileAreaLayoutItem(area);

                if (liArea != null)
                {
                    Rect areaRect = liArea.Rect;

                    areaRect.X += tileAreaOverall.Left;
					areaRect.Y += tileAreaOverall.Top;

					Vector offset;
                    if (area == TileArea.MaximizedTiles)
                        offset = this.ArrangeOffsetForMaxmizedTiles;
                    else
                        offset = this.ArrangeOffset;

					areaRect.X += offset.X;
					areaRect.Y += offset.Y;

                    if (clipInsideTileArea)
                        areaRect.Intersect( tileAreaOverall);

                    return areaRect;
                }
            }
            
            return tileAreaOverall;
        }

                #endregion //GetMaximizedModeTileArea	

                #region GetMaximizedModeTileAreaConstraints

        internal ITileConstraints GetMaximizedModeTileAreaConstraints(TileArea area)
        {
            if (this._isInMaximizedMode == true)
                return this.GetTileAreaLayoutItem(area);
            
            return null;
        }

                #endregion //GetMaximizedModeTileAreaConstraints	

                #region GetOffsetFromScrollPosition

        internal double GetOffsetFromScrollPosition(int scrollPosition)
        {
            if (scrollPosition == 0)
                return 0;

            if (this._panel == null)
                return 0;

            int scrollableItemCount = this._sparseArray.ScrollCount;

            if (this._isInMaximizedMode)
                return Math.Min(scrollPosition, Math.Max(scrollableItemCount - 1, 0));

            if (this._owningManager.NormalModeSettingsSafe.ShowAllTiles)
                return 0;

            switch (this._tileLayoutOrder)
            {
                case TileLayoutOrder.UseExplicitRowColumnOnTile:
                    return 0;

                case TileLayoutOrder.HorizontalVariable:
                case TileLayoutOrder.VerticalVariable:
                    {
                        GridBagLayoutManager gbl = this.GridBagLayoutNormalModeOverall;

                        int count = gbl.LayoutItems.Count;

                        if (count > 0)
                       {
                            // see if the scrollpostion is in view
                            if ( scrollPosition >= this._scrollPosition &&
                                 scrollPosition <= this._scrollPositionOfLastArrangedItem)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    TileRowColumnLayoutItem rcItem = gbl.LayoutItems[i] as TileRowColumnLayoutItem;
                                    if ( scrollPosition >= rcItem.GetFirstScrollPosition() &&
                                         scrollPosition <= rcItem.GetLastScrollPosition())
                                        return rcItem.ScrollOffset;
                                }
                            }

                            double avgTilesPerRowCol    = Math.Max(this._variableAvgItemsPerRowColumn, 1);

                            if (scrollPosition > this._scrollPositionOfLastArrangedItem)
                                return (int)Math.Min(this._scrollLogicalOffsetOfLastArrangedItem
                                    + ((scrollPosition - this._scrollPositionOfLastArrangedItem) / avgTilesPerRowCol), this._variableTotalRowColmuns);
                            else
                                return (int)Math.Max(this._scrollLogicalOffset - ((this._scrollPosition - scrollPosition) / avgTilesPerRowCol), 0);
                              
                        }

                        return 0;
                    }

                case TileLayoutOrder.Horizontal:
                case TileLayoutOrder.Vertical:
                    {
                        int itemsPerRowCol = Math.Max( this.NormalModeItemsPerRowColumn, 1 );
                        return Math.Floor( (double)Math.Min(this.SparseArray.ScrollCount - 1, scrollPosition) / itemsPerRowCol );
                    }

            }
            return 0;
        }

                #endregion //GetOffsetFromScrollPosition	

                #region GetResizeRange

        internal void GetResizeRange(ItemTileInfo itemInfo, out double maxDeltaLeft, out double maxDeltaRight,
                out double maxDeltaTop, out double maxDeltaBottom)
        {
            LayoutItem li = this.GetLayoutItem(itemInfo.Item);

            if (li != null)
            {
				// JJD 5/9/11 - TFS74206 
				// Changed out parameter type
				//TileRowColumnLayoutItem rcli;
				ILayoutItem rcli;

                GridBagLayoutManager gbl = this.GetContainingGridBagLayoutManager(li, out rcli);

                if (gbl != null)
                {
                    object layoutContext = rcli != null ? (object)rcli : gbl;

                    gbl.GetResizeRange(this, layoutContext, li, out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom);
                    return;
                }
            }

            maxDeltaLeft    = 0;
            maxDeltaRight   = 0;
            maxDeltaTop     = 0;
            maxDeltaBottom  = 0;
        }

                #endregion //GetResizeRange	

                #region GetScrollPositionFromOffset

        internal int GetScrollPositionFromOffset(double offset)
        {
            if ((int)offset == 0)
                return 0;

            if (this._panel == null)
                return 0;

            int scrollableItemCount = this._sparseArray.ScrollCount;

            if (this._isInMaximizedMode)
                return Math.Min((int)offset, Math.Max(scrollableItemCount - 1, 0));

            if (this._owningManager.NormalModeSettingsSafe.ShowAllTiles)
                return 0;

            // JJD 2/22/10 - TFS26056
            // Deal with the offset as an integer
            offset = Math.Floor(offset);

            switch (this._tileLayoutOrder)
            {
                case TileLayoutOrder.UseExplicitRowColumnOnTile:
                    return 0;

                case TileLayoutOrder.HorizontalVariable:
                case TileLayoutOrder.VerticalVariable:
                    {
                        GridBagLayoutManager gbl = this.GridBagLayoutNormalModeOverall;

                        int count = gbl.LayoutItems.Count;

                        if ( count > 0)
                        {
                            // see if the scrollpostion is in view
                            if ((int)offset >= this._scrollLogicalOffset &&
                                 (int)offset < this._scrollLogicalOffset + count)
                            {
                                TileRowColumnLayoutItem rcItem = gbl.LayoutItems[(int)offset - this._scrollLogicalOffset] as TileRowColumnLayoutItem;
                                
                                return rcItem.GetFirstScrollPosition();
                            }

                            double avgTilesPerRowCol = Math.Max(this._variableAvgItemsPerRowColumn, 1);

                            if ( this._scrollLogicalOffsetOfLastArrangedItem < (int)offset )
                                return (int)Math.Min( this.SparseArray.ScrollCount - 1,
                                    
                                    
                                    
                                    this._scrollPosition + ((offset - this._scrollLogicalOffset) * avgTilesPerRowCol));
                            else
                                return (int)Math.Max(this._scrollPosition - ((this._scrollLogicalOffset - offset) * avgTilesPerRowCol), 0);

                        }

                        return 0;
                    }

                case TileLayoutOrder.Horizontal:
                case TileLayoutOrder.Vertical:
                    {
                        int itemsPerRowCol = Math.Max( this.NormalModeItemsPerRowColumn, 1 );
                        return Math.Min(Math.Max(this.SparseArray.ScrollCount - 1, 0), itemsPerRowCol * (int)offset);
                    }

            }
            return 0;
        }

                #endregion //GetScrollPositionFromOffset	

                #region InitializeLayoutItem
    
        internal LayoutItem InitializeLayoutItem(XamTile container, object item, Dock enterFromSide)
        {
            LayoutItem layoutItem = this.GetLayoutItem(item, container, true, true);

            layoutItem.EnterFromSide = enterFromSide;
            layoutItem.SetDimensions(null);

            return layoutItem;
        }

                #endregion //InitializeLayoutItem
    
                #region InitializePanel

        /// <summary>
        /// For internal use only
        /// </summary>
        /// <param name="panel"></param>
        internal protected void InitializePanel(TileAreaPanel panel)
        {
            
            // clear all the maps
			foreach (LayoutItem li in _layoutItemMap.Values)
				li.RecyclingRelease(true);

            this._itemsToBeArranged.Clear();
            this._itemsInView.Clear();
            this._lastItemLocations = null;
            this._containerToItemMap.Clear();
            this._layoutItemMap.Clear();

            this._panel = panel;

			if (_panel != null)
				this.InitializeAllItems();

       }

                #endregion //InitializePanel	
 
                #region IsContainerInArrangeCache

        internal bool IsContainerInArrangeCache(XamTile container)
        {
            object item;
            
            if (!this._containerToItemMap.TryGetValue(container, out item))
                return false;

            LayoutItem li;

            if (this._layoutItemMap.TryGetValue(item, out li))
                return this._itemsToBeArranged.Contains(li);

            return false;
        }

                #endregion //IsContainerInArrangeCache	

                #region IsItemInView

        internal bool IsItemInView(ItemTileInfo item)
        {
            return this._itemsInView.Contains(item);
        }

                #endregion //IsItemInView	
        
                #region MakeVisible


        internal Rect MakeVisible(Visual visual, Rect rectangle)



        {
            if (this._panel == null)
                return Rect.Empty;

            UIElement element = visual as UIElement;

            
            // if they didn't pass in a rect then initialize it
            if ( rectangle.IsEmpty && element != null)
                rectangle = new Rect(new Point(), element.RenderSize);

            DependencyObject child = visual;
            DependencyObject parent = visual;

            // walk up the tree looking for the immediate child of the panel
            while (parent != null && parent != this._panel)
            {
                child = parent;
                parent = VisualTreeHelper.GetParent(child);
            }

            XamTile tile = child as XamTile;

            if (parent == null || tile == null)
                return Rect.Empty;

            ItemTileInfo info = null;
            LayoutItem li = this.GetLayoutItem(null, tile, false, false);

			//if (li == null)
			//{
			//    ContentControl cc = feChild as ContentControl;

			//    if ( cc != null )
			//        li = this.GetLayoutItem(cc.Content, null, true, false);
			//}

            //Debug.Assert(li != null, "We should have a LayoutItem for every child of the TileAreaPanel");

            if ( li == null )
                return Rect.Empty;

            info = li.ItemInfo;

            Debug.Assert(info != null, "We should have a ItemTileInfo for every child of the TileAreaPanel");

            if ( info == null )
                return Rect.Empty;

            // get the panel rect
            Rect panelRect = new Rect(new Point(), this._panel.TileAreaSize);
            Rect liRect;

			// JJD 07/09/12 - TFS114908
			// Cache the current scroll offset
			Vector originalScrollOffset = this._panel.ScrollDataInfo._offset;

			// JJD 07/09/12 - TFS114908
			// Moved requestedRectInPanelCoords xform logic from inside if block
			// below to here
			GeneralTransform xform = element.TransformToVisual(_panel);

            // convert the passed in rect to panel coordinates
            Point offset = xform.Transform(new Point(0, 0));
			Rect requestedRectInPanelCoords = rectangle;
			requestedRectInPanelCoords.X += offset.X;
			requestedRectInPanelCoords.Y += offset.Y;

            if (tile.Visibility == Visibility.Visible && this._itemsInView.Contains(info))
            {
                // get the targetRect of the Item
                liRect = li.TargetRect;

                // if the panel completely contains the item then return
                if (panelRect.Contains( new Point(liRect.Left, liRect.Top)) &&
					panelRect.Contains( new Point(liRect.Right, liRect.Bottom)))
                    return rectangle;

				// JJD 07/09/12 - TFS114908 - moved xform logic above
				//GeneralTransform xform = element.TransformToVisual(_panel);

				//// convert the passed in rect to panel coordinates
				//Point offset = xform.Transform(new Point(0, 0));
				////Rect requestedRectInPanelCoords = rectangle;
				//requestedRectInPanelCoords.X += offset.X;
				//requestedRectInPanelCoords.Y += offset.Y;

                // if the intersection completely contains the requested
                // rect then return
                if (panelRect.Contains(new Point(requestedRectInPanelCoords.Left, requestedRectInPanelCoords.Top)) &&
					panelRect.Contains(new Point(requestedRectInPanelCoords.Right, requestedRectInPanelCoords.Bottom)))
                    return rectangle;

				// JJD 07/09/12 - TFS114908 
				// If an animation is in progress then offset the requested rect so that
				// it represesnts were it will end up once the animation is complete
				if (_panel.IsAnimationInProgress)
				{
					Rect? currentRect = li.CurrentRectInternal;

					if (currentRect.HasValue)
					{
						requestedRectInPanelCoords.X += li.TargetRect.X - currentRect.Value.X;
						requestedRectInPanelCoords.Y += li.TargetRect.Y - currentRect.Value.Y;
					}
				}
			}

            // re-get the targetRect of the Item
            liRect = li.TargetRect;

			// do a first pass of pixel scrolling before we try to scroll the tiles
			// JJD 07/09/12 - TFS114908
			// Pass the xformed rectangle and the original scroll offset into the MakeVisiblePixelScrollingHelper method
            //bool wasScrollingPerformed = MakeVisiblePixelScrollingHelper(ref rectangle, ref panelRect, ref liRect);
			bool wasScrollingPerformed = MakeVisiblePixelScrollingHelper(requestedRectInPanelCoords, panelRect, liRect, originalScrollOffset);

            if (this._scrollTilesVertically || this._scrollTilesHorizontally)
            {
                #region For scrollable tiles scroll the tile into view

                if (info.OccupiesScrollPosition)
                {
                    ScrollDirection direction;

                    int scrollPosOfItem = info.ScrollPosition;
                    double scrollOffsetOfItem = this.GetOffsetFromScrollPosition(scrollPosOfItem);
                    int scrollableItemsInView = 0;

                    foreach (ItemTileInfo inViewInfo in this._itemsInView)
                    {
                        if (inViewInfo.OccupiesScrollPosition)
                            scrollableItemsInView++;
                    }

                    int newScrollPos = 0;
                    if (scrollPosOfItem < this._scrollPosition)
                    {
                        direction = ScrollDirection.Decrement;
                        newScrollPos = scrollPosOfItem;
                    }
                    else
                    {
                        direction = ScrollDirection.Increment;
                        newScrollPos = Math.Max(this._scrollPosition + 1, scrollPosOfItem - scrollableItemsInView);
                    }

                    double newScrollOffset = this.GetOffsetFromScrollPosition(newScrollPos);

                    TileAreaPanel.ScrollData scrollData = this._panel.ScrollDataInfo;

                    if (this._scrollTilesVertically)
                        scrollData._offset.Y = newScrollOffset;
                    else
                        scrollData._offset.X = newScrollOffset;

                    wasScrollingPerformed = true;

                    scrollData.VerifyScrollData(scrollData._viewport, scrollData._extent);

                    if (direction == ScrollDirection.Increment)
                    {
                        this._panel.Measure(this._panel.LastMeasureSize);

                        // re-get the targetRect of the Item
                        liRect = li.TargetRect;

                        double lastScrollOffset = -1;

                        // if the panel doesn't completely contain the item 
                        // keep performing scrolling operations until it does
                        // or we hit the scroll offset of the item or the
                        // offset hasn't changed since the last iteration
                        while (this._scrollLogicalOffset != scrollOffsetOfItem &&
                                this._scrollLogicalOffset != lastScrollOffset &&
                                (liRect.Width == 0 || !TileUtilities.IsContainedBy(panelRect, liRect, this._scrollTilesHorizontally)))
                        {
                            lastScrollOffset = this._scrollLogicalOffset;

                            if (this._scrollTilesVertically)
                                ((IScrollInfo)(this.Panel)).LineDown();
                            else
                                ((IScrollInfo)(this.Panel)).LineRight();

                            this._panel.Measure(this._panel.LastMeasureSize);

                            // re-get the targetRect of the Item
                            liRect = li.TargetRect;
                        }
                    }
                }

                #endregion //For scrollable tiles scroll the tile into view
            }

            // re-get the targetRect of the Item
            liRect = li.TargetRect;

            // after the tile has been scrolled into view do a second pass
            // in case we need to do pixel scrolling in the other dimension
			// JJD 07/09/12 - TFS114908
			// Pass the xformed rectangle and the original scroll offset into the MakeVisiblePixelScrollingHelper method
            //if (MakeVisiblePixelScrollingHelper(ref rectangle, ref panelRect, ref liRect) || wasScrollingPerformed)
			if (MakeVisiblePixelScrollingHelper(requestedRectInPanelCoords, panelRect, liRect, originalScrollOffset) || wasScrollingPerformed)
            {
                this._panel.InvalidateMeasure();
                this._panel.InvalidateArrange();
                return rectangle;
            }

            return Rect.Empty;
        }

		// JJD 07/09/12 - TFS114908 - added originalScrollOffset parameter
        //private bool MakeVisiblePixelScrollingHelper(ref Rect rectangle, ref Rect panelRect, ref Rect liRect)
		private bool MakeVisiblePixelScrollingHelper(Rect rectangle, Rect panelRect, Rect liRect, Vector originalScrollOffset)
        {
            if (liRect.Width == 0 || liRect.Height == 0)
                return false;

            TileAreaPanel.ScrollData scrollDataInfo = this._panel.ScrollDataInfo;

			// JJD 07/09/12 - TFS114908
			// use the passed in rectangle instead since it is now in panel coordinates
			//Rect testRect = new Rect(new Point(liRect.Left, liRect.Top), new Size(rectangle.Width, rectangle.Height));

			//testRect.X += rectangle.X;
			//testRect.Y += rectangle.Y;

            bool wasScrollingPerformed = false;

            #region Perform necessary pixel scrolling left to right

            if (this._scrollTilesHorizontally == false &&
                !TileUtilities.IsContainedBy(panelRect, rectangle, true))
            {
				// JJD 07/09/12 - TFS114908
				// use the passed in rectangle instead since it is now in panel coordinates
				//if (panelRect.Left > testRect.Left)
				if (panelRect.Left > rectangle.Left)
				{
					// JJD 07/09/12 - TFS114908
					// Set the scroll offset such that the rect's left will align with the left of the panel
					//scrollDataInfo._offset.X = Math.Max(0, testRect.Left - panelRect.Left);
					scrollDataInfo._offset.X = Math.Max( originalScrollOffset.X + rectangle.Left, 0);
				}
				else
				{
					// JJD 07/09/12 - TFS114908
					// Set the scroll offset such that the rect's right will align with the right of the panel
					//scrollDataInfo._offset.X = testRect.Right - panelRect.Right;
					scrollDataInfo._offset.X = Math.Max( originalScrollOffset.X + rectangle.Right - scrollDataInfo._viewport.Width, 0);
				}

                this._panel.InvalidateMeasure();

                wasScrollingPerformed = true;
            }

            #endregion //Perform necessary pixel scrolling left to right

            #region Perform necessary pixel scrolling top to bottom

            if (this._scrollTilesVertically == false &&
                !TileUtilities.IsContainedBy(panelRect, rectangle, false))
            {
				// JJD 07/09/12 - TFS114908
				// use the passed in rectangle instead since it is now in panel coordinates
				//if (panelRect.Top > testRect.Top)
				if (panelRect.Top > rectangle.Top)
				{
					// JJD 07/09/12 - TFS114908
					// Set the scroll offset such that the rect's top will align with the top of the panel
					//scrollDataInfo._offset.Y = Math.Max(0, testRect.Top - panelRect.Top);
					scrollDataInfo._offset.Y = Math.Max( originalScrollOffset.Y + rectangle.Top, 0);
				}
				else
				{
					// JJD 07/09/12 - TFS114908
					// Set the scroll offset such that the rect's bottom will align with the bottom of the panel
					//scrollDataInfo._offset.Y = testRect.Bottom - panelRect.Bottom;
					scrollDataInfo._offset.Y = Math.Max( originalScrollOffset.Y + rectangle.Bottom - scrollDataInfo._viewport.Height, 0);
				}

                this._panel.InvalidateMeasure();

                wasScrollingPerformed = true;
            }

            #endregion //Perform necessary pixel scrolling top to bottom

            scrollDataInfo.VerifyScrollData(scrollDataInfo._viewport, scrollDataInfo._extent);

            return wasScrollingPerformed;
        }

                #endregion //MakeVisible	
    
                #region MoveTileHelper

        internal bool MoveTileHelper(LayoutItem sourceItem, LayoutItem targetItem, bool calledFromDragManager)
        {
            if (this._panel == null)
                return false;

            if (sourceItem == null ||
                targetItem == null ||
                sourceItem == targetItem ||
                !(sourceItem.Container is FrameworkElement) ||
                !(targetItem.Container is FrameworkElement))
                return false;

            ItemTileInfo sourceInfo = sourceItem.ItemInfo;
            ItemTileInfo targetInfo = targetItem.ItemInfo;

            if (sourceInfo == null ||
                targetInfo == null)
                return false;

            FrameworkElement feSource = sourceItem.Container as FrameworkElement;
            FrameworkElement feTarget = targetItem.Container as FrameworkElement;

            if (feSource == null ||
                feTarget == null)
                return false;

            AllowTileDragging allowDragging = this._panel.GetAllowTileDragging(feSource, sourceInfo);

            if (allowDragging == AllowTileDragging.No)
                return false;

            bool isSwapping = allowDragging == AllowTileDragging.Swap;

            // if the maximized state of the source and target are different then
            // we can only do a swap
            if (isSwapping == false)
                isSwapping = sourceInfo.IsMaximized != targetInfo.IsMaximized;

            // If we are not being called from the drag manager we need to call the 
            // panel's CanSwapContainers method to allow a cancelable event to be raised.
            // The drag manager would have already called this method and we wouldn't have
            // gotten this far it it had returned false
            if (isSwapping && !calledFromDragManager)
            {
                if (!this._panel.CanSwapContainers(feSource, sourceInfo, feTarget, targetInfo))
                    return false;
            }

            if (sourceInfo.IsMaximized &&
                 targetInfo.IsMaximized)
            {
                #region Re-order items in MaximizedItems collection

                ObservableCollectionExtended<object> maximizedItems = this._owningManager.MaximizedItemsInternal;

                int oldIndex = maximizedItems.IndexOf(sourceInfo.Item);
                int newIndex = maximizedItems.IndexOf(targetInfo.Item);

                Debug.Assert(oldIndex >= 0, "old index not found in maximized item collection");
                Debug.Assert(newIndex >= 0, "new index not found in maximized item collection");

                if (oldIndex == newIndex)
                    return false;

                maximizedItems.BeginUpdate();

                try
                {
                    if (allowDragging == AllowTileDragging.Swap)
                    {
                        if (oldIndex < newIndex)
                        {
                            maximizedItems.RemoveAt(oldIndex);
                            maximizedItems.RemoveAt(newIndex - 1);
                            maximizedItems.Insert(oldIndex, targetInfo.Item);
                            maximizedItems.Insert(newIndex, sourceInfo.Item);
                        }
                        else
                        {
                            maximizedItems.RemoveAt(newIndex);
                            maximizedItems.RemoveAt(oldIndex - 1);
                            maximizedItems.Insert(newIndex, sourceInfo.Item);
                            maximizedItems.Insert(oldIndex, targetInfo.Item);
                        }
                    }
                    else
                    {
                        maximizedItems.RemoveAt(oldIndex);
                        maximizedItems.Insert(newIndex, sourceInfo.Item);
                    }
                }
                finally
                {
                    maximizedItems.EndUpdate();
                }

                #endregion //Re-order items in MaximizedItems collection

                if (isSwapping )
                    this._owningManager.OnContainersSwapped(feSource, sourceInfo, feTarget, targetInfo);

                this._panel.InvalidateMeasure();
                return true;
            }

            // we don't currently support dragging between the 
            // minimized and maximzed areas so if we get to
            // here and either item is maxmized then
            // fall thru and let the panel's implementation of
            // OnContainersSwapped handle the actual swap loig
            if (sourceInfo.IsMaximized ||
                 targetInfo.IsMaximized )
            {
                this._owningManager.OnContainersSwapped(feSource, sourceInfo, feTarget, targetInfo);

                this._panel.InvalidateMeasure();
                return true;
            }

            // in explicit mode we just swap the row/col settings
            if (this._isInMaximizedMode == false &&
                this._tileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile)
            {
                #region Process explicit layout row/column swap

                int targetColumn = XamTileManager.GetColumn(targetItem.Container);
				int targetColumnSpan = XamTileManager.GetColumnSpan(targetItem.Container);
				int targetRow = XamTileManager.GetRow(targetItem.Container);
				int targetRowSpan = XamTileManager.GetRowSpan(targetItem.Container);

				int origColumn = XamTileManager.GetColumn(sourceItem.Container);
				int origColumnSpan = XamTileManager.GetColumnSpan(sourceItem.Container);
				int origRow = XamTileManager.GetRow(sourceItem.Container);
				int origRowSpan = XamTileManager.GetRowSpan(sourceItem.Container);

				XamTileManager.SetColumn(targetItem.Container, origColumn);
				XamTileManager.SetColumnSpan(targetItem.Container, origColumnSpan);
				XamTileManager.SetRow(targetItem.Container, origRow);
				XamTileManager.SetRowSpan(targetItem.Container, origRowSpan);

				XamTileManager.SetColumn(sourceItem.Container, targetColumn);
				XamTileManager.SetColumnSpan(sourceItem.Container, targetColumnSpan);
				XamTileManager.SetRow(sourceItem.Container, targetRow);
				XamTileManager.SetRowSpan(sourceItem.Container, targetRowSpan);

                this._panel.InvalidateMeasure();

                #endregion //Process explicit layout row/column swap

                this._owningManager.OnContainersSwapped(feSource, sourceInfo, feTarget, targetInfo);
                return true;
            }

            int oldLogicalIndex = sourceInfo.LogicalIndex;

            if (oldLogicalIndex < 0)
                return false;

            int newLogicalIdex = targetInfo.LogicalIndex;

            if (newLogicalIdex < 0 || newLogicalIdex == oldLogicalIndex)
                return false;

            #region Change scroll positions in sparse array

            TileLayoutManager.ScrollManagerSparseArray spArray = this.SparseArray;

            if (allowDragging == AllowTileDragging.Swap)
            {
                if (oldLogicalIndex < newLogicalIdex)
                {
                    spArray.RemoveAt(oldLogicalIndex);
                    spArray.RemoveAt(newLogicalIdex - 1);
                    spArray.Insert(oldLogicalIndex, targetInfo);
                    spArray.Insert(newLogicalIdex, sourceInfo);
                }
                else
                {
                    spArray.RemoveAt(newLogicalIdex);
                    spArray.RemoveAt(oldLogicalIndex - 1);
                    spArray.Insert(newLogicalIdex, sourceInfo);
                    spArray.Insert(oldLogicalIndex, targetInfo);
                }
            }
            else
            {
                spArray.RemoveAt(oldLogicalIndex);
                spArray.Insert(newLogicalIdex, sourceInfo);
            }

            #endregion //Change scroll positions in sparse array

            if (isSwapping )
                this._owningManager.OnContainersSwapped(feSource, sourceInfo, feTarget, targetInfo);

            this._panel.InvalidateMeasure();

            return true;
        }

                #endregion //MoveTileHelper	

                #region OnArrange

        
        
        internal void OnArrange(out bool itemLocationsAreaTheSame, out bool inViewItemsAreTheSame )
        {

            // Snapshot the current positions 0f the items in view
            List<ItemRowColumnSizeInfo> newLocations = this.GetItemRowColumnSnapshot();

            itemLocationsAreaTheSame = true;
            
            int count = newLocations.Count;

            if (this._lastItemLocations == null )
            {
                itemLocationsAreaTheSame = true;
                
                
                inViewItemsAreTheSame = newLocations.Count == 0;
            }
            else
            if ( this._lastItemLocations.Count != count)
            {
                itemLocationsAreaTheSame    = false;
                
                
                inViewItemsAreTheSame       = false;
            }
            else
            {
                
                inViewItemsAreTheSame       = true;

                // loop over the lists stopping at the first item that doesn't match
                for (int i = 0; i < count; i++)
                {
                    ItemRowColumnSizeInfo newLocation = newLocations[i];
                    ItemRowColumnSizeInfo oldLocation = this._lastItemLocations[i];

                    
                    if (newLocation.Info != oldLocation.Info)
                    {
                        inViewItemsAreTheSame = false;
                        itemLocationsAreaTheSame = false;
                        break;
                    }

                    if (newLocation.Row != oldLocation.Row ||
                        newLocation.Column != oldLocation.Column ||
                        // JJD 2/5/10 - TFS27167
                        // Check to make sure the preferred size hasn't changed as well
                        newLocation.PreferredSize != oldLocation.PreferredSize)
                    {
                        itemLocationsAreaTheSame = false;
                        break;
                    }
                }
            }

            if ( this._lastItemLocations == null ||
                 itemLocationsAreaTheSame == false )
                this._lastItemLocations = newLocations;

            HorizontalAlignment hAlign = this._panel.GetHorizontalTileAreaAlignment();
            VerticalAlignment vAlign = this._panel.GetVerticalTileAreaAlignment();

            if (hAlign != this._lastHorizontalTileAreaAlignment ||
                 vAlign != this._lastVerticalTileAreaAlignment)
            {
                itemLocationsAreaTheSame = false;
                this._lastHorizontalTileAreaAlignment = hAlign;
                this._lastVerticalTileAreaAlignment = vAlign;
            }

        }

                #endregion //OnArrange	

				#region OnItemAdded

		private void OnItemAdded(object item)
		{
			XamTile tile = item as XamTile;

			if (tile != null)
				tile.Initialize(_owningManager);
		}

				#endregion //OnItemAdded	
     
				#region OnItemRemoved

		private void OnItemRemoved(object item)
		{
		}

				#endregion //OnItemRemoved	
       
                #region OnItemsChanged

        internal void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
			if (_panel != null)
			{
				_panel.InvalidateMeasure();
				_panel.InvalidateArrange();
			}

            if ( this._itemsInOriginalOrder == null ) 
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    #region Process Add

                    {
                        int startIndex	= e.NewStartingIndex;
                        int count		= e.NewItems.Count;

                        for (int i = 0; i < count; i++)
                        {
                            object item = e.NewItems[i];

							int insertAtIndex = startIndex + i;
                            this._itemsInOriginalOrder.Insert(insertAtIndex, item);

							ItemTileInfo itemInfo = this.GetItemInfo(item);
							Debug.Assert(itemInfo != null);

							this._sparseArray.Insert(insertAtIndex, itemInfo);

							this.OnItemAdded(item);
                        }
                    }

                    #endregion //Process Add
                    break;

                case NotifyCollectionChangedAction.Move:
                    #region Process Move
                    {
                        IList list;

                        if (e.NewItems != null)
                            list = e.NewItems;
                        else
                            list = e.OldItems;

                        int count = list.Count;

                        // If we are out of sync for some reason then process the
                        // notification as a reset.
                        // 
                        if (e.NewStartingIndex + count >= this._itemsInOriginalOrder.Count ||
                            e.OldStartingIndex + count >= this._itemsInOriginalOrder.Count)
                        {
                            this.ProcessResetNotification();
                            return;
                        }

                        // allocate an array for each item moved
                        object[] slotArray = new object[count];

                        // loop over the old index slots and cache any ContainerReferences into the
                        // slotArray allocated above
                        for (int i = 0; i < slotArray.Length; i++)
                        {
                            slotArray[i] = this._itemsInOriginalOrder[i + e.OldStartingIndex];
                        }

                        // remove the old range from the index map
                        this._itemsInOriginalOrder.RemoveRange(e.OldStartingIndex, list.Count);

                        // insert the cached removed slots into the new index
                        this._itemsInOriginalOrder.InsertRange(e.NewStartingIndex, slotArray);

                        if (this._sparseArray[Math.Min(e.NewStartingIndex, e.OldStartingIndex)] != null)
                        {
                            // we have to blow away the contents of the sparsearray at this point because
                            // we wouldn't know how to move the items in the _sparseArray array assuming
                            // that they may have been re-arranged by the user
                            this._sparseArray.Clear();
                            this._sparseArray.Expand(this._itemsInOriginalOrder.Count);
                        }
                    }

                    #endregion //Process Move
                    break;

				case NotifyCollectionChangedAction.Replace:
					#region Process Replace
					{
						this._owningManager.VerifyMaximizedItems();

						int count = e.NewItems.Count;

						// calc the end index
						int endIndex = e.NewStartingIndex + count - 1;

						// loop over the replaced items replace the corresponding item info
						// in the sparseArray array
						
						
						for (int i = 0; i < count; i++)
						{
							object newItem = e.NewItems[i];
							object oldItem = e.OldItems[i];

							ItemTileInfo oldItemInfo = this.GetItemInfo(oldItem);

							if (oldItemInfo != null)
							{
								int index = this._sparseArray.IndexOf(oldItemInfo);

								if (index >= 0)
									this._sparseArray[index] = this.GetItemInfo(newItem);
							}
						}

						// remove the old items
						this.RemoveRangeHelper(e.NewStartingIndex, e.OldItems);

						int insertIndex = e.NewStartingIndex;

						foreach (object item in e.NewItems)
						{
							_itemsInOriginalOrder.Insert(insertIndex++, item);
							this.OnItemAdded(item);
						}
					}

					#endregion //Process Replace
					break;
                case NotifyCollectionChangedAction.Remove:
                    #region Process Remove
                    {
                        this._owningManager.VerifyMaximizedItems();

                        int startIndex;
                        IList list;

                        if (e.NewItems != null)
                        {
                            startIndex = e.NewStartingIndex;
                            list = e.NewItems;
                        }
                        else
                        {
                            startIndex = e.OldStartingIndex;
                            list = e.OldItems;
                        }

                        int count = list.Count;
                        int endIndex = startIndex + count - 1;

                        // If we are out of sync for some reason then process the
                        // notification as a reset.
                        // 
                        if (endIndex >= this._itemsInOriginalOrder.Count)
                        {
                            this.ProcessResetNotification();
                            return;
                        }

                        // remove the range from the index map
                        RemoveRangeHelper(startIndex, list);

                    }

                    #endregion //Process Remove
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.ProcessResetNotification();
                    break;
            }

			AutomationPeer peer = FrameworkElementAutomationPeer.FromElement(this._owningManager);

			if (peer != null)
				AutomationPeerHelper.InvalidateChildren(peer);

        }

                #endregion //OnItemsChanged

                #region OnScrollOffsetChanged

        internal void OnScrollOffsetChanged(Vector newOffset, Vector oldOffset)
        {
            int oldScrollPosition = this._scrollPosition;

            if (this._scrollTilesHorizontally == false &&
                 this._scrollTilesVertically == false)
            {
                this._scrollLogicalOffset = 0;
                this._arrangeOffset.X = -newOffset.X;
                this._arrangeOffset.Y = -newOffset.Y;
            }
            else
            {
                if (this._scrollTilesHorizontally)
                {
                    this._scrollPosition = this.GetScrollPositionFromOffset((int)newOffset.X);
                    this._scrollLogicalOffset = (int)newOffset.X;

					// JJD 5/11/11 - TFS75082
					// Since we are scrolling tiles horizontally we should zero out the 
					// arrange offset in the X dimension
					this._arrangeOffset.X = 0;
                }
                else
                    this._arrangeOffset.X = -newOffset.X;

                if (this._scrollTilesVertically)
                {
                    this._scrollPosition = this.GetScrollPositionFromOffset((int)newOffset.Y);
                    this._scrollLogicalOffset = (int)newOffset.Y;

					// JJD 5/11/11 - TFS75082
					// Since we are scrolling tiles vertically we should zero out the 
					// arrange offset in the Y dimension
					this._arrangeOffset.Y = 0;
                }
                else
                    this._arrangeOffset.Y = -newOffset.Y;
            }

            this._arrangeOffsetForMaxmizedTiles = this._arrangeOffset;

            if ( this._isInMaximizedMode )
            {
                switch (this._maximizedTileLocation)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Right:
                        this._arrangeOffsetForMaxmizedTiles.Y = 0;
                        break;
                    default:
                        this._arrangeOffsetForMaxmizedTiles.X = 0;
                        break;
                }
            }

            if (oldScrollPosition != this._scrollPosition)
            {
                this._panel.InvalidateMeasure();
            }
        }

                #endregion //OnScrollOffsetChanged	

                #region OnItemResized

        internal void OnItemResized(ItemTileInfo itemInfo, double deltaX, double deltaY)
        {
            if (CoreUtilities.AreClose(deltaX, 0))
                deltaX = 0;

            if (CoreUtilities.AreClose(deltaY, 0))
                deltaY = 0;

            bool resizeWidthFirst = false;

            if (this._isInMaximizedMode)
            {
                switch (this._maximizedTileLayoutOrder)
                {
                    case MaximizedTileLayoutOrder.Horizontal:
                    case MaximizedTileLayoutOrder.HorizontalWithLastTileFill:
                        resizeWidthFirst = true;
                        break;
                }
            }
            else
            {
                switch (this._tileLayoutOrder)
                {
                    case TileLayoutOrder.Horizontal:
                    case TileLayoutOrder.HorizontalVariable:
                    case TileLayoutOrder.UseExplicitRowColumnOnTile:
                        resizeWidthFirst = true;
                        break;
                }
            }

            // if we are resizing the width first then process it now
            if (deltaX != 0 && resizeWidthFirst)
            {
                if (this.ItemResizeHelper(itemInfo, deltaX, 0))
                {
                    this._panel.InvalidateMeasure();

                    if (deltaY != 0)
                        this._panel.UpdateLayout();
                }

                // clear deltaX so we don't do it again below
                deltaX = 0;
            }

            // process the height change
            if (deltaY != 0)
            {
                if (this.ItemResizeHelper(itemInfo, 0, deltaY))
                {
                    this._panel.InvalidateMeasure();

                    if (deltaX != 0)
                        this._panel.UpdateLayout();
                }
            }

            // process the width change if we haven't done it above
            if (deltaX != 0)
            {
                if (this.ItemResizeHelper(itemInfo, deltaX, 0))
                {
                    this._panel.InvalidateMeasure();
                }
            }
        }

                #endregion //OnItemResized	

                #region RemoveContainer

        internal void RemoveContainer(XamTile container)
        {
            if (container != null &&
                 this._containerToItemMap.ContainsKey(container))
            {
                object item = this._containerToItemMap[container];

                this._containerToItemMap.Remove(container);

                if (this._layoutItemMap.ContainsKey(item))
                {
                    LayoutItem li = this._layoutItemMap[item];

                    this.RemoveLayoutItem(li, false);
                }
            }
        }

                #endregion //RemoveContainer	

                #region RemoveLayoutItem

        internal void RemoveLayoutItem(LayoutItem li, bool dispose)
        {
            XamTile tile = li.Container;
            object item = li.Item;

			ItemTileInfo info = li.ItemInfo;

			if (info != null && info.Index >= 0)
			{

				if (tile.IsFocused ||
					tile.IsKeyboardFocusWithin ||
					tile.IsMouseCaptureWithin)
					return;


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


				if (info.IsMaximized)
					return;
			}

            if (tile != null &&
                 this._containerToItemMap.ContainsKey(tile))
                this._containerToItemMap.Remove(tile);

			li.RecyclingRelease(dispose);

            if (this._itemsToBeArranged.Contains(li))
            {
                this._itemsToBeArranged.Remove(li);
            }
            else
            {
                
                // Since an entry didn't exist keyed by the li see
                // if there is any entry in the cache that represents the
                // tile. If so then remove it
                if (tile != null)
                {
                    foreach (LayoutItem liInCache in this._itemsToBeArranged)
                    {
                        if (liInCache.Container == tile)
                        {
                            this._itemsToBeArranged.Remove(liInCache);
                            break;
                        }
                    }
                }
            }

            if (item != null &&
                 this._layoutItemMap.ContainsKey(item))
                this._layoutItemMap.Remove(item);
        }

                #endregion //RemoveLayoutItem	

                #region SnapshotItemRects

        internal void SnapshotItemRects()
        {
            foreach (LayoutItem li in this._itemsToBeArranged)
                li.SnapshotCurrentRect();
        }

                #endregion //SnapshotItemRects	

                #region VerifyArrangeSize

        internal void VerifyArrangeSize()
        {
            bool isScrollingVertically;

            GridBagLayoutManager gbl = GetOverallGridBagLayout(out isScrollingVertically);

            
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


            bool requiresAnotherPass = this.UpdateScrollingInfo(gbl, this._lastMeasureDesiredSize, isScrollingVertically, true);

            if (requiresAnotherPass)
            {
                this._panel.InvalidateMeasure();
                this._panel.InvalidateArrange();
            }
        }

                #endregion //VerifyArrangeSize	
    
            #endregion //Internal Methods

            #region Private Methods

                
                #region AddItemToBeArranged

        private void AddItemToBeArranged(LayoutItem li)
        {
            
            // Make sure there are no duplcates
            this.RemoveDuplicateContainerRefs(li);

            this._itemsToBeArranged.Add(li);

			// JJD 03/14/12 - TFS100150 - Added touch support
			// Flag the layoutItem that we want to arrange it in view
			li.ShouldArrangeInView = true;
        }

                #endregion //AddItemToBeArranged	
    
                #region CalculateTileScrollExtents

        private bool CalculateTileScrollExtents(bool checkIfScrolledTooFar, ref Size extent, ref Size viewport)
        {
            int smallChange = 1;
            int largeChange = 1;
            int maxOffset = 0;
            int extentValue = 0;
            int viewportValue = 1;
            Size tileAreaSize = this._panel.TileAreaSize;
            double panelExtent = this._scrollTilesVertically ? tileAreaSize.Height : tileAreaSize.Width;

            bool requiresAnotherPass = false;

            if (this._isInMaximizedMode)
            {
                #region Calculate scroll extents for minimized tiles

                GridBagLayoutManager gbm = this.MinimizedTileArea.GridBagManager;

                extentValue = this._sparseArray.ScrollCount;
                viewportValue = Math.Max(1, gbm.LayoutItems.Count);
                smallChange = 1;
                largeChange = viewportValue;

                if (gbm.LayoutItems.Count < 1)
                {
					// JJD 07/13/12 - TFS107785
					// If there is a extent > 0 that means there is a scroll count. In that
					// case we want to set the MaxOffset so at least one tile will e in view.
					// In this case we need to requiresAnotherPass flag to true so that, in effect,
					// we scroll up.
                    //maxOffset = 0;
					if (extentValue == 0)
						maxOffset = 0;
					else
					{
						maxOffset = Math.Max(extentValue - viewportValue, 0);
						requiresAnotherPass = true;
					}
				}
                else
                {
                    LayoutItem liLast = (LayoutItem)gbm.LayoutItems[gbm.LayoutItems.Count - 1];

                    this._scrollPositionOfLastArrangedItem      = liLast.ItemInfo.ScrollPosition;
                    this._scrollLogicalOffsetOfLastArrangedItem = this._scrollPositionOfLastArrangedItem;

                    maxOffset = Math.Max(extentValue - viewportValue, 0);

                    if (extentValue > 0 && maxOffset > 0 && gbm.LayoutItems.Count > 1 && this._scrollLogicalOffset > 0)
                    {
                        LayoutItem liFirst = gbm.LayoutItems[0] as LayoutItem;
                        requiresAnotherPass = AdjustMaxOffset(ref maxOffset, ref extentValue, ref viewportValue, panelExtent, gbm, liFirst, liLast, !checkIfScrolledTooFar);
                    }

                    if ( requiresAnotherPass == false)
                    {
                        #region If the last minimized tile is fully in view adjust the viewport so the scroll thumb is at the bottom

                        
                        
                        
                        
                        if (this._scrollPositionOfLastArrangedItem >= extentValue - 1 &&
                            viewportValue > extentValue + 1)
                        {
                            bool isLastTileFullyVisible = false;

                            switch (this._maximizedTileLocation)
                            {
                                default:
                                case MaximizedTileLocation.Left:
                                case MaximizedTileLocation.Right:
                                    isLastTileFullyVisible = (liLast.TargetRect.Bottom <= this._panel.TileAreaSize.Height + 1);
                                    break;
                                case MaximizedTileLocation.Top:
                                case MaximizedTileLocation.Bottom:
                                    isLastTileFullyVisible = (liLast.TargetRect.Right <= this._panel.TileAreaSize.Width + 1);
                                    break;
                            }

                            if (isLastTileFullyVisible)
                            {
                                viewportValue++;
                                largeChange = viewportValue;
                            }
                        }

                        #endregion //If the last minimized tile is fully in view adjust the viewport so the scroll thumb is at the bottom
                    }
                }

                #endregion //Calculate scroll extents for minimized tiles
            }
            else
            {
                switch (this._tileLayoutOrder)
                {
                    case TileLayoutOrder.Vertical:
                    case TileLayoutOrder.Horizontal:
                        #region Process Vertical and Horizontal non-variable tile layouts

                        {
                            extentValue = Math.Max((this._sparseArray.ScrollCount + this._normalModeItemsPerRowColumn - 1) / Math.Max(this._normalModeItemsPerRowColumn, 1), 0);

                            viewportValue = Math.Max(1, this._normalModeRowColumnsInView);

                            
                            
                            
                            
                            
                            
                            
                            
                            if ( viewportValue < extentValue  || this._scrollPosition > 0 ||
                                this._normalModeRowColumnsInViewActual > this._normalModeRowColumnsInView)
                                viewportValue = Math.Min(2, Math.Max(1, this._normalModeRowColumnsInView - 1));

                            smallChange = 1;
                            largeChange = this._normalModeRowColumnsInView;

                            GridBagLayoutManager gbm = this.GridBagLayoutNormal;

                            int layoutItemsCount = gbm.LayoutItems.Count;

                            if (layoutItemsCount == 0)
                            {
                                this._scrollPositionOfLastArrangedItem = 0;
                             }
                            else
                            {
                                LayoutItem liLast = (LayoutItem)gbm.LayoutItems[layoutItemsCount - 1];

                                this._scrollPositionOfLastArrangedItem = liLast.ItemInfo.ScrollPosition;
                            }

                            this._scrollLogicalOffsetOfLastArrangedItem = this._scrollPositionOfLastArrangedItem;

                            if (this._normalModeRowColumnsInView < 1)
                                maxOffset = 0;
                            else
                            {
                                maxOffset = Math.Max(extentValue - viewportValue, 0);

								
								
                                
                                
                                
								if (extentValue > 0 && layoutItemsCount > 1)
                                {
                                    int expectedScrollPos = this._scrollLogicalOffset * this._normalModeItemsPerRowColumn;

                                    // Make sure the scrollposition is an exact multiple of the
                                    // offset. This can be different if the _normalModeItemsPerRowColumn 
                                    // has changed since the last scroll operation.
                                    // In this case we want to sync up the scrollPosition 
                                    // and return a flag to re-gen everything
                                    if (expectedScrollPos != this._scrollPosition && _normalModeItemsPerRowColumn > 0)
                                    {
                                        
                                        // Re-calculate the logical offset and update the scroll position accordingly 
                                        
                                        if ( this._lastNormalModeItemsPerRowColumn > this._normalModeItemsPerRowColumn )
                                            this._scrollLogicalOffset = this._scrollPosition  / this._normalModeItemsPerRowColumn;
                                        else
                                            this._scrollLogicalOffset = (this._scrollPosition + 1) / this._normalModeItemsPerRowColumn;
                                        
                                        this._scrollPosition = this._scrollLogicalOffset * this._normalModeItemsPerRowColumn;

                                        requiresAnotherPass = true;
                                    }
                                    else
                                    {
                                        if (this._normalModeRowColumnsInView > 1)
                                        {
                                            LayoutItem liFirst = gbm.LayoutItems[0] as LayoutItem;
                                            LayoutItem liLast = gbm.LayoutItems[layoutItemsCount - 1] as LayoutItem;
                                            
                                            
                                            // Do the the 'scrolled too far' check if the falg is passed to us.
                                            // Now that we are doing it in the arrange and have a flag to prevent a race
                                            // condition. 
                                            
                                            // However, if an explicit max is specified in the scrolling
                                            // dimension then we don't want to do the 'scrolled to far' check
                                            
                                            
                                            
                                            //requiresAnotherPass = AdjustMaxOffset(ref maxOffet, ref extentValue, ref viewportValue, panelExtent, gbm, liFirst, liLast, !checkIfScrolledTooFar);
                                            bool bypassScrolledTooFarCheck = !checkIfScrolledTooFar;

                                            if (bypassScrolledTooFarCheck == false && 
                                                this._normalModeRowColumnsInViewActual == this._normalModeRowColumnsInView)
                                            {
                                                int explicitMax;

                                                if (this._scrollTilesHorizontally)
                                                    explicitMax = this._owningManager.NormalModeSettingsSafe.MaxColumns;
                                                else
                                                    explicitMax =this._owningManager.NormalModeSettingsSafe.MaxRows;

                                                if (explicitMax > 0)
                                                     bypassScrolledTooFarCheck = this._normalModeRowColumnsInViewActual >= explicitMax;
                                            }
 
                                            requiresAnotherPass = AdjustMaxOffset(ref maxOffset, ref extentValue, ref viewportValue, panelExtent, gbm, liFirst, liLast, bypassScrolledTooFarCheck);
                                        }
                                    }
                                }
                            }

                        }

                        #endregion //Process Vertical and Horizontal non-variable tile layouts
                        break;
                    case TileLayoutOrder.VerticalVariable:
                    case TileLayoutOrder.HorizontalVariable:
                        #region Process Variable layouts
                        {
                            extentValue = 0;
                            viewportValue = 1;
                            smallChange = 1;
                            largeChange = 1;
                            maxOffset = 0;

                            GridBagLayoutManager gbm = this.GridBagLayoutNormalModeOverall;

                            int count = gbm.LayoutItems.Count;

                            if (count == 0)
                            {
                                if (this._scrollLogicalOffset > 0)
                                    requiresAnotherPass = true;

                                this._scrollPosition = 0;
                                this._scrollPositionOfLastArrangedItem = 0;
                                this._scrollLogicalOffset = 0;
                                this._scrollLogicalOffsetOfLastArrangedItem = 0;
                                this._scrollPosition = 0;
                                this._variableAvgItemsPerRowColumn = 1;
                                this._variableTotalRowColmuns = 0;
                            }
                            else
                            {
                                int fullyInViewCount;

                                
                                // Use _totalColumnsDisplayed or _totalRowsDisplayed
                                // to determine the number of fully visible rows/columns
                                if (this._tileLayoutOrder == TileLayoutOrder.VerticalVariable)
                                    fullyInViewCount = this._totalColumnsDisplayed;
                                else
                                    fullyInViewCount = this._totalRowsDisplayed;

                                TileRowColumnLayoutItem rcItemFirst = gbm.LayoutItems[0] as TileRowColumnLayoutItem;
                                TileRowColumnLayoutItem rcItemLast = gbm.LayoutItems[fullyInViewCount - 1] as TileRowColumnLayoutItem;

                                this._scrollLogicalOffset                   = rcItemFirst.ScrollOffset;
                                this._scrollPositionOfLastArrangedItem      = rcItemLast.GetLastScrollPosition();
                                this._scrollLogicalOffsetOfLastArrangedItem = rcItemLast.ScrollOffset;

                                // calculate an the avg number of tiles per row/col
                                this._variableAvgItemsPerRowColumn = Math.Max((double)(this._scrollPositionOfLastArrangedItem + 1 ) / (double)(Math.Max(this._scrollLogicalOffsetOfLastArrangedItem + 1, 1)), 1);

                                int scrollCount = this._sparseArray.ScrollCount;

                                if (this._scrollPositionOfLastArrangedItem >= scrollCount - 1)
                                {
                                    this._variableTotalRowColmuns = this._scrollLogicalOffsetOfLastArrangedItem + 1;

                                    // since we are showing the last item check if the
                                    // first item is shown. If so make sure there is no scrollbar
                                    if (this._scrollLogicalOffset == 0)
                                    {
                                        extentValue = 1;
                                        viewportValue = 1;
                                    }
                                    else
                                    {
                                        extentValue = this._variableTotalRowColmuns;
                                        viewportValue = Math.Max(fullyInViewCount, 1);
                                    }
                                }
                                else
                                {
                                    
                                    
                                    
                                    this._variableTotalRowColmuns = Math.Max((int)(Math.Ceiling((double)scrollCount / this._variableAvgItemsPerRowColumn)), this._scrollLogicalOffsetOfLastArrangedItem + 1);
                                    extentValue = this._variableTotalRowColmuns;
                                    viewportValue = Math.Max(fullyInViewCount, 1);
                                }


                                largeChange = Math.Max(fullyInViewCount, 1);

                                maxOffset = Math.Max(extentValue - viewportValue, 0);
                            }

                        }

                        #endregion //Process Variable layouts
                        break;
                }
            }

            if (this._scrollTilesHorizontally)
            {
                this._scrollSmallChange.X   = smallChange;
                this._scrollLargeChange.X   = largeChange;
                this._scrollMaxOffset.X     = maxOffset;
                extent.Width                = extentValue;
                viewport.Width              = viewportValue;
            }
            else
            {
                this._scrollSmallChange.Y   = smallChange;
                this._scrollLargeChange.Y   = largeChange;
                this._scrollMaxOffset.Y     = maxOffset;
                extent.Height               = extentValue;
                viewport.Height             = viewportValue;
            }

            return requiresAnotherPass;
        }

        private bool AdjustMaxOffset(ref int maxOffset, ref int extentValue, ref int viewportValue, double panelExtent, GridBagLayoutManager gbm, LayoutItem liFirst, LayoutItem liLast, bool bypassScrolledTooFarCheck)
        {
            double rangeFrom = this._scrollTilesVertically ? liFirst.TargetRect.Top : liFirst.TargetRect.Left;
            double rangeTo = this._scrollTilesVertically ? liLast.TargetRect.Bottom : liLast.TargetRect.Right;
            double lastElementExtent = this._scrollTilesVertically ? liLast.TargetRect.Height : liLast.TargetRect.Width;

            double extraSpace = rangeFrom + panelExtent - rangeTo;
            
            double interItemSpacing;
            if (this._scrollTilesHorizontally)
                interItemSpacing = gbm.InterItemSpacingHorizontal;
            else
                interItemSpacing = gbm.InterItemSpacingHorizontal;

            // JJD 2/22/10 - TFS27951
            // Disable scrollToFar logic
            
            // check if we have enough space for another element
            //bool scrolledTooFar = bypassScrolledTooFarCheck == false &&
            //                        this._scrolledTooFarAdjustmentMade == false &&
            //                        extraSpace >= lastElementExtent + interItemSpacing;
            bool scrolledTooFar = false;

            bool requiresAnotherPass = false;

            // if we have scrolled too then set the maxoffset to the current offset 
            // minus 1 and return a flag that will let the MeasureOverride code
            // inside the panel know to re-layout the elements
            if (scrolledTooFar)
            {
                if (this._scrollLogicalOffset == 0)
                {
                    extentValue = 0;
                    viewportValue = 1;
                    maxOffset = 0;
                }
                else
                {
                    maxOffset = this._scrollLogicalOffset - 1;

                    requiresAnotherPass = true;
                }
            }
            else
            {
                // Since we didn't scroll too far check if we are at the maxoffset.
                // if so make sure the last element is fully visible by checking for
                // negative extraSpace
                if (extraSpace < 0 && bypassScrolledTooFarCheck == false &&
                    this._scrollLogicalOffset >= maxOffset)
                {
                    int oldMaxOffset = maxOffset;

                    // if this is the case we need to adjust the maxOffset so we
                    // can scroll further to allow the last item to be scrolled into view
                    // but we can't go too far (past the scorll count)
                    maxOffset = Math.Min( this.SparseArray.ScrollCount - 1, this._scrollLogicalOffset + 1 );

                    int delta = maxOffset - oldMaxOffset;

                    viewportValue -= delta;
                    if (viewportValue < 1)
                    {
                        extentValue += 1 - viewportValue;
                        viewportValue = 1;
                    }
                }
                else
                {
                    maxOffset = Math.Max(this._scrollLogicalOffset, maxOffset);
                }
            }

            return requiresAnotherPass;
        }

                #endregion //CalculateTileScrollExtents	
    
                #region CalculateAutoLayoutHelper

        private GridBagLayoutItemDimensionsCollection CalculateAutoLayoutHelper(GridBagLayoutManager gbl,
            TileAreaPanel.GenerationCache genCache,
            IEnumerator<ILayoutItem> enumerator,
            bool autoFitAllItems,
            Size constraint, 
            int minRows, int minCols, int maxRows, int maxCols, 
            Orientation orientation)
        {
            Orientation? scrollbar = null;

            
            // For maximized tiles we only have the use of a single scrollbar so pass
            // in the appropriate scrollbar orientation into CalculateAutoLayout
            // 
            if (this._gridBagLayoutMaximized == gbl)
            {
                switch (this._maximizedTileLocation)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Right:
                        scrollbar = Orientation.Horizontal;
                        break;
                    case MaximizedTileLocation.Top:
                    case MaximizedTileLocation.Bottom:
                        scrollbar = Orientation.Vertical;
                        break;
                }
            }

            GridBagLayoutItemDimensionsCollection dimsCollection = gbl.CalculateAutoLayout(
                                    enumerator,
                                    orientation,
                                    autoFitAllItems,
                                    constraint,
                                    minRows, minCols, maxRows, maxCols, 
									scrollbar );

            this.ProcessDimensionsCollection(gbl, genCache, dimsCollection);

            return dimsCollection;
        }

                #endregion //CalculateAutoLayoutHelper	

                #region CreateExpectedItemsList

        // JJD 3/18/10 - TFS28705 - Optimization
        // Create a list to help optimize recycling
        private void CreateExpectedItemsList(TileAreaPanel.GenerationCache genCache)
        {
            if (this._itemsToBeArranged == null ||
                this._itemsToBeArranged.Count < 1 )
                return;

            bool arrangeAllItems = false;

            if (this._isInMaximizedMode)
                arrangeAllItems = this._owningManager.MaximizedModeSettingsSafe.ShowAllMinimizedTiles;
            else
                arrangeAllItems = this._tileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile || this._owningManager.NormalModeSettingsSafe.ShowAllTiles;

            // if we are arranging all items then initialize _itemsExpectedToBeGenerated with every item
            if (arrangeAllItems)
            {
                genCache._itemsExpectedToBeGenerated = new HashSet<object>();

				foreach (object item in this.Items)
					genCache._itemsExpectedToBeGenerated.Add(item);

                return;
            }

            IList maximizedItems = null;
            
            if (this._isInMaximizedMode)
                maximizedItems = this._owningManager.MaximizedItemsInternal;

            // if we are animating then use the items that were arranged the last time since they
            // are still needed until they are animated out of view
            if (this._owningManager.ShouldAnimate)
            {
                genCache._itemsExpectedToBeGenerated = new HashSet<object>();

				if (maximizedItems != null)
				{
					foreach (object item in maximizedItems)
						genCache._itemsExpectedToBeGenerated.Add(item);
				}

                // add the old items into the list
                foreach (LayoutItem li in this._itemsToBeArranged)
                {
                    DependencyObject container = li.Container;

                    if (container != null)
                    {
                        object item = li.Item;

                        if (item != null)
                            genCache._itemsExpectedToBeGenerated.Add(item);
                    }
                }

                return;
            }

            int scrollingViewport = this.IsMaximizedAreaLeftOrRight 
                            ? (int)this._panel.ScrollDataInfo._viewport.Height
                            : (int)this._panel.ScrollDataInfo._viewport.Width;


            int scrollableRcdsToInclude;

            if (this._isInMaximizedMode)
            {
                scrollableRcdsToInclude = Math.Max(1, scrollingViewport);
            }
            else
            {
                int itemsPerRowCol;
                switch (this._tileLayoutOrder)
                {
                    case TileLayoutOrder.HorizontalVariable:
                    case TileLayoutOrder.VerticalVariable:
                        itemsPerRowCol = (int)Math.Ceiling(this._variableAvgItemsPerRowColumn);
                        break;
                    default:
                        itemsPerRowCol = this._normalModeItemsPerRowColumn;
                        break;

                }
                scrollableRcdsToInclude = 1 + (scrollingViewport * itemsPerRowCol);
            }

            //genCache._itemsExpectedToBeGenerated = new ArrayList(scrollableRcdsToInclude + (maximizedItems != null ? maximizedItems.Count : 0));
            genCache._itemsExpectedToBeGenerated = new HashSet<object>();

			if (maximizedItems != null)
			{
				foreach (object item in this.Items)
					genCache._itemsExpectedToBeGenerated.Add(item);
			}


            // add scrollable items
            for (int i = 0; i < scrollableRcdsToInclude; i++)
            {
                int scrollIndex = this._scrollPosition + i;

                if (scrollIndex >= this._sparseArray.ScrollCount)
                    break;

                object item = this.GetItemAtScrollIndex(scrollIndex);

                if ( item != null )
                    genCache._itemsExpectedToBeGenerated.Add(item);
            }


        }

                #endregion //CreateExpectedItemsList

                #region EnsureItemArrayIsAllocated

        private void EnsureItemArrayIsAllocated()
        {
            if (this._itemsInOriginalOrder == null)
            {
                int count = this.Items.Count;
                
                this._itemsInOriginalOrder = new List<object>(count + 10);

                this.ExpandCachedItemArrays(count);
            }

            Debug.Assert(this._itemsInOriginalOrder.Count == this.Items.Count, "ItemsInOriginalOrder count out of sync");
            Debug.Assert(this._sparseArray.Count == this.Items.Count, "_sparseArray count out of sync");
        }

                #endregion //EnsureItemArrayIsAllocated	

                #region ExpandCachedItemArrays

        private void ExpandCachedItemArrays(int newCount)
        {
            if (this._itemsInOriginalOrder == null)
                return;

            int count = this._itemsInOriginalOrder.Count;

            // add empty slots to the _itemsInOriginalOrder cache
            while (count < newCount)
            {
                this._itemsInOriginalOrder.Add(null);
                count++;
            }

            this._sparseArray.Expand(newCount);
        }

                #endregion //ExpandCachedItemArrays	

                #region GenerateMaximizedElements

        private void GenerateMaximizedElements(TileAreaPanel.GenerationCache genCache)
        {
            GridBagLayoutManager gbl = this.GridBagLayoutMaximized;

            gbl.InterItemSpacingHorizontal = this._panel.GetInterTileSpacing(false, TileState.Maximized);
            gbl.InterItemSpacingVertical = this._panel.GetInterTileSpacing(true, TileState.Maximized);

            bool fillAvailabelRows      = false;
            bool fillAvailableColumns   = false;

            int maxRows = 0;
            int maxCols = 0;
            Orientation orientation = Orientation.Horizontal;

            switch (this._maximizedTileLayoutOrder)
            {
                case MaximizedTileLayoutOrder.Horizontal:
                    break;
                case MaximizedTileLayoutOrder.HorizontalWithLastTileFill:
                    fillAvailableColumns = true;
                    break;
                case MaximizedTileLayoutOrder.Vertical:
                    orientation = Orientation.Vertical;
                    break;
                case MaximizedTileLayoutOrder.VerticalWithLastTileFill:
                    fillAvailabelRows = true;
                    orientation = Orientation.Vertical;
                    break;
                case MaximizedTileLayoutOrder.SingleColumn:
                    maxCols = 1;
                    orientation = Orientation.Vertical;
                    break;
                case MaximizedTileLayoutOrder.SingleRow:
                    maxRows = 1;
                    break;
            }

            // call auto layout to generate all the items that will fir
            GridBagLayoutItemDimensionsCollection dims = CalculateAutoLayoutHelper(gbl, genCache,
                new LayoutItemGeneratorEnumerator(this, genCache, true, 0, orientation, ScrollDirection.Increment),
                true,
                this._panel.TileAreaSize, 
                1, 1, maxRows, maxCols, orientation);

            this._maximizedTileArea.InitializeCachedSizes(dims);

            this._totalMaximizedColumns = dims.ColumnDims.Length - 1;
            this._totalMaximizedRows    = dims.RowDims.Length - 1;

            this._totalColumnsDisplayed = this._totalMaximizedColumns;
            this._totalRowsDisplayed    = this._totalMaximizedRows;

            int count = gbl.LayoutItems.Count;

            // if we area filling the available columns or rows set the
            // appropriate flag on the last item in the collection
            if (count > 0)
            {
                if (fillAvailableColumns == true)
                    ((LayoutItem)(gbl.LayoutItems[count - 1])).SetFillAvailableColumns(true);
                else if (fillAvailabelRows == true)
                    ((LayoutItem)(gbl.LayoutItems[count - 1])).SetFillAvailableRows(true);
            }


        }
                
                #endregion //GenerateMaximizedElements	

                #region GenerateMinimizedElements

        private void GenerateMinimizedElements(TileAreaPanel.GenerationCache genCache)
        {
            GridBagLayoutManager gbl = this.GridBagLayoutMinimized;

            gbl.InterItemSpacingHorizontal = this._panel.GetInterTileSpacing(false, TileState.Minimized);
            gbl.InterItemSpacingVertical = this._panel.GetInterTileSpacing(true, TileState.Minimized);

            int maxRows;
            int maxCols;
            Orientation orientation;

            bool showAllTiles = this._owningManager.MaximizedModeSettingsSafe.ShowAllMinimizedTiles;

            if (showAllTiles != this._lastShowAllTiles)
            {
                this._lastShowAllTiles = showAllTiles;

                //if we are now showing all tiles then reset the scroll position back to 0 
                if (showAllTiles)
                {
                    this._scrollPosition = 0;
                    this._scrollLogicalOffset = 0;
                }
            }
            
            gbl.ShrinkToFitHeight = null;
            gbl.ShrinkToFitWidth = null;

            switch (this._maximizedTileLocation)
            {
                case MaximizedTileLocation.Left:
                case MaximizedTileLocation.Right:
                    maxRows = 0;
                    maxCols = 1;
                    orientation = Orientation.Horizontal;
                    if (showAllTiles)
                        gbl.ShrinkToFitHeight = true;

                    gbl.ShrinkToFitWidth = true;
                    break;
                default:
                case MaximizedTileLocation.Bottom:
                case MaximizedTileLocation.Top:
                    maxRows = 1;
                    maxCols = 0;
                    orientation = Orientation.Vertical;
                    if (showAllTiles)
                        gbl.ShrinkToFitWidth = true;
                    
                    gbl.ShrinkToFitHeight = true;
                    break;
            }

            gbl.HorizontalContentAlignment  = this._panel.GetHorizontalTileAreaAlignment();
            gbl.VerticalContentAlignment    = this._panel.GetVerticalTileAreaAlignment();
            gbl.ExpandToFitWidth            = gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch;
            gbl.ExpandToFitHeight           = gbl.VerticalContentAlignment == VerticalAlignment.Stretch;

            // call auto layout to generate all the items that will fir
            GridBagLayoutItemDimensionsCollection dims = CalculateAutoLayoutHelper(gbl, genCache,
                                        new LayoutItemGeneratorEnumerator(this, genCache, false, this._scrollPosition, orientation, ScrollDirection.Increment),
                                        showAllTiles,
                                        this._panel.TileAreaSize, 
                                        1, 1, maxRows, maxCols, orientation);

            this._minimizedTileArea.InitializeCachedSizes(dims);


            switch (this._maximizedTileLocation)
            {
                case MaximizedTileLocation.Left:
                case MaximizedTileLocation.Right:
                    this._totalColumnsDisplayed++;
                    this._totalRowsDisplayed = Math.Max(dims.RowDims.Length - 1, this._totalMaximizedRows );
                    break;
                default:
                case MaximizedTileLocation.Bottom:
                case MaximizedTileLocation.Top:
                    this._totalRowsDisplayed++;
                    this._totalColumnsDisplayed = Math.Max(dims.ColumnDims.Length - 1, this._totalMaximizedColumns );
                    break;
            }
        }
                
                #endregion //GenerateMinimizedElements	

                #region GenerateNormalElements

        private void GenerateNormalElements(TileAreaPanel.GenerationCache genCache)
        {
            Orientation orientation;

            switch (this._tileLayoutOrder)
            {
                default:
                case TileLayoutOrder.Vertical:
                    orientation         = Orientation.Vertical;
                    break;
                case TileLayoutOrder.Horizontal:
                    orientation         = Orientation.Horizontal;
                    break;
                case TileLayoutOrder.VerticalVariable:
                    this.GenerateNormalVariableElements(genCache, false);
                    return;
                case TileLayoutOrder.HorizontalVariable:
                    this.GenerateNormalVariableElements(genCache, true);
                    return;
                case TileLayoutOrder.UseExplicitRowColumnOnTile:
                    this.GenerateNormalExplicitElements(genCache);
                    return;
            }

            
            // Keep track of the previous items per row/column
            this._lastNormalModeItemsPerRowColumn = this._normalModeItemsPerRowColumn;

            GridBagLayoutManager gbl = this.GridBagLayoutNormal;

            gbl.InterItemSpacingHorizontal  = this._panel.GetInterTileSpacing(false, TileState.Normal);
            gbl.InterItemSpacingVertical = this._panel.GetInterTileSpacing(true, TileState.Normal);

            GridBagLayoutItemDimensionsCollection dimsFromFirstItem = null;

            bool showAllTiles = this._owningManager.NormalModeSettingsSafe.ShowAllTiles;

            if (showAllTiles != this._lastShowAllTiles)
            {
                this._lastShowAllTiles = showAllTiles;

                //if we are now showing all tiles then reset the scroll position back to 0 
                if (showAllTiles)
                {
                    this._scrollPosition = 0;
                    this._scrollLogicalOffset = 0;
                }
            }

            if (showAllTiles)
            {
                gbl.ShrinkToFitHeight = true;
                gbl.ShrinkToFitWidth = true;
            }
            else
            {
                gbl.ShrinkToFitHeight = null;
                gbl.ShrinkToFitWidth = null;

                
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

            }

            gbl.HorizontalContentAlignment  = this._panel.GetHorizontalTileAreaAlignment();
            gbl.VerticalContentAlignment    = this._panel.GetVerticalTileAreaAlignment();
            gbl.ExpandToFitWidth            = gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch;
            gbl.ExpandToFitHeight           = gbl.VerticalContentAlignment == VerticalAlignment.Stretch;

			// SSP 4/11/2011 TFS32232
			// 
			gbl.LayoutMode = GridBagLayoutManager.GridBagLayoutMode.Standard;

			NormalModeSettings settings = _owningManager.NormalModeSettingsSafe;

            // call auto layout to generate all the items that will fit
            GridBagLayoutItemDimensionsCollection dims = CalculateAutoLayoutHelper(gbl, genCache,
                                        new LayoutItemGeneratorEnumerator(this, genCache, false, this._scrollPosition, orientation, ScrollDirection.Increment),
                                        showAllTiles,
                                        this._panel.TileAreaSize, 
                                        settings.MinRows,
                                        settings.MinColumns,
                                        settings.MaxRows,
                                        settings.MaxColumns,
                                        orientation);

            
            // Keep track of the actual number of row/cols generated on this pass
            if (orientation == Orientation.Vertical)
            {
                this._normalModeRowColumnsInViewActual = dims.ColumnDims.Length - 1;
                this._normalModeItemsPerRowColumnActual = dims.RowDims.Length - 1;
            }
            else
            {
                this._normalModeRowColumnsInViewActual = dims.RowDims.Length - 1;
                this._normalModeItemsPerRowColumnActual = dims.ColumnDims.Length - 1;
            }

            if (dimsFromFirstItem == null)
                dimsFromFirstItem = dims;

            int colEntries = dimsFromFirstItem.ColumnDims.Length;
            int rowEntries = dimsFromFirstItem.RowDims.Length;

            this._totalColumnsDisplayed     = colEntries - 1;
            this._totalRowsDisplayed        = rowEntries - 1;

            int itemsPerRowColumn;
            int rowColumnsInView;

            // Calculate the number a rows and columns in view.
            // Note that the RowDims and ColumnDims arrays have a length
            // that is always 1 greater than the number of rows/cols
            if (orientation == Orientation.Vertical)
            {
                itemsPerRowColumn = this._totalRowsDisplayed;
                rowColumnsInView = this._totalColumnsDisplayed;

                if (gbl.ExpandToFitWidth == false &&
                    
                    
                    //this._normalModeRowColumnsInView > 1 &&
                    rowColumnsInView > 1 &&
                    dimsFromFirstItem.ColumnDims[colEntries - 1] > this._panel.TileAreaSize.Width)
                    rowColumnsInView--;
            }
            else
            {
                itemsPerRowColumn = this._totalColumnsDisplayed;
                rowColumnsInView = this._totalRowsDisplayed;

                if (gbl.ExpandToFitHeight == false &&
                    
                    
                    //this._normalModeRowColumnsInView > 1 &&
                    rowColumnsInView > 1 &&
                    dimsFromFirstItem.RowDims[rowEntries - 1] > this._panel.TileAreaSize.Height)
                    rowColumnsInView--;
            }

            // make sure we don't adjust the the layout numbers if we have scrolled the
            // last item into view
            if (this._scrollLogicalOffset == 0 ||
                !this._wasLastScrollableItemGened ||
                this._normalModeRowColumnsInView < 1 ||
                this._normalModeItemsPerRowColumn < 1)
            {
                this._normalModeItemsPerRowColumn   = itemsPerRowColumn;
                this._normalModeRowColumnsInView    = rowColumnsInView;

                
                // Keep track of the previous items per row/column
                if (this._lastNormalModeItemsPerRowColumn < 1)
                    this._lastNormalModeItemsPerRowColumn = this._normalModeItemsPerRowColumn;
            }
            else
            {
                if (rowColumnsInView > this._normalModeRowColumnsInView)
                {
                    this._normalModeRowColumnsInView = rowColumnsInView - 1;
                }
            }

        }
                
                #endregion //GenerateNormalElements	

                #region GenerateNormalExplicitElements

        private void GenerateNormalExplicitElements(TileAreaPanel.GenerationCache genCache)
        {
			bool useMultipleGbls = false;

			// JJD 5/9/11 - TFS74206 
			// It we are synchronizing only one dimension then we need multiple grid bag layout managers
			switch (this._explicitTileSizeBehavior)
			{
				case ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAcrossRows:
				case ExplicitLayoutTileSizeBehavior.SynchronizeTileHeightsAcrossColumns:
					useMultipleGbls = true;
					break;
			}

            GridBagLayoutManager gbl;

			double interTileSpacingX = this._panel.GetInterTileSpacing(false, TileState.Normal);
			double interTileSpacingY = this._panel.GetInterTileSpacing(true, TileState.Normal);

			// JJD 5/9/11 - TFS74206 
			// Get the appropriate outer grid bag layout manager
			if (useMultipleGbls)
				gbl = this.GridBagLayoutNormalModeOverall;
			else
				gbl = this.GridBagLayoutNormal;

			gbl.InterItemSpacingHorizontal	= interTileSpacingX;
			gbl.InterItemSpacingVertical	= interTileSpacingY;
            gbl.HorizontalContentAlignment  = this._panel.GetHorizontalTileAreaAlignment();
            gbl.VerticalContentAlignment    = this._panel.GetVerticalTileAreaAlignment();
			gbl.ExpandToFitWidth			= gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch;
			gbl.ExpandToFitHeight			= gbl.VerticalContentAlignment == VerticalAlignment.Stretch;

			// SSP 4/11/2011 TFS32232
			// 
			gbl.LayoutMode = GridBagLayoutManager.GridBagLayoutMode.Uniform;

            LayoutItemsCollection liCollection = gbl.LayoutItems;

            IList items = this.Items;
            
            int count = items.Count;
			
			// JJD 5/9/11 - TFS74206 
			List<IGridBagConstraint> tempLiList =null;
			List<ExplicitRowColumnLayoutItem> oldChildGbls = null;

			// JJD 5/9/11 - TFS74206 
			// If we are using multiple managers then allocated a temp list to hold the layout items
			// until we can determine where each one goes
			if (useMultipleGbls )
			{
				tempLiList = new List<IGridBagConstraint>(count);
				oldChildGbls = new List<ExplicitRowColumnLayoutItem>();

				if (genCache._previousGridBagLayoutNormalModeOverall != null)
				{
					LayoutItemsCollection oldLiCollection = genCache._previousGridBagLayoutNormalModeOverall.LayoutItems;

					// JJD 5/9/11 - TFS74206 
					// loop over the old collection to see if we can reuse any old child grid bag layout managers
					for (int i = 0; i < oldLiCollection.Count; i++)
					{
						ExplicitRowColumnLayoutItem childGbl = oldLiCollection[i] as ExplicitRowColumnLayoutItem;

						if (childGbl != null)
							oldChildGbls.Add(childGbl);
					}
				}
			}

            liCollection.Clear();

            // generate and populate the LayoutItem collection
            for (int i = 0; i < count; i++)
            {
                XamTile container = genCache.GenerateElement(i);
                UIElement element = container as UIElement;

                if (container == null ||
                    (element != null && element.Visibility == Visibility.Collapsed))
                    continue; 
                
                object item = this.GetItem(i, false);

                Dock enterFromSide = Dock.Right;

                LayoutItem li = this.InitializeLayoutItem(container, item, enterFromSide);

                
                
                
                this.AddItemToBeArranged(li);

                this._itemsInView.Add(li.ItemInfo);

                UpdateGenCacheOnItemAdded(genCache, li);

				// JJD 5/9/11 - TFS74206 
				// If we are using multiple grid bag managers then just add the layout item to
				// the temp list. Otherwise, add it directly to the gbl collection.
				if (useMultipleGbls)
					tempLiList.Add(li);
				else
					liCollection.Add(li, li);

				IGridBagConstraint gridBagConstraint = (IGridBagConstraint)li;

				this._totalColumnsDisplayed = Math.Max(gridBagConstraint.Column + gridBagConstraint.ColumnSpan, this._totalColumnsDisplayed);
				this._totalRowsDisplayed = Math.Max(gridBagConstraint.Row + gridBagConstraint.RowSpan, this._totalRowsDisplayed);
            }


			// JJD 5/9/11 - TFS74206 
			// If we are using multiple grid bag managers then just add the layout item to
			// the temp list and continue
			if (useMultipleGbls == false || tempLiList.Count == 0)
				return;

			if (tempLiList.Count > 1)
				tempLiList.Sort(new ExplicitLayoutItemComparer(_explicitTileSizeBehavior));

			#region Slot the items into sections

			List<ExplicitRowColumnSection> sections = new List<ExplicitRowColumnSection>();

			int sectionCount = 0;
			int highestSlotSoFar = -1;

			foreach (IGridBagConstraint item in tempLiList)
			{
				int startRowCol;
				int span;

				if (_explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileHeightsAcrossColumns)
				{
					startRowCol = item.Row;
					span = item.RowSpan;
				}
				else
				{
					startRowCol = item.Column;
					span = item.ColumnSpan;
				}

				int endRowCol = startRowCol + span - 1;

				ExplicitRowColumnSection section = null;

				// if start slot is <= the highest slot we have seen so far
				// then check the previous sections to see if this item
				// matches
				if (startRowCol <= highestSlotSoFar)
				{
					for (int i = sectionCount - 1; i >= 0; i--)
					{
						if (sections[i].Intersects(startRowCol, endRowCol))
						{
							section = sections[i];
							section.Add(item, startRowCol, endRowCol);
							break;
						}
					}
				}

				if (section == null)
				{
					section = new ExplicitRowColumnSection();
					section._endRowCol = endRowCol;
					section._startRowCol = startRowCol;
					section._span = span;
					section._items.Add(item);
					sections.Add(section);
					sectionCount++;
				}

				// keep track of the highest slow we have processed
				highestSlotSoFar = Math.Max(highestSlotSoFar, endRowCol);
			}

			#endregion //Slot the items into sections

			#region Add an ExplicitRowColumnLayoutItem for each section

			int oldChildCount = oldChildGbls.Count;

			foreach (ExplicitRowColumnSection section in sections)
			{
				ExplicitRowColumnLayoutItem childGblItem = null;

				// see if we already have one that matches
				for (int i = 0; i < oldChildCount; i++)
				{
					if (oldChildGbls[i].isEquivalent(section))
					{
						childGblItem = oldChildGbls[i];
						oldChildGbls.RemoveAt(i);
						oldChildCount--;
						break;
					}
				}

				if (childGblItem == null)
					childGblItem = new ExplicitRowColumnLayoutItem(this, section);
				else
					childGblItem.GridBagManager.InvalidateLayout();

				GridBagLayoutManager childGbl = childGblItem.GridBagManager;

				childGbl.InterItemSpacingHorizontal = interTileSpacingX;
				childGbl.InterItemSpacingVertical = interTileSpacingY;
				childGbl.HorizontalContentAlignment = gbl.HorizontalContentAlignment;
				childGbl.VerticalContentAlignment = gbl.VerticalContentAlignment;
				childGbl.ExpandToFitWidth = gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch;
				childGbl.ExpandToFitHeight = gbl.VerticalContentAlignment == VerticalAlignment.Stretch;
				childGbl.LayoutMode = GridBagLayoutManager.GridBagLayoutMode.Uniform;

				childGblItem.CalculateminMaxPreffered();

				liCollection.Add(childGblItem, childGblItem);
			}

			#endregion //Add an ExplicitRowColumnLayoutItem for each section	
    	
		}
                #endregion //GenerateNormalExplicitElements

                #region GenerateNormalVariableElements

        private void GenerateNormalVariableElements(TileAreaPanel.GenerationCache genCache, bool createRows)
        {
            GridBagLayoutManager gbl = this.GridBagLayoutNormalModeOverall;

            gbl.HorizontalContentAlignment      = this._panel.GetHorizontalTileAreaAlignment();
            gbl.VerticalContentAlignment        = this._panel.GetVerticalTileAreaAlignment();
            gbl.ExpandToFitWidth                = gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch;
            gbl.ExpandToFitHeight               = gbl.VerticalContentAlignment == VerticalAlignment.Stretch;

			// SSP 4/11/2011 TFS32232
			// 
			gbl.LayoutMode = GridBagLayoutManager.GridBagLayoutMode.Standard;

            bool showAllTiles = this._owningManager.NormalModeSettingsSafe.ShowAllTiles;

            if (showAllTiles)
            {
                gbl.ShrinkToFitHeight = true;
                gbl.ShrinkToFitWidth = true;
            }
            else
            {
                gbl.ShrinkToFitHeight = false;
                gbl.ShrinkToFitWidth = false;

                // if we aren't at scroll position 0 we need to find the items
                // at the top of the logical col/row from the previous layout
                // 
                if (this._scrollLogicalOffset > 0)
                {
                    LayoutItemsCollection oldLayoutItems = genCache._previousGridBagLayoutNormalModeOverall != null ? genCache._previousGridBagLayoutNormalModeOverall.LayoutItems : null;

                    int count = oldLayoutItems != null ? oldLayoutItems.Count : 0;

                    if (oldLayoutItems != null && oldLayoutItems.Count > 0)
                    {
                        TileRowColumnLayoutItem firstRowCol = (TileRowColumnLayoutItem)oldLayoutItems[0];
                        TileRowColumnLayoutItem lastRowCol = (TileRowColumnLayoutItem)oldLayoutItems[oldLayoutItems.Count - 1];
                        LayoutItem firstRowFirstItem = firstRowCol.FirstChildLayoutItem;

                        if (firstRowFirstItem == null)
                        {
                            // null out genCache._previousGridBagLayoutNormalModeOverall which will cause
                            // the scrollposition to be set to 0 and will cause another gn pass
                            genCache._previousGridBagLayoutNormalModeOverall = null;
                        }
                        else
                        {
                            // see if the desired row columns is already in view
                            if (this._scrollLogicalOffset >= firstRowCol.ScrollOffset &&
                                this._scrollLogicalOffset <= lastRowCol.ScrollOffset)
                            {
                                TileRowColumnLayoutItem targetRowCol = (TileRowColumnLayoutItem)oldLayoutItems[this._scrollLogicalOffset - firstRowCol.ScrollOffset];

                                this._scrollPosition = targetRowCol.GetFirstScrollPosition();
                            }
                            else
                            {
                                int offsetDelta;

                                if (this._scrollLogicalOffset < firstRowCol.ScrollOffset)
                                    offsetDelta = firstRowCol.ScrollOffset - this._scrollLogicalOffset;
                                else
                                    offsetDelta = this._scrollLogicalOffset - lastRowCol.ScrollOffset;

                                double itemDelta = offsetDelta * this._variableAvgItemsPerRowColumn;

                                // if we are within another page's worth of items then hydrate all 
                                // the intervening containers so we can do a precise scroll.
                                // Otherwise, leave the calcualte scrollposition alonge. It was calculated
                                // based on the _variableAvgItemsPerRowColumn and is therefore just
                                // an estaimated position. 
                                
                                
                                
                                if (itemDelta <= Math.Max(this._variableAvgItemsPerRowColumn * (oldLayoutItems.Count + 1), 20))
                                {
                                    #region Create temp col/cor layouts and walk forward or backward to fin the right scroll position

                                    int logicalOffset;
                                    int step;
                                    int scrollPos;
                                    ScrollDirection direction;

                                    // determine if we are scrolling forward or backward from 
                                    // the previous layout
                                    if (this._scrollLogicalOffset < firstRowCol.ScrollOffset)
                                    {
                                        logicalOffset = firstRowCol.ScrollOffset;
                                        step = -1;
                                        direction = ScrollDirection.Decrement;
                                        
                                        
                                        //scrollPos = firstRowCol.GetFirstScrollPosition() - 1;
                                        scrollPos = firstRowCol.GetFirstScrollPosition();
                                    }
                                    else
                                    {
                                        logicalOffset = lastRowCol.ScrollOffset;
                                        step = 1;
                                        direction = ScrollDirection.Increment;
                                        
                                        
                                        
                                        scrollPos = lastRowCol.GetLastScrollPosition();
                                    }

                                    int prevScrollPos = scrollPos;

                                    
                                    bool wasScrollPosAdjusted = false;

                                    while (logicalOffset != this._scrollLogicalOffset &&
                                           logicalOffset > 0 &&
                                           scrollPos > 0 &&
                                           scrollPos < this._sparseArray.ScrollCount)
                                    {
                                        logicalOffset += step;

                                        
                                        // Adjust the scrollPos up or down based on the direction
                                        scrollPos += step;
                                   
                                        
                                        // Set the flag so we know we have been thru here at least once
                                        wasScrollPosAdjusted = true;

                                        GridBagLayoutItemDimensionsCollection dims = gbl.CalculateAutoLayout(
                                                        new TileRowColumnGeneratorEnumerator(this, genCache, createRows, scrollPos, logicalOffset, direction),
                                                        createRows ? Orientation.Vertical : Orientation.Horizontal,
                                                        false,
                                                        this._panel.TileAreaSize,
                                                        1, 1, 1, 1, null );


                                        this._wasLastScrollableItemGened = false;

                                        TileRowColumnLayoutItem rcLayoutItem = null;
                                        foreach (TileRowColumnLayoutItem rc in dims)
                                            rcLayoutItem = rc;

                                        Debug.Assert(rcLayoutItem != null, "We should have created 1 rowcol here");

                                        if (rcLayoutItem == null)
                                            break;

                                        
                                        // If we are going backwards use the position of the last element
                                        // that was laid out above
                                        if ( direction == ScrollDirection.Decrement )
                                            scrollPos = rcLayoutItem.LastScrollPositionGenerated;

                                        // Make sure we don't end up in a loop
                                        if (scrollPos == prevScrollPos)
                                        {
                                            if (direction == ScrollDirection.Increment)
                                                scrollPos++;
                                            else
                                                scrollPos--;
                                        }

                                        prevScrollPos = scrollPos;
                                    }

                                    
                                    // If we never entered the while loop above then
                                    // adjust the scrollPos and logicalOffset by the step 
                                    if (wasScrollPosAdjusted == false)
                                    {
                                        scrollPos += step;
                                        logicalOffset += step;
                                    }

                                    if (scrollPos < 0)
                                        scrollPos = 0;

                                    scrollPos = Math.Min(scrollPos, this._sparseArray.ScrollCount - 1);

                                    if (scrollPos == 0)
                                        logicalOffset = 0;

                                    this._scrollPosition = scrollPos;
                                    this._scrollLogicalOffset = logicalOffset;
                                    #endregion //Create temp col/cor layouts and walk forward or backward to fin the right scroll position
                                }
                            }
                        }
                    }

                }
            }

            gbl.InterItemSpacingHorizontal      = this._panel.GetInterTileSpacing(false, TileState.Normal);
            gbl.InterItemSpacingVertical        = this._panel.GetInterTileSpacing(true, TileState.Normal);

			NormalModeSettings settings = _owningManager.NormalModeSettingsSafe;

            // call auto layout to generate all the items that will fit
            GridBagLayoutItemDimensionsCollection dimsCollection
             =  CalculateAutoLayoutHelper(gbl, genCache,
                                        new TileRowColumnGeneratorEnumerator(this, genCache, createRows, this._scrollPosition, this._scrollLogicalOffset, ScrollDirection.Increment),
                                        showAllTiles,
                                        this._panel.TileAreaSize, 
                                        createRows ? settings.MinRows : 1,
                                        createRows ? 1 : settings.MinColumns,
                                        createRows ? settings.MaxRows : 1,
                                        createRows ? 1 : settings.MaxColumns,
                                        
                                        
                                        
                                        createRows ? Orientation.Horizontal : Orientation.Vertical);

            if (createRows)
            {
                this._totalRowsDisplayed = dimsCollection.RowDims.Length - 1;

                
                // If there were partial tiles generated then
                // adjust the _totalRowsDisplayed down by 1
                if (this._totalRowsDisplayed > 1)
                {
                    double overallHeight = dimsCollection.RowDims[this._totalRowsDisplayed];

                    if (overallHeight > this._panel.TileAreaSize.Height)
                        this._totalRowsDisplayed--;
                }
            }
            else
            {
                this._totalColumnsDisplayed = dimsCollection.ColumnDims.Length - 1;

                
                // If there were partial tiles generated then
                // adjust the _totalColumnsDisplayed down by 1
                if (this._totalColumnsDisplayed > 1)
                {
                    double overallWidth = dimsCollection.ColumnDims[this._totalColumnsDisplayed];

                    if (overallWidth > this._panel.TileAreaSize.Width)
                        this._totalColumnsDisplayed--;
                }
            }
        }
                
                #endregion //GenerateNormalVariableElements	

                #region GetContainingGridBagLayoutManager

		// JJD 5/9/11 - TFS74206 
		// Changed out parameter to ILayoutItem sectionContainer to support ExplicitRowColumnLayoutItem
		// private GridBagLayoutManager GetContainingGridBagLayoutManager(ILayoutItem layoutItem, out TileRowColumnLayoutItem trci)
        private GridBagLayoutManager GetContainingGridBagLayoutManager(ILayoutItem layoutItem, out ILayoutItem sectionContainer)
        {
            sectionContainer = null;

            GridBagLayoutManager gbl = null;
            if (this._isInMaximizedMode)
            {
                gbl = this.GridBagLayoutMaximizedModeOverall;

                if (gbl != null && layoutItem is LayoutItem)
                {
                    foreach (TileAreaLayoutItem tali in gbl.LayoutItems)
                    {
                        if (tali != null)
                        {
                            if (tali.GridBagManager.LayoutItems.Contains(layoutItem))
                                return tali.GridBagManager;
                        }
                    }
                }
            }
            else
            {
                switch (this._tileLayoutOrder)
                {
                    default:
					case TileLayoutOrder.Vertical:
                    case TileLayoutOrder.Horizontal:
                        gbl = this.GridBagLayoutNormal;
                        break;
                    case TileLayoutOrder.UseExplicitRowColumnOnTile:
						// JJD 5/9/11 - TFS74206 - added ExplicitLayoutTileSizeBehavior
						if (this._explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAndHeights)
							gbl = this.GridBagLayoutNormal;
						else
						{
							gbl = this.GridBagLayoutNormalModeOverall;
							if (gbl != null && layoutItem is LayoutItem)
							{
								foreach (ExplicitRowColumnLayoutItem exli in gbl.LayoutItems)
								{
									if (exli != null)
									{
										if (exli.GridBagManager.LayoutItems.Contains(layoutItem))
										{
											sectionContainer = exli;
											return exli.GridBagManager;
										}
									}
								}
							}
						}
						break;
                    case TileLayoutOrder.VerticalVariable:
                    case TileLayoutOrder.HorizontalVariable:
                        gbl = this.GridBagLayoutNormalModeOverall;

                        if (gbl != null && layoutItem is LayoutItem)
                        {
                            foreach (TileRowColumnLayoutItem rcli in gbl.LayoutItems)
                            {
                                if (rcli != null)
                                {
                                    if (rcli.GridBagManager.LayoutItems.Contains(layoutItem))
                                    {
                                        sectionContainer = rcli;
                                        return rcli.GridBagManager;
                                    }
                                }
                            }
                        }

                        break;
                }
            }

            if (gbl != null && gbl.LayoutItems.Contains(layoutItem))
                return gbl;

            return null;

        }

                #endregion //GetContainingGridBagLayoutManager	

                #region GetCurrenRowColumnHelper

        private int GetCurrenRowColumnHelper(ItemTileInfo info, bool isColumn)
        {
            LayoutItem li = this.GetLayoutItem(info.Item);

            if (li == null)
                return -1;

            if (this._isInMaximizedMode)
            {
                if (info.IsMaximized)
                {
                    #region Process maximized items

                    GridBagLayoutManager gbmMaximized = this.GridBagLayoutMaximized;

                    int slot = this.GetRowColumnSlot(isColumn, li, gbmMaximized);

                    if (slot < 1)
                        return -1;

                    switch (this._maximizedTileLocation)
                    {
                        case MaximizedTileLocation.Right:
                            if (isColumn)
                                return slot + 1;
                            break;

                        case MaximizedTileLocation.Bottom:
                            if (!isColumn)
                                return slot + 1;
                            break;

                    }

                    return slot;

                    #endregion //Process maximized items
                }
                else
                {
                    #region Process minimized items

                    GridBagLayoutManager gbmMinimized = this.GridBagLayoutMinimized;

                    int index = gbmMinimized.LayoutItems.IndexOf(li);

                    if (index < 1)
                        return -1;

                    switch (this._maximizedTileLocation)
                    {
                        case MaximizedTileLocation.Left:
                            if (isColumn)
                                index += this._totalMaximizedColumns;
                            break;
                        case MaximizedTileLocation.Right:
                            if (isColumn)
                                return 0;
                            break;
                        case MaximizedTileLocation.Top:
                            if (!isColumn)
                                index += this._totalMaximizedRows;
                            break;
                        case MaximizedTileLocation.Bottom:
                            if (!isColumn)
                                return 0;
                            break;
                    }

                    return index;

                    #endregion //Process minimized items
                }
            }

			// JJD 5/9/11 - TFS74206 
			if (this._tileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile)
				return isColumn ? ((IGridBagConstraint)li).Column : ((IGridBagConstraint)li).Row; 

			// JJD 5/9/11 - TFS74206 
			// Changed out parameter type
			//TileRowColumnLayoutItem trci;
			ILayoutItem subSection;
            GridBagLayoutManager gbm = this.GetContainingGridBagLayoutManager(li, out subSection);

            if (gbm == null)
                return -1;

            GridBagLayoutItemDimensionsCollection dims = gbm.GetLayoutItemDimensions(this, gbm);
            TileRowColumnLayoutItem trci = subSection as TileRowColumnLayoutItem;

            if (trci != null)
            {
                switch (this._tileLayoutOrder)
                {
                    case TileLayoutOrder.VerticalVariable:
                        if (isColumn)
                            return this.GetRowColumnSlot(isColumn, trci, this.GridBagLayoutNormalModeOverall);
                        break;
                    case TileLayoutOrder.HorizontalVariable:
                        if (!isColumn)
                            return this.GetRowColumnSlot(isColumn, trci, this.GridBagLayoutNormalModeOverall);
                        break;
                }
            }

            return this.GetRowColumnSlot(isColumn, li, gbm);

        }

                #endregion //GetCurrenRowColumnHelper	
                
                #region GetItemConstraints

        private ITileConstraints GetItemConstraints(XamTile tile)
        {
            if (this._lastTile != tile)
            {
                this._lastTile = tile;
 
                TileState state = tile.State;

                if (this._lastElementState.HasValue == false ||
                    state != this._lastElementState.Value)
                {
                    this._lastElementState = state;
                    this._lastElementDefaultConstraints = this._panel.GetDefaultConstraints(state);
                }

                this._lastElementConstraints = this._panel.GetContainerConstraints(tile, state);
            }

            return this._lastElementConstraints;
        }

                #endregion //GetItemConstraints	
 
                #region GetItemRowColumnSnapshot

        private List<ItemRowColumnSizeInfo> GetItemRowColumnSnapshot()
        {
            List<ItemRowColumnSizeInfo> list = new List<ItemRowColumnSizeInfo>();

            if (this._isInMaximizedMode)
            {
                // add maximized items first
                this.GetItemRowColumnSnapshotHelper(list, this.GridBagLayoutMaximized.LayoutItems, null, null);

                // then add minimized items
                this.GetItemRowColumnSnapshotHelper(list, this.GridBagLayoutMinimized.LayoutItems, null, null);
            }
            else
            {
                switch (this._tileLayoutOrder)
                {
                    default:
					case TileLayoutOrder.Vertical:
                    case TileLayoutOrder.Horizontal:
                        this.GetItemRowColumnSnapshotHelper(list, this.GridBagLayoutNormal.LayoutItems, null, null);
                        break;

                    case TileLayoutOrder.VerticalVariable:
                    case TileLayoutOrder.HorizontalVariable:
                        {
                            LayoutItemsCollection layoutItemsOverall = this.GridBagLayoutNormalModeOverall.LayoutItems;

                            int count = layoutItemsOverall.Count;

                            int? explicitRow = null;
                            int? explicitColumn = null;

                            bool isVertical = this._tileLayoutOrder == TileLayoutOrder.VerticalVariable;

                            for (int i = 0; i < count; i++)
                            {
                                TileRowColumnLayoutItem rowColumnItem = layoutItemsOverall[i] as TileRowColumnLayoutItem;

                                Debug.Assert(rowColumnItem != null, "should be TileRowColumnLayoutItem in GridBagLayoutNormalModeOverall");

                                if (rowColumnItem == null)
                                    break;

                                if (isVertical)
                                    explicitColumn = i;
                                else
                                    explicitRow = i;

                                this.GetItemRowColumnSnapshotHelper(list, rowColumnItem.GridBagManager.LayoutItems, explicitRow, explicitColumn);
                            }

                            break;
                        }
                }
            }

            return list;
        }

        private void GetItemRowColumnSnapshotHelper(List<ItemRowColumnSizeInfo> list, LayoutItemsCollection layoutItems, int? explicitRow, int? explicitColumn)
        {
            int count = layoutItems.Count;

            for (int i = 0; i < count; i++)
            {
                LayoutItem li = (LayoutItem)layoutItems[i];

                Debug.Assert(li != null, "wrong item in layout item collection");

                if (li == null)
                    return;

                IGridBagConstraint constraint = (IGridBagConstraint)li;

                list.Add(new ItemRowColumnSizeInfo(li.ItemInfo,
                                                explicitRow.HasValue ? explicitRow.Value : constraint.Row,
                                                explicitColumn.HasValue ? explicitColumn.Value : constraint.Column,
                                                li.TargetRect,
                                                li.PreferredSizeCache));
            }
        }

                #endregion //GetItemRowColumnSnapshot	
    
                #region GetRowColumnProperty

        private object GetRowColumnProperty(XamTile tile, DependencyProperty property)
        {
            // Call GetItemConstraints method which will cache the tile state
            // in the _lastElementState member
            ITileConstraints constraints = this.GetItemConstraints(tile);

            TileState state;

            if (this._lastElementState.HasValue)
                state = this._lastElementState.Value;
            else
                state = TileState.Normal;

            bool isElementMaximized = false;

            switch (state)
            {
                case TileState.Normal:
                    return tile.GetValue(property);
 
                case TileState.Maximized:
                    isElementMaximized = true;
                    break;
            }

            
            
            
            
            bool areMinimizedTilesStackedVertically = this.IsMaximizedAreaLeftOrRight;

            switch ( DependencyPropertyUtilities.GetName( property))
            {
                case "Column":
                    if (isElementMaximized)
                    {
                        return this._maximizedTileLayoutOrder == MaximizedTileLayoutOrder.SingleColumn
                            ? 0 : GridBagConstraintConstants.Relative;
                    }

                    return areMinimizedTilesStackedVertically ? 0 : GridBagConstraintConstants.Relative;

                case "ColumnSpan":
                    if (isElementMaximized)
                    {
                        return this._maximizedTileLayoutOrder == MaximizedTileLayoutOrder.SingleColumn
                            ? 1 : GridBagConstraintConstants.Remainder;
                    }

                    return areMinimizedTilesStackedVertically ? 1 : GridBagConstraintConstants.Remainder;

                case "ColumnWeight":
                    return 0f;
 
                case "Row":
                    if (isElementMaximized)
                    {
                        return this._maximizedTileLayoutOrder == MaximizedTileLayoutOrder.SingleRow
                            ? 0 : GridBagConstraintConstants.Relative;
                    }
 
                    return areMinimizedTilesStackedVertically ? GridBagConstraintConstants.Relative : 0;

                case "RowSpan":
                    if (isElementMaximized)
                    {
                        return this._maximizedTileLayoutOrder == MaximizedTileLayoutOrder.SingleRow
                            ? 1 : GridBagConstraintConstants.Remainder;
                    }

                    return areMinimizedTilesStackedVertically ? GridBagConstraintConstants.Remainder : 1;

                case "RowWeight":
                    
                    
                    // If the minimized tiles are arranged vertically then give weight to 
                    // expanded tiles only
                    if ( areMinimizedTilesStackedVertically && state == TileState.MinimizedExpanded )
                        return 1f;
                    else
                        return 0f;
            }

            Debug.Assert(false, "Invalid property name", DependencyPropertyUtilities.GetName( property ));

            return null;
        }

                #endregion //GetRowColumnProperty

                #region GetRowColumnSlot

        private int GetRowColumnSlot(bool isColumn, ILayoutItem li, GridBagLayoutManager gbm)
        {
            GridBagLayoutItemDimensionsCollection dimsCollection = gbm.GetLayoutItemDimensions(this, gbm);

            GridBagLayoutItemDimensions dims = dimsCollection[li];

            if (dims != null)
            {
                if (isColumn)
                    return dims.Column;
                else
                    return dims.Row;
            }

            return -1;
        }

                #endregion //GetRowColumnSlot	

                #region GetTileAreaLayoutItem

        private TileAreaLayoutItem GetTileAreaLayoutItem(TileArea area)
        {
            GridBagLayoutManager gbm = this.GridBagLayoutMaximizedModeOverall;
            int count = gbm.LayoutItems.Count;
            for (int i = 0; i < count; i++) 
            {
                TileAreaLayoutItem liArea = gbm.LayoutItems[i] as TileAreaLayoutItem;

                Debug.Assert(liArea != null, "unexpected layout item in GridBagLayoutMaximizedModeOverall");
                if (liArea != null && liArea.Area == area)
                    return liArea;
            }
             
            return null;
        }

                #endregion //GetTileAreaLayoutItem	

				#region InitializeAllItems

		private void InitializeAllItems()
		{
			//loop over the items to make sure that if they are XamTiles that they are initialized
			foreach (object item in this.Items)
			{
				this.OnItemAdded(item);
			}
		}

				#endregion //InitializeAllItems	
        
                #region ItemResizeHelper

        private bool ItemResizeHelper(ItemTileInfo itemInfo, double deltaX, double deltaY)
        {
            LayoutItem li = this.GetLayoutItem(itemInfo.Item);

            if (li == null)
                return false;

            
            // If we are not in a fixed layout (i.e UseExplicitRowColumnOnTile) and not in maximized mode
            // then we just resize the target item.
            // This prevents a situation where the resize operation caused a re-layout of items such
            // that they are in different row/columns.
            // The downside is that when size an items smaller it is possible that the user won't see
            // any change if the tile's Vertical/HorizontalAlignment is set the 'Stretch' (the default).
			// JJD 5/9/11 - TFS74206
			// Fall thru unless we are using Horizontal or Vertical (letting thru HorizontalVariable and VerticalVariable)
			//if (!this._isInMaximizedMode &&
			//    this._tileLayoutOrder != TileLayoutOrder.UseExplicitRowColumnOnTile )
			if (!this._isInMaximizedMode &&
				(this._tileLayoutOrder == TileLayoutOrder.Horizontal ||
				this._tileLayoutOrder == TileLayoutOrder.Vertical))
			{
                ItemTileInfo liInfo = li.ItemInfo;

                if (liInfo != null)
                {
					Size newPreferred = new Size( li.TargetRect.Width, li.TargetRect.Height);

					newPreferred.Width = Math.Max(1, newPreferred.Width + deltaX);
					newPreferred.Height = Math.Max(1, newPreferred.Height + deltaY);

					// JJD 5/9/11 - TFS74206
					// Use new UpdatePreferredSizeOverride method which will only keep the
					// the preferred size override in the dimension(s) that are being resized
					//liInfo.SizeOverride = newPreferred;
					UpdatePreferredSizeOverride(newPreferred, deltaX, deltaY, li, liInfo);

                    return true;
                }
                
                return false;
            }

			// JJD 5/9/11 - TFS74206 
			// Changed out parameter type
			//TileRowColumnLayoutItem rcli;
			ILayoutItem sectionContainer;

			GridBagLayoutManager gbl = this.GetContainingGridBagLayoutManager(li, out sectionContainer);

            if (gbl == null)
                return false;
            
			object layoutContext = sectionContainer != null ? (object)sectionContainer : gbl;

			// JJD 5/9/11 - TFS74206
			// Cache the expandToFit/shrinkToFit settings 
			bool holdExpandWidth	= gbl.ExpandToFitWidth;
			bool holdExpandHeight	= gbl.ExpandToFitHeight;
			bool? holdShrinkWidth	= gbl.ShrinkToFitWidth;
			bool? holdShrinkHeight	= gbl.ShrinkToFitHeight;

			bool autoFitWidth		= gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch;
			bool autoFitHeight		= gbl.VerticalContentAlignment == VerticalAlignment.Stretch;

			ExplicitRowColumnLayoutItem explicitContainer = sectionContainer as ExplicitRowColumnLayoutItem;

			// JJD 5/9/11 - TFS74206
			// if this is an ExplicitRowColumnLayoutItem then we need to temporarily override
			// the ExpandToFit... and ShrinkToFit... settins in dimension that corresponds to the 
			// ExplicitLayoutTileSizeBehavior. We also need to set the preferred size override 
			// of its child items to their respective target sizes in that dimension.
			// This has to be done before we call ResizeItem eblow
			if (explicitContainer != null)
			{
				double deltaXToUse = 0;
				double deltaYToUse = 0;
				if (_explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAcrossRows)
				{
					autoFitWidth = false;
					gbl.ExpandToFitWidth = false;
					gbl.ShrinkToFitWidth = false;
					deltaXToUse = deltaX;
				}
				else
				{
					autoFitHeight = false;
					gbl.ExpandToFitHeight = false;
					gbl.ShrinkToFitHeight = false;
					deltaYToUse = deltaY;
				}

				SetPreferredSizeOverrideToTargetSize(deltaXToUse, deltaYToUse, explicitContainer);
			}

			//Dictionary<ILayoutItem, Size> itemsToResize = gbl.ResizeItem(this, layoutContext, li, deltaX, deltaY,
			//                                gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch,
			//                                gbl.VerticalContentAlignment == VerticalAlignment.Stretch);
			Dictionary<ILayoutItem, Size> itemsToResize = gbl.ResizeItem(this, layoutContext, li, deltaX, deltaY,
                                            autoFitWidth,autoFitHeight);

			// JJD 5/9/11 - TFS74206
			// restore the expandToFit/shrinkToFit settings cached abive
			gbl.ExpandToFitWidth	= holdExpandWidth;
			gbl.ExpandToFitHeight	= holdExpandHeight;
			gbl.ShrinkToFitWidth	= holdShrinkWidth;
			gbl.ShrinkToFitHeight	= holdShrinkHeight;

            int count = itemsToResize.Count;

            foreach (KeyValuePair<ILayoutItem, Size> entry in itemsToResize)
            {
                LayoutItem siblingLi = entry.Key as LayoutItem;

                if (siblingLi != null)
                {
                    ItemTileInfo siblingInfo = siblingLi.ItemInfo;

					if (siblingInfo != null)
					{
						double deltaXToUse = deltaX;
						double deltaYToUse = deltaY;

						// JJD 5/9/11 - TFS74206
						// When in one of the variable flow layouts only update siblings
						// preferred size overrides in the appropriate dimension
						if (siblingLi != li)
						{
							if (this._tileLayoutOrder == TileLayoutOrder.HorizontalVariable)
								deltaYToUse = 0;
							else
								if (this._tileLayoutOrder == TileLayoutOrder.VerticalVariable)
									deltaXToUse = 0;
						}


						// JJD 5/9/11 - TFS74206
						// Use new UpdatePreferredSizeOverride method which will only keep the
						// the preferred size override in the dimension(s) that are being resized
						//siblingInfo.SizeOverride = entry.Value;
						UpdatePreferredSizeOverride(entry.Value, deltaXToUse, deltaYToUse, siblingLi, siblingInfo);
						
					}
				}
            }

			// JJD 5/9/11 - TFS74206
			// If the section is a ExplicitRowColumnLayoutItem we need to resize its sibling
			// ExplicitRowColumnLayoutItems if we are being resized in correspondng dimension
			if ( explicitContainer != null)
			{
				bool updateOtherSections = false;

				if ( this._explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAcrossRows)
					updateOtherSections = deltaX != 0 && explicitContainer.GridBagManager.HorizontalContentAlignment == HorizontalAlignment.Stretch;
				else
					updateOtherSections = deltaY != 0 && explicitContainer.GridBagManager.VerticalContentAlignment == VerticalAlignment.Stretch;

				if (updateOtherSections)
				{
					// JJD 5/9/11 - TFS74206
					// since we are only intereseted in the corresponding dimension use 0 for the other
					double deltaXToUse = 0;
					double deltaYToUse = 0;
					if (_explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAcrossRows)
						deltaXToUse = deltaX; 
					else
						deltaYToUse = deltaY; 

					// JJD 5/9/11 - TFS74206
					// Call ResizeItem on the overall guy so we can get back the appropriate sizes for its siblings
					GridBagLayoutManager gblOverall = this.GridBagLayoutNormalModeOverall;

					Dictionary<ILayoutItem, Size> sectionsToResize = gblOverall.ResizeItem(this, gblOverall, explicitContainer, deltaXToUse, deltaYToUse,
													gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch,
													gbl.VerticalContentAlignment == VerticalAlignment.Stretch);


					int sectionCount = sectionsToResize.Count;

					// JJD 5/9/11 - TFS74206
					// Walk over the entries in the returned dictionary and use
					foreach (KeyValuePair<ILayoutItem, Size> entry in sectionsToResize)
					{
						ExplicitRowColumnLayoutItem siblingSection = entry.Key as ExplicitRowColumnLayoutItem;

						if (siblingSection != null && siblingSection != explicitContainer)
						{
							// set the section's Rect which will set the appropriate target rect of
							// all items in contains
							Rect sibSectionRect = siblingSection.Rect;
							
							siblingSection.Rect = new Rect(sibSectionRect.Left, sibSectionRect.Top, entry.Value.Width, entry.Value.Height);

							// Sync the preferred size override with the new target rects
							SetPreferredSizeOverrideToTargetSize(deltaXToUse, deltaYToUse, siblingSection);

							// invalidate the layout and re-calculate the cached sizes
							siblingSection.GridBagManager.InvalidateLayout();
							siblingSection.CalculateminMaxPreffered();

						}
					}

					foreach (ILayoutItem sibling in explicitContainer.GridBagManager.LayoutItems)
					{
						LayoutItem sibLi = sibling as LayoutItem;

						if ( sibLi != null )
							sibLi.DirtyPreferredSizeCache();
					}

					// invalidate the layout and re-caclulate the cached sizes of the
					// items in the original resized section
					explicitContainer.GridBagManager.InvalidateLayout();
					explicitContainer.CalculateminMaxPreffered();

				}
			}

            return count > 0;
        }

                #endregion //ItemResizeHelper	
    
                #region OnItemsCollectionChanged

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnItemsChanged(e);
        }

                #endregion //OnItemsCollectionChanged	
        
                #region ProcessDimensionsCollection

        private void ProcessDimensionsCollection(GridBagLayoutManager gbl, TileAreaPanel.GenerationCache genCache, GridBagLayoutItemDimensionsCollection dimsCollection)
        {
            foreach (ILayoutItem entry in dimsCollection)
            {

                LayoutItem li = entry as LayoutItem;

                if (li != null)
                {
                    li.SetDimensions(dimsCollection[li]);

                    
                    
                    //this._itemsToBeArranged.Add(li);
                    this.AddItemToBeArranged(li);

                    this._itemsInView.Add(li.ItemInfo);

                    UpdateGenCacheOnItemAdded(genCache, li);

                    // JJD 3/5/10 - TFS29071
                    // Set the ZIndex of maximized tiles such that they are in
                    // front of non-maximized tiles
                    if (li.ItemInfo.IsMaximized && li.Container != null)
                        li.Container.SetValue(Canvas.ZIndexProperty, 1000);
                }
                else
                {
                    TileRowColumnLayoutItem rcli = entry as TileRowColumnLayoutItem;

                    if (rcli != null)
                    {
                        rcli.ProcessGeneratedElements(genCache);
                    }
                }

                gbl.LayoutItems.Add(entry, entry);
            }
        }

                #endregion //ProcessDimensionsCollection	
    
                #region ProcessResetNotification

        private void ProcessResetNotification()
        {
            this._owningManager.VerifyMaximizedItems();

            try
            {
				if (this._itemsInOriginalOrder == null)
				{
					// Make sure all the items are initialized
					this.InitializeAllItems();
                    return;
				}

                List<object> oldItems = new List<object>(this._itemsInOriginalOrder.Count);
                IList newItems = this.Items;
                int newItemCount = newItems.Count;
                int oldItemCount = this._itemsInOriginalOrder.Count;

                if (this._sparseArray.Count != oldItemCount)
                {
                    Debug.Assert(false, "The cached item arrays are out of sync in a reset notification");
                    this._sparseArray.Clear();
                    this._sparseArray.Expand(newItemCount);
                    this._itemsInOriginalOrder = null;
					// Make sure all the items are initialized
					this.InitializeAllItems();
					return;
                }

                bool itemsInSameOrder = true;
                int oldItemsFound = 0;

                // walk backwards over the original item array to
                // see if the new items are in the same order
                for (int i = oldItemCount - 1; i >= 0; i--)
                {
                    object item = this._itemsInOriginalOrder[i];

                    if (item != null)
                    {
                        oldItemsFound++;

                        if (i >= newItemCount ||
                            item != newItems[i])
                        {
                            itemsInSameOrder = false;
                            break;
                        }
                    }
                }

                // if the items aren't in the same order then clear both cached lists
                // and they will get recreated lazily
                if ( oldItemsFound == 0 || !itemsInSameOrder)
                {
                    this._itemsInOriginalOrder = null;
                    this._sparseArray.Clear();
					// Make sure all the items are initialized
					this.InitializeAllItems();
					return;
                }

                if (newItemCount < oldItemCount)
                {
                    // first loop over the old trailing items and remove their
                    // corresponding entries from the sparseArray
                    for (int i = newItemCount; i < oldItemCount; i++)
                    {
                        object item = this._itemsInOriginalOrder[i];

                        if (item != null)
                        {
                            ItemTileInfo info = this.GetItemInfo(item, false, -1);

                            if (info != null)
                                this._sparseArray.Remove(info);
                        }
                    }

                    this._itemsInOriginalOrder.RemoveRange(newItemCount, oldItemCount - newItemCount);

                    // JJD 3/2/10 - TFS28760
                    // If the sparse array is still larger than the new items list we need to remove
                    // any trailing slots. This can happen when there are null slots in the sparse array.
                    int sparseArrayCount = this._sparseArray.Count;

                    if ( sparseArrayCount > newItemCount )
                        this._sparseArray.RemoveRange(newItemCount, sparseArrayCount - newItemCount);

                    return;
                }

                if (newItemCount > oldItemCount)
                    this.ExpandCachedItemArrays(newItemCount);
            }
            finally
            {
				List<object> itemsToRemove = new List<object>();

                // Loop over all of the entries in the itemInfoMap looking
                // entries that no longer exist (i.e their index will be -1)
                foreach (KeyValuePair<object, ItemTileInfo> entry in this._itemInfoMap)
                {
                    if (entry.Value.Index < 0)
                        itemsToRemove.Add(entry.Key);
                }

                int count = itemsToRemove.Count;

                // remove the entries that no longer exist
                for (int i = 0; i < count; i++)
                {
                    this.RemoveItemFromMaps(itemsToRemove[i], true);

					this.OnItemRemoved(itemsToRemove[i]);
                }

				// when we have no items left then release all generated XamTiles
				if (_itemInfoMap.Count == 0)
					RecyclingManager.Manager.ReleaseAll(_panel);
            }
        }

                #endregion //ProcessResetNotification

                #region RemoveDuplicateContainerRefs

        private void RemoveDuplicateContainerRefs(LayoutItem layoutItem)
        {
            DependencyObject container = layoutItem.Container;

            if (container == null)
                return;

            if (this._genCache != null)
            {
                LayoutItem li;

                // see if an enter exists keyed by the tile
                if ( this._genCache._previousItemsToBeArrangedTempCache.TryGetValue(container, out li))
                {
                    // If the value is different from the passed in layoutItem then we need to remove it
                    if ( li != layoutItem )
                    {
                        
                        
                        // Remove it from the _itemsToBeArranged hash (which is keyed by LayoutItem)
						if (this._itemsToBeArranged.Contains(li))
						{
							this._itemsToBeArranged.Remove(li);

							li.RecyclingRelease(false);
						}

                        // Remove it from the _previousItemsToBeArrangedTempCache hash (which is keyed by tile)
                        this._genCache._previousItemsToBeArrangedTempCache.Remove(container);
                    }
                }

                return;
            }

            foreach (LayoutItem liToBeArranged in this._itemsToBeArranged)
            {
                if (liToBeArranged != layoutItem &&
                    liToBeArranged.Container == container)
                {
                    

                    this._itemsToBeArranged.Remove(liToBeArranged);

					liToBeArranged.RecyclingRelease(false);
                    break;
                }
            }
        }

                #endregion //RemoveDuplicateContainerRefs	
    
                #region RemoveItemFromMaps

        private void RemoveItemFromMaps(object item, bool dispose)
        {
            ItemTileInfo info;

            if (this._itemInfoMap.TryGetValue(item, out info))
            {
                this._itemInfoMap.Remove(item);

                this._sparseArray.Remove(info);
            }
			else // JM 01-14-10 TFS26059
			{
				if (this._sparseArray.Count > 0)
					this._sparseArray.RemoveAt(this._sparseArray.Count - 1);
			}

            if (this._layoutItemMap.ContainsKey(item))
            {
                LayoutItem li = this._layoutItemMap[item];

                this.RemoveLayoutItem(li, dispose);
            }

        }

                #endregion //RemoveItemFromMaps	
    
                #region RemoveRangeHelper

        private void RemoveRangeHelper(int startIndex, IList list)
        {
            int count = list.Count;

			// JJD 02/13/12 - TFS100969
			// Since some data sources (e.g. DataView) can send remove notifications without
			// supplying the list of items being removed we should check if the list
			// contains the items. If not we should populate a list using the items we
			// have cached in the _itemsInOriginalOrder list. These can then be used
			// inside the calls to RemoveItemFromMaps below so that we can keep our
			// data sturctures in sync.
			if ( count > 0 && list[0] == null )
			{
				List<object> tempList = new List<object>(count);
				for (int i = 0; i < count; i++)
					tempList.Add(_itemsInOriginalOrder[startIndex + i]);

				list = tempList;
			}

            this._itemsInOriginalOrder.RemoveRange(startIndex, count);

            // loop over the removed items and get rid of the corresponding
            // entries in the 2 maps
            for (int i = 0; i < count; i++)
            {
                object item = list[i];
                this.RemoveItemFromMaps(item, true);

				this.OnItemRemoved(item);
			}
        }

                #endregion //RemoveRangeHelper	

				// JJD 5/9/11 - TFS74206 - added
				#region SetPreferredSizeOverrideToTargetSize







		private static void SetPreferredSizeOverrideToTargetSize(double deltaX, double deltaY, ExplicitRowColumnLayoutItem siblingSection)
		{
			if (deltaX == 0 && deltaY == 0)
				return;

			foreach (ILayoutItem cousin in siblingSection.GridBagManager.LayoutItems)
			{
				LayoutItem cousinLi = cousin as LayoutItem;

				if (cousinLi == null)
					continue;

				ItemTileInfo cousinInfo = cousinLi.ItemInfo;

				if (cousinInfo != null)
				{
					// Use new UpdatePreferredSizeOverride method which will only keep the
					// the preferred size override in the dimension(s) that are being resized
					UpdatePreferredSizeOverride(new Size(cousinLi.TargetRect.Width, cousinLi.TargetRect.Height), deltaX, deltaY, cousinLi, cousinInfo);

				}
			}
		}

				#endregion //SetPreferredSizeOverrideToTargetSize	
     
                #region SynchLastScrollPosition

        private void SynchLastScrollPosition()
        {
			this._isSynchLastScrollPosOperationPending = false;
            this._lastScrollPosition            = this._scrollPosition;
            this._lastScrollLogicalOffset       = this._scrollLogicalOffset;
        }

                #endregion //SynchLastScrollPosition	

                #region UpdateGenCacheOnItemAdded

        private static void UpdateGenCacheOnItemAdded(TileAreaPanel.GenerationCache genCache, LayoutItem li)
        {
            
            
            
            
            if (genCache._previousItemsToBeArrangedTempCache.ContainsKey(li.Container))
                genCache._previousItemsToBeArrangedTempCache.Remove(li.Container);
            else
                genCache._newItemsToBeArrangedTempCache.Add(li);
        }

                #endregion //UpdateGenCacheOnItemAdded	

				// JJD 5/9/11 - TFS74206 - added
				#region UpdatePreferredSizeOverride







		private static void UpdatePreferredSizeOverride(Size targetSize, double deltaX, double deltaY, LayoutItem li, ItemTileInfo liInfo)
		{
			Size? oldPreferredSizeOverride = liInfo.SizeOverride;
			Size newPreferredSizeOverride = new Size();

			// if we are resizing in the X dimension then save the width.
			// Otherwise, use the old saved width
			// Note: adjust the width a half pixel smaller to prevent rounding errors triggering
			// an unnecessary re-flowing of the layout
			if (deltaX != 0)
				newPreferredSizeOverride.Width = Math.Max(targetSize.Width - .5d, 0);
			else
				if (oldPreferredSizeOverride.HasValue)
					newPreferredSizeOverride.Width = oldPreferredSizeOverride.Value.Width;

			// if we are resizing in the Y dimension then save the height
			// Otherwise, use the old saved height
			// Note: adjust the width a half pixel smaller to prevent rounding errors triggering
			// an unnecessary re-flowing of the layout
			if (deltaY != 0)
				newPreferredSizeOverride.Height = Math.Max( targetSize.Height - .5d, 0);
			else
				if (oldPreferredSizeOverride.HasValue)
					newPreferredSizeOverride.Height = oldPreferredSizeOverride.Value.Height;

			liInfo.SizeOverride = newPreferredSizeOverride;

			li.DirtyPreferredSizeCache();
		}

				#endregion //UpdatePreferredSizeOverride	
            
        
                #region UpdateScrollingInfo

        private bool UpdateScrollingInfo(GridBagLayoutManager gbl, Size preferredSize, bool isScrollingTilesVertically, bool checkIfScrolledTooFar)
        {
            this._scrollTilesVertically     = false;
            this._scrollTilesHorizontally   = false;

            bool arrangingAllTiles;
            bool useMinSizeConstraints;

            if (this._isInMaximizedMode)
            {
                useMinSizeConstraints       = true;
                arrangingAllTiles           = this._owningManager.MaximizedModeSettingsSafe.ShowAllMinimizedTiles;
            }
            else
            {
                useMinSizeConstraints       = this._owningManager.NormalModeSettingsSafe.ShowAllTiles;

                arrangingAllTiles           = useMinSizeConstraints || this._tileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile;
            }

            if (arrangingAllTiles == false)
            {
                this._scrollTilesHorizontally = !isScrollingTilesVertically;
                this._scrollTilesVertically = isScrollingTilesVertically;
            }
            else
            {
                this._scrollPositionOfLastArrangedItem = this._sparseArray.ScrollCount - 1;
            }

            Size overallExtent;

            if (useMinSizeConstraints)
                overallExtent = gbl.CalculateMinimumSize(this, gbl);
            else
                overallExtent = preferredSize;

            Size extent = new Size();
            Size viewport = new Size(1, 1);

            Size tileAreaSize = this._panel.TileAreaSize;

            bool requiresAnotherPass = false;

            if (this._scrollTilesHorizontally)
            {
                requiresAnotherPass = this.CalculateTileScrollExtents(checkIfScrolledTooFar, ref extent, ref viewport);
            }
            else
            {
                this._scrollSmallChange.X = NON_TILE_SCROLL_SMALL_CHANGE;
                this._scrollLargeChange.X = Math.Max(tileAreaSize.Width - NON_TILE_SCROLL_SMALL_CHANGE, NON_TILE_SCROLL_SMALL_CHANGE);

                extent.Width = overallExtent.Width;
                viewport.Width = tileAreaSize.Width;
                
                this._scrollMaxOffset.X = Math.Max(extent.Width - viewport.Width, 0);
            }

            if (this._scrollTilesVertically)
            {
                requiresAnotherPass = this.CalculateTileScrollExtents(checkIfScrolledTooFar, ref extent, ref viewport);
            }
            else
            {
                this._scrollSmallChange.Y = NON_TILE_SCROLL_SMALL_CHANGE;
                this._scrollLargeChange.Y = Math.Max(tileAreaSize.Height - NON_TILE_SCROLL_SMALL_CHANGE, NON_TILE_SCROLL_SMALL_CHANGE);

                extent.Height = overallExtent.Height;
                viewport.Height = tileAreaSize.Height;
                
                this._scrollMaxOffset.Y = Math.Max(extent.Height - viewport.Height, 0);
            }

            
            
            
            
            
            
            // Only call VerifyScrollData if the requiresAnotherPass flag is true
            // or we are being aclled inside a measure pass (checkIfScrolledTooFar == false)
            if (requiresAnotherPass == true ||
                checkIfScrolledTooFar == false)
            {
                TileAreaPanel.ScrollData sdi = this._panel.ScrollDataInfo;

                if (this._scrollMaxOffset.X == 0)
                {
                    viewport.Width = 1;
                    extent.Width = 1;
                }

                if (this._scrollMaxOffset.Y == 0)
                {
                    viewport.Height = 1;
                    extent.Height = 1;
                }

                
                // Make sure the scrolldata offset is in sync with the _scrollLogicalOffset value
                if ( this._scrollTilesVertically )
                    sdi._offset.Y = this._scrollLogicalOffset;
                else
                if ( this._scrollTilesHorizontally )
                    sdi._offset.X = this._scrollLogicalOffset;
                
                // verify that the viewpot and extent haven't changed.
                // Note: this will also verify that the offset is within bounds
                sdi.VerifyScrollData(viewport, extent);
            }

            return requiresAnotherPass;
        }

                #endregion //UpdateScrollingInfo	

                #region VerifyItemSlot

        private bool VerifyItemSlot(object item, ref int originalItemIndex)
        {
            if (originalItemIndex < 0)
            {
                originalItemIndex = this.Items.IndexOf(item);

                if (originalItemIndex < 0)
                    return false;
            }

            return true;
        }

                #endregion //VerifyItemSlot	
    
            #endregion //Private Methods

        #endregion //Methods

        #region ILayoutContainer Members

        #region GetBounds

        Rect ILayoutContainer.GetBounds(object containerContext)
        {
            if (containerContext == this._gridBagLayoutMaximized)
                return this.MaximizedTileArea.Rect;

            if (containerContext == this._gridBagLayoutMinimized)
                return this.MinimizedTileArea.Rect;

            TileRowColumnLayoutItem tileRowColumn = containerContext as TileRowColumnLayoutItem;

            if (tileRowColumn != null)
                return tileRowColumn.Rect;

			// JJD 5/9/11 - TFS74206 - added 
			ExplicitRowColumnLayoutItem explicitRowColumn = containerContext as ExplicitRowColumnLayoutItem;

			if (explicitRowColumn != null)
				return explicitRowColumn.Rect;

            Debug.Assert(containerContext == this._gridBagLayoutMaximizedModeOverall ||
                         containerContext == this._gridBagLayoutNormalModeOverall ||
                         containerContext == this._gridBagLayoutNormal, "Invalid containerContext");

            GridBagLayoutManager gbl = containerContext as GridBagLayoutManager;

            Size size = this._panel.TileAreaSize;

            if (gbl != null)
            {
                // JJD 2/9/10 - TFS27405
                // Since we can't pass infinity to the GridBagLayoutManager we need to check
                // if either the width or height ar infinity. If so then use the
                // calculated preferred size instead
                if (double.IsPositiveInfinity(size.Width) || double.IsPositiveInfinity(size.Height))
                {
                    Size preferredSize = gbl.CalculatePreferredSize(this, gbl);

                    if (double.IsPositiveInfinity(size.Width))
                        size.Width = preferredSize.Width;

                    if (double.IsPositiveInfinity(size.Height))
                        size.Height = preferredSize.Height;

                }

                Size minSize = gbl.CalculateMinimumSize(this, gbl);

                size.Width = Math.Max(size.Width, minSize.Width);
                size.Height = Math.Max(size.Height, minSize.Height);
            }

            return new Rect(new Point(), size);
        }

        #endregion //GetBounds	
    
        #region PositionItem

        void ILayoutContainer.PositionItem(ILayoutItem item, Rect rect, object containerContext)
        {
            if (containerContext == this._gridBagLayoutMaximizedModeOverall)
            {
                TileAreaLayoutItem tileAreaItem = item as TileAreaLayoutItem;

                Debug.Assert(tileAreaItem != null);

                if (tileAreaItem != null)
                    tileAreaItem.Rect = rect;

                return;
            }

            if (containerContext == this._gridBagLayoutNormalModeOverall)
            {
				// JJD 5/9/11 - TFS74206 - added 
				ExplicitRowColumnLayoutItem explicitRowColumn = item as ExplicitRowColumnLayoutItem;

				if (explicitRowColumn != null)
				{
					explicitRowColumn.Rect = rect;
					return;
				}

				TileRowColumnLayoutItem tileRowColumn = item as TileRowColumnLayoutItem;

                Debug.Assert(tileRowColumn != null);

                if (tileRowColumn != null)
                    tileRowColumn.Rect = rect;

                return;
            }

            LayoutItem layoutItem = item as LayoutItem;

            Debug.Assert(layoutItem != null);

            if (layoutItem != null)
            {
                layoutItem.SetTargetRect(rect, true);
                return;
            }

            return;
        }

        #endregion //PositionItem	
    
        #endregion

        #region ItemRowColumnInfo

        private class ItemRowColumnSizeInfo
        {
            private ItemTileInfo _info;
            private int _row;
            private int _column;
            private Rect _targetRect;
            private Size _preferredSize;
            private bool _wasMaximized;

            internal ItemRowColumnSizeInfo(ItemTileInfo info, int row, int column, Rect targetRect, Size preferredSize)
            {
                this._info          = info;
                this._row           = row;
                this._column        = column;
                this._targetRect    = targetRect;
                this._preferredSize = preferredSize;
                
                // JJD 3/5/10 - TFS29058 
                // Cache maximized state
                this._wasMaximized   = info.IsMaximized;
            }

            internal int            Column { get { return this._column; } }
            internal ItemTileInfo   Info { get { return this._info; } }
            internal int            Row { get { return this._row; } }
            internal Size           PreferredSize { get { return this._preferredSize; } }
            internal Rect           TargetRect { get { return this._targetRect; } }
            internal bool           WasMaximized { get { return this._wasMaximized; } }

            // JJD 3/5/10 - TFS29058 - added
            #region VerifyMaximizeState

            internal Vector VerifyMaximizeState(Vector offset, Vector offsetForMaxmizedTiles)
            {
                bool isMaxmized = this._info.IsMaximized;

                // When the maxmized state changes we need to adjust the target rect based on
                // the maxmized and minimized offsets that were in place when this item
                // was laid out
                if (this._wasMaximized != isMaxmized)
                {
                    Vector adjustment;

                    if (this._wasMaximized)
                        adjustment = offsetForMaxmizedTiles - offset;
                    else
                        adjustment = offset - offsetForMaxmizedTiles;

                    this._targetRect.X += adjustment.X;
                    this._targetRect.Y += adjustment.Y;

                    this._wasMaximized = isMaxmized;

                    return adjustment;
                }

                return new Vector();
            }

            #endregion //VerifyMaximizeState
        }

        #endregion //ItemRowColumnInfo	
    
        #region LayoutItemGeneratorEnumerator private nexted class

        private class LayoutItemGeneratorEnumerator : IEnumerator<ILayoutItem>
        {
            private TileLayoutManager _manager;
            private TileAreaPanel _panel;
            private TileAreaPanel.GenerationCache _genCache;
            private int _countMaximized;
            private int _scrollCount;
            private int _currentLogicalIndex = -2;
            private int _startIndex;
            private bool _isEndReached;
            private bool _enumerateMaximizedTiles;
            private IList _items;
            private IList _maximizedItems;
            private ILayoutItem _currentItem;
            private Orientation _orientation;
            private ScrollDirection _direction;

            internal LayoutItemGeneratorEnumerator(TileLayoutManager manager, TileAreaPanel.GenerationCache genCache, bool enumerateMaximizedTiles, int startIndex, Orientation orientation, ScrollDirection direction)
            {
                this._manager                   = manager;
                this._panel                     = manager._panel;
                this._genCache                  = genCache;
                this._enumerateMaximizedTiles   = enumerateMaximizedTiles;
                this._items                     = this._manager.Items;
                this._scrollCount               = this._manager.SparseArray.ScrollCount;
                this._maximizedItems            = this._manager._owningManager.MaximizedItemsInternal;
                this._countMaximized            = this._maximizedItems.Count;
                this._startIndex                = startIndex;
                this._orientation               = orientation;
                this._direction                 = direction;

                this.Reset();
            }

            #region Methods

            #region EnsureNotEndReached

            private void EnsureNotEndReached()
            {
                if (this._isEndReached)
                    throw new InvalidOperationException(TileUtilities.GetString("LE_InvalidOperationException_1"));
            }

            #endregion //EnsureNotEndReached

            #region GetNextIndexToGen

            private int GetNextIndexToGen()
            {
                object item;
                ItemTileInfo info;

                if (this._enumerateMaximizedTiles)
                {
                    if (this._countMaximized > 0)
                    {
                        if (this._currentLogicalIndex < -1)
                            this._currentLogicalIndex = 0;

                        while (this._currentLogicalIndex < this._countMaximized)
                        {
                            item = this._maximizedItems[this._currentLogicalIndex];

                            this._currentLogicalIndex++;

                            info = this._manager.GetItemInfo(item);

                            if (!info.IsClosed)
                            {
                                //Debug.Assert(info.GetIsMaximized(), "IsMaximized flag out of sync");
                                return info.Index;
                            }
                        }
                    }

                    return -1;
                }

                if (this._currentLogicalIndex < -1)
                    this._currentLogicalIndex = this._startIndex;

                if (this._currentLogicalIndex < 0 ||
                    this._currentLogicalIndex >= this._scrollCount)
                    return -1;

                if (this._currentLogicalIndex >= this._scrollCount)
                    return -1;

                info = this._manager.GetItemAtScrollIndex(this._currentLogicalIndex);

                if ( this._direction == ScrollDirection.Increment )
                     this._currentLogicalIndex++;
                else
                     this._currentLogicalIndex--;

                if (info == null)
                    return -1;
                
                return info.Index;
             }

            #endregion //GetNextIndexToGen

            #endregion //Methods

            #region IEnumerator<ILayoutItem> Members

            public ILayoutItem Current
            {
                get
                {
                    this.EnsureNotEndReached();

                    if (this._currentItem == null)
                        throw new InvalidOperationException(TileUtilities.GetString("LE_InvalidOperationException_2"));

                    return this._currentItem;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                this.EnsureNotEndReached();

                int indexToGen = this.GetNextIndexToGen();

                while (indexToGen >= 0)
                {
                    object item = this._items[indexToGen];

                    XamTile container = this._genCache.GenerateElement(indexToGen);
                    UIElement element = container as UIElement;

					if (container == null ||
						(element != null && element.Visibility == Visibility.Collapsed))
					{
						// JJD 6/28/10 - TFS34917
						// Set the indexToGen to the next index so we don't get caught in
						// an infinite loop
						indexToGen = this.GetNextIndexToGen();
						continue;
					}

                    Dock enterFromSide;

                    if (indexToGen < this._manager._lastScrollPosition)
                    {
                        enterFromSide = this._orientation == Orientation.Vertical
                                            ? Dock.Left : Dock.Top;
                    }
                    else
                    {
                        enterFromSide = this._orientation == Orientation.Vertical
                                            ? Dock.Right : Dock.Bottom;
                    }

                    this._currentItem = this._manager.InitializeLayoutItem(container, item, enterFromSide);

                    return true;
                }

                this._isEndReached = true;
                this._currentItem = null;

                if (this._enumerateMaximizedTiles == false)
                    this._manager._wasLastScrollableItemGened = true;

                return false;
            }

            public void Reset()
            {
                this._isEndReached = false;
                this._currentLogicalIndex = -2;
                this._currentItem = null;
            }

            #endregion
        }

        #endregion //LayoutItemGeneratorEnumerator private nested class	

        #region TileArea enum

        internal enum TileArea
        {
            MaximizedTiles,
            MinimizedTiles,
        }

        #endregion //TileArea enum	
    
        #region LayoutItem internal nested class

        internal class LayoutItem : RecyclingContainer<XamTile>, ILayoutItem, IGridBagConstraint, ITileConstraints
        {
            #region Private members

            private XamTile			_container;
            private object          _item;
            private TileLayoutManager     _layoutManager;

            // The following rects are used during animations. Once the animation completes they should all be the same
            private Rect? _currentRect;
            private Rect _targetRect;
            private Rect _originalRect;
            
            
            
            private Dock _enterFromSide;
            private TileState _lastAtRestTileState;
            private GridBagLayoutItemDimensions _dimensions;
            private bool _fillAvailableRows;
            private bool _fillAvailableColumns;
            private Size _preferredSizeCache;
			// JJD 1/17/11 - TFS36589
            private Size _desiredSizeCache;

			private Size		_lastMeasureSize;

            #endregion //Private members	
    
            #region Constructor

            internal LayoutItem(XamTile container, object item, TileLayoutManager layoutManager)
            {
                this._layoutManager = layoutManager;
                
                this._item = item;

                this.InitializeContainer(container, true);
            }

            #endregion //Constructor	

			#region Base class overrides

			#region CreateInstanceOfRecyclingElement

			/// <summary>
			/// Creates a new instance of a <see cref="XamTile"/> that represents the object.
			/// </summary>
			/// <returns></returns>
			protected override XamTile CreateInstanceOfRecyclingElement()
			{
				XamTile tile = _layoutManager._owningManager.GetContainerForItemOverride();

				tile.InitializeIsGenerated();

				return tile;
			}

			#endregion //CreateInstanceOfRecyclingElement

			#region OnElementAttached

			/// <summary>
			/// Invoked when a XamTile is being attached to an item.
			/// </summary>
			/// <param name="tile">The tile being attached</param>
			protected override void OnElementAttached(XamTile tile)
			{
				_layoutManager._owningManager.PrepareContainerForItemOverride(tile, this.Item);
				
				base.OnElementAttached(tile);

				// Call GetLayoutItem passing in the new tile tile which will
				// update the various maps with the new tile as well as call this.InitializeContainer 
				TileLayoutManager.LayoutItem li = _layoutManager.GetLayoutItem(this.Item, tile, false, true);

				Debug.Assert(li == this, "GetLayoutItem should return this instance inside OnElementAttached");
				Debug.Assert(this._container == ((ISupportRecycling)this).AttachedElement, "Attached element out of sync");

			}

			#endregion //OnElementAttached	
    
			#region OnElementReleased

			/// <summary>
			/// Invoked when a XamTile is being released.
			/// </summary>
			/// <param name="tile">The released tile.</param>
			protected override void OnElementReleased(XamTile tile)
			{
				_layoutManager._owningManager.ClearContainerForItemOverride(tile, this.Item);

				_dimensions = null;
				_currentRect = null;
				_desiredSizeCache = new Size();
				_preferredSizeCache = new Size();
				_lastMeasureSize = new Size();
				_lastAtRestTileState = new TileState();
				_container = null;

				base.OnElementReleased(tile);
			}

			#endregion //OnElementReleased	
 
			#region OnElementReleasing

			/// <summary>
			/// Invoked when a XamTile is about to be released.
			/// </summary>
			/// <param name="tile">The tile being released</param>
			/// <returns>False, if the tile shouldn't be released.</returns>
			protected override bool OnElementReleasing(XamTile tile)
			{
				return true;
			}

			#endregion //OnElementReleasing	
			
			#region RecyclingElementType

			/// <summary>
			/// Returns the type of the recycling element, in this case <see cref="XamTile"/>
			/// </summary>
			protected override Type RecyclingElementType
			{
				get
				{
					return typeof(XamTile);
				}
			}

			#endregion //RecyclingElementType	
           
			#endregion //Base class overrides	
                    
            #region Properties
    
                #region Container

            internal XamTile Container { get { return this._container; } }

                #endregion //Container	

                #region CurrentRect

            internal Rect? CurrentRectInternal
            {
                get { return this._currentRect; }
            }

            internal Rect CurrentRect
            {
                get
                {
                    if (this._currentRect.HasValue)
                        return this._currentRect.Value;

                    Rect rect = this._targetRect;
                    switch (this._enterFromSide)
                    {
                        case Dock.Top:
                            rect.Y = -rect.Height;
                            break;
                        case Dock.Bottom:
                            rect.Y = this._layoutManager._panel.ActualHeight;
                            break;
                        case Dock.Left:
                            rect.X = -rect.Width;
                            break;
                        case Dock.Right:
                            rect.X = this._layoutManager._panel.ActualWidth;
                            break;
                    }

                    
                    
                    
                    //this._originalRect = rect;

                    return rect;
                }
                set 
                {
                    if (this._currentRect != value)
                    {
                        this._currentRect = value;

                        if (this.IsCurrentSameAsTarget)
                        {
                            this._originalRect = this._targetRect;
                            this._lastAtRestTileState = _container != null ? _container.State : TileState.Normal;
                        }
                    }
                }
            }

                #endregion //CurrentRect	
    
                #region EnterFromSide

            internal Dock EnterFromSide
            {
                get { return this._enterFromSide; }
                set { this._enterFromSide = value; }
            }

                #endregion //EnterFromSide	

                #region IsCurrentSameAsTarget

            internal bool IsCurrentSameAsTarget
            {
                get
                {
                    if (!this._currentRect.HasValue)
                        return false;

                    return this._currentRect.Value == this._targetRect;
                }
            }

                #endregion //IsCurrentSameAsTarget	
    
                #region Item

            internal object Item { get { return this._item; } }

                #endregion //Item	
    
                #region ItemInfo

            internal ItemTileInfo ItemInfo 
            { 
                get 
                { 
                    return this._layoutManager.GetItemInfo( this._item, false, -1 ); 
                } 
            }

                #endregion //ItemInfo	
    
                #region OriginalRect

            internal Rect OriginalRect { get { return this._originalRect; } }

                #endregion //OriginalRect	

                #region PreferredSizeCache

            internal Size PreferredSizeCache { get { return this._preferredSizeCache; } }

                #endregion //PreferredSizeCache	
    
                #region ShouldAnimateResize

            internal bool ShouldAnimateResize
            {
                get
                {
                    TileState state = this._container != null ? _container.State : TileState.Normal;

                    // only animate the size change if the tile state has changed
                    return this._lastAtRestTileState != state;
                }
            }

                #endregion //ShouldAnimateResize	

				// JJD 03/14/12 - TFS100150 - Added touch support
				#region ShouldArrangeInView

			internal bool ShouldArrangeInView { get; set; }

   				#endregion //ShouldArrangeInView	
    
                #region TargetRect

            internal Rect TargetRect { get { return this._targetRect; } }

                #endregion //TargetRect	
    
            #endregion //Properties
            
            #region Methods

				#region DirtyPreferredSizeCache

			internal void DirtyPreferredSizeCache()
			{
				this._preferredSizeCache = Size.Empty;
			}

				#endregion //DirtyPreferredSizeCache	
    
                #region EnsureMeasure

            private void EnsureMeasure()
            {
				if (this._container != null)
				{
					Size preferredSize		= this.GetPreferredSizeHelper(true);

					bool measureRequired	=  !this._container.IsMeasureValid;



					
					// JJD 1/17/11 - TFS36589
					Size panelSize = this._layoutManager._panel != null ? this._layoutManager._panel.LastMeasureSize : new Size(1000, 1000);

					// JJD 1/17/11 - TFS36589
					// If the panel's size changes then we need to re-measure
					if (measureRequired == false)
						

						measureRequired = (preferredSize != this._lastMeasureSize && panelSize != this._lastMeasureSize) || _desiredSizeCache.IsEmpty;




					if (measureRequired)
					{
						// JJD 12/20/10 - TFS61057/TFS61064
						// Use the panel's available size when measuring instead of infinity
						//this._element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
						//this._lastMeasureSize = this._element.DesiredSize;
						if (this._layoutManager._panel != null)
						{
							// JJD 1/14/11 - TFS36589
							// If the last measure size is exactly equal to the panel size
							// we need to slightly tweak the size we use to measure to force
							// the measure to process. Otherwise the framework might ignore
							// the measure call below
							//this._lastMeasureSize = this._manager._panel.LastMeasureSize;
							//this._element.Measure(this._lastMeasureSize);
							Size sizeToMeasure = panelSize;


							if (panelSize == this._lastMeasureSize)
								sizeToMeasure.Width += (double)(DateTime.Now.Millisecond + 1 ) / 1000d;


							this._lastMeasureSize = panelSize;

							this._container.Measure(sizeToMeasure);

							// JJD 1/17/11 - TFS36589
							// Cache the desired size
							this._desiredSizeCache = this._container.DesiredSize;
						}
					}
					else
					{
						// JJD 1/17/11 - TFS36589
						// If the panelSize == the last measure size then
						// re-cache the desired size since we didn't measure
						// with a smaller size inside the SetTargetRect method
						if (panelSize == this._lastMeasureSize)
						{
							this._desiredSizeCache = this._container.DesiredSize;
						}

					}
				}
            }

                #endregion //EnsureMeasure	

                #region GetPreferredSizeHelper

            internal Size GetPreferredSizeHelper(bool calledFromEnsureMeasure)
            {
				Size size = GetNonSynchronizedPreferredSize(calledFromEnsureMeasure);
                
                if (this._layoutManager._panel != null && this._layoutManager._isInMaximizedMode == false)
                {
                    Size? synchronizedSize = null;

                    synchronizedSize = this._layoutManager._synchronizedSize;

                    if (synchronizedSize != null)
                    {
                        bool synchronizeWidth  = false;
                        bool synchronizeHeight = false;

                        switch (this._layoutManager._tileLayoutOrder)
                        {
                            case TileLayoutOrder.Horizontal:
                            case TileLayoutOrder.Vertical:
                                synchronizeWidth = true;
                                synchronizeHeight = true;
                                break;
                            case TileLayoutOrder.HorizontalVariable:
                                synchronizeHeight = true;
                                break;
                            case TileLayoutOrder.VerticalVariable:
                                synchronizeWidth = true;
                                break;
                        }

                        if (synchronizeWidth &&
                             synchronizedSize.Value.Width > 0)
                        {
                            size.Width = synchronizedSize.Value.Width;
						}

                        if (synchronizeHeight &&
                             synchronizedSize.Value.Height > 0)
                        {
                            size.Height = synchronizedSize.Value.Height;
						}
                   }
                }

                return size;
            }

            private Size GetNonSynchronizedPreferredSize(bool calledFromEnsureMeasure)
            {
                Size? explicitSize = null;

                double width = double.NaN;
                double height = double.NaN;

                TileState state = this._container != null ? _container.State : TileState.Normal;

                if (state == TileState.Normal)
                {
                    ItemTileInfo info = this.ItemInfo;

                    if (info != null)
                    {
                        explicitSize = info.SizeOverride;

						if (explicitSize.HasValue)
						{
							// JJD 5/9/11 - TFS74206
							// Only return if we hav both explicit the width and height values
							//	return explicitSize.Value;
							if (explicitSize.Value.Width > 0 && explicitSize.Value.Height > 0)
								return explicitSize.Value;

							// JJD 5/9/11 - TFS74206
							// Othwerwise use either the width or height
							if (explicitSize.Value.Width > 0)
								width = explicitSize.Value.Width;

							if (explicitSize.Value.Height > 0)
								height = explicitSize.Value.Height;
						}
					}
                }

                ITileConstraints constraints = this._layoutManager.GetItemConstraints(this._container);

                if (constraints != null)
                {
                    if (double.IsNaN(width))
                        width = constraints.PreferredWidth;

                    if (double.IsNaN(height))
                        height = constraints.PreferredHeight;
                }

                constraints = this._layoutManager.LastElementDefaultConstraints;

                if (constraints != null)
                {
                    if (double.IsNaN(width))
                        width = constraints.PreferredWidth;

                    if (double.IsNaN(height))
                        height = constraints.PreferredHeight;
                }

                if (this._container != null && calledFromEnsureMeasure == false)
                {
					if (double.IsNaN(width))
					{
						this.EnsureMeasure();
						// JJD 1/17/11 - TFS36589
						// Use the cached desired size
						//width = this._element.DesiredSize.Width;
						width = this._desiredSizeCache.Width;
					}

					if (double.IsNaN(height))
                    {
                        this.EnsureMeasure();
						// JJD 1/17/11 - TFS36589
						// Use the cached desired size
                        //height = this._element.DesiredSize.Height;
                        height = this._desiredSizeCache.Height;
					}
                }

                return new Size(width, height);
            }

                #endregion //GetPreferredSizeHelper	
    
                #region InitializeContainer

            internal void InitializeContainer(XamTile container, bool clearDimensions)
            {
                // JJD 2/5/10 - TFS27167
                // Empty the preferred size cache so we we re-calculate it
                // on each measure pass
                if (clearDimensions || container != this._container)
                {
                    this._preferredSizeCache    = Size.Empty;
					
					// JJD 1/17/11 - TFS36589
					// Clear the _desiredSizeCache



					this._desiredSizeCache		= Size.Empty;

                    this._dimensions            = null;
                    this._fillAvailableRows     = false;
                    this._fillAvailableColumns  = false;
                }

                if (container == this._container)
                    return;

                
                

                this._container = container;
            }

                #endregion //InitializeContainer	

                #region OffsetAllRects
    
            internal void OffsetAllRects(Vector offset)
            {
				this._targetRect.X += offset.X;
				this._targetRect.Y += offset.Y;
                this._originalRect.X += offset.X;
                this._originalRect.Y += offset.Y;

				if (this._currentRect.HasValue)
				{
					Rect rect = this._currentRect.Value;
					rect.X += offset.X;
					rect.Y += offset.Y;
					this._currentRect = rect;
				}
            }

   	            #endregion //OffsetAllRects	
        
				#region RecyclingRelease

			internal void RecyclingRelease(bool dispose)
			{
				if (_container == null)
					return;

				Panel parentPanel = VisualTreeHelper.GetParent(_container) as Panel;

				Debug.Assert(parentPanel == _layoutManager._panel, "Parent panel out of sync in RecyclingRelease");

				if (parentPanel != null)
				{
					// if the container is the same as the item we need to remove it from the panel's child collection
					if (_container == _item)
					{
						int index = parentPanel.Children.IndexOf(_container);

						if (index >= 0)
							parentPanel.Children.RemoveAt(index);
						
						return;
					}

					// if the dispose parameter is true then set our ZIsDirty flag to true.
					// This will cause the container XamTile to be removed as a child of
					// the panel on the call to ReleaseElement below
					if (dispose)
						this.IsDirty = true;

					RecyclingManager.Manager.ReleaseElement(this, parentPanel);

					// clear the member
					_container = null;
				}
			}

				#endregion //RecyclingRelease	
    
                #region SetDimensions

            internal void SetDimensions(GridBagLayoutItemDimensions dimensions)
            {
                this._fillAvailableRows = false;
                this._fillAvailableColumns
                                        = false;
                this._dimensions        = dimensions;
            }

                #endregion //SetDimensions	
    
                #region SetFillAvailableColumns

            internal void SetFillAvailableColumns(bool fillAvailableColumns)
            {
                this._fillAvailableColumns = fillAvailableColumns;
            }

                #endregion //SetFillAvailableColumns	
    
                #region SetFillAvailableRows

            internal void SetFillAvailableRows(bool fillAvailableRows)
            {
                this._fillAvailableRows = fillAvailableRows;
            }

                #endregion //SetFillAvailableRows	
        
                #region SetTargetRect

            
            internal void SetTargetRect(Rect rect, bool measureElement)
            {
				Debug.Assert(!double.IsInfinity(rect.Left), "rect.Left is infinity");
				Debug.Assert(!double.IsInfinity(rect.Top), "rect.Top is infinity");
				Debug.Assert(!double.IsInfinity(rect.Width), "rect.Width is infinity");
				Debug.Assert(!double.IsInfinity(rect.Height), "rect.Height is infinity");

				this._targetRect = rect;

				Debug.Assert(this.Item is XamTile || _container == ((ISupportRecycling)this).AttachedElement, "container out of sync");

				
				
                
                // Measure the element if requested
                
                if (this._container != null && measureElement)
				{
					bool measureRequired = false;
					// JJD 1/17/11 - TFS36589 - added
					bool recacheDesiredSize = false;

					// JJD 10/8/10 - TFS42855
					// Only call measure with the constrained size if it is
					// less than the tile's desired size
					Size sizeToMeasure = new Size( rect.Width, rect.Height);
					Size desiredSize = _container.DesiredSize;

					if (CoreUtilities.GreaterThan( desiredSize.Height, sizeToMeasure.Height) ||
						CoreUtilities.GreaterThan( desiredSize.Width , sizeToMeasure.Width))
						measureRequired = true;
					else
					// JJD 12/20/10 - TFS61057/TFS61064
					// Also call measure if the passed in rect is bigger than 
					// the last measure size. In that case call measure with
					// the panel's last measure size
					if (CoreUtilities.LessThan( _lastMeasureSize.Width, sizeToMeasure.Width) ||
						CoreUtilities.LessThan( _lastMeasureSize.Height, sizeToMeasure.Height))
					{
						measureRequired = true;

						// JJD 1/17/11 - TFS36589
						// Re-cache the desired size
						recacheDesiredSize = true;
					}




					if (measureRequired)
					{
						this._lastMeasureSize = sizeToMeasure;
						this._container.Measure(sizeToMeasure);

						// JJD 1/17/11 - TFS36589
						// Re-cache the desired size if flag was set above
						if (recacheDesiredSize)
							_desiredSizeCache = this._container.DesiredSize;
					}
				}

//#if DEBUG
//                ContentControl cc = this._container as ContentControl;

//                Debug.WriteLine(string.Format("SetTargetRect, rect: {0}, LI Hashcode: {1}, Tile content: {2}", rect, this.GetHashCode(), cc != null ? cc.Content : null));
//#endif
            }

                #endregion //SetTargetRect	
        
                #region SetOriginalRect

            
            internal void SetOriginalRect(Rect rect, bool calledFromArrange)
            {
                this._originalRect = rect;
                
                
                

                
                if (!calledFromArrange)
                    this._currentRect = rect;
            }

                #endregion //SetOriginalRect	
            
                #region SnapshotCurrentRect

            internal void SnapshotCurrentRect()
            {
                
                
                

                
                
                
                
                if (this._currentRect.HasValue)
                {
                    this._originalRect = this._currentRect.Value;
                }
            }

                #endregion //SnapshotCurrentRect	
    
            #endregion //Methods

            #region ILayoutItem Members

            #region MaximumSize

            Size ILayoutItem.MaximumSize
            {
                get
                {
                    double width = double.PositiveInfinity;
                    double height = double.PositiveInfinity;

                    ITileConstraints constraints = this._layoutManager.GetItemConstraints(this._container);

                    if (constraints != null)
                    {
                        width = constraints.MaxWidth;
                        height = constraints.MaxHeight;
                    }

                    constraints = this._layoutManager.LastElementDefaultConstraints;

                    if (constraints != null)
                    {
                        if (double.IsPositiveInfinity(width))
                            width = constraints.MaxWidth;

                        if (double.IsPositiveInfinity(height))
                            height = constraints.MaxHeight;
                    }

                    width = Math.Min(width, (double)this._container.GetValue(FrameworkElement.MaxWidthProperty));
                    height = Math.Min(height, (double)this._container.GetValue(FrameworkElement.MaxHeightProperty));

                    return new Size(width, height);
                }
            }

            #endregion //MaximumSize	
    
            #region MinimumSize

            Size ILayoutItem.MinimumSize
            {
                get
                {
                    double width = 0;
                    double height = 0;

                    ITileConstraints constraints = this._layoutManager.GetItemConstraints(this._container);

                    if (constraints != null)
                    {
                        width = constraints.MinWidth;
                        height = constraints.MinHeight;
                    }

                    constraints = this._layoutManager.LastElementDefaultConstraints;

                    if (constraints != null)
                    {
                        if (width == 0)
                            width = constraints.MinWidth;

                        if (height == 0)
                            height = constraints.MinHeight;
                    }

                    width = Math.Max(width, (double)this._container.GetValue(FrameworkElement.MinWidthProperty));
                    height = Math.Max(height, (double)this._container.GetValue(FrameworkElement.MinHeightProperty));

                    if (width == 0)
                        width = this._layoutManager._defaultMinimunItemSize.Width;

                    if (height == 0)
                        height = this._layoutManager._defaultMinimunItemSize.Height;

                    return new Size(width, height);
                }
            }

            #endregion //MinimumSize	
    
            #region PreferredSize

            Size ILayoutItem.PreferredSize
            {
                get
                {
                    // JJD 2/5/10 - TFS27167
                    // Cache the preferred size for each measue pass
                    if ( this._preferredSizeCache.IsEmpty )
					    this._preferredSizeCache = this.GetPreferredSizeHelper(false);

                    return this._preferredSizeCache;
                }
            }

            #endregion //PreferredSize	

            #region Visibility

            Visibility ILayoutItem.Visibility
            {
                get { return this._container != null ? this._container.Visibility : Visibility.Visible; }
            }

            #endregion //Visibility	
    
            #endregion

            #region IGridBagConstraint Members

            #region Column/ColumnSpan/ColumnWeight

            int IGridBagConstraint.Column
            {
                get
                {
                    if (this._dimensions != null)
                        return this._dimensions.Column;

					return (int)this._layoutManager.GetRowColumnProperty(this._container, XamTileManager.ColumnProperty);
                }
            }

            int IGridBagConstraint.ColumnSpan
            {
                get
                {
                    if (this._dimensions != null)
                    {
                        if (this._fillAvailableColumns)
                            return GridBagConstraintConstants.Remainder;

                        return this._dimensions.ColumnSpan;
                    }

					return (int)this._layoutManager.GetRowColumnProperty(this._container, XamTileManager.ColumnSpanProperty);
                }
            }

            float IGridBagConstraint.ColumnWeight
            {
                get
                {
					return (float)this._layoutManager.GetRowColumnProperty(this._container, XamTileManager.ColumnWeightProperty);
                }
            }

            #endregion //Column/ColumnSpan/ColumnWeight	
    
            #region HorizontalAlignment

            HorizontalAlignment IGridBagConstraint.HorizontalAlignment
            {
                get
                {
                    HorizontalAlignment? vAlign = null;

                    ITileConstraints constraints = this._layoutManager.GetItemConstraints(this._container);

                    if (constraints != null)
                    {
                        vAlign = constraints.HorizontalAlignment;

                        if (vAlign.HasValue)
                            return vAlign.Value;
                    }

                    constraints = this._layoutManager.LastElementDefaultConstraints;

                    if (constraints != null)
                        vAlign = constraints.HorizontalAlignment;

                    if (vAlign.HasValue)
                        return vAlign.Value;

                    return (HorizontalAlignment)this._container.GetValue(FrameworkElement.HorizontalAlignmentProperty); ;
                }
            }

            #endregion //HorizontalAlignment	
    
            #region Margin

            Thickness IGridBagConstraint.Margin
            {
                get
                {
                    Thickness? thickness = null;

                    ITileConstraints constraints = this._layoutManager.GetItemConstraints(this._container);

                    if (constraints != null)
                    {
                        thickness = constraints.Margin;

                        if (thickness.HasValue)
                            return thickness.Value;
                    }

                    constraints = this._layoutManager.LastElementDefaultConstraints;

                    if (constraints != null)
                        thickness = constraints.Margin;

                    if (thickness.HasValue)
                        return thickness.Value;
					
					// JJD 4/22/11 - TFS63837
					// Don't return the margin property value for the layout margin.
					// Otherwise, the margin will be used twice.
                    //return (Thickness)this._container.GetValue(FrameworkElement.MarginProperty); ;
                    return new Thickness();
                }
            }

            #endregion //Margin	
    
            #region Row/RowSpan/RowWeight

            int IGridBagConstraint.Row
            {
                get
                {
                    if (this._dimensions != null)
                        return this._dimensions.Row;

					return (int)this._layoutManager.GetRowColumnProperty(this._container, XamTileManager.RowProperty);
                }
            }

            int IGridBagConstraint.RowSpan
            {
                get
                {
                    if (this._dimensions != null)
                    {
                        if (this._fillAvailableRows)
                            return GridBagConstraintConstants.Remainder;

                        return this._dimensions.RowSpan;
                    }

					return (int)this._layoutManager.GetRowColumnProperty(this._container, XamTileManager.RowSpanProperty);
                }
            }

            float IGridBagConstraint.RowWeight
            {
                get
                {
					return (float)this._layoutManager.GetRowColumnProperty(this._container, XamTileManager.RowWeightProperty);
                }
            }

            #endregion //Row/RowSpan/RowWeight	
    
            #region VerticalAlignment

            VerticalAlignment IGridBagConstraint.VerticalAlignment
            {
                get
                {
                    VerticalAlignment? vAlign = null;

                    ITileConstraints constraints = this._layoutManager.GetItemConstraints(this._container);

                    if (constraints != null)
                    {
                        vAlign = constraints.VerticalAlignment;

                        if (vAlign.HasValue)
                            return vAlign.Value;
                    }

                    constraints = this._layoutManager.LastElementDefaultConstraints;

                    if (constraints != null)
                        vAlign = constraints.VerticalAlignment;

                    if (vAlign.HasValue)
                        return vAlign.Value;

                    return (VerticalAlignment)this._container.GetValue(FrameworkElement.VerticalAlignmentProperty); ;
                }
            }

            #endregion //VerticalAlignment	
    
            #endregion

            #region ITileConstraints Members

            HorizontalAlignment? ITileConstraints.HorizontalAlignment
            {
                get { return ((IGridBagConstraint)this).HorizontalAlignment; }
            }

            Thickness? ITileConstraints.Margin
            {
                get { return ((IGridBagConstraint)this).Margin; }
            }

            double ITileConstraints.MaxHeight
            {
                get { return ((ILayoutItem)this).MaximumSize.Height; }
            }

            double ITileConstraints.MaxWidth
            {
                get { return ((ILayoutItem)this).MaximumSize.Width; }
            }

            double ITileConstraints.MinHeight
            {
                get { return ((ILayoutItem)this).MinimumSize.Height; }
            }

            double ITileConstraints.MinWidth
            {
                get { return ((ILayoutItem)this).MinimumSize.Width; }
            }

            double ITileConstraints.PreferredHeight
            {
                get { return ((ILayoutItem)this).PreferredSize.Height; }
            }

            double ITileConstraints.PreferredWidth
            {
                get { return ((ILayoutItem)this).PreferredSize.Width; }
            }

            VerticalAlignment? ITileConstraints.VerticalAlignment
            {
                get { return ((IGridBagConstraint)this).VerticalAlignment; }
            }

            #endregion
		}

        #endregion //LayoutItem internal nested class

		// JJD 5/9/11 - TFS74206 - added
		#region ExplicitItemConstraintWrapper

		// wrapper used to adjust the row and column returned by an item
		private class ExplicitItemConstraintWrapper : IGridBagConstraint
		{
			#region Private Members

			private ExplicitRowColumnLayoutItem _containerItem;
			private IGridBagConstraint _sourceConstraint;

			#endregion //Private Members

			#region Constructor

			internal ExplicitItemConstraintWrapper(ExplicitRowColumnLayoutItem containerItem, IGridBagConstraint sourceConstraint)
			{
				_containerItem = containerItem;
				_sourceConstraint = sourceConstraint;
			}

			#endregion //Constructor

			#region IGridBagConstraint Members

			public int Column
			{
				// Adjust the returned column by the tile's column offset
				get { return _sourceConstraint.Column + _containerItem.ColumnOffset; }
			}

			public int ColumnSpan
			{
				get { return _sourceConstraint.ColumnSpan; }
			}

			public float ColumnWeight
			{
				get { return _sourceConstraint.ColumnWeight; }
			}

			public HorizontalAlignment HorizontalAlignment
			{
				get { return _sourceConstraint.HorizontalAlignment; }
			}

			public Thickness Margin
			{
				get { return _sourceConstraint.Margin; }
			}

			public int Row
			{
				// Adjust the returned row by the tile's row offset
				get { return _sourceConstraint.Row + _containerItem.RowOffset; }
			}

			public int RowSpan
			{
				get { return _sourceConstraint.RowSpan; }
			}

			public float RowWeight
			{
				get { return _sourceConstraint.RowWeight; }

			}

			public VerticalAlignment VerticalAlignment
			{
				get { return _sourceConstraint.VerticalAlignment; }
			}

			#endregion
		}

		#endregion //ExplicitItemConstraintWrapper	
    
		// JJD 5/9/11 - TFS74206 - added
		#region ExplicitLayoutItemComparer internal nested class

		private class ExplicitLayoutItemComparer : Comparer<IGridBagConstraint>
		{
			private ExplicitLayoutTileSizeBehavior _behavior;

			internal ExplicitLayoutItemComparer(ExplicitLayoutTileSizeBehavior behavior)
			{
				_behavior = behavior;
			}

			public override int Compare(IGridBagConstraint x, IGridBagConstraint y)
			{
				if (x == y)
					return 0;

				if (x == null)
					return -1;

				if (y == null)
					return 1;

				int xSpan;
				int ySpan;

				if ( _behavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileHeightsAcrossColumns )
				{
					xSpan		= x.RowSpan;
					ySpan		= y.RowSpan;
				}
				else
				{
					xSpan		= x.ColumnSpan;
					ySpan		= y.ColumnSpan;
				}

				if (xSpan != ySpan)
					return (xSpan < ySpan) ? 1 : -1;

				int xRowCol;
				int yRowCol;

				if ( _behavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileHeightsAcrossColumns )
				{
					xRowCol		= x.Row;
					yRowCol		= y.Row;
				}
				else
				{
					xRowCol		= x.Column;
					yRowCol		= y.Column;
				}
				
				if (xRowCol < yRowCol)
					return -1;

				if (xRowCol > yRowCol)
					return 1;

				return 0;
			}
		}

		#endregion //ExplicitLayoutItemComparer internal nested class

		// JJD 5/9/11 - TFS74206 - added 
		#region ExplicitRowColumnLayoutItem internal nested class

		// Represents either a row section or acolumn section when layout is explicit and
		// ExplicitLayoutTileSizeBehavior is either SynchronizeTileHeightsAcrossColumns or
		// SynchronizeTileWidthsAcrossRows
        internal class ExplicitRowColumnLayoutItem : ILayoutItem, IGridBagConstraint, ITileConstraints
        {
            #region Private members

            private TileLayoutManager                             _manager;
            private int                                     _column;
            private int                                     _columnOffset;
            private int                                     _columnSpan;
            private float                                   _columnWeight;
            private int                                     _row;
            private int                                     _rowOffset;
            private int                                     _rowSpan;
            private float                                   _rowWeight;
            private GridBagLayoutManager                    _gridBagManager = new GridBagLayoutManager();
            private Rect                                    _rect;
            private Size                                    _preferredSize;
            private Size                                    _minSize;
			private Size									_maxSize = new Size( double.PositiveInfinity, double.PositiveInfinity );

            #endregion //Private members	
    
            #region Constructor

            internal ExplicitRowColumnLayoutItem(TileLayoutManager manager, ExplicitRowColumnSection section)
            {
                this._manager           = manager;

				if (_manager._explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileHeightsAcrossColumns)
				{
					this._column = 0;
					this._columnSpan = 1;
					this._row = section._startRowCol;
					this._rowSpan = section._span;
					this._rowOffset = -_row;
				}
				else
				{
					this._column = section._startRowCol;
					this._columnSpan = section._span;
					this._columnOffset = -_column;
					this._row = 0;
					this._rowSpan = 1;
				}

				LayoutItemsCollection liCollection = _gridBagManager.LayoutItems;

				// add each item to the LayoutItems collection but use a ExplicitItemConstraintWrapper
				// as its IGridBadConstraint so that the row/column returned by the item is relative
				// to this section
				foreach (IGridBagConstraint item in section._items)
					liCollection.Add((ILayoutItem)item, new ExplicitItemConstraintWrapper(this, item));
            }

            #endregion //Constructor	
            
            #region Properties

				#region ColumnOffset

			internal int ColumnOffset { get { return _columnOffset; } }

				#endregion //ColumnOffset	
    
                #region GridBagManager

            internal GridBagLayoutManager GridBagManager
            {
                get
                {
                    return this._gridBagManager;
                }
            }

                #endregion //GridBagManager	
            
                #region Rect

            internal Rect Rect
            {
                get { return this._rect; }
                set
                {
                    this._rect = value;

					this._gridBagManager.LayoutContainer(this._manager, this);
                }
            }

                #endregion //Rect	
			
				#region RowOffset

			internal int RowOffset { get { return _rowOffset; } }

				#endregion //RowOffset	
        
            #endregion //Properties
            
            #region Methods

			internal void CalculateminMaxPreffered()
			{
				// cache the calulated min/max and preferred sizes
				this._minSize = this._gridBagManager.CalculateMinimumSize(_manager, _gridBagManager);
				this._maxSize = this._gridBagManager.CalculateMaximumSize(_manager, _gridBagManager);
				this._preferredSize = this._gridBagManager.CalculatePreferredSize(_manager, _gridBagManager);

				// sum the row or column weights based on the ExplicitLayoutTileSizeBehavior
				if (_manager._explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileHeightsAcrossColumns)
				{
					_columnWeight = 0;
					_rowWeight = SumWeights(_gridBagManager.RowWeights, _row + _rowOffset, _rowSpan);
				}
				else
				{
					_columnWeight = SumWeights(_gridBagManager.ColumnWeights, _column + _columnOffset, _columnSpan);
					_rowWeight = 0;
				}
			}

			#region isEquivalent

			// returns true if this item contains all of the items and has the same col or row index/span
			internal bool isEquivalent(ExplicitRowColumnSection section)
			{
				if (this._manager._explicitTileSizeBehavior == ExplicitLayoutTileSizeBehavior.SynchronizeTileHeightsAcrossColumns)
				{
					if (_row != section._startRowCol || _rowSpan != section._span)
						return false;
				}
				else
				{
					if (_column != section._startRowCol || _columnSpan != section._span)
						return false;
				}

				LayoutItemsCollection layoutItems = _gridBagManager.LayoutItems;

				int count = section._items.Count;

				if (count != layoutItems.Count)
					return false;

				for (int i = 0; i < count; i++)
				{
					if (section._items[i] != layoutItems[i])
						return false;
				}

				return true;
			}

				#endregion //isEquivalent	

			#region SumWeights

			private static float SumWeights(float[] weights, int index, int span)
			{
				int start = index;
				int end = start + span - 1;

				int count = weights.Length;

				Debug.Assert(start >= 0);
				Debug.Assert(start < count);
				Debug.Assert(end < count);

				if (end >= count)
					end = count - 1;

				float result = 0;

				for (int i = start; i < end; i++)
					result += weights[i];

				return result;
			}

			#endregion //SumWeights	
     
			#endregion //Methods

			#region ILayoutItem Members

			#region MaximumSize

			Size ILayoutItem.MaximumSize
            {
                get
                {
                    return this._maxSize;
                }
            }

            #endregion //MaximumSize	
    
            #region MinimumSize

            Size ILayoutItem.MinimumSize
            {
                get
                {
                    return this._minSize;
                }
            }

            #endregion //MinimumSize	
    
            #region PreferredSize

            Size ILayoutItem.PreferredSize
            {
                get
                {
                    return this._preferredSize;
                }
            }

            #endregion //PreferredSize	
    
            #region Visibility

            Visibility ILayoutItem.Visibility
            {
                get { return Visibility.Visible; }
            }

            #endregion //Visibility	
    
            #endregion

            #region IGridBagConstraint Members

            #region Column/ColumnSpan/ColumnWeight

            int IGridBagConstraint.Column
            {
                get
                {
                    return this._column;
                }
            }

            int IGridBagConstraint.ColumnSpan
            {
                get
                {
                    return this._columnSpan;
                }
            }

            float IGridBagConstraint.ColumnWeight
            {
                get
                {
                    return this._columnWeight;
                }
            }

            #endregion //Column/ColumnSpan/ColumnWeight	
    
            #region HorizontalAlignment

            HorizontalAlignment IGridBagConstraint.HorizontalAlignment
            {
                get
                {
                    return HorizontalAlignment.Stretch;
                }
            }

            #endregion //HorizontalAlignment	
    
            #region Margin

            Thickness IGridBagConstraint.Margin
            {
                get
                {
                    return new Thickness();
                }
            }

            #endregion //Margin	
    
            #region Row/RowSpan/RowWeight

            int IGridBagConstraint.Row
            {
                get
                {
                    return this._row;
                }
            }

            int IGridBagConstraint.RowSpan
            {
                get
                {
                    return _rowSpan;
                }
            }

            float IGridBagConstraint.RowWeight
            {
                get
                {
                    return this._rowWeight;
                }
            }

            #endregion //Row/RowSpan/RowWeight	
    
            #region VerticalAlignment

            VerticalAlignment IGridBagConstraint.VerticalAlignment
            {
                get
                {
                    return VerticalAlignment.Stretch;
                }
            }

            #endregion //VerticalAlignment	
    
            #endregion

            #region ITileConstraints Members

            HorizontalAlignment? ITileConstraints.HorizontalAlignment
            {
                get { return ((IGridBagConstraint)this).HorizontalAlignment; }
            }

            Thickness? ITileConstraints.Margin
            {
                get { return ((IGridBagConstraint)this).Margin; }
            }

            double ITileConstraints.MaxHeight
            {
                get { return ((ILayoutItem)this).MaximumSize.Height; }
            }

            double ITileConstraints.MaxWidth
            {
                get { return ((ILayoutItem)this).MaximumSize.Width; }
            }

            double ITileConstraints.MinHeight
            {
                get { return ((ILayoutItem)this).MinimumSize.Height; }
            }

            double ITileConstraints.MinWidth
            {
                get { return ((ILayoutItem)this).MinimumSize.Width; }
            }

            double ITileConstraints.PreferredHeight
            {
                get { return ((ILayoutItem)this).PreferredSize.Height; }
            }

            double ITileConstraints.PreferredWidth
            {
                get { return ((ILayoutItem)this).PreferredSize.Width; }
            }

            VerticalAlignment? ITileConstraints.VerticalAlignment
            {
                get { return ((IGridBagConstraint)this).VerticalAlignment; }
            }

            #endregion
        }

        #endregion //ExplicitRowColumnLayoutItem internal nested class

		// JJD 5/9/11 - TFS74206 - added 
		#region ExplicitRowColumnSection internal nested class

		// Temporary class used for slotting the tiles into row or column sections
		internal class ExplicitRowColumnSection
		{
			internal int _endRowCol;
			internal int _startRowCol;
			internal int _span = 1;
			internal List<IGridBagConstraint> _items = new List<IGridBagConstraint>();

			// returns trus if the passed in start and end intersect with this 
			// section's start and end
			internal bool Intersects(int start, int end)
			{
				if (end < _startRowCol)
					return false;
				
				if (start > _endRowCol)
					return false;

				return true;
			}

			internal void Add(IGridBagConstraint item, int start, int end)
			{
				// expand the star/end/span settings to include the new item
				_startRowCol = Math.Min(start, _startRowCol);
				_endRowCol = Math.Max(end, _endRowCol);
				_span = (_endRowCol + 1) - _startRowCol;

				_items.Add(item);
			}

		}

		#endregion //ExplicitRowColumnSection internal nested class	
        
        #region TileAreaLayoutItem internal nested class

        internal class TileAreaLayoutItem : ILayoutItem, IGridBagConstraint, ITileConstraints
        {
            #region Private members

            private TileLayoutManager     _manager;
            private TileArea        _area;
            private Rect            _rect;
            private Size            _minSizeContents;
            private Size            _maxSizeContents;
            private Size            _minSizeArea;
            private Size            _maxSizeArea;
            private Size            _preferredSize;

            #endregion //Private members	
    
            #region Constructor

            internal TileAreaLayoutItem(TileLayoutManager manager, TileArea area)
            {
                this._manager = manager;
                this._area = area;
            }

            #endregion //Constructor	
            
            #region Properties

            #region Area
    
            internal TileArea Area { get { return this._area; } }

   	        #endregion //Area	
    
            #region GridBagManager

            internal GridBagLayoutManager GridBagManager
            {
                get
                {
                    if (this._area == TileArea.MaximizedTiles)
                        return this._manager.GridBagLayoutMaximized;
                    else
                        return this._manager.GridBagLayoutMinimized;
                }
            }

            #endregion //GridBagManager	
        
            #region Rect

            internal Rect Rect
            {
                get { return this._rect; }
                set
                {
                    this._rect = value;

                    GridBagLayoutManager gbl = this.GridBagManager;

                    // layout the child items
                    gbl.LayoutContainer(this._manager, gbl);

                    
                    
                    bool isLeftOrRight = this._manager.IsMaximizedAreaLeftOrRight;

                    Size panelSize = this._manager._panel.TileAreaSize;

                    double oldExtent, newExtent;
                    // depending on area location set the preferred size width or height to the panel's size
                    // also make sure the rect's width or height is not greater than the panel's size
                    // unless it has a min size that is greater.
                    if (isLeftOrRight)
                    {
                        oldExtent = this._rect.Height;
                        
                        this._rect.Height = Math.Max(Math.Min(panelSize.Height, this._rect.Height), this._minSizeContents.Height);

                        // JJD 2/28/10 - TFS28139
                        // The maximized tile are should occupy the panel's full extent
                        // in the scrolling dimension, so set its offset to 0 and
                        // the extent to the pane's extent. 
                        if (this._area == TileArea.MaximizedTiles)
                        {
                            // JJD 3/25/10 - TFS29344
                            // If the panel exetnt is infinity then use the passed
                            // in rect but but make sure it's origin is 0
                            if (double.IsPositiveInfinity(panelSize.Height))
                            {
                                this._rect.Height += Math.Max(this._rect.Y, 0);
                                this._rect.Y = 0;
                            }
                            else
                            {
                                this._rect.Y = 0;
                                this._rect.Height = panelSize.Height;
                            }
                        }
                        
                        newExtent = this._rect.Height;

                        if (this._area == TileArea.MinimizedTiles)
                            this._manager._minimizedAreaActualExtent = this._rect.Width;

                    }
                    else
                    {
                        oldExtent = this._rect.Width;

                        this._rect.Width = Math.Max(Math.Min(panelSize.Width, this._rect.Width), this._minSizeContents.Width);

                        // JJD 2/28/10 - TFS28139
                        // The maximized tile are should occupy the panel's full extent
                        // in the scrolling dimension, so set its offset to 0 and
                        // the extent to the pane's extent. 
                        if (this._area == TileArea.MaximizedTiles)
                        {
                            // JJD 3/25/10 - TFS29344
                            // If the panel exetnt is infinity then use the passed
                            // in rect but but make sure it's origin is 0
                            if (double.IsPositiveInfinity(panelSize.Width))
                            {
                                this._rect.Width += Math.Max(this._rect.X, 0);
                                this._rect.X = 0;
                            }
                            else
                            {
                                this._rect.X = 0;
                                this._rect.Width = panelSize.Width;
                            }
                        }

                        newExtent = this._rect.Width;

                        if (this._area == TileArea.MinimizedTiles)
                            this._manager._minimizedAreaActualExtent = this._rect.Height;
                    }

                    // re-layout the child items if the rect has changed
                    if (!CoreUtilities.AreClose(oldExtent, newExtent))
                    {
                        gbl.LayoutContainer(this._manager, gbl);
                    }

                }
            }

            #endregion //Rect	
    
            #endregion //Properties
            
            #region Methods

                #region InitializeCachedSizes

            internal void InitializeCachedSizes(GridBagLayoutItemDimensionsCollection dims)
            {
                // cache the min/max sizes for the contents that we will return from ITileConstraints
                this._minSizeContents       = dims.MinimumSize;
                this._maxSizeContents       = dims.MaximumSize;

                // also cache the min/max sizes that we will return from ILayoutItem
                // these can be adjuested below based on an explicit extent provided 
                // by the panel
                this._minSizeArea           = this._minSizeContents;

                
                // Always initialize the max size of the Maxmized area to infinity since we want 
                // it to get any extra available space
                if (this._area == TileArea.MaximizedTiles)
                    this._maxSizeArea       = new Size(double.PositiveInfinity, double.PositiveInfinity);
                else
                    this._maxSizeArea       = this._maxSizeContents;

                this._preferredSize         = dims.PreferredSize;

                
                
                

                
                //{
                
                
                
                
                
                bool isLeftOrRight = this._manager.IsMaximizedAreaLeftOrRight;

                // if an explict size is specified for the minimized area
                // by the panel then adjust the min/max sizes 
                // based on location
                if (this._area == TileArea.MinimizedTiles &&
                    this._manager._explicitMinimizedAreaExtent > 0 &&
                    !double.IsPositiveInfinity(this._manager._explicitMinimizedAreaExtent))
                {
                    if (isLeftOrRight)
                    {
                        // JJD 3/25/10 - TFS29270
                        // If the min size is greater than the explicit size then
                        // use the larger value for both min and max
                        //this._minSizeArea.Width = this._manager._explicitMinimizedAreaExtent;
                        //this._maxSizeArea.Width = this._manager._explicitMinimizedAreaExtent;
                        this._minSizeArea.Width = Math.Max(this._minSizeArea.Width, this._manager._explicitMinimizedAreaExtent);
                        this._maxSizeArea.Width = this._minSizeArea.Width;
                    }
                    else
                    {
                        // JJD 3/25/10 - TFS29270
                        // If the min size is greater than the explicit size then
                        // use the larger value for both min and max
                        //this._minSizeArea.Height = this._manager._explicitMinimizedAreaExtent;
                        //this._maxSizeArea.Height = this._manager._explicitMinimizedAreaExtent;
                        this._minSizeArea.Height = Math.Max(this._minSizeArea.Height, this._manager._explicitMinimizedAreaExtent);
                        this._maxSizeArea.Height = this._minSizeArea.Height;
                    }
                }
 
                Size panelSize = this._manager._panel.TileAreaSize;

                // depending on area location set the preferred size width or height to the panel's size
                if (isLeftOrRight)
                {
                    // JJD 2/11/10 - TFS27495
                    // Never set the preferred size to infinity
                    if ( !double.IsPositiveInfinity(panelSize.Height))
                        this._preferredSize.Height = panelSize.Height;
                }
                else
                {
                    // JJD 2/11/10 - TFS27495
                    // Never set the preferred size to infinity
                    if ( !double.IsPositiveInfinity(panelSize.Width))
                        this._preferredSize.Width = panelSize.Width;
                }

            }

                #endregion //InitializeCachedSizes	

            #endregion //Methods

            #region ILayoutItem Members

            #region MaximumSize

            Size ILayoutItem.MaximumSize
            {
                get
                {
                    return this._maxSizeArea;
                }
            }

            #endregion //MaximumSize	
    
            #region MinimumSize

            Size ILayoutItem.MinimumSize
            {
                get
                {
                    return this._minSizeArea;
                }
            }

            #endregion //MinimumSize	
    
            #region PreferredSize

            Size ILayoutItem.PreferredSize
            {
                get
                {
                    return this._preferredSize;
                }
            }

            #endregion //PreferredSize	
    
            #region Visibility

            Visibility ILayoutItem.Visibility
            {
                get { return Visibility.Visible; }
            }

            #endregion //Visibility	
    
            #endregion

            #region IGridBagConstraint Members

            #region Column/ColumnSpan/ColumnWeight

            int IGridBagConstraint.Column
            {
                get
                {
                    if (this._area == TileArea.MaximizedTiles)
                    {
                        if (this._manager._maximizedTileLocation == MaximizedTileLocation.Right)
                            return 1;
                        else
                            return 0;
                    }
                    else
                    {

                        if (this._manager._maximizedTileLocation == MaximizedTileLocation.Left)
                            return 1;
                        else
                            return 0;
                    }
                }
            }

            int IGridBagConstraint.ColumnSpan
            {
                get
                {
                    return 1;
                }
            }

            float IGridBagConstraint.ColumnWeight
            {
                get
                {
                    if (this._area == TileArea.MaximizedTiles)
                        return 1f;
                    else
                        return 0f;
                }
            }

            #endregion //Column/ColumnSpan/ColumnWeight	
    
            #region HorizontalAlignment

            HorizontalAlignment IGridBagConstraint.HorizontalAlignment
            {
                get
                {
                    
                    // For the miminmized tile area return the HorizontalTileAreaAlignment
                    // setting if the maximized area is top or bottom
                    if (this._area == TileArea.MinimizedTiles &&
                        this._manager._panel != null)
                    {
                        if (!this._manager.IsMaximizedAreaLeftOrRight)
                            return this._manager._panel.GetHorizontalTileAreaAlignment();
                    }

                    return HorizontalAlignment.Stretch;
                }
            }

            #endregion //HorizontalAlignment	
    
            #region Margin

            Thickness IGridBagConstraint.Margin
            {
                get
                {
                    return new Thickness();
                }
            }

            #endregion //Margin	
    
            #region Row/RowSpan/RowWeight

            int IGridBagConstraint.Row
            {
                get
                {
                    if (this._area == TileArea.MaximizedTiles)
                    {
                        if (this._manager._maximizedTileLocation == MaximizedTileLocation.Bottom)
                            return 1;
                        else
                            return 0;
                    }
                    else
                    {
                        if (this._manager._maximizedTileLocation == MaximizedTileLocation.Top)
                            return 1;
                        else
                            return 0;
                    }
                }
            }

            int IGridBagConstraint.RowSpan
            {
                get
                {
                    return 1;
                }
            }

            float IGridBagConstraint.RowWeight
            {
                get
                {
                    if (this._area == TileArea.MaximizedTiles)
                        return 1f;
                    else
                        return 0f;
                }
            }

            #endregion //Row/RowSpan/RowWeight	
    
            #region VerticalAlignment

            VerticalAlignment IGridBagConstraint.VerticalAlignment
            {
                get
                {
                    
                    // For the miminmized tile area return the VerticalTileAreaAlignment
                    // setting if the maximized area is left or right
                    if (this._area == TileArea.MinimizedTiles &&
                        this._manager._panel != null)
                    {
                        if (this._manager.IsMaximizedAreaLeftOrRight)
                            return this._manager._panel.GetVerticalTileAreaAlignment();
                    }

                    return VerticalAlignment.Stretch;
                }
            }

            #endregion //VerticalAlignment	
    
            #endregion

            #region ITileConstraints Members

            HorizontalAlignment? ITileConstraints.HorizontalAlignment
            {
                get { return ((IGridBagConstraint)this).HorizontalAlignment; }
            }

            Thickness? ITileConstraints.Margin
            {
                get { return ((IGridBagConstraint)this).Margin; }
            }

            double ITileConstraints.MaxHeight
            {
                get { return this._maxSizeContents.Height; }
            }

            double ITileConstraints.MaxWidth
            {
                get { return this._maxSizeContents.Width; }
            }

            double ITileConstraints.MinHeight
            {
                get { return this._minSizeContents.Height; }
            }

            double ITileConstraints.MinWidth
            {
                get { return this._minSizeContents.Width; }
            }

            double ITileConstraints.PreferredHeight
            {
                get { return this._preferredSize.Height; }
            }

            double ITileConstraints.PreferredWidth
            {
                get { return this._preferredSize.Width; }
            }

            VerticalAlignment? ITileConstraints.VerticalAlignment
            {
                get { return ((IGridBagConstraint)this).VerticalAlignment; }
            }

            #endregion
        }

        #endregion //TileAreaLayoutItem internal nested class

        #region TileRowColumnLayoutItem internal nested class

        internal class TileRowColumnLayoutItem : ILayoutItem, IGridBagConstraint, ITileConstraints
        {
            #region Private members

            private TileLayoutManager                             _manager;
            private int                                     _rowColumnIndex;
            private int                                     _scrollOffset;
            private int                                     _lastScrollPositionGenerated;
            private bool                                    _isTileRow;
            private GridBagLayoutManager                    _gridBagManager = new GridBagLayoutManager();
            private GridBagLayoutItemDimensionsCollection   _childDimensionsCollection;
            private Rect                                    _rect;
            private Size                                    _preferredSize;
            private Size                                    _minSize;
			
			
			
            
			private Size									_maxSize = new Size( double.PositiveInfinity, double.PositiveInfinity );

            #endregion //Private members	
    
            #region Constructor

            internal TileRowColumnLayoutItem(TileLayoutManager manager, bool isTileRow, int rowColumnIndex, int scrollOffset)
            {
                this._manager           = manager;
                this._isTileRow         = isTileRow;
                this._rowColumnIndex    = rowColumnIndex;
                this._scrollOffset      = scrollOffset;
            }

            #endregion //Constructor	
            
            #region Properties

                #region FirstChildLayoutItem

            internal LayoutItem FirstChildLayoutItem
            {
                get
                {
                    if (this._gridBagManager.LayoutItems.Count < 1)
                        return null;

                    return (LayoutItem)this._gridBagManager.LayoutItems[0];
                }
            }

                #endregion //FirstChildLayoutItem	
    
                #region GridBagManager

            internal GridBagLayoutManager GridBagManager
            {
                get
                {
                    return this._gridBagManager;
                }
            }

                #endregion //GridBagManager	

                #region LastChildLayoutItem

            internal LayoutItem LastChildLayoutItem
            {
                get
                {
                    if (this._gridBagManager.LayoutItems.Count < 1)
                        return null;

                    return (LayoutItem)this._gridBagManager.LayoutItems[this._gridBagManager.LayoutItems.Count - 1];
                }
            }

                #endregion //LastChildLayoutItem	

                #region LastScrollPositionGenerated

            internal int LastScrollPositionGenerated { get { return this._lastScrollPositionGenerated; } }

                #endregion //LastScrollPositionGenerated	
            
                #region Rect

            internal Rect Rect
            {
                get { return this._rect; }
                set
                {
                    this._rect = value;

                    Size tileAreaSize = this._manager._panel.TileAreaSize;

                    if (this._isTileRow)
                    {
                        if (this._manager._panel.GetHorizontalTileAreaAlignment() == HorizontalAlignment.Stretch)
                            this._rect.Width = Math.Max(tileAreaSize.Width, this._rect.Width);
                    }
                    else
                    {
                        if (this._manager._panel.GetVerticalTileAreaAlignment() == VerticalAlignment.Stretch)
                            this._rect.Height = Math.Max(tileAreaSize.Height, this._rect.Height);
                    }

                    this._gridBagManager.LayoutContainer(this._manager, this);
                }
            }

                #endregion //Rect	

                #region ScrollOffset

            internal int ScrollOffset { get { return this._scrollOffset; } }

                #endregion //ScrollOffset	
    
            #endregion //Properties
            
            #region Methods

                #region CalculateAutoLayoutHelper

            internal int CalculateAutoLayoutHelper(TileAreaPanel.GenerationCache genCache,
                IEnumerator<ILayoutItem> enumerator,
                bool autoFitAllItems,
                Size constraint,
                int minRows, int minCols, int maxRows, int maxCols,
                Orientation orientation,
                ScrollDirection direction,
                out Size preferredSize)
            {
                this._childDimensionsCollection = this.GridBagManager.CalculateAutoLayout(
                                        enumerator,
                                        orientation,
                                        false,
                                        constraint,
                                        minRows, minCols, maxRows, maxCols, null );

                if (orientation == Orientation.Vertical)
                    this._manager._totalRowsDisplayed = Math.Max( this._childDimensionsCollection.RowDims.Length - 1, this._manager._totalRowsDisplayed);
                else
                    this._manager._totalColumnsDisplayed = Math.Max(this._childDimensionsCollection.ColumnDims.Length - 1, this._manager._totalColumnsDisplayed);;
               
                LayoutItem li = null;
     
                // get the last entry
                foreach (ILayoutItem entry in this._childDimensionsCollection)
                {
                    li = entry as LayoutItem;
                }

                if (li != null)
                {
                    ItemTileInfo info = this._manager.GetItemInfo(li.Item);

                    if (info != null)
                        this._lastScrollPositionGenerated = info.ScrollPosition;
                }

                preferredSize = this._childDimensionsCollection.PreferredSize;

				// SSP 2/8/10 TFS26049
				// Initialize the preferred size of this layout item. Otherwise when the outer layout manager
				// is calculating its auto-layout, this object's ILayoutItem.PreferredSize returns 0,0 and that
				// throws off auto-layout calculations.
				// 
				_preferredSize = preferredSize;

                //Debug.WriteLine(string.Format("Tile col index: {0}, last tile index: {1}, preferred size: {2}", this._rowColumnIndex, lastLogicalIndex, this._preferredSize));

                return this._lastScrollPositionGenerated;
            }

                #endregion //CalculateAutoLayoutHelper	

                #region GetFirstScrollPosition

            internal int GetFirstScrollPosition()
            {
                for (int i = 0; i < this._gridBagManager.LayoutItems.Count; i++)
                {
                    ItemTileInfo info = ((LayoutItem)(this._gridBagManager.LayoutItems[i])).ItemInfo;

                    if (info != null)
                    {
                        int scrollPos = info.ScrollPosition;

                        if (scrollPos >= 0)
                            return scrollPos;
                    }
                }

                return -1;
            }

                #endregion //GetFirstScrollPosition	

                #region GetLastScrollPosition

            internal int GetLastScrollPosition()
            {
                for (int i = this._gridBagManager.LayoutItems.Count - 1; i >= 0; i--)
                {
                    int scrollPos = ((LayoutItem)(this._gridBagManager.LayoutItems[i])).ItemInfo.ScrollPosition;

                    if (scrollPos >= 0)
                        return scrollPos;
                }

                return -1;
            }

                #endregion //GetLastScrollPosition	
    
                #region ProcessGeneratedElements

            internal void ProcessGeneratedElements(TileAreaPanel.GenerationCache genCache)
            {
                if (this._childDimensionsCollection != null)
                {
                    this._manager.ProcessDimensionsCollection(this._gridBagManager, genCache, this._childDimensionsCollection);

                    this._preferredSize = this._childDimensionsCollection.PreferredSize;
                    this._minSize = this._childDimensionsCollection.MinimumSize;
                    this._maxSize = this._childDimensionsCollection.MaximumSize;

                    this._childDimensionsCollection = null;
                }
            }

                #endregion //ProcessGeneratedElements	

            #endregion //Methods

            #region ILayoutItem Members

            #region MaximumSize

            Size ILayoutItem.MaximumSize
            {
                get
                {
                    return this._maxSize;
                }
            }

            #endregion //MaximumSize	
    
            #region MinimumSize

            Size ILayoutItem.MinimumSize
            {
                get
                {
                    return this._minSize;
                }
            }

            #endregion //MinimumSize	
    
            #region PreferredSize

            Size ILayoutItem.PreferredSize
            {
                get
                {
                    return this._preferredSize;
                }
            }

            #endregion //PreferredSize	
    
            #region Visibility

            Visibility ILayoutItem.Visibility
            {
                get { return Visibility.Visible; }
            }

            #endregion //Visibility	
    
            #endregion

            #region IGridBagConstraint Members

            #region Column/ColumnSpan/ColumnWeight

            int IGridBagConstraint.Column
            {
                get
                {
                    return this._isTileRow ? 0 : this._rowColumnIndex;
                }
            }

            int IGridBagConstraint.ColumnSpan
            {
                get
                {
                    return 1;
                }
            }

            float IGridBagConstraint.ColumnWeight
            {
                get
                {
                    return 0f;
                }
            }

            #endregion //Column/ColumnSpan/ColumnWeight	
    
            #region HorizontalAlignment

            HorizontalAlignment IGridBagConstraint.HorizontalAlignment
            {
                get
                {
                    return HorizontalAlignment.Stretch;
                }
            }

            #endregion //HorizontalAlignment	
    
            #region Margin

            Thickness IGridBagConstraint.Margin
            {
                get
                {
                    return new Thickness();
                }
            }

            #endregion //Margin	
    
            #region Row/RowSpan/RowWeight

            int IGridBagConstraint.Row
            {
                get
                {
                    return this._isTileRow ? this._rowColumnIndex : 0;
                }
            }

            int IGridBagConstraint.RowSpan
            {
                get
                {
                    return 1;
                }
            }

            float IGridBagConstraint.RowWeight
            {
                get
                {
                    return 0f;
                }
            }

            #endregion //Row/RowSpan/RowWeight	
    
            #region VerticalAlignment

            VerticalAlignment IGridBagConstraint.VerticalAlignment
            {
                get
                {
                    return VerticalAlignment.Stretch;
                }
            }

            #endregion //VerticalAlignment	
    
            #endregion

            #region ITileConstraints Members

            HorizontalAlignment? ITileConstraints.HorizontalAlignment
            {
                get { return ((IGridBagConstraint)this).HorizontalAlignment; }
            }

            Thickness? ITileConstraints.Margin
            {
                get { return ((IGridBagConstraint)this).Margin; }
            }

            double ITileConstraints.MaxHeight
            {
                get { return ((ILayoutItem)this).MaximumSize.Height; }
            }

            double ITileConstraints.MaxWidth
            {
                get { return ((ILayoutItem)this).MaximumSize.Width; }
            }

            double ITileConstraints.MinHeight
            {
                get { return ((ILayoutItem)this).MinimumSize.Height; }
            }

            double ITileConstraints.MinWidth
            {
                get { return ((ILayoutItem)this).MinimumSize.Width; }
            }

            double ITileConstraints.PreferredHeight
            {
                get { return ((ILayoutItem)this).PreferredSize.Height; }
            }

            double ITileConstraints.PreferredWidth
            {
                get { return ((ILayoutItem)this).PreferredSize.Width; }
            }

            VerticalAlignment? ITileConstraints.VerticalAlignment
            {
                get { return ((IGridBagConstraint)this).VerticalAlignment; }
            }

            #endregion
        }

        #endregion //TileRowColumnLayoutItem internal nested class

        #region TileRowColumnGeneratorEnumerator private nexted class

        private class TileRowColumnGeneratorEnumerator : IEnumerator<ILayoutItem>
        {
            private TileLayoutManager _manager;
            private TileAreaPanel _panel;
            private TileAreaPanel.GenerationCache _genCache;
            private int _scrollCount;
            private int _currentScrollPosition = -2;
            private int _currentScrollOffset;
            private int _currentRowColumnIndex = -1;
            private int _startIndex;
            private double _extentRemaining;
            private bool _isEndReached;
            private bool _enumerateRows;
            private ScrollDirection _direction;
			private TileRowColumnLayoutItem _currentTileRowColumn;
            private List<TileRowColumnLayoutItem> _tileRowColumns = new List<TileRowColumnLayoutItem>();
			private int			_maxElementsToGenerate;

            internal TileRowColumnGeneratorEnumerator(TileLayoutManager manager, TileAreaPanel.GenerationCache genCache, bool enumerateRows, int startIndex, int scrollOffset, ScrollDirection direction)
            {
                this._manager               = manager;
                this._panel                 = manager._panel;
                this._genCache              = genCache;
                this._enumerateRows         = enumerateRows;
                this._scrollCount           = this._manager.SparseArray.ScrollCount;
                this._startIndex            = startIndex;
                this._currentScrollOffset   = scrollOffset;
                this._direction             = direction;
				
				if (enumerateRows)
					this._maxElementsToGenerate = manager._owningManager.NormalModeSettingsSafe.MaxColumns;
				else
					this._maxElementsToGenerate = manager._owningManager.NormalModeSettingsSafe.MaxRows;

                this.Reset();
            }

            #region Methods

            #region EnsureNotEndReached

            private void EnsureNotEndReached()
            {
                if (this._isEndReached)
                    throw new InvalidOperationException(TileUtilities.GetString("LE_InvalidOperationException_1"));
            }

            #endregion //EnsureNotEndReached

            #region GetNextIndexToGen

            private int GetNextIndexToGen()
            {
                if (this._extentRemaining <= 0)
                    return -1;

                ItemTileInfo info;

                if (this._currentScrollPosition < -1)
                    this._currentScrollPosition = this._startIndex;

                if (this._currentScrollPosition < 0 ||
                    this._currentScrollPosition >= this._scrollCount)
                    return -1;

                info = this._manager.GetItemAtScrollIndex(this._currentScrollPosition);

                if (info != null)
                    return info.Index;

                return -1;
            }

            #endregion //GetNextIndexToGen

            #endregion //Methods

            #region IEnumerator<ILayoutItem> Members

            public ILayoutItem Current
            {
                get
                {
                    this.EnsureNotEndReached();

                    if (this._currentTileRowColumn == null)
                        throw new InvalidOperationException(TileUtilities.GetString("LE_InvalidOperationException_2"));

                    return this._currentTileRowColumn;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            public bool MoveNext()
            {
                this.EnsureNotEndReached();

                int indexToGen = this.GetNextIndexToGen();

                if (indexToGen >= 0)
                {
                    this._currentRowColumnIndex++;

                    if (this._currentRowColumnIndex >= this._tileRowColumns.Count)
                    {
                        this._currentTileRowColumn = new TileRowColumnLayoutItem(this._manager, this._enumerateRows, this._currentRowColumnIndex, this._currentScrollOffset);

                        if ( this._direction == ScrollDirection.Increment)
                            this._currentScrollOffset++;
                        else
                            this._currentScrollOffset = Math.Max( this._currentScrollOffset - 1, 0);

                        GridBagLayoutManager gbl = this._currentTileRowColumn.GridBagManager;

                        gbl.InterItemSpacingHorizontal      = this._panel.GetInterTileSpacing(false, TileState.Normal);
                        gbl.InterItemSpacingVertical        = this._panel.GetInterTileSpacing(true, TileState.Normal);
                        gbl.HorizontalContentAlignment      = this._panel.GetHorizontalTileAreaAlignment();
                        gbl.VerticalContentAlignment        = this._panel.GetVerticalTileAreaAlignment();
                        gbl.ExpandToFitHeight               = gbl.VerticalContentAlignment == VerticalAlignment.Stretch;
                        gbl.ExpandToFitWidth                = gbl.HorizontalContentAlignment == HorizontalAlignment.Stretch;
                        gbl.ShrinkToFitHeight               = true;
                        gbl.ShrinkToFitWidth                = true;

                        Orientation orientation = this._enumerateRows ? Orientation.Horizontal : Orientation.Vertical;

                        Size preferredSize;

                        this._currentScrollPosition = this._currentTileRowColumn.CalculateAutoLayoutHelper(this._genCache,
                            new LayoutItemGeneratorEnumerator(this._manager, this._genCache, false, this._currentScrollPosition, orientation, this._direction),
                            false,
                            this._panel.TileAreaSize,
                            0, 0,
                            this._enumerateRows ? 1 : this._maxElementsToGenerate,
                            this._enumerateRows ? this._maxElementsToGenerate : 1,
                            orientation,
                            this._direction,
                            out preferredSize);
                        
                        if (this._enumerateRows)
                            this._extentRemaining -= preferredSize.Height;
                        else
                            this._extentRemaining -= preferredSize.Width;

                        if (this._currentScrollPosition >= 0)
                        {
                            this._tileRowColumns.Add(this._currentTileRowColumn);
                        }
                        else
                            this._currentTileRowColumn = null;

                        if (this._direction == ScrollDirection.Increment)
                            this._currentScrollPosition++;
                        else
                            this._currentScrollPosition--;
                    }
                    else
                        this._currentTileRowColumn = this._tileRowColumns[this._currentRowColumnIndex];

                    if (this._currentTileRowColumn != null)
                        return true;
                }

                this._isEndReached = true;
                this._currentTileRowColumn = null;

                return false;
            }

            public void Reset()
            {
                this._isEndReached = false;
                this._currentScrollPosition = -2;
                this._currentRowColumnIndex = -1;
                this._currentTileRowColumn = null;

                if (this._enumerateRows)
                    this._extentRemaining = this._panel.TileAreaSize.Height;
                else
                    this._extentRemaining = this._panel.TileAreaSize.Width;
            }

            #endregion
        }

        #endregion //TileRowColumnGeneratorEnumerator private nexted class	

        #region ScrollManagerSparseArray internal class

        internal class ScrollManagerSparseArray : SparseArray
        {
            internal ScrollManagerSparseArray()
                : base(20, 1.0f, true, true)
            {
            }

            #region Properties

                #region ScrollCount

            internal int ScrollCount { get { return this.GetScrollCount(); } }

                #endregion //ScrollCount	
    
            #endregion //Properties	
    
            #region Methods

                #region GetItemAtScrollIndex

            internal ItemTileInfo GetItemAtScrollIndex(int index)
            {
                return (ItemTileInfo)this.GetItemAtScrollIndex(index, null);
            }

                #endregion //GetItemAtScrollIndex

                #region GetScrollIndexOfItem

            internal int GetScrollIndexOfItem(ItemTileInfo item)
            {
                return this.GetScrollIndexOf(item);
            }

                #endregion //GetScrollIndexOfItem	
    
                #region InvalidateScrollCounts

            internal void InvalidateScrollCounts()
            {
                this.DirtyScrollCountInfo();
            }

                #endregion //InvalidateScrollCounts	
    
                #region InvalidateItem

            internal void InvalidateItem(ItemTileInfo arrayItem)
            {
                this.NotifyItemScrollCountChanged(arrayItem);
            }

                #endregion //InvalidateItem	
    
            #endregion //Methods	
    
        }

        #endregion //ScrollManagerSparseArray
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