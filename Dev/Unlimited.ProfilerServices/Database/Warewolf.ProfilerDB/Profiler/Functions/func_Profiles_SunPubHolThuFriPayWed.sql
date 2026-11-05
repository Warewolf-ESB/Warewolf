CREATE FUNCTION [Profiler].[func_Profiles_SunPubHolThuFriPayWed] (
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
	DECLARE @MasterDates TABLE (
		[DateName] DATE
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
		)

	INSERT INTO @MasterDates
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
	FROM [Profiler].[func_MasterDates](@FromDate, @ToDate)


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
		,day(SundayDebitRunDate) AS DebitRunID
		,datename(weekday, SundayDebitRunDate) AS DebitRunIDDayName
		,SundayDebitRunDate AS DebitRunIDDate
		,datename(month, SundayDebitRunDate) AS DebitRunIDMonthName
		,'Sun-PubHolThuFri-PayWed' AS LUCCollectionProfile
	FROM (
		SELECT (SELECT [DateName] FROM @MasterDates Where DateName = DATEADD(DAY,1,d.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,(SELECT [DateName] FROM @MasterDates WHERE DateName = DATEADD(DAY,2,d.DateName) AND PubHolFlag = 1) FridayDebitRunDate
			,(SELECT [DateName] FROM @MasterDates WHERE DateName = DATEADD(DAY,3,d.DateName) AND PubHolFlag = 0 ) SaturdayDebitRunDate
			,(SELECT [DateName] FROM @MasterDates WHERE DateName = DATEADD(DAY,4,d.DateName) AND PubHolFlag = 0 ) SundayDebitRunDate
			,[DateName]
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
		FROM @MasterDates d
		WHERE DayName = 'Wednesday'
			AND PubHolFlag = 0
		) dates
	WHERE SaturdayDebitRunDate IS NOT NULL
		AND FridayDebitRunDate IS NOT NULL
		AND ThursDayDebitRunDate IS NOT NULL

	RETURN
END
