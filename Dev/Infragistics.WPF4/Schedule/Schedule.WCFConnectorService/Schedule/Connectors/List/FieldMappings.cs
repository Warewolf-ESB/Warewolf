using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Windows.Data;


#pragma warning disable 1574
using Infragistics.Services;
using Infragistics.Collections.Services;
using Infragistics.Controls.Schedules.Services;
using Infragistics;

namespace Infragistics.Services






{
	#region PropertyMappingBase<TKey> Class

	/// <summary>
	/// Contains information regarding a specific property of the underlying data object 
	/// and which property of the associated view object that data object property is 
	/// mapped to.
	/// </summary>
	/// <typeparam name="TKey">
	/// Either an enum that enumerates the properties of the view object or string that 
	/// identifies the property or the metadata key when used by the 
	/// <see cref="MetadataPropertyMapping"/>.
	/// </typeparam>
	public abstract class PropertyMappingBase<TKey>
	{
		#region Member Vars

		private TKey _key;
		private string _value;
		internal ConverterInfo _converterInfo = new ConverterInfo( );

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public PropertyMappingBase( )
		{
		}

		#endregion // Constructor

		#region ScheduleProperty

		/// <summary>
		/// Identifies a property on the XamSchedule object.
		/// </summary>
		/// <remarks>
		/// <b>ScheduleProperty</b> identifies a property on the XamSchedule object, like Apppointment or Resource.
		/// </remarks>
		internal TKey ScheduleProperty
		{
			get
			{
				return _key;
			}
			set
			{
				if ( !EqualityComparer<TKey>.Default.Equals( _key, value ) )
				{
					_key = value;
				}
			}
		}

		#endregion // ScheduleProperty

		#region DataObjectProperty

		/// <summary>
		/// Specifies the property of the data objects in the data source.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The value of this property of a data source object will be used for 
		/// the associated view object's property. The view object's property is
		/// specified using the appropriate property exposed by the view object
		/// specific mapping class derived from this class. For example, in the case
		/// of <see cref="AppointmentPropertyMapping"/>, <i>AppointmentPropertyMapping</i>'s
		/// <see cref="DataObjectProperty"/> identifies 
		/// the property of the data objects in the 
		/// <see cref="ListScheduleDataConnector.AppointmentItemsSource"/>.
		/// <i>AppointmentPropertyMapping</i>'s <see cref="AppointmentPropertyMapping.AppointmentProperty"/>
		/// identifies the property of the <see cref="Appointment"/> object, including properties
		/// of its base class <see cref="ActivityBase"/>, that this data object property maps to.
		/// </para>
		/// </remarks>
		/// <seealso cref="AppointmentPropertyMapping.AppointmentProperty"/>
		/// <seealso cref="JournalPropertyMapping.JournalProperty"/>
		/// <seealso cref="TaskPropertyMapping.TaskProperty"/>
		/// <seealso cref="ResourcePropertyMapping.ResourceProperty"/>
		/// <seealso cref="ResourceCalendarPropertyMapping.ResourceCalendarProperty"/>
		public string DataObjectProperty
		{
			get
			{
				return _value;
			}
			set
			{
				if ( _value != value )
				{
					_value = value;
				}
			}
		}

		#endregion // DataObjectProperty

		#region Converter

		/// <summary>
		/// Specifies the converter to use when converting between the schedule property's value
		/// and the data object's property's value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Converter's Convert method is used when copying value from the data source to the 
		/// schedule property. ConvertBack method is used when copying value from the schedule 
		/// property to the data source, which happens when the schedule property's value is 
		/// modified, for example by the user via appointment modification dialog.
		/// </para>
		/// </remarks>
		/// <seealso cref="ConverterParameter"/>
		public IValueConverter Converter
		{
			get
			{
				return _converterInfo._converter;
			}
			set
			{
				if ( _converterInfo._converter != value )
				{
					_converterInfo._converter = value;
				}
			}
		}

		#endregion // Converter

		#region ConverterParameter

		/// <summary>
		/// Specifies the converter parameter that will be passed in the Convert and ConvertFrom methods of
		/// the <see cref="Converter"/>. If null then the underlying data item will be passed.
		/// </summary>
		/// <seealso cref="Converter"/>
		public object ConverterParameter
		{
			get
			{
				return _converterInfo._parameter;
			}
			set
			{
				if ( _converterInfo._parameter != value )
				{
					_converterInfo._parameter = value;
				}
			}
		}

		#endregion // ConverterParameter
	}

	#endregion // PropertyMappingBase<TKey> Class

	#region MetadataPropertyMapping Class

	/// <summary>
	/// Used for mapping a field in the data source to an entry in the Metadata dictionary.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>MetadataPropertyMapping</b> used by the <see cref="MetadataPropertyMappingCollection"/>.
	/// MetadataPropertyMapping objects in the collection are used to populate the 
	/// metadata dictionary of the associated schedule object (for example the 
	/// <see cref="ActivityBase.Metadata"/> dictionary of the <see cref="Appointment"/> object).
	/// </para>
	/// </remarks>
	public class MetadataPropertyMapping : PropertyMappingBase<string>
	{
		#region MetadataProperty

		/// <summary>
		/// Specifies the key in the Metadata dictionary.
		/// </summary>
		public string MetadataProperty
		{
			get
			{
				return base.ScheduleProperty;
			}
			set
			{
				base.ScheduleProperty = value;
			}
		}

		#endregion // MetadataProperty
	}

	#endregion // MetadataPropertyMapping Class

	#region IPropertyMappingCollection<TKey> Interface

	/// <summary>
	/// This is used by ListManager.
	/// </summary>
	/// <typeparam name="TKey">Property key type.</typeparam>
	internal interface IPropertyMappingCollection<TKey> : IList
	{
		IEnumerable<TKey> Keys
		{
			get;
		}

		string this[TKey key]
		{
			get;
			set;
		}

		bool UseDefaultMappings
		{
			get;
		}

		MetadataPropertyMappingCollection MetadataPropertyMappings
		{
			get;
		}

		PropertyMappingBase<TKey> GetItem( TKey key );
	} 

	#endregion // IPropertyMappingCollection<TKey> Interface

	#region PropertyMappingCollection<TKey>  Class

