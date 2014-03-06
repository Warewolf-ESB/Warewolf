#region Using statements

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Security;






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







	using Infragistics.Shared;


#endregion //Using statements


namespace Infragistics.Calculations.Engine







{
	/// <summary>
	/// Class used to manage the stock and user defined <see cref="UltraCalcFunction"/> instances
	/// </summary>

    internal





            class UltraCalcFunctionFactory : IEnumerable
	{
		#region Member Variables

        private Dictionary<string, UltraCalcFunction> functions;
		private UltraCalcFunction[]	operators;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UltraCalcFunctionFactory"/>
		/// </summary>
		public UltraCalcFunctionFactory()
		{
			#region Operators

			this.operators = new UltraCalcFunction[15];
			this.operators[(int)UltraCalcOperatorFunction.Add] = new UltraCalcFunctionPlus();
			this.operators[(int)UltraCalcOperatorFunction.Subtract] = new UltraCalcFunctionMinus();
			this.operators[(int)UltraCalcOperatorFunction.Multiply] = new UltraCalcFunctionMultiply();
			this.operators[(int)UltraCalcOperatorFunction.Divide] = new UltraCalcFunctionDivide();
			this.operators[(int)UltraCalcOperatorFunction.Equal] = new UltraCalcFunctionEqual();
			this.operators[(int)UltraCalcOperatorFunction.NotEqual] = new UltraCalcFunctionNE();
			this.operators[(int)UltraCalcOperatorFunction.GreaterThanOrEqual] = new UltraCalcFunctionGE();
			this.operators[(int)UltraCalcOperatorFunction.GreaterThan] = new UltraCalcFunctionGT();
			this.operators[(int)UltraCalcOperatorFunction.LessThanOrEqual] = new UltraCalcFunctionLE();
			this.operators[(int)UltraCalcOperatorFunction.LessThan] = new UltraCalcFunctionLT();
			this.operators[(int)UltraCalcOperatorFunction.Concatenate] = new UltraCalcFunctionConcat();
			this.operators[(int)UltraCalcOperatorFunction.Exponent] = new UltraCalcFunctionExpon();
			this.operators[(int)UltraCalcOperatorFunction.Percent] = new UltraCalcFunctionPercent();
			this.operators[(int)UltraCalcOperatorFunction.UnaryMinus] = new UltraCalcFunctionUnaryMinus();
			this.operators[(int)UltraCalcOperatorFunction.UnaryPlus] = new UltraCalcFunctionUnaryPlus();

			this.ValidateOperatorList();

			#endregion //Operators

			#region Functions

			// MD 4/6/12 - TFS101506
            //this.functions = new Dictionary<string, UltraCalcFunction>();
			this.functions = new Dictionary<string, UltraCalcFunction>(StringComparer.InvariantCultureIgnoreCase);

			// add the base functions
			this.Add( new UltraCalcFunctionSum() );
			this.Add( new UltraCalcFunctionAverage() );
			this.Add( new UltraCalcFunctionIf() );
			this.Add( new UltraCalcFunctionAbs() );
			this.Add( new UltraCalcFunctionMod() );
			this.Add( new UltraCalcFunctionQuotient() );
			this.Add( new UltraCalcFunctionProduct() );
			this.Add( new UltraCalcFunctionPower() );
			this.Add( new UltraCalcFunctionInt() );
			this.Add( new UltraCalcFunctionCount() );
			this.Add( new UltraCalcFunctionTrue() );
			this.Add( new UltraCalcFunctionFalse() );
			this.Add( new UltraCalcFunctionAnd() );
			this.Add( new UltraCalcFunctionOr() );
			this.Add( new UltraCalcFunctionNot() );
			this.Add( new UltraCalcFunctionMax() );
			this.Add( new UltraCalcFunctionMin() );
			this.Add( new UltraCalcFunctionMedian() );
			this.Add( new UltraCalcFunctionVar() );
			this.Add( new UltraCalcFunctionStdev() );
			this.Add( new UltraCalcFunctionRound() );
			this.Add( new UltraCalcFunctionExp() );
			this.Add( new UltraCalcFunctionPi() );
			this.Add( new UltraCalcFunctionSqrt() );
			this.Add( new UltraCalcFunctionCos() );
			this.Add( new UltraCalcFunctionCosh() );
			this.Add( new UltraCalcFunctionACos() );
			this.Add( new UltraCalcFunctionSin() );
			this.Add( new UltraCalcFunctionSinh() );
			this.Add( new UltraCalcFunctionASin() );
			this.Add( new UltraCalcFunctionTan() );
			this.Add( new UltraCalcFunctionTanh() );
			this.Add( new UltraCalcFunctionATan() );
			this.Add( new UltraCalcFunctionATan2() );
			this.Add( new UltraCalcFunctionFloor() );
			this.Add( new UltraCalcFunctionCeiling() );
			this.Add( new UltraCalcFunctionRand() );
			this.Add( new UltraCalcFunctionLn() );
			this.Add( new UltraCalcFunctionLog() );
			this.Add( new UltraCalcFunctionLog10() );
			this.Add( new UltraCalcFunctionNPV() );
			this.Add( new UltraCalcFunctionFV() );
			this.Add( new UltraCalcFunctionPV() );
			this.Add( new UltraCalcFunctionPmt() );
			this.Add( new UltraCalcFunctionNPer() );
			this.Add( new UltraCalcFunctionTrunc() );
			this.Add( new UltraCalcFunctionEven() );
			this.Add( new UltraCalcFunctionOdd() );
			this.Add( new UltraCalcFunctionPPmt() );
			this.Add( new UltraCalcFunctionIPmt() );
			this.Add( new UltraCalcFunctionConcatenate() );
			this.Add( new UltraCalcFunctionLower() );
			this.Add( new UltraCalcFunctionUpper() );
			this.Add( new UltraCalcFunctionLeft() );
			this.Add( new UltraCalcFunctionRight() );
			this.Add( new UltraCalcFunctionMid() );
			this.Add( new UltraCalcFunctionLen() );
			this.Add( new UltraCalcFunctionTrim() );
			this.Add( new UltraCalcFunctionValue() );
			this.Add( new UltraCalcFunctionFind() );
			this.Add( new UltraCalcFunctionReplace() );
			this.Add( new UltraCalcFunctionSYD() );
			this.Add( new UltraCalcFunctionSLN() );
			this.Add( new UltraCalcFunctionDB() );
			this.Add( new UltraCalcFunctionDDB() );
			this.Add( new UltraCalcFunctionIntRate() );
			this.Add( new UltraCalcFunctionDate() );
			this.Add( new UltraCalcFunctionDateValue() );
			this.Add( new UltraCalcFunctionDays360() );
			this.Add( new UltraCalcFunctionDay() );
			this.Add( new UltraCalcFunctionMonth() );
			this.Add( new UltraCalcFunctionYear() );
			this.Add( new UltraCalcFunctionHour() );
			this.Add( new UltraCalcFunctionMinute() );
			this.Add( new UltraCalcFunctionSecond() );
			this.Add( new UltraCalcFunctionTime() );
			this.Add( new UltraCalcFunctionNow() );
			this.Add( new UltraCalcFunctionTimeValue() );
			this.Add( new UltraCalcFunctionNa() );
			this.Add( new UltraCalcFunctionType() );
			this.Add( new UltraCalcFunctionErrorType() );
			this.Add( new UltraCalcFunctionIsBlank() );
			this.Add( new UltraCalcFunctionIsErr() );
			this.Add( new UltraCalcFunctionIsError() );
			this.Add( new UltraCalcFunctionIsLogical() );
			this.Add( new UltraCalcFunctionIsNa() );
			this.Add( new UltraCalcFunctionIsNonText() );
			this.Add( new UltraCalcFunctionIsNumber() );
			this.Add( new UltraCalcFunctionIsRef() );
			this.Add( new UltraCalcFunctionIsText() );
			this.Add( new UltraCalcFunctionIsOdd() );
			this.Add( new UltraCalcFunctionIsEven() );
			this.Add( new UltraCalcFunctionIRR() );
			this.Add( new UltraCalcFunctionRate() );
			this.Add( new UltraCalcFunctionNull() );
			this.Add( new UltraCalcFunctionIsNull() );
			this.Add( new UltraCalcFunctionDBNull() );
			this.Add( new UltraCalcFunctionIsDBNull() );
			this.Add( new UltraCalcFunctionDateAdd() );
			this.Add( new UltraCalcFunctionDateDiff() );
			// JAS 12/22/04 BR01396 *** Start ***
			this.Add( new UltraCalcFunctionChar() );
			this.Add( new UltraCalcFunctionCode() );
			this.Add( new UltraCalcFunctionFixed() );
			this.Add( new UltraCalcFunctionToday() );
			this.Add( new UltraCalcFunctionASinh() );
			this.Add( new UltraCalcFunctionATanh() );
			this.Add( new UltraCalcFunctionACosh() );
			// JAS 12/22/04 BR01396 *** End ***  

            // MRS NAS 8.3 *** Start ***
            this.Add(new UltraCalcFunctionEDate());
            this.Add(new UltraCalcFunctionEOMonth());
            this.Add(new UltraCalcFunctionWeekDay());
            this.Add(new UltraCalcFunctionNetWorkDays());
            this.Add(new UltraCalcFunctionWeekNum());
            this.Add(new UltraCalcFunctionWorkDay());

            // MRS 6/23/2008 - I just can't get this to work for the 0 or 4 case, so I'm leaving it out for now. 
            //this.Add(new UltraCalcFunctionYearFrac());

            this.Add(new UltraCalcFunctionDec2Bin());
            this.Add(new UltraCalcFunctionDec2Hex());
            this.Add(new UltraCalcFunctionDec2Oct());

            this.Add(new UltraCalcFunctionBin2Dec());
            this.Add(new UltraCalcFunctionHex2Dec());
            this.Add(new UltraCalcFunctionOct2Dec());

            this.Add(new UltraCalcFunctionBin2Oct());
            this.Add(new UltraCalcFunctionBin2Hex());

            this.Add(new UltraCalcFunctionOct2Bin());
            this.Add(new UltraCalcFunctionOct2Hex());

            this.Add(new UltraCalcFunctionHex2Bin());
            this.Add(new UltraCalcFunctionHex2Oct());

            this.Add(new UltraCalcFunctionConvert());

            this.Add(new UltraCalcFunctionDelta());
            this.Add(new UltraCalcFunctionGeStep());

            this.Add(new UltraCalcFunctionComplex());
            this.Add(new UltraCalcFunctionImAbs());
            this.Add(new UltraCalcFunctionImaginary());
            this.Add(new UltraCalcFunctionImReal());
            this.Add(new UltraCalcFunctionImArgument());
            this.Add(new UltraCalcFunctionImConjugate());
            this.Add(new UltraCalcFunctionImCos());
            this.Add(new UltraCalcFunctionImDiv());
            this.Add(new UltraCalcFunctionImProduct());
            this.Add(new UltraCalcFunctionImSum());
            this.Add(new UltraCalcFunctionImSub());
            this.Add(new UltraCalcFunctionImExp());
            this.Add(new UltraCalcFunctionImLn());
            this.Add(new UltraCalcFunctionImLog10());
            this.Add(new UltraCalcFunctionImLog2());
            this.Add(new UltraCalcFunctionImSin());
            this.Add(new UltraCalcFunctionImSqrt());
            this.Add(new UltraCalcFunctionImPower());

            this.Add(new UltraCalcFunctionDollarFr());
            this.Add(new UltraCalcFunctionDollarDe());

            this.Add(new UltraCalcFunctionInfo());
            this.Add(new UltraCalcFunctionN());

            this.Add(new UltraCalcFunctionFact());
            this.Add(new UltraCalcFunctionFactDouble());

            this.Add(new UltraCalcFunctionCombin());

            this.Add(new UltraCalcFunctionDegrees());
            this.Add(new UltraCalcFunctionRadians());

            this.Add(new UltraCalcFunctionGcd());
            this.Add(new UltraCalcFunctionLcm());
            this.Add(new UltraCalcFunctionMultinomial());

            this.Add(new UltraCalcFunctionMRound());
            this.Add(new UltraCalcFunctionRandBetween());

            this.Add(new UltraCalcFunctionRoman());

            this.Add(new UltraCalcFunctionRoundDown());
            this.Add(new UltraCalcFunctionRoundUp());

            this.Add(new UltraCalcFunctionSeriesSum());

            this.Add(new UltraCalcFunctionSign());
            this.Add(new UltraCalcFunctionSqrtPi());

            // MRS NAS 8.3 *** End ***

			// MD 7/14/08 - Excel formula solving
			this.Add( new UltraCalcFunctionChoose() );

			// MD 3/30/10 - TFS30100
			this.Add(new UltraCalcFunctionRept());

			// MD 2/14/11 - TFS66313
			this.Add(new UltraCalcFunctionSubtotal());

			// MD 5/24/11 - TFS75560
			this.Add(new UltraCalcFunctionText());

			// MD 8/29/11 - TFS85072
			this.Add(new UltraCalcFunctionIfError());

			// MD 2/24/12 - 12.1 - Table Support
			this.Add(new UltraCalcFunctionCountA());

			// MD 3/2/12 - TFS103729
			this.Add(new UltraCalcFunctionSearch());
			this.Add(new UltraCalcFunctionSearchB());

			// MD 5/10/12 - TFS106835
			this.Add(new UltraCalcFunctionAveDev());



#region Infragistics Source Cleanup (Region)





















































#endregion // Infragistics Source Cleanup (Region)


			#endregion //Functions
		}
		#endregion //Constructor

