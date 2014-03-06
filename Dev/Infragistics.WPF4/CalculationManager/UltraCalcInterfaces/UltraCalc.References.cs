using System;
using System.Diagnostics;
using System.Collections;
using System.Globalization;


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



namespace Infragistics.Calculations.Engine





{
	#region RefBase

	/// <summary>
	/// This abstract base class is used to implement the <see cref="IUltraCalcReference"/> interface for an
	/// object with a complex hierarchy of referencable elements.
	/// </summary>
	public abstract class RefBase : IUltraCalcReference
	{
		#region Member Variables

		// Stores flags used by the <b>UltraCalcEngine</b> to manage the recalculation network. 
		private UltraCalcFormulaStates formulaStates; 






		// MD 8/27/08 - Code Analysis - Performance
		//private RefParser rp = null;
		private RefParser rp;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RefBase"/>
		/// </summary>
		public RefBase()
		{
			this.formulaStates = new UltraCalcFormulaStates(); 
		}
		#endregion //Constructor

		#region Properties


		/// <summary>
		/// Returns a context for the Reference.
		/// </summary>
		/// <remarks><p class="body">Returns a meaningful object context for the reference. This could be a Cell, Field, NamedReference, Control, etc...</p></remarks>






		public virtual object Context { get{ return null;} }

		// MD 6/25/12 - TFS113177

		/// <summary>
		/// Gets the value indicating whether the formula editor should include default indexes after this reference's address when 
		/// enumerable references are used where a single value is expected.
		/// </summary>
		public virtual bool ShouldFormulaEditorIncludeIndex
		{
			get { return false; }
		}


		/// <summary>
		/// The RefBase Parent reference of this referenced element.  Used to navigate the element hierarchy.  If this
		/// referenced element is the root element, <b>BaseParent</b> returns null.  Read Only.
		/// </summary>
		public abstract RefBase BaseParent { get; } 
		/// <summary>
		/// Returns true if this reference is anchored.  An anchored reference is a direct reference to an element. 
		/// An unanchored reference is an indirect reference.  An indirect reference stores context dependent properties
		/// such as the relative name used to create the reference.  Read Only.
		/// </summary>
		/// <seealso cref="RefUnAnchored"/>
		/// <seealso cref="RelativeReference"/>
		public virtual bool IsAnchored {get {return true;} }

		/// <summary>
		/// Returns true if this reference is a Range Reference.  Read Only.
		/// </summary>
		/// <seealso cref="RefRange"/> 
		public virtual bool IsRange { get { return false; } } 

		/// <summary>
		/// Returns the indirect reference of an UnAnchored reference.  Read only.
		/// </summary>
		/// <seealso cref="RefUnAnchored"/>
		/// <seealso cref="IsAnchored"/>
		public virtual RefBase WrappedReference { get {return this;} }

		/// <summary>
		/// Gets or Sets the parsed representation of this reference.  This returns the absolute reference name.  Use
		/// <b>RelativeReference</b> to get the parsed representation of the string used to create this reference if this 
		/// reference is UnAnchored.
		/// </summary>
		/// <seealso cref="IsAnchored"/>
		/// <seealso cref="RelativeReference"/>
		public virtual RefParser ParsedReference
		{ 
			get 
			{
				if( this.rp == null ) 
					this.rp = this.CreateParsedReference();

				return this.rp;
			}

			set
			{
				this.rp = value;
			}
		}

		/// <summary>
		/// Gets or Sets the parsed representation of the string used to create this reference if this reference
		/// is UnAnchored.  Use <b>AbsoluteReference</b> to get the parsed representation of the anchored reference.
		/// </summary>
		/// <seealso cref="IsAnchored"/>
		/// <seealso cref="ParsedReference"/>
		public virtual RefParser RelativeReference
		{
			get
			{
				return this.ParsedReference;
			}

			set
			{
				throw new UltraCalcException( SR.GetString( "Error_AnchoredReference", new Object[] { GetType().ToString() + " " + ElementName } ) );
			}
		} 

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Returns the Root reference of a reference hierarchy.  The root reference is used to resolve the
		/// fully qualifed form of a reference.  Note:  Must be overridden by the Root reference to return "this".
		/// </summary>
		/// <returns>The Root Reference</returns>
		protected virtual RefBase FindRoot() { return BaseParent.FindRoot(); } 

		/// <summary>
		/// Returns the named reference relative to this reference.
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		public abstract IUltraCalcReference FindItem( string name );

		/// <summary>
		/// Returns the named reference relative to this reference with scope "All".
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		/// <seealso cref="BuildReference"/>
		public abstract IUltraCalcReference FindAll( string name );

		/// <summary>
		/// Returns the named SummaryItem reference relative to this reference.
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		/// <seealso cref="BuildReference"/>
		public abstract IUltraCalcReference FindSummaryItem( string name ); 

		/// <summary>
		/// Returns the named indexed reference relative to this reference.
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <param name="index">The index name.</param>
		/// <returns>The referenced element</returns>
		/// <seealso cref="BuildReference"/>
		public abstract IUltraCalcReference FindItem( string name,string index );

		/// <summary>
		/// Returns the named indexed reference relative to this reference.  If <b>isRelative</b>
		/// is true, then <b>index</b> is treated as a relative to this element index.  Otherwise the
		/// index is treated as absolute.
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <param name="index">The index value.</param>
		/// <param name="isRelative">true if <b>index</b> is relative to this index</param>
		/// <returns>The referenced element</returns>
		/// <seealso cref="BuildReference"/>
		public abstract IUltraCalcReference FindItem( string name,int index,bool isRelative );

		/// <summary>
		/// Returns a reference to this elements Parent.  Some Reference implementations (RefColumn) override 
		/// this to return something different than BaseParent when handling ".."
		/// </summary>
		/// <returns>The referenced element</returns>
		/// <seealso cref="BuildReference"/>
		public virtual RefBase FindParent()
		{
			return this.BaseParent;
		} 

		/// <summary>
		/// Returns a <see cref="IUltraCalcReferenceCollection"/> for enumerable references constrained to the specified reference.
		/// </summary>
		/// <param name="scopeRP">The constraining reference</param>
		/// <returns>A reference collection</returns>
		public virtual IUltraCalcReferenceCollection ScopedReferences( RefParser scopeRP )
		{
			throw new UltraCalcException( SR.GetString( "Error_ReferenceNotEumerable",new Object[] {GetType().ToString() + " " + ElementName}) );
		}

		/// <summary>
		/// Constructs a reference relative to this reference by walking all the elements of the specified parsed reference.
		/// If <b>forceDataRef</b> is true, then any <b>IsDataReference</b> references are forced to resolve to a specific
		/// data item when an incomplete identifier is specified. <b>RefLoop</b> is used to iterate over the specified 
		/// parsed reference.
		/// <p>This method must be overridden by the Root reference object to deal with external or root references.
		/// The Root referece object must advance past the optional control name in a Fully Qualified Reference before
		/// calling <b>RefLoop</b></p>
		/// </summary>
		/// <param name="buildRP">The parsed representation of the reference to construct</param>
		/// <param name="forceDataRef">Force resolution to a specific data item.</param>
		/// <returns>The specified reference</returns>
		/// <seealso cref="RefLoop"/>
		protected virtual IUltraCalcReference BuildReference( RefParser buildRP,bool forceDataRef )
		{
			IEnumerator tupleEnum = buildRP.GetEnumerator();

			return this.RefLoop( this, tupleEnum, forceDataRef );
		}

		/// <summary>
		/// Constructs a reference relative to the specified starting reference by walking the specified <b>RefTuple</b>
		/// enumerator.  For each <b>RefTuple</b>, the next "current" reference is constucted by calling the <b>Find</b>
		/// method based on the scope of the <b>RefTuple</b>.
		/// If <b>forceDataRef</b> is true, then any <b>IsDataReference</b> references are forced to resolve to a specific
		/// data item when an incomplete identifier is specified. 
		/// </summary>
		/// <param name="start">The starting Reference.</param>
		/// <param name="tupleEnum">A <b>RefTuple</b> enumerator positioned at the starting tuple.</param>
		/// <param name="forceDataRef">Force resolution to a specific data item.</param>
		/// <returns>The specified reference</returns>
		/// <seealso cref="FindItem(string)"/>
		/// <seealso cref="RefParser"/>
		/// <seealso cref="RefTuple"/>
		protected IUltraCalcReference RefLoop( RefBase start, IEnumerator tupleEnum, bool forceDataRef )
		{
			RefTuple tuple = null;
			IUltraCalcReference nextElement = start;

			while ( tupleEnum.MoveNext() )
			{
				RefBase activeElement = nextElement as RefBase;

				// get a reference to the element we're talking about
				tuple = (RefTuple)tupleEnum.Current;
				switch ( tuple.Type )
				{
					case RefTuple.RefType.Parent:
						if ( tuple.Scope != RefTuple.RefScope.Any )
						{
							return new UltraCalcReferenceError( null, SR.GetString( "Error_UnexpectedScope", new Object[] { GetType().ToString() + " " + ElementName, tuple.Scope } ) );
						}
						nextElement = activeElement.FindParent();
						break;

					case RefTuple.RefType.ExplicitThis:
						return new UltraCalcReferenceError( null, SR.GetString( "Error_ExplicitThis", new Object[] { GetType().ToString() + " " + ElementName } ) );

					case RefTuple.RefType.Identifier:
						switch ( tuple.Scope )
						{
							case RefTuple.RefScope.Any:
								nextElement = activeElement.FindItem( tuple.Name );
								break;
						case RefTuple.RefScope.All:
							if ( tuple.Last )
								nextElement = activeElement.FindAll( tuple.Name );
							else
								return new UltraCalcReferenceError( null, SR.GetString( "Error_ScopeAllNotLast", new Object[] { GetType().ToString() + " " + ElementName, tuple.Name } ) );
							break; 
						case RefTuple.RefScope.SummaryValue:
							nextElement = activeElement.FindSummaryItem( tuple.Name );
							break;
						case RefTuple.RefScope.Identifier:
							nextElement = activeElement.FindItem( tuple.Name,tuple.ScopeID );
							break;
						case RefTuple.RefScope.Index:
							nextElement = activeElement.FindItem( tuple.Name, tuple.ScopeIndex, false );
							break;
						case RefTuple.RefScope.RelativeIndex:
							nextElement = activeElement.FindItem( tuple.Name, tuple.ScopeIndex, true );
							break; 
							default:
								return new UltraCalcReferenceError( null, SR.GetString( "Error_UnknownScope", new Object[] { GetType().ToString() + " " + ElementName, tuple.Scope } ) );
						}
						break;

					default:
						return new UltraCalcReferenceError( null, SR.GetString( "Error_UnknownRefType", new Object[] { GetType().ToString() + " " + ElementName, tuple.Type } ) );
				}

				if ( forceDataRef
					&& nextElement.IsDataReference == false  
					&& !( nextElement is UltraCalcReferenceError ) )
				{
					nextElement = activeElement.FindItem( tuple.Name, 0, false );
				}

				if ( nextElement is UltraCalcReferenceError )
					return nextElement;
			}

			return nextElement;
		} 

		#region  Implementation of IUltraCalcReference

		/// <summary>
		/// The fully qualified unique name for the referenced element.  Read Only.
		/// </summary>
		public virtual string AbsoluteName
		{
			get 
			{
				return BaseParent.AbsoluteName + RefParser.RefSeperator + ElementName; 
			}
		}

		// SSP 9/7/04
		// Added NormalizedAbsoluteName property as a part of case insensitive absolute
		// name implementation.
		//
		#region NormalizedAbsoluteName

		/// <summary>
		/// Returns the normalized absolute name. Calculation engine makes use of normalized
		/// absolute names of references to compare two references and search for references.
		/// This property should return the absolute name with case insensitive parts of
		/// the absolute names converted to lower case.
		/// </summary>
		public virtual string NormalizedAbsoluteName
		{
			get
			{
				return this.ParsedReference.ToString();
			}
		} 

		#endregion // NormalizedAbsoluteName

		/// <summary>
		/// The unqualified name of this referenced element.  Used to construct the fully qualified name by the
		/// <b>AbsoluteName</b> method. Read Only.
		/// </summary>
		/// <seealso cref="AbsoluteName"/>
		public abstract string ElementName {get;}

		/// <summary>
		/// The <see cref="UltraCalcValue"/>, if any, associated with this Reference.  If this reference 
		/// does not contain a Value then a <see cref="UltraCalcErrorValue"/> is returned.
		/// </summary>
		public virtual UltraCalcValue Value
		{
			get
			{
				return new UltraCalcValue( new UltraCalcErrorValue( UltraCalcErrorCode.Value ) );
			}

			set
			{
				throw new UltraCalcException( SR.GetString( "Error_ReadOnlyReference",new Object[] {GetType().ToString() + " " + ElementName}) );
			}
		}

		/// <summary>
		/// The <see cref="IUltraCalcFormula"/>, if any, associated with this Reference.  If this reference 
		/// can not contain a formula then null is returned.
		/// </summary>
		public virtual IUltraCalcFormula Formula
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Flag used by the calculation engine to manage the recalculation network. 
		/// Denotes the reference is visible.
		/// </summary>
		public virtual bool RecalcDeferred
		{
			get { return this.formulaStates.RecalcDeferred; }
			set { this.formulaStates.RecalcDeferred = value; }
		}


		/// <summary>
		/// Flag used by the calculation engine to manage the recalculation network. 
		/// Denotes the reference is visible.
		/// </summary>
		public virtual bool RecalcVisible
		{
			get { return this.formulaStates.RecalcVisible; }
			set { this.formulaStates.RecalcVisible = value; }
		}

		/// <summary>
		/// True indicates that the reference was constructed with a relative index.  Read Only.
		/// </summary>
		public virtual bool HasRelativeIndex
		{
			get { return false; }
		}

		/// <summary>
		/// True indicates that the reference was constructed with a relative index.  Read Only.
		/// </summary>
		public virtual bool HasAbsoluteIndex
		{
			get { return false; }
		}

		/// <summary>
		/// True indicates that the reference was constructed with a scope all index.  Read Only.
		/// </summary>
		public virtual bool HasScopeAll
		{
			get { return false; }
		} 

		/// <summary>
		/// For each reference tuple in this reference that has a relative index, mark the
		/// corresponding tuple in inReference.
		/// </summary>
		/// <param name="inReference">The Reference to be marked.</param>
		public virtual void MarkRelativeIndices(IUltraCalcReference inReference)
		{
			// if it's not a grid reference, just return the reference
			if( !(inReference is RefBase) ) return;
			this.ParsedReference.MarkRelativeIndices( ((RefBase)inReference).ParsedReference,false );
		}

		/// <summary>
		/// Create a new reference relative to this reference.
		/// </summary>
		/// <param name="inReference">The reference string.</param>
		/// <returns>The new reference.</returns>
		public virtual IUltraCalcReference CreateReference( string inReference )
		{
			try
			{
				// If it's fully qualified, we need to start at the root.
				RefParser rp = new RefParser( inReference );

				if ( rp.TupleCount == 0 )
				{
					throw new UltraCalcException( SR.GetString( "Error_RefMalformed", new Object[] { GetType().ToString() + " " + ElementName, inReference } ) );
				}

				if ( rp.IsFullyQualified || rp.IsRoot )
				{
					RefBase rootReference = this.FindRoot();
					if ( rootReference != this )
						return rootReference.CreateReference( inReference );
				}  

				IUltraCalcReference builtReference = this.BuildReference( rp, false );

				if ( builtReference is UltraCalcReferenceError )
				{
					return new UltraCalcReferenceError(inReference, ((UltraCalcReferenceError)builtReference).Message);
				}
				else if ( builtReference == null )
					return new UltraCalcReferenceError( inReference );

				RefBase result = builtReference as RefBase;
				if ( result.IsAnchored )
					result = new RefUnAnchored( result );

				result.RelativeReference = rp;
				return result;
			}
			catch ( Exception e )
			{
				return new UltraCalcReferenceError( inReference, e.Message );
			}

		} 

		/// <summary>
		/// Create a Range reference relative to this reference.
		/// </summary>
		/// <param name="fromReference">The start of the range, inclusive.</param>
		/// <param name="toReference">The end of the range, inclusive.</param>
		/// <returns>A RangeReference</returns>
		public virtual IUltraCalcReference CreateRange( string fromReference, string toReference )
		{
			// SSP 9/16/04
			// Enclosed the existing code in try-catch block so if the range reference was
			// incorrect then we would return a reference error instead of blowing up which
			// is what the constructor of RefRange does.
			//
			try
			{
				IUltraCalcReference fromR = CreateReference( fromReference );
				if ( fromR is UltraCalcReferenceError )
					return fromR;
				if ( !( fromR is RefBase ) )
				{
					return fromR.CreateRange( fromReference, toReference );
				}


				IUltraCalcReference toR = CreateReference( toReference );
				if ( toR is UltraCalcReferenceError )
					return toR;
				if ( !( toR is RefBase ) )
				{
					return new UltraCalcReferenceError( toReference, SR.GetString( "Error_ExternalRange" ) );
				}

				return new RefRange( (RefBase)fromR, (RefBase)toR );
			}
			catch ( Exception e )
			{
				// SSP 9/18/04
				// Changed the range separator from ".." to ":". Added the RangeSeparator and
				// RangeSeparatorWithSpaces constants. Before the range separator was hard-coded in
				// multiple places.
				//
				//return new UltraCalcReferenceError( fromReference + ".." + toReference, e.Message );
				return new UltraCalcReferenceError( fromReference + RefParser.RangeSeparator + toReference, e.Message );
			}
		} 

		/// <summary>
		/// Resolves a reference relative to this reference.  For example, if this
		/// reference is a reference to a row, and inReference is a reference to a
		/// column, the resolved reference is the cell reference that is the intersection
		/// of this row and the input column.
		/// </summary>
		/// <param name="inReference">The reference to resolve.</param>
		/// <param name="referenceType"></param>
		/// <returns>The resolved reference.</returns>
		public virtual IUltraCalcReference ResolveReference( IUltraCalcReference inReference, ResolveReferenceType referenceType )
		{
			try
			{
				// if it's not a grid reference, just return the reference
				if ( !( inReference is RefBase ) )
					return inReference;

				if ( ( (RefBase)inReference ).IsRange )
				{
					if ( inReference is RefRange )
					{
						IUltraCalcReference fromUCR = this.ResolveReference( ( (RefRange)inReference ).FromRef, referenceType );
						IUltraCalcReference toUCR = this.ResolveReference( ( (RefRange)inReference ).ToRef, referenceType );

						if ( !( fromUCR is RefBase ) )
							return new UltraCalcReferenceError( inReference.ToString(), SR.GetString( "Error_RangeFromRefBase", fromUCR.ToString() ) );

						if ( !( toUCR is RefBase ) )
							return new UltraCalcReferenceError( inReference.ToString(), SR.GetString( "Error_RangeFromRefBase", toUCR.ToString() ) );

						return new RefRange( (RefBase)fromUCR, (RefBase)toUCR );
					}
					else
					{
						return new UltraCalcReferenceError( inReference.ToString(), SR.GetString( "Error_UnknownRange" ) );
					}
				}
				else  
				{
					RefParser newRP = new RefParser();
					newRP.Merge( this.ParsedReference, ( (RefBase)inReference ).ParsedReference, true );

					bool forceDataRef = this.IsDataReference && newRP.HasSummaryScope; 

					IUltraCalcReference builtReference = null;

					if ( newRP.IsFullyQualified || newRP.IsRoot )
						builtReference = FindRoot().BuildReference( newRP, forceDataRef );
					else  
						builtReference = this.BuildReference( newRP, forceDataRef );

					if ( builtReference is UltraCalcReferenceError )
						return new UltraCalcReferenceError( inReference.ToString(), ( (UltraCalcReferenceError)builtReference ).Message );
					else if ( builtReference == null )
						return new UltraCalcReferenceError( inReference.ToString() );

					RefBase result = builtReference as RefBase;

					if ( result.IsAnchored )
						result = new RefUnAnchored( result );

					result.RelativeReference = newRP;
					return result;
				}
			}
			catch ( Exception e )
			{
				return new UltraCalcReferenceError( inReference.ToString(), e.Message );
			}
		} 

		/// <summary>
		/// Return a collection of references if this Reference is enumerable.  An exception is
		/// thrown if this reference is not enumerable.
		/// </summary>
		/// <returns>A Reference collection.</returns>
		public virtual IUltraCalcReferenceCollection References
		{
			get
			{
				//Debug.Assert(false, "This object does not return any references.");

				//throw new UltraCalcException( SR.GetString( "Error_ReferenceNotEumerable",new Object[] {GetType().ToString() + " " + ElementName}) );
				return null;
			}
		}

		/// <summary>
		/// Returns true if this reference contains inReference
		/// </summary>
		/// <param name="inReference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public virtual bool ContainsReference( IUltraCalcReference inReference )
		{
			if ( !( inReference is RefBase ) && !( inReference is UCReference ) )
				return false;

			RefParser inputParsed = null;
			if ( inReference is UCReference )
			{
				// SSP 6/20/11
				// Use the new helper method in the utils.
				// 
				//if ( String.Compare( this.FindRoot().ElementName, ( (UCReference)inReference ).RootName, true ) == 0 )
				if ( RefParser.AreStringsEqual( this.FindRoot( ).ElementName, ( (UCReference)inReference ).RootName, true ) )
				{
					IUltraCalcReference wrappedR = ( (UCReference)inReference ).Reference;
					if ( wrappedR is RefBase )
					{
						inputParsed = ( (RefBase)wrappedR ).ParsedReference;
					}
					else if ( wrappedR is UCReference )
					{
						inputParsed = new RefParser( wrappedR.AbsoluteName );
					}
				}
			}
			else
			{
				inputParsed = ( (RefBase)inReference ).ParsedReference;
			}

			if ( inputParsed != null )
			{
				RefParser baseParsed = this.ParsedReference;
				return baseParsed.Contains( inputParsed );
			}

			return false;

			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		} 