	/// <summary>
	/// Base class for various field mapping collections used for specifying which field in the 
	/// schedule object points to which field in the data source.
	/// </summary>
	/// <typeparam name="TKey">
	/// Either an enum that enumerates the properties of the schedule object (for example when 
	/// used by the <see cref="AppointmentPropertyMappingCollection"/>) or string that identifies 
	/// the metadata key when used by the <see cref="MetadataPropertyMapping"/>.
	/// </typeparam>
	/// <typeparam name="TMapping">
	/// A <see cref="PropertyMappingBase&lt;TKey&gt;"/> derived class.
	/// </typeparam>
	public abstract class PropertyMappingCollection<TKey, TMapping> : ObservableCollectionExtended<TMapping>, IPropertyMappingCollection<TKey>
		where TMapping : PropertyMappingBase<TKey>, new( )
	{
		#region Member Vars

		private MetadataPropertyMappingCollection _metadataMappings;
		private bool _useDefaultMappings;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;"/>.
		/// </summary>
		public PropertyMappingCollection( )
			: base( false, true )
		{
		}

		#endregion // Constructor

		#region Indexers

		/// <summary>
		/// Returns the data member to which the specified schedule property or metadata entry is mapped to.
		/// </summary>
		/// <param name="key">Identifies the schedule property or metadata entry.</param>
		/// <returns>The data member or null if the mapping for the specified key has not been specified yet.</returns>
		public string this[TKey key]
		{
			get
			{
				TMapping item = this.GetItemWithKey( key );
				return null != item ? item.DataObjectProperty : null;
			}
			set
			{
				if ( string.IsNullOrEmpty( value ) )
				{
					this.Remove( key );
				}
				else
				{
					TMapping item = this.GetItemWithKey( key );
					if ( null != item )
					{
						item.DataObjectProperty = value;
					}
					else
					{
						item = this.CreateNew( key, value );
						this.Add( item );
					}
				}
			}
		}

		#endregion // Indexers

		#region Properties

		#region Public Properties
		
		#region MetadataPropertyMappings

		/// <summary>
		/// Used for defining metadata field mappings.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>MetadataPropertyMappings</b> property is used to specify fields from which 
		/// the Metadata (<see cref="ActivityBase.Metadata"/>) dictionary will be populated
		/// (as well as saved). An entry for each of the fields specified in this collection 
		/// will be created in the Metadata dictionary of the associated object. This can 
		/// be used in conjuction with a Metadata field mapping entry in the containing field 
		/// mapping collection in which case all the entries in the Metadata dictionary will
		/// be saved to and retrieved from that field except for the entries for which an 
		/// explicit mapping has been defined in the collection returned by this property. 
		/// </para>
		/// <para class="body">
		/// For example, let's say
		/// you have "A", "B" and "C" entries in the Metadata dictionary. You define an entry
		/// in the containining <see cref="PropertyMappingCollection&lt;T, TMapping&gt;"/> that specifies that the
		/// Metadata dictionary will be saved and retrieved from "Meta" field. You also define 
		/// an entry for "C" in the collection returned by this property. In such a scenario, the
		/// "A" and "B" values of the Metadata dictionary will be saved to the "Meta" field
		/// in the form of an XML document and the value of the "C" entry will be saved to
		/// the field specified by the associated mapping in the MetadataPropertyMappings collection.
		/// </para>
		/// </remarks>
		public MetadataPropertyMappingCollection MetadataPropertyMappings
		{
			get
			{
				if ( null == _metadataMappings && !( this is MetadataPropertyMappingCollection ) )
					_metadataMappings = new MetadataPropertyMappingCollection( );

				return _metadataMappings;
			}
			set
			{
				if ( _metadataMappings != value )
				{
					_metadataMappings = value;

					this.RaisePropertyChangedEvent( "MetadataPropertyMappings" );
				}
			}
		}

		#endregion // MetadataPropertyMappings

		#endregion // Public Properties

		#region Internal Properties

		#region UseDefaultMappings

		/// <summary>
		/// Specifies whether to use default mappings.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>UseDefaultMappings</b> property is used to automatically map fields in the data source.
		/// The name of the field in the data source is expected be the same as the name of the 
		/// member of the enum that enumerates the fields of the associated object whose member mappings
		/// are being provided (for example the <see cref="AppointmentProperty"/> enum).
		/// </para>
		/// </remarks>
		public bool UseDefaultMappings
		{
			get
			{
				return _useDefaultMappings;
			}
			set
			{
				if ( _useDefaultMappings != value )
				{
					_useDefaultMappings = value;

					this.RaisePropertyChangedEvent( "UseDefaultMappings" );
				}
			}
		}

		#endregion // UseDefaultMappings

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Protected Methods

		#region CreateNew

		/// <summary>
		/// Creates a new instance of mapping object that's compatible with this collection.
		/// </summary>
		/// <param name="key">Identifies the schedule property or metadata key depending on whether 
		/// this mappings collection is for a schedule class (like Appointment) or for metadata dictionary.</param>
		/// <param name="value">The name of the field in the underlying data source.</param>
		/// <returns>New instance of mapping object that's compatible with this collection.</returns>
		protected virtual TMapping CreateNew( TKey key, string value )
		{
			TMapping item = new TMapping( );
			item.ScheduleProperty = key;
			item.DataObjectProperty = value;

			return item;
		}

		#endregion // CreateNew

		#region RaisePropertyChangedEvent

		/// <summary>
		/// Raises PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">Property name.</param>
		protected void RaisePropertyChangedEvent( string propertyName )
		{
			this.OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
		} 

		#endregion // RaisePropertyChangedEvent

		#endregion // Protected Methods

		#region Public Methods

		#region GetItem

		/// <summary>
		/// Gets the <typeparamref name="TMapping"/> instance associated with the specified key.
		/// </summary>
		/// <param name="key">Identifies the property.</param>
		/// <returns>If a mapping for the specified key has been specified, returns the mapping object. Otherwise returns null.</returns>
		public TMapping GetItem( TKey key )
		{
			return this.GetItemWithKey( key );
		}

		#endregion // GetItem

		#region Remove

		/// <summary>
		/// Removes the mapping for the specified key.
		/// </summary>
		/// <param name="key">Key of the mapping to remove from the collection.</param>
		/// <returns>True if the item was found and removed. False if the item wasn't found.</returns>
		/// <remarks>
		/// <para class="body">
		/// If the mapping doesn't existing in the collection, this method will do nothing.
		/// </para>
		/// </remarks>
		public bool Remove( TKey key )
		{
			TMapping item = this.GetItemWithKey( key );
			if ( null != item )
				return this.Remove( item );

			return false;
		}

		#endregion // Remove
		
		#endregion // Public Methods

		#region Private Methods

		#region GetItemWithKey

		private TMapping GetItemWithKey( TKey key )
		{
			EqualityComparer<TKey> keyComparer = EqualityComparer<TKey>.Default;

			for ( int i = 0, count = this.Count; i < count; i++ )
			{
				TMapping ii = this[i];
				if ( keyComparer.Equals( ii.ScheduleProperty, key ) )
					return ii;
			}

			return null;
		}

		#endregion // GetItemWithKey
		
		#endregion // Private Methods

		#region Internal Methods

		#endregion // Internal Methods

		#endregion // Methods

		#region IPropertyMappingCollection<TKey> Interface Implementation

		PropertyMappingBase<TKey> IPropertyMappingCollection<TKey>.GetItem( TKey key )
		{
			return this.GetItemWithKey( key );
		}

		IEnumerable<TKey> IPropertyMappingCollection<TKey>.Keys
		{
			get
			{
				return from ii in this select ii.ScheduleProperty;
			}
		}

		#endregion // IPropertyMappingCollection<TKey> Interface Implementation
	}

	#endregion // PropertyMappingCollection<TKey> Class

	#region MetadataPropertyMappingCollection Class

	/// <summary>
	/// Collection used for specifying fields whose values will be added to the <see cref="ActivityBase.Metadata"/> dictionary.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>MetadataPropertyMappingCollection</b> class is used by the <see cref="PropertyMappingCollection&lt;T, TMapping&gt;.MetadataPropertyMappings"/>
	/// property. The underlying <see cref="ActivityBase"/> derived instance created from a data item in the data source for
	/// which these mappings are for will have its <see cref="ActivityBase.Metadata"/> dictionary populated with values
	/// from fields that are specified in this mappings collection. Changes to the Metadata dictionary will also be posted
	/// back to the data source based on these mappings.
	/// </para>
	/// </remarks>
	/// <seealso cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.MetadataPropertyMappings"/>
	/// <seealso cref="ActivityBase.Metadata"/>
	public class MetadataPropertyMappingCollection : PropertyMappingCollection<string, MetadataPropertyMapping>
	{
		/// <summary>
		/// Adds mapping for the metadata entry whose key is 'metadataKey'.
		/// </summary>
		/// <param name="metadataKey">Identifies the key of the entry in the associated schedule object's <see cref="ActivityBase.Metadata"/> dictionary.</param>
		/// <param name="dataSourceField">The data member in the data source.</param>
		public void Add( string metadataKey, string dataSourceField )
		{
			base.Add( this.CreateNew( metadataKey, dataSourceField ) );
		}
	}

