CREATE VIEW [Profiler].[view_SatPaySat]
	AS 
SELECT	[DateName]
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
		,[DayOfMonth]	AS	DebitRunID
		,[DayName]  AS  DebitRunIDDayName
		,[DateName]  AS  DebitRunIDDate
		,[MonthName]  AS  DebitRunIDMonthName
		,'Sat-PaySat'		AS	LUCCollectionProfile				
FROM	[Profiler].MasterDates d
WHERE	DayName = 'Saturday'
			AND PubHolFlag = 0

