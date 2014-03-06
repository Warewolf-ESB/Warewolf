using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Media;


#pragma warning disable 1574
using Infragistics.Services;
using Infragistics.Collections.Services;
using Infragistics;

namespace Infragistics.Controls.Schedules.Services





{
	#region Resource Class

	/// <summary>
	/// Represents a resource (owner) in a schedule.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// A <b>Resource</b> object represents a resource or am owner in the schedule object model. A resource can be a person
	/// or a resource like conference room.
	/// </para>
	/// </remarks>
	/// <seealso cref="Appointment"/>
	/// <seealso cref="Task"/>
	/// <seealso cref="Journal"/>
	/// <seealso cref="XamScheduleDataManager.ResourceItems"/>
	[DebuggerDisplay("Id={Id}, Name={Name}, DataItem={DataItem}")]
	public class Resource : PropertyChangeNotifier, ISupportPropertyChangeNotifications
	{
		#region Data Structures

		#region StorageProps Class

		internal static class StorageProps
		{
			internal const int Id = 0;
			internal const int Name = 1;
			internal const int IsVisible = 2;
			internal const int IsLocked = 3;
            internal const int EmailAddress = 4;
            internal const int Description = 5;
			internal const int PrimaryCalendarId = 6;
			internal const int PrimaryTimeZoneId = 7;
			internal const int FirstDayOfWeek = 8;
            internal const int Calendars = 9;
			internal const int DaysOfWeek = 10;
			internal const int DaySettingsOverrides = 11;
			// SSP 12/8/10 - NAS11.1 Activity Categories
			// 
			internal const int CustomActivityCategories = 12;
			internal const int CachedPrevCustomActivityCategories = 13;
			internal const int Metadata = 14;
			internal const int BeginEditData = 15;

            internal class Info : StoragePropsInfo, ITypedPropertyChangeListener<Resource, int>
			{
				protected override void Initialize( IMap<int, StoragePropsInfo.PropInfo> map )
				{
                    Func<Resource, IPropertyStorage<Resource, int>> sr = i => i.Storage;

					StoragePropsInfo.PropInfo[] infos = new StoragePropsInfo.PropInfo[]
					{
						new TypedPropInfo<Resource, string>( Id, "Id", sr, ResourceProperty.Id ),
						new TypedPropInfo<Resource, string>( Name, "Name", sr, ResourceProperty.Name ),
						new TypedPropInfo<Resource, bool?>( IsVisible, "IsVisible", sr, ResourceProperty.IsVisible ),
						new TypedPropInfo<Resource, bool>( IsLocked, "IsLocked", sr, ResourceProperty.IsLocked ),
						new TypedPropInfo<Resource, string>( EmailAddress, "EmailAddress", sr, ResourceProperty.EmailAddress ),
                        new TypedPropInfo<Resource, string>( Description, "Description", sr, ResourceProperty.Description ),
						new TypedPropInfo<Resource, string>( PrimaryCalendarId, "PrimaryCalendarId", sr, ResourceProperty.PrimaryCalendarId ),
						new TypedPropInfo<Resource, string>( PrimaryTimeZoneId, "PrimaryTimeZoneId", sr, ResourceProperty.PrimaryTimeZoneId ),
						new TypedPropInfo<Resource, DayOfWeek?>( FirstDayOfWeek, "FirstDayOfWeek", sr, ResourceProperty.FirstDayOfWeek ),
                        new TypedPropInfo<Resource, ObservableCollectionExtended<ResourceCalendar>>( Calendars, "Calendars", sr, null, isReadOnly: true, defaultValueFactoryFunc: CreateDefaultValue_Calendars ),
						new TypedPropInfo<Resource, ScheduleDaysOfWeek>( DaysOfWeek, "DaysOfWeek", sr, ResourceProperty.DaysOfWeek, converter: new XmlDeserializerConverter( typeof( ScheduleDaysOfWeek ) ) ),
						new TypedPropInfo<Resource, DaySettingsOverrideCollection>( DaySettingsOverrides, "DaySettingsOverrides", sr, ResourceProperty.DaySettingsOverrides, converter: new XmlDeserializerConverter( typeof( DaySettingsOverrideCollection ) ) ),
						// SSP 12/8/10 - NAS11.1 Activity Categories
						// 
						new TypedPropInfo<Resource, ActivityCategoryCollection>( CustomActivityCategories, "CustomActivityCategories", sr, ResourceProperty.CustomActivityCategories, converter: new XmlDeserializerConverter( typeof( ActivityCategoryCollection ) ), copyMethod: CopyMethod.CopyClone ),
						new TypedPropInfo<Resource, ActivityCategoryCollection>( CachedPrevCustomActivityCategories, "CachedPrevCustomActivityCategories", sr, copyMethod: CopyMethod.None ),
						new TypedPropInfo<Resource, MetadataPropertyValueStore>( Metadata, "Metadata", sr, isReadOnly: true, copyMethod: CopyMethod.CopyContents, defaultValueFactoryFunc: CreateHelper<Resource, DictionaryMetadataPropertyValueStore>, equalityComparer: new DelegateEqualityComparer<MetadataPropertyValueStore>( MetadataPropertyValueStore.HasSameValues, MetadataPropertyValueStore.GetHashCode ) ),
						new TypedPropInfo<Resource, Resource>( BeginEditData, "BeginEditData", sr, copyMethod: CopyMethod.None ),
					};

					FillMap( infos, map );

					int[] unmappedPropertiesStoreCandidates = new int[]
					{
						Name,
						IsVisible,
						IsLocked,
						EmailAddress,
						Description,
						PrimaryTimeZoneId,
						FirstDayOfWeek,
						Calendars,
						DaysOfWeek,
						DaySettingsOverrides,
						// SSP 12/8/10 - NAS11.1 Activity Categories
						// 
						CustomActivityCategories
					};

					MapsFactory.SetValues( _unmappedPropertiesStoreCandidates, unmappedPropertiesStoreCandidates, true );
				}

