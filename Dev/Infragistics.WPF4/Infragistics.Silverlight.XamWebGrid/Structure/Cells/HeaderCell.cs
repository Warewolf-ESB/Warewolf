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
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A cell that represents a particular column in the header of the <see cref="XamGrid"/>
	/// </summary>
	public class HeaderCell : CellBase
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="HeaderCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="HeaderRow"/> object that owns the <see cref="HeaderCell"/></param>
		/// <param propertyName="column">The <see cref="ColumnBase"/> object that the <see cref="HeaderCell"/> represents.</param>
		protected internal HeaderCell(RowBase row, Column column)
			: base(row, column)
		{
		}

        ObservableCollection<Column> _columnsHiddenToLeft, _columnsHiddenToRight;

		#endregion // Constructor

		#region Overrides

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="HeaderCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				if (this.Style == null)
					return this.Column.HeaderStyleResolved;
				else
					return this.Style;
			}
		}

		#endregion // ResolveStyle

        #region RecyclingElementType

        /// <summary>
        /// Gets the Type of control that should be created for the <see cref="Cell"/>.
        /// </summary>
        protected override Type RecyclingElementType
        {
            get
            {
                return null;
            }
        }
        #endregion // RecyclingElementType

        #region RecyclingIdentifier

        /// <summary>
        /// If a <see cref="RecyclingElementType"/> isn't specified, this property can be used to offer another way of identifying 
        /// a reyclable element.
        /// </summary>
        protected override string RecyclingIdentifier
        {
            get
            {
                return this.Row.RowType.ToString() + "_" + this.Column.Key + "_" + this.Column.ColumnLayout.Key;
            }
        }
        #endregion // RecyclingIdentifier

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of a <see cref="HeaderCellControl"/> for the <see cref="HeaderCell"/>.
		/// </summary>
		/// <returns>A new <see cref="HeaderCellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new HeaderCellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#region EnsureCurrentState

		/// <summary>
		/// Ensures that <see cref="HeaderCell"/> is in the correct state.
		/// </summary>
		protected internal override void EnsureCurrentState()
		{
            ColumnLayout layout = this.Row.ColumnLayout;

            if (layout == null)
            {
                return;
            }

			base.EnsureCurrentState();

			if (this.Control != null)
			{
 			    // Active States
				CellBase activeCell = layout.Grid.ActiveCell;
				if (activeCell != null && this.Column == activeCell.Column)
					this.Control.GoToState("Active", false);
				else
					this.Control.GoToState("InActive", false);

				Column col = this.Column;

				if (col != null)
				{
					bool layoutContainsColumn = layout.Columns.Contains(col);

                    if (!layoutContainsColumn)
                    {
                        ReadOnlyKeyedColumnBaseCollection<ColumnBase> columns = layout.Columns.AllColumns;
                        layoutContainsColumn = columns.Contains(col);
                    }

					// Selected States
					if (col.IsSelected)
						this.Control.GoToState("Selected", false);
					else
						this.Control.GoToState("NotSelected", false);

					// Fixed States
					FixedColumnType type = layout.FixedColumnSettings.AllowFixedColumnsResolved;
					bool isFixed = !(col.IsFixed == FixedState.NotFixed);

                    bool isMerged = layout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells && col.IsGroupBy;

					if (type != FixedColumnType.Disabled && !isMerged)
					{
						bool pinable = (type == FixedColumnType.Both || type == FixedColumnType.Indicator);
						if (layoutContainsColumn)
						{
							if (col.IsFixable && pinable)
							{
								if (isFixed)
									this.Control.GoToState("Pinned", false);
								else
									this.Control.GoToState("Unpinned", false);
							}
							else
								this.Control.GoToState("NotFixable", false);
						}
					}
					else
						this.Control.GoToState("NotFixable", false);

					if (isFixed)
						this.Control.GoToState("Fixed", false);
					else
						this.Control.GoToState("Unfixed", false);


					// Sorting States
					SortingSettingsOverride sortingSettings = layout.SortingSettings;
					if (layoutContainsColumn)
					{
						if (sortingSettings.ShowSortIndicatorResolved)
						{
							if (col.IsSorted == SortDirection.Ascending)
								this.Control.GoToState("Ascending", false);
							else if (col.IsSorted == SortDirection.Descending)
								this.Control.GoToState("Descending", false);
							else
								this.Control.GoToState("NotSorted", false);
						}
						else
							this.Control.GoToState("NotSorted", false);
					}

					// Summation States
					SummaryRowSettingsOverride sumSettings = layout.SummaryRowSettings;
					if (layoutContainsColumn)
					{
						bool summableCol = col.IsSummable;
						if (sumSettings.AllowSummaryRowResolved != SummaryRowLocation.None && summableCol)
						{
							this.Control.GoToState("Summable", false);
						}
						else
						{
							this.Control.GoToState("Unsummable", false);
						}
					}
					else
					{
						this.Control.GoToState("Unsummable", false);
					}

					// Filtered States
					if (layoutContainsColumn)
					{
						if (layout.FilteringSettings.AllowFilteringResolved == FilterUIType.FilterMenu && col.IsFilterable)
						{
							this.Control.GoToState("FilterIcon", false);
						}
						else
						{
							this.Control.GoToState("NoIcon", false);
						}
					}
					else
					{
						this.Control.GoToState("NoIcon", false);
					}

					if (layoutContainsColumn)
					{
						RowsManager rm = (RowsManager)this.Row.Manager;

						RowsFilter rf = (RowsFilter)rm.RowFiltersCollectionResolved[this.Column.Key];

						if (rf == null || rf.Conditions.Count == 0)
						{
							this.Control.GoToState("NotFiltered", false);
						}
						else
						{
							this.Control.GoToState("Filtered", false);
						}
					}
					else
					{
						this.Control.GoToState("NotFiltered", false);
					}

                    // Hidden Column States
                    if (col.IsHideable && layout.ColumnChooserSettings.AllowHideColumnIconResolved)
                    {
                        this.Control.GoToState("VisibilityIconVisible", false);
                    }
                    else
                    {
                        this.Control.GoToState("VisibilityIconHidden", false);
                    }

                    if (layout.ColumnChooserSettings.AllowHiddenColumnIndicatorResolved)
                    {
                        List<Column> cols = new List<Column>(layout.Columns.DataColumns);
                        List<Column> visCols = new List<Column>(layout.Columns.VisibleColumns);
                        List<Column> mergedCols = new List<Column>();

                        int colCountAdjustment = 2;

                        if (col.ParentColumn != null)
                        {
                            cols = new List<Column>(col.ParentColumn.ResolveChildColumns());
                            List<Column> visibleColumns = new List<Column>();
                            foreach (Column c in cols)
                            {
                                if (c.Visibility == Visibility.Visible)
                                    visibleColumns.Add(c);
                            }
                            visCols = visibleColumns;

                            colCountAdjustment = 1;
                        }
                        else
                        {
                            if (layout.Grid.GroupBySettings.GroupByOperation == GroupByOperation.MergeCells)
                            {
                                ReadOnlyCollection<Column> groupedCols = layout.Grid.GroupBySettings.GroupByColumns[layout];
                                int count = groupedCols.Count;
                                for(int i = count -1; i >= 0; i--)
                                {
                                    Column groupedCol = groupedCols[i];
                                    if (cols.Contains(groupedCol))
                                    {
                                        cols.Remove(groupedCol);
                                        cols.Insert(0, groupedCol);
                                    }

                                    if (visCols.Contains(groupedCol))
                                    {
                                        visCols.Remove(groupedCol);
                                        visCols.Insert(0, groupedCol);
                                    }

                                    if (groupedCol.Visibility == Visibility.Visible)
                                        mergedCols.Add(groupedCol);

                                }
                            }
                        }

                        //Reset the hidden columns.
                        this.ColumnsHiddenToRight.Clear();
                        this.ColumnsHiddenToLeft.Clear();
                        
                        int index = cols.IndexOf(col);

                        // So if there are no visible columns, and we're on a filler column, then put the indicator on it.
                        if (col is FillerColumn && visCols.Count == 1)
                        {
                            bool onlyColumnLeft = true;
                            
                            foreach (Column dataCol in cols)
                            {
                                if (dataCol.Visibility == Visibility.Visible)
                                {
                                    onlyColumnLeft = false;
                                    break;
                                }
                            }

                            if (onlyColumnLeft)
                                index = cols.Count;
                        }


                        // Loop through the columns and appened to the ColumnsHiddenToLeft collection, while there are collapsed columns.
                        // As soon as a non Collapsed column is found, break.
                        for (int i = index - 1; i >= 0; i--)
                        {
                            Column leftCol = cols[i];
                            if (leftCol.Visibility == Visibility.Collapsed)
                                this.ColumnsHiddenToLeft.Add(leftCol);
                            else
                                break;
                        }


                        // Time to Populate the ColumnsHiddenToRight Collection. 
                        // If we're the last Visible Column (Note we ignore the FillerColumn)
                        // Then populate all other hidden columns.
                        int visColIndex = visCols.IndexOf(col);
                        int visColCount = visCols.Count - colCountAdjustment;
                        if (visColIndex == visColCount)
                        {
                            bool isLastCol = true;

                            // Ok, so there aren't any "visible" columns, so lets check the fixed columns collections.
                            if (visColIndex == -1)
                            {
                                isLastCol = false;

                                FixedColumnsCollection leftFCC = layout.FixedColumnSettings.FixedColumnsLeft;
                                FixedColumnsCollection rightFCC = layout.FixedColumnSettings.FixedColumnsRight;

                                List<Column> fixedLeft = new List<Column>(leftFCC);
                                List<Column> fixedRight = new List<Column>(rightFCC);

                                fixedLeft.InsertRange(0, mergedCols);

                                // Can't use foreach b/c we're most likely already in a foreach for FixedColumnsLeft, and that would reset it.
                                for(int i = 0; i < leftFCC.Count; i++)
                                {
                                    Column fixedCol = leftFCC[i];
                                    if (fixedCol.Visibility == Visibility.Collapsed)
                                        fixedLeft.Remove(fixedCol);
                                }

                                // Can't use foreach b/c we're most likely already in a foreach for FixedColumnsRight, and that would reset it.
                                for (int i = 0; i < rightFCC.Count; i++)
                                {
                                    Column fixedCol = rightFCC[i];
                                    if (fixedCol.Visibility == Visibility.Collapsed)
                                        fixedRight.Remove(fixedCol);
                                }

                                int fixedColRightIndex = fixedRight.IndexOf(col);
                                int fixedColLeftIndex = fixedLeft.IndexOf(col);

                                isLastCol = (fixedColRightIndex != -1 && fixedColRightIndex == fixedRight.Count - 1) || (fixedColLeftIndex != -1 && fixedRight.Count == 0 && fixedColLeftIndex == fixedLeft.Count - 1);
                            }

                            if (isLastCol)
                            {
                                List<Column> colsToAdd = new List<Column>();
                                for (int i = index + 1; i < cols.Count; i++)
                                {
                                    Column colToAdd = cols[i];
                                    if (colToAdd.Visibility == Visibility.Collapsed)
                                        colsToAdd.Add(colToAdd);
                                    else
                                    {
                                        colsToAdd.Clear();
                                    }
                                }

                                foreach (Column colToAdd in colsToAdd)
                                    this.ColumnsHiddenToRight.Add(colToAdd);
                            }
                        }

                        // Update the Left Adjacent states.
                        if (this.ColumnsHiddenToLeft.Count > 0)
                        {
                            this.Control.GoToState("HiddenLeftAdjacentColumns", false);
                        }
                        else
                        {
                            this.Control.GoToState("NoHiddenLeftAdjacentColumns", false);
                        }
                        
                        // Update the Right Adjacent states.
                        if (this.ColumnsHiddenToRight.Count > 0)
                        {
                            this.Control.GoToState("LastVisibleColumnWithHiddenRightAdjacentColumns", false);
                        }
                        else
                        {
                            this.Control.GoToState("NoHiddenRightAdjacentColumns", false);
                        }
                    }
                    else
                    {
                        // No indicators should be made visible, the feature isn't turned on.
                        this.Control.GoToState("NoHiddenLeftAdjacentColumns", false);
                        this.Control.GoToState("NoHiddenRightAdjacentColumns", false);
                    }
				}
			}
		}
		#endregion // EnsureCurrentState

		#region OnCellDoubleClick
		/// <summary>
		/// Invoked when a cell is double clicked.
		/// </summary>
		protected internal override void OnCellDoubleClick()
		{
			if (this.Control != null && this.Column.ColumnLayout != null)
			{
				ColumnResizingSettingsOverride settings = this.Column.ColumnLayout.ColumnResizingSettings;
				if (this.Control.AllowUserResizing && settings.AllowDoubleClickToSizeResolved)
				{
					
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                    Column col = this.Column.ResizeColumnResolved;
                    col.IsInitialAutoSet = false;
					col.Width = ColumnWidth.Auto;
					col.Width = ColumnWidth.InitialAuto;
                    Column parentCol = col.ParentColumn;
                    while (parentCol != null)
                    {
                        parentCol.Width = ColumnWidth.InitialAuto;
                        parentCol = parentCol.ParentColumn;
                    }

					if (settings.AllowMultipleColumnResizeResolved == true)
					{
						SelectedColumnsCollection selectedColumns = this.Column.ColumnLayout.Grid.SelectionSettings.SelectedColumns;

						if (selectedColumns.Contains(this.Column))
						{
							foreach (Column c in selectedColumns)
							{
                                Column resizeCol = c.ResizeColumnResolved;
                                resizeCol.Width = ColumnWidth.Auto;
                                resizeCol.Width = ColumnWidth.InitialAuto;
                                parentCol = c.ParentColumn;
                                while (parentCol != null)
                                {
                                    parentCol.Width = ColumnWidth.InitialAuto;
                                    parentCol = parentCol.ParentColumn;
                                }
							}
						}
					}
					this.Control.IsResizing = false;
					this.Control.EndDragResize(true);
				}
				else
					base.OnCellDoubleClick();
			}
		}
		#endregion // OnCellDoubleClick

		#endregion // Overrides

        #region Properties

        #region Public

        #region ColumnsHiddenToLeft

        /// <summary>
        /// Gets a collection of <see cref="Column"/> objects that are currently hidden to the left, of this <see cref="Column"/>
        /// </summary>
        public ObservableCollection<Column> ColumnsHiddenToLeft
        {
            get
            {
                if (this._columnsHiddenToLeft == null)
                    this._columnsHiddenToLeft = new ObservableCollection<Column>();

                return this._columnsHiddenToLeft;
            }
        }
        #endregion // ColumnsHiddenToLeft

        #region ColumnsHiddenToRight
        /// <summary>
        /// Gets a collection of <see cref="Column"/> objects that are currently hidden to the right, of this <see cref="Column"/>
        /// </summary>
        /// <remarks>
        /// This collection is only populates if this particular column is the last visible column of the <see cref="ColumnLayout"/>
        /// </remarks>
        public ObservableCollection<Column> ColumnsHiddenToRight
        {
            get
            {
                if (this._columnsHiddenToRight == null)
                    this._columnsHiddenToRight = new ObservableCollection<Column>();

                return this._columnsHiddenToRight;
            }
        }
        #endregion // ColumnsHiddenToRight

        #endregion // Public

        #endregion // Properties
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