using System;
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


#pragma warning disable 1058


namespace Infragistics.Calculations.Engine





{
	// SSP 9/27/04 - Circular Relative Index Support
	// Added RowIterationType enum.
	//
	internal enum RowIterationType
	{
		Any,
		Forward,
		Backward			
	}
	
	/// <summary>
	/// Provides methods to compile and evaluate formulas.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// </p>
	/// </remarks>
	// MD 8/17/08 - Excel formula solving
	// Refactoring - Renamed UltraCalcFormula to UltraCalcFormulaBase and split in abstract base class and derived class 
	// so the PerCederberg compiler is not needed for Excel formulas.
	//internal class UltraCalcFormula : IUltraCalcFormula
	internal abstract class UltraCalcFormulaBase : IUltraCalcFormula
	{
		#region Member Variables

		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		//// UCCompilation class instance
		//private UCCompilation comp;
		//
		//// Error message returned from formula compilation
		//private string error;
		//
		//// Formula base reference (LHS)used to resolve relative references
		//private IUltraCalcReference reference;

		// SSP 9/27/04 - Circular Relative Index Support
		//
		#region Circular Relative Index Support 

		private int siblingNumber = -1; 
		private int dependancySortNumber = -1;
		private int evaluationRowOffset = 0;
		private int evaluationGroupNumber = -1;
		private RowIterationType rowIterationType = RowIterationType.Any; 
		private ReferenceHolder baseReferenceHolder = null;

		#endregion // Circular Relative Index Support

		// MD 7/25/08
		// Found while implementing Excel formula solving
		// The static references should really be cached.
		private UltraCalcFormulaReferenceCollection references;

		// MD 7/30/08 - Excel formula solving
		private IEnumerable<IUltraCalcReference> allReferences;

		// MD 8/20/08 - Excel formula solving
		private bool isCircularFormula;
		private int iterationCount;
		private int originalDependancySortNumber;

		#endregion //Member Variables

		#region Properties

		/// <summary>
		/// Returns true if the formula has a syntax error. Formulas with syntax errors can not be added to the calc-network.
		/// </summary>
		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		//public bool HasSyntaxError { get { return error!=null; } }
		public abstract bool HasSyntaxError { get; }

		/// <summary>
		/// Syntax error message. Returns null if the formula has no syntax errors, as would be indicated by <see cref="HasSyntaxError"/> method.
		/// </summary>
		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		//public string SyntaxError { get { return error; } }
		public abstract string SyntaxError { get; } 

		/// <summary>
		/// Denotes whether the formula contains an always dirty function
		/// </summary>
		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		//public bool HasAlwaysDirty { get { return comp.HasAlwaysDirty; } }
		public abstract bool HasAlwaysDirty { get; }

		#endregion //Properties

		#region Constructor
		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		#region Moved to derived class

		///// <summary>
		///// Initializes and compiles a new <see cref="UltraCalcFormula"/>
		///// </summary>
		///// <param name="formulaReference">Formula base reference</param>
		///// <param name="formula">Formula string to be compiled</param>
		//public UltraCalcFormula(IUltraCalcReference formulaReference, string formula) : this(formulaReference, formula, null)
		//{
		//}
		//
		///// <summary>
		///// Initializes and compiles a new <see cref="UltraCalcFormula"/>
		///// </summary>
		///// <param name="formulaReference">Formula base reference</param>
		///// <param name="formula">Formula string to be compiled</param>
		///// <param name="functionFactory">Class providing the functions</param>
		//public UltraCalcFormula(IUltraCalcReference formulaReference, string formula, UltraCalcFunctionFactory functionFactory)
		//{
		//    this.comp = new UCCompilation(functionFactory);
		//    this.Compile(formulaReference, formula);
		//} 

		#endregion Moved to derived class
		protected UltraCalcFormulaBase() { }

		#endregion //Constructor

		#region Properties

		#region FormulaString
		/// <summary>
		/// Returns the formula string
		/// </summary>
		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		//public string FormulaString
		//{
		//    get 
		//    {
		//        return comp.FormulaString;
		//    }
		//}
		public abstract string FormulaString { get; }
		#endregion //FormulaString

		// SSP 9/27/04 - Circular Relative Index Support
		//
		#region Circular Relative Index Support 

		#region SiblingNumber
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int SiblingNumber
		{
			get
			{
				return this.siblingNumber;
			}
			set
			{
				this.siblingNumber = value;
			}
		}

		#endregion // SiblingNumber

		#region DependancySortNumber
		