				private static Info g_instance = new Info( );

				internal static Info Instance
				{
					get
					{
						return g_instance;
					}
				}

				protected override bool CopyCloneOverride( StoragePropsInfo.PropInfo propInfo, object sourceVal, ref object destVal )
				{
					if ( sourceVal is ActivityCategoryCollection )
					{
						destVal = ( (ActivityCategoryCollection)sourceVal ).Clone( );
						return true;
					}

					return base.CopyCloneOverride( propInfo, sourceVal, ref destVal );
				}

                public void OnPropertyValueChanged( Resource item, int property, object extraInfo )
                {
                    PropInfo info = this.Props[property];

                    Debug.Assert( null != info );

					if ( null != info )
					{
						
						switch ( property )
						{
							case StorageProps.DaysOfWeek:
								ScheduleUtilities.ManageListenerHelper( ref item._cachedDaysOfWeek, item.DaysOfWeek, item.PropChangeListeners, true );
								break;
							case StorageProps.DaySettingsOverrides:
								ScheduleUtilities.ManageListenerHelper( ref item._cachedDaySettingsOverrides, item.DaySettingsOverrides, item.PropChangeListeners, true );
								break;
							case StorageProps.CustomActivityCategories:
								if ( !ScheduleUtilities.Antirecursion.InProgress( item, "categories" ) )
								{
									ActivityCategoryCollection oldColl = item.GetValueHelper<ActivityCategoryCollection>( CachedPrevCustomActivityCategories );
									ActivityCategoryCollection newColl = item.GetValueHelper<ActivityCategoryCollection>( CustomActivityCategories );
									ScheduleUtilities.ManageListenerHelper( ref oldColl, newColl, item.PropChangeListeners, true );
									item.SetValueHelper( CachedPrevCustomActivityCategories, newColl );
								}
								break;
						}

						item.RaisePropertyChangedEvent( info._name );
					}
                }

				internal IPropertyStorage<Resource, int> CreateDefaultStorage( Resource item )
				{
					return new DictionaryPropertyStorage<Resource, int>( this, this.GetDefaultValueFactories( ) );
				}

