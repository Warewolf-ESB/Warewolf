using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.FormulaUtilities.Tokens
{
	// MD 6/13/12 - CalcEngineRefactor
	#region Old Code

	//internal abstract class OperatorToken : FormulaToken
	//{
	//    protected const int Precendence0 = 0;
	//    protected const int Precendence1 = 1;
	//    protected const int Precendence2 = 2;
	//    protected const int Precendence3 = 3;
	//    protected const int Precendence4 = 4;
	//    protected const int Precendence5 = 5;
	//    protected const int Precendence6 = 6;
	//    protected const int Precendence7 = 7;
	//    protected const int Precendence8 = 8;

	//    // MD 10/22/10 - TFS36696
	//    // We don't need to store the formula on the token anymore.
	//    //public OperatorToken( Formula formula, TokenClass tokenClass )
	//    //    : base( formula, tokenClass ) { }
	//    public OperatorToken(TokenClass tokenClass)
	//        : base(tokenClass) { }

	//    // True for ^, - (U), and + (U)
	//    public virtual bool IsRightAssociative
	//    {
	//        get { return false; }
	//    }

	//    // 0| :      (Range/Area)
	//    // 1| ' '    (Isect)
	//    // 2| ,		 (List)
	//    // 3| - + %  (UMinus, UPlus, Percent)
	//    // 4| ^      
	//    // 5| * /    
	//    // 6| + -    (Add, Sub)
	//    // 7| &      
	//    // 8| = <> < > <= >=
	//    public abstract int Precedence { get;}
	//}

	#endregion // Old Code
	internal abstract class OperatorToken : FormulaToken
	{
		protected const int Precendence0 = 0;
		protected const int Precendence1 = 1;
		protected const int Precendence2 = 2;
		protected const int Precendence3 = 3;
		protected const int Precendence4 = 4;
		protected const int Precendence5 = 5;
		protected const int Precendence6 = 6;
		protected const int Precendence7 = 7;
		protected const int Precendence8 = 8;

		public OperatorToken(TokenClass tokenClass)
			: base(tokenClass) { }

		public override TokenClass GetExpectedParameterClass(int index)
		{
			return TokenClass.Value;
		}

		// True for ^, - (U), and + (U)
		public virtual bool IsRightAssociative
		{
			get { return false; }
		}

		// 0| :      (Range/Area)
		// 1| ' '    (Isect)
		// 2| ,		 (List)
		// 3| - + %  (UMinus, UPlus, Percent)
		// 4| ^      
		// 5| * /    
		// 6| + -    (Add, Sub)
		// 7| &      
		// 8| = <> < > <= >=
		public abstract int Precedence { get; }
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