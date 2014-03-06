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
	internal abstract class CalendarGroupAdapterBase<T> : RecyclingContainer<T>
		where T : CalendarGroupPresenterBase
	{
		#region Member Variables

		private CalendarGroupBase _calendarGroup;
		private bool _hasCalendarAreaInControl;

		#endregion // Member Variables

		#region Constructor
		internal CalendarGroupAdapterBase(ScheduleControlBase control, CalendarGroupBase calendarGroup)
		{
			_hasCalendarAreaInControl = control.CalendarHeaderAreaVisibilityResolved != Visibility.Collapsed;
			_calendarGroup = calendarGroup;
		} 
		#endregion // Constructor

		#region Base class overrides

		#region OnElementAttached
		protected override void OnElementAttached(T element)
		{
			element.CalendarGroup = this.CalendarGroup;
			element.DataContext = _calendarGroup.SelectedCalendar;
			element.HasCalendarAreaInControl = _hasCalendarAreaInControl;

			base.OnElementAttached(element);
		}
		#endregion // OnElementAttached

		#region OnElementReleased
		protected override void OnElementReleased(T element)
		{
			element.CalendarGroup = null;
			element.DataContext = null;

			base.OnElementReleased(element);
		}
		#endregion // OnElementReleased

		#endregion // Base class overrides

		#region Properties
		internal CalendarGroupBase CalendarGroup
		{
			get { return _calendarGroup; }
		} 
		#endregion // Properties

		#region Methods

		#region OnCalendarHeaderAreaVisibilityChanged
		internal virtual void OnCalendarHeaderAreaVisibilityChanged(bool hasCalendarArea)
		{
			_hasCalendarAreaInControl = hasCalendarArea;

			var element = this.AttachedElement as CalendarGroupPresenterBase;

			if (null != element)
				element.HasCalendarAreaInControl = hasCalendarArea;
		}
		#endregion // OnCalendarHeaderAreaVisibilityChanged

		#region OnTodayChanged
		internal virtual void OnTodayChanged(DateTime? today)
		{
		}
		#endregion // OnTodayChanged

		#region OnSelectedCalendarChanged
		internal virtual void OnSelectedCalendarChanged(ResourceCalendar selectedCalendar)
		{
			FrameworkElement fe = this.AttachedElement;

			if (null != fe)
				fe.DataContext = selectedCalendar;
		}
		#endregion // OnSelectedCalendarChanged

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