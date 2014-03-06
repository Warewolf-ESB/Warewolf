using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


#pragma warning disable 1574
using Infragistics.Services;
using Infragistics.Controls.Schedules.Primitives.Services;

namespace Infragistics.Controls.Schedules.Services





{
	/// <summary>
	/// Used for specifying settings for a specific date or a recurring date.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// If you need to specify some settings for a specific day in the year, or some recurring date
	/// then you can use the <see cref="ScheduleSettings"/>' <see cref="ScheduleSettings.DaySettingsOverrides"/>
	/// collection to add a <b>DaySettingsOverride</b> instance that contains the day settings for that day
	/// or recurring date.
	/// </para>
	/// <para class="body">
	/// You can also specify DaySettingsOverrides for a specific resource using Resource' 
	/// <see cref="Resource.DaySettingsOverrides"/> collection.
	/// </para>
	/// </remarks>
	/// <seealso cref="ScheduleSettings.DaySettingsOverrides"/>
	/// <seealso cref="Resource.DaySettingsOverrides"/>
	/// <seealso cref="XamScheduleDataManager.Settings"/>
	public class DaySettingsOverride : DependencyObject, ISupportPropertyChangeNotifications
	{
        #region Member Vars

        private PropertyChangeListenerList _propChangeListeners;

        #endregion // Member Vars

        #region Properties

        #region Public Properties

        #region Date

        /// <summary>
        /// Identifies the <see cref="Date"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DateProperty = DependencyPropertyUtilities.Register(
            "Date",
            typeof( DateTime ),
            typeof( DaySettingsOverride ),
            DependencyPropertyUtilities.CreateMetadata( default( DateTime ), OnPropertyChangedCallback )
        );

        /// <summary>
        /// Specifies the date. The date is always considered to be relative to the local time-zone.
        /// </summary>



		public DateTime Date
        {
            get
            {
                return (DateTime)this.GetValue( DateProperty );
            }
            set
            {
                this.SetValue( DateProperty, value );
            }
        }

        #endregion // Date

        #region DaySettings

        /// <summary>
        /// Identifies the <see cref="DaySettings"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DaySettingsProperty = DependencyPropertyUtilities.Register(
            "DaySettings",
            typeof( DaySettings ),
            typeof( DaySettingsOverride ),
            null, OnPropertyChangedCallback
        );

        /// <summary>
        /// Specifies the DaySettings object.
        /// </summary>
        public DaySettings DaySettings
        {
            get
            {
                return (DaySettings)this.GetValue( DaySettingsProperty );
            }
            set
            {
                this.SetValue( DaySettingsProperty, value );
            }
        }

        #endregion // DaySettings

        #region Recurrence

        /// <summary>
        /// Identifies the <see cref="Recurrence"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RecurrenceProperty = DependencyPropertyUtilities.Register(
            "Recurrence",
            typeof( DateRecurrence ),
            typeof( DaySettingsOverride ),
            DependencyPropertyUtilities.CreateMetadata( null, OnPropertyChangedCallback )
        );

        /// <summary>
        /// Specifies the date recurrence. If Recurrence is specified then <see cref="Date"/> property's value
		/// is used as the start time for the recurrence.
        /// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that if the date of the first instance of the recurrence is different than the 
		/// <i>Date</i> property's value, the <i>Date</i> property is also used for matching.
		/// </para>
		/// </remarks>
        public DateRecurrence Recurrence
        {
            get
            {
                return (DateRecurrence)this.GetValue( RecurrenceProperty );
            }
            set
            {
                this.SetValue( RecurrenceProperty, value );
            }
        }

        #endregion // Recurrence

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

        #endregion // Internal Properties 

        #endregion // Properties

        #region Methods

        #region Internal Methods

		#region GetSerializationInfo

		internal static ObjectSerializationInfo GetSerializationInfo( )
		{
			return new DependencyObjectSerializationInfo(
				typeof( DaySettingsOverride ),
				DateProperty,
				DaySettingsProperty,
				RecurrenceProperty
			);
		}

		#endregion // GetSerializationInfo 



#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)


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
            DaySettingsOverride dso = (DaySettingsOverride)sender;
            ScheduleUtilities.NotifyListenersHelper( dso, e, dso.PropChangeListeners, true, true );
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