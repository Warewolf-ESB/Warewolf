using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using Infragistics.Collections;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Infragistics.Controls.Layouts.Primitives
{
	/// <summary>
	/// A <see cref="System.Windows.Controls.Panel"/> used exclusively in the template of a <see cref="XamTileManager"/> to measure and arrange its <see cref="XamTile"/> elements.
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note:</b> this panel does not support stand-alone usage. It must be used in the template of a <see cref="XamTileManager"/>.</para>
	/// </remarks>
	[DesignTimeVisible(false)]	
	public class TileAreaPanel : Panel, IScrollInfo
	{
        #region Private Members
		
		private XamTileManager _owner;

		private Storyboard				_repositionStoryBoard;
		private Storyboard				_resizeStoryBoard;
		private TileAreaSplitter		_splitter;
		private bool					_asynchAnimationStartPending;

		private bool					_processFactorChangePending;
		private bool					_isInMeasure;

        private Size                    _lastMeasureSize;  
        private Size                    _lastArrangeSize;

        private Size                    _lastDesiredSize; 
 
        private Size                    _tileAreaSize;
        private TileDragManager         _dragManager; 

        private int                     _activeAnimationCount;
        private int                     _arrangeCount;
        private int                     _measureCount;
        private Vector                   _lastScrollOffset;
        private DateTime                _lastMeasureTimeStamp = DateTime.Now;
        private Size                    _itemsControlLastRenderSize;
        private bool                    _bypassArrangeVerification;
        private bool                    _bypassArrange;
        private bool                    _ignoreChildSizeChanges;
        private bool                    _childSizeChanged; 
		
		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
        private bool                    _lastArrangeIsInMaximizedMode; 
		internal bool					_bypassAnimations;
		private double					_panningPixelOffsetX;
		private double					_panningPixelOffsetY;

        
        private bool                    _inViewItemsHaveChanged;
        private bool                    _isRestartingAnimations;
		private bool					_bypassNextFactorChange;
		private bool					_isDragging;
		private Canvas					_extraChildrenCanvas;

        private double _repositionFactor	= (double)DependencyPropertyUtilities.GetDefaultValue(typeof(TileAreaPanel), RepositionFactorProperty);
        private double _resizeFactor		= (double)DependencyPropertyUtilities.GetDefaultValue(typeof(TileAreaPanel), ResizeFactorProperty);

		private NormalModeSettings _normalModeSettingsSafe;
		private MaximizedModeSettings _maximizedModeSettingsSafe;

        #endregion //Private Members	

		#region Attach

		internal void Attach(XamTileManager owner)
		{
			this._owner = owner;

			_normalModeSettingsSafe = null;
			_maximizedModeSettingsSafe = null;
		}

		#endregion //Attach	
    
		#region Detach

		internal void Detach()
		{
			this._owner = null;
		}

		#endregion //Detach	
    
        #region Constructor

        /// <summary>
        /// Instantiates a new instance of a <see cref="TileAreaPanel"/>.
        /// </summary>
        public TileAreaPanel() 
        {

			// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
			this.SetCurrentValue(BackgroundProperty, Brushes.Transparent);

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
			// JJD 10/18/11 - TFS92439
			// Check to make sure we are still wired to an owner XamTileManager
			//if (this._bypassArrange)
            if (this._bypassArrange || _owner == null)
            {
                // JJD 2/22/10 - TFS28021/TFS28025
                // Clear the counts
                this._arrangeCount = 0;
                this._measureCount = 0;
                
                this._bypassArrange = false;

                return finalSize;
            }

			// Clear the _bypassNextFactorChange flag since we are in the arrange pass
			// since this flag was meant to ignore the 1st of 2 factor changes (i.e. the Reposition one)
			// so the other factor wasn't treated as 1.0 before the second animation
			// (the Resize one) had a chance to initialize its from (0.0) value.
			// This was only a problem in SL but we are doing it in WPF in case the
			// WPF framework changes how it deals with animations
			_bypassNextFactorChange = false;

            bool areLocationsTheSameAsLastTime = false;
            this._ignoreChildSizeChanges = true;

            // JJD 2/13/10 - Race condition detection between Measure and arrange
            // Keep track of the arrange passes
            this._arrangeCount++;

            try
            {
                TileLayoutManager tm = this.LayoutManager;

                // check the _bypassArrangeVerification flag.
                // this should prevent getting in an endless loop going
                // between measure and arrange passes
                if (this._bypassArrangeVerification)
                    this._bypassArrangeVerification = false;
                else
                {
                    // call VerifyArrangeSize which will make sure the 
                    // scrollbar hasn't scrolled too far. If so it will return false
                    tm.VerifyArrangeSize();


                    // if it return false then invalidate our measure and return
                    if (!this.IsMeasureValid)
                    {
                        // set a flag so we bypass the verify on the next arrange pass
                        // so we don't end up in a race condition between measure and arrange
                        this._bypassArrangeVerification = true;
                        return finalSize;
                    }

                }



#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)


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

            //bool bypassAnimations = false;
            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
			// If mode was changed then don't bypass animations
			if (_lastArrangeIsInMaximizedMode != _owner.IsInMaximizedMode)
			{
				_lastArrangeIsInMaximizedMode = _owner.IsInMaximizedMode;
				_bypassAnimations = false;
			}

			// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
			// Only bypass animations if the flag is set and an animation isn't already in process
			//bool requiresAnimation = this.ArrangeHelper(true, false, bypassAnimations);
			bool requiresAnimation = this.ArrangeHelper(true, false, _bypassAnimations && this.IsAnimationInProgress == false);

			// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
			// reset flag
			_bypassAnimations = false;
 
            if (requiresAnimation)
            {
                
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

                if (this.IsAnimationInProgress == false || this._repositionFactor > .5)
                {
                    
                    
                    // On the first pass try to start the animations asynchronously
					if (this._asynchAnimationStartPending == false)
					{
						this._asynchAnimationStartPending = true;

						this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new TileUtilities.MethodDelegate(this.ProcessAnimationRestart));



					}
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

			// Get the child elements that have been released and arrange them out of view 
			// In SL we call GetRecentlyAvailableElements with false because we don't want to clear the 
			// cached list of elements. This seems necessary because the call to arrange the children doesn't
			// seem to take the first time thru.
			

			List<FrameworkElement> recentlyReleased = RecyclingManager.Manager.GetRecentlyAvailableElements(this, true);



			foreach (FrameworkElement child in recentlyReleased)
			{
				Size size = child.DesiredSize;

				// arrange out of view
				child.Arrange(new Rect(-100000, -100000, size.Width, size.Height));
			}


			if (_splitter != null)
			{
				Rect splitterRect = this.GetSplitterRect();

				if (splitterRect.IsEmpty)
				{
					_splitter.SetValue(VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
				}
				else
				{
					_splitter.SetValue(VisibilityProperty, KnownBoxes.VisibilityVisibleBox);

					double measureExtent;
					double desiredExtent;

					if (TileAreaSplitter.GetOrientation(this._splitter) == Orientation.Vertical)
					{
						measureExtent = splitterRect.Width;
						desiredExtent = this._splitter.DesiredSize.Width;
						splitterRect.Width = desiredExtent;

						// Make sure we don't try to arrange with an infinite height
						if (double.IsPositiveInfinity(splitterRect.Height))
							splitterRect.Height = finalSize.Height;
					}
					else
					{
						measureExtent = splitterRect.Height;
						desiredExtent = this._splitter.DesiredSize.Height;
						splitterRect.Height = desiredExtent;

						// Make sure we don't try to arrange with an infinite width
						if (double.IsPositiveInfinity(splitterRect.Width))
							splitterRect.Height = finalSize.Width;
					}

					bool isVertical = TileAreaSplitter.GetOrientation(this._splitter) == Orientation.Vertical;

					double delta = measureExtent - desiredExtent;

					// if the splitter resists the size change then try to center it
					if (delta > 0 && !TileUtilities.AreClose(delta, 0))
					{
						if (isVertical)
							splitterRect.X += delta / 2;
						else
							splitterRect.Y += delta / 2;
					}
				}

				if (!splitterRect.IsEmpty)
					_splitter.Arrange(splitterRect);
			}

			// if we have any extra children, e.g. resizer bars or area splitter, then arrange the canvas now
			if (_extraChildrenCanvas != null)
				_extraChildrenCanvas.Arrange(new Rect(new Point(), finalSize));

            return finalSize;
        }

			#endregion //ArrangeOverride	

			#region CreateUIElementCollection


		/// <summary>
		/// Called to create the collection to hold the child elements.
		/// </summary>
		/// <param name="logicalParent"></param>
		/// <returns>An instance of a UIElementCollection</returns>
		protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
		{
			// Pass null into the ctor for the logicalParent parameter since all children, other than
			// the XamTile wrappers, will be logical children of the associated XamTileManager.
			return new UIElementCollection(this, null);
		}
	

			#endregion //CreateUIElementCollection	
    
            #region GetLayoutClip

        /// <summary>
        /// Returns a geometry for the clipping mask for the element.
        /// </summary>
        /// <param name="layoutSlotSize">The size of the element</param>
        /// <returns>A geometry to clip that takes into account the TileAreaPadding</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            Thickness tileAreaPadding = this._owner.TileAreaPadding;

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
    
			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
            //Debug.WriteLine("TileAreaPanel.Measure available size = " + availableSize.ToString());

			this._lastMeasureSize = availableSize;

			this._isInMeasure = true;

			try
			{
				_lastDesiredSize = this.MeasureHelper(availableSize);
			}
			finally
			{
				this._isInMeasure = false;
			}

			return _lastDesiredSize;
		}

		// JJD 4/15/11 - TFS70658 - OS/Framework bug workaround - added MeasureHelper method
		// When a window is maximized on certain high resolution monitors
		// we can be measured with a size smaller than what we get in the arrange.
		// Move logic to MeasureHelper method
		private Size MeasureHelper(Size availableSize)
		{

            this._tileAreaSize = availableSize;

			// JJD 1/5/12 - TFS97649
			// Call measure on all of the panel's direct children 
			if (_splitter != null)
				_splitter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

			// JJD 1/5/12 - TFS97649
			// Call measure on all of the panel's direct children 
			if (_extraChildrenCanvas != null)
				_extraChildrenCanvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            #region Detect race condition between measure and arrange

            // JJD 2/13/10 - Race condition detection between Measure and arrange
            // Keep track of the measure passes
            this._measureCount++;

            // by default we will assume that we don't want to clear the race condition counters
            bool clearRaceConditionCounters = false;

            // if we are the items host for an ItemsControl and it is being resized we can
            // clear our race condition counts
            if (_owner != null)
            {
                Size renderSize = _owner.RenderSize;

                // If the size is not the same as last time then we can safely
                // clear the race condition counters
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
			    // snapshot the time
				this._lastMeasureTimeStamp = DateTime.Now;
			}
            
            
            else if (this._arrangeCount > 30 && this._measureCount > 30)
            {
                Debug.WriteLine("*** Race condition detected between TileAreaPanel Measure and Arrange ***");
                this._bypassArrange = true;
			    // snapshot the time
				this._lastMeasureTimeStamp = DateTime.Now;
            }

            
            

            #endregion //Detect race condition between measure and arrange	
    

            Thickness tileAreaPadding = this._owner.TileAreaPadding;

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

            // keep trying to layout items until the GenerationCache returns
            // false for _requiresAnotherLayoutPass. It returns true when
            // it needs to scroll up because we have scrolled too far.
            // This is the pull down logic
            while (genRequired)
            {
				bool isInMaximizedMode = this._owner.IsInMaximizedMode;

                using (genCache = new GenerationCache(this, availableSize))
                    genCache.GenerateElements();
                
                // we have to get the flag outside of the 'using' statement
                // above because the implementation of the Dispose method
                // is where that info is determined

				// JJD 8/17/10 - TFS35319
				// If the gen pass above caused use to go into/out of maximized
				// mode then do another layout pass
                //genRequired = genCache._requiresAnotherLayoutPass;
                genRequired = genCache._requiresAnotherLayoutPass ||
								isInMaximizedMode != this._owner.IsInMaximizedMode;
            }

            Size desiredSize = genCache._desiredSize;
            //Debug.WriteLine("TileAreaPanel measure desired size: " + desiredSize.ToString());

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

		//    #region OnChildDesiredSizeChanged

		///// <summary>
		///// Invoked when the <see cref="UIElement.DesiredSize"/> of an element changes.
		///// </summary>
		///// <param name="child">The child whose size is being changed.</param>
		//protected override void OnChildDesiredSizeChanged(UIElement child)
		//{
		//    if (this._ignoreChildSizeChanges)
		//    {
		//        // JJD 2/12/10 
		//        // Set a flag so we know a child size was changed when we ignored it
		//        this._childSizeChanged = true;
		//        return;
		//    }

		//    base.OnChildDesiredSizeChanged(child);
		//}

		//    #endregion //OnChildDesiredSizeChanged	

		//    #region OnVisualChildrenChanged

		///// <summary>
		///// Called when a child element is added or moved
		///// </summary>
		///// <param name="visualAdded"></param>
		///// <param name="visualRemoved"></param>
		//protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		//{
		//    base.OnVisualChildrenChanged(visualAdded, visualRemoved);

		//    if (visualRemoved != null)
		//        this.LayoutManager.RemoveContainer(visualRemoved);
		//}

		//    #endregion //OnVisualChildrenChanged	
    
		#endregion //Base Class Overrides

        #region Properties

            #region Internal Properties

                #region IsAnimationInProgress





        internal bool IsAnimationInProgress
        {
            get
            {
                 return this._activeAnimationCount > 0;
            }
        }

                #endregion //IsAnimationInProgress

				#region IsDragging

		internal bool IsDragging { get { return _isDragging && _dragManager != null; } }

				#endregion //IsDragging	
    
				#region IsInMeasure

		internal bool IsInMeasure { get { return _isInMeasure; } }

				#endregion //IsInMeasure	
        
                #region LastMeasureSize

        internal Size LastMeasureSize { get { return this._lastMeasureSize; } }

                #endregion //LastMeasureSize	

				#region LayoutManagwer

		internal TileLayoutManager LayoutManager
		{
			get
			{
				return _owner != null ? _owner.LayoutManager : null;
			}
		}

				#endregion //LayoutManagwer	
    
				#region MaximizedModeSettingsSafe

		internal MaximizedModeSettings MaximizedModeSettingsSafe
		{
			get
			{
				if (_owner != null)
					return _owner.MaximizedModeSettingsSafe;

				if (_maximizedModeSettingsSafe == null)
					_maximizedModeSettingsSafe = new MaximizedModeSettings();

				return _maximizedModeSettingsSafe;
			}
		}

				#endregion //MaximizedModeSettingsSafe	

				#region Manager

		internal XamTileManager Manager { get { return _owner; } }

				#endregion //Manager
    
                #region MinimizedAreaCurrentExtent

        internal double MinimizedAreaCurrentExtent
        {
            get
            {
                double extent = this.MinimizedAreaExplicitExtent;

                if (extent > 0)
                    return extent;

                return this.GetActualMinimizedAreaExtent();
            }
        }

                #endregion //MinimizedAreaCurrentExtent	

                #region MinimizedAreaExplicitExtent

        internal double MinimizedAreaExplicitExtent
        {
            get
            {
				if (this._owner == null)
                    return 0;

				// JJD 06/04/12 - TFS105829
				// if all of the miminized tiles have been closed (i.e. the scroll count is zero)
				// then return 0 so the maximized tiles take up all of the available space.
				if (this._owner.IsInMaximizedMode &&
					this._owner.LayoutManager.SparseArray.ScrollCount == 0 )
					return 0;

                switch (this.MaximizedModeSettingsSafe.MaximizedTileLocation)
                {
                    case MaximizedTileLocation.Left:
                    case MaximizedTileLocation.Right:
                        return this._owner.MinimizedAreaExplicitExtentX;
                    default:
						return this._owner.MinimizedAreaExplicitExtentY;
                }
            }
            set
            {
				if (this._owner != null)
                {
                    switch (this.MaximizedModeSettingsSafe.MaximizedTileLocationResolved)
                    {
                        case MaximizedTileLocation.Left:
                        case MaximizedTileLocation.Right:
							this._owner.MinimizedAreaExplicitExtentX = value;
                            break;
                        default:
							this._owner.MinimizedAreaExplicitExtentY = value;
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
				ITileConstraints constraints = this.LayoutManager.GetMaximizedModeTileAreaConstraints(TileLayoutManager.TileArea.MinimizedTiles);

                if (constraints == null)
                    return 0; 

                switch (this.MaximizedModeSettingsSafe.MaximizedTileLocationResolved)
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
				bool isSplitterVertical;

				// determine if the splitter is vertical or horizontal
                switch (this.MaximizedModeSettingsSafe.MaximizedTileLocationResolved)
				{
					case MaximizedTileLocation.Left:
					case MaximizedTileLocation.Right:
						isSplitterVertical = true;
						break;
					default:
						isSplitterVertical = false;
						break;
				}

				// get the explicit max extent for the minimized tile area
				double minAreaMaxExtent = GetMinimizedAreaMaxExtent(isSplitterVertical);

				// get the explicit min extent for the maximized tile area
				double maxAreaMinExtent = GetMamizedAreaMinExtent(isSplitterVertical);
				
				double panelExtent;

				// get the overall usable extent of the panel (backing out the splitter extent
				if ( isSplitterVertical )
					panelExtent = this.ActualWidth - this.GetSplitterRect().Width;
				else
					panelExtent = this.ActualHeight - this.GetSplitterRect().Height;

				// return whichever is less
				// a: the explicit max constraint for the minimized tile area
				// b: the available panel extent after subtracting the explicit min constraint for the maximized tile area 
				return Math.Min(minAreaMaxExtent, Math.Max( panelExtent - maxAreaMinExtent, 0));
            }
        }

                #endregion //MinimizedAreaMaxExtent	

				#region NormalModeSettingsSafe

		internal NormalModeSettings NormalModeSettingsSafe
		{
			get
			{
				if (_owner != null)
					return _owner.NormalModeSettingsSafe;

				if (_normalModeSettingsSafe == null)
					_normalModeSettingsSafe = new NormalModeSettings();

				return _normalModeSettingsSafe;
			}
		}

				#endregion //NormalModeSettingsSafe	

				#region RepositionFactor

		/// <summary>
		/// For internal use 
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never)]
		public static readonly DependencyProperty RepositionFactorProperty = DependencyPropertyUtilities.Register("RepositionFactor",
			typeof(double), typeof(TileAreaPanel),
			DependencyPropertyUtilities.CreateMetadata(1.0d, new PropertyChangedCallback(OnRepositionFactorChanged))
			);

        private static void OnRepositionFactorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileAreaPanel panel = target as TileAreaPanel;

            panel._repositionFactor = (double)e.NewValue;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

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
                this.SetValue(TileAreaPanel.RepositionFactorProperty, value);
            }
        }

                #endregion //RepositionFactor

				#region ResizeFactor

		/// <summary>
		/// For internal use 
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never)]
		public static readonly DependencyProperty ResizeFactorProperty = DependencyPropertyUtilities.Register("ResizeFactor",
			typeof(double), typeof(TileAreaPanel),
			DependencyPropertyUtilities.CreateMetadata(1.0d, new PropertyChangedCallback(OnResizeFactorChanged))
			);

        private static void OnResizeFactorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileAreaPanel panel = target as TileAreaPanel;

            panel._resizeFactor = (double)e.NewValue;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

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
                this.SetValue(TileAreaPanel.ResizeFactorProperty, value);
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

                #region ShouldAnimate

        internal bool ShouldAnimate
        {
            get
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                    return false;

                if (_owner != null && _owner.IsInMaximizedMode)
                    return this.MaximizedModeSettingsSafe.ShouldAnimate;
                else
                    return this.NormalModeSettingsSafe.ShouldAnimate;
            }
        }

                #endregion //ShouldAnimate	

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

				#region LogicalOrientation


		/// <summary>
		/// The orientation of the panel
		/// </summary>
		protected override Orientation LogicalOrientation



		{
			get
			{
				if (_owner == null)
					return Orientation.Vertical;

				if (_owner.IsInMaximizedMode)
				{
                    switch (this.MaximizedModeSettingsSafe.MaximizedTileLocationResolved)
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
					switch (this.NormalModeSettingsSafe.TileLayoutOrder)
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

                #region MaxHorizontalOffset

        internal double MaxHorizontalOffset
        {
            get
            {
                return this.LayoutManager.ScrollMaxOffset.X;
            }
        }

                #endregion //MaxHorizontalOffset	
    
                #region MaxVerticalOffset

        internal double MaxVerticalOffset
        {
            get
            {
                return this.LayoutManager.ScrollMaxOffset.Y;
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
    
            #endregion //Public Methods	
    
            #region Internal Methods

			#region AddExtraChild

		internal void AddExtraChild(FrameworkElement child)
		{

			TileAreaSplitter splitter = child as TileAreaSplitter;

			if (splitter != null)
			{
				this._splitter = splitter;

				this.Children.Add(_splitter);
				return;
			}

			if (_extraChildrenCanvas == null)
			{
				_extraChildrenCanvas = new Canvas();

				this.Children.Add(_extraChildrenCanvas);

				Canvas.SetZIndex(_extraChildrenCanvas, 5000);

				this.InvalidateArrange();

			}
			else
			{
				if (_extraChildrenCanvas.Children.Contains(child))
				{
					Debug.Assert(false, "extra child already added");
					return;
				}

				Debug.Assert(this._extraChildrenCanvas.Children.Count < 3, "There can only be at most 2 resizer bars and one splitter");
			}

			this._extraChildrenCanvas.Children.Add(child);

		}

			#endregion //AddExtraChild	
    
            #region CanSwapContainers



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

        internal bool CanSwapContainers(FrameworkElement dragItemContainer, ItemTileInfo dragItemInfo, FrameworkElement swapTargetContainer, ItemTileInfo swapTargetItemInfo)
        {
			XamTile source = dragItemContainer as XamTile;
			XamTile target = swapTargetContainer as XamTile;

			if (source == null ||
				 target == null)
				return false;

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

			XamTileManager mgr = this.Manager;
			if (mgr == null)
				return false;

			TileSwappingEventArgs args = new TileSwappingEventArgs(source, dragItemInfo.Item, target, swapTargetItemInfo.Item);

			// JJD 11/1/11 - TFS88171 
			// Initialize the SwapIsExpandedWhenMinimized property on the TileSwappingEventArgs 
			// based on whether the source or target is maximized and the other tile is not
			args.SwapIsExpandedWhenMinimized = isSourceMaximized != isTargetMaximized;

			// raise the TileSwapping event
			mgr.RaiseTileSwapping(args);

			// if canceled return false
			if (args.Cancel)
				return false;

			// JJD 11/1/11 - TFS88171 
			// Cache the swap info for use after the swap
			mgr._swapinfo = new XamTileManager.SwapInfo();
			mgr._swapinfo._swapIsExpandedWhenMinimized = args.SwapIsExpandedWhenMinimized;
			mgr._swapinfo._sourceIsExpandedWhenMinimized = source.IsExpandedWhenMinimized;
			mgr._swapinfo._targetIsExpandedWhenMinimized = target.IsExpandedWhenMinimized;

			return true;
		}

            #endregion //CanSwapContainers	

            #region BringIndexIntoView

        internal void BringIndexIntoView(int index)
        {
			if ( _owner != null )
				this._owner.LayoutManager.BringIndexIntoView(index);
        }

            #endregion //BringIndexIntoView	

            #region GetActualMinimizedAreaExtent



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        internal double GetActualMinimizedAreaExtent()
        {
            return this.LayoutManager.MinimizedAreaActualExtent;
        }

            #endregion //GetActualMinimizedAreaExtent	
            
            #region GetAllowTileDragging



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal AllowTileDragging GetAllowTileDragging(FrameworkElement tile, ItemTileInfo itemInfo)
        {
			if (_owner == null)
				return AllowTileDragging.No;

			
			if (this._owner.IsInMaximizedMode)
				return this.MaximizedModeSettingsSafe.AllowTileDragging;
			else
				return this.NormalModeSettingsSafe.AllowTileDragging;
		}

            #endregion //GetAllowTileDragging	
 
            #region GetContainerConstraints



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal ITileConstraints GetContainerConstraints(XamTile tile, TileState state)
        {
			ITileConstraints constraints = null;

			switch (state)
			{
				case TileState.Normal:
					constraints = XamTileManager.GetConstraints(tile);
					break;

				case TileState.Maximized:
					constraints = XamTileManager.GetConstraintsMaximized(tile);
					break;

				case TileState.Minimized:
					constraints = XamTileManager.GetConstraintsMinimized(tile);
					break;

				case TileState.MinimizedExpanded:
					constraints = XamTileManager.GetConstraintsMinimizedExpanded(tile);
					break;
			}

			return constraints;
		}

                #endregion //GetContainerConstraints	
 
            #region GetDefaultConstraints



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal ITileConstraints GetDefaultConstraints(TileState state)
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

                #endregion //GetDefaultConstraints	
 
            #region GetDefaultMinimumItemSize



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal Size GetDefaultMinimumItemSize()
        {
            return new Size(16,12);
        }

                #endregion //GetDefaultMinimumItemSize	
    
            #region GetHorizontalTileAreaAlignment






		internal HorizontalAlignment GetHorizontalTileAreaAlignment()
        {
			if (_owner == null)
				return System.Windows.HorizontalAlignment.Left;

			if (_owner.IsInMaximizedMode)
				return this.MaximizedModeSettingsSafe.HorizontalTileAreaAlignment;
			else
				return this.NormalModeSettingsSafe.HorizontalTileAreaAlignment;
		}

                #endregion //GetHorizontalTileAreaAlignment	
    
            #region GetInterTileAreaSpacing






		internal double GetInterTileAreaSpacing()
        {
			if (_owner == null)
				return 0;

			double spacing = _owner.InterTileAreaSpacingResolved;

			// if we are showing the splitter then make sure we allow room for it
			if (this.MaximizedModeSettingsSafe.ShowTileAreaSplitter)
				spacing = Math.Max(_owner.SplitterMinExtent, spacing);

			return spacing;
		}

                #endregion //GetInterTileAreaSpacing	
    
            #region GetInterTileSpacing



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal double GetInterTileSpacing(bool vertical, TileState state)
        {
			if (_owner == null)
				return 0;

			switch (state)
			{
				case TileState.Normal:
					if (vertical)
						return _owner.InterTileSpacingY;
					else
						return _owner.InterTileSpacingX;

				case TileState.Maximized:
					if (vertical)
						return _owner.InterTileSpacingYMaximizedResolved;
					else
						return _owner.InterTileSpacingXMaximizedResolved;

				case TileState.Minimized:
				case TileState.MinimizedExpanded:
					if (vertical)
						return _owner.InterTileSpacingYMinimizedResolved;
					else
						return _owner.InterTileSpacingXMinimizedResolved;
			}

			return 0;
		}

                #endregion //GetInterTileSpacing	

			#region GetSplitterRect

		internal Rect GetSplitterRect()
		{
			if (!this.Manager.IsInMaximizedMode)
				return Rect.Empty;

			Rect area1 = this.LayoutManager.GetMaximizedModeTileArea(TileLayoutManager.TileArea.MaximizedTiles, false);
			Rect area2 = this.LayoutManager.GetMaximizedModeTileArea(TileLayoutManager.TileArea.MinimizedTiles, false);

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

			splitterRect.Intersect(this.GetTileArea());

			return splitterRect;
		}

			#endregion //GetSplitterRect	

            #region GetTargetRectOfItem



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal Rect GetTargetRectOfItem(object item)
        {
            TileLayoutManager tm = this.LayoutManager;

            TileLayoutManager.LayoutItem li = tm.GetLayoutItem(item);

			// JJD 10/18/11 - TFS92439
			// Check to make sure we are still wired to an owner XamTileManager
			if (li == null ||
                li.Container == null ||
                _owner == null ||
                !tm.IsContainerInArrangeCache(li.Container))
                return Rect.Empty;

            // Calculate an offset to use when arranging the items from the
            // tile area padding and any scroll offset
            Rect rect = li.TargetRect;

            Thickness tileAreaPadding = this._owner.TileAreaPadding;

            Vector offset;
            ItemTileInfo info = li.ItemInfo;

            if (info != null && info.IsMaximized)
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



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal Rect GetTargetRectOfContainer(XamTile tile)
        {
            TileLayoutManager tm = this.LayoutManager;

            object item = _owner.ItemFromTile(tile as XamTile);

            if (item == null )
                return Rect.Empty;

            return this.GetTargetRectOfItem(item);
        }

                #endregion //GetTargetRectOfContainer

            #region GetTileArea



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal Rect GetTileArea()
        {

			
            //Rect tileArea = new Rect( this.DesiredSize);
            // Initialize a rect with a size of the TileAreaSize that was
            // calculated during the last measure
            Rect tileArea = new Rect(new Point(), this.TileAreaSize);
            
			// JJD 10/18/11 - TFS92439
			// Check to make sure we are still wired to an owner XamTileManager
			Thickness padding = _owner != null ? this._owner.TileAreaPadding : new Thickness();

            // offset the rect based on the padding
            tileArea.X += padding.Left;
            tileArea.Y += padding.Top;
 
            
            
            

            return tileArea;
        }

            #endregion //GetTileArea	
    
            #region GetVerticalTileAreaAlignment






		internal VerticalAlignment GetVerticalTileAreaAlignment()
        {
			if ( _owner == null )
				return VerticalAlignment.Top;

			if (this._owner.IsInMaximizedMode)
				return this.MaximizedModeSettingsSafe.VerticalTileAreaAlignment;
			else
				return this.NormalModeSettingsSafe.VerticalTileAreaAlignment;
		}

            #endregion //GetVerticalTileAreaAlignment	

            #region MoveTile



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		internal bool MoveTile(XamTile tile, object item, Point point)
        {
            TileLayoutManager tm = this.LayoutManager;

            TileLayoutManager.LayoutItem sourceItem = tm.GetLayoutItem(item, tile, false, false);

            if (sourceItem == null)
                return false;

            TileLayoutManager.LayoutItem targetItem = tm.GetLayoutItemFromPoint(point, sourceItem);

            if (targetItem == null)
                return false;

            return tm.MoveTileHelper(sourceItem, targetItem, false);
        }

            #endregion //MoveTile	

			#region MoveTileHelper

		internal bool MoveTileHelper(XamTile container, object item, Point ptInScreenCoordinates)
		{
			if (_owner == null)
				return false;

			TileDraggingEventArgs args = new TileDraggingEventArgs(container, item);

			// raise the TileDragging event
			_owner.RaiseTileDragging(args);

			// if canceled return false
			if (args.Cancel)
				return false;


			// JJD 03/28/12 - TFS103699
			// Instead of caling PointFromScreen (which will throw an exception in a low-trust xbap application)
			// call PointFromScreenSafe.
			//Point pt = this.PointFromScreen(ptInScreenCoordinates);
			Point pt = Infragistics.Windows.Utilities.PointFromScreenSafe(this, ptInScreenCoordinates);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			return this.MoveTile(container, item, pt);
		}

			#endregion //MoveTileHelper	

			#region OnAnimationEnded







		internal void OnAnimationEnded(XamTile tile)
		{
			if (tile != null)
				tile.OnAnimationEnded();
		}

			#endregion //OnAnimationEnded

            #region OnEndDrag

        internal void OnEndDrag(FrameworkElement container)
        {
            container.ClearValue(XamTileManager.IsDraggingPropertyKey);
        }

            #endregion //OnEndDrag	

			// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
            #region OnPanComplete

		internal void OnPanComplete()
        {
			_panningPixelOffsetX = 0;
			_panningPixelOffsetY = 0;
			this.InvalidateMeasure();
        }

			#endregion //OnPanComplete

			#region ProcessLostMouseCapture







		internal void ProcessLostMouseCapture(MouseEventArgs e)
		{
			if (this._dragManager != null)
			{
				this._dragManager.OnDragEnd(e, true);

				this._dragManager = null;

				this._isDragging = false;

				// JJD 1/6/12 - TFS98924 
				// Resume resizing mouse move processing
				_owner._resizeController.ResumeMouseMoveProcessing();


				e.Handled = true;

			}
		}

			#endregion //ProcessLostMouseCapture
		
			#region ProcessMouseLeftButtonUp







		internal bool ProcessMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (this._dragManager == null)
				return false;

			TileDragManager tdm = this._dragManager;
			this._dragManager = null;
			this._isDragging = false;

			// JJD 1/6/12 - TFS98924 
			// Resume resizing mouse move processing
			_owner._resizeController.ResumeMouseMoveProcessing();

			this.ReleaseMouseCapture();

			tdm.OnDragEnd(e, false);

			e.Handled = true;

			this.InvalidateArrange();

			return true;
		}

			#endregion //ProcessMouseLeftButtonUp

			#region ProcessMouseMove







		internal bool ProcessMouseMove(MouseEventArgs e)
		{
			if (this._dragManager == null)
				return false;

			this._dragManager.OnMouseMove(e);


			e.Handled = true;


			return true;
		}

			#endregion //ProcessMouseMove	
    
			#region RemoveExtraChild

		internal void RemoveExtraChild(FrameworkElement child)
		{

			if (child == _splitter)
			{
				int idx = this.Children.IndexOf(child);
			
				Debug.Assert(idx >= 0, "Splitter not found in children collection");

				if (idx >= 0)
					this.Children.RemoveAt(idx);

				_splitter = null;

				return;
			}

			Debug.Assert(_extraChildrenCanvas != null, "Extra child list not allocated");

			if (_extraChildrenCanvas == null)
				return;

			int index = _extraChildrenCanvas.Children.IndexOf(child);

			Debug.Assert(index >= 0, "Extra child not found in extra child list");

			if (index >= 0)
				_extraChildrenCanvas.Children.RemoveAt(index);


		}

			#endregion //RemoveExtraChild	
    
			#region StartDrag

		// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
		// Changed signature to take a point instead of the MouseEventArgs
		//internal bool StartDrag(XamTile tile, MouseEventArgs e)
		internal bool StartDrag(XamTile tile, Point point)
		{
			if (_owner == null)
				return false;

			TileDraggingEventArgs args = new TileDraggingEventArgs(tile, _owner.ItemFromTile(tile));

			// raise the TileDragging event
			this._owner.RaiseTileDragging(args);

			// if canceled return false
			if (args.Cancel)
				return false;

			object item = _owner.ItemFromTile(tile);

			TileLayoutManager lm = this._owner.LayoutManager;

			if (lm == null)
				return false;

			ItemTileInfo info = lm.GetItemInfo(item);

			if (info == null)
				return false;

			AllowTileDragging allowTileDragging = this.GetAllowTileDragging(tile, info);

			if (allowTileDragging == AllowTileDragging.No)
				return false;

			if (allowTileDragging == AllowTileDragging.Slide)
			{
				if (_owner.IsInMaximizedMode == false &&
					this.NormalModeSettingsSafe.TileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile)
					allowTileDragging = AllowTileDragging.Swap;
			}

			this.CaptureMouse();

			TileLayoutManager.LayoutItem li = lm.GetLayoutItem(item, tile, true, false);

			this._dragManager = new TileDragManager(this, tile, info, li, tile, allowTileDragging);
			
			// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
			// Changed signature to take a point instead of the MouseEventArgs
			//this._dragManager.StartDrag(e);
			this._dragManager.StartDrag(point);

			// arrange the tile out of view
			if (li != null)
			{
				Rect trect = li.TargetRect;

				Size tsize = trect.Size;





				if (!tile.IsMeasureValid)

					tile.Measure(tsize);

			// JJD 12/22/11 - TFS97685
			// Only call the tile's Arrange method in WPF since it doesn't do anything
			// in SL outside of the panel's ArrangeOverride

				tile.Arrange(new Rect(new Point(-100000, -100000), tsize));
			}
			else
			{
				tile.Arrange(new Rect(new Point(-100000, -100000), tile.DesiredSize));
			}


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			tile.SetValue(XamTileManager.IsDraggingPropertyKey, KnownBoxes.TrueBox);

			this._isDragging = true;

			// JJD 1/6/12 - TFS98924 
			// Suspend resizing mouse move processing
			_owner._resizeController.SuspendMouseMoveProcessing();

			return true;
		}

			#endregion //StartDrag	
 
            #endregion //Internal Methods
    
            #region Protected Virtual Methods
     
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
			// JJD 10/18/11 - TFS92439
			// Check to make sure we are still wired to an owner XamTileManager
			if ( _owner == null )
				return false;


            // JJD 2/24/10 - TFS27654
            // If we aren't called from arrange and arrange is valid then wait
            // for the next arrange call
            if ( calledFromArrange == false &&
                this.IsArrangeValid == false)
                return false;

			// Check the _bypassNextFactorChange flag. If set to true reset it since we
			// want to ignore the 1st of 2 factor changes (i.e. the Reposition one)
			// so the other factor wasn't treated as 1.0 before the second animation
			// (the Resize one) had a chance to initialize its from (0.0) value.
			// This was only a problem in SL but we are doing it in WPF in case the
			// WPF framework changes how it deals with animations
			if (_bypassNextFactorChange && false == calledFromArrange)
			{
				_bypassNextFactorChange = false;
				return false;
			}

            Thickness tileAreaPadding = this._owner.TileAreaPadding;

            TileLayoutManager manager = this.LayoutManager;

            // Calculate an offset to use when arranging the items from the
            // tile area padding and any scroll offset
			Vector arrangeOffset = new Vector(tileAreaPadding.Left, tileAreaPadding.Top) + manager.ArrangeOffset;
			
			// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
			// Add panning pixel offsets to the arrange offset
            Vector extraPixelOffset = new Vector(_panningPixelOffsetX, _panningPixelOffsetY);

            // Calculate an offset to use when arranging the maximized items from the
            // tile area padding and any scroll offset
            Vector arrangeOffsetMaximized = new Vector(tileAreaPadding.Left, tileAreaPadding.Top) + manager.ArrangeOffsetForMaxmizedTiles;
            
            Rect panelRect = new Rect(new Point(), this._lastArrangeSize);

            panelRect.X += arrangeOffset.X;
            panelRect.Y += arrangeOffset.Y;

            panelRect.Width = Math.Max(panelRect.Width - (tileAreaPadding.Left + tileAreaPadding.Right), 0);
            panelRect.Height = Math.Max(panelRect.Height - (tileAreaPadding.Top + tileAreaPadding.Bottom), 0);

            panelRect.Width = Math.Max(panelRect.Width, this._tileAreaSize.Width);
            panelRect.Height = Math.Max(panelRect.Height, this._tileAreaSize.Height);
            
            Vector tileRemoveOffset = new Vector(-100000, -100000);
            
            double repositionFactor = this.RepositionFactor;
            double resizeFactor = this.ResizeFactor;

            if (CoreUtilities.AreClose(repositionFactor, 1.0d))
                repositionFactor = 1.0;

            if (CoreUtilities.AreClose(resizeFactor, 1.0d))
                resizeFactor = 1.0;

            //Debug.WriteLine("Reposition factor: " + repositionFactor);
            //Debug.WriteLine("Resize factor: " + resizeFactor);

            IEnumerator enumerator = this._owner.LayoutManager.GetItemsToBeArranged();

            Stack<TileLayoutManager.LayoutItem> itemsToRemove = null;

			FrameworkElement containerBeingDragged = this._dragManager != null ? this._dragManager.Tile : null;
 
            bool shouldAnimate = bypassAnimations == false && this._owner.ShouldAnimate;

            bool removeTilesNotInView = animationsEnded || shouldAnimate == false;

            if (removeTilesNotInView == true)
            {
                if (this._owner.IsInMaximizedMode )
                {
                    // In maximized mode if we are showing all minimized tiles then
                    // we never want to remove an item, even if is is out of view 
                    // which can only happen based on min size restrictions.
                    if (this._owner.MaximizedModeSettingsSafe.ShowAllMinimizedTiles)
                        removeTilesNotInView = false;
                }
                else
                {
                    // In normal mode if we are showing all tiles or the layout order is explicit 
                    // then we never want to remove an item, even if is is out of view
					NormalModeSettings normalSettings = this.NormalModeSettingsSafe;
					if (normalSettings.ShowAllTiles ||
						normalSettings.TileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile)
                        removeTilesNotInView = false;
                }

            }

            bool isAnimatingSizeChange = false;
            bool requiresAnimation = false;

            List<TileLayoutManager.LayoutItem> layoutItems = new List<TileLayoutManager.LayoutItem>();

            // JJD 2/12/10 - TFS26775
            // Copy the items into an array since it is possible for UpdateLayout to be called
            // from inside the process loop below which would invalidate the enumerator
            while (enumerator.MoveNext())
            {
                TileLayoutManager.LayoutItem li = enumerator.Current as TileLayoutManager.LayoutItem;

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
                    TileLayoutManager.LayoutItem li = layoutItems[i];

                    if (li != null)
                    {
                        XamTile tile = li.Container;

                        // bypass a tile that is being dragged since it was positioned out
                        // of sight at the start of the drag operation
						if (tile == containerBeingDragged)
						{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

							continue;
						}

						Rect targetRct = li.TargetRect;

						Size targetSize = new Size(targetRct.Width, targetRct.Height);

                        if (tile.Visibility == Visibility.Collapsed ||
                            targetSize.Width < 1 || targetSize.Height < 1)
                        {
                            
                            // If the element is not visible we don't need to measure and arrange it
							if (tile.Visibility != Visibility.Collapsed)
							{
								if ( targetSize.Width > 0 && targetSize.Height > 0)
									li.CurrentRect = targetRct;


								if (!tile.IsMeasureValid)
								{
									// JJD 12/20/10 - TFS61057/TFS61064
									// Use the panel's available size when measuring instead of infinity
									//element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
									tile.Measure(this._lastMeasureSize);
								}


								tile.Arrange(new Rect(new Point(tileRemoveOffset.X, tileRemoveOffset.Y), tile.DesiredSize));
							}

							// if we have an attached element check to see if the item is still in the list.
							// If so don't remove it
							if (((ISupportRecycling)li).AttachedElement != null)
							{
								ItemTileInfo tileinfo = this._owner.LayoutManager.GetItemInfo(li.Item, false, -1);

								if (tileinfo != null && tileinfo.Index >= 0)
									continue;
							}

							
							// Don't queue tiles that are not generated for removal
							if (tile != null &&
								 tile == li.Item)
							{
								// JJD 03/14/12 - TFS100150 - Added touch support
								// Flag the layoutItem that we no longer want to arrange it in view
								li.ShouldArrangeInView = false;
								continue;
							}

                            if (itemsToRemove == null)
                                itemsToRemove = new Stack<TileLayoutManager.LayoutItem>();

                            itemsToRemove.Push(li);

                            continue;
                        }

                        // let the tile now to clean up any cached images
						// JJD 4/22/11 - TFS63921
						// Use the animationsEnded or shouldAnimate flags instead of removeTilesNotInView
						// since that flag can be set to false if the all tiles in view option is set
                        //if (removeTilesNotInView)
						if (animationsEnded || shouldAnimate == false)
                            this.OnAnimationEnded(tile);

                        // note: we can only animate tiles
                        if (shouldAnimate && tile != null)
                        {
                            bool isCurrentSameAsTarget = li.IsCurrentSameAsTarget;

                            if (requiresAnimation == false)
                                requiresAnimation = !isCurrentSameAsTarget;

                            if (calledFromArrange == false &&
                                isCurrentSameAsTarget == false)
                            {
                                // Access the current rect first because in certain situations
                                // it will initialize the OriginalRect, e.g. when a new tile
                                // is entering the firs time
                                Rect currentRect = li.CurrentRect;
                                Rect originalRect = li.OriginalRect;
								Rect targetRect = targetRct;

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
								{

									currentRect.Size = targetRect.Size;




								}

                                li.CurrentRect = currentRect;
                            }
                        }
                        else
							li.CurrentRect = targetRct;


						Rect itemRect = Rect.Offset(li.CurrentRect, arrangeOffset);





                        ItemTileInfo info = li.ItemInfo;

                        bool isMaximized = info != null && info.IsMaximized;

                        Vector offset = isMaximized ? arrangeOffsetMaximized : arrangeOffset;

                        bool removeTile = false;


						// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
						// See if the targetRect intersects with the panel. 
						bool isTargetRectinPanel = panelRect.IntersectsWith(Rect.Offset(targetRct, arrangeOffset));


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

						// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
						// If target rect intersects with the panel and the tile is not maximized
						// and se still want to arrange it in view
						// then offset the item rect by the extra pixel offset
						if (isTargetRectinPanel  && false == isMaximized && li.ShouldArrangeInView)
							offset += extraPixelOffset;

						if (removeTilesNotInView)
                        {
                            if (!isMaximized)
                            {
                                // see if the item rect intersects with the panel
                                
                                // remove the tile if the itemRect and target rect don't intersect
                                

								// JJD 03/06/12 - TFS100150 
								// Use isTargetRectinPanel stack variaable initialized above
								//removeTile = !panelRect.IntersectsWith(itemRect) &&
								//             !panelRect.IntersectsWith(Rect.Offset(targetRct, arrangeOffset));
								removeTile = false == isTargetRectinPanel &&
											 !panelRect.IntersectsWith(itemRect);


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

							}
                        }

                       // keep track of items that are no longer inside the panel's rect
                        // so we can remove them from the cache below
                        if (tile == null || removeTile)
                        {
							
							// Only queue tiles that are not generated for removal
							if (tile == null &&
								 tile != li.Item)
							{
								if (itemsToRemove == null)
									itemsToRemove = new Stack<TileLayoutManager.LayoutItem>();

								itemsToRemove.Push(li);
							}
							else
							{
								// JJD 03/14/12 - TFS100150 - Added touch support
								// Flag the layoutItem that we no longer want to arrange it in view
								li.ShouldArrangeInView = false;
							}

                            offset = tileRemoveOffset;

							itemRect = targetRct;

							itemRect.X += offset.X;
                            itemRect.Y += offset.Y;

                            li.CurrentRect = targetRct;
                        }

                        if (tile != null)
                        {
                            // we always arrange tiles at 0,0 and then use 
                            // the Tile's RenderTransform to position and size it properly

                            if (calledFromArrange || tile.IsArrangeValid == false)



								tile.Arrange(new Rect(0, 0, targetRct.Width, targetRct.Height));

                            // JJD 2/24/10
                            // On the arrange pass initialize the original rect since we will
                            // always restart the animations is necessary
                            if (calledFromArrange)
                                li.SetOriginalRect(li.CurrentRect, true);

							tile.UpdateTransform(li.OriginalRect, li.CurrentRect, targetRct, offset, resizeFactor, calledFromArrange);
                        }
                    }
                }

                if (itemsToRemove != null)
                {
                    // remove old items from the manager's cache
                    while (itemsToRemove.Count > 0)
                        manager.RemoveLayoutItem(itemsToRemove.Pop(), false);
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

        private Storyboard AnimateDoubleProperty(DependencyProperty dp, Timeline animation)
        {

			animation = animation.Clone();


			Storyboard sb = new Storyboard();

			if ( dp == ResizeFactorProperty )
				sb.Completed += new EventHandler(OnAnimationCompleted_Resize);
			else
				sb.Completed += new EventHandler(OnAnimationCompleted_Reposition);

			sb.FillBehavior = FillBehavior.Stop;

			Storyboard.SetTargetProperty(animation, new PropertyPath(dp));
			
			sb.Children.Add(animation);
 
            this._activeAnimationCount++;

			if (_activeAnimationCount == 1)
				_owner.SyncIsAnimationInProgress();

			Storyboard.SetTarget(animation, this);


			sb.Begin(this, HandoffBehavior.Compose, true);




            return sb;
        }

                #endregion //AnimateDoubleProperty	
    
                #region AnimateRepositionAndResize

        private void AnimateRepositionAndResize()
        {

            if ( _owner == null || _owner.ShouldAnimate == false)
                return;

			// Set the _bypassNextFactorChange flag to true. Since we
			// want to ignore the 1st of 2 factor changes (i.e. the Reposition one)
			// so the other factor won't be treated as 1.0 before the second animation
			// (the Resize one) had a chance to initialize its from (0.0) value.
			// This was only a problem in SL but we are doing it in WPF in case the
			// WPF framework changes how it deals with animations
			_bypassNextFactorChange = true;

            if (this._repositionStoryBoard != null)
            {




                this.UnhookAnimation(this._repositionStoryBoard);
            }

            if (this._resizeStoryBoard != null)
            {




				this.UnhookAnimation(this._resizeStoryBoard);
            }

            Timeline animation = _owner.GetRepositionAnimation();

			if (animation != null)
			{






				this._repositionStoryBoard = this.AnimateDoubleProperty(TileAreaPanel.RepositionFactorProperty, animation);
			}

            animation = _owner.GetResizeAnimation();

            if (animation != null)
			{






                this._resizeStoryBoard = this.AnimateDoubleProperty(TileAreaPanel.ResizeFactorProperty, animation);
			}

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

				#region GetMamizedAreaMinExtent

		private double GetMamizedAreaMinExtent(bool isSplitterVertical)
		{
			ITileConstraints constraints = this.LayoutManager.GetMaximizedModeTileAreaConstraints(TileLayoutManager.TileArea.MaximizedTiles);

			if (constraints == null)
				return 0;

			if (isSplitterVertical)
				return constraints.MinWidth;
			else
				return constraints.MinHeight;
		}

				#endregion //GetMamizedAreaMinExtent	
    
				#region GetMinimizedAreaMaxExtent

		private double GetMinimizedAreaMaxExtent(bool isSplitterVertical)
		{
			ITileConstraints constraints = this.LayoutManager.GetMaximizedModeTileAreaConstraints(TileLayoutManager.TileArea.MinimizedTiles);

			if (constraints == null)
				return double.PositiveInfinity;

			if (isSplitterVertical)
				return constraints.MaxWidth;
			else
				return constraints.MaxHeight;
		}

				#endregion //GetMinimizedAreaMaxExtent	
    
                #region OnAnimationCompleted_Resize

        private void OnAnimationCompleted_Resize(object sender, EventArgs e)
        {
            this.UnhookAnimation(_resizeStoryBoard);
        }

                #endregion //OnAnimationCompleted_Resize	

                #region OnAnimationCompleted_Reposition

        private void OnAnimationCompleted_Reposition(object sender, EventArgs e)
        {
            this.UnhookAnimation(_repositionStoryBoard);
        }

                #endregion //OnAnimationCompleted_Reposition	

                #region OnArrangeComplete

        private void OnArrangeComplete()
        {
            if (this._inViewItemsHaveChanged)
            {
                this._inViewItemsHaveChanged = false;

				if (_owner != null )
					this._owner.OnItemsInViewChanged();

            }

            
            
            

        }

                #endregion //OnArrangeComplete	
    
                #region OnConstraintPropertyChanged

        private static void OnConstraintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement v = d as UIElement;
            TileAreaPanel panel = null != v ? VisualTreeHelper.GetParent(v) as TileAreaPanel : null;
            if (null != panel)
                panel.InvalidateMeasure();
        }

                #endregion // OnConstraintPropertyChanged

                #region OnFactorChanged

        private void OnFactorChanged()
        {
            if (this._repositionFactor < .1 && this._resizeFactor < .1)
                this.ProcessFactorChange();
            else
            {
                if (this._processFactorChangePending == false)
                {
					this._processFactorChangePending = true;


					this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new TileUtilities.MethodDelegate(this.ProcessFactorChange));




                }
            }
        }

                #endregion //OnFactorChanged	

                #region ProcessFactorChange

        private void ProcessFactorChange()
        {
            this._processFactorChangePending = false;

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
            this._asynchAnimationStartPending = false;

            
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

		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling	
		// Added overload for panning scroll
		private void SetHorizontalOffset(double offset)
		{
			this.SetHorizontalOffset(offset, false);
		}
		internal void SetHorizontalOffset(double offset, bool isPanningScroll)
		{
			if (this.IsScrollClient)
			{
                TileLayoutManager manager = this.LayoutManager;

				// JJD 02/22/12 - TFS100150 - Added touch support for scrolling	
				// hold the offset in a stack variable
				double originalOffset = offset;

                // if we are scrolling tiles in this dimension then round to offset
 				
				
                //if (manager.ScrollTilesVertically)
                if (manager.ScrollTilesHorizontally)
				{
					if (isPanningScroll)
						offset = Math.Floor(offset);
					else
						offset = TileUtilities.RoundToIntegerValue(offset);
				}

				// JJD 02/22/12 - TFS100150 - Added touch support for scrolling	
				// reset the panning pixel offset in thos dimesnion
				_panningPixelOffsetX = 0;

                double newOffset = Math.Max(Math.Min(manager.ScrollMaxOffset.X, offset), 0);

				// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
				// If called from the pan scroll logic. Take the delta (decimal porion only)
				// between the original offset and the rounded property we will use for the
				// scroll offset and then calculate the pixel adjustment based on the
				// first item extent in this dimension time the delta.
				if (isPanningScroll)
				{
					if (originalOffset != newOffset && newOffset == offset)
					{
						double itemWidth = this._owner.GetFirstItemWidth();
						_panningPixelOffsetX = itemWidth * (newOffset - originalOffset);
					}
				}

				
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


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

		// JJD 02/22/12 - TFS100150 - Added touch support for scrolling	
		// Added overload for panning scroll
		private void SetVerticalOffset(double offset)
		{
			this.SetVerticalOffset(offset, false);
		}
		internal void SetVerticalOffset(double offset, bool isPanningScroll)
		{
			if (this.IsScrollClient)
			{
                TileLayoutManager manager = this.LayoutManager;

				// JJD 02/22/12 - TFS100150 - Added touch support for scrolling	
				// hold the offset in a stack variable
				double originalOffset = offset;

                // if we are scrolling tiles in this dimension then round to offset 
                if (manager.ScrollTilesVertically)
				{
					if (isPanningScroll)
						offset = Math.Floor(offset);
					else
						offset = TileUtilities.RoundToIntegerValue(offset);
				}

				// JJD 02/22/12 - TFS100150 - Added touch support for scrolling	
				// reset the panning pixel offset in thos dimesnion
				_panningPixelOffsetY = 0;

				double newOffset = Math.Max(Math.Min(manager.ScrollMaxOffset.Y, offset), 0);

				// JJD 02/22/12 - TFS100150 - Added touch support for scrolling
				// If called from the pan scroll logic. Take the delta (decimal porion only)
				// between the original offset and the rounded property we will use for the
				// scroll offset and then calculate the pixel adjustment based on the
				// first item extent in this dimension time the delta.
				if (isPanningScroll)
				{
					if (originalOffset != newOffset && newOffset == offset)
					{
						double itemHeight = this._owner.GetFirstItemHeight();
						_panningPixelOffsetY = itemHeight * (newOffset - originalOffset);
					}
				}

				
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


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

        private void UnhookAnimation(Storyboard storyBoard)
        {
            Debug.Assert(storyBoard != null);

            if ( storyBoard == null )
                return;

			//if (storyBoard.GetCurrentState() != ClockState.Stopped)
				storyBoard.Stop();

            DependencyProperty dp = null;

            if (storyBoard == this._repositionStoryBoard)
            {
                dp = RepositionFactorProperty;
                this._repositionStoryBoard = null;
				storyBoard.Completed -= new EventHandler(OnAnimationCompleted_Reposition);
            }
            else
            if (storyBoard == this._resizeStoryBoard)
            {
                dp = ResizeFactorProperty;
                this._resizeStoryBoard = null;
				storyBoard.Completed -= new EventHandler(OnAnimationCompleted_Resize);
            }
            else
            {
                Debug.Assert(false, "Unknown storyboard");
            }

			storyBoard.Children.Clear();

            if (dp != null)
            {
                Debug.Assert(this._activeAnimationCount > 0, "activeAnimationCount should be greater than 0");
                this._activeAnimationCount--;

				// JJD 10/18/11 - TFS92439
				// Check to make sure we are still wired to an owner XamTileManager
				//if (_activeAnimationCount == 0)
				if (_activeAnimationCount == 0 && _owner != null)
					_owner.SyncIsAnimationInProgress();

                this.ClearValue(dp);

				// JJD 10/18/11 - TFS92439
				// Check to make sure we are still wired to an owner XamTileManager
                //if (this._activeAnimationCount < 1)
                if (this._activeAnimationCount < 1 && _owner != null)
                    this.ProcessAllAnimationsEnded();
            }
        }

                #endregion //UnhookAnimation	

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
            this.AdjustVerticalOffset(this.LayoutManager.ScrollSmallChange.Y);
        }

        void IScrollInfo.LineLeft()
        {
            this.AdjustHorizontalOffset(-this.LayoutManager.ScrollSmallChange.X);
        }

        void IScrollInfo.LineRight()
        {
            this.AdjustHorizontalOffset(this.LayoutManager.ScrollSmallChange.X);
        }

        void IScrollInfo.LineUp()
        {
            this.AdjustVerticalOffset(-this.LayoutManager.ScrollSmallChange.Y);
        }


        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)



        {
            if (this.IsScrollClient)
                return this.LayoutManager.MakeVisible(visual, rectangle);

            return rectangle;
        }

        void IScrollInfo.MouseWheelDown()
        {

            this.AdjustVerticalOffset(SystemParameters.WheelScrollLines * this.LayoutManager.ScrollSmallChange.Y);



        }

        void IScrollInfo.MouseWheelLeft()
        {

            this.AdjustHorizontalOffset(-SystemParameters.WheelScrollLines * this.LayoutManager.ScrollSmallChange.X);



        }

        void IScrollInfo.MouseWheelRight()
        {

            this.AdjustHorizontalOffset(SystemParameters.WheelScrollLines * this.LayoutManager.ScrollSmallChange.X);



        }

        void IScrollInfo.MouseWheelUp()
        {

            this.AdjustVerticalOffset(-SystemParameters.WheelScrollLines * this.LayoutManager.ScrollSmallChange.Y);



        }

        void IScrollInfo.PageDown()
        {
            this.AdjustVerticalOffset(this.LayoutManager.ScrollLargeChange.Y);
        }

        void IScrollInfo.PageLeft()
        {
            this.AdjustHorizontalOffset(-this.LayoutManager.ScrollLargeChange.X);
        }

        void IScrollInfo.PageRight()
        {
            this.AdjustHorizontalOffset(this.LayoutManager.ScrollLargeChange.X);
        }

        void IScrollInfo.PageUp()
        {
            this.AdjustVerticalOffset(-this.LayoutManager.ScrollLargeChange.Y);
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

            internal TileAreaPanel _panel;
            private XamTileManager _owningManager;
            private TileLayoutManager _layoutManager;
            private IList _childElements;
            private int _childElementCount;
            private Size _constraint;
            private Size _measureConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
            internal HashSet<object> _itemsExpectedToBeGenerated;
            private int _currentGeneratorPositionIndex;
            private int _nextChildElementIndex;
            internal GridBagLayoutManager _previousGridBagLayoutNormalModeOverall;
            internal Dictionary<DependencyObject, TileLayoutManager.LayoutItem> _previousItemsToBeArrangedTempCache;
            internal HashSet<object> _newItemsToBeArrangedTempCache;
            internal Size _desiredSize;
            internal bool _requiresAnotherLayoutPass;





            #endregion //Private Members

            #region Constructor

            internal GenerationCache(TileAreaPanel panel,
                                    Size constraint)
            {
                this._panel = panel;
				this._owningManager = panel.Manager;
                this._layoutManager = panel.LayoutManager;
                this._constraint = constraint;
                this._childElements = this._panel.Children;
                this._childElementCount = this._childElements.Count;
                
                // Clear the expected items list since we don't need it anymore
                this._itemsExpectedToBeGenerated = null;
				
				ScrollDirection scrollDirection = this._layoutManager.BeginGeneration(this);




             }

            #endregion //Constructor

            #region Properties

            #endregion //Properties

            #region Methods

                #region Internal Methods

            internal void GenerateElements()
            {
                this._layoutManager.GenerateElements(this);
            }

                #endregion //Internal Methods

                #region Private Methods
            
                    #region GenerateElement

            internal XamTile GenerateElement(int indexToGen)
            {








                bool isNewlyRealized;

                //// generate the next element
				XamTile generatedTile = this.GenerateTile(indexToGen, out isNewlyRealized);

                // bump the _currentGeneratorPosition  so we know which element will be generated
                // the next time GenerateNext is called and can compare it above to restart the generator
                // if the indexToGen doesn't match
                this._currentGeneratorPositionIndex++;

                Debug.Assert(generatedTile != null, "The element should have been generated.");

                if (generatedTile == null)
                    return null;


                // If the generated item is 'newly realized', add it to our children collection
                // and 'prepare' it.
                if (isNewlyRealized)
                {
                     // update the childcount
                    this._childElementCount++;
                    




                }

                // Bump _nextChildElementIndex so we know where to insert the next child element.
                if (this._nextChildElementIndex >= 0)
                    this._nextChildElementIndex++;


                return generatedTile;
            }

                    #endregion //GenerateNextElement

					#region GenerateTile

			private XamTile GenerateTile(int indexToGen, out bool newlyRealized)
			{
				object item = _owningManager.Items[indexToGen];

				if ((item is XamTile) && _owningManager.IsItemItsOwnContainerOverride(item))
				{
					DependencyObject parent = VisualTreeHelper.GetParent(item as UIElement);

					// cover the case where the template of the XamTileManager has changed and the XamTile is still
					// parented to the old panel
					if (parent != null && parent != _panel)
					{
						Panel oldPanel = parent as Panel;

						if (oldPanel != null)
						{
							oldPanel.Children.Remove(item as UIElement);
							parent = null;
						}
					}

					Debug.Assert(parent == null || parent == _panel, "Invalid visual parent for tile in GenerateTile");

					if (parent == null)
						_panel.Children.Add(item as UIElement);

					newlyRealized = false;

					// If the owner's ItemContainerStyle is set we should apply it here
					_owningManager.InitializeTileStyle(item as XamTile);

					return item as XamTile;
				}

				TileLayoutManager.LayoutItem li = _layoutManager.GetLayoutItem(item, null, true, false);

				newlyRealized = RecyclingManager.Manager.AttachElement(li, _panel);

				return li.Container;
			}

					#endregion //GenerateTile	
    
            #endregion //Private Methods

            #endregion //Methods

            #region IDisposable Members

            public void Dispose()
            {

                this._desiredSize = this._layoutManager.EndGeneration(this);



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

            private TileAreaPanel _panel;

			#endregion //Member Variables

            #region Constructor

            internal ScrollData(TileAreaPanel panel)
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
                this._panel.LayoutManager.OnScrollOffsetChanged(this._offset, oldOffset);
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

				bool isDifferent = false == TileLayoutManager.AreClose(this._viewport.Width, viewPort.Width) |
                    false == TileLayoutManager.AreClose(this._viewport.Height, viewPort.Height) ||
                    false == TileLayoutManager.AreClose(this._extent.Width, extent.Width) ||
                    false == TileLayoutManager.AreClose(this._extent.Height, extent.Height);

                Size oldviewPort    = this._viewport;
                Size oldExtent      = this._extent;

				this._viewport = viewPort;
				this._extent = extent;

                //Debug.WriteLine("VerfifyScrollData viewport: " + this._viewport.ToString() + ", extent: " + this._extent.ToString());
                //Debug.WriteLine("VerfifyScrollData offset before: " + this._offset.ToString());
				
                Vector oldOffset = this._offset;

				isDifferent |= this.VerifyOffset();
                
                if ( isDifferent )
                    this._panel.LayoutManager.OnScrollOffsetChanged(this._offset, oldOffset);

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
                return false == TileLayoutManager.AreClose(this._offset.X, oldOffset.X) ||
                    false == TileLayoutManager.AreClose(this._offset.Y, oldOffset.Y);
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
    
		#endregion //Nested Classes

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