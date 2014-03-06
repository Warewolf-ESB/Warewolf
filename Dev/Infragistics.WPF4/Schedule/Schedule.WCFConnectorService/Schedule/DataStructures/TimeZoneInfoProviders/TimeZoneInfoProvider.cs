using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.IO;
using System.Windows;
using System.Diagnostics;


using Infragistics.Services;
using Infragistics.Collections.Services;
using Infragistics.Controls.Schedules.Primitives.Services;

namespace Infragistics.Controls.Schedules.Services






{
	/// <summary>
	/// Abstract base class used to provide time zone information for dateTime time conversions to and from utc dateTime times.
	/// </summary>
	public abstract class TimeZoneInfoProvider : DependencyObject, ISupportPropertyChangeNotifications
	{





		#region Private Members

		private Dictionary<string, TimeZoneToken> _timeZoneIdMap = new Dictionary<string, TimeZoneToken>();
		private Dictionary<string, TimeZoneToken> _timeZoneDisplayNameMap = new Dictionary<string, TimeZoneToken>(StringComparer.OrdinalIgnoreCase);

		private TimeZoneToken _localToken;
		private string _localTimeZoneId;
		private PropertyChangeListenerList _propChangeListeners;
		private ObservableCollectionExtended<TimeZoneToken> _tokens;

		#endregion //Private Members	
	
		#region Nested Classes and Structures

		#region DaylightTimeForYear

		internal class DaylightTimeForYear
		{
			private DateTime _start;
			private DateTime _end;
			private TimeSpan _delta;

			internal DaylightTimeForYear(DateTime start, DateTime end, TimeSpan delta)
			{
				_start = start;
				_end = end;
				_delta = delta;
			}

			internal DateTime Start { get { return _start; ; } }
			internal DateTime End { get { return _end; ; } }
			internal TimeSpan Delta { get { return _delta; ; } }

		}

		#endregion //DaylightTimeForYear	
	
		#region TimeZoneInformation internal class

		internal class TimeZoneInformation
		{
			#region Private Members

			private TimeZoneDefinition[] _timeZoneDefinitions;
			private int _version;
			private DateTime _createDate;

			internal const int CurrentVersion = 1;

			#endregion //Private Members

			#region Constructors

			internal TimeZoneInformation(TimeZoneDefinition[] timeZoneDefinitions)
				: this(timeZoneDefinitions, CurrentVersion, DateTime.Now)
			{
			}

			internal TimeZoneInformation(TimeZoneDefinition[] timeZoneDefinitions, int version, DateTime createDate)
			{
				this._timeZoneDefinitions = timeZoneDefinitions;
				this._version = version;
				this._createDate = createDate;
			}

			#endregion //Constructors

			#region Properties

			public DateTime CreateDate { get { return this._createDate; } }
			public TimeZoneDefinition[] TimeZoneDefinitions { get { return this._timeZoneDefinitions; } }
			public int Version { get { return this._version; } }

			#endregion //Properties
		}

		#endregion //TimeZoneInformation internal class	
	
		#region TimeZoneInformationSerializationInfo Class

		/// <summary>
		/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a TimeZoneInformation.
		/// </summary>
		internal class TimeZoneInformationSerializationInfo : ObjectSerializationInfo
		{
			protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
			{
				return new PropertySerializationInfo[]
				{
					new PropertySerializationInfo( typeof( DateTime ), "CreateDate" ),
					new PropertySerializationInfo( typeof( TimeZoneDefinitionCollection ), "TimeZoneDefinitions" ),
					new PropertySerializationInfo( typeof( int ), "Version" ),
				};
			}

			public override Dictionary<string, object> Serialize(object obj)
			{
				TimeZoneInformation tzInfo = (TimeZoneInformation)obj;
				Dictionary<string, object> map = null;

				map = ScheduleUtilities.AddEntryHelper(map, "CreateDate", tzInfo.CreateDate);
				map = ScheduleUtilities.AddEntryHelper(map, "TimeZoneDefinitions", new TimeZoneDefinitionCollection(tzInfo.TimeZoneDefinitions));
				map = ScheduleUtilities.AddEntryHelper(map, "Version", tzInfo.Version);
				return map;
			}

			public override object Deserialize(Dictionary<string, object> values)
			{
				object v;

				DateTime createDate = DateTime.Now;

				if (values.TryGetValue("CreateDate", out v))
					createDate = (DateTime)v;

				int version = TimeZoneInformation.CurrentVersion;

				if (values.TryGetValue("ExportId", out v))
					version = (int)v;

				TimeZoneDefinitionCollection tzDefs = null;

				if (values.TryGetValue("TimeZoneDefinitions", out v))
					tzDefs = (TimeZoneDefinitionCollection)v;

				return new TimeZoneInformation(tzDefs != null ? tzDefs.ToArray<TimeZoneDefinition>() : new TimeZoneDefinition[0], version, createDate);
			}
		}

		#endregion // TimeZoneInformationSerializationInfo Class

		#region TimeZoneDefinition protected class

		/// <summary>
		/// A class that represents a specific time zone
		/// </summary>
		protected internal class TimeZoneDefinition
		{
			#region Private Members

			private TimeSpan _baseUtcOffset;
			private string _id;
			private string _exportId;
			private string _dayLightName;
			private string _displayName;
			private ScheduleResourceString _localizedDisplayName;
			private string _standardName;
			private bool _supportsDaylightSavingsTime;
			private ReadOnlyCollection<TimeAdjustmentRule> _adjustmentRules;

			#endregion //Private Members	
	
			#region Constructor

			internal TimeZoneDefinition(	string id,
								string exportId,
								TimeSpan baseUtcOffset,
								string dayLightName,
								string displayName,
								string standardName,
								bool supportsDaylightSavingsTime,
								TimeAdjustmentRule[] adjustmentRules)
			{
				this._id = id;
				this._exportId = exportId;
				this._baseUtcOffset = baseUtcOffset;
				this._dayLightName = dayLightName;
				this._displayName = displayName;
				this._standardName = standardName;
				this._supportsDaylightSavingsTime = supportsDaylightSavingsTime;
				this._adjustmentRules = new ReadOnlyCollection<TimeAdjustmentRule>(adjustmentRules);

				if (string.IsNullOrWhiteSpace(this._displayName))
				{
					this._localizedDisplayName = new ScheduleResourceString();
					this._localizedDisplayName.ResourceName = id;
				}

			}

			#endregion //Constructor	
	
			#region Properties

			#region Public Properties

			#region Rules

			/// <summary>
			/// Returns a read-only collection of adjustmentRules that determine when daylight savings offsets are active.
			/// </summary>
			public ReadOnlyCollection<TimeAdjustmentRule> AdjustmentRules { get { return _adjustmentRules; } }

			#endregion //Rules

			#region BaseUtcOffset

			/// <summary>
			/// Returns the base offset from UTC time (read-only)
			/// </summary>
			public TimeSpan BaseUtcOffset { get { return _baseUtcOffset; } }

			#endregion //BaseUtcOffset	

			#region DaylightIime

			/// <summary>
			/// Returns the name of this time zone when in daylight savings time (read-only)
			/// </summary>
			public string DaylightName { get { return _dayLightName; } }

			#endregion //DaylightIime

			#region DisplayName

			/// <summary>
			/// Returns the string to use when displaying this time zone (read-only)
			/// </summary>
			public string DisplayName
			{ 
				get 
				{
					if ( this._localizedDisplayName != null )
						return this._localizedDisplayName.Value;

					return this._displayName;
				} 
			}

			#endregion //DisplayName

			#region ExportId

			/// <summary>
			/// Returns the identifier to use this time zone when exporting from the ICalendar format. (read-only)
			/// </summary>
			public string ExportId { get { return _exportId; } }

			#endregion //ExportId	

			#region Id

			/// <summary>
			/// Returns the identifier for the time zone (read-only)
			/// </summary>
			public string Id { get { return _id; } }

			#endregion //Id	

			#region StandardName

			/// <summary>
			/// Returns the name of this time zone when not in daylight savings time (read-only)
			/// </summary>
			public string StandardName { get { return _standardName; } }

			#endregion //StandardName

			#region SupportsDaylightSavingsTime

			/// <summary>
			/// Returns true is this time zone supports daylight savings time (read-only)
			/// </summary>
			public bool SupportsDaylightSavingsTime { get { return _supportsDaylightSavingsTime; } }

			#endregion //SupportsDaylightSavingsTime

			#endregion //Public Properties
	
			#endregion //Properties

			#region Methods

			#region GetAmbiguousTimeOffsetsForRule

			internal TimeSpan[] GetAmbiguousTimeOffsetsForRule(TimeAdjustmentRule rule)
			{
				TimeSpan[] array = new TimeSpan[2];

				if (rule.Delta > TimeSpan.Zero)
				{
					array[0] = this.BaseUtcOffset;
					array[1] = this.BaseUtcOffset + rule.Delta;
				}
				else
				{
					array[0] = this.BaseUtcOffset + rule.Delta;
					array[1] = this.BaseUtcOffset;
				}

				return array;
			}

			#endregion //GetAmbiguousTimeOffsetsForRule

			#endregion //Methods
		}

		#endregion //TimeZoneDefinition public class	

		#region TimeZoneDefinitionSerializationInfo Class

		/// <summary>
		/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a TimeZoneDefinition.
		/// </summary>
		internal class TimeZoneDefinitionSerializationInfo : ObjectSerializationInfo
		{
			private bool _includeDisplayNames;

			internal TimeZoneDefinitionSerializationInfo( bool includeDisplayNames)
			{
				this._includeDisplayNames = includeDisplayNames;
			}

			protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
			{
				return new PropertySerializationInfo[]
				{
					new PropertySerializationInfo( typeof( string ), "Id" ),
					new PropertySerializationInfo( typeof( string ), "ExId" ),
					new PropertySerializationInfo( typeof( TimeSpan ), "Offset" ),
					new PropertySerializationInfo( typeof( string ), "Daylight" ),
					new PropertySerializationInfo( typeof( string ), "Display" ),
					new PropertySerializationInfo( typeof( string ), "Standard" ),
					new PropertySerializationInfo( typeof( bool ), "SupportsDST" ),
					new PropertySerializationInfo( typeof( Rules ), "Rules" ),
				};
			}

