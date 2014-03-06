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
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{

	#region CalendarDateSelectionMode

	/// <summary>
	/// Used to specify the type of selection that is allowed for an object.
	/// </summary>
	public enum CalendarDateSelectionMode
	{

		/// <summary>
		/// Extended Select. Multiple dates in multiple ranges may be selected at once.
		/// </summary>
		Extended = 0,

		/// <summary>
		/// No dates may be selected.
		/// </summary>
		None = 1,

		/// <summary>
		/// Single Select. Only one date may be selected at any time.
		/// </summary>
		SingleDate = 2,

		/// <summary>
		/// A single range that could include multiple contiguous dates may be selected.
		/// </summary>
		Range = 3,
	}

	#endregion CalendarDateSelectionMode

	#region CalendarStates
	/// <summary>
    /// Enumeration used to indicate the current state of the component.
    /// </summary>
    [Flags()]
    public enum CalendarStates : long
    {
        /// <summary>
        /// The <see cref="XamCalendar.MinDate"/> is in view
        /// </summary>
        MinDateInView = 0x00000001,

        /// <summary>
        /// The <see cref="XamCalendar.MaxDate"/> is in view
        /// </summary>
        MaxDateInView = 0x00000002,

        /// <summary>
        /// Indicates that the current date is within the minimum and maximum date range of the <see cref="CalendarBase"/>
        /// </summary>
        TodayIsEnabled = 0x00000004,

        /// <summary>
        /// Indicates that the <see cref="CalendarBase.ActiveDate"/> is non-null.
        /// </summary>
        ActiveDate = 0x00000008,

        /// <summary>
        /// Indicates that the <see cref="CalendarBase.CurrentMode"/> can be changed to a higher scope - e.g. from Days to Months.
        /// </summary>
        CanZoomOutCalendarMode = 0x00000010,

        /// <summary>
		/// Indicates that the <see cref="CalendarBase.CurrentMode"/> can be changed towards the <see cref="XamCalendar.MinCalendarMode"/> - e.g. from Months to Days.
        /// </summary>
        CanZoomInCalendarMode = 0x00000020,

        /// <summary>
        /// Indicates that the <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>
        /// </summary>
        CalendarModeDays = 0x00000040,

        /// <summary>
        /// Indicates that the <see cref="CalendarBase.CurrentMode"/> is set to <b>Months</b>
        /// </summary>
        CalendarModeMonths = 0x00000080,

        /// <summary>
        /// Indicates that the <see cref="CalendarBase.CurrentMode"/> is set to <b>Years</b>
        /// </summary>
        CalendarModeYears = 0x00000100,

        /// <summary>
        /// Indicates that the <see cref="CalendarBase.CurrentMode"/> is set to <b>Decades</b>
        /// </summary>
        CalendarModeDecades = 0x00000200,

        /// <summary>
        /// Indicates that the <see cref="CalendarBase.CurrentMode"/> is set to <b>Centuries</b>
        /// </summary>
        CalendarModeCenturies = 0x00000400,

        /// <summary>
		/// Indicates that the <see cref="CalendarBase.CurrentMode"/> is the same value as the <see cref="XamCalendar.MinCalendarMode"/>
        /// </summary>
        MinCalendarMode = 0x00000800,

		// AS 1/5/10 TFS23198
		/// <summary>
		/// Indicates that the FlowDirection of the <see cref="CalendarBase"/> is RightToLeft.
		/// </summary>
		RightToLeft = 0x00001000,
	}
    #endregion //CalendarStates

    #region CalendarChange
	internal enum CalendarChange : short
    {
        WeekRuleChanged,
        WeekNumberVisibility,
        FirstDayOfWeekChanged,
        DaysOfWeekChanged,
        AllowableDatesChanged,
        DisabledDatesChanged,
        CalendarInfoChanged,
        ItemStyleChange,
        SelectionChanged,
        WorkdaysChanged,
        CurrentModeChanged,
		TodayChanged,
        IsSelectionActiveChanged,
		ReferenceDateChanged,
		Resources,
    } 
    #endregion //CalendarChange

	#region CalendarCommandType

	/// <summary>
	/// Identifies the commands exposed by XamCalendar
	/// </summary>
	public enum CalendarCommandType : short
	{
		/// <summary>
		/// Activate a particular date. The source must be a <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> or within it - or the CommandParameter must be the date of the day to activate.
		/// </summary>
		ActivateDate,
		/// <summary>
		/// Ensures the <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> that represents the <see cref="CalendarBase.SelectedDate"/> is in view and has the input focus.
		/// </summary>
		ActivateSelectedDate,
		/// <summary>
		/// Navigates to the first <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> of the first <see cref="CalendarItemGroup"/> in a <see cref="CalendarBase"/> (e.g. the first day of the first month currently in view when <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>).
		/// </summary>
		FirstItemOfFirstGroup,
		/// <summary>
		/// Navigates to the first <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> in the current group (e.g. the first day of the month containing the current <see cref="CalendarBase.ActiveDate"/>  when <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>).
		/// </summary>
		FirstItemOfGroup,
		/// <summary>
		/// Navigates to the last <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> in the current group (e.g. the last day of the month containing the current <see cref="CalendarBase.ActiveDate"/>  when <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>).
		/// </summary>
		LastItemOfGroup,
		/// <summary>
		/// Navigates to the last <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> of the last <see cref="CalendarItemGroup"/> in a <see cref="CalendarBase"/> (e.g. the first day of the last month currently in view when <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>).
		/// </summary>
		LastItemOfLastGroup,
		/// <summary>
		/// Navigates to the <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> in the previous group (e.g. the same day of the month as the <see cref="CalendarBase.ActiveDate"/> in the following month when <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>).
		/// </summary>
		NextGroup,
		/// <summary>
		/// Navigates to the <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> after the <see cref="CalendarBase.ActiveDate"/>.
		/// </summary>
		NextItem,
		/// <summary>
		/// Navigates to the <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> in the previous row (e.g. the same day of the week as the <see cref="CalendarBase.ActiveDate"/> in the following week when <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>).
		/// </summary>
		NextItemRow,
		/// <summary>
		/// Navigates to the <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> in the previous group (e.g. the same day of the month as the <see cref="CalendarBase.ActiveDate"/> in the previous month when <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>).
		/// </summary>
		PreviousGroup,
		/// <summary>
		/// Navigates to the <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> previous to the <see cref="CalendarBase.ActiveDate"/>.
		/// </summary>
		PreviousItem,
		/// <summary>
		/// Navigates to the <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/> in the previous row (e.g. the same day of the week as the <see cref="CalendarBase.ActiveDate"/> in the previous week when <see cref="CalendarBase.CurrentMode"/> is set to <b>Days</b>).
		/// </summary>
		PreviousItemRow,
		/// <summary>
		/// Scroll forward by one group
		/// </summary>
		ScrollNextGroup,
		/// <summary>
		/// Scroll forward by the number of groups currently in view
		/// </summary>
		ScrollNextGroups,
		/// <summary>
		/// Scroll backward by one group
		/// </summary>
		ScrollPreviousGroup,
		/// <summary>
		/// Scroll backward by the number of groups currently in view
		/// </summary>
		ScrollPreviousGroups,
		/// <summary>
		/// Scrolls to the date specified in the command parameter. If this command is sent from within a <see cref="CalendarItemGroup"/>, the date will be scrolled into view in that group if possible - even if it is already in view within another <see cref="CalendarItemGroup"/>.
		/// </summary>
		ScrollToDate,
		/// <summary>
		/// Activating the <see cref="CalendarDay"/> that represents the current date.
		/// </summary>
		Today,
		/// <summary>
		/// Toggles the selection of the item represented by the <see cref="CalendarBase.ActiveDate"/>.
		/// </summary>
		ToggleActiveDateSelection,
		/// <summary>
		/// Increases the <see cref="CalendarBase.CurrentMode"/> to a larger date range - e.g. from <b>Days</b> to <b>Months</b>.
		/// </summary>
		ZoomOutCalendarMode,
		/// <summary>
		/// Decreases the <see cref="CalendarBase.CurrentMode"/> to a smaller date range - e.g. from <b>Months</b> to <b>Days</b>.
		/// </summary>
		ZoomInCalendarMode
	}

	#endregion //CalendarCommandType	
    
    #region CalendarItemChange
	internal enum CalendarItemChange : short
    {
        // CalendarItem|Day prop changed
        Style,
        // Enabled days changed
        Enabled,
        // Selection changed
        Selection,
		// AS 3/23/10 TFS26461
		Today,

		IsSelectionActive,
		
		Resources,
	} 
    #endregion //CalendarItemChange

    #region CalendarAnimation
	internal enum CalendarAnimation : short
    {
        Fade,
        Scroll,
        ZoomIn,
        ZoomOut,
    } 
    #endregion //CalendarAnimation

	#region CalendarResourceId

	/// <summary>
	/// An enum that is used to identify certain resources, e.g. brushes, used by elements within the visual tree of <see cref="CalendarBase"/> derived controls
	/// </summary>
	/// <seealso cref="CalendarResourceProvider"/>
	public enum CalendarResourceId : short
	{
		/// <summary>
		/// The background brush of the navigator control
		/// </summary>
		CalendarBackgroundBrush,
		/// <summary>
		/// The style used for <see cref="Infragistics.Controls.Editors.Primitives.CalendarDay"/>s
		/// </summary>
		CalendarDayStyle,
		/// <summary>
		/// The style used for <see cref="Infragistics.Controls.Editors.Primitives.CalendarItem"/>s
		/// </summary>
		CalendarItemStyle,
		/// <summary>
		/// The background brush of a day of week header
		/// </summary>
		DayOfWeekBackgroundBrush,
		/// <summary>
		/// The foreground brush of a day of week header
		/// </summary>
		DayOfWeekForegroundBrush,
		/// <summary>
		/// The brush used for the separator between the day week header and the days
		/// </summary>
		DayOfWeekSeparatorBrush,
		// JJD 8/26/11 - TFS85067 - added DisabledItemBackgroundBrush id
		/// <summary>
		/// The background brush of an item when it is disabled
		/// </summary>
		DisabledItemBackgroundBrush,
		/// <summary>
		/// The foreground brush of an item when it is disabled
		/// </summary>
		DisabledItemForegroundBrush,
		/// <summary>
		/// The background brush of an item group title
		/// </summary>
		GroupTitleBackgroundBrush,
		/// <summary>
		/// The foreground brush of an item group title
		/// </summary>
		GroupTitleForegroundBrush,
		/// <summary>
		/// The background brush of an item
		/// </summary>
		ItemBackgroundBrush,
		/// <summary>
		/// The border brush of an item
		/// </summary>
		ItemBorderBrush,
		/// <summary>
		/// The corner radius of an item
		/// </summary>
		ItemCorderRadius,
		/// <summary>
		/// The foreground brush of an item 
		/// </summary>
		ItemForegroundBrush,
		/// <summary>
		/// The inner border brush of an item 
		/// </summary>
		ItemInnerBorderBrush,
		/// <summary>
		/// The foreground brush of an item that is before or after the months being displayed.
		/// </summary>
		LeadingOrTrailingItemForegroundBrush,
		/// <summary>
		/// The background brush of an item group title when the mouse is over it.
		/// </summary>
		MouseOverGroupTitleBackgroundBrush,
		/// <summary>
		/// The foreground brush of an item group title when the mouse is over it.
		/// </summary>
		MouseOverGroupTitleForegroundBrush,
		/// <summary>
		/// The background brush of an item when the mouse is over it.
		/// </summary>
		MouseOverItemBackgroundBrush,
		/// <summary>
		/// The border brush of an item when the mouse is over it.
		/// </summary>
		MouseOverItemBorderBrush,
		/// <summary>
		/// The foreground brush of an item when the mouse is over it. 
		/// </summary>
		MouseOverItemForegroundBrush,
		/// <summary>
		/// The inner border brush of an item when the mouse is over it. 
		/// </summary>
		MouseOverItemInnerBorderBrush,
		/// <summary>
		/// The style used for the RepeatButton that will navigate forward.
		/// </summary>
		ScrollNextRepeatButtonStyle,
		/// <summary>
		/// The style used for the RepeatButton that will navigate backward.
		/// </summary>
		ScrollPreviousRepeatButtonStyle,
		/// <summary>
		/// The background brush of an item when it is selected
		/// </summary>
		SelectedFocusedItemBackgroundBrush,
		/// <summary>
		/// The border brush of an item when it is selected and focused
		/// </summary>
		SelectedFocusedItemBorderBrush,
		/// <summary>
		/// The foreground brush of an item when it is selected and focused
		/// </summary>
		SelectedFocusedItemForegroundBrush,
		/// <summary>
		/// The inner border brush of an item when it is selected and focused
		/// </summary>
		SelectedFocusedItemInnerBorderBrush,
		/// <summary>
		/// The background brush of an item when it is selected and focused
		/// </summary>
		SelectedItemBackgroundBrush,
		/// <summary>
		/// The border brush of an item when it is selected
		/// </summary>
		SelectedItemBorderBrush,
		/// <summary>
		/// The foreground brush of an item when it is selected 
		/// </summary>
		SelectedItemForegroundBrush,
		/// <summary>
		/// The inner border brush of an item when it is selected 
		/// </summary>
		SelectedItemInnerBorderBrush,
		/// <summary>
		/// The background brush of the item representing today.
		/// </summary>
		TodayBackgroundBrush,
		/// <summary>
		/// The border brush of the item representing today.
		/// </summary>
		TodayBorderBrush,
		/// <summary>
		/// The style used for the Button that will navigate to today.
		/// </summary>
		TodayButtonStyle,
		/// <summary>
		/// The foreground brush of the item representing today.
		/// </summary>
		TodayForegroundBrush,
		/// <summary>
		/// The inner border brush of the item representing today.
		/// </summary>
		TodayInnerBorderBrush,
	}

	#endregion //CalendarResourceId	
    
	#region CalendarResourceSet

	/// <summary>
	/// An enum that is used to identify the built in themes used by elements within the visual tree of <see cref="CalendarBase"/> derived controls
	/// </summary>
	/// <seealso cref="CalendarResourceProvider"/>
	/// <seealso cref="CalendarResourceProvider.ResourceSet"/>
	public enum CalendarResourceSet : short
	{
		/// <summary>
		/// The default resource set
		/// </summary>
		Generic,
		/// <summary>
		/// The resource set for the Office 2010 Blue theme
		/// </summary>
		Office2010Blue,
		/// <summary>
		/// The resource set for the Office 2010 Black theme
		/// </summary>
		Office2010Black,
		/// <summary>
		/// The resource set for the Office 2010 Silver theme
		/// </summary>
		Office2010Silver,
		/// <summary>
		/// The resource set for the Office 2007 Blue theme
		/// </summary>
		Office2007Blue,
		/// <summary>
		/// The resource set for the Office 2007 Black theme
		/// </summary>
		Office2007Black,
		/// <summary>
		/// The resource set for the Office 2007 Silver theme
		/// </summary>
		Office2007Silver,
		/// <summary>
		/// The resource set for the IG theme
		/// </summary>
		IGTheme
	}

	#endregion //CalendarResourceSet
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