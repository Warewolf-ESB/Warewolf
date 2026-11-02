CREATE VIEW [Profiler].[view_MonPubHolMonPayTue]
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
				,'Mon-PubHolMon-PayTue'		AS	LUCCollectionProfile
		FROM	(
					SELECT	
							(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,-1,d.DateName) AND PubHolFlag = 1 ) MondayDebitRunDate
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
		WHERE	MondayDebitRunDate IS NOT NULL	