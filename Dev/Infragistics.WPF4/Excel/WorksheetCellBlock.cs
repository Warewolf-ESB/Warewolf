using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.CalcEngine;






using System.Drawing;
using Infragistics.Shared;
using System.Runtime.InteropServices;


namespace Infragistics.Documents.Excel
{
	// MD 1/31/12 - TFS100573
	// Made this an abstract base class.
	//internal class WorksheetCellBlock
	internal abstract class WorksheetCellBlock
	{
		#region Member Variables

		// MD 1/31/12 - TFS100573
		// Moved these members down to the WorksheetCellBlockFull base class.
		#region Removed

		//private DataTypesCompressed dataTypes00Thru07;
		//private DataTypesCompressed dataTypes08Thru15;
		//private DataTypesCompressed dataTypes16Thru23;
		//private DataTypesCompressed dataTypes24Thru31;

		//private CellValue value00;
		//private CellValue value01;
		//private CellValue value02;
		//private CellValue value03;
		//private CellValue value04;
		//private CellValue value05;
		//private CellValue value06;
		//private CellValue value07;
		//private CellValue value08;
		//private CellValue value09;
		//private CellValue value10;
		//private CellValue value11;
		//private CellValue value12;
		//private CellValue value13;
		//private CellValue value14;
		//private CellValue value15;
		//private CellValue value16;
		//private CellValue value17;
		//private CellValue value18;
		//private CellValue value19;
		//private CellValue value20;
		//private CellValue value21;
		//private CellValue value22;
		//private CellValue value23;
		//private CellValue value24;
		//private CellValue value25;
		//private CellValue value26;
		//private CellValue value27;
		//private CellValue value28;
		//private CellValue value29;
		//private CellValue value30;
		//private CellValue value31;

		#endregion // Removed

		// MD 1/31/12 - TFS100573
		// The block will now store its own index so we can use a sorted List<T> instead of a SortedList<T>, which
		// has two arrays instead of one.
		private readonly short blockIndex;

		#endregion // Member Variables

		#region Constructor

		// MD 1/31/12 - TFS100573
		//public WorksheetCellBlock() { } 
		public WorksheetCellBlock(short blockIndex) 
		{
			this.blockIndex = blockIndex;
		} 

		#endregion  // Constructor

		#region Methods

		// MD 1/31/12 - TFS100573
		// Added these abstract methods to be override by derived classes.
		protected abstract bool ConvertToLargerBlockIfFull(out WorksheetCellBlock replacementBlock);
		protected abstract CellValue GetCellValue(short columnIndex);
		protected abstract DataType GetDataType(short columnIndex);

		// MD 2/8/12 - 12.1 - Table Support
		protected abstract bool IsInTableHeaderOrTotalRow(short columnIndex);

		protected abstract void SetCellValue(short columnIndex, CellValue value, bool isNull);
		protected abstract void SetDataType(short columnIndex, DataType dataType);

		// MD 2/8/12 - 12.1 - Table Support
		protected abstract void SetIsInTableHeaderOrTotalRow(short columnIndex, bool value);

		#region Static Methods

		#region ApplyBlockingValue







		public static void ApplyBlockingValue(IWorksheetRegionBlockingValue value)
		{
			WorksheetCellBlock.ApplyValueToRegion(value.Region, value, true);
		}

		#endregion ApplyBlockingValue

		#region ApplyValueToRegion






		private static void ApplyValueToRegion(WorksheetRegion region, object value, bool checkForBlockingValues)
		{
			if (region == null)
				return;

			Worksheet worksheet = region.Worksheet;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (worksheet == null)
			{
				Utilities.DebugFail("This is unexpected");
				return;
			}

			// We have to do this backwards because if has an ArrayFormula or WorksheetDataTable on the cells previously, 
			// setting the top-left cell's value to null first would cause all other cells to not know what their old value 
			// was anymore, since they just have a reference to the top-left cell's value.
			for (int rowIndex = region.LastRow; rowIndex >= region.FirstRow; rowIndex--)
			{
				WorksheetRow row = worksheet.Rows[rowIndex];

				for (short columnIndex = region.LastColumnInternal; columnIndex >= region.FirstColumnInternal; columnIndex--)
					row.SetCellValueRaw(columnIndex, value, checkForBlockingValues);
			}
		}

		#endregion ApplyValueToRegion

		#region ClearBlockingValue







		public static void ClearBlockingValue(IWorksheetRegionBlockingValue value)
		{
			WorksheetCellBlock.ApplyValueToRegion(value.Region, null, false);
		}

		#endregion ClearBlockingValue

		// MD 1/8/12 - 12.1 - Cell Format Updates
		// Removed - this is no longer needed.
		#region Removed

		//#region GetResolvedCellFormatHelper

		//public static WorksheetCellFormatData GetResolvedCellFormatHelper(
		//    Worksheet worksheet,
		//    WorksheetColumn column,
		//    WorksheetCellFormatData cellFormatData,
		//    WorksheetCellFormatData rowFormatData,
		//    WorksheetCellFormatData emptyFormatData)
		//{
		//    WorksheetCellFormatData columnFormatData = column != null && column.HasCellFormat
		//        ? column.CellFormatInternal.Element
		//        : emptyFormatData;

		//    // Combine the row and column formats, giving the row format height priority
		//    WorksheetCellFormatData resolvedData = WorksheetCellFormatData.Combine(rowFormatData, columnFormatData, emptyFormatData);

		//    // MD 8/20/07 - BR25818
		//    // The resolved data must be in the cell formats collection for the fonts to combine correctly
		//    // Find an equivalent data object in the collection
		//    // MD 7/26/10 - TFS34398
		//    // Use the cached workbook.
		//    //WorksheetCellFormatData equivalentResolvedData = this.worksheet.Workbook.CellFormats.Find( resolvedData );
		//    WorksheetCellFormatData equivalentResolvedData = worksheet.Workbook.CellFormats.Find(resolvedData);
		//    WorksheetCellFormatData dataToRemoveFromCollection = null;

		//    if (equivalentResolvedData == null)
		//    {
		//        // If there was no equivalent data object, add the resolved data object to the collection 
		//        // and store it so we can remove it later.
		//        dataToRemoveFromCollection = resolvedData;
		//        // MD 7/26/10 - TFS34398
		//        // Use the cached workbook.
		//        //this.worksheet.Workbook.CellFormats.Add( resolvedData );
		//        worksheet.Workbook.CellFormats.Add(resolvedData);
		//    }
		//    else
		//    {
		//        // If there was an equivalent data object in the collection, use that as the resolved object 
		//        // instead because it is already in the collection.
		//        resolvedData = equivalentResolvedData;
		//    }

		//    // Combine the cell format with the resolved format, giving the cell format the highest priority.
		//    resolvedData = WorksheetCellFormatData.Combine(cellFormatData, resolvedData, emptyFormatData);

		//    // MD 8/20/07 - BR25818
		//    // If we added the resolved data to the collection, remove it, we don't need it in the collection anymore
		//    if (dataToRemoveFromCollection != null)
		//        // MD 7/26/10 - TFS34398
		//        // Use the cached workbook.
		//        //this.worksheet.Workbook.CellFormats.Remove( dataToRemoveFromCollection );
		//        worksheet.Workbook.CellFormats.Remove(dataToRemoveFromCollection);

		//    return resolvedData;
		//}

		//#endregion  // GetResolvedCellFormatHelper

		#endregion // Removed

		#region IsCellValueSavedAsString






		private static bool IsCellValueSavedAsString(DataType dataType, CellValue cellValue)
		{
			switch (dataType)
			{
				case DataType.Null:
					return false;

				case DataType.String:
				case DataType.FormattedString:
					return true;

				case DataType.Encoded:
					{
						switch (cellValue.DataTypeEncoded)
						{
							case DataTypeEncoded.Char:
							case DataTypeEncoded.StringBuilder:
							case DataTypeEncoded.Guid:
							case DataTypeEncoded.Enum:
								return true;
						}
						break;
					}
			}

			return false;
		}

		#endregion // IsCellValueSavedAsString

		#region IsCellValueSupported






		public static bool IsCellValueSupported(object value)
		{
			if (value is int
				|| value is double
				|| value is bool
				|| value is DateTime
				|| value is string
				|| value is FormattedString
				|| value is ErrorValue
				|| value is Formula
				|| value is StringElement)
			{
				return true;
			}

			if (value is byte
				|| value is sbyte
				|| value is short
				|| value is ushort
				|| value is uint
				|| value is long
				|| value is ulong
				|| value is float
				|| value is decimal
				|| value is char
				|| value is StringBuilder
				|| value is DBNull
				|| value is WorksheetDataTable
				|| value is Guid)
			{
				return true;
			}

			if (value.GetType().IsEnum)
			{
				return true;
			}

			return false;
		}

		#endregion IsCellValueSupported

		#region IsGeneralFormat

		public static bool IsGeneralFormat(string formatString)
		{
			return
				formatString == null ||
				StringComparer.InvariantCultureIgnoreCase.Compare(formatString, "General") == 0;
		}

		#endregion IsGeneralFormat

		#region ShouldValueSetDefaultFormatString

		public static bool ShouldValueSetDefaultFormatString(object value)
		{
			if (value is DateTime)
				return true;

			return false;
		}

		#endregion ShouldValueSetDefaultFormatString

		#region VerifyDirectSetValue

		public static void VerifyDirectSetValue(object value)
		{
			if (value == null || value is ValueType)
				return;

			if (value is Formula)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CantSetFormulaDirectly"));

			if (value is WorksheetDataTable)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CantSetDataTableDirectly"));

			// MD 8/20/08 - Excel formula solving
			// The Circularity error value should not be set directly on a cell.
			if ((value as ErrorValue) == ErrorValue.Circularity)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CantSetCircularityErrorDirectly"));
		}

		#endregion VerifyDirectSetValue

		#region VerifyRegionForBlockingValue






		// MD 3/2/12 - 12.1 - Table Support
		//public static void VerifyRegionForBlockingValue(IWorksheetRegionBlockingValue value, WorksheetRegion region)
		public static void VerifyRegionForBlockingValue(IWorksheetRegionBlockingValue value, WorksheetRegion fullRegion, WorksheetRegion region)
		{
			Worksheet worksheet = region.Worksheet;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (worksheet == null)
				throw new InvalidOperationException(SR.GetString("LE_ArgumentException_RegionShiftedOffWorksheet"));

			if ((value is ArrayFormula) && region.IsSingleCell)
			{
				// Array formulas are allowed in tables in they are in a single cell only.
			}
			else
			{
				
				foreach (WorksheetCell cell in fullRegion)
				{
					if (cell.AssociatedTable != null)
						value.ThrowExceptionWhenTableInRegion();
				}
			}

			
			for (int rowIndex = region.FirstRow; rowIndex <= region.LastRow; rowIndex++)
			{
				WorksheetRow row = worksheet.Rows[rowIndex];

				for (short columnIndex = (short)region.FirstColumn; columnIndex <= region.LastColumn; columnIndex++)
				{
					if (row.GetCellAssociatedMergedCellsRegionInternal(columnIndex) != null)
						value.ThrowExceptionWhenMergedCellsInRegion();

					// Check to see if a blocking value exists already in one of the cells in the region
					IWorksheetRegionBlockingValue blockingValue = row.GetCellValueRaw(columnIndex) as IWorksheetRegionBlockingValue;

					if (blockingValue != null && blockingValue != value)
					{
						WorksheetRegion blockingRegion = blockingValue.Region;
						Debug.Assert(blockingRegion != null);

						if (blockingRegion != null)
						{
							if (region.Contains(blockingRegion))
							{
								// If the new region completely contains the already applied blocking value, remove the value
								blockingValue.RemoveFromRegion();
							}
							else
							{
								// If the blocking region already applied to the current cell is not fully contained by this new region,
								// the operation is not allowed
								blockingValue.ThrowBlockingException();
							}
						}
					}
				}
			}
		}

		#endregion VerifyRegionForBlockingValue

		#region VerifyStringLength

		public static void VerifyStringLength(object newValue)
		{
			if (newValue == null || newValue is ValueType)
				return;

			int strLength = 0;

			string strValue = newValue as string;
			FormattedString formattedString = newValue as FormattedString;
			StringElement formattedStringElement = newValue as StringElement;
			StringBuilder stringBuilder = newValue as StringBuilder;

			if (strValue != null)
				strLength = strValue.Length;
			else if (formattedString != null)
				strLength = formattedString.UnformattedString.Length;
			else if (formattedStringElement != null)
				strLength = formattedStringElement.UnformattedString.Length;
			else if (stringBuilder != null)
				strLength = stringBuilder.Length;

			const int maxStringLength = Int16.MaxValue;

			if (maxStringLength < strLength)
				throw new ArgumentException(String.Format(SR.GetString("LE_ArgumentException_CellValueStringLength"), maxStringLength));
		}

		#endregion VerifyStringLength

		#endregion // Static Methods

		#region Public Methods

