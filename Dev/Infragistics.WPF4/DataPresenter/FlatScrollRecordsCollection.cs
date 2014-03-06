using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using System.Windows.Threading;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{

	// SSP 7/23/09 - NAS9.2 Enhanced grid-view
	// Added FlatScrollRecordsCollection class.
	// 
	#region FlatScrollRecordsCollection Class

	/// <summary>
	/// Represents records that correspond to scroll positions in the view.
	/// </summary>






	internal class FlatScrollRecordsCollection : PropertyChangeNotifier, IList, IList<Record>, INotifyCollectionChanged

	{
		#region Nested Data Structures

		#region Enumerator Class

		private class Enumerator : IEnumerator<Record>
		{
			private FlatScrollRecordsCollection _list;
			private int _index;
			private Record _current;

			internal Enumerator( FlatScrollRecordsCollection list )
			{
				_list = list;
				this.Reset( );
			}

			#region IEnumerator<Record> Members

			public Record Current
			{
				get
				{
					return _current;
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose( )
			{
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get
				{
					return _current;
				}
			}

			public bool MoveNext( )
			{
				_index++;

				if ( _index < _list.Count )
				{
					_current = _list[_index];
					return true;
				}

				return false;
			}

			public void Reset( )
			{
				_index = -1;
			}

			#endregion
		}

		#endregion // Enumerator Class

		#region EventHandlerHolder Class

		private class EventHandlerHolder
		{
			internal FlatScrollRecordsCollection _flatRecords;
			internal INotifyPropertyChanged _obj;
			internal PropertyChangedEventHandler _handler;

			internal EventHandlerHolder( FlatScrollRecordsCollection flatRecords, INotifyPropertyChanged obj )
			{
				_flatRecords = flatRecords;
				_obj = obj;
				_handler = new PropertyChangedEventHandler( this.OnPropertyChanged );
				obj.PropertyChanged += _handler;
			}

			internal void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
			{
				string propName = e.PropertyName;
				switch ( propName )
				{
					case "Current":
						if ( _obj is RecordManager )
							_flatRecords.RaiseCollectionChanged_Reset( true );
						else
							Debug.Assert( false );

						break;
					case "ScrollCount":
					case "Item[]":
						if ( _obj is ViewableRecordCollection )
							_flatRecords.RaiseCollectionChanged_Reset( true );
						else
							Debug.Assert( false );

						break;
				}
			}

			internal void Unhook( )
			{
				_obj.PropertyChanged -= _handler;
			}
		}

		#endregion // EventHandlerHolder Class

		#region RecordsInViewGenerator Class

		/// <summary>
		/// A class that is used to get records that should be displayed on screen at a specific scroll position.
		/// </summary>
		public class RecordsInViewGenerator
		{
			#region Nested Data Structures

			#region TopFixedAdjustmentInfo Class

			private class TopFixedAdjustmentInfo
			{
				internal int _adjustment;
				internal Record _adjustmentRecord;

				internal TopFixedAdjustmentInfo( int adjustment, Record adjustmentRecord )
				{
					_adjustment = adjustment;
					_adjustmentRecord = adjustmentRecord;
				}
			}

			#endregion // TopFixedAdjustmentInfo Class

			#endregion // Nested Data Structures

			#region Member Vars

			private FlatScrollRecordsCollection _coll;
			private int _topScrollPos;
			private List<Record> _prefixRecords;
			private Record _firstRecord;
			private Dictionary<ViewableRecordCollection, TopFixedAdjustmentInfo> _adjustedVrc;
			private int _successiveScrollPos;

			/// <summary>
			/// Indicates whether to exclude top root fixed records from the prefix records
			/// and bottom root fixed records from the GetSuccessiveRecord method.
			/// </summary>
			private bool _excludeRootFixedRecords = true;

			/// <summary>
			/// Used for keeping track of records that were returned from GetSuccessiveRecord so we 
			/// can hook into these records' ViewableRecordCollections to get notified when anything 
			/// changes.
			/// </summary>
			internal List<Record> _successiveRecords;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="coll">Records from this FlatScrollRecordsCollection instance will be generated.</param>
			/// <param name="topScrollPos">Current top scroll position. This corresponds to the top-most scroll position</param>
			public RecordsInViewGenerator( FlatScrollRecordsCollection coll, int topScrollPos )
			{
				_coll = coll;
				_topScrollPos = topScrollPos;

				// Keep track of generators that were created during BeginMeasure/EndMeasure call.
				// Typically there should be only one.
				// 
				if ( null != _coll._generatorsInMeasure )
					_coll._generatorsInMeasure.Add( this );
			}

			#endregion // Constructor

			#region Methods

			#region Private Methods

			#region EnsureCalculated

			private void EnsureCalculated( )
			{
				if ( null == _firstRecord )
				{
					_prefixRecords = new List<Record>( );
					_adjustedVrc = new Dictionary<ViewableRecordCollection, TopFixedAdjustmentInfo>( );
					_firstRecord = this.GetRecordAtScrollPosHelper( _coll._vrc, _topScrollPos, true );
					_successiveScrollPos = 1 + _topScrollPos;
				}
			}

			#endregion // EnsureCalculated

			#region GetRecordAtScrollPosHelper

			private Record GetRecordAtScrollPosHelper( ViewableRecordCollection vrc, int scrollPos, bool calculating )
			{
				int totalRecords = vrc.ScrollCount;

				// We need to adjust the scrollPos to take into account nested fixed records when getting
				// the first record associated with the top scroll position. For successive records, we
				// need make sure that we adjust the scrollPos for the same vrc's that we did when we
				// got the first record for the top scroll pos. Therefore we are always adjusting the
				// scrollPos the first time and caching the vrc's so in successive calls, we adjust the
				// scroll pos for only those vrc's.
				// 
				TopFixedAdjustmentInfo adjustment = this.GetTopFixedRecordsAdjustmentHelper( vrc, calculating );
				
				// Make sure the scrollPos is not beyond the scroll count.
				// 
				if ( null != adjustment && scrollPos < totalRecords )
				{
					scrollPos += adjustment._adjustment;

					// Wrap the scrollPos back to top fixed records if the scrollPos that was specified
					// refers to one of the top fixed records.
					// 
					if ( scrollPos >= totalRecords )
						scrollPos -= totalRecords;
				}

				// Get the record whose descendant hierarchy contains the scrollPos.
				// 
				Record record = vrc.GetRecordContainingScrollIndex( ref scrollPos );
				bool recordOccupiesScrollPos = null != record && record.OccupiesScrollPosition;

				bool isVrcRoot = _coll._vrc == vrc;

				// MD 6/1/10 - ChildRecordsDisplayOrder feature
				// Refactored and moved inside the if block below where we already know the record is not null.
				//// If we are to not include root fixed records then check to see if the record is a root
				//// fixed record and if so then return null.
				//// 
				//if ( null != record && _excludeRootFixedRecords && isVrcRoot 
				//    && 0 == scrollPos && this.IsRootFixedRecord( record ) )
				//    return null;

				//Debug.Assert( isVrcRoot || null != record );
				if ( null != record )
				{
					// MD 5/26/10 - ChildRecordsDisplayOrder feature
					// Cache the value of AreChildrenAfterParent so we don't have to get it multiple times.
					bool areChildrenAfterParent = record.AreChildrenAfterParent;

					// MD 6/1/10 - ChildRecordsDisplayOrder feature
					// Moved and refactored this code from above.
					// If we are to not include root fixed records then check to see if the record is a root
					// fixed record and if so then return null.
					if (_excludeRootFixedRecords && isVrcRoot && this.IsRootFixedRecord(record))
					{
						if (areChildrenAfterParent || record.IsExpanded == false)
						{
							// If the children are after the parent or the record is not expanded, the scroll position of 0 
							// will point to the fixed record.
							if (scrollPos == 0)
								return null;
						}
						else
						{
							// Otherwise, if children are after the parent and it is expanded, the fixed record will appear 
							// after all the children.
							// JJD 09/22/11  - TFS84708 - Optimization
							// Use ViewableChildRecordsIfNeeded instead
							//ViewableRecordCollection childVrc = record.ViewableChildRecords;
							//if (scrollPos == childVrc.ScrollCount)
							ViewableRecordCollection childVrc = record.ViewableChildRecordsIfNeeded;
							if (childVrc != null)
							{
								if (scrollPos == childVrc.ScrollCount)
									return null;
							}
							else
							{
								if (scrollPos == 0)
									return null;
							}
						}
					}

					// Populate the prefixRecords with the top fixed records that are to be displayed before
					// the scrollable record associated with the scrollPos.
					// 
					if ( calculating && ( !_excludeRootFixedRecords || !isVrcRoot ) && null != adjustment )
					{
						int startIndex = _prefixRecords.Count;
						bool recordEncountered = false;

						for ( int i = 0; i < adjustment._adjustment; i++ )
						{
							Record ii = vrc[i];
							// SSP 8/19/09 TFS20681
							// This is to cover the case where a top fixed record is expanded, in which case we
							// treat it as scrolling record for the purposes of scroll index calculations however
							// we include it in prefix records to fix it on screen.
							// 
							//if ( ii == record )
							if ( ii == record && ( adjustment._adjustmentRecord != record 
								|| 0 == scrollPos && recordOccupiesScrollPos ) )
							{
								recordEncountered = true;
								break;
							}

							if ( null != ii && FixedRecordLocationInternal.Top == ii.FixedRecordLocationOverride )
							{
								_prefixRecords.Add( ii );
							}
							else
							{
								Debug.Assert( ii == record, "Something wrong with fixed records on top!" );
								break;
							}
						}

						if ( recordEncountered && startIndex < _prefixRecords.Count )
							_prefixRecords.RemoveRange( startIndex, _prefixRecords.Count - startIndex );
					}

					// MD 5/26/10 - ChildRecordsDisplayOrder feature
					// We only want to return the record for a 0 index or decrement the index otherwise when the child records 
					// are displayed after the parent. Otherwise, we need to look at the child records first.
					//if ( recordOccupiesScrollPos )
					if (recordOccupiesScrollPos && areChildrenAfterParent)
					{
						// If the scrollPos is associated with the record itself, then return the record.
						// 
						if ( 0 == scrollPos )
							return record;

						// Otherwise decrement the scrollPos by 1 to get the relative scrollPos within
						// the child records hierarchy of the record.
						// 
						scrollPos--;
					}

					// Now get the descendant record associated with the relative scrollPos.
					// 
					Debug.Assert( scrollPos >= 0 );

					// MD 5/26/10
					// Found while implementing the ChildRecordsDisplayOrder feature.
					// We should only be looking at the children when the record is expanded.
					//if ( scrollPos >= 0 )
					if (scrollPos >= 0 && record.IsExpanded)
					{
						// JJD 09/22/11  - TFS84708 - Optimization
						// Use ViewableChildRecordsIfNeeded instead
						//ViewableRecordCollection childVrc = record.ViewableChildRecords;
						ViewableRecordCollection childVrc = record.ViewableChildRecordsIfNeeded;

						// AS 2/22/11 NA 2011.1 Word Writer
						// Found this while debugging. We can get here for an expandablefieldrecord 
						// whose field is not expandable by default but whose IsExpandable is set 
						// to true.
						//
						if (childVrc == null)
						{
							Debug.Assert(scrollPos == 0, "Should we be returning this record?");
							return record;
						}

						Debug.Assert(null != childVrc, "No child vrc!");

						// MD 5/26/10 - ChildRecordsDisplayOrder feature
						// If the index we are looking for is one greater than the max scroll index we can get to in the 
						// child records collection, and the parent is being displayed after the child records,return the parent.
						if (recordOccupiesScrollPos &&
							areChildrenAfterParent == false &&
							scrollPos == childVrc.ScrollCount)
						{
							return record;
						}

						return this.GetRecordAtScrollPosHelper( childVrc, scrollPos, calculating );
					}
				}

				return record;
			}

			#endregion // GetRecordAtScrollPosHelper

			#region GetTopFixedRecordsAdjustmentHelper

			private TopFixedAdjustmentInfo GetTopFixedRecordsAdjustmentHelper( ViewableRecordCollection vrc, bool calculating )
			{
				TopFixedAdjustmentInfo info;
				if ( _adjustedVrc.TryGetValue( vrc, out info ) || !calculating )
					return info;

				int topFixedRecords = vrc.CountOfFixedRecordsOnTop;
				int adjustment = topFixedRecords;
				Record adjustmentRecord = null;
				if ( topFixedRecords > 0 )
				{
					Record record = vrc[topFixedRecords - 1];
					if ( record.ScrollCountInternal > 1 )
						adjustmentRecord = record;
				}

				if ( adjustment > 0 || null != adjustmentRecord )
				{
					info = new TopFixedAdjustmentInfo( adjustment, adjustmentRecord );
					_adjustedVrc[vrc] = info;
					return info;
				}

				return null;
			}

			#endregion // GetTopFixedRecordsAdjustmentHelper

			#region IsRootFixedRecord

			/// <summary>
			/// Returns true if the specified record is a top or bottom fixed record from the root record collection.
			/// </summary>
			/// <param name="record">Record to check if it's a root fixed record.</param>
			/// <returns>True if the record is a root fixed record, false otherwise.</returns>
			private bool IsRootFixedRecord( Record record )
			{
				FixedRecordLocationInternal fixedLoc = record.FixedRecordLocationOverride;
				return ( FixedRecordLocationInternal.Top == fixedLoc || FixedRecordLocationInternal.Bottom == fixedLoc )
					&& record.ParentCollection == _coll._vrc.RecordCollection;
			}

			#endregion // IsRootFixedRecord

			#endregion // Private Methods

			#region Public Methods

			#region GetSuccessiveRecord

			/// <summary>
			/// Returns the next record that is to be displayed in the view.
			/// </summary>
			/// <returns>Next record to be displayed. Null if no remaining records to display.</returns>
			public Record GetSuccessiveRecord( )
			{
				this.EnsureCalculated( );

				Record record;

				for ( ; ; )
				{
					record = this.GetRecordAtScrollPosHelper( _coll._vrc, _successiveScrollPos, false );
					_successiveScrollPos++;

					if ( null == record || !_prefixRecords.Contains( record ) )
						break;
				}

				// Keep track of records that were returned from GetSuccessiveRecord so we can hook into
				// these records' ViewableRecordCollections to get notified when anything changes.
				// 
				if ( null != record )
				{
					if ( null == _successiveRecords )
						_successiveRecords = new List<Record>( );

					_successiveRecords.Add( record );
				}

				return record;
			}

			#endregion // GetSuccessiveRecord

			#endregion // Public Methods

			#endregion // Methods

			#region Properties

			#region Public Properties

			#region PrefixRecords

			/// <summary>
			/// Returns the records that should be displayed before the FirstRecord.
			/// </summary>
			public List<Record> PrefixRecords
			{
				get
				{
					this.EnsureCalculated( );

					return _prefixRecords;
				}
			}

			#endregion // PrefixRecords

			#region FirstRecord

			/// <summary>
			/// Returns the record associated with the top scroll position. This essentially is the
			/// first record that is to be displayed after all the prefix records.
			/// </summary>
			public Record FirstRecord
			{
				get
				{
					this.EnsureCalculated( );

					return _firstRecord;
				}
			}

			#endregion // FirstRecord

			#endregion // Public Properties

			#endregion // Properties
		}

		#endregion // RecordsInViewGenerator Class

		#endregion // Nested Data Structures

		#region Member Vars

		private RecordManager _recordManager;
		private ViewableRecordCollection _vrc;
		private SparseArray _headers;
		private RecordCollectionBase _headerRecordsParentCollection;
		private int _beginMeasureCount;
		private bool[] _usedHeaders;
		private List<HeaderRecord> _headersUsedInLastMeasure;
		private List<PropertyValueTracker> _pvtTrackers = new List<PropertyValueTracker>( );
		private List<EventArgs> _pendingNotifications;
		private int _desiredHeaderCount = 40;
		private FieldLayoutCollection _lastHookedFieldLayoutsColl;

		/// <summary>
		/// Used for hooking into field layout's special records version as well as summary and filter version.
		/// </summary>
		private WeakDictionary<FieldLayout, List<PropertyValueTracker>> _fieldLayoutPvtTrackers = new WeakDictionary<FieldLayout, List<PropertyValueTracker>>( true, false );

		/// <summary>
		/// Used for hooking into record managers and viewable record collections of last generated records.
		/// Keys are either RecordManager or ViewableRecordCollection instances.
		/// </summary>
		private WeakDictionary<object, EventHandlerHolder> _hookedIntoObjects = new WeakDictionary<object, EventHandlerHolder>( true, false );

		/// <summary>
		/// Used for keeping track of generators that were created during BeginMeasure/EndMeasure call.
		/// Typically there should be only one.
		/// </summary>
		private List<RecordsInViewGenerator> _generatorsInMeasure;

		// MD 6/28/11 - TFS75111





		private bool _pendingNotificationsHasReset;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="recordManager">Record manager.</param>
		internal FlatScrollRecordsCollection( RecordManager recordManager )
		{
			GridUtilities.ValidateNotNull( recordManager );
			_recordManager = recordManager;

			DataPresenterBase dp = recordManager.DataPresenter;
			GridUtilities.ValidateNotNull( dp );

            this.VerifyViewableRecords( );

			PropertyValueTracker pvt = new PropertyValueTracker( _recordManager, "Current", this.VerifyViewableRecords );
			_pvtTrackers.Add( pvt );

			// SSP 8/28/09 TFS21591
			// Added ScrollCountRecalcVersionProperty on the data presenter which gets bumped whenever
			// something that affects the way we do scroll count calculations changes, like IsExpandable
			// property of a field.
			// 
			pvt = new PropertyValueTracker( dp, DataPresenterBase.ScrollCountRecalcVersionProperty, this.RaiseCollectionChanged_ResetAsync );
			_pvtTrackers.Add( pvt );

			_headers = new SparseArray( );

			this.HookIntoFieldLayouts( );
		}

		#endregion // Constructor

		#region Base Overrides

		#region OnHasListenersChanged

		/// <summary>
		/// Overridden. Called when the value of the HasListeners property changes.
		/// </summary>
		protected override void OnHasListenersChanged( )
		{
			base.OnHasListenersChanged( );

			this.OnHasListenersChangedInternal( );
		}

		#endregion // OnHasListenersChanged

		#endregion // Base Overrides

		#region Methods

		#region Public Methods

		#region IndexOf

		/// <summary>
		/// Returns the index of the specified record in this collection.
		/// </summary>
		/// <param name="record">Record whose index to return.</param>
		/// <returns>Index in the collection where the record is located.</returns>
		public int IndexOf( Record record )
		{
			// JJD 11/11/11 - TFS91364
			// Check the new _vrcPlaceholderRecord member
			//if ( record is HeaderRecord )
			if ( ( record is HeaderRecord ) && ((HeaderRecord)record)._vrcPlaceholderRecord == null )
			{
				int index = _headers.IndexOf( record );
				if ( index >= 0 )
					index += _vrc.ScrollCount;

				return index;
			}


			// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
			// 
			
			if ( record.ParentCollection == record.RecordManager.Sorted && record.RecordManager.HasGroups )
				return -1;

			return record.OverallScrollPosition;
		}

		#endregion // IndexOf

		#region Contains

		/// <summary>
		/// Returns a value indicating whether the specified record exists in the collection.
		/// </summary>
		/// <param name="record">Record instance to check if it exists in collection.</param>
		/// <returns>True if the record exists in the collection, false otherwise.</returns>
		public bool Contains( Record record )
		{
			return this.IndexOf( record ) >= 0;
		}

		#endregion // Contains

		#region CopyTo

		/// <summary>
		/// Copies the items in this collection to the specified array.
		/// </summary>
		/// <param name="array">Array to which to copy the items.</param>
		/// <param name="arrayIndex">Location in the array where to start copying items to.</param>
		public void CopyTo( Record[] array, int arrayIndex )
		{
			for ( int i = 0; i < this.Count; i++ )
				array[arrayIndex++] = this[i];
		}

		#endregion // CopyTo

		#endregion // Public Methods

		#region Private/Internal Methods

		#region OnHasListenersChangedInternal

		private void OnHasListenersChangedInternal( )
		{
			if ( !this.HasListeners )
			{
				if ( null != _pendingNotifications )
				{
					_pendingNotifications.Clear( );

					// MD 6/28/11 - TFS75111
					// Reset the flag which indicates whether the _pendingNotifications collection has a reset notification, 
					// because now it doesn't.
					_pendingNotificationsHasReset = false;
				}
			}
		}

		#endregion // OnHasListenersChangedInternal

		#endregion // Private/Internal Methods

		#region Internal Methods

		#region BeginMeasure

		/// <summary>
		/// For internal use only. Used to start caching for GetNextHeaderRecord method.
		/// </summary>
        public void BeginMeasure()
        {
            this.BeginMeasure(null);
		}

		internal void BeginMeasure( GridViewPanelFlat.GenerationCache generatorCache )
		{
			Debug.Assert( 0 == _beginMeasureCount, "BeginMeasure is getting called more than once without intervening EndMeasure call." );

			if ( 0 == _beginMeasureCount )
			{
                
                
                
				
				

				this.PopulateHeaderRecords( );

				// Pre-allocate the records that are going to be displayed. In case the logic
				// in InitializeRecord event handler is changing the visibility or expanding the 
				// record causing changes to the collection while measuring, which will cause
				// us to bail out of the record generation process and restart. This will
				// minimize the chances of that happening.
				// 
				if ( null != generatorCache )
					this.EnsureItemsAllocated( generatorCache );
				
                
                
                // Raise any pending notifications.
				// 
				this.RaisePendingNotifications( );

				// This is used to keep track of which header records have been used so far.
				// 
				_usedHeaders = new bool[this.CountOfHeaderRecords];

				_generatorsInMeasure = new List<RecordsInViewGenerator>( );
			}

			_beginMeasureCount++;
		}

		#endregion // BeginMeasure
            
		#region EndMeasure

		/// <summary>
		/// For internal use only. Used to start caching for GetNextHeaderRecord method.
		/// </summary>
		public void EndMeasure( )
		{
			if ( _beginMeasureCount > 0 )
			{
				_beginMeasureCount--;

				if ( 0 == _beginMeasureCount )
				{
					// SSP 9/1/09 
					// Added logic to maintain list of headers used during the last measure. This is
					// for the use by automation logic.
					// 
					_headersUsedInLastMeasure = this.CreateUsedHeadersList( _usedHeaders );

					this.CleanupHeaderRecords( );
					_usedHeaders = null;

					List<RecordsInViewGenerator> generatorsList = _generatorsInMeasure;
					_generatorsInMeasure = null;					
					this.HookIntoGeneratedRecordsHelper( generatorsList, true );
				}
			}
		}

		#endregion // EndMeasure

		#region GetNextHeaderRecord

		/// <summary>
		/// For internal use only. Only valid between BeginMeasure and EndMeasure calls.
		/// </summary>
		/// <param name="fieldLayout">Field layout for which to get the header record.</param>
		/// <param name="headerRecord">This out parameter will be assigned the header record.</param>
		/// <returns>The index in the collection where the returned header record belongs.</returns>
		public int GetNextHeaderRecord( FieldLayout fieldLayout, out HeaderRecord headerRecord )
		{
			headerRecord = null;
			Debug.Assert( _beginMeasureCount > 0, "GetNextHeaderRecord method call is only valid between BeginMeasure and EndMeasure calls." );
			if ( _beginMeasureCount <= 0 )
				return -1;

			for ( int pass = 0; pass < 2; pass++ )
			{
				for ( int i = 0, count = _headers.Count; i < count; i++ )
				{
					if ( !_usedHeaders[i] )
					{
						HeaderRecord ii = (HeaderRecord)_headers[i];
						if ( null == ii || 1 == pass && ii._isPlaceHolder )
							_headers[i] = ii = new HeaderRecord( fieldLayout, _headerRecordsParentCollection );

						if ( ii.FieldLayout == fieldLayout )
						{
							_usedHeaders[i] = true;
                            ii._isPlaceHolder = false;
							headerRecord = ii;
							return _vrc.ScrollCount + i;
						}
					}
				}
			}

			Debug.Assert( false, "Headers exhaused." );

			_desiredHeaderCount *= 2;
			this.RaiseCollectionChanged_Reset( true );

			return -1;
		}

		#endregion // GetNextHeaderRecord

        #region GetRecordAtScrollPosition

        internal Record GetRecordAtScrollPosition(int scrollPosition)
        {
            Debug.Assert(this.CountOfNonHeaderRecords == 0 || scrollPosition >= 0 && scrollPosition < this.CountOfNonHeaderRecords, "Invalid scroll position");

            if (scrollPosition >= 0 && scrollPosition < this.CountOfNonHeaderRecords)
                return this.GetRecordsInViewGenerator(scrollPosition).FirstRecord;

            return null;
        }

        #endregion //GetRecordAtScrollPosition	
    
		#region GetRecordsInViewGenerator

		/// <summary>
		/// Returns a new RecordsInViewGenerator instance used for getting records that are
		/// to be displayed when the current scrollbar position is 'topScrollPosition'. Note
		/// that this method will always return an instance of RecordsInViewGenerator even
		/// if the specified scroll position is invalid, however in that case the returned
		/// generator will return null from its methods for getting records.
		/// </summary>
		/// <param name="topScrollPosition">Current scrollbar position.</param>
		/// <returns>A new RecordsInViewGenerator instance.</returns>
		public RecordsInViewGenerator GetRecordsInViewGenerator( int topScrollPosition )
		{
			return new RecordsInViewGenerator( this, topScrollPosition );
		}

		#endregion // GetRecordsInViewGenerator

        #region GetScrollPositionOfRecord

        internal int GetScrollPositionOfRecord(Record record)
        {
            Debug.Assert(record != null, "Record is null in call to GetScrollPositionOfRecord");
            if (record == null)
                return -1;

            int indexInFlatList = this.IndexOf(record);

            if (indexInFlatList < 0)
                return -1;

            int adjustment = 0;
           
            Record parentRecord = record.ParentRecord;

            // check if record s a root record
            if (parentRecord == null)
            {
                // adjust based on index in list
                if (record.IsFixed)
                {
                    if (record.IsOnTopWhenFixed)
                        return this.Count - (indexInFlatList + 1);
                    else
                        return indexInFlatList - this.CountOfFixedRecordsOnTop;
                }
                else
                    adjustment -= this.CountOfFixedRecordsOnTop;
            }
            else
            {
                // walk up the ancestor chain and calculate any fixed record adjustments
                while (record != null)
                {
                    ViewableRecordCollection vrc = this.GetVrc(record);

                    parentRecord = record.ParentRecord;

                    if (parentRecord == null)
                    {
                        if (!record.IsFixed)
                            adjustment -= vrc.CountOfFixedRecordsOnTop;
                    }
                    else
                    if (record.IsFixed && record.IsOnTopWhenFixed)
                        adjustment += vrc.Count - vrc.CountOfFixedRecordsOnTop;
                    else
                        adjustment -= vrc.CountOfFixedRecordsOnTop;

                    record = parentRecord;
                }
            }

            return indexInFlatList + adjustment;
        }

        #endregion //GetScrollPositionOfRecord	

		#region HookIntoGeneratedRecordsHelper

		private void HookIntoGeneratedRecordsHelper( List<RecordsInViewGenerator> generatorList, bool unhookFromPreviousRecords )
		{
			WeakDictionary<object, EventHandlerHolder> hookedIntoObjects = _hookedIntoObjects;

			HashSet set = new HashSet( );

			for ( int i = 0, count = generatorList.Count; i < count; i++ )
			{
				RecordsInViewGenerator generator = generatorList[i];
				IEnumerable<Record> generatedRecords = new GridUtilities.AggregateEnumerable<Record>(
				generator.PrefixRecords,
					new Record[] { generator.FirstRecord },
					generator._successiveRecords );

				foreach ( Record ii in generatedRecords )
				{
					ViewableRecordCollection vrc = this.GetVrc( ii );
					if ( null != vrc )
					{
						set.Add( vrc );

						RecordManager rm = vrc.RecordManager;
						if ( null != rm )
							set.Add( rm );

						// SSP 11/4/11 TFS84216
						// 
						DataRecord dr = ii as DataRecord;
						if ( null != dr && dr.IsExpanded )
							this.AddChildVrcs( dr, set );
					}
				}
			}

			if ( unhookFromPreviousRecords )
			{
				List<EventHandlerHolder> objectsToUnhookFrom = null;

				foreach ( KeyValuePair<object, EventHandlerHolder> ii in hookedIntoObjects )
				{
					if ( !set.Exists( ii.Key ) )
					{
						if ( null == objectsToUnhookFrom )
							objectsToUnhookFrom = new List<EventHandlerHolder>( );

						objectsToUnhookFrom.Add( ii.Value );
					}
				}

				if ( null != objectsToUnhookFrom )
				{
					for ( int i = 0, count = objectsToUnhookFrom.Count; i < count; i++ )
					{
						EventHandlerHolder ii = objectsToUnhookFrom[i];
						ii.Unhook( );
						hookedIntoObjects.Remove( ii._obj );
					}
				}
			}

			foreach ( object ii in set )
			{
				if ( ! hookedIntoObjects.ContainsKey( ii ) )
				{
					Debug.Assert( ii is ViewableRecordCollection || ii is RecordManager );
					hookedIntoObjects[ii] = new EventHandlerHolder( this, (INotifyPropertyChanged)ii );
				}
			}
		}


		#endregion // HookIntoGeneratedRecordsHelper
    
		#region RaisePendingNotifications

		/// <summary>
		/// Raises pending change notifications if any. Returns true if there were pending
		/// notifications.
		/// </summary>
		/// <returns></returns>
		internal bool RaisePendingNotifications( )
		{
			List<EventArgs> list = _pendingNotifications;

			// MD 6/28/11 - TFS75111
			// Cache the value that indicates whether we have a pending reset notification.
			bool hasResetNotification = _pendingNotificationsHasReset;

			_pendingNotifications = null;

			// MD 6/28/11 - TFS75111
			// Reset the flag which indicates whether the _pendingNotifications collection has a reset notification, 
			// because now it doesn't.
			_pendingNotificationsHasReset = false;

			if ( null != list && list.Count > 0 )
			{
				NotifyCollectionChangedEventArgs resetNotificationEventArgs = null;

				// MD 6/28/11 - TFS75111
				// Wrapped this code in an if block. We only want to try to find the pending reset notification if we 
				// know we have one.
				if (hasResetNotification)
				{
				foreach ( EventArgs ii in list )
				{
					if ( ii is NotifyCollectionChangedEventArgs
						&& NotifyCollectionChangedAction.Reset == ( (NotifyCollectionChangedEventArgs)ii ).Action )
					{
						resetNotificationEventArgs = (NotifyCollectionChangedEventArgs)ii;
						break;
					}
				}
				}

				if ( null != resetNotificationEventArgs )
				{
					this.RaiseChangeEvent( resetNotificationEventArgs, false );
				}
				else
				{
					foreach ( EventArgs ii in list )
						this.RaiseChangeEvent( ii, false );
				}

				return true;
			}

			return false;
		}

		#endregion // RaisePendingNotifications

		#endregion // Internal Methods

		#region Private Methods

		#region AddChildVrcs

		// SSP 11/4/11 TFS84216
		// 
		private void AddChildVrcs( DataRecord dr, HashSet set )
		{
			ExpandableFieldRecordCollection childRecords = dr.ChildRecordsIfAllocated;
			if ( null != childRecords )
			{
				foreach ( ExpandableFieldRecord iiChildRecord in childRecords )
				{
					ViewableRecordCollection iiChildVrc = iiChildRecord.ViewableChildRecordsIfNeeded;
					if ( null != iiChildVrc )
						set.Add( iiChildVrc );
				}
			}
		}

		#endregion // AddChildVrcs

		#region CleanupHeaderRecords

		private void CleanupHeaderRecords( )
		{
			for ( int i = 0; i < _headers.Count; i++ )
			{
				HeaderRecord hr = (HeaderRecord)_headers[i];
				if ( null != _usedHeaders && _usedHeaders[i] )
					continue;

				FieldLayout fl = hr != null ? hr.FieldLayout : null;

				// JJD 08/17/12 - TFS119037
				// If the fieldlayout was removed then replace the unused (i.e. placeholder) header with a new
				// one so we don't root an old fieldlayout
				if (fl != null && fl.WasRemovedFromCollection && null != _recordManager.FieldLayout && fl != _recordManager.FieldLayout)
				{
					hr = new HeaderRecord(_recordManager.FieldLayout, _headerRecordsParentCollection);
					hr._isPlaceHolder = true;
					_headers[i] = hr;
					continue;
				}
				
				if ( null == hr.AssociatedRecordPresenter )
					hr._isPlaceHolder = true;
			}
		}

		#endregion // CleanupHeaderRecords

		#region CreateUsedHeadersList

		
		
		
		private List<HeaderRecord> CreateUsedHeadersList( bool[] usedHeadersFlags )
		{
			List<HeaderRecord> usedHeadersList = new List<HeaderRecord>( );
			if ( null != usedHeadersFlags )
			{
				for ( int i = 0; i < usedHeadersFlags.Length; i++ )
				{
					if ( usedHeadersFlags[i] )
					{
						HeaderRecord headerRecord = _headers[i] as HeaderRecord;
						Debug.Assert( null != headerRecord );
						if ( null != headerRecord )
							usedHeadersList.Add( headerRecord );
					}
				}
			}

			return usedHeadersList;
		}

		#endregion // CreateUsedHeadersList

		#region EnsureItemsAllocated

		private void EnsureItemsAllocated( GridViewPanelFlat.GenerationCache generatorCache )
		{
			if ( null != generatorCache && generatorCache._estimatedRecordsToLayout > 0 )
			{
				this.EnsureItemsAllocated( generatorCache._currentScrollPosition, generatorCache._estimatedRecordsToLayout );
			}
		}

		private void EnsureItemsAllocated( int startIndex, int count )
		{
			// JJD 2/14/11 - TFS66166 - Optimization
			ViewableRecordCollection lastVrc = null;
			RecordManager lastRm = null;

			for (int i = 0; i < count; i++)
			{
				Record record = this[startIndex + i];
				if ( null == record )
					break;

				ViewableRecordCollection vrc = this.GetVrc( record );
				if (null != vrc)
				{
					// JJD 2/14/11 - TFS66166 - Optimization
					// Only call verify on a break in vrcs
					if (vrc != lastVrc)
					{
						vrc.Verify();
						lastVrc = vrc;

						RecordManager rm = vrc.RecordManager;
						if (null != rm)
						{
							// JJD 2/14/11 - TFS66166 - Optimization
							// Only call verify on a break in rms
							if (rm != lastRm)
							{
								rm.VerifySort();
								lastRm = rm;
							}
						}
					}
				}
			}
		}

		#endregion // EnsureItemsAllocated

		#region GetVrc

		private ViewableRecordCollection GetVrc( Record record )
		{
			if ( null != record )
			{
				RecordCollectionBase parentCollection = record.ParentCollection;
				if ( null != parentCollection )
				{
					ViewableRecordCollection vrc = parentCollection.ViewableRecords;
					Debug.Assert( null != vrc );

					return vrc;
				}
			}

			return null;
		}

		#endregion // GetVrc

		#region HookIntoFieldLayout

		private void HookIntoFieldLayout( FieldLayout fl )
		{
			if ( !_fieldLayoutPvtTrackers.ContainsKey( fl ) )
			{
				PropertyValueTracker pvt;
				List<PropertyValueTracker> flPvtList = new List<PropertyValueTracker>( );

				pvt = new PropertyValueTracker( fl, FieldLayout.SpecialRecordsVersionProperty,
					this.OnFieldLayout_SpecialRecordsVersionChanged );

				flPvtList.Add( pvt );

				_fieldLayoutPvtTrackers.Add( fl, flPvtList );
			}
		}

		#endregion // HookIntoFieldLayout

		#region HookIntoFieldLayouts

		private void HookIntoFieldLayouts( )
		{
			DataPresenterBase dp = _recordManager.DataPresenter;
			FieldLayoutCollection fieldLayouts = null != dp ? dp.FieldLayouts : null;

			if ( _lastHookedFieldLayoutsColl != fieldLayouts )
			{
				if ( null != _lastHookedFieldLayoutsColl )
					_lastHookedFieldLayoutsColl.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnFieldLayouts_CollectionChanged );

				fieldLayouts.CollectionChanged += new NotifyCollectionChangedEventHandler( OnFieldLayouts_CollectionChanged );
				_lastHookedFieldLayoutsColl = fieldLayouts;
			}

			for ( int i = 0, count = fieldLayouts.Count; i < count; i++ )
				this.HookIntoFieldLayout( fieldLayouts[i] );
		}

		#endregion // HookIntoFieldLayouts

		#region OnFieldLayouts_CollectionChanged

		private void OnFieldLayouts_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.HookIntoFieldLayouts( );
		}

		#endregion // OnFieldLayouts_CollectionChanged

		#region OnFieldLayout_SpecialRecordsVersionChanged

		private void OnFieldLayout_SpecialRecordsVersionChanged( )
		{
			this.RaiseCollectionChanged_Reset( true );
		}

		#endregion // OnFieldLayout_SpecialRecordsVersionChanged

		#region PopulateHeaderRecords

		private void PopulateHeaderRecords( )
		{
			if ( null == _headerRecordsParentCollection
				|| _headerRecordsParentCollection.FieldLayout != _recordManager.FieldLayout )
				_headerRecordsParentCollection = new RecordCollection( null, _recordManager, null );

			if ( _headers.Count < _desiredHeaderCount && null != _recordManager.FieldLayout )
			{
				for ( int i = _headers.Count; i < _desiredHeaderCount; i++ )
				{
					HeaderRecord hr = new HeaderRecord( _recordManager.FieldLayout, _headerRecordsParentCollection );
					hr._isPlaceHolder = true;
					_headers.Add( hr );
				}

				this.RaiseCollectionChanged_Reset( true );
			}
		}

		#endregion // PopulateHeaderRecords

		#region RaisePendingNotificationsCallback

		private void RaisePendingNotificationsCallback( )
		{
			this.RaisePendingNotifications( );
		}

		#endregion // RaisePendingNotificationsCallback

		#region OnVrc_PropertyChanged

		private void OnVrc_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			switch ( e.PropertyName )
			{
				case "Item[]":
				case "ScrollCount":
					this.RaiseCollectionChanged_Reset( true );
					break;
			}
		}

		#endregion // OnVrc_PropertyChanged

		#region RaiseCollectionChanged

		private void RaiseCollectionChanged_ResetAsync( )
		{
			this.RaiseCollectionChanged_Reset( true );
		}

		private void RaiseCollectionChanged_Reset( bool async )
		{
			NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );
			this.RaiseChangeEvent( eventArgs, async );
			this.RaiseChangeEvent( new PropertyChangedEventArgs( "Count" ), async );
		}

		/// <summary>
		/// Raises either CollectionChanged or PropertyChanged notification.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="async"></param>
		private void RaiseChangeEvent( EventArgs e, bool async )
		{
			if ( async )
			{
				// MD 6/28/11 - TFS75111
				// If we have a pending reset notification, don't add any more notifications to the pending list.
				if (_pendingNotificationsHasReset)
					return;

				if ( null == _pendingNotifications )
					_pendingNotifications = new List<EventArgs>( );

				// MD 6/28/11 - TFS75111
				// Cache a value indicating whether we should invoke the callback method.
				bool shouldBeginInvokeCallback = (_pendingNotifications.Count == 0);

				// MD 6/28/11 - TFS75111
				// If we are adding a reset notification to the collection, clear out all previous notifications and set a flag 
				// indicating that we shouldn't add any more notifications to the _pendingNotifications collection.
				NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = e as NotifyCollectionChangedEventArgs;
				if (notifyCollectionChangedEventArgs != null && notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
				{
					_pendingNotificationsHasReset = true;
					_pendingNotifications.Clear();
				}

				_pendingNotifications.Add( e );

				// MD 6/28/11 - TFS75111
				// Use the cache value, because we may have cleared the old items in the _pendingNotifications collection.
				//if ( 1 == _pendingNotifications.Count )
				if (shouldBeginInvokeCallback)
				{
					DataPresenterBase dp = _recordManager.DataPresenter;
					if ( null != dp )
					{
						Dispatcher dispatcher = dp.Dispatcher;
						if ( null != dispatcher )
							dispatcher.BeginInvoke( DispatcherPriority.Send, new GridUtilities.MethodDelegate( this.RaisePendingNotificationsCallback ) );
					}
				}
			}
			else
			{
				if ( e is PropertyChangedEventArgs )
					this.RaisePropertyChangedEvent( ( (PropertyChangedEventArgs)e ).PropertyName );
				else if ( e is NotifyCollectionChangedEventArgs )
					this.RaiseCollectionChangedHelper( (NotifyCollectionChangedEventArgs)e );
				else
					Debug.Assert( false, "Unknown type of event args." );
			}
		}

		private void RaiseCollectionChangedHelper( NotifyCollectionChangedEventArgs eventArgs )
		{
			if ( null != _collectionChanged )
				_collectionChanged( this, eventArgs );
		}

		#endregion // RaiseCollectionChanged

		#region VerifyViewableRecords

		private void VerifyViewableRecords( )
		{
			ViewableRecordCollection newVrc = _recordManager.ViewableRecords;
			if ( _vrc != newVrc )
			{
				if ( null != _vrc )
					_vrc.PropertyChanged -= new PropertyChangedEventHandler( OnVrc_PropertyChanged );

				_vrc = newVrc;

				if ( null != _vrc )
					_vrc.PropertyChanged += new PropertyChangedEventHandler( OnVrc_PropertyChanged );

				// If no one's hooked into the collection then don't bother enquing the notification
				// into pending event list.
				// 
				if ( this.HasListeners )
					this.RaiseCollectionChanged_Reset( true );
			}
		}

		#endregion // VerifyViewableRecords

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the count of the items in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return _vrc.ScrollCount + this.CountOfHeaderRecords;
			}
		}

		#endregion // Count

        #region CountOfFixedRecordsOnBottom

		/// <summary>
		/// Returns the number of root fixed records on bottom.
		/// </summary>
        public int CountOfFixedRecordsOnBottom
        {
            get
            {
                return this._vrc.CountOfFixedRecordsOnBottom;
            }
        }

        #endregion //CountOfFixedRecordsOnBottom	

        #region CountOfFixedRecordsOnTop

		/// <summary>
		/// Returns the number of root fixed records on top.
		/// </summary>
        public int CountOfFixedRecordsOnTop
        {
            get
            {
                return this._vrc.CountOfFixedRecordsOnTop;
            }
        }

        #endregion //CountOfFixedRecordsOnTop	

		#region HasPendingNotifications

		/// <summary>
		/// Indicates if the collection has been modified and has pending notifications that
		/// it needs to raise.
		/// </summary>
		public bool HasPendingNotifications
		{
			get
			{
				return null != _pendingNotifications && _pendingNotifications.Count > 0;
			}
		}

		#endregion // HasPendingNotifications

		#region CountOfHeaderRecords

		/// <summary>
		/// Number of headers at the end. Count property includes this value.
		/// </summary>
		public int CountOfHeaderRecords
		{
			get
			{
				return _headers.Count;
			}
        }

        #endregion // CountOfHeaderRecords

        #region CountOfNonHeaderRecords

		/// <summary>
		/// Returns the number of non-header records.
		/// </summary>
		public int CountOfNonHeaderRecords
        {
            get
            {
                return this._vrc.ScrollCount;
            }
		}

		#endregion // CountOfNonHeaderRecords

		#region Indexer

		/// <summary>
		/// Returns the record at the specified index.
		/// </summary>
		/// <param name="index">Record at this index will be returned.</param>
		/// <returns>The record at the specified index.</returns>
		public Record this[int index]
		{
			get
			{
				int scrollCount = _vrc.ScrollCount;
				if ( index < scrollCount )
					return _vrc.GetRecordAtScrollPosition( index );

				return (Record)_headers[index - scrollCount];
			}
			set
			{
				throw new NotImplementedException( );
			}
		}

		#endregion // Indexer

		#endregion // Public Properties

		#region Internal Properties

		#region HasListeners

		internal new bool HasListeners
		{
			get
			{
				return null != _collectionChanged || base.HasListeners;
			}
		}

		#endregion // HasListeners

		#region HeadersUsedInLastMeasure

		
		
		
		/// <summary>
		/// Returns a list of header records that were used in the last measure.
		/// </summary>
		internal List<HeaderRecord> HeadersUsedInLastMeasure
		{
			get
			{
				return _headersUsedInLastMeasure;
			}
		}

		#endregion // HeadersUsedInLastMeasure

		#region IsMeasuring

		/// <summary>
		/// Returns true if BeginMeasure has been called without a corresponding EndMeasure.
		/// </summary>
		internal bool IsMeasuring
		{
			get
			{
				return _beginMeasureCount > 0;
			}
		}

		#endregion // IsMeasuring

        #region RootViewableRecords

        internal ViewableRecordCollection RootViewableRecords { get { return this._vrc; } }

        #endregion //RootViewableRecords

		#region RecordManager

		internal RecordManager RecordManager
		{
			get
			{
				return _recordManager;
			}
		}

		#endregion // RecordManager

		#endregion // Internal Properties

		#endregion // Properties

		#region IList<Record> Members

		void IList<Record>.Insert( int index, Record item )
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		void IList<Record>.RemoveAt( int index )
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		#endregion

		#region ICollection<Record> Members

		void ICollection<Record>.Add( Record item )
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		void ICollection<Record>.Clear( )
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		bool ICollection<Record>.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		bool ICollection<Record>.Remove( Record item )
		{
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
		}

		#endregion

		#region IEnumerable<Record> Members

		/// <summary>
		/// Gets an enumerator that can be used to enumerate all the records in the collection.
		/// </summary>
		/// <returns>Enumerator for enumerating all the records of this collection.</returns>
		public IEnumerator<Record> GetEnumerator( )
		{
			return new Enumerator( this );
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}

		#endregion

		#region INotifyCollectionChanged Members

		private event NotifyCollectionChangedEventHandler _collectionChanged;

		/// <summary>
		/// Occurs when a changes are made to the colleciton, i.e. records are added, removed or the entire collection is reset.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add
			{
				_collectionChanged = Delegate.Combine( _collectionChanged, value ) as NotifyCollectionChangedEventHandler;
				this.OnHasListenersChangedInternal( );
			}
			remove
			{
				_collectionChanged = System.Delegate.Remove( _collectionChanged, value ) as NotifyCollectionChangedEventHandler;
				this.OnHasListenersChangedInternal( );
			}
		}

		#endregion

        #region IList Members

        int IList.Add(object value)
        {
            ((IList<Record>)this).Add(value as Record);

            return this.Count - 1;
        }

        void IList.Clear()
        {
            ((IList<Record>)this).Clear();
        }

        bool IList.Contains(object value)
        {
            return ((IList<Record>)this).Contains(value as Record);
        }

        int IList.IndexOf(object value)
        {
           return ((IList<Record>)this).IndexOf( value as Record);
        }

        void IList.Insert(int index, object value)
        {
            ((IList<Record>)this).Insert(index, value as Record);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        void IList.Remove(object value)
        {
            ((IList<Record>)this).Remove(value as Record);
        }

        void IList.RemoveAt(int index)
        {
            ((IList<Record>)this).RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = value as Record;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            for (int i = 0; i < this.Count; i++)
                array.SetValue(this[i], index++);
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
    }

	#endregion // FlatScrollRecordsCollection Class
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