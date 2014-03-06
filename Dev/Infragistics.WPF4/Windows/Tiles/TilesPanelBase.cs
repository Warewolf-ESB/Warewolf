
//#define OUTPUT_ITEMS_GENED

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
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Virtualization;
using System.Xml;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Windows.Resizing;
using Infragistics.Shared;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Internal;
using Infragistics.Windows.Scrolling;
using Infragistics.Collections;

namespace Infragistics.Windows.Tiles
{
	/// <summary>
	/// A <see cref="System.Windows.Controls.Panel"/> derived element that arranges and displays its child elements as tiles, with native support for scrolling and virtualizing those items.
	/// </summary>
    public abstract class TilesPanelBase : RecyclingItemsPanel,
                                            IScrollInfo,
											IDeferredScrollPanel
    {
        #region Private Members

        private AnimationClock          _repositionClock;
        private AnimationClock          _resizeClock;
        private Size                    _lastMeasureSize;  
        private Size                    _lastArrangeSize;

		// JJD 4/15/11 - TFS70658 - OS/Framework bug workaround
		// Cache the last desired size returned from MeasureOverride as well
		// as the size we last measured at which may not be the same. 
		// When a window is maxmized on certain high resoultion monitors
		// we can be measured with a size smaller than what we get in the arrange
        private Size                    _lastDesiredSize; 
		private Size					_lastMeasureSizeAdjusted = Size.Empty;  
 
        private Size                    _tileAreaSize;
        private TileManager             _defaultManager;
        private TileDragManager         _dragManager;

        private DispatcherOperation     _asynchAnimationStart;

        private int                     _activeAnimationCount;
        private int                     _arrangeCount;
        private int                     _measureCount;
        private Vector                  _lastScrollOffset;
        private DateTime                _lastMeasureTimeStamp = DateTime.Now;
        private Size                    _itemsControlLastRenderSize;
        private bool                    _bypassArrangeVerification;
        private bool                    _bypassArrange;
        private bool                    _ignoreChildSizeChanges;
        private bool                    _childSizeChanged; 
        
        private bool                    _inViewItemsHaveChanged;
        private bool                    _isRestartingAnimations;

        private double _repositionFactor = (double)RepositionFactorProperty.DefaultMetadata.DefaultValue;
        private double _resizeFactor    = (double)ResizeFactorProperty.DefaultMetadata.DefaultValue;
        
        private ObservableCollectionExtended<object> _defaultMaximizedItems;

        #endregion //Private Members	
    
        #region Constructor

        /// <summary>
        /// Instantiates a new instance of a <see cref="TilesPanelBase"/>.
        /// </summary>
        protected TilesPanelBase() : base(true)
        {
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
            // JJD 2/13/10 - Race condition detection between Measure and arrange
            // If the _bypassArrange flag is set then clear it and eat the arrange
            if (this._bypassArrange)
            {
                // JJD 2/22/10 - TFS28021/TFS28025
                // Clear the counts
                this._arrangeCount = 0;
                this._measureCount = 0;
                
                this._bypassArrange = false;

                return finalSize;
            }
            bool areLocationsTheSameAsLastTime = false;
            this._ignoreChildSizeChanges = true;

			// JJD 4/15/11 - TFS70658 - OS/Framework bug workaround
			// Cache the last desired size returned from MeasureOverride as well
			// as the size we last measured at which may not be the same. 
			// When a window is maxmized on certain high resoultion monitors
			// we can be measured with a size smaller than what we get in the arrange.
			// If this is the case call MeasureHelper with the arrange size
			// JJD 05/11/12 - TFS110351
			// Changed both checks to be > instead of >= so we don't get in here in a normal screolling operation
			//if ((finalSize.Height > _lastMeasureSize.Height && finalSize.Width >= _lastMeasureSize.Width) ||
			//     (finalSize.Height >= _lastMeasureSize.Height && finalSize.Width > _lastMeasureSize.Width))
			if ((finalSize.Height > _lastMeasureSize.Height && finalSize.Width > _lastMeasureSize.Width) ||
				 (finalSize.Height > _lastMeasureSize.Height && finalSize.Width > _lastMeasureSize.Width))
			{
				if (finalSize != _lastMeasureSizeAdjusted)
				{
					_lastMeasureSizeAdjusted = finalSize;
					this.MeasureHelper(finalSize);
				}
			}
			else
				_lastMeasureSizeAdjusted = Size.Empty;


            // JJD 2/13/10 - Race condition detection between Measure and arrange
            // Keep track of the arrange passes
            this._arrangeCount++;

            try
            {
                TileManager tm = this.GetManager();

                // check the _bypassArrangeVerification flag.
                // this should prvent getting in an endless loop going
                // between measure and arrange passes
                if (this._bypassArrangeVerification)
                    this._bypassArrangeVerification = false;
                else
                {
                    // call VerifyArrangeSize which will make sure the 
                    // scrollbar hasn't scrolled too far. If so it will return false
                    tm.VerifyArrangeSize();

                    // if it return false then invalidate our measrue and return
                    if (!this.IsMeasureValid)
                    {
                        // set a flag so we bypass the verify on the next arrange pass
                        // so we don't end up in a race condition between measure and arrange
                        this._bypassArrangeVerification = true;
                        return finalSize;
                    }
                }

                // let the manager know that the arrange has been called.
                // Note: OnArrange will return true if the items and their locations are
                // the same as the last time this method was called
                
                
                bool inViewItemsAreTheSame;

                tm.OnArrange(out areLocationsTheSameAsLastTime, out inViewItemsAreTheSame);

                if (inViewItemsAreTheSame == false)
                    this._inViewItemsHaveChanged = true;
            }
            finally
            {
                this._ignoreChildSizeChanges = false;
            }

            this._lastArrangeSize = finalSize;

            bool bypassAnimations = false;

            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


            bool requiresAnimation = this.ArrangeHelper(true, false, bypassAnimations);

            if (requiresAnimation)
            {
                
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

                if (this.IsAnimationInProgress == false || this._repositionFactor > .5)
                {
                    
                    
                    // On the first pass try to start the animations asynchonously
                    if (this._asynchAnimationStart == null)
                        this._asynchAnimationStart = this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Utilities.MethodDelegate(this.ProcessAnimationRestart));
                    else
                        ProcessAnimationRestart();
                }
            }
            else
            {
                
                // If we aren't doing animations then call OnArrangeComplete
                if ( this.IsAnimationInProgress == false)
                    this.OnArrangeComplete();
            }

            return finalSize;
        }

			#endregion //ArrangeOverride	

            #region BringIndexIntoView

        /// <summary>
        /// Generates the container for the specific index and brings it into view
        /// </summary>
        /// <param name="index">The position of the item in the list</param>
        /// <exception cref="ArgumentOutOfRangeException">When the index is out of range.</exception>
        protected override void BringIndexIntoView(int index)
        {
            this.GetManager().BringIndexIntoView(index);
        }

            #endregion //BringIndexIntoView	
    
			#region CreateUIElementCollection

		/// <summary>
		/// Returns a UIElementCollection instance for storing child elements.
		/// </summary>
		/// <param name="logicalParent">The logical parent element of the collection to be created.</param>
		/// <returns>An ordered collection of elements that have the specified logical parent.</returns>
		protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
		{
            TilesPanelUIElementCollection coll = new TilesPanelUIElementCollection(this, this, logicalParent);
            coll.CollectionChanged += new EventHandler<EventArgs>(OnCollectionChanged);
            return coll;
        }

			#endregion //CreateUIElementCollection	

			#region GetCanCleanupItem

		/// <summary>
		/// Derived classes must return whether the item at the specified index can be cleaned up.
		/// </summary>
		/// <param name="itemIndex">The index of the item to be cleaned up.</param>
		/// <returns>True if the item at the specified index can be cleaned up, false if it cannot.</returns>
		protected override bool GetCanCleanupItem(int itemIndex)
		{
            if (this.IsAnimationInProgress)
                return false;

			// JJD 4/20/11 - TFS73180
			// Moved logic into helper method so we could call it for the requested
			// index and if that retyrns true then call it with the prior index.
			// Only allow cleanup if both return true. This is because we can generate
			// an extra item in the measure. If we keep cleaning it up here we
			// will cause continuous generation/cleanup activity
			if (this.GetCanCleanupItemHelper(itemIndex))
				return itemIndex == 0 ? true : this.GetCanCleanupItemHelper(itemIndex - 1);

			return false;
		}

		private bool GetCanCleanupItemHelper(int itemIndex)
		{
			TileManager tm = this.GetManager();

			IList items = tm.Items;

			if (items == null)
				return false;

			if (itemIndex < 0 || itemIndex >= items.Count)
				return false;

			object item = items[itemIndex];

			ItemInfoBase info = tm.GetItemInfo(item);

			if (info == null ||
				 info.GetIsMaximized())
				return false;

			FrameworkElement fe = ContainerFromIndex(itemIndex) as FrameworkElement;

			// JJD 4/20/11 - TFS73180
			// If the item is equal to the container then return false
			if (item == fe)
				return false;

			if (fe != null)
			{
				if (fe.IsFocused ||
					fe.IsKeyboardFocusWithin ||
					fe.IsMouseCaptureWithin)
					return false;
			}

			return !tm.IsContainerInArrangeCache(fe);
		}

			#endregion //GetCanCleanupItem

            #region GetLayoutClip
        /// <summary>
        /// Returns a geometry for the clipping mask for the element.
        /// </summary>
        /// <param name="layoutSlotSize">The size of the element</param>
        /// <returns>A geometry to clip that takes into account the TileAreaPadding</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            Thickness tileAreaPadding = this.GetTileAreaPadding();

            if (tileAreaPadding.Bottom == 0 &&
                tileAreaPadding.Top == 0 &&
                tileAreaPadding.Left == 0 &&
                tileAreaPadding.Right == 0)
            {
                this._tileAreaSize = layoutSlotSize;
                return base.GetLayoutClip(layoutSlotSize);
            }

            Rect clipRect = new Rect(layoutSlotSize);

            clipRect.X = tileAreaPadding.Left;
            clipRect.Y = tileAreaPadding.Top;

            clipRect.Width = Math.Max(clipRect.Width - (tileAreaPadding.Left + tileAreaPadding.Right), 0);
            clipRect.Height = Math.Max(clipRect.Height - (tileAreaPadding.Top + tileAreaPadding.Bottom), 0);
            
            this._tileAreaSize = clipRect.Size;