		internal int DependancySortNumber
		{
			get
			{
				return this.dependancySortNumber;
			}
			set
			{
				this.dependancySortNumber = value;

				// MD 8/20/08 - Excel formula solving
				// Store the original sort number when it is set directly
				this.originalDependancySortNumber = dependancySortNumber;
			}
		}

		#endregion // DependancySortNumber

		#region EvaluationRowOffset
		






		internal int EvaluationRowOffset
		{
			get
			{
				return this.evaluationRowOffset;
			}
			set
			{
				this.evaluationRowOffset = value;
			}
		}

		#endregion // EvaluationRowOffset

		#region RowIterationType
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal RowIterationType RowIterationType
		{
			get
			{
				return this.rowIterationType;
			}
			set
			{
				this.rowIterationType = value;
			}
		}

		#endregion // RowIterationType

		#region RowIterationTypeResolved
		





		internal RowIterationType RowIterationTypeResolved
		{
			get
			{
				if ( RowIterationType.Any == this.RowIterationType )
					return RowIterationType.Forward;

				return this.RowIterationType;
			}
		}

		#endregion // RowIterationTypeResolved

		#region EvaluationGroupNumber
		


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int EvaluationGroupNumber
		{
			get
			{
				return this.evaluationGroupNumber;
			}
			set
			{
				this.evaluationGroupNumber = value;
			}
		}

		#endregion // EvaluationGroupNumber

		#region BaseReferenceHolder
		






		internal ReferenceHolder BaseReferenceHolder
		{
			get
			{
				return this.baseReferenceHolder;
			}
			set
			{
				this.baseReferenceHolder = value;
			}
		}

		#endregion // BaseReferenceHolder

		// MD 8/20/08 - Excel formula solving
		#region IsCircularFormula






		public bool IsCircularFormula
		{
			get { return this.isCircularFormula; }
			set { this.isCircularFormula = value; }
		} 

		#endregion IsCircularFormula

		#region IterationCount






		public int IterationCount
		{
			get { return this.iterationCount; }
		} 

		#endregion IterationCount

		#endregion // Circular Relative Index Support 

		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		//#region DebugRootNode
		//internal PerCederberg.Grammatica.Parser.Node DebugRootNode
		//{
		//    get
		//    {
		//        return comp.RootNode;
		//    }
		//}
		//#endregion //DebugRootNode

		#region BaseReference
		/// <summary>
		/// Gets or sets the formula base reference
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The base reference of a formula is sometimes referred to as the Left-Hand-Side (LHS)of an expression.  
		/// It represents the location within the grid, or other object, whose value is being computed by the given formula.  
		/// References within the formula expression my be fully qualified or relative to the formula's base reference.  
		/// For example, if we want to calculate a "[Total]" column in a grid we might have a formula such as "[Units] * [Price]".  Each cells within the
		/// "Total" column would be computed multiplying the "Units" cell and "Price" cell for row in the band.
		/// </p>
		/// </remarks>
		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		//public IUltraCalcReference BaseReference 
		//{
		//    get
		//    {
		//        return reference;
		//    }
		//}
		public abstract IUltraCalcReference BaseReference { get; }
		#endregion //BaseReference

		#region Tokens
		/// <summary>
		/// Returns a collection of formula tokens in pre-fix form, called Reverse Polish Notation.
		/// </summary>
		/// <returns>Formula token collection</returns>
		/// <remarks>
		/// <p class="body">
		/// Compiling a formula converts the in-fix expression string into an intemediate post-fix form optimized for evaluation.  
		/// </p>
		/// </remarks>
		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		//internal UltraCalcFormulaTokenCollection Tokens
		//{
		//    get { return comp.Tokens; }
		//}
		internal abstract UltraCalcFormulaTokenCollection Tokens { get; }
		#endregion //Tokens

		#region References
		/// <summary>
		/// Retuns a collection of references contained in the formula token string
		/// </summary>
		/// <returns>Collection of IUltraCalcReferences</returns>
		public IUltraCalcReferenceCollection References
		{
			// MD 8/17/08 - Excel formula solving
			// Moved all code to the ReferencesInternal property
			get { return this.ReferencesInternal; }
		}

