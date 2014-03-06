using System;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

using UltraCalcFunction = Infragistics.Documents.Excel.CalcEngine.ExcelCalcFunction;
using UltraCalcNumberStack = Infragistics.Documents.Excel.CalcEngine.ExcelCalcNumberStack;
using UltraCalcValue = Infragistics.Documents.Excel.CalcEngine.ExcelCalcValue;
using IUltraCalcReference = Infragistics.Documents.Excel.CalcEngine.IExcelCalcReference;
using IUltraCalcFormula = Infragistics.Documents.Excel.CalcEngine.IExcelCalcFormula;
using UltraCalcErrorValue = Infragistics.Documents.Excel.CalcEngine.ExcelCalcErrorValue;
using IUltraCalcReferenceCollection = Infragistics.Documents.Excel.CalcEngine.IExcelCalcReferenceCollection;
using UltraCalcErrorCode = Infragistics.Documents.Excel.CalcEngine.ExcelCalcErrorCode;







using Infragistics.Shared;





namespace Infragistics.Documents.Excel.CalcEngine





{


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


	#region UltraCalcErrorCode
	/// <summary>
	/// Enumeration of error codes assigned to <see cref="UltraCalcErrorValue"/>.
	/// </summary>






	public

		 enum ExcelCalcErrorCode 



	{
		/// <summary>
		/// Occurs when an invalid or disconnected reference is encountered while evaluating a formula
		/// </summary>
		Reference,
		/// <summary>
		/// Occurs when the wrong type of argument or operand is used in a formula
		/// </summary>
		Value,
		/// <summary>
		/// Occurs when a number is divided by zero (0)
		/// </summary>
		Div,
		/// <summary>
		/// Occurs when @NA is entered into a formula
		/// </summary>
		NA,
		/// <summary>
		/// Occurs with invalid numeric values in a formula or function
		/// </summary>
		Num,

		// MD 8/20/08 - Excel formula solving
		/// <summary>
		/// Occurs when a circularity formula is used when circularities are not allowed.
		/// </summary>
		Circularity,


		// MD 7/14/08 - Excel formula solving
		// These values are needed so we can map all error values in Excel to error codes in calc manager.

		/// <summary>
		/// Occurs when there is an intersection of two references that do not contain any common cells.
		/// </summary>
		Null,

		/// <summary>
		/// Occurs when text in a formula is not recognized.
		/// </summary>
		Name,

	}
	#endregion //UltraCalcErrorCode

	#region UltraCalcErrorException
	/// <summary>
	/// Exception containing an <see cref="UltraCalcErrorValue"/>.
	/// </summary>



    internal class UltraCalcErrorException : UltraCalcException



	{
		#region Member Variables

		private UltraCalcErrorValue value;

		#endregion //Member Variables

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="UltraCalcErrorException"/> using the specified <see cref="UltraCalcErrorValue"/>
		/// </summary>
        /// <param name="errorValue">The UltraCalcErrorValue that has generated the exception.</param>



		public UltraCalcErrorException(UltraCalcErrorValue errorValue)

		{
			this.value = errorValue;
		}
		#endregion //Constructors

		#region Value
		/// <summary>
		/// Get the <b>UltraCalcErrorValue</b> associated with the exception
		/// </summary>
		public UltraCalcErrorValue Value
		{
			get { return this.value; }
		}
		#endregion //Value

		#region ToString
		/// <summary>
		/// Returns the string representation of the underlying error value.
		/// </summary>
        /// <returns>The string representation of the underlying error value.</returns>
        public override string ToString()
		{
			return this.value.ToString();
		}
		#endregion //ToString
	}
	#endregion //UltraCalcErrorException

	#region UltraCalcException
	/// <summary>
	/// Generic Calc Exception.  All Calc Exceptions derive from this so that a developer can
	/// easily turn off our exceptions during debugging.
	/// </summary>



    internal class UltraCalcException : Exception



	{
		// MD 8/3/11 - XamFormulaEditor
		/// <summary>
		/// For Infragistics internal use only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly object LocationPlaceholder = new object();

		#region Member Variables

		// MD 8/3/11 - XamFormulaEditor
		private int column;
		private int line;
		private object[] messageParams;
		private string messageRaw;

		#endregion  // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UltraCalcException"/>
		/// </summary>



		public UltraCalcException()

