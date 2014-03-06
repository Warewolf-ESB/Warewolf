using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Formula operator. Indicates a function with a fixed number of arguments.
	//    /// </summary>  
	//#endif
	//    internal class FunctionOperator : OperatorToken
	//    {
	//        private Function function;
	//        private byte numberOfArguments;

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public FunctionOperator( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        //
	//        //public FunctionOperator( Formula formula, Function function )
	//        //    : this( formula, function, function.MinParams ) { }
	//        public FunctionOperator(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        public FunctionOperator(Function function)
	//            : this(function, function.MinParams) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //protected FunctionOperator( Formula formula, Function function, byte numberOfArguments )
	//        //    : this( formula, function.ReturnClass )
	//        protected FunctionOperator(Function function, byte numberOfArguments)
	//            : this(function.ReturnClass)
	//        {
	//            this.function = function;
	//            this.numberOfArguments = numberOfArguments;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //protected FunctionOperator( Formula formula, Function function, byte numberOfArguments, TokenClass tokenClass )
	//        //    : this( formula, tokenClass )
	//        protected FunctionOperator(Function function, byte numberOfArguments, TokenClass tokenClass)
	//            : this(tokenClass)
	//        {
	//            this.function = function;
	//            this.numberOfArguments = numberOfArguments;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We can use the default implementation of this now.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new FunctionOperator( newOwningFormula, this.function, numberOfArguments, this.TokenClass );
	//        //}

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            this.function = Function.GetFunction( stream.ReadUInt16FromBuffer( ref data, ref dataIndex ) );
	//            this.numberOfArguments = (byte)this.function.MinParams;
	//        }

	//        public override TokenClass GetExpectedParameterClass( int index )
	//        {
	//            return this.function.GetExpectedParameterClass( index );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            return 3;
	//        }

	//        // MD 12/22/11 - 12.1 - Table Support
	//        public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//            WorksheetRow sourceRow, short sourceColumnIndex,
	//            WorksheetRow comparisonRow, short comparisonColumnIndex)
	//        {
	//            if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//                return false;

	//            FunctionOperator comparisonFunctionOperator = (FunctionOperator)comparisonToken;

	//            return
	//                this.function == comparisonFunctionOperator.function &&
	//                this.numberOfArguments == comparisonFunctionOperator.numberOfArguments;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            stream.Write( (ushort)this.function.ID );
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//        public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//        {
	//            return this.function.Name;
	//        }

	//        public Function Function
	//        {
	//            get { return this.function; }

	//            // MD 5/10/12 - TFS111368
	//            //protected set { this.function = value; }
	//            set { this.function = value; }
	//        }

	//        public byte NumberOfArguments
	//        {
	//            get { return this.numberOfArguments; }
	//            set
	//            {
	//                if ( ( this is FunctionVOperator ) == false )
	//                    Utilities.DebugFail( "Only the variable argument functions should have their number of arguments set." );

	//                this.numberOfArguments = value;
	//            }
	//        }

	//        public override int Precedence
	//        {
	//            get { return Int32.MaxValue; }
	//        }

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch ( this.TokenClass )
	//                {
	//                    case TokenClass.Array:		return Token.FuncA;
	//                    case TokenClass.Reference:	return Token.FuncR;
	//                    case TokenClass.Value:		return Token.FuncV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.FuncV;
	//                }
	//            }
	//        }
	//    }

	#endregion // Old Code





	internal class FunctionOperator : OperatorToken
	{
		#region Member Variables

		private Function function;
		private byte numberOfArguments;

		#endregion // Member Variables

		#region Constructor

		public FunctionOperator(TokenClass tokenClass)
			: base(tokenClass) { }

		public FunctionOperator(Function function)
			: this(function, function.MinParams) { }

		protected FunctionOperator(Function function, byte numberOfArguments)
			: this(function.ReturnClass)
		{
			this.function = function;
			this.numberOfArguments = numberOfArguments;
		}

		protected FunctionOperator(Function function, byte numberOfArguments, TokenClass tokenClass)
			: this(tokenClass)
		{
			this.function = function;
			this.numberOfArguments = numberOfArguments;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region GetExpectedParameterClass

		public override TokenClass GetExpectedParameterClass(int index)
		{
			return this.function.GetExpectedParameterClass(index);
		}

		#endregion // GetExpectedParameterClass

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			FunctionOperator comparisonFunctionOperator = (FunctionOperator)comparisonToken;

			return
				this.function == comparisonFunctionOperator.function &&
				this.numberOfArguments == comparisonFunctionOperator.numberOfArguments;
		}

		#endregion // IsEquivalentTo

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			this.function = Function.GetFunction(stream.ReadUInt16FromBuffer(ref data, ref dataIndex));
			this.numberOfArguments = (byte)this.function.MinParams;
		}

		#endregion // Load

		#region Precedence

		public override int Precedence
		{
			get { return Int32.MaxValue; }
		}

		#endregion // Precedence

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			stream.Write((ushort)this.function.ID);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.FuncA;
					case TokenClass.Reference: return Token.FuncR;
					case TokenClass.Value: return Token.FuncV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.FuncV;
				}
			}
		}

		#endregion // Token

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return this.function.Name;
		}

		#endregion // ToString

		#endregion // Base Class Overrides

		#region Function

		public Function Function
		{
			get { return this.function; }
			set { this.function = value; }
		}

		#endregion // Function

		#region NumberOfArguments

		public byte NumberOfArguments
		{
			get { return this.numberOfArguments; }
			set
			{
				if ((this is FunctionVOperator) == false)
					Utilities.DebugFail("Only the variable argument functions should have their number of arguments set.");

				this.numberOfArguments = value;
			}
		}

		#endregion // NumberOfArguments
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