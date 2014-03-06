using System;
using System.Collections;
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



namespace Infragistics.Calculations.Engine





{
	#region UltraCalcFormulaToken.cs

	/// <summary>
	/// Abstract base class from which all formula tokens must derive.
	/// </summary>
	internal abstract class UltraCalcFormulaToken :
        // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
        IUltraCalcFormulaToken
	{
		#region Member Variables

		// Storage for the formula token type
		private UltraCalcFormulaTokenType tokenType;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		protected UltraCalcFormulaToken(UltraCalcFormulaTokenType type)
		{
			this.tokenType = type;
		}
		#endregion //Constructor

		#region Type
		/// <summary>
		/// Return the token's type code
		/// </summary>
		/// <returns>The <b>UltraClacFormulaTokenType</b> for the this token</returns>
		public UltraCalcFormulaTokenType Type
		{
			get { return this.tokenType; }
		}
		#endregion //Type        
    }

	#endregion // UltraCalcFormulaToken.cs

	#region UltraCalcValueToken
	internal class UltraCalcValueToken : UltraCalcFormulaToken, 
        // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
        IUltraCalcValueToken
	{
		private UltraCalcValue value;

		internal UltraCalcValueToken(UltraCalcValue value) : base(UltraCalcFormulaTokenType.Value)
		{
			this.value = value;
		}

		public UltraCalcValue Value
		{
			get { return this.value; }
		}
    }
	#endregion //UltraCalcValueToken

	#region UltraCalcFunctionToken
	internal class UltraCalcFunctionToken : UltraCalcFormulaToken, 
        // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
        IUltraCalcFunctionToken
	{
		#region Member Variables

		private UltraCalcFunction	function;
		private int					argumentCount;

        // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
        private Nullable<UltraCalcOperatorFunction> ultraCalcOperatorFunction;

		#endregion //Member Variables

		#region Constructor
        // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
        // Overload for backward compatibility
        internal UltraCalcFunctionToken(UltraCalcFunction function, int argumentCount)
            : this(function, argumentCount, null)
        {
        }

        // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
		//internal UltraCalcFunctionToken(UltraCalcFunction function, int argumentCount) : base(UltraCalcFormulaTokenType.Function)
        internal UltraCalcFunctionToken(UltraCalcFunction function, int argumentCount, Nullable<UltraCalcOperatorFunction> ultraCalcOperatorFunction)
            : base(UltraCalcFormulaTokenType.Function)
		{
			this.argumentCount = argumentCount;
			this.function = function;

            // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
            this.ultraCalcOperatorFunction = ultraCalcOperatorFunction;
		}
		#endregion //Constructor

		#region Methods
		internal void Evaluate(UltraCalcNumberStack numberStack)
		{
			this.function.PerformEvaluation(numberStack, this.argumentCount);
		}
		#endregion //Methods

		#region Properties
		internal UltraCalcFunction Function
		{
			get { return this.function; }
		}

		#endregion //Properties


        #region IUltraCalcFunctionToken Members

        #region ArgumentCount
        /// <summary>
        /// Returns the number of arguments to the function.
        /// </summary>
        public int ArgumentCount
        {
            get { return this.argumentCount; }
        }
        #endregion //ArgumentCount

        #region FunctionName
        /// <summary>
        /// The name of the UltraCalcFunction represented by the token. 
        /// </summary>
        string IUltraCalcFunctionToken.FunctionName
        {
            get { return this.Function.Name; }
        }
        #endregion //FunctionName

        #region FunctionOperator
        /// <summary>
        /// Returns an UltraCalcOperatorFunction indicating the operator that the function reprsents or null of the function does not represent an operator.   
        /// </summary>
        Nullable<UltraCalcOperatorFunction> IUltraCalcFunctionToken.FunctionOperator
        {
            get { return this.ultraCalcOperatorFunction; }
        }
        #endregion //FunctionOperator

        #endregion
    }
	#endregion //UltraCalcFunctionToken

	#region UltraCalcFormulaTokenCollection.cs
	/// <summary>
	/// Provides method and properties to manage a collection of <b>UltraCalcFormulaTokens.</b>
	/// </summary>
	internal class UltraCalcFormulaTokenCollection : ICollection
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public UltraCalcFormulaTokenCollection()
		{
            // MRS 12/22/2009 - TFS24556
            //TokenArray = new List<UltraCalcFormulaToken>();
            TokenArray = new List<IUltraCalcFormulaToken>();
		}

		/// <summary>
		/// Add a token to the collection.
		/// </summary>
		/// <param name="token">Token to add to collection.</param>
        // MRS 12/22/2009 - TFS24556
		//public void Add( UltraCalcFormulaToken token )
        public void Add(IUltraCalcFormulaToken token)
		{
			TokenArray.Add( token );
		}

		/// <summary>
		/// Sets the capacity to the actual number of elements in the underlying ArrayList.
		/// </summary>
		public void TrimToSize()
		{
			TokenArray.TrimExcess();
		} 

		/// <summary>
		/// Copies the elements of the underlying ArrayList to a new array.
		/// </summary>
		/// <returns>Copy of collection.</returns>
		public object[] ToArray()
		{
			return TokenArray.ToArray();
		} 

		#region Implementation of ICollection
		/// <summary>
		/// Copies the collection to an array.
		/// </summary>
		/// <param name="array">Array that is the destination of the copy.</param>
		/// <param name="index">Index into array to begin copying collection elements.</param>
		public void CopyTo(System.Array array, int index)
		{
            // MRS 12/22/2009 - TFS24556
			//TokenArray.CopyTo((UltraCalcFormulaToken[])array,index);            
            TokenArray.CopyTo((IUltraCalcFormulaToken[])array, index);            
		}

		/// <summary>
		/// Denotes whether the collection is thread-safe.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
                return CalcManagerUtilities.IsSynchronized(TokenArray);
			}
		}

		/// <summary>
		/// Returns the number of collection elements.
		/// </summary>
		public int Count
		{
			get
			{
				return TokenArray.Count;
			}
		}

		/// <summary>
		/// Returns an object that can be used to synchronize access to the collection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
                return CalcManagerUtilities.SyncRoot(TokenArray);
			}
		}
		#endregion

		#region Implementation of IEnumerable
		/// <summary>
		/// Returns the collection enumerator
		/// </summary>
		/// <returns>Collection enumerator</returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return TokenArray.GetEnumerator();
		}
		#endregion

		/// <summary>
		/// Underlying ArrayList used to manage token collection.
		/// </summary>
        // MRS 12/22/2009 - TFS24556
        private List<IUltraCalcFormulaToken> TokenArray;
	}
	#endregion // UltraCalcFormulaTokenCollection.cs
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