			public override Dictionary<string, object> Serialize(object obj)
			{
				TimeZoneDefinition tzDef = (TimeZoneDefinition)obj;
				Dictionary<string, object> map = null;

				map = ScheduleUtilities.AddEntryHelper(map, "Id", tzDef.Id);

				if (!CoreUtilities.IsValueEmpty( tzDef.ExportId ))
					map = ScheduleUtilities.AddEntryHelper(map, "ExId", tzDef.ExportId);

				map = ScheduleUtilities.AddEntryHelper(map, "Offset", tzDef.BaseUtcOffset);
				map = ScheduleUtilities.AddEntryHelper(map, "Daylight", tzDef.DaylightName);

				if ( this._includeDisplayNames )
					map = ScheduleUtilities.AddEntryHelper(map, "Display", tzDef.DisplayName);

				if ( tzDef.StandardName != tzDef.Id )
					map = ScheduleUtilities.AddEntryHelper(map, "Standard", tzDef.StandardName);

				if ( tzDef.AdjustmentRules.Count > 0)
					map = ScheduleUtilities.AddEntryHelper(map, "Rules", new Rules(tzDef.AdjustmentRules));

				if (tzDef.SupportsDaylightSavingsTime != tzDef.AdjustmentRules.Count > 0)
					map = ScheduleUtilities.AddEntryHelper(map, "SupportsDST", tzDef.SupportsDaylightSavingsTime);

				return map;
			}

			public override object Deserialize(Dictionary<string, object> values)
			{
				object v;

				string id = null;

				if (values.TryGetValue("Id", out v))
					id = (string)v;

				string exportId = null;

				if (values.TryGetValue("ExId", out v))
					exportId = (string)v;

				TimeSpan baseUtcOffset = new TimeSpan();

				if (values.TryGetValue("Offset", out v))
					baseUtcOffset = (TimeSpan)v;

				string daylightName = null;

				if (values.TryGetValue("Daylight", out v))
					daylightName = (string)v;

				string displayName = null;

				if (this._includeDisplayNames && values.TryGetValue("Display", out v))
					displayName = (string)v;

				string standardName = null;

				if (values.TryGetValue("Standard", out v))
					standardName = (string)v;
				else
					standardName = id;

				bool? supportsDaylightSavingsTime = null;

				if (values.TryGetValue("SupportsDST", out v))
					supportsDaylightSavingsTime = (bool)v;

				Rules adjustmentRules = null;

				if (values.TryGetValue("Rules", out v))
					adjustmentRules = (Rules)v;

				if ( !supportsDaylightSavingsTime.HasValue )
					supportsDaylightSavingsTime = adjustmentRules != null && adjustmentRules.Count > 0;

				return new TimeZoneDefinition(id, 
									exportId, 
									baseUtcOffset, 
									daylightName, 
									displayName, 
									standardName, 
									supportsDaylightSavingsTime.Value, 
									adjustmentRules != null
										? adjustmentRules.ToArray<TimeAdjustmentRule>()
										: new TimeAdjustmentRule[0]);
			}
		}

		#endregion // TimeZoneDefinitionSerializationInfo Class

		#region TimeAdjustmentRule

		/// <summary>
		/// A class that describes the rule for when to transition into and out of daylight savings time
		/// </summary>
		public sealed class TimeAdjustmentRule : IEquatable<TimeZoneInfoProvider.TimeAdjustmentRule>
		{
			#region Private Members

			private DateTime _start;
			private DateTime _end;
			private TimeSpan _delta;
			private TransitionTime _transitionStart;
			private TransitionTime _transitionEnd;

			#endregion //Private Members

			#region Static Create Methods

			/// <summary>
			/// Creates and returns an instance of a TimeAdjustmentRule
			/// </summary>
			/// <param name="start">The start of the dateTime range where this tt is in force.</param>
			/// <param name="end">The end of the dateTime range where this tt is in force.</param>
			/// <param name="delta">The time adjustment to apply between the transition start and end while this tt is in force.</param>
			/// <param name="transitionStart">The start dateTime tt that idenifies when to transition to the adjusted time.</param>
			/// <param name="transitionEnd">The end dateTime tt that idenifies when to transition back to standard time.</param>
			/// <returns></returns>
			public static TimeAdjustmentRule Create(DateTime start, DateTime end, TimeSpan delta, TransitionTime transitionStart, TransitionTime transitionEnd)
			{
				Validate(start, end, delta, transitionStart, transitionEnd);

				TimeAdjustmentRule rule = new TimeAdjustmentRule();
				rule._start = start;
				rule._end = end;
				rule._delta = delta;
				rule._transitionStart = transitionStart;
				rule._transitionEnd = transitionEnd;
				return rule;
			}


			internal static TimeAdjustmentRule FromOSRule(TimeZoneInfo.AdjustmentRule osRule)
			{
				TimeAdjustmentRule rule = new TimeAdjustmentRule();
				rule._start = osRule.DateStart;
				rule._end = osRule.DateEnd;
				rule._delta = osRule.DaylightDelta;
				rule._transitionStart = TransitionTime.FromOSTransitionTime(osRule.DaylightTransitionStart);
				rule._transitionEnd = TransitionTime.FromOSTransitionTime(osRule.DaylightTransitionEnd);
				return rule;
			}

			internal static TimeAdjustmentRule[] FromOSRuleArray(TimeZoneInfo.AdjustmentRule[] osRules)
			{
				int count = osRules != null ? osRules.Length : 0;

				TimeAdjustmentRule[] rules = new TimeAdjustmentRule[count];

				for (int i = 0; i < count; i++)
					rules[i] = FromOSRule(osRules[i]);

				return rules;
			}


			#endregion //Static Create Methods

			#region Base class overrides

			#region Equals

			/// <summary>
			/// Returns true if the object is equilavent to this
			/// </summary>
			public override bool Equals(object obj)
			{
				return obj is TimeAdjustmentRule && this.Equals((TimeAdjustmentRule)obj);
			}

			#endregion //Equals

			#region GetHashCode

			/// <summary>
			/// Returns a code to use for hashing
			/// </summary>
			public override int GetHashCode()
			{
				return this._start.GetHashCode();
			}

			#endregion //GetHashCode

			#region Operator Overloads
			/// <summary>
			/// Compares the values of two <see cref="TimeAdjustmentRule"/> structures for equality
			/// </summary>
			/// <param name="rule1">The first structure</param>
			/// <param name="rule2">The other structure</param>
			/// <returns>true if the two instances are equal; otherwise false</returns>
			public static bool operator ==(TimeAdjustmentRule rule1, TimeAdjustmentRule rule2)
			{
				if (Object.ReferenceEquals(rule1, null))
					return Object.ReferenceEquals(rule2, null);

				return rule1.Equals(rule2);
			}

			/// <summary>
			/// Compares the values of two <see cref="TimeAdjustmentRule"/> structures for inequality
			/// </summary>
			/// <param name="rule1">The first structure</param>
			/// <param name="rule2">The other structure</param>
			/// <returns>true if the two instances are not equal; otherwise false</returns>
			public static bool operator !=(TimeAdjustmentRule rule1, TimeAdjustmentRule rule2)
			{
				if (Object.ReferenceEquals(rule1, null))
					return !Object.ReferenceEquals(rule2, null);

				return !(rule1.Equals(rule2));
			}
			#endregion //Operator Overloads

			#endregion //Base class overrides

			#region Properties

			#region Delta

			/// <summary>
			/// Returns the time adjustment to apply between the transition start and end while this tt is in force. (read-only)
			/// </summary>
			public TimeSpan Delta { get { return _delta; } }

			#endregion //Delta

			#region End

			/// <summary>
			/// Returns the end of the dateTime range where this tt is in force (read-only)
			/// </summary>
			public DateTime End { get { return _end; } }

			#endregion //End

			#region Start

			/// <summary>
			/// Returns the start of the dateTime range where this tt is in force (read-only)
			/// </summary>
			public DateTime Start { get { return _start; } }

			#endregion //Start

			#region TransitionEnd

			/// <summary>
			/// Returns the end dateTime tt that idenifies when to transition back to standard time. (read-only)
			/// </summary>
			public TransitionTime TransitionEnd { get { return _transitionEnd; } }

			#endregion //TransitionEnd

			#region TransitionStart

			/// <summary>
			/// Returns the start dateTime tt that idenifies when to transition to the adjusted time. (read-only)
			/// </summary>
			public TransitionTime TransitionStart { get { return _transitionStart; } }

			#endregion //TransitionStart

			#endregion //Properties

			#region Methods

			#region Internal Methods

			#region GetDaylightTimeForYear

			internal DaylightTimeForYear GetDaylightTimeForYear(int year)
			{
				return new DaylightTimeForYear(TransitionTime.ToDateTimeForYear(year, this._transitionStart),
												TransitionTime.ToDateTimeForYear(year, this._transitionEnd),
												this._delta);

			}

			#endregion //GetDaylightTimeForYear

			#endregion //Internal Methods

			#region Validate

			private static void Validate(DateTime start, DateTime end, TimeSpan delta, TransitionTime transitionStart, TransitionTime transitionEnd)
			{
				if (start.Kind != DateTimeKind.Unspecified)
					throw new ArgumentException(ScheduleUtilities.GetString("LE_MustBeUnspecified"), "start");// "Kind must be unspecified."

				if (end.Kind != DateTimeKind.Unspecified)
					throw new ArgumentException(ScheduleUtilities.GetString("LE_MustBeUnspecified"), "end");// "Kind must be unspecified."

				if (start >= end)
					throw new ArgumentException(ScheduleUtilities.GetString("LE_EndBeforeStart")); // "end must be after start"

				if (!TimeZoneInfoProvider.IsOffsetValid(delta))
					throw new ArgumentOutOfRangeException(ScheduleUtilities.GetString("LE_InavlidDelta"));// "invalid delta"

				if (delta.Ticks % 600000000 != (long)0)
					throw new ArgumentOutOfRangeException(ScheduleUtilities.GetString("LE_DeltaHasSeconds")); // "delta has seconds"

				if (start.TimeOfDay != TimeSpan.Zero)
					throw new ArgumentException(ScheduleUtilities.GetString("LE_HasTimeOfDay"), "start"); // "date has time of day"

				if (end.TimeOfDay != TimeSpan.Zero)
					throw new ArgumentException(ScheduleUtilities.GetString("LE_HasTimeOfDay"), "end"); // "date has time of day"

			}

			#endregion //Validate

			#endregion //Methods

			#region IEquatable<TimeAdjustmentRule> Members

			/// <summary>
			/// Returns true if both objects are equal
			/// </summary>
			/// <param name="other">The other object</param>
			/// <returns>Ture if equal, otherwise false</returns>
			public bool Equals(TimeAdjustmentRule other)
			{
				if (other == null)
					return false;

				return other._start == _start &&
						other._end == _end &&
						other._delta == _delta &&
						other._transitionStart == _transitionStart &&
						other._transitionEnd == _transitionEnd;
			}

			#endregion
		}

		#endregion //TimeAdjustmentRule	

		#region TimeAdjustmentRuleSerializationInfo Class

