using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using System.Drawing;


namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//#if DEBUG
	//    /// <summary>
	//    /// Cell reference within a shared formula operand.
	//    /// </summary> 
	//#endif
	//    internal class RefNToken : RefToken
	//    {
	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public RefNToken( Formula formula, TokenClass tokenClass )
	//        //    : base( formula, tokenClass ) { }
	//        //
	//        //public RefNToken( Formula formula, CellAddress cellAddress )
	//        //    : base( formula, cellAddress ) { }
	//        //
	//        //public RefNToken( Formula formula, CellAddress cellAddress, TokenClass tokenClass )
	//        //    : base( formula, cellAddress, tokenClass ) { }
	//        public RefNToken(TokenClass tokenClass)
	//            : base(tokenClass) { }

	//        public RefNToken(CellAddress cellAddress)
	//            : base(cellAddress) { }

	//        public RefNToken(CellAddress cellAddress, TokenClass tokenClass)
	//            : base(cellAddress, tokenClass) { }

	//        // MD 10/22/10 - TFS36696
	//        // We don't need to store the formula on the token anymore.
	//        //public override FormulaToken Clone( Formula newOwningFormula )
	//        //{
	//        //    return new RefNToken( newOwningFormula, this.CellAddress.Clone(), this.TokenClass );
	//        //}
	//        public override FormulaToken GetTokenForClonedFormula()
	//        {
	//            return new RefNToken(this.CellAddress, this.TokenClass);
	//        }

	//        // MD 5/21/07 - BR23050
	//        // Changed the cell type because it may be a merged region
	//        //public override FormulaToken GetNonSharedEquivalent( WorksheetCell originCell )
	//        // MD 6/31/08 - Excel 2007 Format
	//        //public override FormulaToken GetNonSharedEquivalent( IWorksheetCell originCell )
	//        // MD 4/12/11 - TFS67084
	//        // Moved away from using WorksheetCell objects.
	//        //public override FormulaToken GetNonSharedEquivalent( Workbook workbook, IWorksheetCell originCell )
	//        // MD 5/13/11 - Data Validations / Page Breaks
	//        //public override FormulaToken GetNonSharedEquivalent(Workbook workbook, WorksheetRow originCellRow, short originCellColumnIndex)
	//        public override FormulaToken GetNonSharedEquivalent(WorkbookFormat format, int originCellRowIndex, short originCellColumnIndex)
	//        {
	//            return new RefToken( 
	//                // MD 10/22/10 - TFS36696
	//                // We don't need to store the formula on the token anymore.
	//                //this.Formula,
	//                // MD 6/31/08 - Excel 2007 Format
	//                //new CellAddress( originCell, this.CellAddress ), 
	//                // MD 4/12/11 - TFS67084
	//                // Moved away from using WorksheetCell objects.
	//                //new CellAddress( workbook, originCell, this.CellAddress ), 
	//                // MD 5/13/11 - Data Validations / Page Breaks
	//                //new CellAddress(workbook, originCellRow, originCellColumnIndex, this.CellAddress), 
	//                // MD 9/19/11 - TFS86108
	//                // This constructor was ambiguous so its logic was moved to the ToAbsolute method.
	//                //new CellAddress(format, originCellRowIndex, originCellColumnIndex, this.CellAddress), 
	//                this.CellAddress.ToAbsolute(format, originCellRowIndex, originCellColumnIndex), 
	//                this.TokenClass );
	//        }

	//        // MD 5/21/07 - BR23050
	//        public override bool AreRelativeAddressesOffsets
	//        {
	//            get { return true; }
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
	//                    case TokenClass.Array:		return Token.RefNA;
	//                    case TokenClass.Reference:	return Token.RefNR;
	//                    case TokenClass.Value:		return Token.RefNV;

	//                    default:
	//                        Utilities.DebugFail( "Invalid token class" );
	//                        return Token.RefNV;
	//                }
	//            }
	//        }
	//    }

	#endregion // Old Code





	internal class RefNToken : RefToken
	{
		#region Constructor

		public RefNToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public RefNToken(CellAddress cellAddress)
			: base(cellAddress) { }

		public RefNToken(CellAddress cellAddress, TokenClass tokenClass)
			: base(cellAddress, tokenClass) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region AreRelativeAddressesOffsets

		public override bool AreRelativeAddressesOffsets
		{
			get { return true; }
		}

		#endregion // AreRelativeAddressesOffsets

		#region GetNonSharedEquivalent

		public override FormulaToken GetNonSharedEquivalent(FormulaContext context)
		{
			return new RefToken(this.CellAddress.ToAbsolute(context), this.TokenClass);
		}

		#endregion // GetNonSharedEquivalent

		#region GetTokenForClonedFormula

		public override FormulaToken GetTokenForClonedFormula()
		{
			return new RefNToken(this.CellAddress, this.TokenClass);
		}

		#endregion // GetTokenForClonedFormula

		#region Token

		public override Token Token
		{
			get
			{
				switch (this.TokenClass)
				{
					case TokenClass.Array: return Token.RefNA;
					case TokenClass.Reference: return Token.RefNR;
					case TokenClass.Value: return Token.RefNV;

					default:
						Utilities.DebugFail("Invalid token class");
						return Token.RefNV;
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