	#endregion // MetadataPropertyMappingCollection Class
}


namespace Infragistics.Controls.Schedules.Services



{
	#region ActivityCategoryProperty Enum

	// SSP 12/8/10 - NAS11.1 Activity Categories
	// 

	/// <summary>
	/// Lists <see cref="ActivityCategory"/> properties that can be mapped to underlying data source.
	/// </summary>



	public enum ActivityCategoryProperty
	{
		/// <summary>
		/// Identifies <see cref="ActivityCategory.CategoryName"/> property.
		/// </summary>
		CategoryName,

		/// <summary>
		/// Identifies <see cref="ActivityCategory.Color"/> property.
		/// </summary>
		Color,

		/// <summary>
		/// Identifies <see cref="ActivityCategory.Description"/> property.
		/// </summary>
		Description
	}

	#endregion // ActivityCategoryProperty Enum

	#region ActivityProperty Enum

	/// <summary>
    /// Lists some properties that are common across all activity types.
    /// </summary>
    internal enum ActivityProperty
    {
        /// <summary>
        /// Designates a default value that doesn't map to any property.
        /// </summary>
        None,

        /// <summary>
        /// Identifies <see cref="ActivityBase.Id"/> property.
        /// </summary>
        Id,

        /// <summary>
        /// Identifies <see cref="ActivityBase.OwningResourceId"/> property.
        /// </summary>
        OwningResourceId,

        /// <summary>
        /// Identifies <see cref="ActivityBase.OwningCalendarId"/> property.
        /// </summary>
        OwningCalendarId,

        /// <summary>
        /// Identifies <see cref="ActivityBase.Start"/> property.
        /// </summary>
        Start,

		/// <summary>
		/// Identifies <see cref="ActivityBase.End"/> property.
		/// </summary>
		End,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsTimeZoneNeutral"/> property.
		/// </summary>		
		IsTimeZoneNeutral,

		/// <summary>
		/// Identifies <see cref="ActivityBase.StartTimeZoneId"/> property.
		/// </summary>
		StartTimeZoneId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.EndTimeZoneId"/> property.
		/// </summary>
		EndTimeZoneId,

		/// <summary>
        /// Identifies <see cref="ActivityBase.Subject"/> property.
        /// </summary>
        Subject,

        /// <summary>
        /// Identifies <see cref="ActivityBase.Description"/> property.
        /// </summary>
        Description,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsVisible"/> property.
		/// </summary>
		IsVisible,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Recurrence"/> property. Underlying data object property can be a string type.
		/// </summary>
		Recurrence,

		/// <summary>
		/// Identifies <see cref="ActivityBase.RecurrenceVersion"/> property.
		/// </summary>
		RecurrenceVersion,

		/// <summary>
		/// Identifies the property that contains the maximum date when the last of the occurrences
		/// can occur. This is used as an optimization when querying for recurring activities to
		/// limit the number of recurrences that have to be included in the result.
		/// </summary>
		MaxOccurrenceDateTime,

		/// <summary>
		/// Applies to variance of a recurrence.
		/// </summary>
		RootActivityId,

		/// <summary>
		/// Applies to variance of a recurrence.
		/// </summary>
		OriginalOccurrenceStart,

		/// <summary>
		/// Applies to variance of a recurrence.
		/// </summary>
		OriginalOccurrenceEnd,

		/// <summary>
		/// Applies to variance of a recurrence.
		/// </summary>
		IsOccurrenceDeleted,

		/// <summary>
		/// Applies to variance of a recurrence. Identifies <see cref="ActivityBase.VariantProperties"/> property.
		/// Underlying data object property can be a long type.
		/// </summary>
		VariantProperties,

		/// <summary>
		/// Applies to appointments and tasks. Doesn't apply to journals.
		/// </summary>
		ReminderInterval,

		/// <summary>
		/// Applies to appointments and tasks. Doesn't apply to journals.
		/// </summary>
		ReminderEnabled,

		/// <summary>
		/// Applies to appointments and tasks. Doesn't apply to journals.
		/// </summary>
		Reminder,

		/// <summary>
		/// The time when the activity was last modified.
		/// </summary>
		LastModifiedTime,

		/// <summary>
		/// Used to store data for any unmapped properties. The data will be stored as XML.
		/// </summary>
		UnmappedProperties
    } 

    #endregion // ActivityProperty Enum

	#region AppointmentProperty Enum

	/// <summary>
    /// Lists <see cref="Appointment"/> properties that can be mapped to underlying data source.
	/// </summary>
	/// <seealso cref="ListScheduleDataConnector.AppointmentPropertyMappings"/>
	public enum AppointmentProperty
	{
		/// <summary>
		/// Identifies <see cref="ActivityBase.Id"/> property.
		/// </summary>
		Id,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OwningResourceId"/> property.
		/// </summary>
		OwningResourceId,

        /// <summary>
        /// Identifies <see cref="ActivityBase.OwningCalendarId"/> property.
        /// </summary>
        OwningCalendarId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Start"/> property.
		/// </summary>
		Start,

		/// <summary>
		/// Identifies <see cref="ActivityBase.End"/> property.
		/// </summary>
		End,

		/// <summary>
		/// Identifies <see cref="ActivityBase.StartTimeZoneId"/> property.
		/// </summary>
		StartTimeZoneId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.EndTimeZoneId"/> property.
		/// </summary>
		EndTimeZoneId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsTimeZoneNeutral"/> property.
		/// </summary>
		IsTimeZoneNeutral,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Subject"/> property.
		/// </summary>
		Subject,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Description"/> property.
		/// </summary>
		Description,

		/// <summary>
		/// Identifies <see cref="Appointment.Location"/> property.
		/// </summary>
		Location,

		/// <summary>
		/// Identifies <see cref="ActivityBase.ReminderInterval"/> property.
		/// </summary>
		ReminderInterval,

		/// <summary>
		/// Identifies <see cref="ActivityBase.ReminderEnabled"/> property.
		/// </summary>
		ReminderEnabled,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Reminder"/> property.
		/// </summary>
		Reminder,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsVisible"/> property.
		/// </summary>
		IsVisible,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsLocked"/> property.
		/// </summary>
		IsLocked,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Recurrence"/> property.
		/// </summary>
		Recurrence,

		/// <summary>
		/// Identifies <see cref="ActivityBase.RecurrenceVersion"/> property.
		/// </summary>
		RecurrenceVersion,

		/// <summary>
		/// Identifies <see cref="ActivityBase.RootActivityId"/> property.
		/// </summary>
		RootActivityId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OriginalOccurrenceStart"/> property.
		/// </summary>
		OriginalOccurrenceStart,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OriginalOccurrenceEnd"/> property.
		/// </summary>
		OriginalOccurrenceEnd,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsOccurrenceDeleted"/> property.
		/// </summary>
		IsOccurrenceDeleted,

		/// <summary>
		/// Applies to variance of a recurrence. Identifies <see cref="ActivityBase.VariantProperties"/> property.
		/// </summary>
		VariantProperties,

		/// <summary>
		/// Identifies <see cref="ActivityBase.MaxOccurrenceDateTime"/> property.
		/// </summary>
		MaxOccurrenceDateTime,

		/// <summary>
		/// Identifies <see cref="ActivityBase.LastModifiedTime"/> property.
		/// </summary>
		LastModifiedTime,

