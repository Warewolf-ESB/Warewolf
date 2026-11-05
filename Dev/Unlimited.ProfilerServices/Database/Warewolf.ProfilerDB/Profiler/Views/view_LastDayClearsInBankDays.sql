CREATE VIEW [Profiler].[view_LastDayClearsInBankDays]
AS
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
	,LastDayPreviousMonth.[DayName] AS DebitRunIDDayName
	,LastDayPreviousMonth.[DateName] AS DebitRunIDDate
	,LastDayPreviousMonth.[MonthName] AS DebitRunIDMonthName
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
	FROM [Profiler].MasterDates dimDate
	WHERE (
			WeekDayFlag = 1
			OR [DayName] = 'Saturday'
			)
		AND PubHolFlag = 0
	) AS RankedWorkingDays
INNER JOIN [Profiler].MasterDates LastDayPreviousMonth ON LastDayPreviousMonth.DateName = dateadd(day, - 1, RankedWorkingDays.MonthStartDate)
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
