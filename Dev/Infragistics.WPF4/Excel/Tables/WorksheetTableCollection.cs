using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 12/7/11 - 12.1 - Table Support



	/// <summary>
	/// The collection of <see cref="WorksheetTable"/> instances on a <see cref="Worksheet"/>.
	/// </summary>
	/// <seealso cref="Worksheet.Tables"/>
	[DebuggerDisplay("WorksheetTableCollection: Count - {Count}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class WorksheetTableCollection :
		IList<WorksheetTable>
	{
		#region Member Variables

		private List<WorksheetTable> _tables;
		private Dictionary<uint, WorksheetTable> _tablesById;
		private Worksheet _worksheet;

		#endregion // Member Variables

		#region Constructor

		internal WorksheetTableCollection(Worksheet worksheet)
		{
			_tables = new List<WorksheetTable>();
			_tablesById = new Dictionary<uint, WorksheetTable>();
			_worksheet = worksheet;
		}

		#endregion Constructor

		#region Interfaces

		#region IList<WorksheetTable> Members

		void IList<WorksheetTable>.Insert(int index, WorksheetTable item)
		{
			this.ThrowOnDirectInsert();
		}

		WorksheetTable IList<WorksheetTable>.this[int index]
		{
			get
			{
				return _tables[index];
			}
			set
			{
				this.ThrowOnDirectInsert();
			}
		}

		#endregion

		#region ICollection<WorksheetTable> Members

		void ICollection<WorksheetTable>.Add(WorksheetTable item)
		{
			this.ThrowOnDirectInsert();
		}

		void ICollection<WorksheetTable>.CopyTo(WorksheetTable[] array, int arrayIndex)
		{
			_tables.CopyTo(array, arrayIndex);
		}

		bool ICollection<WorksheetTable>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<WorksheetTable> Members

		IEnumerator<WorksheetTable> IEnumerable<WorksheetTable>.GetEnumerator()
		{
			return _tables.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _tables.GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		
		#region Add

		/// <summary>
		/// Formats a region as a table and adds an associated <see cref="WorksheetTable"/> to the collection.
		/// </summary>
		/// <param name="region">The region to format as a table.</param>
		/// <param name="tableHasHeaders">
		/// A value which indicates whether the top row of the region contains the headers for the table.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// When the table is created, the <see cref="Workbook.DefaultTableStyle"/> will be applied to the <seealso cref="WorksheetTable.Style"/> 
		/// value.
		/// </p>
		/// <p class="body">
		/// When the table is created, the column names will be taken from the cells in the header row if <paramref name="tableHasHeaders"/> 
		/// is True. If it is False, the column names will be generated and the cells for the header row will be inserted into the worksheet.
		/// </p>
		/// <p class="body">
		/// The column names are unique within the owning WorksheetTable. If, when the table is created, there are two or more columns with 
		/// the same name, the second and subsequent duplicate column names will have a number appended to make them unique. If any cells in 
		/// the header row have a non-string value, their value will be changed to a string (the current display text of the cell). If any 
		/// cells in the header row have no value, they will be given a generated column name.
		/// </p>
		/// <p class="body">
		/// If the region partially contains any merged cell regions, they will be removed from the worksheet and the table region will be expanded 
		/// to include all cells from the merged region.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The owning worksheet has been removed from its workbook.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="region"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="region"/> is not a valid region address in the workbook's cell reference mode.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells from another <see cref="WorksheetTable"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells which have a multi-cell <see cref="ArrayFormula"/> applied.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells which are part of a <see cref="WorksheetDataTable"/>.
		/// </exception>
		/// <returns>The <see cref="WorksheetTable"/> created the represent the formatted table for the region.</returns>
		/// <seealso cref="WorksheetTable"/>
		/// <seealso cref="Excel.Worksheet.Tables"/>
		/// <seealso cref="WorksheetTableColumn.Name"/>
		/// <seealso cref="WorksheetTable.IsHeaderRowVisible"/>
		/// <seealso cref="WorksheetRegion.FormatAsTable(bool)"/>
		public WorksheetTable Add(string region, bool tableHasHeaders)
		{
			return this.Add(region, tableHasHeaders, null);
		}

		/// <summary>
		/// Formats a region as a table and adds an associated <see cref="WorksheetTable"/> to the collection.
		/// </summary>
		/// <param name="region">The region to format as a table.</param>
		/// <param name="tableHasHeaders">
		/// A value which indicates whether the top row of the region contains the headers for the table.
		/// </param>
		/// <param name="tableStyle">
		/// The <see cref="WorksheetTableStyle"/> to apply to the table or null to use the <see cref="Workbook.DefaultTableStyle"/>.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// When the table is created, the specified <paramref name="tableStyle"/> will be applied to the <seealso cref="WorksheetTable.Style"/> 
		/// value.
		/// </p>
		/// <p class="body">
		/// When the table is created, the column names will be taken from the cells in the header row if <paramref name="tableHasHeaders"/> 
		/// is True. If it is False, the column names will be generated and the cells for the header row will be inserted into the worksheet.
		/// </p>
		/// <p class="body">
		/// The column names are unique within the owning WorksheetTable. If, when the table is created, there are two or more columns with 
		/// the same name, the second and subsequent duplicate column names will have a number appended to make them unique. If any cells in 
		/// the header row have a non-string value, their value will be changed to a string (the current display text of the cell). If any 
		/// cells in the header row have no value, they will be given a generated column name.
		/// </p>
		/// <p class="body">
		/// If the region partially contains any merged cell regions, they will be removed from the worksheet and the table region will be expanded 
		/// to include all cells from the merged region.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The owning worksheet has been removed from its workbook.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="region"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="region"/> is not a valid region address in the workbook's cell reference mode.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The specified <paramref name="tableStyle"/> does not exist in the <see cref="Workbook.CustomTableStyles"/> or 
		/// <see cref="Workbook.StandardTableStyles"/> collections.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells from another <see cref="WorksheetTable"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells which have a multi-cell <see cref="ArrayFormula"/> applied.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells which are part of a <see cref="WorksheetDataTable"/>.
		/// </exception>
		/// <returns>The <see cref="WorksheetTable"/> created the represent the formatted table for the region.</returns>
		/// <seealso cref="WorksheetTable"/>
		/// <seealso cref="Excel.Worksheet.Tables"/>
		/// <seealso cref="WorksheetTableColumn.Name"/>
		/// <seealso cref="Workbook.CustomTableStyles"/>
		/// <seealso cref="Workbook.StandardTableStyles"/>
		/// <seealso cref="WorksheetTable.Style"/>
		/// <seealso cref="WorksheetTable.IsHeaderRowVisible"/>
		/// <seealso cref="WorksheetRegion.FormatAsTable(bool,WorksheetTableStyle)"/>
		public WorksheetTable Add(string region, bool tableHasHeaders, WorksheetTableStyle tableStyle)
		{
			return this.Add(_worksheet.GetRegion(region), tableHasHeaders, tableStyle);
		}

		internal WorksheetTable Add(WorksheetRegion region, bool tableHasHeaders, WorksheetTableStyle tableStyle)
		{
			Workbook workbook = _worksheet.Workbook;
			if (workbook == null)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotAddTableToRemovedWorksheet"));

			if (tableStyle == null)
			{
				tableStyle = workbook.DefaultTableStyle;
			}
			else if (tableStyle.IsCustom &&
				workbook.CustomTableStyles.Contains(tableStyle) == false)
			{
				throw new ArgumentException(SR.GetString("LE_ArgumentException_TableStyleFromOtherWorkbook"));
			}

			
			List<WorksheetMergedCellsRegion> mergedRegionsToRemove = null;
			foreach (WorksheetCell cell in region)
			{
				WorksheetMergedCellsRegion mergedRegion = cell.AssociatedMergedCellsRegion;
				if (mergedRegion == null)
					continue;

				if (mergedRegionsToRemove == null)
					mergedRegionsToRemove = new List<WorksheetMergedCellsRegion>();

				if (mergedRegionsToRemove.Contains(mergedRegion) == false)
				{
					mergedRegionsToRemove.Add(mergedRegion);
					region = WorksheetRegion.Union(region, mergedRegion);
				}
			}

			
			
			foreach (WorksheetCell cell in region)
			{
				if (cell.AssociatedTable != null)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_OverlappingTable"));

				IWorksheetRegionBlockingValue blockingValue = cell.Value as IWorksheetRegionBlockingValue;
				if (blockingValue != null)
					blockingValue.ThrowBlockingException();
			}

			if (mergedRegionsToRemove != null)
			{
				for (int i = 0; i < mergedRegionsToRemove.Count; i++)
					_worksheet.MergedCellsRegions.Remove(mergedRegionsToRemove[i]);
			}

			string name;
			string nameBase = SR.GetString("GenerateTableName");

			int tableIndex = 1;
			while (true)
			{
				name = nameBase + tableIndex++;
				if (workbook.GetWorkbookScopedNamedItem(name) == null)
					break;
			}

			bool isInsertRowVisible = false;
			bool wereCellsShiftedToShowInsertRow = false;

			// Cache the region values values and null out the region because it may shift when we insert cells below, 
			// so we shouldn't use it again in this method.
			int firstRowIndex = region.FirstRow;
			int lastRowIndex = region.LastRow;
			short firstColumnIndex = region.FirstColumnInternal;
			short lastColumnIndex = region.LastColumnInternal;
			region = null;

			if (tableHasHeaders == false)
			{
				// If a table has no headers, we move the data down one row and use the top row for headers.
				// However, if any cells directly below the table have data, the header cells need to be inserted.

				string headerRowDescription = SR.GetString("TableHeaderRowDescription");
				if (lastRowIndex == workbook.MaxRowCount - 1)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotInsertTableRow_TableOnBottomOfWorksheet", headerRowDescription));

				CellShiftResult shiftResult;
				if (_worksheet.AreCellsNonTrivial(lastRowIndex + 1, firstColumnIndex, lastColumnIndex))
				{
					shiftResult = _worksheet.InsertCellsAndShiftDown(
						firstRowIndex, firstColumnIndex, lastColumnIndex,
						CellShiftInitializeFormatType.FromShiftedCellsAdjacentToInsertRegion);
				}
				else
				{
					shiftResult = _worksheet.ShiftCellsVertically(
						firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex, 1,
						CellShiftInitializeFormatType.FromShiftedCellsAdjacentToInsertRegion);
				}

				WorksheetTable.ProcessCellShiftResult(shiftResult, headerRowDescription);

				lastRowIndex++;
				tableHasHeaders = true;
			}
			else if (firstRowIndex == lastRowIndex)
			{
				// If table only has a header row and no data, add an "insert row" so the user can enter data for the first row of the table.

				string insertRowDescription = SR.GetString("TableInsertRowDescription");

				if (lastRowIndex == workbook.MaxRowCount - 1)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotInsertTableRow_TableOnBottomOfWorksheet", insertRowDescription));

				// If there is data in the place where the insert row needs to go, the cells for the insert row need to be inserted.
				if (_worksheet.AreCellsNonTrivial(lastRowIndex + 1, firstColumnIndex, lastColumnIndex))
				{
					CellShiftResult shiftResult = _worksheet.InsertCellsAndShiftDown(
						lastRowIndex + 1, firstColumnIndex, lastColumnIndex,
						CellShiftInitializeFormatType.FromStationaryCellsAdjacentToInsertRegion);

					WorksheetTable.ProcessCellShiftResult(shiftResult, insertRowDescription);
					wereCellsShiftedToShowInsertRow = true;
				}

				lastRowIndex++;
				isInsertRowVisible = true;
			}

			WorksheetTable table = new WorksheetTable(name, workbook.GetNewTableId(), firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);

			table.Style = tableStyle;
			table.IsHeaderRowVisible = tableHasHeaders;
			table.IsInsertRowVisible = isInsertRowVisible;
			table.WereCellsShiftedToShowInsertRow = wereCellsShiftedToShowInsertRow;

			int columnCount = lastColumnIndex - firstColumnIndex + 1;
			for (int i = 0; i < columnCount; i++)
				table.InsertColumn();

			this.InternalAdd(table);

			table.InitializeAreaFormats();
			table.InitializeColumnFormulas();

			return table;
		}

		#endregion Add

		#region Clear

		/// <summary>
		/// Clears the collection and removes all tables from the worksheet.
		/// </summary>
		public void Clear()
		{
			for (int i = _tables.Count - 1; i >= 0; i--)
				this.RemoveAt(i);
		}

		#endregion Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified <see cref="WorksheetTable"/> is in the collection.
		/// </summary>
		/// <param name="table">The WorksheetTable to find in the collection.</param>
		/// <returns>True if the WorksheetTable is in the collection; False otherwise.</returns>
		public bool Contains(WorksheetTable table)
		{
			return _tables.Contains(table);
		}

		#endregion Contains

		#region Exists

		/// <summary>
		/// Determines whether a <see cref="WorksheetTable"/> with the specified name is in the collection.
		/// </summary>
		/// <param name="name">The name of the WorksheetTable to find.</param>
		/// <remarks>
		/// <p class="body">
		/// Table names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <returns>True if a WorksheetTable with the specified name is in the collection; False otherwise.</returns>
		public bool Exists(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;

			// MD 4/9/12 - TFS101506
			CultureInfo culture = _worksheet.Culture;

			foreach (WorksheetTable table in _tables)
			{
				// MD 4/9/12 - TFS101506
				//if (String.Compare(table.Name, name, StringComparison.CurrentCultureIgnoreCase) == 0)
				if (String.Compare(table.Name, name, culture, CompareOptions.IgnoreCase) == 0)
					return true;
			}

			return false;
		}

		#endregion Exists

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified <see cref="WorksheetTable"/> in the collection.
		/// </summary>
		/// <param name="table">The WorksheetTable to find in the collection.</param>
		/// <returns>
		/// The 0-based index of the specified WorksheetTable in the collection or -1 if the item is not in the collection.
		/// </returns>
		public int IndexOf(WorksheetTable table)
		{
			return _tables.IndexOf(table);
		}

		#endregion IndexOf

		#region Remove

		/// <summary>
		/// Removes the <see cref="WorksheetTable"/> from the collection.
		/// </summary>
		/// <param name="table">The WorksheetTable to remove from the collection.</param>
		/// <returns>True if the WorksheetTable was found and removed; False otherwise.</returns>
		public bool Remove(WorksheetTable table)
		{
			int index = _tables.IndexOf(table);

			if (index < 0)
				return false;

			this.RemoveAt(index);
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the <see cref="WorksheetTable"/> at the specified index.
		/// </summary>
		/// <param name="index">The 0-based index of the WorksheetTable to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt(int index)
		{
			WorksheetTable table = _tables[index];
			_tables.RemoveAt(index);
			_tablesById.Remove(table.Id);

			table.OnRemovedFromWorksheet();
		}

		#endregion RemoveAt

		#endregion // Public Methods

		#region Internal Methods

		#region GetTableById

		internal WorksheetTable GetTableById(uint id)
		{
			WorksheetTable table;
			_tablesById.TryGetValue(id, out table);
			return table;
		}

		#endregion // GetTableById

		#region InternalAdd

		internal void InternalAdd(WorksheetTable table)
		{
			Workbook workbook = _worksheet.Workbook;
			if (workbook != null)
			{
				workbook.VerifyItemName(table.Name, table);
				workbook.NextTableId = Math.Max(workbook.NextTableId, table.Id + 1);
			}

			_tables.Add(table);

			Debug.Assert(_tablesById.ContainsKey(table.Id) == false, "There should not be two tables with the same id.");
			_tablesById[table.Id] = table;

			table.OnRooted(_worksheet);
		}

		#endregion //InternalAdd

		#endregion // Internal Methods

		#region ThrowOnDirectInsert

		private void ThrowOnDirectInsert()
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CannotAddTableDirectly"));
		}

		#endregion // ThrowOnDirectInsert

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of tables in the collection.
		/// </summary>
		/// <value>The number of tables in the collection.</value>
		public int Count
		{
			get { return _tables.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the <see cref="WorksheetTable"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the WorksheetTable to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The WorksheetTable at the specified index.</value>
		public WorksheetTable this[int index]
		{
			get
			{
				if (index < 0 || this.Count <= index)
					throw new ArgumentOutOfRangeException("index", SR.GetString("LE_ArgumentOutOfRangeException_CollectionIndex"));

				return _tables[index];
			}
		}

		#endregion Indexer [ int ]

		#region Indexer [ string ]

		/// <summary>
		/// Gets the <see cref="WorksheetTable"/> with the specified name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Worksheet names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <param name="name">The name of the WorksheetTable to get.</param>
		/// <exception cref="InvalidOperationException">
		/// A WorksheetTable with the specified name does not exist in the collection. 
		/// </exception>
		/// <value>The WorksheetTable with the specified name.</value>
		/// <seealso cref="NamedReferenceBase.Name"/>
		public WorksheetTable this[string name]
		{
			get
			{
				// MD 4/9/12 - TFS101506
				CultureInfo culture = _worksheet.Culture;

				foreach (WorksheetTable table in _tables)
				{
					// MD 4/9/12 - TFS101506
					//if (String.Compare(table.Name, name, StringComparison.CurrentCultureIgnoreCase) == 0)
					if (String.Compare(table.Name, name, culture, CompareOptions.IgnoreCase) == 0)
						return table;
				}

				throw new InvalidOperationException(SR.GetString("LER_Exception_KeyNotFound"));
			}
		}

		#endregion Indexer [ string ]

		#endregion Public Properties

		#endregion Properties
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