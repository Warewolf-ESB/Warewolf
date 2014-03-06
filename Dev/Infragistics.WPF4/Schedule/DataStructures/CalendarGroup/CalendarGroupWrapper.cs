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

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Class used to maintain information for a CalendarGroup within a ScheduleControlBase
	/// </summary>
	internal class CalendarGroupWrapper
		: IPropertyChangeListener
	{
		#region Member Variables

		private ScheduleControlBase _control;
		private CalendarGroupBase _calendarGroup;
		private ResourceCalendar _selectedCalendar;

		private CalendarGroupTimeslotAreaAdapter _timeslotArea;
		private CalendarHeaderAreaAdapter _groupHeader;

		#endregion // Member Variables

		#region Constructor
		internal CalendarGroupWrapper(ScheduleControlBase control, CalendarGroupBase calendarGroup)
		{
			_control = control;
			_calendarGroup = calendarGroup;

			_timeslotArea = control.CreateTimeslotCalendarGroup(calendarGroup);
			_groupHeader = new CalendarHeaderAreaAdapter(this);
			_selectedCalendar = calendarGroup.SelectedCalendar;

			var ispcn = calendarGroup as ISupportPropertyChangeNotifications;
			
			if (null != ispcn)
				ispcn.AddListener(this, true);
		} 
		#endregion // Constructor

		#region Properties

		#region Internal Properties

		#region CalendarGroup
		internal CalendarGroupBase CalendarGroup
		{
			get { return _calendarGroup; }
		}
		#endregion // CalendarGroup

		#region Control
		internal ScheduleControlBase Control
		{
			get { return _control; }
		} 
		#endregion // Control

		#region GroupHeader
		internal CalendarHeaderAreaAdapter GroupHeader
		{
			get { return _groupHeader; }
		} 
		#endregion // GroupHeader

		#region GroupTimeslotArea
		internal CalendarGroupTimeslotAreaAdapter GroupTimeslotArea
		{
			get { return _timeslotArea; }
		}
		#endregion // GroupTimeslotArea

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region OnCalendarHeaderAreaVisibilityChanged
		internal virtual void OnCalendarHeaderAreaVisibilityChanged(bool hasCalendarArea)
		{
			_timeslotArea.OnCalendarHeaderAreaVisibilityChanged(hasCalendarArea);
			_groupHeader.OnCalendarHeaderAreaVisibilityChanged(hasCalendarArea);
		}
		#endregion // OnCalendarHeaderAreaVisibilityChanged

		#region OnSelectedCalendarChanged
		internal virtual void OnSelectedCalendarChanged(ResourceCalendar selectedCalendar)
		{
			_timeslotArea.OnSelectedCalendarChanged(selectedCalendar);
			_groupHeader.OnSelectedCalendarChanged(selectedCalendar);
		} 
		#endregion // OnSelectedCalendarChanged 

		#region OnTodayChanged
		internal virtual void OnTodayChanged(DateTime? today)
		{
			_timeslotArea.OnTodayChanged(today);
			_groupHeader.OnTodayChanged(today);
		}
		#endregion // OnTodayChanged

		#region VerifySelectedCalendar
		private bool VerifySelectedCalendar()
		{
			ResourceCalendar selectedCalendar = _calendarGroup.SelectedCalendar;

			if (selectedCalendar != _selectedCalendar)
			{
				_selectedCalendar = selectedCalendar;
				this.OnSelectedCalendarChanged(selectedCalendar);
				return true;
			}

			return false;
		}
		
		#endregion // VerifySelectedCalendar

		#region VerifyState
		internal virtual void VerifyState()
		{
			this.VerifySelectedCalendar();

			_groupHeader.VerifyState();
		} 
		#endregion // VerifyState

		#endregion // Methods

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object sender, string propertyName, object extraInfo)
		{
			if (propertyName == "SelectedCalendar")
			{
				// if the groups are dirty then don't worry about processing this as it will be fixed up at that point
				if (_control.IsVerifyGroupsPending)
					return;

				this.VerifySelectedCalendar();
			}
		}

		#endregion
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