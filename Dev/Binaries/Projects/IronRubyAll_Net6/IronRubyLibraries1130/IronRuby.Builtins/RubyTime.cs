using System;
using System.Globalization;
using System.Security;
using System.Text.RegularExpressions;
using IronRuby.Runtime;

namespace IronRuby.Builtins
{
	public class RubyTime : IComparable, IComparable<RubyTime>, IEquatable<RubyTime>, IFormattable
	{
		private sealed class TZ : TimeZone
		{
			private readonly TimeSpan _offset;

			private readonly string _standardName;

			public override string DaylightName
			{
				get
				{
					return _standardName;
				}
			}

			public override string StandardName
			{
				get
				{
					return _standardName;
				}
			}

			public TZ(TimeSpan offset, string standardName)
			{
				_offset = offset;
				_standardName = standardName;
			}

			public override TimeSpan GetUtcOffset(DateTime time)
			{
				if (time.Kind != DateTimeKind.Local)
				{
					return TimeSpan.Zero;
				}
				return _offset;
			}

			public override DateTime ToLocalTime(DateTime time)
			{
				return Adjust(time, _offset, DateTimeKind.Local);
			}

			public override DateTime ToUniversalTime(DateTime time)
			{
				return Adjust(time, -_offset, DateTimeKind.Utc);
			}

			private static DateTime Adjust(DateTime time, TimeSpan targetOffset, DateTimeKind targetKind)
			{
				if (time.Kind == targetKind)
				{
					return time;
				}
				long num = time.Ticks + targetOffset.Ticks;
				if (num > DateTime.MaxValue.Ticks)
				{
					return new DateTime(DateTime.MaxValue.Ticks, targetKind);
				}
				if (num < DateTime.MinValue.Ticks)
				{
					return new DateTime(DateTime.MinValue.Ticks, targetKind);
				}
				return new DateTime(num, targetKind);
			}

			public override bool IsDaylightSavingTime(DateTime time)
			{
				throw new NotSupportedException();
			}

			public override DaylightTime GetDaylightChanges(int year)
			{
				throw new NotSupportedException();
			}
		}

		public static readonly DateTime Epoch;

		internal static TimeZone _CurrentTimeZone;

		private static Regex _tzPattern;

		private DateTime _dateTime;

		public long TicksSinceEpoch
		{
			get
			{
				return ToUniversalTime().Ticks - Epoch.Ticks;
			}
		}

		public DateTime DateTime
		{
			get
			{
				return _dateTime;
			}
			set
			{
				_dateTime = Round(value);
			}
		}

		public long Ticks
		{
			get
			{
				return _dateTime.Ticks;
			}
		}

		public int Microseconds
		{
			get
			{
				return (int)(_dateTime.Ticks / 10 % 1000000);
			}
		}

		public DateTimeKind Kind
		{
			get
			{
				return _dateTime.Kind;
			}
		}

		static RubyTime()
		{
			Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			string timeZoneEnvSpec;
			try
			{
				timeZoneEnvSpec = Environment.GetEnvironmentVariable("TZ");
			}
			catch (SecurityException)
			{
				timeZoneEnvSpec = null;
			}
			TimeZone timeZone;
			TryParseTimeZone(timeZoneEnvSpec, out timeZone);
			_CurrentTimeZone = timeZone ?? TimeZone.CurrentTimeZone;
		}

