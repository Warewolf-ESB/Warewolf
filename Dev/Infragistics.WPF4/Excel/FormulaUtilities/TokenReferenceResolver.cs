using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;






using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.FormulaUtilities

{
	internal class TokenReferenceResolver : EvaluationManager<FormulaToken>
	{
		private Stack<FormulaToken> ifAndChooseTokens;

		// MD 7/6/09 - TFS18865
		private List<FormulaToken> postfixTokenList;

		private Stack<AttrSkipToken> skipTokens;

		// MD 10/22/10 - TFS36696
		// Certain info is no longer stored on the tokens, so we need to store it in a dictionary. since this evaluator sets some of that info, 
		// it needs to take the dictionary.
		private Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream;

		// MD 10/22/10 - TFS36696
		// Certain info is no longer stored on the tokens, so we need to store it in a dictionary. since this evaluator sets some of that info, 
		// it needs to take the dictionary.
		//public TokenReferenceResolver( Formula formula )
		//    // MD 7/6/09 - TFS18865
		//    //	: base( formula ) { }
		//    : this( formula, formula.PostfixTokenList ) { }
		public TokenReferenceResolver(Formula formula, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
			: this(formula, formula.PostfixTokenList, tokenPositionsInRecordStream) { }

		// MD 7/6/09 - TFS18865
		// Added a new constructor to take an alternate token list
		// MD 10/22/10 - TFS36696
		// Certain info is no longer stored on the tokens, so we need to store it in a dictionary. since this evaluator sets some of that info, 
		// it needs to take the dictionary.
		//public TokenReferenceResolver( Formula formula, List<FormulaToken> postfixTokenList )
		public TokenReferenceResolver(Formula formula, List<FormulaToken> postfixTokenList, Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream)
			: base( formula )
		{
			this.postfixTokenList = postfixTokenList;

			// MD 10/22/10 - TFS36696
			// Certain info is no longer stored on the tokens, so we need to store it in a dictionary. since this evaluator sets some of that info, 
			// it needs to take the dictionary.
			this.tokenPositionsInRecordStream = tokenPositionsInRecordStream;
		}

		public override EvaluationResult<FormulaToken> Evaluate()
		{
			this.ifAndChooseTokens = new Stack<FormulaToken>();
			this.skipTokens = new Stack<AttrSkipToken>();

			return base.Evaluate();
		}

		protected override bool EvaluateBinary( FormulaToken currentToken, FormulaToken firstOperand, FormulaToken secondOperand )
		{
			// MD 10/22/10 - TFS36696
			// The FirstTokenInExpression is no longer stored on the token.
			//currentToken.FirstTokenInExpression = firstOperand.FirstTokenInExpression;
			this.CopyFirstTokenInExpression(currentToken, firstOperand);

			this.EvaluationStack.Push( currentToken );

			return true;
		}

		// MD 10/8/07 - BR27172
		// An extra parameter is now passed to the method
		//protected override bool EvaluateFunction( FormulaToken currentToken, FormulaToken[] arguments )
		// MD 8/18/08 - Excel formula solving
		// The function name is no longer passed as a string, it is now passed as the type on the evaluation stack.
		//protected override bool EvaluateFunction( FormulaToken currentToken, string functionName, FormulaToken[] arguments )
		protected override bool EvaluateFunction( FormulaToken currentToken, FormulaToken functionName, FormulaToken[] arguments )
		{
			if ( arguments.Length > 0 )
			{
				// MD 10/22/10 - TFS36696
				// The FirstTokenInExpression is no longer stored on the token.
				//currentToken.FirstTokenInExpression = arguments[ 0 ].FirstTokenInExpression;
				this.CopyFirstTokenInExpression(currentToken, arguments[0]);
			}

			FunctionOperator function = currentToken as FunctionOperator;

			if ( function != null )
			{
				// MD 10/8/07
				// Found while fixing BR27172
				// This is a better way to determine which function we are processing
				//if ( function.Function.Name == "IF" )
				if ( function.Function == Function.IF )
				{
					AttrIfToken ifToken = this.ifAndChooseTokens.Pop() as AttrIfToken;

					if ( ifToken == null )
					{
						Utilities.DebugFail( "There should have been an IF special token here" );
						return false;
					}

					ifToken.SetIfFunction( function );

					if ( arguments.Length == 2 )
						ifToken.SetFalseConditionJumpToToken( function );
					else if ( arguments.Length == 3 )
						ifToken.SetFalseConditionJumpToToken( arguments[ 2 ] );
					else
						Utilities.DebugFail( "The number of arguments was unexpected" );

					for ( int i = 1; i < arguments.Length; i++ )
						this.skipTokens.Pop().SetSkipAfterToken( function );
				}
				// MD 10/8/07
				// Found while fixing BR27172
				// This is a better way to determine which function we are processing
				//else if ( function.Function.Name == "CHOOSE" )
				else if ( function.Function == Function.CHOOSE )
				{
					AttrChooseToken chooseToken = this.ifAndChooseTokens.Pop() as AttrChooseToken;

					if ( chooseToken == null )
					{
						Utilities.DebugFail( "There should have been a CHOOSE special token here" );
						return false;
					}

					chooseToken.SetChooseFunction( function );

					for ( int i = 1; i < arguments.Length; i++ )
					{
						chooseToken.ChooseOptions.Add( arguments[ i ] );
						this.skipTokens.Pop().SetSkipAfterToken( function );
					}
				}
			}

			this.EvaluationStack.Push( currentToken );
			return true;
		}

		protected override bool EvaluateLeftAssociativeUnary( FormulaToken currentToken, FormulaToken firstOperand )
		{
			// MD 10/22/10 - TFS36696
			// The FirstTokenInExpression is no longer stored on the token.
			//currentToken.FirstTokenInExpression = firstOperand.FirstTokenInExpression;
			this.CopyFirstTokenInExpression(currentToken, firstOperand);

			this.EvaluationStack.Push( currentToken );
			return true;
		}

		protected override bool EvaluateOperand( FormulaToken currentToken )
		{
			// MD 2/27/10 - TFS27559
			// This doesn't seem to be necessary for the fix for TFS18865 and it causes an issue when spaces are used before 
			// the token passed into this method, because it should really be the FirstTokenInExpression and we would end up
			// overwriting it here.
			//// MD 7/6/09 - TFS18865
			//// Make sure the FirstTokenInExpression is updated for the token.
			//currentToken.FirstTokenInExpression = currentToken;

			this.EvaluationStack.Push( currentToken );
			return true;
		}

		protected override bool EvaluateParens( FormulaToken currentToken, FormulaToken parenContents )
		{
			// MD 10/22/10 - TFS36696
			// The FirstTokenInExpression is no longer stored on the token.
			//currentToken.FirstTokenInExpression = parenContents.FirstTokenInExpression;
			this.CopyFirstTokenInExpression(currentToken, parenContents);

			this.EvaluationStack.Push( currentToken );
			return true;
		}

		protected override bool EvaluateRightAssociativeUnary( FormulaToken currentToken, FormulaToken firstOperand )
		{
			// MD 10/22/10 - TFS36696
			// The FirstTokenInExpression is no longer stored on the token.
			//currentToken.FirstTokenInExpression = firstOperand.FirstTokenInExpression;
			this.CopyFirstTokenInExpression(currentToken, firstOperand);

			this.EvaluationStack.Push( currentToken );
			return true;
		}

		protected override bool EvaluateSpecial( FormulaToken currentToken )
		{
			MemOperatorBase memOperator = currentToken as MemOperatorBase;

			if ( memOperator != null )
			{
				// MD 7/6/09 - TFS18865
				// Use the virtual property instead of getting the collection directly off the formula.
				//int index = this.Formula.PostfixTokenList.IndexOf( memOperator );
				//this.Formula.PostfixTokenList[ index + 1 ].FirstTokenInExpression = memOperator.FirstTokenInExpression;
				int index = this.PostfixTokenList.IndexOf( memOperator );

				// MD 10/22/10 - TFS36696
				// The FirstTokenInExpression is no longer stored on the token.
				//this.PostfixTokenList[ index + 1 ].FirstTokenInExpression = memOperator.FirstTokenInExpression;
				this.CopyFirstTokenInExpression(this.PostfixTokenList[index + 1], memOperator);

				return true;
			}

			AttrSpaceToken spaceToken = currentToken as AttrSpaceToken;

			if ( spaceToken != null )
			{
				// MD 7/6/09 - TFS18865
				// Use the virtual property instead of getting the collection directly off the formula.
				//int index = this.Formula.PostfixTokenList.IndexOf( spaceToken );
				//this.Formula.PostfixTokenList[ index + 1 ].FirstTokenInExpression = spaceToken.FirstTokenInExpression;
				int index = this.PostfixTokenList.IndexOf( spaceToken );

				// MD 10/22/10 - TFS36696
				// The FirstTokenInExpression is no longer stored on the token.
				//this.PostfixTokenList[ index + 1 ].FirstTokenInExpression = spaceToken.FirstTokenInExpression;
				this.CopyFirstTokenInExpression(this.PostfixTokenList[index + 1], spaceToken);

				return true;
			}

			AttrSkipToken skipToken = currentToken as AttrSkipToken;

			if ( skipToken != null )
			{
				this.skipTokens.Push( skipToken );
				return true;
			}

			if ( currentToken is AttrChooseToken || currentToken is AttrIfToken )
			{
				this.ifAndChooseTokens.Push( currentToken );
				return true;
			}

			return true;
		}

		// MD 7/6/09 - TFS18865
		// Provide a different token list becasue it might have non shared equivalents of the formula tokens.
		protected override List<FormulaToken> PostfixTokenList
		{
			get { return this.postfixTokenList; }
		}

		// MD 10/22/10 - TFS36696
		private void CopyFirstTokenInExpression(FormulaToken targetToken, FormulaToken sourceToken)
		{
			this.SetFirstTokenInExpression(targetToken, this.GetFirstTokenInExpression(sourceToken));
		}

		// MD 10/22/10 - TFS36696
		private FormulaToken GetFirstTokenInExpression(FormulaToken token)
		{
			TokenPositionInfo positionInfo;
			if (this.tokenPositionsInRecordStream.TryGetValue(token, out positionInfo))
				return positionInfo.FirstTokenInExpression;

			return token;
		}

		// MD 10/22/10 - TFS36696
		private void SetFirstTokenInExpression(FormulaToken token, FormulaToken firstTokenInExpression)
		{
			TokenPositionInfo positionInfo;
			if (this.tokenPositionsInRecordStream.TryGetValue(token, out positionInfo))
				positionInfo.FirstTokenInExpression = firstTokenInExpression;
			else
				this.tokenPositionsInRecordStream[token] = new TokenPositionInfo(-1, firstTokenInExpression);
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