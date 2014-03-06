using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;
using System.Diagnostics;
using System.Windows.Media;

namespace Infragistics.Controls.Schedules
{
	#region ActivitySettings Class

	/// <summary>
	/// Contains settings information regarding the operations that the user can perform on activities.
	/// </summary>
	public class ActivitySettings : DependencyObject, ISupportPropertyChangeNotifications
	{
		#region Member Vars

		private PropertyChangeListenerList _propChangeListeners = new PropertyChangeListenerList( );

		#endregion // Member Vars

		#region Properties

		#region Public Properties

		#region AllowAdd

		/// <summary>
		/// Identifies the <see cref="AllowAdd"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowAddProperty = DependencyPropertyUtilities.Register(
			"AllowAdd",
			typeof( bool? ),
			typeof( ActivitySettings ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnPropertyChangedCallback ) )
		);

		/// <summary>
		/// Specifies whether the user is allowed to add new activities.
		/// </summary>
		/// <seealso cref="AllowAddProperty"/>
		public bool? AllowAdd
		{
			get
			{
				return (bool?)this.GetValue( AllowAddProperty );
			}
			set
			{
				this.SetValue( AllowAddProperty, value );
			}
		}

		#endregion // AllowAdd

		#region AllowEdit

		/// <summary>
		/// Identifies the <see cref="AllowEdit"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowEditProperty = DependencyPropertyUtilities.Register(
			"AllowEdit",
			typeof( bool? ),
			typeof( ActivitySettings ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnPropertyChangedCallback ) )
		);

		/// <summary>
		/// Specifies whether the user is allowed to modify existing activities.
		/// </summary>
		/// <seealso cref="AllowEditProperty"/>
		public bool? AllowEdit
		{
			get
			{
				return (bool?)this.GetValue( AllowEditProperty );
			}
			set
			{
				this.SetValue( AllowEditProperty, value );
			}
		}

		#endregion // AllowEdit

		#region AllowDragging

		/// <summary>
		/// Identifies the <see cref="AllowDragging"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowDraggingProperty = DependencyPropertyUtilities.Register(
			"AllowDragging",
			typeof( AllowActivityDragging? ),
			typeof( ActivitySettings ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnPropertyChangedCallback ) )
		);

		/// <summary>
		/// Specifies whether the user is allowed to drag activities.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The user can drag and drop an activity to change its time or make a copy of the activity, depending
		/// upon whether the Ctrl key is held down when the activity is dropped. Holding the Ctrl key down makes
		/// the copy of the activity being dragged. Otherwise the activity's time is changed to occupy the target
		/// time slot.
		/// </para>
		/// <para class="body">
		/// Activity can also be dragged to a different calendar of the same resource or a calendar of a 
		/// different resource. Doing so not only changes time, but also changes the OwningCalendarId and/or
		/// OwningResourceId of the activity. Whether the user is allowed to drag accross calendars or resources
		/// is controlled by the <see cref="Infragistics.Controls.Schedules.AllowActivityDragging"/> 
		/// enum value that's set on the property. However <b>note</b> that the underlying schedule data connector
		/// may impose limitations on whether an activity's resource calendar or resource can be changed or not, in
		/// which case regardless of what this property is set to, the underlying schedule data connector restriction 
		/// will be effective.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Controls.Schedules.AllowActivityDragging"/>
		/// <seealso cref="AllowDraggingProperty"/>
		public AllowActivityDragging? AllowDragging
		{
			get
			{
				return (AllowActivityDragging?)this.GetValue( AllowDraggingProperty );
			}
			set
			{
				this.SetValue( AllowDraggingProperty, value );
			}
		}

		#endregion // AllowDragging

		#region AllowRecurring

		/// <summary>
		/// Identifies the <see cref="AllowRecurring"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowRecurringProperty = DependencyPropertyUtilities.Register(
			"AllowRecurring",
			typeof( bool? ),
			typeof( ActivitySettings ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnPropertyChangedCallback ) )
		);

		/// <summary>
		/// Specifies whether the user is allowed to add recurring activities.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that this property doesn't prevent existing recurring activities from being displayed. 
		/// It simply disallows the user from creating recurring activities through the UI.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityBase.Recurrence"/>
		/// <seealso cref="AllowRecurringProperty"/>
		public bool? AllowRecurring
		{
			get
			{
				return (bool?)this.GetValue( AllowRecurringProperty );
			}
			set
			{
				this.SetValue( AllowRecurringProperty, value );
			}
		}

		#endregion // AllowRecurring

		#region AllowRemove

		/// <summary>
		/// Identifies the <see cref="AllowRemove"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowRemoveProperty = DependencyPropertyUtilities.Register(
			"AllowRemove",
			typeof( bool? ),
			typeof( ActivitySettings ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnPropertyChangedCallback ) )
		);

		/// <summary>
		/// Specifies whether the user is allowed to remove activities.
		/// </summary>
		/// <seealso cref="AllowRemoveProperty"/>
		public bool? AllowRemove
		{
			get
			{
				return (bool?)this.GetValue( AllowRemoveProperty );
			}
			set
			{
				this.SetValue( AllowRemoveProperty, value );
			}
		}

		#endregion // AllowRemove

		#region AllowResizing

		/// <summary>
		/// Identifies the <see cref="AllowResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowResizingProperty = DependencyPropertyUtilities.Register(
			"AllowResizing",
			typeof(AllowActivityResizing?),
			typeof(ActivitySettings),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnPropertyChangedCallback))
		);

		/// <summary>
		/// Specifies whether the user is allowed to resize activities.
		/// </summary>
		/// <seealso cref="AllowResizingProperty"/>
		public AllowActivityResizing? AllowResizing
		{
			get
			{
				return (AllowActivityResizing?)this.GetValue(AllowResizingProperty);
			}
			set
			{
				this.SetValue(AllowResizingProperty, value);
			}
		}

		#endregion // AllowResizing

		#region AllowTimeZoneNeutral

		/// <summary>
		/// Identifies the <see cref="AllowTimeZoneNeutral"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowTimeZoneNeutralProperty = DependencyPropertyUtilities.Register(
			"AllowTimeZoneNeutral",
			typeof( bool? ),
			typeof( ActivitySettings ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnPropertyChangedCallback ) )
		);

		/// <summary>
		/// Specifies whether the user is allowed to create time-zone neutral activities.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that this property doesn't prevent display of time-zone neutral activities. It simply disallows the
		/// user from creating time-zone neutral activities (appointments, tasks etc...) through the UI.
		/// </para>
		/// <para class="body">
		/// A time-zone neutral activity is an activity that occurs at the same numerical time value across all time zones.
		/// That is, let's say a time-zone neutral activity starts at 9:00 AM. It starts at 9:00 AM local time in every time-zone.
		/// Holidays are a good example of time-zone neutral activities where they start at, let's say 12:00 AM, regardless of 
		/// the time-zone.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityBase.IsTimeZoneNeutral"/>
		public bool? AllowTimeZoneNeutral
		{
			get
			{
				return (bool?)this.GetValue( AllowTimeZoneNeutralProperty );
			}
			set
			{
				this.SetValue( AllowTimeZoneNeutralProperty, value );
			}
		}

		#endregion // AllowTimeZoneNeutral

		// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		#region IsAddViaClickToAddEnabled

		/// <summary>
		/// Identifies the <see cref="IsAddViaClickToAddEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAddViaClickToAddEnabledProperty = DependencyPropertyUtilities.Register("IsAddViaClickToAddEnabled",
			typeof(bool?), typeof(ActivitySettings),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback( OnPropertyChangedCallback ))
			);

		/// <summary>
		/// Returns or sets a boolean indicating whether an activity may be created by clicking on the Click To Add prompt when hovering the mouse over a timeslot.
		/// </summary>
		/// <seealso cref="IsAddViaClickToAddEnabledProperty"/>
		public bool? IsAddViaClickToAddEnabled
		{
			get
			{
				return (bool?)this.GetValue(ActivitySettings.IsAddViaClickToAddEnabledProperty);
			}
			set
			{
				this.SetValue(ActivitySettings.IsAddViaClickToAddEnabledProperty, value);
			}
		}

		#endregion //IsAddViaClickToAddEnabled

		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		#region IsAddViaDoubleClickEnabled

		/// <summary>
		/// Identifies the <see cref="IsAddViaDoubleClickEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAddViaDoubleClickEnabledProperty = DependencyPropertyUtilities.Register("IsAddViaDoubleClickEnabled",
			typeof(bool?), typeof(ActivitySettings),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnPropertyChangedCallback))
			);

		/// <summary>
		/// Returns or sets a boolean indicating whether an activity may be created by double clicking on a timeslot or in the timeslot header area.
		/// </summary>
		/// <seealso cref="IsAddViaDoubleClickEnabledProperty"/>
		public bool? IsAddViaDoubleClickEnabled
		{
			get
			{
				return (bool?)this.GetValue(ActivitySettings.IsAddViaDoubleClickEnabledProperty);
			}
			set
			{
				this.SetValue(ActivitySettings.IsAddViaDoubleClickEnabledProperty, value);
			}
		}

		#endregion //IsAddViaDoubleClickEnabled

		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		#region IsAddViaTypingEnabled

		/// <summary>
		/// Identifies the <see cref="IsAddViaTypingEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsAddViaTypingEnabledProperty = DependencyPropertyUtilities.Register("IsAddViaTypingEnabled",
			typeof(bool?), typeof(ActivitySettings),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnPropertyChangedCallback))
			);

		/// <summary>
		/// Returns or sets a boolean indicating whether an activity may be created by typing while one or more timeslots are selected.
		/// </summary>
		/// <seealso cref="IsAddViaTypingEnabledProperty"/>
		public bool? IsAddViaTypingEnabled
		{
			get
			{
				return (bool?)this.GetValue(ActivitySettings.IsAddViaTypingEnabledProperty);
			}
			set
			{
				this.SetValue(ActivitySettings.IsAddViaTypingEnabledProperty, value);
			}
		}

		#endregion //IsAddViaTypingEnabled

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region IsActivityOperationAllowed

		internal bool IsActivityOperationAllowed( ActivityOperation operation )
		{
			switch ( operation )
			{
				case ActivityOperation.Edit:
					return this.AllowEdit ?? true;
				case ActivityOperation.Add:
					return this.AllowAdd ?? true;
				case ActivityOperation.Remove:
					return this.AllowRemove ?? true;
				default:
					Debug.Assert( false );
					break;
			}

			return false;
		}

		#endregion // IsActivityOperationAllowed

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
			ActivitySettings settings = (ActivitySettings)sender;

			ScheduleUtilities.NotifyListenersHelper( settings, e, settings._propChangeListeners, true, true );
		}

		#endregion // OnPropertyChangedCallback

		#endregion // Private Methods

		#endregion // Methods

		#region ISupportPropertyChangeNotifications Interface Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			_propChangeListeners.Add( listener, useWeakReference );
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			_propChangeListeners.Remove( listener );
		}

		#endregion // ISupportPropertyChangeNotifications Interface Implementation
	} 

	#endregion // ActivitySettings Class

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