		{
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcException"/> with the specified error message
		/// </summary>
		/// <param name="message">Error message</param>



		public UltraCalcException( string message ) : base(message)

		{
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcException"/> with the specified error message and exception instance.
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">The exception that caused this exception</param>



		public UltraCalcException( string message,Exception innerException ) : base(message,innerException)

		{
		}


		// MD 8/3/11 - XamFormulaEditor
		/// <summary>
		/// For Infragistics internal use only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]



		public UltraCalcException(

				string messageRaw, int line, int column, object[] messageParams)
			: base(ConstructMessage(messageRaw, line, column, messageParams))
		{
			this.messageRaw = messageRaw;
			this.line = line;
			this.column = column;
			this.messageParams = messageParams;
		}

		#endregion //Constructor

		// MD 8/3/11 - XamFormulaEditor
		#region Methods

		private static string ConstructMessage(string messageRaw, int line, int column, object[] messageParams)
		{
			object[] actualParams = new object[messageParams.Length];
			for (int i = 0; i < messageParams.Length; i++)
			{
				object currentParam = messageParams[i];

				if (currentParam == LocationPlaceholder)
					actualParams[i] = SR.GetString("Error_Location", line, column);
				else
					actualParams[i] = currentParam;
			}

			return string.Format(messageRaw, actualParams);
		}

		#endregion  // Methods

		// MD 8/3/11 - XamFormulaEditor
		#region Properties

		internal int Column
		{
			get { return this.column; }
		}

		internal string ErrorLocationRaw
		{
			get { return SR.GetString("Error_Location"); }
		}

		internal bool HasDynamicMessage
		{
			get { return this.messageRaw != null; }
		}

		internal int Line
		{
			get { return this.line; }
		}

		internal object[] MessageParams
		{
			get { return this.messageParams; }
		}

		internal string MessageRaw
		{
			get { return this.messageRaw; }
		}

		#endregion  // Properties
	}
	#endregion //UltraCalcException



#region Infragistics Source Cleanup (Region)


























































#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)


























































#endregion // Infragistics Source Cleanup (Region)


	#region UltraCalcErrorValue

	/// <summary>
	/// Provides methods and properties used to define and manage a calculation error value.
	/// </summary>






	public

		 class ExcelCalcErrorValue



	{
		#region Member Variables

		private UltraCalcErrorCode	errorCode;		// Storage for error code
		private string				errorMessage;	// Storage for error message
		private object				errorValue;		// Storage for error object

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UltraCalcErrorValue"/> with the specified error code.
		/// </summary>
		/// <param name="code"><see cref="UltraCalcErrorCode"/> value to assign this instance</param>



        public ExcelCalcErrorValue(UltraCalcErrorCode code)



		{
			this.errorCode = code;
			if (code == UltraCalcErrorCode.Div)
				this.errorMessage = SR.GetString( "Error_UCErrorCode_Div" );
			else if (code == UltraCalcErrorCode.NA)
				this.errorMessage = SR.GetString( "Error_UCErrorCode_NA" );
			else if (code == UltraCalcErrorCode.Num)
				this.errorMessage = SR.GetString( "Error_UCErrorCode_Num" );
			else if (code == UltraCalcErrorCode.Reference)
				this.errorMessage = SR.GetString( "Error_UCErrorCode_Reference" );
			else if (code == UltraCalcErrorCode.Value)
				this.errorMessage = SR.GetString( "Error_UCErrorCode_Value" );


			// MD 7/14/08 - Excel formula solving
			// Added messages for new error codes
			else if ( code == UltraCalcErrorCode.Null )
				this.errorMessage = SR.GetString( "Error_UCErrorCode_Null" );
			else if ( code == UltraCalcErrorCode.Name )
				this.errorMessage = SR.GetString( "Error_UCErrorCode_Name" );


			else
				this.errorMessage = SR.GetString( "Error_UCErrorCode_Unknown" );
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcErrorValue"/> with the specified error code and error message
		/// </summary>
		/// <param name="code"><see cref="UltraCalcErrorCode"/> value</param>
		/// <param name="message">Localized Message indicating reason for error</param>



    public ExcelCalcErrorValue(UltraCalcErrorCode code, string message )



		{
			this.errorCode = code;
			this.errorMessage = message;
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcErrorValue"/> with the specified error code, message and error value.
		/// </summary>
		/// <param name="code"><see cref="UltraCalcErrorCode"/> value</param>
		/// <param name="message">Localized Message indicating reason for error</param>
		/// <param name="value">Value associated with error</param>



    public ExcelCalcErrorValue(UltraCalcErrorCode code, string message, object value )



    {
			this.errorCode = code;
			this.errorMessage = message;
			this.errorValue = value;
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Gets the error code for this class instance
		/// </summary>
		public UltraCalcErrorCode Code 
		{ 
			get { return errorCode; } 
		}

		/// <summary>
		/// Gets error message for this class instance. Note that when setting this property the 
		/// message is assumed to be localized.
		/// </summary>
		public string Message 
		{ 
			get { return errorMessage; } 
		}

		/// <summary>
		/// Gets the error object for this class instance
		/// </summary>
		public object ErrorValue 
		{ 
			get { return errorValue; } 
		}
		#endregion //Properties

		#region Methods
		/// <summary>
		/// Return a string message that denotes reason for error
		/// </summary>
		/// <returns>String containing error message</returns>
		public override string ToString()
		{
			if (this.errorCode == UltraCalcErrorCode.Div)
				return SR.GetString( "Value_UCErrorCode_Div" );
			else if (this.errorCode == UltraCalcErrorCode.Num)
				return SR.GetString( "Value_UCErrorCode_Num" );
			else if (this.errorCode == UltraCalcErrorCode.NA)
				return SR.GetString( "Value_UCErrorCode_NA" );
			else if (this.errorCode == UltraCalcErrorCode.Value)
				return SR.GetString( "Value_UCErrorCode_Value" );
			else if (this.errorCode == UltraCalcErrorCode.Reference)
				return SR.GetString( "Value_UCErrorCode_Reference" );


			// MD 7/14/08 - Excel formula solving
			// Added messages for new error codes
			else if ( this.errorCode == UltraCalcErrorCode.Null )
				return SR.GetString( "Value_UCErrorCode_Null" );
			else if ( this.errorCode == UltraCalcErrorCode.Name )
				return SR.GetString( "Value_UCErrorCode_Name" );


			else
				return SR.GetString( "Value_UCErrorCode_Unknown" );
		}

		/// <summary>
		/// Raise an <see cref="UltraCalcErrorException"/> exception containing this class's error information
		/// </summary>



		internal



			void RaiseException()
		{
			if( this.errorValue is UltraCalcException )
				throw (UltraCalcException)this.errorValue;
			else
				throw new UltraCalcErrorException(this);
		}
		#endregion //Methods
	}
    #endregion // UltraCalcErrorValue

	#region IUltraCalcReference
	/// <summary>
	/// The Primary Reference Inteface.
	/// </summary>






	public

		 interface IExcelCalcReference



	{
		/// <summary>
		/// The fully qualified unique name for the referenced element.  Read Only.
		/// </summary>
		string AbsoluteName { get; }

		/// <summary>
		/// The unqualified name of this referenced element.  Read Only.
		/// </summary>
		string ElementName {get;}

		/// <summary>
		/// The <see cref="UltraCalcValue"/>, if any, associated with this Reference.  If this reference 
		/// does not contain a Value then a <see cref="UltraCalcErrorValue"/> is returned.
		/// </summary>
		UltraCalcValue Value { get; set; }

		/// <summary>
		/// The <see cref="IUltraCalcFormula"/>, if any, associated with this Reference.  If this reference 
		/// can not contain a formula then null is returned.
		/// </summary>
		IUltraCalcFormula Formula { get; }



#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)








		/// <summary>
		/// Returns a context for the Reference.
		/// </summary>
		/// <remarks><p class="body">Returns a meaningful object context for the reference. This could be an UltraGridCell, UltraGridColumn, NamedReference, Control, etc.</p></remarks>

		object Context { get; }
   
		/// <summary>
		/// Create a new reference relative to this reference.  
		/// </summary>
		/// <param name="referenceString">The reference string.</param>
		/// <returns>The new reference.</returns>
		/// <remarks>
		/// A reference string may be external or internal.  
		/// <p>A external reference has the form "//object_name/object_specific_part".  
		/// The control identified by object_name must implement <see cref="IUltraCalcReference"/> and be able to 
		/// parse object_specific_part.</p>
		/// <p>An internal reference is a reference within the same control that implements this 
		/// instance of the interface.  An internal reference can be absolute or relative.  Absolute 
		/// references must begin with the "/" character.  Such references must be created relative to
		/// the root object in the control.  Relative references are created relative to this reference.</p>
		/// <p>Note that in complex reference models, multiple reference strings may point to the same
		/// underlying object.  The reference string used to create the reference must be available to
		/// other methods such as ResolveReference, References and MarkRelativeIndicies.  Consequently, if the reference 
		/// string is relative, a proxy reference object should be returned that retains the relative
		/// reference string.
		/// </p>
		/// </remarks>
		IUltraCalcReference CreateReference(string referenceString );



#region Infragistics Source Cleanup (Region)





















































#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Return a collection of references if this Reference is enumerable.  An exception is
		/// thrown if this reference is not enumerable.
		/// </summary>
		/// <returns>A Reference collection.</returns>
		/// <remarks>
		/// The collection returned by this method must be constained to the scope specified by the 
		/// original string used to create the reference.  For example, if the original reference string
		/// was [Customers(State="MA")/Total], then the collection should be constained to the Total cells
		/// for all customers that have State = "MA".
		/// </remarks>
		IUltraCalcReferenceCollection References { get; }

		/// <summary>
		/// Returns true if this reference contains inReference
		/// </summary>
		/// <param name="inReference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		/// <remarks>
		/// This method is used by the calculation engine to determine if the passed in reference
		/// is either a child of this reference or a more fully scoped version of this reference.  
		/// Each element in this reference is compared with the corresponding element in the input 
		/// reference.  If the identifers are the same, and if this scope contains the input scope, then
		/// the input reference is contained by this reference.  If the input reference is longer than 
		/// this reference and the common element are contained, then the input reference is contained.
		/// </remarks>
		bool ContainsReference(IUltraCalcReference inReference);

		/// <summary>
		/// Returns true if inReference is a proper subset of this reference
		/// </summary>
		/// <param name="inReference">The subset candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		/// <remarks>
		/// This method is used by the calculation engine to determine if the passed in reference
		/// is fully contained by this reference.  
		/// Each element in this reference is compared with the corresponding element in the input 
		/// reference.  If the identifers are the same, and if this scope entirely contains the input scope, then
		/// the input reference is a proper subset this reference.  
		/// </remarks>
		bool IsSubsetReference(IUltraCalcReference inReference);

		/// <summary>
		/// Returns true if this reference is enumerable.
		/// </summary>
		bool IsEnumerable { get; }



#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)


		// SSP 9/7/04
		// Added NormalizedAbsoluteName property as a part of case insensitive absolute
		// name implementation.
		//
		/// <summary>
		/// Returns the normalized absolute name. Calculation engine makes use of normalized
		/// absolute names of references to compare two references and search for references.
		/// This property should return the absolute name with case insensitive parts of
		/// the absolute names converted to lower case.
		/// </summary>
		string NormalizedAbsoluteName { get; }
	}
	#endregion //IUltraCalcReference



#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


	#region IUltraCalcReferenceCollection
	/// <summary>
	/// Collection of <see cref="IUltraCalcReference"/> objects
	/// </summary>






	public

		 interface IExcelCalcReferenceCollection : IEnumerable



    {
	}
	#endregion //IUltraCalcReferenceCollection



#region Infragistics Source Cleanup (Region)



























































#endregion // Infragistics Source Cleanup (Region)


	#region UltraCalcReferenceError.cs

	/// <summary>
	/// Implementation of <see cref="IUltraCalcReference"/> interface that denotes a reference error
	/// </summary>



    internal class UltraCalcReferenceError: IUltraCalcReference



	{
		#region Member Variables




		private string message;							// Storage for error message
		private string reference;						// Storage for reference absolute name


		// MD 7/14/08 - Excel formula solving
		// The reference error will now store the error code so it can override the error code represented.
		private UltraCalcErrorCode errorCode = UltraCalcErrorCode.Reference;


		#endregion //Member Variables

		#region Constructor


#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Reference string constructor
		/// </summary>
		/// <param name="reference">Absolute name of reference</param>
		/// <param name="message">Localized Error message</param>



		public UltraCalcReferenceError( string reference,string message )

		{
			this.message = message;
			this.reference = reference;



		}


		// MD 7/14/08 - Excel formula solving
		// The reference error will now store the error code so it can override the error code represented.
		public UltraCalcReferenceError( UltraCalcErrorCode errorCode )
		{
			this.errorCode = errorCode;
		}


		#endregion //Constructor

		#region Message
		/// <summary>
		/// Get reference error message
		/// </summary>
		public string Message 
		{
			get { return this.message; }
		}
		#endregion //Message


		// MD 7/14/08 - Excel formula solving
		// The reference error will now store the error code so it can override the error code represented.
		#region ErrorCode

		internal UltraCalcErrorCode ErrorCode
		{
			get { return this.errorCode; }
		} 

		#endregion ErrorCode


		#region Implementation of IUltraCalcReference

		object IUltraCalcReference.Context { get{ return null;} }

		IUltraCalcReference IUltraCalcReference.CreateReference(string reference)
		{
			return null;
		}



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


		string IUltraCalcReference.AbsoluteName
		{
			get
			{
				return this.reference;
			}
		}

		string IUltraCalcReference.ElementName
		{
			get
			{
				return this.reference;
			}
		}

		UltraCalcValue IUltraCalcReference.Value
		{

			// MD 7/14/08 - Excel formula solving
			// The reference error will now store the error code so it can override the error code represented.
			get { return new UltraCalcValue( new UltraCalcErrorValue( this.errorCode ) ); } 



			
			set {	}
		}



#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)


		IUltraCalcFormula IUltraCalcReference.Formula
		{
			get	{ return null; 	}
		}



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


		IUltraCalcReferenceCollection IUltraCalcReference.References
		{
			get
			{
				return null;
			}
		}

		bool IUltraCalcReference.IsEnumerable
		{
			get
			{
				return false;
			}
		}







		bool IUltraCalcReference.ContainsReference(IUltraCalcReference reference)
		{
			return this == reference;
		}

		bool IUltraCalcReference.IsSubsetReference(IUltraCalcReference reference)
		{
			return false;
		}



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		// SSP 9/7/04
		// Added NormalizedAbsoluteName property as a part of case insensitive absolute
		// name implementation.
		//
		#region NormalizedAbsoluteName

		// AS 9/17/04
		// Since the NormalizedAbsoluteName should not be used for
		// an UltraCalcReferenceError, we've removed the variables
		// used for caching it.
		//
		//private string lastAbsoluteName = null;
		//private string lastNormalizedAbsoluteName = null;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		string IUltraCalcReference.NormalizedAbsoluteName
		{
			get
			{
				// AS 9/17/04
				// Since the NormalizedAbsoluteName should not be used for
				// an UltraCalcReferenceError, we've removed the variables
				// used for caching it.
				//
				Debug.Assert(false, "Attempting to access the 'NormalizedAbsoluteName' of an UltraCalcReferenceError.");
				return string.Empty;
				
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			}
		}

		#endregion // NormalizedAbsoluteName

		#endregion
	}

	#endregion // UltraCalcReferenceError.cs

	#region UltraCalcValue.cs

		/// <summary>
		/// Provides methods that manage a composite data type representing a value.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// An instance of this class may contain one of several base data types including doubles, singles, integers, booleans, strings, and error values.  
		/// <p></p>
		/// The class implements the IConvertible interface providing methods to perform conversions between the basic data types.  
		/// <p></p>
		/// Additionally the class provides methods to perform basic arithmetic operations and comparisons between <see cref="UltraCalcValue"/> objects.
		/// </p>
		/// </remarks>
		// SSP 1/7/05 BR01401
		// Implemented IComparable on the UltraCalcValue.
		//
		//public class UltraCalcValue : object, IConvertible



	// MD 7/29/08 - Excel formula solving
	// Prevent the debugger from using ToString to get the debugger display. It has bad implications when debugging in Excel
	// since the values now have a state that must be maintained which can be changed by resolving the value of a reference.
	[DebuggerDisplay( "ExcelCalcValue: {value}" )]



	public

		 class ExcelCalcValue : object, IConvertible, IComparable



		{
			#region Constants

			// MD 8/12/08 - Excel formula solving
			private const double OffsetFor1904DateSystem = 1462;

			// MD 11/23/11
			// Found while fixing TFS96468
			// We should have a singleton value to represent a null value. It wastes space to create a new calc value for each null value.
			internal static readonly UltraCalcValue Empty = new UltraCalcValue();

			#endregion Constants

			#region Member Variables

			/// <summary>
			/// Storage for the underlying data type
			/// </summary>
			private object value;

			#endregion //Member Variables

			#region Constructor
			/// <summary>
			/// Initializes a new <see cref="UltraCalcValue"/>
			/// </summary>
			/// <param name="value">Object to be represented by the <see cref="UltraCalcValue"/>.</param>



			public ExcelCalcValue(object value)



			{
				// MRS 6/7/05
				// Always store the value, unless it's an Exception
				if ( value is Exception )
					ExceptionHandler((Exception)value);
				else
				{

					// MD 12/1/11 - TFS96113
					// Instead of storing arrays for rectangular regions of values, use an ArrayProxy.
					UltraCalcValue[,] array = value as UltraCalcValue[,];
					if (array != null)
					{
						this.value = new CLRArrayProxy(array);
						return;
					}

					// MD 12/1/11 - TFS96113
					// Instead of storing an array of arrays for groups of rectangular regions of values, use an array of ArrayProxy instances.
					UltraCalcValue[][,] arrayGroup = value as UltraCalcValue[][,];
					if (array != null)
					{
						ArrayProxy[] arrayProxyGroup = new ArrayProxy[arrayGroup.Length];
						for (int i = 0; i < arrayGroup.Length; i++)
							arrayProxyGroup[i] = new CLRArrayProxy(arrayGroup[i]);

						this.value = arrayProxyGroup;
						return;
					}


					this.value = value;
				}
				
//				// if it is one of the base types we know about don't do
//				// anything with the value
//				if (value == null					||
//					value is IConvertible			||
//					value is IUltraCalcReference	||
//					value is UltraCalcErrorValue)
//				{
//					this.value = value;
//				}
//				else if (value is Exception)
//				{
//					ExceptionHandler((Exception)value);
//				}
//				else if (value is IFormattable)
//				{
//					// if its something else then coerce it to a string
//					//~ AS 5/31/05 BR04356
//					//~ Storing the value as a string isn't necessarily the correct
//					//~ thing to do either. The correct thing would be to store the
//					//~ value as is and leave it up to the function and what
//					//~ methods of ultracalcvalue it uses to determine
//					//~ how to deal with the value.
//					//~
//					//~// MRS 4/29/05 - BR03587
//					//~//((IFormattable)value).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
//					//~this.value = ((IFormattable)value).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
//				}
//				else
//				{
//					// default to using the string value
//					//~ AS 5/31/05 BR04356
//					//~ Storing the value as a string isn't necessarily the correct
//					//~ thing to do either. The correct thing would be to store the
//					//~ value as is and leave it up to the function and what
//					//~ methods of ultracalcvalue it uses to determine
//					//~ how to deal with the value.
//					//~
//					//~// MRS 4/29/05 - BR03587
//					//~//value = value.ToString();
//					//~this.value = value.ToString();
//				}
			}

			/// <summary>
			/// Default constructor
			/// </summary>



			public ExcelCalcValue() : this( (object)null )



            {
			}
			#endregion //Constructor

			#region ToInt
			/// <summary>
			/// Convert this class instance's value to an int
			/// </summary>
            /// <returns>A signed 32-bit integer value containing the equivalent value of this instance</returns>
            public int ToInt()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToInt(CultureInfo.InvariantCulture);
				return this.ToInt(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to an int
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A signed 32-bit integer value containing the equivalent value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion to an integer this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public int ToInt(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDecimal method can convert all types to all 32-bit integer values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				return (int)this.ToDecimal( provider );
			}
			#endregion //ToInt

			#region ExceptionHandler
			/// <summary>
			/// Set this class's instance to an <b>UltraCalcErrorValue</b> based on the given exception
			/// </summary>
			/// <param name="e">Exception whose value is used to set this instance's error value</param>
			private void ExceptionHandler(Exception e)
			{
				if  (e is UltraCalcErrorException)
					this.value = ((UltraCalcErrorException)e).Value;
				else if 
					(e is DivideByZeroException)
					this.value = new UltraCalcErrorValue(UltraCalcErrorCode.Div, e.Message, e);
				else if
					(e is ArithmeticException)
					this.value = new UltraCalcErrorValue(UltraCalcErrorCode.Num, e.Message, e);
				else if
					(e is InvalidCastException)
					this.value = new UltraCalcErrorValue(UltraCalcErrorCode.Num, e.Message, e);
				else if
					(e is FormatException)
					this.value = new UltraCalcErrorValue(UltraCalcErrorCode.Value, e.Message, e);


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				else
					this.value = new UltraCalcErrorValue(UltraCalcErrorCode.Value, e.Message, e);

				return;
			}
			#endregion //ExceptionHandler

			#region ToReference
			/// <summary>
			/// Convert this instance's value to a <see cref="IUltraCalcReference"/>.
			/// </summary>
			/// <returns>If this instance contains a object that implements the <see cref="IUltraCalcReference"/> interface, this method returns the object instance, else a <see cref="UltraCalcReferenceError"/> is returned.</returns>
			/// <remarks>
			/// <p class="body">
			/// The instance value's underlying data type must be reference to return a reference, else a <see cref="UltraCalcReferenceError"/> is returned.
			/// <p></p>
			/// If there is no meaningful conversion to a reference, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public IUltraCalcReference ToReference()
			{
				// MD 7/29/08 - Excel formula solving
				// Moved all code to new helper method below
				UltraCalcErrorValue errorValue;
				IUltraCalcReference reference = this.ToReference( true, out errorValue );

				if ( reference != null )
					return reference;

				return new UltraCalcReferenceError( SR.GetString( "Value_UCErrorCode_Reference" ), SR.GetString( "Error_InvalidReference" ) );
			}

			// MD 7/29/08 - Excel formula solving
			// Added new overload which allows a paramter to specify whther dereferencing the reference should add it
			// to the dynamic references of the formula being evaluated.


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			private IUltraCalcReference ToReference( bool addDynamicReferences, out UltraCalcErrorValue errorValue )
			{

				// MD 4/10/12 - TFS108678
				// Moved all code to the new overload.
				return this.ToReference(addDynamicReferences, true, out errorValue);
			}

			// MD 4/10/12 - TFS108678
			// Added a new overload.
			private IUltraCalcReference ToReference(bool addDynamicReferences, bool splitArraysForValueParameters, out UltraCalcErrorValue errorValue)
			{

				errorValue = null;

				// MD 7/17/08 - Excel formula solving
				// Moved all code to new helper method below
				IUltraCalcReference reference = this.ToReferenceHelper();


				ExcelRefBase excelRef = ExcelCalcEngine.GetResolvedReference( reference ) as ExcelRefBase;

				if ( excelRef != null )
				{
					// Get the resolved reference based on the current reference. The reference to use might be different if the 
					// formula being solved is an array formula and the reference is a range reference in a value parameter.
					// MD 4/10/12 - TFS108678
					//ExcelRefBase newReference = excelRef.ResolveReference( this.FormulaOwner, this.ExpectedTokenClass, out errorValue );
					ExcelRefBase newReference = excelRef.ResolveReference(this.FormulaOwner, this.ExpectedTokenClass, splitArraysForValueParameters, out errorValue);

					// If the resolved reference is different from the original reference...
					if ( newReference != excelRef )
					{
						// If there was a problem resolving the reference, return null.
						if ( newReference == null )
						{
							Debug.Assert( errorValue != null, "The error value should be valid if there was a problem resolving the reference." );
							return null;
						}

						// Otherwise, change all references to use the resolved reference.
						reference = newReference;
						excelRef = newReference;

						// Also, the resolved reference is a dynamic reference, because we wouldn't have added a static reference to the
						// range if we knew we didn't need to be the ancestor of all cells in the range.
						this.IsDynamicReference = true;
					}
				}

				// Set the current parameter state on the reference so it can be transferred to its value when it is dereferenced.
				if ( excelRef != null )
				{
					excelRef.ExpectedParameterClass = this.ExpectedTokenClass;
					excelRef.NumberStack = this.Owner;

					// MD 7/29/08 - Excel formula solving
					// If someone is asking for a reference from the value, they may dereference it. Let the reference know that it should add
					// itself to the dynamic references collection of the formula being solved if it is dereferenced.
					excelRef.AddDynamicReferences = addDynamicReferences && this.IsDynamicReference;
				}


				return reference;
			}

			// MD 7/17/08 - Excel formula solving
			private IUltraCalcReference ToReferenceHelper()
			{
				if (this.value is UCReference)
				    return ((UCReference)this.value).Reference;
				else if (this.value is IUltraCalcReference)
				    return (IUltraCalcReference)this.value;
				else if (this.value is UltraCalcErrorValue)
				    ((UltraCalcErrorValue)this.value).RaiseException();
				else
					return new UltraCalcReferenceError(SR.GetString("Value_UCErrorCode_Reference"),SR.GetString("Error_InvalidReference"));
				return null;
			}
			#endregion //ToReference

			#region ToErrorValue
			/// <summary>
			/// Convert this instance's value to an <see cref="UltraCalcErrorValue"/>
			/// </summary>
			/// <returns>A <see cref="UltraCalcErrorValue"/> containing the equivalent error code to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// This method returns an <see cref="UltraCalcErrorValue"/> whose error code is set this instance's error code.
			/// <p></p>
			/// If there this instance does not contain an error, a <see cref="UltraCalcErrorValue"/> containing a default value is returned
			/// </p>
			/// </remarks>
			public UltraCalcErrorValue ToErrorValue()
			{
				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue();

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value is IUltraCalcReference)
				//    return ((IUltraCalcReference)this.value).Value.ToErrorValue();
				//
				//return value as UltraCalcErrorValue; 
				// MD 7/17/08 - Excel formula solving
				// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
				//if ( resolvedValue is IUltraCalcReference )
				//	return ( (IUltraCalcReference)resolvedValue ).Value.ToErrorValue();

				return resolvedValue as UltraCalcErrorValue; 
			}
			#endregion //ToErrorValue

			#region ToString
			/// <summary>
			/// Returns a string representation of this instance's value.
			/// </summary>
			/// <returns>String representation of instance's value</returns>
			public override string ToString()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToString(CultureInfo.InvariantCulture);
				return this.ToString(this.Culture);
			}
			#endregion //ToString

			#region IsReference
			/// <summary>
			/// Returns whether this class instance contains a <see cref="IUltraCalcReference"/> value
			/// </summary>
			/// <returns>True if this instance class contains a reference, else false</returns>
			public bool IsReference
			{
				// MD 8/4/08 - Excel formula solving
				//get { return this.value is IUltraCalcReference; }
				get 
				{
					// If the value store is not a reference, this can't be a reference.
					if ( ( this.value is IUltraCalcReference ) == false )
						return false;

					// If there was no problem resolving the reference, return True.
					UltraCalcErrorValue errorValue;
					return this.ToReference( false, out errorValue ) != null;
				}
			}
			#endregion //IsReference

			#region IsError
			/// <summary>
			/// Returns whether this class instance contains an error value
			/// </summary>
			/// <returns>True if this class instance contains an error, else false</returns>
			public bool IsError
			{
				get
				{
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					object resolvedValue = this.GetResolvedValue( false );


					// MD 2/28/12 - 12.1 - Table Support
					Debug.Assert((resolvedValue is ErrorValue) == false, "This is unexpected.");


					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//if (this.value is IUltraCalcReference)
					//    return ((IUltraCalcReference)this.value).Value.IsError;
					//
					//return this.value is UltraCalcErrorValue;
					// MD 7/17/08 - Excel formula solving
					// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
					//if ( resolvedValue is IUltraCalcReference )
					//	return ( (IUltraCalcReference)resolvedValue ).Value.IsError;

					return resolvedValue is UltraCalcErrorValue;
				}
			}
			#endregion //IsError

			#region IsNull
			/// <summary>
			/// Returns whether this class instance contains a null value
			/// </summary>
			/// <returns>True if this instance class contains a null value, else false</returns>
			public bool IsNull
			{
				get
				{
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					object resolvedValue = this.GetResolvedValue( false );

					// MD 8/4/08 - Excel formula solving
					// Moved all code to the new IsNullHelper method
					return UltraCalcValue.IsNullHelper( resolvedValue );
				}
			}

			// MD 8/4/08 - Excel formula solving
			// Added new helper so this code could be used in multiple places
			private static bool IsNullHelper( object resolvedValue )
			{
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//if (this.value is IUltraCalcReference)
					//    return ((IUltraCalcReference)this.value).Value.IsNull;
					//
					//return this.value == null || (this.value is string && ((string)this.value).Length == 0) || this.value == DBNull.Value;
					// MD 7/17/08 - Excel formula solving
					// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
					//if ( resolvedValue is IUltraCalcReference )
					//	return ( (IUltraCalcReference)resolvedValue ).Value.IsNull;

					// MD 7/29/08 - Excel formula solving
					// Excel does not treat the empty string as a null value, so this needs to be refactored a bit to allow for that.
					//return
					//    resolvedValue == null ||
					//    ( resolvedValue is string && ( (string)resolvedValue ).Length == 0 ) ||
					//    resolvedValue == DBNull.Value;
					if ( resolvedValue == null || resolvedValue is DBNull )
						return true;

					// SSP 8/22/11 - XamCalculationManager
					// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
					// 
					//if ( UltraCalcValue.UseExcelFormulaCompatibility )
					if ( UltraCalcValue.UseExcelValueCompatibility )
						return false;

#pragma warning disable 0162
					string strValue = resolvedValue as string;
#pragma warning restore 0162

					return strValue != null && strValue.Length == 0;

				// MD 8/4/08 - Excel formula solving
				// Not needed anymore
				//}
			}
			#endregion //IsNull

			#region IsDBNull
			/// <summary>
			/// Returns whether this class instance contains a DBNull value
			/// </summary>
			/// <returns>True if this instance class contains a null value, else false</returns>
			public bool IsDBNull
			{
				get
				{
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					object resolvedValue = this.GetResolvedValue( false );

					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//if (this.value is IUltraCalcReference)
					//    return ((IUltraCalcReference)this.value).Value.IsDBNull;
					//
					//return this.value == DBNull.Value;
					// MD 7/17/08 - Excel formula solving
					// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
					//if ( resolvedValue is IUltraCalcReference )
					//	return ( (IUltraCalcReference)resolvedValue ).Value.IsDBNull;

					return resolvedValue == DBNull.Value;
				}
			}
			#endregion //IsDBNull

			#region IsString
			/// <summary>
			/// Returns whether this class instance contains a string value
			/// </summary>
			/// <returns>True if this instance class contains a string, else false</returns>
			public bool IsString
			{
				get
				{
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					object resolvedValue = this.GetResolvedValue( false );


					// MD 12/9/11 - TFS97567
					// The resolved value of a cell references might be a FormattedStringElement. If it is, it should be considered a string.
					if (resolvedValue is StringElement)
						return true;


					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//if (this.value is IUltraCalcReference)
					//    return ((IUltraCalcReference)this.value).Value.IsString;
					// MD 7/17/08 - Excel formula solving
					// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
					//if ( resolvedValue is IUltraCalcReference )
					//    return ( (IUltraCalcReference)resolvedValue ).Value.IsString;

					// SSP 11/9/04 UWG168
					// Use the GetTypeCode implementation of IConvertible in case the value is 
					// a custom object.
					//
					//return this.value is string;
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//return this.value is IConvertible 
					//    && System.TypeCode.String == ((IConvertible)this.value).GetTypeCode( );
					return resolvedValue is IConvertible
						&& System.TypeCode.String == ( (IConvertible)resolvedValue ).GetTypeCode();
				}
			}
			#endregion //IsString

			#region IsBoolean
			/// <summary>
			/// Returns whether this class instance contains a boolean value
			/// </summary>
			/// <returns>True if this instance class contains a boolean, else false</returns>
			public bool IsBoolean
			{
				get
				{
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					object resolvedValue = this.GetResolvedValue( false );

					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//if (this.value is IUltraCalcReference)
					//    return ((IUltraCalcReference)this.value).Value.IsBoolean;
					// MD 7/17/08 - Excel formula solving
					// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
					//if ( resolvedValue is IUltraCalcReference )
					//	return ( (IUltraCalcReference)resolvedValue ).Value.IsBoolean;

					// SSP 11/9/04 UWG168
					// Use the GetTypeCode implementation of IConvertible in case the value is 
					// a custom object.
					//
					//return this.value is Boolean	
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//return this.value is IConvertible 
					//       && System.TypeCode.Boolean == ((IConvertible)this.value).GetTypeCode( );
					return resolvedValue is IConvertible
					   && System.TypeCode.Boolean == ( (IConvertible)resolvedValue ).GetTypeCode();
				}
			}
			#endregion //IsBoolean

            #region IsDateTime
            /// <summary>
            /// Returns whether this class instance contains a DateTime value
            /// </summary>
            /// <returns>True if this instance class contains a DateTime, else false</returns>
            public bool IsDateTime
            {
                get
                {
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					object resolvedValue = this.GetResolvedValue( false );

					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//if (this.value is IUltraCalcReference)
					//    return ((IUltraCalcReference)this.value).Value.IsDateTime;
					//
					//return this.value is IConvertible
					//       && System.TypeCode.DateTime == ((IConvertible)this.value).GetTypeCode();
					// MD 7/17/08 - Excel formula solving
					// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
					//if ( resolvedValue is IUltraCalcReference )
					//	return ( (IUltraCalcReference)resolvedValue ).Value.IsDateTime;

					return resolvedValue is IConvertible
						   && System.TypeCode.DateTime == ( (IConvertible)resolvedValue ).GetTypeCode();
                }
            }
            #endregion //IsDateTime

            #region Value
            /// <summary>
			/// Return the underlying value
			/// </summary>
			public object Value 
			{
				get { return this.value; }
			}
			#endregion //Value

			#region AreValuesEqual

			/// <summary>
			/// Indicates if the value of the specified <see cref="UltraCalcValue"/> is equivalent to the <see cref="Value"/>
			/// </summary>
			/// <param name="x">First <see cref="UltraCalcValue"/> to compare</param>
			/// <param name="y">Second <see cref="UltraCalcValue"/> to compare</param>
			/// <returns>True if the values are the same; otherwise false is returned.</returns>
			public static bool AreValuesEqual(UltraCalcValue x, UltraCalcValue y)
			{
				// Convert operands to doubles and apply operator  
				if ( null == (object)x )
					return null == (object)y;

				if ( null == (object)y )
					return false;

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object xResolvedValue = x.GetResolvedValue();
				object yResolvedValue = y.GetResolvedValue();

				// MD 2/9/12 - TFS101326
				// In Excel, when testing for string equality, strings are compared case-insensitively.
				if (UltraCalcValue.UseExcelValueCompatibility)
				{
					if (x.IsString && y.IsString)
					{
						string xString = xResolvedValue.ToString();
						string yString = yResolvedValue.ToString();

						// MD 4/6/12 - TFS101506
						//if (String.Equals(xString, yString, StringComparison.CurrentCultureIgnoreCase))
						if (String.Compare(xString, yString, x.Culture, CompareOptions.IgnoreCase) == 0)
							return true;
					}
				}

                // MRS 5/4/06 - BR11684
                // The way this is set up, it will compare an empty string to a "0" and find them
                // no equal. At this point, we want to return false, not convert the values into 
                // doubles. 
                //if (Object.Equals(x.value, y.value))
                //    return true;
				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (x.value is IComparable &&
				//    y.value is IComparable &&
				//    x.value.GetType() == y.value.GetType())
				//{
				//    return ((IComparable)x.value).CompareTo(y.value) == 0;
				//}
				if ( xResolvedValue is IComparable &&
					yResolvedValue is IComparable &&
					xResolvedValue.GetType() == yResolvedValue.GetType() )
				{
					return ( (IComparable)xResolvedValue ).CompareTo( yResolvedValue ) == 0;
				}

				// MD 7/14/08 - Excel formula solving
				// When doing equality checks in Excel, boolean and string types do not equate to their numerical equivalents.
				// If only one value is a boolean or string, but not both, there is no way they can be equal.
				// SSP 8/22/11 - XamCalculationManager
				// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
				// 
				//if ( UltraCalcValue.UseExcelFormulaCompatibility )
				if ( UltraCalcValue.UseExcelValueCompatibility )
				{
					// MD 2/3/12 - TFS101005
					// When testing for equality in excel, empty cells are equal to the empty string and False.
					// (They are also equal to 0, but this worked already with the ToDouble calls below).
					if (xResolvedValue == null)
					{
						if (yResolvedValue == null ||
							(yResolvedValue is string && (string)yResolvedValue == string.Empty) ||
							(yResolvedValue is bool && (bool)yResolvedValue == false))
						{
							return true;
						}
					}
					else if (yResolvedValue == null)
					{
						if ((xResolvedValue is string && (string)xResolvedValue == string.Empty) ||
							(xResolvedValue is bool && (bool)xResolvedValue == false))
						{
							return true;
						}
					}

					if ( x.IsBoolean ^ y.IsBoolean )
						return false;

					if ( x.IsString ^ y.IsString )
						return false;
				}

				double xDbl, yDbl;

				if (x.ToDouble(out xDbl) && y.ToDouble(out yDbl))
					return xDbl == yDbl;

				// AS 9/17/04
				// Equals should be more efficient.
				//return String.Compare(x.ToString(),y.ToString()) == 0;
				return string.Equals(x.ToString(), y.ToString());
			}

			#endregion //AreValuesEqual

			#region CompareTo

			// SSP 1/7/05 BR01401
			// Implemented IComparable on the UltraCalcValue.
			//
			int IComparable.CompareTo( object value )
			{
				return this.CompareTo( (UltraCalcValue)value );
			}

			/// <summary>
			/// Compares current instance with the passed in <see cref="UltraCalcValue"/> instance. Returns 
			/// -1, 1 or 0 depending on whether the current instance is less than, greater than
			/// or equal to the passed in instance respectively.
			/// </summary>
            /// <param name="value">The object that this instance should be compared against.</param>
            /// <returns>-1, 1 or 0 depending on whether the current instance is less than, greater than
            /// or equal to the passed in instance respectively.</returns>
			public int CompareTo( UltraCalcValue value )
			{
				return UltraCalcValue.CompareTo( this, value );
			}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			private object GetUnderlyingValue( )
			{
				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue();

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//IUltraCalcReference reference = this.value as IUltraCalcReference;
				//return null == reference ? this.value : reference.Value.GetUnderlyingValue( );
				// MD 7/17/08 - Excel formula solving
				// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
				//IUltraCalcReference reference = resolvedValue as IUltraCalcReference;

				// MD 7/17/08 - Excel formula solving
				// See comment above, this cannot be a reference anymore
				//return null == reference ? resolvedValue : reference.Value.GetUnderlyingValue();
				return resolvedValue;
			}

			/// <summary>
			/// Compares x and y <see cref="UltraCalcValue"/> instances and returns -1 if x is less than y, 
			/// 1 if x is greater than y and 0 if x and y are equal.
			/// </summary>
            /// <param name="x">The first value to compare.</param>
            /// <param name="y">The value to compare with the first value.</param>
            /// <returns>-1, 1 or 0 depending on whether the current instance is less than, greater than
            /// or equal to the passed in instance respectively.</returns>
			public static int CompareTo( UltraCalcValue x, UltraCalcValue y )
			{
				if ( (object)x == (object)y )
					return 0;
				else if ( null == (object)x )
					return -1;
				else if ( null == (object)y )
					return 1;

				// MD 7/14/08 - Excel formula solving
				// SSP 8/22/11 - XamCalculationManager
				// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
				// 
				//if ( UltraCalcValue.UseExcelFormulaCompatibility )
				if (UltraCalcValue.UseExcelValueCompatibility)
				{
					// There is a precedence when comparing values in Excel:
					//   1. Boolean
					//   2. String
					//   3. Number
					// Therefore, if of the type is a boolean, but the other is not, we know for sure which one is greater.
					// The same is true for strings. If both arguments are booleans or both are strings or both are neither,
					// none of the if statements below will be entered and the normal processing below will be used.s
					if ( x.IsBoolean && y.IsBoolean == false )
						return 1;
					else if ( y.IsBoolean && x.IsBoolean == false )
						return -1;
					else if ( x.IsString && y.IsString == false )
						return 1;
					else if ( y.IsString && x.IsString == false )
						return -1;

					// Strings should be compared alphabetically in Excel. They should not be converted to numbers and compared.
					if ( x.IsString && y.IsString )
					{
						string xStringValue = x.ToString();
						string yStringValue = y.ToString();

						return string.Compare( xStringValue, yStringValue );
					}
				}

				// First attempt to perform numeric comparision.
				//
				double xDouble, yDouble;
				if ( x.ToDouble( out xDouble ) && y.ToDouble( out yDouble ) )
				{
					// MD 3/2/12 - TFS103395
					//return xDouble < yDouble ? -1 : ( xDouble > yDouble ? 1 : 0 );
					xDouble = MathUtilities.RoundToExcelDisplayValue(xDouble);
					yDouble = MathUtilities.RoundToExcelDisplayValue(yDouble);
					return xDouble.CompareTo(yDouble);
				}

				object xObject = x.GetUnderlyingValue( );
				object yObject = y.GetUnderlyingValue( );

				// If the values are not numeric but of the same type then perform use
				// IComparable.CompareTo.
				//
				if ( null != xObject && null != yObject && xObject.GetType( ) == yObject.GetType( ) )
				{
					IComparable xComparable = xObject as IComparable;
					if ( null != xComparable )
						return xComparable.CompareTo( yObject );
				}

				// If the values are not the same type or they are not IComparable then
				// convert them to strings and compare the strings.
				//
				string xString = x.ToString( );
				string yString = y.ToString( );

				return string.Compare( xString, yString );
			}

			#endregion // CompareTo			

			#region IsSameValue
			/// <summary>
			/// Indicates if the specified <see cref="UltraCalcValue"/> has the save <see cref="Value"/> as this instance.
			/// </summary>
			/// <param name="value"><see cref="UltraCalcValue"/> to compare against.</param>
			/// <returns>True if the <see cref="Value"/> of both instances are equal; otherwise false.</returns>
			public bool IsSameValue(UltraCalcValue value)
			{
				return UltraCalcValue.AreValuesEqual(this, value);
			}
			#endregion //IsSameValue

            // MRS NAS 8.1
            #region GetResolvedValue
            /// <summary>
            /// Gets the resolved value of the reference. This method will walk down the reference chain recursively to get the resolved value of the reference that is not just another reference. 
            /// </summary>
            /// <returns></returns>
            public object GetResolvedValue()
            {
				// MD 7/17/08 - Excel formula solving
				// This logic has been superseded by the other GetResolvedValue overload
				//object resolvedValue = this.value;
                //           
				//while ( resolvedValue is IUltraCalcReference )
				//    resolvedValue = ( (IUltraCalcReference)resolvedValue ).Value;
				//
				//if (resolvedValue is UltraCalcValue)
				//    return ((UltraCalcValue)resolvedValue).Value;
				//else
				//    return resolvedValue;
				return this.GetResolvedValue( true, false );
            }
            #endregion //GetResolvedValue

		#region Implementation of IConvertible
			/// <summary>
			/// Convert this class instance's value to a ulong data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>An 64-bit unsigned integer equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			ulong IConvertible.ToUInt64(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDecimal method can convert all types to all 64-bit unsigned integer values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				return (ulong)this.ToDecimal( provider );
			}

			/// <summary>
			/// Convert this class instance's value to a SByte data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>An 8-bit signed integer equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			sbyte IConvertible.ToSByte(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDecimal method can convert all types to all 8-bit integer values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)











































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				return (sbyte)this.ToDecimal( provider );
			}

			/// <summary>
			/// Convert this class instance's value to a double data type
			/// </summary>
			/// <seealso cref="ToDouble(System.IFormatProvider)"/>
			public double ToDouble()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToDouble(CultureInfo.InvariantCulture);
				return this.ToDouble(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to a double data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A double-precision floating-point equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public double ToDouble(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code. Use the other overload of ToDouble
				#region Old Code

				
#region Infragistics Source Cleanup (Region)

























































#endregion // Infragistics Source Cleanup (Region)



				#endregion Old Code
				double result;
				Exception exception;
				bool succeeded = this.ToDouble( out result, provider, out exception );

				if ( succeeded )
					return result;

				if ( exception == null )
					throw new InvalidCastException();

				throw exception;
			}

			

			// AS 9/16/04
			// Implemented double method that will attempt to prevent
			// raising exceptions.
			//
			/// <summary>
			/// Converts the <see cref="Value"/> to a double data type
			/// </summary>
			/// <param name="result">The resulting double value. If the return value is false, the result is 0.</param>
			/// <returns>True if the value was successfully converted to a double; otherwise false.</returns>
			public bool ToDouble(out double result)
			{
				// MD 8/12/08 - Excel formula solving
				// Moved all code to the new overload
				Exception exception;
				// MD 4/6/12 - TFS101506
				//return this.ToDouble( out result, CultureInfo.InvariantCulture, out exception );
				return this.ToDouble(out result, this.Culture, out exception);
			}

			// MD 8/12/08 - Excel formula solving
			// Added new overload to contain the logic for all other overloads of ToDouble so they can all call into this and we don't duplicate code.
			private bool ToDouble( out double result, IFormatProvider provider, out Exception exception )
			{
				// MD 8/12/08 - Excel formula solving
				// Initialize the out parameter
				exception = null;

				// MD 8/4/08 - Excel formula solving
				// We need to get the resolved value before checking for null so dynamic references in Excel can be added if necessary,
				// so this check has been moved below.
				//if (this.IsNull)
				//{
				//    result = 0;
				//    return true;
				//}

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue();

				// MD 8/4/08 - Excel formula solving
				// We have to check for null after calling GetResolvedValue so dynamic references are correctly added, use the helper so 
				// the value doesn't have to be resolved twice.
				if ( UltraCalcValue.IsNullHelper( resolvedValue ) )
				{
					result = 0;
					return true;
				}

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value is string)
				//{
				//    return Double.TryParse( (string)this.value, 
				//        NumberStyles.Float | NumberStyles.AllowThousands,
				//        CultureInfo.InvariantCulture, out result );
				//}
				if ( resolvedValue is string )
				{
					// MD 8/12/08 - Excel formula solving
					// If the string could not parse correctly, we want to specify an exception to be thrown by the caller. Also, the format provider is 
					// specified as an argument to the method.
					//return Double.TryParse( (string)resolvedValue,
					//    NumberStyles.Float | NumberStyles.AllowThousands,
					//    CultureInfo.InvariantCulture, out result );
					// MD 4/9/12 - TFS101506
					//if ( Double.TryParse( (string)resolvedValue, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result ) )
					if (MathUtilities.DoubleTryParse((string)resolvedValue, provider, out result))
						return true;

					exception = new FormatException();
					return false;
				}

				// MD 8/12/08 - Excel formula solving
				// With excel formula compatibility, DateTimes convert to numbers in a certain way.
				// SSP 8/22/11 - XamCalculationManager
				// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
				// 
				//if ( UltraCalcValue.UseExcelFormulaCompatibility && resolvedValue is DateTime )
				if ( UltraCalcValue.UseExcelValueCompatibility && resolvedValue is DateTime )
				{
					double? value = UltraCalcValue.DateTimeToExcelDate(

						this.Workbook, 

						(DateTime)resolvedValue );

					// If the date couldn't convert to a number, it was out of range of the valid Excel dates.
					if ( value.HasValue == false )
					{
						exception = new ArgumentOutOfRangeException();
						result = 0;
						return false;
					}

					result = value.Value;
					return true;
				}

				try
				{
					// MD 7/14/08 - Excel formula solving
					// Some additional processing may need to be done when getting the value, so use the value from the new helper method
					//if (this.value is IConvertible) 
					//{
					//    result = ((IConvertible)this.value).ToDouble(CultureInfo.InvariantCulture);
					//    return true;
					//}
					//else if	(this.value is IUltraCalcReference )
					//    return ((IUltraCalcReference)this.value).Value.ToDouble(out result);
					if ( resolvedValue is IConvertible )
					{
						// MD 8/12/08 - Excel formula solving
						// The provider is now specified to the method.
						//result = ( (IConvertible)resolvedValue ).ToDouble( CultureInfo.InvariantCulture );
						result = ( (IConvertible)resolvedValue ).ToDouble( provider );

						return true;
					}
					// MD 7/17/08 - Excel formula solving
					// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
					//else if ( resolvedValue is IUltraCalcReference )
					//	return ( (IUltraCalcReference)resolvedValue ).Value.ToDouble( out result );

					else
					{
						result = 0;
						return false;
					}
				}
				// MD 8/12/08 - Excel formula solving
				// We need to get the exception instance when one occurs.
				//catch (Exception)
				catch ( Exception exc )
				{
					// MD 8/12/08 - Excel formula solving
					// Store the exception on the out parameter.
					exception = exc;

					result = 0;
					return false;
				}
			}

			/// <summary>
			/// Convert this class instance's value to a DateTime data type
			/// </summary>
			/// <seealso cref="ToDateTime(System.IFormatProvider)"/>
			public DateTime ToDateTime()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToDateTime(CultureInfo.InvariantCulture);
				return this.ToDateTime(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to a DateTime data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A <b>DateTime</b> equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public System.DateTime ToDateTime(System.IFormatProvider provider)
			{
				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue( true, true );

				// MD 8/12/08 - Excel formula solving
				// If the resolved value is a DateTime, just return it.
				if ( resolvedValue is DateTime )
					return (DateTime)resolvedValue;

				// MD 8/12/08 - Excel formula solving
				// With excel formula compatibility, numbers convert to DateTimes in a certain way.
				// SSP 8/22/11 - XamCalculationManager
				// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
				// 
				//if ( UltraCalcValue.UseExcelFormulaCompatibility )
				if ( UltraCalcValue.UseExcelValueCompatibility )
				{
					double excelDate;
					Exception exception;
					if ( this.ToDouble( out excelDate, provider, out exception ) )
					{
						DateTime? convertedDate = UltraCalcValue.ExcelDateToDateTime(

						this.Workbook,

						excelDate );

						if ( convertedDate.HasValue == false )
							throw new InvalidCastException();

						return convertedDate.Value;
					}
				}

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value is Int64)
				if ( resolvedValue is Int64 )
				{
					// AS 9/9/04
					// If the long value is not within the valid range of a datetime
					// then raise an invalid cast exception instead of letting the 
					// argument out of range percolate up.
					//
					try
					{
						// MD 7/14/08 - Excel formula solving
						// Some additional processing may need to be done when getting the value, so use the value from the new helper method
						//return new DateTime( ((Int64)this.value) );
						return new DateTime( ( (Int64)resolvedValue ) );
					}
					catch (ArgumentOutOfRangeException)
					{
						throw new InvalidCastException();
					}
				}

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value is IConvertible) 
				//    return ((IConvertible)this.value).ToDateTime(provider);
				//else if	(this.value is IUltraCalcReference )
				//    return ((IConvertible)((IUltraCalcReference)this.value).Value).ToDateTime(provider);
				//else if (this.value is UltraCalcErrorValue)
				//    ((UltraCalcErrorValue)this.value).RaiseException();
				if ( resolvedValue is IConvertible )
					return ( (IConvertible)resolvedValue ).ToDateTime( provider );
				// MD 7/17/08 - Excel formula solving
				// GetResolvedValue will dereference the values automatically and throw an exception on error, we don't need to do this anymore
				//else if ( resolvedValue is IUltraCalcReference )
				//	return ( (IConvertible)( (IUltraCalcReference)resolvedValue ).Value ).ToDateTime( provider );
				//else if ( resolvedValue is UltraCalcErrorValue )
				//	( (UltraCalcErrorValue)resolvedValue ).RaiseException();

				else
					throw new InvalidCastException();

				// MD 7/17/08 - Excel formula solving
				// Unreachable code
				//return System.DateTime.Now;
			}

			/// <summary>
			/// Convert this class instance's value to a float data type
			/// </summary>
			/// <seealso cref="ToSingle(System.IFormatProvider)"/>
			public float ToSingle()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToSingle(CultureInfo.InvariantCulture);
				return this.ToSingle(this.Culture);
			}
			
			/// <summary>
			/// Convert this class instance's value to a float data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A single-precision floating-point equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public float ToSingle(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDouble method can convert all types to all single floating point values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				return (float)this.ToDouble( provider );
			}
		
