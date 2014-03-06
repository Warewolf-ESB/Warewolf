using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Linq;


using Infragistics.Services;
using Infragistics.Collections.Services;
using Infragistics.Controls.Schedules.Primitives.Services;

namespace Infragistics.Controls.Schedules.Services






{


#region Infragistics Source Cleanup (Region)

























































#endregion // Infragistics Source Cleanup (Region)


	internal class OccurrenceManager<TActivity> : PropertyChangeNotifierExtended, IViewItemFactory<ActivityBase>
		where TActivity : ActivityBase, new( )
	{
		#region PropertyStorage Class

		internal class PropertyStorage : IPropertyStorage<ActivityBase, int>
		{
			private OccurrenceManager<TActivity> _manager;
			private IMap<int, IPropertyStore> _varianceStores;

			internal PropertyStorage( OccurrenceManager<TActivity> manager )
			{
				_manager = manager;
				_varianceStores = MapsFactory.CreateMapHelper<int, IPropertyStore>( );
			}

			private IPropertyStore GetStore( int property, bool allocateIfNecessary )
			{
				var store = _varianceStores[property];
				if ( null == store && allocateIfNecessary )
				{
					StoragePropsInfo.PropInfo propInfo = _manager._propsInfo.Props[property];
					_varianceStores[property] = store = propInfo.CreateStore( );

					// When a property of an occurrence is modified, we need to raise notifications on the occurrence.
					// Therefore hook into the variance store and route the property change notification to the activity.
					// 
					ISupportValueChangeNotifications<ActivityBase> storeNotifier = store as ISupportValueChangeNotifications<ActivityBase>;
					if ( null != storeNotifier )
					{
						var t = new ValueChangeListener<StoragePropsInfo.PropInfo, ActivityBase>( propInfo,
							( p, activity ) =>
							{
								var listener = _manager._propsInfo as ITypedPropertyChangeListener<ActivityBase, int>;
								if ( null != listener )
									listener.OnPropertyValueChanged( activity, p._key, null );
							}
						);

						storeNotifier.AddListener( t );
					}
				}

				return store;
			}

			private ActivityBase GetTemplateActivity( ActivityBase occurrenceInstance )
			{
				return occurrenceInstance.RootActivity;
			}

			internal IEnumerable<ActivityBase> GetOccurrences( ActivityBase root )
			{
				
				return from ii in _manager._map.Values where null != ii && ii.RootActivity == root select ii;
			}

			internal bool IsSynchronizedWithRoot( int property )
			{
				return _manager._propsInfo._rootSynchronizedVarianceDataProps[property];
			}

			private void SynchronizePropertyToRootHelper( ActivityBase root, ActivityBase occurrence, int prop )
			{
				if ( occurrence.IsVariance )
				{
					object rootValue = root.GetValueHelper<object>( prop );
					if ( ! occurrence.IsVariantPropertyFlagSet( prop, true ) )
						occurrence.SetValueHelper( prop, rootValue );
				}
				else
				{
					occurrence.PropsInfo.OnPropertyValueChanged( occurrence, prop, null );
				}
			}

			
			internal void OnRootPropertyChanged( ActivityBase root, string property )
			{
				int prop = root.PropsInfo.GetPropFromName( property );
				if ( prop >= 0 && this.IsSynchronizedWithRoot( prop ) )
				{
					foreach ( ActivityBase ii in this.GetOccurrences( root ) )
						this.SynchronizePropertyToRootHelper( root, ii, prop );
				}
				else if ( "IsDeleted" == property )
				{
					foreach ( ActivityBase ii in this.GetOccurrences( root ) )
						ii.RaisePropertyChangedEvent( property );
				}
			}

			public T GetValue<T>( ActivityBase activity, int property )
			{
				var store = this.GetStore( property, false );
				if ( ! this.IsSynchronizedWithRoot( property ) || null != store && store.HasValue( activity ) )
					return null != store ? (T)store.GetValue( activity ) : default( T );

				ActivityBase template = this.GetTemplateActivity( activity );
				Debug.Assert( null != template );
				return null != template ? template.Storage.GetValue<T>( template, property ) : default( T );
			}

			public void SetValue<T>( ActivityBase activity, int property, T newValue )
			{
				// If the the property is a variance property and it's value is the same as the root
				// then don't bother setting the property value. Without this check we end up creating
				// unnecessary entries in the variance stores for activities whose values are the same
				// as root, for example during property value merging process when an occurrence is made
				// variance and the occurrence's data item is initialized.
				// 
				if ( this.IsSynchronizedWithRoot( property ) )
				{
					T oldVal = this.GetValue<T>( activity, property );
					if ( EqualityComparer<T>.Default.Equals( oldVal, newValue ) )
						return;
				}

				var store = this.GetStore( property, true );
				store.SetValue( activity, newValue );
			}

			public bool HasValue( ActivityBase activity, int property )
			{
				IPropertyStore store = _varianceStores[property];
				if ( null != store && store.HasValue( activity ) )
					return true;
				else if ( ! this.IsSynchronizedWithRoot( property ) )
					return false;

				ActivityBase template = this.GetTemplateActivity( activity );
				//Debug.Assert( null != template );
				return null != template && template.Storage.HasValue( template, property );
			}
		}

		#endregion // PropertyStorage Class

		#region Member Vars

		internal readonly IScheduleDataConnector _connector;
		/// <summary>
		/// Contains occurrences, including variances.
		/// </summary>
		private WeakDictionary<OccurrenceId, ActivityBase> _map;
		/// <summary>
		/// Contains variances.
		/// </summary>
		private WeakDictionary<OccurrenceId, ActivityBase> _variances;
		internal readonly PropertyStorage _storage;
		private ActivityItemManager<TActivity> _mainViewItemManager;
		private ActivityBase.StorageProps.Info _propsInfo;
		/// <summary>
		/// Used to cache the start time of the next occurrence in the series that's due for reminder. Keys are root activities.
		/// </summary>
		private WeakDictionary<ActivityBase, DateTime> _latestNonFutureOccurrenceTimes;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connector">Schedule data connector.</param>
		/// <param name="mainViewItemManager">Item manager that keeps track of root activities and variances.</param>
		/// <param name="propsInfo">Properties information for <typeparamref name="TActivity"/> class.</param>
		internal OccurrenceManager( IScheduleDataConnector connector, ActivityItemManager<TActivity> mainViewItemManager, ActivityBase.StorageProps.Info propsInfo )
		{
			CoreUtilities.ValidateNotNull( connector );
			CoreUtilities.ValidateNotNull( mainViewItemManager );
			CoreUtilities.ValidateNotNull( propsInfo );

			_connector = connector;
			_mainViewItemManager = mainViewItemManager;
			_propsInfo = propsInfo;

			
			
			
			_map = new WeakDictionary<OccurrenceId, ActivityBase>( false, true );
			_variances = new WeakDictionary<OccurrenceId, ActivityBase>( false, true );
			_storage = new PropertyStorage( this );
		} 

		#endregion // Constructor

		#region LocalTimeZone

		
		
		internal TimeZoneToken LocalTimeZone
		{
			get
			{
				return _connector.TimeZoneInfoProviderResolved.LocalToken;
			}
		} 

		#endregion // LocalTimeZone

		#region UtcTimeZone

		
		
		internal TimeZoneToken UtcTimeZone
		{
			get
			{
				return _connector.TimeZoneInfoProviderResolved.UtcToken;
			}
		} 

		#endregion // UtcTimeZone

		#region GetRootActivity

		/// <summary>
		/// Gets the root activity with the specified id.
		/// </summary>
		/// <param name="id">Root activity id.</param>
		/// <returns>If a root activity by the specified id is found, returns it. Otherwise returns null.</returns>
		internal ActivityBase GetRootActivity( object id )
		{
			return _mainViewItemManager.GetViewItemWithId( id );
		} 

		#endregion // GetRootActivity

		#region GetRecurrenceCalculator

		/// <summary>
		/// Gets the recurrence calculator for the specified recurrence information.
		/// </summary>
		/// <param name="info">Contains recurrence information.</param>
		/// <returns>Calculator for calculating occurrences.</returns>
		internal DateRecurrenceCalculatorBase GetRecurrenceCalculator( RecurrenceInfo info )
		{
			var calcFactory = _connector.RecurrenceCalculatorFactoryResolved;
			return calcFactory.GetCalculator( info );
		}

		/// <summary>
		/// Gets the recurrence calculator for the specified root activity.
		/// </summary>
		/// <param name="root">Root activity.</param>
		/// <returns>Calculator for calculating occurrences.</returns>
		internal DateRecurrenceCalculatorBase GetRecurrenceCalculator( ActivityBase root )
		{
			var rr = this.GetRecurrenceInfo( root );
			Debug.Assert( null != rr );
			return null != rr ? this.GetRecurrenceCalculator( rr ) : null;
		}

		#endregion // GetRecurrenceCalculator

		#region CreateViewItem

		/// <summary>
		/// Creates or gets a cached occurrence instance for the specified dataItem which must be an OccurrenceId instance.
		/// </summary>
		/// <param name="dataItem">An OccurrenceId instance.</param>
		/// <returns>The occurrence activity.</returns>
		public ActivityBase CreateViewItem( object dataItem )
		{
			OccurrenceId id = (OccurrenceId)dataItem;

			ActivityBase activity;
			if ( !_map.TryGetValue( id, out activity ) )
			{
				activity = new TActivity( );
				activity.Initialize( _connector, _storage );
				DateTime originalDateTime = id._originalDateTime;
				ActivityBase root = id._template;
				if ( null == root )
				{
					Debug.Assert( false );
					id._template = root = this.GetRootActivity( id.RootId );
					Debug.Assert( null != root, "We must always have a template. This could be indicative of bad data, like root activity id property value being bad." );
				}

				activity.InitializeRootActivity( root, originalDateTime );
				activity.Id = id._id;
				activity.Start = originalDateTime;
				activity.End = originalDateTime.Add( null != root ? root.Duration : TimeSpan.Zero );

				this.InitializeReminderEnabled( activity, root );

				this.AddToMapHelper( id, activity );
			}

			return activity;
		} 

		#endregion // CreateViewItem

		#region GetOccurrenceThatsDueForReminder

		/// <summary>
		/// Gets the start time of the next occurrence that's due for reminder.
		/// </summary>
		/// <param name="root">Root activity that defines the series.</param>
		/// <returns>The start time of the next occurrence in the series that's due for reminder.</returns>
		private DateTime GetOccurrenceThatsDueForReminder( ActivityBase root )
		{
			if ( null == _latestNonFutureOccurrenceTimes )
				_latestNonFutureOccurrenceTimes = new WeakDictionary<ActivityBase, DateTime>( true, false );

			DateTime date;
			if ( !_latestNonFutureOccurrenceTimes.TryGetValue( root, out date ) )
			{
				date = root.Start;

				// If reminder mapping hasn't been specified or the reminder's LastDisplayedTime hasn't been
				// initialized yet then display the reminder for all the future occurrences as well as the 
				// last occurrence in the past.
				// 
				var calc = this.GetRecurrenceCalculator( root );
				if ( null != calc )
				{
					DateRange dateRange = new DateRange( root.Start, DateTime.UtcNow.Add( root.ReminderInterval ) );
					DateRange dr = calc.GetOccurrences( dateRange ).LastOrDefault( );

					date = dr.Start;
				}

				_latestNonFutureOccurrenceTimes[root] = date;
			}

			return date;
		} 

		#endregion // GetOccurrenceThatsDueForReminder

		#region InitializeReminderEnabled

		/// <summary>
		/// Initializes the ReminderEnabled property of the specified occurrence based on root
		/// activity's Reminder.LastDisplayedTime and if that's not available then based on the
		/// latest occurrence in the series that's overdue for the reminder.
		/// </summary>
		/// <param name="activity">Occurrence activity.</param>
		/// <param name="root">Root activity.</param>
		private void InitializeReminderEnabled( ActivityBase activity, ActivityBase root )
		{
			// Initialize ReminderEnabled based on the LastDisplayedTime of the root activity's Reminder object.
			// When an occurrence's reminder is dismissed, we don't make it a variance to store that information.
			// Instead we update the root activity's Reminder object's LastDisplayedTime.
			// 
			if ( root.ReminderEnabled )
			{
				Reminder reminder = null != root ? root.Reminder : null;
				bool reminderEnabled = false;

				if ( null != reminder && default( DateTime ) != reminder.LastDisplayedTime )
					reminderEnabled = activity.Start > reminder.LastDisplayedTime;
				else
					reminderEnabled = activity.Start >= this.GetOccurrenceThatsDueForReminder( root );

				activity.ReminderEnabled = reminderEnabled;
			}
		} 

		#endregion // InitializeReminderEnabled

		#region CreateOccurrenceId

		/// <summary>
		/// Creates an OccurrenceId object using the the RootActivity and the OriginalStartTime/OriginalEndTime 
		/// values of the specified occurrence.
		/// </summary>
		/// <param name="activity">Occurrence activity.</param>
		/// <returns>OccurrenceId instance.</returns>
		internal OccurrenceId CreateOccurrenceId( ActivityBase activity )
		{
			Debug.Assert( null != activity );
			return null != activity ? new OccurrenceId( activity ) : null;
		} 

		#endregion // CreateOccurrenceId

		internal RecurrenceInfo GetRecurrenceInfo( ActivityBase root )
		{
			RecurrenceBase recurrence = root.Recurrence;
			Debug.Assert( null != recurrence );
			return null != recurrence ? ScheduleUtilities.GetRecurrenceInfo( root, recurrence, _connector ) : null;
		}

		public void RegisterVariance( ActivityBase activity )
		{
			OccurrenceId id = this.CreateOccurrenceId( activity );
			if ( null != id )
			{
				ActivityBase existing;
				if ( !_variances.TryGetValue( id, out existing ) || existing != activity )
				{
					_variances[id] = activity;
					this.AddToMapHelper( id, activity );

					// Initialize the variance' RootActivity if it hasn't been initialized yet.
					// NOTE: We are assuming that if a variance is returned from an activity query,
					// that query also returns the root activity of the variance. This would be
					// true if MaxOccurrenceDateTime property is maintained properly. It's value
					// is always greater than or equal to the maximum end time of any occurrence,
					// including variances, of the recurring activity.
					// 
					ActivityBase rootActivity = activity.RootActivity;
					if ( null == rootActivity )
					{
						rootActivity = this.GetRootActivity( activity.RootActivityId );
						activity.RootActivity = rootActivity;
					}

					if ( null == id._template )
						id._template = rootActivity;
					
					activity.IsVariance = true;

					// OccurrenceIdCollection instances are listening for when a variance gets registered 
					// or unregistered so they can remove or add the associated occurrence from themselves.
					// 
					this.NotifyListeners( this, "VarianceRegistered", id );
				}
			}
		}

		private void AddToMapHelper( OccurrenceId id, ActivityBase activity )
		{
			_map[id] = activity;
		}

		private void RemoveFromMapHelper( OccurrenceId id )
		{
			_map.Remove( id );
		}

		public void UnregisterVariance( ActivityBase activity )
		{
			OccurrenceId id = this.CreateOccurrenceId( activity );
			if ( null != id )
			{
				if ( _variances.Remove( id ) )
				{
					this.RemoveFromMapHelper( id );

					// OccurrenceIdCollection instances are listening for when a variance gets registered 
					// or unregistered so they can remove or add the associated occurrence from themselves.
					// 
					this.NotifyListeners( this, "VarianceUnregistered", id );
				}
			}
		}

		public bool HasVariance( OccurrenceId occurrenceId )
		{
			return _variances.ContainsKey( occurrenceId );
		}

		public object GetDataItem( ActivityBase activity )
		{
			
			// 
			return activity.DataItem;
		}

		public void SetDataItem( ActivityBase activity, object newDataItem )
		{
			activity.Initialize( _connector, _storage );
		}

		public object GetDataItemComparisonTokenForRecycling( object dataItem )
		{
			return dataItem;
		}

		public void OnError( ActivityBase activity, string message )
		{
			DataErrorInfo error = new DataErrorInfo( message );
			this.OnError( error );
		}

		public void OnError( DataErrorInfo error )
		{
			ScheduleUtilities.RaiseErrorHelper( _connector, error );
		}
	}