				private ObservableCollectionExtended<ResourceCalendar> CreateDefaultValue_Calendars( Resource resource )
				{
					ObservableCollectionExtended<ResourceCalendar> calendars = new ObservableCollectionExtended<ResourceCalendar>( false, true );
					calendars.PropChangeListeners.Add( resource.PropChangeListeners, false );
					return calendars;
				}
			}
		}

		#endregion // StorageProps Class

		#endregion // Data Structures

		#region Member Vars






		private object _dataItem;

		private IPropertyStorage<Resource, int> _storage;
        private ResourceCalendar _primaryCalendar;
        private PropertyChangeListenerList _propChangeListeners;

		private ScheduleDaysOfWeek _cachedDaysOfWeek;
		private DaySettingsOverrideCollection _cachedDaySettingsOverrides;

		#endregion // Member Vars

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="Resource"/>
		/// </summary>
		public Resource()
		{
		} 
		#endregion // Constructor

        #region Base Overrides

        #region OnPropertyChanged

        /// <summary>
        /// Overridden. Called when property changed event is raised.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        protected override void OnPropertyChanged( string propName )
        {
            base.OnPropertyChanged( propName );

            if ( null != _propChangeListeners )
                _propChangeListeners.OnPropertyValueChanged( this, propName, null );
        }

        #endregion // OnPropertyChanged

        #endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region Calendars

		/// <summary>
        /// Gets the calendars associated with this user.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// <b>Note</b> that there's always at least one calendar in the returned collection.
        /// Moreover, the first instance in the calendar is always the primary calendar of the resource.
        /// </para>
        /// </remarks>
        /// <seealso cref="ResourceCalendar"/>
        public ObservableCollection<ResourceCalendar> Calendars
        {
            get
            {
                return this.GetValueHelper<ObservableCollection<ResourceCalendar>>( StorageProps.Calendars );
            }
        }

        #endregion // Calendars

		#region CustomActivityCategories

		// SSP 12/8/10 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Gets or sets a collection of custom activity categories for this resource.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the schedule data connector can also provide a default list of activity categories.
		/// <b>CustomActivityCategories</b> are in addition to any such default activity categories.
		/// </para>
		/// </remarks>



		public ActivityCategoryCollection CustomActivityCategories
		{
			get
			{
				return this.GetValueHelper<ActivityCategoryCollection>( StorageProps.CachedPrevCustomActivityCategories )
					?? this.GetValueHelper<ActivityCategoryCollection>( StorageProps.CustomActivityCategories );
			}
			set
			{
				this.SetValueHelper<ActivityCategoryCollection>( StorageProps.CustomActivityCategories, value );
			}
		}

		#endregion // CustomActivityCategories

		#region DataItem

		/// <summary>
		/// Gets the underlying data item from the data source.
		/// </summary>
		public object DataItem
		{
			get
			{
				ViewItemManager<Resource>.IdToken idToken = this.IdToken;

				if (idToken != null)
					return idToken.DataItem;

				return _dataItem;
			}
			set
			{
				_dataItem = value;
			}
		}

		#endregion // DataItem

        #region DaySettingsOverrides

        /// <summary>
        /// Gets a collection of DaySettingsOverride objects for this resource.
        /// </summary>
        public DaySettingsOverrideCollection DaySettingsOverrides
        {
            get
            {
				return this.GetValueHelper<DaySettingsOverrideCollection>( StorageProps.DaySettingsOverrides );
            }
            set
            {
				this.SetValueHelper<DaySettingsOverrideCollection>( StorageProps.DaySettingsOverrides, value );
            }
        }

        #endregion // DaySettingsOverrides

		#region DaysOfWeek

