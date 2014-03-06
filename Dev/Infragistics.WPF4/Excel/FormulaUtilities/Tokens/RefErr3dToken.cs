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
	//    /// Deleted 3-D cell reference. Indicates a reference to a cell in another worksheet
	//    /// or another workbook which has been deleted.
	//    /// </summary> 
	//#endif
	//    internal class RefErr3dToken : Ref3DToken
	//    {
	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public RefErr3dToken( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        //
	//        //// MD 9/17/08
	//        //public RefErr3dToken( Formula formula, string workbookFileName, string worksheetName )
	//        //    : base( formula, workbookFileName, worksheetName, new CellAddress( 0, false, 0, false ) ) { }
	//        //
	//        //public RefErr3dToken( Formula formula, WorksheetReference worksheet, string workbookFileName, string worksheetName, CellAddress cellAddress, TokenClass tokenClass )
	//        //    : base( formula, worksheet, workbookFileName, worksheetName, cellAddress, tokenClass ) { }
	//        public RefErr3dToken(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        public RefErr3dToken(string workbookFileName, string worksheetName)
	//            : base(workbookFileName, worksheetName, new CellAddress(0, false, 0, false)) { }

	//        public RefErr3dToken(WorksheetReference worksheet, string workbookFileName, string worksheetName, CellAddress cellAddress, TokenClass tokenClass)
	//            : base(worksheet, workbookFileName, worksheetName, cellAddress, tokenClass) { }

	//        // MD 12/1/11 - TFS96540
	//        // Without this override, tokens of this type were using the base implementation, which was returning the reference to the first cell 
	//        // in the worksheet, which is obviously incorrect.
	//        // MD 2/24/12 - 12.1 - Table Support
	//        //public override object GetCalcValue(Workbook workbook, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        public override object GetCalcValue(Workbook workbook, Worksheet worksheet, WorksheetRow formulaOwnerRow, short formulaOwnerColumnIndex)
	//        {
	//            return ErrorValue.InvalidCellReference.ToCalcErrorValue();
	//        }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new RefErr3dToken(
	//        //        newOwningFormula,
	//        //        this.WorksheetReference,
	//        //        this.WorkbookFileName,
	//        //        this.WorksheetName,
	//        //        this.CellAddress.Clone(),
	//        //        this.TokenClass );
	//        //}
	//        public override FormulaToken GetTokenForClonedFormula()
	//        {
	//            return new RefErr3dToken(
	//                this.WorksheetReference,
	//                this.WorkbookFileName,
	//                this.WorksheetName,
	//                // MD 10/22/10 - TFS36696
	//                // This CellAddress is now immutable.
	//                //this.CellAddress.Clone(),
	//                this.CellAddress,
	//                this.TokenClass);
	//        }

	//        // MD 9/17/08
	//        // This is handled by the base now.
	//        //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode )
	//        //{
	//        //    return FormulaParser.ReferenceErrorValue;
	//        //}

	//        // MD 5/21/07 - BR23050
	//        public override bool AreRelativeAddressesOffsets
	//        {
	//            get { return false; }
	//        }

	//        // MD 9/17/08
	//        public override bool IsReferenceError
	//        {
	//            get { return true; }
	//        }

	//        public override Token Token
	//        {
	//            get
	//            {
	//                switch ( this.TokenClass )
	//                {
	//                    case TokenClass.Array:		return Token.RefErr3dA;
	//                    case TokenClass.Reference:	return Token.RefErr3dR;
	//                    case TokenClass.Value:		return Token.RefErr3dV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.RefErrV;
	//                }
	//            }
	//        }
	//    }

	#endregion // Old Code






	internal class RefErr3dToken : Ref3DToken
	{
		#region Constructor

		public RefErr3dToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public RefErr3dToken(WorksheetReference worksheet)
			: base(worksheet, new CellAddress(0, false, 0, false)) { }

		public RefErr3dToken(WorksheetReference worksheet, CellAddress cellAddress, TokenClass tokenClass)
			: base(worksheet, cellAddress, tokenClass) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region AreRelativeAddressesOffsets

		public override bool AreRelativeAddressesOffsets
		{
			get { return false; }
		}

		#endregion // AreRelativeAddressesOffsets

		#region GetCalcValue

		public override object GetCalcValue(FormulaContext context)
		{
			return ErrorValue.InvalidCellReference.ToCalcErrorValue();
		}

		#endregion // GetCalcValue

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			return new RefErr3dToken(this.WorksheetReference, this.CellAddress, this.TokenClass);
		}

		#endregion // GetTokenForClonedFormula

		#region IsReferenceError

		public override bool IsReferenceError
		{
			get { return true; }
		}

		#endregion // IsReferenceError

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.RefErr3dA;
					case TokenClass.Reference: return Token.RefErr3dR;
					case TokenClass.Value: return Token.RefErr3dV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.RefErrV;
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