		/// <summary>
		/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a TimeAdjustmentRule.
		/// </summary>
		internal class TimeAdjustmentRuleSerializationInfo : ObjectSerializationInfo
		{
			protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
			{
				return new PropertySerializationInfo[]
				{
					new PropertySerializationInfo( typeof( DateTime ), "Start" ),
					new PropertySerializationInfo( typeof( DateTime ), "End" ),
					new PropertySerializationInfo( typeof( TimeSpan ), "Delta" ),
					new PropertySerializationInfo( typeof( TransitionTime ), "TrStart" ),
					new PropertySerializationInfo( typeof( TransitionTime ), "TrEnd" ),
				};
			}

			public override Dictionary<string, object> Serialize(object obj)
			{
				TimeAdjustmentRule rule = (TimeAdjustmentRule)obj;
				Dictionary<string, object> map = null;

				map = ScheduleUtilities.AddEntryHelper(map, "Start", rule.Start);
				map = ScheduleUtilities.AddEntryHelper(map, "End", rule.End);
				map = ScheduleUtilities.AddEntryHelper(map, "Delta", rule.Delta);
				map = ScheduleUtilities.AddEntryHelper(map, "TrStart", rule.TransitionStart);
				map = ScheduleUtilities.AddEntryHelper(map, "TrEnd", rule.TransitionEnd);
				return map;
			}

			public override object Deserialize(Dictionary<string, object> values)
			{
				object v;

				DateTime start = new DateTime(1, 1, 1, 0, 0, 0);

				if (values.TryGetValue("Start", out v))
					start = (DateTime)v;

				DateTime end = new DateTime(1, 1, 1, 0, 0, 0);

				if (values.TryGetValue("End", out v))
					end = (DateTime)v;

				TimeSpan delta = new TimeSpan();

				if (values.TryGetValue("Delta", out v))
					delta = (TimeSpan)v;

				TransitionTime transitionStart;
				if (values.TryGetValue("TrStart", out v))
					transitionStart = (TransitionTime)v;
				else
					throw new ArgumentException(ScheduleUtilities.GetString("LE_AdjustRuleMustHave", "TransitionStart")); // "Adjustment transition tinme must have a {0}"

				TransitionTime transitionEnd;
				if (values.TryGetValue("TrEnd", out v))
					transitionEnd = (TransitionTime)v;
				else
					throw new ArgumentException(ScheduleUtilities.GetString("LE_AdjustRuleMustHave", "TransitionEnd")); // "Adjustment transition tinme must have a {0}"

				return TimeAdjustmentRule.Create(start, end, delta, transitionStart, transitionEnd);
			}
		}

		#endregion // TimeAdjustmentRuleSerializationInfo Class

		#region TransitionTime struct

		/// <summary>
		/// Represents a tt for transitioning to or from daylight savings times
		/// </summary>
		public struct TransitionTime : IEquatable<TimeZoneInfoProvider.TransitionTime>
		{
			#region Private Members

			private byte _day;
			private DayOfWeek _dayOfWeek;
			private bool _isFixedDate;
			private byte _month;
			private byte _week;
			private DateTime _timeOfDay;

			#endregion //Private Members

			#region Static Create Methods

			/// <summary>
			/// A static menthod that creates and returns a transition time that occurs on the same dateTime every year.
			/// </summary>
			/// <param name="day">A number between 1 and 31</param>
			/// <param name="month">A number between 1 and 12</param>
			/// <param name="timeOfDay">The time that the transition takes place.</param>
			/// <returns>The newly created instance of a <see cref="TransitionTime"/></returns>
			public static TransitionTime CreateFixedDateTransitionTime(byte day, byte month, DateTime timeOfDay)
			{
				return CreateTransitionTime(day, 1, month, DayOfWeek.Monday, true, timeOfDay);
			}

			/// <summary>
			/// A static menthod that creates and returns a transition time that occurs on a different dateTime every year based on day of week and a relative week number.
			/// </summary>
			/// <param name="week">A number between 1 and 5</param>
			/// <param name="month">A number between 1 and 12</param>
			/// <param name="dayOfWeek">An enumeration that identifies a specific day from 'Sunday' thru 'Saturday'.</param>
			/// <param name="timeOfDay">The time that the transition takes place.</param>
			/// <returns>The newly created instance of a <see cref="TransitionTime"/></returns>
			public static TransitionTime CreateFloatingDateTransitionTime(byte week, byte month, DayOfWeek dayOfWeek, DateTime timeOfDay)
			{
				return CreateTransitionTime(1, week, month, dayOfWeek, false, timeOfDay);
			}

			private static TransitionTime CreateTransitionTime(byte day, byte week, byte month, DayOfWeek dayOfWeek, bool isFixedDate, DateTime timeOfDay)
			{
				Validate(day, week, month, dayOfWeek, timeOfDay);

				TransitionTime tt = new TransitionTime();
				tt._day = day;
				tt._week = week;
				tt._month = month;
				tt._dayOfWeek = dayOfWeek;
				tt._isFixedDate = isFixedDate;
				tt._timeOfDay = timeOfDay;
				return tt;
			}

			internal static TransitionTime FromOSTransitionTime(TimeZoneInfo.TransitionTime osTransition)
			{
				TransitionTime tt = new TransitionTime();
				tt._day = (byte)osTransition.Day;
				tt._week =  (byte)osTransition.Week;
				tt._month = (byte)osTransition.Month;
				tt._dayOfWeek = osTransition.DayOfWeek;
				tt._isFixedDate = osTransition.IsFixedDateRule;
				tt._timeOfDay = osTransition.TimeOfDay;
				return tt;
			}


			#endregion //Static Create Methods

			#region Base class overrides

			#region Equals

			/// <summary>
			/// Returns true if the object is equilavent to this
			/// </summary>
			public override bool Equals(object obj)
			{
				return obj is TransitionTime && this.Equals((TransitionTime)obj);
			}

			#endregion //Equals

			#region GetHashCode

			/// <summary>
			/// Returns a code to use for hashing
			/// </summary>
			public override int GetHashCode()
			{
				return (_month ^ (_week << 8));
			}

			#endregion //GetHashCode

			#region Operator Overloads
			/// <summary>
			/// Compares the values of two <see cref="TransitionTime"/> structures for equality
			/// </summary>
			/// <param name="transition1">The first structure</param>
			/// <param name="transition2">The other structure</param>
			/// <returns>true if the two instances are equal; otherwise false</returns>
			public static bool operator ==(TransitionTime transition1, TransitionTime transition2)
			{
				return transition1.Equals(transition2);
			}

			/// <summary>
			/// Compares the values of two <see cref="TransitionTime"/> structures for inequality
			/// </summary>
			/// <param name="transition1">The first structure</param>
			/// <param name="transition2">The other structure</param>
			/// <returns>true if the two instances are not equal; otherwise false</returns>
			public static bool operator !=(TransitionTime transition1, TransitionTime transition2)
			{
				return !(transition1.Equals(transition2));
			}
			#endregion //Operator Overloads

			#endregion //Base class overrides

			#region Properties

			#region Day

			/// <summary>
			/// Returns the day of the month (read-only)
			/// </summary>
			/// <remarks>
			/// <para class="note"><b>Note</b>: the day number is ignored if <see cref="IsFixedDate"/> is false.</para>
			/// </remarks>
			/// <value>A number from 1 to 31</value>
			public byte Day { get { return _day; } }

			#endregion //Day

			#region DayOfWeek

			/// <summary>
			/// Returns the day of the week (read-only)
			/// </summary>
			/// <remarks>
			/// <para class="note"><b>Note</b>: the day of the week is ignored if <see cref="IsFixedDate"/> is true.</para>
			/// </remarks>
			/// <value>An enumeration from 'Sunday' thru "Saturday'</value>
			public DayOfWeek DayOfWeek { get { return _dayOfWeek; } }

			#endregion //DayOfWeek

			#region IsFixedDate

			/// <summary>
			/// Returns whether the transition occurs on a speciic day number (read-only)
			/// </summary>
			/// <remarks>
			/// <para class="note"><b>Note</b>: if this is true then <see cref="Day"/> is used and <see cref="Week"/> and <see cref="DayOfWeek"/> are ignored. Otherwise, Day is ignored and Week and DayOfWeek are used.</para>
			/// </remarks>
			/// <value>True to use <see cref="Day"/> or false to use <see cref="Week"/> and <see cref="DayOfWeek"/>.</value>
			public bool IsFixedDate { get { return _isFixedDate; } }

			#endregion //IsFixedDate

			#region Month

			/// <summary>
			/// Returns the month of the year (read-only)
			/// </summary>
			/// <value>A number from 1 to 12</value>
			public byte Month { get { return _month; } }

			#endregion //Month

			#region TimeOfDay

			/// <summary>
			/// Returns the time of day when the transition wil take place (read-only)
			/// </summary>
			/// <value>A datetime that contains only hours, minutes and seconds</value>
			public DateTime TimeOfDay { get { return _timeOfDay; } }

			#endregion //TimeOfDay

			#region Week

			/// <summary>
			/// Returns the week number relative to the month (read-only)
			/// </summary>
			/// <remarks>
			/// <para class="note"><b>Note</b>: the week number is ignored if <see cref="IsFixedDate"/> is true.</para>
			/// </remarks>
			/// <value>A number from 1 to 5</value>
			public byte Week { get { return _week; } }

			#endregion //Week

			#endregion //Properties

			#region Methods

			#region Internal Methods

			#region ToDateTimeForYear

			internal static DateTime ToDateTimeForYear(int year, TransitionTime tt)
			{
				DateTime tod = tt.TimeOfDay;
				int month = tt.Month;

				if (tt.IsFixedDate)
				{
					int daysInMonth = DateTime.DaysInMonth(year, month);

					return new DateTime(year, month, Math.Min(tt.Day, daysInMonth), tod.Hour, tod.Minute, tod.Second, tod.Millisecond);
				}

				DateTime dateTime;
				int dayOffset;
				int week = tt.Week;
				int dayOfWk = (int)tt.DayOfWeek;

				if (week > 4)
				{
					int lastDayInMonth = DateTime.DaysInMonth(year, month);

					dateTime = new DateTime(year, month, lastDayInMonth, tod.Hour, tod.Minute, tod.Second, tod.Millisecond);

					dayOffset = (int)dateTime.DayOfWeek - dayOfWk;

					if (dayOffset < 0)
						dayOffset += 7;

					if (dayOffset > 0)
						dateTime = dateTime.AddDays(-dayOffset);

					return dateTime;
				}

				dateTime = new DateTime(year, month, 1, tod.Hour, tod.Minute, tod.Second, tod.Millisecond);

				dayOffset = dayOfWk - (int)dateTime.DayOfWeek;

				if (dayOffset < 0)
					dayOffset += 7;

				// adjust for weeks 2 thru 4
				if (week > 1)
					dayOffset += (week - 1) * 7;

				if (dayOffset > 0)
					dateTime = dateTime.AddDays(dayOffset);

				return dateTime;
			}

			#endregion //ToDateTimeForYear

			#endregion //Internal Methods
					
			#region Private Methods

			#region Validate