		/// <summary>
		/// Used to store data for any unmapped properties. The data will be stored as XML.
		/// </summary>
		UnmappedProperties,

		// SSP 12/8/10 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Identifies <see cref="ActivityBase.Categories"/> property.
		/// </summary>



		Categories
	}

	#endregion // AppointmentProperty Enum

	#region JournalProperty Enum

	/// <summary>
	/// Lists <see cref="Journal"/> properties that can be mapped to underlying data source.
	/// </summary>
	/// <seealso cref="ListScheduleDataConnector.JournalPropertyMappings"/>
	public enum JournalProperty
	{
		/// <summary>
		/// Identifies <see cref="ActivityBase.Id"/> property.
		/// </summary>
		Id,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OwningResourceId"/> property.
		/// </summary>
		OwningResourceId,

        /// <summary>
        /// Identifies <see cref="ActivityBase.OwningCalendarId"/> property.
        /// </summary>
        OwningCalendarId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Start"/> property.
		/// </summary>
		Start,

		/// <summary>
		/// Identifies <see cref="ActivityBase.End"/> property.
		/// </summary>
		End,

		/// <summary>
		/// Identifies <see cref="ActivityBase.StartTimeZoneId"/> property.
		/// </summary>
		StartTimeZoneId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.EndTimeZoneId"/> property.
		/// </summary>
		EndTimeZoneId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsTimeZoneNeutral"/> property.
		/// </summary>
		IsTimeZoneNeutral,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Subject"/> property.
		/// </summary>
		Subject,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Description"/> property.
		/// </summary>
		Description,

		/// <summary>
		/// Identifies <see cref="ActivityBase.ReminderInterval"/> property.
		/// </summary>
		ReminderInterval,

		/// <summary>
		/// Identifies <see cref="ActivityBase.ReminderEnabled"/> property.
		/// </summary>
		ReminderEnabled,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Reminder"/> property.
		/// </summary>
		Reminder,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsVisible"/> property.
		/// </summary>
		IsVisible,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsLocked"/> property.
		/// </summary>
		IsLocked,

		/// <summary>
		/// Identifies <see cref="ActivityBase.RootActivityId"/> property.
		/// </summary>
		RootActivityId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OriginalOccurrenceStart"/> property.
		/// </summary>
		OriginalOccurrenceStart,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OriginalOccurrenceEnd"/> property.
		/// </summary>
		OriginalOccurrenceEnd,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsOccurrenceDeleted"/> property.
		/// </summary>
		IsOccurrenceDeleted,

		/// <summary>
		/// Applies to variance of a recurrence. Identifies <see cref="ActivityBase.VariantProperties"/> property.
		/// </summary>
		VariantProperties,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Recurrence"/> property.
		/// </summary>
		Recurrence,

		/// <summary>
		/// Identifies <see cref="ActivityBase.RecurrenceVersion"/> property.
		/// </summary>
		RecurrenceVersion,

		/// <summary>
		/// Identifies <see cref="ActivityBase.MaxOccurrenceDateTime"/> property.
		/// </summary>
		MaxOccurrenceDateTime,

		/// <summary>
		/// Identifies <see cref="ActivityBase.LastModifiedTime"/> property.
		/// </summary>
		LastModifiedTime,

		/// <summary>
		/// Used to store data for any unmapped properties. The data will be stored as XML.
		/// </summary>
		UnmappedProperties,

		// SSP 12/8/10 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Identifies <see cref="ActivityBase.Categories"/> property.
		/// </summary>



		Categories
	}

	#endregion // JournalProperty Enum

	#region ProjectProperty Enum

	// SSP 1/6/12 - NAS12.1 XamGantt
	// 

//    /// <summary>
//    /// Lists <see cref="Project"/> properties that can be mapped to underlying data source.
//    /// </summary>
//#if WPF && !WCFService
//    [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//    public enum ProjectProperty
//    {
//        /// <summary>
//        /// Identifies <see cref="Project.Id"/> property.
//        /// </summary>
//        Id,

//        /// <summary>
//        /// Identifies <see cref="Project.Start"/> property.
//        /// </summary>
//        Start,

//        /// <summary>
//        /// Identifies <see cref="Project.End"/> property.
//        /// </summary>
//        End,

//        /// <summary>
//        /// Identifies <see cref="Project.ScheduleFromStart"/> property.
//        /// </summary>
//        ScheduleFromStart,

//        /// <summary>
//        /// Identifies <see cref="Project.StatusDate"/> property.
//        /// </summary>
//        StatusDate,

//        /// <summary>
//        /// Identifies <see cref="Project.DaysPerMonth"/> property.
//        /// </summary>
//        DaysPerMonth,

//        /// <summary>
//        /// Identifies <see cref="Project.HoursPerWeek"/> property.
//        /// </summary>
//        HoursPerWeek,

//        /// <summary>
//        /// Identifies <see cref="Project.HoursPerDay"/> property.
//        /// </summary>
//        HoursPerDay,

//        /// <summary>
//        /// Identifies <see cref="Project.LastModifiedDate"/> property.
//        /// </summary>
//        LastModifiedDate,

//        /// <summary>
//        /// Identifies <see cref="Project.Resources"/> property.
//        /// </summary>
//        Resources
//    }

	#endregion // ProjectProperty Enum

	#region ResourceProperty Enum

	/// <summary>
	/// Lists <see cref="Resource"/> properties that can be mapped to underlying data source.
	/// </summary>
	/// <seealso cref="ListScheduleDataConnector.ResourcePropertyMappings"/>
	public enum ResourceProperty
	{
		/// <summary>
		/// Identifies <see cref="Resource.Id"/> property.
		/// </summary>
		Id,

		/// <summary>
		/// Identifies <see cref="Resource.IsVisible"/> property.
		/// </summary>
		IsVisible,

		/// <summary>
		/// Identifies <see cref="Resource.IsLocked"/> property.
		/// </summary>
		IsLocked,

		/// <summary>
		/// Identifies <see cref="Resource.Name"/> property.
		/// </summary>
		Name,

        /// <summary>
        /// Identifies <see cref="Resource.EmailAddress"/> property.
        /// </summary>
        EmailAddress,

        /// <summary>
        /// Identifies <see cref="Resource.Description"/> property.
        /// </summary>
        Description,

        /// <summary>
        /// Identifies <see cref="Resource.PrimaryCalendarId"/> property.
        /// </summary>
        PrimaryCalendarId,

		/// <summary>
		/// Identifies <see cref="Resource.PrimaryTimeZoneId"/> property.
		/// </summary>
		PrimaryTimeZoneId,

		/// <summary>
		/// Identifies <see cref="Resource.FirstDayOfWeek"/> property.
		/// </summary>
		FirstDayOfWeek,

		/// <summary>
		/// Identifies <see cref="Resource.DaysOfWeek"/> property.
		/// </summary>
		DaysOfWeek,

		/// <summary>
		/// Identifies <see cref="Resource.DaySettingsOverrides"/> property.
		/// </summary>
		DaySettingsOverrides,

		/// <summary>
		/// Used to store data for any unmapped properties. The data will be stored as XML.
		/// </summary>
		UnmappedProperties,

		// SSP 12/8/10 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Identifies <see cref="Resource.CustomActivityCategories"/> property.
		/// </summary>



		CustomActivityCategories
	}

	#endregion // ResourceProperty Enum

    #region ResourceCalendarProperty Enum

	/// <summary>
	/// Lists <see cref="ResourceCalendar"/> properties that can be mapped to underlying data source.
	/// </summary>
	/// <seealso cref="ListScheduleDataConnector.ResourceCalendarPropertyMappings"/>
    public enum ResourceCalendarProperty
    {
        /// <summary>
        /// Identifies <see cref="Resource.Id"/> property.
        /// </summary>
        Id,