		/// <summary>
		/// Returns true if inReference is a proper subset of this reference
		/// </summary>
		/// <param name="inReference">The subset candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public virtual bool IsSubsetReference( IUltraCalcReference inReference )
		{
			if ( !( inReference is RefBase ) && !( inReference is UCReference ) )
				return false;

			RefParser inputParsed = null;
			if ( inReference is UCReference )
			{
				// SSP 6/20/11
				// Use the new helper method in the utils.
				// 
				//if ( String.Compare( this.FindRoot().ElementName, ( (UCReference)inReference ).RootName, true ) == 0 ) 
				if ( RefParser.AreStringsEqual( this.FindRoot( ).ElementName, ( (UCReference)inReference ).RootName, true ) ) 
				{
					IUltraCalcReference wrappedR = ( (UCReference)inReference ).Reference;
					if ( wrappedR is RefBase )
					{
						inputParsed = ( (RefBase)wrappedR ).ParsedReference;
					}
					else if ( wrappedR is UCReference )
					{
						inputParsed = new RefParser( wrappedR.AbsoluteName );
					}
				}
			}
			else
			{
				inputParsed = ( (RefBase)inReference ).ParsedReference;
			}

			if ( inputParsed != null )
			{
				RefParser baseParsed = this.ParsedReference;
				return baseParsed.IsSubset( inputParsed );
			}

			return false;

		} 

		/// <summary>
		/// Returns true if this reference is enumerable.
		/// </summary>
		public virtual bool IsEnumerable 
		{ 
			get 
			{
				return false;
			}
		}

		/// <summary>
		/// The RefBase Parent reference of this referenced element.  Used to navigate the element hierarchy.  If this
		/// referenced element is the root element, <b>BaseParent</b> returns null.  Read Only.
		/// </summary>
		public virtual IUltraCalcReference Parent
		{
			get
			{
				return this.BaseParent;
			}
		} 

		/// <summary>
		/// Returns true if this reference is a data reference.  A data reference contains a value, such as a Grid Cell or
		/// a Grid SummaryValue.  Many referenced elements, such as a Grid Column or a Grid Band, do not reference a value.
		/// Read Only. 
		/// </summary>
		public virtual bool IsDataReference {get {return false;} }

		/// <summary>
		/// Determines whether the given reference is a sibling of this reference
		/// </summary>
		/// <param name="reference">The reference to compare against this one</param>
		/// <returns>True if the reference is a sibling of the given reference</returns>
		public virtual bool IsSiblingReference( IUltraCalcReference reference )
		{
			// SSP 10/12/04
			// Return false by default. The overriding class will have to override this method.
			//
			
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)


			return false;
		} 

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public virtual bool IsDisposedReference
		{
			get { return false; }
		}

		#endregion

		/// <summary>
		/// Returns a string representation of the reference based on the reference type and absolute name.
		/// </summary>
        /// <returns>A string representation of the reference based on the reference type and absolute name.</returns>
        public override string ToString()
		{
			return this.GetType().ToString() + "-" + this.AbsoluteName;
		}

		// SSP 9/7/04
		// Added GetChildReferences method to IUltraCalcReference interface and 
		// ReferenceType enum which the added method takes as a parameter.
		//
		/// <summary>
		/// Returns the child references of the reference. This method can return null.
		/// </summary>
		/// <param name="referenceType">The <see cref="ChildReferenceType"/> to retrieve.</param>
		/// <returns>An array of <see cref="IUltraCalcReference"/> objects that represent the child references.</returns>
		public virtual IUltraCalcReference[] GetChildReferences( ChildReferenceType referenceType )
		{
			return null;
		} 

		/// <summary>
		/// Used to create a new <see cref="RefParser"/> for the reference.
		/// </summary>
		protected virtual RefParser CreateParsedReference()
		{
			return new RefParser( this.AbsoluteName );
		}

		// SSP 10/12/04
		// Overrode GetHashCode and Equals methods.
		//
		#region GetHashCode

		/// <summary>
		/// Overridden. Default implementation returns the hash code of the normalized absolute name.
		/// These objects are going to be added to data structure that make use of GetHashCode and 
		/// Equals methods. What this means is that the normalized absolute name must not change 
		/// throughout the lifetime of this instance. If it does change then the overriding class 
		/// must override this method and return a hash code based on some other immutable property 
		/// of the instance.
		/// </summary>
        /// <returns>A hash code of the normalized absolute name.</returns>
		public override int GetHashCode( )
		{
			return this.NormalizedAbsoluteName.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		/// <summary>
		/// Overriden. Default implementation returns true if the normalized absolute names of this
		/// reference and the passed in reference match. These objects are going to be added to data 
		/// structure that make use of GetHashCode and Equals methods. What this means is that the 
		/// normalized absolute name must not change throughout the lifetime of this instance. If it 
		/// does change then the overriding class must override this method and return a hash code 
		/// based on some other immutable property of the instance.
		/// </summary>
        /// <param name="obj">The reference to compare with this instance.</param>
        /// <returns>True if the normalized absolute names of this reference and the passed in reference match</returns>
		public override bool Equals( object obj )
		{
			IUltraCalcReference reference = obj as IUltraCalcReference;
			return null != reference && reference.NormalizedAbsoluteName.Equals( this.NormalizedAbsoluteName );
		}

		#endregion // Equals

		#endregion //Methods
	}

	#endregion

	#region RefRange

	/// <summary>
	/// RefRange is a generic Range Reference
	/// </summary>
    public class RefRange : RefBase, IUltraCalcReferenceCollection
	{
		#region Member Variables
		/// <summary>
		/// The start of the range, not normalized, inclusive.
		/// </summary>
		protected RefBase from;

		/// <summary>
		/// The end of the range, not normalized,  inclusive.
		/// </summary>
		protected RefBase to;

		/// <summary>
		/// The normalized start of the range, inclusive.
		/// </summary>
		protected RefBase normalizedFrom;

		/// <summary>
		/// The normalized end of the range, inclusive.
		/// </summary>
		protected RefBase normalizedTo;

		// AS 9/9/04 UWC81 Delay normalizing the references.
		private bool isNormalized = false;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RefRange"/>
		/// </summary>
		/// <param name="from">The start of the range, inclusive.</param>
		/// <param name="to">The end of the range, inclusive.</param>
		public RefRange( RefBase from, RefBase to )
		{
			if ( from == null )
				throw new UltraCalcException( SR.GetString( "Error_RangeFromNull" ) );

			if ( to == null )
				throw new UltraCalcException( SR.GetString( "Error_RangeToNull" ) );

			this.from = from;
			this.to = to;
			Validate();
		}

		#endregion //Constructor

		#region VerifyNormalized
		// AS 9/9/04 UWC81
		// Delay normalizing the references.
		private void VerifyNormalized()
		{
			if ( !this.isNormalized )
			{
				// get the ref parser as was done in the Validate
				RefParser fromRP = this.from.ParsedReference;
				RefParser toRP = this.to.ParsedReference;

				try
				{
					this.Normalize( fromRP, toRP );
				}
				catch { }
			}
		}
		#endregion //VerifyNormalized

		#region Properties

		/// <summary>
		/// Return the From reference as a Refbase.
		/// </summary>
		public RefBase FromBase { get { return this.from; } }

		/// <summary>
		/// Return the Normalized From reference as a Refbase.
		/// </summary>
		public RefBase NormalizedFromBase
		{
			get
			{
				// AS 9/9/04 UWC81 Delay normalizing the references.
				this.VerifyNormalized();

				return this.normalizedFrom;
			}
		}

		/// <summary>
		/// Return the From reference as a <see cref="IUltraCalcReference"/>.
		/// </summary>
		public IUltraCalcReference FromRef { get { return (IUltraCalcReference)this.from; } }

		/// <summary>
		/// Return the normalized From reference as a <see cref="IUltraCalcReference"/>.
		/// </summary>
		public IUltraCalcReference NormalizedFromRef
		{
			get
			{
				// AS 9/9/04 UWC81 Delay normalizing the references.
				this.VerifyNormalized();

				return (IUltraCalcReference)this.normalizedFrom;
			}
		}

		/// <summary>
		/// Return the To reference as a Refbase.
		/// </summary>
		public RefBase ToBase { get { return this.to; } }

		
		
		
		
		/// <summary>
		/// Return the normalized To reference as a <see cref="RefBase"/>.
		/// </summary>
		public RefBase NormalizedToBase
		{
			get
			{
				// AS 9/9/04 UWC81 Delay normalizing the references.
				this.VerifyNormalized();

				return this.normalizedTo;
			}
		}

		/// <summary>
		/// Return the To reference as a <see cref="IUltraCalcReference"/>.
		/// </summary>
		public IUltraCalcReference ToRef { get { return (IUltraCalcReference)this.to; } }

		/// <summary>
		/// Return the normalized To reference as a <see cref="IUltraCalcReference"/>.
		/// </summary>
		public IUltraCalcReference NormalizedToRef
		{
			get
			{
				// AS 9/9/04 UWC81 Delay normalizing the references.
				this.VerifyNormalized();

				return (IUltraCalcReference)this.normalizedTo;
			}
		}

		#endregion //Properties

		#region Validate
		/// <summary>
		/// Validates the Range.  This implementation requires that a Range is constrained to the last
		/// tuple in the Reference Path.
		/// </summary>
		protected virtual void Validate()
		{
			RefParser fromRP = this.from.ParsedReference;
			RefParser toRP = this.to.ParsedReference;

			bool allScopeDetected = fromRP.HasScopeAll | toRP.HasScopeAll;

			if ( fromRP.TupleCount != toRP.TupleCount )
				throw new UltraCalcException( SR.GetString( "Error_RangeValidateCount", new Object[] { fromRP.TupleCount, toRP.TupleCount } ) );

			int index = 0;
			foreach ( RefTuple fromT in fromRP )
			{
				RefTuple toT = toRP[ index ];
				if ( fromT.Last )
				{
					if ( !fromT.IsSameIdentifier( toT ) )
						throw new UltraCalcException( SR.GetString( "Error_RangeLast", new Object[] { fromT.ToString(), toT.ToString() } ) );

					// AS 9/9/04 UWC81 Delay normalizing the references.
					//Normalize( fromRP,toRP );
					return;
				}

				// the next to last tuple only needs the same identifier since we're iterating over the same column value in multiple rows
				if ( toRP[ index + 1 ].Last || allScopeDetected )
				{
					if ( !fromT.IsSameIdentifier( toT ) )
						throw new UltraCalcException( SR.GetString( "Error_RangeTuple", new Object[] { fromT.ToString(), toT.ToString() } ) );
				}
				else if ( !fromT.IsSameTuple( toT ) )
				{
					throw new UltraCalcException( SR.GetString( "Error_RangeTuple", new Object[] { fromT.ToString(), toT.ToString() } ) );
				}
				++index;
			}
		}
		#endregion //Validate

		#region Normalize
		private void Normalize( RefParser fromRP, RefParser toRP )
		{
			RefParser newFrom = null;
			NormalizeOne( fromRP, ref newFrom );

			RefParser newTo = null;
			NormalizeOne( toRP, ref newTo );

			IUltraCalcReference newFromR = CreateReference( newFrom.ToString() );
			if ( !( newFromR is RefBase ) )
			{
				throw new UltraCalcException( SR.GetString( "Error_RangeFromRefBase", new Object[] { newFromR.ToString() } ) );
			}

			IUltraCalcReference newToR = CreateReference( newTo.ToString() );
			if ( !( newToR is RefBase ) )
			{
				throw new UltraCalcException( SR.GetString( "Error_RangeFromRefBase", new Object[] { newToR.ToString() } ) );
			}

			if ( fromRP.HasScopeAll || toRP.HasScopeAll )
			{
				if ( ( fromRP.HasScopeAll && toRP.HasScopeAll ) || toRP.HasScopeAll )
				{
					this.normalizedFrom = (RefBase)newFromR;
					this.normalizedTo = (RefBase)newToR;
				}
				else
				{
					this.normalizedFrom = (RefBase)newToR;
					this.normalizedTo = (RefBase)newFromR;
				}
			}
			else if ( fromRP.TupleCount <= 1 )
			{
				this.normalizedFrom = (RefBase)newFromR;
				this.normalizedTo = (RefBase)newToR;
			}
			else
			{
				int compareIndex = fromRP.TupleCount - 1;

				if ( newFrom.LastTuple.Scope != RefTuple.RefScope.RelativeIndex ) 
				{
					--compareIndex;
				}

				if ( fromRP[ compareIndex ].IsNumericScopeLTE( toRP[ compareIndex ] ) )
				{
					this.normalizedFrom = (RefBase)newFromR;
					this.normalizedTo = (RefBase)newToR;
				}
				else
				{
					this.normalizedFrom = (RefBase)newToR;
					this.normalizedTo = (RefBase)newFromR;
				}
			}

			// AS 9/9/04 UWC81 Update flag to indicate the normalization is done.
			this.isNormalized = true;
		}
		#endregion //Normalize

		#region NormalizeOne
		// MD 6/28/07 - 7.3 Performance
		// FxCop - Mark members as static
		//private void NormalizeOne( RefParser inRP, ref RefParser outRP )
		private static void NormalizeOne( RefParser inRP, ref RefParser outRP )
		{
			RefParser cloneRP = new RefParser( inRP );

			if ( inRP.TupleCount <= 1 || cloneRP.HasScopeAll )
			{
				outRP = cloneRP;
				return;
			}

			RefTuple cloneNL = cloneRP[ cloneRP.TupleCount - 2 ];

			switch ( cloneNL.Scope )
			{
				case RefTuple.RefScope.Index:
					if ( cloneRP.LastTuple.Scope == RefTuple.RefScope.Index )
					{
						cloneNL.ScopeIndex = cloneRP.LastTuple.ScopeIndex;
						cloneRP.LastTuple.Scope = RefTuple.RefScope.Any;
					}
					break;

				case RefTuple.RefScope.Any:
					if ( cloneRP.LastTuple.Scope == RefTuple.RefScope.Index )
					{
						cloneNL.Scope = RefTuple.RefScope.Index;
						cloneNL.ScopeIndex = cloneRP.LastTuple.ScopeIndex;
						cloneRP.LastTuple.Scope = RefTuple.RefScope.Any;
					}
					break;

				default:
					throw new UltraCalcException( SR.GetString( "Error_RangeNormalizeScope", new Object[] { cloneNL.ToString(), cloneRP.LastTuple.ToString() } ) );

			} 

			outRP = cloneRP;
		}
		#endregion //NormalizeOne

		#region Contains
		/// <summary>
		/// Returns true if targetRP is contained in this Range.
		/// </summary>
		/// <param name="targetRP">The candidate contained range.</param>
		/// <returns>true if targetRP is contained in this range.</returns>
		public bool Contains( RefParser targetRP )
		{
			// AS 9/9/04 UWC81 Delay normalizing the references.
			this.VerifyNormalized();

			// if the reference could not be obtained, return false
			if ( !this.isNormalized )
				return false;

			RefParser fromRP = this.normalizedFrom.ParsedReference;
			RefParser toRP = this.normalizedTo.ParsedReference;

			if ( fromRP.TupleCount != targetRP.TupleCount )
				return false;

			int index = 0;

			// generate index of tuple to compare.  In multi-tuple RPs that don't end in a relative index, 
			// it's the next-to-last tuple (row in row/column).  In in multi-tuple RPs that do end in a 
			// relative index or in single-tuple RPs, it's the last tuple.
			int compareIndex = fromRP.TupleCount - 1;
			if ( fromRP.LastTuple.Scope != RefTuple.RefScope.RelativeIndex && compareIndex > 0 )
			{
				--compareIndex;
			}

			foreach ( RefTuple fromT in fromRP )
			{
				RefTuple targetT = targetRP[ index ];
				if ( !fromT.IsSameIdentifier( targetT ) )
				{
					return false;
				}

				if ( index == compareIndex )
				{
					if ( targetT.Last == false && fromRP.LastTuple.IsSameIdentifier( targetRP.LastTuple ) == false )
					{
						return false;
					}
					return targetT.IsNumericScopeGTE( fromT ) && ( toRP.HasScopeAll || targetT.IsNumericScopeLTE( toRP[ index ] ) );
				}

				++index;
			}
			throw new UltraCalcException( SR.GetString( "Error_RangeFromLast" ) );
		}
		#endregion //Contains

		#region RefRange Overrides of RefBase

		/// <summary>
		/// Returns true. This reference is a Range Reference.  Read Only.
		/// </summary>
		/// <seealso cref="RefRange"/>
		public override bool IsRange { get { return true; } }

		/// <summary>
		/// The unqualified name of this referenced element.  Used to construct the fully 
		/// qualified name by the <b>AbsoluteName</b> method. Read Only.
		/// <p>The ElementName for a Range is from.ElementName + ":" + to.ElementName
		/// </p>
		/// </summary>
		/// <seealso cref="AbsoluteName"/>
		public override string ElementName
		{
			get
			{
				// SSP 9/18/04
				// Changed the range separator from ".." to ":". Added the RangeSeparator and
				// RangeSeparatorWithSpaces constants. Before the range separator was hard-coded in
				// multiple places.
				//
				
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

				string fromRefName = null != this.from ? this.from.ElementName : "???";
				string toRefName = null != this.to ? this.to.ElementName : "???";
				return fromRefName + RefParser.RangeSeparatorWithSpaces + toRefName;
			}
		}

		/// <summary>
		/// The Parent reference of this referenced element.  Used to navigate the element hierarchy.  If this
		/// referenced element is the root element, <b>Parent</b> returns null.  Read Only.
		/// <p>The Parent of a Range is this.from
		/// </p>
		/// </summary>
		public override RefBase BaseParent { get { return this.from; } } 

		/// <summary>
		/// Returns true if this reference is a data reference.  A data reference contains a value, such as a Grid Cell or
		/// a Grid SummaryValue.  Many referenced elements, such as a Grid Column or a Grid Band, do not reference a value.
		/// Read Only. 
		/// <p>A Range is always a Data Reference.
		/// </p>
		/// </summary>
		public override bool IsDataReference {get {return true;} }

		/// <summary>
		/// Used to create a new <see cref="RefParser"/> for the reference.
		/// </summary>
		protected override RefParser CreateParsedReference()
		{
			return this.from.ParsedReference;
		}

		/// <summary>
		/// Returns the named reference relative to this reference.
		/// <p>This always throws an exception.  This method should never be called on a Range because 
		/// a Range is never created by BuildReference
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		public override IUltraCalcReference FindItem( string name )
		{
			return new UltraCalcReferenceError( null, SR.GetString( "Error_MethodNotSupported", new Object[] { GetType().ToString(), "FindItem( " + name + " )" } ) );
		}

		/// <summary>
		/// Returns the named reference relative to this reference with scope "All"
		/// <p>This always throws an exception.  This method should never be called on a Range because 
		/// a Range is never created by BuildReference
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		public override IUltraCalcReference FindAll( string name )
		{
			return new UltraCalcReferenceError( null, SR.GetString( "Error_MethodNotSupported", new Object[] { GetType().ToString(), "FindAll( " + name + " )" } ) );
		}

		/// <summary>
		/// Returns the named SummaryItem reference relative to this reference.
		/// <p>This always throws an exception.  This method should never be called on a Range because 
		/// a Range is never created by BuildReference
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		public override IUltraCalcReference FindSummaryItem( string name )
		{
			return new UltraCalcReferenceError( null, SR.GetString( "Error_MethodNotSupported", new Object[] { GetType().ToString(), "FindSummaryItem( " + name + " )" } ) );
		}

		/// <summary>
		/// Returns the named indexed reference relative to this reference.
		/// <p>This always throws an exception.  This method should never be called on a Range because 
		/// a Range is never created by BuildReference
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <param name="index">The index name.</param>
		/// <returns>The referenced element</returns>
		public override IUltraCalcReference FindItem( string name, string index )
		{
			return new UltraCalcReferenceError( null, SR.GetString( "Error_MethodNotSupported", new Object[] { GetType().ToString(), "FindItem( " + name + "," + index + " )" } ) );
		}

		/// <summary>
		/// Returns the named indexed reference relative to this reference.  If <b>isRelative</b>
		/// is true, then <b>index</b> is treated as a relative to this element index.  Otherwise the
		/// index is treated as absolute.
		/// <p>This always throws an exception.  This method should never be called on a Range because 
		/// a Range is never created by BuildReference
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <param name="index">The index value.</param>
		/// <param name="isRelative">true if <b>index</b> is relative to this index</param>
		/// <returns>The referenced element</returns>
		public override IUltraCalcReference FindItem( string name, int index, bool isRelative )
		{
			string prefix = "";
			if ( isRelative )
			{
				if ( index < 0 )
					prefix = "-";
				else
					prefix = "+";
			}
			return new UltraCalcReferenceError( null, SR.GetString( "Error_MethodNotSupported", new Object[] { GetType().ToString(), "FindItem( " + name + "," + prefix + index + " )" } ) );
		}

