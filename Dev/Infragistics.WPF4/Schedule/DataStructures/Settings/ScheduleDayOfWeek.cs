using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;







namespace Infragistics.Controls.Schedules

{
	#region ScheduleDayOfWeek Class

	/// <summary>
	/// Used for specifying settings for a specific day of week.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// XamScheduleDataManager exposes <see cref="ScheduleSettings.DaysOfWeek"/> collection that
	/// contains seven <b>ScheduleDayOfWeek</b> instances for each of the seven days of week. You can
	/// specify settings, like the description or the working hours, for a specific week day using that
	/// collection.
	/// </para>
	/// </remarks>
	/// <seealso cref="ScheduleSettings.DaysOfWeek"/>
	public class ScheduleDayOfWeek : DependencyObject, ISupportPropertyChangeNotifications
	{
        #region Member Vars

        private PropertyChangeListenerList _propChangeListeners;

        #endregion // Member Vars

        #region Constructor

		/// <summary>
        /// Constructor. Initializes a new instance of <see cref="ScheduleDayOfWeek"/>.
        /// </summary>
		public ScheduleDayOfWeek( )
		{
		}

        #endregion // Constructor

        #region Properties

        #region Public Properties

        #region DaySettings

        /// <summary>
        /// Identifies the <see cref="DaySettings"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DaySettingsProperty = DependencyPropertyUtilities.Register(
            "DaySettings",
            typeof( DaySettings ),
            typeof( ScheduleDayOfWeek ),
            null, OnPropertyChangedCallback
        );

        /// <summary>
        /// Specifies the day settings for this day of week.
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
				typeof( ScheduleDayOfWeek ),
				DaySettingsProperty
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
            ScheduleDayOfWeek dayOfWeek = (ScheduleDayOfWeek)sender;
            ScheduleUtilities.NotifyListenersHelper( dayOfWeek, e, dayOfWeek.PropChangeListeners, true, true );
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

	#endregion // ScheduleDayOfWeek Class
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