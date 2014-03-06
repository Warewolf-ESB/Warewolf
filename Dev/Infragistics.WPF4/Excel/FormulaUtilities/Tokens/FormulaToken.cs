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
	//    [DebuggerDisplay( "{GetType().Name,nq}: {ToString(),nq}" )]
	//#endif
	//    internal abstract class FormulaToken
	//    {
	//        #region Member Variables

	//        // MD 10/22/10 - TFS36696
	//        // To save space, we will no longer save these on the token.
	//        //private long positionInRecordStream;
	//        //
	//        //private FormulaToken firstTokenInExpression;
	//        //
	//        //private int startCharIndex = -1;
	//        //private string portionInFormula;

	//        private TokenClass tokenClass;

	//        // MD 10/22/10 - TFS36696
	//        // To save space, we will no longer save these on the token.
	//        //private List<AttrSpaceToken> preceedingWhitespace;
	//        //
	//        //private Formula formula; 

	//        #endregion Member Variables

	//        #region Constructor

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public FormulaToken( Formula formula, TokenClass tokenClass )
	//        public FormulaToken(TokenClass tokenClass)
	//        {
	//            // MD 10/22/10 - TFS36696
	//            // We don't need to store the formula on the token anymore.
	//            //this.formula = formula;

	//            this.tokenClass = tokenClass;

	//            // MD 10/22/10 - TFS36696
	//            // To save space, we will no longer save these on the token.
	//            //this.firstTokenInExpression = this;
	//        } 

	//        #endregion // Constructor

	//        #region Base Class Overrides

	//        #region ToString

	//        // MD 7/24/07
	//        // Overrode ToString for debugging purposes
	//        public override string ToString()
	//        {
	//            return this.ToString( null, CellReferenceMode.A1, CultureInfo.CurrentCulture );
	//        }

	//        #endregion ToString 

	//        #endregion Base Class Overrides

	//        #region Methods

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        // Also, renamed for clarity and added a default implementation for tokens which don't need to clone themselves.
	//        //public abstract FormulaToken Clone( Formula newOwningFormula );
	//        public virtual FormulaToken GetTokenForClonedFormula()
	//        {
	//            return this;
	//        }

	//        public virtual TokenClass GetExpectedParameterClass( int index )
	//        {
	//            Utilities.DebugFail( "Invalid parameter index for this token." );
	//            return TokenClass.Value;
	//        }

	//        // MD 5/21/07 - BR23050
	//        // Changed the cell type because it may be a merged region
	//        //public virtual FormulaToken GetNonSharedEquivalent( WorksheetCell originCell ) { return this; }
	//        // MD 6/31/08 - Excel 2007 Format
	//        //public virtual FormulaToken GetNonSharedEquivalent( IWorksheetCell originCell ) { return this; }
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public virtual FormulaToken GetNonSharedEquivalent( Workbook workbook, IWorksheetCell originCell ) { return this; }
	//        // MD 5/13/11 - Data Validations / Page Breaks
	//        // This method doesn't need the row instance, just the row index.
	//        // Also, some callers don't have a workbook. Since the workbook is only needed to get max column and row counts, we could get that from a format instead.
	//        //public virtual FormulaToken GetNonSharedEquivalent(Workbook workbook, WorksheetRow originCellRow, short originCellColumnIndex) { return this; }
	//        public virtual FormulaToken GetNonSharedEquivalent(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex) { return this; }

	//        // MD 9/19/11 - TFS86108
	//        public virtual FormulaToken GetSharedEquivalent(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex) { return this; }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public abstract byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference );
	//        public abstract byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream);

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public virtual bool IsEquivalentTo(FormulaToken comparisonToken, WorksheetRow sourceRow, short sourceColumnIndex, WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            // Most operators are equivalent if their types are the same.
	//            return comparisonToken.GetType() == this.GetType();
	//        }

	//        public virtual void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex ) { }
	//        public virtual void LoadAdditionalData( BiffRecordStream stream, ref byte[] data, ref int dataIndex ) { }
	//        public virtual void OffsetReferences( Point offset ) { }

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method
	//        //public virtual void OnNamedReferenceRenamed( NamedReference namedReference, string oldName ) { }
	//        // MD 2/22/12 - 12.1 - Table Support
	//        //public virtual void OnNamedReferenceRenamed(Formula owningFormula, NamedReference namedReference, string oldName) { }
	//        public virtual void OnNamedReferenceRenamed(Formula owningFormula, NamedReferenceBase namedReference, string oldName) { }

	//        // MD 3/2/12 - 12.1 - Table Support
	//        public virtual void ConvertTableReferencesToRanges(Workbook workbook, WorksheetTable table, WorksheetRow owningRow, short owningColumnIndex, out FormulaToken replacementToken)
	//        {
	//            replacementToken = null;
	//        }

	//        public virtual void OnWorksheetRenamed( Worksheet worksheet, string oldName ) { }

	//        // MD 8/20/07 - BR25818
	//        // Since the formula stores its formula type now, we don't need to know whether this is for a named reference
	//        //public virtual void ResolveReferences( WorkbookSerializationManager manager, bool isForExternalNamedReference ) { }
	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method
	//        //public virtual void ResolveReferences( WorkbookSerializationManager manager ) { }
	//        // MD 2/28/12 - 12.1 - Table Support
	//        //public virtual void ResolveReferences(Formula owningFormula, WorkbookSerializationManager manager) { }
	//        public virtual void ResolveReferences(Formula owningFormula, Worksheet owningWorksheet, WorkbookSerializationManager manager) { }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public virtual void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference ) { }
	//        public virtual void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream) { }

	//        public virtual void SaveAdditionalData( BiffRecordStream stream ) { }

	//        // MD 7/24/07
	//        // This caused a stack overflow when ToString was overriden above, make sure derived classes implement this now
	//        //public virtual string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode ) { return this.ToString(); }
	//        //public abstract string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode );
	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public abstract string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture );
	//        public abstract string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture);

	//        // MBS 8/18/08
	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public virtual string ToString(IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture, Dictionary<WorkbookReferenceBase, int> externalReferences)
	//        //{
	//        //    return this.ToString( sourceCell, cellReferenceMode, culture );
	//        //}
	//        public virtual string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture, Dictionary<WorkbookReferenceBase, int> externalReferences)
	//        {
	//            return this.ToString(owningFormula, cellReferenceMode, culture);
	//        }

	//        // MD 2/28/12 - 12.1 - Table Support
	//        public virtual bool UpdateReferencesOnCellsShiftedVertically(Worksheet owningWorksheet, int originalOwningRowIndex, short originalOwningColumnIndex,
	//            Worksheet worksheet,
	//            CellShiftOperation shiftOperation,
	//            ReferenceShiftType shiftType,
	//            out FormulaToken replacementToken)
	//        {
	//            replacementToken = null;
	//            return false;
	//        }

	//        public virtual void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat ) { } 

	//        #endregion Methods

	//        #region Properties

	//        // MD 10/22/10 - TFS36696
	//        // To save space, we will no longer save these on the token.
	//        //public long ExpressionStartInSavedStream
	//        //{
	//        //    get { return this.firstTokenInExpression.PositionInRecordStream; }
	//        //}
	//        //
	//        //public FormulaToken FirstTokenInExpression
	//        //{
	//        //    get { return this.firstTokenInExpression; }
	//        //    set
	//        //    {
	//        //        Debug.Assert( value != null );
	//        //        this.firstTokenInExpression = value;
	//        //    }
	//        //}
	//        //
	//        //public Formula Formula
	//        //{
	//        //    get { return this.formula; }
	//        //}
	//        //
	//        //public string PortionInFormula
	//        //{
	//        //    get { return this.portionInFormula; }
	//        //    set { this.portionInFormula = value; }
	//        //}
	//        //
	//        //public long PositionInRecordStream
	//        //{
	//        //    get { return this.positionInRecordStream; }
	//        //    set { this.positionInRecordStream = value; }
	//        //}
	//        //
	//        //public List<AttrSpaceToken> PreceedingWhitespace
	//        //{
	//        //    get { return this.preceedingWhitespace; }
	//        //    set { this.preceedingWhitespace = value; }
	//        //}

	//        // MD 10/22/10 - TFS36696
	//        // To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
	//        //public int StartCharIndex
	//        //{
	//        //    get { return this.startCharIndex; }
	//        //    set { this.startCharIndex = value; }
	//        //}

	//        public abstract Token Token { get; }

	//        // MBS 9/10/08 - Excel 2007        
	//        //public  TokenClass TokenClass
	//        public virtual TokenClass TokenClass
	//        {
	//            get { return this.tokenClass; }
	//            // MD 7/24/07
	//            // The token class for tokens which are less than 0x20 cannot be changed
	//            //set { this.tokenClass = value; }
	//            set
	//            {
	//                // The token class cannot be changed for tokens under 0x20
	//                if ( (int)this.Token < 0x20 )
	//                {
	//                    // MD 12/21/11 - TFS97840
	//                    // Even though the AttrSumToken don't store a token class in its type, it should store it so we know
	//                    // whether tokens in the list are value tokens or not (data validation formulas need to check that).
	//                    AttrSumToken sumToken = this as AttrSumToken;
	//                    if (sumToken != null)
	//                        this.tokenClass = value;
	//                    // MD 1/10/12 - TFS99080
	//                    // Even though the Paren token doesn't store a token class in its type, it should store it so we know
	//                    // whether tokens in the list are value tokens or not (data validation formulas need to check that).
	//                    else if (this.Token == Token.Paren)
	//                        this.tokenClass = value;

	//                    return;
	//                }

	//                Debug.Assert(
	//                    (this is OperatorToken) == false || (this is FunctionOperator), 
	//                    "Non-function operator tokens should not have their token class changed.");

	//                this.tokenClass = value;
	//            }
	//        } 

	//        #endregion Properties


	//        #region CreateToken

	//        // MD 9/23/09 - TFS19150
	//        // Every read operation is relatively slow, so the buffer is now cached and passed into this method so we can get values from it.
	//        //internal static FormulaToken CreateToken( Formula formula,  BiffRecordStream stream, byte tokenCode )
	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //internal static FormulaToken CreateToken( Formula formula, BiffRecordStream stream, byte tokenCode, ref byte[] data, ref int dataIndex )
	//        internal static FormulaToken CreateToken(BiffRecordStream stream, byte tokenCode, ref byte[] data, ref int dataIndex)
	//        {
	//            switch ( (Token)tokenCode )
	//            {
	//                // MD 10/22/10 - TFS36696
	//                // We don't need to store the formula on the token anymore, so I updated all constructor calls below to not take the formula.
	//                // I also changed to use the singleton instances of any classes where they are defined.

	//                case Token.Exp:			return new ExpToken();
	//                case Token.Tbl:			return new TblToken();
	//                case Token.Add:			return new AddOperator();
	//                case Token.Sub:			return new SubOperator();
	//                case Token.Mul:			return new MulOperator();
	//                case Token.Div:			return new DivOperator();
	//                case Token.Power:		return new PowerOperator();
	//                case Token.Concat:		return new ConcatOperator();
	//                case Token.LT:			return new LTOperator();
	//                case Token.LE:			return new LEOperator();
	//                case Token.EQ:			return new EQOperator();
	//                case Token.GE:			return new GEOperator();
	//                case Token.GT:			return new GTOperator();
	//                case Token.NE:			return new NEOperator();
	//                case Token.Isect:		return new IsectOperator();
	//                case Token.Union:		return new UnionOperator();
	//                case Token.Range:		return new RangeOperator();
	//                case Token.Uplus:		return new UplusOperator();
	//                case Token.Uminus:		return new UminusOperator();
	//                case Token.Percent:		return new PercentOperator();
	//                case Token.Paren:		return new ParenToken();
	//                case Token.MissArg:		return new MissArgToken();
	//                case Token.Str:			return new StrToken();
	//                case Token.Extended:	break;
	//                case Token.Attr:		return AttrTokenBase.CreateOperator( stream, ref data, ref dataIndex );
	//                case Token.Err:			return new ErrToken();
	//                case Token.Bool:		return new BoolToken();
	//                case Token.Int:			return new IntToken();
	//                case Token.Number:		return new NumberToken();
	//                case Token.ArrayR:
	//                case Token.ArrayV:
	//                case Token.ArrayA:		return new ArrayToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.FuncR:
	//                case Token.FuncV:
	//                case Token.FuncA:		return new FunctionOperator( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.FuncVarR:
	//                case Token.FuncVarV:
	//                case Token.FuncVarA:	return new FunctionVOperator( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.NameR:
	//                case Token.NameV:
	//                case Token.NameA:		return new NameToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.RefR:
	//                case Token.RefV:
	//                case Token.RefA:		return new RefToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.AreaR:
	//                case Token.AreaV:
	//                case Token.AreaA:		return new AreaToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.MemAreaR:
	//                case Token.MemAreaV:
	//                case Token.MemAreaA:	return new MemAreaOperator( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.MemErrR:
	//                case Token.MemErrV:
	//                case Token.MemErrA:		return new MemErrOperator( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.MemNoMemR:
	//                case Token.MemNoMemV:
	//                case Token.MemNoMemA:	return new MemNoMemOperator( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.MemFuncR:
	//                case Token.MemFuncV:
	//                case Token.MemFuncA:	return new MemFuncOperator( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.RefErrR:
	//                case Token.RefErrV:
	//                case Token.RefErrA:		return new RefErrToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.AreaErrR:
	//                case Token.AreaErrV:
	//                case Token.AreaErrA:	return new AreaErrToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.RefNR:
	//                case Token.RefNV:
	//                case Token.RefNA:		return new RefNToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.AreaNR:
	//                case Token.AreaNV:
	//                case Token.AreaNA:		return new AreaNToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.NameXR:
	//                case Token.NameXV:
	//                case Token.NameXA:		return new NameXToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.Ref3dR:
	//                case Token.Ref3dV:
	//                case Token.Ref3dA:		return new Ref3DToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.Area3DR:
	//                case Token.Area3DV:
	//                case Token.Area3DA:		return new Area3DToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.RefErr3dR:
	//                case Token.RefErr3dV:
	//                case Token.RefErr3dA:	return new RefErr3dToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//                case Token.AreaErr3dR:
	//                case Token.AreaErr3dV:
	//                case Token.AreaErr3dA:	return new AreaErr3DToken( FormulaToken.TokenClassFromId( tokenCode ) );
	//            }

	//            Utilities.DebugFail( "Unknown operator code: " + (Token)tokenCode );
	//            return null;
	//        }

	//        #endregion CreateToken

	//        #region TokenClassFromId

	//        internal static TokenClass TokenClassFromId( int id )
	//        {
	//            byte classType = (byte)( ( id & 0x60 ) >> 5 );

	//            switch ( classType )
	//            {
	//                case 0x01: return TokenClass.Reference;
	//                case 0x02: return TokenClass.Value;
	//                case 0x03: return TokenClass.Array;

	//                default:
	//                    Utilities.DebugFail( "Unknown class type" );
	//                    return TokenClass.Reference;
	//            }
	//        }

	//        #endregion TokenClassFromId
	//    }

	#endregion // Old Code



	internal abstract class FormulaToken
	{
		#region Member Variables

		private TokenClass tokenClass;

		#endregion Member Variables

		#region Constructor

		public FormulaToken(TokenClass tokenClass)
		{
			this.tokenClass = tokenClass;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region ToString






		public override string ToString()
		{
			return this.ToString(new FormulaContext(null), null);
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Methods

		#region ConnectReferences

		public virtual void ConnectReferences(FormulaContext context) { }

		#endregion // ConnectReferences

		#region ConvertTableReferencesToRanges

		public virtual void ConvertTableReferencesToRanges(FormulaContext context, WorksheetTable table, out FormulaToken replacementToken)
		{
			replacementToken = null;
		}

		#endregion // ConvertTableReferencesToRanges

		#region DisconnectReferences

		public virtual void DisconnectReferences() { }

		#endregion // DisconnectReferences

		#region GetExpectedParameterClass

		public virtual TokenClass GetExpectedParameterClass(int index)
		{
			Utilities.DebugFail("Invalid parameter index for this token.");
			return TokenClass.Value;
		}

		#endregion // GetExpectedParameterClass

		#region GetNonSharedEquivalent

		public virtual FormulaToken GetNonSharedEquivalent(FormulaContext context)
		{
			return this;
		}

		#endregion // GetNonSharedEquivalent

		#region GetSharedEquivalent

		public virtual FormulaToken GetSharedEquivalent(FormulaContext context)
		{
			return this;
		}

		#endregion // GetSharedEquivalent

		#region GetSize

		public virtual byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			switch (this.Token)
			{
				case Token.Add:
				case Token.Concat:
				case Token.Div:
				case Token.EQ:
				case Token.GE:
				case Token.GT:
				case Token.Isect:
				case Token.LE:
				case Token.LT:
				case Token.MissArg:
				case Token.Mul:
				case Token.NE:
				case Token.Paren:
				case Token.Percent:
				case Token.Power:
				case Token.Range:
				case Token.Sub:
				case Token.Uminus:
				case Token.Union:
				case Token.Uplus:
					return 1;

				case Token.Bool:
				case Token.Err:
					return 2;

				case Token.FuncR:
				case Token.FuncV:
				case Token.FuncA:
				case Token.Int:
				case Token.MemFuncR:
				case Token.MemFuncV:
				case Token.MemFuncA:
					return 3;

				case Token.FuncVarR:
				case Token.FuncVarV:
				case Token.FuncVarA:
					return 4;

				case Token.Exp:
				case Token.NameR:
				case Token.NameV:
				case Token.NameA:
				case Token.RefR:
				case Token.RefV:
				case Token.RefA:
				case Token.RefErrR:
				case Token.RefErrV:
				case Token.RefErrA:
				case Token.RefNR:
				case Token.RefNV:
				case Token.RefNA:
				case Token.Tbl:
					return 5;

				case Token.MemAreaR:
				case Token.MemAreaV:
				case Token.MemAreaA:
				case Token.MemErrR:
				case Token.MemErrV:
				case Token.MemErrA:
				case Token.MemNoMemR:
				case Token.MemNoMemV:
				case Token.MemNoMemA:
					return 7;

				case Token.ArrayR:
				case Token.ArrayV:
				case Token.ArrayA:
					return 8;

				case Token.AreaR:
				case Token.AreaV:
				case Token.AreaA:
				case Token.AreaErrR:
				case Token.AreaErrV:
				case Token.AreaErrA:
				case Token.AreaNR:
				case Token.AreaNV:
				case Token.AreaNA:
				case Token.Number:
					return 9;

				case Token.NameXR:
				case Token.NameXV:
				case Token.NameXA:
				case Token.Ref3dR:
				case Token.Ref3dV:
				case Token.Ref3dA:
				case Token.RefErr3dR:
				case Token.RefErr3dV:
				case Token.RefErr3dA:
					if (isForExternalNamedReference)
						return 9;

					return 7;

				case Token.Area3DR:
				case Token.Area3DV:
				case Token.Area3DA:
				case Token.AreaErr3dR:
				case Token.AreaErr3dV:
				case Token.AreaErr3dA:
					if (isForExternalNamedReference)
						return 13;

					return 11;

				case Token.StructuredTableReferenceR:
				case Token.StructuredTableReferenceV:
				case Token.StructuredTableReferenceA:
					Utilities.DebugFail("This token should not be written out to BIFF streams. It only applies to 2007 formats and later.");
					return 0;

				default:
				case Token.Str:
				case Token.Extended:
				case Token.Attr:
					Utilities.DebugFail("This should be overridden in derived classes.");
					return 0;
			}
		}

		#endregion // GetSize

		#region GetTokenForClonedFormula

		public virtual FormulaToken GetTokenForClonedFormula()
		{
			return this;
		}

		#endregion // GetTokenForClonedFormula

		#region InitializeSerializationManager

		public virtual void InitializeSerializationManager(WorkbookSerializationManager manager, FormulaContext context) { }

		#endregion // InitializeSerializationManager

		#region IsEquivalentTo

		public virtual bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			// Most operators are equivalent if their types are the same.
			return comparisonToken.GetType() == this.GetType();
		}

		#endregion // IsEquivalentTo

		#region Load

		public virtual void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex) { }

		#endregion // Load

		#region LoadAdditionalData

		public virtual void LoadAdditionalData(BiffRecordStream stream, ref byte[] data, ref int dataIndex) { }

		#endregion // LoadAdditionalData

		#region OffsetReferences

		public virtual void OffsetReferences(FormulaContext context, Point offset) { }

		#endregion // OffsetReferences

		#region OnNamedReferenceRemoved

		public virtual bool OnNamedReferenceRemoved(NamedReferenceBase namedReference)
		{
			return false;
		}

		#endregion // OnNamedReferenceRemoved

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnTableResizing

		public virtual void OnTableResizing(WorksheetTable table, List<WorksheetTableColumn> columnsBeingRemoved,
			out bool tableReferenced,
			out FormulaToken replacementToken)
		{
			tableReferenced = false;
			replacementToken = null;
		}

		#endregion // OnTableResizing

		#region OnWorksheetMoved

		public virtual bool OnWorksheetMoved(Worksheet worksheet, int oldIndex)
		{
			return false;
		}

		#endregion // OnWorksheetMoved

		#region OnWorksheetRemoved

		public virtual bool OnWorksheetRemoved(Worksheet worksheet, int oldIndex, out FormulaToken replacementToken) 
		{
			replacementToken = null;
			return false;
		}

		#endregion // OnWorksheetRemoved

		#region Save

		public virtual void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream) { }

		#endregion // Save

		#region SaveAdditionalData

		public virtual void SaveAdditionalData(BiffRecordStream stream) { }

		#endregion // SaveAdditionalData

		#region ToString

		public abstract string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences);

		#endregion // ToString

		#region UpdateReferencesOnCellsShiftedVertically

		public virtual bool UpdateReferencesOnCellsShiftedVertically(FormulaContext context,
			CellShiftOperation shiftOperation,
			ReferenceShiftType shiftType,
			out FormulaToken replacementToken)
		{
			replacementToken = null;
			return false;
		}

		#endregion // UpdateReferencesOnCellsShiftedVertically

		#region VerifyFormatLimits

		public virtual void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat) { }

		#endregion // VerifyFormatLimits

		#endregion Methods

		#region Properties

		public abstract Token Token { get; }

		#region TokenClass

		public virtual TokenClass TokenClass
		{
			get { return this.tokenClass; }
			set
			{
				// The token class cannot be changed for tokens under 0x20
				if ((int)this.Token < 0x20)
				{
					// MD 12/21/11 - TFS97840
					// Even though the AttrSumToken don't store a token class in its type, it should store it so we know
					// whether tokens in the list are value tokens or not (data validation formulas need to check that).
					AttrSumToken sumToken = this as AttrSumToken;
					if (sumToken != null)
						this.tokenClass = value;
					// MD 1/10/12 - TFS99080
					// Even though the Paren token doesn't store a token class in its type, it should store it so we know
					// whether tokens in the list are value tokens or not (data validation formulas need to check that).
					else if (this.Token == Token.Paren)
						this.tokenClass = value;

					return;
				}

				Debug.Assert(
					(this is OperatorToken) == false || (this is FunctionOperator),
					"Non-function operator tokens should not have their token class changed.");

				this.tokenClass = value;
			}
		} 

		#endregion // TokenClass

		#endregion Properties


		#region CreateToken

		internal static FormulaToken CreateToken(BiffRecordStream stream, byte tokenCode, ref byte[] data, ref int dataIndex)
		{
			switch ((Token)tokenCode)
			{
				case Token.Exp: return new ExpToken();
				case Token.Tbl: return new TblToken();
				case Token.Add: return new AddOperator();
				case Token.Sub: return new SubOperator();
				case Token.Mul: return new MulOperator();
				case Token.Div: return new DivOperator();
				case Token.Power: return new PowerOperator();
				case Token.Concat: return new ConcatOperator();
				case Token.LT: return new LTOperator();
				case Token.LE: return new LEOperator();
				case Token.EQ: return new EQOperator();
				case Token.GE: return new GEOperator();
				case Token.GT: return new GTOperator();
				case Token.NE: return new NEOperator();
				case Token.Isect: return new IsectOperator();
				case Token.Union: return new UnionOperator();
				case Token.Range: return new RangeOperator();
				case Token.Uplus: return new UplusOperator();
				case Token.Uminus: return new UminusOperator();
				case Token.Percent: return new PercentOperator();
				case Token.Paren: return new ParenToken();
				case Token.MissArg: return new MissArgToken();
				case Token.Str: return new StrToken();
				case Token.Extended: break;
				case Token.Attr: return AttrTokenBase.CreateOperator(stream, ref data, ref dataIndex);
				case Token.Err: return new ErrToken();
				case Token.Bool: return new BoolToken();
				case Token.Int: return new IntToken();
				case Token.Number: return new NumberToken();
				case Token.ArrayR:
				case Token.ArrayV:
				case Token.ArrayA: return new ArrayToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.FuncR:
				case Token.FuncV:
				case Token.FuncA: return new FunctionOperator(FormulaToken.TokenClassFromId(tokenCode));
				case Token.FuncVarR:
				case Token.FuncVarV:
				case Token.FuncVarA: return new FunctionVOperator(FormulaToken.TokenClassFromId(tokenCode));
				case Token.NameR:
				case Token.NameV:
				case Token.NameA: return new NameToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.RefR:
				case Token.RefV:
				case Token.RefA: return new RefToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.AreaR:
				case Token.AreaV:
				case Token.AreaA: return new AreaToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.MemAreaR:
				case Token.MemAreaV:
				case Token.MemAreaA: return new MemAreaOperator(FormulaToken.TokenClassFromId(tokenCode));
				case Token.MemErrR:
				case Token.MemErrV:
				case Token.MemErrA: return new MemErrOperator(FormulaToken.TokenClassFromId(tokenCode));
				case Token.MemNoMemR:
				case Token.MemNoMemV:
				case Token.MemNoMemA: return new MemNoMemOperator(FormulaToken.TokenClassFromId(tokenCode));
				case Token.MemFuncR:
				case Token.MemFuncV:
				case Token.MemFuncA: return new MemFuncOperator(FormulaToken.TokenClassFromId(tokenCode));
				case Token.RefErrR:
				case Token.RefErrV:
				case Token.RefErrA: return new RefErrToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.AreaErrR:
				case Token.AreaErrV:
				case Token.AreaErrA: return new AreaErrToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.RefNR:
				case Token.RefNV:
				case Token.RefNA: return new RefNToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.AreaNR:
				case Token.AreaNV:
				case Token.AreaNA: return new AreaNToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.NameXR:
				case Token.NameXV:
				case Token.NameXA: return new NameXToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.Ref3dR:
				case Token.Ref3dV:
				case Token.Ref3dA: return new Ref3DToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.Area3DR:
				case Token.Area3DV:
				case Token.Area3DA: return new Area3DToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.RefErr3dR:
				case Token.RefErr3dV:
				case Token.RefErr3dA: return new RefErr3dToken(FormulaToken.TokenClassFromId(tokenCode));
				case Token.AreaErr3dR:
				case Token.AreaErr3dV:
				case Token.AreaErr3dA: return new AreaErr3DToken(FormulaToken.TokenClassFromId(tokenCode));
			}

			Utilities.DebugFail("Unknown operator code: " + (Token)tokenCode);
			return null;
		}

		#endregion CreateToken

		#region TokenClassFromId

		internal static TokenClass TokenClassFromId(int id)
		{
			byte classType = (byte)((id & 0x60) >> 5);

			switch (classType)
			{
				case 0x01: return TokenClass.Reference;
				case 0x02: return TokenClass.Value;
				case 0x03: return TokenClass.Array;

				default:
					Utilities.DebugFail("Unknown class type");
					return TokenClass.Reference;
			}
		}

		#endregion TokenClassFromId
	}

	internal class TokenPositionInfo
	{
		public TokenPositionInfo(long positionInRecordStream, FormulaToken firstTokenInExpression)
		{
			this.PositionInRecordStream = positionInRecordStream;
			this.FirstTokenInExpression = firstTokenInExpression;
		}

		public FormulaToken FirstTokenInExpression;
		public long PositionInRecordStream;
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