        /// <summary>
        /// Identifies the <see cref="ResourceCalendar.OwningResourceId"/> property.
        /// </summary>
        OwningResourceId,

        /// <summary>
        /// Identifies <see cref="ResourceCalendar.Name"/> property.
        /// </summary>
        Name,

        /// <summary>
        /// Identifies <see cref="ResourceCalendar.Description"/> property.
        /// </summary>
        Description,

        /// <summary>
        /// Identifies <see cref="ResourceCalendar.IsVisible"/> property.
        /// </summary>
        IsVisible,

        /// <summary>
        /// Identifies <see cref="ResourceCalendar.BaseColor"/> property.
        /// </summary>
        BaseColor,

		/// <summary>
		/// Used to store data for any unmapped properties. The data will be stored as XML.
		/// </summary>
		UnmappedProperties
    }

    #endregion // ResourceCalendarProperty Enum

    #region TaskProperty Enum

	/// <summary>
	/// Lists <see cref="Task"/> properties that can be mapped to underlying data source.
	/// </summary>
	/// <seealso cref="ListScheduleDataConnector.TaskPropertyMappings"/>
	public enum TaskProperty
	{
		/// <summary>
		/// Identifies <see cref="ActivityBase.Id"/> property.
		/// </summary>
		Id,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OwningResourceId"/> property.
		/// </summary>
		OwningResourceId,

        /// <summary>
        /// Identifies <see cref="ActivityBase.OwningCalendarId"/> property.
        /// </summary>
        OwningCalendarId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Start"/> property.
		/// </summary>
		Start,

        /// <summary>
        /// Identifies <see cref="ActivityBase.End"/> property.
        /// </summary>
        End,

		/// <summary>
		/// Identifies <see cref="ActivityBase.StartTimeZoneId"/> property.
		/// </summary>
		StartTimeZoneId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.EndTimeZoneId"/> property.
		/// </summary>
		EndTimeZoneId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsTimeZoneNeutral"/> property.
		/// </summary>
		IsTimeZoneNeutral,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Subject"/> property.
		/// </summary>
		Subject,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Description"/> property.
		/// </summary>
		Description,

		/// <summary>
		/// Identifies <see cref="ActivityBase.ReminderInterval"/> property.
		/// </summary>
		ReminderInterval,

		/// <summary>
		/// Identifies <see cref="ActivityBase.ReminderEnabled"/> property.
		/// </summary>
		ReminderEnabled,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Reminder"/> property.
		/// </summary>
		Reminder,

		/// <summary>
		/// Identifies <see cref="Task.PercentComplete"/> property.
		/// </summary>
		PercentageComplete,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsVisible"/> property.
		/// </summary>
		IsVisible,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsLocked"/> property.
		/// </summary>
		IsLocked,

		/// <summary>
		/// Identifies <see cref="ActivityBase.RootActivityId"/> property.
		/// </summary>
		RootActivityId,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OriginalOccurrenceStart"/> property.
		/// </summary>
		OriginalOccurrenceStart,

		/// <summary>
		/// Identifies <see cref="ActivityBase.OriginalOccurrenceEnd"/> property.
		/// </summary>
		OriginalOccurrenceEnd,

		/// <summary>
		/// Identifies <see cref="ActivityBase.IsOccurrenceDeleted"/> property.
		/// </summary>
		IsOccurrenceDeleted,

		/// <summary>
		/// Applies to variance of a recurrence. Identifies <see cref="ActivityBase.VariantProperties"/> property.
		/// </summary>
		VariantProperties,

		/// <summary>
		/// Identifies <see cref="ActivityBase.Recurrence"/> property.
		/// </summary>
		Recurrence,

		/// <summary>
		/// Identifies <see cref="ActivityBase.RecurrenceVersion"/> property.
		/// </summary>
		RecurrenceVersion,

		/// <summary>
		/// Identifies <see cref="ActivityBase.MaxOccurrenceDateTime"/> property.
		/// </summary>
		MaxOccurrenceDateTime,

		/// <summary>
		/// Identifies <see cref="ActivityBase.LastModifiedTime"/> property.
		/// </summary>
		LastModifiedTime,

		/// <summary>
		/// Used to store data for any unmapped properties. The data will be stored as XML.
		/// </summary>
		UnmappedProperties,

		// SSP 12/8/10 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Identifies <see cref="ActivityBase.Categories"/> property.
		/// </summary>



		Categories,


		// SSP 1/5/12 - NAS12.1 Gantt
		// Added the following properties for Gantt.
		// 

		///// <summary>
		///// Identifies <see cref="Task.UniqueIdPredecessors"/> property.
		///// </summary>
		//UniqueIdPredecessors,

		///// <summary>
		///// Identifies <see cref="Task.Tasks"/> property.
		///// </summary>
		//Tasks,

		///// <summary>
		///// Identifies <see cref="Task.ResourceNames"/> property.
		///// </summary>
		//ResourceNames,

		///// <summary>
		///// Identifies <see cref="Task.ConstraintType"/> property.
		///// </summary>
		//ConstraintType,

		///// <summary>
		///// Identifies <see cref="Task.ConstraintDate"/> property.
		///// </summary>
		//ConstraintDate,

		///// <summary>
		///// Identifies <see cref="Task.Deadline"/> property.
		///// </summary>
		//Deadline,

		///// <summary>
		///// Identifies <see cref="Task.Expanded"/> property.
		///// </summary>
		//Expanded,

		///// <summary>
		///// Identifies <see cref="Task.RowHeight"/> property.
		///// </summary>
		//RowHeight,

		///// <summary>
		///// Identifies <see cref="Task.Manual"/> property.
		///// </summary>
		//Manual,

		///// <summary>
		///// Identifies <see cref="Task.Milestone"/> property.
		///// </summary>
		//Milestone,

		///// <summary>
		///// Identifies <see cref="Task.ProjectId"/> property.
		///// </summary>
		//ProjectId,

		///// <summary>
		///// Identifies <see cref="Task.HideBar"/> property.
		///// </summary>
		//HideBar,

		///// <summary>
		///// Identifies <see cref="Task.Duration"/> property.
		///// </summary>
		//Duration,

		///// <summary>
		///// Identifies <see cref="Task.Rollup"/> property.
		///// </summary>
		//Rollup,

		///// <summary>
		///// Identifies <see cref="Task.Marked"/> property.
		///// </summary>
		//Marked,

		///// <summary>
		///// Identifies <see cref="Task.Active"/> property.
		///// </summary>
		//Active,

		///// <summary>
		///// Identifies <see cref="Task.DurationText"/> property.
		///// </summary>
		//DurationText,

		///// <summary>
		///// Identifies <see cref="Task.StartText"/> property.
		///// </summary>
		//StartText,

		///// <summary>
		///// Identifies <see cref="Task.EndText"/> property.
		///// </summary>
		//EndText,

		///// <summary>
		///// Identifies <see cref="Task.Splits"/> property.
		///// </summary>
		//Splits,

		///// <summary>
		///// Identifies <see cref="Task.BaselineStart"/> property.
		///// </summary>
		//BaselineStart,

		///// <summary>
		///// Identifies <see cref="Task.BaselineEnd"/> property.
		///// </summary>
		//BaselineEnd,

		///// <summary>
		///// Identifies <see cref="Task.BaselineDuration"/> property.
		///// </summary>
		//BaselineDuration
	}

	#endregion // TaskProperty Enum



	#region ActivityCategoryPropertyMapping Class

