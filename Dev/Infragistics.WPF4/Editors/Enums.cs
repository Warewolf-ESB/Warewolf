using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;

namespace Infragistics.Windows.Editors
{

	#region InvalidValueBehavior

	/// <summary>
	/// Specifies the <see cref="ValueEditor.InvalidValueBehavior"/> property.
	/// </summary>
	/// <see cref="ValueEditor.InvalidValueBehavior"/>
	public enum InvalidValueBehavior
	{
		/// <summary>
		/// Default is resolved to <b>DisplayErrorMessage</b>.
		/// </summary>
		Default,

		/// <summary>
		/// Retain value and stay in edit mode.
		/// </summary>
		RetainValue,

		/// <summary>
		/// Revert value to original value.
		/// </summary>
		RevertValue,

		/// <summary>
		/// Display an error message and retain value.
		/// </summary>
		DisplayErrorMessage
	}

	#endregion // InvalidValueBehavior



    #region MonthCalendarStates
    /// <summary>
    /// Enumeration used to indicate the current state of the component.
    /// </summary>
    [Flags()]
    public enum MonthCalendarStates : long
    {
        /// <summary>
        /// The <see cref="XamMonthCalendar.MinDate"/> is in view
        /// </summary>
        MinDateInView = 0x00000001,

        /// <summary>
        /// The <see cref="XamMonthCalendar.MaxDate"/> is in view
        /// </summary>
        MaxDateInView = 0x00000002,

        /// <summary>
        /// Indicates that the current date is within the minimum and maximum date range of the <see cref="XamMonthCalendar"/>
        /// </summary>
        TodayIsEnabled = 0x00000004,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.ActiveDate"/> is non-null.
        /// </summary>
        ActiveDate = 0x00000008,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.CurrentCalendarMode"/> can be changed to a higher scope - e.g. from Days to Months.
        /// </summary>
        CanZoomOutCalendarMode = 0x00000010,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.CurrentCalendarMode"/> can be changed towards the <see cref="XamMonthCalendar.MinCalendarMode"/> - e.g. from Months to Days.
        /// </summary>
        CanZoomInCalendarMode = 0x00000020,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Days</b>
        /// </summary>
        CalendarModeDays = 0x00000040,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Months</b>
        /// </summary>
        CalendarModeMonths = 0x00000080,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Years</b>
        /// </summary>
        CalendarModeYears = 0x00000100,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Decades</b>
        /// </summary>
        CalendarModeDecades = 0x00000200,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.CurrentCalendarMode"/> is set to <b>Centuries</b>
        /// </summary>
        CalendarModeCenturies = 0x00000400,

        /// <summary>
        /// Indicates that the <see cref="XamMonthCalendar.CurrentCalendarMode"/> is the same value as the <see cref="XamMonthCalendar.MinCalendarMode"/>
        /// </summary>
        MinCalendarMode = 0x00000800,

		// AS 1/5/10 TFS23198
		/// <summary>
		/// Indicates that the FlowDirection of the <see cref="XamMonthCalendar"/> is RightToLeft.
		/// </summary>
		RightToLeft = 0x00001000,
	}
    #endregion //MonthCalendarStates

    #region MonthCalendarChange
    internal enum MonthCalendarChange
    {
        WeekRuleChanged,
        FirstDayOfWeekChanged,
        DaysOfWeekChanged,
        MinDateChanged,
        MaxDateChanged,
        DisabledDatesChanged,
        LanguageChanged,
        ItemStyleChange,
        SelectionChanged,
        WorkdaysChanged,
        // AS 10/3/08 TFS8607
        CurrentModeChanged,
		// AS 3/23/10 TFS26461
		TodayChanged,
    } 
    #endregion //MonthCalendarChange

    #region CalendarItemChange
    internal enum CalendarItemChange
    {
        // CalendarItem|Day prop changed
        Style,
        // Enabled days changed
        Enabled,
        // Selection changed
        Selection,
		// AS 3/23/10 TFS26461
		Today,
    } 
    #endregion //CalendarItemChange

    #region CalendarAnimation
    internal enum CalendarAnimation
    {
        Fade,
        Scroll,
        ZoomIn,
        ZoomOut,
    } 
    #endregion //CalendarAnimation

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