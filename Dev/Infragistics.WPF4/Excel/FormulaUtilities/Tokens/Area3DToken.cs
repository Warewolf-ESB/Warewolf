using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// 3-D area reference operand. Indicates a reference to a rectangular region of cells
	//    /// in another worksheet or another workbook.
	//    /// </summary> 
	//#endif
	//    internal class Area3DToken : AreaToken
	//    {
	//        #region Member Variables

	//        private WorksheetReference worksheet;
	//        private string workbookFileName;
	//        private string worksheetName;

	//        #endregion // Member Variables

	//        #region Constructor

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public Area3DToken( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        public Area3DToken(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public Area3DToken( Formula formula, string workbookFileName, string worksheetName, CellAddressRange range )
	//        //    : base( formula, range )
	//        public Area3DToken(string workbookFileName, string worksheetName, CellAddressRange range)
	//            : base(range)
	//        {
	//            this.workbookFileName = workbookFileName;
	//            this.worksheetName = worksheetName;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public Area3DToken( Formula formula, WorksheetReference worksheet, string workbookFileName, string worksheetName, CellAddressRange range, TokenClass tokenClass )
	//        //    : base( formula, range, tokenClass )
	//        public Area3DToken(WorksheetReference worksheet, string workbookFileName, string worksheetName, CellAddressRange range, TokenClass tokenClass)
	//            : base(range, tokenClass)
	//        {
	//            this.worksheet = worksheet;
	//            this.workbookFileName = workbookFileName;
	//            this.worksheetName = worksheetName;
	//        }

	//        #endregion // Constructor

	//        #region Base Class Overrides

	//        #region AreRelativeAddressesOffsets

	//        // MD 5/21/07 - BR23050
	//        public override bool AreRelativeAddressesOffsets
	//        {
	//            get { return false; }
	//        }

	//        #endregion // AreRelativeAddressesOffsets

	//        // MD 2/28/12 - 12.1 - Table Support
	//        #region CreateEquivalentRefErrorToken

	//        protected override CellReferenceToken CreateEquivalentRefErrorToken()
	//        {
	//            return new AreaErr3DToken(this.worksheet, this.WorkbookFileName, this.WorksheetName, this.CellAddressRange, this.TokenClass);
	//        }

	//        #endregion // CreateEquivalentRefErrorToken

	//        #region GetCalcValue

	//        // MD 8/18/08 - Excel formula solving
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public override object GetCalcValue( Workbook workbook, IWorksheetCell formulaOwner )
	//        // MD 2/24/12 - 12.1 - Table Support
	//        //public override object GetCalcValue(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            // MD 2/20/12 - 12.1 - Table Support
	//            int formulaOwnerRowIndex = -1;
	//            if (formulaOwnerRow != null)
	//                formulaOwnerRowIndex = formulaOwnerRow.Index;

	//            // MD 12/11/09 - TFS25428
	//            // If this is an external workbook reference, which we don't support yet, return an invalid cell reference.
	//            // MD 4/2/12 - TFS99854
	//            // Now that the tokens are initialized with the workbook's loading path, we should only get into this block if the 
	//            // workbook file name is an external workbook's path.
	//            //if (this.workbookFileName != null)
	//            if (this.workbookFileName != null && this.workbookFileName != workbook.LoadingPath)
	//            {
	//                // MD 3/30/11 - TFS69969
	//                // We can now try to get a calc reference for external workbook links.
	//                if (workbook.ExternalWorkbooks != null)
	//                {
	//                    ExternalWorkbookReference externalReference;
	//                    if (workbook.ExternalWorkbooks.TryGetValue(this.workbookFileName, out externalReference))
	//                    {
	//                        WorksheetReference worksheetReference = externalReference.GetWorksheetReference(this.worksheetName, false);

	//                        // MD 4/12/11 - TFS67084
	//                        // Moved away from using WorksheetCell objects.
	//                        //int firstColumnIndex = this.CellAddressRange.TopLeftCellAddress.GetAbsoluteColumnIndex(formulaOwner, this.AreRelativeAddressesOffsets);
	//                        //int firstRowIndex = this.CellAddressRange.TopLeftCellAddress.GetAbsoluteRowIndex(formulaOwner, this.AreRelativeAddressesOffsets);
	//                        //int lastColumnIndex = this.CellAddressRange.BottomRightCellAddress.GetAbsoluteColumnIndex(formulaOwner, this.AreRelativeAddressesOffsets);
	//                        //int lastRowIndex = this.CellAddressRange.BottomRightCellAddress.GetAbsoluteRowIndex(formulaOwner, this.AreRelativeAddressesOffsets);
	//                        //short firstColumnIndex = this.CellAddressRange.TopLeftCellAddress.GetAbsoluteColumnIndex(formulaOwnerRow, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//                        //int firstRowIndex = this.CellAddressRange.TopLeftCellAddress.GetAbsoluteRowIndex(formulaOwnerRow, this.AreRelativeAddressesOffsets);
	//                        //short lastColumnIndex = this.CellAddressRange.BottomRightCellAddress.GetAbsoluteColumnIndex(formulaOwnerRow, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//                        //int lastRowIndex = this.CellAddressRange.BottomRightCellAddress.GetAbsoluteRowIndex(formulaOwnerRow, this.AreRelativeAddressesOffsets);
	//                        // MD 2/20/12 - 12.1 - Table Support
	//                        WorkbookFormat currentFormat = workbook.CurrentFormat;
	//                        short firstColumnIndex = this.CellAddressRange.TopLeftCellAddress.GetAbsoluteColumnIndex(currentFormat, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//                        int firstRowIndex = this.CellAddressRange.TopLeftCellAddress.GetAbsoluteRowIndex(currentFormat, formulaOwnerRowIndex, this.AreRelativeAddressesOffsets);
	//                        short lastColumnIndex = this.CellAddressRange.BottomRightCellAddress.GetAbsoluteColumnIndex(currentFormat, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//                        int lastRowIndex = this.CellAddressRange.BottomRightCellAddress.GetAbsoluteRowIndex(currentFormat, formulaOwnerRowIndex, this.AreRelativeAddressesOffsets);

	//                        // MD 2/24/12 - 12.1 - Table Support
	//                        if (firstColumnIndex < 0 || firstRowIndex < 0 || lastColumnIndex < 0 || lastRowIndex < 0)
	//                            return ExcelReferenceError.Instance;

	//                        return worksheetReference.GetCalcReference(firstRowIndex, lastRowIndex, firstColumnIndex, lastColumnIndex);
	//                    }
	//                }

	//                return ErrorValue.InvalidCellReference.ToCalcErrorValue();
	//            }

	//            if (workbook.Worksheets.Exists(this.worksheetName) == false)
	//            {
	//                // MD 12/11/09
	//                // Found while fixing TFS25428
	//                // Null will equate to 0, but this really should be treated as a zero. It should be a #REF! error.
	//                //return null;
	//                return ErrorValue.InvalidCellReference.ToCalcErrorValue();
	//            }

	//            // MD 4/12/11 - TFS67084
	//            // Moved away from using WorksheetCell objects.
	//            //return this.CellAddressRange.GetTargetRegion( workbook.Worksheets[ this.worksheetName ], formulaOwner, this.AreRelativeAddressesOffsets ).CalcReference;
	//            // MD 2/20/12 - 12.1 - Table Support
	//            //return this.CellAddressRange.GetTargetRegion(workbook.Worksheets[this.worksheetName], formulaOwnerRow, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets).CalcReference;
	//            WorksheetRegion region = this.CellAddressRange.GetTargetRegion(workbook.Worksheets[this.worksheetName], formulaOwnerRowIndex, formulaOwnerColumnIndex, this.AreRelativeAddressesOffsets);
	//            if (region != null)
	//                return region.CalcReference;

	//            return ErrorValue.InvalidCellReference.ToCalcErrorValue();
	//        }

	//        #endregion // GetCalcValue

	//        #region GetSharedEquivalent

	//        // MD 9/19/11 - TFS86108
	//        public override FormulaToken GetSharedEquivalent(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex)
	//        {
	//            if (this.CellAddressRange.HasRelativeAddresses == false)
	//                return this;

	//            return new Area3DNToken(
	//                this.worksheet,
	//                this.workbookFileName,
	//                this.worksheetName,
	//                this.CellAddressRange.ToOffset(format, originCellRowIndex, originCellColumnIndex),
	//                this.TokenClass);
	//        }

	//        #endregion // GetSharedEquivalent

	//        #region GetSize

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            if (isForExternalNamedReference)
	//                return 13;

	//            return 11;
	//        }

	//        #endregion // GetSize

	//        #region GetTokenForClonedFormula

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new Area3DToken(
	//        //        newOwningFormula,
	//        //        this.worksheet,
	//        //        this.workbookFileName,
	//        //        this.worksheetName,
	//        //        this.CellAddressRange.Clone(),
	//        //        this.TokenClass );
	//        //}
	//        public override FormulaToken GetTokenForClonedFormula()
	//        {
	//            return new Area3DToken(
	//                this.worksheet,
	//                this.workbookFileName,
	//                this.worksheetName,
	//                // MD 10/22/10 - TFS36696
	//                // This CellAddressRange is now immutable.
	//                //this.CellAddressRange.Clone(),
	//                this.CellAddressRange,
	//                this.TokenClass);
	//        }

	//        #endregion // GetTokenForClonedFormula

	//        #region Is3DReference

	//        // MD 5/18/07 - BR23022
	//        public override bool Is3DReference
	//        {
	//            get { return true; }
	//        }

	//        #endregion // Is3DReference

	//        #region IsEquivalentTo

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            Area3DToken comparisonArea3DToken = (Area3DToken)comparisonToken;
	//            return
	//                this.workbookFileName == comparisonArea3DToken.workbookFileName &&
	//                this.worksheetName == comparisonArea3DToken.worksheetName;
	//        }

	//        #endregion // IsEquivalentTo

	//        #region Load

	//        public override void Load(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
	//        {
	//            if (isForExternalNamedReference)
	//            {
	//                int firstSheetIndex = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
	//                int lastSheetIndex = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
	//                Debug.Assert(firstSheetIndex == lastSheetIndex);

	//                ExternalWorkbookReference workbook = manager.WorkbookReferences[manager.WorkbookReferences.Count - 1] as ExternalWorkbookReference;

	//                Debug.Assert(workbook != null);
	//                if (workbook != null)
	//                {
	//                    // MD 8/20/07 - BR25818
	//                    // There are more than two options now with regards to how requested worksheet references should be handled, 
	//                    // so an enum must be used instead of a boolean
	//                    //this.worksheet = workbook.GetWorksheetReference( firstSheetIndex, true, false );
	//                    this.worksheet = workbook.GetWorksheetReference(firstSheetIndex, true, WorksheetRequestAction.None);
	//                }
	//            }
	//            else
	//            {
	//                int externSheetIndex = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);

	//                // MD 8/20/07 - BR25818
	//                // A helper method is now used just in case the index is out of range, 
	//                // the helper will safely handle the error and throw an assertion failure
	//                //this.worksheet = manager.WorksheetReferences[ externSheetIndex ];
	//                this.worksheet = manager.GetWorksheetReference(externSheetIndex);
	//            }

	//            Utilities.GetWorksheetInfo(this.worksheet, out this.workbookFileName, out this.worksheetName);
	//            base.Load(manager, stream, isForExternalNamedReference, ref data, ref dataIndex);
	//        }

	//        #endregion // Load

	//        #region OnWorksheetRenamed

	//        public override void OnWorksheetRenamed(Worksheet worksheet, string oldName)
	//        {
	//            if ( this.workbookFileName == null &&
	//                // MD 4/6/12 - TFS101506
	//                //String.Compare( oldName, this.worksheetName, StringComparison.CurrentCultureIgnoreCase ) == 0 )
	//                String.Compare(oldName, this.worksheetName, worksheet.Culture, CompareOptions.IgnoreCase) == 0)
	//            {
	//                this.worksheetName = worksheet.Name;
	//            }
	//        }

	//        #endregion // OnWorksheetRenamed

	//        #region ResolveReferences

	//        // MD 8/20/07 - BR25818
	//        // Since the formula stores its formula type now, we don't need to know whether this is for a named reference
	//        //public override void ResolveReferences( WorkbookSerializationManager manager, bool isForExternalNamedReference )
	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method
	//        //public override void ResolveReferences( WorkbookSerializationManager manager )
	//        // MD 2/28/12 - 12.1 - Table Support
	//        //public override void ResolveReferences(Formula owningFormula, WorkbookSerializationManager manager)
	//        public override void ResolveReferences(Formula owningFormula, Worksheet owningWorksheet, WorkbookSerializationManager manager)
	//        {
	//            // MD 8/20/07 - BR25818
	//            // Check the formula type now because the bool parameter is not passed anymore
	//            //this.worksheet = manager.GetWorksheetReference( this.workbookFileName, this.worksheetName, isForExternalNamedReference );
	//            this.worksheet = manager.GetWorksheetReference(
	//                this.workbookFileName,
	//                this.worksheetName,
	//                // MD 10/22/10 - TFS36696
	//                // The token no longer stores the formula, so it needs to be passed into this method
	//                //this.Formula.Type == FormulaType.ExternalNamedReferenceFormula );
	//                owningFormula.Type == FormulaType.ExternalNamedReferenceFormula);
	//        }

	//        #endregion // ResolveReferences

	//        #region Save

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            if (isForExternalNamedReference)
	//            {
	//                stream.Write((ushort)this.worksheet.WorksheetIndex);
	//                stream.Write((ushort)this.worksheet.WorksheetIndex);
	//            }
	//            else
	//            {
	//                int index = (ushort)manager.WorksheetReferences.IndexOf(this.worksheet);
	//                Debug.Assert(index >= 0);
	//                stream.Write((ushort)index);
	//            }

	//            // MD 10/22/10 - TFS36696
	//            // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.	
	//            //base.Save( manager, stream, isForExternalNamedReference );
	//            base.Save(manager, stream, isForExternalNamedReference, tokenPositionsInRecordStream);
	//        }

	//        #endregion // Save

	//        #region SetDefaultWorkbookFileName

	//        // MD 3/30/11 - TFS69969
	//        public override void SetDefaultWorkbookFileName(string workbookFileName)
	//        {
	//            if (this.workbookFileName == null)
	//                this.workbookFileName = workbookFileName;
	//        }

	//        #endregion // SetDefaultWorkbookFileName

	//        #region Token

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch (this.TokenClass)
	//                {
	//                    case TokenClass.Array: return Token.Area3DA;
	//                    case TokenClass.Reference: return Token.Area3DR;
	//                    case TokenClass.Value: return Token.Area3DV;

	//                    default:
	//                        Utilities.DebugFail("Invalid token class");
	//                        return Token.Area3DV;
	//                }
	//            }
	//        }

	//        #endregion // Token

	//        #region ToString

	//        // MBS 8/19/08 - Excel 2007 Format
	//        //public override string ToString(IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode)
	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString(IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture, Dictionary<WorkbookReferenceBase, int> externalReferences)
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture, Dictionary<WorkbookReferenceBase, int> externalReferences)
	//        {
	//            // MD 9/17/08
	//            // The worksheet reference might be a #REF! error. In that case, we don't want to append the cell reference string on the end.
	//            #region Refactored

	//            /*
	//            if (this.worksheet != null)
	//                // MBS 8/19/08 - Excel 2007 Format
	//                //return this.worksheet.ReferenceName + base.ToString( sourceCell, cellReferenceMode );
	//                return this.worksheet.GetReferenceName(externalReferences) + base.ToString(sourceCell, cellReferenceMode);

	//            Debug.Assert(externalReferences == null, "We shouldn't get here with non-null external references");

	//            return
	//                Utilities.CreateReferenceString(this.workbookFileName, this.worksheetName) +
	//                base.ToString(sourceCell, cellReferenceMode);
	//            */

	//            #endregion Refactored
	//            string referenceString = CellReferenceToken.Get3dReferenceString(this.worksheet, externalReferences, this.workbookFileName, this.worksheetName);

	//            if (referenceString == FormulaParser.ReferenceErrorValue)
	//                return referenceString;

	//            // MD 10/22/10 - TFS36696
	//            // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//            //return referenceString + base.ToString( sourceCell, cellReferenceMode, culture );
	//            return referenceString + base.ToString(owningFormula, cellReferenceMode, culture);
	//        }

	//        #endregion // ToString

	//        #region UpdateIndexedWorkbookReference

	//        // MD 3/29/11 - TFS63971
	//        public override void UpdateIndexedWorkbookReference(Excel2007WorkbookSerializationManager manager)
	//        {
	//            manager.UpdateIndexedWorkbookReference(ref this.workbookFileName);
	//        }

	//        #endregion // UpdateIndexedWorkbookReference

	//        #region WorkbookFileName

	//        // MD 5/13/11 - Data Validations / Page Breaks
	//        //public string WorkbookFileName
	//        public override string WorkbookFileName
	//        {
	//            get { return this.workbookFileName; }
	//        }

	//        #endregion // WorkbookFileName

	//        #region WorksheetName

	//        // MD 5/13/11 - Data Validations / Page Breaks
	//        //public string WorksheetName
	//        public override string WorksheetName
	//        {
	//            get { return this.worksheetName; }
	//        }

	//        #endregion // WorksheetName

	//        #endregion // Base Class Overrides

	//        #region WorksheetReference

	//        public WorksheetReference WorksheetReference
	//        {
	//            get { return this.worksheet; }
	//        }

	//        #endregion // WorksheetReference
	//    }

	#endregion // Old Code






	internal class Area3DToken : AreaToken
	{
		#region Member Variables

		private WorksheetReference worksheetReference;

		#endregion // Member Variables

		#region Constructor

		public Area3DToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public Area3DToken(WorksheetReference worksheetReference, CellAddressRange range)
			: base(range)
		{
			this.worksheetReference = worksheetReference;
		}

		public Area3DToken(WorksheetReference worksheetReference, CellAddressRange range, TokenClass tokenClass)
			: base(range, tokenClass)
		{
			this.worksheetReference = worksheetReference;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region AreRelativeAddressesOffsets

		// MD 5/21/07 - BR23050
		public override bool AreRelativeAddressesOffsets
		{
			get { return false; }
		}

		#endregion // AreRelativeAddressesOffsets

		#region ConnectReferences

		public override void ConnectReferences(FormulaContext context)
		{
			this.worksheetReference = this.worksheetReference.Connect(context);
		}

		#endregion // ConnectReferences

		#region CreateEquivalentRefErrorToken

		protected override CellReferenceToken CreateEquivalentRefErrorToken()
		{
			return new AreaErr3DToken(this.worksheetReference, this.CellAddressRange, this.TokenClass);
		}

		#endregion // CreateEquivalentRefErrorToken

		#region DisconnectReferences

		public override void DisconnectReferences()
		{
			this.worksheetReference = this.worksheetReference.Disconnect();
		}

		#endregion // DisconnectReferences

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			WorksheetRegionAddress regionAddress = this.CellAddressRange.ToAbsoluteAddress(context, this.AreRelativeAddressesOffsets);
			if (regionAddress.IsValid == false)
				return ExcelReferenceError.Instance;

			return this.worksheetReference.GetCalcReference(regionAddress);
		}

		#endregion // GetCalcValue

		#region GetSharedEquivalent

		public override FormulaToken GetSharedEquivalent(FormulaContext context)
		{
			if (this.CellAddressRange.HasRelativeAddresses == false)
				return this;

			return new Area3DNToken(this.worksheetReference, this.CellAddressRange.ToOffset(context), this.TokenClass);
		}

		#endregion // GetSharedEquivalent

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			return new Area3DToken(this.worksheetReference, this.CellAddressRange, this.TokenClass);
		}

		#endregion // GetTokenForClonedFormula

		#region InitializeSerializationManager

		public override void InitializeSerializationManager(WorkbookSerializationManager manager, FormulaContext context)
		{
			manager.RetainWorksheetReference(this.worksheetReference);
		}

		#endregion // InitializeSerializationManager

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			this.worksheetReference = this.Load3DData(stream, isForExternalNamedReference, ref data, ref dataIndex);
			base.Load(stream, isForExternalNamedReference, ref data, ref dataIndex);
		}

		#endregion // Load

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			this.Save3DData(stream, this.worksheetReference, isForExternalNamedReference);
			base.Save(stream, isForExternalNamedReference, tokenPositionsInRecordStream);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.Area3DA;
					case TokenClass.Reference: return Token.Area3DR;
					case TokenClass.Value: return Token.Area3DV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.Area3DV;
				}
			}
		}

		#endregion // Token

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			string referenceString = this.worksheetReference.GetReferenceName(externalReferences);
			if (referenceString == FormulaParser.ReferenceErrorValue)
				return referenceString;

			return referenceString + base.ToString(context, externalReferences);
		}

		#endregion // ToString

		#region UpdateIndexedWorkbookReference

		public override void UpdateIndexedWorkbookReference(Excel2007WorkbookSerializationManager manager)
		{
			manager.UpdateIndexedWorkbookReference(ref this.worksheetReference);
		}

		#endregion // UpdateIndexedWorkbookReference

		#region WorksheetReference

		public override WorksheetReference WorksheetReference
		{
			get { return this.worksheetReference; }
			protected set { this.worksheetReference = value; }
		}

		#endregion // WorksheetReference

		#endregion // Base Class Overrides
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