			/// <summary>
			/// Convert this class instance's value to a boolean data type
			/// </summary>
			/// <seealso cref="ToBoolean(System.IFormatProvider)"/>
            /// <returns>A boolean floating-point equivalent to the value of this instance</returns>
            public bool ToBoolean()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToBoolean(CultureInfo.InvariantCulture);
				return this.ToBoolean(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to a boolean data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A boolean floating-point equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public bool ToBoolean(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This has been refactored and simplified.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)


















































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				object resolvedValue = this.GetResolvedValue( true, true );

				if ( resolvedValue is bool )
					return (bool)resolvedValue;

				if ( UltraCalcValue.IsNullHelper( resolvedValue ) )
					return false;

				string strValue = resolvedValue as string;

				if ( strValue != null )
				{
					bool result;
					if ( bool.TryParse( strValue, out result ) )
						return result;

					// SSP 8/22/11 - XamCalculationManager
					// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
					// 
					//if ( UltraCalcValue.UseExcelFormulaCompatibility )
					if ( UltraCalcValue.UseExcelValueCompatibility )
						throw new FormatException();
				}

				return this.ToDouble( provider ) != 0;
			}
			
			/// <summary>
			/// Convert this class instance's value to a int data type
			/// </summary>
			/// <seealso cref="ToInt32(System.IFormatProvider)"/>
            /// <returns>A 32-bit signed integer floating-point equivalent to the value of this instance</returns>
            public int ToInt32()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToInt32(CultureInfo.InvariantCulture);
				return this.ToInt32(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to a int data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A 32-bit signed integer floating-point equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public int ToInt32(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDecimal method can convert all types to all 32-bit integer values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)



















































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				return (int)this.ToDecimal( provider );
			}

