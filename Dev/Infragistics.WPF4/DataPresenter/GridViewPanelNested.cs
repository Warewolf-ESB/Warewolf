using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Documents;
using System.ComponentModel;
using System.Diagnostics;

using Infragistics.Windows.Controls;
using Infragistics.Windows.Selection;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter.Events;
using System.Windows.Data;
using Infragistics.Windows.Scrolling;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Virtualization;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
    // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
    /// <summary>
    /// A <see cref="GridViewPanel"/> derived class used by the <see cref="DataPresenterBase"/> derived controls such as <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/> to arrange <see cref="RecordPresenter"/> instances in a tabular fashion, either horizontally or vertically.  The GridViewPanelNested supports nesting to display hierarchical data and a single unified scrolling capability for all nested panels.
    /// </summary>
    /// <remarks>
    /// <p class="note"><b>Note: </b>The GridViewPanelNested is designed to be used with the <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/> controls and is for Infragistics internal use only.  The control is not
    /// designed to be used outside of the <see cref="XamDataGrid"/> or <see cref="XamDataPresenter"/>.  You may experience undesired results if you try to do so.</p>
    /// </remarks>
    /// <seealso cref="XamDataGrid"/>
    /// <seealso cref="XamDataPresenter"/>
    /// <seealso cref="GridViewPanel"/>
    /// <seealso cref="GridViewPanelNested"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.GridViewSettings.UseNestedPanels"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public sealed class GridViewPanelNested : GridViewPanel
    {
        #region Member variables
        private GridViewPanelNested                 _rootPanel;
//		private DispatcherOperation					_queuedCleanupRequest;
		private DispatcherOperation					_queuedBottomRecordVerification;
		private FieldLayout							_fieldLayoutForHeaderRecordInAdorner;

		private int									_firstVisualScrollableItemIndex;
		private int									_firstVisualTopFixedItemIndex;
		private int									_firstVisualBottomFixedItemIndex;
		private int									_visibleItemCount;
		private int									_scrollPositionCandidate;	
		private int									_scrollPositionOfLastVisibleRecord;	

		private Size								_lastMeasureConstraint;

		private ScrollData							_cachedScrollDataFromMeasure;
		private double								_cachedRemainingAvailableExtentFromMeasure;
		private int									_lastViewportHeight;
		private int									_lastViewportWidth;

		// 5/21/07 - Optimization
		private int									_lastOverallScrollPosition;

        // AS 2/23/09 Optimization
        private int                                 _lastMeasureScrollPosition;

        // JJD 1/23/09 - NA 2009 vol 1 - record filtering
        // Keep track of the actual top record since we process the
        // filtering logic lazily as records get hydrated it is possible
        // that the scrollposition of the top record will change without the
        // user performing a scroll operation. In which case we want to
        // retain the current top record
        private Record                              _lastRecordAtOverallScrollPosition;

		// JJD 6/11/07 - cache the overall scroll count as well
		private int									_lastOverallScrollCount;

		// the offsets within list refer to index offsets within the list that
		// we are bound to. They are calculated during the measure and used during the arrange. 
		private int									_offsetWithinListForTopFixedRecords;
		private int									_offsetWithinListForBottomFixedRecords;
		private int									_offsetWithinListForScrollableRecords;
		private int									_numberOfScrollableRecordsToLayout;

		private bool								_useScrollPositionCandidate;
		private bool								_recordsInViewChanged;
		private bool								_lastOrientationWasVertical;

        // JJD 5/2/08 - BR32390
        // Added flag so we know whether to show scrollbars
        private bool                                _isRootPanelInInfiniteContainer;
		
		// JJD 6/6/07 added flags so we know if the last scrollable record is fully in view
		private bool								_isLastScrollableRecordFullyInView;
		private bool								_isScrollPositionCandidateFullyInView;
		
		// JJD 6/11/07 added anti-recursion flag
		private bool								_isProcessingBottomRecordVerification;

		private GridViewPanelAdorner				_gridViewPanelAdorner;
		private double								_extentUsedByFixedRecordsOnTop;
		private double								_extentUsedByFixedRecordsOnBottom ;
		private double								_extentUsedByHeaderRecords;
		private double								_widthInInfiniteContainer;
		private double								_heightInInfiniteContainer;

        // JJD 6/20/08 
        // Added cache for last extent so we don't oscillate between measures
        // with the height of the other scrollbar first taken out then
        // included
        private double                              _lastScrollableExtentUsedInMeasureOnRootPanel;

        // JJD 2/19/09 - TFS13979
        // cache the last sort version
        private int                                 _lastOverallSortVersion;

		// JM BR29008 12-11-07
		private Size								_lastBottomRecordVerificationAvailableSize = new Size(0, 0);
		private double								_lastBottomRecordVerificationScrollPos; 

		// AS 6/11/09 TFS18382
		private double								_nonPrimaryExtentUsedByHeaderRecords;

        // JJD 10/16/09 - TFS22652
        // Added tracker for nested panels so they can invalidate their measure when the scroll version changes
        private PropertyValueTracker                _tracker;
			// JJD 8/16/10 - TFS30240
        private PropertyValueTracker                _renderVersionTracker;

		// MD 4/27/11 - TFS36608
		private int									_lastVerifiedFiltersVersion;

		// MD 4/28/11 - TFS36608
		private int									_scrollPositionWhenNoChildrenAbovePresent = -1;

		// JJD 11/16/11 - TFS25239
		// Replaced wiring the DP's RecordsInViewChanged event will a PropertyValueTracker 
		// so we don't root this element
		private PropertyValueTracker				_rcdsInViewTracker;

		#endregion //Member Variables
    
        #region Base class overrides

            #region Properties
    
				#region EffectiveScrollPosition



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal override int EffectiveScrollPosition
		{
			get
			{
				GridViewPanelNested rootPanel = this.RootPanel as GridViewPanelNested;

				Debug.Assert(rootPanel != null);

				if (rootPanel == null)
					return 0;

				if (rootPanel._useScrollPositionCandidate)
					return rootPanel._scrollPositionCandidate;
				else
					return this.ViewPanelInfo.OverallScrollPosition;
			}
			set
			{
				GridViewPanelNested rootPanel = this.RootPanel as GridViewPanelNested;

				Debug.Assert(rootPanel != null);

				if (rootPanel == null)
					return;

				if (rootPanel._useScrollPositionCandidate)
				{
					rootPanel._scrollPositionCandidate = value;
					this.DataPresenter.BumpRecordsInViewVersion();
				}
				else
					this.ViewPanelInfo.OverallScrollPosition = value;
			}
		}

				#endregion EffectiveScrollPosition

				#region FirstScrollableRecord

		internal override Record FirstScrollableRecord
		{
			get
			{
				ViewableRecordCollection ourRecordCollection = this.ViewableRecords;

				// JJD 5/25/07
				// Check to make sure that there is a ViewableRecordCollection
				if (ourRecordCollection == null)
					return null;

				int totalRecordCount = ourRecordCollection.Count;

				if (totalRecordCount == 0)
					return null;

				int topFixedCount = ourRecordCollection.CountOfFixedRecordsOnTop;
				int bottomFixedCount = ourRecordCollection.CountOfFixedRecordsOnBottom;

				if (this._offsetWithinListForScrollableRecords >= totalRecordCount - (topFixedCount + bottomFixedCount))
				{
					if (this._offsetWithinListForTopFixedRecords >= topFixedCount)
					{
						return ourRecordCollection[this._offsetWithinListForTopFixedRecords];
					}
					else
					{
						if (this._offsetWithinListForBottomFixedRecords < bottomFixedCount)
							return ourRecordCollection[this._offsetWithinListForBottomFixedRecords + totalRecordCount - topFixedCount];
						else
							return null;
					}
				}
				else
				{
                    // JJD 2/26/09 - TFS14664
                    // check the scroll position and use that instead to account for nested records
                    // Otherwise we will return the root level record even if its content is
                    // scrolled out of view
                    IViewPanelInfo info = this.ViewPanelInfo;

                    if (info != null)
                    {
                        int scrollPos = info.OverallScrollPosition;

                        if (scrollPos >= topFixedCount)
                            return info.GetRecordAtOverallScrollPosition(scrollPos);
                    }

					return ourRecordCollection[topFixedCount + this._offsetWithinListForScrollableRecords];
				}
			}
		}

				#endregion //FirstScrollableRecord	

                #region IsRootPanel

        // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
        // Made abstract
        /// <summary>
        /// Returns true if this is the root panel
        /// </summary>
        public override bool IsRootPanel
        {
            get
            {
                DataPresenterBase dp = this.DataPresenter;

                // JJD 2/25/08 - BR30660
                // Use the CurrentPanel property of the DP as the first stab
                // at this. However, during initialization this may return null
                // so if it does then fallback to the prior methods of determining 
                // if this is the root panel.
                if (dp != null)
                {
                    Panel pnl = dp.CurrentPanel;

                    if (pnl != null)
                        return this == pnl;

                }

                RecordListControl rlc = this.RecordListControl;

                return rlc.ItemsSource ==
                        (rlc.DataPresenter.RecordManager.ViewableRecords as IEnumerable);
            }
        }

                #endregion //IsRootPanel

                #region IsRootPanelInInfiniteContainer

        internal override bool IsRootPanelInInfiniteContainer 
        {
            get { return this._isRootPanelInInfiniteContainer; }
        }

                #endregion //IsRootPanelInInfiniteContainer	

                #region RootPanel

        // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
        // Made public abstract
        /// <summary>
        /// Returns the root panel
        /// </summary>
        public override GridViewPanel RootPanel
        {
            get
            {
                if (this.IsRootPanel)
                    return this;

                // JJD 2/22/08 - BR30660
                // Since this is not the root panel then null out the cached
                // _rootPanel if it is equal to this
                else
                {
                    if (this._rootPanel == this)
                        this._rootPanel = null;
                }

                if (this._rootPanel == null)
                    this._rootPanel = this.DataPresenter.CurrentPanel as GridViewPanelNested;

                return this._rootPanel;
            }
        }

                #endregion RootPanel

                // JJD 1/27/09 - added
                #region RootPanelTopFixedOffset

        internal override int RootPanelTopFixedOffset
        {
            get
            {
                GridViewPanelNested rootPanel = this.RootPanel as GridViewPanelNested;

                if (rootPanel != null &&
                    rootPanel._offsetWithinListForTopFixedRecords == 0)
                {
                    ViewableRecordCollection rootVrc = rootPanel.ViewableRecords;

                    // JJD 7/13/09 - TFS19156
                    // Make sure viewablerecords is not null
                    if (rootVrc != null)
                        return rootVrc.CountOfFixedRecordsOnTop;
                }

                return 0;
            }
        }

                #endregion //RootPanelTopFixedOffset	

                #region ScrollPositionOfLastVisibleRecord

        internal override int ScrollPositionOfLastVisibleRecord
        {
            get { return this._scrollPositionOfLastVisibleRecord; }
        }

                #endregion //ScrollPositionOfLastVisibleRecord	
				
                // 5/21/07 - Optimization
				// Derive from RecyclingItemsPanel 
				#region TotalVisibleGeneratedItems

		/// <summary>
		/// Derived classes must return the number of visible generated items.
		/// </summary>
		protected override int TotalVisibleGeneratedItems
		{
			get
			{
				ViewableRecordCollection ourRecordCollection = this.ViewableRecords;

				// JJD 5/25/07
				// Check to make sure that there is a ViewableRecordCollection
				if (ourRecordCollection == null)
					return 0;

                // JJD 8/3/09 - TFS16793
                // Adjust for CountOfFixedRecordsOnBottom instead of CountOfSpecialRecordsOnBottom
                //return Math.Min(this.TotalItems - 1, (this._numberOfScrollableRecordsToLayout + ourRecordCollection.CountOfFixedRecordsOnTop + ourRecordCollection.CountOfSpecialRecordsOnBottom - (this._offsetWithinListForTopFixedRecords + this._offsetWithinListForBottomFixedRecords)));
                return Math.Min(this.TotalItems - 1, (this._numberOfScrollableRecordsToLayout + ourRecordCollection.CountOfFixedRecordsOnTop + ourRecordCollection.CountOfFixedRecordsOnBottom - (this._offsetWithinListForTopFixedRecords + this._offsetWithinListForBottomFixedRecords)));
			}
		}

				#endregion //TotalVisibleGeneratedItems	

            #endregion //Properties

            #region Methods

				#region ArrangeOverride

		/// <summary>
		/// Called to layout child elements.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The size used by this element to arrange its children.</returns>
		protected override System.Windows.Size ArrangeOverride(Size finalSize)
		{
			#region Setup

			base.ArrangeOverride(finalSize);

			GridViewSettings viewSettings = this.ViewSettings;

			if (viewSettings == null)
				return finalSize;

			bool orientationIsVertical = viewSettings.Orientation == Orientation.Vertical;

            // AS 2/20/09 TFS14305
            // We need to update the horizontal offset before arranging the elements and we
            // need to update the offset used by the adorner - which only happens in the 
            // measure.
            //
            // AS 1/22/09 NA 2009 Vol 1 - Fixed Fields
            // This isn't specific to fixing as it manifested itself in prior versions but 
            // it is more apparant when using fixing since the fixed areas are not updated.
            // Basically if the extent of the record area has changed (e.g. you resize a column 
            // smaller) we may be about to position the records using an incorrect horizontal 
            // offset. We only update the offset when we verify the scroll data at the end 
            // of this routine. By then we have already positioned the records using the 
            // old offset. We can't invalidate this element's arrange because in the arrange 
            // it is invalid. To limit the change I am only going to fix up the non-record 
            // orientation as I don't see a problem otherwise.
            //
            ScrollData scrollingData = this.ScrollingData;

            if (null != scrollingData && null != _cachedScrollDataFromMeasure && this.IsRootPanel)
            {
                // get the offset from the current scrolling info
                Vector scrollingOffset = scrollingData._offset;
                Vector newScrollingOffset = scrollingOffset;

                // get the viewport from the cache
                Size scrollingViewport = _cachedScrollDataFromMeasure._viewport;

                if (orientationIsVertical)
                    newScrollingOffset.X = Math.Min(scrollingOffset.X, finalSize.Width - scrollingViewport.Width);
                else
                    newScrollingOffset.Y = Math.Min(scrollingOffset.Y, finalSize.Height - scrollingViewport.Height);

                if (newScrollingOffset != scrollingOffset)
                {
                    // changing the offset and notifying the scrollviewer fixes 
                    // up the records and nested headers but the root header is still 
                    // off so we need to update its cached location and have it 
                    // invalidate its arrange
                    if (null != _gridViewPanelAdorner)
                        _gridViewPanelAdorner.InitializeOffset(orientationIsVertical ? -newScrollingOffset.X : -newScrollingOffset.Y);

                    scrollingData._offset = newScrollingOffset;
                    OnScrollInfoChange();
                    VerifyFixedFieldInfo();
                }
            }

			Rect arrangeRect = new Rect(finalSize);
			double extentUsedByLastItem = 0;
			// AS 6/22/09 NA 2009.2 Field Sizing
			// This wasn't used anymore.
			//
			//bool isAutoFit = this.DataPresenter != null ? this.DataPresenter.AutoFitResolved : false;

			// AS 3/15/07 BR21138
			// We need to know if any element was arranged such that it wasn't fully in view.
			//
			Rect rectUsed = new Rect();

			// AS 6/1/07 BR22762
			// We need to know the topmost point of the bottom fixed records
			// since that area will not be available to the scrollable records
			// and needs to be excluded when determining if the last record
			// is fully in view.
			//
			Rect rectBottomFixed = Rect.Empty;

			#endregion //Setup	

    
			#region Get the record counts

			ViewableRecordCollection ourRecordCollection = this.ViewableRecords;

			// JJD 5/25/07
			// Check to make sure that there is a ViewableRecordCollection
			if (ourRecordCollection == null)
				return finalSize;

			int topFixedCount = ourRecordCollection.CountOfFixedRecordsOnTop;
			int bottomFixedCount = ourRecordCollection.CountOfFixedRecordsOnBottom;
			int totalRecordCount = ourRecordCollection.Count;
			int scrollableRecordCount = totalRecordCount - (bottomFixedCount + topFixedCount);
			int topFixedToGenerate = Math.Max(0, topFixedCount - this._offsetWithinListForTopFixedRecords);
			int bottomFixedToGenerate = Math.Max(0, bottomFixedCount - this._offsetWithinListForBottomFixedRecords);

			#endregion //Get the record counts	
 

			#region Arrange our active children

            // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
            double overallExtent;

			// Compute our arrange rect.
			if (orientationIsVertical)
			{
                // JJD 3/27/07
				// Don't scroll if we are in autofit mode
                // JJD 1/21/09 - NA 2009 vol 1
                // Allow scrolling in autofit mode
//				if ( !isAutoFit )
					arrangeRect.X	= -1 * this.ScrollingData._offset.X;

				arrangeRect.Y	= this._extentUsedByHeaderRecords;

                // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
                // init overall etent
                overallExtent = finalSize.Height; 
			}
			else
			{
				arrangeRect.X	= this._extentUsedByHeaderRecords;

				// JJD 3/27/07
				// Don't scroll if we are in autofit mode
                // JJD 1/21/09 - NA 2009 vol 1
                // Allow scrolling in autofit mode
				//if ( !isAutoFit )
					arrangeRect.Y	= -1 * this.ScrollingData._offset.Y;
                
                // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
                // init overall etent
                overallExtent = finalSize.Width; 
			}

			// Arrange each child taking into account our orientation.

			// 5/31/07
			// Only process the active children
			//int visualChildrenCount	= this.VisualChildrenCount;
			// JJD 6/6/07
			// Use Children collection to only access active children
			//int activeChildrenCount = this.CountOfActiveContainers;
			// AS 7/9/07
			//IList children = this.Children;
			IList children = this.ChildElements;
			int activeChildrenCount = children.Count;

			// SSP 9/18/08 BR34595
			// The elements can be containers that contain record presentres and therefore keep track of
			// such elements instead of the record presenters.
			// 
			//Stack<RecordPresenter> bottomFixedRecords = null;
			Stack<UIElement> bottomFixedRecords = null;

            int totalItemsArranged = 0;

            Record topRecord = null;
            bool isRootPanel = this.IsRootPanel;

			for (int currentVisualChildIndex = 0; currentVisualChildIndex < activeChildrenCount; currentVisualChildIndex++)
			{
				// JJD 6/6/07
				// Use Children collection to only access active children
				//UIElement visualChildElement = this.GetChildElement(currentVisualChildIndex) as UIElement;
				UIElement visualChildElement = children[currentVisualChildIndex] as UIElement;

				int indexInList = this.GetGeneratedIndexFromChildIndex(currentVisualChildIndex);
                
                // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
                // added clipExtent param to ArrangeHelper
                double clipExtent = double.PositiveInfinity;

				// layout top fixed records
                if (topFixedToGenerate > 0 &&
                     indexInList >= this._firstVisualTopFixedItemIndex &&
                     indexInList < this._firstVisualTopFixedItemIndex + topFixedToGenerate)
                {
                    if (isRootPanel)
                    {
                        double start;

                        
                        // Use totalItemsArranged counter to see if we need to clip the record
                        // The first one we arrange we should not use arrangeRect to calculate the
                        // clip rect since no records have been arranged yet and it hasn't been initialized
                        
                        if (totalItemsArranged == 0)
                            start = this._extentUsedByHeaderRecords;
                        else if (orientationIsVertical)
                            start = arrangeRect.Bottom;
                        else
                            start = arrangeRect.Right;

                        clipExtent = overallExtent - (start + this._extentUsedByFixedRecordsOnBottom);
                    }

                    // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
                    // added clipExtent param to ArrangeHelper
                    extentUsedByLastItem = this.ArrangeHelper(visualChildElement, orientationIsVertical, false, extentUsedByLastItem, clipExtent,  finalSize, ref arrangeRect);

                    totalItemsArranged++;
                }
                else
                    if (indexInList >= this._firstVisualScrollableItemIndex &&
                         indexInList < this._firstVisualScrollableItemIndex + this._numberOfScrollableRecordsToLayout)
                    {
                        if (topRecord == null)
                        {
                            // SSP 9/18/08 BR34595
                            // The elements can be containers that contain record presenters.
                            // 
                            //RecordPresenter rp = visualChildElement as RecordPresenter;
                            RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(visualChildElement);

                            if (rp != null)
                                topRecord = rp.Record;
                        }

                        
                        // Use totalItemsArranged counter to see if we need to clip the record
                        // The first one we arrange we should not use arrangeRect to calculate the
                        // clip rect since no records have been arranged yet and it hasn't been initialized
                        //
                        
                        if (isRootPanel && totalItemsArranged > 0)
                        {
                            clipExtent = overallExtent - this._extentUsedByFixedRecordsOnBottom;

                            if (orientationIsVertical)
                                clipExtent -= arrangeRect.Bottom;
                            else
                                clipExtent -= arrangeRect.Right;
                         }

                        extentUsedByLastItem = this.ArrangeHelper(visualChildElement, orientationIsVertical, false, extentUsedByLastItem, clipExtent,  finalSize, ref arrangeRect);
                        
                        totalItemsArranged++;
                    }
                    else
                        if (indexInList >= this._firstVisualBottomFixedItemIndex &&
                             indexInList < this._firstVisualBottomFixedItemIndex + bottomFixedToGenerate)
                        {
                            // fixed bottom records we want to layout in the reverse order so just
                            // push them onto to stack and continue
                            if (bottomFixedRecords == null)
                                // SSP 9/18/08 BR34595
                                // The elements can be containers that contain record presentres and therefore keep track of
                                // such elements instead of the record presenters.
                                // 
                                //bottomFixedRecords = new Stack<RecordPresenter>();
                                bottomFixedRecords = new Stack<UIElement>();

                            // SSP 9/18/08 BR34595
                            // The elements can be containers that contain record presentres and therefore keep track of
                            // such elements instead of the record presenters.
                            // 
                            // ------------------------------------------------------------------------------------------
                            



                            bottomFixedRecords.Push(visualChildElement);
                            // ------------------------------------------------------------------------------------------
                        }
                        else
                        {
                            // SSP 9/18/08 BR34595
                            // If the visualChildElement is a container element that contains the record presenter
                            // then get the contained record presenter.
                            // 
                            //if (visualChildElement is RecordPresenter)
                            RecordPresenter childRecordPresenter = RecordListControl.GetRecordPresenterFromContainer(visualChildElement);
                            if (null != childRecordPresenter)
                            {
                                // AS 5/10/07 Optimization
                                bool wasHidingRecord = this.IsHidingRecord;
                                this.IsHidingRecord = true;

                                try
                                {

                                    // JJD 3/27/07
                                    // Mark the rp collapsed and don't bother arranging it
                                    // SSP 9/18/08 BR34595
                                    // Change related to above.
                                    // 
                                    
                                    



                                    childRecordPresenter.IsArrangedInView = false;
                                    childRecordPresenter.TreatAsCollapsed = true;
                                    
                                }
                                finally
                                {
                                    // AS 5/10/07 Optimization
                                    this.IsHidingRecord = wasHidingRecord;
                                }
                            }
                            else
                            {
                                Size desiredSize = visualChildElement.DesiredSize;

                                // arrange other child elements out of view
                                //visualChildElement.Arrange(new Rect( new Point(-2 * desiredSize.Width, -2 * desiredSize.Height),desiredSize));
                                visualChildElement.Arrange(new Rect(new Point(-10000 * finalSize.Width, -10000 * finalSize.Height), desiredSize));
                            }
                        }

				// AS 3/15/07 BR21138
				rectUsed.Union(arrangeRect);
			}
    
			#endregion //Arrange our active children


			#region Process bottom fixed records

			if (bottomFixedRecords != null)
			{
				Point pt;

				// create a rect that is offset to the bottom or right but is the size of the available space 
				if (orientationIsVertical)
					pt = new Point(0, finalSize.Height);
				else
					pt = new Point(finalSize.Width, 0);


				// JM 12-03-07 BR28352 - Adjust for horizontal scrolling if we are not in autofit mode
				if (orientationIsVertical)
				{
					// Don't scroll if we are in autofit mode
                    // JJD 1/21/09 - NA 2009 vol 1
                    // Allow scrolling in autofit mode
                    //if (!isAutoFit)
						pt.X = -1 * this.ScrollingData._offset.X;
				}
				else
				{
					// Don't scroll if we are in autofit mode
                    // JJD 1/21/09 - NA 2009 vol 1
                    // Allow scrolling in autofit mode
                    //if (!isAutoFit)
						pt.Y = -1 * this.ScrollingData._offset.Y;
				}


				Rect arrangeRectForBottomFixed = new Rect(pt, finalSize);

				extentUsedByLastItem = 0;
				while (bottomFixedRecords.Count > 0)
				{
					// SSP 9/18/08 BR34595
					// The elements can be containers that contain record presenters.
					// 
					//RecordPresenter rp = bottomFixedRecords.Pop();
					//extentUsedByLastItem = this.ArrangeHelper(rp, orientationIsVertical, true, extentUsedByLastItem, isAutoFit, finalSize, ref arrangeRectForBottomFixed);
					UIElement itemElem = bottomFixedRecords.Pop( );
					extentUsedByLastItem = this.ArrangeHelper( itemElem, orientationIsVertical, true, extentUsedByLastItem, double.PositiveInfinity,  finalSize, ref arrangeRectForBottomFixed );
                    totalItemsArranged++;

					// AS 6/1/07 BR22762
					// Keep track of the area used by the bottom fixed records.
					//
					if (rectBottomFixed.IsEmpty)
						rectBottomFixed = arrangeRectForBottomFixed;
					else
						rectBottomFixed.Union(arrangeRectForBottomFixed);

					// AS 3/15/07 BR21138
					// AS 6/1/07
					// arrangeRect is not adjusted in this routine - the arrangeRectForBottomFixed is.
					//
					//rectUsed.Union(arrangeRect);
					rectUsed.Union(arrangeRectForBottomFixed);
				}
			}

			#endregion //Process bottom fixed records	


			#region Calculate the ScrollViewport and update our scrolling data if necessary

			// Do a refined calculation of the ScrollViewport using data cached in the
			// Measure using a precise calculation of the scroll index of the last visible record
			// based on the visible records from all panels (root + nested).
			if (isRootPanel &&
				this._cachedScrollDataFromMeasure != null &&
				activeChildrenCount > 0)
			{
				// JJD 5/23/07
				// Pass in the size to the VerifyScrollPositionOfLastVisibleRecord
				// because the ActualWidth and ActulaHeight properties are not updated
				// until after the ArrangeOverride returns
				//this.VerifyScrollPositionOfLastVisibleRecord();
				// AS 6/1/07 BR22762
				// The size we pass in should not include the area used by the bottom
				// fixed records since the non-fixed records cannot use that space without
				// being overlayed by the fixed records.
				//
				//this.VerifyScrollPositionOfLastVisibleRecord(finalSize);
				Size availableScrollSize = finalSize;

				if (rectBottomFixed.IsEmpty == false)
					// [JM/JD 06-18-07]
					//availableScrollSize.Height = rectBottomFixed.Top;
					availableScrollSize.Height = Math.Max(rectBottomFixed.Top, 0);

				// JJD 6/6/07 
				// added bool return value so we know if the last record is fully in view
				bool isLastRecordFullyInView = this.VerifyScrollPositionOfLastVisibleRecord(availableScrollSize);

				if (this._useScrollPositionCandidate == false)
				{
					Size scrollViewport;
					// JJD 3/12/07 Use the cached value on the rootpanel
					//int ixLast = this.GetScrollIndexOfLastVisibleRecord();
					int ixLast = this._scrollPositionOfLastVisibleRecord;
					int ixFirst = this.EffectiveScrollPosition;

					int ixLastScrollableRecord;

					// JJD 6/6/07 
					// added flag so we know if the last scrollable record is fully in view
					if (isLastRecordFullyInView)
					{
						if (bottomFixedCount > 0)
							ixLastScrollableRecord = ourRecordCollection[totalRecordCount - (1 + bottomFixedToGenerate)].OverallScrollPosition;
						else
							ixLastScrollableRecord = this.ViewPanelInfo.OverallScrollCount - 1;

						// SSP 6/18/08 BR33922
						// 
						//this._isLastScrollableRecordFullyInView = ixLast >= ixLastScrollableRecord;
						this._isLastScrollableRecordFullyInView = ixLast >= ixLastScrollableRecord
							&& ixLast >= this.GetLastAbsoluteScrollableRecordScrollPos( );
					}
					else
						this._isLastScrollableRecordFullyInView = false;

					if (orientationIsVertical)
					{
						int viewportHeight = Math.Max(1, ixLast - ixFirst + 1);

						// AS 3/15/07 BR21138
						// If we positioned something out of view then decrease the viewport height because
						// we want to consider the item to not be in view - unless it is the last item since
						// we do record level scrolling.
						//
						if (rectUsed.Height > finalSize.Height && ixLast < this._cachedScrollDataFromMeasure._viewport.Height - 1)
							viewportHeight--;

						// To account for the situation where we have reached the end of a long list and there
						// are not enough items to fill the screen, don't base the viewport height on the number
						// of visible items - use the last viewport height calculated instead.
						if (this._cachedRemainingAvailableExtentFromMeasure > 0)
							viewportHeight = Math.Max(viewportHeight, this._lastViewportHeight);


						this._lastViewportHeight = viewportHeight;
						scrollViewport = new Size(this._cachedScrollDataFromMeasure._viewport.Width, viewportHeight);
					}
					else
					{
						int viewportWidth = Math.Max(1, ixLast - ixFirst + 1);

						// AS 3/15/07 BR21138
						// If we positioned something out of view then decrease the viewport height because
						// we want to consider the item to not be in view - unless it is the last item since
						// we do record level scrolling.
						//
						if (rectUsed.Width > finalSize.Width && ixLast < this._cachedScrollDataFromMeasure._viewport.Width - 1)
							viewportWidth--;

						// To account for the situation where we have reached the end of a long list and there
						// are not enough items to fill the screen, don't base the viewport width on the number
						// of visible items - use the last viewport width calculated instead.
						if (this._cachedRemainingAvailableExtentFromMeasure > 0)
							viewportWidth = Math.Max(viewportWidth, this._lastViewportWidth);


						this._lastViewportWidth = viewportWidth;
						scrollViewport = new Size(viewportWidth, this._cachedScrollDataFromMeasure._viewport.Height);
					}


					// Update our scrolling info which will cause InvalidateScrollInfo to be called
					// on our ScrollOwner if it has changed.
					this.VerifyScrollData(this._cachedScrollDataFromMeasure._extent,
										  scrollViewport,
										  this._cachedScrollDataFromMeasure._offset,
						// JJD 6/12/07 - added availableScrollSize parameter
										  availableScrollSize);

					// JJD 6/11/07
					// PostBottomRecordVerification to make sure we don't allow any extra white space at the bottom 
					if (topRecord != null)
						this.PostBottomRecordVerification(availableScrollSize);
				}
				else
				{
					// JJD 6/6/07 
					// added flag so we know if the last scrollable record is fully in view
					if (isLastRecordFullyInView)
					{
						int ixLast = this._scrollPositionOfLastVisibleRecord;

						int ixLastScrollableRecord;

						if (bottomFixedCount > 0)
							ixLastScrollableRecord = ourRecordCollection[totalRecordCount - (1 + bottomFixedToGenerate)].OverallScrollPosition;
						else
							ixLastScrollableRecord = this.ViewPanelInfo.OverallScrollCount - 1;

						// SSP 6/18/08 BR33922
						// 
						//this._isScrollPositionCandidateFullyInView = ixLast >= ixLastScrollableRecord;
						this._isScrollPositionCandidateFullyInView = ixLast >= ixLastScrollableRecord
							&& ixLast >= this.GetLastAbsoluteScrollableRecordScrollPos( );
					}
					else
						this._isScrollPositionCandidateFullyInView = false;
				}
			}
			// JM BR28911 12-05-07 - Reset scrolling data on root panels if the record count is zero.
			else 
			if (isRootPanel							&&
				this._cachedScrollDataFromMeasure != null	&&
				activeChildrenCount < 1)
			{
				ScrollData	scrollData					= this.ScrollingData;	// Optimization
				ScrollData	cachedScrollDataFromMeasure	= this._cachedScrollDataFromMeasure;	//Optimization

				if (orientationIsVertical)
				{
					scrollData._viewportForScrollbar.Height = 1;
					scrollData._viewport.Height				= 1;
					scrollData._extentForScrollbar.Height	= 1;
					scrollData._extent.Height				= 1;

					// JM BR28982 12-06-07
					scrollData._extentForScrollbar.Width	= cachedScrollDataFromMeasure._extent.Width;
					scrollData._extent.Width				= cachedScrollDataFromMeasure._extent.Width;
					scrollData._viewportForScrollbar.Width	= cachedScrollDataFromMeasure._viewport.Width;
					scrollData._viewport.Width				= cachedScrollDataFromMeasure._viewport.Width;

					// JM 11-12-08 TFS9807 - Regression fix
					if (scrollData._extent.Width <= scrollData._viewport.Width)
						// JM 11-12-08 TFS9807
						scrollData._offset					= new Vector(0, 0);
				}
				else
				{
					scrollData._viewportForScrollbar.Width	= 1;
					scrollData._extentForScrollbar.Width	= 1;

					// JM BR28982 12-06-07
					scrollData._extentForScrollbar.Height	= cachedScrollDataFromMeasure._extent.Height;
					scrollData._extent.Height				= cachedScrollDataFromMeasure._extent.Height;
					scrollData._viewportForScrollbar.Height = cachedScrollDataFromMeasure._viewport.Height;
					scrollData._viewport.Height				= cachedScrollDataFromMeasure._viewport.Height;

					// JM 11-12-08 TFS9807 - Regression fix
					if (scrollData._extent.Height <= scrollData._viewport.Height)
						// JM 11-12-08 TFS9807
						scrollData._offset					= new Vector(0, 0);
				}

                // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                this.VerifyFixedFieldInfo();

                this.OnScrollInfoChange();
			}

			#endregion //Calculate the ScrollViewport and update our scrolling data if necessary


			#region Notify the DataPresenter if the records in view has changed

			if (this._recordsInViewChanged == true)
			{
				// reset the flag
				this._recordsInViewChanged = false;

				IViewPanelInfo info = this.ViewPanelInfo;

				// let the datapresenter know
				if (info != null)
					info.OnRecordsInViewChanged();
			}

			#endregion //Notify the DataPresenter if the records in view has changed	

			// AS 7/27/09 NA 2009.2 Field Sizing
			this.OnArrangeComplete();
    
			return finalSize;
		}

				#endregion //ArrangeOverride

				#region EnsureRecordIsVisible
        
        internal override bool EnsureRecordIsVisible(Record record)
        {
            // JJD 3/30/09 - TFS15217
            // Moved logic into helper method. Pass 'true' in for firstPass parameter.
            return this.EnsureRecordIsVisibleHelper(record, true);
        }

        // JJD 3/30/09 - TFS15217 - Added
		private bool EnsureRecordIsVisibleHelper(Record record, bool firstPass)
		{
			if (record == null)
				throw new ArgumentNullException("record");
			
			if (this.IsRootPanel == false)
				return this.RootPanel.EnsureRecordIsVisible(record);

            // JJD 5/27/08 - BR32292
            // If records can't be scrolled then return false
            if (!this.CanRecordsBeScrolled)
                return false;

			GridViewSettings viewSettings = this.ViewSettings;

			if (viewSettings == null)
				return false;

			if (record.VisibilityResolved == Visibility.Collapsed)
				return false;

			if (record.ParentDataRecord == null)
			{
				if (record.IsFixed)
				{
					if (record.IsOnTopWhenFixed)
					{
						if (this._offsetWithinListForTopFixedRecords == 0)
							return true;


					}
					else
					{
						if (this._offsetWithinListForBottomFixedRecords == 0)
							return true;
					}
				}
			}

			IViewPanelInfo info = this.ViewPanelInfo;

			int unadjustedScrollPosOfTargetRecord	= info.GetOverallScrollPositionForRecord(record);

			// fixed records return -1 for scrollpos so we can ignore these
			if (unadjustedScrollPosOfTargetRecord < 0)
				return true;

			int adjustedScrollPosOfTargetRecord = unadjustedScrollPosOfTargetRecord;

			// JJD 3/01/07
			// Adjust scroll position to account for fixed records.
			ViewableRecordCollection vrc = this.ViewableRecords;

			if (vrc == null )
				return false;

            // JJD 3/01/07
            // reduce the scroll count by the count of root fixed top records
            adjustedScrollPosOfTargetRecord -= vrc.CountOfFixedRecordsOnTop;


            // JJD 3/01/07
            // If the record is not in the root panel  then also reduce the scroll count 
            // by the count of nested collection fixed top records
            // JJD 1/12/09
            // Get all of the opt fixed offsets by walking up the parent chain
            //if (record.ParentCollection.ViewableRecords != vrc)
            //    adjustedScrollPosOfTargetRecord -= record.ParentCollection.ViewableRecords.CountOfFixedRecordsOnTop;

            ViewableRecordCollection parentVrc = record.ParentCollection.ViewableRecords;

            // JJD 5/22/09
            // If the record is fixed on toop we only need to adjust for fixed records from
            // ancestor records parent collections. 
            bool bypassFixedRecordAdjustmentAtThisLevel = record.IsFixed && record.IsOnTopWhenFixed;

            while (parentVrc != vrc)
            {
                // JJD 5/22/09
                // If the flag was set above just reset it at this level
                // so we pick up adjustments for ancestor record parent collections
                if (bypassFixedRecordAdjustmentAtThisLevel == true)
                    bypassFixedRecordAdjustmentAtThisLevel = false;
                else
                    adjustedScrollPosOfTargetRecord -= parentVrc.CountOfFixedRecordsOnTop;

                Record parent = parentVrc.RecordCollection.ParentRecord;

                if (parent != null)
                    parentVrc = parent.ParentCollection.ViewableRecords;
                else
                    break;
            }

			// JJD 3/01/07
			// ensure that the adjustments don't make the scrollpos negative (which should not happen)
            Debug.Assert(adjustedScrollPosOfTargetRecord >= 0);
            adjustedScrollPosOfTargetRecord = Math.Max(0, adjustedScrollPosOfTargetRecord);

			int firstRecordScrollPos	= this.EffectiveScrollPosition;
			
			// JJD 3/12/07 Use the cached value on the rootpanel
			//int lastRecordScrollPos		= this.GetScrollIndexOfLastVisibleRecord();
			int lastRecordScrollPos		= this._scrollPositionOfLastVisibleRecord;

			// JJD 3/13/07 
			// If the scroll position is 1 more than can fit then execute linedown or right
			// until the record is fully in view
			// JM 01-04-12 TFS72725 - Use the adjusted scroll position of the target record to ensure we account for 
			// any fixed records on top.
			//while (unadjustedScrollPosOfTargetRecord == lastRecordScrollPos + 1)
			while (adjustedScrollPosOfTargetRecord == lastRecordScrollPos + 1)
			{
				// save the las
				int holdPreviousFirstScrollPos = firstRecordScrollPos;

				if (viewSettings.Orientation == Orientation.Vertical)
					this.LineDown();
				else
					this.LineRight();

				// JJD 3/13/07 
				// Call UpdateLayout so the _scrollPositionOfLastVisibleRecord is synced up
				// with the proper new value
				this.UpdateLayout();
			
				// reget the last scroll pos
				lastRecordScrollPos		= this._scrollPositionOfLastVisibleRecord;

				// reget the 1st scroll pos
				firstRecordScrollPos	= this.EffectiveScrollPosition;

				// if the 1st scrollpos hasn't changed then bail out to prevent 
				// an infinite loop
				if (holdPreviousFirstScrollPos == firstRecordScrollPos)
					break;

			}

			// JM 02-12-09 TFS6109 - Always expand the parent record if it is not already expanded.
			// JJD 3/13/07 
			// If the target record's scroll pos (adjusted for top fixed records)
			// is >= the first scrollable record pos or the unadjusted
			// target record scroll pos is <= the last fully
			// visible scrollable record then all we need to do is
			// ensure that the parent record is expanded and return true
//			if (adjustedScrollPosOfTargetRecord >= firstRecordScrollPos &&
//				unadjustedScrollPosOfTargetRecord <= lastRecordScrollPos)
//			{
				if (record.ParentRecord != null && record.ParentRecord.IsExpanded == false)
				{
					// check with the dp if it is ok to scroll
					if (!info.IsOkToScroll())
						return false;

					record.ParentRecord.IsExpanded = true;
					this.UpdateLayout();	//TFS6109
				}

//				return true;
//			}

			// check with the dp if it is ok to scroll
			if (!info.IsOkToScroll())
				return false;

            if (adjustedScrollPosOfTargetRecord < firstRecordScrollPos)
			{
				if (viewSettings.Orientation == Orientation.Vertical)
					this.SetVerticalOffset(adjustedScrollPosOfTargetRecord);
				else
					this.SetHorizontalOffset(adjustedScrollPosOfTargetRecord);

				if (record.ParentRecord != null  &&  record.ParentRecord.IsExpanded == false)
					record.ParentRecord.IsExpanded = true;

				return true;
			}

			Record	newTopRecord		= null;
            double	newOffsetNormalized = adjustedScrollPosOfTargetRecord - this._visibleItemCount;

            // JJD 1/12/09
            // Use the unadjusted position for the bottom record target since 
            // the DetermineTopRecordForGivenBottomRecordIndex method is expecting
            // the overall scroll position
            //int		bottomRecordIndex	= adjustedScrollPosOfTargetRecord;
			int		bottomRecordIndex	= unadjustedScrollPosOfTargetRecord;

            // JJD 2/23/09 - TFS6324/BR34915
            // Get the record presenter's content site and see if it is already completely inview
            RecordPresenter rp = record.AssociatedRecordPresenter;

            // JJD 3/20/09 - TFS15694
            // Make sure the recordpresenter is visible since deactivated ones can still be around
            // when we are in recycling mode. Also check to make sure it is showing its content
            // so we handle hierarchical situations properly where we could have a parent
            // record that has a record presenter but it is scrolled out of view
            //FrameworkElement rcs = null != rp ? rp.GetRecordContentSite() : null;
            FrameworkElement rcs = null != rp && rp.IsVisible && rp.ShouldDisplayRecordContent ? rp.GetRecordContentSite() : null;

            if (null != rcs && this.IsAncestorOf( rcs ) )
            {
                Rect panelRect = new Rect(this.RenderSize);
                Rect rcsRect = rcs.TransformToAncestor(this).TransformBounds(new Rect(rcs.RenderSize));

                // JJD 2/23/09 - TFS6324/BR34915
                // With GroupByRecords we always want to ccheck the top and bottom.
                // Otherwise based on the orientation check the top and bottom or the left and right
                if (rp is GroupByRecordPresenter || viewSettings.Orientation == Orientation.Vertical)
                {
                    if (rcsRect.Y >= panelRect.Y && rcsRect.Bottom <= panelRect.Bottom)
                        return true;
                }
                else
                {
                    if (rcsRect.X >= panelRect.X && rcsRect.Right <= panelRect.Right)
                        return true;
                }
            }

            // JJD 3/30/09 - TFS15217
            // There are situations where in nested groupbys where the
            // DetermineTopRecordForGivenBottomRecordIndex can return a top record that
            // would be slightly off, i.e. it would result in the taregt record being
            // partially clipped. This is solved by performing the logic in this routine 
            // onced more
            if (firstPass)
            {
                this.DetermineTopRecordForGivenBottomRecordIndex(ref newTopRecord, ref newOffsetNormalized, bottomRecordIndex);

			    if (viewSettings.Orientation == Orientation.Vertical)
				    this.SetVerticalOffset(newOffsetNormalized);
			    else
				    this.SetHorizontalOffset(newOffsetNormalized);

                // JJD 3/30/09 - TFS15217
                // make sure the layout is updated
                this.UpdateLayout();

                // JJD 3/30/09 - TFS15217
                // Call this method again passing in 'false' for the firstPass parameter
                // so we avoid a potential loop.
                return this.EnsureRecordIsVisibleHelper(record, false);
            }

			// JM 02-12-09 TFS6109 - We are now doing this up above before we call DetermineTopRecordForGivenBottomRecordIndex
			//if (record.ParentRecord != null && record.ParentRecord.IsExpanded == false)
			//	record.ParentRecord.IsExpanded = true;

			return true;
		}

				#endregion //EnsureRecordIsVisible

				// 5/21/07 - Optimization
				// Derive from RecyclingItemsPanel 
				#region GetCanCleanupItem

		/// <summary>
		/// Determines whether the item at the specified index can be cleaned up.
		/// </summary>
		/// <param name="itemIndex">The index of the item to be cleaned up.</param>
		/// <returns>True if the item at the specified index can be cleaned up, false if it cannot.</returns>
		protected override bool GetCanCleanupItem(int itemIndex)
		{
			RecordListControl rlc = this.RecordListControl;

			int count = rlc.Items.Count;

			if (itemIndex < 0 || itemIndex >= count)
				return true;

			// JM 03-25-09 TFS 15539 
			if (itemIndex >= this._offsetWithinListForScrollableRecords &&
				itemIndex < (this._offsetWithinListForScrollableRecords + this.TotalVisibleGeneratedItems))
				return false;

			Record rcd = rlc.Items[itemIndex] as Record;

			if (rcd.IsFixed || rcd.IsSpecialRecord)
				return false;

			return true;
		}

				#endregion //GetCanCleanupItem	

				#region MeasureOverride

		/// <summary>
		/// Called to give an element the opportunity to return its desired size and measure its children.
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected override System.Windows.Size MeasureOverride(Size availableSize)
		{
			// MD 4/28/11 - TFS36608
			// If we have set this field, reset it and unhook the RecordsInViewChanged event. We only need this outside of measures so that
			// we can invalidate the measure when the user scrolls up and we have to dirty the measure of a nested panel that previously had 
			// no visible child.
			if (_scrollPositionWhenNoChildrenAbovePresent >= 0)
			{
				_scrollPositionWhenNoChildrenAbovePresent = -1;
				// JJD 11/16/11 - TFS25239
				// Replaced wiring the DP's RecordsInViewChanged event will a PropertyValueTracker 
				// so we don't root this element.
				// So instead of unwiring the event we can just null out the tracker
				//this.DataPresenter.RecordsInViewChanged -= new EventHandler<RecordsInViewChangedEventArgs>(this.OnDataPresenterRecordsInViewChanged);
				_rcdsInViewTracker = null;
			}

			GridViewSettings viewSettings = this.ViewSettings;

			if (viewSettings == null)
				return new Size(1, 1);

			// JJD 8/16/10 - TFS30240
			// If the width or height is < 1 then bail out
			if (availableSize.Width < 1 ||
				availableSize.Height < 1)
			{
				return new Size(1, 1);
			}

				// Create our adorner layer if we haven't yet done so.
			if (this._gridViewPanelAdorner == null)
				this.InitializeAdorner();

            // JJD 1/3/08 - BR28972
            // Cache the availableSize for use below
            Size    originalAvailableSize   = availableSize;
			bool	orientationIsVertical	= (viewSettings.Orientation == Orientation.Vertical);
			bool	isRootPanel				= this.IsRootPanel;

            // JJD 5/2/08 - BR32390
            // Initialize flag so we know whether to show scrollbars
            this._isRootPanelInInfiniteContainer = false;

			// JJD 3/5/07 
			// Keep track of the max extents in infinite containers on the root panel
            if (isRootPanel)
            {
                if (double.IsInfinity(availableSize.Width))
                {
                    this._widthInInfiniteContainer = this.WidthInfiniteContainersResolved;
                    
                    // JJD 5/2/08 - BR32390
                    // Added flag so we know whether to show scrollbars
                    if (!orientationIsVertical)
                        this._isRootPanelInInfiniteContainer = true;
                }
                else
                    this._widthInInfiniteContainer = 0;

                if (double.IsInfinity(availableSize.Height))
                {
                    this._heightInInfiniteContainer = this.HeightInfiniteContainersResolved;
                    
                    // JJD 5/2/08 - BR32390
                    // Added flag so we know whether to show scrollbars
                    if (orientationIsVertical)
                        this._isRootPanelInInfiniteContainer = true;
                }
                else
                    this._heightInInfiniteContainer = 0;

            }
            else
            {
                // JJD/29/08 - BR30660
                // Since this is not the Root panel make sure that the RootPanel property
                // is not null. Otherwise we will throw a null ref exception below.
                // Note this can happen during initialization
                if (this.RootPanel == null)
                {
                    // JJD 05/29/08 -  BR30387
                    // Wire up the LayoutUpdated event so we can recheck the
                    // RootPanel property at the end of the layout process. 
                    // If it is not null by then we will just invalidate the measure
                    // so we will eventually get back in here. This 
                    // corrects a timing issue when changings views.
					// JJD 3/15/11 - TFS65143 - Optimization
					// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
					// and just wire LayoutUpdated on the DP
					//EventHandler handler = new EventHandler(OnLayoutUpdated);
					//this.LayoutUpdated -= handler;
					//this.LayoutUpdated += handler;

					DataPresenterBase dp = this.DataPresenter;

					if (dp != null)
						dp.WireLayoutUpdated(this.OnLayoutUpdated);

                    return new Size(1, 1);
                }
            }


			// Adjust the width and/or the height of the passed-in constraint to the corresponding screen size dimension
			// if the width and/or height are equal to infinity.  This is to avoid creating and measuring elements for every
			// record in the underlying data.
			double	constraintWidth			= double.IsInfinity(availableSize.Width) ? this.WidthInfiniteContainersResolved : availableSize.Width;
			double	constraintHeight		= double.IsInfinity(availableSize.Height) ? this.HeightInfiniteContainersResolved : availableSize.Height;
			availableSize					= new Size(constraintWidth, constraintHeight);

			IViewPanelInfo info				= this.ViewPanelInfo;
			int		scrollOffsetAsItemIndex;
			double	availableExtent;
			Size	effectiveConstraint		= isRootPanel ? availableSize : ((GridViewPanelNested)this.RootPanel)._lastMeasureConstraint;
			Size	adjustedConstraint		= effectiveConstraint;

			// JM 11-25-08 TFS10814 - Access the ViewableRecords.Count property to workaround a timing issue that results in the totalItemsThisPanel variable (the next line)
			//						  being set to a number that is not equal to this.RecordListControl.Items.Count.  We discovered this in a grid that has the AddRow turned on
			//						  while bound to an empty DataSource - in this case the AddRow was being added to the ViewableRecords collection but it was not showing up
			//						  in the grid because totalItemsThisPanel was = zero and further down in this routine we check for totalItemsThisPanel > 0 before generating
			//						  a presenter for the add record.  It appears that there is some kind of timing issue when accessing ItemCollection.Count property since it
			//						  delegates to our ViewableRecords.Count property (the RecordListControl is bound to the ViewableRecordsCollection) which can send out Reset
			//						  notifications which they are evidently not expecting.  By access the ViewableRecords.Count proeprty here first, the timing issues are avoided.
            // JJD 7/13/09 - TFS19156
            // Make sure viewablerecords is not null
            int iUnused = null != this.ViewableRecords ? this.ViewableRecords.Count : 0;

			int		totalItemsThisPanel		= ((this.RecordListControl != null) && this.RecordListControl.HasItems) ? this.RecordListControl.Items.Count : 0;

			ViewableRecordCollection viewableRecords = this.ViewableRecords;

			// JJD 5/25/07
			// Check to make sure that there is a ViewableRecordCollection
			if (viewableRecords == null)
				return new Size(1, 1);

			// AS 6/11/07
			// In some situations, like collapsing an ancestor group by record, the panel
			// may get a measure but its within a collapsed record. This caused us to 
			// hit an assert below regarding scroll position but really we should not be 
			// speading the cycles to do a measure so we'll just escape out if the 
			// ancestor is collapsed.
			//
			Record parentRecord = viewableRecords.RecordCollection.ParentRecord;

			if (parentRecord != null && parentRecord.HasCollapsedAncestor)
				return new Size(1, 1);

			if (totalItemsThisPanel > 0)
			{
				// JJD 4/17/07
				// Get the first item in the list. This will trigger a lazy creation
				// of at least one record record. If the Visibility of the Records is set
				// to 'collapsed' in the InitializeRecord event for all records in the list then firstRecordInList will return null
				// and subsequently the ourRecordCollection.Count will return null
				Record firstRecordInList = viewableRecords[0];

				// reget the count after forcing the creation of the first record above
				totalItemsThisPanel = viewableRecords.Count;
			}
			
			int scrollCount = info.OverallScrollCount;

			Debug.Assert(scrollCount >= totalItemsThisPanel);

			// If we are the root panel, save our constraint - nested panels will look at this.
			if (isRootPanel)
			{
				this._lastMeasureConstraint = availableSize;

				// JJD 6/11/07
				// Only update the _scrollPositionOfLastVisibleRecord if the scroll count has changed
                // JJD 1/23/09 - NA 2009 vol 1 - record filtering
                // Only update the flags if we are not in a pseudo scroll
                if (!this._useScrollPositionCandidate)
                {
                    // JJD 2/19/09 - TFS13979
                    // check the last sort version also
                    //if ( _lastOverallScrollCount != scrollCount ||
                    //    this._isProcessingBottomRecordVerification)
                    DataPresenterBase datapresenter = this.DataPresenter;
                    int sortVersion = datapresenter != null ? datapresenter.OverallSortVersion : 0;
                    if (_lastOverallScrollCount != scrollCount ||
                         sortVersion != this._lastOverallSortVersion ||
                        this._isProcessingBottomRecordVerification)
                    {
                        this._scrollPositionOfLastVisibleRecord = -1;

                        // JJD 2/19/09 - TFS13979
                        // When the sort version changes cache it and
                        // wire up the LayoutUpdated event so we can invalidate the arrange
                        // This is necessary because when the order of hydrated records
                        // changes in a nested situation their are situations where
                        // it will seems that the last record is in view even though it
                        // isn't. This gets fixed up by the framework after the layout is 
                        // completely updated.
                        if (sortVersion != this._lastOverallSortVersion)
                        {
                            this._lastOverallSortVersion = sortVersion;
							// JJD 3/15/11 - TFS65143 - Optimization
							// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
							// and just wire LayoutUpdated on the DP
							//EventHandler handler = new EventHandler(OnLayoutUpdatedForArrange);
							//this.LayoutUpdated -= handler;
							//this.LayoutUpdated += handler;
							DataPresenterBase dp = this.DataPresenter;

							if (dp != null)
								dp.WireLayoutUpdated(this.OnLayoutUpdated);
                        }
                    }
                }
			}

			// JJD 6/11/07
			// cache the OverallScrollCount
            if (scrollCount != this._lastOverallScrollCount)
            {
                this._lastOverallScrollCount = scrollCount;

                // JJD 3/2/09 - TFS14713
                // When the overall scroll count changes we need to clear
                // the _lastBottomRecordVerificationScrollPos so we will 
                // re-verify that the last record is at the bottom
                this._lastBottomRecordVerificationScrollPos = 0;
            }

			// Setup an adjustedConstraint that takes our orientation into account - we will used this for measuring our children.
			// Also setup an availableExtent which represents the amount of space available in the primary scrolling dimension.  Note that since
			// we use the effectiveConstraint to calculate the availablExtent, the available extent for nested panels this will be based
			// on the associated root panel's constraint.
			if (orientationIsVertical)
			{
				adjustedConstraint.Height		= double.PositiveInfinity;
				if (this.RootPanel.IsScrolling && this.RootPanel.CanHorizontallyScroll)
					adjustedConstraint.Width	= double.PositiveInfinity;

				scrollOffsetAsItemIndex			= this.IsScrolling ? this.NormalizeScrollOffset(this.ScrollingData._offset.Y, scrollCount - 1) : 0;
				availableExtent					= effectiveConstraint.Height;
			}
			else
			{
				adjustedConstraint.Width		= double.PositiveInfinity;
				if (this.RootPanel.IsScrolling && this.RootPanel.CanVerticallyScroll)
					adjustedConstraint.Height	= double.PositiveInfinity;

				scrollOffsetAsItemIndex			= this.IsScrolling ? this.NormalizeScrollOffset(this.ScrollingData._offset.X, scrollCount - 1) : 0;
				availableExtent					= effectiveConstraint.Width;
			}

			// cache the old info to determine is something changed
			int lastOffsetWithinListForTopFixedRecords		= this._offsetWithinListForTopFixedRecords;
			int lastOffsetWithinListForBottomFixedRecords	= this._offsetWithinListForBottomFixedRecords;
			int lastOffsetWithinListForScrollableRecords	= this._offsetWithinListForScrollableRecords;
			int lastNumberOfScrollableRecordsToLayout		= this._numberOfScrollableRecordsToLayout;

			// reset the offset values
			this._offsetWithinListForTopFixedRecords = 0;
			this._offsetWithinListForBottomFixedRecords = 0;
			this._offsetWithinListForScrollableRecords = 0;
			this._extentUsedByHeaderRecords = 0;
			this._fieldLayoutForHeaderRecordInAdorner = null;
			this._firstVisualBottomFixedItemIndex = 0;
			this._firstVisualTopFixedItemIndex = 0;
			this._firstVisualScrollableItemIndex = 0;
			this._visibleItemCount = 0;
            this._numberOfScrollableRecordsToLayout = 0;

			Size ourDesiredSize = new Size();
			int highestItemIndexProcessed = -1;
			bool canTopFixedRecordsFit = true;
			bool canAllFixedRecordsFit = true;
			double overallExtentUsed = 0;

			#region Get the record counts

			int totalRecordCount = viewableRecords.Count;
			int topFixedCount = viewableRecords.CountOfFixedRecordsOnTop;
			int bottomFixedCount = viewableRecords.CountOfFixedRecordsOnBottom;
			int scrollableRecordCount = totalRecordCount - (bottomFixedCount + topFixedCount);

            Debug.Assert(scrollableRecordCount >= 0);

			#endregion //Get the record counts	
    
			// 5/21/07 - Optimization
			// Call BeginGeneration so we can support recycling 
			int overallScrollPosition = this.EffectiveScrollPosition;


            // JJD 1/23/09 - NA 2009 vol 1 - record filtering
            // Keep track of the actual top record since we process the
            // filtering logic lazily as records get hydrated it is possible
            // that the scollposition of the top record will change without the
            // user performing a scroll operation. In which case we want to
            // retain the current top record
            if (isRootPanel &&
                totalItemsThisPanel > 0 &&
                this._useScrollPositionCandidate == false &&
				// MD 4/27/11 - TFS36608
				// Since this block is only needed when filtering changes, we should only get in here when the filters version changes.
				// Otherwise, it may cause problems when child records ar displayed above the parent and the top visible record is expanded.
				// In that case, this block ends up moving the expanded record back to the top of the visible area and we never get to see
				// the children, even when we scroll back up. There is just a small blank area that takes up as many scroll positions as there
				// were children.
				_lastVerifiedFiltersVersion != viewableRecords._verifiedFiltersVersion)
            {

				// MD 4/27/11 - TFS36608
				// Cache the last filters version for which we did this shift.
				_lastVerifiedFiltersVersion = viewableRecords._verifiedFiltersVersion;

                // get the record at the scroll position
                Record recordAtScrollPosition = this.GetTopRecordFromScrollOffset(overallScrollPosition);

                // see if the overall scrollposition is the same
                if (recordAtScrollPosition != null &&
                    overallScrollPosition == this._lastOverallScrollPosition &&
                    overallScrollPosition > 0 &&
					// JJD 11/30/10 - TFS31984 
					// Only adjust the position if the scroll behavior is default
					this.DataPresenter != null &&
					this.DataPresenter.ScrollBehaviorOnListChange == ScrollBehaviorOnListChange.Default)
                {
                    // see if the rcd is different from the rcd we cached last time
                    // at this scroll position
                    if (recordAtScrollPosition != this._lastRecordAtOverallScrollPosition &&
                        this._lastRecordAtOverallScrollPosition != null &&
                        // JJD 7/14/09 - TFS19181
                        // Check to make sure the cached last record is still valid 
                        // before trying to get its scroll position
                        this._lastRecordAtOverallScrollPosition.IsStillValid)
                    {
                        // get the new scroll position of the cached record
                        int newScrollPosition = this.GetScrollOffsetFromRecord(this._lastRecordAtOverallScrollPosition);

                        // if the scrollposition are different (which they should be at this poit)
                        // then use the new scroll position and update the info's overall scroll position
                        if (newScrollPosition != this._lastOverallScrollPosition)
                        {
                            recordAtScrollPosition      = this._lastRecordAtOverallScrollPosition;
                            overallScrollPosition       = newScrollPosition; 
                            info.OverallScrollPosition  = overallScrollPosition;
                         }
                    }
                }

                // cache the record for the next time
                this._lastRecordAtOverallScrollPosition = recordAtScrollPosition;
            }

            // AS 2/23/09 Optimization
            // We are now updating the _lastMeasureScrollPosition in the SetOffsetHelper so we cannot 
            // use that for comparison. Instead we will track a new member to know which way we have 
            // moved since the last call to BeginGeneration.
            //

            // JJD 3/11/10 - TFS28705 - Optimization
            // Call GetRecordsExpectedToBeGenerated to provide a list of expected records to BeginGeneration
            //this.BeginGeneration(overallScrollPosition < this._lastOverallScrollPosition ? ScrollDirection.Decrement : ScrollDirection.Increment);
			
			// JJD 2/14/11 - TFS66166 - Optimization
			// Pass in the scroll direction to the GetRecordsExpectedToBeGenerated method
			//this.BeginGeneration(overallScrollPosition < this._lastOverallScrollPosition ? ScrollDirection.Decrement : ScrollDirection.Increment, 
			//    this.GetRecordsExpectedToBeGenerated(overallScrollPosition, lastNumberOfScrollableRecordsToLayout));
			ScrollDirection direction = overallScrollPosition < this._lastOverallScrollPosition ? ScrollDirection.Decrement : ScrollDirection.Increment;
			this.BeginGeneration(direction,
				this.GetRecordsExpectedToBeGenerated(overallScrollPosition, lastNumberOfScrollableRecordsToLayout, direction));

            this._lastMeasureScrollPosition = overallScrollPosition;

			this._lastOverallScrollPosition = overallScrollPosition;

			// JM BR28982 12-06-07 - The extent used by the header records in the non-primary dimension (i.e., if the orientation
			// is vertical then the non-primary dimension would be horizontal and this would represent the width used by the header records)
			double nonPrimaryExtentUsedByHeaderRecords = 0;

			if (totalItemsThisPanel > 0)
			{
				#region Measure fixed records on top and bottom

				int rootTopFixedAdjustment = 0;

                if (isRootPanel)
                    rootTopFixedAdjustment = topFixedCount;
                else
                {
                    GridViewPanelNested rootPanel = this.RootPanel as GridViewPanelNested;

                    if (rootPanel != null)
                    {
                        ViewableRecordCollection rootVrc = rootPanel.ViewableRecords;

                        // JJD 5/25/07
                        // Check to make sure that there is a ViewableRecordCollection on the root panel
                        if (rootVrc != null)
                            rootTopFixedAdjustment = rootVrc.CountOfFixedRecordsOnTop;
                    }
                }

                // JJD 6/30/09 - TFS18956
                // Re-factored code into CalculateLogicalListOffset helper
                int logicalListOffset = CalculateLogicalListOffset(isRootPanel, info, viewableRecords, topFixedCount, overallScrollPosition, rootTopFixedAdjustment);

                // JJD 6/30/09 - TFS18956
                // Keep track of the total # of rcds to generate
                int maxRecordsToGenerate;

                if (isRootPanel)
                    maxRecordsToGenerate = totalRecordCount;
                else
                {
                    maxRecordsToGenerate = totalRecordCount - logicalListOffset;

                    // JJD 6/30/09 - TFS18956
                    // if we aren't generating any records then end generation and 
                    // return an empty size
                    if (maxRecordsToGenerate < 1)
                    {
						// MD 4/28/11 - TFS36608
						// If there were no children present and children are above the parent, we will need to dirty the measure when the user scrolls up again.
						// So hook the RecordsInViewChanged event and cache the scroll position so we know when we scroll up.
						// JJD 11/16/11 - TFS25239
						// Always track when rcds in view have changed for non-root panels
						//if (viewableRecords.FieldLayout != null &&
						//    viewableRecords.FieldLayout.ChildRecordsDisplayOrderResolved != ChildRecordsDisplayOrder.AfterParent)
						if (isRootPanel == false ||
							(viewableRecords.FieldLayout != null &&
						     viewableRecords.FieldLayout.ChildRecordsDisplayOrderResolved != ChildRecordsDisplayOrder.AfterParent))
						{
							_scrollPositionWhenNoChildrenAbovePresent = info.OverallScrollPosition;				
							// JJD 11/16/11 - TFS25239
							// Replaced wiring the DP's RecordsInViewChanged event will a PropertyValueTracker 
							// so we don't root this element.
							// So instead of wiring the event we need to create a tracker which will
							// call us back when the DP's RecordsInViewVersion property changes
							//this.DataPresenter.RecordsInViewChanged += new EventHandler<RecordsInViewChangedEventArgs>(this.OnDataPresenterRecordsInViewChanged);
							if (_rcdsInViewTracker == null)
								_rcdsInViewTracker = new PropertyValueTracker(this.DataPresenter, DataPresenterBase.RecordsInViewVersionProperty, new PropertyValueTracker.PropertyValueChangedHandler(this.OnDataPresenterRecordsInViewChanged));
						}

                        this.EndGeneration();
                        return new Size(1, 1);
                    }
                }

				// generate the top fixed records to get the size they require
				FieldLayout lastFieldLayout = viewableRecords[0].FieldLayout;

                // JJD 1/15/09 - NA 2009 vol 1
                // Added support for header placement
                HeaderPlacement headerPlacement = lastFieldLayout != null 
                    ? lastFieldLayout.HeaderPlacementResolved
                    : HeaderPlacement.OnTopOnly;
                    
				int recordsGenerated;

                // JJD 7/1/09 - TFS18956
                // Keep track of the 1st record displayed in each area
                Record firstTopFixedRecordDisplayed = null;
                Record firstBottomFixedRecordDisplayed = null;
                Record firstScrollableRecordDisplayed = null;

                // JJD 1/15/09 - NA 2009 vol 1
                // Added support for header placement                   
                // JJD 6/30/09 - TFS18956
                // Added maxRecordsToGenerate and topRecordDisplayed params
                // JJD 2/20/09 - NA 2009 Vol 2 - Record fixing
                
                // Added lastRecordDisplayed param.
                Record lastRecordDisplayed = null;
                Size sizeOfFixedRecordsOnTop = this.GenerateRecordPresenters(new Size(double.PositiveInfinity, double.PositiveInfinity), true, false, rootTopFixedAdjustment, headerPlacement, maxRecordsToGenerate, ref firstTopFixedRecordDisplayed, ref lastRecordDisplayed, ref lastFieldLayout, ref highestItemIndexProcessed, out recordsGenerated);

				FieldLayout lastFieldLayoutTopFixed = lastFieldLayout;

				Size sizeOfFixedRecordsOnBottom;

				// generate the bottom fixed records to get the size they require
				if (totalItemsThisPanel <= topFixedCount + scrollableRecordCount)
				{
					sizeOfFixedRecordsOnBottom = new Size();
				}
				else
				{
					lastFieldLayout = viewableRecords[topFixedCount + scrollableRecordCount].FieldLayout;
                    // JJD 1/15/09 - NA 2009 vol 1
                    // Added support for header placement
                    // JJD 6/30/09 - TFS18956
                    // Added maxRecordsToGenerate and topRecordDisplayed params
                    // JJD 2/20/09 - NA 2009 Vol 2 - Record fixing
                    
                    // Added lastRecordDisplayed param.
                    // Pass in a dummy lastRecordDisplayed because since this is bottom fixed we don't care
                    Record dummy = null;
                    sizeOfFixedRecordsOnBottom = this.GenerateRecordPresenters(new Size(double.PositiveInfinity, double.PositiveInfinity), false, true, rootTopFixedAdjustment, headerPlacement, maxRecordsToGenerate, ref firstBottomFixedRecordDisplayed, ref dummy, ref lastFieldLayout, ref highestItemIndexProcessed, out recordsGenerated);
				}

				highestItemIndexProcessed = -1;

				// based on orientation get the extents used by the top and botton fixed records
				if (orientationIsVertical)
				{
					this._extentUsedByFixedRecordsOnTop = sizeOfFixedRecordsOnTop.Height;
					this._extentUsedByFixedRecordsOnBottom = sizeOfFixedRecordsOnBottom.Height;
				}
				else
				{
					this._extentUsedByFixedRecordsOnTop = sizeOfFixedRecordsOnTop.Width;
					this._extentUsedByFixedRecordsOnBottom = sizeOfFixedRecordsOnBottom.Width;
				}

				// for the root panel we need to know if the fixed records can fit in order to
				// adjust the viewport extent
				if (isRootPanel)
				{
					canTopFixedRecordsFit = availableExtent >= this._extentUsedByFixedRecordsOnTop;
					canAllFixedRecordsFit = availableExtent >= this._extentUsedByFixedRecordsOnTop + this._extentUsedByFixedRecordsOnBottom;
				}

				#endregion //Measure fixed records on top and bottom

				#region Calculate logical offsets

				Record firstRecordToDisplay = null;

				// JJD 3/09/07
				// Make sure we sync up the scroll offset with the scroll position on the root panel
				if (isRootPanel)
					scrollOffsetAsItemIndex = overallScrollPosition;

				if (logicalListOffset < 1)
				{
                    
                    
                    
                    if ( totalRecordCount > topFixedCount )
					    firstRecordToDisplay = viewableRecords[topFixedCount];
				}
				else
				{
					if (isRootPanel)
					{
						if (availableExtent <= 0)
						{
							#region Handle the case where the root panel doesn't have enough space to show all the fixed records

							// set the offset for scrollable records such that we don't show any
							this._offsetWithinListForScrollableRecords = totalRecordCount;

							// check if the offset is within the top fixed record range
							if (logicalListOffset < topFixedCount)
							{
								// offset the top records only
								this._offsetWithinListForTopFixedRecords = logicalListOffset;
								firstRecordToDisplay = viewableRecords[logicalListOffset];
							}
							else
							{
								// set the offset for the top fixed records such that we don't show any
								this._offsetWithinListForTopFixedRecords = topFixedCount;

								// [JM/JD 06-18-07]
								//firstRecordToDisplay = viewableRecords[totalRecordCount - bottomFixedCount];
								firstRecordToDisplay = viewableRecords[totalRecordCount - (bottomFixedCount + 1)];

								if (logicalListOffset >= totalRecordCount - bottomFixedCount)
									// [JM/JD 06-18-07]
									//this._offsetWithinListForBottomFixedRecords = logicalListOffset - (1 + totalRecordCount - bottomFixedCount);
									this._offsetWithinListForBottomFixedRecords = logicalListOffset - (totalRecordCount - bottomFixedCount);

							}

							#endregion //Handle the case where the root panel doesn't have enough space to show all the fixed records
						}
						else
						{
							#region Handle the normal case on the root panel where there is enought space for all fixed records

							// set the offset for scrollable records 
							this._offsetWithinListForScrollableRecords = Math.Max(0, Math.Min(logicalListOffset, totalRecordCount - (topFixedCount + bottomFixedCount + 1)));

							if (topFixedCount > 0)
								firstRecordToDisplay = viewableRecords[0];
							else
								firstRecordToDisplay = viewableRecords[this._offsetWithinListForScrollableRecords + topFixedCount];

							#endregion //Handle the normal case on the root panel where there is enought space for all fixed records
						}
					}
					else
					{
						#region Handle nested panels

						int remainingOffset = logicalListOffset;

						this._offsetWithinListForScrollableRecords = logicalListOffset;

						// set the first record to a scrollable record if in range
						if (logicalListOffset < scrollableRecordCount)
							firstRecordToDisplay = viewableRecords[this._offsetWithinListForScrollableRecords + topFixedCount];
						else
							remainingOffset -= scrollableRecordCount;

						if (firstRecordToDisplay == null)
						{
                            
                            
                            //this._offsetWithinListForTopFixedRecords = remainingOffset;

							// set the first record to a top fixed record if in range
							if (remainingOffset < topFixedCount)
								firstRecordToDisplay = viewableRecords[this._offsetWithinListForTopFixedRecords];
							else
								remainingOffset -= topFixedCount;
						}

						if (firstRecordToDisplay == null)
						{
							this._offsetWithinListForBottomFixedRecords = Math.Max(0, Math.Min(remainingOffset, bottomFixedCount - 1));

							// set the first record to a top fixed record if in range
                            // JJD 6/30/09 - TFS18956
                            // check bottom fixed rcds first ten try to pick an approriate rcd based on
                            // the # of top fixed rcds
                            if (bottomFixedCount > 0)
                                firstRecordToDisplay = viewableRecords[this._offsetWithinListForBottomFixedRecords + totalRecordCount - bottomFixedCount];
                            else
                                if (topFixedCount > 0)
                                    firstRecordToDisplay = viewableRecords[Math.Max(0, topFixedCount + scrollableRecordCount + 1 - this._offsetWithinListForScrollableRecords)];
						}

						


						#endregion //Handle nested panels
					}
				}

				#endregion //Calculate logical offsets

				#region Initialize header presenter in adorner layer

				// SSP 4/25/08 - Summaries Feature
				// 
				// --------------------------------------------------------------
				
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


                // JJD 1/15/09 - NA 2009 vol 1 - record filtering
                // We need to allow a header in the adorner layer if the first record is either
                // a filterrecord or a sumary record
                //if ( shouldShowHeaderInAdornerLayer = null != firstRecordToDisplay
                //    && RecordType.DataRecord == firstRecordToDisplay.ParentCollection.RecordsType 
                //    && null != firstRecordToDisplay.FieldLayout
                //    && firstRecordToDisplay.FieldLayout.LabelLocationResolved == LabelLocation.SeparateHeader;
				bool shouldShowHeaderInAdornerLayer = false;

                // JJD 7/1/09 - TFS18956
                // Use the actual first redord that willl be displayed for he header in the adorner layer 
                if (firstTopFixedRecordDisplayed != null)
                    firstRecordToDisplay = firstTopFixedRecordDisplayed;
                else if (firstRecordToDisplay == null)
                    firstRecordToDisplay = firstBottomFixedRecordDisplayed;


				// JM 03-05-09 - While investigating TFS11010 noticed that the Headers were not being shown when the XamDataPresenter's View property
				//				 was set to a new GridView (e.g., this.xamDataPresenter1.View = new GridView() ).  This was happening because 
				//				 FieldLayout.LabelLocationResolved was returning 'InCells' rather than 'SeparateHeader' because the FieldLayout had not
				//				 yet been intialized with the Style Generator.  The intialization normally happens when we start to generate presenters below.  
				//				 This didn't cause a problem prior to 9.1 but is now.  Fixing by forcing the FieldLayout to be initialized here if it is not
				//				 already.
				if (null != firstRecordToDisplay				&&
					null != firstRecordToDisplay.FieldLayout	&&
					null == firstRecordToDisplay.FieldLayout.StyleGenerator)
				{
					if (null != this.DataPresenter &&
						null != this.DataPresenter.CurrentViewInternal)
					{
						FieldLayoutTemplateGenerator generator = this.DataPresenter.CurrentViewInternal.GetFieldLayoutTemplateGenerator(firstRecordToDisplay.FieldLayout);
						if (generator != null)
							firstRecordToDisplay.FieldLayout.Initialize(generator);
					}
				}
                

                if ( null != firstRecordToDisplay 
					&& null != firstRecordToDisplay.FieldLayout
					&& firstRecordToDisplay.FieldLayout.LabelLocationResolved == LabelLocation.SeparateHeader )
                {
                    FieldLayout fl = firstRecordToDisplay.FieldLayout;

                    switch (firstRecordToDisplay.ParentCollection.RecordsType)
                    {
                        case RecordType.DataRecord:
                            // Always show the header above the datarecords if we aren't grouping or
                            // if the HeaderPlacementInGroupBy resolves to 'WithDataRecords'
                            if ( fl.HasGroupBySortFields == false ||
                                 fl.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.WithDataRecords )
                                shouldShowHeaderInAdornerLayer = true;
                            break;
                        case RecordType.GroupByFieldLayout:
                        case RecordType.GroupByField:
                            // Make sure this is not a nested groupby which never shows a header
                            if (!(firstRecordToDisplay.ParentCollection.ParentRecord is GroupByRecord))
                            {
                                // show the header if this is a FilterRecord or a SummaryRecord
                                // or if the HeaderPlacementInGroupBy resolves to 'OnTopOnly'
                                if (firstRecordToDisplay is FilterRecord ||
                                     firstRecordToDisplay is SummaryRecord ||
                                    fl.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.OnTopOnly)
                                    shouldShowHeaderInAdornerLayer = true;
                            }
                            break;
                    }
                }

				// JJD 10/26/11 - TFS91364
				// If this is a HeaderRecord then don't show another one in the AdornerLayer
				if (firstRecordToDisplay is HeaderRecord)
					shouldShowHeaderInAdornerLayer = false;

				// --------------------------------------------------------------

				if (shouldShowHeaderInAdornerLayer)
				{
					// SSP 4/25/08 - Summaries Feature
					// Related to the change above.
					//this._fieldLayoutForHeaderRecordInAdorner = dr.FieldLayout;
					this._fieldLayoutForHeaderRecordInAdorner = firstRecordToDisplay.FieldLayout;

					// JM BR28982 12-06-07
					//this._extentUsedByHeaderRecords =
					Size totalExtentUsedByHeaderRecords =
						this._gridViewPanelAdorner.CreateAndMeasureHeaderRecordPresenter(
							// SSP 4/25/08 - Summaries Feature
							// Related to the change above.
							//dr,
							firstRecordToDisplay,
							adjustedConstraint,
							orientationIsVertical,
							orientationIsVertical ? -1 * this.ScrollingData._offset.X : -1 * this.ScrollingData._offset.Y);

					// JM BR28982 12-06-07
					if (orientationIsVertical)
					{
						this._extentUsedByHeaderRecords		= totalExtentUsedByHeaderRecords.Height;
						nonPrimaryExtentUsedByHeaderRecords	= totalExtentUsedByHeaderRecords.Width;
					}
					else
					{
						this._extentUsedByHeaderRecords		= totalExtentUsedByHeaderRecords.Width;
						nonPrimaryExtentUsedByHeaderRecords	= totalExtentUsedByHeaderRecords.Height;
					}
				}

				#endregion //Initialize header presenter in adorner layer

				#region Measure the scrollable records

				overallExtentUsed = this._extentUsedByFixedRecordsOnTop
										+ this._extentUsedByFixedRecordsOnBottom
										+ this._extentUsedByHeaderRecords;

				Size sizeOfScrollableRecords;

				if (scrollableRecordCount == 0 || this._offsetWithinListForScrollableRecords >= scrollableRecordCount)
					sizeOfScrollableRecords = new Size();
				else
				{
					Size scrollableRecordConstraint = adjustedConstraint;

					if (orientationIsVertical)
						scrollableRecordConstraint.Height = Math.Max(availableExtent - overallExtentUsed, 0);
					else
						scrollableRecordConstraint.Width = Math.Max(availableExtent - overallExtentUsed, 0);

					FieldLayout fieldLayoutOfLastHeader;

					// if we had fixed records then use lastFieldLayoutTopFixed 
					// otherwise use the field layout of the header in the adorner layer
                    if (topFixedCount == 0)
                    {
                        // JJD 1/15/09 NA 2009 vol 1
                        // Since we may not have added a header in the adorner based on the
                        // headerplacement options we need to check to make sure the
                        // member is not null
                        if (this._fieldLayoutForHeaderRecordInAdorner != null)
                            fieldLayoutOfLastHeader = this._fieldLayoutForHeaderRecordInAdorner;
                        else
                            fieldLayoutOfLastHeader = lastFieldLayout;
                    }
                    else
                        fieldLayoutOfLastHeader = lastFieldLayoutTopFixed;

                    // JJD 1/15/09 - NA 2009 vol 1
                    // Added support for header placement
                    // JJD 6/30/09 - TFS18956
                    // Added maxRecordsToGenerate and topRecordDisplayed params
                    // JJD 2/20/09 - NA 2009 Vol 2 - Record fixing
                    
                    // Added lastRecordDisplayed param.
                    sizeOfScrollableRecords = this.GenerateRecordPresenters(scrollableRecordConstraint, false, false, rootTopFixedAdjustment, headerPlacement, maxRecordsToGenerate, ref firstScrollableRecordDisplayed, ref lastRecordDisplayed, ref fieldLayoutOfLastHeader, ref highestItemIndexProcessed, out recordsGenerated);

					this._numberOfScrollableRecordsToLayout = recordsGenerated;

					if (orientationIsVertical)
						overallExtentUsed += sizeOfScrollableRecords.Height;
					else
						overallExtentUsed += sizeOfScrollableRecords.Width;
				}

				this._firstVisualTopFixedItemIndex = this._offsetWithinListForTopFixedRecords;
				this._firstVisualScrollableItemIndex = topFixedCount + this._offsetWithinListForScrollableRecords;
				this._firstVisualBottomFixedItemIndex = topFixedCount + scrollableRecordCount + this._offsetWithinListForBottomFixedRecords;

				#endregion //Measure the scrollable records

				#region Calculate our desired size

				if (orientationIsVertical)
				{
                    // JJD 6/2/09 
                    // Make sure the nonPrimaryExtentUsedByHeaderRecords is taken into acoount.
                    // This covers the situation where child records are grouped and 
                    // HeaderPlacementIngroupBy is set to 'OnTop'
					//ourDesiredSize.Width = Math.Max(Math.Max(sizeOfFixedRecordsOnTop.Width, sizeOfFixedRecordsOnBottom.Width), sizeOfScrollableRecords.Width);
					ourDesiredSize.Width = Math.Max(nonPrimaryExtentUsedByHeaderRecords, Math.Max(Math.Max(sizeOfFixedRecordsOnTop.Width, sizeOfFixedRecordsOnBottom.Width), sizeOfScrollableRecords.Width));
					ourDesiredSize.Height = overallExtentUsed;
				}
				else
				{
					ourDesiredSize.Width = overallExtentUsed;
                    // JJD 6/2/09 
                    // Make sure the nonPrimaryExtentUsedByHeaderRecords is taken into acoount.
                    // This covers the situation where child records are grouped and 
                    // HeaderPlacementIngroupBy is set to 'OnTop'
					//ourDesiredSize.Height = Math.Max(Math.Max(sizeOfFixedRecordsOnTop.Height, sizeOfFixedRecordsOnBottom.Height), sizeOfScrollableRecords.Height);
					ourDesiredSize.Height = Math.Max(nonPrimaryExtentUsedByHeaderRecords, Math.Max(Math.Max(sizeOfFixedRecordsOnTop.Height, sizeOfFixedRecordsOnBottom.Height), sizeOfScrollableRecords.Height));
				}

				#endregion //Calculate our desired size

                
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

            }
			else
			{
				// JJD 5/31/07 - BR23233, BR23055, BR23285, BR23286
				#region Try to add a header based on the DataSource

				// If there are no records in the source list then get a field layout from the list itself (if possible)
				RecordCollectionBase rc = viewableRecords.RecordCollection;

				// JJD 5/31/07 - BR23233, BR23055, BR23285, BR23286
				// This does not apply to ExpandableFieldRecordCollection

				if (!(rc is ExpandableFieldRecordCollection))
				{
					// get the layout based on the data source

					// JJD 1/29/09 - NA 2009 vol 1 - Record filtering
					// If the parent record is an ExpandableFieldRecord then we
					// might be in a situation where all of the child records have 
					// been filtered out but we still need to show the header
					// so there is some Ui (i.e LabelIcons) in the header to change
					// the filter criteria. 
					// So we need to call the CheckHasChildData method which has been
					// updated to return true in this situation.
					// If it returns true then create a header in the adorner for
					// the field layout of the child records
					//FieldLayout fl = this.DataPresenter.RecordManager.FieldLayout;
					FieldLayout fl = null;

					if ((rc.ParentRecord is ExpandableFieldRecord))
					{
						if (((ExpandableFieldRecord)rc.ParentRecord).CheckHasChildData())
						{
							fl = rc.ParentRecordManager.FieldLayout;
						}
					}
					else
					{
						// JJD 2/9/09 - TFS13685/TFS13781
						// Make sure their is no parent data record. This should only be done for the root
						if (rc.ParentDataRecord == null)
						{
							// JJD 2/7/11 - TFS35853
							// If the parent rcd is a GroupByRecord then use its FieldLayout
							GroupByRecord parentGbr = rc.ParentRecord as GroupByRecord;
							if (parentGbr != null)
								fl = parentGbr.FieldLayout;
							else
								fl = rc.ParentRecordManager.FieldLayout;
						}
					}

					if (fl != null && fl.LabelLocationResolved == LabelLocation.SeparateHeader)
					{
						this._fieldLayoutForHeaderRecordInAdorner = fl;
						// JM BR28982 12-06-07
						//this._extentUsedByHeaderRecords =
						Size totalExtentUsedByHeaderRecords =
							this._gridViewPanelAdorner.CreateAndMeasureHeaderRecordPresenter(fl.TemplateDataRecord,
																								  adjustedConstraint,
																								  orientationIsVertical,
																								  orientationIsVertical ? -1 * this.ScrollingData._offset.X : -1 * this.ScrollingData._offset.Y);
						// JM BR28982 12-06-07
						if (orientationIsVertical)
						{
							this._extentUsedByHeaderRecords = totalExtentUsedByHeaderRecords.Height;
							nonPrimaryExtentUsedByHeaderRecords = totalExtentUsedByHeaderRecords.Width;
							// JJD 10/08/08 - TFS6430
							// Update the desired size
							ourDesiredSize.Height = this._extentUsedByHeaderRecords;
						}
						else
						{
							this._extentUsedByHeaderRecords = totalExtentUsedByHeaderRecords.Width;
							nonPrimaryExtentUsedByHeaderRecords = totalExtentUsedByHeaderRecords.Height;
							// JJD 10/08/08 - TFS6430
							// Update the desired size
							ourDesiredSize.Width = this._extentUsedByHeaderRecords;
						}

						// JJD 10/08/08 - TFS6430
						overallExtentUsed = this._extentUsedByHeaderRecords;
					}
				}

				#endregion //Try to add a header based on the DataSource
			}

			// AS 6/11/09 TFS18382
			_nonPrimaryExtentUsedByHeaderRecords = nonPrimaryExtentUsedByHeaderRecords;

			// set a flag if anything has changed

			// JJD 6/14/07
			// Only set the _recordsInViewChanged flag if the _useScrollPositionCandidate flag is false
			if (this._useScrollPositionCandidate == false)
			{
				if (lastOffsetWithinListForTopFixedRecords != this._offsetWithinListForTopFixedRecords ||
					lastOffsetWithinListForBottomFixedRecords != this._offsetWithinListForBottomFixedRecords ||
					lastOffsetWithinListForScrollableRecords != this._offsetWithinListForScrollableRecords ||
					lastNumberOfScrollableRecordsToLayout != this._numberOfScrollableRecordsToLayout)
					this._recordsInViewChanged = true;
			}

			this._lastOrientationWasVertical = orientationIsVertical;

			if (this._fieldLayoutForHeaderRecordInAdorner == null && this._gridViewPanelAdorner != null)
				this._gridViewPanelAdorner.ClearHeaderRecordPresenters();


			#region Update scrolling info (offset, viewport and extent) and desired size.

//			UIElementCollection internalChildren = base.InternalChildren;

			// JM BR28982 12-06-07 - If our desired size in the non-primary dimension is zero (because we have no records) make sure
			// that ourDesiredSize in the non-primary dimension reflects the extent we calculated for the headers.  This will ensure 
			// that we have an active scrollbar in the non-primary dimension if necessary even if we have no records.
			if (orientationIsVertical)
			{
				if (ourDesiredSize.Width == 0)
					ourDesiredSize.Width = nonPrimaryExtentUsedByHeaderRecords;
			}
			else
			{
				if (ourDesiredSize.Height == 0)
					ourDesiredSize.Height = nonPrimaryExtentUsedByHeaderRecords;
			}

			if (this.IsScrolling  ||  isRootPanel == false)
			{
				Vector	newScrollOffset = this.ScrollingData._offset;
				Size	scrollViewport	= effectiveConstraint;
				Size	scrollExtent	= ourDesiredSize;

				// Calculate the total number of items processed.
				int totalItemsProcessedThisPanel = this._numberOfScrollableRecordsToLayout;

				//if ((totalItemsProcessedThisPanel == 0) || availableExtent >= 0)
				//    totalItemsProcessedThisPanel++;

				// AS 3/15/07 BR21138
				// If there isn't enough room then we should consider that not all the records are in 
				// view and so we'll adjust the scrollable record count.
				//
				bool wasDesiredExtentGreater = false;
				if (isRootPanel)
				{
					if (orientationIsVertical)
						wasDesiredExtentGreater = ourDesiredSize.Height > effectiveConstraint.Height;
					else
						wasDesiredExtentGreater = ourDesiredSize.Width > effectiveConstraint.Width;
				}

				// Calculate the desired size to return from this measure.
				if (orientationIsVertical)
					ourDesiredSize.Height = Math.Min(ourDesiredSize.Height, effectiveConstraint.Height);
				else
					ourDesiredSize.Width = Math.Min(ourDesiredSize.Width, effectiveConstraint.Width);

				// AS 3/15/07 BR21138
				// Based on the comment, the following should only be done if the fixed records
				// were bumped to unfixed records in which case, this should be done only if 
				// canAllFixedRecordsFit is true so I moved it into the block below. Otherwise
				// when a record has child records, this will cause us to have too high of a scrollable
				// record count.
				//
				//// JJD 3/5/07
				//// adjust the scroll range if the fixed records need toscroll becuase of space limitations
				//if (isRootPanel)
				//    scrollableRecordCount = scrollCount - (topFixedCount + bottomFixedCount);

				if (canAllFixedRecordsFit == false)
				{
					// AS 3/15/07 BR21138
					// See above comment. Moved from outside the if block to inside.
					//
					// JJD 3/5/07
					// adjust the scroll range if the fixed records need toscroll becuase of space limitations
					if (isRootPanel)
						scrollableRecordCount = scrollCount - (topFixedCount + bottomFixedCount);

					scrollableRecordCount			+= bottomFixedCount;
					totalItemsProcessedThisPanel	+= bottomFixedCount - this._offsetWithinListForBottomFixedRecords;

					if (canTopFixedRecordsFit == false)
					{
						scrollableRecordCount			+= topFixedCount;
						totalItemsProcessedThisPanel	+= topFixedCount - this._offsetWithinListForTopFixedRecords;
					}
				}

				// JJD 3/06/07
				// if we need a scrollbar on the root panel bump the scrollableRecordCount by 1
				// to ensure that the thumb range encompases the last record.
				// AS 3/15/07 BR21138
				// The measure override is always using the cached extent as its extent which means
				// that instead of bumping the scroll record count, we should initialize it based
				// on the scrollable record count.
				//
				//if (isRootPanel && scrollableRecordCount > totalItemsProcessedThisPanel)
				//	scrollableRecordCount++;
				// AS 3/15/07 BR21138
				// If we didn't have enough room to show and we didn't adjust the 
				// scroll record count above (if the fixed rows were displayed as unfixed)
				// then reset the scrollable record count based on the total number of 
				// scrollable records.
				//
				if ((isRootPanel && scrollableRecordCount > totalItemsProcessedThisPanel) ||
					// AS 3/16/07
					// In the case of groupby, all the records may fix in view but there 
					// could be records scrolled out of view. If the scroll thumb isn't
					// at the top then reset the scrollable record count - i.e. the extent.
					//
					//(wasDesiredExtentGreater && canAllFixedRecordsFit))
					(canAllFixedRecordsFit && (wasDesiredExtentGreater || scrollOffsetAsItemIndex > 0)))
				{
					scrollableRecordCount = scrollCount - (topFixedCount + bottomFixedCount);

					if (scrollableRecordCount == totalItemsProcessedThisPanel)
						totalItemsProcessedThisPanel--;
				}

				totalItemsProcessedThisPanel = Math.Max(1, totalItemsProcessedThisPanel);

				#region Set the ScrollViewport and the ScrollExtent based on our orientation.

				int numberOfRecordsInExtent = Math.Max(1, scrollableRecordCount);

				// JJD 4/14/07
				// Account for nested records in the record extent.
				// This fixes a bug in the situation where there was a single root record
				// that was expanded but wasn't showing a vertical scrollbar.
				if (isRootPanel)
				{
					int overallScrollCount = info.OverallScrollCount;
                    if (overallScrollCount > numberOfRecordsInExtent)
                    {
                        
                        
                        
                        numberOfRecordsInExtent = Math.Max(numberOfRecordsInExtent, Math.Min(overallScrollCount, overallScrollCount + numberOfRecordsInExtent - totalRecordCount));
                    }
				}

				if (orientationIsVertical)
				{
					// Set a viewport value here based on the number of items processed this panel.
					// This will be refined in the Arrange when we can accurately determine the 
					// number of visible records from all panels.
					scrollViewport.Height		= totalItemsProcessedThisPanel;

					scrollExtent.Height			= numberOfRecordsInExtent;
					newScrollOffset.Y			= scrollOffsetAsItemIndex;
					newScrollOffset.X			= Math.Max(Math.Min(newScrollOffset.X, (double)(scrollExtent.Width - scrollViewport.Width)), 0d);

					// JM 07-17-06 - Also check that the available extent is not infinity (originally uncovered a problem that
					//				 requires this additional check when using a DataPresenterBase inside a ViewBox control) 
					//if (isRootPanel && totalItemsThisPanel > totalItemsProcessedThisPanel)
					//if (isRootPanel && totalItemsThisPanel > totalItemsProcessedThisPanel && double.IsInfinity(availableExtent) == false)
					//	ourDesiredSize.Height	= availableExtent;

					// JM 11-12-08 TFS9807
					scrollViewport.Width		= Math.Min(scrollViewport.Width, scrollExtent.Width);
                }
				else
				{
					// Set a viewport value here based on the number of items processed this panel.
					// This will be refined in the Arrange when we can accurately determine the 
					// number of visible records from all panels.
					scrollViewport.Width		= totalItemsProcessedThisPanel;

					scrollExtent.Width			= numberOfRecordsInExtent;
					newScrollOffset.X			= scrollOffsetAsItemIndex;
					newScrollOffset.Y			= Math.Max(Math.Min(newScrollOffset.Y, (double)(scrollExtent.Height - scrollViewport.Height)), 0d);


					// JM 07-17-06 - Also check that the available extent is not infinity (originally uncovered a problem that
					//				 requires this additional check when using a DataPresenterBase inside a ViewBox control) 
					//if (isRootPanel && totalItemsThisPanel > totalItemsProcessedThisPanel && double.IsInfinity(availableExtent) == false)
					//if (isRootPanel && totalItemsThisPanel > totalItemsProcessedThisPanel && double.IsInfinity(availableExtent) == false)
					//		ourDesiredSize.Width = availableExtent;

					// JM 11-12-08 TFS9807
					scrollViewport.Height		= Math.Min(scrollViewport.Height, scrollExtent.Height);
				}

				#endregion //Set the ScrollViewport and the ScrollExtent based on our orientation.


				// JJD 3/5/07 
				// If we are in an infinite container on the root panel then always return the correct height and/or width
				if (isRootPanel)
				{
					if (this._widthInInfiniteContainer > 1)
					{
						if (newScrollOffset.X > 0.0001)
							ourDesiredSize.Width = Math.Max(this._widthInInfiniteContainer, ourDesiredSize.Width);

						this._widthInInfiniteContainer = ourDesiredSize.Width;
					}

					if (this._heightInInfiniteContainer > 1)
					{
						if (newScrollOffset.Y > 0.0001)
							ourDesiredSize.Height = Math.Max(this._heightInInfiniteContainer, ourDesiredSize.Height);

						this._heightInInfiniteContainer = ourDesiredSize.Height;
					}
				}

				// Cache our calculated scrolling data and available extent which will 
				// be used in the Arrange to refine the scrollViewport calculation based on the
				// number of visible records from all panels.  Arrange will call InvalidateScrollInfo
				// on the ScrollOwner.
				if (this.IsScrolling)
				{
					this._cachedScrollDataFromMeasure				= new ScrollData();
					this._cachedScrollDataFromMeasure._extent		= scrollExtent;
					this._cachedScrollDataFromMeasure._viewport		= scrollViewport;
					this._cachedScrollDataFromMeasure._offset		= newScrollOffset;
					this._cachedRemainingAvailableExtentFromMeasure = Math.Max(availableExtent - overallExtentUsed, 0);
				}
			}

			#endregion //Update scrolling info (offset, viewport and extent) and desired size.


			// Set the IsBoundaryRecord flag on the RecordPresenters in our list.  We mark our first and
			// last child RecordPresenters as boundary records.  These boundary RecordPresenters bind an internal
			// property to the TopRecord property of the DataPresenterBase which, when set, causes the RecordPresenter
			// to re-measure itself and its parent.  We mark the first and last records as boundary records
			// because these are the records that can change during a scroll.
			//this.SetBoundaryRecordFlags(internalChildren);
			this.SetBoundaryRecordFlags();


			//// Queue a request to cleanup unused generated elements.
			//if (this._queuedCleanupRequest == null)
			//    this._queuedCleanupRequest = base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(this.ProcessQueuedCleanupRequest), null);

			// we don't want to return a zero size becase this screws up thew ScrollContentPresenter for some reason.
			ourDesiredSize.Width = Math.Max(ourDesiredSize.Width, 1);
			ourDesiredSize.Height = Math.Max(ourDesiredSize.Height, 1);

            
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			// 5/21/07 - Optimization
			// Call EndGeneration so we can support recycling 
			this.EndGeneration();

            // JJD 1/3/08 - BR28972
            // On the root panel if we have scroll down from the first record and we weren't passed
            // in an infinity available size then return the available size but only in the scrolling dimension.
            if (isRootPanel && overallScrollPosition > 0)
            {
                if (orientationIsVertical)
                {
                    if (!double.IsInfinity(originalAvailableSize.Height))
                        ourDesiredSize.Height = originalAvailableSize.Height;
                }
                else
                {
                    if (!double.IsInfinity(originalAvailableSize.Width))
                        ourDesiredSize.Width = originalAvailableSize.Width;
                }
            }

			//Debug.WriteLine(string.Format("GVP size: {0}, avaliable: {1}, IsRoot {2}, nesting depth {3}, scrollable rcds {4}", ourDesiredSize, originalAvailableSize, isRootPanel, this.ViewableRecords.Count == 0 ? -1 : this.ViewableRecords[0].NestingDepth, this._numberOfScrollableRecordsToLayout));

			//Debug.WriteLine("GVP measure: " + ourDesiredSize.ToString());

			// Return our calculated desired size.

 
            // JJD 10/16/09 - TFS22652
            // Added tracker for nested panels so they can invalidate their measure when the scroll version changes
            if (isRootPanel == false &&
                this._tracker == null)
            {
                DataPresenterBase dp = this.DataPresenter;

                if (dp != null)
                    this._tracker = new PropertyValueTracker(dp, DataPresenterBase.ScrollVersionProperty, new PropertyValueTracker.PropertyValueChangedHandler(OnScrollVersionChanged));
            }

			// JJD 8/16/10 - TFS30240
			// track the dp's RenderVersion property if we are in a nested panel so we can invalidate our measure
			if (isRootPanel == false &&
                this._renderVersionTracker == null)
            {
                DataPresenterBase dp = this.DataPresenter;

                if (dp != null)
                    this._renderVersionTracker = new PropertyValueTracker(dp, DataPresenterBase.RenderVersionProperty, new PropertyValueTracker.PropertyValueChangedHandler(OnScrollVersionChanged));
            }

			return ourDesiredSize;
		}

				#endregion //MeasureOverride

				// AS 5/10/07 Optimization
				// When hiding a record in the ArrangeOverride, the record will try to invalidate
				// the measure of the parent. However, since we know that the record will not be 
				// in view and have handled the measure there is no reason to cause another measure/arrange.
				//
				#region OnChildDesiredSizeChanged
		/// <summary>
		/// Overriden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
		/// </summary>
		/// <param name="child">The child element whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			if (this.IsHidingRecord)
				return;

			base.OnChildDesiredSizeChanged(child);
		} 
				#endregion //OnChildDesiredSizeChanged

				#region OnClearChildren

		/// <summary>
		/// Called when the children of the panel are cleared
		/// </summary>
		protected override void OnClearChildren()
		{
			base.OnClearChildren();

			// JJD 3/01/07 - BR19868
			// Don't reset the scroll offsets. They automatically get constrained 
			// within the proper bounds during the measure and arrange pass.
			//this.ScrollingData.Reset();
			//this.ViewPanelInfo.OverallScrollPosition	= 0;
			this._firstVisualBottomFixedItemIndex		= 0;
			this._firstVisualScrollableItemIndex		= 0;
			this._firstVisualTopFixedItemIndex			= 0;
			this._numberOfScrollableRecordsToLayout		= 0;
			this._visibleItemCount						= 0;
			this._offsetWithinListForTopFixedRecords	= 0;
			this._offsetWithinListForBottomFixedRecords = 0;
			this._offsetWithinListForScrollableRecords	= 0;
		}

				#endregion //OnClearChildren

				#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			base.OnItemsChanged(sender, args);

			// JJD 6/11/07
			// Clear the cached scroll count to trigger a rest on the next measure (affects the root panel only)

            // JJD 1/23/09 - NA 2009 vol 1 - record filtering
            // Only update the flags if we are not in a pseudo scroll
            if (!this._useScrollPositionCandidate)
            {
                this._lastOverallScrollCount = -1;

                this._recordsInViewChanged = true;
            }
		}

				#endregion //OnItemsChanged

				#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("GridViewPanelNested: ");

			if (this.IsRootPanel)
				sb.Append("Root");
			else
			{
				ViewableRecordCollection vrc = this.ViewableRecords;

				if (vrc != null && vrc.RecordCollection != null)
				{
					Record parentRcd = vrc.RecordCollection.ParentRecord;

					if (parentRcd != null)
					{
						sb.Append("Parent Record is ");
						sb.Append(parentRcd.ToString());
					}

				}
			}
    
			return sb.ToString();
		}

				#endregion //ToString	
    
				#region SetBoundaryRecordFlags







		// JJD 5/22/07 - Optimization
		// GridViewPanel now derives from RecyclingControlPanel 
		// so don't use InternalChildren
		//private void SetBoundaryRecordFlags(UIElementCollection internalChildren)
		internal override void SetBoundaryRecordFlags()
		{
			if (this._visibleItemCount > 0)
			{
				ViewableRecordCollection ourRecordCollection = this.ViewableRecords;
				
				// JJD 5/25/07
				// Check to make sure that there is a ViewableRecordCollection
				if (ourRecordCollection == null)
					return;

				int topFixedCount = ourRecordCollection.CountOfFixedRecordsOnTop;
				int bottomFixedCount = ourRecordCollection.CountOfFixedRecordsOnBottom;
				int countOfScrollableRecords = ourRecordCollection.Count - (topFixedCount + bottomFixedCount);

				Record firstScrollableRecord = ourRecordCollection[Math.Max(0, Math.Min(topFixedCount + countOfScrollableRecords - 1, topFixedCount + this._offsetWithinListForScrollableRecords))];
				
				// JJD 7/19/07 - BR24176
				// Only get the last record if it has already been allocated so we don't load the last record
				// unnecessarily
				//Record lastScrollableRecord = ourRecordCollection[Math.Max(0, topFixedCount + countOfScrollableRecords - 1)];
				Record lastScrollableRecord = null;
				DataRecord dr = firstScrollableRecord as DataRecord;
				
				// JJD 8/02/07 - BR22909
				// Make sure dr.ParentCollection.Count is > than 0 before calling GetItem. Otherwise an IndexOutOfRange exception is raised
				//if (dr == null ||
				//    dr.ParentCollection.SparseArray.GetItem(dr.ParentCollection.Count - 1, false) != null)
				//	lastScrollableRecord = ourRecordCollection[Math.Max(0, topFixedCount + countOfScrollableRecords - 1)];
				int indexOfLastScrollableRecord = Math.Max(0, topFixedCount + countOfScrollableRecords - 1);

				if (dr != null )
				{ 
					int sparseArrayCount = dr.ParentCollection.Count;

					if ( sparseArrayCount > 0 )
					{
						// JJD 8/02/07 - BR22909
						// If the last record in the sparse array has not been allocated then set
						// the indexOfLastScrollableRecord to -1 so we don't cause a premature
						// allocation of the record below
						if (dr.ParentCollection.SparseArray.GetItem( sparseArrayCount - 1, false) == null)
							indexOfLastScrollableRecord = -1;
					}
				}
				
				// JJD 8/02/07 - BR22909
				// Only accesss the last record if the indexOfLastScrollableRecord is > -1
				if (indexOfLastScrollableRecord >= 0 )
					lastScrollableRecord = ourRecordCollection[indexOfLastScrollableRecord];

				// JJD 5/22/07 - Optimization
				// GridViewPanel now derives from RecyclingControlPanel 
				// so don't use InternalChildren
				//int childCount = InternalChildren.Count;
				// JJD 6/6/07
				// Use Children collection to only access active children
				//int childCount = this.CountOfActiveContainers;
				// AS 7/9/07
				//IList children = this.Children;
				IList children = this.ChildElements;
				int childCount = children.Count;

				for (int i = 0; i < childCount; i++)
				{
					// JJD 5/22/07 - Optimization
					// GridViewPanel now derives from RecyclingControlPanel 
					// so don't use InternalChildren
					//RecordPresenter rp = internalChildren[i] as RecordPresenter;
					// JJD 6/6/07
					// Use Children collection to only access active children
					//RecordPresenter rp = this.GetChildElement(i) as RecordPresenter;
					// SSP 9/18/08 BR34595
					// The elements can be containers that contain record presenters.
					// 
					//RecordPresenter rp = children[i] as RecordPresenter;
					RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer( children[i] as DependencyObject );
					if (rp != null)
					{
						Record record = rp.Record;
						rp.IsBoundaryRecord = record == firstScrollableRecord || record == lastScrollableRecord;
					}
				}
			}
		}

				#endregion //SetBoundaryRecordFlags

                #region SetOffsetInternal

        internal override void SetOffsetInternal(double newOffset, double oldOffset, bool isSettingVerticalOffset)
        {
            try
            {
                ScrollData scrollData = this.ScrollingData;


                // JJD 8/3/07
                // Moved from below
                bool orientationIsVertical = this.LogicalOrientation == Orientation.Vertical;
                bool isOffsettingRecords = orientationIsVertical == isSettingVerticalOffset;

                double newOffsetNormalized;

                // JJD 8/3/07
                // When we are not scrolling records we want to constrain the offset by the max
                // of extent minus viewport
                //if ( isSettingVerticalOffset )
                //    newOffsetNormalized = Math.Min(Math.Max(0, newOffset), scrollData._extent.Height - 1);
                //else
                //    newOffsetNormalized = Math.Min(Math.Max(0, newOffset), scrollData._extent.Width - 1);

                double adjustment;

                // JJD 8/3/07
                // Calculate the appropriate constrain adjustment (see note above)
                if (isOffsettingRecords)
                    adjustment = 1;
                else
                    if (isSettingVerticalOffset)
                        adjustment = scrollData._viewport.Height;
                    else
                        adjustment = scrollData._viewport.Width;

                if (isSettingVerticalOffset)
                    newOffsetNormalized = Math.Min(Math.Max(newOffset, 0), scrollData._extent.Height - adjustment);
                else
                    newOffsetNormalized = Math.Min(Math.Max(newOffset, 0), scrollData._extent.Width - adjustment);

                // JJD 7/11/07
                // Make sure the new offset is not negative 
                if (newOffsetNormalized < 0)
                    newOffsetNormalized = 0.0;

                // JJD 5/27/08 - BR32292
                // If records can't be scrolled then set new offset to 0
                if (isOffsettingRecords && !this.CanRecordsBeScrolled)
                    newOffsetNormalized = 0.0;

                if (newOffsetNormalized == oldOffset)
                    return;

                // JJD 8/3/07
                // Moved code above
                //bool orientationIsVertical	= this.LogicalOrientation == Orientation.Vertical;
                //bool isOffsettingRecords	=  orientationIsVertical == isSettingVerticalOffset;


                // JJD 6/6/07
                // If the last scrollable record is already in view and they are trying to scroll down
                // past that then return

                // JJD 2/4/09 - TFS13472
                // Don't bail out if we are in a deferred drag situation since that will prevent
                // a drag down after a drag up
                //if (isOffsettingRecords &&
                //     newOffsetNormalized > oldOffset &&
                //     this._isLastScrollableRecordFullyInView)
                if (isOffsettingRecords &&
                     newOffsetNormalized > oldOffset &&
                     this._isLastScrollableRecordFullyInView &&
                     scrollData._isInDeferredDrag == false)
                {
                    return;
                }

                IViewPanelInfo info = this.ViewPanelInfo;

                // JJD 7/11/07
                //Make sure info is non-null
                Debug.Assert(info != null);
                if (info == null)
                    return;

                // check with the dp if it is ok to scroll
                if (!info.IsOkToScroll())
                    return;

                // JJD 6/12/07
                // See if we are paging forward by more than one record
                bool isPagingForwardMultipleRecords = isOffsettingRecords && newOffsetNormalized > oldOffset + 1;

                Record newTopRecord = null;


                // No need to continue if we have more vertical space than we need.
                if (!isOffsettingRecords)
                {
                    if (this._cachedScrollDataFromMeasure != null)
                    {
                        if (isSettingVerticalOffset)
                        {
                            if (this._cachedScrollDataFromMeasure._extent.Height <= this._cachedScrollDataFromMeasure._viewport.Height)
                                return;
                        }
                        else
                        {
                            if (this._cachedScrollDataFromMeasure._extent.Width <= this._cachedScrollDataFromMeasure._viewport.Width)
                                return;
                        }
                    }
                }

                // If this SetVerticalOffset was triggered by a PageUp, make sure we page up by an amount
                // that will result in the record immediately preceeding the current display being displayed
                // as the last record in the display after the scroll.
                if (this.SetOffsetTriggeredByPageBack && isOffsettingRecords)
                {
                    int topRecordScrollPos = info.OverallScrollPosition;

                    if (topRecordScrollPos > 0)
                        this.DetermineTopRecordForGivenBottomRecordIndex(ref newTopRecord, ref newOffsetNormalized, topRecordScrollPos - 1);
                }

                if (isOffsettingRecords)
                {
					// AS 9/2/09 TFS21748
					// Do not access (and therefore cause to be allocated) a record while using a ScrollingMode of Deferred.
					//
					bool getTopRecord = !scrollData._isInDeferredDrag || scrollData._scrollTip != null;

					if (getTopRecord)
						newTopRecord = this.GetTopRecordFromScrollOffset((int)newOffsetNormalized);

                    // initialize the scroll tip if there is one
                    scrollData.InitializeScrollTip(newTopRecord);

                    // if we're deferred dragging...
                    if (scrollData._isInDeferredDrag)
                    {
                        // store the temp position
                        scrollData._deferredDragOffset = newOffsetNormalized;
                        //Debug.WriteLine(string.Format("Set deferred offset: {0}", newOffsetNormalized));
                        return;
                    }

                    // don't fire the scroll event if we're not changing the top record
                    if (newOffsetNormalized == info.OverallScrollPosition)
                        return;

                    // JJD 6/14/07
                    // if we are paging forward then see if we are near the end
                    if (isPagingForwardMultipleRecords &&
                        newOffsetNormalized > 0 &&
                        scrollData._averageHeightOfRecords >= 1)
                    {
                        double panelExtent;

                        if (isSettingVerticalOffset)
                            panelExtent = this.ActualHeight;
                        else
                            panelExtent = this.ActualWidth;

                        // JJD 6/14/07
                        // Calculate a predicted number of records on the next page (err on the side of expecting too many)
                        double predictedRecordsOnNextPage = 1 + (panelExtent / Math.Max(15, scrollData._averageHeightOfRecords / 2));

                        int lastRecordScrollPosition = this.ViewPanelInfo.OverallScrollCount - 1;

                        // JJD 6/14/07
                        // Based on the expected number calculated above see if it is likely that the
                        // last record will come into view 
                        if (lastRecordScrollPosition <= newOffsetNormalized + predictedRecordsOnNextPage)
                        {
                            int originalScrollPos = (int)newOffsetNormalized;
                            int scrollPos = originalScrollPos;

                            // JJD 6/14/07
                            // Keep moving the scrollpos back until the last record would not be fully in view
                            // and use the scrollpos just before that happens
                            // JJD 7/13/09 - TFS19156
                            // Only decrement the new offset while it is greater than the old offset 
                            //while (scrollPos >= 0 && this.WillLastRecordBeVisibleForGivenTopRecordIndex(scrollPos))
                            while (scrollPos >= 0 && scrollPos > oldOffset && this.WillLastRecordBeVisibleForGivenTopRecordIndex(scrollPos))
                            {
                                newOffsetNormalized = scrollPos;
                                scrollPos--;
                            }

                            if (originalScrollPos > newOffsetNormalized)
                                newTopRecord = this.GetTopRecordFromScrollOffset((int)newOffsetNormalized);
                        }
                    }

                    // JJD 1/3/08 - BR26779
                    // At this point we know that the records in view have changed so set the flag here
                    // so we know to raise the event in the arrange
                    this._recordsInViewChanged = true;
                }


                if (newTopRecord != null)
                {
                    ViewableRecordCollection vrc = this.ViewableRecords;

                    // JJD 5/25/07
                    // Check to make sure that there is a ViewableRecordCollection
                    if (vrc == null)
                        return;

                    // JJD 8/3/09 - TFS16793
                    // Adjust for CountOfFixedRecordsOnBottom instead of CountOfSpecialRecordsOnBottom
                    //newOffsetNormalized = Math.Max(0, Math.Min(newOffsetNormalized, info.OverallScrollCount - (1 + vrc.CountOfFixedRecordsOnTop + vrc.CountOfSpecialRecordsOnBottom)));
                    newOffsetNormalized = Math.Max(Math.Min(newOffsetNormalized, info.OverallScrollCount - (1 + vrc.CountOfFixedRecordsOnTop + vrc.CountOfFixedRecordsOnBottom)), 0);
                }

                // Set the new offset and (if necessary) TopRecord.

                if (isSettingVerticalOffset)
                {
                    if (scrollData._offset.Y != newOffsetNormalized)
                    {
                        // JJD 6/11/07
                        // Set a flag that the offset has changed
                        scrollData._hasOffsetChanged = true;
                        scrollData._offset.Y = newOffsetNormalized;
                    }
                }
                else
                {
                    if (scrollData._offset.X != newOffsetNormalized)
                    {
                        // JJD 6/11/07
                        // Set a flag that the offset has changed
                        scrollData._hasOffsetChanged = true;
                        scrollData._offset.X = newOffsetNormalized;
                    }
                }

                if (newTopRecord != null)
                {
                    // JJD 1/27/09
                    // Initialize the _lastRecordAtOverallScrollPosition to the newTopRecord
                    // so we don't try to reset it in the MeasureOverride
                    this._lastRecordAtOverallScrollPosition = newTopRecord;
                    this._lastOverallScrollPosition = (int)newOffsetNormalized;
                    info.OverallScrollPosition = this._lastOverallScrollPosition;
                }


                // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                this.VerifyFixedFieldInfo();

                this.OnScrollInfoChange();
                this.InvalidateMeasure();
            }
            finally 
            { 
                this.SetOffsetTriggeredByPageBack = false; 
            }
        }

                #endregion //SetOffsetInternal	
    
            #endregion //Method

        #endregion //Base class overrides

        #region Properties

            #region Private Properties

                #region ViewableRecords

		// AS 8/21/09 TFS19388
		// Changed to internal so the adorner can access it for the header DRP it creates.
		//
		//private ViewableRecordCollection ViewableRecords
		internal ViewableRecordCollection ViewableRecords
        {
            // JJD 8/7/09 - NA 2009 Vol 2 - Enhanced grid view
            // Use ViewableRecords property instead
            //get { return this.RecordListControl.ItemsSource as ViewableRecordCollection; }
            get { return this.RecordListControl.ViewableRecords; }
        }

                #endregion //ViewableRecords

            #endregion //Private Properties	
    
        #endregion //Properties	

        #region Methods

            #region Private Methods

				#region ArrangeHelper

		private double ArrangeHelper(UIElement visualChildElement,
								 bool orientationIsVertical,
								 bool isReverseLayoutForBottomFixed,
								 double extentUsedByLastItem,
                                 // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
                                 // added clipExtent param
                                 double clipExtent,
                                 // AS 6/22/09 NA 2009.2 Field Sizing
								 // Removed isAutoFit - this wasn't used.
								 //
								 //bool isAutoFit,
								 Size arrangeSize,
								 ref Rect arrangeRect)
		{

			// SSP 9/18/08 BR34595
			// If visualChildElement is a record presenter container then get the contained record presenter.
			// 
			//RecordPresenter rp = visualChildElement as RecordPresenter;
			RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer( visualChildElement );

			// JJD 3/27/07
			// Make sure the rp knows that it is arranged in view
			if (rp != null)
			{
				rp.TreatAsCollapsed = false;

				if (rp.ShouldDisplayRecordContent)
					rp.IsArrangedInView = true;
			}

			if (orientationIsVertical)
			{
				arrangeRect.Height = visualChildElement.DesiredSize.Height;

				// JJD 3/27/07
				// If autofit is true then use the panel's arrange extent
                // JJD 1/21/09 - NA 2009 vol 1
                // Allow scrolling in autofit mode
                //if (isAutoFit)
                //    arrangeRect.Width = arrangeSize.Width;
                //else
					arrangeRect.Width = Math.Max(arrangeSize.Width, visualChildElement.DesiredSize.Width);

				if (isReverseLayoutForBottomFixed)
					// SSP 4/29/08 BR32427
					// ArrangeRect.Y has the value of last item's top. Therefore all we need to do here is
					// subtract the current item's height to get the top where the current item needs to go.
					// 
					//arrangeRect.Y -= extentUsedByLastItem + arrangeRect.Height;
					arrangeRect.Y -= arrangeRect.Height;
				else
					arrangeRect.Y += extentUsedByLastItem;

				extentUsedByLastItem = arrangeRect.Height;

				if (Utilities.DoubleIsZero(arrangeRect.Y))
					arrangeRect.Y = 0;
			}
			else
			{
				arrangeRect.Width = visualChildElement.DesiredSize.Width;

				// JJD 3/27/07
				// If autofit is true then use the panel's arrange extent
                // JJD 1/21/09 - NA 2009 vol 1
                // Allow scrolling in autofit mode
                //if (isAutoFit)
                //    arrangeRect.Height = arrangeSize.Height;
                //else
					arrangeRect.Height = Math.Max(arrangeSize.Height, visualChildElement.DesiredSize.Height);

				if (isReverseLayoutForBottomFixed)
					// SSP 4/29/08 BR32427
					// ArrangeRect.Y has the value of last item's top. Therefore all we need to do here is
					// subtract the current item's height to get the top where the current item needs to go.
					// 
					//arrangeRect.X -= extentUsedByLastItem + arrangeRect.Width;
					arrangeRect.X -= arrangeRect.Width;
				else
					arrangeRect.X += extentUsedByLastItem;

				extentUsedByLastItem = arrangeRect.Width;

				if (Utilities.DoubleIsZero(arrangeRect.X))
					arrangeRect.X = 0;
			}

            //Debug.WriteLine("GVP arrange: " + arrangeRect.ToString());

            // JJD 4/29/10 - Optimization
            // Moved below until after the clip has been applied . This is not only more
            // efficient but it corrects a problem with the 4.0 framework where clearing the
            // clip after the arrange displated the record as still being clipped from a
            // prior arrange
            //visualChildElement.Arrange(arrangeRect);

            // JJD 8/11/09 - NA 2009 Vol 2 - Enhanced grid view 
            // added clipExtent param
            bool shouldClip = !double.IsInfinity(clipExtent);
			
			// JJD 12/8/11 - TFS97329
			// Refactored - moved logic into helper method that can be called by any GridViewPanel derived class
			SetRecordPresenterClip(orientationIsVertical, extentUsedByLastItem, clipExtent, arrangeRect, shouldClip, rp);

            // JJD 4/29/10 - Optimization
            // Moved from above until after the clip has been applied . This is not only more
            // efficient but it corrects a problem with the 4.0 framework where clearing the
            // clip after the arrange displated the record as still being clipped from a
            // prior arrange
            visualChildElement.Arrange(arrangeRect);

			return extentUsedByLastItem;
		}

				#endregion //ArrangeHelper	

                // JJD 6/30/09 - TFS18956 - added
                #region CalculateLogicalListOffset

        private static int CalculateLogicalListOffset(bool isRootPanel, IViewPanelInfo info, ViewableRecordCollection viewableRecords, int topFixedCount, int overallScrollPosition, int rootTopFixedAdjustment)
        {
            int firstRecordScrollPosition = viewableRecords[0].OverallScrollPosition;

            // 5/21/07 - Optimization
            // Moved above so we could use it to determine scroll direction
            //int overallScrollPosition = this.EffectiveScrollPosition;
            int logicalListOffset = 0;

            // JJD 6/30/09 - TFS18956
            // For child panels use an adjustment that includes its topfixedcount as well as the roottopfixedcount 
            int topAdjustment = rootTopFixedAdjustment;
            if (isRootPanel == false)
                topAdjustment += topFixedCount;

            // calculate the logical offset into this list based on the overall scroll position 
            // JJD 6/30/09 - TFS18956
            // use adjustemt calculated above
            
            if (overallScrollPosition + topAdjustment >= firstRecordScrollPosition)
            {
                // JJD 6/30/09 - TFS18956
                // use adjustemt calculated above
                
                Record overallScrollPositionRecord = info.GetRecordAtOverallScrollPosition(overallScrollPosition + topAdjustment);
                Record siblingOverallScrollPositionRecord = viewableRecords.GetRecordInThisCollectionFromDescendant(overallScrollPositionRecord);

                if (siblingOverallScrollPositionRecord != null)
                {
                    logicalListOffset = viewableRecords.IndexOf(siblingOverallScrollPositionRecord) - topFixedCount;
                }
                else
                {
                    // JJD 6/30/09 - TFS18956
                    // Caclulate an appropriate offset based on the scroll position
                    logicalListOffset = overallScrollPosition + rootTopFixedAdjustment - firstRecordScrollPosition;
                }
            }

            return logicalListOffset;
        }

                #endregion //CalculateLogicalListOffset	

                #region DetermineTopRecordForGivenBottomRecordIndex



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        private void DetermineTopRecordForGivenBottomRecordIndex(ref Record newTopRecord, ref double newOffsetNormalized, int targetScrollIndexForLastVisibleRecord)
        {
            int currentScrollIndexOfLastVisibleRecord = -1;
            Record currentTopRecordCandidate = this.GetTopRecordFromScrollOffset((int)newOffsetNormalized);
            DataPresenterBase dp = this.DataPresenter;
            IViewPanelInfo info = this.ViewPanelInfo;
            int totalIterations = 0;
            GridViewPanelNested rootPanel = this.RootPanel as GridViewPanelNested;


            try
            {
                this._useScrollPositionCandidate = true;

                Record previousTopRecordCandidate = null;
                int previousTopRecordCandidateScrollPos = -1;
                int dupCount = 0;
                bool? previousCurrentLessThanTarget = null;


                while (currentScrollIndexOfLastVisibleRecord != targetScrollIndexForLastVisibleRecord)
                {
                    if (currentTopRecordCandidate != null)
                    {
                        this._scrollPositionCandidate = currentTopRecordCandidate.OverallScrollPosition;

                        // JJD 3/12/07 
                        // we need to bump the RecordsInViewVersion on the dp to invalidate nested panels
                        dp.BumpRecordsInViewVersion();
                    }

                    //					Size ourSize = new Size(this.ActualWidth, this.ActualHeight);
                    this.InvalidateMeasure();
                    this.InvalidateArrange();
                    // JJD 3/12/07 
                    // Don't call measure directly since this can screw up our infinite container logic
                    //					this.Measure(ourSize);
                    this.UpdateLayout();
                    // JJD 3/12/07 
                    // No need to call raange since UpdateLayout will do that automatically
                    //					this.Arrange(new Rect(ourSize));

                    int currentTopRecordCandidateScrollPos = info.GetOverallScrollPositionForRecord(currentTopRecordCandidate);

                    // JJD 3/12/07 Use the cached value on the rootpanel
                    //currentScrollIndexOfLastVisibleRecord = this.GetScrollIndexOfLastVisibleRecord();
                    currentScrollIndexOfLastVisibleRecord = rootPanel._scrollPositionOfLastVisibleRecord;

                    if (currentScrollIndexOfLastVisibleRecord != targetScrollIndexForLastVisibleRecord)
                    {
                        Record recTemp;

                        bool currentLessThanTarget = currentScrollIndexOfLastVisibleRecord < targetScrollIndexForLastVisibleRecord;

                        // JJD 3/14/07
                        // see if the direction changed since the last iteration and exit out if it has
                        if (previousCurrentLessThanTarget.HasValue &&
                             previousCurrentLessThanTarget.Value != currentLessThanTarget)
                        {
                            if (previousCurrentLessThanTarget.Value == true)
                            {
                                newTopRecord = currentTopRecordCandidate;
                                newOffsetNormalized = currentTopRecordCandidateScrollPos;
                            }
                            else
                            {
                                newTopRecord = previousTopRecordCandidate;
                                newOffsetNormalized = previousTopRecordCandidateScrollPos;
                            }
                            break;
                        }

                        // JJD 3/14/07
                        // keep track of the directionto compare on the nextr iteration
                        previousCurrentLessThanTarget = currentLessThanTarget;

                        if (currentLessThanTarget)
                            recTemp = info.GetRecordAtOverallScrollPosition(currentTopRecordCandidateScrollPos + 1);
                        else
                        {
                            if (currentTopRecordCandidateScrollPos > 0)
                                recTemp = info.GetRecordAtOverallScrollPosition(currentTopRecordCandidateScrollPos - 1);
                            else
                                recTemp = null;
                        }


                        // If recTemp use the currentTopRecordCandidate info and we're done.
                        if (recTemp == null)
                        {
                            newTopRecord = currentTopRecordCandidate;
                            newOffsetNormalized = currentTopRecordCandidateScrollPos;

                            break;
                        }


                        // Make sure we are not in a loop bouncing back and forth between 2 TopRecordCandidates.
                        // If so, pick the one that gets us closest to the targetScrollIndexForLastVisibleRecord.
                        // Modified this logic to allow the same TopRecordCandidate to be tried twice by adding
                        // a count (dupCount).  Found that we needed this in certain scenarios (yuck).
                        if (recTemp == previousTopRecordCandidate && dupCount > 0)
                        {
                            if (Math.Abs(targetScrollIndexForLastVisibleRecord - currentScrollIndexOfLastVisibleRecord) <
                                Math.Abs(targetScrollIndexForLastVisibleRecord - previousTopRecordCandidateScrollPos))
                            {
                                newTopRecord = currentTopRecordCandidate;
                                newOffsetNormalized = currentTopRecordCandidateScrollPos;

                                break;
                            }
                            else
                            {
                                newTopRecord = previousTopRecordCandidate;
                                newOffsetNormalized = info.GetOverallScrollPositionForRecord(previousTopRecordCandidate);

                                break;
                            }
                        }
                        else
                        {
                            // Update the dupCount if necessary
                            if (recTemp == previousTopRecordCandidate)
                                dupCount++;

                            previousTopRecordCandidate = currentTopRecordCandidate;
                            // [JM 05-04-07]
                            previousTopRecordCandidateScrollPos = currentTopRecordCandidateScrollPos; //currentScrollIndexOfLastVisibleRecord;
                            currentTopRecordCandidate = recTemp;
                            totalIterations++;
                        }
                    }
                    else
                    {
                        newTopRecord = currentTopRecordCandidate;
                        newOffsetNormalized = currentTopRecordCandidateScrollPos;

                        break;
                    }
                }
            }
            finally { this._useScrollPositionCandidate = false; }

            //#if DEBUG
            //            Debug.WriteLine("DetermineTopRecordForGivenBottomRecordIndex # iterations: " + totalIterations.ToString());
            //#endif

        }

                #endregion //DetermineTopRecordForGivenBottomRecordIndex	
    
				#region GenerateRecordPresenters

		private Size GenerateRecordPresenters(Size adjustedConstraint, 
												bool topFixedRecordsOnly, 
												bool bottomFixedRecordsOnly, 
												int rootTopFixedAdjustment,
                                                // JJD 1/15/09 - NA 2009 vol 1
                                                // Added support for header placement
                                                HeaderPlacement headerPlacement,
                                                // JJD 6/30/09 - TFS18956
                                                // Added maxRecordsToGenerate and TopRecordDsplayed params 
                                                int maxRecordsToGenerate,
                                                ref Record topRecordDisplayed,
                                                // JJD 2/20/09 - NA 2009 Vol 2 - Record fixing
                                                
                                                // Added lastRecordDisplayed param
                                                ref Record lastRecordDisplayed,
												ref FieldLayout lastFieldLayout,
												ref int highestItemIndexProcessed,
												out int numberOfRecordsGenerated)
		{
			// Generate and measure enough elements to fill the available space.
			Size totalSizeReguired = new Size();
			numberOfRecordsGenerated = 0;

			// JJD 5/22/07 - Optimization
			// GridViewPanel now derives from RecyclingControlPanel 
			// so don't use InternalChildren
			//UIElementCollection internalChildren		 = base.InternalChildren;
			ViewableRecordCollection ourRecordCollection = this.ViewableRecords;

			// JJD 5/25/07
			// Check to make sure that there is a ViewableRecordCollection
			if (ourRecordCollection == null)
				return totalSizeReguired;
			
			int totalItemsThisPanel = ourRecordCollection.Count;
			int startIndex = 0;
			int endIndex = -1;

			GridViewSettings viewSettings = this.ViewSettings;

			if (viewSettings == null)
				return totalSizeReguired;

			if (topFixedRecordsOnly)
			{
				// generate all of the top fixed records
                int topFixedCount = ourRecordCollection.CountOfFixedRecordsOnTop;
				
				startIndex = 0;
				
				if ( topFixedCount > 0 )
					endIndex = startIndex + topFixedCount - 1;

                // JJD 6/30/09 - TFS18956
                // Offset the startindex so we do not exceed the maxRecordsToGenerate param 
                if (topFixedCount > maxRecordsToGenerate)
                    startIndex += topFixedCount - maxRecordsToGenerate;
			}
			else
			if (bottomFixedRecordsOnly)
			{
				// generate all of the bottom fixed records
				int bottomFixedCount = ourRecordCollection.CountOfFixedRecordsOnBottom;

				startIndex = ourRecordCollection.Count - bottomFixedCount;

				if (bottomFixedCount > 0)
					endIndex = startIndex + bottomFixedCount - 1;
			}
			else
			{
				int topFixedCount = ourRecordCollection.CountOfFixedRecordsOnTop;
				int bottomFixedCount = ourRecordCollection.CountOfFixedRecordsOnBottom;
				int countOfScrollableRecords = totalItemsThisPanel - (topFixedCount + bottomFixedCount);

				// the _offsetWithinListForScrollableRecords must have been previously calculated in the meause
				// after the top and bottom fixed records were calculated
				if (this._offsetWithinListForScrollableRecords < countOfScrollableRecords)
				{
					startIndex = topFixedCount + this._offsetWithinListForScrollableRecords;
					endIndex = topFixedCount + countOfScrollableRecords - 1;
				}
			}

			if (endIndex < startIndex)
				return totalSizeReguired;

			int firstIndex = 0;

			GeneratorPosition generatorStartPosition = this.GetGeneratorPositionFromItemIndex( startIndex, out firstIndex);

			int indexToInsertNewlyRealizedItems = -1;
			int highestScrollPosition = 0;

            // JJD 6/20/08 
            // Moved up from below so we get it once 
            GridViewPanelNested rootPanel = this.RootPanel as GridViewPanelNested;

			#region Loop through items and generate/measure elements

			if (totalItemsThisPanel > 0)
			{
				IItemContainerGenerator generator = this.GetGenerator();
	
				using (IDisposable disposable1 = generator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
				{
					int currentItemIndex = startIndex;

					// JJD 5/22/07 - Optimization
					// GridViewPanel now derives from RecyclingControlPanel 
					// so cache the child count
					// AS 7/9/07
					//int childCount = this.Children.Count;
					int childCount = this.ChildElements.Count;

					bool orientationIsVertical = (viewSettings.Orientation == Orientation.Vertical);
					double availableExtent;

			        if (orientationIsVertical)
						availableExtent = adjustedConstraint.Height;
					else
						availableExtent = adjustedConstraint.Width;
                    
                    // JJD 3/16/09 
                    // cache a flag o we know whether we need to invalidate the measure
                    // of all record presenters
                    bool invalidateAllRecordMeasures = false;

                    // JJD 6/20/08 
                    // Added cache for last extent so we don't oscillate between measures
                    // with the height of the other scrollbar first taken out then
                    // included. This only applies to the root panel.
                    if (topFixedRecordsOnly == false &&
                         bottomFixedRecordsOnly == false &&
                        this == rootPanel)
                    {
                        Visibility computedScrollbarVisibility = Visibility.Hidden;

                        ScrollViewer sv = this.ScrollOwner;

                        // JJD 6/20/08 
                        // Based on the orientation get the computed scrollbar visibility
                        if ( sv != null )
                        {
					        if (orientationIsVertical)
                                computedScrollbarVisibility = sv.ComputedHorizontalScrollBarVisibility;
                            else
                                computedScrollbarVisibility = sv.ComputedVerticalScrollBarVisibility;
                        }

                        // JJD 6/20/08 
                        // Use the last extent tha was cached when the scrollbar was collapsed
                        // if it was larger than this extent and the computed
                        // scrollbar visibility is now visible
                        if ( computedScrollbarVisibility == Visibility.Visible &&
                             availableExtent < this._lastScrollableExtentUsedInMeasureOnRootPanel)
                        {
                            availableExtent = this._lastScrollableExtentUsedInMeasureOnRootPanel;

                            // JJD 3/16/09 
                            // Since we are changing the available extent we need to invalidate
                            // the measures of all record presenters in case the last one overlaps
                            // the panel just enough to turn on/off the scrollbar and cause a
                            // continuous invalidation of the measue of the panel
                            invalidateAllRecordMeasures = true;
                        }
                        else
                        if ( computedScrollbarVisibility == Visibility.Collapsed )
                            this._lastScrollableExtentUsedInMeasureOnRootPanel = availableExtent;
                    }

                    // JJD 1/15/09 - NA 2009 vol 1 - added support for header placement
                    // keep track of the last record.
                    // JJD 2/20/09 - NA 2009 Vol 2 - Record fixing
                    
                    // Added lastRecordDisplayed param so we don't ned the stack variable
                    //Record lastRecord = null;

					while (currentItemIndex < totalItemsThisPanel && 
							currentItemIndex <= endIndex)
					{
						double totalExtentUsedThisElement = 0;
						bool isNewlyRealized;
						UIElement generatedElement = generator.GenerateNext(out isNewlyRealized) as UIElement;
						if (generatedElement == null)
							break;

						// Cast the generated element as a RecordPresenter.
						// SSP 3/28/08 - Summaries Functionality
						// The element could be container that contains a record presenter. Moved this below
						// because the container element may need to be prepared before we can get record 
						// presenter from it.
						// 
						//RecordPresenter recordPresenter = generatedElement as RecordPresenter;

						// If the generated item is 'newly realized', add it to our children collection
						// and 'prepare' it.
                        if (isNewlyRealized)
                        {
                            if (indexToInsertNewlyRealizedItems < 0)
                            {
                                indexToInsertNewlyRealizedItems = this.FindSlotForNewlyRealizedRecordPresenter(currentItemIndex);
                            }

                            // JJD 5/22/07 - Optimization
                            // GridViewPanel now derives from RecyclingControlPanel 
                            // so don't use InternalChildren
                            //if (indexToInsertNewlyRealizedItems >= internalChildren.Count)
                            if (indexToInsertNewlyRealizedItems >= childCount)
                                this.AddInternalChild(generatedElement);
                            else
                                this.InsertInternalChild(indexToInsertNewlyRealizedItems, generatedElement);

                            // JJD 5/22/07 - Optimization
                            // update the childcount
                            childCount++;

                            generator.PrepareItemContainer(generatedElement);
                        }

                        
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


                        // SSP 3/28/08 - Summaries Functionality
						// The element could be container that contains a record presenter.
						// Moved this below from above since we need to prepare the container so it gets associated
						// with the right record presenter.
						// 
						RecordPresenter recordPresenter = RecordListControl.GetRecordPresenterFromContainer( generatedElement );

						// Decide whether we should show a header.
						if (recordPresenter != null)
						{
							Record rcd = recordPresenter.Record;

							// JJD 4/14/07
							// If the rcd was set to Collapsed then loop around and pick up the nest record
							// Note: This can happen if the generate caused a lazy creation of the Record and
							// its Visibility was set to Collapsed in the InitializeRecord event
							if (rcd == null ||
								 rcd.VisibilityResolved == Visibility.Collapsed)
								continue;

                            // JJD 7/1/09 - TFS18956
                            // Set the ref param to pass back the 1st record generated
                            if (topRecordDisplayed == null)
                                topRecordDisplayed = rcd;

							bool shouldShowHeader = false;

								// SSP 4/23/08 - Summaries Feature
								// A summary record can also have a record. Commented out the condition
								// that checked for record being DataRecord and instead added one that
								// checks for record collection type.
								// 
								//(recordPresenter.Record is DataRecord == true) &&
							if (RecordType.DataRecord == rcd.ParentCollection.RecordsType &&
								(recordPresenter.Visibility == Visibility.Visible) &&
								(recordPresenter.FieldLayout.LabelLocationResolved == LabelLocation.SeparateHeader))
                            {
								if (recordPresenter.FieldLayout != lastFieldLayout) 
                                    shouldShowHeader = true;
                                else
                                {
                                    // JJD 1/15/09 - NA 2009 vol 1
                                    // Added support for header placement of 'OnRecordBreak'
                                    // If the previous record was expanded and had visible children
                                    // then we want to show the header for this next sibling record
                                    // JJD 2/20/09 - NA 2009 Vol 2 - Record fixing
                                    
                                    // Use passed in lastRecordDisplayed param instead of stack variable
                                    if (headerPlacement == HeaderPlacement.OnRecordBreak &&
                                        lastRecordDisplayed != null &&
                                        lastRecordDisplayed.IsExpanded &&
                                        lastRecordDisplayed.HasChildren &&
                                        lastRecordDisplayed.ScrollCountInternal > 1)
                                    {
                                        shouldShowHeader = true;
                                    }
                                }
                            }

                            // JJD 2/20/09 - NA 2009 Vol 2 - Record fixing
                            
                            // set lastRecordDisplayed param
                            lastRecordDisplayed = rcd;

							recordPresenter.InitializeHeaderContent(shouldShowHeader);

							// JJD 3/27/07
							// Reset the TreatAsCollapsed and IsArrangedInView flags on the record presenter
							recordPresenter.TreatAsCollapsed = false;
							recordPresenter.IsArrangedInView = true;
						}


						// bump the count of records generated 
						numberOfRecordsGenerated++;

						// Decide whether we should show the content area of the RecordPresenter.
						if (recordPresenter != null)
						{
							// Do not show the content if the current top record is a descendant of this RecordPresenter
							Record parentRecord = this.GetChildsAncestorRecordInCollection(this.EffectiveScrollPosition + rootTopFixedAdjustment,
																								 ourRecordCollection);

							// MD 6/9/10 - ChildRecordsDisplayOrder feature
							// If the parent is below the child records, it should remain in view even when the current top record 
							// is a descendant of this RecordPresenter.
							//bool shouldShowContent = (parentRecord != recordPresenter.Record);
							bool shouldShowContent = recordPresenter.Record.AreChildrenAfterParent == false || (parentRecord != recordPresenter.Record);

                            // JJD 6/29/09 - NA 2009 Vol 2 - Record fixing
                            // If the record is fixed then we always want to show te content
                            if (shouldShowContent == false)
                                shouldShowContent = recordPresenter.Record.IsFixed;

							recordPresenter.InitializeRecordContentVisibility(shouldShowContent);
							recordPresenter.InitializeExpandableRecordContentVisibility(shouldShowContent);
							recordPresenter.InitializeGroupByRecordContentVisibility(shouldShowContent);

							// JM 10-14-08 TFS7711 
							if (isNewlyRealized == false)
								recordPresenter.InitializeIsAlternate(true);

							if (shouldShowContent && bottomFixedRecordsOnly == false)
								highestScrollPosition = Math.Max(highestScrollPosition, recordPresenter.Record.OverallScrollPosition);
						}

						// Update the variable we are using to keep track of the last field layout encountered.
						lastFieldLayout = recordPresenter.FieldLayout;


						// Bump some counters.

						if ( indexToInsertNewlyRealizedItems >= 0 )
							indexToInsertNewlyRealizedItems++;

						this._visibleItemCount++;

                        if (!isNewlyRealized &&
                            // JJD 3/16/09 
                            // also check the invalidateAllRecordMeasures flag
                            //this._lastOrientationWasVertical != orientationIsVertical)
                             (invalidateAllRecordMeasures || this._lastOrientationWasVertical != orientationIsVertical))
                        {
                            generatedElement.InvalidateMeasure();
                        }


						// Measure the generated element using the adjusted constraint.
						generatedElement.Measure(adjustedConstraint);

						Size generatedElementDesiredSize = generatedElement.DesiredSize;


						// Update our desired size and total extent used based on the generated element's 
						// desired size.
						if (orientationIsVertical)
						{
							totalExtentUsedThisElement += generatedElementDesiredSize.Height;
							totalSizeReguired.Width = Math.Max(totalSizeReguired.Width, generatedElementDesiredSize.Width);
							totalSizeReguired.Height += totalExtentUsedThisElement;
						}
						else
						{
							totalExtentUsedThisElement += generatedElementDesiredSize.Width;
							totalSizeReguired.Width += totalExtentUsedThisElement;
							totalSizeReguired.Height = Math.Max(totalSizeReguired.Height, generatedElementDesiredSize.Height);
						}


						availableExtent -= totalExtentUsedThisElement;
						availableExtent = Math.Max(availableExtent, 0);

						// Exit loop if we have used up the available extent.
						if (availableExtent <= 0)
						{
							highestItemIndexProcessed = currentItemIndex;
							break;
						}

						currentItemIndex++;
					}
				}
			}

			#endregion //Loop through items and generate/measure elements

            // JJD 6/20/08 
            // Moved above so we can use it for another test
            //			GridViewPanel rootPanel = this.RootPanel;

			// SSP 6/18/08 BR33922
			// Enclosed the existing code into the if block. Below block calculates and keeps track of 
			// _scrollPositionOfLastVisibleRecord on the root panel by calculating highestScrollPosition
			// in above logic. However we don't calculate highestScrollPosition if bottomFixedRecordsOnly 
			// is true and therefore we should not rely on the value of bottomFixedRecordsOnly in that 
			// case.
			// 
			if ( !bottomFixedRecordsOnly )
			{
				// JJD 3/12/07 keep track of the highest scrollposition visible on the root panel

				// JJD 5/31/07
				// If the cached _scrollPositionOfLastVisibleRecord is being updated on the root
				// panel make sure to invalidate its arrange to trigger a re-verification of the number
				//rootPanel._scrollPositionOfLastVisibleRecord = Math.Max(rootPanel._scrollPositionOfLastVisibleRecord, highestScrollPosition);
				int scrollPositionOfLastVisibleRecord = Math.Max( rootPanel._scrollPositionOfLastVisibleRecord, highestScrollPosition );
 
                // JJD 1/23/09 - NA 2009 vol 1 - record filtering
                // If we are in a root panel and not in a pseudo scroll
                // check if the overall scroll count has changed.
                // If so reset the highest scroll position to what we just generated
                if (this.IsRootPanel &&
                    this._useScrollPositionCandidate == false &&
                    this._lastOverallScrollCount != this.ViewPanelInfo.OverallScrollCount)
                {
                    scrollPositionOfLastVisibleRecord = highestItemIndexProcessed;
                    this._lastOverallScrollCount = this.ViewPanelInfo.OverallScrollCount;

                }

				if ( scrollPositionOfLastVisibleRecord != rootPanel._scrollPositionOfLastVisibleRecord )
				{
					rootPanel._scrollPositionOfLastVisibleRecord = scrollPositionOfLastVisibleRecord;
					rootPanel.InvalidateArrange( );
				}
			}

			return totalSizeReguired;
		}

				#endregion //GenerateRecordPresenters	
    
				#region GetChildsAncestorRecordInCollection

		private Record GetChildsAncestorRecordInCollection(int overallScrollPosition, ViewableRecordCollection collection)
		{
			if (collection == null)
				return null;

			IViewPanelInfo info = this.ViewPanelInfo;

			Record childRecord = info.GetRecordAtOverallScrollPosition(overallScrollPosition);

			if (childRecord == null)
				return null;

			Record ancestorRecord = collection.GetRecordInThisCollectionFromDescendant(childRecord);

			// since GetRecordInThisCollectionFromDescendant will return the childRecord
			// if it is in the collection we need to check for that case and return null
			if (ancestorRecord == childRecord)
				return null;

			return ancestorRecord;
		}

				#endregion //GetChildsAncestorRecordInCollection

				#region GetLastAbsoluteScrollableRecord

		// SSP 6/20/08 BR33922
		// 
		/// <summary>
		/// Returns the last absolute scrollable record in the grid, across all record hierachies.
		/// </summary>
		/// <returns></returns>
		internal Record GetLastAbsoluteScrollableRecord( )
		{
			ViewableRecordCollection records = this.ViewableRecords;
			//Debug.Assert( null != records );
			if ( null == records )
				return null;

			int ii = records.ScrollCount - 1;
			while ( ii >= 0 )
			{
				Record record = records.GetRecordAtScrollPosition( ii );
				if ( null != record && !record.IsFixed )
					return record;

				ii--;
			}

			return null;
		}

				#endregion // GetLastAbsoluteScrollableRecord

				#region GetLastAbsoluteScrollableRecordScrollPos

		// SSP 6/20/08 BR33922
		// 
		/// <summary>
		/// Returns the scroll index of last absolute scrollable record in the grid, across 
		/// all record hierachies.
		/// </summary>
		/// <returns></returns>
		internal int GetLastAbsoluteScrollableRecordScrollPos( )
		{
			Record record = this.GetLastAbsoluteScrollableRecord( );
			return null != record ? record.OverallScrollPosition : -1;
		}

				#endregion // GetLastAbsoluteScrollableRecordScrollPos

                // JJD 3/10/10 - TFS28705 - Optimization
                #region GetRecordsExpectedToBeGenerated

		// JJD 2/14/11 - TFS66166 - Optimization
		// Added scrollDirection param
		//private IEnumerable GetRecordsExpectedToBeGenerated(int overallScrollPosition, int lastNumberOfScrollableRcds)
		private IEnumerable GetRecordsExpectedToBeGenerated(int overallScrollPosition, int lastNumberOfScrollableRcds, ScrollDirection scrollDirection)
        {
            if (this.IsRootPanel == false || 
                this.ItemContainerGenerationModeResolved != ItemContainerGenerationMode.Recycle)
                return null;

            ViewableRecordCollection vrc = this.ViewableRecords;

            if (vrc == null)
                return null;

            int count = vrc.Count;
            int countTopFixed = vrc.CountOfFixedRecordsOnTop;
            int countBottomFixed = vrc.CountOfFixedRecordsOnBottom;
            int scrollableRecordsToGen = 0;            
            
            Record recordAtScrollPosition = this.GetTopRecordFromScrollOffset(overallScrollPosition);

            int firstScrollableIndex = recordAtScrollPosition != null ? vrc.IndexOf(recordAtScrollPosition) : -1;

            if (firstScrollableIndex >= 0)
            {
                int countOfScrollableRecords = count - (countTopFixed + countBottomFixed);
				
				// JJD 2/14/11 - TFS66166 - Optimization
				// Only increase the number of scrollable rcds to pass in wehen we are scrolling down.
				// When we are scrolling up we should err slightly on the side of passing fewer rcds
				// in so we don't risk excluding extra rcds from the recycling
                //scrollableRecordsToGen = Math.Max(Math.Min(countOfScrollableRecords - firstScrollableIndex, lastNumberOfScrollableRcds + 10), 0);
				int adjustment = scrollDirection == ScrollDirection.Increment ? 10 : -1;

                scrollableRecordsToGen = Math.Max(Math.Min(countOfScrollableRecords - firstScrollableIndex, lastNumberOfScrollableRcds + adjustment), 0);
            }
            
            List<Record> recordsExpected = new List<Record>(countTopFixed + countBottomFixed + scrollableRecordsToGen);

            // add top fixed records
            for (int i = 0; i < countTopFixed; i++)
                recordsExpected.Add(vrc[i]);

            // add bottom fixed records
            for (int i = 0; i < countBottomFixed; i++)
                recordsExpected.Add(vrc[count - (i + 1)]);
 
            // add scrollable rcds
            for (int i = 0; i < scrollableRecordsToGen; i++)
                recordsExpected.Add(vrc[firstScrollableIndex + i]);
       

            return recordsExpected;
        }

                #endregion //GetRecordsExpectedToBeGenerated	

				#region InitializeAdorner

		private void InitializeAdorner()
		{
			if (this._gridViewPanelAdorner != null)
				return;

			this._gridViewPanelAdorner = new GridViewPanelAdorner(this);

			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
			if (adornerLayer != null)
				adornerLayer.Add(this._gridViewPanelAdorner);
		}

				#endregion //InitializeAdorner	

				#region OnHeaderSizeChanged // JJD 9/26/07 - BR25884

		// JJD 9/26/07 - BR25884
		// Added method that is called by the GridViewPanelAdorner when the extent of the header has changed
		// AS 6/11/09 TFS18382
		//internal void OnHeaderExtentChanged(FieldLayout fl, double newExtent)
		internal void OnHeaderExtentChanged(FieldLayout fl, double newExtent, double nonPrimaryExtent)
		{
			DataPresenterBase dp = this.DataPresenter;

			// AS 6/11/09 TFS18382
			// Since the GridViewPanel uses the width of the DRP in the header when determining 
			// the size to return from the measure, we may need to invalidate the measure when 
			// the header size changes. Not doing this was causing the horizontal scrollbar to 
			// show up even though it wasn't needed. This change was only needed in 9.2 because 
			// of a change Joe made on 6/2 involving nested group by records and the 
			// nonPrimaryExtentUsedByHeaderRecords.
			//
			// AS 6/22/09 NA 2009.2 Field Sizing
			// I was going to change this to check the FieldLayout's IsAutoFit. However, there are 
			// other cases where the nonPrimaryExtentUsedByHeaderRecords is causing issues. For example 
			// if you resize the header within the nested content of an expandable field record smaller 
			// the header and cells get smaller but the expandable field record stays as wide because 
			// its using the _nonPrimaryExtentUsedByHeaderRecords in the value it returns from its 
			// measure. So if the header changes we need to dirty the measure.
			//
			//if (dp != null && dp.AutoFitResolved)
			{
				// AS 5/5/10 TFS29508
				// We only did this because we didn't want to remove the Math.Floor call in CalculateCellAreaAutoFitExtent.
				// Now that we removed that we can remove this adjustment. This adjustment was causing us to ignore small 
				// resize changed in the grid which led to the horizontal scrollbar showing up sometimes.
				//
				//if (!GridUtilities.AreClose(nonPrimaryExtent, _nonPrimaryExtentUsedByHeaderRecords, 1d))
				if (!GridUtilities.AreClose(nonPrimaryExtent, _nonPrimaryExtentUsedByHeaderRecords))
					this.InvalidateMeasure();
			}


			Debug.Assert(this._fieldLayoutForHeaderRecordInAdorner != null);
			Debug.Assert(this._fieldLayoutForHeaderRecordInAdorner == fl);
			Debug.Assert( fl != null && fl.LabelLocationResolved == LabelLocation.SeparateHeader);

			if (this._fieldLayoutForHeaderRecordInAdorner == null ||
				this._fieldLayoutForHeaderRecordInAdorner != fl || 
				fl.LabelLocationResolved != LabelLocation.SeparateHeader)
				return;

			if (newExtent == this._extentUsedByHeaderRecords)
				return;

			this._extentUsedByHeaderRecords = newExtent;
            this.InvalidateArrange();

            // AS 1/28/09 TFS12909
            // When the InternalVersion of the FieldLayout is changed (such as when the AllowFixing of a Field is changed),
            // the RecordPreseter clears its _headerContent. The RP in the adorner only gets its InitializeHeaderContent 
            // called from within the measure of the gridviewpanel (or more accurately from within the adorner's 
            // CreateAndMeasureHeaderRecordPresenter which is only called within the measure of the gridviewpanel).
            //
            this.InvalidateMeasure();
		}

				#endregion //OnHeaderSizeChanged	

                // JJD 05/29/08 -  BR30387 - added
                #region OnLayoutUpdated

		// JJD 3/15/11 - TFS65143 - Optimization
		// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
		// and just wire LayoutUpdated on the DP
        //private void OnLayoutUpdated(object sender, EventArgs e)
        private void OnLayoutUpdated()
        {
			// JJD 3/15/11 - TFS65143 - Optimization
			// No need to unhook since the DP is maintaining the callback list and automatically removes the entry
			// unhook the event
			// this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

            // JJD 05/29/08 -  BR30387
            // If the RootPanel is not null just invalidate the measure
            if (this.RootPanel != null)
                this.InvalidateMeasure();
        }

                #endregion //OnLayoutUpdated	

                // JJD 2/19/09 - TFS13979 - added
                #region OnLayoutUpdatedForArrange

        private void OnLayoutUpdatedForArrange(object sender, EventArgs e)
        {
            // unwire the event
            this.LayoutUpdated -= new EventHandler(OnLayoutUpdatedForArrange);

            // JJD 2/19/09 - TFS13979
            // Invzalidate the arrange after a sort version change
            // This is necessary because when the order of hydrated records
            // changes in a nested situation their are situations where
            // it will seems that the last record is in view even though it
            // isn't. This gets fixed up by the framework after the layout is 
            // completely updated.
            this.InvalidateArrange();
        }

                #endregion //OnLayoutUpdatedForArrange	

				// MD 4/28/11 - TFS36608
				#region OnDataPresenterRecordsInViewChanged

		// JJD 11/16/11 - TFS25239
		// Replaced wiring the DP's RecordsInViewChanged event will a PropertyValueTracker 
		// so we don't root this element.
		// Therefore needed to change the signature of the callback method to take no parameters
		//private void OnDataPresenterRecordsInViewChanged(object sender, RecordsInViewChangedEventArgs e)
		private void OnDataPresenterRecordsInViewChanged()
		{
			// JJD 11/16/11 - TFS25239
			// If the element is not longer hooked up then return
			DataPresenterBase dp = this.DataPresenter;
			if ( dp == null )
			{
				_rcdsInViewTracker = null;
				return;
			}

			// If we have scrolled above the position where no children were visible, invalidate the measure because we may 
			// now have visible children above.
			// JJD 11/16/11 - TFS25239
			// Make sure ViewPanelInfo is not null
			//if (_scrollPositionWhenNoChildrenAbovePresent >= 0 && this.ViewPanelInfo.OverallScrollPosition < _scrollPositionWhenNoChildrenAbovePresent)
			if (_scrollPositionWhenNoChildrenAbovePresent >= 0)
			{
				IViewPanelInfo info = this.ViewPanelInfo;
				
				if ( info != null &&info.OverallScrollPosition < _scrollPositionWhenNoChildrenAbovePresent)
					this.InvalidateMeasure();
			}
		}

				#endregion // OnDataPresenterRecordsInViewChanged

                #region OnScrollVersionChanged

        // JJD 10/16/09 - TFS22652
        // Added tracker for nested panels so they can invalidate their measure when the scroll version changes
        private void OnScrollVersionChanged()
        {
            // only invalidate the measure if we are visible
            if (this.IsVisible)
                this.InvalidateMeasure();

            // always clear the tracker. Another one will get created on the next measure
            this._tracker = null;

			// JJD 8/16/10 - TFS30240
			// always clear the _renderVersionTracker. Another one will get created on the next measure
			this._renderVersionTracker = null;
        }

                #endregion //OnScrollVersionChanged	
    
				#region PostBottomRecordVerification

		private void PostBottomRecordVerification(Size availableScrollSize)
		{
            // JJD 2/19/09 - support for printing.
            // We can't do asynchronous operations during a report operation
            DataPresenterBase dp = this.DataPresenter;
            if (dp == null || dp.IsReportControl)
                return;

            // JJD 6/11/07
			// Post a verification to make sure we don't allow any extra white space at the bottom 
			if (this._useScrollPositionCandidate == false &&
				this._isLastScrollableRecordFullyInView == true &&
				this._queuedBottomRecordVerification == null)
				this._queuedBottomRecordVerification = base.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DispatcherOperationCallback(this.ProcessQueuedBottomRecordVerification), availableScrollSize);
		}

				#endregion //PostBottomRecordVerification	
    
				#region ProcessQueuedBottomRecordVerification

		private object ProcessQueuedBottomRecordVerification(object args)
		{
			try
			{
                if (this._isProcessingBottomRecordVerification == true ||
                    this._isLastScrollableRecordFullyInView == false ||
                    this._useScrollPositionCandidate == true ) 
                    // JJD 3/2/09 - TFS14713
                    // Instead of checking for the _offsetWithinListForScrollableRecords check for
                    // the scroll pos below
                    // ||
                    //this._offsetWithinListForScrollableRecords == 0)
                    return null;

                // JJD 3/2/09 - TFS14713
                // ache the scroll pos in a stack variable
                int scrollPos = this.EffectiveScrollPosition;

                // JJD 3/2/09 - TFS14713
                // If the scrollpos is 0 then return
                if (scrollPos == 0)
                    return null;

				// JM BR29008 12-11-07
				if ((Size)args						== this._lastBottomRecordVerificationAvailableSize &&
                    // JJD 3/2/09 - TFS14713 - Optimization
                    // use the cached scrollpos
					//this.EffectiveScrollPosition	== this._lastBottomRecordVerificationScrollPos)
					scrollPos	                    == this._lastBottomRecordVerificationScrollPos )
					return null;
				else
				{
					this._lastBottomRecordVerificationAvailableSize	= (Size)args;
					this._lastBottomRecordVerificationScrollPos		= this.EffectiveScrollPosition;
				}


				this._isProcessingBottomRecordVerification = true;

				this._queuedBottomRecordVerification = null;

				#region Re-verify that the last record is in view

				ViewableRecordCollection ourRecordCollection = this.ViewableRecords;

                // JJD 7/13/09 - TFS19156
                // Make sure viewablerecords is not null
                if (ourRecordCollection == null)
                    return null;

				bool isLastRecordFullyInView = this.VerifyScrollPositionOfLastVisibleRecord(new Size(this.ActualWidth, this.ActualHeight));

				int ixLast = this._scrollPositionOfLastVisibleRecord;
				int ixFirst = this.EffectiveScrollPosition;
				int bottomFixedCount = ourRecordCollection.CountOfFixedRecordsOnBottom;
				int totalRecordCount = ourRecordCollection.Count;
				int bottomFixedToGenerate = Math.Max(0, bottomFixedCount - this._offsetWithinListForBottomFixedRecords);

				int ixLastScrollableRecord;

				if (isLastRecordFullyInView)
				{
					if (bottomFixedCount > 0)
						ixLastScrollableRecord = ourRecordCollection[totalRecordCount - (1 + bottomFixedToGenerate)].OverallScrollPosition;
					else
						ixLastScrollableRecord = this.ViewPanelInfo.OverallScrollCount - 1;

					// SSP 6/18/08 BR33922
					// 
					//this._isLastScrollableRecordFullyInView = ixLast >= ixLastScrollableRecord;
					this._isLastScrollableRecordFullyInView = ixLast >= ixLastScrollableRecord
						&& ixLast >= this.GetLastAbsoluteScrollableRecordScrollPos( );
				}
				else
					this._isLastScrollableRecordFullyInView = false;

				// If the last record is not in view then update the scrollData and exit 
				if (this._isLastScrollableRecordFullyInView == false)
				{
					ScrollData scrollData = this.ScrollingData;

					this.VerifyScrollData(scrollData._extent, scrollData._viewport, scrollData._offset, (Size)args);

					return null;
				}

				#endregion //Re-verify that the last record is in view	
    
				//int scrollPos = this.EffectiveScrollPosition;
				scrollPos = this.EffectiveScrollPosition;

				int originalScrollPos = scrollPos;
				int scrollPosToUse = scrollPos;

				// keep subtracting one from the top record scroll pos until the last record is not fully in view
				while (scrollPos >= 0 && this.WillLastRecordBeVisibleForGivenTopRecordIndex(scrollPos))
				{
                    // JJD 1/15/08 - BR26347
                    // Make a redeundant call to re-confirm that WillLastRecordBeVisibleForGivenTopRecordIndex
                    // returns true. The reason this is necessary is that in certain situations (due to timing/sequencing issues)
                    // the first call can return true in error. This situation seems to only happen when
                    // we are recycling record presenters.
                    if (!this.WillLastRecordBeVisibleForGivenTopRecordIndex(scrollPos))
                        break;

                    scrollPosToUse = scrollPos;
					scrollPos--;
				}

				if (originalScrollPos > scrollPosToUse)
				{
					// update the scroll offset
					if (this.LogicalOrientation == Orientation.Vertical)
						this.SetVerticalOffset(scrollPosToUse);
					else
						this.SetHorizontalOffset(scrollPosToUse);

					this.UpdateLayout();
				}
				else
				{
					// call WillLastRecordBeVisibleForGivenTopRecordIndex with the original
					// scroll osffset to undo the effects of the call above
					this.WillLastRecordBeVisibleForGivenTopRecordIndex(originalScrollPos);
				}

			}
			finally
			{
				this._queuedBottomRecordVerification = null;
				this._isProcessingBottomRecordVerification = false;
			}

			return null;
		}

				#endregion //ProcessQueuedBottomRecordVerification	

				#region VerifyScrollPositionOfLastVisibleRecord

		// JJD 5/23/07
		// Pass in the size to the VerifyScrollPositionOfLastVisibleRecord
		// because the ActualWidth and ActualHeight properties are not updated at this point
		//private void VerifyScrollPositionOfLastVisibleRecord()
		// JJD 6/6/07 
		// added bool return value so we know if the last record is fully in view
		//private void VerifyScrollPositionOfLastVisibleRecord(Size panelSize)
		private bool VerifyScrollPositionOfLastVisibleRecord(Size panelSize)
		{
			// JJD 5/31/07
			// the rootpanel check is not necessary since this method is only called on the root panel
			//GridViewPanel rootPanel = this.RootPanel;

			//if (rootPanel != null && rootPanel != this)
			//    rootPanel.VerifyScrollPositionOfLastVisibleRecord(new Size(rootPanel.ActualWidth, rootPanel.ActualHeight));

			IViewPanelInfo info = this.ViewPanelInfo;

            while (this._scrollPositionOfLastVisibleRecord > 0)
			{
                // JJD 1/27/09 
                // Use GetTopRecordFromScrollOffset metjod instead which adjusts for any
                // fixed rcds on top of the root panel
				//Record rcd = info.GetRecordAtOverallScrollPosition(this._scrollPositionOfLastVisibleRecord);

                // JJD 2/18/09 - TFS13623
                // Hold the position in a stack variable
				//Record rcd = this.GetTopRecordFromScrollOffset(this._scrollPositionOfLastVisibleRecord);
                int pos = this._scrollPositionOfLastVisibleRecord;
				Record rcd = this.GetTopRecordFromScrollOffset(pos);

				if (rcd != null)
				{
                    // JJD 2/18/09 - TFS13623
                    // If the rcd is a root record and its fixed on the bottom keep
                    // decrementing the position until we reach the bottommost rcd
                    // that is not a root record or is not fixed on the bottom
                    while (rcd != null &&
                            pos > 0 &&
                            rcd.ParentRecord == null && 
                            rcd.IsFixed == true && 
                            rcd.IsOnTopWhenFixed == false)
                    {
                        pos--;
				        rcd = this.GetTopRecordFromScrollOffset(pos);
                    }

                    // JJD 2/18/09 - TFS13623
                    // If we didn't find a rcd then return true
                    if (rcd == null)
                        return true;

					RecordPresenter rp = rcd.AssociatedRecordPresenter;

					// JJD 5/31/07
					// Make sure the rp is visible (i.e. not deactivated)
					//if (rp != null && rp.ShouldDisplayRecordContent)
					if (rp != null && rp.IsVisible && rp.ShouldDisplayRecordContent)
					{
						FrameworkElement contentArea = rp.GetRecordContentSite();

						Debug.Assert(contentArea != null);

						if (contentArea == null)
							contentArea = rp;

						Point ptRightBottom = contentArea.TranslatePoint(new Point(contentArea.ActualWidth, contentArea.ActualHeight), this);

						if (this.LogicalOrientation == Orientation.Vertical)
						{
							// JJD 5/23/07
							// Use the passed in size because the ActualHeight properti is not updated at this point
							//	if (this.ActualHeight == 0 || ptRightBottom.Y <= this.ActualHeight)
							// SSP 9/18/08 BR34595
							// Also make sure the item is not scrolled out view on top.
							// 
							//if ( panelSize.Height == 0 || ptRightBottom.Y <= panelSize.Height )

                            // JJD 11/24/08 - TFS6244/BR34703
                            // Make sure to check if values are close, i.e. not off by a tiny amount due to rounding errors
                            //if (panelSize.Height == 0 || ptRightBottom.Y <= panelSize.Height && ptRightBottom.Y >= 0)
                            if (panelSize.Height == 0 || ptRightBottom.Y >= 0 && (ptRightBottom.Y <= panelSize.Height ||
                                GridUtilities.AreClose(ptRightBottom.Y, panelSize.Height)))
                            {
                                // JJD 7/15/09 - TFS19156
                                // Make sure all of the parent records are in view also so the complete last
                                // record can be fully displayed including the chrome from its parent records
                                //return true;
                                return this.VerifyAllParentsAreInView(rp.Record, panelSize);
                            }
						}
						else
						{
							// JJD 5/23/07
							// Use the passed in size because the ActualWidth property is not updated at this point
							//if (this.ActualWidth == 0 || ptRightBottom.X <= this.ActualWidth)
							// SSP 9/18/08 BR34595
							// Also make sure the item is not scrolled out view on left.
							// 
							//if (panelSize.Width == 0 || ptRightBottom.X <= panelSize.Width)

                            // JJD 11/24/08 - TFS6244/BR34703
                            // Make sure to check if values are close, i.e. not off by a tiny amount due to rounding errors
							//if ( panelSize.Width == 0 || ptRightBottom.X <= panelSize.Width && ptRightBottom.X >= 0 )
                            if (panelSize.Width == 0 || ptRightBottom.X >= 0 && (ptRightBottom.X <= panelSize.Width ||
                                GridUtilities.AreClose(ptRightBottom.X, panelSize.Width)))
                            {
                                // JJD 7/15/09 - TFS19156
                                // Make sure all of the parent records are in view also so the complete last
                                // record can be fully displayed including the chrome from its parent records
                                //return true;
                                return this.VerifyAllParentsAreInView(rp.Record, panelSize);
                            }
						}
					}


                    // JJD 1/23/09 - NA 2009 vol 1 - record filtering - optimization
                    // If we didn't find a record presenter then start at the top
                    // add walk down to prevent the situation where we are
                    // way off base
                    if (rp == null)
                    {
                        int scrollPos = this._lastOverallScrollPosition;

                        Record tempRecord = this._lastRecordAtOverallScrollPosition;

                        // JJD 2/9/09
                        // Only use the _lastRecordAtOverallScrollPosition if we aren't
                        // using a scroll candidate
                        if (tempRecord != null && this._useScrollPositionCandidate == false)
                            scrollPos = this.GetScrollOffsetFromRecord(tempRecord);
                        else
                            tempRecord = this.GetTopRecordFromScrollOffset(scrollPos);

                        if (tempRecord == null)
                            return true;

                        int holdScrollPositionOfLastVisibleRecord = this._scrollPositionOfLastVisibleRecord;

                        // JJD 1/23/09 - NA 2009 vol 1 - record filtering - optimization
                        // keep walking down until we find a record that is not in view
                        while (tempRecord != null)
                        {
                            tempRecord = this.GetTopRecordFromScrollOffset(scrollPos);

                            if (tempRecord != null &&
                                tempRecord.AssociatedRecordPresenter != null)
                            {
                                this._scrollPositionOfLastVisibleRecord = scrollPos;

                                // JJD 1/29/09 - NA 2009 vol 1 - record filtering - optimization
                                // make sure we stop when we get to the last scrollable record
                                if (scrollPos >= info.OverallScrollCount)
                                    return true;

                                scrollPos++;
                            }
                            else
                            {
                                // JJD 1/23/09 - NA 2009 vol 1 - record filtering - optimization
                                // bump the scoll position an extra time to compensate
                                // for the fact that it will be decremented below
                                if (holdScrollPositionOfLastVisibleRecord > this._scrollPositionOfLastVisibleRecord)
                                    this._scrollPositionOfLastVisibleRecord++;
                                // JJD 2/9/09 - TFS13693
                                // else return false so we don't end up in a loop
                                else
                                    return false;

                                break;
                            }

                        }
                    }
				}

    		    this._scrollPositionOfLastVisibleRecord--;
			}

			return false;
		}

				#endregion //VerifyScrollPositionOfLastVisibleRecord

				#region VerifyScrollData



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		// JJD 6/12/07 - added availableScrollSize parameter
		private void VerifyScrollData(Size extent, Size viewport, Vector offset, Size availableScrollSize)
		{
			ScrollData scrolldata = this.ScrollingData;

			// AS 4/12/11 TFS62951
			scrolldata._availableScrollSize = availableScrollSize;

			// JJD 6/11/07
			// Check flag to see if offset has changed
			if (this._recordsInViewChanged == true	||
				scrolldata._hasOffsetChanged		||
				scrolldata._extent		!= extent	||
				scrolldata._viewport	!= viewport ||
				scrolldata._offset		!= offset	||
				// JJD 6/6/07 
				// added flag so we know if the last scrollable record is fully in view
				scrolldata._isLastRecordInView != this._isLastScrollableRecordFullyInView	)
			{
				// JJD 6/11/07
				// Reset flag 
				scrolldata._hasOffsetChanged = false;

				bool isVertical = this.LogicalOrientation == Orientation.Vertical;

				// JJD 6/6/07 
				// added flag so we know if the last scrollable record is fully in view
				scrolldata._isLastRecordInView = this._isLastScrollableRecordFullyInView;

				//Debug.WriteLine(viewport.ToString() + " of " + extent.ToString(), DateTime.Now.ToString("hh:mm:ss:ffffff"));
				
				// JJD 3/27/07
				// If we are autosizing then constrain the extent and the offset
                // JJD 1/21/09 - NA 2009 vol 1
                // Allow scrolling in autofit mode
                //if (this.DataPresenter.AutoFitResolved)
                //{
                //    if (isVertical)
                //    {
                //        extent.Width = Math.Min(extent.Width, viewport.Width);
                //        offset.X = 0;
                //    }
                //    else
                //    {
                //        extent.Height = Math.Min(extent.Height, viewport.Height);
                //        offset.Y = 0;
                //    }
                //}

				scrolldata._extent		= extent;
				scrolldata._viewport	= viewport;
				scrolldata._offset		= offset;

				// JJD 6/6/07
				// Added separate viewport and extenbt settings to control scrollbar
				scrolldata._viewportForScrollbar		= viewport;
				scrolldata._extentForScrollbar			= extent;

				double	recordOffset;
				double  recordViewPort;
				double  recordExtent;
				double  scrollHeightOfPanel;

                if (isVertical)
				{
					recordOffset			= offset.Y;
					recordViewPort			= viewport.Height;
					recordExtent			= extent.Height;
					scrollHeightOfPanel		= availableScrollSize.Height;
				}
				else
				{
					recordOffset			= offset.X;
					recordViewPort			= viewport.Width;
					recordExtent			= extent.Width;
					scrollHeightOfPanel		= availableScrollSize.Width;
				}

                // JJD 12/12/07 - BR29082
                // Make sure we don't divide by zero
                //double avgRecordHeightInViewPort = scrollHeightOfPanel / recordViewPort;
				double avgRecordHeightInViewPort = scrollHeightOfPanel / Math.Max(recordViewPort, 1);


				if (recordOffset == 0)
				{
					scrolldata._highestRecordDisplayed = recordViewPort;
					scrolldata._averageHeightOfRecords = avgRecordHeightInViewPort;
				}
				else
				{
					double newHighestRecord = recordOffset + recordViewPort - 1;

					if (newHighestRecord > scrolldata._highestRecordDisplayed)
					{
                        // JJD 12/12/07 - BR29082
                        // Make sure we don't divide by zero
                        //scrolldata._averageHeightOfRecords = ((scrolldata._averageHeightOfRecords * scrolldata._highestRecordDisplayed) + ((newHighestRecord - scrolldata._highestRecordDisplayed) * avgRecordHeightInViewPort))
                        //                                / newHighestRecord;
						scrolldata._averageHeightOfRecords = ((scrolldata._averageHeightOfRecords * scrolldata._highestRecordDisplayed) + ((newHighestRecord - scrolldata._highestRecordDisplayed) * avgRecordHeightInViewPort))
														/ Math.Max(newHighestRecord, 1);
														// (scrolldata._highestRecordDisplayed + Math.Min(recordViewPort, newHighestRecord - scrolldata._highestRecordDisplayed));
						scrolldata._highestRecordDisplayed = newHighestRecord;
					}
				}

				if (scrolldata._averageHeightOfRecords < 1)
					scrolldata._averageHeightOfRecords = 1;

                
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

				{
					double numberOfRecordsBasedOnAvgHeight = Math.Max((scrollHeightOfPanel / scrolldata._averageHeightOfRecords) - 1.0, 1d );

					numberOfRecordsBasedOnAvgHeight = Math.Min(numberOfRecordsBasedOnAvgHeight, Math.Max( recordExtent - (1 + recordOffset), 1d));

					if ( isVertical )
					{
						scrolldata._viewportForScrollbar.Height = numberOfRecordsBasedOnAvgHeight;
						// JM BR27217 12-05-07
						//scrolldata._extentForScrollbar.Height += Math.Min(2.0, numberOfRecordsBasedOnAvgHeight);
						if (numberOfRecordsBasedOnAvgHeight < recordExtent)
							scrolldata._extentForScrollbar.Height += Math.Min(Math.Min(2.0, recordExtent - numberOfRecordsBasedOnAvgHeight), numberOfRecordsBasedOnAvgHeight);

                        // JJD 11/10/09 - TFS24368
                        // If the last record is in view adjust the viewport so the thumb is at the bottom
                        if (this._isLastScrollableRecordFullyInView)
                            scrolldata._viewportForScrollbar.Height = Math.Max(scrolldata._extentForScrollbar.Height - scrolldata._offset.Y, 1);
                    }
					else
					{
						scrolldata._viewportForScrollbar.Width  = numberOfRecordsBasedOnAvgHeight;
						// JM BR27217 12-05-07
						//scrolldata._extentForScrollbar.Width += Math.Min(2.0, numberOfRecordsBasedOnAvgHeight);
						if (numberOfRecordsBasedOnAvgHeight < recordExtent)
							scrolldata._extentForScrollbar.Width += Math.Min(Math.Min(2.0, recordExtent - numberOfRecordsBasedOnAvgHeight), numberOfRecordsBasedOnAvgHeight);

                        // JJD 11/10/09 - TFS24368
                        // If the last record is in view adjust the viewport so the thumb is at the bottom
                        if (this._isLastScrollableRecordFullyInView)
                            scrolldata._viewportForScrollbar.Width = Math.Max(scrolldata._extentForScrollbar.Width - scrolldata._offset.X, 1);
                    }

				}

                // JJD 5/2/08 - BR32390
                // If this is the root panel in an infinite container then
                // we can not allow scrolling if the extent is less than the
                // extent in an inifnite container

                // JJD 5/27/08 - BR32292
                // Moved logic into CanRecordsBeScrolled property
                #region Old code

                //if (this._isRootPanelInInfiniteContainer)
                //{
                //    #region See if scrollbars should be prevented for records

                //    double extentDesired;
                //    double extentInInfinitContainer;

                //    if (isVertical)
                //    {
                //        extentInInfinitContainer = this.HeightInfiniteContainersResolved;
                //        extentDesired = this.DesiredSize.Height;
                //    }
                //    else
                //    {
                //        extentInInfinitContainer = this.WidthInfiniteContainersResolved;
                //        extentDesired = this.DesiredSize.Width;
                //    }

                //    bool preventScrollbarsForRcds = double.IsPositiveInfinity(extentInInfinitContainer) ||
                //                                extentDesired < extentInInfinitContainer;

                //    if (preventScrollbarsForRcds)

                #endregion //Old code
                if (!this.CanRecordsBeScrolled)
                {
                    if (isVertical)
                    {
                        scrolldata._offset.Y = 0;
                        scrolldata._extent.Height = 1;
                        scrolldata._extentForScrollbar.Height = 1;
                        scrolldata._viewport.Height = 2;
                        scrolldata._viewportForScrollbar.Height = 2;

                    }
                    else
                    {
                        scrolldata._offset.X = 0;
                        scrolldata._extent.Width = 1;
                        scrolldata._extentForScrollbar.Width = 1;
                        scrolldata._viewport.Width = 2;
                        scrolldata._viewportForScrollbar.Width = 2;
                    }
                }

                    //#endregion //See if scrollbars should be prevented for records
               // }

                // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
                this.VerifyFixedFieldInfo();

                this.OnScrollInfoChange();
			}
		}

				#endregion //VerifyScrollData

				#region WillLastRecordBeVisibleForGivenTopRecordIndex

		// JJD 6/13/07
		// Added to support new scrolling feature
		private bool WillLastRecordBeVisibleForGivenTopRecordIndex(int scrollPositionCandidateForFirstVisibleRecord)
		{
			try
			{
				this._useScrollPositionCandidate = true;
				this._isScrollPositionCandidateFullyInView = false;
				this._scrollPositionCandidate = scrollPositionCandidateForFirstVisibleRecord;

				this.DataPresenter.BumpRecordsInViewVersion();

				this.InvalidateMeasure();
				this.InvalidateArrange();

				this.UpdateLayout();

				return this._isScrollPositionCandidateFullyInView;
			}
			finally 
			{ 
				this._useScrollPositionCandidate = false; 
			}

		}

				#endregion //WillLastRecordBeVisibleForGivenTopRecordIndex	

            #endregion //Private Methods	
    
        #endregion //Methods	
    
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