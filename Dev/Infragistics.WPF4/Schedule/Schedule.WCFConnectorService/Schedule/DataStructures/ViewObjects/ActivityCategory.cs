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
	#region ActivityCategory Class

	
	

	/// <summary>
	/// Represents an activity category.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// Activity categories lets users categorize activities. Activities will be rendered
	/// using an appearance that is based on the category associated with the activity. This allows the user 
	/// to quickly identify activities that belong to a specific category.
	/// </para>
	/// <para class="body">
	/// Also note that an activity can be associated with multiple categories. An activity's <see cref="ActivityBase.Categories"/>
	/// property returns the categories that have been assigned to it. Categories are referenced by the string assigned to its <see cref="CategoryName"/> property. The <see cref="ActivityBase.Categories"/>
	/// property of the activity returns a string that contains the comma separated list of <see cref="CategoryName"/>s.
	/// The actual <see cref="ActivityCategory"/> instance associated with a particular <see cref="CategoryName"/> can be retrieved using the 
	/// XamScheduleDataManager's <see cref="XamScheduleDataManager.ResolveActivityCategories"/> method. 
	/// </para>
	/// </remarks>
	/// <seealso cref="CategoryName"/>
	/// <seealso cref="ActivityBase.Categories"/>
	/// <seealso cref="XamScheduleDataManager.ResolveActivityCategories"/>
	/// <seealso cref="Resource.CustomActivityCategories"/>
	/// <seealso cref="ListScheduleDataConnector.ActivityCategoryItemsSource"/>



	public class ActivityCategory : PropertyChangeNotifierExtended
	{
		#region Data Structures

		#region StorageProps Class

		internal static class StorageProps
		{
			internal const int CategoryName = 0;
			internal const int Description = 1;
			internal const int Color = 2;
			internal const int AppliesTo = 3;
			internal const int Metadata = 4;

			internal class Info : StoragePropsInfo, ITypedPropertyChangeListener<ActivityCategory, int>
			{
				internal PropertyStorage<ActivityCategory, int> _defaultStorage;

				protected override void Initialize( IMap<int, StoragePropsInfo.PropInfo> map )
				{
					Func<ActivityCategory, IPropertyStorage<ActivityCategory, int>> sr = i => i._storage;

					StoragePropsInfo.PropInfo[] infos = new StoragePropsInfo.PropInfo[]
					{
						new TypedPropInfo<ActivityCategory, string>( CategoryName, "CategoryName", sr, ActivityCategoryProperty.CategoryName ),
						new TypedPropInfo<ActivityCategory, string>( Description, "Description", sr, ActivityCategoryProperty.Description ),
                        new TypedPropInfo<ActivityCategory, Color?>( Color, "Color", sr, ActivityCategoryProperty.Color ),
						new TypedPropInfo<ActivityCategory, MetadataPropertyValueStore>( Metadata, "Metadata", sr, isReadOnly: true, copyMethod: CopyMethod.CopyContents, defaultValueFactoryFunc: CreateDefaultMetadata<ActivityCategory>, equalityComparer: new DelegateEqualityComparer<MetadataPropertyValueStore>( MetadataPropertyValueStore.HasSameValues, MetadataPropertyValueStore.GetHashCode ) )
					};

					FillMap( infos, map );

					this.InitDefaultStorage( );
				}

				private static Info g_instance = new Info( );

				internal static Info Instance
				{
					get
					{
						return g_instance;
					}
				}

				private void InitDefaultStorage( )
				{
					var stores = MapsFactory.CreateMapHelper<int, IPropertyStore>( );
					stores[CategoryName] = new DelegatePropertyStore<ActivityCategory, string>( i => i._categoryName, ( i, v ) => i._categoryName = v );
					stores[Description] = new DelegatePropertyStore<ActivityCategory, string>( i => i._description, ( i, v ) => i._description = v );
					stores[Color] = new DelegatePropertyStore<ActivityCategory, Color?>( i => i._color, ( i, v ) => i._color = v );
					stores[Metadata] = new DelegatePropertyStore<ActivityCategory, MetadataPropertyValueStore>( i => i._metadata, ( i, v ) => i._metadata = v );

					_defaultStorage = new PropertyStorage<ActivityCategory, int>( stores, this );
				}

				public void OnPropertyValueChanged( ActivityCategory item, int property, object extraInfo )
				{
					PropInfo info = this.Props[property];

					Debug.Assert( null != info );

					if ( null != info )
					{
						// When the value in the data source changes, update the member variable of the item.
						// 
						if ( _defaultStorage != item._storage && _defaultStorage != extraInfo )
							_defaultStorage.SetValue<object>( item, property, item._storage.GetValue<object>( item, property ) );

						item.RaisePropertyChangedEvent( info._name );
					}
				}
			}
		}

		#endregion // StorageProps Class

		#endregion // Data Structures

		#region Member Vars

		private string _categoryName;






		private object _dataItem;

		private string _description;
		private Color? _color;
		private IPropertyStorage<ActivityCategory, int> _storage = StorageProps.Info.Instance._defaultStorage;
		private MetadataPropertyValueStore _metadata;

		#endregion // Member Vars

		#region Properties

		#region Public Properties

		#region Color

		/// <summary>
		/// Specifies the color associated with the category. Activities of this category will visually identified using this color.
		/// </summary>
		public Color? Color
		{
			get
			{
				return _color;
			}
			set
			{
				this.SetValueHelper( StorageProps.Color, value );
			}
		}

		#endregion // Color

		#region CategoryName

		/// <summary>
		/// Gets or sets a string value that identifies the category.
		/// </summary>
		public string CategoryName
		{
			get
			{
				return _categoryName;
			}
			set
			{
				this.SetValueHelper( StorageProps.CategoryName, value );
			}
		}

		#endregion // CategoryName

		#region DataItem

		/// <summary>
		/// Gets the associated data item if any.
		/// </summary>
		public object DataItem
		{
			get
			{
				ViewItemManager<ActivityCategory>.IdToken idToken = this.IdToken;

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
		/// Specifies the description of the category.
		/// </summary>
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				this.SetValueHelper( StorageProps.Description, value );
			}
		}

		#endregion // Description

		#endregion // Public Properties 

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Clone

		/// <summary>
		/// Clones the ActivityCategory object.
		/// </summary>
		/// <returns>Clone of the object.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Clone</b> method is used for editing purpopses where the clone of the activity category is used to
		/// retain the original activity category data to be used in case the user cancels editing of activity category.
		/// </para>
		/// </remarks>
		public virtual ActivityCategory Clone( )
		{
			DictionaryMetadataPropertyValueStore cloneMetadata =
				null == _metadata ? null : new DictionaryMetadataPropertyValueStore( _metadata );

			ActivityCategory clone = new ActivityCategory( );
			return new ActivityCategory
			{
				CategoryName = this.CategoryName,
				Description = this.Description,
				Color = this.Color,
				_metadata = cloneMetadata
			};
		}

		#endregion // Clone 

		#endregion // Public Methods

		#region Internal Methods

		#region IdToken

		internal ViewItemManager<ActivityCategory>.IdToken IdToken
		{
			get
			{
				return _dataItem as ViewItemManager<ActivityCategory>.IdToken;
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

		#region Initialize

		/// <summary>
		/// Initializes the activity category with a storage.
		/// </summary>
		/// <param name="storage">Storage instance that retrieves data from the underlying data item.</param>
		internal void Initialize( IPropertyStorage<ActivityCategory, int> storage )
		{
			StorageProps.Info.Instance.InitializeNewStore( this, ref _storage, storage );

			// Copy property values to member variables.
			// 
			StorageProps.Info.Instance.CopyValues( this, _storage, this, StorageProps.Info.Instance._defaultStorage );
		}

		#endregion // Initialize

		#region RaisePropertyChangedEvent

		internal new void RaisePropertyChangedEvent( string propName )
		{
			base.RaisePropertyChangedEvent( propName );
		}

		#endregion // RaisePropertyChangedEvent 

		#endregion // Internal Methods

		#region Private Methods

		#region SetValueHelper

		private void SetValueHelper<T>( int property, T newVal )
		{
			_storage.SetValue<T>( this, property, newVal );
		}

		#endregion // SetValueHelper    

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // ActivityCategory Class
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