			/// <summary>
			/// Convert this class instance's value to a ushort data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A 16-bit unsigned integer floating-point equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			ushort IConvertible.ToUInt16(System.IFormatProvider provider)
			{
				
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)

				return (ushort)this.ToDecimal( provider );
			}
			
			/// <summary>
			/// Convert this class instance's value to a short data type
			/// </summary>
			/// <seealso cref="ToInt16(System.IFormatProvider)"/>
			public short ToInt16()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToInt16(CultureInfo.InvariantCulture);
				return this.ToInt16(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to a short data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A 16-bit signed integer floating-point equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public short ToInt16(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDecimal method can convert all types to all 16-bit integer values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				return (short)this.ToDecimal( provider );
			}

			/// <summary>
			/// Convert this class instance's value to a string instance data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A string instance equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public string ToString(System.IFormatProvider provider)
			{
				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue();

				// MD 7/14/08 - Excel formula solving
				// SSP 8/22/11 - XamCalculationManager
				// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
				// 
				//if ( UltraCalcValue.UseExcelFormulaCompatibility )
				if ( UltraCalcValue.UseExcelValueCompatibility )
				{
					// Booleans convert to upper-case
					if ( resolvedValue is bool )
						return ( (bool)resolvedValue ).ToString( provider ).ToUpper();

					// Dates convert to their Excel date number value and then to a string in Excel.
					if ( resolvedValue is DateTime )
					{
						// MD 5/1/11
						// We end up losing some precision occasionally when converting to a decimal. Convert to a double instead.
						//return this.ToDecimal( provider ).ToString( provider );
						return this.ToDouble(provider).ToString(provider);
					}
				}

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value is IConvertible) 
				//    return ((IConvertible)this.value).ToString(provider);
				//else if	(this.value is IUltraCalcReference )
				//    return ((IConvertible)((IUltraCalcReference)this.value).Value).ToString(provider);
				//else if	(this.value is UltraCalcErrorValue )
				//    return ((UltraCalcErrorValue)this.value).ToString();
				//else if (this.value is UltraCalcErrorException)
				//    return ((UltraCalcErrorException)this.value).ToString();
				//else if (null == value ) 
				//    return "";
				//else
				//    return this.value.ToString();
				if ( resolvedValue is IConvertible )
					return ( (IConvertible)resolvedValue ).ToString( provider );
				// MD 7/17/08 - Excel formula solving
				// GetResolvedValue will dereference the values automatically, we don't need to do this anymore
				//else if ( resolvedValue is IUltraCalcReference )
				//	return ( (IConvertible)( (IUltraCalcReference)resolvedValue ).Value ).ToString( provider );
				else if ( resolvedValue is UltraCalcErrorValue )
					return ( (UltraCalcErrorValue)resolvedValue ).ToString();
				else if ( resolvedValue is UltraCalcErrorException )
					return ( (UltraCalcErrorException)resolvedValue ).ToString();
				else if ( null == resolvedValue )
					return "";
				else
					return resolvedValue.ToString();
			}

			/// <summary>
			/// Convert this class instance's value to a byte data type
			/// </summary>
			/// <seealso cref="ToByte(System.IFormatProvider)"/>
			public byte ToByte()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToByte(CultureInfo.InvariantCulture);
				return this.ToByte(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to a byte data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A 8-bit unsigned integer equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public byte ToByte(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDecimal method can convert all types to all 8-bit unsigned integer values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)











































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				return (byte)this.ToDecimal( provider );
			}

			/// <summary>
			/// Convert this class instance's value to a char data type
			/// </summary>
			/// <seealso cref="ToChar(System.IFormatProvider)"/>
            /// <returns>A Unicode character equivalent to the value of this instance</returns>
            public char ToChar()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToChar(CultureInfo.InvariantCulture);
				return this.ToChar(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to a char type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A Unicode character equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public char ToChar(System.IFormatProvider provider)
			{
				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue( true, true );

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value is IConvertible) 
				//    return ((IConvertible)this.value).ToChar(provider);
				//else if	(this.value is IUltraCalcReference )
				//    return ((IConvertible)((IUltraCalcReference)this.value).Value).ToChar(provider);
				//else if (this.value is UltraCalcErrorValue)
				//    ((UltraCalcErrorValue)this.value).RaiseException();
				if ( resolvedValue is IConvertible )
					return ( (IConvertible)resolvedValue ).ToChar( provider );
				// MD 7/17/08 - Excel formula solving
				// GetResolvedValue will dereference the values automatically and throw an exception on error, we don't need to do this anymore
				//else if ( resolvedValue is IUltraCalcReference )
				//	return ( (IConvertible)( (IUltraCalcReference)resolvedValue ).Value ).ToChar( provider );
				//else if ( resolvedValue is UltraCalcErrorValue )
				//	( (UltraCalcErrorValue)resolvedValue ).RaiseException();

				else
					throw new InvalidCastException();

				// MD 7/17/08 - Excel formula solving
				// Unreachable code
				//return (char)0;
			}

