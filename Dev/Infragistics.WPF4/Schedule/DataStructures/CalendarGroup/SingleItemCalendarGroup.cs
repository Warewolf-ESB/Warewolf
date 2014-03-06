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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Custom <see cref="CalendarGroupBase"/> that represents a single <see cref="ResourceCalendar"/>
	/// </summary>
	internal class SingleItemCalendarGroup : CalendarGroupBase
	{
		#region Member Variables

		private ReadOnlyCollection<ResourceCalendar> _calendars;

		#endregion // Member Variables

		#region Constructor
		internal SingleItemCalendarGroup(ResourceCalendar calendar)
		{
			_calendars = new ReadOnlyCollection<ResourceCalendar>(new ResourceCalendar[] { calendar });
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
			return _calendars.Contains(calendar);
		}

		#endregion //Contains

		#region Equals
		public override bool Equals(object obj)
		{
			SingleItemCalendarGroup other = obj as SingleItemCalendarGroup;

			return other != null && other.SelectedCalendar == this.SelectedCalendar;
		} 
		#endregion // Equals

		#region GetHashCode
		public override int GetHashCode()
		{
			return _calendars[0].GetHashCode();
		} 
		#endregion // GetHashCode

		#region SelectedCalendar
		internal override ResourceCalendar SelectedCalendarInternal
		{
			get
			{
				return _calendars[0];
			}
			set
			{
				if (_calendars[0] != value)
					throw new InvalidOperationException();
			}
		}
		#endregion // SelectedCalendar

		#region VisibleCalendars
		internal override IList<ResourceCalendar> VisibleCalendarsInternal
		{
			get { return _calendars; }
		}
		#endregion // VisibleCalendars

		#endregion // Base class overrides
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