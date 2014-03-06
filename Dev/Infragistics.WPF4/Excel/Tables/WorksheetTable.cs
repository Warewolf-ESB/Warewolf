using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.Sorting;




using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 12/7/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a region of cells formatted as a table.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Tables assist in managing and analyzing a range of related data. This management can be done separately from the rest of the
	/// data in the worksheet.
	/// </p>
	/// <p class="body">
	/// A table can have one or more columns sorted and filtered. There are various sorting and filtering criteria that can be applied 
	/// to the columns. The types pertaining to filtering can be found in the Infragistics.Documents.Excel.Filtering namespace and a filter
	/// can be applied to a column by setting the <see cref="WorksheetTableColumn.Filter"/> property. The types pertaining to sorting can
	/// be found in the Infragistics.Documents.Excel.Sorting namespace and a column can be sorted by setting the 
	/// <see cref="WorksheetTableColumn.SortCondition"/> or by populating the 
	/// <see cref="Sorting.SortSettings&lt;WorksheetTableColumn&gt;.SortConditions"/> collection on the <see cref="WorksheetTable.SortSettings"/>.
	/// </p>
	/// <p class="body">
	/// A table can contain calculated columns which dynamically determine their value based on a formula. A 
	/// <see cref="WorksheetTableColumn"/> can be made a calculated column by setting the <see cref="WorksheetTableColumn.ColumnFormula"/>.
	/// </p>
	/// <p class="body">
	/// A table can also contain a totals row which display total information about the table. This can be shown by setting 
	/// <see cref="IsTotalsRowVisible"/> to True. When the totals row is displayed, each column can display text or a calculated value in the
	/// totals row, by setting either the <see cref="WorksheetTableColumn.TotalLabel"/> or <see cref="WorksheetTableColumn.TotalFormula"/>,
	/// respectively.
	/// </p>
	/// </remarks>
	/// <seealso cref="Excel.Worksheet.Tables"/>
	/// <seealso cref="WorksheetRegion.FormatAsTable(bool)"/>
	/// <seealso cref="WorksheetRegion.FormatAsTable(bool,WorksheetTableStyle)"/>
	/// <seealso cref="WorksheetCell.AssociatedTable"/>
	/// <seealso cref="WorksheetRow.GetCellAssociatedTable"/>
	[DebuggerDisplay("WorksheetTable: {name}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class WorksheetTable : NamedReferenceBase,
		IAreaFormatsOwner<WorksheetTableArea>,
		ISortSettingsOwner
	{
		#region Member Variables

		private WorksheetTableAreaFormatsCollection<WorksheetTableArea> _areaFormats;
		private WorksheetTableColumnCollection _columns;
		private Dictionary<WorksheetTableColumn, string> _columnNames;
		private TableFlags _flags = TableFlags.DisplayBandedRows;
		private readonly uint _id;
		private uint _nextColumnId = 1;
		private SortSettings<WorksheetTableColumn> _sortSettings;
		private WorksheetTableStyle _style;
		private int _suspendFilteringCount;
		private TableCalcReference _tableDataCalcReference;
		private WorksheetRegionAddress _wholeTableAddress;
		private Worksheet _worksheet;

		#endregion // Member Variables

		#region Constructor

		internal WorksheetTable(string name, uint id, int firstRowIndex, int lastRowIndex, short firstColumnIndex, short lastColumnIndex)
			: base(null, false)
		{
			this.Name = name;

			_id = id;
			_wholeTableAddress = new WorksheetRegionAddress(firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);
		}

		#endregion // Constructor

		#region Interfaces

		#region IAreaFormatsOwner Members

		bool IAreaFormatsOwner<WorksheetTableArea>.IsReadOnly
		{
			get { return false; }
		}

		void IAreaFormatsOwner<WorksheetTableArea>.OnAreaFormatAdded(WorksheetTableArea area, WorksheetCellFormatData format) { }

		void IAreaFormatsOwner<WorksheetTableArea>.VerifyCanBeModified() { }

		#endregion

		#region IGenericCachedCollectionEx Members

		Workbook IGenericCachedCollectionEx.Workbook
		{
			get { return this.Workbook; }
		}

		#endregion

		#region ISortSettingsOwner Members

		void ISortSettingsOwner.OnSortSettingsModified()
		{
			this.ReapplySortConditions();
		}

		// MD 4/9/12 - TFS101506
		CultureInfo ISortSettingsOwner.Culture
		{
			get { return this.Culture; }
		}

		WorksheetRegion ISortSettingsOwner.SortRegion
		{
			get { return this.DataAreaRegion; }
		}

		#endregion

		#region IWorksheetCellFormatProxyOwner Members

		WorksheetCellFormatData IWorksheetCellFormatProxyOwner.GetAdjacentFormatForBorderResolution(WorksheetCellFormatProxy sender, CellFormatValue borderValue)
		{
			return null;
		}

		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanged(WorksheetCellFormatProxy sender, IList<CellFormatValue> values, CellFormatValueChangedOptions options)
		{
			WorksheetTableAreaFormatProxy<WorksheetTableArea> areaFormatProxy = (WorksheetTableAreaFormatProxy<WorksheetTableArea>)sender;
			this.SynchronizeAreaFormatWithColumns(areaFormatProxy, values);
		}

		void IWorksheetCellFormatProxyOwner.OnCellFormatValueChanging(WorksheetCellFormatProxy sender, IList<CellFormatValue> values)
		{
			WorksheetTableAreaFormatProxy<WorksheetTableArea> areaFormatProxy = (WorksheetTableAreaFormatProxy<WorksheetTableArea>)sender;
			for (int i = 0; i < values.Count; i++)
				WorksheetTable.VerifyAreaFormatValueCanBeSet(areaFormatProxy.Area, values[i]);
		}

		void IWorksheetCellFormatProxyOwner.VerifyFormatOptions(WorksheetCellFormatProxy sender, WorksheetCellFormatOptions formatOptions)
		{
			WorksheetTableAreaFormatProxy<WorksheetTableArea> areaFormatProxy = (WorksheetTableAreaFormatProxy<WorksheetTableArea>)sender;

			if (areaFormatProxy.Area == WorksheetTableArea.WholeTable)
			{
				const WorksheetCellFormatOptions invalidWholeTableOptions =
					WorksheetCellFormatOptions.All & ~WorksheetCellFormatOptions.ApplyBorderFormatting;

				if ((formatOptions & invalidWholeTableOptions) != WorksheetCellFormatOptions.None)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_InvalidFormatOptionsInWholeTableArea"));
			}
		}

		#endregion

		#endregion // Interfaces

		#region Base Class Overrides

		#region CalcReference

		internal override IExcelCalcReference CalcReference
		{
			get
			{
				if (_tableDataCalcReference == null)
					_tableDataCalcReference = new TableCalcReference(null, this, null, null);

				return _tableDataCalcReference;
			}
		}

		#endregion // CalcReference

		// MD 4/9/12 - TFS101506
		#region Culture

		internal override CultureInfo Culture
		{
			get
			{
				if (this.Worksheet != null)
					return this.Worksheet.Culture;

				return CultureInfo.CurrentCulture;
			}
		}

		#endregion // Culture

		#region IsNameUniqueAcrossScopes

		internal override bool IsNameUniqueAcrossScopes
		{
			get { return true; }
		}

		#endregion // IsNameUniqueAcrossScopes

		#region OnFormulaChanged

		internal override void OnFormulaChanged()
		{
			base.OnFormulaChanged();
			Utilities.DebugFail("A WorksheetTable should not have its formula set.");
		}

		#endregion // OnFormulaChanged

		// MD 7/19/12
		// Found while fixing TFS116808 (Table resizing)
		#region ToString

		/// <summary>
		/// Gets the string representation of the table.
		/// </summary>
		/// <returns>The string representation of the table.</returns>
		public override string ToString()
		{
			return this.Name;
		}

		#endregion // ToString

		#region Workbook

		internal override Workbook Workbook
		{
			get
			{
				Worksheet worksheet = this.Worksheet;
				if (worksheet == null)
					return null;

				return worksheet.Workbook;
			}
		}

		#endregion // Workbook

		#endregion // Base Class Overrides

		#region Methods

		#region Public Methods

		#region ClearFilters

		/// <summary>
		/// Clears all filters from the columns in the table.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any filters are present and removed when this is called, all hidden rows in the data area of the table will be unhidden.
		/// </p>
		/// </remarks>
		/// <seealso cref="ReapplyFilters"/>
		/// <seealso cref="WorksheetTableColumn.Filter"/>
		public void ClearFilters()
		{
			try
			{
				this.SuspendFiltering();

				foreach (WorksheetTableColumn column in this.Columns)
					column.ClearFilter();
			}
			finally
			{
				this.ResumeFiltering();
			}
		}

		#endregion // ClearFilters

		#region ClearSortConditions

		/// <summary>
		/// Clears all sort conditions from the columns in the table.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> Just as in Microsoft Excel, clearing the sort conditions will not revert the table back to its original unsorted 
		/// state. The table will remain in its last sorted order.
		/// </p>
		/// </remarks>
		/// <seealso cref="WorksheetTable.SortSettings"/>
		/// <see cref="Sorting.SortSettings&lt;WorksheetTableColumn&gt;.SortConditions"/>
		/// <seealso cref="WorksheetTableColumn.SortCondition"/>
		public void ClearSortConditions()
		{
			this.SortSettings.SortConditions.Clear();
		}

		#endregion // ClearSortConditions

		#region ReapplyFilters

		/// <summary>
		/// Re-filters all data cells in the table based on the filters from the columns in the table.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Filters are not constantly evaluated as data within the table changes. Filters are applied to the table only when they are 
		/// added or removed on a column in the table or when the ReapplyFilters method is called.
		/// </p>
		/// <p class="body">
		/// If no columns in the table have filters set, this method will not do anything to the data.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> When the filters are reevaluated, the rows of any cells which don't meet the filter criteria of their column will 
		/// be hidden. When a row is filtered out, the entire row is hidden from the worksheet, so any data outside the table but in the 
		/// same row will also be hidden.
		/// </p>
		/// </remarks>
		/// <seealso cref="ClearFilters"/>
		/// <seealso cref="WorksheetTableColumn.Filter"/>
		public void ReapplyFilters()
		{
			if (0 < _suspendFilteringCount)
			{
				this.RefilterRequired = true;
				return;
			}

			if (_worksheet == null)
				return;

			int dataAreaTopRowIndex;
			int dataAreaBottomRowIndex;
			this.GetDataAreaRowIndexes(out dataAreaTopRowIndex, out dataAreaBottomRowIndex);

			List<IFilterable> filteredColumns = new List<IFilterable>();
			for (int i = 0; i < this.Columns.Count; i++)
			{
				WorksheetTableColumn column = this.Columns[i];
				if (column.Filter != null)
					filteredColumns.Add(column);
			}

			_worksheet.ReapplyFilters(dataAreaTopRowIndex, dataAreaBottomRowIndex, filteredColumns);
		}

		#endregion // ReapplyFilters

		#region ReapplySortConditions

		/// <summary>
		/// Re-sorts all data cells in the table based on the sort conditions from the columns in the table.
		/// </summary>
		/// <p class="body">
		/// Sort conditions are not constantly evaluated as data within the table changes. Sort conditions are applied to the table only when 
		/// they are are added or removed on a column in the table or when the ReapplySortCondition method is called.
		/// </p>
		/// <p class="body">
		/// If no columns in the table have sort conditions set, this method will not do anything to the data.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> When the sort conditions are reevaluated, only visible data is sorted. If any rows in the data area of the table are
		/// hidden, the data from those rows will not be sorted.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> When the sort conditions are reevaluated, the cells are moved, but not the rows. Therefore, data outside the table
		/// but in rows which intersect the table's data area will not be moved. If any moved cells within the tables contain formulas, the 
		/// relative references to other cells will be shifted as much as the cell moves. For example, assume cell B5 is in a table and it has 
		/// a formula which references cell C10. If, due to sorting, the cell is shifted down to B12, the reference will also be shifted by 
		/// the same amount and the formula will now reference cell C17. However, updates like this will not be made to formulas referencing 
		/// the moved cells. So a formula referencing a moved cell will reference the new cell which replaces it after the sort is applied.
		/// </p>
		/// <seealso cref="SortSettings"/>
		/// <see cref="Sorting.SortSettings&lt;WorksheetTableColumn&gt;.SortConditions"/>
		/// <seealso cref="WorksheetTableColumn.SortCondition"/>
		public void ReapplySortConditions()
		{
			this.SortSettings.ReapplySortConditionsToRows();
		}

		#endregion // ReapplySortConditions

		
		//
		#region Resize

		//public void Resize(string regionAddress)
		//{
		//    if (this.Worksheet == null)
		//        throw new InvalidOperationException("The table must be on a worksheet to be resized."); // TODO_LOCALIZE

		//    this.ResizeHelper(this.Worksheet.GetRegion(regionAddress), "regionAddress");
		//}

		//public void Resize(WorksheetRegion region)
		//{
		//    if (this.Worksheet == null)
		//        throw new InvalidOperationException("The table must be on a worksheet to be resized."); // TODO_LOCALIZE

		//    if (region.Worksheet != this.Worksheet)
		//        throw new ArgumentException("The specified region not from the same worksheet as the table.", "region"); // TODO_LOCALIZE

		//    this.ResizeHelper(region, "region");
		//}

		//private void ResizeHelper(WorksheetRegion region, string paramName)
		//{
		//    Debug.Assert(this.IsResizing == false, "We should not get in here twice.");

		//    try
		//    {
		//        this.IsResizing = true;

		//        WorksheetRegion wholeTableRegion = this.WholeTableRegion;
		//        if (wholeTableRegion == null)
		//        {
		//            Utilities.DebugFail("We should have a WholeTableRegion here");
		//            return;
		//        }

		//        Worksheet worksheet = this.Worksheet;
		//        Debug.Assert(worksheet != null, "This is unexpected.");

		//        if (worksheet == null || region.TopRow != wholeTableRegion.TopRow)
		//            throw new ArgumentException("The new table region must be from the same worksheet as the table.", paramName); // TODO_LOCALIZE

		//        if (wholeTableRegion.IntersectsWith(region) == false)
		//            throw new ArgumentException("The new table region must overlap with the previous table region.", paramName); // TODO_LOCALIZE

		//        // TODO_Resize_12_1: If the totals row is visible, it needs to be shifted, and all formulas referencing the totals cells need to be updated.
		//        // TODO_Resize_12_1: When columns are removed, all StructuredTableReferences pointing to the removed columns need to be converted to #REF! errors.

		//        WorksheetRegionAddress newAddress = region.Address;

		//        List<WorksheetTableColumn> columnsBeingRemoved = new List<WorksheetTableColumn>();

		//        WorksheetTableColumnCollection columns = this.Columns;
		//        if (_wholeTableAddress.FirstColumnIndex < newAddress.FirstColumnIndex)
		//        {
		//            for (short columnIndex = _wholeTableAddress.FirstColumnIndex;
		//                columnIndex < newAddress.FirstColumnIndex;
		//                columnIndex++)
		//            {
		//                columnsBeingRemoved.Add(columns[columnIndex - _wholeTableAddress.FirstColumnIndex]);
		//            }
		//        }

		//        if (newAddress.LastColumnIndex < _wholeTableAddress.LastColumnIndex)
		//        {
		//            for (short columnIndex = (short)(newAddress.LastColumnIndex + 1);
		//                columnIndex < _wholeTableAddress.LastColumnIndex;
		//                columnIndex++)
		//            {
		//                columnsBeingRemoved.Add(columns[columnIndex - _wholeTableAddress.FirstColumnIndex]);
		//            }
		//        }

		//        if (this.IsTotalsRowVisible)
		//        {
		//            int totalsRowIndex = this.TotalsRow.Index;
		//            if (newAddress.LastRowIndex < totalsRowIndex)
		//            {
		//                newAddress = new WorksheetRegionAddress(
		//                    newAddress.FirstRowIndex, newAddress.LastRowIndex + 1,
		//                    newAddress.FirstColumnIndex, newAddress.LastColumnIndex);
		//            }

		//            CellShiftResult? result = null;
		//            if (totalsRowIndex < newAddress.LastRowIndex)
		//            {
		//                // If the totals row needs to be moved down, rotate the region up so the totals row wraps to under the 
		//                // new rows being added.
		//                result = worksheet.RotateCellsVertically(
		//                    totalsRowIndex, newAddress.LastRowIndex,
		//                    _wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex,
		//                    -1);
		//            }
		//            else if (newAddress.LastRowIndex < totalsRowIndex)
		//            {
		//                // If the totals row needs to be moved up, rotate the region down so the totals row wraps to above the 
		//                // old rows being removed.
		//                result = worksheet.RotateCellsVertically(
		//                    newAddress.LastRowIndex, totalsRowIndex,
		//                    _wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex,
		//                    1);
		//            }

		//            if (result != null && result.Value != CellShiftResult.Success)
		//            {
		//                // TODO_Resize_12_1: Figure out what to do here
		//                Utilities.DebugFail("TODO_Resize_12_1: Figure out what to do here");
		//            }
		//        }

		//        List<Formula> formulasReferencingTable = new List<Formula>();

		//        Workbook workbook = this.Workbook;
		//        if (workbook != null)
		//            workbook.OnTableResizing(this, columnsBeingRemoved, formulasReferencingTable);

		//        WorksheetRegionAddress _originalWholeTableAddress = _wholeTableAddress;
		//        _wholeTableAddress = newAddress;

		//        #region Update Columns Collection

		//        int tableColumnIndex = 0;
		//        if (_originalWholeTableAddress.FirstColumnIndex < _wholeTableAddress.FirstColumnIndex)
		//        {
		//            for (short columnIndex = _originalWholeTableAddress.FirstColumnIndex;
		//                columnIndex < _wholeTableAddress.FirstColumnIndex;
		//                columnIndex++)
		//            {
		//                columns.InternalRemoveAt(0);
		//            }
		//        }
		//        else if (_wholeTableAddress.FirstColumnIndex < _originalWholeTableAddress.FirstColumnIndex)
		//        {
		//            for (short columnIndex = _wholeTableAddress.FirstColumnIndex;
		//                columnIndex < _originalWholeTableAddress.FirstColumnIndex;
		//                columnIndex++, tableColumnIndex++)
		//            {
		//                this.InsertColumnAt(tableColumnIndex);
		//            }
		//        }

		//        if (_wholeTableAddress.Width < columns.Count)
		//        {
		//            for (; tableColumnIndex < _wholeTableAddress.Width; tableColumnIndex++)
		//                columns[tableColumnIndex].ResetCache();

		//            while (_wholeTableAddress.Width < columns.Count)
		//            {
		//                columns.InternalRemoveAt(columns.Count - 1);
		//            }
		//        }
		//        else
		//        {
		//            for (; tableColumnIndex < columns.Count; tableColumnIndex++)
		//                columns[tableColumnIndex].ResetCache();

		//            for (; tableColumnIndex < _wholeTableAddress.Width; tableColumnIndex++)
		//                this.InsertColumn();
		//        }

		//        this.AssignUniqueColumnNames();

		//        if (this.IsHeaderRowVisible)
		//            this.ApplyColumnNamesToHeaderCells();

		//        #endregion // Update Columns Collection

		//        for (int i = 0; i < formulasReferencingTable.Count; i++)
		//            formulasReferencingTable[i].OnTableResized();

		//        if (workbook != null)
		//            workbook.NotifyTableDirtied(this);
		//    }
		//    finally
		//    {
		//        this.IsResizing = false;
		//    }
		//}

		#endregion // Resize

		#endregion // Public Methods

		#region Internal Methods

		#region AssignUniqueColumnNames

		internal void AssignUniqueColumnNames()
		{
			if (this.PreventAssigningUniqueColumnNames)
				return;

			try
			{
				this.PreventAssigningUniqueColumnNames = true;

				Dictionary<WorksheetTableColumn, string> newColumnNames = new Dictionary<WorksheetTableColumn, string>();
				List<KeyValuePair<WorksheetTableColumn, string>> changedColumnNames = new List<KeyValuePair<WorksheetTableColumn, string>>();

				string columnBaseName = SR.GetString("GenerateTableColumnName");
				int columnNamePadder = 1;
				Dictionary<string, bool> usedNamed = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);

				// MD 4/9/12 - TFS101506
				CultureInfo culture = this.Culture;

				for (int tableColumnIndex = 0; tableColumnIndex < this.Columns.Count; tableColumnIndex++)
				{
					WorksheetTableColumn column = this.Columns[tableColumnIndex];

					string originalName = column.Name;

					string columnName = originalName;
					if (String.IsNullOrEmpty(columnName))
					{
						originalName = columnBaseName;

						bool foundMatch;
						do
						{
							columnName = originalName + columnNamePadder++;

							foundMatch = false;
							for (int nextColumnIndex = tableColumnIndex + 1;
								nextColumnIndex < this.Columns.Count;
								nextColumnIndex++)
							{
								// MD 4/9/12 - TFS101506
								//if (String.Equals(columnName, this.Columns[nextColumnIndex].Name, StringComparison.CurrentCultureIgnoreCase))
								if (String.Compare(columnName, this.Columns[nextColumnIndex].Name, culture, CompareOptions.IgnoreCase) == 0)
								{
									foundMatch = true;
									break;
								}
							}
						}
						while (foundMatch);
					}

					while (usedNamed.ContainsKey(columnName))
					{
						// This is a bit weird, but in Excel, when creating new column names, the values start at 1, but
						// when assigning suffixes to make names unique, the values start at 2, but both operations share
						// the same suffix value. So if the value is still 1 and we are assigning a suffix, increment it.
						if (columnNamePadder == 1)
							columnNamePadder++;

						columnName = originalName + columnNamePadder++;
					}

					column.Name = columnName;
					usedNamed.Add(columnName, true);
					newColumnNames.Add(column, columnName);

					if (_columnNames != null)
					{
						string oldName;
						if (_columnNames.TryGetValue(column, out oldName) && oldName != columnName)
							changedColumnNames.Add(new KeyValuePair<WorksheetTableColumn, string>(column, oldName));
					}
				}

				_columnNames = newColumnNames;

				if (changedColumnNames.Count != 0)
				{
					Workbook workbook = this.Workbook;
					if (workbook != null)
						workbook.OnTableColumnsRenamed(this, changedColumnNames);
				}
			}
			finally
			{
				this.PreventAssigningUniqueColumnNames = false;
			}
		}

		#endregion // AssignUniqueColumnNames

		#region CanAreaFormatValueBeSet

		internal static bool CanAreaFormatValueBeSet(WorksheetTableArea area, CellFormatValue value)
		{
			string message;
			return WorksheetTable.VerifyAreaFormatValueCanBeSetHelper(area, value, out message);
		}

		#endregion // CanAreaFormatValueBeSet

		#region Contains

		internal bool Contains(WorksheetRow row, short columnIndex)
		{
			if (_worksheet == null || row.Worksheet != _worksheet)
				return false;

			return this.Contains(row.Index, columnIndex);
		}

		internal bool Contains(int rowIndex, short columnIndex)
		{
			return _wholeTableAddress.Contains(rowIndex, columnIndex);
		}

		#endregion // Contains

		#region GetDataAreaRowIndexes

		internal void GetDataAreaRowIndexes(out int dataAreaTopRowIndex, out int dataAreaBottomRowIndex)
		{
			dataAreaTopRowIndex = _wholeTableAddress.FirstRowIndex;
			if (this.IsHeaderRowVisible)
				dataAreaTopRowIndex++;

			dataAreaBottomRowIndex = _wholeTableAddress.LastRowIndex;
			if (this.IsTotalsRowVisible)
				dataAreaBottomRowIndex--;
		}

		#endregion // GetDataAreaRowIndexes

		#region GetTableAreaOfRow

		internal WorksheetTableArea GetTableAreaOfRow(WorksheetRow row)
		{
			if (this.IsHeaderRowVisible && row.Index == _wholeTableAddress.FirstRowIndex)
				return WorksheetTableArea.HeaderRow;

			if (this.IsTotalsRowVisible && row.Index == _wholeTableAddress.LastRowIndex)
				return WorksheetTableArea.TotalsRow;

			Debug.Assert(_wholeTableAddress.FirstRowIndex <= row.Index && row.Index <= _wholeTableAddress.LastRowIndex, "This is unexpected.");
			return WorksheetTableArea.DataArea;
		}

		#endregion // GetTableAreaOfRow

		#region InitializeAreaFormats

		internal void InitializeAreaFormats()
		{
			Debug.Assert(this.IsTotalsRowVisible == false, "There should be no total cell at this point, so we shouldn't have to initialize anything.");

			Workbook workbook = this.Workbook;

			for (int i = 0; i < this.Columns.Count; i++)
				this.Columns[i].InitializeAreaFormats();

			WorksheetTableColumn firstColumn = this.Columns[0];
			WorksheetCellFormatData headerAreaFormat = firstColumn.AreaFormats.GetFormatElement(workbook, WorksheetTableColumnArea.HeaderCell);
			WorksheetCellFormatData dataAreaFormat = firstColumn.AreaFormats.GetFormatElement(workbook, WorksheetTableColumnArea.DataArea);
			WorksheetCellFormatData totalAreaFormat = firstColumn.AreaFormats.GetFormatElement(workbook, WorksheetTableColumnArea.TotalCell);

			for (int i = 1; i < this.Columns.Count; i++)
			{
				WorksheetTableColumn nextColumn = this.Columns[i];

				if (headerAreaFormat != null)
				{
					WorksheetCellFormatData nextHeaderAreaFormat = nextColumn.AreaFormats.GetFormatElement(workbook, WorksheetTableColumnArea.HeaderCell);
					if (headerAreaFormat.Equals(nextHeaderAreaFormat) == false)
						headerAreaFormat = null;
				}

				if (dataAreaFormat != null)
				{
					WorksheetCellFormatData nextDataAreaFormat = nextColumn.AreaFormats.GetFormatElement(workbook, WorksheetTableColumnArea.DataArea);
					if (dataAreaFormat.Equals(nextDataAreaFormat) == false)
						dataAreaFormat = null;
				}

				if (totalAreaFormat != null)
				{
					WorksheetCellFormatData nextTotalAreaFormat = nextColumn.AreaFormats.GetFormatElement(workbook, WorksheetTableColumnArea.TotalCell);
					if (totalAreaFormat.Equals(nextTotalAreaFormat) == false)
						totalAreaFormat = null;
				}
			}

			try
			{
				this.SuspendAreaFormatSynchronization = true;

				if (headerAreaFormat != null)
					this.AreaFormats[WorksheetTableArea.HeaderRow].SetFormatting(headerAreaFormat);

				if (dataAreaFormat != null)
					this.AreaFormats[WorksheetTableArea.DataArea].SetFormatting(dataAreaFormat);

				if (totalAreaFormat != null)
					this.AreaFormats[WorksheetTableArea.TotalsRow].SetFormatting(totalAreaFormat);

				Dictionary<CellFormatValue, object> borders = WorksheetMergedCellsRegion.GetEdgeBorderValues(this.Worksheet, this.WholeTableAddress, null, false);

				WorksheetCellFormatProxy wholeTableFormat = this.AreaFormats.GetFormatProxy(workbook, WorksheetTableArea.WholeTable);
				foreach (KeyValuePair<CellFormatValue, object> pair in borders)
					wholeTableFormat.SetValue(pair.Key, pair.Value);
			}
			finally
			{
				this.SuspendAreaFormatSynchronization = false;
			}
		}

		#endregion // InitializeAreaFormats

		#region InitializeColumnFormulas

		internal void InitializeColumnFormulas()
		{
			// Initialize the column formulas from the cells in the column if more than half of the cells at the bottom of 
			// the column have equivalent formulas.
			WorksheetRegion dataArea = this.DataAreaRegion;
			if (dataArea == null || dataArea.Worksheet == null)
				return;

			for (int i = 0; i < this.Columns.Count; i++)
			{
				WorksheetTableColumn column = this.Columns[i];

				Formula columnFormula = null;
				int requiredNumberOfEquivalentFormulas = (dataArea.Height / 2) + 1;
				int firstRowIndexToTest = dataArea.LastRow - requiredNumberOfEquivalentFormulas + 1;

				for (int rowIndex = firstRowIndexToTest;
					rowIndex <= dataArea.LastRow;
					rowIndex++)
				{
					WorksheetRow row = _worksheet.Rows.GetIfCreated(rowIndex);

					if (row == null)
					{
						columnFormula = null;
					}
					else
					{
						Formula formula = row.GetCellFormulaInternal(column.WorksheetColumnIndex);

						if (columnFormula == null)
							columnFormula = formula;
						else if (formula == null || formula.IsEquivalentTo(columnFormula) == false)
							columnFormula = null;
					}

					if (columnFormula == null)
						break;
				}

				if (columnFormula != null)
				{
					columnFormula = columnFormula.Clone();

					// MD 7/12/12 - TFS109194
					// OffsetReferences now needs a reference to the workbook.
					//columnFormula.OffsetReferences(new Point(0, dataArea.FirstRow - firstRowIndexToTest));
					columnFormula.OffsetReferences(this.Workbook, new Point(0, dataArea.FirstRow - firstRowIndexToTest));

					column.SetInitialColumnFormula(columnFormula);
				}
			}
		}

		#endregion // InitializeColumnFormulas

		#region InitializeSerializationManager

		internal void InitializeSerializationManager(WorkbookSerializationManager manager)
		{
			foreach (WorksheetTableColumn column in this.Columns)
				column.InitializeSerializationManager(manager);
		}

		#endregion // InitializeSerializationManager

		#region InsertColumn

		internal WorksheetTableColumn InsertColumn()
		{
			return this.InsertColumnHelper(this.Columns.Count, _nextColumnId);
		}

		internal WorksheetTableColumn InsertColumn(uint id)
		{





			return this.InsertColumnHelper(this.Columns.Count, id);
		}

		private WorksheetTableColumn InsertColumnHelper(int index, uint id)
		{
			_nextColumnId = Math.Max(id + 1, _nextColumnId);
			int absoluteColumnIndex = _wholeTableAddress.FirstColumnIndex + index;
			WorksheetTableColumn column = new WorksheetTableColumn(this, id, (short)absoluteColumnIndex);
			_columns.InternalInsert(index, column);
			return column;
		}

		#endregion // InsertColumn

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region InsertColumnAt

		internal WorksheetTableColumn InsertColumnAt(int index)
		{
			return this.InsertColumnHelper(index, _nextColumnId);
		}

		#endregion // InsertColumnAt

		#region OnRemovedFromWorkbook

		internal void OnRemovedFromWorkbook(Workbook oldWorkbook)
		{
			if (oldWorkbook != null)
				oldWorkbook.OnTableRemoved(this);

			if (_areaFormats != null)
				_areaFormats.OnUnrooted();

			if (_columns != null)
			{
				for (int i = 0; i < _columns.Count; i++)
					_columns[i].AreaFormats.OnUnrooted();
			}

			this.ScopeInternal = null;
			this.Style = null;
		}

		#endregion // OnRemovedFromWorkbook

		#region OnRemovedFromWorksheet

		internal void OnRemovedFromWorksheet()
		{
			// MD 5/4/12 - TFS107064
			// Excel will clear the filters before converting a table to a range, so we should as well.
			this.ClearFilters();

			this.ApplyStyleAreaFormatsToCells();

			Workbook oldWorkbook = this.Workbook;

			WorksheetRow headerRow = this.HeaderRow;
			if (headerRow != null)
			{
				for (short columnIndex = _wholeTableAddress.FirstColumnIndex; columnIndex <= _wholeTableAddress.LastColumnIndex; columnIndex++)
					headerRow.SetIsInTableHeaderOrTotalRow(columnIndex, false);
			}

			WorksheetRow totalsRow = this.TotalsRow;
			if (totalsRow != null)
			{
				for (short columnIndex = _wholeTableAddress.FirstColumnIndex; columnIndex <= _wholeTableAddress.LastColumnIndex; columnIndex++)
					totalsRow.SetIsInTableHeaderOrTotalRow(columnIndex, false);
			}

			if (_columns != null)
			{
				for (int i = 0; i < _columns.Count; i++)
					_columns[i].OnTableRemovedFromCollection();
			}

			this.OnRemovedFromWorkbook(oldWorkbook);
			_worksheet = null;
		}

		#endregion // OnRemovedFromWorksheet

		#region OnRooted

		internal void OnRooted(Worksheet worksheet)
		{
			Debug.Assert(_worksheet == null, "This table already belongs to a worksheet.");
			_worksheet = worksheet;

			Workbook workbook = _worksheet.Workbook;
			if (workbook != null)
			{
				Debug.Assert(
					this.Style.IsCustom == false || (this.Style.CustomCollection != null && this.Style.CustomCollection.Workbook == workbook),
					"The table style does not below to the same workbook as the table.");

				this.ScopeInternal = workbook;
				workbook.OnNamedReferenceAdded(this);

				if (_areaFormats != null)
					_areaFormats.OnRooted(workbook);
			}

			if (_columns != null)
			{
				for (int i = 0; i < _columns.Count; i++)
					_columns[i].OnRooted(workbook);
			}

			this.AssignUniqueColumnNames();

			if (this.IsHeaderRowVisible)
				this.ApplyColumnNamesToHeaderCells();

			if (this.IsTotalsRowVisible)
				this.ApplyTotalsToTotalCells();
		}

		#endregion // OnRooted

		#region ProcessCellShiftResult

		internal static void ProcessCellShiftResult(CellShiftResult shiftResult, string rowDescription)
		{
			switch (shiftResult)
			{
				case CellShiftResult.Success:
					break;

				case CellShiftResult.ErrorLossOfData:
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotInsertTableRow_LossOfData", rowDescription));

				case CellShiftResult.ErrorLossOfObject:
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotInsertTableRow_LossOfObject", rowDescription));

				case CellShiftResult.ErrorSplitTable:
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotInsertTableRow_SplitTable", rowDescription));

				case CellShiftResult.ErrorSplitMergedRegion:
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotInsertTableRow_SplitMergedRegion", rowDescription));

				case CellShiftResult.ErrorSplitBlockingValue:
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotInsertTableRow_SplitBlockingValue", rowDescription));

				default:
					Utilities.DebugFail("Unknown CellShiftResult: " + shiftResult);
					goto case CellShiftResult.ErrorLossOfData;
			}
		}

		#endregion // ProcessCellShiftResult

		#region ResumeFiltering

		internal void ResumeFiltering()
		{
			_suspendFilteringCount--;
			Debug.Assert(_suspendFilteringCount >= 0, "The _suspendFilteringCount is invalid.");

			if (_suspendFilteringCount <= 0 && this.RefilterRequired)
			{
				this.RefilterRequired = false;
				this.ReapplyFilters();
			}
		}

		#endregion // ResumeFiltering

		#region ShiftTable

		internal ShiftAddressResult ShiftTable(CellShiftOperation shiftOperation)
		{
			ShiftAddressResult result = shiftOperation.ShiftRegionAddress(ref _wholeTableAddress, false);
			Debug.Assert(result.IsDeleted == false, "A shift should not be allowed if it will remove a table.");
			return result;
		}

		#endregion // ShiftTable

		#region SuspendFiltering

		internal void SuspendFiltering()
		{
			Debug.Assert(_suspendFilteringCount >= 0, "The _suspendFilteringCount is invalid.");
			_suspendFilteringCount++;
		}

		#endregion // SuspendFiltering

		#endregion // Internal Methods

		#region Private Methods

		#region ApplyColumnNamesToHeaderCells

		private void ApplyColumnNamesToHeaderCells()
		{
			WorksheetRow headerRow = this.HeaderRow;
			if (headerRow == null)
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}

			bool isLoading = false;
			Workbook workbook = this.Workbook;
			if (workbook != null)
				isLoading = workbook.IsLoading;

			try
			{
				Debug.Assert(this.PreventAssigningUniqueColumnNames == false, "This is unexpected. If this is now valid, cache the old value and restore it in the finally below.");
				this.PreventAssigningUniqueColumnNames = true;

				for (short tableColumnIndex = 0; tableColumnIndex < this.Columns.Count; tableColumnIndex++)
				{
					WorksheetTableColumn column = this.Columns[tableColumnIndex];

					short columnIndex = (short)(tableColumnIndex + _wholeTableAddress.FirstColumnIndex);
					headerRow.SetIsInTableHeaderOrTotalRow(columnIndex, true);

					if (isLoading == false)
						headerRow.SetCellValue(columnIndex, column.NameInternal);
				}
			}
			finally
			{
				this.PreventAssigningUniqueColumnNames = false;
			}
		}

		#endregion // ApplyColumnNamesToHeaderCells

		#region ApplyStyleAreaFormatsToCells

		private void ApplyStyleAreaFormatsToCells()
		{
			WorksheetTableStyle style = this.Style;
			if (style == null)
				return;

			SortedList<int, WorksheetTableAreaFormatProxy<WorksheetTableStyleArea>> styleAreaFormats =
				new SortedList<int, WorksheetTableAreaFormatProxy<WorksheetTableStyleArea>>();

			foreach (WorksheetTableAreaFormatProxy<WorksheetTableStyleArea> proxy in style.AreaFormats.GetFormatProxies())
				styleAreaFormats[WorksheetTableStyle.GetAreaPrecedence(proxy.Area)] = proxy;

			WorksheetRegion dataAreaRegion = this.DataAreaRegion;
			foreach (WorksheetCell cell in this.WholeTableRegion)
			{
				for (int i = styleAreaFormats.Count - 1; i >= 0; i--)
				{
					WorksheetTableAreaFormatProxy<WorksheetTableStyleArea> styleAreaFormat = styleAreaFormats.Values[i];
					if (this.IsCellInStyleArea(cell, styleAreaFormat.Area, dataAreaRegion) == false)
						continue;

					// MD 5/4/12 - TFS107276
					// For the WholeTable, we need to do something special, because the border properties should only be 
					// copied to certain cells.
					if (styleAreaFormat.Area == WorksheetTableStyleArea.WholeTable)
					{
						if (cell.RowIndex == _wholeTableAddress.FirstRowIndex)
						{
							cell.CellFormatInternal.InitializeDefaultValuesFrom(styleAreaFormat,
								CellFormatValue.TopBorderColorInfo, CellFormatValue.TopBorderStyle);
						}

						if (cell.RowIndex == _wholeTableAddress.LastRowIndex)
						{
							cell.CellFormatInternal.InitializeDefaultValuesFrom(styleAreaFormat,
								CellFormatValue.BottomBorderColorInfo, CellFormatValue.BottomBorderStyle);
						}

						if (cell.ColumnIndexInternal == _wholeTableAddress.FirstColumnIndex)
						{
							cell.CellFormatInternal.InitializeDefaultValuesFrom(styleAreaFormat,
								CellFormatValue.LeftBorderColorInfo, CellFormatValue.LeftBorderStyle);
						}

						if (cell.ColumnIndexInternal == _wholeTableAddress.LastColumnIndex)
						{
							cell.CellFormatInternal.InitializeDefaultValuesFrom(styleAreaFormat,
								CellFormatValue.RightBorderColorInfo, CellFormatValue.RightBorderStyle);
						}

						continue;
					}

					cell.CellFormatInternal.InitializeDefaultValuesFrom(styleAreaFormat);
				}
			}
		}

		#endregion // ApplyStyleAreaFormatsToCells

		#region ApplyTotalsToTotalCells

		private void ApplyTotalsToTotalCells()
		{
			WorksheetRow totalsRow = this.TotalsRow;
			if (totalsRow == null)
			{
				Utilities.DebugFail("This is unexpected.");
				return;
			}

			bool isLoading = false;
			Workbook workbook = this.Workbook;
			if (workbook != null)
				isLoading = workbook.IsLoading;

			for (short tableColumnIndex = 0; tableColumnIndex < this.Columns.Count; tableColumnIndex++)
			{
				WorksheetTableColumn column = this.Columns[tableColumnIndex];

				short columnIndex = (short)(tableColumnIndex + _wholeTableAddress.FirstColumnIndex);
				totalsRow.SetIsInTableHeaderOrTotalRow(columnIndex, true);

				if (isLoading == false)
				{
					if (column.TotalFormulaInternal != null)
					{
						column.TotalFormulaInternal.ApplyTo(totalsRow, columnIndex);
					}
					else
					{
						string totalLabel = column.TotalLabelInternal;
						if (string.IsNullOrEmpty(totalLabel) == false)
							totalsRow.SetCellValue(columnIndex, totalLabel);
						else
							Debug.Assert(totalsRow.GetCellValueInternal(columnIndex) == null, "The totals value should be null here.");
					}
				}
			}
		}

		#endregion // ApplyTotalsToTotalCells

		#region ClearHeaderOrTotalCells

		private void ClearHeaderOrTotalCells(int headerOrTotalRowIndex)
		{
			WorksheetRow headerOrTotalRow = this.Worksheet.Rows.GetIfCreated(headerOrTotalRowIndex);

			if (headerOrTotalRow == null)
				return;

			for (short columnIndex = _wholeTableAddress.FirstColumnIndex; columnIndex <= _wholeTableAddress.LastColumnIndex; columnIndex++)
			{
				headerOrTotalRow.SetIsInTableHeaderOrTotalRow(columnIndex, false);
				headerOrTotalRow.SetCellValue(columnIndex, null);

				WorksheetCellFormatProxy format;
				if (headerOrTotalRow.TryGetCellFormat(columnIndex, out format))
				{
					format.Style = null;
					format.FormatOptions = WorksheetCellFormatOptions.None;
				}
			}
		}

		#endregion // ClearHeaderOrTotalCells

		#region GetFlag

		private bool GetFlag(TableFlags flag)
		{
			return (_flags & flag) == flag;
		}

		#endregion // GetFlag

		#region IsCellInStyleArea

		private bool IsCellInStyleArea(WorksheetCell cell, WorksheetTableStyleArea area, WorksheetRegion dataAreaRegion)
		{
			switch (area)
			{
				case WorksheetTableStyleArea.WholeTable:
					// MD 5/4/12 - TFS107276
					// Not all cells have the WholeTable style applied. Only the edge cells do because the WholeTable only contains
					// outer border information.
					//return true;
					return
						cell.ColumnIndexInternal == _wholeTableAddress.FirstColumnIndex ||
						cell.ColumnIndexInternal == _wholeTableAddress.LastColumnIndex ||
						cell.RowIndex == _wholeTableAddress.FirstRowIndex ||
						cell.RowIndex == _wholeTableAddress.LastRowIndex;

				case WorksheetTableStyleArea.ColumnStripe:
				case WorksheetTableStyleArea.AlternateColumnStripe:
					{
						if (this.DisplayBandedColumns == false || dataAreaRegion.Contains(cell) == false)
							return false;

						int columnStripesWidth = this.Style.ColumnStripeWidth + this.Style.AlternateColumnStripeWidth;
						int relativeIndexInStripes = (cell.ColumnIndex - this.WholeTableAddress.FirstColumnIndex) % columnStripesWidth;

						if (area == WorksheetTableStyleArea.ColumnStripe)
							return relativeIndexInStripes < this.Style.ColumnStripeWidth;

						return this.Style.ColumnStripeWidth <= relativeIndexInStripes;
					}

				case WorksheetTableStyleArea.RowStripe:
				case WorksheetTableStyleArea.AlternateRowStripe:
					{
						if (this.DisplayBandedRows == false || dataAreaRegion.Contains(cell) == false)
							return false;

						int rowStripesHeight = this.Style.RowStripeHeight + this.Style.AlternateRowStripeHeight;
						int relativeIndexInStripes = (cell.RowIndex - dataAreaRegion.FirstRow) % rowStripesHeight;

						if (area == WorksheetTableStyleArea.RowStripe)
							return relativeIndexInStripes < this.Style.RowStripeHeight;

						return this.Style.RowStripeHeight <= relativeIndexInStripes;
					}

				case WorksheetTableStyleArea.LastColumn:
					{
						if (this.DisplayLastColumnFormatting == false)
							return false;

						return (cell.ColumnIndex == this.WholeTableAddress.LastColumnIndex);
					}

				case WorksheetTableStyleArea.FirstColumn:
					{
						if (this.DisplayFirstColumnFormatting == false)
							return false;

						return (cell.ColumnIndex == this.WholeTableAddress.FirstColumnIndex);
					}

				case WorksheetTableStyleArea.HeaderRow:
					return (this.IsHeaderRowVisible && cell.RowIndex == this.WholeTableAddress.FirstRowIndex);

				case WorksheetTableStyleArea.TotalRow:
					return (this.IsTotalsRowVisible && cell.RowIndex == this.WholeTableAddress.LastRowIndex);

				case WorksheetTableStyleArea.FirstHeaderCell:
					{
						if (this.DisplayFirstColumnFormatting == false)
							return false;

						return (this.IsHeaderRowVisible &&
							cell.RowIndex == this.WholeTableAddress.FirstRowIndex &&
							cell.ColumnIndex == this.WholeTableAddress.FirstColumnIndex);
					}

				case WorksheetTableStyleArea.LastHeaderCell:
					{
						if (this.DisplayLastColumnFormatting == false)
							return false;

						return (this.IsHeaderRowVisible &&
							cell.RowIndex == this.WholeTableAddress.FirstRowIndex &&
							cell.ColumnIndex == this.WholeTableAddress.LastColumnIndex);
					}

				case WorksheetTableStyleArea.FirstTotalCell:
					{
						if (this.DisplayFirstColumnFormatting == false)
							return false;

						return (this.IsTotalsRowVisible &&
							cell.RowIndex == this.WholeTableAddress.LastRowIndex &&
							cell.ColumnIndex == this.WholeTableAddress.FirstColumnIndex);
					}

				case WorksheetTableStyleArea.LastTotalCell:
					{
						if (this.DisplayLastColumnFormatting == false)
							return false;

						return (this.IsTotalsRowVisible &&
							cell.RowIndex == this.WholeTableAddress.LastRowIndex &&
							cell.ColumnIndex == this.WholeTableAddress.LastColumnIndex);
					}

				default:
					Utilities.DebugFail("Unknown WorksheetTableStyleArea: " + area);
					return false;
			}
		}

		#endregion // IsCellInStyleArea

		#region OnIsHeaderRowVisibleChanged

		private void OnIsHeaderRowVisibleChanged()
		{
			this.IsFilterUIVisible = this.IsHeaderRowVisible;

			if (this.Worksheet == null)
				return;

			Workbook workbook = this.Workbook;
			if (workbook != null)
				workbook.NotifyTableDirtied(this);

			if (this.IsHeaderRowVisible)
			{
				// If the table without the header row is on the top of the worksheet or has data in cells directly
				// above it, the header cells need to be inserted. Otherwise, the header cells can just take over
				// the existing row above the table.
				if (this.ShouldInsertHeaderCells())
				{
					if (this.Worksheet.InsertCellsAndShiftDown(_wholeTableAddress.FirstRowIndex, _wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex, CellShiftInitializeFormatType.UseDefaultFormat) != CellShiftResult.Success)
						Utilities.DebugFail("This is unexpected. We should have verified that the cells could be inserted in OnIsHeaderRowVisibleChanging.");
				}

				_wholeTableAddress.FirstRowIndex--;
				this.ApplyColumnNamesToHeaderCells();

				foreach (WorksheetTableColumn column in this.Columns)
					column.OnHeaderRowShown();
			}
			else
			{
				this.ClearFilters();
				this.ClearHeaderOrTotalCells(_wholeTableAddress.FirstRowIndex++);
			}
		}

		#endregion // OnIsHeaderRowVisibleChanged

		#region OnIsHeaderRowVisibleChanging

		private void OnIsHeaderRowVisibleChanging()
		{
			if (this.Worksheet == null)
				return;

			if (this.IsHeaderRowVisible)
			{
				foreach (WorksheetTableColumn column in this.Columns)
					column.OnHeaderRowHiding();
			}
			else
			{
				// If the header row is about to be displayed and cells need to be inserted, make sure they can be.
				if (this.ShouldInsertHeaderCells())
				{
					string rowDescription = SR.GetString("TableHeaderRowDescription");

					int maxRowIndex = this.Worksheet.Rows.MaxCount - 1;
					Debug.Assert(_wholeTableAddress.LastRowIndex <= maxRowIndex, "This is unexpected.");

					if (maxRowIndex == _wholeTableAddress.LastRowIndex)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotInsertTableRow_TableOnBottomOfWorksheet", rowDescription));

					CellShiftResult shiftResult = this.Worksheet.VerifyCellShift(
						_wholeTableAddress.LastRowIndex + 1, maxRowIndex - 1,
						_wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex,
						1, CellShiftType.VerticalShift);

					WorksheetTable.ProcessCellShiftResult(shiftResult, rowDescription);
				}
			}
		}

		#endregion // OnIsHeaderRowVisibleChanging

		#region OnTotalsRowHidden

		private void OnTotalsRowHidden()
		{
			if (this.Worksheet == null)
				return;

			Workbook workbook = this.Workbook;
			if (workbook != null)
				workbook.NotifyTableDirtied(this);

			int originalLastRowIndex = _wholeTableAddress.LastRowIndex;

			_wholeTableAddress.LastRowIndex--;
			if (this.Worksheet.DeleteCellsAndShiftUp(originalLastRowIndex, _wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex) != CellShiftResult.Success)
			{
				// If deleting the cells would split a merged region, table, or blocking value, we should just clear the total cells and not delete them.
				this.ClearHeaderOrTotalCells(originalLastRowIndex);
			}

			Debug.Assert(
				originalLastRowIndex - 1 == _wholeTableAddress.LastRowIndex,
				"Deleting the cells in the total row should have shifted up the bottom of the table.");
		}

		#endregion // OnTotalsRowHidden

		#region OnTotalsRowHiding

		private void OnTotalsRowHiding()
		{
			if (this.Worksheet == null)
				return;

			foreach (WorksheetTableColumn column in this.Columns)
				column.OnTotalsRowHiding();
		}

		#endregion // OnIsTotalsRowVisibleChanging

		#region OnTotalsRowShowing

		private void OnTotalsRowShowing(out bool shouldTotalsRowBeInserted)
		{
			shouldTotalsRowBeInserted = true;

			if (this.Worksheet == null)
				return;

			Workbook workbook = this.Workbook;

			int maxRowIndex = this.Worksheet.Rows.MaxCount - 1;
			if (_wholeTableAddress.LastRowIndex == maxRowIndex)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_TotalsRowCannotBeShownInLastRow"));

			// If the totals row is about to be displayed, make sure the total cells can be inserted.
			string rowDescription = SR.GetString("TableTotalsRowDescription");

			CellShiftResult shiftResult = this.Worksheet.VerifyCellShift(
				_wholeTableAddress.LastRowIndex + 1, maxRowIndex - 1,
				_wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex,
				1, CellShiftType.VerticalShift);

			if (shiftResult != CellShiftResult.Success)
			{
				if (this.Worksheet.AreCellsNonTrivial(_wholeTableAddress.LastRowIndex + 1, _wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex))
				{
					WorksheetTable.ProcessCellShiftResult(shiftResult, rowDescription);
					Utilities.DebugFail("ProcessCellShiftResult should have throw an exception.");
				}

				shouldTotalsRowBeInserted = false;
			}

			if (workbook != null && this.HasTotalsRowEverBeenVisible == false)
			{
				if (this.Columns.Count > 1)
				{
					WorksheetTableColumn firstColumn = this.Columns[0];

					if (firstColumn.TotalFormula == null && firstColumn.TotalLabel == null)
						firstColumn.TotalLabel = SR.GetString("DefaultTotalLabel");
				}

				WorksheetTableColumn lastColumn = this.Columns[this.Columns.Count - 1];

				if (lastColumn.TotalFormula == null && lastColumn.TotalLabel == null)
				{
					// MD 4/3/12 - TFS107243
					// The type of total function to use is based on the data.
					//lastColumn.TotalFormula = workbook.GetTotalFormula(lastColumn, ST_TotalsRowFunction.sum);
					ST_TotalsRowFunction totalFunction = ST_TotalsRowFunction.count;

					int dataAreaTopRowIndex;
					int dataAreaBottomRowIndex;
					this.GetDataAreaRowIndexes(out dataAreaTopRowIndex, out dataAreaBottomRowIndex);

					Worksheet worksheet = this.Worksheet;
					foreach (WorksheetRow row in worksheet.Rows.GetItemsInRange(dataAreaTopRowIndex, dataAreaBottomRowIndex))
					{
						object value = row.GetCellValueInternal(lastColumn.WorksheetColumnIndex);
						if (value == null)
							continue;

						DateTime dateTime;
						bool isNumber = 
							Utilities.IsNumber(value) && 
							worksheet.TryGetDateTimeFromCell(row, lastColumn.WorksheetColumnIndex, out dateTime) == false;

						if (isNumber)
						{
							totalFunction = ST_TotalsRowFunction.sum;
							continue;
						}

						totalFunction = ST_TotalsRowFunction.count;
						break;
					}

					lastColumn.TotalFormula = workbook.GetTotalFormula(lastColumn, totalFunction);
				}
			}
		}

		#endregion // OnTotalsRowShowing

		#region OnTotalsRowShown

		private void OnTotalsRowShown(bool shouldTotalsRowBeInserted)
		{
			if (this.Worksheet == null)
				return;

			Workbook workbook = this.Workbook;
			if (workbook != null)
				workbook.NotifyTableDirtied(this);

			if (shouldTotalsRowBeInserted)
			{
				CellShiftResult shiftResult = this.Worksheet.InsertCellsAndShiftDown(
					_wholeTableAddress.LastRowIndex + 1, _wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex,
					CellShiftInitializeFormatType.UseDefaultFormat);

				Debug.Assert(shiftResult == CellShiftResult.Success,
					"This is unexpected. We should have verified that the cells could be inserted in OnIsTotalsRowVisibleChanged.");
			}

			_wholeTableAddress.LastRowIndex++;

			this.ApplyTotalsToTotalCells();
			this.HasTotalsRowEverBeenVisible = true;

			foreach (WorksheetTableColumn column in this.Columns)
				column.OnTotalsRowShown();
		}

		#endregion // OnTotalsRowShown

		#region SetFlag

		private void SetFlag(TableFlags flag, bool value)
		{
			if (value)
				_flags |= flag;
			else
				_flags &= ~flag;
		}

		#endregion // SetFlag

		#region ShouldInsertHeaderCells

		private bool ShouldInsertHeaderCells()
		{
			// If the table is on the first row or there is data or referenced cells directly above the table, the header cells 
			// cannot take the row above, so we will need to insert the header cells and shift the table down.
			return
				_wholeTableAddress.FirstRowIndex == 0 ||
				this.Worksheet.AreCellsNonTrivial(_wholeTableAddress.FirstRowIndex - 1, _wholeTableAddress.FirstColumnIndex, _wholeTableAddress.LastColumnIndex);
		}

		#endregion // ShouldInsertHeaderCells

		#region SynchronizeAreaFormatWithColumns

		private void SynchronizeAreaFormatWithColumns(WorksheetTableAreaFormatProxy<WorksheetTableArea> areaFormatProxy, IList<CellFormatValue> values)
		{
			if (this.SuspendAreaFormatSynchronization)
				return;

			if (areaFormatProxy == null || areaFormatProxy.IsEmpty)
				return;

			Workbook workbook = this.Workbook;
			if (workbook != null && workbook.IsLoading)
				return;

			WorksheetTableArea area = areaFormatProxy.Area;

			WorksheetTableColumnArea columnArea;
			switch (area)
			{
				case WorksheetTableArea.WholeTable:
					this.SynchronizeAreaFormatWithWholeTable(areaFormatProxy, values);
					return;

				case WorksheetTableArea.DataArea:
					columnArea = WorksheetTableColumnArea.DataArea;
					break;

				case WorksheetTableArea.HeaderRow:
					columnArea = WorksheetTableColumnArea.HeaderCell;
					break;

				case WorksheetTableArea.TotalsRow:
					columnArea = WorksheetTableColumnArea.TotalCell;
					break;

				default:
					Utilities.DebugFail("Unknown WorksheetTableArea: " + area);
					return;
			}

			List<CellFormatValue> nonDefaultValues = new List<CellFormatValue>();
			for (int i = 0; i < values.Count; i++)
			{
				CellFormatValue value = values[i];
				if (WorksheetTable.CanAreaFormatValueBeSet(area, value) && areaFormatProxy.IsValueDefault(value) == false)
					nonDefaultValues.Add(value);
			}

			if (nonDefaultValues.Count == 0)
				return;

			for (int i = 0; i < this.Columns.Count; i++)
			{
				Utilities.CopyCellFormatValues(
					areaFormatProxy,
					this.Columns[i].AreaFormats.GetFormatProxy(workbook, columnArea),
					nonDefaultValues);
			}
		}

		#endregion // SynchronizeAreaFormatWithColumns

		#region SynchronizeAreaFormatWithWholeTable

		private void SynchronizeAreaFormatWithWholeTable(WorksheetTableAreaFormatProxy<WorksheetTableArea> areaFormatProxy, IList<CellFormatValue> values)
		{
			Worksheet worksheet = this.Worksheet;
			if (worksheet == null)
				return;

			for (int i = 0; i < values.Count; i++)
			{
				CellFormatValue formatProperty = values[i];
				object formatValue = areaFormatProxy.GetValue(formatProperty);

				if (WorksheetCellFormatData.IsValueDefault(formatProperty, formatValue))
					continue;

				switch (formatProperty)
				{
					case CellFormatValue.BottomBorderColorInfo:
					case CellFormatValue.BottomBorderStyle:
					case CellFormatValue.TopBorderColorInfo:
					case CellFormatValue.TopBorderStyle:
						{
							WorksheetRow row;
							if (formatProperty == CellFormatValue.TopBorderColorInfo || formatProperty == CellFormatValue.TopBorderStyle)
								row = worksheet.Rows[_wholeTableAddress.FirstRowIndex];
							else
								row = worksheet.Rows[_wholeTableAddress.LastRowIndex];

							for (short columnIndex = _wholeTableAddress.FirstColumnIndex;
								columnIndex <= _wholeTableAddress.LastColumnIndex;
								columnIndex++)
							{
								row.GetCellFormatInternal(columnIndex).SetValue(formatProperty, formatValue);
							}
						}
						break;

					case CellFormatValue.LeftBorderColorInfo:
					case CellFormatValue.LeftBorderStyle:
					case CellFormatValue.RightBorderColorInfo:
					case CellFormatValue.RightBorderStyle:
						{
							short columnIndex;
							if (formatProperty == CellFormatValue.LeftBorderColorInfo || formatProperty == CellFormatValue.LeftBorderStyle)
								columnIndex = _wholeTableAddress.FirstColumnIndex;
							else
								columnIndex = _wholeTableAddress.LastColumnIndex;

							for (int rowIndex = _wholeTableAddress.FirstRowIndex;
								rowIndex <= _wholeTableAddress.LastRowIndex;
								rowIndex++)
							{
								worksheet.Rows[rowIndex].GetCellFormatInternal(columnIndex).SetValue(formatProperty, formatValue);
							}
						}
						break;

					default:
						Utilities.DebugFail("Unexpected CellFormatValue: " + formatProperty);
						break;
				}
			}
		}

		#endregion // SynchronizeAreaFormatWithWholeTable

		#region VerifyAreaFormatValueCanBeSet

		private static void VerifyAreaFormatValueCanBeSet(WorksheetTableArea area, CellFormatValue value)
		{
			string message;
			if (WorksheetTable.VerifyAreaFormatValueCanBeSetHelper(area, value, out message) == false)
				throw new InvalidOperationException(message);
		}

		private static bool VerifyAreaFormatValueCanBeSetHelper(WorksheetTableArea area, CellFormatValue value, out string message)
		{
			switch (area)
			{
				case WorksheetTableArea.WholeTable:
					switch (value)
					{
						case CellFormatValue.BottomBorderColorInfo:
						case CellFormatValue.BottomBorderStyle:
						case CellFormatValue.LeftBorderColorInfo:
						case CellFormatValue.LeftBorderStyle:
						case CellFormatValue.RightBorderColorInfo:
						case CellFormatValue.RightBorderStyle:
						case CellFormatValue.TopBorderColorInfo:
						case CellFormatValue.TopBorderStyle:
							message = null;
							return true;
					}
					message = SR.GetString("LE_InvalidOperationException_InvalidWholeTableAreaFormatProperty");
					return false;

				case WorksheetTableArea.DataArea:
					message = null;
					return true;

				case WorksheetTableArea.HeaderRow:
					switch (value)
					{
						case CellFormatValue.TopBorderColorInfo:
						case CellFormatValue.TopBorderStyle:
							message = SR.GetString("LE_InvalidOperationException_InvalidHeaderRowAreaFormatProperty");
							return false;
					}

					message = null;
					return true;

				case WorksheetTableArea.TotalsRow:
					switch (value)
					{
						case CellFormatValue.BottomBorderColorInfo:
						case CellFormatValue.BottomBorderStyle:
							message = SR.GetString("LE_InvalidOperationException_InvalidTotalsRowAreaFormatProperty");
							return false;
					}

					message = null;
					return true;

				default:
					Utilities.DebugFail("Unknown WorksheetTableArea: " + area);
					goto case WorksheetTableArea.DataArea;
			}
		}

		#endregion // VerifyAreaFormatValueCanBeSet

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region AreaFormats

		/// <summary>
		/// Gets the collection of formats used for each area of the <see cref="WorksheetTable"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The available areas of the table which can have a format set are the whole table, header, data, and totals areas.
		/// </p>
		/// <p class="body">
		/// Applying a format to an area will apply the format to all cells in that area.
		/// </p>
		/// <p class="body">
		/// If any area formats on the tables are set when the table is resized to give it more rows, the new cells in the table will get the
		/// new format applied.
		/// </p>
		/// </remarks>
		/// <seealso cref="WorksheetTableColumn.Filter"/>
		/// <seealso cref="WorksheetTableColumn.AreaFormats"/>
		/// <seealso cref="WorksheetTableStyle"/>
		
		///// <seealso cref="Resize"/>
		public WorksheetTableAreaFormatsCollection<WorksheetTableArea> AreaFormats
		{
			get
			{
				if (_areaFormats == null)
					_areaFormats = new WorksheetTableAreaFormatsCollection<WorksheetTableArea>(this);

				return _areaFormats;
			}
		}

		#endregion // AreaFormats

		#region Columns

		/// <summary>
		/// Gets the collection of columns in the table.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Each column is represented by a <see cref="WorksheetTableColumn"/> instance and contains various settings for controlling 
		/// the contents, formatting, sorting, and filtering of the column.
		/// </p>
		/// </remarks>
		/// <seealso cref="WorksheetTableColumn"/>
		public WorksheetTableColumnCollection Columns
		{
			get
			{
				if (_columns == null)
					_columns = new WorksheetTableColumnCollection(this);

				return _columns;
			}
		}

		#endregion // Columns

		#region DataAreaRegion

		/// <summary>
		/// Gets the <see cref="WorksheetRegion"/> which represents the region of cells in the data area of the table.
		/// </summary>
		
		///// <seealso cref="Resize"/>
		public WorksheetRegion DataAreaRegion
		{
			get
			{
				if (_worksheet == null)
					return null;

				int dataAreaTopRowIndex;
				int dataAreaBottomRowIndex;
				this.GetDataAreaRowIndexes(out dataAreaTopRowIndex, out dataAreaBottomRowIndex);

				Debug.Assert(dataAreaTopRowIndex <= dataAreaBottomRowIndex, "The data area row indexes are invalid.");
				return _worksheet.GetCachedRegion(dataAreaTopRowIndex, _wholeTableAddress.FirstColumnIndex, dataAreaBottomRowIndex, _wholeTableAddress.LastColumnIndex);
			}
		}

		#endregion // DataAreaRegion

		#region DisplayBandedColumns

		/// <summary>
		/// Gets or sets the value which indicates whether the alternate column format should be applied to the appropriate columns of the 
		/// <see cref="WorksheetTable"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The column formats are defined by the <see cref="WorksheetTableStyle"/> applied to the WorksheetTable. These are stored in the
		/// <see cref="WorksheetTableStyle.AreaFormats"/> collection and keyed by the <see cref="WorksheetTableStyleArea"/>.ColumnStripe and
		/// WorksheetTableStyleArea.AlternateColumnStripe values. If there is no area format applied for the AlternateColumnStripe value,
		/// this property has no effect on the display of the table.
		/// </p>
		/// <p class="body">
		/// If this value is True and there is an area format for the alternate column stripe, the stripe widths are defined by the 
		/// <see cref="WorksheetTableStyle.ColumnStripeWidth"/> and <see cref="WorksheetTableStyle.AlternateColumnStripeWidth"/> values.
		/// </p>
		/// </remarks>
		/// <seealso cref="Style"/>
		/// <seealso cref="WorksheetTableStyle.AreaFormats"/>
		/// <seealso cref="WorksheetTableStyleArea"/>
		/// <seealso cref="WorksheetTableStyle.ColumnStripeWidth"/>
		/// <seealso cref="WorksheetTableStyle.AlternateColumnStripeWidth"/>
		public bool DisplayBandedColumns
		{
			get { return this.GetFlag(TableFlags.DisplayBandedColumns); }
			set { this.SetFlag(TableFlags.DisplayBandedColumns, value); }
		}

		#endregion // DisplayBandedColumns

		#region DisplayBandedRows

		/// <summary>
		/// Gets or sets the value which indicates whether the alternate row format should be applied to the appropriate rows of the 
		/// <see cref="WorksheetTable"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The row formats are defined by the <see cref="WorksheetTableStyle"/> applied to the WorksheetTable. These are stored in the
		/// <see cref="WorksheetTableStyle.AreaFormats"/> collection and keyed by the <see cref="WorksheetTableStyleArea"/>.RowStripe and
		/// WorksheetTableStyleArea.AlternateRowStripe values. If there is no area format applied for the AlternateRowStripe value,
		/// this property has no effect on the display of the table.
		/// </p>
		/// <p class="body">
		/// If this value is True and there is an area format for the alternate row stripe, the stripe widths are defined by the 
		/// <see cref="WorksheetTableStyle.RowStripeHeight"/> and <see cref="WorksheetTableStyle.AlternateRowStripeHeight"/> values.
		/// </p>
		/// </remarks>
		/// <seealso cref="Style"/>
		/// <seealso cref="WorksheetTableStyle.AreaFormats"/>
		/// <seealso cref="WorksheetTableStyleArea"/>
		/// <seealso cref="WorksheetTableStyle.RowStripeHeight"/>
		/// <seealso cref="WorksheetTableStyle.AlternateRowStripeHeight"/>
		public bool DisplayBandedRows
		{
			get { return this.GetFlag(TableFlags.DisplayBandedRows); }
			set { this.SetFlag(TableFlags.DisplayBandedRows, value); }
		}

		#endregion // DisplayBandedRows

		#region DisplayFirstColumnFormatting

		/// <summary>
		/// Gets or sets the value which indicates whether the first column format should be applied to the appropriate column of the 
		/// <see cref="WorksheetTable"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The first column format is defined by the <see cref="WorksheetTableStyle"/> applied to the WorksheetTable. It is stored in the
		/// <see cref="WorksheetTableStyle.AreaFormats"/> collection and keyed by the <see cref="WorksheetTableStyleArea"/>.FirstColumn 
		/// value. If there is no area format applied for the FirstColumn value, this property has no effect on the display of the table.
		/// </p>
		/// <p class="body">
		/// If there is only one column in the table and both the first and last column formatting should be applied, the last column format
		/// will take precedence.
		/// </p>
		/// </remarks>
		/// <seealso cref="DisplayLastColumnFormatting"/>
		/// <seealso cref="Style"/>
		/// <seealso cref="WorksheetTableStyle.AreaFormats"/>
		/// <seealso cref="WorksheetTableStyleArea"/>
		public bool DisplayFirstColumnFormatting
		{
			get { return this.GetFlag(TableFlags.DisplayFirstColumnFormatting); }
			set { this.SetFlag(TableFlags.DisplayFirstColumnFormatting, value); }
		}

		#endregion // DisplayFirstColumnFormatting

		#region DisplayLastColumnFormatting

		/// <summary>
		/// Gets or sets the value which indicates whether the last column format should be applied to the appropriate column of the 
		/// <see cref="WorksheetTable"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The last column format is defined by the <see cref="WorksheetTableStyle"/> applied to the WorksheetTable. It is stored in the
		/// <see cref="WorksheetTableStyle.AreaFormats"/> collection and keyed by the <see cref="WorksheetTableStyleArea"/>.LastColumn 
		/// value. If there is no area format applied for the LastColumn value, this property has no effect on the display of the table.
		/// </p>
		/// <p class="body">
		/// If there is only one column in the table and both the first and last column formatting should be applied, the last column format
		/// will take precedence.
		/// </p>
		/// </remarks>
		/// <seealso cref="DisplayFirstColumnFormatting"/>
		/// <seealso cref="Style"/>
		/// <seealso cref="WorksheetTableStyle.AreaFormats"/>
		/// <seealso cref="WorksheetTableStyleArea"/>
		public bool DisplayLastColumnFormatting
		{
			get { return this.GetFlag(TableFlags.DisplayLastColumnFormatting); }
			set { this.SetFlag(TableFlags.DisplayLastColumnFormatting, value); }
		}

		#endregion // DisplayLastColumnFormatting

		#region HeaderRowRegion

		/// <summary>
		/// Gets the <see cref="WorksheetRegion"/> which represents the region of cells in the header row of the table.
		/// </summary>
		/// <value>
		/// A WorksheetRegion which represents the region of cells in the header row of the table or null if the header row is not visible.
		/// </value>
		/// <seealso cref="IsHeaderRowVisible"/>
		
		///// <seealso cref="Resize"/>
		public WorksheetRegion HeaderRowRegion
		{
			get
			{
				if (this.Worksheet == null || this.IsHeaderRowVisible == false)
					return null;

				return _worksheet.GetCachedRegion(_wholeTableAddress.FirstRowIndex, _wholeTableAddress.FirstColumnIndex, _wholeTableAddress.FirstRowIndex, _wholeTableAddress.LastColumnIndex);
			}
		}

		#endregion // HeaderRowRegion

		#region IsFilterUIVisible

		/// <summary>
		/// Gets or sets the value indicating whether to allow filtering and show filter buttons in the table headers.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If <see cref="IsHeaderRowVisible"/> is False, this property must be False and setting it to True will cause an error.
		/// If <see cref="IsHeaderRowVisible"/> is set to True, this property will also be set to True automatically.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is True and <see cref="IsHeaderRowVisible"/> is False.
		/// </exception>
		/// <seealso cref="IsHeaderRowVisible"/>
		public bool IsFilterUIVisible
		{
			get { return this.GetFlag(TableFlags.IsFilterUIVisible); }
			set
			{
				if (this.IsFilterUIVisible == value)
					return;

				if (value && this.IsHeaderRowVisible == false)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_ShowFilterUIWhileHeaderRowHidden"));

				this.SetFlag(TableFlags.IsFilterUIVisible, value);

				// MD 3/13/12 - TFS104726
				// If the filter UI is hiding, clear all filters.
				if (value == false)
					this.ClearFilters();
			}
		}

		#endregion // IsFilterUIVisible

		#region IsHeaderRowVisible

		/// <summary>
		/// Gets or sets the value which indicates whether the row containing column headers should be displayed. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When the header row is visible, the cell above each column of data will contain the <see cref="WorksheetTableColumn.Name"/>
		/// value. Therefore, all header cells always contain a string value. Additionally, they will all be unique.
		/// </p>
		/// </remarks>
		/// <value>True if the row containing column headers is visible; False if it is hidden.</value>
		/// <seealso cref="WorksheetTableColumn.Name"/>
		public bool IsHeaderRowVisible
		{
			get { return this.GetFlag(TableFlags.IsHeaderRowVisible); }
			set
			{
				if (this.IsHeaderRowVisible == value)
					return;

				this.OnIsHeaderRowVisibleChanging();
				this.SetFlag(TableFlags.IsHeaderRowVisible, value);
				this.OnIsHeaderRowVisibleChanged();
			}
		}

		#endregion // IsHeaderRowVisible

		#region IsTotalsRowVisible

		/// <summary>
		/// Gets or sets the value which indicates whether the row containing column totals should be displayed. 
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When the totals row is visible, the cell below each column of data will contain either a calculated value, a text value, or nothing.
		/// To display a calculated value in the cell, set the <see cref="WorksheetTableColumn.TotalFormula"/>. To display a text label, set the
		/// <see cref="WorksheetTableColumn.TotalLabel"/>. If both are set, the calculated value takes precedence.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The value is set to True and the table occupies the last row of the worksheet.
		/// </exception>
		/// <value>True if the row containing column totals is visible; False if it is hidden.</value>
		/// <seealso cref="WorksheetTableColumn.TotalFormula"/>
		/// <seealso cref="WorksheetTableColumn.TotalLabel"/>
		public bool IsTotalsRowVisible
		{
			get { return this.GetFlag(TableFlags.IsTotalsRowVisible); }
			set
			{
				if (this.IsTotalsRowVisible == value)
					return;

				if (value)
				{
					bool shouldTotalsRowBeInserted;
					this.OnTotalsRowShowing(out shouldTotalsRowBeInserted);
					this.SetFlag(TableFlags.IsTotalsRowVisible, value);
					this.OnTotalsRowShown(shouldTotalsRowBeInserted);
				}
				else
				{
					this.OnTotalsRowHiding();
					this.SetFlag(TableFlags.IsTotalsRowVisible, value);
					this.OnTotalsRowHidden();
				}
			}
		}

		#endregion // IsTotalsRowVisible

		#region SortSettings

		/// <summary>
		/// Gets the settings which determine how the data within the table should be sorted.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <B>Note:</B> Sort conditions are not constantly evaluated as data within the table changes. Sort conditions are applied to the table 
		/// only when they are are added or removed on a column in the table or when the <see cref="ReapplySortConditions"/> method is called.
		/// </p>
		/// </remarks>
		/// <seealso cref="ReapplySortConditions"/>
		/// <seealso cref="WorksheetTableColumn.SortCondition"/>
		public SortSettings<WorksheetTableColumn> SortSettings
		{
			get
			{
				if (_sortSettings == null)
					_sortSettings = new SortSettings<WorksheetTableColumn>(this);

				return _sortSettings;
			}
		}

		#endregion // SortSettings

		#region Style

		/// <summary>
		/// Gets or sets the style to use on the <see cref="WorksheetTable"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The <see cref="WorksheetTableStyle"/> defines formats to use in various areas of the table. These formats are used as defaults
		/// for cells which don't have their formatting properties already set.
		/// </p>
		/// <p class="body">
		/// The area formats specified in the WorksheetTableStyle are differential formats. In other words, only the properties that are set 
		/// to non-default values will be applied to the appropriate cells. An area format can define only a background color or only font 
		/// information and that format will be applied to the cells while all other formatting properties on the cells will be maintained.
		/// </p>
		/// <p class="body">
		/// If this value is set to null, the Style will be set to the <see cref="Excel.Workbook.DefaultTableStyle"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The value specified is not in the <see cref="Excel.Workbook.CustomTableStyles"/> or <see cref="Excel.Workbook.StandardTableStyles"/> 
		/// collections.
		/// </exception>
		/// <value>The <see cref="WorksheetTableStyle"/> instance which defines the various default table area formats.</value>
		/// <seealso cref="Excel.Workbook.DefaultTableStyle"/>
		/// <seealso cref="Excel.Workbook.CustomTableStyles"/>
		/// <seealso cref="Excel.Workbook.StandardTableStyles"/>
		public WorksheetTableStyle Style
		{
			get { return _style; }
			set
			{
				Workbook workbook = this.Workbook;

				if (value == null)
				{
					if (workbook == null)
						value = StandardTableStyleCollection.Instance.DefaultTableStyle;
					else
						value = workbook.DefaultTableStyle;
				}

				if (_style == value)
					return;

				if (value.IsCustom)
				{
					if (value.CustomCollection == null || (workbook != null && value.CustomCollection.Workbook != workbook))
						throw new ArgumentException(SR.GetString("LE_ArgumentException_TableStyleFromOtherWorkbook"), "value");
				}

				_style = value;
			}
		}

		#endregion // Style

		#region TotalsRowRegion

		/// <summary>
		/// Gets the <see cref="WorksheetRegion"/> which represents the region of cells in the totals row of the table.
		/// </summary>
		/// <value>
		/// A WorksheetRegion which represents the region of cells in the totals row of the table or null if the totals row is not visible.
		/// </value>
		/// <seealso cref="IsTotalsRowVisible"/>
		
		///// <seealso cref="Resize"/>
		public WorksheetRegion TotalsRowRegion
		{
			get
			{
				if (this.Worksheet == null || this.IsTotalsRowVisible == false)
					return null;

				return _worksheet.GetCachedRegion(
					_wholeTableAddress.LastRowIndex, _wholeTableAddress.FirstColumnIndex,
					_wholeTableAddress.LastRowIndex, _wholeTableAddress.LastColumnIndex);
			}
		}

		#endregion // TotalsRowRegion

		#region WholeTableRegion

		/// <summary>
		/// Gets the <see cref="WorksheetRegion"/> which represents the region of cells in the whole table, including the header and totals rows, 
		/// if visible.
		/// </summary>
		
		///// <seealso cref="Resize"/>
		public WorksheetRegion WholeTableRegion
		{
			get
			{
				if (_worksheet == null)
					return null;

				return _worksheet.GetCachedRegion(_wholeTableAddress);
			}
		}

		#endregion // WholeTableRegion

		#region Worksheet

		/// <summary>
		/// Gets the <see cref="Worksheet"/> to which the table belongs.
		/// </summary>
		/// <value>The Worksheet to which the table belongs or null if the table has been removed from the Worksheet.</value>
		/// <seealso cref="Excel.Worksheet.Tables"/>
		public Worksheet Worksheet
		{
			get { return _worksheet; }
		}

		#endregion // Worksheet

		#endregion // Public Properties

		#region Internal Properties

		#region FilterRegion

		internal WorksheetRegion FilterRegion
		{
			get
			{
				if (_worksheet == null)
					return null;

				int dataAreaBottomRowIndex = _wholeTableAddress.LastRowIndex;
				if (this.IsTotalsRowVisible)
					dataAreaBottomRowIndex--;

				Debug.Assert(_wholeTableAddress.FirstRowIndex <= dataAreaBottomRowIndex, "The data area row indexes are invalid.");
				return _worksheet.GetCachedRegion(
					_wholeTableAddress.FirstRowIndex, _wholeTableAddress.FirstColumnIndex, 
					dataAreaBottomRowIndex, _wholeTableAddress.LastColumnIndex);
			}
		}

		#endregion // FilterRegion

		#region HasTotalsRowEverBeenVisible

		internal bool HasTotalsRowEverBeenVisible
		{
			get { return this.GetFlag(TableFlags.HasTotalsRowEverBeenVisible); }
			set { this.SetFlag(TableFlags.HasTotalsRowEverBeenVisible, value); }
		}

		#endregion // HasTotalsRowEverBeenVisible

		#region HeaderRow

		internal WorksheetRow HeaderRow
		{
			get
			{
				if (this.Worksheet == null || this.IsHeaderRowVisible == false)
					return null;

				return _worksheet.Rows[_wholeTableAddress.FirstRowIndex];
			}
		}

		#endregion // HeaderRow

		#region Id

		internal uint Id
		{
			get { return _id; }
		}

		#endregion // Id

		#region IsInsertRowVisible

		internal bool IsInsertRowVisible
		{
			get { return this.GetFlag(TableFlags.IsInsertRowVisible); }
			set
			{
				this.SetFlag(TableFlags.IsInsertRowVisible, value);

				if (value == false)
					this.WereCellsShiftedToShowInsertRow = false;
			}
		}

		#endregion // IsInsertRowVisible

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region IsResizing

		internal bool IsResizing
		{
			get { return this.GetFlag(TableFlags.IsResizing); }
			private set { this.SetFlag(TableFlags.IsResizing, value); }
		}

		#endregion // IsResizing

		#region NextColumnId

		internal uint NextColumnId
		{
			get { return _nextColumnId; }
			set { _nextColumnId = value; }
		}

		#endregion // NextColumnId

		#region Published

		internal bool Published
		{
			get { return this.GetFlag(TableFlags.Published); }
			set { this.SetFlag(TableFlags.Published, value); }
		}

		#endregion // Published

		#region SortAndHeadersRegion

		internal WorksheetRegion SortAndHeadersRegion
		{
			get
			{
				if (_worksheet == null)
					return null;

				int dataAreaBottomRowIndex = _wholeTableAddress.LastRowIndex;
				if (this.IsTotalsRowVisible)
					dataAreaBottomRowIndex--;

				while (dataAreaBottomRowIndex != _wholeTableAddress.FirstRowIndex && _worksheet.IsRowHidden(dataAreaBottomRowIndex))
					dataAreaBottomRowIndex--;

				Debug.Assert(_wholeTableAddress.FirstRowIndex <= dataAreaBottomRowIndex, "The data area row indexes are invalid.");
				return _worksheet.GetCachedRegion(
					_wholeTableAddress.FirstRowIndex, _wholeTableAddress.FirstColumnIndex,
					dataAreaBottomRowIndex, _wholeTableAddress.LastColumnIndex);
			}
		}

		#endregion // SortAndHeadersRegion

		#region TotalsRow

		internal WorksheetRow TotalsRow
		{
			get
			{
				if (this.Worksheet == null || this.IsTotalsRowVisible == false)
					return null;

				return _worksheet.Rows[_wholeTableAddress.LastRowIndex];
			}
		}

		#endregion // TotalsRow

		#region WereCellsShiftedToShowInsertRow

		internal bool WereCellsShiftedToShowInsertRow
		{
			get { return this.GetFlag(TableFlags.WereCellsShiftedToShowInsertRow); }
			set { this.SetFlag(TableFlags.WereCellsShiftedToShowInsertRow, value); }
		}

		#endregion // WereCellsShiftedToShowInsertRow

		#region WholeTableAddress

		internal WorksheetRegionAddress WholeTableAddress
		{
			get { return _wholeTableAddress; }
		}

		#endregion // WholeTableAddress

		#endregion // Internal Properties

		#region Private Properties

		#region PreventAssigningUniqueColumnNames

		private bool PreventAssigningUniqueColumnNames
		{
			get { return this.GetFlag(TableFlags.PreventAssigningUniqueColumnNames); }
			set { this.SetFlag(TableFlags.PreventAssigningUniqueColumnNames, value); }
		}

		#endregion // PreventAssigningUniqueColumnNames

		#region RefilterRequired

		private bool RefilterRequired
		{
			get { return this.GetFlag(TableFlags.RefilterRequired); }
			set { this.SetFlag(TableFlags.RefilterRequired, value); }
		}

		#endregion // RefilterRequired

		#region SuspendAreaFormatSynchronization

		private bool SuspendAreaFormatSynchronization
		{
			get { return this.GetFlag(TableFlags.SuspendAreaFormatSynchronization); }
			set { this.SetFlag(TableFlags.SuspendAreaFormatSynchronization, value); }
		}

		#endregion // SuspendAreaFormatSynchronization

		#endregion // Private Properties

		#endregion // Properties


		#region TableFlags enum

		private enum TableFlags : short
		{
			DisplayBandedColumns = 1 << 0,
			DisplayBandedRows = 1 << 1,
			DisplayFirstColumnFormatting = 1 << 2,
			DisplayLastColumnFormatting = 1 << 3,
			HasTotalsRowEverBeenVisible = 1 << 4,
			IsFilterUIVisible = 1 << 5,
			IsHeaderRowVisible = 1 << 6,
			IsInsertRowVisible = 1 << 7,
			IsResizing = 1 << 8,
			IsTotalsRowVisible = 1 << 9,
			PreventAssigningUniqueColumnNames = 1 << 10,
			Published = 1 << 11,
			RefilterRequired = 1 << 12,
			SuspendAreaFormatSynchronization = 1 << 13,
			WereCellsShiftedToShowInsertRow = 1 << 14,
		}

		#endregion // TableFlags enum
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