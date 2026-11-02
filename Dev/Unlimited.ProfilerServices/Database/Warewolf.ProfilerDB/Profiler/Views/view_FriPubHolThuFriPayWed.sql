CREATE VIEW [Profiler].[view_FriPubHolThuFriPayWed]
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
		,day(FridayDebitRunDate)	AS	DebitRunID
		,datename(weekday,FridayDebitRunDate)  AS  DebitRunIDDayName
		,FridayDebitRunDate  AS  DebitRunIDDate
		,datename(month,FridayDebitRunDate )  AS  DebitRunIDMonthName
		,'Fri-PubHolThuFri-PayWed'		AS	LUCCollectionProfile
FROM (
		SELECT 
			(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,1,d.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
			,(SELECT [DateName] FROM [Profiler].MasterDates WHERE DateName = DATEADD(DAY,2,d.DateName) AND PubHolFlag = 1) FridayDebitRunDate
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
							,[WeekEndDate] FROM [Profiler].MasterDates d
		WHERE DayName = 'Wednesday'
			AND PubHolFlag = 0
		) dates
	WHERE 
		FridayDebitRunDate IS NOT NULL
		AND ThursDayDebitRunDate IS NOT NULL
		UNION ALL
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
	,'37' DebitRunID --First Friday
	,datename(weekday,FridayDebitRunDate)  AS  DebitRunIDDayName
	,FridayDebitRunDate  AS  DebitRunIDDate
	,datename(month,FridayDebitRunDate )  AS  DebitRunIDMonthName
	,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
FROM	(
		SELECT	TOP 100 PERCENT
				ROW_NUMBER() OVER (PARTITION BY Friday.MonthName, Friday.YearNo ORDER BY Friday.MonthName,Friday.YearNo,Friday.DayOfMonth) DayTypeID
				,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
				,WednesDay.[DateName]
				,WednesDay.[DayName]
				,WednesDay.[MonthName]
				,WednesDay.[DayOfWeek]
				,WednesDay.[DayOfMonth]
				,WednesDay.[MonthOfYear]
				,WednesDay.[YearNo]	
				,WednesDay.[MonthStartDate]
				,WednesDay.[MonthEndDate]
				,WednesDay.[PubHolFlag]
				,WednesDay.[WeekDayFlag]
				,WednesDay.[WeekStartDate]
				,WednesDay.[WeekEndDate]
				,Friday.DateName FridayDebitRunDate
		FROM	[Profiler].MasterDates Friday
		left outer join [Profiler].MasterDates WednesDay on Friday.DateName = dateadd(day,2,WednesDay.DateName)	AND	Friday.PubHolFlag = 1
		WHERE	Friday.DayName = 'Friday'
		
	) Dates
WHERE	DayTypeID = 1 
AND		PubHolFlag = 0
AND ThursDayDebitRunDate IS NOT NULL
UNION ALL
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
	,'35' DebitRunID --Second Friday
	,datename(weekday,FridayDebitRunDate)  AS  DebitRunIDDayName
	,FridayDebitRunDate  AS  DebitRunIDDate
	,datename(month,FridayDebitRunDate )  AS  DebitRunIDMonthName
	,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
FROM	(
		SELECT	TOP 100 PERCENT
				ROW_NUMBER() OVER (PARTITION BY Friday.MonthName, Friday.YearNo ORDER BY Friday.MonthName , Friday.YearNo , Friday.DayOfMonth) DayTypeID
				,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
				,WednesDay.[DateName]
				,WednesDay.[DayName]
				,WednesDay.[MonthName]
				,WednesDay.[DayOfWeek]
				,WednesDay.[DayOfMonth]
				,WednesDay.[MonthOfYear]
				,WednesDay.[YearNo]	
				,WednesDay.[MonthStartDate]
				,WednesDay.[MonthEndDate]
				,WednesDay.[PubHolFlag]
				,WednesDay.[WeekDayFlag]
				,WednesDay.[WeekStartDate]
				,WednesDay.[WeekEndDate]
				,Friday.DateName FridayDebitRunDate
		FROM	[Profiler].MasterDates Friday
		left outer join [Profiler].MasterDates WednesDay on Friday.DateName = dateadd(day,2,WednesDay.DateName)	AND	Friday.PubHolFlag = 1 
		WHERE	Friday.DayName = 'Friday'	
	) Dates
WHERE	DayTypeID = 2	
AND		PubHolFlag = 0
AND ThursDayDebitRunDate IS NOT NULL
UNION ALL
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
	,'33' DebitRunID --THIRD FRIDAY
	,datename(weekday,FridayDebitRunDate)  AS  DebitRunIDDayName
	,FridayDebitRunDate  AS  DebitRunIDDate
	,datename(month,FridayDebitRunDate )  AS  DebitRunIDMonthName
	,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
FROM	(
		SELECT	TOP 100 PERCENT
				ROW_NUMBER() OVER (PARTITION BY Friday.MonthName, Friday.YearNo ORDER BY Friday.MonthName , Friday.YearNo , Friday.DayOfMonth) DayTypeID
				,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
				,WednesDay.[DateName]
				,WednesDay.[DayName]
				,WednesDay.[MonthName]
				,WednesDay.[DayOfWeek]
				,WednesDay.[DayOfMonth]
				,WednesDay.[MonthOfYear]
				,WednesDay.[YearNo]	
				,WednesDay.[MonthStartDate]
				,WednesDay.[MonthEndDate]
				,WednesDay.[PubHolFlag]
				,WednesDay.[WeekDayFlag]
				,WednesDay.[WeekStartDate]
				,WednesDay.[WeekEndDate]
				,Friday.DateName FridayDebitRunDate
		FROM	[Profiler].MasterDates Friday
		left outer join [Profiler].MasterDates WednesDay on Friday.DateName = dateadd(day,2,WednesDay.DateName)	AND	Friday.PubHolFlag = 1 
		WHERE	Friday.DayName = 'Friday'	
		
	) Dates
WHERE	DayTypeID = 3	
AND		PubHolFlag = 0
AND ThursDayDebitRunDate IS NOT NULL
UNION ALL
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
	,'34' AS DebitRunID --FOURTH FRIDAY
	,datename(weekday,FridayDebitRunDate)  AS  DebitRunIDDayName
	,FridayDebitRunDate  AS  DebitRunIDDate
	,datename(month,FridayDebitRunDate )  AS  DebitRunIDMonthName
	,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
FROM	(
		SELECT	TOP 100 PERCENT
				ROW_NUMBER() OVER (PARTITION BY Friday.MonthName, Friday.YearNo ORDER BY Friday.MonthName , Friday.YearNo , Friday.DayOfMonth) DayTypeID
				,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
				,WednesDay.[DateName]
				,WednesDay.[DayName]
				,WednesDay.[MonthName]
				,WednesDay.[DayOfWeek]
				,WednesDay.[DayOfMonth]
				,WednesDay.[MonthOfYear]
				,WednesDay.[YearNo]	
				,WednesDay.[MonthStartDate]
				,WednesDay.[MonthEndDate]
				,WednesDay.[PubHolFlag]
				,WednesDay.[WeekDayFlag]
				,WednesDay.[WeekStartDate]
				,WednesDay.[WeekEndDate]
				,Friday.DateName FridayDebitRunDate
		FROM	[Profiler].MasterDates Friday
		left outer join [Profiler].MasterDates WednesDay on Friday.DateName = dateadd(day,2,WednesDay.DateName)	AND	Friday.PubHolFlag = 1 
		WHERE	Friday.DayName = 'Friday'	
		
	) Dates
WHERE	DayTypeID = 4	
AND		PubHolFlag = 0
AND ThursDayDebitRunDate IS NOT NULL
UNION ALL
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
	,'32' DebitRunID --LAST FRIDAY
	,datename(weekday,FridayDebitRunDate)  AS  DebitRunIDDayName
	,FridayDebitRunDate  AS  DebitRunIDDate
	,datename(month,FridayDebitRunDate )  AS  DebitRunIDMonthName
	,'Fri-PubHolThuFri-PayWed' LUCCollectionProfile
FROM	(
		SELECT	TOP 100 PERCENT
				ROW_NUMBER() OVER (PARTITION BY Friday.MonthName, Friday.YearNo ORDER BY Friday.MonthName , Friday.YearNo , Friday.DayOfMonth desc) DayTypeID
				,(SELECT [DateName] FROM [Profiler].MasterDates Where DateName = DATEADD(DAY,- 1,Friday.DateName) AND PubHolFlag = 1) ThursDayDebitRunDate
				,WednesDay.[DateName]
				,WednesDay.[DayName]
				,WednesDay.[MonthName]
				,WednesDay.[DayOfWeek]
				,WednesDay.[DayOfMonth]
				,WednesDay.[MonthOfYear]
				,WednesDay.[YearNo]	
				,WednesDay.[MonthStartDate]
				,WednesDay.[MonthEndDate]
				,WednesDay.[PubHolFlag]
				,WednesDay.[WeekDayFlag]
				,WednesDay.[WeekStartDate]
				,WednesDay.[WeekEndDate]
				,Friday.DateName FridayDebitRunDate
		FROM	[Profiler].MasterDates Friday
		left outer join [Profiler].MasterDates WednesDay on Friday.DateName = dateadd(day,2,WednesDay.DateName)	AND	Friday.PubHolFlag = 1 
		WHERE	Friday.DayName = 'Friday'
	) Dates
WHERE	DayTypeID = 1
AND		PubHolFlag = 0
AND ThursDayDebitRunDate IS NOT NULL