			private static void Validate(byte day, byte week, byte month, DayOfWeek dayOfWeek, DateTime timeOfDay)
			{
				if (month < 1 || month > 12)
					throw new ArgumentOutOfRangeException("month");

				if (day < 1 || day > 31)
					throw new ArgumentOutOfRangeException("day");

				if (week < 1 || week > 5)
					throw new ArgumentOutOfRangeException("week");

				if (!Enum.IsDefined(typeof(DayOfWeek), dayOfWeek))
					throw new ArgumentOutOfRangeException("day of week");

				if (timeOfDay.Kind != DateTimeKind.Unspecified)
					throw new ArgumentException(ScheduleUtilities.GetString("LE_MustBeUnspecified"), "timeofDay"); //" must be unspecified kind");

				if (timeOfDay.Year != 1 || timeOfDay.Month != 1 || timeOfDay.Day != 1 || (timeOfDay.Ticks % (long)10000) != (long)0)
					throw new ArgumentException(ScheduleUtilities.GetString("LE_TimeShouldHaveTicks"), "timeofDay"); // "timeofDay should not have tick component");

			}

			#endregion //Validate

			#endregion //Private Methods	
	
			#endregion //Methods

			#region IEquatable<TransitionTime> Members

			#region Equals

			/// <summary>
			/// Returns true if the object is equilavent to this
			/// </summary>
			public bool Equals(TransitionTime other)
			{
				if (other._isFixedDate != _isFixedDate)
					return false;

				if (other._month != _month)
					return false;

				if (other._timeOfDay != _timeOfDay)
					return false;

				if (_isFixedDate)
					return other._day == _day;
				else
					return other._dayOfWeek == _dayOfWeek && other._week == _week;
			}

			#endregion //Equals

			#endregion
		}

		#endregion //TransitionTime struct

		#region TransitionTimeSerializationInfo Class

		/// <summary>
		/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a TransitionTime.
		/// </summary>
		internal class TransitionTimeSerializationInfo : ObjectSerializationInfo
		{
			protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties()
			{
				return new PropertySerializationInfo[]
				{
					new PropertySerializationInfo( typeof( byte ), "Day" ),
					new PropertySerializationInfo( typeof( byte ), "DayOfWeek" ),
					new PropertySerializationInfo( typeof( bool ), "Fixed" ),
					new PropertySerializationInfo( typeof( byte ), "Month" ),
					new PropertySerializationInfo( typeof( byte ), "Week" ),
					new PropertySerializationInfo( typeof( DateTime ), "Time" ),
				};
			}

			public override Dictionary<string, object> Serialize(object obj)
			{
				TransitionTime transitionTime = (TransitionTime)obj;
				Dictionary<string, object> map = null;

				if ( transitionTime.Day > 1 )
					map = ScheduleUtilities.AddEntryHelper(map, "Day", transitionTime.Day);

				if ( transitionTime.DayOfWeek != DayOfWeek.Sunday )
					map = ScheduleUtilities.AddEntryHelper(map, "DayOfWeek", (byte)transitionTime.DayOfWeek);

				if ( transitionTime.IsFixedDate )
					map = ScheduleUtilities.AddEntryHelper(map, "Fixed", transitionTime.IsFixedDate);

				if ( transitionTime.Month > 1 )
					map = ScheduleUtilities.AddEntryHelper(map, "Month", transitionTime.Month);

				if ( transitionTime.Week != 1 )
					map = ScheduleUtilities.AddEntryHelper(map, "Week", transitionTime.Week);

				map = ScheduleUtilities.AddEntryHelper(map, "Time", transitionTime.TimeOfDay);
				return map;
			}

			public override object Deserialize(Dictionary<string, object> values)
			{
				object v;
				byte day = 1;
				if (values.TryGetValue("Day", out v))
					day = (byte)v;

				DayOfWeek dayOfWeek = DayOfWeek.Sunday;

				if (values.TryGetValue("DayOfWeek", out v))
					dayOfWeek = (DayOfWeek)(byte)v;

				bool isFixedDate = false;
				if (values.TryGetValue("Fixed", out v))
					isFixedDate = (bool)v;

				byte week = 1;
				if (values.TryGetValue("Week", out v))
					week = (byte)v;

				byte month = 1;
				if (values.TryGetValue("Month", out v))
					month = (byte)v;

				DateTime timeOfDay = new DateTime(1, 1, 1, 0, 0, 0);

				if (values.TryGetValue("Time", out v))
					timeOfDay = (DateTime)v;


				return isFixedDate
					? TransitionTime.CreateFixedDateTransitionTime(day, month, timeOfDay)
					: TransitionTime.CreateFloatingDateTransitionTime(week, month, dayOfWeek, timeOfDay);
			}
		}

		#endregion // TransitionTimeSerializationInfo Class

		#region TimeZoneDefinitionCollection class

		internal class TimeZoneDefinitionCollection : List<TimeZoneDefinition>
		{
			public TimeZoneDefinitionCollection() { }
			public TimeZoneDefinitionCollection(IEnumerable<TimeZoneDefinition> collection) : base(collection) { }
		}

		#endregion //TimeZoneDefinitionCollection

		#region Rules collection class

		internal class Rules : List<TimeAdjustmentRule>
		{
			public Rules() { }
			public Rules(IEnumerable<TimeAdjustmentRule> collection) : base(collection) { }
		}

		#endregion //Rules collection class	
	
		#endregion //Nested Classes and Structures

		#region Constructor

		/// <summary>
		/// Creates a new instance of a <see cref="TimeZoneInfoProvider"/> derived class
		/// </summary>
		protected TimeZoneInfoProvider()
		{
			_tokens = new ObservableCollectionExtended<TimeZoneToken>();
			_timeZoneTokens = new ReadOnlyObservableCollection<TimeZoneToken>(_tokens);

			_tokens.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnTokensCollectionChanged);
		}

		#endregion //Constructor	
	
		#region Properties

		#region Public Properties

		#region DefaultProvider

		/// <summary>
		/// Returns a static instance of a TimeZoneInfoProvider pre-loaded with default time zone information 
		/// </summary>
		public static TimeZoneInfoProvider DefaultProvider
		{
			get
			{

				return OSTimeZoneInfoProvider.DefaultProviderInternal;



			}
		}

		#endregion //DefaultProvider

		#region LocalTimeZoneId