            // JJD 4/29/10 - Optimization
            // Freeze the clip geometry so the framework doesn't need to listen for changes
            //return new RectangleGeometry(clipRect);
            Geometry clip = new RectangleGeometry(clipRect);
            clip.Freeze();
            return clip;
        }
            #endregion //GetLayoutClip

			#region HasLogicalOrientation
		/// <summary>
		/// Gets a value that indicates whether the Panel arranges its descendants in a single dimension.
		/// </summary>
		/// <remarks>Always returns True.</remarks>
		protected override bool HasLogicalOrientation
		{
			get { return true; }
		}

			#endregion //HasLogicalOrientation

			#region IsOkToCleanupUnusedGeneratedElements

		/// <summary>
		/// Called when elements are about to be cleaned up.  Return true to allow cleanup, false to prevent cleanup.
		/// </summary>
		protected override bool IsOkToCleanupUnusedGeneratedElements
		{
            get { return this.IsAnimationInProgress == false && this._dragManager == null; }
		}

			#endregion IsOkToCleanupUnusedGeneratedElements

            #region LogicalOrientation

        /// <summary>
        /// The orientation of the panel
        /// </summary>
        protected override Orientation LogicalOrientation
        {
            get
            {
                if (this.GetIsInMaximizedMode())
                {
                    switch (this.GetMaximizedTileLocation())
                    {
                        case MaximizedTileLocation.Left:
                        case MaximizedTileLocation.Right:
                            return Orientation.Vertical;

                        default:
                            return Orientation.Horizontal;
                    }
                }
                else
                {
                    switch (this.GetTileLayoutOrder())
                    {
                        case TileLayoutOrder.Horizontal:
                        case TileLayoutOrder.HorizontalVariable:
                            return Orientation.Horizontal;

                        default:
                            return Orientation.Vertical;
                    }
                }

            }
        }

            #endregion //LogicalOrientation	
    
			#region MaximumUnusedGeneratedItemsToKeep

		/// <summary>
		/// Returns the maximum number of unused generated items that should be kept around at any given time.
		/// </summary>
		protected override int MaximumUnusedGeneratedItemsToKeep
		{
			get 
			{
				return Math.Min(this.GetManager().CountOfItemsToBeArranged, 100);
            }
		}

			#endregion //MaximumUnusedGeneratedItemsToKeep
    
			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
            //Debug.WriteLine("TilesPanelBase.Measure available size = " + availableSize.ToString());

			// JJD 5/25/11 - TFS76655
			// Back out the fix for TFS70658 since it caused a sever regression that
			// prevented scrolling in certain situations
			// JJD 4/15/11 - TFS70658 - OS/Framework bug workaround
			// When a window is maxmized on certain high resoultion monitors
			// we can be measured with a size smaller than what we get in the arrange.
			// Move logic to MeasureHelper method so we can call it with the cached
			// adjusted size from the last arrange
			//bool useAdjustedSizeFromLastArrange = false;
			//if (false == _lastMeasureSizeAdjusted.IsEmpty &&
			//     availableSize == _lastMeasureSize)
			//{
			//    // JJD 4/15/11 - TFS70658 - OS/Framework bug workaround
			//    // Note: we only want to use the cached size if our visual parent's
			//    // measure is valid.
			//    FrameworkElement feParent = VisualTreeHelper.GetParent(this) as FrameworkElement;

			//    useAdjustedSizeFromLastArrange = feParent != null &&
			//         feParent.IsMeasureValid;
			//}
            
			this._lastMeasureSize = availableSize;

			// JJD 5/25/11 - TFS76655
			// Back out the fix for TFS70658 since it caused a sever regression that
			// prevented scrolling in certain situations
			//if (useAdjustedSizeFromLastArrange)
			//{
			//    this.MeasureHelper(_lastMeasureSizeAdjusted);
			//    this.InvalidateArrange();
			//}
			//else
				_lastDesiredSize = this.MeasureHelper(availableSize);

			return _lastDesiredSize;
		}

		// JJD 4/15/11 - TFS70658 - OS/Framework bug workaround - added MeasureHelper method
		// When a window is maxmized on certain high resoultion monitors
		// we can be measured with a size smaller than what we get in the arrange.
		// Move logic to MeasureHelper method
		private Size MeasureHelper(Size availableSize)
		{

            this._tileAreaSize = availableSize;

            #region Detect race condition between measure and arrange

            // JJD 2/13/10 - Race condition detection between Measure and arrange
            // Keep track of the measure passes
            this._measureCount++;

            ItemsControl ic = null;
            if (this.IsItemsHost)
                ic = ItemsControl.GetItemsOwner(this);

            // by default we will assumme that we don't want to clear the race condition counters
            bool clearRaceConditionCounters = false;

            // if we are the items host for an ItemsControl and it is being resized we can
            // clear our race condition counts
            if (ic != null)
            {
                Size renderSize = ic.RenderSize;

                // If the size is not the same as last time then we can safely
                // clear the race conditon counters
                if (renderSize != this._itemsControlLastRenderSize)
                {
                    clearRaceConditionCounters = true;
                    this._itemsControlLastRenderSize = renderSize;
                }
            }

            // If the ItemsControl size is the same then we need to detect whether
            // the last Measure was performed very recently. 
            if (clearRaceConditionCounters == false)
            {
                TimeSpan span = DateTime.Now.Subtract(this._lastMeasureTimeStamp);

                
                
                // since the last measure was performed more that .1 sec ago
                // we can safely clear the counters
                if (span.TotalSeconds > .1)
                    clearRaceConditionCounters = true;
            }

            // finally check the scroll offset. If that has changed then clear the 
            // counters as well
            if (clearRaceConditionCounters == false)
            {
                ScrollData sd = this.ScrollDataInfo;
                if (sd._offset != this._lastScrollOffset)
                {
                    clearRaceConditionCounters = true;
                    this._lastScrollOffset = sd._offset;
                }
            }

            if (clearRaceConditionCounters)
            {
                this._arrangeCount = 0;
                this._measureCount = 0;
                this._bypassArrange = false;
            }
            
            
            else if (this._arrangeCount > 20 && this._measureCount > 20)
            {
                Debug.WriteLine("*** Race condition detected between TilesPanelBase Measure and Arrange ***");
                this._bypassArrange = true;
            }

            // snapshot the time
            this._lastMeasureTimeStamp = DateTime.Now;

            #endregion //Detect race condition between measure and arrange	
    

            Thickness tileAreaPadding = this.GetTileAreaPadding();

            #region Adjust tile area size to allow for tile area padding

            if (tileAreaPadding.Bottom != 0 ||
             tileAreaPadding.Top != 0)
            {
                if (!double.IsPositiveInfinity(this._tileAreaSize.Height))
                    this._tileAreaSize.Height = Math.Max(1, this._tileAreaSize.Height - (tileAreaPadding.Bottom + tileAreaPadding.Top));
            }

            if (tileAreaPadding.Left != 0 ||
                 tileAreaPadding.Right != 0)
            {
                if (!double.IsPositiveInfinity(this._tileAreaSize.Width))
                    this._tileAreaSize.Width = Math.Max(1, this._tileAreaSize.Width - (tileAreaPadding.Left + tileAreaPadding.Right));
            }

            #endregion //Adjust tile area size to allow for tile area padding	

            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


            GenerationCache genCache = null;

            bool genRequired = true;

            // keep trying to layou items until the GenerationCache returns
            // false for _requiresAnotherLayoutPass. It returns true when
            // it needs to scroll up because we have scrolled too far.
            // This is the pulldown logic
            while (genRequired)
            {
				// JJD 8/17/10 - TFS35319
				// Cache the maximized mode state
				bool isInMaximizedMode = this.GetIsInMaximizedMode();

                using (genCache = new GenerationCache(this, availableSize))
                    genCache.GenerateElements();
                
                // we have to get the flag outside of the 'using' statement
                // above because the implementation of the Dispose method
                // is where that info is dtermined

				// JJD 8/17/10 - TFS35319
				// If the gen pass above caused use to go into/out of maximized
				// mode then do another layout pass
                //genRequired = genCache._requiresAnotherLayoutPass;
                genRequired = genCache._requiresAnotherLayoutPass ||
								isInMaximizedMode != this.GetIsInMaximizedMode();
            }

            Size desiredSize = genCache._desiredSize;
            //Debug.WriteLine("TilesPanelBase measure desired size: " + desiredSize.ToString());

            #region Adjust desired size to allow for tile area padding

            if (tileAreaPadding.Bottom != 0 ||
             tileAreaPadding.Top != 0)
            {
                if (!double.IsPositiveInfinity(desiredSize.Height))
                    desiredSize.Height = Math.Max(1, desiredSize.Height - (tileAreaPadding.Bottom + tileAreaPadding.Top));
            }

            if (tileAreaPadding.Left != 0 ||
                 tileAreaPadding.Right != 0)
            {
                if (!double.IsPositiveInfinity(desiredSize.Width))
                    desiredSize.Width = Math.Max(1, desiredSize.Width - (tileAreaPadding.Left + tileAreaPadding.Right));
            }

            #endregion //Adjust desired size to allow for tile area padding	

            return desiredSize;
        }

			#endregion //MeasureOverride	

            #region OnChildDesiredSizeChanged

        /// <summary>
        /// Invoked when the <see cref="UIElement.DesiredSize"/> of an element changes.
        /// </summary>
        /// <param name="child">The child whose size is being changed.</param>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            if (this._ignoreChildSizeChanges)
            {
                // JJD 2/12/10 
                // Set a flag so we know a child size was changed when we ignored it
                this._childSizeChanged = true;
                return;
            }

            base.OnChildDesiredSizeChanged(child);
        }

            #endregion //OnChildDesiredSizeChanged	
    
			#region OnItemsChanged

		/// <summary>
		/// Called when the contents of the associated list changes.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="args">An instance of ItemsChangedEventArgs that contains information about the items that were changed.</param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			base.OnItemsChanged(sender, args);

		}

			#endregion //OnItemsChanged

            #region OnLostMouseCapture

        /// <summary>
        /// Called when mouse capture is lost
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            
            if (this._dragManager != null)
            {
                this._dragManager.OnDragEnd(e, true);

                this._dragManager = null;
                e.Handled = true;
            }
        }

            #endregion //OnLostMouseCapture	

            #region OnMouseLeftButtonUp

        /// <summary>
        /// Called when the left button is released.
        /// </summary>
        /// <param name="e">arguments</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if ( this._dragManager == null)
            {
                base.OnMouseLeftButtonUp(e);
                return;
            }

            TileDragManager tdm = this._dragManager;
            this._dragManager = null;

            this.ReleaseMouseCapture();

            tdm.OnDragEnd(e, false);

            e.Handled = true;

            this.InvalidateArrange();
        }

            #endregion //OnMouseLeftButtonUp	

            #region OnEndDrag

        internal void OnEndDrag(FrameworkElement container)
        {
            container.ClearValue(IsDraggingPropertyKey);

            this.OnTileDragEnd(container);
        }

            #endregion //OnEndDrag	
    
            #region OnMouseMove

        /// <summary>
        /// Called when the mouse is moved.
        /// </summary>
        /// <param name="e">arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ( this._dragManager == null)
            {
                base.OnMouseMove(e);
                return;
            }

            this._dragManager.OnMouseMove(e);

            e.Handled = true;
        }

            #endregion //OnMouseMove	
    
			#region OnNewElementRealized

		/// <summary>
		/// Called after a newly realized element is generated, added to the children collection and 'prepared'.
		/// </summary>
		/// <param name="element">The newly realized element</param>
		/// <param name="index">The position at which the element was added to the children collection</param>
		protected override void OnNewElementRealized(UIElement element, int index)
		{

		}

			#endregion //OnNewElementRealized

            #region OnVisualChildrenChanged

        /// <summary>
        /// Called when a child element is added or meoved
        /// </summary>
        /// <param name="visualAdded"></param>
        /// <param name="visualRemoved"></param>
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualRemoved != null)
                this.GetManager().RemoveContainer(visualRemoved);
        }

            #endregion //OnVisualChildrenChanged	

			#region TotalVisibleGeneratedItems

		/// <summary>
		/// Derived classes must return the number of visible generated items.
		/// </summary>
		protected override int TotalVisibleGeneratedItems
		{
			get 
			{
				if (base.IsItemsHost == false)
					return 0;

                return this.GetManager().CountOfItemsToBeArranged;
			}
		}

			#endregion //TotalVisibleGeneratedItems
    
		#endregion //Base Class Overrides

        #region Properties
        
            #region Public Attached Properties
	
		        #region Column

		/// <summary>
		/// Identifies the Column attached dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnProperty = DependencyProperty.RegisterAttached(
			"Column",
			typeof( int ),
			typeof( TilesPanelBase ),
			new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnConstraintPropertyChanged ) ),
			new ValidateValueCallback( ValidateColumnRow )
		);

		/// <summary>
        /// Gets the value of the Column attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
		/// </summary>
		/// <param name="elem">This element's Column value will be returned.</param>
		/// <returns>The value of the Column attached property. The default value is 0.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        [AttachedPropertyBrowsableForChildren()]
        public static int GetColumn(DependencyObject elem)
		{
			return (int)elem.GetValue( ColumnProperty );
		}

		/// <summary>
        /// Sets the value of the Column attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
		/// </summary>
		/// <param name="elem">This element's Column value will be set.</param>
		/// <param name="value">Value to set. This can be -1 which will position the element relative to previous element in the panel.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        public static void SetColumn(DependencyObject elem, int value)
		{
			elem.SetValue( ColumnProperty, value );
		}

		        #endregion // Column

		        #region ColumnSpan

		/// <summary>
		/// Identifies the ColumnSpan attached dependency property.
		/// </summary>
		public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.RegisterAttached(
			"ColumnSpan",
			typeof( int ),
			typeof( TilesPanelBase ),
			new FrameworkPropertyMetadata( 1, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnConstraintPropertyChanged ) ),
			new ValidateValueCallback( ValidateColumnRowSpan )
		);

        /// <summary>
        /// Gets the value of the ColumnSpan attached property of the specified element, 0 indicates
        /// that the element will occupy the remainder of the space in its logical column. The default is 1. 
        /// </summary>
        /// <param name="elem">This element's ColumnSpan value will be returned.</param>
        /// <returns>The value of the ColumnSpan attached property. The default value is 1.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        [AttachedPropertyBrowsableForChildren()]
        public static int GetColumnSpan(DependencyObject elem)
		{
			return (int)elem.GetValue( ColumnSpanProperty );
		}

        /// <summary>
        /// Sets the value of the ColumnSpan attached property of the specified element, 0 indicates
        /// that the element will occupy the remainder of the space in its logical column. The default is 1. 
        /// </summary>
		/// <param name="elem">This element's ColumnSpan value will be set.</param>
		/// <param name="value">Value to set. This can be 0 to indicate that the element should occupy the remainder of the logical column.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        public static void SetColumnSpan(DependencyObject elem, int value)
		{
			elem.SetValue( ColumnSpanProperty, value );
		}

		        #endregion // ColumnSpan

                #region ColumnWeight

        /// <summary>
        /// Identifies the ColumnWeight attached dependency property
        /// </summary>
        /// <seealso cref="GetColumnWeight"/>
        /// <seealso cref="SetColumnWeight"/>
        public static readonly DependencyProperty ColumnWeightProperty = DependencyProperty.RegisterAttached("ColumnWeight",
            typeof(float), typeof(TilesPanelBase), new FrameworkPropertyMetadata(0f, FrameworkPropertyMetadataOptions.None,
                new PropertyChangedCallback(OnConstraintPropertyChanged)),
                    new ValidateValueCallback(ValidateColumnRowWeight));

        /// <summary>
        /// Gets the value of the ColumnWeight attached property of the specified element. ColumnWeight specifies
        /// how any extra width will be distributed among elements.
        /// </summary>
        /// <returns>The value of the ColumnWeight attached property. The default vaue is 0.</returns>
        /// <seealso cref="ColumnWeightProperty"/>
        /// <seealso cref="SetColumnWeight"/>
        [AttachedPropertyBrowsableForChildren()]
        public static float GetColumnWeight(DependencyObject d)
        {
            return (float)d.GetValue(TilesPanelBase.ColumnWeightProperty);
        }

        /// <summary>
        /// Sets the value of the ColumnWeight attached property of the specified element. ColumnWeight specifies
        /// how any extra width will be distributed among elements.
        /// </summary>
        /// <returns>The value of the ColumnWeight attached property.</returns>
        /// <seealso cref="ColumnWeightProperty"/>
        /// <seealso cref="GetColumnWeight"/>
        public static void SetColumnWeight(DependencyObject d, float value)
        {
            d.SetValue(TilesPanelBase.ColumnWeightProperty, value);
        }

                #endregion //ColumnWeight

                #region IsDragging

        internal static readonly DependencyPropertyKey IsDraggingPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsDragging",
            typeof(bool), typeof(TilesPanelBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the IsDragging" attached readonly dependency property
        /// </summary>
        /// <seealso cref="GetIsDragging"/>
        public static readonly DependencyProperty IsDraggingProperty =
            IsDraggingPropertyKey.DependencyProperty;


        /// <summary>
        /// Returns whether this tile is currently being dragged (read-only)
        /// </summary>
        /// <seealso cref="IsDraggingProperty"/>
        [AttachedPropertyBrowsableForChildren()]
        public static bool GetIsDragging(DependencyObject d)
        {
            return (bool)d.GetValue(TilesPanelBase.IsDraggingProperty);
        }

                #endregion //IsDragging

                #region IsSwapTarget

        internal static readonly DependencyPropertyKey IsSwapTargetPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsSwapTarget",
            typeof(bool), typeof(TilesPanelBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the IsSwapTarget" attached readonly dependency property
        /// </summary>
        /// <seealso cref="GetIsSwapTarget"/>
        public static readonly DependencyProperty IsSwapTargetProperty =
            IsSwapTargetPropertyKey.DependencyProperty;


        /// <summary>
        /// Returns whether another tile is being dragged over this tile and if released will swap positions with this tile (read-only)
        /// </summary>
        /// <seealso cref="IsSwapTargetProperty"/>
        [AttachedPropertyBrowsableForChildren()]
        public static bool GetIsSwapTarget(DependencyObject d)
        {
            return (bool)d.GetValue(TilesPanelBase.IsSwapTargetProperty);
        }

                #endregion //IsSwapTarget

		        #region Row

		/// <summary>
		/// Identifies the Row attached dependency property.
		/// </summary>
		public static readonly DependencyProperty RowProperty = DependencyProperty.RegisterAttached(
			"Row",
			typeof( int ),
			typeof( TilesPanelBase ),
			new FrameworkPropertyMetadata( 0, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnConstraintPropertyChanged ) ),
			new ValidateValueCallback( ValidateColumnRow )
		);

		/// <summary>
        /// Gets the value of the Row attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0. 
		/// </summary>
		/// <param name="elem">This element's Row value will be returned.</param>
		/// <returns>The value of the Row attached property. The default value is 0.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        [AttachedPropertyBrowsableForChildren()]
		public static int GetRow( DependencyObject elem )
		{
			return (int)elem.GetValue( RowProperty );
		}

		/// <summary>
        /// Sets the value of the Row attached property of the specified element, -1 indicates that the tile will be positioned relative to the previous tile in the panel. The default value is 0.
		/// </summary>
		/// <param name="elem">This element's Row value will be set.</param>
		/// <param name="value">Value to set. This can be -1 which will position the element relative to previous element in the panel.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        public static void SetRow(DependencyObject elem, int value)
		{
			elem.SetValue( RowProperty, value );
		}

		        #endregion // Row

		        #region RowSpan

		/// <summary>
		/// Identifies the RowSpan attached dependency property.
		/// </summary>
		public static readonly DependencyProperty RowSpanProperty = DependencyProperty.RegisterAttached(
			"RowSpan",
			typeof( int ),
			typeof( TilesPanelBase ),
			new FrameworkPropertyMetadata( 1, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnConstraintPropertyChanged ) ),
			new ValidateValueCallback( ValidateColumnRowSpan )
		);

		/// <summary>
        /// Gets the value of the RowSpan attached property of the specified element, 0 indicates
        /// that the element will occupy the remainder of the space in its logical column. The default is 1. 
		/// </summary>
		/// <param name="elem">This element's RowSpan value will be returned.</param>
		/// <returns>The value of the RowSpan attached property. The default value is 1.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        [AttachedPropertyBrowsableForChildren()]
        public static int GetRowSpan(DependencyObject elem)
		{
			return (int)elem.GetValue( RowSpanProperty );
		}

		/// <summary>
        /// Sets the value of the RowSpan attached property of the specified element, 0 indicates
        /// that the element will occupy the remainder of the space in its logical column. The default is 1. 
		/// </summary>
		/// <param name="elem">This element's RowSpan value will be set.</param>
		/// <param name="value">Value to set. This can be 0 to indicate that the element should occupy the remainder of the logical row.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property is ignored unless <see cref="TileLayoutOrder"/> is 'UseExplicitRowColumnOnTile'.</para>
        /// </remarks>
        public static void SetRowSpan(DependencyObject elem, int value)
		{
			elem.SetValue( RowSpanProperty, value );
		}

		        #endregion // RowSpan

                #region RowWeight

        /// <summary>
        /// Identifies the RowWeight attached dependency property
        /// </summary>
        /// <seealso cref="GetRowWeight"/>
        /// <seealso cref="SetRowWeight"/>
        public static readonly DependencyProperty RowWeightProperty = DependencyProperty.RegisterAttached("RowWeight",
            typeof(float), typeof(TilesPanelBase), new FrameworkPropertyMetadata(0f, FrameworkPropertyMetadataOptions.None,
                new PropertyChangedCallback(OnConstraintPropertyChanged)),
                    new ValidateValueCallback(ValidateColumnRowWeight));

        /// <summary>
        /// Gets the value of the RowWeight attached property of the specified element. RowWeight specifies
        /// how any extra height will be distributed among elements.
        /// </summary>
        /// <returns>The value of the RowWeight attached property. The default value is 0.</returns>
        /// <seealso cref="RowWeightProperty"/>
        /// <seealso cref="SetRowWeight"/>
        [AttachedPropertyBrowsableForChildren()]
        public static float GetRowWeight(DependencyObject d)
        {
            return (float)d.GetValue(TilesPanelBase.RowWeightProperty);
        }

        /// <summary>
        /// Sets the value of the RowWeight attached property of the specified element. RowWeight specifies
        /// how any extra height will be distributed among elements.
        /// </summary>
        /// <returns>The value of the RowWeight attached property.</returns>
        /// <seealso cref="RowWeightProperty"/>
        /// <seealso cref="GetRowWeight"/>
        public static void SetRowWeight(DependencyObject d, float value)
        {
            d.SetValue(TilesPanelBase.RowWeightProperty, value);
        }

                #endregion //RowWeight

            #endregion //Public Attached Properties

            #region Public Properties

                #region IsAnimationInProgress

        /// <summary>
        /// Returns true if a reposition ot resize animation is in progress (read-only)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsAnimationInProgress
        {
            get
            {
                 return this._activeAnimationCount > 0;
            }
        }

                #endregion //IsAnimationInProgress

            #endregion //Public Properties
  
            #region Protected Properties

				#region ScrollPosition

		/// <summary>
		/// Returns/sets the current scroll position in the list of items.
		/// </summary>
		protected int ScrollPosition
		{
			get { return this.GetManager().ScrollPosition; }
			set { this.GetManager().ScrollPosition = value; }
		}

				#endregion //ScrollPosition

				#region ScrollIndexOfFirstArrangedItem

		/// <summary>
        /// Returns the zero-based index of the first item that is arranged (read-only)
        /// </summary>
        protected int ScrollIndexOfFirstArrangedItem { get { return this.GetManager().ScrollIndexOfFirstArrangedItem; } }

                #endregion //ScrollIndexOfFirstArrangedItem	
    
                #region ScrollIndexOfLastArrangedItem

        /// <summary>
        /// Returns the zero-based index of the last item that is arranged (read-only)
        /// </summary>
        protected int ScrollIndexOfLastArrangedItem { get { return this.GetManager().ScrollIndexOfLastArrangedItem; } }

                #endregion //ScrollIndexOfLastArrangedItem	

            #endregion //Protected Properties	

            #region Internal Properties
    
                #region LastMeasureSize

        internal Size LastMeasureSize { get { return this._lastMeasureSize; } }

                #endregion //LastMeasureSize	
    
                #region RepositionFactor

        internal static readonly DependencyProperty RepositionFactorProperty = DependencyProperty.Register("RepositionFactor",
            typeof(double), typeof(TilesPanelBase), new FrameworkPropertyMetadata(1.0d, new PropertyChangedCallback(OnRepositionFactorChanged)));

        private static void OnRepositionFactorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanelBase panel = target as TilesPanelBase;

            panel._repositionFactor = (double)e.NewValue;

            
            // Check the state of the clock and ignore the change if the clock isn't active
            if (panel._repositionClock != null &&
                panel._repositionClock.CurrentState != ClockState.Active)
                return;

            panel.OnFactorChanged();
        }

        internal double RepositionFactor
        {
            get
            {
                return this._repositionFactor;
            }
            set
            {
                this.SetValue(TilesPanelBase.RepositionFactorProperty, value);
            }
        }

                #endregion //RepositionFactor

                #region ResizeFactor

        internal static readonly DependencyProperty ResizeFactorProperty = DependencyProperty.Register("ResizeFactor",
            typeof(double), typeof(TilesPanelBase), new FrameworkPropertyMetadata(1.0d, new PropertyChangedCallback(OnResizeFactorChanged)));

        private static void OnResizeFactorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TilesPanelBase panel = target as TilesPanelBase;

            panel._resizeFactor = (double)e.NewValue;

            
            // Check the state of the clock and ignore the change if the clock isn't active
            if (panel._resizeClock != null &&
                panel._resizeClock.CurrentState != ClockState.Active)
                return;

            panel.OnFactorChanged();
        }

        internal double ResizeFactor
        {
            get
            {
                return this._resizeFactor;
            }
            set
            {
                this.SetValue(TilesPanelBase.ResizeFactorProperty, value);
            }
        }

                #endregion //ResizeFactor

		        #region ScrollDataInfo

		private ScrollData _scrollDataInfo;

		internal ScrollData ScrollDataInfo
		{
			get
			{
                if (this._scrollDataInfo == null)
                {
                    this._scrollDataInfo = new ScrollData(this);
                }

				return this._scrollDataInfo;
			}
		}
		        #endregion //ScrollDataInfo

                #region TileAreaSize

        internal Size TileAreaSize { get { return this._tileAreaSize; } }

                #endregion //TileAreaSize	

            #endregion //Internal Properties	

            #region Private Properties

                #region IsScrollClient

        private bool IsScrollClient
        {
            // We'll consider the panel to be in control of scrolling if its been provided a scroll owner.
            //
            get { return this._scrollDataInfo != null && this._scrollDataInfo._scrollOwner != null; }
        }

                #endregion //IsScrollClient

                #region MaxHorizontalOffset

        internal double MaxHorizontalOffset
        {
            get
            {
                return this.GetManager().ScrollMaxOffset.X;
            }
        }

                #endregion //MaxHorizontalOffset	
    
                #region MaxVerticalOffset

        internal double MaxVerticalOffset
        {
            get
            {
                return this.GetManager().ScrollMaxOffset.Y;
            }
        }

                #endregion //MaxVerticalOffset	

				#region ScrollBarOrientation

		private Orientation ScrollBarOrientation
		{
			get	
			{ 
				if (this.LogicalOrientation == Orientation.Horizontal)
					return Orientation.Vertical;
				else
					return Orientation.Horizontal;
			}
		}

				#endregion //ScrollBarOrientation	

            #endregion //Private Properties	
            
        #endregion //Properties

        #region Methods

            #region Public Methods

                #region GetInViewRect

        // JJD 2/25/10 - TFS28159 added
        /// <summary>
        /// Returns the area of the specified container that is in view or will be in view when the current animations end.
        /// </summary>
        /// <param name="container">The container in question.</param>
        /// <returns>A rect (in coordinates relative to the container) that represents the area of the container that will be in view once all animations have ended. If no part of the container will be in view then an empty rect is returned.</returns>
        public Rect GetInViewRect(DependencyObject container)
        {
            return this.GetManager().GetInViewRect(container);
        }

            #endregion //GetInViewRect	
    
            #endregion //Public Methods	
    
            #region Internal Methods

				#region FindSlotForNewlyRealizedRecordPresenter

		internal int FindSlotForNewlyRealizedRecordPresenter(int newItemIndex)
		{
			IList	children = this.ChildElements;
			int		count = children.Count;

			if (count == 0)
				return 0;

			IItemContainerGenerator generator = this.GetGenerator();
			for (int i = 0; i < count; i++)
			{
				int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

				if (itemIndex >= newItemIndex)
					return i;
			}

			return count;
		}

				#endregion //FindSlotForNewlyRealizedRecordPresenter	

				#region GetGenerator







		internal IItemContainerGenerator GetGenerator()
		{
			return this.ActiveItemContainerGenerator;
		}

				// JJD 5/22/07 - Optimization
				#region Obsolete code

				#endregion //Obsolete code	
    
				#endregion //GetGenerator	

				#region GetGeneratorPositionFromItemIndex

		internal GeneratorPosition GetGeneratorPositionFromItemIndex(int itemIndex, out int childIndex)
		{
			IItemContainerGenerator generator			= this.GetGenerator();
			GeneratorPosition		generatorPosition	= (generator != null) ? generator.GeneratorPositionFromIndex(itemIndex) :
																				new GeneratorPosition(-1, itemIndex + 1);

			childIndex = (generatorPosition.Offset == 0) ? generatorPosition.Index :
														   generatorPosition.Index + 1;

			return generatorPosition;
		}

				#endregion //GetGeneratorPositionFromItemIndex
 
            #endregion //Internal Methods

            #region Protected Methods

                #region GetActualMinimizedAreaExtent

        /// <summary>
        /// Gets the actual extent of the minimized area
        /// </summary>
        /// <returns>An value that represents the actual width of the minimized area in maximized mode when MaximizedTileLocation is 'Left' or 'Right'. When MaximizedTileLocation is 'Top' or 'Bottom', it represents the actual height.</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if not in maximizd mode then this method returns 0.</para>
        /// </remarks>
        /// <seealso cref="GetExplicitMinimizedAreaExtent"/>
        internal protected double GetActualMinimizedAreaExtent()
        {
            return this.GetManager().MinimizedAreaActualExtent;
        }

                #endregion //GetActualMinimizedAreaExtent	

				#region GetItemsInView

		/// <summary>
        /// Returns an array if <see cref="ItemInfoBase"/> objects representing all of the items that will be arranged in view.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this array represents the items that will be in view after pending animations have been completed. Also, in normal mode with a <see cref="TileLayoutOrder"/> of 'UseExplicitRowColumnOnTile' all items will be returned from this list, not just those in view, since in this mode all items are measured and arranged.</para>
        /// </remarks>
        /// <returns>An array of ItemInfoBase objects</returns>
        protected ItemInfoBase[] GetItemsInView()
        {
            return this.GetManager().GetItemsInView();
        }

                #endregion //GetItemsInView

				#region GetItemAtScrollIndex

		/// <summary>
        /// Returns the item at the specified scroll index
        /// </summary>
        /// <param name="scrollIndex">The zero-based index.</param>
        /// <returns>The item at the specified scroll index</returns>
        protected object GetItemAtScrollIndex(int scrollIndex)
        {
            return this.GetManager().GetItemAtScrollIndex(scrollIndex);
        }

                #endregion //GetItemAtScrollIndex

                #region GetMaximizedTileArea

        /// <summary>
        /// Returns the area of the panel that will include maximized tiles.
        /// </summary>
        /// <param name="clipToOverallTileArea">If true will clip the returned rect to the tile area, inside the TileAreaPadding.</param>
        /// <returns>A rect of the tile area that includes maximized tiles</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this rect is only reliable after a measure has been processed. Also, if the last measure wasn't processed while in maxmized mode this will return the entire tile area.</para>
        /// </remarks>
        internal protected Rect GetMaximizedTileArea(bool clipToOverallTileArea)
        {
            return this.GetManager().GetMaximizedModeTileArea(TileManager.TileArea.MaximizedTiles, clipToOverallTileArea);
        }

                #endregion //GetMaximizedTileArea	

                #region GetMaximizedTileAreaConstraints

        /// <summary>
        /// Returns the constraints for the maximized tile area
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this is only reliable after a measure has been processed.</para>
        /// </remarks>
        /// <returns>A constraints object for the maximized tile area or null if not in maximized mode.</returns>
        internal protected ITileConstraints GetMaximizedTileAreaConstraints()
        {
            return this.GetManager().GetMaximizedModeTileAreaConstraints(TileManager.TileArea.MaximizedTiles);
        }

                #endregion //GetMaximizedTileAreaConstraints	

                #region GetMinimizedTileArea

        /// <summary>
        /// Returns the area of the panel that will include minimized tiles.
        /// </summary>
        /// <param name="clipToOverallTileArea">If true will clip the returned rect to the tile area, inside the TileAreaPadding.</param>
        /// <returns>A rect of the tile area that includes minimized tiles</returns>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this rect is only reliable after a measure has been processed. Also, if the last measure wasn't processed while in maxmized mode this will return the entire tile area.</para>
        /// </remarks>
        internal protected Rect GetMinimizedTileArea(bool clipToOverallTileArea)
        {
            return this.GetManager().GetMaximizedModeTileArea(TileManager.TileArea.MinimizedTiles, clipToOverallTileArea);
        }

                #endregion //GetMinimizedTileArea	

                #region GetMinimizedTileAreaConstraints

        /// <summary>
        /// Returns the constraints for the minimized tile area
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this is only reliable after a measure has been processed.</para>
        /// </remarks>
        /// <returns>A constraints object for the minimized tile area or null if not in maximized mode.</returns>
        internal protected ITileConstraints GetMinimizedTileAreaConstraints()
        {
            return this.GetManager().GetMaximizedModeTileAreaConstraints(TileManager.TileArea.MinimizedTiles);
        }

                #endregion //GetMinimizedTileAreaConstraints	

				#region GetOffsetFromScrollPosition

		/// <summary>
		/// Returns the offset that corresponds to the specified scrollposition.
		/// </summary>
        /// <param name="scrollPosition">The specified scroll position</param>
		/// <returns></returns>
		protected double GetOffsetFromScrollPosition(int scrollPosition)
		{
			return this.GetManager().GetOffsetFromScrollPosition(scrollPosition);
		}

				#endregion //GetOffsetFromScrollPosition

				#region GetScrollPositionFromOffset

		/// <summary>
		/// Returns the scrollposition that corresponds to the specified offset.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		protected int GetScrollPositionFromOffset(double offset)
		{
			return this.GetManager().GetScrollPositionFromOffset(offset);
		}

				#endregion //GetScrollPositionFromOffset

                #region GetTargetRectOfItem

        /// <summary>
        /// Returns the final rect where this item will be arranged once animations have been completed 
        /// </summary>
        /// <param name="item">The item in question.</param>
        /// <returns>The target rect</returns>
        internal protected Rect GetTargetRectOfItem(object item)
        {
            TileManager tm = this.GetManager();

            TileManager.LayoutItem li = tm.GetLayoutItem(item);

            if (li == null ||
                li.Container == null ||
                !tm.IsContainerInArrangeCache(li.Container))
                return Rect.Empty;

            // Calculate an offset to use when arranging the items from the
            // tile area padding and any scroll offset
            Rect rect = li.TargetRect;

            Thickness tileAreaPadding = this.GetTileAreaPadding();

            Vector offset;
            ItemInfoBase info = li.ItemInfo;

            if (info != null && info.GetIsMaximized())
                offset = tm.ArrangeOffsetForMaxmizedTiles;
            else
                offset = tm.ArrangeOffset;

            // Calculate an offset to use when arranging the items from the
            // tile area padding and any scroll offset
            Vector arrangeOffset = new Vector(tileAreaPadding.Left, tileAreaPadding.Top) + offset;

            return Rect.Offset(rect, arrangeOffset);
        }

                #endregion //GetTargetRectOfItem

                #region GetTargetRectOfContainer

        /// <summary>
        /// Returns the final rect where this container will be arranged once animations have been completed 
        /// </summary>
        /// <param name="container">The elemment in question.</param>
        /// <returns>The target rect</returns>
        protected Rect GetTargetRectOfContainer(DependencyObject container)
        {
            TileManager tm = this.GetManager();

            object item = this.ItemFromContainer(container);

            if (item == null )
                return Rect.Empty;

            return this.GetTargetRectOfItem(item);
        }

                #endregion //GetTargetRectOfContainer

                #region GetTileArea

        /// <summary>
        /// Returns the area of the panel that will include tiles, (i.e inside the padding).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this rect is only reliable after a measure has been processed.</para>
        /// </remarks>
        /// <returns>A rect of the tile area</returns>
        internal protected Rect GetTileArea()
        {
            
            //Rect tileArea = new Rect( this.DesiredSize);
            // Initialize a rect with a size of the TileAreaSize that was
            // calculated during the last measure
            Rect tileArea = new Rect( this.TileAreaSize);
            
            Thickness padding = this.GetTileAreaPadding();

            // offset the rect based on the padding
            tileArea.Offset(padding.Left, padding.Top);

            
            
            

            return tileArea;
        }

                #endregion //GetTileArea	

                #region GetTotalColumnsDisplayed

        /// <summary>
        /// Returns the total number of columns that will be displayed once animations have been completed.
        /// </summary>
        /// <returns>The total number of columns that will be displayed once animations have completed</returns>
        protected int GetTotalColumnsDisplayed()
        {
            return this.GetManager().TotalColumnsDisplayed;
        }

                #endregion //GetTargetRectOfContainer

                #region GetTotalRowsDisplayed

        /// <summary>
        /// Returns the total number of rows that will be displayed once animations have been completed.
        /// </summary>
        /// <returns>The total number of rows that will be displayed once animations have completed</returns>
        protected int GetTotalRowsDisplayed()
        {
            return this.GetManager().TotalRowsDisplayed;
        }

                #endregion //GetTargetRectOfContainer

                #region IsContainerInArrangeCache

        /// <summary>
        /// Returns if a specific container is currently in the arrange cache
        /// </summary>
        /// <param name="container">The container in question</param>
        /// <returns>True if the container is being processed by the arrange logic</returns>
        protected bool IsContainerInArrangeCache(DependencyObject container)
        {
            if (this._dragManager != null &&
                 this._dragManager.Tile == container)
                return true;

            return this.GetManager().IsContainerInArrangeCache(container);
        }

                #endregion //IsContainerInArrangeCache

                #region MoveTile

        /// <summary>
        /// Moves a tile to another location.
        /// </summary>
        /// <param name="container">The tile to maove</param>
        /// <param name="item">The assocated item</param>
        /// <param name="point">The point relative to this panel.</param>
        /// <returns>True if the tile was successfully moved.</returns>
        /// <remarks>
        /// <para class="body">This will result in an equilalent action as if the user had dragged the tile to the speecidied point.</para>
        /// <para class="note">This method will return false if <see cref="AllowTileDragging"/> is not 'Swap' or 'Slide' or the specified point is not over another Tile that can be moved.</para>
        /// </remarks>
        internal protected bool MoveTile(FrameworkElement container, object item, Point point)
        {
            TileManager tm = this.GetManager();

            TileManager.LayoutItem sourceItem = tm.GetLayoutItem(item, container, false, false);

            if (sourceItem == null)
                return false;

            TileManager.LayoutItem targetItem = tm.GetLayoutItemFromPoint(point, sourceItem);

            if (targetItem == null)
                return false;

            return tm.MoveTileHelper(sourceItem, targetItem, false);
        }

                #endregion //MoveTile	
    
                #region StartTileDrag

        /// <summary>
        /// Called to start a drag operation on a tile
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="e">Mouse event args</param>
        /// <returns>True if the drag operation was started.</returns>
        protected bool StartTileDrag(FrameworkElement container, object item, MouseEventArgs e)
        {
            TileManager tm = this.GetManager();

            if (tm == null)
                return false;

            ItemInfoBase info = tm.GetItemInfo(item);

            if (info == null)
                return false;

            AllowTileDragging allowTileDragging = this.GetAllowTileDragging(container, info);

            if (allowTileDragging == AllowTileDragging.No)
                return false;

            if (allowTileDragging == AllowTileDragging.Slide)
            {
                if (this.GetIsInMaximizedMode() == false &&
                    this.GetTileLayoutOrder() == TileLayoutOrder.UseExplicitRowColumnOnTile)
                    allowTileDragging = AllowTileDragging.Swap;
            }

            this.CaptureMouse();

            TileManager.LayoutItem li = tm.GetLayoutItem(item, container, true, false);

            this._dragManager = new TileDragManager(this, container, info, li , container, allowTileDragging);
            this._dragManager.StartDrag(e);

            // arrange the container out of view
            if (li != null)
            {
                if ( !container.IsMeasureValid )
                    container.Measure(li.TargetRect.Size);

                container.Arrange(new Rect(new Point(-100000, -100000), li.TargetRect.Size));
            }
            else
            {
                container.Arrange(new Rect(new Point(-100000, -100000), container.DesiredSize));
            }

            container.SetValue(IsDraggingPropertyKey, KnownBoxes.TrueBox);

            this.OnTileDragStart(container);

            return true;
        }

                #endregion //StartTileDrag	
    
            #endregion //Protected Methods	
    
            #region Protected Virtual Methods
            
                #region CanSwapContainers

        /// <summary>
        /// Returns true if the containers can be swapped
        /// </summary>
        /// <param name="dragItemContainer">The container of the item to be dragged.</param>
        /// <param name="dragItemInfo">The associated drag item's info</param>
        /// <param name="swapTargetContainer">The container that is the targetof the swap.</param>
        /// <param name="swapTargetItemInfo">he associated swap target item's info</param>
        /// <returns>The default implementaion returns true if both are maximizd or both are not maximized.</returns>
        internal protected virtual bool CanSwapContainers(FrameworkElement dragItemContainer, ItemInfoBase dragItemInfo, FrameworkElement swapTargetContainer, ItemInfoBase swapTargetItemInfo)
        {
            return dragItemInfo.GetIsMaximized() == swapTargetItemInfo.GetIsMaximized();
        }

                #endregion //CanSwapContainers	
            
                #region GetAllowTileDragging

        /// <summary>
        /// Returns whether this item can be dragged
        /// </summary>
        /// <param name="container">The container of the item to be dragged.</param>
        /// <param name="itemInfo">The associated item's info</param>
        /// <returns>The default implementaion returns 'No'.</returns>
        internal protected virtual AllowTileDragging GetAllowTileDragging(FrameworkElement container, ItemInfoBase itemInfo)
        {
            return AllowTileDragging.No;
        }

                #endregion //GetAllowTileDragging	
            
                #region GetCanDropContainer

        /// <summary>
        /// Returns whether this container can be dropped at this location.
        /// </summary>
        /// <param name="container">The container to be dropped.</param>
        /// <param name="itemInfo">The associated item's info</param>
        /// <param name="newState">The new state of the item if the container is dropped here.</param>
        /// <param name="newLogicalIndex">The new logical index of the item if the container is dropped here.</param>
        /// <returns>The default implementaion return true.</returns>
        internal protected virtual bool GetCanDropContainer(FrameworkElement container, ItemInfoBase itemInfo, TileState newState, int newLogicalIndex)
        {
            return true;
        }

                #endregion //GetCanDropContainer	
 
                #region GetContainerConstraints

        /// <summary>
        /// Gets any explicit constraints for a container
        /// </summary>
        /// <param name="container">The container in question.</param>
        /// <param name="state">The current state of the container.</param>
        /// <returns>A <see cref="ITileConstraints"/> object or null.</returns>
        internal protected virtual ITileConstraints GetContainerConstraints(DependencyObject container, TileState state)
        {
            return null;
        }

                #endregion //GetContainerConstraints	
            
                #region GetContainerState

        /// <summary>
        /// Gets the state of a container
        /// </summary>
        /// <param name="container">The container in question.</param>
        /// <returns>A <see cref="TileState"/> enumeration. The default is 'Normal'.</returns>
        internal protected virtual TileState GetContainerState(DependencyObject container)
        {
            return TileState.Normal;
        }

                #endregion //GetContainerState	
 
                #region GetDefaultConstraints

        /// <summary>
        /// Gets the default constraints for a specific state
        /// </summary>
        /// <param name="state">The state in question.</param>
        /// <returns>A <see cref="ITileConstraints"/> object or null.</returns>
        internal protected virtual ITileConstraints GetDefaultConstraints(TileState state)
        {
            return null;
        }

                #endregion //GetDefaultConstraints	
 
                #region GetDefaultMinimumItemSize

        /// <summary>
        /// Gets the default minimium size for items.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this default value is used if the constraints are left set to 0.</para>
        /// </remarks>
        /// <returns>The default minimum size for items.</returns>
        internal protected virtual Size GetDefaultMinimumItemSize()
        {
            return new Size(16,12);
        }

                #endregion //GetDefaultMinimumItemSize	
    
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
		internal protected virtual ExplicitLayoutTileSizeBehavior GetExplicitLayoutTileSizeBehavior()
        {
			return ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAndHeights;
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
        /// <seealso cref="GetActualMinimizedAreaExtent"/>
        internal protected virtual double GetExplicitMinimizedAreaExtent()
        {
            return 0;
        }

                #endregion //GetExplicitMinimizedAreaExtent	
    
                #region GetHorizontalTileAreaAlignment

        /// <summary>
        /// Determines the horizontal alignment of the complete block of visible tiles within the control.
        /// </summary>
        internal protected virtual HorizontalAlignment GetHorizontalTileAreaAlignment()
        {
            return HorizontalAlignment.Left;
        }

                #endregion //GetHorizontalTileAreaAlignment	
    
                #region GetInterTileAreaSpacing

        /// <summary>
        /// Determines the amount of spacing between the maximized tile area and the miminized tile area when in maximized mode.
        /// </summary>
        internal protected virtual double GetInterTileAreaSpacing()
        {
            return 0;
        }

                #endregion //GetInterTileAreaSpacing	
    
                #region GetInterTileSpacing

        /// <summary>
        /// Gets the amount of spacing between tiles in a specific state.
        /// </summary>
        /// <param name="vertical">True for vertical spacing, false for horzontal spacing.</param>
        /// <param name="state">The state of the tiles.</param>
        internal protected virtual double GetInterTileSpacing(bool vertical, TileState state)
        {
            return 0;
        }

                #endregion //GetInterTileSpacing	
    
                #region GetIsInDeferredScrollingMode

        /// <summary>
        /// Returns true if scrolling is deferred until the scroll thumb is released.
        /// </summary>
		internal protected virtual bool GetIsInDeferredScrollingMode()
        {
            return false;
		}

				#endregion //GetIsInDeferredScrollingMode

				#region GetIsInMaximizedMode

		/// <summary>
        /// Returns true if there is at least one tile whose <see cref="TileState"/> is 'Maximized'.
        /// </summary>
        internal protected virtual bool GetIsInMaximizedMode()
        {
            return false;
        }

                #endregion //GetIsInMaximizedMode	
    
                #region GetManager

        /// <summary>
        /// Gets the associated <see cref="TileManager"/>
        /// </summary>
        /// <returns>Must return a TileManager object</returns>
        internal protected virtual TileManager GetManager()
        {
            if (this._defaultManager == null)
            {
                this._defaultManager = new TileManager();

                this._defaultManager.InitializePanel(this);
            }

            return this._defaultManager;
        }

                #endregion //GetManager	
    
                #region GetMaximizedItems

        /// <summary>
        /// Returns a read-only collection of the items that are maximized.
        /// </summary>
        internal protected virtual ObservableCollectionExtended<object> GetMaximizedItems()
        {
            // if not overriden then return a dummy empy collection that is read-only
            if (this._defaultMaximizedItems == null)
                this._defaultMaximizedItems = new ObservableCollectionExtended<object>();

            return this._defaultMaximizedItems;
        }

                #endregion //GetMaximizedItems	
    
                #region GetMaximizedItemLimit

        /// <summary>
        /// Returns the maximimum number of items allowed in the collection returned from <see cref="GetMaximizedItems()"/>
        /// </summary>
        internal protected virtual int GetMaximizedItemLimit()
        {
            return 0;
        }

                #endregion //GetMaximizedItemLimit	
    
                #region GetMaximizedTileLocation

        /// <summary>
        /// Gets where the maximized tiles will be arranged relative to the minimized tiles.
        /// </summary>
        internal protected virtual MaximizedTileLocation GetMaximizedTileLocation()
        {
            return MaximizedTileLocation.Left;
        }

                #endregion //GetMaximizedTileLocation	
    
                #region GetMaximizedTileLayoutOrder

        /// <summary>
        /// Gets how multiple maximized tiles are laid out relative to one another
        /// </summary>
        internal protected virtual MaximizedTileLayoutOrder GetMaximizedTileLayoutOrder()
        {
            return MaximizedTileLayoutOrder.VerticalWithLastTileFill;
        }

                #endregion //GetMaximizedTileLayoutOrder	
    
                #region GetMin/Max/Columns/Rows

        /// <summary>
        /// Gets the maximum number of colums to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected virtual int GetMaxColumns()
        {
            return 0;
        }

        /// <summary>
        /// Gets the maximum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected virtual int GetMaxRows()
        {
            return 0;
        }

        /// <summary>
        /// Gets the minimum number of colums to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected virtual int GetMinColumns()
        {
            return 1;
        }

        /// <summary>
        /// Gets the minimum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        internal protected virtual int GetMinRows()
        {
            return 1;
        }

                #endregion //GetMin/Max/Columns/Rows	
    
                #region GetRepositionAnimation

        /// <summary>
        /// Determines how a tile> animates from one location to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="GetShouldAnimate()"/> returns 'False'.</para>
        /// </remarks>
        protected abstract DoubleAnimationBase GetRepositionAnimation();

                #endregion //GetRepositionAnimation	
    
                #region GetResizeAnimation

        /// <summary>
        /// Determines how a tile> animates from one size to another.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this property will be ignored if <see cref="GetShouldAnimate()"/> returns 'False'.</para>
        /// </remarks>
		internal protected virtual DoubleAnimationBase GetResizeAnimation()
		{
			return null;
		}

                #endregion //GetResizeAnimation	
    
                #region GetShouldAnimate

        /// <summary>
        /// Gets/sets whether tiles will animate to their new position and size
        /// </summary>
        internal protected abstract bool GetShouldAnimate();

                #endregion //GetShouldAnimate	

				#region GetShowAllTiles

		/// <summary>
        /// Gets whether all tiles should be arranged in view
        /// </summary>
        internal protected virtual bool GetShowAllTiles()
        {
            return false;
        }

                #endregion //GetShowAllTiles	
    
                #region GetShowAllMinimizedTiles

        /// <summary>
        /// Gets whether all minimized tiles should be arranged in view
        /// </summary>
        internal protected virtual bool GetShowAllMinimizedTiles()
        {
            return false;
        }

                #endregion //GetShowAllMinimizedTiles	
    
				#region GetSupportsDeferredScrolling

		/// <summary>
		/// Returns a boolean indicating whether scrolling in the specified orientation can be deferred.
		/// </summary>
		/// <param name="scrollBarOrientation">Orientation of the scrollbar whose thumb is being dragged.</param>
		/// <param name="scrollViewer">ScrollViewer whose scroll thumb is being dragged.</param>
		/// <returns>Returns true if the panel could support deferred scrolling in the specified orientation.</returns>
		internal protected virtual bool GetSupportsDeferredScrolling(Orientation scrollBarOrientation, ScrollViewer scrollViewer)
        {
            return false;
		}

				#endregion //GetSupportsDeferredScrolling

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
        virtual public Size? GetSynchronizedSize()
        {
            return null; 
        }

                #endregion //GetSynchronizedSize	

				#region GetTileAreaPadding

		/// <summary>
        /// Get the amount of space between the panel and the area where the tiles are arranged.
        /// </summary>
        internal protected virtual Thickness GetTileAreaPadding()
        {
            return new Thickness();
        }

                #endregion //GetTileAreaPadding	
    
                #region GetTileLayoutOrder

        /// <summary>
        /// Determines how the panel will layout the tiles.
        /// </summary>
        internal protected virtual TileLayoutOrder GetTileLayoutOrder()
        {
            return TileLayoutOrder.Vertical;
        }

                #endregion //GetTileLayoutOrder	
    
                #region GetVerticalTileAreaAlignment

        /// <summary>
        /// Determines the vertical alignment of the complete block of visible tiles within the control.
        /// </summary>
        internal protected virtual VerticalAlignment GetVerticalTileAreaAlignment()
        {
            return VerticalAlignment.Top;
        }

                #endregion //GetVerticalTileAreaAlignment	

                #region OnAnimationEnded

        /// <summary>
        /// Called when animations have completed on a specific container
        /// </summary>
        /// <param name="container"></param>
        protected virtual void OnAnimationEnded(DependencyObject container) { }

                #endregion //OnAnimationEnded	
            
                #region OnContainersSwapped

        /// <summary>
        /// Called when a container dropped on another to swap it.
        /// </summary>
        /// <param name="dragItemContainer">The container of the item that was dragged.</param>
        /// <param name="dragItemInfo">The associated drag item's info</param>
        /// <param name="swapTargetContainer">The container that is the target of the swap.</param>
        /// <param name="swapTargetItemInfo">The associated swap target item's info</param>
        internal protected virtual void OnContainersSwapped(FrameworkElement dragItemContainer, ItemInfoBase dragItemInfo, FrameworkElement swapTargetContainer, ItemInfoBase swapTargetItemInfo)
        {
        }

                #endregion //OnContainersSwapped	
    
                #region OnDeferredScrollingStarted

        /// <summary>
		/// Called when the user has initiated a scrolling operation by dragging the scroll thumb.
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> This method will only be called if <see cref="GetIsInDeferredScrollingMode"/> returns true.</para>
		/// <para></para>
		/// This is a convenient place for derived classes to create a ToolTip to use as a scroll tip.  The <see cref="OnDeferredScrollingEnded"/> method will be called when the user stops dragging the scroll thumb.
		/// </remarks>
		/// <seealso cref="GetIsInDeferredScrollingMode"/> cref=""/>
		/// <seealso cref="OnDeferredScrollingEnded"/> cref=""/>
		internal protected virtual void OnDeferredScrollingStarted(Thumb thumb, Orientation scrollBarOrientation)
        {
		}

				#endregion //OnDeferredScrollingStarted
    
                #region OnDeferredScrollingEnded

        /// <summary>
		/// Called when the user has completed a scroll thumb drag operation by releasing the mouse.
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> This method will only be called if <see cref="GetIsInDeferredScrollingMode"/> returns true.</para>
		/// <para></para>
		/// This is a convenient place for derived classes to cleanup a ToolTip previously created in OnDeferredScrollingStarted.
		/// </remarks>
		/// <seealso cref="GetIsInDeferredScrollingMode"/> cref=""/>
		/// <seealso cref="OnDeferredScrollingStarted"/> cref=""/>
		internal protected virtual void OnDeferredScrollingEnded(bool cancelled)
        {
		}

				#endregion //OnDeferredScrollingEnded
    
                #region OnDeferredScrollOffsetChanged

		/// <summary>
		/// Called when the scroll position changes while we are in deferred drag mode.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> This method will only be called if <see cref="GetIsInDeferredScrollingMode"/> returns true.</para>
		/// </remarks>
		/// <seealso cref="GetIsInDeferredScrollingMode"/> cref=""/>
		/// <seealso cref="OnDeferredScrollingStarted"/> cref=""/>
		/// <seealso cref="OnDeferredScrollingEnded"/> cref=""/>
		internal protected virtual void OnDeferredScrollOffsetChanged(double newDeferredScrollPosition)
        {
		}

				#endregion //OnDeferredScrollOffsetChanged

                #region OnItemsInViewChanged

        
        /// <summary>
        /// Called when animations have completed after the items in view have changed
        /// </summary>
        protected virtual void OnItemsInViewChanged() { }

                #endregion //OnItemsInViewChanged	

                #region OnTileDragStart

        /// <summary>
        /// Called when a drag operation has started
        /// </summary>
        /// <param name="container"></param>
        protected virtual void OnTileDragStart(FrameworkElement container)
        {

        }

                #endregion //OnTileDragStart	
            
                #region OnTileDragEnd

        /// <summary>
        /// Called when a drag operation has ended
        /// </summary>
        /// <param name="container"></param>
        protected virtual void OnTileDragEnd(FrameworkElement container)
        {

        }

                #endregion //OnTileDragEnd	
    
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
        
        protected virtual void UpdateTransform(DependencyObject container, Rect originalRect, Rect currentRect, Rect targetRect, Vector offset, double resizeFactor, bool calledFromArrange)
        {
            UIElement element = container as UIElement;

            if (element != null)
                element.Arrange(Rect.Offset(currentRect, offset));
        }

                #endregion //UpdateTransform	
    
                #region VerifyMaximizedItems

        /// <summary>
        /// Called to make sure the list returned from <see cref="GetMaximizedItems()"/> is in synch with the items collection.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note: </b>Derived classes that support maximized tiles should override this method and ensure that the MaximizedItems collection is in synch.</para>
        /// </remarks>
        internal protected virtual void VerifyMaximizedItems() { }

                #endregion //VerifyMaximizedItems	
     
            #endregion //Protected Virtual Methods

            #region Private Methods

                #region AdjustHorizontalOffset

        private void AdjustHorizontalOffset(double adjustment)
        {
            this.SetHorizontalOffset(((IScrollInfo)this).HorizontalOffset + adjustment);
        }
                #endregion // AdjustHorizontalOffset

                #region AdjustVerticalOffset

        private void AdjustVerticalOffset(double adjustment)
        {
            this.SetVerticalOffset(((IScrollInfo)this).VerticalOffset + adjustment);
        }
                #endregion // AdjustVerticalOffset
			
                #region ArrangeHelper

		private bool ArrangeHelper(bool calledFromArrange, bool animationsEnded, bool bypassAnimations)
		{
            // JJD 2/24/10 - TFS27654
            // If we aren't called from arrange and arrange is invalid then wait
            // for the next arrange call
            if ( calledFromArrange == false &&
                this.IsArrangeValid == false)
                return false;

            Thickness tileAreaPadding = this.GetTileAreaPadding();

            TileManager manager = this.GetManager();

            // Calculate an offset to use when arranging the items from the
            // tile area padding and any scroll offset
            Vector arrangeOffset = new Vector(tileAreaPadding.Left, tileAreaPadding.Top) + manager.ArrangeOffset;

            // Calculate an offset to use when arranging the maximized items from the
            // tile area padding and any scroll offset
            Vector arrangeOffsetMaximized = new Vector(tileAreaPadding.Left, tileAreaPadding.Top) + manager.ArrangeOffsetForMaxmizedTiles;
            
            Rect panelRect = new Rect(this._lastArrangeSize);

            panelRect.Offset(arrangeOffset);
            panelRect.Width = Math.Max(panelRect.Width - (tileAreaPadding.Left + tileAreaPadding.Right), 0);
            panelRect.Height = Math.Max(panelRect.Height - (tileAreaPadding.Top + tileAreaPadding.Bottom), 0);

            panelRect.Width = Math.Max(panelRect.Width, this._tileAreaSize.Width);
            panelRect.Height = Math.Max(panelRect.Height, this._tileAreaSize.Height);
            
            Vector tileRemoveOffset = new Vector(-100000, -100000);
            
            double repositionFactor = this.RepositionFactor;
            double resizeFactor = this.ResizeFactor;

            if (Utilities.AreClose(repositionFactor, 1.0d))
                repositionFactor = 1.0;

            if (Utilities.AreClose(resizeFactor, 1.0d))
                resizeFactor = 1.0;

            //Debug.WriteLine("Reposition factor: " + repositionFactor);
            //Debug.WriteLine("Resize factor: " + resizeFactor);

            IEnumerator enumerator = this.GetManager().GetItemsToBeArranged();

            Stack<TileManager.LayoutItem> itemsToRemove = null;

            FrameworkElement containerBeingDragged = this._dragManager != null ? this._dragManager.Tile : null;

            bool shouldAnimate = bypassAnimations == false && this.GetShouldAnimate();

            bool removeTilesNotInView = animationsEnded || shouldAnimate == false;

            if (removeTilesNotInView == true)
            {
                if (this.GetIsInMaximizedMode() )
                {
                    // In maximized mode if we are showing all minimized tiles then
                    // we never want to remove an item, even if is is out of view 
                    // which can only happen based on min size restictions.
                    if (this.GetShowAllMinimizedTiles())
                        removeTilesNotInView = false;
                }
                else
                {
                    // In normal mode if we are showing all tiles or the layoutorder is explicit 
                    // then we never want to remove an item, even if is is out of view
                    if (this.GetShowAllTiles() ||
                        this.GetTileLayoutOrder() == TileLayoutOrder.UseExplicitRowColumnOnTile)
                        removeTilesNotInView = false;
                }

            }

            bool isAnimatingSizeChange = false;
            bool requiresAnimation = false;

            List<TileManager.LayoutItem> layoutItems = new List<TileManager.LayoutItem>();

            // JJD 2/12/10 - TFS26775
            // Copy the items into an array since it is possible for UpdateLayout to be called
            // from inside the process loop below which would invalidate the enumerator
            while (enumerator.MoveNext())
            {
                TileManager.LayoutItem li = enumerator.Current as TileManager.LayoutItem;

                if (li != null)
                    layoutItems.Add(li);
            }

            // JJD 2/13/09
            // Set a flag so we bypass child size change notifications during the arrange loop
            bool holdIgnoreFlag = this._ignoreChildSizeChanges;
            if ( holdIgnoreFlag == false )
                this._childSizeChanged = false;

            this._ignoreChildSizeChanges = true;

            try
            {
                int count = layoutItems.Count;

                for (int i = 0; i < count; i++)
                {
                    TileManager.LayoutItem li = layoutItems[i];

                    if (li != null)
                    {
                        UIElement element = li.Container as UIElement;

                        // bypass a container that is being dragged since it was positioned out
                        // of sight at the start of the drag operation
                        if (element == containerBeingDragged)
                            continue;

                        Size targetSize = li.TargetRect.Size;

                        if (element.Visibility == Visibility.Collapsed ||
                            targetSize.Width < 1 || targetSize.Height < 1)
                        {
                            
                            // If the element is not visible we don't need to measure and arrange it
                            if (element.Visibility != Visibility.Collapsed)
                            {
                                li.CurrentRect = li.TargetRect;

								if (!element.IsMeasureValid)
								{
									// JJD 12/20/10 - TFS61057/TFS61064
									// Use the panel's available size when measuring instead of infinity
									//element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
									element.Measure(this._lastMeasureSize);
								}

                                element.Arrange(new Rect(new Point(tileRemoveOffset.X, tileRemoveOffset.Y), element.DesiredSize));
                            }

                            if (itemsToRemove == null)
                                itemsToRemove = new Stack<TileManager.LayoutItem>();

                            itemsToRemove.Push(li);

                            continue;
                        }

                        // let the tile now to clean up any cached images
						// JJD 4/22/11 - TFS63921
						// Use the animationsEnded or shouldAnimate flags instead of removeTilesNotInView
						// since that flag can be set to false if the all tiles in view option is set
                        //if (removeTilesNotInView)
						if (animationsEnded || shouldAnimate == false)
                            this.OnAnimationEnded(element);

                        // note: we can only animate tiles
                        if (shouldAnimate && element != null)
                        {
                            bool isCurrentSameAsTarget = li.IsCurrentSameAsTarget;

                            if (requiresAnimation == false)
                                requiresAnimation = !isCurrentSameAsTarget;

                            if (calledFromArrange == false &&
                                isCurrentSameAsTarget == false)
                            {
                                // Access the current rect first because in certain situations
                                // it will initialize the OriginalRect, e.g. wehen a new tile
                                // is entering the firs time
                                Rect currentRect = li.CurrentRect;
                                Rect originalRect = li.OriginalRect;
                                Rect targetRect = li.TargetRect;

                                currentRect = originalRect;

                                currentRect.X = ApplyFactor(originalRect.X, targetRect.X, repositionFactor);
                                currentRect.Y = ApplyFactor(originalRect.Y, targetRect.Y, repositionFactor);

                                if (li.ShouldAnimateResize)
                                {
                                    if (isAnimatingSizeChange == false)
                                        isAnimatingSizeChange = true;

                                    currentRect.Width = Math.Max(ApplyFactor(originalRect.Width, targetRect.Width, resizeFactor), 1);
                                    currentRect.Height = Math.Max(ApplyFactor(originalRect.Height, targetRect.Height, resizeFactor), 1);
                                }
                                else
                                    currentRect.Size = targetRect.Size;

                                li.CurrentRect = currentRect;
                            }
                        }
                        else
                            li.CurrentRect = li.TargetRect;

                        Rect itemRect = Rect.Offset(li.CurrentRect, arrangeOffset);
                        ItemInfoBase info = li.ItemInfo;

                        bool isMaximized = info != null && info.GetIsMaximized();

                        Vector offset = isMaximized ? arrangeOffsetMaximized : arrangeOffset;

                        bool removeTile = false;

                        if (removeTilesNotInView)
                        {
                            if (!isMaximized)
                            {
                                // see if the item rect intersects with the panel
                                
                                // remove the tile if the itemRect and target rect don't intersect
                                
                                removeTile = !panelRect.IntersectsWith(itemRect) &&
                                             !panelRect.IntersectsWith(Rect.Offset(li.TargetRect, arrangeOffset));
                            }
                        }

                        // keep track of items that are no longer inside the panel's rect
                        // so we can remove them from the cache below
                        if (element == null || removeTile)
                        {
                            if (itemsToRemove == null)
                                itemsToRemove = new Stack<TileManager.LayoutItem>();

                            itemsToRemove.Push(li);

                            offset = tileRemoveOffset;

                            itemRect = li.TargetRect;

                            itemRect = Rect.Offset(itemRect, offset);

                            li.CurrentRect = li.TargetRect;
                        }

                        if (element != null)
                        {
                            // we always arrange tiles at 0,0 and then use 
                            // the Tile's RenderTransform to position and size it properly
                            if (calledFromArrange || element.IsArrangeValid == false)
                                element.Arrange(new Rect(li.TargetRect.Size));

                            // JJD 2/24/10
                            // On the arrange pass initialize the original rect since we will
                            // always restart the animations is necessary
                            if (calledFromArrange)
                                li.SetOriginalRect(li.CurrentRect, true);

                            
                            
                            this.UpdateTransform(element, li.OriginalRect, li.CurrentRect, li.TargetRect, offset, resizeFactor, calledFromArrange);
                        }
                    }
                }

                if (itemsToRemove != null)
                {
                    // remove old items from the manager's cache
                    while (itemsToRemove.Count > 0)
                        manager.RemoveLayoutItem(itemsToRemove.Pop());
                }
            }
            finally
            {
                // JJD 2/13/09
                // Restore the bypass flag
                this._ignoreChildSizeChanges = holdIgnoreFlag;
                
                // JJD 2/13/09
                // If the flag was false at the start and their was a child element whose size had
                // changed the clear the _childSizeChanged flag and invalidate our measure
                if (holdIgnoreFlag == false && this._childSizeChanged == true)
                {
                    this._childSizeChanged = false;
                    this.InvalidateMeasure();
                }
            }

            return requiresAnimation;
		}

			    #endregion //ArrangeHelper	
        
                #region AnimateDoubleProperty

        private AnimationClock AnimateDoubleProperty(DependencyProperty dp, DoubleAnimationBase animation)
        {
            AnimationClock clock = animation.CreateClock();

            this._activeAnimationCount++;
            clock.Completed += new EventHandler(OnAnimationCompleted);

            this.ApplyAnimationClock(dp, clock);

            clock.Controller.Begin();

            this.SetValue(dp, 0.0d);

            return clock;
        }

                #endregion //AnimateDoubleProperty	
    
                #region AnimateRepositionAndResize

        private void AnimateRepositionAndResize()
        {
            
            

            if (!this.GetShouldAnimate())
                return;

            if (this._repositionClock != null)
            {
                this._repositionClock.Controller.Remove();
                this.UnhookAnimation(this._repositionClock);
            }

            if (this._resizeClock != null)
            {
                this._resizeClock.Controller.Remove();
                this.UnhookAnimation(this._resizeClock);
            }

            DoubleAnimationBase animation = this.GetRepositionAnimation();

            if (animation != null)
                this._repositionClock = this.AnimateDoubleProperty(TilesPanelBase.RepositionFactorProperty, animation);

            animation = this.GetResizeAnimation();

            if (animation != null)
                this._resizeClock = this.AnimateDoubleProperty(TilesPanelBase.ResizeFactorProperty, animation);

            Debug.Assert(this._activeAnimationCount < 3, "The animation count should never be > 2");
        }

                #endregion //AnimateRepositionAndResize	

                #region ApplyFactor

        private static double ApplyFactor(double original, double target, double factor)
        {
            if (factor == 1)
                return target;

            return original + ((target - original) * factor);

        }
                #endregion //ApplyFactor	

                #region OnAnimationCompleted

        private void OnAnimationCompleted(object sender, EventArgs e)
        {
            this.UnhookAnimation(sender as AnimationClock);
        }

                #endregion //OnAnimationCompleted	

                #region OnArrangeComplete

        
        private void OnArrangeComplete()
        {
            if (this._inViewItemsHaveChanged)
            {
                this._inViewItemsHaveChanged = false;

                this.OnItemsInViewChanged();

            }

            
            // Trigger a delayed cleanup
            this.TriggerCleanupOfUnusedGeneratedItems(true);

        }

                #endregion //OnArrangeComplete	
        
                #region OnCollectionChanged

        private void OnCollectionChanged(object sender, EventArgs e)
        {
            // If we are hokked up to an items control we can ignore this notification
            // since we are overriding its OnItemsChanged method
            if (this.IsItemsHost || !this.IsInitialized)
                return;

            this.GetManager().OnItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

                #endregion //OnCollectionChanged	
    
                #region OnConstraintPropertyChanged

        private static void OnConstraintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Visual v = d as Visual;
            TilesPanelBase panel = null != v ? VisualTreeHelper.GetParent(v) as TilesPanelBase : null;
            if (null != panel)
                panel.InvalidateMeasure();
        }

                #endregion // OnConstraintPropertyChanged

                #region OnFactorChanged

        private DispatcherOperation _processFactorChangePending;

        private void OnFactorChanged()
        {
            if (this._repositionFactor < .1 && this._resizeFactor < .1)
                this.ProcessFactorChange();
            else
            {
                if (this._processFactorChangePending == null)
                {
                    this._processFactorChangePending = this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Utilities.MethodDelegate(this.ProcessFactorChange));
                }
            }
        }

                #endregion //OnFactorChanged	

                #region ProcessFactorChange

        private void ProcessFactorChange()
        {
            this._processFactorChangePending = null;

            bool requiresAnimation = this.ArrangeHelper(false, false, false);
        }

                #endregion //ProcessFactorChanged	

                #region ProcessAllAnimationsEnded

        private void ProcessAllAnimationsEnded()
        {
            
            // If we are re-starting the animations inside ArrangeOverride then bypass the folllowing logic
            if (!this._isRestartingAnimations)
            {
                
                
                this.ArrangeHelper(false, true, false);

                
                // Call OnArrangeComplete
                this.OnArrangeComplete();

                
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            }
        }

                #endregion //ProcessAllAnimationsEnded	

                
                #region ProcessAnimationRestart

        private void ProcessAnimationRestart()
        {
            this._asynchAnimationStart = null;

            
            // Keep a flag that lets us know we are re-starting th animations
            if (this.IsAnimationInProgress)
                this._isRestartingAnimations = true;

            try
            {
                this.AnimateRepositionAndResize();
            }
            finally
            {
                
                // reset the flag
                this._isRestartingAnimations = false;
            }
        }

                #endregion //ProcessAnimationRestart	
        
		        #region SetHorizontalOffset

		private void SetHorizontalOffset(double offset)
		{
			if (this.IsScrollClient)
			{
                TileManager manager = this.GetManager();

                // if we are scrolling tiles in this dimension then round to offset 
                if (manager.ScrollTilesVertically)
                    offset = Math.Round(offset);

                double newOffset = Math.Max(Math.Min(manager.ScrollMaxOffset.X, offset), 0);

				// If we're deferred dragging...
				if (this.ScrollDataInfo.IsInDeferredDrag)
				{
					// Store the temp position
					this.ScrollDataInfo.DeferredDragOffset = newOffset;

					// Let derived classes know that the deferred scroll position has changed.
					this.OnDeferredScrollOffsetChanged(newOffset);

					return;
				}

				if (newOffset != this.ScrollDataInfo._offset.X)
				{
                    Vector oldOffset = this.ScrollDataInfo._offset;
					this.ScrollDataInfo._offset.X = newOffset;
                    manager.OnScrollOffsetChanged(this.ScrollDataInfo._offset, oldOffset);

					// JJD 8/17/10 - TFS36188
					// Invalidate the measure so we can make sure we sync up the scrollbar thumb position
					//this.InvalidateArrange();
					this.InvalidateMeasure();
				}
			}
		}

		    #endregion // SetHorizontalOffset

		        #region SetVerticalOffset

		private void SetVerticalOffset(double offset)
		{
			if (this.IsScrollClient)
			{
                TileManager manager = this.GetManager();

                // if we are scrolling tiles in this dimension then round to offset 
                if (manager.ScrollTilesVertically)
                    offset = Math.Round(offset);

				double newOffset = Math.Max(Math.Min(manager.ScrollMaxOffset.Y, offset), 0);

				// If we're deferred dragging...
				if (this.ScrollDataInfo.IsInDeferredDrag)
				{
					// Store the temp position
					this.ScrollDataInfo.DeferredDragOffset = newOffset;

					// Let derived classes know that the deferred scroll position has changed.
					this.OnDeferredScrollOffsetChanged(newOffset);

					return;
				}

				if (newOffset != this.ScrollDataInfo._offset.Y)
				{
                    Vector oldOffset = this.ScrollDataInfo._offset;
                    this.ScrollDataInfo._offset.Y = newOffset;
                    manager.OnScrollOffsetChanged(this.ScrollDataInfo._offset, oldOffset);

					// JJD 8/17/10 - TFS36188
					// Invalidate the measure so we can make sure we sync up the scrollbar thumb position
					//this.InvalidateArrange();
					this.InvalidateMeasure();
				}
			}
		}
		        #endregion // SetVerticalOffset
        
                #region UnhookAnimation

        private void UnhookAnimation(AnimationClock clock)
        {
            Debug.Assert(clock != null);

            if ( clock == null )
                return;

            DependencyProperty dp = null;

            if (clock == this._repositionClock)
            {
                dp = RepositionFactorProperty;
                this._repositionClock = null;
            }
            else
            if (clock == this._resizeClock)
            {
                dp = ResizeFactorProperty;
                this._resizeClock = null;
            }
            else
            {
                Debug.Fail("Unknown clock");
            }

            clock.Completed -= new EventHandler(OnAnimationCompleted);

            if (dp != null)
            {
                Debug.Assert(this._activeAnimationCount > 0, "activeAnimationCount should be greater than 0");
                this._activeAnimationCount--;

                this.ClearValue(dp);

                if (this._activeAnimationCount < 1)
                    this.ProcessAllAnimationsEnded();
            }
        }

                #endregion //UnhookAnimation	

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

                #region ValidateMaximizedTileLimit

        internal static bool ValidateMaximizedTileLimit(object objVal)
        {
            int val = (int)objVal;
            return val >= 0;
        }

                #endregion // ValidateMaximizedTileLimit

            #endregion //Private Methods

        #endregion //Methods

        #region IScrollInfo Members

        bool IScrollInfo.CanHorizontallyScroll
        {
            get
            {
                return this.IsScrollClient ? this.ScrollDataInfo._canHorizontallyScroll : false;
            }
            set
            {
                if (this.IsScrollClient)
                    this.ScrollDataInfo._canHorizontallyScroll = value;
            }
        }

        bool IScrollInfo.CanVerticallyScroll
        {
            get
            {
                return this.IsScrollClient ? this.ScrollDataInfo._canVerticallyScroll : false;
            }
            set
            {
                if (this.IsScrollClient)
                    this.ScrollDataInfo._canVerticallyScroll = value;
            }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return this.IsScrollClient ? this.ScrollDataInfo._extent.Height : 0; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return this.IsScrollClient ? this.ScrollDataInfo._extent.Width : 0; }
        }

        double IScrollInfo.HorizontalOffset
        {
            get
            {
                double offset = 0;

                if (this.IsScrollClient)
                {
					if (this.ScrollDataInfo.IsInDeferredDrag && this.ScrollBarOrientation == Orientation.Horizontal)
						offset = this.ScrollDataInfo.DeferredDragOffset;
					else
						offset = this.ScrollDataInfo._offset.X;
                }

                return offset;
            }
        }

        void IScrollInfo.LineDown()
        {
            this.AdjustVerticalOffset(this.GetManager().ScrollSmallChange.Y);
        }

        void IScrollInfo.LineLeft()
        {
            this.AdjustHorizontalOffset(-this.GetManager().ScrollSmallChange.X);
        }

        void IScrollInfo.LineRight()
        {
            this.AdjustHorizontalOffset(this.GetManager().ScrollSmallChange.X);
        }

        void IScrollInfo.LineUp()
        {
            this.AdjustVerticalOffset(-this.GetManager().ScrollSmallChange.Y);
        }

        Rect IScrollInfo.MakeVisible(System.Windows.Media.Visual visual, Rect rectangle)
        {
            if (this.IsScrollClient)
                return this.GetManager().MakeVisible(visual, rectangle);

            return rectangle;
        }

        void IScrollInfo.MouseWheelDown()
        {
            this.AdjustVerticalOffset(SystemParameters.WheelScrollLines * this.GetManager().ScrollSmallChange.Y);
        }

        void IScrollInfo.MouseWheelLeft()
        {
            this.AdjustHorizontalOffset(-SystemParameters.WheelScrollLines * this.GetManager().ScrollSmallChange.X);
        }

        void IScrollInfo.MouseWheelRight()
        {
            this.AdjustHorizontalOffset(SystemParameters.WheelScrollLines * this.GetManager().ScrollSmallChange.X);
        }

        void IScrollInfo.MouseWheelUp()
        {
            this.AdjustVerticalOffset(-SystemParameters.WheelScrollLines * this.GetManager().ScrollSmallChange.Y);
        }

        void IScrollInfo.PageDown()
        {
            this.AdjustVerticalOffset(this.GetManager().ScrollLargeChange.Y);
        }

        void IScrollInfo.PageLeft()
        {
            this.AdjustHorizontalOffset(-this.GetManager().ScrollLargeChange.X);
        }

        void IScrollInfo.PageRight()
        {
            this.AdjustHorizontalOffset(this.GetManager().ScrollLargeChange.X);
        }

        void IScrollInfo.PageUp()
        {
            this.AdjustVerticalOffset(-this.GetManager().ScrollLargeChange.Y);
        }

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get
            {
                return this.IsScrollClient ? this.ScrollDataInfo._scrollOwner : null;
            }
            set
            {
                // AS 2/11/08 NA 2008 Vol 1
                // Whether we are in control of scrolling has a bearing on our measurement
                // so make sure we invalidate the measure if changed.
                //
                if (value != this.ScrollDataInfo._scrollOwner)
                {
                    this.ScrollDataInfo._scrollOwner = value;
                    this.InvalidateMeasure();
                }
            }
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            this.SetHorizontalOffset(offset);
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
            this.SetVerticalOffset(offset);
        }

        double IScrollInfo.VerticalOffset
        {
            get
            {
                double offset = 0;

                if (this.IsScrollClient)
                {
					if (this.ScrollDataInfo.IsInDeferredDrag && this.ScrollBarOrientation == Orientation.Vertical)
						offset = this.ScrollDataInfo.DeferredDragOffset;
					else
						offset = this.ScrollDataInfo._offset.Y;
                }

                return offset;
            }
        }

        double IScrollInfo.ViewportHeight
        {
            get { return this.IsScrollClient ? this.ScrollDataInfo._viewport.Height : 0; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return this.IsScrollClient ? this.ScrollDataInfo._viewport.Width : 0; }
        }

        #endregion //IScrollInfo

		#region Nested Classes

            #region GenerationCache internal class

        internal class GenerationCache : IDisposable
        {
            #region Private Members

            internal TilesPanelBase _panel;
            private TileManager _manager;
            private IList _childElements;
            private int _childElementCount;
            private Size _constraint;
            private Size _measureConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
            private IItemContainerGenerator _generator;
            private IDisposable _currentGeneratorStart;
            // JJD 3/18/10 - TFS28705 - Optimization
            internal ArrayList _itemsExpectedToBeGenerated;
            private int _currentGeneratorPositionIndex;
            private int _nextChildElementIndex;
            private ItemsControl _itemsControl;
            internal GridBagLayoutManager _previousGridBagLayoutNormalModeOverall;
            
            
            
            internal Dictionary<DependencyObject, TileManager.LayoutItem> _previousItemsToBeArrangedTempCache;
            internal HashSet _newItemsToBeArrangedTempCache;
            internal Size _desiredSize;
            internal bool _requiresAnotherLayoutPass;





            #endregion //Private Members

            #region Constructor

            internal GenerationCache(TilesPanelBase panel,
                                    Size constraint)
            {
                this._panel = panel;
                this._itemsControl = ItemsControl.GetItemsOwner(this._panel);

                this._manager = panel.GetManager();
                this._constraint = constraint;
                this._childElements = this._panel.ChildElements;
                this._childElementCount = this._childElements.Count;

                ScrollDirection scrollDirection = this._manager.BeginGeneration(this);

                // JJD 3/18/10 - TFS28705 - Optimization
                // Pass expected items into BeginGeneration
                //this._panel.BeginGeneration(scrollDirection);
                this._panel.BeginGeneration(scrollDirection, this._itemsExpectedToBeGenerated);
                
                // JJD 3/18/10 - TFS28705 - Optimization
                // Clear the expected items list since we don't need it anymore
                this._itemsExpectedToBeGenerated = null;

                this._generator = this._panel.GetGenerator();




             }

            #endregion //Constructor

            #region Properties

            #endregion //Properties

            #region Methods

                #region Internal Methods

            internal void GenerateElements()
            {
                this._manager.GenerateElements(this);
            }

                #endregion //Internal Methods

                #region Private Methods
            
                    #region GenerateElement

            internal UIElement GenerateElement(int indexToGen)
            {




                if ( this._itemsControl == null )
                    return this._childElements[indexToGen] as UIElement;




                bool isNewlyRealized;

                // if the current generator position doesn't match the index we
                // want to gen next then restart the generator
                if (this._currentGeneratorStart == null ||
                    this._currentGeneratorPositionIndex != indexToGen)
                    this.StartGenerator(indexToGen);

                //// generate the next element
                UIElement generatedElement = this._generator.GenerateNext(out isNewlyRealized) as UIElement;

                // bump the _currentGeneratorPosition  so we know which element will be generated
                // the next time GenerateNext is called and can compare it above to restart the generator
                // if the indexToGen doesn't match
                this._currentGeneratorPositionIndex++;

                Debug.Assert(generatedElement != null, "The element should have been generated.");

                if (generatedElement == null)
                    return null;


                // If the generated item is 'newly realized', add it to our children collection
                // and 'prepare' it.
                if (isNewlyRealized)
                {
                    if (this._nextChildElementIndex < 0)
                    {
                        this._nextChildElementIndex = this._panel.FindSlotForNewlyRealizedRecordPresenter(indexToGen);
                    }

                    // GridViewPanel now derives from RecyclingControlPanel 
                    // so don't use InternalChildren
                    //if (indexToInsertNewlyRealizedItems >= internalChildren.Count)
                    if (this._nextChildElementIndex >= this._childElementCount)
                        this._panel.AddInternalChild(generatedElement);
                    else
                        this._panel.InsertInternalChild(this._nextChildElementIndex, generatedElement);

                    // update the childcount
                    this._childElementCount++;

                    this._generator.PrepareItemContainer(generatedElement);
                    




                }

                // Bump _nextChildElementIndex so we know where to insert the next child element.
                if (this._nextChildElementIndex >= 0)
                    this._nextChildElementIndex++;


                return generatedElement;
            }

                    #endregion //GenerateNextElement

                    #region ReleaseGeneratorStart

            private void ReleaseGeneratorStart()
            {
                if (this._currentGeneratorStart != null)
                {
                    this._currentGeneratorStart.Dispose();
                    this._currentGeneratorStart = null;
                }
            }

                    #endregion //ReleaseGeneratorStart

                    #region StartGenerator

            private void StartGenerator(int indexToGen)
            {
                this.ReleaseGeneratorStart();

                this._currentGeneratorPositionIndex = indexToGen;

                GeneratorPosition generatorStartPosition = this._panel.GetGeneratorPositionFromItemIndex(indexToGen, out this._nextChildElementIndex);

                this._currentGeneratorStart = this._generator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true);
            }

                    #endregion //StartGenerator

            #endregion //Private Methods

            #endregion //Methods

            #region IDisposable Members

            public void Dispose()
            {
                this.ReleaseGeneratorStart();

                if (this._generator != null)
                    this._panel.EndGeneration();

                this._desiredSize = this._manager.EndGeneration(this);



            }

            #endregion

        }

            #endregion //GenerationCache internal class	
    
			#region ScrollData Internal Class

		internal class ScrollData
		{
			#region Member Variables

			internal ScrollViewer _scrollOwner = null;
			internal Size _extent = new Size();
			internal Size _viewport = new Size();
			internal Vector _offset = new Vector();
			internal Vector _oldOffset = new Vector();
			internal bool _canHorizontallyScroll = false;
			internal bool _canVerticallyScroll = false;

			private bool				_isInDeferredDrag = false;
			private double				_deferredDragOffset;

            private TilesPanelBase _panel;

			#endregion //Member Variables

            #region Constructor

            internal ScrollData(TilesPanelBase panel)
            {
                this._panel = panel;
            }

            #endregion //Constructor	
    
			#region Methods

			#region Reset

			internal void Reset()
			{
                Vector oldOffset = this._offset;
				this._offset = new Vector();
				this._extent = new Size();
				this._viewport = new Size();
                this._panel.GetManager().OnScrollOffsetChanged(this._offset, oldOffset);
			}

			#endregion //Reset

			#region VerifyScrollData
			internal void VerifyScrollData(Size viewPort, Size extent)
			{
				// if we have endless space use the space we need
				if (double.IsInfinity(viewPort.Width))
					viewPort.Width = extent.Width;
				if (double.IsInfinity(viewPort.Height))
					viewPort.Height = extent.Height;

				bool isDifferent = false == TileManager.AreClose(this._viewport.Width, viewPort.Width) |
                    false == TileManager.AreClose(this._viewport.Height, viewPort.Height) ||
                    false == TileManager.AreClose(this._extent.Width, extent.Width) ||
                    false == TileManager.AreClose(this._extent.Height, extent.Height);

                Size oldviewPort    = this._viewport;
                Size oldExtent      = this._extent;

				this._viewport = viewPort;
				this._extent = extent;

                //Debug.WriteLine("VerfifyScrollData viewport: " + this._viewport.ToString() + ", extent: " + this._extent.ToString());
                //Debug.WriteLine("VerfifyScrollData offset before: " + this._offset.ToString());
				
                Vector oldOffset = this._offset;

				isDifferent |= this.VerifyOffset();
                
                if ( isDifferent )
                    this._panel.GetManager().OnScrollOffsetChanged(this._offset, oldOffset);

                //Debug.WriteLine("VerfifyScrollData offset sfter: " + this._offset.ToString());

				// dirty the scroll viewer if something has changed
				if (null != this._scrollOwner && isDifferent)
				{
                    this._oldOffset = this._offset;
					this._scrollOwner.InvalidateScrollInfo();
				}
			}
			#endregion //VerifyScrollData

			#region VerifyOffset
			private bool VerifyOffset()
			{
                double offsetX = Math.Max(Math.Min(this._offset.X, this._panel.MaxHorizontalOffset), 0);
                double offsetY = Math.Max(Math.Min(this._offset.Y, this._panel.MaxVerticalOffset), 0);
				Vector oldOffset = this._oldOffset;
				this._offset = new Vector(offsetX, offsetY);

				// return true if the offset has changed
                return false == TileManager.AreClose(this._offset.X, oldOffset.X) ||
                    false == TileManager.AreClose(this._offset.Y, oldOffset.Y);
			}
			#endregion //VerifyOffset

			#endregion //Methods

			#region Properties

				#region DeferredDragOffset






		internal double DeferredDragOffset
		{
			get { return this._deferredDragOffset; }
			set { this._deferredDragOffset = value; }
		}

				#endregion //DeferredDragOffset	

				#region IsInDeferredDrag






		internal bool IsInDeferredDrag
		{
			get { return this._isInDeferredDrag; }
			set { this._isInDeferredDrag = value; }
		}

				#endregion //IsInDeferredDrag	

			#endregion //Properties
		}

			#endregion //ScrollData Internal Class
	
            #region TilesPanelUIElementCollection Internal Class

        /// <summary>
        /// The Collection containg the child elements of a <see cref="TilesPanelBase"/> 
        /// </summary>
	    protected class TilesPanelUIElementCollection : UIElementCollection
	    {
		    #region Member Variables

		    private TilesPanelBase						_tilesPanel = null;

		    #endregion //Member Variables

		    #region Constructor

		    /// <summary>
		    /// Creates an instance of the TilesPanelUIElementCollection class.
		    /// </summary>
		    /// <remarks>
		    /// <p class="note"><b>Note: </b>This class is for internal use only.</p>
		    /// </remarks>
		    public TilesPanelUIElementCollection(TilesPanelBase tilesPanel, UIElement visualParent, FrameworkElement logicalParent)
			    : base(visualParent, logicalParent)
		    {
			    this._tilesPanel = tilesPanel;
		    }

		    #endregion //Constructor

		    #region Base Class Overrides

			    #region Add

		    /// <summary>
		    /// Adds an element to the collection.
		    /// </summary>
		    /// <param name="element">The element to add</param>
		    /// <returns></returns>
		    public override int Add(UIElement element)
		    {
			    int index = base.Add(element);
			    this.RaiseCollectionChanged();
			    return index;
		    }

			    #endregion //Add

			    // JM 11-28-07 BR28764
			    #region Clear

            /// <summary>
            /// Clears the collection
            /// </summary>
		    public override void Clear()
		    {
			    base.Clear();
			    this.RaiseCollectionChanged();
		    }

			    #endregion //Clear	
        
			    #region GetEnumerator

		    /// <summary>
		    /// Returns an IEnumerator that can be used to iterate through the collection.
		    /// </summary>
		    /// <returns></returns>
		    public override IEnumerator GetEnumerator()
		    {
			    return base.GetEnumerator();
		    }

			    #endregion //GetEnumerator	
        
			    #region Insert

            /// <summary>
            /// Inserts an item into the collection at a specified index
            /// </summary>
            /// <param name="index">The location to insert the item</param>
            /// <param name="element">The item to insert.</param>
		    public override void Insert(int index, UIElement element)
		    {
			    base.Insert(index, element);
			    this.RaiseCollectionChanged();
		    }

			    #endregion //Insert	
        
			    // JM 11-28-07 BR28764
			    #region Remove

            /// <summary>
            /// Removes an item from the collection.
            /// </summary>
            /// <param name="element">The item to remove</param>
		    public override void Remove(UIElement element)
		    {
			    base.Remove(element);
			    this.RaiseCollectionChanged();
		    }

			    #endregion //Remove	
        
			    // JM 11-28-07 BR28764
			    #region RemoveAt

            /// <summary>
            /// Removes an item from the collection at a specific index..
            /// </summary>
            /// <param name="index">The index of the item to be removed</param>
		    public override void RemoveAt(int index)
		    {
			    base.RemoveAt(index);
			    this.RaiseCollectionChanged();
		    }

			    #endregion //RemoveAt	
        
			    // JM 11-28-07 BR28764
			    #region RemoveRange

            /// <summary>
            /// Removes a range of items from the collection
            /// </summary>
            /// <param name="index">The index of the first items to remove.</param>
            /// <param name="count">The number of items to remove.</param>
		    public override void RemoveRange(int index, int count)
		    {
			    base.RemoveRange(index, count);
			    this.RaiseCollectionChanged();
		    }

			    #endregion //RemoveRange	
        
		    #endregion //Base Class Overrides

		    #region Properties

			    #region Internal Properties

				    #region TilesPanelBase

		    internal TilesPanelBase TilesPanelBase
		    {
			    get { return this._tilesPanel; }
		    }

				    #endregion //TilesPanelBase	
        
			    #endregion //Internal Properties
        
		    #endregion //Properties	

		    #region Events

			    #region CollectionChanged (internal CLR event)

		    internal event EventHandler<EventArgs> CollectionChanged;

			    #endregion //CollectionChanged (internal CLR event)

		    #endregion //Events

		    #region Methods

			    #region RaiseCollectionChanged

		    private void RaiseCollectionChanged()
		    {
                // If the panel is hooked up to an items control we don't need t raise this 
                // notification since we are overriding its OnItemsChanged method
                if (this._tilesPanel.IsItemsHost)
                    return;

                if (this.CollectionChanged != null)
				    CollectionChanged(this, EventArgs.Empty);
		    }

			    #endregion //RaiseCollectionChanged

		    #endregion //Methods
	    }

	        #endregion //TilesPanelUIElementCollection Internal Class
    
		#endregion //Nested Classes

		#region IDeferredScrollPanel Members

		void IDeferredScrollPanel.OnThumbDragComplete(bool cancelled)
		{
			if (this.ScrollDataInfo.IsInDeferredDrag)
			{
				// Turn off the thumb drag flag and perform the scroll
				this.ScrollDataInfo.IsInDeferredDrag = false;

				if (this.ScrollBarOrientation == Orientation.Horizontal)
					this.SetHorizontalOffset(this.ScrollDataInfo.DeferredDragOffset);
				else
					this.SetVerticalOffset(this.ScrollDataInfo.DeferredDragOffset);

				// Let derived classes know that deferred scrolling has ended.
				this.OnDeferredScrollingEnded(cancelled);
			}
		}

		void IDeferredScrollPanel.OnThumbDragStart(Thumb thumb, Orientation scrollBarOrientation)
		{
			if (this.GetIsInDeferredScrollingMode())
			{
				this.ScrollDataInfo.IsInDeferredDrag	= true;
				this.ScrollDataInfo.DeferredDragOffset	= this.ScrollBarOrientation == Orientation.Horizontal
					? this.ScrollDataInfo._offset.X
					: this.ScrollDataInfo._offset.Y;

				// Let derived classes know that deferred scrolling has started.
				this.OnDeferredScrollingStarted(thumb, scrollBarOrientation);
			}
		}

		bool IDeferredScrollPanel.SupportsDeferredScrolling(Orientation scrollBarOrientation, ScrollViewer scrollViewer)
		{
			return this.GetSupportsDeferredScrolling(scrollBarOrientation, scrollViewer);
		}

		#endregion //IDeferredScrollPanel Members
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