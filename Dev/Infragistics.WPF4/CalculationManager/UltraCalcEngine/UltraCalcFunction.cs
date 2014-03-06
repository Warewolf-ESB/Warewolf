using System;
using System.Globalization; 
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Security;
using System.Collections.ObjectModel;










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
	// MD 9/19/08 - TFS7178
	#region OneBasedArgumentNumberingAttribute


	/// <summary>
	/// For Infragistics Internal use only.
	/// </summary>
	[EditorBrowsable( EditorBrowsableState.Never )]
	public class OneBasedArgumentNumberingAttribute : Attribute
	{
		private int argumentIndex;

		/// <summary>
		/// For Infragistics Internal use only.
		/// </summary>
		public OneBasedArgumentNumberingAttribute( int argumentIndex )
		{
			this.argumentIndex = argumentIndex;
		}

		/// <summary>
		/// For Infragistics Internal use only.
		/// </summary>
		public int ArgumentIndex
		{
			get { return this.argumentIndex; }
		}
	} 

		
	#endregion OneBasedArgumentNumberingAttribute

	#region UltraCalcFunction
	/// <summary>
	/// Base class for formula functions.
	/// </summary>

	public abstract class CalculationFunction


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UltraCalcFunction"/>
		/// </summary>

		protected CalculationFunction( )





		{
		}
		#endregion //Constructor

		#region Properties

		#region IsAlwaysDirty
		/// <summary>
		/// Indicates whether the results of the function is always dirty.
		/// </summary>


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

		public virtual bool IsAlwaysDirty
		{
			get { return false; }
		} 

		#endregion //IsAlwaysDirty

		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public abstract string Name
		{ get; }

		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public abstract int MinArgs
		{ get; }

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public abstract int MaxArgs
		{ get; }


		/// <summary>
		/// Summary description for the function displayed by the formula builder tool
		/// </summary>
		public abstract string Description
		{
			get;
		}

		/// <summary>
		/// Category description for the function displayed by the formula builder tool
		/// </summary>
		public abstract string Category
		{
			get;
		}

		/// <summary>
		/// Array list of argument names
		/// </summary>
		public abstract string [] ArgList
		{
			get;
		}

		/// <summary>
		/// Array list of argument descriptors
		/// </summary>
		public abstract string[] ArgDescriptors
		{
			get;
		} 

		// MRS 11/29/04 
		// Added HelpURL
		#region HelpURL
		/// <summary>
		/// A URL to a help topic. 
		/// </summary>
		/// <remarks> 
		/// <p class="body">This property is used by the FunctionBuilder at design-time to provide a link to a help topic describing the function.</p>
		/// <p class="body">The default implementation returns an empty string. In this case, the Designer will not show the Help link."</p>
		/// </remarks>
		// SSP 10/20/11 TFS93173
		// Made this internal until we implement something in the UI for this.
		// 
		

		internal virtual string HelpURL



		{
			get { return string.Empty; }
		}
		#endregion HelpURL




#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


		#endregion //Properties

		#region Methods

		#region PerformEvaluation
		/// <summary>
		/// Evaluates the function against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments.</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>        
		public void PerformEvaluation(UltraCalcNumberStack numberStack, int argumentCount)
		{
			Debug.Assert( this.MinArgs <= argumentCount && argumentCount <= this.MaxArgs, "The function was given an incorrect number of arguments." );

			int stackLevel = numberStack.Count();

			Debug.Assert(stackLevel >= argumentCount, string.Format("Insufficient function arguments for {0}", this.GetType().Name));

			UltraCalcValue result;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			try
			{


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				{
					result = this.Evaluate(numberStack, argumentCount);
				}
			}
			catch(Exception e)
			{
				result = new UltraCalcValue(e);
			}
			finally
			{
				// validate the state of the number stack
				numberStack.Reset(stackLevel-argumentCount);



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			}

			Debug.Assert(numberStack.Count() == (stackLevel - argumentCount), string.Format("Potential numberstack evaluation after evaluating '{0}'", this.GetType().Name));

			if (result == null)
				result = new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.NA, SR.GetString("Error_NullFunctionResult", this.GetType().Name)) );



#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


			// push the result onto the number stack
			numberStack.Push(result);

		}
		#endregion //PerformEvaluation

		#region Evaluate
		/// <summary>
		/// Evaluates the function against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments.</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected abstract UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount);
		#endregion //Evaluate

		#region PopArrayList


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        internal static bool PopArrayList(UltraCalcNumberStack numberStack, int argumentCount, List<double> valueArray, bool ignoreEmptyValues, bool checkForExceptions)
		{
			int stackLevel = numberStack.Count();

			Debug.Assert(stackLevel >= argumentCount, "Insufficient function arguments");

			UltraCalcErrorValue errorValue = null;

			try 
			{
				double d;
								
				for (int arg_count=0; arg_count < argumentCount; arg_count++) 
				{
					UltraCalcValue valueArg = numberStack.Pop();

					if (valueArg.IsReference && valueArg.ToReference().IsEnumerable)
					{
						IUltraCalcReferenceCollection referenceCollection = valueArg.ToReference().References;
						foreach (IUltraCalcReference cellReference in referenceCollection) 
						{
							UltraCalcValue val = cellReference.Value;

							if (ignoreEmptyValues && val.IsNull)
								continue;

							if ( checkForExceptions && val.IsError )
							{
								// exit and push an error onto the number stack 
								errorValue = val.ToErrorValue();
								return false;
							}

							if (!val.ToDouble(out d))
							{
								errorValue = new UltraCalcErrorValue(UltraCalcErrorCode.Num);
								return false;
							}

							valueArray.Add( d );
						}
					} 
					else
					{
						if (valueArg.IsReference)
							valueArg = valueArg.ToReference().Value;

						if (ignoreEmptyValues && valueArg.IsNull)
							continue;

						if ( checkForExceptions && valueArg.IsError )
						{
							// exit and push an error onto the number stack 
							errorValue = valueArg.ToErrorValue();
							return false;
						}



#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)


						if (!valueArg.ToDouble(out d))
						{
							errorValue = new UltraCalcErrorValue(UltraCalcErrorCode.Num);
							return false;
						}

						valueArray.Add( d );
					} 
				}

				return true;
			} 
			finally
			{
				// reset the number stack
				numberStack.Reset(stackLevel-argumentCount);
				
				// push the error value onto the stack
				if (errorValue != null)
					numberStack.Push( new UltraCalcValue(errorValue) );
			}
		}
		#endregion //PopArrayList

		#region GetArguments

		/// <summary>
		/// A helper method for extracting the <see cref="UltraCalcValue"/> instances from the stack.
		/// </summary>
		/// <param name="numberStack">Number stack whose values should be popped.</param>
		/// <param name="argumentCount">Number of items to pop/evaluate from the number stack</param>
		/// <param name="skipEmptyValues">True to ignore values whose IsNull returns true; otherwise false to include empty items in the list.</param>
		/// <returns>An array of <see cref="UltraCalcValue"/> instances removed from the number stack.</returns>
		protected UltraCalcValue[] GetArguments(UltraCalcNumberStack numberStack, int argumentCount, bool skipEmptyValues)
		{


#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

            List<UltraCalcValue> list = new List<UltraCalcValue>();

			for(int i = 0; i < argumentCount; i++)
			{
				this.GetArguments(list, numberStack.Pop(), skipEmptyValues




					);
			}

			return list.ToArray();
		}

		// SSP 8/9/12 TFS118166
		// GetArguments methods apparently return arguments in reverse order or even worse in an undefined order
		// if some values are enumerbale references that are expanded into their contituent values. So added a
		// method to get arguments in order, from left-to-right.
		// 
		/// <summary>
		/// Gets the arguments in order, from left-to-right as they are passed into the methods. It takes
		/// into account any arguments that are enumerable references, in which case the contituent values
		/// of such enumerable references are returned in the correct order as well.
		/// </summary>
		internal void GetArgumentsInOrder( List<UltraCalcValue> list, UltraCalcNumberStack numberStack, int argumentCount, bool skipEmptyValues )
		{
			List<UltraCalcValue> args = new List<UltraCalcValue>( argumentCount );
			for ( int i = 0; i < argumentCount; i++ )
				args.Add( numberStack.Pop( ) );

			args.Reverse( );

			for ( int i = 0; i < argumentCount; i++ )
			{
				this.GetArguments( list, args[i], skipEmptyValues



				);
			}
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		// MD 2/14/11 - TFS66313
		// Made internl so this could be used in other places.
		//private void GetArguments(List<UltraCalcValue> list, UltraCalcValue val, bool skipEmptyValues)
		internal void GetArguments(List<UltraCalcValue> list, UltraCalcValue val, bool skipEmptyValues





			)
		{
			// If the value is a reference (for example a cell or a column).
			//
			if ( val.IsReference )
			{
				IUltraCalcReference reference = val.ToReference();

				// If the reference is a column then loop through it's cells.
				//
				if ( reference.IsEnumerable )
				{
					foreach ( IUltraCalcReference r in reference.References )
					{







						this.GetArguments(list, r.Value, skipEmptyValues




							);
					}
				}
				else
				{







					this.GetArguments(list, reference.Value, skipEmptyValues




						);
				}
			}


#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

			else
			{
				// Get the underlying double value from the UltraCalcValue. Also if skipEmptyValues 
				// is true then skip the empty values. NOTE: Calling ToDouble on something that's 
				// not a double will raise an exception which may be desirable depending on how
				// you want the function to work. Throwing an exception here will lead us to catch
				// it in Evaluate below and return an error code that will be displayed in the cell.
				//
				if ( ! skipEmptyValues || ! val.IsNull )					
					list.Add( val );
			}	
		}

		// MD 7/16/08 - Excel formula solving


#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)


		#endregion // GetArguments

		#region IsDateTime
		
		internal static bool IsDateTime(UltraCalcValue value)
		{
			// MD 7/30/08 - Excel formula solving
			// Refactored this to it can handle null values as well as not call ToReference() twice, 
			// which is now a more expensive operation.
			//if (value.IsReference)
			//	return !value.ToReference().IsEnumerable && IsDateTime(value.ToReference().Value);
			if ( value == null )
				return false;

			// MD 8/4/08 - Excel formula solving
			// We don't need to resolve this here. The GetResolvedValueForTypeQuery method will do that for us,
			// and this causes problems with the new way IsReference is implemented.
			//if ( value.IsReference )
			//{
			//    IUltraCalcReference reference = value.ToReference();
			//
			//    return 
			//        reference.IsEnumerable == false && 
			//        UltraCalcFunction.IsDateTime( reference.Value );
			//}
			//
			//
			//return value.Value is DateTime;
			if ( value.IsReference && value.ToReference().IsEnumerable )
				return false;




			return value.GetResolvedValue() is DateTime; 

		}
		#endregion //IsDateTime

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public virtual bool CanParameterBeEnumerable(int parameterIndex)
		{
			return false;
		}

		#endregion  // CanParameterBeEnumerable

		// MD 2/29/12 - TFS103395
		#region Add

		internal static UltraCalcValue Add(UltraCalcValue left, UltraCalcValue right)
		{
			double leftNumber;
			double rightNumber;
			if (left.ToDouble(out leftNumber) == false || right.ToDouble(out rightNumber) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			return new UltraCalcValue(leftNumber + rightNumber);
		}

		#endregion // Add

		// MD 2/29/12 - TFS103395
		#region Divide

		internal static UltraCalcValue Divide(UltraCalcValue left, UltraCalcValue right)
		{
			double leftNumber;
			double rightNumber;
			if (left.ToDouble(out leftNumber) == false || right.ToDouble(out rightNumber) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			if (rightNumber == 0)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Div));

			return new UltraCalcValue(leftNumber / rightNumber);
		}

		#endregion // Divide

		// MD 2/29/12 - TFS103395
		#region Multiply

		internal static UltraCalcValue Multiply(UltraCalcValue left, UltraCalcValue right)
		{
			double leftNumber;
			double rightNumber;
			if (left.ToDouble(out leftNumber) == false || right.ToDouble(out rightNumber) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			return new UltraCalcValue(leftNumber * rightNumber);
		}

		#endregion // Multiply

		// MD 2/29/12 - TFS103395
		#region Subtract

		internal static UltraCalcValue Subtract(UltraCalcValue left, UltraCalcValue right)
		{
			double leftNumber;
			double rightNumber;
			if (left.ToDouble(out leftNumber) == false || right.ToDouble(out rightNumber) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			return new UltraCalcValue(leftNumber - rightNumber);
		}

		#endregion // Subtract



#region Infragistics Source Cleanup (Region)











































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


		#endregion //Methods
	}

	#endregion //UltraCalcFunction

	#region BuiltInFunctionBase
	/// <summary>
	/// Base class for the built in functions.
	/// </summary>	

	internal





		abstract class BuiltInFunctionBase : UltraCalcFunction
	{
		#region Constructor
		internal BuiltInFunctionBase()
		{
		}
		#endregion //Constructor


		#region Base class overrides
		/// <summary>
		/// Summary description for the function displayed by the formula builder tool
		/// </summary>
		public override string Description
		{
			get
			{
				return SR.GetString( string.Format( "Func_{0}_Desc", this.Name ) );
			}
		}

		/// <summary>
		/// Category description for the function displayed by the formula builder tool
		/// </summary>
		public override string Category
		{
			get
			{
				return SR.GetString( string.Format( "Func_{0}_Category", this.Name ) );
			}
		}

		// MRS - NAS 8.1
		internal virtual int ArgCount
		{
			get
			{
				return ( this.MaxArgs == int.MaxValue ) ? this.MinArgs : this.MaxArgs;
			}
		}

		/// <summary>
		/// Array list of argument names
		/// </summary>
		public override string[] ArgList
		{
			get
			{
				// MRS - NAS 8.1
				//int argListCount = (this.MaxArgs == int.MaxValue) ? this.MinArgs : this.MaxArgs;
				int argListCount = this.ArgCount;

				if ( argListCount == 0 )
					return new string[] { string.Empty };

				string[] argList = new String[ argListCount ];

				for ( int index = 0; index < argListCount; index++ )
				{
					string arg_name = string.Format( "Func_{0}_Arg_{1}", this.Name, index );
					argList[ index ] = SR.GetString( arg_name );

					#region Debug verification






					#endregion //Debug verification
				}

				return argList;
			}
		}

		/// <summary>
		/// Array list of argument descriptors
		/// </summary>
		public override string[] ArgDescriptors
		{
			get
			{
				// MRS - NAS 8.1
				//int argListCount = (this.MaxArgs == int.MaxValue) ? this.MinArgs : this.MaxArgs;
				int argListCount = this.ArgCount;

				if ( argListCount == 0 )
					return new string[] { string.Empty };

				string[] argDescriptors = new String[ argListCount ];

				for ( int index = 0; index < argListCount; index++ )
				{
					string arg_name = string.Format( "Func_{0}_ArgDesc_{1}", this.Name, index );

					argDescriptors[ index ] = SR.GetString( arg_name );

					#region Debug verification






					#endregion //Debug verification
				}

				return argDescriptors;
			}
		}

		#endregion //Base class overrides

		// MRS 11/29/04 
		// Added HelpURL
		#region HelpURL
		/// <summary>
		/// A URL to a help topic. 
		/// </summary>
		/// <remarks> 
		/// <p class="body">This property is used by the FunctionBuilder at design-time to provide a link to a help topic describing the function.</p>
		/// <p class="body">The default implementation returns the Func_CategoryURL_Template resource string. The first substitution string argument '{0}' will be replaced by the Func_FUNCTIONNAME_CategoryURL resource. The second substitution string is the Function Name."</p>
		/// </remarks>
		// SSP 10/20/11 TFS93173
		// Made this internal until we implement something in the UI for this.
		// 
		

		internal override string HelpURL



		{
			[SecuritySafeCritical] // MD 10/26/11 - TFS94120
			get
			{
				string functionName = this.Name;
				string functionCategoryURL = SR.GetString( string.Format( "Func_{0}_CategoryURL", functionName ) );
				// AS 5/3/06 BR12079
				//return SR.GetString("Func_CategoryURL_Template", functionCategoryURL, functionName);
                
                //AVezenkov (SL5 braking change): .GetName() isn't accessible
                //System.Reflection.AssemblyName assemblyName = new System.Reflection.AssemblyName(typeof(BuiltInFunctionBase).Assembly.FullName); 

				// SSP 11/18/11 - SL5 Support
				// Apparently GetName is not exposed in SL5.
				// 

				System.Reflection.Assembly assembly = typeof( BuiltInFunctionBase ).Assembly;
				System.Reflection.AssemblyName assemblyName = new System.Reflection.AssemblyName( assembly.FullName );




				return SR.GetString( "Func_CategoryURL_Template",
					functionCategoryURL, functionName,
					assemblyName.Version.Major, assemblyName.Version.Minor );

			}
		}
		#endregion HelpURL 

	}
	#endregion //BuiltInFunctionBase

	// MD 2/28/12 - TFS103395
	#region UltraCalcBinaryOperatorBase

	/// <summary>
	/// Abstract base class for binary operator functions.
	/// </summary>

	internal





 abstract class UltraCalcBinaryOperatorBase : BuiltInFunctionBase
	{
		#region Base Class Overrides

		#region Evaluate

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An UltraCalcValue represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			Debug.Assert(argumentCount == 2, "Incorrect argument count for the binary operator.");

			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue(left.ToErrorValue());

			if (right.IsError)
				return new UltraCalcValue(right.ToErrorValue());

			return this.Evaluate(left, right);
		}

		#endregion // Evaluate

		#region MinArgs

		/// <summary>
		/// Minimum number of arguments required for the function.
		/// </summary>
		public override int MinArgs
		{
			get { return 2; }
		}

		#endregion // MinArgs

		#region MaxArgs

		/// <summary>
		/// Maximum number of arguments required for the function.
		/// </summary>
		public override int MaxArgs
		{
			get { return 2; }
		}

		#endregion // MaxArgs



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)


		#endregion // Base Class Overrides

		#region Methods

		/// <summary>
		/// Evaluates the the binary operator with the specified arguments and returns the result.
		/// </summary>
		protected abstract UltraCalcValue Evaluate(UltraCalcValue left, UltraCalcValue right);

		#endregion // Methods
	}

	#endregion // UltraCalcBinaryOperatorBase

	#region UltraCalcFunctionUnknown
	
#region Infragistics Source Cleanup (Region)
























































#endregion // Infragistics Source Cleanup (Region)

	#endregion //UltraCalcFunctionUnknown

	#region UltraCalcFunctionAverage
    /// <summary>
    /// Calculates an average (arithmetic mean) for a series of numbers.
    /// </summary>
    /// <remarks>
    /// <p class="body">AVERAGE(value1, value2, ...)</p>
    /// <p class="body">Value1, value2, ... are one or more numeric values or
    /// references to numeric values. An average (arithmetic mean) is calculated
    /// by taking the sum of all values, and dividing by the number of values.</p>
    /// <p class="body">Each value is equally weighted. To obtain a weighted average,
    /// you can multiply each value by a weight in the expression.</p>
    /// <code>
    /// AVERAGE( [Value1]*[Weight1], [Value2]*[Weight2], [Value3]*[Weight3])
    /// </code>
    /// </remarks>

	internal





        class UltraCalcFunctionAverage : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 2/14/11 - TFS66313
			// Moved all code to EvaluateHelper so it could be used in other places.
			//int valueCount = 0;
			//double totalValue = 0.0;
			//
			//UltraCalcValue[] args = this.GetArguments( numberStack, argumentCount, true );
			//
			//foreach ( UltraCalcValue arg in args )
			//{
			//    if ( null != arg )
			//    {
			//        // JJD 9/09/04 - UWC99
			//        // If the value is an error than raise the exception
			//        if ( arg.IsError )
			//            return new UltraCalcValue( arg.ToErrorValue() );
			//
			//        if (!arg.IsBoolean )
			//        {
			//            double value;
			//
			//            if (arg.ToDouble(out value))
			//            {
			//                totalValue += value;
			//                valueCount++;
			//            }
			//        }
			//    }
			//}
			//
			//totalValue /= valueCount;
			//
			//return new UltraCalcValue( totalValue );
			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			return EvaluateHelper(args);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		// MD 2/14/11 - TFS66313
		// Moved this code from Evaluate so it could be used in other places.
		internal static UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			int valueCount = 0;
			double totalValue = 0.0;

			foreach (UltraCalcValue arg in args)
			{
				if (null != arg)
				{
					// JJD 9/09/04 - UWC99
					// If the value is an error than raise the exception
					if (arg.IsError)
						return new UltraCalcValue(arg.ToErrorValue());

					if (!arg.IsBoolean)
					{
						double value;

						if (arg.ToDouble(out value))
						{
							totalValue += value;
							valueCount++;
						}
					}
				}
			}

			totalValue /= valueCount;

			return new UltraCalcValue(totalValue);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "average"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionAverage

	#region UltraCalcFunctionSum
    /// <summary>
    /// Adds a series of numbers to obtain a total amount.
    /// </summary>
    /// <remarks>
    /// <p class="body">SUM(value1,value2,...)</p>
    /// <p class="body">Value1, value2, ... are references for which you want to find the total value.
    /// Text representations of numbers and literal numeric constants that you specify in the list of
    /// arguments will be included in the sum. If <em>value</em> is a column or vector reference then
    /// the sum will be taken of all cells or values contained by the reference. Any error values in
    /// the argument list, or text that is not convertible to a numeric value, will produce an error.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionSum : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 2/14/11 - TFS66313
			// Moved all code to EvaluateHelper so it could be used in other places.
			//double totalValue = 0.0;
			//
			//UltraCalcValue[] args = this.GetArguments( numberStack, argumentCount, true );
			//
			//foreach ( UltraCalcValue arg in args )
			//{
			//    if ( null != arg )
			//    {
			//        // JJD 9/09/04 - UWC99
			//        // If the value is an error than raise the exception
			//        if ( arg.IsError )
			//            return new UltraCalcValue( arg.ToErrorValue() );
			//
			//        double value;
			//
			//        if (arg.ToDouble(out value))
			//            totalValue += value;
			//    }
			//}
			//
			//return new UltraCalcValue( totalValue );
			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			return EvaluateHelper(args);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		// MD 2/14/11 - TFS66313
		// Moved this code from Evaluate so it could be used in other places.
		internal static UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			double totalValue = 0.0;

			foreach (UltraCalcValue arg in args)
			{
				if (null != arg)
				{
					// JJD 9/09/04 - UWC99
					// If the value is an error than raise the exception
					if (arg.IsError)
						return new UltraCalcValue(arg.ToErrorValue());

					double value;

					if (arg.ToDouble(out value))
						totalValue += value;
				}
			}

			return new UltraCalcValue(totalValue);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "sum"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSum

	#region UltraCalcFunctionUnaryPlus
    /// <summary>
    /// '+' Unary plus formula operator (+20)
    /// </summary>

	internal





        class UltraCalcFunctionUnaryPlus : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 7/18/08 - Excel formula solving
			// The unary plus operator does no processing in Excel and just returns the parameter passed to it.
			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if ( UltraCalcValue.UseExcelFormulaCompatibility )
			if ( UltraCalcValue.UseExcelFunctionCompatibility )
			{
				UltraCalcValue value = numberStack.Pop();

				// Although it does no processing, references on the stack must be dereferenced to add any dynamic 
				// references and check for circularities if this was a dynamic reference.
				if ( value.IsReference )
				{
					// MD 4/10/12v
					// Found while fixing TFS108678
					// If an enumerable reference is specified, a #VALUE! error should be returned.
					//value = value.ToReference().Value;
					value = new UltraCalcValue(value.GetResolvedValue());
				}

				return value;
			}

			//				if (((UltraCalcValue)(numberStack.Peek() < 0)).ToBoolean(CultureInfo.InvariantCulture))  
			//					numberStack.Push(numberStack.Pop() * -1);

			// SSP 12/15/04 BR01189
			// Commented out the original code and added the new code below. Unary +, just like
			// the unary - should work on numeric values. In other words "+ [//A]" should give
			// 0 if //A was empty string just like "- [//A]".
			//
			// --------------------------------------------------------------------------------
			//return numberStack.Pop();

#pragma warning disable 0162
			UltraCalcValue calcValue = numberStack.Pop();
#pragma warning restore 0162

			if (calcValue.IsError)
				return new UltraCalcValue( calcValue.ToErrorValue() );

			double result;

			if (!calcValue.ToDouble(out result))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( result );
			// --------------------------------------------------------------------------------
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "+()"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionUnaryPlus

	#region UltraCalcFunctionUnaryMinus
	/// <summary>
	/// '-' Negation formula operator (-20)
	/// </summary>

	internal





        class UltraCalcFunctionUnaryMinus : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue calcValue = numberStack.Pop();

			if (calcValue.IsError)
				return new UltraCalcValue( calcValue.ToErrorValue() );

			double result;

			if (!calcValue.ToDouble(out result))
			{
				// MD 7/18/08
				// Found while implementing Excel formula solving
				// The wrong error is being returned here. It should have returned a #VALUE! error, which can be seen in Excel.
				//return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
			}

			return new UltraCalcValue( result * -1 );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "-()"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionUnaryMinus

	#region UltraCalcFunctionPlus
	/// <summary>
	/// '+' Addition formula operator (1+2)
	/// </summary>

	internal





		// MD 2/28/12 - TFS103395
        //class UltraCalcFunctionPlus : BuiltInFunctionBase
		class UltraCalcFunctionPlus : UltraCalcBinaryOperatorBase
	{
		// MD 2/28/12 - TFS103395
		// This is now implemented on the base.
		#region Removed

		///// <summary>
		///// Evaluates the function against the arguments on the number stack
		///// </summary>
		///// <param name="numberStack">Formula number stack containing function arguments</param>
		///// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		///// <returns>An UltraCalcValue represents the result of the function evaluation.</returns>
		//protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		//{
		//    //~ AS 9/7/04 Added date addition support similar to that of Excel except we'll support commutative operations
		//    UltraCalcValue right = numberStack.Pop();
		//    UltraCalcValue left = numberStack.Pop();

		//    if (left.IsError)
		//        return new UltraCalcValue( left.ToErrorValue() );

		//    if (right.IsError)
		//        return new UltraCalcValue( right.ToErrorValue() );

		//    bool leftIsDate = IsDateTime(left);
		//    bool rightIsDate = IsDateTime(right);

		//    if (leftIsDate || rightIsDate)
		//    {
		//        DateTime newDate;

		//        if (leftIsDate && rightIsDate)
		//            newDate = new DateTime( left.ToInt64(CultureInfo.InvariantCulture) + right.ToInt64(CultureInfo.InvariantCulture));
		//        else if (leftIsDate)
		//        {
		//            double d;
		//            if (!right.ToDouble(out d))
		//                return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

		//            newDate = left.ToDateTime(CultureInfo.InvariantCulture).AddDays( d );
		//        }
		//        else
		//        {
		//            double d;
		//            if (!left.ToDouble(out d))
		//                return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

		//            newDate = right.ToDateTime(CultureInfo.InvariantCulture).AddDays( d );
		//        }

		//        return new UltraCalcValue(newDate);
		//    }

		//    double leftDbl, rightDbl;

		//    if (!left.ToDouble(out leftDbl) || !right.ToDouble(out rightDbl))
		//    {
		//        // MD 7/14/08
		//        // Found while implementing Excel formula solving
		//        // The wrong error is being returned here. It should have returned a #VALUE! error, which can be seen in Excel.
		//        //return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
		//        return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
		//    }

		//    return new UltraCalcValue(leftDbl + rightDbl);
		//}

		#endregion // Removed

		// MD 2/28/12 - TFS103395
		#region Evaluate

		/// <summary>
		/// Evaluates the the binary operator with the specified arguments and returns the result.
		/// </summary>
		protected override UltraCalcValue Evaluate(UltraCalcValue left, UltraCalcValue right)
		{
			bool leftIsDate = IsDateTime(left);
			bool rightIsDate = IsDateTime(right);

			if (leftIsDate || rightIsDate)
			{
				DateTime newDate;

				if (leftIsDate && rightIsDate)
				{
					// MD 4/6/12 - TFS101506
					//newDate = new DateTime(left.ToInt64(CultureInfo.InvariantCulture) + right.ToInt64(CultureInfo.InvariantCulture));
					newDate = new DateTime(left.ToInt64() + right.ToInt64());
				}
				else if (leftIsDate)
				{
					double d;
					if (!right.ToDouble(out d))
						return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

					// MD 4/6/12 - TFS101506
					//newDate = left.ToDateTime(CultureInfo.InvariantCulture).AddDays(d);
					newDate = left.ToDateTime().AddDays(d);
				}
				else
				{
					double d;
					if (!left.ToDouble(out d))
						return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

					// MD 4/6/12 - TFS101506
					//newDate = right.ToDateTime(CultureInfo.InvariantCulture).AddDays(d);
					newDate = right.ToDateTime().AddDays(d);
				}

				return new UltraCalcValue(newDate);
			}

			return UltraCalcFunction.Add(left, right);
		}

		#endregion // Evaluate

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "+"; }
		}
		#endregion //Name

		// MD 2/28/12 - TFS103395
		// These are now implemented on the base.
		#region Removed

		//        #region Min/Max args
		//        /// <summary>
		//        /// Minimum number of arguments required for the function
		//        /// </summary>
		//        public override int MinArgs
		//        { 
		//            get { return 2; }
		//        }

		//        /// <summary>
		//        /// Maximum number of arguments required for the function
		//        /// </summary>
		//        public override int MaxArgs
		//        { 
		//            get { return 2; }
		//        }
		//        #endregion //Min/Max args

		//#if EXCEL
		//        // MD 7/16/08 - Excel formula solving
		//        #region GetExpectedParameterClass

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the Excel token class expected in the parameter at the specified index.
		//        /// </summary> 
		//#endif
		//        internal override TokenClass GetExpectedParameterClass( int index )
		//        {
		//            if ( index == 0 || index == 1 )
		//                return TokenClass.Value;

		//            return base.GetExpectedParameterClass( index );
		//        }

		//        #endregion GetExpectedParameterClass

		//        #region ReturnTokenClass

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the token class of the value returned from the function.
		//        /// </summary>  
		//#endif
		//        internal override TokenClass ReturnTokenClass
		//        {
		//            get { return TokenClass.Value; }
		//        }

		//        #endregion ReturnTokenClass
		//#endif

		#endregion // Removed
	}
	#endregion //UltraCalcFunctionPlus

	#region UltraCalcFunctionMinus
	/// <summary>
	/// '-' Subtraction formula operator (3-2)
	/// </summary>

	internal





		// MD 2/28/12 - TFS103395
        //class UltraCalcFunctionMinus : BuiltInFunctionBase
		class UltraCalcFunctionMinus : UltraCalcBinaryOperatorBase
	{
		// MD 2/28/12 - TFS103395
		// This is now implemented on the base.
		#region Removed

		///// <summary>
		///// Evaluates the function against the arguments on the number stack
		///// </summary>
		///// <param name="numberStack">Formula number stack containing function arguments</param>
		///// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		///// <returns>An UltraCalcValue represents the result of the function evaluation.</returns>
		//protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		//{
		//    //~ AS 9/7/04 Added date subtraction support similar to that of Excel
		//    UltraCalcValue right = numberStack.Pop();
		//    UltraCalcValue left = numberStack.Pop();

		//    if (left.IsError)
		//        return new UltraCalcValue( left.ToErrorValue() );

		//    if (right.IsError)
		//        return new UltraCalcValue( right.ToErrorValue() );

		//    bool leftIsDate = IsDateTime(left);
		//    bool rightIsDate = IsDateTime(right);

		//    if (leftIsDate || rightIsDate)
		//    {
		//        long ticks;

		//        if (leftIsDate && rightIsDate)
		//            ticks = left.ToDateTime(CultureInfo.InvariantCulture).Subtract( right.ToDateTime(CultureInfo.InvariantCulture) ).Ticks;
		//        else if (leftIsDate)
		//        {
		//            double d;

		//            if (!right.ToDouble(out d))
		//                return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

		//            ticks = left.ToDateTime(CultureInfo.InvariantCulture).AddDays( -d ).Ticks;
		//        }
		//        else
		//        {
		//            return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
		//        }

		//        return new UltraCalcValue( new DateTime(ticks) );
		//    }

		//    double leftDbl, rightDbl;

		//    if (!left.ToDouble(out leftDbl) || !right.ToDouble(out rightDbl))
		//    {
		//        // MD 7/14/08
		//        // Found while implementing Excel formula solving
		//        // The wrong error is being returned here. It should have returned a #VALUE! error, which can be seen in Excel.
		//        //return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
		//        return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
		//    }

		//    return new UltraCalcValue(leftDbl - rightDbl);
		//}

		#endregion // Removed

		// MD 2/28/12 - TFS103395
		#region Evaluate

		/// <summary>
		/// Evaluates the the binary operator with the specified arguments and returns the result.
		/// </summary>
		protected override UltraCalcValue Evaluate(UltraCalcValue left, UltraCalcValue right)
		{
			bool leftIsDate = IsDateTime(left);
			bool rightIsDate = IsDateTime(right);

			if (leftIsDate || rightIsDate)
			{
				long ticks;

				if (leftIsDate && rightIsDate)
				{
					// MD 4/6/12 - TFS101506
					//ticks = left.ToDateTime(CultureInfo.InvariantCulture).Subtract(right.ToDateTime(CultureInfo.InvariantCulture)).Ticks;
					ticks = left.ToDateTime().Subtract(right.ToDateTime()).Ticks;
				}
				else if (leftIsDate)
				{
					double d;

					if (!right.ToDouble(out d))
						return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

					// MD 4/6/12 - TFS101506
					//ticks = left.ToDateTime(CultureInfo.InvariantCulture).AddDays(-d).Ticks;
					ticks = left.ToDateTime().AddDays(-d).Ticks;
				}
				else
				{
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
				}

				return new UltraCalcValue(new DateTime(ticks));
			}

			return UltraCalcFunction.Subtract(left, right);
		}

		#endregion // Evaluate

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "-"; }
		}
		#endregion //Name

		// MD 2/28/12 - TFS103395
		// These are now implemented on the base.
		#region Removed

		//        #region Min/Max args
		//        /// <summary>
		//        /// Minimum number of arguments required for the function
		//        /// </summary>
		//        public override int MinArgs
		//        { 
		//            get { return 2; }
		//        }

		//        /// <summary>
		//        /// Maximum number of arguments required for the function
		//        /// </summary>
		//        public override int MaxArgs
		//        { 
		//            get { return 2; }
		//        }
		//        #endregion //Min/Max args

		//#if EXCEL
		//        // MD 7/16/08 - Excel formula solving
		//        #region GetExpectedParameterClass

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the Excel token class expected in the parameter at the specified index.
		//        /// </summary> 
		//#endif
		//        internal override TokenClass GetExpectedParameterClass( int index )
		//        {
		//            if ( index == 0 || index == 1 )
		//                return TokenClass.Value;

		//            return base.GetExpectedParameterClass( index );
		//        }

		//        #endregion GetExpectedParameterClass

		//        #region ReturnTokenClass

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the token class of the value returned from the function.
		//        /// </summary>  
		//#endif
		//        internal override TokenClass ReturnTokenClass
		//        {
		//            get { return TokenClass.Value; }
		//        }

		//        #endregion ReturnTokenClass
		//#endif

		#endregion // Removed
	}
	#endregion //UltraCalcFunctionMinus

	#region UltraCalcFunctionPercent
	/// <summary>
	///  '%' Percent formula operator (20%)
	/// </summary>

	internal





        class UltraCalcFunctionPercent : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue calcValue = numberStack.Pop();

			if (calcValue.IsError)
				return new UltraCalcValue( calcValue.ToErrorValue() );

			double value;
			
			if (!calcValue.ToDouble(out value))
			{
				// MD 7/14/08
				// Found while implementing Excel formula solving
				// The wrong error is being returned here. It should have returned a #VALUE! error, which can be seen in Excel.
				//return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
			}

			return new UltraCalcValue(value / 100d);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "%"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionPercent

	#region UltraCalcFunctionMultiply
	/// <summary>
	/// '*' Multiplication formula operator (2*3)
	/// </summary>

	internal





		// MD 2/28/12 - TFS103395
        //class UltraCalcFunctionMultiply : BuiltInFunctionBase
		class UltraCalcFunctionMultiply : UltraCalcBinaryOperatorBase
	{
		// MD 2/28/12 - TFS103395
		// This is now implemented on the base.
		#region Removed

		///// <summary>
		///// Evaluates the function against the arguments on the number stack
		///// </summary>
		///// <param name="numberStack">Formula number stack containing function arguments</param>
		///// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		///// <returns>An UltraCalcValue represents the result of the function evaluation.</returns>
		//protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		//{
		//    UltraCalcValue right = numberStack.Pop();
		//    UltraCalcValue left = numberStack.Pop();

		//    if (left.IsError)
		//        return new UltraCalcValue( left.ToErrorValue() );

		//    if (right.IsError)
		//        return new UltraCalcValue( right.ToErrorValue() );

		//    double leftDbl, rightDbl;

		//    if (!right.ToDouble(out rightDbl) || !left.ToDouble(out leftDbl))
		//    {
		//        // MD 7/14/08
		//        // Found while implementing Excel formula solving
		//        // The wrong error is being returned here. It should have returned a #VALUE! error, which can be seen in Excel.
		//        //return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
		//        return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
		//    }

		//    return new UltraCalcValue(leftDbl * rightDbl);
		//}

		#endregion // Removed

		// MD 2/28/12 - TFS103395
		#region Evaluate

		/// <summary>
		/// Evaluates the the binary operator with the specified arguments and returns the result.
		/// </summary>
		protected override UltraCalcValue Evaluate(UltraCalcValue left, UltraCalcValue right)
		{
			return UltraCalcFunction.Multiply(left, right);
		}

		#endregion // Evaluate

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "*"; }
		}
		#endregion //Name

		// MD 2/28/12 - TFS103395
		// These are now implemented on the base.
		#region Removed

		//        #region Min/Max args
		//        /// <summary>
		//        /// Minimum number of arguments required for the function
		//        /// </summary>
		//        public override int MinArgs
		//        { 
		//            get { return 2; }
		//        }

		//        /// <summary>
		//        /// Maximum number of arguments required for the function
		//        /// </summary>
		//        public override int MaxArgs
		//        { 
		//            get { return 2; }
		//        }
		//        #endregion //Min/Max args

		//#if EXCEL
		//        // MD 7/16/08 - Excel formula solving
		//        #region GetExpectedParameterClass

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the Excel token class expected in the parameter at the specified index.
		//        /// </summary> 
		//#endif
		//        internal override TokenClass GetExpectedParameterClass( int index )
		//        {
		//            if ( index == 0 || index == 1 )
		//                return TokenClass.Value;

		//            return base.GetExpectedParameterClass( index );
		//        }

		//        #endregion GetExpectedParameterClass

		//        #region ReturnTokenClass

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the token class of the value returned from the function.
		//        /// </summary>  
		//#endif
		//        internal override TokenClass ReturnTokenClass
		//        {
		//            get { return TokenClass.Value; }
		//        }

		//        #endregion ReturnTokenClass
		//#endif

		#endregion // Removed
	}
	#endregion //UltraCalcFunctionMultiply

	#region UltraCalcFunctionDivide
	/// <summary>
	/// '/' Division formula operator (3/2)
	/// </summary>

	internal





		// MD 2/28/12 - TFS103395
        //class UltraCalcFunctionDivide : BuiltInFunctionBase
		class UltraCalcFunctionDivide : UltraCalcBinaryOperatorBase
	{
		// MD 2/28/12 - TFS103395
		// This is now implemented on the base.
		#region Removed

		///// <summary>
		///// Evaluates the function against the arguments on the number stack
		///// </summary>
		///// <param name="numberStack">Formula number stack containing function arguments</param>
		///// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		///// <returns>An UltraCalcValue represents the result of the function evaluation.</returns>
		//protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		//{
		//    UltraCalcValue right = numberStack.Pop();
		//    UltraCalcValue left = numberStack.Pop();

		//    if (left.IsError)
		//        return new UltraCalcValue( left.ToErrorValue() );

		//    if (right.IsError)
		//        return new UltraCalcValue( right.ToErrorValue() );

		//    //double leftDbl, rightDbl;
		//    //
		//    //if (!right.ToDouble(out rightDbl) || !left.ToDouble(out leftDbl))
		//    //{
		//    //    // MD 7/14/08
		//    //    // Found while implementing Excel formula solving
		//    //    // The wrong error is being returned here. It should have returned a #VALUE! error, which can be seen in Excel.
		//    //    //return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
		//    //    return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
		//    //}
		//    decimal leftDbl, rightDbl;
		//    if (right.ToDecimal(out rightDbl) == false || left.ToDecimal(out leftDbl) == false)
		//        return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

		//    // MRS 9/16/2008 - TFS6956
		//    // We were never returning a #DIV/0! error when dividing by zero.
		//    //if (Math.Abs(rightDbl) == 0.0)
		//    if (Math.Abs(rightDbl) == 0M)
		//        return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Div));

		//    return new UltraCalcValue(leftDbl / rightDbl);
		//}

		#endregion // Removed

		// MD 2/28/12 - TFS103395
		#region Evaluate

		/// <summary>
		/// Evaluates the the binary operator with the specified arguments and returns the result.
		/// </summary>
		protected override UltraCalcValue Evaluate(UltraCalcValue left, UltraCalcValue right)
		{
			return UltraCalcFunction.Divide(left, right);
		}

		#endregion // Evaluate

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "/"; }
		}
		#endregion //Name

		// MD 2/28/12 - TFS103395
		// These are now implemented on the base.
		#region Removed

		//        #region Min/Max args
		//        /// <summary>
		//        /// Minimum number of arguments required for the function
		//        /// </summary>
		//        public override int MinArgs
		//        { 
		//            get { return 2; }
		//        }

		//        /// <summary>
		//        /// Maximum number of arguments required for the function
		//        /// </summary>
		//        public override int MaxArgs
		//        { 
		//            get { return 2; }
		//        }
		//        #endregion //Min/Max args

		//#if EXCEL
		//        // MD 7/16/08 - Excel formula solving
		//        #region GetExpectedParameterClass

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the Excel token class expected in the parameter at the specified index.
		//        /// </summary> 
		//#endif
		//        internal override TokenClass GetExpectedParameterClass( int index )
		//        {
		//            if ( index == 0 || index == 1 )
		//                return TokenClass.Value;

		//            return base.GetExpectedParameterClass( index );
		//        }

		//        #endregion GetExpectedParameterClass

		//        #region ReturnTokenClass

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the token class of the value returned from the function.
		//        /// </summary>  
		//#endif
		//        internal override TokenClass ReturnTokenClass
		//        {
		//            get { return TokenClass.Value; }
		//        }

		//        #endregion ReturnTokenClass
		//#endif

		#endregion // Removed
	}
	#endregion //UltraCalcFunctionDivide

	#region UltraCalcFunctionConcat
	/// <summary>
	/// '&amp;' formula operator used to concatenate two strings. ("First" &amp; "Second")
	/// </summary>

	internal





        class UltraCalcFunctionConcat : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();

			// MD 7/14/08
			// Found while implementing Excel formula solving
			// Moved from below: If both the left and the right operands are errors, the error of the left operand should be returned,
			// so we need to pop it off and check it before checking the right side.
			UltraCalcValue left = numberStack.Pop();

			if ( left.IsError )
				return new UltraCalcValue( left.ToErrorValue() );

			// JJD 9/09/04 - UWC99
			// If the value is an error than raise the exception
			if ( right.IsError )
				return new UltraCalcValue( right.ToErrorValue() );

			string rightS = right.ToString();

			// MD 7/14/08
			// Found while implementing Excel formula solving
			// Moved above: See comment above.
			//UltraCalcValue left = numberStack.Pop();
			//
			//// JJD 9/09/04 - UWC99
			//// If the value is an error than raise the exception
			//if ( left.IsError )
			//    return new UltraCalcValue( left.ToErrorValue() );

			string leftS = left.ToString();

			return new UltraCalcValue(leftS + rightS);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "&"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionConcat

	#region UltraCalcFunctionConcatenate
    /// <summary>
    /// Combines two or more text values into a single text string.
    /// </summary>
    /// <remarks>
    /// <p class="body">CONCATENATE(text1, text2, ..., textN)</p>
    /// <p class="body">Text1, text2, ..., textN are multiple text values that
    /// you want to concatenate into one text string. These text values may be
    /// text strings, numbers (which will be converted into text), or a single
    /// value reference (such as a cell reference) containing such a value.</p>
    /// <p class="body">This function provides for elementary text processing
    /// in UltraCalc, such as when building message text or appending some
    /// connective text or punctuation to the results of evaluating other
    /// text-bearing expressions.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionConcatenate : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			// Remainder of stack args are cash flows that we want to process front to back so
			// collect the arguments into an array for processing.
            List<UltraCalcValue> args = new List<UltraCalcValue>();

			for (int arg_count=0; arg_count < argumentCount; arg_count++) 
				args.Add(numberStack.Pop());

			for (int arg_count=args.Count; arg_count > 0; arg_count--) 
			{
				UltraCalcValue valueArg = (UltraCalcValue)args[arg_count-1];

				// MD 4/10/12
				// Found while fixing TFS108678
				// Excel does not concatenate individual values from enumerable values.
				//if (valueArg.IsReference && valueArg.ToReference().IsEnumerable)
				if (UltraCalcValue.UseExcelFunctionCompatibility == false && 
#pragma warning disable 0429
					valueArg.IsReference && valueArg.ToReference().IsEnumerable)
#pragma warning restore 0429
				{
					IUltraCalcReferenceCollection ReferenceCollection = valueArg.ToReference().References;
					foreach (IUltraCalcReference cellReference in ReferenceCollection)
					{
						// JJD 9/09/04 - UWC99
						// If the value is an error than raise the exception
						if (cellReference.Value.IsError)
							return new UltraCalcValue(cellReference.Value.ToErrorValue());

						// MD 4/6/12 - TFS101506
						//sb.Append( cellReference.Value.ToString(CultureInfo.InvariantCulture) );
						sb.Append(cellReference.Value.ToString());
					}
				}
				else 
				{
					// JJD 9/09/04 - UWC99
					// If the value is an error than raise the exception
					if ( valueArg.IsError )
						return new UltraCalcValue( valueArg.ToErrorValue() );

					// MD 4/6/12 - TFS101506
					//sb.Append( valueArg.ToString(CultureInfo.InvariantCulture) );
					sb.Append(valueArg.ToString());
				}
			}

			return new UltraCalcValue(sb.ToString());
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "concatenate"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionConcatenate

	#region UltraCalcFunctionEqual
	/// <summary>
	/// '=' Equality formula operator (Price = 200)
	/// </summary>

	internal





        class UltraCalcFunctionEqual : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			bool isSame = UltraCalcValue.AreValuesEqual(left, right);

			return new UltraCalcValue(isSame);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "="; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionEqual

	#region UltraCalcFunctionNE
	/// <summary>
	/// "&lt;&gt;" Inequality formula operator (Price &lt;&gt; 200)
	/// </summary>

	internal





        class UltraCalcFunctionNE : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			bool isSame = UltraCalcValue.AreValuesEqual(left, right);

			return new UltraCalcValue( !isSame );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "<>"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionNE

	#region UltraCalcFunctionLT
	/// <summary>
	/// '&lt;' Less than formula operator (Price &lt; 200)
	/// </summary>

	internal





        class UltraCalcFunctionLT : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			// SSP 1/7/05 BR01401
			// Also make the <, >, <=, >= etc... operators work with non-numeric types
			// as well, like date time. Added CompareTo on UltraCalcValue class.
			//
			// ----------------------------------------------------------------------
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			return new UltraCalcValue( UltraCalcValue.CompareTo( left, right ) < 0 );
			// ----------------------------------------------------------------------
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "<"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionLT

	#region UltraCalcFunctionLE
	/// <summary>
	/// "&lt;=" Less than or equal formula operator (Price &lt;= 200)
	/// </summary>

	internal





        class UltraCalcFunctionLE : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			// SSP 1/7/05 BR01401
			// Also make the <, >, <=, >= etc... operators work with non-numeric types
			// as well, like date time. Added CompareTo on UltraCalcValue class.
			//
			// ----------------------------------------------------------------------
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			return new UltraCalcValue( UltraCalcValue.CompareTo( left, right ) <= 0 );
			// ----------------------------------------------------------------------
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "<="; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionLE

	#region UltraCalcFunctionGT
	/// <summary>
	/// "&gt;" Greater than formula operator (Price > 200)
	/// </summary>

	internal





        class UltraCalcFunctionGT : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			// SSP 1/7/05 BR01401
			// Also make the <, >, <=, >= etc... operators work with non-numeric types
			// as well, like date time. Added CompareTo on UltraCalcValue class.
			//
			// ----------------------------------------------------------------------
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			return new UltraCalcValue( UltraCalcValue.CompareTo( left, right ) > 0 );
			// ----------------------------------------------------------------------
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return ">"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionGT

	#region UltraCalcFunctionGE
	/// <summary>
	/// "&gt;=" Greater than or equal formula operator (Price >= 200)
	/// </summary>

	internal





        class UltraCalcFunctionGE : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			// SSP 1/7/05 BR01401
			// Also make the <, >, <=, >= etc... operators work with non-numeric types
			// as well, like date time. Added CompareTo on UltraCalcValue class.
			//
			// ----------------------------------------------------------------------
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			return new UltraCalcValue( UltraCalcValue.CompareTo( left, right ) >= 0 );
			// ----------------------------------------------------------------------
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return ">="; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionGE

	#region UltraCalcFunctionExpon
	/// <summary>
	/// "^" Exponentiation formula operator (3^2)
	/// </summary>

	internal





        class UltraCalcFunctionExpon : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			double leftDbl, rightDbl;

			if (!right.ToDouble(out rightDbl) || !left.ToDouble(out leftDbl))
			{
				// MD 7/14/08
				// Found while implementing Excel formula solving
				// The wrong error is being returned here. It should have returned a #VALUE! error, which can be seen in Excel.
				//return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
			}

			return new UltraCalcValue(Math.Pow(leftDbl, rightDbl));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "^"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionExpon

	#region UltraCalcFunctionIf
    /// <summary>
    /// Chooses between two outcomes (or UltraCalc expressions to evaluate) based on
    /// the result of a logical test on a value or UltraCalc expression you specify.
    /// </summary>
    /// <remarks>
    /// <p class="body">IF( boolean_test, result_if_true, [result_if_false])</p>
    /// <p class="body">Boolean_test is a value (or an UltraCalc expression) which the IF function
    /// evaluates to produce a boolean value of either TRUE or FALSE. The outcome of
    /// this test determines which result will be returned by the function.
    /// </p>
    /// <p class="body">Result_if_true is a value (or the outcome of another
    /// UltraCalc expression) that will be returned only when boolean_test has
    /// evaluated to the boolean value, TRUE.</p>
    /// <p class="body">Result_if_false is a value (or the outcome of another
    /// UltraCalc expression) that will be returned only when boolean_test has
    /// evaluated to the boolean value, FALSE.</p>
    /// <p class="body">The IF function allows you to write an UltraCalc expression
    /// that branches to one expression (when the boolean_test is TRUE) or another
    /// (when the boolean_test is FALSE) based on an arbitrary condition you have
    /// specified.</p>
	/// <p class="body">The result_if_false is not specified and boolean_test is FALSE, the 
	/// IF function will return FALSE.</p>
    /// <p class="note">If boolean_test's evaluation produces an error value then
    /// neither Result_if_true nor Result_if_false will be evaluated. Instead, the
    /// IF function returns the error value from its evaluation of boolean_test.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIf : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 7/14/08
			// Found while implementing Excel formula solving
			// The 3rd argument to the IF function is optional. If it is not specified and the condition is False, the function returns False.
			//UltraCalcValue ifFalse = numberStack.Pop();
			UltraCalcValue ifFalse = null;
			if ( argumentCount == 3 )
				ifFalse = numberStack.Pop();

			UltraCalcValue ifTrue = numberStack.Pop();
			UltraCalcValue comparison = numberStack.Pop();

			if (comparison.IsError)
				return new UltraCalcValue( comparison.ToErrorValue() );

			// MD 4/6/12 - TFS101506
			//if( comparison.ToBoolean(CultureInfo.InvariantCulture) == true )
			if (comparison.ToBoolean() == true)
				return ifTrue;
			else
			{
				// MD 7/14/08
				// Found while implementing Excel formula solving
				// The 3rd argument to the IF function is optional. If it is not specified and the condition is False, the function returns False.
				if ( ifFalse == null )
					return new UltraCalcValue( false );

				return ifFalse;
			}
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "if"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			// MD 7/14/08
			// Found while implementing Excel formula solving
			// The 3rd argument to the IF function is optional. If it is not specified and the condition is False, the function returns False.
			//get { return 3; }
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)



































#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionIf

	#region UltraCalcFunctionAbs
    /// <summary>
    /// Calculates a number's absolute value.
    /// </summary>
    /// <remarks>
    /// <p class="body">ABS( value)</p>
    /// <p class="body">A number's absolute value is it's value without any
    /// sign. It represents the magnitude of a value while ignoring it's
    /// direction (positive or negative) on a number line or vector.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionAbs : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double result;

			if (!value.ToDouble(out result))
			{
				// MD 8/12/08
				// Found while implementing Excel formula solving
				// This should have returned a #VALUE! error instead of a #NUM! error.
				//return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
			}

			return new UltraCalcValue(Math.Abs(result));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "abs"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionAbs

	#region UltraCalcFunctionMod
    /// <summary>
    /// Gets the remainder following integer division of two numbers.
    /// </summary>
    /// <remarks>
    /// <p class="body">MOD(numerator, denominator)</p>
    /// <p class="body">Numerator is the number being divided by <em>denominator</em>.
    /// When <em>denominator</em> can only be taken from <em>numerator</em>
    /// a certain whole number of types (the quotient), any leftover is the
    /// remainder.</p>
    /// <p class="body">Denominator is the number dividing the <em>numerator</em>.
    /// Any remainder will have the sign of the <em>denominator</em>. This number
    /// cannot be zero, otherwise the MOD() function returns a #DIV/0 error.</p>
    /// <p class="body">If you want to perform integer division on these two numbers
    /// then use the <see cref="UltraCalcFunctionQuotient">QUOTIENT()</see> function.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionMod : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			double divisor, number;

			if (!right.ToDouble(out divisor) || !left.ToDouble(out number))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			if (divisor == 0.0) 
			{
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Div) );
			} 
			else 
			{
				// SSP 11/8/04 UWC171
				//
				// ----------------------------------------------------------------
				
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				double d = number % divisor;
				// ----------------------------------------------------------------

				// MD 12/30/08 - TFS11957
				// The return value must have the same sign as the divisor.
				int sign = Math.Sign( d );
				if ( sign != 0 && sign != Math.Sign( divisor ) )
					d *= -1;

				return new UltraCalcValue(d);
			}
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "mod"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionMod

	#region UltraCalcFunctionQuotient
    /// <summary>
    /// Performs integer division on two numbers disregarding any remainder.
    /// </summary>
    /// <remarks>
    /// <p class="body">QUOTIENT(numerator, denominator)</p>
    /// <p class="body">Numerator is a numeric value that will be divided
    /// by the <em>denominator</em>. It is sometimes called the dividend.
    /// </p>
    /// <p class="body">Denominator is the numeric value that divides the
    /// <em>numerator</em>. It is sometimes called the divisor.  It cannot
    /// be zero or a #DIV/0 error value will be returned.
    /// </p>
    /// <p class="body">When either the <em>numerator</em> or the 
    /// <em>denominator</em> is not a number, the QUOTIENT() function
    /// returns an error value (#VALUE!).</p>
    /// <p class="body">If you need the remainder from an integer division,
    /// use the <see cref="UltraCalcFunctionMod">MOD()</see> function.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionQuotient : BuiltInFunctionBase
	{

		/// <summary>
		/// Evaluates the function against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			double denominator, numerator;

			if (!right.ToDouble(out denominator) || !left.ToDouble(out numerator))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			if (denominator == 0.0) 
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Div) );
			else 
				// SSP 11/8/04 UWC170
				//
				//return new UltraCalcValue(Convert.ToInt32(numerator / denominator));
				return new UltraCalcValue( ( numerator - ( numerator % denominator ) ) / denominator );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "quotient"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionQuotient

	#region UltraCalcFunctionProduct
    /// <summary>
    /// Multiplies a series of numbers to return their total product.
    /// </summary>
    /// <remarks>
    /// <p class="body">PRODUCT( value1, value2, ..., valueN)</p>
    /// <p class="body">Value1 is the first number (the multiplicand) in a
    /// series of numbers that you want to multiply.</p>
    /// <p class="body">Value2 is the second number (the first multiplier)
    /// in a series of numbers that you want to multiply.</p>
    /// <p class="body">Value<em>N</em> is the last number (the last multiplier)
    /// in a series of numbers that you want to multiply.</p>
    /// <p class="body">This function offers a convenience when you need to
    /// multiply many numbers or expressions at one time.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionProduct : BuiltInFunctionBase
	{

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 2/14/11 - TFS66313
			// Moved all code to EvaluateHelper so it could be used in other places.
			//double prodValue = 1.0;
			//double temp;
			//
			//for (int arg_count=0; arg_count < argumentCount; arg_count++) 
			//{
			//    UltraCalcValue valueArg = numberStack.Pop();
			//
			//    if (valueArg.IsReference && valueArg.ToReference().IsEnumerable) 
			//    {
			//        IUltraCalcReferenceCollection ReferenceCollection  = valueArg.ToReference().References;
			//
			//        foreach (IUltraCalcReference cellReference in ReferenceCollection)
			//        {
			//            if (cellReference.Value.IsError)
			//                return new UltraCalcValue( cellReference.Value.ToErrorValue() );
			//			
			//            if (cellReference.Value.ToDouble(out temp))
			//                prodValue *= temp;
			//        }
			//    } 
			//    else 
			//    {                    
			//        if (valueArg.IsError)
			//            return new UltraCalcValue( valueArg.ToErrorValue() );
			//
			//        if (valueArg.ToDouble(out temp))
			//            prodValue *= temp;
			//    }
			//}
			//
			//return new UltraCalcValue( prodValue );
			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			return EvaluateHelper(args);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		// MD 2/14/11 - TFS66313
		// Moved this code from Evaluate so it could be used in other places.
		internal static UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			double prodValue = 1.0;

			foreach (UltraCalcValue arg in args)
			{
				if (null != arg)
				{
					// JJD 9/09/04 - UWC99
					// If the value is an error than raise the exception
					if (arg.IsError)
						return new UltraCalcValue(arg.ToErrorValue());

					double value;

					if (arg.ToDouble(out value))
						prodValue *= value;
				}
			}

			return new UltraCalcValue(prodValue);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "product"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionProduct

	#region UltraCalcFunctionPower
    /// <summary>
    /// Raises the specific number to a power.
    /// </summary>
    /// <remarks>
    /// <p class="body">POWER(value, exponent)</p>
    ///	<p class="body">Value is a numeric value or reference to a numeric value
    /// which you want to raise to a power.</p>
    /// <p class="body">Exponent is a real number power to which <em>value</em>
    /// is to be raised. Imaginary exponents are not supported.</p>
    /// <p class="body">Common applications of the POWER() function occur when
    /// you need to multiply a number against itself multiple times. For example,
    /// it is common in many computer applications to create bit mask values by
    /// raising the value 2 to integer exponents, which produce a sequence such
    /// as (1, 2, 4, 8, 16, 32, 64, 128, ...)</p>
    /// <p class="body">The POWER() function additionally supports fractional
    /// exponents, and can be used to emulate other functions such as the quad
    /// root (raising to an exponent of 0.25, which is 1/4) or the inverse
    /// square (raising to an exponent of -2.0).</p>
    /// </remarks>

	internal





        class UltraCalcFunctionPower : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue right = numberStack.Pop();
			UltraCalcValue left = numberStack.Pop();

			if (left.IsError)
				return new UltraCalcValue( left.ToErrorValue() );

			if (right.IsError)
				return new UltraCalcValue( right.ToErrorValue() );

			double leftDbl, rightDbl;

			if (!right.ToDouble(out rightDbl) || !left.ToDouble(out leftDbl))
			{
				// MD 2/7/11
				// We were returning the incorrect error code here.
				//return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
			}

			// MD 2/7/11
			// Excel returns errors with certain numbers, so we should do the same when using excel formula compatibility.
			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if (UltraCalcValue.UseExcelFormulaCompatibility && leftDbl == 0)
			if ( UltraCalcValue.UseExcelFunctionCompatibility && leftDbl == 0 )
			{
				if (rightDbl == 0)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
				else if (rightDbl < 0)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Div));
			}

			return new UltraCalcValue( Math.Pow(leftDbl,rightDbl));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "power"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionPower

	#region UltraCalcFunctionInt
    /// <summary>
    /// Converts a real numeric value (which may have a fractional part)
    /// into a whole number.
    /// </summary>
    /// <remarks>
    /// <p class="body">INT(value)</p>
    /// <p class="body">Value is a real numeric value that you want to
    /// convert into a whole number. INT() is a more specialized version
    /// of the <see cref="UltraCalcFunctionTrunc">TRUNC()</see> function
    /// because it always produces integer values (whereas the TRUNC()
    /// function allows you to specify a precision at which to truncate
    /// the numeric value.)</p>
    /// </remarks>

	internal





        class UltraCalcFunctionInt : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 9/26/08
			// A bug was fixed with ToInt and this function was relying on the bug
			//return new UltraCalcValue(numberStack.Pop().ToInt(CultureInfo.InvariantCulture));
			//#if SILVERLIGHT
			//            double d = numberStack.Pop().ToDouble();
			//            return new UltraCalcValue( (int)Math.Floor( d ) );
			//#else
			//            decimal d = numberStack.Pop().ToDecimal();
			//            return new UltraCalcValue( (int)Math.Floor( d ) );
			//#endif
			decimal d = numberStack.Pop().ToDecimal();
			return new UltraCalcValue((int)Decimal.Floor(d));
        }

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "int"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionInt

	#region UltraCalcFunctionTrunc
    /// <summary>
    /// Truncates the fractional portion of a numeric value to produce an integer.
    /// </summary>
    /// <remarks>
    /// <p class="body">TRUNC(value, number_of_digits)</p>
    /// <p class="body">Value is a numeric value or reference to a numeric value
    /// that you want to truncate.</p>
    /// <p class="body">Number_of_digits specifies the precision at which truncation
    /// should occur. By default, truncation occurs zero places right of the decimal
    /// point which will produce an integer.</p>
    /// <p class="body">Truncation always discards the fractional value, causing the
    /// <em>Value</em> to move closer to zero (whether it was positive or negative
    /// before the truncation). It differs from the <see cref="UltraCalcFunctionRound">ROUND()</see>
    /// function in that the value always changes to a lesser value.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionTrunc : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int num_digits = 0;

			// Get option
			if (argumentCount == 2) 
			{
				UltraCalcValue numDigits = numberStack.Pop();

				if (numDigits.IsError)
					return new UltraCalcValue( numDigits.ToErrorValue() );

				// MD 4/6/12 - TFS101506
				//num_digits = numDigits.ToInt32(CultureInfo.InvariantCulture);
				num_digits = numDigits.ToInt32();
			}

			double number;
			
			UltraCalcValue numberVal = numberStack.Pop();

			if (numberVal.IsError)
				return new UltraCalcValue( numberVal.ToErrorValue() );

			if (!numberVal.ToDouble(out number))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			
			// SSP 11/8/04 UWC174
			// Old code didn't take into account the negative numbers.
			//
			// ----------------------------------------------------------------------------
			
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			double factor = Math.Pow( 10, num_digits );
			double result = number * factor;
			result = result >= 0 ? Math.Floor( result ) : Math.Ceiling( result );
			result /= factor;

			return new UltraCalcValue( result );
			// ----------------------------------------------------------------------------
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "trunc"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionTrunc

	#region UltraCalcFunctionCount
    /// <summary>
    /// Counts how many cells have numeric or date/time values. 
    /// </summary>
    /// <remarks>
    /// <p class="body">COUNT(Value1, Value2, ..., ValueN)</p>
    /// <p class="body">Value1, value2, ... valueN can be references to different data structures,
    /// such as columns. Each numeric or date/time value is counted. Empty, error, boolean or text
    /// values that are not convertible into numeric values are not counted.</p>
    /// <p class="body">When a reference is a range reference, only those numeric and date/time
    /// values within the range will be counted.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionCount : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			// MD 2/14/11 - TFS66313
			// Moved all code to EvaluateHelper so it could be used in other places.
			//int count = 0;
			//
			//foreach(UltraCalcValue arg in args)
			//{
			//    if (arg.IsBoolean || arg.IsError)
			//        continue;
			//
			//    double temp;
			//
			//    if (arg.ToDouble(out temp))
			//        count++;
			//    else
			//    {
			//        try 
			//        {
			//            arg.ToDateTime();
			//            count ++; 
			//        } 
			//        catch 
			//        {
			//            // Ignore values that couldn't be converted
			//        }
			//    }
			//}
			//
			//return new UltraCalcValue(count);
			return EvaluateHelper(args);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		// MD 2/14/11 - TFS66313
		// Moved this code from Evaluate so it could be used in other places.
		internal static UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			int count = 0;

			foreach (UltraCalcValue arg in args)
			{
				if (arg.IsBoolean || arg.IsError)
					continue;

				double temp;

				if (arg.ToDouble(out temp))
					count++;
				else
				{
					try
					{
						arg.ToDateTime();
						count++;
					}
					catch
					{
						// Ignore values that couldn't be converted
					}
				}
			}

			return new UltraCalcValue(count);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "count"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionCount

	#region UltraCalcFunctionTrue
    /// <summary>
    /// A constant function always returning the TRUE value of Boolean
    /// logic.
    /// </summary>
    /// <remarks>
    /// <p class="body">TRUE() will always evaluate to the boolean value
    /// of TRUE. It takes no arguments.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionTrue : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			return new UltraCalcValue(true);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "true"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionTrue

	#region UltraCalcFunctionFalse
    /// <summary>
    /// A constant function always returning the FALSE value of Boolean
    /// logic.
    /// </summary>
    /// <remarks>
    /// <p class="body">FALSE() will always evaluate to the boolean value
    /// of FALSE. It takes no arguments.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionFalse : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			return new UltraCalcValue(false);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "false"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionFalse

	#region UltraCalcFunctionNot
    /// <summary>
    /// Logical-NOT returns the inverse boolean value of it's argument.
    /// </summary>
    /// <remarks>
    /// <p class="body">NOT(boolean1)</p>
    /// <p class="body">Boolean1 is any boolean (TRUE or FALSE) value, or
    /// conditional statement (made up of any UltraCalc expression which
    /// itself evaluates to a boolean TRUE or FALSE value) to be inverted.</p>
    /// <p class="body">If boolean1 was TRUE, then the Logical-NOT would
    /// return FALSE. If boolean1 was FALSE, then the Logical-NOT would
    /// return TRUE.</p>
    /// <p class="note">If Boolean1 is an UltraCalc expression that evaluates
    /// to an error value, then the result of a Logical-NOT operation is
    /// undefined because an error value is neither TRUE nor FALSE.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionNot : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			return new UltraCalcValue( !value.ToBoolean() );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "not"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionNot

	#region UltraCalcFunctionAnd
    /// <summary>
    /// Logical-AND returns the boolean value, FALSE, if at least one argument has a value of FALSE.
    /// When <em>all</em> arguments are TRUE, then this function returns TRUE.
    /// </summary>
    /// <remarks>
    /// <p class="body">AND(boolean1, boolean2, ...)</p>
    /// <p class="body">Boolean1, boolean2, ... are a list of boolean (TRUE or FALSE) values or
    /// conditional statements (any UltraCalc expression which itself evaluates to a boolean
    /// TRUE or FALSE value) to be evaluated for the constraint that all arguments should be
    /// TRUE (or conversely, that at least one argument should be FALSE.)</p>
    /// <p class="body">All arguments are tested (<em>i.e.</em>, the logical-AND function
    /// does not use "short-circuit" evaluation, in which the function can stop executing
    /// as soon as the first argument having the boolean value, FALSE, has been processed.)</p>
    /// <p class="note">This function stops evaluating immediately when any argument results in
    /// an error value. An error value is neither TRUE nor FALSE, therefore the return value of
    /// the logical-AND function is undefined.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionAnd : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			bool condition = true;
			bool foundLogicalValue = false;

			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);

			foreach(UltraCalcValue arg in args)
			{
				if (arg.IsError)
					return new UltraCalcValue( arg.ToErrorValue() );

				try
				{
					if (!arg.ToBoolean())
						condition = false;

					foundLogicalValue = true;
				}
				catch {}
			}

			if (foundLogicalValue)
				return new UltraCalcValue(condition);
			
			return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "and"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionAnd

	#region UltraCalcFunctionOr
    /// <summary>
    /// Logical-OR returns the boolean value, TRUE, if at least one argument has a value of TRUE.
    /// When <em>all</em> arguments are FALSE, then this function returns FALSE.
    /// </summary>
    /// <remarks>
    /// <p class="body">OR(boolean1, boolean2, ...)</p>
    /// <p class="body">Boolean1, boolean2, ... are a list of boolean (TRUE or FALSE) values or
    /// conditional statements (any UltraCalc expression which itself evaluates to a boolean
    /// TRUE or FALSE value) to be evaluated for the constraint that at least one argument
    /// should be TRUE (or conversely, that all arguments should be FALSE.)</p>
    /// <p class="body">All arguments are tested (<em>i.e.</em>, the logical-OR function
    /// does not use "short-circuit" evaluation, in which the function can stop executing
    /// as soon as the first argument having the boolean value, TRUE, has been processed.)</p>
    /// <p class="note">This function stops evaluating immediately when any argument results in
    /// an error value. An error value is neither TRUE nor FALSE, therefore the return value of
    /// the logical-OR function is undefined.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionOr : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			bool condition = false;
			bool foundLogicalValue = false;

			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);

			foreach(UltraCalcValue arg in args)
			{
				
				if (arg.IsError)
					return new UltraCalcValue( arg.ToErrorValue() );

				try
				{
					if (arg.ToBoolean())
						condition = true;

					foundLogicalValue = true;
				}
				catch {}
			}

			if (foundLogicalValue)
				return new UltraCalcValue(condition);
			
			return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "or"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionOr

	#region UltraCalcFunctionMin
    /// <summary>
    /// Gives you the smallest numeric value out of a series. 
    /// </summary>
    /// <remarks>
    /// <p class="body">MIN(Value1, value2, ..., valueN)</p>
    /// <p class="body">Value1, value2, ... valueN are any number of numeric values or references to
    /// numeric values from which you want the minimum value found. Arguments may be numbers, boolean
    /// values, text values convertible into numbers, or empty. Error values and text values that are
    /// not convertible into numbers will produce an error.</p>
    /// <p class="body">The minimum value for a series of numbers can be it's largest magnitude
    /// negative number because larger magnitude negative numbers are less than smaller magnitude
    /// negative numbers. For example, given the expression MIN( -1500, -50, 5, 150), the return value
    /// is -1500 and not 5. To determine the numeric value with the smallest magnitude you would
    /// use the <see cref="UltraCalcFunctionAbs">ABS()</see> function on each argument. The
    /// following example would produce a minimum value of 5.</p>
    /// <code>MIN( ABS(-1500), ABS(-50), ABS(5), ABS(150))</code>
    /// <p class="body">When the argument list is empty, MIN() returns zero.</p>
    /// <seealso cref="UltraCalcFunctionMax">MAX()</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionMin : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 2/14/11 - TFS66313
			// Moved all code to EvaluateHelper so it could be used in other places.
			//bool foundValue = false;
			//double minimum = 0.0;
			//double temp;
			//UltraCalcValue[] list = this.GetArguments(numberStack, argumentCount, true);
			//
			//foreach(UltraCalcValue calcValue in list)
			//{
			//    if ( null != calcValue )
			//    {
			//        // JJD 9/09/04 - UWC99
			//        // If the value is an error than raise the exception
			//        if ( calcValue.IsError )
			//            return new UltraCalcValue( calcValue.ToErrorValue() );
			//
			//        if (calcValue.ToDouble(out temp))
			//        {
			//            if (!foundValue || temp < minimum)
			//                minimum = temp;
			//
			//            foundValue = true;
			//        }
			//    }
			//}
			//
			//return new UltraCalcValue(minimum);
			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			return EvaluateHelper(args);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		// MD 2/14/11 - TFS66313
		// Moved this code from Evaluate so it could be used in other places.
		internal static UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			bool foundValue = false;
			double minimum = 0.0;
			double temp;

			foreach (UltraCalcValue calcValue in args)
			{
				if (null != calcValue)
				{
					// JJD 9/09/04 - UWC99
					// If the value is an error than raise the exception
					if (calcValue.IsError)
						return new UltraCalcValue(calcValue.ToErrorValue());

					if (calcValue.ToDouble(out temp))
					{
						if (!foundValue || temp < minimum)
							minimum = temp;

						foundValue = true;
					}
				}
			}

			return new UltraCalcValue(minimum);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "min"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionMin

	#region UltraCalcFunctionMax
    /// <summary>
    /// Gives you the largest numeric value out of a series. 
    /// </summary>
    /// <remarks>
    /// <p class="body">MAX(Value1, value2, ..., valueN)</p>
    /// <p class="body">Value1, value2, ... valueN are any number of numeric values or references to numeric
    /// values from which you want the maximum value found. Arguments may be numbers, boolean
    /// values, text values convertible into numbers, or empty. Error values and text values
    /// that are not convertible into numbers will produce an error.</p>
    /// <p class="body">When the argument list is empty, MAX() returns zero.</p>
    /// <seealso cref="UltraCalcFunctionMin">MIN()</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionMax : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 2/14/11 - TFS66313
			// Moved all code to EvaluateHelper so it could be used in other places.
			//bool foundValue = false;
			//double maximum = 0.0;
			//double temp;
			//UltraCalcValue[] list = this.GetArguments(numberStack, argumentCount, true);
			//
			//foreach(UltraCalcValue calcValue in list)
			//{
			//    if ( null != calcValue )
			//    {
			//        // JJD 9/09/04 - UWC99
			//        // If the value is an error than raise the exception
			//        if ( calcValue.IsError )
			//            return new UltraCalcValue( calcValue.ToErrorValue() );
			//
			//        if (calcValue.ToDouble(out temp))
			//        {
			//            if (!foundValue || temp > maximum)
			//                maximum = temp;
			//
			//            foundValue = true;
			//        }
			//    }
			//}
			//
			//return new UltraCalcValue(maximum);
			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			return EvaluateHelper(args);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		// MD 2/14/11 - TFS66313
		// Moved this code from Evaluate so it could be used in other places.
		internal static UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			bool foundValue = false;
			double maximum = 0.0;
			double temp;

			foreach (UltraCalcValue calcValue in args)
			{
				if (null != calcValue)
				{
					// JJD 9/09/04 - UWC99
					// If the value is an error than raise the exception
					if (calcValue.IsError)
						return new UltraCalcValue(calcValue.ToErrorValue());

					if (calcValue.ToDouble(out temp))
					{
						if (!foundValue || temp > maximum)
							maximum = temp;

						foundValue = true;
					}
				}
			}

			return new UltraCalcValue(maximum);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "max"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionMax

	#region UltraCalcFunctionExp
    /// <summary>
    /// Calculates the mathematical constant, e, raised to the specified power.
    /// </summary>
    /// <remarks>
    /// <p class="body">EXP(exponent)</p>
    /// <p class="body">Exponent is the power to which the base, e, is raised.
    /// This gives the function it's characteristic "exponential" growth.</p>
    /// <p class="body">Euler's Number, e, is the value 2.718281828459..., and
    /// is also the base of the natural logarithm. You can represent the constant
    /// e within your UltraCalc formula by specifying EXP(1). The exponential
    /// function (and natural logarithm) have many applications in mathematics,
    /// engineering, and for modeling behavioral and statistical distributions 
    /// commonly observed in nature and the social sciences.</p>
    /// <seealso cref="UltraCalcFunctionLn">LN()</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionExp : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
			{
				// MD 2/7/11
				// We were returning the incorrect error code here.
				//return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
			}

			// MD 2/7/11
			// Excel returns errors with certain numbers, so we should do the same when using excel formula compatibility.
			//return new UltraCalcValue( Math.Exp(temp) );
			double result = Math.Exp(temp);

			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if (UltraCalcValue.UseExcelFormulaCompatibility && Double.IsInfinity(result))
			if ( UltraCalcValue.UseExcelFunctionCompatibility && Double.IsInfinity( result ) )
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			return new UltraCalcValue(result);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "exp"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionExp

	#region UltraCalcFunctionRound
    /// <summary>
    /// Rounds the fractional portion of a numeric value up or down to produce an
    /// integer.
    /// </summary>
    /// <remarks>
    /// <p class="body">ROUND(value, number_of_digits)</p>
    /// <p class="body">Value is a numeric value or reference to a numeric value
    /// that you want to round up or down.</p>
    /// <p class="body">Number_of_digits specifies the precision at which rounding
    /// should occur. This will be the place value which UltraCalc examines to round
    /// up or down. By default, rounding occurs zero places right of the decimal point
    /// produces an integer.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionRound : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MRS 8/8/05 - BR05144
			// Added an optional RoundStyle param to specify arithmetic (default)
			// or bankers rounding.
			//------------------------------------
			UltraCalcValue roundStyle = null;
			if (argumentCount == 3)
				roundStyle= numberStack.Pop();
			//------------------------------------

			UltraCalcValue num_digits = numberStack.Pop();
			UltraCalcValue number = numberStack.Pop();

			if (num_digits.IsError)
				return new UltraCalcValue( num_digits.ToErrorValue() );

			if (number.IsError)
				return new UltraCalcValue( number.ToErrorValue() );

			// MRS 8/8/05 - BR05144
			// Added an optional RoundStyle param to specify arithmetic (default)
			// or bankers rounding.
			//------------------------------------
			if (roundStyle != null &&
				roundStyle.IsError)
			{
				return new UltraCalcValue( roundStyle.ToErrorValue() );
			}
			//------------------------------------

			// AS 9/8/04 UWC28
			//numberStack.Push(new UltraCalcValue( Math.Round(number.ToDouble(CultureInfo.InvariantCulture),num_digits.ToInt32(CultureInfo.InvariantCulture))));
			double power;

			if (!num_digits.ToDouble(out power))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// JAS 12/22/04 BR01396 - Moved this definition to accomodate for the new Round() method.
			//double factor = Math.Pow(10, (int)power);
			double d;
			if (!number.ToDouble(out d))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// MD 6/7/11 - TFS78166
			// Excel tries to correct some round off errors in the display values of cells, but they will also fix them in the rounding functions, since
			// they could inadvertantly truncate a value.
			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if (UltraCalcValue.UseExcelFormulaCompatibility)
			if ( UltraCalcValue.UseExcelFunctionCompatibility )
				d = MathUtilities.RoundToExcelDisplayValue(d);

			// JAS 12/22/04 BR01396 - Moved this logic to the static Round method so that the FIXED function
			// can share it.
			//
//			// SSP 11/8/04 UWC172
//			//
//			// ----------------------------------------------------------------------------------
//			//double result = Math.Floor( (d * factor) + (0.5d * Math.Sign(d))) / factor;
//			double factor = Math.Pow(10, (int)power);
//			double result = d * factor + 0.5d * Math.Sign( d );
//
//			if ( d >= 0.0 )
//				result = Math.Floor( result );
//			else
//				result = Math.Ceiling( result );
//
//			result /= factor;
//			// ----------------------------------------------------------------------------------

			// MRS 8/8/05 - BR05144
			// Added an optional RoundStyle param to specify arithmetic (default)
			// or bankers rounding.
			//------------------------------------
			double rs = 0.0;
			if (roundStyle != null)
			{
				if (!roundStyle.ToDouble(out rs))
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

				if(	(int)rs < 0 ||
					(int)rs > 1)
				{
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
				}
			}
			//------------------------------------

			// JAS 3/18/05 BR02707
//			return new UltraCalcValue(result);
//			return new UltraCalcValue( UltraCalcFunctionRound.Round( (decimal)d, (int)power ) );
			// MRS 8/8/05 - BR05144
			// Added an optional RoundStyle param to specify arithmetic (default)
			// or bankers rounding.
			//return new UltraCalcValue( UltraCalcFunctionRound.Round( d, (int)power ) );
			return new UltraCalcValue( UltraCalcFunctionRound.Round( d, (int)power , (int)rs) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "round"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			// MRS 8/8/05 - BR05144
			//get { return 2; }
			get { return 3; }
		}
		#endregion //Min/Max args

		// JAS 12/22/04 BR01396
		#region Round

		// MRS 8/8/05 - BR05144
		// Added an optional RoundStyle param to specify arithmetic (default)
		// or bankers rounding.
		internal static double Round( double number, int power )
		{
			return UltraCalcFunctionRound.Round(number, power, 0);
		}

		// JAS 3/18/05 BR02707 - Changed the 'number' param from a Decimal to a Double.
		//
		// MRS 8/8/05 - BR05144
		// Added an optional RoundStyle param to specify arithmetic (default)
		// or bankers rounding.
		//internal static double Round( double number, int power )
		internal static double Round( double number, int power, int roundStyle )
		{			
			double factor = Math.Pow( 10, power );
			// MRS 8/8/05 - BR05144
			// Added an optional RoundStyle param to specify arithmetic (default)
			// or bankers rounding.
			//double result = (number * factor) + (0.5 * Math.Sign( number ));
			double additive = roundStyle == 0 ? 0.5 : 0;
			double result = (number * factor) + (additive * Math.Sign( number ));

			// MRS 8/8/05 - BR05144
			// Added an optional RoundStyle param to specify arithmetic (default)
			// or bankers rounding.
//			if ( number >= 0.0 )
//				result = Math.Floor( result );
//			else
//				result = Math.Ceiling( result );
			if (roundStyle == 1)
				result = Math.Round(result, 0);
			else
			{
				if ( number >= 0.0 )
					result = Math.Floor( result );
				else
					result = Math.Ceiling( result );
			}

			result /= factor;

			return result;
		}

		#region Old Round

		//		internal static decimal Round( decimal number, int power )
		//		{
		//			decimal factor = (decimal)Math.Pow( 10, power );
		//			decimal result = (number * factor) + (0.5m * Math.Sign( number ));
		//
		//			if ( number >= 0.0m )
		//				result = Decimal.Floor( result );
		//			else
		//				// I know that it is dumb to get a double and then cast it to a decimal,
		//				// but for some reason Decimal does not offer a Ceiling method!
		//				result = (decimal)Math.Ceiling( (double)result );
		//
		//			result /= (decimal)factor;
		//
		//			return result;
		//		}

		#endregion // Old Round

		#endregion // Round



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionRound

	#region UltraCalcFunctionPi
    /// <summary>
    /// Returns the mathematical constant for the Greek letter, pi.
    /// </summary>
    /// <remarks>
    /// <p class="body">PI()</p>
    /// <p class="body">The mathematical constant pi represents the relationship
    /// between a circle's diameter and it's circumference. It is also the constant
    /// relating the square of a circle's radius with the surface area of the circle.
    /// It has a great many applications in mathematics, geometry, the sciences, and
    /// engineering.</p>
    /// <p class="body">The constant pi is a non-terminating decimal number, although
    /// UltraCalc approximates it to 15 significant digits of precision (3.14159265358979).</p>
    /// <p class="body">A common use of the PI function in UltraCalc is to convert
    /// a measurement in radians which is the form in which trigonometric functions
    /// take their arguments, and the more conventional degrees of arc. This conversion
    /// can be accomplished by multiplying the radian measurement by PI()/180. The
    /// following table describes some benchmarks for comparison between radians and
    /// degrees.</p>
    /// <table border="0">
    /// <thead>
    /// <th>Radians</th><th>Degrees</th><th>Turns of a wheel (common example)</th>
    /// </thead>
    /// <tbody>
    /// <tr><td>0</td><td>0</td><td>no turns</td></tr>
    /// <tr><td>PI/4</td><td>45</td><td>one-eighth turn</td></tr>
    /// <tr><td>PI/2</td><td>90</td><td>one-quarter turn</td></tr>
    /// <tr><td>PI</td><td>180</td><td>one-half turn</td></tr>
    /// <tr><td>2x PI</td><td>360</td><td>one complete turn</td></tr>
    /// <tr><td>3x PI</td><td>540</td><td>one and one-half turn</td></tr>
    /// </tbody>
    /// </table>
    /// </remarks>

	internal





        class UltraCalcFunctionPi : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			return new UltraCalcValue(3.14159265358979);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "pi"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionPi

	#region UltraCalcFunctionSqrt
    /// <summary>
    /// Calculates the square root of a number.
    /// </summary>
    /// <remarks>
    /// <p class="body">SQRT(value)</p>
    /// <p class="body">Value is the numeric value or reference to a numeric
    /// value which you are calculating the square root of. Only real roots
    /// are supported, therefore if <em>value</em> is negative (which would
    /// produce an imaginary root) the SQRT() function will return an error
    /// value (#NUM!).</p>
    /// <p class="body">The square root is the number whose product, when
    /// the number is multiplied against itself (squared), is <em>value</em>.
    /// Note that it is possible to multiply two negative square roots to
    /// produce the same positive <em>value</em>. By convention, the SQRT()
    /// function only returns the positive root.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionSqrt : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double d;

			if (!value.ToDouble(out d) || d < 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue(Math.Sqrt(d));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "sqrt"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSqrt

	#region UltraCalcFunctionCos
    /// <summary>
    /// Calculates the trigonometric cosine of a specified angle (measured in
    /// radians).
    /// </summary>
    /// <remarks>
    /// <p class="body">COS(value)</p>
    /// <p class="body">Value is the angle measured in radians for which you want
    /// to take the cosine. If your argument must be in degrees, multiply it by
    /// PI()/180 to convert it into radians.</p>
    /// <p class="body">The cosine is an sinusoidal function with a period of 2*PI()
    /// radians. It's value is always between 1 and -1. It behaves like the sine function,
    /// <see cref="UltraCalcFunctionSin">SIN()</see>, with a phase shift (phi) of -PI()/4
    /// radians.</p>
    /// <p class="body">The cosine function value derives from the geometric ratio
    /// between the length of the adjacent (non-hypotenuse) leg to the hypotenuse of
    /// a right triangle, when the hypotenuse has a length of one. It is continuous
    /// because at intervals of PI() radians the length of the adjacent leg and the
    /// hypotenuse are equal.</p>
    /// <p class="body">Another way of looking at this function is to imagine the
    /// hypotenuse is the radius, r, of a unit circle centered on a Cartesian plane
    /// with x- and y- axes.  At any point on the edge of the unit circle, the
    /// hypotenuse makes an angle, theta, with the x-axis.  The cosine function
    /// value of the angle theta is the distance from the center of the circle
    /// to the point in the direction of the x-axis.  Applied in this fashion,
    /// the COS() function can be used to convert between Cartesian and Polar
    /// coordinate systems.</p>
    /// <p class="body">Both interpretations are equivalent because if you drop
    /// a line from a point on the unit circle that intersects the x-axis at a
    /// right angle you form a right triangle.</p>
    /// <p class="body">These trigonometric concepts appear frequently in many
    /// engineering, architectural and scientific applications.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionCos : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Cos(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "cos"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionCos

	#region UltraCalcFunctionACos
    /// <summary>
    /// Returns the angle (measured in radians) having the specified value of the trigonometric cosine function.
    /// </summary>
    /// <remarks>
    /// <p class="body">ACOS(value)</p>
    /// <p class="body">Value is the real number result of the cosine function taken of an angle you want
    /// to find. The legal values of the cosine function are from -1 to 1. An inverse cosine
    /// by default will return the normal angle from 0 up to PI. This is because there are
    /// several angles which may have an identical value of their cosine. For example, the
    /// angles -PI/2, PI/2, 3PI/2, etc., all have a cosine of zero. You can find alternate
    /// angles by adding (or subtracting) any multiple of PI radians to the normal angle
    /// returned by the inverse cosine function.</p>
    /// <p class="body">You can convert the result of the inverse cosine function from
    /// radians into degrees by multiplying it by 180/PI().</p>
    /// </remarks>

	internal





        class UltraCalcFunctionACos : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Acos(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "acos"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionACos

	#region UltraCalcFunctionCosh
    /// <summary>
    /// Returns the angle (measured in radians) having the specified value of it's hyperbolic cosine function.
    /// </summary>
    /// <remarks>
    /// <p class="body">ACOSH(value)</p>
    /// <p class="body">Value is the hyperbolic cosine of some angle (measured in radians) that you want
    /// to find. As you might expect, the values of the hyperbolic cosine function (see the <see cref="UltraCalcFunctionCosh">COSH</see> function)
    /// increase at a hyperbolic rate, but one consequence is that these ever larger changes in the hyperbolic
    /// cosine will correspond to ever smaller changes in the angle. This relationship is intrinsic to many of
    /// the mathematical and engineering applications of the inverse hyperbolic cosine function, such as when
    /// resistance or strain builds up on a body increasingly as it is rotated (hysteresis).</p>
    /// </remarks>

	internal





        class UltraCalcFunctionCosh : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Cosh(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "cosh"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionCosh

	#region UltraCalcFunctionSin
    /// <summary>
    /// Calculates the trigonometric sine of a specified angle (measured in
    /// radians).
    /// </summary>
    /// <remarks>
    /// <p class="body">SIN(value)</p>
    /// <p class="body">Value is the angle measured in radians for which you want
    /// to take the cosine. If your argument must be in degrees, multiply it by
    /// PI()/180 to convert it into radians.</p>
    /// <p class="body">The sine is an sinusoidal function with a period of 2*PI()
    /// radians. It's value is always between 1 and -1.</p>
    /// <p class="body">The sine function value derives from the geometric ratio
    /// between the length of a right triangle's hypotenuse and the length of the
    /// opposite leg, when the hypotenuse has a length of one.  It is a continuous
    /// function because at intervals of PI() radians the length of the opposite
    /// leg and the hypotenuse are equal.</p>
    /// <p class="body">Another way of looking at this function is to imagine the
    /// hypotenuse is the radius, r, of a unit circle centered on a Cartesian plane
    /// with x- and y- axes.  At any point on the edge of the unit circle, the
    /// hypotenuse makes an angle, theta, with the x-axis.  The sine function
    /// value of the angle theta is the distance from the center of the circle
    /// to the point in the direction of the y-axis.  Applied in this fashion,
    /// the SIN() function can be used to convert between Cartesian and Polar
    /// coordinate systems.</p>
    /// <p class="body">Both interpretations are equivalent because if you drop a
    /// line from a point on the unit circle that intersects the y-axis at a right
    /// angle you form a right triangle.</p>
    /// <p class="body">These trigonometric concepts appear frequently in many
    /// engineering, architectural and scientific applications.</p>
    /// <seealso cref="UltraCalcFunctionCos">COS()</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionSin : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Sin(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "sin"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSin

	#region UltraCalcFunctionSinh
    /// <summary>
    /// Calculates the hyperbolic sine of a specified angle measured in radians.
    /// </summary>
    /// <remarks>
    /// <p class="body">SINH(value)</p>
    /// <p class="body">Value is an angle measured in radians for which you want to
    /// calculate the hyperbolic sine. If your angle is measured in degrees, 
    /// multiply it by PI()/180 to convert into radians. </p>
    /// <p class="body">Many applications in mathematics and physics, for example
    /// determining the gravitational potential of a cylinder, make use of the
    /// hyperbolic sine function's characteristics.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionSinh : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Sinh(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "sinh"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSinh

	#region UltraCalcFunctionASin
    /// <summary>
    /// Returns the angle (measured in radians) having the specified value of the trigonometric sine function.
    /// </summary>
    /// <remarks>
    /// <p class="body">ASIN(value)</p>
    /// <p class="body">Value is the sine value of the angle you want. Legal sine values are
    /// confined to real numbers from -1 to 1, inclusive.</p>
    /// <p class="body">The angle returned will be between -PI()/2 and PI()/2 radians. To convert
    /// this angle into degrees, multiply by PI()/180.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionASin : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Asin(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "asin"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionASin

	#region UltraCalcFunctionTan
    /// <summary>
    /// Calculates the trigonometric tangent of a specified angle (measured in
    /// radians).
    /// </summary>
    /// <remarks>
    /// <p class="body">TAN(value)</p>
    /// <p class="body">Value is the angle measured in radians for which you want
    /// to take the tangent. If your argument must be in degrees, multiply it by
    /// PI()/180 to convert it into radians.</p>
    /// <p class="body">The tangent can have a value from -INF to +INF, however it
    /// is undefined at intervals of every PI()/2 radians +/- PI() radians.</p>
    /// <p class="body">The tangent function value derives from a well-known geometric
    /// ratio between the length of the opposite and the adjacent (non-hypotenuse) leg
    /// of a right triangle. All 3 angles inside of any triangle must add up to exactly
    /// PI() radians, and in a right triangle the angle opposite the hypotenuse must be
    /// PI()/2 radians. Given these facts, it is not possible for either of the other
    /// two angles within the triangle to reach PI()/2 radians themselves. If you choose
    /// an angle (other than the triangle's right angle) which approaches PI()/2 radians
    /// then the remaining angle must approach 0 radians. The tangent function value tells
    /// you in these circumstances how the leg of the right triangle opposite your angle
    /// approaches infinite length. These calculations have important applications in
    /// architecture and engineering.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionTan : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Tan(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "tan"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSinh

	#region UltraCalcFunctionTanh
    /// <summary>
    /// Calculates the hyperbolic tangent of a specified angle measured in radians.
    /// </summary>
    /// <remarks>
    /// <p class="body">TANH(value)</p>
    /// <p class="body">Value is an angle measured in radians for which you want to
    /// calculate the hyperbolic tangent. If your angle is measured in degrees, 
    /// multiply it by PI()/180 to convert into radians. The hyperbolic tangent 
    /// has a range from -1 to 1.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionTanh : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Tanh(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "tanh"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSinh

	#region UltraCalcFunctionATan
    /// <summary>
    /// Calculates the normalized angle (measured in radians) which has the
    /// specified tangent function value.
    /// </summary>
    /// <remarks>
    /// <p class="body">ATAN(value)</p>
    /// <p class="body">Value is a number containing a tangent function value
    /// that you want to retrieve the angle of. This function returns an angle
    /// of between PI()/2 and -PI()/2. Although the tangent function is undefined
    /// for values of PI()/2 and -PI()/2, the return value of this function is
    /// rounded to these values if <em>Value</em> is +INF or -INF, respectively.</p>
    /// <p class="body">This function is sometimes referred to as the inverse
    /// tangent function or the arctangent.</p>
    /// <p class="body">The angle returned is the principal value, as
    /// there exist an uncountable number of alternative angles satisfying
    /// the requirement of having <em>Value</em> as their tangent function
    /// value, at regular intervals of +/- PI() to either side of this
    /// principal value.</p>
    /// <p class="body">If you require a result in degrees, multiply the
    /// arctangent by 180/PI().</p>
    /// </remarks>

	internal





        class UltraCalcFunctionATan : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( Math.Atan(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "atan"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionATan

	#region UltraCalcFunctionATan2
    /// <summary>
    /// Calculates the angle made with the x-axis on a Cartesian coordinate
    /// plane by the specified (x, y) coordinates.
    /// </summary>
    /// <remarks>
    /// <p class="body">ATAN2(x_ordinate, y_abscissa)</p>
    /// <p class="body">X_ordinate is a number representing the distance along
    /// the x-axis of a Cartesian point. It may also be thought of as the
    /// adjacent leg of a right triangle, where the right angle is made by
    /// dropping a perpendicular line from (<em>x_ordinate</em>, <em>y_abscissa</em>)
    /// to the point (<em>x_ordinate</em>, 0) on the x-axis.</p>
    /// <p class="body">Y_abscissa is a number representing the distance along
    /// the y-axis of a Cartesian point. It may also be thought of as the
    /// opposite leg of the right triangle constructed above.</p>
    /// <p class="body">This variation on the arctangent (or inverse tangent function)
    /// calculates for you the angle, theta, made with the x-axis when you extend a
    /// line segment from the origin at (0, 0) to your specified coordinates at
    /// (<em>x_ordinate</em>, <em>y_abscissa</em>). This line segment can also be
    /// seen to be the hypotenuse of a right triangle, or the radius of the circle
    /// on which the point, (<em>x_ordinate</em>, <em>y_abscissa</em>), sits on
    /// the edge.  This function is another way of looking at the trigonometric
    /// tangent function value that is useful in many UltraCalc applications
    /// because it affords you a more convenient parameterization for some tasks,
    /// such as converting from Cartesian to Polar coordinate systems.</p>
    /// <p class="body">The return value of this function is within the range of
    /// PI()/2 and -PI()/2 measured in radians. If you need to convert this value
    /// into degrees then multiply the result by 180/PI().</p>
    /// <seealso cref="UltraCalcFunctionATan">ATAN()</seealso>
    /// <seealso cref="UltraCalcFunctionTan">TAN()</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionATan2 : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue yValue = numberStack.Pop();
			UltraCalcValue xValue = numberStack.Pop();

			if (xValue.IsError)
				return new UltraCalcValue( xValue.ToErrorValue() );

			if (yValue.IsError)
				return new UltraCalcValue( yValue.ToErrorValue() );

			double x, y;

			if (!yValue.ToDouble(out y) || !xValue.ToDouble(out x))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			if (x == 0 && y == 0) 
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Div));

			return new UltraCalcValue(Math.Atan2(y, x));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "atan2"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionATan2

	#region UltraCalcFunctionFloor
    /// <summary>
    /// Calculates the next lesser whole number for a specified numeric
    /// value.
    /// </summary>
    /// <remarks>
    /// <p class="body">FLOOR(value)</p>
    /// <p class="body">Value is a real numeric value to be rounded down
    /// to the next lesser whole number (also called an integer). FLOOR()
    /// behaves differently from the <see cref="UltraCalcFunctionInt">INT()</see>
    /// function because when <em>value</em> is negative, the "next lesser
    /// whole number" will be a number having greater magnitude (i.e., it
    /// becomes more negative.)</p>
    /// </remarks>

	internal





        class UltraCalcFunctionFloor : BuiltInFunctionBase
	{

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
            // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
            // Added a second, optional parameter for the Significance to match Excel
            // ----------------------------------------------------------------
            double significance = 1;

            if (argumentCount > 1)
            {
                UltraCalcValue significanceValue = numberStack.Pop();

                if (significanceValue.IsError)
                    return new UltraCalcValue(significanceValue.ToErrorValue());

                if (!significanceValue.ToDouble(out significance))
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
            }
            // ----------------------------------------------------------------

			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

            // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
            // Added a second, optional parameter for the Significance to match Excel
            //return new UltraCalcValue( Math.Floor(temp) ); 
            // ----------------------------------------------------------------
            if (temp == 0 && significance == 0)
                return new UltraCalcValue(0);

            if (significance == 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Div));

            int valueSign = temp < 0 ? -1 : 1;
            int significanceSign = significance < 0 ? -1 : 1;

			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
            //if (UltraCalcValue.UseExcelFormulaCompatibility)
			if ( UltraCalcValue.UseExcelFunctionCompatibility )
            {
				if (temp != 0 && significance != 0 &&
					// MD 8/25/11
					// Found while fixing TFS81868
					// We should only return an error when the value is positive and the significance is negative. 
					// It is allowed for the value to be negative and the significance to be positive.
					//valueSign != significanceSign)
					0 < valueSign && significanceSign < 0)
                {
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
                }
            }

			// MD 8/25/11
			// Found while fixing TFS81868
			// This doesn't seem to be needed and causes a problem when the value is negative and the significance is positive.
			//if (UltraCalcValue.UseExcelFormulaCompatibility)
			//{
			//    temp = Math.Abs(temp);
			//    significance = Math.Abs(significance);
			//}

			// MD 8/25/11 - TFS81868
			// Due to rounding errors when taking a mod, this can be wrong. A better way is to divide the value by the significance,
			// get rid of any round-off errors, take the floor of that value, and multiply the significance back in.
			//double mod = (temp % significance);
			//double dividend = ((temp - mod) / significance);
			//double floor = dividend * significance;
			//if (mod < 0)
			//    floor -= significance;
			double floor = Math.Floor(MathUtilities.RoundToExcelDisplayValue(temp / significance)) * significance;

			// MD 8/25/11
			// Found while fixing TFS81868
			// This doesn't seem to be needed and causes a problem when the value is negative and the significance is positive.
			//if (UltraCalcValue.UseExcelFormulaCompatibility)
			//    floor *= valueSign;

            return new UltraCalcValue(floor);
            // ----------------------------------------------------------------			
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "floor"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{
            get
            {
                // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
                //return 1; 
                return 2;
            }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionFloor

	#region UltraCalcFunctionCeiling
    /// <summary>
    /// Returns the smallest whole number greater than or equal to the given number
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// CEILING(number)
    /// <p></p>
    /// Number   is the numeric value you want to round.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionCeiling : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
            // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
            // Added a second, optional parameter for the Significance to match Excel
            // ----------------------------------------------------------------
            double significance = 1;

            if (argumentCount > 1)
            {
                UltraCalcValue significanceValue = numberStack.Pop();

                if (significanceValue.IsError)
                    return new UltraCalcValue(significanceValue.ToErrorValue());

                if (!significanceValue.ToDouble(out significance))
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
            }
            // ----------------------------------------------------------------

			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );            

            // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
            // Added a second, optional parameter for the Significance to match Excel
            //return new UltraCalcValue( Math.Ceiling(temp) ); 
            // ----------------------------------------------------------------
            if (significance == 0)
                return new UltraCalcValue(0);

            int valueSign = temp < 0 ? -1 : 1;
            int significanceSign = significance < 0 ? -1 : 1;

			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
            //if (UltraCalcValue.UseExcelFormulaCompatibility)
			if ( UltraCalcValue.UseExcelFunctionCompatibility )
            {
                if (temp != 0 && significance != 0 &&
					// MD 8/25/11
					// Found while fixing TFS81868
					// We should only return an error when the value is positive and the significance is negative. 
					// It is allowed for the value to be negative and the significance to be positive.
					//valueSign != significanceSign)
					0 < valueSign && significanceSign < 0)
                {
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
                }
            }

			// MD 8/25/11
			// Found while fixing TFS81868
			// This doesn't seem to be needed and causes a problem when the value is negative and the significance is positive.
			//if (UltraCalcValue.UseExcelFormulaCompatibility)
			//{
			//    temp = Math.Abs(temp);
			//    significance = Math.Abs(significance);
			//}

			// MD 8/25/11 - TFS81868
			// Due to rounding errors when taking a mod, this can be wrong. A better way is to divide the value by the significance,
			// get rid of any round-off errors, take the ceiling of that value, and multiply the significance back in.
			//double mod = (temp % significance);
			//double dividend = ((temp - mod)/ significance);
			//double ceiling = dividend * significance;
			//if (mod > 0)
			//    ceiling += (significance);
			double ceiling = Math.Ceiling(MathUtilities.RoundToExcelDisplayValue(temp / significance)) * significance;

			// MD 8/25/11
			// Found while fixing TFS81868
			// This doesn't seem to be needed and causes a problem when the value is negative and the significance is positive.
			//if (UltraCalcValue.UseExcelFormulaCompatibility)
			//    ceiling *= significanceSign;
            
            return new UltraCalcValue(ceiling);
            // ----------------------------------------------------------------
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "ceiling"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get 
            {
                // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
                //return 1; 
                return 2; 
            }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionCeiling

	#region UltraCalcFunctionRand
    /// <summary>
    /// Generates a pseudorandom number from zero up to (but not including) one.
    /// </summary>
    /// <remarks>
    /// <p class="body">RAND()</p>
    /// <p>The RAND() function will generate another pseudorandom number each time it
    /// is evaluated. It returns a fractional number on a unit basis, therefore
    /// you can use RAND() to produce numbers between 0 and <em>C</em> by multiplying
    /// the RAND() result by <em>C</em>. It follows that to produce a pseudorandom
    /// number between <em>A</em> and <em>B</em> you could translate the result by
    /// <em>A</em> like this:</p>
    /// <code>RAND() * ( [B] - [A] ) + [A]</code>
    /// <p class="body">The number generation of the RAND() function derives from the
    /// system clock, and therefore may not be entirely random. It should not be used
    /// for applications requiring cryptographically-strong randomness or uniform
    /// probability distributions.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionRand : BuiltInFunctionBase
	{
		#region Member Variables

		// AS 9/8/04 UWC54
		private Random random = new Random();

		#endregion //Member Variables

		#region Properties


		#region IsAlwaysDirty
		/// <summary>
		/// Indicates whether the results of the function is always dirty.
		/// </summary>
		public override bool IsAlwaysDirty
		{
			get { return true; }
		}
		#endregion //IsAlwaysDirty 


		#endregion //Properties

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// AS 9/8/04 UWC54
			//numberStack.Push(new UltraCalcValue(new Random().NextDouble()));
			return new UltraCalcValue( this.random.NextDouble() );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "rand"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionRand

	#region UltraCalcFunctionLn
    /// <summary>
    /// Calculates the natural logarithm of a specified numeric value.
    /// </summary>
    /// <remarks>
    /// <p class="body">LN(value)</p>
    /// <p class="body">Value is a real number to calculate the natural
    /// logarithm of. <em>Value</em> must be positive as the logarithm
    /// is undefined for negative values.</p>
    /// <p class="body">A natural logarithm is a special case of a logarithm
    /// having the base of Euler's Number, e (2.71828...). It is the inverse
    /// of the <see cref="UltraCalcFunctionExp">EXP()</see> function.</p>
    /// <p class="body">Logarithms have many applications in mathematics,
    /// life and social sciences.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionLn : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
			{
				// MD 2/7/11
				// We were returning the incorrect error code here.
				//return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
			}

			// MD 2/7/11
			// Excel returns errors with certain numbers, so we should do the same when using excel formula compatibility.
			//return new UltraCalcValue( Math.Log(temp) );
			double result = Math.Log(temp);

			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if (UltraCalcValue.UseExcelFormulaCompatibility && Double.IsInfinity(result))
			if ( UltraCalcValue.UseExcelFunctionCompatibility && Double.IsInfinity( result ) )
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			return new UltraCalcValue(result);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "ln"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionLn

	#region UltraCalcFunctionLog10
    /// <summary>
    /// Calculates the logarithm (base 10) of a specified numeric value.
    /// </summary>
    /// <remarks>
    /// <p class="body">LOG10(value)</p>
    /// <p class="body">Value is a real number to calculate the decimal
    /// logarithm of. <em>Value</em> must be positive as the logarithm
    /// is undefined for negative values.</p>
    /// <p class="body">To specify your own base for a logarithm use the
    /// <see cref="UltraCalcFunctionLog">LOG()</see> function. To calculate
    /// the natural logarithm use the <see cref="UltraCalcFunctionLn">LN()</see>
    /// function.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionLog10 : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double temp;

			if (!value.ToDouble(out temp))
			{
				// MD 2/7/11
				// We were returning the incorrect error code here.
				//return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
			}

			// MD 2/7/11
			// Excel returns errors with certain numbers, so we should do the same when using excel formula compatibility.
			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if (UltraCalcValue.UseExcelFormulaCompatibility && temp == 0)
			if ( UltraCalcValue.UseExcelFunctionCompatibility && temp == 0 )
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			return new UltraCalcValue( Math.Log10(temp) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "log10"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionLog10

	#region UltraCalcFunctionLog
    /// <summary>
    /// Calculates a logarithm for a specified numeric value to the
    /// specified base.
    /// </summary>
    /// <remarks>
    /// <p class="body">LOG(value, [base])</p>
    /// <p class="body">Value is a real number that you want to calculate the logarithm
    /// to <em>base</em> for. This number must be positive, as the logarithm
    /// is undefined for negative numbers.</p>
    /// <p class="body">Base is the base of the logarithm, which defaults to 10.</p>
    /// <p class="body">To calculate the natural logarithm (a logarithm to the base
    /// of Euler's Number, e) it is usually more convenient to call the <see cref="UltraCalcFunctionLn">LN()</see>
    /// function.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionLog : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 4/10/12
			// Found while fixing TFS108678
			//UltraCalcValue logBaseValue = numberStack.Pop();
			UltraCalcValue logBaseValue = null;
			if (argumentCount == 2)
				logBaseValue = numberStack.Pop();

			UltraCalcValue numberValue = numberStack.Pop();

			if (numberValue.IsError)
				return new UltraCalcValue( numberValue.ToErrorValue() );

			// MD 4/10/12
			// Found while fixing TFS108678
			//if (logBaseValue.IsError)
			if (logBaseValue != null && logBaseValue.IsError)
				return new UltraCalcValue( logBaseValue.ToErrorValue() );

			double logBase, number;

			// MD 4/10/12
			// Found while fixing TFS108678
			//if (!logBaseValue.ToDouble(out logBase) || !numberValue.ToDouble(out number))
			//{
			//    // MD 2/7/11
			//    // We were returning the incorrect error code here.
			//    //return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			//    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
			//}
			if (logBaseValue != null)
			{
				if (logBaseValue.ToDouble(out logBase) == false)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
			}
			else
			{
				logBase = 10;
			}

			if (numberValue.ToDouble(out number) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			// MD 2/7/11
			// Excel returns errors with certain numbers, so we should do the same when using excel formula compatibility.
			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if (UltraCalcValue.UseExcelFormulaCompatibility && number == 0)
			if ( UltraCalcValue.UseExcelFunctionCompatibility && number == 0 )
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			return new UltraCalcValue( Math.Log(number, logBase) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "log"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			// MD 4/10/12
			// Found while fixing TFS108678
			// The base argument is optional
			//get { return 2; }
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionLog

	#region UltraCalcFunctionNPV
    /// <summary>
    /// Calculates an investment's Net Present Value given it's expected rate or return and the cash flow
    /// represented as positive numeric values (income) and negative numeric values (payments).
    /// </summary>
    /// <remarks>
    /// <p class="body">NPV(discountRate, value1, value2, ..., valueN)</p>
    /// <p class="body">DiscountRate is the investment's expected rate of return over the life of the
    /// investment, expressed as a percentage growth (or decline) per payment period. In some applications
    /// this may be the fixed interest rate. This numeric value is used to discount cash flows paid into or
    /// received from the investment.</p>
    /// <p class="body">Value1, value2, ... valueN are any number of numeric values representing cash inflows
    /// (as positive numbers) or cash outflows (as negative numbers). These cash flows must occur at a fixed
    /// period (the same period at which the <em>discountRate</em> is expressed), although some cash flows
    /// may be zero. The order of the sequence is important, as <em>value1</em> is assumed to occur (<em>N</em>-1)
    /// periods before <em>valueN</em> and will have been able to accrue that much more interest at the assumed
    /// <em>discountRate</em>.</p>
    /// <p class="body">By convention, the NPV() assumes cash flows occur at the end of each period. Consequently,
    /// the NPV() represents the present value as of the date one period's length before the first cash flow, 
    /// <em>value1</em>, has been made. If you require flexibility in when cash flows occur (the beginning or
    /// end of each period), consider using the <see cref="UltraCalcFunctionPV">PV()</see> function. However,
    /// it differs from NPV() in that NPV() allows cash flows of different amounts.</p>
    /// <p class="body">Only numeric values or text values convertible to numeric values may be passed to this
    /// function. If a column or range reference is passed as a <em>value</em> argument, only numeric values or
    /// text values convertible to numeric values are used.</p>
    /// <seealso cref="UltraCalcFunctionIRR">IRR()</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionNPV : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			double temp;
			double fv = 0.0;

			// Remainder of stack args are cash flows that we want to process front to back so
			// collect the arguments into an array for processing.
			UltraCalcValue[] args = new UltraCalcValue[argumentCount - 1];

			for (int arg_count=0; arg_count < argumentCount-1; arg_count++) 
				args[arg_count] = numberStack.Pop();

			// Compute rate factor (1+DiscountRate)
			double rateFactor;

			if (!numberStack.Pop().ToDouble(out rateFactor))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			rateFactor += 1d;

			// The first cash flow argument is the last item in the array
			int cashFlowCount = 1;
			for (int arg_count=args.Length; arg_count > 0; arg_count--) 
			{
				UltraCalcValue valueArg = (UltraCalcValue)args[arg_count-1];
				if (valueArg.IsReference && valueArg.ToReference().IsEnumerable) 
				{
					IUltraCalcReferenceCollection ReferenceCollection  = valueArg.ToReference().References;
					foreach (IUltraCalcReference cellReference in ReferenceCollection)
					{
						UltraCalcValue val = cellReference.Value;

						// JJD 9/09/04 - UWC99
						// If the value is an error than raise the exception
						if ( val.IsError )
							return new UltraCalcValue( val.ToErrorValue() );
						
						if (val.ToDouble(out temp))
							fv += temp / Math.Pow(rateFactor, cashFlowCount++);
					}
				} 
				else 
				{

					// JJD 9/09/04 - UWC99
					// If the value is an error than raise the exception
					if ( valueArg.IsError )
						return new UltraCalcValue( valueArg.ToErrorValue() );

					if (valueArg.ToDouble(out temp))
						fv += temp / Math.Pow(rateFactor, cashFlowCount++);
				}
			}

			return new UltraCalcValue(fv);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			// Only the valueN parameters can be enumerable
			return 1 <= parameterIndex;
		}

		#endregion  // CanParameterBeEnumerable

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "npv"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args
	}
#endregion //UltraCalcFunctionNPV

	#region UltraCalcFunctionFV
    /// <summary>
    /// Calculates the future value of an annuity having fixed payments and assuming
    /// a fixed interest rate or rate of return.
    /// </summary>
    /// <remarks>
    /// <p class="body">FV(interestRate, nPeriod, payment, presentValue, paymentDue)</p>
    /// <p class="body">InterestRate is the assumed interest rate per period. The future
    /// value calculation assumes no change in the interest rate over the course of the
    /// investment. If you anticipate interest rate fluctuations, you should calculate
    /// the future value incrementally over shorter periods in which the interest rate
    /// is held constant. In some calculations, the <em>interestRate</em> may be
    /// synonymous with an investments' expected growth or rate of return per period.</p>
    /// <p class="body">NPeriod is the total number of payment periods in an annuity.
    /// Each payment is assumed to be of the same amount, and occur at regular fixed
    /// time intervals. A payment earlier in the annuity will be invested for a longer
    /// period of time and therefore would accrue more interest than a payment made
    /// later in the annuity.</p>
    /// <p class="body">Payment is a fixed amount invested in the annuity at each
    /// period. Use a negative number to represent an outflow of cash paid out, and a
    /// positive number to represent an inflow of cash received. Calculating a future
    /// value requires either a non-zero series of payments or a <em>presentValue</em>.</p>
    /// <p class="body">PresentValue is the value of the investment at the beginning of
    /// the annuity. It represents the discounted value of a series of future payments,
    /// which over time could be worth a greater amount because they have accrued interest.
    /// If the annuity has no present value, then you must specify a non-zero <em>payment</em>
    /// amount.</p>
    /// <p class="body">PaymentDue is a numeric value of either 1 or 0, and indicates whether
    /// payments are invested in the annuity at the beginning of each period (1) or at the end
    /// of each period (0).</p>
    /// <p class="body">The <em>interestRate</em> and <em>nPeriod</em> arguments determine the
    /// frequency of payments. If <em>interestRate</em> is given as an annual rate, and <em>nPeriod</em>
    /// is 1 then payments are once per year.  On the other hand, if <em>nPeriod</em> had been 4 then
    /// payments would occur quarterly. Please ensure you use consistent time values when specifying
    /// these arguments.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionFV : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int option = 0;

			if (argumentCount == 5)	// Get option
			{
				// MD 4/6/12 - TFS101506
				//option = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
				// MD 4/10/12
				// Found while fixing TFS108678
				// We need to percolate up errors.
				//option = numberStack.Pop().ToInt32();
				UltraCalcValue optionArg = numberStack.Pop();

				if (optionArg.IsError)
					return optionArg;

				option = optionArg.ToInt32();
			}

			double startingValue = 0;
			
			// Get pv 
			if (argumentCount >= 4)
			{
				UltraCalcValue value = numberStack.Pop();

				if (value.IsError)
					return new UltraCalcValue( value.ToErrorValue() );

				// SSP 2/2/05
				// I believe this was a typo.
				//
				//if (!!value.ToDouble(out startingValue))
				if ( ! value.ToDouble( out startingValue ) )
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			}

			double payment, period, rate;

			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up errors.
			//if (!numberStack.Pop().ToDouble(out payment)	||	// Get payment
			//    !numberStack.Pop().ToDouble(out period)		||	// Get period
			//    !numberStack.Pop().ToDouble(out rate))			// Get interest rate
			//    return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			UltraCalcValue paymentArg = numberStack.Pop();
			UltraCalcValue periodArg = numberStack.Pop();
			UltraCalcValue rateArg = numberStack.Pop();

			if (rateArg.IsError)
				return rateArg;

			if (periodArg.IsError)
				return periodArg;

			if (paymentArg.IsError)
				return paymentArg;

			if (!paymentArg.ToDouble(out payment) ||	// Get payment
				!periodArg.ToDouble(out period) ||	// Get period
				!rateArg.ToDouble(out rate))			// Get interest rate
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			return new UltraCalcValue( CalculateFV(rate, period, payment, startingValue, option) );
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal static double CalculateFV(double rate, double period, double payment, double startingValue, int option)
		{
			return 
				(payment * 
				(
				(
				Math.Pow(1+rate, option == 0 ? period : period+1)
				-1
				)
				/ rate
				) 
				- 
				(
				option == 0 ? 0 : payment
				) 
				+	
				(	startingValue 
				* Math.Pow(1+rate, period)
				)
				) 
				* 
				-1;
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "fv"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 5; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionFV

	#region UltraCalcFunctionPV
    /// <summary>
    /// Calculates an investment's present value by discounting what a series of fixed future payments is worth
    /// at a specified interest rate.
    /// </summary>
    /// <remarks>
    /// <p class="body">PV(interestRate, nPeriods, amount, futureValue, paymentDue)</p>
    /// <p class="body">InterestRate is the per-period rate of interest used to discount the future payments. For
    /// positive interest rates, the value of future payments diminishes (discounts) to what they are worth now.
    /// It is assumed you can reinvest the present value at <em>interestRate</em> to receive <em>futureValue</em>
    /// after <em>nPeriods</em>.</p>
    /// <p class="body">NPeriods are the total number of payment periods over the course of the annuity. If your
    /// retirement plan annuitizes in 35-years, and you make a fixed quarterly contribution into it, then you
    /// would use 35x4 or 140 as your total number of payment periods.</p>
    ///	<p class="body">Amount is how much is paid (or received) each period. This <em>amount</em> must be constant
    /// over the course of the investment. If you need to calculate the Net Present Value of a series of variable
    /// payment amounts, then you should consider using the <see cref="UltraCalcFunctionNPV">NPV()</see> function.</p>
    /// <p class="body">FutureValue is the expected cash balance of the investment at culmination. If this argument
    /// is omitted, it will be assumed to be zero. In the common usage scenario of calculating loan repayments, a
    /// future value of zero represents having a zero liability when the loan is fully repaid. When specifying a
    /// zero future value, ensure that <em>amount</em> is non-zero (otherwise the loan could never be repaid). In
    /// retirement planning, you would normally estimate the future value you would like to have when you retire.
    /// </p>
    /// <p class="body">PaymentDue is a numeric value of either 1 or 0, and indicates whether
    /// payments are invested in the annuity at the beginning of each period (1) or at the end
    /// of each period (0).</p>
    /// </remarks>

	internal





        class UltraCalcFunctionPV : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int option = 0;

			if (argumentCount == 5)	// Get option
				option = numberStack.Pop().ToInt32();

			double fv = 0; // Get fv 
			if (argumentCount >= 4 && !numberStack.Pop().ToDouble(out fv) ) 
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double payment, period, rate;
			
			if (!numberStack.Pop().ToDouble(out payment)	||	// Get payment
				!numberStack.Pop().ToDouble(out period)		||	// Get period
				!numberStack.Pop().ToDouble(out rate))			// Get interest rate
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// Adjust payment given option
			if (option != 0)
				payment *= 1+rate;

			// Calculate the present value
			return new UltraCalcValue((payment * (1 - Math.Pow(1+rate, -period)) / rate + (fv / Math.Pow(1+rate, period))) * -1);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "pv"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 5; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionPV

	#region UltraCalcFunctionPmt
    /// <summary>
    /// Calculates what the payment amount should be on a loan at a fixed interest rate requiring
    /// a fixed number of payments.
    /// </summary>
    /// <remarks>
    /// <p class="body">PMT(interestRate, nPeriods, presentValue, futureValue, paymentDue)</p>
    /// <p class="body">InterestRate is the interest rate for the loan.</p>
    /// <p class="body">NPeriods is the number of payments required to pay back the loan.</p>
    /// <p class="body">PresentValue is the current value of the loan (also called the principal),
    /// which is a lump sum that the future series of <em>nPeriods</em> payments (which accumulate
    /// interest at <em>interestRate</em>) is worth today.</p>
    /// <p class="body">FutureValue is the cash balance in the future (for a loan, this will typically
    /// be a loan liability balance of zero) following this series of fixed payments, accruing a fixed
    /// <em>interestRate</em>. If omitted, a default future value of 0 is used.</p>
    /// <p class="body">PaymentDue is a numeric value of either 1 or 0, and indicates whether
    /// payments are invested in the annuity at the beginning of each period (1) or at the end
    /// of each period (0).</p>
    /// </remarks>

	internal





        class UltraCalcFunctionPmt : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{

			// Get option
			int option = 0;
			if (argumentCount == 5)
				option = numberStack.Pop().ToInt32();

			// Get future value 
			double fv = 0;
			if (argumentCount >= 4 && !numberStack.Pop().ToDouble(out fv))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double pv, period, rate;
			
			if (!numberStack.Pop().ToDouble(out pv)			||	// Get present value 
				!numberStack.Pop().ToDouble(out period)		||	// Get period
				!numberStack.Pop().ToDouble(out rate))			// Get interest rate
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// Compute fv if specified 
			if (fv != 0 && rate != 0)
				fv = option == 0 ? (fv * rate) / (1 - Math.Pow(rate + 1, period)) : ((fv * rate) / (1 - Math.Pow(rate + 1, period))) / (rate + 1);

			// Computer payment based on option and rate
			if (rate != 0)
			{
				if (option == 0)
					return new UltraCalcValue((pv * (rate / (1 - Math.Pow(rate + 1, -period))) - fv ) * -1);
				else
					return new UltraCalcValue(((pv / (1 + rate)) * (rate / (1 - Math.Pow(rate + 1, -period)))	- fv) * -1 );
			}

			return new UltraCalcValue((pv / period - fv ) * -1);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "pmt"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 5; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionPmt

	#region UltraCalcFunctionNPer
    /// <summary>
    /// Calculates an investment's number of payment periods when the payment frequency, amount,
    /// and interest rate are held steady.
    /// </summary>
    /// <remarks>
    /// <p class="body">NPER(interestRate, amount, presentValue, futureValue, paymentDue)</p>
    /// <p class="body">InterestRate is the rate of interest per payment period. This numeric
    /// value must be held fixed for the duration of the investment. If you must calculate
    /// financing based on changes in interest rate then you will need to break up the
    /// calculation into several stages over which the interest rate is held constant.</p>
    /// <p class="body">Amount is the payment amount due each period. This numeric value must
    /// be constant over the duration of the investment.</p>
    /// <p class="body">PresentValue is a discounted value or lump sum payment that if taken today
    /// and invested at the <em>interestRate</em> would be worth the same as this series of fixed
    /// payments in the future.</p>
    /// <p class="body">FutureValue is the value after the last payment has been made. If this
    /// numeric value is omitted it is assumed to be zero (since this function is normally used
    /// in calculating loan repayment you will reach zero liability when the loan is finally
    /// paid off.)</p>
    /// <p class="body">PaymentDue indicates whether payments come due at the beginning of each
    /// payment period (1) or at the end of each payment period (0). If left unspecified, the
    /// default is to assume <em>paymentDue</em> occurs at the end of each payment period.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionNPer : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{

			int option = 0;
			if (argumentCount == 5)		// Get option
				option = numberStack.Pop().ToInt32();

			double fv = 0;	// Get future value 
			if (argumentCount >= 4 && !numberStack.Pop().ToDouble(out fv))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double pv, pmt, rate;

			if (!numberStack.Pop().ToDouble(out pv)			||	// Get present value 
				!numberStack.Pop().ToDouble(out pmt)		||	// Get payment
				pmt == 0									||
				!numberStack.Pop().ToDouble(out rate))			// Get interest rate
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

            // MRS 1/3/2008 - BR29287
            // I did some testing and it looks like the code we have here is wrong more often
            // than not, even before I put in the fix for BR26971.
            // So I'm replacing this entire thing with a call to the VisualBasic.Financial.NPer 
            // function. 
            #region Old Code
            //// MRS 10/3/07 - BR26971
            //// In Excel, the pmt is expected to be negative since it's a cash outflow. 
            //// The formula we are using here apparently expects a positive number, though. 
            //// Since a negative number will never make sense for our formula, and so as not
            //// to break any existing code that is passing in a positive number, we are just
            //// go to call Abs on the payment to make it always positive. 
            //// This means we are a little inconsistent with Excel. In Excel, if you pass in a 
            //// positive number, you get NaN. In CalculationManager, we will act as though 
            //// the number was negative. 
            //pmt = Math.Abs(pmt);
            
            //// Compute fv if specified 
            //if (fv != 0 && rate != 0)
            //{
            //    //	Calculate FV
            //    if (option == 0)
            //        fv = Math.Log(1 + (fv * rate / pmt)) / Math.Log(1 + rate);
            //    else
            //        fv = Math.Log(1 + ((fv / (1 + rate)) * rate / pmt)) / Math.Log(1 + rate);                
            //}

            //// Computer payment based on option
            //if (rate != 0)
            //{
            //    if (pmt != 0)
            //    {
            //        if (option == 0)
            //            return new UltraCalcValue((Math.Log(1 + (pv * rate / pmt)) / Math.Log(1+rate) + fv) * -1);
            //        else
            //            return new UltraCalcValue((Math.Log(1 + ((pv/(1 + rate)) * rate / pmt)) / Math.Log(1+rate) + fv) * -1);
            //    }
            //    else
            //        return new UltraCalcValue(Math.Log(fv/pv) / Math.Log(1+rate));
            //}

            //return new UltraCalcValue(((fv - pv) / pmt) * -1);
            #endregion //Old Code

            // MRS 1/3/2008 - BR29287
            // We don't want to the VB NPer funciton to raise any exceptions, so do some checking
            // before we call it. 
            if (rate <= -1.0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            if (rate == 0.0 && pmt == 0.0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            DueDate dueDate = (DueDate)option;
            double nper = Financial.NPer(rate, pmt, pv, fv, dueDate);

            return new UltraCalcValue(nper);
        }

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "nper"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 5; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionNPer

	#region UltraCalcFunctionEven
    /// <summary>
    /// Rounds a positive number up and a negative number down to the nearest even integer.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// EVEN(Number)
    /// <p></p>
    /// Number	is the value to round.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionEven : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			double number;

			if (!value.ToDouble(out number))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double dividend = Math.Ceiling(Math.Abs(number));
			double result = dividend + Math.Abs(Math.IEEERemainder(dividend,2));

			if (number < 0)
				result *= -1;

			return new UltraCalcValue(result);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "even"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionEven

	#region UltraCalcFunctionOdd
	/// <summary>
	/// Rounds a positive number up and a negative number down to the nearest odd integer.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// ODD(Number)
	/// <p></p>
	/// Number is the value to round.
	/// </p>
	/// </remarks>

	internal





        class UltraCalcFunctionOdd : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			double number;

			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//if (!numberStack.Pop().ToDouble(out number))
			//    return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			UltraCalcValue numberArg = numberStack.Pop();

			if (numberArg.IsError)
				return numberArg;

			if (numberArg.ToDouble(out number) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			double dividend = Math.Ceiling(Math.Abs(number));
			double remainder = Math.Abs(Math.IEEERemainder(dividend,2));

			if (remainder == 0)
				dividend++;

			if (number < 0)
				dividend *= -1;

			return new UltraCalcValue(dividend);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "odd"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionOdd

	#region UltraCalcFunctionPPmt
    /// <summary>
    /// Calculates the principal portion of a payment due on an investment or loan with periodic, fixed payments
    /// and having a fixed rate of interest.
    /// </summary>
    /// <remarks>
    /// <p class="body">PPMT(interestRate, periodNumber, nPeriods, presentValue, futureValue, paymentDue)</p>
    /// <p class="body">InterestRate is a fixed rate of interest per period. This function assumes there
    /// is one payment made on each period, therefore in cases where you want to calculate quarterly or
    /// monthly payments you should adjust an annual <em>interestRate</em> accordingly by dividing it 
    /// by the number of payments per year.</p>
    /// <p class="body">PeriodNumber identifies the period of the current payment, where the first payment
    /// has the number 1. The <em>periodNumber</em> must not exceed the total number of periods (<em>nPeriods</em>).</p>
    /// <p class="body">NPeriods is the total number of payments over the course of this investment or loan.</p>
    /// <p class="body">PresentValue is the discounted value of this series of future payments, if you could take
    /// a lump sum payment today and invest it at the fixed <em>interestRate</em> until the future date when this
    /// investment or loan had been repaid.</p>
    /// <p class="body">FutureValue is the expected value of this series of payments after the last payment has
    /// been made, where all previous payments have been accumulating interest at the fixed <em>interestRate</em>.
    /// When omitted, such as when this function is used for calculating loan payments that reduce an outstanding
    /// liability, the future value is assumed to be zero.</p>
    /// <p class="body">PaymentDue is a numeric value indicating that payments are due at the beginning of each period (1)
    /// or at the end of each period (0).</p>
    /// <p class="body">The PPMT() function calculates the portion of a fixed payment attributed to principal. If you
    /// need to calculate the portion of a fixed payment repaying interest then use the <see cref="UltraCalcFunctionIPmt">IPMT()</see>
    /// function. To calculate fixed payment amounts, use the <see cref="UltraCalcFunctionPmt">PMT()</see> function.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionPPmt : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			double ppmt = 0;
			double option = 0;
			double fv = 0;
			double pv, nper, per, rate;
			
			if ((argumentCount == 6 && !numberStack.Pop().ToDouble(out option)) ||	// option
				(argumentCount >= 5 && !numberStack.Pop().ToDouble(out fv))		||	// future value
				!numberStack.Pop().ToDouble(out pv)			||		// present value
				!numberStack.Pop().ToDouble(out nper)		||		// number of periods
				!numberStack.Pop().ToDouble(out per)		||		// calculated period
				!numberStack.Pop().ToDouble(out rate))				// interest rate
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// Check for valid arguments
			if (nper <= 0 || per <= 0 || per > nper) 
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			// MRS 4/7/05 - BR03118
			//double ipmt = 0;

			//			// Zero Interest?
			//			if (rate == 0) 
			//				ppmt = (pv+fv) / nper;
			//			else 
			//			{
			// MRS 5/23/05 - BR03118
			//				// Calculate payment
			//				numberStack.Push( new UltraCalcValue(rate) );
			//				numberStack.Push( new UltraCalcValue(nper) );
			//				numberStack.Push( new UltraCalcValue(pv) );
			//				numberStack.Push( new UltraCalcValue(fv) ) ;
			//				numberStack.Push( new UltraCalcValue(option) );
			//				UltraCalcFunctionPmt funcPmt = new UltraCalcFunctionPmt();
			//				funcPmt.PerformEvaluation(numberStack, 5);
			//
			//				double pmt;
			//				
			//				if (!numberStack.Pop().ToDouble(out pmt))
			//					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// MRS 4/7/05 - BR03118
			
			
			
			

			//				// Calculate first period interest
			//				if (option == 0) 
			//					ipmt = rate * pv;
			//
			//				pv = pv + pmt + ipmt;
			//
			//				// Are we calculating a period other than the first one?
			//				if (per != 1)
			//				{
			//					// Calculate interest amount
			//					double ifactor = 1 / (1+rate);	
			//					double ifactor2 = 1 - (1 / (1+rate));	
			//
			//					ipmt = rate * ( ((pmt*ifactor + pv*ifactor2) /ifactor2) * ((Math.Pow((1/ifactor),per-2)- Math.Pow((1/ifactor),per-1)) / (1-(1/ifactor))) + (pmt*ifactor / ifactor2) * -1);
			//				} 
			//				// Calculate the principle portion of payment
			//				ppmt = pmt*-1 - ipmt;
			//			}

			// MRS 5/23/05 - BR03118
			// Use the MS VisualBasic function. 
			DueDate dueDate = (DueDate)option;
			ppmt = Financial.PPmt(rate, per, nper, pv, fv, dueDate);

			return new UltraCalcValue(ppmt);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "ppmt"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 4; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 6; }
		}
		#endregion //Min/Max args


	}
	#endregion //UltraCalcFunctionPPmt

	#region UltraCalcFunctionIPmt
    /// <summary>
    /// Calculates the interest portion of a payment due on an investment or loan with periodic, fixed payments
    /// and having a fixed rate of interest.
    /// </summary>
    /// <remarks>
    /// <p class="body">IPMT(interestRate, periodNumber, nPeriods, presentValue, futureValue, paymentDue)</p>
    /// <p class="body">InterestRate is a fixed rate of interest per period. This function assumes there
    /// is one payment made on each period, therefore in cases where you want to calculate quarterly or
    /// monthly payments you should adjust an annual <em>interestRate</em> accordingly by dividing it 
    /// by the number of payments per year.</p>
    /// <p class="body">PeriodNumber identifies the period of the current payment, where the first payment
    /// has the number 1. The <em>periodNumber</em> must not exceed the total number of periods (<em>nPeriods</em>).</p>
    /// <p class="body">NPeriods is the total number of payments over the course of this investment or loan.</p>
    /// <p class="body">PresentValue is the discounted value of this series of future payments, if you could take
    /// a lump sum payment today and invest it at the fixed <em>interestRate</em> until the future date when this
    /// investment or loan had been repaid.</p>
    /// <p class="body">FutureValue is the expected value of this series of payments after the last payment has
    /// been made, where all previous payments have been accumulating interest at the fixed <em>interestRate</em>.
    /// When omitted, such as when this function is used for calculating loan payments that reduce an outstanding
    /// liability, the future value is assumed to be zero.</p>
    /// <p class="body">PaymentDue is a numeric value indicating that payments are due at the beginning of each period (1)
    /// or at the end of each period (0).</p>
    /// <p class="body">The IPMT() function calculates the portion of a fixed payment attributed to interest. If you
    /// need to calculate the portion of a fixed payment repaying loan principal then use the <see cref="UltraCalcFunctionPPmt">PPMT()</see>
    /// function. To calculate fixed payment amounts, use the <see cref="UltraCalcFunctionPmt">PMT()</see> function.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIPmt : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			double option = 0;
			double fv = 0;
			double pv, nper, per, rate;
			
			if ((argumentCount == 6 && !numberStack.Pop().ToDouble(out option)) ||	// option
				(argumentCount >= 5 && !numberStack.Pop().ToDouble(out fv))		||	// future value
				!numberStack.Pop().ToDouble(out pv)			||		// present value
				!numberStack.Pop().ToDouble(out nper)		||		// number of periods
				!numberStack.Pop().ToDouble(out per)		||		// calculated period
				!numberStack.Pop().ToDouble(out rate))				// interest rate
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// Check for valid arguments
			if (nper <= 0 || per <= 0 || per > nper) 
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			double ipmt = 0;

			//			// MRS 5/23/05 - BR03117
			//			// Zero Interest?
			//			if (rate != 0) 
			//			{
			//				// Calculate payment
			//				numberStack.Push( new UltraCalcValue( rate ) );
			//				numberStack.Push( new UltraCalcValue( nper ) );
			//				numberStack.Push( new UltraCalcValue( pv ) );
			//				numberStack.Push( new UltraCalcValue( fv ) );
			//				numberStack.Push( new UltraCalcValue( option ) );
			//				UltraCalcFunctionPmt funcPmt = new UltraCalcFunctionPmt();
			//				funcPmt.PerformEvaluation(numberStack, 5);
			//
			//				double pmt;
			//				
			//				if (!numberStack.Pop().ToDouble(out pmt))
			//					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// MRS 4/7/05 - BR03117
			
			
			
			

			//				// Calculate first period interest
			//				if (option == 0) 
			//					ipmt = rate * pv;
			//
			//				pv = pv + pmt + ipmt;
			//
			//				// Are we calculating a period other than the first one?
			//				if (per != 1)
			//				{
			//					// Calculate interest amount
			//					double ifactor = 1 / (1+rate);	
			//					double ifactor2 = 1 - (1 / (1+rate));	
			//
			//					ipmt = rate * ( ((pmt*ifactor + pv*ifactor2) /ifactor2) * ((Math.Pow((1/ifactor),per-2)- Math.Pow((1/ifactor),per-1)) / (1-(1/ifactor))) + (pmt*ifactor / ifactor2) * -1);
			//				} 					
			//			}

			// MRS 5/23/05 - BR03117
			// Use the MS VisualBasic function. 
			DueDate dueDate = (DueDate)option;
			ipmt = Financial.IPmt(rate, per, nper, pv, fv, dueDate);

			return new UltraCalcValue(ipmt);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "ipmt"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 4; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 6; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIPmt

	#region UltraCalcFunctionLower
    /// <summary>
    /// Converts a text value to all lowercase letters.
    /// </summary>
    /// <remarks>
    /// <p class="body">LOWER(text_value)</p>
    /// <p class="body">Text_value is a piece of text you want converted into lowercase. The invariant
    /// culture is used to translate characters, therefore this function may not be suitable for use
    /// with localizable text values.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionLower : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 3/2/12
			// Found while fixing TFS103729
			// This function was not behaving the way it does in Excel,
			// So if UseExcelFormulaCompatibility is True, we will now use the correct logic.
			if (UltraCalcValue.UseExcelFunctionCompatibility)
			{
				UltraCalcValue value = numberStack.Pop();

				if (value.IsError)
					return value;

				return new UltraCalcValue(value.ToString().ToLower());
			}

#pragma warning disable 0162
			if (numberStack.Peek().IsString)
#pragma warning restore 0162
				// MD 4/6/12 - TFS101506
				//return new UltraCalcValue( numberStack.Pop().ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture) );
				return new UltraCalcValue(numberStack.Pop().ToString().ToLower(numberStack.Culture));
			else
				return numberStack.Pop();
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "lower"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionLower

	#region UltraCalcFunctionUpper
    /// <summary>
    /// Converts all lowercase letters in a text string to uppercase.
    /// </summary>
    /// <remarks>
    /// <p class="body">UPPER(text_value)</p>
    /// <p class="body">Text_value is the text you want to convert to uppercase. UPPER does not change characters in text that are not letters.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionUpper : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 3/2/12
			// Found while fixing TFS103729
			// This function was not behaving the way it does in Excel,
			// So if UseExcelFormulaCompatibility is True, we will now use the correct logic.
			if (UltraCalcValue.UseExcelFunctionCompatibility)
			{
				UltraCalcValue value = numberStack.Pop();

				if (value.IsError)
					return value;

				return new UltraCalcValue(value.ToString().ToUpper());
			}

#pragma warning disable 0162
			if (numberStack.Peek().IsString)
#pragma warning restore 0162
				// MD 4/6/12 - TFS101506
				//return new UltraCalcValue( numberStack.Pop().ToString(CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture) );
				return new UltraCalcValue(numberStack.Pop().ToString().ToUpper(numberStack.Culture));
			else
				return numberStack.Pop();
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "upper"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionUpper

	#region UltraCalcFunctionLeft
    /// <summary>
    /// Gets the leftmost characters from a text value, up to the specified number of characters.
    /// </summary>
    /// <remarks>
    /// <p class="body">LEFT(text_value, num_chars)</p>
    /// <p class="body">Text_value is a piece of text or reference to some text starting with
    /// a substring you want to retrieve.</p>
    /// <p class="body">Num_chars indicate the number of characters retrieved from the beginning
    /// of <em>text_value</em>. An error value is returned if this argument is less than zero.
    /// If this argument exceeds the length of <em>text_value</em>, then all of <em>text_value</em>
    /// is retrieved. If omitted, the first character of <em>text_value</em> is retrieved.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionLeft : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int numChars = 1;
			if (argumentCount > 1) 
			{ 
				UltraCalcValue charCount = numberStack.Pop();

				if (charCount.IsError)
					return new UltraCalcValue( charCount.ToErrorValue() );

				numChars = charCount.ToInt32();

				if (numChars < 0)
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			}
			
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			string s = value.ToString();
			return new UltraCalcValue( s.Substring(0,numChars > s.Length ? s.Length : numChars) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "left"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionLeft

	#region UltraCalcFunctionRight
    /// <summary>
    /// Gets the rightmost characters from a text value, up to the specified number of characters.
    /// </summary>
    /// <remarks>
    /// <p class="body">RIGHT(text_value, num_chars)</p>
    /// <p class="body">Text_value is a piece of text or reference to some text ending with
    /// a substring you want to retrieve.</p>
    /// <p class="body">Num_chars indicate the number of characters retrieved from the end
    /// of <em>text_value</em>. An error value is returned if this argument is less than zero.
    /// If this argument exceeds the length of <em>text_value</em>, then all of <em>text_value</em>
    /// is retrieved. If omitted, the last character of <em>text_value</em> is retrieved.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionRight : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int numChars = 1;

			if (argumentCount > 1)
			{
				UltraCalcValue charCount = numberStack.Pop();

				if (charCount.IsError)
					return new UltraCalcValue( charCount.ToErrorValue() );

				numChars = charCount.ToInt32();

				if (numChars < 0)
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			}

			if (numChars < 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			string s = value.ToString();

			return new UltraCalcValue( s.Substring(numChars > s.Length ? 0 : s.Length-numChars) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "right"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionRight

	#region UltraCalcFunctionMid
    /// <summary>
    /// Gets a specified number of characters from the middle of a text string, starting from a specified position in that string.
    /// </summary>
    /// <remarks>
    /// <p class="body">MID(text_value, starting_point, character_count)</p>
    /// <p class="body">Text_value is a piece of text containing the substring you want to retrieve, when you know
    /// the position and length of your desired substring.</p>
    /// <p class="body">Character_count specifies how long of a substring to retrieve from <em>text_value</em>
    /// measured as a number of characters. If the requested length, when added to the specified <em>starting_point</em>,
    /// exceeds the length of <em>text_value</em> then the remainder of the string starting at <em>starting_point</em>
    /// is returned.</p>
    /// <p class="body">Starting_point is the one-based position within <em>text_value</em> of the first character
    /// in the substring you want to retrieve. If this argument exceeds the length of <em>text_value</em> then the
    /// function will return an empty string.</p>
    /// <p class="body">If either <em>character_count</em> or <em>starting_point</em> has a negative value, or
    /// evaluate to an error value, then the function returns an error value.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionMid : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int numChars = 0;

			if (argumentCount > 2)
			{
				UltraCalcValue charCount = numberStack.Pop();

				if (charCount.IsError)
					return new UltraCalcValue( charCount.ToErrorValue() );

				numChars = charCount.ToInt32();

				if (numChars < 0)
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			}

			int startNum = 1;

			if (argumentCount > 1)
			{
				UltraCalcValue startValue = numberStack.Pop();

				if (startValue.IsError)
					return new UltraCalcValue( startValue.ToErrorValue() );

				startNum = startValue.ToInt32();

				if (numChars < 1)
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			}

			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			string s = value.ToString();

			if (startNum > s.Length)
				return new UltraCalcValue(string.Empty);
			else if (startNum+numChars-1 > s.Length)
				return new UltraCalcValue( s.Substring(startNum-1) );
			else
				return new UltraCalcValue( s.Substring(startNum-1,numChars) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "mid"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionMid

	#region UltraCalcFunctionTrim
    /// <summary>
    /// Removes any leading or trailing spaces from a text value, and normalizes runs of
    /// whitespace within a text value. 
    /// </summary>
    /// <remarks>
    /// <p class="body">TRIM(text_value)</p>
    /// <p class="body">Text_value is a piece of text to trim and normalize the white
    /// space of. Trimming removes all leading and trailing white space. Normalization
    /// reduces runs of consecutive whitespace appearing within the <em>text_value</em>
    /// to single blank spaces.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionTrim : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			// SSP 11/20/04 BR00908
			// TRIM function the way it works in MS Excel replaces occurrences of two consecutive
			// space characters with a single space character in addition to getting rid of the
			// spaces from the beginning and the end of the string. In MS Excel TRIM("the   fox")
			// gives you "the fox".
			//
			//return new UltraCalcValue( value.ToString().Trim() );
			string text = value.ToString( ).Trim( );
			System.Text.StringBuilder sb = new System.Text.StringBuilder( text.Length );			
			bool lastCharWhiteSpace = false;
			for ( int i = 0; i < text.Length; i++ )
			{
				char c = text[i];
				bool isWhiteSpace = char.IsWhiteSpace( c );
				if ( ! lastCharWhiteSpace || ! isWhiteSpace )
					sb.Append( c );

				lastCharWhiteSpace = isWhiteSpace;
			}

			return new UltraCalcValue( sb.ToString( ) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "trim"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionTrim

	#region UltraCalcFunctionLen
    /// <summary>
    /// Counts the number of characters in a text value.
    /// </summary>
    /// <remarks>
    /// <p class="body">LEN(text_value)</p>
    /// <p class="body">Text_value is any text string or reference to a text value
    /// for which you want a character count. The number of characters, including
    /// all whitespace, determines the length of the text string.</p>
    /// <p class="body">Depending on the character encoding used, some whitespace
    /// characters such as line-breaks may count as two characters (one character
    /// is a carriage return, the other character is a line feed).</p>
    /// <p class="body">An empty text string contains no characters.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionLen : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			// MD 4/6/12 - TFS101506
			//return new UltraCalcValue( value.ToString(CultureInfo.InvariantCulture).Length );
			return new UltraCalcValue(value.ToString().Length);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "len"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionLen


	#region UltraCalcFunctionValue
    /// <summary>
    /// Retrieves the numeric value of a piece of text that is supposed to
    /// represent either a number or a currency.
    /// </summary>
    /// <remarks>
    /// <p class="body">VALUE(text_value)</p>
    /// <p class="body">Text_value is any text value or single-value reference to
    /// a text string that you want to convert into a number. It may have been
    /// formatted with a sign, currency symbol, or thousands separator. These
    /// characters will be stripped to yield the numeric value of the text.</p>
    /// <p class="body">If the <em>text_value</em> is an error value or could not
    /// be converted into a numeric value then the function returns an error value.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionValue : BuiltInFunctionBase
	{
		




		private static readonly char[] CurrencySymbols = new char[] {
																		'\x0024',	// Dollar (US, Aus, ...)
																		'\x0080',	// Euro Win CP (NOT a Unicode codepoint -- MICROSOFT SPECIFIC)
																		
																		'\x00A3',	// British Pound Sterling
																		'\x00A4',	// Terran Currency Symbol
																		'\x00A5',	// Yen
																		'\x09F2',	// Bengali Rupee Mark
																		'\x09F3',	// Bengali Rupee Sign
																		'\x0E3F',	// Baht
																		'\x17DB',	// Cambodian Riels
																		'\x20A0',	// ECU symbol (pre-euro, see Unicode Tech Rep #8)
																		'\x20A1',	// Central American Col�n
																		'\x20A2',	// Cruizeiros (old)
																		'\x20A3',	// Francs (old)
																		'\x20A4',	// Lire, Lira, Liri.
																		
																		'\x20A6',	// Naira
																		'\x20A7',	// Pasetas (old)
																		'\x20A8',	// Rupees
																		'\x20A9',	// Won
																		'\x20AA',	// New Sheqels
																		'\x20AB',	// Vietnamese Dong
																		'\x20AC',	// EU Euro
																		'\x20AD',	// Laosian Kips
																		'\x20AE',	// Mongolian Tugriks
																		'\x20AF',	// Drachmae (old)
																		
																		'\xFDFC',	// Arabian Rial
																		'\xFE69',	// Tiny Dollar
																		'\xFF04',	// Full-width Dollar
																		
																		'\xFFE1',	// Full-width Pound Sterling 
																		'\xFFE5',	// Full-width Yen
																		'\xFFE6'	// Full-width Won
																	};

		/// <summary>
		/// Evaluates the function against the argument on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments.</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <remarks>
		/// <p class="body">Returns an error if the <b>UltraCalcValue</b> argument at the top of the number stack was an error.</p>
		/// <p class="body">Returns a value conversion into a double-precision numeric type if the incoming text can be interpreted as a numeric value (possibly after filtering off thousands separators, decimal separators, currency symbols, exponents and signs.)</p>
		/// <p class="body">Returns a numeric conversion from another numeric type if the incoming argument was not text (sometimes when the result of such a conversion is poorly defined this may defy an application's expectations, for example, a <b>DATEVALUE</b> may convert into a numeric value of ticks.)</p>
		/// </remarks>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate( UltraCalcNumberStack numberStack, int argumentCount)
		{
			// Get my single argument off of the numberStack.
			UltraCalcValue value = numberStack.Pop();

			// VALUE(#ERR!) -> #ERR!
			if (value.IsError)
			{
				return new UltraCalcValue( value.ToErrorValue());
			}

			// Successful flag indicates whether result is valid.
			bool   successful = false;
			double result = 0.0;

			// UltraCalcValue's ToDouble() is too restrictive, the VALUE() function
			// requires that I allow currency symbols in the incoming text, BR03563.
			//
			if (value.Value is string)
			{
				string arg = ((string)( value.Value)).Trim();

				// Strip any currency symbols from the front and back of the text,
				// because NumberStyles.AllowCurrencySymbol on the InvariantCulture
				// will strip only the '$', BR03563.
				//
				if ( arg.EndsWith("kr") )	// Swedish krona
					arg = arg.Substring( 0, arg.Length - 2);
					// Brasilian real, or Peruvian nuevo sol
				else if ( arg.StartsWith("R$") || arg.StartsWith( "S/") )
					arg = arg.Substring( 2, arg.Length - 2);
				else	// Rest of the world
					arg = arg.Trim( UltraCalcFunctionValue.CurrencySymbols);

				// MD 4/9/12 - TFS101506
				//successful = Double.TryParse( arg, 
				//    NumberStyles.Float | NumberStyles.AllowThousands,
				//    CultureInfo.InvariantCulture,
				//    out result
				//    );
				successful = MathUtilities.DoubleTryParse(arg, numberStack.Culture, out result);
			}

			// Revert to original behavior when the incoming value isn't a string.
			if ( !successful)
			{
				successful = value.ToDouble( out result);
			}

			// When VALUE() has been successful return the resulting value, otherwise
			// return a #NUM! error.
			return ( successful ) ? new UltraCalcValue( result) : 
				new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Num));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula.
		/// </summary>
		public override string Name
		{ 
			get { return "value"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function.
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionValue


	#region UltraCalcFunctionFind
    /// <summary>
    /// Finds one piece of text within another piece of text.
    /// </summary>
    /// <remarks>
    /// <p class="body">FIND(text_to_find, text_to_search, starting_point)</p>
    /// <p class="body">Text_to_find is the piece of text you want to find. It can
    /// be a reference (for example, a NamedReference to a constant text string or
    /// a CalcSettings reference to a TextBox on the form). It should be shorter in
    /// length than the <em>text_to_search</em>.</p>
    /// <p class="body">Text_to_search is the piece of text you want to search for
    /// <em>text_to_find</em> within. It's commonly a reference to a text string
    /// that you want to search.</p>
    /// <p class="body">Starting_point is the one-based character position inside of 
    /// <em>text_to_search</em> at which UltraCalc will begin searching. This argument
    /// is optional and if omitted, the search will begin at the first character.</p>
    /// <p class="body">If <em>text_to_find</em> is not found this function returns
    /// a Value error, otherwise it returns the starting position of <em>text_to_find</em>
    /// within <em>text_to_search</em>.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionFind : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int startNum = 0;

			if (argumentCount > 2)
			{
				if (numberStack.Peek().IsError)
					return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

				// MD 4/6/12 - TFS101506
				//startNum = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture)-1;
				startNum = numberStack.Pop().ToInt32() - 1;

				if (startNum < 0)
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			}

			if (numberStack.Peek().IsError)
				return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

			// MD 4/6/12 - TFS101506
			//string strWithinText = numberStack.Pop().ToString(CultureInfo.InvariantCulture);
			string strWithinText = numberStack.Pop().ToString();

			if (startNum > strWithinText.Length-1)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			
			if (numberStack.Peek().IsError)
				return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

			int pos = -1;

			// MD 4/6/12 - TFS101506
			//string strFindText = numberStack.Pop().ToString(CultureInfo.InvariantCulture);
			string strFindText = numberStack.Pop().ToString();

			if (strFindText.Length == 0)
				pos = 1;
			else
				pos = strWithinText.IndexOf(strFindText,startNum);

			if (pos != -1)
				return new UltraCalcValue(pos+1);
			else 
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "find"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionFind

	#region UltraCalcFunctionReplace
    /// <summary>
    /// Replaces a substring of a text value with some new text. This substring is specified by giving
    /// it's starting point and it's length in characters, within the original text value.
    /// </summary>
    /// <remarks>
    /// <p class="body">REPLACE(text_value, starting_point, character_count, new_text_value)</p>
    /// <p class="body">Text_value is the original text within which you want to replace some substring of text,
    /// and you already know the <em>starting_point</em> and <em>character_count</em> of that substring.</p>
    /// <p class="body">Starting_point is the one-based position within <em>text_value</em> where the replaced
    /// substring begins.</p>
    /// <p class="body">Character_count is the length of the substring being replaced as a count of the characters
    /// it contains. If this value when added to <em>starting_point</em> would exceed the length remaining in
    /// <em>text_value</em> then the entire remainder of <em>text_value</em> is replaced.</p>
    /// <p class="body">New_text_value is some new text that replaces the text of the specified substring. It is
    /// not required to be the same length as the replaced substring.</p>
    /// <p class="body">When <em>starting_point</em> or <em>character_count</em> are error values, or less than zero,
    /// an error value is returned. If <em>starting_point</em> exceeds the length of <em>text_value</em> then an error
    /// value is returned.</p>
    /// <p class="body">It is possible to use the REPLACE() function with other UltraCalc functions to express the
    /// replacement of one substring with another. The following UltraCalc expression replaces the word "Old" with
    /// the word "New" in the original text value.</p>
    /// <code>REPLACE("Hello Old World", FIND("Old"), LEN("Old"), "New")</code>
    /// </remarks>

	internal





        class UltraCalcFunctionReplace : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 4/6/12 - TFS101506
			//string strNewText = numberStack.Pop().ToString(CultureInfo.InvariantCulture);
			//int numChars = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			//int startNum = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture)-1;
			//string strOldText = numberStack.Pop().ToString(CultureInfo.InvariantCulture);
			// MD 4/10/12v
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string strNewText = numberStack.Pop().ToString();
			//int numChars = numberStack.Pop().ToInt32();
			//int startNum = numberStack.Pop().ToInt32() - 1;
			//string strOldText = numberStack.Pop().ToString();
			UltraCalcValue strNewTextArg = numberStack.Pop();
			UltraCalcValue numCharsArg = numberStack.Pop();
			UltraCalcValue startNumArg = numberStack.Pop();
			UltraCalcValue strOldTextArg = numberStack.Pop();

			if (strOldTextArg.IsError)
				return strOldTextArg;

			if (startNumArg.IsError)
				return startNumArg;

			if (numCharsArg.IsError)
				return numCharsArg;

			if (strNewTextArg.IsError)
				return strNewTextArg;

			string strNewText = strNewTextArg.ToString();
			int numChars = numCharsArg.ToInt32();
			int startNum = startNumArg.ToInt32() - 1;
			string strOldText = strOldTextArg.ToString();

			if (startNum < 0 || startNum > strOldText.Length)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );

			if (numChars < 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );

			if (numChars > strOldText.Length-startNum)
				numChars = strOldText.Length-startNum;

			string strResult = strOldText.Substring(0,startNum) + strNewText + strOldText.Substring(startNum+numChars);

			return new UltraCalcValue( strResult );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "replace"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 4; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 4; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionReplace

	#region UltraCalcFunctionSYD
    /// <summary>
    /// Calculates depreciation of an asset using the sum of years' digits (SYD)
    /// calculation method.
    /// </summary>
    /// <remarks>
    /// <p class="body">SYD(assetCost, salvageValue, lifespan, periodNumber)</p>
    /// <p class="body">AssetCost is the initial value of the asset
    /// when it was purchased new. This numeric value needs to be
    /// greater than the <em>salvageValue</em> (otherwise the asset
    /// would be appreciating in value).</p>
    /// <p class="body">SalvageValue is the market price you can get
    /// for an asset at the end of it's useful life (<em>lifespan</em>
    /// periods). In some situations, this may represent the value of
    /// the asset's spare parts.</p>
    /// <p class="body">Lifespan is the useful life of the asset being
    /// depreciated measured in fixed-length time periods (usually years).
    /// The appropriate <em>lifespan</em> to use may vary with the kind
    /// of asset being depreciated. As the name of this function suggests,
    /// the <em>lifespan</em> when using this depreciation method is normally
    /// measured in years (although this does not necessarily have to be the
    /// case).</p>
    /// <p class="body">PeriodNumber is the one-based number of the period
    /// to calculate the depreciation of the asset for, having a value of
    /// between 1 and <em>lifespan</em>. It must be measured in the same
    /// units of time as the <em>lifespan</em>.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionSYD : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 4/6/12 - TFS101506
			//int per = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			int per = numberStack.Pop().ToInt32();

			if (per <= 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// MD 4/6/12 - TFS101506
			//int life = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			int life = numberStack.Pop().ToInt32();

			if (life <= 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double salvage, cost;

			if (!numberStack.Pop().ToDouble(out salvage) || !numberStack.Pop().ToDouble(out cost))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( ((cost-salvage)*(life-per+1)*2)/(life*(life+1)) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "syd"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 4; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 4; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSYD

	#region UltraCalcFunctionSLN
    /// <summary>
    /// Calculates what the straight-line depreciation
    /// of an asset should be per period.
    /// </summary>
    /// <remarks>
    /// <p class="body">SLN(assetCost, salvageValue, lifespan)</p>
    /// <p class="body">AssetCost is the initial value of the asset
    /// when it was purchased new. This numeric value needs to be
    /// greater than the <em>salvageValue</em> (otherwise the asset
    /// would be appreciating in value).</p>
    /// <p class="body">SalvageValue is the market price you can get
    /// for an asset at the end of it's useful life (<em>lifespan</em>
    /// periods). In some situations, this may represent the value of
    /// the asset's spare parts.</p>
    /// <p class="body">Lifespan is the useful life of the asset being
    /// depreciated measured in fixed-length time periods (usually years).
    /// The appropriate <em>lifespan</em> to use may vary with the kind
    /// of asset being depreciated.</p>
    /// <p class="body">Straight-line depreciation expresses an asset's
    /// depreciation at a constant rate per period. The asset is assumed
    /// to lose useful value no faster in the first period depreciation
    /// is calculated than in the last period. An accountant can advise
    /// you as to which assets the straight-line depreciation calculation
    /// method is suitable.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionSLN : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 4/6/12 - TFS101506
			//int life = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			int life = numberStack.Pop().ToInt32();

			if (life == 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Div) );

			double salvage, cost;

			if (!numberStack.Pop().ToDouble(out salvage) || !numberStack.Pop().ToDouble(out cost))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			return new UltraCalcValue( (cost-salvage)/life );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "sln"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSLN

	#region UltraCalcFunctionDB
    /// <summary>
    /// Calculates an asset's depreciation using the fixed declining balance (DB) method over a specified term.
    /// </summary>
    /// <remarks>
    /// <p class="body">DB(assetCost, salvageValue, lifespan, period, months)</p>
    /// <p class="body">AssetCost is the original cost or purchase price of the asset at the start of the
    /// calculation.</p>
    /// <p class="body">SalvageValue is the market value of the asset after it's expected useful life ends,
    /// sometimes this will be the value of the asset as spare parts.</p>
    /// <p class="body">Lifespan indicates for how many periods the asset is useful (it's useful life), and
    /// must be depreciated. Accounting standards vary on what <em>lifespan</em> is appropriate for different
    /// classes of assets, such as durable and non-durable goods.</p>
    /// <p class="body">Period indicates the number of units of time between decremental calculations of the
    /// depreciation. If <em>lifespan</em> is measured in years, then the value specified for the <em>period</em>
    /// must also be in years.</p>
    /// <p class="body">Months allows you to specify the number of months in the first year, if the depreciation
    /// does not begin on the first day of the year. You might specify <em>months</em> based on when the asset
    /// was purchased new. If omitted, the calculation defaults to 12 which is equivalent to calculating starting
    /// from the first day of the year.</p>
    /// <p class="body">For some assets, alternative calculation methods such as the <see cref="UltraCalcFunctionDDB">DDB()</see>
    /// function may be more appropriate. Your accountant can tell you for which assets the fixed declining
    /// balance method is an acceptable means of calculating depreciation.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionDB : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int month = 12;

			if (argumentCount > 4)
			{
				// MD 4/6/12 - TFS101506
				//month = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
				month = numberStack.Pop().ToInt32();
			}

			// MD 4/6/12 - TFS101506
			//int per = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			int per = numberStack.Pop().ToInt32();

			if (per <= 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double life;
			
			if (!numberStack.Pop().ToDouble(out life) || life <= 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double salvage, cost;

			if (!numberStack.Pop().ToDouble(out salvage) || !numberStack.Pop().ToDouble(out cost))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double rate = Math.Round(1 - Math.Pow(salvage / cost, 1 / life),3);

			double db;

			if (per == 1)
			{
				//if (life ==1)
				//	db = cost-salvage;
				//else
				db = rate * cost * month / 12;
			} 
			else 
			{
				if (per == life) 
					db = Math.Pow((1-rate),(per-1)) * rate * cost;
				else 
					db = Math.Pow((1-rate),(per-1)) * rate * cost;
			}

			return new UltraCalcValue(db);

		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "db"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 4; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 5; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionDB

	#region UltraCalcFunctionDDB
    /// <summary>
    /// Calculates an asset's depreciation using the double declining balance (DDB) or another weighted
    /// factor method over a specified term.
    /// </summary>
    /// <remarks>
    /// <p class="body">DDB(assetCost, salvageValue, lifespan, period, weight)</p>
    /// <p class="body">AssetCost is the original cost or purchase price of the asset at the start of the
    /// calculation.</p>
    /// <p class="body">SalvageValue is the market value of the asset after it's expected useful life ends,
    /// sometimes this will be the value of the asset as spare parts.</p>
    /// <p class="body">Lifespan indicates for how many periods the asset is useful (it's useful life), and
    /// must be depreciated. Accounting standards vary on what <em>lifespan</em> is appropriate for different
    /// classes of assets, such as durable and non-durable goods.</p>
    /// <p class="body">Period indicates the number of units of time between decremental calculations of the
    /// depreciation. If <em>lifespan</em> is measured in years, then the value specified for the <em>period</em>
    /// must also be in years.</p>
    /// <p class="body">Weight allows you to fine tune the calculation method. By default, the double declining
    /// balance method uses a factor of 2.</p>
    /// <p class="body">For some assets, alternative calculation methods such as the <see cref="UltraCalcFunctionDB">DB()</see>
    /// function may be more appropriate. Your accountant can advise you for which assets the double declining
    /// balance method is an acceptable means of calculating depreciation, and what factors can be used when
    /// depreciating certain assets such as high-tech equipment or motor vehicles which exhibit accelerated
    /// depreciation in their first years of use.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionDDB : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			// first get and validate the values
			UltraCalcValue factorVal = argumentCount > 4 ? numberStack.Pop() : new UltraCalcValue(2d);
			UltraCalcValue periodVal = numberStack.Pop();
			UltraCalcValue lifeVal = numberStack.Pop();
			UltraCalcValue salvageVal = numberStack.Pop();
			UltraCalcValue costVal = numberStack.Pop();

			if (factorVal.IsNull || periodVal.IsNull || lifeVal.IsNull || salvageVal.IsNull || costVal.IsNull)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double factor, per, life, salvage, cost;

			// now get the values as doubles
			if (!factorVal.ToDouble(out factor)		||
				!periodVal.ToDouble(out per)		||
				!lifeVal.ToDouble(out life)			||
				!salvageVal.ToDouble(out salvage)	||
				!costVal.ToDouble(out cost))
			{
				// if one could not be coerced then return #Value
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			}

			double adjBase = cost*Math.Pow(1-factor/life, per-1);
			double dep = adjBase - (adjBase * (1-factor/life));
			if (salvage >adjBase-dep)
			{
				if (adjBase-salvage > 0)
					dep = adjBase-salvage;
				else 
					dep = 0;
			}

			return new UltraCalcValue(dep);

		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "ddb"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 4; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 5; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionDDB

	#region UltraCalcFunctionIntRate
    /// <summary>
    /// Calculates the interest rate yielded by a security
    /// investment redeemable at a future date, such as a
    /// zero coupon bond.
    /// </summary>
    /// <remarks>
    /// <p class="body">INTRATE(settlementDate, maturityDate,
    /// amount, redemptionValue, basis)</p>
    /// <p class="body">SettlementDate is the date on which a
    /// security purchase is settled with the buyer taking
    /// possession of the security. Market conventions for
    /// settlement of trades vary by security and exchange.
    /// The settlement date may be substantially later than
    /// a security's issue date when it is traded on the
    /// secondary market.</p>
    /// <p class="body">MaturityDate is the date when the
    /// security can be redeemed. It ceases to accrue any
    /// further value after this date. The maturity date
    /// must be later than the <em>settlementDate</em> or
    /// an error value is returned.</p>
    /// <p class="body">Amount is the purchase price of the
    /// security. For positive interest, this amount will be
    /// smaller than the <em>redemptionValue</em> because it
    /// discounts interest that will be accrued over the time
    /// period the security is held.</p>
    /// <p class="body">RedemptionValue is the price a security
    /// holder may redeem their security for at the <em>maturityDate</em>.
    /// In some cases, this may be called the face value of the
    /// security.</p>
    /// <p class="body">Basis describes what accounting convention to
    /// use when counting days per calendar year, and days on which
    /// interest can accrue. If omitted, a basis consistent with
    /// United States National Association of Security Dealers (NASD)
    /// of 30/360 will be employed.</p>
    /// <table>
    /// <colgroup><col width="100px"/><col width="240px"/></colgroup>
    /// <tr><th>Basis</th><th>Day counting method</th></tr>
    /// <tr><td>0</td><td>30/360 (US NASD)</td></tr>
    /// <tr><td>1</td><td>Actual/actual</td></tr>
    /// <tr><td>2</td><td>Actual/360</td></tr>
    /// <tr><td>3</td><td>Actual/365</td></tr>
    /// <tr><td>4</td><td>30/360 (European)</td></tr>
    /// </table>
    /// <p class="body">Date values should be passed to this function using either the
    /// <see cref="UltraCalcFunctionDate">DATE()</see> function or <strong>UltraCalcValue</strong>
    /// objects containing .NET <strong>DateTime</strong> values. Date values represented
    /// as text may not be interpreted as you had intended.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIntRate : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			double basis = 0;

			if (argumentCount > 4 && !numberStack.Pop().ToDouble(out basis))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			if (basis < 0 || basis > 4)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			double redemption, investment;

			if (!numberStack.Pop().ToDouble(out redemption) ||
				!numberStack.Pop().ToDouble(out investment) )
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			if (investment <= 0 || redemption <= 0)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			UltraCalcValue maturity = numberStack.Pop();

			// MD 4/6/12 - TFS101506
			//DateTime dateMaturity = maturity.ToDateTime(CultureInfo.InvariantCulture);
			DateTime dateMaturity = maturity.ToDateTime();

			UltraCalcValue settlement = numberStack.Pop();

			// MD 4/6/12 - TFS101506
			//DateTime dateSettlement	= settlement.ToDateTime(CultureInfo.InvariantCulture);
			DateTime dateSettlement = settlement.ToDateTime();

			if (dateSettlement >= dateMaturity)
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// Calculate the number of days in a year
			double daysInYear;
			double dim;
			if (basis == 0  || basis == 4)
			{
				// Calculate number of days between settlement and maturity
				// MD 3/2/12 - 12.1 - Table Support
				// Moved the UltraCalcFunctionDays360 logic to a helper method so we don't have to deal with the number stack,
				//numberStack.Push(settlement);
				//numberStack.Push(maturity);
				//numberStack.Push(basis != 4 ? new UltraCalcValue(false) : new UltraCalcValue(true));
				//UltraCalcFunctionDays360 funcDays360 = new UltraCalcFunctionDays360();
				//funcDays360.PerformEvaluation(numberStack, 3);
				//
				//if (!numberStack.Pop().ToDouble(out dim))
				//    return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
				UltraCalcValue result = UltraCalcFunctionDays360.EvaluateHelper(dateSettlement, dateMaturity, basis != 4 ? false : true);
				if (result.ToDouble(out dim) == false)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

				daysInYear = 360;
			}
			else  
			{
				// Use actual difference in days between dates 
				if (basis == 1)
				{
					daysInYear = 365.2422;
					
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)
					
				} 
				else
					if (basis == 2)
				{
					daysInYear = 360;
				} 
				else 
				{
					daysInYear = 365;
				}
				dim = dateMaturity.Subtract(dateSettlement).Days;
			}

			return new UltraCalcValue(((redemption-investment)/investment)*(daysInYear/dim));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "intrate"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 4; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 5; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIntRate

	#region UltraCalcFunctionDateValue
    /// <summary>
    /// Returns the .NET DateTime of the time represented by date formated in a string
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// DATEVALUE(date_text)
    /// <p></p>
    /// Date_text is text that represents a date in a .NET DateTime format
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionDateValue : BuiltInFunctionBase
	{

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 4/6/12 - TFS101506
			//DateTime dt = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime dt = numberStack.Pop().ToDateTime();

			
			return new UltraCalcValue(dt);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "datevalue"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionDateValue

	#region UltraCalcFunctionDate
	/// <summary>
	/// Returns the .NET DateTime ticks number that represents a particular date
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// DATE(year,month,day)
	/// <p></p>
	/// Year is the number representing the year 
	/// <p></p>
	/// Month is a number representing the month of the year
	/// <p></p>
	/// Day is a number representing the day of the month
	/// </p>
	/// </remarks>

	internal





        class UltraCalcFunctionDate : BuiltInFunctionBase
	{

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 11/22/10
			// Found while fixing TFS31956
			// Error values were not being percolated correctly.
			//int day = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			//int month = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			//int year = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			UltraCalcValue dayValue = numberStack.Pop();
			UltraCalcValue monthValue = numberStack.Pop();
			UltraCalcValue yearValue = numberStack.Pop();

			if (yearValue.IsError)
				return yearValue;

			double yearRaw;
			if (yearValue.ToDouble(out yearRaw) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			if (monthValue.IsError)
				return monthValue;

			double monthRaw;
			if (monthValue.ToDouble(out monthRaw) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			if (dayValue.IsError)
				return dayValue;

			double dayRaw;
			if (dayValue.ToDouble(out dayRaw) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			int year = (int)Math.Floor(yearRaw);
			int month = (int)Math.Floor(monthRaw);
			int day = (int)Math.Floor(dayRaw);

			// MD 8/26/11 - TFS84363
			// If the year is less than 1900 in Excel, the year indicates an offset from the year 1900.
			if (UltraCalcValue.UseExcelFunctionCompatibility)
			{
				const int BaseYear = 1900;

				if (year < BaseYear)
					year += BaseYear;
			}

			// MD 11/22/10 - TFS31956
			// The DATE function allows the month and day parameters are allowed to rollover ot the nect year or month.
			while (true)
			{
				int monthIndex = (month - 1);

				int extraYears = monthIndex / 12;
				if (extraYears > 0)
				{
					year += extraYears;
					month = (monthIndex % 12) + 1;
				}

				// MD 8/26/11 - TFS84363
				// If we have gone over year 9999, the call to DaysInMonth will throw an exception and we're alreayd over the year 
				// limit anyway, so return a #NUM! error.
				if (10000 <= year)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

				int maxDay = DateTime.DaysInMonth(year, month);

				if (day <= maxDay)
					break;

				day -= maxDay;
				month++;
			}

			// MD 8/26/11 - TFS84363
			// We need to do some more error checking before returning a value. Out of range years should return a #NUM! error.
			if (UltraCalcValue.UseExcelFunctionCompatibility)
			{
				if (year < 1900)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));







				if (10000 <= year)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
			}
			else
			{
#pragma warning disable 0162
				if (year < 0)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
#pragma warning restore 0162
			}

			DateTime dt = new DateTime(year, month, day);

			
			return new UltraCalcValue(dt);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "date"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionDate

	#region UltraCalcFunctionDays360
	/// <summary>
	/// Returns the number of days between two dates based on a 360-day year (twelve 30-day months), which is used in some accounting calculations.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// DAYS360(start_date,end_date,method)
	/// <p></p>
	/// Start_date and end_date are the two dates between which you want to know the number of days. 
	/// <p></p>
	/// If start_date occurs after end_date, DAYS360 returns a negative number. 
	/// Dates should be entered by using the DATE function, or as results of other formulas or functions. 
	/// For example, use DATE(2008,5,23) for the 23rd day of May, 2008. Problems can occur if dates are entered as text.
	/// <p></p>
	/// Method is a logical value that specifies whether to use the U.S. or European method in the calculation:
	/// <p></p><t></t>
	/// FALSE or omitted U.S. (NASD) method. If the starting date is the 31st of a month, it becomes equal to the 30th of the same month. If the ending date is the 31st of a month and the starting date is earlier than the 30th of a month, the ending date becomes equal to the 1st of the next month; otherwise the ending date becomes equal to the 30th of the same month. 
	/// <p></p><t></t>
	/// TRUE European method. Starting dates and ending dates that occur on the 31st of a month become equal to the 30th of the same month. 
	/// </p>
	/// </remarks>

	internal





        class UltraCalcFunctionDays360 : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			bool method = false;

			if (argumentCount > 2)
			{
				if (numberStack.Peek().IsError)
					return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

				// MD 4/6/12 - TFS101506
				//method = numberStack.Pop().ToBoolean(CultureInfo.InvariantCulture);
				method = numberStack.Pop().ToBoolean();
			}

			
			// MD 4/6/12 - TFS101506
			//DateTime endDate = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			//DateTime startDate = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime endDate = numberStack.Pop().ToDateTime();
			DateTime startDate = numberStack.Pop().ToDateTime();

			// MD 3/2/12 - 12.1 - Table Support
			// Moved this code to a helper method.
			//if (startDate.Day == 31) 
			//    startDate = new DateTime(startDate.Year, startDate.Month, 30);
			//
			//if (endDate.Day == 31)
			//    if (!method) 
			//        if (startDate.Day < 30) 
			//            endDate = new DateTime(endDate.Month < 12 ? endDate.Year : endDate.Year+1, endDate.Month < 12 ? endDate.Month+1 : 1, 1);
			//        else 
			//            endDate = new DateTime(endDate.Year, endDate.Month, 30);
			//    else 
			//        endDate = new DateTime(endDate.Year, endDate.Month, 30);
			//
			//return new UltraCalcValue((endDate.Year * 360 + endDate.Month * 30 + endDate.Day) - (startDate.Year * 360 + startDate.Month * 30 + startDate.Day));

			return UltraCalcFunctionDays360.EvaluateHelper(startDate, endDate, method);
		}

		// MD 3/2/12 - 12.1 - Table Support
		internal static UltraCalcValue EvaluateHelper(DateTime startDate, DateTime endDate, bool method)
		{
			if (startDate.Day == 31)
				startDate = new DateTime(startDate.Year, startDate.Month, 30);

			if (endDate.Day == 31)
				if (!method)
					if (startDate.Day < 30)
						endDate = new DateTime(endDate.Month < 12 ? endDate.Year : endDate.Year + 1, endDate.Month < 12 ? endDate.Month + 1 : 1, 1);
					else
						endDate = new DateTime(endDate.Year, endDate.Month, 30);
				else
					endDate = new DateTime(endDate.Year, endDate.Month, 30);

			return new UltraCalcValue((endDate.Year * 360 + endDate.Month * 30 + endDate.Day) - (startDate.Year * 360 + startDate.Month * 30 + startDate.Day));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "days360"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionDays360

	#region UltraCalcFunctionDay
	/// <summary>
	/// Returns the day of a date value
	/// </summary>

	internal





        class UltraCalcFunctionDay : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			if (numberStack.Peek().IsError)
				return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

			
			// MD 4/6/12 - TFS101506
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime date = numberStack.Pop().ToDateTime();

			return new UltraCalcValue(date.Day);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "day"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionDay

    #region UltraCalcFunctionMonth
    /// <summary>
	/// Returns the month of a date value
	/// </summary>

	internal





        class UltraCalcFunctionMonth : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			if (numberStack.Peek().IsError)
				return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

			
			// MD 4/6/12 - TFS101506
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime date = numberStack.Pop().ToDateTime();
			return new UltraCalcValue(date.Month);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "month"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionMonth

	#region UltraCalcFunctionYear
	/// <summary>
	/// Returns the year of a date value
	/// </summary>

	internal





        class UltraCalcFunctionYear : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			if (numberStack.Peek().IsError)
				return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

			
			// MD 4/6/12 - TFS101506
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime date = numberStack.Pop().ToDateTime();
			return new UltraCalcValue(date.Year);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "year"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionYear

	#region UltraCalcFunctionHour
	/// <summary>
	/// Returns the hour of a time value
	/// </summary>

	internal





        class UltraCalcFunctionHour : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			if (numberStack.Peek().IsError)
				return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

			
			// MD 4/6/12 - TFS101506
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime date = numberStack.Pop().ToDateTime();

			return new UltraCalcValue(date.Hour);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "hour"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionHour

	#region UltraCalcFunctionMinute
	/// <summary>
	/// Returns the minute of a time value
	/// </summary>

	internal





        class UltraCalcFunctionMinute : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			if (numberStack.Peek().IsError)
				return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

			
			// MD 4/6/12 - TFS101506
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime date = numberStack.Pop().ToDateTime();

			return new UltraCalcValue(date.Minute);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "minute"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionMinute

	#region UltraCalcFunctionSecond
	/// <summary>
	/// Returns the second of a time value
	/// </summary>

	internal





        class UltraCalcFunctionSecond : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			if (numberStack.Peek().IsError)
				return new UltraCalcValue( numberStack.Pop().ToErrorValue() );

			
			// MD 4/6/12 - TFS101506
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime date = numberStack.Pop().ToDateTime();

			return new UltraCalcValue(date.Second);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "second"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSecond

	#region UltraCalcFunctionNow
	/// <summary>
	/// Returns a .NET DateTime ticks value of the current day and time
	/// </summary>

	internal





        class UltraCalcFunctionNow : BuiltInFunctionBase
	{
		#region Properties


		#region IsAlwaysDirty
		/// <summary>
		/// Indicates whether the results of the function is always dirty.
		/// </summary>
		public override bool IsAlwaysDirty
		{
			get { return true; }
		}
		#endregion //IsAlwaysDirty 


		#endregion //Properties

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			
			return new UltraCalcValue(DateTime.Now);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "now"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionNow

	#region UltraCalcFunctionTimeValue
	/// <summary>
	/// Returns the .Net DateTime of the time represented by time formated in a string
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// TIMEVALUE(time_text)
	/// <p></p>
	/// Time_text is text that represents a date in the .NET DateTime format
	/// </p>
	/// </remarks>

	internal





        class UltraCalcFunctionTimeValue : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			//if (numberStack.Peek().IsError)
			//    return new UltraCalcValue( numberStack.Pop().ToErrorValue() );
			//
			////~ AS 9/7/04 Changed existing datetime functions to use/return a DateTime instead of ticks.
			//DateTime dt = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			//return new UltraCalcValue( new DateTime(dt.TimeOfDay.Ticks) );
			UltraCalcValue argument = numberStack.Pop();

			if ( argument.IsError )
				return argument;

			
			// MD 4/6/12 - TFS101506
			//TimeSpan time = argument.ToDateTime( CultureInfo.InvariantCulture ).TimeOfDay;
			TimeSpan time = argument.ToDateTime().TimeOfDay;

			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if ( UltraCalcValue.UseExcelFormulaCompatibility )
			if ( UltraCalcValue.UseExcelValueCompatibility )
			{
				DateTime dateTime = new DateTime( 1, 1, 1, time.Hours, time.Minutes, time.Seconds );

				// MD 8/26/11 - TFS84363
				// Moved this code into the UltraCalcFunction.GetWorkbook static method and called it below.
//#if EXCEL
//                ExcelRefBase excelRef = numberStack.FormulaOwner as ExcelRefBase;
//                Debug.Assert( excelRef != null, "The formula must be owned by an ExcelRefBase." );
//                Workbook workbook = excelRef == null
//                    ? null
//                    : excelRef.Workbook;
//#endif

				object value = UltraCalcValue.DateTimeToExcelDate(





					dateTime, false );

				return new UltraCalcValue( value );
			}

#pragma warning disable 0162
			return new UltraCalcValue(new DateTime(time.Ticks)); 
#pragma warning restore 0162
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "timevalue"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionTimeValue

	#region UltraCalcFunctionTime
	/// <summary>
	/// Returns the .NET DateTime ticks value that represents a particular time
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// TIME(hour,minute,second)
	/// <p></p>
	/// Hour is a number representing the time's hour
	/// <p></p>
	/// Minute is a number representing the time's minute
	/// <p></p>
	/// Second is a number representing the time's second
	/// </p>
	/// </remarks>

	internal





        class UltraCalcFunctionTime : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 4/15/09 - TFS16390
			// We need to percolate up the error values.
			//int second = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			//int minute = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			//int hour = numberStack.Pop().ToInt32(CultureInfo.InvariantCulture);
			UltraCalcValue secondArg = numberStack.Pop();
			UltraCalcValue minuteArg = numberStack.Pop();
			UltraCalcValue hourArg = numberStack.Pop();

			if ( hourArg.IsError )
				return hourArg;

			if ( minuteArg.IsError )
				return minuteArg;

			if ( secondArg.IsError )
				return secondArg;

			// MD 4/6/12 - TFS101506
			//int second = secondArg.ToInt32( CultureInfo.InvariantCulture );
			//int minute = minuteArg.ToInt32( CultureInfo.InvariantCulture );
			//int hour = hourArg.ToInt32( CultureInfo.InvariantCulture );
			int second = secondArg.ToInt32();
			int minute = minuteArg.ToInt32();
			int hour = hourArg.ToInt32();

			// MD 4/15/09 - TFS16390
			// The Excel TIME function is much more robust than our calc manager implementation.
			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if ( UltraCalcValue.UseExcelFormulaCompatibility )
			if ( UltraCalcValue.UseExcelFunctionCompatibility )
			{
				int maxValue = Int16.MaxValue;
				if ( second < 0 || minute < 0 || hour < 0 ||
					maxValue < second || maxValue < minute || maxValue < hour )
				{
					return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Num ) );
				}

				// Add additional minutes in the seconds portion to the minutes and truncate the seconds.
				minute += ( second / 60 );
				second = second % 60;

				// Add additional hours in the minutes portion to the hours and truncate the minutes.
				hour += ( minute / 60 );
				minute = minute % 60;

				// Discard additional days in the hours portion and truncate the hours.
				hour = hour % 24;
			}

			DateTime dt = new DateTime(1, 1, 1, hour, minute, second);

			// MD 4/15/09 - TFS16390
			// This is the wrong thing to return when solving Excel formulas. Use the correct was in Excel and allow
			// calc manager to switch over to using the Excel way of solving the formula.
			//return new UltraCalcValue(dt.Ticks);
			// SSP 8/22/11 - XamCalculationManager
			// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
			// 
			//if ( UltraCalcValue.UseExcelFormulaCompatibility )
			if ( UltraCalcValue.UseExcelValueCompatibility )
			{
				// MD 8/26/11 - TFS84363
				// Moved this code into the UltraCalcFunction.GetWorkbook static method and called it below.
//#if EXCEL
//                ExcelRefBase excelRef = numberStack.FormulaOwner as ExcelRefBase;
//                Debug.Assert( excelRef != null, "The formula must be owned by an ExcelRefBase." );
//                Workbook workbook = excelRef == null
//                    ? null
//                    : excelRef.Workbook; 
//#endif

				object value = UltraCalcValue.DateTimeToExcelDate(





					dt, false );

				return new UltraCalcValue( value );
			}
			else
			{
#pragma warning disable 0162
				return new UltraCalcValue( dt.Ticks );
#pragma warning restore 0162
			}
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "time"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionTime

	#region UltraCalcFunctionMedian
    /// <summary>
    /// Calculates the median value of a specified list of numeric values.
    /// </summary>
    /// <remarks>
    /// <p class="body">MEDIAN(Value1, value2, ..., valueN)</p>
    /// <p class="body">Value1, value2, ..., valueN are any number of numeric values or references to
    /// numeric values for which you want the median value found. If any argument is an error value,
    /// or there are no numeric values within the series of arguments, then MEDIAN() will return an
    /// error value.</p>
    /// <p class="body">The median is the middle value of the sorted list of numeric values (you do
    /// not need to sort <em>Value1</em>, <em>value2</em>, ..., <em>valueN</em> prior to passing them
    /// to the MEDIAN() function). When the list contains an odd number of values, the median will be
    /// the value at position CEIL(<em>N</em>/2). For example, the median of the 5-value list 0, 20,
    /// 30, 50, 80, is the value in the third position: 30. When the list contains an even number of
    /// values, the median will be the average of the two values in the middle. For example, the
    /// median of the 4-value list 7, 20, 30, 45, is the average of the two middle elements (20 and
    /// 30): 25. Note that the value of the MEDIAN() varies most from the arithmetic mean of the
    /// sorted list when the distribution of values tends to favor one side or the other of that
    /// arithemtic mean. For example, the arithmetic mean of the 5-value list 1, 2, 3, 21, 43 is
    /// 14 but it's median value is 3.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionMedian : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
            List<double> valueArray = new List<double>();

			// if it fails, then return the error it encountered
			if( !UltraCalcFunction.PopArrayList(numberStack, argumentCount, valueArray, true, true) ) 
				return numberStack.Pop();

			valueArray.Sort();

			// if we didn't find any numbers, push an error
			if( valueArray.Count == 0 ) 
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			// if the number of values is odd, push the middle value, otherwise push the avg of the two
			// middle values
			double median;
			int index = valueArray.Count / 2;

			if( valueArray.Count % 2 == 1 ) 
				median = (double)valueArray[index];
			else 
			{
				double d = ((double)valueArray[index - 1] + (double)valueArray[index]) / 2;
				median = d;
			}

			return new UltraCalcValue(median);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "median"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionMedian

	#region UltraCalcFunctionVar
    /// <summary>
    /// Calculates the estimated variance for a specified sampling of numeric values.
    /// </summary>
    /// <remarks>
    /// <p class="body">VAR(Value1, value2, ..., valueN)</p>
    /// <p class="body">Value1, value2, ..., valueN are any number of numeric values or
    /// references to numeric values you provide to specify a sampling of your larger
    /// data population. If any argument contains an error value or there arte no numeric
    /// values in the sampling, the VAR() function evaluates to the first error value it
    /// encounters.</p>
    /// <p class="body">Variance is a measure of statistical variability, and is used in
    /// statistical studies based on small samplings from much larger data populations to
    /// draw inferences about the variability of those data populations.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionVar : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			return CalculateVariance( numberStack, argumentCount );
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		/// <summary>
		/// Estimates the variance based on a sample.
		/// </summary>
		/// <param name="numberStack">Number stack to evaluate</param>
		/// <param name="argumentCount">Number of arguments on the stack to use</param>
		/// <returns>A boolean indicating if the value was calculated</returns>
		protected UltraCalcValue CalculateVariance(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 2/14/11 - TFS66313
			// Moved all code to EvaluateHelper so it could be used in other places.
			//List<double> valueArray = new List<double>();
			//
			//if( !UltraCalcFunction.PopArrayList(numberStack, argumentCount, valueArray, true, true) ) 
			//    return numberStack.Pop();
			//
			//// if we didn't find any numbers, push an error
			//if( valueArray.Count == 0 ) 
			//    return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			//
			//// if there is only one we cannot calculate the variance
			//if (valueArray.Count == 1)
			//    return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Div) );
			//
			//// calculate the average
			//double average = 0;
			//foreach( double d in valueArray )
			//    average += d;
			//
			//average = average / valueArray.Count;
			//
			//// calculate (V - average) ^ 2
			//double accum = 0;
			//foreach( double d in valueArray )
			//{
			//    double delta  = d - average;
			//    accum += delta * delta;
			//}
			//
			//// divide by count -1 to get the variance
			//double var = accum / (valueArray.Count - 1);
			//
			//return new UltraCalcValue( var );

			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			return EvaluateHelper(args);
		}

		// MD 2/14/11 - TFS66313
		// Moved this code from Evaluate so it could be used in other places.
		internal static UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			// if we didn't find any numbers, push an error
			if (args.Count == 0)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			// if there is only one we cannot calculate the variance
			if (args.Count == 1)
			{
				// MD 4/10/12v
				// Found while fixing TFS108678
				// We need to percolate up any errors.
				if (args[0].IsError)
					return args[0];

				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Div));
			}

			// calculate the average
			double average = 0;
			foreach (UltraCalcValue calcValue in args)
			{
				if (null != calcValue)
				{
					if (calcValue.IsError)
						return new UltraCalcValue(calcValue.ToErrorValue());

					double d;
					if (calcValue.ToDouble(out d))
					{
						average += d;
					}
					else
					{
						return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
					}
				}
			}

			average = average / args.Count;

			// calculate (V - average) ^ 2
			double accum = 0;
			foreach (UltraCalcValue calcValue in args)
			{
				if (null != calcValue)
				{
					if (calcValue.IsError)
						return new UltraCalcValue(calcValue.ToErrorValue());

					double d;
					if (calcValue.ToDouble(out d))
					{
						double delta = d - average;
						accum += delta * delta;
					}
					else
					{
						return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
					}
				}
			}

			// divide by count -1 to get the variance
			double var = accum / (args.Count - 1);

			return new UltraCalcValue(var);
		}
	
		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "var"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionVar

	#region UltraCalcFunctionStdev
    /// <summary>
    /// Calculates an approximate standard deviation of a specified sampling of numeric
    /// values.
    /// </summary>
    /// <remarks>
    /// <p class="body">STDEV(Value1, value2, ..., valueN)</p>
    /// <p class="body">Value1, value2, ..., valueN are any number of numeric values or references
    /// to numeric values that you've given to provide a sample space of your data set. If any value
    /// contains an error, then the function evaluates to the first error encountered. An error value
    /// is returned if the sample space contains no numeric values.</p>
    /// <p class="body">The standard deviation is used in statistical studies to make inferences about
    /// a larger population of data based on sampling only a subset. For well chosen samplings, a small
    /// standard deviation indicates most data points tend to cluster within a narrow range of values.
    /// A larger standard deviations indicates greater variability in the data points, and that there
    /// is a higher likelihood for values to occur farther away and/or more frequently away from the
    /// expected norm.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionStdev : UltraCalcFunctionVar
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 2/14/11 - TFS66313
			// Moved all code to EvaluateHelper so it could be used in other places.
			//UltraCalcValue varianceValue = this.CalculateVariance(numberStack, argumentCount);
			//
			//if (varianceValue.IsError)
			//    return varianceValue;
			//
			//double varVal;
			//
			//if (!varianceValue.ToDouble(out varVal))
			//    return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			//
			//return new UltraCalcValue( Math.Sqrt(varVal) );
			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			return EvaluateHelper(args);
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		// MD 2/14/11 - TFS66313
		// Moved this code from Evaluate so it could be used in other places.
		internal static new UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			UltraCalcValue varianceValue = UltraCalcFunctionVar.EvaluateHelper(args);

			if (varianceValue.IsError)
				return varianceValue;

			double varVal;

			if (!varianceValue.ToDouble(out varVal))
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			return new UltraCalcValue(Math.Sqrt(varVal));
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "stdev"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionStdev

	#region UltraCalcFunctionType
    /// <summary>
    /// Returns the underlying UltraCalc data type for the specified value.
    /// </summary>
    /// <remarks>
    /// <p class="body">TYPE( value)</p>
    /// <p class="body">Value is an <strong>UltraCalcValue</strong> object or
    /// the result of evaluating an UltraCalc expression.</p>
    /// <p class="body">
    /// <table border="0">
    /// <colgroup><col width="160px"/><col width="160px"/></colgroup>
    /// <thead><th>UltraCalc data type:</th><th>TYPE function returns:</th></thead>
    /// <tbody>
    /// <tr><td>Number</td><td>1</td></tr>
    /// <tr><td>Text</td><td>2</td></tr>
    /// <tr><td>Boolean</td><td>4</td></tr>
    /// <tr><td>Error</td><td>16</td></tr>
    /// </tbody>
    /// </table>
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionType : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{

			int type;
			UltraCalcValue value = numberStack.Pop();
			if (value.IsError)
				type = 16;
			else if (value.IsBoolean)
				type = 4;
			else if (value.IsString)
				type = 2;





			else 
			{
				double d;
				
				if (!value.ToDouble(out d))
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
					
				type = 1;
			}

			return new UltraCalcValue(type);

		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "type"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionType

	#region UltraCalcFunctionErrorType
    /// <summary>
    /// When an error value is returned, the ERRORTYPE function returns a
    /// numeric value corresponding to the UltraCalcErrorCode enumeration.
    /// </summary>
    /// <remarks>
    /// <p class="body">ERRORTYPE(error_value)</p>
    /// <p class="body">Error_value is an UltraCalc error value resulting
    /// from the unsuccessful evaluation of an expression.</p>
    /// <p class="body">The ERRORTYPE function returns an error value of
    /// #N/A (Not Applicable) if you pass it an UltraCalc value that was
    /// not an error value.</p>
    /// <p class="body">
    /// <table border="0">
    /// <thead>
    /// <th>Error_value</th>
    /// <th>ERRORTYPE returns</th>
    /// </thead>
    /// <tbody>
    /// <tr><td>#NULL!</td><td>1</td></tr>
    /// <tr><td>#DIV/0!</td><td>2</td></tr>
    /// <tr><td>#VALUE!</td><td>3</td></tr>
    /// <tr><td>#REF!</td><td>4</td></tr>
    /// <tr><td>#NAME?</td><td>5</td></tr>
    /// <tr><td>#NUM!</td><td>6</td></tr>
    /// <tr><td>#N/A!</td><td>7</td></tr>
    /// </tbody>
    /// </table>
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionErrorType : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int type;
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
			{
				// MRS 6/6/05 - BR03428
				//UltraCalcErrorValue errorValue = (UltraCalcErrorValue)value.Value;
				UltraCalcErrorValue errorValue = value.ToErrorValue();

				switch (errorValue.Code)
				{
						//						case UltraCalcErrorCode.Null:
						//							type = 1;
						//							break;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


					case UltraCalcErrorCode.Div:
						type = 2;
						break;

					case UltraCalcErrorCode.Value:
						type = 3;
						break;

					case UltraCalcErrorCode.Reference:
						type = 4;
						break;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


					case UltraCalcErrorCode.Num:
						type = 6;
						break;

					case UltraCalcErrorCode.NA:
						type = 7;
						break;

					default:
						return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.NA));
				}
				return new UltraCalcValue(type);
			} 
			else 
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.NA) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "errortype"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionErrorType

	#region UltraCalcFunctionNa
    /// <summary>
    /// Returns #N/A!, the error value representing a not applicable result.
    /// </summary>
    /// <remarks>
    /// <p class="body">NA()</p>
    /// <p class="body">The NA function always returns the same constant
    /// error value.</p>
    /// <p class="body">You might use this function when you wanted an IF
    /// function or other complex UltraCalc expression to return the #N/A!
    /// error value.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionNa : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.NA) ) ;
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "na"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionNa

	#region UltraCalcFunctionIsBlank
    /// <summary>
    /// Returns TRUE if the specified value is blank.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISBLANK(value)</p>
    /// <p class="body">Value can be any constant, reference or the result of
    /// evaluating an UltraCalc expression.</p>
    /// <p class="body">The definition of what constitutes "blank" is anything
    /// that causes the <strong>IsNull</strong> method of the <strong>UltraCalcValue</strong>
    /// object containing the argument, value, to return TRUE. This may vary
    /// for different kinds of <strong>UltraCalcValue</strong> object.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIsBlank : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();
			return new UltraCalcValue(value.IsNull);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "isblank"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsBlank

	#region UltraCalcFunctionIsErr
    /// <summary>
    /// Returns TRUE if the specified value is any error value, except #N/A!
    /// </summary>
    /// <remarks>
    /// <p class="body">ISERR(value)</p>
    /// <p class="body">Value is an <strong>UltraCalcValue</strong> object (perhaps
    /// the result of evaluating an UltraCalc expression) thought to be an error
    /// value. The ISERR function will return FALSE when value is not an error
    /// value, or it is an error but the error value was not applicable.</p>
    /// <p class="body">This is a weaker variation of the more stringent
    /// <see cref="UltraCalcFunctionIsError">ISERROR</see> function.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIsErr : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();
			return new UltraCalcValue(value.IsError && value.ToErrorValue().Code != UltraCalcErrorCode.NA);

		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "iserr"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsErr

	#region UltraCalcFunctionIsError
    /// <summary>
    /// Returns TRUE if the specified value is any error value without exception.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISERROR(value)</p>
    /// <p class="body">Value is an <strong>UltraCalcValue</strong> object (perhaps
    /// the result of evaluating an UltraCalc expression) thought to be an error
    /// value. The ISERROR function will return FALSE only when value is not an
    /// error value.</p>
    /// <p class="body">A more relaxed variation of this function is the
    /// <see cref="UltraCalcFunctionIsErr">ISERR</see> function, which
    /// returns TRUE when an error value is not applicable. The ISERROR
    /// function treats this case as being an error. If you are using
    /// certain UltraCalc functions then sometimes error values of #N/A!
    /// should be treated by your application as non-errors and you
    /// should choose ISERR instead of ISERROR.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIsError : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();
			return new UltraCalcValue(value.IsError);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula.
		/// </summary>
		public override string Name
		{ 
			get { return "iserror"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function.
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function.
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsError

	#region UltraCalcFunctionIsLogical
    /// <summary>
    /// Returns TRUE if the specified value refers to a two-state logic
    /// or Boolean value.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISLOGICAL(value)</p>
    /// <p class="body">Value is an <strong>UltraCalcValue</strong> object,
    /// or the result of evaluating an UltraCalc expression, which is thought
    /// to be a boolean value (TRUE or FALSE.) Boolean values are named in
    /// honor of George Boole, whose development of Boolean algebra governs
    /// the special characteristics of logical functions upon these two
    /// values.</p>
    /// <p class="body">Depending on the kind of value, it may not be convertible
    /// into a boolean value and therefore the ISLOGICAL function returns FALSE.
    /// The ISLOGICAL function only furnishes information about the compatibility
    /// of a value with the boolean-typed values evaluated by UltraCalc.</p>
    /// <p class="note">The ISLOGICAL function does not perform deductive reasoning.
    /// A return value of TRUE should not be interpreted as proof any value or UltraCalc
    /// expression passed to the ISLOGICAL function represents a logical outcome.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIsLogical : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			return new UltraCalcValue(value.IsBoolean);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "islogical"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsLogical

	#region UltraCalcFunctionIsNa
    /// <summary>
    /// Returns TRUE if an error value is #N/A (not applicable.)
    /// </summary>
    /// <remarks>
    /// <p class="body">ISNA(value)</p>
    /// <p class="body">Value is an error value thought to be the error value
    /// returned when an error has happened, but the error code was not applicable.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIsNa : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();
			return new UltraCalcValue(value.IsError && value.ToErrorValue().Code == UltraCalcErrorCode.NA);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula.
		/// </summary>
		public override string Name
		{ 
			get { return "isna"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function.
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function.
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsNa

	#region UltraCalcFunctionIsNonText
    /// <summary>
    /// Returns TRUE if the specified value refers to any not-text value.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISNONTEXT( value)</p>
    /// <p class="body">Value is an <strong>UltraCalcValue</strong> object
    /// or the result of an UltraCalc expression evaluation that you want
    /// to test to determine whether it is a text value (such as a .NET
    /// <strong>String</strong> object) or a non-text value.</p>
    /// <seealso cref="UltraCalcFunctionIsText">ISTEXT function</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionIsNonText : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();
			return new UltraCalcValue(!value.IsString);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "isnontext"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsNonText

	#region UltraCalcFunctionIsNumber
    /// <summary>
    /// Returns TRUE if the specified value refers to a numeric value.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISNUMBER( value)</p>
    /// <p class="body">Value is an <strong>UltraCalcValue</strong> or the
    /// result of evaluating an UltraCalc expression that you want to test
    /// to determine whether it is a number or not. A numeric value can be
    /// an integer, floating-point or decimal number.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIsNumber : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			int stackLevel = numberStack.Count();

			UltraCalcValue value = numberStack.Pop();

			// MD 2/3/12 - TFS101005
			// In Excel, when the ISNUMBER function is called on an empty cell, it returns False.
			if (UltraCalcValue.UseExcelValueCompatibility)
			{
				if (value.IsNull)
					return new UltraCalcValue(false);
			}

			// SSP 11/9/04 UWC168
			// If the underlying value can be converted to a number then return true. In
			// other words if the underlying value is a string like "123" that can be
			// converted to a double then we should consider it a number.
			// 
			// ----------------------------------------------------------------------------
			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			double d;
			return new UltraCalcValue( value.ToDouble( out d ) );
			// ----------------------------------------------------------------------------
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "isnumber"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsNumber

	#region UltraCalcFunctionIsRef
    /// <summary>
    /// Returns TRUE if the specified value is an UltraCalc reference.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISREF( reference)</p>
    /// <p class="body">Reference is an <strong>UltraCalcValue</strong>
    /// object that contains a reference to some application-specific
    /// control, field, or piece of information. You can think of it as
    /// an address which the UltraCalc engine uses to look-up a value
    /// in your application when one is needed to evaluate an expression.
    /// </p>
    /// <p class="body">The ISREF function returns TRUE when the specified
    /// <strong>UltraCalcValue</strong> is a reference. Since it's value
    /// is defined by the contents at another referenced location, it's
    /// possible for this <strong>UltraCalcValue</strong> object to change
    /// in value based on changes in your application (outside of the
    /// UltraCalc engine.) Proper implementation of the <strong>IUltraCalcReference</strong>
    /// interface and <strong>NotifyValueChange</strong> methods will ensure the
    /// UltraCalc engine is made aware of any changes in a referenced value,
    /// and that all references depending on this value are refreshed.</p>
    /// <p class="body">Conversely, the ISREF function will return FALSE
    /// should the <strong>UltraCalcValue</strong> you supply represent
    /// a constant value or the temporary result of evaluating an UltraCalc
    /// expression (a constant value sitting at the top of the UltraCalc
    /// engine's number stack.)</p>
    /// </remarks>

	internal





        class UltraCalcFunctionIsRef : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();
			return new UltraCalcValue(value.IsReference);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "isref"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsRef

	#region UltraCalcFunctionIsText
    /// <summary>
    /// Returns TRUE if the specified value refers to a text (or string) value.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISTEXT(value)</p>
    /// <p class="body">Value is an <strong>UltraCalcValue</strong> object or
    /// the result of evaluating an UltraCalc expression that you want to test
    /// to determine whether it is of a text (<em>e.g.</em>, string) value or
    /// a non-text value (which could be a numeric, boolean, or error value.)
    /// </p>
    /// <seealso cref="UltraCalcFunctionIsNonText">ISNONTEXT function</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionIsText : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();
			return new UltraCalcValue(value.IsString);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "istext"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsText

	#region UltraCalcFunctionIsEven
    /// <summary>
    /// Returns TRUE if the specified value is an even number.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISEVEN(value)</p>
    /// <p class="body">Value is a number thought to be even (divisible by the
    /// integer 2.) A value that is not an integer will first be converted to
    /// an integer using the <see cref="UltraCalcFunctionFloor">FLOOR</see>
    /// function before evaluation.</p>
    /// <seealso cref="UltraCalcFunctionIsOdd">ISODD function</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionIsEven : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			// truncate the value first
			double number;

			if (!value.ToDouble(out number))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			number = Math.Floor(number);

			return new UltraCalcValue( number % 2 == 0 );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "iseven"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsEven

	#region UltraCalcFunctionIsOdd
    /// <summary>
    /// Returns TRUE if the specified value is an odd number.
    /// </summary>
    /// <remarks>
    /// <p class="body">ISODD(value)</p>
    /// <p class="body">Value is a number thought to be odd (not divisible by
    /// the integer 2.) A value that is not an integer will first be converted
    /// to an integer using the <see cref="UltraCalcFunctionFloor">FLOOR</see>
    /// function before evaluation.</p>
    /// <seealso cref="UltraCalcFunctionIsEven">ISEVEN function</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionIsOdd : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			// truncate the value first
			double number;

			if (!value.ToDouble(out number))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			number = Math.Floor(number);

			// SSP 12/21/04 BR01333
			// Make it work for negative numbers as well.
			//
			//return new UltraCalcValue( number % 2 == 1 );
			int remainder = (int)( number % 2 );
			return new UltraCalcValue( 1 == remainder || -1 == remainder );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "isodd"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsOdd

	#region UltraCalcFunctionIRR
    /// <summary>
    /// Calculates an internal rate of return for a given series of cash flows represented by either positive (incoming) or
    /// negative (outgoing) numeric values.
    /// </summary>
    /// <remarks>
    /// <p class="body">IRR(value_reference, estimate)</p>
    /// <p class="body">Value_reference must be a column or range reference of numeric values representing cash outflows
    /// (payments) as negative numeric values, and cash inflows (income) as positive numeric values. Each cash flow must
    /// occur with a regular period. If payments occur at irregular intervals, then you must represent the cash flow periods
    /// as taking place on a period common to all cash flows, and at intervals without a cash flow specify a zero value. 
    /// For example, if you finance the purchase of a television by taking a $300 loan with monthly payments of $110, but
    /// your payments do not start for three months, your series of cash flows would be monthly but would show three zero
    /// values for the months without payments (+300, 0, 0, 0, -110, -110, -110).</p>
    /// <p class="body">Estimate is an approximation close to what you expect the resulting internal rate of return to
    /// be. This function employs an iterative algorithm starting with this estimate and then repeatedly converging on
    /// a result that has a diminishing margin of error. If you do not provide an <em>estimate</em> then this function
    /// uses 10 percent as it's starting point.</p>
    /// <p class="body">If after twenty iterations the margin of error has not closed to within 1/1000 basis point then
    /// a #NUM error value will be returned.</p>
    /// <p class="body">The IRR() function has applications where you may be given a series of cash flows without an
    /// interest rate, such as in the retail financing example above, and wish to calculate what it's effective rate
    /// of interest would be. The internal rate is based on the period between cash flows, therefore when calculating
    /// payments that are not annual, you must annualize the internal rate.</p>
    /// <seealso cref="UltraCalcFunctionNPV">NPV()</seealso>
    /// </remarks>

	internal





        class UltraCalcFunctionIRR : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// iterate until we have an error < 0.00001%
			double delta = 0.0000001;

			// the default guess is 10%.  If the caller provided a guess, make sure it's bigger than
			// our delta.  If it isn't, go with the default guess.
			double guess = 0.1;

			// SSP 8/6/12 TFS118166
			// Added the if block and enclosed the existing code in the else block. See the notes below.
			// 

			bool hasGuessArgument = false;
			if ( argumentCount >= 2 ) 
			{
				hasGuessArgument = true;




				UltraCalcValue guessArg = numberStack.Pop();

				if (!guessArg.ToDouble(out guess))
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

				if( Math.Abs(guess) < delta )
					guess = 0.1;
			} 

			// calculate 1 + rate
			guess += 1.0;

			// SSP 8/6/12 TFS118166
			// Added the call to GetArguments and enclosed the eixsting code in the #else block.
			// We decided that we did not want to change the behavior of Excel so we are effecting
			// this change only in non-excel case. Before this change the first argument was required
			// to be an enumerable reference that provides the cash-flow values. After this change,
			// we'll allow the cash-flow values to be specified as arguments to the function.
			// 

			List<UltraCalcValue> cashflowList = new List<UltraCalcValue>( argumentCount );
			this.GetArgumentsInOrder( cashflowList, numberStack, argumentCount - ( hasGuessArgument ? 1 : 0 ), false );


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			// set up iteration values
			double fElement = 0;
			double fElementPrime = 0;
			double element = 0;
			// SSP 3/7/05 BR02518
			//double temp;

			for( int iteration = 0;iteration < 20;++iteration ) 
			{
				bool firstValueLoop = true;
				int elementCount = 0;

				// SSP 8/6/12 TFS118166
				// Added the if block and enclosed the existing code into the else block. See notes above.
				// 

				for ( int i = 0, count = cashflowList.Count; i < count; i++ )
				{
					UltraCalcValue val = cashflowList[i];
					if ( ! val.ToDouble( out element ) )
						continue;


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


					elementCount++;

					// first time through, fElement is just element and fElementPrime is 0
					if( firstValueLoop ) 
					{
						fElement = element;
						fElementPrime = 0;
						firstValueLoop = false;
					} 
					else 
					{
						fElementPrime = fElement + (fElementPrime * guess);
						fElement = element + (fElement * guess);
					}
				}

				// if we didn't find enough elements, we have a numeric error
				if( elementCount < 2 )
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

				// calculate the next guess.  If the absolute value is smaller than our delta, we're done
				double nextGuess = fElement / fElementPrime;
				guess -= nextGuess;
				if( Math.Abs(nextGuess) < delta ) 
					return new UltraCalcValue(guess - 1);

				// if our guess goes to 0, we're done
				if( Math.Abs(guess) < delta ) 
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			}

			// if we couldn't converge in 20 iterations we're done.
			return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			// Only the value_reference parameter can be enumerable
			return parameterIndex == 0;
		}

		#endregion  // CanParameterBeEnumerable

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "irr"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get 
			{ 
				// SSP 8/6/12 TFS118166
				// Added the if block and enclosed the existing code in the else block.
				// 

				return int.MaxValue;



			}
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIRR

	#region UltraCalcFunctionRate
    /// <summary>
    /// Calculates the per-period interest rate for a series of cash flows (an annuity).
    /// </summary>
    /// <remarks>
    /// <p class="body">RATE(nPeriods, amount, presentValue, paymentDue, futureValue, estimate)</p>
    /// <p class="body">NPeriods are the total number of cash flows, whether a payment (negative <em>amount</em>)
    /// or a receipt (positive <em>amount</em>) occuring periodically with a fixed time period between each cash
    /// flow.</p>
    /// <p class="body">Amount is the amount of cash paid (negative) or received (positive). It must be held
    /// constant over the course of the annuity.</p>
    /// <p class="body">PresentValue is the value today of the series of future payments. Payments made in the
    /// future are discounted by the interest rate being calculated, because it is assumed that at that interest
    /// rate a smaller sum could be invested today and would grow to the <em>amount</em> at a future time when
    /// that payment became due.</p>
    ///	<p class="body">PaymentDue indicates whether cash flows occur at the beginning of each period (1) or at
    /// the end of each period (0). If not specified, the payments at the end of each period is assumed.</p>
    ///	<p class="body">FutureValue is the accumulated balance attained after <em>nPeriods</em> payments have
    /// been made and accrued interest at the calculated rate. If left unspecified, the default future value
    /// is assumed to be zero (this represents reaching zero loan liability, when a loan has been fully repaid).
    /// </p>
    /// <p class="body">Estimate is an approximation of the interest rate used to start the calculation (which
    /// works by iteratively refining the <em>estimate</em> until it converges on the correct value). When no
    /// <em>estimate</em> is given a default of 10% is assumed.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionRate : BuiltInFunctionBase
	{
		#region Member Variables

		private const int ITMAX = 110;
		private const double CEPS = 1.0e-10;

		#endregion //Member Variables

		#region calcFV
		private bool calcFV( double rate, double term,
			double payment, double pv, int type,
			double fv, ref int sign )
		{
			double fvPrime = UltraCalcFunctionFV.CalculateFV(rate, term, payment, pv, type);
			fvPrime -= fv;
		
			sign = Math.Sign( fvPrime );
			return Math.Abs(fvPrime) < UltraCalcFunctionRate.CEPS;
		}
		#endregion //calcFV

		
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			double fv = 0.0;
			double pv = 0;
			double pmt = 0;
			double term = 0;
			int iType = 0;
			double guess = 0.1;
			
			if( argumentCount < 3 || argumentCount > 6 ) 
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );

			if (argumentCount == 6 && !numberStack.Pop().ToDouble(out guess))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// MD 9/21/11
			// Found while fixing TFS87571
			// These parameters have been swapped from the original Excel function definition.
			//if( argumentCount >= 5 && !numberStack.Pop().ToDouble(out fv))
			//    return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
			//
			//if( argumentCount >= 4 ) 
			//{
			//    iType = numberStack.Pop().ToInt(CultureInfo.InvariantCulture);
			//
			//    if( iType < 0 || iType > 1 ) 
			//        return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			//}
			if (argumentCount >= 5)
			{
				// MD 4/6/12 - TFS101506
				//iType = numberStack.Pop().ToInt(CultureInfo.InvariantCulture);
				iType = numberStack.Pop().ToInt();
			
				if( iType < 0 || iType > 1 ) 
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			}

			if (argumentCount >= 4 && !numberStack.Pop().ToDouble(out fv))
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			if( argumentCount >= 3 ) 
			{
				if (!numberStack.Pop().ToDouble(out pv) ||
					!numberStack.Pop().ToDouble(out pmt) ||
					!numberStack.Pop().ToDouble(out term))
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

				if( term <= 0 ) 
					return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Value) );
			}

			int initialSign = 0;

			if( calcFV(guess, term, pmt, pv, iType, fv, ref initialSign) )
				return new UltraCalcValue(guess);

			double step = 0.0001;
			double upperBound = guess + step;
			double lowerBound = guess - step;

			bool bFoundSignChange = false;

			for( int i = 0;i<UltraCalcFunctionRate.ITMAX && bFoundSignChange == false;++i ) 
			{
				int nextSign = 0;

				if( calcFV(upperBound, term, pmt, pv, iType, fv, ref nextSign) )
					return new UltraCalcValue(upperBound);

				if( nextSign != initialSign ) 
				{
					bFoundSignChange = true;
					lowerBound = upperBound - step;
					continue;
				}

				if( lowerBound > -1.0 ) 
				{
					if( calcFV(lowerBound, term, pmt, pv, iType, fv, ref nextSign) )
						return new UltraCalcValue(lowerBound);

					if( nextSign != initialSign ) 
					{
						bFoundSignChange = true;
						upperBound = lowerBound + step;
						continue;
					}
				}

				if( (i == 25) || (i == 50) || (i == 100) ) 
				{
					step = step * 10;
				}

				upperBound += step;
				lowerBound -= step;
			}

			if( bFoundSignChange == false ) 
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			for( int i=0;i<UltraCalcFunctionRate.ITMAX;++i ) 
			{
				guess = lowerBound + ((upperBound - lowerBound) / 2);

				int guessSign = 0;

				if( calcFV(guess, term, pmt, pv, iType, fv, ref guessSign) )
					return new UltraCalcValue(guess);

				int nextSign = 0;
				
				calcFV(upperBound, term, pmt, pv, iType, fv, ref nextSign);

				if( guessSign == nextSign ) 
					upperBound = guess;
				else 
					lowerBound = guess;
			}

			return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "rate"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 6; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionRate

	#region UltraCalcFunctionIsNull
	/// <summary>
	/// Returns a boolean indicating if the value is null (Nothing in VB).
	/// </summary>

	internal





        class UltraCalcFunctionIsNull : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsReference)
				value = value.ToReference().Value;

			return new UltraCalcValue(value.Value == null);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "isnull"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsNull

	#region UltraCalcFunctionNull
	/// <summary>
	/// Returns a null value (Nothing in VB)
	/// </summary>

	internal





        class UltraCalcFunctionNull : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			return new UltraCalcValue(null);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "null"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionNull

	#region UltraCalcFunctionIsDBNull
	/// <summary>
	/// Returns a boolean indicating if the value is DBNull.
	/// </summary>

	internal





        class UltraCalcFunctionIsDBNull : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			return new UltraCalcValue(numberStack.Pop().IsDBNull);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "isdbnull"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIsDBNull

	#region UltraCalcFunctionDBNull
	/// <summary>
	/// Returns DBNull
	/// </summary>

	internal





        class UltraCalcFunctionDBNull : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			return new UltraCalcValue(DBNull.Value);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "dbnull"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionDBNull

	
	#region UltraCalcFunctionDateAdd
	/// <summary>
	/// Returns a <see cref="DateTime"/> value resulting from adding a specified interval to a DateTime.
	/// </summary>

	internal





        class UltraCalcFunctionDateAdd : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// MD 4/6/12 - TFS101506
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
			DateTime date = numberStack.Pop().ToDateTime();

			double number;
			
			if (!numberStack.Pop().ToDouble(out number))
				return new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			// MD 4/6/12 - TFS101506
			//string interval = numberStack.Pop().ToString(CultureInfo.InvariantCulture).ToLower().Trim();
			string interval = numberStack.Pop().ToString().ToLower().Trim();

			// MRS 5/23/05 - Use the VisualBasic dll function
			DateTime result = CalcManagerUtilities.DateAndTimeDateAdd(interval, number, date);

			return new UltraCalcValue( result );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "dateadd"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionDateAdd

	#region UltraCalcFunctionDateDiff
	/// <summary>
	/// Returns a value specifying the number of time intervals between two DateTime values.
	/// </summary>

	internal





        class UltraCalcFunctionDateDiff : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
            // MBS 4/21/09 - TFS16846
            // DBNull cannot be converted to other types, since the IConvertible implementation will throw
            // an exception.  We should handle all null types here, since we won't be able to convert
            // them to DateTime and this will be more efficient than causing the exception to be raised.
            //
            //DateTime date2 = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
            //DateTime date1 = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture);
            //string interval = numberStack.Pop().ToString(CultureInfo.InvariantCulture).ToLower().Trim();
            //
            UltraCalcValue val2 = numberStack.Pop();
            UltraCalcValue val1 = numberStack.Pop();

			// MD 4/6/12 - TFS101506
            //string interval = numberStack.Pop().ToString(CultureInfo.InvariantCulture).ToLower().Trim();
			string interval = numberStack.Pop().ToString().ToLower().Trim();

            if (val2.IsNull || val1.IsNull)
            {
                UltraCalcErrorValue error = new UltraCalcErrorValue(UltraCalcErrorCode.Num);
                return new UltraCalcValue(error);
            }
            //
			// MD 4/6/12 - TFS101506
			//DateTime date2 = val2.ToDateTime(CultureInfo.InvariantCulture);
			//DateTime date1 = val1.ToDateTime(CultureInfo.InvariantCulture);
			DateTime date2 = val2.ToDateTime();
			DateTime date1 = val1.ToDateTime();

            // MRS NAS 8.3 - refactored this into a helper method. 
            //// MRS 5/23/05 - Use the VisualBasic dll function			
            //FirstDayOfWeek firstDayOfWeek = (FirstDayOfWeek)(((int)CultureInfo.InvariantCulture.DateTimeFormat.FirstDayOfWeek) + 1);							
            //FirstWeekOfYear firstWeekOfYear = GetFirstWeekOfYear( CultureInfo.InvariantCulture.DateTimeFormat.CalendarWeekRule );
            
            //long result = DateAndTime.DateDiff(interval, date1, date2, firstDayOfWeek , firstWeekOfYear );
			// MD 4/6/12 - TFS101506
			//long result = DateDiffInvariant(interval, date1, date2);
			long result = DateDiffInvariant(numberStack.Culture, interval, date1, date2);

			return new UltraCalcValue( result );
		}

		private static double Fix(double number)
		{
			if (number >= 0)
				return Math.Floor(number);
			
			return -Math.Floor(-number);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "datediff"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 3; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args

		#region GetFirstWeekOfYear
		internal static FirstWeekOfYear GetFirstWeekOfYear( CalendarWeekRule calendarWeekRule )
		{
			switch (calendarWeekRule)
			{
				case CalendarWeekRule.FirstFourDayWeek:
					return FirstWeekOfYear.FirstFourDays;
				case CalendarWeekRule.FirstFullWeek:
					return FirstWeekOfYear.FirstFullWeek;
				case CalendarWeekRule.FirstDay:
				default:
					return FirstWeekOfYear.Jan1;
			}
		}
		#endregion GetFirstWeekOfYear

        // MRS NAS 8.3 *** Start ***

        #region DateDiffInvariant
		// MD 4/6/12 - TFS101506
        //internal static long DateDiffInvariant(string interval, DateTime date1, DateTime date2)
		internal static long DateDiffInvariant(CultureInfo culture, string interval, DateTime date1, DateTime date2)
        {
            // MRS 5/23/05 - Use the VisualBasic dll function			
			// MD 4/6/12 - TFS101506
			//FirstDayOfWeek firstDayOfWeek = (FirstDayOfWeek)(((int)CultureInfo.InvariantCulture.DateTimeFormat.FirstDayOfWeek) + 1);
			//FirstWeekOfYear firstWeekOfYear = GetFirstWeekOfYear(CultureInfo.InvariantCulture.DateTimeFormat.CalendarWeekRule);
			FirstDayOfWeek firstDayOfWeek = (FirstDayOfWeek)(((int)culture.DateTimeFormat.FirstDayOfWeek) + 1);
			FirstWeekOfYear firstWeekOfYear = GetFirstWeekOfYear(culture.DateTimeFormat.CalendarWeekRule);

            long result = CalcManagerUtilities.DateAndTimeDateDiff(interval, date1, date2, firstDayOfWeek, firstWeekOfYear);
            return result;
        }

		// MD 4/6/12 - TFS101506
        //internal static long DateDiffInvariant(DateInterval interval, DateTime date1, DateTime date2)
		internal static long DateDiffInvariant(CultureInfo culture, DateInterval interval, DateTime date1, DateTime date2)
        {
            // MRS 5/23/05 - Use the VisualBasic dll function			
			// MD 4/6/12 - TFS101506
			//FirstDayOfWeek firstDayOfWeek = (FirstDayOfWeek)(((int)CultureInfo.InvariantCulture.DateTimeFormat.FirstDayOfWeek) + 1);
			//FirstWeekOfYear firstWeekOfYear = GetFirstWeekOfYear(CultureInfo.InvariantCulture.DateTimeFormat.CalendarWeekRule);
			FirstDayOfWeek firstDayOfWeek = (FirstDayOfWeek)(((int)culture.DateTimeFormat.FirstDayOfWeek) + 1);
			FirstWeekOfYear firstWeekOfYear = GetFirstWeekOfYear(culture.DateTimeFormat.CalendarWeekRule);

            long result = CalcManagerUtilities.DateAndTimeDateDiff(interval, date1, date2, firstDayOfWeek, firstWeekOfYear);
            return result;
        }
        #endregion //DateDiffInvariant

        // MRS NAS 8.3 *** End ***
	}
	#endregion //UltraCalcFunctionDateDiff

	// JAS 12/22/04 BR01396 *** Start ***

	#region UltraCalcFunctionChar
	/// <summary>
	/// Returns the character specified by the code number from the character set for your computer.
	/// </summary>

	internal





        class UltraCalcFunctionChar : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			int number = value.ToInt32();

			return new UltraCalcValue( Convert.ToChar( number ) );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "char"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionChar

	#region UltraCalcFunctionCode
    /// <summary>
    /// Gives you the numeric code corresponding the the first character in a
    /// specific text string.
    /// </summary>
    /// <remarks>
    /// <p class="body">CODE(Text)</p>
    /// <p class="body">Text is a text string of at least one character in length.
    /// This function returns the numeric code of the first character in <em>Text</em>.
    /// These code values correspond to those used by your machine to encode the text
    /// characters in a string (<em>e.g.</em>, Unicode).</p>
    /// <p class="body">If <em>Text</em> is an empty string of zero length, then
    /// this function returns the code zero.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionCode : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue value = numberStack.Pop();

			if (value.IsError)
				return new UltraCalcValue( value.ToErrorValue() );

			string text = value.ToString();

			if( text == null || text.Length == 0 )
				return new UltraCalcValue( 0 );

			return new UltraCalcValue( (int)text[0] );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "code"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionCode

	#region UltraCalcFunctionFixed
    /// <summary>
    /// Formats a numeric value rounded to a specified number of decimal places and
    /// with optional thousands separators.
    /// </summary>
    /// <remarks>
    /// <p class="body">FIXED(value, decimal_places, exclude_thousands_separators)</p>
    /// <p class="body">Value is the numeric value to be rounded and formatted as text.</p>
    /// <p class="body">Decimal_places indicate how many places to the right of the decimal point
    /// the <em>value</em> should be rounded. If omitted, rounding occurs to two decimal places.</p>
    /// <p class="body">Exclude_thousands_separators is a boolean value (TRUE or FALSE) indicating
    /// whether commas should appear in the formatted text value. By default the formatted text
    /// includes commas as thousands separators.</p>
    /// <p class="body">This function may not return text values consistent with your locale's
    /// formatting of numbers because it operates using culture invariant settings (these include
    /// commas as thousands separators and decimal points) to facilitate unambiguously parsing the
    /// text values it formats into numeric values for subsequent calculations. Applications may
    /// instead choose to format numeric values after UltraCalc has finished processing them, but
    /// before presenting them to their end user.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionFixed : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			// JAS 3/18/05 BR02707 - 'Round' method changed param from decimal to double, so this must change too.
//			decimal number   = 0;
			double number   = 0;
			int     decimals = 2;
			bool    noCommas = false;

			System.Text.StringBuilder format = new System.Text.StringBuilder( 100 );

			#region Get Arguments

			UltraCalcValue value;

			if( 2 < argumentCount )
			{
				value = numberStack.Pop();
				if( value.IsError )
					return new UltraCalcValue( value.ToErrorValue() );

				noCommas = value.ToBoolean();
			}

			if( 1 < argumentCount )
			{
				value = numberStack.Pop();
				if( value.IsError )
					return new UltraCalcValue( value.ToErrorValue() );

				decimals = value.ToInt();
			}

			value = numberStack.Pop();
			if( value.IsError )
				return new UltraCalcValue( value.ToErrorValue() );

			// JAS 3/18/05 BR02707
//			number = value.ToDecimal();
			number = value.ToDouble();

			#endregion // Get Arguments

			#region Create Format String

			// Add whole number specifiers.
			int numWholeNumbers = Decimal.MaxValue.ToString().Length;
			int index;
			for( index = 0; index < numWholeNumbers; ++index )
			{
				// MD 3/2/12 - TFS103750
				// Make sure the first integer format character is a 0 so numbers with an absollute value less than 1
				// will have a leading zero.
				if (index == numWholeNumbers - 1)
				{
					format.Append("0");
					continue;
				}

				format.Append( "#" );
			}

			// Add commas, if appropriate.
			if( ! noCommas )
			{
				// First position the index at the breaking point between the hundreds and thousands sections.
				// Break if the index is not greater than 0 (NOT -1) because a comma should never be the first character.
				for( index = format.Length - 4; index > 0; index -= 3 )
				{
					// MD 4/6/12 - TFS101506
					//format.Insert( index, CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator );
					format.Insert(index, numberStack.Culture.NumberFormat.NumberGroupSeparator);
				}
			}

			if( 0 < decimals )
			{
				// Add decimal point and padding.
				// MD 4/6/12 - TFS101506
				//format.Append( CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator );
				format.Append(numberStack.Culture.NumberFormat.NumberDecimalSeparator);

				for( index = 0; index < decimals; ++index )
				{
					format.Append( "0" );
				}
			}
			else
			{
				// If 'decimals' is less than 1, then the behavior of FIXED is the same
				// as that of ROUND, so let ROUND calculate our result.
				number = UltraCalcFunctionRound.Round( number, decimals );
			}

			#endregion // Create Format String

			string retVal = number.ToString( format.ToString() );
			if( retVal == null || retVal.Length == 0 )
				retVal = "0";

			return new UltraCalcValue( retVal ) ;
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "fixed"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 3; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionFixed

	#region UltraCalcFunctionToday
    /// <summary>
    /// Gets the host's current date.
    /// </summary>
    /// <remarks>
    /// <p class="body">No arguments are passed to this function.</p>
    /// <p class="body">The date returned for TODAY() is based on the local time of the
    /// host running UltraCalc. In some distributed applications, such as those built for
    /// the Web, this may be different from today's date at the client.</p>
    /// <p class="body">The date/time value returned is always adjusted to Midnight 
    /// (00:00).</p>
    /// </remarks>

	internal





        class UltraCalcFunctionToday : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{			
			return new UltraCalcValue( DateTime.Today ) ;
		}


		#region IsAlwaysDirty
		/// <summary>
		/// Indicates whether the results of the function is always dirty.
		/// </summary>
		public override bool IsAlwaysDirty
		{
			// Need to return true in case the CalculationManager is being used at 11:59pm.
			get { return true; }
		}
		#endregion //IsAlwaysDirty 


		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{ 
			get { return "today"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{ 
			get { return 0; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 0; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionToday

	#region UltraCalcFunctionASinh
    /// <summary>
    /// Returns the angle (measured in radians) having the specified value of it's hyperbolic sine function.
    /// </summary>
    /// <remarks>
    /// <p class="body">ASINH(value)</p>
    /// <p class="body">Value is the hyperbolic sine of some angle (measured in radians) that you want
    /// to find. As you might expect, the values of the hyperbolic sine function (see the <see cref="UltraCalcFunctionSinh">SINH</see> function)
    /// increase at a hyperbolic rate, but one consequence is that these ever larger changes in the hyperbolic
    /// sine will correspond to ever smaller changes in the angle. This relationship is intrinsic to many of
    /// the mathematical and engineering applications of the inverse hyperbolic sine function, such as when
    /// resistance or strain builds up on a body increasingly as it is rotated (hysteresis).</p>
    /// </remarks>

	internal





        class UltraCalcFunctionASinh : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function, Arcsinh, against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments.</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate( UltraCalcNumberStack numberStack, int argumentCount )
		{
			UltraCalcValue arg = numberStack.Pop();

			if( arg.IsError )
			{
				return new UltraCalcValue( arg.ToErrorValue() );
			}

			double x;

			if( !arg.ToDouble( out x ) )
			{
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Num ) );
			}
			// Domain of Arcsinh is (-INF,+INF) so expression is always valid.
			double arcsinh = Math.Log( x + Math.Sqrt( ( x * x ) + 1.0 ) );

			return new UltraCalcValue( arcsinh );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula.
		/// </summary>
		public override string Name
		{ 
			get { return "asinh"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function.
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function.
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionASinh

	#region UltraCalcFunctionATanh
    /// <summary>
    /// Calculates the hyperbolic tangent of a specified angle measured in radians.
    /// </summary>
    /// <remarks>
    /// <p class="body">TANH(value)</p>
    /// <p class="body">Value is an angle measured in radians for which you want to
    /// calculate the hyperbolic tangent. If your angle is measured in degrees, 
    /// multiply it by PI()/180 to convert into radians. The hyperbolic tangent 
    /// has a range from -1 to 1.</p>
    /// </remarks>

	internal





        class UltraCalcFunctionATanh : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function, Arctanh, against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments.</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate( UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue arg = numberStack.Pop();

			if ( arg.IsError)
			{
				return new UltraCalcValue( arg.ToErrorValue());
			}

			double x;
			if ( !arg.ToDouble( out x))
			{
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Num) );
			}

			double arctanh;
			if ( 1.0 == x )
			{
				arctanh = Double.PositiveInfinity;
			}
			else if ( -1.0 == x )
			{
				arctanh = Double.NegativeInfinity;
			}
			else
			{
				// The domain of Arctanh is from [-1,1] inclusive, so in the
				// following expression:
				//
				// * Arguments less than -1 lead to Ln 0 or negative which
				// produces an appropriate error.
				//
				// * Arguments greater than 1 lead to divide by zero or a
				// Ln negative which produces an appropriate error.
				//
				arctanh = Math.Log( ( 1.0 + x ) / ( 1.0 - x ) ) / 2.0;
			}
			return new UltraCalcValue( arctanh );
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula.
		/// </summary>
		public override string Name
		{ 
			get { return "atanh"; }
		}
		#endregion // Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function.
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function.
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion // UltraCalcFunctionATanh

	#region UltraCalcFunctionACosh
    /// <summary>
    /// Returns the angle (measured in radians) having the specified value of it's hyperbolic cosine function.
    /// </summary>
    /// <remarks>
    /// <p class="body">ACOSH(value)</p>
    /// <p class="body">Value is the hyperbolic cosine of some angle (measured in radians) that you want
    /// to find. As you might expect, the values of the hyperbolic cosine function (see the <see cref="UltraCalcFunctionCosh">COSH</see> function)
    /// increase at a hyperbolic rate, but one consequence is that these ever larger changes in the hyperbolic
    /// cosine will correspond to ever smaller changes in the angle. This relationship is intrinsic to many of
    /// the mathematical and engineering applications of the inverse hyperbolic cosine function, such as when
    /// resistance or strain builds up on a body increasingly as it is rotated (hysteresis).</p>
    /// </remarks>

	internal





        class UltraCalcFunctionACosh : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function, Arccosh, against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments.</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate( UltraCalcNumberStack numberStack, int argumentCount )
		{
			UltraCalcValue arg = numberStack.Pop();

			if( arg.IsError )
			{
				return new UltraCalcValue( arg.ToErrorValue() );
			}

			double x;

			if( ! arg.ToDouble( out x ) )
			{
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Num ) );
			}
			// Domain of Arccosh is greater than 1 (exclusive), so in the following
			// expression:
			//
			// * SqRt will be negative and throw an appropriate error if the argument
			// is less than 1.
			// * Ln will throw an appropriate error if the argument is exactly 1 leading
			// to the invalid expression Ln 0.
			//
			double arccosh = Math.Log( x + Math.Sqrt( ( x * x ) - 1.0 ) );

			return new UltraCalcValue( arccosh );
		}


		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula.
		/// </summary>
		public override string Name
		{ 
			get { return "acosh"; }
		}
		#endregion // Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function.
		/// </summary>
		public override int MinArgs
		{ 
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function.
		/// </summary>
		public override int MaxArgs
		{ 
			get { return 1; }
		}
		#endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionACosh

	// JAS 12/22/04 BR01396 *** End ***   


    #region MRS NAS 8.3 - New Functions

    #region UltraCalcFunctionEDate
    /// <summary>
    /// Returns a date that is the specified number of months before or after the start date.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// DATE(date, numberOfMonths)
    /// <br/>
    /// Date is the starting date
    /// <br/>
    /// NumberOfMonths is a number of months to shift the date. A positive number means a date after the startdate. A negative number indicates a date prior to the start date.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionEDate : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/6/12 - TFS101506
			//double numberOfMonths = numberStack.Pop().ToDouble(CultureInfo.InvariantCulture);
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture).Date;
			double numberOfMonths = numberStack.Pop().ToDouble();
			DateTime date = numberStack.Pop().ToDateTime().Date;

            date = CalcManagerUtilities.DateAndTimeDateAdd(DateInterval.Month, numberOfMonths, date);
            return new UltraCalcValue(date);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "edate"; }
        }
        #endregion //Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion //Min/Max args
    }
    #endregion //UltraCalcFunctionEDate

    #region UltraCalcFunctionEOMonth
    /// <summary>
    /// Returns a date that is at the end of the month which is the specified number of months before or after the start date.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// EOMONTH(date, numberOfMonths)
    /// <br/>
    /// Date is the starting date
    /// <br/>
    /// NumberOfMonths is a number of months to shift the date. A positive number means a date after the startdate. A negative number indicates a date prior to the start date.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionEOMonth : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/6/12 - TFS101506
			//double numberOfMonths = numberStack.Pop().ToDouble(CultureInfo.InvariantCulture);
			//DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture).Date;
			double numberOfMonths = numberStack.Pop().ToDouble();
			DateTime date = numberStack.Pop().ToDateTime().Date;

            date = CalcManagerUtilities.DateAndTimeDateAdd(DateInterval.Month, numberOfMonths, date);
            date = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

            return new UltraCalcValue(date);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "eomonth"; }
        }
        #endregion //Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion //Min/Max args
    }
    #endregion //UltraCalcFunctionEOMonth

    #region UltraCalcFunctionWeekDay
    /// <summary>
    /// Returns a number representing the day of the week of the specified date. 
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// WEEKDAY(date, returnType)
    /// <br/>
    /// Date is a date.
    /// <br/>
    /// ReturnType determines which numbering scheme is used for the days of the week.
    /// 1 (default) = Sunday (1) through Saturday (7)
    /// 2 = Monday (1) through Sunday (7)
    /// 3 = Monday (0) through Sunday (6)
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionWeekDay : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            int returnType = (argumentCount == 2)
				// MD 4/6/12 - TFS101506
                //? numberStack.Pop().ToInt32(CultureInfo.InvariantCulture)
				? numberStack.Pop().ToInt32()
                : 1;

			// MD 4/6/12 - TFS101506
            //DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture).Date;
			DateTime date = numberStack.Pop().ToDateTime().Date;

			// MD 4/10/12
			// Found while fixing TFS108678
			// Excel does not correct the date issue for the WEEKDAY function.
			if (UltraCalcValue.UseExcelFunctionCompatibility)
			{
				if (date <= new DateTime(1900, 3, 1))
					date = date.AddDays(-1);
			}

            DayOfWeek dayOfWeek = date.DayOfWeek;

            int modifier;
            switch (returnType)
            {
                case 1:
                    modifier = 1;
                    break;
                case 2:
                    modifier = (dayOfWeek != DayOfWeek.Sunday) ? 0 : 7;
                    break;
                case 3:
                    modifier = (dayOfWeek != DayOfWeek.Sunday) ? -1 : 6;
                    break;
                default:
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
            }

            int dayOfWeekInt = (int)dayOfWeek;
            dayOfWeekInt += modifier;

            return new UltraCalcValue(dayOfWeekInt);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "weekday"; }
        }
        #endregion //Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionWeekDay

    #region UltraCalcFunctionNetWorkDays
    /// <summary>
    /// Returns the total number of whole work days between the specified dates, excluding any specified holidays.
    /// </summary>
    /// <remarks>    
    /// <p class="body">
    /// NETWORKDAYS(startDate, endDate [, holiday1, holiday2, ..., holidayN])
    /// <br/>
    /// StartDate is the date from which to start.
    /// <br/>
    /// EndDate is the date at which to end.
    /// <br/>
    /// Holiday1, holiday2, ..., holidayN is any number of dates or range references containing dates that are to be considered non-working days.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionNetWorkDays : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue[] holidayRefs = GetArguments(numberStack, argumentCount - 2, true);

            Dictionary<DateTime, object> holidays;
            UltraCalcValue errorValue;

            bool success = UltraCalcFunctionWorkDay.GetHolidayList(holidayRefs, out holidays, out errorValue);
            if (success == false)
                return errorValue;

            DateTime endDate = numberStack.Pop().ToDateTime().Date;
            DateTime startDate = numberStack.Pop().ToDateTime().Date;

            // If the start date is greater than the end date, flip the sign and reverse the dates. 
            int sign = 1;
            if (startDate > endDate)
            {
                sign = -1;
                DateTime tempDate = startDate;
                startDate = endDate;
                endDate = tempDate;
            }

            // Determine the total number of days in the date range. 
			// MD 4/6/12 - TFS101506
            //long workDayCount = UltraCalcFunctionDateDiff.DateDiffInvariant(DateInterval.Day, startDate, endDate);
			long workDayCount = UltraCalcFunctionDateDiff.DateDiffInvariant(numberStack.Culture, DateInterval.Day, startDate, endDate);

            workDayCount += 1;

            // If the count is 0, no need to bother stripping out holidays or weekends. 
            if (workDayCount > 0)
            {
                // calculate the number of weeks and the number of extra days. 
                int weeks = (int)(workDayCount / 7);
                int days = (int)(workDayCount % 7);

                // Subtract 2 days for every week to account for weekends. 
                workDayCount -= (weeks * 2);

                // Account for the any weekends that occur in the extra days. 
                switch (startDate.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                    case DayOfWeek.Saturday:
                        int daysToCoverWeekend = 8 - (int)startDate.DayOfWeek;
                        if (days >= daysToCoverWeekend)
                            workDayCount -= Math.Min(2, daysToCoverWeekend);
                        else if (days == (daysToCoverWeekend - 1))
                            workDayCount -= Math.Min(1, daysToCoverWeekend);

                        break;
                    case DayOfWeek.Sunday:
                        if (days >= 1)
                            workDayCount -= 1;

                        break;
                    default:
                        CalcManagerUtilities.DebugFail("Unknown DayOfWeek");
                        break;
                }

                // Strip out any specified holidays. 
                // Note that when we build the holiday list, we strip out weekends and duplicates.
                // So any holiday on the list is assured to be a unique weekday. 
                if (holidays != null)
                {
                    foreach (DateTime holiday in holidays.Keys)
                    {
                        if (holiday >= startDate &&
                            holiday <= endDate)
                        {
                            workDayCount -= 1;
                        }
                    }
                }

                // If the start date is greater than the end date, flip the sign.                 
                workDayCount *= sign;
            }

            return new UltraCalcValue(workDayCount);
        }

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			// Only the holiday parameter can be enumerable
			return parameterIndex == 2;
		}

		#endregion  // CanParameterBeEnumerable

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "networkdays"; }
        }
        #endregion //Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 3; }
        }
        #endregion //Min/Max args


		#region ArgCount
		internal override int ArgCount
		{
			get
			{
				return 3;
			}
		}
		#endregion ArgCount 

    }
    #endregion //UltraCalcFunctionNetWorkDays

    #region UltraCalcFunctionWeekNum
    /// <summary>
    /// Returns the number of the week into which the specified date falls. 
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// WEEKNUM(date, returnType)
    /// <br/>
    /// Date is a date.
    /// <br/>
    /// ReturnType determines which day begins the week.
    /// 1 (default) = Sunday is the first day of the week.
    /// 2 = Monday is the first day of the week.    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionWeekNum : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            int returnType = (argumentCount == 2)
				// MD 4/6/12 - TFS101506
                //? numberStack.Pop().ToInt32(CultureInfo.InvariantCulture)
				? numberStack.Pop().ToInt32()
                : 1;

            DayOfWeek startingDayOfWeek;
            switch (returnType)
            {
                case 1:
                    startingDayOfWeek = DayOfWeek.Sunday;
                    break;
                case 2:
                    startingDayOfWeek = DayOfWeek.Monday;
                    break;
                default:
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
            }

			// MD 4/6/12 - TFS101506
            //DateTime date = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture).Date;
			DateTime date = numberStack.Pop().ToDateTime().Date;

            // Determine the first day of the year. 
            DateTime firstDayOfYear = new DateTime(date.Year, 1, 1);

            // Calculate the first day of the first week of the year. 
            DateTime firstWeekStart = firstDayOfYear;
            while (firstWeekStart.DayOfWeek != startingDayOfWeek)
            {
                firstWeekStart = firstWeekStart.AddDays(1);
            }


            // Calculate the weeknumber
            // This does not include any partial week that might exist before the firstWeekStart
            int weekNum = (((date - firstWeekStart).Days) / 7) + 1;

            // If the first day of the year is not the same as the beginning of the 
            // first week start, and the date we are examining is after the beginning of the first
            // week, then we need to add one to account for the partial week at the beginning
            // of the year. 
            if (firstDayOfYear != firstWeekStart &&
                date >= firstWeekStart)
            {
                weekNum += 1;
            }

            return new UltraCalcValue(weekNum);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "weeknum"; }
        }
        #endregion //Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionWeekNum

    #region UltraCalcFunctionWorkDay
    /// <summary>
    /// Returns a work day the specified number of days from the specified date, excluding any specified holidays.
    /// </summary>
    /// <remarks>    
    /// <p class="body">
    /// WORKDAY(startDate, days [, holiday1, holiday2, ..., holidayN])
    /// <br/>
    /// StartDate is the date from which to start.
    /// <br/>
    /// Days is the number of work days to offset from the start date.
    /// <br/>
    /// Holiday1, holiday2, ..., holidayN is any number of dates or range references containing dates that are to be considered non-working days.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionWorkDay : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue[] holidayRefs = GetArguments(numberStack, argumentCount - 2, true);

            Dictionary<DateTime, object> holidays;
            UltraCalcValue errorValue;

            bool success = GetHolidayList(holidayRefs, out holidays, out errorValue);
            if (success == false)
                return errorValue;

            int days = numberStack.Pop().ToInt32();
            DateTime startDate = numberStack.Pop().ToDateTime().Date;


            DateTime day = startDate;

            int targetWorkDays = Math.Abs(days);
            int workdaysFound = 0;
            if (days != 0)
            {
                int increment = days < 0 ? -1 : 1;

                do
                {
                    day = day.AddDays(increment);
                    workdaysFound += IsWorkDay(day, holidays) ? 1 : 0;
                }
                while (workdaysFound < targetWorkDays);
            }

            return new UltraCalcValue(day);
        }

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			// Only the holiday argument can be enumerable
			return parameterIndex == 2;
		}

		#endregion  // CanParameterBeEnumerable

        private bool IsWorkDay(DateTime workDay, Dictionary<DateTime, object> holidays)
        {
            if (IsWeekend(workDay))
                return false;

            // MRS 11/17/2008 - TFS10402
            //
            //if (holidays.ContainsKey(workDay))
            if (holidays != null &&
                holidays.ContainsKey(workDay))
                return false;

            return true;
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "workday"; }
        }
        #endregion //Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 3; }
        }
        #endregion //Min/Max args


		#region ArgCount
		internal override int ArgCount
		{
			get
			{
				return 3;
			}
		}
		#endregion ArgCount 


        #region GetHolidayList
        internal static bool GetHolidayList(UltraCalcValue[] holidayRefs, out Dictionary<DateTime, object> holidays, out UltraCalcValue errorValue)
        {
            if (holidayRefs.Length > 0)
            {
                holidays = new Dictionary<DateTime, object>(holidayRefs.Length);

                foreach (UltraCalcValue holidayRef in holidayRefs)
                {
                    if (holidayRef.IsError)
                    {
                        errorValue = new UltraCalcValue(holidayRef.ToErrorValue());
                        return false;
                    }

					// MD 4/6/12 - TFS101506
                    //DateTime holiday = holidayRef.ToDateTime(CultureInfo.InvariantCulture).Date;
					DateTime holiday = holidayRef.ToDateTime().Date;

                    // Don't bother adding it to the list if it's already a weekend. 
                    if (IsWeekend(holiday) == false)
                    {
                        if (holidays.ContainsKey(holiday) == false)
                            holidays.Add(holiday, null);
                    }
                }
            }
            else
                holidays = null;

            errorValue = null;
            return true;
        }
        #endregion //GetHolidayList

        #region IsWeekend
        internal static bool IsWeekend(DateTime date)
        {
            return IsWeekend(date.DayOfWeek);
        }

        internal static bool IsWeekend(DayOfWeek dayOfWeek)
        {
            return dayOfWeek == DayOfWeek.Saturday ||
                dayOfWeek == DayOfWeek.Sunday;
        }
        #endregion //IsWeekend



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionWorkDay

    #region UltraCalcFunctionYearFrac
    // MRS 6/23/2008 - I just can't get this to work for the 0 or 4 case, so I'm leaving it out for now. 
//    /// <summary>
//    /// Returns a fraction representing the part of a year specified between the specified start date and end date.
//    /// </summary>
//    /// <remarks>
//    /// <p class="body">
//    /// YearFrac(startDate, endDate, basis)
//    /// <br/>
//    /// Start date is the starting date in the range. 
//    /// <br/>
//    /// End date is the ending date in the range. 
//    /// <br/>
//    /// Basis determines the basis for the day count to use.
//    /// 0 (Default = US (NASD) 30/360 
//    /// 1 = Actual/actual 
//    /// 2 = Actual/360 
//    /// 3 = Actual/365 
//    /// 4 = European 30/360
//    /// </p>
//    /// </remarks>
//#if EXCEL
//    internal
//#else
//    public
//#endif
//        class UltraCalcFunctionYearFrac : BuiltInFunctionBase
//    {
//        /// <summary>
//        /// Evaluates the function against the arguments on the number stack
//        /// </summary>
//        /// <param name="numberStack">Formula number stack containing function arguments</param>
//        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
//        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
//        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
//        {
//            int basis = (argumentCount == 3) ? numberStack.Pop().ToInt32() : 0;

//            double daysPerYear;
//            switch (basis)
//            {
//                case 0:
//                case 2:
//                case 4:
//                    daysPerYear = 360.0;
//                    break;

//                case 3:
//                    daysPerYear = 365.0;
//                    break;

//                case 1:
//                    daysPerYear = 0.0;
//                    break;

//                default:
//                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
//            }

//            DateTime endDate = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture).Date;
//            DateTime startDate = numberStack.Pop().ToDateTime(CultureInfo.InvariantCulture).Date;

//            long days = UltraCalcFunctionDateDiff.DateDiffInvariant(DateInterval.Day, startDate, endDate);
//            days = Math.Abs(days);

//            if (daysPerYear == 0.0)
//            {
//                if (startDate.Year == endDate.Year)
//                {
//                    daysPerYear = IsLeapYear(endDate.Year)
//                        ? 366
//                        : 365;
//                }
//                else if (days <= 366)
//                {
//                    bool endDateIsLeapYear = IsLeapYear(endDate);
//                    bool startDateIsLeapYear = IsLeapYear(startDate);

//                    DateTime testDate;
//                    if (endDateIsLeapYear || startDateIsLeapYear)
//                    {
//                        if (endDateIsLeapYear)
//                            testDate = new DateTime(endDate.Year, 2, 29);
//                        else
//                            testDate = new DateTime(startDate.Year, 2, 29);

//                        bool crossesLeapDay = (startDate <= testDate) && (testDate <= endDate);
//                        daysPerYear = crossesLeapDay ? 366 : 365;
//                    }
//                }
//                else
//                {
//                    int startYear = startDate.Year;
//                    int endYear = endDate.Year;

//                    long totalDays = 0;
//                    for (int year = startYear; year <= endYear; year++)
//                    {
//                        totalDays += IsLeapYear(year) ? 366 : 365;
//                    }
//                    daysPerYear = totalDays / ((endYear - startYear) + 1);
//                }
//            }

//            double yearFrac = days / daysPerYear;
//            return new UltraCalcValue(yearFrac);
//        }

//        #region Name
//        /// <summary>
//        /// Function name used to reference the function in a formula
//        /// </summary>
//        public override string Name
//        {
//            get { return "yearfrac"; }
//        }
//        #endregion //Name

//        #region Min/Max args
//        /// <summary>
//        /// Minimum number of arguments required for the function
//        /// </summary>
//        public override int MinArgs
//        {
//            get { return 2; }
//        }

//        /// <summary>
//        /// Maximum number of arguments required for the function
//        /// </summary>
//        public override int MaxArgs
//        {
//            get { return 3; }
//        }
//        #endregion //Min/Max args

//        #region IsLeapYear
//        internal static bool IsLeapYear(DateTime date)
//        {
//            return IsLeapYear(date.Year);
//        }

//        internal static bool IsLeapYear(int year)
//        {
//            return CultureInfo.InvariantCulture.DateTimeFormat.Calendar.IsLeapYear(year);
//        }
//        #endregion //IsLeapYear
//    }
    #endregion //UltraCalcFunctionYearFrac


    #region UltraCalcFunctionDec2XBase
    /// <summary>
    /// Returns a string representing the specified decimal value in the specified base number scheme.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Dec2X(number, places)
    /// <br/>
    /// Number is the decimal value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





		abstract class UltraCalcFunctionDec2XBase : BuiltInFunctionBase
    {
        #region Constants
        internal const long MinNumberBin = -512;
        internal const long MaxNumberBin = 511;

        internal const long MinNumberOct = -536870912;
        internal const long MaxNumberOct = 536870911;

        internal const long MinNumberHex = -549755813888;
        internal const long MaxNumberHex = 549755813887;
        #endregion //Constants

        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            int minPlaces = 0;

            if (argumentCount == 2)
            {
                int places = numberStack.Pop().ToInt32();
                if (places <= 0)
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
                else
                    minPlaces = places;
            }

            UltraCalcValue numberArg = numberStack.Pop();
            if (numberArg.IsError)
                return new UltraCalcValue(numberArg.ToErrorValue());

            long number = numberArg.ToInt64();

            if (number < this.MinNumber ||
                number > this.MaxNumber)
            {
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
            }

            string convertedString = ConvertToNumberSystem(number, this.NumberSystem, this.MinNumber, this.MaxNumber);

            if (minPlaces > 0)
            {
                // For some reason, Excel does not return an error condition when the result is 
                // more than 10 digits. 
                if (convertedString.Length < 10 && 
                    convertedString.Length > minPlaces)
                {
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
                }
            }
            else
                minPlaces = 1;

            while (convertedString.Length < minPlaces)
            {
                convertedString = string.Format("{0}{1}", "0", convertedString);
            }

            return new UltraCalcValue(convertedString);
        }

        #region Abstract methods
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        internal abstract protected long MinNumber { get; }

        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        internal abstract protected long MaxNumber { get; }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        internal abstract protected int NumberSystem { get; }
        #endregion //Abstract methods

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion //Min/Max args

        #region ConvertToNumberSystem


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

        internal static string ConvertToNumberSystem(long number, int numberSystem, long minNumber, long maxNumber)
        {
            bool isNegative = false;

            if (number < 0)
            {
                number -= minNumber;
                isNegative = true;
            }

            number &= maxNumber;
            if (isNegative)
                number |= (-minNumber);

            string convertedString = string.Empty;

            if (number == 0)
                convertedString = "0";

            while (number > 0)
            {
                long part = number % numberSystem;
                string partString;

                if (part < 10)
                    partString = part.ToString();
                else
                {
                    part = part - 10;
                    partString = ((char)(part + 65)).ToString();
                }
                convertedString = string.Format("{0}{1}", partString, convertedString);
                number = number / numberSystem;
            }

            return convertedString;
        }
        #endregion //ConvertToNumberSystem
    }
    #endregion //UltraCalcFunctionDec2XBase

    #region UltraCalcFunctionDec2Bin
    /// <summary>
    /// Returns a string representing the specified decimal value as a binary number.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Dec2Bin(number, places)
    /// <br/>
    /// Number is the decimal value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionDec2Bin : UltraCalcFunctionDec2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "dec2bin"; }
        }
        #endregion //Name

        #region MinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberBin; }
        }
        #endregion //MinNumber

        #region MaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberBin; }
        }
        #endregion //MaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NumberSystem
        {
            get { return 2; }
        }
        #endregion //NumberSystem
    }
    #endregion //UltraCalcFunctionDec2Bin

    #region UltraCalcFunctionDec2Hex
    /// <summary>
    /// Returns a string representing the specified decimal value as a hexadecimal number.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Dec2Hex(number, places)
    /// <br/>
    /// Number is the decimal value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionDec2Hex : UltraCalcFunctionDec2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "dec2hex"; }
        }
        #endregion //Name

        #region MinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberHex; }
        }
        #endregion //MinNumber

        #region MaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberHex; }
        }
        #endregion //MaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert to (2 = hexadecimal, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NumberSystem
        {
            get { return 16; }
        }
        #endregion //NumberSystem
    }
    #endregion //UltraCalcFunctionDec2Hex

    #region UltraCalcFunctionDec2Oct
    /// <summary>
    /// Returns a string representing the specified decimal value as a octal number.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Dec2Oct(number, places)
    /// <br/>
    /// Number is the decimal value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionDec2Oct : UltraCalcFunctionDec2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "dec2oct"; }
        }
        #endregion //Name

        #region MinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberOct; }
        }
        #endregion //MinNumber

        #region MaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberOct; }
        }
        #endregion //MaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert to (2 = octal, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NumberSystem
        {
            get { return 8; }
        }
        #endregion //NumberSystem
    }
    #endregion //UltraCalcFunctionDec2Oct


    #region UltraCalcFunctionXBase2Dec
    /// <summary>
    /// Returns the decimal value of a string representation of a number in a non-decimal number system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// X2Dec(number)
    /// <br/>
    /// Number is a string represeting a number in a non-decimal number system.    
    /// </p>
    /// </remarks>

	internal





		abstract class UltraCalcFunctionXBase2Dec : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue numberArg = numberStack.Pop();
            if (numberArg.IsError)
                return new UltraCalcValue(numberArg.ToErrorValue());

            string number = numberArg.ToString();

            if (number.Length > this.MaxLength)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			// SSP 8/22/11 TFS84369
			// Added error out parameter.
			// 
			// ------------------------------------------------------------------------------------------------------------
            //long value = ConvertFromNumberSystem(number, this.NumberSystem, this.MinNumber, this.MaxNumber);
			UltraCalcErrorValue error;

			// MD 4/9/12 - TFS101506
			//long value = ConvertFromNumberSystem( number, this.NumberSystem, this.MinNumber, this.MaxNumber, out error );
			long value = ConvertFromNumberSystem(numberStack.Culture, number, this.NumberSystem, this.MinNumber, this.MaxNumber, out error);

			if ( null != error )
				return new UltraCalcValue( error );
			// ------------------------------------------------------------------------------------------------------------

            return new UltraCalcValue(value);
        }

        #region Abstract methods
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        internal abstract protected long MinNumber { get; }

        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        internal abstract protected long MaxNumber { get; }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        internal abstract protected int NumberSystem { get; }

        /// <summary>
        /// The maximum length of the number string.
        /// </summary>
        internal abstract protected long MaxLength { get; }
        #endregion //Abstract methods

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion //Min/Max args

        #region ConvertFromNumberSystem


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		// SSP 8/22/11 TFS84369
		// Added error out parameter.
		// 
        //internal static long ConvertFromNumberSystem(string number, int numberSystem, long minNumber, long maxNumber)
		// MD 4/6/12 - TFS101506
		//internal static long ConvertFromNumberSystem( string number, int numberSystem, long minNumber, long maxNumber, out UltraCalcErrorValue error )
		internal static long ConvertFromNumberSystem(CultureInfo culture, string number, int numberSystem, long minNumber, long maxNumber, out UltraCalcErrorValue error)
        {
			error = null;

            number = number.Trim();

            long total = 0;
            int charCount = number.Length;
            char[] chars = number.ToCharArray();

            long multiplier = 1;

			// SSP 8/22/11 TFS84369
			// 
			char errorChar = (char)0;

            for (int i = charCount - 1; i >= 0; i--)
            {
                char c = chars[i];
                if (c == '-')
                    continue;

                long digit;
                bool success = long.TryParse(
                    c.ToString(),
                    NumberStyles.Float | NumberStyles.AllowThousands,
					// MD 4/6/12 - TFS101506
                    //CultureInfo.InvariantCulture,
					culture,
                    out digit);

                if (success == false)
                {
                    digit = ((int)c) - 65;
                    digit += 10;
                }

				// SSP 8/22/11 TFS84369
				// Added error out parameter. If we encounter a digit in the number that's invalid for the number
				// system then return an error instead of a correct result.
				// 
				if ( digit >= numberSystem )
				{
					errorChar = c;
					continue;
				}

                total += digit * multiplier;

                multiplier *= numberSystem;
            }

			// SSP 8/22/11 TFS84369
			// Added error out parameter.
			// 
			if ( 0 != errorChar )
			{
				error = new UltraCalcErrorValue( UltraCalcErrorCode.Num, SR.GetString( "Error_InvalidNumberSystemDigit", errorChar, number, numberSystem ) );
				return 0;
			}

            bool isNegative = ((total & -minNumber) != 0);
            if (isNegative)
            {
                total &= ~(-minNumber);
                total += minNumber;
            }

            return total;
        }
        #endregion //ConvertFromNumberSystem



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionXBase2Dec

    #region UltraCalcFunctionBin2Dec
    /// <summary>
    /// Returns the decimal value of a string representation of a number in a binary system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Bin2Dec(number)
    /// <br/>
    /// Number is a string represeting a binary number.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionBin2Dec : UltraCalcFunctionXBase2Dec
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "bin2dec"; }
        }
        #endregion //Name

        #region MinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberBin; }
        }
        #endregion //MinNumber

        #region MaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberBin; }
        }
        #endregion //MaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert from (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NumberSystem
        {
            get { return 2; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength
    }
    #endregion //UltraCalcFunctionXBase2Dec

    #region UltraCalcFunctionHex2Dec
    /// <summary>
    /// Returns the decimal value of a string representation of a number in a hexadecimal system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Hex2Dec(number)
    /// <br/>
    /// Number is a string represeting a hexadecimal number.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionHex2Dec : UltraCalcFunctionXBase2Dec
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "hex2dec"; }
        }
        #endregion //Name

        #region MinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberHex; }
        }
        #endregion //MinNumber

        #region MaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberHex; }
        }
        #endregion //MaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert to (2 = hexadecimal, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NumberSystem
        {
            get { return 16; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength
    }
    #endregion //UltraCalcFunctionXBase2Dec

    #region UltraCalcFunctionOct2Dec
    /// <summary>
    /// Returns the decimal value of a string representation of a number in a octal system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Oct2Dec(number)
    /// <br/>
    /// Number is a string represeting a octal number.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionOct2Dec : UltraCalcFunctionXBase2Dec
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "oct2dec"; }
        }
        #endregion //Name

        #region MinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberOct; }
        }
        #endregion //MinNumber

        #region MaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long MaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberOct; }
        }
        #endregion //MaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert to (2 = octal, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NumberSystem
        {
            get { return 8; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength
    }
    #endregion //UltraCalcFunctionXBase2Dec


    #region UltraCalcFunctionXBase2XBase
    /// <summary>
    /// Converts a string representing a number in a non-decimal number system to a number in another non-decimal number system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// XBase2XBase(number, places)
    /// <br/>
    /// Number is the value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





		abstract class UltraCalcFunctionXBase2XBase : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            int minPlaces = 0;

            if (argumentCount == 2)
            {
                int places = numberStack.Pop().ToInt32();
                if (places <= 0)
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
                else
                    minPlaces = places;
            }

            UltraCalcValue numberArg = numberStack.Pop();
            if (numberArg.IsError)
                return new UltraCalcValue(numberArg.ToErrorValue());

            string number = numberArg.ToString();

            if (number.Length > this.MaxLength)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			// SSP 8/22/11 TFS84369
			// Added error out parameter.
			// 
			// --------------------------------------------------------------------------------------------------------------------
            //long decimalValue = UltraCalcFunctionXBase2Dec.ConvertFromNumberSystem(number, this.OriginalNumberSystem, this.OriginalMinNumber, this.OriginalMaxNumber);
			UltraCalcErrorValue error;

			// MD 4/9/12 - TFS101506
			//long decimalValue = UltraCalcFunctionXBase2Dec.ConvertFromNumberSystem( number, this.OriginalNumberSystem, this.OriginalMinNumber, this.OriginalMaxNumber, out error );
			long decimalValue = UltraCalcFunctionXBase2Dec.ConvertFromNumberSystem(numberStack.Culture, number, this.OriginalNumberSystem, this.OriginalMinNumber, this.OriginalMaxNumber, out error);

			if ( null != error )
				return new UltraCalcValue( error );
			// --------------------------------------------------------------------------------------------------------------------

            string convertedString = UltraCalcFunctionDec2XBase.ConvertToNumberSystem(decimalValue, this.NewNumberSystem, this.NewMinNumber, this.NewMaxNumber);

            if (minPlaces > 0)
            {   
                // For some reason, Excel does not return an error condition when the result is 
                // more than 10 digits. 
                if (convertedString.Length < 10 &&
                    convertedString.Length > minPlaces)
                {
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
                }
            }
            else
                minPlaces = 1;

            while (convertedString.Length < minPlaces)
            {
                convertedString = string.Format("{0}{1}", "0", convertedString);
            }

            return new UltraCalcValue(convertedString);
        }

        #region Abstract methods
        /// <summary>
        /// The minimum value that the number argument to this function will allow in the original number system.
        /// </summary>
        internal abstract protected long OriginalMinNumber { get; }

        /// <summary>
        /// The maximum value that the number argument to this function will allow in the original number system.
        /// </summary>
        internal abstract protected long OriginalMaxNumber { get; }

        /// <summary>
        /// The minimum value that the number argument to this function will allow in the new number system.
        /// </summary>
        internal abstract protected long NewMinNumber { get; }

        /// <summary>
        /// The maximum value that the number argument to this function will allow in the new number system.
        /// </summary>
        internal abstract protected long NewMaxNumber { get; }

        /// <summary>
        /// The number system to convert from (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        internal abstract protected int OriginalNumberSystem { get; }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        internal abstract protected int NewNumberSystem { get; }

        /// <summary>
        /// The maximum length of the number string.
        /// </summary>
        internal abstract protected long MaxLength { get; }

        #endregion //Abstract methods

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion //Min/Max args
    }
    #endregion //UltraCalcFunctionXBase2XBase


    #region UltraCalcFunctionBin2Oct
    /// <summary>
    /// Converts a string representing a binary number  to a string representing the same number in the octal number system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Bin2Oct(number, places)
    /// <br/>
    /// Number is the value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionBin2Oct : UltraCalcFunctionXBase2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "bin2oct"; }
        }
        #endregion //Name

        #region OriginalMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberBin; }
        }
        #endregion //OriginalMinNumber

        #region OriginalMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberBin; }
        }
        #endregion //OriginalMaxNumber

        #region NewMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberOct; }
        }
        #endregion //NewMinNumber

        #region NewMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberOct; }
        }
        #endregion //NewMaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert from (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int OriginalNumberSystem
        {
            get { return 2; }
        }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NewNumberSystem
        {
            get { return 8; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength

    }
    #endregion //UltraCalcFunctionBin2Oct

    #region UltraCalcFunctionBin2Hex
    /// <summary>
    /// Converts a string representing a binary number  to a string representing the same number in the hexadecimal number system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Bin2Hex(number, places)
    /// <br/>
    /// Number is the value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionBin2Hex : UltraCalcFunctionXBase2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "bin2hex"; }
        }
        #endregion //Name

        #region OriginalMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberBin; }
        }
        #endregion //OriginalMinNumber

        #region OriginalMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberBin; }
        }
        #endregion //OriginalMaxNumber

        #region NewMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberHex; }
        }
        #endregion //NewMinNumber

        #region NewMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberHex; }
        }
        #endregion //NewMaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert from (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int OriginalNumberSystem
        {
            get { return 2; }
        }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NewNumberSystem
        {
            get { return 16; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength

    }
    #endregion //UltraCalcFunctionBin2Hex


    #region UltraCalcFunctionOct2Bin
    /// <summary>
    /// Converts a string representing an octal number to a string representing the same number in the binary number system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Oct2Bin(number, places)
    /// <br/>
    /// Number is the value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionOct2Bin : UltraCalcFunctionXBase2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "oct2bin"; }
        }
        #endregion //Name

        #region OriginalMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberOct; }
        }
        #endregion //OriginalMinNumber

        #region OriginalMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberOct; }
        }
        #endregion //OriginalMaxNumber

        #region NewMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberBin; }
        }
        #endregion //NewMinNumber

        #region NewMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberBin; }
        }
        #endregion //NewMaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert from (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int OriginalNumberSystem
        {
            get { return 8; }
        }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NewNumberSystem
        {
            get { return 2; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength

    }
    #endregion //UltraCalcFunctionBin2Oct

    #region UltraCalcFunctionOct2Hex
    /// <summary>
    /// Converts a string representing an octal number to a string representing the same number in the hexadecimal number system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Oct2Hex(number, places)
    /// <br/>
    /// Number is the value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionOct2Hex : UltraCalcFunctionXBase2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "oct2hex"; }
        }
        #endregion //Name

        #region OriginalMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberOct; }
        }
        #endregion //OriginalMinNumber

        #region OriginalMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberOct; }
        }
        #endregion //OriginalMaxNumber

        #region NewMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberHex; }
        }
        #endregion //NewMinNumber

        #region NewMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberHex; }
        }
        #endregion //NewMaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert from (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int OriginalNumberSystem
        {
            get { return 8; }
        }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NewNumberSystem
        {
            get { return 16; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength

    }
    #endregion //UltraCalcFunctionHex2Oct


    #region UltraCalcFunctionHex2Bin
    /// <summary>
    /// Converts a string representing an hexadecimal number to a string representing the same number in the binary number system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Hex2Bin(number, places)
    /// <br/>
    /// Number is the value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionHex2Bin : UltraCalcFunctionXBase2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "hex2bin"; }
        }
        #endregion //Name

        #region OriginalMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberHex; }
        }
        #endregion //OriginalMinNumber

        #region OriginalMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberHex; }
        }
        #endregion //OriginalMaxNumber

        #region NewMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberBin; }
        }
        #endregion //NewMinNumber

        #region NewMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberBin; }
        }
        #endregion //NewMaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert from (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int OriginalNumberSystem
        {
            get { return 16; }
        }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NewNumberSystem
        {
            get { return 2; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength

    }
    #endregion //UltraCalcFunctionBin2Hex

    #region UltraCalcFunctionHex2Oct
    /// <summary>
    /// Converts a string representing an hexadecimal number to a string representing the same number in the octal number system.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Hex2Oct(number, places)
    /// <br/>
    /// Number is the value to be converted.
    /// <br/>
    /// Places is the number of characters to use in representing the value. If places is not specified, the mimumum number of characters neccessary to represent the value will be used. Places allows you to specify leading zeros.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionHex2Oct : UltraCalcFunctionXBase2XBase
    {
        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "hex2oct"; }
        }
        #endregion //Name

        #region OriginalMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberHex; }
        }
        #endregion //OriginalMinNumber

        #region OriginalMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow.
        /// </summary>
        protected internal override long OriginalMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberHex; }
        }
        #endregion //OriginalMaxNumber

        #region NewMinNumber
        /// <summary>
        /// The minimum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMinNumber
        {
            get { return UltraCalcFunctionDec2XBase.MinNumberOct; }
        }
        #endregion //NewMinNumber

        #region NewMaxNumber
        /// <summary>
        /// The maximum value that the number argument to this function will allow in the new number system.
        /// </summary>
        protected internal override long NewMaxNumber
        {
            get { return UltraCalcFunctionDec2XBase.MaxNumberOct; }
        }
        #endregion //NewMaxNumber

        #region NumberSystem
        /// <summary>
        /// The number system to convert from (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int OriginalNumberSystem
        {
            get { return 16; }
        }

        /// <summary>
        /// The number system to convert to (2 = binary, 8 = octal, 16 = Hexadecimal, etc.)
        /// </summary>
        protected internal override int NewNumberSystem
        {
            get { return 8; }
        }
        #endregion //NumberSystem

        #region MaxLength
        /// <summary>
        /// The maximum length of the 'number' string.
        /// </summary>
        protected internal override long MaxLength
        {
            get { return 10; }
        }
        #endregion //MaxLength

    }
    #endregion //UltraCalcFunctionOct2Hex


    #region UltraCalcFunctionConvert
    /// <summary>
    /// Converts a value from one system of measurement to another. For example, meters to inches or hours to seconds.
    /// </summary>
    /// <remarks>
    /// <p class="body">CONVERT(number, fromUnit, toUnit)
    /// <br/>
    /// Number is the value to convert.
    /// <br/>
    /// FromUnit is the unit in which the number is given.
    /// <br/>
    /// ToUnit is the units to convert to.
    /// <br/>
    /// Weight and mass units:
    /// "g" = gram
    /// "sg" = slug
    /// "lbm" = pound mass (avoirdupois)
    /// "u" = U (atomic mass unit)
    /// "ozm" = Ounce mass (avoirdupois)
    /// <br/>
    /// Distance units:
    /// "m" = Meter
    /// "mi" = Statute mile
    /// "Nmi" = Nautical mile 
    /// "in" = Inch
    /// "ft" = Foot 
    /// "yd" = Yard
    /// "ang" = Angstrom
    /// "Pica" = Pica (1/72 in.)    
    /// <br/>
    /// Time units:
    /// "yr" = Year
    /// "day" = Day
    /// "hr" = Hour
    /// "mn" = Minute
    /// "sec" = Second
    /// <br/>
    /// Pressure units
    /// "Pa" (or "p") = Pascal
    /// "atm" (or "at") = Atmosphere
    /// "mmHg" = mm of Mercury
    /// <br/>
    /// Force units
    /// "N" = Newton
    /// "dyn" (or "dy") = Dyne
    /// "lbf" = Pound force
    /// <br/>
    /// Energy units:
    /// "J" = Joule
    /// "e" = Erg
    /// "c" = Thermodynamic calorie
    /// "cal" = IT calorie
    /// "eV" (or "ev") = Electron volt
    /// "HPh" (or "hh") = Horsepower-hour
    /// "Wh" (or "wh") = Watt-hour
    /// "flb" = Foot-pound
    /// "BTU" (or "btu") = BTU
    /// <br/>
    /// Power units:
    /// "HP" (or "h") = Horsepower
    /// "W" (or "w") = Watt
    /// <br/>
    /// Magentism units:
    /// "T" = Tesla
    /// "ga" = Gauss
    /// <br/>
    /// Temperature units:
    /// "C" (or "cel") = Degree Celsius
    /// "F" (or "fah") = Degree Fahrenheit
    /// "K" (or "kel") = Kelvin
    /// <br/>
    /// Liquid measure units:
    /// "tsp" = Teaspoon
    /// "tbs" = Tablespoon
    /// "oz" = Fluid ounce
    /// "cup" = Cup
    /// "pt" (or "us_pt") = U.S. pint
    /// "uk_pt" = U.K. pint
    /// "qt" = Quart
    /// "gal" = Gallon
    /// "l" (or "lt") = Liter
    /// <br/>
    /// Any metric ToUnit or FromUnit may be prefixed with one of the following:
    /// exa ("E") = 1E+18
    /// peta ("P") = 1E+15
    /// tera ("T") = 1E+12
    /// giga ("G") = 1E+09
    /// mega ("M") = 1E+06
    /// kilo ("k") = 1E+03
    /// hecto ("h") = 1E+02
    /// dekao ("e") = 1E+01
    /// deci ("d") = 1E-01
    /// centi ("c") = 1E-02
    /// milli ("m") = 1E-03
    /// micro ("u") = 1E-06
    /// nano ("n") = 1E-09
    /// pico ("p") = 1E-12
    /// femto ("f") = 1E-15
    /// atto ("a") = 1E-18
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionConvert : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            string toUnit = numberStack.Pop().ToString();
            string fromUnit = numberStack.Pop().ToString();

            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double number = arg.ToDouble();

            UnitInfo fromUnitInfo = UnitInfo.FromString(fromUnit);
            UnitInfo toUnitInfo = UnitInfo.FromString(toUnit);

            if (fromUnitInfo == null ||
                toUnitInfo == null)
            {
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.NA));
            }

            MeasurementUnitCategory toUnitCategory = toUnitInfo.MeasurementUnitCategory;
            MeasurementUnitCategory fromUnitCategory = fromUnitInfo.MeasurementUnitCategory;

            if (toUnitCategory == MeasurementUnitCategory.Unknown ||
                fromUnitCategory == MeasurementUnitCategory.Unknown ||
                toUnitCategory != fromUnitCategory)
            {
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.NA));
            }

            fromUnit = fromUnitInfo.Unit;
            toUnit = toUnitInfo.Unit;

            number *= fromUnitInfo.PrefixMultipler;

            double value;

            switch (toUnitCategory)
            {
                case MeasurementUnitCategory.WeightAndMass:
                    value = ConvertWeight(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.Distance:
                    value = ConvertDistance(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.Time:
                    value = ConvertTime(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.Pressure:
                    value = ConvertPressure(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.Force:
                    value = ConvertForce(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.Energy:
                    value = ConvertEnergy(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.Power:
                    value = ConvertPower(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.Magnetism:
                    value = ConvertMagnetism(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.Temperature:
                    value = ConvertTemperature(number, fromUnit, toUnit);
                    break;
                case MeasurementUnitCategory.LiquidMeasure:
                    value = ConvertLiquidMeasure(number, fromUnit, toUnit);
                    break;
                default:
                    CalcManagerUtilities.DebugFail("Unknown MeasurementUnitCategory");
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.NA));
            }

            value /= toUnitInfo.PrefixMultipler;
            return new UltraCalcValue(value);
        }



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "convert"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 3; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 3; }
        }
        #endregion // Min/Max args

        #region ConvertTime
        private static double ConvertTime(double number, string fromUnit, string toUnit)
        {
            const double daysPerYear = 365.25;
            const double hoursPerDay = 24;
            const double minutesPerHour = 60;
            const double secondsPerMinute = 60;            

            double seconds = 0;

            switch (fromUnit)
            {
                case "yr": // Year
                    seconds = number * daysPerYear * hoursPerDay * minutesPerHour * secondsPerMinute;
                    break;

                case "day": // Day
                    seconds = number * hoursPerDay * minutesPerHour * secondsPerMinute;
                    break;

                case "hr": // Hour                    
                    seconds = number * minutesPerHour * secondsPerMinute;
                    break;

                case "mn": // Minute                    
                    seconds = number * secondsPerMinute;
                    break;

                case "sec": // Second                    
                    seconds = number;
                    break;
                default:
                    throw new ArgumentException("fromUnit");
            }

            int secondsInt = (int)Math.Round(seconds, 0);
            int millisecondsInt = (int)((seconds - (double)secondsInt) * (double)1000);
            TimeSpan timeSpan = new TimeSpan(0, 0, 0, secondsInt, millisecondsInt);

            switch (toUnit)
            {
                case "yr": // Year
                    return timeSpan.TotalDays / daysPerYear;
                case "day": // Day
                    return timeSpan.TotalDays;
                case "hr": // Hour
                    return timeSpan.TotalHours;
                case "mn": // Minute
                    return timeSpan.TotalMinutes;
                case "sec": // Second
                    return timeSpan.TotalSeconds;
                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertTime

        #region ConvertWeight
        private static double ConvertWeight(double number, string fromUnit, string toUnit)
        {
            const double gramsPerSlug = 14593.8424189287;
            const double gramsPerPoundMass = 453.592309748811;
            const double gramsPerOunceMass = 28.3495152079732;
            const double gramsPerAtomicMassUnit = 0.000000000000000000000001660531;

            double grams = 0;

            switch (fromUnit)
            {
                case "sg": // slug
                    grams = number * gramsPerSlug;
                    break;
                case "lbm": // pound mass (avoirdupois)
                    grams = number * gramsPerPoundMass;
                    break;
                case "ozm": // Ounce mass (avoirdupois)
                    grams = number * gramsPerOunceMass;
                    break;
                case "g": // gram                
                    grams = number;
                    break;
                case "u": // U (atomic mass unit)
                    grams = number * gramsPerAtomicMassUnit;
                    break;
                default:
                    throw new ArgumentException("fromUnit");
            }


            switch (toUnit)
            {
                case "u": // U (atomic mass unit)
                    return grams / gramsPerAtomicMassUnit;

                case "g": // gram                
                    return grams;

                case "ozm": // Ounce mass (avoirdupois)
                    return grams / gramsPerOunceMass;

                case "lbm": // pound mass (avoirdupois)
                    return grams / gramsPerPoundMass;

                case "sg": // slug
                    return grams / gramsPerSlug;

                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertWeight

        #region ConvertDistance
        private static double ConvertDistance(double number, string fromUnit, string toUnit)
        {
            const double inchesPerNauticalMile = 72913.3858267717;
            const double inchesPerMile = 63360;
            const double inchesPerMeter = 39.3700787401575;
            const double inchesPerYard = 36; // Note that for Excel 2007 appears to use 36.000000011811. So we won't match them exactly, but we decided to do it correctly. 
            const double inchesPerFoot = 12;
            const double inchesPerPica = 0.0138888888888898;
            const double inchesPerAngstrom = 0.00000000393700787401575;

            double inches = 0;

            switch (fromUnit)
            {
                case "Nmi": // Nautical mile 
                    inches = number * inchesPerNauticalMile;
                    break;
                case "mi": // Statute mile
                    inches = number * inchesPerMile;
                    break;
                case "m": // Meter
                    inches = number * inchesPerMeter;
                    break;
                case "yd": // Yard
                    inches = number * inchesPerYard;
                    break;
                case "ft": // Foot
                    inches = number * inchesPerFoot;
                    break;
                case "in": // Inch                
                    inches = number;
                    break;
                case "Pica": // Pica (1/72 in.)
                    inches = number * inchesPerPica;
                    break;
                case "ang": // Angstrom
                    inches = number * inchesPerAngstrom;
                    break;
                default:
                    throw new ArgumentException("fromUnit");
            }

            switch (toUnit)
            {
                case "ang": // Angstrom
                    return inches / inchesPerAngstrom;
                case "Pica": // Pica (1/72 in.)
                    return inches / inchesPerPica;
                case "in": // Inch                
                    return inches;
                case "ft": // Foot
                    return inches / inchesPerFoot;
                case "yd": // Yard
                    return inches / inchesPerYard;
                case "m": // Meter
                    return inches / inchesPerMeter;
                case "mi": // Statute mile
                    return inches / inchesPerMile;
                case "Nmi": // Nautical mile 
                    return inches / inchesPerNauticalMile;
                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertDistance

        #region ConvertPressure
        private static double ConvertPressure(double number, string fromUnit, string toUnit)
        {
            const double mmOfMercuryPERAtmosphere = 760;
            const double mmOfMercuryPERPascal = 0.00750061707998627;

            double mmOfMercury = 0;

            switch (fromUnit)
            {
                case "Pa": // Pascal
                case "p": // Pascal
                    mmOfMercury = number * mmOfMercuryPERPascal;
                    break;
                case "atm": // Atmosphere
                case "at": // Atmosphere
                    mmOfMercury = number * mmOfMercuryPERAtmosphere;
                    break;
                case "mmHg": // mm of Mercury
                    mmOfMercury = number;
                    break;
                default:
                    throw new ArgumentException("fromUnit");
            }

            switch (toUnit)
            {
                case "mmHg": // mm of Mercury
                    return mmOfMercury;
                case "Pa": // Pascal
                case "p": // Pascal
                    return mmOfMercury / mmOfMercuryPERPascal;
                case "atm": // Atmosphere
                case "at": // Atmosphere
                    return mmOfMercury / mmOfMercuryPERAtmosphere;

                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertPressure

        #region ConvertForce
        private static double ConvertForce(double number, string fromUnit, string toUnit)
        {
            const double newtonsPerPoundForce = 4.448222;
            const double newtonsPerDyne = 0.00001;

            double newtons = 0;

            switch (fromUnit)
            {
                case "lbf": // Pound force
                    newtons = number * newtonsPerPoundForce;
                    break;
                case "N": // Newton
                    newtons = number;
                    break;
                case "dyn": // Dyne
                case "dy": // Dyne
                    newtons = number * newtonsPerDyne;
                    break;
                default:
                    throw new ArgumentException("fromUnit");
            }

            switch (toUnit)
            {
                case "dyn": // Dyne
                case "dy": // Dyne
                    return newtons / newtonsPerDyne;
                case "N": // Newton
                    return newtons;
                case "lbf": // Pound force
                    return newtons / newtonsPerPoundForce;
                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertForce

        #region ConvertEnergy
        private static double ConvertEnergy(double number, string fromUnit, string toUnit)
        {
            const double ergsPerHorsePowerHour = 26845161228302.4;
            const double ergsPerWattHour = 35999964751.8369;
            const double ergsPerBTU = 10550576307.4665;
            const double ergsPerITCalorie = 41867928.3372801;
            const double ergsPerThermodynamicCalorie = 41839890.0257312;
            const double ergsPerJoule = 9999995.19343231;
            const double ergsPerFootPound = 421399.80068766;
            const double ergsPerElectronVolt = 0.00000000000160218923136574;

            double ergs;

            switch (fromUnit)
            {
                case "HPh": // Horsepower-hour
                case "hh": // Horsepower-hour
                    ergs = number * ergsPerHorsePowerHour;
                    break;
                case "Wh": // Watt-hour
                case "wh": // Watt-hour
                    ergs = number * ergsPerWattHour;
                    break;
                case "BTU": // BTU
                case "btu": // BTU
                    ergs = number * ergsPerBTU;
                    break;
                case "cal": // IT calorie
                    ergs = number * ergsPerITCalorie;
                    break;
                case "c": // Thermodynamic calorie
                    ergs = number * ergsPerThermodynamicCalorie;
                    break;
                case "J": // Joule
                    ergs = number * ergsPerJoule;
                    break;
                case "flb": // Foot-pound
                    ergs = number * ergsPerFootPound;
                    break;
                case "e": // Erg
                    ergs = number;
                    break;
                case "eV": // Electron volt
                case "ev": // Electron volt                
                    ergs = number * ergsPerElectronVolt;
                    break;

                default:
                    throw new ArgumentException("fromUnit");
            }

            switch (toUnit)
            {
                case "eV": // Electron volt
                case "ev": // Electron volt                
                    return ergs / ergsPerElectronVolt;
                case "e": // Erg
                    return ergs;
                case "flb": // Foot-pound
                    return ergs / ergsPerFootPound;
                case "J": // Joule
                    return ergs / ergsPerJoule;
                case "c": // Thermodynamic calorie
                    return ergs / ergsPerThermodynamicCalorie;
                case "cal": // IT calorie
                    return ergs / ergsPerITCalorie;
                case "BTU": // BTU
                case "btu": // BTU
                    return ergs / ergsPerBTU;
                case "Wh": // Watt-hour
                case "wh": // Watt-hour
                    return ergs / ergsPerWattHour;
                case "HPh": // Horsepower-hour
                case "hh": // Horsepower-hour
                    return ergs / ergsPerHorsePowerHour;

                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertEnergy

        #region ConvertPower
        private static double ConvertPower(double number, string fromUnit, string toUnit)
        {
            double wattsPerHorsePower = 745.701;

            double watts;

            switch (fromUnit)
            {
                case "HP": // Horsepower
                case "h": // Horsepower
                    watts = number * wattsPerHorsePower;
                    break;
                case "W": // Watt
                case "w": // Watt
                    watts = number;
                    break;
                default:
                    throw new ArgumentException("fromUnit");
            }

            switch (toUnit)
            {
                case "W": // Watt
                case "w": // Watt
                    return watts;
                case "HP": // Horsepower
                case "h": // Horsepower
                    return watts / wattsPerHorsePower;
                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertPower

        #region ConvertMagnetism
        private static double ConvertMagnetism(double number, string fromUnit, string toUnit)
        {
            double gaussPerTesla = 10000;

            double gauss;

            switch (fromUnit)
            {
                case "T": // Tesla
                    gauss = number * gaussPerTesla;
                    break;
                case "ga": // Gauss
                    gauss = number;
                    break;
                default:
                    throw new ArgumentException("fromUnit");
            }

            switch (toUnit)
            {
                case "ga": // Gauss
                    return gauss;
                case "T": // Tesla
                    return gauss / gaussPerTesla;
                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertMagnetism

        #region ConvertLiquidMeasure
        private static double ConvertLiquidMeasure(double number, string fromUnit, string toUnit)
        {
            double teaspoonsPerGallon = 768;
            double teaspoonsPerLiter = 202.84;
            double teaspoonsPerQuart = 192;
            double teaspoonsPerUKPint = 115.266;
            double teaspoonsPerPint = 96;
            double teaspoonsPerCup = 48;
            double teaspoonsPerFluidOunce = 6;
            double teaspoonsPerTablespoon = 3;

            double teaspoons;

            switch (fromUnit)
            {
                case "gal": // Gallon
                    teaspoons = number * teaspoonsPerGallon;
                    break;
                case "l": // Liter
                case "lt": // Liter
                    teaspoons = number * teaspoonsPerLiter;
                    break;
                case "qt": // Quart
                    teaspoons = number * teaspoonsPerQuart;
                    break;
                case "uk_pt": // U.K. pint
                    teaspoons = number * teaspoonsPerUKPint;
                    break;
                case "pt": // U.S. pint
                case "us_pt": // U.S. pint
                    teaspoons = number * teaspoonsPerPint;
                    break;
                case "cup": // Cup
                    teaspoons = number * teaspoonsPerCup;
                    break;
                case "oz": // Fluid ounce
                    teaspoons = number * teaspoonsPerFluidOunce;
                    break;
                case "tbs": // Tablespoon
                    teaspoons = number * teaspoonsPerTablespoon;
                    break;
                case "tsp": // Teaspoon                
                    teaspoons = number;
                    break;
                default:
                    throw new ArgumentException("fromUnit");
            }

            switch (toUnit)
            {
                case "tsp": // Teaspoon                
                    return teaspoons;
                case "tbs": // Tablespoon
                    return teaspoons / teaspoonsPerTablespoon;
                case "oz": // Fluid ounce
                    return teaspoons / teaspoonsPerFluidOunce;
                case "cup": // Cup
                    return teaspoons / teaspoonsPerCup;
                case "pt": // U.S. pint
                case "us_pt": // U.S. pint
                    return teaspoons / teaspoonsPerPint;
                case "uk_pt": // U.K. pint
                    return teaspoons / teaspoonsPerUKPint;
                case "qt": // Quart
                    return teaspoons / teaspoonsPerQuart;
                case "l": // Liter
                case "lt": // Liter
                    return teaspoons / teaspoonsPerLiter;
                case "gal": // Gallon
                    return teaspoons / teaspoonsPerGallon;

                default:
                    throw new ArgumentException("toUnit");
            }
        }
        #endregion //ConvertLiquidMeasure

        #region ConvertTemperature
        private static double ConvertTemperature(double number, string fromUnit, string toUnit)
        {
            fromUnit = GetTemperatureUnitResolved(fromUnit);
            toUnit = GetTemperatureUnitResolved(toUnit);

            if (string.Compare(fromUnit, toUnit) == 0)
                return number;

            double celcius;

            switch (fromUnit)
            {
                case "C":
                    switch (toUnit)
                    {
                        case "F":
                            return ((number * 9) / 5) + 32;
                        case "K":
                            return number + 273.15;
                        default:
                            throw new ArgumentException("toUnit");
                    }
                case "F":
                    celcius = ((number - 32) / 9) * 5;
                    switch (toUnit)
                    {
                        case "C":
                            return celcius;
                        case "K":
                            return celcius + 273.15;
                        default:
                            throw new ArgumentException("toUnit");
                    }
                case "K":
                    celcius = (number - 273.15);
                    switch (toUnit)
                    {
                        case "C":
                            return celcius;
                        case "F":
                            return ((celcius * 9) / 5) + 32;
                        default:
                            throw new ArgumentException("toUnit");
                    }
                default:
                    throw new ArgumentException("fromUnit");
            }
        }
        #endregion //ConvertTemperature

        #region GetTemperatureUnitResolved
        private static string GetTemperatureUnitResolved(string unit)
        {
            switch (unit)
            {
                case "C": // Degree Celsius                    
                case "cel": // Degree Celsius
                    return "C";
                case "F": // Degree Fahrenheit
                case "fah": // Degree Fahrenheit
                    return "F";
                case "K": // Kelvin
                case "kel": // Kelvin
                    return "K";
                default:
                    throw new ArgumentException("unit");
            }
        }
        #endregion //GetTemperatureUnitResolved

        #region UnitInfo Class
        internal class UnitInfo
        {
            string unit;
            bool isMetric;
            double prefixMulitplier;
            MeasurementUnitCategory measurementUnitCategory;

            public static UnitInfo FromString(string unit)
            {
                string baseUnit = unit;
                double prefixMultiplier = 1;

                // Try to get the category from the unit, assuming there is no metric prefix. 
                MeasurementUnitCategory measurementUnitCategory = GetUnitCategory(baseUnit);

                if (measurementUnitCategory == MeasurementUnitCategory.Unknown)
                {
                    // If we failed to find a category, assume the first character is a metric prefix and
                    // try again. 
                    baseUnit = baseUnit.Substring(1);
                    measurementUnitCategory = GetUnitCategory(baseUnit);

                    if (measurementUnitCategory == MeasurementUnitCategory.Unknown)
                    {
                        // If we still failed to find a category, something is wrong. Return null;
                        return null;
                    }
                    else
                    {
                        // If we found a category on the second pass, it means the first character is a 
                        // metric prefix.                        
                        bool success = TryGetPrefixMultiplier(unit.Substring(0, 1), out prefixMultiplier);
                        if (success == false)
                        {
                            // The prefix was not valid. Return null;
                            return null;
                        }
                    }
                }

                bool isMetric = IsMetricUnit(baseUnit);
                if (isMetric == false
                    && prefixMultiplier != 1)
                {
                    // Can't apply a multiplier prefix to a non-metric unit. 
                    return null;
                }

                return new UnitInfo(baseUnit, isMetric, prefixMultiplier, measurementUnitCategory);
            }

            private UnitInfo(string unit, bool isMetric, double prefixMulitplier, MeasurementUnitCategory measurementUnitCategory)
            {
                this.unit = unit;
                this.isMetric = isMetric;
                this.prefixMulitplier = prefixMulitplier;
                this.measurementUnitCategory = measurementUnitCategory;
            }

            #region Properties
            #region Unit
            public string Unit
            {
                get { return this.unit; }
            }
            #endregion //Unit

            #region IsMetric
            public bool IsMetric
            {
                get { return this.isMetric; }
            }
            #endregion //IsMetric

            #region PrefixMultipler
            public double PrefixMultipler
            {
                get { return this.prefixMulitplier; }
            }
            #endregion //PrefixMultipler

            #region MeasurementUnitCategory
            public MeasurementUnitCategory MeasurementUnitCategory
            {
                get { return this.measurementUnitCategory; }
            }
            #endregion //MeasurementUnitCategory
            #endregion //Properties

            #region GetUnitCategory
            private static MeasurementUnitCategory GetUnitCategory(string unit)
            {
                switch (unit)
                {
                    // Weight and Mass
                    case "g": // gram
                    case "sg": // slug
                    case "lbm": // pound mass (avoirdupois)
                    case "u": // U (atomic mass unit)
                    case "ozm": // Ounce mass (avoirdupois)
                        return MeasurementUnitCategory.WeightAndMass;

                    // Distance units:
                    case "m": // Meter
                    case "mi": // Statute mile
                    case "Nmi": // Nautical mile 
                    case "in": // Inch
                    case "ft": // Foot 
                    case "yd": // Yard
                    case "ang": // Angstrom
                    case "Pica": // Pica (1/72 in.)    
                        return MeasurementUnitCategory.Distance;

                    // Time units:
                    case "yr": // Year
                    case "day": // Day
                    case "hr": // Hour
                    case "mn": // Minute
                    case "sec": // Second
                        return MeasurementUnitCategory.Time;

                    // Pressure units
                    case "Pa": // Pascal
                    case "p": // Pascal
                    case "atm": // Atmosphere
                    case "at": // Atmosphere
                    case "mmHg": // mm of Mercury
                        return MeasurementUnitCategory.Pressure;

                    // Force units
                    case "N": // Newton
                    case "dyn": // Dyne
                    case "dy": // Dyne
                    case "lbf": // Pound force
                        return MeasurementUnitCategory.Force;

                    // Energy units:
                    case "J": // Joule
                    case "e": // Erg
                    case "c": // Thermodynamic calorie
                    case "cal": // IT calorie
                    case "eV": // Electron volt
                    case "ev": // Electron volt
                    case "HPh": // Horsepower-hour
                    case "hh": // Horsepower-hour
                    case "Wh": // Watt-hour
                    case "wh": // Watt-hour
                    case "flb": // Foot-pound
                    case "BTU": // BTU
                    case "btu": // BTU
                        return MeasurementUnitCategory.Energy;

                    // Power units:
                    case "HP": // Horsepower
                    case "h": // Horsepower
                    case "W": // Watt
                    case "w": // Watt
                        return MeasurementUnitCategory.Power;

                    // Magentism units:
                    case "T": // Tesla
                    case "ga": // Gauss
                        return MeasurementUnitCategory.Magnetism;

                    // Temperature units:
                    case "C": // Degree Celsius
                    case "cel": // Degree Celsius
                    case "F": // Degree Fahrenheit
                    case "fah": // Degree Fahrenheit
                    case "K": // Kelvin
                    case "kel": // Kelvin
                        return MeasurementUnitCategory.Temperature;

                    // Liquid measure units:
                    case "tsp": // Teaspoon
                    case "tbs": // Tablespoon
                    case "oz": // Fluid ounce
                    case "cup": // Cup
                    case "pt": // U.S. pint
                    case "us_pt": // U.S. pint
                    case "uk_pt": // U.K. pint
                    case "qt": // Quart
                    case "gal": // Gallon
                    case "l": // Liter
                    case "lt": // Liter
                        return MeasurementUnitCategory.LiquidMeasure;
                }

                return MeasurementUnitCategory.Unknown;
            }
            #endregion //GetUnitCategory

            #region TryGetPrefixMultiplier
            private static bool TryGetPrefixMultiplier(string prefix, out double multipler)
            {
                switch (prefix)
                {
                    case "E": // exa ("E") = 1E+18
                        multipler = 1E+18;
                        return true;
                    case "P": // peta ("P") = 1E+15
                        multipler = 1E+15;
                        return true;
                    case "T": // tera ("T") = 1E+12
                        multipler = 1E+12;
                        return true;
                    case "G": // giga ("G") = 1E+09
                        multipler = 1E+09;
                        return true;
                    case "M": // mega ("M") = 1E+06
                        multipler = 1E+06;
                        return true;
                    case "k": // kilo ("k") = 1E+03
                        multipler = 1E+03;
                        return true;
                    case "h": // hecto ("h") = 1E+02
                        multipler = 1E+02;
                        return true;
                    case "e": // dekao ("e") = 1E+01
                        multipler = 1E+01;
                        return true;
                    case "d": // deci ("d") = 1E-01
                        multipler = 1E-01;
                        return true;
                    case "c": // centi ("c") = 1E-02
                        multipler = 1E-02;
                        return true;
                    case "m": // milli ("m") = 1E-03
                        multipler = 1E-03;
                        return true;
                    case "u": // micro ("u") = 1E-06
                        multipler = 1E-06;
                        return true;
                    case "n": // nano ("n") = 1E-09
                        multipler = 1E-09;
                        return true;
                    case "p": // pico ("p") = 1E-12
                        multipler = 1E-12;
                        return true;
                    case "f": // femto ("f") = 1E-15
                        multipler = 1E-15;
                        return true;
                    case "a": // atto ("a") = 1E-18
                        multipler = 1E-18;
                        return true;
                }

                multipler = 1;
                return false;
            }
            #endregion //TryGetPrefixMultiplier

            #region IsMetricUnit
            private static bool IsMetricUnit(string baseUnit)
            {
                switch (baseUnit)
                {
                    // Weight and Mass
                    case "g": // gram
                    case "u": // U (atomic mass unit)
                        return true;
                    case "sg": // slug
                    case "lbm": // pound mass (avoirdupois)                    
                    case "ozm": // Ounce mass (avoirdupois)
                        return false;

                    // Distance units:
                    case "m": // Meter
                    case "ang": // Angstrom
                        return true;
                    case "mi": // Statute mile
                    case "Nmi": // Nautical mile 
                    case "in": // Inch
                    case "ft": // Foot 
                    case "yd": // Yard                    
                    case "Pica": // Pica (1/72 in.)    
                        return false;

                    // Time units:
                    case "sec": // Second
                        return true;
                    case "yr": // Year
                    case "day": // Day
                    case "hr": // Hour
                    case "mn": // Minute                    
                        return false;

                    // Pressure units
                    case "Pa": // Pascal
                    case "p": // Pascal                        
                    case "atm": // Atmosphere
                    case "at": // Atmosphere
                    case "mmHg": // mm of Mercury
                        return true;

                    // Force units
                    case "N": // Newton                        
                    case "dyn": // Dyne
                    case "dy": // Dyne
                        return true;
                    case "lbf": // Pound force
                        return false;

                    // Energy units:
                    case "J": // Joule
                    case "e": // Erg
                    case "c": // Thermodynamic calorie
                    case "cal": // IT calorie
                    case "eV": // Electron volt
                    case "ev": // Electron volt                    
                    case "Wh": // Watt-hour
                    case "wh": // Watt-hour                                        
                        return true;
                    case "HPh": // Horsepower-hour
                    case "hh": // Horsepower-hour
                    case "flb": // Foot-pound
                    case "BTU": // BTU
                    case "btu": // BTU
                        return false;

                    // Power units:
                    case "HP": // Horsepower
                    case "h": // Horsepower
                        return false;
                    case "W": // Watt
                    case "w": // Watt
                        return true;

                    // Magentism units:
                    case "T": // Tesla
                    case "ga": // Gauss
                        return true;

                    // Temperature units:
                    case "C": // Degree Celsius
                    case "cel": // Degree Celsius
                    case "F": // Degree Fahrenheit
                    case "fah": // Degree Fahrenheit
                        return false;
                    case "K": // Kelvin
                    case "kel": // Kelvin
                        return true;

                    // Liquid measure units:
                    case "tsp": // Teaspoon
                    case "tbs": // Tablespoon
                    case "oz": // Fluid ounce
                    case "cup": // Cup
                    case "pt": // U.S. pint
                    case "us_pt": // U.S. pint
                    case "uk_pt": // U.K. pint
                    case "qt": // Quart
                    case "gal": // Gallon
                        return false;
                    case "l": // Liter
                    case "lt": // Liter
                        return true;
                }

                throw new ArgumentException("baseUnit must be a unit with no metric prefix", "baseUnit");
            }
            #endregion //IsMetricUnit            

        }
        #endregion //UnitInfo Class

        #region MeasurementUnitCategory Enum
        internal enum MeasurementUnitCategory
        {
            Unknown,
            WeightAndMass,
            Distance,
            Time,
            Pressure,
            Force,
            Energy,
            Power,
            Magnetism,
            Temperature,
            LiquidMeasure
        }
        #endregion //MeasurementUnitCategory Enum
    }
    #endregion //UltraCalcFunctionConvert

    #region UltraCalcFunctionDelta
    /// <summary>
    /// Compares two numbers and returns 1 if they are equal or 0 if they are not. 
    /// </summary>
    /// <remarks>
    /// <p class="body">DELTA(number1, [number2])
    /// <br/>
    /// Number1 is a number.
    /// <br/>
    /// Number2 is a number. If Number2 is not specified, Number1 will be compared to 0.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionDelta : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg;
            double number2;
            if (argumentCount == 2)
            {
                arg = numberStack.Pop();
                if (arg.IsError)
                    return new UltraCalcValue(arg.ToErrorValue());

                number2 = arg.ToDouble();
            }
            else
                number2 = 0;

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double number1 = arg.ToDouble();

            double value = (number1 == number2)
                ? 1
                : 0;

            return new UltraCalcValue(value);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "delta"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionDelta

    #region UltraCalcFunctionGeStep
    /// <summary>
    /// Compares two numbers and returns 1 the first number is greater than or equal to the second or returns 0 if not.
    /// </summary>
    /// <remarks>
    /// <p class="body">GESTEP(number, [step])
    /// <br/>
    /// Number is a number.
    /// <br/>
    /// Step is a number. If step is not specified, Number will be compared to 0.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionGeStep : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg;
            double number2;
            if (argumentCount == 2)
            {
                arg = numberStack.Pop();
                if (arg.IsError)
                    return new UltraCalcValue(arg.ToErrorValue());

                number2 = arg.ToDouble();
            }
            else
                number2 = 0;

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double number1 = arg.ToDouble();

            double value = (number1 >= number2)
                ? 1
                : 0;

            return new UltraCalcValue(value);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "gestep"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionGeStep


    #region UltraCalcFunctionComplex
    /// <summary>
    /// Returns a complex number represented as a string in the format "x + yi" or "x + yj" by comining a real and imaginary number.
    /// </summary>
    /// <remarks>
    /// <p class="body">COMPLEX(realNumber, imaginaryNumber, [suffix])
    /// <br/>
    /// RealNumber is the real coefficient of the complex number.
    /// <br/>
    /// ImaginaryNumber is the imaginary coefficient of the number.
    /// <br/>
    /// Suffix specifies the suffix to use. The acceptable values are "i" or "j". If omitted, "i" is used. Note that the suffix is case-sensitive; upper case "I" and "J" are not allowed and will result in a #VALUE error.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionComplex : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg;

            string suffix = (argumentCount == 3)
                ? numberStack.Pop().ToString()
                : "i";




            if (string.Compare(suffix, "i", false) != 0 &&
                string.Compare(suffix, "j", false) != 0)

            {
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
            }

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double imaginaryNumber = arg.ToDouble();

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double realNumber = arg.ToDouble();

            ComplexNumber complexNumber = new ComplexNumber(realNumber, imaginaryNumber, suffix);
            string value = complexNumber.ToString();

            return new UltraCalcValue(value);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "complex"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 3; }
        }
        #endregion // Min/Max args

        #region ComplexNumber Class
        internal class ComplexNumber
        {
            #region Private Members
            private static ComplexNumber invalidComplexNumber;
            private static char[] signs = new char[] { '+', '-' };
            private static double log10Multiplier = Math.Log10(Math.E);
            private static double log2Multiplier = Math.Log(Math.E, 2);

            double realNumber;
            double imaginaryNumber;
            string suffix;
            bool isValid;
            #endregion //Private Members

            #region Constructor
            internal ComplexNumber(double realNumber, double imaginaryNumber, string suffix)
                : this(realNumber, imaginaryNumber, suffix, true)
            {
            }

			// MD 4/6/12 - TFS101506
			//internal ComplexNumber(string complexNumber)
			//{
			//    this.isValid = TryParseComplexNumber(complexNumber, out this.realNumber, out this.imaginaryNumber, out this.suffix);
			//}
			internal ComplexNumber(CultureInfo culture, string complexNumber)
			{
				this.isValid = TryParseComplexNumber(culture, complexNumber, out this.realNumber, out this.imaginaryNumber, out this.suffix);
			}

            private ComplexNumber(double realNumber, double imaginaryNumber, string suffix, bool isValid)
            {
                this.realNumber = realNumber;
                this.imaginaryNumber = imaginaryNumber;
                this.suffix = suffix;
                this.isValid = isValid;
            }
            #endregion //Constructor

            #region Properties

            #region RealNumber
            public double RealNumber
            {
                get { return this.realNumber; }
                set { this.realNumber = value; }
            }
            #endregion //RealNumber

            #region ImaginaryNumber
            public double ImaginaryNumber
            {
                get { return this.imaginaryNumber; }

				set { this.imaginaryNumber = value; } 

            }
            #endregion //ImaginaryNumber

            #region Suffix
            public string Suffix
            {
                get { return this.suffix; }
                set
                {
                    if (value != "i" &&
                        value != "j")
                        throw new ArgumentOutOfRangeException();

                    this.suffix = value;
                }
            }
            #endregion //Suffix

            #region IsValid
            public bool IsValid
            {
                get
                {
                    if (double.IsInfinity(this.RealNumber) || double.IsNaN(this.RealNumber))
                        return false;

                    if (double.IsInfinity(this.ImaginaryNumber) || double.IsNaN(this.ImaginaryNumber))
                        return false;

                    return this.isValid;
                }
            }
            #endregion //IsValid

            #endregion //Properties

            #region Base Class Overrides

            #region operator /
            public static ComplexNumber operator /(ComplexNumber dividend, ComplexNumber divisor)
            {
                if (string.Compare(divisor.Suffix, dividend.Suffix) != 0)
                    return ComplexNumber.InvalidComplexNumber;

                double d = Math.Pow(divisor.RealNumber, 2) + Math.Pow(divisor.ImaginaryNumber, 2);

                double newRealNumber = ((dividend.RealNumber * divisor.RealNumber) + (dividend.ImaginaryNumber * divisor.ImaginaryNumber)) / d;
                double newImaginaryNumber = ((dividend.ImaginaryNumber * divisor.RealNumber) - (dividend.RealNumber * divisor.ImaginaryNumber)) / d;

                ComplexNumber result = new ComplexNumber(newRealNumber, newImaginaryNumber, dividend.Suffix);
                return result;
            }
            #endregion //operator /

            #region operator *
            public static ComplexNumber operator *(ComplexNumber complexNumber1, ComplexNumber complexNumber2)
            {
                if (string.Compare(complexNumber1.Suffix, complexNumber2.Suffix) != 0)
                    return ComplexNumber.InvalidComplexNumber;

                double newRealNumber = ((complexNumber1.RealNumber * complexNumber2.RealNumber) - (complexNumber1.ImaginaryNumber * complexNumber2.ImaginaryNumber));
                double newImaginaryNumber = ((complexNumber1.RealNumber * complexNumber2.ImaginaryNumber) + (complexNumber1.ImaginaryNumber * complexNumber2.RealNumber));

                ComplexNumber result = new ComplexNumber(newRealNumber, newImaginaryNumber, complexNumber1.Suffix);
                return result;
            }

            public static ComplexNumber operator *(ComplexNumber complexNumber1, double d)
            {
                string suffix = complexNumber1.Suffix;

                double newRealNumber = complexNumber1.RealNumber * d;
                double newImaginaryNumber = complexNumber1.ImaginaryNumber * d;

                return new ComplexNumber(newRealNumber, newImaginaryNumber, suffix);
            }
            #endregion //operator *

            #region operator +
            public static ComplexNumber operator +(ComplexNumber complexNumber1, ComplexNumber complexNumber2)
            {
                if (string.Compare(complexNumber1.Suffix, complexNumber2.Suffix) != 0)
                    return ComplexNumber.InvalidComplexNumber;

                double newRealNumber = complexNumber1.RealNumber + complexNumber2.RealNumber;
                double newImaginaryNumber = complexNumber1.ImaginaryNumber + complexNumber2.ImaginaryNumber;

                ComplexNumber result = new ComplexNumber(newRealNumber, newImaginaryNumber, complexNumber1.Suffix);
                return result;
            }
            #endregion //operator +

            #region operator -
            public static ComplexNumber operator -(ComplexNumber complexNumber1, ComplexNumber complexNumber2)
            {
                if (string.Compare(complexNumber1.Suffix, complexNumber2.Suffix) != 0)
                    return ComplexNumber.InvalidComplexNumber;

                double newRealNumber = complexNumber1.RealNumber - complexNumber2.RealNumber;
                double newImaginaryNumber = complexNumber1.ImaginaryNumber - complexNumber2.ImaginaryNumber;

                ComplexNumber result = new ComplexNumber(newRealNumber, newImaginaryNumber, complexNumber1.Suffix);
                return result;
            }
            #endregion //operator -

            #region ToString
            public override string ToString()
            {
                if (this.IsValid == false)
                    return "#NUM";

                return Complex(this.RealNumber, this.ImaginaryNumber, this.Suffix);
            }
            #endregion //ToString

            #endregion //Base Class Overrides

            #region Methods

            #region Abs
            internal double Abs()
            {
                return Math.Sqrt(Math.Pow(this.RealNumber, 2) + Math.Pow(this.ImaginaryNumber, 2));
            }
            #endregion //Abs

            #region Argument
            internal double Argument()
            {
                // This is what excel does, I don't know why. 
                if (this.ImaginaryNumber == 0 &&
                    this.RealNumber == 0)
                {
                    throw new DivideByZeroException();
                }

                return Math.Atan2(this.ImaginaryNumber, this.RealNumber);
            }
            #endregion //Argument

            #region Conjugate
            internal ComplexNumber Conjugate()
            {
                return new ComplexNumber(this.realNumber, -this.imaginaryNumber, this.suffix);
            }
            #endregion //Conjugate

            #region Cos
            internal ComplexNumber Cos()
            {
                double newRealNumber = Math.Cos(this.RealNumber) * Math.Cosh(this.ImaginaryNumber);
                double newImaginaryNumber = -(Math.Sin(this.RealNumber) * Math.Sinh(this.ImaginaryNumber));

                return new ComplexNumber(newRealNumber, newImaginaryNumber, this.Suffix);
            }
            #endregion //Cos

            #region Exponential
            internal ComplexNumber Exponential()
            {
                double d = Math.Pow(Math.E, this.RealNumber);
                double newRealNumber = Math.Cos(this.ImaginaryNumber) * d;
                double newImaginaryNumber = Math.Sin(this.ImaginaryNumber) * d;
                ComplexNumber result = new ComplexNumber(newRealNumber, newImaginaryNumber, this.Suffix);
                return result;
            }
            #endregion //Exponential

            #region Hypotenuse
            internal double Hypotenuse()
            {
                double realNumberSquared = Math.Pow(this.RealNumber, 2);
                double imaginaryNumberSquared = Math.Pow(this.ImaginaryNumber, 2);
                double sum = realNumberSquared + imaginaryNumberSquared;
                double sqrt = Math.Sqrt(sum);

                return sqrt;
            }
            #endregion //Hypotenuse

            #region LN
            internal ComplexNumber LN()
            {
                double r = this.Hypotenuse();
                double newRealNumber = Math.Log(r);

                double newImaginaryNumber = this.Theta();

                UltraCalcFunctionComplex.ComplexNumber result = new UltraCalcFunctionComplex.ComplexNumber(newRealNumber, newImaginaryNumber, this.Suffix);
                return result;
            }
            #endregion //LN

            #region Log10
            internal ComplexNumber Log10()
            {
                return this.LN() * log10Multiplier;
            }
            #endregion //LNLog10

            #region Log2
            internal ComplexNumber Log2()
            {
                return this.LN() * log2Multiplier;
            }
            #endregion //Log2

            #region Power
            internal ComplexNumber Power(double power)
            {
                double r = this.Hypotenuse();
                double theta = this.Theta();

                double rPower = Math.Pow(r, power);
                double thetaPower = theta * power;

                double newRealNumber = rPower * (Math.Cos(thetaPower));

                double newImaginaryNumber = rPower * (Math.Sin(thetaPower));

                return new ComplexNumber(newRealNumber, newImaginaryNumber, this.Suffix);
            }
            #endregion //Power

            #region Sin
            internal ComplexNumber Sin()
            {
                double newRealNumber = Math.Sin(this.RealNumber) * Math.Cosh(this.ImaginaryNumber);
                double newImaginaryNumber = Math.Cos(this.RealNumber) * Math.Sinh(this.ImaginaryNumber);

                return new ComplexNumber(newRealNumber, newImaginaryNumber, this.Suffix);
            }
            #endregion //Sin

            #region Sqrt
            internal ComplexNumber Sqrt()
            {
                double r = this.Hypotenuse();
                double theta = this.Theta();

                double sqrtR = Math.Sqrt(r);
                double thetaOver2 = theta / 2;

                double newRealNumber = sqrtR * Math.Cos(thetaOver2);
                double newImaginaryNumber = sqrtR * Math.Sin(thetaOver2);

                return new ComplexNumber(newRealNumber, newImaginaryNumber, this.Suffix);
            }
            #endregion //Sqrt

            #region Theta
            internal double Theta()
            {
                return Math.Atan2(this.ImaginaryNumber, this.RealNumber);
            }
            #endregion //Theta

            #endregion //Methods

            #region Static Methods

            #region TryParseComplexNumber
			// MD 4/6/12 - TFS101506
            //internal static bool TryParseComplexNumber(string complexNumber, out double realNumber, out double imaginaryNumber, out string suffix)
			internal static bool TryParseComplexNumber(CultureInfo culture, string complexNumber, out double realNumber, out double imaginaryNumber, out string suffix)
            {
                complexNumber = complexNumber.Replace(" ", "");

                string[] complexNumberParts = SplitComplexNumber(complexNumber);
                int partCount = complexNumberParts.Length;

                if (partCount > 2 ||
                    partCount <= 0)
                {
                    realNumber = 0;
                    imaginaryNumber = 0;
                    suffix = "i";
                    return false;
                }

                string realNumberPart;
                string imaginaryNumberPart;
                bool success;

                if (partCount == 1)
                {
                    imaginaryNumberPart = complexNumber;

					// MD 4/6/12 - TFS101506
                    //success = TryParseImaginaryNumber(complexNumber, out imaginaryNumber, out suffix);
					success = TryParseImaginaryNumber(culture, complexNumber, out imaginaryNumber, out suffix);

                    if (success)
                    {
                        realNumber = 0;
                    }
                    else
                    {
                        imaginaryNumber = 0;

						// MD 4/9/12 - TFS101506
						//success = double.TryParse(
						//    complexNumber,
						//    NumberStyles.Float | NumberStyles.AllowThousands,
						//    CultureInfo.InvariantCulture,
						//    out realNumber);
						success = MathUtilities.DoubleTryParse(complexNumber, culture, out realNumber);

                        if (success == false)
                            return false;
                    }
                }
                else // part count must equal 2
                {
                    realNumberPart = complexNumberParts[0];
                    imaginaryNumberPart = complexNumberParts[1];

					// MD 4/9/12 - TFS101506
					//success = double.TryParse(
					//    realNumberPart,
					//    NumberStyles.Float | NumberStyles.AllowThousands,
					//    CultureInfo.InvariantCulture,
					//    out realNumber);
					success = MathUtilities.DoubleTryParse(realNumberPart, culture, out realNumber);

                    if (success == false)
                    {
                        imaginaryNumber = 0;
                        suffix = "i";
                        return false;
                    }

					// MD 4/6/12 - TFS101506
                    //success = TryParseImaginaryNumber(imaginaryNumberPart, out imaginaryNumber, out suffix);
					success = TryParseImaginaryNumber(culture, imaginaryNumberPart, out imaginaryNumber, out suffix);

                    if (success == false)
                        return false;
                }

                return true;
            }

			// MD 4/6/12 - TFS101506
            //private static bool TryParseImaginaryNumber(string imaginaryNumberString, out double imaginaryNumber, out string suffix)
			private static bool TryParseImaginaryNumber(CultureInfo culture, string imaginaryNumberString, out double imaginaryNumber, out string suffix)
            {
                suffix = imaginaryNumberString.Contains("j") ? "j" : "i";

                if (imaginaryNumberString.Contains("i") == false &&
                    imaginaryNumberString.Contains("j") == false)
                {
                    imaginaryNumber = 0;
                    return false;
                }

                switch (imaginaryNumberString)
                {
                    case "i":
                    case "j":
                    case "+i":
                    case "+j":
                        imaginaryNumber = 1;
                        return true;
                    case "-i":
                    case "-j":
                        imaginaryNumber = -1;
                        return true;
                }

                imaginaryNumberString = imaginaryNumberString.Replace("i", string.Empty);
                imaginaryNumberString = imaginaryNumberString.Replace("j", string.Empty);

				// MD 4/9/12 - TFS101506
				//return double.TryParse(
				//    imaginaryNumberString,
				//    NumberStyles.Float | NumberStyles.AllowThousands,
				//    CultureInfo.InvariantCulture
				//    out imaginaryNumber);
				return MathUtilities.DoubleTryParse(imaginaryNumberString, culture, out imaginaryNumber);
            }

            private static string[] SplitComplexNumber(string complexNumber)
            {
                List<string> parts = new List<string>(1);
                string part = complexNumber.Substring(0, 1);

                char[] chars = complexNumber.ToCharArray();
                for (int i = 1; i < chars.Length; i++)
                {
                    char c = chars[i];

                    if (c == '-' || c == '+')
                    {
                        parts.Add(part);
                        part = string.Empty;
                    }

                    part += c;
                }

                parts.Add(part);

                return parts.ToArray();
            }
            #endregion //TryParseComplexNumber

            #region Complex
            internal static string Complex(double realNumber, double imaginaryNumber, string suffix)
            {
                // Excel formats the numbers in regular (not scientific) notation. But 
                // I can't get DotNet to go past 15 digits after the decimal point. 
                // G19 should would seem to be the way to do it according to the docs, but it does
                // not work. 
                const string numberFormat = "G";

                string value;
                if (realNumber == 0 &&
                    imaginaryNumber == 0)
                {
                    value = "0";
                }
                else if (realNumber != 0 && imaginaryNumber == 0)
                {
                    value = realNumber.ToString(numberFormat);
                }
                else
                {
                    string sign = (imaginaryNumber > 0) ? "+" : "-";
                    string realNumberString;                    

                    if (realNumber != 0)
                    {
                        realNumberString = realNumber.ToString(numberFormat);
                    }
                    else
                    {
                        realNumberString = string.Empty;
                        if (sign == "+")
                            sign = string.Empty;
                    }

                    imaginaryNumber = Math.Abs(imaginaryNumber);

                    string imaginaryNumberString = string.Format("{0}{1}",
                        sign,
                        (imaginaryNumber == 1.0) ? string.Empty : imaginaryNumber.ToString(numberFormat)
                        );                    

                    value = string.Format("{0}{1}{2}",
                        realNumberString,
                        imaginaryNumberString,
                        suffix
                        );
                }
                return value;
            }
            #endregion //Complex

            #endregion //Static Methods

            #region Static Properties

            #region InvalidComplexNumber
            public static ComplexNumber InvalidComplexNumber
            {
                get
                {
                    if (invalidComplexNumber == null)
                        invalidComplexNumber = new ComplexNumber(0, 0, "i", false);

                    return invalidComplexNumber;
                }
            }
            #endregion //InvalidComplexNumber

            #endregion //Static Properties
        }
        #endregion //ComplexNumber Class
    }
    #endregion //UltraCalcFunctionComplex

    #region UltraCalcFunctionImAbs
    /// <summary>
    /// Returns the absolute value of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMABS(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImAbs : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
            //string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
            //UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(complexNumber.Abs());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imabs"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImAbs

    #region UltraCalcFunctionImaginary
    /// <summary>
    /// Returns the imaginary coefficient of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMAGINARY(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImaginary : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            string complexNumberString = numberStack.Pop().ToString();

			// MD 4/6/12 - TFS101506
            //UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(complexNumber.ImaginaryNumber);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imaginary"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args
    }
    #endregion //UltraCalcFunctionImaginary

    #region UltraCalcFunctionImArgument
    /// <summary>
    /// Returns the argument theta, and angle expressed in radians.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMARGUMENT(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImArgument : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
            //UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(complexNumber.Argument());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imargument"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImArgument

    #region UltraCalcFunctionImConjugate
    /// <summary>
    /// Returns the conjugate of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMCONJUGATE(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImConjugate : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.Conjugate();
            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imconjugate"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImConjugate

    #region UltraCalcFunctionImCos
    /// <summary>
    /// Returns the cosine of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMCOS(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImCos : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.Cos();
            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imcos"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImCos

    #region UltraCalcFunctionImDiv
    /// <summary>
    /// Returns the quotient of two complex numbers.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMDIV(dividend, divisor)
    /// <br/>
    /// Dividend is a complex number in the format: "x + yi" or "x + yj".
    /// <br/>
    /// Divisor is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImDiv : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string divisorString = numberStack.Pop().ToString();
			UltraCalcValue divisorArg = numberStack.Pop();
			if (divisorArg.IsError)
				return divisorArg;

			string divisorString = divisorArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber divisor = new UltraCalcFunctionComplex.ComplexNumber(divisorString);
			UltraCalcFunctionComplex.ComplexNumber divisor = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, divisorString);

            if (divisor.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string dividendString = numberStack.Pop().ToString();
			UltraCalcValue dividendArg = numberStack.Pop();
			if (dividendArg.IsError)
				return dividendArg;

			string dividendString = dividendArg.ToString();

            // MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber dividend = new UltraCalcFunctionComplex.ComplexNumber(dividendString);
			UltraCalcFunctionComplex.ComplexNumber dividend = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, dividendString);

            if (dividend.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = dividend / divisor;
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imdiv"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImDiv

    #region UltraCalcFunctionImExp
    /// <summary>
    /// Returns the exponential of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMEXP(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImExp : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.Exponential();
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imexp"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImExp

    #region UltraCalcFunctionImLn
    /// <summary>
    /// Returns the natural logarithm of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMLN(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImLn : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.LN();
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imln"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImLn

    #region UltraCalcFunctionImReal
    /// <summary>
    /// Returns the real coefficient of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMREAL(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImReal : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(complexNumber.RealNumber);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imreal"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImReal

    #region UltraCalcFunctionImSub
    /// <summary>
    /// Returns the difference between two complex numbers.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMSUB(complexNumber1, complexNumber2)
    /// <br/>
    /// ComplexNumber1 is a complex number in the format: "x + yi" or "x + yj" from which ComplexNumber2 will be subtracted.
    /// <br/>
    /// ComplexNumber2 is a complex number in the format: "x + yi" or "x + yj" which will be subtracted from ComplexNumber1.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImSub : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString2 = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg2 = numberStack.Pop();
			if (complexNumberArg2.IsError)
				return complexNumberArg2;

			string complexNumberString2 = complexNumberArg2.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber2 = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString2);
			UltraCalcFunctionComplex.ComplexNumber complexNumber2 = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString2);

            if (complexNumber2.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString1 = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg1 = numberStack.Pop();
			if (complexNumberArg1.IsError)
				return complexNumberArg1;

			string complexNumberString1 = complexNumberArg1.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber1 = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString1);
			UltraCalcFunctionComplex.ComplexNumber complexNumber1 = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString1);

            if (complexNumber1.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber1 - complexNumber2;
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imsub"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImSub

    #region UltraCalcFunctionImProduct
    /// <summary>
    /// Returns the product of 1 to n complex numbers. 
    /// </summary>
    /// <remarks>
    /// <p class="body">IMPRODUCT(complexNumber1 [, complexNumber2, complexNumber3, ..., complexNumberN])
    /// <br/>    
    /// ComplexNumber1, ComplexNumber2, ..., ComplexNumberN is any number of complex numbers in the format: "x + yi" or "x + yj" which will be multiplied together.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImProduct : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue[] args = GetArguments(numberStack, argumentCount, true);

            // Due to the imprecise nature of doubles, we need to make sure we perform the calulcations in
            // order - the same order that Excel apparently uses. 
            Array.Reverse(args);

            UltraCalcFunctionComplex.ComplexNumber product = null;
            string suffix = null;

            foreach (UltraCalcValue arg in args)
            {
                if (arg.IsError)
                    return new UltraCalcValue(arg.ToErrorValue());

                // Get the complex number from the reference. 
                // MD 4/6/12 - TFS101506
				//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(arg.ToString());
				UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, arg.ToString());

                // If the complexNumber isn't valid, return an error. 
                if (complexNumber.IsValid == false)
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

                // All the items in the list must have the same suffix
                if (suffix == null)
                {
                    // If the suffix hasn't been set, set it now. 
                    suffix = complexNumber.Suffix;
                }
                else if (complexNumber.Suffix != suffix)
                {
                    // If the suffix of this item doesn't match the first item, return an error. 
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
                }

                if (product == null)
                    product = complexNumber;
                else
                    product *= complexNumber;
            }

            if (product.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(product.ToString());
        }

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "improduct"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return int.MaxValue; }
        }
        #endregion // Min/Max args
    }
    #endregion //UltraCalcFunctionImProduct

    #region UltraCalcFunctionImSum
    /// <summary>
    /// Returns the sum of 1 to n complex numbers. 
    /// </summary>
    /// <remarks>
    /// <p class="body">IMSUM(complexNumber1 [, complexNumber2, complexNumber3, ..., complexNumberN])
    /// <br/>    
    /// ComplexNumber1, ComplexNumber2, ..., ComplexNumberN is any number of complex numbers in the format: "x + yi" or "x + yj" which will be added together.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImSum : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue[] args = GetArguments(numberStack, argumentCount, true);

            // Due to the imprecise nature of doubles, we need to make sure we perform the calulcations in
            // order - the same order that Excel apparently uses. 
            Array.Reverse(args);

            UltraCalcFunctionComplex.ComplexNumber sum = null;
            string suffix = null;

            foreach (UltraCalcValue arg in args)
            {
                if (arg.IsError)
                    return new UltraCalcValue(arg.ToErrorValue());

                // Get the complex number from the reference. 
                // MD 4/6/12 - TFS101506
				//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(arg.ToString());
				UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, arg.ToString());

                // If the complexNumber isn't valid, return an error. 
                if (complexNumber.IsValid == false)
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

                // All the items in the list must have the same suffix
                if (suffix == null)
                {
                    // If the suffix hasn't been set, set it now. 
                    suffix = complexNumber.Suffix;
                }
                else if (complexNumber.Suffix != suffix)
                {
                    // If the suffix of this item doesn't match the first item, return an error. 
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
                }

                if (sum == null)
                    sum = complexNumber;
                else
                    sum += complexNumber;
            }

            if (sum.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(sum.ToString());
        }

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imsum"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return int.MaxValue; }
        }
        #endregion // Min/Max args
    }
    #endregion //UltraCalcFunctionImSum

    #region UltraCalcFunctionImLog10
    /// <summary>
    /// Returns the base-10 logarithm of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMLOG10(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImLog10 : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.Log10();
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imlog10"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImLog10

    #region UltraCalcFunctionImLog2
    /// <summary>
    /// Returns the base-2 logarithm of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMLOG2(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImLog2 : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.Log2();
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imlog2"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImLog2

    #region UltraCalcFunctionImSin
    /// <summary>
    /// Returns the sine of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMSIN(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImSin : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.Sin();
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imsin"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImSin

    #region UltraCalcFunctionImSqrt
    /// <summary>
    /// Returns the square root of a complex number.
    /// </summary>
    /// <remarks>
    /// <p class="body">IMSQRT(complexNumber)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImSqrt : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.Sqrt();
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "imsqrt"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImSqrt

    #region UltraCalcFunctionImPower
    /// <summary>
    /// Returns the complex number raised to the specified power. 
    /// </summary>
    /// <remarks>
    /// <p class="body">IMPOWER(complexNumber, power)
    /// <br/>
    /// ComplexNumber is a complex number in the format: "x + yi" or "x + yj".    
    /// <br/>
    /// Power is the power to which ComplexNumber will be raised. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionImPower : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double power = arg.ToDouble();

			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to percolate up any errors.
			//string complexNumberString = numberStack.Pop().ToString();
			UltraCalcValue complexNumberArg = numberStack.Pop();
			if (complexNumberArg.IsError)
				return complexNumberArg;

			string complexNumberString = complexNumberArg.ToString();

			// MD 4/6/12 - TFS101506
			//UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(complexNumberString);
			UltraCalcFunctionComplex.ComplexNumber complexNumber = new UltraCalcFunctionComplex.ComplexNumber(numberStack.Culture, complexNumberString);

            if (complexNumber.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcFunctionComplex.ComplexNumber result = complexNumber.Power(power);
            if (result.IsValid == false)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(result.ToString());
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "impower"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionImPower


    #region UltraCalcFunctionDollarFr
    /// <summary>
    /// Converts a dollary amount expressed as a decimal into a dollar amount expressed as a fraction. 
    /// </summary>
    /// <remarks>
    /// <p class="body">DollarFr(decimalDollarAmount, Fraction)
    /// <br/>
    /// DecimalDollarAmount is the dollar amount expressed as a decimal. 
    /// <br/>
    /// Fraction is the value used as the denominator of the fraction. If this value is not an integer, it will be truncated. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionDollarFr : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            decimal d = arg.ToDecimal();

            if (d < 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            int denominator = (int)d;
            if (denominator == 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Div));            

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double dollarAmount = arg.ToDouble();

			double wholeDollarAmount = MathUtilities.Truncate(dollarAmount);
            double fractionDollarAmount = dollarAmount - wholeDollarAmount;

            fractionDollarAmount *= denominator;

            double divisor = CalculateBase(denominator);

            fractionDollarAmount /= divisor;

            dollarAmount = wholeDollarAmount + fractionDollarAmount;
            return new UltraCalcValue(dollarAmount);
        }

        #region CalculateBase
        internal static double CalculateBase(int denominator)
        {
            double log10 = Math.Log10(denominator);
            double round = Math.Round(log10);
            double pow = Math.Pow(10, round);

            if (pow < denominator)
                pow *= 10;

            double divisor = Math.Round(pow, 0);
            return divisor;
        }
        #endregion //CalculateBase

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "dollarfr"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args
    }
    #endregion //UltraCalcFunctionDollarFr

    #region UltraCalcFunctionDollarDe
    /// <summary>
    /// Converts a dollary amount expressed as a decimal into a dollar amount expressed as a fraction. 
    /// </summary>
    /// <remarks>
    /// <p class="body">DollarFr(decimalDollarAmount, Fraction)
    /// <br/>
    /// DecimalDollarAmount is the dollar amount expressed as a decimal. 
    /// <br/>
    /// Fraction is the value used as the denominator of the fraction. If this value is not an integer, it will be truncated. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionDollarDe : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            decimal d = arg.ToDecimal();

            if (d < 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            int denominator = (int)d;
            if (denominator == 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Div));  

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double dollarAmount = arg.ToDouble();

			double wholeDollarAmount = MathUtilities.Truncate(dollarAmount);
            double fractionDollarAmount = dollarAmount - wholeDollarAmount;

            fractionDollarAmount /= denominator;

            double multiplier = UltraCalcFunctionDollarFr.CalculateBase(denominator);
            fractionDollarAmount *= multiplier;

            dollarAmount = wholeDollarAmount + fractionDollarAmount;
            return new UltraCalcValue(dollarAmount);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "dollarde"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args
    }
    #endregion //UltraCalcFunctionDollarDe


    #region UltraCalcFunctionInfo
    /// <summary>
    /// Returns information about the current operating environment. 
    /// </summary>
    /// <remarks>
    /// <p class="body">INFO(type)
    /// <br/>
    /// Type is a string indicating the type of information to return.
    /// <br/>
    /// "directory" = The full path to the current folder.
    /// "osversion" = The currently operating system version as a string. 
    /// "system" = The current operating system. This will always return "pcdos" which indicates Microsoft Windows. The corresponding function in Excel can also return "mac" for Macintosh, but since NetAdvantage is only supported in Windows, this function will always return "pcdos". 
    /// <br/>
    /// The following are supported by Microsoft Excel, but have no correlation in UltraCalcManager: "numfile", "origin", "recalc", "release".
    /// <br/>
    /// The following are supported in older versions of Excel, but not Office2007, and have no correlation in UltraCalcManager: "memavail", "memused", "totmem".
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionInfo : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            string type = arg.ToString();


            switch (type)
            {
                case "directory": //The full path to the current folder.
                    return new UltraCalcValue(Environment.CurrentDirectory);
                case "osversion": //The currently operating system version as a string. 



                    return new UltraCalcValue(Environment.OSVersion.VersionString);

                case "system":
                    // Possible values are "pcdos" for Windows and "mac" for Macintosh. 
                    // Always reutrn "pcdos" unless we start supported NetAdvantage on a Mac.                    
                    return new UltraCalcValue("pcdos");
                case "numfile":
                case "origin":
                case "recalc":
                case "release":
                    
                    break;
                case "memavail":
                case "memused":
                case "totmem":
                    // Supported in older versions of Excel, but discontinued in Office2007 (possibly earlier). 
                    break;
            }

            return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.NA));
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "info"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args
    }
    #endregion //UltraCalcFunctionInfo

    #region UltraCalcFunctionN
    /// <summary>
    /// Converts a value to a number.
    /// </summary>
    /// <remarks>
    /// <p class="body">N(value)
    /// <br/>
    /// Value is the value to be converted to a number. The following are acceptable values:
    /// <br/>
    /// A number - returns the number.
    /// A date - returns the date.
    /// True - returns 1.
    /// False - returns 0.
    /// An error value - returns the error value. 
    /// Any other value - returns 0.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionN : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double n;
            if (arg.IsBoolean)
            {
                n = arg.ToBoolean() ? 1 : 0;
                return new UltraCalcValue(n);
            }

            object value = arg.GetResolvedValue();

            if (value is DateTime)
                return new UltraCalcValue((DateTime)value);

            if (value is string)
                return new UltraCalcValue(0);

            bool success = arg.ToDouble(out n);
            if (success)
                return new UltraCalcValue(n);

            return new UltraCalcValue(0);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "n"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionN


    #region UltraCalcFunctionFact
    /// <summary>
    /// Returns the factorial of a number.
    /// </summary>
    /// <remarks>
    /// <p class="body">FACT(number)
    /// <br/>
    /// Number is a positive number for which the factorial will be calculated. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionFact : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            decimal d = arg.ToDecimal();

            if (d < 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            int number = (int)d;

            double factorial = Factorial(number);

            if (double.IsInfinity(factorial))
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(factorial);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "fact"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args

        #region Factorial
        internal static double Factorial(int number)
        {
            return Factorial(number, 1);
        }

        internal static double Factorial(int number, int step)
        {
            if (number == 0)
                number = 1;

            double factorial = 1;
            for (int i = number; i > 1; i -= step)
            {
                factorial *= i;
            }
            return factorial;
        }
        #endregion //Factorial



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionFact

    #region UltraCalcFunctionFactDouble
    /// <summary>
    /// Returns the double factorial of a number.
    /// </summary>
    /// <remarks>
    /// <p class="body">FACTDOUBLE(number)
    /// <br/>
    /// Number is a positive number for which the double factorial will be calculated. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionFactDouble : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            decimal d = arg.ToDecimal();

            if (d < 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            int number = (int)d;

            double factorial = UltraCalcFunctionFact.Factorial(number, 2);

            if (double.IsInfinity(factorial))
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(factorial);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "factdouble"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionFactDouble

    #region UltraCalcFunctionCombin
    /// <summary>
    /// Returns the number of possible combinations given a set of items and a number of chosen items from that set.
    /// </summary>
    /// <remarks>
    /// <p class="body">Combin(number, numberChosen)
    /// <br/>
    /// Number is the number of items.    
    /// <br/>
    /// NumberChosen is the number of items chosen in each combination.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionCombin : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

			// MD 9/26/08
			// A bug was fixed with ToInt and this function was relying on the bug
            //int numberChosen = arg.ToInt();
			decimal numberChosenD = arg.ToDecimal();

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

			// MD 9/26/08
			// A bug was fixed with ToInt and this function was relying on the bug
            //int number = arg.ToInt();
			decimal numberD = arg.ToDecimal();

			// MD 9/26/08
			// A bug was fixed with ToInt and this function was relying on the bug
			//if (number < 0 ||
			//    numberChosen < 0 ||
			//    number < numberChosen)
			int numberChosen = (int)numberChosenD;
			int number = (int)numberD;

			if ( numberD < 0 ||
				numberChosenD < 0 ||
				number < numberChosen )
            {
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
            }

            double nFact = UltraCalcFunctionFact.Factorial(number);
            double numberChosenFact = UltraCalcFunctionFact.Factorial(numberChosen);

            int nMinusK = number - numberChosen;
            double nMinusKFact = UltraCalcFunctionFact.Factorial(nMinusK);

            double combinations = nFact / (numberChosenFact * nMinusKFact);

            return new UltraCalcValue(combinations);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "combin"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args
    }
    #endregion //UltraCalcFunctionCombin

    #region UltraCalcFunctionDegrees
    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <remarks>
    /// <p class="body">DEGREES(radians)
    /// <br/>
    /// Radians is a value in radians which will be converted to degrees.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionDegrees : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double radians = arg.ToDouble();


            double degrees = Degrees(radians);

            return new UltraCalcValue(degrees);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "degrees"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args

        #region Degrees
        internal static double Degrees(double radians)
        {
            const double multiplier = 180.0 / Math.PI;

            return radians * multiplier;

        }
        #endregion //Degrees



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionDegrees

    #region UltraCalcFunctionRadians
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <remarks>
    /// <p class="body">RADIANS(degrees)
    /// <br/>
    /// Degrees is a value in degrees which will be converted to radians.     
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionRadians : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double degrees = arg.ToDouble();


            double radians = Radians(degrees);

            return new UltraCalcValue(radians);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "radians"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args

        #region Radians
        internal static double Radians(double degrees)
        {
            const double multiplier = Math.PI / 180.0;

            return degrees * multiplier;

        }
        #endregion //Radians



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionRadians


    #region UltraCalcFunctionGcd
    /// <summary>
    /// Returns the greatest common divisor of integer values. 
    /// </summary>
    /// <remarks>
    /// <p class="body">GCD(number1, [number2, number3, ..., numberN])
    /// <br/>
    /// Number1, Number2, ..., NumberN is any number of integers. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionGcd : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue[] args = GetArguments(numberStack, argumentCount, true);

            int num1 = args[0].ToInt();
            if (num1 < 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            int num2;

            if (args.Length == 1)
                return new UltraCalcValue(num1);

            for (int i = 1; i < args.Length; i++)
            {
                UltraCalcValue arg = args[i];
                if (arg.IsError)
                    return new UltraCalcValue(arg.ToErrorValue());

                num2 = arg.ToInt();
                if (num2 < 0)
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

                num1 = GCD(num1, num2);
            }

            return new UltraCalcValue(num1);
        }

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "gcd"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return int.MaxValue; }
        }
        #endregion // Min/Max args

        #region GCD
        internal static int GCD(int num1, int num2)
        {
            if (num2 == 0)
                return num1;

            return GCD(num2, num1 % num2);
        }
        #endregion //GCD



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionGcd

    #region UltraCalcFunctionLcm
    /// <summary>
    /// Returns the least common multiple of integer values.
    /// </summary>
    /// <remarks>
    /// <p class="body">LCM(number1, [number2, number3, ..., numberN])
    /// <br/>
    /// Number1, Number2, ..., NumberN is any number of integers. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionLcm : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue[] args = GetArguments(numberStack, argumentCount, true);

            int num1 = args[0].ToInt();
            if (num1 < 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            int num2;

            if (args.Length == 1)
                return new UltraCalcValue(num1);

            for (int i = 1; i < args.Length; i++)
            {
                UltraCalcValue arg = args[i];
                if (arg.IsError)
                    return new UltraCalcValue(arg.ToErrorValue());

                num2 = arg.ToInt();
                if (num2 < 0)
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

                num1 = LCM(num1, num2);
            }

            return new UltraCalcValue(num1);
        }

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "lcm"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return int.MaxValue; }
        }
        #endregion // Min/Max args

        #region LCM
        internal static int LCM(int num1, int num2)
        {
            if (num1 == 0 &&
                num2 == 0)
                return 0;

            return (num1 * num2) / UltraCalcFunctionGcd.GCD(num1, num2);
        }
        #endregion //LCM



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionLcm

    #region UltraCalcFunctionMultinomial
    /// <summary>
    /// Returns the multinomial of a set of numbers. 
    /// </summary>
    /// <remarks>
    /// <p class="body">MULTINOMIAL(number1, [number2, number3, ..., numberN])
    /// <br/>
    /// Number1, Number2, ..., NumberN is any number of numbers. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionMultinomial : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue[] args = GetArguments(numberStack, argumentCount, true);

            int sum = 0;
            double divisor = 1;

            for (int i = 0; i < args.Length; i++)
            {
                UltraCalcValue arg = args[i];
                if (arg.IsError)
                    return new UltraCalcValue(arg.ToErrorValue());

                int number = arg.ToInt();

                if (number < 0)
                    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

                sum += number;

                divisor *= UltraCalcFunctionFact.Factorial(number);
            }

            double dividend = UltraCalcFunctionFact.Factorial(sum);

            double multinomial = dividend / divisor;

            return new UltraCalcValue(multinomial);
        }

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "multinomial"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return int.MaxValue; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionMultinomial


    #region UltraCalcFunctionMRound
    /// <summary>
    /// Rounds a number to the nearest multiple of another number.
    /// </summary>
    /// <remarks>
    /// <p class="body">MRound(number, multiple)
    /// <br/>
    /// Number is a number to be rounded.
    /// <br/>
    /// Multiple is a number indicating the multiple to which to round.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionMRound : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double multiple = arg.ToDouble();

            //if (multiple == 0)
            //    return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            UltraCalcValue arg2 = numberStack.Pop();
            if (arg2.IsError)
                return new UltraCalcValue(arg2.ToErrorValue());

            double number = arg2.ToDouble();

            if (number < 0 && multiple > 0 ||
                number > 0 && multiple < 0)
            {
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));
            }

            double mround = MRound(number, multiple);
            return new UltraCalcValue(mround);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "mround"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args

        #region MRound
        internal static double MRound(double number, double multiple)
        {
            double d = number / multiple;

            if (double.IsInfinity(d) ||
                double.IsNaN(d))
            {
                return 0;
            }

            d += .5;
            d = Math.Floor(d);

            return d * multiple;
        }
        #endregion //MRound



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionMRound

    #region UltraCalcFunctionRandBetween
    /// <summary>
    /// Generates a pseudorandom integer between two specified numbers. 
    /// </summary>
    /// <remarks>
    /// <p class="body">RANDBETWEEN(bottom, top)
    /// <br/>
    /// Bottom is the minumum value that will be returned.
    /// <br/>
    /// Top is the maximum value that will be returned.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionRandBetween : BuiltInFunctionBase
    {
        #region Member Variables

        // AS 9/8/04 UWC54
        private Random random = new Random();

        #endregion //Member Variables

        #region Properties


		#region IsAlwaysDirty
		/// <summary>
		/// Indicates whether the results of the function is always dirty.
		/// </summary>
		public override bool IsAlwaysDirty
		{
			get { return true; }
		}
		#endregion //IsAlwaysDirty 


        #endregion //Properties

        /// <summary>
        /// Evaluates the function against the arguments on the number stack
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            int max = arg.ToInt();

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            int min = arg.ToInt();

            if (max < min)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            return new UltraCalcValue(random.Next(min, max + 1));
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula
        /// </summary>
        public override string Name
        {
            get { return "randbetween"; }
        }
        #endregion //Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion //Min/Max args
    }
    #endregion //UltraCalcFunctionRandBetween

    #region UltraCalcFunctionRoman
    /// <summary>
    /// Converts a number into a roman number as a string.
    /// </summary>
    /// <remarks>
    /// <p class="body">ROMAN(number, form)
    /// <br/>
    /// Number is the number to convert to roman numerals.
    /// <br/>
    /// Form is a number or boolean value indicating whether to use classic roman numerals or a more concise version. 
    /// <br/>
    /// 0  = (Default) Classic. (499 = "CDXCIX")
    /// 1  = More concise. (499 = "LDVLIV")
    /// 2  = More concise. (499 = "XDIX")
    /// 3  = More concise. (499 = "VDIV")
    /// 4  = Simplified. (499 = "ID")
    /// TRUE  = Classic. 
    /// FALSE = Simplified. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionRoman : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            int form;
            UltraCalcValue arg;

            if (argumentCount == 2)
            {
                arg = numberStack.Pop();
                if (arg.IsError)
                    return new UltraCalcValue(arg.ToErrorValue());

                object formValue = arg.GetResolvedValue();
                if (formValue is bool)
                    form = (bool)formValue ? 0 : 4;
                else
                    form = arg.ToInt();
            }
            else
                form = 0;

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            int number = arg.ToInt();

            if (number < 0 ||
                number > 3999)
            {
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
            }

            RomanNumeral romanNumeral = new RomanNumeral(number);

            return new UltraCalcValue(romanNumeral.ToString(form));
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "roman"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args

        #region RomanNumber Class
        internal class RomanNumeral
        {
            #region Private Members
            private int number;
            private static Dictionary<int, string> classicValues;
            private static Dictionary<int, string> conciseValues1;
            private static Dictionary<int, string> conciseValues2;
            private static Dictionary<int, string> conciseValues3;
            private static Dictionary<int, string> simplifiedValues;

            #endregion //Private Members

            #region Constructor
            public RomanNumeral(int number)
            {
                if (number < 0 ||
                    number > 3999)
                {
                    throw new ArgumentException("Number must be between 1 and 3999", "number");
                }

                this.number = number;
            }
            #endregion //Constructor

            #region Number
            public int Number
            {
                get { return this.number; }
            }
            #endregion //Number

            #region ClassicValues
            private static Dictionary<int, string> ClassicValues
            {
                get
                {
                    if (classicValues == null)
                    {
                        classicValues = new Dictionary<int, string>(13);
                        classicValues.Add(1000, "M");
                        classicValues.Add(900, "CM");
                        classicValues.Add(500, "D");
                        classicValues.Add(400, "CD");
                        classicValues.Add(100, "C");
                        classicValues.Add(90, "XC");
                        classicValues.Add(50, "L");
                        classicValues.Add(40, "XL");
                        classicValues.Add(10, "X");
                        classicValues.Add(9, "IX");
                        classicValues.Add(5, "V");
                        classicValues.Add(4, "IV");
                        classicValues.Add(1, "I");
                    }

                    return classicValues;
                }
            }
            #endregion //ClassicValues

            #region ConciseValues1
            private static Dictionary<int, string> ConciseValues1
            {
                get
                {
                    if (conciseValues1 == null)
                    {
                        conciseValues1 = new Dictionary<int, string>(17);
                        conciseValues1.Add(1000, "M");
                        conciseValues1.Add(950, "LM");
                        conciseValues1.Add(900, "CM");
                        conciseValues1.Add(500, "D");
                        conciseValues1.Add(450, "LD");
                        conciseValues1.Add(400, "CD");
                        conciseValues1.Add(100, "C");
                        conciseValues1.Add(95, "VC");
                        conciseValues1.Add(90, "XC");
                        conciseValues1.Add(50, "L");
                        conciseValues1.Add(45, "VL");
                        conciseValues1.Add(40, "XL");
                        conciseValues1.Add(10, "X");
                        conciseValues1.Add(9, "IX");
                        conciseValues1.Add(5, "V");
                        conciseValues1.Add(4, "IV");
                        conciseValues1.Add(1, "I");
                    }

                    return conciseValues1;
                }
            }
            #endregion //ConciseValues1

            #region ConciseValues2
            private static Dictionary<int, string> ConciseValues2
            {
                get
                {
                    if (conciseValues2 == null)
                    {
                        conciseValues2 = new Dictionary<int, string>(21);
                        conciseValues2.Add(1000, "M");
                        conciseValues2.Add(990, "XM");
                        conciseValues2.Add(950, "LM");
                        conciseValues2.Add(900, "CM");
                        conciseValues2.Add(500, "D");
                        conciseValues2.Add(490, "XD");
                        conciseValues2.Add(450, "LD");
                        conciseValues2.Add(400, "CD");
                        conciseValues2.Add(100, "C");
                        conciseValues2.Add(99, "IC");
                        conciseValues2.Add(95, "VC");
                        conciseValues2.Add(90, "XC");
                        conciseValues2.Add(50, "L");
                        conciseValues2.Add(49, "IL");
                        conciseValues2.Add(45, "VL");
                        conciseValues2.Add(40, "XL");
                        conciseValues2.Add(10, "X");
                        conciseValues2.Add(9, "IX");
                        conciseValues2.Add(5, "V");
                        conciseValues2.Add(4, "IV");
                        conciseValues2.Add(1, "I");
                    }

                    return conciseValues2;
                }
            }
            #endregion //ConciseValues2

            #region ConciseValues3
            private static Dictionary<int, string> ConciseValues3
            {
                get
                {
                    if (conciseValues3 == null)
                    {
                        conciseValues3 = new Dictionary<int, string>(23);
                        conciseValues3.Add(1000, "M");
                        conciseValues3.Add(995, "VM");
                        conciseValues3.Add(990, "XM");
                        conciseValues3.Add(950, "LM");
                        conciseValues3.Add(900, "CM");
                        conciseValues3.Add(500, "D");
                        conciseValues3.Add(495, "VD");
                        conciseValues3.Add(490, "XD");
                        conciseValues3.Add(450, "LD");
                        conciseValues3.Add(400, "CD");
                        conciseValues3.Add(100, "C");
                        conciseValues3.Add(99, "IC");
                        conciseValues3.Add(95, "VC");
                        conciseValues3.Add(90, "XC");
                        conciseValues3.Add(50, "L");
                        conciseValues3.Add(49, "IL");
                        conciseValues3.Add(45, "VL");
                        conciseValues3.Add(40, "XL");
                        conciseValues3.Add(10, "X");
                        conciseValues3.Add(9, "IX");
                        conciseValues3.Add(5, "V");
                        conciseValues3.Add(4, "IV");
                        conciseValues3.Add(1, "I");
                    }

                    return conciseValues3;
                }
            }
            #endregion //ConciseValues3

            #region SimplifiedValues
            private static Dictionary<int, string> SimplifiedValues
            {
                get
                {
                    if (simplifiedValues == null)
                    {
                        simplifiedValues = new Dictionary<int, string>(25);
                        simplifiedValues.Add(1000, "M");
                        simplifiedValues.Add(999, "IM");
                        simplifiedValues.Add(995, "VM");
                        simplifiedValues.Add(990, "XM");
                        simplifiedValues.Add(950, "LM");
                        simplifiedValues.Add(900, "CM");
                        simplifiedValues.Add(500, "D");
                        simplifiedValues.Add(499, "ID");
                        simplifiedValues.Add(495, "VD");
                        simplifiedValues.Add(490, "XD");
                        simplifiedValues.Add(450, "LD");
                        simplifiedValues.Add(400, "CD");
                        simplifiedValues.Add(100, "C");
                        simplifiedValues.Add(99, "IC");
                        simplifiedValues.Add(95, "VC");
                        simplifiedValues.Add(90, "XC");
                        simplifiedValues.Add(50, "L");
                        simplifiedValues.Add(49, "IL");
                        simplifiedValues.Add(45, "VL");
                        simplifiedValues.Add(40, "XL");
                        simplifiedValues.Add(10, "X");
                        simplifiedValues.Add(9, "IX");
                        simplifiedValues.Add(5, "V");
                        simplifiedValues.Add(4, "IV");
                        simplifiedValues.Add(1, "I");
                    }

                    return simplifiedValues;
                }
            }
            #endregion //SimplifiedValues

            #region ToString
            public override string ToString()
            {
                return this.ToString(0);
            }

            public string ToString(bool form)
            {
                if (form)
                    return this.ToString(0);
                else
                    return this.ToString(4);
            }

            public string ToString(int form)
            {
                if (this.number == 0)
                    return string.Empty;

                return GetRomanNumeral(this.number, form);
            }
            #endregion //ToString

            #region GetRomanNumeral
            internal static string GetRomanNumeral(int number, int form)
            {
                Dictionary<int, string> values;
                switch (form)
                {
                    case 0: // Classic
                        values = ClassicValues;
                        break;
                    case 1: // Concise1
                        values = ConciseValues1;
                        break;
                    case 2: // Concise2
                        values = ConciseValues2;
                        break;
                    case 3: // Concise3
                        values = ConciseValues3;
                        break;
                    case 4: // Simplified
                        values = SimplifiedValues;
                        break;
                    default:
                        throw new ArgumentException("form");
                }

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                string rn = string.Empty;

                foreach (int value in values.Keys)
                {
                    while (number >= value)
                    {
                        number -= value;
                        SB.Append(values[value]);
                    }
                }

                return SB.ToString();
            }
            #endregion //GetRomanNumeral
        }
        #endregion //RomanNumber Class



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionRoman


    #region UltraCalcFunctionRoundDown
    /// <summary>
    /// Rounds a number to down to the specified number of digits. 
    /// </summary>
    /// <remarks>
    /// <p class="body">ROUNDDOWN(number, digits)
    /// <br/>
    /// Number is a number to be rounded down.
    /// <br/>
    /// Digits indicates the number of decimal places to round to. Positive numbers indicates places after the decimal point, negative numbers indicate places before the decimal point.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionRoundDown : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            int digits = arg.ToInt();

            UltraCalcValue arg2 = numberStack.Pop();
            if (arg2.IsError)
                return new UltraCalcValue(arg2.ToErrorValue());

            double number = arg2.ToDouble();

            // MRS 11/8/2011 - TFS94313 
            // This has to be done after the multiplication
            //            
            //// MD 6/7/11 - TFS78166
            //// Excel tries to correct some round off errors in the display values of cells, but they will also fix them in the rounding functions, since
            //// they could inadvertantly truncate a value.
            //// SSP 8/22/11 - XamCalculationManager
            //// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
            //// 
            ////if (UltraCalcValue.UseExcelFormulaCompatibility)
            //if ( UltraCalcValue.UseExcelFunctionCompatibility )
            //    number = CalcManagerUtilities.RoundToExcelDisplayValue(number);

            int power = digits;
            double multiplier = Math.Pow(10, power);

            number *= multiplier;

            // MRS 11/8/2011 - TFS94313
            // I did not check here for UseExcelFormulaCompatibility becuase this function
            // was already essentially broken (there were cases when it was clearly wrong, so 
            // there's no problem with breaking compatibililty. 
			number = MathUtilities.RoundToExcelDisplayValue(number);

			number = MathUtilities.Truncate(number);

            number /= multiplier;

            // MRS 11/8/2011 - TFS94313
            // We have to do this again in case the division causes a rounding error. 
			number = MathUtilities.RoundToExcelDisplayValue(number);

            if (number == -0)
                number = 0;

            return new UltraCalcValue(number);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "rounddown"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionRoundDown

    #region UltraCalcFunctionRoundUp
    /// <summary>
    /// Rounds a number to up to the specified number of digits.
    /// </summary>
    /// <remarks>
    /// <p class="body">ROUNDUP(number, digits)
    /// <br/>
    /// Number is a number to be rounded up.
    /// <br/>
    /// Digits indicates the number of decimal places to round to. Positive numbers indicates places after the decimal point, negative numbers indicate places before the decimal point.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionRoundUp : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            int digits = arg.ToInt();

            UltraCalcValue arg2 = numberStack.Pop();
            if (arg2.IsError)
                return new UltraCalcValue(arg2.ToErrorValue());

            double number = arg2.ToDouble();

            // MRS 11/8/2011 - TFS94313 
            // We need to do this after the multiplication.
            //            
            //// MD 6/7/11 - TFS78166
            //// Excel tries to correct some round off errors in the display values of cells, but they will also fix them in the rounding functions, since
            //// they could inadvertantly truncate a value.
            //// SSP 8/22/11 - XamCalculationManager
            //// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
            //// 
            ////if (UltraCalcValue.UseExcelFormulaCompatibility)
            //if ( UltraCalcValue.UseExcelFunctionCompatibility )
            //    number = CalcManagerUtilities.RoundToExcelDisplayValue(number);

            int sign = number >= 0 ? 1 : -1;
            number = Math.Abs(number);

            double multiplier = Math.Pow(10, digits);

            number *= multiplier;

            // MRS 11/8/2011 - TFS94313
            // I did not check here for UseExcelFormulaCompatibility becuase this function
            // was already essentially broken (there were cases when it was clearly wrong, so 
            // there's no problem with breaking compatibililty. 
			number = MathUtilities.RoundToExcelDisplayValue(number);

            number = Math.Ceiling(number);

            number /= multiplier;

            number *= sign;

            // MRS 11/8/2011 - TFS94313
            // We have to do this again in case the division causes a rounding error. 
			number = MathUtilities.RoundToExcelDisplayValue(number);

            if (number == -0)
                number = 0;

            return new UltraCalcValue(number);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "roundup"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 2; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 2; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionRoundUp


    #region UltraCalcFunctionSeriesSum
    /// <summary>
    /// Returns the sum of a power series.
    /// </summary>
    /// <remarks>
    /// <p class="body">SERIESSUM(inputValue, initialPower, step, coefficient1 [, coefficient2, coefficient3, ..., coefficientN])
    /// <br/>
    /// InputValue is the input value to the power series. 
    /// <br/>
    /// InitialPower is the initial power to which X will be raised.
    /// <br/>
    /// Step is the step which will be used to increase N for each term in the series. 
    /// <br/>
    /// Coefficient1, Coefficient2, ..., CoefficientN is a set of coefficients by which each successive power of X is multiplied.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionSeriesSum : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue[] coefficientArgs = GetArguments(numberStack, argumentCount - 3, true);
            Array.Reverse(coefficientArgs);

            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            int step = arg.ToInt();

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            int initialPower = arg.ToInt();

            arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double inputValue = arg.ToDouble();
            int power = initialPower;
            double result = 0;

            foreach (UltraCalcValue coefficientArg in coefficientArgs)
            {
                if (coefficientArg.IsError)
                    return new UltraCalcValue(coefficientArg.ToErrorValue());

                double coefficient = coefficientArg.ToDouble();

                double term = coefficient * Math.Pow(inputValue, power);

                result += term;

                power += step;
            }

            return new UltraCalcValue(result);
        }

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			// Only the coefficient parameter can be enumerable
			return parameterIndex == 3;
		}

		#endregion  // CanParameterBeEnumerable

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "seriessum"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 4; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 4; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionSeriesSum


    #region UltraCalcFunctionSign
    /// <summary>
    /// Returns the sign of a number. (-1, 0, or 1)
    /// </summary>
    /// <remarks>
    /// <p class="body">SIGN(number)
    /// <br/>
    /// The number whose sign wil be returned. 
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionSign : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double number = arg.ToDouble();

            double sign;
            if (number < 0)
                sign = -1;
            else if (number > 0)
                sign = 1;
            else
                sign = 0;

            return new UltraCalcValue(sign);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "sign"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionSign

    #region UltraCalcFunctionSqrtPi
    /// <summary>
    /// Returns the square root of the specified number times Pi.
    /// </summary>
    /// <remarks>
    /// <p class="body">SQRTPI(number)
    /// <br/>
    /// The number which will be multiplied by Pi.
    /// </p>
    /// </remarks>

	internal





        class UltraCalcFunctionSqrtPi : BuiltInFunctionBase
    {
        /// <summary>
        /// Evaluates the function against the arguments on the number stack.
        /// </summary>
        /// <param name="numberStack">Formula number stack containing function arguments.</param>
        /// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
        /// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
        protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
        {
            UltraCalcValue arg = numberStack.Pop();
            if (arg.IsError)
                return new UltraCalcValue(arg.ToErrorValue());

            double number = arg.ToDouble();

            if (number < 0)
                return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Num));

            double sqrtpi = Math.Sqrt(Math.PI * number);

            return new UltraCalcValue(sqrtpi);
        }

        #region Name
        /// <summary>
        /// Function name used to reference the function in a formula.
        /// </summary>
        public override string Name
        {
            get { return "sqrtpi"; }
        }
        #endregion // Name

        #region Min/Max args
        /// <summary>
        /// Minimum number of arguments required for the function.
        /// </summary>
        public override int MinArgs
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximum number of arguments required for the function.
        /// </summary>
        public override int MaxArgs
        {
            get { return 1; }
        }
        #endregion // Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

    }
    #endregion //UltraCalcFunctionSqrtPi

    #endregion //MRS NAS 8.3 - New Functions


	// MD 7/14/08 - Excel formula solving
	#region UltraCalcFunctionChoose

	/// <summary>
	/// The CHOOSE function returns one of the values provided in its arguments based on the number in the first argument.
	/// </summary>
	/// <remarks>
	/// <p class="body">CHOOSE(index_num, value1, [value2, ..., valueN])
	/// </p>
	/// </remarks>




	// MD 9/19/08 - TFS7178
	// The numbering of the value argument names should be 1-based to more closely reflect what will be choosen based on the first argument.
	[OneBasedArgumentNumbering( 1 )]

	internal




		class UltraCalcFunctionChoose : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack.
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments.</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate( UltraCalcNumberStack numberStack, int argumentCount )
		{
			List<UltraCalcValue> chooseValues = new List<UltraCalcValue>();

			for ( int i = 0; i < argumentCount - 1; i++ )
				chooseValues.Insert( 0, numberStack.Pop() );

			UltraCalcValue indexNumArg = numberStack.Pop();

			if ( indexNumArg.IsError )
				return new UltraCalcValue( indexNumArg.ToErrorValue() );

			double indexNumRaw;

			if ( indexNumArg.ToDouble( out indexNumRaw ) == false )
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );

			int indexNum = (int)Math.Floor( indexNumRaw );

			if ( indexNum < 1 || chooseValues.Count < indexNum )
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );

			return chooseValues[ indexNum - 1 ];
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula.
		/// </summary>
		public override string Name
		{
			get { return "choose"; }
		}
		#endregion // Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function.
		/// </summary>
		public override int MinArgs
		{
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function.
		/// </summary>
		public override int MaxArgs
		{
			get { return int.MaxValue; }
		}
		#endregion // Min/Max args



#region Infragistics Source Cleanup (Region)



































#endregion // Infragistics Source Cleanup (Region)

	}

	#endregion UltraCalcFunctionChoose

	// MD 3/30/10 - TFS30100
	#region UltraCalcFunctionRept
	/// <summary>
	/// Repeats text a specific number of times.
	/// </summary>
	/// <remarks>
	/// <p class="body">REPT(text_value, repeat_count)</p>
	/// <p class="body">Text_value is the text which should be repeated.</p>
	/// <p class="body">Repeat_count is the positive number of times <em>text_value</em> should be repeated.</p>
	/// <p class="body">When <em>repeat_count</em> is negative, and error value is returned. When <em>repeat_count</em> 
	/// is 0, an empty string is returned. When <em>repeat_count</em> is not an integer, it will be truncated.</p>
	/// <p class="body">If the length of the returned string would be greater than 32,767, an error value is returned.</p>
	/// <code>REPT("-", 10)</code>
	/// </remarks>

	internal





 class UltraCalcFunctionRept : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue repeatCountArg = numberStack.Pop();
			UltraCalcValue textArg = numberStack.Pop();

			if (textArg.IsError)
				return new UltraCalcValue(textArg.ToErrorValue());

			if (repeatCountArg.IsError)
				return new UltraCalcValue(repeatCountArg.ToErrorValue());

			int repeatCount = repeatCountArg.ToInt32();

			if (repeatCount < 0)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			string text = textArg.ToString();

			if (repeatCount == 0 || String.IsNullOrEmpty(text))
				return new UltraCalcValue(string.Empty);

			int finalLength = text.Length * repeatCount;

			if (Int16.MaxValue < finalLength)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			System.Text.StringBuilder sb = new System.Text.StringBuilder(finalLength);

			for (int i = 0; i < repeatCount; i++)
				sb.Append(text);

			return new UltraCalcValue(sb.ToString());
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{
			get { return "rept"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{
			get { return 2; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionRept

	// MD 2/14/11 - TFS66313
	#region UltraCalcFunctionSubtotal
	/// <summary>
	/// Calculates the subtotal in one of more references.
	/// </summary>
	/// <remarks>
	/// <p class="body">Subtotal(function_num, ref1, ref2,...)</p>
	/// <p class="body">Function_num is the type which should be calculated.
	/// <table>
	/// <colgroup><col width="180px"/><col width="180px"/><col width="80px"/></colgroup>
	/// <tr><th>Function_num (includes hidden values)</th><th>Function_num (ignores hidden values)</th><th>Function</th></tr>
	/// <tr><td>1</td><td>101</td><td>AVERAGE</td></tr>
	/// <tr><td>2</td><td>102</td><td>COUNT</td></tr>
	/// <tr><td>4</td><td>104</td><td>MAX</td></tr>
	/// <tr><td>5</td><td>105</td><td>MIN</td></tr>
	/// <tr><td>6</td><td>106</td><td>PRODUCT</td></tr>
	/// <tr><td>7</td><td>107</td><td>STDEV</td></tr>
	/// <tr><td>9</td><td>109</td><td>SUM</td></tr>
	/// <tr><td>10</td><td>110</td><td>VAR</td></tr>
	/// </table>
	/// </p>
	/// <p class="body">Ref1, ref2, ... are references for which you want to find the subtotal.</p>
	/// </remarks>

	internal





 class UltraCalcFunctionSubtotal : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue[] rawValues = new UltraCalcValue[argumentCount - 1];
			for (int i = rawValues.Length - 1; i >= 0; i--)
			{
				UltraCalcValue value = numberStack.Pop();

				if (value.IsError)
					return value;

				rawValues[i] = value;
			}

			UltraCalcValue functionNumValue = numberStack.Pop();
			if (functionNumValue.IsError)
				return functionNumValue;

			double functionNumDouble;
			if (functionNumValue.ToDouble(out functionNumDouble) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			int functionNum = (int)functionNumDouble;



			if (functionNum > 100)
			{



				functionNum -= 100;
			}

			List<UltraCalcValue> args = new List<UltraCalcValue>();
			for (int i = 0; i < rawValues.Length; i++)
			{
				// MD 12/2/11 - TFS97046
				// This was incorrect. We were specifying ignoreHiddenValues as the skipEmptyValues parameter, but nulls are always counted
				// for these functions. Instead, I added a new parameter in Excel which ignores hidden cells and we will pass in ignoreHiddenValues
				// for that new parameter.
				//this.GetArguments(args, rawValues[i], ignoreHiddenValues);
				this.GetArguments(args, rawValues[i], false



					);
			}

			switch (functionNum)
			{
				case 1:		// AVERAGE
					return UltraCalcFunctionAverage.EvaluateHelper(args);

				case 2:		// COUNT
					return UltraCalcFunctionCount.EvaluateHelper(args);

				case 3:		// COUNTA
					return UltraCalcFunctionCountA.EvaluateHelper(args);

				case 4:		// MAX
					return UltraCalcFunctionMax.EvaluateHelper(args);

				case 5:		// MIN
					return UltraCalcFunctionMin.EvaluateHelper(args);

				case 6:		// PRODUCT
					return UltraCalcFunctionProduct.EvaluateHelper(args);

				case 7:		// STDEV
					return UltraCalcFunctionStdev.EvaluateHelper(args);

				
				
				

				case 9:		// SUM
					return UltraCalcFunctionSum.EvaluateHelper(args);

				case 10:	// VAR
					return UltraCalcFunctionVar.EvaluateHelper(args);

				
				
				

				default:
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
			}
		}

		// MD 9/13/11 - FormulaEditor support
		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			// Only the refN parameters can be enumerable
			return 1 <= parameterIndex;
		}

		#endregion  // CanParameterBeEnumerable

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{
			get { return "subtotal"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSubtotal

	// MD 5/24/11 - TFS75560
	#region UltraCalcFunctionText
	/// <summary>
	/// Converts the value to text based on the specified format.
	/// </summary>
	/// <remarks>
	/// <p class="body">Text(value, format_text)</p>
	/// <p class="body">value is the numeric value which should be formatted.</p>
	/// <p class="body">format_text is a the number format with which to format the value.</p>
	/// </remarks>

	internal





 class UltraCalcFunctionText : BuiltInFunctionBase
	{
		#region Evaluate

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue formatTextArg = numberStack.Pop();
			UltraCalcValue valueArg = numberStack.Pop();

			if (valueArg.IsError)
				return valueArg;

			if (formatTextArg.IsError)
				return formatTextArg;

			if (formatTextArg.IsBoolean)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			string valueString = valueArg.ToString();
			string formatText = formatTextArg.ToString();

			double value;
			bool valueIsValid = valueArg.ToDouble(out value);

			if (valueIsValid == false && valueArg.IsString)
			{
				double? timeValue = this.ParseTime(valueString);

				if (timeValue.HasValue)
				{
					value = timeValue.Value;
					valueIsValid = true;
				}
				else
				{
					DateTime valueDateTime;
					if (DateTime.TryParse(valueString, out valueDateTime))
					{
						double? excelDateTime = UltraCalcValue.DateTimeToExcelDate(



 valueDateTime);

						if (excelDateTime.HasValue)
						{
							value = excelDateTime.Value;
							valueIsValid = true;
						}
					}
				}
			}

			double? valueResolved = null;
			if (valueIsValid)
				valueResolved = value;

			ValueFormatter converter = new ValueFormatter(



 formatText,
 // MD 4/6/12 - TFS101506
 numberStack.Culture
 );

			string text;
			if (converter.TryFormatValue(valueResolved, valueString, out text) == false)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			return new UltraCalcValue(text);
		}

		#endregion // Evaluate

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{
			get { return "text"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{
			get { return 2; }
		}
		#endregion //Min/Max args


		#region ParseTime

		private double? ParseTime(string valueString)
		{
			Match match = this.TimeRegex.Match(valueString);

			if (match.Success == false)
				return null;

			int hours = 0;
			int minutes = 0;
			int seconds = 0;
			int milliseconds = 0;

			Group hoursGroup = match.Groups["Hours"];
			if (hoursGroup.Length != 0)
			{
				if (int.TryParse(hoursGroup.Value, out hours) == false)
					Utilities.DebugFail("Couldn't parse value.");
			}

			Group minutesGroup = match.Groups["Minutes"];
			if (minutesGroup.Length != 0)
			{
				if (int.TryParse(minutesGroup.Value, out minutes) == false)
					Utilities.DebugFail("Couldn't parse value.");
			}

			Group secondsGroup = match.Groups["Seconds"];
			if (secondsGroup.Length != 0)
			{
				if (int.TryParse(secondsGroup.Value, out seconds) == false)
					Utilities.DebugFail("Couldn't parse value.");
			}

			Group millisecondsGroup = match.Groups["Milliseconds"];
			if (millisecondsGroup.Length != 0)
			{
				string millisecondsGroupText = millisecondsGroup.Value;

				while (millisecondsGroupText.Length < 4)
					millisecondsGroupText += "0";

				if (int.TryParse(millisecondsGroupText, out milliseconds) == false)
					Utilities.DebugFail("Couldn't parse value.");
			}

			Group ampmSectionGroup = match.Groups["AMPM"];
			if (ampmSectionGroup.Length != 0)
			{
				if (hours >= 12)
					return null;

				if (ampmSectionGroup.Value.StartsWith("P", StringComparison.InvariantCultureIgnoreCase))
					hours += 12;
			}


			return (hours / 24.0) + (minutes / 1440.0) + (seconds / 86400.0) + (milliseconds / 864000000.0);
		}

		#endregion // ParseTime

		#region TimeRegex

		private Regex timeRegex;

		private Regex TimeRegex
		{
			get
			{
				if (this.timeRegex == null)
				{
					const string hoursGroup = @"(?'Hours'[0-9]{1,4})\s*";
					const string minutesGroup = @"\s*(?'Minutes'[0-9]?[0-9])\s*";
					const string secondsGroup = @"\s*(?'Seconds'[0-9]?[0-9])\s*";
					const string millisecondsGroup = @".\s*(?'Milliseconds'[0-9]{0,4})[0-9]*\s*";
					const string ampmGroup = @"(\s+(?'AMPM'(a|p)m?)\s*)";

					string hoursAndAMPMSection = string.Format("({0}{1})", hoursGroup, ampmGroup);
					string hoursAndMinutes = string.Format("({0}:{1}(:{2}({3})?)?{4}?)", hoursGroup, minutesGroup, secondsGroup, millisecondsGroup, ampmGroup);
					string minutesSecondsAndMilliseconds = string.Format("({0}:{1}{2}{3}?)", minutesGroup, secondsGroup, millisecondsGroup, ampmGroup);

					this.timeRegex = new Regex(string.Format("^({0}|{1}|{2})$", hoursAndAMPMSection, hoursAndMinutes, minutesSecondsAndMilliseconds),
						Utilities.RegexOptionsCompiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
				}

				return this.timeRegex;
			}
		}

		#endregion // TimeRegex



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionText

	// MD 8/29/11 - TFS85072
	#region UltraCalcFunctionIfError
	/// <summary>
	/// Returns one of two values depending on whether or not the first value is an error or not.
	/// </summary>
	/// <remarks>
	/// <p class="body">IFERROR(value, value_if_error)</p>
	/// <p class="body">value is the value to use when it is not an error.</p>
	/// <p class="body">value_if_error is the value to use if the value argument is an error.</p>
	/// </remarks>

	internal





 class UltraCalcFunctionIfError : BuiltInFunctionBase
	{
		#region Evaluate

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue valueIfErrorArg = numberStack.Pop();
			UltraCalcValue valueArg = numberStack.Pop();

			// MD 4/10/12
			// Found while fixing TFS108678
			// We need to split the value up always if it is an array, so always resolve it.
			//if (valueArg.IsError)
			//    return valueIfErrorArg;
			//
			//return valueArg;
			object testValue = valueArg.GetResolvedValue();
			if (testValue is UltraCalcErrorValue)
				return valueIfErrorArg;

			return new UltraCalcValue(testValue);
		}

		#endregion // Evaluate

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{
			get { return "iferror"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{
			get { return 2; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionIfError

	// MD 2/24/12 - 12.1 - Table Support
	#region UltraCalcFunctionCountA

	/// <summary>
	/// Counts how many cells are not blank. 
	/// </summary>
	/// <remarks>
	/// <p class="body">COUNTA(Value1, Value2, ..., ValueN)</p>
	/// <p class="body">Value1, value2, ... valueN can be references to different data structures,
	/// such as columns. Each non-blank value is counted.</p>
	/// <p class="body">
	/// When a reference is a range reference, only the non-blank values within the range will be counted.
	/// </p>
	/// </remarks>

	internal





 class UltraCalcFunctionCountA : BuiltInFunctionBase
	{
		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An <see cref="UltraCalcValue"/> that represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			return EvaluateHelper(args);
		}

		#region CanParameterBeEnumerable

		/// <summary>
		/// Determines whether the parameter at the specified index will accept an enumerable reference.
		/// </summary>
		/// <param name="parameterIndex">In 0-based index of the parameter.</param>
		/// <returns>
		/// True if the parameter at the specified index can accept enumerable references; False otherwise or if the parameter is out of range for this function.
		/// </returns>
		public override bool CanParameterBeEnumerable(int parameterIndex)
		{
			return true;
		}

		#endregion  // CanParameterBeEnumerable

		internal static UltraCalcValue EvaluateHelper(IList<UltraCalcValue> args)
		{
			int count = 0;

			foreach (UltraCalcValue arg in args)
			{
				if (arg.IsNull)
					continue;

				count++;
			}

			return new UltraCalcValue(count);
		}

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{
			get { return "counta"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args
	}
	#endregion //UltraCalcFunctionCountA

	// MD 3/2/12 - TFS103729
	#region UltraCalcFunctionSearch
	/// <summary>
	/// Returns the 1-based index of one string within another, searching case insensitively.
	/// </summary>
	/// <remarks>
	/// <p class="body">SEARCH(search_text, value, [start_index])</p>
	/// <p class="body">search_text is the text to find in value.</p>
	/// <p class="body">value is the text in which to find search_text.</p>
	/// <p class="body">start_index is the 1-based index in which to start the search (if omitted, the start_index is 1).</p>
	/// </remarks>

	internal





 class UltraCalcFunctionSearch : BuiltInFunctionBase
	{
		#region Evaluate

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An UltraCalcValue represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			UltraCalcValue startNumValue = null;
			if (argumentCount > 2)
				startNumValue = numberStack.Pop();

			UltraCalcValue withinTextValue = numberStack.Pop();
			UltraCalcValue findTextValue = numberStack.Pop();

			if (findTextValue.IsError)
				return findTextValue;

			if (withinTextValue.IsError)
				return withinTextValue;

			if (startNumValue != null && startNumValue.IsError)
				return startNumValue;

			string findText = findTextValue.ToString();
			string withinText = withinTextValue.ToString();

			int startIndex = 0;
			if (startNumValue != null)
			{
				double startIndexNumber;
				if (startNumValue.ToDouble(out startIndexNumber) == false)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

				startIndex = (int)MathUtilities.Truncate(startIndexNumber) - 1;

				if (startIndex < 0 || withinText.Length <= startIndex)
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));
			}

			// MD 4/6/12 - TFS101506
			//int index = withinText.IndexOf(findText, startIndex, StringComparison.CurrentCultureIgnoreCase);
			int index = numberStack.Culture.CompareInfo.IndexOf(withinText, findText, startIndex, CompareOptions.IgnoreCase);

			if (index < 0)
				return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Value));

			return new UltraCalcValue(index + 1);
		}

		#endregion // Evaluate

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{
			get { return "search"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			get { return 2; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{
			get { return 3; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionSearch

	// MD 3/2/12 - TFS103729
	#region UltraCalcFunctionSearchB
	/// <summary>
	/// Returns the 1-based index of one string within another, searching case insensitively.
	/// </summary>
	/// <remarks>
	/// <p class="body">SEARCHB(search_text, value, [start_index])</p>
	/// <p class="body">search_text is the text to find in value.</p>
	/// <p class="body">value is the text in which to find search_text.</p>
	/// <p class="body">start_index is the 1-based index in which to start the search (if omitted, the start_index is 1).</p>
	/// </remarks>

	internal





 class UltraCalcFunctionSearchB : UltraCalcFunctionSearch
	{
		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{
			get { return "searchb"; }
		}
		#endregion //Name
	}
	#endregion //UltraCalcFunctionSearchB

	// MD 5/10/12 - TFS106835
	#region UltraCalcFunctionAveDev
	/// <summary>
	/// Returns the average deviation from the average of all numeric values.
	/// </summary>
	/// <remarks>
	/// <p class="body">AVEDEV(number1, number2, ...)</p>
	/// <p class="body">Number1, number2, ... are one or more numeric values or references to numeric values.</p>
	/// </remarks>

	internal





 class UltraCalcFunctionAveDev : BuiltInFunctionBase
	{
		#region Evaluate

		/// <summary>
		/// Evaluates the function against the arguments on the number stack
		/// </summary>
		/// <param name="numberStack">Formula number stack containing function arguments</param>
		/// <param name="argumentCount">Denotes the number of function arguments pushed onto the number stack.</param>
		/// <returns>An UltraCalcValue represents the result of the function evaluation.</returns>
		protected override UltraCalcValue Evaluate(UltraCalcNumberStack numberStack, int argumentCount)
		{
			double totalValue = 0;

			UltraCalcValue[] args = this.GetArguments(numberStack, argumentCount, true);
			List<double> values = new List<double>(args.Length);

			foreach (UltraCalcValue arg in args)
			{
				if (arg == null)
					continue;

				if (arg.IsError)
					return new UltraCalcValue(arg.ToErrorValue());

				double value;
				if (arg.ToDouble(out value))
				{
					totalValue += value;
					values.Add(value);
				}
			}

			double average = totalValue / values.Count;

			double totalDeviation = 0;
			for (int i = 0; i < values.Count; i++)
				totalDeviation += Math.Abs(values[i] - average);

			double averageDeviation = totalDeviation / values.Count;
			return new UltraCalcValue(averageDeviation);
		}

		#endregion // Evaluate

		#region Name
		/// <summary>
		/// Function name used to reference the function in a formula
		/// </summary>
		public override string Name
		{
			get { return "avedev"; }
		}
		#endregion //Name

		#region Min/Max args
		/// <summary>
		/// Minimum number of arguments required for the function
		/// </summary>
		public override int MinArgs
		{
			get { return 1; }
		}

		/// <summary>
		/// Maximum number of arguments required for the function
		/// </summary>
		public override int MaxArgs
		{
			get { return int.MaxValue; }
		}
		#endregion //Min/Max args



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcFunctionAveDev



#region Infragistics Source Cleanup (Region)


























































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

	// MD 7/14/08 - Excel formula solving *** End ***
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