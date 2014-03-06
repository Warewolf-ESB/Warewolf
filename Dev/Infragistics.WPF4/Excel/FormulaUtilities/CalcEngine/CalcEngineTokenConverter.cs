using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities.CalcEngine
{
	internal class CalcEngineTokenConverter : EvaluationManager<CalcEngineTokenConverter.TokenHolder>
	{
		#region Member Variables

		private List<FormulaToken> filteredTokens;
		private UltraCalcFunctionFactory functionFactory;
		private Dictionary<FormulaToken, TokenHolder> tokenHolders;

		#endregion Member Variables

		#region Constructor

		private CalcEngineTokenConverter(ExcelCalcFormula calcFormula, Workbook workbook)
			: base(calcFormula.ExcelFormula)
		{
			this.filteredTokens = new List<FormulaToken>();
			this.functionFactory = workbook.CalcEngine.FunctionFactory;
			this.tokenHolders = new Dictionary<FormulaToken, TokenHolder>();
		}

		#endregion Constructor

		#region Base Class Overrides

		#region Evaluate

		public override EvaluationResult<TokenHolder> Evaluate()
		{
			EvaluationResult<TokenHolder> result = base.Evaluate();
			Debug.Assert( result.Completed, "The evaluation should have completed." );

			// MD 7/6/09 - TFS18865
			// Use the virtual property instead of getting the collection directly off the formula.
			//foreach ( FormulaToken token in this.Formula.PostfixTokenList )
			foreach ( FormulaToken token in this.PostfixTokenList )
			{
				TokenHolder holder = tokenHolders[ token ];

				if ( holder.Ignore )
				{
					if ( holder.ReplacementToken != null )
						this.filteredTokens.Add( holder.ReplacementToken );

					continue;
				}

				this.filteredTokens.Add( token );
			}

			return result;
		}

		#endregion Evaluate

		#region EvaluateBinary

		protected override bool EvaluateBinary( FormulaToken currentToken, TokenHolder firstOperand, TokenHolder secondOperand )
		{
			this.EvaluationStack.Push( this.CreateTokenHolder( currentToken, firstOperand, secondOperand ) );
			return true;
		}

		#endregion EvaluateBinary

		#region EvaluateFunction

		protected override bool EvaluateFunction( FormulaToken currentToken, TokenHolder functionName, TokenHolder[] arguments )
		{
			TokenHolder holder = this.CreateTokenHolder( currentToken, arguments );

			FunctionOperator functionOperator = currentToken as FunctionOperator;

			if ( functionOperator != null )
			{
				if ( functionName != null && functionOperator.Function.IsAddIn )
					functionName.Ignore = true;

				string functionNameStr = functionOperator.Function.Name;
				if ( functionOperator.Function == Function.ERROR_TYPE )
				{
					// The ERRROR.TYPE function name should be converted to ERRORTYPE.
					int dotIndex = functionNameStr.IndexOf( '.' );
					functionNameStr = functionNameStr.Substring( 0, dotIndex ) + functionNameStr.Substring( dotIndex + 1 );
				}

				if (this.functionFactory[functionNameStr] == null)
				{
					// MD 10/22/10 - TFS36696
					// We don't need to store the formula on the token anymore.
					//holder.ReplacementToken = new ErrToken( this.calcFormula.ExcelFormula, ErrorValue.WrongFunctionName );
					holder.ReplacementToken = new ErrToken(ErrorValue.WrongFunctionName);
				}
			}

			this.EvaluationStack.Push( holder );
			return true;
		}

		#endregion EvaluateFunction

		#region EvaluateLeftAssociativeUnary

		protected override bool EvaluateLeftAssociativeUnary( FormulaToken currentToken, TokenHolder firstOperand )
		{
			this.EvaluationStack.Push( this.CreateTokenHolder( currentToken, firstOperand ) );
			return true;
		}

		#endregion EvaluateLeftAssociativeUnary

		#region EvaluateOperand

		protected override bool EvaluateOperand( FormulaToken currentToken )
		{
			this.EvaluationStack.Push( this.CreateTokenHolder( currentToken ) );
			return true;
		}

		#endregion EvaluateOperand

		#region EvaluateParens

		protected override bool EvaluateParens( FormulaToken currentToken, TokenHolder parenContents )
		{
			this.EvaluationStack.Push( this.CreateTokenHolder( currentToken, parenContents ) );
			return true;
		}

		#endregion EvaluateParens

		#region EvaluateRightAssociativeUnary

		protected override bool EvaluateRightAssociativeUnary( FormulaToken currentToken, TokenHolder firstOperand )
		{
			this.EvaluationStack.Push( this.CreateTokenHolder( currentToken, firstOperand ) );
			return true;
		}

		#endregion EvaluateRightAssociativeUnary

		#region EvaluateSpecial

		protected override bool EvaluateSpecial( FormulaToken currentToken )
		{
			TokenHolder holder = this.CreateTokenHolder( currentToken );
			holder.Ignore = true;
			return true;
		}

		#endregion EvaluateSpecial

		#endregion Base Class Overrides

		#region Methods

		#region ConvertOperandToken

		private static UltraCalcFormulaToken ConvertOperandToken( OperandToken operandToken, ExcelCalcFormula formula, Workbook workbook )
		{
			CellCalcReference cellReference = formula.BaseReference as CellCalcReference;

			FormulaContext context = new FormulaContext(workbook, formula.ExcelFormula);
			if (cellReference != null)
				context = new FormulaContext(cellReference.Row.Worksheet, cellReference.Row.Index, cellReference.ColumnIndex, workbook.CurrentFormat, formula.ExcelFormula);
			else
				context = new FormulaContext(workbook);

			object value = operandToken.GetCalcValue(context);

			IExcelCalcReference reference = value as IExcelCalcReference;

			if ( reference != null && ( reference is UCReference ) == false )
				value = new UCReference( reference );

			// MD 7/12/12 - TFS109194
			// If the formula is a named reference formula which has relative cell or region addresses, those relative addresses
			// will be dynamically offset at evaluation time, so mark them as dynamic references.
			//return new UltraCalcValueToken( new ExcelCalcValue( value ) );
			ExcelCalcValue calcValue = new ExcelCalcValue(value);
			if (formula.BaseReference is NamedCalcReferenceBase)
			{
				CellReferenceToken cellReferenceToken = operandToken as CellReferenceToken;
				if (cellReferenceToken != null && cellReferenceToken.HasRelativeAddresses)
					calcValue.IsDynamicReference = true;
			}

			return new UltraCalcValueToken(calcValue);
		}

		#endregion ConvertOperandToken

		#region ConvertOperatorToken

		private UltraCalcFormulaToken ConvertOperatorToken(OperatorToken operatorToken)
		{
			ExcelCalcFunction function = null;
			int argumentCount = 2;

			switch ( operatorToken.Token )
			{
				case Token.Isect:
					function = this.functionFactory["XLIntersect"];
					break;

				case Token.Range:
					function = this.functionFactory["XLRange"];
					break;

				case Token.Union:
					function = this.functionFactory["XLUnion"];
					break;

				case Token.FuncA:
				case Token.FuncR:
				case Token.FuncV:
				case Token.FuncVarA:
				case Token.FuncVarR:
				case Token.FuncVarV:
					{
						FunctionOperator functionOperator = operatorToken as FunctionOperator;

						if ( functionOperator == null )
						{
							Utilities.DebugFail( "Only a FunctionOperator should have had a Func... token value." );
							break;
						}

						string functionName = functionOperator.Function.Name;
						if ( functionOperator.Function == Function.ERROR_TYPE )
						{
							// The ERRROR.TYPE function name should be converted to ERRORTYPE.
							int dotIndex = functionName.IndexOf( '.' );
							functionName = functionName.Substring( 0, dotIndex ) + functionName.Substring( dotIndex + 1 );
						}

						function = this.functionFactory[functionName];
						argumentCount = functionOperator.NumberOfArguments;
					}
					break;

				case Token.Add:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Add);
					break;

				case Token.Concat:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Concatenate);
					break;

				case Token.Div:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Divide);
					break;

				case Token.EQ:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Equal);
					break;

				case Token.Power:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Exponent);
					break;

				case Token.GT:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.GreaterThan);
					break;

				case Token.GE:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.GreaterThanOrEqual);
					break;

				case Token.LT:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.LessThan);
					break;

				case Token.LE:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.LessThanOrEqual);
					break;

				case Token.Mul:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Multiply);
					break;

				case Token.NE:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.NotEqual);
					break;

				case Token.Percent:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Percent);
					argumentCount = 1;
					break;

				case Token.Sub:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.Subtract);
					break;

				case Token.Uminus:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.UnaryMinus);
					argumentCount = 1;
					break;

				case Token.Uplus:
					function = this.functionFactory.GetOperator(UltraCalcOperatorFunction.UnaryPlus);
					argumentCount = 1;
					break;

				default:
					function = this.functionFactory[operatorToken.ToString()];
					break;
			}

			if ( function == null )
			{
				Utilities.DebugFail( "Figure out what to do here." );
				return null;
			}

			return new UltraCalcFunctionToken( function, argumentCount );
		}

		#endregion ConvertOperatorToken

		#region ConvertToken

		private UltraCalcFormulaToken ConvertToken( FormulaToken token, ExcelCalcFormula formula, Workbook workbook )
		{
			OperandToken operandToken = token as OperandToken;

			if ( operandToken != null )
				return CalcEngineTokenConverter.ConvertOperandToken( operandToken, formula, workbook );

			OperatorToken operatorToken = token as OperatorToken;

			if ( operatorToken != null )
				return this.ConvertOperatorToken(operatorToken);

			switch ( token.Token )
			{
				case Token.Attr:
					{
						AttrTokenBase attrToken = token as AttrTokenBase;

						if ( attrToken == null )
						{
							Utilities.DebugFail( "Only a AttrTokenBase should have had a Attr token value." );
							return null;
						}

						if ( attrToken.Type == AttrTokenBase.AttrType.Sum )
							return new UltraCalcFunctionToken(this.functionFactory["SUM"], 1);

						return null;
					}

				case Token.Exp:
				case Token.Paren:
				case Token.MemAreaA:
				case Token.MemAreaV:
				case Token.MemAreaR:
				case Token.MemErrA:
				case Token.MemErrV:
				case Token.MemErrR:
				case Token.MemFuncA:
				case Token.MemFuncV:
				case Token.MemFuncR:
				case Token.MemNoMemA:
				case Token.MemNoMemV:
				case Token.MemNoMemR:
				case Token.Tbl:
					return null;

				default:
					Utilities.DebugFail( "A token was not handled." );
					return null;
			}
		}

		#endregion ConvertToken

		#region ConvertTokens

		public static List<UltraCalcFormulaToken> ConvertTokens(ExcelCalcFormula formula, Workbook workbook)
		{
			CalcEngineTokenConverter converter = new CalcEngineTokenConverter(formula, workbook);
			converter.Evaluate();

			List<UltraCalcFormulaToken> convertedTokens = new List<UltraCalcFormulaToken>();

			// MD 4/10/12 - TFS108678
			Stack<UltraCalcFormulaToken> tokenStack = new Stack<UltraCalcFormulaToken>();

			foreach ( FormulaToken token in converter.FilteredTokens )
			{
				UltraCalcFormulaToken convertedToken = converter.ConvertToken(token, formula, workbook);

				if ( convertedToken != null )
				{
					convertedTokens.Add( convertedToken );

					// MD 4/10/12 - TFS108678
					// We need to create a tree of the function tokens by setting references to parent functions so that
					// functions can see where their results will be passed.
					if (convertedToken.Type == UltraCalcFormulaTokenType.Function)
					{
						UltraCalcFunctionToken functionToken = (UltraCalcFunctionToken)convertedToken;
						for (int i = 0; i < functionToken.ArgumentCount; i++)
						{
							UltraCalcFunctionToken childFunction = tokenStack.Pop() as UltraCalcFunctionToken;
							if (childFunction != null)
							{
								childFunction.ParentFunctionToken = functionToken;
								childFunction.ArgumentIndexInParentFunction = functionToken.ArgumentCount - 1 - i;
							}
						}
					}

					tokenStack.Push(convertedToken);
				}
			}

			return convertedTokens;
		}

		#endregion ConvertTokens

		#region CreateTokenHolder

		private TokenHolder CreateTokenHolder( FormulaToken currentToken, params TokenHolder[] arguments )
		{
			TokenHolder holder = new TokenHolder( arguments );
			this.tokenHolders.Add( currentToken, holder );

			return holder;
		}

		#endregion CreateTokenHolder

		#endregion Methods

		#region Properties

		#region FilteredTokens

		private List<FormulaToken> FilteredTokens
		{
			get { return this.filteredTokens; }
		}

		#endregion FilteredTokens

		#endregion Properties


		#region TokenHolder class

		internal class TokenHolder
		{
			#region Member Variables

			private TokenHolder[] arguments;
			private bool ignore;
			private FormulaToken replacementToken;

			#endregion Member Variables

			#region Constructor

			public TokenHolder( TokenHolder[] arguments )
			{
				this.arguments = arguments;
			}

			#endregion Constructor

			#region Properties

			#region Ignore

			public bool Ignore
			{
				get { return this.ignore; }
				set
				{
					if ( this.ignore == value )
						return;

					this.ignore = value;

					if ( this.ignore && this.arguments != null )
					{
						foreach ( TokenHolder token in this.arguments )
							token.Ignore = true;
					}
				}
			}

			#endregion Ignore

			#region ReplacementToken

			public FormulaToken ReplacementToken
			{
				get { return this.replacementToken; }
				set
				{
					if ( this.replacementToken == value )
						return;

					this.replacementToken = value;

					if ( this.replacementToken != null )
						this.Ignore = true;
				}
			}

			#endregion ReplacementToken

			#endregion Properties
		}

		#endregion TokenHolder class
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