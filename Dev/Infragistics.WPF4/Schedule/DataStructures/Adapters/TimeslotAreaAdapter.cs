using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom class used to represent a group of <see cref="Timeslot"/> instances for a specific set of <see cref="Resource"/> instances
	/// </summary>
	internal class TimeslotAreaAdapter : RecyclingContainer<TimeslotArea>
	{
		#region Member variables

		private DateTime _start;
		private DateTime _end;
		private DateTime _firstSlotStart;
		private DateTime _lastSlotEnd;
		private TimeslotCollection _timeslots;
		private bool _isToday;
		private AdapterActivitiesProvider _activityProvider;

		#endregion //Member variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotAreaAdapter"/>
		/// </summary>
		/// <param name="control">The associated control</param>
		/// <param name="start">The logical starting date for the item</param>
		/// <param name="end">The logical ending date for the item</param>
		/// <param name="firstSlotStart">The time for the first timeslot</param>
		/// <param name="lastSlotEnd">The time for the last timeslot</param>
		/// <param name="timeslots">A collection of <see cref="Timeslot"/> instances</param>
		/// <param name="activityOwner">The calendar group for which activity will be generated</param>
		internal TimeslotAreaAdapter(ScheduleControlBase control,
			DateTime start, DateTime end, 
			DateTime firstSlotStart, DateTime lastSlotEnd, 
			TimeslotCollection timeslots, CalendarGroupBase activityOwner)
		{
			ScheduleUtilities.ValidateNotNull(timeslots, "Timeslots");

			ScheduleUtilities.Normalize(ref start, ref end);
			ScheduleUtilities.Normalize(ref firstSlotStart, ref lastSlotEnd);

			_timeslots = timeslots;
			
			// logical start/end date
			_start = start;
			_end = end;

			// timeslot start/end
			_firstSlotStart = firstSlotStart;
			_lastSlotEnd = lastSlotEnd;

			if (null != activityOwner)
			{
				_activityProvider = new AdapterActivitiesProvider(control, this, activityOwner);
				_activityProvider.ActivityTypes = control.SupportedActivityTypes;
				_activityProvider.Ranges = ScheduleUtilities.ConvertToUtc(control.TimeZoneInfoProviderResolved.LocalToken, timeslots.GroupTemplates);
				_activityProvider.ActivityFilter = control.TimeslotAreaActivityFilter;
			}
		} 
		#endregion //Constructor

		#region Base class overrides

		#region CreateInstanceOfRecyclingElement
		protected override TimeslotArea CreateInstanceOfRecyclingElement()
		{
			return new TimeslotArea();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region OnElementAttached
		protected override void OnElementAttached(TimeslotArea element)
		{
			element.AreaAdapter = this;
			element.IsTodayInternal = _isToday;

			if (_activityProvider != null)
				_activityProvider.OnAttachedElementChanged();
		}
		#endregion // OnElementAttached

		#region OnElementReleased
		protected override void OnElementReleased(TimeslotArea element)
		{
			element.AreaAdapter = null;

			base.OnElementReleased(element);

			if (_activityProvider != null)
				_activityProvider.OnAttachedElementChanged();
		} 
		#endregion // OnElementReleased

		#region ToString
		/// <summary>
		/// Returns the string representation of the object.
		/// </summary>
		/// <returns>A string containing the <see cref="Start"/> and <see cref="End"/></returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0:d}-{1:d}", this.Start, this.End);
		}
		#endregion //ToString

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region End
		/// <summary>
		/// Returns the non-inclusive end time for the last time slot.
		/// </summary>
		public DateTime End
		{
			get { return _end; }
		}
		#endregion //End

		#region Start
		/// <summary>
		/// Returns the start time for the first time slot.
		/// </summary>
		public DateTime Start
		{
			get { return _start; }
		}
		#endregion //Start

		#region Timeslots
		/// <summary>
		/// Returns a read-only collection of <see cref="Timeslot"/> instances.
		/// </summary>
		public TimeslotCollection Timeslots
		{
			get { return _timeslots; }
		}
		#endregion //Timeslots

		#endregion //Public Properties

		#region Internal Properties

		#region ActivityProvider
		internal AdapterActivitiesProvider ActivityProvider
		{
			get { return _activityProvider; }
		}
		#endregion // ActivityProvider

		#region FirstSlotStart
		internal DateTime FirstSlotStart
		{
			get { return _firstSlotStart; }
		} 
		#endregion // FirstSlotStart

		#region IsToday
		internal bool IsToday
		{
			get { return _isToday; }
			set
			{
				if (value != _isToday)
				{
					_isToday = value;

					TimeslotArea tgp = this.AttachedElement as TimeslotArea;

					if (null != tgp)
						tgp.IsTodayInternal = value;
				}
			}
		}
		#endregion // IsToday

		#region LastSlotEnd
		internal DateTime LastSlotEnd
		{
			get { return _lastSlotEnd; }
		} 
		#endregion // LastSlotEnd

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region OnTodayChanged
		internal virtual void OnTodayChanged(DateTime? today)
		{
			this.IsToday = today == this.Start && today == this.End;
		}
		#endregion // OnTodayChanged

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