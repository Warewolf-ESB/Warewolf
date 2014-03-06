using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;




using System.Drawing;


namespace Infragistics.Documents.Excel.FormulaUtilities
{
	#region Old Code

	//    internal class CellAddressRange
	//    {
	//        #region Member Variables

	//        // MD 10/22/10 - TFS36696
	//        // The CellAddress is now immutable.
	//        //private CellAddress topLeftCellAddress;
	//        //private CellAddress bottomRightCellAddress;
	//        private readonly CellAddress topLeftCellAddress;
	//        private readonly CellAddress bottomRightCellAddress;

	//        #endregion Member Variables

	//        #region Constructor

	//        // MD 9/19/11 - TFS86108
	//        // Removed because this constructor doesn't imply whether we are converting to an absolute or an offset address.
	//        // Now there are ToAbsolute and ToOffset methods which are not ambiguous.
	//        #region Removed

	//        //// MD 5/21/07 - BR23050
	//        //// Changed the cell type because it may be a merged region
	//        ////public CellAddressRange( WorksheetCell originCell, CellAddressRange offsetRange )
	//        //// MD 6/31/08 - Excel 2007 Format
	//        ////public CellAddressRange( IWorksheetCell originCell, CellAddressRange offsetRange )
	//        //// MD 4/12/11 - TFS67084
	//        //// Moved away from using WorksheetCell objects.
	//        ////public CellAddressRange( Workbook workbook, IWorksheetCell originCell, CellAddressRange offsetRange )
	//        //// MD 5/13/11 - Data Validations / Page Breaks
	//        //// This method doesn't need the row instance, just the row index.
	//        //// Also, some callers don't have a workbook. Since the workbook is only needed to get max column and row counts, we could get that from a format instead.
	//        ////public CellAddressRange(Workbook workbook, WorksheetRow originCellRow, short originCellColumnIndex, CellAddressRange offsetRange)
	//        //public CellAddressRange(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex, CellAddressRange offsetRange)
	//        //{
	//        //    // MD 6/31/08 - Excel 2007 Format
	//        //    //this.topLeftCellAddress = new CellAddress( originCell, offsetRange.topLeftCellAddress );
	//        //    //this.bottomRightCellAddress = new CellAddress( originCell, offsetRange.bottomRightCellAddress );
	//        //    // MD 4/12/11 - TFS67084
	//        //    // Moved away from using WorksheetCell objects.
	//        //    //this.topLeftCellAddress = new CellAddress( workbook, originCell, offsetRange.topLeftCellAddress );
	//        //    //this.bottomRightCellAddress = new CellAddress( workbook, originCell, offsetRange.bottomRightCellAddress );
	//        //    // MD 5/13/11 - Data Validations / Page Breaks
	//        //    //this.topLeftCellAddress = new CellAddress(workbook, originCellRow, originCellColumnIndex, offsetRange.topLeftCellAddress);
	//        //    //this.bottomRightCellAddress = new CellAddress(workbook, originCellRow, originCellColumnIndex, offsetRange.bottomRightCellAddress);
	//        //    this.topLeftCellAddress = new CellAddress(format, originCellRowIndex, originCellColumnIndex, offsetRange.topLeftCellAddress);
	//        //    this.bottomRightCellAddress = new CellAddress(format, originCellRowIndex, originCellColumnIndex, offsetRange.bottomRightCellAddress);
	//        //}

	//        #endregion  // Removed

	//        public CellAddressRange( CellAddress cell1, CellAddress cell2 )
	//        {
	//            int topRow;
	//            bool topRowIsRelative;
	//            int bottomRow;
	//            bool bottomRowIsRelative;

	//            // MD 4/12/11 - TFS67084
	//            // Use short instead of int so we don't have to cast.
	//            //int leftColumn;
	//            //bool leftColumnIsRelative;
	//            //int rightColumn;
	//            //bool rightColumnIsRelative;
	//            short leftColumn;
	//            bool leftColumnIsRelative;
	//            short rightColumn;
	//            bool rightColumnIsRelative;

	//            if ( cell1.Row <= cell2.Row )
	//            {
	//                topRow = cell1.Row;
	//                topRowIsRelative = cell1.RowAddressIsRelative;
	//                bottomRow = cell2.Row;
	//                bottomRowIsRelative = cell2.RowAddressIsRelative;
	//            }
	//            else
	//            {
	//                topRow = cell2.Row;
	//                topRowIsRelative = cell2.RowAddressIsRelative;
	//                bottomRow = cell1.Row;
	//                bottomRowIsRelative = cell1.RowAddressIsRelative;
	//            }

