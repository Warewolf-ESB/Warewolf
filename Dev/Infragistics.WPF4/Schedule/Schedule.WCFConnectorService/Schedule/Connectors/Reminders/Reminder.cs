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


using Infragistics.Services;
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services





{
	#region Reminder Class

	/// <summary>
	/// Contains information regarding the reminder that's displayed when an activity is due.
	/// </summary>
	public class Reminder : PropertyChangeNotifierExtended
	{
		#region Member Vars

		private string _text;
		private string _action;
		private bool _isSnoozed;
		private TimeSpan _snoozeInterval;
		private DateTime _lastSnoozeTime;
		private DateTime _lastDisplayedTime;

		#endregion // Member Vars

		#region Properties

		#region Public Properties

		#region UserData

		/// <summary>
		/// Specifies user data associated with this reminder.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>UserData</b> property's value is not used by the XamSchedule in any way. It's meant
		/// to allow you to store any piece of data as part of the reminder object so that you can 
		/// retrieve it later, for example when the reminder is activated. As an example, 
		/// <i>UserData</i> can be a string that represents an action to take, for example a command
		/// to execute when the reminder is activated.
		/// </para>
		/// </remarks>
		public string UserData
		{
			get
			{
				return _action;
			}
			set
			{
				if ( _action != value )
				{
					_action = value;
					this.RaisePropertyChangedEvent( "UserData" );
				}
			}
		}

		#endregion // UserData

		#region IsSnoozed

		/// <summary>
		/// Indicates whether the activity is snoozed.
		/// </summary>
		public bool IsSnoozed
		{
			get
			{
				return _isSnoozed;
			}
			set
			{
				if ( _isSnoozed != value )
				{
					_isSnoozed = value;
					this.RaisePropertyChangedEvent( "IsSnoozed" );
				}
			}
		}

		#endregion // IsSnoozed

		#region LastDisplayedTime

		/// <summary>
		/// Indicates the time that the reminder was last displayed.
		/// </summary>
		public DateTime LastDisplayedTime
		{
			get
			{
				return _lastDisplayedTime;
			}
			set
			{
				if ( _lastDisplayedTime != value )
				{
					_lastDisplayedTime = value;
					this.RaisePropertyChangedEvent( "LastDisplayedTime" );
				}
			}
		}

		#endregion // LastDisplayedTime

		#region LastSnoozeTime

		/// <summary>
		/// Indicates the time that the reminder was last snoozed.
		/// </summary>
		public DateTime LastSnoozeTime
		{
			get
			{
				return _lastSnoozeTime;
			}
			set
			{
				if ( _lastSnoozeTime != value )
				{
					_lastSnoozeTime = value;
					this.RaisePropertyChangedEvent( "LastSnoozeTime" );
				}
			}
		}

		#endregion // LastSnoozeTime

		#region SnoozeInterval

		/// <summary>
		/// Indicates the interval after the <see cref="LastSnoozeTime"/> that the reminder will be re-displayed.
		/// </summary>
		public TimeSpan SnoozeInterval
		{
			get
			{
				return _snoozeInterval;
			}
			set
			{
				if ( _snoozeInterval != value )
				{
					_snoozeInterval = value;
					this.RaisePropertyChangedEvent( "SnoozeInterval" );
				}
			}
		}

		#endregion // SnoozeInterval

		#region Text

		/// <summary>
		/// Specifies the text that will be displayed in the reminder.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Text</b> property specifies the text that will be displayed in the reminder.
		/// If <i>Text</i> is note set then a default text will be used in the reminder.
		/// </para>
		/// </remarks>
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if ( _text != value )
				{
					_text = value;
					this.RaisePropertyChangedEvent( "Text" );
				}
			}
		}

		#endregion // Text

		#endregion // Public Properties

		#endregion // Properties
	} 

	#endregion // Reminder Class
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