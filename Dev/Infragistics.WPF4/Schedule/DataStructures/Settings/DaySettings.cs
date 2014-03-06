using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;







namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// Used for specifying settings for a specific day.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>DaySettings</b> class is used for specifying settings like the working hours or visibility of a specific day. 
	/// <see cref="ScheduleDayOfWeek"/> exposes <see cref="ScheduleDayOfWeek.DaySettings"/> property. <i>ScheduleDayOfWeek</i>
	/// objects are accessed via the <i>ScheduleSettings'</i> <see cref="ScheduleSettings.DaysOfWeek"/> collection.
	/// <see cref="DaySettingsOverride"/> also exposes <see cref="DaySettingsOverride.DaySettings"/> property. 
	/// <i>DaySettingsOverride</i> is specified using the <i>ScheduleSettings'</i> <see cref="ScheduleSettings.DaySettingsOverrides"/>
	/// collection.
	/// </para>
	/// </remarks>
	/// <seealso cref="ScheduleSettings.DaysOfWeek"/>
	/// <seealso cref="ScheduleSettings.DaySettingsOverrides"/>
    public class DaySettings : DependencyObject, ISupportPropertyChangeNotifications
	{
		#region Member Vars

        private PropertyChangeListenerList _propChangeListeners;

		#endregion // Member Vars

		#region Properties

		#region Public Properties

		#region IsWorkday

		/// <summary>
		/// Identifies the <see cref="IsWorkday"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsWorkdayProperty = DependencyPropertyUtilities.Register(
			"IsWorkday",
			typeof( bool? ),
			typeof( DaySettings ),
            DependencyPropertyUtilities.CreateMetadata( null, OnPropertyChangedCallback )
		);

		/// <summary>
		/// Specifies whether this day of week is a work day.
		/// </summary>
		public bool? IsWorkday
		{
			get
			{
				return (bool?)this.GetValue( IsWorkdayProperty );
			}
			set
			{
				this.SetValue( IsWorkdayProperty, value );
			}
		}

		#endregion // IsWorkday

		#region WorkingHours

		/// <summary>
		/// Identifies the <see cref="WorkingHours"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty WorkingHoursProperty = DependencyPropertyUtilities.Register(
			"WorkingHours",
            typeof( WorkingHoursCollection ),
			typeof( DaySettings ),
            DependencyPropertyUtilities.CreateMetadata( null, OnPropertyChangedCallback )
		);

		/// <summary>
		/// Specifies the working hours for the day of week. Default value is <b>null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>WorkingHours</b> is used to specify different working hours for the day this DaySettings object
		/// is associated with. If this property is left <b>null</b> or set to an empty collection then the 
		/// working hours will be resolved to the next settings object in the resolution hierarchy.
		/// </para>
		/// </remarks>
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

        #region PropChangeListeners

        /// <summary>
        /// Gets collection of property change listeners.
        /// </summary>
        internal PropertyChangeListenerList PropChangeListeners
        {
            get
            {
                if ( null == _propChangeListeners )
                    _propChangeListeners = new PropertyChangeListenerList( );

                return _propChangeListeners;
            }
        }

        #endregion // PropChangeListeners

        #region WorkingHoursIfAllocated

        internal WorkingHoursCollection WorkingHoursIfAllocated
        {
            get
            {
                
                return this.WorkingHours;
            }
        }

        #endregion // WorkingHoursIfAllocated
        
        #endregion // Internal Properties

		#endregion // Properties

        #region Methods

		#region Internal Methods

		#region GetSerializationInfo

		internal static ObjectSerializationInfo GetSerializationInfo( )
		{
			return new DependencyObjectSerializationInfo(
				typeof( DaySettings ),
				IsWorkdayProperty,
				WorkingHoursProperty
			);
		}

		#endregion // GetSerializationInfo 

		#endregion // Internal Methods

        #region Private Methods

        #region OnPropertyChangedCallback

        /// <summary>
        /// Property changed callback for settings properties.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPropertyChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
        {
            DaySettings settings = (DaySettings)sender;
            ScheduleUtilities.NotifyListenersHelper( settings, e, settings.PropChangeListeners, true, true );
        }

        #endregion // OnPropertyChangedCallback

        #endregion // Private Methods

        #endregion // Methods

        #region ISupportPropertyChangeNotifications Implementation

        void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
        {
            this.PropChangeListeners.Add( listener, useWeakReference );
        }

        void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
        {
            this.PropChangeListeners.Remove( listener );
        }

        #endregion // ISupportPropertyChangeNotifications Implementation
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