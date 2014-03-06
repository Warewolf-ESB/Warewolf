using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities
{
	// MD 5/10/12 - TFS111368
	internal class AddInFunctionResolver : EvaluationManager<FormulaToken>
	{
		public AddInFunctionResolver(Formula formula)
			: base(formula) { }

		protected override FormulaToken CreateFunctionNameEvalutationItem(string functionName)
		{
			return null;
		}

		protected override bool EvaluateBinary(FormulaToken currentToken, FormulaToken firstOperand, FormulaToken secondOperand)
		{
			this.EvaluationStack.Push(currentToken);
			return true;
		}

		protected override bool EvaluateFunction(FormulaToken currentToken, FormulaToken functionName, FormulaToken[] arguments)
		{
			FunctionVOperator funcVOperator = currentToken as FunctionVOperator;
			if (funcVOperator != null && funcVOperator.Function == Function.AddInFunction)
			{
				NameToken nameToken = functionName as NameToken;
				if (nameToken != null)
					funcVOperator.Function = Function.GetFunction(nameToken.Name);
			}

			this.EvaluationStack.Push(currentToken);
			return true;
		}

		protected override bool EvaluateLeftAssociativeUnary(FormulaToken currentToken, FormulaToken firstOperand)
		{
			this.EvaluationStack.Push(currentToken);
			return true;
		}

		protected override bool EvaluateOperand(FormulaToken currentToken)
		{
			this.EvaluationStack.Push(currentToken);
			return true;
		}

		protected override bool EvaluateParens(FormulaToken currentToken, FormulaToken parenContents)
		{
			this.EvaluationStack.Push(currentToken);
			return true;
		}

		protected override bool EvaluateRightAssociativeUnary(FormulaToken currentToken, FormulaToken firstOperand)
		{
			this.EvaluationStack.Push(currentToken);
			return true;
		}

		protected override bool EvaluateSpecial(FormulaToken currentToken)
		{
			this.EvaluationStack.Push(currentToken);
			return true;
		}
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