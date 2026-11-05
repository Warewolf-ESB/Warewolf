CREATE VIEW [Profiler].[view_WeekDay_DisabilityGrant]
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
		,'39' as [DebitRunID]
		,dimDate.[DayName]  AS  DebitRunIDDayName
		,dimDate.[DateName]  AS  DebitRunIDDate
		,dimDate.[MonthName]  AS  DebitRunIDMonthName
		,'Weekday' LUCCollectionProfile
	FROM	[Profiler].[MasterDates] dimDate  
	INNER JOIN [Profiler].[GrantDates] gDates on gdates.[Date] = dimDate.[DateName] and gdates.[GrantTypeId] = 2	
	WHERE [WeekDayFlag] = 1
	AND [PubHolFlag] = 0