		/// <summary>
		/// Keyed collection of DaySettings objects where the key is DayOfWeek enum and the value is the associated DaySettings object. This is for providing per resource day of week settings. For example, let?s say the office hours are 9 AM-5 PM for everybody but the sales team?s office hour are 7 AM - 3 PM then each sales team member will have DayOfWeek entry for each work day in this collection with 7 AM - 3 PM WorkingHours specified.
		/// </summary>
		public ScheduleDaysOfWeek DaysOfWeek
		{
			get
			{
				return this.GetValueHelper<ScheduleDaysOfWeek>( StorageProps.DaysOfWeek );
			}
			set
			{
				this.SetValueHelper<ScheduleDaysOfWeek>( StorageProps.DaysOfWeek, value );
			}
		}

		#endregion // DaysOfWeek

        #region Description

        /// <summary>
        /// Any extra information about the resource.
        /// </summary>
        public string Description
        {
            get
            {
                return this.GetValueHelper<string>( StorageProps.Description );
            }
            set
            {
                this.SetValueHelper<string>( StorageProps.Description, value );
            }
        }

        #endregion // Description

        #region EmailAddress

        /// <summary>
        /// Email address.
        /// </summary>
        public string EmailAddress
        {
            get
            {
                return this.GetValueHelper<string>( StorageProps.EmailAddress );
            }
            set
            {
                this.SetValueHelper<string>( StorageProps.EmailAddress, value );
            }
        }

        #endregion // EmailAddress

		#region FirstDayOfWeek

		

		/// <summary>
		/// Specifies the first day of the week for the resource.
		/// </summary>
		/// <seealso cref="ScheduleSettings.FirstDayOfWeek"/>
		public DayOfWeek? FirstDayOfWeek
		{
			get
			{
				return this.GetValueHelper<DayOfWeek?>( StorageProps.FirstDayOfWeek );
			}
			set
			{
				this.SetValueHelper<DayOfWeek?>( StorageProps.FirstDayOfWeek, value );
			}
		}

		#endregion // FirstDayOfWeek

		#region Id

		/// <summary>
		/// Resource Id.
		/// </summary>
		public string Id
		{
			get
			{
				return this.GetValueHelper<string>( StorageProps.Id );
			}
			set
			{
				this.SetValueHelper<string>( StorageProps.Id, value );
			}
		}

		#endregion // Id

		#region IsVisibleResolved

		/// <summary>
		/// Gets the resolve value indicating whether this resource is visible in the UI.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that this property doesn't indicate whether the resource is presently visible in the UI.
		/// It indicates whether the resource can be displayed in the UI.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsVisible"/>
		/// <seealso cref="ScheduleControlBase.CalendarGroupsResolved"/>
		public bool IsVisibleResolved
		{
			get
			{
                bool? val = this.IsVisible;
                if ( val.HasValue && !val.Value )
                    return false;

				return true;
			}
		}

		#endregion // IsVisibleResolved

		#region IsLocked

		/// <summary>
		/// Specifies whether the activities of this resource can be modified in the UI.
		/// </summary>
		public bool IsLocked
		{
			get
			{
				return this.GetValueHelper<bool>( StorageProps.IsLocked );
			}
			set
			{
				this.SetValueHelper<bool>( StorageProps.IsLocked, value );
			}
		}

		#endregion // IsLocked

		#region Metadata

		/// <summary>
		/// Returns a MetadataPropertyValueStore object that's used for storing and retrieving metadata information.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can use the <b>Metadata</b> to store and later retrieve information. The information is stored in
		/// properties of your data items as specified in the <see cref="MetadataPropertyMappingCollection"/>.
		/// For example, the metadata property mappings is specified using the <see cref="AppointmentPropertyMappingCollection"/>'s
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.MetadataPropertyMappings"/>
		/// property. Each property defined in the mapping collection has a corresponding entry in the returned MetadataPropertyValueStore.
		/// It's indexer is used to retrieve or set the the property's value. Furthermore, you can use bindings to
		/// bind to a specific value in the returned object.
		/// </para>
		/// </remarks>
		/// <seealso cref="MetadataPropertyMappingCollection"/>
		/// <seealso cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.MetadataPropertyMappings"/>
		public MetadataPropertyValueStore Metadata
		{
			get
			{
				return this.GetValueHelper<MetadataPropertyValueStore>( StorageProps.Metadata );
			}
		}