	// SSP 12/8/10 - NAS11.1 Activity Categories
	// 

	/// <summary>
	/// Used for mapping a field in the data source to a property of the <see cref="ActivityCategory"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ActivityCategoryPropertyMapping</b> object contains information regarding a specific field in the
	/// data source specified by the <see cref="ListScheduleDataConnector.ActivityCategoryItemsSource"/>
	/// and which property of the <see cref="ActivityCategory"/> object it maps to. The property of the
	/// <i>ActivityCategory</i> object is identified by the <see cref="ActivityCategoryPropertyMapping.ActivityCategoryProperty"/>
	/// property and the field in the data source is identified by the <see cref="PropertyMappingBase&lt;ActivityCategoryProperty&gt;.DataObjectProperty"/>
	/// property.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.ActivityCategoryItemsSource"/>
	/// <seealso cref="ListScheduleDataConnector.ActivityCategoryPropertyMappings"/>
	/// <seealso cref="ActivityCategoryPropertyMapping.ActivityCategoryProperty"/>
	/// <seealso cref="PropertyMappingBase&lt;ActivityCategoryProperty&gt;.DataObjectProperty"/>



	public class ActivityCategoryPropertyMapping : PropertyMappingBase<ActivityCategoryProperty>
	{
		#region ActivityCategoryProperty

		/// <summary>
		/// Identifies a property of <see cref="ActivityCategory"/> object.
		/// </summary>
		/// <remarks>
		/// <b>ActivityCategoryProperty</b> identifies a property of the <see cref="ActivityCategory"/> object.
		/// </remarks>
		public ActivityCategoryProperty ActivityCategoryProperty
		{
			get
			{
				return base.ScheduleProperty;
			}
			set
			{
				base.ScheduleProperty = value;
			}
		}

		#endregion // ActivityCategoryProperty
	}

	#endregion // ActivityCategoryPropertyMapping Class

	#region AppointmentPropertyMapping Class

	/// <summary>
	/// Used for mapping a field in the data source to a property of the <see cref="Appointment"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>AppointmentPropertyMapping</b> object contains information regarding a specific field in the
	/// data source specified by the <see cref="ListScheduleDataConnector.AppointmentItemsSource"/>
	/// and which property of the <see cref="Appointment"/> object it maps to. The property of the
	/// <i>Appointment</i> object is identified by the <see cref="AppointmentPropertyMapping.AppointmentProperty"/>
	/// property and the field in the data source is identified by the <see cref="PropertyMappingBase&lt;AppointmentProperty&gt;.DataObjectProperty"/>
	/// property.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.AppointmentItemsSource"/>
	/// <seealso cref="ListScheduleDataConnector.AppointmentPropertyMappings"/>
	/// <seealso cref="AppointmentPropertyMapping.AppointmentProperty"/>
	/// <see cref="PropertyMappingBase&lt;AppointmentProperty&gt;.DataObjectProperty"/>
	public class AppointmentPropertyMapping : PropertyMappingBase<AppointmentProperty>
	{
		#region AppointmentProperty

		/// <summary>
		/// Identifies a property of <see cref="Appointment"/> object.
		/// </summary>
		/// <remarks>
		/// <b>AppointmentProperty</b> identifies a property of the <see cref="Appointment"/> object.
		/// </remarks>
		public AppointmentProperty AppointmentProperty
		{
			get
			{
				return base.ScheduleProperty;
			}
			set
			{
				base.ScheduleProperty = value;
			}
		}

		#endregion // AppointmentProperty
	}

	#endregion // AppointmentPropertyMapping Class

	#region JournalPropertyMapping Class

	/// <summary>
	/// Used for mapping a field in the data source to a property of the <see cref="Journal"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>JournalPropertyMapping</b> object contains information regarding a specific field in the
	/// data source specified by the <see cref="ListScheduleDataConnector.JournalItemsSource"/>
	/// and which property of the <see cref="Journal"/> object it maps to. The property of the
	/// <i>Journal</i> object is identified by the <see cref="JournalPropertyMapping.JournalProperty"/>
    /// property and the field in the data source is identified by the <see cref="PropertyMappingBase&lt;JournalProperty&gt;.DataObjectProperty"/>
	/// property.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.JournalItemsSource"/>
	/// <seealso cref="ListScheduleDataConnector.JournalPropertyMappings"/>
	/// <seealso cref="JournalPropertyMapping.JournalProperty"/>
    /// <seealso cref="PropertyMappingBase&lt;JournalProperty&gt;.DataObjectProperty"/>
	public class JournalPropertyMapping : PropertyMappingBase<JournalProperty>
	{
		#region JournalProperty

		/// <summary>
		/// Identifies a property of <see cref="Journal"/> object.
		/// </summary>
		/// <remarks>
		/// <b>JournalProperty</b> identifies a property of the <see cref="Journal"/> object.
		/// </remarks>
		public JournalProperty JournalProperty
		{
			get
			{
				return base.ScheduleProperty;
			}
			set
			{
				base.ScheduleProperty = value;
			}
		}

		#endregion // JournalProperty
	}

	#endregion // JournalPropertyMapping Class	

	#region ProjectPropertyMapping Class

	// SSP 1/6/12 - NAS12.1 XamGantt
	// 

//    /// <summary>
//    /// Used for mapping a field in the data source to a property of the <see cref="Project"/> object.
//    /// </summary>
//    /// <remarks>
//    /// <para class="body">
//    /// <b>ProjectPropertyMapping</b> object contains information regarding a specific field in the
//    /// data source specified by the <see cref="ListScheduleDataConnector.ProjectItemsSource"/>
//    /// and which property of the <see cref="Project"/> object it maps to. The property of the
//    /// <i>Project</i> object is identified by the <see cref="ProjectPropertyMapping.ProjectProperty"/>
//    /// property and the field in the data source is identified by the <see cref="PropertyMappingBase&lt;Project&gt;.DataObjectProperty"/>
//    /// property.
//    /// </para>
//    /// </remarks>
//    /// <seealso cref="ListScheduleDataConnector.ProjectItemsSource"/>
//    /// <seealso cref="ListScheduleDataConnector.ProjectPropertyMappings"/>
//    /// <seealso cref="ProjectPropertyMapping.ProjectProperty"/>
//    /// <seealso cref="PropertyMappingBase&lt;ProjectProperty&gt;.DataObjectProperty"/>
//#if WPF && !WCFService
//    [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//    public class ProjectPropertyMapping : PropertyMappingBase<ProjectProperty>
//    {
//        #region ProjectProperty

//        /// <summary>
//        /// Identifies a property of <see cref="Project"/> object.
//        /// </summary>
//        /// <remarks>
//        /// <b>ProjectProperty</b> identifies a property of the <see cref="Project"/> object.
//        /// </remarks>
//        public ProjectProperty ProjectProperty
//        {
//            get
//            {
//                return base.ScheduleProperty;
//            }
//            set
//            {
//                base.ScheduleProperty = value;
//            }
//        }

//        #endregion // ProjectProperty
//    }

	#endregion // ProjectPropertyMapping Class

	#region ResourcePropertyMapping Class

	/// <summary>
	/// Used for mapping a field in the data source to a property of the <see cref="Resource"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ResourcePropertyMapping</b> object contains information regarding a specific field in the
	/// data source specified by the <see cref="ListScheduleDataConnector.ResourceItemsSource"/>
	/// and which property of the <see cref="Resource"/> object it maps to. The property of the
	/// <i>Resource</i> object is identified by the <see cref="ResourcePropertyMapping.ResourceProperty"/>
    /// property and the field in the data source is identified by the <see cref="PropertyMappingBase&lt;ResourceProperty&gt;.DataObjectProperty"/>
	/// property.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.ResourceItemsSource"/>
	/// <seealso cref="ListScheduleDataConnector.ResourcePropertyMappings"/>
	/// <seealso cref="ResourcePropertyMapping.ResourceProperty"/>
    /// <seealso cref="PropertyMappingBase&lt;ResourceProperty&gt;.DataObjectProperty"/>
	public class ResourcePropertyMapping : PropertyMappingBase<ResourceProperty>
	{
		#region ResourceProperty

