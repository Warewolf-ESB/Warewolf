using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Globalization;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// Represents a specific date in a <see cref="CalendarItemGroup"/>
	/// </summary>
    /// <remarks>
    /// <p class="body">A CalendarDay is a custom <see cref="CalendarItem"/> that represents a 
    /// single DateTime. The <see cref="CalendarItem.StartDate"/> and <see cref="CalendarItem.EndDate"/> 
    /// values will be the same. This element is only used when the <see cref="CalendarItemGroup.GetCurrentMode(DependencyObject)"/> 
    /// is <b>Days</b>.</p>
    /// </remarks>
    //[System.ComponentModel.ToolboxItem(false)]
	[TemplateVisualState(Name = VisualStateUtilities.StateWorkDay,          GroupName = VisualStateUtilities.GroupWorkDay)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNonWorkDay,       GroupName = VisualStateUtilities.GroupWorkDay)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public sealed class CalendarDay : CalendarItem, ITimeRangePresenter
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



			this.InternalSetPropValue(PropFlags.IsWorkday, true);
		}

		internal CalendarDay(DateTime date, CalendarItemGroup month) : base(date, date, month)
		{



			this.InternalSetPropValue(PropFlags.IsWorkday, true);
		}
		#endregion //Constructor

		#region Properties

		#region Public

		#region IsWorkday

		internal static readonly DependencyPropertyKey IsWorkdayPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsWorkday",
			typeof(bool), typeof(CalendarDay),
			KnownBoxes.TrueBox,
			OnIsWorkdayChanged
			);

		private static void OnIsWorkdayChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CalendarDay instance = target as CalendarDay;

			instance.InternalSetPropValue(PropFlags.IsWorkday, (bool)e.NewValue);
			instance.InitializeIsHighlighted();
			instance.UpdateVisualStates();
		}


		/// <summary>
		/// Identifies the read-only <see cref="IsWorkday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWorkdayProperty = IsWorkdayPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns a boolean indicating whether the day is considered a work day as set in the <see cref="XamCalendar.Workdays"/>
        /// </summary>
        /// <remarks>
        /// <p class="body">The IsWorkday is initialized based on the <see cref="XamCalendar.Workdays"/> of the containing 
        /// <see cref="CalendarBase"/> and is intended to allow custom styling of days that represent working or non-working days 
        /// such as weekends.</p>
        /// </remarks>
        /// <seealso cref="IsWorkdayProperty"/>
        /// <seealso cref="XamCalendar.Workdays"/>
        //[Description("Returns a boolean indicating whether the day is considered a work day as set in the CalendarBase's Workdays property.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
		public bool IsWorkday
		{
			get
			{
				return this.InternalGetPropValue(PropFlags.IsWorkday);
			}
			internal set
			{
				this.SetValue(CalendarDay.IsWorkdayPropertyKey, value);
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

			//if ( !this.IsWorkday)
			//    this.ClearValue(IsWorkdayPropertyKey);
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

		#region Methods

		#region Internal Metods

		#region InitializeIsHighlighted

		internal void InitializeIsHighlighted()
		{
			CalendarItemGroup group = this.Group;

			if (group == null || group.IsGroupForSizing)
				return;

			CalendarBase cal = CalendarBase.GetCalendar(group);

			if (cal == null)
				return;

			CalendarUtilities.SetBoolProperty(this, IsHighlightedPropertyKey, cal.ShouldHighlightDay(this), this.IsHighlighted, false);
		}

		#endregion //InitializeIsHighlighted

		#endregion //Internal Metods	
    
		#endregion //Methods

		#region ITimeRangePresenter Members

		TimeRangeKind ITimeRangePresenter.Kind
		{
			get { return TimeRangeKind.Day; }
		}

		#endregion

		#region ITimeRange Members

		DateTime ITimeRange.Start
		{
			get 
			{
				CalendarBase calendar = CalendarUtilities.GetCalendar(this);
				DateTime start = this.StartDate;

				if (calendar != null)
					start = calendar.ApplyLogicalDayOffset(start);
				
				return start; 
			}
		}

		DateTime ITimeRange.End
		{
			get
			{
				CalendarBase calendar = CalendarUtilities.GetCalendar(this);
				DateTime end = ((ITimeRange)this).Start;

				if (calendar != null)
					end = calendar.AddLogicalDayDuration(end);

				return end;
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