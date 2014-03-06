using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A custom Panel that Virtualizes the rows of the <see cref="XamGrid"/>.
	/// </summary>
    public class RowsPanel : Panel, IRecyclableElementHost
	{
		#region Members

		XamGrid _grid;
		List<RowBase> _topRows;
		List<RowBase> _bottomRows;		
		Collection<RowBase> _visibleRows;
		Collection<RowBase> _nonReleasedRows;
		Collection<FixedRowSeparator> _topFixRowSeperators, _bottomFixRowSeperators;
		double _overrideVerticalMax, _overflowAdjustment, _invalidateRowHeight, _measureScrollBarValue;
		List<RowsManagerBase> _visibleManagers;
		bool _reverseMeasure;
		int _reverseRowStartIndex;
        bool _measureCalled, _measureCalledInfinite, _recalcHorizSBVis, _onNextMeasureReleaseVisibleRows, _ensuerVertSBValueUpdated;
		CellBase _scrollIntoViewCell;
		EmptyDelegate _scrollCellIntoViewCallback;
        private EmptyDelegate _invalidateMeasureCallback;
		List<CellBase> _reMeasuredCells;
		List<CellBase> _previouslyRemeasuredCells;
		FixedRowsOrderComparer _fixedRowsOrderComparer;
		double _previousHeight,_previousWidth;
		RectangleGeometry _clipRG;
		Rect _hiddenRowRect = new Rect(-1000, -1000, 0, 0);
        List<FrameworkElement> _notVisibleAndNotArrangedRows;
        bool _unloadedSoResetAll;
        bool _resetHorizSBViewportSize = false, _resetVertSBViewportSize = false;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="RowsPanel"/> class.
		/// </summary>
        public RowsPanel()
        {
            this._clipRG = new RectangleGeometry();
            this.Clip = this._clipRG;
            this._topRows = new List<RowBase>();
            this._bottomRows = new List<RowBase>();
            this._visibleRows = new Collection<RowBase>();
            this._overrideVerticalMax = -1;
            this._visibleManagers = new List<RowsManagerBase>();
            this._reMeasuredCells = new List<CellBase>();
            this._previouslyRemeasuredCells = new List<CellBase>();
            this._fixedRowsOrderComparer = new FixedRowsOrderComparer();
            this._topFixRowSeperators = new Collection<FixedRowSeparator>();
            this._bottomFixRowSeperators = new Collection<FixedRowSeparator>();
            this._nonReleasedRows = new Collection<RowBase>();
            this._notVisibleAndNotArrangedRows = new List<FrameworkElement>();

            this.Unloaded += new RoutedEventHandler(RowsPanel_Unloaded);


            this.SetCurrentValue(UIElement.IsManipulationEnabledProperty, true);

        }

		#endregion // Constructor

		#region Properties

		#region Public

		#region Grid

		/// <summary>
		/// Gets a reference to the <see cref="XamGrid"/> that owns the panel.
		/// </summary>
		public XamGrid Grid
		{
			get { return this._grid; }
			internal set
			{
				this._grid = value;
				this.ScrollInfo = this._grid as IProvideScrollInfo;
			}
		}
		#endregion // Grid

		#region VisibleRows

		/// <summary>
		/// Gets the rows that are currently visible in the Viewport.
		/// </summary>
		public Collection<RowBase> VisibleRows
		{
			get { return this._visibleRows; }
		}

		#endregion // VisibleRows

        #region VisibleManagers

        /// <summary>
        /// Gets the list of currently visible RowsManagers
        /// </summary>
        public List<RowsManagerBase> VisibleManagers
        {
            get { return this._visibleManagers; }
        }

        #endregion // VisibleManagers

        #region FixedRowsBottom

        /// <summary>
		/// Gets the rows that are currently fixed to the bottom of the Viewport.
		/// </summary>
		public List<RowBase> FixedRowsBottom
		{
			get { return this._bottomRows; }
		}

		#endregion // FixedRowsBottom

		#region FixedRowsTop

		/// <summary>
		/// Gets the rows that are currently fixed to the top of the Viewport.
		/// </summary>
		public List<RowBase> FixedRowsTop
		{
			get { return this._topRows; }
		}

		#endregion // FixedRowsBottom

		#region CustomFilterDialogControl

		/// <summary>
		/// Gets / sets the <see cref="CustomFilterDialogControl"/> which will be used to allow for custom UI based filtering.
		/// </summary>
		public ColumnFilterDialogControl CustomFilterDialogControl
		{
			get;
			set;
		}

		#endregion // CustomFilterDialogControl

        #region CompoundFilterDialogControl 
        public CompoundFilterDialogControl CompoundFilterDialogControl
        {
            get;
            set;
        }
        #endregion // CompoundFilterDialogControl

        #endregion // Public

        #region Protected

        #region ScrollInfo

        /// <summary>
		/// A reference to the ScrollInfo object that relates to the <see cref="RowsPanel"/>
		/// </summary>
		protected IProvideScrollInfo ScrollInfo
		{
			get;
			private set;
		}
		#endregion // Protected	

		#endregion // Private

		#region Internal

		internal bool InLayoutPhase
		{
			get;
			set;
		}

		internal bool ScrollCellIntoViewInProgress
		{
			get;
			private set;
		}

		internal int ReverCellStartIndex
		{
			get;
			set;
		}

        internal Column ReverseChildColumn
        {
            get;
            set;
        }

		internal bool ReverseCellMeasure
		{
			get;
			set;
		}

		internal ColumnLayout ReverseColumnLayout
		{
			get;
			set;
		}

		internal List<CellBase> RemeasuredCells
		{
			get
			{
				return this._reMeasuredCells;
			}
		}

		internal List<CellBase> PreviouslyRemeasuredCells
		{
			get
			{
				return this._previouslyRemeasuredCells;
			}
		}

		internal RowsManagerBase HorizontalRowsManager
		{
			get;
			set;
		}

		internal bool MeasureCalled
		{
			get { return this._measureCalled; }
			set{this._measureCalled = value;}
		}

		#endregion // Internal

		#endregion // Properties

		#region Methods

		#region Public

		#region RegisterFixedRow
		/// <summary>
		/// Registers a Fixed Row with the panel, so that the row is displayed.
		/// </summary>
		/// <param name="row">The row that should be displayed.</param>
		/// <param name="alignment">Whether the info should be aligned to the Top or the Bottom of the XamGrid.</param>
		public void RegisterFixedRow(RowBase row, FixedRowAlignment alignment)
		{
			if (row != null)
			{
				List<RowBase> rows = (alignment == FixedRowAlignment.Top) ? this._topRows : this._bottomRows;

				if (!rows.Contains(row))
				{
					rows.Add(row);						
					rows.Sort(this._fixedRowsOrderComparer);
				}
			}
		}
		#endregion // RegisterFixedRow

		#region UnregisterFixedRow
		/// <summary>
		/// Unregisters the specified fixed row from the panel, so that it no longer gets displayed.
		/// </summary>
		/// <param name="row">The row that should no longer be displayed.</param>
		public void UnregisterFixedRow(RowBase row)
		{
			if (this._topRows.Contains(row))
				this._topRows.Remove(row);
			else if (this._bottomRows.Contains(row))
				this._bottomRows.Remove(row);

			if (!this._visibleRows.Contains(row))
				this.ReleaseRow(row);
		}
		#endregion // UnregisterFixedRow

		#region ResetCachedScrollInfo

		/// <summary>
		/// Resets any information that is being cached to stop the scrollbar from jumping around while scrolling. 
		/// Reasons for reseting can be the addition/removal of new rows, or the expansion/collapsing of child rows. 
		/// </summary>
		public void ResetCachedScrollInfo(bool resetVisibleRows)
		{
            if (!this.MeasureCalled)
            {
                this._overflowAdjustment = 0;
                this._overrideVerticalMax = -1;
                this._recalcHorizSBVis = true;

                foreach (RowsManagerBase manager in this._visibleManagers)
                    manager.OverrideHorizontalMax = -1;

                if (resetVisibleRows)
                    this._onNextMeasureReleaseVisibleRows = true;
            }
		}

		#endregion // ResetCachedScrollInfo

		#region ResetRows

		/// <summary>
		/// Releases all Rows from the VisualTree. 
		/// </summary>
		public void ResetRows()
		{
			this.ResetRows(false);
		}

		/// <summary>
		/// Releases all Rows from the VisualTree. 
		/// </summary>
		/// <param name="releaseAll">True if the CellsPanels should be released by the RecyclingManager.</param>
		public void ResetRows(bool releaseAll)
		{
            


		    if (this._grid != null && this.InLayoutPhase)
		    {
		        this.Dispatcher.BeginInvoke(
		            new Action(
		                () =>
		                    {
		                        this.ResetRows(releaseAll);
		                    }));
		        return;
		    }

            this._measureCalled = false;

			foreach (RowBase row in this._topRows)
			{
                if(row.Control != null)
				    this.ReleaseRow(row);
			}

			foreach (RowBase row in this._bottomRows)
			{
                if (row.Control != null)
				    this.ReleaseRow(row);
			}

			foreach (RowBase row in this._visibleRows)
			{
				this.ReleaseRow(row);
			}

			if (releaseAll)
			{
				RecyclingManager.Manager.ReleaseAll(this);
			}

            foreach (XamGridRenderAdorner adorner in this.Grid.RenderAdorners)
                adorner.Reset();

			this._overrideVerticalMax = -1;
			this._visibleRows.Clear();

		}

		#endregion // ResetRows
        		
		#region ScrollCellIntoView

		/// <summary>
		/// Scrolls the specified cell into the Viewport. 
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="alignment">Whether the cell should be aligned to the top or bottom of the panel.</param>
		/// <param name="callback">The function that should be called after the panel updates it's layout.</param>
		/// <remarks>If alignTop and alignBottom are both set, alignTop will win.</remarks>
		internal void ScrollCellIntoView(CellBase cell, CellAlignment alignment, EmptyDelegate callback)
		{
			bool horizontalScrollNeeded = true;
			Column col = cell.Column;
            Column parentCol = col;
			while (col != null)
            {
               // As with Fixed columns, merged columns are also fixed, so if we're scrolling a merged cell or fixed cel into view
                // there is no need to scroll horizontally.
                bool isMerged = col.IsGroupBy && this.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells;
                if(col.IsFixed != FixedState.NotFixed || isMerged)
				    horizontalScrollNeeded = false;

                parentCol = col;
                col = col.ParentColumn;
            }

            col = parentCol;

			this._scrollCellIntoViewCallback = callback; 

			// Make sure that all of the cell's parent rows are expanded, so that it can be scrolled into view. 
			if (cell.Row.Manager.ParentRow != null)
			{
				ExpandableRowBase expandableRow = cell.Row.Manager.ParentRow as ExpandableRowBase;
				while (expandableRow != null)
				{
					if (!expandableRow.IsExpanded)
					{
						ChildBand cb = expandableRow as ChildBand;
						if(cb == null || cb.ResolveIsVisible)
							expandableRow.IsExpanded = true;
					}
					expandableRow = expandableRow.Manager.ParentRow as ExpandableRowBase;
				}
			}

			int index = this.Grid.InternalRows.IndexOf(cell.Row);

			// Is the row that this cell is a child of already in view?
			if (cell.Row.Control != null)
			{
				Rect panelLayout = LayoutInformation.GetLayoutSlot(this);
				Rect rowLayout = LayoutInformation.GetLayoutSlot(cell.Row.Control);

				if (alignment == CellAlignment.Top)
				{
					this.ScrollInfo.VerticalScrollBar.Value = index;
				}
				else if (alignment == CellAlignment.Bottom)
				{
					this._reverseRowStartIndex = index;
					this._reverseMeasure = true;
				}
				else
				{
					
					if (cell.Row.Manager.Level != 0 ||(this.FixedRowsBottom.IndexOf(cell.Row) == -1 && this.FixedRowsTop.IndexOf(cell.Row) == -1))
					{
						double bottomHeight = 0;
						foreach (RowBase r in this._bottomRows)
						{
							if (r.Control != null)
								bottomHeight += LayoutInformation.GetLayoutSlot(r.Control).Height;
						}

						double topHeight = 0;
						foreach (RowBase r in this._topRows)
						{
							if (r.Control != null)
								topHeight += LayoutInformation.GetLayoutSlot(r.Control).Height;
						}

						if ((rowLayout.Height + rowLayout.Top + bottomHeight) > panelLayout.Height)
						{
							// If the row is at the bottom and not fully in view, then we need to reverse load the rows.
							this._reverseRowStartIndex = index;
							this._reverseMeasure = true;
						}
						else if (rowLayout.Top - topHeight <= 0)
						{
							// If the row is at the very top, and not fully in view, then simply set the scroll value to it's index
							// so that it scrolls to the top of that row.
							this.ScrollInfo.VerticalScrollBar.Value = index;
						}
						
					}
				}

                // The Row is in view, lets see if the cell is in view.
				if (horizontalScrollNeeded)
				{
					CellsPanel rowControl = cell.Row.Control;

                    // Make that cell the cell we scroll into view on.
                    CellBase rootCell = cell.Row.Cells[col];

                    ReadOnlyCollection<Column> visColumns = cell.Row.Columns.AllVisibleColumns;

                    int cellIndex = visColumns.IndexOf(col);
					if (cellIndex == -1)
						cellIndex = 0;

					// If the cell is in view. 
					if (rootCell.Control != null)
					{
                        double leftWidth = 0;
                        foreach (CellBase c in rowControl.VisibleFixedLeftCells)
                            leftWidth += LayoutInformation.GetLayoutSlot(c.Control).Width;

                        // Need to take into account indentation as well. 
                        leftWidth += rootCell.Row.Manager.ResolveIndentation(rootCell.Row);

                        if (cell.Column.ParentColumn == null)
                        {
                            Rect cellLayout = LayoutInformation.GetLayoutSlot(rootCell.Control);

                            if ((cellLayout.Width + cellLayout.Left) > panelLayout.Width)
                            {
                                // If the Cell is at the far right edge, and not fully in view, lets reverse render the cells
                                this.ReverCellStartIndex = cellIndex;
                                this.ReverseCellMeasure = true;                                
                                this.ReverseColumnLayout = cell.Row.ColumnLayout;
                            }
                            else if (cellLayout.Left - leftWidth < 0)
                            {
                                // If the cell is a the very right and not fully in view, then we need to 
                                rowControl.SetScrollLeft(cellIndex);
                            }
                        }
                        else
                        {
                            Rect cellLayout = LayoutInformation.GetLayoutSlot(cell.Control);
                            Point cellPoint = cell.Control.TransformToVisual(this).Transform(new Point(0, 0));

                            Point childCellToRootCell = cell.Control.TransformToVisual(rootCell.Control).Transform(new Point(0, 0));
                            Rect rootCellLayout = LayoutInformation.GetLayoutSlot(rootCell.Control);

                            if ((cellLayout.Width + cellPoint.X) > panelLayout.Width)
                            {
                                if (cellLayout.Width <= panelLayout.Width)
                                {
                                    // If the Cell is at the far right edge, and not fully in view, lets reverse render the cells
                                    this.ReverCellStartIndex = cellIndex;
                                    this.ReverseCellMeasure = true;
                                    this.ReverseChildColumn = cell.Column;
                                    this.ReverseColumnLayout = cell.Row.ColumnLayout;
                                }
                            }
                            else if (cellPoint.X - leftWidth < 0 && rootCellLayout.Width > 0)
                            {
                                double percent = childCellToRootCell.X / rootCellLayout.Width;
                                rowControl.SetScrollLeft(cellIndex + percent);
                            }
                            // TFS111823
                            // else
                            // {
                            //     rowControl.SetScrollLeft(cellIndex);
                            // }
                        }
                    }					
					else // If the cell isn't currently in the viewport
					{
						Collection<CellBase> visCells = rowControl.VisibleCells;
						if (visCells.Count > 0)
						{
							int lastIndex = visColumns.IndexOf(visCells[visCells.Count - 1].Column);
							// The last visible cell maybe be a filler cell, so ignore it if it is. 
							if (lastIndex == -1 && visCells.Count > 1)
                                lastIndex = visColumns.IndexOf(visCells[visCells.Count - 2].Column);

                            if (lastIndex != -1)
                            {
                                if (cellIndex > lastIndex)
                                {
                                    // if the cell to scroll into view, has a larger index, then the last visible index
                                    // then we need to render backwards
                                    this.ReverCellStartIndex = cellIndex;
                                    this.ReverseCellMeasure = true;
                                    this.ReverseChildColumn = cell.Column;
                                    this.ReverseColumnLayout = cell.Row.ColumnLayout;
                                }
                                else
                                {
                                    // That must mean that the cell must be to the far left, so lets scroll normally.
                                    rowControl.SetScrollLeft(cellIndex);
                                }
                            }
						}
					}
				}

				// Lets render this row first, to ensure everything is scrolled correctly. 
				this.RenderRow(cell.Row, this._previousWidth);

                // To be safe, its worth recalculationg the HorizontalMax for this manager.
                cell.Row.Manager.OverrideHorizontalMax = -1;
			}
			else
			{
				if (this.VisibleRows.Count > 0)
				{
					int lastIndex;

					if (alignment == CellAlignment.Top)
					{
						this.ScrollInfo.VerticalScrollBar.Value = index;
					}
					else if (alignment == CellAlignment.Bottom)
					{
						this._reverseRowStartIndex = index;
						this._reverseMeasure = true;
					}
					else
					{
						// Find the index of the last visible row. 
						lastIndex = this.Grid.InternalRows.IndexOf(this.VisibleRows[this.VisibleRows.Count - 1]);
						if (index > lastIndex)
						{
							// If the index of the last visible row, is less than the current row
							// lets render backwards
							this._reverseRowStartIndex = index;
							this._reverseMeasure = true;
						}
						else
						{
                            // So, we probably just expanded some rows, which means we need to make sure 
                            // this is set properly, otherwise, we'll never display cell in view.
                            if (index > this.ScrollInfo.VerticalScrollBar.Maximum)
                                this.ScrollInfo.VerticalScrollBar.Maximum = index;

							// this must mean that the row is towards the top, so lets render normally.
							this.ScrollInfo.VerticalScrollBar.Value = index;
						}
					}

					if (horizontalScrollNeeded)
					{

						// Now lets make sure the cell is in view. 
						int currentRowLevel = cell.Row.Manager.Level;
						RowBase referenceRow = null;

						// Lets loop through all the rows and see if we can find a row at the same level 
						// as this row. We need to do this so that we can check the visible cells to determine 
						// how we need to scroll. 
						foreach (RowBase row in this.VisibleRows)
						{
							if (currentRowLevel == row.Manager.Level && row.GetType() == cell.Row.GetType() && row.ColumnLayout == cell.Row.ColumnLayout)
							{
								referenceRow = row;
								break;
							}
						}

						int cellIndex = cell.Row.VisibleCells.IndexOf(cell);

						// If we didn'type find a reference row, then lets just render our row, and see where we are. 
						if (referenceRow == null)
						{
							this.RenderRow(cell.Row, this._previousWidth);
							referenceRow = cell.Row;
						}

						CellsPanel rowControl = referenceRow.Control;

						Collection<CellBase> visCells = rowControl.VisibleCells;
                        if (visCells.Count > 0)
                        {
                            // Scroll by the root most column
                            Column rootColumn = cell.Column;
                            while (rootColumn.ParentColumn != null)
                                rootColumn = rootColumn.ParentColumn;

                            // Make that cell the cell we scroll into view on.
                            CellBase rootCell = cell.Row.Cells[rootColumn];

                            ReadOnlyCollection<Column> visColumns = cell.Row.Columns.AllVisibleColumns;

                            cellIndex = visColumns.IndexOf(rootColumn);
                            if (cellIndex == -1)
                                cellIndex = 0;

                            lastIndex = visColumns.IndexOf(visCells[visCells.Count - 1].Column);
                            // The last visible cell maybe be a filler cell, so ignore it if it is. 
                            if (lastIndex == -1 && visCells.Count > 1)
                                lastIndex = visColumns.IndexOf(visCells[visCells.Count - 2].Column);

                            if (lastIndex != -1 && referenceRow.CanScrollHorizontally) // See if we're on a row that has the ability to even scroll horizontally. 
                            {
                                double firstIndex = cell.Row.Cells.IndexOf(visCells[0]);

                                if (cell.Column.ParentColumn == null) // Ignore this for GroupColumns
                                {
                                    if (cellIndex > lastIndex)
                                    {
                                        // if the cell to scroll into view, has a larger index, then the last visible index
                                        // then we need to render backwards						
                                        this.ReverCellStartIndex = cellIndex;
                                        this.ReverseCellMeasure = true;
                                        this.ReverseColumnLayout = referenceRow.ColumnLayout;
                                    }
                                    else if (cellIndex < firstIndex)
                                    {
                                        // That must mean that the cell must be to the far left, so lets scroll normally.
                                        rowControl.SetScrollLeft(cellIndex);
                                    }
                                }
                            }
                        }

						// Lets render this row first, to ensure everything is scrolled correctly. 
						this.RenderRow(cell.Row, this._previousWidth);

                        // To be safe, its worth recalculationg the HorizontalMax for this manager.
                        cell.Row.Manager.OverrideHorizontalMax = -1;
					}
				}
				
			}
			this._scrollIntoViewCell = cell;
			this.ScrollCellIntoViewInProgress = true;
			this.InvalidateMeasure();
		}

		#endregion // ScrollCellIntoView

		#endregion // Public

		#region Protected

		#region RenderRow

		/// <summary>
		/// Creates a control for the row, and adds it as child of the panel. 
		/// </summary>
		/// <param name="row">The row which is now in View. </param>
		/// <param name="availableWidth">The amount of width the row has to work with.</param>
		/// <returns></returns>
		protected virtual Size RenderRow(RowBase row, double availableWidth)
		{
			// 1/13/09 - SJZ
			// In this method we need the Row to give us its Desired height and width. 
			// B/c we're virtualizing a row, all cells don't neccessarily get created, so we have a scrollbar
			// that tells the CellsPanel what Cells should currently be rendered. The problem is, that since the
			// Size we're passing into the the Measure method doesnt change, the Row.Control(aka CellsPanel) doesn'type always 
			// invoke MeasureOverride of the CellsPanel.  Now, previously I had code to call InvalidateMeasure on the CellsPanel,
			// which would call MeasureOverride, eventually, however it was async in the fact that MeasureOverreide doesnt get
			// triggered the instant its called. Meaning the DesiredSize of the CellsPanel would probably be inaccurate. 
			
			// This brings us to the workaround. The idea behind it, is to modify  the height of we pass into measure every time
			// Measure is called on the RowsPanel. Thus the _invalidateRowHeight member variable. 

			// However, there was one issue with this:
			// When you pass a size to the Measure method of a control, the desired size will be the minimum
			// of the value passed into the method and the size requested by the control.  For example, if i was to pass into the Measure 
			// method a height of 1, and the Control returns a height of 100 in it's MeasureOverride, the Desired Height of the control will be 1.
			// Now, if i was to pass a height of 500 to Measure, and the control was to return a height of 100, the desired height of the
			// control would be 100. 
			// So, my first thought was to toggle between double.PositiveInfinity and double.MaxValue. However, for some reason 
			// Silverlight doesn'type distinguish between the 2, and treated them as if they were the same value. So, i chose a random value
			// 10,000 and 10,0001, figuring that a row's height should never exceed that value. And, since i toggle this value
			// every time a MeasuerOverride is triggered for the RowsPanel, the rows always render. 
							
			if (row.Control == null)
			{
				RecyclingManager.Manager.AttachElement(row, this);
				row.Control.Owner = this;
			}

			// There was one other problem with this fix. A row may have been recycled, so the last time it was rendered, 
			// the height passed into it's measure may have been the same as the current _invalidateRowHeight. 
			// Thus the CellsPanel (aka row.ControL) stores off the last _invalidateRowHeight it used in it's PreviousInvalidateHeight
			// property. Then we just toggle the value, and voila, Measure will trigger the CellsPanel's MeasuerOverride.
			double invalidateHeight = this._invalidateRowHeight;
			if (row.Control.PreviousInvalidateHeight == invalidateHeight)
				invalidateHeight = (invalidateHeight == 10000) ? 10001 : 10000;

			if (!this._visibleManagers.Contains(row.Manager))
			{
				this._visibleManagers.Add(row.Manager);
				row.Manager.OverflowAdjustment = 0;
				row.Manager.IsFirstRowRenderingInThisLayoutCycle = true;
				row.Manager.CachedIndentation = 0;
				row.Manager.RowWidth = 0;
				row.Manager.IndexOfFirstColumnRendered = 0;
                row.Manager.RenderedMergedCells.Clear();

			}

            row.Control.Measure(new Size(availableWidth, invalidateHeight));

            double height = row.Control.DesiredSize.Height;

            // Ead adorner has a chance to add extra content after a row. 
            // If Content is added, we need to make sure we account for it in the height.
            foreach (XamGridRenderAdorner renderer in this.Grid.RenderAdorners)
                height += renderer.MeasureAfterRow(row);

			return new Size(row.Control.DesiredSize.Width, height);
		}

		#endregion // RenderRow

		#region UpdateScrollInfo

		/// <summary>
		/// Updates the ScrollInfo of the <see cref="RowsPanel"/>.
		/// Such as changing the horizontal/vertical scrollbar visibility, or their viewport size.
		/// </summary>
		protected virtual void UpdateScrollInfo(int totalRowCount)
		{

            if (!this.Grid.IsLoaded)
                return;


			IProvideScrollInfo info = this.ScrollInfo;
			ScrollBar vertBar = info.VerticalScrollBar;
			if (vertBar != null)
			{
				double val = vertBar.Value;

				vertBar.Maximum = this._overrideVerticalMax;

				// So, the scrollbar has this weird bug, where sometimes
				// if you change the max, and the value is still within the max and min, it'll still change the 
				// value, even though it shouldn't have touched it. 
                if (vertBar.Value != val && val < vertBar.Maximum && !this._ensuerVertSBValueUpdated)
					vertBar.Value = val;

                this._ensuerVertSBValueUpdated = false;

                // So we should only set the Viewportsize once
                // As we don't want it to change while we're scrolling. 
                if (this._resetVertSBViewportSize)
                {
                    vertBar.ViewportSize = this._visibleRows.Count;
                    this._resetVertSBViewportSize = false;
                }

                double largeChange = this._visibleRows.Count - 1;
                if (largeChange < 0)
                    largeChange = 0; 

                // Limit the large change to one less row, so that partial rows don't get jumped over when changing "pages"
                vertBar.LargeChange = largeChange;
				vertBar.SmallChange = (double)vertBar.ViewportSize / 10;

				Visibility previous = vertBar.Visibility;
				vertBar.Visibility = ((this._overrideVerticalMax <= 0) || totalRowCount == 0) ? Visibility.Collapsed : Visibility.Visible;

				if (vertBar.Visibility != previous && vertBar.Visibility == Visibility.Collapsed)
					vertBar.Value = 0; 
			}

			ScrollBar horizBar = info.HorizontalScrollBar;
			if (horizBar != null)
			{
				double totalCellCount = 0, totalVisibleCellCount = 0; 

				double max = -1;

				foreach(RowsManagerBase manager in this._visibleManagers)
				{
					if (manager.OverrideHorizontalMax >= max && manager.OverrideHorizontalMax > 0)
					{
						this.HorizontalRowsManager = manager;
						max = manager.OverrideHorizontalMax;
						totalCellCount = manager.ScrollableCellCount;
						totalVisibleCellCount = manager.VisibleCellCount;	
					}
				}

				if (max != -1 && !double.IsPositiveInfinity(max))
				{
					// The Scrollbar, actually stores off the original value, when the new max is greater than the previous value
					// Which means, that we can get some weird jumping behavior when resizing columns that cause the horizbar's
					// maximum to change. So, lets make sure the value changes along with the maximum.
					if (horizBar.Value > max)
						horizBar.Value = max;
					horizBar.Maximum = max;

                    // So we should only set the Viewportsize once
                    // As we don't want it to change while we're scrolling. 
                    if (this._resetHorizSBViewportSize)
                    {
                        horizBar.ViewportSize = totalVisibleCellCount;
                        this._resetHorizSBViewportSize = false;
                    }
				}

				horizBar.LargeChange = totalVisibleCellCount;
				horizBar.SmallChange = 1;

				bool collapsed = (max <= 0) && (totalVisibleCellCount == totalCellCount || totalCellCount == 0);
				
				// Once the scrollbar becomes visible, keep it, until otherwise told to reset. 
				if (horizBar.Visibility == Visibility.Collapsed || this._recalcHorizSBVis)
				{
					horizBar.Visibility = (collapsed) ? Visibility.Collapsed : Visibility.Visible;
					this._recalcHorizSBVis = false;

                    horizBar.IsEnabled = true;
				}
				else if (collapsed)
                {
                    // If its visible, but it shouldn'type be enable, disable it. 
					if (horizBar.IsEnabled)
					{
						horizBar.Value = 0;
						horizBar.Maximum = 0;
						horizBar.IsEnabled = false;
					    horizBar.Visibility = Visibility.Collapsed;
					}
				}
				else
				{
					// Re-enable the scrollbar if its visible and was disabled.
					if (!horizBar.IsEnabled)
					{
					    horizBar.IsEnabled = true;
					    horizBar.Visibility = Visibility.Visible;
					}
				}
				
				if (horizBar.Visibility == Visibility.Collapsed)
					horizBar.Value = 0; 
			}

		}
		#endregion // UpdateScrollInfo

		#region ArrangeRow

		/// <summary>
		/// Calls Arrange on the specified row. 
		/// </summary>
		/// <param name="row">The row that should be arranged.</param>
		/// <param name="left">The left value the row should be positioned at.</param>
		/// <param name="top">The top value the row should be positioned at.</param>
		/// <param name="width">The width the row should be.</param>
		/// <param name="height">The height the row should be.</param>
		public static bool ArrangeRow(RowBase row, double left, double top, double width, double height)
		{
			row.Control.ArrangeRaised = false;

			row.Control.Arrange(new Rect(left, top, width, height));

			// If Arrange isn't triggered right away, it means there is probably another Measure cycle that is waiting to fire. 
			// Since we now know that, we can short circuit our ArrangeOverride, which should even increase perf. 
			return row.Control.ArrangeRaised;
		}

		#endregion // ArrangeRow

        #region ResetDataRows

        /// <summary>
        /// Removes all Rows from the Panel.
        /// </summary>
        protected internal void ResetDataRows()
        {
            this.RemoveDataRows(this._topRows);
            this.RemoveDataRows(this._bottomRows);

            this.RemoveDataRows(this._visibleRows);

            // If we're in measure currently invalidate it, so that we reupdate ourselves, as we've just cleared out some of our rows.
            this.MeasureCalled = false;
        }

        #endregion // ResetDataRows

  		#endregion // Protected

		#region Private

		#region ReleaseRow

		private void ReleaseRow(RowBase row)
		{
			if (!RecyclingManager.Manager.ReleaseElement(row, this))
			{
				this._nonReleasedRows.Add(row);
			}
		}

		#endregion // ReleaseRow

		#region ThrowoutUnusedRows
		private void ThrowoutUnusedRows(List<RowBase> previousVisibleRows, List<FixedRowSeparator> previousFixedRowSeparators)
		{
			foreach (RowBase row in previousVisibleRows)
			{
				if (!this._visibleRows.Contains(row))
				{
					if (row.Control != null)
					{
						this.ReleaseRow(row);
					}
				}
			}

			foreach (FixedRowSeparator separator in previousFixedRowSeparators)
			{
				if (!this._topFixRowSeperators.Contains(separator) && !this._bottomFixRowSeperators.Contains(separator) )
				{
					this.Children.Remove(separator);
				}
			}

		}
		#endregion // ThrowoutUnusedRows

		#region ResolveBaseSize

		private Size ResolveBaseSize(Size availableSize)
		{
			Size alternateReturnSize = base.MeasureOverride(availableSize);
			if (alternateReturnSize.Height == 0 && !double.IsInfinity(availableSize.Height) && !double.IsNaN(availableSize.Height))
				alternateReturnSize.Height = availableSize.Height;

			if (alternateReturnSize.Width == 0 && !double.IsInfinity(availableSize.Width) && !double.IsNaN(availableSize.Width))
				alternateReturnSize.Width = availableSize.Width;

			return alternateReturnSize;
		}

		#endregion // ResolveBaseSize

        #region ResetDataRows

        private void RemoveDataRows(IList<RowBase> rows)
        {
            List<RowBase> removedRows = new List<RowBase>();
            foreach (RowBase r in rows)
            {
                if (r.RowType == RowType.DataRow)
                {
                    this.ReleaseRow(r);
                    removedRows.Add(r);
                }
            }

            foreach (RowBase r in removedRows)
            {
                rows.Remove(r);
            }
        }

        #endregion // ResetDataRows

        #endregion // Private

        #region Internal

        internal void RenderRow(RowBase row)
		{
			this.RenderRow(row, this._previousWidth);
		}

        internal void InvalidateMeasure(EmptyDelegate callback)
        {
            this._invalidateMeasureCallback = callback;

            this.InvalidateMeasure();
        }

		#endregion // Internal

		#endregion // Methods

		#region Overrides

		#region MeasureOverride

		/// <summary>
		/// Determines how what rows can fit given the available size and scroll information.
		/// </summary>
		/// <param name="availableSize">
		///	The available size that this object can give to child objects. Infinity can be 
		///	specified as a value to indicate that All rows should be displayed.
		///	</param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size availableSize)
		{

            if (this.Grid == null || !this.Grid.IsLoaded)



                return base.MeasureOverride(availableSize);

			// Measure Gets triggered by lots of things during it's own cycle. Since we don't want to keep evaluating the same thing
			// over and over again, we'll set a flag so that it only does this once. Note: the flag will be reset in ArrageOverride 
            if (this._measureCalled && this._previousWidth == availableSize.Width && this._previousHeight == availableSize.Height)
                return this.DesiredSize;
			else
                this._measureCalled = true;
			
			if(this.Grid.IsDeferredScrollingCurrently)
			    return this.ResolveBaseSize(availableSize);

            // Only if our sizes change, be sure to update the ViewportSize.
            if (this._previousWidth != availableSize.Width)
            {
                this._resetHorizSBViewportSize = true;
            }

            if (this._previousHeight != availableSize.Height)
            {
                this._resetVertSBViewportSize = true;
            }

			// So, now we're storing the overflow adjustment on the rows manager. 
			// This will ensure that they're the same for ever row on a band, so that the header is never off
			// when doing auto calculations. 
			foreach (RowsManagerBase manager in this._visibleManagers)
			{
				manager.OverflowAdjustment = 0;
				manager.IsFirstRowRenderingInThisLayoutCycle = true;
				manager.CachedIndentation = 0;
				manager.RowWidth = 0;
				manager.IndexOfFirstColumnRendered = 0;
                manager.RenderedMergedCells.Clear();
			}

            // For any additional renderers that are plugging into the panel we need to initalize
            foreach (XamGridRenderAdorner adorner in this.Grid.RenderAdorners)
                adorner.Initialize();

            // Store currently Visible Rows, and clear out the global collection of VisibleRows
            List<RowBase> previousVisibleRows = new List<RowBase>();

            if (this._previousHeight != availableSize.Height)
            {
                this._overrideVerticalMax = -1;
            }

            if (this.VisibleRows.Count == 0)
            {
                // This is our first load.
                // So, make sure that we remove the horizontal scrollbar if it isn't neccessary, 
                this._recalcHorizSBVis = true;
                this._resetHorizSBViewportSize = true;
                this._resetVertSBViewportSize = true;
            }

            // Something changed, so we can't rely on our advanced logic later on to, figure out which rows
            // are going to be available on the next go around, so lets just release them all and go again.
            if (this._onNextMeasureReleaseVisibleRows)
            {
                foreach (RowBase row in this.VisibleRows)
                    this.ReleaseRow(row);

                this.VisibleRows.Clear();

                this._onNextMeasureReleaseVisibleRows = false;
            }

			this._previousHeight = availableSize.Height;

			// Store the width, so that if we need to Render a row at some point outside of the MesureOverride (such as in ScrollCellIntoView
			// we can make sure that the width is adhered to. 
			this._previousWidth = availableSize.Width;

			this.InLayoutPhase = true;

			// See comments in RenderRow for a complete description of this. 
			this._invalidateRowHeight = (this._invalidateRowHeight == 10000) ? 10001 : 10000;

			this._visibleManagers.Clear();

			double currentHeight = 0;
			double fixedRowHeight = 0;
			double availableHeight = availableSize.Height;
			double maxRowWidth = 0;

			ScrollBar vertBar = this.ScrollInfo.VerticalScrollBar;
			double scrollTop = (vertBar != null) ? vertBar.Value : 0;

			int rowCount = this.Grid.InternalRows.Count;

			List<FixedRowSeparator> previousVisibleFixedRowSeparators = new List<FixedRowSeparator>(this._topFixRowSeperators);
			previousVisibleFixedRowSeparators.AddRange(this._bottomFixRowSeperators);
			this._topFixRowSeperators.Clear();
			this._bottomFixRowSeperators.Clear();

			// Render Top Fixed Rows
			foreach (RowBase fixedRow in this._topRows)
			{
				Size rowSize = this.RenderRow(fixedRow, availableSize.Width);
				fixedRowHeight += rowSize.Height;
				maxRowWidth = Math.Max(maxRowWidth, rowSize.Width);
				if (fixedRow.RequiresFixedRowSeparator)
				{
					FixedRowSeparator separator = fixedRow.ResolveSeparator();
					if (!this.Children.Contains(separator))
						this.Children.Add(separator);
					separator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					fixedRowHeight += separator.DesiredSize.Height;
					this._topFixRowSeperators.Add(separator);
				}
			}

			// Render Bottom Fixed Rows
			foreach (RowBase fixedRow in this._bottomRows)
			{
				Size rowSize = this.RenderRow(fixedRow, availableSize.Width);
				fixedRowHeight += rowSize.Height;
				maxRowWidth = Math.Max(maxRowWidth, rowSize.Width);
				if (fixedRow.RequiresFixedRowSeparator)
				{
					FixedRowSeparator separator = fixedRow.ResolveSeparator();
					if (!this.Children.Contains(separator))
						this.Children.Add(separator);
					separator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
					fixedRowHeight += separator.DesiredSize.Height;
					this._bottomFixRowSeperators.Add(separator);
				}
			}

			availableHeight -= fixedRowHeight;

			if(rowCount == 0)
			{
                previousVisibleRows.AddRange(this.VisibleRows);
                this.VisibleRows.Clear();
				this.ThrowoutUnusedRows(previousVisibleRows, previousVisibleFixedRowSeparators);

				// If there aren't rows, but the availablesize.height is infinitiy, then we should set the height to
				// be the size of the fixed rows.
				if(double.IsPositiveInfinity(availableHeight))
					availableSize.Height = fixedRowHeight;

                // Make sure to still call render Adorners when there aren't any rows, so any cleanup needed can be done. 
                foreach (XamGridRenderAdorner adorner in this.Grid.RenderAdorners)
                    adorner.Measure(availableSize);

				return this.ResolveBaseSize(availableSize);
			}

			int startRow = 0, currentRow = 0;
			double percentScroll = 0;
			double rowHeight=0;
            if (!this._reverseMeasure)
            {
                // Add First Row
                startRow = (int)scrollTop;

                // Something must have been collapsed. So, now the scrollTop is greater than the amount of scrollable rows
                // So, make, the last scrollable row, the top. 
                if (startRow >= rowCount)
                    startRow = rowCount - 1;

                RowBase firstRow = this.Grid.InternalRows[startRow];

                if (this.VisibleRows.Count > 0)
                {
                    int prevStartIndex = this.VisibleRows.IndexOf(firstRow);

                    if (prevStartIndex != 0)
                    {
                        if (prevStartIndex != -1)
                        {
                            for (int i = prevStartIndex; i > 0; i--)
                            {
                                RowBase zeRow = this.VisibleRows[0];
                                this.ReleaseRow(zeRow);
                                this.VisibleRows.RemoveAt(0);
                            }

                            previousVisibleRows.AddRange(this.VisibleRows);
                        }
                        else
                        {
                            int total = this.VisibleRows.Count;
                            int first = this.Grid.InternalRows.IndexOf(this.VisibleRows[0]);
                            int last = first + total - 1;

                            if (last < startRow)
                            {
                                foreach (RowBase row in this.VisibleRows)
                                    this.ReleaseRow(row);

                                this.VisibleRows.Clear();
                            }
                            else
                            {
                                total--;
                                int diff = first - startRow;

                                for (int i = 0; i < diff && i < total; i++)
                                {
                                    int index = total - i;
                                    RowBase zerRow = this.VisibleRows[index];
                                    this.ReleaseRow(zerRow);
                                    this.VisibleRows.RemoveAt(index);
                                }
                            }

                            previousVisibleRows.AddRange(this.VisibleRows);
                        }
                    }
                    else
                    {
                        previousVisibleRows.AddRange(this.VisibleRows);
                    }

                    this.VisibleRows.Clear();
                }

                // Ok, So in some specific scenarios, a refresh may be triggered on the Grid, and the first row that was visible
                // maybe have been a child row, which hasn't been touched since, and so might not be registered in the InternalRows
                // Collection. When this happens, we actually start out on the wrong first row. The problem is that b/c the row
                // above it is never touched, the child rows which were the top rows, are never registered and thus never come into view
                // Thus this code here, simply just accesses the previous row.  The fact that it does this, ensures that all of it's child rows
                // have a chance to come into view. 
                if (startRow > 0)
                {
                    RowBase xRow = this.Grid.InternalRows[startRow - 1];
                }

                Size firstRowSize = this.RenderRow(firstRow, availableSize.Width);
                rowHeight = firstRowSize.Height;
                maxRowWidth = Math.Max(maxRowWidth, firstRowSize.Width);
                currentHeight += rowHeight;
                this._visibleRows.Add(firstRow);

                // Calculate PercentScroll
                if (vertBar != null)
                {
                    double percent = vertBar.Value - (int)vertBar.Value;
                    percentScroll = rowHeight * percent;
                    currentHeight -= percentScroll;
                    this._measureScrollBarValue = vertBar.Value;
                }

                // Add Rows untill there is no more height left
                for (currentRow = startRow + 1; currentRow < rowCount; currentRow++)
                {
                    if (currentHeight >= availableHeight)
                        break;

                    RowBase r = this.Grid.InternalRows[currentRow];
                    Size rowSize = this.RenderRow(r, availableSize.Width);
                    currentHeight += rowSize.Height;
                    maxRowWidth = Math.Max(maxRowWidth, rowSize.Width);
                    this._visibleRows.Add(r);
                }

                // Add the percent scroll back, so that we can truly validate if we've scrolled past the last item.
                currentHeight += percentScroll;

                startRow--;
            }
            else
            {
                previousVisibleRows.AddRange(this.VisibleRows);
                this.VisibleRows.Clear();
                startRow = this._reverseRowStartIndex;
                if (startRow >= rowCount)
                    startRow = 0;
            }

			// If the height of all the visible rows is less then whats available in the viewport, and there are more rows in the 
			// collection, it means we've scrolled further than we needed to. Since we don't want whitespace to appear under 
			// the last DataRow, lets add more rows and update the maximum scroll value.
			if (currentHeight < availableHeight && this._visibleRows.Count < rowCount)
			{
				for (currentRow = startRow; currentRow >= 0; currentRow--)
				{
					RowBase r = this.Grid.InternalRows[currentRow];
					Size rowSize = this.RenderRow(r, availableSize.Width);
					rowHeight = rowSize.Height;
					maxRowWidth = Math.Max(maxRowWidth, rowSize.Width);
					currentHeight += rowHeight;
					this._visibleRows.Insert(0, r);

					if (currentHeight >= availableHeight)
					{
						if (this._reverseMeasure)
						{
							percentScroll = ((currentHeight - availableHeight) / rowHeight);
                            double val = currentRow + percentScroll;
							this.ScrollInfo.VerticalScrollBar.Value = val;

                            if(this.ScrollInfo.VerticalScrollBar.Value != val)
                            {
                                this._ensuerVertSBValueUpdated = true;
                            }
						}
						break;
					}
				}
			}


            // For any additional renderers that are plugging into the panel
            // performa a measure operation on them, and update the currentHeight to take them into consideration.
            foreach (XamGridRenderAdorner adorner in this.Grid.RenderAdorners)
                adorner.Measure(availableSize);
		
            // If we're not given Infinite room to work with, then we need to figure out the Maximum value for the Vertical Scrollbar.
			if (!double.IsPositiveInfinity(availableSize.Height))
			{
                // I was doing a LOT of extra work in calculating this, which was very expensive on the first load. 
                // However, most grids aren't going to need that extra work, b/c they generally all have rows that are all the same height. 
                // So, since thats the most common case, don't waste performance on an edge case. 
                int currentLastRowIndex = rowCount - this.VisibleRows.Count;
                double sp = (currentHeight - availableHeight) / rowHeight;

                // Whoops, becareful to not divide by zero, as it is possible that a row could have a height of zero.
                if (rowHeight == 0)
                    sp = 0;

                this._overrideVerticalMax = currentLastRowIndex + sp;

				if (this._overrideVerticalMax < 0)
					this._overrideVerticalMax = -1;
			}


			bool exitEarly = false;
			// If the rows count doesn'type match up, it means we've probably loaded 
			// a cached row that was expanded. In this case, lets re-measure the panel. 
			if (this.Grid.InternalRows.Count != rowCount)
			{
				this._overrideVerticalMax = -1;
				this._measureCalled = false;
				this.Measure(availableSize);
				exitEarly = true;
			}

			// Throw out unused rows. 
			this.ThrowoutUnusedRows(previousVisibleRows, previousVisibleFixedRowSeparators);

			if (exitEarly)
				return this.DesiredSize;

			this._overflowAdjustment = (currentHeight - availableHeight);			

			double width = availableSize.Width;
			if (double.IsPositiveInfinity(width))
			{
				width = maxRowWidth;
				if (!this._measureCalledInfinite)
				{
					// So, if the width is infinite, we might not re-measure,
					// which means some rows aren't going to be the full max width. 
					// So, lets re-measure to make sure this occurs correctly. 
					this._measureCalled = false;
					this._measureCalledInfinite = true;
					this.Measure(availableSize);
				}
			}

			if (double.IsPositiveInfinity(availableSize.Height))
				currentHeight += fixedRowHeight;
			else
				currentHeight = availableSize.Height;

			if (currentHeight < 0)
				currentHeight = 0;

			return new Size(width, currentHeight);
		}		
		#endregion // MeasureOverride

		#region ArrangeOverride

		/// <summary>
		/// Arranges each row that should be visible, one on top of the other, similar to a 
		/// Vertical <see cref="StackPanel"/>.
		/// </summary>
		/// <param name="finalSize">
		/// The final area within the parent that this object 
		/// should use to arrange itself and its children.
		/// </param>
		/// <returns></returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			// Since we're in arrange now, its ok to reset the measure flag, so that if measure is called again, it will execute. 
			this._measureCalled = false;
			this._measureCalledInfinite = false;
			this.InLayoutPhase = false;
            bool arrangeNotWorking = false;

			if (this.Grid.IsDeferredScrollingCurrently)
				return base.ArrangeOverride(finalSize);
			
			this._clipRG.Rect = new Rect(0, 0, finalSize.Width, finalSize.Height);
			
            // So, we get rid of elements that were made recently available. But, its still possible for arrange to not be honored
            // which means, the could still techiniaclly wind up in view. So, if they aren't arranged, we store them off for the next
            // arrange cycle, and arrange them then. 
            if (this._notVisibleAndNotArrangedRows.Count > 0)
            {
                List<FrameworkElement> elementsStillNotArranged = new List<FrameworkElement>();
                foreach (CellsPanel row in this._notVisibleAndNotArrangedRows)
                {
                    if (LayoutInformation.GetLayoutSlot(row) != this._hiddenRowRect)
                    {
                        row.ArrangeRaised = false;
                        row.Arrange(this._hiddenRowRect);
                        if (!row.ArrangeRaised)
                        {
                            elementsStillNotArranged.Add(row);
                            break;
                        }
                    }
                }

                this._notVisibleAndNotArrangedRows.Clear();
                this._notVisibleAndNotArrangedRows.AddRange(elementsStillNotArranged);
            }

            // Move all Rows that aren'type being used, out of view. 
            List<FrameworkElement> unusedRows = null;

            // So, if we're unloaded, we need to make sure that we re-arrange all rows/cells that were out of view
            // Thanks to the unloaded event, we can set a flag, so that we only do this in the first arrange pass after we're unloaded,
            // thus allowing us to keep our perf up.
            if (this._unloadedSoResetAll)
            {
                unusedRows = RecyclingManager.Manager.GetAvailableElements(this);
                this._unloadedSoResetAll = false;
            }
            else
            {
                unusedRows = RecyclingManager.Manager.GetRecentlyAvailableElements(this, true);
            }

            foreach (CellsPanel row in unusedRows)
            {
                row.ArrangeRaised = false;
                row.Arrange(this._hiddenRowRect);
                if (!row.ArrangeRaised)
                {
                    this._notVisibleAndNotArrangedRows.Add(row);
                }
            }

			// Some rows, like rows that are in edit mode, may choose to not get released. 
			// If thats the case, then we need to make sure that they get placed out of view. 
			foreach (RowBase row in this._nonReleasedRows)
			{
				if(row.Control != null)
					row.Control.Arrange(this._hiddenRowRect);
			}

			this._nonReleasedRows.Clear();

			int rowCount = this.Grid.InternalRows.Count;
					
			this.UpdateScrollInfo(rowCount);

			double top = 0;

			// Render Top FixedRows
			foreach (RowBase fixedRow in this._topRows)
			{
                if (fixedRow.Control == null)
                    continue;
				double height = fixedRow.Control.DesiredSize.Height;

				RowsPanel.ArrangeRow(fixedRow, 0, top, fixedRow.Control.DesiredSize.Width, height);
				Canvas.SetZIndex(fixedRow.Control, 10003);
				top += height;

				if (fixedRow.RequiresFixedRowSeparator)
				{
					FixedRowSeparator separator = fixedRow.ResolveSeparator();
					separator.Style = this._grid.FixedRowSeparatorStyle;
					separator.Arrange(new Rect(0, top, finalSize.Width, separator.DesiredSize.Height));
					top += separator.DesiredSize.Height;
                    Canvas.SetZIndex(separator, 10002);
				}
			}

			// Render the Bottom Fixed Rows. 
			double bottom = finalSize.Height;
			foreach (RowBase fixedRow in this._bottomRows)
            {
                if (fixedRow.Control == null)
                    continue;
				double height = fixedRow.Control.DesiredSize.Height;

				bottom -= height;

                Canvas.SetZIndex(fixedRow.Control, 10003);

				RowsPanel.ArrangeRow(fixedRow, 0, bottom, fixedRow.Control.DesiredSize.Width, height);
				
				if (fixedRow.RequiresFixedRowSeparator)
				{
					FixedRowSeparator separator = fixedRow.ResolveSeparator();
					separator.Style = this._grid.FixedRowSeparatorStyle;
					bottom -= separator.DesiredSize.Height;
					separator.Arrange(new Rect(0, bottom, finalSize.Width, separator.DesiredSize.Height));
                    Canvas.SetZIndex(separator, 10002);
				}
			}

            // Stores off then end of the FixedRows.
            double fixedRowEnd = top;

			if (rowCount != 0)
			{
				// Calculate the offset TopValue, for the first row in this normal rows collection. 
				ScrollBar vertSB = this.ScrollInfo.VerticalScrollBar;
				if (vertSB != null && vertSB.Visibility == Visibility.Visible && this._visibleRows.Count > 0)
				{
					if (this._measureScrollBarValue != vertSB.Maximum)
					{
						double percent = this._measureScrollBarValue - (int)this._measureScrollBarValue;

                        RowBase row = this._visibleRows[0];

                        double firstRowHeight = row.Control.DesiredSize.Height;

                        // We need to make sure we call ArrangeAfterRow for the top most row, 
                        // so that we can properly account for its total height, as a renderer might 
                        // add additional height to that specific row.
                        foreach (XamGridRenderAdorner adorner in this.Grid.RenderAdorners)
                            firstRowHeight += adorner.ArrangeAfterRow(row, top, finalSize);                   

						double topVal = firstRowHeight * percent;

						double scrollTop = this._measureScrollBarValue;
						double max = this._overrideVerticalMax;
						if (scrollTop >= max && max > 0)
							top -= this._overflowAdjustment;
						else
							top += -topVal;
					}
					else
					{
						// We've reached the last child, so lets make sure its visible. 
						top -= this._overflowAdjustment;
					}

					// For a cleaner scrolling experience, update the small change of the scrollbar to account for the currentIndex visible children.
					vertSB.SmallChange = (double)this._visibleRows.Count / 10;
				}

				// Render Normal Rows
				foreach(RowBase row in this._visibleRows)
				{
					double height = row.Control.DesiredSize.Height;
                    if(!RowsPanel.ArrangeRow(row, 0, top, row.Control.DesiredSize.Width, height))
                        arrangeNotWorking = true;
                    Canvas.SetZIndex(row.Control, 0);                    
					top += height;

                    // We allow renderers to render additional controls after a row. 
                    // So let them be arranged properly, and update the top position accordingly
                    foreach (XamGridRenderAdorner adorner in this.Grid.RenderAdorners)
                        top += adorner.ArrangeAfterRow(row, top, finalSize);                   
				}
			}

            // Now after all rows have rendered, we allow additional adorners to be rendered
            // On top of the rows panel. Each renderer has a chance to plug in here.
            foreach (XamGridRenderAdorner adorner in this.Grid.RenderAdorners)
                adorner.ArrangeAdornments(finalSize, fixedRowEnd);

			this.InLayoutPhase = false;
			this._reverseMeasure = false;
			this.ReverseCellMeasure = false;

			// Make sure we arrange this, otherwise, the Fixed DropArea indicators won't show up. 
			this.Grid.DropAreaIndicatorPanel.Arrange(new Rect(0, 0, 0, 0));

            Canvas.SetZIndex(this.Grid.DropAreaIndicatorPanel, 10003); 

			if (this.ScrollCellIntoViewInProgress)
			{
				if (this._scrollIntoViewCell.Control == null)
					this.ScrollCellIntoView(this._scrollIntoViewCell, CellAlignment.NotSet, null);

				this.ScrollCellIntoViewInProgress = false;
				this._scrollIntoViewCell = null;

				// This Invalidate is here to make sure that everything has a second chance to re-measure
				this.InvalidateMeasure();
			}
			
            if (this._scrollCellIntoViewCallback != null)
			{
			    this._scrollCellIntoViewCallback();
			}

            if (this._invalidateMeasureCallback != null)
            {
                this._invalidateMeasureCallback();
            }

			// So, we can actually get ourselves into a 1 off battle
			// If more things need to be remeasured, then it means that the previously remeasured cells
			// need to be remeasured as well.
            if (this.RemeasuredCells.Count > 0)
            {
                // Instead of adding the cells via AddRange, lets add them
                // cautiously, to avoid an infinite loop. 
                int count = this.RemeasuredCells.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    CellBase cell = this.RemeasuredCells[i];
                    if (this.PreviouslyRemeasuredCells.Contains(cell))
                        this.RemeasuredCells.Remove(cell);
                }
            }

			// So, some of the cells feel that they should be re-measured. 
			// Lets do that for them, so that they can be properly boxed. 
			foreach (CellBase cell in this.RemeasuredCells)
			{
				if(cell.Control != null)
					cell.Control.Measure(cell.MeasuringSize);
			}

            if (!arrangeNotWorking)
            {
                if(this.RemeasuredCells.Count == 0)
                {
                    foreach (CellBase cell in PreviouslyRemeasuredCells)
                        cell.MeasuringSize = Size.Empty;

                    this.PreviouslyRemeasuredCells.Clear();
                }
                this.PreviouslyRemeasuredCells.AddRange(this.RemeasuredCells);
            }

			this.RemeasuredCells.Clear();

            // Make sure we disconnect these. 
            this._scrollCellIntoViewCallback = null;
            this._invalidateMeasureCallback = null;

			return finalSize;
		}
		#endregion // ArrangeOverride

		#endregion // Overrides

        #region EventHandlers

        void RowsPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.Grid != null)
            {
                
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

                
                this.ResetRows(false);
            }

            // Be sure to make the Measure phase false, as we just cleared out everything.
            this._measureCalled = false;

            // So, if we're unloaded, we need to make sure that we re-arrange all rows/cells that were out of view
            // Thanks to the unloaded event, we can set a flag, so that we only do this in the first arrange pass after we're unloaded
            this._unloadedSoResetAll = true;
        }

        #endregion // EventHandlers

        #region IRecyclableElementHost

        /// <summary>
        /// Invoked when an element is associated with an item in the panel
        /// </summary>
        /// <param name="element">The element being associated with an item</param>
        /// <param name="item">The item represented by the specified element</param>
        /// <param name="isNewlyRealized">True if the element is new; false if the element is being recycled</param>
        void IRecyclableElementHost.OnElementAttached(ISupportRecycling item, FrameworkElement element, bool isNewlyRealized)
        {

        }

        /// <summary>
        /// Invoked when an element is detached from an item.
        /// </summary>
        /// <param name="element">The element being released</param>
        /// <param name="item">The item that was represented by the element</param>
        /// <param name="isRemoved">True if the element is being removed from the children; otherwise false if the element is being kept for potential recycling later</param>
        void IRecyclableElementHost.OnElementReleased(ISupportRecycling item, FrameworkElement element, bool isRemoved)
        {
            if (isRemoved)
            {
                CellsPanel cp = element as CellsPanel;

                if (cp != null)
                {
                    cp.Owner = null;
                    RecyclingManager.Manager.ReleaseAll(cp);
                }
            }
        }

        /// <summary>
        /// Invoked when an element being released is to be considered for recycling.
        /// </summary>
        /// <param name="element">The element being released</param>
        /// <param name="item">The item that was represented by the element</param>
        /// <returns>Return true to indicate that the element should be removed from the panel; otherwise return false to allow the element to be recyled.</returns>
        bool IRecyclableElementHost.ShouldRemove(ISupportRecycling item, FrameworkElement element)
        {
            return false;
        }

        #endregion //  IRecyclableElementHost
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