	internal class OccurrenceId
	{
		internal ActivityBase _template;
		internal readonly ActivityBase _variance;
		internal readonly string _id;
		internal readonly DateTime _originalDateTime;

		internal OccurrenceId( ActivityBase template, DateRange dateRange )
		{
			_template = template;
			_originalDateTime = dateRange.Start;
			_id = this.CreateId( template.Id, template.GetValueHelper<int>( ActivityBase.StorageProps.RecurrenceVersion ) );
		}

		internal OccurrenceId( ActivityBase variance )
		{
			_variance = variance;
			_originalDateTime = variance.OriginalOccurrenceStart;
			_id = variance.Id;
		}

		internal string RootId
		{
			get
			{
				// Either template or variance must be there - see in the constructors.
				// 
				return null != _template ? _template.Id : _variance.Id;
			}
		}

		private string CreateId( string rootId, int recurrenceSequence )
		{
			Debug.Assert( ! string.IsNullOrEmpty( rootId ) );
			string dtStr = ScheduleUtilities.ToICalendarString( _originalDateTime, false );

			return null != rootId ? ( rootId + "-" + recurrenceSequence + "-" + dtStr ) : dtStr;
		}

		public override int GetHashCode( )
		{
			return _id.GetHashCode( );
		}

		public override bool Equals( object obj )
		{
			OccurrenceId id = obj as OccurrenceId;
			return null != id && id._id == _id;
		}
	}