		// MD 8/17/08 - Excel formula solving
		// Created an internal references property so members on the actual type of the references collection can be used internally.
		internal UltraCalcFormulaReferenceCollection ReferencesInternal
		{	
			get
			{
				// MD 7/25/08
				// Found while implementing Excel formula solving
				// The static references should really be cached.
				//return new UltraCalcFormulaReferenceCollection(this.Tokens);
				if ( this.references == null )
				{
					// MD 8/4/08 - Excel formula solving
					// The constructor needs more info now, so pass the entire formula, not just the token collection.
					//this.references = new UltraCalcFormulaReferenceCollection( this.Tokens );
					this.references = new UltraCalcFormulaReferenceCollection( this );
				}

				return this.references;
			}
		}
		#endregion //References

		// MD 7/30/08 - Excel formula solving
		// Added a way to enumerate all references in the collection.
		#region AllReferences

		internal IEnumerable<IUltraCalcReference> AllReferences
		{
			get
			{
				if ( this.allReferences == null )
					this.allReferences = new AllReferencesEnumerator( this );

				return this.allReferences;
			}
		} 

		#endregion AllReferences

		#endregion //Properties

		#region Methods

		// MD 8/17/08 - Excel formula solving
		// Refactoring - Split UltraCalcFormula into abstract base class and derived class.
		#region Moved to derived class

		//#region Compile
		///// <summary>
		///// Compile a formula string for the given base reference.
		///// </summary>
		///// <param name="formulaReference">Formula base reference</param>
		///// <param name="formula">Expression string</param>
		///// <returns>True if the formula is successfully compiled, else false</returns>
		///// <remarks>
		///// <p class="body">
		///// Compiling a formula converts the in-fix expression string into an intemediate post-fix form optimized for evaluation.  
		///// If the formula compliation fails, the Error property contains the reason and location of the error. 
		///// Once formulas are compiled, they may be entered into the calculation network for a given UltraCalcManager instance by calling the <b>UlraCalcEngine.AddFormula</b> method.  
		///// </p>
		///// </remarks>
		//private bool Compile(IUltraCalcReference formulaReference, string formula)
		//{
		//    reference = formulaReference;
		//    bool ok = false;
		//
		//    try 
		//    {
		//        ok = comp.Parse( reference,formula );
		//        if( ok == true ) 
		//        {
		//            ok = comp.Tokenize();
		//            error = null;
		//        }
		//
		//        if( ok == false )
		//        {
		//            error = comp.Error;
		//        }
		//    }
		//    catch( Exception ee ) 
		//    {
		//        error = ee.Message;
		//        return false;
		//    }
		//    catch
		//    {
		//        error = "Unknown Exception";
		//        return false;
		//    }
		//
		//    return ok;
		//}
		//#endregion //Compile 

		#endregion Moved to derived class

		#region Evaluate
		/// <summary>
		/// Evaluate the compiled expression against the given base reference
		/// </summary>
		/// <param name="reference">Base reference used to resolve relative references into absolute references</param>
		/// <returns>Retuns an UltraCalcValue containing result of formula evaluation</returns>
		public UltraCalcValue Evaluate(IUltraCalcReference reference)
		{
			// MD 7/21/08 - Excel formula solving
			// Moved all code to new overload except the code that initializes the new number stack.

			// Initialize Number Stack
			UltraCalcNumberStack NumberStack = new UltraCalcNumberStack();

			return this.EvaluateHelper( reference, NumberStack );
		}

