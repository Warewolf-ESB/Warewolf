using System;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;


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







using Infragistics.Shared;



namespace Infragistics.Calculations.Engine







{


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


	#region UltraCalcErrorCode
	/// <summary>
	/// Enumeration of error codes assigned to <see cref="UltraCalcErrorValue"/>.
	/// </summary>

	public enum CalculationErrorCode 


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



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



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

	}
	#endregion //UltraCalcErrorCode

	#region UltraCalcErrorException
	/// <summary>
	/// Exception containing an <see cref="UltraCalcErrorValue"/>.
	/// </summary>

    internal class CalcErrorException : CalculationException





	{
		#region Member Variables

		private UltraCalcErrorValue value;

		#endregion //Member Variables

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="UltraCalcErrorException"/> using the specified <see cref="UltraCalcErrorValue"/>
		/// </summary>
        /// <param name="errorValue">The UltraCalcErrorValue that has generated the exception.</param>

		public CalcErrorException(CalculationErrorValue errorValue)



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

    public class CalculationException : Exception





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

		public CalculationException()



		{
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcException"/> with the specified error message
		/// </summary>
		/// <param name="message">Error message</param>

		public CalculationException( string message ) : base(message)



		{
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcException"/> with the specified error message and exception instance.
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="innerException">The exception that caused this exception</param>

		public CalculationException( string message,Exception innerException ) : base(message,innerException)



		{
		}


		// MD 8/3/11 - XamFormulaEditor
		/// <summary>
		/// For Infragistics internal use only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]

		public CalculationException(



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


	#region UltraCalcValueException
	/// <summary>
	/// Calc value Exception
	/// </summary>

	public class CalculationValueException : CalculationException





	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UltraCalcValueException"/>
		/// </summary>

		public CalculationValueException()



		{
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcValueException"/> with the specified error message.
		/// </summary>
		/// <param name="message">Error message text.</param>
		/// <remarks>
		/// <p class="body">It's the developer's responsibility to ensure the <i>message</i> text
		/// has been properly localized.</p>
		/// </remarks>

		public CalculationValueException( string message )



			: base( message )
		{
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcValueException"/> with the specified error message and exception instance.
		/// </summary>
		/// <param name="message">Error message text.</param>
		/// <param name="innerException">An underlying exception that was responsible for this <see cref="UltraCalcValueException"/> being thrown.</param>
		/// <remarks>
		/// <p class="body">It's the developer's responsibility to ensure the <i>message</i> text
		/// has been properly localized.</p>
		/// </remarks>

		public CalculationValueException( string message, Exception innerException )



			: base( message, innerException )
		{
		}
		#endregion //Constructor
	}
	#endregion //UltraCalcValueException 



	#region UltraCalcNumberException
	/// <summary>
	/// Number Exception
	/// </summary>

	internal class CalcNumberException : CalculationException





	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="UltraCalcNumberException"/>
		/// </summary>

		public CalcNumberException()



		{
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcNumberException"/> with the specified error message.
		/// </summary>
		/// <param name="message">Error message text.</param>
		/// <remarks>
		/// <p class="body">It's the developer's responsibility to ensure the <i>message</i> text
		/// has been properly localized.</p>
		/// </remarks>

		public CalcNumberException( string message )



			: base( message )
		{
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcNumberException"/> with the specified error message and exception instance.
		/// </summary>
		/// <param name="message">Error message text.</param>
		/// <param name="innerException">An underlying exception that was responsible for this <see cref="UltraCalcNumberException"/> being thrown.</param>
		/// <remarks>
		/// <p class="body">It's the developer's responsibility to ensure the <i>message</i> text
		/// has been properly localized.</p>
		/// </remarks>

		public CalcNumberException( string message, Exception innerException )



			: base( message, innerException )
		{
		}
		#endregion //Constructor
	}
	#endregion //UltraCalcNumberException 


	#region UltraCalcErrorValue

	/// <summary>
	/// Provides methods and properties used to define and manage a calculation error value.
	/// </summary>

    public class CalculationErrorValue


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



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

        public CalculationErrorValue(UltraCalcErrorCode code)





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



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			else
				this.errorMessage = SR.GetString( "Error_UCErrorCode_Unknown" );
		}

		/// <summary>
		/// Initializes a new <see cref="UltraCalcErrorValue"/> with the specified error code and error message
		/// </summary>
		/// <param name="code"><see cref="UltraCalcErrorCode"/> value</param>
		/// <param name="message">Localized Message indicating reason for error</param>

    public CalculationErrorValue(UltraCalcErrorCode code, string message )





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

    public CalculationErrorValue(UltraCalcErrorCode code, string message, object value )





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



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


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

    public interface ICalculationReference


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



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


		/// <summary>
		/// Flag used by the calculation engine to manage the recalculation network. 
		/// Denotes the reference is visible.
		/// </summary>
		bool RecalcVisible { get; set; }

		/// <summary>
		/// True indicates that recalculating reference's formula can deferred until it become visible.
		/// </summary>
		bool RecalcDeferred { get; set; }

		/// <summary>
		/// True indicates that the reference was constructed with a relative index.  Read Only.
		/// </summary>
		bool HasRelativeIndex { get; }

		/// <summary>
		/// True indicates that the reference was constructed with an absolute index.  Read Only.
		/// </summary>
		bool HasAbsoluteIndex { get; }

		/// <summary>
		/// True indicates that the reference was constructed with an "*" scope index.  Read Only.
		/// </summary>
		bool HasScopeAll { get; } 



		/// <summary>
		/// Returns a context for the Reference.
		/// </summary>
		/// <remarks><p class="body">Returns a meaningful object context for the reference. This could be a Cell, Field, NamedReference NamedReference, Control, etc..</p></remarks>






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


		/// <summary>
		/// Create a Range reference relative to this reference.
		/// </summary>
		/// <param name="fromReference">The start of the range, inclusive.</param>
		/// <param name="toReference">The end of the range, inclusive.</param>
		/// <returns>A RangeReference</returns>
		/// <remarks>
		/// CreateRange should return a <see cref="IUltraCalcReference"/> implementation whose References 
		/// method returns an iterator over the specified range.
		/// <p>This method must be able to handle external, absolute and relative references as 
		/// described in the CreateReference method.</p>
		/// <p>Note that both references in the range must be local to each other.</p>
		/// </remarks>
		/// <seealso cref="CreateReference"/>
		/// <seealso cref="References"/>
		IUltraCalcReference CreateRange( string fromReference, string toReference ); 

		/// <summary>
		/// Resolves a reference relative to this reference.
		/// </summary>
		/// <param name="reference">The reference to resolve.</param>
		/// <param name="referenceType">Indicates whether the reference being resolved is the lvalue of the formula or an rvalue of the formula.</param>
		/// <returns>The resolved reference.</returns>
		/// <remarks>
		/// ResolveReference is used to merge two references while giving precedence to elements with
		/// more specific scope.  Each element in this reference and the input reference up to but not
		/// including the last element must be identical except for scope.  If the scopes are not identical,
		/// then one of the two scopes must be "any".  Note that elements with a scope of relative index
		/// are converted to scope "any" in the output reference.  This yields an enumerable reference and
		/// allows the calculation engine to properly construct the dirty network.  
		/// <p>The last element of the input reference replaces the
		/// last element of this reference.</p>
		/// <p>This method is used by the evaluator to take an unscoped reference, such as a column reference,
		/// and merge into a fully scoped reference, such as a row reference, to yield a cell reference.
		/// </p><p>
		/// Note that this method must operate on the reference string used to create the reference.  When
		/// a reference with relative scope is created by CreateReference, such as [Price(-1)], the underlying
		/// reference is a column reference. The reference string used to create the refernce is required
		/// to properly apply the scope to the merged result.</p>
		/// </remarks>
		// SSP 9/15/04
		//
		//IUltraCalcReference ResolveReference(IUltraCalcReference inReference);
		IUltraCalcReference ResolveReference( IUltraCalcReference reference, ResolveReferenceType referenceType );

		/// <summary>
		/// For each reference tuple in this reference that has a relative index, mark the
		/// corresponding tuple in inReference.
		/// </summary>
		/// <param name="inReference">The Reference to be marked.</param>
		/// <remarks>
		/// This method is used by the evaluator to set up this reference for resolving an input 
		/// reference that has elements with a scope of relative index.
		/// <see>ResolveReference</see>
		/// </remarks>
		void MarkRelativeIndices( IUltraCalcReference inReference ); 


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


		/// <summary>
		/// Returns true if this reference is a data reference.  A data reference contains a value, such as a Grid Cell or
		/// a Grid SummaryValue.  Many referenced elements, such as a Grid Column or a Grid Band, do not reference a value.
		/// Read Only. 
		/// </summary>
		bool IsDataReference { get; }

		/// <summary>
		/// The Parent reference of this referenced element.  Used to navigate the element hierarchy.  If this
		/// referenced element is the root element, <b>Parent</b> returns null.  Read Only.
		/// </summary>
		IUltraCalcReference Parent {get; }

		/// <summary>
		/// Determines whether the given reference is a sibling of this reference
		/// </summary>
		/// <param name="reference">The reference to compare against this one</param>
		/// <returns>True if the reference is a sibling of the given reference</returns>
		bool IsSiblingReference(IUltraCalcReference reference);

		/// <summary>
		/// True indicates that the reference has been disposed.  Read Only.
		/// </summary>
		bool IsDisposedReference { get; }

		// SSP 9/7/04
		// Added GetChildReferences method to IUltraCalcReference interface and 
		// ReferenceType enum which the added method takes as a parameter.
		//
		/// <summary>
		/// Returns the child references of the reference. This method can return null.
		/// </summary>
		/// <param name="referenceType">The <see cref="ChildReferenceType"/> to retrieve.</param>
		/// <returns>An array of <see cref="IUltraCalcReference"/> objects that represent the child references.</returns>
		IUltraCalcReference[] GetChildReferences( ChildReferenceType referenceType ); 


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


	#region ChildReferenceType
	// SSP 9/7/04
	// Added GetChildReferences method to IUltraCalcReference interface and 
	// ReferenceType enum which the added method takes as a parameter.
	//
	/// <summary>
	/// Used for specifying the referenceType parameter to the <see cref="IUltraCalcReference.GetChildReferences"/> method.
	/// </summary>
	[Flags()]
	public enum ChildReferenceType
	{
		/// <summary>
		/// Returns references that have formulas, like column references or summary references 
		/// but not cell references.
		/// </summary>
		ReferencesWithFormulas = 1
	}
	#endregion //ChildReferenceType 

	#region ResolveReferenceType
	/// <summary>
	/// Specifies the type of reference being resolved in the <see cref="IUltraCalcReference.ResolveReference"/> method.
	/// </summary>
	public enum ResolveReferenceType
	{
		/// <summary>
		/// Specifies that the reference being resolved is the lvalue of the formula.
		/// </summary>
		LeftHandSide = 0,

		/// <summary>
		/// Specifies that the reference being resolved is an rvalue of the formula.
		/// </summary>
		RightHandSide = 1
	}
	#endregion //ResolveReferenceType 


	#region IUltraCalcReferenceCollection
	/// <summary>
	/// Collection of <see cref="IUltraCalcReference"/> objects
	/// </summary>

    public interface ICalculationReferenceCollection : IEnumerable


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



    {
	}
	#endregion //IUltraCalcReferenceCollection


	#region UltraCalcFormulaStates.cs
	/// <summary>
	/// Provides a set of properties used by the <b>UltraCalcEngine</b> to manage the recalculation network.
	/// </summary>

	internal struct UltraCalcFormulaStates



	{
		#region Member Variables

		// Bit array providing storage for the formula states and methods to modify them.
		private int formulaStates;

		#endregion //Member Variables

		#region Properties

		/// <summary>
		/// Denotes the reference is visible.
		/// </summary>
		public bool RecalcVisible
		{
			get { return ( this.formulaStates & 0x1 ) != 0; }
			set
			{
				if ( value )
					this.formulaStates |= 0x1;
				else
					this.formulaStates &= ~0x1;
			}
		}

		/// <summary>
		/// Denotes whether the calculation of the reference's formula can be deferred until its visible
		/// </summary>
		public bool RecalcDeferred
		{
			get { return ( this.formulaStates & 0x2 ) != 0; }
			set
			{
				if ( value )
					this.formulaStates |= 0x2;
				else
					this.formulaStates &= ~0x2;
			}
		}
		#endregion //Properties

		#region Reset
		/// <summary>
		/// Resets the bits to false.
		/// </summary>
		public void Reset()
		{
			this.formulaStates = 0;
		}
		#endregion //Reset
	}

	#endregion // UltraCalcFormulaStates.cs 


	#region UltraCalcReferenceError.cs

	/// <summary>
	/// Implementation of <see cref="IUltraCalcReference"/> interface that denotes a reference error
	/// </summary>

    public class CalculationReferenceError: IUltraCalcReference





	{
		#region Member Variables


		private UltraCalcFormulaStates formulaStates;	// Storage for formula states  

		private string message;							// Storage for error message
		private string reference;						// Storage for reference absolute name







		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="UltraCalcReferenceError"/>.
		/// </summary>
		/// <param name="reference">Absolute name of reference</param>

		public CalculationReferenceError( string reference )



			: this( reference, (string)null )
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="UltraCalcReferenceError"/>.
		/// </summary>
		/// <param name="reference">Absolute name of reference</param>
		/// <param name="exception">Exception for which the reference should be created</param>

		public CalculationReferenceError( string reference, Exception exception )



			: this( reference, exception == null ? (string)null : exception.Message )
		{
		} 


		/// <summary>
		/// Reference string constructor
		/// </summary>
		/// <param name="reference">Absolute name of reference</param>
		/// <param name="message">Localized Error message</param>

		public CalculationReferenceError( string reference,string message )



		{
			this.message = message;
			this.reference = reference;

			this.formulaStates = new UltraCalcFormulaStates(); 

		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


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



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


		#region Implementation of IUltraCalcReference

		object IUltraCalcReference.Context { get{ return null;} }

		IUltraCalcReference IUltraCalcReference.CreateReference(string reference)
		{
			return null;
		}


		IUltraCalcReference IUltraCalcReference.CreateRange( string fromReference, string toReference )
		{
			return null;
		} 

		IUltraCalcReference IUltraCalcReference.ResolveReference(IUltraCalcReference reference, ResolveReferenceType referenceType )
		{
			// AS 9/14/04
			// Return the reference passed in if we don't recognize it.
			//return null;
			return reference;
		}

		bool IUltraCalcReference.IsDisposedReference
		{
			get
			{
				return false;
			}
		} 


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





			get { return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Reference ) );	}

			
			set {	}
		}


		bool IUltraCalcReference.HasRelativeIndex
		{
			get { return false; }
		}

		bool IUltraCalcReference.HasAbsoluteIndex
		{
			get { return false; }
		}

		bool IUltraCalcReference.HasScopeAll
		{
			get { return false; }
		}

		void IUltraCalcReference.MarkRelativeIndices( IUltraCalcReference reference )
		{
		} 


		IUltraCalcFormula IUltraCalcReference.Formula
		{
			get	{ return null; 	}
		}


		bool IUltraCalcReference.RecalcDeferred
		{
			get { return formulaStates.RecalcDeferred; }
			set { formulaStates.RecalcDeferred = value; }
		}

		bool IUltraCalcReference.RecalcVisible
		{
			get { return formulaStates.RecalcVisible; }
			set { formulaStates.RecalcVisible = value; }
		} 


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


		bool IUltraCalcReference.IsDataReference {get {return false;} }

		IUltraCalcReference IUltraCalcReference.Parent { get { return null; } } 


		bool IUltraCalcReference.ContainsReference(IUltraCalcReference reference)
		{
			return this == reference;
		}

		bool IUltraCalcReference.IsSubsetReference(IUltraCalcReference reference)
		{
			return false;
		}


		bool IUltraCalcReference.IsSiblingReference(IUltraCalcReference reference)
		{
			return false;
		}

		// SSP 9/7/04
		// Added GetChildReferences method to IUltraCalcReference interface and 
		// ReferenceType enum which the added method takes as a parameter.
		//
		IUltraCalcReference[] IUltraCalcReference.GetChildReferences( ChildReferenceType referenceType )
		{
			return null;
		} 


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

    public class CalculationValue : object, IConvertible, IComparable


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)



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

			public CalculationValue(object value)





			{
				// MRS 6/7/05
				// Always store the value, unless it's an Exception
				if ( value is Exception )
					ExceptionHandler((Exception)value);
				else
				{


#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


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

			public CalculationValue() : this( (object)null )





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

				else if
					( e is UltraCalcValueException )
					this.value = new UltraCalcErrorValue( UltraCalcErrorCode.Value, e.Message, e );
				else if
							( e is UltraCalcNumberException )
					this.value = new UltraCalcErrorValue( UltraCalcErrorCode.Num, e.Message, e ); 

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


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

				errorValue = null;

				// MD 7/17/08 - Excel formula solving
				// Moved all code to new helper method below
				IUltraCalcReference reference = this.ToReferenceHelper();



#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)


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





				DateTime dateValue )
			{
				// MD 4/15/09 - TFS16390
				// Moved all code to the new overload.
				return UltraCalcValue.DateTimeToExcelDate(



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



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


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





				double excelDate )
			{
				return UltraCalcValue.ExcelDateToDateTime(



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





				double excelDate,
				bool shouldCorrect1900Dates )
			{
				// MD 2/14/12 - 12.1 - Table Support
				// Moved the code to a new overload of ExcelDateToDateTime.
				return UltraCalcValue.ExcelDateToDateTime(



				excelDate, shouldCorrect1900Dates, false);
			}

			// MD 2/14/12 - 12.1 - Table Support
			// Added a new overload which will allow us to get days before the Excel minimum date.
			internal static DateTime? ExcelDateToDateTime(



				double excelDate,
				bool shouldCorrect1900Dates,
				bool allowDateBeforeMinimum)
			{
				// MD 4/15/09 - TFS16390
				// Honor the new shouldCorrect1900Dates parameter.
				//bool correct1900Dates = true;
				bool correct1900Dates = shouldCorrect1900Dates;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


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



			internal

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


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

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



						raiseExceptionOnError);
				}

				object returnValue = this.value;



#region Infragistics Source Cleanup (Region)




































































#endregion // Infragistics Source Cleanup (Region)


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
	#endregion // UltraCalcValue.cs



#region Infragistics Source Cleanup (Region)
































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


	#region IUltraCalcFormula Interface

	/// <summary>
	/// Interface implemented by the formula object.
	/// </summary>

    public interface ICalculationFormula


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



	{
		/// <summary>
		/// Formula string.
		/// </summary>
		string FormulaString { get; }


		/// <summary>
		/// Returns true if the formula has a syntax error. Formulas with syntax errors can not be added to the calc-network.
		/// </summary>
		bool HasSyntaxError { get; } 


		/// <summary>
		/// Returns whether the formula contains an always dirty function.
		/// </summary>
		bool HasAlwaysDirty { get; }


		/// <summary>
		/// Syntax error message. Returns null if the formula has no syntax errors, as would be indicated by <see cref="HasSyntaxError"/> method.
		/// </summary>
		string SyntaxError { get; } 


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


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)








	}

	#endregion // IUltraCalcFormula Interface



#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)



    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
    #region IUltraCalcFormulaToken Interface

    /// <summary>
	/// Interface implemented by the calcengine formula token object.
	/// </summary>

    internal interface IUltraCalcFormulaToken



    {
        #region Type
        /// <summary>
        /// Return the token's type code
        /// </summary>
        /// <returns>The <b>UltraClacFormulaTokenType</b> for the this token</returns>
        UltraCalcFormulaTokenType Type { get; }
        #endregion //Type
    }
    #endregion //IUltraCalcFormulaToken Interface

    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
    #region IUltraCalcValueToken Interface

    /// <summary>
    /// Interface implemented by the calcengine formula token object.
    /// </summary>

    internal interface IUltraCalcValueToken : IUltraCalcFormulaToken



    {
        /// <summary>
        /// The UltraCalcValue represented by the token. 
        /// </summary>
        UltraCalcValue Value { get; }        
    }
    #endregion //IUltraCalcValueToken Interface

    // MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
    #region IUltraCalcFunctionToken Interface

    /// <summary>
    /// Interface implemented by the calcengine formula token object.
    /// </summary>

    internal interface IUltraCalcFunctionToken : IUltraCalcFormulaToken



    {
        #region ArgumentCount
        /// <summary>
        /// Returns the number of arguments to the function.
        /// </summary>
        int ArgumentCount { get; }
        #endregion //ArgumentCount

        #region FunctionName
        /// <summary>
        /// The name of the UltraCalcFunction represented by the token. 
        /// </summary>
        string FunctionName { get; }
        #endregion //FunctionName

        #region FunctionOperator
        /// <summary>
        /// Returns an UltraCalcOperatorFunction indicating the operator that the function reprsents or null of the function does not represent an operator.   
        /// </summary>
        Nullable<UltraCalcOperatorFunction> FunctionOperator { get; }
        #endregion //FunctionOperator
    }
    #endregion //IUltraCalcFunctionToken Interface


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




	#region UltraCalcAction

	// SSP 6/21/11 - XamCalculationManager
	// Moved this here from UltraCalc.CalcManagerInterfaces.cs.
	// 

	// SSP 1/17/05 BR01753
	// Added PerformAction method to IUltraCalcManager and the associated UltraCalcAction enum.
	//

	/// <summary>
	/// Enum used for specifying the first paramter to <see cref="ICalculationManager.PerformAction"/> method.
	/// </summary>
	public enum CalculationEngineAction






	{
		/// <summary>
		/// Recalc action. The data parameter is the ticks parameter (must be long type) to the 
		/// Recalc method of the calc engine. The return value is a boolean indicating whether 
		/// there is more to recalc.
		/// </summary>
		Recalc,

		/// <summary>
		/// The data parameter is the <see cref="IUltraCalcReference"/> instance to add to the recalc chain. 
		/// This action has no return value.
		/// </summary>
		AddReferenceToRecalcChain
	}

	#endregion // UltraCalcAction

	#region UltraCalcReferenceEventHandler

	// SSP 6/21/11 - XamCalculationManager
	// Moved this here from UltraCalc.CalcManagerInterfaces.cs and also excluded it from EXCEL and XAML_.
	// 







	#endregion //UltraCalcReferenceEventHandler


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