	internal class AggregateOccurrenceIdCollection<TViewItem> : AggregateCollection<OccurrenceId>
		where TViewItem: ActivityBase, new( )
	{
		#region OccurrenceIdCollection Class

		internal class OccurrenceIdCollection : ObservableCollectionExtended<OccurrenceId>, IPropertyChangeListener
		{
			#region Member Vars

			private AggregateOccurrenceIdCollection<TViewItem> _owner;
			private OccurrenceManager<TViewItem> _manager;
			private ActivityBase _template;
			private DateRecurrenceCalculatorBase _calculator;
			private HashSet<OccurrenceId> _skippedVariances;

			#endregion // Member Vars

			internal OccurrenceIdCollection( AggregateOccurrenceIdCollection<TViewItem> owner, ActivityBase template )
			{
				_owner = owner;
				_manager = owner._occurrencesManager;
				_template = template;
				_skippedVariances = new HashSet<OccurrenceId>( );
				_manager.AddListener( this, true );

				this.Verify( );
			}

			public ActivityBase RootActivity
			{
				get
				{
					return _template;
				}
			}

			internal void OnVarianceRegistered( OccurrenceId varianceId )
			{
				int index = this.IndexOf( varianceId );
				if ( index >= 0 )
				{
					_skippedVariances.Add( varianceId );
					this.RemoveAt( index );
				}
			}

			private void AddIdHelper( OccurrenceId id )
			{
				if ( _manager.HasVariance( id ) )
					_skippedVariances.Add( id );
				else
					this.Add( id );
			}

			internal void Verify( )
			{
				this.BeginUpdate( );

				this.Clear( );

				// Access the OwningCalendar to have the recurrence template synchronize its OwningCalendar 
				// and OwningResource to the ids. This way the root's storage will have it's Id values set
				// properly.
				// 
				ResourceCalendar calendar = _template.OwningCalendar;
				Debug.Assert( null != calendar, "Root activity has no calendar!" );

				_calculator = _manager.GetRecurrenceCalculator( _template );
				Debug.Assert( null != _calculator );
				if ( null != _calculator )
				{
					DateRange rootDateRange = ScheduleUtilities.GetDateRange( _template );

					// SSP 1/13/10 TFS61964
					// If the activity is time-zone neutral, the generated occurrences are also time-zone neutral. However their
					// Start and End properties return times that are in effect local. These local times have to be checked against
					// date ranges that are also local. _owner._dateRanges are in UTC so we need to convert them to local if the
					// activity is time-zone neutral for the purposes of check to see if an occurrence's date range intersects with
					// the date ranges for which we are generating activities.
					// 
					// ------------------------------------------------------------------------------------------------------------
					//bool includeRoot = ScheduleUtilities.Intersects( rootDateRange, _owner._dateRanges );
					//foreach ( DateRange ii in _calculator.GetOccurrences( _owner._dateRanges ) )
					
					IEnumerable<DateRange> dateRangesResolved = _owner._dateRanges;
					if ( _template.IsTimeZoneNeutral )
						dateRangesResolved = ScheduleUtilities.ConvertFromUtc( _owner._occurrencesManager.LocalTimeZone, dateRangesResolved );

					bool includeRoot = ScheduleUtilities.Intersects( rootDateRange, dateRangesResolved );

					foreach ( DateRange ii in _calculator.GetOccurrences( dateRangesResolved ) )
					// ------------------------------------------------------------------------------------------------------------
					{
						if ( includeRoot )
						{
							includeRoot = false;
							if ( !ScheduleUtilities.IsSameDateTimeWithinSecond( rootDateRange.Start, ii.Start ) )
								this.AddIdHelper( new OccurrenceId( _template, rootDateRange ) );
						}

						OccurrenceId id = new OccurrenceId( _template, ii );
						this.AddIdHelper( id );
					}
				}

				this.EndUpdate( );




			}

			#region IPropertyChangeListener Interface Implementation

			void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object dataItem, string property, object extraInfo )
			{
				if ( dataItem == _manager )
				{
					OccurrenceId id = extraInfo as OccurrenceId;
					if ( null != id )
					{
						switch ( property )
						{
							// When a variance gets registered, we need to remove the associated occurrence from the
							// collection. Variances are included in a different collection.
							// 
							case "VarianceRegistered":
								{
									int index = this.IndexOf( id );
									if ( index >= 0 )
									{
										_skippedVariances.Add( id );
										this.RemoveAt( index );
									}
								}
								break;
							case "VarianceUnregistered":
								{
									if ( _skippedVariances.Remove( id ) )
										this.Add( id );
								}
								break;
						}
					}
				}
			} 

			#endregion // IPropertyChangeListener Interface Implementation
		} 

