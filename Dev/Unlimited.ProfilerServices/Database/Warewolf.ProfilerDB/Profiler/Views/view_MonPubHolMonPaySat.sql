CREATE VIEW [Profiler].[view_MonPubHolMonPaySat]
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
		,day(MondayDebitRunDate)	AS	DebitRunID
		,datename(weekday,MondayDebitRunDate)  AS  DebitRunIDDayName
		,MondayDebitRunDate  AS  DebitRunIDDate
		,datename(month,MondayDebitRunDate )  AS  DebitRunIDMonthName
		,'Mon-PubHolMon-PaySat'		AS	LUCCollectionProfile
FROM	(
			SELECT	
					(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,2,d.DateName) AND PubHolFlag = 1 ) MondayDebitRunDate
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
			WHERE	DayName = 'Saturday'
						AND PubHolFlag = 0
		) dates
WHERE	MondayDebitRunDate IS NOT NULL