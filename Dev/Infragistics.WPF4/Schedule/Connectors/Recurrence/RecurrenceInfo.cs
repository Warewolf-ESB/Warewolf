using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;







using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// Used to pass along recurrence information to the <see cref="RecurrenceCalculatorFactoryBase.GetCalculator"/> method.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>RecurrenceInfo</b> class is used to pass information to the <see cref="RecurrenceCalculatorFactoryBase.GetCalculator"/> method.
	/// </para>
	/// </remarks>
	/// <seealso cref="DateRecurrence"/>
	public class RecurrenceInfo
	{
		#region Member Vars

		private RecurrenceBase _recurrence;
		private DateTime _startDateTime;
		private TimeSpan _occurrenceDuration;
		private TimeZoneToken _timeZone;
		private object _context;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="recurrence">Recurrence object.</param>
		/// <param name="startDateTime">Start of the recurrence. This date-time value is in local time relative to the specified time zone.</param>
		/// <param name="occurrenceDuration">Each occurrence duration.</param>
		/// <param name="timeZone">Identifies the time-zone. 'StartDateTime' parameter's value is relative to this time zone.</param>
		/// <param name="context">Optional context object.</param>
		public RecurrenceInfo(
			RecurrenceBase recurrence,
			DateTime startDateTime,
			TimeSpan occurrenceDuration,
			TimeZoneToken timeZone,
			object context = null )
		{
			CoreUtilities.ValidateNotNull( recurrence );

			_recurrence = recurrence;
			_startDateTime = startDateTime;
			_occurrenceDuration = occurrenceDuration;
			_timeZone = timeZone;
			_context = context;
		} 

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Context

		/// <summary>
		/// Optional context object. Can be an <see cref="ActivityBase"/> instance for example.
		/// </summary>
		public object Context
		{
			get
			{
				return _context;
			}
		}

		#endregion // Context

		#region OccurrenceDuration

		/// <summary>
		/// Duration of each occurrence.
		/// </summary>
		public TimeSpan OccurrenceDuration
		{
			get
			{
				return _occurrenceDuration;
			}
		}

		#endregion // OccurrenceDuration

		#region Recurrence

		/// <summary>
		/// Gets the recurrence object.
		/// </summary>
		public RecurrenceBase Recurrence
		{
			get
			{
				return _recurrence;
			}
		}

		#endregion // Recurrence

		#region StartDateTime

		/// <summary>
		/// Gets the start date-time of the recurrence. All the recurrence instances must occur on
		/// or after this start date-time. The value is relative to the time zone specified by the
		/// <see cref="TimeZone"/> property.
		/// </summary>
		/// <seealso cref="TimeZone"/>
		public DateTime StartDateTime
		{
			get
			{
				return _startDateTime;
			}
		}

		#endregion // StartDateTime

		#region TimeZone

		/// <summary>
		/// Time zone associated with the <see cref="StartDateTime"/> value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Recurrences need to be generated using this time-zone.
		/// </para>
		/// </remarks>
		public TimeZoneToken TimeZone
		{
			get
			{
				return _timeZone;
			}
		}

		#endregion // TimeZone

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region Clone

		internal RecurrenceInfo Clone( )
		{
			RecurrenceInfo clone = (RecurrenceInfo)this.MemberwiseClone( );
			clone._recurrence = _recurrence.Clone( );

			return clone;
		}

		#endregion // Clone

		#endregion // Internal Methods 

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