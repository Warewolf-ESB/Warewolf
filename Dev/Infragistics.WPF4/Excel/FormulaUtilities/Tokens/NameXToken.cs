using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Name or external name operand. Indicates a reference to a name defined with a scope
	//    /// of another worksheet or an external worksheet or workbook.
	//    /// </summary> 
	//#endif
	//    internal class NameXToken : NameToken
	//    {
	//        private WorksheetReference worksheet;

	//        private string workbookFileName;
	//        private string worksheetName;

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public NameXToken( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        public NameXToken(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public NameXToken( Formula formula, string workbookFileName, string worksheetName, string name )
	//        //    : base( formula, name ) 
	//        public NameXToken(string workbookFileName, string worksheetName, string name)
	//            : base(name) 
	//        {
	//            this.workbookFileName = workbookFileName;
	//            this.worksheetName = worksheetName;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public NameXToken( Formula formula, WorksheetReference worksheet, NamedReferenceBase namedReference, string workbookFileName, string worksheetName, string name, TokenClass tokenClass )
	//        //    : base( formula, namedReference, tokenClass )
	//        public NameXToken(WorksheetReference worksheet, NamedReferenceBase namedReference, string workbookFileName, string worksheetName, string name, TokenClass tokenClass)
	//            : base(namedReference, tokenClass)
	//        {
	//            this.worksheet = worksheet;
	//            this.workbookFileName = workbookFileName;
	//            this.worksheetName = worksheetName;
	//            this.Name = name;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We can use the default implementation of this now.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new NameXToken( 
	//        //        newOwningFormula, 
	//        //        this.worksheet,
	//        //        this.NamedReference, 
	//        //        this.workbookFileName,
	//        //        this.worksheetName,
	//        //        this.Name,
	//        //        this.TokenClass );
	//        //}

	//        // MD 8/18/08 - Excel formula solving
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public override object GetCalcValue( Workbook workbook, IWorksheetCell formulaOwner )
	//        // MD 2/24/12 - 12.1 - Table Support
	//        //public override object GetCalcValue(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            NamedReferenceBase namedReference = null;

	//            // MD 3/30/11 - TFS69969
	//            // We can now try to get a calc reference for external workbook links.
	//            // MD 4/2/12 - TFS99854
	//            // Now that the tokens are initialized with the workbook's loading path, we should only get into this block if the 
	//            // workbook file name is an external workbook's path.
	//            //if (this.workbookFileName != null)
	//            if (this.workbookFileName != null && this.workbookFileName != workbook.LoadingPath)
	//            {
	//                if (workbook.ExternalWorkbooks != null)
	//                {
	//                    ExternalWorkbookReference externalReference;
	//                    if (workbook.ExternalWorkbooks.TryGetValue(this.workbookFileName, out externalReference))
	//                    {
	//                        object scope = externalReference.WorkbookScope;
	//                        if (this.worksheetName != null)
	//                            scope = externalReference.GetWorksheetReference(this.worksheetName, true);

	//                        if (scope != null)
	//                            namedReference = externalReference.GetNamedReference(this.Name, scope, false);
	//                    }
	//                }
	//            }
	//            else
	//            // ---------------------- End of TFS69969 fix --------------------
	//            if ( this.worksheetName != null )
	//            {
	//                if ( workbook.Worksheets.Exists( this.worksheetName ) )
	//                    namedReference = workbook.NamedReferences.Find( this.Name, workbook.Worksheets[ this.worksheetName ] );
	//            }
	//            else
	//            {
	//                // MD 2/24/12 - 12.1 - Table Support
	//                // We should also look for tables with the workbook space.
	//                //namedReference = workbook.NamedReferences.Find( this.Name );
	//                namedReference = workbook.GetWorkbookScopedNamedItem(this.Name);
	//            }

	//            if ( namedReference == null )
	//                return ErrorValue.WrongFunctionName.ToCalcErrorValue();

	//            return namedReference.CalcReference;
	//        }

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            if ( isForExternalNamedReference )
	//            {
	//                int firstSheetIndex = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//                int lastSheetIndex = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//                Debug.Assert( firstSheetIndex == lastSheetIndex );

	//                ExternalWorkbookReference workbook = manager.WorkbookReferences[ manager.WorkbookReferences.Count - 1 ] as ExternalWorkbookReference;

	//                Debug.Assert( workbook != null );
	//                if ( workbook != null )
	//                {
	//                    // MD 8/20/07 - BR25818
	//                    // There are more than two options now with regards to how requested worksheet references should be handled, 
	//                    // so an enum must be used instead of a boolean
	//                    //this.worksheet = workbook.GetWorksheetReference( firstSheetIndex, true, false );
	//                    this.worksheet = workbook.GetWorksheetReference( firstSheetIndex, true, WorksheetRequestAction.None );
	//                }
	//            }
	//            else
	//            {
	//                int externSheetIndex = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );

	//                // MD 8/20/07 - BR25818
	//                // A helper method is now used just in case the index is out of range, 
	//                // the helper will safely handle the error and throw an assertion failure
	//                //this.worksheet = manager.WorksheetReferences[ externSheetIndex ];
	//                this.worksheet = manager.GetWorksheetReference( externSheetIndex );
	//            }

	//            ushort indexToNameRecord = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//            stream.ReadUInt16FromBuffer( ref data, ref dataIndex ); // not used

	//            // MD 8/20/07 - BR25818
	//            // If this name token is being deserialized for another named reference's formula, 
	//            // we will not resolve the named reference until the workbook globals stream is done 
	//            // being deserialized, call a helper method which can determiine whether we should 
	//            // resolve the reference or not (this code was moved to ResolveNamedReferenceHelper).
	//            //this.NamedReference = this.worksheet.Workbook.NamedReferences[ indexToNameRecord - 1 ];
	//            //
	//            //this.Name = this.NamedReference.Name;
	//            //
	//            //if ( this.NamedReference is NamedReference )
	//            //{
	//            //    this.workbookFileName = null;
	//            //
	//            //    if ( this.NamedReference.Scope is Workbook )
	//            //        this.worksheetName = null;
	//            //    else
	//            //        this.worksheetName = ( (Worksheet)this.NamedReference.Scope ).Name;
	//            //}
	//            //else
	//            //{
	//            //    this.workbookFileName = ( (ExternalWorkbookReference)((ExternalNamedReference)this.NamedReference).Workbook ).WorkbookFileName;
	//            //
	//            //    if ( this.NamedReference.Scope is WorkbookReferenceBase )
	//            //        this.worksheetName = null;
	//            //    else
	//            //        this.worksheetName = ( (WorksheetReference)this.NamedReference.Scope ).Name;
	//            //}
	//            this.ResolveNamedReference( manager, indexToNameRecord - 1 );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            if ( isForExternalNamedReference )
	//                return 9;

	//            return 7;
	//        }

	//        // MD 3/30/11 - TFS69969
	//        public override bool Is3DReference
	//        {
	//            get { return true; }
	//        }

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            NameXToken comparisonNameXToken = (NameXToken)comparisonToken;
	//            return
	//                this.workbookFileName == comparisonNameXToken.workbookFileName &&
	//                this.worksheetName == comparisonNameXToken.worksheetName;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method
	//        //public override void OnNamedReferenceRenamed( NamedReference namedReference, string oldName )
	//        // MD 2/22/12 - 12.1 - Table Support
	//        //public override void OnNamedReferenceRenamed(Formula owningFormula, NamedReference namedReference, string oldName)
	//        public override void OnNamedReferenceRenamed(Formula owningFormula, NamedReferenceBase namedReference, string oldName)
	//        {
	//            Worksheet worksheetScope = namedReference.Scope as Worksheet;

	//            // MD 4/6/12 - TFS101506
	//            //if ( worksheetScope != null && 
	//            //    this.workbookFileName == null &&
	//            //    String.Compare( worksheetScope.Name, this.worksheetName, StringComparison.CurrentCultureIgnoreCase ) == 0 &&
	//            //    String.Compare( oldName, this.Name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
	//            //{
	//            //    this.Name = namedReference.Name;
	//            //}
	//            if (worksheetScope != null &&
	//                this.workbookFileName == null)
	//            {
	//                CultureInfo culture = owningFormula.Culture;
	//                if (String.Compare(worksheetScope.Name, this.worksheetName, culture, CompareOptions.IgnoreCase) == 0 &&
	//                    String.Compare(oldName, this.Name, culture, CompareOptions.IgnoreCase) == 0)
	//                {
	//                    this.Name = namedReference.Name;
	//                }
	//            }
	//        }

	//        public override void OnWorksheetRenamed( Worksheet worksheet, string oldName )
	//        {
	//            if ( this.workbookFileName == null &&
	//                // MD 4/6/12 - TFS101506
	//                //String.Compare( oldName, this.worksheetName, StringComparison.CurrentCultureIgnoreCase ) == 0 )
	//                String.Compare(oldName, this.worksheetName, worksheet.Culture, CompareOptions.IgnoreCase) == 0)
	//            {
	//                this.worksheetName = worksheet.Name;
	//            }
	//        }

	//        // MD 8/20/07 - BR25818
	//        // Added virtual helper to resolve the named reference when all named references are available.
	//        // This has been moved from the Load override in this class.
	//        protected override void ResolveNamedReferenceHelper( WorkbookSerializationManager manager, int indexToNameRecord )
	//        {
	//            this.NamedReference = this.worksheet.Workbook.NamedReferences[ indexToNameRecord ];

	//            this.Name = this.NamedReference.Name;

	//            if ( this.NamedReference is NamedReference )
	//            {
	//                this.workbookFileName = null;

	//                if ( this.NamedReference.Scope is Workbook )
	//                    this.worksheetName = null;
	//                else
	//                    this.worksheetName = ( (Worksheet)this.NamedReference.Scope ).Name;
	//            }
	//            else
	//            {
	//                // MD 10/8/07 - BR27172
	//                // The named reference isn't necessarily an ExternalNamedReference anymore, use a more generic approach
	//                //this.workbookFileName = ( (ExternalWorkbookReference)( (ExternalNamedReference)this.NamedReference ).Workbook ).WorkbookFileName;
	//                this.workbookFileName = this.NamedReference.WorkbookReference.FileName;

	//                if ( this.NamedReference.Scope is WorkbookReferenceBase )
	//                    this.worksheetName = null;
	//                else
	//                    this.worksheetName = ( (WorksheetReference)this.NamedReference.Scope ).Name;
	//            }
	//        }

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
	//            // TODO2: Parse the external workbook to get the named reference definition
	//            if ( this.workbookFileName == null )
	//            {
	//                if ( this.worksheetName == null )
	//                {
	//                    // MD 8/20/07 - BR25818
	//                    // Named reference formulas always use NameX tokens, even for global named references in the same workbook,
	//                    // so make sure the worksheet reference is resolved.
	//                    //Utilities.DebugFail( "A Name token should have been used instead of a NameX token" );
	//                    // MD 10/22/10 - TFS36696
	//                    // The token no longer stores the formula, so it needs to be passed into this method
	//                    //Debug.Assert( this.Formula.Type == FormulaType.NamedReferenceFormula, "A Name token should have been used instead of a NameX token" );
	//                    Debug.Assert(owningFormula.Type == FormulaType.NamedReferenceFormula, "A Name token should have been used instead of a NameX token");
	//                    // MD 10/30/11 - TFS90733
	//                    // We should be adding the reference to the serialization manager if it has never been accessed before.
	//                    //this.worksheet = manager.CurrentWorkbookReference.GetWorksheetReference( 0, false, WorksheetRequestAction.None );
	//                    this.worksheet = manager.CurrentWorkbookReference.GetWorksheetReference(0, false, WorksheetRequestAction.AddToReferenceListIfUnique);

	//                    // MD 8/20/07 - BR25818
	//                    // Since the formula stores its formula type now, we don't need to know whether this is for a named reference
	//                    //base.ResolveReferences( manager, isForExternalNamedReference );
	//                    // MD 10/22/10 - TFS36696
	//                    // The token no longer stores the formula, so it needs to be passed into this method
	//                    //base.ResolveReferences( manager );
	//                    // MD 2/28/12 - 12.1 - Table Support
	//                    //base.ResolveReferences(owningFormula, manager);
	//                    base.ResolveReferences(owningFormula, owningWorksheet, manager);
	//                }
	//                else
	//                {
	//                    // MD 8/20/07 - BR25818
	//                    // Check the formula type now because the bool parameter is not passed anymore
	//                    //this.worksheet = manager.CurrentWorkbookReference.GetWorksheetReference( this.worksheetName, isForExternalNamedReference );
	//                    this.worksheet = manager.CurrentWorkbookReference.GetWorksheetReference(
	//                        this.worksheetName,
	//                        // MD 10/22/10 - TFS36696
	//                        // The token no longer stores the formula, so it needs to be passed into this method
	//                        //this.Formula.Type == FormulaType.ExternalNamedReferenceFormula );
	//                        owningFormula.Type == FormulaType.ExternalNamedReferenceFormula);

	//                    if ( this.worksheet.WorksheetIndex < 0 )
	//                    {
	//                        // MBS 9/10/08 - Excel 2007
	//                        //ExternalWorkbookReference externalWorkbook = manager.GetExternalReference( this.worksheetName );
	//                        WorkbookReferenceBase externalWorkbook = manager.GetExternalReference(this.worksheetName);

	//                        // MD 8/20/07 - BR25818
	//                        // Check the formula type now because the bool parameter is not passed anymore
	//                        //this.worksheet = externalWorkbook.GetWorksheetReference( this.worksheetName, isForExternalNamedReference );
	//                        this.worksheet = externalWorkbook.GetWorksheetReference(
	//                            this.worksheetName,
	//                            // MD 10/22/10 - TFS36696
	//                            // The token no longer stores the formula, so it needs to be passed into this method
	//                            //this.Formula.Type == FormulaType.ExternalNamedReferenceFormula );
	//                            owningFormula.Type == FormulaType.ExternalNamedReferenceFormula);

	//                        this.NamedReference = externalWorkbook.GetNamedReference( this.Name, externalWorkbook, true );
	//                    }
	//                    else
	//                    {
	//                        Worksheet worksheet = manager.Workbook.Worksheets[ this.worksheet.WorksheetIndex ];

	//                        this.NamedReference = manager.CurrentWorkbookReference.GetNamedReference( this.Name, worksheet, true );
	//                    }
	//                }
	//            }
	//            // MD 10/8/07 - BR27172
	//            // If the workbook file name is the add-in workbook identifier, we need to resolve the refernce in a different way
	//            else if ( this.workbookFileName == AddInFunctionsWorkbookReference.AddInFunctionsWorkbookName )
	//            {
	//                AddInFunctionsWorkbookReference workbookReference = manager.AddInFunctionsWorkbookReference;
	//                this.NamedReference = workbookReference.GetNamedReference( this.Name, workbookReference, true );
	//                this.worksheet = workbookReference.GetWorksheetReference( workbookReference.FunctionCount.ToString(), false );
	//            }
	//            else
	//            {
	//                // MBS 9/10/08 - Excel 2007
	//                // We can't assume that it's an external reference since a workbook-scoped defined name could
	//                // well have the full path to itself listed to distinguish it from a worksheet-scoped name
	//                //
	//                //ExternalWorkbookReference workbookReference = manager.GetExternalReference( this.workbookFileName );
	//                WorkbookReferenceBase workbookReference = manager.GetExternalReference(this.workbookFileName);

	//                if ( this.worksheetName == null )
	//                {
	//                    // MBS 9/10/08 - Excel 2007
	//                    //this.NamedReference = workbookReference.GetNamedReference( this.Name, workbookReference, true );
	//                    this.NamedReference = workbookReference.GetNamedReference(this.Name, workbookReference.WorkbookScope, true);

	//                    this.worksheet = workbookReference.GetWorksheetReference(
	//                        Path.GetFileNameWithoutExtension( this.workbookFileName ),
	//                        // MD 8/20/07 - BR25818
	//                        // Check the formula type now because the bool parameter is not passed anymore
	//                        //isForExternalNamedReference );
	//                        // MD 10/22/10 - TFS36696
	//                        // The token no longer stores the formula, so it needs to be passed into this method
	//                        //this.Formula.Type == FormulaType.ExternalNamedReferenceFormula );
	//                        owningFormula.Type == FormulaType.ExternalNamedReferenceFormula);
	//                }
	//                else
	//                {
	//                    this.worksheet = workbookReference.GetWorksheetReference( this.worksheetName, false );

	//                    this.NamedReference = workbookReference.GetNamedReference( this.Name, this.worksheet, true );
	//                }
	//            }
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            Debug.Assert( this.NamedReference != null );

	//            if ( isForExternalNamedReference )
	//            {
	//                stream.Write( (ushort)this.worksheet.WorksheetIndex );
	//                stream.Write( (ushort)this.worksheet.WorksheetIndex );
	//            }
	//            else
	//            {
	//                int index = manager.WorksheetReferences.IndexOf( this.worksheet );
	//                Debug.Assert( index >= 0 );
	//                stream.Write( (ushort)index );
	//            }

	//            int namedReferenceIndex = this.worksheet.Workbook.NamedReferences.IndexOf( this.NamedReference );
	//            Debug.Assert( namedReferenceIndex >= 0 );

	//            stream.Write( (ushort)( namedReferenceIndex + 1 ) );
	//            stream.Write( (ushort)0 );
	//        }

	//        // MD 3/30/11 - TFS69969
	//        public override void SetDefaultWorkbookFileName(string workbookFileName)
	//        {
	//            if (this.workbookFileName == null)
	//                this.workbookFileName = workbookFileName;
	//        }

	//        // MBS 8/19/08 - Excel 2007 Format
	//        //public override string ToString(IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode)
	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString(IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture, Dictionary<WorkbookReferenceBase, int> externalReferences)
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture, Dictionary<WorkbookReferenceBase, int> externalReferences)
	//        {
	//            if (this.NamedReference != null)
	//            {
	//                // MBS 9/10/08 - Excel 2007 Format
	//                //return this.NamedReference.ToString();
	//                string tokenString = this.NamedReference.ToString(externalReferences);
	//                //
	//                // If we have a scope of the current workbook, the named reference has to be written out accordingly
	//                if(this.NamedReference.Scope is Workbook)                
	//                    tokenString = Utilities.CreateReferenceString( this.workbookFileName, null) + tokenString;
	//                //
	//                return tokenString;
	//            }

	//            Debug.Assert(externalReferences == null, "Expected the external references to be null at this point");

	//            return
	//                Utilities.CreateReferenceString( this.workbookFileName, this.worksheetName ) +
	//                // MD 10/22/10 - TFS36696
	//                // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//                //base.ToString( sourceCell, cellReferenceMode, culture );
	//                base.ToString(owningFormula, cellReferenceMode, culture);
	//        }

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch ( this.TokenClass )
	//                {
	//                    case TokenClass.Array:		return Token.NameXA;
	//                    case TokenClass.Reference:	return Token.NameXR;
	//                    case TokenClass.Value:		return Token.NameXV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.NameXV;
	//                }
	//            }
	//        }

	//        // MD 8/21/08 - Excel formula solving
	//        public bool IsScopedToWorksheet( Worksheet worksheet )
	//        {
	//            if ( this.workbookFileName == null &&
	//                // MD 4/6/12 - TFS101506
	//                //String.Compare( worksheet.Name, this.worksheetName, StringComparison.CurrentCultureIgnoreCase ) == 0 )
	//                String.Compare(worksheet.Name, this.worksheetName, worksheet.Culture, CompareOptions.IgnoreCase) == 0)
	//            {
	//                return true;
	//            }

	//            return false;
	//        }

	//        // MD 5/13/11 - Data Validations / Page Breaks
	//        //internal string WorkbookFileName
	//        public override string WorkbookFileName
	//        {
	//            get { return this.workbookFileName; }
	//        }

	//        // MD 5/13/11 - Data Validations / Page Breaks
	//        //internal string WorksheetName
	//        public override string WorksheetName
	//        {
	//            get { return this.worksheetName; }
	//        }
	//    }

	#endregion // Old Code






	internal class NameXToken : NameToken
	{
		#region Member Variables

		private WorksheetReference worksheetReference;

		#endregion // Member Variables

		#region Constructor

		public NameXToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public NameXToken(string workbookFileName, string worksheetName, string name, WorkbookFormat currentFormat)
			: base(name, currentFormat)
		{
			WorkbookReferenceUnconnected workbookReference = new WorkbookReferenceUnconnected(workbookFileName);
			this.worksheetReference = workbookReference.GetWorksheetReference(worksheetName);
			this.NamedReference.ScopeInternal = this.worksheetReference.NamedReferenceScope;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region ConnectReferences

		public override void ConnectReferences(FormulaContext context)
		{
			this.worksheetReference = this.worksheetReference.Connect(context);
			NamedReferenceBase namedReferenceConnected = worksheetReference.GetNamedReference(this.Name);

			if (namedReferenceConnected != null)
				this.NamedReference = namedReferenceConnected;
		}

		#endregion // ConnectReferences

		#region DisconnectReferences

		public override void DisconnectReferences()
		{
			this.worksheetReference = this.worksheetReference.Disconnect();
			base.DisconnectReferences();
		}

		#endregion // DisconnectReferences

		#region InitializeSerializationManager

		public override void InitializeSerializationManager(WorkbookSerializationManager manager, FormulaContext context)
		{
			manager.RetainWorksheetReference(this.worksheetReference);
		}

		#endregion // InitializeSerializationManager

		#region Is3DReference

		public override bool Is3DReference
		{
			get { return true; }
		}

		#endregion // Is3DReference

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			NameXToken comparisonNameXToken = (NameXToken)comparisonToken;
			return Object.Equals(this.worksheetReference, comparisonNameXToken.worksheetReference);
		}

		#endregion // IsEquivalentTo

		#region IsExternalReference

		public override bool IsExternalReference
		{
			get { return this.worksheetReference.IsExternal; }
		}

		#endregion // IsExternalReference

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			this.worksheetReference = this.Load3DData(stream, isForExternalNamedReference, ref data, ref dataIndex);
			Debug.Assert(this.worksheetReference.IsMultiSheet == false, "The named reference should not belong to multiple sheets.");

			ushort indexToNameRecord = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
			stream.ReadUInt16FromBuffer(ref data, ref dataIndex); // not used

			this.ResolveNamedReferenceDuringLoad(stream.Manager, indexToNameRecord - 1);
		}

		#endregion // Load

		#region OnWorksheetRemoved

		public override bool OnWorksheetRemoved(Worksheet worksheet, int oldIndex, out FormulaToken replacementToken)
		{
			WorksheetReferenceLocal localReference = this.worksheetReference as WorksheetReferenceLocal;
			if (localReference != null && localReference.Worksheet == worksheet)
			{
				this.worksheetReference = localReference.WorkbookReference.GetWorksheetReference(EXTERNSHEETRecord.WorkbookLevelReferenceIndex);
				replacementToken = this;
				return true;
			}
			else
			{
				replacementToken = null;
				return false;
			}
		}

		#endregion // OnWorksheetRemoved

		#region ResolveNamedReference

		protected override void ResolveNamedReference(WorkbookSerializationManager manager, int indexToNameRecord)
		{
			this.NamedReference = this.worksheetReference.WorkbookReference.NamedReferences[indexToNameRecord];
		}

		#endregion // ResolveNamedReference

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			this.Save3DData(stream, this.worksheetReference, isForExternalNamedReference);

			int namedReferenceIndex = this.worksheetReference.WorkbookReference.NamedReferences.IndexOf(this.NamedReference);
			Debug.Assert(namedReferenceIndex >= 0);

			stream.Write((ushort)(namedReferenceIndex + 1));
			stream.Write((ushort)0);
		}

		#endregion // Save

		#region ScopeReference

		public override object ScopeReference
		{
			get { return this.worksheetReference.NamedReferenceScope; }
		}

		#endregion // ScopeReference

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.NameXA;
					case TokenClass.Reference: return Token.NameXR;
					case TokenClass.Value: return Token.NameXV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.NameXV;
				}
			}
		}

		#endregion // Token

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