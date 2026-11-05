CREATE FUNCTION [Profiler].[func_Profiles_LastDayClearsInBankDays] (
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
	SELECT RankedWorkingDays.[DateName]
		,RankedWorkingDays.[DayName]
		,RankedWorkingDays.[MonthName]
		,RankedWorkingDays.[DayOfWeek]
		,RankedWorkingDays.[DayOfMonth]
		,RankedWorkingDays.[MonthOfYear]
		,RankedWorkingDays.[YearNo]
		,RankedWorkingDays.[MonthStartDate]
		,RankedWorkingDays.[MonthEndDate]
		,RankedWorkingDays.[PubHolFlag]
		,RankedWorkingDays.[WeekDayFlag]
		,RankedWorkingDays.[WeekStartDate]
		,RankedWorkingDays.[WeekEndDate]
		,31 AS [DebitRunID]
		,DATENAME(WEEKDAY, dateadd(day, - 1, RankedWorkingDays.MonthStartDate)) AS DebitRunIDDayName
		,DATEADD(day, - 1, RankedWorkingDays.MonthStartDate) AS DebitRunIDDate
		,DATENAME(MONTH, dateadd(day, - 1, RankedWorkingDays.MonthEndDate)) AS DebitRunIDMonthName
		,ClearingDayProfiles AS LUCCollectionProfile
	FROM (
		SELECT dimDate.[DateName]
			,dimDate.[DayName]
			,dimDate.[MonthName]
			,dimDate.[DayOfWeek]
			,dimDate.[DayOfMonth]
			,dimDate.[MonthOfYear]
			,dimDate.[YearNo]
			,dimDate.[MonthStartDate]
			,dimDate.[MonthEndDate]
			,dimDate.[PubHolFlag]
			,dimDate.[WeekDayFlag]
			,dimDate.[WeekStartDate]
			,dimDate.[WeekEndDate]
			,ROW_NUMBER() OVER (
				PARTITION BY [MonthOfYear]
				,[YearNo] ORDER BY [DateName] ASC
				) Ranking
		FROM @MasterDates dimDate
		WHERE (
				WeekDayFlag = 1
				OR [DayName] = 'Saturday'
				)
			AND PubHolFlag = 0
		) AS RankedWorkingDays
	CROSS JOIN (
		VALUES ('LastDay-ClearsInOneBankDay')
			,('LastDay-ClearsInTwoBankDays')
			,('LastDay-ClearsInThreeBankDays')
			,('LastDay-ClearsInFourBankDays')
			,('LastDay-ClearsInFiveBankDays')
			,('LastDay-ClearsInSixBankDays')
			,('LastDay-ClearsInSevenBankDays')
		) AS LastClearingDayProfiles(ClearingDayProfiles)
	WHERE (
			LastClearingDayProfiles.ClearingDayProfiles = 'LastDay-ClearsInOneBankDay'
			AND Ranking = 1
			)
		OR (
			LastClearingDayProfiles.ClearingDayProfiles = 'LastDay-ClearsInTwoBankDays'
			AND Ranking = 2
			)
		OR (
			LastClearingDayProfiles.ClearingDayProfiles = 'LastDay-ClearsInThreeBankDays'
			AND Ranking = 3
			)
		OR (
			LastClearingDayProfiles.ClearingDayProfiles = 'LastDay-ClearsInFourBankDays'
			AND Ranking = 4
			)
		OR (
			LastClearingDayProfiles.ClearingDayProfiles = 'LastDay-ClearsInFiveBankDays'
			AND Ranking = 5
			)
		OR (
			LastClearingDayProfiles.ClearingDayProfiles = 'LastDay-ClearsInSixBankDays'
			AND Ranking = 6
			)
		OR (
			LastClearingDayProfiles.ClearingDayProfiles = 'LastDay-ClearsInSevenBankDays'
			AND Ranking = 7
			)

	RETURN
END
