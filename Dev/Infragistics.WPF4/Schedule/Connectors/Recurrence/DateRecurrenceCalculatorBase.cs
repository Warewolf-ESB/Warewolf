using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;







using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// Evaluates date recurrence rules contained in <see cref="DateRecurrence"/> object and
	/// generates recurrences.
	/// </summary>
	public abstract class DateRecurrenceCalculatorBase
	{
		#region Properties

		#region Public Properties

		#region FirstOccurrenceDate

		/// <summary>
		/// Gets the date-time in UTC of the first occurrence.
		/// </summary>
		public abstract DateTime? FirstOccurrenceDate
		{
			get;
		}

		#endregion // FirstOccurrenceDate

		#region LastOccurrenceDate

		/// <summary>
		/// Gets the date-time in UTC of the last occurrence.
		/// </summary>
		public abstract DateTime? LastOccurrenceDate
		{
			get;
		}

		#endregion // LastOccurrenceDate  

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region GetOccurrences

		/// <summary>
		/// Gets the occurrences within the specified date range. The date-time values of the range are in UTC.
		/// </summary>
		/// <param name="dateRange">Date range with date-time values in UTC.</param>
		/// <returns>Date-time values of occurrences that occur in the specified date range.</returns>
		public abstract IEnumerable<DateRange> GetOccurrences( DateRange dateRange );

		/// <summary>
		/// Gets the occurrences in the specified date ranges. The date-time values of the ranges are in UTC.
		/// </summary>
		/// <param name="dateRanges">Collection of date ranges with date-time values are in UTC.</param>
		/// <returns>Returns the times of occurrences that occurr in the specified date ranges.
		/// Each DateRange instance specifies the start and the end time of an occurrence.
		/// Note that the date-time values are in UTC.</returns>
		public virtual IEnumerable<DateRange> GetOccurrences( IEnumerable<DateRange> dateRanges )
		{
			

			return new CoreUtilities.AggregateEnumerable<DateRange>( from ii in dateRanges select this.GetOccurrences( ii ) );
		}

		#endregion // GetOccurrences

		#region DoesRecurOnDate

		/// <summary>
		/// Specifies whether an occurrence occurs on the same day as the specified date.
		/// </summary>
		/// <param name="date">Date value in UTC.</param>
		/// <returns>True if an occurrence occurs on the specified date. False otherwise.</returns>
		public virtual bool DoesRecurOnDate( DateTime date )
		{
			return this.GetOccurrences( new DateRange( date.Date, date.Date.AddDays( 1 ) ) ).Any( ii => ScheduleUtilities.IsSameDay( ii.Start, date ) );
		}

		#endregion // DoesRecurOnDate

		#region DoesRecurOnDateAndTime

		/// <summary>
		/// Specifies whether an occurrence occurs on the specified date-time value, within second.
		/// </summary>
		/// <param name="dateTime">Date-time value in UTC.</param>
		/// <returns>True if an occurrence occurs on the specified date-time. False otherwise.</returns>
		public virtual bool DoesRecurOnDateAndTime( DateTime dateTime )
		{
			return this.GetOccurrences( new DateRange( dateTime, dateTime ) ).Any( ii => ScheduleUtilities.IsSameDateTimeWithinSecond( ii.Start, dateTime ) );
		}

		#endregion // DoesRecurOnDateAndTime  

		#endregion // Public Methods

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