		#endregion // Metadata

		#region Name

		/// <summary>
		/// Gets or sets the resource name.
		/// </summary>
		public string Name
		{
			get
			{
				return this.GetValueHelper<string>( StorageProps.Name );
			}
			set
			{
				this.SetValueHelper<string>( StorageProps.Name, value );
			}
		}

		#endregion // Name

        #region PrimaryCalendar

        /// <summary>
        /// Primary calendar of the resource.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// A resource can have multiple calendars. A calendar can be designated as a primary calendar.
        /// </para>
        /// </remarks>
		// SSP 10/20/10 TFS57589
		// Added XmlIgnoreAttribute so the object can be serialized using XmlSerializer.
		// 
		[System.Xml.Serialization.XmlIgnoreAttribute]
        public ResourceCalendar PrimaryCalendar
        {
            get
            {
                return _primaryCalendar;
            }
            set
            {
                if ( _primaryCalendar != value )
                {
                    _primaryCalendar = value;
                    this.RaisePropertyChangedEvent( "PrimaryCalendar" );
                }
            }
        }

        #endregion // PrimaryCalendar

        #region PrimaryCalendarId

        /// <summary>
        /// Primary calendar of the resource.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// A resource can have multiple calendars. A calendar can be designated as a primary calendar.
        /// </para>
        /// </remarks>
        public string PrimaryCalendarId
        {
            get
            {
                return this.GetValueHelper<string>( StorageProps.PrimaryCalendarId );
            }
            set
            {
                this.SetValueHelper<string>( StorageProps.PrimaryCalendarId, value );
            }
        }

        #endregion // PrimaryCalendarId

		#region PrimaryTimeZoneId

		

		/// <summary>
		/// Primary time-zone of the resource.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can designate a time-zone to a resource.
		/// </para>
		/// </remarks>
		public string PrimaryTimeZoneId
		{
			get
			{
				return this.GetValueHelper<string>( StorageProps.PrimaryTimeZoneId );
			}
			set
			{
				this.SetValueHelper<string>( StorageProps.PrimaryTimeZoneId, value );
			}
		}

		#endregion // PrimaryTimeZoneId

		#region IsVisible

		/// <summary>
        /// Specifies whether this resource is visible in the UI.
        /// </summary>
        public bool? IsVisible
        {
            get
            {
                return this.GetValueHelper<bool?>( StorageProps.IsVisible );
            }
            set
            {
                this.SetValueHelper<bool?>( StorageProps.IsVisible, value );
            }
        }

        #endregion // IsVisible

		#endregion // Public Properties

		#region Internal Properties

		#region BeginEditData

		/// <summary>
		/// Returns the data that was stored when BeginEdit was called.
		/// </summary>
		internal Resource BeginEditData
		{
			get
			{
				return this.GetValueHelper<Resource>( StorageProps.BeginEditData );
			}
		}

		#endregion // BeginEditData

        #region DaySettingsOverridesIfAllocated

        internal DaySettingsOverrideCollection DaySettingsOverridesIfAllocated
        {
            get
            {
				// This is not a lazily allocated property so simply return the property's value.
				// We'll leave this property in case we change the behavior and start allocating
				// the collection lazily.
				// 
				return this.DaySettingsOverrides;
            }
        } 

        #endregion // DaySettingsOverridesIfAllocated

		#region IdToken

		internal ViewItemManager<Resource>.IdToken IdToken
		{
			get
			{
				return _dataItem as ViewItemManager<Resource>.IdToken;
			}
			set
			{
				if (_dataItem != value)
				{
					_dataItem = value;

					
					
					
				}
			}
		}

		#endregion // IdToken 

        #region PropChangeListeners

        /// <summary>
        /// Gets collection of property change listeners.
        /// </summary>
        internal PropertyChangeListenerList PropChangeListeners
        {
            get
            {
                if ( null == _propChangeListeners )
                {
                    _propChangeListeners = new PropertyChangeListenerList( );
                    _propChangeListeners.Add( new PropertyChangeListener<Resource>( this, OnSubObjectPropertyChanged ), false );
                }

                return _propChangeListeners;
            }
        }

