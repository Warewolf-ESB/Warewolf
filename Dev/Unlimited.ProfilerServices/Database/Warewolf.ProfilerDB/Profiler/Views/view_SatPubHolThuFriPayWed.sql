CREATE VIEW [Profiler].[view_SatPubHolThuFriPayWed]
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
		,day(SaturdayDebitRunDate)	AS	DebitRunID
		,datename(weekday,SaturdayDebitRunDate)  AS  DebitRunIDDayName
		,SaturdayDebitRunDate  AS  DebitRunIDDate
		,datename(month,SaturdayDebitRunDate )  AS  DebitRunIDMonthName
		,'Sat-PubHolThuFri-PayWed'		AS	LUCCollectionProfile
FROM (
		SELECT 
			(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,1,d.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,2,d.DateName) AND PubHolFlag = 1) FridayDebitRunDate
			,(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,3,d.DateName) AND PubHolFlag = 0 ) SaturdayDebitRunDate
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