	//            if ( cell1.Column <= cell2.Column )
	//            {
	//                leftColumn = cell1.Column;
	//                leftColumnIsRelative = cell1.ColumnAddressIsRelative;
	//                rightColumn = cell2.Column;
	//                rightColumnIsRelative = cell2.ColumnAddressIsRelative;
	//            }
	//            else
	//            {
	//                leftColumn = cell2.Column;
	//                leftColumnIsRelative = cell2.ColumnAddressIsRelative;
	//                rightColumn = cell1.Column;
	//                rightColumnIsRelative = cell1.ColumnAddressIsRelative;
	//            }

	//            this.topLeftCellAddress = new CellAddress( topRow, topRowIsRelative, leftColumn, leftColumnIsRelative );
	//            this.bottomRightCellAddress = new CellAddress( bottomRow, bottomRowIsRelative, rightColumn, rightColumnIsRelative );
	//        }

	//        // MD 2/1/11 - Data Validation support
	//        public CellAddressRange(WorksheetRegion region)
	//        {
	//            this.topLeftCellAddress = new CellAddress(region.FirstRow, false, region.FirstColumnInternal, false);
	//            this.bottomRightCellAddress = new CellAddress(region.LastRow, false, region.LastColumnInternal, false);
	//        }

	//        #endregion Constructor

	//        #region Base Class Overrides

	//        // MD 12/22/11 - 12.1 - Table Support
	//        #region Equals

	//        public override bool Equals(object obj)
	//        {
	//            CellAddressRange other = obj as CellAddressRange;
	//            if (other == null)
	//                return false;

	//            return
	//                this.topLeftCellAddress.Equals(other.topLeftCellAddress) &&
	//                this.bottomRightCellAddress.Equals(other.bottomRightCellAddress);
	//        }

	//        #endregion // Equals

	//        #region GetHashCode

	//        public override int GetHashCode()
	//        {
	//            return
	//                this.topLeftCellAddress.GetHashCode() ^
	//                (this.bottomRightCellAddress.GetHashCode() << 1);
	//        }

	//        #endregion // GetHashCode

	//        #endregion // Base Class Overrides

	//        #region Methods

	//        // MD 10/22/10 - TFS36696
	//        // This never needs to be cloned because it is immutable.
	//        //#region Clone
	//        //
	//        //public CellAddressRange Clone()
	//        //{
	//        //    return new CellAddressRange(this.topLeftCellAddress.Clone(), this.bottomRightCellAddress.Clone());
	//        //} 
	//        //
	//        //#endregion Clone

	//        // MD 8/18/08 - Excel formula solving
	//        #region GetTargetRegion

	//#if DEBUG
	//        /// <summary>
	//        /// Gets the range being addressed when this address is used from the specified formula owner.
	//        /// </summary>
	//        /// <param name="formulaOwnerRow">The row of the owning cell of the formula whose tokens use this address.</param>
	//        /// <param name="formulaOwnerColumnIndex">The column index of the owning cell of the formula whose tokens use this address.</param>
	//        /// <param name="relativeAddressesAreOffsets">Indicates whether relative address are offsets from the formula owner.</param>
	//        /// <returns>The range referenced by this address from the same worksheet as the formula owner.</returns>  
	//#endif
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public WorksheetRegion GetTargetRegion( IWorksheetCell formulaOwner, bool relativeAddressesAreOffsets )
	//        //{
	//        //    return this.GetTargetRegion( formulaOwner.Worksheet, formulaOwner, relativeAddressesAreOffsets );
	//        //}
	//        public WorksheetRegion GetTargetRegion(WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex, bool relativeAddressesAreOffsets)
	//        {
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //return this.GetTargetRegion(formulaOwnerRow.Worksheet, formulaOwnerRow, formulaOwnerColumnIndex, relativeAddressesAreOffsets);
	//            return this.GetTargetRegion(formulaOwnerRow.Worksheet, formulaOwnerRow.Index, formulaOwnerColumnIndex, relativeAddressesAreOffsets);
	//        }

