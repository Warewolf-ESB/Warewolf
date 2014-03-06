using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;




using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel.FormulaUtilities
{
	#region Old Code

	//    internal class CellAddress
	//    {
	//        #region Constants

	//        // MD 7/2/08 - Excel 2007 Format
	//        //public const int MaxColumnIndex = 255;
	//        //public const int MaxRowIndex = 65535;

	//        #endregion Constants

	//        #region Member Variables

	//        // MD 10/22/10 - TFS36696
	//        // The CellAddress is now immutable.
	//        //private int row;
	//        //private bool rowAddressIsRelative;
	//        //private int column;
	//        //private bool columnAddressIsRelative;
	//        private readonly int row;
	//        private readonly bool rowAddressIsRelative;

	//        // MD 4/12/11 - TFS67084
	//        // Use short instead of int so we don't have to cast.
	//        //private readonly int column;
	//        private readonly short column;

	//        private readonly bool columnAddressIsRelative;

	//        #endregion Member Variables

	//        #region Constructor

	//        // MD 9/19/11 - TFS86108
	//        // Removed because this constructor doesn't imply whether we are converting to an absolute or an offset address.
	//        // Now there are ToAbsolute and ToOffset methods which are not ambiguous.
	//        #region Removed

	//        //// MD 5/21/07 - BR23050
	//        //// Changed the cell type because it may be a merged region
	//        ////public CellAddress( WorksheetCell originCell, CellAddress offsetAddress )
	//        //// MD 6/31/08 - Excel 2007 Format
	//        ////public CellAddress( IWorksheetCell originCell, CellAddress offsetAddress )
	//        //// MD 4/12/11 - TFS67084
	//        //// Moved away from using WorksheetCell objects.
	//        ////public CellAddress( Workbook workbook, IWorksheetCell originCell, CellAddress offsetAddress )
	//        //// MD 5/13/11 - Data Validations / Page Breaks
	//        //// This method doesn't need the row instance, just the row index.
	//        //// Also, some callers don't have a workbook. Since the workbook is only needed to get max column and row counts, we could get that from a format instead.
	//        ////public CellAddress(Workbook workbook, WorksheetRow originCellRow, short originCellColumnIndex, CellAddress offsetAddress)
	//        //public CellAddress(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex, CellAddress offsetAddress)
	//        //{
	//        //    this.rowAddressIsRelative = offsetAddress.rowAddressIsRelative;
	//        //    this.columnAddressIsRelative = offsetAddress.columnAddressIsRelative;

	//        //    if ( offsetAddress.rowAddressIsRelative )
	//        //    {
	//        //        // MD 5/21/07 - BR23050
	//        //        // The origin cell could be null
	//        //        //this.row = originCell.RowIndex + offsetAddress.row;
	//        //        // MD 4/12/11 - TFS67084
	//        //        // Moved away from using WorksheetCell objects.
	//        //        //if ( originCell != null )
	//        //        //    this.row = originCell.RowIndex + offsetAddress.row;
	//        //        // MD 5/13/11 - Data Validations / Page Breaks
	//        //        //if (originCellRow != null)
	//        //        //    this.row = originCellRow.Index + offsetAddress.row;
	//        //        if (originCellRowIndex >= 0)
	//        //            this.row = originCellRowIndex + offsetAddress.row;

	//        //        // MD 6/31/08 - Excel 2007 Format
	//        //        //this.row %= Workbook.MaxExcelRowCount;
	//        //        // MD 5/13/11 - Data Validations / Page Breaks
	//        //        //this.row %= workbook.MaxRowCount;
	//        //        int maxRowCount = Workbook.GetMaxRowCount(format);
	//        //        this.row %= maxRowCount;

	//        //        if ( this.row < 0 )
	//        //        {
	//        //            // MD 6/31/08 - Excel 2007 Format
	//        //            //this.row += Workbook.MaxExcelRowCount;
	//        //            // MD 5/13/11 - Data Validations / Page Breaks
	//        //            //this.row += workbook.MaxRowCount;
	//        //            this.row += maxRowCount;
	//        //        }
	//        //    }
	//        //    else
	//        //        this.row = offsetAddress.row;

	//        //    if ( offsetAddress.columnAddressIsRelative )
	//        //    {
	//        //        // MD 5/21/07 - BR23050
	//        //        // The origin cell could be null
	//        //        //this.column = originCell.ColumnIndex + offsetAddress.column;
	//        //        // MD 4/12/11 - TFS67084
	//        //        // Moved away from using WorksheetCell objects.
	//        //        //if ( originCell != null )
	//        //        //    this.column = originCell.ColumnIndex + offsetAddress.column;
	//        //        // MD 5/13/11 - Data Validations / Page Breaks
	//        //        //if (originCellRow != null)
	//        //        if (originCellColumnIndex >= 0)
	//        //            this.column = (short)(originCellColumnIndex + offsetAddress.column);

	//        //        // MD 6/31/08 - Excel 2007 Format
	//        //        //this.column %= Workbook.MaxExcelColumnCount;
	//        //        // MD 5/13/11 - Data Validations / Page Breaks
	//        //        //this.column %= workbook.MaxColumnCount;
	//        //        short maxColumnCount = Workbook.GetMaxColumnCountInternal(format);
	//        //        this.column %= maxColumnCount;

	//        //        if ( this.column < 0 )
	//        //        {
	//        //            // MD 6/31/08 - Excel 2007 Format
	//        //            //this.column += Workbook.MaxExcelColumnCount;
	//        //            // MD 5/13/11 - Data Validations / Page Breaks
	//        //            //this.column += workbook.MaxColumnCount;
	//        //            this.column += maxColumnCount;
	//        //        }
	//        //    }
	//        //    else
	//        //        this.column = offsetAddress.column;
	//        //} 

	//        #endregion  // Removed

	//        // MD 4/12/11 - TFS67084
	//        // Use short instead of int so we don't have to cast.
	//        //public CellAddress( int row, bool rowAddressIsRelative, int column, bool columnAddressIsRelative )
	//        public CellAddress(int row, bool rowAddressIsRelative, short column, bool columnAddressIsRelative)
	//        {
	//            this.row = row;
	//            this.rowAddressIsRelative = rowAddressIsRelative;
	//            this.column = column;
	//            this.columnAddressIsRelative = columnAddressIsRelative;
	//        }

	//        #endregion Constructor

	//        #region Base Class Overrides

	//        // MD 12/22/11 - 12.1 - Table Support
	//        #region Equals

	//        public override bool Equals(object obj)
	//        {
	//            CellAddress other = obj as CellAddress;
	//            if (other == null)
	//                return false;

	//            return
	//                this.row == other.row &&
	//                this.rowAddressIsRelative == other.rowAddressIsRelative &&
	//                this.column == other.column &&
	//                this.columnAddressIsRelative == other.columnAddressIsRelative;
	//        }

	//        #endregion // Equals

	//        #region GetHashCode

	//        public override int GetHashCode()
	//        {
	//            int temp = this.column << 16 | this.row;

	//            if (rowAddressIsRelative)
	//                temp <<= 1;

	//            if (columnAddressIsRelative)
	//                temp <<= 2;

	//            return temp;
	//        }

	//        #endregion // GetHashCode

	//        #endregion // Base Class Overrides

	//        #region Methods

	//        // MD 10/22/10 - TFS36696
	//        // The CellAddress is now immutable, so we don't have to clone it.
	//        //#region Clone
	//        //
	//        //public CellAddress Clone()
	//        //{
	//        //    return new CellAddress(this.row, this.rowAddressIsRelative, this.column, this.columnAddressIsRelative);
	//        //}
	//        //
	//        //#endregion Clone

	//        // MD 8/18/08 - Excel formula solving
	//        #region GetAbsoluteColumnIndex

	//#if DEBUG
	//        /// <summary>
	//        /// Gets the absolute column index of the cell being addressed when it is used from the specified formula owner.
	//        /// </summary>
	//        /// <param name="currentFormat">The format of the workbook.</param>
	//        /// <param name="formulaOwnerColumnIndex">The column index of the owner of the formula whose tokens use this address.</param>
	//        /// <param name="relativeAddressesAreOffsets">Indicates whether relative address are offsets from the formula owner.</param>
	//        /// <returns>The absolute column index of the cell referenced by this address.</returns>  
	//#endif
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public int GetAbsoluteColumnIndex( IWorksheetCell formulaOwner, bool relativeAddressesAreOffsets )
	//        // MD 2/20/12 - 12.1 - Table Support
	//        //public short GetAbsoluteColumnIndex(WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex, bool relativeAddressesAreOffsets)
	//        public short GetAbsoluteColumnIndex(WorkbookFormat currentFormat, short formulaOwnerColumnIndex, bool relativeAddressesAreOffsets)
	//        {
	//            // MD 4/12/11 - TFS67084
	//            // Use short instead of int so we don't have to cast.
	//            //int columnIndex = this.Column;
	//            short columnIndex = this.Column;

	//            if ( relativeAddressesAreOffsets == false || this.ColumnAddressIsRelative == false )
	//                return columnIndex;

	//            // MD 2/24/12 - 12.1 - Table Support
	//            if (formulaOwnerColumnIndex < 0)
	//                return -1;

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //columnIndex += formulaOwner.ColumnIndex;
	//            //
	//            //int maxColumns = Workbook.GetMaxColumnCount( formulaOwner.Worksheet.CurrentFormat );
	//            columnIndex += formulaOwnerColumnIndex;

	//            // MD 2/20/12 - 12.1 - Table Support
	//            //short maxColumns = Workbook.GetMaxColumnCountInternal(formulaOwnerRow.Worksheet.CurrentFormat);
	//            short maxColumns = Workbook.GetMaxColumnCountInternal(currentFormat);

	//            if ( columnIndex < 0 )
	//                columnIndex += maxColumns;
	//            else if ( maxColumns <= columnIndex )
	//                columnIndex -= maxColumns;

	//            return columnIndex;
	//        } 

	//        #endregion GetAbsoluteColumnIndex

	//        // MD 8/18/08 - Excel formula solving
	//        #region GetAbsoluteRowIndex

	//#if DEBUG
	//        /// <summary>
	//        /// Gets the absolute row index of the cell being addressed when it is used from the specified formula owner.
	//        /// </summary>
	//        /// <param name="currentFormat">The format of the workbook.</param>
	//        /// <param name="formulaOwnerRowIndex">The row index of the owner of the formula whose tokens use this address.</param>
	//        /// <param name="relativeAddressesAreOffsets">Indicates whether relative address are offsets from the formula owner.</param>
	//        /// <returns>The absolute row index of the cell referenced by this address.</returns>  
	//#endif
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public int GetAbsoluteRowIndex( IWorksheetCell formulaOwner, bool relativeAddressesAreOffsets )
	//        // MD 2/20/12 - 12.1 - Table Support
	//        //public int GetAbsoluteRowIndex(WorksheetRow formulaOwnerRow, bool relativeAddressesAreOffsets)
	//        public int GetAbsoluteRowIndex(WorkbookFormat currentFormat, int formulaOwnerRowIndex, bool relativeAddressesAreOffsets)
	//        {
	//            int rowIndex = this.Row;

	//            if ( relativeAddressesAreOffsets == false || this.RowAddressIsRelative == false )
	//                return rowIndex;

	//            // MD 2/24/12 - 12.1 - Table Support
	//            if (formulaOwnerRowIndex < 0)
	//                return -1;

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //rowIndex += formulaOwner.RowIndex;
	//            //
	//            //int maxRows = Workbook.GetMaxRowCount( formulaOwner.Worksheet.CurrentFormat );
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //rowIndex += formulaOwnerRow.Index;
	//            //
	//            //int maxRows = Workbook.GetMaxRowCount(formulaOwnerRow.Worksheet.CurrentFormat);
	//            rowIndex += formulaOwnerRowIndex;

	//            int maxRows = Workbook.GetMaxRowCount(currentFormat);

	//            if ( rowIndex < 0 )
	//                rowIndex += maxRows;
	//            else if ( maxRows <= rowIndex )
	//                rowIndex -= maxRows;

	//            return rowIndex;
	//        } 

	//        #endregion GetAbsoluteRowIndex

	//        #region GetCellReferenceString

	//        // MD 8/12/08
	//        // Found while implementing Excel formula solving
	//        #region Not Used

	//        /*
	//        public static string GetCellReferenceString(
	//            int row,
	//            int column,
	//            Workbook workbook,	// MD 6/31/08 - Excel 2007 Format
	//            IWorksheetCell sourceCell,
	//            CellReferenceMode cellReferenceMode )
	//        {
	//            // MD 5/21/07 - BR23050
	//            // GetCellReferenceString has another paramter now, indicating whether relative addresses are offsets
	//            //return GetCellReferenceString( row, column, false, false, sourceCell, cellReferenceMode );
	//            // MD 6/31/08 - Excel 2007 Format
	//            //return GetCellReferenceString( row, column, false, false, sourceCell, false, cellReferenceMode );
	//            return GetCellReferenceString( 
	//                row, 
	//                column, 
	//                false, 
	//                false, 
	//                workbook,
	//                sourceCell, 
	//                false, 
	//                cellReferenceMode );
	//        }
	//        */

	//        #endregion Not Used

	//        public static string GetCellReferenceString(
	//            int row,
	//            int column,
	//            bool rowAddressIsRelative,
	//            bool columnAddressIsRelative,
	//            WorkbookFormat format,	// MD 6/31/08 - Excel 2007 Format

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //IWorksheetCell sourceCell,
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //WorksheetRow sourceCellRow, short sourceCellColumnIndex,
	//            int sourceCellRowIndex, short sourceCellColumnIndex,

	//            bool relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
	//            CellReferenceMode cellReferenceMode )
	//        {
	//            // MD 5/21/07 - BR23050
	//            // GetRowString and GetColumnString each have another parameter now, indicating whether relative addresses are offsets
	//            //string rowString = GetRowString( row, rowAddressIsRelative, sourceCell, cellReferenceMode );
	//            //string columnString = GetColumnString( column, columnAddressIsRelative, sourceCell, cellReferenceMode );
	//            // MD 6/31/08 - Excel 2007 Format
	//            //string rowString = GetRowString( row, rowAddressIsRelative, sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
	//            //string columnString = GetColumnString( column, columnAddressIsRelative, sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
	//            string rowString = CellAddress.GetRowString( 
	//                row, 
	//                rowAddressIsRelative,
	//                format,

	//                // MD 4/12/11 - TFS67084
	//                // Moved away from using WorksheetCell objects.
	//                //sourceCell, 
	//                // MD 2/20/12 - 12.1 - Table Support
	//                //sourceCellRow, 
	//                sourceCellRowIndex, 

	//                relativeAddressesAreOffsets, 
	//                cellReferenceMode );
	//            string columnString = CellAddress.GetColumnString( 
	//                column, 
	//                columnAddressIsRelative,
	//                format,

	//                // MD 4/12/11 - TFS67084
	//                // Moved away from using WorksheetCell objects.
	//                //sourceCell, 
	//                sourceCellColumnIndex, 

	//                relativeAddressesAreOffsets, 
	//                cellReferenceMode );

	//            if ( cellReferenceMode == CellReferenceMode.A1 )
	//                return columnString + rowString;
	//            else
	//                return rowString + columnString;
	//        }

	//        #endregion GetCellReferenceString

	//        #region GetColumnString

	//        //  BF 8/7/08
	//        //  Added an overload so that a Workbook reference is not required

	//        public static string GetColumnString( 
	//            int column,
	//            bool columnAddressIsRelative,
	//            WorkbookFormat format,	// MD 6/31/08 - Excel 2007 Format

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //IWorksheetCell sourceCell,
	//            short sourceCellColumnIndex,

	//            bool relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
	//            CellReferenceMode cellReferenceMode )
	//        {
	//            int maxColumnCount = Workbook.GetMaxColumnCount( format );

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //return CellAddress.GetColumnString( column, columnAddressIsRelative, maxColumnCount, sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
	//            return CellAddress.GetColumnString(column, columnAddressIsRelative, maxColumnCount, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode);
	//        }

	//        public static string GetColumnString( 
	//            int column,
	//            bool columnAddressIsRelative,
	//            int maxColumnCount,

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //IWorksheetCell sourceCell,
	//            short sourceCellColumnIndex,

	//            bool relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
	//            CellReferenceMode cellReferenceMode )
	//        {
	//            // MD 5/21/07 - BR23050
	//            // Refactored the code to account for relative offsets that need to be absolutes and vice versa
	//            #region Refactored

	//            //if ( cellReferenceMode == CellReferenceMode.A1 )
	//            //{
	//            //    string columnLetters = ( (char)( 'A' + ( column % 26 ) ) ).ToString();
	//            //
	//            //    if ( column >= 26 )
	//            //        columnLetters = ( (char)( 'A' + ( column / 26 ) - 1 ) ) + columnLetters;
	//            //
	//            //    if ( columnAddressIsRelative == false )
	//            //        return "$" + columnLetters;
	//            //
	//            //    return columnLetters;
	//            //}
	//            //else
	//            //{
	//            //    int columnValue = column;
	//            //
	//            //    if ( sourceCell != null && columnAddressIsRelative )
	//            //        columnValue = column - sourceCell.ColumnIndex;
	//            //
	//            //    if ( columnValue == 0 && columnAddressIsRelative )
	//            //        return "C";
	//            //
	//            //    if ( columnAddressIsRelative )
	//            //        return "C[" + columnValue + "]";
	//            //    else
	//            //        return "C" + ( columnValue + 1 );
	//            //}

	//            #endregion Refactored

	//            // Make sure the column index is within the absolute value range of the column count
	//            // MD 6/31/08 - Excel 2007 Format
	//            //column %= Workbook.MaxExcelColumnCount;
	//            column %= maxColumnCount;

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //int originColumnIndex = 0;
	//            //
	//            //if ( sourceCell != null )
	//            //    originColumnIndex = sourceCell.ColumnIndex;
	//            int originColumnIndex = Math.Max(0, (int)sourceCellColumnIndex);

	//            if ( columnAddressIsRelative )
	//            {
	//                if ( relativeAddressesAreOffsets )
	//                {
	//                    // If relative addresses are offsets, make sure the offset will not have to wrap around the worksheet
	//                    // MD 6/31/08 - Excel 2007 Format
	//                    //if ( column + originColumnIndex < 0 )
	//                    //    column += Workbook.MaxExcelColumnCount;
	//                    //else if ( column + originColumnIndex >= Workbook.MaxExcelColumnCount )
	//                    //    column -= Workbook.MaxExcelColumnCount;
	//                    if ( column + originColumnIndex < 0 )
	//                        column += maxColumnCount;
	//                    else if ( column + originColumnIndex >= maxColumnCount )
	//                        column -= maxColumnCount;
	//                }
	//                else
	//                {
	//                    // If relative addresses are absolute, make sure the absolute column index is valid in the worksheet
	//                    if ( column < 0 )
	//                    {
	//                        // MD 6/31/08 - Excel 2007 Format
	//                        //column += Workbook.MaxExcelColumnCount;
	//                        column += maxColumnCount;
	//                    }
	//                }
	//            }

	//            if ( cellReferenceMode == CellReferenceMode.A1 )
	//            {
	//                // A1 references need to be absolute, change them if they are not
	//                if ( columnAddressIsRelative && relativeAddressesAreOffsets )
	//                {
	//                    // Convert the column offset from the origin to an absolute cell reference
	//                    column += originColumnIndex;
	//                }

	//                // MD 7/23/08 - Excel formula solving
	//                // This code only allowed 2 character column addresses. We must now support 3 character column addresses.
	//                // The new code below allows for any number of characters.
	//                //string columnLetters = ( (char)( 'A' + ( column % 26 ) ) ).ToString();
	//                //
	//                //if ( column >= 26 )
	//                //	columnLetters = ( (char)( 'A' + ( column / 26 ) - 1 ) ) + columnLetters;

	//                // MD 8/22/08 - Excel formula solving - Performance
	//                // We don't need a stringbuild, there are only 3 characters at most.
	//                //StringBuilder columnLettersBuilder = new StringBuilder();
	//                string columnLetters = string.Empty;

	//                bool isLowOrderLetter = true;

	//                // Determine the column leters in reverse order.
	//                do
	//                {
	//                    char currentLetter = (char)( 'A' + ( column % 26 ) );
	//                    column /= 26;

	//                    // The non-low order letters are off by one because an absence of the letter is actually their first value.
	//                    // 'A' is the second value, 'B' is the thrid, and so on. This is not the case for the low order letter,
	//                    // because 'A' is the first value, 'B' is the second, and so on.
	//                    if ( isLowOrderLetter == false )
	//                    {
	//                        // MD 6/8/10 - TFS33389
	//                        // In the Excel 2007 format, there can now be three letters in the address. If the second letter is 'A' here and we
	//                        // need to decrement it, we need to roll it over to 'Z' and carry one letter value from the higher significant position.
	//                        // Otherwise, just decrement the letter.
	//                        //currentLetter--;
	//                        if (currentLetter == 'A')
	//                        {
	//                            currentLetter = 'Z';
	//                            column--;
	//                        }
	//                        else
	//                        {
	//                            currentLetter--;
	//                        }
	//                    }

	//                    // MD 8/22/08 - Excel formula solving - Performance
	//                    // We don't need a stringbuild, there are only 3 characters at most.
	//                    //columnLettersBuilder.Insert( 0, currentLetter );
	//                    columnLetters = currentLetter + columnLetters;

	//                    isLowOrderLetter = false;
	//                }
	//                while ( column > 0 );

	//                // MD 8/22/08 - Excel formula solving - Performance
	//                // We don't need a stringbuild, there are only 3 characters at most.
	//                //string columnLetters = columnLettersBuilder.ToString();

	//                if ( columnAddressIsRelative == false )
	//                    return "$" + columnLetters;

	//                return columnLetters;
	//            }
	//            else
	//            {
	//                if ( columnAddressIsRelative == false )
	//                {
	//                    //return "C" + ( column + 1 ).ToString( CultureInfo.CurrentCulture );
	//                    return "C" + (column + 1).ToString();
	//                }

	//                // R1C1 relative references need to be offsets, change them if they are not
	//                if ( relativeAddressesAreOffsets == false )
	//                {
	//                    // Convert the absolute column index into an offset from the origin column
	//                    column -= originColumnIndex;
	//                }

	//                if ( column == 0 )
	//                    return "C";

	//                return "C[" + column + "]";
	//            }
	//        }

	//        #endregion GetColumnString

	//        #region GetRowString

	//        //  BF 8/7/08
	//        //  Added an overload so that a Workbook reference is not required

	//        public static string GetRowString( 
	//            int row, 
	//            bool rowAddressIsRelative,
	//            WorkbookFormat format,	// MD 6/31/08 - Excel 2007 Format

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //IWorksheetCell sourceCell,
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //WorksheetRow sourceCellRow,
	//            int sourceCellRowIndex,

	//            bool relativeAddressesAreOffsets, 
	//            CellReferenceMode cellReferenceMode )
	//        {
	//            int maxRowCount = Workbook.GetMaxRowCount( format );

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //return GetRowString(row, rowAddressIsRelative, maxRowCount, sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //return GetRowString(row, rowAddressIsRelative, maxRowCount, sourceCellRow, relativeAddressesAreOffsets, cellReferenceMode);
	//            return GetRowString(row, rowAddressIsRelative, maxRowCount, sourceCellRowIndex, relativeAddressesAreOffsets, cellReferenceMode);
	//        }

	//        public static string GetRowString( 
	//            int row, 
	//            bool rowAddressIsRelative,
	//            int maxRowCount,

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //IWorksheetCell sourceCell,
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //WorksheetRow sourceCellRow,
	//            int sourceCellRowIndex,

	//            bool relativeAddressesAreOffsets, 
	//            CellReferenceMode cellReferenceMode )
	//        {
	//            // MD 5/21/07 - BR23050
	//            // Refactored the code to account for relative offsets that need to be absolutes and vice versa
	//            #region Refactored

	//            //string rowNumber = ( row + 1 ).ToString( CultureInfo.CurrentCulture );
	//            //
	//            //if ( cellReferenceMode == CellReferenceMode.A1 )
	//            //{
	//            //    if ( rowAddressIsRelative == false )
	//            //        return "$" + rowNumber;
	//            //
	//            //    return rowNumber;
	//            //}
	//            //else
	//            //{
	//            //    int rowValue = row;
	//            //
	//            //    if ( sourceCell != null && rowAddressIsRelative )
	//            //        rowValue = row - sourceCell.RowIndex;
	//            //
	//            //    if ( rowValue == 0 && rowAddressIsRelative )
	//            //        return "R";
	//            //
	//            //    if ( rowAddressIsRelative )
	//            //        return "R[" + rowValue + "]";
	//            //    else
	//            //        return "R" + ( rowValue + 1 );
	//            //}

	//            #endregion Refactored

	//            // Make sure the row index is within the absolute value range of the row count
	//            // MD 6/31/08 - Excel 2007 Format
	//            //row %= Workbook.MaxExcelRowCount;
	//            //int maxRowCount = workbook.MaxRowCount;
	//            row %= maxRowCount;

	//            int originRowIndex = 0;

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //if ( sourceCell != null )
	//            //    originRowIndex = sourceCell.RowIndex;
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //if (sourceCellRow != null)
	//            //    originRowIndex = sourceCellRow.Index;
	//            if (sourceCellRowIndex != -1)
	//                originRowIndex = sourceCellRowIndex;

	//            if ( rowAddressIsRelative )
	//            {
	//                if ( relativeAddressesAreOffsets )
	//                {
	//                    // If relative addresses are offsets, make sure the offset will not have to wrap around the worksheet
	//                    // MD 6/31/08 - Excel 2007 Format
	//                    //if ( row + originRowIndex < 0 )
	//                    //    row += Workbook.MaxExcelRowCount;
	//                    //else if ( row + originRowIndex >= Workbook.MaxExcelRowCount )
	//                    //    row -= Workbook.MaxExcelRowCount;
	//                    if ( row + originRowIndex < 0 )
	//                        row += maxRowCount;
	//                    else if ( row + originRowIndex >= maxRowCount )
	//                        row -= maxRowCount;
	//                }
	//                else
	//                {
	//                    // If relative addresses are absolute, make sure the absolute row index is valid in the worksheet
	//                    if ( row < 0 )
	//                    {
	//                        // MD 6/31/08 - Excel 2007 Format
	//                        //row += Workbook.MaxExcelRowCount;
	//                        row += maxRowCount;
	//                    }
	//                }
	//            }

	//            if ( cellReferenceMode == CellReferenceMode.A1 )
	//            {
	//                if ( rowAddressIsRelative == false )
	//                {
	//                    // MD 4/6/12 - TFS101506
	//                    //return "$" + ( row + 1 ).ToString( CultureInfo.CurrentCulture );
	//                    return "$" + (row + 1).ToString();
	//                }

	//                // A1 references need to be absolute, change them if they are not
	//                if ( relativeAddressesAreOffsets )
	//                {
	//                    // Convert the row offset from the origin to an absolute cell reference
	//                    row += originRowIndex;
	//                }

	//                // MD 4/6/12 - TFS101506
	//                //return ( row + 1 ).ToString( CultureInfo.CurrentCulture );
	//                return (row + 1).ToString();
	//            }
	//            else
	//            {
	//                if ( rowAddressIsRelative == false )
	//                {
	//                    // MD 4/6/12 - TFS101506
	//                    //return "R" + ( row + 1 ).ToString( CultureInfo.CurrentCulture );
	//                    return "R" + (row + 1).ToString();
	//                }

	//                // R1C1 relative references need to be offsets, change them if they are not
	//                if ( relativeAddressesAreOffsets == false )
	//                {
	//                    // Convert the absolute row index into an offset from the origin row
	//                    row -= originRowIndex;
	//                }

	//                if ( row == 0 )
	//                    return "R";

	//                // MD 4/6/12 - TFS101506
	//                //return "R[" + row.ToString( CultureInfo.CurrentCulture ) + "]";
	//                return "R[" + row.ToString() + "]";
	//            }
	//        }

	//        #endregion GetRowString

	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        #region Removed

	//        //        // MD 8/18/08 - Excel formula solving
	//        //        #region GetTargetCell

	//        //#if DEBUG
	//        //        /// <summary>
	//        //        /// Gets the cell being addressed when this address is used from the specified formula owner.
	//        //        /// </summary>
	//        //        /// <param name="formulaOwner">The owner of the formula whose tokens use this address.</param>
	//        //        /// <param name="relativeAddressesAreOffsets">Indicates whether relative address are offsets from the formula owner.</param>
	//        //        /// <returns>The cell referenced by this address from the same worksheet as the formula owner.</returns>  
	//        //#endif
	//        //        public WorksheetCell GetTargetCell( IWorksheetCell formulaOwner, bool relativeAddressesAreOffsets )
	//        //        {
	//        //            return this.GetTargetCell( formulaOwner.Worksheet, formulaOwner, relativeAddressesAreOffsets );
	//        //        }

	//        //#if DEBUG
	//        //        /// <summary>
	//        //        /// Gets the cell being addressed when this address is used from the specified formula owner.
	//        //        /// </summary>
	//        //        /// <param name="worksheet">The worksheet on which the addressed cell resides.</param>
	//        //        /// <param name="formulaOwner">The owner of the formula whose tokens use this address.</param>
	//        //        /// <param name="relativeAddressesAreOffsets">Indicates whether relative address are offsets from the formula owner.</param>
	//        //        /// <returns>The cell referenced by this address.</returns>  
	//        //#endif
	//        //        public WorksheetCell GetTargetCell( Worksheet worksheet, IWorksheetCell formulaOwner, bool relativeAddressesAreOffsets )
	//        //        {
	//        //            int column = this.GetAbsoluteColumnIndex( formulaOwner, relativeAddressesAreOffsets );
	//        //            int row = this.GetAbsoluteRowIndex( formulaOwner, relativeAddressesAreOffsets );

	//        //            return worksheet.Rows[ row ].Cells[ column ];
	//        //        } 

	//        //        #endregion GetTargetCell 

	//        #endregion  // Removed

	//        #region Offset

	//        // MD 10/22/10 - TFS36696
	//        // Now that the CellAddress is immutable, we can't change the row or column values.
	//        //public void Offset( Point offset )
	//        //{
	//        //    if ( this.columnAddressIsRelative )
	//        //        this.column += (int)offset.X;
	//        //
	//        //    if ( this.rowAddressIsRelative )
	//        //        this.row += (int)offset.Y;
	//        //}
	//        public CellAddress Offset(Point offset)
	//        {
	//            if ((this.columnAddressIsRelative == false || offset.X == 0) &&
	//                (this.rowAddressIsRelative == false || offset.Y == 0))
	//            {
	//                return this;
	//            }

	//            // MD 4/12/11 - TFS67084
	//            // Use short instead of int so we don't have to cast.
	//            //int newColumn = this.column;
	//            //if (this.columnAddressIsRelative)
	//            //    newColumn += (int)offset.X;
	//            short newColumn = this.column;
	//            if (this.columnAddressIsRelative)
	//                newColumn += (short)offset.X;

	//            int newRow = this.row;
	//            if (this.rowAddressIsRelative)
	//                newRow += (int)offset.Y;

	//            return new CellAddress(newRow, this.rowAddressIsRelative, newColumn, this.columnAddressIsRelative);
	//        }

	//        #endregion Offset

	//        // MD 9/19/11 - TFS86108
	//        #region ToAbsolute

	//        public CellAddress ToAbsolute(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex)
	//        {
	//            int row = this.row;
	//            short column = this.column;

	//            if (this.rowAddressIsRelative)
	//            {
	//                if (originCellRowIndex >= 0)
	//                    row = originCellRowIndex + this.row;

	//                int maxRowCount = Workbook.GetMaxRowCount(format);
	//                row %= maxRowCount;

	//                if (row < 0)
	//                    row += maxRowCount;
	//            }

	//            if (this.columnAddressIsRelative)
	//            {
	//                if (originCellColumnIndex >= 0)
	//                    column = (short)(originCellColumnIndex + this.column);

	//                short maxColumnCount = Workbook.GetMaxColumnCountInternal(format);
	//                column %= maxColumnCount;

	//                if (column < 0)
	//                    column += maxColumnCount;
	//            }

	//            return new CellAddress(row, this.rowAddressIsRelative, column, this.columnAddressIsRelative);
	//        }

	//        #endregion  // ToAbsolute

	//        // MD 9/19/11 - TFS86108
	//        #region ToOffset

	//        public CellAddress ToOffset(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex)
	//        {
	//            int row = this.row;
	//            short column = this.column;

	//            if (this.rowAddressIsRelative)
	//            {
	//                if (originCellRowIndex >= 0)
	//                    row = this.row - originCellRowIndex;

	//                int maxRowCount = Workbook.GetMaxRowCount(format);
	//                if (row < 0)
	//                    row += maxRowCount;
	//            }

	//            if (this.columnAddressIsRelative)
	//            {
	//                if (originCellColumnIndex >= 0)
	//                    column = (short)(this.column - originCellColumnIndex);

	//                short maxColumnCount = Workbook.GetMaxColumnCountInternal(format);
	//                if (column < 0)
	//                    column += maxColumnCount;
	//            }

	//            return new CellAddress(row, this.rowAddressIsRelative, column, this.columnAddressIsRelative);
	//        }

	//        #endregion  // ToOffset

	//        #region ToString

	//        // MD 6/31/08 - Excel 2007 Format
	//        //public string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode )
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, WorkbookFormat format )
	//        // MD 2/20/12 - 12.1 - Table Support
	//        //public string ToString(WorksheetRow sourceCellRow, short sourceCellColumnIndex, CellReferenceMode cellReferenceMode, WorkbookFormat format)
	//        public string ToString(int sourceCellRowIndex, short sourceCellColumnIndex, CellReferenceMode cellReferenceMode, WorkbookFormat format)
	//        {
	//            // MD 5/21/07 - BR23050
	//            // Moved code to the other overload
	//            // MD 6/31/08 - Excel 2007 Format
	//            //return this.ToString( sourceCell, false, cellReferenceMode );
	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //return this.ToString( sourceCell, false, cellReferenceMode, format );
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //return this.ToString(sourceCellRow, sourceCellColumnIndex, false, cellReferenceMode, format);
	//            return this.ToString(sourceCellRowIndex, sourceCellColumnIndex, false, cellReferenceMode, format);
	//        }

	//        // MD 5/21/07 - BR23050
	//        // Added a parameter to determine if relative addresses are offsets from the origin cell's address
	//        // MD 6/31/08 - Excel 2007 Format
	//        //public string ToString( IWorksheetCell sourceCell, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode )
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public string ToString( IWorksheetCell sourceCell, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format )
	//        // MD 2/20/12 - 12.1 - Table Support
	//        //public string ToString(WorksheetRow sourceCellRow, short sourceCellColumnIndex, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format)
	//        public string ToString(int sourceCellRowIndex, short sourceCellColumnIndex, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format)
	//        {
	//            return GetCellReferenceString(
	//                this.row,
	//                this.column,
	//                this.rowAddressIsRelative,
	//                this.columnAddressIsRelative,
	//                format,	// MD 6/31/08 - Excel 2007 Format
	//                // MD 4/12/11 - TFS67084
	//                // Moved away from using WorksheetCell objects.
	//                //sourceCell,
	//                // MD 2/20/12 - 12.1 - Table Support
	//                //sourceCellRow, sourceCellColumnIndex,
	//                sourceCellRowIndex, sourceCellColumnIndex,
	//                relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
	//                cellReferenceMode );
	//        }

	//        #endregion ToString

	//        #region VerifyFormatLimits

	//        public void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
	//        {
	//            int maxColumnIndex = Workbook.GetMaxColumnCount( testFormat ) - 1;

	//            if ( maxColumnIndex < Math.Abs( this.column ) )
	//            {
	//                limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxColumnIndex"), this.column, maxColumnIndex));
	//                return;
	//            }

	//            int maxRowIndex = Workbook.GetMaxRowCount( testFormat ) - 1;

	//            if ( maxRowIndex < Math.Abs( this.row ) )
	//                limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxRowIndex"), this.row, maxRowIndex));
	//        } 

	//        #endregion VerifyFormatLimits

	//        #endregion Methods

	//        #region Properties

	//        #region Column

	//        // MD 4/12/11 - TFS67084
	//        // Use short instead of int so we don't have to cast.
	//        //public int Column
	//        public short Column
	//        {
	//            get { return this.column; }
	//        } 

	//        #endregion Column

	//        #region ColumnAddressIsRelative

	//        public bool ColumnAddressIsRelative
	//        {
	//            get { return this.columnAddressIsRelative; }
	//        } 

	//        #endregion ColumnAddressIsRelative

	//        #region HasRelativeAddresses

	//        public bool HasRelativeAddresses
	//        {
	//            get { return this.columnAddressIsRelative || this.rowAddressIsRelative; }
	//        } 

	//        #endregion HasRelativeAddresses

	//        #region Row

	//        public int Row
	//        {
	//            get { return this.row; }
	//        } 

	//        #endregion Row

	//        #region RowAddressIsRelative

	//        public bool RowAddressIsRelative
	//        {
	//            get { return this.rowAddressIsRelative; }
	//        } 

	//        #endregion RowAddressIsRelative

	//        #endregion Properties
	//    }

	#endregion // Old Code
	internal class CellAddress
	{
		#region Constants

		// MD 7/2/08 - Excel 2007 Format
		//public const int MaxColumnIndex = 255;
		//public const int MaxRowIndex = 65535;

		#endregion Constants

		#region Member Variables

		// MD 10/22/10 - TFS36696
		// The CellAddress is now immutable.
		//private int row;
		//private bool rowAddressIsRelative;
		//private int column;
		//private bool columnAddressIsRelative;
		private readonly int row;
		private readonly bool rowAddressIsRelative;

		// MD 4/12/11 - TFS67084
		// Use short instead of int so we don't have to cast.
		//private readonly int column;
		private readonly short column;

		private readonly bool columnAddressIsRelative;

		#endregion Member Variables

		#region Constructor

		// MD 9/19/11 - TFS86108
		// Removed because this constructor doesn't imply whether we are converting to an absolute or an offset address.
		// Now there are ToAbsolute and ToOffset methods which are not ambiguous.
		#region Removed

		//// MD 5/21/07 - BR23050
		//// Changed the cell type because it may be a merged region
		////public CellAddress( WorksheetCell originCell, CellAddress offsetAddress )
		//// MD 6/31/08 - Excel 2007 Format
		////public CellAddress( IWorksheetCell originCell, CellAddress offsetAddress )
		//// MD 4/12/11 - TFS67084
		//// Moved away from using WorksheetCell objects.
		////public CellAddress( Workbook workbook, IWorksheetCell originCell, CellAddress offsetAddress )
		//// MD 5/13/11 - Data Validations / Page Breaks
		//// This method doesn't need the row instance, just the row index.
		//// Also, some callers don't have a workbook. Since the workbook is only needed to get max column and row counts, we could get that from a format instead.
		////public CellAddress(Workbook workbook, WorksheetRow originCellRow, short originCellColumnIndex, CellAddress offsetAddress)
		//public CellAddress(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex, CellAddress offsetAddress)
		//{
		//    this.rowAddressIsRelative = offsetAddress.rowAddressIsRelative;
		//    this.columnAddressIsRelative = offsetAddress.columnAddressIsRelative;

		//    if ( offsetAddress.rowAddressIsRelative )
		//    {
		//        // MD 5/21/07 - BR23050
		//        // The origin cell could be null
		//        //this.row = originCell.RowIndex + offsetAddress.row;
		//        // MD 4/12/11 - TFS67084
		//        // Moved away from using WorksheetCell objects.
		//        //if ( originCell != null )
		//        //    this.row = originCell.RowIndex + offsetAddress.row;
		//        // MD 5/13/11 - Data Validations / Page Breaks
		//        //if (originCellRow != null)
		//        //    this.row = originCellRow.Index + offsetAddress.row;
		//        if (originCellRowIndex >= 0)
		//            this.row = originCellRowIndex + offsetAddress.row;

		//        // MD 6/31/08 - Excel 2007 Format
		//        //this.row %= Workbook.MaxExcelRowCount;
		//        // MD 5/13/11 - Data Validations / Page Breaks
		//        //this.row %= workbook.MaxRowCount;
		//        int maxRowCount = Workbook.GetMaxRowCount(format);
		//        this.row %= maxRowCount;

		//        if ( this.row < 0 )
		//        {
		//            // MD 6/31/08 - Excel 2007 Format
		//            //this.row += Workbook.MaxExcelRowCount;
		//            // MD 5/13/11 - Data Validations / Page Breaks
		//            //this.row += workbook.MaxRowCount;
		//            this.row += maxRowCount;
		//        }
		//    }
		//    else
		//        this.row = offsetAddress.row;

		//    if ( offsetAddress.columnAddressIsRelative )
		//    {
		//        // MD 5/21/07 - BR23050
		//        // The origin cell could be null
		//        //this.column = originCell.ColumnIndex + offsetAddress.column;
		//        // MD 4/12/11 - TFS67084
		//        // Moved away from using WorksheetCell objects.
		//        //if ( originCell != null )
		//        //    this.column = originCell.ColumnIndex + offsetAddress.column;
		//        // MD 5/13/11 - Data Validations / Page Breaks
		//        //if (originCellRow != null)
		//        if (originCellColumnIndex >= 0)
		//            this.column = (short)(originCellColumnIndex + offsetAddress.column);

		//        // MD 6/31/08 - Excel 2007 Format
		//        //this.column %= Workbook.MaxExcelColumnCount;
		//        // MD 5/13/11 - Data Validations / Page Breaks
		//        //this.column %= workbook.MaxColumnCount;
		//        short maxColumnCount = Workbook.GetMaxColumnCountInternal(format);
		//        this.column %= maxColumnCount;

		//        if ( this.column < 0 )
		//        {
		//            // MD 6/31/08 - Excel 2007 Format
		//            //this.column += Workbook.MaxExcelColumnCount;
		//            // MD 5/13/11 - Data Validations / Page Breaks
		//            //this.column += workbook.MaxColumnCount;
		//            this.column += maxColumnCount;
		//        }
		//    }
		//    else
		//        this.column = offsetAddress.column;
		//} 

		#endregion  // Removed

		// MD 4/12/11 - TFS67084
		// Use short instead of int so we don't have to cast.
		//public CellAddress( int row, bool rowAddressIsRelative, int column, bool columnAddressIsRelative )
		public CellAddress(int row, bool rowAddressIsRelative, short column, bool columnAddressIsRelative)
		{
			this.row = row;
			this.rowAddressIsRelative = rowAddressIsRelative;
			this.column = column;
			this.columnAddressIsRelative = columnAddressIsRelative;
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 12/22/11 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			CellAddress other = obj as CellAddress;
			if (other == null)
				return false;

			return
				this.row == other.row &&
				this.rowAddressIsRelative == other.rowAddressIsRelative &&
				this.column == other.column &&
				this.columnAddressIsRelative == other.columnAddressIsRelative;
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			int temp = this.column << 16 | this.row;

			if (rowAddressIsRelative)
				temp <<= 1;

			if (columnAddressIsRelative)
				temp <<= 2;

			return temp;
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

		#region Methods

		#region GetCellReferenceString

		// MD 8/12/08
		// Found while implementing Excel formula solving
		#region Not Used

		
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


		#endregion Not Used

		public static string GetCellReferenceString(
			int row,
			int column,
			bool rowAddressIsRelative,
			bool columnAddressIsRelative,
			WorkbookFormat format,	// MD 6/31/08 - Excel 2007 Format

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//IWorksheetCell sourceCell,
			// MD 2/20/12 - 12.1 - Table Support
			//WorksheetRow sourceCellRow, short sourceCellColumnIndex,
			int sourceCellRowIndex, short sourceCellColumnIndex,

			bool relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
			CellReferenceMode cellReferenceMode)
		{
			// MD 5/21/07 - BR23050
			// GetRowString and GetColumnString each have another parameter now, indicating whether relative addresses are offsets
			//string rowString = GetRowString( row, rowAddressIsRelative, sourceCell, cellReferenceMode );
			//string columnString = GetColumnString( column, columnAddressIsRelative, sourceCell, cellReferenceMode );
			// MD 6/31/08 - Excel 2007 Format
			//string rowString = GetRowString( row, rowAddressIsRelative, sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
			//string columnString = GetColumnString( column, columnAddressIsRelative, sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
			string rowString = CellAddress.GetRowString(
				row,
				rowAddressIsRelative,
				format,

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//sourceCell, 
				// MD 2/20/12 - 12.1 - Table Support
				//sourceCellRow, 
				sourceCellRowIndex,

				relativeAddressesAreOffsets,
				cellReferenceMode);
			string columnString = CellAddress.GetColumnString(
				column,
				columnAddressIsRelative,
				format,

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//sourceCell, 
				sourceCellColumnIndex,

				relativeAddressesAreOffsets,
				cellReferenceMode);

			if (cellReferenceMode == CellReferenceMode.A1)
				return columnString + rowString;
			else
				return rowString + columnString;
		}

		#endregion GetCellReferenceString

		#region GetColumnString

		//  BF 8/7/08
		//  Added an overload so that a Workbook reference is not required

		public static string GetColumnString(
			int column,
			bool columnAddressIsRelative,
			WorkbookFormat format,	// MD 6/31/08 - Excel 2007 Format

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//IWorksheetCell sourceCell,
			short sourceCellColumnIndex,

			bool relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
			CellReferenceMode cellReferenceMode)
		{
			int maxColumnCount = Workbook.GetMaxColumnCount(format);

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//return CellAddress.GetColumnString( column, columnAddressIsRelative, maxColumnCount, sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
			return CellAddress.GetColumnString(column, columnAddressIsRelative, maxColumnCount, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode);
		}

		public static string GetColumnString(
			int column,
			bool columnAddressIsRelative,
			int maxColumnCount,

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//IWorksheetCell sourceCell,
			short sourceCellColumnIndex,

			bool relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
			CellReferenceMode cellReferenceMode)
		{
			// MD 5/21/07 - BR23050
			// Refactored the code to account for relative offsets that need to be absolutes and vice versa
			#region Refactored

			//if ( cellReferenceMode == CellReferenceMode.A1 )
			//{
			//    string columnLetters = ( (char)( 'A' + ( column % 26 ) ) ).ToString();
			//
			//    if ( column >= 26 )
			//        columnLetters = ( (char)( 'A' + ( column / 26 ) - 1 ) ) + columnLetters;
			//
			//    if ( columnAddressIsRelative == false )
			//        return "$" + columnLetters;
			//
			//    return columnLetters;
			//}
			//else
			//{
			//    int columnValue = column;
			//
			//    if ( sourceCell != null && columnAddressIsRelative )
			//        columnValue = column - sourceCell.ColumnIndex;
			//
			//    if ( columnValue == 0 && columnAddressIsRelative )
			//        return "C";
			//
			//    if ( columnAddressIsRelative )
			//        return "C[" + columnValue + "]";
			//    else
			//        return "C" + ( columnValue + 1 );
			//}

			#endregion Refactored

			// Make sure the column index is within the absolute value range of the column count
			// MD 6/31/08 - Excel 2007 Format
			//column %= Workbook.MaxExcelColumnCount;
			column %= maxColumnCount;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//int originColumnIndex = 0;
			//
			//if ( sourceCell != null )
			//    originColumnIndex = sourceCell.ColumnIndex;
			int originColumnIndex = Math.Max(0, (int)sourceCellColumnIndex);

			if (columnAddressIsRelative)
			{
				if (relativeAddressesAreOffsets)
				{
					// If relative addresses are offsets, make sure the offset will not have to wrap around the worksheet
					// MD 6/31/08 - Excel 2007 Format
					//if ( column + originColumnIndex < 0 )
					//    column += Workbook.MaxExcelColumnCount;
					//else if ( column + originColumnIndex >= Workbook.MaxExcelColumnCount )
					//    column -= Workbook.MaxExcelColumnCount;
					if (column + originColumnIndex < 0)
						column += maxColumnCount;
					else if (column + originColumnIndex >= maxColumnCount)
						column -= maxColumnCount;
				}
				else
				{
					// If relative addresses are absolute, make sure the absolute column index is valid in the worksheet
					if (column < 0)
					{
						// MD 6/31/08 - Excel 2007 Format
						//column += Workbook.MaxExcelColumnCount;
						column += maxColumnCount;
					}
				}
			}

			if (cellReferenceMode == CellReferenceMode.A1)
			{
				// A1 references need to be absolute, change them if they are not
				if (columnAddressIsRelative && relativeAddressesAreOffsets)
				{
					// Convert the column offset from the origin to an absolute cell reference
					column += originColumnIndex;
				}

				// MD 7/23/08 - Excel formula solving
				// This code only allowed 2 character column addresses. We must now support 3 character column addresses.
				// The new code below allows for any number of characters.
				//string columnLetters = ( (char)( 'A' + ( column % 26 ) ) ).ToString();
				//
				//if ( column >= 26 )
				//	columnLetters = ( (char)( 'A' + ( column / 26 ) - 1 ) ) + columnLetters;

				// MD 8/22/08 - Excel formula solving - Performance
				// We don't need a stringbuild, there are only 3 characters at most.
				//StringBuilder columnLettersBuilder = new StringBuilder();
				string columnLetters = string.Empty;

				bool isLowOrderLetter = true;

				// Determine the column leters in reverse order.
				do
				{
					char currentLetter = (char)('A' + (column % 26));
					column /= 26;

					// The non-low order letters are off by one because an absence of the letter is actually their first value.
					// 'A' is the second value, 'B' is the thrid, and so on. This is not the case for the low order letter,
					// because 'A' is the first value, 'B' is the second, and so on.
					if (isLowOrderLetter == false)
					{
						// MD 6/8/10 - TFS33389
						// In the Excel 2007 format, there can now be three letters in the address. If the second letter is 'A' here and we
						// need to decrement it, we need to roll it over to 'Z' and carry one letter value from the higher significant position.
						// Otherwise, just decrement the letter.
						//currentLetter--;
						if (currentLetter == 'A')
						{
							currentLetter = 'Z';
							column--;
						}
						else
						{
							currentLetter--;
						}
					}

					// MD 8/22/08 - Excel formula solving - Performance
					// We don't need a stringbuild, there are only 3 characters at most.
					//columnLettersBuilder.Insert( 0, currentLetter );
					columnLetters = currentLetter + columnLetters;

					isLowOrderLetter = false;
				}
				while (column > 0);

				// MD 8/22/08 - Excel formula solving - Performance
				// We don't need a stringbuild, there are only 3 characters at most.
				//string columnLetters = columnLettersBuilder.ToString();

				if (columnAddressIsRelative == false)
					return "$" + columnLetters;

				return columnLetters;
			}
			else
			{
				if (columnAddressIsRelative == false)
				{
					//return "C" + ( column + 1 ).ToString( CultureInfo.CurrentCulture );
					return "C" + (column + 1).ToString();
				}

				// R1C1 relative references need to be offsets, change them if they are not
				if (relativeAddressesAreOffsets == false)
				{
					// Convert the absolute column index into an offset from the origin column
					column -= originColumnIndex;
				}

				if (column == 0)
					return "C";

				return "C[" + column + "]";
			}
		}

		#endregion GetColumnString

		#region GetRowString

		//  BF 8/7/08
		//  Added an overload so that a Workbook reference is not required

		public static string GetRowString(
			int row,
			bool rowAddressIsRelative,
			WorkbookFormat format,	// MD 6/31/08 - Excel 2007 Format

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//IWorksheetCell sourceCell,
			// MD 2/20/12 - 12.1 - Table Support
			//WorksheetRow sourceCellRow,
			int sourceCellRowIndex,

			bool relativeAddressesAreOffsets,
			CellReferenceMode cellReferenceMode)
		{
			int maxRowCount = Workbook.GetMaxRowCount(format);

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//return GetRowString(row, rowAddressIsRelative, maxRowCount, sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
			// MD 2/20/12 - 12.1 - Table Support
			//return GetRowString(row, rowAddressIsRelative, maxRowCount, sourceCellRow, relativeAddressesAreOffsets, cellReferenceMode);
			return GetRowString(row, rowAddressIsRelative, maxRowCount, sourceCellRowIndex, relativeAddressesAreOffsets, cellReferenceMode);
		}

		public static string GetRowString(
			int row,
			bool rowAddressIsRelative,
			int maxRowCount,

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//IWorksheetCell sourceCell,
			// MD 2/20/12 - 12.1 - Table Support
			//WorksheetRow sourceCellRow,
			int sourceCellRowIndex,

			bool relativeAddressesAreOffsets,
			CellReferenceMode cellReferenceMode)
		{
			// MD 5/21/07 - BR23050
			// Refactored the code to account for relative offsets that need to be absolutes and vice versa
			#region Refactored

			//string rowNumber = ( row + 1 ).ToString( CultureInfo.CurrentCulture );
			//
			//if ( cellReferenceMode == CellReferenceMode.A1 )
			//{
			//    if ( rowAddressIsRelative == false )
			//        return "$" + rowNumber;
			//
			//    return rowNumber;
			//}
			//else
			//{
			//    int rowValue = row;
			//
			//    if ( sourceCell != null && rowAddressIsRelative )
			//        rowValue = row - sourceCell.RowIndex;
			//
			//    if ( rowValue == 0 && rowAddressIsRelative )
			//        return "R";
			//
			//    if ( rowAddressIsRelative )
			//        return "R[" + rowValue + "]";
			//    else
			//        return "R" + ( rowValue + 1 );
			//}

			#endregion Refactored

			// Make sure the row index is within the absolute value range of the row count
			// MD 6/31/08 - Excel 2007 Format
			//row %= Workbook.MaxExcelRowCount;
			//int maxRowCount = workbook.MaxRowCount;
			row %= maxRowCount;

			int originRowIndex = 0;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if ( sourceCell != null )
			//    originRowIndex = sourceCell.RowIndex;
			// MD 2/20/12 - 12.1 - Table Support
			//if (sourceCellRow != null)
			//    originRowIndex = sourceCellRow.Index;
			if (sourceCellRowIndex != -1)
				originRowIndex = sourceCellRowIndex;

			if (rowAddressIsRelative)
			{
				if (relativeAddressesAreOffsets)
				{
					// If relative addresses are offsets, make sure the offset will not have to wrap around the worksheet
					// MD 6/31/08 - Excel 2007 Format
					//if ( row + originRowIndex < 0 )
					//    row += Workbook.MaxExcelRowCount;
					//else if ( row + originRowIndex >= Workbook.MaxExcelRowCount )
					//    row -= Workbook.MaxExcelRowCount;
					if (row + originRowIndex < 0)
						row += maxRowCount;
					else if (row + originRowIndex >= maxRowCount)
						row -= maxRowCount;
				}
				else
				{
					// If relative addresses are absolute, make sure the absolute row index is valid in the worksheet
					if (row < 0)
					{
						// MD 6/31/08 - Excel 2007 Format
						//row += Workbook.MaxExcelRowCount;
						row += maxRowCount;
					}
				}
			}

			if (cellReferenceMode == CellReferenceMode.A1)
			{
				if (rowAddressIsRelative == false)
				{
					// MD 4/6/12 - TFS101506
					//return "$" + ( row + 1 ).ToString( CultureInfo.CurrentCulture );
					return "$" + (row + 1).ToString();
				}

				// A1 references need to be absolute, change them if they are not
				if (relativeAddressesAreOffsets)
				{
					// Convert the row offset from the origin to an absolute cell reference
					row += originRowIndex;
				}

				// MD 4/6/12 - TFS101506
				//return ( row + 1 ).ToString( CultureInfo.CurrentCulture );
				return (row + 1).ToString();
			}
			else
			{
				if (rowAddressIsRelative == false)
				{
					// MD 4/6/12 - TFS101506
					//return "R" + ( row + 1 ).ToString( CultureInfo.CurrentCulture );
					return "R" + (row + 1).ToString();
				}

				// R1C1 relative references need to be offsets, change them if they are not
				if (relativeAddressesAreOffsets == false)
				{
					// Convert the absolute row index into an offset from the origin row
					row -= originRowIndex;
				}

				if (row == 0)
					return "R";

				// MD 4/6/12 - TFS101506
				//return "R[" + row.ToString( CultureInfo.CurrentCulture ) + "]";
				return "R[" + row.ToString() + "]";
			}
		}

		#endregion GetRowString

		#region Offset

		// MD 10/22/10 - TFS36696
		// Now that the CellAddress is immutable, we can't change the row or column values.
		//public void Offset( Point offset )
		//{
		//    if ( this.columnAddressIsRelative )
		//        this.column += (int)offset.X;
		//
		//    if ( this.rowAddressIsRelative )
		//        this.row += (int)offset.Y;
		//}
		// MD 7/12/12 - TFS109194
		// Offset now needs a context so it knows the row/column counts for wrapping purposes.
		//public CellAddress Offset(Point offset)
		public CellAddress Offset(FormulaContext context, Point offset)
		{
			if ((this.columnAddressIsRelative == false || offset.X == 0) &&
				(this.rowAddressIsRelative == false || offset.Y == 0))
			{
				return this;
			}

			// MD 4/12/11 - TFS67084
			// Use short instead of int so we don't have to cast.
			//int newColumn = this.column;
			//if (this.columnAddressIsRelative)
			//    newColumn += (int)offset.X;
			short newColumn = this.column;
			if (this.columnAddressIsRelative)
			{
				newColumn += (short)offset.X;

				// MD 7/12/12 - TFS109194
				// We may need to wrap the indexes.
				short columnCount = Workbook.GetMaxColumnCountInternal( context.Format);
				if (newColumn < 0)
					newColumn += columnCount;
				else if (columnCount <= newColumn)
					newColumn -= columnCount;
			}

			int newRow = this.row;
			if (this.rowAddressIsRelative)
			{
				newRow += (int)offset.Y;

				// MD 7/12/12 - TFS109194
				// We may need to wrap the indexes.
				int rowCount = Workbook.GetMaxRowCount(context.Format);
				if (newRow < 0)
					newRow += rowCount;
				else if (rowCount <= newRow)
					newRow -= rowCount;
			}

			return new CellAddress(newRow, this.rowAddressIsRelative, newColumn, this.columnAddressIsRelative);
		}

		#endregion Offset

		#region ToAbsolute

		public CellAddress ToAbsolute(FormulaContext context)
		{
			int row = this.row;
			short column = this.column;

			if (this.rowAddressIsRelative)
			{
				if (context.OwningCellAddress != WorksheetCellAddress.InvalidReference)
					row = context.OwningCellAddress.RowIndex + this.row;

				int maxRowCount = Workbook.GetMaxRowCount(context.Format);
				row %= maxRowCount;

				if (row < 0)
					row += maxRowCount;
			}

			if (this.columnAddressIsRelative)
			{
				if (context.OwningCellAddress != WorksheetCellAddress.InvalidReference)
					column = (short)(context.OwningCellAddress.ColumnIndex + this.column);

				short maxColumnCount = Workbook.GetMaxColumnCountInternal(context.Format);
				column %= maxColumnCount;

				if (column < 0)
					column += maxColumnCount;
			}

			return new CellAddress(row, this.rowAddressIsRelative, column, this.columnAddressIsRelative);
		}

		#endregion  // ToAbsolute

		// MD 6/18/12 - TFS102878
		#region ToAbsoluteAddress

		public WorksheetCellAddress ToAbsoluteAddress(FormulaContext context, bool relativeAddressesAreOffsets)
		{
			short columnIndex = this.Column;
			int rowIndex = this.Row;

			if (relativeAddressesAreOffsets)
			{
				if (this.ColumnAddressIsRelative)
				{
					if (context.OwningCellAddress.IsValid == false)
						return WorksheetCellAddress.InvalidReference;

					columnIndex += context.OwningCellAddress.ColumnIndex;

					short maxColumns = Workbook.GetMaxColumnCountInternal(context.Format);
					if (columnIndex < 0)
						columnIndex += maxColumns;
					else if (maxColumns <= columnIndex)
						columnIndex -= maxColumns;
				}

				if (this.RowAddressIsRelative)
				{
					if (context.OwningCellAddress.IsValid == false)
						return WorksheetCellAddress.InvalidReference;

					rowIndex += context.OwningCellAddress.RowIndex;

					int maxRows = Workbook.GetMaxRowCount(context.Format);
					if (rowIndex < 0)
						rowIndex += maxRows;
					else if (maxRows <= rowIndex)
						rowIndex -= maxRows;
				}
			}

			return new WorksheetCellAddress(rowIndex, columnIndex);
		}

		#endregion // ToAbsoluteAddress

		#region ToOffset

		public CellAddress ToOffset(FormulaContext context)
		{
			int row = this.row;
			short column = this.column;

			if (this.rowAddressIsRelative)
			{
				if (context.OwningCellAddress != WorksheetCellAddress.InvalidReference)
					row = this.row - context.OwningCellAddress.RowIndex;

				int maxRowCount = Workbook.GetMaxRowCount(context.Format);
				if (row < 0)
					row += maxRowCount;
			}

			if (this.columnAddressIsRelative)
			{
				if (context.OwningCellAddress != WorksheetCellAddress.InvalidReference)
					column = (short)(this.column - context.OwningCellAddress.ColumnIndex);

				short maxColumnCount = Workbook.GetMaxColumnCountInternal(context.Format);
				if (column < 0)
					column += maxColumnCount;
			}

			return new CellAddress(row, this.rowAddressIsRelative, column, this.columnAddressIsRelative);
		}

		#endregion  // ToOffset

		#region ToString

		// MD 6/31/08 - Excel 2007 Format
		//public string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode )
		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, WorkbookFormat format )
		// MD 2/20/12 - 12.1 - Table Support
		//public string ToString(WorksheetRow sourceCellRow, short sourceCellColumnIndex, CellReferenceMode cellReferenceMode, WorkbookFormat format)
		public string ToString(int sourceCellRowIndex, short sourceCellColumnIndex, CellReferenceMode cellReferenceMode, WorkbookFormat format)
		{
			// MD 5/21/07 - BR23050
			// Moved code to the other overload
			// MD 6/31/08 - Excel 2007 Format
			//return this.ToString( sourceCell, false, cellReferenceMode );
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//return this.ToString( sourceCell, false, cellReferenceMode, format );
			// MD 2/20/12 - 12.1 - Table Support
			//return this.ToString(sourceCellRow, sourceCellColumnIndex, false, cellReferenceMode, format);
			return this.ToString(sourceCellRowIndex, sourceCellColumnIndex, false, cellReferenceMode, format);
		}

		// MD 5/21/07 - BR23050
		// Added a parameter to determine if relative addresses are offsets from the origin cell's address
		// MD 6/31/08 - Excel 2007 Format
		//public string ToString( IWorksheetCell sourceCell, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode )
		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public string ToString( IWorksheetCell sourceCell, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format )
		// MD 2/20/12 - 12.1 - Table Support
		//public string ToString(WorksheetRow sourceCellRow, short sourceCellColumnIndex, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format)
		public string ToString(int sourceCellRowIndex, short sourceCellColumnIndex, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format)
		{
			return GetCellReferenceString(
				this.row,
				this.column,
				this.rowAddressIsRelative,
				this.columnAddressIsRelative,
				format,	// MD 6/31/08 - Excel 2007 Format
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//sourceCell,
				// MD 2/20/12 - 12.1 - Table Support
				//sourceCellRow, sourceCellColumnIndex,
				sourceCellRowIndex, sourceCellColumnIndex,
				relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
				cellReferenceMode);
		}

		#endregion ToString

		#region VerifyFormatLimits

		public void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		{
			int maxColumnIndex = Workbook.GetMaxColumnCount(testFormat) - 1;

			if (maxColumnIndex < Math.Abs(this.column))
			{
				limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxColumnIndex"), this.column, maxColumnIndex));
				return;
			}

			int maxRowIndex = Workbook.GetMaxRowCount(testFormat) - 1;

			if (maxRowIndex < Math.Abs(this.row))
				limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxRowIndex"), this.row, maxRowIndex));
		}

		#endregion VerifyFormatLimits

		#endregion Methods

		#region Properties

		#region Column

		// MD 4/12/11 - TFS67084
		// Use short instead of int so we don't have to cast.
		//public int Column
		public short Column
		{
			get { return this.column; }
		}

		#endregion Column

		#region ColumnAddressIsRelative

		public bool ColumnAddressIsRelative
		{
			get { return this.columnAddressIsRelative; }
		}

		#endregion ColumnAddressIsRelative

		#region HasRelativeAddresses

		public bool HasRelativeAddresses
		{
			get { return this.columnAddressIsRelative || this.rowAddressIsRelative; }
		}

		#endregion HasRelativeAddresses

		#region Row

		public int Row
		{
			get { return this.row; }
		}

		#endregion Row

		#region RowAddressIsRelative

		public bool RowAddressIsRelative
		{
			get { return this.rowAddressIsRelative; }
		}

		#endregion RowAddressIsRelative

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