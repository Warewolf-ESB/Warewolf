CREATE VIEW [Profiler].[view_MonPubHolMonFriPayThu]
AS
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
	,day(MondayDebitRunDate) AS DebitRunID
	,datename(weekday, MondayDebitRunDate) AS DebitRunIDDayName
	,MondayDebitRunDate AS DebitRunIDDate
	,datename(month, MondayDebitRunDate) AS DebitRunIDMonthName
	,'Mon-PubHolMonFri-PayThu' AS LUCCollectionProfile
FROM (
	SELECT (
			SELECT [DateName] y
			FROM [Profiler].MasterDates
			WHERE DateName = DATEADD(DAY, 1, d.DateName)
				AND PubHolFlag = 1
			) FridayDebitRunDate
		,(
			SELECT [DateName]
			FROM [Profiler].MasterDates
			WHERE DateName = DATEADD(DAY, 4, d.DateName)
				AND PubHolFlag = 1
			) MondayDebitRunDate
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
	FROM [Profiler].MasterDates d
	WHERE DayName = 'Thursday'
		AND PubHolFlag = 0
	) dates
WHERE MondayDebitRunDate IS NOT NULL
	AND FridayDebitRunDate IS NOT NULL
