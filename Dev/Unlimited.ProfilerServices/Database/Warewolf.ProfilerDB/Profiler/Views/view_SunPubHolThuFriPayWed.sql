CREATE VIEW [Profiler].[view_SunPubHolThuFriPayWed]
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
		,'Sun-PubHolThuFri-PayWed'		AS	LUCCollectionProfile
FROM (
		SELECT 
			(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,1,d.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,2,d.DateName) AND PubHolFlag = 1) FridayDebitRunDate
			,(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,3,d.DateName) AND PubHolFlag = 0 ) SaturdayDebitRunDate
			,(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,4,d.DateName) AND PubHolFlag = 0 ) SundayDebitRunDate
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
							,[WeekEndDate] FROM [Profiler].MasterDates d
		WHERE DayName = 'Wednesday'
			AND PubHolFlag = 0
		) dates
	WHERE SaturdayDebitRunDate IS NOT NULL
		AND FridayDebitRunDate IS NOT NULL
		AND ThursDayDebitRunDate IS NOT NULL
