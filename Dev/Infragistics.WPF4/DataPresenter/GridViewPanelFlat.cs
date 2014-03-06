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

namespace Infragistics.Windows.DataPresenter
{
    // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
    /// <summary>
    /// A <see cref="GridViewPanel"/> derived class used by the <see cref="DataPresenterBase"/> derived controls such as <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/> to arrange <see cref="RecordPresenter"/> instances in a tabular fashion, either horizontally or vertically.  The GridViewPanelFlat displays hierarchical data directly with a single instance.
    /// </summary>
    /// <remarks>
    /// <p class="note"><b>Note: </b>The GridViewPanelFlat is designed to be used with the <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/> controls and is for Infragistics internal use only.  The control is not
    /// designed to be used outside of the <see cref="XamDataGrid"/> or <see cref="XamDataPresenter"/>.  You may experience undesired results if you try to do so.</p>
    /// </remarks>
    /// <seealso cref="XamDataGrid"/>
    /// <seealso cref="XamDataPresenter"/>
    /// <seealso cref="GridViewPanel"/>
    /// <seealso cref="GridViewPanelNested"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.GridViewSettings.UseNestedPanels"/>
    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public sealed class GridViewPanelFlat : GridViewPanel
    {
        #region Private methods

        private int                                 _scrollPositionOfLastVisibleRecord;
        private bool                                _isInInfiniteContainer;
		private bool								_recordsInViewChanged;
		// JJD 4/21/11 - TFS73048 - Optimaztion - added
		private bool								_isInMeasure;
        
        private int                                 _bottomFixedElementsGenerated;
        private int                                 _headerElementsGenerated;
        private int                                 _scrollableElementsGenerated;
        private int                                 _scrollableElementsFullyInView;
        private int                                 _topFixedElementsGenerated;

		private bool								_lastOrientationWasVertical = true;
		private int					                _lastOverallScrollPosition;
        private int                                 _lastMeasureScrollPosition;
        private Record                              _lastRecordAtOverallScrollPosition;

		private double								_extentUsedByFixedRecordsOnTop;
		private double								_extentUsedByFixedRecordsOnBottom;
		private double								_extentUsedByScrollableRecords;

		private double								_widthInInfiniteContainer;
		private double								_heightInInfiniteContainer;

		private int									_offsetWithinListForTopFixedRecords;
		private int									_offsetWithinListForBottomFixedRecords;
		private int									_offsetWithinListForScrollableRecords;
        private double                              _averageExtentPerRecord;
        private int                                 _elementsUsedInAverageCalc;

        private bool                                _isLastScrollableRecordFullyInView;
        private bool                                _isVertical = true;

		// JJD 2/14/11 - TFS66166 - Optimization
		// Keep track of the desired height of elements
		private Dictionary<UIElement, double>		_desiredHeightMap = new Dictionary<UIElement,double>();

        private List<UIElement>                     _generatedElements;

        #endregion //Private methods	
    
        #region Base class overrides

            #region Properties

                #region EffectiveScrollPosition






        internal override int EffectiveScrollPosition
        {
            get
            {
                return this.ViewPanelInfo.OverallScrollPosition;
            }
            set
            {
                this.ViewPanelInfo.OverallScrollPosition = value;
            }
        }

                #endregion EffectiveScrollPosition

                #region FirstScrollableRecord

        internal override Record FirstScrollableRecord
        {
            get
            {
                FlatScrollRecordsCollection records = this.Records;

                // Check to make sure that there is a ViewableRecordCollection
                if (records == null)
                    return null;

                int totalRecordCount = records.CountOfNonHeaderRecords;

                if (totalRecordCount == 0)
                    return null;

                int topFixedCount = records.CountOfFixedRecordsOnTop;
                int bottomFixedCount = records.CountOfFixedRecordsOnBottom;

                if (this._offsetWithinListForScrollableRecords >= totalRecordCount - (topFixedCount + bottomFixedCount))
                {
                    if (this._offsetWithinListForTopFixedRecords >= topFixedCount)
                    {
                        return records[this._offsetWithinListForTopFixedRecords];
                    }
                    else
                    {
                        if (this._offsetWithinListForBottomFixedRecords < bottomFixedCount)
                            return records[this._offsetWithinListForBottomFixedRecords + totalRecordCount - topFixedCount];
                        else
                            return null;
                    }
                }
                else
                {
                    return records.GetRecordAtScrollPosition(this.EffectiveScrollPosition);
                }
            }
        }

                #endregion //FirstScrollableRecord	

                #region IsRootPanel

        /// <summary>
        /// Always returns true 
        /// </summary>
        public override bool IsRootPanel
        {
            get
            {
                return true;
            }
        }

                #endregion //IsRootPanel

                #region IsRootPanelInInfiniteContainer

        internal override bool IsRootPanelInInfiniteContainer 
        {
            get { return this._isInInfiniteContainer; }
        }

                #endregion //IsRootPanelInInfiniteContainer	

                #region RootPanel

        // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
        // Made public abstract
        /// <summary>
        /// Always returns itself
        /// </summary>
        public override GridViewPanel RootPanel
        {
            get
            {
                return this;
            }
        }

                #endregion RootPanel

                #region RootPanelTopFixedOffset

        internal override int RootPanelTopFixedOffset
        {
            get
            {
                FlatScrollRecordsCollection records = this.Records;

                if ( records != null )
                    return records.CountOfFixedRecordsOnTop;

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
				
				#region TotalVisibleGeneratedItems

		/// <summary>
		/// Derived classes must return the number of visible generated items.
		/// </summary>
		protected override int TotalVisibleGeneratedItems
		{
			get
			{
                FlatScrollRecordsCollection records = this.Records;

                 // Check to make sure that there is a ViewableRecordCollection
                if (records == null)
                    return 0;

                return Math.Min(this.TotalItems - 1, (this._scrollableElementsGenerated + records.CountOfFixedRecordsOnTop + records.CountOfFixedRecordsOnBottom - (this._offsetWithinListForTopFixedRecords + this._offsetWithinListForBottomFixedRecords)));
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
            
            //Debug.WriteLine("GVP arrange: " + finalSize.ToString());

            base.ArrangeOverride(finalSize);

            GridViewSettings viewSettings = this.ViewSettings;

            if (viewSettings == null)
                return finalSize;

            bool orientationIsVertical = viewSettings.Orientation == Orientation.Vertical;

			// AS 4/12/11 TFS62951
			// Previously we were ensuring that the offset was within the scrollable range but we did that 
			// in the measure before we knew for sure what the viewport extent would be - e.g. the scrollviewer 
			// may measure the children with the horizontal scrollbar collapsed. Now that the measures are done 
			// we can verify the scroll info here.
			//
			var scrollData = this.ScrollingData;
			var offset = scrollData._offset;

			if (orientationIsVertical)
			    offset.X = Math.Max(Math.Min(offset.X, (double)(scrollData._extent.Width - scrollData._viewport.Width)), 0d);
			else
			    offset.Y = Math.Max(Math.Min(offset.Y, (double)(scrollData._extent.Height - scrollData._viewport.Height)), 0d);

			if (offset != scrollData._offset)
				this.VerifyScrollData(scrollData._extent, scrollData._viewport, offset, scrollData._availableScrollSize);

            Rect arrangeRect = new Rect(finalSize);
            double extentUsedByLastItem = 0;

            // We need to know if any element was arranged such that it wasn't fully in view.
            //
            Rect rectUsed = new Rect();

            // We need to know the topmost point of the bottom fixed records
            // since that area will not be available to the scrollable records
            // and needs to be excluded when determining if the last record
            // is fully in view.
            //
            Rect rectBottomFixed = Rect.Empty;

            double recordIndent = 0;

            #endregion //Setup


            #region Get the record counts

            FlatScrollRecordsCollection records = this.Records;

            if (records == null)
                return finalSize;

            int totalNonHeaderElementsGenerated = this._topFixedElementsGenerated + this._bottomFixedElementsGenerated + this._scrollableElementsGenerated;
            int totalElementsGenerated = totalNonHeaderElementsGenerated + this._headerElementsGenerated;
            int firstBottomFixedRecordIndex = records.CountOfNonHeaderRecords - records.CountOfFixedRecordsOnBottom;

            if ( totalElementsGenerated == 0 )
                return finalSize;

            Debug.Assert(totalElementsGenerated == this._generatedElements.Count, "Element counts don't match in GridViewPanelFlat.ArrangeOverride");

            #endregion //Get the record counts


            double overallExtent;

            // Compute our arrange rect.
            if (orientationIsVertical)
            {
                arrangeRect.X = -1 * this.ScrollingData._offset.X;
                overallExtent = finalSize.Height;
            }
            else
            {
                arrangeRect.Y = -1 * this.ScrollingData._offset.Y;
                overallExtent = finalSize.Width;
            }

            // Arrange each child taking into account our orientation.

            IList children = this.ChildElements;
            int activeChildrenCount = children.Count;

            #region Create a header record map and a map for all elements generated

            Dictionary<Record, UIElement> headerRecordMap = new Dictionary<Record, UIElement>();
            Dictionary<UIElement, RecordPresenter> generatedElementMap = new Dictionary<UIElement,RecordPresenter>();
     
            int count = this._generatedElements.Count;

            for (int i = 0; i < count; i++)
            {
                UIElement element = this._generatedElements[i];

                RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(element);

                Debug.Assert(rp != null, "invalid header record container");

                // JJD 3/17/10 - TFS28705 - Optimization 
                
                
                generatedElementMap[element] = rp;

                if (rp == null)
                    continue;

                HeaderRecord hr = rp.Record as HeaderRecord;

                if (hr == null)
                    continue;

                Record attachedToRcd = hr.AttachedToRecord;

                if (attachedToRcd == null)
                    continue;

				// MD 7/7/10
				// Found while fixing TFS35508
				// We should use the cached record above.
				//headerRecordMap[hr.AttachedToRecord] = element;
				headerRecordMap[attachedToRcd] = element;
            }

            #endregion //Create a header record map and a map for all elements generated


            #region Arrange our active children

            Stack<UIElement> bottomFixedRecords = null;

            Record topRecord = null;

            
            
            
            int scrollableElementsRemaining = Math.Max(0, count - (this._topFixedElementsGenerated + this._bottomFixedElementsGenerated + this._headerElementsGenerated));

            int totalItemsArranged = 0;

            for (int i = 0; i < count; i++)
            {
                UIElement element = this._generatedElements[i] as UIElement;

                double clipExtent = double.PositiveInfinity;

                if (i == 0 &&
                    totalNonHeaderElementsGenerated == 0 &&
                    this._headerElementsGenerated > 0)
                {
                    // Handle the case where we have only a header record
                    extentUsedByLastItem = this.ArrangeHelper(element, null, orientationIsVertical, false, extentUsedByLastItem, double.PositiveInfinity,  finalSize, ref arrangeRect, ref recordIndent);
                    totalItemsArranged++;
                }
                else
                {
                    #region Process Top Fixed records

                    if (this._topFixedElementsGenerated > 0 &&
                         i >= 0 &&
                         i < this._topFixedElementsGenerated)
                    {
                        double start;

                        if (i == 0)
                            start = 0;
                        else
                            if (orientationIsVertical)
                                start = arrangeRect.Bottom;
                            else
                                start = arrangeRect.Right;

                        clipExtent = overallExtent - (start + this._extentUsedByFixedRecordsOnBottom);

                        extentUsedByLastItem = this.ArrangeHelper(element, headerRecordMap, orientationIsVertical, false, extentUsedByLastItem, clipExtent,  finalSize, ref arrangeRect, ref recordIndent);
                        totalItemsArranged++;
                    }

                    #endregion //Process Top Fixed records

                    else

                        #region Process scrollable records

                        if (scrollableElementsRemaining > 0)
                        {
                            scrollableElementsRemaining--;

                            if (topRecord == null)
                            {
                                RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(element);

                                if (rp != null)
                                    topRecord = rp.Record;
                            }

                            if (scrollableElementsRemaining == 0 && i > 0)
                            {
                                clipExtent = overallExtent - this._extentUsedByFixedRecordsOnBottom;
                                if (orientationIsVertical)
                                    clipExtent -= arrangeRect.Bottom;
                                else
                                    clipExtent -= arrangeRect.Right;
                            }

                            extentUsedByLastItem = this.ArrangeHelper(element, headerRecordMap, orientationIsVertical, false, extentUsedByLastItem, clipExtent,  finalSize, ref arrangeRect, ref recordIndent);
                            totalItemsArranged++;
                        }

                        #endregion //Process scrollable records

                        else

                            #region Push Bottom fixed records onto a stack for processing below

                            if (i < totalNonHeaderElementsGenerated)
                            {

                                // if we haven't arranged any elements yet and w egot here that means
                                // that all records are fixed to the bottom. In this case we want ro arrange
                                // the header at the top of the panel instead of attaching it to the
                                // first bottom fixed record
                                if (totalItemsArranged == 0 &&
                                    headerRecordMap.Count > 0)
                                {
                                    // get the rp for this first bottom fixed record
                                    RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(element);

                                    if (rp != null)
                                    {
                                        // get the associated header from the header map
                                        UIElement headerElement;
                                        if (headerRecordMap.TryGetValue(rp.Record, out headerElement))
                                        {
                                            headerRecordMap.Remove(rp.Record);
                                            extentUsedByLastItem = this.ArrangeHelper(headerElement, null, orientationIsVertical, false, extentUsedByLastItem, double.PositiveInfinity,  finalSize, ref arrangeRect, ref recordIndent);
                                            totalItemsArranged++;

                                        }
                                    }
                                }

                                // fixed bottom records we want to layout in the reverse order so just
                                // push them onto to stack and continue
                                if (bottomFixedRecords == null)
                                    bottomFixedRecords = new Stack<UIElement>();

                                bottomFixedRecords.Push(element);
                            }

                            #endregion //Push Bottom fixed records onto a stack for processing below

                }
                rectUsed.Union(arrangeRect);
            }

            #endregion //Arrange our active children

			// MD 12/8/10 - TFS36648
			// Any headers still in the header record map were not positioned and should therefore be positioned 
			// out of view so they are no longer seen.
			foreach (UIElement element in headerRecordMap.Values)
			{
				Rect childArrangeRect = new Rect(new Point(-10000, -10000), element.DesiredSize);
				element.Arrange(childArrangeRect);
			}

            #region Hide unused elements
  
			// JJD 04/17/12 - TFS102905
			// See if RecordContainerGenerationMode is set to 'PreLoad'
			DataPresenterBase dp = this.DataPresenter;
			bool isPreload = dp != null && dp.RecordContainerGenerationMode == ItemContainerGenerationMode.PreLoad;
  
            for (int currentVisualChildIndex = 0; currentVisualChildIndex < activeChildrenCount; currentVisualChildIndex++)
            {
                UIElement visualChildElement = children[currentVisualChildIndex] as UIElement;

                // if the element is  not in the map then hide it since it isn't being used now
                if ( !generatedElementMap.ContainsKey(visualChildElement ) )
                {
                    #region Hide unused child elements

                    RecordPresenter childRecordPresenter = RecordListControl.GetRecordPresenterFromContainer(visualChildElement);
                    if (null != childRecordPresenter)
                    {
						// JJD 04/17/12 - TFS102905
						// If RecordContainerGenerationMode is set to 'PreLoad' and this record presenter 
						// hasn't been loaded yet then we need to measure and arrange it so all of the
						// elements is in its visual tree get hydrated properly
						// 
						if (isPreload && false == childRecordPresenter.IsLoaded)
						{
							// measure the childRecordPresenter with infinity
							childRecordPresenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

							// based on the orientation arrange the rp out of view
							Point location = orientationIsVertical ? new Point(0, -10000) : new Point(-10000, 0);
							childRecordPresenter.Arrange(new Rect(location, childRecordPresenter.DesiredSize));
						}
						else
						{
							bool wasHidingRecord = this.IsHidingRecord;
							this.IsHidingRecord = true;

							try
							{
								// Mark the rp collapsed and don't bother arranging it
								childRecordPresenter.IsArrangedInView = false;
								childRecordPresenter.TreatAsCollapsed = true;
								childRecordPresenter.InternalClip = null;

							}
							finally
							{
								this.IsHidingRecord = wasHidingRecord;
							}
						}
                    }
                    else
                    {
                        Size desiredSize = visualChildElement.DesiredSize;

                        // arrange other child elements out of view
                        visualChildElement.Arrange(new Rect(new Point(-10000 * finalSize.Width, -10000 * finalSize.Height), desiredSize));
                    }

                    #endregion //Hide unused child elements
                }

                rectUsed.Union(arrangeRect);
            }

   	        #endregion //Hide unused elements	

    
            #region Process bottom fixed records

            if (bottomFixedRecords != null)
            {
                Point pt;

                // create a rect that is offset to the bottom or right but is the size of the available space 
                if (orientationIsVertical)
                    pt = new Point(0, finalSize.Height);
                else
                    pt = new Point(finalSize.Width, 0);


                if (orientationIsVertical)
                {
                    pt.X = -1 * this.ScrollingData._offset.X;
                }
                else
                {
                    pt.Y = -1 * this.ScrollingData._offset.Y;
                }

                
                // reset record indent
                recordIndent = 0;

                Rect arrangeRectForBottomFixed = new Rect(pt, finalSize);

                extentUsedByLastItem = 0;
                while (bottomFixedRecords.Count > 0)
                {
                    UIElement itemElem = bottomFixedRecords.Pop();
                    extentUsedByLastItem = this.ArrangeHelper(itemElem, headerRecordMap, orientationIsVertical, true, extentUsedByLastItem, double.PositiveInfinity,  finalSize, ref arrangeRectForBottomFixed, ref recordIndent);
                    totalItemsArranged++;

                    if (rectBottomFixed.IsEmpty)
                        rectBottomFixed = arrangeRectForBottomFixed;
                    else
                        rectBottomFixed.Union(arrangeRectForBottomFixed);

                    rectUsed.Union(arrangeRectForBottomFixed);
                }
            }

            #endregion //Process bottom fixed records


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

			this.OnArrangeComplete();

            return finalSize;
		}

				#endregion //ArrangeOverride

                #region EnsureRecordIsVisible

        internal override bool EnsureRecordIsVisible(Record record)
        {
            if (record == null)
                throw new ArgumentNullException("record");

            // If records can't be scrolled then return false
            if (!this.CanRecordsBeScrolled)
                return false;

            GridViewSettings viewSettings = this.ViewSettings;

            if (viewSettings == null)
                return false;

            if (record.VisibilityResolved == Visibility.Collapsed)
                return false;


            #region Process root fixed records
            
            ViewableRecordCollection vrc = record.ParentCollection.ViewableRecords;

            
            // We need to access the Count property of the record's
            // associated ViewableRecordCollection to trigger the verification
            // of any pending fixed or special record processing that needs to be done
            if (vrc != null)
            {
                int count = vrc.Count;
            }

            FlatScrollRecordsCollection records = this.Records;

            // if we have any pending notifications call
            // UpdataLayout so we arrange everything into view based
            // on the current state
            if (records.HasPendingNotifications)
            {
                this.InvalidateMeasure();
                this.UpdateLayout();
            }
            
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

            #endregion //Process roo fixed records	

            IViewPanelInfo info = this.ViewPanelInfo;

            if (info == null)
                return false;

            // Make sure the record's ancestors are expanded
            if (record.ParentRecord != null && record.ParentRecord.IsExpanded == false)
            {
                // check with the dp if it is ok to scroll
                if (!info.IsOkToScroll())
                    return false;

                record.ParentRecord.IsExpanded = true;
                this.UpdateLayout();	
            }

            // if the record is already in view then return true
            if (this.IsRecordInView(record, false))
                return true;

            if (!info.IsOkToScroll())
                return false;

            int scrollPosOfTargetRecord = records.GetScrollPositionOfRecord(record);

            if (scrollPosOfTargetRecord < 0)
                return true;
    
            int firstRecordScrollPos = this.EffectiveScrollPosition;

            // if the record has a lesser scroll pos then we need to scroll up
            // so just set the scroll pos to the record's scroll pos and return
            if (scrollPosOfTargetRecord <= firstRecordScrollPos)
            {
                this.SetScrollPositionToRecord(record);
                this.InvalidateMeasure();
                this.UpdateLayout();
                return true;
            }

            // caclulate a scroll offset thst would try to place the target
            // record at the bottonm of the panel since we eed to scroll down.
            // Use the _scrollableElementsFullyInView to back up an appropriate amount
            int tempScrollPos = Math.Max(scrollPosOfTargetRecord - this._scrollableElementsFullyInView, firstRecordScrollPos + 1);
            int lastEffectiveScrollPos = -1;

            while (tempScrollPos > firstRecordScrollPos)
            {
                this.SetScrollPosition(tempScrollPos);
                this.InvalidateMeasure();
                this.UpdateLayout();

                // if the re cord is in view then return
                if ( this.IsRecordInView(record, false) )
                    return true;
                
                // re-get scroll pos of target record because it could change
                // based on hydrating of records
                scrollPosOfTargetRecord = records.GetScrollPositionOfRecord(record);

                // keep bumping the scroll positon by one until
                // the record is in view or its scroll position has been 
                // reached.
                int effectiveScrollPos = this.EffectiveScrollPosition;

                if (effectiveScrollPos < scrollPosOfTargetRecord &&
                    lastEffectiveScrollPos != effectiveScrollPos)
                {
                    lastEffectiveScrollPos = effectiveScrollPos;
                    tempScrollPos++;
                }
                else
                    break;
            }

            return false;
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

            Record rcd = rlc.Items[itemIndex] as Record;

            if (rcd == null)
                return true;

            if (rcd.IsFixed || rcd.IsSpecialRecord)
                return false;

            if (this.IsRecordInView(rcd, true))
                return false;

            return true;
        }

                #endregion //GetCanCleanupItem	
                
                #region GetScrollOffsetFromRecord

        internal override int GetScrollOffsetFromRecord(Record record)
		{
            FlatScrollRecordsCollection records = this.Records;

            if (records == null)
                return base.GetScrollOffsetFromRecord(record);

			return records.GetScrollPositionOfRecord(record);
		}

				#endregion //GetScrollOffsetFromRecord
    
				#region GetTopRecordFromScrollOffset

		internal override Record GetTopRecordFromScrollOffset(int scrollOffset)
		{
            FlatScrollRecordsCollection records = this.Records;

            if (records == null)
                return base.GetTopRecordFromScrollOffset(scrollOffset);

			return records.GetRecordAtScrollPosition(scrollOffset);
		}

				#endregion //GetTopRecordFromScrollOffset

				#region MeasureOverride

		/// <summary>
		/// Called to give an element the opportunity to return its desired size and measure its children.
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected override System.Windows.Size MeasureOverride(Size availableSize)
		{
            //Debug.WriteLine("GVP measure, available size: " + availableSize.ToString());

            GridViewSettings viewSettings = this.ViewSettings;

			if (viewSettings == null)
				return new Size(1, 1);

            Size    originalAvailableSize   = availableSize;
			bool	isVertical	= (viewSettings.Orientation == Orientation.Vertical);

            // If the orientation changed then release all existing elements 
            if (this._isVertical != isVertical)
            {
                this._isVertical = isVertical;
                this._averageExtentPerRecord = 0;
                this._elementsUsedInAverageCalc = 0;
                this.GetGenerator().RemoveAll();
                this.RemoveAllDeactivatedContainers();
            }

            // Initialize flag so we know whether to show scrollbars
            this._isInInfiniteContainer = false;

            if (double.IsInfinity(availableSize.Width))
            {
                this._widthInInfiniteContainer = this.WidthInfiniteContainersResolved;

                // JJD 5/2/08 - BR32390
                // Added flag so we know whether to show scrollbars
                if (!isVertical)
                    this._isInInfiniteContainer = true;
            }
            else
                this._widthInInfiniteContainer = 0;

            if (double.IsInfinity(availableSize.Height))
            {
                this._heightInInfiniteContainer = this.HeightInfiniteContainersResolved;

                // JJD 5/2/08 - BR32390
                // Added flag so we know whether to show scrollbars
                if (isVertical)
                    this._isInInfiniteContainer = true;
            }
            else
                this._heightInInfiniteContainer = 0;

			// Adjust the width and/or the height of the passed-in constraint to the corresponding screen size dimension
			// if the width and/or height are equal to infinity.  This is to avoid creating and measuring elements for every
			// record in the underlying data.
			double	constraintWidth			= double.IsInfinity(availableSize.Width) ? this.WidthInfiniteContainersResolved : availableSize.Width;
			double	constraintHeight		= double.IsInfinity(availableSize.Height) ? this.HeightInfiniteContainersResolved : availableSize.Height;
            
            Size effectiveConstraint = new Size(constraintWidth, constraintHeight);
            Size adjustedConstraint = effectiveConstraint;

			IViewPanelInfo info				= this.ViewPanelInfo;
			double	availableExtent;

			FlatScrollRecordsCollection records = this.Records;

			if (records == null)
				return new Size(1, 1);

			// JM 11-25-08 TFS10814 - Access the ViewableRecords.Count property to workaround a timing issue that results in the totalItemsThisPanel variable (the next line)
			//						  being set to a number that is not equal to this.RecordListControl.Items.Count.  We discovered this in a grid that has the AddRow turned on
			//						  while bound to an empty DataSource - in this case the AddRow was being added to the ViewableRecords collection but it was not showing up
			//						  in the grid because totalItemsThisPanel was = zero and further down in this routine we check for totalItemsThisPanel > 0 before generating
			//						  a presenter for the add record.  It appears that there is some kind of timing issue when accessing ItemCollection.Count property since it
			//						  delegates to our ViewableRecords.Count property (the RecordListControl is bound to the ViewableRecordsCollection) which can send out Reset
			//						  notifications which they are evidently not expecting.  By access the ViewableRecords.Count proeprty here first, the timing issues are avoided.
            int iUnused = records.Count;

			int		totalItemsThisPanel		= ((this.RecordListControl != null) && this.RecordListControl.HasItems) ? this.RecordListControl.Items.Count : 0;

			if (totalItemsThisPanel > 0)
			{
				// JJD 4/17/07
				// Get the first item in the list. This will trigger a lazy creation
				// of at least one record. If the Visibility of the Records is set
				// to 'collapsed' in the InitializeRecord event for all records in the list then firstRecordInList will return null
				// and subsequently the ourRecordCollection.Count will return null
				Record firstRecordInList = records[0];

				// reget the count after forcing the creation of the first record above
				totalItemsThisPanel = records.Count;
			}

            if (isVertical)
                availableExtent = constraintHeight;
            else
                availableExtent = constraintWidth;

			Size ourDesiredSize = new Size();
			bool canTopFixedRecordsFit = true;
			bool canAllFixedRecordsFit = true;

            // cache the old info to determine is something changed
            int lastOffsetWithinListForTopFixedRecords = this._offsetWithinListForTopFixedRecords;
            int lastOffsetWithinListForBottomFixedRecords = this._offsetWithinListForBottomFixedRecords;
            int lastOffsetWithinListForScrollableRecords = this._offsetWithinListForScrollableRecords;
            int lastNumberOfScrollableRecordsToLayout = this._scrollableElementsGenerated;
			
			int scrollCount                 = 0;
			int topFixedCount               = 0;
			int bottomFixedCount            = 0;
            int scrollableRecordCount       = 0;
			int overallScrollPosition       = 0;

            ScrollDirection scrollDirection = ScrollDirection.Increment;

            bool generationSucceeded        = false;
            bool firstGenPassSucceeded      = false;
            Record lastScrollableRecordInView = null;
            
            double extentRemaining          = 0;
            int scrollPosition = 0;
            bool wasOverAdjusted            = false;
 
            GenerationCache genCache        = null;
		
			// JJD 4/21/11 - TFS73048 - Optimization - added
			_isInMeasure = true;

			// JJD 6/28/11 - TFS79556
			// Cache the scrollbar height
			double scrollbarHeight = SystemParameters.HorizontalScrollBarHeight;

            while (generationSucceeded == false )
            {
                // Until we have succeeded with a generation we need to 
                // initialize our counts and 
                if (firstGenPassSucceeded == false)
                {
                    #region Initialize counts and scroll position

                    scrollCount = records.CountOfNonHeaderRecords;
                    topFixedCount = records.CountOfFixedRecordsOnTop;
                    bottomFixedCount = records.CountOfFixedRecordsOnBottom;
                    scrollableRecordCount = scrollCount - (bottomFixedCount + topFixedCount);

                    Debug.Assert(scrollableRecordCount >= 0);

                    // 5/21/07 - Optimization
                    // Call BeginGeneration so we can support recycling 
                    overallScrollPosition = this.EffectiveScrollPosition;

                    // JJD 1/23/09 - NA 2009 vol 1 - record filtering
                    // Keep track of the actual top record since we process the
                    // filtering logic lazily as records get hydrated it is possible
                    // that the scroll position of the top record will change without the
                    // user performing a scroll operation. In which case we want to
                    // retain the current top record
                    if (totalItemsThisPanel > 0)
                    {
                        // get the record at the scroll position
                        Record recordAtScrollPosition = this.GetTopRecordFromScrollOffset(overallScrollPosition);

                        // see if the overall scroll position is the same
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

                                // if the scroll position is different (which they should be at this point)
                                // then use the new scroll position and update the info's overall scroll position
								// JM 03-11-10 TFS27640
                                //if (newScrollPosition != this._lastOverallScrollPosition)
								if (newScrollPosition != -1 && newScrollPosition != this._lastOverallScrollPosition)
								{
                                    recordAtScrollPosition = this._lastRecordAtOverallScrollPosition;
                                    overallScrollPosition = newScrollPosition;
                                    info.OverallScrollPosition = overallScrollPosition;
                                }
                            }
                        }

                        // cache the record for the next time
                        this._lastRecordAtOverallScrollPosition = recordAtScrollPosition;
                    }
                    
                    scrollDirection = overallScrollPosition < this._lastMeasureScrollPosition ? ScrollDirection.Decrement : ScrollDirection.Increment;
                     
                    this._lastMeasureScrollPosition = overallScrollPosition;
            
                    scrollPosition          = overallScrollPosition;

                    #endregion //Initialize counts and scroll position
                }

                // create a GenerationCache to perform the record element generation
                using ( genCache = new GenerationCache(this, isVertical, effectiveConstraint, scrollPosition, scrollDirection))
                {
                    // Call the cache's GenerateAllRecords method
                    generationSucceeded                 = genCache.GenerateAllRecords();

                    extentRemaining                     = genCache._extentRemaining;
                    scrollPosition                      = genCache._currentScrollPosition;
                    this._averageExtentPerRecord        = genCache._averageExtentPerRecord;
                    this._elementsUsedInAverageCalc     = genCache._elementsUsedInAverageCalc;
                }

                if (generationSucceeded == false)
                {
                    wasOverAdjusted                         = false;
                    firstGenPassSucceeded                   = false;
                    this._isLastScrollableRecordFullyInView = false;
                    continue;
                }
                else
                {
                    if (firstGenPassSucceeded == false)
                    {
                        #region First successful pass logic

                        firstGenPassSucceeded = true;

                        this._isLastScrollableRecordFullyInView = false;

                        
						
						
						
						
						
						
						
						
						//if (extentRemaining > scrollbarHeight )
						// JJD 10/17/11 - TFS88682
						// If there is any extent remaining then the last record is in view
						
						// JJD 11/23/11 - TFS31939
						// Moved up from below
						// Always set lastScrollableRecordInView from the gen cache info
						lastScrollableRecordInView = genCache._highestScrollableRecordFullyInView;
						
						if (extentRemaining > 0)
                        {
							// JJD 11/23/11 - TFS31939
							// Moved above
                            //lastScrollableRecordInView  = genCache._highestScrollableRecordFullyInView;

                            
                            // if we have a scrollable record in view then set the 
                            // _isLastScrollableRecordFullyInView property to true
                            if ( lastScrollableRecordInView != null )
                                this._isLastScrollableRecordFullyInView = true;

                            // if the scroll position is > 0 then scroll up by an estimated number of 
                            // rcds based on the _averageExtentPerRecord
                            if (scrollPosition > 0)
                            {
                                generationSucceeded = false;
								// JJD 6/28/11 - TFS79556
								// Only adjust the scroll position by half the calculated value. This handles
								// situations where there are special rcds (e.g. headers and filter rcds) atta ched
								// to some data records which would skew our calculated value too much.
								// Note: any addtional adjustments below will also half the remaining amount
								// to minimize the tottal number of passes we have to make
								//scrollPosition = Math.Max(scrollPosition - (int)Math.Max(extentRemaining / Math.Max(15, this._averageExtentPerRecord), 1), 0);
								scrollPosition = Math.Max(scrollPosition - ((int)Math.Max(extentRemaining / (2 * ( Math.Max(15, this._averageExtentPerRecord))), 1)), 0);
                                scrollDirection = ScrollDirection.Decrement;
                                continue;
                            }
                        }

                        #endregion //First successful pass logic
                    }
                    else
                    {
                        
                        // If the lastScrollableRecordInView doesn't match
                        // this pass's _highestScrollableRecordFullyInView then
                        // we have over adjusted negatively and need to re-adjust
                        // the scroll position up by one
                        if (lastScrollableRecordInView != null &&
							// JJD 07/06/12 - TFS116559
							// Also make sure the scroll position can be adjusted and that
							// there is at least one scrollable rcd fully in view before
							// adjusting based on the last rcd not being fully in view.
							// This prevents a possible infinite loop in certain scenarios.
                            //lastScrollableRecordInView != genCache._highestScrollableRecordFullyInView)
                            lastScrollableRecordInView != genCache._highestScrollableRecordFullyInView &&
							scrollPosition < records.CountOfNonHeaderRecords - 1 &&
							genCache._scrollableElementsFullyInView > 0)
                        {
							wasOverAdjusted = true;
							generationSucceeded = false;
							scrollPosition = Math.Min(records.CountOfNonHeaderRecords - 1, scrollPosition + 1);
							scrollDirection = ScrollDirection.Increment;
							continue;
                       }
                        else
                        if ( !GridUtilities.AreClose(0.0, extentRemaining ) )
                        {
                            if (extentRemaining > 0)
                            {
								// JJD 6/28/11 - TFS79556
								// Only adjust the scroll position if the extent remaining is > scrollbar height
								// This handles the situation where we get multiple measures from the 
								// ScrollConentPresenter parent
								if (extentRemaining > scrollbarHeight)
								{
									// if we haven't over adjusted the scroll position yet
									// then decrement it by one
									if (wasOverAdjusted == false && scrollPosition > 0)
									{
										generationSucceeded = false;
										// JJD 6/28/11 - TFS79556
										// Only adjust the scroll position by half the calculated value so we
										// minimize the tottal number of passes we have to make
										//scrollPosition = Math.Max(0, scrollPosition - 1);
										scrollPosition = Math.Max(scrollPosition - ((int)Math.Max(extentRemaining / (2 * (Math.Max(15, this._averageExtentPerRecord))), 1)), 0);
										scrollDirection = ScrollDirection.Decrement;
										continue;
									}
								}
                            }
                            else
                            {
                                // since the extentRemaining is negative we have over
                                // adjusted the scroll position and need to  
                                // increment the scroll position up by one and try again
                                if (scrollPosition < records.CountOfNonHeaderRecords - 1)
                                {
                                    wasOverAdjusted = true;
                                    generationSucceeded = false;
                                    scrollPosition = Math.Min(records.CountOfNonHeaderRecords - 1, scrollPosition + 1);
                                    scrollDirection = ScrollDirection.Increment;
                                    continue;
                                }
                            }
                        }
                    }
                }

				// JJD 6/28/11 - TFS79556
				// If the adjusted scroll position is more that 1 less than the EffectiveScrollPosition
				// then set the this.EffectiveScrollPosition to than value. This should limit the
				// number of adjustments required on future measures
				if (scrollPosition < this.EffectiveScrollPosition - 1)
					this.EffectiveScrollPosition = scrollPosition + 1;

                this._bottomFixedElementsGenerated          = genCache._bottomFixedElementsGenerated;
                this._headerElementsGenerated               = genCache._headerElementsGenerated;
                this._topFixedElementsGenerated             = genCache._topFixedElementsGenerated;
                this._scrollableElementsGenerated           = genCache._scrollableElementsGenerated;
                this._scrollableElementsFullyInView         = genCache._scrollableElementsFullyInView;
                this._extentUsedByScrollableRecords         = genCache._extentScrollable;
                this._extentUsedByFixedRecordsOnBottom      = genCache._extentBottomFixed;
                this._extentUsedByFixedRecordsOnTop         = genCache._extentTopFixed;
                this._generatedElements                     = genCache._generatedElements;

                if ( this._generatedElements != null )
                    this._generatedElements.TrimExcess();

                if (genCache._highestScrollableRecordFullyInView != null)
                    this._scrollPositionOfLastVisibleRecord = info.GetOverallScrollPositionForRecord(genCache._highestScrollableRecordFullyInView);
                else
                if (genCache._highestScrollableRecordInView != null)
                    this._scrollPositionOfLastVisibleRecord = info.GetOverallScrollPositionForRecord(genCache._highestScrollableRecordInView);
                else
                    this._scrollPositionOfLastVisibleRecord = scrollPosition;
            }

			// JJD 2/14/11 - TFS66166 - Optimization
			// Keep track of the desired height of elements
			this._desiredHeightMap.Clear();
			if (this._generatedElements != null)
			{
				foreach (UIElement elem in _generatedElements)
				{
					_desiredHeightMap.Add(elem, elem.DesiredSize.Height);
				}
			}

			if (lastOffsetWithinListForTopFixedRecords != this._offsetWithinListForTopFixedRecords ||
				lastOffsetWithinListForBottomFixedRecords != this._offsetWithinListForBottomFixedRecords ||
				lastOffsetWithinListForScrollableRecords != this._offsetWithinListForScrollableRecords ||
				lastNumberOfScrollableRecordsToLayout != this._scrollableElementsGenerated)
				this._recordsInViewChanged = true;

			this._lastOrientationWasVertical = isVertical;

			#region Update scrolling info (offset, viewport and extent) and desired size.

 			if (isVertical)
			{
				ourDesiredSize.Width = genCache._extentNonPrimary;
			}
			else
			{
				ourDesiredSize.Height = genCache._extentNonPrimary;
			}

			if (this.IsScrolling)
			{
				Vector	newScrollOffset = this.ScrollingData._offset;
				Size	scrollViewport	= availableSize;
				Size	scrollExtent	= ourDesiredSize;

				// Calculate the total number of items processed.
				// JJD 11/23/11 - TFS31939
				// Don't decrement the totalItemsProcessedThisPanel by 1
				//int totalItemsProcessedThisPanel = Math.Max(1, this._scrollableElementsFullyInView - 1);
				int totalItemsProcessedThisPanel = Math.Max(1, this._scrollableElementsFullyInView);

				// AS 3/15/07 BR21138
				// If there isn't enough room then we should consider that not all the records are in 
				// view and so we'll adjust the scrollable record count.
				//
				bool wasDesiredExtentGreater = false;

				if (isVertical)
					wasDesiredExtentGreater = ourDesiredSize.Height > effectiveConstraint.Height;
				else
					wasDesiredExtentGreater = ourDesiredSize.Width > effectiveConstraint.Width;

				// Calculate the desired size to return from this measure.
				if (isVertical)
					ourDesiredSize.Height = Math.Min(ourDesiredSize.Height, effectiveConstraint.Height);
				else
					ourDesiredSize.Width = Math.Min(ourDesiredSize.Width, effectiveConstraint.Width);

				if (canAllFixedRecordsFit == false)
				{
					// adjust the scroll range if the fixed records need toscroll becuase of space limitations
					scrollableRecordCount = scrollCount - (topFixedCount + bottomFixedCount);

					scrollableRecordCount			+= bottomFixedCount;
					totalItemsProcessedThisPanel	+= bottomFixedCount - this._offsetWithinListForBottomFixedRecords;

					if (canTopFixedRecordsFit == false)
					{
						scrollableRecordCount			+= topFixedCount;
						totalItemsProcessedThisPanel	+= topFixedCount - this._offsetWithinListForTopFixedRecords;
					}
				}

				// AS 3/15/07 BR21138
				// If we didn't have enough room to show and we didn't adjust the 
				// scroll record count above (if the fixed rows were displayed as unfixed)
				// then reset the scrollable record count based on the total number of 
				// scrollable records.
				//
				if ((scrollableRecordCount > totalItemsProcessedThisPanel) ||
					// AS 3/16/07
					// In the case of groupby, all the records may fix in view but there 
					// could be records scrolled out of view. If the scroll thumb isn't
					// at the top then reset the scrollable record count - i.e. the extent.
					//
					//(wasDesiredExtentGreater && canAllFixedRecordsFit))
					(canAllFixedRecordsFit && (wasDesiredExtentGreater || scrollPosition > 0)))
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
				int overallScrollCount = info.OverallScrollCount;
                if (overallScrollCount > numberOfRecordsInExtent)
                {
                    
                    
                    
                    numberOfRecordsInExtent = Math.Max(numberOfRecordsInExtent, Math.Min(overallScrollCount, overallScrollCount + numberOfRecordsInExtent - scrollCount));
                }

				if (isVertical)
				{
					// Set a viewport value here based on the number of items processed this panel.
					// This will be refined in the Arrange when we can accurately determine the 
					// number of visible records from all panels.
					scrollViewport.Height		= totalItemsProcessedThisPanel;

					scrollExtent.Height			= numberOfRecordsInExtent;
					newScrollOffset.Y			= scrollPosition;

					// AS 4/12/11 TFS62951
					// We may get measured twice - first without a horizontal scrollbar and then with one
					// so if we constrain the offset here we may shift the row back to the left. We'll defer 
					// this until the arrange.
					//
					//newScrollOffset.X			= Math.Max(Math.Min(newScrollOffset.X, (double)(scrollExtent.Width - scrollViewport.Width)), 0d);

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
					newScrollOffset.X			= scrollPosition;

					// AS 4/12/11 TFS62951
					// We may get measured twice - first without a horizontal scrollbar and then with one
					// so if we constrain the offset here we may shift the row back to the left. We'll defer 
					// this until the arrange.
					//
					//newScrollOffset.Y			= Math.Max(Math.Min(newScrollOffset.Y, (double)(scrollExtent.Height - scrollViewport.Height)), 0d);


					// JM 07-17-06 - Also check that the available extent is not infinity (originally uncovered a problem that
					//				 requires this additional check when using a DataPresenterBase inside a ViewBox control) 
					//if (isRootPanel && totalItemsThisPanel > totalItemsProcessedThisPanel && double.IsInfinity(availableExtent) == false)
					//if (isRootPanel && totalItemsThisPanel > totalItemsProcessedThisPanel && double.IsInfinity(availableExtent) == false)
					//		ourDesiredSize.Width = availableExtent;

					// JM 11-12-08 TFS9807
					scrollViewport.Height		= Math.Min(scrollViewport.Height, scrollExtent.Height);
				}

				// JJD 11/23/11 - TFS31939
				// If we have processed all the items in the extent and the
				// extent remaining is 0 and lastScrollableRecordInView variable
				// is not null
				if (numberOfRecordsInExtent == totalItemsProcessedThisPanel &&
					extentRemaining == 0 &&
					lastScrollableRecordInView != null )
					_isLastScrollableRecordFullyInView = true;
            
                Size availableScrollSize = scrollViewport;

				// JJD 3/24/11 - TFS67149
				// Use a more precise method for calculating the scrollable area for rcds
				// by subtracting the fixed and manin header extents from the overall extent
				//if (isVertical)
				//    availableScrollSize.Height = Math.Max(genCache._extentScrollable, 1);
				//else
				//    availableScrollSize.Width = Math.Max(genCache._extentScrollable, 1);
				double adjustment = genCache._extentBottomFixed;

				if (genCache._extentTopFixed > 0)
					adjustment += genCache._extentTopFixed;
				else
					adjustment += genCache._extentRootHeader;

				double availableExtentInPrimary = Math.Max(genCache._extentOverall - adjustment, 1);
                
				if (isVertical)
                    availableScrollSize.Height = availableExtentInPrimary;
                else
                    availableScrollSize.Width = availableExtentInPrimary;

                this.VerifyScrollData(scrollExtent, scrollViewport, newScrollOffset, availableScrollSize);

				#endregion //Set the ScrollViewport and the ScrollExtent based on our orientation.


				// JJD 3/5/07 
				// If we are in an infinite container on the root panel then always return the correct height and/or width
				if (this._isInInfiniteContainer)
				{
                    if ( isVertical )
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


				// Cache our calculated scrolling data and available extent which will 
				// be used in the Arrange to refine the scrollViewport calculation based on the
				// number of visible records from all panels.  Arrange will call InvalidateScrollInfo
				// on the ScrollOwner.
                //if (this.IsScrolling)
                //{
                //    this._cachedScrollDataFromMeasure				= new ScrollData();
                //    this._cachedScrollDataFromMeasure._extent		= scrollExtent;
                //    this._cachedScrollDataFromMeasure._viewport		= scrollViewport;
                //    this._cachedScrollDataFromMeasure._offset		= newScrollOffset;
                //    this._cachedRemainingAvailableExtentFromMeasure = Math.Max(0, availableExtent - overallExtentUsed);
                //}
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


            double totalExentUsed = this._extentUsedByFixedRecordsOnTop + this._extentUsedByFixedRecordsOnBottom + this._extentUsedByScrollableRecords;;

            if (isVertical)
            {
                // JJD 12/17/09 - TFS25669
                // Only return the available size if it is not infinity and the scroll position is
                // greater than 0. If we don't do this then as you scrolldown the size of the
                // panel can change. However, if we don't check for overallScrollPosition > 0
                // then the size of the panel could end up too big.
                //if (!double.IsInfinity(originalAvailableSize.Height))
                if (!double.IsInfinity(originalAvailableSize.Height) && overallScrollPosition > 0)
                    ourDesiredSize.Height = originalAvailableSize.Height;
                else
                    ourDesiredSize.Height = totalExentUsed;

                // JJD 01/05/10 - TFS26101
                // Make sure we don't return a height greater than the available size
                if (!double.IsInfinity(originalAvailableSize.Height))
                    ourDesiredSize.Height = Math.Min(ourDesiredSize.Height, originalAvailableSize.Height);
            }
            else
            {
                // JJD 12/17/09 - TFS25669
                // Only return the available size if it is not infinity and the scroll position is
                // greater than 0. If we don't do this then as you scrolldown the size of the
                // panel can change. However, if we don't check for overallScrollPosition > 0
                // then the size of the panel could end up too big.
                //if (!double.IsInfinity(originalAvailableSize.Width))
                if (!double.IsInfinity(originalAvailableSize.Width) && overallScrollPosition > 0)
                    ourDesiredSize.Width = originalAvailableSize.Width;
                else
                    ourDesiredSize.Width = totalExentUsed;

                // JJD 01/05/10 - TFS26101
                // Make sure we don't return a width greater than the available size
                if (!double.IsInfinity(originalAvailableSize.Width))
                    ourDesiredSize.Width = Math.Min(ourDesiredSize.Width, originalAvailableSize.Width);
            }
 
            //Debug.WriteLine(string.Format("GVP size: {0}, avaliable: {1}, IsRoot {2}, nesting depth {3}, scrollable rcds {4}", ourDesiredSize, originalAvailableSize, isRootPanel, this.ViewableRecords.Count == 0 ? -1 : this.ViewableRecords[0].NestingDepth, this._numberOfScrollableRecordsToLayout));

            //Debug.WriteLine("GVP measure: " + ourDesiredSize.ToString());

			// JJD 4/21/11 - TFS73048 - Optimaztion - added
			_isInMeasure = false;

			// Return our calculated desired size.
			return ourDesiredSize;
		}

				#endregion //MeasureOverride
			
				// JJD 2/14/11 - TFS66166 - Optimization
				#region OnChildDesiredSizeChanged
		/// <summary>
		/// Overridden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
		/// </summary>
		/// <param name="child">The child element whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			// JJD 2/14/11 - TFS66166 - Optimization
			// bypass collapsed children
			if (child.Visibility == Visibility.Collapsed)
				return;

			double oldDesiredHeight;

			// JJD 2/14/11 - TFS66166 - Optimization
			// Get the desired height from the last measure pass
			if (_desiredHeightMap.TryGetValue(child, out oldDesiredHeight))
			{
				Size desiredSize = child.DesiredSize;

				// JJD 2/14/11 - TFS66166 - Optimization
				// if the height hasn't changed we can ignore width changes on certain rcd types
				if (desiredSize.Height == oldDesiredHeight)
				{
					RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(child);

					if (rp != null)
					{
						// JJD 2/14/11 - TFS66166 - Optimization
						// we can always ignore width changes for groupby rcds for flat grid view
						// for both vertical and horizontal orientations
						if (rp is GroupByRecordPresenter)
							return;

						// JJD 2/14/11 - TFS66166 - Optimization
						// For epnadablefieldrcds we can ignore width changes if the orientation 
						// is vertical
						if (rp is ExpandableFieldRecordPresenter &&
							this.LogicalOrientation == Orientation.Vertical)
							return;
					}
				}
			}

			// JJD 4/21/11 - TFS73048 - Optimaztion - added
			// Instead of calling OnChildDesiredSizeChanged here, call InvalidateMeasure asynchronously
			// so we don't interrupt a layout updated pass.
			// JJD 5/31/11 - TFS76852
			// Only bypass calling the base implementation if the dp is 
			// not a synchronuous control, i.e. it supports asynchrous processing
			//if (_isInMeasure)
			DataPresenterBase dp = this.DataPresenter;
			if (_isInMeasure || (dp != null && dp.IsSynchronousControl))
				base.OnChildDesiredSizeChanged(child);
			else
			{
				if (!this.IsHidingRecord &&
					child.Visibility != Visibility.Collapsed)
					GridUtilities.InvalidateMeasureAsynch(this);
			}
		}
				#endregion //OnChildDesiredSizeChanged

                #region OnClearChildren

        /// <summary>
        /// Called when the children of the panel are cleared
        /// </summary>
        protected override void OnClearChildren()
        {
            
            // Make sure the _elementsUsedInAverageCalc is not too high
            // so we don't skew the results too badly if thing change after the clear
            if (this._generatedElements != null)
            {
                int maxElementsToUseInCalc = ((this._generatedElements.Count + 1) * 2) / 3;

                if (this._elementsUsedInAverageCalc > maxElementsToUseInCalc)
                    this._elementsUsedInAverageCalc = Math.Max(2, maxElementsToUseInCalc);
            }

            base.OnClearChildren();

            this._bottomFixedElementsGenerated = 0;
            this._extentUsedByFixedRecordsOnBottom = 0;
            this._extentUsedByFixedRecordsOnTop = 0;
            this._extentUsedByScrollableRecords = 0;
            this._headerElementsGenerated = 0;
            this._isLastScrollableRecordFullyInView = false;
            this._lastMeasureScrollPosition = 0;
            this._lastOverallScrollPosition = 0;
            this._lastRecordAtOverallScrollPosition = null;
            this._offsetWithinListForBottomFixedRecords = 0;
            this._offsetWithinListForScrollableRecords = 0;
            this._offsetWithinListForTopFixedRecords = 0;
            this._scrollableElementsGenerated = 0;
            this._scrollPositionOfLastVisibleRecord = 0;
            this._topFixedElementsGenerated = 0;
         }

                #endregion //OnClearChildren

                // JJD 8/19/09 - NA 2009 Vol 2 - Enhanced grid view
                
                #region OnDeferredRecordScrollStart

        internal override void OnDeferredRecordScrollStart() 
        {
            FlatScrollRecordsCollection records = this.Records;

            // if there are any pending notifications then invalid our measure
            // and call UpdateLayout to force those pending notifications to
            // be processed
            if (records != null &&
                 records.HasPendingNotifications)
            {
                this.InvalidateMeasure();
                this.UpdateLayout();
            }
        }

                #endregion //OnDeferredRecordScrollStart	

                // JJD 05/05/10 - TFS31290 - added
				#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
		{
			base.OnItemsChanged(sender, args);

            // JJD 05/05/10 - TFS31290
            // Set the _recordsInViewChanged flag so we will raise the
            // event in the next arrange pass
            this._recordsInViewChanged = true;
		}

				#endregion //OnItemsChanged

				// JJD 2/14/11 - TFS66166 - Optimization
				#region OnVisualChildrenChanged

		/// <summary>
		/// Called whena child element is added or removed
		/// </summary>
		/// <param name="visualAdded">The child element that was added</param>
		/// <param name="visualRemoved">The child element that was removed</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			// JJD 2/14/11 - TFS66166 - Optimization
			// Remove the element from the _desiredHeightMap so we don't root the element
			UIElement removed = visualRemoved as UIElement;
			if (removed != null)
				_desiredHeightMap.Remove(removed);

			base.OnVisualChildrenChanged(visualAdded, visualRemoved);
		}

				#endregion //OnVisualChildrenChanged	
    
				#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("GridViewPanelFlat: ");

			if (this.DataPresenter != null)
				sb.Append(this.DataPresenter);
    
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
            
		}

				#endregion //SetBoundaryRecordFlags
        
                #region SetOffsetInternal

        internal override void SetOffsetInternal(double newOffset, double oldOffset, bool isSettingVerticalOffset)
        {
            try
            {
                ScrollData scrollData = this.ScrollingData;

                bool orientationIsVertical = this.LogicalOrientation == Orientation.Vertical;
                bool isOffsettingRecords = orientationIsVertical == isSettingVerticalOffset;

                double newOffsetNormalized;
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

                // Make sure the new offset is not negative 
                if (newOffsetNormalized < 0)
                    newOffsetNormalized = 0.0;

                // If records can't be scrolled then set new offset to 0
                if (isOffsettingRecords && !this.CanRecordsBeScrolled)
                    newOffsetNormalized = 0.0;

                if (newOffsetNormalized == oldOffset)
                    return;

                if (isOffsettingRecords &&
                     newOffsetNormalized > oldOffset &&
                     this._isLastScrollableRecordFullyInView &&
                     scrollData._isInDeferredDrag == false)
                {
                    return;
                }

                IViewPanelInfo info = this.ViewPanelInfo;

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

#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

                if (isOffsettingRecords)
                {
					// AS 9/2/09 TFS21748
					// Do not access (and therefore cause to be allocated) a record while using a ScrollingMode of Deferred.
					//
					bool getTopRecord = !scrollData._isInDeferredDrag || scrollData._scrollTip != null;

					if (getTopRecord)
						newTopRecord = this.GetTopRecordFromScrollOffset((int)newOffsetNormalized);

                    // if we're deferred dragging...
                    if (scrollData._isInDeferredDrag)
                    {
                        // initialize the scroll tip if there is one
                        scrollData.InitializeScrollTip(newTopRecord);

                        // store the temp position
                        scrollData._deferredDragOffset = newOffsetNormalized;
                        //Debug.WriteLine(string.Format("Set deferred offset: {0}", newOffsetNormalized));
                        return;
                    }

                    // don't fire the scroll event if we're not changing the top record
                    if (newOffsetNormalized == info.OverallScrollPosition)
                        return;

#region Infragistics Source Cleanup (Region)









































#endregion // Infragistics Source Cleanup (Region)

                    // JJD 1/3/08 - BR26779
                    // At this point we know that the records in view have changed so set the flag here
                    // so we know to raise the event in the arrange
                    this._recordsInViewChanged = true;
                }


                if (newTopRecord != null)
                {
                    FlatScrollRecordsCollection records = this.Records;

                    // JJD 5/25/07
                    // Check to make sure that there is a FlatScrollRecordsCollection
                    if (records == null)
                        return;

                    newOffsetNormalized = Math.Max(Math.Min(newOffsetNormalized, info.OverallScrollCount - (1 + records.CountOfFixedRecordsOnTop + records.CountOfFixedRecordsOnBottom)), 0);
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

                #region Records

        private FlatScrollRecordsCollection Records
        {
            get { return this.RecordListControl.ItemsSource as FlatScrollRecordsCollection; }
        }

                #endregion //Records

            #endregion //Private Properties

        #endregion //Properties

        #region Methods

            #region Private Methods

				#region ArrangeHelper

		private double ArrangeHelper(UIElement visualChildElement,
                                 Dictionary<Record, UIElement> headerRecordMap,
								 bool orientationIsVertical,
								 bool isReverseLayoutForBottomFixed,
								 double extentUsedByLastItem,
                                 double clipExtent,
								 // AS 6/22/09 NA 2009.2 Field Sizing
								 // Removed isAutoFit - this wasn't used.
								 //
								 //bool isAutoFit,
								 Size arrangeSize,
								 ref Rect arrangeRect,
                                 ref double recordIndent)
		{

            bool shouldClip = !double.IsInfinity(clipExtent);

			RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer( visualChildElement );

            Record rcd = null;

            UIElement headerElement = null;

            // cache the previous record indent so we can apply the delta afte rwe get
            // this record's indent
            double previousRecordIndent = recordIndent;

            recordIndent = 0;

			// MD 7/30/10 - ChildRecordsDisplayOrder header placement
			// The record being arranged might not be the record associated with the header record being positioned.
			Record recordWithHeader = null;

            HeaderRecord hr = null;

			// JJD 3/27/07
			// Make sure the rp knows that it is arranged in view
			if (rp != null)
			{
				rp.TreatAsCollapsed = false;

				if (rp.ShouldDisplayRecordContent)
					rp.IsArrangedInView = true;

                rcd = rp.Record;

                if (rcd != null)
                {
					// MD 7/30/10 - ChildRecordsDisplayOrder header placement
					// By default, the current record will be the one associated with the header record.
					recordWithHeader = rcd;

                    hr = rcd as HeaderRecord;
                    
                    recordIndent = this.GetRecordIndent(rcd);

					// MD 7/30/10 - ChildRecordsDisplayOrder header placement
					// We need a reference to the parent record just in case we have to position a header record for that.
					DataRecord parentRecord = null;

                    if (hr == null)
                    {
                        // see if this record has an attached header element
						// JJD 3/14/11 - TFS66823
						// Make sure the headerRecordMap is not null
						//if (headerRecordMap.TryGetValue(rcd, out headerElement))
						if (headerRecordMap != null && headerRecordMap.TryGetValue(rcd, out headerElement))
						{
                            headerRecordMap.Remove(rcd);
						}
						// MD 7/30/10 - ChildRecordsDisplayOrder header placement
						// If the current record doesn't have a header, we still may have to position the header for the parent 
						// record above it if the parent expands its children upwards.
						else
						{
							parentRecord = rcd.ParentDataRecord;
							recordWithHeader = parentRecord;
						}
                    }
					// MD 7/30/10 - ChildRecordsDisplayOrder header placement
					// If a header record is being arranged, we may have to arrange the parent record's header above this record
					// if the parent expands its children upwards.
					else
					{
 
						Record attachedToRecord = hr.AttachedToRecord;

						if (attachedToRecord != null)
						{
							parentRecord = attachedToRecord.ParentDataRecord;
							
							// JJD 1/24/11 - TFS63831
							// Make sure the header is removed from the map so it
							// doesn't get arranged out of view
							// JJD 3/14/11 - TFS66823
							// Make sure the headerRecordMap is not null
							if ( headerRecordMap != null )
								headerRecordMap.Remove(attachedToRecord);
						}
					}

					// MD 7/30/10 - ChildRecordsDisplayOrder header placement
					// If we have a parent record and it shows its children before it, and it should show a header, and the header 
					// should not be attached to the parent record, show the parent's header above the current record being arranged.
					// MD 8/6/10 - TFS36604
					// We don't only need to recursively position headers when the ChildRecordsDisplayOrder is BeforeParent. Refactored this if
					// block to add more conditions.
					//if (parentRecord != null &&
					//    GridViewPanelFlat.DoesRecordHaveHeaderAndChildrenBeforeIt(parentRecord, parentRecord.FieldLayout.HeaderPlacementResolved) &&
					//    parentRecord.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParent)
					//{
					//    if (headerRecordMap.TryGetValue(parentRecord, out headerElement))
					//        headerRecordMap.Remove(parentRecord);
					//}
					if (parentRecord != null &&
						GridViewPanelFlat.DoesRecordHaveHeaderAndChildrenBeforeIt(parentRecord, parentRecord.FieldLayout.HeaderPlacementResolved))
					{
						bool tryPositionHeader = false;

						// If the ChildRecordsDisplayOrder is BeforeParent, we definitely want to try to position another header above 
						// the current record.
						if (parentRecord.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParent)
						{
							tryPositionHeader = true;
						}
						// MD 12/7/10 - TFS36642
						// If the ChildRecordsDisplayOrder is BeforeParentHeadersAttached, and the HeaderPlacement is OnRecordBreak, each parent that is expanded will
						// have children, then a header above it. In that case, when positioning the first child record's header, we do not want to position the parent 
						// header, becasue it should go after the children, even in the case where the first parent record is scrolled out of view. That is becasue the 
						// first visible sibling of the parent occurring before the children will place the header above itself. If, on the other hand, the children are
						// visible first, no parent header should be placed above them because their parent will already display its own header, due to the 
						// OnRecordBreak header placement style. So do nothing here so that we don't get into the else block below.
						else if (parentRecord.FieldLayout.HeaderPlacementResolved == HeaderPlacement.OnRecordBreak)
						{
							// Do nothing and allow tryPositionHeader to remain at False.
						}
						else
						{
							// ...Otherwise, the ChildRecordsDisplayOrder is BeforeParentHeadersAttached. In this case, we also want to
							// recursively try to position headers, but only if the first visible record is out of view. If it were in view,
							// it would have already positioned its header, so its header would no longer be in the headerRecordMap. So if
							// the visible index is not 0, always try to position the header. If the header is in the map, the first viewable
							// record is scrolled out of view.

							// By default, use the visible index of the record.
							int visibleIndex = parentRecord.VisibleIndex;

							// However, if there are group by records and the HeaderPlacementInGroupBy is OnTopOnly, we need to check the 
							// visible index of the top level group, becasue the header is associated with that group by record.
							if (parentRecord.FieldLayout.HasGroupBySortFields &&
								parentRecord.FieldLayout.HeaderPlacementInGroupByResolved == HeaderPlacementInGroupBy.OnTopOnly)
							{
								GroupByRecord topLevelGroup = parentRecord.TopLevelGroup;

								if (topLevelGroup != null)
									visibleIndex = topLevelGroup.VisibleIndex;
							}

							// If the visible index is not 0, try to position the header.
							if (visibleIndex != 0)
							{
								tryPositionHeader = true;
							}
						}

						if (tryPositionHeader)
						{
							// JJD 3/14/11 - TFS66823
							// Make sure the headerRecordMap is not null
							if (headerRecordMap != null && headerRecordMap.TryGetValue(parentRecord, out headerElement))
								headerRecordMap.Remove(parentRecord);
						}
					}
                }
			}

            // For ExpandableFieldRcds with headers attached we want to arrange the header below the expandable field rcd
            // as a placeholder for the data record island. Note this will only happen in the case where there
            // are no child DataRecords visibe (e.g. if they are all filtered out).
            // In this case we want to arrange the header after the expandable field record
			// MD 7/30/10 - ChildRecordsDisplayOrder header placement
			// Check the record associated with the header instead of the record being arranged, because we could be positioning an 
			// expandable field record with its parent record's header above it. Also, we should only be positioning the header after 
			// the record if they children are after the expandable field record. Otherwise, the header should be above the record, 
			// like the children are.
            //bool addHeaderAfterRecord = (rcd is ExpandableFieldRecord);
			bool addHeaderAfterRecord = (recordWithHeader is ExpandableFieldRecord && recordWithHeader.AreChildrenAfterParent);

			// JJD 2/7/11 - TFS35853
			// If a header is attached to the GroupByFieldLayout rcd then it always goes after the record
			if (recordWithHeader is GroupByRecord && 
				recordWithHeader.RecordType == RecordType.GroupByFieldLayout)
				addHeaderAfterRecord = true;

            // if we are reversing the layout order then flip the flag
            if (isReverseLayoutForBottomFixed)
                addHeaderAfterRecord = !addHeaderAfterRecord;

            // arrange the header first if the addHeaderAfterRecord flag is false
            if (headerElement != null &&
                 !addHeaderAfterRecord)
            {
				// MD 7/30/10 - ChildRecordsDisplayOrder header placement
				// Pass off the headerRecordMap because we might position more headers above this header.
                //extentUsedByLastItem = this.ArrangeHelper(headerElement, null, orientationIsVertical, isReverseLayoutForBottomFixed, extentUsedByLastItem, clipExtent, arrangeSize, ref arrangeRect, ref previousRecordIndent);
				extentUsedByLastItem = this.ArrangeHelper(headerElement, headerRecordMap, orientationIsVertical, isReverseLayoutForBottomFixed, extentUsedByLastItem, clipExtent, arrangeSize, ref arrangeRect, ref previousRecordIndent);

                if (shouldClip)
                    clipExtent -= extentUsedByLastItem;
            }

            // calculate the indent delta
            double indentDelta = recordIndent - previousRecordIndent;

            
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			if (orientationIsVertical)
			{
                arrangeRect.X += indentDelta;

				arrangeRect.Height = visualChildElement.DesiredSize.Height;
                    
                
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


				// JJD 3/27/07
				// If autofit is true then use the panel's arrange extent
                // JJD 1/21/09 - NA 2009 vol 1
                // Allow scrolling in autofit mode
                //if (isAutoFit)
                //    arrangeRect.Width = arrangeSize.Width;
                //else
				// AS 11/8/11 TFS88111
				// We should exclude the portion of the area that is used by the indent since this causes an autofit
				// record to be as wide as the viewable area even though it is offset by the indentation.
				//
				//	arrangeRect.Width = Math.Max(arrangeSize.Width, visualChildElement.DesiredSize.Width);
				arrangeRect.Width = Math.Max(arrangeSize.Width - recordIndent, visualChildElement.DesiredSize.Width);

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
                arrangeRect.Y += indentDelta;

                arrangeRect.Width = visualChildElement.DesiredSize.Width;
                
                
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


				// JJD 3/27/07
				// If autofit is true then use the panel's arrange extent
                // JJD 1/21/09 - NA 2009 vol 1
                // Allow scrolling in autofit mode
                //if (isAutoFit)
                //    arrangeRect.Height = arrangeSize.Height;
                //else
				// AS 11/8/11 TFS88111
				// We should exclude the portion of the area that is used by the indent since this causes an autofit
				// record to be as wide as the viewable area even though it is offset by the indentation.
				//
				//	arrangeRect.Height = Math.Max(arrangeSize.Height, visualChildElement.DesiredSize.Height);
				arrangeRect.Height = Math.Max(arrangeSize.Height - recordIndent, visualChildElement.DesiredSize.Height);

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

            
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


            // JJD 4/29/10 - Optimization
            // Moved below until after the clip has been applied . This is not only more
            // efficient but it corrects a problem with the 4.0 framework where clearing the
            // clip after the arrange displated the record as still being clipped from a
            // prior arrange
            //visualChildElement.Arrange(arrangeRect);

			// JJD 12/8/11 - TFS97329
			// Refactored - moved logic into helper method that can be called by any GridViewPanel derived class
			SetRecordPresenterClip(orientationIsVertical, extentUsedByLastItem, clipExtent, arrangeRect, shouldClip, rp);

            // JJD 4/29/10 - Optimization
            // Moved from above until after the clip has been applied . This is not only more
            // efficient but it corrects a problem with the 4.0 framework where clearing the
            // clip after the arrange displated the record as still being clipped from a
            // prior arrange
            visualChildElement.Arrange(arrangeRect);

            // arrange the header first if the addHeaderAfterRecord flag is true
			if (headerElement != null &&
				 addHeaderAfterRecord)
			{
				previousRecordIndent = recordIndent;

				// JJD 2/7/11 - TFS35853
				// take a snapshot of the arrangeRect before arranging the ttrailing header
				Rect arrangeRectSnapshot = arrangeRect;

				// arrange the header last
				// MD 7/30/10 - ChildRecordsDisplayOrder header placement
				// Pass off the headerRecordMap because we might position more headers above this header.
				//extentUsedByLastItem = this.ArrangeHelper(headerElement, null, orientationIsVertical, isReverseLayoutForBottomFixed, extentUsedByLastItem, clipExtent, arrangeSize, ref arrangeRect, ref previousRecordIndent);
				extentUsedByLastItem = this.ArrangeHelper(headerElement, headerRecordMap, orientationIsVertical, isReverseLayoutForBottomFixed, extentUsedByLastItem, clipExtent, arrangeSize, ref arrangeRect, ref previousRecordIndent);

				// JJD 2/7/11 - TFS35853
				// Adjust the arrangeRect's indent back to what it was for the record so
				// we don't leave the rect with the indent of the trailing header arranged above
				if (orientationIsVertical)
					arrangeRect.X = arrangeRectSnapshot.X;
				else
					arrangeRect.Y = arrangeRectSnapshot.Y;
			}

			return extentUsedByLastItem;
		}

				#endregion //ArrangeHelper	

				// MD 7/30/10 - ChildRecordsDisplayOrder header placement
				#region DoesRecordHaveHeaderAndChildrenAbove

		private static bool DoesRecordHaveHeaderAndChildrenBeforeIt(Record record, HeaderPlacement headerPlacement)
		{
			// MD 8/6/10 - TFS36611
			// Moved all code to a new overload.
			return GridViewPanelFlat.DoesRecordHaveHeaderAndChildrenBeforeIt(record, headerPlacement, null);
		}

		// MD 8/6/10 - TFS36611
		// Added a new overload to take the visible index of the record.
		private static bool DoesRecordHaveHeaderAndChildrenBeforeIt(Record record, HeaderPlacement headerPlacement, int? visibleIndex)
		{
			// MD 8/6/10 - TFS36611
			// Refactored so we can resolve the visible index only when we need to so we don't degrade performance.
			//return
			//    record.AreChildrenAfterParent == false &&
			//    record.IsExpanded &&
			//    record.HasVisibleChildren &&
			//    (headerPlacement == HeaderPlacement.OnRecordBreak || record.VisibleIndex == 0);
			if (record.AreChildrenAfterParent == false &&
				record.IsExpanded &&
				record.HasVisibleChildren)
			{
				// If the header placement is OnRecordBreak, all expanded records have the header above them.
				if (headerPlacement == HeaderPlacement.OnRecordBreak)
					return true;

				// Resolve the visible index.
				if (visibleIndex.HasValue == false)
					visibleIndex = record.VisibleIndex;

				// Otherwise, only the top record has the header shown above it.
				if (visibleIndex.Value == 0)
					return true;
			}

			return false;
		}

				#endregion // DoesRecordHaveHeaderAndChildrenAbove

                #region GetRecordIndent

        internal double GetRecordIndent(Record record)
        {
            if (record == null)
                return 0;

            return record.CalculateFlatViewIndent();
        }

                #endregion //GetRecordIndent	

                #region IsRecordInView

        private bool IsRecordInView(Record record, bool allowPartiallyInView)
        {
            if (this._generatedElements == null)
                return false;

            // Walk over our child elements to see if the record is fully visible
            UIElement rcdElement = null;

            foreach (UIElement childElement in this._generatedElements)
            {
                RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(childElement);

                if (rp != null && rp.Record == record)
                {
                    rcdElement = childElement;
                    break;
                }
            }

            if (rcdElement != null)
            {
                Size desiredSize = rcdElement.DesiredSize;

                // JJD 12/18/09
                // Get the ancestor ScrollContentPresenter to get the overall extent
                ScrollContentPresenter scp = Utilities.GetAncestorFromType(this, typeof(ScrollContentPresenter), true, this.DataPresenter) as ScrollContentPresenter;

                FrameworkElement extentProvider = scp != null ? (FrameworkElement)scp : this;

                double startRange = 0;
                double endRange = 0;
                double overallExtent = this._isVertical ? extentProvider.ActualHeight : extentProvider.ActualWidth;

                if (record.IsFixed && record.ParentRecord == null)
                {
                    if (record.IsOnTopWhenFixed)
                    {
                        endRange = this._extentUsedByFixedRecordsOnTop;
                    }
                    else
                    {
                        startRange = overallExtent - this._extentUsedByFixedRecordsOnBottom;
                        endRange = overallExtent;
                    }
                }
                else
                {
                    startRange = this._extentUsedByFixedRecordsOnTop;
                    endRange = overallExtent - this._extentUsedByFixedRecordsOnBottom;
                }

                Point ptLeftTop = rcdElement.TranslatePoint(new Point(0, 0), this);
                Point ptRightBottom = rcdElement.TranslatePoint(new Point(desiredSize.Width, desiredSize.Height), this);

                double startValue = this._isVertical ? ptLeftTop.Y : ptLeftTop.X;
                double endValue = this._isVertical ? ptRightBottom.Y : ptRightBottom.X;

                if (allowPartiallyInView)
                    return ( endValue >= startRange || startValue <= endRange );
                else
                    return (startValue >= startRange && endValue <= endRange);
            }

            return false;
        }

                #endregion //IsRecordInView	

                #region SetScrollPosition

        private void SetScrollPosition(int newScrollPos)
        {
            if (this._isVertical)
                this.SetVerticalOffset(newScrollPos);
            else
                this.SetHorizontalOffset(newScrollPos);
        }

                #endregion //SetScrollPosition	
                
                #region SetScrollPositionToRecord

        private void SetScrollPositionToRecord(Record record)
        {
            int newScrollPos = this.Records.GetScrollPositionOfRecord(record);

            this.SetScrollPosition(Math.Max(0, newScrollPos));
        }

                #endregion //SetScrollPositionToRecord	
    
				#region VerifyScrollData



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

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

                scrolldata._averageHeightOfRecords = this._averageExtentPerRecord;

				if (recordOffset == 0)
				{
					scrolldata._highestRecordDisplayed = recordViewPort;
				}
				else
				{
					double newHighestRecord = recordOffset + recordViewPort - 1;

					if (newHighestRecord > scrolldata._highestRecordDisplayed)
					{
						scrolldata._highestRecordDisplayed = newHighestRecord;
					}
				}

				if (scrolldata._averageHeightOfRecords < 1)
					scrolldata._averageHeightOfRecords = 1;

                
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

				{
					// JJD 3/24/11 - TFS67149
					// Now that we have a morer precise extent for scrolling we don't need to
					// tweak the numberOfRecordsBasedOnAvgHeight calculation
					//double numberOfRecordsBasedOnAvgHeight = Math.Max((scrollHeightOfPanel / scrolldata._averageHeightOfRecords) - 3.0, 1d );
					double numberOfRecordsBasedOnAvgHeight = Math.Max(Math.Ceiling(scrollHeightOfPanel / scrolldata._averageHeightOfRecords), 1d );

					//numberOfRecordsBasedOnAvgHeight = Math.Min(numberOfRecordsBasedOnAvgHeight, Math.Max(recordExtent - (1 + recordOffset), 1d));

					if ( isVertical )
					{
						scrolldata._viewportForScrollbar.Height = numberOfRecordsBasedOnAvgHeight;
						
						if (numberOfRecordsBasedOnAvgHeight < recordExtent)
						{
							// JJD 3/25/11 - TFS67149
							//scrolldata._extentForScrollbar.Height += Math.Min(Math.Min(2.0, recordExtent - numberOfRecordsBasedOnAvgHeight), numberOfRecordsBasedOnAvgHeight);
							scrolldata._extentForScrollbar.Height += Math.Min(Math.Min(1.0, recordExtent - numberOfRecordsBasedOnAvgHeight), Math.Max( numberOfRecordsBasedOnAvgHeight - 1, 0));
						}
                       
                        // JJD 11/10/09 - TFS24368
                        // If the last record is in view adjust the viewport so the thumb is at the bottom
						if (this._isLastScrollableRecordFullyInView)
						{
							// JJD 3/24/11 - TFS67149
							// First adjust the extent then do the viewport to ensure that the thumb is at the bottom
							scrolldata._extentForScrollbar.Height = Math.Max(Math.Ceiling(Math.Max(numberOfRecordsBasedOnAvgHeight, scrolldata._viewportForScrollbar.Height)) + scrolldata._offset.Y, 1);
							scrolldata._viewportForScrollbar.Height = Math.Max(scrolldata._extentForScrollbar.Height - scrolldata._offset.Y, 1);
						}
						else
						{
							// JJD 8/22/11 - TFS83406
							// Since the last record is not fully in view make sure we show a vertical scrollbar
							// by forcing the viewport to be less than the extent
							if ( offset.Y == 0 && scrolldata._viewportForScrollbar.Height >= scrolldata._extentForScrollbar.Height)
								scrolldata._viewportForScrollbar.Height = Math.Max(scrolldata._extentForScrollbar.Height - 1, 1);
							else
							{
								// JJD 10/31/11 - TFS89202
								// Since the last record isn't completely in view make sure that the extent that
								// the scrollbar uses is > the offset plus the viewport. This will prevent
 								// the thumb from being at the bottom before the last record is fully in view
								scrolldata._extentForScrollbar.Height = Math.Max(1 + offset.Y + scrolldata._viewportForScrollbar.Height, scrolldata._extentForScrollbar.Height);
							}
						}
					}
					else
					{
						scrolldata._viewportForScrollbar.Width  = numberOfRecordsBasedOnAvgHeight;

						if (numberOfRecordsBasedOnAvgHeight < recordExtent)
						{
							// JJD 3/25/11 - TFS67149
							//	scrolldata._extentForScrollbar.Width += Math.Min(Math.Min(2.0, recordExtent - numberOfRecordsBasedOnAvgHeight), numberOfRecordsBasedOnAvgHeight);
							scrolldata._extentForScrollbar.Width += Math.Min(Math.Min(1.0, recordExtent - numberOfRecordsBasedOnAvgHeight), Math.Max(numberOfRecordsBasedOnAvgHeight - 1, 0));
						}
                        
                        // JJD 11/10/09 - TFS24368
                        // If the last record is in view adjust the viewport so the thumb is at the bottom
						if (this._isLastScrollableRecordFullyInView)
						{
							// JJD 3/24/11 - TFS67149
							// First adjust the extent then do the viewport to ensure that the thumb is at the bottom
							scrolldata._extentForScrollbar.Width = Math.Max(Math.Ceiling(Math.Max(numberOfRecordsBasedOnAvgHeight, scrolldata._viewportForScrollbar.Width)) + scrolldata._offset.X, 1);
							scrolldata._viewportForScrollbar.Width = Math.Max(scrolldata._extentForScrollbar.Width - scrolldata._offset.X, 1);
						}
						else
						{
							// JJD 8/22/11 - TFS83406
							// Since the last record is not fully in view make sure we show a horizontal scrollbar
							// by forcing the viewport to be less than the extent
							if (offset.X == 0 && scrolldata._viewportForScrollbar.Width >= scrolldata._extentForScrollbar.Width )
								scrolldata._viewportForScrollbar.Width = Math.Max(scrolldata._extentForScrollbar.Width - 1, 1);
							else
							{
								// JJD 10/31/11 - TFS89202
								// Since the last record isn't completely in view make sure that the extent that
								// the scrollbar uses is > the offset plus the viewport. This will prevent
								// the thumb from being at the bottom before the last record is fully in view
								scrolldata._extentForScrollbar.Width = Math.Max(1 + offset.X + scrolldata._viewportForScrollbar.Width, scrolldata._extentForScrollbar.Width);
							}
						}
					}

				}
                if (!this.CanRecordsBeScrolled)
                {
                    if (isVertical)
                    {
                        // JJD 3/24/10 - TFS28905 
                        // If the desired size hasn't been initialized then don't blow away the scroll values
                        if (this.DesiredSize.Height > 0)
                        {
                            scrolldata._offset.Y = 0;
                            scrolldata._extent.Height = 1;
                            scrolldata._extentForScrollbar.Height = 1;
                            scrolldata._viewport.Height = 2;
                            scrolldata._viewportForScrollbar.Height = 2;
                        }

                    }
                    else
                    {
                        // JJD 3/24/10 - TFS28905 
                        // If the desired size hasn't been initialized then don't blow away the scroll values
                        if (this.DesiredSize.Width > 0)
                        {
                            scrolldata._offset.X = 0;
                            scrolldata._extent.Width = 1;
                            scrolldata._extentForScrollbar.Width = 1;
                            scrolldata._viewport.Width = 2;
                            scrolldata._viewportForScrollbar.Width = 2;
                        }
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
    
            #endregion //Private Methods

        #endregion //Methods

        #region GenerationCache internal class

        internal class GenerationCache : IDisposable
        {
            #region Private Members

            internal GridViewPanelFlat _panel;
            private FlatScrollRecordsCollection _records;
            private IList _childElements;
            private int _childElementCount;
            private FieldLayout _currentFieldLayout;
            private ViewableRecordCollection _currentVrc;
            private RecordManager _currentRecordManager;
            private FieldLayout _lastHeaderFieldLayout;
            private ViewableRecordCollection _lastHeaderVrc;
            private int _currentRecordNestingDepth;
            private bool _currentFieldLayoutHasGroups;
            private LabelLocation _currentLabelLocation;
            private HeaderPlacement _currentHeaderPlacement;
            private HeaderPlacementInGroupBy _currentHeaderPlacementInGroupBy;
            private Record _previousRecord;
            internal Record _highestScrollableRecordFullyInView;
            internal Record _highestScrollableRecordInView;
            internal Record _firstBottomFixedRecord;
            // JJD 9/22/09 - TFS21693 - added
			// SSP 3/11/10 TFS27759
			// We can have summary record fixed on the bottom which needs headers and is not a
			// DataRecord. Also we already have _firstBottomFixedRecord above that we currently
			// aren't using it anywhere which we can use in _firstBottomFixedDataRecord's place.
			// 
            //private DataRecord _firstBottomFixedDataRecord;

            private DataPresenterBase _dataPresenter;
            private bool _isVertical;
            private RecordGenerationPhase _currentPhase = RecordGenerationPhase.NotStarted;
            private Size _constraint;
            private Size _measureConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
            // JJD 11/23/09 - TFS24751 - added
            private Size _measureConstraintAutoFit;
            private IItemContainerGenerator _generator;
            internal double _extentOverall;
            internal double _extentRemaining;
            internal double _extentTopFixed;
            internal double _extentBottomFixed;
            internal double _extentScrollable;
            internal double _extentNonPrimary;
			// JJD 3/24/11 - TFS67149
			// Keep track of the extent of a root level header
			internal double _extentRootHeader;
            internal int _currentScrollPosition;
            private IDisposable _currentGeneratorStart;
            private int _currentGeneratorPositionIndex;
            internal int _bottomFixedElementsGenerated;
            internal int _headerElementsGenerated;
            internal int _scrollableElementsFullyInView;
            internal int _scrollableElementsGenerated;
            internal int _topFixedElementsGenerated;
            private int _nextIndexToGen;
            private int _nextChildElementIndex;
            private int _highestScrollPosition;
            private int _recordsToGenerate;
            private bool _isResetPending;
            internal int _estimatedRecordsToLayout;
            internal double _averageExtentPerRecord;
            internal int _elementsUsedInAverageCalc;
            internal List<UIElement> _generatedElements;
            private List<UIElement> _generatedBottomFixedElements;
            private List<UIElement> _generatedHeaderElements;
            private Dictionary<FieldLayout, UIElement> _fieldLayoutHeaderMap;
            
            private Dictionary<RecordManager, FieldLayout> _headersOnTopOfGroupBys;
            private bool _generatingAncestorHeaders;

			// MD 7/30/10 - ChildRecordsDisplayOrder header placement
			private List<HeaderRecord> _headersWhichHaveElementsGenerated;
			private Dictionary<Record, object> _recordsNeedingHeaders;

            #endregion //Private Members

            #region Constructor

            internal GenerationCache(GridViewPanelFlat panel,
                                    bool isVertical,
                                    Size constraint,
                                    int overallScrollPosition,
                                    ScrollDirection scrollDirection)
            {
                this._panel = panel;
                this._records = panel.Records;
                this._isVertical = isVertical;
                this._constraint = constraint;
                this._currentScrollPosition = overallScrollPosition;
                this._dataPresenter = this._panel.DataPresenter;
                this._childElements = this._panel.ChildElements;
                this._childElementCount = this._childElements.Count;

                this._averageExtentPerRecord = this._panel._averageExtentPerRecord;
                this._elementsUsedInAverageCalc = this._panel._elementsUsedInAverageCalc;

                if (this._isVertical)
                {
                    // JJD 11/23/09 - TFS24751
                    // Constrain the height of the size we measure records with to the
                    // passed in constraint. Without doing this the autosize logic for
                    // descendant records doesn't always get refreshed properly.
                    // Note: this is used only for records that are being autofitted
                    this._measureConstraintAutoFit = new Size(double.PositiveInfinity, constraint.Height);
                    this._extentOverall = constraint.Height;
                }
                else
                {
                    // JJD 11/23/09 - TFS24751
                    // Constrain the width of the size we measure records with to the
                    // passed in constraint. Without doing this the autosize logic for
                    // descendant records doesn't always get refreshed properly
                    // Note: this is used only for records that are being autofitted
                    this._measureConstraintAutoFit = new Size(constraint.Width, double.PositiveInfinity);
                    this._extentOverall = constraint.Width;
                }

                if (double.IsInfinity(this._extentOverall))
                {
                    this._extentOverall = double.MaxValue;
                    this._currentScrollPosition = 0;
                }

                int maxRcds = Math.Max(0, this._records.CountOfNonHeaderRecords - (this._records.CountOfFixedRecordsOnTop + overallScrollPosition ));

                this._estimatedRecordsToLayout = Math.Min(maxRcds,(int)Math.Max(this._extentOverall / Math.Max(15, this._averageExtentPerRecord), 2));

                this._extentRemaining = this._extentOverall;

                Debug.Assert(this._dataPresenter != null, "must have a datapresenter here");
                Debug.Assert(!double.IsNaN(this._extentRemaining), "must have a constraint");
                Debug.Assert(!double.IsInfinity(this._extentRemaining), "can't have an infinite constraint");

                // JJD 3/10/10 - TFS28705 - Optimization
                // Get the recordsExpectedToBeGenerated to be generated before
                // we call BeginMeasure below in case the act to getting the records
                // causes a reset notification to get gened
				// JJD 2/14/11 - TFS66166 - Optimization
				// Pass the scroll direction into the GetRecordsExpectedToBeGenerated method
				//IEnumerable recordsExpectedToBeGenerated = this.GetRecordsExpectedToBeGenerated();
				IEnumerable recordsExpectedToBeGenerated = this.GetRecordsExpectedToBeGenerated(scrollDirection);

                // Let the FlatScrollRecordsCollection know we are beginning a measure
                // this should trigger any pending reset notification to be raised
                // before we begin the generation process
                this._records.BeginMeasure(this);

                if (!this.IsResetPending)
                {
                    // JJD 3/10/10 - TFS28705 - Optimization
                    // Pass in the list of records we expect to generate in this pass
                    //this._panel.BeginGeneration(scrollDriection);
                    this._panel.BeginGeneration(scrollDirection, recordsExpectedToBeGenerated);
 
                    this._generator = this._panel.GetGenerator();

                    
                    // allocate a map to keep track of which field layouts have headers generated
                    this._fieldLayoutHeaderMap = new Dictionary<FieldLayout, UIElement>();

                    // allocate a list to hold the generated
                    this._generatedElements = new List<UIElement>(Math.Max(100, this._estimatedRecordsToLayout * 2));

                    int countOfFixedRecordsOnBottom = this._records.CountOfFixedRecordsOnBottom;
                   
                    // If we have fixed bottom records allocate a temporaty list to hold those elements
                    if ( countOfFixedRecordsOnBottom > 0 )
                        this._generatedBottomFixedElements = new List<UIElement>(countOfFixedRecordsOnBottom + 1);
                }

            }

            #endregion //Constructor

            #region Properties

            #region IsResetPending

            internal bool IsResetPending
            {
                get
                {
                    if (this._records.HasPendingNotifications)
                        this._isResetPending = true;

                    return this._isResetPending;
                }
            }

            #endregion //IsResetPending	
    
            #endregion //Properties	

            #region Methods

            #region Internal Methods

            internal bool GenerateAllRecords()
            {
                if (this.IsResetPending)
                    return false;

                bool successful = this.GenerateTopFixedRecords();

                if (successful)
                    successful = this.GenerateBottomFixedRecords();

                if (successful)
                    successful = this.GenerateScrollableRecords();

                // JJD 9/22/09 - TFS21693
                // Use the _firstBottomFixedDataRecord instead of the _firstBottomFixedRecord
                // If we haven't generated a header yet then do it for the first
                // fixed record on the bottom
                if (successful &&
                    this._lastHeaderFieldLayout == null &&
					// SSP 3/11/10 TFS27759
					// 
                    //this._firstBottomFixedDataRecord != null 
					_firstBottomFixedRecord != null
					)
                {
					// SSP 3/11/10 TFS27759
					// 
                    //this.GenerateHeader(this._firstBottomFixedDataRecord.FieldLayout, this._firstBottomFixedDataRecord);
					this.GenerateHeader( _firstBottomFixedRecord.FieldLayout, _firstBottomFixedRecord );
                }

				// MD 7/30/10 - ChildRecordsDisplayOrder header placement
				// Go through the collection of records needing headers and see if any already had headers generated. If any 
				// records haven't had headers generated already, generate them. We need to do this just in case any parent 
				// record expanded upwards and their headers need to show above their children, but they are out of view at the
				// bottom of the grid. In that case, we wouldn't have generated the element for the record and therefore the 
				// record with the associated header either.
				if (_recordsNeedingHeaders != null)
				{
					// Remove all records which already had headers generated.
					if (_recordsNeedingHeaders.Count > 0 && _headersWhichHaveElementsGenerated != null)
					{
						for (int i = 0; i < _headersWhichHaveElementsGenerated.Count; i++)
						{
							HeaderRecord headerRecord = _headersWhichHaveElementsGenerated[i];

							if (headerRecord.AttachedToRecord == null)
								continue;

							_recordsNeedingHeaders.Remove(headerRecord.AttachedToRecord);
						}
					}

					// If there are any records still needing headers, generate them.
					if (_recordsNeedingHeaders.Count > 0)
					{
						foreach (Record record in _recordsNeedingHeaders.Keys)
							this.GenerateHeader(record.FieldLayout, record);
					}
				}

                Debug.Assert(this._generatedElements.Count == this._topFixedElementsGenerated + this._scrollableElementsGenerated, "Generated element count mismatch");
                Debug.Assert(this._bottomFixedElementsGenerated == (this._generatedBottomFixedElements != null ? this._generatedBottomFixedElements.Count : 0), "Generated bottom fixed element count mismatch");
                Debug.Assert(this._headerElementsGenerated == (this._generatedHeaderElements != null ? this._generatedHeaderElements.Count : 0), "Generated header element count mismatch");

                // append the bottom fixed elements
                if ( this._generatedBottomFixedElements != null )
                    this._generatedElements.AddRange(this._generatedBottomFixedElements);

                // append the header elements
                if ( this._generatedHeaderElements != null )
                    this._generatedElements.AddRange(this._generatedHeaderElements);

                // JJD 3/17/10 - TFS28705 
                // Return true only if a reset isn't pending
                //return successful;
                return successful && !this.IsResetPending;
            }

            #endregion //Internal Methods

            #region Private Methods

            #region CacheCurrentRecordSettings

            private void CacheCurrentRecordSettings(Record record, FieldLayout fl)
            {
                this._currentVrc = record.ParentCollection.ViewableRecords;
                this._currentRecordManager = this._currentVrc.RecordManager;
                this._currentFieldLayout = fl;
                this._currentHeaderPlacement = fl.HeaderPlacementResolved;
                this._currentLabelLocation = fl.LabelLocationResolved;
                this._currentFieldLayoutHasGroups = fl.HasGroupBySortFields;
                this._currentRecordNestingDepth = record.NestingDepth;

                if (this._currentFieldLayoutHasGroups)
                    this._currentHeaderPlacementInGroupBy = fl.HeaderPlacementInGroupByResolved;
            }

            #endregion //SetCurrentFieldLayout

            #region GenerateBottomFixedRecords

            private bool GenerateBottomFixedRecords()
            {
                Debug.Assert(this._currentPhase == RecordGenerationPhase.TopFixed, "Bottom fixed records should be 2nd phase");
                this._currentPhase = RecordGenerationPhase.BottomFixed;

                this._recordsToGenerate = this._records.CountOfFixedRecordsOnBottom;

                if (this._recordsToGenerate > 0)
                {
                    this._nextIndexToGen = this._records.CountOfNonHeaderRecords - this._recordsToGenerate;

                    this.GenerateRecordsHelper();
                }

                return !this.IsResetPending;
            }

            #endregion //GenerateBottomFixedRecords
  
            #region GenerateHeader

            private UIElement GenerateHeader(FieldLayout fl, Record attachedToRecord)
            {
                Debug.Assert(fl != null);

                // JJD 2/09/10 - TFS26701
                // Bail out unless LabelLocationResolved is SeparateHeader
                //if (fl == null || this.IsResetPending)
                if (fl == null || this.IsResetPending || fl.LabelLocationResolved != LabelLocation.SeparateHeader)
                    return null;

                HeaderRecord hr;
                int headerIndex = this._records.GetNextHeaderRecord(fl, out hr);

                // see if we have exhausted the header caching slots
                if (hr == null || headerIndex < 0)
                {
                    Debug.Fail("Something is wrong if GetNextHeaderRecord returned null");
                    return null;
                }

                hr.AttachedToRecord = attachedToRecord;

                if ( attachedToRecord != null )
                    hr.AttachedNestingDepth = attachedToRecord.NestingDepth;

                int holdNextIndexToGen = this._nextIndexToGen;

                try
                {
                    this._nextIndexToGen = headerIndex;

                    UIElement headerElement = this.GenerateNextElement(true);

                    if (headerElement != null)
                    {
                        
                        // check _generatingScrollablePrefixHeaders flag
                        if (!this._generatingAncestorHeaders)
                        {
                            if (this._generatedHeaderElements == null)
                                this._generatedHeaderElements = new List<UIElement>();

                            this._generatedHeaderElements.Add(headerElement);
                        }
                        
                        
                        // Update the _fieldLayoutHeaderMap
                        this._fieldLayoutHeaderMap[fl] = headerElement;

						// JM 08-25-09 TFS21365 - Add null check.
						if (attachedToRecord != null)
						{
							// JJD 1/13/12 - TFS95800
							// Moved if block logic into new IsHeaderOnTopOfGroupBys method
							
							// Keep track of all headers that were created on top of groupby rcds
							//RecordType rcdType = attachedToRecord.RecordType;
							//if (rcdType == RecordType.GroupByField ||
							//    ((attachedToRecord is DataRecord) && attachedToRecord.ParentRecord is GroupByRecord) ||
							//    // JJD 9/22/09 - TFS21845
							//    // For SummaryRecords check the parent collection type
							//    // JJD 10/13/09 - TFS22707
							//    // Also check the parent collection type for FilterRecords
							//    //((attachedToRecord is SummaryRecord) && attachedToRecord.ParentCollection.RecordsType == RecordType.GroupByField))
							//    // JJD 1/13/12 - TFS95800
							//    // For summary records we will place a header on top of the group bys if the parent collection
							//    // contains either DataRecords or 
							//    ((attachedToRecord is SummaryRecord || attachedToRecord is FilterRecord) && attachedToRecord.ParentCollection.RecordsType == RecordType.GroupByField))
							if (IsHeaderOnTopOfGroupBys(attachedToRecord))
							{
								if (this._headersOnTopOfGroupBys == null)
									this._headersOnTopOfGroupBys = new Dictionary<RecordManager, FieldLayout>();

								this._headersOnTopOfGroupBys[this._currentRecordManager] = fl;
							}

							// JJD 3/24/11 - TFS67149
							// Keep track of the extent of a root level header
							if (this._extentRootHeader < 1 &&
								attachedToRecord.ParentDataRecord == null)
								this._extentRootHeader = _isVertical ? headerElement.DesiredSize.Height : headerElement.DesiredSize.Width;
						}

                        hr.AttachedToRecordPrevious = attachedToRecord;
                    }

                    return headerElement;
                }
                finally
                {
                    this._nextIndexToGen = holdNextIndexToGen;
                }
            }

            #endregion //GenerateHeader

            #region GenerateNextElement

            private UIElement GenerateNextElement(bool isHeader)
            {
                bool isNewlyRealized;

                // if the current generator position doesn't match the index we
                // want to gen next then restart the generator
                if (this._currentGeneratorStart == null ||
                    this._currentGeneratorPositionIndex != this._nextIndexToGen)
                    this.StartGenerator();

                // generate the next element
                UIElement generatedElement = this._generator.GenerateNext(out isNewlyRealized) as UIElement;

                // JJD 12/17/09 - TFS25760
                // Save the generated index number before we bump
                // _nextIndexToGen below
                int indexGenerated = this._nextIndexToGen;

                // bump the _currentGeneratorPosition and the _nextIndexToGen so we know which element will be generated
                // the next time GenerateNext is called and can compare it above to restart the generator
                // if the _nextIndexToGen is reset in the interim to a different number
                this._currentGeneratorPositionIndex++;
                this._nextIndexToGen++;

                Debug.Assert(generatedElement != null, "The element should have been generated.");

                if (generatedElement == null)
                    return null;


                // If the generated item is 'newly realized', add it to our children collection
                // and 'prepare' it.
                if (isNewlyRealized)
                {
                    if (this._nextChildElementIndex < 0)
                    {
                        // JJD 12/17/09 - TFS25760
                        // Call FindSlotForNewlyRealizedRecordPresenter with the index that 
                        // the element was generated for instead of the next index
                        //this._nextChildElementIndex = this._panel.FindSlotForNewlyRealizedRecordPresenter(this._nextIndexToGen);
                        this._nextChildElementIndex = this._panel.FindSlotForNewlyRealizedRecordPresenter(indexGenerated);
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

                // SSP 3/28/08 - Summaries Functionality
                // The element could be container that contains a record presenter.
                // Moved this below from above since we need to prepare the container so it gets associated
                // with the right record presenter.
                // 
                RecordPresenter recordPresenter = RecordListControl.GetRecordPresenterFromContainer(generatedElement);

                // Decide whether we should show a header.
                if (recordPresenter != null)
                {
                    Record rcd = recordPresenter.Record;

                    // If the rcd was set to Collapsed then loop around and pick up the nest record
                    // Note: This can happen if the generate caused a lazy creation of the Record and
                    // its Visibility was set to Collapsed in the InitializeRecord event
                    if (rcd == null ||
                         rcd.VisibilityResolved == Visibility.Collapsed)
                    {
                        return generatedElement;
                    }

                    bool wasOrientationVertical = recordPresenter.Orientation == Orientation.Vertical;

                    FieldLayout previousFieldLayout = this._currentFieldLayout;

                    FieldLayout fl = rcd.FieldLayout;

                    if (previousFieldLayout == null || isHeader == false )
                        this.CacheCurrentRecordSettings(rcd, fl);

					// JJD 4/09/12 - TFS108549
					// Since we are recycling SummaryRecordPresenters now we need to call
 					// GridUtilities.RecordContentMarginConverter.BumpVersion so the summary
					// cell values will line up correctly
					if (!isNewlyRealized && rcd.RecordType == RecordType.SummaryRecord)
						GridUtilities.RecordContentMarginConverter.BumpVersion(recordPresenter);

                    
                    // Added attached version number property that can be used to trigger a re-conversion
                    // so if the header is being re-used check to see if the attached to record has changed.
                    // If so bump the attached versio n property to trigger the indent re-calculation.
                    if (isHeader && !isNewlyRealized)
                    {
                        HeaderRecord hr = rcd as HeaderRecord;

                        Debug.Assert(hr != null, "This should be a HeaderRecord is isHeader is true");

                        // JJD 06/02/10 - TFS32134
                        // Access the ExpansionIndicatorVisibility property in case something has
                        // changed that affects this visibility of the expansion indicator. If
                        // so accessing the property will raise the propertychanged event
                        if (hr != null)
                        {
                            Visibility vis = hr.ExpansionIndicatorVisibility;
                        }

                        if ( hr != null &&
                             hr.AttachedToRecord != hr.AttachedToRecordPrevious )
                        {
                            GridUtilities.RecordContentMarginConverter.BumpVersion(recordPresenter);

                            // JJD 9/22/09 - TFS21693
                            // We also need to bump the margin converter version on the HeaderLabelArea's parent
                            FrameworkElement fe = recordPresenter.GetHeaderContentSite();

                            if (fe != null)
                            {
                                HeaderLabelArea hla = Utilities.GetDescendantFromType(fe, typeof(HeaderLabelArea), true) as HeaderLabelArea;

                                if (hla != null)
                                {
                                    DockPanel panel = Utilities.GetParent(hla) as DockPanel;

                                    // JJD 9/22/09 - TFS21693
                                    // if the parent is not a DockPanel the margin converter will be
                                    // set directly on the HeaderLabelArea
                                    if ( panel != null )
                                        GridUtilities.RecordContentMarginConverter.BumpVersion(panel);
                                    else
                                        GridUtilities.RecordContentMarginConverter.BumpVersion(hla);
                                }
                            }
                        }
                    }

                    FieldLayout showHeaderForFieldLayout = null;

                    if (isHeader == false &&
                        this._currentPhase != RecordGenerationPhase.BottomFixed &&
                        
                        
                        
                        
                        
                        
                        
                        
                        
                        this._currentLabelLocation == LabelLocation.SeparateHeader)
                    {
                        RecordType rcdType = rcd.RecordType;
 
                        
                        // for Summary and Filter records that are siblings of groupby records
                        // we should treat the record the same way we treat its sibling
                        // groupby records when deciding whether to attach a header
                        switch (rcdType)
                        {
                            case RecordType.SummaryRecord:
                            case RecordType.FilterRecord:
                                {
                                    RecordType collectionRecordType = rcdType;
 
                                    
                                    // if the current record manager has groups then use the 1st group rcd type
                                    if (this._currentRecordManager != null)
                                    {
                                        if (this._currentRecordManager.HasGroups)
                                            collectionRecordType = this._currentRecordManager.Groups[0].RecordType;
                                    }

                                    switch (collectionRecordType)
                                    {
                                        case RecordType.GroupByField:
                                        case RecordType.GroupByFieldLayout:

                                            // JJD 9/22/09 - TFS21845
                                            // We want to treat SummaryRecords that are siblings of DataRecords
                                            // as SummaryRecords (not GroupByrecords).
                                            if (rcdType != RecordType.SummaryRecord ||
                                                rcd.NestingDepthOfGroupByRcds != fl.SortedFields.CountOfGroupByFields)
                                            {
                                                
                                                
                                                
                                                rcdType = RecordType.GroupByField;
                                            }
                                            break;

										// MD 12/3/10 - TFS36634
										// Filter records should also use this logic when they are not fixed.
										case RecordType.FilterRecord:
										// MD 8/6/10 - TFS36611
										case RecordType.SummaryRecord:
											// If this is the top summary record, we may have to place a header above it if the first non-special record 
											// is expanded and has children above it and the headers need to be attached to the associated record.
											if (rcd.FieldLayout.ChildRecordsDisplayOrderResolved == ChildRecordsDisplayOrder.BeforeParentHeadersAttached &&
												rcd.VisibleIndex == 0)
											{
												ViewableRecordCollection viewableRecords = rcd.ParentCollection.ViewableRecords;

												if (viewableRecords != null &&
													// MD 12/3/10 - TFS36634
													// When there are records fixed on top, we should never show the first record's children above the headers, 
													// because there could be fixed data records above the expanded record, and the children should show below them.
													viewableRecords.CountOfFixedRecordsOnTop == 0 &&
													viewableRecords.RecordCollectionSparseArray.VisibleCount > 0)
												{
													Record firstNonSpecialRecord = (Record)viewableRecords.RecordCollectionSparseArray.GetItemAtVisibleIndex(0);

													if (GridViewPanelFlat.DoesRecordHaveHeaderAndChildrenBeforeIt(firstNonSpecialRecord, this._currentHeaderPlacement, 0) &&
														this.ShouldShowHeaderOnFirstRecordInIsland(
															fl,
															this._currentRecordManager,
															this._currentHeaderPlacementInGroupBy,
															this._currentFieldLayoutHasGroups))
													{
														showHeaderForFieldLayout = fl;
													}
												}
											}
											break;

                                    }
                                }
                                break;
                        }


                        switch (rcdType)
                        {
                            #region GroupByRecord

                            case RecordType.GroupByField:
                                {
                                    Record parentRecord = rcd.ParentRecord;

                                    // for top level groupby firled records we show the header if
                                    // the HeaderPlacementInGroupBy is set to OnTopOnly
                                    // and a header has yet to be shown or this is a top level
                                    // groupby.
                                    
                                    // We also waant to show the header for a Filter record that is
                                    // being treated as a groupby
                                    if ((this._currentHeaderPlacementInGroupBy == HeaderPlacementInGroupBy.OnTopOnly ||
                                          rcd.RecordType == RecordType.FilterRecord ||
										  // JJD 1/13/12 - TFS95800 - treat sumary rcds the same as filter rcds
										  rcd.RecordType == RecordType.SummaryRecord) 
                                         &&
                                        (this._lastHeaderFieldLayout == null ||
                                         this._lastHeaderVrc != this._currentVrc ||
                                         parentRecord == null ||
                                         parentRecord.RecordType != RecordType.GroupByField))
                                    {
                                        
                                        // Check the map keyed by vrc to see if the header was
                                        // already displayed
                                        if ((this._lastHeaderFieldLayout != this._currentFieldLayout ||
                                            this._lastHeaderVrc != this._currentVrc) &&
                                            !this.IsGroupByHeaderAlreadyDisplayed(fl))
                                        {
                                            showHeaderForFieldLayout = fl;
                                        }
                                    }
                                }
                                break;

                            #endregion //GroupByRecord

                            #region ExpandableFieldRecord

							// JJD 2/7/11 - TFS35853
							// added logic for GroupByFieldLayout record
							case RecordType.GroupByFieldLayout:
								{
									GroupByRecord gbr = rcd as GroupByRecord;

									Debug.Assert(gbr != null, string.Format("wrong record type for rcd: {0}", rcd));

									// JJD 2/7/11 - TFS35853
									// When an GroupByFieldLayout is expanded and it has a scroll count of 1
									// then we want to show a header after the group by field rcd
									// as a placeholder for the children. For example, this situation can occur when all
									// child rcds are filtered out and the FilterUIType is set to 'LabelIcons'. In this
									// case we need to display the headers for the child data records so there is a ui
									// for changing the filter criteria
									if (gbr != null && gbr.IsExpanded && gbr.ScrollCountInternal == 1)
									{
										showHeaderForFieldLayout = gbr.FieldLayout;
									}
								}
								break;

							case RecordType.ExpandableFieldRecord:
                                {
                                    ExpandableFieldRecord efr = rcd as ExpandableFieldRecord;

                                    Debug.Assert(efr != null, string.Format("wrong record type for rcd: {0}", rcd));

                                    // When an ExpandableFieldRecord is expanded and its child ViewableRecords collection
                                    // has a count of 0 then we want to show a header after the expandable field rcd
                                    // as a placeholder for the children. For example, this situation can occur when all
                                    // child rcds are filtered out and the FilterUIType is set to 'LabelIcons'. In this
                                    // case we need to display the headers for the child data records so there is a ui
                                    // for changing the filter criteria
                                    if (efr != null && efr.IsExpanded)
                                    {
										// JJD 09/22/11  - TFS84708 - Optimization
										// Use the ChildRecordManagerIfNeeded instead which won't create
										// child rcd managers for leaf records
										//RecordManager rm = efr.ChildRecordManager;
										RecordManager rm = efr.ChildRecordManagerIfNeeded;

                                        if (rm != null)
                                        {
                                            ViewableRecordCollection childVrc = rm.ViewableRecords;

                                            if (childVrc != null &&
                                                 childVrc.Count == 0 &&
                                                 rm.Unsorted.Count > 0)
                                            {
                                                showHeaderForFieldLayout = rm.Unsorted[0].FieldLayout;
                                            }
                                        }
                                    }
                                }
                                break;

                            #endregion //ExpandableFieldRecord

                            #region DataRecord, SummaryRecord and FilterRecord

                            case RecordType.DataRecord:
                            case RecordType.FilterRecord:
                            case RecordType.SummaryRecord:
                                {
                                    if (this._lastHeaderFieldLayout == null || this._previousRecord == null ||
                                        (this._previousRecord is ExpandableFieldRecord &&
                                         this._previousRecord == rcd.ParentRecord))
                                    {
                                        // we should always show the header for the first record in an
                                        // island
                                        showHeaderForFieldLayout = fl;
                                    }
                                    else
                                    {
										// MD 7/7/10 - TFS35506
										// If a record has its children displaying above it and it is the first record in its island or the header placement is on record breaks, 
										// it should display a header above it.
										// MD 8/5/10 - TFS36591
										// We also need to check some more conditions, such as whether there is a group by header already displayed.
										//if (GridViewPanelFlat.DoesRecordHaveHeaderAndChildrenBeforeIt(rcd, this._currentHeaderPlacement))
										if (GridViewPanelFlat.DoesRecordHaveHeaderAndChildrenBeforeIt(rcd, this._currentHeaderPlacement) && 
											this.ShouldShowHeaderOnFirstRecordInIsland(
												fl, 
												this._currentRecordManager, 
												this._currentHeaderPlacementInGroupBy, 
												this._currentFieldLayoutHasGroups))
										{
											showHeaderForFieldLayout = fl;
										}
										else
                                        if (this._previousRecord.NestingDepth < rcd.NestingDepth)
                                        {
                                            // Only show the header if our parent is not a groupbyrecord or
                                            // HeaderPlacementInGroupBy resolves to 'WithDataRecords'
											// MD 8/5/10 - TFS36591
											// Moved this check to a helper method so it could be used in other places.
											//if (this._currentHeaderPlacementInGroupBy == HeaderPlacementInGroupBy.WithDataRecords ||
											//    this._currentFieldLayoutHasGroups == false ||
											//    !this.IsGroupByHeaderAlreadyDisplayed(fl))
											if (this.ShouldShowHeaderOnFirstRecordInIsland(
													fl, 
													this._currentRecordManager, 
													this._currentHeaderPlacementInGroupBy, 
													this._currentFieldLayoutHasGroups))
											{
												showHeaderForFieldLayout = fl;
											}
                                        }
                                        else
                                        // if the last header displayed is not from this fieldlayout
                                        // (e.g. with heterogenous data) then show a header now
                                        if (this._previousRecord.FieldLayout != fl &&
                                            this._previousRecord.NestingDepth == rcd.NestingDepth)
                                            showHeaderForFieldLayout = fl;
                                        else
                                        {
                                            // If the header placement is OnRecordBreak and the 
                                            // previous record has a different parent
                                            // then we want to show the header for this record
                                            if (this._currentHeaderPlacement == HeaderPlacement.OnRecordBreak &&
                                                this._previousRecord.ParentRecord != rcd.ParentRecord)
                                            {
                                                showHeaderForFieldLayout = fl;
                                            }

                                        }
                                    }
                                }
                                break;

                            #endregion //DataRecord, SummaryRecord and FilterRecord
                        }
                    }

                    if (!isHeader)
                        this._previousRecord = rcd;

					if (showHeaderForFieldLayout != null && !this.IsResetPending)
					{
						this._lastHeaderFieldLayout = showHeaderForFieldLayout;
						this._lastHeaderVrc = this._currentVrc;

						// try generating a separate header record. If that
						// succeeds then reset shouldShowHeader so we 
						// don't include one with the record
						if (this.GenerateHeader(showHeaderForFieldLayout, rcd) != null)
						{
							showHeaderForFieldLayout = null;
						}
					}
					else
					{
						// JJD 8/23/11 - TFS81798
						// If a Reset was pending then clear the showHeaderForFieldLayout so we
						// don't measure the record presenter to include a header. The header should get 
						// created properly on the next pass.
						showHeaderForFieldLayout = null;
					}

					// MD 7/30/10 - ChildRecordsDisplayOrder header placement
					// Walk up the parent record and keep track of any parent records which show their child before them and need 
					// a header. If they are out of view, we may not generate their elements or their header elements, so we need
					// to keep track of them so we can force the headers do get generated.
					if (isHeader == false)
					{
						Record tempRecord = rcd.ParentRecord;
						while (tempRecord != null)
						{
							if (tempRecord is DataRecord &&
								// MD 8/5/10 - TFS36591
								// We also need to check some more conditions, such as whether there is a group by header already displayed.
								//GridViewPanelFlat.DoesRecordHaveHeaderAndChildrenBeforeIt(tempRecord, tempRecord.FieldLayout.HeaderPlacementResolved))
								GridViewPanelFlat.DoesRecordHaveHeaderAndChildrenBeforeIt(tempRecord, tempRecord.FieldLayout.HeaderPlacementResolved) &&
								this.ShouldShowHeaderOnFirstRecordInIsland(
									tempRecord.FieldLayout,
									tempRecord.RecordManager,
									tempRecord.FieldLayout.HeaderPlacementInGroupByResolved,
									tempRecord.FieldLayout.HasGroupBySortFields))
							{
								if (_recordsNeedingHeaders == null)
									_recordsNeedingHeaders = new Dictionary<Record, object>();

								_recordsNeedingHeaders[tempRecord] = null;
							}

							tempRecord = tempRecord.ParentRecord;
						}
					}

                    recordPresenter.InitializeHeaderContent(isHeader || showHeaderForFieldLayout != null);
                    
                    
                    

                    // Reset the TreatAsCollapsed and IsArrangedInView flags on the record presenter
                    recordPresenter.TreatAsCollapsed = false;
                    recordPresenter.IsArrangedInView = true;

                    recordPresenter.IsActiveHeaderRecord = isHeader;
                    recordPresenter.InitializeRecordContentVisibility(!isHeader);
                    recordPresenter.InitializeExpandableRecordContentVisibility(!isHeader);
                    recordPresenter.InitializeGroupByRecordContentVisibility(!isHeader);

					// SSP 8/28/09 TFS21591
					// Expandable field record that displays a cell inside (like a string field that's
					// made expandable) needs to have nested content. ExpandableFieldRecordPresenter
					// and RecordPresenter classes have the necessary logic to properly initialize or
					// not initialize the nested content and therefore we shouldn't just set it to
					// null here. Enclosed the existing code into the if block.
					// 
					if ( ! ( recordPresenter is ExpandableFieldRecordPresenter ) )
						recordPresenter.InitializeNestedContent(null);

                    if (isNewlyRealized == false && !isHeader)
                        recordPresenter.InitializeIsAlternate(true);

                    if (isHeader == false && this._currentPhase != RecordGenerationPhase.BottomFixed)
                        this._highestScrollPosition = Math.Max(this._highestScrollPosition, recordPresenter.Record.OverallScrollPosition);

                    if (!isNewlyRealized &&
                         (wasOrientationVertical != this._isVertical))
                    {
                        generatedElement.InvalidateMeasure();
                    }

                    // JJD 11/23/09 - TFS24751
                    // See if we are autofitting in the non-scroll dimension
                    bool useAutoFitConstraint = fl == null ? false :
                        useAutoFitConstraint = this._isVertical
                            ? fl.IsAutoFitWidth : fl.IsAutoFitHeight;

                    // Measure the generated element using the adjusted constraint.
                    // JJD 11/23/09 - TFS24751
                    // If we are autofitting in the non-scroll dimension then use
                    // the autofit constraint
                    if (useAutoFitConstraint)
                        generatedElement.Measure(this._measureConstraintAutoFit);
                    else
                        generatedElement.Measure(this._measureConstraint);

                    Size generatedElementDesiredSize = generatedElement.DesiredSize;

                    double elementExtent;
                    double elementExtentNonPrimary;

                    // Update our desired size and total extent used based on the generated element's 
                    // desired size.
                    if (this._isVertical)
                    {
                        elementExtent = generatedElementDesiredSize.Height;
                        elementExtentNonPrimary = generatedElementDesiredSize.Width;
                    }
                    else
                    {
                        elementExtent = generatedElementDesiredSize.Width;
                        elementExtentNonPrimary = generatedElementDesiredSize.Height;
                    }

                    elementExtentNonPrimary += this._panel.GetRecordIndent(rcd);

                    this._extentNonPrimary = Math.Max(elementExtentNonPrimary, this._extentNonPrimary);

                    this._extentRemaining -= elementExtent;

                    #region Maintain counts by phase

                    switch (this._currentPhase)
                    {
                        case RecordGenerationPhase.TopFixed:
                            this._extentTopFixed += elementExtent;

                            if (!isHeader)
                            {
                                this._topFixedElementsGenerated++;
                                this._generatedElements.Add(generatedElement);
                            }
                            break;

                        case RecordGenerationPhase.BottomFixed:
                            this._extentBottomFixed += elementExtent;

                            if (!isHeader)
                            {
                                this._bottomFixedElementsGenerated++;
                                this._generatedBottomFixedElements.Add(generatedElement);

                                if (this._firstBottomFixedRecord == null)
                                    this._firstBottomFixedRecord = rcd;

								
								
								
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                            }

                            break;

                        case RecordGenerationPhase.Scrollable:
                            this._extentScrollable += elementExtent;

                            if (!isHeader)
                            {
                                this._highestScrollableRecordInView = rcd;

                                if (this._extentRemaining >= 0 || GridUtilities.AreClose(0.0, this._extentRemaining))
                                {
                                    this._highestScrollableRecordFullyInView = rcd;
                                    this._scrollableElementsFullyInView++;
                                }
                            }

                            
                            // check _generatingScrollablePrefixHeaders flag
                            if (this._generatingAncestorHeaders || !isHeader)
                            {
                                this._scrollableElementsGenerated++;
                                this._generatedElements.Add(generatedElement);
                            }
                            break;
                    }

                    #endregion //Maintain counts by phase	
    
                    if (isHeader)
                    {
                        
                        // check _generatingScrollablePrefixHeaders flag
                        if (!this._generatingAncestorHeaders)
                            this._headerElementsGenerated++;

						// MD 7/30/10 - ChildRecordsDisplayOrder header placement
						// Keep track of all headers which have been generated. We may need to force some headers to have elements 
						// generated just in case their associated records are out of view and we need to know which w=ones have 
						// already been generated so we don't generate them twice.
						HeaderRecord hr = rcd as HeaderRecord;
						if (hr != null)
						{
							if (_headersWhichHaveElementsGenerated == null)
								_headersWhichHaveElementsGenerated = new List<HeaderRecord>();

							_headersWhichHaveElementsGenerated.Add(hr);
						}
                    }
                    else
                    {
                        #region Calculate average extent per record

                        double oldElementsTimesAverage = this._averageExtentPerRecord * this._elementsUsedInAverageCalc;

                        if (isNewlyRealized)
                            this._elementsUsedInAverageCalc++;
                        else
                        {
                            if (this._elementsUsedInAverageCalc < 1)
                                this._elementsUsedInAverageCalc = 1;
                            else
                                oldElementsTimesAverage -= this._averageExtentPerRecord;
                        }

                        this._averageExtentPerRecord =
                            (elementExtent + oldElementsTimesAverage)
                            / this._elementsUsedInAverageCalc;


                        #endregion //Calculate average extent per record
                    }
                }

                this._extentRemaining = Math.Max(this._extentRemaining, 0);

                return generatedElement;
            }

            #endregion //GenerateNextElement

            
            #region GenerateHeadersForAncestorDataRecords

            private void GenerateHeadersForAncestorDataRecords(Record record)
            {
                int holdNextIndex = this._nextIndexToGen;

                DataRecord parentDr = record.ParentDataRecord;

                Stack<Record> ancestorDataRecordsWithoutHeaders = new Stack<Record>();

                // walk up the parent DataRecord chain and determine
                // which headers need to be placed before the scrollable records
                while (parentDr != null)
                {
                    // if we have already created a header then break out
                    if (this._fieldLayoutHeaderMap.ContainsKey(parentDr.FieldLayout))
                        break;

					// MD 7/7/10 - TFS35508
					// Add a header record for the current record only when children are being shown after the record. 
					// If the children are being shown before the record, we still want to add a header record, but we
					// want to attach it to the first sibling record of the island the corrent record belongs to. That
					// way it remains above the children and doesn't get shifted below them. Also, we should only do this
					// when the parent record is not the first record in the island the the header placement is OnTopOnly.
					// Otherwise, the record will get a header attached to it anyway.
					//ancestorDataRecordsWithoutHeaders.Push(parentDr);
					if (parentDr.AreChildrenAfterParent)
					{
						ancestorDataRecordsWithoutHeaders.Push(parentDr);
					}
					// MD 8/6/10 - TFS36611
					// We need to check the visible index not counting the special records on top if possible.
					// This is so the children of the first parent, when displayed above the special row, will still show
					// the header of their parent when they need to, even though it is not truly at visible index 0.
					//else if (parentDr.VisibleIndex != 0 && parentDr.FieldLayout.HeaderPlacementResolved == HeaderPlacement.OnTopOnly)
					//{
					//    Record firstRecordInIsland = parentDr.ParentCollection.ViewableRecords[0];
					//    ancestorDataRecordsWithoutHeaders.Push(firstRecordInIsland ?? parentDr);
					//}
					else if (parentDr.FieldLayout.HeaderPlacementResolved == HeaderPlacement.OnTopOnly)
					{
						ViewableRecordCollection viewableRecords = parentDr.ParentCollection.ViewableRecords;

						int visibleIndex = 0;
						if (viewableRecords != null)
							visibleIndex = viewableRecords.RecordCollectionSparseArray.GetVisibleIndexOf(parentDr);
						else
							visibleIndex = parentDr.VisibleIndex;

						if (visibleIndex != 0)
						{
							Record firstRecordInIsland = parentDr.ParentCollection.ViewableRecords[0];
							ancestorDataRecordsWithoutHeaders.Push(firstRecordInIsland ?? parentDr);
						}
					}

                    parentDr = parentDr.ParentDataRecord;
                }

                try
                {
                    this._generatingAncestorHeaders = true;

                    // pop off the stack the records that won't be in the scrollable records
                    // but where we still need to show the headers
                    while (ancestorDataRecordsWithoutHeaders.Count > 0)
                    {
                        Record rcd = ancestorDataRecordsWithoutHeaders.Pop();

                        // JJD 2/09/10 - TFS26701
                        // Check to see if GenerateHeader actually returned an element.
                        // If so update the _lastHeaderFieldLayout and _lastHeaderVrc
                        // members so we won't trigger the generation of the header
                        // unnecessarily
                        //this.GenerateHeader(rcd.FieldLayout, rcd);
                        UIElement headerElement = this.GenerateHeader(rcd.FieldLayout, rcd);

                        if (headerElement != null)
                        {
                            this._lastHeaderFieldLayout = rcd.FieldLayout;
                            this._lastHeaderVrc         = rcd.ParentCollection.ViewableRecords;
                        }
                    }
                }
                finally
                {
                    this._generatingAncestorHeaders = false;
                    this._nextIndexToGen = holdNextIndex;
                }
            }

            #endregion //GenerateHeadersForAncestorDataRecords

            #region GenerateRecordsHelper

            private void GenerateRecordsHelper()
            {
                this._previousRecord = null;

                while (this._recordsToGenerate > 0 && this._extentRemaining > 0)
                {
                    UIElement generatedElement = this.GenerateNextElement(false);

                    this._recordsToGenerate--;

                    if (this.IsResetPending || generatedElement == null)
                        break;
                }
            }

            #endregion //GenerateRecordsHelper

            #region GenerateScrollableRecords

            private bool GenerateScrollableRecords()
            {
                Debug.Assert(this._currentPhase == RecordGenerationPhase.BottomFixed, "Scrollable records should be last phase");

                this._currentPhase = RecordGenerationPhase.Scrollable;

                this._recordsToGenerate = this._records.CountOfNonHeaderRecords - (this._records.CountOfFixedRecordsOnTop + this._records.CountOfFixedRecordsOnBottom);

                if (this._recordsToGenerate < 1)
                    return true;

                FlatScrollRecordsCollection.RecordsInViewGenerator rcdsInViewGenerator = this._records.GetRecordsInViewGenerator(this._currentScrollPosition);

                #region Generate any prefix rcds

                List<Record> prefixRcds = rcdsInViewGenerator.PrefixRecords;

                int count = prefixRcds != null ? prefixRcds.Count : 0;
                int index = 0;

                while (index < count && this._recordsToGenerate > 0 && this._extentRemaining > 0)
                {
                    Record rcd = prefixRcds[index];

                    index++;

                    this._nextIndexToGen = this._records.IndexOf(rcd);

                    Debug.Assert(this._nextIndexToGen >= 0, "invalid record index");

                    if (this._nextIndexToGen < 0)
                        continue;

                    
                    // Generate any headers for ancestor data record that haven't been already gened
                    this.GenerateHeadersForAncestorDataRecords(rcd);

                    UIElement generatedElement = this.GenerateNextElement(false);

                    this._recordsToGenerate--;

                    if (this.IsResetPending || generatedElement == null)
                        break;
                }

                if (this._recordsToGenerate < 1 || this.IsResetPending)
                    return !this.IsResetPending;

                #endregion //Generate any prefix rcds

                #region Generate First record

                Record firstRecord = rcdsInViewGenerator.FirstRecord;

                if (firstRecord == null)
                    return !this.IsResetPending;

                this._nextIndexToGen = this._records.IndexOf(firstRecord);

                int lastIndexGened = this._nextIndexToGen;
                Debug.Assert(this._nextIndexToGen >= 0, "invalid record index for 1st record");

                if (this._nextIndexToGen >= 0)
                {
                    
                    
                    // Generate any headers for ancestor data record that haven't been already gened
                    this.GenerateHeadersForAncestorDataRecords(firstRecord);

                    UIElement generatedElement = this.GenerateNextElement(false);

                    this._recordsToGenerate--;

                    if (this._recordsToGenerate < 1 || this.IsResetPending)
                        return !this.IsResetPending;
                }
                else
                    return !this.IsResetPending;

                #endregion //Generate First record

                #region Generate successive rcds

                while (this._recordsToGenerate > 0 && this._extentRemaining > 0)
                {
                    Record rcd = rcdsInViewGenerator.GetSuccessiveRecord();

                    if (rcd == null)
                        break;

                    this._nextIndexToGen = this._records.IndexOf(rcd);

                    if (this.IsResetPending)
                        break;

                    Debug.Assert(this._nextIndexToGen > lastIndexGened, "GetSuccessiveRecord returned an invalid record");
                    Debug.Assert(this._nextIndexToGen >= 0, "invalid record index");

                    if (this._nextIndexToGen <= lastIndexGened)
                        break;

                    lastIndexGened = this._nextIndexToGen;

                    UIElement generatedElement = this.GenerateNextElement(false);

                    this._recordsToGenerate--;

                    if (this.IsResetPending || generatedElement == null)
                        break;
                }

                #endregion //Generate successive rcds

                return !this.IsResetPending;
            }

            #endregion //GenerateScrollableRecords

            #region GenerateTopFixedRecords

            private bool GenerateTopFixedRecords()
            {
                Debug.Assert(this._currentPhase == RecordGenerationPhase.NotStarted, "Top fixed records should be first phase");

                this._currentPhase = RecordGenerationPhase.TopFixed;
                this._nextIndexToGen = 0;
                this._recordsToGenerate = this._records.CountOfFixedRecordsOnTop;

                if (this._recordsToGenerate > 0)
                {
                    this.GenerateRecordsHelper();
                }
                else
                {
                    if (this._records.CountOfNonHeaderRecords == 0 &&
                        this._dataPresenter.DataSource != null)
                    {
                        FieldLayout fl = this._dataPresenter.RecordManager.FieldLayout;

                        if (fl != null && fl.LabelLocationResolved == LabelLocation.SeparateHeader)
                        {
                            UIElement generatedElement = this.GenerateHeader(fl, null);

                            Debug.Assert(generatedElement != null);

                        }
                    }
                }

                return !this.IsResetPending;
            }

            #endregion //GenerateTopFixedRecords

            // JJD 3/10/10 - TFS28705 - Optimization
            #region GetRecordsExpectedToBeGenerated

			// JJD 2/14/11 - TFS66166 - Optimization
			// Added scrollDirection param
			//private IEnumerable GetRecordsExpectedToBeGenerated()
            private IEnumerable GetRecordsExpectedToBeGenerated(ScrollDirection scrollDirection)
            {
                int countTopFixed = this._records.CountOfFixedRecordsOnTop;
                int countBottomFixed = this._records.CountOfFixedRecordsOnBottom;
                int countOfNonHeaderRcds = this._records.CountOfNonHeaderRecords;
                int countOfHeaders = this._records.CountOfHeaderRecords;

				// JJD 06/24/10 - TFS35005
				// Get out if there is a pending reset
				if (this.IsResetPending)
					return null;

                List<Record> headerRcdsToGenerate = new List<Record>();
                
                int oldElementCount = this._panel._generatedElements != null ? this._panel._generatedElements.Count : 0;

                // walk over the old generated elements backwards to get the
                // header elements since they are always at the end
                for (int i = oldElementCount - 1; i >= 0; i--)
                {
                    UIElement element = this._panel._generatedElements[i];

                    if (element == null || element.Visibility == Visibility.Collapsed)
                        continue;

                    RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(element);

                    HeaderRecord hr = rp != null ? rp.Record as HeaderRecord : null;

                    if (hr != null)
                    {
                        if (this._records.Contains(hr))
                            headerRcdsToGenerate.Add(hr);
                    }
                    else
                        break;
                 }

                int scrollableRecordsToGenerate = countOfNonHeaderRcds - (countTopFixed + countBottomFixed);
				
				// JJD 2/14/11 - TFS66166 - Optimization
				// Only increase the number of scrollable rcds to pass in wehen we are scrolling down.
				// When we are scrolling up we should err slightly on the side of passing fewer rcds
				// in so we don't risk excluding extra rcds from the recycling
                //scrollableRecordsToGenerate = Math.Max(Math.Min(scrollableRecordsToGenerate - this._currentScrollPosition, this._estimatedRecordsToLayout + 10 - (countTopFixed + countBottomFixed)), 0);
				int adjustment = scrollDirection == ScrollDirection.Increment ? 10 : -1;
				scrollableRecordsToGenerate = Math.Max(Math.Min(scrollableRecordsToGenerate - this._currentScrollPosition, this._estimatedRecordsToLayout + adjustment - (countTopFixed + countBottomFixed)), 0);

                List<Record> recordsExpected = new List<Record>(countTopFixed + countBottomFixed + scrollableRecordsToGenerate + headerRcdsToGenerate.Count);

                // add top fixed records
				for (int i = 0; i < countTopFixed; i++)
				{
					recordsExpected.Add(this._records[i]);

					// JJD 06/24/10 - TFS35005
					// Get out if there is a pending reset
					if (this.IsResetPending)
						return null;
				}

                // add bottom fixed records
				for (int i = 0; i < countBottomFixed; i++)
				{
					recordsExpected.Add(this._records[countOfNonHeaderRcds - (i + 1)]);

					// JJD 06/24/10 - TFS35005
					// Get out if there is a pending reset
					if (this.IsResetPending)
						return null;
				}

                // add scrollable rcds
				for (int i = 0; i < scrollableRecordsToGenerate; i++)
				{
					// MD 12/3/10
					// Found while fixing TFS36634
					// We were re-adding the fixed records to the collection here. We should have been skipping passed them.
					//recordsExpected.Add(this._records[this._currentScrollPosition + i]);
					recordsExpected.Add(this._records[this._currentScrollPosition + i + countTopFixed]);

					// JJD 06/24/10 - TFS35005
					// Get out if there is a pending reset
					if (this.IsResetPending)
						return null;
				}

                // add previous header records
                if (headerRcdsToGenerate.Count > 0)
                    recordsExpected.AddRange(headerRcdsToGenerate);

                return recordsExpected;
            }

            #endregion //GetRecordsExpectedToBeGenerated	
    
            #region IsGroupByHeaderAlreadyDisplayed

            private bool IsGroupByHeaderAlreadyDisplayed(FieldLayout fl)
            {
				// MD 8/5/10 - TFS36591
				// Moved all code to the new overload.
				return this.IsGroupByHeaderAlreadyDisplayed(fl, _currentRecordManager);
			}

			// MD 8/5/10 - TFS36591
			// Added a new overload to take the record manager.
			private bool IsGroupByHeaderAlreadyDisplayed(FieldLayout fl, RecordManager recordManager)
			{
                if (this._headersOnTopOfGroupBys == null)
                    return false;

                FieldLayout flOfHeader = null;

				// MD 8/5/10 - TFS36591
				// Use the passed in record manager instead of the member variable.
                //this._headersOnTopOfGroupBys.TryGetValue(this._currentRecordManager, out flOfHeader);
				this._headersOnTopOfGroupBys.TryGetValue(recordManager, out flOfHeader);

                return flOfHeader == fl;
            }

            #endregion //IsGroupByHeaderAlreadyDisplayed	

			// JJD 1/13/12 - TFS95800 - added
			#region IsHeaderOnTopOfGroupBys

			private static bool IsHeaderOnTopOfGroupBys(Record attachedToRecord)
			{
				RecordType rcdType = attachedToRecord.RecordType;

				if (rcdType == RecordType.GroupByField)
					return true;

				if (attachedToRecord is DataRecord && attachedToRecord.ParentRecord is GroupByRecord)
					return true;

				if (attachedToRecord is FilterRecord && attachedToRecord.ParentCollection.RecordsType == RecordType.GroupByField)
					return true;

				if (attachedToRecord is SummaryRecord)
				{
					switch (attachedToRecord.ParentCollection.RecordsType)
					{
						case RecordType.GroupByField:
							return true;

						case RecordType.DataRecord:
							// JJD 1/13/12 - TFS95800
							// For summary records we will place a header on top of the group bys if there
							// are groupbys
							return attachedToRecord.FieldLayout.HasGroupBySortFields;
					}
				}

				return false;
			}

			#endregion //IsHeaderOnTopOfGroupBys	
        
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

			// MD 8/5/10 - TFS36591
			#region ShouldShowHeaderOnFirstRecordInIsland

			private bool ShouldShowHeaderOnFirstRecordInIsland(
				FieldLayout fieldLayout, 
				RecordManager recordManager, 
				HeaderPlacementInGroupBy headerPlacementInGroupBy, 
				bool fieldLayoutHasGroups)
			{
				if (fieldLayoutHasGroups == false)
					return true;

				if (headerPlacementInGroupBy == HeaderPlacementInGroupBy.WithDataRecords)
					return true;

				return this.IsGroupByHeaderAlreadyDisplayed(fieldLayout, recordManager) == false;
			}

			#endregion // ShouldShowHeaderOnFirstRecordInIsland

            #region StartGenerator

            private void StartGenerator()
            {
                this.ReleaseGeneratorStart();

                this._currentGeneratorPositionIndex = this._nextIndexToGen;

                GeneratorPosition generatorStartPosition = this._panel.GetGeneratorPositionFromItemIndex(this._nextIndexToGen, out this._nextChildElementIndex);

                this._currentGeneratorStart = this._generator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true);

                this._nextChildElementIndex = -1;
            }

            #endregion //StartGenerator

            #endregion //Private Methods

            #endregion //Methods

            #region IDisposable Members

            public void Dispose()
            {
                this.ReleaseGeneratorStart();

                if ( this._generator != null )
                    this._panel.EndGeneration();

                this._records.EndMeasure();
            }

            #endregion

            #region RecordGenerationPhase privae enum

            private enum RecordGenerationPhase
            {
                NotStarted,
                TopFixed,
                BottomFixed,
                Scrollable
            }

            #endregion //RecordGenerationPhase privae enum
        }

        #endregion //GenerationCache	
    
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