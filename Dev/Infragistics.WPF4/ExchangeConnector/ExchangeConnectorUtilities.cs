using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using Infragistics.Controls.Schedules.EWS;

namespace Infragistics.Controls.Schedules
{
	internal static class ExchangeConnectorUtilities
	{
		#region Methods

		#region AppendInt16

		public static void AppendInt16(List<byte> data, short value)
		{
			data.AddRange(BitConverter.GetBytes(value));
		}

		#endregion  // AppendInt16

		#region AppendInt32

		public static void AppendInt32(List<byte> data, int value)
		{
			data.AddRange(BitConverter.GetBytes(value));
		}

		#endregion  // AppendInt32

		#region CanOccurrencesBeInDateRange

		public static bool CanOccurrencesBeInDateRange(DateRange dateRange, List<ActivityBase> recurringActivityRoots, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			for (int i = 0; i < recurringActivityRoots.Count; i++)
			{
				ActivityBase recurringActivityRoot = recurringActivityRoots[i];

				// If the date range ends before the recurring activity, no occurrences will be in this range, so skip it.
				if (dateRange.End <= ExchangeConnectorUtilities.GetActualStartTimeUtc(recurringActivityRoot, timeZoneInfoProvider))
					continue;

				// If the date range start after the recurring activity's Until value, no occurrences will be in this range, 
				// so skip it.
				DateRecurrence dateRecurrence = recurringActivityRoot.Recurrence as DateRecurrence;
				if (dateRecurrence != null &&
					dateRecurrence.Until.HasValue &&
					dateRecurrence.Until <= dateRange.Start)
				{
					continue;
				}

				return true;
			}

			return false;
		} 

		#endregion  // CanOccurrencesBeInDateRange

		#region CombineRanges

		public static DateRange[] CombineRanges(IEnumerable<DateRange> ranges)
		{
			List<DateTimeHolder> holders = new List<DateTimeHolder>();
			foreach (DateRange range in ranges)
			{
				DateTimeHolder holder = new DateTimeHolder();
				holder.DateTime = range.Start;
				holder.Type = DateTimeType.Start;
				holders.Add(holder);

				holder = new DateTimeHolder();
				holder.DateTime = range.End;
				holder.Type = DateTimeType.End;
				holders.Add(holder);
			}

			if (holders.Count == 0)
				return new DateRange[0];

			CoreUtilities.SortMergeGeneric(holders, DateTimeHolderComparer.Instance);

			int nestedCount = 0;

			List<DateRange> combinedRanges = new List<DateRange>();
			DateRange? currentRange = null;

			for (int i = 0; i < holders.Count; i++)
			{
				DateTimeHolder holder = holders[i];

				if (holder.Type == DateTimeType.Start)
				{
					nestedCount++;

					if (currentRange.HasValue == false)
						currentRange = new DateRange(holder.DateTime);
				}
				else
				{
					nestedCount--;

					if (nestedCount == 0)
					{
						bool shouldEndCurrentRange = true;
						if (i < holders.Count - 1)
						{
							DateTimeHolder nextHolder = holders[i + 1];
							if ((nextHolder.DateTime - holder.DateTime).TotalDays <= 1)
							{
								Debug.Assert(nextHolder.Type == DateTimeType.Start, "I'm not sure how this happened.");
								shouldEndCurrentRange = false;
							}
						}

						if (shouldEndCurrentRange)
						{
							DateRange range = currentRange.Value;
							currentRange = null;

							range.End = holder.DateTime;
							combinedRanges.Add(range);
						}
					}
				}
			}

			return combinedRanges.ToArray();
		}

		#endregion  // CombineRanges

		#region CreateResourceId

		public static string CreateResourceId(string domain, string userName)
		{
			return String.Format("{0}\\{1}", domain, userName);
		} 

		#endregion  // CreateResourceId

		#region DebugFail

		public static void DebugFail(string message)
		{



			Debug.Fail(message);

		}

		#endregion  // DebugFail

		#region EnumGetValues

		public static System.Array EnumGetValues(Type enumType)
		{


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			return Enum.GetValues(enumType);

		}

		#endregion  // EnumGetValues 

		#region GetActualEndTimeUtc

		public static DateTime GetActualEndTimeUtc(ActivityBase activity, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			string timeZoneId = activity.EndTimeZoneId ?? activity.StartTimeZoneId;

			TimeZoneToken localToken;
			if (timeZoneId == null || timeZoneInfoProvider.TryGetTimeZoneToken(timeZoneId, out localToken) == false)
				localToken = timeZoneInfoProvider.LocalToken;

			return activity.GetEndUtc(localToken);
		}

		#endregion  // GetActualEndTimeUtc

		#region GetActualStartTimeUtc

		public static DateTime GetActualStartTimeUtc(ActivityBase activity, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			string timeZoneId = activity.StartTimeZoneId ?? activity.EndTimeZoneId;

			TimeZoneToken localToken;
			if (timeZoneId == null || timeZoneInfoProvider.TryGetTimeZoneToken(timeZoneId, out localToken) == false)
				localToken = timeZoneInfoProvider.LocalToken;

			return activity.GetStartUtc(localToken);
		}

