using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A panel that organizes the <see cref="CellBase"/>s of a <see cref="RowBase"/>.
	/// </summary>
    public class CellsPanel : Panel, ICommandTarget
	{
		#region Members

		RowBase _row;
		Collection<CellBase> _visibleCells, _visibleFixedLeftCells, _visibleFixedRightCells, _additionalCells, _lastCellsDetached, _nonReleasedCells;
		RowsPanel _owner;
		IProvideScrollInfo _scrollInfo;
		RectangleGeometry _clipRG;
		double _prevAvailableWidth = -1;
		Rect _hideCellRect = new Rect(-1000, -1000, 0, 0);        

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="CellsPanel"/> class.
		/// </summary>
		public CellsPanel()
		{
			this._visibleCells = new Collection<CellBase>();
			this._visibleFixedLeftCells = new Collection<CellBase>();
			this._visibleFixedRightCells = new Collection<CellBase>();
			this._additionalCells = new Collection<CellBase>();
			this._clipRG = new RectangleGeometry();
			this.Clip = this._clipRG;
			this._lastCellsDetached = new Collection<CellBase>();
			this._nonReleasedCells = new Collection<CellBase>();
		}

		#endregion // Constructor

		#region Properties

		#region Public	

		#region Row

		/// <summary>
		/// Gets a refernce to the <see cref="RowBase"/> that owns the <see cref="CellsPanel"/>.
		/// </summary>
		public RowBase Row
		{
			get { return this._row; }
		}

		#endregion // Row

		#region Owner

		/// <summary>
		/// Gets/sets the RowsPanel that owns this control.
		/// </summary>
		public RowsPanel Owner
		{
			get { return this._owner; }
			set
			{
				this._owner = value;
                if (value != null)
                {
                    this._scrollInfo = (IProvideScrollInfo)this._owner.Grid;
                }
                else
                {
                    this._scrollInfo = null;
                }
			}
		}

		#endregion // Owner

		#endregion // Public

		#region Protected

		#region VisibleCells

		/// <summary>
		/// Gets a list of <see cref="CellBase"/> objects thats are currently visible.
		/// </summary>
		protected internal Collection<CellBase> VisibleCells
		{
			get { return this._visibleCells; }
		}

		#endregion // VisibleCells

		#region VisibleFixedLeftCells

		/// <summary>
		/// Gets a list of <see cref="CellBase"/> objects that are fixed to the left.
		/// </summary>
		protected internal Collection<CellBase> VisibleFixedLeftCells
		{
			get { return this._visibleFixedLeftCells; }
		}

		#endregion // VisibleFixedLeftCells

		#region VisibleFixedRightCells

		/// <summary>
		/// Gets a list of <see cref="CellBase"/> objects that are fixed to the right. 
		/// </summary>
		protected internal Collection<CellBase> VisibleFixedRightCells
		{
			get { return this._visibleFixedRightCells; }
		}

		#endregion // VisibleFixedRightCells

		#endregion // Protected

		#region Internal

		/// <summary>
		/// Used to store off the last height that was passed into the MeasuerOverride method of this panel.
		/// </summary>
		internal double PreviousInvalidateHeight
		{
			get;
			set;
		}

		internal bool ArrangeRaised
		{
			get;
			set;
		}

        internal RectangleGeometry ClipRect
        {
            get { return this._clipRG; }
        }

		#endregion // Internal

		#endregion // Properties

		#region Methods

		#region Public

		#region OnAttached

		/// <summary>
		/// Called when the <see cref="RowBase"/> is attached to the <see cref="CellsPanel"/>.
		/// </summary>
		/// <param propertyName="row">The row that is being attached to the <see cref="CellsPanel"/></param>
		protected internal virtual void OnAttached(RowBase row)
		{
			this._row = row;
            this.DataContext = this._row.Data;
		}

		#endregion // OnAttached

		#region OnReleased

		/// <summary>
		/// Called when the <see cref="RowBase"/> releases the <see cref="CellsPanel"/>.
		/// </summary>
		protected internal virtual void OnReleased()
		{
			this._row = null;
            this.DataContext = null;

			foreach (CellBase cell in this._visibleFixedLeftCells)
			{
				this.ReleaseCell(cell);
			}

			foreach (CellBase cell in this._visibleCells)
			{
				this.ReleaseCell(cell);
			}

			foreach (CellBase cell in this._visibleFixedRightCells)
			{
				this.ReleaseCell(cell);
			}

			foreach (CellBase cell in this._additionalCells)
			{
				this.ReleaseCell(cell);
			}

			this._additionalCells.Clear();
			this._visibleCells.Clear();
			this._visibleFixedLeftCells.Clear();
			this._visibleFixedRightCells.Clear();
			this._lastCellsDetached.Clear();
		}

		#endregion // OnReleased

		#endregion // Public

		#region Protected

        #region RenderCellsComplex

        private Size RenderCellsComplex(double availableWidth)
		{
			double maxHeight = 0;

			RowsManagerBase manager = this.Row.Manager;
			ReadOnlyCollection<Column> visibleColumns = this.Row.Columns.VisibleColumns;

			bool isInfinite = double.IsPositiveInfinity(availableWidth);

			int colCount = visibleColumns.Count;

			Collection<CellBase> starColumns = new Collection<CellBase>();

			double indentation = (manager.IsFirstRowRenderingInThisLayoutCycle)? manager.ResolveIndentation(this.Row) : manager.CachedIndentation;
			manager.CachedIndentation = indentation;

			double currentWidth = indentation;
			double fixedCellWidth = 0;

            // TFS117323 - If the visibleColumns collection doesn't contain the FillerColumn,
            // let's reset its ActualWidth so it won't interfere with any calculations
            var fillerColumn = visibleColumns.Where(i => i is FillerColumn);

            if(!fillerColumn.Any())
            {
                this.Row.Columns.FillerColumn.ActualWidth = 0;
            }

			// Render Fixed Adorner Columns First. 
			ReadOnlyCollection<Column> fixedColumns = this.Row.Columns.FixedAdornerColumns;
			foreach (Column column in fixedColumns)
			{
				if (currentWidth >= availableWidth)
					break;

				currentWidth += this.RenderCell(column, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);
			}

            bool visibleFixedColumn = false;

            // Place MergedColumns to the Left of the FixedDataColumns.
            if (this.Row.ColumnLayout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
            {
                ReadOnlyCollection<Column> mergedColumns = this.Row.Columns.GroupByColumns[this.Row.ColumnLayout];
                foreach (Column column in mergedColumns)
                {
                    if (column.Visibility == System.Windows.Visibility.Visible)
                    {
                        visibleFixedColumn = true;

                        if (currentWidth >= availableWidth)
                            break;

                        currentWidth += this.RenderCell(column, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);
                    }
                }
            }

			// Next Render Fixed Data Columns
			FixedColumnsCollection fixedDataColumns = this.Row.Columns.FixedColumnsLeft;
			if (fixedDataColumns.Count > 0)
			{
				
				foreach (Column column in fixedDataColumns)
				{
					if (column.Visibility == Visibility.Visible)
					{
						visibleFixedColumn = true;

						if (currentWidth >= availableWidth)
							break;

						currentWidth += this.RenderCell(column, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);
					}
				}
			}

            if (visibleFixedColumn)
            {
                // Reset
                this.Row.Columns.FixedBorderColumnLeft.ActualWidth = 0;
                currentWidth += this.RenderCell(this.Row.Columns.FixedBorderColumnLeft, starColumns, ref maxHeight, false, this._additionalCells, isInfinite);
                visibleFixedColumn = false;
            }

			fixedDataColumns = this.Row.Columns.FixedColumnsRight;
			if (fixedDataColumns.Count > 0)
			{
				foreach (Column column in fixedDataColumns)
				{
					if (column.Visibility == Visibility.Visible)
					{
						visibleFixedColumn = true;
						if (currentWidth >= availableWidth)
							break;

						currentWidth += this.RenderCell(column, starColumns, ref maxHeight, false, this.VisibleFixedRightCells, isInfinite);
					}
				}

                if (visibleFixedColumn)
                {
                    // Reset
                    this.Row.Columns.FixedBorderColumnRight.ActualWidth = 0;
                    currentWidth += this.RenderCell(this.Row.Columns.FixedBorderColumnRight, starColumns, ref maxHeight, false, this._additionalCells, isInfinite);
                }
			}

			fixedCellWidth = currentWidth - indentation;

			int startCell = 0, currentCell = 0;
			double percentScroll = 0;
			double cellWidth = 0;

			double scrollLeft = this.ResolveScrollLeft();

			if (scrollLeft == -1)
			{
				// This must be the first time we're rendering a row on this level, however, we are already horizontal scrolled
				// so lets resolve our HorizontalMax, and try again. 
				if (this.Row.RowType == RowType.DataRow)
				{
					this.InvalidateHorizontalMax(availableWidth - (fixedCellWidth + indentation), ref maxHeight, isInfinite, false);
					scrollLeft = this.ResolveScrollLeft();
					this.Owner.MeasureCalled = false;
					this.Owner.InvalidateMeasure();
				}
				else
				{
					this.InvalidateHorizontalMax(availableWidth - (fixedCellWidth + indentation), ref maxHeight, isInfinite, false);
					scrollLeft = this.ResolveScrollLeft();
					if (scrollLeft != 0)
						manager.InvalidateOverrideHorizontalMax = true;
				}
			}

			if (!this.Owner.ReverseCellMeasure)
			{
				// Add First Cell

				startCell = (int)scrollLeft;
                
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                startCell = startCell < 0 ? 0 : startCell;

				if (startCell < visibleColumns.Count)
				{
					Column firstColumn = visibleColumns[startCell];
					cellWidth = this.RenderCell(firstColumn, starColumns, ref maxHeight, false, this.VisibleCells, isInfinite);
					currentWidth += cellWidth;

					// Calculate PercentScroll
					double percent = scrollLeft - (int)scrollLeft;
					percentScroll = cellWidth * percent;
					currentWidth -= percentScroll;

					// Add Cells until there is no more width left
					double currentCellWidth = 0;
					for (currentCell = startCell + 1; currentCell < colCount; currentCell++)
					{
						if (currentWidth >= availableWidth)
							break;

						Column col = visibleColumns[currentCell];
						currentCellWidth = this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleCells, isInfinite);
						currentWidth += currentCellWidth;
					}

					// Add the percent scroll back, so that we can truly validate if we've scrolled past the last item.
					currentWidth += percentScroll;
				}

				startCell--;


				// Render StarCells
				// This needs to be done ealier than we previously used it, b/c now it returns a width, b/c
				// StartColumns can effect the scrolling width now, due to the fact that they support MinWidth
				currentWidth = this.SetupStarCells(availableWidth, currentWidth, ref maxHeight, starColumns);
			}
			else
			{
				startCell = this.Owner.ReverCellStartIndex;

				int visCellCount = this.Row.VisibleCells.Count;
				int visColCount = visibleColumns.Count;

				// Check and make sure that the startCell isn'type greater than the visible cells. 
				// This would happen if the cell scrolling into view, is on a different band. 
				if (startCell >= visCellCount)
					startCell = visCellCount - 1;
				




                if (startCell >= visColCount)
					startCell = visColCount - 1;
			}


			// If the width of all the visible cells is less then whats available in the viewport, and there are more cells in the 
			// collection, it means we've scrolled further than we needed to. Since we don't want whitespace to appear after 
			// the last cell, lets add more cells to the front.
			if (currentWidth < availableWidth && this._visibleCells.Count < colCount)
			{
				for (currentCell = startCell; currentCell >= 0; currentCell--)
				{
					Column c = visibleColumns[currentCell];
                    
                    // FillerColumn shouldn't have an influence on the determination if percentScroll should be reset to zero.
                    if (c is FillerColumn)
                        startCell--;

					cellWidth = this.RenderCell(c, starColumns, ref maxHeight, true, this.VisibleCells, isInfinite);
					currentWidth += cellWidth;                    

					if (currentWidth >= availableWidth)
					{
						if (this.Owner.ReverseCellMeasure)
						{
                            // If manager.OverflowAdjustment is zero, assume it hasn't been set. 
                            if (manager.OverflowAdjustment == 0)
                                manager.OverflowAdjustment = currentWidth - availableWidth;
                            else
    							manager.OverflowAdjustment = Math.Min(manager.OverflowAdjustment, (currentWidth - availableWidth));

							percentScroll = (manager.OverflowAdjustment / cellWidth);
                            if (currentCell == startCell)
                                percentScroll = 0;

							if (this.Row.ColumnLayout == this.Owner.ReverseColumnLayout)
							{
                                this.InvalidateHorizontalMax(availableWidth - (fixedCellWidth + indentation), ref maxHeight, isInfinite, false);

                                // This is for GroupCells
                                if (this.Owner.ReverseChildColumn != null && this.Owner.ReverseChildColumn.ParentColumn != null && startCell == this.Owner.ReverCellStartIndex)
                                {
                                    if (!double.IsPositiveInfinity(availableWidth))
                                    {
                                        Column rootColumn = visibleColumns[startCell];
                                        CellBase childCell = this.Row.Cells[this.Owner.ReverseChildColumn];
                                        double x = this.ResolveLeftForChildColumn(this.Owner.ReverseChildColumn, rootColumn);
                                        double childWidth = childCell.Column.ActualWidth;

                                        double current = (availableWidth - childWidth);
                                        double val = x - current;

                                        if (val < 0)
                                        {
                                            Column curCol = null;
                                            int i = 0;
                                            current -= x;

                                            for (i = startCell - 1; i >= 0; i--)
                                            {
                                                curCol = visibleColumns[i];

                                                if (curCol.ActualWidth >= current)
                                                    break;

                                                current -= curCol.ActualWidth;
                                            }

                                            if (curCol != null)
                                            {
                                                double percent = 1 - current / curCol.ActualWidth;
                                                this.SetScrollLeft(Math.Abs(i + percent));
                                            }
                                        }
                                        else
                                        {
                                            double percent = val / rootColumn.ActualWidth;
                                            this.SetScrollLeft(Math.Abs(startCell + percent));
                                        }
                                    }
                                }
                                else // This is for normal cells.
                                {
                                    this.SetScrollLeft(currentCell + percentScroll);
                                }
							}
						}
						break;
					}
				}
			}

			// Reverse rendering never takes into account the FillerCell. So lets add a seperate check and make sure that we render
			// it if its needed.
			if (this.Owner.ReverseCellMeasure)
			{
				// Render StarCells
				// This needs to be done ealier than we previously used it, b/c now it returns a width, b/c
				// StartColumns can effect the scrolling width now, due to the fact that they support MinWidth
				currentWidth = this.SetupStarCells(availableWidth, currentWidth, ref maxHeight, starColumns);

				if (currentWidth < availableWidth && visibleColumns.Contains(this.Row.Columns.FillerColumn))
				{
					this.RenderCell(this.Row.Columns.FillerColumn, starColumns, ref maxHeight, false, this.VisibleCells, isInfinite);
				}
			}

			//// Start from the last cell, and work our way left, until we figure out how many cells
			//// can fit in the viewport, so that the horizontal max for horizontal scrolling can be set.
			this.InvalidateHorizontalMax(availableWidth - (fixedCellWidth + indentation), ref maxHeight, isInfinite, true);

			// So, now we're storing the overflow adjustment on the rows manager. 
			// This will ensure that they're the same for ever row on a band, so that the header is never off
			// when doing auto calculations. 
			manager.OverflowAdjustment = Math.Max(manager.OverflowAdjustment, (currentWidth - availableWidth));

			// don't count the FillerColumn.
			if (isInfinite)
				colCount--;

			manager.ScrollableCellCount = colCount;

			manager.VisibleCellCount = this._visibleCells.Count;
			manager.CurrentVisibleWidth = currentWidth - indentation - fixedCellWidth;

			manager.IndexOfFirstColumnRendered = startCell+ 1;

			manager.TotalColumnsRendered = this.VisibleCells.Count;
			manager.RowWidth = currentWidth;

			return new Size(currentWidth, maxHeight);
		}

        #endregion // RenderCellsComplex

        #region RenderCells

        /// <summary>
		/// Lays out which cells will be displayed in the given viewport. 
		/// </summary>
		/// <param propertyName="availableWidth">The total width that the cells have to work with.</param>
		protected virtual Size RenderCells(double availableWidth)
		{
			RowsManagerBase manager = this.Row.Manager;
			Size returnSize;
			if (manager.IsFirstRowRenderingInThisLayoutCycle || this.Owner.ReverseCellMeasure)
			{
				returnSize = this.RenderCellsComplex(availableWidth);
				manager.IsFirstRowRenderingInThisLayoutCycle = false;
			}
			else
			{
                if (manager.InvalidateOverrideHorizontalMax)
                {
                    manager.InvalidateOverrideHorizontalMax = false;
                    manager.OverrideHorizontalMax = -1;
                }

				bool isInfinite = double.IsPositiveInfinity(availableWidth);

				ReadOnlyCollection<Column> visibleColumns = this.Row.Columns.VisibleColumns;
				double maxHeight = 0;
				Collection<CellBase> starColumns = new Collection<CellBase>();

                double width = manager.CachedIndentation; 

				// Render Fixed Adorner Columns First. 
				ReadOnlyCollection<Column> fixedColumns = this.Row.Columns.FixedAdornerColumns;
				foreach (Column col in fixedColumns)
				{
					double currentActualWidth = col.ActualWidth; 

					this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);

					if (currentActualWidth != col.ActualWidth)
					{
						this.VisibleFixedLeftCells.Clear();
						return this.RenderCellsComplex(availableWidth);
					}

                    if (col.WidthResolved.WidthType != ColumnWidthType.Star)
                        width += currentActualWidth;
				}

                bool visibleFixedColumn = false;

                // Place MergedColumns to the Left of the FixedDataColumns.
                if (this.Row.ColumnLayout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                {
                    ReadOnlyCollection<Column> mergedColumns = this.Row.Columns.GroupByColumns[this.Row.ColumnLayout];
                    foreach (Column col in mergedColumns)
                    {
                        if (col.Visibility == System.Windows.Visibility.Visible)
                        {
                            visibleFixedColumn = true;

                            double currentActualWidth = col.ActualWidth;

                            this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);

                            if (currentActualWidth != col.ActualWidth)
                            {
                                this.VisibleFixedLeftCells.Clear();
                                return this.RenderCellsComplex(availableWidth);
                            }

                            if (col.WidthResolved.WidthType != ColumnWidthType.Star)
                                width += currentActualWidth;
                        }
                    }
                }

				// Next Render Fixed Data Columns
				FixedColumnsCollection fixedDataColumns = this.Row.Columns.FixedColumnsLeft;
				if (fixedDataColumns.Count > 0)
				{
					
					foreach (Column col in fixedDataColumns)
					{
						if (col.Visibility == Visibility.Visible)
						{
							double currentActualWidth = col.ActualWidth; 

							visibleFixedColumn = true;
							this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);

							if (currentActualWidth != col.ActualWidth)
							{
                                this.ReleaseCells(this.VisibleFixedLeftCells);
								return this.RenderCellsComplex(availableWidth);
							}

                            if (col.WidthResolved.WidthType != ColumnWidthType.Star)
                                width += currentActualWidth;
						}
					}
				}

                if (visibleFixedColumn)
                {
                    // Reset
                    this.Row.Columns.FixedBorderColumnLeft.ActualWidth = 0;

                    width += this.RenderCell(this.Row.Columns.FixedBorderColumnLeft, starColumns, ref maxHeight, false, this._additionalCells, isInfinite);
                }

                double scrollLeft = this.ResolveScrollLeft();

                if (scrollLeft == -1)
                {
                    // This must be the first time we're rendering a row on this level, however, we are already horizontal scrolled
                    // so lets resolve our HorizontalMax, and try again. 
                    if (this.Row.RowType == RowType.DataRow)
                    {
                        this.InvalidateHorizontalMax(availableWidth - width, ref maxHeight, isInfinite, false);
                        scrollLeft = this.ResolveScrollLeft();
                        this.Owner.MeasureCalled = false;
                        this.Owner.InvalidateMeasure();
                    }
                }

				fixedDataColumns = this.Row.Columns.FixedColumnsRight;
				if (fixedDataColumns.Count > 0)
				{
					visibleFixedColumn = false;
					foreach (Column col in fixedDataColumns)
					{
						if (col.Visibility == Visibility.Visible)
						{
							double currentActualWidth = col.ActualWidth; 

							visibleFixedColumn = true;

							this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleFixedRightCells, isInfinite);

							if (currentActualWidth != col.ActualWidth)
							{
                                this.ReleaseCells(this.VisibleFixedLeftCells);
                                this.ReleaseCells(this.VisibleFixedRightCells);
								return this.RenderCellsComplex(availableWidth);
							}

                            if (col.WidthResolved.WidthType != ColumnWidthType.Star)
                                width += currentActualWidth;
						}
					}

                    if (visibleFixedColumn)
                    {
                        // Reset
                        this.Row.Columns.FixedBorderColumnRight.ActualWidth = 0;

                        width += this.RenderCell(this.Row.Columns.FixedBorderColumnRight, starColumns, ref maxHeight, false, this._additionalCells, isInfinite);
                    }
				}


				int count = manager.TotalColumnsRendered + manager.IndexOfFirstColumnRendered;

                if (count > visibleColumns.Count)
                    count = visibleColumns.Count;

                bool isConditionalFormatting = manager.ColumnLayout != null && manager.ColumnLayout.Grid != null && manager.ColumnLayout.Grid.ConditionalFormattingSettings.AllowConditionalFormatting;

				for (int i = manager.IndexOfFirstColumnRendered; i < count; i++)
				{
					Column col = visibleColumns[i];
					
					double currentActualWidth = col.ActualWidth; 

					this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleCells, isInfinite);

					if (currentActualWidth != col.ActualWidth)
					{
                        this.ReleaseCells(this.VisibleFixedLeftCells);
                        this.ReleaseCells(this.VisibleFixedRightCells);
                        this.ReleaseCells(this.VisibleCells);

                        if (isConditionalFormatting)
                        {
                            this.DataContext = null;
                            this.DataContext = this.Row.Data;
                        }

                        return this.RenderCellsComplex(availableWidth);
					}

                    if(col.WidthResolved.WidthType != ColumnWidthType.Star)
                        width += currentActualWidth;
				}

                manager.RowWidth = width;

                // Render StarCells
                this.SetupStarCells(availableWidth, manager.RowWidth, ref maxHeight, starColumns);

				returnSize = new Size(manager.RowWidth, maxHeight);
			}

			return returnSize;
		}

		#endregion // RenderCells			

        #region RenderCell

        /// <summary>
        /// Displays the <see cref="CellBase"/> for the specified <see cref="ColumnBase"/>.
        /// </summary>
        /// <param name="column">The <see cref="Column"/></param>
        /// <param name="starColumns">A list of cells that have a width of type star.</param>
        /// <param name="maxHeight">The height of the largest cell, if the cell's height that is being rendered is larger, maxHeight should be adjusted.</param>
        /// <param name="insert">Whether or not the cell should be added, or inserted at the first position of the specified visible cells.</param>
        /// <param name="visibleCells">The collection of cells that rendered cell should be added to.</param>
        /// <param name="isInfinite">Lets the method know if the available width is infinite.</param>
        /// <returns>The width that that <see cref="CellBase"/> is consuming.</returns>
        protected virtual double RenderCell(Column column, Collection<CellBase> starColumns, ref double maxHeight, bool insert, Collection<CellBase> visibleCells, bool isInfinite)
        {
            return this.RenderCell(column, starColumns, ref maxHeight, insert, visibleCells, isInfinite, false);
        }

        /// <summary>
        /// Displays the <see cref="CellBase"/> for the specified <see cref="ColumnBase"/>.
        /// </summary>
        /// <param name="column">The <see cref="Column"/></param>
        /// <param name="starColumns">A list of cells that have a width of type star.</param>
        /// <param name="maxHeight">The height of the largest cell, if the cell's height that is being rendered is larger, maxHeight should be adjusted.</param>
        /// <param name="insert">Whether or not the cell should be added, or inserted at the first position of the specified visible cells.</param>
        /// <param name="visibleCells">The collection of cells that rendered cell should be added to.</param>
        /// <param name="isInfinite">Lets the method know if the available width is infinite.</param>
        /// <param name="suppressCellControlAttached">Whether or not the CellControlAttached event should be fired when the CellControl is attached.</param>
        /// <returns>The width that that <see cref="CellBase"/> is consuming.</returns>
        protected virtual double RenderCell(Column column, Collection<CellBase> starColumns, ref double maxHeight, bool insert, Collection<CellBase> visibleCells, bool isInfinite, bool suppressCellControlAttached)
		{
			double prevActualWidth = column.ActualWidth;

			

			ColumnWidth colWidth = column.WidthResolved;
			ColumnWidthType widthType = colWidth.WidthType;

			double widthToMeasure = double.PositiveInfinity;

			if (widthType == ColumnWidthType.Numeric)
				widthToMeasure = colWidth.Value;
            else if (isInfinite && widthType == ColumnWidthType.Star)
                widthType = ColumnWidthType.Auto;
            else if (widthType == ColumnWidthType.InitialAuto && column.IsInitialAutoSet && column.ActualWidth != 0)
                widthToMeasure = column.ActualWidth;

			

			double heightToMeasure = double.PositiveInfinity;
			RowHeight rowHeight = this.Row.HeightResolved;
			if (rowHeight.HeightType == RowHeightType.Numeric)
				heightToMeasure = rowHeight.Value;

			if (column.Visibility == Visibility.Collapsed)
				return 0;

			if (isInfinite && column is FillerColumn)
				return 0;

			CellBase cell = this.Row.ResolveCell(column);
			if (cell != null)
			{
				if(!insert)
					visibleCells.Add(cell);
				else
					visibleCells.Insert(0, cell);

                Size sizeToMeasure = Size.Empty;

                bool manuallyInvokeMeasure = false; 

				if (cell.Control == null)
				{
				    cell.SuppressCellControlAttached = suppressCellControlAttached;
					RecyclingManager.Manager.AttachElement(cell, this);
					cell.Control.EnsureContent();
                    cell.EnsureCurrentState();
					cell.MeasuringSize = Size.Empty;
                    sizeToMeasure = new Size(widthToMeasure, heightToMeasure);
                   	
				}
				else
				{
					cell.ApplyStyle();
					cell.Control.EnsureContent();
                    cell.EnsureCurrentState();

                    if (cell.MeasuringSize.IsEmpty)
                        sizeToMeasure = new Size(widthToMeasure, heightToMeasure);
                    else
                        sizeToMeasure = cell.MeasuringSize;

                    manuallyInvokeMeasure = true;
				}

                if (widthType != ColumnWidthType.Star)
                {
                    cell.Control.MeasureRaised = false;
                    cell.Control.Measure(sizeToMeasure);

                    // Check to see that cell was actually measured.
                    // If measure wasn't called, that's our first clue, as this cell was just attached. 
                    if (!cell.Control.MeasureRaised)
                    {
                        // If it's content is smaller than the desired size, thats our second clue.
                        FrameworkElement elem = cell.Control.Content as FrameworkElement;

                        // In some cases, depeneding on what type of element we're dealing with, the elements don't always invalidate properly
                        // So i added another case, which is kind of random, but if the width of the element is less than 10, then we're going to invalidate it.
                        if (elem != null && ((elem.DesiredSize.Width > cell.Control.DesiredSize.Width) || elem.DesiredSize.Width < 10))
                        {
                            // So, set an invalid size first
                            cell.Control.Measure(new Size(1, 1));

                            // Then reapply the valid size, this will ensure that measure is called. 
                            cell.Control.Measure(sizeToMeasure);
                        }

                        if (manuallyInvokeMeasure)
                            cell.Control.ManuallyInvokeMeasure(sizeToMeasure);
                    }
                }				

				CellControlBase control = cell.Control;
                double width = control.DesiredSize.Width;

                // Added some Validation for Columns what support Overflow tooltips
                if (column.CachedAllowToolTips == AllowToolTips.Overflow)
                {
                    CellControl cc = control as CellControl;
                    // Only supported on Cells
                    if (cc != null)
                    {
                        // Make sure we didn't alreayd measure agains it's max
                        if ((widthToMeasure != double.PositiveInfinity || heightToMeasure != double.PositiveInfinity) && cell.MeasuringSize.IsEmpty)
                        {
                            // Measure against it's max
                            control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                            // Store off value
                            double desiredWidth = control.DesiredSize.Width;

                            // Measure against its reality
                            control.Measure(new Size(widthToMeasure, heightToMeasure));

                            // Compare
                            if (desiredWidth > width && (double.IsPositiveInfinity(widthToMeasure) || widthToMeasure < desiredWidth))
                            {
                                cc.ShowToolTip();
                            }
                            else
                            {
                                cc.HideToolTip();
                            }
                        }
                    }

                }
				
				if (isInfinite && widthType == ColumnWidthType.Star)
				{
					widthType = ColumnWidthType.Auto;
					if (width < control.ActualWidth)
						width = control.ActualWidth;
					
				}

				switch (widthType)
				{
					case ColumnWidthType.InitialAuto:

						if (column.ActualWidth == 0)
							column.IsInitialAutoSet = false;

						if (!column.IsInitialAutoSet && column.ActualWidth < width)
							column.ActualWidth = width;
						break;

					case ColumnWidthType.Auto:

						if (column.ActualWidth < width)
							column.ActualWidth = width;
						break;

					case ColumnWidthType.Numeric:

                        column.ActualWidth = cell.Column.WidthResolved.Value;

						break;

					case ColumnWidthType.SizeToHeader:

                        if (this.Row.RowType == RowType.HeaderRow)
                        {
                            if (column.ActualWidth < width)
                                column.ActualWidth = width;
                        }

						break;

					case ColumnWidthType.SizeToCells:

                        if (!(this.Row.RowType == RowType.HeaderRow) && !(this.Row.RowType == RowType.FooterRow))
						{
							if (column.ActualWidth < width)
								column.ActualWidth = width;
						}

						break;

					case ColumnWidthType.Star:

						if (starColumns != null)
							starColumns.Add(cell);

						return 0;
				}

				if (column.ActualWidth < column.MinimumWidth)
					column.ActualWidth = column.MinimumWidth;

				if (column.ActualWidth > column.MaximumWidth)
					column.ActualWidth = column.MaximumWidth;

				// So the Width of a Column has changed...better make sure that the HorizontalMax is still valid. 
                if (prevActualWidth != column.ActualWidth)
                {
                    if(!(column is FixedBorderColumn))
                        this.Row.Manager.OverrideHorizontalMax = -1;
                }

				maxHeight = Math.Max(maxHeight, cell.Control.DesiredSize.Height);
				return column.ActualWidth;
			}

			return 0;
		}

		#endregion // RenderCell		

		#region OnCellMouseOver

		/// <summary>
		/// Called when a <see cref="CellBase"/> is moused over.
		/// All Cells of the <see cref="RowBase"/> will then go to it's "MouseOver" VisualState.
		/// </summary>
		/// <param name="cell">The cell that was moused over.</param>
        protected virtual void OnCellMouseOver(CellBase cell)
        {
            ColumnLayout colLayout = cell.Row.ColumnLayout;

            //Determine if we have Hover Set for AllowEditing
            bool isHover = colLayout.Grid.EditingSettings.AllowEditing == EditingType.Hover;

            if (this.Row.ResolveRowHover == RowHoverType.Row)
            {
                foreach (CellBase visibleCell in this._visibleFixedLeftCells)
                    visibleCell.EnsureCurrentState();

                foreach (CellBase visibleCell in this._visibleCells)
                    visibleCell.EnsureCurrentState();

                foreach (CellBase visibleCell in this._visibleFixedRightCells)
                    visibleCell.EnsureCurrentState();

                if (isHover)
                {
                    //If hover is set, we need to see if there is actually an editable cell on this row.
                    Row row = this.Row as Row;

                    if (row != null)
                    {
                        CellBase editableCell = cell;
                        if (!cell.IsEditable)
                            editableCell = row.VisibleCells.Where(c => c.IsEditable).FirstOrDefault();

                        if (editableCell != null)
                        {
                            colLayout.Grid.EnterEditMode(row, editableCell);
                            editableCell.Control.ContentProvider.FocusEditor();
                        }
                            
                    }
                }
            }
            else if (this.Row.ResolveRowHover == RowHoverType.Cell)
            {
                cell.EnsureCurrentState();
                if (isHover)
                    colLayout.Grid.EnterEditMode(cell);
            }
        }

		#endregion // OnCellMouseOver

		#region OnCellMouseLeave
		
        /// <summary>
		/// Called when the mouse leaves a <see cref="CellBase"/>.
		/// All Cells of the <see cref="RowBase"/> will then go to it's "Normal" VisualState.
		/// </summary>
		/// <param name="cell">The cell that was moused over.</param>
        /// <param name="newCell">The new cell that is moused over.</param>
        protected virtual void OnCellMouseLeave(CellBase cell, CellBase newCell)
        {
            ColumnLayout colLayout = cell.Row.ColumnLayout;

            bool isHover = colLayout != null && colLayout.Grid.EditingSettings.AllowEditing == EditingType.Hover;

            if (this.Row.ResolveRowHover == RowHoverType.Row)
            {
                foreach (CellBase visibleCell in this._visibleFixedLeftCells)
                    visibleCell.EnsureCurrentState();

                foreach (CellBase visibleCell in this._visibleCells)
                    visibleCell.EnsureCurrentState();

                foreach (CellBase visibleCell in this._visibleFixedRightCells)
                    visibleCell.EnsureCurrentState();
            }
            else
                cell.EnsureCurrentState();

            


            if (isHover && colLayout.Grid.RowHover == RowHoverType.Cell)
            {
                colLayout.Grid.ExitEditModeInternal(false);
            }
        }

		#endregion // OnCellMouseLeave

		#region SetupStarCells

		/// <summary>
		/// Loops through the start columns and updates their width appropriately. 
		/// </summary>
		/// <param propertyName="availableWidth"></param>
		/// <param propertyName="currentWidth"></param>
		/// <param propertyName="maxHeight"></param>
		/// <param propertyName="starColumns"></param>
		protected double SetupStarCells(double availableWidth, double currentWidth, ref double maxHeight, Collection<CellBase> starColumns)
		{
			// Now that we've rendered all the cells, lets see if any of the cells are of type 
			// Star, and allow them to fill the remaining space.
			double remainingWidth = availableWidth - currentWidth;
			double usedWidth = remainingWidth;
			double remainder = 0;
			if (remainingWidth > 0)
			{
				if (starColumns.Count > 0)
				{
					double divider = 0;

					// Calculate the divider
					foreach (CellBase cell in starColumns)
					{
						divider += cell.Column.WidthResolved.Value;
					}

					if (divider != 0)
					{
						double individualWidth = remainingWidth / divider;

						bool reEvaluate = false;

						List<CellBase> actualStarCols = new List<CellBase>(starColumns);

						
						do
						{
							reEvaluate = false;
							List<CellBase> newList = new List<CellBase>();

							// Walk through all the start cells and evaluate whether they will participate in star sizing. 
							// If the their min width is larger than the available width alloted to it, then it will not particapte
							// and the remaining width and divider need to be updated appropriately. 
							// We need to do this in side a while stmt, so that we account for the new remaining width if it changes
							foreach (CellBase cell in actualStarCols)
							{
								double requestedValue = (int)individualWidth * cell.Column.WidthResolved.Value;
								if (requestedValue < cell.Column.MinimumWidth)
								{
									remainingWidth -= cell.Column.MinimumWidth;
									divider -= cell.Column.WidthResolved.Value;
									cell.Column.ActualWidth = cell.Column.MinimumWidth;
                                    
                                    if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.MinimumWidth);
                                    else
                                        this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.MinimumWidth;

									reEvaluate = true;
								}
								else
									newList.Add(cell);
							}

							actualStarCols = newList;
							individualWidth = remainingWidth / divider;

						} while (reEvaluate);

						if (remainingWidth > 0)
						{
							if (actualStarCols.Count > 0)
							{
								foreach (CellBase cell in actualStarCols)
								{
									// We don't want to add decimal values to the widths as it can mess with the way the cells look
									// So, let's store the remainder.
									cell.Column.ActualWidth = (int)individualWidth * cell.Column.WidthResolved.Value;
                                    
                                    if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                    else
                                        this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

									remainder += individualWidth - (int)individualWidth;
								}

								// Now that we have remaining pixels to apply, for every whole pixel, lets start from the begining and append
								// them to the width of the cell. 
								int counter = 0;
								while (remainder > 1)
								{
                                    CellBase cell = starColumns[counter];
									cell.Column.ActualWidth += 1;

                                    if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                    else
                                        this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

									counter++;
									remainder--;
								}

								// We'll add the last few decimal points to the last star column. 
								actualStarCols[actualStarCols.Count - 1].Column.ActualWidth += remainder;
							}
						}
						else
						{
							// After min widths were calculated, we ran out of width, so no star widths
							usedWidth = 0;
							foreach (CellBase cell in actualStarCols)
							{
								// These min width here is probably 0
								cell.Column.ActualWidth = cell.Column.MinimumWidth;

                                if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                    this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                else
                                    this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;
							}
						}

						// Now loop through and make sure these columns are measured correctly, so that all their content is visible.
						foreach (CellBase cell in starColumns)
						{
                            double width = 0;
                            if (this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                                width = this.Row.Manager.StarColumnWidths[cell.Column];

                            if (cell.Column.CachedAllowToolTips == AllowToolTips.Overflow)
                            {
                                CellControl cc = cell.Control as CellControl;
                                // Only supported on Cells
                                if (cc != null)
                                {
                                    // Measure against it's max
                                    cc.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                                    // Store off value
                                    double desiredWidth = cc.DesiredSize.Width;

                                    // Compare
                                    if (desiredWidth > width)
                                    {
                                        cc.ShowToolTip();
                                    }
                                    else
                                    {
                                        cc.HideToolTip();
                                    }
                                }
                            }

                            cell.Control.MeasureRaised = false;
                            cell.Control.Measure(new Size(width, double.PositiveInfinity));

                            // Check to see that cell was actually measured.
                            // If measure wasn't called, that's our first clue, as this cell was just attached. 
                            if (!cell.Control.MeasureRaised)
                            {
                                // If it's content is smaller than the desired size, thats our second clue.
                                FrameworkElement elem = cell.Control.Content as FrameworkElement;

                                // In some cases, depeneding on what type of element we're dealing with, the elements don't always invalidate properly
                                // So i added another case, which is kind of random, but if the width of the element is less than 10, then we're going to invalidate it.
                                if (elem != null && ((elem.DesiredSize.Width > cell.Control.DesiredSize.Width) || elem.DesiredSize.Width < 10))
                                {
                                    // So, set an invalid size first
                                    cell.Control.Measure(new Size(1, 1));

                                    // Then reapply the valid size, this will ensure that measure is called. 
                                    cell.Control.Measure(new Size(width, double.PositiveInfinity));
                                }
                            }

							
                            usedWidth -= width;
							maxHeight = Math.Max(maxHeight, cell.Control.DesiredSize.Height);
						}
					}
				}
			}
			else
			{
				// No remaining width?
				// Then we need to set each column's width to the minimum width (Not 0)
				usedWidth = 0; 
				foreach (CellBase cell in starColumns)
				{
					cell.Column.ActualWidth = cell.Column.MinimumWidth;

                    if (cell.Column.CachedAllowToolTips == AllowToolTips.Overflow)
                    {
                        CellControl cc = cell.Control as CellControl;
                        // Only supported on Cells
                        if (cc != null)
                        {
                            // Measure against it's max
                            cc.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                            // Store off value
                            double desiredWidth = cc.DesiredSize.Width;

                            // Compare
                            if (desiredWidth > cell.Column.ActualWidth)
                            {
                                cc.ShowToolTip();
                            }
                            else
                            {
                                cc.HideToolTip();
                            }
                        }
                    }


                    if (!this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                        this.Row.Manager.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                    else
                        this.Row.Manager.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

                    cell.Control.Measure(new Size(cell.Column.ActualWidth, double.PositiveInfinity));
                    maxHeight = Math.Max(maxHeight, cell.Control.DesiredSize.Height);

					usedWidth -= cell.Column.ActualWidth;
				}
				this.Row.Columns.FillerColumn.ActualWidth = 0; 
			}

			if (double.IsInfinity(usedWidth))
				usedWidth = 0; 

			return currentWidth + Math.Abs(usedWidth);
		}

		#endregion // SetupStarCells

		#region SupportsCommand

		/// <summary>
		/// Returns if the object will support a given command type.
		/// </summary>
		/// <param propertyName="command">The command to be validated.</param>
		/// <returns>True if the object recognizes the command as actionable against it.</returns>
		protected virtual bool SupportsCommand(ICommand command)
		{
			return (command is RowCommandBase);
		}
		#endregion // SupportsCommand

		#region  GetParameter
		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		protected virtual object GetParameter(CommandSource source)
		{
			return this.Row;
		}
		#endregion // GetParameter

		#endregion // Protected

		#region Private

		internal void ReleaseCell(CellBase cell)
		{
			if (cell.Control != null && cell.Control.Parent == this)
			{
                if(!RecyclingManager.Manager.ReleaseElement(cell, this))
				    this._nonReleasedCells.Add(cell);
			}
		}

        private void ReleaseCells(Collection<CellBase> cells)
        {
            foreach (CellBase cell in cells)
            {
                this.ReleaseCell(cell);
            }

            cells.Clear();
        }

		private double ResolveScrollLeft()
		{
			ScrollBar bar = this._scrollInfo.HorizontalScrollBar;
			
			if (bar != null)
			{
				RowsManagerBase horizontalManager = this.Owner.HorizontalRowsManager;
				
				if(horizontalManager == null || horizontalManager == this.Row.Manager)
					return bar.Value;

				if (bar.Value == 0 || bar.Maximum == 0)
				    return 0;

				double percent = bar.Value / bar.Maximum;

				if (this.Row.Manager.OverrideHorizontalMax == -1)
					return -1;

				double sl = this.Row.Manager.OverrideHorizontalMax * percent;

				if (sl < 0 || double.IsPositiveInfinity(sl) || double.IsNaN(sl))
					sl = 0;

				return sl;
			}

			return 0;
		}

		internal void SetScrollLeft(double scrollLeft)
		{
			ScrollBar bar = this._scrollInfo.HorizontalScrollBar;

			if (bar != null && this.Row.Manager.ScrollableCellCount - 1 > 0)
			{
				double value = 0;
				RowsManagerBase horizontalManager = this.Owner.HorizontalRowsManager;
				if (horizontalManager == null || horizontalManager == this.Row.Manager)
					value = scrollLeft;
				else
				{
					if (this.Row.Manager.OverrideHorizontalMax > bar.Maximum)
						bar.Maximum = this.Row.Manager.OverrideHorizontalMax;

					value = (scrollLeft / this.Row.Manager.OverrideHorizontalMax) * bar.Maximum;
				}

                if (double.IsNaN(value) || double.IsInfinity(value))
                    value = 0;

				bar.Value = value;
			}
		}

		private void InvalidateHorizontalMax(double availWidth, ref double maxHeight, bool isInfinite, bool includeStarCols)
		{
			if (this._prevAvailableWidth != availWidth)
				this.Row.Manager.OverrideHorizontalMax = -1;

			this._prevAvailableWidth = availWidth;

			// Start from the last cell, and work our way left, until we figure out how many cells
			// can fit in the viewport, so that the horizontal max for horizontal scrolling can be set.
            if (this.Row.Manager.OverrideHorizontalMax == -1 && !isInfinite)
            {
                List<CellBase> previouslyLastCells = new List<CellBase>(this._lastCellsDetached);
                this._lastCellsDetached.Clear();

                // So, if there is a star column, then we shouldn't set the HorizontalMax.
                foreach (Column col in this.Row.Columns.VisibleColumns)
                {
                    // Check to make sure its not a FillerColumn
                    // Check to see if its a Star Column
                    // Check to see that the column's actual width isn't its minwidth.
                    if (!(col is FillerColumn) && col.WidthResolved.WidthType == ColumnWidthType.Star && col.ActualWidth > col.MinimumWidth)
                    {
                        // If there isn't a control attached, then its safe to assume that we need to set the HorizontalMax
                        // As obviously there are cells left to be rendered, which means Horizontal scrollbar is needed. 
                        CellBase cell = this.Row.Cells[col];
                        if (cell.Control != null)
                        {
                            this.Row.Manager.OverrideHorizontalMax = -1;
                            return;
                        }
                    }
                }


                Collection<CellBase> fakeStarColumns = new Collection<CellBase>();

                double cellWidth = 0;
                double lastCellsWidth = 0;
                int currentLastCellIndex = this.Row.Columns.VisibleColumns.Count - 1;
                while (lastCellsWidth < availWidth && currentLastCellIndex >= 0)
                {
                    Column col = this.Row.Columns.VisibleColumns[currentLastCellIndex];

                    if (col.WidthResolved.WidthType != ColumnWidthType.Star)
                    {
                        cellWidth = this.RenderCell(col, fakeStarColumns, ref maxHeight, false, this._lastCellsDetached, isInfinite, true);
                    }
                    else if (includeStarCols && !(col is FillerColumn))
                    {
                        if (col.ActualWidth < col.MinimumWidth)
                            cellWidth = col.MinimumWidth; 
                        else
                            cellWidth = col.ActualWidth;
                    }

                    lastCellsWidth += cellWidth;
                    currentLastCellIndex--;
                }

                int count = this._lastCellsDetached.Count - 1;
                for (int i = count; i >= 0; i--)
                {
                    CellBase cell = this._lastCellsDetached[i];

                    cell.SuppressCellControlAttached = false;
   
                    if (this.VisibleCells.Contains(cell))
                    {
                        this._lastCellsDetached.Remove(cell);

                        Cell cellToAttach = cell as Cell;
                        ColumnLayout columnLayout = this.Row.Manager.ColumnLayout;

                        // We suppressed CellControlAttached for this cell. We need to make sure that
                        // CellControl attached is fired, because this cell is visible and we will use it.
                        if (cellToAttach != null && columnLayout != null && columnLayout.Grid != null)
                        {
                            this.Row.Manager.ColumnLayout.Grid.OnCellControlAttached(cellToAttach);
                        }
                    }
                    else
                    {
                        this.ReleaseCell(cell);
                    }
                }

                // We cache off the last rows, so that we don't have to keep sizing them to adjust
                // the Vertical scrollbar. But, something changed in the rows collection now, so lets
                // start over.
                foreach (CellBase cell in previouslyLastCells)
                {
                    if (!this.VisibleCells.Contains(cell) && !this.VisibleFixedLeftCells.Contains(cell) && !this.VisibleFixedRightCells.Contains(cell) && cell.Control != null)
                    {
                        this.ReleaseCell(cell);
                    }
                }

                currentLastCellIndex++;
                double sp = (lastCellsWidth - availWidth) / cellWidth;
                this.Row.Manager.OverrideHorizontalMax = currentLastCellIndex + sp;
                if (this.Row.Manager.OverrideHorizontalMax < 0)
                    this.Row.Manager.OverrideHorizontalMax = 0;
            }
		}

        private double ResolveLeftForChildColumn(Column childColumn, Column parentColumn)
        {
            double left = 0;

            ICollectionBase cols = parentColumn.ResolveChildColumns();
            if (cols != null)
            {
                foreach (Column column in cols)
                {
                    if (column == childColumn)
                        break;

                    ICollectionBase childCols = column.ResolveChildColumns();
                    if (childCols == null || childCols.Count == 0)
                    {
                        left += column.ActualWidth;
                    }
                    else
                    {
                        left += this.ResolveLeftForChildColumn(childColumn, column);

                        if (childCols.Contains(childColumn))
                            break;
                    }

                }
            }

            return left;
        }

		#endregion // Private

		#region Internal

		internal void InternalCellMouseEnter(CellBase cell)
		{
			this.OnCellMouseOver(cell);
		}

        internal void InternalCellMouseLeave(CellBase cell, CellBase newCell)
        {
            this.OnCellMouseLeave(cell, newCell);
        }

		internal void RenderCell(CellBase cell)
		{
			double maxHeight = 0;

            Column col = cell.Column;
            while (col.ParentColumn != null)
                col = col.ParentColumn;

			this.RenderCell(col, new Collection<CellBase>(), ref maxHeight, false, this._visibleCells, false);
		}

		#endregion // Internal

		#endregion // Methods

		#region Overrides

        #region MeasureOverride
        /// <summary>
		/// Provides the behavior for the "measure" pass of the <see cref="CellsPanel"/>.
		/// </summary>
		/// <param propertyName="availableSize">The available size that this object can give to child objects. Infinity can be specified
		/// as a value to indicate the object will size to whatever content is available.</param>
		/// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.Row == null)
                return base.MeasureOverride(availableSize);

            this.PreviousInvalidateHeight = availableSize.Height;

            // If the Owning Panel, didn'type invoke this measure, then we need to make sure that it does get measured, so that all of the cells 
            // In the visible grid have the same ColumnWidth.
            if (this.Owner != null && !this.Owner.InLayoutPhase)
                this.Owner.InvalidateMeasure();

            bool isInfinite = double.IsPositiveInfinity(availableSize.Width);

            List<CellBase> previousCells = new List<CellBase>(this._visibleCells);
            previousCells.AddRange(this._visibleFixedLeftCells);
            previousCells.AddRange(this._additionalCells);
            previousCells.AddRange(this._visibleFixedRightCells);
            this._additionalCells.Clear();
            this._visibleCells.Clear();
            this._visibleFixedLeftCells.Clear();
            this._visibleFixedRightCells.Clear();

            Size rowSize = this.RenderCells(availableSize.Width);

            switch (this.Row.HeightResolved.HeightType)
            {
                case RowHeightType.SizeToLargestCell:
                    this.Row.ActualHeight = rowSize.Height = Math.Max(Math.Max(this.Row.ActualHeight, rowSize.Height), this.Row.MinimumRowHeightResolved);
                    break;

                case RowHeightType.Dynamic:
                    rowSize.Height = Math.Max(rowSize.Height, this.Row.MinimumRowHeightResolved);
                    this.Row.ActualHeight = rowSize.Height;
                    break;

                case RowHeightType.Numeric:
                    this.Row.ActualHeight = rowSize.Height = this.Row.HeightResolved.Value;
                    break;
            }

            foreach (CellBase cell in previousCells)
            {
                if (!this._visibleCells.Contains(cell) && !this._visibleFixedLeftCells.Contains(cell) && !this._visibleFixedRightCells.Contains(cell) && !this._additionalCells.Contains(cell))
                {
                    this.ReleaseCell(cell);
                }
            }

            double width = availableSize.Width;
            if (isInfinite)
                width = rowSize.Width;

            return new Size(width, rowSize.Height);
        }

		#endregion // MeasureOverride

		#region ArrangeOverride

		/// <summary>
		/// Arranges each <see cref="CellBase"/> that should be visible, one next to  the other, similar to a 
		/// Horizontal <see cref="StackPanel"/>.
		/// </summary>
		/// <param propertyName="finalSize">
		/// The final area within the parent that this object 
		/// should use to arrange itself and its children.
		/// </param>
		/// <returns></returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			this.ArrangeRaised = true;

			if (this.Row == null)
				return finalSize;
			
			this.Owner.ReverCellStartIndex = -1;
			this.Owner.ReverseColumnLayout = null;
            this.Owner.ReverseChildColumn = null;

			// Move all Cells that aren't being used, out of view. 
            // Note: we have to use GetAvailElements, instead of recent, b/c if we get taken out of the visual tree,
            // and then added back, all elements that were out of view, will not be arranged where we had arranged them.
            List<FrameworkElement> unusedCells = RecyclingManager.Manager.GetAvailableElements(this);
			foreach (FrameworkElement cell in unusedCells)
			{
				cell.Arrange(this._hideCellRect);
			}

			// Some cells, like cells that are in edit mode, may choose to not get released. 
			// If thats the case, then we need to make sure that they get placed out of view. 
			foreach (CellBase cell in this._nonReleasedCells)
			{
				if (cell.Control != null)
					cell.Control.Arrange(this._hideCellRect);
			}

			this._nonReleasedCells.Clear();

			double left = this.Row.Manager.CachedIndentation;

			//Hide the Indentation.
            double clipWidth = finalSize.Width - left;
            if (clipWidth < 0)
                clipWidth = 0;
			this._clipRG.Rect = new Rect(left, 0, clipWidth, finalSize.Height);

            bool showFixedSep = false;

			// Position the Left Fixed Cells.
			foreach (CellBase cell in this._visibleFixedLeftCells)
			{
				Column col = cell.Column;
				FrameworkElement elem = cell.Control;
				double actualWidth = col.ActualWidth;

                if (col.IsGroupBy || col.IsFixed == FixedState.Left)
                    showFixedSep = true;

				if (col != null && col.IsMoving)
					elem.Arrange(new Rect(left, 0, 0, 0));
				else
				{
                    double height = Math.Max(finalSize.Height, elem.DesiredSize.Height);

                    double leftVal = left;
					
					// An Animation must be occurring, so lets arrange these columns into place gradually.
					if (col != null && col.PercentMoved > 0)
					{
						if (!col.ReverseMove)
							leftVal = (left - col.MovingColumnsWidth) + (col.MovingColumnsWidth * col.PercentMoved);
						else
							leftVal = (left + col.MovingColumnsWidth) - (col.MovingColumnsWidth * col.PercentMoved);
					}

					elem.Arrange(new Rect(leftVal, 0, actualWidth, height));
				}
				
				left += actualWidth;
				Canvas.SetZIndex(elem, 1);
			}

			// Place an Indicator to seperate the Left Fixed Cells
            if (showFixedSep)
			{
				foreach (CellBase cell in this._additionalCells)
				{
					if(cell.Column == this.Row.Columns.FixedBorderColumnLeft)
					{
						FrameworkElement elem = cell.Control;
						double actualWidth = cell.Column.ActualWidth;
						elem.Arrange(new Rect(left, 0, actualWidth, finalSize.Height));
						left += actualWidth;
						Canvas.SetZIndex(elem, 1);
						break;
					}
				}
			}

			// Position the Right Fixed Cells.
			double right = finalSize.Width; 

			double fillerColumnWidth = this.Row.Columns.FillerColumn.ActualWidth;
			if (fillerColumnWidth > 0)
				right -= fillerColumnWidth;

			foreach (CellBase cell in this._visibleFixedRightCells)
			{
				Column col = cell.Column;
				FrameworkElement elem = cell.Control;
				double actualWidth = cell.Column.ActualWidth;
				double rightVal = right;
				right -= actualWidth;

				if (col != null && col.IsMoving)
					elem.Arrange(new Rect(right, 0, 0, 0));
				else
				{
					// An Animation must be occurring, so lets arrange these columns into place gradually.
					if (col != null && col.PercentMoved > 0)
					{
						if (!col.ReverseMove)
							rightVal = (rightVal + col.MovingColumnsWidth) - (col.MovingColumnsWidth * col.PercentMoved);
						else 
							rightVal = (rightVal - col.MovingColumnsWidth) + (col.MovingColumnsWidth * col.PercentMoved);
					}
					
					rightVal -= actualWidth;

					elem.Arrange(new Rect(rightVal, 0, actualWidth, Math.Max(finalSize.Height, elem.DesiredSize.Height)));
				}
				
				Canvas.SetZIndex(elem, 2);
			}

			// Place an Indicator to seperate the Right Fixed Cells
			if (this.Row.Columns.FixedColumnsRight.Count > 0)
			{
				foreach (CellBase cell in this._additionalCells)
				{
					if (cell.Column == this.Row.Columns.FixedBorderColumnRight)
					{
						FrameworkElement elem = cell.Control;
						double actualWidth = cell.Column.ActualWidth;
						right -= actualWidth;
						elem.Arrange(new Rect(right, 0, actualWidth, finalSize.Height));
						Canvas.SetZIndex(elem, 2);
						break;
					}
				}
			}

            if (this.Row.CanScrollHorizontally)
            {
                // Calculate the offset LeftValue, for the first cell 
                ScrollBar horiztonalSB = this._scrollInfo.HorizontalScrollBar;
                if (horiztonalSB != null && horiztonalSB.Visibility == Visibility.Visible)
                {
                    if (this.Row.Manager.OverrideHorizontalMax > 0)
                    {
                        // If the horizontal scrollbar is visible, it's safe to say that we don't need the FillerColumn
                        // if we don't do this, then there is a chance that the scroll left changed between measuring and arranging
                        // due to column's resizing. 
                        this.Row.Columns.FillerColumn.ActualWidth = 0;
                    }


                    if (horiztonalSB.Value != horiztonalSB.Maximum)
                    {
                        if (this._visibleCells.Count > 0)
                        {
                            double scrollLeft = this.ResolveScrollLeft();
                            double percent = scrollLeft - (int)scrollLeft;
                            double leftVal = this._visibleCells[0].Column.ActualWidth * percent;

                            if ((this.Row.Manager.CurrentVisibleWidth + left) > right)
                                left += -leftVal;
                        }
                    }
                    else
                    {
                        // We've reached the last child, so lets make sure its visible. 
                        if ((this.Row.Manager.CurrentVisibleWidth + left) > right)
                            left -= this.Row.Manager.OverflowAdjustment;
                    }

                }
            }

			// Position the Scrollable Cells
			foreach (CellBase cell in this._visibleCells)
			{
                Column col = cell.Column;

                if (!col.IsInitialAutoSet)
                {
                    bool ignoreAutoSet = false;
                 
                    RowsManager manager = this.Row.Manager as RowsManager;
                    if (manager != null)
                    {
                        ignoreAutoSet = manager.RegisteredTopRows.Contains(this.Row);

                        if (!ignoreAutoSet)
                            ignoreAutoSet = manager.RegisteredBottomRows.Contains(this.Row);
                    }

                    if (!this.Owner.ReverseCellMeasure && !ignoreAutoSet && cell.Control.IsCellLoaded)
                        col.IsInitialAutoSet = true;
                }

				CellControlBase elem = cell.Control;
				double actualWidth = cell.Column.ActualWidth;

                if (cell.Column.WidthResolved.WidthType == ColumnWidthType.Star)
                {
                    if (this.Row.Manager.StarColumnWidths.ContainsKey(cell.Column))
                        actualWidth = this.Row.Manager.StarColumnWidths[cell.Column];
                }

				if (col != null && col.IsMoving)
					elem.Arrange(new Rect(left, 0, 0, 0));
				else
				{
					double leftVal = left;
					// An Animation must be occurring, so lets arrange these columns into place gradually.
					if (col != null && col.PercentMoved > 0)
					{
						if (!col.ReverseMove)
							leftVal = (left - col.MovingColumnsWidth) + (col.MovingColumnsWidth * col.PercentMoved);
						else
							leftVal = (left + col.MovingColumnsWidth) - (col.MovingColumnsWidth * col.PercentMoved);
					}

					if (col is FillerColumn)
						leftVal = finalSize.Width - actualWidth;

					elem.Arrange(new Rect(leftVal, 0, actualWidth, Math.Max(finalSize.Height, elem.DesiredSize.Height)));
				}

				left += actualWidth;
				Canvas.SetZIndex(elem, 0);
			}
			
			return finalSize;
		}
		#endregion // ArrangeOverride

        #region OnCreateAutomationPeer
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new CellsPanelAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

		#endregion // Overrides

		#region ICommandTarget Members

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return this.SupportsCommand(command);
		}

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return this.GetParameter(source);
		}

		#endregion
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