		public static bool TryParseTimeZone(string timeZoneEnvSpec, out TimeZone timeZone)
		{
			if (string.IsNullOrEmpty(timeZoneEnvSpec))
			{
				timeZone = TimeZone.CurrentTimeZone;
				return true;
			}
			if (_tzPattern == null)
			{
				_tzPattern = new Regex("^\\s*\r\n                    (?<std>[^-+:,0-9\\0]{3,})\r\n                    (?<sh>[+-]?[0-9]{1,2})((:(?<sm>[0-9]{1,2}))?(:(?<ss>[0-9]{1,2}))?)?                    \r\n                    ", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
			}
			Match match = _tzPattern.Match(timeZoneEnvSpec);
			if (!match.Success)
			{
				timeZone = null;
				return false;
			}
			timeZone = new TZ(new TimeSpan(-int.Parse(match.Groups["sh"].Value, CultureInfo.InvariantCulture), match.Groups["sm"].Success ? int.Parse(match.Groups["sm"].Value, CultureInfo.InvariantCulture) : 0, match.Groups["ss"].Success ? int.Parse(match.Groups["ss"].Value, CultureInfo.InvariantCulture) : 0), match.Groups["std"].Value);
			return true;
		}

		public TimeSpan GetCurrentZoneOffset()
		{
			return _CurrentTimeZone.GetUtcOffset(_dateTime);
		}

		public static string GetCurrentZoneName()
		{
			return _CurrentTimeZone.StandardName;
		}

		public bool GetCurrentDst(RubyContext context)
		{
			TimeZone currentTimeZone = _CurrentTimeZone;
			if (currentTimeZone is TZ)
			{
				string standardName = currentTimeZone.StandardName;
				currentTimeZone = TimeZone.CurrentTimeZone;
				context.ReportWarning(string.Format(CultureInfo.InvariantCulture, "Daylight savings rule not available for time zone `{0}'; using the default time zone `{1}'", new object[2] { standardName, currentTimeZone.StandardName }));
			}
			return currentTimeZone.IsDaylightSavingTime(_dateTime);
		}

		public static DateTime ToUniversalTime(DateTime dateTime)
		{
			return _CurrentTimeZone.ToUniversalTime(dateTime);
		}

		public static DateTime ToLocalTime(DateTime dateTime)
		{
			return _CurrentTimeZone.ToLocalTime(dateTime);
		}

		public DateTime ToUniversalTime()
		{
			return ToUniversalTime(_dateTime);
		}

		public DateTime ToLocalTime()
		{
			return ToLocalTime(_dateTime);
		}

		public static DateTime GetCurrentLocalTime()
		{
			return ToLocalTime(DateTime.UtcNow);
		}

		public RubyTime(DateTime dateTime)
		{
			_dateTime = Round(dateTime);
		}

		public RubyTime()
			: this(ToLocalTime(Epoch))
		{
		}

		public RubyTime(long ticks, DateTimeKind kind)
			: this(new DateTime(ticks, kind))
		{
		}

		private static DateTime Round(DateTime dateTime)
		{
			long ticks = dateTime.Ticks;
			ticks = ((ticks % 10 < 5) ? (ticks - ticks % 10) : (ticks - ticks % 10 + 10));
			return new DateTime(ticks, dateTime.Kind);
		}

		internal void SetDateTime(DateTime value)
		{
			_dateTime = value;
		}

		internal static long ToTicks(long seconds, long microseconds)
		{
			return seconds * 10000000 + microseconds * 10;
		}

		internal static DateTime AddSeconds(DateTime dateTime, double seconds)
		{
			bool flag = dateTime.Kind == DateTimeKind.Local;
			if (flag)
			{
				dateTime = ToUniversalTime(dateTime);
			}
			dateTime = dateTime.AddTicks((long)(Math.Round(seconds, 6) * 10000000.0));
			if (flag)
			{
				dateTime = ToLocalTime(dateTime);
			}
			return dateTime;
		}

		public override string ToString()
		{
			return _dateTime.ToString();
		}

		public override int GetHashCode()
		{
			return _dateTime.GetHashCode();
		}

		int IComparable.CompareTo(object other)
		{
			return CompareTo(other as RubyTime);
		}

		public int CompareTo(RubyTime other)
		{
			if (!(other != null))
			{
				return -1;
			}
			return ToUniversalTime(_dateTime).CompareTo(ToUniversalTime(other._dateTime));
		}

		public static bool operator <(RubyTime x, RubyTime y)
		{
			return x.CompareTo(y) < 0;
		}

		public static bool operator <=(RubyTime x, RubyTime y)
		{
			return x.CompareTo(y) <= 0;
		}

		public static bool operator >(RubyTime x, RubyTime y)
		{
			return x.CompareTo(y) > 0;
		}

		public static bool operator >=(RubyTime x, RubyTime y)
		{
			return x.CompareTo(y) >= 0;
		}

		public static TimeSpan operator -(RubyTime x, DateTime y)
		{
			return ToUniversalTime(x._dateTime) - ToUniversalTime(y);
		}

		public static TimeSpan operator -(RubyTime x, RubyTime y)
		{
			return x - y._dateTime;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as RubyTime);
		}

		public bool Equals(RubyTime other)
		{
			return CompareTo(other) == 0;
		}

		public static bool operator ==(RubyTime x, RubyTime y)
		{
			if (!object.ReferenceEquals(x, null))
			{
				return x.Equals(y);
			}
			return object.ReferenceEquals(y, null);
		}

		public static bool operator !=(RubyTime x, RubyTime y)
		{
			return !(x == y);
		}

		public static explicit operator RubyTime(DateTime dateTime)
		{
			return new RubyTime(dateTime);
		}

		public static implicit operator DateTime(RubyTime time)
		{
			return time._dateTime;
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return _dateTime.ToString(format, provider);
		}

		internal string FormatUtcOffset()
		{
			TimeSpan currentZoneOffset = GetCurrentZoneOffset();
			return string.Format(CultureInfo.InvariantCulture, "{0}{1:D2}{2:D2}", new object[3]
			{
				(currentZoneOffset.Hours >= 0) ? "+" : null,
				currentZoneOffset.Hours,
				currentZoneOffset.Minutes
			});
		}
	}
}
