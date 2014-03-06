using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class AggregateTimeRange
	{
		#region Member Variables

		private List<DateRange> _ranges;
		private DateRange[] _combinedRanges;

		#endregion //Member Variables

		#region Constructor
		internal AggregateTimeRange()
		{
			_ranges = new List<DateRange>();
		}
		#endregion //Constructor

		#region Base class overrides
		public override string ToString()
		{
			IList<DateRange> ranges = this.CombinedRanges;

			if (ranges.Count == 1)
				return this.CombinedRanges[0].ToString();
			else if (ranges.Count == 0)
				return base.ToString();

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < ranges.Count; i++)
			{
				if (i > 0)
					sb.Append(", ");

				sb.Append(ranges[i].ToString());
			}

			return sb.ToString();
		}
		#endregion // Base class overrides

		#region Properties

		#region CombinedRanges
		public IList<DateRange> CombinedRanges
		{
			get
			{
				if (_combinedRanges == null)
					_combinedRanges = DateRange.CombineRanges(_ranges);

				return _combinedRanges;
			}
		}
		#endregion //CombinedRanges

		#region Count
		public int Count
		{
			get { return _ranges.Count; }
		}
		#endregion // Count

		#endregion //Properties

		#region Methods

		#region Add
		public void Add(DateTime date, IList<TimeRange> ranges, TimeZoneToken customToLocalToken = null)
		{
			_combinedRanges = null;

			if (null != ranges)
			{
				for (int i = 0, count = ranges.Count; i < count; i++)
				{
					TimeRange range = ranges[i];

					DateTime start = date.Add(range.Start);
					DateTime end = date.Add(range.End);

					// otherwise we need to build the daterange local to the 
					// resource and convert that back to actual local time
					//
					if (customToLocalToken != null)
					{
						start = customToLocalToken.ConvertToLocal(start);
						end = customToLocalToken.ConvertToLocal(end);
					}
					_ranges.Add(new DateRange(start, end));
				}
			}
		}

		public void Add(DateRange dateRange)
		{
			_combinedRanges = null;

			_ranges.Add(dateRange);
		}
		#endregion //Add

		#region Clone
		internal AggregateTimeRange Clone(TimeSpan offset)
		{
			AggregateTimeRange clone = new AggregateTimeRange();
			IList<DateRange> ranges = this.CombinedRanges;

			for (int i = 0, count = ranges.Count; i < count; i++)
			{
				DateRange range = ranges[i];
				range.Offset(offset);
				clone.Add(range);
			}

			return clone;
		}
		#endregion // Clone

		#region Reset
		internal void Reset()
		{
			_ranges.Clear();
			_combinedRanges = null;
		}
		#endregion // Reset

		#endregion //Methods
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