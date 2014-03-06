using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;


using UltraCalcFunction = Infragistics.Calculations.Engine.CalculationFunction;
using UltraCalcNumberStack = Infragistics.Calculations.Engine.CalculationNumberStack;
using UltraCalcValue = Infragistics.Calculations.Engine.CalculationValue;
using IUltraCalcReference = Infragistics.Calculations.Engine.ICalculationReference;
using IUltraCalcFormula = Infragistics.Calculations.Engine.ICalculationFormula;
using UltraCalcErrorValue = Infragistics.Calculations.Engine.CalculationErrorValue;
using IUltraCalcReferenceCollection = Infragistics.Calculations.Engine.ICalculationReferenceCollection;
using UltraCalcErrorCode = Infragistics.Calculations.Engine.CalculationErrorCode;
using UltraCalcReferenceError = Infragistics.Calculations.Engine.CalculationReferenceError;
using UltraCalcException = Infragistics.Calculations.Engine.CalculationException;
using UltraCalcErrorException = Infragistics.Calculations.Engine.CalcErrorException;
using UltraCalcNumberException = Infragistics.Calculations.Engine.CalcNumberException;
using UltraCalcValueException = Infragistics.Calculations.Engine.CalculationValueException;


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)



namespace Infragistics.Calculations.Engine







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

    public class CalculationNumberStack 


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



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

        public CalculationNumberStack()





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



#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)


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


#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			Debug.Assert(this.numberStack.Count > 0, "UltraCalcValue Pop: Stack underflow");

			// MD 7/16/08 - Excel formula solving
			// We may need to modify the value before returning it.
			//return (UltraCalcValue)this.numberStack.Pop();
			
			// MD 4/10/12 - TFS108678
			//UltraCalcValue peekValue = (UltraCalcValue)this.numberStack.Pop();
			UltraCalcValue peekValue = this.numberStack[this.numberStack.Count - 1];
			this.numberStack.RemoveAt(this.numberStack.Count - 1);



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


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



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


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




					return CultureInfo.CurrentCulture;
				}

					else
					{
						return CultureInfo.InvariantCulture;
					}

			}
		}

		#endregion // Culture



#region Infragistics Source Cleanup (Region)





















































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

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