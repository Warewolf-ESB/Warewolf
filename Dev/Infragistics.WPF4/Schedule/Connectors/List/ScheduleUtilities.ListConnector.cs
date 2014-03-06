using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Collections.Specialized;







using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	partial class ScheduleUtilities : CoreUtilities
	{
		#region Nested Data Structures

		#region KeyedCollectionImpl Class

		/// <summary>
		/// Keyed collection that uses a delegate for retrieving key value from item.
		/// </summary>
		/// <typeparam name="TKey">Type of key.</typeparam>
		/// <typeparam name="TItem">Type of item.</typeparam>
		internal class KeyedCollectionImpl<TKey, TItem> : KeyedCollection<TKey, TItem>
		{
			private Converter<TItem, TKey> _itemKeyGetter;

			internal KeyedCollectionImpl(Converter<TItem, TKey> itemKeyGetter)
			{
				ScheduleUtilities.ValidateNotNull(itemKeyGetter);
				_itemKeyGetter = itemKeyGetter;
			}

			protected override TKey GetKeyForItem(TItem item)
			{
				return _itemKeyGetter(item);
			}
		}

		#endregion // KeyedCollectionImpl Class

		#endregion // Nested Data Structures

		#region Constructor

		// There should be a private ctor since this is a static class.
		//
		private ScheduleUtilities()
		{
		}

		#endregion //Constructor

		#region Properties

		#region ParseCulture

		internal static CultureInfo ParseCulture
		{
			get
			{
				return CultureInfo.InvariantCulture;
			}
		}

		#endregion // ParseCulture

		#endregion // Properties

		#region Methods

		#region AddAllItems

		internal static void AddAllItems(IList destList, IEnumerable itemsToAdd)
		{
			if (null != itemsToAdd)
			{
				foreach (object ii in itemsToAdd)
					destList.Add(ii);
			}
		}

		#endregion // AddAllItems

		#region AddEntryHelper<TKey, TValue>

		internal static Dictionary<TKey, TValue> AddEntryHelper<TKey, TValue>( Dictionary<TKey, TValue> map, TKey key, TValue value )
		{
			if ( null != value )
			{
				if ( null == map )
					map = new Dictionary<TKey, TValue>( );

				map[key] = value;
			}

			return map;
		}

		#endregion // AddEntryHelper<TKey, TValue>

		#region AddListenerHelper

		// SSP 1/6/11 - NAS11.1 Activity Categories
		// 

		internal static void AddListenerHelper( object notifierObj, IPropertyChangeListener listener, bool useWeakReference )
		{
			ISupportPropertyChangeNotifications notifier = notifierObj as ISupportPropertyChangeNotifications;
			if ( null != notifier && null != listener )
				notifier.AddListener( listener, useWeakReference );
		} 

		#endregion // AddListenerHelper

		#region Aggregate

		internal static IEnumerable<T> Aggregate<T>(params IEnumerable<T>[] lists)
		{
			return Aggregate(false, lists);
		}

		internal static IEnumerable<T> Aggregate<T>(bool removeDuplicates, IEnumerable<IEnumerable<T>> lists)
		{
			IEnumerable<T> ret = new AggregateEnumerable<T>(lists);

			if (removeDuplicates)
				return new HashSet<T>(ret);

			return ret;
		}

		#endregion // Aggregate

		#region AppendList

		internal static StringBuilder AppendList<T>(StringBuilder sb, IEnumerable<T> items, string separator, Converter<T, string> converter = null)
		{
			if (null == converter)
				converter = ii => ii.ToString();

			int origLen = sb.Length;

			return items.Aggregate(sb,
				(iiSb, ii) =>
				{
					if (iiSb.Length > origLen)
						iiSb.Append(separator);

					return iiSb.Append(converter(ii));
				}
			);
		}

		#endregion // AppendList

		#region BuildList

		internal static string BuildList<T>(IEnumerable<T> items, string separator, Converter<T, string> converter = null)
		{
			return AppendList(new StringBuilder(), items, separator, converter).ToString();
		}

		#endregion // BuildList

		#region ChangePropValueHelper<T>

		internal static void ChangePropValueHelper<T>(ref T memberVar, T newVal, Action<string> raisePropChangeDelegate, string propName)
		{
			if (!EqualityComparer<T>.Default.Equals(memberVar, newVal))
			{
				memberVar = newVal;

				if (null != raisePropChangeDelegate)
					raisePropChangeDelegate(propName);
			}
		}

		#endregion // ChangePropValueHelper<T>

		#region CombineHashCodes

		internal static int CombineHashCodes<T>(IEnumerable<T> items, IEqualityComparer<T> comparer = null)
		{
			int h = 0;

			if (null != items)
			{
				if (null == comparer)
					comparer = EqualityComparer<T>.Default;

				foreach (T ii in items)
					h ^= comparer.GetHashCode(ii);
			}

			return h;
		}

		#endregion // CombineHashCodes

		#region ConvertFromUtc
		internal static DateRange ConvertFromUtc(TimeZoneToken token, DateRange range)
		{
			return ConvertFromUtc(token, range.Start, range.End);
		}

		internal static IEnumerable<DateRange> ConvertFromUtc(TimeZoneToken token, IEnumerable<DateRange> ranges)
		{
			return from ii in ranges select ConvertFromUtc(token, ii);
		}

		internal static DateRange ConvertFromUtc(TimeZoneToken token, DateTime start, DateTime end)
		{
			return new DateRange(ConvertFromUtc(token, start), ConvertFromUtc(token, end));
		}

		internal static DateTime ConvertFromUtc(TimeZoneToken token, DateTime dateTime)
		{
			if (token == null)
				return dateTime;

			return token.ConvertFromUtc(dateTime);
		}

		internal static DateRange ConvertFromUtc(TimeZoneToken token, ActivityBase activity)
		{
			var range = new DateRange(activity.GetStartLocal(token), activity.GetEndLocal(token));

			if ( range.End < range.Start )
			{
				// the start and end can both be in the same ambiguous range where one is with dst and 
				// one is without. in that case the end can be before the start. to get around this 
				// we will represent the activity honoring its duration
				//
				Debug.Assert(token.Provider.IsAmbiguousTime(token, range.Start) && token.Provider.IsAmbiguousTime(token, range.End), "The only time that a valid activity range should have the end before the start is if the start and end are within the ambiguous times and one is DST and the other is post-DST");
				range.End = range.Start.Add(activity.End.Subtract(activity.Start));
			}

			return range;
		}
		#endregion // ConvertFromUtc

		#region ConvertToUtc

		internal static DateRange[] ConvertToUtc(TimeZoneToken token, TimeslotRange[] ranges)
		{
			DateRange[] dateRanges = new DateRange[ranges.Length];

			for (int i = 0; i < ranges.Length; i++)
			{
				dateRanges[i] = ConvertToUtc(token, ranges[i]);
			}

			return dateRanges;
		}

		internal static DateRange ConvertToUtc(TimeZoneToken token, TimeslotRange range)
		{
			return ConvertToUtc(token, range.StartDate, range.EndDate);
		}

		internal static DateRange ConvertToUtc(TimeZoneToken token, DateRange range)
		{
			return ConvertToUtc(token, range.Start, range.End);
		}

		internal static DateRange ConvertToUtc(TimeZoneToken token, DateTime start, DateTime end)
		{
			return new DateRange(ConvertToUtc(token, start), ConvertToUtc(token, end));
		}

		internal static DateTime ConvertToUtc(TimeZoneToken token, DateTime dateTime)
		{
			if (token == null)
				return dateTime;

			return token.ConvertToUtc(dateTime);
		}
		#endregion // ConvertToUtc

		#region Create...FromId

		internal static DataErrorInfo CreateBlockingFromId(object context, string stringId, params object[] args)
		{
			return DataErrorInfo.CreateBlocking(context, ScheduleUtilities.GetString(stringId, args));
		}
		// JJD 4/4/11 - TFS69535 
		// Added overload with isLocalTZTokenError parameter
		internal static DataErrorInfo CreateBlockingFromId(object context, string stringId, bool isLocalTZTokenError, params object[] args)
		{
			return DataErrorInfo.CreateBlocking(context, ScheduleUtilities.GetString(stringId, args), isLocalTZTokenError);
		}
		internal static DataErrorInfo CreateDiagnosticFromId(object context, string stringId, params object[] args)
		{
			return DataErrorInfo.CreateDiagnostic(context, ScheduleUtilities.GetString(stringId, args));
		}
		internal static DataErrorInfo CreateErrorFromId(object context, string stringId, params object[] args)
		{
			return DataErrorInfo.CreateError(context, ScheduleUtilities.GetString(stringId, args));
		}

		internal static DataErrorInfo CreateWarningFromId(object context, string stringId, params object[] args)
		{
			return DataErrorInfo.CreateWarning(context, ScheduleUtilities.GetString(stringId, args));
		}

		#endregion //Create...FromId	

		#region ExpandDateRange

		internal static DateRange ExpandDateRange(DateRange range, TimeSpan leftDelta, TimeSpan rightDelta)
		{
			return new DateRange(range.Start - leftDelta, range.End + rightDelta);
		}

		#endregion // ExpandDateRange

		#region ExpandDateRanges

		internal static IEnumerable<DateRange> ExpandDateRanges(IEnumerable<DateRange> ranges, TimeSpan leftDelta, TimeSpan rightDelta)
		{
			return from ii in ranges select ExpandDateRange(ii, leftDelta, rightDelta);
		}

		#endregion // ExpandDateRanges

		#region GetActivityEndUTC

		internal static DateTime GetActivityEndUTC(ActivityBase activity, TimeZoneToken tz)
		{
			return GetActivityStartEnd(activity, tz, false, true);
		}

		#endregion // GetActivityEndUTC

		#region GetActivityStartEnd

		internal static DateTime GetActivityStartEnd(ActivityBase activity, TimeZoneToken timeZone, bool start, bool inUTC)
		{
			ValidateNotNull(timeZone, "timeZone");

			DateTime date = start ? activity.Start : activity.End;

			if (activity.IsTimeZoneNeutral)
			{
				// Time-zone neutral activities's Start and End are floating times and therefore require no conversion.
				// 
				if (inUTC)
					date = ConvertToUtc(timeZone, date);
			}
			else
			{
				if (!inUTC)
					date = ConvertFromUtc(timeZone, date);
			}

			return date;
		}

		#endregion // GetActivityStartEnd

		#region GetActivityStartUTC

		internal static DateTime GetActivityStartUTC(ActivityBase activity, TimeZoneToken tz)
		{
			return GetActivityStartEnd(activity, tz, true, true);
		}

		#endregion // GetActivityStartUTC

		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		#region GetActivityType
		internal static ActivityType? GetActivityType(ActivityTypes activityType)
		{
			switch (activityType)
			{
				case ActivityTypes.Appointment:
					return ActivityType.Appointment;
				case ActivityTypes.Journal:
					return ActivityType.Journal;
				case ActivityTypes.Task:
					return ActivityType.Task;
				default:
					Debug.Assert(false, "Invalid activitytypes");
					return null;
			}
		}
		#endregion //GetActivityType

		#region GetDateRange

		/// <summary>
		/// Gets the activity's date range.
		/// </summary>
		/// <param name="activity">Activity object.</param>
		/// <returns>Date-time range of the activity.</returns>
		internal static DateRange GetDateRange( ActivityBase activity )
		{
			return new DateRange( activity.Start, activity.End );
		}

		#endregion // GetDateRange

		#region GetDateRangeInUTC

		/// <summary>
		/// Gets the activity's date range.
		/// </summary>
		/// <param name="activity">Activity object.</param>
		/// <param name="localTimeZone">Local time zone. Used when the activity is time-zone neutral in which case its
		/// start and end times are in effect in local time zone, and therefore we need to convert them to UTC in order
		/// to compare to the date ranges which are in UTC.</param>
		/// <returns>Date-time range of the activity in UTC.</returns>
		internal static DateRange GetDateRangeInUTC( ActivityBase activity, TimeZoneToken localTimeZone )
		{
			DateRange range = GetDateRange( activity );

			if ( activity.IsTimeZoneNeutral )
				range = ConvertToUtc( localTimeZone, range );

			return range;
		}

		#endregion // GetDateRangeInUTC

		#region GetEnumValues
		internal static List<T> GetEnumValues<T>()
			where T : struct
		{
			Debug.Assert(typeof(T).IsEnum, "Only handles enums");

			return typeof(T).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
				.Where(f => f.IsLiteral)
				.Select(f => (T)f.GetValue(null))
				.ToList();
		}
		#endregion // GetEnumValues

		#region GetInvalidTimeZoneError

		internal static DataErrorInfo GetInvalidTimeZoneError(string id, object context = null)
		{
			string msg;

			string idStr = null != id ? id : "null";

			ActivityBase activity = context as ActivityBase;
			if (null != activity)
				msg = ScheduleUtilities.GetString("LE_ActivityHasInvalidTZ", activity.Id, idStr);// "Activity with the id of '{0}' has an invalid time-zone id value of '{0}'. No time-zone with the specified id exists."
			else
				msg = ScheduleUtilities.GetString("LE_InvalidTZId", idStr);// "'{0}' time-zone id is invalid. No time-zone with the specified id exists."

			return DataErrorInfo.CreateDiagnostic(context, msg);
		}

		#endregion // GetInvalidTimeZoneError

		#region GetNonNullValues

		internal static T[] GetNonNullValues<T>(params T[] values) where T : class
		{
			List<T> list = new List<T>();

			for (int i = 0; i < values.Length; i++)
			{
				T ii = values[i];
				if (null != ii)
					list.Add(ii);
			}

			return list.ToArray();
		}

		#endregion // GetNonNullValues

		#region GetObjectForDataItemComparison

		internal static object GetObjectForDataItemComparison(object dataItem)
		{

			return Infragistics.Windows.Internal.DataBindingUtilities.GetObjectForComparison(dataItem);



		}

		#endregion // GetObjectForDataItemComparison

		#region GetPrimaryCalendars

		internal static IEnumerable<ResourceCalendar> GetPrimaryCalendars(IEnumerable<Resource> resources)
		{
			return null != resources ? from ii in resources where null != ii.PrimaryCalendar select ii.PrimaryCalendar : null;
		}

		#endregion // GetPrimaryCalendars

		#region GetRecurrenceInfo

		internal static RecurrenceInfo GetRecurrenceInfo( ActivityBase root, RecurrenceBase recurrence, IScheduleDataConnector connector, DateTime? startTimeOverride = null )
		{
			DataErrorInfo error;

			// SSP 11/2/10 TFS58889
			// If recurring activity is time-zone neutral then use UTC as the time zone with which to generate occurrences.
			// 
			//string timeZoneId = root.StartTimeZoneId;
			string timeZoneId = root.IsTimeZoneNeutral ? null : root.StartTimeZoneId;

			TimeZoneToken token = ScheduleUtilities.GetTimeZoneToken( connector, timeZoneId, true, out error, root );

			if ( null != error )
			{
				ScheduleUtilities.RaiseErrorHelper( connector, error );
				return null;
			}

			DateTime startLocal = root.GetLocalHelper( token, startTimeOverride ?? root.Start );
			return new RecurrenceInfo( recurrence, startLocal, root.Duration, token, root );
		} 

		#endregion // GetRecurrenceInfo

		#region GetSingleItemEnumerable

		internal static IEnumerable<T> GetSingleItemEnumerable<T>(T item)
		{
			return new T[] { item };
		}

		#endregion // GetSingleItemEnumerable

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

		#region GetTimeZoneToken

		internal static TimeZoneToken GetTimeZoneToken(
			IScheduleDataConnector dc, string id, bool interpretNullIdAsUTC, out DataErrorInfo error, object context = null)
		{
			return GetTimeZoneToken(dc.TimeZoneInfoProviderResolved, id, interpretNullIdAsUTC, out error, context);
		}

		/// <summary>
		/// Gets the time zone for the resource based on its PrimaryTimeZoneId property. If id not specified or is invalid,
		/// fallbacks to fallbackTimeZone and if that's null then to local time zone.
		/// </summary>
		/// <param name="resource"></param>
		/// <param name="provider"></param>
		/// <param name="fallbackTimeZone"></param>
		/// <returns></returns>
		internal static TimeZoneToken GetTimeZoneToken(Resource resource, TimeZoneInfoProvider provider, TimeZoneToken fallbackTimeZone = null)
		{
			string tzId = null != resource ? resource.PrimaryTimeZoneId : null;

			DataErrorInfo error;
			return GetTimeZoneToken(provider, tzId, false, out error, resource) ?? fallbackTimeZone ?? provider.LocalToken;
		}

		internal static TimeZoneToken GetTimeZoneToken(
			TimeZoneInfoProvider provider, string id, bool interpretNullIdAsUTC, out DataErrorInfo error, object context = null)
		{
			error = null;

			if (string.IsNullOrEmpty(id))
			{
				if (interpretNullIdAsUTC)
					return provider.UtcToken;
			}
			else
			{
				TimeZoneToken token;
				if (provider.TryGetTimeZoneToken(id, out token))
					return token;
			}

			error = GetInvalidTimeZoneError(id, context);
			return null;
		}

		#endregion // GetTimeZoneToken

		#region HasSameItems

		internal static bool HasSameItems<T>(IList<T> x, IList<T> y, bool orderIsSignificant)
		{
			int xCount = null != x ? x.Count : 0;
			int yCount = null != y ? y.Count : 0;

			if (xCount != yCount)
				return false;

			if (xCount > 0)
			{
				if (orderIsSignificant || 1 == xCount)
				{
					return AreEqual(x, y);
				}
				else
				{
					HashSet<T> xSet = x as HashSet<T> ?? new HashSet<T>(x);

					
					
					
					
					return xSet.SetEquals( y );
				}
			}

			return true;
		}

		#endregion // HasSameItems	

		#region Intersects

		internal static bool Intersects( DateRange range, IEnumerable<DateRange> ranges )
		{
			foreach ( DateRange ii in ranges )
			{
				if ( ii.IntersectsWith( range ) )
					return true;
			}

			return false;
		}

		/// <summary>
		/// Checks to see if the activity's date range intersects with one of the date ranges in rangesInUTC collection.
		/// </summary>
		/// <param name="activity">Activity instance.</param>
		/// <param name="rangesInUTC">Date ranges in UTC.</param>
		/// <param name="localTimeZone">Local time zone. Used when the activity is time-zone neutral in which case its
		/// start and end times are in effect in local time zone, and therefore we need to convert them to UTC in order
		/// to compare to the date ranges which are in UTC.</param>
		/// <returns>True if the activity date range intersects with one of the date ranges specified via the 'rangesInUTC' parameter.</returns>
		internal static bool Intersects( ActivityBase activity, IEnumerable<DateRange> rangesInUTC, TimeZoneToken localTimeZone )
		{
			// SSP 1/13/10 TFS61964
			// If the activity is a time-zone neutral activity, then its time is in local time, which needs to be converted
			// to UTC in order to compare it against date ranges which are in UTC.
			// 
			//DateRange activityRange = GetDateRange( activity );
			DateRange activityRange = GetDateRangeInUTC( activity, localTimeZone );
			
			return Intersects( activityRange, rangesInUTC );
		}

		#endregion // Intersects	

		#region IsSameDateTimeWithinSecond

		internal static bool IsSameDateTimeWithinSecond(DateTime x, DateTime y)
		{
			return Math.Abs((x - y).TotalSeconds) < 1 && IsSameDay(x, y);
		}

		#endregion // IsSameDateTimeWithinSecond	

		#region IsSameDay

		internal static bool IsSameDay(DateTime x, DateTime y)
		{
			return x.Date == y.Date;
		}

		#endregion // IsSameDay

		#region ManageListenerHelper

		internal static void ManageListenerHelper<T>(ref T val, T newVal, IPropertyChangeListener listener, bool useWeakReference)
			where T : ISupportPropertyChangeNotifications
		{
			if (val != null)
				val.RemoveListener(listener);

			val = newVal;

			if (val != null)
				val.AddListener(listener, useWeakReference);
		}

		internal static void ManageListenerHelperObj<T>(ref T val, T newVal, IPropertyChangeListener listener, bool useWeakReference)
		{
			ISupportPropertyChangeNotifications vcn = val as ISupportPropertyChangeNotifications;
			if (vcn != null)
				vcn.RemoveListener(listener);

			val = newVal;

			vcn = newVal as ISupportPropertyChangeNotifications;
			if (vcn != null)
				vcn.AddListener(listener, useWeakReference);
		}

		#endregion // ManageListenerHelper

		#region Max
		/// <summary>
		/// Helper method to return the larger value or non-NaN value if one is NaN
		/// </summary>
		internal static double Max(double d1, double d2)
		{
			if (d1 > d2)
				return d1;

			// AS 5/9/12 TFS104555
			// If d1 was NaN then this would have returned NaN.
			//
			//if (d2 > d1)
			//	return d2;
			//
			//return d1;
			if (double.IsNaN(d2))
				return d1;

			return d2;
		}

		internal static DateTime Max(DateTime x, DateTime y)
		{
			if (y > x)
				return y;

			return x;
		}

		#endregion // Max

		#region MaxReached

		/// <summary>
		/// If max > 0 then returns true if value > max;
		/// </summary>
		/// <param name="max"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		internal static bool MaxReached(int max, int value)
		{
			return max > 0 && value >= max;
		}

		#endregion // MaxReached

		#region Min
		/// <summary>
		/// Helper method to return the larger value or non-NaN value if one is NaN
		/// </summary>
		internal static double Min(double d1, double d2)
		{
			if (d1 < d2)
				return d1;

			if (d2 < d1)
				return d2;

			return d1;
		}

		internal static DateTime Min(DateTime x, DateTime y)
		{
			if (y < x)
				return y;

			return x;
		}

		#endregion // Min

		#region NotifyListenersHelper

		internal static void NotifyListenersHelper(DependencyObject item, DependencyPropertyChangedEventArgs e, IPropertyChangeListener listener, bool hookIntoNewSubobject, bool hookAsUseWeakReference)
		{
			ISupportPropertyChangeNotifications sn = e.OldValue as ISupportPropertyChangeNotifications;
			if (null != sn)
				sn.RemoveListener(listener);

			if (hookIntoNewSubobject)
			{
				sn = e.NewValue as ISupportPropertyChangeNotifications;
				if (null != sn)
					sn.AddListener(listener, hookAsUseWeakReference);
			}

			if (null != listener)
				listener.OnPropertyValueChanged(item, DependencyPropertyUtilities.GetName(e.Property), e);
		}

		internal static void NotifyListenersHelperWithResolved<T>(DependencyObject item, DependencyPropertyChangedEventArgs e,
			IPropertyChangeListener listener, bool hookIntoNewSubobject, bool hookAsUseWeakReference, ref T resolvedVar)
			where T : class
		{
			if (null != resolvedVar)
			{
				ISupportPropertyChangeNotifications cn = resolvedVar as ISupportPropertyChangeNotifications;
				if (null != cn)
					cn.RemoveListener(listener);

				resolvedVar = null;
			}

			NotifyListenersHelper(item, e, listener, hookIntoNewSubobject, hookAsUseWeakReference);

			string propName = DependencyPropertyUtilities.GetName(e.Property);
			string resolvedName = null != propName ? propName + "Resolved" : null;
			Debug.Assert(null != resolvedName);

			if (null != resolvedName)
				listener.OnPropertyValueChanged(item, resolvedName, null);
		}

		#endregion // NotifyListenersHelper

		#region RaiseErrorHelper

		internal static void RaiseErrorHelper(IScheduleDataConnector dc, DataErrorInfo error)
		{
			Debug.Assert(null != dc);
			if (null != error && null != dc)
				dc.OnError(error);
		}

		#endregion // RaiseErrorHelper

		#region RegisterSerializerInfos

		internal static void RegisterSerializerInfos(ObjectSerializer serializer)
		{
			serializer.RegisterInfo(typeof(ScheduleDaysOfWeek), new ReflectionSerializationInfo(typeof(ScheduleDaysOfWeek)));
			serializer.RegisterInfo(typeof(ScheduleDayOfWeek), ScheduleDayOfWeek.GetSerializationInfo());
			serializer.RegisterInfo(typeof(DaySettings), DaySettings.GetSerializationInfo());
			serializer.RegisterInfo(typeof(DaySettingsOverride), DaySettingsOverride.GetSerializationInfo());
			serializer.RegisterInfo(typeof(DaySettingsOverrideCollection), new CollectionSerializationInfo<DaySettingsOverrideCollection, DaySettingsOverride>());
			serializer.RegisterInfo(typeof(WorkingHoursCollection), new CollectionSerializationInfo<WorkingHoursCollection, TimeRange>());
			serializer.RegisterInfo(typeof(TimeRange), new TimeRange.SerializationInfo());
			serializer.RegisterReflectionSerializationInfo(typeof(DateRecurrence));
			serializer.RegisterReflectionSerializationInfo(typeof(Reminder));

			
			
			serializer.RegisterInfo( typeof( ActivityCategoryCollection ), new CollectionSerializationInfo<ActivityCategoryCollection, ActivityCategory>( ) );
			serializer.RegisterInfo( typeof( ActivityCategory ), new ReflectionSerializationInfo( typeof( ActivityCategory ) ) );
		}

		#endregion // RegisterSerializerInfos

        // AS 1/20/11 TFS62537
        #region Reinitialize
        /// <summary>
        /// Helper method for invoking the Reinitialize method where the call is only made if the collections differ.
        /// </summary>
        /// <typeparam name="T">The type of items in the collections</typeparam>
        /// <param name="collection">The collection to reinitialize</param>
        /// <param name="newItems">The new list of items used to update the collection</param>
        internal static void Reinitialize<T>(ObservableCollectionExtended<T> collection, IList<T> newItems)
        {
            if (!AreEqual(collection, newItems))
                collection.ReInitialize(newItems);
        } 
        #endregion //Reinitialize

		#region RemoveAllItems

		internal static void RemoveAllItems(IList destList, IEnumerable itemsToRemove)
		{
			if (null != itemsToRemove)
			{
				foreach (object ii in itemsToRemove)
					destList.Remove(ii);
			}
		}

		// SSP 4/16/11 TFS63818
		// 
		internal static void RemoveAllItems<T>( IList<T> destList, IEnumerable<T> itemsToRemove )
		{
			if ( null != itemsToRemove )
			{
				ObservableCollectionExtended<T> collExtended = destList as ObservableCollectionExtended<T>;
				if ( null != collExtended )
					collExtended.BeginUpdate( );

				try
				{
					foreach ( T ii in itemsToRemove )
						destList.Remove( ii );
				}
				finally
				{
					if ( null != collExtended )
						collExtended.EndUpdate( );
				}
			}
		}

		#endregion // RemoveAllItems

		#region ReplaceCollectionContents

		internal static void ReplaceCollectionContents<T>( ObservableCollectionExtended<T> coll, IEnumerable<T> newItems )
		{
			if ( newItems == null )
				coll.Clear( );
			else
				coll.ReInitialize( newItems );
		}

		#endregion // ReplaceCollectionContents

		#region SpecifyKindUtc

		internal static DateTime SpecifyKindUtc(DateTime date)
		{
			return DateTimeKind.Utc == date.Kind ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc);
		}

		#endregion // SpecifyKindUtc

		#region ToCalendars

		/// <summary>
		/// Converts value which can be Resource, IEnumerable&lt;Resource&gt;, ResourceCalendar or IEnumerable&lt;ResourceCalendar&gt; into IEnumerable&lt;ResourceCalendar&gt;.
		/// </summary>
		/// <param name="value">Value can be Resource, IEnumerable&lt;Resource&gt;, ResourceCalendar or IEnumerable&lt;ResourceCalendar&gt;.</param>
		/// <returns></returns>
		internal static IEnumerable<ResourceCalendar> ToCalendars(object value)
		{
			// If a Resource or a collection of Resource objects is passed in than translate them to their primary calendars.
			// 
			if (value is Resource)
				value = ((Resource)value).PrimaryCalendar;
			else if (value is IEnumerable<Resource>)
				value = ScheduleUtilities.GetPrimaryCalendars((IEnumerable<Resource>)value);
			else if (value is ResourceCalendar)
				value = new ResourceCalendar[] { (ResourceCalendar)value };

			// At this point the value should be either a ResourceCalendar instance of an enumerable of resource calendars.
			// 
			IEnumerable<ResourceCalendar> calendars = value as IEnumerable<ResourceCalendar>;
			if (null == calendars)
			{
				if (value is ResourceCalendar)
					calendars = new ResourceCalendar[] { (ResourceCalendar)value };
				else if (value is IEnumerable<ResourceCalendar>)
					calendars = (IEnumerable<ResourceCalendar>)value;
				else
					Debug.Assert(null == value, "Unknown value.");
			}

			return calendars;
		}

		#endregion // ToCalendars

		#region ToCalendarsReadOnlyCollection

		/// <summary>
		/// Converts value which can be Resource, IEnumerable&lt;Resource&gt;, ResourceCalendar or IEnumerable&lt;ResourceCalendar&gt; into ImmutableCollection&lt;ResourceCalendar&gt;.
		/// </summary>
		/// <param name="value">Value can be Resource, IEnumerable&lt;Resource&gt;, ResourceCalendar or IEnumerable&lt;ResourceCalendar&gt;.</param>
		/// <returns></returns>
		internal static ImmutableCollection<ResourceCalendar> ToCalendarsReadOnlyCollection(object value)
		{
			IEnumerable<ResourceCalendar> calendars = ToCalendars(value);

			return ToImmutableCollection<ResourceCalendar>(calendars);
		}

		#endregion // ToCalendarsReadOnlyCollection

		#region ToICalendarString

		internal static string ToICalendarString(DateTime dt, bool appendZ, bool skipTimeIf12AM = false)
		{
			return DateRecurrenceParser.ToICalendarString(dt, appendZ, skipTimeIf12AM);
		}

		#endregion // ToICalendarString	

		#region ToImmutableCollection

		/// <summary>
		/// Converts value which can be a T instance, or an IEnumerable&lt;T&gt; to ImmutableCollection&lt;T&gt;.
		/// </summary>
		/// <param name="value">Value can be a T or an IEnumerable&lt;T&gt;.</param>
		/// <returns></returns>
		internal static ImmutableCollection<T> ToImmutableCollection<T>(object value)
		{
			if (null == value)
				return null;

			if (value is ImmutableCollection<T>)
				return (ImmutableCollection<T>)value;

			IList<T> list;

			if (value is T)
			{
				list = new T[] { (T)value };
			}
			else
			{
				list = value as IList<T>;

				if (null == list)
					list = new List<T>((IEnumerable<T>)value);
			}

			return new ImmutableCollection<T>(list);
		}

		#endregion // ToImmutableCollection

		#region ToTyped

		internal static IEnumerable<T> ToTyped<T>(IEnumerable e)
		{
			IEnumerable<T> r = e as IEnumerable<T>;

			if (null == r && null != e)
				r = new TypedEnumerable<T>(e);

			return r;
		}

		#endregion // ToTyped

		#region TruncateToSecond

		/// <summary>
		/// Truncates the specified date to seconds.
		/// </summary>
		/// <param name="date">Date to trucate.</param>
		/// <returns>Date value truncated to seconds.</returns>
		internal static DateTime TruncateToSecond(DateTime date)
		{
			return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
		}

		#endregion // TruncateToSecond

		#endregion  // Methods

		#region MethodInvoker

		internal delegate void MethodInvoker();

		#endregion //MethodInvoker
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