		#region ClearCell

		public void ClearCell(WorksheetRow row, short columnIndex)
		{
			WorksheetCellBlock replacementBlock;

			// Clear the isInTableHeaderOrTotalRow flag first so we don't try t do any value coercion when setting the value it null.
			this.SetIsInTableHeaderOrTotalRow(row, columnIndex, false, out replacementBlock);
			Debug.Assert(replacementBlock == null, "We should never replace the block when clearing the isInTableHeaderOrTotalRow flag.");

			this.SetCellValueRaw(row, columnIndex, null, out replacementBlock);
			Debug.Assert(replacementBlock == null, "We should never replace the block when setting the value to null.");
		}

		#endregion // ClearCell

		#region DoesCellHaveData

		public bool DoesCellHaveData(WorksheetRow row, short columnIndex)
		{
			if (this.DoesCellHaveValue(columnIndex))
				return true;

			WorksheetCellFormatData cellFormat;
			// MD 2/25/12 - 12.1 - Table Support
			//if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
			if (row.TryGetCellFormat(columnIndex, out cellFormat))
			{
				Workbook workbook = row.Worksheet.Workbook;

				// MD 1/1/12 - 12.1 - Cell Format Updates
				// The default element is now a cell format, not a style format, so we can see if they are equal.
				//if (workbook == null || cellFormat.HasSameData(workbook.CellFormats.DefaultElement) == false)
				if (workbook == null || cellFormat.EqualsInternal(workbook.CellFormats.DefaultElement) == false)
				{
					return true;
				}
			}

			return false;
		}

		#endregion  // DoesCellHaveData

		#region DoesCellHaveValue

		public bool DoesCellHaveValue(short columnIndex)
		{
			return this.GetDataType(columnIndex) != DataType.Null;
		}

		#endregion  // DoesCellHaveValue

		#region DoesCellUseCalculations

	
		public bool DoesCellUseCalculations(short columnIndex)
		{
			// MD 2/27/12 - 12.1 - Table Support
			// Moved all code to the new overload.
			DataType dataType;
			CellValue cellValue;
			return this.DoesCellUseCalculations(columnIndex, out dataType, out cellValue);
		}

		// MD 2/27/12 - 12.1 - Table Support
		public bool DoesCellUseCalculations(short columnIndex, out DataType dataType, out CellValue cellValue)
		{
			// MD 2/27/12 - 12.1 - Table Support
			//DataType dataType = this.GetDataType(columnIndex);
			dataType = this.GetDataType(columnIndex);
			cellValue = this.GetCellValue(columnIndex);

			if (dataType == DataType.Encoded)
			{
				// MD 2/27/12 - 12.1 - Table Support
				//CellValue value = this.GetCellValue(columnIndex);
				//DataTypeEncoded dataTypeEncoded = value.DataTypeEncoded;
				DataTypeEncoded dataTypeEncoded = cellValue.DataTypeEncoded;

				switch (dataTypeEncoded)
				{
					case DataTypeEncoded.ArrayFormulaReference:
					case DataTypeEncoded.Formula:
					case DataTypeEncoded.WorksheetDataTable:
					case DataTypeEncoded.WorksheetDataTableReference:
						return true;
				}
			}

			return false;
		}

		#endregion // DoesCellUseCalculations

		#region GetCalculatedValue

		public object GetCalculatedValue(WorksheetRow row, short columnIndex)
		{
			CellCalcReference cellCalcReference;
			if (row.TryGetCellCalcReference(columnIndex, out cellCalcReference))
				cellCalcReference.EnsureCalculated();

			object valueRaw = this.GetCellValueRaw(row, columnIndex);

			Formula formula = valueRaw as Formula;
			if (formula != null)
				return formula.GetCalculatedValue(row, columnIndex);

			WorksheetDataTable associatedDataTable = valueRaw as WorksheetDataTable;
			if (associatedDataTable != null)
			{
				int relativeRow = row.Index - associatedDataTable.InteriorCells.FirstRow;
				int relativeColumn = columnIndex - associatedDataTable.InteriorCells.FirstColumn;

				return associatedDataTable.CalculatedValues[relativeColumn, relativeRow];
			}

			Utilities.DebugFail("A cell without a formula or data table should not have its calculated value set.");
			return null;
		}

		#endregion  // GetCalculatedValue

		// MD 2/8/12 - 12.1 - Table Support
		#region GetCellTextInternal

		public string GetCellTextInternal(WorksheetRow row, GetCellTextParameters paramters,
			out double? numericValue,
			out ValueFormatter.SectionType formattedAs)
		{
			numericValue = Double.NaN;
			formattedAs = ValueFormatter.SectionType.Text;

			object value;
			if (paramters.UseCalculatedValues == false && this.DoesCellUseCalculations(paramters.ColumnIndex))
			{
				value = this.GetCellValueRaw(row, paramters.ColumnIndex);

				Formula formula = value as Formula;
				if (formula != null)
					return formula.ToString(row.Worksheet.CellReferenceMode);

				WorksheetDataTable dataTable = value as WorksheetDataTable;
				if (dataTable != null)
					return dataTable.GetDisplayFormula();

				Utilities.DebugFail("This is unexpected.");
			}

			value = this.GetCellValue(row, paramters.ColumnIndex);
			return WorksheetCellBlock.GetCellTextInternal(row.Worksheet, row, paramters, value, out numericValue, out formattedAs);
		}

		public static string GetCellTextInternal(Worksheet worksheet, WorksheetRow row, GetCellTextParameters parameters, object value)
		{
			double? numericValue;
			ValueFormatter.SectionType formattedAs;
			return WorksheetCellBlock.GetCellTextInternal(worksheet, row, parameters, value, out numericValue, out formattedAs);
		}

		private static string GetCellTextInternal(Worksheet worksheet, WorksheetRow row, GetCellTextParameters paramters,
			object value,
			out double? numericValue,
			out ValueFormatter.SectionType formattedAs)
		{
			numericValue = null;
			formattedAs = ValueFormatter.SectionType.Text;

			if (value == null)
				return string.Empty;

			string stringValue = WorksheetCellBlock.GetDefaultValueText(value);
			if (Utilities.TestFlag(paramters.PreventTextFormattingTypes, PreventTextFormattingTypes.String) &&
				WorksheetCellBlock.IsStringValue(value))
			{
				return stringValue;
			}

			Workbook workbook = worksheet.Workbook;

			double convertedValue;
			if (Utilities.TryGetNumericValue(workbook, value, out convertedValue))
			{
				numericValue = convertedValue;
				formattedAs = ValueFormatter.SectionType.Number;
			}

			ValueFormatter formatter;
			WorksheetCellFormatData cellFormat = null;
			if (worksheet.DisplayOptions.ShowFormulasInCells)
			{
				string formatString;
				if (numericValue.HasValue)
				{
					if (numericValue.Value % 1 == 0)
						formatString = "0";
					else
						formatString = "0.0";
				}
				else
				{
					formatString = "@";
				}

				paramters.TextFormatMode = TextFormatMode.IgnoreCellWidth;

				// MD 4/9/12 - TFS101506
				//formatter = new ValueFormatter(workbook, formatString);
				formatter = new ValueFormatter(workbook, formatString, worksheet.Culture);
			}
			else
			{
				cellFormat = worksheet.GetCellFormatElementReadOnly(row, paramters.ColumnIndex);

				if (workbook != null)
				{
					formatter = workbook.Formats.GetValueFormatter(cellFormat.FormatStringIndexResolved);
				}
				else
				{
					// MD 4/9/12 - TFS101506
					//formatter = new ValueFormatter(workbook, cellFormat.FormatStringResolved);
					formatter = new ValueFormatter(workbook, cellFormat.FormatStringResolved, worksheet.Culture);
				}
			}

			if (formatter.IsValid == false)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_InvalidFormatString_GetTextCall"));

			int cellWidthInPixels = -1;
			if (paramters.TextFormatMode == TextFormatMode.IgnoreCellWidth)
			{
				cellFormat = null;
			}
			else
			{
				cellWidthInPixels = Math.Max(0, (int)worksheet.GetColumnWidthInPixels(paramters.ColumnIndex) - worksheet.GetColumnWidthPadding());
				if (cellFormat == null)
					cellFormat = worksheet.GetCellFormatElementReadOnly(row, paramters.ColumnIndex);
			}

			return formatter.FormatValue(numericValue, stringValue, value, cellWidthInPixels, cellFormat, out formattedAs);
		}

		#endregion // GetCellTextInternal

		#region GetCellValue

		public object GetCellValue(WorksheetRow row, short columnIndex)
		{
			// MD 2/27/12 - 12.1 - Table Support
			//if (this.DoesCellUseCalculations(columnIndex))
			DataType dataType;
			CellValue cellValue;
			if (this.DoesCellUseCalculations(columnIndex, out dataType, out cellValue))
				return this.GetCalculatedValue(row, columnIndex);

			// MD 2/27/12 - 12.1 - Table Support
			//object valueInternal = this.GetCellValueInternal(row, columnIndex);
			object valueRaw = this.GetCellValueRaw(row, columnIndex, null, dataType, cellValue);

			StringElement formattedStringElement = valueRaw as StringElement;
			if (formattedStringElement != null)
				return formattedStringElement.UnformattedString;

			FormattedStringValueReference formattedStringValueReference = valueRaw as FormattedStringValueReference;
			if (formattedStringValueReference != null)
				return formattedStringValueReference.Value;

			return valueRaw;
		}

		#endregion  // GetCellValue

		// MD 1/29/12 - 12.1 - Cell Format Updates
		#region GetCellValueIfFormattedString

		public StringElement GetCellValueIfFormattedString(WorksheetRow row, short columnIndex)
		{
			DataType type = this.GetDataType(columnIndex);

			if (type != DataType.FormattedString)
				return null;

			return this.GetFormattedStringElement(row, columnIndex, this.GetCellValue(columnIndex));
		}

		#endregion  // GetCellValueIfFormattedString

		// MD 4/18/11 - TFS62026
		// Added a method for the special case where we only need to get the value if it is a StringBuilder.
		// That way, we don't have to go getting values that are relatively expensive to get (shared strings) when we don't need them.
		#region GetCellValueIfStringBuilder

		public StringBuilder GetCellValueIfStringBuilder(WorksheetRow row, short columnIndex)
		{
			DataType type = this.GetDataType(columnIndex);

			if (type != DataType.Encoded)
				return null;

			CellValue cellValue = this.GetCellValue(columnIndex);

			if (cellValue.DataTypeEncoded != DataTypeEncoded.StringBuilder)
				return null;

			return this.GetValueObject(row, columnIndex) as StringBuilder;
		}

		#endregion  // GetCellValueIfStringBuilder

		#region GetCellValueRaw

		public object GetCellValueRaw(WorksheetRow row, short columnIndex)
		{
			// MD 2/1/12 - TFS100573
			// Moved all code to a new overload.
			return this.GetCellValueRaw(row, columnIndex, null);
		}

		// MD 3/4/12 - 12.1 - Table Support
		public object GetCellValueRaw(WorksheetRow row, short columnIndex, out DataType type, out CellValue cellValue)
		{
			type = this.GetDataType(columnIndex);
			cellValue = this.GetCellValue(columnIndex);
			return this.GetCellValueRaw(row, columnIndex, null, type, cellValue);
		}

		// MD 2/1/12 - TFS100573
		// Created a new overload which should be called when getting save values.
		public object GetCellValueRaw(WorksheetRow row, short columnIndex, WorkbookSerializationManager managerForSaving)
		{
			// MD 2/27/12 - 12.1 - Table Support
			// Moved all code to a new overload
			DataType type = this.GetDataType(columnIndex);
			CellValue cellValue = this.GetCellValue(columnIndex);
			return this.GetCellValueRaw(row, columnIndex, managerForSaving, type, cellValue);
		}

		// MD 2/27/12 - 12.1 - Table Support
		public object GetCellValueRaw(WorksheetRow row, short columnIndex, WorkbookSerializationManager managerForSaving, DataType type, CellValue cellValue)
		{
			// MD 2/27/12 - 12.1 - Table Support
			//DataType type;
			//CellValue cellValue;
			//
			//// MD 2/1/12 - TFS100573
			//// There is now an managerForSaving parameter and if we are saving, we should return what actually gets saved in the 
			//// file and not necessarily what is returned at runtime.
			////return this.GetCellValueInternal(row, columnIndex, out type, out cellValue);
			//bool isForSaving = managerForSaving != null;
			//object value = this.GetCellValueInternal(row, columnIndex, isForSaving, out type, out cellValue);
			bool isForSaving = managerForSaving != null;
			object value = this.GetCellValueRaw(row, columnIndex, isForSaving, type, cellValue);

