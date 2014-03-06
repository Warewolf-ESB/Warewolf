using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using System.Collections.ObjectModel;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	internal static class CalcUtilities
	{
		// MD 2/24/12 - 12.1 - Table Support
		#region ContainsReferenceHelper

		public static bool ContainsReferenceHelper(WorksheetRegion referenceRegion, IExcelCalcReference inReference)
		{
			if (referenceRegion == null)
				return false;

			CellCalcReference otherCellReference = inReference.Context as CellCalcReference;
			if (otherCellReference != null)
			{
				WorksheetRow otherCellRow = otherCellReference.Row;
				short otherCellColumn = otherCellReference.ColumnIndex;
				return referenceRegion.Contains(otherCellRow, otherCellColumn);
			}

			WorksheetRegion otherRegion = inReference.Context as WorksheetRegion;
			if (otherRegion != null)
				return referenceRegion.IntersectsWith(otherRegion);

			List<WorksheetRegion> otherRegionGroup = inReference.Context as List<WorksheetRegion>;
			if (otherRegionGroup != null)
			{
				foreach (WorksheetRegion otherRegion2 in otherRegionGroup)
				{
					if (referenceRegion.IntersectsWith(otherRegion2))
						return true;
				}

				return false;
			}

			return false;
		}

		#endregion // ContainsReferenceHelper

		#region CreateExcelCalcValue

		public static ExcelCalcValue CreateExcelCalcValue( object value )
		{
			ErrorValue error = value as ErrorValue;

			if ( error != null )
			{
				if ( error == ErrorValue.Circularity )
					return new ExcelCalcValue( 0 );

				return new ExcelCalcValue( error.ToCalcErrorValue() );
			}

			if ( value is FormattedString ||
				value is StringBuilder ||
				value is char ||
				// MD 4/12/11 - TFS67084
				// Removed the FormattedStringProxy class. The FormattedStringElement now represents regular strings.
				//// MD 11/3/10 - TFS49093 - These now represent regular strings
				//value is FormattedStringProxy ||
				value is StringElement ||
				value is FormattedStringValueReference )
			{
				value = value.ToString();
			}

			return new ExcelCalcValue( value );
		} 

		#endregion CreateExcelCalcValue

		#region GetCalcErrorCode

		public static ExcelCalcErrorCode GetCalcErrorCode( int excelErrorCode )
		{
			switch ( excelErrorCode )
			{
				case ErrorValue.ArgumentOrFunctionNotAvailableValue:
					return ExcelCalcErrorCode.NA;

				case ErrorValue.DivisionByZeroValue:
					return ExcelCalcErrorCode.Div;

				case ErrorValue.EmptyCellRangeIntersectionValue:
					return ExcelCalcErrorCode.Null;

				case ErrorValue.InvalidCellReferenceValue:
					return ExcelCalcErrorCode.Reference;

				case ErrorValue.ValueRangeOverflowValue:
					return ExcelCalcErrorCode.Num;

				case ErrorValue.WrongFunctionNameValue:
					return ExcelCalcErrorCode.Name;

				case ErrorValue.WrongOperandTypeValue:
					return ExcelCalcErrorCode.Value;

				default:
					Utilities.DebugFail( "Unknown excel error code: " + excelErrorCode );
					goto case ErrorValue.ValueRangeOverflowValue;
			}
		} 

		#endregion GetCalcErrorCode

		#region GetExpectedFormulaResultTokenClass

		internal static TokenClass GetExpectedFormulaResultTokenClass( IExcelCalcReference reference )
		{
			if ( reference == null )
			{
				Utilities.DebugFail( "The reference should not be null." );
				return TokenClass.Value;
			}

			reference = ExcelCalcEngine.GetResolvedReference( reference );

			if ( reference is CellCalcReference )
				return TokenClass.Value;

			// MD 3/30/11 - TFS69969
			// Both NamedCalcReference and ExternalNamedCalcReference have reference results, so check for their common base.
			//if ( reference is NamedCalcReference )
			if (reference is NamedCalcReferenceBase)
				return TokenClass.Reference;

			Utilities.DebugFail( "Unknown formula owner." );
			return TokenClass.Value;
		} 

		#endregion GetExpectedFormulaResultTokenClass

		#region GetReference

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static IExcelCalcReference GetReference( string name, WorksheetCell originCell, Worksheet worksheet, Workbook workbook )
		//{
		//    return CalcUtilities.GetReference( name, originCell, worksheet, workbook, CellReferenceMode.A1 );
		//}
		public static IExcelCalcReference GetReference(string name, WorksheetRow originCellRow, short originCellColumnIndex, Worksheet worksheet, Workbook workbook)
		{
			return CalcUtilities.GetReference(name, originCellRow, originCellColumnIndex, worksheet, workbook, CellReferenceMode.A1);
		}

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static IExcelCalcReference GetReference( string name, WorksheetCell originCell, Worksheet worksheet, Workbook workbook, CellReferenceMode cellReferenceMode )
		public static IExcelCalcReference GetReference(string name, WorksheetRow originCellRow, short originCellColumnIndex, Worksheet worksheet, Workbook workbook, CellReferenceMode cellReferenceMode)
		{
			// MD 2/24/12 - 12.1 - Table Support
			// The workbook can now parse the reference and it handles more cases than the old code.
			#region Old Code

			//if ( workbook == null )
			//    return ExcelReferenceError.Instance;

			//WorkbookFormat format = workbook.CurrentFormat;

			//int addressStart = 0;

			//int worksheetSeparatorIndex = name.IndexOf( FormulaParser.WorksheetNameSeparator );
			//string worksheetName = null;

			//if ( worksheetSeparatorIndex >= 0 )
			//{
			//    addressStart += worksheetSeparatorIndex + 1;
			//    worksheetName = name.Substring( 0, worksheetSeparatorIndex );

			//    // If the worksheet name is quoted, remove the quotes
			//    if ( worksheetName.Length >= 2 &&
			//        worksheetName[ 0 ] == '\'' &&
			//        worksheetName[ worksheetName.Length - 1 ] == '\'' )
			//    {
			//        worksheetName = worksheetName.Substring( 1, worksheetName.Length - 2 );
			//    }
			//    else if ( FormulaParser.ShouldWorksheetNameBeQuoted( worksheetName ) )
			//    {
			//        return ExcelReferenceError.Instance;
			//    }

			//    if ( workbook.Worksheets.Exists( worksheetName ) == false )
			//        return ExcelReferenceError.Instance;

			//    worksheet = workbook.Worksheets[ worksheetName ];
			//}

			//string address = name.Substring( addressStart );

			//if ( FormulaParser.IsValidNamedReference( address, format ) )
			//{
			//    // MD 2/24/12 - 12.1 - Table Support
			//    //NamedReference namedReference = null;
			//    NamedReferenceBase namedReference = null;

			//    // If the worksheet name scope was specified, only check with the worksheet scope
			//    if ( worksheetName != null )
			//    {
			//        // If the worksheet name did not point to a valid worksheet, don't set the namedReference so we return 
			//        // a Name error later.
			//        if ( worksheet != null )
			//            namedReference = workbook.NamedReferences.Find( address, worksheet );
			//    }
			//    else
			//    {
			//        // If no worksheet name was specified, check for a name with the workbook scope first
			//        //namedReference = workbook.NamedReferences.Find( address );
			//        // MD 2/24/12 - 12.1 - Table Support
			//        namedReference = workbook.GetWorkbookScopedNamedItem(address);

			//        // If there is no global named reference with that name, try using the named reference scoped to the worksheet
			//        // containing the current reference.
			//        if ( namedReference == null && worksheet != null )
			//            namedReference = workbook.NamedReferences.Find( address, worksheet );
			//    }

			//    if ( namedReference == null )
			//        return new UltraCalcReferenceError( ExcelCalcErrorCode.Name );

			//    return namedReference.CalcReference;
			//}

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////WorksheetRegion region;
			////WorksheetCell cell;
			////Utilities.ParseRegionAddress( address, worksheet, cellReferenceMode, originCell, out region, out cell );
			////
			////if ( region != null )
			////    return region.CalcReference;
			////
			////if ( cell != null )
			////    return cell.CalcReference;
			//int firstRowIndex;
			//short firstColumnIndex;
			//int lastRowIndex;
			//short lastColumnIndex;
			//// MD 5/13/11 - Data Validations / Page Breaks
			////Utilities.ParseRegionAddress(address, worksheet, cellReferenceMode, originCellRow, originCellColumnIndex, 
			//Utilities.ParseRegionAddress(address, worksheet.CurrentFormat, cellReferenceMode, originCellRow, originCellColumnIndex, 
			//    out firstRowIndex, out firstColumnIndex, out lastRowIndex, out lastColumnIndex);

			//if (0 <= lastRowIndex)
			//    return worksheet.GetCachedRegion(firstRowIndex, firstColumnIndex, lastRowIndex, lastColumnIndex).CalcReference;

			//if (0 <= firstRowIndex)
			//    return worksheet.Rows[firstRowIndex].GetCellCalcReference(firstColumnIndex);

			//return ExcelReferenceError.Instance;

			#endregion // Old Code
			if (workbook != null)
			{
				bool isNamedReference;
				ExcelRefBase parsedReference = workbook.ParseReference(name.TrimEnd(), cellReferenceMode, worksheet, originCellRow, originCellColumnIndex, out isNamedReference);
				if (parsedReference != null)
					return parsedReference;
			}

			return ExcelReferenceError.Instance;
		} 

		#endregion GetReference

		#region GetReferenceValuesForRegion

		public static ExcelCalcValue[ , ] GetReferenceValuesForRegion( WorksheetRegion region )
		{
			// MD 2/24/12 - 12.1 - Table Support
			if (region == null)
				return new ExcelCalcValue[0, 0];

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if(region.Worksheet == null)
				return new ExcelCalcValue[,] { { new ExcelCalcValue(ErrorValue.InvalidCellReference.ToCalcErrorValue()) } };

			ExcelCalcValue[ , ] references = new ExcelCalcValue[ region.Width, region.Height ];

			for ( int rowIndex = region.FirstRow, rowIndexInRegion = 0; rowIndex <= region.LastRow; rowIndex++, rowIndexInRegion++ )
			{
				WorksheetRow row = region.Worksheet.Rows[ rowIndex ];

				// MD 4/12/11 - TFS67084
				// Use short instead of int so we don't have to cast.
				//for ( int columnIndex = region.FirstColumn, columnIndexInRegion = 0; columnIndex <= region.LastColumn; columnIndex++, columnIndexInRegion++ )
				for (short columnIndex = region.FirstColumnInternal, columnIndexInRegion = 0; 
					columnIndex <= region.LastColumnInternal; 
					columnIndex++, columnIndexInRegion++)
				{
					references[ columnIndexInRegion, rowIndexInRegion ] =
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//CalcUtilities.CreateExcelCalcValue( row.Cells[ columnIndex ].CalcReference );
						CalcUtilities.CreateExcelCalcValue(row.GetCellCalcReference(columnIndex));
				}
			}

			return references;
		} 

		#endregion GetReferenceValuesForRegion

		#region Removed

		#region GetRegionGroup

		// MD 2/24/12 - 12.1 - Table Support
		// Removed this because it is no longer a good way to do things now that tables are supported.
		// Now there is a GetRegionGroup method on ExcelRefBase.
		#region Removed

		//public static WorksheetRegion[] GetRegionGroup( object item )
		//{
		//    // MD 4/12/11 - TFS67084
		//    // Moved away from using WorksheetCell objects.
		//    //WorksheetCell cell = item as WorksheetCell;
		//    //
		//    //if ( cell != null )
		//    //    return new WorksheetRegion[] { cell.CachedRegion };
		//    CellCalcReference cellReference = item as CellCalcReference;

		//    if (cellReference != null)
		//    {
		//        WorksheetRow row = cellReference.Row;
		//        short columnIndex = cellReference.ColumnIndex;
		//        return new WorksheetRegion[] { row.Worksheet.GetCachedRegion(row.Index, columnIndex, row.Index, columnIndex) };
		//    }

		//    WorksheetRegion region = item as WorksheetRegion;

		//    if ( region != null )
		//        return new WorksheetRegion[] { region };

		//    // MD 12/1/11 - TFS96113
		//    // The RegionGroupCalcReference now has a context of type ReadOnlyCollection<WorksheetRegion> to prevent us from accidentally
		//    // modifying the collection.
		//    //List<WorksheetRegion> regionGroup = item as List<WorksheetRegion>;
		//    ReadOnlyCollection<WorksheetRegion> regionGroup = item as ReadOnlyCollection<WorksheetRegion>;

		//    if ( regionGroup != null )
		//    {
		//        // MD 12/1/11 - TFS96113
		//        //return regionGroup.ToArray();
		//        WorksheetRegion[] regions = new WorksheetRegion[regionGroup.Count];
		//        regionGroup.CopyTo(regions, 0);
		//        return regions;
		//    }

		//    Debug.Assert( item is NamedReference, "Unknown item type" );
		//    return new WorksheetRegion[ 0 ];
		//} 

		#endregion // Removed
		public static IList<WorksheetRegion> GetRegionGroup(IExcelCalcReference reference)
		{
			ExcelRefBase resolvedReference = ExcelCalcEngine.GetResolvedReference(reference) as ExcelRefBase;
			if (resolvedReference == null)
				return null;

			return resolvedReference.GetRegionGroup();
		}

		#endregion GetRegionGroup

		#endregion // Removed

		#region GetSingleReference

		internal static CellCalcReference GetSingleReference( CellCalcReference formulaOwner, WorksheetRegion region, out ExcelCalcErrorValue errorValue )
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (region.Worksheet == null)
			{
				errorValue = new ExcelCalcErrorValue(ExcelCalcErrorCode.Reference);
				return null;
			}

			errorValue = null;
			int relativeColumn;
			int relativeRow;
			if ( CalcUtilities.SplitArrayForArrayFormula( formulaOwner, region.Width, region.Height, out relativeColumn, out relativeRow ) == false )
			{
				errorValue = new ExcelCalcErrorValue( ExcelCalcErrorCode.NA );
				return null;
			}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//return region.GetCell( relativeColumn, relativeRow ).CalcReference;
			int absoluteRowIndex = relativeRow + region.FirstRow;
			short absoluteColumnIndex = (short)(relativeColumn + region.FirstColumnInternal);
			return region.Worksheet.Rows[absoluteRowIndex].GetCellCalcReference(absoluteColumnIndex);
		}

		#endregion GetSingleReference 

		#region GetValueFormExcelCalcValue

		public static object GetValueFormExcelCalcValue( object resolvedValue )
		{
			// If a formula evaluates to null, a zero is shown in the cell
			if ( resolvedValue == null || resolvedValue is DBNull )
			{
				Utilities.DebugFail( "This should have been converted to 0 already." );
				return 0d;
			}

			// MD 2/28/12 - 12.1 - Table Support
			if (resolvedValue is ErrorValue)
				return resolvedValue;

			if ( resolvedValue is string ||
				resolvedValue is bool ||
				resolvedValue is DateTime )
			{
				return resolvedValue;
			}

			ExcelCalcErrorValue errorValue = resolvedValue as ExcelCalcErrorValue;

			if ( errorValue == null )
			{
				Debug.Assert((resolvedValue is ExcelCalcFormula) == false, "A formula should never be used as a cell value.");

				try
				{
					double value = Convert.ToDouble( resolvedValue );

					if ( Double.IsNaN( value ) )
					{
						Utilities.DebugFail( "This should have been a division by zero error." );
						return ErrorValue.DivisionByZero;
					}

					return value;
				}
				catch ( InvalidCastException )
				{
					Utilities.DebugFail( "Error converting the value to a double: " + resolvedValue );
					return resolvedValue;
				}
			}

			switch ( errorValue.Code )
			{
				case ExcelCalcErrorCode.Reference:
					return ErrorValue.InvalidCellReference;

				case ExcelCalcErrorCode.Value:
					return ErrorValue.WrongOperandType;

				case ExcelCalcErrorCode.Div:
					return ErrorValue.DivisionByZero;

				case ExcelCalcErrorCode.NA:
					return ErrorValue.ArgumentOrFunctionNotAvailable;

				case ExcelCalcErrorCode.Num:
					return ErrorValue.ValueRangeOverflow;

				case ExcelCalcErrorCode.Null:
					return ErrorValue.EmptyCellRangeIntersection;

				case ExcelCalcErrorCode.Name:
					return ErrorValue.WrongFunctionName;

				case ExcelCalcErrorCode.Circularity:
					return ErrorValue.Circularity;

				default:
					Utilities.DebugFail( "Unknown error code: " + errorValue.Code );
					goto case ExcelCalcErrorCode.Reference;
			}
		}

		#endregion GetValueFormExcelCalcValue

		#region PerformDataTableReferenceReplacement

		public static bool PerformDataTableReferenceReplacement( ref CellCalcReference cellReference, ExcelCalcNumberStack numberStack )
		{
			// If the reference being used is not a cell, no data table replacement will need to be done.
			if ( cellReference == null )
				return false;

			IExcelCalcReference formulaOwner = numberStack.FormulaOwner;

			if ( formulaOwner == null )
				return false;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell targetCell = formulaOwner.Context as WorksheetCell;
			//
			//// If the formula owner is not a cell, no data table replacement will need to be done.
			//if ( targetCell == null )
			//    return false;
			//
			//WorksheetDataTable dataTable = targetCell.AssociatedDataTable;
			CellCalcReference targetCellReference = formulaOwner.Context as CellCalcReference;

			// If the formula owner is not a cell, no data table replacement will need to be done.
			if (targetCellReference == null)
				return false;

			WorksheetRow targetCellRow = targetCellReference.Row;
			short targetCellColumnIndex = targetCellReference.ColumnIndex;

			WorksheetDataTable dataTable = targetCellRow.GetCellValueRaw(targetCellColumnIndex) as WorksheetDataTable;

			// If the target cell is not in the interior cells of a data table, no data table replacement will need to be done.
			if ( dataTable == null )
				return false;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//return CalcUtilities.PerformDataTableReferenceReplacement( ref cellReference, targetCell, dataTable );
			return CalcUtilities.PerformDataTableReferenceReplacement(ref cellReference, targetCellRow, targetCellColumnIndex, dataTable);
		}

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public static bool PerformDataTableReferenceReplacement( ref CellCalcReference cellReference, WorksheetCell targetCell, WorksheetDataTable dataTable )
		public static bool PerformDataTableReferenceReplacement(ref CellCalcReference cellReference, WorksheetRow targetCellRow, short targetCellColumnIndex, WorksheetDataTable dataTable)
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell sourceCell = (WorksheetCell)cellReference.Context;
			CellCalcReference sourceCellReference = (CellCalcReference)cellReference.Context;
			WorksheetRow sourceCellRow = sourceCellReference.Row;
			short sourceCellColumnIndex = sourceCellReference.ColumnIndex;

			CellCalcReference newCellReference = null;

			// MD 3/12/12 - 12.1 - Table Support
			WorksheetCell rowInputCell = dataTable.RowInputCell;
			WorksheetCell columnInputCell = dataTable.ColumnInputCell;

			// The the cell being references if either the row input or cell input cells, replace the reference will the appropriate reference.
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if ( sourceCell == dataTable.RowInputCell )
			// MD 3/12/12 - 12.1 - Table Support
			//if (dataTable.RowInputCell != null && 
			//    dataTable.RowInputCell.Row == sourceCellRow && 
			//    dataTable.RowInputCell.ColumnIndexInternal == sourceCellColumnIndex)
			if (rowInputCell != null &&
				rowInputCell.Row == sourceCellRow &&
				rowInputCell.ColumnIndexInternal == sourceCellColumnIndex)
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//newCellReference = targetCell.Worksheet.Rows[ dataTable.CellsInTable.FirstRow ].Cells[ targetCell.ColumnIndex ].CalcReference;
				newCellReference = targetCellRow.Worksheet.Rows[dataTable.CellsInTable.FirstRow].GetCellCalcReference(targetCellColumnIndex);
			}
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//else if ( sourceCell == dataTable.ColumnInputCell )
			// MD 3/12/12 - 12.1 - Table Support
			//else if (dataTable.ColumnInputCell != null &&
			//    dataTable.ColumnInputCell.Row == sourceCellRow &&
			//    dataTable.ColumnInputCell.ColumnIndexInternal == sourceCellColumnIndex)
			else if (columnInputCell != null &&
				columnInputCell.Row == sourceCellRow &&
				columnInputCell.ColumnIndexInternal == sourceCellColumnIndex)
			{
				// MD 7/26/10 - TFS34398
				// Since the row is stored directly on the cell, use that.
				//newCellReference = targetCell.Worksheet.Rows[ targetCell.RowIndex ].Cells[ dataTable.CellsInTable.FirstColumn ].CalcReference;
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//newCellReference = targetCell.Row.Cells[dataTable.CellsInTable.FirstColumn].CalcReference;
				newCellReference = targetCellRow.GetCellCalcReference(dataTable.CellsInTable.FirstColumnInternal);
			}

			if ( newCellReference == null )
				return false;

			cellReference = newCellReference;
			return true;
		}

		#endregion PerformDataTableReferenceReplacement

		#region ShouldReferenceBeEvaluatedFirst



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		public static bool ShouldReferenceBeEvaluatedFirst( IExcelCalcReference reference, IExcelCalcReference circularChildReference )
		{
			ExcelRefBase excelReference = ExcelCalcEngine.GetResolvedReference( reference ) as ExcelRefBase;
			ExcelRefBase excelCircularChildReference = ExcelCalcEngine.GetResolvedReference( circularChildReference ) as ExcelRefBase;

			if ( excelReference == null || excelCircularChildReference == null )
				return false;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//IWorksheetCell cell = excelReference.Context as IWorksheetCell;
			//IWorksheetCell circularChildCell = excelCircularChildReference.Context as IWorksheetCell;
			//
			//if ( cell == null || circularChildCell == null )
			//    return false;
			//
			//int sheetDifference = circularChildCell.Worksheet.Index - cell.Worksheet.Index;
			//
			//if ( sheetDifference != 0 )
			//    return sheetDifference > 0;
			//
			//int rowDifference = circularChildCell.RowIndex - cell.RowIndex;
			//
			//if ( rowDifference != 0 )
			//    return rowDifference > 0;
			//
			//int columnDifference = circularChildCell.ColumnIndex - cell.ColumnIndex;
			//
			//if ( columnDifference != 0 )
			//    return columnDifference > 0;
			CellCalcReference cellReference = excelReference as CellCalcReference;
			CellCalcReference circularChildCellReference = excelCircularChildReference as CellCalcReference;

			if (cellReference == null || circularChildCellReference == null)
				return false;

			int sheetDifference = circularChildCellReference.Row.Worksheet.Index - cellReference.Row.Worksheet.Index;

			if (sheetDifference != 0)
				return sheetDifference > 0;

			int rowDifference = circularChildCellReference.Row.Index - cellReference.Row.Index;

			if (rowDifference != 0)
				return rowDifference > 0;

			int columnDifference = circularChildCellReference.ColumnIndex - cellReference.ColumnIndex;

			if (columnDifference != 0)
				return columnDifference > 0;

			return false;
		}

		#endregion ShouldReferenceBeEvaluatedFirst

		#region SplitArrayForArrayFormula

		// MD 12/1/11 - TFS96113
		// Instead of storing arrays for rectangular regions of values we now use an ArrayProxy.
		//internal static ExcelCalcValue SplitArrayForArrayFormula( ExcelCalcValue[ , ] array, CellCalcReference formulaOwner )
		internal static ExcelCalcValue SplitArrayForArrayFormula(ArrayProxy array, CellCalcReference formulaOwner)
		{
			int relativeColumn;
			int relativeRow;
			if ( CalcUtilities.SplitArrayForArrayFormula( formulaOwner, array.GetLength( 0 ), array.GetLength( 1 ), out relativeColumn, out relativeRow ) == false )
				return null;

			// Get the value from the array at the position corresponding to the target cell's relative position in the array formula's 
			// target region.
			return array[ relativeColumn, relativeRow ];
		}

		internal static bool SplitArrayForArrayFormula( CellCalcReference formulaOwner, int columnCount, int rowCount, out int relativeColumn, out int relativeRow )
		{
			// Determine the formula owner's relative position in the array formula's target region.
			formulaOwner.GetRelativeAddressesInArrayFormula( out relativeColumn, out relativeRow );

			// If the column index is out of range...
			if ( columnCount <= relativeColumn )
			{
				// ... but the value array is only one column wide, just use the value from the first column. Otherwise, return an #N/A error.
				if ( columnCount == 1 )
					relativeColumn = 0;
				else
					return false;
			}

			// If the row index is out of range...
			if ( rowCount <= relativeRow )
			{
				// ... but the value array is only one row wide, just use the value from the first row. Otherwise, return an #N/A error.
				if ( rowCount == 1 )
					relativeRow = 0;
				else
					return false;
			}

			return true;
		}

		#endregion SplitArrayForArrayFormula
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