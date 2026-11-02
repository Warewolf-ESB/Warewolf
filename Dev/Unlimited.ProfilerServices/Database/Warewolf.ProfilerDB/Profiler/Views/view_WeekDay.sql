CREATE VIEW [Profiler].[view_WeekDay]
	AS 
	SELECT	
		dimDate.[DateName]
		,dimDate.[DayName]
		,dimDate.[MonthName]
		,dimDate.[DayOfWeek]
		,dimDate.[DayOfMonth]
		,dimDate.[MonthOfYear]
		,dimDate.[YearNo]	
		,dimDate.[MonthStartDate]
		,dimDate.[MonthEndDate]
		,dimDate.[PubHolFlag]
		,dimDate.[WeekDayFlag]
		,dimDate.[WeekStartDate]
		,dimDate.[WeekEndDate]
		,dimDate.[DayOfMonth] [DebitRunID]
		,dimDate.[DayName]  AS  DebitRunIDDayName
		,dimDate.[DateName]  AS  DebitRunIDDate
		,dimDate.[MonthName]  AS  DebitRunIDMonthName
		,'Weekday' LUCCollectionProfile
	FROM	[Profiler].MasterDates dimDate 
	WHERE	WeekDayFlag = 1
				AND PubHolFlag = 0