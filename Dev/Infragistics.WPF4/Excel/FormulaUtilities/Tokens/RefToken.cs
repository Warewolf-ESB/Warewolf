using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;




using System.Drawing;


namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Cell reference operand. Indicates a reference to a cell in the same worksheet.
	//    /// </summary> 
	//#endif
	//    // MD 5/18/07 - BR23022
	//    //internal class RefToken : FormulaToken
	//    internal class RefToken : CellReferenceToken
	//    {
	//        private CellAddress cellAddress;

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public RefToken( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        public RefToken(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public RefToken( Formula formula, CellAddress cellAddress )
	//        //    : this( formula, TokenClass.Reference )
	//        public RefToken(CellAddress cellAddress)
	//            : this(TokenClass.Reference)
	//        {
	//            this.cellAddress = cellAddress;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public RefToken( Formula formula, CellAddress cellAddress, TokenClass tokenClass )
	//        //    : this( formula, tokenClass )
	//        public RefToken(CellAddress cellAddress, TokenClass tokenClass)
	//            : this(tokenClass)
	//        {
	//            this.cellAddress = cellAddress;
	//        }

	//        // MD 2/28/12 - 12.1 - Table Support
	//        protected virtual CellReferenceToken CreateEquivalentRefErrorToken()
	//        {
	//            return new RefErrToken(this.TokenClass);
	//        }

	//        // MD 9/19/11 - TFS86108
	//        public override FormulaToken GetSharedEquivalent(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex)
	//        {
	//            if (this.CellAddress.HasRelativeAddresses == false)
	//                return this;

	//            return new RefNToken(
	//                this.CellAddress.ToOffset(format, originCellRowIndex, originCellColumnIndex),
	//                this.TokenClass);
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new RefToken( newOwningFormula, this.cellAddress.Clone(), this.TokenClass );
	//        //}
	//        public override FormulaToken GetTokenForClonedFormula()
	//        {
	//            return new RefToken(this.cellAddress, this.TokenClass);
	//        }

	//        // MD 8/18/08 - Excel formula solving
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public override object GetCalcValue(Workbook workbook, IWorksheetCell formulaOwner)
	//        //{
	//        //    if (formulaOwner == null)
	//        //        return null;
	//        //
	//        //    return this.CellAddress.GetTargetCell(formulaOwner, this.AreRelativeAddressesOffsets).CalcReference;
	//        //}
	//        // MD 2/24/12 - 12.1 - Table Support
	//        //public override object GetCalcValue(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            // MD 2/24/12 - 12.1 - Table Support
	//            //if (formulaOwnerRow == null)
	//            //    return null;
	//            //
	//            //// MD 2/20/12 - 12.1 - Table Support
	//            ////short columnIndex = (short)this.CellAddress.GetAbsoluteColumnIndex(formulaOwnerRow, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//            ////int rowIndex = this.CellAddress.GetAbsoluteRowIndex(formulaOwnerRow, this.AreRelativeAddressesOffsets);
	//            //WorkbookFormat currentFormat = workbook.CurrentFormat;
	//            //short columnIndex = (short)this.CellAddress.GetAbsoluteColumnIndex(currentFormat, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//            //int rowIndex = this.CellAddress.GetAbsoluteRowIndex(currentFormat, formulaOwnerRow.Index, this.AreRelativeAddressesOffsets);
	//            //
	//            //return formulaOwnerRow.Worksheet.Rows[rowIndex].GetCellCalcReference(columnIndex);

	//            int formulaOwnerRowIndex = -1;
	//            if (formulaOwnerRow != null)
	//                formulaOwnerRowIndex = formulaOwnerRow.Index;

	//            WorkbookFormat currentFormat = workbook.CurrentFormat;
	//            short columnIndex = (short)this.CellAddress.GetAbsoluteColumnIndex(currentFormat, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//            int rowIndex = this.CellAddress.GetAbsoluteRowIndex(currentFormat, formulaOwnerRowIndex, this.AreRelativeAddressesOffsets);

	//            if (columnIndex < 0 || rowIndex < 0)
	//                return ExcelReferenceError.Instance;

	//            return worksheet.Rows[rowIndex].GetCellCalcReference(columnIndex);
	//        }

	//        // MD 2/24/12 - 12.1 - Table Support
	//        public override bool HasRelativeAddresses
	//        {
	//            get { return this.CellAddress.HasRelativeAddresses; }
	//        }

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            RefToken comparisonRefToken = (RefToken)comparisonToken;

	//            if (this.CellAddress.ColumnAddressIsRelative != comparisonRefToken.CellAddress.ColumnAddressIsRelative)
	//                return false;

	//            if (this.CellAddress.RowAddressIsRelative != comparisonRefToken.CellAddress.RowAddressIsRelative)
	//                return false;

	//            int sourceRowIndex = -1;

	//            // MD 2/24/12
	//            // Found while implementing 12.1 - Table Support
	//            // We should use the least restrictive format version when there is no workbook, not the most.
	//            //WorkbookFormat sourceFormat = WorkbookFormat.Excel97To2003;
	//            WorkbookFormat sourceFormat = Workbook.LatestFormat;

	//            if (sourceRow != null)
	//            {
	//                sourceFormat = sourceRow.Worksheet.CurrentFormat;
	//                sourceRowIndex = sourceRow.Index;
	//            }

	//            short columnIndexAbsolute = (short)this.CellAddress.GetAbsoluteColumnIndex(sourceFormat, sourceColumnIndex, this.AreRelativeAddressesOffsets);
	//            int rowIndexAbsolute = this.CellAddress.GetAbsoluteRowIndex(sourceFormat, sourceRowIndex, this.AreRelativeAddressesOffsets);

	//            int comparisonRowIndex = -1;

	//            // MD 2/24/12
	//            // Found while implementing 12.1 - Table Support
	//            // We should use the least restrictive format version when there is no workbook, not the most.
	//            //WorkbookFormat comparisonFormat = WorkbookFormat.Excel97To2003;
	//            WorkbookFormat comparisonFormat = Workbook.LatestFormat;

	//            if (sourceRow != null)
	//            {
	//                comparisonFormat = comparisonRow.Worksheet.CurrentFormat;
	//                comparisonRowIndex = comparisonRow.Index;
	//            }

	//            short comparisonColumnIndexAbsolute = (short)comparisonRefToken.CellAddress.GetAbsoluteColumnIndex(comparisonFormat, comparisonColumnIndex, comparisonRefToken.AreRelativeAddressesOffsets);
	//            int comparisonRowIndexAbsolute = comparisonRefToken.CellAddress.GetAbsoluteRowIndex(comparisonFormat, comparisonRowIndex, comparisonRefToken.AreRelativeAddressesOffsets);

	//            int columnOffset = 0;
	//            if (this.CellAddress.ColumnAddressIsRelative)
	//                columnOffset = comparisonColumnIndex - sourceColumnIndex;

	//            int rowOffset = 0;
	//            if (this.CellAddress.RowAddressIsRelative && comparisonRowIndex != -1 && sourceRowIndex != -1)
	//                rowOffset = comparisonRowIndex - sourceRowIndex;

	//            return
	//                (columnIndexAbsolute + columnOffset) == comparisonColumnIndexAbsolute &&
	//                (rowIndexAbsolute + rowOffset) == comparisonRowIndexAbsolute;
	//        }

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            this.cellAddress = stream.ReadFormulaCellAddressFromBuffer( ref data, ref dataIndex );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            return 5;
	//        }

	//        public override void OffsetReferences( Point offset )
	//        {
	//            // MD 10/22/10 - TFS36696
	//            // This CellAddress is now immutable.
	//            //this.cellAddress.Offset( offset );
	//            // MD 12/22/11 - 12.1 - Table Support
	//            //this.cellAddress = this.cellAddress.Offset(offset);
	//            if (this.AreRelativeAddressesOffsets == false)
	//                this.cellAddress = this.cellAddress.Offset(offset);
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            stream.Write( this.cellAddress );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            // MD 9/17/08
	//            if ( this.IsReferenceError )
	//                return FormulaParser.ReferenceErrorValue;

	//            // MD 10/22/10 - TFS36696
	//            // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell and format from the formula.
	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //IWorksheetCell sourceCell = null;
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //WorksheetRow sourceCellRow = null;
	//            int sourceCellRowIndex = -1;

	//            short sourceCellColumnIndex = -1;

	//            // MD 2/24/12
	//            // Found while implementing 12.1 - Table Support
	//            // We should use the least restrictive format version when there is no workbook, not the most.
	//            //WorkbookFormat currentFormat = WorkbookFormat.Excel97To2003;
	//            WorkbookFormat currentFormat = Workbook.LatestFormat;

	//            if (owningFormula != null)
	//            {
	//                // MD 4/12/11 - TFS67084
	//                // Moved away from using WorksheetCell objects.
	//                //sourceCell = owningFormula.OwningCell;
	//                // MD 2/20/12 - 12.1 - Table Support
	//                //sourceCellRow = owningFormula.OwningCellRow;
	//                if (owningFormula.OwningCellRow != null)
	//                    sourceCellRowIndex = owningFormula.OwningCellRow.Index;

	//                sourceCellColumnIndex = owningFormula.OwningCellColumnIndex;

	//                currentFormat = owningFormula.CurrentFormat;
	//            }

	//            // MD 5/21/07 - BR23050
	//            // Pass in new parameter which indicates whether relative addresses are offsets from the origin cell
	//            //return this.cellAddress.ToString( sourceCell, cellReferenceMode );
	//            // MD 6/31/08 - Excel 2007 Format
	//            //return this.cellAddress.ToString( sourceCell, this.AreRelativeAddressesOffsets, cellReferenceMode );
	//            return this.cellAddress.ToString(
	//                // MD 4/12/11 - TFS67084
	//                // Moved away from using WorksheetCell objects.
	//                //sourceCell, 
	//                // MD 2/20/12 - 12.1 - Table Support
	//                //sourceCellRow, sourceCellColumnIndex, 
	//                sourceCellRowIndex, sourceCellColumnIndex, 
	//                this.AreRelativeAddressesOffsets, 
	//                cellReferenceMode,
	//                // MD 10/22/10 - TFS36696
	//                // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell and format from the formula.
	//                //this.Formula.CurrentFormat );
	//                currentFormat);
	//        }

	//        // MD 2/28/12 - 12.1 - Table Support
	//        public override bool UpdateReferencesOnCellsShiftedVertically(Worksheet owningWorksheet, int originalOwningRowIndex, short originalOwningColumnIndex,
	//            Worksheet worksheet,
	//            CellShiftOperation shiftOperation,
	//            ReferenceShiftType shiftType,
	//            out FormulaToken replacementToken)
	//        {
	//            replacementToken = null;

	//            Worksheet worksheetWithReferencedCell;
	//            if (this.Is3DReference)
	//            {
	//                if (this.WorkbookFileName != null)
	//                    return false;

	//                if (worksheet.Workbook == null ||
	//                    worksheet.Workbook.Worksheets.Exists(this.WorksheetName) == false)
	//                {
	//                    return false;
	//                }

	//                worksheetWithReferencedCell = worksheet.Workbook.Worksheets[this.WorksheetName];
	//            }
	//            else
	//            {
	//                worksheetWithReferencedCell = owningWorksheet;
	//            }

	//            bool isFormulaShifted = owningWorksheet == worksheet && 
	//                Utilities.RegionContains(
	//                shiftOperation.FirstRowIndex, shiftOperation.LastRowIndex, originalOwningRowIndex,
	//                shiftOperation.FirstColumnIndex, shiftOperation.LastColumnIndex, originalOwningColumnIndex);

	//            int referencedAbsoluteRowIndex = this.CellAddress.GetAbsoluteRowIndex(worksheet.CurrentFormat, originalOwningRowIndex, this.AreRelativeAddressesOffsets);

	//            bool isReferenceShifted = false;
	//            if (worksheetWithReferencedCell == worksheet)
	//            {
	//                short referencedAbsoluteColumnIndex = this.CellAddress.GetAbsoluteColumnIndex(worksheet.CurrentFormat, originalOwningColumnIndex, this.AreRelativeAddressesOffsets);

	//                isReferenceShifted = Utilities.RegionContains(
	//                    shiftOperation.FirstRowIndex, shiftOperation.LastRowIndex, referencedAbsoluteRowIndex,
	//                    shiftOperation.FirstColumnIndex, shiftOperation.LastColumnIndex, referencedAbsoluteColumnIndex);

	//                if (shiftType == ReferenceShiftType.MaintainReference)
	//                {
	//                    if (CellReferenceToken.WouldShiftMoveIntoReference(shiftOperation,
	//                        referencedAbsoluteRowIndex, referencedAbsoluteColumnIndex, referencedAbsoluteColumnIndex))
	//                    {
	//                        replacementToken = this.CreateEquivalentRefErrorToken();
	//                        return true;
	//                    }
	//                }
	//            }

	//            if (isFormulaShifted == false && isReferenceShifted == false)
	//                return false;

	//            if (CellReferenceToken.VerifyShift(worksheet, isReferenceShifted, referencedAbsoluteRowIndex, shiftType, shiftOperation) == false)
	//            {
	//                replacementToken = this.CreateEquivalentRefErrorToken();
	//                return true;
	//            }

	//            return this.ShiftCellAddress(ref this.cellAddress, isFormulaShifted, isReferenceShifted, shiftType, shiftOperation);
	//        }

	//        public override void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
	//        {
	//            this.cellAddress.VerifyFormatLimits( limitErrors, testFormat );
	//        }

	//        // MD 5/21/07 - BR23050
	//        public override bool AreRelativeAddressesOffsets
	//        {
	//            get { return false; }
	//        }

	//        public CellAddress CellAddress
	//        {
	//            get { return this.cellAddress; }
	//        }

	//        // MD 5/18/07 - BR23022
	//        public override bool Is3DReference
	//        {
	//            get { return false; }
	//        }

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch ( this.TokenClass )
	//                {
	//                    case TokenClass.Array:		return Token.RefA;
	//                    case TokenClass.Reference:	return Token.RefR;
	//                    case TokenClass.Value:		return Token.RefV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.RefV;
	//                }
	//            }
	//        }
	//    }

	#endregion // Old Code





	internal class RefToken : CellReferenceToken
	{
		#region Member Variables

		private CellAddress cellAddress;

		#endregion // Member Variables

		#region Constructor

		public RefToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public RefToken(CellAddress cellAddress)
			: this(cellAddress, TokenClass.Reference) { }

		public RefToken(CellAddress cellAddress, TokenClass tokenClass)
			: this(tokenClass)
		{
			this.cellAddress = cellAddress;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region AreRelativeAddressesOffsets

		public override bool AreRelativeAddressesOffsets
		{
			get { return false; }
		}

		#endregion // AreRelativeAddressesOffsets

		#region CreateEquivalentRefErrorToken

		protected override CellReferenceToken CreateEquivalentRefErrorToken()
		{
			return new RefErrToken(this.TokenClass);
		}

		#endregion // CreateEquivalentRefErrorToken

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			WorksheetCellAddress address = this.CellAddress.ToAbsoluteAddress(context, this.AreRelativeAddressesOffsets);
			if (address.IsValid == false)
				return ExcelReferenceError.Instance;

			if (context.Worksheet == null)
			{
				Utilities.DebugFail("The Ref token should only be used from a cell's formula.");
				return ExcelReferenceError.Instance;
			}

			return context.Worksheet.Rows[address.RowIndex].GetCellCalcReference(address.ColumnIndex);
		}

		#endregion // GetCalcValue

		#region GetSharedEquivalent

		public override FormulaToken GetSharedEquivalent(FormulaContext context)
		{
			if (this.CellAddress.HasRelativeAddresses == false)
				return this;

			return new RefNToken(
				this.CellAddress.ToOffset(context),
				this.TokenClass);
		}

		#endregion // GetSharedEquivalent

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			return new RefToken(this.cellAddress, this.TokenClass);
		}

		#endregion // GetTokenForClonedFormula

		#region HasRelativeAddresses

		public override bool HasRelativeAddresses
		{
			get { return this.CellAddress.HasRelativeAddresses; }
		}

		#endregion // HasRelativeAddresses

		#region IsEquivalentTo

		public sealed override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			RefToken comparisonRefToken = (RefToken)comparisonToken;

			if (this.CellAddress.ColumnAddressIsRelative != comparisonRefToken.CellAddress.ColumnAddressIsRelative)
				return false;

			if (this.CellAddress.RowAddressIsRelative != comparisonRefToken.CellAddress.RowAddressIsRelative)
				return false;

			WorksheetCellAddress address = this.CellAddress.ToAbsoluteAddress(sourceContext, this.AreRelativeAddressesOffsets);
			WorksheetCellAddress comparisonAddress = comparisonRefToken.CellAddress.ToAbsoluteAddress(comparisonContext, comparisonRefToken.AreRelativeAddressesOffsets);
			
			int columnOffset = 0;
			if (this.CellAddress.ColumnAddressIsRelative)
				columnOffset = comparisonContext.OwningCellAddress.ColumnIndex - sourceContext.OwningCellAddress.ColumnIndex;

			int rowOffset = 0;
			if (this.CellAddress.RowAddressIsRelative &&
				comparisonContext.OwningCellAddress.RowIndex != -1 &&
				sourceContext.OwningCellAddress.RowIndex != -1)
			{
				rowOffset = comparisonContext.OwningCellAddress.RowIndex - sourceContext.OwningCellAddress.RowIndex;
			}

			return
				(address.ColumnIndex + columnOffset) == comparisonAddress.ColumnIndex &&
				(address.RowIndex + rowOffset) == comparisonAddress.RowIndex;
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			this.cellAddress = stream.ReadFormulaCellAddressFromBuffer(ref data, ref dataIndex);
		}

		#endregion // Load

		#region OffsetReferences

		public override void OffsetReferences(FormulaContext context, Point offset)
		{
			if (this.AreRelativeAddressesOffsets == false)
				this.cellAddress = this.cellAddress.Offset(context, offset);
		}

		#endregion // OffsetReferences

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write(this.cellAddress);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.RefA;
					case TokenClass.Reference: return Token.RefR;
					case TokenClass.Value: return Token.RefV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.RefV;
				}
			}
		}

		#endregion // Token

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			if (this.IsReferenceError)
				return FormulaParser.ReferenceErrorValue;

			return this.cellAddress.ToString(
				context.OwningCellAddress.RowIndex, context.OwningCellAddress.ColumnIndex,
				this.AreRelativeAddressesOffsets,
				context.CellReferenceMode,
				context.Format);
		}

		#endregion // ToString

		#region UpdateReferencesOnCellsShiftedVertically

		public override bool UpdateReferencesOnCellsShiftedVertically(FormulaContext context,
			CellShiftOperation shiftOperation,
			ReferenceShiftType shiftType,
			out FormulaToken replacementToken)
		{
			replacementToken = null;

			WorksheetCellAddress newOwningCellAddress = context.OwningCellAddress;
			WorksheetCellAddress originalOwningCellAddress = (context.Worksheet == shiftOperation.Worksheet)
				? shiftOperation.GetAddressBeforeShift(newOwningCellAddress)
				: newOwningCellAddress;

			bool isFormulaShifted = (originalOwningCellAddress != newOwningCellAddress);

			WorksheetCellAddress originalReferencedAbsoluteAddress = this.CellAddress.ToAbsoluteAddress(context, this.AreRelativeAddressesOffsets);
			WorksheetCellAddress newReferencedAbsoluteAddress = originalReferencedAbsoluteAddress;
			bool isReferenceShifted = false;
			if (this.GetWorksheetContainingReference(context) == shiftOperation.Worksheet)
			{
				ShiftAddressResult result = shiftOperation.ShiftCellAddress(ref newReferencedAbsoluteAddress);
				isReferenceShifted = result.DidShift;

				if (result.IsDeleted && shiftType == ReferenceShiftType.MaintainReference)
				{
					// If the formulas should always point to the same cells and they were deleted by the shift, change the token
					// to a #REF! error.
					replacementToken = this.CreateEquivalentRefErrorToken();
					return true;
				}
			}

			if (isFormulaShifted == false && isReferenceShifted == false)
				return false;

			int rowOffset;
			if (this.GetCellAddressOffset(this.cellAddress,
				originalOwningCellAddress, newOwningCellAddress,
				newReferencedAbsoluteAddress.RowIndex - originalReferencedAbsoluteAddress.RowIndex,
				shiftType, out rowOffset) == false)
			{
				return false;
			}

			this.cellAddress = new CellAddress(
				this.cellAddress.Row + rowOffset, this.cellAddress.RowAddressIsRelative,
				this.cellAddress.Column, this.cellAddress.ColumnAddressIsRelative);

			return true;
		}

		#endregion // UpdateReferencesOnCellsShiftedVertically

		#region VerifyFormatLimits

		public override void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		{
			this.cellAddress.VerifyFormatLimits(limitErrors, testFormat);
		}

		#endregion // VerifyFormatLimits

		#endregion // Base Class Overrides

		#region CellAddress

		public CellAddress CellAddress
		{
			get { return this.cellAddress; }
		}

		#endregion // CellAddress
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