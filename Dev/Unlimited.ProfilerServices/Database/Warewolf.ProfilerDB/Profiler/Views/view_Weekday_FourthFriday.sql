CREATE VIEW [Profiler].[view_Weekday_FourthFriday]
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
		,'34' AS DebitRunID
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
WHERE	DayTypeID = 4		
			AND PubHolFlag = 0