		#region Properties

		#region Indexer
		/// <summary>
		/// Returns the function with the specified name
		/// </summary>
		public UltraCalcFunction this[string functionName]
		{
			get
			{
				if (functionName == null)
					return null;

				// MD 4/6/12 - TFS101506
				//functionName = functionName.ToLower(System.Globalization.CultureInfo.InvariantCulture);
				//return this.functions.ContainsKey(functionName) ? this.functions[functionName] : null;
				UltraCalcFunction function;
				this.functions.TryGetValue(functionName, out function);
				return function;
			}
		}
		#endregion //Indexer

		#endregion //Properties

		#region Methods

		#region Add
		/// <summary>
		/// Adds the specified function to the function list.
		/// </summary>
		/// <param name="function">Function to add</param>
		/// <returns>A boolean indicating if the function was added.</returns>
		public bool Add(UltraCalcFunction function)
		{
			if (function == null)
				throw new ArgumentNullException();

			string functionName = this.GetFunctionName(function);

			if (functionName.Length == 0)
				return false;

			this.functions[functionName] = function;

			return true;
		}
		#endregion //Add

		#region AddLibrary


#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
		/// Adds a library of user defined functions contained in the given assembly.
		/// </summary>
		/// <param name="assembly">Loaded assembly containing a library of user defined functions</param>
		/// <remarks>
		/// <p class="body">
		/// Once the assembly is loaded, any type that derives from <b>UltraCalcFunction</b> will be registered with the function factory
		/// </p>
		/// </remarks>
        /// <returns>True if the function library was successfully loaded and its functions registered, else false</returns>
		public bool AddLibrary(System.Reflection.Assembly assembly)
		{
			if (assembly == null) 
				return false;

			foreach (Type functionType in assembly.GetTypes()) 
			{
				if (functionType.IsSubclassOf(typeof(UltraCalcFunction)) &&
					!functionType.IsAbstract) 
				{
					object instance = assembly.CreateInstance(functionType.FullName);
					
					UltraCalcFunction function = instance as UltraCalcFunction;

					if (function != null) 
						this.Add(function);
				}
			}

			return true;
		}
		#endregion //AddLibrary

