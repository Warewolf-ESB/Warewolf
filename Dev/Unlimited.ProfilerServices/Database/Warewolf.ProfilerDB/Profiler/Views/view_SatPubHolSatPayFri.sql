CREATE VIEW [Profiler].[view_SatPubHolSatPayFri]
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
		,'Sat-PubHolSat-PayFri'		AS	LUCCollectionProfile
FROM	(
			SELECT	
					(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,1,d.DateName) AND PubHolFlag = 1 ) SaturdayDebitRunDate
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
			WHERE	DayName = 'Friday'
						AND PubHolFlag = 0
		) dates
WHERE	SaturdayDebitRunDate IS NOT NULL	