		// MD 7/21/08 - Excel formula solving
		// Created helper method out of the original Evaluate method. This does everything Evaluate did except intialize the
		// numnber stack. It is passed to this helper method. This is so the same number stack can be used to evaluate an
		// UltraCalcFormula if it is a value on the number stack.
		// MD 8/20/08 - Excel formula solving
		// Added another parameter to indicate whether or not to dereference the cell.
		//internal UltraCalcValue EvaluateHelper( IUltraCalcReference reference, UltraCalcNumberStack NumberStack )
		internal UltraCalcValue EvaluateHelper( IUltraCalcReference reference, UltraCalcNumberStack NumberStack )
		{
			// MD 8/21/08 - Excel formula solving
			// Store the original count so we can verify the number stack count later
			int originalCount = NumberStack.Count();

			// Initialize result value to error.
			UltraCalcValue Value = new UltraCalcValue( new UltraCalcErrorValue(UltraCalcErrorCode.Num) );

			try 
			{
				// MD 7/21/08 - Excel formula solving
				// Removed: The number stack is now passed into the method
				#region Removed

				//                // Initialize Number Stack
				//                UltraCalcNumberStack NumberStack = new UltraCalcNumberStack();
				//
				//#if EXCEL
				//                // MD 7/16/08 - Excel formula solving
				//                // Some functions need a reference to the reference owning the formula.
				//                NumberStack.FormulaOwner = this.BaseReference;
				//#endif 

				#endregion Removed

				// Loop over the token collection applying operators
				foreach (UltraCalcFormulaToken token in this.Tokens) 
				{
					// If (Token Type is an operand) {
					if (token.Type == UltraCalcFormulaTokenType.Value) 
					{
						// Reference value?
						if (((UltraCalcValueToken)token).Value.IsReference)
						{
							// Push a reference value that has been resolved against its base reference
							NumberStack.Push( new UltraCalcValue(
								reference.ResolveReference(((UltraCalcValueToken)token).Value.ToReference(), ResolveReferenceType.RightHandSide)
								 ) );
						}
						else 
						{
							// Push the non-reference token
							NumberStack.Push( ((UltraCalcValueToken)token).Value);
							}
					}
					else
					{
						// Evaluate the function against its args pushed onto the number stack
						// MD 4/10/12 - TFS108678
						// For Excel, the number stack needs a reference to the function being solved.
						//((UltraCalcFunctionToken)token).Evaluate(NumberStack);
						UltraCalcFunctionToken functionToken = (UltraCalcFunctionToken)token;
						functionToken.Evaluate(NumberStack);
					}
				}

				// Pop result of number stack
				Value = NumberStack.Pop();

				if ( Value.IsReference )
					Value = Value.ToReference().Value; 

				//???? TEMP FIX TO CONVERT ULTRACALCERROREXCEPTION
				if (Value.Value is UltraCalcErrorException)
					Value = new UltraCalcValue( new UltraCalcErrorValue(((UltraCalcErrorException)Value.Value).Value.Code) );

				// MD 8/21/08 - Excel formula solving
				//Debug.Assert(NumberStack.Count() == 0);
				Debug.Assert( NumberStack.Count() == originalCount, "The number stack was not cleared after evaluating the formula." );
			}
			catch(Exception e)
			{
				Debug.WriteLine(e.Message);
				Value = new UltraCalcValue(e);
			}

			// Pop Result Value off number stack and return it
			return Value;
		}
		#endregion //Evaluate

		// MRS NAS v8.3 - Exporting CalculationManager Formulas to Excel
		#region GetTokens
		public IUltraCalcFormulaToken[] GetTokens()
		{
			IUltraCalcFormulaToken[] array = new IUltraCalcFormulaToken[ this.Tokens.Count ];
			this.Tokens.CopyTo( array, 0 );
			return array;
		}
		#endregion //GetTokens 

		#region IncrementIterationCount






		public void IncrementIterationCount()
		{
			this.iterationCount++;
		} 

		#endregion IncrementIterationCount

		#region OnBeforeRecalc






		public void OnBeforeRecalc()
		{
			this.dependancySortNumber = this.originalDependancySortNumber;
			this.iterationCount = 0;
		} 

		#endregion OnBeforeRecalc

		#region SetDependancySortNumberForCircularReference






		public void SetDependancySortNumberForCircularReference( int newDependancySortNumber )
		{
			this.dependancySortNumber = newDependancySortNumber;
		} 

		#endregion SetDependancySortNumberForCircularReference

        #endregion //Methods


		// MD 7/30/08 - Excel formula solving
		#region AllReferencesEnumerator class

		private class AllReferencesEnumerator : IEnumerable<IUltraCalcReference>
		{
			private UltraCalcFormulaBase owner;

			public AllReferencesEnumerator( UltraCalcFormulaBase owner )
			{
				this.owner = owner;
			}

			#region IEnumerable<IUltraCalcReference> Members

			public IEnumerator<IUltraCalcReference> GetEnumerator()
			{
				foreach ( IUltraCalcReference reference in this.owner.References )
				{
					{
						yield return reference;
					}
				}
			}

			#endregion

			#region IEnumerable Members

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			#endregion
		} 

		#endregion AllReferencesEnumerator class
	}

	internal class UltraCalcFormula : UltraCalcFormulaBase
	{
		// UCCompilation class instance
		private UCCompilation comp;
		
		// Error message returned from formula compilation
		private string error;
		
		// Formula base reference (LHS)used to resolve relative references
		private IUltraCalcReference reference;
		
		/// <summary>
		/// Initializes and compiles a new <see cref="UltraCalcFormula"/>
		/// </summary>
		/// <param name="formulaReference">Formula base reference</param>
		/// <param name="formula">Formula string to be compiled</param>
		public UltraCalcFormula(IUltraCalcReference formulaReference, string formula) : this(formulaReference, formula, null)
		{
		}