	//#if DEBUG
	//        /// <summary>
	//        /// Gets the range being addressed when this address is used from the specified formula owner.
	//        /// </summary>
	//        /// <param name="worksheet">The worksheet on which the addressed range resides.</param>
	//        /// <param name="formulaOwnerRowIndex">The row index of the owning cell of the formula whose tokens use this address.</param>
	//        /// <param name="formulaOwnerColumnIndex">The column index of the owning cell of the formula whose tokens use this address.</param>
	//        /// <param name="relativeAddressesAreOffsets">Indicates whether relative address are offsets from the formula owner.</param>
	//        /// <returns>The range referenced by this address.</returns>  
	//#endif
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public WorksheetRegion GetTargetRegion( Worksheet worksheet, IWorksheetCell formulaOwner, bool relativeAddressesAreOffsets )
	//        // MD 2/20/12 - 12.1 - Table Support
	//        //public WorksheetRegion GetTargetRegion(Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex, bool relativeAddressesAreOffsets)
	//        public WorksheetRegion GetTargetRegion(Worksheet worksheet, int formulaOwnerRowIndex, short formulaOwnerColumnIndex, bool relativeAddressesAreOffsets)
	//        {
	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //int topLeftColumn = this.TopLeftCellAddress.GetAbsoluteColumnIndex( formulaOwner, relativeAddressesAreOffsets );
	//            //int topLeftRow = this.TopLeftCellAddress.GetAbsoluteRowIndex( formulaOwner, relativeAddressesAreOffsets );
	//            //int bottomRightColumn = this.BottomRightCellAddress.GetAbsoluteColumnIndex( formulaOwner, relativeAddressesAreOffsets );
	//            //int bottomRightRow = this.BottomRightCellAddress.GetAbsoluteRowIndex( formulaOwner, relativeAddressesAreOffsets );
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //int topLeftColumn = this.TopLeftCellAddress.GetAbsoluteColumnIndex(formulaOwnerRow, formulaOwnerColumnIndex, relativeAddressesAreOffsets);
	//            //int topLeftRow = this.TopLeftCellAddress.GetAbsoluteRowIndex(formulaOwnerRow, relativeAddressesAreOffsets);
	//            //int bottomRightColumn = this.BottomRightCellAddress.GetAbsoluteColumnIndex(formulaOwnerRow, formulaOwnerColumnIndex, relativeAddressesAreOffsets);
	//            //int bottomRightRow = this.BottomRightCellAddress.GetAbsoluteRowIndex(formulaOwnerRow, relativeAddressesAreOffsets);
	//            WorkbookFormat currentFormat = worksheet.CurrentFormat;
	//            int topLeftColumn = this.TopLeftCellAddress.GetAbsoluteColumnIndex(currentFormat, formulaOwnerColumnIndex, relativeAddressesAreOffsets);
	//            int topLeftRow = this.TopLeftCellAddress.GetAbsoluteRowIndex(currentFormat, formulaOwnerRowIndex, relativeAddressesAreOffsets);
	//            int bottomRightColumn = this.BottomRightCellAddress.GetAbsoluteColumnIndex(currentFormat, formulaOwnerColumnIndex, relativeAddressesAreOffsets);
	//            int bottomRightRow = this.BottomRightCellAddress.GetAbsoluteRowIndex(currentFormat, formulaOwnerRowIndex, relativeAddressesAreOffsets);

	//            // MD 2/24/12 - 12.1 - Table Support
	//            if (topLeftColumn < 0 || topLeftRow < 0 || bottomRightColumn < 0 || bottomRightRow < 0)
	//                return null;

	//            // MD 11/13/09 - TFS24818
	//            // If the relative addresses are offsets, we could have sorted the cell addresses in the correct order in the constructor
	//            // still have them in the wrong order after resolving them. For example, in an Excel 97-2003 format, if you have a formula
	//            // of =SUM(RC[1]:R[65535]C[1]), the row of the second cell in the range will correctly be the bottom right row. If you apply 
	//            // the formula to a cell in the top row, everything will work fine and the second cell in the range will be at the bottom of
	//            // the worksheet. However, if you apply the formula to any other cell, teh second cell in the range will be in the row above
	//            // the owning cell (due to wrapping of relative references). Therefore, we must verify that resolved rows and columns are 
	//            // in the correct order when there are relative references and they are used for offsets.
	//            if ( relativeAddressesAreOffsets )
	//            {
	//                if ( this.TopLeftCellAddress.ColumnAddressIsRelative || this.BottomRightCellAddress.ColumnAddressIsRelative )
	//                {
	//                    if ( bottomRightColumn < topLeftColumn )
	//                        Utilities.SwapValues( ref bottomRightColumn, ref topLeftColumn );
	//                }

	//                if ( this.TopLeftCellAddress.RowAddressIsRelative || this.BottomRightCellAddress.RowAddressIsRelative )
	//                {
	//                    if ( bottomRightRow < topLeftRow )
	//                        Utilities.SwapValues( ref bottomRightRow, ref topLeftRow );
	//                }
	//            }

	//            return worksheet.GetCachedRegion( topLeftRow, topLeftColumn, bottomRightRow, bottomRightColumn );
	//        } 

	//        #endregion GetTargetRegion

	//        #region Offset

	//        // MD 10/22/10 - TFS36696
	//        // Now that the CellAddress is immutable, we can't change the values.
	//        //public void Offset( Point offset )
	//        //{
	//        //    this.bottomRightCellAddress.Offset( offset );
	//        //    this.topLeftCellAddress.Offset( offset );
	//        //} 
	//        public CellAddressRange Offset(Point offset)
	//        {
	//            CellAddress newBottomRightCellAddress = this.bottomRightCellAddress.Offset(offset);
	//            CellAddress newTopLeftCellAddress = this.topLeftCellAddress.Offset(offset);

	//            if (newBottomRightCellAddress == this.bottomRightCellAddress &&
	//                newTopLeftCellAddress == this.topLeftCellAddress)
	//            {
	//                return this;
	//            }

