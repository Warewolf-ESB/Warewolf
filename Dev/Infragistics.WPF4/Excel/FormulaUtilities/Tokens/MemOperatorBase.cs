using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//internal abstract class MemOperatorBase : FormulaToken
	//{
	//    private ushort sizeOfRefSubExpression;

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public MemOperatorBase( Formula formula, TokenClass tokenClass )
	//    //    : base( formula, tokenClass ) { }
	//    public MemOperatorBase(TokenClass tokenClass)
	//        : base(tokenClass) { }

	//    // MD 12/22/11 - 12.1 - Table Support
	//    public override bool IsEquivalentTo(FormulaToken comparisonToken,
	//        WorksheetRow sourceRow, short sourceColumnIndex,
	//        WorksheetRow comparisonRow, short comparisonColumnIndex)
	//    {
	//        if (base.IsEquivalentTo(comparisonToken, sourceRow, sourceColumnIndex, comparisonRow, comparisonColumnIndex) == false)
	//            return false;

	//        MemOperatorBase comparisonMemOperatorBase = (MemOperatorBase)comparisonToken;
	//        return this.sizeOfRefSubExpression == comparisonMemOperatorBase.sizeOfRefSubExpression;
	//    }

	//    // MD 7/24/07
	//    // MD 10/22/10 - TFS36696
	//    // The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
	//    //public override string ToString( IWorksheetCell sourceCell, CellReferenceMode cellReferenceMode, CultureInfo culture )
	//    public override string ToString(Formula owningFormula, CellReferenceMode cellReferenceMode, CultureInfo culture)
	//    {
	//        return this.GetType().Name;
	//    }

	//    public ushort SizeOfRefSubExpression
	//    {
	//        get { return this.sizeOfRefSubExpression; }
	//        set { this.sizeOfRefSubExpression = value; }
	//    }
	//}

	#endregion // Old Code
	internal abstract class MemOperatorBase : FormulaToken
	{
		#region Member Variables

		private ushort sizeOfRefSubExpression;

		#endregion // Member Variables

		#region Constructor

		public MemOperatorBase(TokenClass tokenClass)
			: base(tokenClass) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region IsEquivalentTo

		public override bool IsEquivalentTo(FormulaContext sourceContext, FormulaToken comparisonToken, FormulaContext comparisonContext)
		{
			if (base.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				return false;

			MemOperatorBase comparisonMemOperatorBase = (MemOperatorBase)comparisonToken;
			return this.sizeOfRefSubExpression == comparisonMemOperatorBase.sizeOfRefSubExpression;
		}

		#endregion // IsEquivalentTo

		#region ToString

		public override string ToString(FormulaContext context, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return this.GetType().Name;
		}

		#endregion // ToString

		#endregion // Base Class Overrides

		#region SizeOfRefSubExpression

		public ushort SizeOfRefSubExpression
		{
			get { return this.sizeOfRefSubExpression; }
			set { this.sizeOfRefSubExpression = value; }
		}

		#endregion // SizeOfRefSubExpression
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