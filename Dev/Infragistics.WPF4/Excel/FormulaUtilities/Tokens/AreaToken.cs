using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;




using System.Drawing;


namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Area reference operand. Indicates a reference to a rectangular range of cells in the same worksheet.
	//    /// </summary> 
	//#endif
	//    // MD 5/18/07 - BR23022
	//    //internal class AreaToken : FormulaToken
	//    internal class AreaToken : CellReferenceToken
	//    {
	//        private CellAddressRange range;

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public AreaToken( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        public AreaToken(TokenClass tokenClass)
	//            : base( tokenClass) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public AreaToken( Formula formula, CellAddressRange range )
	//        //    : this( formula, TokenClass.Reference )
	//        public AreaToken(CellAddressRange range)
	//            : this(TokenClass.Reference)
	//        {
	//            this.range = range;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public AreaToken( Formula formula, CellAddressRange range, TokenClass tokenClass )
	//        //    : this( formula, tokenClass )
	//        public AreaToken(CellAddressRange range, TokenClass tokenClass)
	//            : this(tokenClass)
	//        {
	//            this.range = range;
	//        }

	//        // MD 2/28/12 - 12.1 - Table Support
	//        protected virtual CellReferenceToken CreateEquivalentRefErrorToken()
	//        {
	//            return new AreaErrToken(this.TokenClass);
	//        }

	//        // MD 9/19/11 - TFS86108
	//        public override FormulaToken GetSharedEquivalent(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex)
	//        {
	//            if (this.CellAddressRange.HasRelativeAddresses == false)
	//                return this;

	//            return new AreaNToken(
	//                this.CellAddressRange.ToOffset(format, originCellRowIndex, originCellColumnIndex),
	//                this.TokenClass);
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new AreaToken( newOwningFormula, this.range.Clone(), this.TokenClass );
	//        //}
	//        public override FormulaToken GetTokenForClonedFormula()
	//        {
	//            return new AreaToken(this.range, this.TokenClass);
	//        }

	//        // MD 8/18/08 - Excel formula solving
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public override object GetCalcValue( Workbook workbook, IWorksheetCell formulaOwner )
	//        //{
	//        //    if ( formulaOwner == null )
	//        //        return null;
	//        //
	//        //    return this.CellAddressRange.GetTargetRegion( formulaOwner, this.AreRelativeAddressesOffsets ).CalcReference;
	//        //}
	//        // MD 2/24/12 - 12.1 - Table Support
	//        //public override object GetCalcValue(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            // MD 2/24/12 - 12.1 - Table Support
	//            //if (formulaOwnerRow == null)
	//            //    return null;
	//            //
	//            //return this.CellAddressRange.GetTargetRegion(formulaOwnerRow, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets).CalcReference;
	//            int formulaOwnerRowIndex = -1;
	//            if (formulaOwnerRow != null)
	//                formulaOwnerRowIndex = formulaOwnerRow.Index;

	//            WorksheetRegion region = this.CellAddressRange.GetTargetRegion(worksheet, formulaOwnerRowIndex, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//            if (region != null)
	//                return region.CalcReference;

	//            return ErrorValue.InvalidCellReference.ToCalcErrorValue();
	//        }

	//        // MD 2/24/12 - 12.1 - Table Support
	//        public override bool HasRelativeAddresses
	//        {
	//            get { return this.CellAddressRange.HasRelativeAddresses; }
	//        }

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            AreaToken comparisonAreaToken = (AreaToken)comparisonToken;

	//            if (this.CellAddressRange.TopLeftCellAddress.ColumnAddressIsRelative !=
	//                comparisonAreaToken.CellAddressRange.TopLeftCellAddress.ColumnAddressIsRelative)
	//                return false;

	//            if (this.CellAddressRange.TopLeftCellAddress.RowAddressIsRelative !=
	//                comparisonAreaToken.CellAddressRange.TopLeftCellAddress.RowAddressIsRelative)
	//                return false;

	//            if (this.CellAddressRange.BottomRightCellAddress.ColumnAddressIsRelative !=
	//                comparisonAreaToken.CellAddressRange.BottomRightCellAddress.ColumnAddressIsRelative)
	//                return false;

	//            if (this.CellAddressRange.BottomRightCellAddress.RowAddressIsRelative !=
	//                comparisonAreaToken.CellAddressRange.BottomRightCellAddress.RowAddressIsRelative)
	//                return false;

	//            WorksheetRegion sourceRegion = this.CellAddressRange.GetTargetRegion(sourceRow, sourceColumnIndex, this.AreRelativeAddressesOffsets);
	//            WorksheetRegion comparisonRegion = comparisonAreaToken.CellAddressRange.GetTargetRegion(comparisonRow, comparisonColumnIndex, comparisonAreaToken.AreRelativeAddressesOffsets);

	//            int topColumnOffset = 0;
	//            if (this.CellAddressRange.TopLeftCellAddress.ColumnAddressIsRelative)
	//                topColumnOffset = comparisonColumnIndex - sourceColumnIndex;

	//            int topRowOffset = 0;
	//            if (this.CellAddressRange.TopLeftCellAddress.RowAddressIsRelative)
	//                topRowOffset = comparisonRow.Index - sourceRow.Index;

	//            int bottomColumnOffset = 0;
	//            if (this.CellAddressRange.BottomRightCellAddress.ColumnAddressIsRelative)
	//                bottomColumnOffset = comparisonColumnIndex - sourceColumnIndex;

	//            int bottomRowOffset = 0;
	//            if (this.CellAddressRange.BottomRightCellAddress.RowAddressIsRelative)
	//                bottomRowOffset = comparisonRow.Index - sourceRow.Index;

	//            return
	//                (sourceRegion.FirstColumnInternal + topColumnOffset) == comparisonRegion.FirstColumnInternal &&
	//                (sourceRegion.FirstRow + topRowOffset) == comparisonRegion.FirstRow &&
	//                (sourceRegion.LastColumnInternal + bottomColumnOffset) == comparisonRegion.LastColumnInternal &&
	//                (sourceRegion.LastRow + bottomRowOffset) == comparisonRegion.LastRow;
	//        }

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            this.range = stream.ReadFormulaCellAddressRangeFromBuffer( ref data, ref dataIndex );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            return 9;
	//        }

	//        public override void OffsetReferences( Point offset )
	//        {
	//            // MD 10/22/10 - TFS36696
	//            // This CellAddressRange is now immutable.
	//            //this.range.Offset( offset );
	//            // MD 12/22/11 - 12.1 - Table Support
	//            //this.range = this.range.Offset(offset);
	//            if (this.AreRelativeAddressesOffsets == false)
	//                this.range = this.range.Offset(offset);
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            stream.Write( this.range );
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
	//            //return this.range.ToString( sourceCell, cellReferenceMode );
	//            // MD 6/31/08 - Excel 2007 Format
	//            //return this.range.ToString( sourceCell, this.AreRelativeAddressesOffsets, cellReferenceMode );
	//            return this.range.ToString(
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

	//            int referencedTopAbsoluteRowIndex = this.CellAddressRange.TopLeftCellAddress.GetAbsoluteRowIndex(worksheet.CurrentFormat, originalOwningRowIndex, this.AreRelativeAddressesOffsets);
	//            int referencedBottomAbsoluteRowIndex = this.CellAddressRange.BottomRightCellAddress.GetAbsoluteRowIndex(worksheet.CurrentFormat, originalOwningRowIndex, this.AreRelativeAddressesOffsets);

	//            bool isTopReferenceShifted = false;
	//            bool isBottomReferenceShifted = false;
	//            if (worksheetWithReferencedCell == worksheet)
	//            {
	//                short referencedLeftAbsoluteColumnIndex = this.CellAddressRange.TopLeftCellAddress.GetAbsoluteColumnIndex(worksheet.CurrentFormat, originalOwningColumnIndex, this.AreRelativeAddressesOffsets);
	//                short referencedRightAbsoluteColumnIndex = this.CellAddressRange.BottomRightCellAddress.GetAbsoluteColumnIndex(worksheet.CurrentFormat, originalOwningColumnIndex, this.AreRelativeAddressesOffsets);

	//                // Only shift if all columns in the area spans were shifted 
	//                if (shiftOperation.FirstColumnIndex <= referencedLeftAbsoluteColumnIndex && referencedRightAbsoluteColumnIndex <= shiftOperation.LastColumnIndex)
	//                {
	//                    isTopReferenceShifted = Utilities.RegionContains(
	//                        shiftOperation.FirstRowIndex, shiftOperation.LastRowIndex, referencedTopAbsoluteRowIndex,
	//                        shiftOperation.FirstColumnIndex, shiftOperation.LastColumnIndex, referencedLeftAbsoluteColumnIndex);

	//                    isBottomReferenceShifted = Utilities.RegionContains(
	//                        shiftOperation.FirstRowIndex, shiftOperation.LastRowIndex, referencedBottomAbsoluteRowIndex,
	//                        shiftOperation.FirstColumnIndex, shiftOperation.LastColumnIndex, referencedLeftAbsoluteColumnIndex);

	//                    // When shifting cells up, a region reference which is touching the bottom of the worksheet must stay touching 
	//                    // the bottom of the worksheet.
	//                    if (isBottomReferenceShifted &&
	//                        shiftOperation.ShiftAmount < 0 &&
	//                        referencedBottomAbsoluteRowIndex == Workbook.GetMaxRowCount(worksheet.CurrentFormat) - 1)
	//                    {
	//                        isBottomReferenceShifted = false;
	//                    }

	//                    if (shiftType == ReferenceShiftType.MaintainReference &&
	//                        referencedTopAbsoluteRowIndex == referencedBottomAbsoluteRowIndex)
	//                    {
	//                        if (CellReferenceToken.WouldShiftMoveIntoReference(shiftOperation,
	//                            referencedTopAbsoluteRowIndex, referencedLeftAbsoluteColumnIndex, referencedRightAbsoluteColumnIndex))
	//                        {
	//                            replacementToken = this.CreateEquivalentRefErrorToken();
	//                            return true;
	//                        }
	//                    }
	//                }
	//            }

	//            if (isFormulaShifted == false && isTopReferenceShifted == false && isBottomReferenceShifted == false)
	//                return false;

	//            if (CellReferenceToken.VerifyShift(worksheet, isTopReferenceShifted, referencedTopAbsoluteRowIndex, shiftType, shiftOperation) == false ||
	//                CellReferenceToken.VerifyShift(worksheet, isBottomReferenceShifted, referencedBottomAbsoluteRowIndex, shiftType, shiftOperation) == false)
	//            {
	//                replacementToken = this.CreateEquivalentRefErrorToken();
	//                return true;
	//            }

	//            CellAddress topLeftAddress = this.CellAddressRange.TopLeftCellAddress;
	//            CellAddress bottomRightAddress = this.CellAddressRange.BottomRightCellAddress;

	//            // Don't use short circuit evaluation here because we want both to be called.
	//            if (this.ShiftCellAddress(ref topLeftAddress, isFormulaShifted, isTopReferenceShifted, shiftType, shiftOperation) |
	//                this.ShiftCellAddress(ref bottomRightAddress, isFormulaShifted, isBottomReferenceShifted, shiftType, shiftOperation))
	//            {
	//                // If the shifted references have crossed each other, it is invalid.
	//                int newTopAbsoluteRowIndex = topLeftAddress.GetAbsoluteRowIndex(worksheet.CurrentFormat, originalOwningRowIndex, this.AreRelativeAddressesOffsets);
	//                int newBottomAbsoluteRowIndex = bottomRightAddress.GetAbsoluteRowIndex(worksheet.CurrentFormat, originalOwningRowIndex, this.AreRelativeAddressesOffsets);
	//                if (newBottomAbsoluteRowIndex < newTopAbsoluteRowIndex)
	//                {
	//                    replacementToken = this.CreateEquivalentRefErrorToken();
	//                    return true;
	//                }

	//                this.range = new CellAddressRange(topLeftAddress, bottomRightAddress);
	//                return true;
	//            }

	//            return false;
	//        }

	//        public override void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
	//        {
	//            this.range.VerifyFormatLimits( limitErrors, testFormat );
	//        }

	//        // MD 5/21/07 - BR23050
	//        public override bool AreRelativeAddressesOffsets
	//        {
	//            get { return false; }
	//        }

	//        public CellAddressRange CellAddressRange
	//        {
	//            get { return this.range; }
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
	//                    case TokenClass.Array:		return Token.AreaA;
	//                    case TokenClass.Reference:	return Token.AreaR;
	//                    case TokenClass.Value:		return Token.AreaV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.AreaV;
	//                }
	//            }
	//        }
	//    }

	#endregion // Old Code





	internal class AreaToken : CellReferenceToken
	{
		#region Member Variables

		private CellAddressRange range;

		#endregion // Member Variables

		#region Constructors

		public AreaToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public AreaToken(CellAddressRange range)
			: this(range, TokenClass.Reference) { }

		public AreaToken(CellAddressRange range, TokenClass tokenClass)
			: this(tokenClass)
		{
			this.range = range;
		}

		#endregion // Constructors

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
			return new AreaErrToken(this.TokenClass);
		}

		#endregion // CreateEquivalentRefErrorToken

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			WorksheetRegion region = this.CellAddressRange.GetTargetRegion(context, this.AreRelativeAddressesOffsets);
			if (region != null)
				return region.CalcReference;

			return ErrorValue.InvalidCellReference.ToCalcErrorValue();
		}

		#endregion // GetCalcValue

		#region GetSharedEquivalent

		public override FormulaToken GetSharedEquivalent(FormulaContext context)
		{
			if (this.CellAddressRange.HasRelativeAddresses == false)
				return this;

			return new AreaNToken(
				this.CellAddressRange.ToOffset(context),
				this.TokenClass);
		}

		#endregion // GetSharedEquivalent

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			return new AreaToken(this.range, this.TokenClass);
		}

		#endregion // GetTokenForClonedFormula

		#region HasRelativeAddresses

		public override bool HasRelativeAddresses
		{
			get { return this.CellAddressRange.HasRelativeAddresses; }
		}

		#endregion // HasRelativeAddresses

		#region IsEquivalentTo

		public sealed override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			AreaToken comparisonAreaToken = (AreaToken)comparisonToken;

			if (this.CellAddressRange.TopLeftCellAddress.ColumnAddressIsRelative !=
				comparisonAreaToken.CellAddressRange.TopLeftCellAddress.ColumnAddressIsRelative)
				return false;

			if (this.CellAddressRange.TopLeftCellAddress.RowAddressIsRelative !=
				comparisonAreaToken.CellAddressRange.TopLeftCellAddress.RowAddressIsRelative)
				return false;

			if (this.CellAddressRange.BottomRightCellAddress.ColumnAddressIsRelative !=
				comparisonAreaToken.CellAddressRange.BottomRightCellAddress.ColumnAddressIsRelative)
				return false;

			if (this.CellAddressRange.BottomRightCellAddress.RowAddressIsRelative !=
				comparisonAreaToken.CellAddressRange.BottomRightCellAddress.RowAddressIsRelative)
				return false;

			WorksheetRegion sourceRegion = this.CellAddressRange.GetTargetRegion(sourceContext, this.AreRelativeAddressesOffsets);
			WorksheetRegion comparisonRegion = comparisonAreaToken.CellAddressRange.GetTargetRegion(comparisonContext, comparisonAreaToken.AreRelativeAddressesOffsets);

			int topColumnOffset = 0;
			if (this.CellAddressRange.TopLeftCellAddress.ColumnAddressIsRelative)
				topColumnOffset = comparisonContext.OwningCellAddress.ColumnIndex - sourceContext.OwningCellAddress.ColumnIndex;

			int topRowOffset = 0;
			if (this.CellAddressRange.TopLeftCellAddress.RowAddressIsRelative)
				topRowOffset = comparisonContext.OwningCellAddress.RowIndex - sourceContext.OwningCellAddress.RowIndex;

			int bottomColumnOffset = 0;
			if (this.CellAddressRange.BottomRightCellAddress.ColumnAddressIsRelative)
				bottomColumnOffset = comparisonContext.OwningCellAddress.ColumnIndex - sourceContext.OwningCellAddress.ColumnIndex;

			int bottomRowOffset = 0;
			if (this.CellAddressRange.BottomRightCellAddress.RowAddressIsRelative)
				bottomRowOffset = comparisonContext.OwningCellAddress.RowIndex - sourceContext.OwningCellAddress.RowIndex;

			return
				(sourceRegion.FirstColumnInternal + topColumnOffset) == comparisonRegion.FirstColumnInternal &&
				(sourceRegion.FirstRow + topRowOffset) == comparisonRegion.FirstRow &&
				(sourceRegion.LastColumnInternal + bottomColumnOffset) == comparisonRegion.LastColumnInternal &&
				(sourceRegion.LastRow + bottomRowOffset) == comparisonRegion.LastRow;
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			this.range = stream.ReadFormulaCellAddressRangeFromBuffer(ref data, ref dataIndex);
		}

		#endregion // Load

		#region OffsetReferences

		public override void OffsetReferences(FormulaContext context, Point offset)
		{
			if (this.AreRelativeAddressesOffsets == false)
				this.range = this.range.Offset(context, offset);
		}

		#endregion // OffsetReferences

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write(this.range);
		}

		#endregion // Save

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			if (this.IsReferenceError)
				return FormulaParser.ReferenceErrorValue;

			return this.range.ToString(
				context.OwningCellAddress.RowIndex, context.OwningCellAddress.ColumnIndex,
				this.AreRelativeAddressesOffsets,
				context.CellReferenceMode,
				context.Format);
		}

		#endregion // ToString

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.AreaA;
					case TokenClass.Reference: return Token.AreaR;
					case TokenClass.Value: return Token.AreaV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.AreaV;
				}
			}
		}

		#endregion // Token

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

			WorksheetRegionAddress originalReferencedAbsoluteAddress = this.CellAddressRange.ToAbsoluteAddress(context, this.AreRelativeAddressesOffsets);
			WorksheetRegionAddress newReferencedAbsoluteAddress = originalReferencedAbsoluteAddress;
			bool isReferenceShifted = false;
			if (this.GetWorksheetContainingReference(context) == shiftOperation.Worksheet)
			{
				ShiftAddressResult result = shiftOperation.ShiftRegionAddress(ref newReferencedAbsoluteAddress, true);
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

			int topRowOffset;
			bool topCellShifted = this.GetCellAddressOffset(this.CellAddressRange.TopLeftCellAddress,
				originalOwningCellAddress, newOwningCellAddress,
				newReferencedAbsoluteAddress.FirstRowIndex - originalReferencedAbsoluteAddress.FirstRowIndex,
				shiftType, out topRowOffset);

			int bottomRowOffset;
			bool bottomCellShifted = this.GetCellAddressOffset(this.CellAddressRange.BottomRightCellAddress,
				originalOwningCellAddress, newOwningCellAddress,
				newReferencedAbsoluteAddress.LastRowIndex - originalReferencedAbsoluteAddress.LastRowIndex,
				shiftType, out bottomRowOffset);

			if (topCellShifted == false && bottomCellShifted == false)
				return false;

			CellAddress topLeftAddress = this.CellAddressRange.TopLeftCellAddress;
			CellAddress bottomRightAddress = this.CellAddressRange.BottomRightCellAddress;

			CellAddress newTopLeftAddress = new CellAddress(
				topLeftAddress.Row + topRowOffset, topLeftAddress.RowAddressIsRelative,
				topLeftAddress.Column, topLeftAddress.ColumnAddressIsRelative);

			CellAddress newBottomRightAddress = new CellAddress(
				bottomRightAddress.Row + bottomRowOffset, bottomRightAddress.RowAddressIsRelative,
				bottomRightAddress.Column, bottomRightAddress.ColumnAddressIsRelative);

			// If the shifted references have crossed each other, it is invalid.
			WorksheetCellAddress newTopLeftAbsoluteAddress = newTopLeftAddress.ToAbsoluteAddress(context, this.AreRelativeAddressesOffsets);
			WorksheetCellAddress newBottomRightAbsoluteAddress = newBottomRightAddress.ToAbsoluteAddress(context, this.AreRelativeAddressesOffsets);
			if (newBottomRightAbsoluteAddress.RowIndex < newTopLeftAbsoluteAddress.RowIndex)
			{
				replacementToken = this.CreateEquivalentRefErrorToken();
				return true;
			}

			this.range = new CellAddressRange(newTopLeftAddress, newBottomRightAddress);
			return true;
		}

		#endregion // UpdateReferencesOnCellsShiftedVertically

		#region VerifyFormatLimits

		public override void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		{
			this.range.VerifyFormatLimits(limitErrors, testFormat);
		}

		#endregion // VerifyFormatLimits

		#endregion // Base Class Overrides

		#region CellAddressRange

		public CellAddressRange CellAddressRange
		{
			get { return this.range; }
		}

		#endregion // CellAddressRange
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