			/// <summary>
			/// Convert this class instance's value to a long data type
			/// </summary>
			/// <seealso cref="ToInt64(System.IFormatProvider)"/>
			public long ToInt64()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToInt64(CultureInfo.InvariantCulture);
				return this.ToInt64(this.Culture);
			}

			/// <summary>
			/// Convert this class instance's value to a long data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A 64-bit signed integer equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public long ToInt64(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDecimal method can convert all types to all 64-bit integer values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)







































































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				// SSP 8/22/11 - XamCalculationManager
				// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
				// 
				//if ( UltraCalcValue.UseExcelFormulaCompatibility == false )
				if ( UltraCalcValue.UseExcelValueCompatibility == false )
				{
#pragma warning disable 0162
					object resolvedValue = this.GetResolvedValue( true, true );
#pragma warning restore 0162

					if ( resolvedValue is DateTime )
						return ( (DateTime)resolvedValue ).Ticks;
				}

				return (long)this.ToDecimal( provider );
			}

			/// <summary>
			/// Returns the <b>TypeCode</b> for this instance
			/// </summary>
			/// <returns>The enumerated constant that is the <b>TypeCode</b> of the class or value type that implements this interface.</returns>
			public System.TypeCode GetTypeCode()
			{
				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue( false );

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value == null || !(this.value is IConvertible)) 
				//    return new System.TypeCode();
				//else 
				//    return ((IConvertible)this.value).GetTypeCode();
				if ( resolvedValue == null || !( resolvedValue is IConvertible ) )
					return new System.TypeCode();
				else
					return ( (IConvertible)resolvedValue ).GetTypeCode();
			}

			/// <summary>
			/// Convert this class instance's value to a decimal data type
			/// </summary>
			/// <seealso cref="ToDecimal(System.IFormatProvider)"/>
            /// <returns>A <b>Decimal</b> equivalent to the value of this instance</returns>
            /// <seealso cref="ToDecimal(System.IFormatProvider)"/>
            public decimal ToDecimal()
			{
				// MD 4/6/12 - TFS101506
				//return this.ToDecimal(CultureInfo.InvariantCulture);
				return this.ToDecimal(this.Culture);
			}

			// MD 2/28/12 - TFS103395
			/// <summary>
			/// Converts the <see cref="Value"/> to a decimal data type
			/// </summary>
			/// <param name="result">The resulting decimal value. If the return value is false, the result is 0.</param>
			/// <returns>True if the value was successfully converted to a decimal; otherwise false.</returns>
			public bool ToDecimal(out decimal result)
			{
				Exception exc;
				// MD 4/6/12 - TFS101506
				//return this.ToDecimalHelper(out result, CultureInfo.CurrentCulture, out exc);
				return this.ToDecimalHelper(out result, this.Culture, out exc);
			}

			/// <summary>
			/// Convert this class instance's value to a decimal data type
			/// </summary>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>A <b>Decimal</b> equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public decimal ToDecimal(System.IFormatProvider provider)
			{
				// MD 2/28/12 - TFS103395
				// Moved all code to a helper method so it could be used in other places.
				decimal result;
				Exception exception;
				if (this.ToDecimalHelper(out result, provider, out exception))
					return result;

				if (exception == null)
					throw new InvalidCastException();

				throw exception;
			}

			// MD 2/28/12 - TFS103395
			private bool ToDecimalHelper(out decimal result, System.IFormatProvider provider, out Exception exception)
			{
				exception = null;

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue( true, true );

				// MD 8/12/08 - Excel formula solving
				// Nulls convert to 0.
				if ( UltraCalcValue.IsNullHelper( resolvedValue ) )
				{
					// MD 2/28/12 - TFS103395
					//return 0;
					result = 0;
					return true;
				}

				// MD 8/12/08 - Excel formula solving
				// With excel formula compatibility, DateTimes convert to numbers in a certain way.
				// SSP 8/22/11 - XamCalculationManager
				// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
				// 
				//if ( UltraCalcValue.UseExcelFormulaCompatibility && resolvedValue is DateTime )
				if ( UltraCalcValue.UseExcelValueCompatibility && resolvedValue is DateTime )
				{
					// MD 2/28/12 - TFS103395
//                    return (decimal)UltraCalcValue.DateTimeToExcelDate(
//#if EXCEL
//                        this.Workbook,
//#endif
//                        (DateTime)resolvedValue );

					double? value = UltraCalcValue.DateTimeToExcelDate(

						this.Workbook,

						(DateTime)resolvedValue);

					// If the date couldn't convert to a number, it was out of range of the valid Excel dates.
					if (value.HasValue == false)
					{
						exception = new ArgumentOutOfRangeException();
						result = 0;
						return false;
					}

					result = (decimal)value.Value;
					return true;
				}

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value is IConvertible) 
				//    if (this.IsNull)
				//        return 0;
				//    else
				//        return ((IConvertible)this.value).ToDecimal(provider);
				//else if	(this.value is IUltraCalcReference )
				//    return ((IConvertible)((IUltraCalcReference)this.value).Value).ToDecimal(provider);
				//else if (this.value is UltraCalcErrorValue)
				//    ((UltraCalcErrorValue)this.value).RaiseException();
				if (resolvedValue is IConvertible)
				{
					// MD 8/12/08 - Excel formula solving
					// We would never get in here for nulls. This check has been moved above. Also, the else block has been wrapped in a try...catch 
					// just in case it can't be converted to a decimal but can be converted to a double (the string "1E0").
					//if ( this.IsNull )
					//    return 0;
					//else
					try
					{
						// MD 2/28/12 - TFS103395
						//return ( (IConvertible)resolvedValue ).ToDecimal( provider );
						result = ((IConvertible)resolvedValue).ToDecimal(provider);
						return true;
					}
					// MD 2/28/12 - TFS103395
					//catch ( FormatException )
					catch (FormatException exc)
					{
						// SSP 8/22/11 - XamCalculationManager
						// Split the UseExcelFormulaCompatibility into UseExcelFunctionCompatibility and UseExcelValueCompatibility.
						// 
						//if ( UltraCalcValue.UseExcelFormulaCompatibility )
						if ( UltraCalcValue.UseExcelValueCompatibility )
						{
							string strValue = resolvedValue as string;

							if (strValue != null)
							{
								// MD 2/28/12 - TFS103395
								//double dblValue;
								//if ( Double.TryParse( strValue, NumberStyles.Float | NumberStyles.AllowThousands, provider, out dblValue ) )
								//    return (decimal)dblValue;
								decimal parsedValue;
								// MD 4/9/12 - TFS101506
								//if (Decimal.TryParse(strValue, NumberStyles.Float | NumberStyles.AllowThousands, provider, out parsedValue))
								if (MathUtilities.DecimalTryParse(strValue, provider, out parsedValue))
								{
									result = parsedValue;
									return true;
								}
							}
						}

						// MD 2/28/12 - TFS103395
						//throw;
						exception = exc;
						result = 0;
						return false;
					}
				}
				// MD 7/17/08 - Excel formula solving
				// GetResolvedValue will dereference the values automatically and throw an exception on error, we don't need to do this anymore
				//else if ( resolvedValue is IUltraCalcReference )
				//	return ( (IConvertible)( (IUltraCalcReference)resolvedValue ).Value ).ToDecimal( provider );
				//else if ( resolvedValue is UltraCalcErrorValue )
				//	( (UltraCalcErrorValue)resolvedValue ).RaiseException();

				else
				{
					// MD 2/28/12 - TFS103395
					//throw new InvalidCastException();
					result = 0;
					return false;
				}

				// MD 7/17/08 - Excel formula solving
				// Unreachable code
				//return 0;
			}

			/// <summary>
			/// Convert this class instance's value to a uint data type
			/// </summary>
			/// <param name="conversionType">The <b>Type</b> to which the value of this instance is converted</param>
			/// <param name="provider">An <b>IFormatProvider</b> interface implementation that supplies culture-specific formatting information</param>
			/// <returns>An Object instance of type conversionType whose value is equivalent to the value of this instance.</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			public object ToType(System.Type conversionType, System.IFormatProvider provider)
			{
				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				object resolvedValue = this.GetResolvedValue( true, true );

				// MD 7/14/08 - Excel formula solving
				// Some additional processing may need to be done when getting the value, so use the value from the new helper method
				//if (this.value is IConvertible) 
				//    return ((IConvertible)this.value).ToType(conversionType, provider);
				//else if	(this.value is IUltraCalcReference )
				//    return ((IConvertible)((IUltraCalcReference)this.value).Value).ToType(conversionType, provider);
				//else if (this.value is UltraCalcErrorValue)
				//    ((UltraCalcErrorValue)this.value).RaiseException();
				if ( resolvedValue is IConvertible )
					return ( (IConvertible)resolvedValue ).ToType( conversionType, provider );
				// MD 7/17/08 - Excel formula solving
				// GetResolvedValue will dereference the values automatically and throw an exception on error, we don't need to do this anymore
				//else if ( resolvedValue is IUltraCalcReference )
				//	return ( (IConvertible)( (IUltraCalcReference)resolvedValue ).Value ).ToType( conversionType, provider );
				//else if ( resolvedValue is UltraCalcErrorValue )
				//	( (UltraCalcErrorValue)resolvedValue ).RaiseException();

				else
					throw new InvalidCastException();

				// MD 7/17/08 - Excel formula solving
				// Unreachable code
				//return null;
			}

			/// <summary>
			/// Convert this class instance's value to a uint data type
			/// </summary>
			/// <param name="provider">Format provider to use in conversion</param>
			/// <returns>A 32-bit unsigned integer equivalent to the value of this instance</returns>
			/// <remarks>
			/// <p class="body">
			/// If there is no meaningful conversion, this method will throw an <b>InvalidCastException</b>
			/// </p>
			/// </remarks>
			uint IConvertible.ToUInt32(System.IFormatProvider provider)
			{
				// MD 8/12/08 - Excel formula solving
				// This was all redudant code and the ToDecimal method can convert all types to all 32-bit unsigned integer values anyway, 
				// so just use that method and cast the result.
				#region Old Code

				
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)


				#endregion Old Code
				return (uint)this.ToDecimal( provider );
			}
		#endregion

			// 8/8/08 - Excel formula solving
			// This has been moved from Infragistics.Documents.Excel.Utilities
			// MD 10/24/07 - BR27751
			#region DateTimeToExcelDate

			/// <summary>
			/// Converts a DateTime to Excel's numerical representation of a date.
			/// </summary>
			/// <remarks>
			/// <p class="body">
			/// When using the 1900 date system in Excel, dates before 3/1/1900 must be corrected, because Excel 
			/// incorrectly assumes 1900 is a leap year. This overload assumes the 1900 date system is being used
			/// as so it corrects the date values.
			/// </p>
			/// </remarks>
			/// <param name="dateValue">The DateTime value to convert to the Microsoft Excel date format.</param>
			// MD 4/15/09 - TFS16390
			// This has been made public so that it could be used by the functions in the calc manager assembly.
			//internal static double? DateTimeToExcelDate(
			public static double? DateTimeToExcelDate(

#pragma warning disable 1573
				Workbook workbook,
#pragma warning restore 1573

				DateTime dateValue )
			{
				// MD 4/15/09 - TFS16390
				// Moved all code to the new overload.
				return UltraCalcValue.DateTimeToExcelDate(

					workbook,  

					dateValue, true );
			}

			// MD 4/15/09 - TFS16390
			// Added a new overload because I needed a way to turn off the 1900 date correction. I needed to do this when converting 
			// time values, which technically have a date of 1/1/0001, but I don't want their value corrected.
			/// <summary>
			/// Converts a DateTime to Excel's numerical representation of a date.
			/// </summary>
			/// <param name="dateValue">The DateTime value to convert to the Microsoft Excel date format.</param>
			/// <param name="shouldCorrect1900Dates">
			/// When using the 1900 date system in Excel, dates before 3/1/1900 must be corrected, because Excel 
			/// incorrectly assumes 1900 is a leap year. Pass False to disable this correction.
			/// </param>
			public static double? DateTimeToExcelDate(

#pragma warning disable 1573
				Workbook workbook,
#pragma warning restore 1573

				DateTime dateValue,
				bool shouldCorrect1900Dates )
			{
				// We don't want to do the leap year correction for the min date
				if ( dateValue.Ticks == 0 )
				{
					// MD 9/16/11 - TFS87857
					// The min date is not a valid excel date, so return null.
					//return 0;
					return null;
				}

				// MD 9/16/11 - TFS87857
				// This will throw an exception if there is no OA date to describe the date. 
				// If this is the case, the date is not a valid excel date, so return null.
				//double result = dateValue.ToOADate();
				double result;
				try
				{
					result = dateValue.ToOADate();
				}
				catch 
				{
					return null;
				}

				// MD 4/15/09 - TFS16390
				// Honor the new shouldCorrect1900Dates parameter.
				//bool correct1900Dates = true;
				bool correct1900Dates = shouldCorrect1900Dates;

				bool allowZero = false;


				if ( workbook != null && workbook.DateSystem == DateSystem.From1904 )
				{
					result -= UltraCalcValue.OffsetFor1904DateSystem;
					correct1900Dates = false;
					allowZero = true;
				}


				// MRS 12/12/06 - BR18467
				// Excel incorrectly assumes that the year 1900 is a leap year
				// http://support.microsoft.com/kb/214326/en-us
				if ( correct1900Dates && dateValue < new DateTime( 1900, 3, 1 ) )
					result -= 1;

				if ( allowZero == false && result == 0 )
					return null;

				if ( result < 0 )
					return null;

				return result;
			}

			#endregion DateTimeToExcelDate
			
			// MD 8/12/08 - Excel formula solving
			#region ExcelDateToDateTime

			/// <summary>
			/// Converts Excel's numerical representation of a date to a DateTime.
			/// </summary>
			/// <remarks>
			/// <p class="body">
			/// When using the 1900 date system in Excel, dates before 3/1/1900 must be corrected, because Excel 
			/// incorrectly assumes 1900 is a leap year. This overload assumes the 1900 date system is being used
			/// as so it corrects the date values.
			/// </p>
			/// </remarks>
			/// <param name="excelDate">The Microsoft Excel date format which should be converted to a DateTime.</param>		
			// MD 4/15/09 - TFS16390
			// Made this public so the customer can have corresponding utilities methods since DateTimeToExcelDate 
			// is now exposed.
			//internal static DateTime? ExcelDateToDateTime(
			public static DateTime? ExcelDateToDateTime(

#pragma warning disable 1573
				Workbook workbook,
#pragma warning restore 1573

				double excelDate )
			{
				return UltraCalcValue.ExcelDateToDateTime(

					workbook,  

					excelDate, true );
			}

			// MD 4/15/09 - TFS16390
			// Added a new overload to correspond to the new overload of DateTimeToExcelDate.
			/// <summary>
			/// Converts Excel's numerical representation of a date to a DateTime.
			/// </summary>
			/// <param name="excelDate">The Microsoft Excel date format which should be converted to a DateTime.</param>
			/// <param name="shouldCorrect1900Dates">
			/// When using the 1900 date system in Excel, dates before 3/1/1900 must be corrected, because Excel 
			/// incorrectly assumes 1900 is a leap year. Pass False to disable this correction.
			/// </param>
			public static DateTime? ExcelDateToDateTime(

#pragma warning disable 1573
				Workbook workbook,
#pragma warning restore 1573

				double excelDate,
				bool shouldCorrect1900Dates )
			{
				// MD 2/14/12 - 12.1 - Table Support
				// Moved the code to a new overload of ExcelDateToDateTime.
				return UltraCalcValue.ExcelDateToDateTime(

				workbook,

				excelDate, shouldCorrect1900Dates, false);
			}

			// MD 2/14/12 - 12.1 - Table Support
			// Added a new overload which will allow us to get days before the Excel minimum date.
			internal static DateTime? ExcelDateToDateTime(

				Workbook workbook,

				double excelDate,
				bool shouldCorrect1900Dates,
				bool allowDateBeforeMinimum)
			{
				// MD 4/15/09 - TFS16390
				// Honor the new shouldCorrect1900Dates parameter.
				//bool correct1900Dates = true;
				bool correct1900Dates = shouldCorrect1900Dates;

				if ( workbook != null && workbook.DateSystem == DateSystem.From1904 )
				{
					excelDate += UltraCalcValue.OffsetFor1904DateSystem;
					correct1900Dates = false;
				}


				// MD 5/24/11 - TFS75560
				// If the value is too high, this will throw an exception. So catch that and return null.
				//DateTime dateValue = DateTime.FromOADate( excelDate );
				DateTime dateValue;
				try
				{
					dateValue = DateTime.FromOADate(excelDate);
				}
				catch
				{
					return null;
				}

				// MD 2/14/12 - 12.1 - Table Support
				// Correct dates before the minimum (but not before 12/30/1899), because we may need to return them now.
				//if ( correct1900Dates && dateValue < new DateTime( 1900, 3, 1 ) )
				if (correct1900Dates && dateValue < new DateTime(1900, 3, 1) && dateValue >= new DateTime(1899, 12, 31))
				{
					// Excel incorrectly assumes that the year 1900 is a leap year
					dateValue = dateValue.AddDays( 1 );
				}

				// MD 2/14/12 - 12.1 - Table Support
				// Only return null if allowDateBeforeMinimum is False.
				//if ( dateValue < new DateTime( 1900, 1, 1 ) )
				if (allowDateBeforeMinimum == false && dateValue < new DateTime(1900, 1, 1))
					return null;

				return dateValue;
			}

			#endregion ExcelDateToDateTime

			// MD 5/24/11 - TFS75560
			#region ExcelDateToTimeOfDay

			/// <summary>
			/// Converts Excel's numerical representation of a time of day to a TimeSpan.
			/// </summary>
			/// <remarks>
			/// <p class="body">
			/// In Excel, only the fractional portion of the date value represents the time of day, so if <paramref name="excelDate"/>
			/// is greater than 1, only the fractional part will be used.
			/// </p>
			/// </remarks>
			/// <param name="excelDate">The Microsoft Excel date format which should be converted to a DateTime.</param>	
			public static TimeSpan ExcelDateToTimeOfDay(double excelDate)
			{
				DateTime? dateTime = UltraCalcValue.ExcelDateToDateTime(

null,

 1000 + excelDate);

				if (dateTime.HasValue == false)
				{
					UltraCalcValue.DebugFail("Couldn't convert the date.");
					return TimeSpan.Zero;
				}

				return dateTime.Value.TimeOfDay;
			}

			#endregion  // ExcelDateToTimeOfDay

			// MD 5/24/11 - TFS75560
			#region TimeOfDayToExcelDate

			/// <summary>
			/// Converts a TimeSpan to Excel's numerical representation of a date.
			/// </summary>
			/// <remarks>
			/// <p class="body">
			/// The TimeSpan.Days portion of the <paramref name="timeValue"/> will not be used. Only the time of day will be used from the TimeSpan.
			/// </p>
			/// </remarks>
			/// <param name="timeValue">The TimeSpan value to convert to the Microsoft Excel date format.</param>
			public static double TimeOfDayToExcelDate(TimeSpan timeValue)
			{
				// MD 8/29/11 - TFS84895
				// There can be some round-off errors doing things this way. Use a different algorithm instead.
				//                DateTime baseDate = new DateTime(1900, 1, 1);
				//                baseDate.AddHours(timeValue.Hours);
				//                baseDate.AddMinutes(timeValue.Minutes);
				//                baseDate.AddSeconds(timeValue.Seconds);
				//                baseDate.AddMilliseconds(timeValue.Milliseconds);
				//
				//                double? excelDate = UltraCalcValue.DateTimeToExcelDate(
				//#if EXCEL
				//null,
				//#endif
				// DateTime.Today.Add(timeValue));
				//
				//                if (excelDate.HasValue == false)
				//                {
				//                    UltraCalcValue.DebugFail("Couldn't convert the date.");
				//                    return 0;
				//                }
				//
				//                return excelDate.Value % 1;
				return
					timeValue.Hours / 24d +
					timeValue.Minutes / 1440d +
					timeValue.Seconds / 86400d +
					timeValue.Milliseconds / 86400000d;
			}

			#endregion  // TimeOfDayToExcelDate

			// MD 5/24/11 - TFS75560
			#region DebugFail

			[Conditional("DEBUG")]
			private static void DebugFail(string message)
			{



				Debug.Fail(message);

			}

			#endregion  // DebugFail

			// MD 6/12/12
			// This is no longer needed.
			#region Removed

			//            // MD 11/30/11 - TFS96468
			//            // We may have a proxy type that represents an array, but creates the array lazily, so we need a helper method to get the array.
			//            #region GetRawArray

			//            private static UltraCalcValue[,] GetRawArray(object value)
			//            {
			//                UltraCalcValue[,] array = value as UltraCalcValue[,];
			//                if (array != null)
			//                    return array;
			//#if EXCEL
			//                ExternalRegionCalcReference.ExternalRegionValuesArray externalValuesArray =
			//                    value as ExternalRegionCalcReference.ExternalRegionValuesArray;

			//                if (externalValuesArray != null)
			//                    return externalValuesArray.ToArray();
			//#endif

			//                return null;
			//            }

			//            #endregion  // GetRawArray

			#endregion // Removed

			// MD 7/29/08 - Excel formula solving
			#region GetResolvedValue( bool )

			/// <summary>
			/// Gets the resolved value of the reference. This method will walk down the reference chain recursively to get the resolved value of the reference that is not just another reference. 
			/// </summary>
			/// <remarks>
			/// <p class="body">
			/// If this is being called to use the resolved value in a calculation, <paramref name="addDynamicReferences"/> should be True. Otherwise, if this is just being used to determine
			/// what type of resolved value is held, <paramref name="addDynamicReferences"/> should be False.
			/// </p>
			/// </remarks>
			/// <param name="addDynamicReferences">If True and a reference is held by this value, the reference will be added to the dynamic references of the formula being evaluated.</param>

			public  



				object GetResolvedValue( bool addDynamicReferences )
			{
				// Since this method is only being used to determine it's type, we don't need to throw an exception when an error 
				// occurs and we don't need to add the dynamic reference to the formula being solved, even though we are dereferencing
				// the value. It will get added later when the actual value is accessed.
				return this.GetResolvedValue( addDynamicReferences, false );
			}

			#endregion GetResolvedValue( bool )

			// MD 7/17/08 - Excel formula solving
			#region GetResolvedValue( bool, bool )



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			internal object GetResolvedValue( bool addDynamicReferences, bool raiseExceptionOnError )
			{
				// MD 12/1/11 - TFS96113
				// Moved code from the GetResolvedValue(bool, bool, bool) overload because that has been removed.
				#region Old Code

				//                bool ensureArraysCreated = false;

				//#if EXCEL
				//                if ( this.ExpectedTokenClass == TokenClass.Value )
				//                    ensureArraysCreated = true;
				//#endif

				//                return this.GetResolvedValue( addDynamicReferences, raiseExceptionOnError, ensureArraysCreated );

				#endregion // Old Code

				// MD 9/2/08 - Excel formula solving
				// If the parameter is never dereferenced, we never want to add dynamic references.
				if (addDynamicReferences && this.WillParameterAlwaysBeDereferenced == false)
					addDynamicReferences = false;


				return this.GetResolvedValue(this, addDynamicReferences, raiseExceptionOnError);
			}

			#endregion GetResolvedValue( bool, bool )

			// MD 12/1/11 - TFS96113
			// This has been removed because the ensureArraysCreated parameter is not used anymore.
			#region Removed

			//            // MD 7/17/08 - Excel formula solving
			//            #region GetResolvedValue( bool, bool, bool )

			//#if DEBUG
			//            /// <summary>
			//            /// Gets the resolved, dereferenced value from this value holder. This will also selected the correct value from a stored
			//            /// array in array formulas of cells.
			//            /// </summary>
			//            /// <param name="addDynamicReferences">
			//            /// Only applies if this value is a dynamic reference. True to add the dynamic reference represented by this calc value to the 
			//            /// formula being solved when the value is dereferenced.
			//            /// </param>
			//            /// <param name="raiseExceptionOnError">True to raise an exception if the single value resolves to an error value.</param>
			//            /// <param name="ensureArraysCreated">True to make sure range references create the cells they contain.</param>
			//#endif
			//            internal object GetResolvedValue( bool addDynamicReferences, bool raiseExceptionOnError, bool ensureArraysCreated )
			//            {
			//#if EXCEL
			//                // MD 9/2/08 - Excel formula solving
			//                // If the parameter is never dereferenced, we never want to add dynamic references.
			//                if ( addDynamicReferences && this.WillParameterAlwaysBeDereferenced == false )
			//                    addDynamicReferences = false; 
			//#endif

			//                return this.GetResolvedValue( this, addDynamicReferences, raiseExceptionOnError, ensureArraysCreated );
			//            }

			//            #endregion GetResolvedValue( bool, bool, bool  )

			#endregion // Removed

			// MD 9/3/08 - Excel formula solving
			#region GetResolvedValue( UltraCalcValue, bool, bool )



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			// MD 12/1/11 - TFS96113
			// Removed the ensureArraysCreated parameter because it is not used anymore.
			//private object GetResolvedValue( UltraCalcValue originalValue, bool addDynamicReferences, bool raiseExceptionOnError, bool ensureArraysCreated )
			private object GetResolvedValue(UltraCalcValue originalValue, bool addDynamicReferences, bool raiseExceptionOnError)
			{

				// MD 4/10/12 - TFS108678
				// Moved all code to a new overload.
				return this.GetResolvedValue(originalValue, addDynamicReferences, true, raiseExceptionOnError);
			}

			// MD 4/10/12 - TFS108678
			// Moved all code to a new overload.
			private object GetResolvedValue(UltraCalcValue originalValue, bool addDynamicReferences, bool splitArraysForValueParameters, bool raiseExceptionOnError)
			{

				// The resolved value should recursively dereference the value if it is a reference. Don't use IsReference to check 
				// if it is a reference because that method calls GetResolvedValue to make sure the reference can be used in an array
				// formula and we will get a stack overflow. Plus, we want to get in here even if the reference can't be used in an 
				// array formula, so IsReference is not a good test.
				if ( this.value is IUltraCalcReference )
				{
					// Get the resolved reference in the value.
					UltraCalcErrorValue errorValue;

					// MD 4/10/12 - TFS108678
					//IUltraCalcReference reference = this.ToReference( addDynamicReferences, out errorValue );
					IUltraCalcReference reference = this.ToReference(addDynamicReferences,

						splitArraysForValueParameters,

						out errorValue);

					// If there was an error getting the reference, return the error value that occurred.
					if ( reference == null )
					{
						Debug.Assert( errorValue != null, "If a reference was not returned, the error value should be non null." );
						return errorValue;
					}

					// MD 12/1/11 - TFS96113
					// This is no longer needed now that we are using ArrayProxy instances for arrays.
					#region Removed

					//#if EXCEL
					//                    if ( ensureArraysCreated )
					//                    {
					//                        ExcelRefBase excelReference = UltraCalcEngine.GetResolvedReference( reference ) as ExcelRefBase;
					//
					//                        if ( excelReference != null )
					//                            excelReference.EnsureArrayValuesCreated();
					//                    }
					//#endif

					#endregion // Removed

					// Dereference the value and from the reference.
					UltraCalcValue dereferencedValue = reference.Value;

					if ( dereferencedValue == null )
						return null;

					// If a valid value was returned, recursively resolve the dereferenced value.
					// MD 12/1/11 - TFS96113
					// Removed the ensureArraysCreated parameter because it is not used anymore.
					//return dereferencedValue.GetResolvedValue( originalValue, addDynamicReferences, raiseExceptionOnError, ensureArraysCreated );
					// MD 4/10/12 - TFS108678
					// Pass along the new splitArraysForValueParameters parameter.
					//return dereferencedValue.GetResolvedValue(originalValue, addDynamicReferences, raiseExceptionOnError);
					return dereferencedValue.GetResolvedValue(originalValue, addDynamicReferences,

						splitArraysForValueParameters, 

						raiseExceptionOnError);
				}

				object returnValue = this.value;


				// MD 4/10/12 - TFS108678
				// This is no longer true because we may get the resolved values before starting the function evaluation.
				//Debug.Assert(
				//    this.Owner == null || this.ExpectedTokenClass != (TokenClass)0,
				//    "The token class has not been initialized for the parameter." );

				// Any parameter expecting a value type might need to have the value changed (if it is an array or group of arrays).
				// MD 4/10/12 - TFS108678
				// Only split up an array when splitArraysForValueParameters is True.
				//if ( this.ExpectedTokenClass == TokenClass.Value )
				if (splitArraysForValueParameters && this.ExpectedTokenClass == TokenClass.Value)
				{
					// MD 11/30/11 - TFS96468
					// We may have a proxy type that represents an array, but creates the array lazily, so use a helper method to get the array.
					//UltraCalcValue[ , ] array = this.value as UltraCalcValue[ , ];
					// MD 12/1/11 - TFS96113
					// We now use ArrayProxy instances to represent arrays.
					//UltraCalcValue[,] array = UltraCalcValue.GetRawArray(this.value);
					ArrayProxy arrayProxy = this.value as ArrayProxy;

					// If the value is an array of values, try to get a single value from the array.
					// MD 12/1/11 - TFS96113
					// Rewrote this code to work with ArrayProxy instances instead of arrays.
					#region Old Code

					//if ( array != null )
					//{
					//    returnValue = this.GetSingleValue( originalValue, array, raiseExceptionOnError, addDynamicReferences, ensureArraysCreated );
					//}
					//else
					//{
					//    UltraCalcValue[][ , ] arrayGroup = this.value as UltraCalcValue[][ , ];
					//
					//    // If the value is a group of arrays, we can only get the value if it is one array. If so, again, try to get 
					//    // a single value from the array. If not, return a #VALUE! error.
					//    if ( arrayGroup != null )
					//    {
					//        if ( arrayGroup.Length != 1 )
					//            returnValue = new UltraCalcErrorValue( ExcelCalcErrorCode.Value );
					//        else
					//            returnValue = this.GetSingleValue( originalValue, arrayGroup[ 0 ], raiseExceptionOnError, addDynamicReferences, ensureArraysCreated );
					//    }
					//}

					#endregion // Old Code
					if (arrayProxy != null)
					{
						returnValue = this.GetSingleValue(originalValue, arrayProxy, raiseExceptionOnError, addDynamicReferences);
					}
					else
					{
						ArrayProxy[] arrayProxyGroup = this.value as ArrayProxy[];

						// If the value is a group of arrays, we can only get the value if it is one array. If so, again, try to get 
						// a single value from the array. If not, return a #VALUE! error.
						if (arrayProxyGroup != null)
						{
							if (arrayProxyGroup.Length != 1)
								returnValue = new UltraCalcErrorValue(ExcelCalcErrorCode.Value);
							else
								returnValue = this.GetSingleValue(originalValue, arrayProxyGroup[0], raiseExceptionOnError, addDynamicReferences);
						}
					}
				}

				// MD 11/29/10
				// Infinity values should be considered #NUM! errors.
				if (value is double && Double.IsInfinity((double)value))
				{
					returnValue = new UltraCalcErrorValue(ExcelCalcErrorCode.Num); 
				}


				// If an exception should be raised and an error value was resolved, have the error value raise it's exception.
				if ( raiseExceptionOnError )
				{
					UltraCalcErrorValue errorValue = returnValue as UltraCalcErrorValue;

					if ( errorValue != null )
						errorValue.RaiseException();
				}

				return returnValue;
			}

			#endregion GetResolvedValue( UltraCalcValue, bool, bool )

			// MD 7/18/08 - Excel formula solving
			#region UseExcelFormulaCompatibility

			// SSP 8/22/11 - XamCalculationManager
			// We decided to make the functions excel compatible in XAML and also not expose this flag.
			// 




			internal const bool UseExcelValueCompatibility = true;
			internal const bool UseExcelFunctionCompatibility = true;