		/// <summary>
		/// Returns a <see cref="IUltraCalcReferenceCollection"/> for enumerable references constrained to the specified reference.
		/// <p>A Range returns a RefRangeCollection.
		/// </p>
		/// </summary>
		/// <param name="scopeRP">The constraining reference</param>
		/// <returns>A RefRangeCollection</returns>
		public override IUltraCalcReferenceCollection ScopedReferences( RefParser scopeRP )
		{
			return this;
		}

		#endregion

		#region RefRange Implementation of IUltraCalcReference

		/// <summary>
		/// The fully qualified unique name for the referenced element.  Read Only.
		/// <p>The AbsoluteName for a Range is from.AbsoluteName + ":" + to.AbsoluteName
		/// </p>
		/// </summary>
		public override string AbsoluteName
		{
			get
			{
				// SSP 9/18/04
				// Changed the range separator from ".." to ":". Added the RangeSeparator and
				// RangeSeparatorWithSpaces constants. Before the range separator was hard-coded in
				// multiple places.
				//
				//return this.from.AbsoluteName + " .. " + this.to.AbsoluteName;
				return this.from.AbsoluteName + RefParser.RangeSeparatorWithSpaces + this.to.AbsoluteName;
			}
		}

		// SSP 9/7/04
		// Added NormalizedAbsoluteName property as a part of case insensitive absolute
		// name implementation.
		//
		#region NormalizedAbsoluteName

		/// <summary>
		/// Returns the normalized absolute name. Calculation engine makes use normalized
		/// absolute names of references to compare two references and search for references.
		/// This property should return the absolute name with case insensitive parts of
		/// the absolute names converted to lower case.
		/// </summary>
		public override string NormalizedAbsoluteName
		{
			get
			{
				// SSP 9/18/04
				// Changed the range separator from ".." to ":". Added the RangeSeparator and
				// RangeSeparatorWithSpaces constants. Before the range separator was hard-coded in
				// multiple places.
				//
				//return this.from.NormalizedAbsoluteName + " .. " + this.to.NormalizedAbsoluteName;
				return this.from.NormalizedAbsoluteName + RefParser.RangeSeparatorWithSpaces + this.to.NormalizedAbsoluteName;
			}
		}

		#endregion // NormalizedAbsoluteName

		/// <summary>
		/// True indicates that the reference was constructed with a relative index.  Read Only.
		/// <p>The HasRelativeIndex for a Range is from.HasRelativeIndex
		/// </p>
		/// </summary>
		public override bool HasRelativeIndex
		{
			get { return true; }
		}

		/// <summary>
		/// For each reference tuple in this reference that has a relative index, mark the
		/// corresponding tuple in inReference.
		/// </summary>
		/// <param name="inReference">The Reference to be marked.</param>
		public override void MarkRelativeIndices( IUltraCalcReference inReference )
		{
			// if it's not a grid reference, just return the reference
			if ( !( inReference is RefBase ) )
				return;

			this.ParsedReference.MarkRelativeIndices( ( (RefBase)inReference ).ParsedReference, true );
		} 

		/// <summary>
		/// Resolves a reference relative to this reference.  For example, if this
		/// reference is a reference to a row, and inReference is a reference to a
		/// column, the resolved reference is the cell reference that is the intersection
		/// of this row and the input column.
		/// <p>ResolveReference for a Range is from.ResolveReference
		/// </p>
		/// </summary>
		/// <param name="inReference">The reference to resolve.</param>
		/// <param name="referenceType">Enumeration indicating the type of reference to resolve</param>
		/// <returns>The resolved reference.</returns>
		public override IUltraCalcReference ResolveReference( IUltraCalcReference inReference, ResolveReferenceType referenceType )
		{
			return this.from.ResolveReference( inReference, referenceType );
		}

		/// <summary>
		/// Return a collection of references if this Reference is enumerable.  An exception is
		/// thrown if this reference is not enumerable.
		/// <p>A Range returns a RefRangeCollection.
		/// </p>
		/// </summary>
		/// <returns>RefRangeCollection.</returns>
		public override IUltraCalcReferenceCollection References
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// Returns true if this Range contains inReference
		/// </summary>
		/// <param name="inReference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this range.</returns>
		public override bool ContainsReference( IUltraCalcReference inReference )
		{
			//			if( !(inReference is RefBase) ) return false;
			if ( !( inReference is RefBase ) && !( inReference is UCReference ) )
				return false;

			RefBase inBase = null;
			if ( inReference is UCReference )
			{
				// SSP 6/20/11
				// Use the new helper method in the utils.
				// 
				//if ( String.Compare( this.FindRoot().ElementName, ( (UCReference)inReference ).RootName, true ) == 0 ) 
				if ( RefParser.AreStringsEqual( this.FindRoot( ).ElementName, ( (UCReference)inReference ).RootName, true ) ) 
				{
					IUltraCalcReference wrappedR = ( (UCReference)inReference ).Reference;
					if ( wrappedR is RefBase )
						inBase = (RefBase)wrappedR;
				}
			}
			else
			{
				inBase = (RefBase)inReference;
			}


			// JM 09-10-04
			if ( inBase == null )
				return false;


			return this.Contains( inBase.ParsedReference );
		}

		/// <summary>
		/// Returns true. A range is enumerable.
		/// </summary>
		public override bool IsEnumerable
		{
			get
			{
				return true;
			}
		}

		#endregion

		#region ToString
		/// <summary>
		/// Returns a string representation of the range including the class type as well as the to and from reference.
		/// </summary>
		public override string ToString()
		{
			return string.Format( "{0}-\n  from = {1}\n  to ={2}", this.GetType(), this.from, this.to );
		}
		#endregion //ToString

		#region IEnumerable
		/// <summary>
		/// Returns an Enumerator over the Range.
		/// </summary>
		/// <returns>this IEnumerator</returns>
		public IEnumerator GetEnumerator()
		{
			return new RefRangeEnumerator( this );
		}
		#endregion // IEnumerable

		#region RefRangeEnumerator
		/// <summary>
		/// Enumerator for a <see cref="RefRange"/>
		/// </summary>
		protected class RefRangeEnumerator : IEnumerator
		{
			#region Member Variables

			private RefBase current;	// The current reference in the Range.
			private RefRange range;		// the collection being enumerated

			#endregion //Member Variables

			#region Constructor
			/// <summary>
			/// Initializes a new <see cref="RefRangeEnumerator"/>
			/// </summary>
			/// <param name="range">Range to enumerate</param>
			public RefRangeEnumerator( RefRange range )
			{
				this.range = range;
			}
			#endregion //Constructor

			#region Current
			/// <summary>
			/// Returns the current reference in the Range.
			/// </summary>
			public object Current
			{
				get
				{
					if ( this.current == null )
						throw new UltraCalcException( SR.GetString( "Error_NoCurrentRangeElement" ) );
					return (IUltraCalcReference)this.current;
				}
			}
			#endregion //Current

			#region MoveNext
			/// <summary>
			/// Sets the current reference to the next reference.  Returns false if there is no
			/// next reference.
			/// </summary>
			/// <returns>true if a next reference is available.</returns>
			public bool MoveNext()
			{
				if ( this.current == null )
				{
					this.current = this.range.NormalizedFromBase;
					return this.current != null;
				}

				RefParser nextRP = new RefParser( this.current.ParsedReference );

				// generate index of tuple to increment.  In multi-tuple RPs that don't end in a relative index, 
				// it's the next-to-last tuple (row in row/column).  In in multi-tuple RPs that do end in a 
				// relative index or in single-tuple RPs, it's the last tuple.
				int incIndex = nextRP.TupleCount - 1;
				if ( nextRP.LastTuple.Scope != RefTuple.RefScope.RelativeIndex && incIndex > 0 )
				{
					--incIndex;
				}

				if ( nextRP[ incIndex ].IncrementScope() )
				{
					if ( this.range.Contains( nextRP ) )
					{
						IUltraCalcReference currentRef = this.current.CreateReference( nextRP.ToString() );
						if ( currentRef is RefBase )
						{
							this.current = (RefBase)currentRef;
							return true;
						}
					}
				}

				this.current = null;
				return false;
			}
			#endregion //MoveNext

			#region Reset
			/// <summary>
			/// Resets the enumeration to the begining.
			/// </summary>
			public void Reset()
			{
				this.current = null;
			}
			#endregion //Reset
		}
		#endregion //RefRangeEnumerator
	}

	#endregion 

	#region RefTuple

	/// <summary>
	/// This class represents a element identifier and a scope.
	/// </summary>
    public class RefTuple
	{
		#region Constants

		private const string ParentTypeString = "..";		// String used to indicate Parent Element.
		private const string ExplicitThisTypeString = ".";	// String used to indicate "this" element.
		private const string AllScopeString = "*";			// String used to indicate "all" scope.

		#endregion //Constants

		#region Enumerations

		#region RefType
		/// <summary>
		/// The different types of elements.
		/// </summary>
		public enum RefType
		{
			/// <summary>
			/// A parent reference (e.g. "..")
			/// </summary>
			Parent,

			/// <summary>
			/// A self reference (e.g. ".")
			/// </summary>
			ExplicitThis, 

			/// <summary>
			/// A string identifying the name of the reference.
			/// </summary>
			Identifier,

			/// <summary>
			/// An unknown reference type
			/// </summary>
			Unknown
		}
		#endregion //RefType

		#region RefScope
		/// <summary>
		/// The different scopes.
		/// </summary>
		public enum RefScope
		{
			/// <summary>
			/// No scope was specified
			/// </summary>
			Any,

			/// <summary>
			/// A numeric value indicating the index of the reference
			/// </summary>
			Index,

			/// <summary>
			/// A numeric value indicating a relative offset
			/// </summary>
			RelativeIndex, 

			/// <summary>
			/// An identifier
			/// </summary>
			Identifier,

			/// <summary>
			/// An empty parentheses
			/// </summary>
			SummaryValue,

			/// <summary>
			/// An identifier indicating all (e.g. "(*)")
			/// </summary>
			All, 

			/// <summary>
			/// An unknown scope
			/// </summary>
			Unknown
		}
		#endregion //RefScope

		#region RefTupleFlags
		// SSP 9/21/04
		// Changed the flags from BitArray to flagged enum and also added ScopeIDRequiresQuotes member
		// which indicates whether the scope identifier is a standalone string (as in the reference
		// //ultraGrid1/Customers/State(NY) ) instead of a named value pair (as in the reference
		// //ultraGrid1/Customers(State=NY) ).
		// Commented out the original code and added the new code.
		//
		// ------------------------------------------------------------------------------------------
		[Flags()]
		private enum RefTupleFlags
		{
			Marked = 1, 
			Last = 2,
			NextToLast = 4,



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			ScopeIDRequiresQuotes = 8
		}
		#endregion //RefTupleFlags

		#region State
		[Flags()]
		private enum State
		{
			TupleNamePortion = 1,
			Escape = 2,
			TupleScopePortion = 4,
			QuotedString = 8,
			TupleScopeEnded = 16
		}
		#endregion //State

		#endregion //Enumerations

		#region Member Variables
		// ////////////////////////////////////////////////////////////////////////////////////////
		// if you add a member, you must add it to the copy constructor (RefTuple( RefTuple clone))
		// ////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// The tuple name for reference type Identifier.
		/// </summary>
		private string refName = null;

		/// <summary>
		/// The tuple reference type.
		/// </summary>
		private RefType refType = RefType.Unknown;

		/// <summary>
		/// The tuple scope Identifier for scope type Identifier.
		/// </summary>
		private string scopeIdentifier = null;

		/// <summary>
		/// The tuple scope type.
		/// </summary>
		private RefScope scopeType = RefScope.Unknown;

		/// <summary>
		/// The tuple scope index for scope type Index or RelativeIndex.
		/// </summary>
		private int scopeIndex = 0;

		// SSP 9/21/04
		// Use flags enum instead of BitArray.
		//
		private RefTupleFlags flags = 0;
		
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Cloning constructor.
		/// </summary>
		/// <param name="clone">The RefTuple to clone.</param>
		public RefTuple( RefTuple clone )
		{
			this.refType = clone.refType;
			this.refName = clone.refName;
			this.scopeIdentifier = clone.scopeIdentifier;
			this.scopeType = clone.scopeType;
			this.scopeIndex = clone.scopeIndex;

			// SSP 9/21/04
			// Use flags enum instead of BitArray.
			//
			this.flags = clone.flags;
			




		}

		// SSP 9/18/04
		// Implemented a way to escape special characters in reference names.
		// Commented out the old RefTuple constructor.
		//
		/// <summary>
		/// Parsing contructor.
		/// </summary>
		/// <param name="refName">The tuple in string form.</param>
		public RefTuple( string refName )
			// MD 6/25/12 - TFS113177
			// Moved add code to the new overload.
			: this(refName, false) { }

		// MD 6/25/12 - TFS113177
		// Added a new overload which can maintain the case of the the original refName.
		internal RefTuple(string refName, bool maintainCase)
		{
			this.refType = RefType.Unknown;
			this.scopeType = RefScope.Unknown;

			// MD 6/25/12 - TFS113177
			//string error = this.Parse( refName );
			string error = this.Parse(refName, maintainCase);

			if ( null != error )
				throw new UltraCalcException( error );
		}

		#endregion //Constructor

		#region Properties
		/// <summary>
		/// The tuple type.  Read Only.
		/// </summary>
		public RefType Type
		{
			get { return this.refType; }
		}

		/// <summary>
		/// The tuple scope.  Read Only.
		/// </summary>
		public RefScope Scope
		{
			get { return this.scopeType; }
			set { this.scopeType = value; } 
		}

		/// <summary>
		/// The tuple name for reference type Identifier. Read Only.
		/// </summary>
		public string Name
		{
			get { return this.refName; }
		}

		/// <summary>
		/// Indicator that this tuple is Marked.
		/// </summary>
		public bool Marked
		{
			get
			{
				return 0 != ( this.flags & RefTupleFlags.Marked ); 
			}
			set
			{
				if ( this.Marked != value )
					this.flags ^= RefTupleFlags.Marked;
			} 
		}

		/// <summary>
		/// Indicator that this tuple is is the last tuple in a RefParser tuple path.
		/// </summary>
		public bool Last
		{
			get
			{
				return 0 != ( this.flags & RefTupleFlags.Last );
			}
			set
			{
				if ( this.Last != value )
					this.flags ^= RefTupleFlags.Last;
			}
		}

		/// <summary>
		/// Indicator that this tuple is is the next-to-last tuple in a RefParser tuple path.
		/// </summary>
		public bool NextToLast
		{
			get
			{
				return 0 != ( this.flags & RefTupleFlags.NextToLast );
			}
			set
			{
				if ( this.NextToLast != value )
					this.flags ^= RefTupleFlags.NextToLast;
			}
		}

		// SSP 9/21/04
		//






		private bool ScopeIDRequiresQuotes
		{
			get
			{
				return 0 != ( RefTupleFlags.ScopeIDRequiresQuotes & this.flags );
			}
			set
			{
				if ( this.ScopeIDRequiresQuotes != value )
					this.flags ^= RefTupleFlags.ScopeIDRequiresQuotes;
			}
		}

		/// <summary>
		/// The tuple scope Identifier for scope type Identifier. Read Only.
		/// </summary>
		public string ScopeID 
		{
			get { return this.scopeIdentifier;	}
		}

		/// <summary>
		/// The tuple scope index for scope type Index or RelativeIndex. Read Only.
		/// </summary>
		public int ScopeIndex 
		{
			get { return this.scopeIndex; }
			set { this.scopeIndex = value; }
		}

		#region IsAbsolute
		/// <summary>
		/// A tuple is Absolute if it is of type Identifier with a scope type other than RelativeIndex.
		/// </summary>
		public bool IsAbsolute
		{
			get
			{
				if ( this.refType == RefType.Identifier )
				{
					switch ( this.scopeType )
					{
						case RefScope.Identifier:
						case RefScope.Index:
						case RefScope.SummaryValue:
						case RefScope.All:
							return true;

						default:
							return false;
					}

				}
				return false;
			}
		}
		#endregion //IsAbsolute

		#endregion //Properties

		#region Methods

		#region Dump


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		#endregion //Dump

		#region ParseAndNormalizeScope


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private string ParseAndNormalizeScope( string scopePortion, out string normalizedScopePortion )
		{
			normalizedScopePortion = null;
			RefParser.NameValuePair[] pairs;
			string nonNameValuePairScope;
			string error = RefParser.ParseScopeHelper( scopePortion, out pairs, out nonNameValuePairScope );

			if ( null != error )
				return error;

			if ( null != pairs && pairs.Length > 0 )
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder( scopePortion.Length );

				for ( int i = 0; i < pairs.Length; i++ )
				{
					// Put commas between multiple name-value pairs.
					//
					if ( i > 0 )
						sb.Append( RefParser.NameValuePairSeparatorCharacter );

					RefParser.NameValuePair pair = pairs[ i ];

					// Append Name="Value".
					//
					sb.Append( RefParser.EscapeString( pair.Name.ToLower( CultureInfo.InvariantCulture ), false ) );
					sb.Append( RefParser.NameValueSeparatorCharacter );
					sb.Append( RefParser.QuotesCharacter );
					sb.Append( RefParser.EscapeString( pair.Value, true ) );
					sb.Append( RefParser.QuotesCharacter );
				}

				this.ScopeIDRequiresQuotes = false;
				normalizedScopePortion = sb.ToString();
			}
			else if ( null != nonNameValuePairScope )
			{
				this.ScopeIDRequiresQuotes = true;
				normalizedScopePortion = RefParser.EscapeString( nonNameValuePairScope, true );
			}
			else
			{
				Debug.Assert( false );
				error = SR.GetString( "Error_RT_InvalidScope", scopePortion );
			}

			return error;
		}
		#endregion //ParseAndNormalizeScope

		#region Parse


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// MD 6/25/12 - TFS113177
		//private string Parse( string refName )
		private string Parse(string refName, bool maintainCase)
		{
			if ( null == refName || 0 == refName.Length )
				return SR.GetString( "Error_RT_NullRefName" );

			string tuplePortion = null;
			string scopePortion = null;
			int lastEscapedCharIndex = -1;
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder( refName.Length );
			State state = State.TupleNamePortion;

			for ( int index = 0; index < refName.Length; index++ )
			{
				char c = refName[ index ];

				if ( 0 != ( State.Escape & state ) )
				{
					state ^= State.Escape;

					// Don't unescape the scope portion because we will be doing that in 
					// ParseAndNormalizeScope method. Also escape characters that need to be escaped.
					//
					if ( State.TupleScopePortion == state || RefParser.DoesCharNeedEscaping( c ) )
						stringBuilder.Append( RefParser.EscapeCharacter );

					lastEscapedCharIndex = stringBuilder.Length;
					stringBuilder.Append( c );
				}
				else if ( RefParser.EscapeCharacter == c )
				{
					state |= State.Escape;
				}
				else if ( RefParser.RefBeginScope == c && State.TupleNamePortion == state )
				{
					RefParser.TrimTrailingSpaces( stringBuilder, lastEscapedCharIndex );
					tuplePortion = stringBuilder.ToString();
					stringBuilder.Length = 0;
					if ( 0 == tuplePortion.Length )
						return SR.GetString( "Error_RT_InvalidTuple_NamePortionEmpty", refName );

					state = State.TupleScopePortion;
				}
				else if ( RefParser.RefEndScope == c && State.TupleScopePortion == state )
				{
					// The tuple should have no more characters left after the close parenthesis. If
					// it does, then return an error string indicating an error has occured.
					//
					if ( 1 + index != refName.Length )
						return SR.GetString( "Error_RT_InvalidTuple_CharactersAfterScopeEnd", refName );

					RefParser.TrimTrailingSpaces( stringBuilder, lastEscapedCharIndex );
					scopePortion = stringBuilder.ToString();
					state = State.TupleScopeEnded;
				}
				else if ( State.TupleScopeEnded == state )
				{
					// The tuple should have no more characters left after the close parenthesis. If
					// it does, then return an error string indicating an error has occured.
					//
					if ( !char.IsWhiteSpace( c ) )
						return SR.GetString( "Error_RT_InvalidTuple_CharactersAfterScopeEnd", refName );
				}
				else if ( 0 == stringBuilder.Length && 0 == ( State.QuotedString & state ) && char.IsWhiteSpace( c ) )
				{
					// Skip spaces in the beginning of the scope text or a tuple name.
					//
				}
				else if ( RefParser.QuotesCharacter == c && State.TupleScopePortion == state )
				{
					state |= State.QuotedString;
					stringBuilder.Append( c );
				}
				else if ( RefParser.QuotesCharacter == c && 0 != ( State.QuotedString & state ) )
				{
					state ^= State.QuotedString;
					stringBuilder.Append( c );
				}
				else if ( 0 != ( State.QuotedString & state ) )
				{
					// Allow any character whithing double quotes. Only characters that have
					// special meaning in quoted string is the quotes and the escape character
					// which are taken care of above.
					//
					stringBuilder.Append( c );
				}
				// Don't check for unescaped special characters in the scope portion because the
				// parsing logic for the RefTuple (RefTuple.Parse method) will take care of that.
				//
				else if ( 0 == ( State.TupleScopePortion & state ) && RefParser.DoesCharNeedEscaping( c ) )
				{
					return SR.GetString( "Error_RT_UnescapedCharacterInTuple", c, refName, index );
				}
				else
				{
					stringBuilder.Append( c );
				}
			}

			// If the last character was an escape character, then return an error.
			//
			if ( 0 != ( State.Escape & state ) )
				return SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", RefParser.EscapeCharacter, refName, refName.Length - 1 );

			if ( 0 != ( State.QuotedString & state ) )
				return SR.GetString( "Error_RT_InvalidTuple_MissingEndQuotes", refName );

			if ( 0 != ( State.TupleScopePortion & state ) )
				return SR.GetString( "Error_RT_InvalidTuple_MissingCloseParenthesis", refName );

			// Since the tuple name doesn't have any ending marker the state can be TupleNamePortion
			// in which case get the tuple portion from the string builder.
			//
			if ( state == State.TupleNamePortion )
			{
				RefParser.TrimTrailingSpaces( stringBuilder, lastEscapedCharIndex );
				if ( 0 == stringBuilder.Length )
					return SR.GetString( "Error_RT_NullRefName" );

				tuplePortion = stringBuilder.ToString();
			}

			Debug.Assert( tuplePortion.Length > 0 );

			if ( string.Equals( tuplePortion, RefTuple.ParentTypeString ) )
			{
				this.refType = RefType.Parent;
			}
			else if ( string.Equals( tuplePortion, RefTuple.ExplicitThisTypeString ) )
			{
				this.refType = RefType.ExplicitThis;
			}
			else 
			{
				// Convert the tuple portion to lower case since reference names are case insensitive.
				//
				// MD 6/25/12 - TFS113177
				// If maintainCase is True, don't call ToLower.
				//this.refName = tuplePortion.ToLower( CultureInfo.InvariantCulture );
				if (maintainCase)
					this.refName = tuplePortion;
				else
					this.refName = tuplePortion.ToLower(CultureInfo.InvariantCulture);

				this.refType = RefType.Identifier;
			}

			if ( null == scopePortion )
			{
				// If no scope was specified then scopePortion would be null.
				//
				this.scopeType = RefScope.Any;
			}
			else if ( 0 == scopePortion.Length )
			{
				// () means the scope of summary value.
				//
				this.scopeType = RefScope.SummaryValue;
			}
			else if ( string.Equals( RefTuple.AllScopeString, scopePortion ) )
			{
				// (*)
				//
				this.scopeType = RefScope.All;
			} 
			else 
			{
				bool isRelative = '+' == scopePortion[0] || '-' == scopePortion[0];
				int i = isRelative ? 1 : 0;
				while ( i < scopePortion.Length && char.IsDigit( scopePortion, i ) )
					i++;						

				if ( i == scopePortion.Length )
				{
					try
					{
						this.scopeIndex = Convert.ToInt32( scopePortion, CultureInfo.InvariantCulture );
					}
					catch ( Exception e )
					{
						return SR.GetString( "Error_RT_InvalidTuple_ScopeIndexLarge", refName, scopePortion, e.Message );
					}

					this.scopeType = isRelative ? RefScope.RelativeIndex : RefScope.Index;
				}
				else
				{
					this.scopeType = RefScope.Identifier;
					string error = this.ParseAndNormalizeScope( scopePortion, out this.scopeIdentifier );
					if ( null != error )
						return error;
				}
			}

			return null;
		}
		#endregion //Parse

		
#region Infragistics Source Cleanup (Region)



















































































































































































#endregion // Infragistics Source Cleanup (Region)


