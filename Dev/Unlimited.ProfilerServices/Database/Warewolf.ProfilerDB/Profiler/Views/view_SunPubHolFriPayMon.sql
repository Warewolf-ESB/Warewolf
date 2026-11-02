CREATE VIEW [Profiler].[view_SunPubHolFriPayMon]
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
		,day(SundayDebitRunDate)	AS	DebitRunID
		,datename(weekday,SundayDebitRunDate)  AS  DebitRunIDDayName
		,SundayDebitRunDate  AS  DebitRunIDDate
		,datename(month,SundayDebitRunDate )  AS  DebitRunIDMonthName
		,'Sun-PubHolFri-PayMon'		AS	LUCCollectionProfile
FROM	(
			SELECT	
					(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,-1,d.DateName)  ) SundayDebitRunDate
					,(SELECT DayOfMonth FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,-3,d.DateName) AND PubHolFlag = 1 ) FridayDebitRunID
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
			FROM	[Profiler].MasterDates d
			WHERE	DayName = 'Monday'
						AND PubHolFlag = 0
		) dates
WHERE	FridayDebitRunID IS NOT NULL