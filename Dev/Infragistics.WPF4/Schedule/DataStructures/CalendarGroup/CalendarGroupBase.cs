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

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Base class for an object that contains one or more <see cref="ResourceCalendar"/> instances
	/// </summary>
	public abstract class CalendarGroupBase : PropertyChangeNotifier
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CalendarGroupBase"/>
		/// </summary>
		internal CalendarGroupBase()
		{
		}
		#endregion // Constructor

		#region Properties

		#region Public Properties
		/// <summary>
		/// Returns the collection of <see cref="ResourceCalendar"/> instances whose <see cref="ResourceCalendar.IsVisibleResolved"/> is true
		/// </summary>
		public IList<ResourceCalendar> VisibleCalendars
		{
			get { return this.VisibleCalendarsInternal; }
		}

		/// <summary>
		/// Returns or sets the selected calendar
		/// </summary>
		public ResourceCalendar SelectedCalendar
		{
			get { return this.SelectedCalendarInternal; }
			set { this.SelectedCalendarInternal = value; }
		} 
		#endregion //Public Properties

		// AS 1/20/11 TFS62558
		// The binding infrastructure seems to just do a GetType on the source and then 
		// a GetProperty on that object's type (e.g. source.GetType().GetProperty("propertyPath")).
		// So if the source is an internal class and it has overriden the property designated 
		// by the property path and you are under partial trust (e.g. Silverlight), then you 
		// cannot get to the value of that property because a MethodAccessException will 
		// occur because the DeclaringType of that PropertyInfo is internal. It would seem 
		// like the correct thing for the WPF/SL framework to do would be to get the PropertyInfo 
		// from the base type that originally defined the property since that would work but 
		// since we cannot control that I had to change the public abstract properties to 
		// non-abstract and then add internal abstract properties that we override on the 
		// derived classes.
		//
		#region Internal Properties
		internal abstract ResourceCalendar SelectedCalendarInternal
		{
			get;
			set;
		}

		internal abstract IList<ResourceCalendar> VisibleCalendarsInternal
		{
			get;
		} 
		#endregion //Internal Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Contains

		/// <summary>
		/// Returns true if the calendar is in the group.
		/// </summary>
		/// <param name="calendar">The calendar to check.</param>
		/// <returns></returns>
		public abstract bool Contains(ResourceCalendar calendar);

		#endregion //Contains

		#endregion //Public Methods

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