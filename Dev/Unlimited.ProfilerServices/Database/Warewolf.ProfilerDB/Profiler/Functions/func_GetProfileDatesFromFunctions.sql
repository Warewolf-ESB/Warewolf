CREATE FUNCTION [Profiler].[func_GetProfileDatesFromFunctions] (
	@FromDate DATE
	,@Todate DATE
	)
RETURNS @DATES TABLE (
	[DateName] DATETIME
	,[DayName] VARCHAR(50)
	,[MonthName] VARCHAR(50)
	,[DayOfMonth] SMALLINT
	,[MonthOfYear] SMALLINT
	,[YearNo] SMALLINT
	,[PubHolFlag] BIT
	,[WeekDayFlag] BIT
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
		,[DayOfMonth]
		,[MonthOfYear]
		,[YearNo]
		,[PubHolFlag]
		,[WeekDayFlag]
		,[DebitRunID]
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
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_CrossApplyLastDayToProfiles(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_CrossApplyLastWorkingDayProfiles(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_LastDayClearsInBankDays(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_OlderPersonGrant(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_DisabilityGrant(@FromDate, @ToDate)
		
		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_ChildrensGrant(@FromDate, @ToDate)

		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SatPubHolThuFriPayWed(@FromDate, @ToDate)

		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_FriPubHolThuFriPayWed(@FromDate, @ToDate)

		UNION ALL
		
		SELECT *
		FROM [Profiler].func_Profiles_SunPubHolThuFriPayWed(@FromDate, @ToDate)
		) AS ProfileDates
	--WHERE [MonthName] = [DebitRunIDMonthName] --Rule to prevent dates into other months
	--OR [LUCCollectionProfile] LIKE 'LastDay-ClearsIn%' --Rule Allow tracking into next month
	--OR (DATEPART(MONTH,DebitRunIDDate) > DATEPART(MONTH,[DateName])
 --       and DebitRunIDDate > [DateName])
	RETURN
END
