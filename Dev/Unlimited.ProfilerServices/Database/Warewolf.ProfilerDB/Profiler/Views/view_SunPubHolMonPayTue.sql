CREATE VIEW [Profiler].[view_SunPubHolMonPayTue]
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
				,'Sun-PubHolMon-PayTue'		AS	LUCCollectionProfile
		FROM	(
					SELECT	(SELECT [DateName]y FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,-2,d.DateName)) SundayDebitRunDate
							,(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,-1,d.DateName) AND PubHolFlag = 1 ) MondayDebitRunDate
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
					WHERE	DayName = 'Tuesday'
								AND PubHolFlag = 0
				) dates
		WHERE	MondayDebitRunDate IS NOT NULL