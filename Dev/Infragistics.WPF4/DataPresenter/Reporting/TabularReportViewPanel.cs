using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Virtualization;
using Infragistics.Windows.Reporting;
using System.Windows;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Selection;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading;
using System.Windows.Documents;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Panel responsible for laying out records in a tabular arrangment on behalf of a <see cref="TabularReportView"/>.
	/// </summary>
    /// <remarks>
    /// <p class="note"><b>Note: </b>The TabularReportViewPanel is designed to be used with the <see cref="XamDataGrid"/> 
    /// and <see cref="XamDataPresenter"/> controls and is for Infragistics internal use only.  The control is not
    /// designed to be used outside of the <see cref="XamDataGrid"/> or <see cref="XamDataPresenter"/>.  
    /// You may experience undesired results if you try to do so.</p>
    /// </remarks>
	/// <seealso cref="TabularReportView"/>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public sealed class TabularReportViewPanel : RecyclingItemsPanel,
												 IEmbeddedVisualPaginator,
                                                 IScrollInfo
    {

        #region Member variables
        int _pageCount;
        bool _isEndReached;
        double _scaleFactor;
        Hashtable _repeatTypeHelper;
        DataPresenterBase _dataPresenter;
        TabularPagePosition _currentPagePosition = null;
        double _availableExtentInPreviousMeasure;
        double _largestRecordExtentInPreviousMeasure;
        RecordListControl _recordListControl;
        ReportSettings _reportSettings;
        Orientation _gridViewOrientation;
        int _totalVisibleGeneratedItems;
        #endregion //Memeber variables

        #region Constructors
        /// <summary>
        /// TabularReportViewPanel's constructor
        /// </summary>
        public TabularReportViewPanel()
        {
            _repeatTypeHelper = new Hashtable();
            this._currentPagePosition = new TabularPagePosition();
        }
        #endregion //Constructors

        #region Properties

            #region Public properties

                #region DataPresenter
        /// <summary>
        /// Returns the Infragistics.Windows.DataPresenter.DataPresenterBase that owns this panel
        /// </summary>
        public DataPresenterBase DataPresenter
        {
            get
            {
                return this._dataPresenter;
            }
        }

                #endregion //DataPresenter

            #endregion

            #region Internal properties

                #region RecordListControl

    




            internal RecordListControl RecordListControl
            {
                get
                {
                    if (this._recordListControl == null)
                        this._recordListControl = ItemsControl.GetItemsOwner(this) as RecordListControl;

                    return this._recordListControl;
                }
            }

                    #endregion //RecordListControl

                #region ScaleFactor
    





            internal double ScaleFactor
            {
                get { return _scaleFactor; }
            }

            #endregion //ScaleFactor

            #endregion //Internal properties

        #endregion

        #region Base Class Overrides

            #region Methods

                #region ArrangeOverride

            /// <summary>
        /// Called to layout child elements.
        /// </summary>
        /// <param name="finalSize">The size available to this element for arranging its children.</param>
        /// <returns>The size used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);

            if (this._currentPagePosition == null)
                return finalSize;

            IList children = this.ChildElements;

            TabularReportView view = this.DataPresenter.CurrentViewInternal as TabularReportView;

            double levelIndentation = view.LevelIndentation;
            double y = -this._currentPagePosition.VerticalOffset;
            double x = -this._currentPagePosition.HorizontalOffset;
            double elementHeight;
            double elementWidth;
            bool isVertical = this._gridViewOrientation == Orientation.Vertical;
            bool scale = ((this._reportSettings.HorizontalPaginationMode == HorizontalPaginationMode.Scale) &&
                (_scaleFactor < 1));
            Vector extraOffset = new Vector();
            Stack<Vector> extraOffsetStack = null;

            // JJD 10/16/08 - TFS8270
            // KeepTrack of the number of records we have included so far
            int rcdsIncluded = 0;

            // we only need the extra offset stack in horizontal orientation for handling groupby indents
            if (!isVertical)
            {
                extraOffsetStack = this._currentPagePosition.ExtraOffsetStack;
                if (extraOffsetStack.Count > 0)
                {
                    foreach (Vector vector in extraOffsetStack)
                        extraOffset += vector;
                }
            }

            foreach (UIElement el in children)
            {
                // JJD 4/07/08 - Summaries Functionality
                // The element could be container that contains a record presenter so call the
                // static GetRecordPresenterFromContainer method to get the RecordPresenter.
                RecordPresenter rp = RecordListControl.GetRecordPresenterFromContainer(el);

                bool isGroupByRecord = rp is GroupByRecordPresenter;
                
                int groupbyNestingDepth = rp.Record.NestingDepthOfGroupByRcds;

                double indent = 0;

                if (rp != null)
                {
                    indent = rp.Record.NestingDepth * levelIndentation;

                    if (extraOffsetStack != null)
                    {
                        while (groupbyNestingDepth < extraOffsetStack.Count)
                        {
                            extraOffset.Y -= extraOffsetStack.Pop().Y;
                        }

                        if ( !isGroupByRecord )
                            indent -= groupbyNestingDepth * levelIndentation;
                    }

                    if ( isGroupByRecord )
                    {
                        x = (indent * this._scaleFactor) + extraOffset.X;

                        if (!isVertical)
                            y = -this._currentPagePosition.VerticalOffset + extraOffset.Y;

                    }
                    else if (isVertical)
                        x = -this._currentPagePosition.HorizontalOffset + (indent * this._scaleFactor) + extraOffset.X;
                    else
                    {
                        y = -this._currentPagePosition.VerticalOffset + (indent * this._scaleFactor) + extraOffset.Y;
                    }
                }

                double unscaledHeight = el.DesiredSize.Height;
                double unscaledWidth = el.DesiredSize.Width;

                 if (isGroupByRecord)
                {
                    if (isVertical)
                    {
                        if (scale == false)
                        {
                            // indent the groupby record and make it as wide as the page
                            // so that it repeats on every page
                            
                            // JJD 9/3/09 - TFS20581
                            // Add the page offset to the width since we will now be moving the description
                            // into view using render transforms below
                            // unscaledWidth = Math.Max(finalSize.Width - indent, 10);
                            unscaledWidth = Math.Max(Math.Max(this._largestRecordExtentInPreviousMeasure, finalSize.Width + this._currentPagePosition.HorizontalOffset) - indent, 10);

                            x = indent;
                            
                            // JJD 9/3/09 - TFS20581
                            // Adjust the x coordinate used during the arrange by the page offset value then compensate
                            // by setting the FixedNearElementTransform Property
                            // NOte: when and if we support field fixing inside the report we will have to 
                            // modify/replace this logic.
                            x -= this._currentPagePosition.HorizontalOffset;
                            rp.SetValue(RecordPresenter.FixedNearElementTransformPropertyKey, new TranslateTransform(this._currentPagePosition.HorizontalOffset, 0d));
                            
                            // JJD 9/3/09 - TFS20581
                            // we need to do a mirror of the FixedNearElementTransform for the ScrollableElementTransform
                            // property so any summary elements contained in the group by maintain their correct alignment
                            rp.SetValue(RecordPresenter.ScrollableElementTransformPropertyKey, new TranslateTransform(-this._currentPagePosition.HorizontalOffset, 0d));

                        }
                        else
                            unscaledWidth = Math.Max(unscaledWidth, this._largestRecordExtentInPreviousMeasure - indent);
                    }
                    else
                    {
                    }
                }

                elementHeight = unscaledHeight;
                elementWidth = unscaledWidth;
                if (scale)
                {
                    // corect element height before check for arrange
                    // this is important for last records
                    elementHeight = unscaledHeight * this._scaleFactor;
                    elementWidth = unscaledWidth * this._scaleFactor;
                }

				// JJD 1/14/11 - TFS61227
				// Moved logic to set the scale transform before we call arrange below
				if (scale)
				{
					// apply transform only when the record doesnt fit the page
					el.RenderTransform = new ScaleTransform(_scaleFactor, _scaleFactor);
				}
				else
					el.ClearValue(RenderTransformProperty);

                // If the entire record can't fit then arrange it out of view

                // JJD 10/16/08 - TFS8270
                // Make sure we show at least 1 record
                //if (((y + elementHeight > finalSize.Height) && isVertical)
                // || ((x + elementWidth > finalSize.Width) && !isVertical))
				if (rcdsIncluded > 0 &&
					(((y + elementHeight > finalSize.Height) && isVertical)
					|| ((x + elementWidth > finalSize.Width) && !isVertical)))
				{
					// JJD 10/14/10 - TFS48063
					// Since we are arranging the record out of view set the 
					// IsArrangedInView to false so we bypass the record when
					// calculating the autofit width of the cell area
					if (rp != null)
						rp.IsArrangedInView = false;

					el.Arrange(new Rect(-100000, -100000, unscaledWidth, unscaledHeight));
				}
				else
				{
					// JJD 10/14/10 - TFS48063
					// Since we are arranging the record out of view set the 
					// IsArrangedInView to true
					if (rp != null)
						rp.IsArrangedInView = true;

					el.Arrange(new Rect(x, y, unscaledWidth, unscaledHeight));

					// JJD 10/16/08 - TFS8270
					// KeepTrack of the number of records we have included so far
					rcdsIncluded++;
				}

                // prepare for next record
                if (isGroupByRecord)
                {
                    y += elementHeight;
                    extraOffset.Y += elementHeight;

                    if (extraOffsetStack != null)
                        extraOffsetStack.Push(new Vector(levelIndentation, elementHeight));
                }
                else if (isVertical)
                    y += elementHeight;
                else
                {
                    x += elementWidth;
                    if ( rp != null && rp.Record.ParentRecord is GroupByRecord )
                        extraOffset.X += elementWidth;
                }
             }
            return finalSize;
        }
                #endregion //ArrangeOverride

                #region GetCanCleanupItem

        /// <summary>
		/// Derived classes must return whether the item at the specified index can be cleaned up.
		/// </summary>
		/// <param name="itemIndex">The index of the item to be cleaned up.</param>
		/// <returns>True if the item at the specified index can be cleaned up, false if it cannot.</returns>
		protected override bool GetCanCleanupItem(int itemIndex)
		{
            return true;
		}


				#endregion //GetCanCleanupItem		
    
                #region MeasureOverride

        /// <summary>
        /// Called to give an element the opportunity to return its desired size and measure its children.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this._currentPagePosition == null)
                return new Size(1, 1);

            // this is not wrong, but some times Measure is call more than once and last call is correct
            //if (_currentPagePosition.IndexOfLastRecordOnPage != -1)
            //    return availableSize;

            #region Init local and class members that will be used
            DataPresenterBase dp = this.DataPresenter;
            TabularReportView view = dp.CurrentViewInternal as TabularReportView;

            if (this._reportSettings.RepeatType == RepeatType.PageBreak)
            {
                // clear hash for every page
                _repeatTypeHelper.Clear();
            }

            this._largestRecordExtentInPreviousMeasure = 0;
            double levelIndentation = view.LevelIndentation;

            Size sizeRequired = new Size(0, 0);
            bool isNewlyRealized;
            bool isInfiniteHeight = double.IsPositiveInfinity(availableSize.Height);
            bool isInfiniteWidth = double.IsPositiveInfinity(availableSize.Width);
            int indexToInsertNewlyRealizedItems = -1;
            int firstIndex = 0;
            int currentItemIndex = this._currentPagePosition.IndexOfFirstRecordOnPage;
            FieldLayout previousFieldLayout = null;

            // JJD 1/28/09 - TFS11778
            // Keep track of the previusRecord
            Record previousRecord = null;

            // JJD 6/4/09 - TFS17060
            // Keep track of Header placementInGroupby
            HeaderPlacementInGroupBy previousHeaderPlacementInGroupBy = HeaderPlacementInGroupBy.WithDataRecords;
            
            // JJD 6/4/09 - TFS17060
            // keep a flag if the first rcd hase been processed
            bool wasFirstRecordProcessed = false;

            int previousNestingDepth = 0;

            // params for whole page scaling
            double scaleWidth = availableSize.Width;
            double scaleHeight = availableSize.Height;
            double scaleXPrevious = 0;
            // init scale factor for current page
            this._scaleFactor = 1;
            Vector groupByOffset = new Vector();
            bool isVertical = this._gridViewOrientation == Orientation.Vertical;

            // JJD 3/25/09 - TFS15888
            // Added key that contains both fieldlayout and nesting depth to keep track of
            // when headers are needed
            FieldLayoutNestingLevelKey nestingKey = null;

            // JJD 10/16/08 - TFS8270
            // KeepTrack of the number of records we have included so far
            int rcdsIncluded = 0;

            #endregion

            this.BeginGeneration(ScrollDirection.Increment);
            GeneratorPosition generatorStartPosition = GetGeneratorPositionFromItemIndex(currentItemIndex, out firstIndex);

            IItemContainerGenerator generator = this.ActiveItemContainerGenerator;
            using (IDisposable disposable = generator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
            {
                int totalRcds = this.RecordListControl.Items.Count;
                int childCount = this.ChildElements.Count;

				// JJD 8/13/10 - TFS35641
				// Handle the special case where there weren't any rcds so we inserted the template rcd
				// so the headers would be displayed.
				// Set a flag so we know not to show the template data rcd, just its header
				bool isTemplateRcd = false;
				if (totalRcds == 1)
				{
					DataRecord dr = this.RecordListControl.Items[0] as DataRecord;
					isTemplateRcd = dr != null && dr.IsTemplateDataRecord;
				}

                while (currentItemIndex < totalRcds)
                {
                    UIElement generatedElement = generator.GenerateNext(out isNewlyRealized) as UIElement;

                    generatedElement.Visibility = Visibility.Visible;

                    if (isNewlyRealized)
                    {
                        if (indexToInsertNewlyRealizedItems < 0)
                        {
                            indexToInsertNewlyRealizedItems = this.FindSlotForNewlyRealizedRecordPresenter(currentItemIndex);
                        }

                        if (indexToInsertNewlyRealizedItems >= childCount)
                            this.AddInternalChild(generatedElement);
                        else
                            this.InsertInternalChild(indexToInsertNewlyRealizedItems, generatedElement);

                        indexToInsertNewlyRealizedItems++;

                        childCount++;

                        generator.PrepareItemContainer(generatedElement);
                    }

                    // JJD 4/07/08 - Summaries Functionalitymo
                    // The element could be container that contains a record presenter so call the
                    // static GetRecordPresenterFromContainer method to get the RecordPresenter.
                    RecordPresenter recordPresenter = RecordListControl.GetRecordPresenterFromContainer(generatedElement);

                    double indent = 0;

                    if (recordPresenter != null)
                    {
						// JJD 8/13/10 - TFS35641
						// Handle the special case where there weren't any rcds so we inserted the template rcd
						// so the headers would be displayed.
						// Use the flag set above so we know not to show the content for the template rcd, just its header
						//bool shouldShowContent = true;
                        bool shouldShowContent = !isTemplateRcd;
                        bool shouldShowHeader = false;
                        recordPresenter.InitializeRecordContentVisibility(shouldShowContent);
                        recordPresenter.InitializeExpandableRecordContentVisibility(shouldShowContent);
                        recordPresenter.InitializeGroupByRecordContentVisibility(shouldShowContent);
                        Record rcd = recordPresenter.Record;

                        int nestingDepth = rcd.NestingDepth;

                        indent = levelIndentation * nestingDepth;

                        if (rcd is DataRecord)
                        {
                            #region Handle DataRecords

                            FieldLayout fl = rcd.FieldLayout;

                            // JJD 3/25/09 - TFS15888
                            // Reuse or create key that contains both fieldlayout and nesting depth to keep track of
                            // when headers are needed
                            if (nestingKey == null ||
                                 nestingKey.IsSameKey(fl, nestingDepth) == false)
                                nestingKey = new FieldLayoutNestingLevelKey(fl, nestingDepth);

                            // resolve how header appears
                            if (fl != previousFieldLayout ||
                                 nestingDepth != previousNestingDepth)
                            {
                                if (this._reportSettings.RepeatType == RepeatType.FirstOccurrence)
                                {
                                    // JJD 3/25/09 - TFS15888
                                    // use the nestingkey created above
                                    //DataRecord hashValue = _repeatTypeHelper[nestingDepth] as DataRecord;
                                    DataRecord hashValue = _repeatTypeHelper[nestingKey] as DataRecord;
                                    if (hashValue == null)
                                    {
                                        shouldShowHeader = true;
                                        // JJD 3/25/09 - TFS15888
                                        // use the nestingkey created above
                                        //_repeatTypeHelper[nestingDepth] = rcd;
                                        _repeatTypeHelper[nestingKey] = rcd;
                                    }
                                    else if (hashValue == rcd)
                                    {
                                        // show header when the printing is mosaic
                                        shouldShowHeader = true;
                                    }
                                }
                                else if (this._reportSettings.RepeatType == RepeatType.PageBreak)
                                {
                                    // for PageBreak the appearing depends from when we clear the _repeatTypeHelper
                                    // hash table for each page
                                    // JJD 3/25/09 - TFS15888
                                    // use the nestingkey created above
                                    //bool? hashValue = _repeatTypeHelper[nestingDepth] as bool?;
                                    bool? hashValue = _repeatTypeHelper[nestingKey] as bool?;
                                    if (!hashValue.HasValue)
                                    {
                                        shouldShowHeader = true;
                                        // JJD 3/25/09 - TFS15888
                                        // use the nestingkey created above
                                        //_repeatTypeHelper[nestingDepth] = true;
                                        _repeatTypeHelper[nestingKey] = true;
                                    }
                                }
                                else
                                {
                                    shouldShowHeader = true;
                                }
                                previousNestingDepth = nestingDepth;
                                previousFieldLayout = fl;
                                
                                // JJD 6/4/09 - TFS17060
                                // Keep track of Header placementInGroupby
                                previousHeaderPlacementInGroupBy = fl.HeaderPlacementInGroupByResolved; 
                            }

                            // JJD 1/28/09 - TFS11778
                            // We always want to show a header if the previous record was a groupby
                            if (previousRecord is GroupByRecord)
                                shouldShowHeader = true;

                            // JJD 6/4/09 - TFS17060
                            // If we are in a groupby situation and the HeaderPlacementIngroupBy is OnTop
                            // then we don't want to have headers with data records except if its the 
                            // 1st record on the page
                            if (wasFirstRecordProcessed == true &&
                                previousHeaderPlacementInGroupBy == HeaderPlacementInGroupBy.OnTopOnly &&
                                rcd.ParentRecord is GroupByRecord)
                                shouldShowHeader = false;

                            #endregion //Handle DataRecords
                        }
                        // JJD 6/4/09 - TFS17060
                        // Added support for HeaderPalecementingroupBy OnTop
                        else if (rcd is GroupByRecord ) 
                        {
                            #region Handle GroupByRecords

                            FieldLayout fl = rcd.FieldLayout;

                            if (fl != previousFieldLayout)
                            {
                                previousHeaderPlacementInGroupBy = fl.HeaderPlacementInGroupByResolved;

                                if (previousHeaderPlacementInGroupBy == HeaderPlacementInGroupBy.OnTopOnly)
                                {
                                    // JJD 6/4/09 - TFS17060
                                    // If this is a top level Groupy or its the first record on the page
                                    // then show the header
                                    if (previousFieldLayout == null ||
                                        !(rcd.ParentRecord is GroupByRecord))
                                        shouldShowHeader = true;
                                         
                                }
                                previousNestingDepth = rcd.NestingDepth;
                                previousFieldLayout = fl;
                            }

                            #endregion //Handle GroupByRecords
                        }

                        recordPresenter.InitializeHeaderContent(shouldShowHeader);

                        // JJD 1/28/09 - TFS11778
                        // Keep track of the previusRecord
                        previousRecord = rcd;
                    }

                    generatedElement.InvalidateMeasure();

                    generatedElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Size desiredSize = generatedElement.DesiredSize;

                    if (isVertical)
                    {
                        #region Check if page filled based on vertical orientation

                        this._availableExtentInPreviousMeasure = availableSize.Width;

                        // JJD 10/16/08 - TFS8270
                        // Make sure we process at least one record
                        //if (isInfiniteHeight ||
                        //    sizeRequired.Height + desiredSize.Height <= scaleHeight)
                        if (isInfiniteHeight ||
                            rcdsIncluded == 0 ||
                            sizeRequired.Height + desiredSize.Height <= scaleHeight)
                        {
                            // JJD 10/16/08 - TFS8270
                            // KeepTrack of the number of records we have included so far
                            rcdsIncluded++;

                            double overallExtent = indent + desiredSize.Width;

                            this._largestRecordExtentInPreviousMeasure = Math.Max(this._largestRecordExtentInPreviousMeasure, overallExtent);

                            sizeRequired.Width = this._largestRecordExtentInPreviousMeasure;
                            sizeRequired.Height += desiredSize.Height;

                            // check for whole page scaling
                            if (this._reportSettings.HorizontalPaginationMode == HorizontalPaginationMode.Scale)
                            {
                                scaleXPrevious = availableSize.Width / (indent + generatedElement.DesiredSize.Width);
                                if ((scaleXPrevious < 1) && (scaleXPrevious < _scaleFactor))
                                {
                                    _scaleFactor = scaleXPrevious;
                                    scaleHeight = availableSize.Height / _scaleFactor;
                                }
                            }
                        }
                        else
                            break;

                        #endregion //Check if page filled based on vertical orientation
                    }
                    else
                    {
                        #region Check if page filled based on horizontal orientation

                        this._availableExtentInPreviousMeasure = availableSize.Height;

                        bool isGroupByRcd = recordPresenter is GroupByRecordPresenter;

                        double widthRequiredIfIncluded = 0;

                        if (isGroupByRcd)
                             widthRequiredIfIncluded = Math.Max(sizeRequired.Width, groupByOffset.X + indent + desiredSize.Width);
                        else
                             widthRequiredIfIncluded = Math.Max(sizeRequired.Width, groupByOffset.X + desiredSize.Width);

                        // JJD 10/16/08 - TFS8270
                        // Make sure we process at least one record
                        //if (isInfiniteWidth ||
                        //    widthRequiredIfIncluded <= scaleWidth)
                        if (isInfiniteWidth || 
                            rcdsIncluded == 0 ||
                            widthRequiredIfIncluded <= scaleWidth)
                        {
                            // JJD 10/16/08 - TFS8270
                            // KeepTrack of the number of records we have included so far
                            rcdsIncluded++;

                            double overallExtent = desiredSize.Height;

                            if (isGroupByRcd)
                            {
                                if (recordPresenter.Record.ParentRecord == null)
                                    groupByOffset.Y = 0;
                            }

                            if (isGroupByRcd)
                            {
                                groupByOffset.X += indent + desiredSize.Width;
                                groupByOffset.Y += desiredSize.Height;
                            }
                            else
                            {

                                groupByOffset.X += desiredSize.Width;
                                groupByOffset.Y += indent;
                                sizeRequired.Height = this._largestRecordExtentInPreviousMeasure;
                            }

                            this._largestRecordExtentInPreviousMeasure = Math.Max(this._largestRecordExtentInPreviousMeasure, overallExtent);
                            sizeRequired.Width = widthRequiredIfIncluded;


                            // check for whole page scaling
                            if (this._reportSettings.HorizontalPaginationMode == HorizontalPaginationMode.Scale)
                            {
                                scaleXPrevious = availableSize.Height / (indent + desiredSize.Height);
                                if ((scaleXPrevious < 1) && (scaleXPrevious < _scaleFactor))
                                {
                                    _scaleFactor = scaleXPrevious;
                                    scaleWidth = availableSize.Width / _scaleFactor;
                                }
                            }
                        }
                        else
                            break;

                        #endregion //Check if page filled based on horizontal orientation
                    }
 
                    currentItemIndex++;

                    // JJD 6/4/09 - TFS17060
                    // set the 1st record processed flag
                    wasFirstRecordProcessed = true;


                } // end while

                if (this._currentPagePosition.IndexOfLastRecordOnPage != currentItemIndex - 1)
                {
                    this._currentPagePosition.IndexOfLastRecordOnPage = Math.Max(this._currentPagePosition.IndexOfFirstRecordOnPage, currentItemIndex - 1);
                }
                // estimate total visible items
                this._totalVisibleGeneratedItems = this._currentPagePosition.IndexOfLastRecordOnPage - this._currentPagePosition.IndexOfFirstRecordOnPage;

            }

            this.EndGeneration();

            if (!isInfiniteWidth)
            {
                // JJD 5/20/10 - TFS32438 
                // only use the available extent if the calculated extent is greater or 0
                
                if (sizeRequired.Width == 0 ||
                    sizeRequired.Width > availableSize.Width)
                {
                    sizeRequired.Width = availableSize.Width;
                }
            }

            if (!isInfiniteHeight)
            {
                // JJD 5/20/10 - TFS32438 
                // only use the available extent if the calculated extent is greater or 0
                
                if (sizeRequired.Height == 0 ||
                    sizeRequired.Height > availableSize.Height)
                {
                    sizeRequired.Height = availableSize.Height;
                }
            }

            if (sizeRequired.Height == 0 && sizeRequired.Width == 0)
                return new Size(1, 1);

            return sizeRequired;
        }
        #endregion

                #region OnPropertyChanged
        /// <summary>
		/// Called when a property has changed
		/// </summary>
		/// <param name="e">Event data.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DataPresenterBase.DataPresenterProperty)
            {
                this._dataPresenter = e.NewValue as DataPresenterBase;
                this._reportSettings = (this._dataPresenter  as DataPresenterReportControl).Section.Report.ReportSettings;
            }
        }
                #endregion
 
            #endregion //Methods

            #region Properties

                #region TotalVisibleGeneratedItems

        /// <summary>
		/// Derived classes must return the number of visible generated items.
		/// </summary>
		protected override int TotalVisibleGeneratedItems
		{
			get { return this._totalVisibleGeneratedItems; }
		}

				#endregion //TotalVisibleGeneratedItems	
    
			#endregion //Properties	
    
		#endregion //Base Class Overrides

        #region Methods

            #region Private methods

                // JJD 9/3/09 - TFS21787 - added 
                #region CalculateExtraExtentNeeded

        private double CalculateExtraExtentNeeded()
        {
            bool scale = ((this._reportSettings.HorizontalPaginationMode == HorizontalPaginationMode.Scale) &&
                (_scaleFactor < 1));

            // if we are scaling to fit on a single page then we will never have any extra extent
            if (scale)
                return 0;

            return Math.Max(this._largestRecordExtentInPreviousMeasure - this._availableExtentInPreviousMeasure, 0);
        }

                #endregion //CalculateExtraExtentNeeded	
       
   
				#region FindSlotForNewlyRealizedRecordPresenter

		private int FindSlotForNewlyRealizedRecordPresenter(int newItemIndex)
		{
			// JJD 5/22/07 - Optimization
			// GridViewPanel now derives from RecyclingControlPanel 
			// so use CountOfActiveContainers instead
			//UIElementCollection internalChildren = base.InternalChildren;

			//int count = internalChildren.Count;
			// JJD 6/6/07
			// Use Children collection to only access active children
			//int activeChildrenCount = this.CountOfActiveContainers;
			// AS 7/9/07
			//IList	children = this.Children;
			IList	children = this.ChildElements;
			int		count = children.Count;

			if (count == 0)
				return 0;

            IItemContainerGenerator generator = this.ActiveItemContainerGenerator;
			for (int i = 0; i < count; i++)
			{
				int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

				if (itemIndex >= newItemIndex)
					return i;
			}

			return count;
		}

				#endregion //FindSlotForNewlyRealizedRecordPresenter	

                #region GetGeneratorPositionFromItemIndex

        private GeneratorPosition GetGeneratorPositionFromItemIndex(int itemIndex, out int childIndex)
        {
            IItemContainerGenerator generator = this.ActiveItemContainerGenerator;
            GeneratorPosition generatorPosition = (generator != null) ? generator.GeneratorPositionFromIndex(itemIndex) :
                                                                                new GeneratorPosition(-1, itemIndex + 1);

            childIndex = (generatorPosition.Offset == 0) ? generatorPosition.Index :
                                                           generatorPosition.Index + 1;

            return generatorPosition;
        }

                #endregion //GetGeneratorPositionFromItemIndex	
                
                #region EstimatePageCount

        private void EstimatePageCount()
        {
            // Estimates very approximately page count to give some progress feedback
            double temp;
            int extraHorizontalColumn;
            int rowsInPage;
            int totalRowsInGrid;

            // because the index is 0 based we must increase rowsInPage with 1
            rowsInPage = 1 + this._currentPagePosition.IndexOfLastRecordOnPage - this._currentPagePosition.IndexOfFirstRecordOnPage;
            if (rowsInPage < 1)
            {
                // the grid has incorect set of width and height
                this._pageCount = 0;
                this._isEndReached = true;
                return;
            }

            totalRowsInGrid = this._recordListControl.Items.Count;

            temp = this._largestRecordExtentInPreviousMeasure / this._availableExtentInPreviousMeasure;
            extraHorizontalColumn = (int)Math.Ceiling(temp);

            temp =  (double)totalRowsInGrid / (double)rowsInPage;
            this._pageCount = (int)Math.Ceiling(temp);

            // TK 10/15/08 TFS8263
            //if (extraHorizontalColumn > 1)
            if (extraHorizontalColumn > 1 && this._reportSettings.HorizontalPaginationMode != HorizontalPaginationMode.Scale)
            {
                this._pageCount = this._pageCount * extraHorizontalColumn;
            }
        }
                #endregion //GetGeneratorPositionFromItemIndex	

            #endregion
        
        #endregion

        #region IEmbeddedVisualPaginator Members

            #region BeginPagination

        /// <summary>
		/// Called when Pagination is about to begin.
		/// </summary>
        void IEmbeddedVisualPaginator.BeginPagination(ReportSection section)
		{
            TabularReportView view = this._dataPresenter.CurrentViewInternal as TabularReportView;
            
            // JJD 10/10/16 - TFS8345
            // Use view's LogicalOrientation which will resolve properly
            //this._gridViewOrientation = view.Orientation.HasValue ? view.Orientation.Value : Orientation.Vertical;
            this._gridViewOrientation = view.LogicalOrientation;
           
            this._currentPagePosition = new TabularPagePosition();
            this._currentPagePosition.GridViewOrientation = this._gridViewOrientation;

            this._isEndReached = false;
            this._repeatTypeHelper.Clear();
            // move in to begining
            (this as IEmbeddedVisualPaginator).MoveToPosition(PagePosition.FirstPage);
            //this.InvalidateMeasure();
            //this.UpdateLayout();
            EstimatePageCount();
		}

			#endregion //BeginPagination	
    
			#region CurrentPageDataContext

		/// <summary>
		/// Returns an piece of opaque data that will be set as the DataContext on the root visual of a <see cref="ReportBase"/>'s current page.
		/// Returns null if there is no DataContext for the current page.
		/// </summary>
		object IEmbeddedVisualPaginator.CurrentPageDataContext
		{
			get 
            {
                
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


                // Return the first record on the current page
                if (this._currentPagePosition != null)
                {
                    int index = this._currentPagePosition.IndexOfFirstRecordOnPage;

                    if (index >= 0)
                    {
                        RecordListControl rlc = this.RecordListControl;

                        if (rlc != null &&
                            rlc.Items.Count > index)
                            return rlc.Items[index];
                    }
                }

                return null; 
            }
		}

			#endregion //CurrentPageDataContext	
    
			#region CurrentPagePosition

		/// <summary>
		/// Returns a PagePosition instance that represents the current page position in the embedded visual.
		/// </summary>
		PagePosition IEmbeddedVisualPaginator.CurrentPagePosition
		{
			get 
            {
                if (this._currentPagePosition == null)
                    return PagePosition.InvalidPage;
                
                if (this._isEndReached)
                    return PagePosition.LastPage;

                return this._currentPagePosition.Clone() as PagePosition; 
            }
		}

			#endregion //CurrentPagePosition	
    
            #region LogicalPageNumber
        /// <summary>
        /// Returns integer that indicates the row of logical grid within the panel is fallen.
        /// </summary>
        int IEmbeddedVisualPaginator.LogicalPageNumber
        {
            get { return this._currentPagePosition.LogicalPageNumber; }
        }
            #endregion LogicalPageNumber

            #region LogicalPagePartNumber
        /// <summary>
        /// Returns integer that indicates the column of logical grid within the page is fallen.
        /// </summary>
        int IEmbeddedVisualPaginator.LogicalPagePartNumber
        {
            get { return this._currentPagePosition.LogicalPagePartNumber; }
        }
            #endregion LogicalPagePartNumber

            #region EstimatedPageCount
        /// <summary>
        /// Returns calculated page's count of visual that is being printed.
        /// </summary>
        int IEmbeddedVisualPaginator.EstimatedPageCount
        {
            get { return this._pageCount; }
        }

            #endregion //EstimatedPageCount

            #region EndPagination

        /// <summary>
		/// Called when pagination has ended.
		/// </summary>
		void IEmbeddedVisualPaginator.EndPagination()
		{
            this._currentPagePosition = null;
		}

			#endregion //EndPagination	
    
			#region MoveToNextPage

		/// <summary>
		/// Requests that the implementor to move to the next page and update the embedded visual's elements if necessary.
		/// </summary>
		/// <returns>true if the 'next page' is moved to, or false if the implementor could not move to the next page.</returns>
        bool IEmbeddedVisualPaginator.MoveToNextPage()
        {
            // JJD 9/3/09 - TFS21787
            // Call the CalculateExtraExtentNeeded method instead
            //double extraExtentNeeded = this._largestRecordExtentInPreviousMeasure - this._availableExtentInPreviousMeasure;
            double extraExtentNeeded = this.CalculateExtraExtentNeeded();

            this._currentPagePosition.ExtraExtentNeeded = extraExtentNeeded;
            this._currentPagePosition.TotalRowCount = this.RecordListControl.Items.Count;

            #region Scale
            if (this._reportSettings.HorizontalPaginationMode == HorizontalPaginationMode.Scale)
            {
                this._currentPagePosition.ExtraExtentNeeded = 0;
                if (this._gridViewOrientation == Orientation.Vertical)
                {
                    if (!this._currentPagePosition.MoveDown(this._availableExtentInPreviousMeasure))
                        return false;
                }
                else
                {
                    if (!this._currentPagePosition.MoveLeft(this._availableExtentInPreviousMeasure))
                        return false;
                }
            }
            #endregion

            #region Horizontal print order
            else if (this._reportSettings.PagePrintOrder == PagePrintOrder.Horizontal)
            {
                if (!this._currentPagePosition.MoveLeft(this._availableExtentInPreviousMeasure))
                {
                    if (!this._currentPagePosition.MoveDownGoToStartOfRow(this._availableExtentInPreviousMeasure))
                        return false;
                }
                else
                {
                    // optimization. we dont need to generate record again, we only need to arrange them with offset
                    // of avaible size
                    if (this._gridViewOrientation == Orientation.Vertical)
                    {
                        //this.InvalidateMeasure();
                        this.InvalidateArrange();
                        this.UpdateLayout();
                        // check for end
                        if (this._currentPagePosition.IsEndReached)
                            this._isEndReached = true;

                        return true;
                    }
                }
            }
            #endregion //Horizontal print order

            #region Vertical print order
            else if (this._reportSettings.PagePrintOrder == PagePrintOrder.Vertical)
            {
                if (!this._currentPagePosition.MoveDown(this._availableExtentInPreviousMeasure))
                {
                    if (!this._currentPagePosition.MoveLeftGoToTop(this._availableExtentInPreviousMeasure))
                        return false;
                }
            }
            #endregion // Vertical print order
            else
            {
                throw new Exception("Invalid PagePrintOrder value!");
            }

            //GenerateVisual(this._availableSize);
            this.InvalidateMeasure();
            //this.InvalidateArrange();

            this.UpdateLayout();

            // estimate actual information for extend and give it to current position
            
            // JJD 9/3/09 - TFS21787
            // Call the CalculateExtraExtentNeeded method instead
            //double extraExtentNeeded = this._largestRecordExtentInPreviousMeasure - this._availableExtentInPreviousMeasure;
            extraExtentNeeded = this.CalculateExtraExtentNeeded();
            this._currentPagePosition.ExtraExtentNeeded = extraExtentNeeded;

            if (this._currentPagePosition.IsEndReached)
                this._isEndReached = true;

            return true;
        }

			#endregion //MoveToNextPage	
    
			#region MoveToPosition

		/// <summary>
		/// Requests that the implementor to move to a specific page represented by the supplied <see cref="PagePosition"/> and update the embedded visual's elements if necessary.
		/// </summary>
		/// <param name="pagePosition">A <see cref="PagePosition"/> instance that represents the page to move to.</param>
		/// <returns>True if the implementor was able to move to the requested page, or false if the move failed.</returns>
		bool IEmbeddedVisualPaginator.MoveToPosition(PagePosition pagePosition)
		{
            TabularPagePosition pp = pagePosition as TabularPagePosition;
            this._isEndReached = false;

            if (pp == null)
            {
                if (pagePosition == PagePosition.FirstPage)
                {
                    // chech if we aren't already there
                    if (this._currentPagePosition.IsOnFirstPosition())
                    {

                        if ((this._scaleFactor * this._largestRecordExtentInPreviousMeasure) - this._availableExtentInPreviousMeasure > 0)
                        {
                            this._isEndReached = false;
                            return true;
                        }

                        if (this._currentPagePosition.IsEndReached)
                            this._isEndReached = true;
                        
                        return true;
                    }

                    this._currentPagePosition.MoveToFirstPage();
                }
                else if (pagePosition == PagePosition.LastPage)
                {
                    return ((IEmbeddedVisualPaginator)this).MoveToNextPage();
                }
                else
                    throw new ArgumentException(DataPresenterBase.GetString("LE_InvalidMoveToPagePosition", pagePosition));
            }
            else
            {
                // get copy for current position
                this._currentPagePosition = pp.Clone() as TabularPagePosition;
            }

            // JJD 9/3/09 - TFS21787
            // Call the CalculateExtraExtentNeeded method instead
            //double extraExtentNeeded = this._largestRecordExtentInPreviousMeasure - this._availableExtentInPreviousMeasure;
            double extraExtentNeeded = this.CalculateExtraExtentNeeded();
            this._currentPagePosition.ExtraExtentNeeded = extraExtentNeeded;
            this._currentPagePosition.TotalRowCount = this.RecordListControl.Items.Count;

            //GenerateVisual(this._availableSize);
            this.InvalidateMeasure();
            //this.InvalidateArrange();

            this.UpdateLayout();

            //this._currentPagePosition = pp;
            if (this._currentPagePosition.IsEndReached)
                this._isEndReached = true;

            return true;
		}

			#endregion //MoveToPosition	
    
		#endregion //IEmbeddedVisualPaginator Members

        #region IScrollInfo Members

        bool IScrollInfo.CanHorizontallyScroll
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        bool IScrollInfo.CanVerticallyScroll
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        double IScrollInfo.ExtentHeight
        {
            get { return 1000000; }
        }

        double IScrollInfo.ExtentWidth
        {
            get { return 1000000; }
        }

        double IScrollInfo.HorizontalOffset
        {
			// JJD 3/23/11 - TFS65143
			// Return the actual offset
			get { return this._currentPagePosition != null ? this._currentPagePosition.HorizontalOffset : 0; }
        }

        void IScrollInfo.LineDown()
        {
            //throw new NotImplementedException();
        }

        void IScrollInfo.LineLeft()
        {
            //throw new NotImplementedException();
        }

        void IScrollInfo.LineRight()
        {
            //throw new NotImplementedException();
        }

        void IScrollInfo.LineUp()
        {
            //throw new NotImplementedException();
        }

        Rect IScrollInfo.MakeVisible(System.Windows.Media.Visual visual, Rect rectangle)
        {
            return new Rect();
        }

        void IScrollInfo.MouseWheelDown()
        {
            return;
        }

        void IScrollInfo.MouseWheelLeft()
        {
            //throw new NotImplementedException();
        }

        void IScrollInfo.MouseWheelRight()
        {
            //throw new NotImplementedException();
        }

        void IScrollInfo.MouseWheelUp()
        {
            //throw new NotImplementedException();
            return;
        }

        void IScrollInfo.PageDown()
        {
            //throw new NotImplementedException();
        }

        void IScrollInfo.PageLeft()
        {
            //throw new NotImplementedException();
        }

        void IScrollInfo.PageRight()
        {
            //throw new NotImplementedException();
        }

        void IScrollInfo.PageUp()
        {
            //throw new NotImplementedException();
        }

        private ScrollViewer _scrollOwner;

        ScrollViewer IScrollInfo.ScrollOwner
        {
            get
            {
                return this._scrollOwner;
            }
            set
            {
               this._scrollOwner = value;
            }
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
        }

        void IScrollInfo.SetVerticalOffset(double offset)
        {
        }

        double IScrollInfo.VerticalOffset
        {
			// JJD 3/23/11 - TFS65143
			// Return the actual offset
			get { return this._currentPagePosition != null ? this._currentPagePosition.VerticalOffset : 0; }
		}

        double IScrollInfo.ViewportHeight
        {
            get { return this.ActualHeight; }
        }

        double IScrollInfo.ViewportWidth
        {
            get { return this.ActualWidth; }
        }

        #endregion

        #region TabularPagePosition private class

        private class TabularPagePosition : PagePosition, ICloneable
        {
            #region Member variables
            /// <summary>
            /// I use this as a stek (FIFO). I fill the array with index of first row on page, so when the page does not fit horizontally 
            /// in paper size a store index in that array. when i use this index i delete it.
            /// </summary>
            private List<PositionData> _horizontalOptimization = new List<PositionData>();
            private int _logicalRow;
            private int _logicalColumn;

            private int _indexOfFirstRecordOnPage;
            private int _indexOfLastRecordOnPage;
            private double _horizontalOffset;
            private double _verticalOffset;
            private double _extraExtentNeeded;
            private int _totalRowCount;
            private Orientation _gridViewOrientation;
            private Size _availableSize;

            private bool _isLeftThenDown = false;
            private Stack<Vector> _extraOffsetStack;
            #endregion //Member variables

            #region Constructors
            internal TabularPagePosition() : this(0, 0, 0, 0) { }

            internal TabularPagePosition(int indexOfFirstRecordOnPage) : this(indexOfFirstRecordOnPage, 0, 0, 0) { }

            internal TabularPagePosition(int indexOfFirstRecordOnPage, int indexOfLastRecordOnPage, double horizontalOffset, double verticalOffset)
            {
                this._indexOfFirstRecordOnPage = indexOfFirstRecordOnPage;
                this._indexOfLastRecordOnPage = indexOfLastRecordOnPage;
                this._horizontalOffset = horizontalOffset;
                this._verticalOffset = verticalOffset;
                this._extraExtentNeeded = 0;
                this._totalRowCount = 0;
                this._logicalRow = 1;
                this._logicalColumn = 1;
            }
            #endregion // Constructors

            #region Properties
            internal bool IsEndReached
            {
                get
                {
                    if (_isLeftThenDown)
                    {
                        if (this._totalRowCount <= this._indexOfLastRecordOnPage + 1)
                        {
                            if (this._gridViewOrientation == Orientation.Vertical)
                            {
                                if (this._extraExtentNeeded <= this._horizontalOffset)
                                    return true;
                            }
                            else
                            {
                                if (this._extraExtentNeeded <= this._verticalOffset)
                                    return true;
                            }
                        }
                        return false;
                    }


                    double offset = this._gridViewOrientation == Orientation.Vertical ? this._horizontalOffset :
                        this._verticalOffset;

                    // JJD 10/16/08 - TFS8345
                    // Never return true if the extra extent requires more pages
                    if (this._extraExtentNeeded <= offset)
                    {
                        if ((offset == 0) &&
                            (this._totalRowCount <= this._indexOfLastRecordOnPage + 1) &&
                            (this._horizontalOptimization.Count == 0))
                            return true;
                        else if ((offset != 0) &&
                                 (this._horizontalOptimization.Count == 0))
                            return true;
                    }


                    return false;
                }
            }

            internal Size AvailableSize
            {
                get { return this._availableSize; }
                set { this._availableSize = value; } 
            }

            internal Stack<Vector> ExtraOffsetStack
            {
                get
                {
                    if (this._extraOffsetStack == null)
                        this._extraOffsetStack = new Stack<Vector>();

                    return this._extraOffsetStack;
                }
            }

            internal Orientation GridViewOrientation
            {
                get { return this._gridViewOrientation; } 
                set { this._gridViewOrientation = value; } 
            }
            internal int LogicalPageNumber 
            {
                get
                {
                    return this._logicalRow;
                }
            }
            internal int LogicalPagePartNumber 
            {
                get
                {
                        return this._logicalColumn;
                }
            }
            internal int IndexOfFirstRecordOnPage 
            { 
                get { return this._indexOfFirstRecordOnPage; } 
            }
            internal int IndexOfLastRecordOnPage 
            { 
                get { return this._indexOfLastRecordOnPage; }
                set { this._indexOfLastRecordOnPage = value; }
            }
            internal double HorizontalOffset 
            { 
                get { return this._horizontalOffset; }
           }
            internal double VerticalOffset 
            { 
                get { return this._verticalOffset; }
            }
            internal double ExtraExtentNeeded 
            {
                get { return this._extraExtentNeeded; }
                set { this._extraExtentNeeded = value; }
            }

            internal int TotalRowCount 
            { 
                get { return this._totalRowCount; }
                set { this._totalRowCount = value; }
            }
            #endregion

            #region Base class overrides

                #region Equals

            public override bool Equals(object obj)
            {
                TabularPagePosition pos = obj as TabularPagePosition;
                if (pos == null)
                    return false;
                if (pos.IndexOfFirstRecordOnPage == this.IndexOfFirstRecordOnPage &&
                    pos.IndexOfLastRecordOnPage == this.IndexOfLastRecordOnPage &&
                    pos.HorizontalOffset == this.HorizontalOffset &&
                    pos.VerticalOffset == this.VerticalOffset)
                {
                    return true;
                }

                return false;
            }

                #endregion //Equals	

            #region GetHashCode

            public override int GetHashCode()
            {
                int hashcode = (this._indexOfFirstRecordOnPage.GetHashCode() / 4) +
                                (this._indexOfLastRecordOnPage.GetHashCode() / 4) +
                                (this._verticalOffset.GetHashCode() / 4) +
                                (this._horizontalOffset.GetHashCode() / 4);
                return hashcode;
            }

            #endregion //GetHashCode	
        
            #endregion //Base class overrides	
    
            #region Methods
            internal bool MoveLeft(double offset)
            {
                this._isLeftThenDown = true;
                
                #region Vertical grid orientation
                if (this._gridViewOrientation == Orientation.Vertical)
                {
                    if (this._extraExtentNeeded <= this._horizontalOffset)
                        return false;

                    this._horizontalOffset += offset;
                    //this._extraExtentNeeded = 0;
                }
                #endregion

                #region Horizontal grid orientation

                else
                {
                    // mark the horizontal page as printable. check before all other to be sure you get the last page
                    if (this._extraExtentNeeded > this._verticalOffset)
                        _horizontalOptimization.Add(new PositionData(this._indexOfFirstRecordOnPage, this._logicalColumn));

                    // move down
                    if (this._totalRowCount <= this._indexOfLastRecordOnPage + 1)
                        return false;

                    if (this._verticalOffset == 0)
                    {
                        this._indexOfFirstRecordOnPage = this._indexOfLastRecordOnPage + 1;
                        this._indexOfLastRecordOnPage = -1;
                    }
                    else
                    {
                        if (_horizontalOptimization.Count == 0)
                            return false;

                        this._indexOfFirstRecordOnPage = _horizontalOptimization[0].FirstRowOnPage + 1;
                        this._indexOfLastRecordOnPage = -1;
                        this._logicalColumn = _horizontalOptimization[0].LogicalRow;
                        _horizontalOptimization.RemoveAt(0);
                        return true;
                    }
                }
                #endregion

                this._logicalColumn++;
                return true;
            }

            internal bool MoveLeftGoToTop(double offset)
            {
                #region Vertical grid orientation

                if (this._gridViewOrientation == Orientation.Vertical)
                {
                    //if (this._extraExtentNeeded <= this._horizontalOffset)
                    //    return false;


                    //// move to first row with horizontal offset
                    //this._indexOfFirstRecordOnPage = 0;
                    //this._indexOfLastRecordOnPage = 0;
                    //this._horizontalOffset += offset;
                    //this._verticalOffset = 0;
                    ////this._extraExtentNeeded = 0;

                    if (_horizontalOptimization.Count == 0)
                        return false;
                    // move to first row with horizontal offset

                    // JJD 10/16/08 - TFS8345
                    // We don't want to add 1 here
                    //this._indexOfFirstRecordOnPage = _horizontalOptimization[0].FirstRowOnPage + 1;
                    this._indexOfFirstRecordOnPage = _horizontalOptimization[0].FirstRowOnPage;

					// JJD 3/15/11 - TFS65143
					// Set index of last record to -1
					//this._indexOfLastRecordOnPage = 0;
                    this._indexOfLastRecordOnPage = -1;

                    this._logicalRow = _horizontalOptimization[0].LogicalRow;
                    
                    this._horizontalOffset += offset;
                    this._verticalOffset = 0;

                    _horizontalOptimization.RemoveAt(0);

                }
                #endregion

                #region Horizontal grid orientation

                else
                {
                    if (this._totalRowCount <= this._indexOfLastRecordOnPage + 1)
                        return false;

                    this._verticalOffset = 0;
                    this._indexOfFirstRecordOnPage = this._indexOfLastRecordOnPage + 1;
                    this._indexOfLastRecordOnPage = -1;
                
					// JJD 3/15/11 - TFS65143
					//Moved from below
					
					this._logicalRow = 1;
                }
                #endregion

				// JJD 3/15/11 - TFS65143
				//Moved above into else block so we don't step on logical row setting
                //this._logicalRow = 1;
                this._logicalColumn++;
                return true;
            }

            internal void MoveToFirstPage()
            {
                this._indexOfFirstRecordOnPage = 0;
                this._indexOfLastRecordOnPage = -1;
                this._verticalOffset = 0;
                this._horizontalOffset = 0;
                this._extraExtentNeeded = 0;
                this._logicalRow = 1;
                this._logicalColumn = 1;
            }

            internal bool MoveDown(double offset)
            {

                #region Vertical grid orientation
                if (this._gridViewOrientation == Orientation.Vertical)
                {
                    // check if extra extend needed, so store the info about it
                    if (this._extraExtentNeeded > this._horizontalOffset)
                        _horizontalOptimization.Add(new PositionData(this._indexOfFirstRecordOnPage, this._logicalRow));

                    if (this._totalRowCount <= this._indexOfLastRecordOnPage + 1)
                        return false;


                    if (this._horizontalOffset == 0)
                    {
                        this._indexOfFirstRecordOnPage = this._indexOfLastRecordOnPage + 1;
                        this._indexOfLastRecordOnPage = -1;
                    }
                    else
                    {
                        if (_horizontalOptimization.Count == 0)
                            return false;

						// JJD 3/15/11 - TFS65143
						// Shouldn't add 1 to the first age in vertical orienattion
                        //this._indexOfFirstRecordOnPage = _horizontalOptimization[0].FirstRowOnPage + 1;
                        this._indexOfFirstRecordOnPage = _horizontalOptimization[0].FirstRowOnPage;
                        this._indexOfLastRecordOnPage = -1;
                        this._logicalRow = _horizontalOptimization[0].LogicalRow;
                        _horizontalOptimization.RemoveAt(0);
                        return true;
                    }

                }
                #endregion

                #region Horizontal grid orientation
                else
                {
                    if (this._extraExtentNeeded <= this._verticalOffset)
                        return false;

                    this._verticalOffset += offset;
                    //this._extraExtentNeeded = 0;
                }
                #endregion

                this._logicalRow++;
                return true;
            }

            internal bool MoveDownGoToStartOfRow(double offset)
            {
                #region Vertical grid orientation

                if (this._gridViewOrientation == Orientation.Vertical)
                {
                    if (this._totalRowCount <= this._indexOfLastRecordOnPage + 1)
                        return false;

                    this._horizontalOffset = 0;
                    this._indexOfFirstRecordOnPage = this._indexOfLastRecordOnPage + 1;
                    this._indexOfLastRecordOnPage = -1;

                }
                #endregion

                #region Horizontal grid orientation

                else
                {
                    //if (this._extraExtentNeeded <= this._verticalOffset)
                    //    return false;

                    //// move to first row with horizontal offset
                    //this._indexOfFirstRecordOnPage = 0;
                    //this._indexOfLastRecordOnPage = 0;
                    //this._horizontalOffset = 0;
                    //this._verticalOffset += offset;
                    //this._extraExtentNeeded = 0;

                    if (_horizontalOptimization.Count == 0)
                        return false;
                    // move to first row with horizontal offset
                    // JJD 10/16/08 - TFS8345
                    // We don't want to add 1 here
                    //this._indexOfFirstRecordOnPage = _horizontalOptimization[0].FirstRowOnPage + 1;
                    this._indexOfFirstRecordOnPage = _horizontalOptimization[0].FirstRowOnPage;
                    this._indexOfLastRecordOnPage = 0;
                    this._logicalColumn = _horizontalOptimization[0].LogicalRow;

                    this._horizontalOffset = 0;
                    this._verticalOffset += offset;

                    _horizontalOptimization.RemoveAt(0);

                }
                #endregion 
                this._logicalRow++;
                this._logicalColumn = 1;
                return true;
            }

            internal bool IsOnFirstPosition()
            {
                if (this._indexOfFirstRecordOnPage == 0 &&
                this._indexOfLastRecordOnPage != 0 &&
                this._verticalOffset == 0 &&
                this._horizontalOffset == 0)
                    return true;

                return false;
            }

            #endregion

            #region ICloneable Members

            public object Clone()
            {
                TabularPagePosition pos = new TabularPagePosition(this._indexOfFirstRecordOnPage, this._indexOfLastRecordOnPage,
                    this._horizontalOffset, this._verticalOffset);

                pos.GridViewOrientation = this.GridViewOrientation;
                pos._logicalColumn = this._logicalColumn;
                pos._logicalRow = this._logicalRow;
                pos._extraExtentNeeded = this._extraExtentNeeded;
                // Clone horizontal optimization list
                pos._horizontalOptimization = new List<PositionData>(this._horizontalOptimization);

                // JJD 9/26/09
                // Clone the extra space stack
                if (this._extraOffsetStack != null &&
                     this._extraOffsetStack.Count > 0)
                {
                    Vector[] vectors = this._extraOffsetStack.ToArray();
                    Array.Reverse(vectors, 0, vectors.Length);
                    pos._extraOffsetStack = new Stack<Vector>(vectors);
                }

                return pos;
            }

            #endregion

        }

        #endregion //TabularPagePosition private class

        #region PositionData private class
        private class PositionData
        {
            public int FirstRowOnPage;
            public int LogicalRow;

            public PositionData(int indexRow, int logicalRow)
            {
                FirstRowOnPage = indexRow;
                LogicalRow = logicalRow;
            }
        }
        #endregion //PositionData private class

        // JJD 3/25/09 - TFS15888 - added
        #region FieldLayoutNestingLevelKey

        /// <summary>
        /// Key that contains both fieldlayout and nesting depth to keep track of when headers are needed
        /// </summary>
        private class FieldLayoutNestingLevelKey
        {
            #region Private members

            private FieldLayout _fieldLayout;
            private int _nestingLevel;

            #endregion //Private members

            #region Constructors

            internal FieldLayoutNestingLevelKey(FieldLayout fieldLayout, int nestingLevel)
            {
                this._fieldLayout = fieldLayout;
                this._nestingLevel = nestingLevel;
            }

            #endregion //Constructors

            #region Base class overrides

            public override bool Equals(object obj)
            {
                FieldLayoutNestingLevelKey key = obj as FieldLayoutNestingLevelKey;

                if (key == null)
                    return false;

                return key._nestingLevel == this._nestingLevel &&
                        key._fieldLayout == this._fieldLayout;
            }

            public override int GetHashCode()
            {
                return (this._fieldLayout.GetHashCode() / 10) + (this._nestingLevel.GetHashCode() / 10);
            }

            #endregion //Base class overrides

            #region Methods

            internal bool IsSameKey(FieldLayout fl, int nestingDepth)
            {
                return nestingDepth == this._nestingLevel &&
                        fl == this._fieldLayout;
            }

            #endregion //Methods
        }

        #endregion //FieldLayoutNestingLevelKey

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