        #endregion // PropChangeListeners

		#region Storage

		internal IPropertyStorage<Resource, int> Storage
		{
			get
			{
				if ( null == _storage )
					_storage = StorageProps.Info.Instance.CreateDefaultStorage( this );

				return _storage;
			}
		}

		#endregion // Storage

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region ClearBeginEditData

		internal void ClearBeginEditData( )
		{
			this.ClearValueHelper<Resource>( StorageProps.BeginEditData );
		}

		#endregion // ClearBeginEditData

		#region ClearValueHelper

		internal void ClearValueHelper<T>( int property )
		{
			this.Storage.SetValue<T>( this, property, default( T ) );
		}

		#endregion // ClearValueHelper

		#region Clone

		// SSP 1/8/11 - NAS11.1 Activity Categories
		// 
		internal Resource Clone( bool copyId )
		{
			Resource clone = new Resource( );
			StorageProps.Info.Instance.CopyValues( this, this.Storage, clone, clone.Storage );

			return clone;
		}

		#endregion // Clone

		#region GetCalendar

		internal ResourceCalendar GetCalendar( string calendarId )
		{
			IList<ResourceCalendar> calendars = this.Calendars;
			if ( null != calendars )
			{
				// If calendars table is not provided then match null calendar id's to the default auto-created calendar.
				// 
				if ( string.IsNullOrEmpty( calendarId ) )
				{
					ResourceCalendar calendar = 1 == calendars.Count ? calendars[0] : null;
					if ( null != calendar && calendar.IsAutoCreated )
						return calendar;
				}
				else
				{
					for ( int i = 0, count = calendars.Count; i < count; i++ )
					{
						ResourceCalendar cal = calendars[i];

						if ( null != cal && cal.Id == calendarId )
							return cal;
					}
				}
			}

			return null;
		}

		#endregion // GetCalendar

		#region Initialize

		internal void Initialize( IPropertyStorage<Resource, int> storage, bool createDefaultCalendar )
		{
			if ( _storage != storage )
			{
				StorageProps.Info.Instance.InitializeNewStore( this, ref _storage, storage );
				this.InitializeCalendars( createDefaultCalendar );
			}
		}

		#endregion // Initialize

		#region RaisePropertyChangedEvent

		internal new void RaisePropertyChangedEvent( string propName )
		{
			base.RaisePropertyChangedEvent( propName );
		}

		#endregion // RaisePropertyChangedEvent

		#region RestoreBeginEditData

		// SSP 1/8/11 - NAS11.1 Activity Categories
		// 
		internal bool RestoreBeginEditData( )
		{
			Resource origData = this.BeginEditData;
			if ( null == origData )
				return false;

			StorageProps.Info.Instance.CopyValues( origData, origData.Storage, this, this.Storage );
			this.ClearBeginEditData( );

			return true;
		}

		#endregion // RestoreBeginEditData

		#region StoreBeginEditData

		// SSP 1/8/11 - NAS11.1 Activity Categories
		// 
		internal bool StoreBeginEditData( )
		{
			Resource data = this.BeginEditData;
			if ( null != data )
				return false;

			data = this.Clone( true );
			this.SetValueHelper( StorageProps.BeginEditData, data );
			return true;
		}

		#endregion // StoreBeginEditData

		#endregion // Internal Methods

        #region Private Methods

        #region GetValueHelper

        private T GetValueHelper<T>( int property )
        {
            return this.Storage.GetValue<T>( this, property );
        }

        #endregion // GetValueHelper

        #region InitializeCalendars

