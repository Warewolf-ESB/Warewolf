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
using Infragistics.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents the header for a specific <see cref="CalendarGroup"/> in a specific <see cref="ScheduleControlBase"/>
	/// </summary>
	internal class CalendarHeaderAreaAdapter : CalendarGroupAdapterBase<CalendarHeaderArea>
	{
		#region Member Variables

		private ScheduleControlBase _control;
		private ObservableCollectionExtended<CalendarHeaderAdapter> _items;
		private ResourceCalendar _activeCalendar;

		#endregion // Member Variables

		#region Constructor
		internal CalendarHeaderAreaAdapter(CalendarGroupWrapper wrapper) : base (wrapper.Control, wrapper.CalendarGroup)
		{
			_items = new ObservableCollectionExtended<CalendarHeaderAdapter>();

			_control = wrapper.Control;
		} 
		#endregion // Constructor

		#region Base class overrides
		protected override CalendarHeaderArea CreateInstanceOfRecyclingElement()
		{
			return new CalendarHeaderArea();
		}

		protected override void OnElementAttached(CalendarHeaderArea element)
		{
			base.OnElementAttached(element);

			ScheduleControlBase ctrl = ScheduleUtilities.GetControl(element);
			Debug.Assert(null != ctrl, "need the control to find out the orientation of the items");
			element.Orientation = ctrl != null ? ctrl.CalendarGroupOrientation : Orientation.Vertical;

			element.Items = _items;
			element.VerifyState();
		}

		protected override void OnElementReleased(CalendarHeaderArea element)
		{
			base.OnElementReleased(element);

			element.Items = null;
		}

		#region OnCalendarHeaderAreaVisibilityChanged
		internal override void OnCalendarHeaderAreaVisibilityChanged(bool hasCalendarArea)
		{
			base.OnCalendarHeaderAreaVisibilityChanged(hasCalendarArea);

			foreach (CalendarHeaderAdapter item in _items)
				item.VerifyState();
		}
		#endregion // OnCalendarHeaderAreaVisibilityChanged

		#region OnSelectedCalendarChanged
		internal override void OnSelectedCalendarChanged(ResourceCalendar selectedCalendar)
		{
			FrameworkElement fe = this.AttachedElement;
			ResourceCalendar oldSelected = fe != null ? fe.DataContext as ResourceCalendar : null;

			base.OnSelectedCalendarChanged(selectedCalendar);

			CalendarHeaderAdapter oldItem = null;
			CalendarHeaderAdapter newItem = null;

			for (int i = 0, count = _items.Count; i < count; i++)
			{
				CalendarHeaderAdapter item = _items[i];

				if (item.Calendar == oldSelected)
					oldItem = item;
				else if (item.Calendar == selectedCalendar)
					newItem = item;
			}

			if (null != oldItem)
				oldItem.IsSelected = false;

			if (null != newItem)
				newItem.IsSelected = true;

			if (null != fe)
			{
				var peer = FrameworkElementAutomationPeer.FromElement(fe) as CalendarHeaderAreaAutomationPeer;

				if (null != peer)
					peer.RaiseSelectionEvents(oldSelected, selectedCalendar);
			}
		}
		#endregion // OnSelectedCalendarChanged

		#endregion // Base class overrides

		#region Properties

		#region ActiveCalendar
		internal ResourceCalendar ActiveCalendar
		{
			get { return _activeCalendar; }
			set
			{
				if (value != _activeCalendar)
				{
					_activeCalendar = value;

					// make sure the selected calendar is updated. if it is then 
					// it will pass along the notification to the headers. otherwise 
					// we need to notify the state.
					this.OnActiveCalendarChanged(_activeCalendar);
				}
			}
		}
		#endregion // IsActiveGroup

		#endregion //Properties

		#region Methods

		#region GetAdapter
		internal static CalendarHeaderAdapter GetAdapter(IList<CalendarHeaderAdapter> adapters, ResourceCalendar calendar)
		{
			if (adapters != null)
			{
				for (int i = 0; i < adapters.Count; i++)
				{
					if (adapters[i].Calendar == calendar)
					{
						return adapters[i];
					}
				}
			}

			return null;
		} 
		#endregion // GetAdapter

		#region OnActiveCalendarChanged
		private void OnActiveCalendarChanged(ResourceCalendar activeCalendar)
		{
			foreach (CalendarHeaderAdapter adapter in _items)
			{
				adapter.IsActive = adapter.Calendar == activeCalendar;
			}
		}
		#endregion // OnActiveCalendarChanged

		// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		#region OnCurrentUserChanged
		internal void OnCurrentUserChanged(Resource currentUser)
		{
			foreach (CalendarHeaderAdapter adapter in _items)
			{
				adapter.IsCurrentUser = currentUser != null &&
					adapter.Calendar != null &&
					adapter.Calendar.OwningResource == currentUser;
			}
		}
		#endregion //OnCurrentUserChanged

		#region VerifyState
		internal void VerifyState()
		{
			List<CalendarHeaderAdapter> newItems = new List<CalendarHeaderAdapter>();
			Dictionary<ResourceCalendar, CalendarHeaderAdapter> oldItems = new Dictionary<ResourceCalendar, CalendarHeaderAdapter>();
			ResourceCalendar selectedCalendar = this.CalendarGroup.SelectedCalendar;
			Resource currentUser = _control.DataManagerResolved == null ? null : _control.DataManagerResolved.CurrentUser; // AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader

			foreach (CalendarHeaderAdapter item in _items)
				oldItems[item.Calendar] = item;

			foreach (ResourceCalendar calendar in this.CalendarGroup.VisibleCalendars)
			{
				CalendarHeaderAdapter adapter;

				if (!oldItems.TryGetValue(calendar, out adapter))
				{
					adapter = new CalendarHeaderAdapter(calendar, this._control);
				}
				else
				{
					// remove it just in case there are duplicate instances of the calendar
					oldItems.Remove(calendar);
				}

				// initialize the selected state
				adapter.IsSelected = calendar == selectedCalendar;
				adapter.IsActive = calendar == _activeCalendar;
				adapter.IsCurrentUser = calendar != null && currentUser != null && calendar.OwningResource == currentUser; // AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader

				newItems.Add(adapter);

				adapter.VerifyState();
			}

            // AS 1/20/11 TFS62537
            //if (!ScheduleUtilities.AreEqual(_items, newItems, null))
			//	_items.ReInitialize(newItems);
            ScheduleUtilities.Reinitialize(_items, newItems);

			// make sure that the contained tab panel is using the right layout style
			CalendarHeaderArea header = this.AttachedElement as CalendarHeaderArea;

			if (null != header)
				header.VerifyState();
		}
		#endregion // VerifyState

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