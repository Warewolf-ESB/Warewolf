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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A panel that lays out <see cref="ComboCellControl"/> objects in a horizontal list.
    /// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class ComboCellsPanel : Panel
  {
		#region Members

		ComboRowBase _row;
		Collection<ComboCellBase> _visibleCells, _visibleFixedLeftCells, _visibleFixedRightCells, _additionalCells, _lastCellsDetached, _nonReleasedCells;
		MultiColumnComboItemsPanel _owner;
		XamMultiColumnComboEditor _comboEditor;
		RectangleGeometry _clipRG;
		double _prevAvailableWidth = -1;
		Rect _hideCellRect = new Rect(-1000, -1000, 0, 0);        

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ComboCellsPanel"/> class.
		/// </summary>
        public ComboCellsPanel()
		{
            this._visibleCells = new Collection<ComboCellBase>();
            this._visibleFixedLeftCells = new Collection<ComboCellBase>();
            this._visibleFixedRightCells = new Collection<ComboCellBase>();
            this._additionalCells = new Collection<ComboCellBase>();
			this._clipRG = new RectangleGeometry();
			this.Clip = this._clipRG;
            this._lastCellsDetached = new Collection<ComboCellBase>();
            this._nonReleasedCells = new Collection<ComboCellBase>();
		}

		#endregion // Constructor

		#region Properties

		#region Public	

		#region Row

		/// <summary>
        /// Gets a refernce to the <see cref="ComboRowBase"/> that owns the <see cref="ComboCellsPanel"/>.
		/// </summary>
		public ComboRowBase Row
		{
			get { return this._row; }
		}

		#endregion // Row

		#region Owner

		/// <summary>
        /// Gets/sets the MultiColumnComboItemsPanel that owns this control.
		/// </summary>
		public MultiColumnComboItemsPanel Owner
		{
			get { return this._owner; }
			set
			{
				this._owner = value;
                if (value != null)
                {
                    this._comboEditor = (XamMultiColumnComboEditor)this._owner.ComboEditor;
                }
                else
                {
                    this._comboEditor = null;
                }
			}
		}

		#endregion // Owner

		#endregion // Public

		#region Protected

		#region VisibleCells

		/// <summary>
        /// Gets a list of <see cref="ComboCellBase"/> objects thats are currently visible.
		/// </summary>
		protected internal Collection<ComboCellBase> VisibleCells
		{
			get { return this._visibleCells; }
		}

		#endregion // VisibleCells

		#region VisibleFixedLeftCells

		/// <summary>
        /// Gets a list of <see cref="ComboCellBase"/> objects that are fixed to the left.
		/// </summary>
        protected internal Collection<ComboCellBase> VisibleFixedLeftCells
		{
			get { return this._visibleFixedLeftCells; }
		}

		#endregion // VisibleFixedLeftCells

		#region VisibleFixedRightCells

		/// <summary>
        /// Gets a list of <see cref="ComboCellBase"/> objects that are fixed to the right. 
		/// </summary>
        protected internal Collection<ComboCellBase> VisibleFixedRightCells
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

        #region MeasureRaised

        internal bool MeasureRaised
        {
            get;
            set;
        }
        #endregion // MeasureRaised


        internal RectangleGeometry ClipRect
        {
            get { return this._clipRG; }
        }

		#endregion // Internal

		#endregion // Properties

		#region Methods

        #region Protected
        
        #region OnAttached

        /// <summary>
        /// Called when the <see cref="ComboRowBase"/> is attached to the <see cref="ComboCellsPanel"/>.
		/// </summary>
        /// <param propertyName="row">The row that is being attached to the <see cref="ComboCellsPanel"/></param>
		protected internal virtual void OnAttached(ComboRowBase row)
		{
			this._row = row;
            this.DataContext = this._row.Data;
		}

		#endregion // OnAttached

		#region OnReleased

		/// <summary>
        /// Called when the <see cref="ComboRowBase"/> releases the <see cref="ComboCellsPanel"/>.
		/// </summary>
        /// <param propertyName="row">The row that is being removed from the <see cref="ComboCellsPanel"/></param>
        protected internal virtual void OnReleased(ComboRowBase row)
		{
			this._row = null;
            this.DataContext = null;

			foreach (ComboCellBase cell in this._visibleFixedLeftCells)
			{
				this.ReleaseCell(cell);
			}

            foreach (ComboCellBase cell in this._visibleCells)
			{
				this.ReleaseCell(cell);
			}

            foreach (ComboCellBase cell in this._visibleFixedRightCells)
			{
				this.ReleaseCell(cell);
			}

            foreach (ComboCellBase cell in this._additionalCells)
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

        #region RenderCells

        /// <summary>
		/// Lays out which cells will be displayed in the given viewport. 
		/// </summary>
		/// <param propertyName="availableWidth">The total width that the cells have to work with.</param>
		protected virtual Size RenderCells(double availableWidth)
		{
			Size returnSize;
			if (this._comboEditor.IsFirstRowRenderingInThisLayoutCycle)
			{
				returnSize = this.RenderCellsComplex(availableWidth);
                this._comboEditor.IsFirstRowRenderingInThisLayoutCycle = false;
			}
			else
			{
                if (this._comboEditor.InvalidateOverrideHorizontalMax)
                {
                    this._comboEditor.InvalidateOverrideHorizontalMax = false;
                    this._comboEditor.OverrideHorizontalMax = -1;
                }

				bool isInfinite = double.IsPositiveInfinity(availableWidth);

				ReadOnlyCollection<ComboColumn> visibleColumns = this._comboEditor.Columns.VisibleColumns;
				double maxHeight = 0;
                Collection<ComboCellBase> starColumns = new Collection<ComboCellBase>();

                double width = 0; 

				// Render Fixed Adorner Columns First. 
                ReadOnlyCollection<ComboColumn> fixedColumns = this._comboEditor.Columns.FixedAdornerColumns;
                foreach (ComboColumn col in fixedColumns)
				{
					double currentActualWidth = col.ActualWidth; 

					this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);

					if (currentActualWidth != col.ActualWidth)
					{
						this.VisibleFixedLeftCells.Clear();
						return this.RenderCellsComplex(availableWidth);
					}

                    if (col.WidthResolved.WidthType != ComboColumnWidthType.Star)
                        width += currentActualWidth;
				}

				// Next Render Fixed Data Columns
				FixedComboColumnsCollection fixedDataColumns = this._comboEditor.Columns.FixedColumnsLeft;
				if (fixedDataColumns.Count > 0)
				{
					
					foreach (ComboColumn col in fixedDataColumns)
					{
						if (col.Visibility == Visibility.Visible)
						{
							double currentActualWidth = col.ActualWidth; 

							this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);

							if (currentActualWidth != col.ActualWidth)
							{
                                this.ReleaseCells(this.VisibleFixedLeftCells);
								return this.RenderCellsComplex(availableWidth);
							}

                            if (col.WidthResolved.WidthType != ComboColumnWidthType.Star)
                                width += currentActualWidth;
						}
					}
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

				fixedDataColumns = this._comboEditor.Columns.FixedColumnsRight;
				if (fixedDataColumns.Count > 0)
				{
					foreach (ComboColumn col in fixedDataColumns)
					{
						if (col.Visibility == Visibility.Visible)
						{
							double currentActualWidth = col.ActualWidth; 

							this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleFixedRightCells, isInfinite);

							if (currentActualWidth != col.ActualWidth)
							{
                                this.ReleaseCells(this.VisibleFixedLeftCells);
                                this.ReleaseCells(this.VisibleFixedRightCells);
								return this.RenderCellsComplex(availableWidth);
							}

                            if (col.WidthResolved.WidthType != ComboColumnWidthType.Star)
                                width += currentActualWidth;
						}
					}
				}


                int count = this._comboEditor.TotalColumnsRendered + this._comboEditor.IndexOfFirstColumnRendered;

                if (count > visibleColumns.Count)
                    count = visibleColumns.Count;

				for (int i = this._comboEditor.IndexOfFirstColumnRendered; i < count; i++)
				{
					ComboColumn col = visibleColumns[i];
					
					double currentActualWidth = col.ActualWidth; 

					this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleCells, isInfinite);

					if (currentActualWidth != col.ActualWidth)
					{
                        this.ReleaseCells(this.VisibleFixedLeftCells);
                        this.ReleaseCells(this.VisibleFixedRightCells);
                        this.ReleaseCells(this.VisibleCells);

                        return this.RenderCellsComplex(availableWidth);
					}

                    if(col.WidthResolved.WidthType != ComboColumnWidthType.Star)
                        width += currentActualWidth;
				}

                this._comboEditor.RowWidth = width;

                // Render StarCells
                this.SetupStarCells(availableWidth, this._comboEditor.RowWidth, ref maxHeight, starColumns);

                returnSize = new Size(this._comboEditor.RowWidth, maxHeight);
			}

			return returnSize;
		}

		#endregion // RenderCells			

		#region RenderCell

		/// <summary>
        /// Displays the <see cref="ComboCellBase"/> for the specified <see cref="ComboColumn"/>.
		/// </summary>
        /// <param propertyName="column">The <see cref="ComboColumn"/></param>
		/// <param propertyName="starColumns">A list of cells that have a width of type star.</param>
		/// <param propertyName="maxHeight">The height of the largest cell, if the cell's height that is being rendered is larger, maxHeight should be adjusted.</param>
		/// <param propertyName="insert">Whether or not the cell should be added, or inserted at the first position of the specified visible cells.</param>
		/// <param propertyName="visibleCells">The collection of cells that rendered cell should be added to.</param>
		/// <param propertyName="isInfinite">Lets the method know if the available width is infinite.</param>
        /// <returns>The width that that <see cref="ComboCellBase"/> is consuming.</returns>
        protected virtual double RenderCell(ComboColumn column, Collection<ComboCellBase> starColumns, ref double maxHeight, bool insert, Collection<ComboCellBase> visibleCells, bool isInfinite)
		{
			double prevActualWidth = column.ActualWidth;

			

			ComboColumnWidth colWidth = column.WidthResolved;
			ComboColumnWidthType widthType = colWidth.WidthType;

			double widthToMeasure = double.PositiveInfinity;

			if (widthType == ComboColumnWidthType.Numeric)
				widthToMeasure = colWidth.Value;
            else if (isInfinite && widthType == ComboColumnWidthType.Star)
                widthType = ComboColumnWidthType.Auto;
            else if (widthType == ComboColumnWidthType.InitialAuto && column.IsInitialAutoSet && column.ActualWidth != 0)
                widthToMeasure = column.ActualWidth;

			

			double heightToMeasure = double.PositiveInfinity;

			if (column.Visibility == Visibility.Collapsed)
				return 0;

			if (isInfinite && column is FillerComboColumn)
				return 0;

			ComboCellBase cell = this.Row.ResolveCell(column);
			if (cell != null)
			{
				if(!insert)
					visibleCells.Add(cell);
				else
					visibleCells.Insert(0, cell);

                Size sizeToMeasure = Size.Empty;

				if (cell.Control == null)
				{
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
				}

                if (widthType != ComboColumnWidthType.Star)
                {
                    cell.Control.IsMeasureRaised = false;
                    cell.Control.Measure(sizeToMeasure);

                    // Check to see that cell was actually measured.
                    // If measure wasn't called, that's our first clue, as this cell was just attached. 
                    if (!cell.Control.IsMeasureRaised)
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
                            cell.Control.Measure(new Size(widthToMeasure, heightToMeasure));
                        }
                    }
                }

				ComboCellControlBase control = cell.Control;
                double width = control.DesiredSize.Width;

				if (isInfinite && widthType == ComboColumnWidthType.Star)
				{
					widthType = ComboColumnWidthType.Auto;
					if (width < control.ActualWidth)
						width = control.ActualWidth;
					
				}

				switch (widthType)
				{
					case ComboColumnWidthType.InitialAuto:

						if (column.ActualWidth == 0)
							column.IsInitialAutoSet = false;

						if (!column.IsInitialAutoSet && column.ActualWidth < width)
							column.ActualWidth = width;
						break;

					case ComboColumnWidthType.Auto:

						if (column.ActualWidth < width)
							column.ActualWidth = width;
						break;

					case ComboColumnWidthType.Numeric:

                        column.ActualWidth = cell.Column.WidthResolved.Value;

						break;

					case ComboColumnWidthType.SizeToHeader:

                        if (this.Row.RowType == RowType.HeaderRow)
                        {
                            if (column.ActualWidth < width)
                                column.ActualWidth = width;
                        }

						break;

					case ComboColumnWidthType.SizeToCells:

                        if (!(this.Row.RowType == RowType.HeaderRow))
						{
							if (column.ActualWidth < width)
								column.ActualWidth = width;
						}

						break;

					case ComboColumnWidthType.Star:

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
                    this._comboEditor.OverrideHorizontalMax = -1;
                }

				maxHeight = Math.Max(maxHeight, cell.Control.DesiredSize.Height);
				return column.ActualWidth;
			}

			return 0;
		}

		#endregion // RenderCell		

		#region OnCellMouseOver

		/// <summary>
        /// Called when a <see cref="ComboCellBase"/> is moused over.
        /// All Cells of the <see cref="ComboRowBase"/> will then go to it's "MouseOver" VisualState.
		/// </summary>
        protected virtual void OnCellMouseOver()
        {
			this.InternalEnsureAllCellVisualStates();
		}

		#endregion // OnCellMouseOver

		#region OnCellMouseLeave
		/// <summary>
        /// Called when the mouse  leaves a <see cref="ComboCellBase"/>.
        /// All Cells of the <see cref="ComboRowBase"/> will then go to it's "Normal" VisualState.
		/// </summary>
        protected virtual void OnCellMouseLeave()
        {
			this.InternalEnsureAllCellVisualStates();
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
		protected double SetupStarCells(double availableWidth, double currentWidth, ref double maxHeight, Collection<ComboCellBase> starColumns)
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
					foreach (ComboCellBase cell in starColumns)
					{
						divider += cell.Column.WidthResolved.Value;
					}

					if (divider != 0)
					{
						double individualWidth = remainingWidth / divider;

						bool reEvaluate = false;

                        List<ComboCellBase> actualStarCols = new List<ComboCellBase>(starColumns);

						
						do
						{
							reEvaluate = false;
                            List<ComboCellBase> newList = new List<ComboCellBase>();

							// Walk through all the start cells and evaluate whether they will participate in star sizing. 
							// If the their min width is larger than the available width alloted to it, then it will not particapte
							// and the remaining width and divider need to be updated appropriately. 
							// We need to do this in side a while stmt, so that we account for the new remaining width if it changes
                            foreach (ComboCellBase cell in actualStarCols)
							{
								double requestedValue = (int)individualWidth * cell.Column.WidthResolved.Value;
								if (requestedValue < cell.Column.MinimumWidth)
								{
									remainingWidth -= cell.Column.MinimumWidth;
									divider -= cell.Column.WidthResolved.Value;
									cell.Column.ActualWidth = cell.Column.MinimumWidth;

                                    if (!this.Row.ComboEditor.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.ComboEditor.StarColumnWidths.Add(cell.Column, cell.Column.MinimumWidth);
                                    else
                                        this.Row.ComboEditor.StarColumnWidths[cell.Column] = cell.Column.MinimumWidth;

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
								foreach (ComboCellBase cell in actualStarCols)
								{
									// We don't want to add decimal values to the widths as it can mess with the way the cells look
									// So, let's store the remainder.
									cell.Column.ActualWidth = (int)individualWidth * cell.Column.WidthResolved.Value;

                                    if (!this.Row.ComboEditor.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.ComboEditor.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                    else
                                        this.Row.ComboEditor.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

									remainder += individualWidth - (int)individualWidth;
								}

								// Now that we have remaining pixels to apply, for every whole pixel, lets start from the begining and append
								// them to the width of the cell. 
								int counter = 0;
								while (remainder > 1)
								{
                                    ComboCellBase cell = starColumns[counter];
									cell.Column.ActualWidth += 1;

                                    if (!this.Row.ComboEditor.StarColumnWidths.ContainsKey(cell.Column))
                                        this.Row.ComboEditor.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                    else
                                        this.Row.ComboEditor.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

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
							foreach (ComboCellBase cell in actualStarCols)
							{
								// These min width here is probably 0
								cell.Column.ActualWidth = cell.Column.MinimumWidth;

                                if (!this.Row.ComboEditor.StarColumnWidths.ContainsKey(cell.Column))
                                    this.Row.ComboEditor.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                                else
                                    this.Row.ComboEditor.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;
							}
						}

						// Now loop through and make sure these columns are measured correctly, so that all their content is visible.
						foreach (ComboCellBase cell in starColumns)
						{
                            double width = 0;
                            if (this.Row.ComboEditor.StarColumnWidths.ContainsKey(cell.Column))
                                width = this.Row.ComboEditor.StarColumnWidths[cell.Column];

							cell.Control.Measure(new Size(width, double.PositiveInfinity));
                            usedWidth -= width;
							maxHeight = Math.Max(maxHeight, cell.Control.DesiredSize.Height);
						}
					}
				}
			}
			else
			{
                // Since we need to account for minimum widths, we might not have encountered the star column yet. 
                // So, lets loop through all of our star columns, and render them.
                ReadOnlyCollection<ComboColumn> actualStarColumns = this.Row.ComboEditor.Columns.StarColumns;
                if (actualStarColumns.Count > starColumns.Count)
                {
                    foreach (ComboColumn col in actualStarColumns)
                    {
                        ComboCellBase cell = this.Row.Cells[col];
                        if (!this.VisibleCells.Contains(cell))
                        {
                            bool alreadyStarColumn = starColumns.Contains(cell);

                            this.RenderCell(col, starColumns, ref maxHeight, false, this.VisibleCells, false);

                            // We need to clean up when we're done, otherwise we'll be adding columns to places they don't belong.
                            this.VisibleCells.Remove(cell);
                            
                            if(!alreadyStarColumn)
                                starColumns.Remove(cell);
                        }
                    }
                }


				// No remaining width?
				// Then we need to set each column's width to the minimum width (Not 0)
				usedWidth = 0; 
				foreach (ComboCellBase cell in starColumns)
				{
					cell.Column.ActualWidth = cell.Column.MinimumWidth;

                    if (!this.Row.ComboEditor.StarColumnWidths.ContainsKey(cell.Column))
                        this.Row.ComboEditor.StarColumnWidths.Add(cell.Column, cell.Column.ActualWidth);
                    else
                        this.Row.ComboEditor.StarColumnWidths[cell.Column] = cell.Column.ActualWidth;

                    cell.Control.Measure(new Size(cell.Column.ActualWidth, double.PositiveInfinity));
                    maxHeight = Math.Max(maxHeight, cell.Control.DesiredSize.Height);

					usedWidth -= cell.Column.ActualWidth;
				}
                this.Row.ComboEditor.Columns.FillerColumn.ActualWidth = 0; 
			}

			if (double.IsInfinity(usedWidth))
				usedWidth = 0; 

			return currentWidth + Math.Abs(usedWidth);
		}

		#endregion // SetupStarCells

		#endregion // Protected

		#region Private

        #region RenderCellsComplex

        private Size RenderCellsComplex(double availableWidth)
        {
            double maxHeight = 0;

            ReadOnlyCollection<ComboColumn> visibleColumns = this.Row.ComboEditor.Columns.VisibleColumns;

            bool isInfinite = double.IsPositiveInfinity(availableWidth);

            int colCount = visibleColumns.Count;

            Collection<ComboCellBase> starColumns = new Collection<ComboCellBase>();

            double currentWidth = 0;
            double fixedCellWidth = 0;

            // Render Fixed Adorner Columns First. 
            ReadOnlyCollection<ComboColumn> fixedColumns = this.Row.ComboEditor.Columns.FixedAdornerColumns;
            foreach (ComboColumn column in fixedColumns)
            {
                if (currentWidth >= availableWidth)
                    break;

                currentWidth += this.RenderCell(column, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);
            }

            // Next Render Fixed Data Columns
            FixedComboColumnsCollection fixedDataColumns = this.Row.ComboEditor.Columns.FixedColumnsLeft;
            if (fixedDataColumns.Count > 0)
            {
                foreach (ComboColumn column in fixedDataColumns)
                {
                    if (column.Visibility == Visibility.Visible)
                    {
                        if (currentWidth >= availableWidth)
                            break;

                        currentWidth += this.RenderCell(column, starColumns, ref maxHeight, false, this.VisibleFixedLeftCells, isInfinite);
                    }
                }
            }

            fixedDataColumns = this.Row.ComboEditor.Columns.FixedColumnsRight;
            if (fixedDataColumns.Count > 0)
            {
                foreach (ComboColumn column in fixedDataColumns)
                {
                    if (column.Visibility == Visibility.Visible)
                    {
                        if (currentWidth >= availableWidth)
                            break;

                        currentWidth += this.RenderCell(column, starColumns, ref maxHeight, false, this.VisibleFixedRightCells, isInfinite);
                    }
                }
            }

            fixedCellWidth = currentWidth;

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
                    this.InvalidateHorizontalMax(availableWidth - fixedCellWidth, ref maxHeight, isInfinite, false);
                    scrollLeft = this.ResolveScrollLeft();
                    this.Owner.MeasureCalled = false;
                    this.Owner.InvalidateMeasure();
                }
                else
                {
                    this.InvalidateHorizontalMax(availableWidth - fixedCellWidth, ref maxHeight, isInfinite, false);
                    scrollLeft = this.ResolveScrollLeft();
                    if (scrollLeft != 0)
                        this._comboEditor.InvalidateOverrideHorizontalMax = true;
                }
            }

            // Add First Cell
            startCell = (int)scrollLeft;
            if (startCell < visibleColumns.Count)
            {
                ComboColumn firstColumn = visibleColumns[startCell];
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

                    ComboColumn col = visibleColumns[currentCell];
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


            // If the width of all the visible cells is less then whats available in the viewport, and there are more cells in the 
            // collection, it means we've scrolled further than we needed to. Since we don't want whitespace to appear after 
            // the last cell, lets add more cells to the front.
            if (currentWidth < availableWidth && this._visibleCells.Count < colCount)
            {
                for (currentCell = startCell; currentCell >= 0; currentCell--)
                {
                    ComboColumn c = visibleColumns[currentCell];

                    // FillerComboColumn shouldn't have an influence on the determination if percentScroll should be reset to zero.
                    if (c is FillerComboColumn)
                        startCell--;

                    cellWidth = this.RenderCell(c, starColumns, ref maxHeight, true, this.VisibleCells, isInfinite);
                    currentWidth += cellWidth;
                }
            }

            //// Start from the last cell, and work our way left, until we figure out how many cells
            //// can fit in the viewport, so that the horizontal max for horizontal scrolling can be set.
            this.InvalidateHorizontalMax(availableWidth - fixedCellWidth, ref maxHeight, isInfinite, true);

            // So, now we're storing the overflow adjustment on the rows manager. 
            // This will ensure that they're the same for ever row on a band, so that the header is never off
            // when doing auto calculations. 
            this._comboEditor.OverflowAdjustment = Math.Max(this._comboEditor.OverflowAdjustment, (currentWidth - availableWidth));

            // don't count the FillerComboColumn.
            if (isInfinite)
                colCount--;

            this._comboEditor.ScrollableCellCount = colCount;

            this._comboEditor.VisibleCellCount = this._visibleCells.Count;
            this._comboEditor.CurrentVisibleWidth = currentWidth - fixedCellWidth;

            this._comboEditor.IndexOfFirstColumnRendered = startCell + 1;

            this._comboEditor.TotalColumnsRendered = this.VisibleCells.Count;
            this._comboEditor.RowWidth = currentWidth;

            return new Size(currentWidth, maxHeight);
        }

        #endregion // RenderCellsComplex

		internal void ReleaseCell(ComboCellBase cell)
		{
			if (cell.Control != null && cell.Control.Parent == this)
			{
                if(!RecyclingManager.Manager.ReleaseElement(cell, this))
				    this._nonReleasedCells.Add(cell);
			}
		}

        private void ReleaseCells(Collection<ComboCellBase> cells)
        {
            foreach (ComboCellBase cell in cells)
            {
                this.ReleaseCell(cell);
            }

            cells.Clear();
        }

		private double ResolveScrollLeft()
		{
			ScrollBar bar = this._comboEditor.HorizontalScrollBar;
			
			if (bar != null)
			{
				return bar.Value;
			}

			return 0;
		}

		internal void SetScrollLeft(double scrollLeft)
		{
			ScrollBar bar = this._comboEditor.HorizontalScrollBar;

			if (bar != null && this.Row.ComboEditor.ScrollableCellCount - 1 > 0)
			{
				bar.Value = scrollLeft;
			}
		}

		private void InvalidateHorizontalMax(double availWidth, ref double maxHeight, bool isInfinite, bool includeStarCols)
		{
			if (this._prevAvailableWidth != availWidth)
				this.Row.ComboEditor.OverrideHorizontalMax = -1;

			this._prevAvailableWidth = availWidth;

			// Start from the last cell, and work our way left, until we figure out how many cells
			// can fit in the viewport, so that the horizontal max for horizontal scrolling can be set.
            if (this.Row.ComboEditor.OverrideHorizontalMax == -1 && !isInfinite)
            {
                List<ComboCellBase> previouslyLastCells = new List<ComboCellBase>(this._lastCellsDetached);
                this._lastCellsDetached.Clear();

                // So, if there is a star column, then we shouldn't set the HorizontalMax.
                foreach (ComboColumn col in this.Row.ComboEditor.Columns.VisibleColumns)
                {
                    // Check to make sure its not a FillerComboColumn
                    // Check to see if its a Star Column
                    // Check to see that the column's actual width isn't its minwidth.
                    if (!(col is FillerComboColumn) && col.WidthResolved.WidthType == ComboColumnWidthType.Star && col.ActualWidth > col.MinimumWidth)
                    {
                        // If there isn't a control attached, then its safe to assume that we need to set the HorizontalMax
                        // As obviously there are cells left to be rendered, which means Horizontal scrollbar is needed. 
                        ComboCellBase cell = this.Row.Cells[col];
                        if (cell.Control != null)
                        {
                            this.Row.ComboEditor.OverrideHorizontalMax = -1;
                            return;
                        }
                    }
                }


                Collection<ComboCellBase> fakeStarColumns = new Collection<ComboCellBase>();

                double cellWidth = 0;
                double lastCellsWidth = 0;
                int currentLastCellIndex = this.Row.ComboEditor.Columns.VisibleColumns.Count - 1;
                while (lastCellsWidth < availWidth && currentLastCellIndex >= 0)
                {
                    ComboColumn col = this.Row.ComboEditor.Columns.VisibleColumns[currentLastCellIndex];

                    if (col.WidthResolved.WidthType != ComboColumnWidthType.Star)
                    {
                        cellWidth = this.RenderCell(col, fakeStarColumns, ref maxHeight, false, this._lastCellsDetached, isInfinite);
                    }
                    else if (includeStarCols && !(col is FillerComboColumn))
                    {
                        cellWidth = col.ActualWidth;
                    }

                    lastCellsWidth += cellWidth;
                    currentLastCellIndex--;
                }

                int count = this._lastCellsDetached.Count - 1;
                for (int i = count; i >= 0; i--)
                {
                    ComboCellBase cell = this._lastCellsDetached[i];
                    if (this.VisibleCells.Contains(cell))
                    {
                        this._lastCellsDetached.Remove(cell);
                    }
                    else
                    {
                        this.ReleaseCell(cell);
                    }
                }

                // We cache off the last rows, so that we don't have to keep sizing them to adjust
                // the Vertical scrollbar. But, something changed in the rows collection now, so lets
                // start over.
                foreach (ComboCellBase cell in previouslyLastCells)
                {
                    if (!this.VisibleCells.Contains(cell) && !this.VisibleFixedLeftCells.Contains(cell) && !this.VisibleFixedRightCells.Contains(cell) && cell.Control != null)
                    {
                        this.ReleaseCell(cell);
                    }
                }

                currentLastCellIndex++;
                double sp = (lastCellsWidth - availWidth) / cellWidth;
                this.Row.ComboEditor.OverrideHorizontalMax = currentLastCellIndex + sp;
                if (this.Row.ComboEditor.OverrideHorizontalMax < 0)
                    this.Row.ComboEditor.OverrideHorizontalMax = 0;
            }
		}
      
		#endregion // Private

		#region Internal

		internal void InternalCellMouseEnter()
		{
			this.OnCellMouseOver();
		}

        internal void InternalCellMouseLeave()
        {
            this.OnCellMouseLeave();
        }

        internal void RenderCell(ComboCellBase cell)
		{
			double maxHeight = 0;

            this.RenderCell(cell.Column, new Collection<ComboCellBase>(), ref maxHeight, false, this._visibleCells, false);
		}

		// JM 10-27-11 TFS94275 Added.
		#region InternalEnsureAllCellVisualStates
		internal void InternalEnsureAllCellVisualStates()
		{
			foreach (ComboCellBase visibleCell in this._visibleFixedLeftCells)
				visibleCell.EnsureCurrentState();

			foreach (ComboCellBase visibleCell in this._visibleCells)
				visibleCell.EnsureCurrentState();

			foreach (ComboCellBase visibleCell in this._visibleFixedRightCells)
				visibleCell.EnsureCurrentState();
		}
		#endregion //InternalEnsureAllCellVisualStates

		#endregion // Internal

		#endregion // Methods

		#region Overrides

		#region MeasureOverride
		/// <summary>
        /// Provides the behavior for the "measure" pass of the <see cref="ComboCellsPanel"/>.
		/// </summary>
		/// <param propertyName="availableSize">The available size that this object can give to child objects. Infinity can be specified
		/// as a value to indicate the object will size to whatever content is available.</param>
		/// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            this.MeasureRaised = true;

            if (this.Row == null || availableSize.Height == 1 && availableSize.Width == 1)
                return base.MeasureOverride(availableSize);

            this.PreviousInvalidateHeight = availableSize.Height;

            // If the Owning Panel, didn'type invoke this measure, then we need to make sure that it does get measured, so that all of the cells 
            // In the visible grid have the same ComboColumnWidth.
            if (this.Owner != null && !this.Owner.InLayoutPhase)
                this.Owner.InvalidateMeasure();

            bool isInfinite = double.IsPositiveInfinity(availableSize.Width);

            List<ComboCellBase> previousCells = new List<ComboCellBase>(this._visibleCells);
            previousCells.AddRange(this._visibleFixedLeftCells);
            previousCells.AddRange(this._additionalCells);
            previousCells.AddRange(this._visibleFixedRightCells);
            this._additionalCells.Clear();
            this._visibleCells.Clear();
            this._visibleFixedLeftCells.Clear();
            this._visibleFixedRightCells.Clear();

            Size rowSize = this.RenderCells(availableSize.Width);

            this.Row.ActualHeight = Math.Max(this.Row.ActualHeight, rowSize.Height);

            foreach (ComboCellBase cell in previousCells)
            {
                if (!this._visibleCells.Contains(cell) && !this._visibleFixedLeftCells.Contains(cell) && !this._visibleFixedRightCells.Contains(cell) && !this._additionalCells.Contains(cell))
                {
                    this.ReleaseCell(cell);
                }
            }

            double width = availableSize.Width;
            if (isInfinite)
                width = rowSize.Width;

			// JM 9-1-11 TFS85157 - Use Row.ActualHeight instead of rowSize.Height
            return new Size(width, this.Row.ActualHeight);
        }

		#endregion // MeasureOverride

		#region ArrangeOverride

		/// <summary>
        /// Arranges each <see cref="ComboCellBase"/> that should be visible, one next to  the other, similar to a 
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
            foreach (ComboCellBase cell in this._nonReleasedCells)
			{
				if (cell.Control != null)
					cell.Control.Arrange(this._hideCellRect);
			}

			this._nonReleasedCells.Clear();

			double left = 0;

			//Hide the Indentation.
            double clipWidth = finalSize.Width - left;
            if (clipWidth < 0)
                clipWidth = 0;
			this._clipRG.Rect = new Rect(left, 0, clipWidth, finalSize.Height);

			// Position the Left Fixed Cells.
            foreach (ComboCellBase cell in this._visibleFixedLeftCells)
			{
				ComboColumn col = cell.Column;
				FrameworkElement elem = cell.Control;
				double actualWidth = col.ActualWidth;

                double height = Math.Max(finalSize.Height, elem.DesiredSize.Height);

                double leftVal = left;
					
				elem.Arrange(new Rect(leftVal, 0, actualWidth, height));
				
				left += actualWidth;
				Canvas.SetZIndex(elem, 1);
			}

			// Position the Right Fixed Cells.
			double right = finalSize.Width; 

			double fillerColumnWidth = this.Row.ComboEditor.Columns.FillerColumn.ActualWidth;
			if (fillerColumnWidth > 0)
				right -= fillerColumnWidth;

            foreach (ComboCellBase cell in this._visibleFixedRightCells)
            {
                ComboColumn col = cell.Column;
                FrameworkElement elem = cell.Control;
                double actualWidth = cell.Column.ActualWidth;
                double rightVal = right;
                right -= actualWidth;

                rightVal -= actualWidth;

                elem.Arrange(new Rect(rightVal, 0, actualWidth, Math.Max(finalSize.Height, elem.DesiredSize.Height)));

                Canvas.SetZIndex(elem, 1);
            }

            if (this.Row.CanScrollHorizontally)
            {
                // Calculate the offset LeftValue, for the first cell 
                ScrollBar horiztonalSB = this._comboEditor.HorizontalScrollBar;
                if (horiztonalSB != null && horiztonalSB.Visibility == Visibility.Visible)
                {
                    if (this.Row.ComboEditor.OverrideHorizontalMax > 0)
                    {
                        // If the horizontal scrollbar is visible, it's safe to say that we don't need the FillerComboColumn
                        // if we don't do this, then there is a chance that the scroll left changed between measuring and arranging
                        // due to column's resizing. 
                        this.Row.ComboEditor.Columns.FillerColumn.ActualWidth = 0;
                    }


                    if (horiztonalSB.Value != horiztonalSB.Maximum)
                    {
                        if (this._visibleCells.Count > 0)
                        {
                            double scrollLeft = this.ResolveScrollLeft();
                            double percent = scrollLeft - (int)scrollLeft;
                            double leftVal = this._visibleCells[0].Column.ActualWidth * percent;

                            if ((this.Row.ComboEditor.CurrentVisibleWidth + left) > right)
                                left += -leftVal;
                        }
                    }
                    else
                    {
                        // We've reached the last child, so lets make sure its visible. 
                        if ((this.Row.ComboEditor.CurrentVisibleWidth + left) > right)
                            left -= this.Row.ComboEditor.OverflowAdjustment;
                    }

                }
            }

			// Position the Scrollable Cells
            foreach (ComboCellBase cell in this._visibleCells)
            {
                ComboColumn col = cell.Column;

                if (!col.IsInitialAutoSet)
                {
                    bool ignoreAutoSet = this.Owner.FixedRowsTop.Contains(this.Row);

                    if (!ignoreAutoSet && cell.Control.IsCellLoaded)
                        col.IsInitialAutoSet = true;
                }

                ComboCellControlBase elem = cell.Control;
                double actualWidth = cell.Column.ActualWidth;

                if (cell.Column.WidthResolved.WidthType == ComboColumnWidthType.Star)
                {
                    if (this.Row.ComboEditor.StarColumnWidths.ContainsKey(cell.Column))
                        actualWidth = this.Row.ComboEditor.StarColumnWidths[cell.Column];
                }

                double leftVal = left;

                if (col is FillerComboColumn)
                    leftVal = finalSize.Width - actualWidth;

				elem.Arrange(new Rect(leftVal, 0, actualWidth, Math.Max(finalSize.Height, elem.DesiredSize.Height)));

                left += actualWidth;
                Canvas.SetZIndex(elem, 0);
            }
			
			return finalSize;
		}
		#endregion // ArrangeOverride

		// JM 10-3-11 TFS90123 - Added.
		#region OnCreateAutomationPeer
		/// <summary>
		/// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the Silverlight automation infrastructure.
		/// </summary>
		/// <returns>
		/// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
		/// </returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new ComboCellsPanelAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#endregion // Overrides
	
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