		#region GetFunctionName
		private string GetFunctionName(UltraCalcFunction function)
		{
			// MD 9/12/08 - 8.3 Performance
			// Since the Name is virtual, its better to cache it than get it twice.
			//if (function.Name == null)
			string functionName = function.Name;
			if ( functionName == null )
				return string.Empty;

			// MD 9/12/08 - 8.3 Performance
			// Use the cached name from above
			//return function.Name.ToLower(System.Globalization.CultureInfo.InvariantCulture);
			return functionName.ToLower( System.Globalization.CultureInfo.InvariantCulture );
		}
		#endregion //GetFunctionName

		#region ValidateOperatorList
		[Conditional("DEBUG")]
		private void ValidateOperatorList()
		{
			Debug.Assert(this.operators.Length == CalcManagerUtilities.EnumGetValues(typeof(UltraCalcOperatorFunction)).Length, "The operator list does not coincide with the number of entries in the 'UltraCalcOperatorFunction' enumeration.");

			for(int i = 0; i < this.operators.Length; i++)
				Debug.Assert(this.operators[i] != null, string.Format("The function entry for index #{0} '{1}' was not created!", i, Enum.ToObject(typeof(UltraCalcOperatorFunction), i)));
		}
		#endregion //ValidateOperatorList

		#region GetOperator
		/// <summary>
		/// Returns the <see cref="UltraCalcFunction"/> for the specified operator enumeration
		/// </summary>
		/// <param name="operatorType">Operator whose function should be returned.</param>
		/// <returns>The <see cref="UltraCalcFunction"/> associated with the specified enumeration.</returns>
		public UltraCalcFunction GetOperator(UltraCalcOperatorFunction operatorType)
		{
			int index = (int)operatorType;

// MD 10/22/10 - TFS36696
// Wrapped this check in an #if because we are only calling this method internally in the excel code, so we know we are passing in a valid value.

			// make sure its a valid enum value
			if (!Enum.IsDefined(typeof(UltraCalcOperatorFunction), index))
				throw new InvalidEnumArgumentException("operatorType", index, typeof(UltraCalcOperatorFunction));


			return this.operators[ index ];
		}
		#endregion //GetOperator


