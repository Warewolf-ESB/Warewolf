using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;






using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities

{
	internal class TokenClassResolver : EvaluationManager<TokenClassResolver.FormulaTokenNode>
	{
		private FormulaType formulaType;

		public TokenClassResolver( Formula formula, FormulaType formulaType )
			: base( formula )
		{
			this.formulaType = formulaType;
		}

		public override EvaluationResult<FormulaTokenNode> Evaluate()
		{
			EvaluationResult<FormulaTokenNode> result = base.Evaluate();

			if ( result.Completed == false )
				return result;

			// MD 12/21/11 - TFS97840
			// Data validation formulas need to be parsed slightly differently, so they need their own formula types.
			// Also, external named references should have a reference token class as well (although we probably never get
			// in here with that type anyway).
			//if ( formulaType == FormulaType.NamedReferenceFormula )
			//    result.Result.SetTokenClass( TokenClass.Reference, false );
			//else
			//    result.Result.SetTokenClass( TokenClass.Value, false );
			switch (formulaType)
			{
				case FormulaType.NamedReferenceFormula:
				case FormulaType.ExternalNamedReferenceFormula:
				case FormulaType.ListDataValidationFormula:
					result.Result.SetTokenClass(TokenClass.Reference, false);
					break;

				default:
					result.Result.SetTokenClass(TokenClass.Value, false);
					break;
			}

			return result;
		}

		protected override bool EvaluateBinary( FormulaToken currentToken, FormulaTokenNode firstOperand, FormulaTokenNode secondOperand )
		{
			switch ( currentToken.Token )
			{
				case Token.Isect:
				case Token.Union:
				case Token.Range:
					if ( firstOperand.TokenClass != TokenClass.Reference )
						return false;

					if ( secondOperand.TokenClass != TokenClass.Reference )
						return false;
					
					break;
			}

			// MD 7/24/07
			// Parameter added to the FormulaTokenNode constructor
			//this.EvaluationStack.Push( new FormulaTokenNode( currentToken, firstOperand, secondOperand ) );
			this.EvaluationStack.Push( new FormulaTokenNode( this, currentToken, firstOperand, secondOperand ) );
			return true;
		}

		// MD 10/8/07 - BR27172
		// An extra parameter is now passed to the method
		//protected override bool EvaluateFunction( FormulaToken currentToken, FormulaTokenNode[] arguments )
		// MD 8/18/08 - Excel formula solving
		// The function name is no longer passed as a string, it is now passed as the type on the evaluation stack.
		//protected override bool EvaluateFunction( FormulaToken currentToken, string functionName, FormulaTokenNode[] arguments )
		protected override bool EvaluateFunction( FormulaToken currentToken, FormulaTokenNode functionName, FormulaTokenNode[] arguments )
		{
			FunctionOperator function = currentToken as FunctionOperator;

			// Valid the parameters to certain functions
			if ( function != null )
			{
				// MD 10/9/07 - BR27172
				// Other functions have parameter requriments like this. Instead of useing if statements, 
				// the function now stores which parameters need to be references.
				#region Old Code

				//if (
				//    function.Function == Function.AREAS ||
				//    function.Function == Function.COLUMN ||
				//    function.Function == Function.COUNTBLANK ||
				//    function.Function == Function.COUNTIF ||
				//    function.Function == Function.DAVERAGE ||
				//    function.Function == Function.DCOUNT ||
				//    function.Function == Function.DCOUNTA ||
				//    function.Function == Function.DGET ||
				//    function.Function == Function.DMAX ||
				//    function.Function == Function.DMIN ||
				//    function.Function == Function.DPRODUCT ||
				//    function.Function == Function.DSTDEV ||
				//    function.Function == Function.DSTDEVP ||
				//    function.Function == Function.DSUM ||
				//    function.Function == Function.DVAR ||
				//    function.Function == Function.DVARP ||
				//    function.Function == Function.OFFSET ||
				//    function.Function == Function.PHONETIC ||
				//    function.Function == Function.ROW ||
				//    function.Function == Function.SUMIF
				//    )
				//{
				//    if ( arguments[ 0 ].TokenClass != TokenClass.Reference )
				//        return false;
				//}
				//
				//if ( 
				//    function.Function == Function.CELL ||
				//    function.Function == Function.RANK ||
				//    function.Function == Function.SUBTOTAL
				//    )
				//{
				//    if ( arguments[ 1 ].TokenClass != TokenClass.Reference )
				//        return false;
				//}
				//
				//if (
				//    function.Function == Function.SUMIF
				//    )
				//{
				//    if ( arguments[ 2 ].TokenClass != TokenClass.Reference )
				//        return false;
				//}

				#endregion Old Code
				if ( function.Function.ForcedReferenceIndices != null )
				{
					foreach ( int index in function.Function.ForcedReferenceIndices )
					{
                        // MBS 7/10/08 - Excel 2007 Format
                        // This is going to cause an exception if we provide any number of arguments
                        // less than the maximum (i.e. if we provide 5 parameters when the function allows
                        // anywhere between 1 and 30), so we should also check this.
                        //
                        //if ( arguments[ index ].TokenClass != TokenClass.Reference )
						if ( index < arguments.Length && arguments[ index ].TokenClass != TokenClass.Reference )
						{
							// MD 7/25/08
							// Found while implementing Excel formula solving
							// Forced reference parameter arguments can take #REF! errors as a parameter.
							ErrToken errorToken = arguments[ index ].FormulaToken as ErrToken;
							if ( errorToken != null && errorToken.Value == ErrorValue.InvalidCellReference )
								continue;

							return false;
						}
					}
				}
			}

			// MD 7/24/07
			// Parameter added to the FormulaTokenNode constructor
			//this.EvaluationStack.Push( new FormulaTokenNode( currentToken, arguments ) );
			this.EvaluationStack.Push( new FormulaTokenNode( this, currentToken, arguments ) );
			return true;
		}

		protected override bool EvaluateLeftAssociativeUnary( FormulaToken currentToken, FormulaTokenNode firstOperand )
		{
			// MD 7/24/07
			// Parameter added to the FormulaTokenNode constructor
			//this.EvaluationStack.Push( new FormulaTokenNode( currentToken, firstOperand ) );
			this.EvaluationStack.Push( new FormulaTokenNode( this, currentToken, firstOperand ) );
			return true;
		}

		protected override bool EvaluateOperand( FormulaToken currentToken )
		{
			// MD 7/24/07
			// Parameter added to the FormulaTokenNode constructor
			//this.EvaluationStack.Push( new FormulaTokenNode( currentToken ) );
			this.EvaluationStack.Push( new FormulaTokenNode( this, currentToken ) );
			return true;
		}

		protected override bool EvaluateParens( FormulaToken currentToken, FormulaTokenNode parenContents )
		{
			// MD 7/24/07
			// Parameter added to the FormulaTokenNode constructor
			//this.EvaluationStack.Push( new FormulaTokenNode( currentToken, parenContents ) );
			this.EvaluationStack.Push( new FormulaTokenNode( this, currentToken, parenContents ) );
			return true;
		}

		protected override bool EvaluateRightAssociativeUnary( FormulaToken currentToken, FormulaTokenNode firstOperand )
		{
			// MD 7/24/07
			// Parameter added to the FormulaTokenNode constructor
			//this.EvaluationStack.Push( new FormulaTokenNode( currentToken, firstOperand ) );
			this.EvaluationStack.Push( new FormulaTokenNode( this, currentToken, firstOperand ) );
			return true;
		}

		protected override bool EvaluateSpecial( FormulaToken currentToken )
		{
			return true;
		}

		internal class FormulaTokenNode
		{
			private FormulaToken formulaToken;
			private FormulaTokenNode[] paramNodes;

			// MD 7/24/07
			private TokenClassResolver owner;

			// MD 7/24/07
			// Added new parameter
			//public FormulaTokenNode( FormulaToken formulaToken, params FormulaTokenNode[] paramNodes )
			public FormulaTokenNode( TokenClassResolver owner, FormulaToken formulaToken, params FormulaTokenNode[] paramNodes )
			{
				this.formulaToken = formulaToken;
				this.paramNodes = paramNodes;

				// MD 7/24/07
				this.owner = owner;
			}

			public void SetTokenClass( TokenClass tokenClass, bool forceArrayClass )
			{
				if ( tokenClass == TokenClass.Reference )
				{
					// MD 7/24/07
					// If the formula this token belongs to is not a cell type formula, the forceArrayClass flag is true 
					switch ( this.owner.formulaType )
					{
						case FormulaType.ArrayFormula:
						case FormulaType.NamedReferenceFormula:
						case FormulaType.ExternalNamedReferenceFormula:
							forceArrayClass = true;
							break;
					}

					if ( this.formulaToken.TokenClass == TokenClass.Value && forceArrayClass )
						this.formulaToken.TokenClass = TokenClass.Array;
				}
				else if ( tokenClass == TokenClass.Value )
				{
					if ( forceArrayClass )
						this.formulaToken.TokenClass = TokenClass.Array;
					else
						this.formulaToken.TokenClass = TokenClass.Value;
				}
				else if ( tokenClass == TokenClass.Array )
				{
					this.formulaToken.TokenClass = TokenClass.Array;
					forceArrayClass = true;
				}

				if ( this.formulaToken is ParenToken )
				{
					Debug.Assert( this.paramNodes.Length == 1 );
					this.paramNodes[ 0 ].SetTokenClass( tokenClass, forceArrayClass );
				}
				else
				{
					for ( int i = 0; i < this.paramNodes.Length; i++ )
						this.paramNodes[ i ].SetTokenClass( this.formulaToken.GetExpectedParameterClass( i ), forceArrayClass );
				}
			}

			public FormulaToken FormulaToken
			{
				get { return this.formulaToken; }
			}

			public TokenClass TokenClass
			{
				get 
				{
					if ( this.formulaToken is ParenToken )
						return this.paramNodes[ 0 ].TokenClass;

					return this.formulaToken.TokenClass; 
				}
			}
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