		#endregion // OccurrenceIdCollection Class

		#region Member Vars

		private OccurrenceManager<TViewItem> _occurrencesManager;
		private IList<ActivityBase> _source;
		private IEnumerable<DateRange> _dateRanges;
		private PropertyChangeListener<AggregateOccurrenceIdCollection<TViewItem>> _listener;
		private Dictionary<ActivityBase, OccurrenceIdCollection> _collectionMap;
		private ObservableCollection<IEnumerable> _collectionList;
		private ObservableCollectionExtended<OccurrenceId> _varianceList;

		#endregion // Member Vars

		internal AggregateOccurrenceIdCollection( OccurrenceManager<TViewItem> occurrencesManager, IList<ActivityBase> source, IEnumerable<DateRange> dateRanges )
			: base( )
		{
			CoreUtilities.ValidateNotNull( occurrencesManager );
			CoreUtilities.ValidateNotNull( source );
			CoreUtilities.ValidateNotNull( dateRanges );

			_occurrencesManager = occurrencesManager;
			_dateRanges = dateRanges;
			_listener = new PropertyChangeListener<AggregateOccurrenceIdCollection<TViewItem>>( this, OnSourceChanged );
			this.Source = source;
		}

		internal IList<ActivityBase> Source
		{
			get
			{
				return _source;
			}
			set
			{
				if ( _source != value )
				{
					ScheduleUtilities.ManageListenerHelperObj( ref _source, value, _listener, false );

					this.Verify( );
				}
			}
		}