	//            return new CellAddressRange(newTopLeftCellAddress, newBottomRightCellAddress);
	//        }

	//        #endregion Offset

	//        // MD 9/19/11 - TFS86108
	//        #region ToAbsolute

	//        public CellAddressRange ToAbsolute(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex)
	//        {
	//            return new CellAddressRange(
	//                this.topLeftCellAddress.ToAbsolute(format, originCellRowIndex, originCellColumnIndex),
	//                this.bottomRightCellAddress.ToAbsolute(format, originCellRowIndex, originCellColumnIndex));
	//        }

	//        #endregion  // ToAbsolute

	//        // MD 9/19/11 - TFS86108
	//        #region ToOffset

	//        public CellAddressRange ToOffset(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex)
	//        {
	//            return new CellAddressRange(
	//                this.topLeftCellAddress.ToOffset(format, originCellRowIndex, originCellColumnIndex),
	//                this.bottomRightCellAddress.ToOffset(format, originCellRowIndex, originCellColumnIndex));
	//        }

	//        #endregion  // ToOffset

	//        #region ToString

	//        // MD 5/21/07 - BR23050
	//        // Added a parameter to determine if relative addresses are offsets from the origin cell's address
	//        //public string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode )
	//        // MD 6/31/08 - Excel 2007 Format
	//        //public string ToString( IWorksheetCell sourceCell, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode )
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public string ToString( IWorksheetCell sourceCell, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format )
	//        // MD 2/20/12 - 12.1 - Table Support
	//        //public string ToString(WorksheetRow sourceCellRow, short sourceCellColumnIndex, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format)
	//        public string ToString(int sourceCellRowIndex, short sourceCellColumnIndex, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format)
	//        {
	//            if ( this.topLeftCellAddress.Column == 0 &&
	//                // MD 7/2/08 - Excel 2007 Format
	//                //this.bottomRightCellAddress.Column == CellAddress.MaxColumnIndex )
	//                this.bottomRightCellAddress.Column == Workbook.GetMaxColumnCount( format ) - 1 )
	//            {
	//                string topLeftRowString = CellAddress.GetRowString(
	//                    topLeftCellAddress.Row,
	//                    topLeftCellAddress.RowAddressIsRelative,
	//                    format,	// MD 6/31/08 - Excel 2007 Format
	//                    // MD 4/12/11 - TFS67084
	//                    // Moved away from using WorksheetCell objects.
	//                    //sourceCell,
	//                    // MD 2/20/12 - 12.1 - Table Support
	//                    //sourceCellRow,
	//                    sourceCellRowIndex,
	//                    relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
	//                    cellReferenceMode );

	//                string bottomRightRowString = CellAddress.GetRowString(
	//                    bottomRightCellAddress.Row,
	//                    bottomRightCellAddress.RowAddressIsRelative,
	//                    format,	// MD 6/31/08 - Excel 2007 Format
	//                    // MD 4/12/11 - TFS67084
	//                    // Moved away from using WorksheetCell objects.
	//                    //sourceCell,
	//                    // MD 2/20/12 - 12.1 - Table Support
	//                    //sourceCellRow,
	//                    sourceCellRowIndex,
	//                    relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
	//                    cellReferenceMode );

	//                if ( cellReferenceMode == CellReferenceMode.R1C1 && this.topLeftCellAddress.Row == this.bottomRightCellAddress.Row )
	//                    return topLeftRowString;

	//                return topLeftRowString + FormulaParser.RangeOperator + bottomRightRowString;
	//            }

	//            if ( this.topLeftCellAddress.Row == 0 &&
	//                // MD 7/2/08 - Excel 2007 Format
	//                //this.bottomRightCellAddress.Row == CellAddress.MaxRowIndex )
	//                this.bottomRightCellAddress.Row == Workbook.GetMaxRowCount( format ) - 1 )
	//            {
	//                // MD 9/2/08 - bad copy paste error
	//                //string topLeftColumnString = CellAddress.GetRowString(
	//                string topLeftColumnString = CellAddress.GetColumnString(
	//                    topLeftCellAddress.Column,
	//                    topLeftCellAddress.ColumnAddressIsRelative,
	//                    format,	// MD 6/31/08 - Excel 2007 Format
	//                    // MD 4/12/11 - TFS67084
	//                    // Moved away from using WorksheetCell objects.
	//                    //sourceCell,
	//                    sourceCellColumnIndex,
	//                    relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
	//                    cellReferenceMode );

	//                // MD 9/2/08 - bad copy paste error
	//                //string bottomRightColumnString = CellAddress.GetRowString(
	//                string bottomRightColumnString = CellAddress.GetColumnString(
	//                    bottomRightCellAddress.Column,
	//                    bottomRightCellAddress.ColumnAddressIsRelative,
	//                    format,	// MD 6/31/08 - Excel 2007 Format
	//                    // MD 4/12/11 - TFS67084
	//                    // Moved away from using WorksheetCell objects.
	//                    //sourceCell,
	//                    sourceCellColumnIndex,
	//                    relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
	//                    cellReferenceMode );