		/// <summary>
		/// Initializes and compiles a new <see cref="UltraCalcFormula"/>
		/// </summary>
		/// <param name="formulaReference">Formula base reference</param>
		/// <param name="formula">Formula string to be compiled</param>
		/// <param name="functionFactory">Class providing the functions</param>
		public UltraCalcFormula(IUltraCalcReference formulaReference, string formula, UltraCalcFunctionFactory functionFactory)
		{
			this.comp = new UCCompilation(functionFactory);
			this.Compile(formulaReference, formula);
		}		 
	
		#region BaseReference
		/// <summary>
		/// Gets or sets the formula base reference
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The base reference of a formula is sometimes referred to as the Left-Hand-Side (LHS)of an expression.  
		/// It represents the location within the grid, or other object, whose value is being computed by the given formula.  
		/// References within the formula expression my be fully qualified or relative to the formula's base reference.  
		/// For example, if we want to calculate a "[Total]" column in a grid we might have a formula such as "[Units] * [Price]".  Each cells within the
		/// "Total" column would be computed multiplying the "Units" cell and "Price" cell for row in the band.
		/// </p>
		/// </remarks>
		public override IUltraCalcReference BaseReference 
		{
			get
			{
				return reference;
			}
		}
		#endregion //BaseReference
		
		#region FormulaString
		/// <summary>
		/// Returns the formula string
		/// </summary>
		//public string FormulaString
		public override string FormulaString
		{
			get 
			{
				return comp.FormulaString;
			}
		}
		#endregion //FormulaString
	  
		public override bool HasAlwaysDirty { get { return comp.HasAlwaysDirty; } }
		
		/// <summary>
		/// Returns true if the formula has a syntax error. Formulas with syntax errors can not be added to the calc-network.
		/// </summary>
		public override bool HasSyntaxError { get { return error!=null; } }

		/// <summary>
		/// Syntax error message. Returns null if the formula has no syntax errors, as would be indicated by <see cref="HasSyntaxError"/> method.
		/// </summary>
		public override string SyntaxError { get { return error; } }
	 
		#region Tokens
		/// <summary>
		/// Returns a collection of formula tokens in pre-fix form, called Reverse Polish Notation.
		/// </summary>
		/// <returns>Formula token collection</returns>
		/// <remarks>
		/// <p class="body">
		/// Compiling a formula converts the in-fix expression string into an intemediate post-fix form optimized for evaluation.  
		/// </p>
		/// </remarks>
		//internal UltraCalcFormulaTokenCollection Tokens
		internal override UltraCalcFormulaTokenCollection Tokens
		{
			get { return comp.Tokens; }
		}
		#endregion //Tokens
		 
		#region Compile
		/// <summary>
		/// Compile a formula string for the given base reference.
		/// </summary>
		/// <param name="formulaReference">Formula base reference</param>
		/// <param name="formula">Expression string</param>
		/// <returns>True if the formula is successfully compiled, else false</returns>
		/// <remarks>
		/// <p class="body">
		/// Compiling a formula converts the in-fix expression string into an intemediate post-fix form optimized for evaluation.  
		/// If the formula compliation fails, the Error property contains the reason and location of the error. 
		/// Once formulas are compiled, they may be entered into the calculation network for a given UltraCalcManager instance by calling the <b>UlraCalcEngine.AddFormula</b> method.  
		/// </p>
		/// </remarks>
		private bool Compile(IUltraCalcReference formulaReference, string formula)
		{
			reference = formulaReference;
			bool ok = false;

			try 
			{
				ok = comp.Parse( reference,formula );
				if( ok == true ) 
				{
					ok = comp.Tokenize();
					error = null;
				}

				if( ok == false )
				{
					error = comp.Error;
				}
			}
			catch( Exception ee ) 
			{
				error = ee.Message;
				return false;
			}
			catch
			{
				error = "Unknown Exception";
				return false;
			}

			return ok;
		}
		#endregion //Compile
		
		#region DebugRootNode
		internal PerCederberg.Grammatica.Parser.Node DebugRootNode
		{
			get
			{
				return comp.RootNode;
			}
		}
		#endregion //DebugRootNode

		// MD 7/28/11
		#region ParseException

		/// <summary>
		/// Return the exception which occurred while parsing the formula.
		/// </summary>
		public Exception ParseException
		{
			get { return this.comp.ParseException; }
		}

		#endregion  // ParseException
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