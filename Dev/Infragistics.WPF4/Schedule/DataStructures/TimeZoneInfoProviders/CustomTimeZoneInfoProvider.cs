using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Resources;
using System.Windows;
using System.IO;







using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// A class used to load custom time zone information for date time conversions to and from utc date times.
	/// </summary>
	public class CustomTimeZoneInfoProvider : TimeZoneInfoProvider
	{
		#region Private Members

		private Dictionary<string, TimeZoneDefinition> _timeZoneDefinitionMap = new Dictionary<string, TimeZoneDefinition>();
		private List<TimeZoneDefinition> _timeZoneDefinitions = new List<TimeZoneDefinition>();
		private ReadOnlyCollection<TimeZoneDefinition> _timeZoneDefinitionsReadOnly;
		private TimeZoneToken _utcToken;
		private int _beginUpdateCount;
		private bool _versionDirty;
		private string _localTimeZoneIdDeduced;
		private string _utcTimeZoneId = "UTC";





		#endregion //Private Members

		#region Constructor

		/// <summary>
		/// Creates a  new instnace of a <see cref="CustomTimeZoneInfoProvider"/>
		/// </summary>
		public CustomTimeZoneInfoProvider()
		{
			this._timeZoneDefinitionsReadOnly = new ReadOnlyCollection<TimeZoneDefinition>(this._timeZoneDefinitions);
		}

		#endregion //Constructor	
	
		#region Base class overrides

		#region Properties

		#region LocalTimeZoneIdResolved

		/// <summary>
		/// Returns the id for the local time zone (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if <see cref="TimeZoneInfoProvider.LocalTimeZoneId"/> is set its value will be returned. If it is not set an attempt will be made to deduce it.</para>
		/// </remarks>
		public override string LocalTimeZoneIdResolved
		{
			get
			{
				this.VerifyAccess();

				string localId = base.LocalTimeZoneId;

				if (!string.IsNullOrWhiteSpace(localId))
					return localId;


				if (!string.IsNullOrWhiteSpace(TimeZoneInfoProvider._timeZoneIdSelected))
					return TimeZoneInfoProvider._timeZoneIdSelected;


				if (this._localTimeZoneIdDeduced == null)
					this._localTimeZoneIdDeduced = DeduceLocalTimeZoneId();

				return this._localTimeZoneIdDeduced;
			}
		}

		#endregion //LocalTimeZoneIdResolved

		#region UtcToken

		/// <summary>
		/// Returns the token that represents the UTC time zone
		/// </summary>
		public override TimeZoneToken UtcToken
		{
			get
			{
				this.VerifyAccess();

				if (this._utcToken == null)
				{
					if (!CoreUtilities.IsValueEmpty(this._utcTimeZoneId))
						this._utcToken = this.GetTimeZoneToken(this._utcTimeZoneId, false);
				}

				return this._utcToken;
			}
		}

		#endregion //UtcToken

		#endregion //Properties	

		#region Methods
		
		#region BumpVersion

		/// <summary>
		/// Should be called whenever any time zone information has changed
		/// </summary>
		protected override void BumpVersion()
		{
			this.VerifyAccess();

			if (this._beginUpdateCount > 0)
			{
				this._versionDirty = true;
				return;
			}

			base.BumpVersion();
		}

		#endregion //BumpVersion	

		#region ClearCachedValues

		/// <summary>
		/// Clears any values that have been cached so they can be re-created when needed
		/// </summary>
		public override void ClearCachedValues()
		{
			this.VerifyAccess();

			this._timeZoneDefinitionMap.Clear();
			this._timeZoneDefinitions.Clear();
			this.Tokens.Clear();

			this._utcToken = null;
			this._localTimeZoneIdDeduced = null;
			
			base.ClearCachedValues();
		}

		#endregion //ClearCachedValues

		#region ConvertTime

		/// <summary>
		/// Converts a DateTime from one time zone to another
		/// </summary>
		/// <param name="source">The time zone of the passed in date time.</param>
		/// <param name="destination">The time zone to convert the date time to.</param>
		/// <param name="dateTime">The date time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override DateTime ConvertTime(TimeZoneToken source, TimeZoneToken destination, DateTime dateTime)
		{

			TimeZoneDefinition tziSource = this.VerifyTimeZoneDefinition(source);
			TimeZoneDefinition tziDest = this.VerifyTimeZoneDefinition(destination);

			if (tziSource != null && tziDest != null)
			{
				if (tziSource == tziDest)
					return dateTime;
				
				return ConvertTime(dateTime, tziSource, tziDest);
			}

			return dateTime;
		}


		/// <summary>
		/// Converts a DateTime from one time zone to another
		/// </summary>
		/// <param name="source">The time zone of the passed in date time.</param>
		/// <param name="destination">The time zone to convert the date time to.</param>
		/// <param name="dateTimeOffset">The date time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override DateTimeOffset ConvertTime(TimeZoneToken source, TimeZoneToken destination, DateTimeOffset dateTimeOffset)
		{
			TimeZoneDefinition tziSource = this.VerifyTimeZoneDefinition(source);
			TimeZoneDefinition tziDest = this.VerifyTimeZoneDefinition(destination);

			if (tziSource != null && tziDest != null)
			{
				if (tziSource == tziDest)
					return dateTimeOffset;

				// first adjust the source time to utc
				TimeSpan utcOffset = GetUtcOffset( dateTimeOffset, tziSource);

				dateTimeOffset = dateTimeOffset.Add(utcOffset);

				// adjust the destination from utc
				utcOffset = GetUtcOffset( dateTimeOffset, tziDest);

				dateTimeOffset = dateTimeOffset.Subtract(utcOffset);
			}

			return dateTimeOffset;
		}

		#endregion //ConvertTime

		#region GetAdjustmentRules

		/// <summary>
		/// Returns an array of adjustment rules for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override TimeAdjustmentRule[] GetAdjustmentRules(TimeZoneToken token)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			if (tzDef != null)
				return tzDef.AdjustmentRules.ToArray<TimeAdjustmentRule>();

			return new TimeAdjustmentRule[0];
		}

		#endregion //GetAdjustmentRules

		#region GetAmbiguousTimeOffsets

		/// <summary>
		/// Gets the ambiguous time offsets for a time when a daylight time transition is occuring
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The date time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		/// <exception cref="ArgumentException">If DateTime is not ambiguous.</exception>
		/// <seealso cref="IsAmbiguousTime(TimeZoneToken,DateTime)"/>
		public override TimeSpan[] GetAmbiguousTimeOffsets(TimeZoneToken token, DateTime dateTime)
		{
			TimeZoneDefinition tzi = this.VerifyTimeZoneDefinition(token);

			if (tzi != null)
				return GetAmbiguousTimeOffsets(dateTime, tzi);

			return new TimeSpan[0];
		}

		/// <summary>
		/// Gets the ambiguous time offsets for a time when a daylight time transition is occuring
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTimeOffset">The date time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		/// <exception cref="ArgumentException">If DateTime is not ambiguous.</exception>
		/// <seealso cref="IsAmbiguousTime(TimeZoneToken,DateTimeOffset)"/>
		public override TimeSpan[] GetAmbiguousTimeOffsets(TimeZoneToken token, DateTimeOffset dateTimeOffset)
		{
			TimeZoneDefinition tzi = this.VerifyTimeZoneDefinition(token);

			if (tzi != null)
				return GetAmbiguousTimeOffsets(dateTimeOffset, tzi);

			return new TimeSpan[0];
		}

		#endregion //GetAmbiguousTimeOffsets

		#region GetBaseUtcOffset

		/// <summary>
		/// Returns the base offset to utc time (when not in daylight saving time)
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <returns>A TimeSpan with the UTC offset.</returns>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override TimeSpan GetBaseUtcOffset(TimeZoneToken token)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			return tzDef != null ? tzDef.BaseUtcOffset : new TimeSpan();
		}

		#endregion //GetBaseUtcOffset

		#region GetDaylightName

		/// <summary>
		/// Returns the display name for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override string GetDaylightName(TimeZoneToken token)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			return tzDef != null ? tzDef.DaylightName : string.Empty;
		}

		#endregion //GetDaylightName

		#region GetDisplayName

		/// <summary>
		/// Returns the display name for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override string GetDisplayName(TimeZoneToken token)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			if (tzDef != null)
			{
				string displayName = tzDef.DisplayName;

				if (string.IsNullOrWhiteSpace(displayName))
					return tzDef.Id;

				return displayName;
			}

			return string.Empty;
		}

		#endregion //GetDisplayName

		#region GetExportId

		/// <summary>
		/// Returns the export id for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <returns>The export id or null.</returns>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override string GetExportId(TimeZoneToken token)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			return tzDef != null ? tzDef.ExportId : null;
		}

		#endregion //GetExportId

		#region GetStandardName

		/// <summary>
		/// Returns the display name for the time zone
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override string GetStandardName(TimeZoneToken token)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			return tzDef != null ? tzDef.StandardName : string.Empty;
		}

		#endregion //GetStandardName

		#region GetTimeZoneDefinitions

		/// <summary>
		/// Returns a read-only collection of <see cref="TimeZoneInfoProvider.TimeZoneDefinition"/> objects
		/// </summary>
		protected override ReadOnlyCollection<TimeZoneDefinition> GetTimeZoneDefinitions()
		{
			return this._timeZoneDefinitionsReadOnly;
		}

		#endregion //GetTimeZoneDefinitions

		#region GetUtcOffset

		/// <summary>
		/// Returns the offset to convert a date time to Utc time
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The specific date time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override TimeSpan GetUtcOffset(TimeZoneToken token, DateTime dateTime)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			if ( tzDef != null )
				return GetUtcOffsetForDateTime(dateTime, tzDef);

			return new TimeSpan();
		}

		/// <summary>
		/// Returns the offset to convert a date time offset to Utc time
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTimeOffset">The specific date time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override TimeSpan GetUtcOffset(TimeZoneToken token, DateTimeOffset dateTimeOffset)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			if (tzDef != null)
				return GetUtcOffset(dateTimeOffset, tzDef);

			return new TimeSpan();
		}

		#endregion //GetUtcOffset

		#region IsAmbiguousTime

		/// <summary>
		/// Determines if a DateTime is ambiguous, i.e. it is within a daylight time transition
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The date time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override bool IsAmbiguousTime(TimeZoneToken token, DateTime dateTime)
		{
			TimeZoneDefinition tzi = this.VerifyTimeZoneDefinition(token);

			if (tzi != null)
				return IsAmbiguous(dateTime, tzi);

			return false;
		}

		/// <summary>
		/// Determines if a DateTime is ambiguous, i.e. it is within a daylight time transition
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTimeOffset">The date time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override bool IsAmbiguousTime(TimeZoneToken token, DateTimeOffset dateTimeOffset)
		{
			TimeZoneDefinition tzi = this.VerifyTimeZoneDefinition(token);

			if (tzi != null)
				return IsAmbiguous(dateTimeOffset, tzi);

			return false;
		}

		#endregion //IsAmbiguousTime

		#region IsDaylightSavingsTime

		/// <summary>
		/// Determines if a DateTime is within a daylight savings adjustment period
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The date time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override bool IsDaylightSavingsTime(TimeZoneToken token, DateTime dateTime)
		{
			TimeZoneDefinition tzi = this.VerifyTimeZoneDefinition(token);

			if (tzi != null)
				return this.IsDaylightSavingsTime(dateTime, tzi);

			return false;
		}

		/// <summary>
		/// Determines if a DateTime is within a daylight savings adjustment period
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTimeOffset">The date time and offset</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override bool IsDaylightSavingsTime(TimeZoneToken token, DateTimeOffset dateTimeOffset)
		{
			TimeZoneDefinition tzi = this.VerifyTimeZoneDefinition(token);

			if (tzi != null)
				return this.IsDaylightSavingsTime(dateTimeOffset, tzi);

			return false;
		}

		#endregion //IsDaylightSavingsTime

		#region IsInvalidTime

		/// <summary>
		/// Determines if a DateTime is invalid
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The date time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override bool IsInvalidTime(TimeZoneToken token, DateTime dateTime)
		{
			TimeZoneDefinition tzi = this.VerifyTimeZoneDefinition(token);

			if (tzi != null)
				return this.IsInvalidTime(dateTime, tzi);

			return false;
		}

		#endregion //IsInvalidTime

		#region IsValidTimeZoneId

		/// <summary>
		/// Determines if a string is a valid time zone id
		/// </summary>
		/// <param name="id">The id to verify.</param>
		/// <returns>True if the id is valid, otherwise false</returns>
		public override bool IsValidTimeZoneId(string id)
		{
			this.VerifyAccess();

			if (id == null)
				return false;

			return this._timeZoneDefinitionMap.ContainsKey(id);
		}

		#endregion //IsValidTimeZoneId

		#region SupportsDayLightSavingsTime

		/// <summary>
		/// Returns true if the time zone supports daylight savings time
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <returns>True if the time zone supports daylight savings time, otherwise false.</returns>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override bool SupportsDayLightSavingsTime(TimeZoneToken token)
		{
			TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

			return tzDef != null ? tzDef.SupportsDaylightSavingsTime : false;
		}

		#endregion //SupportsDayLightSavingsTime

		#endregion //Methods	
	
		#endregion //Base class overrides	

		#region Properties

		#region Public Properties

		#region UtcTimeZoneId

		/// <summary>
		/// Returns or sets the id for the Utc time zone
		/// </summary>
		public string UtcTimeZoneId
		{
			get
			{
				return this._utcTimeZoneId;
			}
			set
			{
				if (this._utcTimeZoneId != value)
				{
					//// call GetTimeZoneToken which will throw an exception if the Id is not valid
					//if (value != null && value.Length > 0 &&
					//    this.GetTimeZoneToken(value) != null)
					this._utcTimeZoneId = value;
				}
			}
		}

		#endregion //UtcTimeZoneId	

		#endregion //Public Properties	

		#region Internal Properties

		#region DefaultProviderInternal