			if (isForSaving)
			{
				FormattedStringValueReference valueReference = value as FormattedStringValueReference;
				if (valueReference != null)
					return this.GetStringElementIndex(row, columnIndex, valueReference);

				StringBuilder stringBuilder = value as StringBuilder;
				if (stringBuilder != null)
					return new StringElementIndex(managerForSaving.AdditionalStringsInStringTable[stringBuilder]);

				if (value is DateTime)
				{
					double? excelDate = ExcelCalcValue.DateTimeToExcelDate(managerForSaving.Workbook, (DateTime)value);

					if (excelDate.HasValue)
						return excelDate;

					Utilities.DebugFail("We should never get into this situation.");
					return 0;
				}
			}

			return value;
		}

		// MD 2/1/12 - TFS100573
		// Added an isForSaving parameter to indicate whether we need to get the value which will be saved in the file format,
		// which will be the index of the string for strings instead of the string itself.
		//private object GetCellValueInternal(WorksheetRow row, short columnIndex, out DataType type, out CellValue cellValue)
		private object GetCellValueRaw(WorksheetRow row, short columnIndex, bool isForSaving, out DataType type, out CellValue cellValue)
		{
			type = this.GetDataType(columnIndex);
			cellValue = this.GetCellValue(columnIndex);

			// MD 2/27/12 - 12.1 - Table Support
			// Moved all code to a new overload.
			return this.GetCellValueRaw(row, columnIndex, isForSaving, type, cellValue);
		}

		// MD 2/27/12 - 12.1 - Table Support
		private object GetCellValueRaw(WorksheetRow row, short columnIndex, bool isForSaving, DataType type, CellValue cellValue)
		{
			switch (type)
			{
				case DataType.Null: return null;

				case DataType.Int64: return (long)cellValue.UInt64Value;
				case DataType.UInt64: return cellValue.UInt64Value;
				case DataType.Double: return cellValue.DoubleValue;
				case DataType.DateTime:
					{
						ulong dateTimeEncoded = cellValue.UInt64Value;
						DateTimeKind kind = (DateTimeKind)(dateTimeEncoded >> 0x3E);
						long ticks = (long)(dateTimeEncoded & 0x3fffffffffffffff);
						return new DateTime(ticks, kind);
					}

				case DataType.String:

					// MD 2/1/12 - TFS100573
					if (isForSaving)
						return this.GetStringElementIndex(row, columnIndex, cellValue);

					return this.GetFormattedStringElement(row, columnIndex, cellValue);

				case DataType.FormattedString:
					{
						// MD 2/1/12 - TFS100573
						if (isForSaving)
							return this.GetStringElementIndex(row, columnIndex, cellValue);

						StringElement element = this.GetFormattedStringElement(row, columnIndex, cellValue);

						Debug.Assert(element != null, "Could not find the string in the string table.");
						if (element != null)
						{
							// MD 2/2/12 - TFS100573
							// The string element no longer has a reference to the Workbook.
							//FormattedString formattedString = new FormattedString(element, false);
							FormattedString formattedString = new FormattedString(row.Worksheet.Workbook, element, true, false);

							formattedString.SetOwningCell(row, columnIndex);
							return formattedString;
						}

						return null;
					}

				case DataType.Encoded:
					{
						switch (cellValue.DataTypeEncoded)
						{
							case DataTypeEncoded.Byte: return (byte)cellValue.EncodedUIntValue;
							case DataTypeEncoded.SByte: return (sbyte)cellValue.EncodedUIntValue;
							case DataTypeEncoded.Int16: return (short)cellValue.EncodedUIntValue;
							case DataTypeEncoded.UInt16: return (ushort)cellValue.EncodedUIntValue;
							case DataTypeEncoded.Int32: return (int)cellValue.EncodedUIntValue;
							case DataTypeEncoded.UInt32: return cellValue.EncodedUIntValue;
							case DataTypeEncoded.Single: return cellValue.EncodedSingleValue;
							case DataTypeEncoded.Boolean: return cellValue.EncodedUIntValue != 0;
							case DataTypeEncoded.DBNull: return DBNull.Value;
							case DataTypeEncoded.ErrorValue: return ErrorValue.FromValue((byte)cellValue.EncodedUIntValue);

							case DataTypeEncoded.Formula:
								Formula formula = row.FindFormula(columnIndex);
								Debug.Assert(formula != null, "We did not find the formula in the row.");
								return formula;

							case DataTypeEncoded.WorksheetDataTable:
							case DataTypeEncoded.Decimal:
							case DataTypeEncoded.StringBuilder:
								return this.GetValueObject(row, columnIndex);

							// These will be stored in a dictionary on the row as a FormattedStringValueReference
							case DataTypeEncoded.Char:
							case DataTypeEncoded.DateTimeNotConvertible:
							case DataTypeEncoded.Enum:
							case DataTypeEncoded.Guid:
								return this.GetValueObject(row, columnIndex);

							case DataTypeEncoded.ArrayFormulaReference:
							case DataTypeEncoded.WorksheetDataTableReference:
								object referencedValue = row.Worksheet.Rows[cellValue.RowIndex].GetCellValueRaw(cellValue.ColumnIndex);
								Debug.Assert(referencedValue != null, "The refernce value is missing from the top-left interior cell.");
								return referencedValue;

							default:
								Utilities.DebugFail("Unknown DataTypeEncoded: " + cellValue.DataTypeEncoded);
								return null;
						}
					}

				default:
					Utilities.DebugFail("Unknown DataType: " + type);
					return null;
			}
		}

		#endregion // GetCellValueRaw

		// MD 2/23/12 - 12.1 - Table Support
		#region GetDefaultValueText

		public static string GetDefaultValueText(object value)
		{
			if (value == null)
				return string.Empty;

			if (value is bool)
				return value.ToString().ToUpper();

			return value.ToString();
		}

		#endregion // GetDefaultValueText

		#region GetFormula

		public Formula GetFormula(WorksheetRow row, short columnIndex)
		{
			return this.GetCellValueRaw(row, columnIndex) as Formula;
		}

		#endregion  // GetFormula

		// MD 3/1/12 - 12.1 - Table Support
		#region GetIsInTableHeaderOrTotalRow

		public bool GetIsInTableHeaderOrTotalRow(short columnIndex)
		{
			return this.IsInTableHeaderOrTotalRow(columnIndex);
		}

		#endregion // GetIsInTableHeaderOrTotalRow

		// MD 2/16/12 - 12.1 - Table Support
		#region GetValueCoercionType

		private ValueCoercionType GetValueCoercionType(WorksheetRow row, short columnIndex, out WorksheetTable table)
		{
			return this.GetValueCoercionType(row, columnIndex, this.IsInTableHeaderOrTotalRow(columnIndex), out table);
		}

		private ValueCoercionType GetValueCoercionType(WorksheetRow row, short columnIndex, bool isInTableHeaderOrTotalRow, out WorksheetTable table)
		{
			table = null;
			if (isInTableHeaderOrTotalRow == false)
				return ValueCoercionType.None;

			
			foreach (WorksheetTable testTable in row.Worksheet.Tables)
			{
				if (testTable.Contains(row, columnIndex) == false)
					continue;

				WorksheetRegion headerRegion = testTable.HeaderRowRegion;
				if (headerRegion != null && headerRegion.Contains(row, columnIndex))
				{
					table = testTable;
					return ValueCoercionType.TableHeaderCell;
				}

				WorksheetRegion totalsRegion = testTable.TotalsRowRegion;
				if (totalsRegion != null && totalsRegion.Contains(row, columnIndex))
				{
					table = testTable;
					return ValueCoercionType.TableTotalCell;
				}
			}

			Utilities.DebugFail("This is unexpected.");
			return ValueCoercionType.None;
		}

		#endregion // GetValueCoercionType

		#region IsCellValueSavedAsString

		public bool IsCellValueSavedAsString(WorksheetRow row, short columnIndex, out object value)
		{
			DataType type;
			CellValue cellValue;

			// MD 2/1/12 - TFS100573
			// Pass in False for the isForSaving parameter.
			//value = this.GetCellValueInternal(row, columnIndex, out type, out cellValue);
			value = this.GetCellValueRaw(row, columnIndex, false, out type, out cellValue);

			return WorksheetCellBlock.IsCellValueSavedAsString(type, cellValue);
		}

		#endregion  // IsCellValueSavedAsString

		// MD 2/27/12 - 12.1 - Table Support
		#region IsStringValue

		public static bool IsStringValue(object value)
		{
			if (value == null)
				return false;

			return
				value is string ||
				value is FormattedString ||
				value is char ||
				value is Guid ||
				value is StringBuilder ||
				value.GetType().IsEnum;
		}

		#endregion // IsStringValue

		// MD 3/12/12 - 12.1 - Table Support
		#region RegenerateFormulas

		public static void RegenerateFormulas(WorksheetRow row, short columnIndex, object valueRaw)
		{
			row.SetFormulaOnCalcReference(columnIndex, valueRaw, true);
			row.DirtyCellCalcReference(columnIndex);
		}

		#endregion // RegenerateFormulas

		#region SetCalculatedValue

		public void SetCalculatedValue(WorksheetRow row, short columnIndex, object caclulatedValue)
		{
			object valueRaw = this.GetCellValueRaw(row, columnIndex);

			Formula formula = valueRaw as Formula;
			if (formula != null)
			{
				formula.SetCalculatedValue(row, columnIndex, caclulatedValue);
				return;
			}

			WorksheetDataTable associatedDataTable = valueRaw as WorksheetDataTable;
			if (associatedDataTable != null)
			{
				int relativeRow = row.Index - associatedDataTable.InteriorCells.FirstRow;
				int relativeColumn = columnIndex - associatedDataTable.InteriorCells.FirstColumn;

				associatedDataTable.CalculatedValues[relativeColumn, relativeRow] = caclulatedValue;
				return;
			}

			Utilities.DebugFail("A cell without a formula or data table should not have its calculated value set.");
		}

		#endregion  // SetCalculatedValue

		// MD 1/31/12 - TFS100573
		#region SetCellValueAndData

		public void SetCellValueAndData(short columnIndex, CellValue value, DataType dataType)
		{
			this.SetCellValue(columnIndex, value, dataType == DataType.Null);
			this.SetDataType(columnIndex, dataType);
		}

		#endregion // SetCellValueAndData

		// MD 2/8/12 - 12.1 - Table Support
		#region SetCellValueCoercionType

		public void SetIsInTableHeaderOrTotalRow(WorksheetRow row, short columnIndex, bool isInTableHeaderOrTotalRow, out WorksheetCellBlock replacementBlock)
		{
			this.SetIsInTableHeaderOrTotalRow(row, columnIndex, isInTableHeaderOrTotalRow, true, out replacementBlock);
		}

		public void SetIsInTableHeaderOrTotalRow(WorksheetRow row, short columnIndex, bool isInTableHeaderOrTotalRow, bool shouldUpdateCurrentValue, out WorksheetCellBlock replacementBlock)
		{
			if (isInTableHeaderOrTotalRow == false)
			{
				replacementBlock = null;
			}
			else if (this.ConvertToLargerBlockIfFull(out replacementBlock))
			{
				row.ReplaceCellBlock(columnIndex, replacementBlock);

				WorksheetCellBlock temp;
				replacementBlock.SetIsInTableHeaderOrTotalRow(row, columnIndex, isInTableHeaderOrTotalRow, out temp);
				Debug.Assert(temp == null, "The replacement block should not also have been replaced.");
				return;
			}

			bool oldIsInTableHeaderOrTotalRow = this.IsInTableHeaderOrTotalRow(columnIndex);
			if (oldIsInTableHeaderOrTotalRow == isInTableHeaderOrTotalRow)
				return;

			this.SetIsInTableHeaderOrTotalRow(columnIndex, isInTableHeaderOrTotalRow);

			if (shouldUpdateCurrentValue)
			{
				WorksheetTable table;
				ValueCoercionType valueCoercionType = this.GetValueCoercionType(row, columnIndex, isInTableHeaderOrTotalRow, out table);
				if (valueCoercionType == ValueCoercionType.None)
					return;

				if (valueCoercionType == ValueCoercionType.TableTotalCell &&
					this.DoesCellUseCalculations(columnIndex))
					return;

				object value = this.GetCellValue(row, columnIndex);
				if (value is string)
					return;

				if (valueCoercionType == ValueCoercionType.TableTotalCell &&
					value == null)
					return;

				this.SetCellValueRaw(row, columnIndex, value, false, true, out replacementBlock);
				Debug.Assert(replacementBlock == null, "We shouldn't have replaced the block here.");
			}
		}

		#endregion // SetCellValueCoercionType

		#region SetCellValueRaw

		// MD 1/31/12 - TFS100573
		// Added an out parameter which indicates a reaplcement cell block, if any.
		//public void SetCellValueInternal(WorksheetRow row, short columnIndex, object newValueInternal)
		//{
		//    this.SetCellValueInternal(row, columnIndex, newValueInternal, true);
		//}
		public void SetCellValueRaw(WorksheetRow row, short columnIndex, object newValueRaw, out WorksheetCellBlock replacementBlock)
		{
			this.SetCellValueRaw(row, columnIndex, newValueRaw, true, out replacementBlock);
		}

