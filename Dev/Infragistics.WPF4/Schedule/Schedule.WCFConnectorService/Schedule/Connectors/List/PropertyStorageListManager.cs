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

namespace Infragistics.Controls.Schedules.Services






{
	#region PropertyStorageListManager Class

	/// <summary>
	/// List manager that supports property storage mechanism for managing mapped property values on view items.
	/// The view item class needs to provide a StoragePropsInfo which essentially is a collection of PropInfo
	/// objects that describe the property of the view item. The prop info is used to manage the field value
	/// notifications as well as providing the view items the value for the associated field via a property 
	/// storage.
	/// </summary>
	/// <typeparam name="TViewItem"></typeparam>
	/// <typeparam name="TMappingKey"></typeparam>
	internal abstract class PropertyStorageListManager<TViewItem, TMappingKey> : ListManager<TViewItem, TMappingKey>
        where TViewItem: class
    {
        #region Nested Data Structures

		#region PropertyStoreWrapperBase Class

		/// <summary>
		/// This IPropertyStore implementation dynamically switches between using a IFieldValueAccessor if a property has a field mapping for it
		/// or using a DictionaryPropertyStore if there's no mapping for the field.
		/// </summary>
		internal abstract class PropertyStoreWrapperBase : SupportValueChangeNotificationsImpl<TViewItem>
		{
			#region Member Vars

			protected PropertyStorageListManager<TViewItem, TMappingKey> _listManager;
			protected TMappingKey _mappedField;
			protected bool _hasMappedField;
			protected Converter<TViewItem, object> _viewItemToDataItemConverter;
			protected IFieldValueAccessor _fieldAccessor;
			protected ISupportValueChangeNotifications<TViewItem> _fieldValueChangeNotifier;
			protected StoragePropsInfo.PropInfo _propInfo;

			#endregion // Member Vars

			#region FieldAccessor

			internal IFieldValueAccessor FieldAccessor
			{
				get
				{
					return _fieldAccessor;
				}
			}

			#endregion // FieldAccessor

			#region PropInfo

			internal StoragePropsInfo.PropInfo PropInfo
			{
				get
				{
					return _propInfo;
				}
			} 

			#endregion // PropInfo

			#region Methods

			#region Initialize

			internal void Initialize( PropertyStorageListManager<TViewItem, TMappingKey> listManager,
					TMappingKey mappedField, bool hasMappedField, Converter<TViewItem, object> viewItemToDataItemConverter,
					StoragePropsInfo.PropInfo propInfo )
			{
				_mappedField = mappedField;
				_hasMappedField = hasMappedField;
				_viewItemToDataItemConverter = viewItemToDataItemConverter;
				_listManager = listManager;
				_propInfo = propInfo;

				this.OnInitialized( );

				this.VerifyFieldAccessor( );
			}

			#endregion // Initialize

			#region OnInitialized

			protected virtual void OnInitialized( )
			{
			}

			#endregion // OnInitialized

			#region VerifyFieldAccessor

			internal virtual void VerifyFieldAccessor( bool dontUseFieldValueAccessor = false,
				IFieldValueAccessor fieldValueAccessorOverride = null, ISupportValueChangeNotifications<TViewItem> fieldValueChangeNotifierOverride = null )
			{
				// If the property has a field mapping for it then use the field value accessor.
				// 
				ISupportValueChangeNotifications<TViewItem> fieldValueChangeNotifier = null;

				// Unhook from the last notifier.
				// 
				if ( null != _fieldValueChangeNotifier )
				{
					_fieldValueChangeNotifier.RemoveListener( this.Listeners );
					_fieldValueChangeNotifier = null;
				}

				_fieldAccessor = fieldValueAccessorOverride ??
					( _hasMappedField && ! dontUseFieldValueAccessor ? _listManager.GetFieldValueAccessor( _mappedField, out fieldValueChangeNotifier ) : null );

				if ( null != fieldValueChangeNotifierOverride )
					fieldValueChangeNotifier = fieldValueChangeNotifierOverride;

				if ( null != fieldValueChangeNotifier )
					fieldValueChangeNotifier.AddListener( this.Listeners );
			}

			#endregion // VerifyFieldAccessor 

			#region ReInitializeInternalStoreFromUnmappedPropertiesStorage

			internal abstract void ReInitializeInternalStoreFromUnmappedPropertiesStorage( XmlDictionaryStorage<TViewItem> unmappedFieldsStorage = null ); 

			#endregion // ReInitializeInternalStoreFromUnmappedPropertiesStorage

			#region InitializeFieldValueAccessorFrom

			internal virtual void InitializeFieldValueAccessorFrom( PropertyStoreWrapperBase store )
			{
				this.VerifyFieldAccessor( fieldValueAccessorOverride: store._fieldAccessor );
			} 

			#endregion // InitializeFieldValueAccessorFrom

			#endregion // Methods
		}

		#endregion // PropertyStoreWrapperBase Class

		#region PropertyStoreWrapper Class

		/// <summary>
		/// This IPropertyStore implementation dynamically switches between using a IFieldValueAccessor if a property has a field mapping for it
		/// or using a DictionaryPropertyStore if there's no mapping for the field.
		/// </summary>
		/// <typeparam name="TValue">Type of values that are stored.</typeparam>
		internal class PropertyStoreWrapper<TValue> : PropertyStoreWrapperBase, IPropertyStore<TViewItem, TValue>
		{
			#region Member Vars

			private IPropertyStore<TViewItem, TValue> _fallbackStore;
			private bool _isFallbackStoreInternallyCreated;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			internal PropertyStoreWrapper( )
				: base( )
			{
			}

			#endregion // Constructor

			#region Base Overrides

			#region OnInitialized

			protected override void OnInitialized( )
			{
				this.ReInitializeInternalStore( null );
			}

			#endregion // OnInitialized

			#endregion // Base Overrides

			#region Properties

			#region InternalStore

			internal IPropertyStore<TViewItem, TValue> InternalStore
			{
				get
				{
					return _fallbackStore;
				}
			}

			#endregion // InternalStore 

			#endregion // Properties

			#region Methods

			#region Public Methods

			#region GetValue

			public TValue GetValue( TViewItem viewItem )
			{
				if ( null != _fieldAccessor )
				{
					object dataItem = _viewItemToDataItemConverter( viewItem );

					// SSP 7/17/12 TFS100159
					// For any metadata mappings, do go to the underlying field value accessor - even if
					// the data item is our view item since they can derive a class from the view item class
					// and add properties and use them for metadata. In such a case, we wouldn't have a 
					// _propInfo and therefore the value should be accessed via FVA.
					// 
					//if ( dataItem != (object)viewItem )
					if ( dataItem != (object)viewItem || null == _propInfo )
					{
						object val = _fieldAccessor.GetValue( dataItem );
						return null != val ? (TValue)val : default( TValue );
					}
				}

				return _fallbackStore.GetValue( viewItem );
			}

			#endregion // GetValue

			#region HasValue

			public bool HasValue( TViewItem viewItem )
			{
				if ( null != _fieldAccessor )
				{
					object dataItem = _viewItemToDataItemConverter( viewItem );

					// SSP 7/17/12 TFS100159
					// 
					//if ( dataItem != (object)viewItem )
					if ( dataItem != (object)viewItem || null == _propInfo )
					{
						object val = _fieldAccessor.GetValue( dataItem );
						return null != val && !EqualityComparer<TValue>.Default.Equals( (TValue)val, default( TValue ) );
					}
				}

				return _fallbackStore.HasValue( viewItem );
			}

			#endregion // HasValue

			#region SetValue

			public void SetValue( TViewItem viewItem, TValue newValue )
			{
				if ( null != _fieldAccessor )
				{
					object dataItem = _viewItemToDataItemConverter( viewItem );

					// SSP 7/17/12 TFS100159
					// 
					//if ( dataItem != (object)viewItem )
					if ( dataItem != (object)viewItem || null == _propInfo )
					{
						_fieldAccessor.SetValue( dataItem, newValue );

						// If the data source doesn't support item property change notifications then notify the listeners
						// manually.
						// 
						DataListEventListener dataSourceListener = _listManager.ListEventListener;
						if ( null == dataSourceListener || !dataSourceListener.SupportsItemPropChangeNotifications )
							this.Listeners.OnValueChanged( viewItem );

						return;
					}
				}

				_fallbackStore.SetValue( viewItem, newValue );
			}

			#endregion // SetValue 

			#endregion // Public Methods

			#region Internal Methods

			#region ReInitializeInternalStore

			internal override void ReInitializeInternalStoreFromUnmappedPropertiesStorage( XmlDictionaryStorage<TViewItem> unmappedFieldsStorage = null )
			{
				var store = null != unmappedFieldsStorage && null != _propInfo && _hasMappedField 
					// If the field was explicitly defined and thus has a field value accessor to it, then don't store its
					// value in the unmapped properties field.
					// 
					&& null == _fieldAccessor
					? unmappedFieldsStorage.CreateStore<TValue>( _propInfo ) : null;

				this.ReInitializeInternalStore( store );
			}

			internal void ReInitializeInternalStore( IPropertyStore<TViewItem, TValue> fallbackStore )
			{
				ISupportValueChangeNotifications<TViewItem> vcn = _fallbackStore as ISupportValueChangeNotifications<TViewItem>;
				if ( null != vcn )
					vcn.RemoveListener( this.Listeners );

				if ( null != fallbackStore )
				{
					_isFallbackStoreInternallyCreated = false;
					_fallbackStore = fallbackStore;
				}
				// If the property doesn't have a field mapping for it then use a DictionaryPropertyStore.
				// If we had already created the DictionaryPropertyStore then don't re-create it - that's
				// what _isFallbackStoreInternallyCreated flag is for.
				// 
				else if ( ! _isFallbackStoreInternallyCreated )
				{
					_fallbackStore = new DictionaryPropertyStore<TViewItem, TValue>( null != _propInfo ? _propInfo._defaultValueFactory : null );
					_isFallbackStoreInternallyCreated = true;
				}

				vcn = _fallbackStore as ISupportValueChangeNotifications<TViewItem>;
				if ( null != vcn )
					vcn.AddListener( this.Listeners );
			}

			#endregion // ReInitializeInternalStore  

			#endregion // Internal Methods

			#endregion // Methods

			#region IPropertyStore Members

			object IPropertyStore.GetValue( object viewItem )
			{
				return this.GetValue( (TViewItem)viewItem );
			}

			void IPropertyStore.SetValue( object viewItem, object newValue )
			{
				this.SetValue( (TViewItem)viewItem, null != newValue ? (TValue)newValue : default( TValue ) );
			}

			bool IPropertyStore.HasValue( object viewItem )
			{
				return this.HasValue( (TViewItem)viewItem );
			}

			#endregion
		}

		#endregion // PropertyStoreWrapper Class

        #region PropertyValueConverter Class

        internal class PropertyValueConverter : IValueConverter
        {
			#region Member Vars

			private StoragePropsInfo.PropInfo _propInfo;

			/// <summary>
			/// User supplied converter info.
			/// </summary>
			private ConverterInfo _converterInfo; 

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="propInfo"></param>
			/// <param name="converterInfo"></param>
			internal PropertyValueConverter( StoragePropsInfo.PropInfo propInfo, ConverterInfo converterInfo )
			{
				_propInfo = propInfo;
				_converterInfo = converterInfo;
			} 

			#endregion // Constructor

			#region Methods

			#region Convert

			public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
			{
				return this.ConvertHelper( value, targetType, parameter, culture, false );
			}

			#endregion // Convert

			#region ConvertBack

			public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
			{
				return this.ConvertHelper( value, targetType, parameter, culture, true );
			}

			#endregion // ConvertBack

			#region ConvertHelper

			private object ConvertHelper( object valueParm, Type targetType, object parameter, System.Globalization.CultureInfo culture, bool convertingBack )
			{
				object val = valueParm;

				if ( convertingBack || null == targetType || typeof( object ) == targetType || null != _propInfo && _propInfo._type == targetType )
				{
					// If there's a user provided converter info, use that.
					// 
					if ( null != _converterInfo && null != _converterInfo._converter )
					{
						val = !convertingBack
							? _converterInfo.Convert( val )
							: _converterInfo.ConvertBack( val, targetType );
					}

					// If property info has a converter then use that. Typically complex properties, like collection types, have
					// converters that convert string, which can be xml, to the complex property type object.
					// 
					IValueConverter converter = null != _propInfo ? _propInfo._converter : null;
					if ( null != converter )
					{
						val = !convertingBack
							? converter.Convert( val, _propInfo._type, parameter, culture )
							: converter.ConvertBack( val, targetType, parameter, culture );
					}

					// SSP 10/28/10 TFS57603
					// If the above conversion logic does not convert (because no converter is specified for example),
					// then try generic conversion logic.
					// 
					if ( null != val )
					{
						Type type = !convertingBack ? _propInfo._type : targetType;
						if ( null != type && ! type.IsInstanceOfType( val ) )
						{
							val = CoreUtilities.ConvertDataValue( val, type, culture, null );
							if ( null == val )
							{
								val = new DataErrorInfo( string.Format( "Unable to convert value {0} of type {1} to {2} for property {3}.",
									valueParm, valueParm.GetType( ).Name, _propInfo._type.Name, _propInfo._name ) );
							}
						}
					}
				}
				else
				{
					Debug.Assert( false, "Unknown target type." );
				}

				
				if ( val is DataErrorInfo )
					val = null;

				return val;
			}

			#endregion // ConvertHelper 

			#endregion // Methods
        }

        #endregion // PropertyValueConverter Class
        
        #endregion // Nested Data Structures

        #region Member Vars

        protected PropertyStorage<TViewItem, int> _propertyStorage;
        internal readonly Converter<TViewItem, object> _viewItemToDataItemConverter;
        protected StoragePropsInfo _propsInfo;
		private object _unmappedPropertiesMappingKey;
		private XmlDictionaryStorage<TViewItem> _unmappedPropertiesStorage;

        #endregion // Member Vars

        #region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="propsInfo">Property information of the TViewItem.</param>
		/// <param name="viewItemToDataItemConverter">Converter that retrieves the data item from the view item.</param>
		internal PropertyStorageListManager( StoragePropsInfo propsInfo, Converter<TViewItem, object> viewItemToDataItemConverter )
        {
            CoreUtilities.ValidateNotNull( propsInfo );
            CoreUtilities.ValidateNotNull( viewItemToDataItemConverter );

            _propsInfo = propsInfo;
            _viewItemToDataItemConverter = viewItemToDataItemConverter;
        }

        #endregion // Constructor

		#region Base Overrides

		#region VerifyFieldValueAccessors

		protected override void VerifyFieldValueAccessors( )
		{
			base.VerifyFieldValueAccessors( );

			if ( null != _propertyStorage )
			{
				// Verifiy metadata property store.
				// 
				PropertyStoreWrapper<MetadataPropertyValueStore> metadataStore = this.VerifyMetadataPropertyStore( );

				// Verifiy stores for unmapped properties.
				// 
				this.VerifyUnmappedPropertiesStorage( );

				IMap<int, IPropertyStore> stores = _propertyStorage.PropertyStores;
				foreach ( int i in stores )
				{
					PropertyStoreWrapperBase w = stores[i] as PropertyStoreWrapperBase;
					if ( null != w && metadataStore != w )
					{
						w.VerifyFieldAccessor( );

						var prop = w.PropInfo;
						if ( null != prop && _propsInfo._unmappedPropertiesStoreCandidates[ prop._key ] )
							w.ReInitializeInternalStoreFromUnmappedPropertiesStorage( _unmappedPropertiesStorage );
					}
				}
			}
		}

		#endregion // VerifyFieldValueAccessors

		#endregion // Base Overrides

        #region Properties

        #region Internal Properties

		#region MinimumRequiredFields

		internal virtual TMappingKey[] MinimumRequiredFields
		{
			get
			{
				return null;
			}
		} 

		#endregion // MinimumRequiredFields

        #region PropertyStorage

        internal PropertyStorage<TViewItem, int> PropertyStorage
        {
            get
            {
				if ( null == _propertyStorage )
				{
					_propertyStorage = this.CreatePropertyStorage( );
					this.OnPropertyStorageCreated( );
				}

                return _propertyStorage;
            }
        }

        #endregion // PropertyStorage

        #endregion // Internal Properties

        #endregion // Properties

        #region Methods

        #region Public Methods

        #endregion // Public Methods

        #region Protected Methods

        #region CreateFieldValueAccessorOverride

		/// <summary>
		/// Overridden. Creates IFieldValueAccessor for the specified TViewItem property.
		/// </summary>
		/// <param name="field">Identifies the TViewItem property.</param>
		/// <returns></returns>
        protected override IFieldValueAccessor CreateFieldValueAccessorOverride( TMappingKey field )
        {
            if ( this.IsListViewItemsList )
            {
                StoragePropsInfo.PropInfo propInfo = null != _propsInfo ? _propsInfo.GetMappedPropInfo<TMappingKey>( field ) : null;
                IFieldValueAccessor accessor = propInfo as IFieldValueAccessor;
                if ( null != accessor )
                    return accessor;
            }

            return base.CreateFieldValueAccessorOverride( field );
        }

        #endregion // CreateFieldValueAccessorOverride

        #region CreatePropertyStorage

        private PropertyStorage<TViewItem, int> CreatePropertyStorage( )
        {
            IMap<int, IPropertyStore> stores = MapsFactory.CreateMapHelper<int, IPropertyStore>( );

			this.CreateStoresHelper( _propsInfo, _viewItemToDataItemConverter, stores );

            ITypedPropertyChangeListener<TViewItem, int> listener = _propsInfo as ITypedPropertyChangeListener<TViewItem, int>;
            return new PropertyStorage<TViewItem, int>( stores, listener );
        }

        #endregion // CreatePropertyStorage

        #region CreatePropertyStoreWrapper

        private IPropertyStore CreatePropertyStoreWrapper( StoragePropsInfo.PropInfo propInfo, TMappingKey mappedField, bool hasMappedField, Type type, Converter<TViewItem, object> viewItemToDataItemConverter )
        {
            PropertyStoreWrapperBase ret;

            if ( typeof( string ) == type )
                ret = new PropertyStoreWrapper<string>( );
            else if ( typeof( int ) == type )
                ret = new PropertyStoreWrapper<int>( );
            else if ( typeof( int? ) == type )
                ret = new PropertyStoreWrapper<int?>( );
            else if ( typeof( long ) == type )
                ret = new PropertyStoreWrapper<long>( );
            else if ( typeof( long? ) == type )
                ret = new PropertyStoreWrapper<long?>( );
            else if ( typeof( bool ) == type )
                ret = new PropertyStoreWrapper<bool>( );
            else if ( typeof( bool? ) == type )
                ret = new PropertyStoreWrapper<bool?>( );
            else if ( typeof( DateTime ) == type )
                ret = new PropertyStoreWrapper<DateTime>( );
            else if ( typeof( DateTime? ) == type )
                ret = new PropertyStoreWrapper<DateTime?>( );
            else if ( typeof( TimeSpan ) == type )
                ret = new PropertyStoreWrapper<TimeSpan>( );
            else if ( typeof( TimeSpan? ) == type )
                ret = new PropertyStoreWrapper<TimeSpan?>( );
            else if ( typeof( DayOfWeek ) == type )
                ret = new PropertyStoreWrapper<DayOfWeek>( );
            else if ( typeof( DayOfWeek? ) == type )
                ret = new PropertyStoreWrapper<DayOfWeek?>( );
            else if ( typeof( ObservableCollectionExtended<ResourceCalendar> ) == type )
				ret = new PropertyStoreWrapper<ObservableCollectionExtended<ResourceCalendar>>( );
			else if ( typeof( MetadataPropertyValueStore ) == type )
				ret = new PropertyStoreWrapper<MetadataPropertyValueStore>( );
			else
				ret = new PropertyStoreWrapper<object>( );

			ret.Initialize( this, mappedField, hasMappedField, viewItemToDataItemConverter, propInfo );

            return (IPropertyStore)ret;
        }

        #endregion // CreatePropertyStoreWrapper

        #region GetAllMappingKeys

        /// <summary>
        /// Returns all potential mapping keys.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<TMappingKey> GetAllMappingKeys( )
        {
            IEnumerable<TMappingKey> ret = _propsInfo.GetAllMappedFields<TMappingKey>( );

			// Also add the unmapped properties field if it's not already in the mapped fields list.
			// 
			if ( null != _unmappedPropertiesMappingKey )
			{
				TMappingKey unmappedField = (TMappingKey)_unmappedPropertiesMappingKey;
				if ( !CoreUtilities.Contains( ret, unmappedField ) )
					ret = ScheduleUtilities.Aggregate( ret, new TMappingKey[] { unmappedField } );
			}

			return ret;
        }

        #endregion // GetAllMappingKeys

        #region GetConverter

        protected override ConverterInfo GetConverter( TMappingKey field )
        {
            ConverterInfo ci = base.GetConverter( field );

            StoragePropsInfo.PropInfo propInfo = _propsInfo.GetMappedPropInfo<TMappingKey>( field );
            if ( null == propInfo )
                return ci;

            ConverterInfo cc = new ConverterInfo( );
            cc._converter = new PropertyValueConverter( propInfo, ci );

            return cc;
        }

        #endregion // GetConverter

        #region GetDefaultPropertyMapping

        protected override string GetDefaultPropertyMapping( TMappingKey key )
        {
            StoragePropsInfo.PropInfo propInfo = _propsInfo.GetMappedPropInfo<TMappingKey>( key );
            return null != propInfo ? propInfo._name : null;
        }

        #endregion // GetDefaultPropertyMapping

		#region OnPropertyStorageCreated

		protected virtual void OnPropertyStorageCreated( )
		{
		} 

		#endregion // OnPropertyStorageCreated

		#endregion // Protected Methods

		#region Internal Methods

		#region CreateStoresHelper

		internal void CreateStoresHelper( StoragePropsInfo info, Converter<TViewItem, object> viewItemToDataItemConverter, IMap<int, IPropertyStore> stores )
        {
            // For each property of the view item, create a PropertyStoreWrapper that will dynamically switch between using IFieldValueAccessor
            // when there's a field mapping for the property or to storing the property values to DictionaryPropertyStore. It will handle
            // dynamic changes to field mappings collection as well and switch appropriately.
            // 
            IMap<int, StoragePropsInfo.PropInfo> props = info.Props;
            foreach ( int i in props )
            {
                StoragePropsInfo.PropInfo ii = props[i];

                // Certain properties may not have any field mappings at all. In other words, the property doesn't have any corresponding
                // TMappingKey value.
                // 
                TMappingKey mappedField;
                bool hasMappedField = ii.GetMappedField<TMappingKey>( out mappedField );

                // Create a property store wrapper that will get the value from either the data source (using IFieldValueAccessor)
                // or store it using a DictionaryPropertyStore.
                // 
                stores[ii._key] = this.CreatePropertyStoreWrapper( ii, mappedField, hasMappedField, ii._type, viewItemToDataItemConverter );
            }
        }

        #endregion // CreateStoresHelper

		#region InitializeUnmappedPropertiesMappingKey

		/// <summary>
		/// Initializes unmapped properties field key. Any unampped field values will be stored as XML in this field.
		/// </summary>
		/// <param name="unmappedPropertiesMappingKey">Field mapping key where the XML will be stored.</param>
		internal void InitializeUnmappedPropertiesMappingKey( TMappingKey unmappedPropertiesMappingKey )
		{
			_unmappedPropertiesMappingKey = unmappedPropertiesMappingKey;
		} 

		#endregion // InitializeUnmappedPropertiesMappingKey

		#region ValidateFieldMappingsHelper

		internal bool ValidateFieldMappingsHelper( TMappingKey[] props, bool required, out DataErrorInfo error )
		{
			error = null;

			if ( this.HasMappedFields( props, true ) )
				return true;

			string userMsg = ScheduleUtilities.GetString("LE_AppConfigError");//"Application configuration error. The functionality is not supported."
			string diagnosticMsg = ScheduleUtilities.GetString("LE_MissingMappingHeader", typeof( TViewItem ).Name ) + System.Environment.NewLine + ScheduleUtilities.BuildList( props, System.Environment.NewLine );

			error = new DataErrorInfo( userMsg ) { DiagnosticText = diagnosticMsg };
			if ( required )
				error.Severity = ErrorSeverity.SevereError;

			return false;
		}

		#endregion // ValidateFieldMappingsHelper

		#region ValidateMinimumFieldMappings

		internal void ValidateMinimumFieldMappings( List<DataErrorInfo> errorList )
		{
			var mappings = this.Mappings;
			if ( !this.UseDefaultMappingsResolved && ( null == mappings || 0 == mappings.Count ) )
				errorList.Add(ScheduleUtilities.CreateBlockingFromId(null, "LE_MissingMappings", typeof(TViewItem).Name));

			DataErrorInfo error;
			var requiredFields = this.MinimumRequiredFields;
			if ( null != requiredFields && !this.ValidateFieldMappingsHelper( requiredFields, true, out error ) && null != error )
				errorList.Add( error );
		}

		#endregion // ValidateMinimumFieldMappings

        #endregion // Internal Methods

        #region Private Methods

		#region VerifyMetadataPropertyStore

		private PropertyStoreWrapper<MetadataPropertyValueStore> VerifyMetadataPropertyStore( )
		{
			PropertyStoreWrapper<MetadataPropertyValueStore> metadataStoreWrapper = null;

			var storage = this.PropertyStorage;
			var stores = null != storage ? storage.PropertyStores : null;
			if ( null != stores )
			{
				foreach ( int ii in stores )
				{
					PropertyStoreWrapperBase psw = stores[ii] as PropertyStoreWrapperBase;
					if ( null != psw && null != psw.PropInfo && psw.PropInfo._type == typeof( MetadataPropertyValueStore ) )
					{
						metadataStoreWrapper = psw as PropertyStoreWrapper<MetadataPropertyValueStore>;
						Debug.Assert( null != metadataStoreWrapper );
					}
				}
			}

			if ( null != metadataStoreWrapper )
			{
				IMap<string, IPropertyStore> metadataPropertyStores = null;
				var metadataMappings = null != this.Mappings ? this.Mappings.MetadataPropertyMappings : null;
				if ( null != metadataMappings )
				{
					foreach ( MetadataPropertyMapping mapping in metadataMappings )
					{
						string metadataKey = mapping.MetadataProperty;

						ISupportValueChangeNotifications<TViewItem> notifier;
						IFieldValueAccessor fva = this.GetMetadataFieldValueAccessor( metadataKey, out notifier );
						if ( null != fva )
						{
							var store = new PropertyStoreWrapper<object>( );
							store.Initialize( this, default( TMappingKey ), false, _viewItemToDataItemConverter, null );
							store.VerifyFieldAccessor( fieldValueAccessorOverride: fva, fieldValueChangeNotifierOverride: notifier );

							if ( null == metadataPropertyStores )
								metadataPropertyStores = MapsFactory.CreateMapHelper<string, IPropertyStore>( );

							metadataPropertyStores[metadataKey] = store;
						}
					}
				}

				MetadataStore<TViewItem> metadataStore = new MetadataStore<TViewItem>( );
				metadataStore.Initialize( metadataPropertyStores, null );

				metadataStoreWrapper.ReInitializeInternalStore( metadataStore );
			}

			return metadataStoreWrapper;
		} 

		#endregion // VerifyMetadataPropertyStore

		#region VerifyUnmappedPropertiesStorage

		private void VerifyUnmappedPropertiesStorage( )
		{
			_unmappedPropertiesStorage = null;

			if ( null != _unmappedPropertiesMappingKey )
			{
				TMappingKey unmappedPropertiesMappingKey = (TMappingKey)_unmappedPropertiesMappingKey;
				IFieldValueAccessor fva = this.GetFieldValueAccessor( unmappedPropertiesMappingKey );
				if ( null != fva )
				{
					var store = new PropertyStoreWrapper<string>( );
					store.Initialize( this, unmappedPropertiesMappingKey, true, _viewItemToDataItemConverter, null );
					store.VerifyFieldAccessor( fieldValueAccessorOverride: fva );
					var dictionaryStorage = new XmlDictionaryStorage<TViewItem>( );
					dictionaryStorage.Initialize( store );

					_unmappedPropertiesStorage = dictionaryStorage;
				}
			}
		}

		#endregion // VerifyUnmappedPropertiesStorage

        #endregion // Private Methods

        #endregion // Methods
    }

	#endregion // PropertyStorageListManager Class
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