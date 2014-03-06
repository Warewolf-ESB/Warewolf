using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Collections;
using System.Collections.Generic;

namespace Infragistics.Controls.Schedules
{
	internal class MergedCalendarGroup : CalendarGroupBase
		, ISupportPropertyChangeNotifications
		, IPropertyChangeListener
	{
		#region Member Variables

		private CalendarGroupCollection _groups;
		private PropertyChangeListenerList _propChangeListeners;

		private ObservableCollectionExtended<ResourceCalendar> _calendars;
		private ReadOnlyNotifyCollection<ResourceCalendar> _readOnlyCalendars;
		private ResourceCalendar _selectedCalendar;

		private bool _isDirty = true;

		#endregion // Member Variables

		#region Constructor
		internal MergedCalendarGroup(CalendarGroupCollection groups)
		{
			_groups = groups;
			_calendars = new CalendarGroupCalendarCollection(this, true);
			_readOnlyCalendars = new ReadOnlyNotifyCollection<ResourceCalendar>(_calendars);

			// we need to get a notification when a calendar is made visible or hidden so we can update 
			// the visible calendars
			_propChangeListeners = new PropertyChangeListenerList();
			_calendars.PropChangeListeners.Add(_propChangeListeners, false);

			ScheduleUtilities.AddListener(groups, this, false);
		}
		#endregion // Constructor

		#region Base class overrides

		#region Contains

		/// <summary>
		/// Returns true if the calendar is in the group.
		/// </summary>
		/// <param name="calendar">The calendar to check.</param>
		/// <returns></returns>
		public override bool Contains(ResourceCalendar calendar)
		{
			foreach (CalendarGroup group in this._groups)
			{
				if (group.Contains(calendar))
					return true;
			}

			return false;
		}

		#endregion //Contains

		#region SelectedCalendar
		internal override ResourceCalendar SelectedCalendarInternal
		{
			get
			{
				_groups.ProcessPendingChanges();
				return _selectedCalendar;
			}
			set
			{
				if (value != _selectedCalendar)
				{
					_groups.ProcessPendingChanges();
					_selectedCalendar = value;

					this.RaisePropertyChangedEvent("SelectedCalendar");
				}
			}
		}
		#endregion // SelectedCalendar

		#region OnPropertyChanged
		protected override void OnPropertyChanged(string propertyName)
		{
			base.OnPropertyChanged(propertyName);

			ListenerList.RaisePropertyChanged<object, string>(_propChangeListeners, this, propertyName, null);
		}
		#endregion // OnPropertyChanged

		#region VisibleCalendars
		internal override IList<ResourceCalendar> VisibleCalendarsInternal
		{
			get
			{
				return _calendars;
			}
		}
		#endregion // VisibleCalendars

		#endregion // Base class overrides

		#region Methods

		#region VerifySelection
		private void VerifySelection()
		{
			ResourceCalendar newSelected = _selectedCalendar;

			// if there are no visible calendars then nothing is selected
			if (_calendars.Count == 0)
				newSelected = null;
			else if (newSelected == null)
				newSelected = _calendars[0];
			else if (newSelected.IsVisibleResolved && _calendars.Contains(newSelected))
			{
				// just use the one we have if its visible
			}
			else
			{
				var allCalendarsArray = _groups.Select((CalendarGroup group) => { return (IEnumerable<ResourceCalendar>)group.Calendars; }).ToArray();
				AggregateCollection<ResourceCalendar> allCalendars = new AggregateCollection<ResourceCalendar>(allCalendarsArray);

				// if the currently selected calendar is not visible then select the adjacent one
				newSelected = ScheduleUtilities.FindNearestAfterOrDefault(allCalendars, allCalendars.IndexOf(newSelected), (ResourceCalendar c) => { return c.IsVisibleResolved; });
			}

			this.SelectedCalendar = newSelected;
		}
		#endregion // VerifySelection

		#region VerifyState
		internal void VerifyState()
		{
			if (!_isDirty)
				return;

			_isDirty = false;
			_calendars.BeginUpdate();
			_calendars.Clear();

			foreach (CalendarGroup group in _groups)
			{
				foreach (ResourceCalendar calendar in group.VisibleCalendars)
				{
					_calendars.Add(calendar);
				}
			}

			this.VerifySelection();

			_calendars.EndUpdate();
		}
		#endregion // VerifyState

		#endregion // Methods

		#region ITypedSupportPropertyChangeNotifications<object,string> Members

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener(ITypedPropertyChangeListener<object, string> listener, bool useWeakReference)
		{
			if (null == _propChangeListeners)
				_propChangeListeners = new PropertyChangeListenerList();

			_propChangeListeners.Add(listener, useWeakReference);
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener(ITypedPropertyChangeListener<object, string> listener)
		{
			if (null != _propChangeListeners)
				_propChangeListeners.Remove(listener);
		}

		#endregion //ITypedSupportPropertyChangeNotifications<object,string> Members

		#region ITypedPropertyChangeListener<object,string> Members

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object sender, string property, object extraInfo)
		{
			var calendars = sender as CalendarGroupCalendarCollection;

			if (null != calendars && calendars.IsVisibleCalendars)
			{
				_isDirty = true;
			}
			else if (sender == _groups)
			{
				_isDirty = true;
			}
		}

		#endregion //ITypedPropertyChangeListener<object,string> Members
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