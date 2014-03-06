using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization
{
	#region MultipleCellValueInfo abstract class

	// MD 10/19/07
	// Found while fixing BR27421
	// Added support for multiple cell value records
	internal abstract class MultipleCellValueInfo
	{
		// MD 1/10/12 - 12.1 - Cell Format Updates
		// MD 2/1/12 - TFS100573
		// This needs to take the manager now.
		//public delegate bool ShouldIncludeInMultipleCellValueCallback(CellContext cellContext, short columnIndex, out object value);
		public delegate bool ShouldIncludeInMultipleCellValueCallback(WorkbookSerializationManager manager, CellContext cellContext, short columnIndex, out object value);

		#region Member Variables

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private List<WorksheetCell> cells;
		private WorksheetRow row;

		// MD 4/18/11 - TFS62026
		// Store the row cache on this object for performance.
		private WorksheetRowSerializationCache rowCache;

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// We need to store the cell formats of the values as well.
		private List<WorksheetCellFormatData> cellFormats;

		private List<short> columnIndexes;

		private List<object> values;

		#endregion // Member Variables

		#region Constructors

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//protected MultipleCellValueInfo( List<WorksheetCell> cells, List<object> values )
		//{
		//    this.cells = cells;
		//    this.values = values;
		//}
		// MD 1/10/12 - 12.1 - Cell Format Updates
		// We need to store the cell formats of the values as well.
		//protected MultipleCellValueInfo(WorksheetRow row, WorksheetRowSerializationCache rowCache, List<short> columnIndexes, List<object> values)
		protected MultipleCellValueInfo(WorksheetRow row, WorksheetRowSerializationCache rowCache, List<WorksheetCellFormatData> cellFormats, List<short> columnIndexes, List<object> values)
		{
			this.row = row;

			// MD 4/18/11 - TFS62026
			// Store the row cache on this object for performance.
			this.rowCache = rowCache;

			// MD 1/10/12 - 12.1 - Cell Format Updates
			this.cellFormats = cellFormats;

			this.columnIndexes = columnIndexes;
			this.values = values;

			// MD 4/18/11 - TFS62026
			// Verify that we have consecutive column index values.




		}

		#endregion // Constructors

		#region Methods

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public WorksheetCell GetCell( int index )
		//{
		//    return this.cells[ index ];
		//}

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region GetCellFormat(int)

		public WorksheetCellFormatData GetCellFormat(int index)
		{
			return this.cellFormats[index];
		}

		#endregion // GetCellFormat(int)

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region GetCellFormat(CellContext, WorksheetCellFormatData)

		private static WorksheetCellFormatData GetCellFormat(WorksheetRow row, short columnIndex, WorksheetCellFormatData rowFormat)
		{
			WorksheetCellFormatData cellFormat;
			row.TryGetCellFormat(columnIndex, out cellFormat);
			return cellFormat ?? rowFormat;
		}

		#endregion // GetCellFormat(CellContext, WorksheetCellFormatData)

		// MD 1/10/12 - 12.1 - Cell Format Updates
		#region GetMultipleCellInfoHelper

		// MD 2/1/12 - TFS100573
		// This needs to take the manager now.
		//public static bool GetMultipleCellInfoHelper(CellContext cellContext, ShouldIncludeInMultipleCellValueCallback callback,
		public static bool GetMultipleCellInfoHelper(WorkbookSerializationManager manager, CellContext cellContext, ShouldIncludeInMultipleCellValueCallback callback,
			out List<WorksheetCellFormatData> cellFormats,
			out List<short> columnIndexes,
			out List<object> values)
		{
			cellFormats = null;
			columnIndexes = null;
			values = null;

			object value;
			// MD 2/1/12 - TFS100573
			// This needs to take the manager now.
			//if (callback(cellContext, cellContext.ColumnIndex, out value) == false)
			if (callback(manager, cellContext, cellContext.ColumnIndex, out value) == false)
				return false;

			Workbook workbook = cellContext.Row.Worksheet.Workbook;

			WorksheetCellFormatData rowFormat;
			if (cellContext.Row.HasCellFormat)
				rowFormat = cellContext.Row.CellFormatInternal.Element;
			else
				rowFormat = workbook.CellFormats.DefaultElement;

			cellFormats = new List<WorksheetCellFormatData>();
			columnIndexes = new List<short>();
			values = new List<object>();

			cellFormats.Add(MultipleCellValueInfo.GetCellFormat(cellContext.Row, cellContext.ColumnIndex, rowFormat));
			columnIndexes.Add(cellContext.ColumnIndex);
			values.Add(value);

			int maxColumnCount = workbook.MaxColumnCount;
			for (short i = (short)(cellContext.ColumnIndex + 1); i < maxColumnCount; i++)
			{
				// MD 2/1/12 - TFS100573
				// This needs to take the manager now.
				//if (callback(cellContext, i, out value) == false)
				if (callback(manager, cellContext, i, out value) == false)
					break;

				cellFormats.Add(MultipleCellValueInfo.GetCellFormat(cellContext.Row, i, rowFormat));
				columnIndexes.Add(i);
				values.Add(value);
			}

			return columnIndexes.Count >= 2;
		}

		#endregion // GetMultipleCellInfoHelper

		#region GetValue

		public object GetValue(int index)
		{
			return this.values[index];
		}

		#endregion // GetValue

		#endregion // Methods

		#region Properties

		public int FirstColumnIndex
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//get { return this.cells[ 0 ].ColumnIndex; }
			get { return this.columnIndexes[0]; }
		}

		public int LastColumnIndex
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//get { return this.cells[ this.cells.Count - 1 ].ColumnIndex; }
			get { return this.columnIndexes[this.columnIndexes.Count - 1]; }
		}

		public int NumberOfCells
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//get { return this.cells.Count; }
			get { return this.columnIndexes.Count; }
		}

		// MD 4/18/11 - TFS62026
		// We now store the row cache on this object for performance.
		public WorksheetRowSerializationCache RowCache
		{
			get { return this.rowCache; }
		}

		public int RowIndex
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//get { return this.cells[ 0 ].RowIndex; }
			get { return this.row.Index; }
		}

		#endregion // Properties
	}

	#endregion MultipleCellValueInfo abstract class


	#region MultipleCellBlankInfo class

	// MD 10/19/07
	// Found while fixing BR27421
	// Added support for multiple cell value records
	internal class MultipleCellBlankInfo : MultipleCellValueInfo
	{
		// MD 1/10/12 - 12.1 - Cell Format Updates
		private readonly static ShouldIncludeInMultipleCellValueCallback ShouldIncludeValueCallback = new ShouldIncludeInMultipleCellValueCallback(MultipleCellBlankInfo.ShouldIncludeValue);

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private MultipleCellBlankInfo( List<WorksheetCell> cells, List<object> values )
		//    : base( cells, values ) { }
		// MD 1/10/12 - 12.1 - Cell Format Updates
		// We need to store the cell formats of the values as well.
		//private MultipleCellBlankInfo(WorksheetRow row, WorksheetRowSerializationCache rowCache, List<short> columnIndexes, List<object> values)
		//    : base(row, rowCache, columnIndexes, values) { }
		private MultipleCellBlankInfo(WorksheetRow row, WorksheetRowSerializationCache rowCache, List<WorksheetCellFormatData> cellFormats, List<short> columnIndexes, List<object> values)
			: base(row, rowCache, cellFormats, columnIndexes, values) { }

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static MultipleCellBlankInfo GetMultipleCellBlankInfo( WorksheetCell cell, object cellValue )
		// MD 2/1/12 - TFS100573
		// This needs to take the manager now.
		//public static MultipleCellBlankInfo GetMultipleCellBlankInfo(CellContext cellContext)
		public static MultipleCellBlankInfo GetMultipleCellBlankInfo(WorkbookSerializationManager manager, CellContext cellContext)
		{
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Refactored this code into a helper method to reduce code duplication
			#region Old Code

			//// MD 1/10/12 - 12.1 - Cell Format Updates
			//WorksheetCellFormatData rowFormat;
			//if (cellContext.Row.HasCellFormat)
			//    rowFormat = cellContext.Row.CellFormatInternal.Element;
			//else
			//    rowFormat = cellContext.Row.Worksheet.Workbook.CellFormats.DefaultElement;

			//List<WorksheetCellFormatData> cellFormats = new List<WorksheetCellFormatData>();	

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////List<WorksheetCell> cells = new List<WorksheetCell>();
			//List<short> columnIndexes = new List<short>();

			//List<object> values = new List<object>();

			//// MD 1/10/12 - 12.1 - Cell Format Updates
			//WorksheetCellFormatData cellFormat;
			//cellContext.Row.TryGetCellFormat(cellContext.ColumnIndex, out cellFormat);
			//if (cellFormat == null)
			//    cellFormat = rowFormat;
			//cellFormats.Add(cellFormat);

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////cells.Add( cell );
			//columnIndexes.Add(cellContext.ColumnIndex);

			//values.Add(cellContext.Value);

			//// MD 7/26/10 - TFS34398
			//// Since the row is stored directly on the cell, use that.
			////WorksheetRow row = cell.Worksheet.Rows[ cell.RowIndex ];
			//// MD 4/12/11 - TFS67084
			//// The row is passed in now.
			////WorksheetRow row = cell.Row;

			//// MD 6/31/08 - Excel 2007 Format
			////for ( int i = cell.ColumnIndex + 1; i < Workbook.MaxExcelColumnCount; i++ )
			//// MD 7/26/10 - TFS34398
			//// Get the worksheet from the row instead of the cell, because the cell will just get it from the row anyway.
			////int maxColumnCount = cell.Worksheet.Workbook.MaxColumnCount;
			//int maxColumnCount = cellContext.Row.Worksheet.Workbook.MaxColumnCount;

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////for (int i = cell.ColumnIndex + 1; i < maxColumnCount; i++)
			////{
			////    WorksheetCell nextCell = row.Cells.GetIfCreated(i);
			////
			////    if (nextCell == null)
			////        break;
			////
			////    object value = WorkbookSerializationManager.GetSerializableCellValue(nextCell);
			////
			////    if (WorkbookSerializationManager.IsValueBlank(value) == false)
			////    {
			////        break;
			////    }
			////
			////    cells.Add(nextCell);
			////    values.Add(value);
			////}
			////
			////if ( cells.Count < 2 )
			////    return null;
			////
			////return new MultipleCellBlankInfo( cells, values );
			//for (short i = (short)(cellContext.ColumnIndex + 1); i < maxColumnCount; i++)
			//{
			//    // All unallocated cells will have a blank value, so if we hit an unallocated cell or it has no cell 
			//    // format either, we won't write it out, so break the multiple value here.
			//    WorksheetCellBlock cellBlock;
			//    if (cellContext.Row.TryGetCellBlock(i, out cellBlock) == false ||
			//        cellBlock.DoesCellHaveData(cellContext.Row, i) == false)
			//    {
			//        break;
			//    }

			//    object value = WorkbookSerializationManager.GetSerializableCellValue(cellContext.Row, i, cellBlock);
			//    if ( WorkbookSerializationManager.IsValueBlank( value ) == false )
			//    {
			//        break;
			//    }

			//    // MD 1/10/12 - 12.1 - Cell Format Updates
			//    cellContext.Row.TryGetCellFormat(i, out cellFormat);
			//    if (cellFormat == null)
			//        cellFormat = rowFormat;
			//    cellFormats.Add(cellFormat);

			//    columnIndexes.Add(i);
			//    values.Add( value );
			//}

			//if (columnIndexes.Count < 2)
			//    return null;

			//// MD 1/10/12 - 12.1 - Cell Format Updates
			////return new MultipleCellBlankInfo(cellContext.Row, cellContext.RowCache, columnIndexes, values);
			//return new MultipleCellBlankInfo(cellContext.Row, cellContext.RowCache, cellFormats, columnIndexes, values);

			#endregion // Old Code
			List<WorksheetCellFormatData> cellFormats;
			List<short> columnIndexes;
			List<object> values;
			// MD 2/1/12 - TFS100573
			// This needs to take the manager now.
			//if (MultipleCellValueInfo.GetMultipleCellInfoHelper(cellContext, MultipleCellBlankInfo.ShouldIncludeValueCallback, out cellFormats, out columnIndexes, out values) == false)
			if (MultipleCellValueInfo.GetMultipleCellInfoHelper(manager, cellContext, MultipleCellBlankInfo.ShouldIncludeValueCallback, out cellFormats, out columnIndexes, out values) == false)
				return null;

			return new MultipleCellBlankInfo(cellContext.Row, cellContext.RowCache, cellFormats, columnIndexes, values);
		}

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// MD 2/1/12 - TFS100573
		// This needs to take the manager now.
		//private static bool ShouldIncludeValue(CellContext cellContext, short columnIndex, out object value)
		private static bool ShouldIncludeValue(WorkbookSerializationManager manager, CellContext cellContext, short columnIndex, out object value)
		{
			value = null;

			// All unallocated cells will have a blank value, so if we hit an unallocated cell or it has no cell 
			// format either, we won't write it out, so break the multiple value here.
			WorksheetCellBlock cellBlock;
			if (cellContext.Row.TryGetCellBlock(columnIndex, out cellBlock) == false ||
				cellBlock.DoesCellHaveData(cellContext.Row, columnIndex) == false)
			{
				return false;
			}

			// MD 2/1/12 - TFS100573
			// This is now an instance method.
			//value = WorkbookSerializationManager.GetSerializableCellValue(cellContext.Row, columnIndex, cellBlock);
			value = manager.GetSerializableCellValue(cellContext.Row, columnIndex, cellBlock);

			return WorkbookSerializationManager.IsValueBlank(value);
		}
	} 

	#endregion MultipleCellBlankInfo class

	#region MultipleCellRKInfo class

	// MD 10/19/07
	// Found while fixing BR27421
	// Added support for multiple cell value records
	internal class MultipleCellRKInfo : MultipleCellValueInfo
	{
		// MD 1/10/12 - 12.1 - Cell Format Updates
		private readonly static ShouldIncludeInMultipleCellValueCallback ShouldIncludeValueCallback = new ShouldIncludeInMultipleCellValueCallback(MultipleCellRKInfo.ShouldIncludeValue);

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private MultipleCellRKInfo( List<WorksheetCell> cells, List<object> values )
		//    : base( cells, values ) { }
		// MD 1/10/12 - 12.1 - Cell Format Updates
		// We need to store the cell formats of the values as well.
		//private MultipleCellRKInfo(WorksheetRow row, WorksheetRowSerializationCache rowCache, List<short> columnIndexes, List<object> values)
		//    : base(row, rowCache, columnIndexes, values) { }
		private MultipleCellRKInfo(WorksheetRow row, WorksheetRowSerializationCache rowCache, List<WorksheetCellFormatData> cellFormats, List<short> columnIndexes, List<object> values)
			: base(row, rowCache, cellFormats, columnIndexes, values) { }

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static MultipleCellRKInfo GetMultipleCellRKInfo( WorksheetCell cell, uint cellValue )
		// MD 1/10/12 - 12.1 - Cell Format Updates
		// We don't need the cellValue to be passed in anymore.
		//public static MultipleCellRKInfo GetMultipleCellRKInfo(CellContext cellContext, uint cellValue)
		// MD 2/1/12 - TFS100573
		// This needs to take the manager now.
		//public static MultipleCellRKInfo GetMultipleCellRKInfo(CellContext cellContext)
		public static MultipleCellRKInfo GetMultipleCellRKInfo(WorkbookSerializationManager manager, CellContext cellContext)
		{
			// MD 1/10/12 - 12.1 - Cell Format Updates
			// Refactored this code into a helper method to reduce code duplication
			#region Old Code

			//// MD 1/10/12 - 12.1 - Cell Format Updates
			//WorksheetCellFormatData rowFormat;
			//if (cellContext.Row.HasCellFormat)
			//    rowFormat = cellContext.Row.CellFormatInternal.Element;
			//else
			//    rowFormat = cellContext.Row.Worksheet.Workbook.CellFormats.DefaultElement;

			//List<WorksheetCellFormatData> cellFormats = new List<WorksheetCellFormatData>();

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////List<WorksheetCell> cells = new List<WorksheetCell>();
			//List<short> columnIndexes = new List<short>();

			//List<object> values = new List<object>();

			//// MD 1/10/12 - 12.1 - Cell Format Updates
			//WorksheetCellFormatData cellFormat;
			//cellContext.Row.TryGetCellFormat(cellContext.ColumnIndex, out cellFormat);
			//if (cellFormat == null)
			//    cellFormat = rowFormat;
			//cellFormats.Add(cellFormat);

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////cells.Add( cell );
			//columnIndexes.Add(cellContext.ColumnIndex);

			//values.Add(cellValue);

			//// MD 7/26/10 - TFS34398
			//// Since the row is stored directly on the cell, use that.
			////WorksheetRow row = cell.Worksheet.Rows[ cell.RowIndex ];
			//// MD 4/12/11 - TFS67084
			//// The row is already passed in.
			////WorksheetRow row = cell.Row;

			//// MD 6/31/08 - Excel 2007 Format
			////for ( int i = cell.ColumnIndex + 1; i < Workbook.MaxExcelColumnCount; i++ )
			//// MD 7/26/10 - TFS34398
			//// Get the worksheet from the row instead of the cell, because the cell will just get it from the row anyway.
			////int maxColumnCount = cell.Worksheet.Workbook.MaxColumnCount;
			//int maxColumnCount = cellContext.Row.Worksheet.Workbook.MaxColumnCount;

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////for (int i = cell.ColumnIndex + 1; i < maxColumnCount; i++)
			////{
			////
			////    WorksheetCell nextCell = row.Cells.GetIfCreated(i);
			////
			////    if (nextCell == null)
			////        break;
			////
			////    object value = WorkbookSerializationManager.GetSerializableCellValue(nextCell);
			////
			////    uint rkValue;
			////    if (WorkbookSerializationManager.IsRKValue(value, out rkValue) == false)
			////    {
			////        break;
			////    }
			////
			////    cells.Add(nextCell);
			////    values.Add(rkValue);
			////}
			////
			////if (cells.Count < 2)
			////    return null;
			////
			////return new MultipleCellRKInfo(cells, values);
			//for (short i = (short)(cellContext.ColumnIndex + 1); i < maxColumnCount; i++)
			//{
			//    object value = WorkbookSerializationManager.GetSerializableCellValue(cellContext.Row, i);

			//    uint rkValue;
			//    if (WorkbookSerializationManager.IsRKValue(value, out rkValue) == false)
			//    {
			//        break;
			//    }

			//    // MD 1/10/12 - 12.1 - Cell Format Updates
			//    cellContext.Row.TryGetCellFormat(i, out cellFormat);
			//    if (cellFormat == null)
			//        cellFormat = rowFormat;
			//    cellFormats.Add(cellFormat);

			//    columnIndexes.Add(i);
			//    values.Add(rkValue);
			//}

			//if (columnIndexes.Count < 2)
			//    return null;

			//// MD 1/10/12 - 12.1 - Cell Format Updates
			////return new MultipleCellRKInfo(cellContext.Row, cellContext.RowCache, columnIndexes, values);
			//return new MultipleCellRKInfo(cellContext.Row, cellContext.RowCache, cellFormats, columnIndexes, values);

			#endregion // Old Code
			List<WorksheetCellFormatData> cellFormats;
			List<short> columnIndexes;
			List<object> values;
			// MD 2/1/12 - TFS100573
			// This needs to take the manager now.
			//if (MultipleCellValueInfo.GetMultipleCellInfoHelper(cellContext, MultipleCellRKInfo.ShouldIncludeValueCallback, out cellFormats, out columnIndexes, out values) == false)
			if (MultipleCellValueInfo.GetMultipleCellInfoHelper(manager, cellContext, MultipleCellRKInfo.ShouldIncludeValueCallback, out cellFormats, out columnIndexes, out values) == false)
				return null;

			return new MultipleCellRKInfo(cellContext.Row, cellContext.RowCache, cellFormats, columnIndexes, values);
		}

		// MD 1/10/12 - 12.1 - Cell Format Updates
		// MD 2/1/12 - TFS100573
		// This needs to take the manager now.
		//private static bool ShouldIncludeValue(CellContext cellContext, short columnIndex, out object value)
		private static bool ShouldIncludeValue(WorkbookSerializationManager manager, CellContext cellContext, short columnIndex, out object value)
		{
			value = null;

			// MD 2/1/12 - TFS100573
			// This is now an instance method.
			//object cellValue = WorkbookSerializationManager.GetSerializableCellValue(cellContext.Row, columnIndex);
			object cellValue = manager.GetSerializableCellValue(cellContext.Row, columnIndex);

			uint rkValue;
			if (WorkbookSerializationManager.IsRKValue(cellValue, out rkValue) == false)
				return false;

			value = rkValue;
			return true;
		}
	} 

	#endregion MultipleCellRKInfo class
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