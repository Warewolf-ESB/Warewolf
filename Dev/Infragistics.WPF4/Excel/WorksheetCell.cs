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


namespace Infragistics.Documents.Excel
{
	// MD 4/12/11 - TFS67084
	// Refactored this class and moved all logic to the WorksheetRow and WorksheetCellBlock classes.
	// The class is redefined below with all pre-existing public members and a few internal helper member.
	#region Old Code

	
#region Infragistics Source Cleanup (Region)













































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

	
#region Infragistics Source Cleanup (Region)





















































































































































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

	
#region Infragistics Source Cleanup (Region)













































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

	
#region Infragistics Source Cleanup (Region)















































































































































#endregion // Infragistics Source Cleanup (Region)

	
#region Infragistics Source Cleanup (Region)






































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


	#endregion // Old Code
	/// <summary>
	/// Represents a cell in a Microsoft Excel worksheet.
	/// </summary>



	public

		 sealed class WorksheetCell :
		ICellFormatOwner,		// MD 1/8/12 - 12.1 - Cell Format Updates
		IComparable<WorksheetCell>,
		IFormattedStringOwner
	{
		#region Static Members

		internal readonly static WorksheetCell InvalidReference = new WorksheetCell(null, -1);

		#endregion // Static Members

		#region Member Variables

		private WorksheetCellFormatProxy cellFormatProxy;

		// MD 2/29/12 - 12.1 - Table Support
		private int cellShiftHistoryVersion;

		private short columnIndex;
		private WorksheetRow row;

		#endregion Member Variables

		#region Constructor

		internal WorksheetCell(WorksheetRow row, short columnIndex)
		{
			this.columnIndex = columnIndex;
			this.row = row;

			// MD 2/29/12 - 12.1 - Table Support
			if (this.row != null)
				this.cellShiftHistoryVersion = this.row.Worksheet.CellShiftHistoryVersion;
		}

		#endregion Constructor

		#region Interfaces

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region ICellFormatOwner Members

		WorksheetCellFormatProxy ICellFormatOwner.CellFormatInternal
		{
			// MD 2/29/12 - 12.1 - Table Support
			//get { return this.row.GetCellFormatInternal(this.columnIndex); }
			get { return this.CellFormatInternal; }
		}

		bool ICellFormatOwner.HasCellFormat
		{
			get { return this.HasCellFormat; }
		}

		#endregion

		#region IComparable<WorksheetCell> Members

		int IComparable<WorksheetCell>.CompareTo(WorksheetCell other)
		{
			// MD 2/29/12 - 12.1 - Table Support
			if (Object.ReferenceEquals(this, other))
				return 0;

			if (other == null)
				return -1;

			// MD 2/29/12 - 12.1 - Table Support
			//int result = this.row.Index - other.row.Index;
			WorksheetRow row = this.Row;
			WorksheetRow otherRow = other.Row;
			if (row == null && otherRow == null)
				return 0;

			if (row == null)
				return 1;
			else if (otherRow == null)
				return -1;

			int result = row.Index - otherRow.Index;

			if (result != 0)
				return result;

			return this.columnIndex - other.columnIndex;
		}

		#endregion

		#region IFormattedStringOwner Members

		void IFormattedStringOwner.OnUnformattedStringChanged(FormattedString sender) 
		{
			// MD 5/31/11 - TFS75574
			// We need more information than just the key from the element, so just pass the element directly.
			//this.row.UpdateFormattedStringKeyOnCell(this.columnIndex, sender.Element.Key);
			// MD 2/29/12 - 12.1 - Table Support
			//this.row.UpdateFormattedStringElementOnCell(this.columnIndex, sender.Element);
			WorksheetRow row = this.Row;
			if (row != null)
				row.UpdateFormattedStringElementOnCell(this.columnIndex, sender.Element);
		}

		#endregion

		#endregion Interfaces

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the specified object is another <see cref="WorksheetCell"/> instance which refers 
		/// to the same location on the same worksheet as this cell.
		/// </summary>
		/// <param name="obj">The instance to check for equality.</param>
		/// <returns>True if the cells refer to the same location on the same worksheet; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			// MD 2/29/12 - 12.1 - Table Support
			//WorksheetCell otherCell = obj as WorksheetCell;
			//if (otherCell == null)
			//    return false;
			//
			//return
			//    this.row == otherCell.row &&
			//    this.columnIndex == otherCell.columnIndex;
			return ((IComparable<WorksheetCell>)this).CompareTo(obj as WorksheetCell) == 0;
		} 

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code of the <see cref="WorksheetCell"/>.
		/// </summary>
		/// <returns>The hash code of the <see cref="WorksheetCell"/>.</returns>
		public override int GetHashCode()
		{
			// MD 2/29/12 - 12.1 - Table Support
			//return this.row.GetHashCode() ^ this.columnIndex.GetHashCode();
			WorksheetRow row = this.Row;
			if (row == null)
				return 0;

			return row.GetHashCode() ^ this.columnIndex.GetHashCode();
		} 