#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)




			#endregion UseExcelFormulaCompatibility

			// MD 4/6/12 - TFS101506
			#region Culture

			internal CultureInfo Culture
			{
				get
				{
					if (UltraCalcValue.UseExcelValueCompatibility)
					{

						if (this.Owner != null)
							return this.Owner.Culture;

						return CultureInfo.CurrentCulture;
					}






				}
			}

			#endregion // Culture


			// MD 7/14/08 - Excel formula solving
			#region CopyExpectedTokenClass

			// MD 12/1/11 - TFS96113
			// These methods have been removed. The one for array groups has been replaced below and the one for arrays is no longer needed because 
			// the ArrayProxy now takes the ExpectedTokenClass and sets it on all values before returning them from the indexer.
			#region Removed

			//#if DEBUG
			//            /// <summary>
			//            /// Copies the ExpectedTokenClass of this value to each value stored in the specified array group.
			//            /// </summary>
			//#endif
			//            private void CopyExpectedTokenClass( UltraCalcValue[][ , ] arrayGroup )
			//            {
			//                foreach ( UltraCalcValue[ , ] array in arrayGroup )
			//                    this.CopyExpectedTokenClass( array );
			//            }

			//#if DEBUG
			//            /// <summary>
			//            /// Copies the ExpectedTokenClass of this value to each value stored in the specified array.
			//            /// </summary>
			//#endif
			//            private void CopyExpectedTokenClass( UltraCalcValue[ , ] array )
			//            {
			//                foreach ( UltraCalcValue value in array )
			//                {
			//                    if ( value != null )
			//                        value.ExpectedTokenClass = this.ExpectedTokenClass;
			//                }
			//            }

			#endregion // Removed
			private void CopyExpectedTokenClass(ArrayProxy[] arrayProxyGroup)
			{
				foreach (ArrayProxy arrayProxy in arrayProxyGroup)
				{
					// MD 1/11/12 - TFS99215
					// There are other values that need to be copied over, so instead of copying them all here, set the owning value and the proper 
					// state will be accessed when it is needed.
					//arrayProxy.ExpectedTokenClass = this.ExpectedTokenClass;
					arrayProxy.OwningValue = this;
				}
			}

			#endregion CopyExpectedTokenClass

			#region ExpectedTokenClass

			private TokenClass expectedTokenClass;







			internal TokenClass ExpectedTokenClass
			{
				get { return this.expectedTokenClass; }
				set { this.expectedTokenClass = value; }
			}

			#endregion ExpectedTokenClass

			#region FormulaOwner






			private IUltraCalcReference FormulaOwner
			{
				get
				{
					if ( this.Owner == null )
						return null;

					return this.Owner.FormulaOwner;
				}
			} 

			#endregion FormulaOwner

			#region GetSingleValue



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			// MD 12/1/11 - TFS96113
			// Changed the 2nd parameter to be an ArrayProxy instead of an array and removed the ensureArraysCreated parameter because it is 
			// never used.
			//private object GetSingleValue( UltraCalcValue originalValue, UltraCalcValue[ , ] array, bool raiseExceptionOnError, bool addDynamicReferences, bool ensureArraysCreated )
			private object GetSingleValue(UltraCalcValue originalValue, ArrayProxy arrayProxy, bool raiseExceptionOnError, bool addDynamicReferences)
			{
				// MD 12/1/11 - TFS96113
				//int columnCount = array.GetLength( 0 );
				//int rowCount = array.GetLength( 1 );
				int columnCount = arrayProxy.GetLength(0);
				int rowCount = arrayProxy.GetLength(1);

				// If either dimension of the array is empty, return a #VALUE! error.
				if ( columnCount == 0 || rowCount == 0 )
					return new ExcelCalcErrorValue( ExcelCalcErrorCode.Value );
				
				ExcelCalcValue singleValue;

				// Determine whether this value is being evaluated in an array formula and also get a reference to the cell
				// owning the formula. If it is an array formula, we will need it to determine it's relative position in the
				// array formula's target region.
				CellCalcReference cellFormulaOwner = this.FormulaOwner as CellCalcReference;

				if ( cellFormulaOwner != null && cellFormulaOwner.HasArrayFormula )
				{
					// MD 12/1/11 - TFS96113
					//singleValue = CalcUtilities.SplitArrayForArrayFormula( array, cellFormulaOwner );
					singleValue = CalcUtilities.SplitArrayForArrayFormula(arrayProxy, cellFormulaOwner);

					if ( singleValue == null )
						return new ExcelCalcErrorValue( ExcelCalcErrorCode.NA );
				}
				// If it is not an array formula and the array is a range reference and has multiple rows or columns...
				else if ( ( 1 < columnCount || 1 < rowCount ) && originalValue.IsReference )
				{
					singleValue = null;
					bool canBeSplit = false;

					// And one of the dimensions of the range is 1, the reference might be able to be split up, depending on where the cell owner of the formula is.
					if ( ( columnCount == 1 || rowCount == 1 ) &&
						cellFormulaOwner != null )
					{
						// MD 2/24/12 - 12.1 - Table Support
						//WorksheetRegion[] regions = CalcUtilities.GetRegionGroup( originalValue.ToReference().Context );
						//
						//// If the reference is a single range the range might be able to be split up.
						//if ( regions.Length == 1 )
						IList<WorksheetRegion> regions = CalcUtilities.GetRegionGroup(originalValue.ToReference());

						// If the reference is a single range the range might be able to be split up.
						if (regions != null && regions.Count == 1)
						{
							WorksheetRegion region = regions[ 0 ];

							// The region can be split up if it has one column and the cell owning the fomrula is in one of the same rows as the range reference.
							// If that is the case, the value used is the cell reference in the same row as the cell owning the formula.
							if ( columnCount == 1 )
							{
								// MD 4/12/11 - TFS67084
								// Moved away from using WorksheetCell objects.
								//int rowIndex = cellFormulaOwner.Cell.RowIndex;
								int rowIndex = cellFormulaOwner.Row.Index;

								if ( region.FirstRow <= rowIndex && rowIndex <= region.LastRow )
								{
									// MD 2/4/11
									// Found while fixing TFS65015
									// When it is a single column region, we need to ignore the region's position and just index into the 0th column.
									// Otherwise, we're going to get an exception.
									//singleValue = array[ region.FirstColumn, rowIndex - region.FirstRow ];
									// MD 12/1/11 - TFS96113
									//singleValue = array[0, rowIndex - region.FirstRow];
									singleValue = arrayProxy[0, rowIndex - region.FirstRow];

									canBeSplit = true;
								}
							}

							// Also, the region can be split up if it has one row and the cell owning the fomrula is in one of the same columns as the range reference.
							// If that is the case, the value used is the cell reference in the same column as the cell owning the formula.
							if ( rowCount == 1 )
							{
								// MD 4/12/11 - TFS67084
								// Moved away from using WorksheetCell objects.
								//int columnIndex = cellFormulaOwner.Cell.ColumnIndex;
								int columnIndex = cellFormulaOwner.ColumnIndex;

								if ( region.FirstColumn <= columnIndex && columnIndex <= region.LastColumn )
								{
									// MD 2/4/11
									// Found while fixing TFS65015
									// When it is a single row region, we need to ignore the region's position and just index into the 0th row.
									// Otherwise, we're going to get an exception.
									//singleValue = array[ columnIndex - region.FirstColumn, region.FirstRow ];
									// MD 12/1/11 - TFS96113
									//singleValue = array[columnIndex - region.FirstColumn, 0];
									singleValue = arrayProxy[columnIndex - region.FirstColumn, 0];

									canBeSplit = true;
								}
							}
						}
					}

					if ( canBeSplit == false )
						return new ExcelCalcErrorValue( ExcelCalcErrorCode.Value );
				}
				else
				{
					// Otherwise, use the first value in the array.
					// MD 12/1/11 - TFS96113
					//singleValue = array[ 0, 0 ];
					singleValue = arrayProxy[0, 0];
				}
				

				// Copy the ExpectedTokenClass from this value to the single value.
				singleValue.ExpectedTokenClass = this.ExpectedTokenClass;

				// Get the resolved value from the single value, which could be a reference.
				// MD 12/1/11 - TFS96113
				// Removed the ensureArraysCreated parameter because it is not used anymore.
				//return singleValue.GetResolvedValue( addDynamicReferences, raiseExceptionOnError, ensureArraysCreated );
				return singleValue.GetResolvedValue(addDynamicReferences, raiseExceptionOnError);
			}

			#endregion GetSingleValue

			#region IsArray

			/// <summary>
			/// Returns whether this class instance contains an array value.
			/// </summary>
			/// <returns>True if this instance class contains an array, else false.</returns>
			/// <seealso cref="ToArrayProxy"/>
			public bool IsArray
			{
				get
				{
					// MD 12/1/11 - TFS96113
					// Instead of storing arrays for rectangular regions of values we now use an ArrayProxy.
					#region Old Code

					//                    // MD 11/30/11 - TFS96468
					//                    // We may have a proxy type that represents an array, but creates the array lazily, so check for that as well.
					//                    //return this.GetResolvedValue( false ) is ExcelCalcValue[ , ];
					//                    object resolvedValue = this.GetResolvedValue(false);
					//                    if (resolvedValue is ExcelCalcValue[,])
					//                        return true;

					//#if EXCEL
					//                    if (resolvedValue is ExternalRegionCalcReference.ExternalRegionValuesArray)
					//                        return true;
					//#endif

					//                    return false;

					#endregion // Old Code
					return this.GetResolvedValue(false) is ArrayProxy;
				}
			}

			#endregion IsArray

			#region IsArrayGroup

			/// <summary>
			/// Returns whether this class instance contains a group of array values.
			/// </summary>
			/// <returns>True if this instance class contains an array group, else false.</returns>
			/// <seealso cref="ToArrayProxyGroup"/>
			public bool IsArrayGroup
			{
				get
				{
					// MD 12/1/11 - TFS96113
					// Instead of storing an array of arrays for groups of rectangular regions of values we now use an array of ArrayProxy instances.
					//return this.GetResolvedValue( false ) is ExcelCalcValue[][ , ];
					return this.GetResolvedValue( false ) is ArrayProxy[];
				}
			}

			#endregion IsArrayGroup

			// MD 4/10/12 - TFS108678
			#region IsArrayRaw

			internal bool IsArrayRaw
			{
				get
				{
					return this.GetResolvedValue(this, false, false, false) is ArrayProxy;
				}
			}

			#endregion IsArrayRaw

			#region IsDynamicReference

			private bool isDynamicReference;






			internal bool IsDynamicReference
			{
				get { return this.isDynamicReference; }
				set 
				{
					Debug.Assert( value == false || this.value is IUltraCalcReference, "Only a reference can be marked as a dynamic reference." );
					this.isDynamicReference = value; 
				}
			} 

			#endregion IsDynamicReference

			#region Owner

			private UltraCalcNumberStack owner;






			internal UltraCalcNumberStack Owner
			{
				get { return this.owner; }
				set { this.owner = value; }
			} 

			#endregion Owner

			#region ToArray

			/// <summary>
			/// Convert this class instance's value to an array data type
			/// </summary>
			/// <returns>An array equivalent to the value of this instance</returns>
			// MD 12/1/11 - TFS96113
			// Marked this property obsolete because it is more performant to use ToArrayProxy
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("ToArray is deprecated. Use ToArrayProxy instead for better performance.")]
			public UltraCalcValue[ , ] ToArray()
			{
				// MD 12/1/11 - TFS96113
				// Removed this code and delegated off to ToArrayProxy.
				#region Old Code

				//object resolvedValue = this.GetResolvedValue( true, false, true );

				//// MD 11/30/11 - TFS96468
				//// We may have a proxy type that represents an array, but creates the array lazily, so use a helper method to get the array.
				////UltraCalcValue[ , ] array = resolvedValue as UltraCalcValue[ , ];
				//UltraCalcValue[,] array = UltraCalcValue.GetRawArray(resolvedValue);

				//if ( array == null )
				//{
				//    UltraCalcValue[][ , ] arrayGroup = resolvedValue as UltraCalcValue[][ , ];

				//    // If the value is a group of arrays and there is only one array in the group, return that array. 
				//    // Otherwise, return a #VALUE! error.
				//    if ( arrayGroup != null )
				//    {
				//        if ( arrayGroup.Length != 1 )
				//            return new UltraCalcValue[ , ] { { new UltraCalcValue( new UltraCalcErrorValue( ExcelCalcErrorCode.Value ) ) } };

				//        array = arrayGroup[ 0 ];
				//    }
				//    else
				//    {
				//        // If the value is just a single value, wrap it in an array. Single values can always be converted to arrays.
				//        array = new UltraCalcValue[ , ] { { this } };
				//    }
				//}

				//// Copy over the ExpectedTokenClass to all values in the array before returning it.
				//this.CopyExpectedTokenClass( array );

				//return array;

				#endregion // Old Code
				return this.ToArrayProxy().ToArray();
			}

			#endregion ToArray

			// MD 12/1/11 - TFS96113
			#region ToArrayProxy

			/// <summary>
			/// Convert this class instance's value to an <see cref="ArrayProxy"/> data type.
			/// </summary>
			/// <returns>An array equivalent to the value of this instance.</returns>
			/// <seealso cref="ArrayProxy"/>
			public ArrayProxy ToArrayProxy()
			{
				object resolvedValue = this.GetResolvedValue(true, false);

				ArrayProxy arrayProxy = resolvedValue as ArrayProxy;

				if (arrayProxy == null)
				{
					ArrayProxy[] arrayProxyGroup = resolvedValue as ArrayProxy[];

					// If the value is a group of arrays and there is only one array in the group, return that array. 
					// Otherwise, return a #VALUE! error.
					if (arrayProxyGroup != null)
					{
						if (arrayProxyGroup.Length != 1)
							return new CLRArrayProxy(new UltraCalcValue(new UltraCalcErrorValue(ExcelCalcErrorCode.Value)));

						arrayProxy = arrayProxyGroup[0];
					}
					else
					{
						// If the value is just a single value, wrap it in an array. Single values can always be converted to arrays.
						arrayProxy = new CLRArrayProxy(this);
					}
				}

				// MD 1/11/12 - TFS99215
				// There are other values that need to be copied over, so instead of copying them all here, set the owning value and the proper 
				// state will be accessed when it is needed.
				//// Copy over the ExpectedTokenClass to all values in the array before returning it.
				//arrayProxy.ExpectedTokenClass = this.ExpectedTokenClass;
				arrayProxy.OwningValue = this;

				return arrayProxy;
			}

			#endregion ToArrayProxy

			// MD 4/10/12 - TFS108678
			#region ToArrayProxyRaw

			internal ArrayProxy ToArrayProxyRaw()
			{
				ArrayProxy arrayProxy = this.GetResolvedValue(this, false, false, false) as ArrayProxy;
				Debug.Assert(arrayProxy != null, "This should only be called when we know we have an array.");
				return arrayProxy;
			}

			#endregion // ToArrayProxyRaw

			#region ToArrayGroup

			/// <summary>
			/// Convert this class instance's value to an array group data type
			/// </summary>
			/// <returns>An array group equivalent to the value of this instance</returns>
			// MD 12/1/11 - TFS96113
			// Marked this property obsolete because it is more performant to use ToArrayProxy
			[EditorBrowsable(EditorBrowsableState.Never)]
			[Obsolete("ToArrayGroup is deprecated. Use ToArrayProxyGroup instead for better performance.")]
			public UltraCalcValue[][ , ] ToArrayGroup()
			{
				// MD 12/1/11 - TFS96113
				// Removed this code and delegated off to ToArrayProxyGroup.
				#region Old Code

				//object resolvedValue = this.GetResolvedValue( true, false, true );

				//UltraCalcValue[][ , ] arrayGroup = resolvedValue as UltraCalcValue[][ , ];

				//if ( arrayGroup == null )
				//{
				//    // MD 11/30/11 - TFS96468
				//    // We may have a proxy type that represents an array, but creates the array lazily, so use a helper method to get the array.
				//    //UltraCalcValue[ , ] array = resolvedValue as UltraCalcValue[ , ];
				//    UltraCalcValue[,] array = UltraCalcValue.GetRawArray(resolvedValue);

				//    // If the value is just an array or just a single value, wrap it in a group of arrays and return that.
				//    if ( array != null )
				//        arrayGroup = new UltraCalcValue[][ , ] { array };
				//    else
				//        arrayGroup = new UltraCalcValue[][ , ] { new UltraCalcValue[ , ] { { this } } };
				//}

				//// Copy over the ExpectedTokenClass to all values in the array group before returning it.
				//this.CopyExpectedTokenClass( arrayGroup );

				//return arrayGroup;

				#endregion // Old Code
				ArrayProxy[] arrayProxyGroup = this.ToArrayProxyGroup();
				UltraCalcValue[][,] arrayGroup = new UltraCalcValue[arrayProxyGroup.Length][,];
				for (int i = 0; i < arrayProxyGroup.Length; i++)
					arrayGroup[i] = arrayProxyGroup[i].ToArray();

				return arrayGroup;
			}

			#endregion ToArrayGroup

			// MD 12/1/11 - TFS96113
			#region ToArrayProxyGroup

			/// <summary>
			/// Convert this class instance's value to an array group data type.
			/// </summary>
			/// <returns>An array group equivalent to the value of this instance.</returns>
			/// <seealso cref="ArrayProxy"/>
			public ArrayProxy[] ToArrayProxyGroup()
			{
				object resolvedValue = this.GetResolvedValue(true, false);

				ArrayProxy[] arrayProxyGroup = resolvedValue as ArrayProxy[];

				if (arrayProxyGroup == null)
				{
					ArrayProxy arrayProxy = resolvedValue as ArrayProxy;

					// If the value is just an array or just a single value, wrap it in a group of arrays and return that.
					if (arrayProxy != null)
						arrayProxyGroup = new ArrayProxy[] { arrayProxy };
					else
						arrayProxyGroup = new ArrayProxy[] { new CLRArrayProxy(this) };
				}

				// Copy over the ExpectedTokenClass to all values in the array group before returning it.
				this.CopyExpectedTokenClass(arrayProxyGroup);

				return arrayProxyGroup;
			}

			#endregion // ToArrayProxyGroup

			#region WillParameterAlwaysBeDereferenced

			private bool willParameterAlwaysBeDereferenced = true;

			internal bool WillParameterAlwaysBeDereferenced
			{
				get { return this.willParameterAlwaysBeDereferenced; }
				// MD 1/11/12 - TFS99215
				// If a parameter will not always be dereferenced, it will be a dynamic reference if and when it is dereferenced.
				//set { this.willParameterAlwaysBeDereferenced = value; }
				set 
				{
					if (this.willParameterAlwaysBeDereferenced == value)
						return;

					this.willParameterAlwaysBeDereferenced = value;

					if (this.willParameterAlwaysBeDereferenced == false && this.IsReference)
						this.IsDynamicReference = true;
				}
			}

			#endregion WillParameterAlwaysBeDereferenced

			#region Workbook

			/// <summary>
			/// Gets the workbook associated with the formula being solved.
			/// </summary>
			// MD 5/24/11 - TFS75560
			//private Workbook Workbook
			internal Workbook Workbook
			{
				get
				{
					ExcelRefBase excelRef = ExcelCalcEngine.GetResolvedReference( this.FormulaOwner ) as ExcelRefBase;

					if ( excelRef == null )
						return null;

					return excelRef.Workbook;
				}
			} 

			#endregion Workbook

		}
	#endregion // UltraCalcValue.cs


	// MD 12/1/11 - TFS96113
	#region ArrayProxy class

	/// <summary>
	/// Represents a two-dimensional array of <see cref="ExcelCalcValue"/> instances used in calculations.
	/// </summary>



	public

		abstract class ArrayProxy :
		// MD 4/10/12
		// Found while fixing TFS108678
		// We should have made this an enumerable class.
		IEnumerable<ExcelCalcValue>
	{
		#region Member Variables

		// MD 1/11/12 - TFS99215
		// There are other values that need to be copied over, so instead of storing them all here, store the owning value and the proper 
		// state will be copied when it is needed.
		//private ExcelCalcValue owningValue;
		private ExcelCalcValue owningValue;

		#endregion  // Member Variables

		#region Interfaces

		// MD 4/10/12
		// Found while fixing TFS108678
		// We should have made this an enumerable class.
		#region IEnumerable<ExcelCalcValue> Members

		IEnumerator<UltraCalcValue> IEnumerable<UltraCalcValue>.GetEnumerator()
		{
			int width = this.GetLength(0);
			int height = this.GetLength(1);

			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
					yield return this[i, j];
			}
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<UltraCalcValue>)this).GetEnumerator();
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region Public Methods

		#region GetLength

		/// <summary>
		/// Gets the number of elements in the specified dimension of the array.
		/// </summary>
		/// <param name="dimension">The zero-based index of the dimension.</param>
		/// <returns>The number of elements in the specified dimension.</returns>
		public abstract int GetLength(int dimension);

		#endregion  // GetLength

		#endregion  // Public Methods

		#region Internal Methods

		internal abstract ExcelCalcValue GetValue(int x, int y);
		internal abstract ExcelCalcValue[,] ToArray();

		#region GetIterator



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		// MD 1/11/12 - TFS99215
		// Made this into a helper method so we can copy over some state to the component values before they are returned.
		//internal abstract IEnumerable<KeyValuePair<int, ExcelCalcValue>> GetIterator(int dimension, int index);
		internal IEnumerable<KeyValuePair<int, ExcelCalcValue>> GetIterator(int dimension, int index)
		{
			foreach (KeyValuePair<int, ExcelCalcValue> pair in this.GetIteratorHelper(dimension, index))
			{
				this.CopyStateToContainedValue(pair.Value);
				yield return pair;
			}
		}
		internal abstract IEnumerable<KeyValuePair<int, ExcelCalcValue>> GetIteratorHelper(int dimension, int index);

		#endregion  // GetIterator

		// MD 7/5/12 - TFS112278
		#region IterateValues

		internal delegate void ValueCallback(UltraCalcValue currentTestValue, int relativeRowIndex, int relativeColumnIndex);
		internal virtual void IterateValues(bool shouldIterateBlanks, ValueCallback valueCallback)
		{
			int length0 = this.GetLength(0);
			int length1 = this.GetLength(1);

			for (int columnIndex = 0; columnIndex < length0; columnIndex++)
			{
				for (int rowIndex = 0; rowIndex < length1; rowIndex++)
					valueCallback(this[columnIndex, rowIndex], rowIndex, columnIndex);
			}
		}

		#endregion // IterateValues

		#region ThrowOutOfBoundsException

		internal void ThrowOutOfBoundsException()
		{
			throw new IndexOutOfRangeException(SR.GetString("LE_IndexOutOfRangeException_ArrayBounds"));
		}

		#endregion  // ThrowOutOfBoundsException

		#endregion  // Internal Methods

		#region Private Methods

		// MD 1/11/12 - TFS99215
		#region CopyStateToContainedValue

		private void CopyStateToContainedValue(ExcelCalcValue value)
		{
			if (this.owningValue == null)
				return;

			value.ExpectedTokenClass = this.owningValue.ExpectedTokenClass;
			value.Owner = this.owningValue.Owner;

			
			
			

			if (this.ContainsReferences)
				value.IsDynamicReference = this.owningValue.IsDynamicReference;
		}

		#endregion  // CopyStateToContainedValue

		#endregion  // Private Methods 

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region Indexer[int,int]

		/// <summary>
		/// Gets the <see cref="ExcelCalcValue"/> at the specified index.
		/// </summary>
		/// <param name="x">The index for the first dimension of the array. This is the relative column index for arrays representing cell regions on a worksheet.</param>
		/// <param name="y">The index for the second dimension of the array. This is the relative row index for arrays representing cell regions on a worksheet.</param>
		/// <returns>An <see cref="ExcelCalcValue"/> representing the specified value in the array.</returns>
		public ExcelCalcValue this[int x, int y]
		{
			get
			{
				ExcelCalcValue value = this.GetValue(x, y);

				// MD 1/11/12 - TFS99215
				// There are other state values which need to be copied, so call a helper method to do it.
				//value.ExpectedTokenClass = this.expectedTokenClass;
				this.CopyStateToContainedValue(value);

				return value;
			}
		}

		#endregion  // Indexer[int,int]

		#endregion  // Public Properties

		#region Internal Properties

		// MD 1/11/12 - TFS99215
		#region ContainsReferences

		internal virtual bool ContainsReferences
		{
			get { return false; }
		}

		#endregion  // ContainsReferences

		// MD 1/11/12 - TFS99215
		// There are other values that need to be copied over, so instead of storing them all here, store the owning value and the proper 
		// state will be copied when it is needed.
		//internal TokenClass ExpectedTokenClass
		//{
		//    get { return this.expectedTokenClass; }
		//    set { this.expectedTokenClass = value; }
		//}

		// MD 1/11/12 - TFS99215
		#region OwningValue

		internal ExcelCalcValue OwningValue
		{
			get { return this.owningValue; }
			set { this.owningValue = value; }
		}

		#endregion  // OwningValue

		#endregion  // Internal Properties

		#endregion  // Properties
	}

	#endregion // ArrayProxy class

	// MD 12/1/11 - TFS96113
	#region CLRArrayProxy class

	internal class CLRArrayProxy : ArrayProxy
	{
		private ExcelCalcValue[,] array;

		public CLRArrayProxy(ExcelCalcValue singleValue)
		{
			this.array = new ExcelCalcValue[,] { { singleValue } };
		}

		public CLRArrayProxy(ExcelCalcValue[,] array)
		{
			this.array = array;
		}

		internal override IEnumerable<KeyValuePair<int, UltraCalcValue>> GetIteratorHelper(int dimension, int index)
		{
			switch (dimension)
			{
				case 0:
					{
						int length = this.GetLength(1);
						for (int i = 0; i < length; i++)
							yield return new KeyValuePair<int, UltraCalcValue>(i, this.array[index, i]);
					}
					break;

				case 1:
					{
						int length = this.GetLength(0);
						for (int i = 0; i < length; i++)
							yield return new KeyValuePair<int, UltraCalcValue>(i, this.array[i, index]);
					}
					break;

				default:
					this.ThrowOutOfBoundsException();
					break;
			}
		}

		public override int GetLength(int dimension)
		{
			return this.array.GetLength(dimension);
		}

		internal override ExcelCalcValue GetValue(int x, int y)
		{
			return this.array[x, y];
		}

		internal override ExcelCalcValue[,] ToArray()
		{
			return array;
		}
	}

	#endregion // CLRArrayProxy class


	#region IUltraCalcFormula Interface

	/// <summary>
	/// Interface implemented by the formula object.
	/// </summary>






	public

		 interface IExcelCalcFormula



	{
		/// <summary>
		/// Formula string.
		/// </summary>
		string FormulaString { get; }








		/// <summary>
		/// Returns whether the formula contains an always dirty function.
		/// </summary>
		bool HasAlwaysDirty { get; }








		/// <summary>
		/// Base reference associated with the formula.
		/// </summary>
		/// <remarks>
		/// <p class="body">Base reference associated with the formula. For example, in the case of a column formula, BaseReference would be the column reference.</p>
		/// </remarks>
		IUltraCalcReference BaseReference { get; }

		/// <summary>
		/// Evaluate the compiled expression against the given base reference
		/// </summary>
		/// <param name="reference">Base reference used to resolve relative references into absolute references</param>
		/// <returns>Retuns an <see cref="UltraCalcValue"/> containing result of formula evaluation</returns>
		UltraCalcValue Evaluate(IUltraCalcReference reference);

		/// <summary>
		/// Retuns a collection of references contained in the formula token string
		/// </summary>
		/// <returns>Collection of <see cref="IUltraCalcReference"/>s</returns>
		IUltraCalcReferenceCollection References { get; }

		// MD 7/25/08 - Excel formula solving

		/// <summary>
		/// Adds a reference created during the evaluation of the formula to the refernces of the formula.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the reference is already in the references or dynamic references collection of the formula, this will have no effect.
		/// </p>
		/// </remarks>
		/// <param name="reference">The reference to add to the formula's dynamic references collection.</param>
		bool AddDynamicReference( IUltraCalcReference reference );

		/// <summary>
		/// Gets the collection of reference created during the last evaluation of the formula.
		/// </summary>
		IUltraCalcReferenceCollection DynamicReferences { get; }








	}

	#endregion // IUltraCalcFormula Interface



