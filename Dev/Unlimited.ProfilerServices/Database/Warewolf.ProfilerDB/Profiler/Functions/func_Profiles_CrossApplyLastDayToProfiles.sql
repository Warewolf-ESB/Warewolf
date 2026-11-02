CREATE FUNCTION [Profiler].[func_Profiles_CrossApplyLastDayToProfiles] (
	@FromDate DATE
	,@Todate DATE
	)
RETURNS @DATES TABLE (
	[DateName] DATETIME
	,[DayName] VARCHAR(50)
	,[MonthName] VARCHAR(50)
	,[DayOfWeek] SMALLINT
	,[DayOfMonth] SMALLINT
	,[MonthOfYear] SMALLINT
	,[YearNo] SMALLINT
	,[MonthStartDate] DATE
	,[MonthEndDate] DATE
	,[PubHolFlag] BIT
	,[WeekDayFlag] BIT
	,[WeekStartDate] DATE
	,[WeekEndDate] DATE
	,DebitRunID SMALLINT
	,DebitRunIDDayName VARCHAR(50)
	,DebitRunIDDate DATE
	,DebitRunIDMonthName VARCHAR(50)
	,LUCCollectionProfile VARCHAR(50)
	)
AS
BEGIN
	INSERT INTO @DATES
	SELECT [DateName]
		,[DayName]
		,[MonthName]
		,[DayOfWeek]
		,[DayOfMonth]
		,[MonthOfYear]
		,[YearNo]
		,[MonthStartDate]
		,[MonthEndDate]
		,[PubHolFlag]
		,[WeekDayFlag]
		,[WeekStartDate]
		,[WeekEndDate]
		,DebitRun.n AS [DebitRunID]
		,[DebitRunIDDayName]
		,[DebitRunIDDate]
		,[DebitRunIDMonthName]
		,[LUCCollectionProfile]
	FROM (
		SELECT *
		FROM [Profiler].func_Profiles_FriPubHolFriPayMon(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_FriPubHolFriPayThu(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_MonPubHolMonPayFri(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_MonPubHolMonPaySat(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_MonPubHolMonPayTue(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPayFri(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPayMon(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPaySat(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPubHolFriPayMon(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPubHolFriPayThu(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPubHolMonPayFri(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPubHolMonPayTue(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPubHolSatPayFri(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPubHolSatPayMon(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPayFri(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPayMon(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPaySat(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPubHolFriPayMon(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPubHolFriPaySat(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPubHolFriPayThu(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPubHolMonPayFri(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPubHolMonPaySat(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPubHolMonPayTue(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_WeekDay(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_Weekday_FirstFriday(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_Weekday_FourthFriday(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_Weekday_LastFriday(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_Weekday_SecondFriday(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_Weekday_ThirdFriday(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_WeekdayPublicHolidayPayDayAfter(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_WeekdayPublicHolidayPayDayBefore(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_FriPubHolFriPaySat(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPubHolMonFriPaySat(@FromDate, @ToDate)
		) AS ProfileDates
	CROSS JOIN [Profiler].func_Nums(31) DebitRun
	WHERE MonthEndDate = DebitRunIDDate
		AND day(MonthEndDate) = DebitRunID
		AND DebitRun.n > DebitRunID

	RETURN
END
