using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Linq;
using Infragistics.Windows.DataPresenter.Calculations;


using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using Infragistics.Calculations;
using Infragistics.Windows.DataPresenter;
using Infragistics.Calculations.Engine;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter.Calculations
{
	#region DataPresenterRefBase Class

	/// <summary>
	/// Base class for all the UltraWinGrid reference implementations.
	/// </summary>
	internal abstract class DataPresenterRefBase : RefBase
	{
		#region Member variables

		internal readonly DataPresenterReference _rootReference;
		private object _context;
		private int verifiedParserInstanceVersion = 0;

		// SSP 1/20/05 BR01804
		// If a formula column is sorted or grouped by we need to make sure it's fully 
		// recalculated before we can sort the row collections based on its values.
		// Added disableRecalcDeferredOverride flag to disable recalc deferred on sort
		// columns.
		//
		internal bool _disableRecalcDeferredOverride;

		private string _cachedElementName;
		private string _cachedNormalizedElemenetName;

		#endregion // Member variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="rootReference">Data presenter root reference.</param>
		/// <param name="elementName">Element name.</param>
		/// <param name="context">Reference context.</param>
		internal DataPresenterRefBase( DataPresenterReference rootReference, string elementName, object context )
			: base( )
		{
			if ( null == rootReference && this is DataPresenterReference )
				rootReference = (DataPresenterReference)this;

			CoreUtilities.ValidateNotNull( rootReference );

			_rootReference = rootReference;
			_cachedElementName = elementName;
			_cachedNormalizedElemenetName = null != elementName ? Utils.NormalizeElementName( elementName ) : null;
			_context = context;
		}

		#endregion // Constructor

		#region ElementName

		public override string ElementName
		{
			get
			{
				return _cachedElementName;
			}
		}

		#endregion // ElementName

		#region NormalizedElementName

		internal string NormalizedElementName
		{
			get
			{
				return _cachedNormalizedElemenetName;
			}
		}

		#endregion // NormalizedElementName

		#region ParserInstanceVersion

		/// <summary>
		/// Parsed references on rows, cells, rows collections and nested column references
		/// have to be recreated whenever their absolute names change due to a row getting
		/// inserted, deleted, the rows collection getting sorted or resynced. For example,
		/// When the first row is deleted from a rows collection then the next row's absolute
		/// name changes from //ultraGrid1/Customers(1) to //ultraGrid1/Customers(0). However
		/// the parsed reference as returned by RefBase.ParsedReference property is still 
		/// based on the old absolute name. So we need to recreate the parsed reference.
		/// </summary>
		internal virtual int ParserInstanceVersion
		{
			get
			{
				return 0;
			}
		}

		#endregion // ParserInstanceVersion

		#region ParsedReference

		/// <summary>
		/// Gets or Sets the parsed representation of this reference.  This returns the absolute reference name.  Use
		/// <b>RelativeReference</b> to get the parsed representation of the string used to create this reference if this 
		/// reference is UnAnchored.
		/// </summary>
		public override RefParser ParsedReference
		{
			get
			{
				// If the row indexes have so that the absolute name of the reference would be
				// different than before then recreate the parser instance because the old
				// parser instance was created using the old absolute name.
				//
				if ( this.verifiedParserInstanceVersion != this.ParserInstanceVersion )
				{
					this.ParsedReference = null;
					this.verifiedParserInstanceVersion = this.ParserInstanceVersion;
				}

				return base.ParsedReference;
			}
			set
			{
				base.ParsedReference = value;
			}
		}

		#endregion // ParsedReference

		#region FieldLayoutContext

		/// <summary>
		/// Returns the context of a field layout if any. Cell, record, column, field layout references etc... all
		/// have the context of a field layout.
		/// </summary>
		internal virtual FieldLayout FieldLayoutContext
		{
			get
			{
				if ( _context is FieldLayout )
					return (FieldLayout)_context;
				else if ( null != this.FieldContext )
					return this.FieldContext.Owner;

				DataPresenterRefBase parent = this.BaseParent as DataPresenterRefBase;
				return null != parent ? parent.FieldLayoutContext : null;
			}
		}

		#endregion // FieldLayoutContext

		#region FieldContext

		/// <summary>
		/// Returns the context of a field if any. Cell and field references etc... all
		/// have the context of a field.
		/// </summary>
		internal virtual Field FieldContext
		{
			get
			{
				if ( _context is Field )
					return (Field)_context;

				DataPresenterRefBase parent = this.BaseParent as DataPresenterRefBase;
				return null != parent ? parent.FieldContext : null;
			}
		}

		#endregion // FieldContext

		#region CalcManager

		/// <summary>
		/// Gets the calc manager.
		/// </summary>
		public virtual XamCalculationManager CalcManager
		{
			get
			{
				return _rootReference.CalcManager;
			}
		}

		#endregion // CalcManager

		#region NotifyCalcEngineValueChanged

		/// <summary>
		/// Notifies the calc manager of the change in this reference' value.
		/// </summary>
		internal void NotifyCalcEngineValueChanged( )
		{
			ICalculationManager cm = this.CalcManager;
			if ( null != cm )
				cm.NotifyValueChanged( this );
		}

		#endregion // NotifyCalcEngineValueChanged

		#region RefBase Overrides

		#region FindRoot

		protected override RefBase FindRoot( )
		{
			return _rootReference;
		}

		#endregion // FindRoot

		// SSP 9/16/04
		// Overrode Find methods so they don't have to be overridden on deriving classes where these
		// methods will never be called.
		//

		#region FindItem

		public override ICalculationReference FindItem( string name )
		{
			return new CalculationReferenceError( name, new NotSupportedException( ) );
		}

		#endregion // FindItem

		#region FindAll

		public override ICalculationReference FindAll( string name )
		{
			return new CalculationReferenceError( name, new NotSupportedException( ) );
		}

		#endregion // FindAll

		#region FindItem

		public override ICalculationReference FindItem( string name, string index )
		{
			return new CalculationReferenceError( name, new NotSupportedException( ) );
		}

		#endregion // FindItem

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return new CalculationReferenceError( name, new NotSupportedException( ) );
		}

		#endregion // FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			return new CalculationReferenceError( name, new NotSupportedException( ) );
		}

		#endregion // FindSummaryItem

		#region Context

		/// <summary>
		/// Returns the object associated with this references. Returns field for the field reference and cell for the cell reference etc...
		/// </summary>
		public override object Context
		{
			get
			{
				return _context;
			}
		}

		#endregion // Context

		#region ContainsReferenceHelper

		protected virtual bool ContainsReferenceHelper( ICalculationReference inReference, bool isProperSubset )
		{
			RefBase inRef = inReference as RefBase;
			DataPresenterRefBase dpRef = RefUtils.GetUnderlyingReference( inRef ) as DataPresenterRefBase;
			bool isDPRef = null != dpRef;

			// SSP 12/11/06 BR18268
			// Moved this optimization in RowsCollectionReference.ContainsReferenceHelper override.
			// 
			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			if ( null != inRef && isDPRef )
			{
				// If this is data presenter reference then simply check if the inReference is from this data presenter.
				// 
				if ( this == _rootReference && _rootReference == dpRef._rootReference )
					return true;

				// SSP 9/17/04 - Optmizations
				//
				if ( inRef is CellReference && this is FieldReference )
					return ( (CellReference)inRef ).FieldContext == ( (FieldReference)this ).Field;

				// SSP 9/12/04
				// Multiple LHS for a single summary related.
				//
				if ( this is SummaryDefinitionReference && inRef is SummaryDefinitionReference )
					return string.Equals( this.NormalizedAbsoluteName, inRef.NormalizedAbsoluteName );

				RefParser thisRP = this.ParsedReference;
				RefParser testRP = inRef.ParsedReference;

				if ( !thisRP.IsFullyQualified && !thisRP.IsRoot
					|| !testRP.IsFullyQualified && !testRP.IsRoot
					|| thisRP.TupleCount <= 0 || testRP.TupleCount <= 0 )
					return false;

				bool rootTuplesMatch = isProperSubset
						? thisRP[0].IsSubset( testRP[0] )
						: thisRP[0].Contains( testRP[0] );

				if ( !rootTuplesMatch )
					return false;

				// MD 8/10/07 - 7.3 Performance
				// Use generics
				//ArrayList list1 = RefUtils.InsertGroupByColumnTuples( this.Layout, thisRP );
				//ArrayList list2 = RefUtils.InsertGroupByColumnTuples( this.Layout, testRP );
				List<RefTuple> list1 = _rootReference._utils.InsertGroupByFieldTuples( thisRP );
				List<RefTuple> list2 = _rootReference._utils.InsertGroupByFieldTuples( testRP );

				if ( null != list1 && null != list2 && list1.Count <= list2.Count )
				{
					bool allTuplesMatch = true;

					for ( int i = 0; allTuplesMatch && i < list1.Count; i++ )
						allTuplesMatch = isProperSubset
							// MD 8/10/07 - 7.3 Performance
							// Use generics
							//? ((RefTuple)list1[i]).IsSubset( (RefTuple)list2[i] )
							//: ((RefTuple)list1[i]).Contains( (RefTuple)list2[i] );
							? list1[i].IsSubset( list2[i] )
							: list1[i].Contains( list2[i] );

					return allTuplesMatch;
				}
			}

			return base.ContainsReference( inReference );
		}

		#endregion // ContainsReferenceHelper

		// SSP 9/3/04
		// Overrode ContainsReference so we can take into account the group-by tuples.
		//
		#region ContainsReference

		/// <summary>
		/// Returns true if this reference contains inReference
		/// </summary>
		/// <param name="inReference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public override bool ContainsReference( ICalculationReference inReference )
		{
			return this.ContainsReferenceHelper( inReference, false );
		}

		#endregion // ContainsReference

		// SSP 9/8/04
		// Overrode IsSubsetReference so we can take into account the group-by tuples.
		//
		#region IsSubsetReference

		/// <summary>
		/// Returns true if inReference is a proper subset of this reference
		/// </summary>
		/// <param name="inReference">The subset candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public override bool IsSubsetReference( ICalculationReference inReference )
		{
			return this.ContainsReferenceHelper( inReference, true );
		}

		#endregion // IsSubsetReference

		#endregion // RefBase Overrides

		#region FilterReference

		/// <summary>
		/// UltraCalcEngine has a requirement that two separate instances of ICalculationReference can't have 
		/// the same absolute name and not share the recalc flags or that two instances of 
		/// ICalculationReference have different absolute names but share the recalc flags. What this 
		/// means that since a NestedFieldReference with the context of the root rows collection has the 
		/// same absolute name as the associated FieldReference, we have to make sure to return the 
		/// ColumnRefernece instead of the NestedFieldReference in such cases where the nested column 
		/// reference absolute name happens to be the same as the associated column reference' absolute 
		/// name. This would occur like I said before when the NestedFieldReference has the root rows 
		/// collection context. We may have to also modify the NestedReference classes to delgate the recalc 
		/// flags to the associated non-nested references however as long as this fix works out we won't 
		/// have to do it. The reason why this is preferrable is that tokens in the formula should really be 
		/// getting the column and summarysettings references and not the nested ones when they are 
		/// reconnected.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		internal static ICalculationReference FilterReference( ICalculationReference result )
		{
			NestedReference reference = RefUtils.GetUnderlyingReference( result ) as NestedReference;
			RefBase newReference = null;

			if ( null != reference && reference.Scope.IsRootLevel )
			{
				if ( reference is NestedFieldReference )
					newReference = ( (NestedFieldReference)reference )._fieldRef;
				else if ( reference is NestedFieldLayoutReference )
					newReference = ( (NestedFieldLayoutReference)reference )._fieldLayoutRef;
				else if ( reference is NestedSummaryDefinitionReference )
					newReference = ( (NestedSummaryDefinitionReference)reference ).SummaryReference;
			}
			else
			{
				SummaryResultReference summaryResultRef = RefUtils.GetUnderlyingReference( result ) as SummaryResultReference;
				if ( null != summaryResultRef && summaryResultRef.SummaryResult.ParentCollection.Records.IsRootLevel )
					newReference = summaryResultRef._summaryDefinitionReference;
			}

			if ( null != newReference && newReference != reference )
			{
				if ( result is RefUnAnchored )
				{
					RefUnAnchored ru = new RefUnAnchored( newReference );
					ru.RelativeReference = ( (RefUnAnchored)result ).RelativeReference;
					newReference = ru;
				}

				return newReference;
			}

			return result;
		}

		#endregion // FilterReference

		#region CreateReference

		/// <summary>
		/// Gets the reference for the specified reference name. The reference name can be absolute or relative.
		/// </summary>
		/// <param name="referenceName"></param>
		/// <returns></returns>
		public override ICalculationReference CreateReference( string referenceName )
		{
			// SSP 9/13/04
			// Read the comment above FilterReference for more info.
			//
			// MD 7/26/07 - 7.3 Performance
			// FxCop - Mark members as static
			//return this.FilterReference( base.CreateReference( referenceName ) );
			return FilterReference( base.CreateReference( referenceName ) );
		}

		#endregion // CreateReference

		#region ResolveReference

		// SSP 9/11/04
		// Overrode ResolveReference.
		//
		public override ICalculationReference ResolveReference( ICalculationReference inReference, ResolveReferenceType referenceType )
		{
			// If the reference being resolved is a SummaryDefinitionReference or GroupLevelSummaryDefinitionReference
			// or a NestedSummaryDefinitionReference (in other words a summary settings), then just return
			// the passed in reference in the case this is a column or a band. Also if this is a rows collection
			// then return the nested summary reference for the passed in summary reference.
			//
			if ( inReference is SummaryDefinitionReference || inReference is NestedSummaryDefinitionReference )
			{
				// SSP 9/18/04
				//
				//if ( this is FieldReference || this is FieldLayoutReference )
				if ( this is FieldReference || this is FieldLayoutReference || this is SummaryDefinitionReference )
					return inReference;

				// If we can get more specific.
				//
				if ( this is NestedReference && inReference is SummaryDefinitionReference )
				{
					RecordCollectionBase scope = ( (NestedReference)this ).Scope;

					// MRS 10/10/2008 - TFS8793
					// Don't do this if the reference are not from the same grid. 
					//                    
					//if ( ! rows.Band.IsDescendantOfBand( ((SummaryDefinitionReference)inReference).Summary.Band ) )
					//  return RefUtils.GetNestedSummaryDefinitionReference(rows, (SummaryDefinitionReference)inReference);
					FieldLayout thisFieldLayout = scope.FieldLayout;
					SummaryDefinitionReference summaryDefinitionReference = (SummaryDefinitionReference)inReference;
					FieldLayout summaryFieldLayout = summaryDefinitionReference.Summary.FieldLayout;
					if ( _rootReference == summaryDefinitionReference._rootReference &&
						!RefUtils.IsDescendantOf( thisFieldLayout, summaryFieldLayout ) )
					{
						return _rootReference._utils.GetNestedSummaryDefinitionReference( scope, summaryDefinitionReference );
					}

					return inReference;
				}
			}

			// SSP 9/13/04
			// Read the comment above FilterReference for more info.
			//
			//return base.ResolveReference( inReference );
			// MD 7/26/07 - 7.3 Performance
			// FxCop - Mark members as static
			//return this.FilterReference( base.ResolveReference( inReference, referenceType ) );
			return FilterReference( base.ResolveReference( inReference, referenceType ) );
		}

		#endregion // ResolveReference

		#region CreateRange

		// SSP 9/16/04
		// Overrode CreateRange because the base class implementation (off RefBase) uses RefRange
		// class which doesn't know anything about group-by tupes so it doesn't work when you
		// have group-by on. Ranges like [Col(0)]..[Col] fail because from-reference is a cell 
		// reference and includes group-by tuples whereas to-reference is a column reference and 
		// doesn't include group-by tuples. As far as the RefRange is concerned, all the tuples
		// upto the last tuple have to match. To fix this created RangeReference class.
		//

		private RefTuple GetLastRangeRefTuple( RefParser rp, out string errorMessage )
		{
			errorMessage = null;
			RefTuple lastTuple = new RefTuple( rp.LastTuple );

			RefTuple nextToLast = rp[rp.TupleCount - 2];

			if ( RefTuple.RefScope.Any == lastTuple.Scope )
			{
				lastTuple.Scope = nextToLast.Scope;
				lastTuple.ScopeIndex = nextToLast.ScopeIndex;
			}

			// Ranges only work with no scope like in [Col] in [Col(0)]..[Col]
			// or All scope like in [Col(0)]..[Col(*)] which means from 0th row to the last row
			// or a relative index or a specific index like in [Col(0)]..[Col(-1)] respectively.
			// 
			if ( RefTuple.RefScope.Any == lastTuple.Scope
				|| RefTuple.RefScope.All == lastTuple.Scope
				|| RefTuple.RefScope.Index == lastTuple.Scope
				// Relative indexes on range references only make sense in column formulas since
				// there is a cell context. On a summary saying [Col(-1)] doesn't make sense 
				// since summary doesn't have a context of a row (except the parent row which
				// would be either a group-by row or from a different band.
				//
				|| RefTuple.RefScope.RelativeIndex == lastTuple.Scope && this is FieldReference )
				return lastTuple;

			errorMessage = "Invalid scope.";

			// If there is an invalid tuple, then return null to indicate invalid range
			// reference specification.
			//
			return null;
		}

		/// <summary>
		/// Create a range reference relative to this reference.
		/// </summary>
		/// <param name="fromReferenceName">The start of the range, inclusive.</param>
		/// <param name="toReferenceName">The end of the range, inclusive.</param>
		/// <returns></returns>
		public override ICalculationReference CreateRange( string fromReferenceName, string toReferenceName )
		{
			ICalculationReference createdFromRef = this.CreateReference( fromReferenceName );

			if ( createdFromRef is CalculationReferenceError )
				return createdFromRef;

			ICalculationReference createdToRef = this.CreateReference( toReferenceName );

			if ( createdToRef is CalculationReferenceError )
				return createdToRef;

			DataPresenterRefBase fromRef = RefUtils.GetUnderlyingReference( createdFromRef ) as DataPresenterRefBase;
			RefParser fromRefRP = createdFromRef is RefUnAnchored
				? ( (RefUnAnchored)createdFromRef ).ParsedReference : fromRef.ParsedReference;

			DataPresenterRefBase toRef = RefUtils.GetUnderlyingReference( createdToRef ) as DataPresenterRefBase;
			RefParser toRefRP = createdToRef is RefUnAnchored
				? ( (RefUnAnchored)createdToRef ).ParsedReference : toRef.ParsedReference;

			// If the reference were not grid references then call the base implementation 
			// since we wouldn't know how to handle reference from something other than a grid.
			//
			if ( null == fromRef || null == toRef || null == fromRefRP || null == toRefRP
				|| fromRefRP.TupleCount < 2 || toRefRP.TupleCount < 2 )
				return base.CreateRange( fromReferenceName, toReferenceName );

			string errorMessage = null;

			if ( null == fromRef.FieldContext || null == toRef.FieldContext )
				errorMessage = "From or to reference does not specify a column.";
			else if ( fromRef.FieldContext != toRef.FieldContext )
				errorMessage = "From and to references in the range reference are not the same columns.";

			RefTuple fromLastTuple = null;
			RefTuple toLastTuple = null;

			if ( null == errorMessage )
				fromLastTuple = this.GetLastRangeRefTuple( fromRefRP, out errorMessage );

			if ( null == errorMessage )
				toLastTuple = this.GetLastRangeRefTuple( toRefRP, out errorMessage );

			string rangeReferenceName = fromReferenceName + RefUtils.RANGE_REFERENCE_SEPARATOR + toReferenceName;
			if ( null != errorMessage || null == fromLastTuple || null == toLastTuple )
				return new CalculationReferenceError( rangeReferenceName, errorMessage );

			ICalculationReference fieldRef = _rootReference._utils.GetFieldReference( fromRef.FieldContext, true );
			if ( !( fieldRef is FieldReference ) )
				return fieldRef;

			return new RangeReference( (FieldReference)fieldRef, fromRefRP, toRefRP, fromLastTuple, toLastTuple );
		}

		#endregion // CreateRange

		// SSP 9/30/04
		// Commented out the original code and added the new code below.
		//
		#region IsSiblingReference

		/// <summary>
		/// Determines whether the given reference is a sibling of this reference
		/// </summary>
		/// <param name="reference">The reference to compare against this one</param>
		/// <returns>True if the reference is a sibling of the given reference</returns>
		public override bool IsSiblingReference( ICalculationReference reference )
		{
			DataPresenterRefBase ref1 = RefUtils.GetUnderlyingReference( reference ) as DataPresenterRefBase;
			DataPresenterRefBase ref2 = this;

			if ( ref1 is NestedFieldReference && ( (NestedFieldReference)ref1 ).Scope.IsRootLevel )
				ref1 = ( (NestedFieldReference)ref1 )._fieldRef;

			if ( ref2 is NestedFieldReference && ( (NestedFieldReference)ref2 ).Scope.IsRootLevel )
				ref2 = ( (NestedFieldReference)ref2 )._fieldRef;

			if ( ref1 is FieldReference && ref2 is FieldReference )
				return ( (FieldReference)ref1 ).FieldLayoutContext == ( (FieldReference)ref2 ).FieldLayoutContext;

			return false;
		}

		#endregion // IsSiblingReference

		#region RecalcDeferred

		// SSP 1/17/05 BR01753
		// Print and export layout won't calculate formulas but rather they will copy over 
		// the calculated values from the display layout. We need to turn off the deferred
		// calculations while printing so that all of the cells, visible or otherwise get
		// calculated.
		//
		public override bool RecalcDeferred
		{
			get
			{
				// Turn off recalc deferred while printing so.
				//
				return base.RecalcDeferred && !_rootReference._recalcDeferredSuspended
					// SSP 1/20/05 BR01804
					// If a formula column is sorted or grouped by we need to make sure it's fully 
					// recalculated before we can sort the row collections based on its values.
					// Added disableRecalcDeferredOverride flag to disable recalc deferred on sort
					// columns.
					//
					&& !_disableRecalcDeferredOverride;
			}
			set
			{
				// SSP 1/20/05 BR01804
				// If a formula column is sorted or grouped by we need to make sure it's fully 
				// recalculated before we can sort the row collections based on its values.
				// Added disableRecalcDeferredOverride flag to disable recalc deferred on sort
				// columns.
				//
				base.RecalcDeferred = value && !_disableRecalcDeferredOverride;
			}
		}

		internal bool RecalcDeferredBase
		{
			get
			{
				return base.RecalcDeferred;
			}
		}

		#endregion // RecalcDeferred
	}

	#endregion // DataPresenterRefBase Class

	#region DataPresenterReference Class

	/// <summary>
	/// Reference implementation that represents a grid control.
	/// </summary>
	internal class DataPresenterReference : DataPresenterRefBase
	{
		#region Private Variables

		private readonly XamCalculationManager _calcManager;
		private readonly DataPresenterBase _dataPresenter;
		internal readonly RefUtils _utils;
		internal readonly DataPresenterCalculationAdapter _adapter;
		private bool _isValid = true;
		private RecordCollectionBase _rootRecords;

		// SSP 1/18/05 BR01753
		//
		internal bool _recalcDeferredSuspended;

		private List<FieldLayout> _validFieldLayouts = new List<FieldLayout>( );
		private List<FieldLayout> _hierarchicalFieldLayouts = new List<FieldLayout>( );
		private List<FieldLayout> _nonHierarchicalFieldLayouts = new List<FieldLayout>( );

		private Dictionary<FieldLayout, FieldLayoutReference> _fieldLayoutReferences = new Dictionary<FieldLayout, FieldLayoutReference>( );

		internal readonly WeakDictionary<object, string> _autoGeneratedKeys = new WeakDictionary<object, string>( true, false );

		#endregion Private Variables

		#region GridReference

		internal DataPresenterReference( DataPresenterCalculationAdapter adapter, string elementName )
			: base( null, elementName, adapter.DataPresenter )
		{
			_adapter = adapter;
			_dataPresenter = adapter.DataPresenter;
			_calcManager = adapter.CalcManager;
			_rootRecords = _dataPresenter.Records;
			_utils = new RefUtils( this );

			this.Initialize( );
		}

		#endregion // GridReference

		#region GetAutoGeneratedName

		internal string GetAutoGeneratedName( object item, bool autoGenerateIfNotAlready )
		{
			string name;
			if ( !_autoGeneratedKeys.TryGetValue( item, out name ) && autoGenerateIfNotAlready )
				_autoGeneratedKeys[item] = name = Guid.NewGuid( ).ToString( );

			return name;
		}

		#endregion // GetAutoGeneratedName

		private void Initialize( )
		{
			this.IdentifyFieldLayouts( );

			this.CreateFieldLayoutReferences( );
		}

		private void CreateFieldLayoutReferences( )
		{
			_fieldLayoutReferences.Clear( );

			foreach ( var ii in _validFieldLayouts )
				this.CreateFieldLayoutReferencesHelper( ii, _fieldLayoutReferences );
		}

		private void CreateFieldLayoutReferencesHelper( FieldLayout fieldLayout, Dictionary<FieldLayout, FieldLayoutReference> references )
		{
			if ( !references.ContainsKey( fieldLayout ) )
			{
				FieldLayoutReference parentFieldLayoutReference = null;

				var parentFieldLayout = fieldLayout.ParentFieldLayout;
				if ( null != parentFieldLayout )
				{
					this.CreateFieldLayoutReferencesHelper( parentFieldLayout, references );
					parentFieldLayoutReference = references[parentFieldLayout];
				}

				references[fieldLayout] = new FieldLayoutReference( _rootReference, fieldLayout, parentFieldLayoutReference );
			}
		}

		private bool IsHierarchicalFieldLayout( FieldLayout fieldLayout )
		{
			FieldLayout parent;
			while ( null != ( parent = fieldLayout.ParentFieldLayout ) && parent != fieldLayout )
				fieldLayout = parent;

			FieldLayout rootFieldLayout = this.DataPresenter.RecordManager.FieldLayout;
			return fieldLayout == rootFieldLayout;
		}

		private bool IsValidFieldLayout( FieldLayout fieldLayout )
		{
			
			// that was used with previous data source but will not be used with the 
			// current data source of the data presenter.

			FieldLayout parent = fieldLayout.ParentFieldLayout;
			if ( null != parent && !this.IsValidFieldLayout( parent ) )
				return false;

			return true;
		}

		private void IdentifyFieldLayouts( )
		{
			_validFieldLayouts.Clear( );
			_hierarchicalFieldLayouts.Clear( );
			_nonHierarchicalFieldLayouts.Clear( );

			foreach ( var ii in _dataPresenter.FieldLayouts )
			{
				if ( this.IsValidFieldLayout( ii ) )
				{
					_validFieldLayouts.Add( ii );

					if ( this.IsHierarchicalFieldLayout( ii ) )
						_hierarchicalFieldLayouts.Add( ii );
					else
						_nonHierarchicalFieldLayouts.Add( ii );
				}
			}
		}

		#region GetFieldLayout

		internal FieldLayout GetFieldLayout( FieldLayout ensureParentFieldLayout, string name )
		{
			// SSP 12/13/06 BR18342
			
			
			// 
			
			FieldLayout fieldLayout = this.GetFieldLayout( name, ensureParentFieldLayout, true );

			if ( ensureParentFieldLayout != fieldLayout.ParentFieldLayout )
			{
				if ( null == ensureParentFieldLayout )
					RefUtils.ThrowRootFieldLayoutNameExpectedException( this, name );
				else
					throw new CalculationException( string.Format(
						"{0} is not a child field-layout of {1}", ReferenceManager.ResolveElementName( this, ensureParentFieldLayout ), name ) );
			}

			return fieldLayout;
		}

		
		
		
		
		internal FieldLayout GetFieldLayout( string name, FieldLayout preferredParentFieldLayout, bool raiseException )
		{
			FieldLayout retFieldLayout = null;

			foreach ( FieldLayout ii in this.FieldLayouts )
			{
				if ( RefParser.AreStringsEqual( ReferenceManager.ResolveElementName( this, ii ), name, true ) )
				{
					// SSP 12/13/06 BR18342
					// Added an overload of GetBand that takes in preferredParentBand parameter. This is in case there are
					// multiple bands with the same name.
					// 
					// --------------------------------------------------------------------------------
					//return band;
					retFieldLayout = ii;

					// If the band's parent band is the preferredParentBand then look no further and
					// break out. If not then keep looking for a matching band.
					// 
					if ( null == preferredParentFieldLayout || preferredParentFieldLayout == ii.ParentFieldLayout )
						break;
					// --------------------------------------------------------------------------------
				}
			}

			// SSP 12/13/06 BR18342
			// 
			//if ( raiseException ) 
			if ( null == retFieldLayout && raiseException )
				throw new CalculationException( RefUtils.GetString( "LER_Calc_FieldLayoutNameExpected", name ) );

			return retFieldLayout;
		}

		#endregion // GetFieldLayout

		#region GetFieldLayoutReference

		internal ICalculationReference GetFieldLayoutReference( FieldLayout ensureParentFieldLayout, string name, bool returnRefError )
		{
			// SSP 12/13/06 BR18342
			// Added an overload of GetBand that takes in preferredParentBand parameter. This is in case there are
			// multiple bands with the same name.
			// 
			//UltraGridBand band = GetBand( layout, name, false );
			FieldLayout fieldLayout = this.GetFieldLayout( name, ensureParentFieldLayout, false );

			if ( null == fieldLayout )
				return returnRefError ? new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_FieldLayoutNameExpected", name ) ) : null;

			if ( ensureParentFieldLayout != fieldLayout.ParentFieldLayout )
			{
				if ( !returnRefError )
					return null;

				if ( null == ensureParentFieldLayout )
					return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_RootFieldLayoutNameExpected", this.ElementName, name ) );
				else
					return new CalculationReferenceError( name, string.Format( "{0} is not a child field-layout of {1}", ReferenceManager.ResolveElementName( this, ensureParentFieldLayout ), name ) );
			}

			return this.GetFieldLayoutReference( fieldLayout, returnRefError );
		}

		internal ICalculationReference GetFieldLayoutReference( string name )
		{
			FieldLayout fl = this.GetFieldLayout( name, null, false );

			if ( null != fl )
				return this.GetFieldLayoutReference( fl, true );

			return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_FieldLayoutNameExpected", name ) );
		}

		internal ICalculationReference GetFieldLayoutReference( FieldLayout fieldLayout, bool returnRefError )
		{
			FieldLayoutReference flRef;
			if ( _fieldLayoutReferences.TryGetValue( fieldLayout, out flRef ) )
				return flRef;

			return returnRefError
				? new CalculationReferenceError( fieldLayout.CalculationReferenceId, RefUtils.GetString( "LER_Calc_InvalidFieldLayout", fieldLayout.ToString( ) ) )
				: null;
		}

		#endregion // GetFieldLayoutReference

		#region CalcManagerNotificationsSuspended

		public bool CalcManagerNotificationsSuspended
		{
			get
			{
				
				return false;
			}
		}

		#endregion // CalcManagerNotificationsSuspended

		#region DataPresenter

		public DataPresenterBase DataPresenter
		{
			get
			{
				return _dataPresenter;
			}
		}

		#endregion // DataPresenter

		#region IsValid

		/// <summary>
		/// Indicates if the reference is valid.
		/// </summary>
		internal bool IsValid
		{
			get
			{
				return _isValid;
			}
		}

		#endregion // IsValid

		#region RootRecords

		public RecordCollectionBase RootRecords
		{
			get
			{
				return _rootRecords;
			}
		} 

		#endregion // RootRecords

		#region Dispose

		/// <summary>
		/// Sets IsValid to false.
		/// </summary>
		internal void Dispose( )
		{
			_isValid = false;
		}

		#endregion // Dispose

		#region RefBase Overrides

		public override XamCalculationManager CalcManager
		{
			get
			{
				return _calcManager;
			}
		}

		internal IList<FieldLayout> FieldLayouts
		{
			get
			{
				return this.DataPresenter.FieldLayouts;
			}
		}

		public override RefBase BaseParent
		{
			get
			{
				return null;
			}
		}

		public override ICalculationReference FindItem( string name )
		{
			var flRef = this.GetFieldLayoutReference( name );

			if ( !( flRef is FieldLayoutReference ) )
			{
				Debug.Assert( flRef is CalculationReferenceError, "Unexpected band reference type." );
				return flRef;
			}

			// If the field-layout is root field-layout then return the root records reference.
			// 
			var rootRecords = this.RootRecords;
			if ( rootRecords.FieldLayout == ( (FieldLayoutReference)flRef ).FieldLayout )
				return _utils.GetRecordCollectionReference( rootRecords, true );

			// Otherwise return nested reference that represents all the records associated with the
			// field-layout.
			// 
			return _utils.GetNestedRecordsReference( this.RootRecords, (FieldLayoutReference)flRef );
		}

		public override ICalculationReference FindSummaryItem( string name )
		{
			return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_RootFieldLayoutNameExpected", this.ElementName, name ) );
		}

		public override ICalculationReference FindItem( string name, string index )
		{
			// Name must be the root field-layout name.
			// 
			ICalculationReference flRef = this.GetFieldLayoutReference( name );

			if ( !( flRef is FieldLayoutReference ) )
				return flRef;

			return _utils.GetRecordReference( this.RootRecords, ((FieldLayoutReference)flRef).FieldLayout, index, name );
		}

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			// Call GetBand to make sure the name is the name of the first band. 
			// GetBand will throw an exception if name is not the name of the 
			// root band.
			// 
			ICalculationReference flRef = this.GetFieldLayoutReference( null, name, true );

			if ( !( flRef is FieldLayoutReference ) )
				return flRef;

			if ( isRelative )
				return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_RelativeIndexInvalid" ) );

			return _utils.GetRecordReference( this.RootRecords, index, name, true );
		}

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		public override string AbsoluteName
		{
			get
			{
				return RefParser.RefFullyQualifiedString + this.ElementName;
			}
		}

		private ICalculationReference BuildReferenceHelper( RefParser newRef, bool forceDataRef, out bool returnedRefFromThisGrid )
		{
			returnedRefFromThisGrid = false;
			IEnumerator tupleEnum = newRef.GetEnumerator( );

			// If fully qualified, advance past the control name. Make sure it's this control.
			// If not, pass it off to the right control
			//
			if ( newRef.IsFullyQualified )
			{
				if ( !tupleEnum.MoveNext( ) )
					return new CalculationReferenceError( newRef.ToString( ), RefUtils.GetString( "LER_Calc_InvalidReference", newRef.ToString( ) ) );

				RefTuple tuple = (RefTuple)tupleEnum.Current;
				if ( tuple.Type != RefTuple.RefType.Identifier )
					return new CalculationReferenceError( newRef.ToString( ), RefUtils.GetString( "LER_Calc_InvalidReference", newRef.ToString( ) ) );

				if ( tuple.Scope != RefTuple.RefScope.Any )
					return new CalculationReferenceError( newRef.ToString( ), RefUtils.GetString( "LER_Calc_InvalidReference", newRef.ToString( ) ) );

				if ( 0 != String.Compare( this.ElementName, tuple.Name, true ) )
					return (RefBase)this.CalcManager.GetReference( newRef.ToString( ) );
			}

			returnedRefFromThisGrid = true;
			return RefLoop( this, tupleEnum, forceDataRef );
		}

		protected override ICalculationReference BuildReference( RefParser newRef, bool forceDataRef )
		{
			bool refFromThisGrid;
			return this.BuildReferenceHelper( newRef, forceDataRef, out refFromThisGrid );
		}

		public override ICalculationReference CreateReference( string reference )
		{
			try
			{
				RefParser rp = new RefParser( reference );
				if ( 0 == rp.TupleCount )
					return new CalculationReferenceError( reference, RefUtils.GetString( "LER_Calc_InvalidReference", reference ) );

				bool refFromThisGrid;
				ICalculationReference refLoopResult = this.BuildReferenceHelper( rp, false, out refFromThisGrid );

				if ( !refFromThisGrid || refLoopResult is CalculationReferenceError || !( refLoopResult is RefBase ) )
					return refLoopResult;

				RefBase result = (RefBase)refLoopResult;
				if ( result.IsAnchored )
					result = new RefUnAnchored( result );

				result.RelativeReference = rp;

				return FilterReference( result );
			}
			catch ( Exception e )
			{
				return new CalculationReferenceError( reference, e.Message );
			}
		}

		#region GetChildReferences

		// SSP 9/7/04
		// Added GetChildReferences method to ICalculationReference interface.
		//
		public override ICalculationReference[] GetChildReferences( ChildReferenceType referenceType )
		{
			return _fieldLayoutReferences.Values.ToArray<ICalculationReference>( );
		}

		#endregion // GetChildReferences

		#endregion // RefBase Overrides
	}

	#endregion // DataPresenterReference Class



	#region RangeReference Class

	/// <summary>
	/// A class to represent range references.
	/// </summary>
	internal class RangeReference : DataPresenterRefBase
	{
		#region Private Vars

		internal readonly FieldReference _fieldReference;
		private string _referenceName;
		private RefParser _fromRP;
		private RefParser _toRP;
		private RefTuple _fromTuple;
		private RefTuple _toTuple;

		#endregion // Private Vars

		#region Constructor

		internal RangeReference( FieldReference fieldReference,
			RefParser fromRP, RefParser toRP, RefTuple fromTuple, RefTuple toTuple )
			: base( fieldReference._rootReference, null, fieldReference.Context )
		{
			if ( null == fromTuple || null == toTuple || null == fromRP || null == toRP )
				throw new ArgumentNullException( );

			_fieldReference = fieldReference;
			_fromRP = fromRP;
			_toRP = toRP;
			_fromTuple = fromTuple;
			_toTuple = toTuple;
			_referenceName = _fromRP.ToString( ) + RefUtils.RANGE_REFERENCE_SEPARATOR + _toRP.ToString( );
		}

		#endregion // Constructor

		#region FieldContext

		internal override Field FieldContext
		{
			get
			{
				return _fieldReference.Field;
			}
		}

		#endregion // FieldContext

		#region RefBase Overrides

		public override RefBase BaseParent
		{
			get
			{
				return _fieldReference._fieldLayoutReference;
			}
		}

		public override string ElementName
		{
			get
			{
				return _fromTuple.ToString( ) + RefUtils.RANGE_REFERENCE_SEPARATOR + _toTuple.ToString( );
			}
		}

		public override string AbsoluteName
		{
			get
			{
				return _fromRP.ToString( ) + RefUtils.RANGE_REFERENCE_SEPARATOR + _toRP.ToString( );
			}
		}

		public override string NormalizedAbsoluteName
		{
			get
			{
				return this.AbsoluteName;
			}
		}

		public override RefParser ParsedReference
		{
			get
			{
				return _fromRP;
			}
		}

		public override bool ContainsReference( ICalculationReference reference )
		{
			return _fieldReference.ContainsReference( reference );
		}

		public override bool IsEnumerable
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Return true from HasRelativeIndex since this tells the calc-engine to mark the formula
		/// refernece holding this range reference as a token. Marking mechanism is used to dirty
		/// the whole column when a dependant cell gets modified so the whole column gets recalculated
		/// instead of just the corresponding cell.
		/// </summary>
		public override bool HasRelativeIndex
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// For each reference tuple in this reference that has a relative index, mark the
		/// corresponding tuple in inReference.
		/// </summary>
		/// <param name="formulaReference">The Reference to be marked.</param>
		public override void MarkRelativeIndices( ICalculationReference formulaReference )
		{
			if ( formulaReference is DataPresenterRefBase )
			{
				RefParser fieldRP = _fieldReference.ParsedReference;
				RefParser targetRP = ( (DataPresenterRefBase)formulaReference ).ParsedReference;

				Debug.Assert( null != fieldRP && null != targetRP );
				if ( null != fieldRP && null != targetRP )
					fieldRP.MarkRelativeIndices( targetRP, true );
			}
		}

		// SSP 2/4/05 BR02240
		// Overrode the Formula property so we can return the column's formula here. The reason
		// for doing this is that if some formula has this range reference as one of its tokens, 
		// dependancy calculations need to know about the formula of the range reference. 
		//
		public override ICalculationFormula Formula
		{
			get
			{
				return _fieldReference.Formula;
			}
		}

		#endregion // RefBase Overrides

		#region RangeEnumerator

		private class RangeEnumerator : IEnumerator
		{
			private ResolvedRangeReference _resolvedRange;
			private int _index;
			private ICalculationReference _currentCell;
			private RefUtils _utils;


			/// <summary>
			/// From and to are inclusive.
			/// </summary>
			/// <param name="resolvedRange"></param>
			internal RangeEnumerator( ResolvedRangeReference resolvedRange )
			{
				_resolvedRange = resolvedRange;
				_utils = resolvedRange._rootReference._utils;
				this.Reset( );
			}

			public void Reset( )
			{
				_currentCell = null;
				_index = _resolvedRange._from - 1;
			}

			public bool MoveNext( )
			{
				_index++;
				_currentCell = null;
				if ( _index <= _resolvedRange._to )
				{
					DataRecordReference recordRef = _utils.GetRecordReference( _resolvedRange._recordsReference._records, _index, false ) as DataRecordReference;
					if ( null != recordRef )
						_currentCell = _utils.GetCellReference( recordRef, _resolvedRange._range._fieldReference );
				}

				return null != _currentCell;
			}

			public object Current
			{
				get
				{
					return _currentCell;
				}
			}
		}

		#endregion // RangeEnumerator

		#region RangeReference

		private class ResolvedRangeReference : DataPresenterRefBase, ICalculationReferenceCollection
		{
			internal readonly RecordCollectionReference _recordsReference;
			internal readonly RangeReference _range;
			internal int _from;
			internal int _to;
			private RefParser _fromRP;
			private RefParser _toRP;

			internal ResolvedRangeReference( RecordCollectionReference recordsReference, int from, int to, RangeReference range )
				: base( recordsReference._rootReference, null, range.FieldContext )
			{
				_recordsReference = recordsReference;

				if ( _recordsReference.FieldLayout != range.FieldContext.Owner || _recordsReference.IsGroupByRecords )
					throw new ArgumentException( "Invalid rows collection. Rows must be from the same band as the columns in the range reference." );

				_range = range;
				_from = from;
				_to = to;
			}

			public override ICalculationReferenceCollection References
			{
				get
				{
					return this;
				}
			}

			public IEnumerator GetEnumerator( )
			{
				return new RangeEnumerator( this );
			}

			public override RefBase BaseParent
			{
				get
				{
					return _range;
				}
			}

			public override string ElementName
			{
				get
				{
					
					//
					return _range.ElementName;
				}
			}

			public override string AbsoluteName
			{
				get
				{
					this.CreateParsedReference( );
					return _fromRP.ToString( ) + RefParser.RangeSeparatorWithSpaces + _toRP.ToString( );
				}
			}

			protected override RefParser CreateParsedReference( )
			{
				if ( null == _fromRP )
				{
					string rowsName = _recordsReference.AbsoluteName;
					StringBuilder sb = new StringBuilder( rowsName );
					int len = sb.Length;
					sb.Append( RefParser.RefBeginScope ).Append( _from ).Append( RefParser.RefEndScope );

					_fromRP = new RefParser( sb.ToString( ) );

					sb.Remove( len, sb.Length - len );
					sb.Append( RefParser.RefBeginScope ).Append( _to ).Append( RefParser.RefEndScope );

					_toRP = new RefParser( sb.ToString( ) );
				}

				return _fromRP;
			}

			public override string NormalizedAbsoluteName
			{
				get
				{
					return this.AbsoluteName;
				}
			}

			public override bool IsEnumerable
			{
				get
				{
					return true;
				}
			}
		}

		#endregion // RangeReference

		#region GetIndexFromTuple

		private int GetIndexFromTuple( RefTuple tuple, RecordCollectionReference recordsReference, RecordReferenceBase recordReferenceContext, out CalculationReferenceError error )
		{
			error = null;
			int index = 0;

			// SSP 10/17/04 - Formula row source index
			// 
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			if ( RefTuple.RefScope.RelativeIndex == tuple.Scope )
			{
				int rowCalcIndex = null != recordReferenceContext ? _rootReference._utils.GetRecordCalcIndex( recordReferenceContext.Record ) : -1;
				index = rowCalcIndex >= 0 ? rowCalcIndex + tuple.ScopeIndex : -1;
			}
			else if ( RefTuple.RefScope.Index == tuple.Scope )
				index = tuple.ScopeIndex;
			else if ( RefTuple.RefScope.Any == tuple.Scope )
				index = null != recordReferenceContext ? _rootReference._utils.GetRecordCalcIndex( recordReferenceContext.Record ) : -1;
			else if ( RefTuple.RefScope.All == tuple.Scope )
				index = tuple == _fromTuple ? 0 : _rootReference._utils.GetMaxCalcIndex( recordsReference.Records );

			if ( index < 0 || index > _rootReference._utils.GetMaxCalcIndex( recordsReference.Records ) )
				error = new CalculationReferenceError( _referenceName, "Invalid range for the cell." );

			return index;
		}

		#endregion // GetIndexFromTuple

		#region GetResolvedRangeReference

		internal ICalculationReference GetResolvedRangeReference( RecordCollectionReference recordsReference, RecordReferenceBase recordReferenceContext )
		{
			if ( recordsReference.FieldLayoutContext != this.FieldLayoutContext || null != recordReferenceContext && recordReferenceContext.FieldLayoutContext != this.FieldLayoutContext )
				return new CalculationReferenceError( _referenceName, "Formula not from the same band as the columns in the range reference." );

			// Row context must not be a group-by row. It can be null if from and to don't have scope of
			// RelativeIndex.
			//
			if ( recordReferenceContext is GroupByRecordReference || recordsReference.IsGroupByRecords )
				return new CalculationReferenceError( _referenceName, "Invalid context of group-by row." );

			if ( null == recordReferenceContext
				&& ( RefTuple.RefScope.RelativeIndex == _fromTuple.Scope
				|| RefTuple.RefScope.RelativeIndex == _toTuple.Scope ) )
				return new CalculationReferenceError( _referenceName, "Range refernece doesn't have any context of row." );

			CalculationReferenceError error;

			int fromIndex = this.GetIndexFromTuple( _fromTuple, recordsReference, recordReferenceContext, out error );
			if ( null != error )
				return error;

			int toIndex = this.GetIndexFromTuple( _toTuple, recordsReference, recordReferenceContext, out error );
			if ( null != error )
				return error;

			return new ResolvedRangeReference( recordsReference, fromIndex, toIndex, this );
		}

		#endregion // GetResolvedRangeReference
	}

	#endregion // RangeReference Class


	internal class ItemCache<TKey, TItem> : IEnumerable<TKey>
		where TKey : class
		where TItem : class
	{
		private readonly Dictionary<TKey, TItem> _references;
		private Func<TKey, TItem> _referenceCreator;
		private DataPresenterReference _rootRef;
		private Func<TItem, string> _nameRetriever;
		private Action<TItem> _referenceInitializer;

		internal ItemCache( DataPresenterReference rootRef, Func<TKey, TItem> referenceCreator, Action<TItem> referenceInitializer = null, Func<TItem, string> nameRetriever = null )
		{
			CoreUtilities.ValidateNotNull( rootRef );
			CoreUtilities.ValidateNotNull( referenceCreator );

			_rootRef = rootRef;
			_referenceCreator = referenceCreator;
			_referenceInitializer = referenceInitializer;
			_nameRetriever = nameRetriever;

			_references = new Dictionary<TKey, TItem>( );
		}

		internal TItem GetItem( TKey item, bool allocate = true )
		{
			TItem reference;
			if ( !_references.TryGetValue( item, out reference ) && allocate )
			{
				_references[item] = reference = _referenceCreator( item );

				if ( null != _referenceInitializer )
					_referenceInitializer( reference );
			}

			return reference;
		}

		internal TItem GetItem( string name )
		{
			if ( null != _nameRetriever )
			{
				foreach ( var ii in _references )
				{
					if ( RefParser.AreStringsEqual( _nameRetriever( ii.Value ), name, true ) )
						return ii.Value;
				}
			}

			return null;
		}

		internal void Remove( TKey item )
		{
			_references.Remove( item );
		}

		public IEnumerator<TKey> GetEnumerator( )
		{
			return _references.Keys.GetEnumerator( );
		}

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}
	}


	#region FieldLayoutReference Class

	/// <summary>
	/// Reference implementation that represents a grid band.
	/// </summary>
	internal class FieldLayoutReference : DataPresenterRefBase
	{
		#region Private Variables

		private FieldLayout _fieldLayout;

		internal readonly ItemCache<Field, ReferenceManager> _fieldReferences;
		internal readonly ItemCache<SummaryDefinition, ReferenceManager> _summaryReferences;
		private int verifiedColumnsVersion = -1;
		private readonly FieldLayoutReference _parentFieldLayoutReference;
		private PropertyValueTracker _summaryDefinitionsTracker;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="rootRef">Data presenter root reference.</param>
		/// <param name="fieldLayout">Field-layout object.</param>
		/// <param name="parentFieldLayoutReference">Parent field-layout reference if the field-layout has a parent field-layout.</param>
		internal FieldLayoutReference( DataPresenterReference rootRef, FieldLayout fieldLayout, FieldLayoutReference parentFieldLayoutReference )
			: base( rootRef, ReferenceManager.ResolveElementName( rootRef, fieldLayout ), fieldLayout )
		{
			_fieldLayout = fieldLayout;
			_parentFieldLayoutReference = parentFieldLayoutReference;
			_fieldReferences = new ItemCache<Field, ReferenceManager>( _rootReference, field => new ReferenceManager( this, field ), rm => rm.Initialize( ), rm => rm._elementName );
			_summaryReferences = new ItemCache<SummaryDefinition, ReferenceManager>( _rootReference, summary => new ReferenceManager( this, summary ), rm => rm.Initialize( ), rm => rm._elementName );

			_summaryDefinitionsTracker = new PropertyValueTracker( fieldLayout, "SummaryDefinitions.SummariesVersion", this.ProcessSummaryDefinitionsChanged, true );

			Debug.Assert( ( null != parentFieldLayoutReference ) == ( null != fieldLayout.ParentFieldLayout )
				, "If the field-layout has a parent field-layout then the 'parentFieldLayoutReference' parameter must be passed in." );
		}

		#endregion // Constructor

		#region FieldLayout

		/// <summary>
		/// Returns the associated field-layout.
		/// </summary>
		internal FieldLayout FieldLayout
		{
			get
			{
				return _fieldLayout;
			}
		}

		#endregion // FieldLayout

		#region ParentFieldLayoutReference

		/// <summary>
		/// Gets the parent field-layout reference if any.
		/// </summary>
		internal FieldLayoutReference ParentFieldLayoutReference
		{
			get
			{
				return _parentFieldLayoutReference;
			}
		}

		#endregion // ParentFieldLayoutReference

		#region VerifyGroupLevelSummaryFormulas

		internal int _verifiedSummariesVersion = -1;
		private int _verifiedGroupByHierarchyVersion = -1;

		private List<GroupLevelSummaryDefinitionReference> _groupLevelRelatedReferences = null;

		// SSP 3/30/06 BR11065
		// 
		#region Bug fix for BR11065

		private int _suspendVerifyGroupLevelSummaryFormulas_Counter = 0;
		internal void SuspendVerifyGroupLevelSummaryFormulas( )
		{
			this._suspendVerifyGroupLevelSummaryFormulas_Counter++;
		}

		private bool IsVerifyGroupLevelSummaryFormulasSuspended
		{
			get
			{
				return this._suspendVerifyGroupLevelSummaryFormulas_Counter > 0;
			}
		}

		internal void ResumeVerifyGroupLevelSummaryFormulas( bool verifyOnResume )
		{
			if ( this.IsVerifyGroupLevelSummaryFormulasSuspended )
			{
				this._suspendVerifyGroupLevelSummaryFormulas_Counter--;

				if ( !this.IsVerifyGroupLevelSummaryFormulasSuspended && verifyOnResume )
					this.VerifyGroupLevelSummaryFormulas( );
			}
		}

		#endregion // Bug fix for BR11065

		/// <summary>
		/// Creates the group-by level summary formulas whenever the group-by hierarchy changes.
		/// </summary>
		internal void VerifyGroupLevelSummaryFormulas( )
		{
			// SSP 2/16/05 BR01753
			// Create the group-by level and group-by summary references regardless of
			// whether the calc is suspended or there is no calc manager. Otherwise when a
			// summary's CalcReference is being created, it won't be able to find
			// appropriate group-by level reference (if group-by hierarchy have changed
			// since suspend, a common situation when printing with group-by columns) and
			// will end up throwing an exception in the constructor of the summary value
			// reference.
			//
			//if ( this._verifiedGroupByHierarchyVersion == this.Band.GroupByHierarchyVersion
			//	|| this.Layout.CalcManagerNotificationsSuspended || null == this.CalcManager )
			//	return;
			// SSP 5/26/05 BR04280
			// We also need to recreate the group-by level summary references if the summaries 
			// are added or removed.
			//
			// --------------------------------------------------------------------------------
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			int groupByVersion = _fieldLayout.GroupByVersion;
			var summaries = _fieldLayout.SummaryDefinitionsIfAllocated;
			int summariesVersion = null != summaries ? summaries.SummariesVersion : 0;

			if ( this._verifiedGroupByHierarchyVersion == groupByVersion
				&& this._verifiedSummariesVersion == summariesVersion )
				return;

			// SSP 3/30/06 BR11065
			// 
			if ( this.IsVerifyGroupLevelSummaryFormulasSuspended )
				return;

			// SSP 6/13/05 BR01753
			// Don't cause the other summaries to be recalculated when a summary is added.
			// 
			bool reuseReferences = this._verifiedGroupByHierarchyVersion == groupByVersion;

			this._verifiedGroupByHierarchyVersion = groupByVersion;
			this._verifiedSummariesVersion = summariesVersion;
			// --------------------------------------------------------------------------------

			ICalculationManager calcManager = this.CalcManager;

			// SSP 2/16/05 BR01753
			// Related to the change above regarding creating the group-by level references
			// regardless of whether the calc is suspended.
			//
			bool isSuspended = null == calcManager || _rootReference.CalcManagerNotificationsSuspended;

			List<GroupLevelSummaryDefinitionReference> reuseReferencesList = new List<GroupLevelSummaryDefinitionReference>( );

			if ( null != this._groupLevelRelatedReferences )
			{
				List<GroupLevelSummaryDefinitionReference> list = this._groupLevelRelatedReferences;

				this._groupLevelRelatedReferences = null;

				foreach ( GroupLevelSummaryDefinitionReference glsr in list )
				{
					if ( null != glsr )
					{
						// SSP 6/13/05 BR01753
						// Don't cause the other summaries to be recalculated when a summary is added.
						// 
						// ------------------------------------------------------------------------------
						if ( reuseReferences && _fieldLayout.SummaryDefinitions.Contains( glsr.Summary ) )
						{
							string oldFormula = null != glsr.Formula ? glsr.Formula.FormulaString : string.Empty;
							ReferenceManager summaryRM = _summaryReferences.GetItem( glsr.Summary, false );
							string newFormula = ( null != summaryRM ? summaryRM._formula : null ) ?? string.Empty;
							if ( oldFormula == newFormula )
							{
								reuseReferencesList.Add( glsr );
								continue;
							}
						}
						// ------------------------------------------------------------------------------

						if ( null != glsr.Formula )
						{
							// SSP 2/4/05
							// Use the AddFormulaHelper and RemoveFormulaHelper utility methods 
							// instead. See comments above those method definitions for more info.
							//
							//calcManager.RemoveFormula( glsr.Formula );
							// SSP 2/16/05 BR01753
							// Only remove if not suspended.
							//
							if ( !isSuspended )
								RefUtils.RemoveFormulaHelper( glsr, glsr.Formula );

							// SSP 6/10/05 
							// Moved this below after the this if block because we need to set disposed
							// on the reference regardless of whether it had any formulas.
							// 
							//glsr.InternalSetIsDisposed( true );
						}

						// SSP 6/10/05 
						// Moved this here from above if block because we need to set disposed
						// on the reference regardless of whether it had any formulas.
						// 
						glsr.InternalSetIsDisposed( true );
					}

					// SSP 2/16/05 BR01753
					// Only remove if not suspended.
					//
					if ( !isSuspended )
					{
						// MD 8/10/07 - 7.3 Performance
						// Use generics
						//calcManager.RemoveReference( reference );
						calcManager.RemoveReference( glsr );
					}
				}
			}

			if ( _fieldLayout.HasGroupBySortFields )
			{
				_groupLevelRelatedReferences = new List<GroupLevelSummaryDefinitionReference>( );

				foreach ( SummaryDefinition summary in _fieldLayout.SummaryDefinitions )
				{
					SummaryDefinitionReference summaryReference = (SummaryDefinitionReference)this.GetSummaryDefinitionReference( summary, false );
					if ( null == summaryReference )
						continue;

					// SSP 2/16/05 BR01753
					// Create the summary reference regardless of whether it has a formula.
					//
					//if ( summary.HasActiveFormula )
					//{
					DataPresenterRefBase parentReference = this;
					var sortedFields = _fieldLayout.SortedFields;
					for ( int i = 0; i < sortedFields.Count; i++ )
					{
						var sortedFieldDescription = sortedFields[i];
						Field groupByField = sortedFieldDescription.Field;

						// Break out once we encounter a sort column that's not a group-by column
						// since the sorted columns collection always maintains group-by sort 
						// columns before the non-group-by sort columns.
						//
						if ( !groupByField.IsGroupBy )
							break;

						GroupLevelSummaryDefinitionReference reference = null;

						// SSP 6/13/05 BR01753
						// Don't cause the other summaries to be recalculated when a summary is added.
						// 
						// ------------------------------------------------------------------------------
						if ( reuseReferences && null != reuseReferencesList )
						{
							for ( int j = 0; j < reuseReferencesList.Count; j++ )
							{
								// MD 8/10/07 - 7.3 Performance
								// Use generics
								//GroupLevelSummaryDefinitionReference glsr = reuseReferencesList[j] as GroupLevelSummaryDefinitionReference;
								GroupLevelSummaryDefinitionReference glsr = reuseReferencesList[j];

								if ( null != glsr && glsr.Summary == summary && glsr.GroupByField == groupByField )
								{
									reuseReferencesList[j] = null;
									reference = glsr;
									break;
								}
							}
						}
						// ------------------------------------------------------------------------------

						if ( null == reference )
						{
							FieldReference groupByFieldReference = (FieldReference)this.GetFieldReference( groupByField, false );
							Debug.Assert( null != groupByFieldReference );

							GroupLevelReference parentGlReference = new GroupLevelReference( parentReference, groupByFieldReference );
							parentReference = parentGlReference;
							reference = new GroupLevelSummaryDefinitionReference( parentGlReference, summaryReference );
						}

						this._groupLevelRelatedReferences.Add( reference );
					}
					//}
				}

				// SSP 2/16/05 BR01753
				// Enclosed the existing code into the if block.
				//
				if ( !isSuspended )
				{
					foreach ( object item in this._groupLevelRelatedReferences )
					{
						GroupLevelSummaryDefinitionReference reference = item as GroupLevelSummaryDefinitionReference;
						if ( null != reference )
						{
							calcManager.AddReference( reference );
							var calcSettings = reference.Summary.CalculationSettings;
							string formula = null != calcSettings ? calcSettings.Formula : null;
							reference.EnsureFormulaRegistered( formula );
						}
					}
				}

				if ( this._groupLevelRelatedReferences.Count <= 0 )
					this._groupLevelRelatedReferences = null;
			}
		}

		#endregion // VerifyGroupLevelSummaryFormulas

		#region GetFieldHelper

		internal Field GetFieldHelper( string name )
		{
			var fields = _fieldLayout.Fields;
			int index = fields.IndexOf( name );
			if ( index < 0 )
			{
				for ( int i = 0, count = fields.Count; i < count; i++ )
				{
					if ( RefParser.AreStringsEqual( ReferenceManager.ResolveElementName( fields[i] ), name, true ) )
					{
						index = i;
						break;
					}
				}
			}

			Field field = index >= 0 ? fields[index] : null;

			return field;
		}

		#endregion // GetFieldHelper

		#region GetFieldReference

		internal ICalculationReference GetFieldReference( Field field, bool returnErrorRef )
		{
			ReferenceManager rm = _fieldReferences.GetItem( field );
			FieldReference fieldRef = null != rm ? (FieldReference)rm.CalcReference : null;

			if ( null == fieldRef && returnErrorRef )
				return new CalculationReferenceError( field.Name, RefUtils.GetString( "LER_Calc_InvalidField", this.ElementName, field.Name ) );

			return fieldRef;
		}

		internal ICalculationReference GetFieldReference( string name, bool returnErrorRef )
		{
			ReferenceManager rm = _fieldReferences.GetItem( name );
			if ( null != rm && null != rm.CalcReference )
				return rm.CalcReference;

			Field field = this.GetFieldHelper( name );
			if ( null != field )
				return this.GetFieldReference( field, returnErrorRef );

			if ( returnErrorRef )
				return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_FieldNotFound", this.ElementName, name ) );

			return null;
		}

		#endregion // GetFieldReference

		#region GetSummaryDefinitionReference

		internal SummaryDefinition GetSummaryDefinitionHelper( string name )
		{
			SummaryDefinitionCollection summaries = _fieldLayout.SummaryDefinitionsIfAllocated;
			if ( null != summaries )
				return summaries.FirstOrDefault( summary => 
					RefParser.AreStringsEqual( ReferenceManager.ResolveElementName( summary ), name, true ) );

			return null;
		}

		internal ICalculationReference GetSummaryDefinitionReference( string name, bool returnRefError )
		{
			var summary = this.GetSummaryDefinitionHelper( name );

			var reference = null != summary ? this.GetSummaryDefinitionReference( summary, false ) : null;

			if ( null == reference && returnRefError )
				return new CalculationReferenceError( string.Format( "Summary {0} not found.", name ) );

			return reference;
		}

		internal ICalculationReference GetSummaryDefinitionReference( SummaryDefinition summaryDefinition, bool returnRefError )
		{
			ReferenceManager rm = _summaryReferences.GetItem( summaryDefinition, true );
			SummaryDefinitionReference summaryRef = null != rm ? (SummaryDefinitionReference)rm.CalcReference : null;

			if ( null == summaryRef && returnRefError )
				return new CalculationReferenceError( summaryDefinition.Key, string.Format( "Cannot create reference for summary with the key of '{0}'.", summaryDefinition.Key ) );

			return summaryRef;
		}

		/// <summary>
		/// Gets the summary reference for a particular group level associated with the passed in summary value.
		/// </summary>
		/// <param name="summaryResult"></param>
		/// <returns></returns>
		internal SummaryDefinitionReference GetSummaryDefinitionReference( SummaryResult summaryResult )
		{
			GroupByRecord groupByRecord = summaryResult.ParentCollection.Records.ParentRecord as GroupByRecord;
			Field groupByField = null != groupByRecord ? groupByRecord.GroupByField : null;
			return this.GetSummaryDefinitionReference( summaryResult.SummaryDefinition, groupByField );
		}

		/// <summary>
		/// Gets the summary reference for a particular group level associated with the passed in group-by column.
		/// </summary>
		/// <param name="summary"></param>
		/// <param name="groupByField"></param>
		/// <returns></returns>
		internal SummaryDefinitionReference GetSummaryDefinitionReference( SummaryDefinition summary, Field groupByField )
		{
			this.VerifyGroupLevelSummaryFormulas( );

			if ( null != this._groupLevelRelatedReferences )
			{
				foreach ( GroupLevelSummaryDefinitionReference reference in this._groupLevelRelatedReferences )
				{
					// SSP 6/10/05 BR04539
					// Added below condition that checks to see if the the summary is the same.
					// 
					if ( reference.Summary == summary && groupByField == reference.GroupByField )
					{
						return reference;
					}
				}
			}

			if ( null == groupByField )
				return (SummaryDefinitionReference)this.GetSummaryDefinitionReference( summary, false );

			return null;
		}

		#endregion // GetSummaryDefinitionReference

		#region ProcessFieldsChanged

		/// <summary>
		/// Called when the field-layout's Fields collection is changed.
		/// </summary>
		internal void ProcessFieldsChanged( )
		{
			var addedFields = _fieldLayout.Fields.Except( _fieldReferences );
			var removedFields = _fieldReferences.Except( _fieldLayout.Fields );

			foreach ( var field in removedFields )
			{
				ReferenceManager referenceManager = _fieldReferences.GetItem( field, false );
				if ( null != referenceManager )
				{
					referenceManager.UnregisterFromCalcManager( );
					_fieldReferences.Remove( field );
				}
			}

			foreach ( var field in addedFields )
			{
				// Simply allocate the ReferenceManager for the fields with the formulas to register 
				// those fields and their formulas.
				// 
				_fieldReferences.GetItem( field, true );
			}
		} 

		#endregion // ProcessFieldsChanged

		#region ProcessSummaryDefinitionsChanged

		/// <summary>
		/// Called when the field-layout's Fields collection is changed.
		/// </summary>
		internal void ProcessSummaryDefinitionsChanged( )
		{
			var addedSummaries = _fieldLayout.SummaryDefinitions.Except( _summaryReferences );
			var removedSummaries = _summaryReferences.Except( _fieldLayout.SummaryDefinitions );

			foreach ( var summary in removedSummaries )
			{
				ReferenceManager referenceManager = _summaryReferences.GetItem( summary, false );
				if ( null != referenceManager )
				{
					referenceManager.UnregisterFromCalcManager( );
					_summaryReferences.Remove( summary );
				}
			}

			foreach ( var summary in addedSummaries )
			{
				// Simply allocate the ReferenceManager for the summaries to register 
				// those summaries and their formulas.
				// 
				_summaryReferences.GetItem( summary, true );
			}
		}

		#endregion // ProcessSummaryDefinitionsChanged

		#region RefBase Overrides

		#region Parent

		public override RefBase BaseParent
		{
			get
			{
				// The parent of a field-layout reference is the parent field-layout reference if any or the
				// data presenter reference, even for those field-layouts that are not necessarily the root
				// field-layouts.
				// 
				return (RefBase)_parentFieldLayoutReference ?? (RefBase)_rootReference;
			}
		}

		#endregion // Parent

		#region FindItem_Nested

		internal ICalculationReference FindItem_Nested( string name, RecordCollectionBase scope )
		{
			ICalculationReference reference = this.FindItem( name );

			if ( reference is FieldReference )
				reference = _rootReference._utils.GetNestedFieldReference( scope, (FieldReference)reference );
			else if ( reference is SummaryDefinitionReference )
				reference = _rootReference._utils.GetNestedSummaryDefinitionReference( scope, (SummaryDefinitionReference)reference );
			else if ( reference is FieldLayoutReference )
				reference = _rootReference._utils.GetNestedRecordsReference( scope, (FieldLayoutReference)reference );

			return reference;
		} 

		#endregion // FindItem_Nested

		#region FindItem

		public override ICalculationReference FindItem( string name )
		{
			var fieldRef = this.GetFieldReference( name, false );

			// If no such field exists then see if a summary by that name exists.
			// 
			if ( null == fieldRef )
			{
				var summaryRef = this.GetSummaryDefinitionReference( name, false );
				if ( null != summaryRef )
					return summaryRef;

				// SSP 3/23/12 TFS98845
				// Find the child field-layout if there's no field or summary by the name.
				// 
				var childFieldLayoutRef = _rootReference.GetFieldLayoutReference( _fieldLayout, name, false );
				if ( childFieldLayoutRef is FieldLayoutReference )
					return childFieldLayoutRef;

				// If a summary doesn't exist either, then call GetFieldReference again
				// and pass true for the 'returnErrorRef' parameter so a reference error
				// gets returned.
				// 
				fieldRef = this.GetFieldReference( name, true );
			}

			if ( !( fieldRef is FieldReference ) )
				return fieldRef;

			Field field = ( (FieldReference)fieldRef ).Field;

			if ( _rootReference._utils.IsChaptered( field ) )
				return _rootReference.GetFieldLayoutReference( this.FieldLayout, name, true );
			else
				return this.GetFieldReference( field, true );
		}

		#endregion // FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			return this.GetSummaryDefinitionReference( name, true );
		}

		#endregion // FindSummaryItem

		#region FindItem

		public override ICalculationReference FindItem( string name, string index )
		{
			return this.FindItem( name );
		}

		#endregion // FindItem

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return this.FindItem( name );
		}

		#endregion // FindItem

		#region FindAll

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		#endregion // FindAll

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return null == _fieldLayout
					
					//|| !_fieldLayout.IsStillValid
					;
			}
		}

		#endregion // IsDisposedReference

		#region GetChildReferences

		// SSP 9/7/04
		// Added GetChildReferences method to ICalculationReference interface.
		//
		public override ICalculationReference[] GetChildReferences( ChildReferenceType referenceType )
		{
			List<ICalculationReference> list = new List<ICalculationReference>( );

			if ( ! this.IsDisposedReference )
			{
				foreach ( var field in this.FieldLayout.Fields )
				{
					FieldReference fieldRef = (FieldReference)this.GetFieldReference( field, false );
					if ( null != fieldRef && !fieldRef.IsDisposedReference )
					{
						fieldRef._referenceManager.RegisterWithCalcManager( );
						list.Add( fieldRef );
					}
				}

				var summaries = this.FieldLayout.SummaryDefinitionsIfAllocated;
				if ( null != summaries )
				{
					foreach ( var summary in summaries )
					{
						var summaryDefRef = (SummaryDefinitionReference)this.GetSummaryDefinitionReference( summary, false );

						if ( null != summaryDefRef && !summaryDefRef.IsDisposedReference )
						{
							summaryDefRef._referenceManager.RegisterWithCalcManager( );
							list.Add( summaryDefRef );
						}
					}
				}
			}

			return list.Count > 0 ? list.ToArray( ) : null;
		}

		#endregion // GetChildReferences

		// SSP 10/12/04
		// Overrode GetHashCode, Equals, ContainsReference and IsSubsetReference methods.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return _fieldLayout.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored or NestedBandReference then delegate the call to it.
			//
			if ( obj is RefUnAnchored || obj is NestedFieldLayoutReference )
				return obj.Equals( this );

			FieldLayoutReference flRef = obj as FieldLayoutReference;
			return null != flRef && _fieldLayout == flRef._fieldLayout;
		}

		#endregion // Equals

		// MD 6/25/12 - TFS113177
		#region ShouldFormulaEditorIncludeIndex

		/// <summary>
		/// Gets the value indicating whether the formula editor should include default indexes after this reference's address when 
		/// enumerable references are used where a single value is expected.
		/// </summary>
		public override bool ShouldFormulaEditorIncludeIndex
		{
			get { return true; }
		}

		#endregion // ShouldFormulaEditorIncludeIndex

		#endregion // RefBase Overrides
	}

	#endregion // FieldLayoutReference Class

	#region ReferenceManager Class

	internal class ReferenceManager : PropertyChangeNotifierExtended, IFormulaProvider
	{
		#region Private Vars

		internal readonly FieldLayoutReference _fieldLayoutReference;
		private DataPresenterReference _rootRef;
		private DataPresenterRefBase _calcReference;
		private object _clientObject;

		private PropertyValueTracker _calcSettingsTracker;
		private PropertyValueTracker _fieldLayoutReferenceIdTracker;
		private DataPresenterCalculationSettingsBase _calcSettings;
		private Func<object, string> _defaultNameGetter;
		private XamCalculationManager _calcManager;

		private bool _calcReferenceDirty;
		internal string _name;
		internal string _formula;
		internal string _elementName;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fieldLayoutReference">Reference of the field-layout to which the clientObject belongs.</param>
		/// <param name="clientObject">Field or SummaryDefinition.</param>
		internal ReferenceManager( FieldLayoutReference fieldLayoutReference, object clientObject )
		{
			CoreUtilities.ValidateNotNull( fieldLayoutReference );
			CoreUtilities.ValidateNotNull( clientObject );

			_fieldLayoutReference = fieldLayoutReference;
			_rootRef = fieldLayoutReference._rootReference;
			_clientObject = clientObject;
			_calcManager = fieldLayoutReference.CalcManager;

			if ( clientObject is SummaryDefinition || clientObject is Field )
				_calcSettingsTracker = new PropertyValueTracker( _clientObject, "CalculationSettings", this.OnCalcSettingsChanged );
			else
				throw new ArgumentException( "Only Field and SummaryDefinition are supported." );
		}

		#endregion // Constructor

		#region Properties

		#region CalcReference

		internal DataPresenterRefBase CalcReference
		{
			get
			{
				return _calcReference;
			}
		}

		#endregion // CalcReference

		#region FormulaErrorValue

		internal object FormulaErrorValue
		{
			get
			{
				
				return null;
			}
		} 

		#endregion // FormulaErrorValue

		#endregion // Properties

		#region Methods

		#region FromCalcValue

		internal object FromCalcValue( CalculationValue calcValue, out bool error )
		{
			error = false;
			bool isConvertedValue = false;
			CultureInfo culture = CultureInfo.CurrentCulture;

			Type desiredType = typeof( object );
			if ( _clientObject is Field )
				desiredType = ( (Field)_clientObject ).DataType;

			object val = calcValue.GetResolvedValue( );

			if ( null != _calcSettings )
			{
				var converter = _calcSettings.ValueConverter;

				// If a converter was specified on the control calc settings then use that.
				// 
				if ( null != converter )
				{
					val = converter.ConvertBack( val, desiredType, _clientObject, culture );
					isConvertedValue = true;
				}
			}

			if ( !isConvertedValue && null != desiredType )
			{
				val = CoreUtilities.ConvertDataValue( val, desiredType, culture, null );

				// If conversion failed then set error out parameter to true.
				// 
				if ( null == val && !CoreUtilities.IsValueEmpty( val ) )
					error = true;

				isConvertedValue = true;
			}

			return val;
		}

		#endregion // FromCalcValue

		#region CreateReferenceHelper

		private DataPresenterFormulaRefBase CreateReferenceHelper( string elementName )
		{
			if ( _clientObject is Field )
				return new FieldReference( this, elementName, (Field)_clientObject );
			else if ( _clientObject is SummaryDefinition )
				return new SummaryDefinitionReference( this, elementName, (SummaryDefinition)_clientObject );

			return null;
		}

		#endregion // CreateReferenceHelper

		#region Initialize

		internal void Initialize( )
		{
			this.OnCalcSettingsChangedHelper( false );

			this.VerifyName( );
			this.VerifyFormula( );
			this.RegisterWithCalcManager( );
		}

		#endregion // Initialize

		#region OnCalcSettingsChanged

		private void OnCalcSettingsChanged( )
		{
			this.OnCalcSettingsChangedHelper( true );
		}

		#endregion // OnCalcSettingsChanged

		#region OnCalcSettingsChangedHelper

		private void OnCalcSettingsChangedHelper( bool verify )
		{
			DataPresenterCalculationSettingsBase calcSettings = (DataPresenterCalculationSettingsBase)_calcSettingsTracker.Target;
			PropertyChangeListenerList.ManageListenerHelper( ref _calcSettings, calcSettings, this, true );

			if ( verify )
			{
				this.VerifyName( false );
				this.VerifyFormula( false );
				this.RegisterWithCalcManager( true );
			}
		}

		#endregion // OnCalcSettingsChangedHelper

		#region OnSubObjectPropertyChanged

		internal override void OnSubObjectPropertyChanged( object sender, string property, object extraInfo )
		{
			if ( _calcSettings == sender )
			{
				switch ( property )
				{
					case "ReferenceId":
						this.VerifyName( );
						break;
					case "Formula":
						this.VerifyFormula( );
						break;
					case "TreatAsType":
					case "ValueConverter":
						if ( null != _calcReference )
							_calcReference.NotifyCalcEngineValueChanged( );
						break;
					default:
						Debug.Assert( false );
						break;
				}
			}
		}

		#endregion // OnSubObjectPropertyChanged

		#region RegisterWithCalcManager

		/// <summary>
		/// Indicates whether to 
		/// </summary>
		/// <param name="reregister"></param>
		internal void RegisterWithCalcManager( bool reregister = false )
		{
			if ( _rootRef.IsValid && ( null == _calcReference || _calcReferenceDirty || reregister ) )
			{
				// Remove the old reference.
				// 
				this.UnregisterFromCalcManager( );

				if ( !string.IsNullOrEmpty( _elementName ) )
				{
					// Create a new reference and register it.
					// 
					_calcReference = this.CreateReferenceHelper( _elementName );

					_calcManager.InternalAddReference( _calcReference );

					// EnsureFormulaRegistered checks for the string being null or empty.
					// 
					DataPresenterFormulaRefBase formulaRef = _calcReference as DataPresenterFormulaRefBase;
					Debug.Assert( null != formulaRef );
					if ( null != formulaRef )
						formulaRef.EnsureFormulaRegistered( _formula );

					// Raise Reference property changed. IFormulaProvider also implements INotifyPropertyChanged.
					// 
					this.RaisePropertyChangedEvent( "Reference" );
				}
			}
		}

		#endregion // RegisterWithCalcManager

		#region ResolveElementName

		internal static string ResolveElementName( Field field, bool normalized = false )
		{
			string name = ResolveName( field );

			return null != name ? RefParser.EscapeString( name, false ) : null;
		}

		internal static string ResolveElementName( DataPresenterReference rootRef, FieldLayout fieldLayout, bool normalized = false )
		{
			string name = ResolveName( rootRef, fieldLayout );

			return null != name ? RefParser.EscapeString( name, false ) : null;
		}

		internal static string ResolveElementName( SummaryDefinition summary, bool normalized = false )
		{
			string name = ResolveName( summary );

			return null != name ? RefParser.EscapeString( name, false ) : null;
		}

		#endregion // ResolveElementName

		#region ResolveName

		internal static string ResolveName( Field field )
		{
			DataPresenterCalculationSettingsBase settings = field.CalculationSettings;
			string name = null != settings ? settings.ReferenceId : null;

			if ( string.IsNullOrEmpty( name ) )
				name = field.Name;

			return name;
		}

		internal static string ResolveName( DataPresenterReference rootRef, FieldLayout fieldLayout )
		{
			string refId = fieldLayout.CalculationReferenceId;

			if ( string.IsNullOrEmpty( refId ) )
				refId = fieldLayout.Description;

			if ( string.IsNullOrEmpty( refId ) )
				refId = rootRef.GetAutoGeneratedName( fieldLayout, true );

			return refId;
		}

		internal static string ResolveName( SummaryDefinition summary )
		{
			DataPresenterCalculationSettingsBase settings = summary.CalculationSettings;
			string name = null != settings ? settings.ReferenceId : null;

			if ( string.IsNullOrEmpty( name ) )
				name = summary.Key;

			return name;
		}

		#endregion // ResolveName

		#region ToCalcValue

		internal CalculationValue ToCalcValue( object val )
		{
			CultureInfo culture = CultureInfo.CurrentCulture;

			if ( null != _calcSettings )
			{
				var converter = _calcSettings.ValueConverter;
				var type = _calcSettings.TreatAsTypeResolved;

				// If a converter was specified on the control calc settings then use that.
				// 
				if ( null != converter )
				{
					val = converter.Convert( val, type ?? typeof( object ), _clientObject, culture );
				}
				// Otherwise if TreatAsType was specified then convert the control's value
				// tot that type using control's language.
				// 
				else if ( null != type )
				{
					val = CoreUtilities.ConvertDataValue( val, type, culture, null );
				}
			}

			return Utils.ToCalcValue( val );
		}

		#endregion // ToCalcValue

		#region VerifyName

		/// <summary>
		/// Re-verifies the Name based on ReferenceId and control's Name settings.
		/// </summary>
		private void VerifyName( bool reregisterIfChanged = true )
		{
			string newName = null;

			// Only applicable for field-layout.
			// 
			if ( null != _fieldLayoutReferenceIdTracker )
				newName = (string)_fieldLayoutReferenceIdTracker.Target;

			if ( string.IsNullOrEmpty( newName ) )
			{
				if ( _clientObject is Field )
					newName = ResolveName( (Field)_clientObject );
				else if ( _clientObject is SummaryDefinition )
					newName = ResolveName( (SummaryDefinition)_clientObject );
			}

			if ( _name != newName )
			{
				_name = newName;

				_elementName = null != _name ? RefParser.EscapeString( _name, false ) : null;

				if ( reregisterIfChanged )
					this.RegisterWithCalcManager( true );
			}
		}

		#endregion // VerifyName

		#region VerifyFormula

		/// <summary>
		/// Called when the Formula is changed on the named reference or the control. It will re-compile and
		/// associate the new formula with the reference while deleting the old formula from the calc engine.
		/// </summary>
		internal void VerifyFormula( bool reregisterIfChanged = true )
		{
			string formula = null != _calcSettings ? _calcSettings.Formula : null;

			if ( _formula != formula )
			{
				_formula = formula;

				if ( reregisterIfChanged )
				{
					if ( null != _calcReference && !_calcReferenceDirty )
					{
						// Re-register the formula. EnsureFormulaRegistered method will take the necessary steps to
						// remove the formula if the passed in formula string is null or empty. If the calc reference
						// hasn't been created yet then we don't need to take any action since when it does get 
						// created, it will register the formula properly.
						// 
						DataPresenterFormulaRefBase formulaRef = _calcReference as DataPresenterFormulaRefBase;
						Debug.Assert( null != formulaRef );
						if ( null != formulaRef )
							formulaRef.EnsureFormulaRegistered( _formula );
					}
				}

				// Raise Formula property changed. IFormulaProvider also implements INotifyPropertyChanged.
				// 
				this.RaisePropertyChangedEvent( "Formula" );
			}
		}

		#endregion // VerifyFormula

		#region UnregisterFromCalcManager

		/// <summary>
		/// Removes the calc reference from the calc manager along with its formula if any.
		/// </summary>
		internal void UnregisterFromCalcManager( )
		{
			if ( _rootRef.IsValid && null != _calcReference )
			{
				var oldReference = _calcReference;
				_calcReference = null;

				// Remove the old reference.
				// 
				if ( null != oldReference && null != _calcManager )
					_calcManager.InternalRemoveReference( oldReference );

				this.RaisePropertyChangedEvent( "Reference" );
			}
		}

		#endregion // UnregisterFromCalcManager

		#endregion // Methods

		#region IFormulaProvider Interface Implementation

		#region CalculationManager

		ICalculationManager IFormulaProvider.CalculationManager
		{
			get
			{
				return _rootRef.CalcManager;
			}
		}

		#endregion // CalculationManager

		#region Formula

		string IFormulaProvider.Formula
		{
			get
			{
				return _formula;
			}
			set
			{
				if ( null != _calcSettings )
				{
					_calcSettings.Formula = value;
				}
				else
				{
					Field field = _clientObject as Field;
					if ( null != field )
					{
						Debug.Assert( null == field.CalculationSettings );
						field.CalculationSettings = new FieldCalculationSettings( )
						{
							Formula = value
						};
					}
					else
					{
						SummaryDefinition summary = _clientObject as SummaryDefinition;
						if ( null != summary )
						{
							Debug.Assert( null == summary.CalculationSettings );
							summary.CalculationSettings = new SummaryCalculationSettings( )
							{
								Formula = value
							};
						}
						else
							Debug.Assert( false );
					}
				}
			}
		}

		#endregion // Formula

		#region Participant

		ICalculationParticipant IFormulaProvider.Participant
		{
			get
			{
				return _rootRef._adapter;
			}
		}

		#endregion // Participant

		#region Reference

		ICalculationReference IFormulaProvider.Reference
		{
			get
			{
				return _calcReference;
			}
		}

		#endregion // Reference

		#endregion // IFormulaProvider Interface Implementation
	}

	#endregion // ReferenceManager Class

	#region DataPresenterFormulaRefBase Class

	/// <summary>
	/// Base class for references that have formula like field or summary references.
	/// </summary>
	internal abstract class DataPresenterFormulaRefBase : DataPresenterRefBase
	{
		#region Private Variables

		private ICalculationFormula _formula;
		internal int _valueDirtyCounter = 0;
		internal int _visibleCounter = 0;

		// SSP 6/24/05 BR04577
		// If dummyFormula is true then this summary is using a formula strictly to emulate
		// dependancy chaining of non-formula summaries. If a non-summary formula is referenced
		// by a formula whenever the source column of the summary is changed, this formula needs
		// to be added to the calc chain. For example, B() = A (non-formula), C = B(). When A
		// changes, B is considered dirty and C needs to be enqued into the re-calc chain. A 
		// dummy formula is used for this.
		// 
		internal bool _dummyFormula = false;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal DataPresenterFormulaRefBase( DataPresenterReference rootRef, string elementName, object context )
			: base( rootRef, elementName, context )
		{
		}

		#endregion // Constructor

		#region HasFormulaSyntaxError

		/// <summary>
		/// Returns true if the reference has a formula and the formula has a syntax error.
		/// </summary>
		internal bool HasFormulaSyntaxError
		{
			get
			{
				return null != this.Formula && this.Formula.HasSyntaxError;
			}
		}

		#endregion // HasFormulaSyntaxError

		#region FormulaSyntaxError

		/// <summary>
		/// Returns the syntax error string if there is a syntax error otherwise returns null.
		/// </summary>
		internal string FormulaSyntaxError
		{
			get
			{
				return this.HasFormulaSyntaxError ? this.Formula.SyntaxError : null;
			}
		}

		#endregion // FormulaSyntaxError

		#region EnsureFormulaRegistered

		/// <summary>
		/// Makes sure that this reference is using the specified formula string as
		/// its formula. If not it will delete the old formula and compile and add
		/// the new formula.
		/// </summary>
		/// <param name="formulaString"></param>
		internal void EnsureFormulaRegistered( string formulaString )
		{
			if ( null == _formula || formulaString != _formula.FormulaString )
				this.RegisterFormulaHelper( formulaString );
		}

		#endregion // EnsureFormulaRegistered

		#region RegisterFormulaHelper

		/// <summary>
		/// Removes the old formula from the calc network and adds the new formula to 
		/// the calc network.
		/// </summary>
		/// <param name="newFormulaString"></param>
		internal void RegisterFormulaHelper( string newFormulaString )
		{
			ICalculationManager calcManager = this.CalcManager;

			if ( null != calcManager )
			{
				// Remove the old formula first.
				//
				if ( null != _formula )
				{
					// SSP 2/4/05
					// Use the AddFormulaHelper and RemoveFormulaHelper utility methods 
					// instead. See comments above those method definitions for more info.
					//
					//calcManager.RemoveFormula( this.formula );
					// MD 1/28/09 - TFS13109
					// We need to clear the formula before calling RemoveFormulaHelper. If we don't, there is a weird timing issue with synchronous 
					// calculations where removing the formula tries to recalculate it and it causes a null reference exception.
					//RefUtils.RemoveFormulaHelper( calcManager, this.formula );
					ICalculationFormula tempFormula = _formula;
					_formula = null;
					RefUtils.RemoveFormulaHelper( this, tempFormula );
				}

				// SSP 6/24/05 BR04577
				// If dummyFormula is true then this summary is using a formula strictly to emulate
				// dependancy chaining of non-formula summaries. If a non-summary formula is referenced
				// by a formula whenever the source column of the summary is changed, this formula needs
				// to be added to the calc chain. For example, B() = A (non-formula), C = B(). When A
				// changes, B is considered dirty and C needs to be enqued into the re-calc chain. A 
				// dummy formula is used for this.
				// 
				// ------------------------------------------------------------------------------------
				SummaryDefinitionReference summaryRef = this as SummaryDefinitionReference;
				if ( null != summaryRef )
				{
					// SSP 12/28/05 BR08479
					// Moved this from inside the below if block. 
					// Reset the dummyFormula flag.
					// 
					summaryRef._dummyFormula = false;

					if ( !summaryRef.IsDisposedReference
						&& ( null == newFormulaString || 0 == newFormulaString.Length ) )
					{
						// SSP 12/28/05 BR08479
						// Moved this above, outside of this if block.
						// 
						





						FieldReference fieldRef = (FieldReference)summaryRef._fieldLayoutReference.GetFieldReference( summaryRef.Summary.SourceFieldName, false );
						string sourceFieldRefName = null != fieldRef ? fieldRef.ElementName : null;
						if ( !string.IsNullOrEmpty( sourceFieldRefName ) )
						{
							string dummyFormula = "COUNT( [" + sourceFieldRefName + "] )";

							// Set the dummyFormula to true to indicate that the formula results should be
							// discarded.
							// 
							summaryRef._dummyFormula = true;
							newFormulaString = dummyFormula;
						}
					}
				}
				// ------------------------------------------------------------------------------------

				if ( null != newFormulaString && newFormulaString.Length > 0 )
				{
					_formula = calcManager.CompileFormula( this, newFormulaString, false );

					// SSP 2/4/05
					// Use the AddFormulaHelper and RemoveFormulaHelper utility methods 
					// instead. See comments above those method definitions for more info.
					//
					//calcManager.AddFormula( formula );
					RefUtils.AddFormulaHelper( this, _formula );
				}
			}
		}

		#endregion // RegisterFormulaHelper

		#region RefBase Overrides

		#region Formula

		public override ICalculationFormula Formula
		{
			get
			{
				return _formula;
			}
		}

		#endregion // Formula

		#region HasFormula

		/// <summary>
		/// Indicates if the reference has a formula. This property returns true even if 
		/// the formula has a syntax error.
		/// </summary>
		internal bool HasFormula
		{
			get
			{
				return null != this.Formula;
			}
		}

		#endregion // HasFormula

		#region CreateReference

		/// <summary>
		/// A helper method that parses out relative column(*) or summary() names, optionally preceded
		/// by series of "../" relative parent specifiers. This is to support [../../Summary()]
		/// and [../../column1(*)] types of references to allow for being able to refer to the parent 
		/// group's summary or cells. For example, in a group-by situation where customer rows are 
		/// grouped by State and City columns. Let's say you have a summary with formula 
		/// Total() = sum([column1]). A column formula of [Total()] refers to the total of the current 
		/// city. A column formula of [../../Total()] referes to the total of the current state. Notice 
		/// how we are using two sets of double dots; this is because one set has no meaning (as
		/// was decided by the Melville team). In other words [Total()] and [../Total()] refer
		/// to the same summary and this is consistant with how [column1] and [../column1] refer
		/// to the same column in a column formula. The same logic as summaries applies to column(*)
		/// references too. Examples of anologous column references are [column1(*)], [../../column1(*)]. 
		/// [column1(*)] reference resolves to all the column1 cells in the current group
		/// of rows, current City in our example. [../../column1(*)] resolves to all the cells
		/// in the current State, which is the parent group-by column.
		/// </summary>
		/// <param name="referenceName"></param>
		/// <returns></returns>
		private RefBase CreateReferenceHelper( string referenceName )
		{
			Debug.Assert( this is FieldReference || this is SummaryDefinitionReference );

			FieldLayout fieldLayout = this.FieldLayoutContext;
			FieldLayoutReference fieldLayoutReference = null != fieldLayout ? (FieldLayoutReference)_rootReference.GetFieldLayoutReference( fieldLayout, false ) : null;
			Debug.Assert( null != fieldLayoutReference );
			if ( null == fieldLayoutReference )
				return null;

			System.Text.RegularExpressions.Match match =
				System.Text.RegularExpressions.Regex.Match(
					referenceName, @"^(\.\.\/)*([^\/\(]+)(\(\s*(\*?)\s*\))?$" );

			if ( null != match && match.Success )
			{
				string name = match.Groups[2].Value;

				// Determine if the reference is a summary or a column.
				// Something(*) and Something are column references where as Something()
				// is a summary reference.
				//
				bool isSummary = 0 != match.Groups[3].Value.Length && 0 == match.Groups[4].Value.Length;
				bool isColumnWithSubStar = !isSummary && "*" == match.Groups[4].Value;

				if ( isSummary )
				{
					SummaryDefinitionReference summaryReference = (SummaryDefinitionReference)fieldLayoutReference.GetSummaryDefinitionReference( name, false );
					if ( null != summaryReference )
					{
						// The following code is for maintaining the same group-by level summary tokens. 
						// For example, with State and City group-by columns, s1() = [s2()] formula
						// is associated with the root level, State/s1() = [State/s2()] is associated
						// with the second level, and State/City/s1() = [State/City/s2()] is associated
						// with the city level.
						//
						GroupLevelSummaryDefinitionReference glsr = this as GroupLevelSummaryDefinitionReference;
						return null != glsr
							? fieldLayoutReference.GetSummaryDefinitionReference( summaryReference.Summary, glsr.GroupByField )
							: summaryReference;
					}
				}
				else if ( isColumnWithSubStar )
				{
					FieldReference fieldReference = (FieldReference)fieldLayoutReference.GetFieldReference( name, false );
					if ( null != fieldReference )
						return fieldReference;
				}
			}

			return null;
		}

		// SSP 9/8/04
		// Overrode CreateReference method so from the formula base references we can return
		// column and summary formula base references from the CreateReference. This fixes
		// the problem in Connect/Reconnect code of the calc engine where a token referring
		// to a column reference gets disconnected and reconnected to a NestedFieldReference.
		//
		/// <summary>
		/// Gets the reference for the specified reference name. The reference name can be absolute or relative.
		/// </summary>
		/// <param name="referenceName"></param>
		/// <returns></returns>
		public override ICalculationReference CreateReference( string referenceName )
		{
			Debug.Assert( this is FieldReference || this is SummaryDefinitionReference );

			// We are going to treat "Column(*)" and "Summary()" referneces, optionally
			// preceded by series of "../", specially to support group-by stuff.
			//
			// SSP 9/13/04 UWC89
			// First attempt to see if the specified reference exists in the ancestor/sibling
			// bands. For example, with a reference of "../../Column 1", if Column 1 exists in
			// the parent band then we want to return that regardless of whether there is such
			// a column in this band and there are group-by columns in this band.
			// Also if this is a group level summary reference then call CreateReference on the
			// top level summary settings reference because that's the reference relative to 
			// which the user would've entered the relative references in the formula. For example,
			// in a group-by situation with State and City group-by columns in Orders band, 
			// //ultraGrid1/Customers/Orders/Total() == "../../CustomerID" should get the CustomerID
			// of the Customers band and not from the Orders band as far as the user is thinking of
			// when he enters the formula. As far the user sees it, it's relative to the Orders
			// band and not relative to //ultraGrid1/Customers/State/Total() or 
			// //ultraGrid1/Customers/State/City/Total().
			// 
			ICalculationReference createdReference = this is GroupLevelSummaryDefinitionReference
				? (ICalculationReference)( (GroupLevelSummaryDefinitionReference)this )._summaryReference.CreateReference( referenceName )
				: (ICalculationReference)base.CreateReference( referenceName );

			if ( null == createdReference || createdReference is CalculationReferenceError )
			{
				RefBase tmpReference = this.CreateReferenceHelper( referenceName );
				if ( null != tmpReference )
				{
					RefUnAnchored ru = new RefUnAnchored( tmpReference );
					ru.RelativeReference = new RefParser( referenceName );
					createdReference = ru;
				}
			}

			return createdReference;
		}

		#endregion // CreateReference

		#endregion // RefBase Overrides
	}

	#endregion // DataPresenterFormulaRefBase Class

	#region DataPresenterFormulaTargetRefBase Class

	/// <summary>
	/// Base class for reference objects that are targets of formula evaluation like cell reference and summary value reference.
	/// </summary>
	internal abstract class DataPresenterFormulaTargetRefBase : DataPresenterRefBase
	{
		#region Private Variables

		protected CalculationValue _formulaEvaluationResult;
		internal int _verifiedValueDirtyCounter = -1;

		// The formula ref class that contains this target ref. For example, a column reference
		// contains all the associated cell references. So in the case of a cell reference,
		// containingFormulaRef is the column reference.
		//
		private DataPresenterFormulaRefBase _containingFormulaRef = null;

		// This is associated with the FormulaCalculationErrorEventArgs.ErrorDisplayText property.
		//
		internal string _formulaCalculationErrorDisplayText = null;

		// This is associated with the FormulaCalculationErrorEventArgs.ErrorValue property.
		//
		internal object _formulaCalculatiorErrorStoreValue = null;

		internal int _verifiedRecalcVisibleVersion = -1;

		#endregion // Private Variables

		#region Constructor

		protected DataPresenterFormulaTargetRefBase( DataPresenterReference rootRef, string elementName, DataPresenterFormulaRefBase containingFormulaRef, object context )
			: base( rootRef, elementName, context )
		{
			CoreUtilities.ValidateNotNull( containingFormulaRef, "containingFormulaRef" );

			_containingFormulaRef = containingFormulaRef;

			// SSP 8/18/04
			// By default mark everything visible (that is it needs to be calculated). The
			// row iterator is the one that will return only the rows that are visible in the
			// scroll regions.
			// 
			this.RecalcVisible = true;
		}

		#endregion // Constructor

		#region ContainingFormulaRef

		/// <summary>
		/// Returns the associated FormulaRefBase instance.
		/// </summary>
		internal DataPresenterFormulaRefBase ContainingFormulaRef
		{
			get
			{
				return _containingFormulaRef;
			}
		}

		#endregion // ContainingFormulaRef

		#region FormulaEvaluationResult

		/// <summary>
		/// Returns the result of formula evaluation. If this summary doesn't have a formula
		/// associated with it, returns null.
		/// </summary>
		internal CalculationValue FormulaEvaluationResult
		{
			get
			{
				return _formulaEvaluationResult;
			}
		}

		#endregion // FormulaEvaluationResult

		#region HasFormulaSyntaxError

		/// <summary>
		/// Returns true if the associated formula has a syntax error.
		/// </summary>
		internal bool HasFormulaSyntaxError
		{
			get
			{
				return _containingFormulaRef.HasFormulaSyntaxError;
			}
		}

		#endregion // HasFormulaSyntaxError

		#region FormulaSyntaxError

		/// <summary>
		/// Returns the syntax error string if there is a syntax error otherwise returns null.
		/// </summary>
		internal string FormulaSyntaxError
		{
			get
			{
				return _containingFormulaRef.FormulaSyntaxError;
			}
		}

		#endregion // FormulaSyntaxError

		#region Formula

		public override ICalculationFormula Formula
		{
			get
			{
				return _containingFormulaRef.Formula;
			}
		}

		#endregion // Formula

		#region HasFormula

		/// <summary>
		/// Indicates if this reference has a formula.
		/// </summary>
		internal bool HasFormula
		{
			get
			{
				return null != this.Formula;
			}
		}

		#endregion // HasFormula

		#region IsValueDirty

		/// <summary>
		/// Returns true if the formula needs to be recalculated.
		/// </summary>
		internal bool IsValueDirty
		{
			get
			{
				return _verifiedValueDirtyCounter < _containingFormulaRef._valueDirtyCounter && this.HasFormula;
			}
		}

		#endregion // IsValueDirty

		#region Value

		public override CalculationValue Value
		{
			get
			{
				return _formulaEvaluationResult;
			}
			set
			{
				_formulaEvaluationResult = value;
				_verifiedValueDirtyCounter = _containingFormulaRef._valueDirtyCounter;

				// Reset the error display text and the error store value when we get a set
				// on the Value.
				//
				_formulaCalculationErrorDisplayText = null;
				_formulaCalculatiorErrorStoreValue = null;
				if ( null != value && value.IsError )
				{
					FormulaCalculationErrorEventArgs e = this.CalcManager.RaiseFormulaError( this, value.ToErrorValue( ), null );

					if ( null != e )
					{
						_formulaCalculatiorErrorStoreValue = e.ErrorValue;
						_formulaCalculationErrorDisplayText = e.ErrorDisplayText;
						if ( string.Empty == _formulaCalculationErrorDisplayText )
							_formulaCalculationErrorDisplayText = null;
					}
				}
			}
		}

		#endregion // Value

		// SSP 9/6/04
		// Overrode RecalcVisible. There is a problem with the canc-engine that needs to be
		// fixed. There shouldn't be any need to override this if that problem is fixed.
		//
		#region RecalcVisible

		public override bool RecalcVisible
		{
			get
			{
				if ( _verifiedRecalcVisibleVersion != _containingFormulaRef._visibleCounter )
					return true;

				return base.RecalcVisible;
			}
			set
			{
				_verifiedRecalcVisibleVersion = _containingFormulaRef._visibleCounter;

				base.RecalcVisible = value;
			}
		}

		#endregion // RecalcVisible
	}

	#endregion // DataPresenterFormulaTargetRefBase Class

	#region CellReference Class

	internal class CellReference : DataPresenterFormulaTargetRefBase
	{
		#region Private Variables

		internal readonly FieldReference _fieldReference;
		internal readonly DataRecordReference _recordReference;
		internal bool _inValueSet = false;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal CellReference( FieldReference fieldReference, DataRecordReference recordReference )
			: base( fieldReference._rootReference, fieldReference.ElementName, fieldReference, null )
		{
			_fieldReference = fieldReference;
			_recordReference = recordReference;
		}

		#endregion // Constructor

		#region Context

		public override object Context
		{
			get
			{
				DataRecord dr = _recordReference.Record as DataRecord;
				return null != dr ? dr.Cells[_fieldReference.Field] : null;
			}
		}

		#endregion // Context

		#region ParserInstanceVersion

		// Parsed references on rows, cells, rows collections and nested column references
		// have to be recreated whenever their absolute names change due to a row getting
		// inserted, deleted, the rows collection getting sorted or resynced. For example,
		// When the first row is deleted from a rows collection then the next row's absolute
		// name changes from //ultraGrid1/Customers(1) to //ultraGrid1/Customers(0). However
		// the parsed reference as returned by RefBase.ParsedReference property is still 
		// based on the old absolute name. So we need to recreate the parsed reference.
		// 
		internal override int ParserInstanceVersion
		{
			get
			{
				RecordCollectionBase records = _recordReference.Record.ParentCollection;
				RecordCollectionReference recordsRef = null != records ? _rootReference._utils.GetRecordCollectionReference( records ) : null;
				return null != recordsRef ? recordsRef.ParserInstanceVersion : 0;
			}
		}

		#endregion // ParserInstanceVersion

		#region FieldContext

		internal override Field FieldContext
		{
			get
			{
				return _fieldReference.Field;
			}
		}

		#endregion // FieldContext

		#region RefBase Overrides

		public override bool IsDataReference
		{
			get
			{
				return true;
			}
		}

		public DataRecordReference RecordReference
		{
			get
			{
				return _recordReference;
			}
		}

		public override RefBase BaseParent
		{
			get
			{
				return _recordReference;
			}
		}


		public override ICalculationReference FindItem( string name )
		{
			return _recordReference.FindItem( name );
		}

		public override ICalculationReference FindAll( string name )
		{
			return _recordReference.FindAll( name );
		}

		public override ICalculationReference FindSummaryItem( string name )
		{
			return _recordReference.FindSummaryItem( name );
		}

		public override ICalculationReference FindItem( string name, string index )
		{
			return _recordReference.FindItem( name, index );
		}

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return _recordReference.FindItem( name, index, isRelative );
		}

		#region ResolveReference

		public override ICalculationReference ResolveReference( ICalculationReference reference, ResolveReferenceType referenceType )
		{
			ICalculationReference tmp = _recordReference.ResolveReferenceHelper( reference, referenceType, _fieldReference );
			if ( null != tmp )
				return tmp;

			return base.ResolveReference( reference, referenceType );
		}

		#endregion // ResolveReference

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return _fieldReference.IsDisposedReference || _recordReference.IsDisposedReference;
			}
		}

		#endregion // IsDisposedReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return _recordReference.Record.GetHashCode( ) ^ _fieldReference.Field.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored instance then delegate the call to it.
			//
			if ( obj is RefUnAnchored )
				return obj.Equals( this );

			CellReference cellRef = obj as CellReference;
			return null != cellRef 
				&& _recordReference.Record == cellRef._recordReference.Record
				&& _fieldReference.Field == cellRef._fieldReference.Field;
		}

		#endregion // Equals

		#endregion // RefBase Overrides

		#region ICalculationReference Overrides

		public override CalculationValue Value
		{
			get
			{
				CalculationValue val = base.Value;
				if ( null != val && this.HasFormula )
					return val;

				return _fieldReference._referenceManager.ToCalcValue( _recordReference.Record.GetCellValue( _fieldReference.Field ) );
			}
			set
			{
				// MD 3/25/09 - TFS15830
				// Cache the original calc reference value because we may have to restore it later.
				CalculationValue originalValue = base.Value;

				base.Value = value;

				// SSP 1/17/05 BR01753
				// If the cell is disposed then don't bother setting the value.
				//
				if ( this.IsDisposedReference )
					return;

				bool isError;
				object newCellValue = _fieldReference._referenceManager.FromCalcValue( value, out isError );

				bool origInValueSet = this._inValueSet;
				DataRecordReference recordReference = this.RecordReference;
				bool origInRowCellReferenceValueSet = recordReference._inCellReferenceValueSet;
				this._inValueSet = true;
				recordReference._inCellReferenceValueSet = true;
				try
				{
					if ( isError )
					{
						// If an error value was specified in the FormulaCalculationError event then
						// use that otherwise use the FormulaErrorValue setting.
						//
						newCellValue = null != _formulaCalculatiorErrorStoreValue
							? _formulaCalculatiorErrorStoreValue
							: _fieldReference._referenceManager.FormulaErrorValue;
					}

					recordReference.Record.SetCellValue( _fieldReference.Field, newCellValue, true, false, true, null, false, true );
				}
				finally
				{
					// Reset the flags to their original values.
					//
					this._inValueSet = origInValueSet;
					recordReference._inCellReferenceValueSet = origInRowCellReferenceValueSet;
				}
			}
		}

		#endregion // ICalculationReference  Overrides
	}

	#endregion // CellReference Class

	#region FieldReference Class

	/// <summary>
	/// Reference implementation that represents a field.
	/// </summary>
	internal class FieldReference : DataPresenterFormulaRefBase
	{
		#region Private Variables

		internal readonly FieldLayoutReference _fieldLayoutReference;
		internal readonly ReferenceManager _referenceManager;
		private Field _field;
		private ItemCache<DataRecordReference, CellReference> _cellReferences;

		#endregion // Private Variables

		#region Constructor

		internal FieldReference( ReferenceManager referenceManager, string elementName, Field field )
			: base( referenceManager._fieldLayoutReference._rootReference, elementName, field )
		{
			_referenceManager = referenceManager;
			_fieldLayoutReference = referenceManager._fieldLayoutReference;
			_field = field;

			_cellReferences = new ItemCache<DataRecordReference, CellReference>(
				_rootReference,
				ii => new CellReference( this, ii )
			);
		}

		#endregion // Constructor

		#region Field

		internal Field Field
		{
			get
			{
				return _field;
			}
		}

		#endregion // Field

		#region RefBase Overrides

		#region BaseParent

		public override RefBase BaseParent
		{
			get
			{
				return _fieldLayoutReference;
			}
		}

		#endregion // BaseParent

		#region FindItem

		public override ICalculationReference FindItem( string name )
		{
			return this.BaseParent.FindItem( name );
		}

		#endregion // FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			return this.BaseParent.FindSummaryItem( name );
		}

		#endregion // FindSummaryItem

		#region FindItem

		public override ICalculationReference FindItem( string name, string index )
		{
			return this.FindItem( name );
		}

		#endregion // FindItem

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return this.FindItem( name );
		}

		#endregion // FindItem

		#region ScopedReferences

		public override ICalculationReferenceCollection ScopedReferences( RefParser scopeRP )
		{
			return new RefCellCollection( this, this, scopeRP );
		}

		#endregion // ScopedReferences

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return null == _field || _field.Index < 0;
			}
		}

		#endregion // IsDisposedReference

		#region References

		public override ICalculationReferenceCollection References
		{
			get
			{
				return this.ScopedReferences( this.ParsedReference );
			}
		}

		#endregion // References

		#region IsEnumerable

		public override bool IsEnumerable
		{
			get
			{
				return true;
			}
		}

		#endregion // IsEnumerable

		#region FindAll

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		#endregion // FindAll

		#endregion // RefBase Overrides

		// SSP 10/12/04
		// Overrode GetHashCode, Equals, ContainsReference and IsSubsetReference methods.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return _field.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored or NestedFieldReference then delegate the call to it.
			//
			if ( obj is RefUnAnchored || obj is NestedFieldReference )
				return obj.Equals( this );

			FieldReference fieldRef = obj as FieldReference;
			return null != fieldRef && _field == fieldRef._field;
		}

		#endregion // Equals

		#region ContainsReferenceHelper

		protected override bool ContainsReferenceHelper( ICalculationReference inReference, bool isProperSubset )
		{
			DataPresenterRefBase testRef = RefUtils.GetUnderlyingReference( inReference ) as DataPresenterRefBase;
			return null != testRef && this.FieldContext == testRef.FieldContext;
		}

		#endregion // ContainsReferenceHelper

		#region GetCellReference

		internal CellReference GetCellReference( DataRecordReference dataRecordRef, bool allocate = true )
		{
			return _cellReferences.GetItem( dataRecordRef, allocate );
		} 

		#endregion // GetCellReference
	}

	#endregion // FieldReference Class


	#region NestedReference Class

	/// <summary>
	/// Base class for nested references.
	/// </summary>
	internal abstract class NestedReference : DataPresenterRefBase
	{
		#region Private Vars

		private readonly RecordCollectionBase _scope;
		private DataPresenterRefBase _nestingReference;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal NestedReference( RecordCollectionBase scope, DataPresenterRefBase nestingReference, object context )
			: base( nestingReference._rootReference, nestingReference.ElementName, context )
		{
			CoreUtilities.ValidateNotNull( scope, "scope" );
			CoreUtilities.ValidateNotNull( nestingReference, "nestingReference" );

			_nestingReference = nestingReference;
			_scope = scope;
		}

		#endregion // Constructor

		#region Scope

		/// <summary>
		/// Returns the associated record collection that defines the scope of this nested reference.
		/// </summary>
		internal RecordCollectionBase Scope
		{
			get
			{
				return _scope;
			}
		}

		#endregion // Scope

		#region RecalcDeferred

		/// <summary>
		/// Returns true if the recalc deferred is on for this enumerable reference.
		/// </summary>
		public override bool RecalcDeferred
		{
			get
			{
				return _nestingReference.RecalcDeferred;
			}
		}

		#endregion // RecalcDeferred
	}

	#endregion // NestedReference Class

	#region NestedFieldLayoutReference Class

	/// <summary>
	/// Reference implementation that represents a nested field-layout.
	/// </summary>
	internal class NestedFieldLayoutReference : NestedReference
	{
		#region Private Variables

		internal readonly FieldLayoutReference _fieldLayoutRef;

		#endregion // Private Variables

		#region Constructor

		internal NestedFieldLayoutReference( RecordCollectionBase scope, FieldLayoutReference fieldLayoutReference )
			: base( scope, fieldLayoutReference, fieldLayoutReference.Context )
		{
			_fieldLayoutRef = fieldLayoutReference;
		}

		#endregion // Constructor

		#region FieldLayout

		/// <summary>
		/// Returns the associated field-layout.
		/// </summary>
		internal FieldLayout FieldLayout
		{
			get
			{
				return _fieldLayoutRef.FieldLayout;
			}
		}

		#endregion // FieldLayout

		#region RefBase Overrides

		#region BaseParent

		public override RefBase BaseParent
		{
			get
			{
				var scopeFl = this.Scope.FieldLayout;
				if ( scopeFl == this.FieldLayout || scopeFl == this.FieldLayout.ParentFieldLayout )
					return _rootReference._utils.GetRecordCollectionReference( this.Scope );
				else
				{

					var parentFLRef = _fieldLayoutRef.ParentFieldLayoutReference;
					return _rootReference._utils.GetNestedRecordsReference( this.Scope, parentFLRef ) as RefBase ?? parentFLRef;
				}
			}
		}

		#endregion // BaseParent

		#region FindItem

		public override ICalculationReference FindItem( string name )
		{
			return _fieldLayoutRef.FindItem_Nested( name, this.Scope );
		}

		#endregion // FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			ICalculationReference reference = _fieldLayoutRef.GetSummaryDefinitionReference( name, true );

			if ( !( reference is SummaryDefinitionReference ) )
				return reference;

			SummaryDefinitionReference summaryReference = (SummaryDefinitionReference)reference;

			return _rootReference._utils.GetNestedSummaryDefinitionReference( this.Scope, summaryReference );
		}

		#endregion // FindSummaryItem

		#region FindItem

		public override ICalculationReference FindItem( string name, string index )
		{
			ICalculationReference reference = this.FindItem( name );

			if ( reference is RefBase )
				return new RefUnAnchored( reference as RefBase );
			else
			{
				Debug.Assert( reference is CalculationReferenceError, "Unexpected reference type was returned." );
				return reference;
			}
		}

		#endregion // FindItem

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return this.FindItem( name );
		}

		#endregion // FindItem

		#region FindAll

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		#endregion // FindAll

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				RecordCollectionReference recordsRef = _rootReference._utils.GetRecordCollectionReference( this.Scope );

				return _fieldLayoutRef.IsDisposedReference 
					|| null == recordsRef || recordsRef.IsDisposedReference;
			}
		}

		#endregion // IsDisposedReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			int hashCode = this.FieldLayout.GetHashCode( );
			if ( !this.Scope.IsRootLevel )
				hashCode ^= this.Scope.GetHashCode( );

			return hashCode;
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored instance then delegate the call to it.
			//
			if ( obj is RefUnAnchored )
				return obj.Equals( this );

			NestedFieldLayoutReference nestedFLRef = obj as NestedFieldLayoutReference;
			if ( null != nestedFLRef )
				return this.FieldLayout == nestedFLRef.FieldLayout && this.Scope == nestedFLRef.Scope;

			FieldLayoutReference flRef = obj as FieldLayoutReference;
			return null != flRef && this.Scope.IsRootLevel && this.FieldLayout == flRef.FieldLayout;
		}

		#endregion // Equals

		#endregion // RefBase Overrides
	}

	#endregion // NestedFieldLayoutReference Class

	#region NestedFieldReference Class

	/// <summary>
	/// Reference implementation that represents a column within a particular rows collection.
	/// This enumerable references enumerates all the cells associated with the column that
	/// either belongs to the associated rows collection or descendants rows of the associated
	/// rows collection. Note: the column can be from the same band as the rows collection or
	/// from a descendant band. For example, //ultraGrid1/Customers(0)/Orders/OrderDetails/Total
	/// reference would be a nested column reference with rows collection of the child rows 
	/// collection of Customers(0) and the column of Total column of OrderDetails band.
	/// </summary>
	internal class NestedFieldReference : NestedReference
	{
		#region Private Variables

		internal readonly FieldReference _fieldRef;
		internal RecordCollectionBase _scopeParameter;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal NestedFieldReference( RecordCollectionBase scope, FieldReference fieldRef, bool forSummary )
			// SSP 10/29/04 - Formula Row Index Source
			// If the formula row index type is list index then use the top level row collection because
			// the list indexes go across all the group-by rows. Also for summary value evaluation do not
			// get the top level row collection because when evaluating summaries (like summing a column)
			// you want the sum to be the sum of cells in the current island rather than all the cells in
			// the top level row collection.
			//
			: base( forSummary || scope.FieldLayout != fieldRef.FieldLayoutContext || CalculationScope.FullUnsortedList != scope.FieldLayout.CalculationScopeResolved
					? scope : RefUtils.GetTopLevelRecordCollection( scope ), fieldRef, fieldRef.Context )
		{
			if ( RefUtils.IsDescendantOf( scope.FieldLayout, fieldRef.FieldLayoutContext ) )
				throw new ArgumentException( "Scope must be at a higher level than the field in the field-layout hierarchy." );

			_fieldRef = fieldRef;
			_scopeParameter = scope;
		}

		#endregion // Constructor

		#region ParserInstanceVersion

		// Parsed references on rows, cells, rows collections and nested column references
		// have to be recreated whenever their absolute names change due to a row getting
		// inserted, deleted, the rows collection getting sorted or resynced. For example,
		// When the first row is deleted from a rows collection then the next row's absolute
		// name changes from //ultraGrid1/Customers(1) to //ultraGrid1/Customers(0). However
		// the parsed reference as returned by RefBase.ParsedReference property is still 
		// based on the old absolute name. So we need to recreate the parsed reference.
		// 
		internal override int ParserInstanceVersion
		{
			get
			{
				RecordCollectionReference recordsRef = _rootReference._utils.GetRecordCollectionReference( this.Scope );

				return null != recordsRef ? recordsRef.ParserInstanceVersion : 0;
			}
		}

		#endregion // ParserInstanceVersion

		#region Field

		/// <summary>
		/// Returns the associated column.
		/// </summary>
		internal Field Field
		{
			get
			{
				return _fieldRef.Field;
			}
		}

		#endregion // Field

		#region RefBase Overrides

		#region BaseParent

		public override RefBase BaseParent
		{
			get
			{
				if ( this.Scope.FieldLayout == _fieldRef.FieldLayoutContext )
				{
					return _rootReference._utils.GetRecordCollectionReference( this.Scope );
				}
				else
				{
					return (RefBase)_rootReference._utils.GetNestedRecordsReference( this.Scope, _fieldRef._fieldLayoutReference );
				}
			}
		}

		#endregion // BaseParent

		internal RecordCollectionReference RecordsReference
		{
			get
			{
				return _rootReference._utils.GetRecordCollectionReference( this.Scope );
			}
		}

		#region FindItem

		public override ICalculationReference FindItem( string name )
		{
			return this.RecordsReference.FindItem( name );
		}

		#endregion // FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			return this.RecordsReference.FindSummaryItem( name );
		}

		#endregion // FindSummaryItem

		#region FindItem

		public override ICalculationReference FindItem( string name, string index )
		{
			return this.FindItem( name );
		}

		#endregion // FindItem

		#region FindAll

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		#endregion // FindAll

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return this.FindItem( name );
		}

		#endregion // FindItem

		#region ScopedReferences

		public override ICalculationReferenceCollection ScopedReferences( RefParser scopeRP )
		{
			return new RefCellCollection( this, this._fieldRef, scopeRP );
		}

		#endregion // ScopedReferences

		#region Formula

		public override ICalculationFormula Formula
		{
			get
			{
				return _fieldRef.Formula;
			}
		}

		#endregion // Formula

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return _fieldRef.IsDisposedReference
					|| null == this.RecordsReference || this.RecordsReference.IsDisposedReference;
			}
		}

		#endregion // IsDisposedReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			int hashCode = _fieldRef.Field.GetHashCode( );
			if ( ! this.Scope.IsRootLevel )
				hashCode ^= this.Scope.GetHashCode( );

			return hashCode;
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored instance then delegate the call to it.
			//
			if ( obj is RefUnAnchored )
				return obj.Equals( this );

			NestedFieldReference nestedFieldRef = obj as NestedFieldReference;
			if ( null != nestedFieldRef )
				return _fieldRef.Field == nestedFieldRef.Field && this.Scope == nestedFieldRef.Scope;

			FieldReference fieldRef = obj as FieldReference;
			return null != fieldRef && this.Scope.IsRootLevel && _fieldRef.Field == fieldRef.Field;
		}

		#endregion // Equals

		#endregion // RefBase Overrides

		#region Overrides of ICalculationReference

		#region References

		public override ICalculationReferenceCollection References
		{
			get
			{
				return this.ScopedReferences( this.ParsedReference );
			}
		}

		#endregion // References

		#region IsEnumerable

		public override bool IsEnumerable
		{
			get
			{
				return true;
			}
		}

		#endregion // IsEnumerable

		#endregion // ICalculationReference Overrides
	}

	#endregion // NestedFieldReference Class

	#region NestedSummaryDefinitionReference Class

	/// <summary>
	/// Reference implementation that represents a summary definition with 
	/// context of a record collection.
	/// </summary>
	internal class NestedSummaryDefinitionReference : NestedReference
	{
		#region Private Variables

		private SummaryDefinitionReference _summaryReference;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal NestedSummaryDefinitionReference( SummaryDefinitionReference summaryRef, RecordCollectionBase scope )
			: base( scope, summaryRef, summaryRef.Context )
		{
			_summaryReference = summaryRef;
		}

		#endregion // Constructor

		#region Formula

		public override ICalculationFormula Formula
		{
			get
			{
				return _summaryReference.Formula;
			}
		}

		#endregion // Formula

		#region SummaryReference

		/// <summary>
		/// Returns the associated summary reference object.
		/// </summary>
		internal SummaryDefinitionReference SummaryReference
		{
			get
			{
				return _summaryReference;
			}
		}

		#endregion // SummaryReference

		#region Summary

		/// <summary>
		/// Returns the associated summary object.
		/// </summary>
		internal SummaryDefinition Summary
		{
			get
			{
				return _summaryReference.Summary;
			}
		}

		#endregion // Summary

		#region RefBase Overrides

		public override string ElementName
		{
			get
			{
				return _summaryReference.ElementName;
			}
		}

		public override RefBase BaseParent
		{
			get
			{
				RecordCollectionBase scope = this.Scope;
				SummaryDefinition summary = this.Summary;
				if ( scope.FieldLayout == summary.FieldLayout )
				{
					if ( _summaryReference is GroupLevelSummaryDefinitionReference )
					{
						GroupLevelSummaryDefinitionReference groupLevelSummaryReference = _summaryReference as GroupLevelSummaryDefinitionReference;

						// MBS 3/19/09 - TFS13679
						// The fix to BR30242 here causes a series of issues by changing the tuples that created based on the BaseParent.
						// After discussing this with Sandip, we're not quite sure which scenario this optimization 
						// particularly addresses, but the time gained by changing a cell's value within multiple
						// group nestings (in my test, 3 GroupByRows) yielded a minimal gain with this fix, and 
						// re-sorting the entire grid only gained a couple seconds (out of around 30 total), so this
						// fix isn't really worth keeping in now since it breaks other customers.
						//
						#region Old Code
						//// SSP 5/2/08 BR30242
						//// Ideally we should be returning a reference that intersects the rows collection and
						//// the summary reference's parent however since we don't have any that represents that,
						//// simply return the the group-by level's parent reference unless rows collection 
						//// reference is more specific.
						//// ------------------------------------------------------------------------------------
						////return this.SummaryReference.BaseParent;
						//UltraGridColumn groupByCol = rows.GroupByColumn;
						//if ( null != groupByCol )
						//{
						//    UltraGridBand band = rows.Band;
						//    int rowsGroupLevelIndex = band.SortedColumns.GetColumnIndex( groupByCol );
						//    int refGroupLevelIndex = band.SortedColumns.GetColumnIndex( groupLevelSummaryReference.GroupByColumn );
						//    if ( rowsGroupLevelIndex >= 0 && rowsGroupLevelIndex < refGroupLevelIndex - 1 )
						//        return groupLevelSummaryReference.BaseParent;
						//}
						//// ------------------------------------------------------------------------------------
						#endregion //Old Code
						//
						return groupLevelSummaryReference.BaseParent;
					}

					return _rootReference._utils.GetRecordCollectionReference( scope );
				}
				else
					return _rootReference._utils.GetNestedRecordsReference( scope, _summaryReference._fieldLayoutReference ) as RefBase;
			}
		}

		private RecordCollectionReference RecordsReference
		{
			get
			{
				return _rootReference._utils.GetRecordCollectionReference( this.Scope );
			}
		}

		public override ICalculationReference FindItem( string name )
		{
			return this.RecordsReference.FindItem( name );
		}

		public override ICalculationReference FindSummaryItem( string name )
		{
			return this.RecordsReference.FindSummaryItem( name );
		}

		public override ICalculationReference FindItem( string name, string index )
		{
			return this.RecordsReference.FindItem( name, index );
		}

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return this.RecordsReference.FindItem( name, index, isRelative );
		}

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		public override ICalculationReferenceCollection ScopedReferences( RefParser scopeRP )
		{
			return new RefSummaryResultCollection( this, _summaryReference, scopeRP );
		}

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return _summaryReference.IsDisposedReference
					|| null == this.RecordsReference || this.RecordsReference.IsDisposedReference;
			}
		}

		#endregion // IsDisposedReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			int hashCode = this.Summary.GetHashCode( );
			if ( ! this.Scope.IsRootLevel )
				hashCode ^= this.Scope.GetHashCode( );

			return hashCode;
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored instance then delegate the call to it.
			//
			if ( obj is RefUnAnchored )
				return obj.Equals( this );

			NestedSummaryDefinitionReference nestedSummaryRef = obj as NestedSummaryDefinitionReference;
			if ( null != nestedSummaryRef )
				return this.Scope == nestedSummaryRef.Scope && _summaryReference.Equals( nestedSummaryRef._summaryReference );

			SummaryDefinitionReference summaryRef = obj as SummaryDefinitionReference;
			return null != summaryRef && this.Scope.IsRootLevel && _summaryReference.Equals( summaryRef );
		}

		#endregion // Equals

		#endregion // RefBase Overrides

		#region IUltraCalcReference Overrides

		public override ICalculationReferenceCollection References
		{
			get
			{
				return this.ScopedReferences( this.ParsedReference );
			}
		}

		public override bool IsEnumerable
		{
			get
			{
				return true;
			}
		}

		#endregion IUltraCalcReference Overrides
	}

	#endregion // NestedSummaryDefinitionReference Class

	#region RecordCollectionReference Class

	/// <summary>
	/// Reference implementation that represents a rows collection.
	/// </summary>
	internal class RecordCollectionReference : DataPresenterRefBase
	{
		#region Private Variables

		internal readonly RecordCollectionBase _records;
		internal readonly FieldLayoutReference _fieldLayoutReference;

		// Parsed references on rows, cells, rows collections and nested column references
		// have to be recreated whenever their absolute names change due to a row getting
		// inserted, deleted, the rows collection getting sorted or resynced. For example,
		// When the first row is deleted from a rows collection then the next row's absolute
		// name changes from //ultraGrid1/Customers(1) to //ultraGrid1/Customers(0). However
		// the parsed reference as returned by RefBase.ParsedReference property is still 
		// based on the old absolute name. So we need to recreate the parsed reference.
		// 
		private int _parserInstanceVersion;

		internal int _resyncEventCounter;

		// SSP 9/21/06 BR16000 - Optmization
		// 
		internal int _resyncEventSuspended;

		internal ItemCache<SummaryResult, SummaryResultReference> _summaryResults;

		#endregion // Private Variables

		#region Constructor

		internal RecordCollectionReference( FieldLayoutReference fieldLayoutReference, RecordCollectionBase records )
			: base( fieldLayoutReference._rootReference, fieldLayoutReference.ElementName, records )
		{
			_records = records;
			_fieldLayoutReference = fieldLayoutReference;

			_summaryResults = new ItemCache<SummaryResult, SummaryResultReference>( _rootReference,
				summaryResult =>
				{
					var sdRef = (SummaryDefinitionReference)_fieldLayoutReference.GetSummaryDefinitionReference( summaryResult.SummaryDefinition, false );
					Debug.Assert( null != sdRef );

					return null != sdRef ? new SummaryResultReference( this, sdRef, summaryResult ) : null;
				},
				nameRetriever: summaryResultRef => summaryResultRef.ElementName
			);
		}

		#endregion // Constructor

		#region BumpParserInstanceVersion

		internal void BumpParserInstanceVersion( )
		{
			_parserInstanceVersion++;
		}

		#endregion // BumpParserInstanceVersion

		#region ParserInstanceVersion

		// Parsed references on rows, cells, rows collections and nested column references
		// have to be recreated whenever their absolute names change due to a row getting
		// inserted, deleted, the rows collection getting sorted or resynced. For example,
		// When the first row is deleted from a rows collection then the next row's absolute
		// name changes from //ultraGrid1/Customers(1) to //ultraGrid1/Customers(0). However
		// the parsed reference as returned by RefBase.ParsedReference property is still 
		// based on the old absolute name. So we need to recreate the parsed reference.
		// 
		internal override int ParserInstanceVersion
		{
			get
			{
				int ret = _parserInstanceVersion;
				var records = _records;
				while ( null != records.ParentRecord )
				{
					records = records.ParentRecord.ParentCollection;
					var recordsReference = _rootReference._utils.GetRecordCollectionReference( records );
					ret += recordsReference._parserInstanceVersion;
				}

				return ret;
			}
		}

		#endregion // ParserInstanceVersion

		#region Records

		/// <summary>
		/// Returns the associated record collection.
		/// </summary>
		internal RecordCollectionBase Records
		{
			get
			{
				return _records;
			}
		}

		#endregion // Records

		#region FieldLayout

		/// <summary>
		/// Returns the associated field-layout.
		/// </summary>
		internal FieldLayout FieldLayout
		{
			get
			{
				return _records.FieldLayout;
			}
		}

		#endregion // FieldLayout

		#region FieldLayoutContext

		/// <summary>
		/// Returns the context of a field-layout if any. Cell, record, field, field-layout references etc... all
		/// have the context of a field-layout.
		/// </summary>
		internal override FieldLayout FieldLayoutContext
		{
			get
			{
				return this.FieldLayout;
			}
		}

		#endregion // FieldLayoutContext

		#region GroupByFieldReferenceName

		/// <summary>
		/// If the associated record collection is a group-by record collection then returns the
		/// reference name of the associated group-by field otherwise returns null.
		/// </summary>
		private string GroupByFieldReferenceName
		{
			get
			{
				Field field = _records.GroupByField;
				var fieldRef = null != field ? _fieldLayoutReference.GetFieldReference( field, false ) : null;
				return null != fieldRef ? fieldRef.ElementName : null;
			}
		}

		#endregion // GroupByFieldReferenceName

		#region IsGroupByRecords

		internal bool IsGroupByRecords
		{
			get
			{
				return _rootReference._utils.IsGroupByRecords( _records );
			}
		}

		#endregion // IsGroupByRecords

		#region RefBase Overrides

		#region BaseParent

		public override RefBase BaseParent
		{
			get
			{
				var parentRecord = _records.ParentRecord;
				return null != parentRecord
					? (RefBase)_rootReference._utils.GetRecordReference( parentRecord )
					: (RefBase)_rootReference;
			}
		}

		#endregion // BaseParent

		#region FindItem

		public override ICalculationReference FindItem( string name )
		{
			// SSP 3/23/12 TFS98845
			// 
			return _fieldLayoutReference.FindItem_Nested( name, this.Records );
			
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			return this.GetSummaryResultReference( name, true );
		}

		#endregion // FindSummaryItem

		#region FindItem

		public override ICalculationReference FindItem( string name, string index )
		{
			if ( _rootReference._utils.IsGroupByRecords( this.Records ) )
			{
				if ( RefParser.AreStringsEqual( this.GroupByFieldReferenceName, name, true ) )
				{
					var groupByRecord = _rootReference._utils.FindGroupByRecord( this.Records, index );
					if ( null == groupByRecord )
						return new CalculationReferenceError( name, string.Format( "Can not find reference with '{0}' criteria.", index ) );
					
					return _rootReference._utils.GetRecordReference( groupByRecord );
				}
			}

			return this.FindItem( name );
		}

		#endregion // FindItem

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			if ( _rootReference._utils.IsGroupByRecords( this.Records ) )
			{
				if ( RefParser.AreStringsEqual( this.GroupByFieldReferenceName, name, true ) )
				{
					if ( isRelative )
						return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_RelativeIndexInvalid" ) );

					return _rootReference._utils.GetRecordReference( this.Records, index, name );
				}
			}

			return this.FindItem( name );
		}

		#endregion // FindItem

		#region FindAll

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		#endregion // FindAll

		// SSP 9/5/04
		// Overrode ResolveReference.
		//
		#region ResolveReference

		private ICalculationReference ResolveReferenceHelperHelper( RefTuple referenceTuple, DataPresenterRefBase reference,
			ResolveReferenceType resolveReferenceType, RecordCollectionBase records, Record recordContext )
		{
			Debug.Assert( null == recordContext || recordContext.IsDataRecord );
			Debug.Assert( reference is FieldReference || reference is SummaryDefinitionReference );

			// Is the incoming reference being referenced via relative index or with a scope
			// of All, in which case we need to recalculate the whole field.
			//
			bool isMarked = referenceTuple.Marked;
			if ( !isMarked )
			{
				foreach ( RefTuple t in reference.ParsedReference )
				{
					if ( t.Marked )
					{
						isMarked = true;
						break;
					}
				}
			}

			// SSP 9/16/04 UWC114
			// If a tuple other than the last tuple has a relative index then return the column
			// reference itself. We could optimize this however typically indicates a relative 
			// reference from a different band. This would be rare.
			//
			if ( isMarked && !referenceTuple.Marked && ResolveReferenceType.LeftHandSide == resolveReferenceType )
				return reference;

			if ( reference is FieldReference )
			{
				FieldReference fieldRef = (FieldReference)reference;
				Field field = fieldRef.Field;

				ICalculationReference retVal = null;

				RefTuple.RefScope scopeResolved = ResolveReferenceType.LeftHandSide == resolveReferenceType
					&& isMarked ? RefTuple.RefScope.All : referenceTuple.Scope;

				switch ( scopeResolved )
				{
					// If no scope.
					//
					case RefTuple.RefScope.Any:
						retVal = null != recordContext && recordContext.FieldLayout == field.Owner
							? (ICalculationReference)_rootReference._utils.GetCellReference( _rootReference._utils.GetRecordReference( recordContext ) as DataRecordReference, fieldRef )
							: (ICalculationReference)_rootReference._utils.GetNestedFieldReference( records, fieldRef );
						break;
					// Is this a Column(*) reference (has scope of all).
					//
					case RefTuple.RefScope.All:
						retVal = _rootReference._utils.GetNestedFieldReference( records, fieldRef );
						break;
					case RefTuple.RefScope.RelativeIndex:
						{
							ICalculationReference recordRef = null != recordContext && recordContext.FieldLayout == field.Owner
								? _rootReference._utils.GetRecordRelativeReference( recordContext, referenceTuple.ScopeIndex, null )
								: _rootReference._utils.GetNestedFieldReference( records, fieldRef );

							if ( !( recordRef is DataRecordReference ) )
								return recordRef;

							retVal = _rootReference._utils.GetCellReference( (DataRecordReference)recordRef, fieldRef );
							break;
						}
					case RefTuple.RefScope.Index:
						{
							// MD 11/20/08 - TFS5843
							// The row reference returned might be a group by row, but if we are looking for a column reference,
							// we actually want a non-group by row, so we have to do a descendant search because we could have many 
							// levels of group-by nesting.
							//ICalculationReference rowRef = RefUtils.GetRowReference( rows, referenceTuple.ScopeIndex, null );
							//
							//if ( !(rowRef is RowReferenceBase ) )
							//    return rowRef;
							//
							//retVal = RefUtils.GetCellReference( ((RowReferenceBase)rowRef).Row, column );
							RefBase nestedFieldReference = (RefBase)_rootReference._utils.GetNestedFieldReference( records, fieldRef );

							int count = 0;
							foreach ( ICalculationReference childRef in nestedFieldReference.References )
							{
								if ( count == referenceTuple.ScopeIndex )
									return childRef;

								count++;
							}
							break;
						}
					case RefTuple.RefScope.Identifier:
						{
							ICalculationReference recordRef = _rootReference._utils.GetRecordReference( records, recordContext.FieldLayout, referenceTuple.ScopeID, null );

							if ( !( recordRef is DataRecordReference ) )
								return recordRef;

							retVal = _rootReference._utils.GetCellReference( ( (DataRecordReference)recordRef ), fieldRef );
							break;
						}
				}

				return retVal;
			}
			else
			{
				SummaryDefinitionReference summaryDefinitionRef = (SummaryDefinitionReference)reference;

				if ( ResolveReferenceType.LeftHandSide == resolveReferenceType )
					return _rootReference._utils.GetNestedSummaryDefinitionReference( RefUtils.GetTopLevelRecordCollection( records ), summaryDefinitionRef );

				RecordCollectionReference recordsRef = _rootReference._utils.GetRecordCollectionReference( records );

				return records.FieldLayout == summaryDefinitionRef.FieldLayoutContext 
					? recordsRef.GetSummaryResultReference( summaryDefinitionRef, true )
					: _rootReference._utils.GetNestedSummaryDefinitionReference( records, summaryDefinitionRef );
			}
		}


		/// <summary>
		/// Helper method.
		/// </summary>
		/// <param name="inReference">Reference to resolve.</param>
		/// <param name="referenceType">Reference type.</param>
		/// <param name="recordReferenceContext">Optional record context.</param>
		/// <returns></returns>
		internal ICalculationReference ResolveReferenceHelper( ICalculationReference inReference,
			ResolveReferenceType referenceType, RecordReferenceBase recordReferenceContext )
		{
			RefUnAnchored refUnAnchored = inReference as RefUnAnchored;
			DataPresenterRefBase reference = null != refUnAnchored
				? refUnAnchored.WrappedReference as DataPresenterRefBase
				: inReference as DataPresenterRefBase;

			// SSP 9/17/04
			// Added code to take care of range references in group-by situation. Added a new RangeReference
			// class for representing grid range references.
			//
			if ( reference is RangeReference )
			{
				RangeReference range = (RangeReference)reference;
				return range.GetResolvedRangeReference( this, recordReferenceContext );
			}

			RefParser relativeRP = null != refUnAnchored ? refUnAnchored.RelativeReference
				: ( null != reference ? reference.ParsedReference : null );

			// SSP 9/9/04
			// We are going to treat "Column(*)" and "Summary()" referneces optionally
			// preceded by series of "../" specially to support group-by stuff.
			//
			bool calledFromSummaryResultResolve = null == recordReferenceContext;

			RefTuple relativeRPLastTuple = null != relativeRP ? relativeRP.LastTuple : null;

			try
			{
				if ( null != relativeRPLastTuple
					&& ( reference is FieldReference || reference is SummaryDefinitionReference ) )
				{
					FieldLayout referenceFieldLayout = reference.FieldLayoutContext;

					// SSP 9/13/04 UWC96
					// If reference being resolved is from a different band than this rows collection then
					// relative reference specification doesn't apply.
					// Added the below if block.
					//
					if ( referenceFieldLayout != this.FieldLayout )
					{
						if ( RefUtils.IsDescendantOf( this.FieldLayout, referenceFieldLayout ) )
						{
							// If the reference' band is an ancestor band find the ancestor row 
							// corresponding to the ancestor band.
							//
							Record record = _records.ParentRecord;
							while ( null != record && referenceFieldLayout != record.FieldLayout )
								record = record.ParentRecord;

							Debug.Assert( null != record );

							ICalculationReference result = this.ResolveReferenceHelperHelper( relativeRPLastTuple, reference, referenceType, record.ParentCollection, record );

							if ( result is CalculationReferenceError )
								return new CalculationReferenceError( relativeRP.ToString( ), ( (CalculationReferenceError)result ).Message );

							return result;
						}
						else
						{
							// If the reference' band is a sibling, descendant of this band or a descendant of a 
							// a sibling band.
							//
							RecordCollectionBase records = null;

							if ( RefUtils.IsDescendantOf( referenceFieldLayout, this.FieldLayout ) )
							{
								// If the reference' band is a descendant band then get the appropriate rows 
								// collection based on whether there is a row context.
								//
								if ( null != recordReferenceContext )
								{
									FieldLayout tmpFieldLayout = referenceFieldLayout;
									while ( this.FieldLayout != tmpFieldLayout.ParentFieldLayout )
										tmpFieldLayout = tmpFieldLayout.ParentFieldLayout;

									Debug.Assert( null != tmpFieldLayout );
									records = RefUtils.GetImmediateChildRecordsMatchingFieldLayout( recordReferenceContext.Record, tmpFieldLayout );
									
									Debug.Assert( null != records );
								}
								else
								{
									records = this.Records;
								}
							}
							// SSP 2/17/05
							// If the inReference is from a different grid then return null which
							// will cause the caller to use the base implementation which will lead
							// to the same reference being returned. If the column is from a 
							// different 
							else if ( reference._rootReference != _rootReference )
							{
								return null;
							}
							else
							{
								// Here the reference band is not a descendant band or an ancestor band. So it
								// must be either a sibling band or a descendant of a sibling band. In that 
								// case find sibling band that the reference band is a trivial descendant of.
								//
								FieldLayout tmpFieldLayout = referenceFieldLayout;
								while ( this.FieldLayout.ParentFieldLayout != tmpFieldLayout.ParentFieldLayout )
									tmpFieldLayout = tmpFieldLayout.ParentFieldLayout;

								// If the reference' band is sibling, get the sibling rows collection.
								//
								records = RefUtils.GetImmediateChildRecordsMatchingFieldLayout(
									RefUtils.GetTopLevelRecordCollection( this.Records ).ParentDataRecord, tmpFieldLayout );
							}

							Debug.Assert( null != records );

							ICalculationReference result = this.ResolveReferenceHelperHelper( relativeRPLastTuple, reference, referenceType, records, null );

							if ( result is CalculationReferenceError )
								return new CalculationReferenceError( relativeRP.ToString( ), ( (CalculationReferenceError)result ).Message );

							return result;
						}
					}
					else
					{
						RecordCollectionBase records = this.Records;

						if ( reference is FieldReference
							&& ( RefTuple.RefScope.All == relativeRPLastTuple.Scope
								|| calledFromSummaryResultResolve && RefTuple.RefScope.Any == relativeRPLastTuple.Scope )
							|| reference is SummaryDefinitionReference )
						{
							// If the name was specified using a relative string like [summary()] or [../../summary()]
							// or [../../column] etc..., then in a group-by situation we want to get the proper rows
							// collection at the right level. Such relative references are valid from a cell as well
							// as a summary formula.
							//
							if ( null != relativeRP && relativeRP.IsRelative )
							{
								bool fistParentTupleSkipped = false;
								foreach ( RefTuple t in relativeRP )
								{
									if ( RefTuple.RefType.Parent == t.Type )
									{
										// Since the first set of double dots is reduntant, skip it. For example,
										// [../Col] and [Col] are the same references from since from a context
										// of a column or a summary (which are the only items that can have formulas)
										// ".." gets you to band and the band will look for the column in it's columns 
										// collection giving back the same column that a column in the band would
										// have found. In other words, column searches its siblings to find items
										// where as a band searches for its children to find items.
										//
										if ( !fistParentTupleSkipped )
										{
											fistParentTupleSkipped = true;
										}
										else
										{
											if ( null == records.ParentRecord )
												return new CalculationReferenceError( inReference.AbsoluteName, "Invalid reference." );

											records = records.ParentRecord.ParentCollection;
										}
									}
									else
										break;
								}

								// If there were too many sets of double-dots then retrun a reference error.
								//
								if ( records.FieldLayout != this.FieldLayout )
									return new CalculationReferenceError( relativeRP.ToString( ), "Invalid reference." );
							}
							else
							{
								// Here the reference name was specified using an absolute name.
								//

								// Take care of group-by level summary formulas.
								//
								if ( reference is GroupLevelSummaryDefinitionReference )
								{
									
									//
									GroupLevelSummaryDefinitionReference glsr = (GroupLevelSummaryDefinitionReference)reference;

									RecordCollectionBase tmpRecords = records;
									while ( tmpRecords.ParentRecord is GroupByRecord
										&& glsr.ParentReference.GroupByField != ( (GroupByRecord)tmpRecords.ParentRecord ).GroupByField )
										tmpRecords = tmpRecords.ParentRecord.ParentCollection;

									if ( null == tmpRecords.ParentRecord || !( tmpRecords.ParentRecord is GroupByRecord ) )
										return _rootReference._utils.GetNestedSummaryDefinitionReference( _records, (GroupLevelSummaryDefinitionReference)reference );

									records = tmpRecords;
								}
								else
								{
									// If a column or a summary is being referred to using the absolute name
									// like [//ultraGrid1/Customers/Freight] or [//ultraGrid1/Customers/Freight Total()]
									// then give the column or the summary of the overall rows collection.
									//
									records = RefUtils.GetTopLevelRecordCollection( records );
								}
							}
						}

						ICalculationReference result = this.ResolveReferenceHelperHelper( relativeRPLastTuple, reference, 
							referenceType, records, null != recordReferenceContext ? recordReferenceContext.Record : null );

						if ( result is CalculationReferenceError )
							return new CalculationReferenceError( relativeRP.ToString( ), ( (CalculationReferenceError)result ).Message );

						return result;
					}
				}
			}
			catch ( Exception )
			{
				return new CalculationReferenceError( relativeRP.ToString( ), "Invalid reference." );
			}

			return null;
		}

		public override ICalculationReference ResolveReference( ICalculationReference reference, ResolveReferenceType referenceType )
		{
			ICalculationReference tmp = this.ResolveReferenceHelper( reference, referenceType, null );
			if ( null != tmp )
				return tmp;

			return base.ResolveReference( reference, referenceType );
		}

		#endregion // ResolveReference

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return ! _rootReference._utils.IsStillValid( _records );
				
					// SSP 9/21/06 BR16000 
					// 
					//|| null != this.rows.ParentRow && this.rows.ParentRow.CalcReference.IsDisposedReference;
			}
		}

		#endregion // IsDisposedReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return _records.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored instance then delegate the call to it.
			//
			if ( obj is RefUnAnchored )
				return obj.Equals( this );

			RecordCollectionReference recordsRef = obj as RecordCollectionReference;
			return null != recordsRef && _records == recordsRef._records;
		}

		#endregion // Equals

		#region ContainsReferenceHelper

		// SSP 12/11/06 BR18268
		// 
		protected override bool ContainsReferenceHelper( ICalculationReference inReference, bool isProperSubset )
		{
			RefBase inRef = inReference as RefBase;
			DataPresenterRefBase gridRef = RefUtils.GetUnderlyingReference( inRef ) as DataPresenterRefBase;

			// SSP 10/1/04
			// Columns or bands are not subset of rows collections even though their absolute names
			// may say otherwise.
			//
			if ( isProperSubset && ( inRef is FieldReference || inRef is FieldLayoutReference ) )
				return false;

			if ( !isProperSubset )
			{
				if ( gridRef is FieldReference || gridRef is FieldLayoutReference )
				{
					// SSP 12/5/07 BR28652
					// Since it's Contains, we also have to check for descendant bands.
					// 
					// --------------------------------------------------------------------------------------------
					//return gridRef.BandContext == this.Band;
					FieldLayout thisFieldLayout = this.FieldLayout;
					FieldLayout gridRefFieldLayout = gridRef.FieldLayoutContext;
					return null != gridRefFieldLayout && null != thisFieldLayout && RefUtils.IsTrivialDescendantOf( gridRefFieldLayout, thisFieldLayout );
					// --------------------------------------------------------------------------------------------
				}
			}

			if ( gridRef is NestedSummaryDefinitionReference )
			{
				return RefUtils.IsTrivialDescendantOf( ( (NestedSummaryDefinitionReference)gridRef ).Scope, this.Records );
			}

			return base.ContainsReferenceHelper( inReference, isProperSubset );
		}

		#endregion // ContainsReferenceHelper

		// MD 6/25/12 - TFS113177
		#region ShouldFormulaEditorIncludeIndex

		/// <summary>
		/// Gets the value indicating whether the formula editor should include default indexes after this reference's address when 
		/// enumerable references are used where a single value is expected.
		/// </summary>
		public override bool ShouldFormulaEditorIncludeIndex
		{
			get { return true; }
		}

		#endregion // ShouldFormulaEditorIncludeIndex

		#endregion // RefBase Overrides

		#region GetSummaryReference

		internal ICalculationReference GetSummaryResultReference( SummaryDefinitionReference summaryDefinitionReference, bool returnErrorRef )
		{
			ICalculationReference retVal = null;

			var summaryResult = _records.SummaryResults.GetItem( summaryDefinitionReference._summary );
			if ( null != summaryResult )
				retVal = _summaryResults.GetItem( summaryResult, true );

			if ( null == retVal && returnErrorRef )
				retVal = new CalculationReferenceError( summaryDefinitionReference.ElementName, RefUtils.GetString( "LER_Calc_SummaryNotFound", ReferenceManager.ResolveName( summaryDefinitionReference._rootReference, this.FieldLayout ), summaryDefinitionReference.ElementName ) );

			return retVal;
		}

		internal ICalculationReference GetSummaryResultReference( string name, bool returnErrorRef )
		{
			var summaryRef = _summaryResults.GetItem( name );
			if ( null == summaryRef )
			{
				var summaryDefinitionRef = _fieldLayoutReference.GetSummaryDefinitionReference( name, false ) as SummaryDefinitionReference;
				summaryRef = this.GetSummaryResultReference( summaryDefinitionRef, false ) as SummaryResultReference;
			}

			if ( null == summaryRef && returnErrorRef )
				return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_SummaryNotFound", ReferenceManager.ResolveName( _rootReference, this.FieldLayout ), name ) );

			return summaryRef;
		} 

		#endregion // GetSummaryReference
	}

	#endregion // RecordCollectionReference Class


	#region RefRecordIterator Class

	internal class RefRecordIterator
	{
		private DataPresenterReference _rootRef;
		private FieldLayout _fieldLayout;
		private RefTuple _tuple;
		private RefTuple _childTuple;
		private RefRecordIterator _child;

		private RecordCollectionBase _records;
		private Record _record;

		private bool _recalcDeferredIterator;

		private bool _usedInRangeRecalc;
		private CalculationScope _calcScope = CalculationScope.Default;

		// SSP 11/9/04 UWC51
		// RefRecordIterator is being used for traversing summaries as well. If a row collection
		// contains no rows then before we were skipping the summaries of that row collection.
		// This is to fix that problem.
		//
		private bool _firstTime = true;

		internal static readonly IComparer<Record> g_listIndexComparer = CoreUtilities.CreateComparer<Record>(
			( x, y ) =>
			{
				DataRecord xx = x as DataRecord;
				DataRecord yy = y as DataRecord;
				if ( null != xx && null != yy )
					return xx.DataItemIndex.CompareTo( yy.DataItemIndex );

				Debug.Assert( false );
				return 0;
			}
		);

		private RefRecordIterator( FieldLayout fieldLayout, RefTuple tuple )
		{
			_fieldLayout = fieldLayout;
			_tuple = tuple;
			_calcScope = fieldLayout.CalculationScopeResolved;
		}

		internal static RefRecordIterator CreateRecordIterator( DataPresenterReference rootRef,
			RefParser rp, DataPresenterRefBase referenceBeingEnumerated )
		{
			RefUtils utils = rootRef._utils;

			// Since we are adding more than one LHS for summaries in group-by situation
			// we should only loop through the summaries of the LHS that's being enumerated.
			//			
			FieldLayout exemptFieldLayoutFromInsertingGroupByTuples = null;

			SummaryDefinitionReference summaryDefinitionRef = referenceBeingEnumerated as SummaryDefinitionReference;

			if ( null != summaryDefinitionRef )
				exemptFieldLayoutFromInsertingGroupByTuples = summaryDefinitionRef.Summary.FieldLayout;
			else
			{
				NestedSummaryDefinitionReference nestedSummaryDefinitionRef = referenceBeingEnumerated as NestedSummaryDefinitionReference;

				if ( null != nestedSummaryDefinitionRef )
					exemptFieldLayoutFromInsertingGroupByTuples = nestedSummaryDefinitionRef.Summary.FieldLayout;
			}

			// This is to support row index type of ListIndex in group-by mode.
			//
			bool usedInRangeRecalc = false;
			if ( referenceBeingEnumerated is FieldReference || referenceBeingEnumerated is NestedFieldReference )
			{
				Field field = referenceBeingEnumerated.FieldContext;
				if ( null != field && CalculationScope.FullUnsortedList == field.Owner.CalculationScopeResolved )
				{
					RecordCollectionBase records = referenceBeingEnumerated is FieldReference ? rootRef.RootRecords
						: ( (NestedFieldReference)referenceBeingEnumerated ).Scope;

					if ( null != records && ( records.FieldLayout != field.Owner || records.IsTopLevel ) )
						usedInRangeRecalc = true;
				}
			}

			List<RefTuple> list = utils.InsertGroupByFieldTuples( rp, exemptFieldLayoutFromInsertingGroupByTuples );

			if ( null == list || 0 == list.Count )
			{
				// AS 10/8/04
				// Don't throw an exception. This is currently called during the creation
				// of the refcell and summary enumerators.
				//
				//throw new Exception( string.Format( "Invalid reference: {0}", rp.ToString( ) ) );
				return null;
			}

			bool recalcDeferredIterator = referenceBeingEnumerated.RecalcDeferred;
			FieldLayout lastFieldLayout = null;
			RefTuple lastFieldLayoutTuple = null;
			RefRecordIterator rootIterator = null;
			RefRecordIterator lastFieldLayoutRecordIterator = null;

			// This flag keeps track of whether we've already created an iterator for this fieldLayout so
			// we don't create another one. The reason for having this is that in the case of the
			// group-by the fieldLayout tuple is repeated in the reference name.
			//
			bool lastFieldLayoutProcessed = false;

			foreach ( RefTuple tuple in list )
			{
				// See if any of the tuple in the reference requires that the iterator be not 
				// recalc deferred iterator.
				//
				recalcDeferredIterator = recalcDeferredIterator
					&& ( RefTuple.RefScope.Any == tuple.Scope || RefTuple.RefScope.Index == tuple.Scope );

				// SSP 10/15/04
				// Check for the scope type of the tuple. If it's summary then don't get the field.
				//
				
				FieldReference fieldRef = null != lastFieldLayout && RefTuple.RefScope.SummaryValue != tuple.Scope
					? utils.GetFieldReference( lastFieldLayout, tuple.Name, false ) as FieldReference : null;

				FieldLayoutReference fieldLayoutRef = RefTuple.RefScope.SummaryValue != tuple.Scope
					? rootRef.GetFieldLayoutReference( lastFieldLayout, tuple.Name, false ) as FieldLayoutReference : null;

				RefRecordIterator ri = null;

				// If the lastFieldLayout has not been initialized then that means the the first tuple should
				// be the root fieldLayout tuple. If the lastFieldLayout has been initialized and the current tuple 
				// is a chaptered field (which must be associated with the child of the last field-layout 
				// for the reference to be a valid reference) then get the associated child field-layout.
				//
				// SSP 3/23/12 TFS98845
				// 
				//if ( null == lastFieldLayout || null != fieldRef && utils.IsChaptered( fieldRef ) )
				if ( null == lastFieldLayout || null != fieldLayoutRef )
				{
					lastFieldLayout = rootRef.GetFieldLayout( lastFieldLayout, tuple.Name );
					lastFieldLayoutTuple = tuple;
					lastFieldLayoutProcessed = false;

					// If the fieldLayout has group-by columns, then ignore the first fieldLayout tuple
					// because we are repeating the fieldLayout tuple twice.
					//
					if ( !lastFieldLayout.HasGroupBySortFields )
					{
						ri = new RefRecordIterator( lastFieldLayout, tuple );
						lastFieldLayoutProcessed = true;
					}
				}
				else
				{
					// Create a child row iterator if we encounter a group-by or fieldLayout tuple.
					//
					if ( !lastFieldLayoutProcessed )
					{
						if ( RefTuple.RefScope.SummaryValue != tuple.Scope
							&& RefParser.AreStringsEqual( ReferenceManager.ResolveElementName( rootRef, lastFieldLayout ), tuple.Name, true ) )
						{
							lastFieldLayoutProcessed = true;
							ri = new RefRecordIterator( lastFieldLayout, tuple );
						}
						else if ( null != fieldRef && fieldRef.Field.IsGroupBy )
						{
							ri = new RefRecordIterator( lastFieldLayout, tuple );
						}
						else
						{
							// SSP 9/11/04
							// This is the case where the current tuple is something other than
							// the group-by field even though the fieldLayout has group-by columns. 
							// This could occur we are exmpting a fieldLayout from inserting group-by
							// tuples (see the beginning of the method for more info).
							//
							lastFieldLayoutProcessed = true;
							ri = new RefRecordIterator( lastFieldLayout, lastFieldLayoutTuple );
							ri._childTuple = tuple;
						}
					}
					else
					{
						lastFieldLayoutRecordIterator._childTuple = tuple;
					}
				}

				if ( null != ri )
				{
					ri._rootRef = rootRef;

					if ( null == rootIterator )
						rootIterator = ri;

					if ( null != lastFieldLayoutRecordIterator )
					{
						lastFieldLayoutRecordIterator._child = ri;
						lastFieldLayoutRecordIterator._childTuple = ri._tuple;
					}

					lastFieldLayoutRecordIterator = ri;
				}
			}

			lastFieldLayoutRecordIterator = rootIterator;
			while ( null != lastFieldLayoutRecordIterator )
			{
				lastFieldLayoutRecordIterator._usedInRangeRecalc = usedInRangeRecalc;

				lastFieldLayoutRecordIterator._recalcDeferredIterator = recalcDeferredIterator;
				lastFieldLayoutRecordIterator = lastFieldLayoutRecordIterator._child;
			}

			rootIterator._records = rootRef.RootRecords;

			return rootIterator;
		}

		public Record Current
		{
			get
			{
				return null != _child ? _child.Current : _record;
			}
		}

		// SSP 11/9/04 UWC51
		// RefRecordIterator is being used for traversing summaries as well. If a row collection
		// contains no rows then before we were skipping the summaries of that row collection.
		// This is to fix that problem.
		//
		/// <summary>
		/// Returns the current lowest level rows collection being traversed.
		/// </summary>
		public RecordCollectionBase CurrentRecordCollection
		{
			get
			{
				return null != _child ? _child.CurrentRecordCollection : _records;
			}
		}

		private List<Record> _recalcDeferredRecordsList;

		private int _recordIndexInRecordsSortedByListIndex = -1;
		private Record[] _recordsSortedByListIndex;

		private Record GetFirstRecord( )
		{
			CalculationScope calcScope = _calcScope;
			if ( CalculationScope.FullUnsortedList == calcScope )
			{
				if ( null == _child )
				{
					// If the row index type is list index and this iterator is being used in a range
					// recalc then we have to return rows in the same order as they are in the bound
					// list otherwise relative index calculations will be thrown off.
					//
					_recordsSortedByListIndex = null;
					_recordIndexInRecordsSortedByListIndex = -1;
					if ( _usedInRangeRecalc )
					{
						Record firstRecord = RefUtils.SafeItemAt( _records.ParentRecordManager.Unsorted, 0 );

						if ( null == firstRecord || ! RefUtils.IsDescendantOf( firstRecord, _records ) )
							return null;

						_recordsSortedByListIndex = _records.ParentRecordManager.Unsorted.ToArray( );
					}
					else if ( ! _rootRef._utils.IsGroupByRecords( _records ) )
					{
						_recordsSortedByListIndex = _records.ToArray( );
						CoreUtilities.SortMergeGeneric( _recordsSortedByListIndex, g_listIndexComparer );
					}

					if ( null != _recordsSortedByListIndex )
					{
						if ( _recordsSortedByListIndex.Length > 0 )
						{
							return _recordsSortedByListIndex[_recordIndexInRecordsSortedByListIndex = 0];
						}
						else
						{
							_recordsSortedByListIndex = null;
							return null;
						}
					}
				}

				// If this iterator iterates either the group-by records or is the lowest level record iterator
				// (as indicated by child iterator being null) then use the record index type of RecordIndex 
				// instead of ListIndex because for group-by records there are no list indeces and for 
				// non-lowest level iterators it doesn't make any difference.
				//
				calcScope = CalculationScope.FullSortedList;
			}

			return _rootRef._utils.GetRecord( _records, 0, calcScope );
		}

		private Record GetNextRecord( )
		{
			CalculationScope calcScope = _calcScope;
			if ( CalculationScope.FullUnsortedList == calcScope )
			{
				if ( null == _child && _record.IsDataRecord )
				{
					++_recordIndexInRecordsSortedByListIndex;
					if ( null == _recordsSortedByListIndex
						|| _recordIndexInRecordsSortedByListIndex >= _recordsSortedByListIndex.Length )
						return null;

					return _recordsSortedByListIndex[ _recordIndexInRecordsSortedByListIndex ];
				}

				// If this iterator iterates either the group-by records or is the lowest level record iterator
				// (as indicated by child iterator being null) then use the record index type of RecordIndex 
				// instead of ListIndex because for group-by records there are no list indeces and for 
				// non-lowest level iterators it doesn't make any difference.
				//
				calcScope = CalculationScope.FullSortedList;
			}

			int calcIndex = _rootRef._utils.GetRecordCalcIndex( _record, calcScope );
			return calcIndex >= 0 ? _rootRef._utils.GetRecord( _records, ++calcIndex, calcScope ) : null;
		}

		private List<Record> GetVisibleRecordsHelper( )
		{
			var records = ( from ii in _rootRef._adapter.VisibleRecords
				   let recordType = ii.RecordType
				   where RecordType.DataRecord == recordType || RecordType.GroupByField == recordType
				   select ii ).ToArray( );

			HashSet<Record> set = new HashSet<Record>( records );

			foreach ( var record in records )
			{
				var ii = record;
				while ( null != ii )
				{
					ii = ii.ParentRecord;
					if ( null != ii )
					{
						if ( set.Contains( ii ) )
							break;

						set.Add( ii );
					}
				}
			}

			
			var list = set.Where( ii => ii.ParentCollection == _records ).ToList( );

			CoreUtilities.SortMergeGeneric(
				list,
				CoreUtilities.CreateComparer<Record>(
					( x, y ) => 
					{
						return x.OverallSelectionPosition.CompareTo( y.OverallSelectionPosition );
					}
				)
			);

			return list;
		}

		public bool MoveNext( )
		{
			// SSP 11/19/04 BR00885
			// Moved this here from the beginning of the do loop below.
			//
			if ( null != _child && ! _firstTime && _child.MoveNext( ) )
				return true;

			do
			{
				// SSP 11/9/04 UWC51
				// RefRecordIterator is being used for traversing summaries as well. If a row collection
				// contains no rows then before we were skipping the summaries of that row collection.
				// This is to fix that problem.
				//
				// ------------------------------------------------------------------------------------
				//bool firstTime = null == this.row;
				bool firstTime = _firstTime;
				_firstTime = false;
				// ------------------------------------------------------------------------------------

				// SSP 11/19/04 BR00885
				// Moved this before the do loop starts.
				//
				//if ( null != this.child && ! firstTime && this.child.MoveNext( ) )
				//	return true;

				// SSP 8/16/04 - Recalc Deferred
				// Added the following if block and enclosed the existing code into the else block.
				//
				if ( false && _recalcDeferredIterator )
				{
					if ( firstTime )
					{
						_recalcDeferredRecordsList = this.GetVisibleRecordsHelper( );
					}

					do
					{
						// Get the next row from the visible rows collection.
						//
						int index = null != _record ? 1 + _recalcDeferredRecordsList.IndexOf( _record ) : 0;
						_record = RefUtils.SafeItemAt( _recalcDeferredRecordsList, index );

						// Keep doing so until the row has the right scope index. RecalcDeferred would
						// be true only for scopes of Any and Index. In the case of scope Any, all the 
						// rows match. In the case of the scope Index, match the row's index with the 
						// scope.
						//
					} while ( null != _record && RefTuple.RefScope.Index == _tuple.Scope
						&& _tuple.ScopeIndex != _rootRef._utils.GetRecordCalcIndex( _record ) );
				}
				else
				{
					RefTuple.RefScope scopeResolved = _tuple.Scope;

					if ( null != _childTuple )
					{
						if ( RefTuple.RefScope.All == _childTuple.Scope )
						{
							// If the child tupple has a scope of All (like in the reference [Price(*)])
							// Then traverse all rows even when the current tupple has a sepcified scope
							// index. 
							//
							scopeResolved = RefTuple.RefScope.All;
						}
						else if ( RefTuple.RefScope.SummaryValue == _childTuple.Scope && !firstTime )
						{
							// If the child is a summary result then break out after processing the first record
							// because summary results are associated with record collections and not individual
							// records.
							//
							_record = null;
							return false;
						}
					}

					switch ( scopeResolved )
					{
						case RefTuple.RefScope.Any:
						case RefTuple.RefScope.All:
							{
								if ( firstTime )
								{
									_record = this.GetFirstRecord( );

									// SSP 11/9/04 UWC51
									// RefRecordIterator is being used for traversing summaries as well. If a row collection
									// contains no rows then before we were skipping the summaries of that row collection.
									// This is to fix that problem.
									//
									// --------------------------------------------------------------------------------------
									if ( null == _record && null != _childTuple && RefTuple.RefScope.SummaryValue == _childTuple.Scope )
									{
										return true;
									}
									// --------------------------------------------------------------------------------------
								}
								else
									_record = this.GetNextRecord( );
							}
							break;

						case RefTuple.RefScope.Identifier:
							{
								// SSP 5/17/05
								// First time in the MoveNext we have to get the first row. Commented out the
								// original code and added the new code below.
								//
								// ------------------------------------------------------------------------------
								_record = firstTime ? this.GetFirstRecord( ) : this.GetNextRecord( );

								RefParser.NameValuePair[] pairs = null;
								if ( null != _record )
								{
									if ( _record is GroupByRecord )
									{
										Field groupByField = ( (GroupByRecord)_record ).GroupByField;
										string groupByFieldName = ReferenceManager.ResolveElementName( groupByField );

										pairs = new RefParser.NameValuePair[] 
										{ 
											new RefParser.NameValuePair( groupByFieldName, _tuple.ScopeID )
										};
									}
									else
									{
										string error;
										pairs = RefParser.ParseNameValuePairs( _tuple.ScopeID, out error );
										if ( null != error )
											throw new CalculationException( error );
									}
								}

								while ( null != _record && !_rootRef._utils.DoesRecordMatchCriteria( _record, pairs ) )
									_record = this.GetNextRecord( );

								
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

								// ------------------------------------------------------------------------------
							}
							break;

						case RefTuple.RefScope.Index:
							{
								if ( firstTime )
								{
									_record = _rootRef._utils.GetRecord( _records, _tuple.ScopeIndex );

									if ( CalculationScope.FullUnsortedList == _calcScope
										&& null != _record && _records != _record.ParentCollection )
										_record = null;
								}
								else
									_record = null;
							}
							break;

						default:
							throw new CalculationException( RefUtils.GetString( "LER_Calc_InvalidReference_InvalidScope", _tuple.ToString( ) ) );
					}
				}

				if ( null != _record && null != _child )
				{
					var childRecords = RefUtils.GetImmediateChildRecordsMatchingFieldLayout( _record, _child._fieldLayout );
					_child.InitRecordsCollection( childRecords );
				}

			} while ( null != _record && null != _child && !_child.MoveNext( ) );

			// Return true if we found a row. If we got here then it also means that 
			// the child iterator if any also found a row matching row.
			//
			return null != _record;
		}

		public void Reset( )
		{
			_record = null;

			// SSP 11/9/04 UWC51
			// RefRecordIterator is being used for traversing summaries as well. If a row collection
			// contains no rows then before we were skipping the summaries of that row collection.
			// This is to fix that problem.
			//
			_firstTime = true;
		}

		private void InitRecordsCollection( RecordCollectionBase records )
		{
			_records = records;
			this.Reset( );
		}
	}

	#endregion // RefRecordIterator Class

	#region RefCellCollection Class

	internal class RefCellCollection : ICalculationReferenceCollection
	{
		#region Private Variables

		private FieldReference _fieldRef;
		private RefParser _scopeRP;
		private DataPresenterRefBase _referenceBeingEnumerated;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="referenceBeingEnumerated"></param>
		/// <param name="fieldRef">The field whose cells to iterate.</param>
		/// <param name="scopeRP">Provides scope information.</param>
		public RefCellCollection( DataPresenterRefBase referenceBeingEnumerated, FieldReference fieldRef, RefParser scopeRP )
		{
			_referenceBeingEnumerated = referenceBeingEnumerated;
			_fieldRef = fieldRef;
			_scopeRP = scopeRP;
		}

		#endregion // Constructor

		#region IEnumerable

		public IEnumerator GetEnumerator( )
		{
			return new RefCellCollectionEnumerator( this );
		}

		#endregion //IEnumerable

		#region RefCellCollectionEnumerator

		private class RefCellCollectionEnumerator : IEnumerator
		{
			private RefCellCollection _collection;
			private DataRecord _iiRecord = null;
			private RefRecordIterator _recordIterator = null;

			internal RefCellCollectionEnumerator( RefCellCollection collection )
			{
				_collection = collection;
				_recordIterator = RefRecordIterator.CreateRecordIterator( 
					collection._referenceBeingEnumerated._rootReference, collection._scopeRP, collection._referenceBeingEnumerated );
			}

			public object Current
			{
				get
				{
					if ( null == _iiRecord )
						throw new InvalidOperationException( );

					return _collection._referenceBeingEnumerated._rootReference._utils.GetCellReference( _iiRecord, _collection._fieldRef );
				}
			}

			public bool MoveNext( )
			{
				if ( null == _recordIterator || ! _recordIterator.MoveNext( ) )
					return false;

				var current = _recordIterator.Current;
				Debug.Assert( null == current || current is DataRecord );
				_iiRecord = current as DataRecord;
				return null != _iiRecord;
			}

			public void Reset( )
			{
				_iiRecord = null;
				_recordIterator.Reset( );
			}
		}
		#endregion
	}

	#endregion // RefCellCollection Class

	#region RefSummaryResultCollection Class

	internal class RefSummaryResultCollection : ICalculationReferenceCollection
	{
		private SummaryDefinitionReference _summaryDefinitionReference;
		private SummaryDefinition _summaryDefinition;
		private RefParser _scopeRP;
		private DataPresenterRefBase _referenceBeingEnumerated;

		public RefSummaryResultCollection( DataPresenterRefBase referenceBeingEnumerated, 
			SummaryDefinitionReference summaryDefinitionReference, RefParser scopeRP )
		{
			_summaryDefinitionReference = summaryDefinitionReference;
			_summaryDefinition = _summaryDefinitionReference._summary;
			_referenceBeingEnumerated = referenceBeingEnumerated;
			_scopeRP = scopeRP;
		}

		#region IEnumerable

		public IEnumerator GetEnumerator( )
		{
			return new RefSummaryResultCollectionEnumerator( this );
		}

		#endregion //IEnumerable

		#region Implementation of IEnumerator

		private class RefSummaryResultCollectionEnumerator : IEnumerator
		{
			private RefSummaryResultCollection _collection;
			private RefRecordIterator _recordIterator;

			internal RefSummaryResultCollectionEnumerator( RefSummaryResultCollection collection )
			{
				_collection = collection;
				_recordIterator = RefRecordIterator.CreateRecordIterator( 
					_collection._summaryDefinitionReference._rootReference, 
					_collection._scopeRP, 
					_collection._referenceBeingEnumerated 
				);
			}

			public object Current
			{
				get
				{
					var currentRecords = _recordIterator.CurrentRecordCollection;
					var currentRecordsRef = _collection._summaryDefinitionReference._rootReference._utils.GetRecordCollectionReference( currentRecords );

					return currentRecordsRef.GetSummaryResultReference( _collection._summaryDefinitionReference, true );
				}
			}

			public bool MoveNext( )
			{
				return _recordIterator.MoveNext( );
			}

			public void Reset( )
			{
				_recordIterator.Reset( );
			}
		}

		#endregion
	}

	#endregion // RefSummaryResultCollection Class

	#region SummaryDefinitionReference Class

	/// <summary>
	/// Reference implementation that represents a summary definition.
	/// </summary>
	internal class SummaryDefinitionReference : DataPresenterFormulaRefBase
	{
		#region Private Variables

		internal readonly SummaryDefinition _summary;
		internal readonly ReferenceManager _referenceManager;
		internal readonly FieldLayoutReference _fieldLayoutReference;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal SummaryDefinitionReference( ReferenceManager referenceManager, string elementName, SummaryDefinition summary )
			: base( referenceManager._fieldLayoutReference._rootReference, elementName, summary )
		{
			_referenceManager = referenceManager;
			_fieldLayoutReference = referenceManager._fieldLayoutReference;
			_summary = summary;
		}

		#endregion // Constructor

		#region Summary

		/// <summary>
		/// Returns the associated summary object.
		/// </summary>
		internal SummaryDefinition Summary
		{
			get
			{
				return _summary;
			}
		}

		#endregion // Summary

		#region RefBase Overrides

		#region BaseParent

		public override RefBase BaseParent
		{
			get
			{
				return this.FieldLayoutReference;
			}
		}

		#endregion // BaseParent

		#region FieldLayoutContext

		internal override FieldLayout FieldLayoutContext
		{
			get
			{
				return _fieldLayoutReference.FieldLayoutContext;
			}
		} 

		#endregion // FieldLayoutContext

		#region FieldLayoutReference

		private FieldLayoutReference FieldLayoutReference
		{
			get
			{
				return _fieldLayoutReference;
			}
		}

		#endregion // FieldLayoutReference

		#region FindItem

		public override ICalculationReference FindItem( string name )
		{
			return _fieldLayoutReference.FindItem( name );
		}

		#endregion // FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			return _fieldLayoutReference.FindSummaryItem( name );
		}

		#endregion // FindSummaryItem

		#region FindItem

		public override ICalculationReference FindItem( string name, string index )
		{
			return _fieldLayoutReference.FindItem( name, index );
		}

		#endregion // FindItem

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return _fieldLayoutReference.FindItem( name, index, isRelative );
		}

		#endregion // FindItem

		#region FindAll

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		#endregion // FindAll

		#region ScopedReferences

		public override ICalculationReferenceCollection ScopedReferences( RefParser scopeRP )
		{
			return new RefSummaryResultCollection( this, this, scopeRP );
		}

		#endregion // ScopedReferences

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed. Read only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return null == _summary || ! _rootReference._utils.IsStillValid( _summary );
			}
		}

		#endregion // IsDisposedReference

		#endregion // RefBase Overrides

		#region ICalculationReference Overrides

		#region References

		public override ICalculationReferenceCollection References
		{
			get
			{
				return this.ScopedReferences( this.ParsedReference );
			}
		}

		#endregion // References

		#region IsRootSummary

		private bool IsRootSummary
		{
			get
			{
				return !( this is GroupLevelSummaryDefinitionReference ) && null == this.FieldLayoutContext.ParentFieldLayout;
			}
		}

		#endregion // IsRootSummary

		#region IsEnumerable

		public override bool IsEnumerable
		{
			get
			{
				// SSP 9/13/04
				// Return false from IsEnumerable for the root summary because there is a single
				// value associated with it anyways. This change was made because the 
				// //ultraGrid1/Customers/Total() summary reference should refer to the value of 
				// the root summary since there is only one. There is no need for this reference 
				// to be enumerable because if it were then the you would have to enclose it
				// in a SUM function to get the value externally from a text box like 
				// sum([//ultraGrid1/Customers/Total()]).
				//
				//return true;
				return !this.IsRootSummary;
			}
		}

		#endregion // IsEnumerable

		#region Value

		// SSP 9/13/04
		// Overrode the Value property. Now we are returning false from IsEnumerable for the root 
		// summary because there is a single value associated with it anyways. This change was 
		// made because the //ultraGrid1/Customers/Total() summary reference should refer to the 
		// value of the root summary since there is only one. There is no need for this reference 
		// to be enumerable because if it were then the you would have to enclose it in a SUM 
		// function to get the value externally from a text box like 
		// sum([//ultraGrid1/Customers/Total()]).
		//
		public override CalculationValue Value
		{
			get
			{
				if ( this.IsRootSummary )
				{
					RecordCollectionReference recordsRef = _rootReference._utils.GetRecordCollectionReference( _rootReference.RootRecords, true );
					SummaryResultReference summaryResultRef = (SummaryResultReference)recordsRef.GetSummaryResultReference( this, false );
					return null != summaryResultRef ? summaryResultRef.Value : null;
				}

				return base.Value;
			}
			set
			{
				if ( this.IsRootSummary )
				{
					RecordCollectionReference recordsRef = _rootReference._utils.GetRecordCollectionReference( _rootReference.RootRecords, true );
					SummaryResultReference summaryResultRef = (SummaryResultReference)recordsRef.GetSummaryResultReference( this, false );
					if ( null != summaryResultRef )
						summaryResultRef.Value = value;
				}
				else
				{
					base.Value = value;
				}
			}
		}

		#endregion // Value

		#region ResolveReference

		// SSP 11/1/04
		// Overrode ResolveReference.
		//
		public override ICalculationReference ResolveReference( ICalculationReference reference, ResolveReferenceType referenceType )
		{
			if ( this.IsRootSummary )
			{
				RecordCollectionReference recordsRef = _rootReference._utils.GetRecordCollectionReference( _rootReference.RootRecords, true );
				SummaryResultReference summaryResultRef = (SummaryResultReference)recordsRef.GetSummaryResultReference( this, false );
				if ( null != summaryResultRef )
					return summaryResultRef.ResolveReference( reference, referenceType );
			}

			return base.ResolveReference( reference, referenceType );
		}

		#endregion // ResolveReference

		// SSP 10/12/04
		// Overrode GetHashCode, Equals, ContainsReference and IsSubsetReference methods.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return _summary.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored or NestedSummaryDefinitionReference or 
			// GroupLevelSummaryDefinitionReference then delegate the call to it.
			//
			if ( obj is RefUnAnchored || obj is NestedSummaryDefinitionReference
				|| obj is GroupLevelSummaryDefinitionReference )
				return obj.Equals( this );

			SummaryDefinitionReference summaryRef = obj as SummaryDefinitionReference;
			return null != summaryRef && _summary == summaryRef._summary;
		}

		#endregion // Equals

		#endregion ICalculationReference Overrides
	}

	#endregion // SummaryDefinitionReference Class

	#region SummaryResultReference Class

	internal class SummaryResultReference : DataPresenterFormulaTargetRefBase
	{
		#region Private Variables

		private SummaryResult _summaryResult;
		private RecordCollectionReference _recordsReference;
		internal readonly SummaryDefinitionReference _summaryDefinitionReference;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal SummaryResultReference( RecordCollectionReference recordsReference, SummaryDefinitionReference summaryDefinitionReference, SummaryResult summaryResult )
			: base( recordsReference._rootReference, summaryDefinitionReference.ElementName, summaryDefinitionReference, summaryResult )
		{
			_summaryResult = summaryResult;
			_recordsReference = recordsReference;
			_summaryDefinitionReference = summaryDefinitionReference;
		}

		#endregion // Constructor

		#region SummaryResult

		/// <summary>
		/// Returns the associated summary result.
		/// </summary>
		internal SummaryResult SummaryResult
		{
			get
			{
				return _summaryResult;
			}
		}

		#endregion // SummaryResult

		#region FieldLayoutContext

		internal override FieldLayout FieldLayoutContext
		{
			get
			{
				return _summaryDefinitionReference.FieldLayoutContext;
			}
		} 

		#endregion // FieldLayoutContext

		#region ParserInstanceVersion

		// Parsed references on rows, cells, rows collections and nested column references
		// have to be recreated whenever their absolute names change due to a row getting
		// inserted, deleted, the rows collection getting sorted or resynced. For example,
		// When the first row is deleted from a rows collection then the next row's absolute
		// name changes from //ultraGrid1/Customers(1) to //ultraGrid1/Customers(0). However
		// the parsed reference as returned by RefBase.ParsedReference property is still 
		// based on the old absolute name. So we need to recreate the parsed reference.
		// 
		internal override int ParserInstanceVersion
		{
			get
			{
				return _recordsReference.ParserInstanceVersion;
			}
		}

		#endregion // ParserInstanceVersion

		#region RefBase Overrides

		public override bool IsDataReference
		{
			get
			{
				return true;
			}
		}

		public override RefBase BaseParent
		{
			get
			{
				return _recordsReference;
			}
		}

		public override ICalculationReference FindItem( string name )
		{
			return this.GetNestedFieldReferenceHelper( name );
		}

		public override ICalculationReference FindSummaryItem( string name )
		{
			return _recordsReference.GetSummaryResultReference( name, true );
		}

		private ICalculationReference GetNestedFieldReferenceHelper( string name )
		{
			ICalculationReference fieldRef = _rootReference._utils.GetNestedFieldReference( _recordsReference.Records, name );

			// It should be either a NestedFieldReference or a CalculationReferenceError.
			// 
			Debug.Assert( fieldRef is NestedFieldReference || fieldRef is CalculationReferenceError, "Unexpected nested field reference type." );

			return fieldRef;
		}

		public override ICalculationReference FindItem( string name, string index )
		{
			ICalculationReference fieldRef = this.GetNestedFieldReferenceHelper( name );
			NestedFieldReference nestedRef = fieldRef as NestedFieldReference;
			return null == nestedRef ? fieldRef : nestedRef.FindItem( name, index );
		}

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			ICalculationReference fieldRef = this.GetNestedFieldReferenceHelper( name );
			NestedFieldReference nestedRef = fieldRef as NestedFieldReference;
			return null == nestedRef ? fieldRef : nestedRef.FindItem( name, index, isRelative );
		}

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return _summaryDefinitionReference.IsDisposedReference || _recordsReference.IsDisposedReference;
			}
		}

		#endregion // IsDisposedReference

		#endregion // RefBase Overrides

		#region ICalculationReference Overrides

		public override CalculationValue Value
		{
			get
			{
				// SSP 6/24/05 BR04577
				// If dummyFormula is true then this summary is using a formula strictly to emulate
				// dependancy chaining of non-formula summaries. If a non-summary formula is referenced
				// by a formula whenever the source column of the summary is changed, this formula needs
				// to be added to the calc chain. For example, B() = A (non-formula), C = B(). When A
				// changes, B is considered dirty and C needs to be enqued into the re-calc chain. A 
				// dummy formula is used for this.
				// Enclosed the existing code into the if block. Don't return the calculated value when
				// using dummy formula.
				// 
				if ( !this.ContainingFormulaRef._dummyFormula )
				{
					CalculationValue val = base.Value;
					if ( null != (object)val && this.HasFormula )
						return val;
				}

				return _summaryDefinitionReference._referenceManager.ToCalcValue( this.SummaryResult.Value );
			}
			set
			{
				// SSP 6/24/05 BR04577
				// If dummyFormula is true then this summary is using a formula strictly to emulate
				// dependancy chaining of non-formula summaries. If a non-formula summary is referenced
				// by a formula whenever the source column of the summary is changed, this formula needs
				// to be added to the calc chain. For example, B() = A (non-formula), C = B(). When A
				// changes, B is considered dirty and C needs to be enqued into the re-calc chain. A 
				// dummy formula is used for this.
				// 
				if ( this.ContainingFormulaRef._dummyFormula )
					return;

				// SSP 6/10/05 BR04553
				// Fire SummaryResultChanged notification.
				// 
				// ------------------------------------------------------------------
				//base.Value = value;

				// Get the old value.
				//
				object oldVal = base.Value;

				// Set the new calc value by calling the base implementation.
				// 
				base.Value = value;

				// Get the new value. SummaryResult's Value property makes use of the 
				// calc value we just set.
				// 
				if ( !this.IsDisposedReference )
				{
					bool isError;
					object newValue = _summaryDefinitionReference._referenceManager.FromCalcValue( value, out isError );
					if ( isError )
					{
						// If an error value was specified in the FormulaCalculationError event then
						// use that otherwise use the FormulaErrorValue setting.
						//
						newValue = null != _formulaCalculatiorErrorStoreValue
							? _formulaCalculatiorErrorStoreValue
							: _summaryDefinitionReference._referenceManager.FormulaErrorValue;
					}

					this.SummaryResult.InternalSetNewValue( newValue );
				}
				// ------------------------------------------------------------------
			}
		}

		#region ResolveReference

		public override ICalculationReference ResolveReference( ICalculationReference reference, ResolveReferenceType referenceType )
		{
			if ( !( reference is RefBase ) ) return reference;

			ICalculationReference retRef = _recordsReference.ResolveReference( reference, referenceType );

			// SSP 10/29/04 - Formula Row Index Source
			// This is for formula row index source support.
			//
			NestedFieldReference nestedFieldRef = retRef as NestedFieldReference;
			if ( null != nestedFieldRef && null != nestedFieldRef._scopeParameter
				&& nestedFieldRef._scopeParameter != nestedFieldRef.Scope )
				return new NestedFieldReference( nestedFieldRef._scopeParameter, nestedFieldRef._fieldRef, true );

			return retRef;
		}

		#endregion // ResolveReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return _summaryResult.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored instance then delegate the call to it.
			//
			if ( obj is RefUnAnchored )
				return obj.Equals( this );

			SummaryResultReference summaryResultRef = obj as SummaryResultReference;
			return null != summaryResultRef && _summaryResult == summaryResultRef._summaryResult;
		}

		#endregion // Equals

		#endregion // ICalculationReference Overrides
	}

	#endregion // SummaryResultReference Class

	#region RecordReferenceBase Class

	/// <summary>
	/// Base class for RecordReference and GroupByRecordReference.
	/// </summary>
	internal abstract class RecordReferenceBase : DataPresenterRefBase
	{
		#region Private Variables

		internal readonly Record _record;
		internal readonly RecordCollectionReference _recordsReference;
		protected int _lastValidIndex = -1;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal RecordReferenceBase( RecordCollectionReference recordsReference, Record record )
			: base( recordsReference._rootReference, null, record )
		{
			CoreUtilities.ValidateNotNull( record );
			_record = record;
			_recordsReference = recordsReference;
		}

		#endregion // Constructor

		#region ParserInstanceVersion

		// Parsed references on rows, cells, rows collections and nested column references
		// have to be recreated whenever their absolute names change due to a row getting
		// inserted, deleted, the rows collection getting sorted or resynced. For example,
		// When the first row is deleted from a rows collection then the next row's absolute
		// name changes from //ultraGrid1/Customers(1) to //ultraGrid1/Customers(0). However
		// the parsed reference as returned by RefBase.ParsedReference property is still 
		// based on the old absolute name. So we need to recreate the parsed reference.
		// 
		internal override int ParserInstanceVersion
		{
			get
			{
				return _recordsReference.ParserInstanceVersion;
			}
		}

		#endregion // ParserInstanceVersion

		#region Record

		/// <summary>
		/// Returns the associated record.
		/// </summary>
		internal Record Record
		{
			get
			{
				return _record;
			}
		}

		#endregion // Record

		#region FieldLayoutContext

		/// <summary>
		/// Returns the context of a field-layout if any. Cell, record, field, field-layout references etc... all
		/// have the context of a field-layout.
		/// </summary>
		internal override FieldLayout FieldLayoutContext
		{
			get
			{
				return _record.FieldLayout;
			}
		}

		#endregion // FieldLayoutContext

		#region RefBase Overrides

		/// <summary>
		/// Returns false since record doesn't represent a data reference.
		/// </summary>
		public override bool IsDataReference
		{
			get
			{
				return false;
			}
		}

		public override ICalculationReference FindItem( string name )
		{
			return this.FindItemHelper( name, RefTuple.RefScope.Any, null );
		}

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItemHelper( name, RefTuple.RefScope.All, null );
		}

		public override ICalculationReference FindItem( string name, string index )
		{
			return this.FindItemHelper( name, RefTuple.RefScope.Identifier, index );
		}

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return this.FindItemHelper( name, isRelative ? RefTuple.RefScope.RelativeIndex : RefTuple.RefScope.Index, index );
		}

		public override ICalculationReference FindSummaryItem( string name )
		{
			return _recordsReference.FindSummaryItem( name );
		}

		/// <summary>
		/// Finds cell, column, or child records etc... based on the scope.
		/// </summary>
		/// <param name="name">Name to find.</param>
		/// <param name="scope">Can be Any, All, Index and RelativeIndex.</param>
		/// <param name="scopeValue">The value based on the scope type</param>
		internal abstract ICalculationReference FindItemHelper( string name, RefTuple.RefScope scope, object scopeValue );

		// SSP 8/9/04
		// Overrode ContainsReference to fix the problem where when a row got deleted,
		// the calc engine was adding the summary value to the recalc chain and then
		// removing it thinking that the summary reference was contained by the row
		// reference which is not the case for summaries that are from the same band.
		//
		/// <summary>
		/// Returns true if this reference contains inReference
		/// </summary>
		/// <param name="inReference">The contained candidate.</param>
		/// <returns>true if inReference is contained by this reference.</returns>
		public override bool ContainsReference( ICalculationReference inReference )
		{
			ICalculationReference inRef = RefUtils.GetUnderlyingReference( inReference );

			// Return false if the passed in reference is a summary reference.
			//
			if ( inRef is SummaryDefinitionReference
				|| inRef is SummaryResultReference && ( (SummaryResultReference)inRef ).FieldLayoutContext == this.FieldLayoutContext )
				return false;

			return base.ContainsReference( inReference );
		}

		#region ResolveReferenceHelper

		/// <summary>
		/// A helper method for resolving cell and summary references.
		/// </summary>
		/// <param name="inReference">Reference being resolved.</param>
		/// <param name="referenceType">Indicates whether the reference being resolved is the lvalue of the formula or an rvalue of the formula.</param>
		/// <param name="cellContext">Optional cell context.</param>
		/// <returns></returns>
		internal ICalculationReference ResolveReferenceHelper( ICalculationReference inReference, ResolveReferenceType referenceType, FieldReference cellContext )
		{
			RefBase inRef = inReference as RefBase;
			if ( null != inRef && !inRef.IsRange )
			{
				try
				{
					Field field = null;
					RefUnAnchored refUnanchored = inRef as RefUnAnchored;
					if ( null != refUnanchored )
						inRef = refUnanchored.WrappedReference;

					FieldReference fieldRef = inRef as FieldReference;
					if ( null == fieldRef && inRef is NestedFieldReference )
						fieldRef = ( (NestedFieldReference)inRef )._fieldRef;

					field = null != fieldRef ? fieldRef.Field : null;
					if ( null != field && field.Owner == _record.FieldLayout && _record.IsDataRecord )
					{
						// Also check to see that the requested reference doesn't have a scope of All,
						// for example a reference with * like [Field(*)].
						// 
						RefParser rp = null != refUnanchored ? refUnanchored.ParsedReference : inRef.ParsedReference;
						if ( null == rp || rp.TupleCount <= 0
							|| ( !rp.HasRelativeIndex && !rp.HasAbsoluteIndex
							&& !rp.LastTuple.Marked && RefTuple.RefScope.All != rp.LastTuple.Scope ) )
							return _rootReference._utils.GetCellReference( (DataRecordReference)this, fieldRef );
					}
				}
				catch ( Exception exc )
				{
					return new CalculationReferenceError( inRef.AbsoluteName, exc.Message );
				}

				return _recordsReference.ResolveReferenceHelper( inReference, referenceType, this );
			}

			return null;
		}

		#endregion // ResolveReferenceHelper

		#region ResolveReference

		// SSP 9/5/04
		// Overrode ResolveReference.
		//
		public override ICalculationReference ResolveReference( ICalculationReference reference, ResolveReferenceType referenceType )
		{
			ICalculationReference tmp = this.ResolveReferenceHelper( reference, referenceType, null );
			if ( null != tmp )
				return tmp;

			return base.ResolveReference( reference, referenceType );
		}

		#endregion // ResolveReference

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				// SSP 9/13/04
				// Return true from IsDisposedReference as a fix to the problem that when
				// the calc-engine goes to process the enqued Delete event, the row is disposed
				// and they don't dirty the necessary dependants that would have been affected.
				// This fix should work out fine since when the calc-engine processes Delete, 
				// it will disconnect any tokens referncing the row as well.
				//
				if ( !_record.IsStillValid
					// SSP 9/21/06 BR16000 
					// 
					|| _rootReference._utils.GetRecordCalcIndex( _record ) < 0 )
					return true;

				// MRS 11/18/2010 - TFS58092 (Also see BR16000)
				// 
				RecordReferenceBase parentRecordRef = null != _record.ParentRecord
					? _rootReference._utils.GetRecordReference( _record.ParentRecord, false )
					: null;
				if ( null != parentRecordRef && parentRecordRef.IsDisposedReference )
					return true;

				return false;
			}
		}

		#endregion // IsDisposedReference

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return _record.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored instance then delegate the call to it.
			//
			if ( obj is RefUnAnchored )
				return obj.Equals( this );

			RecordReferenceBase recordRef = obj as RecordReferenceBase;
			return null != recordRef && _record == recordRef._record;
		}

		#endregion // Equals

		#region ContainsReferenceHelper

		protected override bool ContainsReferenceHelper( ICalculationReference inReference, bool isSubset )
		{
			DataPresenterRefBase testRef = RefUtils.GetUnderlyingReference( inReference ) as DataPresenterRefBase;

			if ( testRef is CellReference )
				return RefUtils.IsTrivialDescendantOf( ( (CellReference)testRef )._recordReference.Record, _record );

			if ( testRef is FieldReference )
				return !isSubset && RefUtils.IsTrivialDescendantOf( testRef.FieldLayoutContext, this.FieldLayoutContext );

			return base.ContainsReferenceHelper( inReference, isSubset );
		}

		#endregion // ContainsReferenceHelper

		#endregion // RefBase Overrides
	}

	#endregion // RowReference Class

	#region DataRecordReference Class

	/// <summary>
	/// Reference implementation that represents an DataRecord.
	/// </summary>
	internal class DataRecordReference : RecordReferenceBase
	{
		#region Private Variables

		internal bool _inCellReferenceValueSet;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal DataRecordReference( RecordCollectionReference recordsReference, DataRecord dataRecord )
			: base( recordsReference, dataRecord )
		{
		}

		#endregion // Constructor

		#region RefBase Overrides

		public override RefBase BaseParent
		{
			get
			{
				Record parentRecord = RefUtils.GetParentRecord( _record );
				return null != parentRecord
					? (RefBase)_rootReference._utils.GetRecordReference( parentRecord )
					: (RefBase)_rootReference;
			}
		}

		public override string ElementName
		{
			get
			{
				int rowIndex = _rootReference._utils.GetRecordCalcIndex( _record );
				if ( rowIndex < 0 && _lastValidIndex >= 0 )
					rowIndex = _lastValidIndex;

				_lastValidIndex = rowIndex;

				string fieldLayoutName = _recordsReference.ElementName;
				System.Text.StringBuilder sb = new System.Text.StringBuilder( fieldLayoutName.Length + 8 );
				sb.Append( fieldLayoutName ).Append( RefParser.RefBeginScope ).Append( rowIndex ).Append( RefParser.RefEndScope );
				return sb.ToString( );
			}
		}


		/// <summary>
		/// Finds cell, column, or child records etc... based on the scope.
		/// </summary>
		/// <param name="name">Name to find.</param>
		/// <param name="scope">Can be Any, All, Index and RelativeIndex.</param>
		/// <param name="scopeValue">The value based on the scope type</param>
		internal override ICalculationReference FindItemHelper( string name, RefTuple.RefScope scope, object scopeValue )
		{
			DataRecord record = this.Record;
			FieldLayout fl = record.FieldLayout;
			RefUtils utils = _rootReference._utils;

			ICalculationReference iiFieldRef = utils.GetFieldReference( fl, name, true );
			FieldReference fieldRef = iiFieldRef as FieldReference;
			if ( null == fieldRef )
				return iiFieldRef;

			Field field = fieldRef.Field;

			if ( _rootReference._utils.IsChaptered( field ) )
			{
				ExpandableFieldRecord efr = utils.FindChildExpandableFieldRecord( record, field );
				if ( null == efr )
					return new CalculationReferenceError( name, "Invalid field {0}." );

				var childRecords = efr.ChildRecords;

				if ( RefTuple.RefScope.All == scope || RefTuple.RefScope.Any == scope )
					return utils.GetRecordCollectionReference( childRecords, true );
				else if ( RefTuple.RefScope.Index == scope )
					return utils.GetRecordReference( childRecords, (int)scopeValue, name );
				else if ( RefTuple.RefScope.Index == scope )
					return utils.GetRecordReference( childRecords, (string)scopeValue, name );
				else if ( RefTuple.RefScope.RelativeIndex == scope )
					return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_RelativeIndexInvalid" ) );

				return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_InvalidReference_InvalidScope" ) );
			}
			else
			{
				ICalculationReference recordRef = null;

				if ( RefTuple.RefScope.Index == scope )
				{
					if ( scopeValue is string )
						recordRef = utils.GetRecordReference( record.ParentCollection, (string)scopeValue, name );
					else
						recordRef = utils.GetRecordReference( record.ParentCollection, (int)scopeValue, name );
				}
				else if ( RefTuple.RefScope.RelativeIndex == scope )
					recordRef = utils.GetRecordRelativeReference( record, (int)scopeValue, name );
				else if ( RefTuple.RefScope.All == scope )
					return utils.GetNestedFieldReference( record.ParentCollection, fieldRef );
				else if ( RefTuple.RefScope.Any == scope )
					recordRef = this;

				if ( !( recordRef is DataRecordReference ) )
					return recordRef;

				return fieldRef.GetCellReference( (DataRecordReference)recordRef, true );
			}
		}


		#endregion // RefBase Overrides

		#region Record

		/// <summary>
		/// Gets the associated DataRecord.
		/// </summary>
		public new DataRecord Record
		{
			get
			{
				return (DataRecord)_record;
			}
		}

		#endregion // Record
	}

	#endregion // RowReference Class

	#region GroupByRecordReference Class

	/// <summary>
	/// Reference implementation that represents a GroupByRecord.
	/// </summary>
	internal class GroupByRecordReference : RecordReferenceBase
	{
		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal GroupByRecordReference( RecordCollectionReference recordsReference, GroupByRecord record )
			: base( recordsReference, record )
		{
		}

		#endregion // Constructor

		#region Record

		/// <summary>
		/// Returns the associated group-by record.
		/// </summary>
		public new GroupByRecord Record
		{
			get
			{
				return (GroupByRecord)base.Record;
			}
		}

		#endregion // Record

		#region RefBase Overrides

		public override RefBase BaseParent
		{
			get
			{
				Record parentRecord = RefUtils.GetParentRecord( this.Record );
				return null == parentRecord || !( parentRecord is GroupByRecord )
					? (RefBase)_recordsReference
					: (RefBase)_rootReference._utils.GetRecordReference( parentRecord );
			}
		}

		public override string ElementName
		{
			get
			{
				
				// Make sure the implementation is correct, especially regarding the group-by rows.
				//
				int recordIndex = _rootReference._utils.GetRecordCalcIndex( this.Record );
				if ( recordIndex < 0 && _lastValidIndex >= 0 )
					recordIndex = _lastValidIndex;

				_lastValidIndex = recordIndex;

				string groupByFieldKey = ReferenceManager.ResolveElementName( this.Record.GroupByField );
				System.Text.StringBuilder sb = new System.Text.StringBuilder( groupByFieldKey.Length + 8 );
				sb.Append( groupByFieldKey ).Append( RefParser.RefBeginScope ).Append( recordIndex ).Append( RefParser.RefEndScope );
				return sb.ToString( );
			}
		}

		/// <summary>
		/// Finds cell, column, or child records etc... based on the scope.
		/// </summary>
		/// <param name="name">Name to find.</param>
		/// <param name="scope">Can be Any, All, Index and RelativeIndex.</param>
		/// <param name="scopeValue">The value based on the scope type</param>
		internal override ICalculationReference FindItemHelper( string name, RefTuple.RefScope scope, object scopeValue )
		{
			GroupByRecord record = this.Record;
			FieldLayout fl = record.FieldLayout;
			RefUtils utils = _rootReference._utils;
			FieldLayoutReference flRef = _recordsReference._fieldLayoutReference;

			// SSP 3/23/12 TFS98819
			// In group-by situations where group-by fields are State and City, a reference like this
			// [//ultraGrid1/Customers/State(2)/City(3)/Customers(2)/Total] means that the leaf group-by 
			// record will get the field-layout's name passed into the FindItem. Take that into account.
			// 
 			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			bool isFieldLayoutName = RefParser.AreStringsEqual( flRef.ElementName, name, true );
			ICalculationReference ooFieldRef = flRef.GetFieldReference( name, ! isFieldLayoutName );
			FieldReference fieldRef = ooFieldRef as FieldReference;
			if ( null == fieldRef && ! isFieldLayoutName )
				return ooFieldRef;

			Field field = null != fieldRef ? fieldRef.Field : null;
			if ( null != field && utils.IsChaptered( field ) )
			{
				ICalculationReference ooChildFieldLayoutRef = _rootReference.GetFieldLayoutReference( fl, name, true );
				FieldLayoutReference childFieldLayoutRef = ooChildFieldLayoutRef as FieldLayoutReference;
				if ( null == childFieldLayoutRef )
					return ooChildFieldLayoutRef;

				var childRecords = record.ChildRecords;

				if ( RefTuple.RefScope.All == scope || RefTuple.RefScope.Any == scope )
					return utils.GetNestedRecordsReference( childRecords, childFieldLayoutRef );
				else
					return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_InvalidReference_InvalidScope" ) );
			}
			else
			{
				var childRecords = record.ChildRecords;

				Field nextGroupByField = RefUtils.GetNextGroupByField( record.GroupByField, fl );
				if ( field == nextGroupByField )
				{
					if ( RefTuple.RefScope.All == scope || RefTuple.RefScope.Any == scope )
						return utils.GetRecordCollectionReference( childRecords, true );
					else if ( RefTuple.RefScope.Index == scope )
						return utils.GetRecordReference( childRecords, (int)scopeValue, name );
					else if ( RefTuple.RefScope.Identifier == scope )
						return utils.GetRecordReference( childRecords, (string)scopeValue, name );
					else if ( RefTuple.RefScope.RelativeIndex == scope )
						return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_RelativeIndexInvalid" ) );

					return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_InvalidReference_InvalidScope" ) );
				}
				
				if ( null != fieldRef && RefTuple.RefScope.All == scope )
					return utils.GetNestedFieldReference( childRecords, fieldRef );

				return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_InvalidReference_InvalidScope" ) );
			}
		}


		// SSP 5/18/05
		// Overrode FindSummaryItem so the group-by row can return the summary of the child rows
		// instead of the summary of its parent row collection.
		//
		public override ICalculationReference FindSummaryItem( string name )
		{
			var childRecordsRef = _rootReference._utils.GetRecordCollectionReference( this.Record.ChildRecords, true );

			return childRecordsRef.GetSummaryResultReference( name, true );
		}

		#region ResolveReference

		// SSP 10/28/04
		// Overrode ResolveReference.
		//
		public override ICalculationReference ResolveReference( ICalculationReference reference, ResolveReferenceType referenceType )
		{
			RecordCollectionReference childRecordsRef = _rootReference._utils.GetRecordCollectionReference( this.Record.ChildRecords );

			Debug.Assert( null != childRecordsRef );
			if ( null != childRecordsRef )
			{
				ICalculationReference tmp = childRecordsRef.ResolveReferenceHelper( reference, referenceType, null );
				if ( tmp is NestedFieldReference )
					return tmp;
			}

			return base.ResolveReference( reference, referenceType );
		}

		#endregion // ResolveReference

		#endregion // RefBase Overrides
	}

	#endregion // GroupByRecordReference Class

	#region GroupLevelReference

	// SSP 9/9/04
	// Added GroupLevelReference that represents a group level in a group-by situation. This
	// is used for summaries only.
	//
	/// <summary>
	/// Represents a group level in a group-by situation. This will be used as a parent of 
	/// GroupLevelSummaryDefinitionReference instances. This class will return the name of
	/// the associated group-by column as its element name. This class is only used for
	/// summaries.
	/// </summary>
	internal class GroupLevelReference : DataPresenterRefBase
	{
		#region Private Vars

		private DataPresenterRefBase _parentReference;
		private Field _groupByField;
		private FieldReference _groupByFieldReference;
		private FieldLayoutReference _fieldLayoutReference;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal GroupLevelReference( DataPresenterRefBase parentReference, FieldReference groupByFieldReference )
			: base( groupByFieldReference._rootReference, groupByFieldReference.ElementName, groupByFieldReference.FieldLayoutContext )
		{
			CoreUtilities.ValidateNotNull( parentReference );
			CoreUtilities.ValidateNotNull( groupByFieldReference );

			if ( !( parentReference is GroupLevelReference ) && !( parentReference is FieldLayoutReference ) )
				throw new ArgumentException( "parentReference must be either a FieldLayoutReference or a GroupLevelReference." );

			if ( ! groupByFieldReference.Field.IsGroupBy )
				throw new ArgumentException( "Field must be a group-by field." );

			_fieldLayoutReference = groupByFieldReference._fieldLayoutReference;
			_groupByFieldReference = groupByFieldReference;
			_groupByField = groupByFieldReference.Field;
			_parentReference = parentReference;
		}

		#endregion // Constructor

		#region GroupByField

		/// <summary>
		/// Returns the associated group-by field.
		/// </summary>
		internal Field GroupByField
		{
			get
			{
				return _groupByField;
			}
		}

		#endregion // GroupByField

		#region RefBase Overrides

		#region BaseParent

		public override RefBase BaseParent
		{
			get
			{
				return _parentReference;
			}
		}

		#endregion // BaseParent

		#region FindItem

		public override ICalculationReference FindItem( string name )
		{
			return _fieldLayoutReference.FindItem( name );
		}

		#endregion // FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			return _fieldLayoutReference.FindSummaryItem( name );
		}

		#endregion // FindSummaryItem

		#region FindItem

		public override ICalculationReference FindItem( string name, string index )
		{
			return this.FindItem( name );
		}

		#endregion // FindItem

		#region FindAll

		public override ICalculationReference FindAll( string name )
		{
			return this.FindItem( name );
		}

		#endregion // FindAll

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return this.FindItem( name, index, isRelative );
		}

		#endregion // FindItem

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return 0x7fedcba ^ _groupByField.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is a RefUnAnchored instance then delegate the call to it.
			//
			if ( obj is RefUnAnchored )
				return obj.Equals( this );

			GroupLevelReference glRef = obj as GroupLevelReference;
			return null != glRef && _groupByField == glRef._groupByField;
		}

		#endregion // Equals

		#endregion // RefBase Overrides
	}

	#endregion // GroupLevelReference

	#region GroupByLevelSummaryDefinitionReference Class

	// SSP 9/9/04
	// Added GroupLevelSummaryDefinitionReference class.
	//
	/// <summary>
	/// Represents a summary settings reference at a particular group-by level. We needed
	/// to do this because when a cell got dirtied, all the summary values across all the 
	/// group-by levels need to be dirtied. The root summary settings object (one that 
	/// would be associated with the overall summary is always represented by 
	/// SummaryDefinitionReference. GroupLevelSummaryDefinitionReference only represents 
	/// summaries at lower levels.
	/// </summary>
	internal class GroupLevelSummaryDefinitionReference : SummaryDefinitionReference
	{
		#region Private Vars

		private GroupLevelReference _parentReference;
		private bool _isDisposed;
		internal readonly SummaryDefinitionReference _summaryReference;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal GroupLevelSummaryDefinitionReference( GroupLevelReference parentReference, SummaryDefinitionReference summaryReference )
			: base( summaryReference._referenceManager, summaryReference.ElementName, summaryReference.Summary )
		{
			_parentReference = parentReference;
			_summaryReference = summaryReference;
		}

		#endregion // Constructor

		#region ParentReference

		/// <summary>
		/// Returns the associated GroupLevelReference reference.
		/// </summary>
		internal GroupLevelReference ParentReference
		{
			get
			{
				return _parentReference;
			}
		}

		#endregion // ParentReference

		#region GroupByField

		/// <summary>
		/// Returns the associated group-by field.
		/// </summary>
		internal Field GroupByField
		{
			get
			{
				return this.ParentReference.GroupByField;
			}
		}

		#endregion // GroupByField

		#region RefBase Overrides

		public override RefBase BaseParent
		{
			get
			{
				return this.ParentReference;
			}
		}

		#endregion // RefBase Overrides

		#region InternalSetIsDisposed

		internal void InternalSetIsDisposed( bool disposed )
		{
			_isDisposed = disposed;
		}

		#endregion // InternalSetIsDisposed

		#region IsDisposedReference

		/// <summary>
		/// True indicates that the reference was disposed.  Read Only.
		/// </summary>
		public override bool IsDisposedReference
		{
			get
			{
				return _isDisposed || base.IsDisposedReference;
			}
		}

		// SSP 10/12/04
		// Overrode GetHashCode and Equals. Calc engine will make use of these instead of
		// normalized absolute name comparisions.
		//
		#region GetHashCode

		public override int GetHashCode( )
		{
			return 0x7abcdef ^ _parentReference.GetHashCode( );
		}

		#endregion // GetHashCode

		#region Equals

		public override bool Equals( object obj )
		{
			// If the object is RefUnAnchored or NestedSummaryDefinitionReference then delegate the 
			// call to it.
			//
			if ( obj is RefUnAnchored || obj is NestedSummaryDefinitionReference )
				return obj.Equals( this );

			GroupLevelSummaryDefinitionReference glRef = obj as GroupLevelSummaryDefinitionReference;
			return null != glRef && _parentReference.Equals( glRef._parentReference )
				&& this.Summary == glRef.Summary;
		}

		#endregion // Equals

		#endregion // IsDisposedReference
	}

	#endregion // GroupByLevelSummaryDefinitionReference Class


	/// <summary>
	/// A helper class.
	/// </summary>
	internal class RefUtils
	{
		#region Member Vars

		internal const string SUMMARY_REFERENCE_POSTFIX = "()";
		internal const string RANGE_REFERENCE_SEPARATOR = RefParser.RangeSeparator;

		private DataPresenterReference _rootRef;
		private ItemCache<RecordCollectionBase, RecordCollectionReference> _recordCollectionReferences;
		private ItemCache<Record, RecordReferenceBase> _recordReferences;		

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="rootRef"></param>
		internal RefUtils( DataPresenterReference rootRef )
		{
			CoreUtilities.ValidateNotNull( rootRef );
			_rootRef = rootRef;
		}

		#endregion // Constructor

		#region AddFormulaHelper

		// SSP 2/4/05 
		// Added AddFormulaHelper and RemoveFormulaHelper methods. Apparently the calc-engine does
		// not handle call to RemoveFormula properly if the formula has syntax errors, even if the
		// formula was never added to it, which is what we have been doing, not adding any formulas
		// with syntax errors.
		//
		/// <summary>
		/// Adds the specified formula to the specified calc manager. If the formula has syntax error
		/// then it does not add it.
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="formula"></param>
		/// <returns></returns>
		internal static bool AddFormulaHelper( DataPresenterRefBase reference, ICalculationFormula formula )
		{
			if ( !formula.HasSyntaxError )
			{
				var calcManager = reference.CalcManager;
				if ( null != calcManager )
				{
					calcManager.InternalAddFormula( formula );
					return true;
				}
			}

			return false;
		}

		#endregion // AddFormulaHelper

		#region CreateMissingFieldReference

		internal CalculationReferenceError CreateMissingFieldReference( FieldLayout fieldLayout, string name )
		{
			return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_FieldNotFound", ReferenceManager.ResolveName( _rootRef, fieldLayout ), name ) );
		}

		#endregion // CreateMissingFieldReference

		#region FindChildExpandableFieldRecord

		internal ExpandableFieldRecord FindChildExpandableFieldRecord( DataRecord record, Field field )
		{
			var records = record.HasExpandableFieldRecords ? record.ChildRecords : null;

			return null != records ? records.FirstOrDefault<ExpandableFieldRecord>( ii => ii.Field == field ) : null;
		}		 

		#endregion // FindChildExpandableFieldRecord

		#region FindRow

		internal bool EqualValue( Record record, Field field, string testVal )
		{
			
			//

			GroupByRecord groupByRecord = record as GroupByRecord;
			if ( null != groupByRecord && field == groupByRecord.GroupByField )
			{
				object val = groupByRecord.Value;
				return (object)val == (object)testVal || null != val && null != testVal && val.ToString( ).Equals( testVal );
			}

			DataRecord dr = record as DataRecord;
			if ( null != dr )
			{
				string text = dr.GetCellText( field );
				return RefParser.AreStringsEqual( text, testVal, true );
			}

			Debug.Assert( false );
			return false;
		}

		internal bool DoesRecordMatchCriteria( Record record, RefParser.NameValuePair[] nameValuePairs )
		{
			Debug.Assert( nameValuePairs.Length > 0 );

			foreach ( RefParser.NameValuePair nv in nameValuePairs )
			{
				Field field = this.GetField( record.FieldLayout, nv.Name, false );
				if ( null == field || ! this.EqualValue( record, field, nv.Value ) )
					return false;
			}

			return true;
		}

		internal GroupByRecord FindGroupByRecord( RecordCollectionBase records, string index )
		{
			if ( ! this.IsGroupByRecords( records ) )
				throw new CalculationException( "Specified rows collection does not contain group-by rows." );

			// VINDEX
			foreach ( GroupByRecord record in records )
			{
				if ( this.EqualValue( record, records.GroupByField, index ) )
					return record;
			}

			return null;
		}

		internal Record FindRecord( RecordCollectionBase records, FieldLayout fieldLayout, string index, out string exception )
		{
			// assume no errors
			exception = null;
			Record matchingRecord = null;

			if ( fieldLayout != records.FieldLayout && IsDescendantOf( fieldLayout, records.FieldLayout ) )
				exception = string.Format( "{0} is not a descendant field-layout of the {1} field-layout.", fieldLayout, records.FieldLayout );

			if ( exception == null )
			{
				RefParser.NameValuePair[] nameValuePairs = RefParser.ParseNameValuePairs( index, out exception );

				if ( exception == null )
				{
					CalculationScope calcScope = fieldLayout.CalculationScopeResolved;
					bool visibleOnly = CalculationScope.FilteredSortedList == calcScope;

					foreach ( Record record in records.GetRecordEnumerator( RecordType.DataRecord, fieldLayout, fieldLayout, visibleOnly ) )
					{
						if ( this.DoesRecordMatchCriteria( record, nameValuePairs ) )
						{
							matchingRecord = record;
							break;
						}
					}

					if ( null == matchingRecord )
						exception = SR.GetString( "LER_Calc_IndexedRecordNotFound", index );
				}
			}

			return matchingRecord;
		}

		#endregion // FindRow

		#region GetCalculationScopeResolved

		internal CalculationScope GetCalculationScopeResolved( Record record )
		{
			return record.FieldLayout.CalculationScopeResolved;
		}

		internal CalculationScope GetCalculationScopeResolved( RecordCollectionBase records )
		{
			return records.FieldLayout.CalculationScopeResolved;
		} 

		#endregion // GetCalculationScopeResolved

		#region GetCellReference

		internal CellReference GetCellReference( DataRecord dr, Field field, bool allocate = true )
		{
			FieldReference fieldRef = (FieldReference)this.GetFieldReference( field, false );

			return null != fieldRef ? this.GetCellReference( dr, fieldRef, allocate ) : null;
		}

		internal CellReference GetCellReference( DataRecord dr, FieldReference fieldRef, bool allocate = true )
		{
			DataRecordReference drRef = (DataRecordReference)this.GetRecordReference( dr, allocate );

			return null != drRef ? this.GetCellReference( drRef, fieldRef, allocate ) : null;
		}

		internal CellReference GetCellReference( DataRecordReference drRef, FieldReference fieldRef, bool allocate = true )
		{
			return null != fieldRef && null != drRef ? fieldRef.GetCellReference( drRef, allocate ) : null;
		} 

		#endregion // GetCellReference

		#region GetField

		internal Field GetField( FieldLayout fieldLayout, string name, bool raiseException )
		{
			FieldReference fieldReference = this.GetFieldReference( fieldLayout, name, false ) as FieldReference;
			if ( null != fieldReference )
				return fieldReference.Field;

			if ( raiseException )
				throw new CalculationException( RefUtils.GetString( "LER_Calc_FieldNotFound", ReferenceManager.ResolveElementName( _rootRef, fieldLayout ), name ) );

			return null;
		}

		internal ICalculationReference GetFieldReference( FieldLayout fieldLayout, string name, bool returnErrorReference )
		{
			var ooFLReference = _rootRef.GetFieldLayoutReference( fieldLayout, true );
			var flReference = ooFLReference as FieldLayoutReference;
			if ( null == flReference )
				return ooFLReference;

			return flReference.GetFieldReference( name, true );
		}

		internal ICalculationReference GetFieldReference( Field field, bool returnErrorReference )
		{
			ICalculationReference ooFLRef = _rootRef.GetFieldLayoutReference( field.Owner, true );
			FieldLayoutReference flRef = ooFLRef as FieldLayoutReference;
			if ( null == flRef )
				return returnErrorReference ? ooFLRef : null;

			return flRef.GetFieldReference( field, returnErrorReference );
		}

		#endregion // GetField

		#region GetImmediateChildRecordsMatchingFieldLayout

		internal static RecordCollectionBase GetImmediateChildRecordsMatchingFieldLayout( Record record, FieldLayout childFieldLayout )
		{
			DataRecord dr = record as DataRecord;
			if ( null != dr )
			{
				ExpandableFieldRecordCollection childRecords = dr.ChildRecords;
				if ( null != childRecords )
				{
					foreach ( var ii in childRecords )
					{
						var iiChildRecords = ii.ChildRecords;
						if ( null != iiChildRecords && iiChildRecords.FieldLayout == childFieldLayout )
							return ii.ChildRecords;
					}
				}

				return null;
			}

			GroupByRecord gr = record as GroupByRecord;
			if ( null != gr )
			{
				var childRecords = gr.ChildRecords;
				if ( null != childRecords && childRecords.FieldLayout == childFieldLayout )
					return childRecords;

				return null;
			}

			Debug.Assert( false, "Unknown type of record." );
			return null;
		} 

		#endregion // GetImmediateChildRecordsMatchingFieldLayout

		#region GetMaxCalcIndex

		internal int GetMaxCalcIndex( RecordCollectionBase records )
		{
			return this.GetMaxCalcIndex( records, records.FieldLayout.CalculationScopeResolved );
		}

		internal int GetMaxCalcIndex( RecordCollectionBase records, CalculationScope calcScope )
		{
			if ( CalculationScope.FullUnsortedList == calcScope && !this.IsGroupByRecords( records ) )
				return records.ParentRecordManager.Unsorted.Count - 1;

			if ( CalculationScope.FilteredSortedList == calcScope )
				return records.SparseArray.GetVisibleCount( ) - 1;

			return records.Count - 1;
		}

		#endregion // GetMaxCalcIndex

		#region GetNestedFieldReference

		internal ICalculationReference GetNestedFieldReference( RecordCollectionBase records, FieldReference fieldRef )
		{
			return new NestedFieldReference( records, fieldRef, false );
		}

		internal ICalculationReference GetNestedFieldReference( RecordCollectionBase records, string name )
		{
			var fieldRef = this.GetFieldReference( records.FieldLayout, name, true );
			if ( !( fieldRef is FieldReference ) )
				return fieldRef;

			return this.GetNestedFieldReference( records, (FieldReference)fieldRef );
		} 

		#endregion // GetNestedFieldReference

		#region GetNestedRecordsReference

		internal ICalculationReference GetNestedRecordsReference( RecordCollectionBase scope, FieldLayoutReference descendantFieldLayout )
		{
			return new NestedFieldLayoutReference( scope, descendantFieldLayout );
		}

		#endregion // GetNestedRecordsReference

		#region GetNestedSummaryDefinitionReference

		internal NestedSummaryDefinitionReference GetNestedSummaryDefinitionReference( RecordCollectionBase records, SummaryDefinitionReference summaryReference )
		{
			return new NestedSummaryDefinitionReference( summaryReference, records );
		}

		#endregion // GetNestedSummaryDefinitionReference

		#region GetNextGroupByField

		/// <summary>
		/// Returns the group-by field after the passed in group-by field. If null is passed in for groupByField then
		/// returns the first group-by field in the specified field-layout.
		/// </summary>
		/// <param name="groupByField"></param>
		/// <returns></returns>
		internal static Field GetNextGroupByField( Field groupByField, FieldLayout fieldLayout = null )
		{
			if ( null == fieldLayout && null != groupByField )
				fieldLayout = groupByField.Owner;

			if ( null != fieldLayout )
			{
				var sortedFields = fieldLayout.SortedFields;

				int nextIndex;
				if ( null == groupByField )
				{
					nextIndex = 0;
				}
				else
				{
					int index = sortedFields.IndexOf( groupByField );
					nextIndex = index >= 0 ? 1 + index : -1;
				}

				if ( nextIndex >= 0 && nextIndex < sortedFields.Count )
				{
					Field nextSortField = sortedFields[nextIndex].Field;

					if ( null == nextSortField || nextSortField.IsGroupBy )
						return nextSortField;
				}
			}

			return null;
		}

		#endregion // GetNextGroupByField

		#region GetParentRecord

		internal static Record GetParentRecord( Record record )
		{
			var parentRecord = record.ParentRecord;

			ExpandableFieldRecord efr = parentRecord as ExpandableFieldRecord;
			if ( null != efr )
				parentRecord = GetParentRecord( efr );

			return parentRecord;
		}

		#endregion // GetParentRecord

		#region GetRecordCalcIndex

		internal int GetRecordCalcIndex( Record record )
		{
			CalculationScope calcScope = record.FieldLayout.CalculationScopeResolved;

			return GetRecordCalcIndex( record, calcScope );
		}

		internal int GetRecordCalcIndex( Record record, CalculationScope calcScope )
		{
			if ( CalculationScope.FullUnsortedList == calcScope && record is DataRecord )
				return ( (DataRecord)record ).DataItemIndex;

			var sparseArray = record.ParentCollection.SparseArray;
			if ( CalculationScope.FilteredSortedList == calcScope )
				return sparseArray.GetVisibleIndexOf( record );

			return sparseArray.IndexOf( record );
		}

		#endregion // GetRecordCalcIndex

		#region GetRecordCollectionReference

		internal RecordCollectionReference GetRecordCollectionReference( RecordCollectionBase records, bool allocate = true )
		{
			return this.RecordCollectionReferences.GetItem( records, allocate );
		}

		#endregion // GetRecordCollectionReference

		#region GetRecordRelativeReference

		internal ICalculationReference GetRecordRelativeReference( Record record, int offset, string name )
		{
			int recordCalcIndex = this.GetRecordCalcIndex( record );
			if ( recordCalcIndex >= 0 )
				recordCalcIndex += offset;

			return this.GetRecordReference( record.ParentCollection, recordCalcIndex, name );
		}

		#endregion // GetRecordRelativeReference

		#region GetRecord

		internal Record GetRecord( RecordCollectionBase records, int calcIndex )
		{
			return this.GetRecord( records, calcIndex, records.FieldLayout.CalculationScopeResolved );
		}

		internal Record GetRecord( RecordCollectionBase records, int calcIndex, CalculationScope calcScope )
		{
			if ( CalculationScope.FullUnsortedList == calcScope && !this.IsGroupByRecords( records ) )
				return SafeItemAt( records.ParentRecordManager.Unsorted, calcIndex );

			if ( CalculationScope.FilteredSortedList == calcScope )
			{
				MainRecordSparseArray arr = records.SparseArray as MainRecordSparseArray;
				if ( null != arr )
					return arr.GetItemAtVisibleIndex( calcIndex );

				return (Record)records.SparseArray.GetItem( calcIndex );
			}

			return SafeItemAt( records, calcIndex );
		}

		#endregion // GetRecord

		#region GetRecordReference

		internal ICalculationReference GetRecordReference( RecordCollectionBase records, int calcIndex, bool returnError = true )
		{
			return this.GetRecordReference( records, calcIndex, null, returnError );
		}

		internal ICalculationReference GetRecordReference( RecordCollectionBase records, int calcIndex, string name, bool returnError = true )
		{
			Record record = this.GetRecord( records, calcIndex );

			if ( null == record )
				return new CalculationReferenceError( name, RefUtils.GetString( "LER_Calc_RecordNotFound", calcIndex ) );

			return this.GetRecordReference( record );
		}

		#region RecordReferences

		internal ItemCache<Record, RecordReferenceBase> RecordReferences
		{
			get
			{
				if ( null == _recordReferences )
				{
					_recordReferences = new ItemCache<Record, RecordReferenceBase>(
						_rootRef,
						ii =>
						{
							var recordsReference = this.GetRecordCollectionReference( ii.ParentCollection );

							if ( null != recordsReference )
							{
								if ( ii is GroupByRecord )
									return new GroupByRecordReference( recordsReference, (GroupByRecord)ii );
								else if ( ii is DataRecord )
									return new DataRecordReference( recordsReference, (DataRecord)ii );
							}

							Debug.Assert( false );
							return null;
						}
					);
				}

				return _recordReferences;
			}
		} 

		#endregion // RecordReferences

		#region RecordCollectionReferences

		internal ItemCache<RecordCollectionBase, RecordCollectionReference> RecordCollectionReferences
		{
			get
			{
				if ( null == _recordCollectionReferences )
				{
					_recordCollectionReferences = new ItemCache<RecordCollectionBase, RecordCollectionReference>(
						_rootRef,
						ii =>
						{
							FieldLayoutReference flRef = (FieldLayoutReference)_rootRef.GetFieldLayoutReference( ii.FieldLayout, false );
							return null != flRef ? new RecordCollectionReference( flRef, ii ) : null;
						}
					);
				}

				return _recordCollectionReferences;
			}
		}

		#endregion // RecordCollectionReferences

		internal RecordReferenceBase GetRecordReference( Record record, bool allocate = true )
		{
			return this.RecordReferences.GetItem( record, allocate );
		}

		internal ICalculationReference GetRecordReference( RecordCollectionBase records, string index, string name )
		{
			return this.GetRecordReference( records, records.FieldLayout, index, name );
		}
		
		internal ICalculationReference GetRecordReference( RecordCollectionBase records, FieldLayout fieldLayout, string index, string name )
		{
			string error;
			Record record = this.FindRecord( records, fieldLayout, index, out error );

			if ( null == record )
				return new CalculationReferenceError( name, error );

			return this.GetRecordReference( record );
		}

		#endregion // GetRecordReference

		#region GetString

		internal static string GetString( string name, params object[] args )
		{
#pragma warning disable 436
			return SR.GetString( name, args );
#pragma warning restore 436
		}

		#endregion // GetString

		#region GetSummaryDefinitionReference

		internal ICalculationReference GetSummaryDefinitionReference( SummaryDefinition summary, bool returnErrorReference )
		{
			var fl = summary.FieldLayout;
			ICalculationReference ooFLRef = null != fl ? _rootRef.GetFieldLayoutReference( fl, true ) : null;
			FieldLayoutReference flRef = ooFLRef as FieldLayoutReference;
			if ( null == flRef )
				return returnErrorReference ? ooFLRef : null;

			return flRef.GetSummaryDefinitionReference( summary, returnErrorReference );
		}

		#endregion // GetSummaryDefinitionReference

		#region GetTopLevelRecordCollection

		internal static RecordCollectionBase GetTopLevelRecordCollection( RecordCollectionBase records )
		{
			while ( !records.IsTopLevel )
				records = records.ParentRecord.ParentCollection;

			return records;
		} 

		#endregion // GetTopLevelRecordCollection

		#region GetUnderlyingReference

		internal static ICalculationReference GetUnderlyingReference( ICalculationReference reference )
		{
			RefUnAnchored ru = reference as RefUnAnchored;
			return null != ru ? ru.WrappedReference : reference;
		}

		#endregion // GetUnderlyingReference

		#region InsertGroupByColumnTuples

		internal List<RefTuple> InsertGroupByFieldTuples( RefParser rp )
		{
			return this.InsertGroupByFieldTuples( rp, null );
		}

		internal List<RefTuple> InsertGroupByFieldTuples( RefParser rp, FieldLayout skipFieldLayout )
		{
			var rootRef = _rootRef;

			// MD 8/10/07 - 7.3 Performance
			// Use generics
			//ArrayList list = new ArrayList( );
			List<RefTuple> list = new List<RefTuple>( );

			try
			{
				// If the typle count of an absolute name is 1 or less than the only component in the
				// absolute name can be the name of the grid. This method can't do anything with such
				// a reference name.
				//
				if ( rp.TupleCount < 2 )
					return list;

				// Get the root band. First tuple in rp should be the grid tuple and the second
				// should be the band tuple.
				//
				FieldLayout lastFieldLayout = rootRef.GetFieldLayout( null, rp[1].Name );
				list.Add( rp[1] );
				int lastFieldLayoutTupleIndex = 0;
				bool lastFieldLayoutAlreadyProcessed = false;
				Field lastGroupByField = null;

				for ( int i = 2; i < rp.TupleCount; i++ )
				{
					RefTuple tuple = rp[i];
					bool isLastTuple = i + 1 == rp.TupleCount;

					Field field = RefTuple.RefScope.SummaryValue != tuple.Scope
						? this.GetField( lastFieldLayout, tuple.Name, false ) : null;

					// Last tuple can be a field that's a group-by field. In such a case
					// we don't want to treat the tuple as a group-by type but rather a field
					// tuple as in a formula.
					//
					if ( null != field && field.IsGroupBy && !isLastTuple )
					{
						lastGroupByField = field;
					}
					else
					{
						if ( !lastFieldLayoutAlreadyProcessed && lastFieldLayout != skipFieldLayout )
						{
							while ( null != ( lastGroupByField = RefUtils.GetNextGroupByField( lastGroupByField, lastFieldLayout ) ) )
								list.Add( new RefTuple( ReferenceManager.ResolveElementName( lastGroupByField, true ) ) );

							string lastFieldLayoutId;
							if ( lastFieldLayout.HasGroupBySortFields
								&& ( lastFieldLayoutId = ReferenceManager.ResolveElementName( rootRef, lastFieldLayout, true ) ) != tuple.Name )
							{
								// In group-by mode, the band tuple is repeated. The first band
								// tuple shouldn't have any scope index. The one after all the group-by
								// columns should have the scope index. For example, 
								// //ultraGrid1/Customers/State(NY)/City(Albany)/Customers(0).
								// However a reference without the group tuples will be like
								// //ultraGrid1/Customers(0). When transforming this to contain
								// group-by tuples, the Customers(0) tuple needs to be added after
								// the group-by column tuples. Following three lines do exactly this.
								//
								RefTuple originalTuple = list[lastFieldLayoutTupleIndex];

								list.Add( originalTuple );
								list[lastFieldLayoutTupleIndex] = new RefTuple( lastFieldLayoutId );
							}
						}

						lastFieldLayoutAlreadyProcessed = true;
						lastGroupByField = null;

						if ( null != field && field.IsExpandableResolved )
						{
							lastFieldLayout = _rootRef.GetFieldLayout( lastFieldLayout, tuple.Name );
							lastFieldLayoutAlreadyProcessed = false;
							lastFieldLayoutTupleIndex = list.Count;
						}
					}

					list.Add( tuple );
				}
			}
			catch ( Exception exc )
			{
				Debug.Assert( false, "InsertGroupByColumnTuples: " + exc.ToString( ) );
				return null;
			}

			return list;
		}

		#endregion // InsertGroupByColumnTuples

		#region IsChaptered

		public bool IsChaptered( Field field )
		{
			return field.IsExpandableResolved;
		} 

		#endregion // IsChaptered

		#region IsDescendantOf

		/// <summary>
		/// Returns true if the 'testFieldLayout' is a descendant field-layout of 'ancestorFieldLayout'.
		/// </summary>
		/// <param name="testFieldLayout"></param>
		/// <param name="ancestorFieldLayout"></param>
		/// <returns></returns>
		internal static bool IsDescendantOf( FieldLayout testFieldLayout, FieldLayout ancestorFieldLayout )
		{
			var parentFL = testFieldLayout.ParentFieldLayout;
			if ( parentFL == ancestorFieldLayout )
				return true;

			return null != parentFL && IsDescendantOf( parentFL, ancestorFieldLayout );
		}

		internal static bool IsDescendantOf( Record recordToTest, RecordCollectionBase ancestorRecords )
		{
			while ( null != recordToTest )
			{
				if ( recordToTest.ParentCollection == ancestorRecords )
					return true;

				recordToTest = recordToTest.ParentRecord;
			}

			return false;
		}

		#endregion // IsDescendantOf

		#region IsGroupByRecords

		/// <summary>
		/// Indicates if the records are group-by records.
		/// </summary>
		/// <param name="records"></param>
		/// <returns></returns>
		internal bool IsGroupByRecords( RecordCollectionBase records )
		{
			return null != records.GroupByField;
		}

		#endregion // IsGroupByRecords

		#region IsIncludedInCalcScope

		internal bool IsIncludedInCalcScope( Record record )
		{
			var scope = this.GetCalculationScopeResolved( record );
			if ( CalculationScope.FilteredSortedList == scope )
				return Visibility.Visible == record.VisibilityResolved;

			return true;
		} 

		#endregion // IsIncludedInCalcScope

		#region IsObjectOfType

		/// <summary>
		/// Returns true if the specified object is an instance of the specified
		/// type or a type that's derived from specified type. It also checks for
		/// interfaces.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static bool IsObjectOfType( object obj, Type type )
		{
			return null != obj && IsObjectOfType( obj.GetType( ), type );
		}

		/// <summary>
		/// Returns true if the objectType is the same as the type or is 
		/// a type derived from the type. It also checks for interfaces.
		/// </summary>
		/// <param name="objectType"></param>
		/// <param name="typeToCheck"></param>
		/// <returns></returns>
		internal static bool IsObjectOfType( Type objectType, Type typeToCheck )
		{
			return objectType == typeToCheck || typeToCheck.IsAssignableFrom( objectType );
		}

		#endregion // IsObjectOfType

		#region IsStillValid

		internal bool IsStillValid( SummaryDefinition summary )
		{
			
			return true;
		}

		internal bool IsStillValid( RecordCollectionBase records )
		{
			
			return true;
		}

		internal bool IsStillValid( Record record )
		{
			
			return record.IsStillValid;
		} 

		#endregion // IsStillValid

		#region IsTrivialDescendantOf

		internal static bool IsTrivialDescendantOf( Record testRecord, Record ancestorRecord )
		{
			if ( testRecord == ancestorRecord )
				return true;

			var parent = testRecord.ParentRecord;
			return null != parent && IsTrivialDescendantOf( parent, ancestorRecord );
		}

		internal static bool IsTrivialDescendantOf( FieldLayout testFieldLayout, FieldLayout ancestorFieldLayout )
		{
			if ( testFieldLayout == ancestorFieldLayout )
				return true;

			var parent = testFieldLayout.ParentFieldLayout;
			return null != parent && IsTrivialDescendantOf( parent, ancestorFieldLayout );
		}

		internal static bool IsTrivialDescendantOf( RecordCollectionBase test, RecordCollectionBase ancestor )
		{
			if ( test == ancestor )
				return true;

			var parent = test.ParentRecord;
			return null != parent && IsDescendantOf( parent, ancestor );
		}

		#endregion // IsTrivialDescendantOf

		#region RemoveFormulaHelper

		// SSP 2/4/05 
		// Added AddFormulaHelper and RemoveFormulaHelper methods. Apparently the calc-engine does
		// not handle call to RemoveFormula properly if the formula has syntax errors, even if the
		// formula was never added to it, which is what we have been doing, not adding any formulas
		// with syntax errors.
		//
		/// <summary>
		/// Removes the specified formula from the specified calc manager. If the formula has 
		/// syntax error then it takes no action as a formula with syntax error should never have
		/// been added to the calc network in the first place.
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="formula"></param>
		internal static void RemoveFormulaHelper( DataPresenterRefBase reference, ICalculationFormula formula )
		{
			if ( !formula.HasSyntaxError )
			{
				var calcManager = reference.CalcManager;
				if ( null != calcManager )
					calcManager.InternalRemoveFormula( formula );
			}
		}

		#endregion // RemoveFormulaHelper

		#region SafeItemAt

		internal static T SafeItemAt<T>( IList<T> list, int index )
		{
			int count = list.Count;
			if ( index >= 0 && index < count )
				return list[index];

			return default( T );
		}

		#endregion // SafeItemAt

		#region ThrowRootBandNameExpectedException

		internal static void ThrowRootFieldLayoutNameExpectedException( DataPresenterReference dpRef, string name )
		{
			throw new CalculationException( RefUtils.GetString( "LER_Calc_RootFieldLayoutNameExpected", dpRef.ElementName, name ) );
		}

		#endregion // ThrowRootBandNameExpectedException
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