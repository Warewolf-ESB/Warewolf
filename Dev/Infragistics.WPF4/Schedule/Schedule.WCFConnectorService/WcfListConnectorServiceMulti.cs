using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Collections;
using System.Diagnostics;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Services.Schedules
{
	/// <summary>
	/// A WCF service which provides list schedule data to client. A new instance of this service will be created for 
	/// each remote call into the service and calls may be made on different threads. Because of this, the data sources
	/// provided to this service must be thread safe. Also, there is some overhead in setting up the service instance and 
	/// hooking up the data sources on the calls where they are needed. So if only a relatively small number of clients 
	/// will be connecting to the service, it is recommended that the <see cref="WcfListConnectorServiceSingle"/>
	/// is used instead, since the only one instance is ever created and the data sources only need to be hooked up once.
	/// </summary>
	/// <seealso cref="WcfListConnectorServiceSingle"/>
	[ServiceBehavior(
		ConcurrencyMode = ConcurrencyMode.Multiple,
		InstanceContextMode = InstanceContextMode.PerCall
	)]
	public abstract class WcfListConnectorServiceMulti : WcfListConnectorService
	{
		#region Static Variables

		private static object CreateChangeInformationLock = new object();

		// MD 2/9/11 - TFS65718
		private static ListManagerChangeInformation _activityCategoryChangeInformation;

		private static ListManagerChangeInformation _appointmentChangeInformation;
		private static ListManagerChangeInformation _journalChangeInformation;
		private static ListManagerChangeInformation _recurrenceAppointmentChangeInformation;
		private static ListManagerChangeInformation _resourceChangeInformation;
		private static ListManagerChangeInformation _resourceCalendarChangeInformation;
		private static ListManagerChangeInformation _taskChangeInformation;

		#endregion  // Static Variables

		#region Member Variables

		// MD 2/9/11 - TFS65718
		private IEnumerable _activityCategoryList;

		private IEnumerable _appointmentList;
		private IEnumerable _journalList;
		private IEnumerable _recurrenceAppointmentList;
		private IEnumerable _resourceList;
		private IEnumerable _resourceCalendarList;
		private IEnumerable _taskList;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="WcfListConnectorServiceMulti"/> instance.
		/// </summary>
		public WcfListConnectorServiceMulti()
		{
			
		}

		#endregion  // Constructor

		#region Base Class Overrides

		#region GetChangeInformation

		internal override ListManagerChangeInformation GetChangeInformation(ItemSourceType listManagerType)
		{
			switch (listManagerType)
			{
				// MD 2/9/11 - TFS65718
				case ItemSourceType.ActivityCategory:
					WcfListConnectorServiceMulti.VerifyChangeInformation(ref _activityCategoryChangeInformation);
					return _activityCategoryChangeInformation;

				case ItemSourceType.Appointment:
					WcfListConnectorServiceMulti.VerifyChangeInformation(ref _appointmentChangeInformation);
					return _appointmentChangeInformation;

				case ItemSourceType.Journal:
					WcfListConnectorServiceMulti.VerifyChangeInformation(ref _journalChangeInformation);
					return _journalChangeInformation;

				case ItemSourceType.RecurringAppointment:
					WcfListConnectorServiceMulti.VerifyChangeInformation(ref _recurrenceAppointmentChangeInformation);
					return _recurrenceAppointmentChangeInformation;

				case ItemSourceType.Resource:
					WcfListConnectorServiceMulti.VerifyChangeInformation(ref _resourceChangeInformation);
					return _resourceChangeInformation;

				case ItemSourceType.ResourceCalendar:
					WcfListConnectorServiceMulti.VerifyChangeInformation(ref _resourceCalendarChangeInformation);
					return _resourceCalendarChangeInformation;

				case ItemSourceType.Task:
					WcfListConnectorServiceMulti.VerifyChangeInformation(ref _taskChangeInformation);
					return _taskChangeInformation;

				default:
					Debug.Fail("Unknown ItemSourceType: " + listManagerType);
					goto case ItemSourceType.Appointment;
			}
		}

		#endregion  // GetChangeInformation

		#region GetList

		internal override IEnumerable GetList(ItemSourceType listManagerType)
		{
			switch (listManagerType)
			{
				// MD 2/9/11 - TFS65718
				case ItemSourceType.ActivityCategory:
					this.VerifyList(listManagerType, ref _activityCategoryList);
					return _activityCategoryList;

				case ItemSourceType.Appointment:
					this.VerifyList(listManagerType, ref _appointmentList);
					return _appointmentList;

				case ItemSourceType.Journal:
					this.VerifyList(listManagerType, ref _journalList);
					return _journalList;

				case ItemSourceType.RecurringAppointment:
					this.VerifyList(listManagerType, ref _recurrenceAppointmentList);
					return _recurrenceAppointmentList;

				case ItemSourceType.Resource:
					this.VerifyList(listManagerType, ref _resourceList);
					return _resourceList;

				case ItemSourceType.ResourceCalendar:
					this.VerifyList(listManagerType, ref _resourceCalendarList);
					return _resourceCalendarList;

				case ItemSourceType.Task:
					this.VerifyList(listManagerType, ref _taskList);
					return _taskList;

				default:
					Debug.Fail("Unknown ItemSourceType: " + listManagerType);
					goto case ItemSourceType.Appointment;
			}
		}

		#endregion  // GetList

		#region GetListManager

		internal override IListManager GetListManager(ItemSourceType listManagerType)
		{
			switch (listManagerType)
			{
				// MD 2/9/11 - TFS65718
				case ItemSourceType.ActivityCategory:
					if (_activityCategoryListManager == null)
					{
						_activityCategoryListManager = new ActivityCategoryListManager(this);
						_activityCategoryListManager.List = this.GetList(listManagerType);
						_activityCategoryListManager.Mappings = new ActivityCategoryPropertyMappingCollection();
						this.InitializePropertyMappings(listManagerType, _activityCategoryListManager.Mappings);

						WcfListConnectorServiceMulti.AssignChangeInformation<ActivityCategory, ActivityCategoryProperty, ActivityCategoryPropertyMapping>(
							_activityCategoryListManager, this.GetChangeInformation(listManagerType));
					}

					return _activityCategoryListManager;

				case ItemSourceType.Appointment:
					if (_appointmentListManager == null)
					{
						_appointmentListManager = new AppointmentListManager(this);
						_appointmentListManager.List = this.GetList(listManagerType);
						_appointmentListManager.Mappings = new AppointmentPropertyMappingCollection();
						this.InitializePropertyMappings(listManagerType, _appointmentListManager.Mappings);

						WcfListConnectorServiceMulti.AssignChangeInformation<ActivityBase, AppointmentProperty, AppointmentPropertyMapping>(
							_appointmentListManager, this.GetChangeInformation(listManagerType));

						_appointmentListManager.SetRecurringListManager((AppointmentListManager)this.GetListManager(ItemSourceType.RecurringAppointment));
					}

					return _appointmentListManager;

				case ItemSourceType.Journal:
					if (_journalListManager == null)
					{
						_journalListManager = new JournalListManager(this);
						_journalListManager.List = this.GetList(listManagerType);
						_journalListManager.Mappings = new JournalPropertyMappingCollection();
						this.InitializePropertyMappings(listManagerType, _journalListManager.Mappings);

						WcfListConnectorServiceMulti.AssignChangeInformation<ActivityBase, JournalProperty, JournalPropertyMapping>(
							_journalListManager, this.GetChangeInformation(listManagerType));
					}

					return _journalListManager;

				case ItemSourceType.RecurringAppointment:
					if (_recurringAppointmentListManager == null)
					{
						_recurringAppointmentListManager = new AppointmentListManager(this);
						_recurringAppointmentListManager.List = this.GetList(listManagerType);
						_recurringAppointmentListManager.Mappings = new AppointmentPropertyMappingCollection();
						this.InitializePropertyMappings(listManagerType, _recurringAppointmentListManager.Mappings);

						WcfListConnectorServiceMulti.AssignChangeInformation<ActivityBase, AppointmentProperty, AppointmentPropertyMapping>(
							_recurringAppointmentListManager, this.GetChangeInformation(listManagerType));
					}

					return _recurringAppointmentListManager;

				case ItemSourceType.Resource:
					if (_resourceListManager == null)
					{
						_resourceListManager = new ResourceListManager(this);
						_resourceListManager.List = this.GetList(listManagerType);
						_resourceListManager.Mappings = new ResourcePropertyMappingCollection();
						this.InitializePropertyMappings(listManagerType, _resourceListManager.Mappings);

						WcfListConnectorServiceMulti.AssignChangeInformation<Resource, ResourceProperty, ResourcePropertyMapping>(
							_resourceListManager, this.GetChangeInformation(listManagerType));
					}

					return _resourceListManager;

				case ItemSourceType.ResourceCalendar:
					if (_resourceCalendarListManager == null)
					{
						_resourceCalendarListManager = new ResourceCalendarListManager(this);
						_resourceCalendarListManager.List = this.GetList(listManagerType);
						_resourceCalendarListManager.Mappings = new ResourceCalendarPropertyMappingCollection();
						this.InitializePropertyMappings(listManagerType, _resourceCalendarListManager.Mappings);

						WcfListConnectorServiceMulti.AssignChangeInformation<ResourceCalendar, ResourceCalendarProperty, ResourceCalendarPropertyMapping>(
							_resourceCalendarListManager, this.GetChangeInformation(listManagerType));
					}

					return _resourceCalendarListManager;

				case ItemSourceType.Task:
					if (_taskListManager == null)
					{
						_taskListManager = new TaskListManager(this);
						_taskListManager.List = this.GetList(listManagerType);
						_taskListManager.Mappings = new TaskPropertyMappingCollection();
						this.InitializePropertyMappings(listManagerType, _taskListManager.Mappings);

						WcfListConnectorServiceMulti.AssignChangeInformation<ActivityBase, TaskProperty, TaskPropertyMapping>(
							_taskListManager, this.GetChangeInformation(listManagerType));
					}

					return _taskListManager;

				default:
					Debug.Fail("Unknown ItemSourceType: " + listManagerType);
					return _appointmentListManager;
			}
		}

		#endregion  // GetListManager

		#endregion  // Base Class Overrides

		#region Methods

		#region Abstract Methods

		/// <summary>
		/// Gets the item source for a specific collection.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> For each remote call, this method will be called once at most per <see cref="ItemSourceType"/>.
		/// </p>
		/// </remarks>
		/// <param name="itemSourceType">A value indicating which item source is required.</param>
		/// <returns>An IEnumerable instance or null of there is no item source for the requested value.</returns>
		protected abstract IEnumerable GetItemSource(ItemSourceType itemSourceType);

		/// <summary>
		/// Initializes the property mappings collection for the specified type of item source.
		/// </summary>
		/// <param name="itemSourceType">Indicates the item source to which the property mappings apply.</param>
		/// <param name="mappings">The property mappings collection. This must be downcast to the concrete type to be initialized.</param>
		/// <seealso cref="AppointmentPropertyMappingCollection"/>
		/// <seealso cref="JournalPropertyMappingCollection"/>
		/// <seealso cref="ResourceCalendarPropertyMappingCollection"/>
		/// <seealso cref="ResourcePropertyMappingCollection"/>
		/// <seealso cref="TaskPropertyMappingCollection"/>
		protected abstract void InitializePropertyMappings(ItemSourceType itemSourceType, object mappings);

		#endregion  // Abstract Methods

		#region Private Methods

		#region AreDifferent

		private static bool AreDifferent<TMappingKey, TMapping>(
			PropertyMappingCollection<TMappingKey, TMapping> previousMappings, 
			PropertyMappingCollection<TMappingKey, TMapping> currentMappings) 
			where TMapping : PropertyMappingBase<TMappingKey>, new()
		{
			if (previousMappings == null)
				return true;

			if (previousMappings.UseDefaultMappings != currentMappings.UseDefaultMappings)
				return true;

			if (previousMappings.Count != currentMappings.Count)
				return true;

			foreach (TMapping previousMapping in previousMappings)
			{
				TMapping currentMapping = currentMappings.GetItem(previousMapping.ScheduleProperty);

				if (previousMapping.DataObjectProperty != currentMapping.DataObjectProperty)
					return true;
			}

			return false;
		} 

		#endregion  // AreDifferent

		#region AssignChangeInformation

		private static void AssignChangeInformation<TViewItem, TMappingKey, TMapping>(
			ListManager<TViewItem, TMappingKey> listManager,
			ListManagerChangeInformation changeInformation)
			where TViewItem : class
			where TMapping : PropertyMappingBase<TMappingKey>, new()
		{
			lock (changeInformation)
			{
				PropertyMappingCollection<TMappingKey, TMapping> previousMappings =
					(PropertyMappingCollection<TMappingKey, TMapping>)changeInformation.PreviousPropertyMappings;
				PropertyMappingCollection<TMappingKey, TMapping> currentMappings =
					(PropertyMappingCollection<TMappingKey, TMapping>)listManager.Mappings;

				if (WcfListConnectorServiceMulti.AreDifferent<TMappingKey, TMapping>(previousMappings, currentMappings))
					changeInformation.BumpPropertyMappingsVersion();

				changeInformation.PreviousPropertyMappings = currentMappings;
				listManager.ChangeInformation = changeInformation;
			}
		} 

		#endregion  // AssignChangeInformation

		#region VerifyChangeInformation

		private static void VerifyChangeInformation(ref ListManagerChangeInformation changeInformation)
		{
			if (changeInformation != null)
				return;

			lock (CreateChangeInformationLock)
			{
				if (changeInformation != null)
					return;

				changeInformation = new ListManagerChangeInformation(true);
			}
		}

		#endregion  // VerifyChangeInformation

		#region VerifyList

		private void VerifyList(ItemSourceType listManagerType, ref IEnumerable list)
		{
			if (list == null)
				list = this.GetItemSource(listManagerType);
		}

		#endregion  // VerifyList

		#endregion  // Private Methods

		#endregion  // Methods
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