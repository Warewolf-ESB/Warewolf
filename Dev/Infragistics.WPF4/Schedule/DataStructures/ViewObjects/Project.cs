using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// Represents a Project object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// A project is an organized set of tasks.
	/// </para>
	/// </remarks>

	[InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]

	public class Project : PropertyChangeNotifierExtended
	{
		#region Member Vars






		private object _dataItem;

		private IPropertyStorage<Project, int> _storage;

		#endregion // Member Vars

		#region Data Structures

		#region StorageProps Class

		internal static class StorageProps
		{
			internal const int Id = 0;
			internal const int Start = 1;
			internal const int End = 2;
			internal const int ScheduleFromStart = 3;
			internal const int StatusDate = 4;
			internal const int DaysPerMonth = 5;
			internal const int HoursPerWeek = 6;
			internal const int HoursPerDay = 7;
			internal const int LastModifiedDate = 8;
			internal const int Resources = 9;
			internal const int Metadata = 10;

			internal const string Name_Resources = "Resources";

			internal class Info : StoragePropsInfo, ITypedPropertyChangeListener<Project, int>
			{
				/// <summary>
				/// Delegate used to get the storage instance from an project.
				/// </summary>
				protected readonly Func<Project, IPropertyStorage<Project, int>> _sr = i => i.Storage;

				protected override void Initialize( IMap<int, StoragePropsInfo.PropInfo> map )
				{
					StoragePropsInfo.PropInfo[] infos = new StoragePropsInfo.PropInfo[]
					{
						new TypedPropInfo<Project, string>( Id, "Id", _sr, ProjectProperty.Id ),
						new TypedPropInfo<Project, DateTime?>( Start, "Start", _sr, ProjectProperty.Start ),
						new TypedPropInfo<Project, DateTime?>( End, "End", _sr, ProjectProperty.End ),
						new TypedPropInfo<Project, bool>( ScheduleFromStart, "ScheduleFromStart", _sr, ProjectProperty.ScheduleFromStart, defaultValueFactoryFunc: ii => true ),
						new TypedPropInfo<Project, DateTime?>( StatusDate, "StatusDate", _sr, ProjectProperty.StatusDate ),
						new TypedPropInfo<Project, int>( DaysPerMonth, "DaysPerMonth", _sr, ProjectProperty.DaysPerMonth, defaultValueFactoryFunc: ii => 20 ),
						new TypedPropInfo<Project, int>( HoursPerWeek, "HoursPerWeek", _sr, ProjectProperty.HoursPerWeek, defaultValueFactoryFunc: ii => 40 ),
						new TypedPropInfo<Project, int>( HoursPerDay, "HoursPerDay", _sr, ProjectProperty.HoursPerDay, defaultValueFactoryFunc: ii => 8 ),
						new TypedPropInfo<Project, DateTime?>( LastModifiedDate, "LastModifiedDate", _sr, ProjectProperty.LastModifiedDate ),
						// Is this property a comma separated resource string ids or a collection of resources?
						new TypedPropInfo<Project, ObservableCollectionExtended<Resource>>( Resources, "Resources", _sr, ProjectProperty.Resources ),
						new TypedPropInfo<Project, MetadataPropertyValueStore>( Metadata, "Metadata", _sr, isReadOnly: true, copyMethod: CopyMethod.CopyContents, defaultValueFactoryFunc: CreateHelper<Project, DictionaryMetadataPropertyValueStore>, equalityComparer: new DelegateEqualityComparer<MetadataPropertyValueStore>( MetadataPropertyValueStore.HasSameValues, MetadataPropertyValueStore.GetHashCode ) )
					};		

					FillMap( infos, map );
				}

				private static Info g_instance = new Info( );

				internal static Info Instance
				{
					get
					{
						return g_instance;
					}
				}

				public void OnPropertyValueChanged( Project item, int property, object extraInfo )
				{
					PropInfo info = this.Props[property];

					Debug.Assert( null != info );

					if ( null != info )
					{
						item.RaisePropertyChangedEvent( info._name );
					}
				}

				internal IPropertyStorage<Project, int> CreateDefaultStorage( Project item )
				{
					return new DictionaryPropertyStorage<Project, int>( this, this.GetDefaultValueFactories( ) );
				}
			}
		}

		#endregion // StorageProps Class

		#endregion // Data Structures

		#region Properties

		#region Public Properties

		#region DataItem

		/// <summary>
		/// Gets the associated data item if any.
		/// </summary>
		public object DataItem
		{
			get
			{
				ViewItemManager<Project>.IdToken idToken = this.IdToken;

				if ( idToken != null )
					return idToken.DataItem;

				return _dataItem;
			}
			set
			{
				_dataItem = value;
			}
		}

		#endregion // DataItem

		#region Id

		/// <summary>
		/// Returns or sets the string id for the project.
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

		#region Start

		/// <summary>
		/// Returns or sets the start date for the project.
		/// </summary>
		public DateTime Start
		{
			get
			{
				return this.GetValueHelper<DateTime>( StorageProps.Start );
			}
			set
			{
				this.SetValueHelper<DateTime>( StorageProps.Start, value );
			}
		}

		#endregion // Start

		#region End

		/// <summary>
		/// Returns or sets the end date for the project.
		/// </summary>
		public DateTime End
		{
			get
			{
				return this.GetValueHelper<DateTime>( StorageProps.End );
			}
			set
			{
				this.SetValueHelper<DateTime>( StorageProps.End, value );
			}
		}

		#endregion // End

		#region ScheduleFromStart

		/// <summary>
		/// Returns or sets a boolean indicating if the schedule is calculated forward from the 
		/// start date. This property defaults to true.
		/// </summary>
		public bool ScheduleFromStart
		{
			get
			{
				return this.GetValueHelper<bool>( StorageProps.ScheduleFromStart );
			}
			set
			{
				this.SetValueHelper<bool>( StorageProps.ScheduleFromStart, value );
			}
		}

		#endregion // ScheduleFromStart

		#region StatusDate

		/// <summary>
		/// Returns or sets a nullable DateTime used as the current status date for the project.
		/// </summary>
		public DateTime? StatusDate
		{
			get
			{
				return this.GetValueHelper<DateTime?>( StorageProps.StatusDate );
			}
			set
			{
				this.SetValueHelper<DateTime?>( StorageProps.StatusDate, value );
			}
		}

		#endregion // StatusDate

		#region DaysPerMonth

		/// <summary>
		/// Returns or sets an integer indicating the default number of days per month. This property defaults to 20.
		/// </summary>
		public int DaysPerMonth
		{
			get
			{
				return this.GetValueHelper<int>( StorageProps.DaysPerMonth );
			}
			set
			{
				this.SetValueHelper<int>( StorageProps.DaysPerMonth, value );
			}
		}

		#endregion // DaysPerMonth

		#region HoursPerWeek

		/// <summary>
		/// Returns or sets an integer indicating the default number of hours per week. This property defaults to 40.
		/// </summary>
		public int HoursPerWeek
		{
			get
			{
				return this.GetValueHelper<int>( StorageProps.HoursPerWeek );
			}
			set
			{
				this.SetValueHelper<int>( StorageProps.HoursPerWeek, value );
			}
		}

		#endregion // HoursPerWeek

		#region HoursPerDay

		/// <summary>
		/// Returns or sets an integer indicating the default number of hours per day. This defaults to 8.
		/// </summary>
		public int HoursPerDay
		{
			get
			{
				return this.GetValueHelper<int>( StorageProps.HoursPerDay );
			}
			set
			{
				this.SetValueHelper<int>( StorageProps.HoursPerDay, value );
			}
		}

		#endregion // HoursPerDay

		#region LastModifiedDate

		/// <summary>
		/// Returns or sets a DateTime that represents the last modified time of the project or any of its tasks.
		/// </summary>
		public DateTime LastModifiedDate
		{
			get
			{
				return this.GetValueHelper<DateTime>( StorageProps.LastModifiedDate );
			}
			set
			{
				this.SetValueHelper<DateTime>( StorageProps.LastModifiedDate, value );
			}
		}

		#endregion // LastModifiedDate

		#region Resources

		/// <summary>
		/// Returns (or sets?) a collection of Resource instances that are specific to the project.
		/// </summary>
		public ObservableCollectionExtended<Resource> Resources
		{
			get
			{
				return this.GetValueHelper<ObservableCollectionExtended<Resource>>( StorageProps.Resources );
			}
			set
			{
				this.SetValueHelper<ObservableCollectionExtended<Resource>>( StorageProps.Resources, value );
			}
		}

		#endregion // Resources

		#region Metadata

		/// <summary>
		/// 
		/// </summary>
		public MetadataPropertyValueStore Metadata
		{
			get
			{
				return this.GetValueHelper<MetadataPropertyValueStore>( StorageProps.Metadata );
			}
			set
			{
				this.SetValueHelper<MetadataPropertyValueStore>( StorageProps.Metadata, value );
			}
		}

		#endregion // Metadata

		#endregion // Public Properties

		#region Internal Properties

		#region IdToken

		internal ViewItemManager<Project>.IdToken IdToken
		{
			get
			{
				return _dataItem as ViewItemManager<Project>.IdToken;
			}
			set
			{
				if ( _dataItem != value )
				{
					_dataItem = value;

					
					
					
				}
			}
		}

		#endregion // IdToken

		#region Storage

		internal IPropertyStorage<Project, int> Storage
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

		#region GetValueHelper

		internal T GetValueHelper<T>( int property )
		{
			return this.Storage.GetValue<T>( this, property );
		}

		#endregion // GetValueHelper

		#region Initialize

		/// <summary>
		/// Initializes the Project with a storage.
		/// </summary>
		/// <param name="storage">Storage instance that retrieves data from the underlying data item.</param>
		internal void Initialize( IPropertyStorage<Project, int> storage )
		{
			StorageProps.Info.Instance.InitializeNewStore( this, ref _storage, storage );
		}

		#endregion // Initialize

		#region RaisePropertyChangedEvent

		internal new void RaisePropertyChangedEvent( string propName )
		{
			base.RaisePropertyChangedEvent( propName );
		}

		#endregion // RaisePropertyChangedEvent

		#region SetValueHelper

		internal void SetValueHelper<T>( int property, T newVal )
		{
			this.Storage.SetValue<T>( this, property, newVal );
		}

		#endregion // SetValueHelper

		#endregion // Internal Methods 

		#endregion // Methods
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