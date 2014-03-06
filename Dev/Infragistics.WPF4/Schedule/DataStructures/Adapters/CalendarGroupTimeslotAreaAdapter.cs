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
using System.Collections.Generic;
using Infragistics.Collections;
using System.Windows.Data;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the area within a CalendarGroup that contains the time information for the group.
	/// </summary>
	internal class CalendarGroupTimeslotAreaAdapter : CalendarGroupAdapterBase<CalendarGroupTimeslotArea>
	{
		#region Member Variables

		private IList<TimeslotAreaAdapter> _timeslotGroupsSource;
		private ReadOnlyNotifyCollection<TimeslotAreaAdapter> _timeslotGroups;
		private bool _showsTodayHighlight;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CalendarGroupTimeslotAreaAdapter"/>
		/// </summary>
		/// <param name="control">The owning control</param>
		/// <param name="calendarGroup">Associated group</param>
		/// <param name="timeSlotGroups">A collection of the <see cref="TimeslotAreaAdapter"/> instances that the group represents</param>
		public CalendarGroupTimeslotAreaAdapter(ScheduleControlBase control, CalendarGroupBase calendarGroup, IList<TimeslotAreaAdapter> timeSlotGroups) : base(control, calendarGroup)
		{
			ScheduleUtilities.ValidateNotNull(timeSlotGroups, "timeSlotGroups");

			_timeslotGroupsSource = timeSlotGroups;
			_timeslotGroups = new ReadOnlyNotifyCollection<TimeslotAreaAdapter>(timeSlotGroups);
			_showsTodayHighlight = control.SupportsTodayHighlight;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region CreateInstanceOfRecyclingElement
		protected override CalendarGroupTimeslotArea CreateInstanceOfRecyclingElement()
		{
			return new CalendarGroupTimeslotArea();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region OnElementAttached
		protected override void OnElementAttached(CalendarGroupTimeslotArea element)
		{
			element.Items = this.TimeslotAreas;

			base.OnElementAttached(element);
		}
		#endregion // OnElementAttached

		#region OnElementReleased
		protected override void OnElementReleased(CalendarGroupTimeslotArea element)
		{
			base.OnElementReleased(element);

			element.Items = null;
		} 
		#endregion // OnElementReleased

		#region OnTodayChanged
		internal override void OnTodayChanged(DateTime? today)
		{
			if (!_showsTodayHighlight)
				return;

			foreach (TimeslotAreaAdapter group in _timeslotGroups)
			{
				group.OnTodayChanged(today);
			}

			base.OnTodayChanged(today);
		} 
		#endregion // OnTodayChanged

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region TimeslotAreas
		/// <summary>
		/// Returns a collection of <see cref="TimeslotAreaAdapter"/> instances
		/// </summary>
		public ReadOnlyNotifyCollection<TimeslotAreaAdapter> TimeslotAreas
		{
			get { return _timeslotGroups; }
		}
		#endregion //TimeslotAreas

		#endregion //Public Properties

		#region Internal Properties
		
		#region TimeslotGroupsSource
		internal IList<TimeslotAreaAdapter> TimeslotGroupsSource
		{
			get { return _timeslotGroupsSource; }
		}
		#endregion //TimeslotGroupsSource

		#endregion //Internal Properties

		#endregion //Properties
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