        private void InitializeCalendars( bool createDefaultCalendar )
        {
            // If PrimaryCalendarId has been specified then don't auto-create the default calendar. We should wait
            // for the calendars data source to be specified and populated. Presence of a PrimaryCalendarId should
            // mean that there's a calendars data source.
            // 
			Debug.Assert( null != this.Calendars );
            if ( createDefaultCalendar && string.IsNullOrEmpty( this.PrimaryCalendarId ) )
            {
                IList<ResourceCalendar> calendars = this.Calendars;

                if ( null != calendars && 0 == calendars.Count )
                    calendars.Add( ResourceCalendar.CreateDefaultCalendar( this ) );
            }

            this.SyncPrimaryCalendarHelper( null );
        }

        #endregion // InitializeCalendars

        #region OnSubObjectPropertyChanged

        private static void OnSubObjectPropertyChanged( Resource resource, object sender, string propName, object extraInfo )
        {
            resource.OnSubObjectPropertyChangedHelper( sender, propName, extraInfo );
        }

        private void OnSubObjectPropertyChangedHelper( object sender, string propName, object extraInfo )
        {
            // If calendars collection changes then synchronize the primary calendar and primary calendar id properties.
            // 
			// SSP 6/18/12 TFS108893
			// 
			//if ( sender == this.Calendars )
			if ( sender is ObservableCollection<ResourceCalendar> && sender == this.Calendars )
            {
                this.SyncPrimaryCalendarHelper( null );
            }
            else if ( sender == this.PrimaryCalendar )
            {
                this.SyncPrimaryCalendarHelper( false );
            }
			else if ( sender == this )
			{
				switch ( propName )
				{
					case "PrimaryCalendarId":
						this.SyncPrimaryCalendarHelper( true );
						break;
					case "PrimaryCalendar":
						this.SyncPrimaryCalendarHelper( false );
						break;
					case "IsVisible":
						this.RaisePropertyChangedEvent( "IsVisibleResolved" );
						break;
					case "IsVisibleResolved":

						IList<ResourceCalendar> calendars = this.Calendars;
						if ( null != calendars )
						{
							for ( int i = 0, count = calendars.Count; i < count; i++ )
							{
								calendars[i].RaiseIsVisibleResolved( );
							}
						}

						break;
				}
			}
			else if ( sender is ActivityCategoryCollection || sender is ActivityCategory )
			{
				if ( !ScheduleUtilities.Antirecursion.InProgress( this, "categories" ) )
				{
					ScheduleUtilities.Antirecursion.Enter( this, "categories", true );

					try
					{
						this.SetValueHelper( StorageProps.CustomActivityCategories, this.CustomActivityCategories );
					}
					finally
					{
						ScheduleUtilities.Antirecursion.Exit( this, "categories" );
					}
				}
			}
        }

        #endregion // OnSubObjectPropertyChanged

        #region SetValueHelper

        private void SetValueHelper<T>( int property, T newVal )
        {
            this.Storage.SetValue<T>( this, property, newVal );
        }

        #endregion // SetValueHelper

        #region SyncPrimaryCalendarHelper

        private void SyncPrimaryCalendarHelper( bool? syncWithId )
        {
            if ( !syncWithId.HasValue )
            {
                if ( !ScheduleUtilities.IsValueEmpty( this.PrimaryCalendarId ) )
                    syncWithId = true;
                else if ( null != this.PrimaryCalendar )
                    syncWithId = false;
                else
                {
                    this.PrimaryCalendar = ScheduleUtilities.GetFirstItem( this.Calendars );
                    return;
                }
            }

            if ( syncWithId.Value )
            {
                string id = this.PrimaryCalendarId;
				this.PrimaryCalendar = this.GetCalendar( id );
            }
            else
            {
				ResourceCalendar primaryCalendar = this.PrimaryCalendar;
                this.PrimaryCalendarId = null != primaryCalendar ? primaryCalendar.Id : null;
            }
        }

        #endregion // SyncPrimaryCalendarHelper 

        #endregion // Private Methods

		#endregion // Methods

        #region ISupportPropertyChangeNotifications Implementation

        void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
        {
            this.PropChangeListeners.Add( listener, useWeakReference );
        }

        void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
        {
            this.PropChangeListeners.Remove( listener );
        }

        #endregion // ISupportPropertyChangeNotifications Implementation
	} 

	#endregion // Resource Class
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