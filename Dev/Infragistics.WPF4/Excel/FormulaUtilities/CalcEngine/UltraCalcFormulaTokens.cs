using System;
using System.Collections;
using System.Collections.Generic;

namespace Infragistics.Documents.Excel.CalcEngine
{
	#region UltraCalcFormulaToken.cs

	internal abstract class UltraCalcFormulaToken
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
	internal class UltraCalcValueToken : UltraCalcFormulaToken
	{
		private ExcelCalcValue value;

		internal UltraCalcValueToken(ExcelCalcValue value)
			: base(UltraCalcFormulaTokenType.Value)
		{
			this.value = value;
		}

		public ExcelCalcValue Value
		{
			get { return this.value; }
		}
	}
	#endregion //UltraCalcValueToken

	#region UltraCalcFunctionToken
	internal class UltraCalcFunctionToken : UltraCalcFormulaToken
	{
		#region Member Variables

		private ExcelCalcFunction function;
		private int argumentCount;

		private Nullable<UltraCalcOperatorFunction> ultraCalcOperatorFunction;
		private UltraCalcFunctionToken parentFunctionToken;
		private int argumentIndexInParentFunction;

		#endregion //Member Variables

		#region Constructor
		// MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
		// Overload for backward compatibility
		internal UltraCalcFunctionToken(ExcelCalcFunction function, int argumentCount)
			: this(function, argumentCount, null)
		{
		}

		// MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
		//internal UltraCalcFunctionToken(UltraCalcFunction function, int argumentCount) : base(UltraCalcFormulaTokenType.Function)
		internal UltraCalcFunctionToken(ExcelCalcFunction function, int argumentCount, Nullable<UltraCalcOperatorFunction> ultraCalcOperatorFunction)
			: base(UltraCalcFormulaTokenType.Function)
		{
			this.argumentCount = argumentCount;
			this.function = function;

			// MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
			this.ultraCalcOperatorFunction = ultraCalcOperatorFunction;
		}
		#endregion //Constructor

		#region Methods
		internal void Evaluate(ExcelCalcNumberStack numberStack)
		{
			this.function.PerformEvaluation(numberStack, this.argumentCount);
		}
		#endregion //Methods

		#region Properties
		internal ExcelCalcFunction Function
		{
			get { return this.function; }
		}

		// MD 4/10/12 - TFS108678
		internal int ArgumentIndexInParentFunction
		{
			get { return this.argumentIndexInParentFunction; }
			set { this.argumentIndexInParentFunction = value; }
		}

		// MD 4/10/12 - TFS108678
		internal UltraCalcFunctionToken ParentFunctionToken
		{
			get { return this.parentFunctionToken; }
			set { this.parentFunctionToken = value; }
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
		public string FunctionName
		{
			get { return this.Function.Name; }
		}
		#endregion //FunctionName

		#region FunctionOperator
		/// <summary>
		/// Returns an UltraCalcOperatorFunction indicating the operator that the function reprsents or null of the function does not represent an operator.   
		/// </summary>
		public Nullable<UltraCalcOperatorFunction> FunctionOperator
		{
			get { return this.ultraCalcOperatorFunction; }
		}
		#endregion //FunctionOperator

		#endregion
	}
	#endregion //UltraCalcFunctionToken
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