		#endregion  // GetActualStartTimeUtc

		#region GetString

		internal static string GetString(string name)
		{
#pragma warning disable 436
			return SR.GetString(name);
#pragma warning restore 436
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}

		#endregion //GetString

		#region IsRecurringMaster

		public static bool IsRecurringMaster(ActivityBase activity)
		{
			return activity.IsRecurrenceRoot && (activity is Task) == false;
		}

		#endregion  // IsRecurringMaster

		#region ReadInt16

		public static short ReadInt16(byte[] data, ref int index)
		{
			short value = BitConverter.ToInt16(data, index);
			index += 2;
			return value;
		}

		#endregion  // ReadInt16

		#region ReadInt32

		public static int ReadInt32(byte[] data, ref int index)
		{
			int value = BitConverter.ToInt32(data, index);
			index += 4;
			return value;
		}

		#endregion  // ReadInt32

		#region RemoveAll

		public static void RemoveAll<T>(List<T> list, T item)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (Object.ReferenceEquals(list[i], item))
					list.RemoveAt(i);
			}
		} 

		#endregion // RemoveAll

		#region SetStartEnd

		// MD 11/4/11 - TFS75795
		// The SetStartEnd method needs the time zone for the start/end time.
		//internal static void SetStartEnd(ActivityBase activity, DateTime dateTime, TimeZoneInfoProvider provider, bool supportsTimeOfDay, bool start)
		internal static void SetStartEnd(ActivityBase activity, 
			DateTime dateTime, 
			TimeZoneToken dateTimeToken,
			TimeZoneInfoProvider provider, 
			bool supportsTimeOfDay, 
			bool start)
		{
			DateTime resolvedDateTime = dateTime;

			if (activity.IsTimeZoneNeutral)
			{
				if (resolvedDateTime.Kind == DateTimeKind.Utc)
				{
					// MD 11/4/11 - TFS75795
					// We need to convert the time to the local time for the activity, which is not necessarily the local time of the system.
					//DateTime localTime = provider.ConvertTime(provider.UtcToken, provider.LocalToken, resolvedDateTime);
					DateTime localTime = provider.ConvertTime(provider.UtcToken, dateTimeToken, resolvedDateTime);

					resolvedDateTime = DateTime.SpecifyKind(localTime, DateTimeKind.Utc);
				}
				// MD 11/4/11 - TFS75795
				// Local times are in the local time of the system, but we need the time in the local time for the activity, which may not be the same.
				else if (resolvedDateTime.Kind == DateTimeKind.Local)
				{
					DateTime localTime = provider.ConvertTime(provider.LocalToken, dateTimeToken, DateTime.SpecifyKind(resolvedDateTime, DateTimeKind.Unspecified));
					resolvedDateTime = DateTime.SpecifyKind(localTime, DateTimeKind.Utc);
				}
			}
			else
			{
				if (resolvedDateTime.Kind == DateTimeKind.Local)
					resolvedDateTime = provider.ConvertTime(provider.LocalToken, provider.UtcToken, DateTime.SpecifyKind(resolvedDateTime, DateTimeKind.Unspecified));
			}

			
			//if (supportsTimeOfDay == false)
			//    resolvedDateTime = resolvedDateTime.Date;

			// The Local kind is only used for the local time on the system. Since we are going to look at the start time zone or end time zone 
			// on the activity, we should use an Unspecified kind.
			if (resolvedDateTime.Kind == DateTimeKind.Local)
				resolvedDateTime = DateTime.SpecifyKind(resolvedDateTime, DateTimeKind.Unspecified);

			if (start)
				activity.Start = resolvedDateTime;
			else
				activity.End = resolvedDateTime;
		} 

		#endregion  // SetStartEnd

		#region ShouldDeleteAllTaskOccurrences

		public static bool ShouldDeleteAllTaskOccurrences(ActivityBase activity)
		{
			return activity.IsRecurrenceRoot || activity.RootActivity == null;
		} 

		#endregion  // ShouldDeleteAllTaskOccurrences

		#endregion  // Methods


		#region DateTimeHolder struct

		private struct DateTimeHolder
		{
			public DateTime DateTime;
			public DateTimeType Type;
		} 

		#endregion  // DateTimeHolder struct

		#region DateTimeHolderComparer class

		private class DateTimeHolderComparer : IComparer<DateTimeHolder>
		{
			public static readonly DateTimeHolderComparer Instance = new DateTimeHolderComparer();

			private DateTimeHolderComparer() { }

			#region IComparer<DateTimeHolder> Members

			int IComparer<DateTimeHolder>.Compare(DateTimeHolder x, DateTimeHolder y)
			{
				return x.DateTime.CompareTo(y.DateTime);
			}

			#endregion
		} 

		#endregion  // DateTimeHolderComparer class

		#region DateTimeType enum

		private enum DateTimeType : byte
		{
			Start,
			End
		} 

		#endregion  // DateTimeType enum
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