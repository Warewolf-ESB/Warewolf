CREATE VIEW [Profiler].[view_Weekday_FirstFriday]
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
		,'37' DebitRunID
		,[DayName]  AS  DebitRunIDDayName
		,[DateName]  AS  DebitRunIDDate
		,[MonthName]  AS  DebitRunIDMonthName
		,'Weekday' LUCCollectionProfile
FROM	(
			SELECT	TOP 100 PERCENT
					ROW_NUMBER() OVER (PARTITION BY MonthName, YearNo ORDER BY MonthName , YearNo , DayOfMonth) DayTypeID
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
			FROM	[Profiler].MasterDates
			WHERE	DayName = 'Friday'
			
		) Dates
WHERE	DayTypeID = 1
			AND PubHolFlag = 0