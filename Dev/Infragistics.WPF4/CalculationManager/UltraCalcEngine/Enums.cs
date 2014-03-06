using System;


namespace Infragistics.Calculations.Engine





{
    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
    // Moved this enum into CalcEngineInterfaces in Shared. 
    // Also changed the names of the enum values to remove the "e" from the beginning. 
	#region UltraCalcFormulaTokenType
	/// <summary>
	/// Identifies formula token types in the <b>UltraCalcFormulaToken</b> class
	/// </summary>
    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
    //internal enum UltraCalcFormulaTokenType
    //{
    //    eValue, 
    //    eFunction,
    //    eNull
    //}
	#endregion //UltraCalcFormulaTokenType

	#region ReferenceActionCode Enumeration
	internal enum ReferenceActionCode
	{
		NA,
		Create,
		Remove,
		Insert,
		Delete,
		Resync,
		Sort,
		Visible,
		AddFormula,
		DeleteFormula
	}
	#endregion // ReferenceActionCode Enumeration

    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
    // Moved this enum to Shared
	#region UltraCalcOperatorFunction
//    /// <summary>
//    /// Enumeration of operator functions.
//    /// </summary>
//#if EXCEL
//    internal
//#else
//    public
//#endif 
//        enum UltraCalcOperatorFunction
//    {
//        /// <summary>
//        /// Operator used to add two values ("+")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionPlus"/>
//        Add = 0,

//        /// <summary>
//        /// Operator used to subtract two values ("-")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionMinus"/>
//        Subtract = 1,

//        /// <summary>
//        /// Operator used to multiply two values ("*")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionMultiply"/>
//        Multiply = 2,

//        /// <summary>
//        /// Operator used to divide two values ("/")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionDivide"/>
//        Divide = 3,

//        /// <summary>
//        /// Operator used to compare two objects for equality ("=")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionEqual"/>
//        Equal = 4,

//        /// <summary>
//        /// Operator used to compare if two values are different ("&gt;&lt;" or "!=")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionNE"/>
//        NotEqual = 5,

//        /// <summary>
//        /// Operator used to determine if one value is greater than or equal to a second value. ("&gt;=")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionGE"/>
//        GreaterThanOrEqual = 6,

//        /// <summary>
//        /// Operator used to determine if one value is greater than a second value. ("&gt;")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionGT"/>
//        GreaterThan = 7,

//        /// <summary>
//        /// Operator used to determine if one value is less than or equal to a second value. ("&lt;=")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionLE"/>
//        LessThanOrEqual = 8,

//        /// <summary>
//        /// Operator used to determine if one value is less than a second value. ("&lt;")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionLT"/>
//        LessThan = 9,

//        /// <summary>
//        /// Operator used to concatenate 2 strings ("&amp;")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionConcat"/>
//        Concatenate = 10,

//        /// <summary>
//        /// Operator used to raise a value to a specified power ("^")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionExpon"/>
//        Exponent = 11,

//        /// <summary>
//        /// Operator used to convert a value to a percentage ("%")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly one operand/argument.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionPercent"/>
//        Percent = 12,

//        /// <summary>
//        /// Negative unary operator ("-")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly one operand/argument.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionUnaryMinus"/>
//        UnaryMinus = 13,

//        /// <summary>
//        /// Positive unary operator ("+")
//        /// </summary>
//        /// <remarks>
//        /// <p class="note"><b>Note:</b> This operator must take exactly one operand/argument.</p>
//        /// </remarks>
//        /// <seealso cref="UltraCalcFunctionUnaryPlus"/>
//        UnaryPlus = 14,
//    }
	#endregion //UltraCalcOperatorFunction
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