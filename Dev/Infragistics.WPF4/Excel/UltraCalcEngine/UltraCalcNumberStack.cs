using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)





using System.Drawing;


using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

using UltraCalcFunction = Infragistics.Documents.Excel.CalcEngine.ExcelCalcFunction;
using UltraCalcNumberStack = Infragistics.Documents.Excel.CalcEngine.ExcelCalcNumberStack;
using UltraCalcValue = Infragistics.Documents.Excel.CalcEngine.ExcelCalcValue;
using IUltraCalcReference = Infragistics.Documents.Excel.CalcEngine.IExcelCalcReference;





namespace Infragistics.Documents.Excel.CalcEngine





{
	/// <summary>
	/// Stack of <see cref="UltraCalcValue"/> instances used to evaluate formulas.  
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The number stack is used for evaluating formulas.  When formulas are compiled, the formula tokens are 
	/// placed in a collection in post-fix, or Reverse Polish Notation (RPN) format.  RPN format arranges the formula token list so each 
	/// sub-expressions's terms are placed before their operator, and sub-expressions are ordered to enforce correct operator precedence.
	/// This format allows the formula evaluate method to loop through the formula token collection front to back pushing an operator's terms onto 
	/// the number stack until an operator is reached.  Each time an operator is reached, it's subexpression is computed and resulting value pushed 
	/// onto the number stack.  Once the end of the end of the formula collection is reached, the formulas calculated value is at the top of the 
	/// number stack.
	/// </p>
	/// </remarks>






	public

		 class ExcelCalcNumberStack 



	{
		#region Member Variables

		// Stack structure providing storage and methods used to implement the number stack.
		
		// MD 4/10/12 - TFS108678
		// We may need random access to the values, so use a list.
        //private Stack<UltraCalcValue> numberStack;
		private List<UltraCalcValue> numberStack;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UltraCalcNumberStack"/>
		/// </summary>



        public ExcelCalcNumberStack()



		{
			// MD 4/10/12 - TFS108678
            //this.numberStack = new Stack<UltraCalcValue>();
			this.numberStack = new List<UltraCalcValue>();
		}
		#endregion //Constructor

		#region Methods
		/// <summary>
		/// Push a value onto number stack.
		/// </summary>
		/// <param name="value">Value to push onto the number stack</param>
		public void Push(UltraCalcValue value)
		{
			if (value == null)
				throw new ArgumentNullException();


			// MD 7/16/08 - Excel formula solving
			// The values on the stack need a reference to the number stack on which they reside.
			value.Owner = this;

			// MD 4/10/12 - TFS108678
			// Moved this logic from the BeforeReturnValue method to here because we will need to resolved value 
			// on the stack when doing random access into the stack as well as when popping off values.
			object resolvedValue = value.GetResolvedValue(false);
			ExcelCalcFormula formula = resolvedValue as ExcelCalcFormula;
			if (formula != null)
			{
				// MD 7/12/12 - TFS109194
				// If the named reference is not the result of an INDIRECT function (or created through some other dynamic means)
				// and its formula has relative references, they need to be offset for the calling function. For example, if the
				// name has a function of "=Sheet1!B2", and it is used from cell H7, the actual formula which needs to be solved
				// is "=Sheet1!I8", because the offset from A1 to B2 is the same as the offset from H7 to I8.
				if (value.IsDynamicReference == false &&
					formula.ExcelFormula != null &&
					formula.ExcelFormula.DoesHaveRelativeReferences())
				{
					Workbook workbook = this.Workbook;
					WorksheetCell owningCell = this.OwningCell;
					if (workbook != null && owningCell != null)
					{
						Formula offsetFormula = formula.ExcelFormula.Clone();
						offsetFormula.OffsetReferences(workbook, new Point(owningCell.ColumnIndex, owningCell.RowIndex));
						formula = workbook.CompileFormula(formula.BaseReference, offsetFormula);
					}
				}

				value = formula.EvaluateHelper(formula.BaseReference, this);
				value.Owner = this;
			}


			// MD 4/10/12 - TFS108678
			//this.numberStack.Push(value);
			this.numberStack.Add(value);
		}

