using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace Infragistics.Controls.Schedules.Services



{
	internal struct TimeslotRange
		: IEquatable<TimeslotRange>
	{
		#region Member Variables

		public DateTime StartDate;
		public DateTime EndDate;
		public TimeSpan TimeslotInterval;
		public int TimeslotCount;

		#endregion //Member Variables

		#region Constructor
		public TimeslotRange(DateTime start, DateTime end, TimeSpan timeslotInterval)
		{
			this.StartDate = start;
			this.EndDate = end;
			this.TimeslotInterval = timeslotInterval;

			TimeSpan duration = end.Subtract(start);
			long tsCount = (duration.Ticks - 1) / timeslotInterval.Ticks + 1;

			Debug.Assert(tsCount <= int.MaxValue);

			this.TimeslotCount = (int)tsCount;
		}
		#endregion //Constructor

		#region Base class overrides
		public override bool Equals(object obj)
		{
			if (obj is TimeslotRange == false)
				return false;

			return this.Equals((TimeslotRange)obj);
		}

		public override int GetHashCode()
		{
			return StartDate.GetHashCode() | EndDate.GetHashCode() | TimeslotCount.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0}-{1} TimeslotCount={2}", StartDate, EndDate, TimeslotCount);
		}
		#endregion //Base class overrides

		#region IEquatable<TimeslotRange> Members

		public bool Equals(TimeslotRange other)
		{
			return other.EndDate == this.EndDate &&
				other.StartDate == this.StartDate &&
				other.TimeslotInterval == this.TimeslotInterval;
		}

		#endregion //IEquatable<TimeslotRange> Members
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