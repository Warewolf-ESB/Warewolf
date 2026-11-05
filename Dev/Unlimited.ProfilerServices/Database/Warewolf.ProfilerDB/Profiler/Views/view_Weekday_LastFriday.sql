CREATE VIEW [Profiler].[view_Weekday_LastFriday]
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
		,'32' DebitRunID
		,[DayName]  AS  DebitRunIDDayName
		,[DateName]  AS  DebitRunIDDate
		,[MonthName]  AS  DebitRunIDMonthName
		,'Weekday' LUCCollectionProfile
FROM	(
			SELECT	TOP 100 PERCENT
					ROW_NUMBER() OVER (PARTITION BY MonthName, YearNo ORDER BY MonthName , YearNo, DayOfMonth DESC) DayTypeID
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