		/// <summary>
		/// Identifies a property of <see cref="Resource"/> object.
		/// </summary>
		/// <remarks>
		/// <b>ResourceProperty</b> identifies a property of the <see cref="Resource"/> object.
		/// </remarks>
		public ResourceProperty ResourceProperty
		{
			get
			{
				return base.ScheduleProperty;
			}
			set
			{
				base.ScheduleProperty = value;
			}
		}

		#endregion // ResourceProperty
	}

	#endregion // ResourcePropertyMapping Class

    #region ResourceCalendarPropertyMapping Class

    /// <summary>
    /// Used for mapping a field in the data source to a property of the <see cref="ResourceCalendar"/> object.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <b>ResourceCalendarPropertyMapping</b> object contains information regarding a specific field in the
    /// data source specified by the <see cref="ListScheduleDataConnector.ResourceCalendarItemsSource"/>
    /// and which property of the <see cref="ResourceCalendar"/> object it maps to. The property of the
	/// <i>ResourceCalendar</i> object is identified by the <see cref="ResourceCalendarPropertyMapping.ResourceCalendarProperty"/>
    /// property and the field in the data source is identified by the <see cref="PropertyMappingBase&lt;ResourceCalendarProperty&gt;.DataObjectProperty"/>
    /// property.
    /// </para>
    /// </remarks>
    /// <seealso cref="ListScheduleDataConnector.ResourceCalendarItemsSource"/>
    /// <seealso cref="ListScheduleDataConnector.ResourceCalendarPropertyMappings"/>
	/// <seealso cref="ResourceCalendarPropertyMapping.ResourceCalendarProperty"/>
    /// <seealso cref="PropertyMappingBase&lt;ResourceCalendarProperty&gt;.DataObjectProperty"/>
    public class ResourceCalendarPropertyMapping : PropertyMappingBase<ResourceCalendarProperty>
    {
		#region ResourceCalendarProperty

		/// <summary>
		/// Identifies a property of <see cref="ResourceCalendar"/> object.
		/// </summary>
		/// <remarks>
		/// <b>ResourceCalendarProperty</b> identifies a property of the <see cref="ResourceCalendar"/> object.
		/// </remarks>
		public ResourceCalendarProperty ResourceCalendarProperty
		{
			get
			{
				return base.ScheduleProperty;
			}
			set
			{
				base.ScheduleProperty = value;
			}
		}

		#endregion // ResourceCalendarProperty
    }

    #endregion // ResourceCalendarPropertyMapping Class

    #region TaskPropertyMapping Class

    /// <summary>
	/// Used for mapping a field in the data source to a property of the <see cref="Task"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>TaskPropertyMapping</b> object contains information regarding a specific field in the
	/// data source specified by the <see cref="ListScheduleDataConnector.TaskItemsSource"/>
	/// and which property of the <see cref="Task"/> object it maps to. The property of the
	/// <i>Task</i> object is identified by the <see cref="TaskPropertyMapping.TaskProperty"/>
    /// property and the field in the data source is identified by the <see cref="PropertyMappingBase&lt;TaskProperty&gt;.DataObjectProperty"/>
	/// property.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.TaskItemsSource"/>
	/// <seealso cref="ListScheduleDataConnector.TaskPropertyMappings"/>
	/// <seealso cref="TaskPropertyMapping.TaskProperty"/>
    /// <seealso cref="PropertyMappingBase&lt;TaskProperty&gt;.DataObjectProperty"/>
	public class TaskPropertyMapping : PropertyMappingBase<TaskProperty>
	{
		#region TaskProperty

		/// <summary>
		/// Identifies a property of <see cref="Task"/> object.
		/// </summary>
		/// <remarks>
		/// <b>TaskProperty</b> identifies a property of the <see cref="Task"/> object.
		/// </remarks>
		public TaskProperty TaskProperty
		{
			get
			{
				return base.ScheduleProperty;
			}
			set
			{
				base.ScheduleProperty = value;
			}
		}

		#endregion // TaskProperty
	}

	#endregion // TaskPropertyMapping Class



	#region ActivityCategoryPropertyMappingCollection Class

	// SSP 12/8/10 - NAS11.1 Activity Categories
	// 

	/// <summary>
	/// Collection used for specifying field mappings for <see cref="ActivityCategory"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ActivityCategoryPropertyMappingCollection</b> class is used by the <see cref="ListScheduleDataConnector"/>'s
	/// <see cref="ListScheduleDataConnector.ActivityCategoryPropertyMappings"/> property. It's a collection
	/// of <see cref="ActivityCategoryPropertyMapping"/> objects where each <i>ActivityCategoryPropertyMapping</i> object provides a 
	/// mapping for a property of <see cref="ActivityCategory"/> class to the field in the activity categorys data source.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.ActivityCategoryPropertyMappings"/>
	/// <seealso cref="ListScheduleDataConnector.ActivityCategoryItemsSource"/>
	/// <seealso cref="ActivityCategoryPropertyMapping"/>



	public class ActivityCategoryPropertyMappingCollection : PropertyMappingCollection<ActivityCategoryProperty, ActivityCategoryPropertyMapping>
	{
		/// <summary>
		/// Adds mapping for the specified activityCategory property.
		/// </summary>
		/// <param name="activityCategoryProperty">Identifies the property of the <see cref="ActivityCategory"/> class.</param>
		/// <param name="dataObjectProperty">The name of the property in the underlying data source object.</param>
		public void Add( ActivityCategoryProperty activityCategoryProperty, string dataObjectProperty )
		{
			base.Add( this.CreateNew( activityCategoryProperty, dataObjectProperty ) );
		}
	}

	#endregion // ActivityCategoryPropertyMappingCollection Class

	#region AppointmentPropertyMappingCollection Class

	/// <summary>
	/// Collection used for specifying field mappings for <see cref="Appointment"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>AppointmentPropertyMappingCollection</b> class is used by the <see cref="ListScheduleDataConnector"/>'s
	/// <see cref="ListScheduleDataConnector.AppointmentPropertyMappings"/> property. It's a collection
	/// of <see cref="AppointmentPropertyMapping"/> objects where each <i>AppointmentPropertyMapping</i> object provides a 
	/// mapping for a property of <see cref="Appointment"/> class to the field in the appointments data source.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.AppointmentPropertyMappings"/>
	/// <seealso cref="ListScheduleDataConnector.AppointmentItemsSource"/>
	/// <seealso cref="ListScheduleDataConnector.RecurringAppointmentItemsSource"/>
	/// <seealso cref="AppointmentPropertyMapping"/>
	public class AppointmentPropertyMappingCollection : PropertyMappingCollection<AppointmentProperty, AppointmentPropertyMapping>
	{
		/// <summary>
		/// Adds mapping for the specified appointment property.
		/// </summary>
		/// <param name="appointmentProperty">Identifies the property of the <see cref="Appointment"/> class.</param>
		/// <param name="dataObjectProperty">The name of the property in the underlying data source object.</param>
		public void Add( AppointmentProperty appointmentProperty, string dataObjectProperty )
		{
			base.Add( this.CreateNew( appointmentProperty, dataObjectProperty ) );
		}
	}