		#endregion // GetHashCode

		#region ToString

		/// <summary>
		/// Gets the string representation of the address of the cell.
		/// </summary>
		/// <returns>The string representation of the address of the cell.</returns>
		public override string ToString()
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			//return this.ToString(this.Worksheet.CellReferenceMode, true);
			CellReferenceMode cellReferenceMode = CellReferenceMode.A1;

			Worksheet worksheet = this.Worksheet;
			if (worksheet != null)
				cellReferenceMode = worksheet.CellReferenceMode;

			return this.ToString(cellReferenceMode, true);
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Methods

		#region Public Methods

		#region ApplyFormula

		/// <summary>
		/// Applies a formula to the cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// <paramref name="value"/> is parsed based on the <see cref="CellReferenceMode"/> of the <see cref="Workbook"/>
		/// to which the cell belongs. If the cell's <see cref="Worksheet"/> has been removed from its parent collection,
		/// the A1 CellReferenceMode will be used to parse the formula.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.ApplyCellFormula"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this method is equivalent to using the WorksheetRow.ApplyCellFormula method.
		/// </p>
		/// </remarks>
		/// <param name="value">The formula to parse and apply to the cell.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="value"/> is not a valid formula.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The cell is part of an array formula or data table which is not confined to just the cell.
		/// </exception>
		/// <seealso cref="Formula"/>
		/// <seealso cref="WorksheetRow.ApplyCellFormula"/>
		public void ApplyFormula(string value)
		{
			// MD 2/29/12 - 12.1 - Table Support
			//this.row.ApplyCellFormula(this.columnIndex, value);
			WorksheetRow row = this.Row;
			if (row == null)
				throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));

			row.ApplyCellFormula(this.columnIndex, value);
		}

		#endregion ApplyFormula

		#region ClearComment

		/// <summary>
		/// Removes the comment associated with the cell.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.SetCellComment"/> method and pass in null as the 
		/// comment parameter, which does not create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the 
		/// WorksheetCell instance already exists, using this method is equivalent to using the WorksheetRow.SetCellComment method and
		/// passing in null as the comment parameter.
		/// </p>
		/// </remarks>
		/// <seealso cref="Comment"/>
		/// <seealso cref="HasComment"/>
		/// <seealso cref="WorksheetRow.SetCellComment"/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ClearComment()
		{
			// MD 2/29/12 - 12.1 - Table Support
			//this.row.SetCellCommentInternal(this.columnIndex, null);
			WorksheetRow row = this.Row;
			if (row == null)
				throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));

			row.SetCellCommentInternal(this.columnIndex, null);
		}

		#endregion ClearComment

		#region GetBoundsInTwips

		/// <summary>
		/// Gets the bounds of the cell in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The bounds returned by this method are only valid with the current configuration of the worksheet.
		/// If any rows or columns before the cell are resized, these bounds will no longer reflect the 
		/// position of the cell.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellBoundsInTwips(int)"/> method, which 
		/// does not create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this method is equivalent to using the WorksheetRow.GetBoundsInTwips method.
		/// </p>
		/// </remarks>
		/// <returns>The bounds of the cell on its worksheet.</returns>
		/// <seealso cref="WorksheetRow.GetCellBoundsInTwips(int)"/>
		public Rectangle GetBoundsInTwips()
		{
			// MD 2/29/12 - 12.1 - Table Support
			//return this.row.GetCellBoundsInTwipsInternal(this.columnIndex, PositioningOptions.None);
			return this.GetBoundsInTwips(PositioningOptions.None);
		}

		// MD 3/24/10 - TFS28374
		// Added new overload.
		/// <summary>
		/// Gets the bounds of the cell in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The bounds returned by this method are only valid with the current configuration of the worksheet.
		/// If any rows or columns before the cell are resized, these bounds will no longer reflect the 
		/// position of the cell.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellBoundsInTwips(int,PositioningOptions)"/> method, 
		/// which does not create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this method is equivalent to using the WorksheetRow.GetBoundsInTwips method.
		/// </p>
		/// </remarks>
		/// <param name="options">The options to use when getting the bounds of the cell.</param>
		/// <returns>The bounds of the cell on its worksheet.</returns>
		/// <seealso cref="WorksheetRow.GetCellBoundsInTwips(int,PositioningOptions)"/>
		public Rectangle GetBoundsInTwips(PositioningOptions options)
		{
			// MD 2/29/12 - 12.1 - Table Support
			//return this.row.GetCellBoundsInTwipsInternal(this.columnIndex, options);
			WorksheetRow row = this.Row;
			if (row == null)
				throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));

			return row.GetCellBoundsInTwipsInternal(this.columnIndex, options);
		}

		#endregion GetBoundsInTwips

        // MRS 9/20/2011 - TFS85272
        #region GetCellAddressString

        /// <summary>
        /// Gets the string representation of the address of the cell.
        /// </summary>
        /// <param name="worksheetRow">The WorksheetRow of the cell.</param>
        /// <param name="columnIndex">The index of the column of the cell.</param>
        /// <param name="cellReferenceMode">The mode used to generate cell references.</param>
        /// <param name="includeWorksheetName">The value indicating whether to include the worksheet name in the cell address.</param>
        /// <returns>The string representation of the address of the cell.</returns>
        public static string GetCellAddressString(WorksheetRow worksheetRow, int columnIndex, CellReferenceMode cellReferenceMode, bool includeWorksheetName)
        {
            return WorksheetCell.GetCellAddressString(
                worksheetRow, 
                columnIndex, 
                cellReferenceMode, 
                includeWorksheetName, 
                false, //useRelativeColumn, 
                false  //useRelativeRow
                );
        }

        /// <summary>
        /// Gets the string representation of the address of the cell.
        /// </summary>
        /// <param name="worksheetRow">The WorksheetRow of the cell.</param>
        /// <param name="columnIndex">The index of the column of the cell.</param>
        /// <param name="cellReferenceMode">The mode used to generate cell references.</param>
        /// <param name="includeWorksheetName">The value indicating whether to include the worksheet name in the cell address.</param>
        /// <param name="useRelativeColumn">The value indicating whether to use a relative column address.</param>
        /// <param name="useRelativeRow">The value indicating whether to use a relative row address.</param>
        /// <returns>The string representation of the address of the cell.</returns>
        public static string GetCellAddressString(WorksheetRow worksheetRow, int columnIndex, CellReferenceMode cellReferenceMode, bool includeWorksheetName, bool useRelativeColumn, bool useRelativeRow)
        {
            Worksheet workSheet = worksheetRow.Worksheet;
            return (includeWorksheetName ? Utilities.CreateReferenceString(null, workSheet.Name) : string.Empty) +
                CellAddress.GetCellReferenceString(
                    worksheetRow.Index, columnIndex,
                    useRelativeRow, useRelativeColumn,
					// MD 2/20/12 - 12.1 - Table Support
                    //workSheet.CurrentFormat, worksheetRow, (short)columnIndex, false, cellReferenceMode);
					workSheet.CurrentFormat, worksheetRow.Index, (short)columnIndex, false, cellReferenceMode);
        }
        #endregion // GetCellAddressString

		// MD 12/19/11 - 12.1 - Table Support
		#region GetText

		/// <summary>
		/// Gets the text displayed in the cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The display text is based on the value of the cell and the format string applied to the cell.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellText(int)"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this property is equivalent to using the WorksheetRow.GetText method.
		/// </p>
		/// </remarks>
		/// <seealso cref="Value"/>
		/// <seealso cref="IWorksheetCellFormat.FormatString"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public string GetText()
		{
			return this.GetText(TextFormatMode.AsDisplayed);
		}

		/// <summary>
		/// Gets the text of the cell.
		/// </summary>
		/// <param name="textFormatMode">The format mode to use when getting the cell text.</param>
		/// <remarks>
		/// <p class="body">
		/// The text is based on the value of the cell and the format string applied to the cell.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellText(int,TextFormatMode)"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this property is equivalent to using the WorksheetRow.GetText method.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="textFormatMode"/> is not defined in the <see cref="TextFormatMode"/> enumeration.
		/// </exception>
		/// <seealso cref="Value"/>
		/// <seealso cref="IWorksheetCellFormat.FormatString"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public string GetText(TextFormatMode textFormatMode)
		{
			WorksheetRow row = this.Row;
			if (row == null)
				throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));

			return row.GetCellTextInternal(this.columnIndex, textFormatMode);
		}

		#endregion // GetText

		#region GetResolvedCellFormat

		/// <summary>
		/// Gets the resolved cell formatting for this cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If any cell format properties are the default values on the cell, the values from the owning row's cell format will be used.
		/// If those are default, then the values from the owning column's cell format will be used. Otherwise, the workbook default values
		/// will be used.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetResolvedCellFormat"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this method is equivalent to using the WorksheetRow.GetResolvedCellFormat method.
		/// </p>
		/// </remarks>
		/// <returns>A format object describing the actual formatting that will be used when displayed this cell in Microsoft Excel.</returns>
		/// <seealso cref="CellFormat"/>
		/// <seealso cref="RowColumnBase.CellFormat"/>
		/// <seealso cref="WorksheetRow.GetResolvedCellFormat"/>
		public IWorksheetCellFormat GetResolvedCellFormat()
		{
			WorksheetRow row = this.Row;
			if (row == null)
				throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));

			return row.GetResolvedCellFormat(this.columnIndex);
		}

		#endregion // GetResolvedCellFormat

		#region IsCellTypeSupported

		/// <summary>
		/// Returns True if a particular type can be exported to excel.
		/// </summary>
		/// <param name="cellType">The type to test.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="cellType"/> is null.
		/// </exception>
		/// <returns>True if the type is supported as a cell value, False otherwise.</returns>
		public static bool IsCellTypeSupported(Type cellType)
		{
			if (cellType == null)
				throw new ArgumentNullException("cellType");

			if (cellType.IsEnum)
				return true;

			switch (cellType.FullName)
			{
				case "System.Byte":
				case "System.SByte":
				case "System.Int16":
				case "System.Int64":
				case "System.UInt16":
				case "System.UInt64":
				case "System.UInt32":
				case "System.Int32":
				case "System.Single":
				case "System.Double":
				case "System.Boolean":
				case "System.Char":
				case "System.Enum":
				case "System.Decimal":
				case "System.DateTime":
				case "System.String":
				case "System.Text.StringBuilder":
				case "System.DBNull":
				case "System.Guid":					// MD 5/28/10 - TFS31886
				case "Infragistics.Documents.Excel.ErrorValue":
				case "Infragistics.Documents.Excel.FormattedString":
				case "Infragistics.Documents.Excel.Formula":
				case "Infragistics.Documents.Excel.ArrayFormula":
				case "Infragistics.Documents.Excel.WorksheetDataTable":
					return true;

				default:
					return false;
			}
		}

		#endregion IsCellTypeSupported

		#region ToString( CellReferenceMode, bool )

		/// <summary>
		/// Gets the string representation of the address of the cell.
		/// </summary>
		/// <param name="cellReferenceMode">The mode used to generate cell references.</param>
		/// <param name="includeWorksheetName">The value indicating whether to include the worksheet name in the cell address.</param>
		/// <returns>The string representation of the address of the cell.</returns>
		public string ToString(CellReferenceMode cellReferenceMode, bool includeWorksheetName)
		{
			// MD 2/29/12 - 12.1 - Table Support
			//return this.row.GetCellAddressString(this.columnIndex, cellReferenceMode, includeWorksheetName);
			WorksheetRow row = this.Row;
			if (row == null)
				return ErrorValue.InvalidCellReference.ToString();

			return row.GetCellAddressString(this.columnIndex, cellReferenceMode, includeWorksheetName);
		}

		#endregion ToString( CellReferenceMode, bool )

		#region ToString( CellReferenceMode, bool, bool, bool )

		/// <summary>
		/// Gets the string representation of the address of the cell.
		/// </summary>
		/// <param name="cellReferenceMode">The mode used to generate cell references.</param>
		/// <param name="includeWorksheetName">The value indicating whether to include the worksheet name in the cell address.</param>
		/// <param name="useRelativeColumn">The value indicating whether to use a relative column address.</param>
		/// <param name="useRelativeRow">The value indicating whether to use a relative row address.</param>
		/// <returns>The string representation of the address of the cell.</returns>
		public string ToString(CellReferenceMode cellReferenceMode, bool includeWorksheetName, bool useRelativeColumn, bool useRelativeRow)
		{
			// MD 2/29/12 - 12.1 - Table Support
			//return this.row.GetCellAddressString(this.columnIndex, cellReferenceMode, includeWorksheetName, useRelativeColumn, useRelativeRow);
			WorksheetRow row = this.Row;
			if (row == null)
				return ErrorValue.InvalidCellReference.ToString();

			return row.GetCellAddressString(this.columnIndex, cellReferenceMode, includeWorksheetName, useRelativeColumn, useRelativeRow);
		}

		#endregion ToString( CellReferenceMode, bool, bool, bool )

		#endregion Public Methods

		#region Internal Methods

		// MD 5/13/11 - Data Validations / Page Breaks
		#region GetCachedRegion

		internal WorksheetRegion GetCachedRegion()
		{
			// MD 2/29/12 - 12.1 - Table Support
			//return this.Worksheet.GetCachedRegion(this.row.Index, this.columnIndex, this.row.Index, this.columnIndex);
			WorksheetRow row = this.Row;
			if (row == null || row.Worksheet == null)
				return null;

			return row.Worksheet.GetCachedRegion(row.Index, this.columnIndex, row.Index, this.columnIndex);
		}

		#endregion  // GetCachedRegion

		#endregion  // Internal Methods

		#region Private Methods

		// MD 2/29/12 - 12.1 - Table Support
		#region OnShiftedOffWorksheet

		private void OnShiftedOffWorksheet()
		{
			this.row = null;
			this.columnIndex = -1;
		}

		#endregion // OnShiftedOffWorksheet

		// MD 2/29/12 - 12.1 - Table Support
		#region VerifyCellAddress

		private void VerifyCellAddress()
		{
			if (this.row == null)
				return;

			Worksheet worksheet = this.row.Worksheet;
			if (worksheet == null || worksheet.VerifyCellAddress(ref this.row, ref this.columnIndex, ref this.cellShiftHistoryVersion) == false)
				this.OnShiftedOffWorksheet();
		}

		#endregion // VerifyCellAddress

		#endregion // Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region AssociatedDataTable

		/// <summary>
		/// Gets the data table to which the cell belongs.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The cells in the left-most column and top-most row of the data table will return null for the associated data table.
		/// </p>
		/// <p class="body">
		/// If a data table is associated with the cell, getting the <see cref="Value"/> will return the calculated value for the cell.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellAssociatedDataTable"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this property is equivalent to using the WorksheetRow.GetCellAssociatedDataTable method.
		/// </p>
		/// </remarks>
		/// <value>The data table to which the cell belongs or null if the cell does not belong to a data table.</value>
		/// <seealso cref="Excel.Worksheet.DataTables"/>
		/// <seealso cref="WorksheetDataTableCollection.Add"/>
		/// <seealso cref="WorksheetRow.GetCellAssociatedDataTable"/>
		public WorksheetDataTable AssociatedDataTable
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				//return this.row.GetCellAssociatedDataTable(this.columnIndex); 
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				return row.GetCellAssociatedDataTable(this.columnIndex);
			}
		}

		#endregion AssociatedDataTable

		#region AssociatedMergedCellsRegion

		/// <summary>
		/// Gets the merged cells region which contains the cell, or null if the cell is not merged.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellAssociatedMergedCellsRegion"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this property is equivalent to using the WorksheetRow.GetCellAssociatedMergedCellsRegion method.
		/// </p>
		/// </remarks>
		/// <value>The merged cells region which contains the cell, or null if the cell is not merged.</value>
		/// <seealso cref="WorksheetRow.GetCellAssociatedMergedCellsRegion"/>
		public WorksheetMergedCellsRegion AssociatedMergedCellsRegion
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				//return this.row.GetCellAssociatedMergedCellsRegionInternal(this.columnIndex); 
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				return row.GetCellAssociatedMergedCellsRegionInternal(this.columnIndex);
			}
		}

		#endregion AssociatedMergedCellsRegion

		// MD 12/14/11 - 12.1 - Table Support
		#region AssociatedTable

		/// <summary>
		/// Gets the <see cref="WorksheetTable"/> to which this cell belongs.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A cell belongs to a table if it exists in any area of the table. It can be a header cell, total cell, or a cell in the data area.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellAssociatedTable"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this property is equivalent to using the WorksheetRow.GetCellAssociatedTable method.
		/// </p>
		/// </remarks>
		/// <seealso cref="WorksheetTable"/>
		/// <seealso cref="Excel.Worksheet.Tables"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public WorksheetTable AssociatedTable
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				//return this.row.GetCellAssociatedTableInternal(this.columnIndex); 
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				return row.GetCellAssociatedTableInternal(this.columnIndex); 
			}
		}

		#endregion // AssociatedTable

		#region CellFormat

		/// <summary>
		/// Gets the cell formatting for this cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Use this property to set cell formatting specific to the cell. If you will be applying the format to numerous cells, 
		/// see the <see cref="Workbook.CreateNewWorksheetCellFormat"/> method for performance considerations.
		/// </p>
		/// <p class="body">
		/// If this cell belongs to a merged cell region, getting the CellFormat will get the CellFormat of the associated merged 
		/// cell region.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellFormat"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this property is equivalent to using the WorksheetRow.GetCellFormat method.
		/// </p>
		/// </remarks>
		/// <value>The cell formatting for this cell.</value>
		/// <seealso cref="GetResolvedCellFormat"/>
		/// <seealso cref="WorksheetRow.GetCellFormat"/>
		public IWorksheetCellFormat CellFormat
		{
			// MD 2/29/12 - 12.1 - Table Support
			// Moved this code to the CellFormatInternal property.
			get { return this.CellFormatInternal; }
		}

		// MD 2/29/12 - 12.1 - Table Support
		internal WorksheetCellFormatProxy CellFormatInternal
		{
			get
			{
				// MD 2/29/12 - 12.1 - Table Support
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				if (this.cellFormatProxy == null)
				{
					// MD 2/29/12 - 12.1 - Table Support
					//this.cellFormatProxy = this.row.GetCellFormatInternal(this.columnIndex);
					this.cellFormatProxy = row.GetCellFormatInternal(this.columnIndex);
				}

				return this.cellFormatProxy;
			}
		}

		// MD 5/31/11 - TFS75574
		/// <summary>
		/// Gets the value which indicates whether the cell's format has been initialized yet.
		/// </summary>
		/// <seealso cref="CellFormat"/>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool HasCellFormat
		{
			get
			{
				// MD 2/29/12 - 12.1 - Table Support
				//WorksheetCellFormatData tmp;
				//return this.row.TryGetCellFormat(this.columnIndex, out tmp);
				WorksheetRow row = this.Row;
				if (row == null)
					return false;

				WorksheetCellFormatData tmp;
				return row.TryGetCellFormat(this.columnIndex, out tmp);
			}
		}

		#endregion CellFormat

		#region ColumnIndex

		/// <summary>
		/// Gets the column index of the cell.
		/// </summary>
		/// <value>The column index of the cell.</value>
		public int ColumnIndex
		{
			// MD 2/29/12 - 12.1 - Table Support
			//get { return this.columnIndex; }
			get { return this.ColumnIndexInternal; }
		}

		internal short ColumnIndexInternal
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				this.VerifyCellAddress();

				return this.columnIndex; 
			}
		}

		#endregion ColumnIndex

		#region Comment

		/// <summary>
		/// Gets or sets the comment applied to the cell.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellComment"/> or 
		/// <see cref="WorksheetRow.SetCellComment"/> methods, which do not create <see cref="WorksheetCell"/> instances internally. However, 
		/// if a reference to the WorksheetCell instance already exists, using this property is equivalent to using the 
		/// WorksheetRow.GetCellComment or WorksheetRow.SetCellComment methods.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The value applied only has only one anchor cell set. It should have both or neither anchor cells set.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value has its <see cref="WorksheetShape.TopLeftCornerCell"/> and <see cref="WorksheetShape.BottomRightCornerCell"/> 
		/// anchors set but they are from different worksheets.
		/// </exception>
		/// <value>The comment applied to the cell.</value>
		/// <seealso cref="WorksheetRow.GetCellComment"/>
		/// <seealso cref="WorksheetRow.SetCellComment"/>
		public WorksheetCellComment Comment
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				//return this.row.GetCellCommentInternal(this.columnIndex); 
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				return row.GetCellCommentInternal(this.columnIndex); 
			}
			set 
			{
				// MD 2/29/12 - 12.1 - Table Support
				//this.row.SetCellCommentInternal(this.columnIndex, value); 
				WorksheetRow row = this.Row;
				if (row == null)
					throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));

				row.SetCellCommentInternal(this.columnIndex, value); 
			}
		}

		#endregion Comment

		// MD 5/13/11 - Data Validations / Page Breaks
		#region DataValidationRule

		/// <summary>
		/// Gets or sets the data validation rule for the <see cref="WorksheetCell"/>.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Occurs when the value specified is already applied to cells in another worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when the value specified is invalid based on its rule type's requirements.
		/// </exception>
		/// <seealso cref="Infragistics.Documents.Excel.Worksheet.DataValidationRules"/>
		/// <seealso cref="AnyValueDataValidationRule"/>
		/// <seealso cref="ListDataValidationRule"/>
		/// <seealso cref="CustomDataValidationRule"/>
		/// <seealso cref="OneConstraintDataValidationRule"/>
		/// <seealso cref="TwoConstraintDataValidationRule"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public DataValidationRule DataValidationRule
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				//return this.Worksheet.DataValidationRules.FindRule(this); 
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				return row.Worksheet.DataValidationRules.FindRule(this); 
			}
			set
			{
				// MD 2/29/12 - 12.1 - Table Support
				//Worksheet worksheet = this.Worksheet;
				WorksheetRow row = this.Row;
				if (row == null)
					throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));

				Worksheet worksheet = row.Worksheet;

				if (value == null)
				{
					if (worksheet.HasDataValidationRules)
						worksheet.DataValidationRules.Remove(this);
				}
				else
				{
					worksheet.DataValidationRules.Add(value, new WorksheetReferenceCollection(this));
				}
			}
		}

		#endregion  // DataValidationRule

		#region HasComment

		/// <summary>
		/// Get the value indicating whether the cell has an associated comment.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellComment"/> method and check for a non null 
		/// return value, which does not create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the 
		/// WorksheetCell instance already exists, using this property is equivalent to using the WorksheetRow.GetCellComment method 
		/// and checking for a non null return value.
		/// </p>
		/// </remarks>
		/// <value>True if the cell has an associated comment; False otherwise.</value>
		/// <seealso cref="Comment"/>
		/// <seealso cref="WorksheetRow.GetCellComment"/>
		public bool HasComment
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				//return this.row.GetCellCommentInternal(this.columnIndex) != null; 
				WorksheetRow row = this.Row;
				if (row == null)
					return false;

				return row.GetCellCommentInternal(this.columnIndex) != null; 
			}
		}

		#endregion HasComment

		#region Formula

		/// <summary>
		/// Gets the formula which has been applied to the cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If a formula has been applied to the cell, getting the <see cref="Value"/> will return the calculated value of the formula.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellFormula"/> method, which does not 
		/// create a <see cref="WorksheetCell"/> instance internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this property is equivalent to using the WorksheetRow.GetCellFormula method.
		/// </p>
		/// </remarks>
		/// <value>The formula which has been applied to the cell or null if no formula has been applied.</value>
		/// <seealso cref="Excel.Formula.ApplyTo(WorksheetCell)"/>
		/// <seealso cref="Excel.Formula.ApplyTo(WorksheetRegion)"/>
		/// <seealso cref="Excel.Formula.ApplyTo(WorksheetRegion[])"/>
		/// <seealso cref="ApplyFormula"/>
		/// <seealso cref="WorksheetRegion.ApplyFormula"/>
		/// <seealso cref="WorksheetRegion.ApplyArrayFormula"/>
		/// <seealso cref="WorksheetRow.GetCellFormula"/>
		public Formula Formula
		{
			get
			{
				// MD 2/29/12 - 12.1 - Table Support
				//return this.row.GetCellFormulaInternal(this.columnIndex); 
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				return row.GetCellFormulaInternal(this.columnIndex); 
			}
		}

		#endregion Formula

		#region RowIndex

		/// <summary>
		/// Gets the row index of the cell.
		/// </summary>
		/// <value>The row index of the cell.</value>
		public int RowIndex
		{
			// MD 7/26/10 - TFS34398
			// The row index is not stored on the cell anymore, so get the index from the row.
			//get { return this.rowIndex; }
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				//return this.row.Index; 
				WorksheetRow row = this.Row;
				if (row == null)
					return -1;

				return row.Index; 
			}
		}

		#endregion RowIndex

		#region Value

		/// <summary>
		/// Gets or sets the value of the cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this cell belongs to a merged cell region and it is the top-left cell of the region, getting and setting the value 
		/// will get and set the value of the associated merged cell region. Getting the value of other cells in a merged cell region
		/// will always return null. Setting the value of other cells in a merged cell region will have no effect.
		/// </p>
		/// <p class="body">
		/// If a formula has been applied to the cell or a data table is associated with the cell, getting the Value will return the 
		/// calculated value of the cell.
		/// </p>
		/// <p class="body">
		/// The types supported for the value are:
		/// <BR/>
		/// <ul>
		/// <li class="taskitem"><span class="taskitemtext">System.Byte</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.SByte</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int16</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int64</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt16</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt64</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.UInt32</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Int32</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Single</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Double</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Boolean</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Char</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Enum</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Decimal</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.DateTime</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.String</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.Text.StringBuilder</span></li>
		/// <li class="taskitem"><span class="taskitemtext">System.DBNull</span></li>
		/// <li class="taskitem"><span class="taskitemtext"><see cref="ErrorValue"/></span></li>
		/// <li class="taskitem"><span class="taskitemtext"><see cref="FormattedString"/></span></li>
		/// </ul>
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> it is slightly faster to use the <see cref="WorksheetRow.GetCellValue(int)"/> or <see cref="WorksheetRow.SetCellValue"/> 
		/// methods, which do not create <see cref="WorksheetCell"/> instances internally. However, if a reference to the WorksheetCell instance 
		/// already exists, using this property is equivalent to using the WorksheetRow.GetCellComment or WorksheetRow.SetCellComment methods.
		/// </p>
		/// </remarks>
		/// <exception cref="System.NotSupportedException">
		/// The assigned value's type is not supported and can't be exported to Excel.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is a <see cref="Formula"/>. Instead, <see cref="Excel.Formula.ApplyTo(WorksheetCell)"/> 
		/// should be called on the Formula, passing in the cell.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is a <see cref="WorksheetDataTable"/>. Instead, the <see cref="WorksheetDataTable.CellsInTable"/>
		/// should be set to a region containing the cell.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is a FormattedString which is the value another cell or merged cell region.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value is assigned and this cell is part of an <see cref="ArrayFormula"/> or WorksheetDataTable.
		/// </exception>
		/// <value>The value of the cell.</value>
		/// <seealso cref="AssociatedMergedCellsRegion"/>
		/// <seealso cref="IsCellTypeSupported"/>
		/// <seealso cref="WorksheetMergedCellsRegion.Value"/>
		/// <seealso cref="WorksheetCell.Formula"/>
		/// <seealso cref="AssociatedDataTable"/>
		/// <seealso cref="WorksheetRow.GetCellValue(int)"/>
		/// <seealso cref="WorksheetRow.SetCellValue"/>
		public object Value
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				// return this.row.GetCellValueInternal(this.columnIndex); 
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				return row.GetCellValueInternal(this.columnIndex); 
			}
			set 
			{
				// MD 2/29/12 - 12.1 - Table Support
				// this.row.SetCellValue(this.columnIndex, value); 
				WorksheetRow row = this.Row;
				if (row == null)
					throw new InvalidOperationException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"));

				row.SetCellValue(this.columnIndex, value); 
			}
		}

		#endregion Value

		#region Worksheet

		/// <summary>
		/// Gets the worksheet to which the cell belongs.
		/// </summary>
		/// <value>The worksheet to which the cell belongs.</value>
		public Worksheet Worksheet
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				// return this.row.Worksheet; 
				WorksheetRow row = this.Row;
				if (row == null)
					return null;

				return row.Worksheet; 
			}
		}

		#endregion Worksheet

		#endregion Public Properties

		#region Internal Properties

		// MD 3/27/12 - 12.1 - Table Support
		#region Address

		internal WorksheetCellAddress Address
		{
			get
			{
				WorksheetRow row = this.Row;

				if (row == null)
					return WorksheetCellAddress.InvalidReference;

				return new WorksheetCellAddress(row.Index, this.columnIndex);
			}
		}

		#endregion // RegionAddress

		// MD 3/13/12 - 12.1 - Table Support
		#region RegionAddress

		internal WorksheetRegionAddress RegionAddress
		{
			get
			{
				WorksheetRow row = this.Row;

				if (row == null)
					return WorksheetRegionAddress.InvalidReference;

				return new WorksheetRegionAddress(row.Index, row.Index, this.columnIndex, this.columnIndex);
			}
		}

		#endregion // RegionAddress

		#region Row

		internal WorksheetRow Row
		{
			get 
			{
				// MD 2/29/12 - 12.1 - Table Support
				this.VerifyCellAddress();

				return this.row; 
			}
		}

		#endregion // Row

		#endregion Internal Properties

		#endregion Properties

		#region Operators

		/// <summary>
		/// Determines whether two cell instances are equivalent.
		/// </summary>
		/// <returns>True if the cells refer to the same location on the same worksheet; False otherwise.</returns>
		public static bool operator ==(WorksheetCell a, WorksheetCell b)
		{
			// MD 2/29/12 - 12.1 - Table Support
			if (Object.ReferenceEquals(a, b))
				return true;

			bool aIsNull = (object)a == null;
			bool bIsNull = (object)b == null;

			if (aIsNull && bIsNull)
				return true;

			if (aIsNull || bIsNull)
				return false;

			return 
				a.row == b.row &&
				a.columnIndex == b.columnIndex;
		}

		/// <summary>
		/// Determines whether two cell instances are not equivalent.
		/// </summary>
		/// <returns>False if the cells refer to the same location on the same worksheet; True otherwise.</returns>
		public static bool operator !=(WorksheetCell a, WorksheetCell b)
		{
			return !(a == b);
		}

		#endregion // Operators
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