		/// <summary>
		/// Pop value off top of the number stack.
		/// </summary>
		/// <returns><see cref="UltraCalcValue"/> that was at the top of the number stack.</returns>
		public UltraCalcValue Pop()
		{

			// MD 8/4/08 - Excel formula solving
			// Moved all code to the new overload
			return this.Pop( true );
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal UltraCalcValue Pop( bool performDataTableReplacements )
		{

			Debug.Assert(this.numberStack.Count > 0, "UltraCalcValue Pop: Stack underflow");

			// MD 7/16/08 - Excel formula solving
			// We may need to modify the value before returning it.
			//return (UltraCalcValue)this.numberStack.Pop();
			
			// MD 4/10/12 - TFS108678
			//UltraCalcValue peekValue = (UltraCalcValue)this.numberStack.Pop();
			UltraCalcValue peekValue = this.numberStack[this.numberStack.Count - 1];
			this.numberStack.RemoveAt(this.numberStack.Count - 1);


			// MD 9/24/08
			// Found while fixing TFS8014
			// We don't have a member for the current argument anymore. Also, reset the new member indicating whether the next popped value is a reference
			// argument. We also have to pass in the original number stack count before popping the value.
			//peekValue = this.BeforeReturnValue( peekValue, performDataTableReplacements );
			//this.nextArgumentIndex--;
			peekValue = this.BeforeReturnValue( peekValue, performDataTableReplacements, this.numberStack.Count + 1 );
			this.treatNextArgumentAsReferenceParameter = false;


			return peekValue;
		}

		/// <summary>
		/// Return value off top of the number stack.
		/// </summary>
		/// <returns><see cref="UltraCalcValue"/> that is at the top of the number stack.</returns>
		public UltraCalcValue Peek()
		{
			Debug.Assert(this.numberStack.Count > 0, "UltraCalcValue Peek: Stack underflow");

			// MD 7/16/08 - Excel formula solving
			// We may need to modify the value before returning it.
			//return (UltraCalcValue)this.numberStack.Peek();
			
			// MD 4/10/12 - TFS108678
			//UltraCalcValue peekValue = (UltraCalcValue)this.numberStack.Peek();
			UltraCalcValue peekValue = this.numberStack[this.numberStack.Count - 1];


			// MD 9/24/08
			// Found while fixing TFS8014
			// We have to pass in the original number stack count before peeking the value (which is the current count as well).
			//UltraCalcValue newPeekValue = this.BeforeReturnValue( peekValue, true );
			UltraCalcValue newPeekValue = this.BeforeReturnValue( peekValue, true, this.numberStack.Count );

			// The the peek value has been modified by BeforeReturnValue, the old peek value should 
			// be popped off the stack and the new peek value should be pushed on the stack.
			if ( Object.ReferenceEquals( newPeekValue, peekValue ) == false )
			{
				// MD 4/10/12 - TFS108678
				//this.numberStack.Pop();
				//this.numberStack.Push( newPeekValue );
				this.numberStack[this.numberStack.Count - 1] = newPeekValue;

				peekValue = newPeekValue;
			}


			return peekValue;
		}

		/// <summary>
		/// Remove all values from number stack.
		/// </summary>
		public void Clear()
		{
			this.numberStack.Clear();		
		}

		/// <summary>
		/// Clear elements off top of number stack until it contains given number of elements
		/// </summary>
		/// <param name="elements">Denotes the desired stack level</param>
		public void Reset(int elements)
		{
			// MD 4/10/12 - TFS108678
			//while (this.numberStack.Count > elements)
			//    this.numberStack.Pop();
			this.numberStack.RemoveRange(elements, this.numberStack.Count - elements);
		}

		/// <summary>
		/// Return the number of values on number stack
		/// </summary>
		/// <returns>Number of stack values</returns>
		public int Count()
		{
			return this.numberStack.Count;
		}
		#endregion //Methods

		// MD 4/6/12 - TFS101506
		#region Culture

		internal CultureInfo Culture
		{
			get
			{
				if (UltraCalcValue.UseExcelValueCompatibility)
				{

					if (this.Workbook != null)
						return this.Workbook.CultureResolved;

					return CultureInfo.CurrentCulture;
				}






			}
		}

		#endregion // Culture


		// MD 4/10/12 - TFS108678
		private UltraCalcFunctionToken currentFunctionToken;

		// MD 7/16/08 - Excel formula solving
		// MD 9/24/08
		// Found while fixing TFS8014
		// The argument index is not stored anymore, it is calculated.
		//private int nextArgumentIndex;
		private Stack<ExcelCalcNumberStackState> perviousEvaluationStates = new Stack<ExcelCalcNumberStackState>();
		private int stackCountBeforeFunctionEvaluation;
		private bool treatNextArgumentAsReferenceParameter;
		

		#region BeforeReturnValue

		// MD 9/24/08
		// Found while fixing TFS8014
		// Added another argument so we can get the original stack count before the peek or pop operation.
		//private UltraCalcValue BeforeReturnValue( UltraCalcValue value, bool performDataTableReplacements )
		private UltraCalcValue BeforeReturnValue( UltraCalcValue value, bool performDataTableReplacements, int stackCountBeforePeekOrPop )
		{
			// MD 7/21/08 - Excel formula solving
			// If the value being asked for is a reference to the row or column input cells of a data table and the current formula
			// being solved is an interior cell of the data table, the reference will have to be changed to point to the appropriate 
			// cell in the data table exterior.
			if ( performDataTableReplacements && value.IsReference )
			{
				CellCalcReference cellReference = ExcelCalcEngine.GetResolvedReference( value.ToReference() ) as CellCalcReference;

				if ( CalcUtilities.PerformDataTableReferenceReplacement( ref cellReference, this ) )
				{
					value = new UltraCalcValue( cellReference );

					// Redirected data table references are automatically dynamic
					value.IsDynamicReference = true;
				}
			}

			value.WillParameterAlwaysBeDereferenced = true;

			if ( this.CurrentFunction == null )
			{
				value.ExpectedTokenClass = this.FinalTokenClass;
			}
			else
			{
				// MD 9/24/08
				// Found while fixing TFS8014
				// The next argument index is not stored anymore. This has been changed slightly.
				//Debug.Assert( this.nextArgumentIndex >= 0, "The index of the next argument is not valid." );
				//
				//if ( this.nextArgumentIndex >= 0 )
				//{
				//    value.ExpectedTokenClass = this.CurrentFunction.GetExpectedParameterClass( this.nextArgumentIndex );
				//    value.WillParameterAlwaysBeDereferenced = this.CurrentFunction.WillParameterAlwaysBeDereferenced( this.nextArgumentIndex );
				int currentArgumentIndex = this.CurrentFunctionArgumentCount - 1 - ( this.stackCountBeforeFunctionEvaluation - stackCountBeforePeekOrPop );

				Debug.Assert( currentArgumentIndex < this.currentFunctionArgumentCount, "The index cannot be greater than the argument count." );
				Debug.Assert( currentArgumentIndex >= 0, "The index of the next argument is not valid." );

				if ( currentArgumentIndex >= 0 )
				{
					if ( this.treatNextArgumentAsReferenceParameter )
					{
						value.ExpectedTokenClass = TokenClass.Reference;
						value.WillParameterAlwaysBeDereferenced = true;
					}
					else
					{
						value.ExpectedTokenClass = this.CurrentFunction.GetExpectedParameterClass( currentArgumentIndex );
						value.WillParameterAlwaysBeDereferenced = this.CurrentFunction.WillParameterAlwaysBeDereferenced( currentArgumentIndex );
					}


					// If a dynamic reference is being pushed on the stack and we are sure it will be dereferenced, add it to
					// the dynamic references of the formula. We have to do this here because if a dynamic range was pushed on
					// the stack, then popped off and enumerated and each cell in the range were dereferenced, we would get
					// dynamic references created for each cell, not a single reference for the entire range.
					if ( this.FormulaOwner != null &&
						value.WillParameterAlwaysBeDereferenced &&
						value.IsReference &&
						value.IsDynamicReference )
					{
						// However, if the dynamic reference is a range that will be split up in an array formula, then don't 
						// add the dynamic reference. Only one cell from the range will be used, and it will be added to the
						// dyanmic reference when the value is obtained.
						// MD 8/17/08 - Excel formula solving
						// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
						//if ( ( (UltraCalcFormula)this.FormulaOwner.Formula ).IsArrayFormula == false ||
						if (((ExcelCalcFormula)this.FormulaOwner.Formula).IsArrayFormula == false ||
							// MD 9/24/08
							// Found while fixing TFS8014
							// The current arguemnt index is calculated instead of stored now.
							//this.CurrentFunction.GetExpectedParameterClass( this.nextArgumentIndex ) != TokenClass.Value )
							this.CurrentFunction.GetExpectedParameterClass( currentArgumentIndex ) != TokenClass.Value )
						{
							this.FormulaOwner.Formula.AddDynamicReference( value.ToReference() );
						}
					}
				}
			}

			// MD 4/10/12 - TFS108678
			// Moved this logic to the Push method because we will need to resolved value on the stack when doing random 
			// access into the stack as well as when popping off values.
			#region Moved

			//object resolvedValue = value.GetResolvedValue( false );

			//// MD 8/17/08 - Excel formula solving
			//// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase
			////UltraCalcFormula formula = resolvedValue as UltraCalcFormula;
			//UltraCalcFormulaBase formula = resolvedValue as UltraCalcFormulaBase;

			//if ( formula != null )
			//{
			//    // Cache the expected parameter class before evaluating the formula.
			//    TokenClass expectedTokenClass = value.ExpectedTokenClass;

			//    value = formula.EvaluateHelper( formula.BaseReference, this );

			//    // Store the expected token class on the evaluated value.
			//    value.ExpectedTokenClass = expectedTokenClass;
			//}

			#endregion // Moved

			return value;
		}

		#endregion BeforeReturnValue

		#region CurrentFunction

		private UltraCalcFunction currentFunction;






		internal UltraCalcFunction CurrentFunction
		{
			get { return this.currentFunction; }
			set { this.currentFunction = value; }
		}

		#endregion CurrentFunction

		#region CurrentFunctionArgumentCount

		private int currentFunctionArgumentCount;






		internal int CurrentFunctionArgumentCount
		{
			get { return this.currentFunctionArgumentCount; }
			set 
			{ 
				this.currentFunctionArgumentCount = value;

				// MD 9/24/08
				// Found while fixing TFS8014
				// The argument index is not stored anymore, it is calculated.
				//this.nextArgumentIndex = this.currentFunctionArgumentCount - 1;
			}
		}

		#endregion CurrentFunctionArgumentCount

		// MD 4/10/12 - TFS108678
		#region CurrentFunctionToken

		internal UltraCalcFunctionToken CurrentFunctionToken
		{
			get { return this.currentFunctionToken; }
			set { this.currentFunctionToken = value; }
		}

		#endregion // CurrentFunctionToken

		#region FinalTokenClass

		private TokenClass finalTokenClass;






		internal TokenClass FinalTokenClass
		{
			get { return this.finalTokenClass; }
		}

		#endregion FinalTokenClass

		#region FormulaOwner

		private IUltraCalcReference formulaOwner;






		internal IUltraCalcReference FormulaOwner
		{
			get { return this.formulaOwner; }
			set 
			{
				this.formulaOwner = value;

				this.finalTokenClass = CalcUtilities.GetExpectedFormulaResultTokenClass( value );
			}
		}

		#endregion FormulaOwner

		// MD 4/10/12 - TFS108678
		#region GetParameterForCurrentFunction

		internal UltraCalcValue GetParameterForCurrentFunction(int currentFunctionParamIndex)
		{
			Debug.Assert(this.CurrentFunction != null, "The current function should not be null.");

			int resolvedIndex = this.stackCountBeforeFunctionEvaluation - this.CurrentFunctionArgumentCount + currentFunctionParamIndex;
			if (resolvedIndex < 0)
			{
				Utilities.DebugFail("The ValueIsArray index is out of range.");
				return new UltraCalcValue(null);
			}

			return this.numberStack[resolvedIndex];
		}

		#endregion // GetParameterForCurrentFunction

		// MD 3/16/12 - TFS105077
		#region OwningCell

		/// <summary>
		/// Gets the <see cref="WorksheetCell"/> instance whose formula is currently being solved.
		/// </summary>
		public WorksheetCell OwningCell
		{
			get
			{
				CellCalcReference cellReference = this.FormulaOwner as CellCalcReference;
				if (cellReference == null)
					return null;

				return cellReference.Row.Cells[cellReference.ColumnIndex];
			}
		}

		#endregion // OwningCell

		// MD 9/24/08
		// Found while fixing TFS8014
		#region OnAfterFunctionEvaluation

		internal void OnAfterFunctionEvaluation()
		{
			this.treatNextArgumentAsReferenceParameter = true;

			if ( this.perviousEvaluationStates.Count == 0 )
			{
				Utilities.DebugFail( "The stack should not be empty here." );
				return;
			}

			this.perviousEvaluationStates.Pop().RestoreState();
		} 

		#endregion OnAfterFunctionEvaluation

		#region OnBeforeFunctionEvaluation

		internal void OnBeforeFunctionEvaluation()
		{
			this.perviousEvaluationStates.Push( new ExcelCalcNumberStackState( this ) );

			this.stackCountBeforeFunctionEvaluation = this.numberStack.Count;
			this.treatNextArgumentAsReferenceParameter = false;
		} 

		#endregion OnBeforeFunctionEvaluation

		#region ExcelCalcNumberStackState class

		private class ExcelCalcNumberStackState
		{
			private ExcelCalcNumberStack numberStack;

			private ExcelCalcFunction currentFunction;
			private int currentFunctionArgumentCount;
			private int stackCountBeforeFunctionEvaluation;

			public ExcelCalcNumberStackState( ExcelCalcNumberStack numberStack )
			{
				this.numberStack = numberStack;
				this.currentFunction = this.numberStack.currentFunction;
				this.currentFunctionArgumentCount = this.numberStack.currentFunctionArgumentCount;
				this.stackCountBeforeFunctionEvaluation = this.numberStack.stackCountBeforeFunctionEvaluation;
			}

			public void RestoreState()
			{
				this.numberStack.currentFunction = this.currentFunction;
				this.numberStack.currentFunctionArgumentCount = this.currentFunctionArgumentCount;
				this.numberStack.stackCountBeforeFunctionEvaluation = this.stackCountBeforeFunctionEvaluation;
			} 
		} 

		#endregion ExcelCalcNumberStackState class

		private Workbook cachedWorkbook;

		#region Workbook

		internal Workbook Workbook
		{
			get
			{
				if (this.cachedWorkbook == null)
				{
					ExcelRefBase excelRef = ExcelCalcEngine.GetResolvedReference(this.FormulaOwner) as ExcelRefBase;

					if (excelRef == null)
						return null;

					this.cachedWorkbook = excelRef.Workbook;
				}

				return this.cachedWorkbook;
			}
		}

		#endregion Workbook

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