		#region SetOperator
		/// <summary>
		/// Replaces the <see cref="UltraCalcFunction"/> for the specified operator enumeration with the specified function
		/// </summary>
		/// <param name="operatorType">Operator whose function should be returned.</param>
		/// <param name="function">Function used when compiling that operator.</param>
		public void SetOperator( UltraCalcOperatorFunction operatorType, UltraCalcFunction function )
		{
			if ( function == null )
				throw new ArgumentNullException();

			int index = (int)operatorType;

			// make sure its a valid enum value
			if ( !Enum.IsDefined( typeof( UltraCalcOperatorFunction ), index ) )
				throw new InvalidEnumArgumentException( "operatorType", index, typeof( UltraCalcOperatorFunction ) );

			int arity = GetOperatorArity( operatorType );

			// verify that it takes the same number of args
			if ( function.MinArgs > arity || function.MaxArgs < arity )
				throw new InvalidOperationException( SR.GetString( "Error_InvalidOperatorArgCount", operatorType, function.MinArgs ) );

			// otherwise replace the operator function
			this.operators[ index ] = function;
		}
		#endregion //SetOperator 

		#region GetOperatorArity
		internal static int GetOperatorArity(UltraCalcOperatorFunction operatorType)
		{
			switch(operatorType)
			{
				case UltraCalcOperatorFunction.Percent:
				case UltraCalcOperatorFunction.UnaryMinus:
				case UltraCalcOperatorFunction.UnaryPlus:
					return 1;
				default:
					return 2;
			}
		}
		#endregion //GetOperatorArity


		#endregion //Methods

		#region IEnumerable interface
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new UltraCalcFunctionFactoryEnumerator(this.functions.GetEnumerator());
		}
		#endregion //IEnumerable interface

		#region UltraCalcFunctionFactoryEnumerator
		private class UltraCalcFunctionFactoryEnumerator : IEnumerator
		{
			#region Member Variables

			private IDictionaryEnumerator functionEnumerator;

			#endregion //Member Variables

			#region Constructor
			internal UltraCalcFunctionFactoryEnumerator(IDictionaryEnumerator functionEnumerator)
			{
				this.functionEnumerator = functionEnumerator;
			}
			#endregion //Constructor

			#region IEnumerator implementation
			void IEnumerator.Reset()
			{
				this.functionEnumerator.Reset();
			}

			bool IEnumerator.MoveNext()
			{
				return this.functionEnumerator.MoveNext();
			}

			object IEnumerator.Current
			{
				get
				{
					return this.functionEnumerator.Value;
				}
			}
			#endregion //IEnumerator implementation
		}
		#endregion //UltraCalcFunctionFactoryEnumerator
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