		// MD 1/31/12 - TFS100573
		// Added an out parameter which indicates a reaplcement cell block, if any.
		//public void SetCellValueInternal(WorksheetRow row, short columnIndex, object newValueInternal, bool checkForBlockingValues)
		public void SetCellValueRaw(WorksheetRow row, short columnIndex, object newValueRaw, bool checkForBlockingValues, out WorksheetCellBlock replacementBlock)
		{
			// MD 2/23/12 - 12.1 - Table Support
			// Moved all code to a new overload.
			this.SetCellValueRaw(row, columnIndex, newValueRaw, checkForBlockingValues, false, out replacementBlock);
		}

		// MD 2/23/12 - 12.1 - Table Support
		// Added a forceSet parameter.
		private void SetCellValueRaw(WorksheetRow row, short columnIndex, object newValueRaw, bool checkForBlockingValues, bool forceSet, out WorksheetCellBlock replacementBlock)
		{
			// MD 3/4/12 - 12.1 - Table Support
			// This was moved to SetCellValueRawDirect
			#region Moved

			//// MD 1/31/12 - TFS100573
			//// We may need to replace this block if it is full.
			//if (newValueRaw == null)
			//{
			//    replacementBlock = null;
			//}
			//else if (this.ConvertToLargerBlockIfFull(out replacementBlock))
			//{
			//    row.ReplaceCellBlock(columnIndex, replacementBlock);

			//    WorksheetCellBlock temp;
			//    replacementBlock.SetCellValueRaw(row, columnIndex, newValueRaw, checkForBlockingValues, out temp);
			//    Debug.Assert(temp == null, "The replacement block should not also have been replaced.");
			//    return;
			//}

			#endregion // Moved
			replacementBlock = null;

			DataType oldType;
			CellValue oldCellValue;

			// MD 2/1/12 - TFS100573
			// Pass in False for the isForSaving parameter.
			//object oldValueInternal = this.GetCellValueInternal(row, columnIndex, out oldType, out oldCellValue);
			object oldValueRaw = this.GetCellValueRaw(row, columnIndex, false, out oldType, out oldCellValue);

			// MD 2/23/12 - 12.1 - Table Support
			if (forceSet == false)
			{
				// MD 7/21/11 - TFS82017
				// FormattedString instances are now just a wrapper around the element and two different FormattedString instances can point
				// to the same element. In if that is the case, the == check above will fail. So check for element equality. This also is a 
				// slight performance improvement when the same primitive type is set on the cell since previously, we would have been checking 
				// for the equality of two different boxed values, which would have failed. Now that we are doing an Equals call, the values
				// being boxed will be compared as well.
				//if (newValueInternal == oldValueInternal)
				if (Object.Equals(newValueRaw, oldValueRaw))
					return;

				// MD 2/23/12
				// Found while implementing 12.1 - Table Support
				// We could be setting the value to a string which is already set on the cell as a StringElement.
				if (newValueRaw is string)
				{
					StringElement stringElement = oldValueRaw as StringElement;
					if (stringElement != null &&
						stringElement.HasFormatting == false &&
						stringElement.UnformattedString == (string)newValueRaw)
					{
						return;
					}
				}
			}

			WorksheetMergedCellsRegion mergedCellRegion = row.GetCellAssociatedMergedCellsRegionInternal(columnIndex);
			if (mergedCellRegion != null &&
				(mergedCellRegion.FirstRow != row.Index || mergedCellRegion.FirstColumn != columnIndex))
			{
				return;
			}

			// MD 2/8/12 - 12.1 - Table Support
			// Coerce the value is the cell only allows a certain type of value.
			WorksheetTable table;
			ValueCoercionType valueCoercionType = this.GetValueCoercionType(row, columnIndex, out table);

			if (valueCoercionType != ValueCoercionType.None)
				newValueRaw = this.CoerceValue(row, columnIndex, newValueRaw, valueCoercionType);

			// Make sure the new value is valid for the cell
			this.VerifyNewCellValue(row, columnIndex, oldValueRaw, newValueRaw, checkForBlockingValues);

			// MD 3/4/12 - 12.1 - Table Support
			// This was moved to SetCellValueRawDirect
			#region Moved

			//// MD 2/3/12 - TFS100573
			//// Wrapped this code in an if statement for a slight improvement on cells which are just getting their first value.
			//if (oldValueRaw != null)
			//{
			//    this.ClearValueObject(row, columnIndex, oldType, oldCellValue);

			//    // If the current value in the cell is an owned value, let it know no cell owns it anymore
			//    IWorksheetCellOwnedValue oldOwnedValue = oldValueRaw as IWorksheetCellOwnedValue;
			//    if (oldOwnedValue != null)
			//    {
			//        if (oldOwnedValue.IsOwnedByAllCellsAppliedTo)
			//            oldOwnedValue.SetOwningCell(null, -1);
			//    }
			//    else
			//    {
			//        // If the current value is in the shared string table, remove it.
			//        StringElement oldFormattedStringElement = oldValueRaw as StringElement;
			//        if (oldFormattedStringElement == null)
			//        {
			//            FormattedStringValueReference oldFormattedStringValueReference = oldValueRaw as FormattedStringValueReference;
			//            if (oldFormattedStringValueReference != null)
			//                oldFormattedStringElement = oldFormattedStringValueReference.Element;
			//        }

			//        if (oldFormattedStringElement != null)
			//            GenericCacheElement.Release(oldFormattedStringElement, row.SharedStringTable);
			//    }
			//}

			//DataType type;
			//CellValue cellValue;
			//this.SetCellValueRawHelper(row, columnIndex, newValueRaw, out type, out cellValue);

			//// MD 1/31/12 - TFS100573
			//// This method now takes and isNull parameters.
			////this.SetCellValue(columnIndex, cellValue);
			//this.SetCellValue(columnIndex, cellValue, type == DataType.Null);

			//this.SetDataType(columnIndex, type);

			//// MD 4/19/11 - TFS73111
			//// When the cell value changes to or from null, the count of cells with data may change by one, so dirty the count.
			//if (oldType == DataType.Null || type == DataType.Null)
			//    row.Cells.DirtyCellCount();

			//// If either the old value or the new value is a string, text might be wrapped and so the changing value might affect the row height, 
			//// so clear the cached row height so it is recalculated.
			//if (WorksheetCellBlock.IsCellValueSavedAsString(oldType, oldCellValue) ||
			//    WorksheetCellBlock.IsCellValueSavedAsString(type, cellValue))
			//{
			//    // MD 2/27/12
			//    // Found while implementing 12.1 - Table Support
			//    // Only reset the cache if we are in a cell with wrapped text.
			//    //row.ResetCachedHeightWithWrappedText();
			//    WorksheetCellFormatData cellFormat = row.Worksheet.GetCellFormatElementReadOnly(row, columnIndex);
			//    if (cellFormat.WrapTextResolved == ExcelDefaultableBoolean.True)
			//        row.ResetCachedHeightWithWrappedText();
			//}

			//// If the new value is an owned value, let it know it is owned by the cell
			//IWorksheetCellOwnedValue ownedValue = newValueRaw as IWorksheetCellOwnedValue;

			//// Only apply the owner here is all cells that get the value applied will actually own the value.
			//if (ownedValue != null && ownedValue.IsOwnedByAllCellsAppliedTo)
			//    ownedValue.SetOwningCell(row, columnIndex);

			#endregion // Moved
			this.SetCellValueRawDirect(row, columnIndex, newValueRaw, oldValueRaw, oldType, oldCellValue, out replacementBlock);

			// MD 4/9/12 - TFS103739
			// Determine if the workbook is currently loading.
			bool isLoading = false;
			Workbook workbook = row.Worksheet.Workbook;
			if (workbook != null)
				isLoading = workbook.IsLoading;

			// MD 4/9/12 - TFS103739
			// If we are loading, don't auto-set anything on the cell format.
			if (isLoading == false)
			{
				// MD 5/10/12 - TFS111420
				// If the value is being set and the column has a cell format, we should force the cell format to get created
				// and initialized so it can be written out with the cell. If we don't, it will just inherit the row format.
				if (newValueRaw != null && row.Worksheet.GetColumnBlock(columnIndex).CellFormat.IsEmpty == false)
					row.GetCellFormatInternal(columnIndex);

				if (WorksheetCellBlock.ShouldValueSetDefaultFormatString(newValueRaw))
				{
					WorksheetCellFormatProxy cellFormat;
					if (WorksheetCellBlock.ShouldModifyFormatString(row, columnIndex, out cellFormat))
					{
						// Setting a DateTime on the cell should automatically change the format string
						if (newValueRaw is DateTime)
						{
							DateTime dt = (DateTime)newValueRaw;

							// The format should always use the date format
							string formatString = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

							// If there is a specific time in the DateTime, append the time format as well
							if (dt.TimeOfDay.TotalHours != 0)
								formatString += " " + DateTimeFormatInfo.CurrentInfo.ShortTimePattern.Replace("tt", "AM/PM");

							if (cellFormat == null)
								cellFormat = row.GetCellFormatInternal(columnIndex);

							cellFormat.FormatString = formatString;
						}
					}
				}

				// MD 4/9/12 - TFS103739
				// If a string value is being set and it has newlines, initialize the CellFormat.WrapText value to True.
				// Because newlines are not rendered in cells unless CellFormat.WrapText is True.
				if (newValueRaw is string || newValueRaw is FormattedString || newValueRaw is StringElement)
				{
					string stringValue = newValueRaw.ToString();
					if (stringValue.IndexOf('\n') >= 0)
					{
						WorksheetCellFormatProxy cellFormat = row.GetCellFormatInternal(columnIndex);
						if (cellFormat.WrapText == ExcelDefaultableBoolean.Default)
							cellFormat.WrapText = ExcelDefaultableBoolean.True;
					}
				}
			}

			// MD 3/4/12 - 12.1 - Table Support
			// This was moved to SetCellValueRawDirect
			//row.SetFormulaOnCalcReference(columnIndex, newValueRaw, true);
			//row.DirtyCellCalcReference(columnIndex);

			// MD 2/23/12 - 12.1 - Table Support
			if (table != null &&
				valueCoercionType == ValueCoercionType.TableHeaderCell)
			{
				table.AssignUniqueColumnNames();
			}
		}

		public void SetCellValueRawDirect(WorksheetRow row, short columnIndex, object newValueRaw, object oldValueRaw, DataType oldType, CellValue oldCellValue, out WorksheetCellBlock replacementBlock)
		{
			this.SetCellValueRawDirect(row, columnIndex, newValueRaw, oldValueRaw, oldType, oldCellValue, false, out replacementBlock);
		}

		
		public void SetCellValueRawDirect(WorksheetRow row, short columnIndex, object newValueRaw, object oldValueRaw, DataType oldType, CellValue oldCellValue, bool isShiftingCells, out WorksheetCellBlock replacementBlock)
		{
			if (newValueRaw == null)
			{
				replacementBlock = null;
			}
			else if (this.ConvertToLargerBlockIfFull(out replacementBlock))
			{
				row.ReplaceCellBlock(columnIndex, replacementBlock);

				WorksheetCellBlock temp;
				replacementBlock.SetCellValueRawDirect(row, columnIndex, newValueRaw, oldValueRaw, oldType, oldCellValue, isShiftingCells, out temp);
				Debug.Assert(temp == null, "The replacement block should not also have been replaced.");
				return;
			}

			// MD 2/3/12 - TFS100573
			// Wrapped this code in an if statement for a slight improvement on cells which are just getting their first value.
			if (oldValueRaw != null)
			{
				this.ClearValueObject(row, columnIndex, oldType, oldCellValue);

				// If the current value in the cell is an owned value, let it know no cell owns it anymore
				IWorksheetCellOwnedValue oldOwnedValue = oldValueRaw as IWorksheetCellOwnedValue;
				if (oldOwnedValue != null)
				{
					if (isShiftingCells == false && oldOwnedValue.IsOwnedByAllCellsAppliedTo)
						oldOwnedValue.SetOwningCell(null, -1);
				}
				else
				{
					// If the current value is in the shared string table, remove it.
					StringElement oldFormattedStringElement = oldValueRaw as StringElement;
					if (oldFormattedStringElement == null)
					{
						FormattedStringValueReference oldFormattedStringValueReference = oldValueRaw as FormattedStringValueReference;
						if (oldFormattedStringValueReference != null)
							oldFormattedStringElement = oldFormattedStringValueReference.Element;
					}

					if (oldFormattedStringElement != null)
						GenericCacheElement.Release(oldFormattedStringElement, row.SharedStringTable);
				}
			}

			DataType type;
			CellValue cellValue;
			this.SetCellValueRawHelper(row, columnIndex, newValueRaw, out type, out cellValue);

			// MD 1/31/12 - TFS100573
			// This method now takes and isNull parameters.
			//this.SetCellValue(columnIndex, cellValue);
			this.SetCellValue(columnIndex, cellValue, type == DataType.Null);

			this.SetDataType(columnIndex, type);

			// MD 4/19/11 - TFS73111
			// When the cell value changes to or from null, the count of cells with data may change by one, so dirty the count.
			if (oldType == DataType.Null || type == DataType.Null)
				row.Cells.DirtyCellCount();

			// If either the old value or the new value is a string, text might be wrapped and so the changing value might affect the row height, 
			// so clear the cached row height so it is recalculated.
			if (WorksheetCellBlock.IsCellValueSavedAsString(oldType, oldCellValue) ||
				WorksheetCellBlock.IsCellValueSavedAsString(type, cellValue))
			{
				// MD 2/27/12
				// Found while implementing 12.1 - Table Support
				// Only reset the cache if we are in a cell with wrapped text.
				//row.ResetCachedHeightWithWrappedText();
				WorksheetCellFormatData cellFormat = row.Worksheet.GetCellFormatElementReadOnly(row, columnIndex);
				if (cellFormat.WrapTextResolved == ExcelDefaultableBoolean.True)
					row.ResetCachedHeightWithWrappedText();
			}

			// If the new value is an owned value, let it know it is owned by the cell
			IWorksheetCellOwnedValue ownedValue = newValueRaw as IWorksheetCellOwnedValue;

			// Only apply the owner here is all cells that get the value applied will actually own the value.
			if (ownedValue != null && ownedValue.IsOwnedByAllCellsAppliedTo)
				ownedValue.SetOwningCell(row, columnIndex);

			// MD 3/12/12 - 12.1 - Table Support
			// When shifting the data table interior cells, prevent the formulas from being regenerated until the 
			// entire table is moved.
			if (isShiftingCells && newValueRaw is WorksheetDataTable)
				return;

			WorksheetCellBlock.RegenerateFormulas(row, columnIndex, newValueRaw);
		}