	#endregion // AppointmentPropertyMappingCollection Class

	#region JournalPropertyMappingCollection Class

	/// <summary>
	/// Collection used for specifying field mappings for <see cref="Journal"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>JournalPropertyMappingCollection</b> class is used by the <see cref="ListScheduleDataConnector"/>'s
	/// <see cref="ListScheduleDataConnector.JournalPropertyMappings"/> property. It's a collection
	/// of <see cref="JournalPropertyMapping"/> objects where each <i>JournalPropertyMapping</i> object provides a 
	/// mapping for a property of <see cref="Journal"/> class to the field in the journals data source.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.JournalPropertyMappings"/>
	/// <seealso cref="ListScheduleDataConnector.JournalItemsSource"/>
	/// <seealso cref="JournalPropertyMapping"/>
	public class JournalPropertyMappingCollection : PropertyMappingCollection<JournalProperty, JournalPropertyMapping>
	{
		/// <summary>
		/// Adds mapping for the specified journal property.
		/// </summary>
		/// <param name="journalProperty">Identifies the property of the <see cref="Journal"/> class.</param>
		/// <param name="dataObjectProperty">The name of the property in the underlying data source object.</param>
		public void Add( JournalProperty journalProperty, string dataObjectProperty )
		{
			base.Add( this.CreateNew( journalProperty, dataObjectProperty ) );
		}
	}

	#endregion // JournalPropertyMappingCollection Class

	#region ProjectPropertyMappingCollection Class

	// SSP 1/6/12 - NAS12.1 XamGantt
	// 

//    /// <summary>
//    /// Collection used for specifying field mappings for <see cref="Project"/> object.
//    /// </summary>
//    /// <remarks>
//    /// <para class="body">
//    /// <b>ProjectPropertyMappingCollection</b> class is used by the <see cref="ListScheduleDataConnector"/>'s
//    /// <see cref="ListScheduleDataConnector.ProjectPropertyMappings"/> property. It's a collection
//    /// of <see cref="ProjectPropertyMapping"/> objects where each <i>ProjectPropertyMapping</i> object provides a 
//    /// mapping for a property of <see cref="Project"/> class to the field in the projects items source.
//    /// </para>
//    /// </remarks>
//    /// <seealso cref="ListScheduleDataConnector.ProjectPropertyMappings"/>
//    /// <seealso cref="ListScheduleDataConnector.ProjectItemsSource"/>
//    /// <seealso cref="ProjectPropertyMapping"/>
//#if WPF && !WCFService
//    [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//    public class ProjectPropertyMappingCollection : PropertyMappingCollection<ProjectProperty, ProjectPropertyMapping>
//    {
//        /// <summary>
//        /// Adds mapping for the specified project property.
//        /// </summary>
//        /// <param name="projectProperty">Identifies the property of the <see cref="Project"/> class.</param>
//        /// <param name="dataObjectProperty">The name of the property in the underlying data source object.</param>
//        public void Add( ProjectProperty projectProperty, string dataObjectProperty )
//        {
//            base.Add( this.CreateNew( projectProperty, dataObjectProperty ) );
//        }
//    }

	#endregion // ProjectPropertyMappingCollection Class

	#region ResourcePropertyMappingCollection Class

	/// <summary>
	/// Collection used for specifying field mappings for <see cref="Resource"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ResourcePropertyMappingCollection</b> class is used by the <see cref="ListScheduleDataConnector"/>'s
	/// <see cref="ListScheduleDataConnector.ResourcePropertyMappings"/> property. It's a collection
	/// of <see cref="ResourcePropertyMapping"/> objects where each <i>ResourcePropertyMapping</i> object provides a 
	/// mapping for a property of <see cref="Resource"/> class to the field in the resources data source.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.ResourcePropertyMappings"/>
	/// <seealso cref="ListScheduleDataConnector.ResourceItemsSource"/>
	/// <seealso cref="ResourcePropertyMapping"/>
	public class ResourcePropertyMappingCollection : PropertyMappingCollection<ResourceProperty, ResourcePropertyMapping>
	{
		/// <summary>
		/// Adds mapping for the specified resource property.
		/// </summary>
		/// <param name="resourceProperty">Identifies the property of the <see cref="Resource"/> class.</param>
		/// <param name="dataObjectProperty">The name of the property in the underlying data source object.</param>
		public void Add( ResourceProperty resourceProperty, string dataObjectProperty )
		{
			base.Add( this.CreateNew( resourceProperty, dataObjectProperty ) );
		}
	}

	#endregion // ResourcePropertyMappingCollection Class

    #region ResourceCalendarPropertyMappingCollection Class

    /// <summary>
    /// Collection used for specifying field mappings for <see cref="ResourceCalendar"/> object.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <b>ResourceCalendarPropertyMappingCollection</b> class is used by the <see cref="ListScheduleDataConnector"/>'s
    /// <see cref="ListScheduleDataConnector.ResourceCalendarPropertyMappings"/> property. It's a collection
    /// of <see cref="ResourceCalendarPropertyMapping"/> objects where each <i>ResourceCalendarPropertyMapping</i> object provides a 
    /// mapping for a property of <see cref="ResourceCalendar"/> class to the field in the resource calendars data source.
    /// </para>
    /// </remarks>
    /// <seealso cref="ListScheduleDataConnector.ResourceCalendarPropertyMappings"/>
    /// <seealso cref="ListScheduleDataConnector.ResourceCalendarItemsSource"/>
    /// <seealso cref="ResourceCalendarPropertyMapping"/>
    public class ResourceCalendarPropertyMappingCollection : PropertyMappingCollection<ResourceCalendarProperty, ResourceCalendarPropertyMapping>
    {
        /// <summary>
        /// Adds mapping for the specified resource calendar property.
        /// </summary>
		/// <param name="resourceCalendarProperty">Identifies the property of the <see cref="ResourceCalendar"/> class.</param>
		/// <param name="dataObjectProperty">The name of the property in the underlying data source object.</param>
		public void Add( ResourceCalendarProperty resourceCalendarProperty, string dataObjectProperty )
        {
			base.Add( this.CreateNew( resourceCalendarProperty, dataObjectProperty ) );
        }
    }

    #endregion // ResourceCalendarPropertyMappingCollection Class

	#region TaskPropertyMappingCollection Class

	/// <summary>
	/// Collection used for specifying field mappings for <see cref="Task"/> object.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>TaskPropertyMappingCollection</b> class is used by the <see cref="ListScheduleDataConnector"/>'s
	/// <see cref="ListScheduleDataConnector.TaskPropertyMappings"/> property. It's a collection
	/// of <see cref="TaskPropertyMapping"/> objects where each <i>TaskPropertyMapping</i> object provides a 
	/// mapping for a property of <see cref="Task"/> class to the field in the tasks data source.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector.TaskPropertyMappings"/>
	/// <seealso cref="ListScheduleDataConnector.TaskItemsSource"/>
	/// <seealso cref="TaskPropertyMapping"/>
	public class TaskPropertyMappingCollection : PropertyMappingCollection<TaskProperty, TaskPropertyMapping>
	{
		/// <summary>
		/// Adds mapping for the specified task property.
		/// </summary>
		/// <param name="taskProperty">Identifies the property of the <see cref="Task"/> class.</param>
		/// <param name="dataObjectProperty">The name of the property in the underlying data source object.</param>
		public void Add( TaskProperty taskProperty, string dataObjectProperty )
		{
			base.Add( this.CreateNew( taskProperty, dataObjectProperty ) );
		}
	}

	#endregion // TaskPropertyMappingCollection Class

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