using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Name operand. Indicates a reference to a name defined with a scope of the workbook
	//    /// or the same sheet with the formula.
	//    /// </summary> 
	//#endif
	//    // MD 8/18/08 - Excel formula solving
	//    //internal class NameToken : FormulaToken
	//    // MD 3/30/11 - TFS69969
	//    // Added a common base class for all CellReferenceTokens and NameTokens
	//    //internal class NameToken : OperandToken
	//    internal class NameToken : ReferenceToken
	//    {
	//        // MD 8/20/07 - BR25818
	//        // If this name token is being deserialized for another named reference's formula, 
	//        // we will not resolve the named reference until the workbook globals stream is done 
	//        // being deserialized, so we may need to store the named reference index temporarily.
	//        private int cachedIndexToNamedReference;

	//        private NamedReferenceBase namedReference;
	//        private string name;

	//        // MD 10/9/07 - BR27172
	//        private bool isReferenceToAddInFunction;

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public NameToken( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        public NameToken(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public NameToken( Formula formula, NamedReferenceBase namedReference, TokenClass tokenClass )
	//        //    : base( formula, tokenClass )
	//        public NameToken(NamedReferenceBase namedReference, TokenClass tokenClass)
	//            : base(tokenClass)
	//        {
	//            this.namedReference = namedReference;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public NameToken( Formula formula, string name )
	//        //    : base( formula, TokenClass.Reference )
	//        public NameToken(string name)
	//            : base(TokenClass.Reference)
	//        {
	//            this.name = name;
	//        }

	//        // MD 10/9/07 - BR27172
	//        // Added a new constructor to take the value for a new member variable
	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public NameToken( Formula formula, string name, bool isReferenceToAddInFunction )
	//        //    : this( formula, name )
	//        public NameToken(string name, bool isReferenceToAddInFunction)
	//            : this(name)
	//        {
	//            this.isReferenceToAddInFunction = isReferenceToAddInFunction;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We can use the default implementation of this now.
	//        //public override FormulaToken Clone(Formula newOwningFormula)
	//        //{
	//        //    NameToken nameToken = new NameToken(newOwningFormula, namedReference, this.TokenClass);
	//        //    nameToken.name = this.name;
	//        //
	//        //    // MD 10/9/07 - BR27172
	//        //    nameToken.isReferenceToAddInFunction = this.isReferenceToAddInFunction;
	//        //
	//        //    return nameToken;
	//        //}

	//        // MD 8/18/08 - Excel formula solving
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public override object GetCalcValue( Workbook workbook, IWorksheetCell formulaOwner )
	//        // MD 2/24/12 - 12.1 - Table Support
	//        //public override object GetCalcValue(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            // MD 2/24/12
	//            // Found while implementing 12.1 - Table Support
	//            // We should honor the name on the worksheet first.
	//            //// MD 2/24/12 - 12.1 - Table Support
	//            //// We should also look for tables with the workbook space.
	//            ////NamedReference namedReference = workbook.NamedReferences.Find( this.name );
	//            //NamedReferenceBase namedReference = workbook.GetWorkbookScopedNamedItem(this.name);
	//            //
	//            //// MD 4/12/11 - TFS67084
	//            //// Moved away from using WorksheetCell objects.
	//            ////if ( namedReference == null && formulaOwner != null )
	//            ////    namedReference = workbook.NamedReferences.Find( this.name, formulaOwner.Worksheet );
	//            //// MD 2/24/12 - 12.1 - Table Support
	//            ////if (namedReference == null && formulaOwnerRow != null)
	//            ////    namedReference = workbook.NamedReferences.Find(this.name, formulaOwnerRow.Worksheet);
	//            //if (namedReference == null && worksheet != null)
	//            //    namedReference = workbook.NamedReferences.Find(this.name, worksheet);

	//            NamedReferenceBase namedReference = null;
	//            if (worksheet != null)
	//                namedReference = workbook.NamedReferences.Find(this.name, worksheet);

	//            if (namedReference == null)
	//                namedReference = workbook.GetWorkbookScopedNamedItem(this.name);

	//            if ( namedReference == null )
	//                return new UCReference( this.Name );

	//            return namedReference.CalcReference;
	//        }

	//        // MD 3/30/11 - TFS69969
	//        public override bool Is3DReference
	//        {
	//            get { return false; }
	//        }

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            NameToken comparisonNameToken = (NameToken)comparisonToken;
	//            return this.name == comparisonNameToken.name;
	//        }

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            ushort indexToNameRecord = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//            stream.ReadUInt16FromBuffer( ref data, ref dataIndex ); // Not used

	//            // MD 8/20/07 - BR25818
	//            // If this name token is being deserialized for another named reference's formula, 
	//            // we will not resolve the named reference until the workbook globals stream is done 
	//            // being deserialized, call a helper method which can determiine whether we should 
	//            // resolve the reference or not (this code was moved to ResolveNamedReferenceHelper).
	//            //this.namedReference = manager.CurrentWorkbookReference.NamedReferences[ indexToNameRecord - 1 ];
	//            //Debug.Assert( this.namedReference is NamedReference );
	//            //
	//            //this.name = this.namedReference.Name;
	//            this.ResolveNamedReference( manager, indexToNameRecord - 1 );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            return 5;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method
	//        //public override void OnNamedReferenceRenamed( NamedReference namedReference, string oldName )
	//        // MD 2/22/12 - 12.1 - Table Support
	//        //public override void OnNamedReferenceRenamed(Formula owningFormula, NamedReference namedReference, string oldName)
	//        public override void OnNamedReferenceRenamed(Formula owningFormula, NamedReferenceBase namedReference, string oldName)
	//        {
	//            // MD 4/6/12 - TFS101506
	//            //if ( String.Compare( oldName, this.name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
	//            if (String.Compare(oldName, this.name, owningFormula.Culture, CompareOptions.IgnoreCase) == 0)
	//            {
	//                Worksheet worksheetScope = namedReference.Scope as Worksheet;

	//                // If this is a named reference with a scope of the workbook, this name token is definitely pointing to it.
	//                if ( worksheetScope == null )
	//                {
	//                    this.name = namedReference.Name;
	//                }
	//                else
	//                {
	//                    // Otherwise, this name token is only pointing to the named reference if the scope is the worksheet with 
	//                    // this formula and there is no global reference in the workbook with the scope of the workbook.

	//                    // MD 10/22/10 - TFS36696
	//                    // The token no longer stores the formula, so it needs to be passed into this method
	//                    //IWorksheetCell owningCell = this.Formula.OwningCell;
	//                    // MD 4/12/11 - TFS67084
	//                    // Moved away from using WorksheetCell objects.
	//                    //IWorksheetCell owningCell = owningFormula.OwningCell;
	//                    //
	//                    //if ( owningCell != null && owningCell.Worksheet == worksheetScope )
	//                    WorksheetRow owningCellRow = owningFormula.OwningCellRow;
	//                    if (owningCellRow != null && owningCellRow.Worksheet == worksheetScope)
	//                    {
	//                        // If the scope of the named reference is the worksheet containing the formula, make sure there is no global
	//                        // named reference with the name, because then this token really points to that other named reference.
	//                        // MD 2/24/12 - 12.1 - Table Support
	//                        //if ( worksheetScope.Workbook.NamedReferences.Find( oldName ) == null )
	//                        if (worksheetScope.Workbook.GetWorkbookScopedNamedItem(oldName) == null)
	//                            this.name = namedReference.Name;
	//                    }
	//                }
	//            }
	//        }

	//        // MD 3/2/12 - 12.1 - Table Support
	//        public override void ConvertTableReferencesToRanges(Workbook workbook, WorksheetTable table, WorksheetRow owningRow, short owningColumnIndex, out FormulaToken replacementToken)
	//        {
	//            replacementToken = null;

	//            if (table == null)
	//            {
	//                table = this.GetTable(workbook, owningRow, owningColumnIndex);
	//                if (table == null)
	//                    return;
	//            }

	//            // MD 4/9/12 - TFS101506
	//            CultureInfo culture = CultureInfo.CurrentCulture;
	//            if (workbook != null)
	//                culture = workbook.CultureResolved;

	//            if (this.WorkbookFileName == null &&
	//                this.WorksheetName == null &&
	//                // MD 4/9/12 - TFS101506
	//                //String.Equals(table.Name, this.name, StringComparison.CurrentCultureIgnoreCase))
	//                String.Compare(table.Name, this.name, culture, CompareOptions.IgnoreCase) == 0)
	//            {
	//                Debug.Assert(table.Workbook.GetTable(this.name) == table, "This is unexpected.");

	//                WorksheetRegion region = table.DataAreaRegion;
	//                if (region == null)
	//                    return;

	//                replacementToken = new Area3DToken(
	//                    null,
	//                    null,
	//                    table.Worksheet.Name,
	//                    new CellAddressRange(region),
	//                    this.TokenClass);
	//            }
	//        }

	//        // MD 8/20/07 - BR25818
	//        // Add helper which will resolved the named references for this token after the 
	//        // workbook globals section has finished being loaded
	//        public void ResolveNamedReference( WorkbookSerializationManager manager )
	//        {
	//            if ( this.namedReference != null )
	//                return;

	//            this.ResolveNamedReference( manager, this.cachedIndexToNamedReference );
	//        }

	//        // MD 8/20/07 - BR25818
	//        // Added helper method to resolve a named reference when the token is being loaded.
	//        // If the workbook globals section is currently being loaded, this will not actually
	//        // resolve the reference. It will cache the reference index to be resolved later.
	//        protected void ResolveNamedReference( WorkbookSerializationManager manager, int indexToNameRecord )
	//        {
	//            if ( manager.ShouldResolveNamedReferences )
	//                this.ResolveNamedReferenceHelper( manager, indexToNameRecord );
	//            else
	//                this.cachedIndexToNamedReference = indexToNameRecord;
	//        }

	//        // MD 8/20/07 - BR25818
	//        // Added virtual helper to resolve the named reference when all named references are available.
	//        // This has been moved from the Load override in this class.
	//        protected virtual void ResolveNamedReferenceHelper( WorkbookSerializationManager manager, int indexToNameRecord )
	//        {
	//            this.namedReference = manager.CurrentWorkbookReference.NamedReferences[ indexToNameRecord ];
	//            Debug.Assert( this.namedReference is NamedReference );

	//            this.name = this.namedReference.Name;
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
	//            // MD 10/9/07 - BR27172
	//            // The named reference must be resolved differently for references to add-in functions
	//            if ( this.isReferenceToAddInFunction )
	//            {
	//                this.namedReference = manager.CurrentWorkbookReference.GetNamedReference( this.name, manager.Workbook, true, true );
	//                NamedReference namedReference = (NamedReference)this.namedReference;
	//                namedReference.IsFunction = true;
	//                namedReference.IsMacroName = true;
	//                return;
	//            }

	//            this.namedReference = manager.CurrentWorkbookReference.GetNamedReference( this.name, manager.Workbook, false );

	//            if ( this.namedReference != null )
	//                return;

	//            // MD 2/28/12 - 12.1 - Table Support
	//            //Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];
	//            Worksheet worksheet = owningWorksheet;

	//            if ( worksheet != null )
	//            {
	//                this.namedReference = manager.CurrentWorkbookReference.GetNamedReference( this.name, worksheet, false );

	//                if ( this.namedReference != null )
	//                    return;
	//            }

	//            this.namedReference = manager.CurrentWorkbookReference.GetNamedReference( this.name, manager.Workbook, true );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            Debug.Assert( this.namedReference != null );

	//            int index = manager.CurrentWorkbookReference.NamedReferences.IndexOf( this.namedReference );
	//            Debug.Assert( index >= 0 );

	//            stream.Write( (ushort)( index + 1 ) );
	//            stream.Write( (ushort)0 );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            if ( this.namedReference != null )
	//                return this.namedReference.ToString();

	//            return this.name;
	//        }

	//        #region GetTable

	//        private WorksheetTable GetTable(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            if (this.WorkbookFileName != null || this.WorksheetName != null)
	//                return null;

	//            if (this.name == null)
	//            {
	//                if (formulaOwnerRow != null)
	//                    return formulaOwnerRow.GetCellAssociatedTable(formulaOwnerColumnIndex);
	//                else
	//                    return null;
	//            }

	//            if (workbook != null)
	//                return workbook.GetTable(this.name);

	//            return null;
	//        }

	//        #endregion // GetTable

	//        public string Name
	//        {
	//            get { return this.name; }
	//            set { this.name = value; }
	//        }

	//        public NamedReferenceBase NamedReference
	//        {
	//            get { return this.namedReference; }
	//            protected set { this.namedReference = value; }
	//        }

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch ( this.TokenClass )
	//                {
	//                    case TokenClass.Array:		return Token.NameA;
	//                    case TokenClass.Reference:	return Token.NameR;
	//                    case TokenClass.Value:		return Token.NameV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.NameV;
	//                }
	//            }
	//        }
	//    }

	#endregion // Old Code






	internal class NameToken : ReferenceToken
	{
		#region Member Variables

		private int cachedIndexToNamedReference;
		private NamedReferenceBase namedReference;

		#endregion // Member Variables

		#region Constructor

		public NameToken(TokenClass tokenClass)
			: base(tokenClass) 
		{
			this.namedReference = new NamedReferenceUnconnected(null, Workbook.LatestFormat);
		}

		public NameToken(NamedReferenceBase namedReference, TokenClass tokenClass)
			: base(tokenClass)
		{
			this.namedReference = namedReference;
		}

		public NameToken(string name, WorkbookFormat currentFormat)
			: base(TokenClass.Reference)
		{
			this.namedReference = new NamedReferenceUnconnected(name, currentFormat);
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region ConnectReferences

		public override void ConnectReferences(FormulaContext context)
		{
			if (this.namedReference.Name == null)
				return;

			NamedReferenceBase namedReferenceConnected = null;
			if (context.Worksheet != null)
			{
				// We should honor the name on the worksheet first.
				namedReferenceConnected = context.Workbook.NamedReferences.Find(this.Name, context.Worksheet);
			}

			if (namedReferenceConnected == null)
			{
				// Find the table or workbook scoped name if nothing was found on the worksheet.
				namedReferenceConnected = context.Workbook.GetWorkbookScopedNamedItem(this.Name);
			}

			if (namedReferenceConnected != null)
				this.namedReference = namedReferenceConnected;
		}

		#endregion // ConnectReferences

		#region ConvertTableReferencesToRanges

		public override void ConvertTableReferencesToRanges(FormulaContext context, WorksheetTable table, out FormulaToken replacementToken)
		{
			replacementToken = null;

			if ((this.ScopeReference == null || this.ScopeReference == context.Workbook) &&
				this.namedReference == table)
			{
				Debug.Assert(table.Workbook.GetTable(this.Name) == table, "This is unexpected.");

				WorksheetRegion region = table.DataAreaRegion;
				if (region == null)
					return;

				replacementToken = new Area3DToken(
					table.Workbook.CurrentWorkbookReference.GetWorksheetReference(table.Worksheet.Name, null),
					new CellAddressRange(region),
					this.TokenClass);
			}
		}

		#endregion // ConvertTableReferencesToRanges

		#region DisconnectReferences

		public override void DisconnectReferences()
		{
			this.namedReference = new NamedReferenceUnconnected(this.namedReference.Name, this.namedReference.CurrentFormat);
		}

		#endregion // DisconnectReferences

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			return this.NamedReference.CalcReference;
		}

		#endregion // GetCalcValue

		#region InitializeSerializationManager

		public override void InitializeSerializationManager(WorkbookSerializationManager manager, FormulaContext context)
		{
			manager.RetainNamedReference(ref this.namedReference);
		}

		#endregion // InitializeSerializationManager

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			if (sourceContext.Worksheet != comparisonContext.Worksheet)
				return false;

			NameToken comparisonNameToken = (NameToken)comparisonToken;
			return Object.Equals(this.NamedReference, comparisonNameToken.NamedReference);
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			ushort indexToNameRecord = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
			stream.ReadUInt16FromBuffer(ref data, ref dataIndex); // Not used

			// MD 8/20/07 - BR25818
			// If this name token is being deserialized for another named reference's formula, 
			// we will not resolve the named reference until the workbook globals stream is done 
			// being deserialized, call a helper method which can determiine whether we should 
			// resolve the reference or not (this code was moved to ResolveNamedReferenceHelper).
			//this.namedReference = manager.CurrentWorkbookReference.NamedReferences[ indexToNameRecord - 1 ];
			//Debug.Assert( this.namedReference is NamedReference );
			//
			//this.name = this.namedReference.Name;
			this.ResolveNamedReferenceDuringLoad(stream.Manager, indexToNameRecord - 1);
		}

		#endregion // Load

		#region OnNamedReferenceRemoved

		public sealed override bool OnNamedReferenceRemoved(NamedReferenceBase namedReference)
		{
			if (this.namedReference == namedReference)
			{
				this.DisconnectReferences();
				return true;
			}

			return false;
		}

		#endregion // OnNamedReferenceRemoved

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnTableResizing

		public override void OnTableResizing(WorksheetTable table, List<WorksheetTableColumn> columnsBeingRemoved, 
			out bool tableReferenced, 
			out FormulaToken replacementToken)
		{
			tableReferenced = (table == this.NamedReference);
			replacementToken = null;
		}

		#endregion // OnTableResizing

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			Debug.Assert(this.namedReference != null);

			int index = stream.Manager.Workbook.CurrentWorkbookReference.NamedReferences.IndexOf(this.namedReference);
			Debug.Assert(index >= 0);

			stream.Write((ushort)(index + 1));
			stream.Write((ushort)0);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.NameA;
					case TokenClass.Reference: return Token.NameR;
					case TokenClass.Value: return Token.NameV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.NameV;
				}
			}
		}

		#endregion // Token

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return this.namedReference.ToString(externalReferences);
		}

		#endregion // ToString

		#endregion // Base Class Overrides

		#region Methods

		#region GetTable

		private WorksheetTable GetTable(FormulaContext context)
		{
			if (this.ScopeReference != null)
				return null;

			if (this.Name == null)
			{
				WorksheetRow row = context.OwningRow;
				if (row != null)
					return row.GetCellAssociatedTable(context.OwningCellAddress.ColumnIndex);
				else
					return null;
			}

			if (context.Workbook != null)
				return context.Workbook.GetTable(this.Name);

			return null;
		}

		#endregion // GetTable

		#region ResolveNamedReference

		protected virtual void ResolveNamedReference(WorkbookSerializationManager manager, int indexToNameRecord)
		{
			this.namedReference = manager.Workbook.CurrentWorkbookReference.NamedReferences[indexToNameRecord];
			Debug.Assert(this.namedReference is NamedReference, "This is unexpected.");
		}

		#endregion // ResolveNamedReference

		#region ResolveNamedReferenceAfterLoad

		public void ResolveNamedReferenceAfterLoad(WorkbookSerializationManager manager)
		{
			if (this.namedReference != null)
				return;

			this.ResolveNamedReference(manager, this.cachedIndexToNamedReference);
		}

		#endregion // ResolveNamedReferenceAfterLoad

		#region ResolveNamedReferenceDuringLoad

		protected void ResolveNamedReferenceDuringLoad(WorkbookSerializationManager manager, int indexToNameRecord)
		{
			if (manager.ShouldResolveNamedReferences)
				this.ResolveNamedReference(manager, indexToNameRecord);
			else
				this.cachedIndexToNamedReference = indexToNameRecord;
		}

		#endregion // ResolveNamedReferenceDuringLoad

		#endregion // Methods

		#region Properties

		#region Name

		public string Name
		{
			get { return this.namedReference.Name; }
		}

		#endregion // Name

		#region NamedReference

		public NamedReferenceBase NamedReference
		{
			get { return this.namedReference; }
			protected set { this.namedReference = value; }
		}

		#endregion // NamedReference

		#region ScopeReference

		public virtual object ScopeReference
		{
			get { return null; }
		}

		#endregion // ScopeReference

		#endregion // Properties
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