	//                if ( cellReferenceMode == CellReferenceMode.R1C1 && this.topLeftCellAddress.Column == this.bottomRightCellAddress.Column )
	//                    return topLeftColumnString;

	//                return topLeftColumnString + FormulaParser.RangeOperator + bottomRightColumnString;
	//            }

	//            // MD 5/21/07 - BR23050
	//            // Pass in new parameter which indicates whether relative addresses are offsets from the origin cell
	//            //return
	//            //    this.topLeftCellAddress.ToString( sourceCell, cellReferenceMode ) +
	//            //    FormulaParser.RangeOperator +
	//            //    this.bottomRightCellAddress.ToString( sourceCell, cellReferenceMode );
	//            // MD 6/31/08 - Excel 2007 Format
	//            //return
	//            //    this.topLeftCellAddress.ToString( sourceCell, relativeAddressesAreOffsets, cellReferenceMode ) +
	//            //    FormulaParser.RangeOperator +
	//            //    this.bottomRightCellAddress.ToString( sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //return
	//            //    this.topLeftCellAddress.ToString( sourceCell, relativeAddressesAreOffsets, cellReferenceMode, format ) +
	//            //    FormulaParser.RangeOperator +
	//            //    this.bottomRightCellAddress.ToString( sourceCell, relativeAddressesAreOffsets, cellReferenceMode, format );
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //return
	//            //    this.topLeftCellAddress.ToString(sourceCellRow, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode, format) +
	//            //    FormulaParser.RangeOperator +
	//            //    this.bottomRightCellAddress.ToString(sourceCellRow, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode, format);
	//            return
	//                this.topLeftCellAddress.ToString(sourceCellRowIndex, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode, format) +
	//                FormulaParser.RangeOperator +
	//                this.bottomRightCellAddress.ToString(sourceCellRowIndex, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode, format);
	//        }

	//        #endregion ToString

	//        #region VerifyFormatLimits

	//        public void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
	//        {
	//            this.topLeftCellAddress.VerifyFormatLimits( limitErrors, testFormat );
	//            this.bottomRightCellAddress.VerifyFormatLimits( limitErrors, testFormat );
	//        } 

	//        #endregion VerifyFormatLimits

	//        #endregion Methods

	//        #region Properties

	//        #region BottomRightCellAddress

	//        public CellAddress BottomRightCellAddress
	//        {
	//            get { return this.bottomRightCellAddress; }
	//        }

	//        #endregion BottomRightCellAddress

	//        #region HasRelativeAddresses

	//        public bool HasRelativeAddresses
	//        {
	//            get
	//            {
	//                return
	//                    this.bottomRightCellAddress.HasRelativeAddresses ||
	//                    this.topLeftCellAddress.HasRelativeAddresses;
	//            }
	//        }

	//        #endregion HasRelativeAddresses

	//        #region TopLeftCellAddress

	//        public CellAddress TopLeftCellAddress
	//        {
	//            get { return this.topLeftCellAddress; }
	//        }

	//        #endregion TopLeftCellAddress

	//        #endregion Properties
	//    }

	#endregion // Old Code
	internal class CellAddressRange
	{
		#region Member Variables

		// MD 10/22/10 - TFS36696
		// The CellAddress is now immutable.
		//private CellAddress topLeftCellAddress;
		//private CellAddress bottomRightCellAddress;
		private readonly CellAddress topLeftCellAddress;
		private readonly CellAddress bottomRightCellAddress;

		#endregion Member Variables

		#region Constructor

		// MD 9/19/11 - TFS86108
		// Removed because this constructor doesn't imply whether we are converting to an absolute or an offset address.
		// Now there are ToAbsolute and ToOffset methods which are not ambiguous.
		#region Removed