		#region IsSameTuple
		/// <summary>
		/// Tuple Equivalence comparison.  True if same Type and Scope.
		/// </summary>
		/// <param name="tuple">The tuple to compare against.</param>
		/// <returns>true iff tuple is same type and scope as this.</returns>
		public bool IsSameTuple( RefTuple tuple )
		{
			if ( this.refType == tuple.Type )
			{
				if ( this.refType == RefType.Identifier )
				{
					// SSP 6/20/11
					// Use the new helper method in the utils.
					// 
					//if ( String.Compare( this.refName, tuple.Name, true ) == 0 )
					if ( RefParser.AreStringsEqual( this.refName, tuple.Name, true ) )
					{
						return IsSameScope( tuple );
					}
				}
				else
				{
					return IsSameScope( tuple );
				}
			}

			return false;
		}
		#endregion //IsSameTuple

		#region IsSameIdentifier
		/// <summary>
		/// Compares this tuple identifiers with the input tuple identifier.
		/// </summary>
		/// <param name="tuple">The input tuple.</param>
		/// <returns>true if the identifiers are equal.</returns>
		public bool IsSameIdentifier( RefTuple tuple )
		{
			if ( this.refType == tuple.Type )
			{
				if ( this.refType == RefType.Identifier )
				{
					// SSP 6/20/11
					// Use the new helper method in the utils.
					// 
					//if ( String.Compare( this.refName, tuple.Name, true ) == 0 )
					if ( RefParser.AreStringsEqual( this.refName, tuple.Name, true ) )
					{
						if ( this.scopeType == RefScope.SummaryValue || tuple.Scope == RefScope.SummaryValue )
						{
							return this.scopeType == tuple.Scope;
						} 

						return true;
					}
				}
			}

			return false;
		}
		#endregion //IsSameIdentifier

		#region Contains
		/// <summary>
		/// True if this tuple contains the input tuple.  The input tuple is considered to be 
		/// contained if both tuples have the same identifier and if this scope contains the 
		/// input scope.  The input scope is contained if neither scope types are type Unknown and
		/// they meet one of the following conditions:
		/// <list>
		/// <item>Both tuples have SummaryValue scope.</item>
		/// <item>Either tuple has scope type Any.</item>
		/// <item>Both are type Identifier and have lexographicly equal scope identifiers.</item>
		/// <item>Both are the same type of index (Index or RelativeIndex) and have the same scope index.</item>
		/// </list>
		/// </summary>
		/// <param name="tuple">The <see cref="RefTuple"/> to check.</param>
		/// <returns>True if this tuple contains the input tuple.</returns>
		public bool Contains( RefTuple tuple )
		{
			if ( this.refType == tuple.Type )
			{
				if ( this.refType == RefType.Identifier )
				{
					// SSP 6/20/11
					// Use the new helper method in the utils.
					// 
					//if ( String.Compare( this.refName, tuple.Name, true ) == 0 )
					if ( RefParser.AreStringsEqual( this.refName, tuple.Name, true ) )
					{
						return ContainsScope( tuple );
					}
				}
				else
				{
					return ContainsScope( tuple );
				}
			}

			return false;
		}
		#endregion //Contains

		#region IsSubset
		/// <summary>
		/// True if the input tuple is a subset of this tuble.  The input tuple is considered to be 
		/// a subset if both tuples have the same identifier and if this scope entirely contains the 
		/// input scope.  The input scope is contained if neither scope types are type Unknown and
		/// they meet one of the following conditions:
		/// <list>
		/// <item>Both tuples have SummaryValue scope.</item>
		/// <item>This tuple has scope type Any.</item>
		/// <item>Both scopes are type Identifier and have lexographicly equal scope identifiers.</item>
		/// <item>Both scopes are the same type of index (Index or RelativeIndex) and have the same scope index.</item>
		/// </list>
		/// </summary>
		/// <param name="tuple">The <see cref="RefTuple"/> to check.</param>
		/// <returns>True if the input tuple is a subset of this tuble.</returns>
		public bool IsSubset( RefTuple tuple )
		{
			if ( this.refType == tuple.Type )
			{
				if ( this.refType == RefType.Identifier )
				{
					// SSP 6/20/11
					// Use the new helper method in the utils.
					// 
					//if ( String.Compare( this.refName, tuple.Name, true ) == 0 )
					if ( RefParser.AreStringsEqual( this.refName, tuple.Name, true ) )
					{
						return IsScopeSubset( tuple );
					}
				}
				else
				{
					return IsScopeSubset( tuple );
				}
			}

			return false;
		}
		#endregion //IsSubset

		#region IsSameScope
		/// <summary>
		/// True if if the scope of the input tuple is identical to this scope.
		/// </summary>
		/// <param name="tuple">The input tuple.</param>
		/// <returns>true if the scope of the input tuple is identical to this scope.</returns>
		private bool IsSameScope( RefTuple tuple )
		{
			if( this.scopeType == RefScope.SummaryValue || tuple.Scope == RefScope.SummaryValue ) 
			{
				return this.scopeType == tuple.Scope;
			}

			if ( this.scopeType == RefScope.All || tuple.Scope == RefScope.All )
			{
				return this.scopeType == tuple.Scope;
			} 

			if ( this.scopeType == tuple.Scope )
			{
				switch ( this.scopeType )
				{
					case RefScope.Identifier:
						return String.Compare(this.scopeIdentifier,tuple.ScopeID) == 0;

					case RefScope.Index:
					case RefScope.RelativeIndex:
						return this.scopeIndex == tuple.ScopeIndex; 

					default:
						return true;
				}

			}

			return false;
		}
		#endregion //IsSameScope

		#region ContainsScope
		/// <summary>
		/// True if this tuple scope contains the input tuple scope. The input scope is contained 
		/// if neither scope types are type Unknown and they meet one of the following conditions:
		/// <list>
		/// <item>Both tuples have SummaryValue scope.</item>
		/// <item>Either tuple has scope type Any.</item>
		/// <item>Both are type Identifier and have lexographicly equal scope identifiers.</item>
		/// <item>Both are the same type of index (Index or RelativeIndex) and have the same scope index.</item>
		/// </list>
		/// </summary>
		/// <param name="tuple"></param>
		/// <returns></returns>
		private bool ContainsScope( RefTuple tuple )
		{

			if ( this.scopeType == RefScope.Unknown || tuple.Scope == RefScope.Unknown )
			{
				return false;
			}

			if ( this.scopeType == RefScope.SummaryValue || tuple.Scope == RefScope.SummaryValue )
			{
				return this.scopeType == tuple.Scope;
			} 

			if ( this.scopeType == RefScope.Any || tuple.Scope == RefScope.Any )
			{
				return true;
			}

			if ( this.scopeType == RefScope.All || tuple.Scope == RefScope.All )
			{
				return true;
			} 

			if( this.scopeType == RefScope.Identifier && tuple.Scope == RefScope.Identifier )
			{
				return String.Compare(this.scopeIdentifier,tuple.ScopeID) == 0;
			}

			if ( this.scopeType == RefScope.Index && tuple.Scope == RefScope.Index )
			{
				return this.scopeIndex == tuple.ScopeIndex;
			}

			if ( this.scopeType == RefScope.RelativeIndex && tuple.Scope == RefScope.RelativeIndex )
			{
				return this.scopeIndex == tuple.ScopeIndex;
			} 
			return false;
		}
		#endregion //ContainsScope

		#region IsScopeSubset
		/// <summary>
		/// True if input tuple scope is entirely contained by this tuple scope. The input scope is enitrely contained 
		/// if neither scope types are type Unknown and they meet one of the following conditions:
		/// <list>
		/// <item>Both tuples have SummaryValue scope.</item>
		/// <item>This tuple has scope type Any.</item>
		/// <item>Both are type Identifier and have lexographicly equal scope identifiers.</item>
		/// <item>Both are the same type of index (Index or RelativeIndex) and have the same scope index.</item>
		/// </list>
		/// </summary>
		/// <param name="tuple"></param>
		/// <returns></returns>
		private bool IsScopeSubset( RefTuple tuple )
		{

			if ( this.scopeType == RefScope.Unknown || tuple.Scope == RefScope.Unknown )
			{
				return false;
			}

			if ( this.scopeType == RefScope.SummaryValue || tuple.Scope == RefScope.SummaryValue )
			{
				return this.scopeType == tuple.Scope;
			} 

			if ( this.scopeType == RefScope.Any )
			{
				return true;
			}

			if ( this.scopeType == RefScope.All )
			{
				return true;
			} 

			if( this.scopeType == RefScope.Identifier && tuple.Scope == RefScope.Identifier )
			{
				return String.Compare(this.scopeIdentifier,tuple.ScopeID) == 0;
			}

			if ( this.scopeType == RefScope.Index && tuple.Scope == RefScope.Index )
			{
				return this.scopeIndex == tuple.ScopeIndex;
			}

			if ( this.scopeType == RefScope.RelativeIndex && tuple.Scope == RefScope.RelativeIndex )
			{
				return this.scopeIndex == tuple.ScopeIndex;
			} 
			return false;
		}
		#endregion //IsScopeSubset

		#region IsNumericScopeLTE
		/// <summary>
		/// True if both tuples have the same type and numeric scope type and
		/// this numeric index &lt;= the input tuple numeric index
		/// </summary>
		/// <param name="tuple">The tuple to test.</param>
		/// <returns>true iff this is &lt;= input.</returns>
		/// <seealso cref="IsNumericScopeGTE"/>
		public bool IsNumericScopeLTE( RefTuple tuple )
		{

			if ( this.scopeType == RefScope.SummaryValue || tuple.Scope == RefScope.SummaryValue )
			{
				return this.scopeType == tuple.Scope;
			} 

			if ( this.scopeType == RefScope.Any || tuple.Scope == RefScope.Any )
			{
				return true;
			}

			if ( this.scopeType == RefScope.All || tuple.Scope == RefScope.All )
			{
				return true;
			} 

			if ( this.refType != tuple.Type )
				return false;

			if ( this.refType == RefType.Identifier )
			{
				// SSP 6/20/11
				// Use the new helper method in the utils.
				// 
				//if ( String.Compare( this.refName, tuple.Name, true ) != 0 )
				if ( ! RefParser.AreStringsEqual( this.refName, tuple.Name, true ) )
					return false;
			}

			if ( this.scopeType == RefScope.Index )
			{
				if ( tuple.Scope == RefScope.Index )
					return this.scopeIndex <= tuple.ScopeIndex;

				if ( tuple.Scope == RefScope.RelativeIndex )
				{
					return this.scopeIndex <= ( this.scopeIndex + tuple.ScopeIndex );
				}
			}
			else if ( this.scopeType == RefScope.RelativeIndex )
			{
				if ( tuple.Scope == RefScope.Index )
					return ( this.scopeIndex + tuple.ScopeIndex ) <= tuple.ScopeIndex;

				if ( tuple.Scope == RefScope.RelativeIndex )
				{
					return this.scopeIndex <= tuple.ScopeIndex;
				}
			} 

			return false;
		}
		#endregion //IsNumericScopeLTE 

		#region IsNumericScopeGTE
		/// <summary>
		/// True if both tuples have the same type and numeric scope type and
		/// this numeric index >= the input tuple numeric index
		/// </summary>
		/// <param name="tuple">The tuple to test.</param>
		/// <returns>true iff this is >= input.</returns>
		/// <seealso cref="IsNumericScopeLTE"/>
		public bool IsNumericScopeGTE( RefTuple tuple )
		{

			if ( this.scopeType == RefScope.SummaryValue || tuple.Scope == RefScope.SummaryValue )
			{
				return this.scopeType == tuple.Scope;
			} 

			if ( this.scopeType == RefScope.Any || tuple.Scope == RefScope.Any )
			{
				return true;
			}

			if ( this.scopeType == RefScope.All || tuple.Scope == RefScope.All )
			{
				return true;
			} 

			if ( this.refType != tuple.Type )
				return false;

			if ( this.refType == RefType.Identifier )
			{
				// SSP 6/20/11
				// Use the new helper method in the utils.
				// 
				//if ( String.Compare( this.refName, tuple.Name, true ) != 0 )
				if ( !RefParser.AreStringsEqual( this.refName, tuple.Name, true ) )
					return false;
			}

			if ( this.scopeType == RefScope.Index )
			{
				if ( tuple.Scope == RefScope.Index )
					return this.scopeIndex >= tuple.ScopeIndex;

				if ( tuple.Scope == RefScope.RelativeIndex )
				{
					return this.scopeIndex >= ( this.scopeIndex + tuple.ScopeIndex );
				}
			}
			else if ( this.scopeType == RefScope.RelativeIndex )
			{
				if ( tuple.Scope == RefScope.Index )
					return ( this.scopeIndex + tuple.ScopeIndex ) >= tuple.ScopeIndex;

				if ( tuple.Scope == RefScope.RelativeIndex )
				{
					return this.scopeIndex >= tuple.ScopeIndex;
				}
			} 

			return false;
		}
		#endregion //IsNumericScopeGTE 

		#region IncrementScope
		/// <summary>
		/// Increments the scope if the scope if type Index or RelativeIndex.
		/// </summary>
		/// <returns>true if scope was incremented.</returns>
		public bool IncrementScope()
		{
			switch ( this.scopeType )
			{
				case RefScope.Index:
				case RefScope.RelativeIndex:
					++this.scopeIndex;
					return true;
			}

			return false;
		}
		#endregion //IncrementScope

		#region Not Used

		// MD 7/6/07 - 7.3 Performance
		// FxCop - Avoid uncalled private code
		//#region CompareScopeID
		///// <summary>
		///// Compares scope identifiers by first attempting a numeric compare.  If that fails then the
		///// scope identifiers are lexographicly compared.
		///// 
		///// </summary>
		///// <param name="one">first scope identifier</param>
		///// <param name="two">second scope identifier</param>
		///// <returns>0 if one == two, &lt;0 if one &lt; two, &gt;0 if one &gt; two.</returns>
		//// MD 6/28/07 - 7.3 Performance
		//// FxCop - Mark members as static
		////private int CompareScopeID( string one,string two )
		//private static int CompareScopeID( string one, string two )
		//{
		//    try 
		//    {
		//        double d1 = Double.Parse(one);
		//        double d2 = Double.Parse(two);
		//        return (int)(d1 - d2);
		//    } 
		//    catch
		//    {
		//    }
		//    return String.Compare(one,two);
		//}
		//#endregion //CompareScopeID

		#endregion Not Used

		#region ToString
		// SSP 9/21/04
		// Added ToString overload that takes in a string builder.
		//
		/// <summary>
		/// Appends the tuple to the specified string builder.
		/// </summary>		
		/// <param name="sb">The <see cref="System.Text.StringBuilder"/> to append to.</param>
		public void ToString( System.Text.StringBuilder sb )
		{
			switch ( this.refType )
			{
				case RefType.ExplicitThis:
					sb.Append( RefTuple.ExplicitThisTypeString );
					break;
				case RefType.Parent:
					sb.Append( RefTuple.ParentTypeString );
					break; 
				case RefType.Identifier:
					sb.Append( this.refName );
					break;
				case RefType.Unknown:
					Debug.Assert( false );
					sb.Append( "?unknown?" );
					break;
				default:
					Debug.Assert( false );
					sb.Append( "?default?" );
					break;
			}

			if ( RefScope.Any != this.scopeType )
			{
				sb.Append( RefParser.RefBeginScope );

				switch ( this.scopeType )
				{
					case RefScope.Identifier:
						if ( this.ScopeIDRequiresQuotes )
							sb.Append( RefParser.QuotesCharacter ).Append( this.scopeIdentifier ).Append( RefParser.QuotesCharacter );
						else 
							sb.Append( this.scopeIdentifier );
						break;
					case RefScope.SummaryValue:
						break;
					case RefScope.All:
						sb.Append( RefTuple.AllScopeString );
						break; 
					case RefScope.Index:
						sb.Append( this.scopeIndex );
						break;
					case RefScope.RelativeIndex:
						if ( this.scopeIndex > 0 )
							sb.Append( '+' );

						sb.Append( this.scopeIndex );
						break;
					case RefScope.Unknown:
						Debug.Assert( false );
						sb.Append( "?unknown?" );
						break;
					default:
						Debug.Assert( false );
						sb.Append( "?default?" );
						break;
				}

				sb.Append( RefParser.RefEndScope );
			}
		}
		#endregion //ToString

