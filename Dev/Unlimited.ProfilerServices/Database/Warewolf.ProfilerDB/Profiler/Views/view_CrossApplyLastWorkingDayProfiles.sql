CREATE VIEW [Profiler].[view_CrossApplyLastWorkingDayProfiles]
	AS 
		select 
		[DateName]
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
		,LastWorkingDayDRIs.DebitRunId as [DebitRunID]
		,[DebitRunIDDayName]
		,[DebitRunIDDate]
		,[DebitRunIDMonthName]
		,[LUCCollectionProfile]	
	from
		(
		select 
		*
		,ROW_NUMBER() over (partition by [MonthOfYear],[YearNo] order by [DateName] desc)  Ranking
		from [Profiler].view_WeekDay
		) 
	as ProfileDates
	CROSS JOIN (VALUES (42),(43),(44),(45)) AS LastWorkingDayDRIs (DebitRunId)
	Where  (LastWorkingDayDRIs.DebitRunId = 42 and Ranking = 2)
	or (LastWorkingDayDRIs.DebitRunId = 43 and Ranking = 3)
	or (LastWorkingDayDRIs.DebitRunId = 44 and Ranking = 4)
	or (LastWorkingDayDRIs.DebitRunId = 45 and Ranking = 5)