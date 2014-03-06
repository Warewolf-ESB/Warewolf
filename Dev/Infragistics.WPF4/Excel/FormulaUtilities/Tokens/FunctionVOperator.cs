using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Function operator. Indicates a function with a variable number of arguments.
	//    /// </summary> 
	//#endif
	//    internal class FunctionVOperator : FunctionOperator
	//    {
	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public FunctionVOperator( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        //
	//        //public FunctionVOperator( Formula formula, Function function, byte numberOfArguments )
	//        //    : base( formula, function, numberOfArguments ) { }
	//        //
	//        //public FunctionVOperator( Formula formula, Function function, byte numberOfArguments, TokenClass tokenClass )
	//        //    : base( formula, function, numberOfArguments, tokenClass ) { }
	//        public FunctionVOperator(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        public FunctionVOperator(Function function, byte numberOfArguments)
	//            : base(function, numberOfArguments) { }

	//        public FunctionVOperator(Function function, byte numberOfArguments, TokenClass tokenClass)
	//            : base(function, numberOfArguments, tokenClass) { }

	//        // MD 10/22/10 - TFS36696
	//        // We can use the default implementation of this now.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new FunctionVOperator( newOwningFormula, this.Function, this.NumberOfArguments, this.TokenClass );
	//        //}

	//        public override void Load( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex )
	//        {
	//            byte numberOfArguments = stream.ReadByteFromBuffer( ref data, ref dataIndex );
	//            numberOfArguments &= 0x7F;

	//            ushort sheetFunctionIndex = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );
	//            sheetFunctionIndex &= 0x7FFF;

	//            this.NumberOfArguments = numberOfArguments;
	//            this.Function = Function.GetFunction( sheetFunctionIndex );

	//            // MD 10/8/07 - BR27172
	//            // Add-in functions indicate they have an extra parameter (the NameX token containing the function name).
	//            // Resolve to the actual amount of parameters.
	//            if ( this.Function.IsAddIn )
	//                this.NumberOfArguments--;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override byte GetSize( BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override byte GetSize(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            return 4;
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // To save space, the positionInRecordStream is no longer stored on the token, so it needs to be passed in here.
	//        //public override void Save( WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference )
	//        public override void Save(WorkbookSerializationManager manager, BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
	//        {
	//            // MD 10/8/07 - BR27172
	//            // Add-in functions should indicate they have one more parameter than they actually do 
	//            // (for the NameX token containing the function name).
	//            //manager.CurrentRecordStream.Write( (byte)this.NumberOfArguments );
	//            byte numberOfArguments = this.NumberOfArguments;

	//            if ( this.Function.IsAddIn )
	//                numberOfArguments++;

	//            stream.Write( numberOfArguments );

	//            stream.Write( (ushort)this.Function.ID );
	//        }

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch ( this.TokenClass )
	//                {
	//                    case TokenClass.Array:		return Token.FuncVarA;
	//                    case TokenClass.Reference:	return Token.FuncVarR;
	//                    case TokenClass.Value:		return Token.FuncVarV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.FuncVarV;
	//                }
	//            }
	//        }
	//    }

	#endregion // Old Code





	internal class FunctionVOperator : FunctionOperator
	{
		#region Constructor

		public FunctionVOperator(TokenClass tokenClass)
			: base(tokenClass) { }

		public FunctionVOperator(Function function, byte numberOfArguments)
			: base(function, numberOfArguments) { }

		public FunctionVOperator(Function function, byte numberOfArguments, TokenClass tokenClass)
			: base(function, numberOfArguments, tokenClass) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region Load

		public override void Load(BiffRecordStream stream, bool isForExternalNamedReference, ref byte[] data, ref int dataIndex)
		{
			byte numberOfArguments = stream.ReadByteFromBuffer(ref data, ref dataIndex);
			numberOfArguments &= 0x7F;

			ushort sheetFunctionIndex = stream.ReadUInt16FromBuffer(ref data, ref dataIndex);
			sheetFunctionIndex &= 0x7FFF;

			this.NumberOfArguments = numberOfArguments;
			this.Function = Function.GetFunction(sheetFunctionIndex);

			// MD 10/8/07 - BR27172
			// Add-in functions indicate they have an extra parameter (the NameX token containing the function name).
			// Resolve to the actual amount of parameters.
			if (this.Function.IsAddIn)
				this.NumberOfArguments--;
		}

		#endregion // Load

		#region Save

		public override void Save(BiffRecordStream stream, bool isForExternalNamedReference, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
		{
			// MD 10/8/07 - BR27172
			// Add-in functions should indicate they have one more parameter than they actually do 
			// (for the NameX token containing the function name).
			byte numberOfArguments = this.NumberOfArguments;

			if (this.Function.IsAddIn)
				numberOfArguments++;

			stream.Write(numberOfArguments);
			stream.Write((ushort)this.Function.ID);
		}

		#endregion // Save

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.FuncVarA;
					case TokenClass.Reference: return Token.FuncVarR;
					case TokenClass.Value: return Token.FuncVarV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.FuncVarV;
				}
			}
		}

		#endregion // Token

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