		private static void OnSourceChanged( AggregateOccurrenceIdCollection<TViewItem> owner, object sender, string propName, object extraInfo )
		{
			bool reevalRecurrence = false;
			bool recurrenceChanged = false;
			bool reevalOccurrence = false;
			ActivityBase activity = null;

			if ( sender is ActivityBase )
			{
				activity = (ActivityBase)sender;

				if ( ActivityBase.StorageProps.Name_RecurrenceVersion == propName )
				{
					recurrenceChanged = true;
					reevalRecurrence = true;
				}
				else if ( ActivityBase.StorageProps.Name_IsOccurrenceDeleted == propName )
				{
					reevalOccurrence = true;
				}

				
				if ( activity.IsRecurrenceRoot )
					owner._occurrencesManager._storage.OnRootPropertyChanged( activity, propName );
			}
			else if ( sender == owner._source )
			{
				owner.OnSourceCollectionChanged( sender, propName, extraInfo );
			}

			if ( reevalRecurrence || recurrenceChanged )
			{
				owner.ProcessActivityRecurrenceChange( activity );
			}
			else if ( reevalOccurrence )
			{
				owner.ProcessActivity( activity, false );
			}
		}

		private void OnSourceCollectionChanged( object sender, string propName, object extraInfo )
		{
			bool processAsReset = false;

			NotifyCollectionChangedEventArgs args = extraInfo as NotifyCollectionChangedEventArgs;
			switch ( null != args ? args.Action : NotifyCollectionChangedAction.Reset )
			{
				case NotifyCollectionChangedAction.Add:
					if ( null != args.NewItems )
						this.ProcessActivities( ScheduleUtilities.ToTyped<ActivityBase>( args.NewItems ), false );
					else
						processAsReset = true;

					break;
				case NotifyCollectionChangedAction.Remove:
					if ( null != args.OldItems )
						this.ProcessActivities( ScheduleUtilities.ToTyped<ActivityBase>( args.OldItems ), true );
					else
						processAsReset = true;

					break;
				default:
					processAsReset = true;
					break;
			}

			if ( processAsReset )
			{
				this.Verify( );
			}
		}

