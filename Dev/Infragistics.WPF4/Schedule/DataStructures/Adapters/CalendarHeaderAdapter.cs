using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class CalendarHeaderAdapter : RecyclingContainer<CalendarHeader>
	{
		#region Member Variables

		private ResourceCalendar _calendar;
		private ScheduleControlBase _control;
		private bool _isSelected;
		private bool _isActive;
		private bool _isCurrentUser; // AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader

		#endregion // Member Variables

		#region Constructor
		internal CalendarHeaderAdapter(ResourceCalendar calendar, ScheduleControlBase control)
		{
			_calendar = calendar;
			_control = control;
		} 
		#endregion // Constructor

		#region Base class overrides
		protected override CalendarHeader CreateInstanceOfRecyclingElement()
		{
			return this._control.CreateCalendarHeader();
		}

		protected override void OnElementAttached(CalendarHeader element)
		{
			base.OnElementAttached(element);

			element.Calendar = _calendar;
			element.DataContext = _calendar;
			element.IsSelected = _isSelected;
			element.IsActive = _isActive;
			element.IsCurrentUser = _isCurrentUser; // AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader

			element.VerifyState();
		}

		protected override void OnElementReleased(CalendarHeader element)
		{
			base.OnElementReleased(element);
			element.Calendar = null;
			element.DataContext = null;
		}
		#endregion // Base class overrides

		#region Properties
		public ResourceCalendar Calendar
		{
			get { return _calendar; }
		}

		public bool IsActive
		{
			get { return _isActive; }
			internal set
			{
				if (value != _isActive)
				{
					_isActive = value;

					// update the element if we're associated with one
					if (this.AttachedElement != null)
						((CalendarHeader)this.AttachedElement).IsActive = value;
				}
			}
		}

		// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		public bool IsCurrentUser
		{
			get { return _isCurrentUser; }
			internal set
			{
				if (value != _isCurrentUser)
				{
					_isCurrentUser = value;

					// update the element if we're associated with one
					if (this.AttachedElement != null)
						((CalendarHeader)this.AttachedElement).IsCurrentUser = value;
				}
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			internal set 
			{
				if (value != _isSelected)
				{
					_isSelected = value;

					// update the element if we're associated with one
					if (this.AttachedElement != null)
						((CalendarHeader)this.AttachedElement).IsSelected = value;
				}
			}
		}
		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region VerifyState
		internal void VerifyState()
		{
			// make sure that the contained tab panel is using the right layout style
			CalendarHeader header = this.AttachedElement as CalendarHeader;

			if (null != header)
				header.VerifyState();
		}
		#endregion // VerifyState

		#endregion //Internal Methods	
	
		#endregion //Methods	
		
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