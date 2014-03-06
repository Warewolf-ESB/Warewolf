using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;






using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities

{
	internal abstract class EvaluationManager<T>
	{
		#region Member Variables

		private Formula formula;
		private Stack<T> evaluationStack;

		#endregion Member Variables

		#region Constructor

		protected EvaluationManager( Formula formula )
		{
			this.formula = formula;

			// MD 7/14/08 - Excel formula solving
			// We may need this without calling the Evaluate method, so just create it in the constructor.
			this.evaluationStack = new Stack<T>();
		}

		#endregion Constructor

		#region Methods

		// MD 8/18/08 - Excel formula solving
		// Becasue the EvaluateFunction method was changed, we need a way to convert the string function name to the 
		// evaluation stack type. 
		protected virtual T CreateFunctionNameEvalutationItem( string functionName )
		{
			return default( T );
		}

		#region Evaluate

		public virtual EvaluationResult<T> Evaluate()
		{
			// MD 7/14/08 - Excel formula solving
			// We may need this without calling the Evaluate method, so just create it in the constructor.
			// Instead, clear it here so we know its an empty stack.
			//this.evaluationStack = new Stack<T>();
			this.evaluationStack.Clear();

			// MD 7/6/09 - TFS18865
			// Use the virtual property instead of getting the collection directly off the formula.
			//foreach ( FormulaToken token in formula.PostfixTokenList )
			foreach ( FormulaToken token in this.PostfixTokenList )
			{
				if ( this.EvaluateToken( token ) == false )
					return new EvaluationResult<T>( token );
			}

			Debug.Assert( this.evaluationStack.Count == 1, "The result value should have been left on the evaluation stack." );
			if ( this.evaluationStack.Count != 1 )
				return new EvaluationResult<T>( null );

			return new EvaluationResult<T>( this.evaluationStack.Pop() );
		}

		#endregion Evaluate

		protected abstract bool EvaluateBinary( FormulaToken currentToken, T firstOperand, T secondOperand );

		// MD 10/8/07 - BR27172
		// Add an extra parameter because for add-in functions, we need the function name passed in, 
		// it is not stored with the Function instance of the FunctionToken.
		//protected abstract bool EvaluateFunction( FormulaToken currentToken, T[] arguments );
		// MD 8/18/08 - Excel formula solving
		// The function name argument must be in the type for the evaluation stack.
		//protected abstract bool EvaluateFunction( FormulaToken currentToken, string functionName, T[] arguments );
		protected abstract bool EvaluateFunction( FormulaToken currentToken, T functionName, T[] arguments );

		protected abstract bool EvaluateLeftAssociativeUnary( FormulaToken currentToken, T firstOperand );
		protected abstract bool EvaluateOperand( FormulaToken currentToken );
		protected abstract bool EvaluateParens( FormulaToken currentToken, T parenContents );
		protected abstract bool EvaluateRightAssociativeUnary( FormulaToken currentToken, T firstOperand );
		protected abstract bool EvaluateSpecial( FormulaToken currentToken );

		#region EvaluateToken

		public bool EvaluateToken( FormulaToken currentToken )
		{
			switch ( currentToken.Token )
			{
				case Token.Add:
				case Token.Sub:
				case Token.Mul:
				case Token.Div:
				case Token.Concat:
				case Token.LT:
				case Token.LE:
				case Token.EQ:
				case Token.GE:
				case Token.GT:
				case Token.NE:
				case Token.Isect:
				case Token.Union:
				case Token.Range:
				case Token.Power:
					{
						T secondOperand = this.evaluationStack.Pop();
						T firstOperand = this.evaluationStack.Pop();
						return this.EvaluateBinary( currentToken, firstOperand, secondOperand );
					}

				case Token.Uplus:
				case Token.Uminus:
					return this.EvaluateRightAssociativeUnary( currentToken, this.evaluationStack.Pop() );

				case Token.Percent:
					return this.EvaluateLeftAssociativeUnary( currentToken, this.evaluationStack.Pop() );

				case Token.MissArg:
				case Token.Str:
				case Token.Err:
				case Token.Bool:
				case Token.Int:
				case Token.Number:
				case Token.ArrayR:
				case Token.ArrayV:
				case Token.ArrayA:
				case Token.NameR:
				case Token.NameV:
				case Token.NameA:
				case Token.RefR:
				case Token.RefV:
				case Token.RefA:
				case Token.AreaR:
				case Token.AreaV:
				case Token.AreaA:
				case Token.RefErrR:
				case Token.RefErrV:
				case Token.RefErrA:
				case Token.AreaErrR:
				case Token.AreaErrV:
				case Token.AreaErrA:
				case Token.RefNR:
				case Token.RefNV:
				case Token.RefNA:
				case Token.AreaNR:
				case Token.AreaNV:
				case Token.AreaNA:
				case Token.NameXR:
				case Token.NameXV:
				case Token.NameXA:
				case Token.Ref3dR:
				case Token.Ref3dV:
				case Token.Ref3dA:
				case Token.Area3DR:
				case Token.Area3DV:
				case Token.Area3DA:
				case Token.RefErr3dR:
				case Token.RefErr3dV:
				case Token.RefErr3dA:
				case Token.AreaErr3dR:
				case Token.AreaErr3dV:
				case Token.AreaErr3dA:
				// MD 12/7/11 - 12.1 - Table Support
				case Token.StructuredTableReferenceR:
				case Token.StructuredTableReferenceV:
				case Token.StructuredTableReferenceA:
					return this.EvaluateOperand( currentToken );

				case Token.FuncR:
				case Token.FuncV:
				case Token.FuncA:
				case Token.FuncVarR:
				case Token.FuncVarV:
				case Token.FuncVarA:
					{
						FunctionOperator function = currentToken as FunctionOperator;

						Debug.Assert( function != null );
						if ( function == null )
							return false;

						T[] operands = new T[ function.NumberOfArguments ];

						for ( int i = function.NumberOfArguments - 1; i >= 0; i-- )
							operands[ i ] = this.evaluationStack.Pop();

						// MD 8/18/08 - Excel formula solving
						// The function name is no longer passed as a string, it is now passed as the type on the evaluation stack.
						//// MD 10/8/07 - BR27172
						//// An extra parameter containing the function name must be passed now
						////return this.EvaluateFunction( function, operands );
						//string functionName = function.Function.Name;
						//
						//// If the function is an add-in function, the function name is stored as a named reference 
						//// before all function parameters (below them on the stack).
						//if ( function.Function.IsAddIn )
						//    functionName = this.evaluationStack.Pop().ToString();
						//
						//return this.EvaluateFunction( function, functionName, operands );
						T functionName;

						// If the function is an add-in function, the function name is stored as a named reference 
						// before all function parameters (below them on the stack).
						if ( function.Function.IsAddIn )
							functionName = this.evaluationStack.Pop();
						else
							functionName = this.CreateFunctionNameEvalutationItem( function.Function.Name );

						return this.EvaluateFunction( function, functionName, operands );
					}

				case Token.Paren:
					return this.EvaluateParens( currentToken, this.evaluationStack.Pop() );

				case Token.Exp:
				case Token.Tbl:
				case Token.Extended:
				case Token.MemAreaR:
				case Token.MemAreaV:
				case Token.MemAreaA:
				case Token.MemErrR:
				case Token.MemErrV:
				case Token.MemErrA:
				case Token.MemNoMemR:
				case Token.MemNoMemV:
				case Token.MemNoMemA:
				case Token.MemFuncR:
				case Token.MemFuncV:
				case Token.MemFuncA:
					return this.EvaluateSpecial( currentToken );

				case Token.Attr:
					{
						if ( currentToken is AttrSumToken )
						{
							// MD 10/8/07 - BR27172
							// The function name must now be passed as a parameter
							//return this.EvaluateFunction( currentToken, new T[] { this.evaluationStack.Pop() } );
							// MD 8/18/08 - Excel formula solving
							// The function name is no longer passed as a string, it is now passed as the type on the evaluation stack.
							//return this.EvaluateFunction( currentToken, Function.SUM.Name, new T[] { this.evaluationStack.Pop() } );
							return this.EvaluateFunction( currentToken, this.CreateFunctionNameEvalutationItem( Function.SUM.Name ), new T[] { this.evaluationStack.Pop() } );
						}


						return this.EvaluateSpecial( currentToken );
					}

				default:
					Utilities.DebugFail( "Unhandled token: " + currentToken.Token );
					return false;
			}
		}

		#endregion EvaluateToken 

		#endregion Methods

		#region Properties

		#region EvaluationStack

		public Stack<T> EvaluationStack
		{
			get { return this.evaluationStack; }
		}

		#endregion EvaluationStack

		#region Formula

		public Formula Formula
		{
			get { return this.formula; }
		}

		#endregion Formula

		// MD 7/6/09 - TFS18865
		// Added a virtual property so derived types could provide alternate token lists.
		#region PostfixTokenList

		protected virtual List<FormulaToken> PostfixTokenList
		{
			get { return this.formula.PostfixTokenList; }
		}

		#endregion PostfixTokenList

		#endregion Properties
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