		/// <summary>
		/// Identifies the <see cref="LocalTimeZoneId"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LocalTimeZoneIdProperty = DependencyPropertyUtilities.Register("LocalTimeZoneId",
			typeof(string), typeof(TimeZoneInfoProvider),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnLocalTimeZoneIdChanged))
			);

		private static void OnLocalTimeZoneIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeZoneInfoProvider instance = (TimeZoneInfoProvider)d;

			instance._localTimeZoneId = e.NewValue as string;

			instance._localToken = null;
			
			ScheduleUtilities.NotifyListenersHelper(instance, e, instance.PropChangeListeners, false, false);
			
			instance.BumpVersion();

		}

		/// <summary>
		/// Returns or sets the Id for the local time zone
		/// </summary>
		/// <seealso cref="LocalTimeZoneIdProperty"/>
		public string LocalTimeZoneId
		{
			get
			{
				return this._localTimeZoneId;
			}
			set
			{
				this.SetValue(TimeZoneInfoProvider.LocalTimeZoneIdProperty, value);
			}
		}

		#endregion //LocalTimeZoneId
			
		#region LocalTimeZoneIdResolved

		/// <summary>
		/// Returns the id for the local time zone (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if <see cref="LocalTimeZoneId"/> is set its value will be returned. If it is not set an attempt will be made to deduce it.</para>
		/// </remarks>
		public virtual string LocalTimeZoneIdResolved
		{
			get
			{
				this.VerifyAccess();

				if (!string.IsNullOrWhiteSpace(this._localTimeZoneId))
					return this._localTimeZoneId;







				return TimeZoneInfo.Local.Id;



			}
		}

		#endregion //LocalTimeZoneIdResolved

		#region LocalToken

		/// <summary>
		/// Returns the token that represents the local time zone
		/// </summary>
		public TimeZoneToken LocalToken
		{
			get
			{
				this.VerifyAccess();

				if (this._localToken == null)
				{
					string id = this.LocalTimeZoneIdResolved;

					if (!CoreUtilities.IsValueEmpty(id))
						this.TryGetTimeZoneToken(id, out this._localToken);
				}

				return this._localToken;
			}
		}

		#endregion //LocalToken

		#region TimeZoneTokens

		private ReadOnlyObservableCollection<TimeZoneToken> _timeZoneTokens;

		/// <summary>
		/// Returns a read-only collection of time zone tokens
		/// </summary>
		public ReadOnlyObservableCollection<TimeZoneToken> TimeZoneTokens
		{
			get
			{
				return _timeZoneTokens;
			}
		}

		#endregion //TimeZoneTokens
	
		#region UtcToken

		/// <summary>
		/// Returns the token that represents the UTC time zone
		/// </summary>
		public abstract TimeZoneToken UtcToken { get; }

		#endregion //UtcToken	

		#region Version

		private static readonly DependencyPropertyKey VersionPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Version",
			typeof(int), typeof(TimeZoneInfoProvider),
			0,
			new PropertyChangedCallback(OnVersionChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="Version"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VersionProperty = VersionPropertyKey.DependencyProperty;

		private static void OnVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeZoneInfoProvider instance = (TimeZoneInfoProvider)d;

			ScheduleUtilities.NotifyListenersHelper(instance, e, instance.PropChangeListeners, false, false);

		}

		/// <summary>
		/// Returns a version number that gets bumped when any change is made that affects time zone information
		/// </summary>
		/// <seealso cref="VersionProperty"/>
		public int Version
		{
			get
			{
				return (int)this.GetValue(TimeZoneInfoProvider.VersionProperty);
			}
			internal set
			{
				this.SetValue(TimeZoneInfoProvider.VersionPropertyKey, value);
			}
		}

		#endregion //Version
	
		#endregion //Public Properties	

		#region Protected Properties

		#region Tokens

		/// <summary>
		/// Returns a collection of tokens.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: it is the repsonsibility of the derived classes to maintain this collection</para>
		/// </remarks>
		protected ObservableCollectionExtended<TimeZoneToken> Tokens
		{
			get
			{
				return _tokens;
			}
		}

		#endregion // Tokens

		#endregion //Protected Properties	

		#region Internal Properties

		#region IsValid

		internal bool IsValid
		{
			get { return this.LocalToken != null && this.UtcToken != null; }
		}

		#endregion //IsValid	
    
		#region PropChangeListeners

		/// <summary>
		/// Gets collection of property change listeners.
		/// </summary>
		internal PropertyChangeListenerList PropChangeListeners
		{
			get
			{
				if (null == _propChangeListeners)
					_propChangeListeners = new PropertyChangeListenerList();

				return _propChangeListeners;
			}
		}

		#endregion // PropChangeListeners

		#endregion //Internal Properties	
		
		#endregion //Properties	
	
		#region Methods

		#region Public Methods

		#region ClearCachedValues

		/// <summary>
		/// Clears any values that have been cached so they can be re-created when needed
		/// </summary>
		public virtual void ClearCachedValues()
		{
			this.VerifyAccess();

			this._timeZoneIdMap.Clear();
			this._timeZoneDisplayNameMap.Clear();
			this._localToken = null;

			this.BumpVersion();
		}

		#endregion //ClearCachedValues

		#region ConvertLocalToUtc
		/// <summary>
		/// Converts the specified local time to universal time.
		/// </summary>
		/// <param name="dateTime">The local datetime to convert</param>
		/// <returns>The universal datetime</returns>
		public DateTime ConvertLocalToUtc(DateTime dateTime)
		{
			Debug.Assert(dateTime.Kind != DateTimeKind.Utc, "DateTime.Kind expected to be unspecified");
			return this.ConvertTime(this.LocalToken, this.UtcToken, dateTime);
		}
		#endregion // ConvertLocalToUtc

		#region ConvertTime

		/// <summary>
		/// Converts a DateTime from one time zone to another
		/// </summary>
		/// <param name="source">The time zone of the passed in dateTime time.</param>
		/// <param name="destination">The time zone to convert the dateTime time to.</param>
		/// <param name="dateTime">The dateTime time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract DateTime ConvertTime(TimeZoneToken source, TimeZoneToken destination, DateTime dateTime);

		/// <summary>
		/// Converts a DateTime from one time zone to another
		/// </summary>
		/// <param name="source">The time zone of the passed in dateTime time.</param>
		/// <param name="destination">The time zone to convert the dateTime time to.</param>
		/// <param name="dateTimeOffset">The dateTime time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract DateTimeOffset ConvertTime(TimeZoneToken source, TimeZoneToken destination, DateTimeOffset dateTimeOffset);

		#endregion //ConvertTime

		#region ConvertUtcToLocal
		/// <summary>
		/// Converts the specified universal time to local time.
		/// </summary>
		/// <param name="dateTime">The universal datetime to convert</param>
		/// <returns>The local datetime</returns>
		public DateTime ConvertUtcToLocal(DateTime dateTime)
		{
			//Debug.Assert(dateTime.Kind == DateTimeKind.Utc, "Date expected to be a UTC date");
			return this.ConvertTime(this.UtcToken, this.LocalToken, dateTime);
		}
		#endregion // ConvertUtcToLocal

		#region GetAdjustmentRules

		/// <summary>
		/// Returns an array of adjustment rules for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract TimeAdjustmentRule[] GetAdjustmentRules(TimeZoneToken token);

		#endregion //GetAdjustmentRules

		#region GetAmbiguousTimeOffsets

		/// <summary>
		/// Gets the ambiguous time offsets for a time when a daylight time transition is occuring
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The dateTime time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		/// <exception cref="ArgumentException">If DateTime is not ambiguous.</exception>
		/// <seealso cref="IsAmbiguousTime(TimeZoneToken,DateTime)"/>
		public abstract TimeSpan[] GetAmbiguousTimeOffsets(TimeZoneToken token, DateTime dateTime);

		/// <summary>
		/// Gets the ambiguous time offsets for a time when a daylight time transition is occuring
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTimeOffset">The dateTime time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		/// <exception cref="ArgumentException">If DateTime is not ambiguous.</exception>
		/// <seealso cref="IsAmbiguousTime(TimeZoneToken,DateTimeOffset)"/>
		public abstract TimeSpan[] GetAmbiguousTimeOffsets(TimeZoneToken token, DateTimeOffset dateTimeOffset);

		#endregion //GetAmbiguousTimeOffsets

		#region GetBaseUtcOffset

		/// <summary>
		/// Returns the base offset to utc time (when not in daylight saving time)
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <returns>A TimeSpan with the UTC offset.</returns>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract TimeSpan GetBaseUtcOffset(TimeZoneToken token);

		#endregion //GetBaseUtcOffset

		#region GetDaylightName

		/// <summary>
		/// Returns the display name for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract string GetDaylightName(TimeZoneToken token);

		#endregion //GetDaylightName

		#region GetDisplayName

		/// <summary>
		/// Returns the display name for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract string GetDisplayName(TimeZoneToken token);

		#endregion //GetDisplayName

		#region GetExportId

		/// <summary>
		/// Returns the export id for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <returns>The export id or null.</returns>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract string GetExportId(TimeZoneToken token);

		#endregion //GetExportId

		#region GetStandardName

		/// <summary>
		/// Returns the display name for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract string GetStandardName(TimeZoneToken token);

		#endregion //GetStandardName
	
		#region GetTimeZoneToken

		/// <summary>
		/// Returns the <see cref="TimeZoneToken"/> for the time zone
		/// </summary>
		/// <param name="id">A string that identifies a specific time zone</param>
		public TimeZoneToken GetTimeZoneToken(string id)
		{
			return this.GetTimeZoneToken(id, true);
		}

		#endregion //GetTimeZoneToken

		#region GetUtcOffsetFromUtcTime

		/// <summary>
		/// Returns the offset to convert a dateTime time to Utc time
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The specific dateTime time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract TimeSpan GetUtcOffset(TimeZoneToken token, DateTime dateTime);

		/// <summary>
		/// Returns the offset to convert a dateTime time offset to Utc time
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTimeOffset">The specific dateTime time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract TimeSpan GetUtcOffset(TimeZoneToken token, DateTimeOffset dateTimeOffset);

		#endregion //GetUtcOffsetFromUtcTime

		#region IsAmbiguousTime

		/// <summary>
		/// Determines if a DateTime is ambiguous, i.e. it is within a daylight time transition
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The dateTime time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract bool IsAmbiguousTime(TimeZoneToken token, DateTime dateTime);

		/// <summary>
		/// Determines if a DateTime is ambiguous, i.e. it is within a daylight time transition
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTimeOffset">The dateTime time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract bool IsAmbiguousTime(TimeZoneToken token, DateTimeOffset dateTimeOffset);

		#endregion //IsAmbiguousTime

		#region IsDaylightSavingsTime

		/// <summary>
		/// Determines if a DateTime is within a daylight savings adjustment period
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The dateTime time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract bool IsDaylightSavingsTime(TimeZoneToken token, DateTime dateTime);

		/// <summary>
		/// Determines if a DateTime is within a daylight savings adjustment period
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTimeOffset">The dateTime time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract bool IsDaylightSavingsTime(TimeZoneToken token, DateTimeOffset dateTimeOffset);

		#endregion //IsDaylightSavingsTime

		#region IsInvalidTime

		/// <summary>
		/// Determines if a DateTime is invalid
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The dateTime time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract bool IsInvalidTime(TimeZoneToken token, DateTime dateTime);

		#endregion //IsInvalidTime

		#region IsValidTimeZoneId

		/// <summary>
		/// Determines if a string is a valid time zone id
		/// </summary>
		/// <param name="id">The id to verify.</param>
		/// <returns>True if the id is valid, otherwise false</returns>
		public abstract bool IsValidTimeZoneId(string id);

		#endregion //IsValidTimeZoneId

		#region SaveToXml

		/// <summary>
		/// Saves all the time zone definition information as xml
		/// </summary>
		/// <returns>A string containing the saved xml</returns>
		public string SaveToXml()
		{
			return this.SaveToXml(true);
		}

		/// <summary>
		/// Saves all the time zone definition information as xml
		/// </summary>
		/// <param name="includeDisplayNames">Determines if time zone display names will be included in the xml.</param>
		/// <returns>A string containing the saved xml</returns>
		public string SaveToXml(bool includeDisplayNames)
		{
			this.VerifyAccess();

			AttributeValueParser avp = new AttributeValueParser();
			ObjectSerializer serializer = new ObjectSerializer(avp);

			RegisterSerializationInfo(serializer, includeDisplayNames);

			ReadOnlyCollection<TimeZoneDefinition> tzDefs = this.GetTimeZoneDefinitions();
			TimeZoneInformation tzInfo = new TimeZoneInformation(tzDefs.ToArray<TimeZoneDefinition>());

			string xml;
			bool succeeded = serializer.SaveAsXmlFragment(tzInfo, out xml);

			return xml;
		}
		#endregion //SaveToXml	
	
		#region SupportsDayLightSavingsTime

		/// <summary>
		/// Returns true if the time zone supports daylight savings time
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <returns>True if the time zone supports daylight savings time, otherwise false.</returns>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public abstract bool SupportsDayLightSavingsTime(TimeZoneToken token);

		#endregion //SupportsDayLightSavingsTime
	
		#region TryGetTimeZoneToken

		/// <summary>
		/// Tries to get the <see cref="TimeZoneToken"/> for the time zone safely
		/// </summary>
		/// <param name="id">A string that identifies a specific time zone</param>
		/// <param name="token">An out parameter which will be set to the associated token.</param>
		/// <returns>True if the id was valid and the token was set, otherwise false.</returns>
		public bool TryGetTimeZoneToken(string id, out TimeZoneToken token)
		{
			this.VerifyAccess();

			if (!IsValidTimeZoneId(id))
			{
				token = null;
				return false;
			}

			token = this.GetTimeZoneToken(id, false);

			return token != null;
		}

		#endregion //TryGetTimeZoneToken

		#endregion //Public Methods

		#region Protected Methods

		#region BumpVersion

		/// <summary>
		/// Should be called whenever any time zone information has changed
		/// </summary>
		protected virtual void BumpVersion()
		{
			this.VerifyAccess();

			this.Version += 1;
		}

		#endregion //BumpVersion	
	
		#region ConvertTime

		/// <summary>
		/// Converts a DateTime from one time zone to another
		/// </summary>
		/// <param name="dateTime">The source date time</param>
		/// <param name="sourceTimeZone">The source time zone</param>
		/// <param name="destinationTimeZone">The destination time zone.</param>
		/// <returns>The DateTime in the destination time zone.</returns>
		protected DateTime ConvertTime(DateTime dateTime, TimeZoneDefinition sourceTimeZone, TimeZoneDefinition destinationTimeZone)
		{
			CoreUtilities.ValidateNotNull(sourceTimeZone, "sourceTimeZone");
			CoreUtilities.ValidateNotNull(destinationTimeZone, "destinationTimeZone");

			TimeZoneToken sourceToken = this.GetTimeZoneToken(sourceTimeZone.Id);
			TimeZoneToken destinationToken = this.GetTimeZoneToken(destinationTimeZone.Id);

			if (sourceToken == null)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "sourceTimeZone"); // "token not found");

			if (destinationToken == null)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "destinationTimeZone"); // "token not found");

			DateTimeKind sourceKind = this.GetDateTimeKind(sourceToken);
			DateTimeKind destinationKind = this.GetDateTimeKind(destinationToken);

			dateTime = EnsureKindNotLocal(dateTime);

			// If the source kind is not UTC but the datetimekind is then first convert the time
			// into the ource time from UTC
			if (dateTime.Kind == DateTimeKind.Utc &&
				sourceKind != DateTimeKind.Utc)
				dateTime = this.ConvertTime(this.UtcToken, sourceToken, dateTime);

			if (dateTime.Kind != DateTimeKind.Unspecified && sourceKind != DateTimeKind.Unspecified && sourceKind == destinationKind)
				return dateTime;

			TimeAdjustmentRule rule = this.GetRuleForTime(dateTime, sourceTimeZone);
			TimeSpan baseOffset = sourceTimeZone.BaseUtcOffset;

			if (rule != null)
			{
				DaylightTimeForYear daylightTime = rule.GetDaylightTimeForYear(dateTime.Year);

				if (IsInDaylightSavingsTime(dateTime, rule, daylightTime))
					baseOffset += rule.Delta;
			}

			long ticks = dateTime.Ticks - baseOffset.Ticks;

			bool isAmbiguous;
			bool isDaylightSavings;

			DateTime dt = this.ConvertUtcTicksToTimeZone(ticks, destinationTimeZone, out isDaylightSavings, out isAmbiguous);

			return new DateTime(dt.Ticks, destinationKind);
		}

		#endregion //ConvertTime	
	
		#region CreateTimeZoneDefinition

		/// <summary>
		/// Creates and returns an instance of a <see cref="TimeZoneDefinition"/> 
		/// </summary>
		/// <param name="token">The token that represents the time zone.</param>
		/// <param name="exportId">The export id</param>
		/// <param name="baseUtcOffset">The offset from UTC to use when not in daylight savings time.</param>
		/// <param name="dayLightName">The name of the time zone in daylight savings time</param>
		/// <param name="displayName">The display name of the time zone</param>
		/// <param name="standardName">The name of the time zone in standard time</param>
		/// <param name="supportsDaylightSavingsTime">Whether this time zone supports daylight savings.</param>
		/// <param name="adjustmentRules">The adjustment rules for daylight savings</param>
		/// <returns>The new instance of a <see cref="TimeZoneDefinition"/> </returns>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		protected TimeZoneDefinition CreateTimeZoneDefinition(TimeZoneToken token,
													string exportId,
													TimeSpan baseUtcOffset,
													string dayLightName,
													string displayName,
													string standardName,
													bool supportsDaylightSavingsTime,
													TimeAdjustmentRule[] adjustmentRules)
		{
			VerifyToken(token);

			TimeZoneDefinition tzi = new TimeZoneDefinition(token.Id, exportId, baseUtcOffset, dayLightName, displayName, standardName, supportsDaylightSavingsTime, adjustmentRules);

			token.TimeZoneDefinition = tzi;

			return tzi;
		}

		#endregion //CreateTimeZoneDefinition

		#region CreateToken

		/// <summary>
		/// Creates an returns a <see cref="TimeZoneToken"/>
		/// </summary>
		/// <param name="id">The id of the time zone.</param>
		/// <returns>A token representing the specified id</returns>
		protected TimeZoneToken CreateToken(string id)
		{
			VerifyAccess();

			return new TimeZoneToken(id, this);
		}

		#endregion //CreateToken	
    
		#region GetAmbiguousTimeOffsets

		/// <summary>
		/// Gets the ambiguous time offsets for a time when a daylight time transition is occuring
		/// </summary>
		/// <param name="dateTime">The date time and offset</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		protected TimeSpan[] GetAmbiguousTimeOffsets(DateTime dateTime, TimeZoneDefinition timeZoneDef)
		{
			CoreUtilities.ValidateNotNull(timeZoneDef, "timeZoneDef");

			if (!timeZoneDef.SupportsDaylightSavingsTime)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TimeNotAmbiguous"), "dateTime"); // "Date time is not ambiguous"

			dateTime = EnsureKindNotLocal(dateTime);

			switch (dateTime.Kind)
			{
				case DateTimeKind.Utc:
					{
						TimeZoneToken token = this.GetTimeZoneToken(timeZoneDef.Id);
						if (token == null)
							throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "timeZoneDef");

						if (GetDateTimeKind(token) != DateTimeKind.Utc)
						{
							TimeZoneToken utcToken = this.UtcToken;
							if (utcToken == null)
								throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "UTC");

							dateTime = ConvertTime(utcToken, token, dateTime);
						}
					}
					break;

				case DateTimeKind.Unspecified:
				default:
					break;	
			}

			bool isAmbiguous = false;

			TimeAdjustmentRule rule = GetRuleForTime(dateTime, timeZoneDef);

			if (rule != null)
			{
				DaylightTimeForYear dlt = rule.GetDaylightTimeForYear(dateTime.Year);

				isAmbiguous = IsAmbiguous(dateTime, rule, dlt);
			}

			if (!isAmbiguous)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TimeNotAmbiguous"), "dateTime"); // "Date time is not ambiguous"


			return timeZoneDef.GetAmbiguousTimeOffsetsForRule(rule);
		}

		/// <summary>
		/// Gets the ambiguous time offsets for a time when a daylight time transition is occuring
		/// </summary>
		/// <param name="dateTimeOffset">The date time and offset</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		protected TimeSpan[] GetAmbiguousTimeOffsets(DateTimeOffset dateTimeOffset, TimeZoneDefinition timeZoneDef)
		{
			CoreUtilities.ValidateNotNull(timeZoneDef, "timeZoneDef");

			if (!timeZoneDef.SupportsDaylightSavingsTime)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TimeNotAmbiguous"), "dateTimeOffset"); // "Date time is not ambiguous"
			
			TimeZoneToken token = this.GetTimeZoneToken(timeZoneDef.Id);
			if (token == null)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "timeZoneDef"); 


			DateTime dt = ConvertTime(token, token, dateTimeOffset).DateTime;

			bool isAmbiguous = false;

			TimeAdjustmentRule rule = GetRuleForTime(dt, timeZoneDef);

			if (rule != null)
			{
				DaylightTimeForYear dlt = rule.GetDaylightTimeForYear(dt.Year);

				isAmbiguous = IsAmbiguous(dt, rule, dlt);
			}

			if (!isAmbiguous)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TimeNotAmbiguous"), "dateTime"); // "Date time is not ambiguous"


			return timeZoneDef.GetAmbiguousTimeOffsetsForRule(rule);

		}

		#endregion //GetAmbiguousTimeOffsets	
	
		#region GetTimeZoneDefinitions

		/// <summary>
		/// Returns a read-only collection of <see cref="TimeZoneDefinition"/> objects
		/// </summary>
		protected abstract ReadOnlyCollection<TimeZoneDefinition> GetTimeZoneDefinitions();

		#endregion //GetTimeZoneDefinitions

		#region GetTimeZoneToken

		/// <summary>
		/// Returns the <see cref="TimeZoneToken"/> for the time zone
		/// </summary>
		/// <param name="id">A string that identifies a specific time zone</param>
		/// <param name="verifyIdIsValid">True to make sure the id is already defined.</param>
		protected TimeZoneToken GetTimeZoneToken(string id, bool verifyIdIsValid)
		{
			CoreUtilities.ValidateNotEmpty(id, "id");

			TimeZoneToken tzid = null;
			
			this.VeriftTokenIdMap();

			if (!this._timeZoneIdMap.TryGetValue(id, out tzid))
			{
				if (verifyIdIsValid)
				{
					if (!IsValidTimeZoneId(id))
						throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound", id), "id");
				}
			}

			return tzid;

		}

		#endregion //GetTimeZoneToken

		#region GetUtcOffset

		/// <summary>
		/// Gets the Utc offset for a specific date time 
		/// </summary>
		/// <param name="dateTimeOffset">The specific date time</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		/// <returns>The offset from Utc time that applies to this date time</returns>
		protected TimeSpan GetUtcOffset(DateTimeOffset dateTimeOffset, TimeZoneDefinition timeZoneDef)
		{
			return GetUtcOffsetForDateTime(dateTimeOffset.UtcDateTime, timeZoneDef);
		}

		#endregion //GetUtcOffset	
	
		#region GetUtcOffsetForDateTime

		/// <summary>
		/// Gets the Utc offset for a specific date time 
		/// </summary>
		/// <param name="dateTime">The specific date time</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		/// <returns>The offset from Utc time that applies to this date time</returns>
		protected TimeSpan GetUtcOffsetForDateTime(DateTime dateTime, TimeZoneDefinition timeZoneDef)
		{
			CoreUtilities.ValidateNotNull(timeZoneDef, "timeZoneDef");

			dateTime = EnsureKindNotLocal(dateTime);

			switch (dateTime.Kind)
			{
				case DateTimeKind.Utc:
					{
						TimeZoneToken token = this.GetTimeZoneToken(timeZoneDef.Id);
						if (token == null)
							throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "timeZoneDef"); 

						if (GetDateTimeKind(token) == DateTimeKind.Utc)
							return timeZoneDef.BaseUtcOffset;

						bool dummy1, dummy2;

						return GetUtcOffsetFromUtcTime(dateTime, timeZoneDef, out dummy1, out dummy2);
					}


				case DateTimeKind.Unspecified:
				default:
					return GetUtcOffset(dateTime, timeZoneDef);
			}
		}

		#endregion //GetUtcOffsetForDateTime	

		#region IsAmbiguous

		/// <summary>
		/// Returns whether a time is ambiguous, i.e. it it at a daylight savings transition
		/// </summary>
		/// <param name="dateTime">The specific date time</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		protected bool IsAmbiguous(DateTime dateTime, TimeZoneDefinition timeZoneDef)
		{
			CoreUtilities.ValidateNotNull(timeZoneDef, "timeZoneDef");

			if (!timeZoneDef.SupportsDaylightSavingsTime)
				return false;

			dateTime = EnsureKindNotLocal(dateTime);
			
			TimeAdjustmentRule rule = GetRuleForTime(dateTime, timeZoneDef );

			if ( rule == null )
				return false;

			switch (dateTime.Kind)
			{
				case DateTimeKind.Utc:
					{
						TimeZoneToken token = this.GetTimeZoneToken(timeZoneDef.Id);
						if (token == null)
							throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "timeZoneDef"); 

						if (GetDateTimeKind(token) != DateTimeKind.Utc)
						{
							TimeZoneToken utcToken = this.UtcToken;
							if (utcToken == null)
								throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "UTC"); 

							dateTime = ConvertTime(utcToken, token, dateTime);
						}
					}
					break;

			}

			DaylightTimeForYear dlt = rule.GetDaylightTimeForYear(dateTime.Year);

			return IsAmbiguous(dateTime, rule, dlt);
		}
		
		/// <summary>
		/// Returns whether a time is ambiguous, i.e. it it at a daylight savings transition
		/// </summary>
		/// <param name="dateTimeOffset">The specific date time</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		protected bool IsAmbiguous(DateTimeOffset dateTimeOffset, TimeZoneDefinition timeZoneDef)
		{
			CoreUtilities.ValidateNotNull(timeZoneDef, "timeZoneDef");

			if (!timeZoneDef.SupportsDaylightSavingsTime)
				return false;

			DateTimeOffset dtOffset = ConvertDateTimeOffset(dateTimeOffset, timeZoneDef, timeZoneDef);

			return IsAmbiguous(dtOffset, timeZoneDef);
		}

		#endregion //IsAmbiguous

		#region IsDaylightSavingsTime

		/// <summary>
		/// Determines if a DateTime is within a daylight savings adjustment period
		/// </summary>
		/// <param name="dateTime">The specific date time</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		protected bool IsDaylightSavingsTime(DateTime dateTime, TimeZoneDefinition timeZoneDef)
		{
			CoreUtilities.ValidateNotNull(timeZoneDef, "timeZoneDef");

			if (!timeZoneDef.SupportsDaylightSavingsTime)
				return false;

			if (timeZoneDef.AdjustmentRules.Count == 0)
				return false;

			dateTime = EnsureKindNotLocal(dateTime);

			switch (dateTime.Kind)
			{
				case DateTimeKind.Utc:
					{
						TimeZoneToken token = this.GetTimeZoneToken(timeZoneDef.Id);
						if (token == null)
							throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "timeZoneDef"); 
						
						if (GetDateTimeKind(token) == DateTimeKind.Utc)
							return false;

						bool isDaylightSavings;
						bool dummy;
						GetUtcOffsetFromUtcTime(dateTime, timeZoneDef, out isDaylightSavings, out dummy);

						return isDaylightSavings;
					}
			}

			TimeAdjustmentRule rule = GetRuleForTime(dateTime, timeZoneDef );

			if ( rule == null )
				return false;

			DaylightTimeForYear dlt = rule.GetDaylightTimeForYear(dateTime.Year);

			return IsInDaylightSavingsTime(dateTime, rule, dlt);
		}

		/// <summary>
		/// Determines if a DateTime is within a daylight savings adjustment period
		/// </summary>
		/// <param name="dateTimeOffset">The specific date time</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		protected bool IsDaylightSavingsTime(DateTimeOffset dateTimeOffset, TimeZoneDefinition timeZoneDef)
		{
			CoreUtilities.ValidateNotNull(timeZoneDef, "timeZoneDef");

			if (!timeZoneDef.SupportsDaylightSavingsTime)
				return false;

			if (timeZoneDef.AdjustmentRules.Count == 0)
				return false;

			bool dummy;
			bool isDaylightTime;

			GetUtcOffsetFromUtcTime(dateTimeOffset.UtcDateTime, timeZoneDef, out isDaylightTime, out dummy);

			return isDaylightTime;
		}

		#endregion //IsDaylightSavingsTime

		#region IsInvalidTime

		/// <summary>
		/// Determines if a DateTime is invalid
		/// </summary>
		/// <param name="dateTime">The date time</param>
		/// <param name="timeZoneDef">The time zone the date time is relative to</param>
		protected bool IsInvalidTime(DateTime dateTime, TimeZoneDefinition timeZoneDef)
		{
			CoreUtilities.ValidateNotNull(timeZoneDef, "timeZoneDef");
		
			TimeZoneToken token = this.GetTimeZoneToken(timeZoneDef.Id);
			if (token == null)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "timeZoneDef"); 

			DateTimeKind kind = dateTime.Kind;

			if ( kind != DateTimeKind.Unspecified && 
				(kind != DateTimeKind.Local || GetDateTimeKind(token) != DateTimeKind.Local))
				return false;

			dateTime = EnsureKindNotLocal(dateTime);

			TimeAdjustmentRule rule = GetRuleForTime(dateTime, timeZoneDef);

			if (rule == null)
				return false;

			DaylightTimeForYear dlt = rule.GetDaylightTimeForYear(dateTime.Year);

			return IsInvalidTime(dateTime, rule, dlt);
		}

		#endregion //IsInvalidTime
	
		#endregion //Protected Methods	
		
		#region Internal Methods

		#region EnsureKindNotLocal

		internal static DateTime EnsureKindNotLocal(DateTime dateTime)
		{
			if (dateTime.Kind != DateTimeKind.Local)
				return dateTime;

			return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc);
		}

		#endregion //EnsureKindNotLocal	
		
		#region GetDaylightTime

		internal DaylightTimeForYear GetDaylightTime(TimeZoneToken token, DateTime dateTime)
		{
			CoreUtilities.ValidateNotNull(token, "token");

			DateTimeKind kind = dateTime.Kind;

			if (kind != DateTimeKind.Unspecified &&
				(kind != DateTimeKind.Local || GetDateTimeKind(token) != DateTimeKind.Local))
				return null;

			dateTime = EnsureKindNotLocal(dateTime);

			TimeAdjustmentRule rule = GetRuleForTime(dateTime, token);

			if (rule == null)
				return null;

			return rule.GetDaylightTimeForYear(dateTime.Year);
		}

		#endregion //GetDaylightTime

		#region IsOffsetValid

		internal static bool IsOffsetValid(TimeSpan offset)
		{
			double hours = offset.TotalHours;

			return hours < 14 && hours > -14;
		}

		#endregion //IsOffsetValid

		#region OnDefaultLocalTimeZomeChanged

		internal void OnDefaultLocalTimeZomeChanged()
		{


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			// JJD 6/7/11 - TFS74310
			// Call BumpVersion asynchronously to allow the prompt dialog to close before
			// triggering any processing
			//this.BumpVersion();
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.BumpVersion));
		}

		#endregion //OnDefaultLocalTimeZomeChanged	
    
		#region RegisterSerializationInfo

		internal static void RegisterSerializationInfo(ObjectSerializer serializer, bool includeDispayNames)
		{
			serializer.RegisterInfo(typeof(TimeZoneInformation), new TimeZoneInformationSerializationInfo());
			serializer.RegisterInfo(typeof(TimeZoneDefinitionCollection), new CollectionSerializationInfo<TimeZoneDefinitionCollection, TimeZoneDefinition>());
			serializer.RegisterInfo(typeof(TimeZoneDefinition), new TimeZoneDefinitionSerializationInfo(includeDispayNames));
			serializer.RegisterInfo(typeof(TimeAdjustmentRule), new TimeAdjustmentRuleSerializationInfo());
			serializer.RegisterInfo(typeof(Rules), new CollectionSerializationInfo<Rules, TimeAdjustmentRule>());
			serializer.RegisterInfo(typeof(TimeZoneInfoProvider.TransitionTime), new TransitionTimeSerializationInfo());
		}

		#endregion //RegisterSerializationInfo	

		#region TryGetTimeZoneTokenByDisplayName

		internal bool TryGetTimeZoneTokenByDisplayName(string displayName, out TimeZoneToken token)
		{
			this.VeriftTokenIdMap();
			return this._timeZoneDisplayNameMap.TryGetValue(displayName, out token);
		}

		#endregion  // TryGetTimeZoneTokenByDisplayName

		#region VerifyAccess



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


		#endregion //VerifyAccess

		#region VerifyToken

		internal void VerifyToken(TimeZoneToken token)
		{
			this.VerifyAccess();

			CoreUtilities.ValidateNotNull(token, "token");

			if (token.Provider != this)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFromProvier", token.Id), "token"); // "Token was not created by this TimeZonoInfoProvider"
		}

		#endregion //VerifyToken

		#endregion //Internal Methods	

		#region Private Methods

		#region CreateDateTimeFromTicks

		private static DateTime CreateDateTimeFromTicks(long ticks)
		{
			if (ticks > DateTime.MaxValue.Ticks)
				return DateTime.MaxValue;

			if (ticks < DateTime.MinValue.Ticks)
				return DateTime.MinValue;

			return new DateTime(ticks);
		}

		#endregion //CreateDateTimeFromTicks	
		
		#region ConvertDateTimeOffset

		private DateTimeOffset ConvertDateTimeOffset(DateTimeOffset dateTimeOffset, TimeZoneDefinition sourceTimeZone, TimeZoneDefinition destinationTimeZone)
		{
			CoreUtilities.ValidateNotNull(sourceTimeZone, "sourceTimeZone");
			CoreUtilities.ValidateNotNull(destinationTimeZone, "destinationTimeZone");

			bool dummy1, dummy2;

			DateTime utcDate = dateTimeOffset.UtcDateTime;
			TimeSpan offsetFromUtc = GetUtcOffsetFromUtcTime(utcDate, destinationTimeZone, out dummy1, out dummy2);

			return CreateDateTimeFromTicks(utcDate.Ticks + offsetFromUtc.Ticks);
		}

		#endregion //ConvertDateTimeOffset	
		
		#region ConvertUtcTicksToTimeZone

		private DateTime ConvertUtcTicksToTimeZone(long ticks, TimeZoneDefinition destinationTimeZone, out bool isDaylightSavings, out bool isAmbiguous)
		{
			TimeSpan offset = GetUtcOffsetFromUtcTime(CreateDateTimeFromTicks(ticks), destinationTimeZone, out isDaylightSavings, out isAmbiguous);

			return CreateDateTimeFromTicks(ticks + offset.Ticks);
		}

		#endregion //ConvertUtcTicksToTimeZone	

		#region GetDateTimeKind

		private DateTimeKind GetDateTimeKind(TimeZoneToken token)
		{
			// MD 6/21/11 - TFS77450
			// A DateTimeKind of Local is reserved for the local time on the system. But our LocalToken may not match the
			// one on the system. So we should be returning Unspecified when it matches the LocalToken.
			//if (token == this.LocalToken)
			//    return DateTimeKind.Local;

			if (token == this.UtcToken)
				return DateTimeKind.Utc;

			return DateTimeKind.Unspecified;
		}

		#endregion //GetDateTimeKind
 
		#region GetRuleForTime

		private TimeAdjustmentRule GetRuleForTime(DateTime dateTime, TimeZoneToken token)
		{
			CoreUtilities.ValidateNotNull(token, "timeZoneToken");

			TimeAdjustmentRule[] rules = this.GetAdjustmentRules(token);
			
			return GetRuleForTime(dateTime, rules.ToArray<TimeAdjustmentRule>());
		}

		private TimeAdjustmentRule GetRuleForTime(DateTime dateTime, TimeZoneDefinition timeZone)
		{
			CoreUtilities.ValidateNotNull(timeZone, "timeZone");

			TimeZoneToken token = this.GetTimeZoneToken(timeZone.Id);

			if (token == null)
				throw new ArgumentException(ScheduleUtilities.GetString("LE_TokenNotFound"), "timeZone"); 

			ReadOnlyCollection<TimeAdjustmentRule> rules = timeZone.AdjustmentRules;

			return GetRuleForTime(dateTime, rules.ToArray<TimeAdjustmentRule>());
		}

		private static TimeAdjustmentRule GetRuleForTime(DateTime dateTime, TimeAdjustmentRule[] rules)
		{
			int count = rules.Length;

			if (count < 1)
				return null;

			DateTime dateOnly = dateTime.Date;

			for (int i = 0; i < count; i++)
			{
				TimeAdjustmentRule rule = rules[i];

				if (rule.Start <= dateOnly && rule.End >= dateOnly)
					return rule;
			}

			return null;
		}

		#endregion //GetRuleForTime

		#region GetUtcOffset

		private TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneDefinition timeZoneDef)
		{
			TimeSpan baseOffset = timeZoneDef.BaseUtcOffset;
			TimeAdjustmentRule rule = GetRuleForTime(dateTime, timeZoneDef);

			if (rule != null )
			{
				DaylightTimeForYear dlt = rule.GetDaylightTimeForYear(dateTime.Year);
				bool isInDaylightSavings = IsInDaylightSavingsTime(dateTime, rule, dlt);

				if (isInDaylightSavings)
					baseOffset += rule.Delta;
			}

			return baseOffset;
		}

		#endregion //GetUtcOffset

		#region GetUtcOffsetHelper

		private TimeSpan GetUtcOffsetHelper(DateTime dateTime, TimeZoneDefinition timeZoneDef)
		{
			TimeSpan baseOffset = timeZoneDef.BaseUtcOffset;
			TimeAdjustmentRule rule = GetRuleForTime(dateTime, timeZoneDef);

			if (rule != null )
			{
				DaylightTimeForYear dlt = rule.GetDaylightTimeForYear(dateTime.Year);
				bool isInDaylightSavings = IsInDaylightSavingsTime(dateTime, rule, dlt);

				if (isInDaylightSavings)
					baseOffset += rule.Delta;
			}

			return baseOffset;
		}

		#endregion //GetUtcOffsetHelper

		#region GetUtcOffsetFromUtcTime

		private TimeSpan GetUtcOffsetFromUtcTime(DateTime dateTime, TimeZoneDefinition timeZoneDef, out bool isInDaylightSavings, out bool isAmbiguousWhenOffset)
		{
			isInDaylightSavings = false;
			isAmbiguousWhenOffset = false;
			TimeSpan baseOffset = timeZoneDef.BaseUtcOffset;
			DateTime dtForRule;
			TimeAdjustmentRule rule;
			int year;

			if (dateTime > new DateTime(9999, 12, 31))
			{
				year = 9999;
				dtForRule = DateTime.MaxValue;
			}
			else if (dateTime < new DateTime(1, 1, 2))
			{
				year = 1;
				dtForRule = DateTime.MinValue;
			}
			else
			{
				dtForRule = dateTime + baseOffset;
				year = dateTime.Year;
			}

			rule = GetRuleForTime(dtForRule, timeZoneDef);

			if (rule != null )
			{
				isInDaylightSavings = IsInDaylightSavingsTimeFromUtc(dateTime, rule, year, baseOffset, out isAmbiguousWhenOffset);

				if (isInDaylightSavings)
					baseOffset += rule.Delta;
			}

			return baseOffset;
		}

		#endregion //GetUtcOffsetFromUtcTime

		#region IsAmbiguous

		private static bool IsAmbiguous(DateTime dateTime, TimeAdjustmentRule rule, DaylightTimeForYear daylight)
		{
			if (rule == null || rule.Delta == TimeSpan.Zero)
				return false;

			DateTime ambiguousStart;
			DateTime ambiguousEnd;

			if (rule.Delta > TimeSpan.Zero)
			{
				ambiguousEnd = daylight.End;
				ambiguousStart = ambiguousEnd - rule.Delta;
			}
			else
			{
				ambiguousEnd = daylight.Start;
				ambiguousStart = ambiguousEnd + rule.Delta;
			}

			bool inAmbiguousRange = IsInDateInRange(dateTime, ambiguousStart, ambiguousEnd, 0);

			if (inAmbiguousRange || ambiguousStart.Year == ambiguousEnd.Year)
				return inAmbiguousRange;


			return IsInDateInRange(dateTime, ambiguousStart, ambiguousEnd, 1) ||
					IsInDateInRange(dateTime, ambiguousStart, ambiguousEnd, -1);
		}

		#endregion //IsAmbiguous	

		#region IsInDateInRange

		private static bool IsInDateInRange(DateTime dateTime, DateTime rangeStart, DateTime rangeEnd, int rangeYearDelta)
		{
			try
			{
				if (rangeYearDelta != 0)
				{
					rangeStart = rangeStart.AddYears(rangeYearDelta);
					rangeEnd = rangeEnd.AddYears(rangeYearDelta);
				}

				return dateTime >= rangeStart && dateTime < rangeEnd;
			}
			catch (ArgumentOutOfRangeException) { }

			return false;
		}

		#endregion //IsInDateRange

		#region IsInDateInTransitionRange

		private static bool IsInDateInTransitionRange(DateTime date, DateTime transitionStart, DateTime transitionEnd)
		{
			transitionStart = SynchronizeYear(transitionStart, date);
			transitionEnd = SynchronizeYear(transitionEnd, date);

			if (transitionStart < transitionEnd)
				return date >= transitionStart && date < transitionEnd;
			else
				return date >= transitionStart || date < transitionEnd;
		}

		#endregion //IsInDateInTransitionRange	

		#region IsInDaylightSavingsTime

		private static bool IsInDaylightSavingsTime(DateTime dateTime, TimeAdjustmentRule rule, DaylightTimeForYear daylight)
		{
			if (rule == null)
				return false;

			DateTime startTime;
			DateTime endTime;
			DateTimeKind kind = dateTime.Kind;

			if (kind != DateTimeKind.Local)
			{
				if (rule.Delta > TimeSpan.Zero)
				{
					startTime = daylight.Start + rule.Delta;
					endTime = daylight.End - rule.Delta;
				}
				else
				{
					startTime = daylight.Start;
					endTime = daylight.End;
				}
			}
			else
			{
				startTime = daylight.Start + daylight.Delta;
				endTime = daylight.End;
			}

			bool isInDstRange = IsInDateInTransitionRange(dateTime, startTime, endTime);

			if (isInDstRange &&
				kind == DateTimeKind.Local &&
				IsAmbiguous(dateTime, rule, daylight))
			{
				isInDstRange = TimeZoneInfo.Local.IsAmbiguousTime(dateTime);
			}

			return isInDstRange;
		}

		#endregion //IsInDaylightSavingsTime	
	
		#region IsInDaylightSavingsTimeFromUtc

		private static bool IsInDaylightSavingsTimeFromUtc(DateTime dateTime, TimeAdjustmentRule rule, int year, TimeSpan utcOffset, out bool isAmbiguousWhenOffset)
		{
			isAmbiguousWhenOffset = false;

			if (rule == null || rule.Delta == TimeSpan.Zero)
				return false;

			TimeSpan ruleDelta = rule.Delta;

			DaylightTimeForYear daylightTime = rule.GetDaylightTimeForYear(year);
			DateTime startTime = daylightTime.Start - utcOffset;
			DateTime endTime = daylightTime.End - (utcOffset + ruleDelta);
			DateTime ambiguousStart, ambiguousEnd;

			if (daylightTime.Delta.Ticks > 0)
			{
				ambiguousEnd = endTime;
				ambiguousStart = ambiguousEnd - daylightTime.Delta;
			}
			else
			{
				ambiguousStart = startTime;
				ambiguousEnd = ambiguousStart - daylightTime.Delta;
			}

			bool isInDstRange = IsInDateInTransitionRange(dateTime, startTime, endTime);

			if (isInDstRange)
			{
				isAmbiguousWhenOffset = IsInDateInRange(dateTime, ambiguousStart, ambiguousEnd, 0);

				if (isAmbiguousWhenOffset || ambiguousStart.Year == ambiguousEnd.Year)
					return true;


				isAmbiguousWhenOffset = IsInDateInRange(dateTime, ambiguousStart, ambiguousEnd, 1) ||
										IsInDateInRange(dateTime, ambiguousStart, ambiguousEnd, -1);
			}

			return isInDstRange;
		}

		#endregion //IsInDaylightSavingsTimeFromUtc	
	
		#region IsInvalidTime

		private static bool IsInvalidTime(DateTime dateTime, TimeAdjustmentRule rule, DaylightTimeForYear daylight)
		{
			if (rule == null || rule.Delta == TimeSpan.Zero)
				return false;

			DateTime startTime;
			DateTime endTime;
			DateTimeKind kind = dateTime.Kind;

			if (rule.Delta > TimeSpan.Zero)
			{
				startTime = daylight.Start;
				endTime = daylight.Start + rule.Delta;
			}
			else
			{
				startTime = daylight.End;
				endTime = daylight.End - rule.Delta;
			}

			bool isInValidRange = IsInDateInRange(dateTime, startTime, endTime, 0);

			if ( isInValidRange || startTime.Year == endTime.Year)
				return isInValidRange;

			return IsInDateInRange(dateTime, startTime, endTime, 1) ||
					IsInDateInRange(dateTime, startTime, endTime, -1);
		}

		#endregion //IsInValidTime

		#region OnTokensCollectionChanged

		private void OnTokensCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this._timeZoneIdMap.Clear();
			this._timeZoneDisplayNameMap.Clear();
		}

		#endregion //OnTokensCollectionChanged	
		
		#region SynchronizeYear

		private static DateTime SynchronizeYear(DateTime date, DateTime syncFromDate)
		{
			if (date.Year == syncFromDate.Year)
				return date;
			else
				return date.AddYears(syncFromDate.Year - date.Year);
		}

		#endregion //SynchronizeYear	

		#region VeriftTokenIdMap

		private void VeriftTokenIdMap()
		{
			if (this._timeZoneIdMap.Count == this._tokens.Count)
				return;

			this._timeZoneIdMap.Clear();
			this._timeZoneDisplayNameMap.Clear();

			foreach (TimeZoneToken token in this._tokens)
			{
				this._timeZoneIdMap[token.Id] = token;
				this._timeZoneDisplayNameMap[token.DisplayName] = token;
			}
		}

		#endregion //VeriftTokenIdMap	
	
		#endregion //Private Methods	
					
		#endregion //Methods

		#region ITypedSupportPropertyChangeNotifications<object,string> Members

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener(ITypedPropertyChangeListener<object, string> listener, bool useWeakReference)
		{
			this.PropChangeListeners.Add(listener, useWeakReference);
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener(ITypedPropertyChangeListener<object, string> listener)
		{
			this.PropChangeListeners.Remove(listener);
		}

		#endregion
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