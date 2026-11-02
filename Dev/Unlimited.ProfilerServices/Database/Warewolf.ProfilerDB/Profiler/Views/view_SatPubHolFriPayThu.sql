CREATE VIEW [Profiler].[view_SatPubHolFriPayThu]
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
			,'Sat-PubHolFri-PayThu'		AS	LUCCollectionProfile
	FROM	(
				SELECT	
						(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,2,d.DateName) AND PubHolFlag = 0 ) SaturdayDebitRunDate
						,(SELECT [DayOfMonth] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,1,d.DateName) AND PubHolFlag = 1 ) FridayDebitRunID
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
				WHERE	[DayName] = 'Thursday'
							AND PubHolFlag = 0
			) dates
	WHERE	FridayDebitRunID IS NOT NULL	
				AND SaturdayDebitRunDate IS NOT NULL