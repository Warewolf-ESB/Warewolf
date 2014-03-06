using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;


using Infragistics.Services;
using Infragistics.Controls.Schedules.Primitives.Services;

namespace Infragistics.Controls.Schedules.Services





{
	internal class DefaultDateRecurrenceCalculator : DateRecurrenceCalculatorBase
	{
		#region Member Vars

		private RecurrenceInfo _info;
		private DateRecurrenceCache _cache; 

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="info"></param>
		public DefaultDateRecurrenceCalculator( RecurrenceInfo info )
		{
			_info = info;
			_cache = new DateRecurrenceCache( info );
		} 

		#endregion // Constructor

		private static DateTime? ToNullIfDefault( DateTime date )
		{
			return default( DateTime ) != date ? date : default( DateTime? );
		}
		
		public override DateTime? FirstOccurrenceDate
		{
			get
			{
				// SSP 4/11/11 TFS66178
				// 
				//DateTime dt = _cache.Generate( new DateRange( _info.StartDateTime, DateTime.MaxValue ), 1 ).FirstOrDefault( );
				DateTime dt = _cache.GetFirstOccurrenceDate( new DateRange( _info.StartDateTime, DateTime.MaxValue ) );

				return ToNullIfDefault( dt );
			}
		}

		public override DateTime? LastOccurrenceDate
		{
			get
			{
				DateTime dt = _cache.Generate( new DateRange( _info.StartDateTime, DateTime.MaxValue ) ).LastOrDefault( );
				return ToNullIfDefault( dt );
			}
		}

		public override IEnumerable<DateRange> GetOccurrences( DateRange dateRange )
		{
			

			TimeZoneToken timeZone = _info.TimeZone;
			TimeSpan duration = _info.OccurrenceDuration;
			
			// Make sure the duration is not negative.
			// 
			if ( duration < TimeSpan.Zero )
				duration = TimeSpan.Zero;

			// DateRange parameter is in UTC so convert it to local date-time.
			// 
			DateRange localDateRange = ScheduleUtilities.ConvertFromUtc( timeZone, dateRange );

			return from ii in _cache.Generate( localDateRange ) 
				   let iiUtc = null != timeZone ? timeZone.ConvertToUtc( ii ) : ii 
				   select new DateRange( iiUtc, iiUtc.Add( duration ) );
		}
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