		private void SetCellValueRawHelper(WorksheetRow row, short columnIndex, object valueRaw, out DataType type, out CellValue cellValue)
		{
			type = DataType.Null;
			cellValue = new CellValue();

			if (valueRaw == null)
				return;

			#region Commonly Used Types

			if (valueRaw is double)
			{
				type = DataType.Double;
				cellValue.DoubleValue = (double)valueRaw;
				return;
			}

			Workbook workbook = row.Worksheet.Workbook;

			StringElement element;
			string stringValue = valueRaw as string;
			if (stringValue != null)
			{
				// MD 1/6/12 - TFS98536
				// Fix up any invalid characters in the string.
				stringValue = Utilities.FixupCellString(stringValue);

				type = DataType.String;

				// MD 2/2/12 - TFS100573
				// The string element no longer has a reference to the Workbook.
				//element = new StringElement(workbook, stringValue);
				element = new StringElement(stringValue);

				// MD 12/21/11 - 12.1 - Table Support
				//GenericCachedCollection<StringElement> sharedStringTable = row.SharedStringTable;
				GenericCachedCollection<StringElement> sharedStringTable = workbook == null ? null : workbook.SharedStringTable;

				if (sharedStringTable == null)
				{
					this.SetValueObject(row, columnIndex, element);
				}
				else
				{
					element = GenericCacheElement.FindExistingOrAddToCache(element, sharedStringTable);

					// MD 5/31/11 - TFS75574
					// We now store two pieces of information on the CellValue for string because we eliminated the lookup table for keys. Now we 
					// will get the bucket in the generic collection with the hash code and loop through the items linearly looking for the key.
					//cellValue.UInt64Value = element.Key;
					cellValue.SetFormattedStringInfo(element);
				}

				return;
			}

			element = valueRaw as StringElement;
			if (element != null)
			{
				type = DataType.String;
				if (workbook == null)
				{
					if (element.ReferenceCount > 0)
					{
						// MD 2/2/12 - TFS100573
						//element = (StringElement)element.Clone();
						element = (StringElement)element.Clone(null);
					}

					this.SetValueObject(row, columnIndex, element);
				}
				else
				{
					element = GenericCacheElement.FindExistingOrAddToCache(element, workbook.SharedStringTable);

					// MD 5/31/11 - TFS75574
					// We now store two pieces of information on the CellValue for string because we eliminated the lookup table for keys. Now we 
					// will get the bucket in the generic collection with the hash code and loop through the items linearly looking for the key.
					//cellValue.UInt64Value = element.Key;
					cellValue.SetFormattedStringInfo(element);
				}
				return;
			}

			FormattedString formattedStringValue = valueRaw as FormattedString;
			if (formattedStringValue != null)
			{
				type = DataType.FormattedString;
				if (workbook == null)
				{
					this.SetValueObject(row, columnIndex, formattedStringValue.Element);
				}
				else
				{
					formattedStringValue.OnRooted(workbook.SharedStringTable);

					// MD 5/31/11 - TFS75574
					// We now store two pieces of information on the CellValue for string because we eliminated the lookup table for keys. Now we 
					// will get the bucket in the generic collection with the hash code and loop through the items linearly looking for the key.
					//cellValue.UInt64Value = formattedStringValue.Element.Key;
					cellValue.SetFormattedStringInfo(formattedStringValue.Element);
				}
				return;
			}

			Formula formulaValue = valueRaw as Formula;
			if (formulaValue != null)
			{
				type = DataType.Encoded;

				ArrayFormula arrayFormula = formulaValue as ArrayFormula;
				if (arrayFormula != null)
				{
					WorksheetRegion cellRange = arrayFormula.CellRange;

					// MD 2/29/12 - 12.1 - Table Support
					// The worksheet can now be null.
					if (cellRange.Worksheet == null)
					{
						Utilities.DebugFail("This is unexpected");
						return;
					}

					if (cellRange.TopRow != row || cellRange.FirstColumnInternal != columnIndex)
					{
						cellValue.DataTypeEncoded = DataTypeEncoded.ArrayFormulaReference;
						cellValue.RowIndex = cellRange.FirstRow;
						cellValue.ColumnIndex = cellRange.FirstColumnInternal;
						return;
					}
				}

				cellValue.DataTypeEncoded = DataTypeEncoded.Formula;
				int index = row.FindFormulaIndex(columnIndex);
				Debug.Assert(index < 0, "The formula should not be in the CellOwnedFormulas collection yet.");
				if (index < 0)
					row.CellOwnedFormulas.Insert(~index, formulaValue);

				return;
			}

			if (valueRaw is int)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Int32;
				cellValue.EncodedUIntValue = (uint)(int)valueRaw;
				return;
			}

			if (valueRaw is bool)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Boolean;
				cellValue.EncodedUIntValue = (bool)valueRaw ? 1U : 0U;
				return;
			}

