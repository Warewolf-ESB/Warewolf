CREATE VIEW [Profiler].[MasterDates]
	AS 
		SELECT 
		[DATE] as [DateName]
		,WEEKDAY_NAME_LONG as [DayName]
		,MONTH_NAME_LONG as [MonthName]
		,DAY_OF_WEEK as [DayOfWeek]
		,DAY_OF_MONTH as [DayOfMonth]
		,[MONTH] as [MonthOfYear]
		,[YEAR] as [YearNo]
		,START_OF_MONTH_DATE as [MonthStartDate]
		,END_OF_MONTH_DATE as [MonthEndDate]
		,case when (select top 1 PublicHolidayDate from [Profiler].[func_PublicHolidaysByYear](DateMaster.[YEAR])  where PublicHolidayDate = [DATE] order by PublicHolidayDate desc) is not null then 1 else 0 end as [PubHolFlag]
		,case when DAY_OF_WEEK between 2 and 6 then 1 else 0 end as [WeekDayFlag]
		,START_OF_WEEK_STARTING_MON_DATE as [WeekStartDate]
		,END_OF_WEEK_STARTING_MON_DATE as [WeekEndDate]
	FROM	[Profiler].[func_dates]('2001-12-31','2030-12-31') DateMaster