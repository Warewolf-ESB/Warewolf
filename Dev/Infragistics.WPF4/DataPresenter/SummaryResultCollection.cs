using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.DataPresenter.Internal;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Infragistics.Windows.DataPresenter
{
	#region SummaryResultCollection Class

	/// <summary>
	/// A collection of <see cref="SummaryResult"/> objects.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// RecordCollection's <see cref="RecordCollectionBase.SummaryResults"/> property returns
	/// an instance of this collection. It contains a <see cref="SummaryResult"/> object for each
	/// <see cref="SummaryDefinition"/> specified on the FieldLayout's <see cref="FieldLayout.SummaryDefinitions"/>
	/// collection.
	/// </para>
	/// </remarks>
	/// <seealso cref="RecordCollectionBase.SummaryResults"/>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// <seealso cref="SummaryResult"/>
	public class SummaryResultCollection : PropertyChangeNotifier, IList<SummaryResult>, IList
	{
		#region Private Vars

		private RecordCollectionBase _records;
		private SummaryDefinitionCollection _summaryDefinitions;
		private FieldLayout _fieldLayout;
		private int _verifiedSummariesCollectionVersion;
		private bool _inVerifyCollection;
		private List<SummaryResult> _list;
		private object _summaryRecordHeader;

		// SSP 8/2/09 - Summary Recalc Optimizations
		// 
		private object _pendingDirtyAffectedSummaries;
		/// <summary>
		/// Maps source field to the start and end index in the _cachedSortedResultsArr.
		/// </summary>
		private Dictionary<Field, GridUtilities.Range> _cachedSourceFieldMap;
		/// <summary>
		/// List of summary results sorted by source field.
		/// </summary>
		private SummaryResult[] _cachedSortedResultsArr;

		// SSP 9/19/2011 TFS88364
		// Added fieldLayoutContext parameter.
		// 
		private bool _cachedHasCalculationFieldFromDifferentFieldLayout;

		// SSP 1/10/12 TFS96332
		// 
		private PropertyValueTracker _summaryCollectionVersionTracker;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryResultCollection"/>.
		/// </summary>
		/// <param name="records">Associated records.</param>
		/// <param name="fieldLayout">Associated field layout.</param>
		/// <param name="summaryDefinitions">Associated summary definitions.</param>
		internal SummaryResultCollection( RecordCollectionBase records, 
			FieldLayout fieldLayout, SummaryDefinitionCollection summaryDefinitions ) 
			: base( )
		{
			GridUtilities.ValidateNotNull( records );
			GridUtilities.ValidateNotNull( fieldLayout );
			GridUtilities.ValidateNotNull( summaryDefinitions );

			_records = records;
			_fieldLayout = fieldLayout;
			_summaryDefinitions = summaryDefinitions;
			_list = new List<SummaryResult>( );
		}

		#endregion // Constructor

		#region Base Overrides

		#region OnHasListenersChanged

		// SSP 1/10/12 TFS96332
		// If a binding is used to access summary result (like "dataPresenter.Records.SummaryValues[0].Value"), then we need
		// to actively raise Item[] property change notification when the summary definitions are added or removed.
		// 
		/// <summary>
		/// Overridden. Called when HasListeners property changes.
		/// </summary>
		protected override void OnHasListenersChanged( )
		{
			base.OnHasListenersChanged( );

			if ( this.HasListeners )
			{
				if ( null == _summaryCollectionVersionTracker )
					_summaryCollectionVersionTracker = new PropertyValueTracker( this.SummaryDefinitions, "SummariesVersion", this.VerifyCollection );
			}
			else
			{
				_summaryCollectionVersionTracker = null;
			}
		}

		#endregion // OnHasListenersChanged 

		#endregion // Base Overrides

		#region Indexers

		#region Indexer (string key)

		/// <summary>
		/// Gets the SummaryResult object with the specified summary key.
		/// </summary>
		/// <param name="summaryKey">Key of the SummaryResult</param>
		/// <returns>Returns the matching SummaryResult object. If a match is not found, raises an exception.</returns>
		public SummaryResult this[string summaryKey]
		{
			get
			{
				SummaryResult ret = this.GetItem( summaryKey );

				// SSP 1/10/12 TFS96332
				// 
				if ( null == ret )
				{
					int index;
					if ( int.TryParse( summaryKey, out index ) )
					{
						if ( index >= 0 && index < this.Count )
							ret = this[index];
					}
				}

				if ( null == ret )
					GridUtilities.RaiseKeyNotFound( );

				return ret;
			}
		}

		#endregion // Indexer (string key)

		#region Indexer (int index)

		/// <summary>
		/// Gets the SummaryResult at the spcified index.
		/// </summary>
		/// <param name="index">Specifies the index at which to get the item</param>
		/// <returns>The SummaryResult at the specified index.</returns>
		/// <seealso cref="SummaryResult"/>
		public SummaryResult this[int index]
		{
			get
			{
				this.VerifyCollection( );
				return _list[index];
			}
			set
			{
				this.VerifyCollection( );
				_list[index] = value;
			}
		}

		#endregion // Indexer (int index)

		#region Indexer (SummaryDefinition summary)

		/// <summary>
		/// Gets the SummaryResult object associated with the specified summary.
		/// </summary>
		/// <param name="summary">SummaryDefinition object</param>
		/// <returns>Returns the SummaryResult associated with the specified summary. 
		/// If a match is not found, raises an exception.</returns>
		public SummaryResult this[SummaryDefinition summary]
		{
			get
			{
				SummaryResult ret = this.GetItem( summary );

				if ( null == ret )
					GridUtilities.RaiseKeyNotFound( );

				return ret;
			}
		}

		#endregion // Indexer (SummaryDefinition summary)

		#endregion // Indexers

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Returns the number of items in this collection.
		/// </summary>
		public int Count
		{
			get
			{
				this.VerifyCollection( );
				return _list.Count;
			}
		}

		#endregion // Count

		#region Records

		/// <summary>
		/// Gets the associated records whose data is summarized.
		/// </summary>
		/// <remarks>
		/// This SummaryResultCollection contains results of summary calculations of record data
		/// from this RecordCollection.
		/// </remarks>
		/// <seealso cref="RecordCollectionBase"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		public RecordCollectionBase Records
		{
			get
			{
				return _records;
			}
		}

		#endregion // Records

		#region SummaryDefinitions

		/// <summary>
		/// Gets the associated summary definitions.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryDefinitions</b> property returns the associated summary definitions.
		/// For each summary definition in the summary definitions collection, there is a
		/// <see cref="SummaryResult"/> in this summary results collection.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="SummaryDefinitionCollection"/>
		public SummaryDefinitionCollection SummaryDefinitions
		{
			get
			{
				return _summaryDefinitions;
			}
		}

		#endregion // SummaryDefinitions

		#region SummaryRecordHeader

		/// <summary>
		/// Specifies what to dispaly in the header of the summary record.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// By default the summary record's header contains text derived from settings of
		/// FieldLayout's <see cref="FieldLayout.SummaryDescriptionMask"/> and 
		/// <see cref="FieldLayout.SummaryDescriptionMaskInGroupBy"/> properties.
		/// You can use <b>SummaryRecordHeader</b> property to to specify summary record header's
		/// contents on a per record collection basis. Also using this property you can
		/// specify contents as any arbitrary value which will be presented using a ContentPresenter.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayout.SummaryDescriptionMask"/>
		/// <seealso cref="FieldLayout.SummaryDescriptionMaskInGroupBy"/>
		/// <seealso cref="SummaryRecord.SummaryRecordHeaderResolved"/>
		//[Description( "Specifies the what to dispaly in the header of the summary record." )]
		//[Category( "Data" )]
		public object SummaryRecordHeader
		{
			get
			{
				return _summaryRecordHeader;
			}
			set
			{
				if ( _summaryRecordHeader != value )
				{
					_summaryRecordHeader = value;
					
					this.RaisePropertyChangedEvent( "SummaryRecordHeader" );
				}
			}
		}

		#endregion // SummaryRecordHeader

		#endregion // Public Properties

		#region Private/Internal Properties

		#region SummariesVersion

		internal int SummariesVersion
		{
			get
			{
				this.VerifyCollection( );
				return _summaryDefinitions.SummariesVersion;
			}
		}

		#endregion // SummariesVersion

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Contains

		/// <summary>
		/// Indicates whether the specified item is in this collection.
		/// </summary>
		/// <param name="item">The item to check if it's in the collection.</param>
		/// <returns>True if the item is in the collection, false otherwise.</returns>
		public bool Contains( SummaryResult item )
		{
			this.VerifyCollection( );
			return _list.Contains( item );
		}

		#endregion // Contains

		#region IndexOf

		/// <summary>
		/// Returns the location of the specified item in this collection.
		/// </summary>
		/// <param name="item">The item whose location to return.</param>
		/// <returns>The location of the item in the collection. If the item is not found, returns -1.</returns>
		public int IndexOf( SummaryResult item )
		{
			this.VerifyCollection( );
			return _list.IndexOf( item );
		}

		#endregion // IndexOf

		#region Refresh

		/// <summary>
		/// Re-calculates the summary results.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Typically it's not necessary to call this method as summaries are recalculated 
		/// automatically whenever data changes.
		/// </para>
		/// <seealso cref="SummaryDefinition.Refresh"/>
		/// <seealso cref="SummaryDefinitionCollection.Refresh"/>
		/// </remarks>
		public void Refresh( )
		{
			foreach ( SummaryResult result in this )
			{
				result.Refresh( );
			}
		}

		#endregion // Refresh

		#endregion // Public Methods

		#region Private/Internal Methods

		#region DirtyAffectedSummaries

		// SSP 8/2/09 - Optimizations
		// Added DirtyAffectedSummaries method.
		// 

		// SSP 9/19/2011 TFS88364
		// Added fieldLayoutContext parameter.
		// 
		//internal void DirtyAffectedSummariesAsync( Field field )
		internal void DirtyAffectedSummariesAsync( Field field, FieldLayout fieldLayoutContext )
		{
			// SSP 12/22/11 TFS67264 - Optimizations
			// 
			if ( 0 == _list.Count )
				return;

			// SSP 9/19/2011 TFS88364
			// If a record from a field-layout other than the one associated with the summaries in this collection
			// is changed, we don't need to re-calc the summaries in this collection.
			// 
			// ------------------------------------------------------------------------------------------------------
			if ( null == field && null != fieldLayoutContext && fieldLayoutContext != _fieldLayout )
			{
				this.VerifySourceFieldMap( );

				if ( !_cachedHasCalculationFieldFromDifferentFieldLayout )
					return;
			}
			// ------------------------------------------------------------------------------------------------------

			DataPresenterBase dp = _records.DataPresenter;
			Dispatcher dispatcher = null != dp ? dp.Dispatcher : null;
			if ( null != dispatcher )
			{
				
				bool doBeginInvoke = null == _pendingDirtyAffectedSummaries;

				if ( null == field )
				{
					_pendingDirtyAffectedSummaries = DBNull.Value;
				}
				else if ( null == _pendingDirtyAffectedSummaries )
				{
					_pendingDirtyAffectedSummaries = field;

					
					
					
					
				}
				else if ( DBNull.Value != _pendingDirtyAffectedSummaries )
				{
					HashSet set = _pendingDirtyAffectedSummaries as HashSet;
					if ( null == set )
					{
						Field tmpField = (Field)_pendingDirtyAffectedSummaries;
						set = new HashSet( );
						set.Add( tmpField );

						// AS 8/20/09 TFS20762
						_pendingDirtyAffectedSummaries = set;
					}

					set.Add( field );
				}

				
				
				if ( doBeginInvoke )
					dispatcher.BeginInvoke( DispatcherPriority.Background,
						new GridUtilities.MethodDelegate( this.DirtyAffectedSummariesAsyncHanlder ) );
			}
			else
			{
				this.DirtyAffectedSummaries( field );
			}
		}

		internal void CleanPendingDirtyAffectedSummaries( )
		{
			this.DirtyAffectedSummariesAsyncHanlder( );
		}

		private void DirtyAffectedSummariesAsyncHanlder( )
		{
			object data = _pendingDirtyAffectedSummaries;
			if ( null != data )
			{
				_pendingDirtyAffectedSummaries = null;

				// _pendingDirtyAffectedSummaries can be either a HashSet of fields, a Field object
				// or DBNull to indicate all fields.
				// 
				HashSet set = data as HashSet;
				if ( null == set )
				{
					Field field = DBNull.Value == data ? null : (Field)data;
					this.DirtyAffectedSummaries( field );
				}
				else
				{
					foreach ( Field field in set )
						this.DirtyAffectedSummaries( field );
				}
			}
		}

		internal void DirtyAffectedSummaries( Field field )
		{
			this.VerifySourceFieldMap( );
			if ( null != field )
			{
				Dictionary<Field, GridUtilities.Range> map = _cachedSourceFieldMap;

				GridUtilities.Range range;
				if ( map.TryGetValue( field, out range ) )
				{
					SummaryResult[] sortedResultsArr = _cachedSortedResultsArr;

					for ( int i = range.Start; i <= range.End; i++ )
						sortedResultsArr[i].DirtyCalculation( );
				}
			}
			else
			{
				SummaryResult[] sortedResultsArr = _cachedSortedResultsArr;

				for ( int i = 0; i < sortedResultsArr.Length; i++ )
					sortedResultsArr[i].DirtyCalculation( );
			}
		}

		#endregion // DirtyAffectedSummaries

		#region DirtySourceFieldMap

		// SSP 8/2/09 - Summary Recalc Optimizations
		// 
		internal void DirtySourceFieldMapCache( )
		{
			_cachedSourceFieldMap = null;
			_cachedSortedResultsArr = null;
		}

		#endregion // DirtySourceFieldMap

		#region GetItem

		internal SummaryResult GetItem( string summaryKey )
		{
			SummaryDefinitionCollection summaries = this.SummaryDefinitions;
			SummaryDefinition summary = null != summaries ? summaries.GetItem( summaryKey ) : null;
			return null != summary ? this.GetItem( summary ) : null;
		}

		internal SummaryResult GetItem( SummaryDefinition summary )
		{
			this.VerifyCollection( );

			SummaryDefinitionCollection summaries = this.SummaryDefinitions;
			if ( null != summaries && null != summary )
			{
				foreach ( SummaryResult result in _list )
				{
					if ( result.SummaryDefinition == summary )
						return result;
				}
			}

			return null;
		}

		#endregion // GetItem

		#region GetSummaryRecord



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        // JJD 4/8/09 - TFS6338/BR34948
        // Added isRootLevel parameter
        //internal SummaryRecord GetSummaryRecord( bool top, bool isFixed, Record[] recordsToRecycle )
		internal SummaryRecord GetSummaryRecord( bool top, bool isFixed, Record[] recordsToRecycle, bool isRootLevel )
		{
			SummaryDisplayAreaContext context = SummaryDisplayAreaContext.GetContext( top, isFixed, this );

			IEnumerable<SummaryResult> results = this.GetSummaryResults( context, null );
			if ( GridUtilities.HasItems( results ) )
			{
				SummaryRecord summaryRecord = null;

				if ( null != recordsToRecycle )
				{
					for ( int i = 0; i < recordsToRecycle.Length; i++ )
					{
						SummaryRecord sr = recordsToRecycle[i] as SummaryRecord;
						if ( null != sr && context.Equals( sr.SummaryDisplayAreaContext ) 
							// SSP 2/9/09 TFS11121
							// Only reuse the record if the field layout matches.
							// 
							&& sr.FieldLayout == _fieldLayout
							)
						{
							summaryRecord = sr;
							recordsToRecycle[i] = null;
						}
					}
				}

				if ( null == summaryRecord )
					summaryRecord = new SummaryRecord( _records.FieldLayout, _records, context );

                // JJD 4/8/09 - TFS6338/BR34948
                // Only support fixed records on the bottom for root level rcds
                //summaryRecord.FixedRecordLocationOverride = GridUtilities.GetFixedRecordLocation( top, isFixed );
				summaryRecord.FixedRecordLocationOverride = GridUtilities.GetFixedRecordLocation( top, isFixed && (top || isRootLevel) );
				return summaryRecord;
			}

			return null;
		}

		#endregion // GetSummaryRecord

		#region GetSummaryResults

		/// <summary>
		/// Returns the summary results that match the specified context criteria. Returns null
		/// if there are no matching summary results.
		/// </summary>
		/// <param name="displayAreaContext">Display area context</param>
		/// <param name="positionContext">Summary position context</param>
		/// <returns>List of summary results. If no matching summary results found, then null.</returns>
		internal IEnumerable<SummaryResult> GetSummaryResults( 
			SummaryDisplayAreaContext displayAreaContext, SummaryPositionContext positionContext )
		{
			this.VerifyCollection( );

			IEnumerable<SummaryResult> ret = _list;

			if ( null != displayAreaContext )
				ret = displayAreaContext.Filter( ret );

			if ( null != positionContext )
				ret = displayAreaContext.Filter( ret );

			return ret;
		}

		#endregion // GetSummaryResults

		#region HasSummaryResultsFor

		// SSP 2/10/10 TFS26510
		// 

		internal bool HasSummaryResultsFor( Field field )
		{
			this.VerifySourceFieldMap( );

			return null != _cachedSourceFieldMap && _cachedSourceFieldMap.ContainsKey( field );
		}

		internal static bool HasSummaryDependentOn( Cell cell, bool verify )
		{
			Field field = cell.Field;
			Record record = cell.Record;
			RecordCollectionBase records = null != record ? record.ParentCollection : null;

			while ( null != records )
			{
				SummaryResultCollection summaryResults = verify ? records.SummaryResults : records.SummaryResultsIfAllocated;

				if ( null != summaryResults && summaryResults.HasSummaryResultsFor( field ) )
					return true;

				record = records.ParentRecord;
				records = null != record ? record.ParentCollection : null;
			}

			return false;
		}

		#endregion // HasSummaryResultsFor

		#region RefreshSummariesAffectedBySort

		
		
		/// <summary>
		/// Recalculates summaries with calculators that are affected by sort.
		/// </summary>
		internal void RefreshSummariesAffectedBySort( )
		{
			foreach ( SummaryResult result in this )
			{
				SummaryCalculator calculator = result.SummaryDefinition.Calculator;
				if ( null != calculator && calculator.IsCalculationAffectedBySort )
					result.Refresh( );
			}
		}

		#endregion // RefreshSummariesAffectedBySort

		#region VerifyCollection

		internal void VerifyCollection( )
		{
			if ( _inVerifyCollection )
				return;

			SummaryDefinitionCollection summaries = this.SummaryDefinitions;
			if ( null != summaries && _verifiedSummariesCollectionVersion == summaries.CollectionVersion )
				return;

			_inVerifyCollection = true;
			try
			{
				List<SummaryResult> list = _list;
				if ( null == summaries )
				{
					list.Clear( );
				}
				else
				{
					Dictionary<SummaryDefinition, SummaryResult> oldItems = new Dictionary<SummaryDefinition, SummaryResult>( );
					foreach ( SummaryResult result in list )
						oldItems[ result.SummaryDefinition ] = result;

					list.Clear( );

					foreach ( SummaryDefinition summary in summaries )
					{
						SummaryResult result;
						if ( !oldItems.TryGetValue( summary, out result ) )
							result = new SummaryResult( this, summary );

						list.Add( result );
					}

					// SSP 8/2/09 - Summary Recalc Optimizations
					// 
					this.DirtySourceFieldMapCache( );

					_verifiedSummariesCollectionVersion = summaries.CollectionVersion;
				}
			}
			finally
			{
				_inVerifyCollection = false;

				this.RaisePropertyChangedEvent( "Count" );
				// SSP 1/10/12 TFS96332
				// This was a typo.
				// 
				//this.RaisePropertyChangedEvent( "Items[]" );
				this.RaisePropertyChangedEvent( "Item[]" );
			}
		}

		#endregion // VerifyCollection

		#region VerifySourceFieldMap

		// SSP 8/2/09 - Summary Recalc Optimizations
		// 
		private void VerifySourceFieldMap( )
		{
			this.VerifyCollection( );
			if ( null == _cachedSourceFieldMap )
			{
				Dictionary<Field, List<SummaryResult>> sourceFieldMap = new Dictionary<Field, List<SummaryResult>>( );
				int summaryCount = 0;

				foreach ( SummaryResult summary in _list )
				{
					Field sourceField = summary.SourceField;
					if ( null != sourceField )
					{
						List<SummaryResult> summaryList;
						if ( !sourceFieldMap.TryGetValue( sourceField, out summaryList ) )
							sourceFieldMap[sourceField] = summaryList = new List<SummaryResult>( );

						summaryList.Add( summary );
						summaryCount++;
					}
				}

				SummaryResult[] sourceFieldArr = new SummaryResult[summaryCount];
				Dictionary<Field, GridUtilities.Range> rangeMap = new Dictionary<Field, GridUtilities.Range>( );

				// SSP 9/19/2011 TFS88364
				// 
				bool hasCalculationFieldFromDifferentFieldLayout = false;

				int counter = 0;
				foreach ( KeyValuePair<Field, List<SummaryResult>> ii in sourceFieldMap )
				{
					int start = counter;
					foreach ( SummaryResult jj in ii.Value )
						sourceFieldArr[counter++] = jj;

					int end = counter - 1;

					rangeMap[ii.Key] = new GridUtilities.Range( start, end );

					// SSP 9/19/2011 TFS88364
					// 
					hasCalculationFieldFromDifferentFieldLayout = _cachedHasCalculationFieldFromDifferentFieldLayout || ii.Key.Owner != _fieldLayout;
				}

				_cachedSortedResultsArr = sourceFieldArr;
				_cachedSourceFieldMap = rangeMap;

				// SSP 9/19/2011 TFS88364
				// 
				_cachedHasCalculationFieldFromDifferentFieldLayout = hasCalculationFieldFromDifferentFieldLayout;
			}
		}

		#endregion // VerifySourceFieldMap

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region IList Members

		int IList.Add( object value )
		{
			throw new NotSupportedException( );
		}

		void IList.Clear( )
		{
			throw new NotSupportedException( );
		}

		bool IList.Contains( object item )
		{
			return this.Contains( item as SummaryResult );
		}

		int IList.IndexOf( object value )
		{
			return this.IndexOf( value as SummaryResult );
		}

		void IList.Insert( int index, object value )
		{
			throw new NotSupportedException( );
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Returns true since this collection is read-only.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryResultCollection</b> is read-only. It contains a <see cref="SummaryResult"/> object for each
		/// <see cref="SummaryDefinition"/> object in the associated <see cref="SummaryDefinitionCollection"/>.
		/// </para>
		/// <para class="body">
		/// To add summaries, use the FieldLayout's <see cref="FieldLayout.SummaryDefinitions"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		public bool IsReadOnly
		{
			get { return true; }
		}

		void IList.Remove( object value )
		{
			throw new NotSupportedException( );
		}

		void IList.RemoveAt( int index )
		{
			throw new NotSupportedException( );
		}

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException( );
			}
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo( Array array, int arrayIndex )
		{
			this.VerifyCollection( );
			((IList)_list).CopyTo( array, arrayIndex );
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator( )
		{
			IEnumerable<SummaryResult> ee = this;
			return ee.GetEnumerator( );
		}

		#endregion

		#region IList<SummaryResult> Members

		void IList<SummaryResult>.Insert( int index, SummaryResult item )
		{
			throw new NotSupportedException( );
		}

		void IList<SummaryResult>.RemoveAt( int index )
		{
			throw new NotSupportedException( );
		}

		#endregion

		#region ICollection<SummaryResult> Members

		void ICollection<SummaryResult>.Add( SummaryResult item )
		{
			throw new NotSupportedException( );
		}

		void ICollection<SummaryResult>.Clear( )
		{
			throw new NotSupportedException( );
		}

		/// <summary>
		/// Copies items from this collection to the specified array.
		/// </summary>
		/// <param name="array">Arra to copy items to.</param>
		/// <param name="arrayIndex">The location in the array where to start copying items to.</param>
		public void CopyTo( SummaryResult[] array, int arrayIndex )
		{
			( (ICollection)this ).CopyTo( (object[])array, arrayIndex );
		}

		bool ICollection<SummaryResult>.Remove( SummaryResult item )
		{
			throw new NotSupportedException( );
		}

		#endregion

		#region IEnumerable<SummaryResult> Members

		IEnumerator<SummaryResult> IEnumerable<SummaryResult>.GetEnumerator( )
		{
			this.VerifyCollection( );
			return _list.GetEnumerator( );
		}

		#endregion
	}

	#endregion // SummaryResultCollection Class

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