		//// MD 5/21/07 - BR23050
		//// Changed the cell type because it may be a merged region
		////public CellAddressRange( WorksheetCell originCell, CellAddressRange offsetRange )
		//// MD 6/31/08 - Excel 2007 Format
		////public CellAddressRange( IWorksheetCell originCell, CellAddressRange offsetRange )
		//// MD 4/12/11 - TFS67084
		//// Moved away from using WorksheetCell objects.
		////public CellAddressRange( Workbook workbook, IWorksheetCell originCell, CellAddressRange offsetRange )
		//// MD 5/13/11 - Data Validations / Page Breaks
		//// This method doesn't need the row instance, just the row index.
		//// Also, some callers don't have a workbook. Since the workbook is only needed to get max column and row counts, we could get that from a format instead.
		////public CellAddressRange(Workbook workbook, WorksheetRow originCellRow, short originCellColumnIndex, CellAddressRange offsetRange)
		//public CellAddressRange(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex, CellAddressRange offsetRange)
		//{
		//    // MD 6/31/08 - Excel 2007 Format
		//    //this.topLeftCellAddress = new CellAddress( originCell, offsetRange.topLeftCellAddress );
		//    //this.bottomRightCellAddress = new CellAddress( originCell, offsetRange.bottomRightCellAddress );
		//    // MD 4/12/11 - TFS67084
		//    // Moved away from using WorksheetCell objects.
		//    //this.topLeftCellAddress = new CellAddress( workbook, originCell, offsetRange.topLeftCellAddress );
		//    //this.bottomRightCellAddress = new CellAddress( workbook, originCell, offsetRange.bottomRightCellAddress );
		//    // MD 5/13/11 - Data Validations / Page Breaks
		//    //this.topLeftCellAddress = new CellAddress(workbook, originCellRow, originCellColumnIndex, offsetRange.topLeftCellAddress);
		//    //this.bottomRightCellAddress = new CellAddress(workbook, originCellRow, originCellColumnIndex, offsetRange.bottomRightCellAddress);
		//    this.topLeftCellAddress = new CellAddress(format, originCellRowIndex, originCellColumnIndex, offsetRange.topLeftCellAddress);
		//    this.bottomRightCellAddress = new CellAddress(format, originCellRowIndex, originCellColumnIndex, offsetRange.bottomRightCellAddress);
		//}

		#endregion  // Removed

		public CellAddressRange(CellAddress cell1, CellAddress cell2)
		{
			int topRow;
			bool topRowIsRelative;
			int bottomRow;
			bool bottomRowIsRelative;

			// MD 4/12/11 - TFS67084
			// Use short instead of int so we don't have to cast.
			//int leftColumn;
			//bool leftColumnIsRelative;
			//int rightColumn;
			//bool rightColumnIsRelative;
			short leftColumn;
			bool leftColumnIsRelative;
			short rightColumn;
			bool rightColumnIsRelative;

			if (cell1.Row <= cell2.Row)
			{
				topRow = cell1.Row;
				topRowIsRelative = cell1.RowAddressIsRelative;
				bottomRow = cell2.Row;
				bottomRowIsRelative = cell2.RowAddressIsRelative;
			}
			else
			{
				topRow = cell2.Row;
				topRowIsRelative = cell2.RowAddressIsRelative;
				bottomRow = cell1.Row;
				bottomRowIsRelative = cell1.RowAddressIsRelative;
			}

			if (cell1.Column <= cell2.Column)
			{
				leftColumn = cell1.Column;
				leftColumnIsRelative = cell1.ColumnAddressIsRelative;
				rightColumn = cell2.Column;
				rightColumnIsRelative = cell2.ColumnAddressIsRelative;
			}
			else
			{
				leftColumn = cell2.Column;
				leftColumnIsRelative = cell2.ColumnAddressIsRelative;
				rightColumn = cell1.Column;
				rightColumnIsRelative = cell1.ColumnAddressIsRelative;
			}

			this.topLeftCellAddress = new CellAddress(topRow, topRowIsRelative, leftColumn, leftColumnIsRelative);
			this.bottomRightCellAddress = new CellAddress(bottomRow, bottomRowIsRelative, rightColumn, rightColumnIsRelative);
		}

