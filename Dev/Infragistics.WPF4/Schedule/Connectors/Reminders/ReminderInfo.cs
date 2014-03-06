using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Linq;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// Contains information regarding an invoked reminder.
	/// </summary>
	public class ReminderInfo : PropertyChangeNotifierExtended
	{
		#region Member Vars

		private Reminder _reminder;
		private object _context;
		private bool _isDismissed; 

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <seealso cref="ReminderInfo"/> object.
		/// </summary>
		/// <param name="reminder">Associated reminder object if any.</param>
		/// <param name="context">Object associated with the reminder.</param>
		public ReminderInfo( Reminder reminder, object context )
		{
			this.Context = context;
			this.Reminder = reminder;
		} 

		#endregion // Constructor

		#region Base Overrides

		#region OnSubObjectPropertyChanged

		internal override void OnSubObjectPropertyChanged( object sender, string property, object extraInfo )
		{
			base.OnSubObjectPropertyChanged( sender, property, extraInfo );

			if ( sender == _reminder )
			{
				// Re-raise the listener notification when reminder subobject changes so the listener
				// knows when the reminder of a reminder info is changed and has the context of the 
				// reminder info.
				// 
				this.NotifyListeners( this, property, _reminder );
			}
		}

		#endregion // OnSubObjectPropertyChanged 

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region Context

		/// <summary>
		/// Gets the object if any associated with the reminder.
		/// </summary>
		public object Context
		{
			get
			{
				return _context;
			}
			set
			{
				if ( _context != value )
				{
					ScheduleUtilities.ManageListenerHelperObj( ref _context, value, this, true );

					this.RaisePropertyChangedEvent( "Context" );
				}
			}
		}

		#endregion // Context

		#region IsDismissed

		/// <summary>
		/// Indicates if the reminder has been dismissed.
		/// </summary>
		public bool IsDismissed
		{
			get
			{
				return _isDismissed;
			}
			internal set
			{
				if ( _isDismissed != value )
				{
					_isDismissed = value;
					this.RaisePropertyChangedEvent( "IsDismissed" );
				}
			}
		}

		#endregion // IsDismissed

		#region IsSnoozed

		/// <summary>
		/// Indicates if the reminder has been snoozed.
		/// </summary>
		public bool IsSnoozed
		{
			get
			{
				return ! _isDismissed && null != _reminder && _reminder.IsSnoozed;
			}
		}

		#endregion // IsSnoozed

		#region Reminder

		/// <summary>
		/// Gets the associated <see cref="Reminder"/> object if any.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that if the <b>Reminder</b> property is null then it will be set to a new <see cref="Reminder"/> object
		/// when <see cref="Snooze"/> method is called.
		/// </para>
		/// </remarks>
		/// <seealso cref="Dismiss"/>
		/// <seealso cref="Snooze"/>
		public Reminder Reminder
		{
			get
			{
				return _reminder;
			}
			set
			{
				if ( _reminder != value )
				{
					ScheduleUtilities.ManageListenerHelper( ref _reminder, value, this, true );

					this.RaisePropertyChangedEvent( "Reminder" );
				}
			}
		}

		#endregion // Reminder

		#endregion // Public Properties 

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Dismiss

		/// <summary>
		/// Dismisses the reminder.
		/// </summary>
		/// <remarks>
		/// <b>Dismiss</b> is called when the user acknowledges the reminder and no longer wants the reminder
		/// to be displayed.
		/// </remarks>
		/// <seealso cref="Snooze"/>
		public void Dismiss( )
		{
			this.IsDismissed = true;
		} 

		#endregion // Dismiss

		#region Snooze

		/// <summary>
		/// Snoozes the reminder.
		/// </summary>
		/// <param name="snoozeTime">When the reminder was snoozed. This date-time value is in UTC.</param>
		/// <param name="snoozeInterval">The snooze interval after which to re-display the reminder.</param>
		/// <remarks>
		/// <para class="body">
		/// <b>Snooze</b> method is called when the user wants to be re-reminded after a certain interval of time.
		/// </para>
		/// </remarks>
		/// <seealso cref="Dismiss"/>
		public void Snooze( DateTime snoozeTime, TimeSpan snoozeInterval )
		{
			this.EnsureReminderAllocated( );

			Reminder reminder = this.Reminder;
			reminder.SnoozeInterval = snoozeInterval;
			reminder.LastSnoozeTime = snoozeTime;
			reminder.IsSnoozed = true;
		}

		#endregion // Snooze

		#endregion // Public Methods

		#region Private Methods

		#region EnsureReminderAllocated

		/// <summary>
		/// Allocates a reminder if it hasn't been allocated.
		/// </summary>
		internal void EnsureReminderAllocated( )
		{
			if ( null == this.Reminder )
				this.Reminder = new Reminder( );
		}

		#endregion // EnsureReminderAllocated 

		#endregion // Private Methods

		#endregion // Methods
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