			ErrorValue errorValue = valueRaw as ErrorValue;
			if (errorValue != null)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.ErrorValue;
				cellValue.EncodedUIntValue = errorValue.Value;
				return;
			}

			if (valueRaw is DateTime)
			{
				DateTime dateTime = (DateTime)valueRaw;

				double? excelDate = ExcelCalcValue.DateTimeToExcelDate(workbook, dateTime);
				if (excelDate.HasValue)
				{
					type = DataType.DateTime;
					cellValue.UInt64Value = (ulong)(dateTime.Ticks | (((long)dateTime.Kind) << 0x3E));
				}
				else
				{
					type = DataType.Encoded;
					cellValue.DataTypeEncoded = DataTypeEncoded.DateTimeNotConvertible;
					this.SetValueObject(row, columnIndex, new FormattedStringValueReference(valueRaw, workbook));
				}

				return;
			}

			WorksheetDataTable worksheetDataTableValue = valueRaw as WorksheetDataTable;
			if (worksheetDataTableValue != null)
			{
				type = DataType.Encoded;

				WorksheetRegion interiorCells = worksheetDataTableValue.InteriorCells;

				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				if (interiorCells == null || interiorCells.Worksheet == null)
				{
					Utilities.DebugFail("This is unexpected");
					return;
				}

				if (interiorCells.TopRow == row && interiorCells.FirstColumnInternal == columnIndex)
				{
					cellValue.DataTypeEncoded = DataTypeEncoded.WorksheetDataTable;
					this.SetValueObject(row, columnIndex, worksheetDataTableValue);
				}
				else
				{
					cellValue.DataTypeEncoded = DataTypeEncoded.WorksheetDataTableReference;
					cellValue.RowIndex = interiorCells.FirstRow;
					cellValue.ColumnIndex = interiorCells.FirstColumnInternal;
				}
				return;
			}

			if (valueRaw is float)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Single;
				cellValue.EncodedSingleValue = (float)valueRaw;
				return;
			}

			if (valueRaw is long)
			{
				type = DataType.Int64;
				cellValue.UInt64Value = (ulong)(long)valueRaw;
				return;
			}

			#endregion  // Commonly Used Types

			#region Lesser Used Types

			if (valueRaw is byte)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Byte;
				cellValue.EncodedUIntValue = (byte)valueRaw;
				return;
			}

			if (valueRaw is sbyte)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.SByte;
				cellValue.EncodedUIntValue = (byte)(sbyte)valueRaw;
				return;
			}

			if (valueRaw is short)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Int16;
				cellValue.EncodedUIntValue = (ushort)(short)valueRaw;
				return;
			}

			if (valueRaw is ushort)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.UInt16;
				cellValue.EncodedUIntValue = (ushort)valueRaw;
				return;
			}

			if (valueRaw is uint)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.UInt32;
				cellValue.EncodedUIntValue = (uint)valueRaw;
				return;
			}

			if (valueRaw is ulong)
			{
				type = DataType.UInt64;
				cellValue.UInt64Value = (ulong)valueRaw;
				return;
			}

			if (valueRaw is DBNull)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.DBNull;
				return;
			}

			if (valueRaw is char)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Char;
				this.SetValueObject(row, columnIndex, new FormattedStringValueReference(valueRaw, workbook));
				return;
			}

			if (valueRaw is decimal)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Decimal;
				this.SetValueObject(row, columnIndex, valueRaw);
				return;
			}

			if (valueRaw is Guid)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Guid;
				this.SetValueObject(row, columnIndex, new FormattedStringValueReference(valueRaw, workbook));
				return;
			}

			if (valueRaw.GetType().IsEnum)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.Enum;
				this.SetValueObject(row, columnIndex, new FormattedStringValueReference(valueRaw, workbook));
				return;
			}

			StringBuilder stringBuilderValue = valueRaw as StringBuilder;
			if (stringBuilderValue != null)
			{
				type = DataType.Encoded;
				cellValue.DataTypeEncoded = DataTypeEncoded.StringBuilder;
				this.SetValueObject(row, columnIndex, valueRaw);
				return;
			}

			#endregion  // Lesser Used Types

			Utilities.DebugFail("Type not supported: " + valueRaw.GetType().FullName);
		}

		#endregion  // SetCellValueRaw

		#region UpdateFormattedStringElement

		// MD 5/31/11 - TFS75574
		// We need more information than just the key from the element, so just pass the element directly.
		// Also, renamed for clarity.
		//public void SetFormattedStringKey(short columnIndex, uint key)
		public void UpdateFormattedStringElement(short columnIndex, StringElement element)
		{
			Debug.Assert(this.GetDataType(columnIndex) == DataType.FormattedString || this.GetDataType(columnIndex) == DataType.String, 
				"This should only be done for formatted strings");

			CellValue value = new CellValue();

			// MD 5/31/11 - TFS75574
			// We now store two pieces of information on the CellValue for string because we eliminated the lookup table for keys. Now we 
			// will get the bucket in the generic collection with the hash code and loop through the items linearly looking for the key.
			//value.UInt64Value = key;
			value.SetFormattedStringInfo(element);

			// MD 1/31/12 - TFS100573
			//this.SetCellValue(columnIndex, value);
			this.SetCellValue(columnIndex, value, false);
		}

		#endregion  // UpdateFormattedStringElement

		#region VerifyNewCellValue

		public void VerifyNewCellValue(WorksheetRow row, short columnIndex, object oldValueRaw, object newValueRaw, bool checkForBlockingValues)
		{
			// MD 1/25/08
			// Found while fixing BR30021
			// We shouldn't allow strings with lengths over 32767, so verify the string length
			WorksheetCellBlock.VerifyStringLength(newValueRaw);

			if (checkForBlockingValues)
			{
				IWorksheetRegionBlockingValue blockingValue = oldValueRaw as IWorksheetRegionBlockingValue;

				if (blockingValue != null)
				{
					WorksheetRegion blockingRegion = blockingValue.Region;

					if (blockingRegion.FirstRow == blockingRegion.LastRow &&
						blockingRegion.FirstColumn == blockingRegion.LastColumn)
					{
						// If a blocking region value only exists in this cell, just remove it
						blockingValue.RemoveFromRegion();
					}
					else
					{
						// If a blocking region value exists in this cell, but extends to other cells, the value cannot be set on this cell
						blockingValue.ThrowBlockingException();
					}
				}
			}

			// If the new cell value is not a supported type, throw an exception
			if (newValueRaw != null && WorksheetCellBlock.IsCellValueSupported(newValueRaw) == false)
				throw new NotSupportedException(SR.GetString("LE_NotSupportedException_CellType", newValueRaw.GetType()));

			// If the new value is an owned value, verify the cell is a valid owner
			IWorksheetCellOwnedValue ownedValue = newValueRaw as IWorksheetCellOwnedValue;
			if (ownedValue != null)
				ownedValue.VerifyNewOwner(row, columnIndex);
		}

		#endregion VerifyNewCellValue

		#endregion // Public Methods

		#region Private Methods

		#region ClearValueObject

		private void ClearValueObject(WorksheetRow row, short columnIndex, DataType dataType, CellValue cellValue)
		{
			// Only Encoded values are stored in the cell values collection.
			if (dataType != DataType.Encoded)
				return;

			switch (cellValue.DataTypeEncoded)
			{
				case DataTypeEncoded.Formula:
					row.RemoveFormula(columnIndex);
					break;

				// Only these types store the value in the cell values collection
				case DataTypeEncoded.WorksheetDataTable:
				case DataTypeEncoded.Decimal:
				case DataTypeEncoded.StringBuilder:
				case DataTypeEncoded.Char:
				case DataTypeEncoded.DateTimeNotConvertible:
				case DataTypeEncoded.Enum:
				case DataTypeEncoded.Guid:
					// MD 3/27/12 - 12.1 - Table Support
					//row.Worksheet.CellValues.Remove(new WorksheetCellAddress(row, columnIndex));
					row.Worksheet.CellValues.Remove(new WorksheetCellAddress(row.Index, columnIndex));
					break;

				default:
					break;
			}
		}

		#endregion // ClearValueObject

		// MD 2/8/12 - 12.1 - Table Support
		#region CoerceValue

		private object CoerceValue(WorksheetRow row, short columnIndex, object value, ValueCoercionType valueCoercionType)
		{
			if (valueCoercionType == ValueCoercionType.None)
				return value;

			if (value is Formula || value is WorksheetDataTable)
			{
				if (valueCoercionType == ValueCoercionType.TableTotalCell)
					return value;

				if (valueCoercionType == ValueCoercionType.TableHeaderCell)
				{
					value = 0d;
				}
				else
				{
					Utilities.DebugFail("This is unexpected.");
					value = null;
				}
			}

			GetCellTextParameters parameters = new GetCellTextParameters(columnIndex);
			parameters.TextFormatMode = TextFormatMode.IgnoreCellWidth;

			switch (valueCoercionType)
			{
				case ValueCoercionType.TableTotalCell:
					parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.None;
					return WorksheetCellBlock.GetCellTextInternal(row.Worksheet, row, parameters, value);

				case ValueCoercionType.TableHeaderCell:
					// For header cells, we will allow null to be set and we will assign unique column headers after the set occurs.
					if (value == null)
						return null;

					// Otherwise, get the display text of the value, but prevent strings from being formatted.
					// They can just be set directly on the header cells.
					parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.String;
					return WorksheetCellBlock.GetCellTextInternal(row.Worksheet, row, parameters, value);

				default:
					Utilities.DebugFail("Unknown ValueCoercionType: " + valueCoercionType);
					return value;
			}
		}

		#endregion // CoerceValue

		// MD 1/31/12 - TFS100573
		// Made these into abstract methods and moved these implementations down to WorksheetCellBlockFull.
		#region Removed

		//#region GetCellValue

		//private CellValue GetCellValue(short columnIndex)
		//{
		//    switch (columnIndex & 0x1F)
		//    {
		//        case 00: return value00;
		//        case 01: return value01;
		//        case 02: return value02;
		//        case 03: return value03;
		//        case 04: return value04;
		//        case 05: return value05;
		//        case 06: return value06;
		//        case 07: return value07;
		//        case 08: return value08;
		//        case 09: return value09;
		//        case 10: return value10;
		//        case 11: return value11;
		//        case 12: return value12;
		//        case 13: return value13;
		//        case 14: return value14;
		//        case 15: return value15;
		//        case 16: return value16;
		//        case 17: return value17;
		//        case 18: return value18;
		//        case 19: return value19;
		//        case 20: return value20;
		//        case 21: return value21;
		//        case 22: return value22;
		//        case 23: return value23;
		//        case 24: return value24;
		//        case 25: return value25;
		//        case 26: return value26;
		//        case 27: return value27;
		//        case 28: return value28;
		//        case 29: return value29;
		//        case 30: return value30;
		//        case 31: return value31;

		//        default:
		//            Utilities.DebugFail("Wrong offset");
		//            return new CellValue();
		//    }
		//} 

		//#endregion // GetCellValue

		//#region GetDataType

		//private DataType GetDataType(short columnIndex)
		//{
		//    switch (columnIndex & 0x18) // Bits 4,5,6 are the start index of the compressed data type
		//    {
		//        case 00:
		//            return this.dataTypes00Thru07.GetDataType(columnIndex & 0x07);

		//        case 08:
		//            return this.dataTypes08Thru15.GetDataType(columnIndex & 0x07);

		//        case 16:
		//            return this.dataTypes16Thru23.GetDataType(columnIndex & 0x07);

		//        case 24:
		//            return this.dataTypes24Thru31.GetDataType(columnIndex & 0x07);

		//        default:
		//            Utilities.DebugFail("Wrong offset");
		//            return DataType.Null;
		//    }
		//}

		//#endregion // GetDataType

		#endregion // Removed

		#region GetFormattedStringElement

		private StringElement GetFormattedStringElement(WorksheetRow row, short columnIndex, CellValue value)
		{
			Workbook workbook = row.Worksheet.Workbook;
			if (workbook == null)
				return this.GetValueObject(row, columnIndex) as StringElement;

			// MD 5/31/11 - TFS75574
			// The Find method also needs the hash code.
			//return workbook.SharedStringTable.Find((uint)value.UInt64Value);
			return workbook.SharedStringTable.Find(value.FormattedStringHashCodeInCache, value.FormattedStringKey);
		}

		#endregion // GetFormattedStringElement

		// MD 2/1/12 - TFS100573
		#region GetStringElementIndex

		private object GetStringElementIndex(WorksheetRow row, short columnIndex, CellValue value)
		{
			Workbook workbook = row.Worksheet.Workbook;
			if (workbook == null)
			{
				Utilities.DebugFail("This shouldn't have happened.");
			}

			return new StringElementIndex(workbook.SharedStringTable.FindStringIndex(value.FormattedStringHashCodeInCache, value.FormattedStringKey));
		}

		private object GetStringElementIndex(WorksheetRow row, short columnIndex, FormattedStringValueReference valueReference)
		{
			Workbook workbook = row.Worksheet.Workbook;
			if (workbook == null)
			{
				Utilities.DebugFail("This shouldn't have happened.");
			}

			int hashCode = HashHelpers.InternalGetHashCode(valueReference.Element);
			return new StringElementIndex(workbook.SharedStringTable.FindStringIndex(hashCode, valueReference.Element.Key));
		}

		#endregion // GetStringElementIndex

		#region GetValueObject

		private object GetValueObject(WorksheetRow row, short columnIndex)
		{
			if (row.Worksheet.HasCellValues == false)
				return null;

			object value;
			// MD 3/27/12 - 12.1 - Table Support
			//if (row.Worksheet.CellValues.TryGetValue(new WorksheetCellAddress(row, columnIndex), out value))
			if (row.Worksheet.CellValues.TryGetValue(new WorksheetCellAddress(row.Index, columnIndex), out value))
				return value;

			return null;
		}

		#endregion // GetValueObject

		// MD 1/31/12 - TFS100573
		// Made these into abstract methods and moved these implementations down to WorksheetCellBlockFull.
		#region Removed

		//#region SetCellValue

		//private void SetCellValue(short columnIndex, CellValue value)
		//{
		//    switch (columnIndex & 0x1F)
		//    {
		//        case 00:
		//            value00 = value;
		//            return;
		//        case 01:
		//            value01 = value;
		//            return;
		//        case 02:
		//            value02 = value;
		//            return;
		//        case 03:
		//            value03 = value;
		//            return;
		//        case 04:
		//            value04 = value;
		//            return;
		//        case 05:
		//            value05 = value;
		//            return;
		//        case 06:
		//            value06 = value;
		//            return;
		//        case 07:
		//            value07 = value;
		//            return;
		//        case 08:
		//            value08 = value;
		//            return;
		//        case 09:
		//            value09 = value;
		//            return;
		//        case 10:
		//            value10 = value;
		//            return;
		//        case 11:
		//            value11 = value;
		//            return;
		//        case 12:
		//            value12 = value;
		//            return;
		//        case 13:
		//            value13 = value;
		//            return;
		//        case 14:
		//            value14 = value;
		//            return;
		//        case 15:
		//            value15 = value;
		//            return;
		//        case 16:
		//            value16 = value;
		//            return;
		//        case 17:
		//            value17 = value;
		//            return;
		//        case 18:
		//            value18 = value;
		//            return;
		//        case 19:
		//            value19 = value;
		//            return;
		//        case 20:
		//            value20 = value;
		//            return;
		//        case 21:
		//            value21 = value;
		//            return;
		//        case 22:
		//            value22 = value;
		//            return;
		//        case 23:
		//            value23 = value;
		//            return;
		//        case 24:
		//            value24 = value;
		//            return;
		//        case 25:
		//            value25 = value;
		//            return;
		//        case 26:
		//            value26 = value;
		//            return;
		//        case 27:
		//            value27 = value;
		//            return;
		//        case 28:
		//            value28 = value;
		//            return;
		//        case 29:
		//            value29 = value;
		//            return;
		//        case 30:
		//            value30 = value;
		//            return;
		//        case 31:
		//            value31 = value;
		//            return;

		//        default:
		//            Utilities.DebugFail("Wrong offset");
		//            return;
		//    }
		//} 

		//#endregion // SetCellValue

		//#region SetDataType

		//private void SetDataType(short columnIndex, DataType dataType)
		//{
		//    switch (columnIndex & 0x18)  // Bits 4,5,6 are the start index of the compressed data type
		//    {
		//        case 00:
		//            this.dataTypes00Thru07.SetDataType(columnIndex & 0x07, dataType);
		//            break;

		//        case 08:
		//            this.dataTypes08Thru15.SetDataType(columnIndex & 0x07, dataType);
		//            break;

		//        case 16:
		//            this.dataTypes16Thru23.SetDataType(columnIndex & 0x07, dataType);
		//            break;

		//        case 24:
		//            this.dataTypes24Thru31.SetDataType(columnIndex & 0x07, dataType);
		//            break;

		//        default:
		//            Utilities.DebugFail("Wrong offset");
		//            return;
		//    }
		//} 

		//#endregion // SetDataType

		#endregion // Removed

		#region SetValueObject

		private void SetValueObject(WorksheetRow row, short columnIndex, object value)
		{
			// MD 3/27/12 - 12.1 - Table Support
			//row.Worksheet.CellValues[new WorksheetCellAddress(row, columnIndex)] = value;
			row.Worksheet.CellValues[new WorksheetCellAddress(row.Index, columnIndex)] = value;
		}

		#endregion // SetValueObject

		#region ShouldModifyFormatString

		private static bool ShouldModifyFormatString(WorksheetRow row, short columnIndex, out WorksheetCellFormatProxy cellFormat)
		{
			// Only cells whose resolved format is general should have their format changed when the value is set
			// MD 2/25/12 - 12.1 - Table Support
			// This can now be simplified.
			#region Old Code

			//string formatString = null;

			//if (row.HasCellFormatForCellResolved(columnIndex, out cellFormat))
			//    formatString = cellFormat.FormatString;

			//if (WorksheetCellBlock.IsGeneralFormat(formatString) == false)
			//    return false;

			//Worksheet worksheet = row.Worksheet;
			//WorksheetColumn column = worksheet.Columns.GetIfCreated(columnIndex);

			//if (column != null)
			//{
			//    formatString = column.HasCellFormat ? column.CellFormat.FormatString : null;

			//    if (WorksheetCellBlock.IsGeneralFormat(formatString) == false)
			//        return false;
			//}

			//formatString = row.HasCellFormat ? row.CellFormat.FormatString : null;

			//if (WorksheetCellBlock.IsGeneralFormat(formatString) == false)
			//    return false;

			//return true;

			#endregion // Old Code
			if (WorksheetCellBlock.IsGeneralFormat(row.Worksheet.GetCellFormatElementReadOnly(row, columnIndex).FormatStringResolved))
			{
				cellFormat = row.GetCellFormatInternal(columnIndex);
				return true;
			}
			
			cellFormat = null;
			return false;
		}

		#endregion  // ShouldModifyFormatString

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		// MD 1/31/12 - TFS100573
		#region BlockIndex

		public short BlockIndex
		{
			get { return this.blockIndex; }
		}

		#endregion // BlockIndex

		#endregion // Properties


		#region CellValue struct

		[StructLayout(LayoutKind.Explicit)]
		// MD 1/31/12 - TFS100573
		//private struct CellValue
		internal struct CellValue
		{
			// 8-byte types
			[FieldOffset(0)]
			public double DoubleValue;

			[FieldOffset(0)]
			public ulong UInt64Value;

			// Encoded types
			[FieldOffset(0)]
			public DataTypeEncoded DataTypeEncoded;

			[FieldOffset(4)]
			public uint EncodedUIntValue;

			[FieldOffset(4)]
			public float EncodedSingleValue;

			[FieldOffset(4)]
			public uint EncodedCharValue;

			[FieldOffset(2)]
			public int RowIndex;

			[FieldOffset(6)]
			public short ColumnIndex;

			// MD 5/31/11 - TFS75574
			// -----------------------------------------
			// Fields for (formatted) strings
			[FieldOffset(0)]
			public int FormattedStringHashCodeInCache;

			[FieldOffset(4)]
			public uint FormattedStringKey;

			public void SetFormattedStringInfo(StringElement element)
			{
				this.FormattedStringHashCodeInCache = HashHelpers.InternalGetHashCode(element);
				this.FormattedStringKey = element.Key;
			}
			// -------------- TFS75574 ------------------
		}

		#endregion // CellValue struct

		#region DataType enum

		// MD 1/31/12 - TFS100573
		//private enum DataType : byte
		internal enum DataType : byte
		{
			Null = 0,
			Int64 = 1,
			UInt64 = 2,
			Double = 3,
			DateTime = 4,

			// These will be stored in the string table. The value will be the key of the string.
			String = 5,
			FormattedString = 6,

			Encoded = 7,
		}

		#endregion // DataType enum

		#region DataTypeEncoded enum

		// MD 1/31/12 - TFS100573
		//private enum DataTypeEncoded : byte
		internal enum DataTypeEncoded : byte
		{
			Byte = 0,
			SByte = 1,
			Int16 = 2,
			UInt16 = 3,
			Int32 = 4,
			UInt32 = 5,
			Single = 6,
			Boolean = 7,
			DBNull = 8,
			ErrorValue = 9,

			// These will be stored in a dictionary on the row
			Formula = 10,
			ArrayFormulaReference = 11,
			WorksheetDataTable = 12,
			WorksheetDataTableReference = 13,
			Decimal = 14,
			StringBuilder = 15,

			// These will be stored in a dictionary on the row as a FormattedStringValueReference
			Char = 16,
			DateTimeNotConvertible = 17,
			Enum = 18,
			Guid = 19,
		}

		#endregion // DataTypeEncoded enum

		#region DataTypesCompressed struct



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		// MD 1/31/12 - TFS100573
		//private struct DataTypesCompressed
		protected struct DataTypesCompressed
		{
			private byte value1;
			private byte value2;
			private byte value3;

			public DataType GetDataType(int offset)
			{
				switch (offset)
				{
					// Get bits 0-2 of value1
					case 0: return (DataType)((this.value1 & 0xE0) >> 5);

					// Get bits 3-5 of value1
					case 1: return (DataType)((this.value1 & 0x1C) >> 2);

					// Get bits 6 and 7 of value1 or-ed with bit 0 of value2
					case 2: return (DataType)(((this.value1 & 0x03) << 1) | ((this.value2 & 0x80) >> 7));

					// Get bits 1-3 of value2
					case 3: return (DataType)((this.value2 & 0x70) >> 4);

					// Get bits 4-6 of value2
					case 4: return (DataType)((this.value2 & 0x0E) >> 1);

					// Get bit 7 of value2 or-ed with bits 0 and 1 of value3
					case 5: return (DataType)(((this.value2 & 0x01) << 2) | ((this.value3 & 0xC0) >> 6));

					// Get bits 2-4 of value3
					case 6: return (DataType)((this.value3 & 0x38) >> 3);

					// Get bits 5-7 of value3
					case 7: return (DataType)(this.value3 & 0x07);

					default:
						Utilities.DebugFail("Incorrect DataTypesCompressed offset: " + offset);
						return DataType.Null;
				}
			}

			public void SetDataType(int offset, DataType type)
			{
				byte typeData = (byte)type;

				switch (offset)
				{
					// Set bits 0-2 of value1
					case 0:
						this.value1 = (byte)((typeData << 5) | (this.value1 & 0x1F));
						break;

					// Set bits 3-5 of value1
					case 1:
						this.value1 = (byte)((typeData << 2) | (this.value1 & 0xE3));
						break;

					// Set bits 6 and 7 of value1 or-ed with bit 0 of value2
					case 2:
						this.value1 = (byte)((typeData >> 1) | (this.value1 & 0xFC));
						this.value2 = (byte)(((typeData << 7) & 0x80) | (this.value2 & 0x7F));
						break;

					// Set bits 1-3 of value2
					case 3:
						this.value2 = (byte)((typeData << 4) | (this.value2 & 0x8F));
						break;

					// Set bits 4-6 of value2
					case 4:
						this.value2 = (byte)((typeData << 1) | (this.value2 & 0xF1));
						break;

					// Set bit 7 of value2 or-ed with bits 0 and 1 of value3
					case 5:
						this.value2 = (byte)((typeData >> 2) | (this.value2 & 0xFE));
						this.value3 = (byte)(((typeData << 6) & 0xC0) | (this.value3 & 0x3F));
						break;

					// Set bits 2-4 of value3
					case 6:
						this.value3 = (byte)((typeData << 3) | (this.value3 & 0xC7));
						break;

					// Set bits 5-7 of value3
					case 7:
						this.value3 = (byte)(typeData | (this.value3 & 0xF8));
						break;

					default:
						Utilities.DebugFail("Incorrect DataTypesCompressed offset: " + offset);
						break;
				}
			}
		}

		#endregion // DataTypesCompressed struct

		// MD 2/8/12 - 12.1 - Table Support
		#region ValueCoercionType enum

		internal enum ValueCoercionType : byte
		{
			None = 00,
			TableHeaderCell = 0x01,
			TableTotalCell = 0x02,

			// If we have more than 4 values here, the bit flags holding these values needs to be expanded. Right now
			// we are only allocating 2 bits per ValueCoercionType value.
		}

		#endregion // ValueCoercionType enum
	}

	// MD 1/31/12 - TFS100573
	internal class WorksheetCellBlockFull : WorksheetCellBlock
	{
		#region Member Variables

		private DataTypesCompressed dataTypes00Thru07;
		private DataTypesCompressed dataTypes08Thru15;
		private DataTypesCompressed dataTypes16Thru23;
		private DataTypesCompressed dataTypes24Thru31;

		// MD 2/8/12 - 12.1 - Table Support
		private uint isInTableHeaderOrTotalRowFlags;

		private CellValue value00;
		private CellValue value01;
		private CellValue value02;
		private CellValue value03;
		private CellValue value04;
		private CellValue value05;
		private CellValue value06;
		private CellValue value07;
		private CellValue value08;
		private CellValue value09;
		private CellValue value10;
		private CellValue value11;
		private CellValue value12;
		private CellValue value13;
		private CellValue value14;
		private CellValue value15;
		private CellValue value16;
		private CellValue value17;
		private CellValue value18;
		private CellValue value19;
		private CellValue value20;
		private CellValue value21;
		private CellValue value22;
		private CellValue value23;
		private CellValue value24;
		private CellValue value25;
		private CellValue value26;
		private CellValue value27;
		private CellValue value28;
		private CellValue value29;
		private CellValue value30;
		private CellValue value31;

		#endregion // Member Variables

		#region Constructor

		public WorksheetCellBlockFull(short blockIndex)
			: base(blockIndex) { }

		#endregion  // Constructor

		#region Base Class Overrides

		#region ConvertToLargerBlockIfFull

		protected override bool ConvertToLargerBlockIfFull(out WorksheetCellBlock replacementBlock)
		{
			replacementBlock = null;
			return false;
		}

		#endregion // ConvertToLargerBlockIfFull

		#region GetCellValue

		protected override CellValue GetCellValue(short columnIndex)
		{
			switch (columnIndex & 0x1F)
			{
				case 00: return value00;
				case 01: return value01;
				case 02: return value02;
				case 03: return value03;
				case 04: return value04;
				case 05: return value05;
				case 06: return value06;
				case 07: return value07;
				case 08: return value08;
				case 09: return value09;
				case 10: return value10;
				case 11: return value11;
				case 12: return value12;
				case 13: return value13;
				case 14: return value14;
				case 15: return value15;
				case 16: return value16;
				case 17: return value17;
				case 18: return value18;
				case 19: return value19;
				case 20: return value20;
				case 21: return value21;
				case 22: return value22;
				case 23: return value23;
				case 24: return value24;
				case 25: return value25;
				case 26: return value26;
				case 27: return value27;
				case 28: return value28;
				case 29: return value29;
				case 30: return value30;
				case 31: return value31;

				default:
					Utilities.DebugFail("Wrong offset");
					return new CellValue();
			}
		}

		#endregion // GetCellValue

		#region GetDataType

		protected override DataType GetDataType(short columnIndex)
		{
			switch (columnIndex & 0x18) // Bits 4,5,6 are the start index of the compressed data type
			{
				case 00:
					return this.dataTypes00Thru07.GetDataType(columnIndex & 0x07);

				case 08:
					return this.dataTypes08Thru15.GetDataType(columnIndex & 0x07);

				case 16:
					return this.dataTypes16Thru23.GetDataType(columnIndex & 0x07);

				case 24:
					return this.dataTypes24Thru31.GetDataType(columnIndex & 0x07);

				default:
					Utilities.DebugFail("Wrong offset");
					return DataType.Null;
			}
		}

		#endregion // GetDataType

		// MD 2/8/12 - 12.1 - Table Support
		#region IsInTableHeaderOrTotalRow

		protected override bool IsInTableHeaderOrTotalRow(short columnIndex)
		{
			int flagIndex = columnIndex & 0x1F;
			return Utilities.TestBit(this.isInTableHeaderOrTotalRowFlags, flagIndex);
		}

		#endregion // IsInTableHeaderOrTotalRow

		#region SetCellValue

		protected override void SetCellValue(short columnIndex, CellValue value, bool isNull)
		{
			switch (columnIndex & 0x1F)
			{
				case 00:
					value00 = value;
					return;
				case 01:
					value01 = value;
					return;
				case 02:
					value02 = value;
					return;
				case 03:
					value03 = value;
					return;
				case 04:
					value04 = value;
					return;
				case 05:
					value05 = value;
					return;
				case 06:
					value06 = value;
					return;
				case 07:
					value07 = value;
					return;
				case 08:
					value08 = value;
					return;
				case 09:
					value09 = value;
					return;
				case 10:
					value10 = value;
					return;
				case 11:
					value11 = value;
					return;
				case 12:
					value12 = value;
					return;
				case 13:
					value13 = value;
					return;
				case 14:
					value14 = value;
					return;
				case 15:
					value15 = value;
					return;
				case 16:
					value16 = value;
					return;
				case 17:
					value17 = value;
					return;
				case 18:
					value18 = value;
					return;
				case 19:
					value19 = value;
					return;
				case 20:
					value20 = value;
					return;
				case 21:
					value21 = value;
					return;
				case 22:
					value22 = value;
					return;
				case 23:
					value23 = value;
					return;
				case 24:
					value24 = value;
					return;
				case 25:
					value25 = value;
					return;
				case 26:
					value26 = value;
					return;
				case 27:
					value27 = value;
					return;
				case 28:
					value28 = value;
					return;
				case 29:
					value29 = value;
					return;
				case 30:
					value30 = value;
					return;
				case 31:
					value31 = value;
					return;

				default:
					Utilities.DebugFail("Wrong offset");
					return;
			}
		}

		#endregion // SetCellValue

		#region SetDataType

		protected override void SetDataType(short columnIndex, DataType dataType)
		{
			switch (columnIndex & 0x18)  // Bits 4,5,6 are the start index of the compressed data type
			{
				case 00:
					this.dataTypes00Thru07.SetDataType(columnIndex & 0x07, dataType);
					break;

				case 08:
					this.dataTypes08Thru15.SetDataType(columnIndex & 0x07, dataType);
					break;

				case 16:
					this.dataTypes16Thru23.SetDataType(columnIndex & 0x07, dataType);
					break;

				case 24:
					this.dataTypes24Thru31.SetDataType(columnIndex & 0x07, dataType);
					break;

				default:
					Utilities.DebugFail("Wrong offset");
					return;
			}
		}

		#endregion // SetDataType

		// MD 2/8/12 - 12.1 - Table Support
		#region SetIsInTableHeaderOrTotalRow

		protected override void SetIsInTableHeaderOrTotalRow(short columnIndex, bool value)
		{
			int flagIndex = columnIndex & 0x1F;
			Utilities.SetBit(ref this.isInTableHeaderOrTotalRowFlags, value, flagIndex);
		}

		#endregion // SetIsInTableHeaderOrTotalRow

		#endregion // Base Class Overrides
	}

	// MD 1/31/12 - TFS100573
	internal class WorksheetCellBlockHalf : WorksheetCellBlock
	{
		#region Constants

		private const int BlockSize = 16;
		private const int ColumnOffsetSize = 5;

		#endregion // Constants

		#region Member Variables

		private ushort columnOffsets00Thru02;
		private ushort columnOffsets03Thru05;
		private ushort columnOffsets06Thru08;
		private ushort columnOffsets09Thru11;
		private ushort columnOffsets12Thru14;
		private ushort columnOffsets15;

		private DataTypesCompressed dataTypes00Thru07;
		private DataTypesCompressed dataTypes08Thru15;

		// MD 2/8/12 - 12.1 - Table Support
		private ushort isInTableHeaderOrTotalRowFlags;

		private uint usedCells;
		private ushort usedMembers;

		private CellValue value00;
		private CellValue value01;
		private CellValue value02;
		private CellValue value03;
		private CellValue value04;
		private CellValue value05;
		private CellValue value06;
		private CellValue value07;
		private CellValue value08;
		private CellValue value09;
		private CellValue value10;
		private CellValue value11;
		private CellValue value12;
		private CellValue value13;
		private CellValue value14;
		private CellValue value15;

		#endregion // Member Variables

		#region Constructor

		public WorksheetCellBlockHalf(short blockIndex)
			: base(blockIndex) { }

		#endregion  // Constructor

		#region Base Class Overrides

		#region ConvertToLargerBlockIfFull

		protected override bool ConvertToLargerBlockIfFull(out WorksheetCellBlock replacementBlock)
		{
			if (this.usedMembers != UInt16.MaxValue)
			{
				replacementBlock = null;
				return false;
			}

			replacementBlock = new WorksheetCellBlockFull(this.BlockIndex);

			for (int memberIndex = 0; memberIndex < WorksheetCellBlockHalf.BlockSize; memberIndex++)
			{
				short columnOffset = GetColumOffset(memberIndex);
				replacementBlock.SetCellValueAndData(columnOffset, this.GetCellValue(columnOffset), this.GetDataType(columnOffset));
			}

			return true;
		}

		#endregion // ConvertToLargerBlockIfFull

		#region GetCellValue

		protected override CellValue GetCellValue(short columnIndex)
		{
			int memberIndex = this.GetMemberIndex(columnIndex, false);

			if (memberIndex == -1)
				return new CellValue();

			switch (memberIndex)
			{
				case 00: return value00;
				case 01: return value01;
				case 02: return value02;
				case 03: return value03;
				case 04: return value04;
				case 05: return value05;
				case 06: return value06;
				case 07: return value07;
				case 08: return value08;
				case 09: return value09;
				case 10: return value10;
				case 11: return value11;
				case 12: return value12;
				case 13: return value13;
				case 14: return value14;
				case 15: return value15;

				default:
					Utilities.DebugFail("Wrong offset");
					return new CellValue();
			}
		}

		#endregion // GetCellValue

		#region GetDataType

		protected override DataType GetDataType(short columnIndex)
		{
			int memberIndex = this.GetMemberIndex(columnIndex, false);

			if (memberIndex == -1)
				return DataType.Null;

			switch (memberIndex & 0x08) // Bits 4 is the start index of the compressed data type
			{
				case 00:
					return this.dataTypes00Thru07.GetDataType(memberIndex & 0x07);

				case 08:
					return this.dataTypes08Thru15.GetDataType(memberIndex & 0x07);

				default:
					Utilities.DebugFail("Wrong offset");
					return DataType.Null;
			}
		}

		#endregion // GetDataType

		// MD 2/8/12 - 12.1 - Table Support
		#region IsInTableHeaderOrTotalRow

		protected override bool IsInTableHeaderOrTotalRow(short columnIndex)
		{
			int memberIndex = this.GetMemberIndex(columnIndex, false);

			if (memberIndex == -1)
				return false;

			return Utilities.TestBit(this.isInTableHeaderOrTotalRowFlags, memberIndex);
		}

		#endregion // IsInTableHeaderOrTotalRow

		#region SetCellValue

		protected override void SetCellValue(short columnIndex, CellValue value, bool isNull)
		{
			switch (this.GetMemberIndex(columnIndex, isNull == false))
			{
				case 00:
					value00 = value;
					return;
				case 01:
					value01 = value;
					return;
				case 02:
					value02 = value;
					return;
				case 03:
					value03 = value;
					return;
				case 04:
					value04 = value;
					return;
				case 05:
					value05 = value;
					return;
				case 06:
					value06 = value;
					return;
				case 07:
					value07 = value;
					return;
				case 08:
					value08 = value;
					return;
				case 09:
					value09 = value;
					return;
				case 10:
					value10 = value;
					return;
				case 11:
					value11 = value;
					return;
				case 12:
					value12 = value;
					return;
				case 13:
					value13 = value;
					return;
				case 14:
					value14 = value;
					return;
				case 15:
					value15 = value;
					return;

				default:
					Debug.Assert(isNull, "Wrong offset");
					return;
			}
		}

		#endregion // SetCellValue

		#region SetDataType

		protected override void SetDataType(short columnIndex, DataType dataType)
		{
			int memberIndex = this.GetMemberIndex(columnIndex, dataType != DataType.Null);
			if (memberIndex < 0)
				return;

			switch (memberIndex & 0x08)  // Bits 4 is the start index of the compressed data type
			{
				case 00:
					this.dataTypes00Thru07.SetDataType(memberIndex & 0x07, dataType);
					break;

				case 08:
					this.dataTypes08Thru15.SetDataType(memberIndex & 0x07, dataType);
					break;

				default:
					Debug.Assert(dataType == DataType.Null, "Wrong offset");
					return;
			}

			if (dataType == DataType.Null && this.IsInTableHeaderOrTotalRow(columnIndex) == false)
				this.ClearMemberIndex(columnIndex, memberIndex);
		}

		#endregion // SetDataType

		// MD 2/8/12 - 12.1 - Table Support
		#region SetIsInTableHeaderOrTotalRow

		protected override void SetIsInTableHeaderOrTotalRow(short columnIndex, bool value)
		{
			int memberIndex = this.GetMemberIndex(columnIndex, value);
			if (memberIndex < 0)
				return;

			Utilities.SetBit(ref this.isInTableHeaderOrTotalRowFlags, value, memberIndex);

			if (value == false && this.GetDataType(columnIndex) == DataType.Null)
				this.ClearMemberIndex(columnIndex, memberIndex);
		}

		#endregion // SetIsInTableHeaderOrTotalRow

		#endregion // Base Class Overrides

		#region Methods

		#region ClearColumnOffset

		private void ClearColumnOffset(int memberIndex)
		{
			int columnOffsetMemberId = memberIndex / 3;
			int indexOffsetStart = (memberIndex % 3) * WorksheetCellBlockHalf.ColumnOffsetSize;
			int indexOffsertEnd = indexOffsetStart + WorksheetCellBlockHalf.ColumnOffsetSize - 1;

			switch (columnOffsetMemberId)
			{
				case 0:
					Utilities.ClearBits(ref this.columnOffsets00Thru02, indexOffsetStart, indexOffsertEnd);
					break;

				case 1:
					Utilities.ClearBits(ref this.columnOffsets03Thru05, indexOffsetStart, indexOffsertEnd);
					break;

				case 2:
					Utilities.ClearBits(ref this.columnOffsets06Thru08, indexOffsetStart, indexOffsertEnd);
					break;

				case 3:
					Utilities.ClearBits(ref this.columnOffsets09Thru11, indexOffsetStart, indexOffsertEnd);
					break;

				case 4:
					Utilities.ClearBits(ref this.columnOffsets12Thru14, indexOffsetStart, indexOffsertEnd);
					break;

				case 5:
					Utilities.ClearBits(ref this.columnOffsets15, indexOffsetStart, indexOffsertEnd);
					break;

				default:
					Utilities.DebugFail("Something is wrong.");
					return;
			}
		}

		#endregion // ClearColumnOffset

		#region ClearMemberIndex

		private void ClearMemberIndex(short columnIndex, int memberIndex)
		{
			int columnOffset = columnIndex & 0x1F;

			// Mark the members as not being used.
			Utilities.SetBit(ref this.usedMembers, false, memberIndex);
			Utilities.SetBit(ref this.usedCells, false, columnOffset);
			this.ClearColumnOffset(memberIndex);
		}

		#endregion // ClearMemberIndex

		#region GetColumOffset

		private short GetColumOffset(int memberIndex)
		{
			int columnOffsetMemberId = memberIndex / 3;
			int indexOffsetStart = (memberIndex % 3) * WorksheetCellBlockHalf.ColumnOffsetSize;
			int indexOffsertEnd = indexOffsetStart + WorksheetCellBlockHalf.ColumnOffsetSize - 1;

			switch (columnOffsetMemberId)
			{
				case 0:
					return (short)Utilities.GetBits(this.columnOffsets00Thru02, indexOffsetStart, indexOffsertEnd);

				case 1:
					return (short)Utilities.GetBits(this.columnOffsets03Thru05, indexOffsetStart, indexOffsertEnd);

				case 2:
					return (short)Utilities.GetBits(this.columnOffsets06Thru08, indexOffsetStart, indexOffsertEnd);

				case 3:
					return (short)Utilities.GetBits(this.columnOffsets09Thru11, indexOffsetStart, indexOffsertEnd);

				case 4:
					return (short)Utilities.GetBits(this.columnOffsets12Thru14, indexOffsetStart, indexOffsertEnd);

				case 5:
					return (short)Utilities.GetBits(this.columnOffsets15, indexOffsetStart, indexOffsertEnd);

				default:
					Utilities.DebugFail("Something is wrong.");
					return 0;
			}
		}

		#endregion // GetColumOffset

		#region GetMemberIndex

		private int GetMemberIndex(short columnIndex, bool allocateNew)
		{
			short columnOffset = (short)(columnIndex & 0x1F);

			int unusedMemberIndex = -1;

			if (this.usedCells == 0)
			{
				unusedMemberIndex = 0;
			}
			else 
			{
				bool cellExists = Utilities.TestBit(this.usedCells, columnOffset);

				if (cellExists || allocateNew)
				{
					for (int memberIndex = 0; memberIndex < WorksheetCellBlockHalf.BlockSize; memberIndex++)
					{
						bool memberIsUsed = Utilities.TestBit(this.usedMembers, memberIndex);

						if (cellExists)
						{
							if (memberIsUsed && this.GetColumOffset(memberIndex) == columnOffset)
								return memberIndex;
						}
						else if (memberIsUsed == false)
						{
							if (unusedMemberIndex < 0)
								unusedMemberIndex = memberIndex;

							continue;
						}
					}
				}
			}

			if (allocateNew)
			{
				if (unusedMemberIndex < 0)
				{
					Utilities.DebugFail("Cannot find an unused index.");
				}
				else
				{
					// Mark the index as being used and update the saved column offsets.
					Utilities.SetBit(ref this.usedMembers, true, unusedMemberIndex);
					this.SetColumnOffset(unusedMemberIndex, columnOffset);
					return unusedMemberIndex;
				}
			}

			return -1;
		}

		#endregion // GetMemberIndex

		#region SetColumnOffset

		private void SetColumnOffset(int memberIndex, short columnOffset)
		{
			int columnOffsetMemberId = memberIndex / 3;
			int indexOffsetStart = (memberIndex % 3) * WorksheetCellBlockHalf.ColumnOffsetSize;
			int indexOffsertEnd = indexOffsetStart + WorksheetCellBlockHalf.ColumnOffsetSize - 1;

			switch (columnOffsetMemberId)
			{
 				case 0:
					Utilities.AddBits(ref this.columnOffsets00Thru02, columnOffset, indexOffsetStart, indexOffsertEnd);
					break;

				case 1:
					Utilities.AddBits(ref this.columnOffsets03Thru05, columnOffset, indexOffsetStart, indexOffsertEnd);
					break;

				case 2:
					Utilities.AddBits(ref this.columnOffsets06Thru08, columnOffset, indexOffsetStart, indexOffsertEnd);
					break;

				case 3:
					Utilities.AddBits(ref this.columnOffsets09Thru11, columnOffset, indexOffsetStart, indexOffsertEnd);
					break;

				case 4:
					Utilities.AddBits(ref this.columnOffsets12Thru14, columnOffset, indexOffsetStart, indexOffsertEnd);
					break;

				case 5:
					Utilities.AddBits(ref this.columnOffsets15, columnOffset, indexOffsetStart, indexOffsertEnd);
					break;

				default:
					Utilities.DebugFail("Something is wrong.");
					return;
			}

			Utilities.SetBit(ref this.usedCells, true, columnOffset);
		}

		#endregion // SetColumnOffset

		#endregion // Methods
	}


	[Flags]
	internal enum PreventTextFormattingTypes
	{
		None = 0,
		String = 1,
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