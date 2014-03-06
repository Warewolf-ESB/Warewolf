using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
using System.Collections;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Services.Schedules
{
	/// <summary>
	/// A WCF service which provides list schedule data to client. Only one instance of this service class will be created
	/// and all remote calls will be run on the same thread. This reduces overhead on each remote call, but because of this, 
	/// this service should only be used when it is known that a relatively small number of clients will be connecting to the 
	/// service. If too many clients are connecting to this service and/or timeouts are occurring frequently on client machines, 
	/// use the <see cref="WcfListConnectorServiceMulti"/> instead.
	/// </summary>
	/// <seealso cref="WcfListConnectorServiceMulti"/>
	[ServiceBehavior(
		ConcurrencyMode = ConcurrencyMode.Single,
		InstanceContextMode = InstanceContextMode.Single
	)]
	public class WcfListConnectorServiceSingle : WcfListConnectorService
	{
		#region Constructor

		/// <summary>
		/// Creates a new <see cref="WcfListConnectorServiceSingle"/> instance.
		/// </summary>
		public WcfListConnectorServiceSingle()
		{
			// MD 2/9/11 - TFS65718
			_activityCategoryListManager = new ActivityCategoryListManager(this);
			_activityCategoryListManager.ChangeInformation = new ListManagerChangeInformation(false);

			_appointmentListManager = new AppointmentListManager(this);
			_appointmentListManager.ChangeInformation = new ListManagerChangeInformation(false);
			_journalListManager = new JournalListManager(this);
			_journalListManager.ChangeInformation = new ListManagerChangeInformation(false);
			_recurringAppointmentListManager = new AppointmentListManager(this);
			_recurringAppointmentListManager.ChangeInformation = new ListManagerChangeInformation(false);
			_taskListManager = new TaskListManager(this);
			_taskListManager.ChangeInformation = new ListManagerChangeInformation(false);
			_resourceListManager = new ResourceListManager(this);
			_resourceListManager.ChangeInformation = new ListManagerChangeInformation(false);
			_resourceCalendarListManager = new ResourceCalendarListManager(this);
			_resourceCalendarListManager.ChangeInformation = new ListManagerChangeInformation(false);

			_appointmentListManager.SetRecurringListManager(_recurringAppointmentListManager);
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region GetListManager

		internal override IListManager GetListManager(ItemSourceType listManagerType)
		{
			switch (listManagerType)
			{
				// MD 2/9/11 - TFS65718
				case ItemSourceType.ActivityCategory:
					return _activityCategoryListManager;

				case ItemSourceType.Appointment:
					return _appointmentListManager;

				case ItemSourceType.Journal:
					return _journalListManager;

				case ItemSourceType.RecurringAppointment:
					return _recurringAppointmentListManager;

				case ItemSourceType.Resource:
					return _resourceListManager;

				case ItemSourceType.ResourceCalendar:
					return _resourceCalendarListManager;

				case ItemSourceType.Task:
					return _taskListManager;

				default:
					Debug.Fail("Unknown ItemSourceType: " + listManagerType);
					return _appointmentListManager;
			}
		}

		#endregion  // GetListManager

		#endregion  // Base Class Overrides

		#region Properties

		// MD 2/9/11 - TFS65718
		#region ActivityCategoryItemsSource

		/// <summary>
		/// Specifies the data source where the activity categories are stored.
		/// </summary>
		/// <seealso cref="AppointmentPropertyMappings"/>
		public IEnumerable ActivityCategoryItemsSource
		{
			get { return _activityCategoryListManager.List; }
			set { _activityCategoryListManager.List = value; }
		}

		#endregion  // ActivityCategoryItemsSource

		// MD 2/9/11 - TFS65718
		#region ActivityCategoryPropertyMappings

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="ActivityCategory"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityCategoryPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="ActivityCategoryItemsSource"/> provide data for which properties of the
		/// <see cref="ActivityCategory"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>ActivityCategoryPropertyMappingCollection</i> to true if the field names 
		/// in the data source are the same as the property names as defined by the <see cref="ActivityCategoryProperty"/> 
		/// enum. You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having the 
		/// rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityCategoryPropertyMappingCollection"/>
		/// <seealso cref="ActivityCategoryPropertyMapping"/>
		/// <seealso cref="ActivityCategoryProperty"/>
		public ActivityCategoryPropertyMappingCollection ActivityCategoryPropertyMappings
		{
			get { return (ActivityCategoryPropertyMappingCollection)_activityCategoryListManager.Mappings; }
			set { _activityCategoryListManager.Mappings = value; }
		}

		#endregion  // ActivityCategoryPropertyMappings

		#region AppointmentItemsSource

		/// <summary>
		/// Specifies the data source where the appointments are stored.
		/// </summary>
		/// <seealso cref="AppointmentPropertyMappings"/>
		public IEnumerable AppointmentItemsSource
		{
			get { return _appointmentListManager.List; }
			set { _appointmentListManager.List = value; }
		}

		#endregion  // AppointmentItemsSource

		#region AppointmentPropertyMappings

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Appointment"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AppointmentPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="AppointmentItemsSource"/> provide data for which properties of the
		/// <see cref="Appointment"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>AppointmentPropertyMappingCollection</i> to true if the field names 
		/// in the data source are the same as the property names as defined by the <see cref="AppointmentProperty"/> 
		/// enum. You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having the 
		/// rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="AppointmentPropertyMappingCollection"/>
		/// <seealso cref="AppointmentPropertyMapping"/>
		/// <seealso cref="AppointmentProperty"/>
		public AppointmentPropertyMappingCollection AppointmentPropertyMappings
		{
			get { return (AppointmentPropertyMappingCollection)_appointmentListManager.Mappings; }
			set { _appointmentListManager.Mappings = value; }
		}

		#endregion  // AppointmentPropertyMappings

		#region JournalItemsSource

		/// <summary>
		/// Specifies the data source where the journals are stored.
		/// </summary>
		/// <seealso cref="JournalPropertyMappings"/>
		public IEnumerable JournalItemsSource
		{
			get { return _journalListManager.List; }
			set { _journalListManager.List = value; }
		}

		#endregion  // JournalItemsSource

		#region JournalPropertyMappings

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Journal"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>JournalPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="JournalItemsSource"/> provide data for which properties of the
		/// <see cref="Journal"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>JournalPropertyMappingCollection</i>
		/// to true if the field names in the data source are the same as the 
		/// property names as defined by the <see cref="JournalProperty"/> enum.
		/// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having
		/// the rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="JournalPropertyMappingCollection"/>
		/// <seealso cref="JournalPropertyMapping"/>
		/// <seealso cref="JournalProperty"/>
		public JournalPropertyMappingCollection JournalPropertyMappings
		{
			get { return (JournalPropertyMappingCollection)_journalListManager.Mappings; }
			set { _journalListManager.Mappings = value; }
		}

		#endregion  // JournalPropertyMappings

		#region RecurringAppointmentItemsSource

		/// <summary>
		/// Specifies the data source where the recurring appointments are stored.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RecurringAppointmentItemsSource</b> specifies the data source where the recurring
		/// appointments are stored. This is optional. If not specified then the recurring appointments
		/// will be stored in the data source specified by <see cref="AppointmentItemsSource"/>.
		/// If specified then the recurring appointments, including variances, will be stored in this 
		/// data source.
		/// </para>
		/// <para class="body">
		/// Note that if this data source is specified then all the appointments in the 
		/// <i>AppointmentItemsSource</i> will be assumed to be non-recurring appointments. 
		/// In other words it's not allowed for both the <i>AppointmentItemsSource</i> and 
		/// <i>RecurringAppointmentItemsSource</i> data sources to contain recurring appointments.
		/// </para>
		/// </remarks>
		/// <seealso cref="RecurringAppointmentPropertyMappings"/>
		public IEnumerable RecurringAppointmentItemsSource
		{
			get { return _recurringAppointmentListManager.List; }
			set { _recurringAppointmentListManager.List = value; }
		}

		#endregion  // RecurringAppointmentItemsSource

		#region RecurringAppointmentPropertyMappings


		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Appointment"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RecurringAppointmentPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="RecurringAppointmentItemsSource"/> provide data for which properties of the
		/// <see cref="Appointment"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>AppointmentPropertyMappingCollection</i> to true if the field names 
		/// in the data source are the same as the property names as defined by the <see cref="AppointmentProperty"/> 
		/// enum. You can also set <i>UseDefaultMappings</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having the 
		/// rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="AppointmentPropertyMappingCollection"/>
		/// <seealso cref="AppointmentPropertyMapping"/>
		/// <seealso cref="AppointmentProperty"/>
		/// <seealso cref="RecurringAppointmentItemsSource"/>
		public AppointmentPropertyMappingCollection RecurringAppointmentPropertyMappings
		{
			get { return (AppointmentPropertyMappingCollection)_recurringAppointmentListManager.Mappings; }
			set { _recurringAppointmentListManager.Mappings = value; }
		}

		#endregion  // RecurringAppointmentPropertyMappings

		#region ResourceCalendarItemsSource

		/// <summary>
		/// Specifies the data source where the resource calendars are stored.
		/// </summary>
		/// <seealso cref="ResourceCalendarPropertyMappings"/>
		public IEnumerable ResourceCalendarItemsSource
		{
			get { return _resourceCalendarListManager.List; }
			set { _resourceCalendarListManager.List = value; }
		}

		#endregion  // ResourceCalendarItemsSource

		#region ResourceCalendarPropertyMappings

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="ResourceCalendar"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ResourceCalendarPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="ResourceItemsSource"/> provide data for which properties of the
		/// <see cref="Resource"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>ResourceCalendarPropertyMappingCollection</i>  
		/// to true if the field names in the data source are the same as the 
		/// property names as defined by the <see cref="ResourceProperty"/> enum.
		/// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having
		/// the rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="ResourceCalendarPropertyMappingCollection"/>
		/// <seealso cref="ResourceCalendarPropertyMapping"/>
		/// <seealso cref="ResourceCalendarProperty"/>
		public ResourceCalendarPropertyMappingCollection ResourceCalendarPropertyMappings
		{
			get { return (ResourceCalendarPropertyMappingCollection)_resourceCalendarListManager.Mappings; }
			set { _resourceCalendarListManager.Mappings = value; }
		}

		#endregion  // ResourceCalendarPropertyMappings

		#region ResourceItemsSource

		/// <summary>
		/// Specifies the data source where the resources are stored.
		/// </summary>
		/// <seealso cref="ResourcePropertyMappings"/>
		public IEnumerable ResourceItemsSource
		{
			get { return _resourceListManager.List; }
			set { _resourceListManager.List = value; }
		}

		#endregion  // ResourceItemsSource

		#region ResourcePropertyMappings

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Resource"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ResourcePropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="ResourceItemsSource"/> provide data for which properties of the
		/// <see cref="Resource"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>ResourcePropertyMappingCollection</i>  
		/// to true if the field names in the data source are the same as the 
		/// property names as defined by the <see cref="ResourceProperty"/> enum.
		/// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having
		/// the rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="ResourcePropertyMappingCollection"/>
		/// <seealso cref="ResourcePropertyMapping"/>
		/// <seealso cref="ResourceProperty"/>
		public ResourcePropertyMappingCollection ResourcePropertyMappings
		{
			get { return (ResourcePropertyMappingCollection)_resourceListManager.Mappings; }
			set { _resourceListManager.Mappings = value; }
		}

		#endregion  // ResourcePropertyMappings

		#region TaskItemsSource

		/// <summary>
		/// Specifies the data source where the tasks are stored.
		/// </summary>
		/// <seealso cref="TaskPropertyMappings"/>
		public IEnumerable TaskItemsSource
		{
			get { return _taskListManager.List; }
			set { _taskListManager.List = value; }
		}

		#endregion  // TaskItemsSource

		#region TaskPropertyMappings

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Task"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>TaskPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="TaskItemsSource"/> provide data for which properties of the
		/// <see cref="Task"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>TaskPropertyMappingCollection</i>  
		/// to true if the field names in the data source are the same as the 
		/// property names as defined by the <see cref="TaskProperty"/> enum.
		/// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having
		/// the rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="TaskPropertyMappingCollection"/>
		/// <seealso cref="TaskPropertyMapping"/>
		/// <seealso cref="TaskProperty"/>
		public TaskPropertyMappingCollection TaskPropertyMappings
		{
			get { return (TaskPropertyMappingCollection)_taskListManager.Mappings; }
			set { _taskListManager.Mappings = value; }
		}

		#endregion  // TaskPropertyMappings

		#endregion  // Properties
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