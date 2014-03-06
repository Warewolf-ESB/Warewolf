using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using Infragistics.Collections;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules

{
    #region ResourceCalendar

    /// <summary>
    /// Represents a calendar.
    /// </summary>
	[DebuggerDisplay("Id={Id}, Name={Name}, OwningResourceId={OwningResourceId}, DataItem={DataItem}")]
	public class ResourceCalendar : PropertyChangeNotifierExtended
    {
        #region Data Structures

        #region StorageProps Class

        internal static class StorageProps
        {
            internal const int Id = 0;
            internal const int OwningResourceId = 1;
            internal const int Name = 2;
            internal const int Description = 3;
            internal const int IsVisible = 4;
            internal const int BaseColor = 5;
			internal const int Metadata = 6;

			internal const string Name_OwningResource = "OwningResource";

            internal class Info : StoragePropsInfo, ITypedPropertyChangeListener<ResourceCalendar, int>
            {
                protected override void Initialize( IMap<int, StoragePropsInfo.PropInfo> map )
                {
                    Func<ResourceCalendar, IPropertyStorage<ResourceCalendar, int>> sr = i => i.Storage;

                    StoragePropsInfo.PropInfo[] infos = new StoragePropsInfo.PropInfo[]
					{
						new TypedPropInfo<ResourceCalendar, string>( Id, "Id", sr, ResourceCalendarProperty.Id ),
                        new TypedPropInfo<ResourceCalendar, string>( OwningResourceId, "OwningResourceId", sr, ResourceCalendarProperty.OwningResourceId ),
						new TypedPropInfo<ResourceCalendar, string>( Name, "Name", sr, ResourceCalendarProperty.Name ),
                        new TypedPropInfo<ResourceCalendar, string>( Description, "Description", sr, ResourceCalendarProperty.Description ),
						new TypedPropInfo<ResourceCalendar, bool?>( IsVisible, "IsVisible", sr, ResourceCalendarProperty.IsVisible ),
                        new TypedPropInfo<ResourceCalendar, Color?>( BaseColor, "BaseColor", sr, ResourceCalendarProperty.BaseColor ),
						new TypedPropInfo<ResourceCalendar, MetadataPropertyValueStore>( Metadata, "Metadata", sr, isReadOnly: true, copyMethod: CopyMethod.CopyContents, defaultValueFactoryFunc: CreateDefaultMetadata<ResourceCalendar>, equalityComparer: new DelegateEqualityComparer<MetadataPropertyValueStore>( MetadataPropertyValueStore.HasSameValues, MetadataPropertyValueStore.GetHashCode ) )
					};

                    FillMap( infos, map );

					int[] unmappedPropertiesStoreCandidates = new int[]
					{
						Name,
						Description,
						IsVisible,
						BaseColor
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


                public void OnPropertyValueChanged( ResourceCalendar item, int property, object extraInfo )
                {
                    PropInfo info = this.Props[property];

                    Debug.Assert( null != info );

                    if ( null != info )
                        item.RaisePropertyChangedEvent( info._name );
                }

				
				
				internal IPropertyStorage<ResourceCalendar, int> CreateDefaultStorage( ResourceCalendar item )
				{
					return new DictionaryPropertyStorage<ResourceCalendar, int>( this, this.GetDefaultValueFactories( ) );
				}
            }
        }

        #endregion // StorageProps Class

        #endregion // Data Structures

        #region Member Vars






		private object _dataItem;

        private IPropertyStorage<ResourceCalendar, int> _storage;
        private Resource _resource;
        private bool _isAutoCreated;
        private CalendarBrushProvider _brushProvider;
        private int _brushVersion;
        private WeakReference _brushProviderTokenRef;
		private Color? _baseColorWhenAssigned; // AS 9/1/10 - TFS37171

        private Binding _brushVersionBinding;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ResourceCalendar"/> object.
        /// </summary>
        public ResourceCalendar( )
        {
            this._brushVersionBinding = new Binding("BrushVersion");
            this._brushVersionBinding.Source = this;
        }

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ResourceCalendar"/> object.
        /// </summary>
        /// <param name="resource"></param>
        public ResourceCalendar( Resource resource ) : this()
        {
            _resource = resource;

            if ( null != resource )
                this.Storage.SetValue<string>( this, StorageProps.OwningResourceId, resource.Id );
        }

        #endregion // Constructor

        #region Base Overrides

        #region OnPropertyChanged

        /// <summary>
        /// Overridden. Called when property changed event is raised.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected override void OnPropertyChanged( string propertyName )
        {
            base.OnPropertyChanged( propertyName );

            switch ( propertyName )
            {
                // Since IsVisibleResolved is dependent on IsVisible, raise IsVisibleResolved property change
                // notification when IsVisible property changes.
                // 
                case "IsVisible":
                    this.RaiseIsVisibleResolved( );
                    break;
            }
        }

        #endregion // OnPropertyChanged 

        #endregion // Base Overrides

        #region Properties

        #region Public Properties

        #region BaseColor

        /// <summary>
        /// Gets or sets the base color used to display this calendar resource.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note</b>: if this property is null (the default) then an appropitae <see cref="BrushProvider"/> will be assigned when this ResourceCalendar is visible in any <see cref="ScheduleControlBase"/> derived control</para>
        /// </remarks>
        /// <seealso cref="BrushProvider"/>
        public Color? BaseColor
        {
            get
            {
                return this.GetValueHelper<Color?>( StorageProps.BaseColor );
            }
            set
            {
                this.SetValueHelper<Color?>( StorageProps.BaseColor, value );
            }
        }

        #endregion // BaseColor

        #region BrushProvider

        /// <summary>
        /// Returns the assigned CalendarBrushProvider
        /// </summary>
        /// <seealso cref="BaseColor"/>
        /// <remarks>
        /// <para class="note"><b>Note</b>: this property will return null unless this ResourceCalendar is visible in one or more <see cref="ScheduleControlBase"/> derived controls</para>
        /// </remarks>
        [ReadOnly(true)]
        public CalendarBrushProvider BrushProvider
        {
            get
            {
                return this._brushProvider;
            }
        }

        #endregion // BrushProvider

        #region BrushVersion

        /// <summary>
        /// Returns the version of the brush cache. 
        /// </summary>
        /// <remarks>
        /// <para class="body">When something changes that invalidates the cache, e.g. the <see cref="OfficeColorScheme"/> is changed or a new <see cref="BrushProvider"/> is asigned then this number is incremented.</para>
        /// </remarks>
        /// <seealso cref="CalendarBrushProvider.BrushVersion"/>
        [ReadOnly(true)]
        [Bindable(true)]
        public int BrushVersion
        {
            get { return this._brushVersion; }
        }

        #endregion //BrushVersion

        #region DataItem

        /// <summary>
        /// Gets the underlying data item from the data source.
        /// </summary>
        public object DataItem
        {
            get
            {
				ViewItemManager<ResourceCalendar>.IdToken idToken = this.IdToken;

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

        #region Description

        /// <summary>
        /// Gets or sets the description.
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

        #region Id

        /// <summary>
        /// Gets or sets the calendar id.
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

        #region OwningResourceId

        /// <summary>
        /// Gets the id of the resource to which this calendar belongs.
        /// </summary>
        public string OwningResourceId
        {
            get
            {
                return this.GetValueHelper<string>( StorageProps.OwningResourceId );
            }
			set
			{
				this.SetValueHelper<string>( StorageProps.OwningResourceId, value );
			}
        }

        #endregion // OwningResourceId

        #region IsVisibleResolved

        /// <summary>
        /// Gets the resolve value indicating whether this resource is visible in the UI.
        /// </summary>
        public bool IsVisibleResolved
        {
            get
            {
                bool? val = this.IsVisible;
                if ( val.HasValue && !val.Value )
                    return false;

                return null == _resource || _resource.IsVisibleResolved;
            }
        }

        #endregion // IsVisibleResolved

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
        /// Gets or sets the name of the calendar.
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

        #region OwningResource

        /// <summary>
        /// Returns the associated Resource.
        /// </summary>
        /// <seealso cref="Resource"/>
        public Resource OwningResource
        {
            get
            {
                return _resource;
            }
        }

        #endregion // OwningResource

        #region IsVisible

        /// <summary>
        /// Specifies whether this calendar is visible in the UI.
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

        #region BrushProviderToken

        internal object BrushProviderToken
        {
            get
            {
                object token = CoreUtilities.GetWeakReferenceTargetSafe(this._brushProviderTokenRef);

                if (token == null)
                {
                    token = new object();
                    this._brushProviderTokenRef = new WeakReference(token);
                }

                return token;
            }
        }

        #endregion // BrushProviderToken

        #region IsAutoCreated

        internal bool IsAutoCreated
        {
            get
            {
                return _isAutoCreated;
            }
        }

        #endregion // IsAutoCreated

		// AS 9/1/10 - TFS37171
		#region NeedsNewBrushProvider
		internal bool NeedsNewBrushProvider
		{
			get
			{
				// if we don't have a provider then we need one
				if (_brushProvider == null)
					return true;

				Color? baseColor = this.BaseColor;

				// if the base color we had when assigned hasn't changed 
				// then we can keep using the existing one
				if (_baseColorWhenAssigned == baseColor)
					return false;

				// if we went from having a color to not having one then 
				// we need a new provider since we don't know which color 
				// would have been used
				if (baseColor == null)
					return true;

				// if we've been assigned a color and that matches the 
				// current provider's color then we can keep the existing one
				return baseColor != _brushProvider.BaseColor;
			}
		} 
		#endregion // NeedsNewBrushProvider

        #region Storage

        internal IPropertyStorage<ResourceCalendar, int> Storage
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

        #region Private/Internal Methods


        #region BindToBrushVerion

        internal void BindToBrushVerion(DependencyObject target)
        {
            if (target != null)
                BindingOperations.SetBinding(target, ScheduleControlBase.BrushVersionProperty, this._brushVersionBinding);
        }

        #endregion //BindToBrushVerion	

    
        #region BumpBrushVersion

        private void BumpBrushVersion()
        {
            this._brushVersion++;
            this.RaisePropertyChangedEvent("BrushVersion");
        }

        #endregion //BumpBrushVersion	

        #region CreateDefaultCalendar

        internal static ResourceCalendar CreateDefaultCalendar( Resource resource )
        {
            ResourceCalendar calendar = new ResourceCalendar( resource );
            calendar._isAutoCreated = true;
            calendar.Id = "[default]";
			calendar.Name = resource.Name;

            return calendar;
        } 

        #endregion // CreateDefaultCalendar
    
        #region GetValueHelper

        private T GetValueHelper<T>( int property )
        {
            return this.Storage.GetValue<T>( this, property );
        }

        #endregion // GetValueHelper

		#region IdToken

		internal ViewItemManager<ResourceCalendar>.IdToken IdToken
		{
			get
			{
				return _dataItem as ViewItemManager<ResourceCalendar>.IdToken;
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

        #region InitializeBrushProvider

        internal void InitializeBrushProvider(CalendarBrushProvider provider)
        {
            if (this._brushProvider != provider)
            {
				// AS 9/2/10 TFS37171
				// We need to know what the base color was when the provider was assigned.
				//
				_baseColorWhenAssigned = this.BaseColor;

                // unwire the old provider's property change event
                if ( this._brushProvider != null )
                    this._brushProvider.PropertyChanged -= new PropertyChangedEventHandler(OnBrushProviderPropertyChanged);

                this._brushProvider = provider;
                
                // wire the new provider's property change event
                if ( this._brushProvider != null )
                    this._brushProvider.PropertyChanged += new PropertyChangedEventHandler(OnBrushProviderPropertyChanged);

                this.RaisePropertyChangedEvent("BrushProvider");

                this.BumpBrushVersion();
            }
        }

        void OnBrushProviderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
			if (e.PropertyName == "BrushVersion")
	            this.BumpBrushVersion();
        }

        #endregion //InitializeBrushProvider	
    
		#region Initialize

		internal void Initialize( IPropertyStorage<ResourceCalendar, int> storage )
		{
			StorageProps.Info.Instance.InitializeNewStore( this, ref _storage, storage );
		}

		#endregion // Initialize

        #region InitializeResource

        internal void InitializeResource( Resource resource )
        {
            if ( _resource != resource )
            {
                _resource = resource;
				this.RaisePropertyChangedEvent( StorageProps.Name_OwningResource );

                if ( null != _resource && string.IsNullOrEmpty( this.OwningResourceId ) )
                    this.SetValueHelper<string>( StorageProps.OwningResourceId, _resource.Id );
            }
        }

        #endregion // InitializeResource

        #region RaiseIsVisibleResolved

        internal void RaiseIsVisibleResolved( )
        {
            this.RaisePropertyChangedEvent( "IsVisibleResolved" );
        }

        #endregion // RaiseIsVisibleResolved

		#region RaisePropertyChangedEvent

		internal new void RaisePropertyChangedEvent( string propName )
		{
			base.RaisePropertyChangedEvent( propName );
		}

		#endregion // RaisePropertyChangedEvent

        #region SetValueHelper

        private void SetValueHelper<T>( int property, T newVal )
        {
            this.Storage.SetValue<T>( this, property, newVal );
        }

        #endregion // SetValueHelper  
        
        #endregion // Private/Internal Methods

        #endregion // Methods
    } 

    #endregion // ResourceCalendar
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