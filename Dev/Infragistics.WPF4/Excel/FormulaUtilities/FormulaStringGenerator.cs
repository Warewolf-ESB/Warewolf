using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel.FormulaUtilities
{
	internal class FormulaStringGenerator : EvaluationManager<string>
	{
		#region Member Variables

		private readonly string cachedDecimalSeparator;

		// MD 6/13/12 - CalcEngineRefactor
		//private readonly CultureInfo culture;

		private readonly string unionOperatorResolved;

		private bool ignoreWhitespace;

		// MD 6/13/12 - CalcEngineRefactor
		//private CellReferenceMode cellReferenceMode;

		private List<AttrSpaceToken> whitespaceTokens;

		// MD 10/22/10 - TFS36696
		// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
		private Dictionary<FormulaToken, string> tokenPortionsInFormula;

		// MD 2/4/11 - TFS65015
		private bool isForSaving;

		// MD 6/13/12 - CalcEngineRefactor
		private FormulaContext formulaContext;

		#endregion Member Variables

		#region Constructor

		public FormulaStringGenerator( Formula formula, CellReferenceMode cellReferenceMode, CultureInfo culture )
			: this( formula, cellReferenceMode, culture, false ) { }

		public FormulaStringGenerator(Formula formula, CellReferenceMode cellReferenceMode, CultureInfo culture, Dictionary<FormulaToken, string> tokenPortionsInFormula)
			: this(formula, cellReferenceMode, culture) 
		{
			this.tokenPortionsInFormula = tokenPortionsInFormula;
		}

		public FormulaStringGenerator( Formula formula, CellReferenceMode cellReferenceMode, CultureInfo culture, bool ignoreWhitespace )
			: base( formula )
		{
			// MD 6/13/12 - CalcEngineRefactor
			//this.cellReferenceMode = cellReferenceMode;

			this.ignoreWhitespace = ignoreWhitespace;

			// MD 7/14/08 - Excel formula solving
			// We may need this without calling the Evaluate method, so just create it in the constructor.
			this.whitespaceTokens = new List<AttrSpaceToken>();

			// MD 6/13/12 - CalcEngineRefactor
			//// MD 5/13/11 - Data Validations / Page Breaks
			//// We allow a null culture.
			////this.culture = culture;
			//this.culture = culture ?? CultureInfo.CurrentCulture;
			//
			//this.cachedDecimalSeparator = this.culture.NumberFormat.NumberDecimalSeparator;
			CultureInfo cultureResolved = culture ?? CultureInfo.CurrentCulture;
			this.cachedDecimalSeparator = cultureResolved.NumberFormat.NumberDecimalSeparator;

			this.unionOperatorResolved = FormulaParser.GetUnionOperatorResolved( this.cachedDecimalSeparator );

			this.formulaContext = new FormulaContext(formula, cellReferenceMode, cultureResolved);
		}

		#endregion Constructor

		// MD 5/25/11 - Data Validations / Page Breaks
		#region CombineWhitespaceAndOperandStrings

		protected virtual string CombineWhitespaceAndOperandStrings(FormulaToken currentToken, Dictionary<WorkbookReferenceBase, int> externalReferences, string whitespace)
		{
			return whitespace + currentToken.ToString(this.formulaContext, externalReferences);
		}

		#endregion  // CombineWhitespaceAndOperandStrings

		#region GetWhitespace

		private string GetWhitespace( List<AttrSpaceToken> whitespaceTokens, params WhitespaceType[] whitespaceTypes )
		{
			string whitespace = string.Empty;

			for ( int i = whitespaceTokens.Count - 1; i >= 0; i-- )
			{
				AttrSpaceToken currentToken = whitespaceTokens[ i ];

				foreach ( WhitespaceType type in whitespaceTypes )
				{
					if ( currentToken.WhitespaceType == type )
					{
						whitespaceTokens.RemoveAt( i );
						whitespace = currentToken.ToString(this.formulaContext, this.ExternalReferences) + whitespace;
						break;
					}
				}
			}

			return whitespace;
		}

		#endregion GetWhitespace

		#region ExternalReferences

		protected virtual Dictionary<WorkbookReferenceBase, int> ExternalReferences
		{
			get { return null; }
		}

		#endregion // ExternalReferences

		// MD 2/4/11 - TFS65015
		#region IsForSaving

		public bool IsForSaving
		{
			get { return this.isForSaving; }
			set { this.isForSaving = value; }
		}

		#endregion // IsForSaving

		// MD 8/18/08 - Excel formula solving
		// The function name is no longer passed as a string, it is now passed as the type on the evaluation stack, so we need to pass the
		// function name back when the manager is asking for an evaluation stack.
		protected override string CreateFunctionNameEvalutationItem( string functionName )
		{
			return functionName;
		}

		public override EvaluationResult<string> Evaluate()
		{
			// MD 7/14/08 - Excel formula solving
			// We may need this without calling the Evaluate method, so just create it in the constructor.
			// Instead, clear it here so we know its an empty list.
			//this.whitespaceTokens = new List<AttrSpaceToken>();
			this.whitespaceTokens.Clear();

			EvaluationResult<string> result = base.Evaluate();

			if ( result.Completed == false )
				return result;

			return new EvaluationResult<string>( "=" + result.Result );
		}

		protected override bool EvaluateBinary( FormulaToken currentToken, string firstOperand, string secondOperand )
		{
			string whitespace = this.GetWhitespace(
				this.whitespaceTokens,
				WhitespaceType.SpacesBeforeNextToken,
				WhitespaceType.CarriageReturnsBeforeNextToken );

			string portionInFormula = firstOperand + whitespace + currentToken.ToString(this.formulaContext, this.ExternalReferences) + secondOperand;
			this.PushStringOnEvaluationStack(currentToken, portionInFormula);

			return true;
		}

		// MD 10/8/07 - BR27172
		// An extra parameter is now passed to the method
		//protected override bool EvaluateFunction( FormulaToken currentToken, string[] arguments )
		protected override bool EvaluateFunction( FormulaToken currentToken, string functionName, string[] arguments )
		{
			// MD 2/4/11 - TFS65015
			// When saving in the 2007 format and the function is an add-in function, the name must be preceeded by "_xll.".
			if (this.IsForSaving && Utilities.Is2003Format(this.Formula.CurrentFormat) == false)
			{
				FunctionOperator function = currentToken as FunctionOperator;

				// MD 8/29/11 - TFS85072
				// Don't add in the _xll prefix if this is an Excel 2007 only function. They are only add-in functions in Excel 2003.
				//if (function != null && function.Function.IsAddIn)
				if (function != null && function.Function.IsAddIn && function.Function.IsExcel2007OnlyFunction == false)
					functionName = "_xll." + functionName;
			}

			string whitespaceBeforeParen = this.GetWhitespace(
				whitespaceTokens,
				WhitespaceType.SpacesBeforeClosingParens,
				WhitespaceType.CarriageReturnsBeforeClosingParens );

			string whitespaceBeforeName = this.GetWhitespace(
				whitespaceTokens,
				WhitespaceType.SpacesBeforeNextToken,
				WhitespaceType.CarriageReturnsBeforeNextToken );

			// MD 10/8/07 - BR27172
			// Use the function name which is passed to the function instead of trying to figure it out here
			//StringBuilder sb = new StringBuilder( whitespaceBeforeName + currentToken.ToString( this.Formula.OwningCell, this.cellReferenceMode ) + "(" );
			StringBuilder sb = new StringBuilder( whitespaceBeforeName + functionName + "(" );

			for ( int i = 0; i < arguments.Length; i++ )
			{
				sb.Append( arguments[ i ] );

				if ( i != arguments.Length - 1 )
				{
					// MD 9/11/09 - TFS20376
					// The UnionOperator will be different based on the culture, use the member variable instead.
					//sb.Append( FormulaParser.UnionOperator );
					sb.Append( this.unionOperatorResolved );
				}
			}

			sb.Append( whitespaceBeforeParen + ")" );

			// MD 10/22/10 - TFS36696
			// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
			//currentToken.PortionInFormula = sb.ToString();
			//this.EvaluationStack.Push( currentToken.PortionInFormula );
			string portionInFormula = sb.ToString();
			this.PushStringOnEvaluationStack(currentToken, portionInFormula);

			return true;
		}

		protected override bool EvaluateLeftAssociativeUnary( FormulaToken currentToken, string firstOperand )
		{
			string whitespace = this.GetWhitespace(
				this.whitespaceTokens,
				WhitespaceType.SpacesBeforeNextToken,
				WhitespaceType.CarriageReturnsBeforeNextToken );

			string portionInFormula = firstOperand + whitespace + currentToken.ToString(this.formulaContext, this.ExternalReferences);
			this.PushStringOnEvaluationStack(currentToken, portionInFormula);

			return true;
		}

        protected override bool EvaluateOperand(FormulaToken currentToken)
        {
			string whitespace = this.GetWhitespace(
				this.whitespaceTokens,
				WhitespaceType.SpacesBeforeNextToken,
				WhitespaceType.CarriageReturnsBeforeNextToken );

			// MD 10/22/10 - TFS36696
			// The token no longer stores the formula, so it needs to be passed into this method, and we can get the source cell from the formula.
			// Also, to save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
			//currentToken.PortionInFormula = whitespace + currentToken.ToString( this.Formula.OwningCell, this.cellReferenceMode, this.culture, externalReferences );
			//
			//this.EvaluationStack.Push( currentToken.PortionInFormula );
			// MD 5/25/11 - Data Validations / Page Breaks
			// Some derived generators may need to get involved in the process here.
			//string portionInFormula = whitespace + currentToken.ToString(this.Formula, this.cellReferenceMode, this.culture, externalReferences);
			string portionInFormula = this.CombineWhitespaceAndOperandStrings(currentToken, this.ExternalReferences, whitespace);

			this.PushStringOnEvaluationStack(currentToken, portionInFormula);

			return true;
		}

		protected override bool EvaluateParens( FormulaToken currentToken, string parenContents )
		{
			string whitespaceBeforeOpen = this.GetWhitespace(
				whitespaceTokens,
				WhitespaceType.SpacesBeforeOpeningParens,
				WhitespaceType.CarriageReturnsBeforeOpeningParens );

			string whitespaceBeforeClose = this.GetWhitespace(
				whitespaceTokens,
				WhitespaceType.SpacesBeforeClosingParens,
				WhitespaceType.CarriageReturnsBeforeClosingParens );

			// MD 10/22/10 - TFS36696
			// To save space, we will no longer save this on the token. It is only needed when parsing formulas anyway.
			//currentToken.PortionInFormula = whitespaceBeforeOpen + "(" + parenContents + whitespaceBeforeClose + ")";
			//this.EvaluationStack.Push( currentToken.PortionInFormula );
			string portionInFormula = whitespaceBeforeOpen + "(" + parenContents + whitespaceBeforeClose + ")";
			this.PushStringOnEvaluationStack(currentToken, portionInFormula);

			return true;
		}

		protected override bool EvaluateRightAssociativeUnary( FormulaToken currentToken, string firstOperand )
		{
			string whitespace = this.GetWhitespace(
				this.whitespaceTokens,
				WhitespaceType.SpacesBeforeNextToken,
				WhitespaceType.CarriageReturnsBeforeNextToken );

			string portionInFormula = whitespace + currentToken.ToString(this.formulaContext, this.ExternalReferences) + firstOperand;
			this.PushStringOnEvaluationStack(currentToken, portionInFormula);
			return true;
		}

		protected override bool EvaluateSpecial( FormulaToken currentToken )
		{
			AttrSpaceToken space = currentToken as AttrSpaceToken;

			if ( space != null )
			{
				if ( this.ignoreWhitespace == false )
					this.whitespaceTokens.Add( space );

				return true;
			}

			if ( currentToken is ExpToken || currentToken is TblToken )
			{
				string portionInFormula = currentToken.ToString(this.formulaContext, this.ExternalReferences);
				this.PushStringOnEvaluationStack(currentToken, portionInFormula);
				return true;
			}

			return true;
		}

		private void PushStringOnEvaluationStack(FormulaToken currentToken, string tokenPortionInFormula)
		{
			if (tokenPortionsInFormula != null)
				tokenPortionsInFormula[currentToken] = tokenPortionInFormula;

			this.EvaluationStack.Push(tokenPortionInFormula);
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