		// MD 2/1/11 - Data Validation support
		public CellAddressRange(WorksheetRegion region)
		{
			this.topLeftCellAddress = new CellAddress(region.FirstRow, false, region.FirstColumnInternal, false);
			this.bottomRightCellAddress = new CellAddress(region.LastRow, false, region.LastColumnInternal, false);
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 12/22/11 - 12.1 - Table Support
		#region Equals

		public override bool Equals(object obj)
		{
			CellAddressRange other = obj as CellAddressRange;
			if (other == null)
				return false;

			return
				this.topLeftCellAddress.Equals(other.topLeftCellAddress) &&
				this.bottomRightCellAddress.Equals(other.bottomRightCellAddress);
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return
				this.topLeftCellAddress.GetHashCode() ^
				(this.bottomRightCellAddress.GetHashCode() << 1);
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

		#region Methods

		// MD 10/22/10 - TFS36696
		// This never needs to be cloned because it is immutable.
		//#region Clone
		//
		//public CellAddressRange Clone()
		//{
		//    return new CellAddressRange(this.topLeftCellAddress.Clone(), this.bottomRightCellAddress.Clone());
		//} 
		//
		//#endregion Clone

		// MD 8/18/08 - Excel formula solving
		#region GetTargetRegion

		// MD 6/16/12 - CalcEngineRefactor
		public WorksheetRegion GetTargetRegion(FormulaContext context, bool relativeAddressesAreOffsets)
		{
			WorksheetRegionAddress regionAddress = this.ToAbsoluteAddress(context, relativeAddressesAreOffsets);
			if (regionAddress.IsValid == false || context.Worksheet == null)
				return null;

			return context.Worksheet.GetCachedRegion(regionAddress);
		}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		public WorksheetRegion GetTargetRegion(Worksheet worksheet, int formulaOwnerRowIndex, short formulaOwnerColumnIndex, bool relativeAddressesAreOffsets)
		{
			WorkbookFormat format = Workbook.LatestFormat;
			if (worksheet != null)
				format = worksheet.CurrentFormat;

			return this.GetTargetRegion(
				new FormulaContext(worksheet, formulaOwnerRowIndex, formulaOwnerColumnIndex, format, null), 
				relativeAddressesAreOffsets);
		}

		#endregion GetTargetRegion

		#region Offset

		// MD 10/22/10 - TFS36696
		// Now that the CellAddress is immutable, we can't change the values.
		//public void Offset( Point offset )
		//{
		//    this.bottomRightCellAddress.Offset( offset );
		//    this.topLeftCellAddress.Offset( offset );
		//} 
		// MD 7/12/12 - TFS109194
		// Offset now needs a context so it knows the row/column counts for wrapping purposes.
		//public CellAddressRange Offset(Point offset)
		public CellAddressRange Offset(FormulaContext context, Point offset)
		{
			// MD 7/12/12 - TFS109194
			// Offset now needs a context so it knows the row/column counts for wrapping purposes.
			//CellAddress newBottomRightCellAddress = this.bottomRightCellAddress.Offset(offset);
			//CellAddress newTopLeftCellAddress = this.topLeftCellAddress.Offset(offset);
			CellAddress newBottomRightCellAddress = this.bottomRightCellAddress.Offset(context, offset);
			CellAddress newTopLeftCellAddress = this.topLeftCellAddress.Offset(context, offset);

			if (newBottomRightCellAddress == this.bottomRightCellAddress &&
				newTopLeftCellAddress == this.topLeftCellAddress)
			{
				return this;
			}

			return new CellAddressRange(newTopLeftCellAddress, newBottomRightCellAddress);
		}

		#endregion Offset

		// MD 9/19/11 - TFS86108
		#region ToAbsolute

		public CellAddressRange ToAbsolute(FormulaContext context)
		{
			return new CellAddressRange(
				this.topLeftCellAddress.ToAbsolute(context),
				this.bottomRightCellAddress.ToAbsolute(context));
		}

		#endregion  // ToAbsolute

		// MD 6/18/12 - TFS102878
		#region ToAbsoluteAddress

		public WorksheetRegionAddress ToAbsoluteAddress(FormulaContext context, bool relativeAddressesAreOffsets)
		{
			WorksheetCellAddress topLeftAddress = this.TopLeftCellAddress.ToAbsoluteAddress(context, relativeAddressesAreOffsets);
			WorksheetCellAddress bottomRightAddress = this.BottomRightCellAddress.ToAbsoluteAddress(context, relativeAddressesAreOffsets);
			return new WorksheetRegionAddress(topLeftAddress, bottomRightAddress);
		}

		#endregion // ToAbsoluteAddress

		#region ToOffset

		public CellAddressRange ToOffset(FormulaContext context)
		{
			return new CellAddressRange(
				this.topLeftCellAddress.ToOffset(context),
				this.bottomRightCellAddress.ToOffset(context));
		}

		#endregion  // ToOffset

		#region ToString

		// MD 5/21/07 - BR23050
		// Added a parameter to determine if relative addresses are offsets from the origin cell's address
		//public string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode )
		// MD 6/31/08 - Excel 2007 Format
		//public string ToString( IWorksheetCell sourceCell, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode )
		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//public string ToString( IWorksheetCell sourceCell, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format )
		// MD 2/20/12 - 12.1 - Table Support
		//public string ToString(WorksheetRow sourceCellRow, short sourceCellColumnIndex, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format)
		public string ToString(int sourceCellRowIndex, short sourceCellColumnIndex, bool relativeAddressesAreOffsets, CellReferenceMode cellReferenceMode, WorkbookFormat format)
		{
			if (this.topLeftCellAddress.Column == 0 &&
				// MD 7/2/08 - Excel 2007 Format
				//this.bottomRightCellAddress.Column == CellAddress.MaxColumnIndex )
				this.bottomRightCellAddress.Column == Workbook.GetMaxColumnCount(format) - 1)
			{
				string topLeftRowString = CellAddress.GetRowString(
					topLeftCellAddress.Row,
					topLeftCellAddress.RowAddressIsRelative,
					format,	// MD 6/31/08 - Excel 2007 Format
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//sourceCell,
					// MD 2/20/12 - 12.1 - Table Support
					//sourceCellRow,
					sourceCellRowIndex,
					relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
					cellReferenceMode);

				string bottomRightRowString = CellAddress.GetRowString(
					bottomRightCellAddress.Row,
					bottomRightCellAddress.RowAddressIsRelative,
					format,	// MD 6/31/08 - Excel 2007 Format
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//sourceCell,
					// MD 2/20/12 - 12.1 - Table Support
					//sourceCellRow,
					sourceCellRowIndex,
					relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
					cellReferenceMode);

				if (cellReferenceMode == CellReferenceMode.R1C1 && this.topLeftCellAddress.Row == this.bottomRightCellAddress.Row)
					return topLeftRowString;

				return topLeftRowString + FormulaParser.RangeOperator + bottomRightRowString;
			}

			if (this.topLeftCellAddress.Row == 0 &&
				// MD 7/2/08 - Excel 2007 Format
				//this.bottomRightCellAddress.Row == CellAddress.MaxRowIndex )
				this.bottomRightCellAddress.Row == Workbook.GetMaxRowCount(format) - 1)
			{
				// MD 9/2/08 - bad copy paste error
				//string topLeftColumnString = CellAddress.GetRowString(
				string topLeftColumnString = CellAddress.GetColumnString(
					topLeftCellAddress.Column,
					topLeftCellAddress.ColumnAddressIsRelative,
					format,	// MD 6/31/08 - Excel 2007 Format
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//sourceCell,
					sourceCellColumnIndex,
					relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
					cellReferenceMode);

				// MD 9/2/08 - bad copy paste error
				//string bottomRightColumnString = CellAddress.GetRowString(
				string bottomRightColumnString = CellAddress.GetColumnString(
					bottomRightCellAddress.Column,
					bottomRightCellAddress.ColumnAddressIsRelative,
					format,	// MD 6/31/08 - Excel 2007 Format
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//sourceCell,
					sourceCellColumnIndex,
					relativeAddressesAreOffsets, // MD 5/21/07 - BR23050
					cellReferenceMode);

				if (cellReferenceMode == CellReferenceMode.R1C1 && this.topLeftCellAddress.Column == this.bottomRightCellAddress.Column)
					return topLeftColumnString;

				return topLeftColumnString + FormulaParser.RangeOperator + bottomRightColumnString;
			}

			// MD 5/21/07 - BR23050
			// Pass in new parameter which indicates whether relative addresses are offsets from the origin cell
			//return
			//    this.topLeftCellAddress.ToString( sourceCell, cellReferenceMode ) +
			//    FormulaParser.RangeOperator +
			//    this.bottomRightCellAddress.ToString( sourceCell, cellReferenceMode );
			// MD 6/31/08 - Excel 2007 Format
			//return
			//    this.topLeftCellAddress.ToString( sourceCell, relativeAddressesAreOffsets, cellReferenceMode ) +
			//    FormulaParser.RangeOperator +
			//    this.bottomRightCellAddress.ToString( sourceCell, relativeAddressesAreOffsets, cellReferenceMode );
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//return
			//    this.topLeftCellAddress.ToString( sourceCell, relativeAddressesAreOffsets, cellReferenceMode, format ) +
			//    FormulaParser.RangeOperator +
			//    this.bottomRightCellAddress.ToString( sourceCell, relativeAddressesAreOffsets, cellReferenceMode, format );
			// MD 2/20/12 - 12.1 - Table Support
			//return
			//    this.topLeftCellAddress.ToString(sourceCellRow, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode, format) +
			//    FormulaParser.RangeOperator +
			//    this.bottomRightCellAddress.ToString(sourceCellRow, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode, format);
			return
				this.topLeftCellAddress.ToString(sourceCellRowIndex, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode, format) +
				FormulaParser.RangeOperator +
				this.bottomRightCellAddress.ToString(sourceCellRowIndex, sourceCellColumnIndex, relativeAddressesAreOffsets, cellReferenceMode, format);
		}

		#endregion ToString

		#region VerifyFormatLimits

		public void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		{
			this.topLeftCellAddress.VerifyFormatLimits(limitErrors, testFormat);
			this.bottomRightCellAddress.VerifyFormatLimits(limitErrors, testFormat);
		}

		#endregion VerifyFormatLimits

		#endregion Methods

		#region Properties

		#region BottomRightCellAddress

		public CellAddress BottomRightCellAddress
		{
			get { return this.bottomRightCellAddress; }
		}

		#endregion BottomRightCellAddress

		#region HasRelativeAddresses

		public bool HasRelativeAddresses
		{
			get
			{
				return
					this.bottomRightCellAddress.HasRelativeAddresses ||
					this.topLeftCellAddress.HasRelativeAddresses;
			}
		}

		#endregion HasRelativeAddresses

		#region TopLeftCellAddress

		public CellAddress TopLeftCellAddress
		{
			get { return this.topLeftCellAddress; }
		}

		#endregion TopLeftCellAddress

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