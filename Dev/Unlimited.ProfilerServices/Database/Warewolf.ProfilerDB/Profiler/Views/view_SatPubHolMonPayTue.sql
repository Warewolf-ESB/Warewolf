CREATE VIEW [Profiler].[view_SatPubHolMonPayTue]
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
			,'Sat-PubHolMon-PayTue'		AS	LUCCollectionProfile
	FROM	(
				SELECT	
						(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,-3,d.DateName) AND PubHolFlag = 0 ) SaturdayDebitRunDate
						,(SELECT DayOfMonth FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,-1,d.DateName) AND PubHolFlag = 1 ) MondayDebitRunID
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
						,[WeekEndDate]			FROM	[Profiler].MasterDates d
				WHERE	DayName = 'Tuesday'
							AND PubHolFlag = 0
			) dates
	WHERE	MondayDebitRunID IS NOT NULL
				AND SaturdayDebitRunDate IS NOT NULL