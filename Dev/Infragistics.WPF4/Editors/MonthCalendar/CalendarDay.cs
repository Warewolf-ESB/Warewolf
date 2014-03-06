using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Editors
{
	/// <summary>
	/// Represents a specific date in a <see cref="CalendarItemGroup"/>
	/// </summary>
    /// <remarks>
    /// <p class="body">A CalendarDay is a custom <see cref="CalendarItem"/> that represents a 
    /// single DateTime. The <see cref="CalendarItem.StartDate"/> and <see cref="CalendarItem.EndDate"/> 
    /// values will be the same. This element is only used when the <see cref="CalendarItemGroup.GetCurrentCalendarMode(DependencyObject)"/> 
    /// is <b>Days</b>.</p>
    /// </remarks>
    //[System.ComponentModel.ToolboxItem(false)]

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateWorkDay,          GroupName = VisualStateUtilities.GroupWorkDay)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNonWorkDay,       GroupName = VisualStateUtilities.GroupWorkDay)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CalendarDay : CalendarItem
	{
		#region Constructor

		static CalendarDay()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarDay), new FrameworkPropertyMetadata(typeof(CalendarDay)));
		}

		/// <summary>
		/// Initializes a new <see cref="CalendarDay"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This constructor is only used for styling purposes. At runtime, the controls are automatically generated.</p>
		/// </remarks>
		public CalendarDay()
		{
		}

		internal CalendarDay(DateTime date, CalendarItemGroup month) : base(date, date, month)
		{
		}
		#endregion //Constructor

		#region Properties

		#region Public

        #region IsWorkday

        internal static readonly DependencyPropertyKey IsWorkdayPropertyKey =
            DependencyProperty.RegisterReadOnly("IsWorkday",
            typeof(bool), typeof(CalendarDay), new FrameworkPropertyMetadata(KnownBoxes.TrueBox

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

        ));

        /// <summary>
        /// Identifies the <see cref="IsWorkday"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsWorkdayProperty = IsWorkdayPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns or sets a boolean indicating whether the day is considered a work day as set in the <see cref="XamMonthCalendar.Workdays"/>
        /// </summary>
        /// <remarks>
        /// <p class="body">The IsWorkday is initialized based on the <see cref="XamMonthCalendar.Workdays"/> of the containing 
        /// <see cref="XamMonthCalendar"/> and is intended to allow custom styling of days that represent working or non-working days 
        /// such as weekends.</p>
        /// </remarks>
        /// <seealso cref="IsWorkdayProperty"/>
        /// <seealso cref="XamMonthCalendar.Workdays"/>
        //[Description("Returns or sets a boolean indicating whether the day is considered a work day as set in the XamMonthCalendar's Workdays property.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public bool IsWorkday
        {
            get
            {
                return (bool)this.GetValue(CalendarDay.IsWorkdayProperty);
            }
            set
            {
                this.SetValue(CalendarDay.IsWorkdayProperty, value);
            }
        }

        #endregion //IsWorkday

		#endregion //Public

		#region Private

		#endregion //Private

		#endregion //Properties

		#region Base class overrides

        #region Recycle
        internal override void Recycle(DateTime start, DateTime end)
        {
            base.Recycle(start, end);

            this.ClearValue(IsWorkdayPropertyKey);
        } 
        #endregion //Recycle

		#region ToString

		/// <summary>
		/// Overriden. Returns the date that the item represents
		/// </summary>
		/// <returns>A string including the <see cref="CalendarItem.StartDate"/>.</returns>
		public override string ToString()
		{
            return string.Format(this.CalendarManager.DateTimeFormat, "CalendarDay {0:d}", this.StartDate);
		}
		#endregion //ToString

        #region SetVisualState


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected override void SetVisualState(bool useTransitions)
        {

            base.SetVisualState(useTransitions);

            // Set WorkDay states
            if (this.IsWorkday)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateWorkDay, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNonWorkDay, useTransitions);

        }



        #endregion //SetVisualState	
    
		#endregion //Base class overrides
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