		private void ProcessActivityRecurrenceChange( ActivityBase activity )
		{
			if ( null != _collectionMap )
			{
				if ( null == activity.Recurrence )
				{
					this.RemoveRecurringActivity( activity );
				}
				else
				{
					OccurrenceIdCollection idCollection;
					if ( _collectionMap.TryGetValue( activity, out idCollection ) )
					{
						idCollection.Verify( );
					}
					else
					{
						this.AddRecurringActivity( activity );
					}
				}
			}
		}

		private void AddRecurringActivity( ActivityBase activity )
		{
			if ( null != _collectionMap && !_collectionMap.ContainsKey( activity ) )
			{
				OccurrenceIdCollection idCollection = new OccurrenceIdCollection( this, activity );
				_collectionMap[activity] = idCollection;
				_collectionList.Add( idCollection );
			}
			else
				Debug.Assert( false );
		}

		private void RemoveVariances( ActivityBase root )
		{
			
			
			var list = _varianceList;
			if ( null != list )
			{
				list.BeginUpdate( );
				try
				{
					for ( int i = 0, count = list.Count; i < count; i++ )
					{
						OccurrenceId ii = list[i];
						if ( null != ii._variance && root == ii._variance.RootActivity )
							list[i] = null;
					}

					CoreUtilities.RemoveAll( list, null );
				}
				finally
				{
					list.EndUpdate( );
				}
			}
		}

		private void RemoveRecurringActivity( ActivityBase activity )
		{
			if ( null != _collectionMap && null != _collectionList )
			{
				OccurrenceIdCollection idCollection;
				if ( _collectionMap.TryGetValue( activity, out idCollection ) )
				{
					Debug.Assert( _collectionList.Contains( idCollection ) );

					_collectionMap.Remove( activity );
					_collectionList.Remove( idCollection );

					this.RemoveVariances( activity );

					
					if ( null != idCollection )
						_occurrencesManager._storage.OnRootPropertyChanged( activity, "IsDeleted" );
				}
				else
					Debug.Assert( false );
			}
		}

		private void ProcessActivities( IEnumerable<ActivityBase> activities, bool remove )
		{
			foreach ( ActivityBase ii in activities )
				this.ProcessActivity( ii, remove, 1 );

			foreach ( ActivityBase ii in activities )
				this.ProcessActivity( ii, remove, 2 );





		}

		/// <summary>
		/// Adds or removes the activity which can be a recurring activity or a variance.
		/// </summary>
		/// <param name="ii">Activity</param>
		/// <param name="remove">If true then removes the activity otherwise adds it.</param>
		/// <param name="phase">Phase 1 will only add variances and skip recurring activities 
		/// and phase 2 will add only recurring activities and skip variances.</param>
		private void ProcessActivity( ActivityBase ii, bool remove, int phase = 0 )
		{
			// If the activity is a variance.
			// 
			if ( null != ii.RootActivityId )
			{
				if ( 0 == phase || 1 == phase )
				{
					// A variance should not have Recurrence set on it.
					// 
					if ( null != ii.Recurrence )
						_occurrencesManager.OnError( ii, "Activity cannot have 'RootActivityId' and 'Recurrence' set at the same time."
							+ " Presence of 'RootActivityId' value means the activity is a variance and therefore should not have"
							+ " 'Recurrence' set on it. Ignoring Recurrence value." );

					// When the variance is removed from the underlying result associated with this occurrence id collection, that
					// doesn't mean that it's completely removed from the underlying data source. It could have been removed because
					// the query criteria no longer matches the result and thus it's removed from the result. And therefore it should
					// not be unregistered from the occurrence manager.
					// 
					if ( !remove )
						_occurrencesManager.RegisterVariance( ii );

					OccurrenceId occurrenceId = _occurrencesManager.CreateOccurrenceId( ii );
					if ( null != occurrenceId )
					{
						if ( !remove && !ii.IsOccurrenceDeleted
							// SSP 1/13/10 TFS61964
							// If the activity is a time-zone neutral activity, then its time is in local time, which needs to be converted
							// to UTC in order to compare it against date ranges which are in UTC.
							// 
							//&& ScheduleUtilities.Intersects( ii, _dateRanges ) 
							&& ScheduleUtilities.Intersects( ii, _dateRanges, _occurrencesManager.LocalTimeZone ) 
							)
							_varianceList.Add( occurrenceId );
						else
							_varianceList.Remove( occurrenceId );
					}
				}
			}
			// If the activity is a recurring activity.
			// 
			else if ( null != ii.Recurrence
				// A recurring activity can be made into a non-recurring activity, for example by using the Remove button on the 
				// recurrence dialog.
				// 
				|| remove )
			{
				if ( 0 == phase || 2 == phase )
				{
					if ( !remove )
						this.AddRecurringActivity( ii );
					else
						this.RemoveRecurringActivity( ii );
				}
			}
			else
			{
				Debug.Assert( false );
				_occurrencesManager.OnError( ii, "Activity is neither a recurring activity nor a variance." );
			}
		}