		#region ToString
		/// <summary>
		/// Returns the string form of the tuple.
		/// </summary>
		/// <returns>The tuple in string form.</returns>
		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			this.ToString( sb );
			return sb.ToString();
		}
		#endregion //ToString

		#endregion //Methods
	}

	#endregion 

	#region RefParser

	/// <summary>
	/// A RefParser is a parsed representation of a Reference Path.  It contains a RefTuple array 
	/// representing each element in the Reference Path.
	/// </summary>
    public class RefParser : IEnumerable 
	{
		#region Constants

		/// <summary>
		/// String used to denote the start of a fully qualified reference.
		/// </summary>
		public const string				RefFullyQualifiedString = "//";

		/// <summary>
		/// Character used as a Path element seperator.
		/// </summary>
		public const char				RefSeperator = '/';

		/// <summary>
		/// String used as a Path element seperator.
		/// </summary>
		public const string				RefSeperatorString = "/";

		/// <summary>
		/// Character used to denote the begining of a scope.
		/// </summary>
		public const char				RefBeginScope = '(';

		/// <summary>
		/// Character used to denote the end of a scope.
		/// </summary>
		public const char				RefEndScope = ')';

		// SSP 9/18/04
		// Changed the range separator from ".." to ":". Added the RangeSeparator and
		// RangeSeparatorWithSpaces constants. Before the range separator was hard-coded in
		// multiple places.
		//
		/// <summary>
		/// Character used to denote a range of references.
		/// </summary>
		public const string				RangeSeparator = ":";

		/// <summary>
		/// A space padded string representing a range.
		/// </summary>
		public const string				RangeSeparatorWithSpaces = " : ";

		// SSP 9/20/04
		//
		/// <summary>
		/// Character used for escaping special characters. It's the \ (backward slash) character.
		/// </summary>
		public const char				EscapeCharacter = '\\';

		/// <summary>
		/// Character used for specifying a quoted string. It's the " (double quotes) character.
		/// </summary>
		public const char				QuotesCharacter = '"';

		/// <summary>
		/// Character used for separating multiple name-value pairs. It's the , (comma) character.
		/// </summary>
		public const char				NameValuePairSeparatorCharacter = ',';

		/// <summary>
		/// Character used for separating name portion and the value portion in a name-value pair. It's the = (equal) character.
		/// </summary>
		public const char				NameValueSeparatorCharacter = '=';

		// SSP 10/12/04
		// Got rid of the BitArray and instead added code to make use of flags enum.
		//
		
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


		[Flags()]
		private enum RefParserFlags
		{
			FullyQualified = 0x1,
			Root = 0x2,
			HasRelativeIndex = 0x4,
			HasSummaryScope = 0x8,
			HasAbsoluteIndex = 0x10,
			HasScopeAll = 0x20
		} 

		#endregion //Constants

		#region Member Variables

		private ArrayList tuples = new ArrayList( );		// Array of RefTuples in the parsed reference.
		private RefParserFlags flags = 0;		// Flag storage.

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public RefParser()
		{
		}

		// SSP 9/7/04
		// Added a constructor that takes in a reference name for convenient.
		//
		/// <summary>
		/// Constructor. Parses the passed in string.
		/// </summary>
        /// <param name="refName">The string representation of a reference.</param>
		public RefParser( string refName )
			// MD 6/25/12 - TFS113177
			// Moved add code to the new overload.
			: this(refName, false) { }

		// MD 6/25/12 - TFS113177
		// Added a new overload which can maintain the case of the the original refName.
		internal RefParser(string refName, bool maintainCase)
		{
			// MD 6/25/12 - TFS113177
			//string error = this.ParseReference( refName );
			string error = this.ParseReference(refName, maintainCase);

			if ( null != error )
				throw new UltraCalcException( error );
		}


		/// <summary>
		/// Clone Constructor.
		/// </summary>
		/// <param name="rp">A RefParser instance from which a clone should be created.</param>
		public RefParser( RefParser rp )
		{
			this.flags = rp.flags;
			foreach ( RefTuple tuple in rp )
			{
				this.tuples.Add( new RefTuple( tuple ) );
			}
		} 

		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Return the tuple count. Read Only.
		/// </summary>
		public int TupleCount
		{
			get
			{
				return this.tuples.Count;
			}
		}

		/// <summary>
		/// Tuple indexer.  Read Only.
		/// </summary>
		public RefTuple this[ int index ]
		{
			get
			{
				return (RefTuple)( this.tuples[ index ] );
			}
		} 

		/// <summary>
		/// Return the Last tuple or null if none available.  Read Only.
		/// </summary>
		public RefTuple LastTuple
		{
			get
			{
				int count = this.tuples.Count;
				return count > 0 ? (RefTuple)this.tuples[ count - 1 ] : null;
			}
		}

		/// <summary>
		/// Return the NextToLast tuple or null if none available.  Read Only.
		/// </summary>
		public RefTuple NextToLastTuple
		{
			get
			{
				if ( this.tuples.Count > 1 )
					return (RefTuple)this.tuples[ this.tuples.Count - 2 ];
				else
					return null;
			}
		}

		// SSP 7/20/11 - XamCalculationManager
		// 
		/// <summary>
		/// Returns the root name. If there are no tuples, returns null.
		/// </summary>
		internal string RootName
		{
			get
			{
				return this.TupleCount > 0 ? this[0].Name : null;
			}
		}

		/// <summary>
		/// Returns true if the Reference is fully qualified.  Read Only.
		/// ex: //MyGrid/Something.
		/// </summary>
		public bool IsFullyQualified
		{
			get { return 0 != ( RefParserFlags.FullyQualified & this.flags );	}
		}

		/// <summary>
		/// Returns true if the Reference is a root reference.  Read Only.
		/// ex: /something
		/// </summary>
		public bool IsRoot
		{
			get	{ return 0 != ( RefParserFlags.Root & this.flags );	}
		}
        
		/// <summary>
		/// Returns true if the reference is not fully qualified or a root. Read Only.
		/// ex: total
		/// </summary>
		public bool IsRelative
		{
			get	{ return ! this.IsRoot && ! this.IsFullyQualified;	}
		}

		/// <summary>
		/// Returns true if the Reference has at least one RefTuple with a relative index.  Read Only.
		/// ex: Total(-1)
		/// </summary>
		public bool HasRelativeIndex
		{
			get { return 0 != ( RefParserFlags.HasRelativeIndex & this.flags ); }
		}

		/// <summary>
		/// Returns true if the Reference has at least one RefTuple with a scope of "All".  Read Only.
		/// ex: Total(*)
		/// </summary>
		public bool HasScopeAll
		{
			get { return 0 != ( RefParserFlags.HasScopeAll & this.flags ); }
		}

		/// <summary>
		/// Returns true if the Reference has at least one RefTuple with an absolute index.  Read Only.
		/// ex: Total(0)
		/// </summary>
		public bool HasAbsoluteIndex
		{
			get { return 0 != ( RefParserFlags.HasAbsoluteIndex & this.flags ); }
		}

		/// <summary>
		/// Returns true if the Reference has a Summary Scope.  Read Only.
		/// ex: Total()
		/// </summary>
		public bool HasSummaryScope
		{
			get { return 0 != ( RefParserFlags.HasSummaryScope & this.flags ); }
		} 

		private void SetFlag( RefParserFlags flag, bool value )
		{
			if ( value )
				this.flags |= flag;
			else
				this.flags &= ~flag;
		}

		#endregion //Properties

		#region Methods

		#region MarkIndices
		/// <summary>
		/// Determine the values of the FlagHasRelativeIndex,FlagHasAbsoluteIndex and FlagHasSummaryScope flags.
		/// Set the Last flag on the Last tuple.
		/// </summary>
		private void MarkIndices()
		{
			this.tuples.TrimToSize();

			this.flags &= ~( RefParserFlags.HasRelativeIndex | RefParserFlags.HasAbsoluteIndex
							 | RefParserFlags.HasSummaryScope | RefParserFlags.HasScopeAll );

			if ( this.tuples.Count > 0 )
			{
				foreach ( RefTuple tuple in this.tuples )
				{
					if ( tuple.Scope == RefTuple.RefScope.RelativeIndex )
					{
						this.flags |= RefParserFlags.HasRelativeIndex;
					}

					if ( tuple.Scope == RefTuple.RefScope.All )
					{
						this.flags |= RefParserFlags.HasScopeAll | RefParserFlags.HasRelativeIndex;
					}

					if ( tuple.Scope == RefTuple.RefScope.Index )
					{
						this.flags |= RefParserFlags.HasAbsoluteIndex;
					}

					if ( this.HasRelativeIndex && this.HasAbsoluteIndex && this.HasScopeAll )
					{
						break;
					}
				}

				( (RefTuple)this.tuples[ this.tuples.Count - 1 ] ).Last = true;
				if ( this.tuples.Count > 1 )
				{
					( (RefTuple)this.tuples[ this.tuples.Count - 2 ] ).NextToLast = true;
				}

				if ( ( (RefTuple)this.tuples[ this.tuples.Count - 1 ] ).Scope == RefTuple.RefScope.SummaryValue )
				{
					this.flags |= RefParserFlags.HasSummaryScope;
				}

			}

			return;
		}
		#endregion //MarkIndices 

		#region MarkRelativeIndices
		/// <summary>
		/// For each tuple in this reference with a Relative Index, mark the corresponding tuple 
		/// in targetRP
		/// </summary>
		/// <param name="targetRP">The tuples to mark.</param>
		/// <param name="forceLastTuple">Forces the last tuple to be marked.</param>
		public virtual void MarkRelativeIndices( RefParser targetRP, bool forceLastTuple )
		{
			int baseCount = this.TupleCount;
			int baseIndex = 0;

			int targetCount = targetRP.TupleCount;
			int targetIndex = 0;

			if ( this.IsFullyQualified != targetRP.IsFullyQualified )
			{
				if ( this.IsFullyQualified )
				{
					++baseIndex;
				}
				if ( targetRP.IsFullyQualified )
				{
					++targetIndex;
				}
			}


			while ( baseIndex < baseCount && targetIndex < targetCount )
			{
				RefTuple baseTuple = this[ baseIndex ];
				RefTuple targetTuple = targetRP[ targetIndex ];

				if ( baseTuple.Type == targetTuple.Type )
				{
					if ( baseTuple.Type == RefTuple.RefType.Identifier )
					{
						if ( baseTuple.IsSameIdentifier( targetTuple ) )
						{
							if ( baseTuple.NextToLast )
							{
								if ( this.LastTuple.Scope == RefTuple.RefScope.RelativeIndex
									|| this.LastTuple.Scope == RefTuple.RefScope.All
									// SSP 11/9/04 UWC113
									// Also mark the target tuple if the tuple has scope of index.
									//
									|| RefTuple.RefScope.Index == this.LastTuple.Scope )
								{
									targetTuple.Marked = true;
								}
							}
							if ( baseTuple.Scope == RefTuple.RefScope.RelativeIndex
								|| baseTuple.Scope == RefTuple.RefScope.All
								// SSP 11/9/04 UWC113
								// Also mark the target tuple if the tuple has scope of index.
								//
								|| RefTuple.RefScope.Index == baseTuple.Scope )
							{
								targetTuple.Marked = true;
							}
						}
						else if ( baseTuple.Last || targetTuple.Last )
						{
							if ( baseTuple.Scope == RefTuple.RefScope.SummaryValue || targetTuple.Scope == RefTuple.RefScope.SummaryValue )
							{
								break;
							}

							if ( forceLastTuple || baseTuple.Scope == RefTuple.RefScope.RelativeIndex
								|| baseTuple.Scope == RefTuple.RefScope.All
								// SSP 11/9/04 UWC113
								// Also mark the target tuple if the tuple has scope of index.
								//
								|| RefTuple.RefScope.Index == baseTuple.Scope )
							{
								targetTuple.Marked = true;
							}
							break;
						}
						else
						{
							break;
						}
					}
				}
				else
				{
					break;
				}
				++baseIndex;
				++targetIndex;

			}
		}
		#endregion //MarkRelativeIndices 

		#region Dump


#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

		#endregion //Dump

		#region Merge
		/// <summary>
		/// Populate this RefParser by merging the input baseRP with the input mergeRP. If 
		/// respectMarked is true, then any tuples in mergeRP that are Marked have precedence and
		/// are converted to scope Any.
		/// </summary>
		/// <param name="baseRP">The base reference.</param>
		/// <param name="mergeRP">The reference to merge into the base.</param>
		/// <param name="respectMarked">if true, mergeRP tuples that are marked have precedence.</param>
		public void Merge( RefParser baseRP,RefParser mergeRP,bool respectMarked )
		{
			this.SetFlag( RefParserFlags.FullyQualified, baseRP.IsFullyQualified );
			this.SetFlag( RefParserFlags.Root, baseRP.IsRoot );

			// can't perform the operation on a relative base
			if( baseRP.IsRelative ) 
				throw new UltraCalcException( SR.GetString( "Error_RP_MergeRelativeBase") );

			int baseCount = baseRP.TupleCount;
			int baseIndex = 0;

			int mergeCount = mergeRP.TupleCount;
			int mergeIndex = 0;

			// advance past the first element of either RP if those RPs are fully qualified
			if( baseRP.IsFullyQualified ) 
			{
				// if both base and merge are fully qualified, and they're from difference controls, just clone the 
				// merge rp
				if( mergeRP.IsFullyQualified && false == baseRP[baseIndex].IsSameIdentifier(mergeRP[mergeIndex]) )
				{
					for( int i=0;i<mergeRP.TupleCount;++i ) 
					{
						this.tuples.Add( new RefTuple(mergeRP[i]) );
					}
					this.SetFlag( RefParserFlags.Root, mergeRP.IsRoot );
					this.MarkIndices();
					return;
				}

				this.tuples.Add( new RefTuple(baseRP[baseIndex]) );
				++baseIndex;
			}
			if( mergeRP.IsFullyQualified ) 
			{
				++mergeIndex;
			}

			// ignore initial relative elements in both base and merge
			while( baseIndex < baseCount ) 
			{
				RefTuple baseTuple = baseRP[baseIndex];

				if( baseTuple.Type == RefTuple.RefType.Identifier ) 
				{
					break;
				} 

				++baseIndex;
			}

			while( mergeIndex < mergeCount ) 
			{
				RefTuple mergeTuple = mergeRP[mergeIndex];

				if( mergeTuple.Type == RefTuple.RefType.Identifier ) 
				{
					break;
				} 

				++mergeIndex;
			}

			// if the base is at a root, but the merge isn't, we need to advance the base
			if( baseRP.IsRoot && (mergeRP.IsRoot == false) ) 
			{
				RefTuple mergeTuple = mergeRP[mergeIndex];
				while( baseIndex < baseCount ) 
				{
					RefTuple baseTuple = baseRP[baseIndex];
					if( baseTuple.Last == true )
						break;

					// SSP 6/20/11
					// Use the new helper method in the utils.
					// 
					//if( String.Compare(baseTuple.Name,mergeTuple.Name,true) == 0 ) 
					if ( RefParser.AreStringsEqual( baseTuple.Name, mergeTuple.Name, true ) ) 
					{
						break;
					} 

					this.tuples.Add( new RefTuple(baseTuple) );
					++baseIndex;
				}
			}

			// at this point, the RP are aligned.
			while( baseIndex < baseCount && mergeIndex < mergeCount ) 
			{
				RefTuple baseTuple = baseRP[baseIndex];
				RefTuple mergeTuple = mergeRP[mergeIndex];

				// if we have the same non-identifier tuple, just add it to the path
				if( (baseTuple.Type != RefTuple.RefType.Identifier) && baseTuple.IsSameTuple(mergeTuple) ) 
				{
					this.tuples.Add( new RefTuple(baseTuple) );
					++baseIndex;
					++mergeIndex;
					continue;
				}

				// we can ignore relative stuff in the merge path since they point to the same object
				if( mergeTuple.Type != RefTuple.RefType.Identifier ) 
				{
					++mergeIndex;
					continue;
				}

				if( baseTuple.Type != RefTuple.RefType.Identifier ) 
				{
					++baseIndex;
					this.tuples.Add( new RefTuple(baseTuple) );
					continue;
				}

				// if we have tuples with different identifiers. break out of this loop and 
				// add the remaining merge elements to the output path
				if( baseTuple.IsSameIdentifier(mergeTuple) == false ) 
				{
					break;
				} 

				// at this point, we have to tuples with the same identifier .
				// If the merge tuple has scope any, use the base, otherwise use the merge
				// precedence.
				bool bForceDropScope = false;
				if( respectMarked ) 
				{
					if( mergeTuple.Last == false ) 
					{
						RefTuple nextMerged = mergeRP[mergeIndex + 1];
						if( nextMerged.Last ) 
						{
							bForceDropScope = nextMerged.Marked; 
						}
					}
					bForceDropScope = bForceDropScope | mergeTuple.Marked;
				}

				if( bForceDropScope ) 
				{
					this.tuples.Add( new RefTuple(mergeTuple.Name) );

				} 
				else if( mergeTuple.Scope == RefTuple.RefScope.Any ) 
				{
					this.tuples.Add( new RefTuple(baseTuple) );
				} 
				else 
				{
					this.tuples.Add( new RefTuple(mergeTuple) );
				}
				++baseIndex;
				++mergeIndex;
			}

			while( mergeIndex < mergeCount ) 
			{
				RefTuple mergeTuple = mergeRP[mergeIndex];
				if( mergeTuple.Type != RefTuple.RefType.Identifier ) 
				{
					++mergeIndex;
					continue;
				}

				this.tuples.Add( new RefTuple(mergeTuple) );
				++mergeIndex;
			}

			this.MarkIndices();
		}
		#endregion //Merge

		#region BuildReference
		/// <summary>
		/// Populate this RefParser by merging the input baseRP with the input mergeRP. If 
		/// respectMarked is true, then any tuples in mergeRP that are Marked have precedence and
		/// are converted to scope Any.
		/// </summary>
		/// <param name="baseRP">The base reference.</param>
		/// <param name="mergeRP">The reference to merge into the base.</param>
		public void BuildReference( RefParser baseRP, RefParser mergeRP )
		{
			Debug.WriteLine( "BaseRP : " + baseRP.ToString() );
			Debug.WriteLine( "MergeRP : " + mergeRP.ToString() );

			this.SetFlag( RefParserFlags.FullyQualified, baseRP.IsFullyQualified );
			this.SetFlag( RefParserFlags.Root, baseRP.IsRoot );

			// can't perform the operation on a relative base
			if ( baseRP.IsRelative )
				throw new UltraCalcException( SR.GetString( "Error_RP_MergeRelativeBase" ) );

			// can't perform the operation on a relative base
			if ( mergeRP.IsFullyQualified )
				throw new UltraCalcException( SR.GetString( "Error_RP_MergeRelativeBase" ) );


			// Get Base tuple count - 1 since we're replacing at least the last tuple of the base reference
			int baseCount = baseRP.TupleCount - 1;
			int mergeCount = mergeRP.TupleCount;
			int mergeStart = 0;

			// Merge Reference is a root reference
			if ( !mergeRP.IsRoot )
			{
				// For each parent reftype, decrement the number of base tuples we're using and increment the starting position of the merge reference
				for ( int i = 0; i < mergeCount; i++ )
				{
					if ( mergeRP[ i ].Type == RefTuple.RefType.Identifier )
						break;

					if ( mergeRP[ i ].Type == RefTuple.RefType.Parent )
					{
						baseCount--;
						mergeStart++;
					}
					if ( mergeRP[ i ].Type == RefTuple.RefType.ExplicitThis )
					{
						mergeStart++;
					} 
				}
			}
			else
			{
				// Just use the base reference object name
				baseCount = 1;
			}

			// Copy the base reference tuples to this reference
			for ( int i = 0; i < baseCount; i++ )
			{
				this.tuples.Add( new RefTuple( baseRP[ i ] ) );
			}

			// Append 
			for ( int i = mergeStart; i < mergeCount; i++ )
			{
				this.tuples.Add( new RefTuple( mergeRP[ i ] ) );
			}
			this.MarkIndices();

			Debug.WriteLine( "RefRP : " + this.ToString() );
		}
		#endregion //BuildReference 

		#region Flags enum
		[Flags]
			private enum State
		{
			Tuple			= 1,
			TupleEnded		= 2,
			Escape			= 4,
			Scope			= 8,
			QuotedString	= 16
		}
		#endregion //Flags enum

		private static string SPECIAL_CHARACTERS = new string(
			new char[] { 
						   RefParser.RefBeginScope, 
						   RefParser.RefEndScope,
						   RefParser.RefSeperator,
						   RefParser.EscapeCharacter,
						   RefParser.QuotesCharacter,
						   RefParser.NameValuePairSeparatorCharacter, // comma
						   RefParser.NameValueSeparatorCharacter,	  // equal sign
						   '[' , ']'
					   } );

		// Following characters have to escaped when in a quoted string.
		//
		private static string SPECIAL_CHARACTERS_WITHIN_QUOTES = new string(
			new char[] { 
						   RefParser.EscapeCharacter,
						   RefParser.QuotesCharacter,
		} );

		// SSP 9/20/04
		// Optimization technique.
		//
		#region Static Members

		private static readonly long mask_specialCharacters;
		private static readonly long mask_specialCharactersWithinQuotes;

		#region Static Constructor

		static RefParser( )
		{
			mask_specialCharacters = 1;
			mask_specialCharactersWithinQuotes = 1;

			foreach ( char c in RefParser.SPECIAL_CHARACTERS )
			{
				long tmp = mask_specialCharacters * (int)c;				
				if ( tmp > mask_specialCharacters )
				{
					mask_specialCharacters = tmp;
				}
				else
				{
					// We can't use the optmization anymore.
					//
					mask_specialCharacters = 0;
					break;
				}
			}

			foreach ( char c in RefParser.SPECIAL_CHARACTERS_WITHIN_QUOTES )
			{
				long tmp = mask_specialCharactersWithinQuotes * (int)c;				
				if ( tmp > mask_specialCharactersWithinQuotes )
				{
					mask_specialCharactersWithinQuotes = tmp;
				}
				else
				{
					// We can't use the optmization anymore.
					//
					mask_specialCharactersWithinQuotes = 0;
					break;
				}
			}
		}

		#endregion //Static Constructor

		internal static bool DoesCharNeedEscaping( char c, bool withinQuotes )
		{
			return withinQuotes ? DoesCharNeedEscapingWithinQuotes( c ) : DoesCharNeedEscaping( c );
		}

		internal static bool DoesCharNeedEscaping( char c )
		{
			return 0 == mask_specialCharacters % (int)c
				&& RefParser.SPECIAL_CHARACTERS.IndexOf( c ) >= 0;
		}

		internal static bool DoesCharNeedEscapingWithinQuotes( char c )
		{
			return 0 == mask_specialCharactersWithinQuotes % (int)c
				&& RefParser.SPECIAL_CHARACTERS_WITHIN_QUOTES.IndexOf( c ) >= 0;
		}

		#region AreEqual

		// SSP 6/20/11 - XamCalculationManager
		// 
		internal static bool AreStringsEqual( string x, string y, bool ignoreCase )
		{
			return 0 == CompareStrings( x, y, ignoreCase );
		}

		#endregion // AreEqual

		#region CompareStrings

		// SSP 6/20/11 - XamCalculationManager
		// 
		internal static int CompareStrings( string x, string y, bool ignoreCase )
		{
			return String.Compare( x, y, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture );
		}

		#endregion // CompareStrings

		#endregion //Static Members

		// SSP 9/20/04
		// Add the following region of code.
		//
		#region SSP 9/20/04

		#region EscapeString

		/// <summary>
		/// Escapes all the special charactes in the specified text. If withinQuotes is true then it
		/// only escapes the special characters that need to be escaped within double quotes, namely 
		/// the '"' and '\' characters.
		/// </summary>
		/// <param name="text">Text that needs to be escaped.</param>
		/// <param name="withinQuotes">Whether the text is going to be enclosed in double quotes. If true then only escapes characters that need to be escaped in double quotes.</param>
		/// <returns></returns>
		public static string EscapeString( string text, bool withinQuotes )
		{
			System.Text.StringBuilder sb = null;

			for ( int i = 0; i < text.Length; i++ )
			{
				char c = text[ i ];

				if ( RefParser.DoesCharNeedEscaping( c, withinQuotes )
					// Also escape the leading and trailing spaces so they don't loose their
					// meaning when they are reparsed back. For example, in reference 
					// Customers( State\ = NY ), the column in the name-value pair is "State "
					// since the trailing space was escaped. So when escaping "State ", we should
					// end up with "State\ " as well so that when ToString operation is performed
					// on a parsed reference, you get the trailing space escaped in the column 
					// name. 
					//
					|| ( 0 == i || 1 + i == text.Length ) && !withinQuotes && char.IsWhiteSpace( c ) )
				{
					if ( null == sb )
						sb = new System.Text.StringBuilder( text, 0, i, 2 * text.Length - i );

					sb.Append( RefParser.EscapeCharacter ).Append( c );
				}
				else if ( null != sb )
					sb.Append( c );
			}

			return null == sb ? text : sb.ToString();
		}

		#endregion // EscapeString

		#region UnEscapeString

		/// <summary>
		/// Unescapes all the escaped charactes in the specified text.
		/// </summary>
		/// <param name="text">The text whose escaped characters should be unescaped.</param>
		/// <returns>A string with all escaped characters unescaped.</returns>
		public static string UnEscapeString( string text )
		{
			System.Text.StringBuilder sb = null;

			for ( int i = 0; i < text.Length; i++ )
			{
				char c = text[ i ];

				if ( RefParser.EscapeCharacter == c )
				{
					if ( null == sb )
						sb = new System.Text.StringBuilder( text, 0, i, text.Length - 1 );

					i++;
					if ( i < text.Length )
						sb.Append( text[ i ] );
					else
						Debug.Assert( false, "Unescaped escape character \\ encountered." );
				}
				else if ( null != sb )
					sb.Append( c );
			}

			return null == sb ? text : sb.ToString();
		}

		#endregion // UnEscapeString 

		#region NormalizeEscapement

		/// <summary>
		/// This method has the same effect as calling UnEscapeString and then calling EscapeString
		/// on the specified text. This ensures that the text is minimally escaped.
		/// </summary>
		/// <param name="text">The text that should be normalized.</param>
		/// <param name="withinQuotes">Whether the text is going to be enclosed in double quotes. If true then only escapes characters that need to be escaped in double quotes.</param>
		/// <returns>A string that has been unescaped and then escaped.</returns>
		public string NormalizeEscapement( string text, bool withinQuotes )
		{
			return EscapeString( UnEscapeString( text ), withinQuotes );
		}

		#endregion // NormalizeEscapement 

		#region Not Used

		// MD 7/6/07 - 7.3 Performance
		// FxCop - Avoid uncalled private code
		//        #region TrimUnEscapedSpaces

		//#if DEBUG
		//        /// <summary>
		//        /// Trims the passed in string builder of spaces at the beginning and at the end. It
		//        /// does not trim escaped space characters however.
		//        /// </summary>
		//        /// <param name="sb"></param>
		//#endif
		//        internal static void TrimUnEscapedSpaces( System.Text.StringBuilder sb )
		//        {
		//            int firstNonSpaceCharacter = 0;
		//            while ( firstNonSpaceCharacter < sb.Length 
		//                && char.IsWhiteSpace( sb[ firstNonSpaceCharacter ] ) )
		//                firstNonSpaceCharacter++;

		//            // If all the characters in the string are white space characters then return 
		//            // empty string.
		//            //
		//            if ( firstNonSpaceCharacter == sb.Length )
		//            {
		//                sb.Length = 0;
		//                return;
		//            }

		//            int lastNonSpaceCharacter = sb.Length - 1;
		//            while ( lastNonSpaceCharacter > firstNonSpaceCharacter 
		//                && char.IsWhiteSpace( sb[ lastNonSpaceCharacter ] ) )
		//                lastNonSpaceCharacter--;

		//            if ( RefParser.EscapeCharacter == sb[ lastNonSpaceCharacter ] )
		//            {
		//                int escapeCharacterCount = 1;
		//                while ( lastNonSpaceCharacter - escapeCharacterCount >= 0 
		//                    && RefParser.EscapeCharacter == sb[ lastNonSpaceCharacter - escapeCharacterCount ] )
		//                    escapeCharacterCount++;

		//                // If we odd number of escape characters, then the space character following
		//                // the last escape character should not be trimmed.
		//                //
		//                if ( 1 == escapeCharacterCount % 2 )
		//                    lastNonSpaceCharacter++;
		//            }

		//            if ( 1 + lastNonSpaceCharacter < sb.Length )
		//                sb.Remove( 1 + lastNonSpaceCharacter, sb.Length - ( 1 + lastNonSpaceCharacter ) );

		//            if ( firstNonSpaceCharacter > 0 )
		//                sb.Remove( 0, firstNonSpaceCharacter );
		//        }

		//        #endregion // TrimUnEscapedSpaces

		#endregion Not Used

		#region NameValuePair

		/// <summary>
		/// Class for representing a name value pair.
		/// </summary>
        public class NameValuePair
		{
			private object tag	 = null;
			private string name  = null;
			private string value = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="name">Unescaped name.</param>
			/// <param name="value">Unescaped value.</param>
			public NameValuePair( string name, string value )
			{
				this.name = name;
				this.value = value;
			}

			/// <summary>
			/// Returns the name, unescaped.
			/// </summary>
			public string Name
			{
				get
				{
					return this.name;
				}
			}

			/// <summary>
			/// Returns the value, unescaped.
			/// </summary>
			public string Value 
			{
				get
				{
					return this.value;
				}
			}

			/// <summary>
			/// Gets or sets a tag value.
			/// </summary>
			public object Tag
			{
				get
				{
					return this.tag;
				}
				set
				{
					this.tag = value;
				}
			}
		}

		#endregion // NameValuePair

		#region ScopeParseState

	[Flags]
	private enum ScopeParseState
	{
		Initial = 0,
		Escape = 1,
		Column = 2,
		Value = 4,
		Quote = 8
	}

	#endregion // ScopeParseState 

		#region ParseReference

	// SSP 7/20/11 - XamCalculationManager
	// 
	internal static RefParser Parse( string refName )
	{
		return new RefParser( refName );
	} 

		#endregion // ParseReference

		#region ParseColumnValuePairs

		/// <summary>
		/// Parses specified name-value pair. Examples of name-value pairs that this method parses are:
		/// <code>
		/// State = New York, City = New York
		/// State = "New York", City = "New York"
		/// State=New York,City=New York
		/// A\/B = C\/D (column name is A/B and value is C/D.)
		/// A = 10 miles\/hour, B = "20 miles/hour" (Withing double quotes only double quotes and backward slash have to be escaped.)
		/// P1 = 10\, 20, P2 = "10, 20"
		/// </code>
		/// Returns an array of NameValuePair objects. Note: Names and values will be unescaped and also stripped of surrounding quotes.
		/// </summary>
		/// <param name="pairs">A string containing the name-value pair.</param>
		/// <param name="error">A string that will be populated with an error message, should one arise.</param>
		/// <returns>An array of <see cref="RefParser.NameValuePair"/> objects.</returns>
		public static RefParser.NameValuePair[] ParseNameValuePairs( string pairs, out string error )
		{
			NameValuePair[] nameValuePairs;
			string discard;
			error = RefParser.ParseScopeHelper( pairs, out nameValuePairs, out discard );
			return nameValuePairs;
		}

		#endregion // ParseNameValuePairs

		#region ParseScopeHelper

		internal static void TrimTrailingSpaces( System.Text.StringBuilder sb, int lastEscapedCharIndex )
		{
			// Remove trailing spaces from the column name. For example, State = New York 
			// should remove the space after State and before the equal sign. Don't remove 
			// a space character that was escaped.
			//
			for ( int j = sb.Length - 1; j >= 0 && j > lastEscapedCharIndex; j-- )
			{
				if ( char.IsWhiteSpace( sb[ j ] ) )
					sb.Remove( j, 1 );
				else
					break;
			}
		} 

		/// <summary>
		/// Helper method for parsing scope text. This parses name-value pairs as well as standalone
		/// string values. Example of a name value pair is <code>State=New York</code> where as example
		/// of a standalone string value is <code>New York</code>. Standalone text values are used to
		/// specify the scope in group-by rows. Example of a reference using standalone scope and a
		/// name value pair scope is <code>//ultraGrid1/Customers/State(NY)/Customers(CustomerID=ABCD)</code>.
		/// If the scope was name-value pairs then sets the nameValuePairs to an array of parsed name-value
		/// pairs. If the scope was a standalone text then sets the nonNameValuePairScope parameter.
		/// If there was an error, returns an error string. If there was no error, returns null.
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="nameValuePairs"></param>
		/// <param name="nonNameValuePairScope"></param>
		/// <returns>Returns an error string if any.</returns>
		internal static string ParseScopeHelper( string scope, out NameValuePair[] nameValuePairs, out string nonNameValuePairScope )
		{
			nameValuePairs = null;
			nonNameValuePairScope = null;

			bool hasError = false;
			string error = null;
			ScopeParseState state = ScopeParseState.Column;
			ArrayList list = new ArrayList( 4 );
			System.Text.StringBuilder columnName = new System.Text.StringBuilder();
			System.Text.StringBuilder cellValue = new System.Text.StringBuilder();

			int lastEscapedCharIndex = -1;
			int i = 0;
			while ( i < scope.Length )
			{
				char c = scope[ i ];

				if ( 0 != ( ScopeParseState.Escape & state ) )
				{
					if ( 0 != ( ScopeParseState.Column & state ) )
					{
						lastEscapedCharIndex = columnName.Length;
						columnName.Append( c );
					}
					else
					{
						lastEscapedCharIndex = cellValue.Length;
						cellValue.Append( c );
					}

					state ^= ScopeParseState.Escape;
				}
				else if ( RefParser.EscapeCharacter == c )
				{
					state |= ScopeParseState.Escape;
				}
				else if ( ScopeParseState.Quote == state && RefParser.QuotesCharacter == c )
				{
					// We have encountered a closing double quotes. Double quotes are only allowed
					// in value portion so it indicates the end of a name-value pair.
					//
					list.Add( new NameValuePair( columnName.ToString(), cellValue.ToString() ) );
					columnName.Length = cellValue.Length = 0;
					lastEscapedCharIndex = -1;
					state = ScopeParseState.Initial;
				}
				else if ( ScopeParseState.Quote == state )
				{
					// Allow everything while in double quotes.
					//
					cellValue.Append( c );
				}
				else if ( char.IsWhiteSpace( c ) &&
					( ScopeParseState.Column == state && 0 == columnName.Length
					|| ScopeParseState.Value == state && 0 == cellValue.Length
					|| ScopeParseState.Initial == state ) )
				{
					// Skip spaces in the beginning of the column or the value. Note that this 
					// doesn't skip spcaes withing double quotes since we would've had the Quote
					// state.
					//
				}
				else if ( ScopeParseState.Column == state && RefParser.NameValueSeparatorCharacter == c )
				{
					if ( 0 == columnName.Length )
					{
						// If we encounter '=' sign without any column name before it then the 
						// column-value pairs is invalid.
						//
						error = SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", c, scope, i );
						hasError = true;
						break;
					}

					// Remove trailing spaces from the column name. For example, State = New York 
					// should remove the space after State and before the equal sign. Don't remove 
					// a space character that was escaped.
					//
					RefParser.TrimTrailingSpaces( columnName, lastEscapedCharIndex );
					state = ScopeParseState.Value;
				}
				else if ( ScopeParseState.Value == state && RefParser.QuotesCharacter == c )
				{
					if ( cellValue.Length > 0 )
					{
						// If we encounter an unescaped double quote in the middle of a value
						// then the column value pair specificaion is invalid.
						//
						error = SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", c, scope, i );
						hasError = true;
						break;
					}

					state = ScopeParseState.Quote;
				}
				else if ( ScopeParseState.Value == state && RefParser.NameValuePairSeparatorCharacter == c && 0 != cellValue.Length )
				{
					// Remove trailing spaces from the value. For example, in "A = 1 , B = 2" scope,
					// value is "1" and not "1 ".
					//
					RefParser.TrimTrailingSpaces( cellValue, lastEscapedCharIndex );
					list.Add( new NameValuePair( columnName.ToString(), cellValue.ToString() ) );
					columnName.Length = cellValue.Length = 0;
					lastEscapedCharIndex = -1;
					state = ScopeParseState.Column;
				}
				else if ( ScopeParseState.Initial == state && RefParser.NameValuePairSeparatorCharacter == c )
				{
					state = ScopeParseState.Column;
				}
				else if ( RefParser.DoesCharNeedEscaping( c ) )
				{
					error = SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", c, scope, i );
					hasError = true;
					break;
				}
				else if ( ScopeParseState.Column == state )
				{
					columnName.Append( c );
				}
				else if ( ScopeParseState.Value == state )
				{
					cellValue.Append( c );
				}
				else
				{
					Debug.Assert( false, "We shouldn't have gotten here." );
					error = SR.GetString( "Error_RT_InvalidScope", scope );
					hasError = true;
					break;
				}

				i++;
			}

			if ( !hasError )
			{
				RefParser.TrimTrailingSpaces( cellValue, lastEscapedCharIndex );

				if ( i >= scope.Length && ScopeParseState.Value == state && 0 != columnName.Length && 0 != cellValue.Length )
				{
					list.Add( new NameValuePair( columnName.ToString(), cellValue.ToString() ) );
				}
				else if ( ScopeParseState.Initial != state || i < scope.Length )
				{
					hasError = true;
					if ( ScopeParseState.Escape == state )
						error = SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", scope[ scope.Length - 1 ], scope, scope.Length - 1 );
					else if ( ScopeParseState.Quote == state )
						error = SR.GetString( "Error_RP_MissingEndQuotes", scope );
					else
						error = SR.GetString( "Error_RT_InvalidScope", scope );
				}
			}

			if ( list.Count <= 0 )
			{
				// Since the scope wasn't a name-value pair, try to see if it's just a standalone value
				// as in //ultraGrid1/Customers/State(NY) where State would be a group-by column.
				// 
				state = ScopeParseState.Value;
				cellValue.Length = 0;
				i = 0;
				lastEscapedCharIndex = -1;
				while ( i < scope.Length )
				{
					char c = scope[ i ];
					if ( 0 != ( ScopeParseState.Escape & state ) )
					{
						lastEscapedCharIndex = cellValue.Length;
						cellValue.Append( c );
						state ^= ScopeParseState.Escape;
					}
					else if ( RefParser.EscapeCharacter == c )
					{
						state |= ScopeParseState.Escape;
					}
					else if ( RefParser.QuotesCharacter == c && ScopeParseState.Quote == state )
					{
						// Goto Initial state if we encounter the closing double quotes.
						//
						state = ScopeParseState.Initial;
					}
					else if ( RefParser.QuotesCharacter == c && ScopeParseState.Value == state )
					{
						if ( cellValue.Length > 0 )
						{
							// If we encounter an unescaped double quote in the middle of a value
							// then the column value pair specificaion is invalid.
							//
							error = SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", c, scope, i );
							break;
						}

						state = ScopeParseState.Quote;
					}
					else if ( ScopeParseState.Initial == state )
					{
						// If we encounter anything other than spaces after closing double quotes, 
						// the scope is invalid.
						// 
						if ( !char.IsWhiteSpace( c ) )
						{
							error = SR.GetString( "Error_RT_InvalidScope_CharactersAfterClosingQuotes", scope, i );
							break;
						}
					}
					else if ( ScopeParseState.Value == state && 0 == cellValue.Length && char.IsWhiteSpace( c ) )
					{
						// Skip spaces in the beginning of the scope.
						//
					}
					else if ( ScopeParseState.Quote != state && RefParser.DoesCharNeedEscaping( c ) )
					{
						error = SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", c, scope, i );
						break;
					}
					else
					{
						cellValue.Append( c );
					}

					i++;
				}

				// If we were able to properly parse the standalone value scope, then return that.
				//
				if ( i >= scope.Length )
				{
					RefParser.TrimTrailingSpaces( cellValue, lastEscapedCharIndex );
					nonNameValuePairScope = cellValue.ToString();
					error = null;
				}
			}
			else
			{
				if ( !hasError )
					nameValuePairs = (NameValuePair[])list.ToArray( typeof( NameValuePair ) );
			}

			return error;
		} 

		#endregion // ParseScopeHelper

		#region ParseReference



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		// MD 6/25/12 - TFS113177
		//private string ParseReference( string refName )
		private string ParseReference(string refName, bool maintainCase)
		{
			if ( null == refName )
				return SR.GetString( "Error_RT_NullRefName" );

			// Check to see if it's an absolute reference....it's absolute if it begins with 
			// the RefFullyQualifiedString token.
			//
			int index;
			if ( refName.StartsWith( RefParser.RefFullyQualifiedString ) )
			{
				this.flags |= RefParserFlags.FullyQualified | RefParserFlags.Root;
				index = RefParser.RefFullyQualifiedString.Length;
			}
			else if ( refName.Length > 0 && refName[ 0 ] == RefParser.RefSeperator )
			{
				this.flags &= ~RefParserFlags.FullyQualified;
				this.flags |= RefParserFlags.Root;
				index = 1;
			}
			else
			{
				this.flags &= ~( RefParserFlags.FullyQualified | RefParserFlags.Root );
				index = 0;
			}

			this.tuples.Clear();
			State state = State.Tuple;
			System.Text.StringBuilder tuple = new System.Text.StringBuilder();

			for ( ; index < refName.Length; index++ )
			{
				char c = refName[ index ];

				if ( 0 != ( State.Escape & state ) )
				{
					tuple.Append( c );
					state ^= State.Escape;
				}
				else if ( RefParser.EscapeCharacter == c )
				{
					// Keep the escape characters where they are at this point since the RefTuple's
					// parsing logic will take them out. The reason is that escaped space characters
					// loose their meaning if we unescape them here. They have to be unescaped in
					// RefTuple's parsing logic as a result.
					// 
					tuple.Append( c );
					state |= State.Escape;
				}
				else if ( RefParser.QuotesCharacter == c && 0 != ( State.QuotedString & state ) )
				{
					tuple.Append( c );
					state ^= State.QuotedString;
				}
				else if ( 0 != ( State.QuotedString & state ) )
				{
					// Skip all characters within double quotes within a scope. For example,
					// in //ultraGrid1/Band 0("Miles/hour"="10m/h")/id reference, '/' characters
					// in quoted column and value portions should not be treated specially.
					// This applies to all characters except the '"' iteself since that's 
					// an indicator of closing quote. '"' must be escaped.
					//
					tuple.Append( c );
				}
				else if ( RefParser.RefSeperator == c && State.TupleEnded == state )
				{
					// If previously the tuple was ended because we encountered end parenthesis
					// then the next character must be a tuple seperator character '/'.
					//
					state = State.Tuple;
				}
				else if ( RefParser.RefSeperator == c && State.Tuple == state )
				{
					// Check to see if there was an empty tuple.
					//
					if ( 0 == tuple.Length )
						return SR.GetString( "Error_RP_EmptyElement", refName, index );

					// MD 6/25/12 - TFS113177
					//this.tuples.Add( new RefTuple( tuple.ToString() ) );
					this.tuples.Add(new RefTuple(tuple.ToString(), maintainCase));

					tuple.Length = 0;
				}
				else if ( RefParser.RefBeginScope == c && State.Tuple == state )
				{
					// Check to see if there was an empty tuple. You can't have begin scope character
					// '(' without preceding tuple identifier.
					//
					if ( 0 == tuple.Length )
						return SR.GetString( "Error_RP_EmptyElement", refName, index );

					tuple.Append( c );
					state |= State.Scope;
				}
				else if ( RefParser.RefEndScope == c && 0 != ( State.Scope & state ) )
				{
					tuple.Append( c );

					// MD 6/25/12 - TFS113177
					//this.tuples.Add( new RefTuple( tuple.ToString() ) );
					this.tuples.Add(new RefTuple(tuple.ToString(), maintainCase));

					tuple.Length = 0;

					// Remove the Scope and Tuple states and add TupleEnded to indicate that
					// the next character if any must be '/'. Scope can only occur in Tuple state.
					//
					state ^= State.Scope | State.Tuple;
					state |= State.TupleEnded;
				}
				else if ( RefParser.QuotesCharacter == c && 0 != ( State.Scope & state ) )
				{
					tuple.Append( c );
					state |= State.QuotedString;
				}
				// Don't check for unescaped special characters in the scope portion because the
				// parsing logic for the RefTuple (RefTuple.Parse method) will take care of that.
				//
				else if ( 0 == ( State.Scope & state ) && RefParser.DoesCharNeedEscaping( c ) )
				{
					return SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", c, refName, index );
				}
				else
				{
					tuple.Append( c );
				}
			}

			if ( 0 != ( State.Escape & state ) )
			{
				return SR.GetString( "Error_RT_InvalidTuple_UnescapedCharacter", refName[ refName.Length - 1 ], refName, refName.Length - 1 );
			}
			else if ( State.Tuple == state )
			{
				// Check to see if there was an empty tuple.
				//
				if ( 0 == tuple.Length )
				{
					if ( this.tuples.Count <= 0 )
						return SR.GetString( "Error_RP_EmptyReference", refName );
					else
						return SR.GetString( "Error_RP_EmptyElement", refName, index );
				}

				// MD 6/25/12 - TFS113177
				//this.tuples.Add( new RefTuple( tuple.ToString() ) );
				this.tuples.Add(new RefTuple(tuple.ToString(), maintainCase));

				tuple.Length = 0;
			}
			else if ( tuple.Length > 0 )
			{
				if ( 0 != ( State.QuotedString & state ) )
					return SR.GetString( "Error_RP_MissingEndQuotes", refName );

				if ( 0 != ( State.Scope & state ) )
					return SR.GetString( "Error_RP_MissingCloseParenthesis", refName );
			}

			this.MarkIndices();

			return null;
		}

		#endregion // ParseReference 

		#endregion // SSP 9/20/04

		// SSP 9/21/04
		// Origianl Parse method. Added new parsing logic that allows for escaping special 
		// characters above.
		//
		
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)


		#region Contains
		/// <summary>
		/// Returns true if the input RefParser in contained by this RefParser.  The RefParser is
		/// contained if each RefTuple in this RefParser contains the corresponding RefTuple in the
		/// input RefParser.
		/// </summary>
		/// <param name="inputParsed">The contained candidate</param>
		/// <returns>true if inputParsed is contained by this.</returns>
		/// <seealso cref="RefTuple.Contains"/>
		public bool Contains( RefParser inputParsed )
		{
			if ( inputParsed.TupleCount < this.TupleCount )
				return false;

			IEnumerator thisEnum = this.GetEnumerator();
			IEnumerator inputEnum = inputParsed.GetEnumerator();
			bool haveThisElement = thisEnum.MoveNext();
			if ( haveThisElement == false )
				throw new UltraCalcException( SR.GetString( "Error_RP_ContainsBase" ) );

			bool haveInputElement = inputEnum.MoveNext();
			if ( haveInputElement == false )
				throw new UltraCalcException( SR.GetString( "Error_RP_ContainsTarget" ) );

			while ( haveThisElement && haveInputElement )
			{
				RefTuple thisTuple = (RefTuple)thisEnum.Current;
				RefTuple inputTuple = (RefTuple)inputEnum.Current;
				if ( thisTuple.Contains( inputTuple ) == false )
					return false;

				haveThisElement = thisEnum.MoveNext();
				haveInputElement = inputEnum.MoveNext();
			}

			return true;
		}
		#endregion //Contains 

		#region IsSubset
		/// <summary>
		/// Returns true if the input RefParser is a proper subset of this RefParser.  The RefParser is
		/// a proper subset if each RefTuple in this RefParser has a scope that completely contains 
		/// the corresponding RefTuple in the
		/// input RefParser.
		/// </summary>
		/// <param name="inputParsed">The subset candidate</param>
		/// <returns>true if inputParsed is a propersubset of this.</returns>
		/// <seealso cref="RefTuple.IsSubset"/>
		public bool IsSubset( RefParser inputParsed ) 
		{
			if( inputParsed.TupleCount < this.TupleCount ) 
				return false;

			IEnumerator thisEnum = this.GetEnumerator();
			IEnumerator inputEnum = inputParsed.GetEnumerator();
			bool haveThisElement = thisEnum.MoveNext();
			if( haveThisElement == false ) 
				throw new UltraCalcException( SR.GetString( "Error_RP_ContainsBase") );

			bool haveInputElement = inputEnum.MoveNext();
			if( haveInputElement == false ) 
				throw new UltraCalcException( SR.GetString( "Error_RP_ContainsTarget") );

			while( haveThisElement && haveInputElement ) 
			{
				RefTuple thisTuple = (RefTuple)thisEnum.Current;
				RefTuple inputTuple = (RefTuple)inputEnum.Current;
				if( thisTuple.IsSubset(inputTuple) == false ) 
					return false;

				haveThisElement = thisEnum.MoveNext();
				haveInputElement = inputEnum.MoveNext();
			}

			return true;
		}
		#endregion //IsSubset
	
		#region ToString
		// SSP 9/21/04
		// Added ToString overload that takes in a string builder.
		//
		/// <summary>
		/// Appends the string representation of the parsed reference to the specified string builder.
		/// </summary>
        /// <param name="sb">The <see cref="System.Text.StringBuilder"/> onto which the string representation of this object should be appended.</param>
		public void ToString( System.Text.StringBuilder sb )
		{
			// SSP 9/21/04
			// Commented out the original code and added the new one.
			//
			if ( this.IsFullyQualified ) 
			{
				sb.Append( RefParser.RefFullyQualifiedString );
			} 
			else if ( this.IsRoot ) 
			{
				sb.Append( RefParser.RefSeperatorString );
			}

			for ( int i = 0; i < this.tuples.Count; i++ )
			{
				if ( i > 0 )
					sb.Append( RefParser.RefSeperator );

				((RefTuple)this.tuples[i]).ToString( sb );
			}

			// Original code.
			//
			
#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)

		}

		/// <summary>
		/// Returns a string representation of the parsed reference.
		/// </summary>
        /// <returns>A string representation of the parsed reference.</returns>
		public override string ToString( )
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder( );
			this.ToString( sb );
			return sb.ToString( );
		}
		#endregion //ToString

		#endregion //Methods

		#region IEnumerable

		/// <summary>
		/// Returns the tuples IEnumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return this.tuples.GetEnumerator();
		}

		#endregion //IEnumerable  
	}
	#endregion

	#region RefUnAnchored

	/// <summary>
	/// RefUnAnchored is a wrapper for a RefBase reference that keeps a parsed representation of the 
	/// reference string used to create the reference.  There can be multiple RefUnAnchored instances 
	/// wrapping the same anchored reference.
	/// </summary>
	public class RefUnAnchored : RefBase
	{
		/// <summary>
		/// The RefBase being wrapped.
		/// </summary>
		protected RefBase baseRef;

		/// <summary>
		/// The parsed form of the reference string used to create the reference.
		/// </summary>
		protected RefParser relativeRP;

		/// <summary>
		/// Constructor takes a base reference.
		/// </summary>
		/// <param name="baseRef">The base reference to wrap</param>
		public RefUnAnchored( RefBase baseRef )
		{
			this.baseRef = baseRef;
			this.relativeRP = null;
		}

		/// <summary>
		/// Returns true if the relative reference is unqualified.  Read Only.
		/// </summary>
		public virtual bool IsUnQualifiedReference
		{
			get
			{
				return this.RelativeReference.TupleCount == 1;
			}
		} 

		#region RefUnanchored Overrides of RefBase

		/// <summary>
		/// The unqualified name of this referenced element.  Used to construct the fully qualified name by the
		/// <b>AbsoluteName</b> method. Read Only.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <seealso cref="AbsoluteName"/>
		public override string ElementName { get { return this.baseRef.ElementName; } }

		/// <summary>
		/// The RefBase Parent reference of this referenced element.  Used to navigate the element hierarchy.  If this
		/// referenced element is the root element, <b>BaseParent</b> returns null.  Read Only.
		/// </summary>
		/// <remarks>
		/// <p>RefUnAnchored forwards this to the base reference.</p>
		/// </remarks>
		public override RefBase BaseParent { get { return this.baseRef.BaseParent; } } 
		
		/// <summary>
		/// Returns true if this reference is a Range Reference.  Read Only.
		/// <p>RefUnAnchored forwards this to the base reference.</p>
		/// </summary>
		/// <seealso cref="RefRange"/>
		public override bool IsRange {get {return this.baseRef.IsRange;} }

		/// <summary>
		/// Returns true if this reference is anchored.  An anchored reference is a direct reference to an element. 
		/// An unanchored reference is an indirect reference.  An indirect reference stores context dependent properties
		/// such as the relative name used to create the reference.  Read Only.
		/// <p>RefUnAnchored always returns false.
		/// </p>
		/// </summary>
		/// <seealso cref="RefUnAnchored"/>
		/// <seealso cref="RelativeReference"/>
		public override bool IsAnchored { get { return false; } }

		/// <summary>
		/// Returns true if this reference is a data reference.  A data reference contains a value, such as a Grid Cell or
		/// a Grid SummaryValue.  Many referenced elements, such as a Grid Column or a Grid Band, do not reference a value.
		/// Read Only. 
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		public override bool IsDataReference { get { return this.baseRef.IsDataReference; } } 


		/// <summary>
		/// Returns the indirect reference of an UnAnchored reference.  Read only.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <seealso cref="RefUnAnchored"/>
		/// <seealso cref="IsAnchored"/>
		public override RefBase WrappedReference { get { return this.baseRef.WrappedReference; } }

		/// <summary>
		/// Gets or Sets the parsed representation of this reference.  This returns the absolute reference name.  Use
		/// <b>RelativeReference</b> to get the parsed representation of the string used to create this reference if this 
		/// reference is UnAnchored.
		/// <p>RefUnAnchored returns a reference constructed from merging the relative reference into the base reference.
		/// </p>
		/// </summary>
		/// <seealso cref="IsAnchored"/>
		/// <seealso cref="RelativeReference"/>
		public override RefParser ParsedReference
		{
			get { return base.ParsedReference; }
			set { base.ParsedReference = value; }
		}

		/// <summary>
		/// The fully qualified unique name for the referenced element.  Read Only.
		/// </summary>
		public override string AbsoluteName
		{
			get
			{
				return ParsedReference.ToString();
			}
		}

		/// <summary>
		/// Gets or Sets the parsed representation of the string used to create this reference if this reference
		/// is UnAnchored.  Use <b>AbsoluteReference</b> to get the parsed representation of the anchored reference.
		/// <p>RefUnAnchored returns the relative reference. When setting the relative reference, the merge 
		/// reference is constructed by merging the relative reference into the base reference.
		/// </p>
		/// </summary>
		/// <seealso cref="IsAnchored"/>
		public override RefParser RelativeReference
		{
			get { return this.relativeRP; }
			set
			{
				this.relativeRP = value;
				RefParser rp = new RefParser();

				// After the merge, we need to adjust the next-to-last tuple if relativeRP is NOT fully qualified and
				// ends in a RelativeIndex, and the next-to-last tuple of the base reference 
				// has a scope of AbsoluteIndex.  This is to deal with the following situation.  
				// if you called CreateReference on /row(1)/column passing in column(-1), the resulting
				// reference would be /row(0)/column.  The merge would yield
				// /row(0)/column(-1) which would be incorrect.  We want to keep the relative index, so we
				// need to adjust the absolute index of the row.  After the adjustment, we have /row(1)/column(-1),
				// which is the same thing as /row(0)/column.
				rp.Merge( this.baseRef.ParsedReference, this.relativeRP, false );
				if ( rp.NextToLastTuple != null && this.relativeRP.IsRelative )
				{
					if ( rp.LastTuple.Scope == RefTuple.RefScope.RelativeIndex && rp.NextToLastTuple.Scope == RefTuple.RefScope.Index )
					{
						rp.NextToLastTuple.ScopeIndex -= rp.LastTuple.ScopeIndex;
					}
				} 

				this.ParsedReference = rp;

			}
		}

		/// <summary>
		/// Returns the named reference relative to this reference.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		public override IUltraCalcReference FindItem( string name )
		{
			return this.baseRef.FindItem( name );
		}

		/// <summary>
		/// Returns the named reference relative to this reference with scope "ALL".
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		/// <seealso cref="RefBase.BuildReference"/>
		public override IUltraCalcReference FindAll( string name )
		{
			return this.baseRef.FindAll( name );
		}

		/// <summary>
		/// Returns the named SummaryItem reference relative to this reference.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		/// <seealso cref="RefBase.BuildReference"/>
		public override IUltraCalcReference FindSummaryItem( string name )
		{
			return this.baseRef.FindSummaryItem( name );
		} 

		/// <summary>
		/// Returns the named indexed reference relative to this reference.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <param name="index">The index name.</param>
		/// <returns>The referenced element</returns>
		/// <seealso cref="RefBase.BuildReference"/>
		public override IUltraCalcReference FindItem( string name, string index )
		{
			return this.baseRef.FindItem( name, index );
		}

		/// <summary>
		/// Returns the named indexed reference relative to this reference.  If <b>isRelative</b>
		/// is true, then <b>index</b> is treated as a relative to this element index.  Otherwise the
		/// index is treated as absolute.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <param name="index">The index value.</param>
		/// <param name="isRelative">true if <b>index</b> is relative to this index</param>
		/// <returns>The referenced element</returns>
		/// <seealso cref="RefBase.BuildReference"/>
		public override IUltraCalcReference FindItem( string name, int index, bool isRelative )
		{
			return this.baseRef.FindItem( name, index, isRelative );
		}

		/// <summary>
		/// Returns a reference to this elements Parent.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <returns>The referenced element</returns>
		/// <seealso cref="RefBase.BuildReference"/>
		public override RefBase FindParent()
		{
			return this.baseRef.FindParent();
		} 

		/// <summary>
		/// Returns a <see cref="IUltraCalcReferenceCollection"/> for enumerable references constrained to the specified reference.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <param name="scopeRP">The constraining reference</param>
		/// <returns>A reference collection</returns>
		public override IUltraCalcReferenceCollection ScopedReferences( RefParser scopeRP )
		{
			return this.baseRef.ScopedReferences( scopeRP );
		}

		// MD 6/25/12 - TFS113177

		/// <summary>
		/// Gets the value indicating whether the formula editor should include default indexes after this reference's address when 
		/// enumerable references are used where a single value is expected.
		/// </summary>
		public override bool ShouldFormulaEditorIncludeIndex
		{
			get { return this.baseRef.ShouldFormulaEditorIncludeIndex; }
		}


		#endregion

		#region RefUnAnchored Overrides of IUltraCalcReference

		/// <summary>
		/// Returns a context for the Reference.
		/// </summary>
		/// <remarks><p class="body">Returns a meaningful object context for the reference. This could be a data grid Cell, Field, NamedReference, Control, etc...</p></remarks>






		public override object Context { get { return this.baseRef.Context; } }

		/// <summary>
		/// The <see cref="UltraCalcValue"/>, if any, associated with this Reference.  If this reference 
		/// does not contain a Value then a <see cref="UltraCalcErrorValue"/> is returned.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		public override UltraCalcValue Value
		{
			get { return this.baseRef.Value; }
			set { this.baseRef.Value = value; }
		}

		/// <summary>
		/// The <see cref="IUltraCalcFormula"/>, if any, associated with this Reference.  If this reference 
		/// can not contain a formula then null is returned.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		public override IUltraCalcFormula Formula
		{
			get { return this.baseRef.Formula; }
		}

		/// <summary>
		/// Flag used by the calculation engine to manage the recalculation network. 
		/// Denotes the reference is visible.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		public override bool RecalcVisible
		{
			get { return this.baseRef.RecalcVisible; }
			set { this.baseRef.RecalcVisible = value; }
		}

		/// <summary>
		/// True indicates that recalculating reference's formula can deferred until it become visible.
		/// </summary>
		public override bool RecalcDeferred
		{
			get { return this.baseRef.RecalcDeferred; }
			set { this.baseRef.RecalcDeferred = value; }
		}

		/// <summary>
		/// True indicates that the reference was constructed with a relative index.  Read Only.
		/// <p>RefUnAnchored uses the relative rp.
		/// </p>
		/// </summary>
		public override bool HasRelativeIndex
		{
			get { return this.relativeRP.HasRelativeIndex; }
		}

		/// <summary>
		/// True indicates that the reference was constructed with a scope all index.  Read Only.
		/// </summary>
		public override bool HasScopeAll
		{
			get { return this.relativeRP.HasScopeAll; }
		}

		/// <summary>
		/// True indicates that the reference was constructed with a absolute index.  Read Only.
		/// <p>RefUnAnchored uses the relative rp.
		/// </p>
		/// </summary>
		public override bool HasAbsoluteIndex
		{
			get { return this.relativeRP.HasAbsoluteIndex; }
		} 

		/// <summary>
		/// Return a collection of references if this Reference is enumerable.  An exception is
		/// thrown if this reference is not enumerable.
		/// <p>RefUnAnchored returns the baseRef enumeration scoped to the merged reference.
		/// </p>
		/// </summary>
		/// <returns>A Reference collection.</returns>
		public override IUltraCalcReferenceCollection References
		{
			get
			{
				return this.baseRef.ScopedReferences( this.ParsedReference );
			}
		}

		/// <summary>
		/// Resolves a reference relative to this reference.  For example, if this
		/// reference is a reference to a row, and inReference is a reference to a
		/// column, the resolved reference is the cell reference that is the intersection
		/// of this row and the input column.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <param name="reference">The reference to resolve.</param>
		/// <param name="referenceType">Enumeration indicating the type of reference to resolve.</param>
		/// <returns>The resolved reference.</returns>
		public override IUltraCalcReference ResolveReference( IUltraCalcReference reference, ResolveReferenceType referenceType )
		{
			return this.baseRef.ResolveReference( reference, referenceType );
		} 

		/// <summary>
		/// Returns true if this reference contains inReference
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		/// <param name="reference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public override bool ContainsReference( IUltraCalcReference reference )
		{
			return this.baseRef.ContainsReference( reference );
		}

		/// <summary>
		/// Returns true if inReference is a proper subset of this reference
		/// </summary>
		/// <param name="reference">The subset candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public override bool IsSubsetReference( IUltraCalcReference reference )
		{
			return this.baseRef.IsSubsetReference( reference );
		}

		/// <summary>
		/// Determines whether the given reference is a sibling of this reference
		/// </summary>
		/// <param name="reference">The reference to compare against this one</param>
		/// <returns>True if the reference is a sibling of the given reference</returns>
		public override bool IsSiblingReference( IUltraCalcReference reference )
		{
			return this.baseRef.IsSiblingReference( reference );
		} 

		/// <summary>
		/// Returns true if this reference is enumerable.
		/// <p>RefUnAnchored forwards this to the base reference.
		/// </p>
		/// </summary>
		public override bool IsEnumerable
		{
			get { return this.baseRef.IsEnumerable; }
		}


		#endregion

		/// <summary>
		/// Returns a string representation of the object.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format( "{0} ({1}) -{2}", this.GetType(), this.baseRef.GetType(), this.ParsedReference );
		}

		// SSP 10/12/04
		// Overrode GetHashCode and Equals methods.
		//
		// hasSameAbsoluteNameAsBaseRef: -1 means hasn't been decided, 0 means false, 1 means true.
		//
		private int hasSameAbsoluteNameAsBaseRef = -1;

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <returns>A hash code that identifies this instance.</returns>
		public override int GetHashCode()
		{
			int hashCode = this.baseRef.GetHashCode();

			if ( -1 == this.hasSameAbsoluteNameAsBaseRef )
				this.hasSameAbsoluteNameAsBaseRef = this.NormalizedAbsoluteName == this.baseRef.NormalizedAbsoluteName ? 1 : 0;

			// If absolute name of the ref unanchored is different than the base ref then take into
			// account the absolute name.
			//
			if ( 0 == hasSameAbsoluteNameAsBaseRef )
				hashCode ^= this.NormalizedAbsoluteName.GetHashCode();

			return hashCode;
		}

		/// <summary>
		/// Overriden.
		/// </summary>
		/// <param name="obj">The objects to test for equality.</param>
		/// <returns>True if the absolute name is the same as the base reference and the base implementation considers them equal.</returns>
		public override bool Equals( object obj )
		{
			RefUnAnchored ru = obj as RefUnAnchored;
			if ( null != ru )
			{
				return this.baseRef.Equals( ru.baseRef )
					&& ( 1 == this.hasSameAbsoluteNameAsBaseRef && 1 == ru.hasSameAbsoluteNameAsBaseRef
							|| this.NormalizedAbsoluteName == ru.NormalizedAbsoluteName );
			}

			IUltraCalcReference testReference = obj as IUltraCalcReference;

			// If the absolute name of this ref unanchored is the same as the base ref then
			// simply compare the base ref with the test reference.
			//
			return null != testReference && 1 == this.hasSameAbsoluteNameAsBaseRef && this.baseRef.Equals( testReference );
		}

		#region IsDisposedReference

		// SSP 7/7/05
		// Overrode IsDisposedReference so we can return false if the wrapped reference
		// is disposed.
		// 
		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return this.baseRef.IsDisposedReference;
			}
		}

		#endregion // IsDisposedReference

	}

	#endregion 

	#region UCReference

	/// <summary>
	/// Implements the IUltraCalcReference and provides methods to connect and disconnect from an underlying calculation object reference
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// UCReference wraps an internal IUltraCalcReference class instance that points to the underlying object.  That is, each method simply passes 
	/// the IUltraCalcReference request to the underlying reference if the object is connected, else it returns, or throws, an appropriate error.  
	/// UCReference has storage for the reference string and its underlying reference object.  
	/// <p></p>
	/// There are two additional methods on UCReference: Connect and Disconnect.  
	/// <p></p>
	///	Connect will attempt to obtain a reference to the object it refers to given a base reference and it's reference string by calling the base 
	///	reference's CreateReference method passing it the reference string.  If its successful, it will store the newly created reference and use 
	///	it to dispatch IUltraCalcReference requests against it.
	/// <p></p>
	/// Disconnect will simply free the internal reference object after which its IUltraCalcReference methods will deal with a disconnected state.
	/// <p></p>
	/// Virtually all references can be in a disconnected state at some point in their lifetime, therefore the CalcEngine uses UCReference instances to
	/// connect to all references.  That is, the UltraCalcFormulaCompiler.Compile method creates UCRefernces for the all references in the formula string.  The CalcEngine exposes 
	/// a <b>TopoChange</b> event that objects used to notify it of topological changes.  The TopoChange event passes a reference to the CalcEngine
	/// that describes the object that changed (e.g. column, row, grid, etc) along with a Create / Delete action argument.  
	/// <p></p>
	/// On Delete events, the CalcEngine will track down all formulas whose LHS is contained by the disconnected reference and delete them.  
	/// This will clean up the recalc chain and ancestor map of any references including sub-references under the deleted reference.  That is, 
	/// column formulas will be deleted when the event's deleted reference is an entire grid, or cell references that are on the recalc chain when 
	/// the deleted reference is a column.  The ContainsReference on the reference passed with the TopoChange event determines whether the event
	/// affects a given reference.  Secondly, all formulas who reference the deleted reference will be recalculated.
	/// <p></p>
	/// On Create events, the CalcEngine will track down all references contained by the created reference and call the Connect method passing in the 
	/// their formula's BaseReference.  This will cause the CalcReference to reconnect to the underlying reference.  Secondly, each re-connected 
	/// reference's ancestor's will be recalculated.
	/// </p>
	/// </remarks>

    internal class UCReference : IUltraCalcReference



	{
		#region Member Variables

		private UltraCalcFormulaStates formulaStates;	// Storage for the formula states when object is disconnected  
		private string referenceName;			// Storage for the reference string within the formula

		private IUltraCalcReference reference;	// Storage for the underlying object's reference instance
		private bool connected;					// Storage for boolean that denotes whether the object is connected or disconnected
		private bool isRange;					// Storage for boolean that denotes whether the object is a range
		private string fromName;				// Storage for fromName for a Range
		private string toName;					// Storage for toName for a Range  

		private string cachedUnconnectedElementName;
		private string originalReferenceName;

		#endregion //Member Variables

		#region Constructor
		private UCReference(bool isConnected, bool isRange)
		{
			this.connected = isConnected;
			this.isRange = isRange;
			this.formulaStates = new UltraCalcFormulaStates(); 
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public UCReference()
			: this(false, false)
		{
		}

		/// <summary>
		/// Reference name constructor
		/// </summary>
		/// <param name="referenceName">Name of reference entered into formula used to create the underlying object reference</param>
		public UCReference(string referenceName)
			: this(false, false)
		{
			// spm: can't store reference name directly, need to parse it first to normalize the case insensitive parts.
			RefParser rp = new RefParser( referenceName );
			this.referenceName = rp.ToString(); 

			// MD 10/7/09 - TFS21185
			// Cache the original reference name.
			this.originalReferenceName = this.referenceName;
		}

		/// <summary>
		/// Initializes a new <see cref="UCReference"/> for a range.
		/// </summary>
		/// <param name="fromName">Start of the range</param>
		/// <param name="toName">End of the range</param>
		public UCReference( string fromName, string toName )
			: this( false, true )
		{
			// spm: can't store reference name directly, need to parse it first to normalize the case insensitive parts.
			RefParser rpFrom = new RefParser( fromName );
			this.fromName = rpFrom.ToString();

			RefParser rpTo = new RefParser( toName );
			this.toName = rpTo.ToString();

			// SSP 9/18/04
			// Changed the range separator from ".." to ":". Added the RangeSeparator and
			// RangeSeparatorWithSpaces constants. Before the range separator was hard-coded in
			// multiple places.
			//
			//this.referenceName = this.fromName + " .. " + this.toName;
			this.referenceName = this.fromName + RefParser.RangeSeparatorWithSpaces + this.toName;

			// MD 10/7/09 - TFS21185
			// Cache the original reference name.
			this.originalReferenceName = this.referenceName;
		} 

		// MD 7/25/08 - Excel formula solving





		internal UCReference( IUltraCalcReference reference )
			: this(true, false)
		{
			this.reference = reference;
		}

		#endregion //Constructor

		#region IUltraCalcReference
		/// <summary>
		/// Returns a context for the Reference.
		/// </summary>
		/// <remarks><p class="body">Returns a meaningful object context for the reference. This could be an UltraGridCell, UltraGridColumn, NamedReference, Control, etc.</p></remarks>
		public object Context { get{ return null;} }

		/// <summary>
		/// Create a new reference relative to this reference.
		/// </summary>
		/// <param name="reference">The reference string.</param>
		/// <returns>The new reference.</returns>
		public IUltraCalcReference CreateReference(string reference)
		{
			if (this.connected)
				return this.reference.CreateReference(reference);
			else
				return new UCReference(reference);
		}

		/// <summary>
		/// Create a Range reference relative to this reference.
		/// </summary>
		/// <param name="fromReference">The start of the range, inclusive.</param>
		/// <param name="toReference">The end of the range, inclusive.</param>
		/// <returns>A RangeReference</returns>
		public IUltraCalcReference CreateRange( string fromReference, string toReference )
		{
			if ( this.connected )
				return this.reference.CreateRange( fromReference, toReference );
			else
				return new UCReference( fromReference, toReference );
		} 

		/// <summary>
		/// Resolves a reference relative to this reference.  For example, if this
		/// reference is a reference to a row, and inReference is a reference to a
		/// column, the resolved reference is the cell reference that is the intersection
		/// of this row and the input column.
		/// </summary>
		/// <param name="reference">The reference to resolve.</param>
		/// <param name="referenceType"></param>
		/// <returns>The resolved reference.</returns>
		public IUltraCalcReference ResolveReference( IUltraCalcReference reference, ResolveReferenceType referenceType )
		{
			if ( this.connected )
				return this.reference.ResolveReference( reference, referenceType );
			else
				return reference;
		} 

		/// <summary>
		/// Returns the absolute name for the reference
		/// </summary>
		public string AbsoluteName
		{
			get
			{
				if (this.connected)
					return this.reference.AbsoluteName;
				else
					return referenceName;
			}
		}

		// SSP 9/7/04
		// Added NormalizedAbsoluteName property as a part of case insensitive absolute
		// name implementation.
		//
		#region NormalizedAbsoluteName

		// MD 8/27/08 - Code Analysis - Performance
		//private string lastAbsoluteName = null;
		//private string lastNormalizedAbsoluteName = null;
		private string lastAbsoluteName;
		private string lastNormalizedAbsoluteName;

		/// <summary>
		/// Returns the normalized absolute name. UltraCalcEngine makes use normalized
		/// absolute names of references to compare two references and search for references.
		/// This property should return the absolute name with case insensitive parts of
		/// the absolute names converted to lower case.
		/// </summary>
		public string NormalizedAbsoluteName
		{
			get
			{
				string absoluteName = this.AbsoluteName;
				if ( this.lastAbsoluteName == absoluteName )
					return this.lastNormalizedAbsoluteName;

				this.lastAbsoluteName = absoluteName;

				this.lastNormalizedAbsoluteName =
					!isRange ? new RefParser( absoluteName ).ToString()
					: new RefParser( this.fromName ).ToString( )
						+ RefParser.RangeSeparatorWithSpaces + new RefParser( this.toName ).ToString()
					;

				return this.lastNormalizedAbsoluteName;
			}
		}

		#endregion // NormalizedAbsoluteName

		/// <summary>
		/// The unqualified name of this referenced element.  Used to construct the fully qualified name by the
		/// <b>AbsoluteName</b> method. Read Only.
		/// </summary>
		/// <seealso cref="AbsoluteName"/>
		public string ElementName
		{
			get
			{
				if (this.connected)
					return this.reference.ElementName;
				else
				{
					// MD 10/7/09
					// Found while fixing TFS21185.
					// The reference name can be an absolute name. We should return the last tuple as the element name in this case.
					//return referenceName;
					if ( this.cachedUnconnectedElementName == null )
					{
						RefParser refParser = new RefParser( this.referenceName );
						this.cachedUnconnectedElementName = refParser.LastTuple.Name;
					}

					return this.cachedUnconnectedElementName;
				}
			}
		}
		/// <summary>
		/// The <b>UltraCalcValue</b>, if any, associated with this Reference.  If this reference 
		/// does not contain a Value then a <b>UltraCalcErrorValue</b> is returned.
		/// </summary>
		public UltraCalcValue Value
		{
			get
			{
				if (this.connected)
					return this.reference.Value;
				else
					return new UltraCalcValue(new UltraCalcErrorValue(UltraCalcErrorCode.Reference));
			}
			set
			{ 
				if (this.connected)
					this.reference.Value = value;
			}
		}

		/// <summary>
		/// The <b>IUltraCalcFormula</b>, if any, associated with this Reference.  If this reference 
		/// can not contain a formula then null is returned.
		/// </summary>
		public IUltraCalcFormula Formula
		{
			get	
			{
				if (this.connected) 
					return this.reference.Formula;
				else
					return null; 	
			}
		}

		/// <summary>
		/// Indicates if the underlying reference has a relative index.
		/// </summary>
		public bool HasRelativeIndex
		{
			get
			{
				if ( this.connected )
					return this.reference.HasRelativeIndex;
				else
					return false;
			}
		}

		/// <summary>
		/// Indicates if the underlying reference has an absolute index.
		/// </summary>
		public bool HasAbsoluteIndex
		{
			get
			{
				if ( this.connected )
					return this.reference.HasAbsoluteIndex;
				else
					return false;
			}
		}

		/// <summary>
		/// Indicates if the underlying reference was constructed with a scope all index.
		/// </summary>
		public bool HasScopeAll
		{
			get
			{
				if ( this.connected )
					return this.reference.HasScopeAll;
				else
					return false;
			}
		}

		/// <summary>
		/// For each reference tuple in this reference that has a relative index, mark the
		/// corresponding tuple in inReference.
		/// </summary>
		/// <param name="reference">The Reference to be marked.</param>
		public void MarkRelativeIndices( IUltraCalcReference reference )
		{
			if ( this.connected )
				this.reference.MarkRelativeIndices( reference );
		} 

		/// <summary>
		/// Flag used by the <b>UltraCalcEngine</b> to manage the recalculation network. 
		/// Denotes the reference is visible.
		/// </summary>
		public bool RecalcDeferred
		{
			get 
			{ 
				if (this.connected)
					return this.reference.RecalcDeferred;
				else
					return formulaStates.RecalcDeferred; 
			}
			set	
			{
				if (this.connected)
					this.reference.RecalcDeferred = value;
				else
					formulaStates.RecalcDeferred = value; 
			}
		}

		/// <summary>
		/// Flag used by the <b>UltraCalcEngine</b> to manage the recalculation network. 
		/// Denotes the reference is visible.
		/// </summary>
		public bool RecalcVisible
		{
			get 
			{ 
				if (this.connected)
					return this.reference.RecalcVisible;
				else
					return formulaStates.RecalcVisible; 
			}
			set	
			{
				if (this.connected)
					this.reference.RecalcVisible = value;
				else
					formulaStates.RecalcVisible = value; 
			}
		}

		/// <summary>
		/// Return a collection of references if this Reference is enumerable.  An exception is
		/// thrown if this reference is not enumerable.
		/// </summary>
		/// <returns>A Reference collection.</returns>
		public IUltraCalcReferenceCollection References
		{
			get
			{
				if (this.connected)
					return this.reference.References;
				else
					return null;
			}
		}

		/// <summary>
		/// Returns true if this reference is enumerable.
		/// </summary>
		public bool IsEnumerable
		{
			get
			{
				if (this.connected)
					return this.reference.IsEnumerable;
				else
					return false;
			}
		}

		/// <summary>
		/// Returns true if this reference is a data reference.  A data reference contains a value, such as a Grid Cell or
		/// a Grid SummaryValue.  Many referenced elements, such as a Grid Column or a Grid Band, do not reference a value.
		/// Read Only. 
		/// </summary>
		public bool IsDataReference
		{
			get
			{
				if (this.connected)
					return this.reference.IsDataReference;
				else
					return false;
			}
		}

		/// <summary>
		/// The RefBase Parent reference of this referenced element.  Used to navigate the element hierarchy.  If this
		/// referenced element is the root element, <b>BaseParent</b> returns null.  Read Only.
		/// </summary>
		public IUltraCalcReference Parent
		{
			get
			{
				if ( this.connected )
					return this.reference.Parent;
				else
					return null;
			}
		} 

		/// <summary>
		/// Returns true if this reference contains inReference
		/// </summary>
		/// <param name="reference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public bool ContainsReference(IUltraCalcReference reference)
		{
			if (this.connected)
				return this.reference.ContainsReference(reference);
			else
				// SSP 9/7/04
				// Perform a case insensitive comparison.
				//
				//return String.Compare(referenceName, reference.AbsoluteName) == 0;
				return this.NormalizedAbsoluteName == reference.NormalizedAbsoluteName;
		}

		/// <summary>
		/// Returns true if inReference is a proper subset of this reference
		/// </summary>
		/// <param name="reference">The subset candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public bool IsSubsetReference(IUltraCalcReference reference)
		{
			if (this.connected)
				return this.reference.IsSubsetReference(reference);
			else
				return String.Compare(referenceName, reference.AbsoluteName) == 0;
		}

		/// <summary>
		/// Determines whether the given reference is a sibling of this reference
		/// </summary>
		/// <param name="reference">The reference to compare against this one</param>
		/// <returns>True if the reference is a sibling of the given reference</returns>
		public bool IsSiblingReference( IUltraCalcReference reference )
		{
			if ( this.connected )
				return this.reference.IsSiblingReference( reference );
			else
				return false;
		} 

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public bool IsDisposedReference
		{
			get
			{
				if (this.connected)
					return this.reference.IsDisposedReference;
				else
					return false;
			}
		}

		#region GetChildReferences

		// SSP 9/7/04
		// Added GetChildReferences method to IUltraCalcReference interface and 
		// ReferenceType enum which the added method takes as a parameter.
		//
		IUltraCalcReference[] IUltraCalcReference.GetChildReferences( ChildReferenceType referenceType )
		{
			return this.connected && null != this.reference
				? this.reference.GetChildReferences( referenceType )
				: null;
		}

		#endregion //GetChildReferences 

		#endregion // IUltraCalcReference

		#region Properties

		#region Reference

		/// <summary>
		/// Returns the underlying reference or this instance if the object is not connected.
		/// </summary>
		public virtual IUltraCalcReference Reference
		{
			get	
			{
				if (this.connected) 
					return this.reference;
				else
					return this; 	
			}
		}

		#endregion // Reference

		#region RootName

		/// <summary>
		/// Returns the root name for the reference
		/// </summary>
		public virtual string RootName
		{
			get 
			{ 
				if (AbsoluteName.StartsWith("//")) 
				{
					int len = AbsoluteName.IndexOf('/',2);
					string name = AbsoluteName.Substring(2,len == -1 ? AbsoluteName.Length-2 : len-2);
					return name;
				} 
				else 
				{
					// Illegal reference format
					//throw new UltraCalcErrorException(new UltraCalcErrorValue(UltraCalcErrorCode.Reference, AbsoluteName, this));
					//int len = AbsoluteName.IndexOf('/');
					//return AbsoluteName.Substring(0,len == -1 ? AbsoluteName.Length : len);
					return null;
				}
			}
		}

		#endregion //RootName

		#region IsConnected

		/// <summary>
		/// Indicates if the UCReference is currently connected to an underlying <see cref="IUltraCalcReference"/>
		/// </summary>
		public bool IsConnected
		{
			get	{ return this.connected; }
		}

		#endregion //IsConnected

		#endregion //Properties

		#region Methods

		#region Connect

		/// <summary>
		/// Associates the <see cref="UCReference"/> with the specified <see cref="IUltraCalcReference"/>
		/// </summary>
		/// <param name="reference">New underlying reference for the instance</param>
		/// <returns>Returns true if the operation caused the UCReference to change so that it represents a different reference than before.</returns>
		public bool Connect(IUltraCalcReference reference)
		{
			//Debug.Assert(!connected);
			if (!this.connected)
			{

				// MD 10/7/09 - TFS21185
				// Restore the old reference name which was temporarily changed while we were disconnected.
				this.referenceName = this.originalReferenceName;
				this.cachedUnconnectedElementName = null;

				if ( this.isRange )
				{
					this.reference = reference.CreateRange( this.fromName, this.toName );
				}
				else 
				{
					this.reference = reference.CreateReference(referenceName);

					// MD 10/7/09 - TFS21185
					// If a non-error reference is connected, store the absolute name as the reference name so that when we are later disconnected,
					// the absolute name of this reference will still return the correct absolute name.
                    // MRS 1/10/2011 - TFS62117
                    // Check for null
					//if ( ( this.reference is UltraCalcReferenceError ) == false )
                    if ( this.reference != null &&
                        (this.reference is UltraCalcReferenceError) == false)
					{
						this.referenceName = this.reference.AbsoluteName;
						this.cachedUnconnectedElementName = null;
					}
				}

				if  (this.reference != null && !(this.reference is UltraCalcReferenceError))
				{
					//referenceName = null;
					this.formulaStates.Reset(); 
					this.connected = true;
					return true;
				}
				// SSP 10/12/04
				// Following code is not doing anything to the UCReference. It ends up being no op 
				// since the lines that assign referenceName are commented out.
				//
				
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

			}

			return false;
		}

		#endregion //Connect

		#region Disconnect

		/// <summary>
		/// Disconnects the <see cref="UCReference"/> from the underlying <see cref="IUltraCalcReference"/>
		/// </summary>
		/// <returns>Returns true if the operation caused the UCReference to change so that it represents a different reference than before.</returns>
		public bool Disconnect( )
		{
			//Debug.Assert(connected);
			if (connected)
			{
				//referenceName = this.reference.AbsoluteName;
				formulaStates = new UltraCalcFormulaStates(); 
				reference = null;
				connected = false;
				return true;
			}

			return false;
		}

		#endregion //Disconnect

		#region Reconnect

		/// <summary>
		/// Disconnects from the previous reference and connects to the specified <see cref="IUltraCalcReference"/>
		/// </summary>
		/// <param name="reference">New reference that should be the underlying reference for this instance</param>
		/// <returns>Returns true if the operation caused the UCReference to change so that it represents a different reference than before.</returns>
		public bool Reconnect(IUltraCalcReference reference)
		{
			bool origConnected = this.connected;
			IUltraCalcReference origReference = this.reference;

			Disconnect();
			Connect(reference);

			return origConnected != this.connected || origReference != this.reference 
				&& ( null == origReference || null == this.reference || ! origReference.Equals( this.reference ) );
		}

		#endregion //Reconnect

		// SSP 10/12/04
		// Overrode GetHashCode and Equals methods.
		//
		#region GetHashCode

		/// <summary>
		/// Overridden. Default implementation returns the hash code of the normalized absolute name.
		/// These objects are going to be added to data structure that make use of GetHashCode and 
		/// Equals methods. What this means is that the normalized absolute name must not change 
		/// throughout the lifetime of this instance. If it does change then the overriding class 
		/// must override this method and return a hash code based on some other immutable property 
		/// of the instance.
		/// </summary>
        /// <returns>A hash code of the normalized absolute name.</returns>
		public override int GetHashCode( )
		{
			Debug.Assert( false, "UCReference should only be used internally by the UltraCalcEngine." ); 
			return this.NormalizedAbsoluteName.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		/// <summary>
		/// Overriden. Default implementation returns true if the normalized absolute names of this
		/// reference and the passed in reference match. These objects are going to be added to data 
		/// structure that make use of GetHashCode and Equals methods. What this means is that the 
		/// normalized absolute name must not change throughout the lifetime of this instance. If it 
		/// does change then the overriding class must override this method and return a hash code 
		/// based on some other immutable property of the instance.
		/// </summary>
        /// <param name="obj">The object to test for equality.</param>
        /// <returns>True if the normalized absolute names of this instance and the passed in reference match.</returns>
		public override bool Equals( object obj )
		{
			if ( ! ( obj is IUltraCalcReference ) )
				return false;

			UCReference uc = obj as UCReference;
			if ( null != uc && uc.connected && this.connected )
			{
				IUltraCalcReference r1 = this.reference;
				IUltraCalcReference r2 = uc.reference;
				return r1 == r2 || null != r1 && null != r2 && r1.Equals( r2 );
			}

			// If obj was not an UCReference then compare this UCReference' underlying reference to
			// the passed in reference.
			//
			// SSP 7/7/05
			// Just something I noticed. this.Reference can return itself if the reference is disconnected.
			// So to prevent infinite recursion check for this.connected.
			// 
			//if ( null == uc && null != this.Reference )
			if ( null == uc && this.connected && null != this.Reference )
				return this.Reference.Equals( obj );
						
			// Otherwise perform normalized absolute name comparision.
			//
			return this.NormalizedAbsoluteName == ((IUltraCalcReference)obj).NormalizedAbsoluteName;
		}

		#endregion // Equals

		#endregion //Methods
	}

	#endregion // UCReference

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