#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


		#endregion //DefaultProviderInternal

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region BeginUpdate

		/// <summary>
		/// Suspends bumping the <see cref="TimeZoneInfoProvider.Version"/> property until <see cref="EndUpdate"/> is called.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: every call to <b>BeginUpdate</b> must be paired with a subsequewnt call to <see cref="EndUpdate"/></para>
		/// </remarks>
		/// <seealso cref="TimeZoneInfoProvider.Version"/>
		/// <seealso cref="EndUpdate"/>
		public void BeginUpdate()
		{
			this.VerifyAccess();

			this._beginUpdateCount++;
			this.Tokens.BeginUpdate();
		}

		#endregion //BeginUpdate	
	
		#region EndUpdate

		/// <summary>
		/// Restores the bumping of the <see cref="TimeZoneInfoProvider.Version"/> property.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: every call to <see cref="BeginUpdate"/> must be paired with a subsequewnt call to <b>EndUpdate</b></para>
		/// </remarks>
		/// <seealso cref="TimeZoneInfoProvider.Version"/>
		/// <seealso cref="BeginUpdate"/>
		public void EndUpdate()
		{
			this.VerifyAccess();

			this.Tokens.EndUpdate();

			if (this._beginUpdateCount == 0)
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_BeginUpdateBeforeEnd"));// "Must call BeginUpdate before calling EndUpdate."

			this._beginUpdateCount--;

			if (this._beginUpdateCount == 0)
			{
				if (this._versionDirty)
				{
					this._versionDirty = false;
					this.BumpVersion();
				}
			}
		}

		#endregion //EndUpdate	
    
		#region LoadFromXml

		/// <summary>
		/// Loads all the time zone definition information from an xml string
		/// </summary>
		/// <param name="xml">A string containing the xml</param>
		/// <returns>True is succesful, otherwise false.</returns>
		public bool LoadFromXml(string xml)
		{
			return this.LoadFromXml(xml, true);
		}

		/// <summary>
		/// Loads all the time zone definition information from an xml string
		/// </summary>
		/// <param name="xml">A string containing the xml</param>
		/// <param name="includeDisplayNames">Determines if time zone display names will be de-serialized from the xml.</param>
		/// <returns>True is succesful, otherwise false.</returns>
		public bool LoadFromXml(string xml, bool includeDisplayNames)
		{
			this.VerifyAccess();

			AttributeValueParser avp = new AttributeValueParser();
			ObjectSerializer serializer = new ObjectSerializer(avp);

			RegisterSerializationInfo(serializer, includeDisplayNames);

			object value;

			bool succeeded = serializer.ParseXmlFragment(xml, typeof(TimeZoneInformation), out value);

			TimeZoneInformation tzInfo = value as TimeZoneInformation;

			if (tzInfo != null)
			{
				this.BeginUpdate();

				this.ClearCachedValues();

				ObservableCollectionExtended<TimeZoneToken> tokens = this.Tokens;

				foreach (TimeZoneDefinition tzDef in tzInfo.TimeZoneDefinitions)
				{
					this._timeZoneDefinitions.Add(tzDef);
					this._timeZoneDefinitionMap[tzDef.Id] = tzDef;

					TimeZoneToken token = this.CreateToken(tzDef.Id);

					token.TimeZoneDefinition = tzDef;

					tokens.Add(token);
				}

				this.EndUpdate();

				return succeeded;
			}

			return false;
		}

		#endregion //LoadFromXml

		#region RegisterCustomTimeZone

		/// <summary>
		/// Registers a custom time zone
		/// </summary>
		/// <param name="id">The id of the time zone.</param>
		/// <param name="exportId">The export id</param>
		/// <param name="baseUtcOffset">The offset from UTC to use when not in daylight savings time.</param>
		/// <param name="dayLightName">The name of the time zone in daylight savings time</param>
		/// <param name="displayName">The display name of the time zone</param>
		/// <param name="standardName">The name of the time zone in standard time</param>
		/// <param name="supportsDaylightSavingsTime">Whether this time zone supports daylight savings.</param>
		/// <param name="adjustmentRules">The adjustment rules for daylight savings</param>
		public void RegisterCustomTimeZone(string id,
											string exportId,
											TimeSpan baseUtcOffset,
											string dayLightName,
											string displayName,
											string standardName,
											bool supportsDaylightSavingsTime,
											TimeAdjustmentRule[] adjustmentRules)
		{
			this.VerifyAccess();

			CoreUtilities.ValidateNotEmpty(id, "id");

			ObservableCollectionExtended<TimeZoneToken> tokens = this.Tokens;

			if (this._timeZoneDefinitionMap.ContainsKey(id))
			{
				this._timeZoneDefinitions.Remove(this._timeZoneDefinitionMap[id]);
				
				int count = tokens.Count;
				for (int i = 0; i < count; i++)
				{
					if (tokens[i].Id == id)
					{
						tokens.RemoveAt(i);
						break;
					}
				}
			}

			TimeZoneToken token = this.CreateToken(id);

			TimeZoneDefinition tzDef = this.CreateTimeZoneDefinition(token, exportId, baseUtcOffset, dayLightName, displayName, standardName, supportsDaylightSavingsTime, adjustmentRules);
			this._timeZoneDefinitionMap[id] = tzDef;
			this._timeZoneDefinitions.Add(tzDef);

			token.TimeZoneDefinition = tzDef;

			tokens.Add(token);

			this._localTimeZoneIdDeduced = null;

			this.BumpVersion();
		}

		#endregion //RegisterCustomTimeZone

		#endregion //Public Methods	
	
		#region Internal Methods

		#region HasSameUtcOffset

		internal bool HasSameUtcOffset(TimeZoneInfo tzOS, TimeZoneToken tzTokenToTest, DateTime dateTime, TimeSpan before, TimeSpan after, TimeSpan step)
		{
			DateTime from = dateTime - before;
			DateTime thru = dateTime + after;

			for (DateTime dt = from; dt < thru; dt += step)
			{
				if (!HasSameUtcOffset(tzOS, tzTokenToTest, dt))
					return false;
			}

			return true;
		}

		internal bool HasSameUtcOffset(TimeZoneInfo tzOS, TimeZoneToken tzTokenToTest, DateTime dateTime)
		{
			return tzOS.GetUtcOffset(dateTime) == this.GetUtcOffset(tzTokenToTest, dateTime);
		}

		#endregion //HasSameUtcOffset

		#region TreatsAllDatesTheSame

		internal bool TreatsAllDatesTheSame(TimeZoneInfo tzOS, TimeZoneToken tzTokenToTest, IEnumerable<TimeAdjustmentRule> rulesToTest)
		{

			foreach (TimeAdjustmentRule rule in rulesToTest)
			{
				if (!TreatsRuleBoundaryDatesTheSame(tzOS, tzTokenToTest, rule))
					return false;
			}

			return true;
		}

		#endregion //TreatsAllDatesTheSame

		#region TreatsRuleBoundaryDatesTheSame

		internal bool TreatsRuleBoundaryDatesTheSame(TimeZoneInfo tzOS, TimeZoneToken tzTokenToTest, TimeAdjustmentRule ruleToTest)
		{
			int fromYear = Math.Max(ruleToTest.Start.Year, DateTime.Now.Year - 10);
			int thruYear = Math.Min(ruleToTest.End.Year, DateTime.Now.Year + 10);

			TimeSpan before = TimeSpan.FromHours(Math.Abs(ruleToTest.Delta.Hours) + 1);
			TimeSpan after = TimeSpan.FromHours(Math.Abs(ruleToTest.Delta.Hours) + 1);
			TimeSpan step = TimeSpan.FromMinutes(15);

			for (int year = fromYear; year <= thruYear; year++)
			{
				DaylightTimeForYear dlt = ruleToTest.GetDaylightTimeForYear(year);

				// check time around transition start
				if (!HasSameUtcOffset(tzOS, tzTokenToTest, dlt.Start, before, after, step))
					return false;

				// check time around transition end
				if (!HasSameUtcOffset(tzOS, tzTokenToTest, dlt.End, before, after, step))
					return false;
			}

			return true;
		}

		#endregion //TreatsRuleBoundaryDatesTheSame

		#endregion //Internal Methods	
    	
		#region Private Methods

		#region DeduceLocalTimeZoneId

		private string DeduceLocalTimeZoneId()
		{


			if (this._timeZoneDefinitionMap.ContainsKey(TimeZoneInfo.Local.Id))
			{
				this._localTimeZoneIdDeduced = TimeZoneInfo.Local.Id;
				return this._localTimeZoneIdDeduced;
			}


			if (this._timeZoneDefinitions.Count > 0)
			{
				List<TimeZoneDefinition> candidates = new List<TimeZoneDefinition>();
				Dictionary<TimeAdjustmentRule, TimeZoneDefinition> aggregatedRules = new Dictionary<TimeAdjustmentRule, TimeZoneDefinition>();

				TimeSpan localOffset = TimeZoneInfo.Local.BaseUtcOffset;

				// look for a timezones with the same offset and aggregate all of their adjustment rules
				foreach (TimeZoneDefinition tzDef in this._timeZoneDefinitions)
				{
					TimeSpan offset = tzDef.BaseUtcOffset;
					if (offset == localOffset)
					{
						candidates.Add(tzDef);

						// update the dictionary with all of the rules
						foreach (TimeAdjustmentRule rule in tzDef.AdjustmentRules)
							aggregatedRules[rule] = tzDef;

					}
				}

				if (candidates.Count > 0)
				{
					// special case a local offset of zero. 
					// In this case if there are more than 1 candidate we want to remove
					// the UTC def from the running so we need to remove it from the candidates.
					// 
					if (localOffset == TimeSpan.Zero &&
						candidates.Count > 1 &&
						this.UtcToken != null)
					{
						TimeZoneDefinition utcDef = this.UtcToken.TimeZoneDefinition;
						for (int i = 0; i < candidates.Count; i++)
						{
							if (candidates[i] == utcDef)
							{
								candidates.RemoveAt(i);
								break;
							}
						}
					}

					if (aggregatedRules.Count == 0)
					{
						// since there were no rules aggregated above and there are multiple candidates then
						// we can't deduce the correct one.
						// However, if there is only one candidate then we can test it with various dates
						if (candidates.Count == 1)
						{
							TimeZoneDefinition tzDef = candidates[0];

							TimeZoneToken token = this.GetTimeZoneToken(tzDef.Id);

							if (token != null)
							{
								// since we have a single candidate with no rules then check dates for the last
								// 20 years thru the next 20 years if they all has the same utc offset then
								// accept the timezone as local
								DateTime from = new DateTime(DateTime.Now.Year - 20, 1, 15);
								DateTime thru = new DateTime(DateTime.Now.Year + 20, 12, 15);
								for (DateTime dt = from; dt < thru; dt = dt.AddDays(17))
								{
									if (!HasSameUtcOffset(TimeZoneInfo.Local, token, dt))
										return string.Empty;
								}

								return tzDef.Id;
							}
						}
					}
					else
					{
						TimeZoneDefinition exactMatch = null;

						foreach (TimeZoneDefinition tzDef in candidates)
						{
							TimeZoneToken token = this.GetTimeZoneToken(tzDef.Id);

							if (token != null)
							{
								// otherwise make sure the utc offsets are the same using the rule
								// transition dates to check
								if (TreatsAllDatesTheSame(TimeZoneInfo.Local, token, aggregatedRules.Keys))
								{
									// if there was a prior exact match then return an empty string
									// since we can't assume which one is correct
									if (exactMatch != null)
										return string.Empty;

									exactMatch = tzDef;
								}
							}
						}

						if (exactMatch != null)
							return exactMatch.Id;
					}
				}
			}

			return string.Empty;
		}

		#endregion //DeduceLocalTimeZoneId	
	
		#region VerifyTimeZoneDefinition

		private TimeZoneDefinition VerifyTimeZoneDefinition(TimeZoneToken token)
		{
			this.VerifyToken(token);

			TimeZoneDefinition tzDef = token.TimeZoneDefinition;

			if (tzDef != null)
				return tzDef;

			this._timeZoneDefinitionMap.TryGetValue(token.Id, out tzDef);

			return tzDef;
		}

		#endregion //VerifyTimeZoneDefinition

		#endregion //Private Methods	
	
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