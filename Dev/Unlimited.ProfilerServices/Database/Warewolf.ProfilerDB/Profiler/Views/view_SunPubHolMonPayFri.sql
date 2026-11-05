CREATE VIEW [Profiler].[view_SunPubHolMonPayFri]
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
		,'Sun-PubHolMon-PayFri'		AS	LUCCollectionProfile
FROM	(
			SELECT	
					(SELECT [DateName]y FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,2,d.DateName)) SundayDebitRunDate
					,(SELECT DayOfMonth FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,3,d.DateName) AND PubHolFlag = 1 ) MondayDebitRunID
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
WHERE	MondayDebitRunID IS NOT NULL