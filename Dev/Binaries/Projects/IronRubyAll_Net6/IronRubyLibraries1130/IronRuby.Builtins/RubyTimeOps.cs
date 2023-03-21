using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[RubyClass("Time", Extends = typeof(RubyTime), Inherits = typeof(object))]
	[Includes(new Type[] { typeof(Comparable) })]
	public static class RubyTimeOps
	{
		private static string[] _Months = new string[12]
		{
			"jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct",
			"nov", "dec"
		};

		[RubyConstructor]
		public static RubyTime Create(RubyClass self)
		{
			return new RubyTime(RubyTime.GetCurrentLocalTime());
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyTime Reinitialize(RubyTime self)
		{
			self.DateTime = RubyTime.GetCurrentLocalTime();
			return self;
		}

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static RubyTime InitializeCopy(RubyTime self, [NotNull] RubyTime other)
		{
			self.SetDateTime(other.DateTime);
			return self;
		}

		[RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime Create(RubyClass self, [NotNull] RubyTime other)
		{
			return new RubyTime(other.Ticks, other.Kind);
		}

		[RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime Create(RubyClass self, double seconds)
		{
			return new RubyTime(RubyTime.ToLocalTime(RubyTime.AddSeconds(RubyTime.Epoch, seconds)));
		}

		[RubyMethod("at", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime Create(RubyClass self, int seconds, int microseconds)
		{
			return new RubyTime(RubyTime.ToLocalTime(RubyTime.Epoch.AddTicks(RubyTime.ToTicks(seconds, microseconds))));
		}

		[RubyMethod("now", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime Now(RubyClass self)
		{
			return Create(self);
		}

		private static int NormalizeYear(int year)
		{
			if (year == 0)
			{
				return 2000;
			}
			return year;
		}

		private static RubyTime CreateTime(int year, int month, int day, int hour, int minute, int second, int microsecond, DateTimeKind kind)
		{
			DateTime dateTime = new DateTime(NormalizeYear(year), month, day, hour, minute, (second == 60) ? 59 : second, 0, kind);
			if (second == 60)
			{
				dateTime = dateTime.AddSeconds(1.0);
			}
			return new RubyTime(dateTime.AddTicks(microsecond * 10));
		}

		[RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime CreateLocalTime(object self, int year, int month, int day, [Optional] int hour, [Optional] int minute, [Optional] int second, [Optional] int microsecond)
		{
			return CreateTime(year, month, day, hour, minute, second, microsecond, DateTimeKind.Local);
		}

		[RubyMethod("local", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("mktime", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime CreateLocalTime(ConversionStorage<int> conversionStorage, ConversionStorage<MutableString> strConversionStorage, RubyClass self, params object[] components)
		{
			return CreateTime(conversionStorage, strConversionStorage, components, DateTimeKind.Local);
		}

		[RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime CreateGmtTime(object self, int year, int month, int day, [Optional] int hour, [Optional] int minute, [Optional] int second, [Optional] int microsecond)
		{
			return CreateTime(year, month, day, hour, minute, second, microsecond, DateTimeKind.Utc);
		}

		[RubyMethod("gm", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("utc", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime CreateGmtTime(ConversionStorage<int> conversionStorage, ConversionStorage<MutableString> strConversionStorage, RubyClass self, params object[] components)
		{
			return CreateTime(conversionStorage, strConversionStorage, components, DateTimeKind.Utc);
		}

		private static int GetComponent(ConversionStorage<int> conversionStorage, object[] components, int index, int defValue, bool zeroOk)
		{
			if (index >= components.Length || components[index] == null)
			{
				return defValue;
			}
			object obj = components[index];
			int num;
			try
			{
				num = Protocols.CastToFixnum(conversionStorage, obj);
			}
			catch (InvalidOperationException)
			{
				MutableString mutableString = obj as MutableString;
				if (mutableString == null)
				{
					throw;
				}
				num = (int)MutableStringOps.ToInteger(mutableString, 10);
			}
			if (num == 0 && !zeroOk)
			{
				return defValue;
			}
			return num;
		}

		private static int GetComponent(ConversionStorage<int> conversionStorage, object[] components, int index, int defValue)
		{
			return GetComponent(conversionStorage, components, index, defValue, true);
		}

		private static int GetYearComponent(ConversionStorage<int> conversionStorage, object[] components, int index)
		{
			return GetComponent(conversionStorage, components, index, 2000, false);
		}

		private static int GetMonthComponent(ConversionStorage<int> conversionStorage, ConversionStorage<MutableString> strConversionStorage, object[] components, int index)
		{
			if (index >= components.Length || components[index] == null)
			{
				return 1;
			}
			MutableString mutableString = Protocols.TryCastToString(strConversionStorage, components[index]);
			if (mutableString != null)
			{
				string text = mutableString.ConvertToString();
				if (text.Length == 3)
				{
					string strLower = text.ToLowerInvariant();
					int num = _Months.FindIndex((string obj) => obj == strLower);
					if (num != -1)
					{
						return num + 1;
					}
				}
				components[index] = mutableString;
			}
			return GetComponent(conversionStorage, components, index, 1, false);
		}

		private static RubyTime CreateTime(ConversionStorage<int> conversionStorage, ConversionStorage<MutableString> strConversionStorage, object[] components, DateTimeKind kind)
		{
			if (components.Length == 10)
			{
				object[] array = new object[6];
				Array.Copy(components, array, 6);
				Array.Reverse(array);
				components = array;
			}
			else if (components.Length > 7 || components.Length == 0)
			{
				throw RubyExceptions.CreateArgumentError("wrong number of arguments ({0} for 7)", components.Length);
			}
			return CreateTime(GetYearComponent(conversionStorage, components, 0), GetMonthComponent(conversionStorage, strConversionStorage, components, 1), GetComponent(conversionStorage, components, 2, 1), GetComponent(conversionStorage, components, 3, 0), GetComponent(conversionStorage, components, 4, 0), GetComponent(conversionStorage, components, 5, 0), GetComponent(conversionStorage, components, 6, 0), kind);
		}

		[RubyMethod("times", RubyMethodAttributes.PublicSingleton, Compatibility = RubyCompatibility.Ruby186, BuildConfig = "!SILVERLIGHT")]
		public static RubyStruct Times(RubyClass self)
		{
			return RubyProcess.GetTimes(self);
		}

		[RubyMethod("_dump")]
		public static MutableString Dump(RubyContext context, RubyTime self, [Optional] int depth)
		{
			if (self.DateTime.Year < 1900 || self.DateTime.Year > 2038)
			{
				throw RubyExceptions.CreateTypeError("unable to marshal time");
			}
			DateTime dateTime = RubyTime.ToUniversalTime(self.DateTime);
			uint num = ((self.Kind == DateTimeKind.Utc) ? 3221225472u : 2147483648u);
			num |= (uint)(dateTime.Year - 1900 << 14);
			num |= (uint)(dateTime.Month - 1 << 10);
			num |= (uint)(dateTime.Day << 5);
			num |= (uint)dateTime.Hour;
			uint num2 = 0u;
			num2 |= (uint)(dateTime.Minute << 26);
			num2 |= (uint)(dateTime.Second << 20);
			num2 |= (uint)(int)(dateTime.Ticks % 10000000 / 10);
			MemoryStream memoryStream = new MemoryStream(8);
			RubyEncoder.Write(memoryStream, num, !BitConverter.IsLittleEndian);
			RubyEncoder.Write(memoryStream, num2, !BitConverter.IsLittleEndian);
			return MutableString.CreateBinary(memoryStream.ToArray());
		}

		private static uint GetUint(byte[] data, int start)
		{
			return (uint)((((data[start + 3] << 8) + data[start + 2] << 8) + data[start + 1] << 8) + data[start]);
		}

		[RubyMethod("_load", RubyMethodAttributes.PublicSingleton)]
		public static RubyTime Load(RubyContext context, RubyClass self, [NotNull] MutableString time)
		{
			byte[] array = time.ConvertToBytes();
			if (array.Length != 8)
			{
				throw RubyExceptions.CreateTypeError("marshaled time format differ");
			}
			uint @uint = GetUint(array, 0);
			uint uint2 = GetUint(array, 4);
			if ((array[3] & 0x80) == 0)
			{
				int num = (int)@uint;
				uint num2 = uint2;
				return new RubyTime(RubyTime.ToLocalTime(RubyTime.Epoch.AddTicks(RubyTime.ToTicks(num, num2))));
			}
			bool flag = (array[3] & 0x40) != 0;
			int year = (int)(1900 + ((@uint >> 14) & 0xFFFF));
			int month = (int)(1 + ((@uint >> 10) & 0xF));
			int day = (int)((@uint >> 5) & 0x1F);
			int hour = (int)(@uint & 0x1F);
			int minute = (int)((uint2 >> 26) & 0x2F);
			int second = (int)((uint2 >> 20) & 0x2F);
			int num3 = (int)(uint2 & 0xFFFFF);
			DateTime dateTime;
			try
			{
				dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(num3 * 10);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw RubyExceptions.CreateTypeError("marshaled time format differ");
			}
			return new RubyTime(flag ? dateTime : RubyTime.ToLocalTime(dateTime));
		}

		[RubyMethod("succ")]
		public static RubyTime SuccessiveSecond(RubyTime self)
		{
			return AddSeconds(self, 1.0);
		}

		[RubyMethod("+")]
		public static RubyTime AddSeconds(RubyTime self, [DefaultProtocol] double seconds)
		{
			try
			{
				return new RubyTime(RubyTime.AddSeconds(self.DateTime, seconds));
			}
			catch (OverflowException)
			{
				throw RubyExceptions.CreateRangeError("time + {0:F6} out of Time range", seconds);
			}
		}

		[RubyMethod("+")]
		public static RubyTime AddSeconds(RubyTime self, [NotNull] RubyTime seconds)
		{
			throw RubyExceptions.CreateTypeError("time + time?");
		}

		[RubyMethod("-")]
		public static RubyTime SubtractSeconds(RubyTime self, [DefaultProtocol] double seconds)
		{
			DateTime dateTime;
			try
			{
				dateTime = RubyTime.AddSeconds(self.DateTime, 0.0 - seconds);
			}
			catch (OverflowException)
			{
				throw RubyExceptions.CreateRangeError("time - {0:F6} out of Time range", seconds);
			}
			return new RubyTime(dateTime);
		}

		[RubyMethod("-")]
		public static double SubtractTime(RubyTime self, [NotNull] RubyTime other)
		{
			return (self - other).TotalSeconds;
		}

		[RubyMethod("-")]
		public static double SubtractTime(RubyTime self, DateTime other)
		{
			return (self.DateTime - other).TotalSeconds;
		}

		[RubyMethod("<=>")]
		public static int CompareTo(RubyTime self, [NotNull] RubyTime other)
		{
			return self.CompareTo(other);
		}

		[RubyMethod("<=>")]
		public static object CompareSeconds(RubyTime self, object other)
		{
			return null;
		}

		[RubyMethod("eql?")]
		public static bool Eql(RubyTime self, [NotNull] RubyTime other)
		{
			return self.Equals(other);
		}

		[RubyMethod("eql?")]
		public static bool Eql(RubyTime self, object other)
		{
			return false;
		}

		[RubyMethod("hash")]
		public static int GetHash(RubyTime self)
		{
			return self.GetHashCode();
		}

		[RubyMethod("gmtime")]
		[RubyMethod("utc")]
		public static RubyTime SwitchToUtc(RubyTime self)
		{
			self.SetDateTime(self.ToUniversalTime());
			return self;
		}

		[RubyMethod("localtime")]
		[RubyMethod("getlocal")]
		public static RubyTime ToLocalTime(RubyTime self)
		{
			self.SetDateTime(self.ToLocalTime());
			return self;
		}

		[RubyMethod("utc?")]
		[RubyMethod("gmt?")]
		public static bool IsUts(RubyTime self)
		{
			return self.DateTime.Kind == DateTimeKind.Utc;
		}

		[RubyMethod("dst?")]
		[RubyMethod("isdst")]
		public static object IsDst(RubyContext context, RubyTime self)
		{
			return self.GetCurrentDst(context);
		}

		[RubyMethod("gmt_offset")]
		[RubyMethod("gmtoff")]
		[RubyMethod("utc_offset")]
		public static object Offset(RubyTime self)
		{
			return Protocols.Normalize(self.GetCurrentZoneOffset().Ticks / 10000000);
		}

		[RubyMethod("getgm")]
		[RubyMethod("getutc")]
		public static RubyTime GetUTC(RubyTime self)
		{
			return new RubyTime(self.ToUniversalTime());
		}

		[RubyMethod("zone")]
		public static MutableString GetZone(RubyContext context, RubyTime self)
		{
			if (self.Kind == DateTimeKind.Utc)
			{
				return MutableString.CreateAscii("UTC");
			}
			string currentZoneName = RubyTime.GetCurrentZoneName();
			if (currentZoneName.IsAscii())
			{
				return MutableString.CreateAscii(currentZoneName);
			}
			return MutableString.Create(currentZoneName, context.GetPathEncoding());
		}

		[RubyMethod("hour")]
		public static int Hour(RubyTime self)
		{
			return self.DateTime.Hour;
		}

		[RubyMethod("min")]
		public static int Minute(RubyTime self)
		{
			return self.DateTime.Minute;
		}

		[RubyMethod("sec")]
		public static int Second(RubyTime self)
		{
			return self.DateTime.Second;
		}

		[RubyMethod("usec")]
		[RubyMethod("tv_usec")]
		public static int GetMicroSeconds(RubyTime self)
		{
			return self.Microseconds;
		}

		[RubyMethod("year")]
		public static int Year(RubyTime self)
		{
			return self.DateTime.Year;
		}

		[RubyMethod("month")]
		[RubyMethod("mon")]
		public static int Month(RubyTime self)
		{
			return self.DateTime.Month;
		}

		[RubyMethod("mday")]
		[RubyMethod("day")]
		public static int Day(RubyTime self)
		{
			return self.DateTime.Day;
		}

		[RubyMethod("yday")]
		public static int DayOfYear(RubyTime self)
		{
			return self.DateTime.DayOfYear;
		}

		[RubyMethod("wday")]
		public static int DayOfWeek(RubyTime self)
		{
			return (int)self.DateTime.DayOfWeek;
		}

		[RubyMethod("strftime")]
		public static MutableString FormatTime(RubyContext context, RubyTime self, [DefaultProtocol][NotNull] MutableString format)
		{
			MutableString mutableString = MutableString.CreateMutable(format.Encoding);
			bool flag = false;
			MutableString.CharacterEnumerator characters = format.GetCharacters();
			while (characters.MoveNext())
			{
				MutableString.Character current = characters.Current;
				int num = (current.IsValid ? current.Value : (-1));
				if (!flag)
				{
					if (num == 37)
					{
						flag = true;
					}
					else
					{
						mutableString.Append(current);
					}
					continue;
				}
				flag = false;
				string text = null;
				switch (num)
				{
				case 37:
					mutableString.Append('%');
					break;
				case 97:
					text = "ddd";
					break;
				case 65:
					text = "dddd";
					break;
				case 98:
					text = "MMM";
					break;
				case 66:
					text = "MMMM";
					break;
				case 99:
					text = "g";
					break;
				case 100:
					text = "dd";
					break;
				case 68:
					text = "MM/dd/yy";
					break;
				case 101:
				{
					int day = self.DateTime.Day;
					if (day < 10)
					{
						mutableString.Append(' ');
					}
					mutableString.Append(day.ToString(CultureInfo.InvariantCulture));
					break;
				}
				case 72:
					text = "HH";
					break;
				case 73:
					text = "hh";
					break;
				case 106:
					mutableString.AppendFormat("{0:000}", self.DateTime.DayOfYear);
					break;
				case 108:
				{
					int num2 = self.DateTime.Hour;
					if (num2 == 0)
					{
						num2 = 12;
					}
					else if (num2 > 12)
					{
						num2 -= 12;
					}
					if (num2 < 10)
					{
						mutableString.Append(' ');
					}
					mutableString.Append(num2.ToString(CultureInfo.InvariantCulture));
					break;
				}
				case 109:
					text = "MM";
					break;
				case 77:
					text = "mm";
					break;
				case 112:
					text = "tt";
					break;
				case 83:
					text = "ss";
					break;
				case 84:
					text = "HH:mm:ss";
					break;
				case 85:
					FormatDayOfWeek(mutableString, self.DateTime, 7);
					break;
				case 87:
					FormatDayOfWeek(mutableString, self.DateTime, 8);
					break;
				case 119:
					mutableString.Append(((int)self.DateTime.DayOfWeek).ToString(CultureInfo.InvariantCulture));
					break;
				case 120:
					text = "d";
					break;
				case 88:
					text = "t";
					break;
				case 121:
					text = "yy";
					break;
				case 89:
					text = "yyyy";
					break;
				case 90:
					text = "%K";
					break;
				case 122:
					if (context.RubyOptions.Compatibility > RubyCompatibility.Ruby186)
					{
						mutableString.Append(self.FormatUtcOffset());
					}
					else
					{
						mutableString.Append(RubyTime.GetCurrentZoneName());
					}
					break;
				default:
					if (context.RubyOptions.Compatibility > RubyCompatibility.Ruby186)
					{
						mutableString.Append(current);
						break;
					}
					return MutableString.CreateEmpty();
				}
				if (text != null)
				{
					mutableString.Append(self.ToString(text, CultureInfo.InvariantCulture));
				}
			}
			if (flag)
			{
				if (context.RubyOptions.Compatibility > RubyCompatibility.Ruby186)
				{
					return mutableString.Append('%');
				}
				return MutableString.CreateEmpty();
			}
			return mutableString;
		}

		private static void FormatDayOfWeek(MutableString result, DateTime dateTime, int start)
		{
			DateTime dateTime2 = dateTime.AddDays(1 - dateTime.DayOfYear);
			DateTime dateTime3 = dateTime2.AddDays((int)(start - dateTime2.DayOfWeek) % 7);
			int num = 1 + (int)Math.Floor((double)(dateTime - dateTime3).Days / 7.0);
			result.AppendFormat("{0:00}", num);
		}

		[RubyMethod("to_f")]
		public static double ToFloatSeconds(RubyTime self)
		{
			return (double)self.TicksSinceEpoch / 10000000.0;
		}

		[RubyMethod("tv_sec")]
		[RubyMethod("to_i")]
		public static object ToSeconds(RubyTime self)
		{
			return Protocols.Normalize(self.TicksSinceEpoch / 10000000);
		}

		[RubyMethod("ctime")]
		[RubyMethod("asctime")]
		public static MutableString CTime(RubyTime self)
		{
			return MutableString.CreateAscii(string.Format(CultureInfo.InvariantCulture, "{0:ddd MMM} {1,2} {0:HH:mm:ss yyyy}", new object[2]
			{
				self.DateTime,
				self.DateTime.Day
			}));
		}

		[RubyMethod("inspect")]
		[RubyMethod("to_s")]
		public static MutableString ToString(RubyContext context, RubyTime self)
		{
			return MutableString.CreateAscii(string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd HH:mm:ss} {1}", new object[2]
			{
				self.DateTime,
				(self.Kind == DateTimeKind.Utc) ? "UTC" : self.FormatUtcOffset()
			}));
		}

		[RubyMethod("to_a")]
		public static RubyArray ToArray(RubyContext context, RubyTime self)
		{
			RubyArray rubyArray = new RubyArray();
			rubyArray.Add(self.DateTime.Second);
			rubyArray.Add(self.DateTime.Minute);
			rubyArray.Add(self.DateTime.Hour);
			rubyArray.Add(self.DateTime.Day);
			rubyArray.Add(self.DateTime.Month);
			rubyArray.Add(self.DateTime.Year);
			rubyArray.Add((int)self.DateTime.DayOfWeek);
			rubyArray.Add(self.DateTime.DayOfYear);
			rubyArray.Add(self.GetCurrentDst(context));
			rubyArray.Add(GetZone(context, self));
			return rubyArray;
		}
	}
}