		private void Verify( )
		{
			_collectionMap = new Dictionary<ActivityBase, OccurrenceIdCollection>( );
			_collectionList = new ObservableCollection<IEnumerable>( );
			_varianceList = new ObservableCollectionExtended<OccurrenceId>( false, false );

			this.ProcessActivities( _source, false );

			_collectionList.Add( _varianceList );
			this.Collections = _collectionList;
		}
	}

	internal class DividedCollection<T> : ReadOnlyNotifyCollection<ReadOnlyNotifyCollection<T>>
	{
		private IList<T> _source;
		private IList<Predicate<T>> _predicates;
		private ObservableCollectionExtended<ReadOnlyNotifyCollection<T>> _subCollections;
		private List<ObservableCollectionExtended<T>> _subCollectionSources;
		private PropertyChangeListener<DividedCollection<T>> _listener;

		internal DividedCollection( IList<T> source, IList<Predicate<T>> predicates )
			: base( new ObservableCollectionExtended<ReadOnlyNotifyCollection<T>>( false, true )  )
		{
			CoreUtilities.ValidateNotNull( source );
			CoreUtilities.ValidateNotNull( predicates );

			_source = source;
			_predicates = predicates;
			_subCollections = (ObservableCollectionExtended<ReadOnlyNotifyCollection<T>>)this.Source;
			_subCollectionSources = new List<ObservableCollectionExtended<T>>( );
			_listener = new PropertyChangeListener<DividedCollection<T>>( this, OnPropertyChangedHandler );

			ISupportPropertyChangeNotifications vcn = _source as ISupportPropertyChangeNotifications;
			if ( null != vcn )
				vcn.AddListener( _listener, true );

			vcn = predicates as ISupportPropertyChangeNotifications;
			if ( null != vcn )
				vcn.AddListener( _listener, true );

			this.Verify( );
		}

		private void ProcessItems( IEnumerable<T> items, bool remove )
		{
			foreach ( T ii in items )
				this.ProcessItem( ii, remove );
		}

		private void ProcessItem( T item, bool remove )
		{
			if ( remove )
			{
				foreach ( IList<T> ii in _subCollectionSources )
					ii.Remove( item );
			}
			else
			{
				var subCollectionSources = _subCollectionSources;
				var predicates = _predicates;
				for ( int i = 0, count = predicates.Count; i < count; i++ )
				{
					var ii = predicates[i];
					if ( ii( item ) )
					{
						subCollectionSources[i].Add( item );
						break;
					}
				}
			}
		}

		private void Verify( )
		{
			_subCollections.BeginUpdate( );
			
			_subCollections.Clear( );
			_subCollectionSources.Clear( );

			var predicates = _predicates;
			for ( int i = 0, count = predicates.Count; i < count; i++ )
			{
				var subCollectionSource = new ObservableCollectionExtended<T>( false, true );
				_subCollectionSources.Add( subCollectionSource );
				_subCollections.Add( new ReadOnlyNotifyCollection<T>( subCollectionSource ) );
			}

			this.ProcessItems( _source, false );

			_subCollections.EndUpdate( );
		}

		private void OnPropertyChangedHelper( object sender, string propName, object extraInfo )
		{
			bool processAsReset = false;

			if ( sender == _source )
			{
				NotifyCollectionChangedEventArgs args = extraInfo as NotifyCollectionChangedEventArgs;
				if ( null != args )
				{
					switch ( args.Action )
					{
						case NotifyCollectionChangedAction.Add:
							if ( null != args.NewItems )
								this.ProcessItems( ScheduleUtilities.ToTyped<T>( args.NewItems ), false );
							else
								processAsReset = true;
							break;
						case NotifyCollectionChangedAction.Remove:
							if ( null != args.OldItems )
								this.ProcessItems( ScheduleUtilities.ToTyped<T>( args.OldItems ), true );
							else
								processAsReset = true;
							break;
						default:
							processAsReset = true;
							break;
					}
				}
			}
			else if ( sender == _predicates )
			{
				processAsReset = true;
			}

			if ( processAsReset )
				this.Verify( );
		}

		private static void OnPropertyChangedHandler( DividedCollection<T> owner, object sender, string propName, object extraInfo )
		{
			owner.OnPropertyChangedHelper( sender, propName, extraInfo );
		}
	}

	internal class UnorderedFilteredCollection<T> : ReadOnlyNotifyCollection<T>
	{
		private IList<T> _source;
		private ObservableCollectionExtended<T> _dest;
		private Predicate<T> _predicate;
		private PropertyChangeListener<UnorderedFilteredCollection<T>> _listener;

		internal UnorderedFilteredCollection( IList<T> source, Predicate<T> predicate )
			: base( new ObservableCollectionExtended<T>( false, true ) )
		{
			CoreUtilities.ValidateNotNull( source );
			CoreUtilities.ValidateNotNull( predicate );

			_source = source;
			_dest = (ObservableCollectionExtended<T>)this.Source;
			_predicate = predicate;
			_listener = new PropertyChangeListener<UnorderedFilteredCollection<T>>( this, OnPropertyChangedHandler );

			ISupportPropertyChangeNotifications vcn = _source as ISupportPropertyChangeNotifications;
			if ( null != vcn )
				vcn.AddListener( _listener, true );

			this.Verify( );
		}

		private void ProcessItems( IEnumerable<T> items, bool remove )
		{
			foreach ( T ii in items )
				this.ProcessItem( ii, remove );
		}

		private void ProcessItem( T item, bool remove )
		{
			if ( remove )
			{
				_dest.Remove( item );
			}
			else
			{
				if ( _predicate( item ) )
					_dest.Add( item );
			}
		}

		private void Verify( )
		{
			_dest.BeginUpdate( );

			_dest.Clear( );
			
			this.ProcessItems( _source, false );

			_dest.EndUpdate( );
		}

		private void OnPropertyChangedHelper( object sender, string propName, object extraInfo )
		{
			bool processAsReset = false;

			if ( sender == _source )
			{
				NotifyCollectionChangedEventArgs args = extraInfo as NotifyCollectionChangedEventArgs;
				if ( null != args )
				{
					switch ( args.Action )
					{
						case NotifyCollectionChangedAction.Add:
							if ( null != args.NewItems )
								this.ProcessItems( ScheduleUtilities.ToTyped<T>( args.NewItems ), false );
							else
								processAsReset = true;
							break;
						case NotifyCollectionChangedAction.Remove:
							if ( null != args.OldItems )
								this.ProcessItems( ScheduleUtilities.ToTyped<T>( args.OldItems ), true );
							else
								processAsReset = true;
							break;
						default:
							processAsReset = true;
							break;
					}
				}
			}

			if ( processAsReset )
				this.Verify( );
		}

		private static void OnPropertyChangedHandler( UnorderedFilteredCollection<T> owner, object sender, string propName, object extraInfo )
		{
			owner.OnPropertyChangedHelper( sender, propName, extraInfo );
		}
	}

	internal class UnorderedTranslatedCollection<S, T> : ReadOnlyNotifyCollection<T>
	{
		private IList<S> _source;
		private ObservableCollectionExtended<T> _dest;
		private Converter<S, T> _conveter;
		private PropertyChangeListener<UnorderedTranslatedCollection<S, T>> _listener;
		private Dictionary<S, T> _table;

		internal UnorderedTranslatedCollection( IList<S> source, Converter<S, T> conveter )
			: base( new ObservableCollectionExtended<T>( false, true ) )
		{
			CoreUtilities.ValidateNotNull( source );
			CoreUtilities.ValidateNotNull( conveter );

			_source = source;
			_conveter = conveter;
			_dest = (ObservableCollectionExtended<T>)this.Source;
			_table = new Dictionary<S, T>( );

			_listener = new PropertyChangeListener<UnorderedTranslatedCollection<S, T>>( this, OnPropertyChangedHandler );

			ISupportPropertyChangeNotifications vcn = _source as ISupportPropertyChangeNotifications;
			if ( null != vcn )
				vcn.AddListener( _listener, true );

			this.Verify( );
		}

		private void ProcessItems( IEnumerable<S> items, bool remove )
		{
			foreach ( S s in items )
				this.ProcessItem( s, remove );
		}

		private T Translate( S s )
		{
			T t;
			if ( ! _table.TryGetValue( s, out t ) )
				_table[s] = t = _conveter( s );

			return t;
		}

		private void ProcessItem( S s, bool remove )
		{
			T t = this.Translate( s );
			if ( null != t )
			{
				if ( remove )
				{
					_table.Remove( s );
					_dest.Remove( t );
				}
				else
				{
					_dest.Add( t );
				}
			}
		}

		private void Verify( )
		{
			_dest.BeginUpdate( );

			_dest.Clear( );

			this.ProcessItems( _source, false );

			_dest.EndUpdate( );
		}

		private void OnPropertyChangedHelper( object sender, string propName, object extraInfo )
		{
			bool processAsReset = false;

			if ( sender == _source )
			{
				NotifyCollectionChangedEventArgs args = extraInfo as NotifyCollectionChangedEventArgs;
				if ( null != args )
				{
					switch ( args.Action )
					{
						case NotifyCollectionChangedAction.Add:
							if ( null != args.NewItems )
								this.ProcessItems( ScheduleUtilities.ToTyped<S>( args.NewItems ), false );
							else
								processAsReset = true;
							break;
						case NotifyCollectionChangedAction.Remove:
							if ( null != args.OldItems )
								this.ProcessItems( ScheduleUtilities.ToTyped<S>( args.OldItems ), true );
							else
								processAsReset = true;
							break;
						default:
							processAsReset = true;
							break;
					}
				}
			}

			if ( processAsReset )
				this.Verify( );
		}

		private static void OnPropertyChangedHandler( UnorderedTranslatedCollection<S, T> owner, object sender, string propName, object extraInfo )
		{
			owner.OnPropertyChangedHelper( sender, propName, extraInfo );
		}
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