#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)








































































#endregion // Infragistics Source Cleanup (Region)


	// MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
    // Moved this enum from the Calc Engine to here in Shared. 
    #region UltraCalcFormulaTokenType
	/// <summary>
	/// Identifies formula token types in the <b>UltraCalcFormulaToken</b> class
	/// </summary>
    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
	//internal enum UltraCalcFormulaTokenType    



    internal enum UltraCalcFormulaTokenType



	{
        /// <summary>
        /// Indicates a value token containing an IUltraCalcReference.
        /// </summary>
		//eValue, 
        Value, 

        /// <summary>
        /// Indicates a function token containing an IUltraCalcFunction. 
        /// </summary>
		//eFunction,
        Function,

        // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
        // Removed this since it is never used. 
		//eNull
	}
	#endregion //UltraCalcFormulaTokenType

    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
    // Moved this enum from the Calc Engine to here in Shared. 
    #region UltraCalcOperatorFunction
    /// <summary>
    /// Enumeration of operator functions.
    /// </summary>



	internal enum UltraCalcOperatorFunction



    {
        /// <summary>
        /// Operator used to add two values ("+")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        Add = 0,

        /// <summary>
        /// Operator used to subtract two values ("-")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        Subtract = 1,

        /// <summary>
        /// Operator used to multiply two values ("*")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        Multiply = 2,

        /// <summary>
        /// Operator used to divide two values ("/")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        Divide = 3,

        /// <summary>
        /// Operator used to compare two objects for equality ("=")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        Equal = 4,

        /// <summary>
        /// Operator used to compare if two values are different ("&gt;&lt;" or "!=")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        NotEqual = 5,

        /// <summary>
        /// Operator used to determine if one value is greater than or equal to a second value. ("&gt;=")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        GreaterThanOrEqual = 6,

        /// <summary>
        /// Operator used to determine if one value is greater than a second value. ("&gt;")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        GreaterThan = 7,

        /// <summary>
        /// Operator used to determine if one value is less than or equal to a second value. ("&lt;=")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        LessThanOrEqual = 8,

        /// <summary>
        /// Operator used to determine if one value is less than a second value. ("&lt;")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        LessThan = 9,

        /// <summary>
        /// Operator used to concatenate 2 strings ("&amp;")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        Concatenate = 10,

        /// <summary>
        /// Operator used to raise a value to a specified power ("^")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly two operands/arguments.</p>
        /// </remarks>
        Exponent = 11,

        /// <summary>
        /// Operator used to convert a value to a percentage ("%")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly one operand/argument.</p>
        /// </remarks>
        Percent = 12,

        /// <summary>
        /// Negative unary operator ("-")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly one operand/argument.</p>
        /// </remarks>
        UnaryMinus = 13,

        /// <summary>
        /// Positive unary operator ("+")
        /// </summary>
        /// <remarks>
        /// <p class="note"><b>Note:</b> This operator must take exactly one operand/argument.</p>
        /// </remarks>
        UnaryPlus = 14,
    }
    #endregion //UltraCalcOperatorFunction




#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)

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