using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules
{

	#region ScheduleSettings Class

	/// <summary>
	/// Contains schedule settings information.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ScheduleSettings</b> is used by the <see cref="XamScheduleDataManager.Settings"/> property.
	/// </para>
	/// </remarks>
	public class ScheduleSettings : DependencyObject, ISupportPropertyChangeNotifications
	{
        #region Member Vars

        internal const DayOfWeekFlags DEFAULT_WORKDAYS = DayOfWeekFlags.Monday | DayOfWeekFlags.Tuesday | DayOfWeekFlags.Wednesday | DayOfWeekFlags.Thursday | DayOfWeekFlags.Friday;

        private DaySettingsOverrideCollection _daySettingsOverrides;

        private PropertyChangeListenerList _propChangeListeners = new PropertyChangeListenerList( );

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ScheduleSettings"/>.
        /// </summary>
        public ScheduleSettings( )
        {
			_cachedLogicalDayOffset = (TimeSpan)this.GetValue(LogicalDayOffsetProperty);
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

		#region AllowCalendarClosing

		/// <summary>
        /// Identifies the <see cref="AllowCalendarClosing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowCalendarClosingProperty = DependencyPropertyUtilities.Register(
            "AllowCalendarClosing",
            typeof( bool ),
            typeof( ScheduleSettings ),
            DependencyPropertyUtilities.CreateMetadata( KnownBoxes.TrueBox, OnPropertyChangedCallback )
        );

        /// <summary>
        /// Specifies whether the user is allowed to close calendars via the UI.
        /// </summary>
		/// <remarks>
		/// <p class="body">The XamSchedule view controls optionally display a close button in the <see cref="CalendarHeader"/> that allows 
		/// the end user to close a <see cref="ResourceCalendar"/>. The AllowCalendarClosing property can be used to prevent the end 
		/// user from closing/hiding a calendar.</p>
		/// <p class="body">The <see cref="ScheduleControlBase.ShowCalendarCloseButton"/> property is used to determine if the close button is 
		/// displayed within the <see cref="CalendarHeader"/>. By default that property is set to null. When left to this default, the visibility 
		/// of the close button is resolved as follows. If the AllowCalendarClosing is false then the button will be hidden. Otherwise the default 
		/// visibility is up to the control. For example, in <see cref="XamScheduleView"/>, the close button is not displayed by default but for 
		/// <see cref="XamDayView"/> and <see cref="XamMonthView"/> it will be displayed. This mimics the default ui displayed in Microsoft Outlook.</p>
		/// </remarks>
		/// <seealso cref="CalendarHeader.CloseButtonVisibility"/>
		/// <seealso cref="ScheduleControlBase.ShowCalendarCloseButton"/>
		public bool AllowCalendarClosing
        {
            get
            {
                return (bool)this.GetValue( AllowCalendarClosingProperty );
            }
            set
            {
                this.SetValue( AllowCalendarClosingProperty, value );
            }
        }

        #endregion // AllowCalendarClosing

		#region AllowCustomizedCategories

		// SSP 1/6/11 - NAS11.1 Activity Categories
		// 

		/// <summary>
		/// Identifies the <see cref="AllowCustomizedCategories"/> dependency property
		/// </summary>

		[InfragisticsFeature( FeatureName = "ActivityCategories", Version = "11.1" )]

		public static readonly DependencyProperty AllowCustomizedCategoriesProperty = DependencyPropertyUtilities.Register(
			"AllowCustomizedCategories",
			typeof( bool? ),
			typeof( ScheduleSettings ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnPropertyChangedCallback ) )
		);

		/// <summary>
		/// Specifies whether the user is allowed to create custom activity categories.
		/// </summary>
		/// <seealso cref="AllowCustomizedCategoriesProperty"/>
		/// <remarks>
		/// <para class="body">
		/// <b>AllowCustomizedCategories</b> property specifies whether the user can create and modify
		/// custom activity categories. User's custom activity categories are stored in <see cref="Resource"/>'s 
		/// <see cref="Resource.CustomActivityCategories"/> property. When this feature is enabled,
		/// an UI will be presented to the user in the activity dialog to allow adding a new custom
		/// category as well as modifying or removing any custom categories previously added. The UI 
		/// will not allow the user to modify the categories specified via the
		/// <see cref="ListScheduleDataConnector.ActivityCategoryItemsSource"/> since they are common
		/// across all resources. The user (<see cref="XamScheduleDataManager.CurrentUser"/>) can only 
		/// modify the his or her own custom categories.
		/// </para>
		/// </remarks>
		/// <seealso cref="ListScheduleDataConnector.ActivityCategoryItemsSource"/>
		/// <seealso cref="Resource.CustomActivityCategories"/>

		[InfragisticsFeature( FeatureName = "ActivityCategories", Version = "11.1" )]

		public bool? AllowCustomizedCategories
		{
			get
			{
				return (bool?)this.GetValue( AllowCustomizedCategoriesProperty );
			}
			set
			{
				this.SetValue( AllowCustomizedCategoriesProperty, value );
			}
		}

		#endregion // AllowCustomizedCategories

		#region AppointmentSettings

		/// <summary>
		/// Identifies the <see cref="AppointmentSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AppointmentSettingsProperty = DependencyPropertyUtilities.Register(
			"AppointmentSettings",
			typeof( AppointmentSettings ),
			typeof( ScheduleSettings ),
			null, OnPropertyChangedCallback
		);

		/// <summary>
		/// Gets or sets <see cref="Infragistics.Controls.Schedules.AppointmentSettings"/> object that contains settings for appointments.
		/// </summary>
		/// <seealso cref="Infragistics.Controls.Schedules.AppointmentSettings"/>
		/// <seealso cref="JournalSettings"/>
		/// <seealso cref="TaskSettings"/>
		public AppointmentSettings AppointmentSettings
		{
			get
			{
				return (AppointmentSettings)this.GetValue( AppointmentSettingsProperty );
			}
			set
			{
				this.SetValue( AppointmentSettingsProperty, value );
			}
		}

		#endregion // AppointmentSettings		

        #region DaySettingsOverrides

        /// <summary>
        /// Collection of <see cref="DaySettingsOverride"/> objects. Used for overriding day settings 
        /// (like working hours, whether the day is a workday etc…) for specific dates as well as specific 
        /// recurring dates.
        /// </summary>
        public DaySettingsOverrideCollection DaySettingsOverrides
        {
            get
            {
                if ( null == _daySettingsOverrides )
                {
					DaySettingsOverrideCollection coll = new DaySettingsOverrideCollection( );
                    _daySettingsOverrides = coll;

                    coll.PropChangeListeners.Add( _propChangeListeners, false );
                }

                return _daySettingsOverrides;
            }
        }

        #endregion // DaySettingsOverrides

		#region DaysOfWeek

		/// <summary>
		/// Identifies the <see cref="DaysOfWeek"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DaysOfWeekProperty = DependencyPropertyUtilities.Register(
			"DaysOfWeek",
			typeof( ScheduleDaysOfWeek ),
			typeof( ScheduleSettings ),
			DependencyPropertyUtilities.CreateMetadata( null, OnPropertyChangedCallback )
		);

		/// <summary>
		/// Gets or sets <see cref="ScheduleDaysOfWeek"/> object that contains settings for days of week. Default value is <b>Null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DaysOfWeek</b> returns <see cref="ScheduleDaysOfWeek"/> object that is used to specifiy settings, like
		/// whether the day is a workday, for specific days of week.
		/// </para>
		/// </remarks>
		/// <seealso cref="ScheduleDaysOfWeek"/>
		public ScheduleDaysOfWeek DaysOfWeek
		{
			get
			{
				return (ScheduleDaysOfWeek)this.GetValue( DaysOfWeekProperty );
			}
			set
			{
				this.SetValue( DaysOfWeekProperty, value );
			}
		}

		#endregion // DaysOfWeek

		#region FirstDayOfWeek

		/// <summary>
		/// Identifies the <see cref="FirstDayOfWeek"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstDayOfWeekProperty = DependencyPropertyUtilities.Register(
			"FirstDayOfWeek",
			typeof( DayOfWeek? ),
			typeof( ScheduleSettings ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnPropertyChangedCallback ) )
		);

		/// <summary>
		/// Specifies the first day of week.
		/// </summary>
		/// <seealso cref="FirstDayOfWeekProperty"/>
		/// <seealso cref="DaysOfWeek"/>
		/// <seealso cref="DaySettings"/>
		/// <seealso cref="Resource.FirstDayOfWeek"/>
		public DayOfWeek? FirstDayOfWeek
		{
			get
			{
				return (DayOfWeek?)this.GetValue( FirstDayOfWeekProperty );
			}
			set
			{
				this.SetValue( FirstDayOfWeekProperty, value );
			}
		}

		#endregion // FirstDayOfWeek

		#region JournalSettings

		/// <summary>
		/// Identifies the <see cref="JournalSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty JournalSettingsProperty = DependencyPropertyUtilities.Register(
			"JournalSettings",
			typeof( JournalSettings ),
			typeof( ScheduleSettings ),
			null, OnPropertyChangedCallback
		);

		/// <summary>
		/// Gets or sets <see cref="Infragistics.Controls.Schedules.JournalSettings"/> object that contains settings for journals.
		/// </summary>
		/// <seealso cref="Infragistics.Controls.Schedules.JournalSettings"/>
		/// <seealso cref="AppointmentSettings"/>
		/// <seealso cref="TaskSettings"/>
		public JournalSettings JournalSettings
		{
			get
			{
				return (JournalSettings)this.GetValue( JournalSettingsProperty );
			}
			set
			{
				this.SetValue( JournalSettingsProperty, value );
			}
		}

		#endregion // JournalSettings		
        
        #region LogicalDayDuration

        /// <summary>
        /// Identifies the <see cref="LogicalDayDuration"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LogicalDayDurationProperty = DependencyPropertyUtilities.Register(
            "LogicalDayDuration",
            typeof( TimeSpan ),
            typeof( ScheduleSettings ),
            DependencyPropertyUtilities.CreateMetadata( TimeSpan.FromDays(1), OnPropertyChangedCallback )
        );

        /// <summary>
        /// Specifies the logical day duration - the duration of the day. 
        /// </summary>
        public TimeSpan LogicalDayDuration
        {
            get
            {
                return (TimeSpan)this.GetValue( LogicalDayDurationProperty );
            }
            set
            {
                this.SetValue( LogicalDayDurationProperty, value );
            }
        }

        #endregion // LogicalDayDuration

        #region LogicalDayOffset

		private TimeSpan _cachedLogicalDayOffset;

        /// <summary>
        /// Identifies the <see cref="LogicalDayOffset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LogicalDayOffsetProperty = DependencyPropertyUtilities.Register(
            "LogicalDayOffset",
            typeof( TimeSpan ),
            typeof( ScheduleSettings ),
            DependencyPropertyUtilities.CreateMetadata( TimeSpan.Zero, OnPropertyChangedCallback )
        );

        /// <summary>
        /// Specifies the logical day offset - the time when the day starts.
        /// </summary>
        public TimeSpan LogicalDayOffset
        {
            get
            {
				return _cachedLogicalDayOffset;
            }
            set
            {
                this.SetValue( LogicalDayOffsetProperty, value );
            }
        }

        #endregion // LogicalDayOffset

        #region MaxDate

        /// <summary>
        /// Identifies the <see cref="MaxDate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxDateProperty = DependencyPropertyUtilities.Register(
            "MaxDate",
            typeof( DateTime? ),
            typeof( ScheduleSettings ),
            DependencyPropertyUtilities.CreateMetadata( null, OnPropertyChangedCallback )
        );

        /// <summary>
        /// Maximum date allowed in the UI. Appointment creation will also be restricted within the MinDate and MaxDate.
        /// </summary>



		public DateTime? MaxDate
        {
            get
            {
                return (DateTime?)this.GetValue( MaxDateProperty );
            }
            set
            {
                this.SetValue( MaxDateProperty, value );
            }
        }

        #endregion // MaxDate

        #region MinDate

        /// <summary>
        /// Identifies the <see cref="MinDate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinDateProperty = DependencyPropertyUtilities.Register(
            "MinDate",
            typeof( DateTime? ),
            typeof( ScheduleSettings ),
            DependencyPropertyUtilities.CreateMetadata( null, OnPropertyChangedCallback )
        );

        /// <summary>
        /// Minimum date allowed in the UI. Appointment creation will also be restricted within the MinDate and MaxDate.
        /// </summary>



		public DateTime? MinDate
        {
            get
            {
                return (DateTime?)this.GetValue( MinDateProperty );
            }
            set
            {
                this.SetValue( MinDateProperty, value );
            }
        }

        #endregion // MinDate

		#region TaskSettings

		/// <summary>
		/// Identifies the <see cref="TaskSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TaskSettingsProperty = DependencyPropertyUtilities.Register(
			"TaskSettings",
			typeof( TaskSettings ),
			typeof( ScheduleSettings ),
			null, OnPropertyChangedCallback
		);

		/// <summary>
		/// Gets or sets <see cref="Infragistics.Controls.Schedules.TaskSettings"/> object that contains settings for tasks.
		/// </summary>
		/// <seealso cref="Infragistics.Controls.Schedules.TaskSettings"/>
		/// <seealso cref="AppointmentSettings"/>
		/// <seealso cref="TaskSettings"/>
		public TaskSettings TaskSettings
		{
			get
			{
				return (TaskSettings)this.GetValue( TaskSettingsProperty );
			}
			set
			{
				this.SetValue( TaskSettingsProperty, value );
			}
		}

		#endregion // TaskSettings

        #region WorkDays

        /// <summary>
        /// Identifies the <see cref="WorkDays"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WorkDaysProperty = DependencyPropertyUtilities.Register(
            "WorkDays",
            typeof( DayOfWeekFlags ),
            typeof( ScheduleSettings ),
            DependencyPropertyUtilities.CreateMetadata( DEFAULT_WORKDAYS, OnPropertyChangedCallback )
        );

        /// <summary>
        /// Specifies which days of week are workdays.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// <b>WorkDays</b> property is used to specify which days of week are work-days. You can use the <see cref="WorkingHours"/>
        /// property to specify the working hours. You can also use the <see cref="DaysOfWeek"/> property to specify
        /// working hours on a per day basis as well as whether a day is a work-day or not.
        /// </para>
		/// <para class="body">
		/// You can also specify work-days and work-hours for a specific resource using the <see cref="Resource"/>'s <see cref="Resource.DaysOfWeek"/> collection.
		/// </para>
        /// </remarks>
        /// <seealso cref="WorkDaysProperty"/>
        /// <seealso cref="WorkingHours"/>
        /// <seealso cref="DaySettings.IsWorkday"/>
        /// <seealso cref="DaysOfWeek"/>
		/// <seealso cref="Resource.DaysOfWeek"/>
        public DayOfWeekFlags WorkDays
        {
            get
            {
                return (DayOfWeekFlags)this.GetValue( WorkDaysProperty );
            }
            set
            {
                this.SetValue( WorkDaysProperty, value );
            }
        }

        #endregion // WorkDays

        #region WorkingHours



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		/// <summary>
        /// Identifies the <see cref="WorkingHours"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WorkingHoursProperty = DaySettings.WorkingHoursProperty.AddOwner(
            typeof( ScheduleSettings ), DependencyPropertyUtilities.CreateMetadata( OnPropertyChangedCallback ) );


        /// <summary>
        /// Specifies working hours. Default value is null.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// <b>WorkingHours</b> property specifies the working hours for all days of week. You can also 
        /// use the <see cref="DaysOfWeek"/> property to specify per day working hours as well which 
        /// days of week are work days. You can also specify per resource working hours and work days
        /// using the <see cref="Resource.DaysOfWeek"/> property.
        /// </para>
        /// <para class="body">
        /// If working hours for a day are not specified via <i>DaysOfWeek</i> property then this property's
        /// value is used. If this property is not specified then the default value of 9:00 AM - 5:00 PM 
        /// will be used.
        /// </para>
        /// </remarks>
        /// <seealso cref="WorkingHoursProperty"/>
        /// <seealso cref="ScheduleSettings.DaysOfWeek"/>
        /// <seealso cref="Resource.DaysOfWeek"/>
        /// <seealso cref="ScheduleDayOfWeek.DaySettings"/>
        /// <seealso cref="DaySettings.IsWorkday"/>
        /// <seealso cref="DaySettings.WorkingHours"/>
        public WorkingHoursCollection WorkingHours
        {
            get
            {
                return (WorkingHoursCollection)this.GetValue( WorkingHoursProperty );
            }
            set
            {
                this.SetValue( WorkingHoursProperty, value );
            }
        }

        #endregion // WorkingHours

        #endregion // Public Properties 

        #region Internal Properties

        #region DaySettingsOverridesIfAllocated

		internal DaySettingsOverrideCollection DaySettingsOverridesIfAllocated
        {
            get
            {
                return _daySettingsOverrides;
            }
        }

        #endregion // DaySettingsOverridesIfAllocated 

        #endregion // Internal Properties

        #endregion // Properties

        #region Methods

        #region Private Methods

        #region OnPropertyChangedCallback

        /// <summary>
        /// Property changed callback for settings properties.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPropertyChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
        {
            ScheduleSettings settings = (ScheduleSettings)sender;

			if (e.Property == LogicalDayOffsetProperty)
			{
				settings._cachedLogicalDayOffset = (TimeSpan)e.NewValue;
			}

            ScheduleUtilities.NotifyListenersHelper( settings, e, settings._propChangeListeners, true, true );
        }

        #endregion // OnPropertyChangedCallback

        #endregion // Private Methods 

        #endregion // Methods

        #region ISupportPropertyChangeNotifications Implementation

        void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
        {
            _propChangeListeners.Add( listener, useWeakReference );
        }

        void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
        {
            _propChangeListeners.Remove( listener );
        }

        #endregion // ISupportPropertyChangeNotifications Implementation
	} 

	#endregion // ScheduleSettings Class

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