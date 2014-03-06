using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Resources;
using System.IO.Compression;
using System.Collections.ObjectModel;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// A class used to provide time zone information for date time conversions to and from utc date times from the information provided by the Windows OS.
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note</b>: this class is only available in WPF. In Silverlight a custom class should be derived from <see cref="TimeZoneInfoProvider"/> or an instance of the <see cref="CustomTimeZoneInfoProvider"/> should be used.</para>
	/// </remarks>
	public class OSTimeZoneInfoProvider : TimeZoneInfoProvider
	{
		#region Private Members

		private Dictionary<string, string> _exportIdMap;
		private List<TimeZoneDefinition> _timeZoneDefinitions;
		private ReadOnlyCollection<TimeZoneDefinition> _timeZoneDefinitionsReadOnly;
		private Dictionary<string, ScheduleResourceString> _displayNameMap = new Dictionary<string, ScheduleResourceString>();
		private TimeZoneToken _utcToken;
		private string _utcDisplayName;
		private bool _useOsDisplayNames;
		
		[ThreadStatic()]
		private static OSTimeZoneInfoProvider s_defaultProvider;

		#endregion //Private Members	

		#region Constructor

		/// <summary>
		/// Creates a  new instnace of a <see cref="OSTimeZoneInfoProvider"/>
		/// </summary>
		public OSTimeZoneInfoProvider()
		{

			this.InitializeTokens();

		}

		#endregion //Constructor

		#region Base class overrides

		#region Properties

		#region LocalTimeZoneIdResolved

		/// <summary>
		/// Returns the id for the local time zone (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if <see cref="TimeZoneInfoProvider.LocalTimeZoneId"/> is set its value will be returned. If it is not then the sytem local time zone id will be returned.</para>
		/// </remarks>
		public override string LocalTimeZoneIdResolved
		{
			get
			{
				this.VerifyAccess();
				
				string localId = this.LocalTimeZoneId;

				if (!string.IsNullOrWhiteSpace(localId))
					return localId;


				if (!string.IsNullOrWhiteSpace(TimeZoneInfoProvider._timeZoneIdSelected))
					return TimeZoneInfoProvider._timeZoneIdSelected;

				return TimeZoneInfo.Local.Id;
			}
		}

		#endregion //LocalTimeZoneIdResolved

		#region UseOsDisplayNames

		/// <summary>
		/// Identifies the <see cref="UseOsDisplayNames"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseOsDisplayNamesProperty = DependencyPropertyUtilities.Register("UseOsDisplayNames",
			typeof(bool), typeof(OSTimeZoneInfoProvider),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnUseOsDisplayNamesChanged))
			);

		private static void OnUseOsDisplayNamesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			OSTimeZoneInfoProvider instance = (OSTimeZoneInfoProvider)d;
			
			instance._useOsDisplayNames = (bool)e.NewValue;
		}

		/// <summary>
		/// Returns or sets whether to use the system display names for time zones
		/// </summary>
		/// <seealso cref="UseOsDisplayNamesProperty"/>
		public bool UseOsDisplayNames
		{
			get
			{
				return this._useOsDisplayNames;
			}
			set
			{
				this.SetValue(OSTimeZoneInfoProvider.UseOsDisplayNamesProperty, value);
			}
		}

		#endregion //UseOsDisplayNames

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
					// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
					// Call TryGetTimeZoneToken with "UTC" as the id. Calling GetTimeZoneToken
					// threw an exception on certain versions of the XP OS, e.g. 
					// some versions of Japanese localized systems.
					//this._utcToken = this.GetTimeZoneToken(TimeZoneInfo.Utc.Id);
					this.TryGetTimeZoneToken("UTC", out this._utcToken);


					Debug.Assert(this._utcToken != null, "UTC token not found");


				}

				return this._utcToken;
			}
		}

		#endregion //UtcToken

		#endregion //Properties	
	
		#region Methods

		#region ClearCachedValues

		/// <summary>
		/// Clears any values that have been cached so they can be re-created when needed
		/// </summary>
		public override void ClearCachedValues()
		{
			this._timeZoneDefinitions = null;
			this._utcToken = null;

			base.ClearCachedValues();

			this.InitializeTokens();
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

			TimeZoneInfo tziSource = this.VerifyTimeZoneInfo(source);
			TimeZoneInfo tziDest = this.VerifyTimeZoneInfo(destination);

			if (tziSource != null && tziDest != null)
			{
				if (tziSource == tziDest)
					return dateTime;

				dateTime = EnsureKindNotLocal(dateTime);

				return TimeZoneInfo.ConvertTime(dateTime, tziSource, tziDest);
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
			TimeZoneInfo tziSource = this.VerifyTimeZoneInfo(source);
			TimeZoneInfo tziDest = this.VerifyTimeZoneInfo(destination);

			if (tziSource != null && tziDest != null)
			{
				if (tziSource == tziDest)
					return dateTimeOffset;

				// first adjust the source time to utc
				TimeSpan utcOffset = tziSource.GetUtcOffset(dateTimeOffset);

				dateTimeOffset = dateTimeOffset.Add(utcOffset);

				// adjust the destination from utc
				utcOffset = tziDest.GetUtcOffset(dateTimeOffset);

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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			if (tzi != null)
				return TimeAdjustmentRule.FromOSRuleArray(tzi.GetAdjustmentRules());

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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			dateTime = EnsureKindNotLocal(dateTime);

			return tzi != null ? tzi.GetAmbiguousTimeOffsets(dateTime) : new TimeSpan[0];
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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			return tzi != null ? tzi.GetAmbiguousTimeOffsets(dateTimeOffset) : new TimeSpan[0];
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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			return tzi != null ? tzi.BaseUtcOffset : new TimeSpan();
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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			return tzi != null ? tzi.DaylightName : string.Empty;
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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			if (tzi != null)
			{
				string id = token.Id;

				if (this._useOsDisplayNames)
				{
					if (tzi == TimeZoneInfo.Utc)
					{
						if (this._utcDisplayName == null)
						{
							this._utcDisplayName = "UTC";

							foreach (TimeZoneInfo sysTzi in TimeZoneInfo.GetSystemTimeZones())
							{
								if (string.Compare(sysTzi.Id, "UTC", StringComparison.OrdinalIgnoreCase) == 0)
								{
									this._utcDisplayName = sysTzi.DisplayName;
									break;
								}
							}
						}

						return this._utcDisplayName;
					}

					return tzi.DisplayName;
				}

				ScheduleResourceString resString;

				if (!this._displayNameMap.TryGetValue(id, out resString))
				{
					resString = new ScheduleResourceString();
					resString.ResourceName = id;

					this._displayNameMap[id] = resString;
				}

				return resString.Value;
			}

			return tzi != null ? tzi.DisplayName : string.Empty;
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
			CoreUtilities.ValidateNotNull(token, "token");

			string exportId;

			if (this.ExportIdMap.TryGetValue(token.Id, out exportId))
				return exportId;

			return null;
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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			return tzi != null ? tzi.StandardName : string.Empty;
		}

		#endregion //GetStandardName

		#region GetTimeZoneDefinitions

		/// <summary>
		/// Returns a read-only collection of <see cref="TimeZoneInfoProvider.TimeZoneDefinition"/> objects
		/// </summary>
		protected override ReadOnlyCollection<TimeZoneDefinition> GetTimeZoneDefinitions()
		{
			this.VerifyTimeZoneDefinitionCache();
			return this._timeZoneDefinitionsReadOnly;
		}

		#endregion //GetTimeZoneDefinitions

		#region GetUtcOffsetFromUtcTime

		/// <summary>
		/// Returns the offset to convert a date time to Utc time
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The specific date time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override TimeSpan GetUtcOffset(TimeZoneToken token, DateTime dateTime)
		{
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			dateTime = EnsureKindNotLocal(dateTime);

			if (tzi != null)
				return tzi.GetUtcOffset(dateTime);

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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			if (tzi != null)
				return tzi.GetUtcOffset(dateTimeOffset);

			return new TimeSpan();
		}

		#endregion //GetUtcOffsetFromUtcTime

		#region IsAmbiguousTime

		/// <summary>
		/// Determines if a DateTime is ambiguous, i.e. it is within a daylight time transition
		/// </summary>
		/// <param name="token">An object that represents a specific time zone.</param>
		/// <param name="dateTime">The date time</param>
		/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
		public override bool IsAmbiguousTime(TimeZoneToken token, DateTime dateTime)
		{
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			dateTime = EnsureKindNotLocal(dateTime);

			if (tzi != null)
				return tzi.IsAmbiguousTime(dateTime);

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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			if (tzi != null)
				return tzi.IsAmbiguousTime(dateTimeOffset);

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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			dateTime = EnsureKindNotLocal(dateTime);

			if (tzi != null)
				return tzi.IsDaylightSavingTime(dateTime);

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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			if (tzi != null)
				return tzi.IsDaylightSavingTime(dateTimeOffset);

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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			dateTime = EnsureKindNotLocal(dateTime);

			if (tzi != null)
				return tzi.IsInvalidTime(dateTime);

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

			// JJD 10/27/10 - TFS57632, TFS57534 and TFS57635
			// Always allow the utc id
			if (id == TimeZoneInfo.Utc.Id)
				return true;

			foreach (System.TimeZoneInfo tzi in System.TimeZoneInfo.GetSystemTimeZones())
			{
				if (tzi.Id == id)
					return true;
			}

			return false;
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
			TimeZoneInfo tzi = this.VerifyTimeZoneInfo(token);

			return tzi != null ? tzi.SupportsDaylightSavingTime : false;
		}

		#endregion //SupportsDayLightSavingsTime

		#endregion //Methods	
	
		#endregion //Base class overrides	

		#region Properties

		#region Public Properties

		#region ExportIdMap

		/// <summary>
		/// Returns a dictionary that contains windows time zone ids as keys and the associated export ids as values.
		/// </summary>
		public Dictionary<string, string> ExportIdMap
		{
			get
			{
				this.VerifyAccess();

				if (this._exportIdMap == null)
					this._exportIdMap = LoadDefaultExportIdMap();

				return this._exportIdMap;
			}
		}

		#endregion //ExportIdMap

		#endregion //Public Properties	

		#region Internal Properties

		#region DefaultProviderInternal

		internal static OSTimeZoneInfoProvider DefaultProviderInternal
		{
			get
			{
				if (s_defaultProvider == null)
				{
					s_defaultProvider = new OSTimeZoneInfoProvider();
				}

				return s_defaultProvider;
			}
		}

		#endregion //DefaultProviderInternal

		#endregion //Internal Properties	
    
		#endregion //Properties

		#region Methods

		#region Private Methods

		#region InitializeTokens

		private void InitializeTokens()
		{
			ObservableCollectionExtended<TimeZoneToken> tokens = this.Tokens;

			tokens.BeginUpdate();

			tokens.Clear();

			// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
			// Get the utc id
			string utcId = TimeZoneInfo.Utc.Id;
			int firstZeroOffsetEntry = -1;
			bool utcFound = false;

			foreach (TimeZoneInfo tzi in TimeZoneInfo.GetSystemTimeZones())
			{
				string id = tzi.Id;

				if (!string.IsNullOrWhiteSpace(id))
				{
					// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
					// Keep track of the index of the first time zone with a base offset of 0
					if (tzi.BaseUtcOffset == TimeSpan.Zero)
					{
						if (firstZeroOffsetEntry < 0)
							firstZeroOffsetEntry = tokens.Count;

						// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
						// set a flag that we found a time zone with the utc id
						if ( id == utcId )
							utcFound = true;
					}

					tokens.Add(this.CreateToken(id));
				}
			}

			// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
			// If we didn't find a time zone with the utc id insert it now so that
			// it falls with the other 0 offset time zones
			if (utcFound == false)
				tokens.Insert(Math.Max(firstZeroOffsetEntry, 0), this.CreateToken(utcId));

			tokens.EndUpdate();
		}

		#endregion //InitializeTokens	
	
		#region LoadDefaultExportIdMap static method

		private static Dictionary<string, string> LoadDefaultExportIdMap()
		{
			Dictionary<string, string> map = new Dictionary<string, string>();

			Uri uri = CoreUtilities.BuildEmbeddedResourceUri(typeof(OSTimeZoneInfoProvider).Assembly, "datastructures/timezoneinfoproviders/tzmaps.gzip");

			StreamResourceInfo info = Application.GetResourceStream(uri);

			if (info != null)
			{
				Stream stream = info.Stream;

				GZipStream zip = new GZipStream(stream, CompressionMode.Decompress);

				StreamReader reader = new StreamReader(zip);

				String str = reader.ReadToEnd();

				string[] sections = str.Split(new char[] { ',', (char)10, (char)13, (char)0 }, StringSplitOptions.RemoveEmptyEntries);

				int index = 0;

				while (index < sections.Length - 1)
				{
					map.Add(sections[index], sections[index + 1]);

					index += 2;
				}

				reader.Close();
				zip.Close();
				stream.Close();
			}

			return map;
		}

		#endregion //LoadDefaultExportIdMap static method

		#region VerifyTimeZoneDefinitionCache

		private void VerifyTimeZoneDefinitionCache()
		{
			VerifyAccess();

			if (this._timeZoneDefinitions != null &&
				 this._timeZoneDefinitions.Count > 0)
				return;

			ReadOnlyCollection<TimeZoneInfo> systemTimeZones = TimeZoneInfo.GetSystemTimeZones();

			this._timeZoneDefinitions = new List<TimeZoneDefinition>(systemTimeZones.Count);

			// JJD 10/27/10 - TFS57632, TFS57534 and TFS57635
			// Get the utc id
			string utcId = TimeZoneInfo.Utc.Id;
			int firstZeroOffsetEntry = -1;
			bool utcFound = false;

			foreach (TimeZoneInfo tzi in systemTimeZones)
			{
				TimeZoneToken token = this.GetTimeZoneToken(tzi.Id);

				// JJD 10/27/10 - TFS57632, TFS57534 and TFS57635
				// Keep track of the index of the first time zone with a base offset of 0
				if (tzi.BaseUtcOffset == TimeSpan.Zero)
				{
					if (firstZeroOffsetEntry < 0)
						firstZeroOffsetEntry = this._timeZoneDefinitions.Count;

					// JJD 10/27/10 - TFS57632, TFS57534 and TFS57635
					// set a flag that we found a time zone with the utc id
					if (tzi.Id == utcId)
						utcFound = true;
				}
				
				token.TimeZoneInfo = tzi;

				TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

				this._timeZoneDefinitions.Add(tzDef);
			}

			// JJD 10/27/10 - TFS57632, TFS57534 and TFS57635
			// If we didn't find a time zone with the utc id insert it now so that
			// it falls with the other 0 offset time zones
			if (utcFound == false)
			{
				TimeZoneToken token;
					
				this.TryGetTimeZoneToken(utcId, out token);

				if (token != null)
				{
					token.TimeZoneInfo = TimeZoneInfo.Utc;

					TimeZoneDefinition tzDef = this.VerifyTimeZoneDefinition(token);

					this._timeZoneDefinitions.Insert(Math.Max(firstZeroOffsetEntry, 0), tzDef);
				}
			}

			this._timeZoneDefinitionsReadOnly = new ReadOnlyCollection<TimeZoneDefinition>(this._timeZoneDefinitions);
		}

		#endregion //VerifyTimeZoneDefinitionCache

		#region VerifyTimeZoneDefinition

		private TimeZoneDefinition VerifyTimeZoneDefinition(TimeZoneToken token)
		{
			this.VerifyToken(token);

			TimeZoneDefinition tzDef = token.TimeZoneDefinition;

			if (tzDef != null)
				return tzDef;

			tzDef = this.CreateTimeZoneDefinition(token,
							this.GetExportId(token),
							this.GetBaseUtcOffset(token),
							this.GetDaylightName(token),
							this.GetDisplayName(token),
							this.GetStandardName(token),
							this.SupportsDayLightSavingsTime(token),
							this.GetAdjustmentRules(token));

			token.TimeZoneDefinition = tzDef;

			return tzDef;
		}

		#endregion //VerifyTimeZoneDefinition

		#region VerifyTimeZoneInfo

		private TimeZoneInfo VerifyTimeZoneInfo(TimeZoneToken token)
		{
			this.VerifyToken(token);

			TimeZoneInfo tzi = token.TimeZoneInfo;

			if (tzi != null)
				return tzi;

			// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
			// If the id is the same as the Utc time zone then use TimeZoneInfo.Utc
			string id = token.Id;
			if (id == TimeZoneInfo.Utc.Id)
				tzi = TimeZoneInfo.Utc;
			else
				tzi = TimeZoneInfo.FindSystemTimeZoneById(token.Id);

			token.TimeZoneInfo = tzi;

			return tzi;
		}

		#endregion //VerifyTimeZoneInfo	
	
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