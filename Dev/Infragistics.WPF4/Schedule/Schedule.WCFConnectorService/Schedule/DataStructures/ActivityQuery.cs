using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;


#pragma warning disable 1574
using Infragistics.Services;
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services





{
	#region ActivityQuery Class

	/// <summary>
	/// Represents an activity query.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ActivityQuery</b> class is used to specify the query criteria when using 
	/// <see cref="XamScheduleDataManager.GetActivities"/> method.
	/// </para>
	/// </remarks>
	/// <seealso cref="XamScheduleDataManager.GetActivities"/>
	public class ActivityQuery
	{
		#region Member Vars

        private ImmutableCollection<DateRange> _dateRanges;
		private ImmutableCollection<ResourceCalendar> _calendars;
		private ActivityTypes _activityTypesToQuery;
		private ActivityQueryRequestedDataFlags _requestedInformation = ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges;
		private bool _isSealed;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityQuery"/>.
		/// </summary>
		public ActivityQuery( )
		{
		}

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ActivityQuery"/>.
        /// </summary>
        /// <param name="activityTypesToQuery">Types of activities to query.</param>
        /// <param name="dateRange">Activities within this date range will be queried.</param>
        /// <param name="resources">Activities belonging to primary calendars of these resources will be queried.
        /// If null then all activities within the specified date range will be queried.</param>
        public ActivityQuery( ActivityTypes activityTypesToQuery, DateRange dateRange, IEnumerable<Resource> resources )
            : this( activityTypesToQuery, dateRange, resources, null )
        {
        }

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ActivityQuery"/>.
        /// </summary>
        /// <param name="activityTypesToQuery">Types of activities to query.</param>
        /// <param name="dateRange">Activities within this date range will be queried.</param>
        /// <param name="calendars">Activities belonging to these calendars will be queried.
        /// If null then all activities within the specified date range will be queried.</param>
        public ActivityQuery( ActivityTypes activityTypesToQuery, DateRange dateRange, IEnumerable<ResourceCalendar> calendars )
            : this( activityTypesToQuery, dateRange, calendars, null )
        {
        }

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityQuery"/>.
		/// </summary>
		/// <param name="activityTypesToQuery">Types of activities to query.</param>
		/// <param name="dateRange">Activities within this date range will be queried.</param>
		/// <param name="calendar">Activities belonging to this calendar will be queried.
		/// If null then all activities within the specified date range will be queried.</param>
		public ActivityQuery( ActivityTypes activityTypesToQuery, DateRange dateRange, ResourceCalendar calendar )
			: this( activityTypesToQuery, dateRange, calendar, null )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityQuery"/>.
		/// </summary>
		/// <param name="activityTypesToQuery">Types of activities to query.</param>
		/// <param name="dateRanges">One or more DateRange objects. Activities within these date ranges will be queried.</param>
		/// <param name="calendars">Activities belonging to these calendars will be queried.
		/// If null then all activities within the specified date ranges will be queried.</param>
		public ActivityQuery( ActivityTypes activityTypesToQuery, IEnumerable<DateRange> dateRanges, IEnumerable<ResourceCalendar> calendars )
			: this( activityTypesToQuery, dateRanges, calendars, null )
		{
		}

        private ActivityQuery( ActivityTypes activityTypesToQuery, object dateRanges, object calendars, object discard )
        {
			_dateRanges = ScheduleUtilities.ToImmutableCollection<DateRange>( dateRanges );
			
			if ( null == _dateRanges || 0 == _dateRanges.Count )
				throw new ArgumentException(ScheduleUtilities.GetString("LE_NoDataRange")); //"At least one date range must be specified."

            _calendars = ScheduleUtilities.ToCalendarsReadOnlyCollection( calendars );
            _activityTypesToQuery = activityTypesToQuery;
        }

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region ActivityTypesToQuery

		/// <summary>
		/// Specifies which activity types to query for - whether appointments, journals, tasks or a combination of these activity types.
		/// </summary>
		/// <remarks>
		/// <b>ActivityTypesToQuery</b> property specifies what type of activities to get. <b>ActivityTypes</b> enum is a flagged enum
		/// allowing you to specify a combination of activity types.
		/// </remarks>
		public ActivityTypes ActivityTypesToQuery
		{
			get
			{
				return _activityTypesToQuery;
			}
			set
			{
				this.VerifyNotSealed( );

				_activityTypesToQuery = value;
			}
		}

		#endregion // ActivityTypesToQuery

        #region Calendars

        /// <summary>
        /// Specifies the calendars for which to get the activities. If null then activities for
        /// all the calendars will be returned.
        /// </summary>
		public ImmutableCollection<ResourceCalendar> Calendars
        {
            get
            {
                return _calendars;
            }
            set
            {
				this.VerifyNotSealed( );

                _calendars = value;
            }
        }

        #endregion // Calendars

		#region DateRanges

		/// <summary>
		/// Specifies one or more date ranges.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This query will return activities occurring within these date ranges.
		/// </para>
		/// </remarks>
		public ImmutableCollection<DateRange> DateRanges
		{
			get
			{
				return _dateRanges;
			}
			set
			{
				this.VerifyNotSealed( );

				_dateRanges = value;
			}
		}

		#endregion // DateRanges

		#region RequestedInformation

		/// <summary>
		/// Specifies the information that's to be queried.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RequestedInformation</b> property specifies what information to retrieve when the query is performed.
		/// <see cref="ActivityQueryRequestedDataFlags"/> enum is a flagged enum and therefore you can specify multiple
		/// pieces of information to be retrieved.
		/// </para>
		/// </remarks>
		public ActivityQueryRequestedDataFlags RequestedInformation
		{
			get
			{
				return _requestedInformation;
			}
			set
			{
				this.VerifyNotSealed( );

				_requestedInformation = value;
			}
		}

		#endregion // RequestedInformation
		
        #endregion // Public Properties

        #endregion // Properties

		#region Methods

		#region Seal

		/// <summary>
		/// Once an activity query has been sealed, it can't be modified.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <i>ActivityQuery</i> object sealed from further modifications when the query is performed
		/// and associated with an <see cref="ActivityQueryResult"/> object. Any attempt to modify the
		/// activity query afterwards will result in an InvalidOperationException.
		/// </para>
		/// </remarks>
		public void Seal( )
		{
			_isSealed = true;
		} 

		#endregion // Seal

		#region Private Methods

		#region VerifyNotSealed

		private void VerifyNotSealed( )
		{
			if ( _isSealed )
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_QuerySealed")); // "An ActivityQuery cannot be modified after it has been executed."
		}

		#endregion // VerifyNotSealed

		#endregion // Private Methods 

		#endregion // Methods
    }

	#endregion // ActivityQuery Class
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