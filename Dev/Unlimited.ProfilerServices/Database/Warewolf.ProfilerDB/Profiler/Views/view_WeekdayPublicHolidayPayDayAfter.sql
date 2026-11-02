CREATE VIEW [Profiler].[view_WeekdayPublicHolidayPayDayAfter]
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
		,day(PublichHolidayDRI)	AS	DebitRunID
		,datename(weekday,PublichHolidayDRI)  AS  DebitRunIDDayName
		,PublichHolidayDRI  AS  DebitRunIDDate
		,datename(month,PublichHolidayDRI )  AS  DebitRunIDMonthName
		,'Weekday-PublicHoliday-PayDayAfter'		AS	LUCCollectionProfile
FROM	(
			SELECT	
					(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,-1,d.DateName) AND PubHolFlag = 1 ) PublichHolidayDRI
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
			WHERE	d.[WeekDayFlag] = 1
			and d.DayName in ('Wednesday','Thursday','Friday')
						AND PubHolFlag = 0
		) dates
WHERE	PublichHolidayDRI IS NOT NULL