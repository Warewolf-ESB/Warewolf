using System;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.CalcEngine
{
	internal class UltraCalcFormulaReferenceCollection : IExcelCalcReferenceCollection
	{
		private List<IExcelCalcReference> tokenArray;

		public UltraCalcFormulaReferenceCollection(ExcelCalcFormula formula)
		{
			tokenArray = new List<IExcelCalcReference>();

			if (formula.Tokens != null)
			{
				// MD 7/30/08 - Excel formula solving
				// We can't just add all references to the static references. Some references will never be dereferenced
				// and shouldn't be added. Instead, we need to determine which references are definitely going to be 
				// dereferenced and only add those.
				Stack<UltraCalcFormulaToken> tokenStack = new Stack<UltraCalcFormulaToken>();

				foreach (UltraCalcFormulaToken token in formula.Tokens)
				{
					// Value tokens should just be pushed on the stack
					if (token.Type == UltraCalcFormulaTokenType.Value)
					{
						tokenStack.Push(token);
						continue;
					}

					// Other tokens are assumed to be function tokens and should pop their arguments off the token stack.
					UltraCalcFunctionToken functionToken = (UltraCalcFunctionToken)token;

					// The arguments are in reverse order coming off the stack, so start the argument count at the end.
					for (int parameterIndex = functionToken.ArgumentCount - 1; parameterIndex >= 0; parameterIndex--)
					{
						this.TryAdd(formula, tokenStack.Pop()
							, functionToken.Function.WillParameterAlwaysBeDereferenced(parameterIndex)
							, functionToken.Function.GetExpectedParameterClass(parameterIndex)
							);
					}

					// Push the function token on the stack so it can be used by the next function
					tokenStack.Push(functionToken);
				}

				// After all tokens are processed, there should only be one token left on the stack.
				Debug.Assert(tokenStack.Count == 1);

				// Add all remaining references on the stack to the static references collection.
				while (tokenStack.Count > 0)
				{
					this.TryAdd(formula, tokenStack.Pop()
						, true
						, CalcUtilities.GetExpectedFormulaResultTokenClass(formula.BaseReference)
						);
				}
			}
		}

		#region Implementation of ICollection
		
		public void CopyTo(System.Array array, int index)
		{
			this.tokenArray.CopyTo(array as IExcelCalcReference[], index);	
		}
		public bool IsSynchronized
		{
			get
			{
				return CalcManagerUtilities.IsSynchronized(this.tokenArray);
			}
		}

		public int Count
		{
			get
			{
				return this.tokenArray.Count;
			}
		}

		public object SyncRoot
		{
			get
			{
				return CalcManagerUtilities.SyncRoot(this.tokenArray);
			}
		}
		#endregion

		#region Implementation of IEnumerable
		
		public System.Collections.IEnumerator GetEnumerator()
		{
			return this.tokenArray.GetEnumerator();
		}
		#endregion

		#region Contains

		internal bool Contains(IExcelCalcReference reference, bool checkEquality)
		{
			foreach (IExcelCalcReference existingRef in this.tokenArray)
			{
				if (checkEquality)
				{
					if (reference.Equals(existingRef))
						return true;

					Debug.Assert(
						ExcelCalcEngine.GetResolvedReference(reference).Equals(ExcelCalcEngine.GetResolvedReference(existingRef)) == false,
						"The references should be resolved before they are compared in here.");
				}
				else
				{
					if (reference.ContainsReference(existingRef))
						return true;
				}
			}

			return false;
		}

		#endregion Contains

		#region TryAdd

		private void TryAdd(ExcelCalcFormula formula, UltraCalcFormulaToken currentArgument
			, bool willParameterAlwaysBeDereferenced
			, TokenClass expectedParameterClass
 )
		{
			// Only value tokens can be static references
			if (currentArgument.Type != UltraCalcFormulaTokenType.Value)
				return;

			UltraCalcValueToken valueArgument = (UltraCalcValueToken)currentArgument;

			// Only reference values can be added as static tokens, so ignore it if it is not a reference.
			if (valueArgument.Value.IsReference == false)
				return;

			// MD 7/12/12 - TFS109194
			// Dynamic references (such as relative references in named reference formulas) are not statically referenced.
			// The reference will change depending on which cell's formula is using it.
			if (valueArgument.Value.IsDynamicReference)
				return;

			IExcelCalcReference reference = (IExcelCalcReference)valueArgument.Value.Value;

			// If the parameter may not be dereferenced, don't add it as a static reference.
			if (willParameterAlwaysBeDereferenced == false)
				return;

			// MD 8/4/08 - Excel formula solving
			// If the formula is an array formula and the parameter is a range that will be split up, we don't want to 
			// make a static reference to the range, because only a single value from the range will be used.
			if (formula.IsArrayFormula && expectedParameterClass == TokenClass.Value)
			{
				IExcelCalcReference resolvedReference = ExcelCalcEngine.GetResolvedReference(reference);

				WorksheetRegion region = resolvedReference.Context as WorksheetRegion;

				// Go to the next token when it is a range with multiple cells.
				if (region != null && region.IsSingleCell == false)
					return;

				List<WorksheetRegion> regions = resolvedReference.Context as List<WorksheetRegion>;

				if (regions != null)
				{
					// Go to the next token when it is a range with multiple cells.
					if (regions.Count != 1 || regions[0].IsSingleCell == false)
						return;
				}
			}

			// Add the reference to the static references collection
			this.tokenArray.Add(reference);

			return;
		}

		#endregion TryAdd
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